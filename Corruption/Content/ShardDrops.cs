using System.Collections.Generic;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Adds corruption shards to creature drops. Runs on the machine that simulates the creature, the same place
    ///     vanilla rolls its drops, so it works the same in single player, hosted and dedicated-server games.
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
