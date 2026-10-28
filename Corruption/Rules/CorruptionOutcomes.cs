using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the shard corruption table (<c>Corruption/Data/corruption_outcomes.csv</c>) and rolls what
    ///     happens when a shard is used on an item. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class CorruptionOutcomes
    {
        /// <summary>Destroyed; nothing added but locked; one corrupted affix added and locked. A new outcome needs code.</summary>
        public static readonly string[] Outcomes = { "bricked", "unchanged", "affix" };

        private static readonly string[] RequiredColumns = { "outcome", "status", "chance" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Outcome → chance in percent. The chances add up to 100.</summary>
        public readonly Dictionary<string, float> Percent = new Dictionary<string, float>();

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static CorruptionOutcomes Parse(string csv, List<string> errors)
        {
            var table = new CorruptionOutcomes();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("corruption_outcomes.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"corruption_outcomes.csv: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return table;
            }

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

                string outcome = Cell("outcome").ToLowerInvariant();
                string where = $"corruption_outcomes.csv line {i + 1} ({(outcome.Length > 0 ? outcome : "no outcome")})";
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Outcomes.Contains(outcome))
                {
                    errors.Add($"{where}: outcome must be one of {string.Join(", ", Outcomes)}.");
                    continue;
                }
                if (table.Percent.ContainsKey(outcome))
                {
                    errors.Add($"{where}: outcome is listed twice.");
                    continue;
                }
                if (!float.TryParse(Cell("chance"), NumberStyles.Float, CultureInfo.InvariantCulture, out var chance) || chance < 0f)
                {
                    errors.Add($"{where}: chance '{Cell("chance")}' must be a percentage of 0 or more (use a dot for decimals).");
                    continue;
                }
                table.Percent[outcome] = chance;
            }

            foreach (var outcome in Outcomes.Where(o => !table.Percent.ContainsKey(o)))
            {
                errors.Add($"corruption_outcomes.csv: no row for {outcome}.");
            }
            float total = table.Percent.Values.Sum();
            if (table.Percent.Count == Outcomes.Length && Math.Abs(total - 100f) > 0.01f)
            {
                errors.Add($"corruption_outcomes.csv: the chances add up to {total.ToString(CultureInfo.InvariantCulture)}, not 100.");
            }
            return table;
        }

        /// <summary>What happens when a shard is used on an item: one of <see cref="Outcomes" />.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public string Roll(Func<double> random)
        {
            double roll = random() * 100.0;
            double upTo = 0.0;
            foreach (var outcome in Outcomes)
            {
                upTo += Percent.TryGetValue(outcome, out var chance) ? chance : 0f;
                if (roll < upTo)
                {
                    return outcome;
                }
            }
            return Outcomes[Outcomes.Length - 1];
        }
    }
}
