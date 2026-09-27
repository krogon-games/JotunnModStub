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

        public static int Count(string name, int quality, bool matchWorldLevel)
        {
            if (_countCacheFrame != Time.frameCount)
            {
                CountCache.Clear();
                _countCacheFrame = Time.frameCount;
            }

            string key = name + "|" + quality + "|" + matchWorldLevel;
            if (CountCache.TryGetValue(key, out int cached))
            {
                return cached;
            }

            int total = 0;
            foreach (var container in GetNearby())
            {
                total += container.GetInventory().CountItems(name, quality, matchWorldLevel);
            }
            CountCache[key] = total;
            return total;
        }

        /// <summary>Removes up to <paramref name="amount"/> items from nearby containers and returns how many were removed.</summary>
        public static int Remove(string name, int amount, int quality, bool worldLevelBased)
        {
            int removed = 0;
            foreach (var container in GetNearby())
            {
                if (removed >= amount)
                {
                    break;
                }

                // Make sure we act on the latest synced contents before changing them.
                container.Load();
                var inventory = container.GetInventory();
                int available = inventory.CountItems(name, quality, worldLevelBased);
                if (available <= 0)
                {
                    continue;
                }

                int take = Mathf.Min(available, amount - removed);
                // Only the owner may write the container's contents; the change is saved and synced by the game.
                container.m_nview.ClaimOwnership();
                inventory.RemoveItem(name, take, quality, worldLevelBased);
                removed += take;
            }

            CountCache.Clear();
            return removed;
        }

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

            if (container.GetComponent<TombStone>())
            {
                return false;
            }

            if (excluded.Count > 0 && excluded.Contains(Utils.GetPrefabName(container.gameObject)))
            {
                return false;
            }

            // Someone else has it open.
            if (container != openContainer && nview.GetZDO().GetInt(ZDOVars.s_inUse) == 1)
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
