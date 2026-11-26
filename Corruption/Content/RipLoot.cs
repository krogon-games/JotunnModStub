using System.Collections.Generic;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The chest a destroyed rip heart leaves: one of the rip biome's own vanilla chests (from
    ///     <c>Corruption/Data/rip_loot.csv</c>, weighted by how often each appears in that biome) with its normal loot,
    ///     plus corruption shards (user: 1-3, 2026-10-28); per player near the rip one roll of the orb table and one
    ///     Magic/Rare gear roll, at the rip multiplier (Krogon, 2026-11-19 and 2026-11-23); and one guaranteed map of the
    ///     rip's biome (Krogon, 2026-11-23: each rip pays back its map).
    /// </summary>
    internal static class RipLoot
    {
        private static List<RipLootTable.Chest> s_chests = new List<RipLootTable.Chest>();

        public static void Load()
        {
            var errors = new List<string>();
            s_chests = RipLootTable.Parse(DataFiles.Read("rip_loot.csv"), errors);
            Jotunn.Logger.LogInfo($"Rip loot: {s_chests.Count} chest entries loaded.");
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
            // Its vanilla filling doesn't roll as a dungeon chest: the rip's own orb rolls follow below.
            GameObject instance;
            OrbDrops.PlacingRipChest = true;
            try
            {
                instance = Object.Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            }
            finally
            {
                OrbDrops.PlacingRipChest = false;
            }
            Jotunn.Logger.LogInfo($"Rip chest {chest} placed at {position} for biome {biome}.");
            var container = instance.GetComponent<Container>();
            var shard = ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab);
            int min = CorruptionConfig.RipChestShardsMin.Value;
            int shards = Random.Range(min, Mathf.Max(min, CorruptionConfig.RipChestShardsMax.Value) + 1);
            if (container && shard && shards > 0)
            {
                container.GetInventory().AddItem(shard, shards);
                Jotunn.Logger.LogInfo($"Rip chest: added {shards} corruption shards.");
            }
            else
            {
                Jotunn.Logger.LogWarning($"Rip chest: no shards added (container {(container ? "found" : "missing")}, shard item {(shard ? "found" : "missing")}).");
            }
            // "Players near the rip": the game's own nearby-players range, as for kills. Maps have no tiers yet: tier 1.
            var nearby = new List<Player>();
            Player.GetPlayersInRange(heartPosition, Game.instance ? Game.instance.m_difficultyScaleRange : 100f, nearby);
            int rolls = DeathLoot.RollsFor(nearby.Count);
            var sources = new[] { "rip_tier_1" };
            Safe.Run("add orbs to the rip chest", () =>
            {
                var orbs = new List<string>();
                for (int i = 0; i < rolls; i++)
                {
                    orbs.AddRange(OrbDrops.Roll(sources));
                }
                int added = orbs.Count > 0 ? OrbDrops.AddTo(container, orbs) : 0;
                Jotunn.Logger.LogInfo($"Rip chest: {rolls} orb roll(s), {added} orb(s) added" + (orbs.Count > 0 ? $" ({string.Join(", ", orbs)})." : "."));
            });
            Safe.Run("add gear to the rip chest", () =>
            {
                // The same Magic/Rare roll as a kill, per player (Krogon, 2026-11-23); uniques come with ROADMAP L6.
                var gear = new List<string>();
                for (int i = 0; i < rolls; i++)
                {
                    foreach (var rarity in Loot.Drops.Roll(sources, () => Random.value * 0.99999f))
                    {
                        if (rarity != "magic" && rarity != "rare")
                        {
                            continue;
                        }
                        var item = Loot.Create(rarity, biome);
                        if (item != null)
                        {
                            AddOrDrop(container, item, position);
                            gear.Add(rarity);
                        }
                    }
                }
                Jotunn.Logger.LogInfo($"Rip chest: {rolls} gear roll(s), {gear.Count} item(s)" + (gear.Count > 0 ? $" ({string.Join(", ", gear)})." : "."));
            });
            Safe.Run("add the map to the rip chest", () =>
            {
                var map = MapDrops.Prefab(MapDrops.MapFor(biome));
                if (!map)
                {
                    Jotunn.Logger.LogWarning($"Rip chest: no map item for biome '{biome}'.");
                    return;
                }
                var item = map.GetComponent<ItemDrop>().m_itemData.Clone();
                item.m_dropPrefab = map;
                item.m_stack = 1;
                AddOrDrop(container, item, position);
                Jotunn.Logger.LogInfo($"Rip chest: added a {map.name}.");
            });
        }

        /// <summary>Into the chest, or on the ground beside it when it's full: the chest's loot is never lost.</summary>
        private static void AddOrDrop(Container container, ItemDrop.ItemData item, Vector3 position)
        {
            var inventory = container ? container.GetInventory() : null;
            if (inventory != null && inventory.AddItem(item))
            {
                return;
            }
            Loot.Drop(item, position + Vector3.up);
            Jotunn.Logger.LogInfo($"Rip chest: full, {item.m_dropPrefab?.name} dropped beside it.");
        }
    }
}
