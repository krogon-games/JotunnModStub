using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the map table (<c>Corruption/Data/maps.csv</c>, compiled into the DLL): one map per biome,
    ///     its recipe (essence, biome material, shards) and text. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapList
    {
        public const string ItemPrefix = "CorruptionMap_";

        /// <summary>The game's biome names (Heightmap.Biome) a map can lead to.</summary>
        public static readonly string[] Biomes =
            { "Meadows", "BlackForest", "Swamp", "Mountain", "Plains", "Mistlands", "AshLands", "DeepNorth" };

        private static readonly string[] RequiredColumns =
            { "biome", "label", "status", "name", "essence", "material", "material_amount", "shards", "description" };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        public sealed class Map
        {
            /// <summary>Heightmap.Biome name, e.g. BlackForest.</summary>
            public string Biome;
            /// <summary>Biome name for players, e.g. Black Forest.</summary>
            public string Label;
            public string Name;
            public string Description;
            /// <summary>Essence look id from essences.csv, e.g. eikthyr.</summary>
            public string Essence;
            public string Material;
            public int MaterialAmount;
            public int Shards;

            public string Item => ItemPrefix + Biome;
            public string EssenceItem => EssenceTable.ItemPrefix + Essence;
        }

        public static List<Map> Parse(string csv, List<string> errors)
        {
            var maps = new List<Map>();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("maps.csv is empty.");
                return maps;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in RequiredColumns.Where(c => !header.Contains(c)))
            {
                errors.Add($"maps.csv: missing column '{column}'.");
            }
            if (errors.Count > 0)
            {
                return maps;
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

                var m = new Map
                {
                    Biome = Cell("biome"),
                    Label = Cell("label"),
                    Name = Cell("name"),
                    Description = Cell("description"),
                    Essence = Cell("essence"),
                    Material = Cell("material")
                };
                string where = $"maps.csv line {i + 1} ({(m.Biome.Length > 0 ? m.Biome : "no biome")})";

                if (!Biomes.Contains(m.Biome))
                {
                    errors.Add($"{where}: biome must be one of {string.Join(", ", Biomes)}.");
                }
                else if (maps.Any(x => x.Biome == m.Biome))
                {
                    errors.Add($"{where}: biome is listed twice.");
                }
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (m.Name.Length == 0 || m.Description.Length == 0 || m.Label.Length == 0)
                {
                    errors.Add($"{where}: name and description can't be empty.");
                }
                if (m.Essence.Length == 0 || m.Material.Length == 0)
                {
                    errors.Add($"{where}: essence and material can't be empty.");
                }
                if (!int.TryParse(Cell("material_amount"), NumberStyles.Integer, CultureInfo.InvariantCulture, out m.MaterialAmount) ||
                    m.MaterialAmount < 1)
                {
                    errors.Add($"{where}: material_amount must be a whole number, 1 or more.");
                }
                if (!int.TryParse(Cell("shards"), NumberStyles.Integer, CultureInfo.InvariantCulture, out m.Shards) || m.Shards < 1)
                {
                    errors.Add($"{where}: shards must be a whole number, 1 or more.");
                }
                maps.Add(m);
            }
            return maps;
        }
    }
}
