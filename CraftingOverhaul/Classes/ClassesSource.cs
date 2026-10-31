using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using CraftingOverhaul.CharacterSheet;
using Jotunn.Entities;
using UnityEngine;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Where the classes UI gets its data (<c>docs/classes-ui-brief.md</c> §3): the Orlog plugin's <c>Orlog.Api</c>,
    ///     found by reflection like the character sheet finds EquipmentAndQuickSlots, or the built-in sample data
    ///     (<see cref="DemoClasses" />) when the <c>Demo</c> setting is on. Without either, <see cref="Available" /> is
    ///     false and the class screens stay hidden; the health, eitr and stamina bars don't need it.
    ///
    ///     Every answer is a JSON string read with <see cref="FlatJson" />; "" means no data.
    /// </summary>
    internal static class ClassesSource
    {
        public const string OrlogGuid = "com.krogongames.orlog";
        public const int KnownApiVersion = 1;

        private static bool _lookedUp;
        private static Func<int> _changeCounter;
        private static Func<int> _stoneRequest;
        private static Func<string> _character;
        private static Func<string, string> _attributeInfo;
        private static Func<string> _classIds;
        private static Func<string, string> _classInfo;
        private static Func<string, Sprite> _classIcon;
        private static Func<string, string> _skillIds;
        private static Func<string, string> _skillInfo;
        private static Func<string, Sprite> _skillIcon;
        private static Func<string> _skillBar;
        private static Func<int, string> _slotState;
        private static Func<string, string> _spendStat;
        private static Func<string, string> _spendSkill;
        private static Func<int, string, string> _setSlot;
        private static Func<string, string> _chooseClass;

        private static DemoClasses _demo;
        private static readonly Dictionary<string, Sprite> DemoIcons = new Dictionary<string, Sprite>();

        /// <summary>The demo replaces Orlog's answers while it's on, so the screens can be tried before classes exist.</summary>
        public static bool Demo => ModConfig.ClassesDemo.Value;

        public static bool HasOrlog
        {
            get
            {
                LookUp();
                return _character != null;
            }
        }

        /// <summary>Levels and XP can be shown (Orlog installed, or the demo).</summary>
        public static bool Available => Demo || HasOrlog;

        public static DemoClasses DemoData => _demo ?? (_demo = new DemoClasses());

        private static void LookUp()
        {
            if (_lookedUp)
            {
                return;
            }
            _lookedUp = true;
            if (!Chainloader.PluginInfos.TryGetValue(OrlogGuid, out var info) || !info.Instance)
            {
                Jotunn.Logger.LogInfo("Orlog isn't installed: the class screens are off (the health, eitr and stamina bars still show).");
                return;
            }
            var api = info.Instance.GetType().Assembly.GetType("Orlog.Api");
            var version = Method<Func<int>>(api, "GetApiVersion");
            int found = version != null ? version() : 0;
            if (found < 1 || found > KnownApiVersion)
            {
                Jotunn.Logger.LogWarning($"Orlog's API is version {found}; this CraftingOverhaul knows version {KnownApiVersion}. The class screens are off.");
                return;
            }
            _changeCounter = Method<Func<int>>(api, "GetChangeCounter");
            _stoneRequest = Method<Func<int>>(api, "GetClassStoneRequest");
            _attributeInfo = Method<Func<string, string>>(api, "GetAttributeInfoJson");
            _classIds = Method<Func<string>>(api, "GetClassIdsJson");
            _classInfo = Method<Func<string, string>>(api, "GetClassInfoJson");
            _classIcon = Method<Func<string, Sprite>>(api, "GetClassIcon");
            _skillIds = Method<Func<string, string>>(api, "GetSkillIdsJson");
            _skillInfo = Method<Func<string, string>>(api, "GetSkillInfoJson");
            _skillIcon = Method<Func<string, Sprite>>(api, "GetSkillIcon");
            _skillBar = Method<Func<string>>(api, "GetSkillBarJson");
            _slotState = Method<Func<int, string>>(api, "GetSlotStateJson");
            _spendStat = Method<Func<string, string>>(api, "SpendStatPoint");
            _spendSkill = Method<Func<string, string>>(api, "SpendSkillPoint");
            _setSlot = Method<Func<int, string, string>>(api, "SetSkillSlot");
            _chooseClass = Method<Func<string, string>>(api, "ChooseClass");
            // Set last: it's what HasOrlog checks.
            _character = Method<Func<string>>(api, "GetCharacterJson");
            Jotunn.Logger.LogInfo("Orlog's classes API (version 1) found: the class screens are on.");
        }

        private static T Method<T>(Type api, string name) where T : class
        {
            if (api == null)
            {
                return null;
            }
            var invoke = typeof(T).GetMethod("Invoke");
            var parameters = Array.ConvertAll(invoke.GetParameters(), p => p.ParameterType);
            var method = api.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, parameters, null);
            if (method == null || method.ReturnType != invoke.ReturnType)
            {
                Jotunn.Logger.LogWarning($"Orlog's API has no {name}; that part of the class screens stays empty.");
                return null;
            }
            return (T)(object)Delegate.CreateDelegate(typeof(T), method);
        }

        // ------------------------------------------------------------ the API, one call each (brief §3)

        public static int ChangeCounter => Demo ? DemoData.ChangeCounter : HasOrlog && _changeCounter != null ? _changeCounter() : 0;
        public static int StoneRequest => Demo ? DemoData.StoneRequest : HasOrlog && _stoneRequest != null ? _stoneRequest() : 0;
        public static string Character() => Demo ? DemoData.GetCharacterJson() : HasOrlog ? _character() : "";
        public static string AttributeInfo(string attribute) => Demo ? DemoData.GetAttributeInfoJson(attribute) : Call(_attributeInfo, attribute);
        public static string ClassIds() => Demo ? DemoData.GetClassIdsJson() : _classIds != null ? _classIds() ?? "" : "";
        public static string ClassInfo(string classId) => Demo ? DemoData.GetClassInfoJson(classId) : Call(_classInfo, classId);
        public static string SkillIds(string classId) => Demo ? DemoData.GetSkillIdsJson(classId) : Call(_skillIds, classId);
        public static string SkillInfo(string skillId) => Demo ? DemoData.GetSkillInfoJson(skillId) : Call(_skillInfo, skillId);
        public static string SkillBar() => Demo ? DemoData.GetSkillBarJson() : _skillBar != null ? _skillBar() ?? "" : "";
        public static string SlotState(int slot) =>
            Demo ? DemoData.GetSlotStateJson(slot, Time.time, HeldWeapon(), Player.m_localPlayer ? Player.m_localPlayer.GetEitr() : 0f)
            : _slotState != null ? _slotState(slot) ?? "" : "";

        /// <summary>For the demo's slot states: "bow" for a bow or crossbow in hand, "spear" for a spear, else "".</summary>
        private static string HeldWeapon()
        {
            var weapon = Player.m_localPlayer ? Player.m_localPlayer.GetCurrentWeapon() : null;
            var type = weapon?.m_shared?.m_skillType ?? Skills.SkillType.None;
            return type == Skills.SkillType.Bows || type == Skills.SkillType.Crossbows ? "bow" : type == Skills.SkillType.Spears ? "spear" : "";
        }
        public static string SpendStatPoint(string attribute) => Demo ? DemoData.SpendStatPoint(attribute) : Act(_spendStat, attribute);
        public static string SpendSkillPoint(string skillId) => Demo ? DemoData.SpendSkillPoint(skillId) : Act(_spendSkill, skillId);
        public static string ChooseClass(string classId) => Demo ? DemoData.ChooseClass(classId) : Act(_chooseClass, classId);

        public static string SetSkillSlot(int slot, string skillId)
        {
            if (Demo)
            {
                return DemoData.SetSkillSlot(slot, skillId);
            }
            return _setSlot != null ? _setSlot(slot, skillId ?? "") ?? "" : "Not available";
        }

        public static Sprite ClassIcon(string classId)
        {
            var icon = !Demo && _classIcon != null ? _classIcon(classId) : null;
            return icon ? icon : SheetArt.SkillIcon(classId) ?? (Demo ? ItemIcon("BowHuntsman") : null);
        }

        public static Sprite SkillIcon(string skillId)
        {
            // Orlog's icon first; else our placeholder set (tools/make_skill_icons.py); else, in the demo, a vanilla item.
            var icon = !Demo && _skillIcon != null ? _skillIcon(skillId) : null;
            if (icon)
            {
                return icon;
            }
            icon = SheetArt.SkillIcon(skillId);
            if (icon)
            {
                return icon;
            }
            return Demo && DemoSkillIcons.TryGetValue(skillId ?? "", out string prefab) ? ItemIcon(prefab) : null;
        }

        private static string Call(Func<string, string> f, string arg) => f != null ? f(arg) ?? "" : "";
        private static string Act(Func<string, string> f, string arg) => f != null ? f(arg) ?? "" : "Not available";

        // ------------------------------------------------------------ demo icons

        /// <summary>Placeholders until skill art is asked for (brief §5): a vanilla item that suits each skill.</summary>
        private static readonly Dictionary<string, string> DemoSkillIcons = new Dictionary<string, string>
        {
            { "MagicArrow", "ArrowWood" }, { "FireArrow", "ArrowFire" }, { "ColdArrow", "ArrowFrost" },
            { "MultipleShot", "ArrowFlint" }, { "ExplodingArrow", "BombLava" }, { "IceArrow", "ArrowSilver" },
            { "GuidedArrow", "ArrowNeedle" }, { "Strafe", "ArrowIron" }, { "ImmolationArrow", "ArrowCharred" },
            { "FreezingArrow", "ArrowObsidian" }, { "InnerSight", "Demister" }, { "CriticalStrike", "KnifeFlint" },
            { "Dodge", "CapeFeather" }, { "SlowMissiles", "Wishbone" }, { "Avoid", "ShieldWood" }, { "Penetrate", "ArrowCarapace" },
            { "Decoy", "TrophyGreydwarf" }, { "Evade", "ShieldBronzeBuckler" }, { "Valkyrie", "TrophyEikthyr" }, { "Pierce", "ArrowBronze" },
            { "Jab", "SpearFlint" }, { "PowerStrike", "SpearBronze" }, { "PoisonJavelin", "ArrowPoison" }, { "Impale", "SpearElderbark" },
            { "LightningBolt", "Thunderstone" }, { "ChargedStrike", "AtgeirBronze" }, { "PlagueJavelin", "BombOoze" },
            { "Fend", "SpearWolfFang" }, { "LightningStrike", "StaffLightning" }, { "LightningFury", "SpearCarapace" }
        };

        private static Sprite ItemIcon(string prefab)
        {
            if (DemoIcons.TryGetValue(prefab, out var cached) && cached)
            {
                return cached;
            }
            var go = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(prefab) : null;
            var drop = go ? go.GetComponent<ItemDrop>() : null;
            var icon = drop != null && drop.m_itemData != null ? drop.m_itemData.GetIcon() : null;
            if (icon)
            {
                DemoIcons[prefab] = icon;
            }
            return icon;
        }
    }

    /// <summary>
    ///     <c>co_classes_stone</c>: with the classes UI demo on, acts as if you used the Orlog stone (the demo character
    ///     loses its class and the class choice window opens).
    /// </summary>
    internal sealed class ClassesStoneCommand : ConsoleCommand
    {
        public override string Name => "co_classes_stone";

        public override string Help => "CraftingOverhaul: with the classes UI demo on, opens the class choice window as the Orlog stone would";

        public override void Run(string[] args)
        {
            if (!ClassesSource.Demo)
            {
                Console.instance.Print("Turn on '5 - Classes UI' > Demo first (com.krogongames.craftingoverhaul.cfg).");
                return;
            }
            ClassesSource.DemoData.UseStone();
            Console.instance.Print("Demo: the character has no class now; the class choice window opens.");
        }
    }
}
