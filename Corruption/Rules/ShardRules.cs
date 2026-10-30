using System;

namespace Corruption.Rules
{
    /// <summary>How many corruption shards a dying creature drops. No game dependencies, so it is unit-tested.</summary>
    internal static class ShardRules
    {
        /// <summary>The shard's prefab name (<c>Texts.ShardPrefab</c> uses this one).</summary>
        public const string ShardPrefab = "CorruptionShard";

        public sealed class Settings
        {
            public float OneStarChance;
            public float TwoStarChance;
            public int BossMin;
            public int BossMax;
            public bool BossFirstKillOnly;
        }

        /// <param name="level">Creature level: 1 = no stars, 2 = 1 star, 3 = 2 stars.</param>
        /// <param name="alreadyDefeated">For bosses: whether this boss's defeat key is already set in the world.</param>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static int ShardsFor(bool isBoss, int level, bool alreadyDefeated, Settings settings, Func<double> random)
        {
            if (isBoss)
            {
                if (settings.BossFirstKillOnly && alreadyDefeated)
                {
                    return 0;
                }
                int min = Math.Max(0, settings.BossMin);
                int max = Math.Max(min, settings.BossMax);
                return min + (int)Math.Floor(random() * (max - min + 1));
            }

            float chance = level >= 3 ? settings.TwoStarChance : level == 2 ? settings.OneStarChance : 0f;
            return chance > 0f && random() < chance ? 1 : 0;
        }
    }
}
