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
        public static int Sum(IEnumerable<LootItem> items, string effect, IDictionary<string, AffixTable.Affix> affixes)
        {
            return items.Where(i => i != null).Sum(i => i.Total(effect, affixes));
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
            float total = Sum(list, "movement_speed", affixes) + Sum(list, "held_movement_speed", affixes);
            return Cap(total, "movement_speed", caps);
        }

        /// <summary>
        ///     Less damage taken from one element, in percent: that element's resistance plus all resistances, capped per
        ///     element (affix_caps.csv: "applies to each element separately").
        /// </summary>
        public static float ResistancePercent(string element, IEnumerable<LootItem> items, IDictionary<string, AffixTable.Affix> affixes,
            AffixCaps caps)
        {
            var list = items.ToList();
            float total = Sum(list, element + "_resistance", affixes) + Sum(list, "all_resistances", affixes);
            return Cap(total, element + "_resistance", caps);
        }
    }
}
