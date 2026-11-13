using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using CraftingOverhaul.Biomes.Collectors;

namespace CraftingOverhaul.Biomes
{
    /// <summary>
    ///     One run of the collectors and the solver, as a sequence of small steps (see <see cref="SlicedWork" />).
    ///     Normally <see cref="BiomeResolver" /> runs a few milliseconds of it per frame and the solver itself on a
    ///     background thread; set <see cref="CollectRun.Synchronous" /> (even half-way) to finish it right away.
    /// </summary>
    internal sealed class BiomeResolveJob
    {
        public readonly CollectRun Run = new CollectRun();
        public readonly List<ItemSource> Sources = new List<ItemSource>();
        public readonly SliceStats Stats = new SliceStats();
        public readonly Stopwatch Wall = Stopwatch.StartNew();

        /// <summary>The cache key worked out when the job started, saved with the result.</summary>
        public readonly IList<KeyValuePair<string, string>> CacheKey;

        public BiomeSolver Result { get; private set; }
        public bool IsDone { get; private set; }

        /// <summary>Per collector: sources found and main-thread time used, for the log.</summary>
        public readonly StringBuilder Report = new StringBuilder();

        private readonly ISourceCollector[] _collectors;
        private readonly string _overrides;
        private readonly Func<List<Tool>> _findTools;
        private IEnumerator _work;

        public BiomeResolveJob(ISourceCollector[] collectors, string overrides, Func<List<Tool>> findTools,
            IList<KeyValuePair<string, string>> cacheKey)
        {
            _collectors = collectors;
            _overrides = overrides;
            _findTools = findTools;
            CacheKey = cacheKey;
            _work = Steps();
        }

        /// <summary>Runs up to <paramref name="budgetMs" /> of work. Returns true while there is more to do.</summary>
        public bool Pump(double budgetMs)
        {
            if (IsDone)
            {
                return false;
            }
            bool more = SlicedWork.Pump(_work, budgetMs, Stats);
            if (!more)
            {
                Finish();
            }
            return more;
        }

        /// <summary>Finishes the rest now, on this frame.</summary>
        public void RunToEnd()
        {
            Run.Synchronous = true;
            while (Pump(double.MaxValue))
            {
            }
        }

        /// <summary>Stops the job and releases any asset it holds.</summary>
        public void Cancel()
        {
            Finish();
        }

        private void Finish()
        {
            IsDone = true;
            (_work as IDisposable)?.Dispose();
            _work = null;
        }

        private IEnumerator Steps()
        {
            foreach (var collector in _collectors)
            {
                int before = Sources.Count;
                double busy = 0;
                IEnumerator steps = null;
                try
                {
                    while (true)
                    {
                        long start = Stopwatch.GetTimestamp();
                        bool more = Step(collector, ref steps);
                        busy += (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
                        if (!more)
                        {
                            break;
                        }
                        yield return steps.Current;
                    }
                }
                finally
                {
                    (steps as IDisposable)?.Dispose();
                }
                Report.Append($" {collector.Name} {Sources.Count - before} ({busy:0} ms);");
                yield return null;
            }

            // The solver needs no game objects, so it runs on a background thread; tools and overrides are read here.
            var solver = new BiomeSolver(Sources, BiomeOverrides.Parse(_overrides), _findTools());
            yield return null;
            if (Run.Synchronous)
            {
                if (!Solve(solver))
                {
                    solver = null;
                }
            }
            else
            {
                var task = Task.Run(() => solver.Solve());
                while (!task.IsCompleted)
                {
                    if (Run.Synchronous)
                    {
                        Wait(task);
                        break;
                    }
                    yield return SlicedWork.WaitFrame;
                }
                if (task.IsFaulted)
                {
                    Jotunn.Logger.LogWarning($"Biome solver failed: {task.Exception?.GetBaseException()}");
                    solver = null;
                }
            }
            Result = solver;
        }

        /// <summary>One collector step, starting the collector on first use. A failing collector is skipped.</summary>
        private bool Step(ISourceCollector collector, ref IEnumerator steps)
        {
            try
            {
                if (steps == null)
                {
                    steps = collector.Collect(Sources, Run).GetEnumerator();
                }
                return steps.MoveNext();
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Biome collector {collector.Name} failed: {ex}");
                return false;
            }
        }

        private static bool Solve(BiomeSolver solver)
        {
            try
            {
                solver.Solve();
                return true;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Biome solver failed: {ex}");
                return false;
            }
        }

        private static void Wait(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException)
            {
                // Reported by the caller through task.IsFaulted.
            }
        }
    }
}
