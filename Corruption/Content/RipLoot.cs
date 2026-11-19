using System.Collections.Generic;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The chest a destroyed rip heart leaves: one of the rip biome's own vanilla chests (from
    ///     <c>Corruption/Data/rip_loot.csv</c>, weighted by how often each appears in that biome) with its normal loot,
    ///     plus corruption shards (user: 1-3, 2026-10-28) and one roll of the orb table per player near the rip, at the
    ///     rip multiplier (Krogon, 2026-11-19).
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
            Safe.Run("add orbs to the rip chest", () =>
            {
                // "Players near the rip": the game's own nearby-players range, as for kills. Maps have no tiers yet: tier 1.
                var nearby = new List<Player>();
                Player.GetPlayersInRange(heartPosition, Game.instance ? Game.instance.m_difficultyScaleRange : 100f, nearby);
                int rolls = DeathLoot.RollsFor(nearby.Count);
                var orbs = new List<string>();
                for (int i = 0; i < rolls; i++)
                {
                    orbs.AddRange(OrbDrops.Roll(new[] { "rip_tier_1" }));
                }
                int added = orbs.Count > 0 ? OrbDrops.AddTo(container, orbs) : 0;
                Jotunn.Logger.LogInfo($"Rip chest: {rolls} orb roll(s), {added} orb(s) added" + (orbs.Count > 0 ? $" ({string.Join(", ", orbs)})." : "."));
            });
        }
    }
}
