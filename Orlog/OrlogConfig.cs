using BepInEx.Configuration;
using Jotunn.Configs;

namespace Orlog
{
    internal static class OrlogConfig
    {
        private const string LevelSection = "1 - Levels";
        private const string DisplaySection = "2 - Display";
        private const string AttributeSection = "3 - Attributes";

        public static ConfigEntry<int> MaxLevel;
        public static ConfigEntry<int> StatPointsPerLevel;
        public static ConfigEntry<int> SkillPointsPerLevel;
        public static ConfigEntry<float> XpMultiplier;
        public static ConfigEntry<float> ShareRange;
        public static ConfigEntry<float> PartyBonusPerPlayer;
        public static ConfigEntry<bool> HighLevelPenalty;

        public static ConfigEntry<bool> ShowXpGains;

        public static ConfigEntry<float> HealthPerD2Life;
        public static ConfigEntry<float> StaminaPerD2Stamina;
        public static ConfigEntry<float> EitrStart;
        public static ConfigEntry<float> EitrPerLevel;
        public static ConfigEntry<float> EitrPerEnergy;
        public static ConfigEntry<float> DamagePerStrength;
        public static ConfigEntry<float> DamagePerDexterity;

        public static void Bind(ConfigFile config)
        {
            // Diablo 2's rules by default (user, 2026-10-30), every one changeable.
            MaxLevel = config.Bind(LevelSection, "MaxLevel", 99,
                AdminOnly("Highest character level (Diablo 2: 99). XP stops at this level.", new AcceptableValueRange<int>(1, 99)));
            StatPointsPerLevel = config.Bind(LevelSection, "StatPointsPerLevel", 5,
                AdminOnly("Attribute points gained each level (Diablo 2: 5).", new AcceptableValueRange<int>(0, 20)));
            SkillPointsPerLevel = config.Bind(LevelSection, "SkillPointsPerLevel", 1,
                AdminOnly("Skill points gained each level (Diablo 2: 1).", new AcceptableValueRange<int>(0, 10)));
            XpMultiplier = config.Bind(LevelSection, "XpMultiplier", 1f,
                AdminOnly("Multiplies all XP from kills. 1 = the pace of classic WoW without quests (Orlog/Data/creature_xp.csv). " +
                          "2 = levels twice as fast.", new AcceptableValueRange<float>(0f, 100f)));
            ShareRange = config.Bind(LevelSection, "ShareRange", 70f,
                AdminOnly("Players within this many metres of a kill share its XP, as a Diablo 2 party (70 = EpicMMOSystem's group range).",
                    new AcceptableValueRange<float>(0f, 500f)));
            PartyBonusPerPlayer = config.Bind(LevelSection, "PartyBonusPerPlayer", 0.35f,
                AdminOnly("Extra XP per additional player sharing a kill, before it is split by level (Diablo 2: 0.35).",
                    new AcceptableValueRange<float>(0f, 2f)));
            HighLevelPenalty = config.Bind(LevelSection, "HighLevelPenalty", false,
                AdminOnly("Diablo 2's XP cut from level 70 (95% at 70, down to 0.5% at 99). Off by default: with it, level 90 by " +
                          "Kall Fimbulbringer (the levelling guide) is out of reach."));

            // Diablo 2's gains per level and per point (Orlog/Data/classes.csv) turned into Valheim numbers.
            HealthPerD2Life = config.Bind(AttributeSection, "HealthPerD2Life", 0.5f,
                AdminOnly("Valheim health per Diablo 2 life point (life per level and per Vitality point). 0.5 = Valheim's starting " +
                          "health (25) / D2's starting life (Amazon 50).", new AcceptableValueRange<float>(0f, 10f)));
            StaminaPerD2Stamina = config.Bind(AttributeSection, "StaminaPerD2Stamina", 0.89f,
                AdminOnly("Valheim stamina per Diablo 2 stamina point. 0.89 = Valheim's starting stamina (75) / D2's (Amazon 84).",
                    new AcceptableValueRange<float>(0f, 10f)));
            EitrStart = config.Bind(AttributeSection, "EitrStart", 15f,
                AdminOnly("Eitr every class starts with, before food (user: 15).", new AcceptableValueRange<float>(0f, 1000f)));
            EitrPerLevel = config.Bind(AttributeSection, "EitrPerLevel", 1.5f,
                AdminOnly("Eitr gained per level (user: 1.5).", new AcceptableValueRange<float>(0f, 100f)));
            EitrPerEnergy = config.Bind(AttributeSection, "EitrPerEnergy", 1.5f,
                AdminOnly("Eitr per Energy point (user: 1.5).", new AcceptableValueRange<float>(0f, 100f)));
            DamagePerStrength = config.Bind(AttributeSection, "DamagePerStrength", 1f,
                AdminOnly("% more physical damage per Strength point, melee and thrown (Diablo 2: 1%).", new AcceptableValueRange<float>(0f, 10f)));
            DamagePerDexterity = config.Bind(AttributeSection, "DamagePerDexterity", 1f,
                AdminOnly("% more physical damage per Dexterity point, bows and crossbows (Diablo 2: 1%).", new AcceptableValueRange<float>(0f, 10f)));

            // Any change (a synced value, a reload on save) refreshes the bonuses.
            config.SettingChanged += (sender, args) => Content.Progress.NotifyChanged();

            // Per player, not synced.
            ShowXpGains = config.Bind(DisplaySection, "ShowXpGains", true, "Show \"+N XP\" in the top-left corner for each kill.");
        }

        private static ConfigDescription AdminOnly(string description, AcceptableValueBase range = null)
        {
            // Synced from the server, so the host decides the pace for everyone.
            return new ConfigDescription(description, range, new ConfigurationManagerAttributes { IsAdminOnly = true });
        }
    }
}
