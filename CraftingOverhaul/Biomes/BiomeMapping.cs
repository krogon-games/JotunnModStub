namespace CraftingOverhaul.Biomes
{
    internal static class BiomeMapping
    {
        // Plain parallel arrays: tuples would need System.ValueTuple.dll, which Valheim doesn't ship.
        private static readonly Heightmap.Biome[] GameBiomes =
        {
            Heightmap.Biome.Meadows,
            Heightmap.Biome.BlackForest,
            Heightmap.Biome.Swamp,
            Heightmap.Biome.Ocean,
            Heightmap.Biome.Mountain,
            Heightmap.Biome.Plains,
            Heightmap.Biome.Mistlands,
            Heightmap.Biome.AshLands,
            Heightmap.Biome.DeepNorth
        };

        private static readonly Biome[] Tiers =
        {
            Biome.Meadows,
            Biome.BlackForest,
            Biome.Swamp,
            Biome.Ocean,
            Biome.Mountain,
            Biome.Plains,
            Biome.Mistlands,
            Biome.Ashlands,
            Biome.DeepNorth
        };

        /// <summary>The earliest biome in a set of game biome flags, or null when none is set.</summary>
        public static Biome? Earliest(Heightmap.Biome flags)
        {
            // Entries that apply in every land biome (e.g. raid spawn lists) say nothing about where something is found.
            if (IsEverywhere(flags))
            {
                return null;
            }
            for (int i = 0; i < GameBiomes.Length; i++)
            {
                if ((flags & GameBiomes[i]) != 0)
                {
                    return Tiers[i];
                }
            }
            return null;
        }

        private const Heightmap.Biome AllLand = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest |
                                                Heightmap.Biome.Swamp | Heightmap.Biome.Mountain |
                                                Heightmap.Biome.Plains | Heightmap.Biome.Mistlands |
                                                Heightmap.Biome.AshLands | Heightmap.Biome.DeepNorth;

        public static bool IsEverywhere(Heightmap.Biome flags)
        {
            return (flags & AllLand) == AllLand;
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
