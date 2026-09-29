using System.Collections.Generic;
using UnityEngine;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>Creatures (and fish) from the world spawn tables, with their drops, per biome.</summary>
    internal sealed class SpawnCollector : ISourceCollector
    {
        public string Name => "Spawns";

        public void Collect(List<ItemSource> sources)
        {
            foreach (var list in FindSpawnLists())
            {
                foreach (var spawn in list.m_spawners)
                {
                    if (!spawn.m_enabled || !spawn.m_prefab)
                    {
                        continue;
                    }
                    var biome = BiomeMapping.Earliest(spawn.m_biome);
                    if (biome == null)
                    {
                        continue;
                    }
                    CollectorUtil.AddAll(sources, PrefabWalker.Walk(spawn.m_prefab), biome, Name,
                        string.IsNullOrEmpty(spawn.m_requiredGlobalKey) ? null : spawn.m_requiredGlobalKey);
                }
            }
        }

        internal static IEnumerable<SpawnSystemList> FindSpawnLists()
        {
            var seen = new HashSet<SpawnSystemList>();
            var zoneCtrl = ZoneSystem.instance ? ZoneSystem.instance.m_zoneCtrlPrefab : null;
            if (zoneCtrl)
            {
                foreach (var system in zoneCtrl.GetComponentsInChildren<SpawnSystem>(true))
                {
                    foreach (var list in system.m_spawnLists)
                    {
                        if (list && seen.Add(list))
                        {
                            yield return list;
                        }
                    }
                }
            }

            // Mods may register extra lists elsewhere; include any loaded ones.
            foreach (var list in Resources.FindObjectsOfTypeAll<SpawnSystemList>())
            {
                if (list && seen.Add(list))
                {
                    yield return list;
                }
            }
        }
    }
}
