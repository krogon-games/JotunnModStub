using System;

namespace Corruption.Rules
{
    /// <summary>
    ///     Maps as loot (Krogon, 2026-11-23, decisions 18-19 and 31-32): the rip heart's loot always holds one map of the rip's
    ///     biome, and creatures can drop a map of the biome they die in. Monster drops start once the world's first rip
    ///     heart has been destroyed, and a biome's map drops only after that biome's boss is beaten in the world, so the
    ///     chain "each biome's maps need its boss" still holds. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapDropRules
    {
        /// <summary>World key set when any rip heart is destroyed (the game's global keys, shared by everyone on the server).</summary>
        public const string RipClearedKey = "corruption_rip_cleared";

        /// <summary>Whether a creature dying in a biome may roll for a map at all.</summary>
        /// <param name="ripCleared">The world's first rip heart has been destroyed.</param>
        /// <param name="biomeHasMap">The biome has a map (maps.csv; the Ocean has none).</param>
        /// <param name="bossDefeated">The boss whose essence makes this biome's maps is beaten in the world.</param>
        public static bool CanDrop(bool ripCleared, bool biomeHasMap, bool bossDefeated)
        {
            return ripCleared && biomeHasMap && bossDefeated;
        }

        /// <summary>Chance (0–1) of one roll: 1 in <paramref name="oneIn" /> times the sources' multiplier, capped at 1; 0 when off.</summary>
        public static float Chance(float oneIn, float multiplier)
        {
            return oneIn < 1f ? 0f : (float)Math.Min(1.0, multiplier / oneIn);
        }

        /// <summary>One player's map roll for one kill.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static bool Roll(float oneIn, float multiplier, Func<double> random)
        {
            float chance = Chance(oneIn, multiplier);
            return chance > 0f && random() < chance;
        }
    }
}
