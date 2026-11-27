using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>The two map tables an orb or a rip needs.</summary>
    internal sealed class MapTables
    {
        public MapTierTable Tiers;
        public MapAffixTable Affixes;
    }

    /// <summary>What an orb did to a map. Nothing changes on the item until the caller writes <see cref="State" /> back.</summary>
    internal sealed class MapOrbResult
    {
        public bool Done;
        /// <summary>Why nothing happened ("" when done).</summary>
        public string Refusal = "";
        /// <summary>The map afterwards (a new object; the input is left as it was).</summary>
        public MapState State;
        public List<string> Added = new List<string>();
        public List<string> Removed = new List<string>();
        /// <summary>Divine Orb: the ids of the affixes whose values were rerolled.</summary>
        public List<string> Rerolled = new List<string>();
        /// <summary>A short line for the Forge, e.g. "Orb of Augmentation: now tier 3".</summary>
        public string Headline = "";
        /// <summary>Mirror of Kalandra: the caller makes the copy (and marks it mirrored); the map itself is unchanged.</summary>
        public bool Copy;
    }

    /// <summary>
    ///     The currency orbs on maps (Krogon's rips round decision 6b, 2026-11-26): they put affixes on a map exactly as on
    ///     gear, but a map's affixes have no prefix/suffix sides. Each affix rolls a value from its range (2026-11-27) and the
    ///     Divine Orb rerolls them; no two affixes of one group share a map. The tier follows the affix count. Refusal
    ///     texts reuse <see cref="OrbRules" />' gear wording. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapOrbRules
    {
        /// <summary>Divine Orb on a map none of whose affixes has a range.</summary>
        public const string NoValues = "Map affixes have no values to reroll.";
        public const string NoSockets = "Maps have no sockets.";
        public const string OnlyNormalMap = "Only on Normal maps";
        public const string OnlyMagicOne = "Only on Magic maps with 1 affix";
        public const string OnlyMagicTwo = "Only on Magic maps with 2 affixes";
        public const string OnlyRareThree = "Only on Rare maps with 3 affixes";
        public const string NoAffixesLeft = "This map has no affixes";
        public const string TooFewAffixes = "Not enough different map affixes are left to roll";

        /// <summary>Why the orb can't be used on the map, or null when it can.</summary>
        public static string Refusal(string orb, MapState state, MapTables tables)
        {
            if (!OrbRules.Orbs.Contains(orb))
            {
                return "Unknown orb";
            }
            if (orb == "mirror")
            {
                // Corrupted maps can be mirrored (as corrupted gear); a mirror copy can't be copied again.
                return state.Mirrored ? OrbRules.MirroredCopy : null;
            }
            if (state.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            if (state.Corrupted)
            {
                return OrbRules.CorruptedLocked;
            }
            int count = state.Affixes.Count;
            string rarity = tables.Tiers.Row(state.Tier).Rarity;
            int add = 0;
            switch (orb)
            {
                case "jeweller":
                    return NoSockets;
                case "divine":
                    return state.Rolls(tables.Affixes).Any(r => r.Affix.Ranged) ? null : NoValues;
                case "transmutation":
                    if (rarity != "normal") return OnlyNormalMap;
                    add = 1;
                    break;
                case "alchemy":
                case "chance":
                    if (rarity != "normal") return OnlyNormalMap;
                    add = 3;
                    break;
                case "augmentation":
                    if (rarity != "magic" || count != 1) return OnlyMagicOne;
                    add = 1;
                    break;
                case "regal":
                    if (rarity != "magic" || count != 2) return OnlyMagicTwo;
                    add = 1;
                    break;
                case "exalted":
                    if (rarity != "rare" || count != 3) return OnlyRareThree;
                    add = 1;
                    break;
                case "annulment":
                    return count > 0 ? null : NoAffixesLeft;
            }
            return Capacity(state, tables) >= add ? null : TooFewAffixes;
        }

        /// <summary>The rollable affixes the map can take: not on it already, and none whose group is already on it.</summary>
        public static List<MapAffixTable.Affix> Candidates(MapState state, MapTables tables)
        {
            return tables.Affixes.Rollable.Where(a => tables.Affixes.FitsWith(a, state.Affixes)).ToList();
        }

        /// <summary>How many more affixes the map can take: candidates, counting each group once.</summary>
        public static int Capacity(MapState state, MapTables tables)
        {
            var pool = Candidates(state, tables);
            return pool.Count(a => a.Group.Length == 0) + pool.Where(a => a.Group.Length > 0).Select(a => a.Group).Distinct().Count();
        }

        /// <summary>Uses the orb on the map. Checks <see cref="Refusal" /> first; never changes <paramref name="state" />.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static MapOrbResult Apply(string orb, MapState state, MapTables tables, Func<double> random)
        {
            var result = new MapOrbResult();
            string refusal = Refusal(orb, state, tables);
            if (refusal != null)
            {
                result.Refusal = refusal;
                return result;
            }
            var next = state.Copy();
            result.State = next;
            result.Done = true;
            switch (orb)
            {
                case "mirror":
                    result.Copy = true;
                    result.Headline = OrbRules.Name(orb) + ": a mirrored copy";
                    return result;
                case "annulment":
                    int index = Math.Min((int)(random() * next.Affixes.Count), next.Affixes.Count - 1);
                    result.Removed.Add(next.Affixes[index]);
                    next.Values.Remove(next.Affixes[index]);
                    next.Affixes.RemoveAt(index);
                    break;
                case "divine":
                    foreach (var roll in next.Rolls(tables.Affixes))
                    {
                        if (roll.Affix.Ranged)
                        {
                            next.Values[roll.Affix.Id] = roll.Affix.Roll(random);
                            result.Rerolled.Add(roll.Affix.Id);
                        }
                    }
                    break;
                case "transmutation":
                case "augmentation":
                case "regal":
                case "exalted":
                    AddRandom(next, tables, random, 1, result.Added);
                    break;
                case "alchemy":
                case "chance":
                    AddRandom(next, tables, random, 3, result.Added);
                    break;
            }
            next.Refresh(tables.Tiers);
            result.Headline = orb == "divine" ? OrbRules.Name(orb) + ": values rerolled" : OrbRules.Name(orb) + ": now tier " + next.Tier;
            return result;
        }

        /// <summary>
        ///     Adds <paramref name="count" /> random affixes (weighted, never a repeat or a second of a group), each with a rolled
        ///     value; stops early if none fit. Also used by the co_map dev command.
        /// </summary>
        public static void AddRandom(MapState state, MapTables tables, Func<double> random, int count, List<string> added)
        {
            for (int n = 0; n < count; n++)
            {
                var pool = Candidates(state, tables);
                if (pool.Count == 0)
                {
                    break;
                }
                double total = pool.Sum(a => a.Weight);
                double roll = random() * total;
                var pick = pool[pool.Count - 1];
                foreach (var affix in pool)
                {
                    if (roll < affix.Weight)
                    {
                        pick = affix;
                        break;
                    }
                    roll -= affix.Weight;
                }
                state.Affixes.Add(pick.Id);
                state.Values[pick.Id] = pick.Roll(random);
                added.Add(pick.Id);
            }
        }
    }
}
