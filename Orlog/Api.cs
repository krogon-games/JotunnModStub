using System.Globalization;
using System.Text;
using Orlog.Content;
using UnityEngine;

namespace Orlog
{
    /// <summary>
    ///     The contract with CraftingOverhaul's UI (<c>docs/classes-ui-brief.md</c>, version 1), read by reflection. Works on
    ///     the local player; JSON answers are flat objects or string arrays; "" means no data. Levels and XP are built;
    ///     classes, attributes and skills answer "no data" until they are.
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
            var json = new StringBuilder("{");
            Field(json, "class", "");
            Field(json, "level", level);
            Field(json, "maxLevel", Progress.MaxLevel);
            Field(json, "xp", xp);
            Field(json, "xpLevelStart", start);
            Field(json, "xpNextLevel", next);
            Field(json, "statPoints", (long)(level - 1) * OrlogConfig.StatPointsPerLevel.Value);
            Field(json, "skillPoints", (long)(level - 1) * OrlogConfig.SkillPointsPerLevel.Value);
            Field(json, "strength", 0);
            Field(json, "dexterity", 0);
            Field(json, "vitality", 0);
            Field(json, "energy", 0);
            json.Length--;
            return json.Append('}').ToString();
        }

        public static string GetAttributeInfoJson(string attribute)
        {
            return "";
        }

        public static string GetClassIdsJson()
        {
            return "[]";
        }

        public static string GetClassInfoJson(string classId)
        {
            return "";
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
            return NotYet;
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
