using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Adds the boss's essence to its drops on every kill. Runs on the machine that simulates the boss, where vanilla
    ///     rolls its drops, so it works the same in single player, hosted and dedicated-server games.
    /// </summary>
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    internal static class EssenceDrops
    {
        private static void Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
        {
            if (__result == null || !CorruptionConfig.EssencesEnabled.Value)
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
                if (!EssenceItems.ByBoss.TryGetValue(Utils.GetPrefabName(character.gameObject), out var essence))
                {
                    return;
                }
                var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(essence.Item) : null;
                if (prefab)
                {
                    __result.Add(new KeyValuePair<GameObject, int>(prefab, essence.PerKill));
                }
            }
            catch (System.Exception ex)
            {
                Safe.Report("add boss essences to drops", ex);
            }
        }
    }
}
