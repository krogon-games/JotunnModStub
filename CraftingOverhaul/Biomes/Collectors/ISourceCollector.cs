using System.Collections.Generic;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Reads one kind of game data and turns it into <see cref="ItemSource" />s. Each collector is independent,
    ///     so a rule can be changed, disabled or inspected (see the co_dump output) on its own.
    /// </summary>
    internal interface ISourceCollector
    {
        string Name { get; }

        void Collect(List<ItemSource> sources);
    }

    internal static class CollectorUtil
    {
        /// <summary>Adds walker results with the collector's biome and name filled in.</summary>
        public static void AddAll(List<ItemSource> into, IEnumerable<ItemSource> found, Biome? biome, string collector,
            string requiredKey = null)
        {
            foreach (var source in found)
            {
                source.Base = biome;
                source.Collector = collector;
                if (string.IsNullOrEmpty(source.RequiredKey))
                {
                    source.RequiredKey = requiredKey;
                }
                into.Add(source);
            }
        }
    }
}
