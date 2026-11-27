using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     What a rip's map tier and affixes change (Krogon's rips round decisions 3, 4, 4b, 6e and 7-10, 2026-11-26; the affix pool and rolled values 2026-11-27): waves,
    ///     guaranteed 2-stars, the heart's shards and gear floor, drop sources and the creatures' damage. Numbers come from
    ///     <c>map_tiers.csv</c> and <c>map_affixes.csv</c>. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class RipTierRules
    {
        private static IEnumerable<MapAffixRoll> Of(IEnumerable<MapAffixRoll> rolls, string effect)
        {
            return (rolls ?? Enumerable.Empty<MapAffixRoll>()).Where(r => r.Affix.Effect == effect);
        }

        private static double Product(IEnumerable<MapAffixRoll> rolls, string effect, bool more)
        {
            return Of(rolls, effect).Aggregate(1.0, (p, r) => p * (more ? 1 + r.Value / 100.0 : 1 - r.Value / 100.0));
        }

        private static double Sum(IEnumerable<MapAffixRoll> rolls, string effect)
        {
            return Of(rolls, effect).Sum(r => r.Value);
        }

        /// <summary>The tier's wave multiplier times (1 + density%) for each density affix (Teeming 50: x1.5).</summary>
        public static double WaveMultiplier(MapTierTable.TierRow tier, IEnumerable<MapAffixRoll> rolls)
        {
            return tier.WaveMultiplier * Product(rolls, MapAffixTable.Effects.Density, true);
        }

        /// <summary>Experience creatures give: product of (1 + v/100) over experience affixes.</summary>
        public static double ExperienceFactor(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.Experience, true);
        }

        /// <summary>Damage creatures deal: product of (1 + v/100) over damage affixes.</summary>
        public static double DamageFactor(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.Damage, true);
        }

        /// <summary>Attack speed: product of (1 + v/100) over attack_speed affixes.</summary>
        public static double AttackSpeedFactor(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.AttackSpeed, true);
        }

        /// <summary>Chance (0 to 1) that a hit deals double damage: 1 minus the product of (1 - v/100).</summary>
        public static double DoubleDamageChance(IEnumerable<MapAffixRoll> rolls)
        {
            return 1.0 - Product(rolls, MapAffixTable.Effects.DoubleDamage, false);
        }

        /// <summary>Health: product of (1 + v/100) over health affixes.</summary>
        public static double HealthFactor(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.Health, true);
        }

        /// <summary>Extra move speed as a fraction: sum of v/100 over move_speed affixes.</summary>
        public static double MoveSpeedBonus(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.MoveSpeed) / 100.0;
        }

        /// <summary>Whether creatures can't be staggered.</summary>
        public static bool NoStagger(IEnumerable<MapAffixRoll> rolls)
        {
            return Of(rolls, MapAffixTable.Effects.NoStagger).Any();
        }

        /// <summary>Multiplier on physical damage creatures take: product of (1 - v/100).</summary>
        public static double PhysicalTaken(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.PhysicalTaken, false);
        }

        /// <summary>Multiplier on elemental damage creatures take: product of (1 - v/100).</summary>
        public static double ElementalTaken(IEnumerable<MapAffixRoll> rolls)
        {
            return Product(rolls, MapAffixTable.Effects.ElementalTaken, false);
        }

        /// <summary>Health points a player near the heart loses each second.</summary>
        public static double LifeDrainPerSecond(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.LifeDrain);
        }

        /// <summary>Stamina points a player near the heart loses each second.</summary>
        public static double StaminaDrainPerSecond(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.StaminaDrain);
        }

        /// <summary>Percentage points taken off every resistance of a player near the heart.</summary>
        public static double ResistancePenaltyPercent(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.Resistances);
        }

        /// <summary>Share of a hit's physical damage added as poison (0.5 = 50%).</summary>
        public static double PoisonShare(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.PoisonShare) / 100.0;
        }

        /// <summary>Share of a hit's physical damage added as fire.</summary>
        public static double FireShare(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.FireShare) / 100.0;
        }

        /// <summary>Share of a hit's physical damage added as lightning.</summary>
        public static double LightningShare(IEnumerable<MapAffixRoll> rolls)
        {
            return Sum(rolls, MapAffixTable.Effects.LightningShare) / 100.0;
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
    }
}
