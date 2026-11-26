using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The game-side rules of personal loot (see <see cref="PersonalLoot" />): a drop locked to someone else can't be
    ///     picked up by any route, can't be interacted with, and merges with nothing. The hover text part is in
    ///     <see cref="LootHoverText" />. Each patch lets vanilla run if our check fails.
    /// </summary>
    internal static class PersonalLootPatches
    {
        /// <summary>
        ///     Every pickup path (the E key, auto-pickup, console) ends in this method, before any other mod's
        ///     inventory hook sees the item. A drop that is Theirs for the picking player is refused as if not there.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Pickup))]
        private static class Pickup
        {
            // The item is read by position (__0), not by name, so a renamed argument in a newer game doesn't break it.
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(Humanoid __instance, GameObject __0, ref bool __result)
            {
                bool refuse = false;
                try
                {
                    var player = __instance as Player;
                    var drop = player && __0 ? __0.GetComponent<ItemDrop>() : null;
                    if (drop && PersonalLoot.Enabled)
                    {
                        long viewer = player == Player.m_localPlayer ? PersonalLoot.LocalPlayerId : player.GetPlayerID();
                        refuse = PersonalLoot.StateOf(drop, viewer) == LootLockState.Theirs;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("check who may pick a drop up", ex);
                    refuse = false;
                }
                if (refuse)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        /// <summary>The E key on a drop locked to someone else does nothing, quietly (the item is invisible to that player).</summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Interact))]
        private static class Interact
        {
            private static bool Prefix(ItemDrop __instance, ref bool __result)
            {
                bool refuse = false;
                try
                {
                    refuse = PersonalLoot.Enabled && PersonalLoot.IsHiddenFromMe(__instance);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("check who may use a drop", ex);
                }
                if (refuse)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        ///     Vanilla merges nearby same-name stacks it owns (only with more than 200 drops loaded) and destroys one of
        ///     them, which would lose its owner or beam. Merging only happens when neither carries our keys.
        /// </summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.AutoStackItems))]
        private static class AutoStack
        {
            private static bool Prefix(ItemDrop __instance)
            {
                try
                {
                    if (!PersonalLoot.Enabled || !__instance)
                    {
                        return true;
                    }
                    bool thisTagged = PersonalLoot.IsTagged(__instance);
                    if (!PersonalLootRules.MayAutoStack(thisTagged, false))
                    {
                        // Never merges: checked once, not again every 10 seconds.
                        __instance.m_haveAutoStacked = true;
                        return false;
                    }
                    // The same lookup vanilla uses: items within 4 m, through their rigidbodies.
                    foreach (var collider in Physics.OverlapSphere(__instance.transform.position, 4f, LayerMask.GetMask("item")))
                    {
                        var body = collider.attachedRigidbody;
                        var other = body ? body.GetComponent<ItemDrop>() : null;
                        if (other && other != __instance && !PersonalLootRules.MayAutoStack(thisTagged, PersonalLoot.IsTagged(other)))
                        {
                            return false;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("keep personal drops from merging", ex);
                }
                return true;
            }
        }

        /// <summary>
        ///     A new or loaded drop on any game: if its ZDO carries our keys, its view starts (hiding, beam). A shard the
        ///     game drops from a creature's own drop list (see <see cref="CreatureDrops" />) gets a shared beam here.
        /// </summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
        private static class Awake
        {
            private static void Postfix(ItemDrop __instance)
            {
                Safe.Run("start a drop's loot beam", () =>
                {
                    if (!PersonalLoot.Enabled || !__instance || !__instance.m_nview || !__instance.m_nview.IsValid())
                    {
                        return;
                    }
                    if (PersonalLoot.HasTags(__instance.m_nview.GetZDO()))
                    {
                        PersonalLoot.Attach(__instance);
                        return;
                    }
                    if (PersonalLoot.CreatureDropDepth > 0 && __instance.m_nview.IsOwner() &&
                        PersonalLoot.BeamRowForPrefab(Utils.GetPrefabName(__instance.gameObject)) == LootBeamTable.Shard)
                    {
                        PersonalLoot.Tag(__instance, 0L, LootBeamTable.Shard);
                    }
                });
            }
        }

        /// <summary>
        ///     Marks "the game is dropping a creature's loot" while it runs, so the shards it spawns from the drop list
        ///     get a beam (<see cref="Awake" />). Matched by name only: its arguments changed between game versions.
        /// </summary>
        [HarmonyPatch(typeof(CharacterDrop), "DropItems")]
        private static class CreatureDrops
        {
            private static void Prefix() => PersonalLoot.CreatureDropDepth++;

            private static void Finalizer()
            {
                if (PersonalLoot.CreatureDropDepth > 0)
                {
                    PersonalLoot.CreatureDropDepth--;
                }
            }
        }
    }
}
