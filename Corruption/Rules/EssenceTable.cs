using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the boss essence table (<c>Corruption/Data/essences.csv</c>, compiled into the DLL): which
    ///     boss drops which essence, its look and how many per kill. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class EssenceTable
    {
        public const string ItemPrefix = "CorruptionEssence_";

        private static readonly string[] RequiredColumns =
            { "boss", "status", "look", "name", "glow", "addon", "addon_color", "per_kill", "maps_into", "description" };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        public sealed class Essence
        {
            /// <summary>Boss prefab name, e.g. Eikthyr, gd_king.</summary>
            public string Boss;
            public string Look;
            public string Name;
            public string Description;
            public string Glow;
            public string Addon;
            public string AddonColor;
            public int PerKill;
            public string MapsInto;

            /// <summary>The essence item's prefab name.</summary>
            public string Item => ItemPrefix + Look;
        }

        public static List<Essence> Parse(string csv, List<string> errors)
        {
            var essences = new List<Essence>();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("essences.csv is empty.");
                return essences;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in RequiredColumns.Where(c => !header.Contains(c)))
            {
                errors.Add($"essences.csv: missing column '{column}'.");
            }
            if (errors.Count > 0)
            {
                return essences;
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

                var e = new Essence
                {
                    Boss = Cell("boss"),
                    Look = Cell("look"),
                    Name = Cell("name"),
                    Description = Cell("description"),
                    Glow = Cell("glow"),
                    Addon = Cell("addon"),
                    AddonColor = Cell("addon_color"),
                    MapsInto = Cell("maps_into")
                };
                string where = $"essences.csv line {i + 1} ({(e.Boss.Length > 0 ? e.Boss : "no boss")})";

                if (e.Boss.Length == 0 || essences.Any(x => x.Boss == e.Boss))
                {
                    errors.Add($"{where}: boss is empty or listed twice.");
                }
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (e.Look.Length == 0 || e.Look.Any(c => !(c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_')))
                {
                    errors.Add($"{where}: look must be lowercase letters, digits and _ (it names the texture files).");
                }
                else if (essences.Any(x => x.Look == e.Look))
                {
                    errors.Add($"{where}: look '{e.Look}' is used twice.");
                }
                if (e.Name.Length == 0 || e.Description.Length == 0)
                {
                    errors.Add($"{where}: name and description can't be empty.");
                }
                if (!IsColor(e.Glow))
                {
                    errors.Add($"{where}: glow '{e.Glow}' must be a colour like #FF8800.");
                }
                if (e.Addon.Length > 0 && !IsColor(e.AddonColor))
                {
                    errors.Add($"{where}: addon_color '{e.AddonColor}' must be a colour like #FF8800.");
                }
                if (!int.TryParse(Cell("per_kill"), NumberStyles.Integer, CultureInfo.InvariantCulture, out e.PerKill) || e.PerKill < 1)
                {
                    errors.Add($"{where}: per_kill must be a whole number, 1 or more.");
                }
                essences.Add(e);
            }
            return essences;
        }

        /// <summary>#RRGGBB.</summary>
        public static bool IsColor(string value)
        {
            return value != null && value.Length == 7 && value[0] == '#' &&
                   int.TryParse(value.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
        }
    }
}
