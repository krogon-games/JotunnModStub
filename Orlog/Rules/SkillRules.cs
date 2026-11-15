using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orlog.Rules
{
    /// <summary>
    ///     A class's skill tree (<c>Orlog/Data/&lt;class&gt;_skills.csv</c>, made by <c>tools/make_orlog_tables.py</c>):
    ///     Diablo 2's three tabs of ten with our status per skill. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class SkillTable
    {
        /// <summary>D2's tiers: the required level of each tree row (row 1 = level 1 … row 6 = level 30).</summary>
        public static readonly int[] TierLevels = { 1, 6, 12, 18, 24, 30 };

        private static readonly string[] Statuses = { "v1", "reworked", "later", "dropped" };
        private static readonly string[] Kinds = { "attack", "cast", "passive" };

        public sealed class Skill
        {
            public string Id;
            public string Name;
            public int Tab;
            public int Row;
            public int Col;
            public int ReqLevel;
            public string[] Prereqs = new string[0];
            /// <summary><c>v1</c>, <c>reworked</c>, <c>later</c> or <c>dropped</c> (<c>class-design.md</c> §4).</summary>
            public string Status;
            /// <summary><c>attack</c> (on the skill button), <c>cast</c> (on its hotkey) or <c>passive</c> (no slot).</summary>
            public string Kind;
            /// <summary>The weapon it needs (<c>bow</c>, <c>spear</c>) or "".</summary>
            public string Needs;
            public float Eitr;
            public float EitrPerLevel;
            public string Description;

            /// <summary>Built skills can take points; later and dropped ones can't (and don't block others).</summary>
            public bool Built => Status == "v1" || Status == "reworked";
        }

        private readonly List<Skill> _skills = new List<Skill>();
        private readonly Dictionary<string, Skill> _byId = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        public IList<Skill> Skills => _skills;

        public Skill Find(string id)
        {
            return id != null && _byId.TryGetValue(id, out var skill) ? skill : null;
        }

        public static SkillTable Parse(string csv, string file, List<string> errors)
        {
            var table = new SkillTable();
            var rows = Table.Read(csv, file, new[]
            {
                "id", "name", "tab", "row", "col", "req_level", "prereqs", "status", "kind", "needs", "eitr", "eitr_per_level", "description"
            }, errors);
            var places = new HashSet<string>();
            foreach (var row in rows)
            {
                var skill = new Skill
                {
                    Id = row["id"].Trim(),
                    Name = row["name"].Trim(),
                    Tab = Table.Int(row, "tab", errors),
                    Row = Table.Int(row, "row", errors),
                    Col = Table.Int(row, "col", errors),
                    ReqLevel = Table.Int(row, "req_level", errors),
                    Prereqs = row["prereqs"].Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray(),
                    Status = row["status"].Trim().ToLowerInvariant(),
                    Kind = row["kind"].Trim().ToLowerInvariant(),
                    Needs = row["needs"].Trim().ToLowerInvariant(),
                    Eitr = Table.Float(row, "eitr", errors),
                    EitrPerLevel = Table.Float(row, "eitr_per_level", errors),
                    Description = row["description"].Trim()
                };
                if (skill.Id.Length == 0 || table._byId.ContainsKey(skill.Id))
                {
                    errors.Add($"{file}: skill id '{skill.Id}' is empty or listed twice.");
                    continue;
                }
                if (!Statuses.Contains(skill.Status))
                {
                    errors.Add($"{file}: {skill.Id} has status '{skill.Status}' ({string.Join(", ", Statuses)}).");
                }
                if (!Kinds.Contains(skill.Kind))
                {
                    errors.Add($"{file}: {skill.Id} has kind '{skill.Kind}' ({string.Join(", ", Kinds)}).");
                }
                if (skill.Tab < 0 || skill.Tab > 2 || skill.Row < 1 || skill.Row > 6 || skill.Col < 1 || skill.Col > 3)
                {
                    errors.Add($"{file}: {skill.Id} is outside the tree (tab 0-2, row 1-6, col 1-3).");
                }
                else if (skill.ReqLevel != TierLevels[skill.Row - 1])
                {
                    errors.Add($"{file}: {skill.Id} needs level {skill.ReqLevel}, but row {skill.Row} is level {TierLevels[skill.Row - 1]}.");
                }
                if (!places.Add($"{skill.Tab}/{skill.Row}/{skill.Col}"))
                {
                    errors.Add($"{file}: {skill.Id} takes a place another skill already has.");
                }
                table._skills.Add(skill);
                table._byId[skill.Id] = skill;
            }
            foreach (var skill in table._skills)
            {
                foreach (var prereq in skill.Prereqs.Where(p => table.Find(p) == null))
                {
                    errors.Add($"{file}: {skill.Id} needs '{prereq}', which isn't in the table.");
                }
            }
            return table;
        }
    }

    /// <summary>Diablo 2's rules for spending skill points, with our tree rule. No game dependencies, so it is unit-tested.</summary>
    internal static class SkillRules
    {
        /// <summary>
        ///     Unspent skill points; never below 0 (a lowered setting keeps the spent points). The total is overflow-safe
        ///     (<see cref="AttributeRules.Total" />), so a damaged save reads as nothing to spend instead of throwing.
        /// </summary>
        public static int Unspent(int level, int perLevel, IDictionary<string, int> points, int bonus = 0)
        {
            long earned = (long)AttributeRules.Earned(level, perLevel) + Math.Max(0, bonus);
            return (int)Math.Max(0L, Math.Min(int.MaxValue, earned - AttributeRules.Total(points?.Values)));
        }

        /// <summary>The bosses (by biome) a character has had its first-kill points for, saved as <c>Meadows;Swamp</c>.</summary>
        public static HashSet<string> ParseBossKills(string saved)
        {
            return new HashSet<string>((saved ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(b => b.Trim()).Where(b => b.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        public static string FormatBossKills(IEnumerable<string> biomes)
        {
            return string.Join(";", biomes.OrderBy(b => b, StringComparer.Ordinal));
        }

        /// <summary>Skill points from first boss kills (roadmap C4; user 2026-11-11): each biome's <c>boss_skill_points</c>, once.</summary>
        public static int BossBonus(IEnumerable<string> killedBiomes, BiomeLevels biomes)
        {
            if (killedBiomes == null || biomes == null)
            {
                return 0;
            }
            long total = 0;
            foreach (var biome in biomes.All)
            {
                if (killedBiomes.Contains(biome.Name, StringComparer.OrdinalIgnoreCase))
                {
                    total += Math.Max(0, biome.BossSkillPoints);
                }
            }
            return (int)Math.Min(int.MaxValue, total);
        }

        public static int PointsIn(IDictionary<string, int> points, string id)
        {
            return id != null && points.TryGetValue(id, out int n) ? n : 0;
        }

        /// <summary>
        ///     Why a point can't go into <paramref name="skill" />, or "" if it can. In order: not built yet; at
        ///     <paramref name="maxPoints" />; character level below the skill's tier; a built prerequisite without a point
        ///     (a later or dropped prerequisite is skipped, <c>class-design.md</c> §4); with <paramref name="pace" />, D2's one
        ///     more point per level past the required level; no points left.
        /// </summary>
        public static string SpendBlocked(SkillTable table, SkillTable.Skill skill, int level, IDictionary<string, int> points,
            int unspent, int maxPoints, bool pace)
        {
            if (!skill.Built)
            {
                return "Coming later";
            }
            int have = PointsIn(points, skill.Id);
            if (have >= maxPoints)
            {
                return "Maxed";
            }
            if (level < skill.ReqLevel)
            {
                return "Needs level " + skill.ReqLevel.ToString(CultureInfo.InvariantCulture);
            }
            foreach (string id in skill.Prereqs)
            {
                var prereq = table.Find(id);
                if (prereq != null && prereq.Built && PointsIn(points, prereq.Id) == 0)
                {
                    return "Needs " + prereq.Name;
                }
            }
            if (pace && (long)have >= (long)level - skill.ReqLevel + 1)
            {
                return "Needs level " + ((long)skill.ReqLevel + have).ToString(CultureInfo.InvariantCulture);
            }
            return unspent <= 0 ? "No skill points" : "";
        }

        /// <summary>D2's mana cost at a skill level, as eitr (never below 0: Magic Arrow becomes free, as in D2).</summary>
        public static float EitrCost(SkillTable.Skill skill, int skillLevel)
        {
            if (skill.Kind == "passive")
            {
                return 0f;
            }
            return Math.Max(0f, skill.Eitr + skill.EitrPerLevel * (Math.Max(1, skillLevel) - 1));
        }

        /// <summary>The saved form: <c>Id:points;Id:points</c> (counts as <see cref="AttributeRules.TryParsePoints" /> reads them).</summary>
        public static Dictionary<string, int> Parse(string saved)
        {
            var points = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in (saved ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = part.IndexOf(':');
                if (colon > 0 && AttributeRules.TryParsePoints(part.Substring(colon + 1), out int n))
                {
                    points[part.Substring(0, colon).Trim()] = n;
                }
            }
            return points;
        }

        public static string Format(IDictionary<string, int> points)
        {
            return string.Join(";", points.Where(p => p.Value > 0).OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => p.Key + ":" + p.Value.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
