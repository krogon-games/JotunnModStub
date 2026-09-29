using System.Collections.Generic;

namespace CraftingOverhaul.Biomes
{
    internal enum ToolKind
    {
        Any,
        Chop,
        Pickaxe
    }

    /// <summary>A minimum tool tier needed to harvest something, e.g. a bronze axe for birch trees.</summary>
    internal readonly struct ToolRequirement
    {
        public static readonly ToolRequirement None = new ToolRequirement(0, ToolKind.Any);

        public readonly int Tier;
        public readonly ToolKind Kind;

        public ToolRequirement(int tier, ToolKind kind)
        {
            Tier = tier;
            Kind = kind;
        }

        /// <summary>The stricter of two requirements (a log from a tree needing tier 2 keeps tier 2).</summary>
        public ToolRequirement Max(int tier, ToolKind kind)
        {
            return tier > Tier ? new ToolRequirement(tier, kind) : this;
        }

        public override string ToString()
        {
            return Tier <= 0 ? "" : $"{Kind} tier {Tier}";
        }
    }

    /// <summary>What a source produces: an item (by prefab name) or a world progression key such as "defeated_eikthyr".</summary>
    internal enum SourceTarget
    {
        Item,
        GlobalKey
    }

    /// <summary>
    ///     One way to obtain an item (or unlock a global key). Its biome is the latest of: the biome it is found
    ///     in, the biomes of the items it requires, the biome of the cheapest tool that can harvest it, and the
    ///     biome of the boss that sets its required global key.
    /// </summary>
    internal sealed class ItemSource
    {
        public SourceTarget Target = SourceTarget.Item;
        public string Name;
        public Biome? Base;
        public readonly List<string> RequiredItems = new List<string>();
        public ToolRequirement Tool = ToolRequirement.None;
        public string RequiredKey;

        /// <summary>
        ///     Weak evidence, such as random chest loot: only used for items that have no other source, so a stray
        ///     item in an early chest doesn't pull its whole group earlier.
        /// </summary>
        public bool IsWeak;

        /// <summary>Which collector produced the source, e.g. "Vegetation".</summary>
        public string Collector;

        /// <summary>Human-readable chain, e.g. "MineRock_Copper > DropTable".</summary>
        public string Origin;
    }
}
