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

        /// <summary>
        ///     The shards every boss kill adds for the players near it (Krogon, 2026-11-23, decisions 21 and 30): this many
        ///     per player, on top of <see cref="ShardsFor" />'s first-kill shards and a corrupted boss's own; at least one
        ///     player counts (the killer may be out of range of the game running the boss).
        /// </summary>
        public static int BossPerPlayer(int playersNearby, int perPlayer)
        {
            return perPlayer <= 0 ? 0 : perPlayer * Math.Max(1, playersNearby);
        }

        /// <summary>A vanilla chest with its own loot: 1 shard on a roll of <paramref name="chance" /> (0–1), else 0 (decision 33: 0.05).</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static int ChestShards(float chance, Func<double> random)
        {
            return chance > 0f && random() < chance ? 1 : 0;
        }
    }
}
