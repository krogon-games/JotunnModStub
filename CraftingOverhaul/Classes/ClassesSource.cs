using System;
using System.Reflection;
using BepInEx.Bootstrap;
using CraftingOverhaul.CharacterSheet;
using UnityEngine;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Where the classes UI gets its data (<c>docs/classes-ui-brief.md</c> §3): the Orlog plugin's <c>Orlog.Api</c>,
    ///     found by reflection like the character sheet finds EquipmentAndQuickSlots. Without Orlog,
    ///     <see cref="Available" /> is false and the class screens stay hidden; the health, eitr and stamina bars don't need
    ///     it. (The built-in sample data, the "demo", was removed on 2026-11-13 once Orlog served everything; user.)
    ///
    ///     Every answer is a JSON string read with <see cref="FlatJson" />; "" means no data.
    /// </summary>
    internal static class ClassesSource
    {
        public const string OrlogGuid = "com.krogongames.orlog";
        public const int KnownApiVersion = 1;

        /// <summary>The skill slots on the cross and in the skills panel (brief §2).</summary>
        public const int Slots = 4;

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
        private static Func<int, string> _toggleQuick;
        private static Func<string, string> _chooseClass;

        public static bool HasOrlog
        {
            get
            {
                LookUp();
                return _character != null;
            }
        }

        /// <summary>Levels and XP can be shown (Orlog installed).</summary>
        public static bool Available => HasOrlog;

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
            // Added 2026-11-13 (quick cast per skill); an older Orlog without it just has no toggle.
            _toggleQuick = api?.GetMethod("ToggleQuickCast", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null) != null
                ? Method<Func<int, string>>(api, "ToggleQuickCast") : null;
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

        public static int ChangeCounter => HasOrlog && _changeCounter != null ? _changeCounter() : 0;
        public static int StoneRequest => HasOrlog && _stoneRequest != null ? _stoneRequest() : 0;
        public static string Character() => HasOrlog ? _character() : "";
        public static string AttributeInfo(string attribute) => Call(_attributeInfo, attribute);
        public static string ClassIds() => _classIds != null ? _classIds() ?? "" : "";
        public static string ClassInfo(string classId) => Call(_classInfo, classId);
        public static string SkillIds(string classId) => Call(_skillIds, classId);
        public static string SkillInfo(string skillId) => Call(_skillInfo, skillId);
        public static string SkillBar() => _skillBar != null ? _skillBar() ?? "" : "";
        public static string SlotState(int slot) => _slotState != null ? _slotState(slot) ?? "" : "";
        public static string SpendStatPoint(string attribute) => Act(_spendStat, attribute);
        public static string SpendSkillPoint(string skillId) => Act(_spendSkill, skillId);
        public static string ChooseClass(string classId) => Act(_chooseClass, classId);

        public static string SetSkillSlot(int slot, string skillId)
        {
            return _setSlot != null ? _setSlot(slot, skillId ?? "") ?? "" : "Not available";
        }

        /// <summary>Right click on a skill cross slot: quick cast on or off for its skill. "" or why not.</summary>
        public static string ToggleQuickCast(int slot)
        {
            return _toggleQuick != null ? _toggleQuick(slot) ?? "" : "Not available";
        }

        public static Sprite ClassIcon(string classId)
        {
            var icon = _classIcon != null ? _classIcon(classId) : null;
            return icon ? icon : SheetArt.SkillIcon(classId);
        }

        /// <summary>Orlog's icon first; else our placeholder set (tools/make_skill_icons.py).</summary>
        public static Sprite SkillIcon(string skillId)
        {
            var icon = _skillIcon != null ? _skillIcon(skillId) : null;
            return icon ? icon : SheetArt.SkillIcon(skillId);
        }

        private static string Call(Func<string, string> f, string arg) => f != null ? f(arg) ?? "" : "";
        private static string Act(Func<string, string> f, string arg) => f != null ? f(arg) ?? "" : "Not available";
    }
}
