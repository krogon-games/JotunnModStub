using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orlog.Rules
{
    /// <summary>
    ///     A class's skills (<c>Orlog/Data/&lt;class&gt;_skills.csv</c>, made by <c>tools/make_orlog_tables.py</c>): three
    ///     tabs of exactly nine, each skill with its place on the tab's three-by-three card grid and our status. Not a
    ///     tree since the overhaul (Krogon, 2026-11-15): no level tiers and no prerequisites, for every class. No game
    ///     dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class SkillTable
    {
        public const int Tabs = 3;
        public const int Rows = 3;
        public const int Cols = 3;
        /// <summary>Every tab of every class holds exactly this many skills (Krogon, 2026-11-15).</summary>
        public const int SkillsPerTab = Rows * Cols;

        private static readonly string[] Statuses = { "v1", "reworked", "later", "dropped", "basic" };
        /// <summary>The tab of the basic skills, shown as cards above the three tabs (Krogon, 2026-11-28).</summary>
        public const int BasicTab = -1;
        private static readonly string[] Kinds = { "attack", "cast", "passive" };

        public sealed class Skill
        {
            public string Id;
            public string Name;
            public int Tab;
            public int Row;
            public int Col;
            /// <summary>
            ///     <c>v1</c>, <c>reworked</c>, <c>later</c> or <c>dropped</c> (<c>class-design.md</c> §4), or <c>basic</c>: outside
            ///     the tabs, always learned at level 1, no skill points (Gungnir's Throw; Krogon, 2026-11-28).
            /// </summary>
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

            /// <summary>A basic skill (<see cref="BasicTab" />): every character with the class has it, at level 1, for no points.</summary>
            public bool IsBasic => Status == "basic";

            /// <summary>Works now: built, or a basic skill.</summary>
            public bool Works => Built || IsBasic;
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
                "id", "name", "tab", "row", "col", "status", "kind", "needs", "eitr", "eitr_per_level", "description"
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
                if (skill.IsBasic)
                {
                    if (skill.Tab != BasicTab || skill.Row != 1 || skill.Col < 1)
                    {
                        errors.Add($"{file}: {skill.Id} is a basic skill: tab {BasicTab}, row 1, col 1 or more.");
                    }
                }
                else if (skill.Tab < 0 || skill.Tab >= Tabs || skill.Row < 1 || skill.Row > Rows || skill.Col < 1 || skill.Col > Cols)
                {
                    errors.Add($"{file}: {skill.Id} is outside the grid (tab 0-{Tabs - 1}, row 1-{Rows}, col 1-{Cols}).");
                }
                if (!places.Add($"{skill.Tab}/{skill.Row}/{skill.Col}"))
                {
                    errors.Add($"{file}: {skill.Id} takes a place another skill already has.");
                }
                table._skills.Add(skill);
                table._byId[skill.Id] = skill;
            }
            for (int tab = 0; tab < Tabs && table._skills.Count > 0; tab++)
            {
                int count = table._skills.Count(s => s.Tab == tab);
                if (count != SkillsPerTab)
                {
                    errors.Add($"{file}: tab {tab} has {count} skills; every tab has exactly {SkillsPerTab}.");
                }
            }
            return table;
        }
    }

    /// <summary>
    ///     The rules for spending skill points: Diablo 2's points and cap, any skill from level 1 (Krogon, 2026-11-15). No
    ///     game dependencies, so it is unit-tested.
    /// </summary>
    internal static class SkillRules
    {
        /// <summary>Why a basic skill takes no points (and the Learn button's text for it).</summary>
        public const string BasicNoPoints = "Basic skill: always learned";

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

        /// <summary>A skill's level from the points in it; a basic skill is always level 1 (Krogon, 2026-11-28).</summary>
        public static int LevelIn(IDictionary<string, int> points, SkillTable.Skill skill)
        {
            return skill == null ? 0 : skill.IsBasic ? 1 : PointsIn(points, skill.Id);
        }

        /// <summary>
        ///     The saved points without the skills the class no longer has, so their points count as unspent again (the
        ///     2026-11-15 overhaul removed skills characters had points in). A null table keeps everything.
        /// </summary>
        public static Dictionary<string, int> KnownOnly(IDictionary<string, int> points, SkillTable table)
        {
            var known = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in points)
            {
                if (table == null || table.Find(pair.Key) != null)
                {
                    known[pair.Key] = pair.Value;
                }
            }
            return known;
        }

        /// <summary>
        ///     Why a point can't go into <paramref name="skill" />, or "" if it can. In order: not built yet; at
        ///     <paramref name="maxPoints" />; with <paramref name="pace" />, D2's one more point per level (every skill opens
        ///     at level 1, so a skill holds at most as many points as the character has levels); no points left. There are
        ///     no level tiers and no prerequisites (Krogon, 2026-11-15).
        /// </summary>
        public static string SpendBlocked(SkillTable.Skill skill, int level, IDictionary<string, int> points, int unspent, int maxPoints, bool pace)
        {
            if (skill.IsBasic)
            {
                return BasicNoPoints;
            }
            if (!skill.Built)
            {
                return "Coming later";
            }
            int have = PointsIn(points, skill.Id);
            if (have >= maxPoints)
            {
                return "Maxed";
            }
            if (pace && have >= level)
            {
                return "Needs level " + ((long)have + 1).ToString(CultureInfo.InvariantCulture);
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
