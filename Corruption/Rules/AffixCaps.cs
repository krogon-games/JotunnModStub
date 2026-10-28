using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the affix cap table (<c>Corruption/Data/affix_caps.csv</c>): the most a stat can gain from
    ///     all affixes together. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class AffixCaps
    {
        private static readonly string[] RequiredColumns = { "cap", "status", "max", "unit", "families" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        public sealed class Cap
        {
            public string Id;
            public float Max;
            public string Unit;
            public List<string> Families = new List<string>();
        }

        public readonly List<Cap> Caps = new List<Cap>();

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static AffixCaps Parse(string csv, List<string> errors)
        {
            var table = new AffixCaps();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("affix_caps.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in RequiredColumns.Where(c => !header.Contains(c)))
            {
                errors.Add($"affix_caps.csv: missing column '{column}'.");
            }
            if (errors.Count > 0)
            {
                return table;
            }

            var cappedFamilies = new Dictionary<string, string>();
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

                var cap = new Cap { Id = Cell("cap"), Unit = Cell("unit") };
                string where = $"affix_caps.csv line {i + 1} ({(cap.Id.Length > 0 ? cap.Id : "no cap")})";
                if (cap.Id.Length == 0 || table.Caps.Any(c => c.Id == cap.Id))
                {
                    errors.Add($"{where}: cap name is empty or used twice.");
                }
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!float.TryParse(Cell("max"), NumberStyles.Float, CultureInfo.InvariantCulture, out cap.Max) || cap.Max <= 0f)
                {
                    errors.Add($"{where}: max '{Cell("max")}' must be a number above 0 (use a dot for decimals).");
                }
                cap.Families = Cell("families").Split(';').Select(f => f.Trim()).Where(f => f.Length > 0).ToList();
                if (cap.Families.Count == 0)
                {
                    errors.Add($"{where}: list the affix families it limits, separated by ;.");
                }
                foreach (var family in cap.Families)
                {
                    if (cappedFamilies.TryGetValue(family, out var other))
                    {
                        errors.Add($"{where}: family {family} is already limited by {other}.");
                    }
                    else
                    {
                        cappedFamilies[family] = cap.Id;
                    }
                }
                table.Caps.Add(cap);
            }
            return table;
        }

        /// <summary>The cap that limits an affix family, or null if it has none.</summary>
        public Cap For(string family)
        {
            return Caps.FirstOrDefault(c => c.Families.Contains(family));
        }

        /// <summary>A total limited to its cap.</summary>
        public float Limit(string family, float total)
        {
            var cap = For(family);
            return cap == null || total <= cap.Max ? total : cap.Max;
        }
    }
}
