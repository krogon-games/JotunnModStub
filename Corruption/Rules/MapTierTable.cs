using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the map tier table (<c>Corruption/Data/map_tiers.csv</c>, Krogon's rips round decisions 6 and 6e,
    ///     2026-11-26): for each tier the map's rarity and affix count, the waves, the guaranteed 2-stars, the heart's shards,
    ///     the drop-rate source and the heart's gear floor. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class MapTierTable
    {
        public static readonly string[] Rarities = { "normal", "magic", "rare" };
        public static readonly string[] Floors = { "none", "magic", "rare" };

        private static readonly string[] RequiredColumns =
        {
            "tier", "status", "rarity", "affixes", "corrupted", "wave_multiplier", "two_star_percent", "shards_min",
            "shards_max", "drop_source", "heart_floor"
        };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        public sealed class TierRow
        {
            public int Tier;
            /// <summary>normal, magic or rare.</summary>
            public string Rarity;
            /// <summary>How many affixes a map of this tier carries.</summary>
            public int Affixes;
            /// <summary>The tier made by a shard on a full map (locked).</summary>
            public bool Corrupted;
            /// <summary>Multiplies every raid entry's most-alive-at-once and divides its spawn interval.</summary>
            public double WaveMultiplier;
            /// <summary>Chance (percent) that each wave creature is a guaranteed 2-star.</summary>
            public double TwoStarPercent;
            public int ShardsMin;
            public int ShardsMax;
            /// <summary>The <c>drop_modifiers.csv</c> source for this tier's drop rates.</summary>
            public string DropSource;
            /// <summary>Lowest rarity of each player's gear roll at the heart: none, magic or rare.</summary>
            public string HeartFloor;
        }

        public readonly List<TierRow> Rows = new List<TierRow>();

        /// <summary>The most affixes a map can carry (the highest affix count in the table).</summary>
        public int MaxAffixes => Rows.Count == 0 ? 0 : Rows.Max(r => r.Affixes);

        /// <summary>The tier of a corrupted map (the table's corrupted row), 0 when the table is empty.</summary>
        public int CorruptTier => Rows.Where(r => r.Corrupted).Select(r => r.Tier).DefaultIfEmpty(0).First();

        /// <summary>The row of a tier; tiers below 1 count as tier 1 and tiers above the last as the last.</summary>
        public TierRow Row(int tier)
        {
            if (Rows.Count == 0)
            {
                return null;
            }
            if (tier < 1)
            {
                tier = 1;
            }
            var row = Rows.FirstOrDefault(r => r.Tier == tier);
            return row ?? Rows[Rows.Count - 1];
        }

        /// <summary>
        ///     The tier a map has: the corrupted tier when corrupted, otherwise the tier whose affix count matches
        ///     (more affixes than the table has count as the most).
        /// </summary>
        /// <param name="affixCount">The map's normal affixes (corrupted-only ones don't count).</param>
        /// <param name="corrupted">
        ///     A corrupted map is one tier above its normal affixes' tier (Krogon, 2026-11-28, decision 27 on
        ///     claude/wizardly-wozniak-4aix76: "Corrupting a map always adds a corruption affix, and always bumps up the tier
        ///     of the map to the next tier"), so a corrupted full map is the corrupted tier (6) as before.
        /// </param>
        public int TierFor(int affixCount, bool corrupted)
        {
            int count = affixCount < 0 ? 0 : affixCount > MaxAffixes ? MaxAffixes : affixCount;
            var row = Rows.FirstOrDefault(r => !r.Corrupted && r.Affixes == count);
            int tier = row != null ? row.Tier : 1;
            if (!corrupted)
            {
                return tier;
            }
            int top = Rows.Count == 0 ? tier : Rows.Max(r => r.Tier);
            return tier + 1 > top ? top : tier + 1;
        }

        public static MapTierTable Parse(string csv, List<string> errors)
        {
            var table = new MapTierTable();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("map_tiers.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"map_tiers.csv: missing column '{column}'.");
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

                string where = $"map_tiers.csv line {i + 1} (tier {Cell("tier")})";
                var row = new TierRow
                {
                    Rarity = Cell("rarity").ToLowerInvariant(),
                    DropSource = Cell("drop_source"),
                    HeartFloor = Cell("heart_floor").ToLowerInvariant()
                };
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!int.TryParse(Cell("tier"), NumberStyles.Integer, CultureInfo.InvariantCulture, out row.Tier) || row.Tier < 1)
                {
                    errors.Add($"{where}: tier must be a whole number, 1 or more.");
                }
                if (!Rarities.Contains(row.Rarity))
                {
                    errors.Add($"{where}: rarity must be one of {string.Join(", ", Rarities)}.");
                }
                if (!int.TryParse(Cell("affixes"), NumberStyles.Integer, CultureInfo.InvariantCulture, out row.Affixes) || row.Affixes < 0)
                {
                    errors.Add($"{where}: affixes must be a whole number, 0 or more.");
                }
                string corrupted = Cell("corrupted").ToLowerInvariant();
                if (corrupted != "true" && corrupted != "false")
                {
                    errors.Add($"{where}: corrupted must be true or false.");
                }
                row.Corrupted = corrupted == "true";
                if (!Number(Cell("wave_multiplier"), out row.WaveMultiplier) || row.WaveMultiplier <= 0)
                {
                    errors.Add($"{where}: wave_multiplier must be a number above 0 (use a dot for decimals).");
                }
                if (!Number(Cell("two_star_percent"), out row.TwoStarPercent) || row.TwoStarPercent < 0 || row.TwoStarPercent > 100)
                {
                    errors.Add($"{where}: two_star_percent must be a number from 0 to 100.");
                }
                if (!int.TryParse(Cell("shards_min"), NumberStyles.Integer, CultureInfo.InvariantCulture, out row.ShardsMin) || row.ShardsMin < 1)
                {
                    errors.Add($"{where}: shards_min must be a whole number, 1 or more.");
                }
                if (!int.TryParse(Cell("shards_max"), NumberStyles.Integer, CultureInfo.InvariantCulture, out row.ShardsMax) || row.ShardsMax < 1)
                {
                    errors.Add($"{where}: shards_max must be a whole number, 1 or more.");
                }
                else if (row.ShardsMin > row.ShardsMax)
                {
                    errors.Add($"{where}: shards_min can't be more than shards_max.");
                }
                if (row.DropSource.Length == 0 || row.DropSource.Any(c => !(c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_')))
                {
                    errors.Add($"{where}: drop_source must be lowercase letters, digits and _ (a source of drop_modifiers.csv).");
                }
                if (!Floors.Contains(row.HeartFloor))
                {
                    errors.Add($"{where}: heart_floor must be one of {string.Join(", ", Floors)}.");
                }
                table.Rows.Add(row);
            }

            if (table.Rows.Count == 0)
            {
                errors.Add("map_tiers.csv has no rows.");
                return table;
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i].Tier != i + 1)
                {
                    errors.Add($"map_tiers.csv: tiers must run 1, 2, 3 ... in order, without gaps (row {i + 1} is tier {table.Rows[i].Tier}).");
                    break;
                }
            }
            for (int i = 1; i < table.Rows.Count; i++)
            {
                if (table.Rows[i].Affixes < table.Rows[i - 1].Affixes)
                {
                    errors.Add($"map_tiers.csv: tier {table.Rows[i].Tier} has fewer affixes than tier {table.Rows[i - 1].Tier}.");
                }
            }
            int corruptedRows = table.Rows.Count(r => r.Corrupted);
            if (corruptedRows != 1 || !table.Rows[table.Rows.Count - 1].Corrupted)
            {
                errors.Add("map_tiers.csv: exactly one row must be corrupted, and it must be the last tier.");
            }
            return table;
        }

        private static bool Number(string value, out double number)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }
    }
}
