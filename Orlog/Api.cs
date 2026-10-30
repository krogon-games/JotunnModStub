using System.Globalization;
using System.Text;
using Orlog.Content;
using Orlog.Rules;
using UnityEngine;

namespace Orlog
{
    /// <summary>
    ///     The contract with CraftingOverhaul's UI (<c>docs/classes-ui-brief.md</c>, version 1), read by reflection. Works on
    ///     the local player; JSON answers are flat objects or string arrays; "" means no data. Levels, XP, attributes and
    ///     the class (set with <c>orlog_class</c> until the Orlog stone exists) are built; skills answer "no data" until they are.
    /// </summary>
    public static class Api
    {
        private const string NotYet = "Not available yet";

        public static int GetApiVersion()
        {
            return 1;
        }

        public static int GetChangeCounter()
        {
            return Progress.ChangeCounter;
        }

        public static int GetClassStoneRequest()
        {
            return 0;
        }

        public static string GetCharacterJson()
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                return "";
            }
            int level = Progress.GetLevel(player);
            long xp = Progress.GetXp(player);
            long start = Progress.Levels.TotalFor(level);
            long next = start + Progress.Levels.ToNext(level, Progress.MaxLevel);
            var cls = Attributes.ClassOf(player);
            var json = new StringBuilder("{");
            Field(json, "class", cls != null ? cls.Name : "");
            Field(json, "level", level);
            Field(json, "maxLevel", Progress.MaxLevel);
            Field(json, "xp", xp);
            Field(json, "xpLevelStart", start);
            Field(json, "xpNextLevel", next);
            Field(json, "statPoints", cls != null ? Attributes.Unspent(player) : 0);
            Field(json, "skillPoints", cls != null ? AttributeRules.Earned(level, OrlogConfig.SkillPointsPerLevel.Value) : 0);
            Field(json, "strength", Attributes.Value(player, Attribute.Strength));
            Field(json, "dexterity", Attributes.Value(player, Attribute.Dexterity));
            Field(json, "vitality", Attributes.Value(player, Attribute.Vitality));
            Field(json, "energy", Attributes.Value(player, Attribute.Energy));
            json.Length--;
            return json.Append('}').ToString();
        }

        public static string GetAttributeInfoJson(string attribute)
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready || Attributes.ClassOf(player) == null || !Attributes.TryParse(attribute, out var which))
            {
                return "";
            }
            var cls = Attributes.ClassOf(player);
            var scales = Attributes.Scales();
            int spent = Attributes.Spent(player)[(int)which];
            string effect;
            switch (which)
            {
                case Attribute.Strength:
                    effect = $"+{scales.DamagePerStrength * 100f * spent:0.#}% melee and thrown damage";
                    break;
                case Attribute.Dexterity:
                    effect = $"+{scales.DamagePerDexterity * 100f * spent:0.#}% bow and crossbow damage";
                    break;
                case Attribute.Vitality:
                    effect = $"+{scales.Health * cls.LifePerVitality * spent:0.#} health, +{scales.Stamina * cls.StaminaPerVitality * spent:0.#} stamina";
                    break;
                default:
                    effect = $"+{scales.EitrPerEnergy * spent:0.#} eitr";
                    break;
            }
            var json = new StringBuilder("{");
            Field(json, "name", which.ToString());
            Field(json, "value", Attributes.Value(player, which));
            Field(json, "effect", effect);
            json.Length--;
            return json.Append('}').ToString();
        }

        public static string GetClassIdsJson()
        {
            if (!Progress.Ready)
            {
                return "[]";
            }
            var json = new StringBuilder("[");
            foreach (var cls in Progress.Classes.Playable)
            {
                json.Append('"').Append(cls.Name).Append("\",");
            }
            if (json.Length > 1)
            {
                json.Length--;
            }
            return json.Append(']').ToString();
        }

        public static string GetClassInfoJson(string classId)
        {
            var cls = Progress.Ready ? Progress.Classes.Find(classId) : null;
            if (cls == null)
            {
                return "";
            }
            var json = new StringBuilder("{");
            Field(json, "id", cls.Name);
            Field(json, "name", cls.Name);
            json.Length--;
            return json.Append('}').ToString();
        }

        public static Sprite GetClassIcon(string classId)
        {
            return null;
        }

        public static string GetSkillIdsJson(string classId)
        {
            return "[]";
        }

        public static string GetSkillInfoJson(string skillId)
        {
            return "";
        }

        public static Sprite GetSkillIcon(string skillId)
        {
            return null;
        }

        public static string GetSkillBarJson()
        {
            return "";
        }

        public static string GetSlotStateJson(int slot)
        {
            return "";
        }

        public static string SpendStatPoint(string attribute)
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                return "No character";
            }
            return Attributes.TryParse(attribute, out var which) ? Attributes.Spend(player, which, 1) : "Unknown attribute";
        }

        public static string SpendSkillPoint(string skillId)
        {
            return NotYet;
        }

        public static string SetSkillSlot(int slot, string skillId)
        {
            return NotYet;
        }

        public static string ChooseClass(string classId)
        {
            return NotYet;
        }

        private static void Field(StringBuilder json, string name, string value)
        {
            json.Append('"').Append(name).Append("\":\"").Append(value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\",");
        }

        private static void Field(StringBuilder json, string name, long value)
        {
            json.Append('"').Append(name).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture)).Append(',');
        }
    }
}
