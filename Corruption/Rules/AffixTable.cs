using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the affix table (<c>Corruption/Data/affixes.csv</c>): one row per affix, tagged with the
    ///     item classes it can roll on. No game dependencies, so it is unit-tested; the tests also check the real file.
    /// </summary>
    internal static class AffixTable
    {
        /// <summary>Item class columns, in file order. An item can belong to several (a one-handed axe is weapon + axe_1h).</summary>
        public static readonly string[] ItemClasses =
            { "weapon", "shield", "helmet", "chest", "legs", "cape", "jewellery", "pickaxe", "axe_1h" };

        private static readonly string[] RequiredColumns =
        {
            "id", "status", "pool", "side", "family", "name", "item_name", "text", "min", "max", "unit", "weight", "effect"
        };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };
        private static readonly string[] Sides = { "prefix", "suffix" };
        private static readonly string[] Units = { "%", "flat", "m", "levels" };

        /// <summary>Item classes each pool may use. Gathering pools stay on tools; gear stays off pickaxes.</summary>
        private static readonly Dictionary<string, string[]> PoolClasses = new Dictionary<string, string[]>
        {
            { "gathering-universal", new[] { "pickaxe", "axe_1h" } },
            { "gathering-mining", new[] { "pickaxe" } },
            { "gathering-woodcutting", new[] { "axe_1h" } },
            { "gear", new[] { "weapon", "shield", "helmet", "chest", "legs", "cape", "jewellery" } }
        };

        public sealed class Affix
        {
            public string Id;
            public string Status;
            public string Pool;
            public string Side;
            public string Family;
            public string Name;
            public string ItemName;
            public string Text;
            public float Min;
            public float Max;
            public string Unit;
            public float Weight;
            public string Effect;
            public HashSet<string> Classes = new HashSet<string>();
            public string Notes;

            public bool IsPrefix => Side == "prefix";
        }

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static List<Affix> Parse(string csv, List<string> errors)
        {
            var affixes = new List<Affix>();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("The file is empty.");
                return affixes;
            }

            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in RequiredColumns.Concat(ItemClasses))
            {
                if (!header.Contains(column))
                {
                    errors.Add($"Missing column '{column}'.");
                }
            }
            if (errors.Count > 0)
            {
                return affixes;
            }

            var ids = new HashSet<string>();
            var familyUnits = new Dictionary<string, string>();
            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i];
                if (cells.All(c => c.Trim().Length == 0))
                {
                    continue;
                }

                int line = i + 1;
                Func<string, string> cell = column =>
                {
                    int index = header.IndexOf(column);
                    return index >= 0 && index < cells.Count ? cells[index].Trim() : "";
                };

                var affix = new Affix
                {
                    Id = cell("id"),
                    Status = cell("status").ToLowerInvariant(),
                    Pool = cell("pool").ToLowerInvariant(),
                    Side = cell("side").ToLowerInvariant(),
                    Family = cell("family"),
                    Name = cell("name"),
                    ItemName = cell("item_name"),
                    Text = cell("text"),
                    Unit = cell("unit"),
                    Effect = cell("effect"),
                    Notes = header.Contains("notes") ? cell("notes") : ""
                };
                string where = $"Line {line} ({(affix.Id.Length > 0 ? affix.Id : "no id")})";

                if (affix.Id.Length == 0 || affix.Id.Any(c => !(c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_')))
                {
                    errors.Add($"{where}: id must be lowercase letters, digits and _.");
                }
                else if (!ids.Add(affix.Id))
                {
                    errors.Add($"{where}: id is used twice.");
                }
                Check(errors, where, "status", affix.Status, Statuses);
                Check(errors, where, "side", affix.Side, Sides);
                Check(errors, where, "unit", affix.Unit, Units);
                if (!PoolClasses.ContainsKey(affix.Pool))
                {
                    errors.Add($"{where}: pool '{affix.Pool}' must be one of {string.Join(", ", PoolClasses.Keys)}.");
                }
                foreach (var column in new[] { "family", "name", "item_name", "effect" })
                {
                    if (cell(column).Length == 0)
                    {
                        errors.Add($"{where}: {column} is empty.");
                    }
                }
                if (!affix.Text.Contains("{0}"))
                {
                    errors.Add($"{where}: text needs {{0}} where the rolled value goes.");
                }

                affix.Min = Number(errors, where, "min", cell("min"));
                affix.Max = Number(errors, where, "max", cell("max"));
                affix.Weight = Number(errors, where, "weight", cell("weight"));
                if (affix.Min > affix.Max)
                {
                    errors.Add($"{where}: min is larger than max.");
                }
                if (affix.Weight <= 0f)
                {
                    errors.Add($"{where}: weight must be above 0.");
                }

                foreach (var itemClass in ItemClasses)
                {
                    string mark = cell(itemClass).ToLowerInvariant();
                    if (mark == "x")
                    {
                        affix.Classes.Add(itemClass);
                    }
                    else if (mark.Length > 0)
                    {
                        errors.Add($"{where}: column {itemClass} should be x or empty, not '{mark}'.");
                    }
                }
                if (affix.Classes.Count == 0)
                {
                    errors.Add($"{where}: mark at least one item class with x.");
                }
                else if (PoolClasses.TryGetValue(affix.Pool, out var allowed))
                {
                    foreach (var itemClass in affix.Classes.Where(c => !allowed.Contains(c)))
                    {
                        errors.Add($"{where}: pool {affix.Pool} can't roll on {itemClass}.");
                    }
                }

                if (affix.Family.Length > 0)
                {
                    if (familyUnits.TryGetValue(affix.Family, out var unit) && unit != affix.Unit)
                    {
                        errors.Add($"{where}: family {affix.Family} mixes units ({unit} and {affix.Unit}).");
                    }
                    familyUnits[affix.Family] = affix.Unit;
                }

                affixes.Add(affix);
            }
            return affixes;
        }

        /// <summary>Affixes that can roll on an item belonging to the given classes, on one side.</summary>
        public static List<Affix> Candidates(IEnumerable<Affix> affixes, ICollection<string> itemClasses, bool prefix)
        {
            return affixes.Where(a => a.IsPrefix == prefix && a.Classes.Any(itemClasses.Contains)).ToList();
        }

        private static void Check(List<string> errors, string where, string column, string value, string[] allowed)
        {
            if (!allowed.Contains(value))
            {
                errors.Add($"{where}: {column} '{value}' must be one of {string.Join(", ", allowed)}.");
            }
        }

        private static float Number(List<string> errors, string where, string column, string value)
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
