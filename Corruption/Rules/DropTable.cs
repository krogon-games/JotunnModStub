using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the drop chance tables (<c>Corruption/Data/drop_chances.csv</c> and
    ///     <c>drop_modifiers.csv</c>) and works out a drop's final chance. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class DropTable
    {
        /// <summary>Rarities with their own drop roll. Adding one also needs a column in the modifier table and code.</summary>
        public static readonly string[] Rarities = { "magic", "rare", "unique" };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Rarity → base chance as "1 in N".</summary>
        public readonly Dictionary<string, float> OneIn = new Dictionary<string, float>();

        /// <summary>Source → rarity → multiplier. Blank cells are left out (x1).</summary>
        public readonly Dictionary<string, Dictionary<string, float>> Modifiers = new Dictionary<string, Dictionary<string, float>>();

        /// <summary>Parses both tables. Every problem found is added to <paramref name="errors" />, naming the file and line.</summary>
        public static DropTable Parse(string chancesCsv, string modifiersCsv, List<string> errors)
        {
            var table = new DropTable();

            var rows = Csv.ReadRows(chancesCsv);
            var header = Header(rows, "drop_chances.csv", new[] { "rarity", "status", "one_in" }, errors);
            if (header != null)
            {
                for (int i = 1; i < rows.Count; i++)
                {
                    if (Blank(rows[i]))
                    {
                        continue;
                    }
                    string rarity = Cell(rows[i], header, "rarity").ToLowerInvariant();
                    string where = $"drop_chances.csv line {i + 1} ({rarity})";
                    CheckStatus(Cell(rows[i], header, "status"), where, errors);
                    if (!Rarities.Contains(rarity))
                    {
                        errors.Add($"{where}: rarity must be one of {string.Join(", ", Rarities)}.");
                        continue;
                    }
                    if (table.OneIn.ContainsKey(rarity))
                    {
                        errors.Add($"{where}: rarity is listed twice.");
                        continue;
                    }
                    float oneIn = Number(Cell(rows[i], header, "one_in"), where, "one_in", errors);
                    if (oneIn < 1f)
                    {
                        errors.Add($"{where}: one_in must be 1 or more (1 = always drops).");
                    }
                    table.OneIn[rarity] = oneIn;
                }
                foreach (var rarity in Rarities.Where(r => !table.OneIn.ContainsKey(r)))
                {
                    errors.Add($"drop_chances.csv: no row for {rarity}.");
                }
            }

            rows = Csv.ReadRows(modifiersCsv);
            header = Header(rows, "drop_modifiers.csv", new[] { "source", "status" }.Concat(Rarities).ToArray(), errors);
            if (header != null)
            {
                for (int i = 1; i < rows.Count; i++)
                {
                    if (Blank(rows[i]))
                    {
                        continue;
                    }
                    string source = Cell(rows[i], header, "source");
                    string where = $"drop_modifiers.csv line {i + 1} ({(source.Length > 0 ? source : "no source")})";
                    CheckStatus(Cell(rows[i], header, "status"), where, errors);
                    if (source.Length == 0 || source.Any(c => !(c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_')))
                    {
                        errors.Add($"{where}: source must be lowercase letters, digits and _.");
                        continue;
                    }
                    if (table.Modifiers.ContainsKey(source))
                    {
                        errors.Add($"{where}: source is listed twice.");
                        continue;
                    }
                    var multipliers = new Dictionary<string, float>();
                    foreach (var rarity in Rarities)
                    {
                        string value = Cell(rows[i], header, rarity);
                        if (value.Length == 0)
                        {
                            continue;
                        }
                        float multiplier = Number(value, where, rarity, errors);
                        if (multiplier <= 0f)
                        {
                            errors.Add($"{where}: {rarity} multiplier must be above 0 (leave it blank for no change).");
                        }
                        multipliers[rarity] = multiplier;
                    }
                    table.Modifiers[source] = multipliers;
                }
            }
            return table;
        }

        /// <summary>
        ///     Chance (0–1) that one roll drops an item of this rarity: the base chance times the multiplier of every
        ///     source that applies (e.g. <c>star_2</c> and <c>scar</c>), capped at 1. Unknown sources count as x1.
        /// </summary>
        public float Chance(string rarity, IEnumerable<string> sources)
        {
            if (!OneIn.TryGetValue(rarity, out var oneIn) || oneIn <= 0f)
            {
                return 0f;
            }
            double chance = 1.0 / oneIn;
            foreach (var source in sources ?? Enumerable.Empty<string>())
            {
                if (Modifiers.TryGetValue(source, out var multipliers) && multipliers.TryGetValue(rarity, out var multiplier))
                {
                    chance *= multiplier;
                }
            }
            return (float)Math.Min(1.0, chance);
        }

        /// <summary>
        ///     One player's loot roll for one kill or chest (personal loot: each nearby player rolls separately).
        ///     Magic and Rare roll separately, but at most one of them drops: if both succeed, only the Rare drops.
        ///     A Unique rolls on its own, on top. Returns the rarities that drop, rarest first.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public List<string> Roll(IEnumerable<string> sources, Func<double> random)
        {
            var list = sources == null ? new List<string>() : sources.ToList();
            var drops = new List<string>();
            if (random() < Chance("unique", list))
            {
                drops.Add("unique");
            }
            bool rare = random() < Chance("rare", list);
            bool magic = random() < Chance("magic", list);
            if (rare)
            {
                drops.Add("rare");
            }
            else if (magic)
            {
                drops.Add("magic");
            }
            return drops;
        }

        private static List<string> Header(List<List<string>> rows, string file, string[] required, List<string> errors)
        {
            if (rows.Count == 0)
            {
                errors.Add($"{file} is empty.");
                return null;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = required.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"{file}: missing column '{column}'.");
            }
            return missing.Count == 0 ? header : null;
        }

        private static string Cell(List<string> row, List<string> header, string column)
        {
            int index = header.IndexOf(column);
            return index >= 0 && index < row.Count ? row[index].Trim() : "";
        }

        private static bool Blank(List<string> row)
        {
            return row.All(c => c.Trim().Length == 0);
        }

        private static void CheckStatus(string status, string where, List<string> errors)
        {
            if (!Statuses.Contains(status.ToLowerInvariant()))
            {
                errors.Add($"{where}: status '{status}' must be one of {string.Join(", ", Statuses)}.");
            }
        }

        private static float Number(string value, string where, string column, List<string> errors)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return number;
            }
            errors.Add($"{where}: {column} '{value}' is not a number (use a dot for decimals).");
            return 0f;
        }
    }
}
