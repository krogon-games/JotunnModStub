using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the map affix table (<c>Corruption/Data/map_affixes.csv</c>, Krogon's rips round decisions 7-10,
    ///     2026-11-26). Every map affix is danger only. <c>approved</c> rows roll on maps; <c>later</c> rows are designed but
    ///     not built and never roll. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class MapAffixTable
    {
        private static readonly string[] RequiredColumns =
            { "id", "status", "name", "danger", "weight", "wave_multiplier", "damage_taken", "maddened_factor", "fire_share" };

        private static readonly string[] Statuses = { "approved", "later" };

        public sealed class Affix
        {
            public string Id;
            public string Status;
            public string Name;
            /// <summary>What it does to the rip, for the tooltip.</summary>
            public string Danger;
            /// <summary>Roll weight among the affixes an orb can add.</summary>
            public double Weight;
            /// <summary>Multiplies the tier's waves; 1 = no effect.</summary>
            public double WaveMultiplier = 1;
            /// <summary>Multiplies the damage rip creatures take; 1 = no effect.</summary>
            public double DamageTaken = 1;
            /// <summary>Multiplies the maddened bonuses (damage and speed); 1 = no effect.</summary>
            public double MaddenedFactor = 1;
            /// <summary>Share of each hit's damage added as fire; 0 = none.</summary>
            public double FireShare;

            public bool Approved => Status == "approved";
        }

        public readonly List<Affix> Affixes = new List<Affix>();

        /// <summary>
        ///     Affix ids an admin has switched off (Krogon, 2026-11-27: "make the option to disable some affixes from being able
        ///     to roll - until we catch up and create the mechanics"). They never roll; maps that already carry one keep it.
        /// </summary>
        public readonly HashSet<string> Disabled = new HashSet<string>();

        /// <summary>The affixes orbs can roll: approved rows that aren't switched off.</summary>
        public IEnumerable<Affix> Rollable => Affixes.Where(a => a.Approved && !Disabled.Contains(a.Id));

        /// <summary>
        ///     Replaces <see cref="Disabled" /> from a comma- or space-separated list of ids (case and spaces ignored).
        ///     Returns the ids that aren't in the table, so the caller can warn about typos.
        /// </summary>
        public List<string> SetDisabled(string list)
        {
            Disabled.Clear();
            var unknown = new List<string>();
            foreach (var raw in (list ?? "").Split(new[] { ',', ';', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string id = raw.Trim().ToLowerInvariant();
                if (id.Length == 0)
                {
                    continue;
                }
                if (Find(id) == null)
                {
                    unknown.Add(id);
                }
                else
                {
                    Disabled.Add(id);
                }
            }
            return unknown;
        }

        /// <summary>The affix with this id (any status), or null.</summary>
        public Affix Find(string id)
        {
            return id == null ? null : Affixes.FirstOrDefault(a => a.Id == id);
        }

        public static MapAffixTable Parse(string csv, List<string> errors)
        {
            var table = new MapAffixTable();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("map_affixes.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"map_affixes.csv: missing column '{column}'.");
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

                var a = new Affix
                {
                    Id = Cell("id"),
                    Status = Cell("status").ToLowerInvariant(),
                    Name = Cell("name"),
                    Danger = Cell("danger")
                };
                string where = $"map_affixes.csv line {i + 1} ({(a.Id.Length > 0 ? a.Id : "no id")})";
                if (a.Id.Length == 0 || a.Id.Any(c => !(c >= 'a' && c <= 'z')))
                {
                    errors.Add($"{where}: id must be lowercase letters only.");
                }
                else if (table.Affixes.Any(x => x.Id == a.Id))
                {
                    errors.Add($"{where}: id is listed twice.");
                }
                if (!Statuses.Contains(a.Status))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)} (approved = rolls on maps, later = designed, never rolls).");
                }
                if (a.Name.Length == 0 || a.Danger.Length == 0)
                {
                    errors.Add($"{where}: name and danger can't be empty.");
                }
                string weight = Cell("weight");
                if (weight.Length > 0)
                {
                    if (!Number(weight, out a.Weight) || a.Weight <= 0)
                    {
                        errors.Add($"{where}: weight must be a number above 0.");
                    }
                }
                else if (a.Status == "approved")
                {
                    errors.Add($"{where}: an approved affix needs a weight above 0.");
                }
                double value;
                string text = Cell("wave_multiplier");
                if (text.Length > 0)
                {
                    if (!Number(text, out value) || value <= 0)
                    {
                        errors.Add($"{where}: wave_multiplier must be a number above 0 (blank = no effect).");
                    }
                    else
                    {
                        a.WaveMultiplier = value;
                    }
                }
                text = Cell("damage_taken");
                if (text.Length > 0)
                {
                    if (!Number(text, out value) || value <= 0 || value > 1)
                    {
                        errors.Add($"{where}: damage_taken must be above 0 and at most 1 (0.7 = 30% less damage; blank = no effect).");
                    }
                    else
                    {
                        a.DamageTaken = value;
                    }
                }
                text = Cell("maddened_factor");
                if (text.Length > 0)
                {
                    if (!Number(text, out value) || value < 1)
                    {
                        errors.Add($"{where}: maddened_factor must be 1 or more (2 = the maddened bonuses doubled; blank = no effect).");
                    }
                    else
                    {
                        a.MaddenedFactor = value;
                    }
                }
                text = Cell("fire_share");
                if (text.Length > 0)
                {
                    if (!Number(text, out value) || value <= 0 || value > 10)
                    {
                        errors.Add($"{where}: fire_share must be above 0 and at most 10 (0.25 = 25% of the hit as fire; blank = none).");
                    }
                    else
                    {
                        a.FireShare = value;
                    }
                }
                table.Affixes.Add(a);
            }
            return table;
        }

        private static bool Number(string value, out double number)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }
    }
}
