using System;
using System.Collections.Generic;

namespace CraftingOverhaul.Biomes
{
    /// <summary>Parses the BiomeMaterialOverrides setting: comma separated PrefabName:Biome pairs.</summary>
    internal static class BiomeOverrides
    {
        public static Dictionary<string, Biome> Parse(string setting)
        {
            var result = new Dictionary<string, Biome>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in (setting ?? "").Split(','))
            {
                var parts = pair.Split(':');
                if (parts.Length != 2 || parts[0].Trim().Length == 0)
                {
                    continue;
                }
                if (Enum.TryParse(parts[1].Trim(), true, out Biome biome))
                {
                    result[parts[0].Trim()] = biome;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Unknown biome '{parts[1].Trim()}' in BiomeMaterialOverrides");
                }
            }
            return result;
        }
    }
}
