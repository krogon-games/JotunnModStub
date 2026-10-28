using System.Collections.Generic;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The chest a destroyed rip heart leaves: one of the rip biome's own vanilla chests (from
    ///     <c>Corruption/Data/rip_loot.csv</c>, weighted by how often each appears in that biome) with its normal loot,
    ///     plus corruption shards (user: 1-3, 2026-10-28).
    /// </summary>
    internal static class RipLoot
    {
        private static List<RipLootTable.Chest> s_chests = new List<RipLootTable.Chest>();

        public static void Load()
        {
            var errors = new List<string>();
            s_chests = RipLootTable.Parse(DataFiles.Read("rip_loot.csv"), errors);
            foreach (var error in errors)
            {
                Jotunn.Logger.LogWarning(error);
            }
        }

        public static void SpawnChest(string biome, Vector3 heartPosition)
        {
            string chest = RipLootTable.Pick(s_chests, biome, () => Random.value * 0.99999f);
            var prefab = chest != null ? ZNetScene.instance.GetPrefab(chest) : null;
            if (!prefab)
            {
                Jotunn.Logger.LogWarning($"Rip loot: no chest for biome '{biome}' ({chest ?? "none in rip_loot.csv"}).");
                return;
            }
            var position = heartPosition + new Vector3(-3f, 0f, 0f);
            position.y = ZoneSystem.instance.GetGroundHeight(position);
            // The chest fills its own vanilla loot when it's created on its owner's game (Container.Awake).
            var instance = Object.Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            var container = instance.GetComponent<Container>();
            var shard = ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab);
            int min = CorruptionConfig.RipChestShardsMin.Value;
            int shards = Random.Range(min, Mathf.Max(min, CorruptionConfig.RipChestShardsMax.Value) + 1);
            if (container && shard && shards > 0)
            {
                container.GetInventory().AddItem(shard, shards);
            }
        }
    }
}
