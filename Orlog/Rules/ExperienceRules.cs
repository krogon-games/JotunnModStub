using System;

namespace Orlog.Rules
{
    /// <summary>
    ///     How much XP each player gets for a kill. Creatures at or below the player's level follow Diablo 2 (Arreat
    ///     Summit, "Experience"); creatures above follow classic WoW (vmangos <c>Formulas.h</c>), because D2 pays a higher
    ///     creature's full XP and that made levelling 2-3x faster than the pace (play-test 2026-10-30, user chose this).
    ///     No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class ExperienceRules
    {
        // D2: % of a lower monster's XP for 6..10 levels below the character; 10 or more below = 5%, 5 or fewer = 100%.
        private static readonly int[] Below = { 81, 62, 43, 24, 5 };

        /// <summary>D2's % of a lower creature's XP (0-1); 1 for a creature at or above the player's level.</summary>
        public static double LowerCreatureFactor(int playerLevel, int creatureLevel)
        {
            int below = playerLevel - creatureLevel;
            return below <= 5 ? 1.0 : Below[Math.Min(below, 10) - 6] / 100.0;
        }

        /// <summary>Classic WoW's bonus for a higher creature: +5% per level above the player, at most +20%.</summary>
        public static double HigherCreatureBonus(int playerLevel, int creatureLevel)
        {
            return 1.0 + 0.05 * Math.Max(0, Math.Min(creatureLevel - playerLevel, 4));
        }

        /// <summary>
        ///     What a kill is worth to one player before sharing. At or below the player's level: the creature's own XP
        ///     (<paramref name="xpAt" /> of its level) times D2's lower-creature factor. Above: the XP of a creature of the
        ///     player's own level times WoW's higher-creature bonus.
        /// </summary>
        public static double Worth(Func<int, long> xpAt, int playerLevel, int creatureLevel)
        {
            playerLevel = Math.Max(1, playerLevel);
            creatureLevel = Math.Max(1, creatureLevel);
            return creatureLevel > playerLevel
                ? xpAt(playerLevel) * HigherCreatureBonus(playerLevel, creatureLevel)
                : xpAt(creatureLevel) * LowerCreatureFactor(playerLevel, creatureLevel);
        }

        /// <summary>
        ///     Each player's XP from a kill. <paramref name="xpAt" /> gives a creature's XP by level (already including stars
        ///     and the XP multiplier). D2's party rule: with more than one player, the total grows by
        ///     <paramref name="bonusPerExtraPlayer" /> per extra player and is split by level (a player's share = their level
        ///     / the sum of levels), each share taken of what the kill is worth to that player (<see cref="Worth" />); then
        ///     D2's high-level ratio when <paramref name="highLevelRatios" /> is given. Rounded (halves up); never below 1
        ///     while the kill is worth anything.
        /// </summary>
        public static long[] Share(Func<int, long> xpAt, int creatureLevel, int[] playerLevels, float bonusPerExtraPlayer,
            Func<int, float> highLevelRatios = null)
        {
            var result = new long[playerLevels.Length];
            if (playerLevels.Length == 0)
            {
                return result;
            }
            // Through decimal so a float setting like 0.35 is exactly 0.35.
            double bonus = (double)(decimal)Math.Max(0f, bonusPerExtraPlayer);
            double pool = 1.0 + bonus * (playerLevels.Length - 1);
            double levelSum = 0;
            foreach (int level in playerLevels)
            {
                levelSum += Math.Max(1, level);
            }
            for (int i = 0; i < playerLevels.Length; i++)
            {
                int level = Math.Max(1, playerLevels[i]);
                double xp = Worth(xpAt, level, creatureLevel) * pool * level / levelSum *
                            (highLevelRatios != null ? highLevelRatios(level) : 1f);
                if (xp <= 0)
                {
                    continue;
                }
                result[i] = Math.Max(1, (long)Math.Round(xp, MidpointRounding.AwayFromZero));
            }
            return result;
        }
    }
}
