using System.Collections;
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

        /// <summary>
        ///     Adds sources to the list. Yields after each small piece of work (one prefab, one recipe...) so the
        ///     work can be spread over frames, and yields <see cref="SlicedWork.WaitFrame" /> while an asset loads.
        /// </summary>
        IEnumerable Collect(List<ItemSource> sources, CollectRun run);
    }

    /// <summary>How a collection is being run. Can switch to synchronous half-way (a console command needs the result now).</summary>
    internal sealed class CollectRun
    {
        /// <summary>Do everything now (load assets directly, never wait for a frame).</summary>
        public bool Synchronous;
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
