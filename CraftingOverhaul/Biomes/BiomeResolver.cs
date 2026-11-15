using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CraftingOverhaul.Biomes.Collectors;
using UnityEngine;

namespace CraftingOverhaul.Biomes
{
    /// <summary>
    ///     Works out the earliest biome each item can be obtained in, from the game's own data:
    ///     <list type="number">
    ///         <item>Collectors turn world data (vegetation, spawns, locations, stations, recipes...) into sources.</item>
    ///         <item>
    ///             A source's biome is the latest of its own biome, its required items, the cheapest tool that can
    ///             harvest it and the boss that sets its required global key.
    ///         </item>
    ///         <item>An item's biome is its earliest source. This is repeated until nothing changes (a fixed point).</item>
    ///     </list>
    ///     Overrides from the config are fixed values that the rest of the chain builds on.
    ///     The rules themselves live in <see cref="BiomeSolver" />; this class gathers the data and keeps the result.
    ///     <para>
    ///         Collecting loads every location and dungeon room, which takes a while. So after spawning it runs in the
    ///         background (<see cref="StartResolving" />): a few milliseconds per frame, assets loaded asynchronously,
    ///         the solver on another thread. The result is saved to <see cref="CachePath" /> and reused on later loads
    ///         while the game, the mods and the game data are unchanged (<see cref="BiomeCacheKey" />). Until it is
    ///         ready, <see cref="IsResolved" /> is false and the recipe list keeps the game's order.
    ///     </para>
    /// </summary>
    internal static class BiomeResolver
    {
        /// <summary>Main-thread time per frame for collecting: a quarter of a 60 fps frame.</summary>
        private const double FrameBudgetMs = 4;

        private static readonly ISourceCollector[] Collectors =
        {
            new VegetationCollector(),
            new SpawnCollector(),
            new LocationCollector(),
            new ConversionCollector(),
            new ProducerPieceCollector(),
            new HatchCollector(),
            new RecipeCollector()
        };

        /// <summary>Runs the background work's coroutine (the plugin).</summary>
        internal static MonoBehaviour Host;

        private static Dictionary<string, BiomeResolution> _items;
        private static Dictionary<string, BiomeResolution> _keys;
        private static List<ItemSource> _sources;
        private static BiomeResolveJob _job;

        public static bool IsResolved => _items != null;

        private static string CachePath => Path.Combine(BepInEx.Paths.CachePath, "CraftingOverhaul.biomes.txt");

        /// <summary>Every collected source, for the dump command. Collects again if the result came from the saved file.</summary>
        public static IReadOnlyList<ItemSource> Sources
        {
            get
            {
                EnsureSources();
                return _sources;
            }
        }

        /// <summary>Collects every source now if they are not in memory (the result came from the saved file).</summary>
        public static void EnsureSources()
        {
            if (_sources == null)
            {
                Invalidate();
                EnsureResolved(false);
            }
        }

        /// <summary>Forgets the result and stops any work in progress (new world, changed overrides).</summary>
        public static void Invalidate()
        {
            _job?.Cancel();
            _job = null;
            _items = null;
            _keys = null;
            _sources = null;
        }

        /// <summary>False while the biomes are not worked out yet; it never waits for them.</summary>
        public static bool TryGet(string itemPrefab, out BiomeResolution resolution)
        {
            resolution = null;
            return _items != null && _items.TryGetValue(itemPrefab, out resolution);
        }

        public static bool TryGetKey(string globalKey, out BiomeResolution resolution)
        {
            resolution = null;
            return _keys != null && _keys.TryGetValue(globalKey, out resolution);
        }

        /// <summary>Every resolved item; works the biomes out now if needed (console commands only).</summary>
        public static IEnumerable<KeyValuePair<string, BiomeResolution>> AllItems()
        {
            EnsureResolved();
            return _items;
        }

        /// <summary>
        ///     Starts working the biomes out in the background, unless they are known or already being worked out.
        ///     Uses the saved file when it still matches, which makes the result available at once. Needs a loaded world.
        /// </summary>
        public static void StartResolving()
        {
            if (_items != null || _job != null || !WorldLoaded())
            {
                return;
            }
            var key = BiomeCacheKey.Build();
            if (TryLoadCache(key))
            {
                return;
            }
            _job = NewJob(key);
            if (Host)
            {
                Host.StartCoroutine(Drive(_job));
            }
            else
            {
                Complete(_job);
            }
        }

        /// <summary>
        ///     Works the biomes out now, finishing any background work on this frame (console commands).
        ///     <paramref name="useCache" /> false ignores the saved file (co_biome refresh, the dump).
        /// </summary>
        public static void EnsureResolved(bool useCache = true)
        {
            if (_items != null)
            {
                return;
            }
            if (_job == null)
            {
                var key = BiomeCacheKey.Build();
                if (useCache && TryLoadCache(key))
                {
                    return;
                }
                _job = NewJob(key);
            }
            var job = _job;
            job.RunToEnd();
            Complete(job);
        }

        private static bool WorldLoaded()
        {
            return ObjectDB.instance && ZNetScene.instance && ZoneSystem.instance;
        }

        private static BiomeResolveJob NewJob(IList<KeyValuePair<string, string>> key)
        {
            return new BiomeResolveJob(Collectors, ModConfig.BiomeMaterialOverrides.Value, FindTools, key);
        }

        private static IEnumerator Drive(BiomeResolveJob job)
        {
            while (_job == job)
            {
                if (!WorldLoaded())
                {
                    // Left the world: drop the work, it starts again in the next one.
                    Invalidate();
                    yield break;
                }
                bool more;
                try
                {
                    more = job.Pump(FrameBudgetMs);
                }
                catch (Exception ex)
                {
                    Jotunn.Logger.LogWarning($"Working out item biomes failed: {ex}");
                    more = false;
                }
                if (!more)
                {
                    Complete(job);
                    RefreshRecipeList();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>Takes a finished job's result (once), logs how long it took and saves it.</summary>
        private static void Complete(BiomeResolveJob job)
        {
            if (_job != job)
            {
                return;
            }
            if (!job.IsDone)
            {
                job.RunToEnd();
            }
            _job = null;
            _sources = job.Sources;
            var solver = job.Result;
            _items = solver?.Items ?? new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
            _keys = solver?.Keys ?? new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
            RecipeList.RecipeCategories.ClearCache();

            var s = job.Stats;
            Jotunn.Logger.LogInfo(
                $"Resolved biomes for {_items.Count} items from {_sources.Count} sources in {job.Wall.Elapsed.TotalSeconds:0.0} s " +
                $"({s.BusyMs:0} ms of game time over {s.Slices} frames; longest frame {s.LongestSliceMs:0} ms, " +
                $"longest step {s.LongestStepMs:0} ms).{job.Report}");

            if (solver == null || !solver.Settled)
            {
                Jotunn.Logger.LogWarning("Biome resolution did not settle; results may be incomplete and are not saved.");
                return;
            }
            SaveCache(job.CacheKey);
        }

        private static bool TryLoadCache(IList<KeyValuePair<string, string>> key)
        {
            try
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                string text = File.Exists(CachePath) ? File.ReadAllText(CachePath) : null;
                if (!BiomeCacheFile.TryRead(text, key, out var items, out var keys, out string reason))
                {
                    Jotunn.Logger.LogInfo($"Working out item biomes in the background (saved biomes not used: {reason}).");
                    return false;
                }
                _items = items;
                _keys = keys;
                _sources = null;
                RecipeList.RecipeCategories.ClearCache();
                Jotunn.Logger.LogInfo($"Loaded biomes for {items.Count} items from the saved file in {timer.ElapsedMilliseconds} ms.");
                return true;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Could not read the saved biomes ({CachePath}); working them out again. {ex.Message}");
                return false;
            }
        }

        private static void SaveCache(IList<KeyValuePair<string, string>> key)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
                string temp = CachePath + ".tmp";
                File.WriteAllText(temp, BiomeCacheFile.Write(key, _items, _keys));
                if (File.Exists(CachePath))
                {
                    File.Delete(CachePath);
                }
                File.Move(temp, CachePath);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Could not save the item biomes to {CachePath}: {ex.Message}");
            }
        }

        private static void RefreshRecipeList()
        {
            Safe.Run("refresh the recipe list after working out biomes", RecipeList.RecipeListOrganizer.Refresh);
        }

        /// <summary>Axes and pickaxes (anything with chop or pickaxe damage) and their tool tier.</summary>
        private static List<Tool> FindTools()
        {
            var tools = new List<Tool>();
            if (!ObjectDB.instance)
            {
                return tools;
            }
            foreach (var prefab in ObjectDB.instance.m_items)
            {
                var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (!drop)
                {
                    continue;
                }
                var shared = drop.m_itemData.m_shared;
                if (shared.m_damages.m_chop > 0f)
                {
                    tools.Add(new Tool(prefab.name, shared.m_toolTier, ToolKind.Chop));
                }
                if (shared.m_damages.m_pickaxe > 0f)
                {
                    tools.Add(new Tool(prefab.name, shared.m_toolTier, ToolKind.Pickaxe));
                }
            }
            return tools;
        }
    }
}
