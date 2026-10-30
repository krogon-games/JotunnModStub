using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Rolls a new Magic or Rare item's affixes (AFFIX_RULES → How an item rolls). No game dependencies, so it is
    ///     unit-tested.
    /// </summary>
    internal static class AffixRoller
    {
        /// <summary>Most affixes on one side: Magic 1 prefix + 1 suffix, Rare 2 + 2.</summary>
        public static int SideLimit(string rarity)
        {
            return rarity == "rare" ? 2 : 1;
        }

        /// <summary>
        ///     Rolls <paramref name="count" /> affixes for an item of the given classes and mechanics:
        ///     <list type="bullet">
        ///         <item>each pick is weighted by the affix's <c>weight</c> among every affix that still fits;</item>
        ///         <item>a side holds at most <see cref="SideLimit" /> affixes, and one affix per family per side;</item>
        ///         <item>values are whole numbers anywhere in the affix's range (every range in the table is whole);</item>
        ///         <item>when both halves of a twin roll, they merge into one resonant line, rolled from the resonant range.</item>
        ///     </list>
        ///     Stops early if nothing else fits, so an item can end with fewer affixes.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static LootItem Roll(string rarity, int count, IEnumerable<AffixTable.Affix> table, ICollection<string> classes,
            ICollection<string> mechanics, Func<double> random)
        {
            var item = new LootItem { Rarity = rarity };
            var picked = new List<AffixTable.Affix>();
            int limit = SideLimit(rarity);
            var all = table.ToDictionary(a => a.Id);
            var eligible = all.Values.Where(a => a.Classes.Any(classes.Contains) &&
                                            (a.Requires.Length == 0 || mechanics.Contains(a.Requires))).ToList();

            for (int n = 0; n < count; n++)
            {
                int prefixes = picked.Count(a => a.IsPrefix);
                int suffixes = picked.Count - prefixes;
                var candidates = eligible.Where(a =>
                    (a.IsPrefix ? prefixes : suffixes) < limit &&
                    !picked.Any(p => p.IsPrefix == a.IsPrefix && p.Family == a.Family) &&
                    a.Weight > 0f).ToList();
                if (candidates.Count == 0)
                {
                    break;
                }
                var affix = WeightedPick(candidates, random);
                picked.Add(affix);

                var twin = item.Lines.FirstOrDefault(l => !l.Resonant && TwinOf(l, affix, picked));
                if (twin != null)
                {
                    var other = picked.First(p => p.Id == twin.Ids[0]);
                    twin.Ids = other.IsPrefix ? new[] { other.Id, affix.Id } : new[] { affix.Id, other.Id };
                    LootItem.Range(twin, all, out int min, out int max);
                    twin.Value = RollValue(min, max, random);
                }
                else
                {
                    item.Lines.Add(new LootItem.Line { Ids = new[] { affix.Id }, Value = RollValue((int)affix.Min, (int)affix.Max, random) });
                }
            }

            // Prefixes first, as the tooltip reads; a resonant line counts as a prefix.
            var byId = picked.ToDictionary(a => a.Id);
            var ordered = item.Lines.OrderBy(l => byId[l.Ids[0]].IsPrefix ? 0 : 1).ToList();
            item.Lines.Clear();
            item.Lines.AddRange(ordered);
            return item;
        }

        /// <summary>A whole number from min to max, both included.</summary>
        public static int RollValue(int min, int max, Func<double> random)
        {
            if (max <= min)
            {
                return min;
            }
            return min + Math.Min(max - min, (int)(random() * (max - min + 1)));
        }

        private static bool TwinOf(LootItem.Line line, AffixTable.Affix affix, List<AffixTable.Affix> picked)
        {
            var other = picked.FirstOrDefault(p => p.Id == line.Ids[0]);
            return other != null && other != affix && other.Family == affix.Family && other.IsPrefix != affix.IsPrefix;
        }

        private static AffixTable.Affix WeightedPick(List<AffixTable.Affix> candidates, Func<double> random)
        {
            double total = candidates.Sum(a => (double)a.Weight);
            double roll = random() * total;
            foreach (var affix in candidates)
            {
                roll -= affix.Weight;
                if (roll < 0)
                {
                    return affix;
                }
            }
            return candidates[candidates.Count - 1];
        }
    }
}
