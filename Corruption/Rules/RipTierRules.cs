using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     What a rip's map tier and affixes change (Krogon's rips round decisions 3, 4, 4b, 6e and 7-10, 2026-11-26): waves,
    ///     guaranteed 2-stars, the heart's shards and gear floor, drop sources and the creatures' damage. Numbers come from
    ///     <c>map_tiers.csv</c> and <c>map_affixes.csv</c>. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class RipTierRules
    {
        /// <summary>The tier's wave multiplier times each affix's (Swarming x2).</summary>
        public static double WaveMultiplier(MapTierTable.TierRow tier, IEnumerable<MapAffixTable.Affix> affixes)
        {
            double multiplier = tier.WaveMultiplier;
            foreach (var affix in affixes ?? Enumerable.Empty<MapAffixTable.Affix>())
            {
                multiplier *= affix.WaveMultiplier;
            }
            return multiplier;
        }

        /// <summary>Whether a wave creature is a guaranteed 2-star. <paramref name="random01" /> is in [0, 1).</summary>
        public static bool IsGuaranteedTwoStar(double percent, double random01)
        {
            return percent > 0 && random01 < percent / 100.0;
        }

        /// <summary>The heart's shards: lowest and highest.</summary>
        public static void ShardRange(MapTierTable.TierRow tier, out int min, out int max)
        {
            min = tier.ShardsMin;
            max = tier.ShardsMax;
        }

        /// <summary>
        ///     The rarity of a player's gear at the heart: the higher of what the roll gave (null, "magic" or "rare", from
        ///     <see cref="DropTable.Roll" />) and the tier's floor ("none", "magic" or "rare"). Null only when both are nothing.
        /// </summary>
        public static string HeartGear(string rolled, string floor)
        {
            int a = Rank(rolled);
            int b = Rank(floor);
            int best = Math.Max(a, b);
            return best == 2 ? "rare" : best == 1 ? "magic" : null;
        }

        private static int Rank(string rarity)
        {
            return rarity == "rare" ? 2 : rarity == "magic" ? 1 : 0;
        }

        /// <summary>The <c>drop_modifiers.csv</c> sources the tier adds to every drop in the rip.</summary>
        public static List<string> DropSources(MapTierTable.TierRow tier)
        {
            return new List<string> { tier.DropSource };
        }

        /// <summary>Product of the affixes' maddened factors; 1 when none sets one (Rabid: 2).</summary>
        public static double MaddenedFactor(IEnumerable<MapAffixTable.Affix> affixes)
        {
            return (affixes ?? Enumerable.Empty<MapAffixTable.Affix>()).Aggregate(1.0, (p, a) => p * a.MaddenedFactor);
        }

        /// <summary>Product of the affixes' damage-taken factors; 1 when none sets one (Hardened: 0.7).</summary>
        public static double DamageTaken(IEnumerable<MapAffixTable.Affix> affixes)
        {
            return (affixes ?? Enumerable.Empty<MapAffixTable.Affix>()).Aggregate(1.0, (p, a) => p * a.DamageTaken);
        }

        /// <summary>Sum of the affixes' fire shares; 0 when none sets one (Smouldering: 0.25).</summary>
        public static double FireShare(IEnumerable<MapAffixTable.Affix> affixes)
        {
            return (affixes ?? Enumerable.Empty<MapAffixTable.Affix>()).Sum(a => a.FireShare);
        }
    }
}
