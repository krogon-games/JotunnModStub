using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the currency orb drop table (<c>Corruption/Data/orbs.csv</c>) and rolls it. Krogon's rules
    ///     of 2026-11-19 (#33): every orb can drop from all content; each orb rolls on its own, per kill and per player
    ///     (personal loot, as Magic and Rare gear), so one roll can give several orbs or none; the chance is the orb's
    ///     "1 in N" times the <c>orb</c> column of <c>drop_modifiers.csv</c> for every source that applies, capped at
    ///     100%. A blank <c>one_in</c> never drops (the Jeweller's Orb until sockets exist). No game dependencies, so it
    ///     is unit-tested.
    /// </summary>
    internal sealed class OrbDropTable
    {
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Orb id (<see cref="OrbRules.Orbs" />) → base chance as "1 in N". Orbs that never drop are left out.</summary>
        public readonly Dictionary<string, float> OneIn = new Dictionary<string, float>();

        /// <summary>Parses the table. Every problem found is added to <paramref name="errors" />, naming the file and line.</summary>
        public static OrbDropTable Parse(string csv, List<string> errors)
        {
            var table = new OrbDropTable();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("orbs.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in new[] { "orb", "status", "one_in" }.Where(c => !header.Contains(c)))
            {
                errors.Add($"orbs.csv: missing column '{column}'.");
            }
            if (!header.Contains("orb") || !header.Contains("status") || !header.Contains("one_in"))
            {
                return table;
            }
            string Cell(List<string> row, string column)
            {
                int index = header.IndexOf(column);
                return index < row.Count ? row[index].Trim() : "";
            }

            var seen = new HashSet<string>();
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string orb = Cell(row, "orb").ToLowerInvariant();
                string where = $"orbs.csv line {i + 1} ({orb})";
                if (!Statuses.Contains(Cell(row, "status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell(row, "status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!OrbRules.Orbs.Contains(orb))
                {
                    errors.Add($"{where}: orb must be one of {string.Join(", ", OrbRules.Orbs)}.");
                    continue;
                }
                if (!seen.Add(orb))
                {
                    errors.Add($"{where}: orb is listed twice.");
                    continue;
                }
                string value = Cell(row, "one_in");
                if (value.Length == 0)
                {
                    continue;
                }
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var oneIn) || oneIn < 1f)
                {
                    errors.Add($"{where}: one_in '{value}' must be a number of 1 or more (1 = always drops), or blank for never.");
                    continue;
                }
                table.OneIn[orb] = oneIn;
            }
            foreach (var orb in OrbRules.Orbs.Where(o => !seen.Contains(o)))
            {
                errors.Add($"orbs.csv: no row for {orb} (leave one_in blank if it shouldn't drop).");
            }
            return table;
        }

        /// <summary>Chance (0–1) that one roll drops this orb, with the sources' combined multiplier; 0 for an orb that never drops.</summary>
        public float Chance(string orb, float multiplier)
        {
            if (!OneIn.TryGetValue(orb, out var oneIn) || oneIn <= 0f)
            {
                return 0f;
            }
            return (float)Math.Min(1.0, multiplier / oneIn);
        }

        /// <summary>
        ///     One orb, always: picked with the table's own odds as weights (each orb's 1 / one_in), so the common orbs
        ///     come most often and every orb that drops from kills can come. For Hoardfinder's and of Hidden Riches (Krogon,
        ///     2026-11-19: "a guaranteed orb … heavily weighted towards the more common orbs"). Null only when no orb drops.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public string Pick(Func<double> random)
        {
            double total = OrbRules.Orbs.Sum(o => (double)Chance(o, 1f));
            if (total <= 0.0)
            {
                return null;
            }
            double roll = random() * total;
            string last = null;
            foreach (var orb in OrbRules.Orbs)
            {
                double weight = Chance(orb, 1f);
                if (weight <= 0.0)
                {
                    continue;
                }
                last = orb;
                if (roll < weight)
                {
                    return orb;
                }
                roll -= weight;
            }
            return last;
        }

        /// <summary>
        ///     One roll of the table: each orb that can drop rolls on its own, in <see cref="OrbRules.Orbs" />' order.
        ///     Returns the orbs that drop (often none).
        /// </summary>
        /// <param name="multiplier">The sources' combined <c>orb</c> multiplier (<see cref="DropTable.Multiplier" />); 1 for none.</param>
        /// <param name="random">Returns a value in [0, 1).</param>
        public List<string> Roll(float multiplier, Func<double> random)
        {
            var drops = new List<string>();
            foreach (var orb in OrbRules.Orbs)
            {
                float chance = Chance(orb, multiplier);
                if (chance > 0f && random() < chance)
                {
                    drops.Add(orb);
                }
            }
            return drops;
        }
    }
}
