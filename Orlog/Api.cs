using System.Globalization;
using System.Text;
using Orlog.Content;
using Orlog.Rules;
using UnityEngine;

namespace Orlog
{
    /// <summary>
    ///     The contract with CraftingOverhaul's UI (<c>docs/classes-ui-brief.md</c>, version 1), read by reflection. Works on
    ///     the local player; JSON answers are flat objects or string arrays; "" means no data. Levels, XP, attributes,
    ///     the class, the Orlog stone and the skill tree (spending) are built; the skill bar answers "no data" until the
    ///     skills work (roadmap C5).
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
            return OrlogStone.Requests;
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
            Field(json, "skillPoints", cls != null ? SkillPoints.Unspent(player) : 0);
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
            var table = Progress.Ready ? SkillPoints.TableFor(Progress.Classes.Find(classId)) : null;
            if (table == null)
            {
                return "[]";
            }
            var json = new StringBuilder("[");
            foreach (var skill in table.Skills)
            {
                json.Append('"').Append(skill.Id).Append("\",");
            }
            if (json.Length > 1)
            {
                json.Length--;
            }
            return json.Append(']').ToString();
        }

        public static string GetSkillInfoJson(string skillId)
        {
            var player = Player.m_localPlayer;
            var table = player && Progress.Ready ? SkillPoints.TableOf(player) : null;
            var skill = table?.Find(skillId);
            if (skill == null)
            {
                return "";
            }
            int points = SkillRules.PointsIn(SkillPoints.Points(player), skill.Id);
            int max = OrlogConfig.MaxSkillPoints.Value;
            string blocked = SkillPoints.SpendBlocked(player, table, skill);
            var json = new StringBuilder("{");
            Field(json, "id", skill.Id);
            Field(json, "name", skill.Name);
            Field(json, "tab", skill.Tab);
            Field(json, "row", skill.Row);
            Field(json, "col", skill.Col);
            Field(json, "reqLevel", skill.ReqLevel);
            Field(json, "prereqs", string.Join("+", skill.Prereqs));
            Field(json, "kind", skill.Kind);
            Field(json, "status", skill.Status);
            Field(json, "maxPoints", max);
            Field(json, "points", points);
            // No +skills yet (items, Battle Command): the effective level is the hard points.
            Field(json, "level", points);
            Field(json, "canSpend", blocked.Length == 0);
            Field(json, "spendBlocked", blocked);
            Field(json, "description", (points > 0 ? "" : "Not learned yet. ") + skill.Description);
            Field(json, "nextDescription", points < max ? "Level " + (points + 1).ToString(CultureInfo.InvariantCulture) + ": " + skill.Description : "");
            Field(json, "eitrCost", SkillRules.EitrCost(skill, points));
            json.Length--;
            return json.Append('}').ToString();
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
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                return "No character";
            }
            return Safe.Try("spend a skill point", () => SkillPoints.Spend(player, skillId), "Something went wrong");
        }

        public static string SetSkillSlot(int slot, string skillId)
        {
            return NotYet;
        }

        public static string ChooseClass(string classId)
        {
            return Safe.Try("choose a class", () => OrlogStone.Choose(classId), "Something went wrong");
        }

        private static void Field(StringBuilder json, string name, string value)
        {
            json.Append('"').Append(name).Append("\":\"").Append(value.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\",");
        }

        private static void Field(StringBuilder json, string name, bool value)
        {
            json.Append('"').Append(name).Append("\":").Append(value ? "true" : "false").Append(',');
        }

        private static void Field(StringBuilder json, string name, float value)
        {
            json.Append('"').Append(name).Append("\":").Append(value.ToString("0.###", CultureInfo.InvariantCulture)).Append(',');
        }

        private static void Field(StringBuilder json, string name, long value)
        {
            json.Append('"').Append(name).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture)).Append(',');
        }
    }
}
