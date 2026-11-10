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
    ///     The tab buttons (Bag · Currencies · Splinters · Maps · Runes · Essences, user 2026-11-09) on top of the
    ///     player's inventory panel. Choosing a tab shows it in place of the bag: a grid of our own, cloned from the
    ///     game's player grid so it looks and behaves the same (drag, split, tooltips, right-click to use a map, drag
    ///     to and from chests). In Currencies and Essences each material has a fixed cell; one the player has none of
    ///     shows the material dimmed with a 0. Runes shows only a short "not available yet" note: there's no grid, so
    ///     nothing can be put there. The bag's grid is only hidden, and EquipmentAndQuickSlots' slots (kept outside it)
    ///     stay where they are. Back to Bag each time the inventory closes.
    ///
    ///     Mouse only: the tab grid isn't part of the game's gamepad navigation.
    /// </summary>
    internal static class InventoryTabsUi
    {
        /// <summary>The widest a button gets (the three-tab layout's size); six share the panel's width below that.</summary>
        private const float MaxButtonWidth = 112f;
        private const float ButtonHeight = 30f;
        private const float ButtonGap = 4f;
        private const float ButtonLeft = 14f;
        private static readonly Color Selected = new Color(1f, 0.631f, 0.235f);
        private static readonly Color Unselected = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color Ghost = new Color(1f, 1f, 1f, 0.28f);
        private static readonly Color GhostAmount = new Color(0.6f, 0.6f, 0.6f, 0.8f);

        private static readonly InventoryTab[] Order = TabRules.DisplayOrder;

        private static InventoryTab _shown = InventoryTab.None;
        private static InventoryGrid _grid;
        private static GameObject _runesNote;
        private static Text _runesText;
        private static Color? _amountColor;

        /// <summary>The tab on show (None = the Bag).</summary>
        public static InventoryTab Shown => _shown;

        /// <summary>The grid that shows the current tab (null while the Bag is shown).</summary>
        public static InventoryGrid TabGrid => _shown != InventoryTab.None && _grid && _grid.gameObject.activeSelf ? _grid : null;
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

            CreateRunesNote(source);

            // Six buttons share the panel's width (never wider than the old three), so they stay inside the panel and
            // clear of EquipmentAndQuickSlots' slots and the crafting panel beside it.
            float panelWidth = gui.m_player.rect.width > 100f ? gui.m_player.rect.width : 600f;
            float buttonWidth = Mathf.Min(MaxButtonWidth, (panelWidth - 2f * ButtonLeft - (Order.Length - 1) * ButtonGap) / Order.Length);
            _buttons = new GameObject("Corruption_TabButtons", typeof(RectTransform));
            var row = (RectTransform)_buttons.transform;
            row.SetParent(gui.m_player, false);
            row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
            row.pivot = new Vector2(0f, 0f);
            row.anchoredPosition = new Vector2(ButtonLeft, 2f);
            row.sizeDelta = new Vector2(Order.Length * (buttonWidth + ButtonGap), ButtonHeight);

            _buttonTexts = new Text[Order.Length];
            for (int i = 0; i < Order.Length; i++)
            {
                var tab = Order[i];
                var button = GUIManager.Instance.CreateButton(Localization.instance.Localize(InventoryTabs.NameToken(tab)), row,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(i * (buttonWidth + ButtonGap) + buttonWidth / 2f, ButtonHeight / 2f),
                    buttonWidth, ButtonHeight);
                button.name = "Tab_" + tab;
                // Jötunn's buttons also play the "select" sound on mouse down, so each click sounded twice; keep the click.
                var sfx = button.GetComponent<ButtonSfx>();
                if (sfx)
                {
                    sfx.m_selectSfxPrefab = null;
                }
                button.GetComponent<Button>().onClick.AddListener(() => Show(tab));
                _buttonTexts[i] = button.GetComponentInChildren<Text>();
                if (_buttonTexts[i])
                {
                    // "Currencies" and "Essences" must fit a narrower button.
                    _buttonTexts[i].resizeTextForBestFit = true;
                    _buttonTexts[i].resizeTextMinSize = 10;
                    _buttonTexts[i].resizeTextMaxSize = _buttonTexts[i].fontSize > 0 ? _buttonTexts[i].fontSize : 16;
                    _buttonTexts[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                }
            }
            Show(InventoryTab.None);
            Jotunn.Logger.LogInfo("Inventory tabs added (Bag, Currencies, Splinters, Maps, Runes, Essences).");
        }

        /// <summary>The Runes tab's note, where the grid would be. No grid, so nothing can be dropped there.</summary>
        private static void CreateRunesNote(InventoryGrid source)
        {
            var sourceRect = (RectTransform)source.transform;
            _runesNote = new GameObject("Corruption_RunesNote", typeof(RectTransform));
            var rect = (RectTransform)_runesNote.transform;
            rect.SetParent(sourceRect.parent, false);
            rect.anchorMin = sourceRect.anchorMin;
            rect.anchorMax = sourceRect.anchorMax;
            rect.pivot = sourceRect.pivot;
            rect.anchoredPosition = sourceRect.anchoredPosition;
            rect.sizeDelta = sourceRect.sizeDelta;
            // Catches clicks, so a dragged item clicked here isn't dropped on the ground through the empty space.
            var blocker = _runesNote.AddComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;
            var gm = GUIManager.Instance;
            var textGo = gm.CreateText("", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, gm.AveriaSerifBold, 18,
                gm.ValheimOrange, true, Color.black, Mathf.Max(200f, sourceRect.rect.width - 40f), Mathf.Max(80f, sourceRect.rect.height - 20f), false);
            _runesText = textGo.GetComponent<Text>();
            _runesText.alignment = TextAnchor.MiddleCenter;
            _runesText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _runesText.raycastTarget = false;
            _runesNote.SetActive(false);
        }

        private static void Show(InventoryTab tab)
        {
            if (InventoryTabs.IsLocked(tab))
            {
                // Couldn't be loaded this session: there's nothing to show, and nothing may be dropped in.
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, string.Format(
                    Localization.instance.Localize("$corruption_tab_locked"),
                    Localization.instance.Localize(InventoryTabs.NameToken(tab))));
                tab = InventoryTab.None;
            }
            _shown = InventoryTabs.Enabled ? tab : InventoryTab.None;
            bool showingTab = _shown != InventoryTab.None;
            bool runes = _shown == InventoryTab.Runes;
            if (_grid)
            {
                _grid.gameObject.SetActive(showingTab && !runes);
            }
            if (_runesNote)
            {
                _runesNote.SetActive(runes);
                if (runes && _runesText)
                {
                    _runesText.text = Localization.instance.Localize(InventoryTabs.HasSavedRuneData()
                        ? "$corruption_tab_runes_note\n$corruption_tab_runes_kept"
                        : "$corruption_tab_runes_note");
                }
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
            if (_grid && inventory != null && _grid.gameObject.activeSelf)
            {
                // No player: the game would number the first row as hotbar keys.
                _grid.UpdateInventory(inventory, null, gui.m_dragItem);
                ShowEmptyHomes(inventory, InventoryTabs.HomesOf(_shown));
            }
        }

        /// <summary>
        ///     In Currencies and Essences, a material the player has none of keeps its cell: its icon dimmed, a 0, and a
        ///     tooltip saying so. Drawn over the game's own (empty) cell after every update, so the game's grid code is
        ///     untouched; the cell stays empty and accepts any drop the tab accepts.
        /// </summary>
        private static void ShowEmptyHomes(Inventory inventory, IList<string> homes)
        {
            var elements = _grid.m_elements;
            if (elements.Count == 0)
            {
                return;
            }
            if (_amountColor == null && elements[0].m_amount)
            {
                _amountColor = elements[0].m_amount.color;
            }
            foreach (var element in elements)
            {
                if (element.m_amount && _amountColor.HasValue)
                {
                    element.m_amount.color = _amountColor.Value;
                }
            }
            if (homes == null)
            {
                return;
            }
            int width = inventory.GetWidth();
            for (int cell = 0; cell < homes.Count && cell < elements.Count; cell++)
            {
                var element = elements[cell];
                if (inventory.GetItemAt(cell % width, cell / width) != null || !element.m_icon)
                {
                    continue;
                }
                var data = ItemOf(homes[cell]);
                if (data == null)
                {
                    continue;
                }
                element.m_icon.enabled = true;
                element.m_icon.sprite = data.GetIcon();
                element.m_icon.color = Ghost;
                if (element.m_amount)
                {
                    element.m_amount.enabled = true;
                    element.m_amount.text = "0";
                    element.m_amount.color = GhostAmount;
                }
                if (element.m_tooltip)
                {
                    element.m_tooltip.m_topic = data.m_shared.m_name;
                    element.m_tooltip.m_text = "$corruption_tab_none_left";
                }
            }
        }

        /// <summary>A material's item data, from its prefab (cached); null if the prefab is missing.</summary>
        public static ItemDrop.ItemData ItemOf(string prefab)
        {
            if (string.IsNullOrEmpty(prefab) || !ObjectDB.instance)
            {
                return null;
            }
            if (!Data.TryGetValue(prefab, out var data))
            {
                var go = ObjectDB.instance.GetItemPrefab(prefab);
                var drop = go ? go.GetComponent<ItemDrop>() : null;
                data = drop ? drop.m_itemData : null;
                Data[prefab] = data;
            }
            return data;
        }

        private static readonly Dictionary<string, ItemDrop.ItemData> Data = new Dictionary<string, ItemDrop.ItemData>();

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
            private static void Postfix(bool __runOriginal)
            {
                // Not when the inventory stayed open (Escape that only dropped a Riftforge pick).
                if (__runOriginal)
                {
                    Safe.Run("reset the inventory tabs", () => Show(InventoryTab.None));
                }
            }
        }

        /// <summary>Keeps the shown tab's grid up to date, every frame the inventory updates its own grids.</summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventory))]
        private static class UpdateInventory_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                try
                {
                    if (_shown != InventoryTab.None && _grid)
                    {
                        if (!InventoryTabs.Enabled)
                        {
                            Show(InventoryTab.None);
                        }
                        else
                        {
                            Refresh(__instance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("show an inventory tab", ex);
                }
                // The Riftforge workspace marks the picked material and what can't be used, on whichever grid shows it.
                RiftforgeWorkspace.DecorateGrids(__instance, TabGrid);
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
            private static bool Prefix(InventoryGui __instance, InventoryGrid __0, ItemDrop.ItemData __1, InventoryGrid.Modifier __3,
                bool __runOriginal)
            {
                // The Riftforge workspace already handled this click (it only dropped a pick).
                if (!__runOriginal)
                {
                    return false;
                }
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
                _runesNote = null;
                _runesText = null;
                _amountColor = null;
                _bagGroup = null;
                _buttonTexts = null;
                _buttons = null;
                _shown = InventoryTab.None;
            }
        }
    }
}
