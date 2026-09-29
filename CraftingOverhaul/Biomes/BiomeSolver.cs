using System;
using System.Collections.Generic;

namespace CraftingOverhaul.Biomes
{
    internal sealed class BiomeResolution
    {
        public Biome Biome;
        public ItemSource Source; // null for overrides
        public bool IsOverride;
    }

    internal readonly struct Tool
    {
        public readonly string Item;
        public readonly int Tier;
        public readonly ToolKind Kind;

        public Tool(string item, int tier, ToolKind kind)
        {
            Item = item;
            Tier = tier;
            Kind = kind;
        }
    }

    /// <summary>
    ///     The resolution rules, free of game dependencies so they can be tested on their own.
    ///     <list type="bullet">
    ///         <item>
    ///             A source's biome is the latest of its own biome, its required items, the cheapest tool that can
    ///             harvest it and the biome where its required global key is unlocked. Unknown requirement = unusable.
    ///         </item>
    ///         <item>An item's (or key's) biome is its earliest usable source.</item>
    ///         <item>Overrides are fixed and are used by everything that depends on them.</item>
    ///         <item>
    ///             Weak sources (random chest loot) only count for items that no other source can place. This takes
    ///             two runs: one without weak sources to find those items, then one with weak sources limited to them.
    ///         </item>
    ///     </list>
    ///     Each run repeats until nothing changes. Values only move earlier, so it always settles.
    /// </summary>
    internal sealed class BiomeSolver
    {
        public readonly Dictionary<string, BiomeResolution> Items = new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, BiomeResolution> Keys = new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
        public bool Settled { get; private set; }

        private readonly List<ItemSource> _sources;
        private readonly List<Tool> _tools;
        private readonly IDictionary<string, Biome> _overrides;

        public BiomeSolver(List<ItemSource> sources, IDictionary<string, Biome> overrides, List<Tool> tools)
        {
            _sources = sources;
            _tools = tools;
            _overrides = overrides;
        }

        public BiomeSolver Solve(int maxPasses = 100)
        {
            // Run 1: strong sources only. Anything still unplaced may then use weak sources.
            Run(_ => false, maxPasses);
            if (!_sources.Exists(s => s.IsWeak))
            {
                return this;
            }
            var unplaced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in _sources)
            {
                if (s.Target == SourceTarget.Item && !Items.ContainsKey(s.Name))
                {
                    unplaced.Add(s.Name);
                }
            }
            Run(s => unplaced.Contains(s.Name), maxPasses);
            return this;
        }

        private void Run(Predicate<ItemSource> weakAllowed, int maxPasses)
        {
            Items.Clear();
            Keys.Clear();
            Settled = false;
            foreach (var entry in _overrides)
            {
                Items[entry.Key] = new BiomeResolution { Biome = entry.Value, IsOverride = true };
            }

            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool changed = false;
                foreach (var source in _sources)
                {
                    if (source.IsWeak && (source.Target != SourceTarget.Item || !weakAllowed(source)))
                    {
                        continue;
                    }
                    var value = Evaluate(source);
                    if (value == null)
                    {
                        continue;
                    }
                    var table = source.Target == SourceTarget.GlobalKey ? Keys : Items;
                    if (!table.TryGetValue(source.Name, out var current) ||
                        (!current.IsOverride && value.Value < current.Biome))
                    {
                        table[source.Name] = new BiomeResolution { Biome = value.Value, Source = source };
                        changed = true;
                    }
                }
                if (!changed)
                {
                    Settled = true;
                    return;
                }
            }
        }

        /// <summary>The biome a source becomes available in, or null while any requirement is unknown.</summary>
        public Biome? Evaluate(ItemSource source)
        {
            Biome? value = source.Base;

            foreach (var item in source.RequiredItems)
            {
                if (!Items.TryGetValue(item, out var r))
                {
                    return null;
                }
                value = Later(value, r.Biome);
            }

            if (source.Tool.Tier > 0)
            {
                var tool = CheapestTool(source.Tool);
                if (tool == null)
                {
                    return null;
                }
                value = Later(value, tool.Value);
            }

            if (!string.IsNullOrEmpty(source.RequiredKey))
            {
                if (!Keys.TryGetValue(source.RequiredKey, out var key))
                {
                    return null;
                }
                value = Later(value, key.Biome);
            }

            return value;
        }

        private Biome? CheapestTool(ToolRequirement need)
        {
            Biome? best = null;
            foreach (var tool in _tools)
            {
                if (tool.Tier < need.Tier || (need.Kind != ToolKind.Any && tool.Kind != need.Kind))
                {
                    continue;
                }
                if (Items.TryGetValue(tool.Item, out var r) && (best == null || r.Biome < best.Value))
                {
                    best = r.Biome;
                }
            }
            return best;
        }

        private static Biome Later(Biome? a, Biome b)
        {
            return a == null || b > a.Value ? b : a.Value;
        }
    }
}
