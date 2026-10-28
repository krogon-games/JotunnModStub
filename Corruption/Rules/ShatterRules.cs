using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Shattering items into Affix Splinters and which items can take one. Reads and checks
    ///     <c>Corruption/Data/shattering.csv</c>. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class ShatterRules
    {
        /// <summary>Most affixes an item can have and still take a splinter, counting the splinter (the Magic limit).</summary>
        public const int MaxAffixesWithSplinters = 2;

        private static readonly string[] Settings = { "splinter_chance" };
        private static readonly string[] RequiredColumns = { "setting", "status", "value" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Chance in percent that each affix line of a shattered item becomes a splinter.</summary>
        public float SplinterChance;

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static ShatterRules Parse(string csv, List<string> errors)
        {
            var rules = new ShatterRules();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("shattering.csv is empty.");
                return rules;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"shattering.csv: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return rules;
            }

            var seen = new HashSet<string>();
            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i];
                if (cells.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string Cell(string column)
                {
                    int index = header.IndexOf(column);
                    return index >= 0 && index < cells.Count ? cells[index].Trim() : "";
                }

                string setting = Cell("setting").ToLowerInvariant();
                string where = $"shattering.csv line {i + 1} ({(setting.Length > 0 ? setting : "no setting")})";
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Settings.Contains(setting))
                {
                    errors.Add($"{where}: setting must be one of {string.Join(", ", Settings)}.");
                    continue;
                }
                if (!seen.Add(setting))
                {
                    errors.Add($"{where}: setting is listed twice.");
                    continue;
                }
                if (!float.TryParse(Cell("value"), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < 0f || value > 100f)
                {
                    errors.Add($"{where}: value '{Cell("value")}' must be a percentage from 0 to 100 (use a dot for decimals).");
                    continue;
                }
                rules.SplinterChance = value;
            }
            foreach (var setting in Settings.Where(s => !seen.Contains(s)))
            {
                errors.Add($"shattering.csv: no row for {setting}.");
            }
            return rules;
        }

        /// <summary>Uniques and corrupted items can't be shattered; Magic and Rare items can.</summary>
        public static bool CanShatter(string rarity, bool corrupted)
        {
            return !corrupted && (rarity == "magic" || rarity == "rare");
        }

        /// <summary>
        ///     Rolls a shattered item's affix lines: true where that line becomes a splinter. A resonant twin line is one
        ///     line (one roll) and gives one splinter with the combined value.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public bool[] RollSplinters(int affixLines, Func<double> random)
        {
            var result = new bool[Math.Max(0, affixLines)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = random() * 100.0 < SplinterChance;
            }
            return result;
        }

        /// <summary>
        ///     Whether an item can take a splinter: Normal or Magic, not corrupted, and still within 2 affixes in total
        ///     once the splinter is added. A resonant splinter (prefix and suffix together) counts as 2 affixes.
        ///     Slot, family and item-class checks are separate.
        /// </summary>
        public static bool CanTakeSplinter(string rarity, bool corrupted, int affixCount, bool resonantSplinter)
        {
            if (corrupted || rarity != "normal" && rarity != "magic")
            {
                return false;
            }
            return affixCount + (resonantSplinter ? 2 : 1) <= MaxAffixesWithSplinters;
        }
    }
}
