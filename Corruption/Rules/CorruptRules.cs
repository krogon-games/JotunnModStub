using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>What a Corruption shard did. Nothing changes on the item until the caller acts on it.</summary>
    internal sealed class CorruptResult
    {
        public bool Done;

        /// <summary>Why nothing happened ("" when done). A refusal uses no shard.</summary>
        public string Refusal = "";

        /// <summary>One of <see cref="CorruptionOutcomes.Outcomes" />: bricked, unchanged, affix.</summary>
        public string Outcome = "";

        /// <summary>The item's affixes afterwards (affix outcome only; a new object). Its rarity is unchanged.</summary>
        public LootItem Loot;

        /// <summary>The corrupted line that was added, or the resonant line it completed (affix outcome only).</summary>
        public LootItem.Line Line;

        /// <summary>Whether the corrupted affix completed a twin (the two values added, plus 25%).</summary>
        public bool CompletedTwin;
    }

    /// <summary>
    ///     Corrupting gear with a Corruption shard at the Riftforge (corruption-design.md → Corrupted gear; issue #29):
    ///     which items take it, the risk shown before confirming, and the three outcomes (chances from
    ///     <c>corruption_outcomes.csv</c>). The corrupted affix rolls from the item's normal pool, on top of the normal
    ///     limit, may repeat a family, and when it completes a twin the two values are added with the resonance bonus on
    ///     top (Krogon, 2026-11-10: as a splinter does). Every surviving outcome
    ///     locks the item. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class CorruptRules
    {
        public const string OperationId = "corrupt";

        // Refusal texts (riftforge-ui-brief.md §2).
        public const string AlreadyCorrupted = "Already corrupted";
        public const string NoUniques = "Uniques can't be corrupted";

        // Result texts (Krogon, 2026-11-10: every outcome is said in words; "unchanged" must never read as "no change").
        public const string UnchangedMessage = "Corrupted and locked; no affix added.";

        /// <summary>Why the shard can't be used on the item, or "" when it can. The first reason that applies.</summary>
        public static string Refusal(CraftItem item, IDictionary<string, AffixTable.Affix> affixes)
        {
            if (item.Corrupted)
            {
                return AlreadyCorrupted;
            }
            if (item.Rarity == "unique")
            {
                return NoUniques;
            }
            // A Mirror's copy is locked like a corrupted item (Krogon, 2026-11-10).
            if (item.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            // The affix outcome must be possible before anything is rolled.
            return Candidates(item, affixes).Count > 0 ? "" : OrbRules.CantTakeAffixes;
        }

        /// <summary>
        ///     Affixes the corrupted affix can be: everything the item's class and mechanics can roll (weight above 0).
        ///     The side limits and the one-per-family rule don't bind it.
        /// </summary>
        public static List<AffixTable.Affix> Candidates(CraftItem item, IDictionary<string, AffixTable.Affix> affixes)
        {
            if (item.Classes == null)
            {
                return new List<AffixTable.Affix>();
            }
            return affixes.Values.Where(a => a.Weight > 0f && a.Classes.Any(item.Classes.Contains) &&
                                             (a.Requires.Length == 0 || item.Mechanics.Contains(a.Requires))).ToList();
        }

        /// <summary>
        ///     The risk shown before confirming, from the table: one line per outcome with its chance, then the lock.
        ///     Never suggests which outcome will come.
        /// </summary>
        public static List<string> Preview(CorruptionOutcomes table)
        {
            return new List<string>
            {
                Percent(table, "bricked") + " destroyed: the item is gone for good",
                Percent(table, "unchanged") + " unchanged and locked: no affix is added",
                Percent(table, "affix") + " gains one corrupted affix, and is locked",
                "If it survives it is corrupted: locked for good. Only a Mirror of Kalandra still works on it."
            };
        }

        private static string Percent(CorruptionOutcomes table, string outcome)
        {
            return (table.Percent.TryGetValue(outcome, out var chance) ? chance : 0f).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>Uses a shard on the item. Checks <see cref="Refusal" /> first; never changes <paramref name="item" />.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static CorruptResult Apply(CraftItem item, IDictionary<string, AffixTable.Affix> affixes, CorruptionOutcomes table,
            Func<double> random)
        {
            var result = new CorruptResult { Refusal = Refusal(item, affixes) };
            if (result.Refusal.Length > 0)
            {
                return result;
            }
            result.Outcome = table.Roll(random);
            result.Done = true;
            if (result.Outcome != "affix")
            {
                return result;
            }

            var loot = new LootItem { Rarity = item.Rarity };
            if (item.Loot != null)
            {
                loot.Name = item.Loot.Name;
                loot.Lines.AddRange(item.Loot.Lines.Select(l => new LootItem.Line { Ids = l.Ids.ToArray(), Value = l.Value, Corrupted = l.Corrupted }));
            }
            var affix = AffixRoller.WeightedPick(Candidates(item, affixes), random);
            var twin = loot.Lines.FirstOrDefault(l => !l.Resonant && affixes.TryGetValue(l.Ids[0], out var other) &&
                                                      other.Family == affix.Family && other.IsPrefix != affix.IsPrefix);
            LootItem.Line line;
            if (twin != null)
            {
                // Like a splinter (Krogon, 2026-11-10): the corrupted half rolls its own value, then the two values are
                // added and the resonance bonus (+25%) goes on top. The item's half keeps its value; nothing rerolls.
                float rolled = AffixRoller.RollValue(affix.Min, affix.Max, LootItem.Decimals(affix), random);
                twin.Ids = affix.IsPrefix ? new[] { affix.Id, twin.Ids[0] } : new[] { twin.Ids[0], affix.Id };
                twin.Value = SplinterRules.ResonantSum(twin.Value, rolled, LootItem.Decimals(twin, affixes));
                twin.Corrupted = true;
                line = twin;
                result.CompletedTwin = true;
            }
            else
            {
                line = new LootItem.Line
                {
                    Ids = new[] { affix.Id }, Corrupted = true,
                    Value = AffixRoller.RollValue(affix.Min, affix.Max, LootItem.Decimals(affix), random)
                };
                loot.Lines.Add(line);
            }
            result.Loot = loot;
            result.Line = line;
            return result;
        }
    }
}
