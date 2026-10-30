using BepInEx.Configuration;
using Jotunn.Configs;

namespace Orlog
{
    internal static class OrlogConfig
    {
        private const string LevelSection = "1 - Levels";
        private const string DisplaySection = "2 - Display";

        public static ConfigEntry<int> MaxLevel;
        public static ConfigEntry<int> StatPointsPerLevel;
        public static ConfigEntry<int> SkillPointsPerLevel;
        public static ConfigEntry<float> XpMultiplier;
        public static ConfigEntry<float> ShareRange;
        public static ConfigEntry<float> PartyBonusPerPlayer;
        public static ConfigEntry<bool> HighLevelPenalty;

        public static ConfigEntry<bool> ShowXpGains;

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
