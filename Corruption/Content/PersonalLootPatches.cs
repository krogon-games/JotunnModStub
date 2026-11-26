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

        /// <summary>
        ///     The lock no longer depends only on the hidden view: vanilla auto-pickup calls <c>RequestOwn</c> on a drop it
        ///     can't take yet, and <c>RPC_RequestOwn</c> (not patched, so the lock's owner always gets the item) hands
        ///     ownership to anyone. A drop locked to another player never asks for ownership from this game.
        /// </summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.RequestOwn))]
        private static class RequestOwn
        {
            private static bool Prefix(ItemDrop __instance)
            {
                bool skip = false;
                try
                {
                    skip = IsLockedAway(__instance);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("keep a locked drop from asking for ownership", ex);
                    skip = false;
                }
                return !skip;
            }
        }

        /// <summary>
        ///     A drop locked to another player can't be picked up from this game, whatever its view did: auto-pickup then
        ///     goes to <c>RequestOwn</c> (skipped above) instead of pulling the item, and creatures run by other games can't
        ///     eat it. A refusal also stops the game's pickup retry loop (<c>PickupUpdate</c>, started by
        ///     <c>ItemDrop.Pickup</c> when this returns false), so a loop can't hand the item over by itself once the lock
        ///     ends; the player just picks it up again then.
        /// </summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.CanPickup))]
        private static class CanPickup
        {
            private static bool Prefix(ItemDrop __instance, ref bool __result)
            {
                bool refuse = false;
                try
                {
                    refuse = IsLockedAway(__instance);
                    if (refuse)
                    {
                        __instance.CancelInvoke("PickupUpdate");
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("keep a locked drop from being picked up", ex);
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

        /// <summary>All the game-member access: true when this drop is locked to another player, as seen from this game.</summary>
        private static bool IsLockedAway(ItemDrop drop) => PersonalLoot.Enabled && drop && PersonalLoot.IsHiddenFromMe(drop);

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
        ///     them, which would lose its owner or beam. A drop that carries our keys never merges, and neither does one
        ///     with a tagged neighbour vanilla would merge it with.
        /// </summary>
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.AutoStackItems))]
        private static class AutoStack
        {
            private static bool Prefix(ItemDrop __instance)
            {
                bool skip = false;
                try
                {
                    skip = PersonalLoot.Enabled && __instance && ShouldSkip(__instance);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("keep personal drops from merging", ex);
                    skip = false;
                }
                return !skip;
            }

            /// <summary>All the game-member access: true when vanilla's merge must not run for this drop.</summary>
            private static bool ShouldSkip(ItemDrop drop)
            {
                bool thisTagged = PersonalLoot.IsTagged(drop);
                if (!PersonalLootRules.MayAutoStack(thisTagged, false))
                {
                    // Never merges: checked once, not again every 10 seconds.
                    drop.m_haveAutoStacked = true;
                    return true;
                }
                var data = drop.m_itemData;
                // The same lookup and the same conditions vanilla uses: items within 4 m, through their rigidbodies.
                foreach (var collider in Physics.OverlapSphere(drop.transform.position, 4f, LayerMask.GetMask("item")))
                {
                    var body = collider.attachedRigidbody;
                    var other = body ? body.GetComponent<ItemDrop>() : null;
                    if (other && other != drop && other.m_itemData.m_shared.m_autoStack && other.m_nview && other.m_nview.IsValid() &&
                        other.m_nview.IsOwner() && other.m_itemData.m_shared.m_name == data.m_shared.m_name &&
                        other.m_itemData.m_quality == data.m_quality &&
                        !PersonalLootRules.MayAutoStack(thisTagged, PersonalLoot.IsTagged(other)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        ///     The crosshair looks through a drop hidden from this player. Vanilla stops at the first thing the ray hits, so
        ///     an invisible drop would block hovering whatever is behind it; when that is the result, the loop is run again
        ///     skipping hidden drops. Arguments by position (they are <c>out</c> in the game).
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.FindHoverObject))]
        private static class FindHover
        {
            private static void Postfix(Player __instance, ref GameObject __0, ref Character __1)
            {
                try
                {
                    if (PersonalLoot.Enabled && __0)
                    {
                        Redo(__instance, ref __0, ref __1);
                    }
                }
                catch (System.Exception ex)
                {
                    // Vanilla's result stays (a hidden drop then just shows no hover).
                    Safe.Report("look through hidden drops", ex);
                }
            }

            /// <summary>Our own copy of the game's hit comparer (nearest first), so no game type is needed for it.</summary>
            private sealed class ByDistance : System.Collections.Generic.IComparer<RaycastHit>
            {
                public static readonly ByDistance Instance = new ByDistance();

                public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance);
            }

            private static ItemDrop DropOf(GameObject go)
            {
                var drop = go.GetComponent<ItemDrop>();
                if (drop)
                {
                    return drop;
                }
                var body = go.GetComponent<Rigidbody>();
                return body ? body.GetComponent<ItemDrop>() : null;
            }

            private static bool IsHiddenDrop(Rigidbody body)
            {
                var drop = body ? body.GetComponent<ItemDrop>() : null;
                return drop && PersonalLoot.IsHiddenFromMe(drop);
            }

            /// <summary>Vanilla's <c>FindHoverObject</c> loop (game 1.0.14), plus skipping hidden drops. Starts only when vanilla landed on one.</summary>
            private static void Redo(Player player, ref GameObject hover, ref Character hoverCreature)
            {
                var first = DropOf(hover);
                if (!first || !PersonalLoot.IsHiddenFromMe(first))
                {
                    return;
                }
                GameObject newHover = null;
                Character newCreature = null;
                var camera = GameCamera.instance.transform;
                var hits = player.m_raycastHoverHits;
                int count = Physics.RaycastNonAlloc(camera.position, camera.forward, hits, 50f, player.m_interactMask);
                System.Array.Sort(hits, 0, count, ByDistance.Instance);
                for (int i = 0; i < count; i++)
                {
                    var hit = hits[i];
                    var body = hit.collider.attachedRigidbody;
                    if (body && body.gameObject == player.gameObject)
                    {
                        continue;
                    }
                    if (IsHiddenDrop(body))
                    {
                        continue;
                    }
                    if (newCreature == null)
                    {
                        var character = body ? body.GetComponent<Character>() : hit.collider.GetComponent<Character>();
                        if (character != null && (!character.GetBaseAI() || !character.GetBaseAI().IsSleeping()) &&
                            !ParticleMist.IsMistBlocked(player.GetCenterPoint(), character.GetCenterPoint()))
                        {
                            newCreature = character;
                        }
                    }
                    var hoverable = hit.collider.GetComponentInParent<Hoverable>();
                    if (Vector3.Distance(player.m_eye.position, hit.point) < player.m_maxInteractDistance + (hoverable?.GetHoverOffset() ?? 0f))
                    {
                        if (hit.collider.GetComponent<Hoverable>() != null)
                        {
                            newHover = hit.collider.gameObject;
                        }
                        else if (body)
                        {
                            newHover = body.gameObject;
                        }
                        else
                        {
                            newHover = hit.collider.gameObject;
                        }
                    }
                    break;
                }
                hover = newHover;
                hoverCreature = newCreature;
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
