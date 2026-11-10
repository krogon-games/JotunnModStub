using System;

namespace Corruption.Rules
{
    /// <summary>
    ///     The player's extra inventory tabs. Started 2026-10-29 (Currency and Maps); six tabs since Krogon's round
    ///     brief of 2026-11-09 (issue #30). <c>None</c> is the Bag. The numbers are internal only (the tabs are saved
    ///     under string keys), so new tabs are added at the end.
    /// </summary>
    internal enum InventoryTab
    {
        None,
        Currency,
        Maps,
        Splinters,
        Runes,
        Essences
    }

    /// <summary>
    ///     Which tab an item belongs in, and the small sums the tabs need. Pure: no Unity, unit-tested.
    ///
    ///     Decided by the user: 2026-10-29, tabs of 8 x 4 slots; picked-up currency and maps go straight to their tab
    ///     (the bag only when the tab is full); tab contents weigh nothing; on death they go into the gravestone with
    ///     the rest of the inventory. 2026-11-09 (#30): six tabs, Bag · Currencies · Splinters · Maps · Runes ·
    ///     Essences; boss essences move out of Currencies into Essences; Splinters is a plain bag-sized grid; Runes
    ///     takes nothing at all until runes are built.
    /// </summary>
    internal static class TabRules
    {
        /// <summary>
        ///     The game's own bag (<c>Humanoid.m_inventory</c>: 8 x 4, without EquipmentAndQuickSlots' or a backpack's
        ///     extra rows). Every tab has this size, Splinters by the user's rule ("capacity equals the default bag").
        /// </summary>
        public const int Width = 8;

        public const int Height = 4;

        /// <summary>The tab buttons, left to right (user, 2026-11-09). <c>None</c> is the Bag.</summary>
        public static readonly InventoryTab[] DisplayOrder =
        {
            InventoryTab.None, InventoryTab.Currency, InventoryTab.Splinters, InventoryTab.Maps, InventoryTab.Runes, InventoryTab.Essences
        };

        /// <summary>The tabs that hold items and are saved with the character. Runes isn't one of them.</summary>
        public static readonly InventoryTab[] StoredTabs =
        {
            InventoryTab.Currency, InventoryTab.Splinters, InventoryTab.Maps, InventoryTab.Essences
        };

        /// <summary>
        ///     Whether anything may ever be put in a tab. Runes takes nothing (user, 2026-11-09: an empty placeholder
        ///     until the rune feature is deliberately built); the Bag and every stored tab do.
        /// </summary>
        public static bool AcceptsItems(InventoryTab tab)
        {
            return tab != InventoryTab.Runes;
        }

        /// <summary>
        ///     Where an item belongs, by prefab name: orbs and the Corruption shard in Currencies, boss essences in
        ///     Essences, Affix Splinters in Splinters, maps in Maps, everything else in the bag. Items are recognised by
        ///     the prefixes their tables give them (<see cref="OrbRules.ItemPrefix" />, <see cref="EssenceTable.ItemPrefix" />,
        ///     <see cref="ShatterRules.SplinterPrefab" />, <see cref="MapList.ItemPrefix" />), so a new row goes to its tab
        ///     without code changes. Never <see cref="InventoryTab.Runes" />: no name sends an item there.
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
            if (prefabName.StartsWith(EssenceTable.ItemPrefix, StringComparison.Ordinal))
            {
                return InventoryTab.Essences;
            }
            if (prefabName.StartsWith(ShatterRules.SplinterPrefab, StringComparison.Ordinal))
            {
                return InventoryTab.Splinters;
            }
            if (prefabName == ShardRules.ShardPrefab || prefabName.StartsWith(OrbRules.ItemPrefix, StringComparison.Ordinal))
            {
                return InventoryTab.Currency;
            }
            return InventoryTab.None;
        }

        /// <summary>
        ///     Whether an item sitting in tab <paramref name="current" /> belongs in another tab, and which: an essence
        ///     saved in Currencies before Essences existed, say. <see cref="InventoryTab.None" /> when it's where it
        ///     belongs, or belongs in no tab (then it stays put: nothing is pushed out to a full bag).
        /// </summary>
        public static InventoryTab MoveTo(InventoryTab current, string prefabName)
        {
            var belongs = TabFor(prefabName);
            return belongs == InventoryTab.None || belongs == current || !AcceptsItems(belongs) ? InventoryTab.None : belongs;
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
