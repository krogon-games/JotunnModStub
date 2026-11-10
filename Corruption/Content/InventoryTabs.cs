using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The player's extra inventory tabs (design phase 9; behaviour decided by the user 2026-10-29; six tabs since
    ///     2026-11-09, #30): Currencies, Splinters, Maps and Essences are 8 x 4 inventories of the local player, saved
    ///     with the character in its custom data, each under its own key. Runes is only a placeholder: it has no
    ///     inventory at all, so nothing can enter it by any route, and its key is never read or written (anything
    ///     found there is left untouched).
    ///
    ///     - Picked-up orbs and shards, essences, splinters and maps go straight to their tab (<see cref="TabRules.TabFor" />);
    ///       the bag only when the tab is full. Anything that still lands in the bag (crafted, dragged in, from an old
    ///       save) is moved to its tab twice a second, and so is anything in the wrong tab (the essences that older
    ///       versions kept in Currencies move to Essences; with Essences full, they wait in Currencies).
    ///     - Currencies and Essences give each material a fixed cell (<see cref="FixedLayout" />); their contents are
    ///       re-arranged whenever they change. Only positions change.
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
        // Save keys: Currency and Maps keep the names they've had since 2026-10-29, so existing saves load unchanged.
        private const string CurrencyKey = "corruption_tab_currency";
        private const string SplintersKey = "corruption_tab_splinters";
        private const string MapsKey = "corruption_tab_maps";
        private const string EssencesKey = "corruption_tab_essences";

        /// <summary>Reserved for runes when they're built. Never read or written now, so anything saved there is kept.</summary>
        public const string RunesKey = "corruption_tab_runes";

        private const float SweepInterval = 0.5f;
        private const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";

        /// <summary>The stored tabs, their save keys and their names, in parallel (<see cref="TabRules.StoredTabs" />).</summary>
        private static readonly InventoryTab[] Tabs = TabRules.StoredTabs;
        private static readonly string[] Keys = { CurrencyKey, SplintersKey, MapsKey, EssencesKey };

        private static Player _owner;
        private static Inventory[] _inventories;
        /// <summary>Which tabs loaded completely; a tab that didn't is locked and its saved blob never written over.</summary>
        private static readonly TabStore Store = new TabStore(Keys);
        /// <summary>The locked-tab warning, shown once the HUD is up.</summary>
        private static string _pendingWarning;
        private static float _nextSweep;
        private static bool _movedToGrave;
        private static bool _arranging;
        private static bool _arrangePending;
        private static Func<int> _eaqsVisibleRows;
        private static bool _eaqsLookedUp;
        private static List<string> _currencyHomes;

        public static bool Enabled => CorruptionConfig.TabsEnabled.Value;

        /// <summary>The name token of a tab's button and messages.</summary>
        public static string NameToken(InventoryTab tab)
        {
            switch (tab)
            {
                case InventoryTab.Currency: return "$corruption_tab_currency";
                case InventoryTab.Splinters: return "$corruption_tab_splinters";
                case InventoryTab.Maps: return "$corruption_tab_maps";
                case InventoryTab.Runes: return "$corruption_tab_runes";
                case InventoryTab.Essences: return "$corruption_tab_essences";
                default: return "$corruption_tab_bag";
            }
        }

        /// <summary>
        ///     The local player's inventory for a tab (created empty for a new character), or null. Null too for a tab
        ///     that couldn't be loaded (<see cref="IsLocked" />): nothing is spent from it or put in it this session; and
        ///     always null for Runes, which holds nothing.
        /// </summary>
        public static Inventory Get(InventoryTab tab)
        {
            var player = Player.m_localPlayer;
            int i = Array.IndexOf(Tabs, tab);
            if (!player || i < 0)
            {
                return null;
            }
            Ensure(player);
            return IsLocked(tab) ? null : _inventories[i];
        }

        /// <summary>Whether a tab's saved contents couldn't be loaded this session (it's then kept in the save, untouched).</summary>
        public static bool IsLocked(InventoryTab tab)
        {
            int i = Array.IndexOf(Tabs, tab);
            return i >= 0 && !Store.IsUsable(Keys[i]);
        }

        /// <summary>Whether the character's save has something under the reserved Runes key (kept as it is).</summary>
        public static bool HasSavedRuneData()
        {
            var player = Player.m_localPlayer;
            return player && player.m_customData != null && player.m_customData.TryGetValue(RunesKey, out var value) && !string.IsNullOrEmpty(value);
        }

        /// <summary>Which tab an inventory is, or None for anything that isn't one of the local player's usable tabs.</summary>
        public static InventoryTab TabOf(Inventory inventory)
        {
            if (inventory == null || _inventories == null || _owner != Player.m_localPlayer)
            {
                return InventoryTab.None;
            }
            for (int i = 0; i < _inventories.Length; i++)
            {
                if (_inventories[i] == inventory)
                {
                    return IsLocked(Tabs[i]) ? InventoryTab.None : Tabs[i];
                }
            }
            return InventoryTab.None;
        }

        /// <summary>The usable tabs (locked ones left out), in <see cref="TabRules.StoredTabs" /> order.</summary>
        private static IEnumerable<Inventory> UsableTabs()
        {
            foreach (var tab in Tabs)
            {
                var inventory = Get(tab);
                if (inventory != null)
                {
                    yield return inventory;
                }
            }
        }

        /// <summary>The tab an item belongs in, if the tabs are on; null for everything else (and never Runes).</summary>
        public static Inventory TabFor(ItemDrop.ItemData item)
        {
            if (!Enabled || item == null || !item.m_dropPrefab)
            {
                return null;
            }
            InventoryTab tab = TabRules.TabFor(item.m_dropPrefab.name);
            return TabRules.AcceptsItems(tab) ? Get(tab) : null;
        }

        public static bool IsTab(Inventory inventory)
        {
            return TabOf(inventory) != InventoryTab.None;
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

        /// <summary>Where an item of the local player's is: the bag or a usable tab; null if it's in neither.</summary>
        public static Inventory SourceOf(Player player, ItemDrop.ItemData item)
        {
            if (!player || item == null)
            {
                return null;
            }
            foreach (var source in Sources(player))
            {
                if (source != null && source.ContainsItem(item))
                {
                    return source;
                }
            }
            return null;
        }

        private static IEnumerable<Inventory> Sources(Player player)
        {
            yield return player.GetInventory();
            if (Enabled && player == Player.m_localPlayer)
            {
                foreach (var tab in UsableTabs())
                {
                    yield return tab;
                }
            }
        }

        private static void Ensure(Player player)
        {
            if (_owner == player && _inventories != null)
            {
                return;
            }
            _owner = player;
            Store.Reset();
            _pendingWarning = null;
            _inventories = new Inventory[Tabs.Length];
            for (int i = 0; i < Tabs.Length; i++)
            {
                _inventories[i] = NewTab(Tabs[i]);
            }
        }

        private static Inventory NewTab(InventoryTab which)
        {
            var tab = new Inventory(NameToken(which), null, TabRules.Width, TabRules.Height);
            tab.m_onChanged = (Action)Delegate.Combine(tab.m_onChanged, new Action(() =>
            {
                Learn(tab);
                // Arranged on the next frame, never inside the game's own inventory code: a drag-and-drop swap is
                // several steps, and moving items between them could make a step fail and lose the item.
                _arrangePending = true;
            }));
            return tab;
        }

        /// <summary>The fixed cells of a tab's materials (<see cref="FixedLayout" />), or null for a plain grid.</summary>
        public static IList<string> HomesOf(InventoryTab tab)
        {
            if (tab == InventoryTab.Currency)
            {
                if (_currencyHomes == null)
                {
                    _currencyHomes = new List<string> { ShardRules.ShardPrefab };
                    foreach (var orb in OrbRules.Orbs)
                    {
                        _currencyHomes.Add(OrbRules.Prefab(orb));
                    }
                }
                return _currencyHomes;
            }
            return tab == InventoryTab.Essences ? EssenceItems.InOrder : null;
        }

        /// <summary>Arranges the fixed tabs after a change, once nothing is being dragged (called every frame).</summary>
        private static void ArrangePending()
        {
            if (!_arrangePending || _inventories == null || InventoryGui.instance && InventoryGui.instance.m_dragItem != null)
            {
                return;
            }
            _arrangePending = false;
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (!IsLocked(Tabs[i]))
                {
                    Arrange(_inventories[i], Tabs[i]);
                }
            }
        }

        /// <summary>
        ///     Puts each material of a fixed tab in its home cell (<see cref="FixedLayout.Arrange" />). Only grid
        ///     positions change. Called between frames only (<see cref="ArrangePending" />), never while dragging.
        /// </summary>
        private static void Arrange(Inventory tab, InventoryTab which)
        {
            var homes = HomesOf(which);
            if (_arranging || homes == null || homes.Count == 0 || tab == null)
            {
                return;
            }
            _arranging = true;
            try
            {
                var items = tab.m_inventory;
                int count = items.Count;
                var keys = new string[count];
                var cells = new int[count];
                var stacks = new int[count];
                var pinned = new bool[count];
                var dragged = InventoryGui.instance ? InventoryGui.instance.m_dragItem : null;
                int width = tab.GetWidth();
                for (int i = 0; i < count; i++)
                {
                    var item = items[i];
                    keys[i] = item.m_dropPrefab ? item.m_dropPrefab.name : "";
                    cells[i] = item.m_gridPos.y * width + item.m_gridPos.x;
                    stacks[i] = item.m_stack;
                    pinned[i] = item == dragged;
                }
                var arranged = FixedLayout.Arrange(width, tab.GetHeight(), homes, keys, cells, stacks, pinned);
                bool moved = false;
                for (int i = 0; i < count; i++)
                {
                    if (arranged[i] != cells[i])
                    {
                        items[i].m_gridPos = new Vector2i(arranged[i] % width, arranged[i] / width);
                        moved = true;
                    }
                }
                if (moved)
                {
                    tab.Changed();
                    // Our own Changed() mustn't ask for another pass.
                    _arrangePending = false;
                }
            }
            catch (Exception ex)
            {
                Safe.Report("arrange an inventory tab", ex);
            }
            finally
            {
                _arranging = false;
            }
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
                    var problems = new List<string>();
                    for (int i = 0; i < Tabs.Length; i++)
                    {
                        _inventories[i].RemoveAll();
                        LoadTab(__instance, Keys[i], _inventories[i], NameToken(Tabs[i]), problems);
                    }
                    _pendingWarning = problems.Count == 0 ? null : string.Join("\n", problems);
                    // Older saves kept essences in Currencies: move them to Essences now (and on every sweep after,
                    // while Essences is full). Only between usable tabs; a locked tab keeps everything it had.
                    int moved = TidyTabs();
                    if (moved > 0)
                    {
                        Jotunn.Logger.LogInfo($"Inventory tabs: moved {moved} item stack(s) to the tab they now belong in (essences to Essences).");
                    }
                    _arrangePending = true;
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

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new DamageCommand());
            CommandManager.Instance.AddConsoleCommand(new MisplaceCommand());
        }

        /// <summary>
        ///     co_tabs_misplace: puts an essence straight into Currencies, the way saves from before the Essences tab
        ///     hold them, so the move to Essences can be watched. Testing only.
        /// </summary>
        private sealed class MisplaceCommand : ConsoleCommand
        {
            public override string Name => "co_tabs_misplace";

            public override string Help =>
                "Corruption (testing): co_tabs_misplace [count] - put Eikthyr essences straight into your Currencies tab, as older " +
                "saves have them; within half a second they should move to the Essences tab";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var currency = Get(InventoryTab.Currency);
                var prefab = ObjectDB.instance && EssenceItems.InOrder.Count > 0 ? ObjectDB.instance.GetItemPrefab(EssenceItems.InOrder[0]) : null;
                var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (currency == null || !drop)
                {
                    Console.instance.Print("Needs a loaded character with a usable Currencies tab and the essences registered.");
                    return;
                }
                int count = args.Length > 0 && int.TryParse(args[0], out var n) && n > 0 ? Math.Min(n, drop.m_itemData.m_shared.m_maxStackSize) : 1;
                var item = drop.m_itemData.Clone();
                item.m_dropPrefab = prefab;
                item.m_stack = count;
                if (!currency.AddItem(item))
                {
                    Console.instance.Print("Currencies is full.");
                    return;
                }
                Console.instance.Print($"Put {count} essence(s) in Currencies. Open the inventory: they should be in Essences within half a second.");
            }
        }

        /// <summary>
        ///     co_tabs_damage: replaces a tab's saved data with a damaged copy (an item whose prefab doesn't exist, or
        ///     cut-off bytes), so the locked-tab handling can be play-tested; <c>undo</c> puts the original back. Both
        ///     lock the tab at once, so this session's saves don't overwrite what was written; log out and back in.
        /// </summary>
        private sealed class DamageCommand : ConsoleCommand
        {
            public override string Name => "co_tabs_damage";

            public override string Help =>
                "Corruption (testing): co_tabs_damage <currency|splinters|maps|essences> <missing|garbled|undo> - damage a tab's saved data to test " +
                "how a broken save is handled (the original is kept and 'undo' restores it); log out and back in after each";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                int index = args.Length > 0 ? Array.IndexOf(new[] { "currency", "splinters", "maps", "essences" }, args[0]) : -1;
                string key = index >= 0 ? Keys[index] : null;
                string mode = args.Length > 1 ? args[1] : null;
                if (!player || player != _owner || key == null || (mode != "missing" && mode != "garbled" && mode != "undo"))
                {
                    Console.instance.Print(Help);
                    return;
                }
                var tab = _inventories[index];
                string backupKey = key + "_backup";
                var data = player.m_customData;
                if (mode == "undo")
                {
                    if (!data.TryGetValue(backupKey, out string original))
                    {
                        Console.instance.Print("Nothing to undo for that tab.");
                        return;
                    }
                    data[key] = original;
                    data.Remove(backupKey);
                    Store.Lock(key, "restored by co_tabs_damage undo; log out and back in");
                    Console.instance.Print("Original tab data restored. Log out and back in: the tab should load normally with all its items.");
                    return;
                }
                if (!data.ContainsKey(backupKey))
                {
                    data[backupKey] = Store.IsUsable(key) ? Save_Patch.Write(tab) : data.TryGetValue(key, out string saved) ? saved : "";
                }
                string damaged;
                if (mode == "garbled")
                {
                    var bytes = Convert.FromBase64String(Save_Patch.Write(tab));
                    Array.Resize(ref bytes, bytes.Length / 2);
                    damaged = Convert.ToBase64String(bytes);
                }
                else
                {
                    damaged = WithMissingItem(tab);
                    if (damaged == null)
                    {
                        Console.instance.Print("Couldn't make a test item (no shard prefab).");
                        return;
                    }
                }
                data[key] = damaged;
                Store.Lock(key, "damaged by co_tabs_damage; log out and back in");
                Console.instance.Print("Tab data damaged and the tab locked. Log out and back in: expect the locked-tab message. " +
                                       "Then 'co_tabs_damage " + args[0] + " undo' and log out and back in again to get everything back.");
            }

            /// <summary>The tab's items plus a copy of a shard pointing at a prefab that doesn't exist.</summary>
            private static string WithMissingItem(Inventory tab)
            {
                var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab("CorruptionShard") : null;
                var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (!drop)
                {
                    return null;
                }
                var missing = new GameObject("CorruptionMissingItem_Test");
                try
                {
                    var fake = drop.m_itemData.Clone();
                    fake.m_dropPrefab = missing;
                    fake.m_stack = 1;
                    fake.m_gridPos = new Vector2i(TabRules.Width - 1, TabRules.Height - 1);
                    var copy = new Inventory(tab.m_name, null, tab.GetWidth(), tab.GetHeight());
                    copy.m_inventory.AddRange(tab.m_inventory);
                    copy.m_inventory.Add(fake);
                    return Save_Patch.Write(copy);
                }
                finally
                {
                    UnityEngine.Object.Destroy(missing);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        private static class Save_Patch
        {
            private static void Prefix(Player __instance)
            {
                if (__instance != _owner || _inventories == null)
                {
                    return;
                }
                Safe.Run("save the inventory tabs", () =>
                {
                    // A locked tab's blob stays exactly as it was loaded; the Runes key is never written.
                    for (int i = 0; i < Tabs.Length; i++)
                    {
                        var tab = _inventories[i];
                        Store.Save(__instance.m_customData, Keys[i], () => Write(tab));
                    }
                });
            }

            internal static string Write(Inventory tab)
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
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                if (_arrangePending)
                {
                    ArrangeSafely();
                }
                if (Time.time < _nextSweep)
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
                Safe.Run("tidy items into their inventory tabs", () =>
                {
                    Sweep(player);
                    TidyTabs();
                });
            }

            private static void ArrangeSafely()
            {
                Safe.Run("arrange the inventory tabs", ArrangePending);
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

        /// <summary>
        ///     Moves items that sit in the wrong tab to their own (<see cref="TabRules.MoveTo" />): the essences older
        ///     versions kept in Currencies, mostly. Only between usable tabs, only when the right tab has room, never
        ///     while the item is being dragged, and never into Runes. Returns how many stacks moved.
        /// </summary>
        private static int TidyTabs()
        {
            if (!Enabled || _inventories == null)
            {
                return 0;
            }
            var dragged = InventoryGui.instance ? InventoryGui.instance.m_dragItem : null;
            int moved = 0;
            for (int i = 0; i < Tabs.Length; i++)
            {
                var from = Get(Tabs[i]);
                if (from == null)
                {
                    continue;
                }
                List<ItemDrop.ItemData> moving = null;
                foreach (var item in from.GetAllItems())
                {
                    if (item != dragged && item.m_dropPrefab && TabRules.MoveTo(Tabs[i], item.m_dropPrefab.name) != InventoryTab.None)
                    {
                        (moving ?? (moving = new List<ItemDrop.ItemData>())).Add(item);
                    }
                }
                if (moving == null)
                {
                    continue;
                }
                foreach (var item in moving)
                {
                    var to = Get(TabRules.MoveTo(Tabs[i], item.m_dropPrefab.name));
                    if (to != null && to != from && to.CanAddItem(item))
                    {
                        // The game's own move: the item leaves 'from' only once it's in 'to'.
                        to.MoveItemToThis(from, item);
                        moved++;
                    }
                }
            }
            return moved;
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
                if (__instance != Player.m_localPlayer || !Enabled || __instance != _owner || _inventories == null)
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
                var tabFree = new Dictionary<Inventory, int>();
                foreach (var tab in UsableTabs())
                {
                    tabFree[tab] = tab.GetEmptySlots();
                }
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
                        if (tabFree.TryGetValue(tab, out int free) && free > 0)
                        {
                            tabFree[tab] = free - 1;
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
