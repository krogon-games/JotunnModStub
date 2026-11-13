using System.Collections;
using System.Collections.Generic;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>World-generation vegetation: trees, rocks, ore deposits, berry bushes and other pickables, per biome.</summary>
    internal sealed class VegetationCollector : ISourceCollector
    {
        public string Name => "Vegetation";

        public IEnumerable Collect(List<ItemSource> sources, CollectRun run)
        {
            if (!ZoneSystem.instance)
            {
                yield break;
            }
            foreach (var veg in ZoneSystem.instance.m_vegetation)
            {
                if (!veg.m_enable || !veg.m_prefab)
                {
                    continue;
                }
                var biome = BiomeMapping.Earliest(veg.m_biome);
                if (biome == null)
                {
                    continue;
                }
                CollectorUtil.AddAll(sources, PrefabWalker.Walk(veg.m_prefab), biome, Name);
                yield return null;
            }
        }
    }
}
