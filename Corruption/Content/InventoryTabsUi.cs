using System;
using System.Collections.Generic;
using System.Reflection;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Corruption.Content
{
    /// <summary>
    ///     The tab buttons (Bag, Currency, Maps) on top of the player's inventory panel. Choosing Currency or Maps shows
    ///     that tab in place of the bag: a grid of our own, cloned from the game's player grid so it looks and behaves
    ///     the same (drag, split, tooltips, right-click to use a map, drag to and from chests). The bag's grid is only
    ///     hidden, and EquipmentAndQuickSlots' slots (kept outside it) stay where they are. Back to Bag each time the
    ///     inventory closes.
    ///
    ///     Mouse only: the tab grid isn't part of the game's gamepad navigation.
    /// </summary>
    internal static class InventoryTabsUi
    {
        private const float ButtonWidth = 112f;
        private const float ButtonHeight = 30f;
        private const float ButtonGap = 6f;
        private const float ButtonLeft = 14f;
        private static readonly Color Selected = new Color(1f, 0.631f, 0.235f);
        private static readonly Color Unselected = new Color(0.85f, 0.85f, 0.85f);

        private static readonly InventoryTab[] Order = { InventoryTab.None, InventoryTab.Currency, InventoryTab.Maps };
        private static readonly string[] Labels = { "$corruption_tab_bag", "$corruption_tab_currency", "$corruption_tab_maps" };

        private static InventoryTab _shown = InventoryTab.None;
        private static InventoryGrid _grid;
        private static CanvasGroup _bagGroup;
        private static Text[] _buttonTexts;
        private static GameObject _buttons;
        private static int _keepTabDrag;

        private static void EnsureCreated(InventoryGui gui)
        {
            if (_grid || GUIManager.IsHeadless() || !gui.m_playerGrid)
            {
                return;
            }

            // A copy of the game's player grid, emptied: the grid builds fresh cells (with their click handlers) the
            // first time it's shown.
            var source = gui.m_playerGrid;
            var go = UnityEngine.Object.Instantiate(source.gameObject, source.transform.parent, false);
            go.name = "Corruption_TabGrid";
            go.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);
            _grid = go.GetComponent<InventoryGrid>();
            foreach (Transform cell in _grid.m_gridRoot)
            {
                UnityEngine.Object.Destroy(cell.gameObject);
            }
            _grid.m_elements.Clear();
            _grid.m_width = 0;
            _grid.m_height = 0;
            // Every callback the game gives its own grids (InventoryGui.Awake), except moving the gamepad cursor
            // between grids. Drawing calls CanDropDragOntoItem unconditionally; without it the grid stopped after the
            // first item (play-test 2026-10-30), and without m_onReleased nothing could be dragged out.
            _grid.m_onSelected = gui.OnSelectedItem;
            _grid.m_onReleased = gui.OnReleasedItem;
            _grid.m_onRightClick = gui.OnRightClickItem;
            _grid.m_onEnter = gui.OnEnterElement;
            _grid.OnSetTouchSelection = gui.SetTouchSelection;
            _grid.CanDropDragOntoItem = gui.CanDropDragOntoItem;
            _grid.OnMoveToUpperInventoryGrid = null;
            _grid.OnMoveToLowerInventoryGrid = null;
            go.SetActive(false);

            _bagGroup = source.GetComponent<CanvasGroup>();
            if (!_bagGroup)
            {
                _bagGroup = source.gameObject.AddComponent<CanvasGroup>();
            }

            _buttons = new GameObject("Corruption_TabButtons", typeof(RectTransform));
            var row = (RectTransform)_buttons.transform;
            row.SetParent(gui.m_player, false);
            row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
            row.pivot = new Vector2(0f, 0f);
            row.anchoredPosition = new Vector2(ButtonLeft, 2f);
            row.sizeDelta = new Vector2(Order.Length * (ButtonWidth + ButtonGap), ButtonHeight);

            _buttonTexts = new Text[Order.Length];
            for (int i = 0; i < Order.Length; i++)
            {
                var tab = Order[i];
                var button = GUIManager.Instance.CreateButton(Localization.instance.Localize(Labels[i]), row,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(i * (ButtonWidth + ButtonGap) + ButtonWidth / 2f, ButtonHeight / 2f),
                    ButtonWidth, ButtonHeight);
                button.name = "Tab_" + tab;
                button.GetComponent<Button>().onClick.AddListener(() => Show(tab));
                _buttonTexts[i] = button.GetComponentInChildren<Text>();
            }
            Show(InventoryTab.None);
            Jotunn.Logger.LogInfo("Inventory tabs added (Bag, Currency, Maps).");
        }

        private static void Show(InventoryTab tab)
        {
            _shown = InventoryTabs.Enabled ? tab : InventoryTab.None;
            bool showingTab = _shown != InventoryTab.None;
            if (_grid)
            {
                _grid.gameObject.SetActive(showingTab);
            }
            if (_bagGroup)
            {
                _bagGroup.alpha = showingTab ? 0f : 1f;
                _bagGroup.blocksRaycasts = !showingTab;
                _bagGroup.interactable = !showingTab;
            }
            if (_buttonTexts != null)
            {
                for (int i = 0; i < Order.Length; i++)
                {
                    if (_buttonTexts[i])
                    {
                        _buttonTexts[i].color = Order[i] == _shown ? Selected : Unselected;
                    }
                }
            }
            if (showingTab && InventoryGui.instance)
            {
                Refresh(InventoryGui.instance);
            }
        }

        private static void Refresh(InventoryGui gui)
        {
            var inventory = InventoryTabs.Get(_shown);
            if (_grid && inventory != null)
            {
                // No player: the game would number the first row as hotbar keys.
                _grid.UpdateInventory(inventory, null, gui.m_dragItem);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
        private static class Show_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                Safe.Run("add the inventory tabs", () =>
                {
                    EnsureCreated(__instance);
                    if (_buttons)
                    {
                        _buttons.SetActive(InventoryTabs.Enabled);
                    }
                });
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
        private static class Hide_Patch
        {
            private static void Postfix()
            {
                Safe.Run("reset the inventory tabs", () => Show(InventoryTab.None));
            }
        }

        /// <summary>Keeps the shown tab's grid up to date, every frame the inventory updates its own grids.</summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventory))]
        private static class UpdateInventory_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                if (_shown == InventoryTab.None || !_grid)
                {
                    return;
                }
                try
                {
                    if (!InventoryTabs.Enabled)
                    {
                        Show(InventoryTab.None);
                        return;
                    }
                    Refresh(__instance);
                }
                catch (Exception ex)
                {
                    Safe.Report("show an inventory tab", ex);
                }
            }
        }

        /// <summary>
        ///     Only currency goes in the Currency tab and only maps in the Maps tab; and a tab item dropped onto another
        ///     item elsewhere mustn't swap that item into the tab.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem))]
        private static class DropItem_Patch
        {
            private static bool Prefix(InventoryGrid __instance, Inventory __0, ItemDrop.ItemData __1, Vector2i __3, ref bool __result)
            {
                try
                {
                    if (__instance == _grid && _shown != InventoryTab.None && !Belongs(__1, _shown))
                    {
                        __result = false;
                        return false;
                    }
                    if (InventoryTabs.IsTab(__0) && __instance.m_inventory != __0)
                    {
                        var target = __instance.m_inventory.GetItemAt(__3.x, __3.y);
                        if (target != null && target != __1 && InventoryTabs.TabFor(target) != __0)
                        {
                            __result = false;
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("check a drop into an inventory tab", ex);
                }
                return true;
            }

            private static bool Belongs(ItemDrop.ItemData item, InventoryTab tab)
            {
                return item != null && item.m_dropPrefab && TabRules.TabFor(item.m_dropPrefab.name) == tab;
            }
        }

        /// <summary>
        ///     Shift-click in a tab with a chest open moves the item to the chest. The game's own handling assumes
        ///     anything not in the chest is in the bag, and would leave a copy behind.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
        private static class OnSelectedItem_Patch
        {
            private static bool Prefix(InventoryGui __instance, InventoryGrid __0, ItemDrop.ItemData __1, InventoryGrid.Modifier __3)
            {
                if (__0 != _grid || __0 == null || __1 == null || __3 != InventoryGrid.Modifier.Move || __instance.m_dragGo || !__instance.m_currentContainer)
                {
                    return true;
                }
                try
                {
                    __instance.m_currentContainer.GetInventory().MoveItemToThis(__0.GetInventory(), __1);
                    __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("move an item from an inventory tab to a chest", ex);
                    return false;
                }
            }
        }

        /// <summary>
        ///     The game drops any drag whose item isn't in the bag whenever no chest is open (every frame in
        ///     <c>UpdateContainer</c>, and in <c>CloseContainer</c>): it assumes such an item came from a chest that's no
        ///     longer there. A drag from a tab is kept (play-test 2026-10-30: left-click couldn't pick up tab items
        ///     unless a container was open; the first fix guarded <c>UpdateInventory</c> by mistake). Every other way
        ///     of ending a drag is untouched.
        /// </summary>
        [HarmonyPatch]
        private static class KeepTabDrag_Scope
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer));
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.CloseContainer));
            }

            private static void Prefix() => _keepTabDrag++;
            private static void Finalizer() => _keepTabDrag--;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupDragItem))]
        private static class SetupDragItem_Patch
        {
            private static bool Prefix(InventoryGui __instance, ItemDrop.ItemData __0)
            {
                return _keepTabDrag <= 0 || __0 != null || !InventoryTabs.IsTab(__instance.m_dragInventory);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                _grid = null;
                _bagGroup = null;
                _buttonTexts = null;
                _buttons = null;
                _shown = InventoryTab.None;
            }
        }
    }
}
