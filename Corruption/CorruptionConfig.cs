using BepInEx.Configuration;
using Jotunn.Configs;

namespace Corruption
{
    internal static class CorruptionConfig
    {
        private const string ShardSection = "1 - Corruption Shards";
        private const string LookSection = "2 - Look";
        private const string TalismanSection = "3 - Talismans";
        private const string EssenceSection = "4 - Essences";
        private const string RipSection = "5 - Rips";
        private const string CorruptedSection = "6 - Corrupted Bosses";

        public static ConfigEntry<float> OneStarShardChance;
        public static ConfigEntry<float> TwoStarShardChance;
        public static ConfigEntry<int> BossShardsMin;
        public static ConfigEntry<int> BossShardsMax;
        public static ConfigEntry<bool> BossShardsFirstKillOnly;

        public static ConfigEntry<bool> TalismansEnabled;
        public static ConfigEntry<bool> EssencesEnabled;
        public static ConfigEntry<bool> RipsEnabled;
        public static ConfigEntry<float> RipOpenDays;
        public static ConfigEntry<float> RipBaseDistance;
        public static ConfigEntry<float> RipHeartHealth;
        public static ConfigEntry<float> CorruptedDamageDealt;
        public static ConfigEntry<float> CorruptedDamageTaken;
        public static ConfigEntry<int> CorruptedEssencesMin;
        public static ConfigEntry<int> CorruptedEssencesMax;
        public static ConfigEntry<int> CorruptedShardsMin;
        public static ConfigEntry<int> CorruptedShardsMax;
        public static ConfigEntry<float> CorruptedTintStrength;
        public static ConfigEntry<int> TalismanSlots;
        public static ConfigEntry<float> ProvokeSeconds;
        public static ConfigEntry<string> TalismanExcludedCreatures;

        public static ConfigEntry<string> ShardBasePrefab;
        public static ConfigEntry<string> ShardColor;
        public static ConfigEntry<float> ShardTintStrength;
        public static ConfigEntry<bool> ShardCustomModel;
        public static ConfigEntry<float> ShardGlowIntensity;
        public static ConfigEntry<float> ShardGlowRange;
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

            TalismansEnabled = config.Bind(TalismanSection, "Enabled", true,
                AdminOnly("Trophy talismans repel their creature family. Off: talismans can still be crafted but do nothing."));
            TalismanSlots = config.Bind(TalismanSection, "Slots", 2,
                AdminOnly("How many talisman slots work (needs EquipmentAndQuickSlots, which shows them in its equipment panel). " +
                          "The vanilla trinket slot stays free for a vanilla trinket.", new AcceptableValueRange<int>(1, Content.TalismanSlots.MaxSlots)));
            ProvokeSeconds = config.Bind(TalismanSection, "ProvokeSeconds", 30f,
                AdminOnly("After the wearer hits a repelled creature, that creature fights back for this many seconds after the last hit.",
                    new AcceptableValueRange<float>(0f, 600f)));
            TalismanExcludedCreatures = config.Bind(TalismanSection, "ExcludedCreatures", CorruptionDefaults.ExcludedCreatures,
                AdminOnly("Creature prefab names talismans never repel (quest and named minibosses). '*' matches any text. Restart to apply."));

            EssencesEnabled = config.Bind(EssenceSection, "Enabled", true,
                AdminOnly("Bosses drop their essence on every kill (how many per kill: Corruption/Data/essences.csv). " +
                          "Off: no essence drops; essences already found stay."));

            RipsEnabled = config.Bind(RipSection, "Enabled", true,
                AdminOnly("Maps open rips. Off: using a map does nothing (the map is kept)."));
            RipOpenDays = config.Bind(RipSection, "OpenDays", 1f,
                AdminOnly("How long a rip stays open, in in-game days (sleeping skips time). Both its portals close then.",
                    new AcceptableValueRange<float>(0.1f, 10f)));
            RipBaseDistance = config.Bind(RipSection, "BaseDistance", 0f,
                AdminOnly("Rips never open within this many metres of a bed, ward or portal (any player's). " +
                          "0 = the game's ward radius (the area a ward protects).", new AcceptableValueRange<float>(0f, 1000f)));
            RipHeartHealth = config.Bind(RipSection, "HeartHealth", 0f,
                AdminOnly("Health of a rip's heart. Destroying it opens the way home. " +
                          "0 = the health of the greydwarf nest it's made from.", new AcceptableValueRange<float>(0f, 100000f)));

            // Starting values from the design doc (corruption-design.md -> Corrupted bosses).
            CorruptedDamageDealt = config.Bind(CorruptedSection, "DamageDealt", 1.5f,
                AdminOnly("Damage multiplier on a corrupted boss's attacks.", new AcceptableValueRange<float>(0.1f, 10f)));
            CorruptedDamageTaken = config.Bind(CorruptedSection, "DamageTaken", 0.7f,
                AdminOnly("Damage multiplier on hits a corrupted boss takes (0.7 = 30% less).", new AcceptableValueRange<float>(0.1f, 10f)));
            CorruptedEssencesMin = config.Bind(CorruptedSection, "EssencesMin", 2,
                AdminOnly("Fewest essences a corrupted boss drops (instead of the normal per-kill amount).", new AcceptableValueRange<int>(0, 20)));
            CorruptedEssencesMax = config.Bind(CorruptedSection, "EssencesMax", 3,
                AdminOnly("Most essences a corrupted boss drops.", new AcceptableValueRange<int>(0, 20)));
            CorruptedShardsMin = config.Bind(CorruptedSection, "ShardsMin", 3,
                AdminOnly("Fewest corruption shards a corrupted boss drops (every kill).", new AcceptableValueRange<int>(0, 50)));
            CorruptedShardsMax = config.Bind(CorruptedSection, "ShardsMax", 5,
                AdminOnly("Most corruption shards a corrupted boss drops.", new AcceptableValueRange<int>(0, 50)));

            // Visual only and per player, so not synced. Applied at startup: restart the game after changing.
            ShardBasePrefab = config.Bind(LookSection, "ShardBaseItem", "GemstoneRed",
                "Vanilla item whose model the shard uses, e.g. GemstoneRed (Bloodstone), Crystal, Ruby. Restart to apply.");
            ShardColor = config.Bind(LookSection, "ShardColor", "#B3170D",
                "Shard colour as an HTML colour, e.g. #B3170D (deep red). Restart to apply.");
            ShardTintStrength = config.Bind(LookSection, "ShardTintStrength", 0.35f,
                new ConfigDescription("How strongly the colour is applied: 0 = base item's own colours, 1 = fully the shard colour. Restart to apply.",
                    new AcceptableValueRange<float>(0f, 1f)));
            ShardCustomModel = config.Bind(LookSection, "ShardCustomModel", true,
                "Use the shard's own crystal model and texture. Off: the ShardBaseItem's model with the tint below. Restart to apply.");
            ShardGlowIntensity = config.Bind(LookSection, "ShardGlowIntensity", 2f,
                new ConfigDescription("Brightness of the glow a dropped shard or essence casts on the ground, so it's easy to spot. 0 = no glow. Restart to apply.",
                    new AcceptableValueRange<float>(0f, 8f)));
            ShardGlowRange = config.Bind(LookSection, "ShardGlowRange", 2.5f,
                new ConfigDescription("How far the shard and essence glow reaches, in metres. Restart to apply.",
                    new AcceptableValueRange<float>(0.5f, 10f)));
            RiftforgeColor = config.Bind(LookSection, "RiftforgeColor", "#B3170D",
                "Riftforge colour as an HTML colour. Restart to apply.");
            CorruptedTintStrength = config.Bind(LookSection, "CorruptedBossTintStrength", 0.6f,
                new ConfigDescription("How strongly corrupted bosses are tinted red (0-1). Default: the Riftforge's, which was tested in-game.",
                    new AcceptableValueRange<float>(0f, 1f)));
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
