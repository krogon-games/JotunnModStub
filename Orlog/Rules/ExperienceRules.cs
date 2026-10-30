using System;

namespace Orlog.Rules
{
    /// <summary>
    ///     How much XP each player gets for a kill: Diablo 2's rules (Arreat Summit, "Experience"), applied to the XP the
    ///     creature is worth. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class ExperienceRules
    {
        // D2, characters below level 25: % of the monster's XP by (monster level - character level), for +6..+10 and -6..-10.
        // 5 levels or less either way = 100%; more than 10 above = 2%; 10 or more below = 5%.
        private static readonly int[] AboveUnder25 = { 88, 68, 36, 15, 2 };
        private static readonly int[] Below = { 81, 62, 43, 24, 5 };

        /// <summary>D2's % of a creature's XP for its level compared with the player's (0-1).</summary>
        public static float LevelDifferenceFactor(int playerLevel, int creatureLevel)
        {
            int diff = creatureLevel - playerLevel;
            if (diff > 0 && playerLevel >= 25)
            {
                // D2, level 25 and up: any higher monster gives XP * (player level / monster level).
                return (float)playerLevel / creatureLevel;
            }
            if (diff >= -5 && diff <= 5)
            {
                return 1f;
            }
            if (diff < -5)
            {
                return Below[Math.Min(-diff, 10) - 6] / 100f;
            }
            return diff > 10 ? 0.02f : AboveUnder25[diff - 6] / 100f;
        }

        /// <summary>
        ///     Each player's XP from a kill worth <paramref name="creatureXp" />. D2's party rule: with more than one player,
        ///     the total grows by <paramref name="bonusPerExtraPlayer" /> per extra player and is split by level (a player's
        ///     share = their level / the sum of levels); then each share gets the level difference factor and, when
        ///     <paramref name="highLevelRatios" /> is given, D2's high-level ratio for that player's level. Rounded (halves up); never
        ///     below 1 while the factor is above 0.
        /// </summary>
        public static long[] Share(long creatureXp, int creatureLevel, int[] playerLevels, float bonusPerExtraPlayer,
            Func<int, float> highLevelRatios = null)
        {
            var result = new long[playerLevels.Length];
            if (creatureXp <= 0 || playerLevels.Length == 0)
            {
                return result;
            }
            // Through decimal so a float setting like 0.35 is exactly 0.35.
            double bonus = (double)(decimal)Math.Max(0f, bonusPerExtraPlayer);
            double total = creatureXp * (1.0 + bonus * (playerLevels.Length - 1));
            double levelSum = 0;
            foreach (int level in playerLevels)
            {
                levelSum += Math.Max(1, level);
            }
            for (int i = 0; i < playerLevels.Length; i++)
            {
                int level = Math.Max(1, playerLevels[i]);
                double factor = LevelDifferenceFactor(level, creatureLevel) * (highLevelRatios != null ? highLevelRatios(level) : 1f);
                if (factor <= 0)
                {
                    continue;
                }
                double xp = total * level / levelSum * factor;
                result[i] = Math.Max(1, (long)Math.Round(xp, MidpointRounding.AwayFromZero));
            }
            return result;
        }
    }
}
