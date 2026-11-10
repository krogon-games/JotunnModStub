using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Adds up player-stat affixes over the items a player has equipped and applies the stat caps
    ///     (<c>Corruption/Data/affix_caps.csv</c>). No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class AffixTotals
    {
        public static readonly string[] Elements = { "fire", "frost", "lightning", "poison" };

        /// <summary>The total of one affix effect over several items (e.g. every equipped item's <c>carry_weight</c>).</summary>
        public static float Sum(IEnumerable<LootItem> items, string effect, IDictionary<string, AffixTable.Affix> affixes)
        {
            return items.Where(i => i != null).Sum(i => i.Total(effect, affixes));
        }

        /// <summary>The same total over only the shard's corrupted lines, or only the ordinary ones.</summary>
        public static float Sum(IEnumerable<LootItem> items, string effect, IDictionary<string, AffixTable.Affix> affixes, bool corrupted)
        {
            return items.Where(i => i != null).Sum(i => i.Total(effect, affixes, corrupted));
        }

        /// <summary>
        ///     A capped stat with corrupted affixes: the ordinary total is capped as usual and the corrupted total adds
        ///     on top, so a corrupted item can go past the cap without uncapping anything else (corruption-design.md →
        ///     Corrupted gear; Krogon, 2026-11-10: over everything worn).
        /// </summary>
        public static float Capped(float ordinary, float corrupted, string family, AffixCaps caps)
        {
            return Cap(ordinary, family, caps) + Math.Max(0f, corrupted);
        }

        /// <summary>The value after the cap that lists this affix family; uncapped families pass through.</summary>
        public static float Cap(float total, string family, AffixCaps caps)
        {
            var cap = caps?.Caps.FirstOrDefault(c => c.Families.Contains(family));
            return cap == null ? total : Math.Min(total, cap.Max);
        }

        /// <summary>
        ///     Movement speed in percent: worn movement affixes plus held ones (Fleet), together under the movement cap
        ///     (its families: movement_speed, held_movement, foraging).
        /// </summary>
        public static float MovementPercent(IEnumerable<LootItem> items, IDictionary<string, AffixTable.Affix> affixes, AffixCaps caps)
        {
            var list = items.ToList();
            return Capped(MovementOrdinary(list, affixes), MovementCorrupted(list, affixes), "movement_speed", caps);
        }

        /// <summary>Worn and held movement affixes that count under the cap.</summary>
        public static float MovementOrdinary(IEnumerable<LootItem> items, IDictionary<string, AffixTable.Affix> affixes)
        {
            var list = items.ToList();
            return Sum(list, "movement_speed", affixes, false) + Sum(list, "held_movement_speed", affixes, false);
        }

        /// <summary>Worn and held movement affixes added by shards: on top of the cap.</summary>
        public static float MovementCorrupted(IEnumerable<LootItem> items, IDictionary<string, AffixTable.Affix> affixes)
        {
            var list = items.ToList();
            return Sum(list, "movement_speed", affixes, true) + Sum(list, "held_movement_speed", affixes, true);
        }

        /// <summary>
        ///     Less damage taken from one element, in percent: that element's resistance plus all resistances, capped per
        ///     element (affix_caps.csv: "applies to each element separately").
        /// </summary>
        public static float ResistancePercent(string element, IEnumerable<LootItem> items, IDictionary<string, AffixTable.Affix> affixes,
            AffixCaps caps)
        {
            var list = items.ToList();
            float ordinary = Sum(list, element + "_resistance", affixes, false) + Sum(list, "all_resistances", affixes, false);
            float corrupted = Sum(list, element + "_resistance", affixes, true) + Sum(list, "all_resistances", affixes, true);
            // Never above 100%: past that the hit would heal.
            return Math.Min(100f, Capped(ordinary, corrupted, element + "_resistance", caps));
        }
    }
}
