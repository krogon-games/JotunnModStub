using System;
using System.Reflection;
using BepInEx.Bootstrap;
using Corruption.Rules;
using HarmonyLib;

namespace Corruption.Content
{
    /// <summary>
    ///     Talisman slots, added to EquipmentAndQuickSlots' equipment panel through its slot API. A talisman works
    ///     while it sits in one of them, so the vanilla trinket slot stays free for vanilla trinkets.
    ///
    ///     The slots are part of the player's own inventory and nobody else's game can see them, so each player's
    ///     game writes the talismans it wears to its player object (<see cref="WornKeys" />), which every game sees.
    ///     The repel check (<see cref="TalismanRepel" />) reads them from there.
    /// </summary>
    internal static class TalismanSlots
    {
        public const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";
        public const string SlotNameToken = "$corruption_slot_talisman";

        /// <summary>Slots registered; the admin setting decides how many of them work (up to this many).</summary>
        public const int MaxSlots = 3;

        /// <summary>Player ZDO keys holding the worn talismans' prefab hashes (0 = none).</summary>
        public static readonly int[] WornKeys =
        {
            "corruption_talisman_0".GetStableHashCode(),
            "corruption_talisman_1".GetStableHashCode(),
            "corruption_talisman_2".GetStableHashCode()
        };

        private const float PublishInterval = 0.5f;

        private delegate bool TryGetSlotItemFn(string slotId, ref ItemDrop.ItemData item);

        private static TryGetSlotItemFn _tryGetSlotItem;
        private static readonly string[] SlotIds = new string[MaxSlots];
        private static readonly int[] SlotContents = new int[MaxSlots];
        private static readonly ItemDrop.ItemData[] SlotItems = new ItemDrop.ItemData[MaxSlots];
        private static readonly int[] Others = new int[MaxSlots - 1];
        private static float _nextPublish;

        public static bool Available => _tryGetSlotItem != null;

        /// <summary>Adds the slots. Runs in the plugin's Awake, after EquipmentAndQuickSlots has loaded (soft dependency).</summary>
        public static void Register()
        {
            if (!Chainloader.PluginInfos.TryGetValue(EaqsGuid, out var info) || !info.Instance)
            {
                Jotunn.Logger.LogWarning("EquipmentAndQuickSlots isn't installed: talismans have no slot to go in and do nothing.");
                return;
            }

            var api = info.Instance.GetType().Assembly.GetType("EquipmentAndQuickSlots.API");
            var addSlot = api?.GetMethod("AddSlot", BindingFlags.Public | BindingFlags.Static);
            var tryGetSlotItem = api?.GetMethod("TryGetSlotItem", BindingFlags.Public | BindingFlags.Static);
            if (addSlot == null || tryGetSlotItem == null)
            {
                Jotunn.Logger.LogWarning("This EquipmentAndQuickSlots version has no slot API (AddSlot/TryGetSlotItem): talismans do nothing.");
                return;
            }
            _tryGetSlotItem = (TryGetSlotItemFn)Delegate.CreateDelegate(typeof(TryGetSlotItemFn), tryGetSlotItem);

            for (int i = 0; i < MaxSlots; i++)
            {
                int index = i;
                string slotId = "CorruptionTalisman" + (i + 1);
                Func<ItemDrop.ItemData, bool> isValid = item => FitsSlot(index, item);
                Func<bool> isActive = () => index < CorruptionConfig.TalismanSlots.Value;
                if ((bool)addSlot.Invoke(null, new object[] { slotId, CorruptionPlugin.PluginGUID, SlotNameToken, isValid, isActive }))
                {
                    SlotIds[i] = slotId;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"EquipmentAndQuickSlots refused talisman slot {slotId} (its custom slots may all be taken).");
                }
            }
        }

        private static bool FitsSlot(int index, ItemDrop.ItemData item)
        {
            if (!Talismans.TryGetTalisman(item, out int talisman))
            {
                return false;
            }
            ReadSlots();
            for (int i = 0, o = 0; i < MaxSlots; i++)
            {
                if (i != index)
                {
                    // Moving a talisman from one talisman slot to another: its old slot doesn't count against it.
                    Others[o++] = SlotItems[i] == item ? 0 : SlotContents[i];
                }
            }
            return TalismanSlotRules.CanSlot(talisman, Others);
        }

        private static void ReadSlots()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                ItemDrop.ItemData item = null;
                if (SlotIds[i] == null || !_tryGetSlotItem(SlotIds[i], ref item))
                {
                    item = null;
                }
                SlotItems[i] = item;
                SlotContents[i] = Talismans.TryGetTalisman(item, out int hash) ? hash : 0;
            }
        }

        /// <summary>The local player publishes what it wears, twice a second, only writing when something changed.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        private static class PublishWorn_Patch
        {
            private static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer || UnityEngine.Time.time < _nextPublish)
                {
                    return;
                }
                _nextPublish = UnityEngine.Time.time + PublishInterval;
                PublishSafely(__instance);
            }

            // Separate method: the lambda's captured player would otherwise be allocated on every Update call.
            private static void PublishSafely(Player player)
            {
                Safe.Run("publish worn talismans", () => Publish(player));
            }

            private static void Publish(Player player)
            {
                var zdo = player.m_nview ? player.m_nview.GetZDO() : null;
                if (zdo == null || !player.m_nview.IsOwner())
                {
                    return;
                }
                int active = 0;
                if (Available && CorruptionConfig.TalismansEnabled.Value)
                {
                    ReadSlots();
                    active = CorruptionConfig.TalismanSlots.Value;
                }
                int[] worn = TalismanSlotRules.Worn(SlotContents, active, WornKeys.Length);
                for (int i = 0; i < WornKeys.Length; i++)
                {
                    if (zdo.GetInt(WornKeys[i]) != worn[i])
                    {
                        zdo.Set(WornKeys[i], worn[i]);
                    }
                }
            }
        }

        /// <summary>The talisman (prefab hash) a player wears in published position <paramref name="index" />, or 0.</summary>
        public static int WornAt(ZDO playerZdo, int index)
        {
            return playerZdo.GetInt(WornKeys[index]);
        }
    }
}
