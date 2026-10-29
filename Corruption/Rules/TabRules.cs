using System;

namespace Corruption.Rules
{
    /// <summary>The player's extra inventory tabs (design phase 9; decided by the user 2026-10-29).</summary>
    internal enum InventoryTab
    {
        None,
        Currency,
        Maps
    }

    /// <summary>
    ///     Which tab an item belongs in, and the small sums the tabs need. Pure: no Unity, unit-tested.
    ///
    ///     Decided by the user (2026-10-29): two tabs of 8 x 4 slots; picked-up currency and maps go straight to their
    ///     tab (the bag only when the tab is full); tab contents weigh nothing; on death they go into the gravestone
    ///     with the rest of the inventory.
    /// </summary>
    internal static class TabRules
    {
        public const int Width = 8;
        public const int Height = 4;

        /// <summary>
        ///     Currency (shards, boss essences; later orbs, Affix Splinters and runes) and maps, by prefab name. Items
        ///     are recognised by the prefixes their tables give them (<see cref="EssenceTable.ItemPrefix" />,
        ///     <see cref="MapList.ItemPrefix" />), so a new essence or map row goes to its tab without code changes.
        /// </summary>
        public static InventoryTab TabFor(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return InventoryTab.None;
            }
            if (prefabName.StartsWith(MapList.ItemPrefix, StringComparison.Ordinal))
            {
                return InventoryTab.Maps;
            }
            if (prefabName == ShardRules.ShardPrefab || prefabName.StartsWith(EssenceTable.ItemPrefix, StringComparison.Ordinal))
            {
                return InventoryTab.Currency;
            }
            return InventoryTab.None;
        }

        /// <summary>
        ///     Of a cost of <paramref name="amount" />, how much to take from the tabs, which hold <paramref name="inTabs" />:
        ///     the tabs pay first (that's where currency lives), the rest is left to the bag and other sources.
        /// </summary>
        public static int TakeFromTabs(int amount, int inTabs)
        {
            if (amount <= 0 || inTabs <= 0)
            {
                return 0;
            }
            return Math.Min(amount, inTabs);
        }

        /// <summary>
        ///     Where the <paramref name="index" />th tab item goes when it's put in a gravestone: in rows added under the
        ///     inventory's own (<paramref name="firstRow" /> onwards), <paramref name="width" /> to a row, so nothing
        ///     overlaps the items already there.
        /// </summary>
        public static void GravePosition(int index, int width, int firstRow, out int x, out int y)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            x = index % width;
            y = firstRow + index / width;
        }

        /// <summary>Rows a gravestone needs to show every item, given its own height and its lowest item's row.</summary>
        public static int GraveHeight(int height, int lowestRow)
        {
            return Math.Max(height, lowestRow + 1);
        }
    }
}
