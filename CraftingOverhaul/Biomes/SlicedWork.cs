using System;
using System.Collections;
using System.Diagnostics;

namespace CraftingOverhaul.Biomes
{
    /// <summary>Main-thread time a piece of sliced work has used, for the log.</summary>
    internal sealed class SliceStats
    {
        public double BusyMs;
        public double LongestStepMs;
        public double LongestSliceMs;
        public int Slices;
    }

    /// <summary>
    ///     Spreads long work over many frames. The work is an enumerator whose every step is a small piece of work
    ///     (one prefab walked, one recipe read...). Each frame runs steps until a time budget is spent; a step that
    ///     yields <see cref="WaitFrame" /> (e.g. waiting for an asset to load in the background) ends the frame's
    ///     slice at once. Free of game dependencies, so it is tested on its own.
    /// </summary>
    internal static class SlicedWork
    {
        /// <summary>Yielded by a step that is waiting on something else: nothing more to do this frame.</summary>
        public static readonly object WaitFrame = new object();

        private static readonly Func<double> StopwatchMs = () => Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency;

        /// <summary>
        ///     Runs steps until the budget is spent, a step yields <see cref="WaitFrame" />, or the work ends.
        ///     Returns true while there is more to do. A single step that runs over the budget still finishes.
        /// </summary>
        public static bool Pump(IEnumerator work, double budgetMs, SliceStats stats, Func<double> clockMs = null)
        {
            var now = clockMs ?? StopwatchMs;
            double start = now();
            while (true)
            {
                double stepStart = now();
                bool more = work.MoveNext();
                double end = now();
                stats.LongestStepMs = Math.Max(stats.LongestStepMs, end - stepStart);

                bool stop = !more || ReferenceEquals(work.Current, WaitFrame) || end - start >= budgetMs;
                if (stop)
                {
                    stats.BusyMs += end - start;
                    stats.LongestSliceMs = Math.Max(stats.LongestSliceMs, end - start);
                    stats.Slices++;
                    return more;
                }
            }
        }
    }
}
