using BepInEx.Configuration;
using Jotunn.Configs;

namespace Corruption
{
    internal static class CorruptionConfig
    {
        private const string ShardSection = "1 - Corruption Shards";

        public static ConfigEntry<float> OneStarShardChance;
        public static ConfigEntry<float> TwoStarShardChance;
        public static ConfigEntry<int> BossShardsMin;
        public static ConfigEntry<int> BossShardsMax;
        public static ConfigEntry<bool> BossShardsFirstKillOnly;

        public static void Bind(ConfigFile config)
        {
            OneStarShardChance = config.Bind(ShardSection, "OneStarChance", 0.02f,
                AdminOnly("Chance (0-1) that a 1-star creature drops a corruption shard.", new AcceptableValueRange<float>(0f, 1f)));
            TwoStarShardChance = config.Bind(ShardSection, "TwoStarChance", 0.05f,
                AdminOnly("Chance (0-1) that a 2-star (or higher) creature drops a corruption shard.", new AcceptableValueRange<float>(0f, 1f)));
            BossShardsMin = config.Bind(ShardSection, "BossShardsMin", 1,
                AdminOnly("Fewest shards a boss drops.", new AcceptableValueRange<int>(0, 20)));
            BossShardsMax = config.Bind(ShardSection, "BossShardsMax", 3,
                AdminOnly("Most shards a boss drops.", new AcceptableValueRange<int>(0, 20)));
            BossShardsFirstKillOnly = config.Bind(ShardSection, "BossShardsFirstKillOnly", true,
                AdminOnly("Bosses only drop shards the first time each is defeated in a world. Off: every kill."));
        }

        private static ConfigDescription AdminOnly(string description, AcceptableValueBase range = null)
        {
            // Synced from the server, so the host decides drop rates for everyone.
            return new ConfigDescription(description, range, new ConfigurationManagerAttributes { IsAdminOnly = true });
        }
    }
}
