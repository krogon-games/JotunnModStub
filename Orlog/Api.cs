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
    ///     the class, the Orlog stone, the skill tree (spending) and the skill bar (roadmap C5, 2026-11-11) are built.
    /// </summary>
    public static class Api
    {
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
            Field(json, "description", cls.Description);
            Field(json, "tab0", cls.Tabs[0]);
            Field(json, "tab1", cls.Tabs[1]);
            Field(json, "tab2", cls.Tabs[2]);
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
            // Passives show their chance at the current and the next level (roadmap C3).
            var cls = Attributes.ClassOf(player);
            string now = Passives.Describe(cls, skill.Id, points) ?? SkillBar.Describe(player, skill.Id, points) ?? skill.Description;
            string next = Passives.Describe(cls, skill.Id, points + 1) ?? SkillBar.Describe(player, skill.Id, points + 1) ?? skill.Description;
            Field(json, "description", points > 0 ? now : "Not learned yet. " + skill.Description);
            Field(json, "nextDescription", points < max ? "Level " + (points + 1).ToString(CultureInfo.InvariantCulture) + ": " + next : "");
            Field(json, "eitrCost", ActiveSkillRules.EitrCost(skill, SkillBar.ValuesFor(cls), points));
            json.Length--;
            return json.Append('}').ToString();
        }

        public static Sprite GetSkillIcon(string skillId)
        {
            return null;
        }

        public static string GetSkillBarJson()
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready || Attributes.ClassOf(player) == null)
            {
                return "";
            }
            return Safe.Try("read the skill bar", () => SkillBar.BarJson(player), "");
        }

        public static string GetSlotStateJson(int slot)
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready || Attributes.ClassOf(player) == null)
            {
                return "";
            }
            return Safe.Try("read a skill slot", () => SkillBar.SlotStateJson(player, slot), "");
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
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                return "No character";
            }
            return Safe.Try("set a skill slot", () => SkillBar.SetSlot(player, slot, skillId), "Something went wrong");
        }

        /// <summary>Right click on a skill cross slot: quick cast on or off for its skill (user, 2026-11-13). "" or why not.</summary>
        public static string ToggleQuickCast(int slot)
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready || Attributes.ClassOf(player) == null)
            {
                return "No character";
            }
            return Safe.Try("toggle quick cast", () => SkillBar.ToggleQuickCast(player, slot), "Something went wrong");
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
