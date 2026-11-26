using System.Collections.Generic;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Adds corruption shards to creature drops. Runs on the machine that simulates the creature, the same place
    ///     vanilla rolls its drops, so it works the same in single player, hosted and dedicated-server games.
    ///     The shared shards stay in the creature's drop list (first-kill 1-3, corrupted boss 3-5, starred creatures, rip
    ///     multipliers) and get a shared loot beam. The shards a boss kill gives each nearby player (Krogon, 2026-11-23)
    ///     are not in the list: <see cref="DropBossShards" /> drops them, one stack per player, personal to that player.
    /// </summary>
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    internal static class ShardDrops
    {
        private static void Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
        {
            if (__result == null)
            {
                return;
            }
            try
            {
                var character = __instance.m_character;
                if (!character || character.IsPlayer() || character.IsTamed())
                {
                    return;
                }

                bool isBoss = character.IsBoss();
                string key = character.m_defeatSetGlobalKey;
                // The drop list is built before the game records the defeat, so this is still false on the first kill.
                bool alreadyDefeated = isBoss && !string.IsNullOrEmpty(key) && ZoneSystem.instance &&
                                       ZoneSystem.instance.GetGlobalKey(key);

                // A corrupted boss drops its guaranteed range on every kill (design: 3-5).
                var settings = CurrentSettings();
                if (Maddened.IsMaddened(character))
                {
                    // Rip creatures: the starred chances times the rip multiplier (user: double, 2026-10-28).
                    settings.OneStarChance = Mathf.Min(1f, settings.OneStarChance * CorruptionConfig.RipKillShardMultiplier.Value);
                    settings.TwoStarChance = Mathf.Min(1f, settings.TwoStarChance * CorruptionConfig.RipKillShardMultiplier.Value);
                }
                int shards = isBoss && CorruptedBosses.IsCorrupted(character)
                    ? Random.Range(CorruptionConfig.CorruptedShardsMin.Value, Mathf.Max(CorruptionConfig.CorruptedShardsMin.Value, CorruptionConfig.CorruptedShardsMax.Value) + 1)
                    : ShardRules.ShardsFor(isBoss, character.GetLevel(), alreadyDefeated, settings,
                        () => Random.value * 0.99999f);
                if (shards <= 0)
                {
                    return;
                }

                var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab) : null;
                if (prefab)
                {
                    __result.Add(new KeyValuePair<GameObject, int>(prefab, shards));
                }
            }
            catch (System.Exception ex)
            {
                Safe.Report("add corruption shards to drops", ex);
            }
        }

        /// <summary>
        ///     Every boss kill: <c>BossShardsPerPlayer</c> shards for each player near the boss, dropped at the boss, each
        ///     stack locked to its player for the first seconds (<see cref="PersonalLoot" />). With nobody near, one roll's
        ///     worth (<see cref="ShardRules.BossPerPlayer" />) with no owner. Needs no loot tables. Called once per death
        ///     on the creature's owner (<c>LootDrops</c>, which knows both of the game's death paths).
        /// </summary>
        public static void DropBossShards(Vector3 position, List<Player> nearby, bool cheated)
        {
            int perPlayer = CorruptionConfig.BossShardsPerPlayer.Value;
            var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab) : null;
            if (!prefab || perPlayer <= 0)
            {
                return;
            }
            if (nearby.Count == 0)
            {
                Drop(prefab, ShardRules.BossPerPlayer(0, perPlayer), position, 0L, cheated);
                return;
            }
            foreach (var player in nearby)
            {
                if (player)
                {
                    Drop(prefab, perPlayer, position, player.GetPlayerID(), cheated);
                }
            }
        }

        /// <summary>Drops shards in stacks up to the item's stack size, scattered like a creature's drops, locked to <paramref name="owner" /> (0 = nobody); <paramref name="cheated" /> is the kill's flag.</summary>
        private static void Drop(GameObject prefab, int count, Vector3 position, long owner, bool cheated)
        {
            var template = prefab.GetComponent<ItemDrop>();
            if (!template)
            {
                return;
            }
            int stackSize = Mathf.Max(1, template.m_itemData.m_shared.m_maxStackSize);
            while (count > 0)
            {
                int stack = Mathf.Min(count, stackSize);
                var item = template.m_itemData.Clone();
                item.m_dropPrefab = prefab;
                item.m_worldLevel = (byte)Game.m_worldLevel;
                // Like vanilla's drops: the stack carries the kill's cheated flag.
                item.m_cheated = cheated;
                PersonalLoot.Tag(Loot.Drop(item, position, stack), owner, LootBeamTable.Shard);
                count -= stack;
            }
        }

        private static ShardRules.Settings CurrentSettings()
        {
            return new ShardRules.Settings
            {
                OneStarChance = CorruptionConfig.OneStarShardChance.Value,
                TwoStarChance = CorruptionConfig.TwoStarShardChance.Value,
                BossMin = CorruptionConfig.BossShardsMin.Value,
                BossMax = CorruptionConfig.BossShardsMax.Value,
                BossFirstKillOnly = CorruptionConfig.BossShardsFirstKillOnly.Value
            };
        }
    }
}
