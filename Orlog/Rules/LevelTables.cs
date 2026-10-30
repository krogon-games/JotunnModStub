using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Corruption.Rules;

namespace Orlog.Rules
{
    /// <summary>
    ///     The XP needed for each level and D2's high-level XP ratio (<c>Orlog/Data/levels.csv</c>, Diablo 2's experience
    ///     table). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class LevelTable
    {
        private readonly long[] _total;
        private readonly int[] _ratio;

        private LevelTable(long[] total, int[] ratio)
        {
            _total = total;
            _ratio = ratio;
        }

        /// <summary>The highest level in the table.</summary>
        public int TopLevel => _total.Length;

        public static LevelTable Parse(string csv, List<string> errors)
        {
            var rows = Table.Read(csv, "levels.csv", new[] { "level", "xp_total", "exp_ratio" }, errors);
            var total = new List<long>();
            var ratio = new List<int>();
            foreach (var row in rows)
            {
                int level = Table.Int(row, "level", errors);
                if (level != total.Count + 1)
                {
                    errors.Add($"levels.csv: expected level {total.Count + 1}, found {level}.");
                    break;
                }
                long xp = Table.Long(row, "xp_total", errors);
                if (total.Count > 0 && xp <= total[total.Count - 1])
                {
                    errors.Add($"levels.csv: level {level} needs more XP than level {level - 1}.");
                }
                if (level == 1 && xp != 0)
                {
                    errors.Add("levels.csv: level 1 must start at 0 XP.");
                }
                int r = Table.Int(row, "exp_ratio", errors);
                if (r < 0 || r > 1024)
                {
                    errors.Add($"levels.csv: level {level} exp_ratio must be 0-1024.");
                }
                total.Add(xp);
                ratio.Add(r);
            }
            if (total.Count == 0)
            {
                errors.Add("levels.csv has no levels.");
                total.Add(0);
                ratio.Add(1024);
            }
            return new LevelTable(total.ToArray(), ratio.ToArray());
        }

        /// <summary>Total XP needed to reach <paramref name="level" /> (clamped to the table).</summary>
        public long TotalFor(int level)
        {
            return _total[Math.Max(1, Math.Min(level, TopLevel)) - 1];
        }

        /// <summary>The level a total of <paramref name="xp" /> gives, never above <paramref name="maxLevel" />.</summary>
        public int LevelFor(long xp, int maxLevel)
        {
            int cap = Math.Max(1, Math.Min(maxLevel, TopLevel));
            int level = 1;
            while (level < cap && xp >= _total[level])
            {
                level++;
            }
            return level;
        }

        /// <summary>XP from <paramref name="level" /> to the next, or 0 at <paramref name="maxLevel" />.</summary>
        public long ToNext(int level, int maxLevel)
        {
            int cap = Math.Max(1, Math.Min(maxLevel, TopLevel));
            return level >= cap ? 0 : TotalFor(level + 1) - TotalFor(level);
        }

        /// <summary>D2's XP ratio for a character of this level: 1 below level 70, falling to 5/1024 at 99.</summary>
        public float HighLevelRatio(int level)
        {
            return _ratio[Math.Max(1, Math.Min(level, TopLevel)) - 1] / 1024f;
        }
    }

    /// <summary>The XP a creature of each level gives (<c>Orlog/Data/creature_xp.csv</c>, made by <c>tools/make_orlog_tables.py</c>).</summary>
    internal sealed class CreatureXpTable
    {
        private readonly long[] _xp;

        private CreatureXpTable(long[] xp)
        {
            _xp = xp;
        }

        public static CreatureXpTable Parse(string csv, List<string> errors)
        {
            var rows = Table.Read(csv, "creature_xp.csv", new[] { "level", "xp" }, errors);
            var xp = new List<long>();
            foreach (var row in rows)
            {
                int level = Table.Int(row, "level", errors);
                if (level != xp.Count + 1)
                {
                    errors.Add($"creature_xp.csv: expected level {xp.Count + 1}, found {level}.");
                    break;
                }
                long value = Table.Long(row, "xp", errors);
                if (value <= 0)
                {
                    errors.Add($"creature_xp.csv: level {level} must give more than 0 XP.");
                }
                xp.Add(value);
            }
            if (xp.Count == 0)
            {
                errors.Add("creature_xp.csv has no levels.");
                xp.Add(1);
            }
            return new CreatureXpTable(xp.ToArray());
        }

        public int TopLevel => _xp.Length;

        /// <summary>XP for a creature of <paramref name="level" /> (clamped to the table).</summary>
        public long For(int level)
        {
            return _xp[Math.Max(1, Math.Min(level, TopLevel)) - 1];
        }
    }

    /// <summary>
    ///     Each biome's creature level range, the health span of its creatures and its boss
    ///     (<c>Orlog/Data/biome_levels.csv</c>, from the user's levelling guide and the game dump).
    /// </summary>
    internal sealed class BiomeLevels
    {
        public sealed class Biome
        {
            public string Name;
            public int MinLevel;
            public int MaxLevel;
            public float HealthMin;
            public float HealthMax;
            public int BossLevel;
            public string[] Bosses;
        }

        private readonly Dictionary<string, Biome> _biomes = new Dictionary<string, Biome>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _bosses = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<Biome> All => _biomes.Values;

        public static BiomeLevels Parse(string csv, List<string> errors)
        {
            var result = new BiomeLevels();
            var rows = Table.Read(csv, "biome_levels.csv",
                new[] { "biome", "min_level", "max_level", "health_min", "health_max", "boss_level", "bosses" }, errors);
            foreach (var row in rows)
            {
                var biome = new Biome
                {
                    Name = row["biome"].Trim(),
                    MinLevel = Table.Int(row, "min_level", errors),
                    MaxLevel = Table.Int(row, "max_level", errors),
                    HealthMin = Table.Float(row, "health_min", errors),
                    HealthMax = Table.Float(row, "health_max", errors),
                    BossLevel = Table.Int(row, "boss_level", errors),
                    Bosses = row["bosses"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(b => b.Trim()).ToArray()
                };
                if (biome.Name.Length == 0 || result._biomes.ContainsKey(biome.Name))
                {
                    errors.Add($"biome_levels.csv: biome '{biome.Name}' is empty or listed twice.");
                    continue;
                }
                if (biome.MinLevel < 1 || biome.MaxLevel < biome.MinLevel)
                {
                    errors.Add($"biome_levels.csv: {biome.Name} needs 1 <= min_level <= max_level.");
                }
                if (biome.HealthMin <= 0 || biome.HealthMax < biome.HealthMin)
                {
                    errors.Add($"biome_levels.csv: {biome.Name} needs 0 < health_min <= health_max.");
                }
                result._biomes[biome.Name] = biome;
                foreach (var boss in biome.Bosses)
                {
                    result._bosses[boss] = biome.BossLevel;
                }
            }
            return result;
        }

        /// <summary>The boss's level from the levelling guide, or 0 if <paramref name="prefab" /> isn't a listed boss.</summary>
        public int BossLevel(string prefab)
        {
            return prefab != null && _bosses.TryGetValue(prefab, out int level) ? level : 0;
        }

        /// <summary>
        ///     A creature's level in <paramref name="biome" />: placed in the biome's level range by its base health, on a log
        ///     scale between the biome's weakest and strongest creature (clamped). 0 when the biome has no range (Ocean).
        /// </summary>
        public int CreatureLevel(string biome, float baseHealth)
        {
            if (biome == null || !_biomes.TryGetValue(biome, out var b))
            {
                return 0;
            }
            if (b.MaxLevel <= b.MinLevel || b.HealthMax <= b.HealthMin || baseHealth <= b.HealthMin)
            {
                return b.MinLevel;
            }
            if (baseHealth >= b.HealthMax)
            {
                return b.MaxLevel;
            }
            double t = (Math.Log(baseHealth) - Math.Log(b.HealthMin)) / (Math.Log(b.HealthMax) - Math.Log(b.HealthMin));
            return b.MinLevel + (int)Math.Round(t * (b.MaxLevel - b.MinLevel));
        }
    }

    /// <summary>Shared row reading for the tables above.</summary>
    internal static class Table
    {
        public static List<Dictionary<string, string>> Read(string csv, string file, string[] required, List<string> errors)
        {
            var result = new List<Dictionary<string, string>>();
            var rows = Csv.ReadRows(csv ?? "");
            if (rows.Count == 0)
            {
                errors.Add($"{file} is empty.");
                return result;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = required.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"{file}: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return result;
            }
            foreach (var cells in rows.Skip(1))
            {
                if (cells.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                var row = new Dictionary<string, string>();
                for (int i = 0; i < header.Count; i++)
                {
                    row[header[i]] = i < cells.Count ? cells[i] : "";
                }
                result.Add(row);
            }
            return result;
        }

        public static int Int(Dictionary<string, string> row, string column, List<string> errors)
        {
            if (int.TryParse(row[column].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }
            errors.Add($"'{row[column]}' in column {column} is not a whole number.");
            return 0;
        }

        public static long Long(Dictionary<string, string> row, string column, List<string> errors)
        {
            if (long.TryParse(row[column].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            {
                return value;
            }
            errors.Add($"'{row[column]}' in column {column} is not a whole number.");
            return 0;
        }

        public static float Float(Dictionary<string, string> row, string column, List<string> errors)
        {
            if (float.TryParse(row[column].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }
            errors.Add($"'{row[column]}' in column {column} is not a number.");
            return 0f;
        }
    }
}
