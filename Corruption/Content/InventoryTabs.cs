using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The Currency and Maps tabs (design phase 9; behaviour decided by the user 2026-10-29): two extra 8 x 4
    ///     inventories of the local player, saved with the character in its custom data.
    ///
    ///     - Picked-up shards, essences and maps go straight to their tab (<see cref="TabRules.TabFor" />); the bag only
    ///       when the tab is full. Anything that still lands in the bag (crafted, dragged in, from an old save) is moved
    ///       to its tab twice a second.
    ///     - The tabs weigh nothing: they're not part of the bag, which is what the game weighs.
    ///     - They count and pay as part of the player's inventory, so recipes, the Riftforge, boss altars and building
    ///       costs use them (tabs pay first).
    ///     - On death their contents go into the gravestone, in rows under the bag's, and come back to the tabs when
    ///       the gravestone is emptied.
    ///
    ///     The tab panel is <see cref="InventoryTabsUi" />. Every patch acts only on the local player's own inventory.
    /// </summary>
    internal static class InventoryTabs
    {
        private const string CurrencyKey = "corruption_tab_currency";
        private const string MapsKey = "corruption_tab_maps";
        private const float SweepInterval = 0.5f;
        private const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";

        private static Player _owner;
        private static Inventory _currency;
        private static Inventory _maps;
        /// <summary>Which tabs loaded completely; a tab that didn't is locked and its saved blob never written over.</summary>
        private static readonly TabStore Store = new TabStore(CurrencyKey, MapsKey);
        /// <summary>The locked-tab warning, shown once the HUD is up.</summary>
        private static string _pendingWarning;
        private static float _nextSweep;
        private static bool _movedToGrave;
        private static Func<int> _eaqsVisibleRows;
        private static bool _eaqsLookedUp;

        public static bool Enabled => CorruptionConfig.TabsEnabled.Value;

        /// <summary>
        ///     The local player's inventory for a tab (created empty for a new character), or null. Null too for a tab
        ///     that couldn't be loaded (<see cref="IsLocked" />): nothing is spent from it or put in it this session.
        /// </summary>
        public static Inventory Get(InventoryTab tab)
        {
            var player = Player.m_localPlayer;
            if (!player || tab == InventoryTab.None)
            {
                return null;
            }
            Ensure(player);
            return IsLocked(tab) ? null : tab == InventoryTab.Currency ? _currency : _maps;
        }

        /// <summary>Whether a tab's saved contents couldn't be loaded this session (it's then kept in the save, untouched).</summary>
        public static bool IsLocked(InventoryTab tab)
        {
            return tab != InventoryTab.None && !Store.IsUsable(KeyOf(tab));
        }

        private static string KeyOf(InventoryTab tab) => tab == InventoryTab.Currency ? CurrencyKey : MapsKey;

        /// <summary>The usable tabs (none, one or both).</summary>
        private static IEnumerable<Inventory> UsableTabs()
        {
            var currency = Get(InventoryTab.Currency);
            if (currency != null)
            {
                yield return currency;
            }
            var maps = Get(InventoryTab.Maps);
            if (maps != null)
            {
                yield return maps;
            }
        }

        /// <summary>The tab an item belongs in, if the tabs are on; null for everything else.</summary>
        public static Inventory TabFor(ItemDrop.ItemData item)
        {
            if (!Enabled || item == null || !item.m_dropPrefab)
            {
                return null;
            }
            InventoryTab tab = TabRules.TabFor(item.m_dropPrefab.name);
            return tab == InventoryTab.None ? null : Get(tab);
        }

        public static bool IsTab(Inventory inventory)
        {
            return inventory != null && _owner == Player.m_localPlayer &&
                   (inventory == _currency && !IsLocked(InventoryTab.Currency) || inventory == _maps && !IsLocked(InventoryTab.Maps));
        }

        /// <summary>The first item in the bag or the tabs that matches; <paramref name="inventory" /> is where it is.</summary>
        public static ItemDrop.ItemData Find(Player player, Predicate<ItemDrop.ItemData> match, out Inventory inventory)
        {
            inventory = null;
            if (!player)
            {
                return null;
            }
            foreach (var source in Sources(player))
            {
                if (source == null)
                {
                    continue;
                }
                foreach (var item in source.GetAllItems())
                {
                    if (match(item))
                    {
                        inventory = source;
                        return item;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<Inventory> Sources(Player player)
        {
            yield return player.GetInventory();
            if (Enabled && player == Player.m_localPlayer)
            {
                yield return Get(InventoryTab.Currency);
                yield return Get(InventoryTab.Maps);
            }
        }

        private static void Ensure(Player player)
        {
            if (_owner == player && _currency != null)
            {
                return;
            }
            _owner = player;
            Store.Reset();
            _pendingWarning = null;
            _currency = NewTab("$corruption_tab_currency");
            _maps = NewTab("$corruption_tab_maps");
        }

        private static Inventory NewTab(string name)
        {
            var tab = new Inventory(name, null, TabRules.Width, TabRules.Height);
            tab.m_onChanged = (Action)Delegate.Combine(tab.m_onChanged, new Action(() => Learn(tab)));
            return tab;
        }

        /// <summary>
        ///     Items in a tab count as picked up for recipe unlocks (the game only learns items from the bag): the
        ///     Riftforge, for one, unlocks with the first shard.
        /// </summary>
        private static void Learn(Inventory tab)
        {
            var player = _owner;
            if (!player || player != Player.m_localPlayer)
            {
                return;
            }
            foreach (var item in tab.GetAllItems())
            {
                player.AddKnownItem(item);
            }
        }

        private static bool IsPlayerBag(Inventory inventory)
        {
            var player = Player.m_localPlayer;
            return Enabled && player && inventory != null && inventory == player.m_inventory;
        }

        // ------------------------------------------------------------ saving with the character

        /// <summary>
        ///     Loads each tab from its own saved blob, transactionally (<see cref="TabStore" />): the blob is read into a
        ///     scratch inventory and only a complete load is shown. A blob that can't be read completely (garbled, an
        ///     item whose prefab is missing) locks that tab for the session and keeps the blob, untouched, in the save.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.Load))]
        private static class Load_Patch
        {
            /// <summary>Locked first: if the game's load or anything below fails, both tabs stay locked and their saves untouched.</summary>
            private static void Prefix(Player __instance)
            {
                try
                {
                    Ensure(__instance);
                }
                finally
                {
                    Store.BeginLoad();
                }
            }

            private static void Postfix(Player __instance)
            {
                Safe.Run("load the inventory tabs", () =>
                {
                    _currency.RemoveAll();
                    _maps.RemoveAll();
                    var problems = new List<string>();
                    LoadTab(__instance, CurrencyKey, _currency, "$corruption_tab_currency", problems);
                    LoadTab(__instance, MapsKey, _maps, "$corruption_tab_maps", problems);
                    _pendingWarning = problems.Count == 0 ? null : string.Join("\n", problems);
                });
            }

            private static void LoadTab(Player player, string key, Inventory tab, string nameToken, List<string> problems)
            {
                Inventory scratch = null;
                bool complete = Store.Load(player.m_customData, key, bytes => ReadInto(tab, bytes, out scratch));
                if (complete)
                {
                    if (scratch != null)
                    {
                        tab.m_inventory.AddRange(scratch.m_inventory);
                        tab.Changed();
                    }
                    return;
                }
                Jotunn.Logger.LogError($"Inventory tabs: the {key} tab couldn't be loaded: {Store.Problem(key)}. It is locked " +
                                       "this session: nothing is taken from it or put in it, and its saved contents are kept " +
                                       "unchanged in the character file. Pickups that would go there go to the bag.");
                problems.Add(string.Format(Localization.instance.Localize("$corruption_tab_locked"),
                    Localization.instance.Localize(nameToken)));
            }

            /// <summary>The game's own loader into a scratch inventory of the tab's size; counts what arrived.</summary>
            private static TabLoadReport ReadInto(Inventory tab, byte[] bytes, out Inventory scratch)
            {
                var header = new ZPackage(bytes);
                int version = header.ReadInt();
                int expected = version >= (int)global::Version.Item.Smaller ? header.ReadUShort() : header.ReadInt();
                scratch = new Inventory(tab.m_name, null, tab.GetWidth(), tab.GetHeight());
                var pkg = new ZPackage(bytes);
                scratch.Load(pkg);
                return new TabLoadReport { Expected = expected, Loaded = scratch.m_inventory.Count, ReadToEnd = pkg.GetPos() >= pkg.Size() };
            }
        }

        /// <summary>Shows the locked-tab warning once the player is in the world.</summary>
        private static void ShowPendingWarning(Player player)
        {
            if (_pendingWarning == null || !MessageHud.instance)
            {
                return;
            }
            player.Message(MessageHud.MessageType.Center, _pendingWarning);
            _pendingWarning = null;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        private static class Save_Patch
        {
            private static void Prefix(Player __instance)
            {
                if (__instance != _owner || _currency == null)
                {
                    return;
                }
                Safe.Run("save the inventory tabs", () =>
                {
                    // A locked tab's blob stays exactly as it was loaded.
                    Store.Save(__instance.m_customData, CurrencyKey, () => Write(_currency));
                    Store.Save(__instance.m_customData, MapsKey, () => Write(_maps));
                });
            }

            private static string Write(Inventory tab)
            {
                var pkg = new ZPackage();
                tab.Save(pkg);
                return Convert.ToBase64String(pkg.GetArray());
            }
        }

        // ------------------------------------------------------------ into the tabs

        /// <summary>Picking up (and anything else that adds an item without a slot) puts currency and maps in their tab.</summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(ItemDrop.ItemData))]
        private static class AddItem_Patch
        {
            [HarmonyPriority(Priority.High)]
            private static bool Prefix(Inventory __instance, ItemDrop.ItemData __0, ref bool __result)
            {
                try
                {
                    if (!IsPlayerBag(__instance))
                    {
                        return true;
                    }
                    var tab = TabFor(__0);
                    // A full tab leaves the item to the bag.
                    if (tab == null || !tab.CanAddItem(__0) || !tab.AddItem(__0))
                    {
                        return true;
                    }
                    __result = true;
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("put an item in its inventory tab", ex);
                    return true;
                }
            }
        }

        /// <summary>A full bag can still pick up currency and maps while their tab has room (auto-pickup asks this first).</summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.CanAddItem), typeof(ItemDrop.ItemData), typeof(int))]
        private static class CanAddItem_Patch
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(Inventory __instance, ItemDrop.ItemData __0, int __1, ref bool __result)
            {
                if (__result || !IsPlayerBag(__instance))
                {
                    return;
                }
                try
                {
                    var tab = TabFor(__0);
                    __result = tab != null && tab.CanAddItem(__0, __1);
                }
                catch (Exception ex)
                {
                    Safe.Report("check room in an inventory tab", ex);
                }
            }
        }

        /// <summary>Taking everything from a gravestone or chest: currency and maps go to their tabs first.</summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll))]
        private static class MoveAll_Patch
        {
            private static void Prefix(Inventory __instance, Inventory __0)
            {
                if (!IsPlayerBag(__instance) || __0 == null)
                {
                    return;
                }
                Safe.Run("move items into the inventory tabs", () =>
                {
                    foreach (var item in new List<ItemDrop.ItemData>(__0.GetAllItems()))
                    {
                        var tab = TabFor(item);
                        if (tab != null && tab.CanAddItem(item) && tab.AddItem(item))
                        {
                            __0.RemoveItem(item);
                        }
                    }
                });
            }
        }

        /// <summary>
        ///     The game puts materials in the bottom row first (the bag keeps tools at the top); a tab fills from the
        ///     top-left instead, like a list.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.TopFirst))]
        private static class TopFirst_Patch
        {
            private static void Postfix(Inventory __instance, ref bool __result)
            {
                if (!__result && IsTab(__instance))
                {
                    __result = true;
                }
            }
        }

        /// <summary>Twice a second, moves currency and maps that landed in the bag (crafted, dragged in, old saves) to their tab.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        private static class Sweep_Patch
        {
            private static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer || Time.time < _nextSweep)
                {
                    return;
                }
                _nextSweep = Time.time + SweepInterval;
                if (_pendingWarning != null)
                {
                    ShowPendingWarning(__instance);
                }
                if (Enabled)
                {
                    SweepSafely(__instance);
                }
            }

            // Separate method: the lambda's captured player would otherwise be allocated on every Update call.
            private static void SweepSafely(Player player)
            {
                Safe.Run("tidy currency and maps into their tabs", () => Sweep(player));
            }
        }

        private static void Sweep(Player player)
        {
            var bag = player.GetInventory();
            var dragged = InventoryGui.instance ? InventoryGui.instance.m_dragItem : null;
            int rows = BagRows(bag);
            List<ItemDrop.ItemData> moving = null;
            foreach (var item in bag.GetAllItems())
            {
                // Only the bag's own rows: EquipmentAndQuickSlots keeps its slots in rows under them.
                if (item == dragged || item.m_equipped || item.m_gridPos.y >= rows || TabFor(item) == null)
                {
                    continue;
                }
                (moving ?? (moving = new List<ItemDrop.ItemData>())).Add(item);
            }
            if (moving == null)
            {
                return;
            }
            foreach (var item in moving)
            {
                var tab = TabFor(item);
                if (tab.CanAddItem(item))
                {
                    tab.MoveItemToThis(bag, item);
                }
            }
        }

        /// <summary>The bag's visible rows: EquipmentAndQuickSlots' (its API) when it's installed, else the whole inventory.</summary>
        private static int BagRows(Inventory bag)
        {
            if (!_eaqsLookedUp)
            {
                _eaqsLookedUp = true;
                if (Chainloader.PluginInfos.TryGetValue(EaqsGuid, out var info) && info.Instance)
                {
                    var method = info.Instance.GetType().Assembly.GetType("EquipmentAndQuickSlots.API")
                        ?.GetMethod("GetVisibleRows", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        _eaqsVisibleRows = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), method);
                    }
                }
            }
            return _eaqsVisibleRows != null ? _eaqsVisibleRows() : bag.GetHeight();
        }

        // ------------------------------------------------------------ paying from the tabs

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.CountItems), typeof(string), typeof(int), typeof(bool))]
        private static class CountItems_Patch
        {
            private static void Postfix(Inventory __instance, string __0, int __1, bool __2, ref int __result)
            {
                if (__0 == null || !IsPlayerBag(__instance))
                {
                    return;
                }
                try
                {
                    foreach (var tab in UsableTabs())
                    {
                        __result += tab.CountItems(__0, __1, __2);
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("count items in the inventory tabs", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.HaveItem), typeof(string), typeof(bool))]
        private static class HaveItem_Patch
        {
            private static void Postfix(Inventory __instance, string __0, bool __1, ref bool __result)
            {
                if (__result || __0 == null || !IsPlayerBag(__instance))
                {
                    return;
                }
                try
                {
                    foreach (var tab in UsableTabs())
                    {
                        if (tab.HaveItem(__0, __1))
                        {
                            __result = true;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("look for items in the inventory tabs", ex);
                }
            }
        }

        /// <summary>
        ///     Paying a cost: the tabs pay first, then the rest is left to the bag (and to other mods' sources:
        ///     AdventureBackpacks' backpack and CraftingOverhaul's nearby chests, whose prefixes run after this one and
        ///     see only what's still owed). Does nothing if an earlier prefix already paid and skipped the original.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(string), typeof(int), typeof(int), typeof(bool))]
        private static class RemoveItem_Patch
        {
            [HarmonyPriority(Priority.High)]
            private static void Prefix(Inventory __instance, string __0, ref int __1, int __2, bool __3, bool __runOriginal)
            {
                if (!__runOriginal || __0 == null || __1 <= 0 || !IsPlayerBag(__instance))
                {
                    return;
                }
                try
                {
                    foreach (var tab in UsableTabs())
                    {
                        int take = TabRules.TakeFromTabs(__1, tab.CountItems(__0, __2, __3));
                        if (take > 0)
                        {
                            tab.RemoveItem(__0, take, __2, __3);
                            __1 -= take;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("pay from the inventory tabs", ex);
                }
            }
        }

        // ------------------------------------------------------------ death and the gravestone

        /// <summary>
        ///     Before the gravestone is made, the tabs' contents join the bag in rows under its own, so the game's own
        ///     death rules (gravestone, world modifiers that keep or delete items) treat them like everything else.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
        private static class CreateTombStone_Patch
        {
            private static void Prefix(Player __instance)
            {
                _movedToGrave = false;
                if (__instance != Player.m_localPlayer || !Enabled || __instance != _owner || _currency == null)
                {
                    return;
                }
                Safe.Run("put the inventory tabs in the gravestone", () =>
                {
                    if (ZoneSystem.instance && ZoneSystem.instance.GetGlobalKey(GlobalKeys.DeathKeepInventory))
                    {
                        return;
                    }
                    var bag = __instance.GetInventory();
                    int firstRow = bag.GetHeight();
                    int index = 0;
                    // A locked tab is empty here; its saved contents stay in the character file.
                    foreach (var tab in new List<Inventory>(UsableTabs()))
                    {
                        foreach (var item in new List<ItemDrop.ItemData>(tab.GetAllItems()))
                        {
                            TabRules.GravePosition(index++, bag.GetWidth(), firstRow, out int x, out int y);
                            item.m_gridPos = new Vector2i(x, y);
                            bag.m_inventory.Add(item);
                        }
                        tab.m_inventory.Clear();
                        tab.Changed();
                    }
                    _movedToGrave = index > 0;
                });
            }

            private static void Finalizer()
            {
                _movedToGrave = false;
            }
        }

        /// <summary>The gravestone gets enough rows to show the tabs' items under the bag's.</summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveInventoryToGrave))]
        private static class MoveInventoryToGrave_Patch
        {
            private static void Postfix(Inventory __instance)
            {
                if (!_movedToGrave)
                {
                    return;
                }
                Safe.Run("size the gravestone for the inventory tabs", () =>
                {
                    int lowest = 0;
                    foreach (var item in __instance.GetAllItems())
                    {
                        lowest = Math.Max(lowest, item.m_gridPos.y);
                    }
                    __instance.m_height = TabRules.GraveHeight(__instance.m_height, lowest);
                });
            }
        }

        /// <summary>
        ///     "Take all" from your gravestone checks the bag has room and can carry it; currency and maps go to their
        ///     tabs and weigh nothing, so they don't count against either.
        /// </summary>
        [HarmonyPatch(typeof(TombStone), nameof(TombStone.EasyFitInInventory))]
        private static class EasyFit_Patch
        {
            private static void Postfix(TombStone __instance, Player __0, ref bool __result)
            {
                if (__result || !Enabled || !__0 || __0 != Player.m_localPlayer)
                {
                    return;
                }
                try
                {
                    __result = FitsWithTabs(__0, __instance.m_container.GetInventory());
                }
                catch (Exception ex)
                {
                    Safe.Report("check a gravestone against the inventory tabs", ex);
                }
            }

            private static bool FitsWithTabs(Player player, Inventory grave)
            {
                var bag = player.GetInventory();
                int bagFree = bag.GetEmptySlots();
                var currency = Get(InventoryTab.Currency);
                var maps = Get(InventoryTab.Maps);
                int currencyFree = currency != null ? currency.GetEmptySlots() : 0;
                int mapsFree = maps != null ? maps.GetEmptySlots() : 0;
                float weight = bag.GetTotalWeight();
                foreach (var item in grave.GetAllItems())
                {
                    var tab = TabFor(item);
                    if (tab != null)
                    {
                        if (tab.FindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel) >= item.m_stack)
                        {
                            continue;
                        }
                        if (tab == currency && currencyFree > 0)
                        {
                            currencyFree--;
                            continue;
                        }
                        if (tab == maps && mapsFree > 0)
                        {
                            mapsFree--;
                            continue;
                        }
                    }
                    if (bag.FindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel) < item.m_stack)
                    {
                        bagFree--;
                    }
                    weight += item.GetWeight();
                }
                return bagFree >= 0 && weight <= player.GetMaxCarryWeight();
            }
        }
    }
}
