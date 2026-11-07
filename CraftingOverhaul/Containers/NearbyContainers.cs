using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CraftingOverhaul.Containers
{
    /// <summary>Finds containers the local player may take crafting materials from, and counts/takes items from them.</summary>
    internal static class NearbyContainers
    {
        private const float RefreshInterval = 0.5f;

        private static readonly List<Container> All = new List<Container>();
        private static readonly List<Container> Nearby = new List<Container>();
        private static readonly Dictionary<string, int> CountCache = new Dictionary<string, int>();

        private static float _lastRefresh = -1f;
        private static Vector3 _lastCenter;
        private static int _countCacheFrame = -1;

        /// <summary>
        ///     What nearby chests hold. <paramref name="heldOnly" />: only chests held for paying
        ///     (<see cref="ChestLeases.IsHeld" />), for the checks right before a payment; otherwise every usable chest,
        ///     for what the recipe list and build menu show.
        /// </summary>
        public static int Count(string name, int quality, bool matchWorldLevel, bool heldOnly)
        {
            if (_countCacheFrame != Time.frameCount)
            {
                CountCache.Clear();
                _countCacheFrame = Time.frameCount;
            }

            string key = name + "|" + quality + "|" + matchWorldLevel + "|" + heldOnly;
            if (CountCache.TryGetValue(key, out int cached))
            {
                return cached;
            }

            int total = 0;
            foreach (var container in GetNearby())
            {
                if (!heldOnly || ChestLeases.IsHeld(container))
                {
                    total += container.GetInventory().CountItems(name, quality, matchWorldLevel);
                }
            }
            if (!heldOnly)
            {
                CountCache[key] = total;
            }
            return total;
        }

        /// <summary>
        ///     Takes <paramref name="amount" /> from held chests (closest first), all or nothing: returns
        ///     <paramref name="amount" />, or 0 (and takes nothing) when the held chests hold less. Held chests are ours
        ///     alone (owned and marked in use, contents current), so nobody else changes them meanwhile; the change is
        ///     saved and synced by the game.
        /// </summary>
        public static int Remove(string name, int amount, int quality, bool worldLevelBased)
        {
            var held = new List<Container>();
            var stock = new List<int>();
            foreach (var container in GetNearby())
            {
                if (!ChestLeases.IsHeld(container))
                {
                    continue;
                }
                int available = container.GetInventory().CountItems(name, quality, worldLevelBased);
                if (available > 0)
                {
                    held.Add(container);
                    stock.Add(available);
                }
            }
            var take = PayPlan.Take(amount, stock);
            CountCache.Clear();
            if (take == null)
            {
                Jotunn.Logger.LogWarning($"Craft from chests: held chests have fewer than {amount} {name}; nothing taken from them.");
                return 0;
            }
            for (int i = 0; i < held.Count; i++)
            {
                if (take[i] > 0)
                {
                    held[i].GetInventory().RemoveItem(name, take[i], quality, worldLevelBased);
                }
            }
            return amount;
        }

        /// <summary>Usable chests in range, closest first.</summary>
        public static List<Container> Usable() => new List<Container>(GetNearby());

        private static List<Container> GetNearby()
        {
            var player = Player.m_localPlayer;
            if (!player)
            {
                Nearby.Clear();
                return Nearby;
            }

            var station = player.GetCurrentCraftingStation();
            Vector3 center = station ? station.transform.position : player.transform.position;

            if (Time.time - _lastRefresh < RefreshInterval && (center - _lastCenter).sqrMagnitude < 1f)
            {
                return Nearby;
            }
            _lastRefresh = Time.time;
            _lastCenter = center;

            Nearby.Clear();
            All.RemoveAll(c => !c);

            float range = ModConfig.ContainerRange.Value;
            float rangeSqr = range * range;
            long playerId = player.GetPlayerID();
            var excluded = ModConfig.ExcludedContainerNames();
            var openContainer = InventoryGui.instance ? InventoryGui.instance.m_currentContainer : null;

            foreach (var container in All)
            {
                if ((container.transform.position - center).sqrMagnitude <= rangeSqr &&
                    IsUsable(container, playerId, excluded, openContainer))
                {
                    Nearby.Add(container);
                }
            }

            // Closest first, so materials are taken from the nearest chests.
            Nearby.Sort((a, b) => (a.transform.position - center).sqrMagnitude
                .CompareTo((b.transform.position - center).sqrMagnitude));
            CountCache.Clear();
            return Nearby;
        }

        private static bool IsUsable(Container container, long playerId, HashSet<string> excluded, Container openContainer)
        {
            var nview = container.m_nview;
            if (!nview || !nview.IsValid() || container.GetInventory() == null)
            {
                return false;
            }

            // Ships and carts: taking ownership would also move their physics, which disrupts whoever is sailing or pulling.
            if (container.m_rootObjectOverride)
            {
                return false;
            }

            // Containers carried by a player, e.g. AdventureBackpacks' backpack: that mod counts and spends them itself.
            if (container.GetComponentInParent<Player>())
            {
                return false;
            }

            if (container.GetComponent<TombStone>())
            {
                return false;
            }

            if (excluded.Count > 0 && excluded.Contains(Utils.GetPrefabName(container.gameObject)))
            {
                return false;
            }

            // Someone else has it open or is paying from it. In use by us (our chest window, or held for paying) is fine:
            // only the owner's own mark counts there.
            bool inUseByUs = container == openContainer || (nview.IsOwner() && container.m_inUse);
            if (!inUseByUs && (nview.IsOwner() ? container.m_inUse : nview.GetZDO().GetInt(ZDOVars.s_inUse) == 1))
            {
                return false;
            }

            switch (container.m_privacy)
            {
                case Container.PrivacySetting.Public:
                    break;
                case Container.PrivacySetting.Private:
                    if (!container.m_piece || container.m_piece.GetCreator() != playerId)
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            if (container.m_checkGuardStone && !PrivateArea.CheckAccess(container.transform.position, 0f, false))
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
        private static class ContainerAwake_Patch
        {
            private static void Postfix(Container __instance)
            {
                if (__instance.m_nview && __instance.m_nview.GetZDO() != null)
                {
                    All.Add(__instance);
                }
            }
        }
    }
}
