using System.Collections.Generic;
using Orlog.Rules;

namespace Orlog.Content
{
    /// <summary>
    ///     The local player's skill points, saved in the character's custom data (<c>orlog_skills</c>, as
    ///     <c>Id:points;…</c>), and each class's skill tree (<c>Orlog/Data/&lt;class&gt;_skills.csv</c>). Spending follows
    ///     <see cref="SkillRules" />; what the skills do comes with each skill (roadmap C3 onwards).
    /// </summary>
    internal static class SkillPoints
    {
        public const string PointsKey = "orlog_skills";

        private static readonly Dictionary<string, SkillTable> Tables = new Dictionary<string, SkillTable>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>The class's tree, read once from its embedded table; null if the class has none.</summary>
        public static SkillTable TableFor(ClassTable.Class cls)
        {
            if (cls == null)
            {
                return null;
            }
            if (!Tables.TryGetValue(cls.Name, out var table))
            {
                string file = cls.Name.ToLowerInvariant() + "_skills.csv";
                string csv = Progress.ReadData(file);
                var errors = new List<string>();
                table = csv.Length > 0 ? SkillTable.Parse(csv, file, errors) : null;
                foreach (var error in errors)
                {
                    Jotunn.Logger.LogError("Orlog data: " + error);
                }
                Tables[cls.Name] = table;
            }
            return table;
        }

        public static SkillTable TableOf(Player player)
        {
            return TableFor(Attributes.ClassOf(player));
        }

        public static Dictionary<string, int> Points(Player player)
        {
            return SkillRules.Parse(player && player.m_customData.TryGetValue(PointsKey, out string saved) ? saved : "");
        }

        public static int Unspent(Player player)
        {
            return SkillRules.Unspent(Progress.GetLevel(player), OrlogConfig.SkillPointsPerLevel.Value, Points(player));
        }

        /// <summary>Why a point can't go into <paramref name="skill" /> for the local player, "" if it can.</summary>
        public static string SpendBlocked(Player player, SkillTable table, SkillTable.Skill skill)
        {
            return SkillRules.SpendBlocked(table, skill, Progress.GetLevel(player), Points(player), Unspent(player),
                OrlogConfig.MaxSkillPoints.Value, OrlogConfig.SkillPace.Value);
        }

        /// <summary>Spends one point; "" on success, otherwise why not.</summary>
        public static string Spend(Player player, string skillId)
        {
            var table = TableOf(player);
            if (table == null)
            {
                return "Choose a class at the Orlog stone first";
            }
            var skill = table.Find(skillId);
            if (skill == null)
            {
                return "Unknown skill";
            }
            string blocked = SpendBlocked(player, table, skill);
            if (blocked.Length > 0)
            {
                return blocked;
            }
            var points = Points(player);
            points[skill.Id] = AttributeRules.AddClamped(SkillRules.PointsIn(points, skill.Id), 1);
            player.m_customData[PointsKey] = SkillRules.Format(points);
            Progress.NotifyChanged();
            return "";
        }

        /// <summary>Gives back every skill point (with the attribute points, D2's respec).</summary>
        public static void Respec(Player player)
        {
            player.m_customData.Remove(PointsKey);
            Progress.NotifyChanged();
        }
    }
}
