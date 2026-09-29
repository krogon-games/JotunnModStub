namespace CraftingOverhaul.Biomes
{
    internal static class BiomeMapping
    {
        private static readonly (Heightmap.Biome Game, Biome Tier)[] Map =
        {
            (Heightmap.Biome.Meadows, Biome.Meadows),
            (Heightmap.Biome.BlackForest, Biome.BlackForest),
            (Heightmap.Biome.Swamp, Biome.Swamp),
            (Heightmap.Biome.Ocean, Biome.Ocean),
            (Heightmap.Biome.Mountain, Biome.Mountain),
            (Heightmap.Biome.Plains, Biome.Plains),
            (Heightmap.Biome.Mistlands, Biome.Mistlands),
            (Heightmap.Biome.AshLands, Biome.Ashlands),
            (Heightmap.Biome.DeepNorth, Biome.DeepNorth)
        };

        /// <summary>The earliest biome in a set of game biome flags, or null when none is set.</summary>
        public static Biome? Earliest(Heightmap.Biome flags)
        {
            foreach (var entry in Map)
            {
                if ((flags & entry.Game) != 0)
                {
                    return entry.Tier;
                }
            }
            return null;
        }

        public static string Label(Biome biome)
        {
            switch (biome)
            {
                case Biome.BlackForest: return "Black Forest";
                case Biome.DeepNorth: return "Deep North";
                default: return biome.ToString();
            }
        }
    }
}
