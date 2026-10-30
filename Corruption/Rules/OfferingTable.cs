using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the corrupted offerings table (<c>Corruption/Data/offerings.csv</c>, compiled into the DLL):
    ///     for each boss, the vanilla item that summons it, how it's used (offered at an altar, placed on the altar's
    ///     item stands, or a door key) and how many vanilla items one corrupted item takes. No game dependencies, so it
    ///     is unit-tested.
    /// </summary>
    internal static class OfferingTable
    {
        public const string ItemPrefix = "Corrupted";

        public const string UseAltar = "altar";
        public const string UseStand = "stand";
        public const string UseDoor = "door";

        private static readonly string[] RequiredColumns = { "boss", "item", "use", "amount", "name", "description" };
        private static readonly string[] Uses = { UseAltar, UseStand, UseDoor };

        public sealed class Offering
        {
            /// <summary>The boss prefab the offering summons, e.g. Eikthyr, FrozenKing.</summary>
            public string Boss;
            /// <summary>The vanilla item prefab, e.g. TrophyDeer.</summary>
            public string Item;
            public string Use;
            /// <summary>Vanilla items in one corrupted item (plus 1 shard).</summary>
            public int Amount;
            public string Name;
            public string Description;

            /// <summary>The corrupted item's prefab name.</summary>
            public string CorruptedItem => ItemPrefix + Item;
        }

        public static List<Offering> Parse(string csv, List<string> errors)
        {
            var offerings = new List<Offering>();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("offerings.csv is empty.");
                return offerings;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in RequiredColumns.Where(c => !header.Contains(c)))
            {
                errors.Add($"offerings.csv: missing column '{column}'.");
            }
            if (errors.Count > 0)
            {
                return offerings;
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

                var o = new Offering
                {
                    Boss = Cell("boss"),
                    Item = Cell("item"),
                    Use = Cell("use").ToLowerInvariant(),
                    Name = Cell("name"),
                    Description = Cell("description")
                };
                string where = $"offerings.csv line {i + 1} ({(o.Item.Length > 0 ? o.Item : "no item")})";
                string[] values = { o.Boss, o.Item, o.Name, o.Description };
                string[] columns = { "boss", "item", "name", "description" };
                for (int c = 0; c < values.Length; c++)
                {
                    if (values[c].Length == 0)
                    {
                        errors.Add($"{where}: {columns[c]} can't be empty.");
                    }
                }
                if (!Uses.Contains(o.Use))
                {
                    errors.Add($"{where}: use '{o.Use}' must be one of {string.Join(", ", Uses)}.");
                }
                if (!int.TryParse(Cell("amount"), out o.Amount) || o.Amount < 1)
                {
                    errors.Add($"{where}: amount must be a whole number of at least 1.");
                }
                else if (o.Use != UseAltar && o.Amount != 1)
                {
                    errors.Add($"{where}: a {o.Use} item is corrupted one at a time, so amount must be 1.");
                }
                if (offerings.Any(x => x.Item == o.Item))
                {
                    errors.Add($"{where}: {o.Item} is listed twice.");
                }
                offerings.Add(o);
            }
            return offerings;
        }
    }
}
