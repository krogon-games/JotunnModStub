using BepInEx.Configuration;
using Jotunn.Configs;

namespace Orlog
{
    internal static class OrlogConfig
    {
        private const string LevelSection = "1 - Levels";
        private const string DisplaySection = "2 - Display";
        private const string AttributeSection = "3 - Attributes";
        private const string StoneSection = "4 - Orlog Stone";
        private const string SkillSection = "5 - Skills";
        private const string KeySection = "6 - Skill Keys";

        public static ConfigEntry<int> MaxLevel;
        public static ConfigEntry<int> StatPointsPerLevel;
        public static ConfigEntry<int> SkillPointsPerLevel;
        public static ConfigEntry<float> XpMultiplier;
        public static ConfigEntry<float> ShareRange;
        public static ConfigEntry<float> PartyBonusPerPlayer;
        public static ConfigEntry<bool> HighLevelPenalty;
        public static ConfigEntry<int> MaxSkillPoints;
        public static ConfigEntry<bool> SkillPace;

        public static ConfigEntry<bool> ShowXpGains;

        public static ConfigEntry<float> HealthPerD2Life;
        public static ConfigEntry<float> StaminaPerD2Stamina;
        public static ConfigEntry<float> EitrStart;
        public static ConfigEntry<float> EitrPerLevel;
        public static ConfigEntry<float> EitrPerEnergy;
        public static ConfigEntry<float> DamagePerStrength;
        public static ConfigEntry<float> DamagePerDexterity;

        public static ConfigEntry<float> SkillDamageMultiplier;
        public static ConfigEntry<float> InnerSightDamageTaken;

        public static ConfigEntry<UnityEngine.KeyCode> SkillButton;
        public static ConfigEntry<UnityEngine.KeyCode> BlockKey;
        public static ConfigEntry<UnityEngine.KeyCode>[] SlotKeys = new ConfigEntry<UnityEngine.KeyCode>[4];

        public static ConfigEntry<float> StoneSize;
        public static ConfigEntry<float> StoneGap;
        public static ConfigEntry<float> StoneTurn;

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

            MaxSkillPoints = config.Bind(LevelSection, "MaxSkillPoints", 20,
                AdminOnly("Most points one skill can take (Diablo 2: 20).", new AcceptableValueRange<int>(1, 99)));
            SkillPace = config.Bind(LevelSection, "SkillPace", true,
                AdminOnly("Diablo 2's pace: a skill takes one more point per character level past its required level."));

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

            // The Orlog stone (user, 2026-11-02): behind the Eikthyr trophy stone, twice as tall, 3 m between them.
            // 1.7: the Vegvisir is 1.85 m; at 15.5 m the user saw about 10 trophy stones, so one reads as about 1.55 m,
            // and twice that is 3.1 m = 1.7 x 1.85 m (play-test 2026-11-02).
            StoneSize = config.Bind(StoneSection, "StoneSize", 1.7f,
                AdminOnly("The Orlog stone's size, in times the Vegvisir it copies (1.85 m tall). 1.7 = about twice a boss trophy " +
                          "stone. Restart the game to apply.",
                    new AcceptableValueRange<float>(0.5f, 6f)));
            StoneGap = config.Bind(StoneSection, "StoneGap", 3f,
                AdminOnly("Metres between the Eikthyr trophy stone and the Orlog stone behind it. The stone moves the next time a player " +
                          "comes to the spawn area.", new AcceptableValueRange<float>(0f, 30f)));
            StoneTurn = config.Bind(StoneSection, "StoneTurn", 0f,
                AdminOnly("Degrees added to the stone's turn (it copies the Eikthyr stone's turn). 180 turns it round if it faces the wrong way.",
                    new AcceptableValueRange<float>(-180f, 180f)));

            // Skills (roadmap C5). Damage: D2's numbers × Orlog/Data/skill_damage_scale.csv × this.
            SkillDamageMultiplier = config.Bind(SkillSection, "SkillDamageMultiplier", 1f,
                AdminOnly("Multiplies the damage skills add (fire, frost, explosions, Magic Arrow). 1 = D2's ratio of skill damage to bow " +
                          "damage (Orlog/Data/skill_damage_scale.csv). Proposed 2026-11-11, waiting for the user's approval.",
                    new AcceptableValueRange<float>(0f, 10f)));
            InnerSightDamageTaken = config.Bind(SkillSection, "InnerSightDamageTaken", 25f,
                AdminOnly("% more damage enemies under Inner Sight take. 25 = the game's own 'slightly weak' step (Claude's interim " +
                          "choice, 2026-11-11: D2 lowers defence instead, which Valheim lacks).", new AcceptableValueRange<float>(0f, 200f)));

            // Keys, per player (class-design.md §4; user 2026-10-30: right click is the skill button, block moves, both rebindable).
            SkillButton = config.Bind(KeySection, "SkillButton", UnityEngine.KeyCode.Mouse1,
                "Fires the chosen attack skill (right click, as in Diablo 2). Only for characters with a class.");
            // Blank by default for now (user, 2026-11-13: "i cant think of a good alternative at the moment").
            BlockKey = config.Bind(KeySection, "BlockKey", UnityEngine.KeyCode.None,
                "Block and parry for characters with a class, while the game's Block key is the same as the skill button (right click). " +
                "Blank (None) by default: set a key here (e.g. Mouse4, the mouse's back side button; Valheim's dodge is block + jump), " +
                "or set Block to another key in the game's own controls, which then blocks too.");
            UnityEngine.KeyCode[] slotDefaults = { UnityEngine.KeyCode.F1, UnityEngine.KeyCode.F2, UnityEngine.KeyCode.F3, UnityEngine.KeyCode.F4 };
            for (int i = 0; i < SlotKeys.Length; i++)
            {
                // F1-F4: Diablo 2's skill keys; unused by Valheim alone (Ctrl+F1, Ctrl+F3 are) and by the group's mods.
                SlotKeys[i] = config.Bind(KeySection, "Slot" + (i + 1) + "Key", slotDefaults[i],
                    "Skill slot " + (i + 1) + ": an attack skill goes on the skill button, a cast skill casts.");
            }

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
