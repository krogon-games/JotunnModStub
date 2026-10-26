using BepInEx.Configuration;
using Jotunn.Configs;

namespace Corruption
{
    internal static class CorruptionConfig
    {
        private const string ShardSection = "1 - Corruption Shards";
        private const string LookSection = "2 - Look";

        public static ConfigEntry<float> OneStarShardChance;
        public static ConfigEntry<float> TwoStarShardChance;
        public static ConfigEntry<int> BossShardsMin;
        public static ConfigEntry<int> BossShardsMax;
        public static ConfigEntry<bool> BossShardsFirstKillOnly;

        public static ConfigEntry<string> ShardBasePrefab;
        public static ConfigEntry<string> ShardColor;
        public static ConfigEntry<float> ShardTintStrength;
        public static ConfigEntry<string> RiftforgeColor;
        public static ConfigEntry<float> RiftforgeTintStrength;

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

            // Visual only and per player, so not synced. Applied at startup: restart the game after changing.
            ShardBasePrefab = config.Bind(LookSection, "ShardBaseItem", "GemstoneRed",
                "Vanilla item whose model the shard uses, e.g. GemstoneRed (Bloodstone), Crystal, Ruby. Restart to apply.");
            ShardColor = config.Bind(LookSection, "ShardColor", "#B3170D",
                "Shard colour as an HTML colour, e.g. #B3170D (deep red). Restart to apply.");
            ShardTintStrength = config.Bind(LookSection, "ShardTintStrength", 0.35f,
                new ConfigDescription("How strongly the colour is applied: 0 = base item's own colours, 1 = fully the shard colour. Restart to apply.",
                    new AcceptableValueRange<float>(0f, 1f)));
            RiftforgeColor = config.Bind(LookSection, "RiftforgeColor", "#B3170D",
                "Riftforge colour as an HTML colour. Restart to apply.");
            RiftforgeTintStrength = config.Bind(LookSection, "RiftforgeTintStrength", 0.6f,
                new ConfigDescription("How strongly the Riftforge colour is applied (0-1). Restart to apply.",
                    new AcceptableValueRange<float>(0f, 1f)));
        }

        private static ConfigDescription AdminOnly(string description, AcceptableValueBase range = null)
        {
            // Synced from the server, so the host decides drop rates for everyone.
            return new ConfigDescription(description, range, new ConfigurationManagerAttributes { IsAdminOnly = true });
        }
    }
}
