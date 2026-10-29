using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads <c>Corruption/Data/rip_loot.csv</c> (compiled into the DLL): which vanilla chests a rip's end chest can
    ///     be, per biome, weighted by how often each appears in that biome. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class RipLootTable
    {
        public struct Chest
        {
            public string Biome;
            public string Prefab;
            public float Weight;
        }

        public static List<Chest> Parse(string csv, List<string> errors)
        {
            var chests = new List<Chest>();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("rip_loot.csv is empty.");
                return chests;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            foreach (var column in new[] { "biome", "chest", "weight" }.Where(c => !header.Contains(c)))
            {
                errors.Add($"rip_loot.csv: missing column '{column}'.");
            }
            if (errors.Count > 0)
            {
                return chests;
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
                string where = $"rip_loot.csv line {i + 1}";
                var chest = new Chest { Biome = Cell("biome"), Prefab = Cell("chest") };
                if (!MapList.Biomes.Contains(chest.Biome))
                {
                    errors.Add($"{where}: biome must be one of {string.Join(", ", MapList.Biomes)}.");
                }
                if (chest.Prefab.Length == 0)
                {
                    errors.Add($"{where}: chest can't be empty.");
                }
                if (!float.TryParse(Cell("weight"), NumberStyles.Float, CultureInfo.InvariantCulture, out chest.Weight) || chest.Weight <= 0f)
                {
                    errors.Add($"{where}: weight must be a number above 0.");
                }
                chests.Add(chest);
            }
            return chests;
        }

        /// <summary>A weighted random chest for the biome, or null if it has none. <paramref name="random" /> returns [0, 1).</summary>
        public static string Pick(IList<Chest> chests, string biome, Func<double> random)
        {
            var options = chests.Where(c => c.Biome == biome).ToList();
            float total = options.Sum(c => c.Weight);
            if (total <= 0f)
            {
                return null;
            }
            double roll = random() * total;
            foreach (var option in options)
            {
                roll -= option.Weight;
                if (roll < 0)
                {
                    return option.Prefab;
                }
            }
            return options[options.Count - 1].Prefab;
        }
    }
}
