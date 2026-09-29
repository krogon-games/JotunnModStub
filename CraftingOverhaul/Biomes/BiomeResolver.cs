using System;
using System.Collections.Generic;
using System.Diagnostics;
using CraftingOverhaul.Biomes.Collectors;

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
    ///     The rules themselves live in <see cref="BiomeSolver" />; this class gathers the data and caches the result.
    /// </summary>
    internal static class BiomeResolver
    {
        private static readonly ISourceCollector[] Collectors =
        {
            new VegetationCollector(),
            new SpawnCollector(),
            new LocationCollector(),
            new ConversionCollector(),
            new ProducerPieceCollector(),
            new RecipeCollector()
        };

        private static Dictionary<string, BiomeResolution> _items;
        private static Dictionary<string, BiomeResolution> _keys;
        private static List<ItemSource> _sources;

        public static bool IsResolved => _items != null;

        /// <summary>Every collected source, for the dump command.</summary>
        public static IReadOnlyList<ItemSource> Sources
        {
            get
            {
                EnsureResolved();
                return _sources;
            }
        }

        public static void Invalidate()
        {
            _items = null;
            _keys = null;
            _sources = null;
        }

        public static bool TryGet(string itemPrefab, out BiomeResolution resolution)
        {
            EnsureResolved();
            return _items.TryGetValue(itemPrefab, out resolution);
        }

        public static bool TryGetKey(string globalKey, out BiomeResolution resolution)
        {
            EnsureResolved();
            return _keys.TryGetValue(globalKey, out resolution);
        }

        public static IEnumerable<KeyValuePair<string, BiomeResolution>> AllItems()
        {
            EnsureResolved();
            return _items;
        }

        /// <summary>Runs the collectors and solver if not done yet for this world. Needs a loaded world.</summary>
        public static void EnsureResolved()
        {
            if (_items != null)
            {
                return;
            }

            var timer = Stopwatch.StartNew();
            _sources = new List<ItemSource>();
            foreach (var collector in Collectors)
            {
                int before = _sources.Count;
                try
                {
                    collector.Collect(_sources);
                }
                catch (Exception ex)
                {
                    Jotunn.Logger.LogWarning($"Biome collector {collector.Name} failed: {ex}");
                }
                Jotunn.Logger.LogDebug($"Biome collector {collector.Name}: {_sources.Count - before} sources");
            }

            Solve();
            Jotunn.Logger.LogInfo($"Resolved biomes for {_items.Count} items from {_sources.Count} sources in {timer.ElapsedMilliseconds} ms");
        }

        private static void Solve()
        {
            var solver = new BiomeSolver(_sources, BiomeOverrides.Parse(ModConfig.BiomeMaterialOverrides.Value), FindTools()).Solve();
            if (!solver.Settled)
            {
                Jotunn.Logger.LogWarning("Biome resolution did not settle; results may be incomplete.");
            }
            _items = solver.Items;
            _keys = solver.Keys;
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
