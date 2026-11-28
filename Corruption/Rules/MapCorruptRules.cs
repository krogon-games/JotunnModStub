using System;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     A Corruption Shard on a map (Krogon's rips round decisions 6 and 6c, 2026-11-26; decision 27 of 2026-11-28 on
    ///     claude/wizardly-wozniak-4aix76): a map of any tier can be corrupted; it goes up one tier (tier 5 becomes 6), is
    ///     locked, and is never destroyed. Since 2026-11-28 (#97, the rips round's
    ///     decision 6d: "once corrupted-only affixes exist, every corrupted map gets one") it also gains one random
    ///     corrupted-only affix as its 5th (<see cref="MapAffixTable.RollableCorrupted" />), when any is rollable. No game
    ///     dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapCorruptRules
    {
        /// <summary>Why the shard can't be used on the map, or null when it can.</summary>
        public static string Refusal(MapState state, MapTables tables)
        {
            if (state.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            if (state.Corrupted)
            {
                return "This map is already corrupted.";
            }
            // Any tier (decision 27, 2026-11-28; it was "only a full tier 5 map").
            return null;
        }

        private static bool IsCorruptedOnly(string id, MapTables tables)
        {
            var affix = tables.Affixes.Find(id);
            return affix != null && affix.CorruptedOnly;
        }

        /// <summary>
        ///     Adds one random corrupted-only affix (weighted, with a rolled value) unless the map already has one or none
        ///     is rollable. Returns its id, or null. Also used by the co_map dev command for a random tier 6 map.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1); null picks the first.</param>
        public static string AddCorruptedAffix(MapState state, MapTables tables, Func<double> random)
        {
            if (state.Affixes.Any(id => IsCorruptedOnly(id, tables)))
            {
                return null;
            }
            var pool = tables.Affixes.RollableCorrupted.Where(a => tables.Affixes.FitsWith(a, state.Affixes)).ToList();
            if (pool.Count == 0)
            {
                return null;
            }
            var pick = pool[0];
            if (random != null)
            {
                double roll = random() * pool.Sum(a => a.Weight);
                pick = pool[pool.Count - 1];
                foreach (var affix in pool)
                {
                    if (roll < affix.Weight)
                    {
                        pick = affix;
                        break;
                    }
                    roll -= affix.Weight;
                }
            }
            state.Affixes.Add(pick.Id);
            state.CorruptedOnly++;
            state.Values[pick.Id] = random != null ? pick.Roll(random) : pick.Min;
            return pick.Id;
        }

        /// <summary>The corrupted map (a new object), or null when <see cref="Refusal" /> says no. The map is never destroyed.</summary>
        /// <param name="random">Rolls the corrupted-only affix and its value; null gives the first one at its lowest value.</param>
        public static MapState Apply(MapState state, MapTables tables, out string headline, Func<double> random = null)
        {
            string refusal = Refusal(state, tables);
            if (refusal != null)
            {
                headline = refusal;
                return null;
            }
            var next = state.Copy();
            next.Corrupted = true;
            AddCorruptedAffix(next, tables, random);
            next.Refresh(tables.Tiers);
            headline = "Corrupted: now tier " + next.Tier;
            return next;
        }
    }
}
