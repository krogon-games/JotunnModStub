using System;

namespace Corruption.Rules
{
    /// <summary>
    ///     Affixes renamed after players may already have them saved (on gear, as Affix Splinters). Saved lines keep
    ///     their old id in the item's custom data, so reading a line maps the old id to the new one
    ///     (<see cref="LootItem.LineFromText" />); the next write saves the new id. A splinter's item is named after its
    ///     family (<c>CorruptionAffixSplinter_tool_&lt;family&gt;</c>), so the old splinter item names stay registered
    ///     too (<c>SplinterItem</c>), or saved splinters would vanish when the world loads. Keep every row: removing one
    ///     breaks saves made before its rename. No game dependencies, so it is unit-tested.
    ///     Parallel arrays: tuples would need System.ValueTuple.
    /// </summary>
    internal static class AffixRenames
    {
        // 2026-11-19 (Krogon, #33, decisions 12-13): the orb finders. Stonecutter's became Hoardfinder's, Resinous
        // ("of Resin") became "of Hidden Riches"; their effects and families went from extra stone and resin to orbs.
        public static readonly string[] OldIds = { "stonecutters", "resinous" };
        public static readonly string[] NewIds = { "hoardfinders", "hidden_riches" };

        public static readonly string[] OldFamilies = { "extra_stone", "extra_resin" };
        public static readonly string[] NewFamilies = { "orb_find_mining", "orb_find_woodcutting" };

        /// <summary>The current id for a saved or typed affix id (unchanged when it wasn't renamed).</summary>
        public static string Id(string id)
        {
            int i = Array.IndexOf(OldIds, id);
            return i >= 0 ? NewIds[i] : id;
        }

        /// <summary>The family's name before its rename, or null when it was never renamed.</summary>
        public static string OldFamily(string family)
        {
            int i = Array.IndexOf(NewFamilies, family);
            return i >= 0 ? OldFamilies[i] : null;
        }
    }
}
