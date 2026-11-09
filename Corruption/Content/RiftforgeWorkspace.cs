using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Corruption.Content
{
    /// <summary>
    ///     The Riftforge's crafting workspace (U3, issue #30; Krogon's round brief of 2026-11-09): a third tab on the
    ///     Riftforge's crafting panel, next to Craft and Upgrade, with one target slot.
    ///
    ///     - The target is a reference: the gear never leaves the bag, so closing the window can't lose it. Drag gear
    ///       from the Bag onto the slot to put it in; right-click the slot (or Take out) to take it out. It's cleared
    ///       when the item is destroyed or leaves the bag.
    ///     - Right-click a material (an orb; later a Corruption shard or an Affix Splinter) in any inventory tab to pick
    ///       it: it follows the cursor and its cell is highlighted. Left-click the slot to use it once; hold Shift to
    ///       keep the pick for the next click. Picking another material replaces the pick; Escape, a right-click or any
    ///       other inventory click drops it (and does nothing else).
    ///     - The target's normal tooltip stays beside it, refreshed after every use, with the result message below.
    ///     - Shatter (and corrupting with a shard) destroy gear, so they show their warning and chances and need Confirm.
    ///       Both are #29's rules and aren't built yet: the Shatter button says so and nothing is faked.
    ///
    ///     Every rule check is <see cref="RiftforgeCrafting" />'s (Options, Apply, IsMaterial and registered operations),
    ///     re-checked by Apply on every use. Outside this tab the inventory behaves exactly as before.
    /// </summary>
    internal static class RiftforgeWorkspace
    {
        private const string TabLabel = "Forge";
        private const float DetailsInterval = 0.5f;
        private const float EligibilityInterval = 1f;

        private const string HintText =
            "Drag gear from your Bag onto the slot. Right-click an orb to pick it, then left-click the slot to use it " +
            "(hold Shift to keep it). Esc or right-click drops the pick.";

        private const string ShatterNotBuilt = "Shattering isn't built yet (it comes with the Affix Splinters).";
        private const string ShardNotBuilt = "Corrupting gear with a shard isn't built yet.";
        private const string SplinterNotBuilt = "Affix Splinters can't be applied yet.";
        private const string NotAMaterial = "Only orbs (and later shards and splinters) are used at the forge.";
        private const string BagOnly = "Put in gear from your Bag.";
        private const string NotATarget = "This item can't be worked at the Riftforge.";
        private const string NoTarget = "Put gear in the slot first: drag it from your Bag.";
        private const string NothingToDo = "This material does nothing to this item.";

        private static readonly Color GoodColor = new Color(0.75f, 0.95f, 0.6f);
        private static readonly Color BadColor = new Color(1f, 0.55f, 0.45f);
        private static readonly Color IncompatibleTint = new Color(1f, 0.45f, 0.45f, 0.9f);
        private static readonly Color PanelColor = new Color(0.12f, 0.09f, 0.07f, 0.97f);

        private static InventoryGui _gui;
        private static Button _tabButton;
        private static TMP_Text _tabText;
        private static RectTransform _panel;
        private static InventoryElement _slot;
        private static TMP_Text _details;
        private static Text _pickLine;
        private static Text _result;
        private static Button _takeOut;
        private static Button _shatter;
        private static Text _shatterNote;
        private static RectTransform _confirm;
        private static Text _confirmText;
        private static GameObject _cursor;
        private static Image _cursorIcon;
        private static Text _cursorAmount;

        private static bool _active;
        private static ItemDrop.ItemData _target;
        private static ItemDrop.ItemData _pick;
        private static string _pickPrefab;
        private static string _confirmOperation;
        private static ItemDrop.ItemData _confirmMaterial;
        private static int _escapeFrame = -1;
        private static bool _detailsDirty;
        private static float _nextDetails;
        private static float _nextEligibility;
        private static readonly Dictionary<string, bool> Usable = new Dictionary<string, bool>();

        /// <summary>The forge tab is open (the only place crafting clicks exist).</summary>
        public static bool Active => _active && _panel;

        // ------------------------------------------------------------ building the tab

        private static void EnsureCreated(InventoryGui gui)
        {
            if (_tabButton || GUIManager.IsHeadless() || !gui.m_tabUpgrade || !gui.m_crafting)
            {
                return;
            }
            _gui = gui;
            CreateTabButton(gui);
            CreatePanel(gui);
            CreateCursor(gui);
            Deactivate();
            Jotunn.Logger.LogInfo("Riftforge workspace tab added.");
        }

        /// <summary>A copy of the game's Upgrade tab button, to its right, so it looks like the station's own tabs.</summary>
        private static void CreateTabButton(InventoryGui gui)
        {
            var upgrade = (RectTransform)gui.m_tabUpgrade.transform;
            var craft = (RectTransform)gui.m_tabCraft.transform;
            var go = UnityEngine.Object.Instantiate(gui.m_tabUpgrade.gameObject, upgrade.parent, false);
            go.name = "Corruption_ForgeTab";
            var rect = (RectTransform)go.transform;
            Vector2 step = upgrade.anchoredPosition - craft.anchoredPosition;
            if (step.sqrMagnitude < 1f)
            {
                step = new Vector2(upgrade.rect.width + 4f, 0f);
            }
            rect.anchoredPosition = upgrade.anchoredPosition + step;
            // The copied text would be re-localized to "Upgrade" by the game's localize component.
            foreach (var component in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component && component.GetType().Name == "Localize")
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
            _tabText = go.GetComponentInChildren<TMP_Text>(true);
            if (_tabText)
            {
                _tabText.text = TabLabel;
            }
            _tabButton = go.GetComponent<Button>();
            // Drop the Upgrade button's own click handlers.
            _tabButton.onClick = new Button.ButtonClickedEvent();
            _tabButton.onClick.AddListener(Activate);
            _tabButton.interactable = true;
            go.SetActive(false);
        }

        private static void CreatePanel(InventoryGui gui)
        {
            var crafting = gui.m_crafting;
            _panel = new GameObject("Corruption_ForgePanel", typeof(RectTransform)).GetComponent<RectTransform>();
            _panel.SetParent(crafting, false);
            _panel.anchorMin = Vector2.zero;
            _panel.anchorMax = Vector2.one;
            _panel.pivot = new Vector2(0.5f, 0.5f);
            _panel.offsetMin = Vector2.zero;
            _panel.offsetMax = new Vector2(0f, -TopInset(crafting, gui.m_tabCraft));
            _panel.SetAsLastSibling();
            var background = _panel.gameObject.AddComponent<Image>();
            var own = crafting.GetComponent<Image>();
            if (own && own.sprite)
            {
                background.sprite = own.sprite;
                background.type = own.type;
                background.color = own.color;
            }
            else
            {
                background.color = PanelColor;
            }
            // Blocks clicks, so nothing behind the workspace (the recipe list, the drop-on-the-ground button) reacts.
            background.raycastTarget = true;

            var gm = GUIManager.Instance;
            var title = Label("Title", 20, gm.ValheimOrange, TextAnchor.UpperLeft);
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(300f, 28f));
            title.text = "Riftforge";

            CreateSlot(gui);

            _pickLine = Label("Pick", 14, Color.white, TextAnchor.UpperLeft);
            _pickLine.horizontalOverflow = HorizontalWrapMode.Wrap;
            Place(_pickLine.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -126f), new Vector2(170f, 64f));

            _takeOut = MakeButton("Take out", new Vector2(16f, -196f), 170f, () => SetTarget(null, ""));
            _shatter = MakeButton("Shatter", new Vector2(16f, -236f), 170f, OnShatterPressed);
            _shatterNote = Label("ShatterNote", 12, new Color(0.8f, 0.8f, 0.8f), TextAnchor.UpperLeft);
            _shatterNote.horizontalOverflow = HorizontalWrapMode.Wrap;
            Place(_shatterNote.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -272f), new Vector2(170f, 60f));

            // The target's own tooltip, as the game builds it (rich text: TextMeshPro, like the game's tooltips).
            var detailsGo = new GameObject("Details", typeof(RectTransform));
            var detailsRect = (RectTransform)detailsGo.transform;
            detailsRect.SetParent(_panel, false);
            detailsRect.anchorMin = new Vector2(0f, 0f);
            detailsRect.anchorMax = new Vector2(1f, 1f);
            detailsRect.pivot = new Vector2(0f, 1f);
            detailsRect.offsetMin = new Vector2(200f, 96f);
            detailsRect.offsetMax = new Vector2(-14f, -40f);
            var details = detailsGo.AddComponent<TextMeshProUGUI>();
            if (gui.m_craftingStationName)
            {
                details.font = gui.m_craftingStationName.font;
            }
            details.fontSize = 16f;
            details.enableAutoSizing = true;
            details.fontSizeMin = 10f;
            details.fontSizeMax = 16f;
            details.richText = true;
            details.textWrappingMode = TextWrappingModes.Normal;
            details.overflowMode = TextOverflowModes.Ellipsis;
            details.alignment = TextAlignmentOptions.TopLeft;
            details.raycastTarget = false;
            details.color = Color.white;
            _details = details;

            _result = Label("Result", 15, GoodColor, TextAnchor.LowerLeft);
            _result.horizontalOverflow = HorizontalWrapMode.Wrap;
            _result.rectTransform.anchorMin = new Vector2(0f, 0f);
            _result.rectTransform.anchorMax = new Vector2(1f, 0f);
            _result.rectTransform.pivot = new Vector2(0f, 0f);
            _result.rectTransform.offsetMin = new Vector2(16f, 40f);
            _result.rectTransform.offsetMax = new Vector2(-14f, 92f);

            var hint = Label("Hint", 12, new Color(0.8f, 0.8f, 0.8f), TextAnchor.LowerLeft);
            hint.horizontalOverflow = HorizontalWrapMode.Wrap;
            hint.text = HintText;
            hint.rectTransform.anchorMin = new Vector2(0f, 0f);
            hint.rectTransform.anchorMax = new Vector2(1f, 0f);
            hint.rectTransform.pivot = new Vector2(0f, 0f);
            hint.rectTransform.offsetMin = new Vector2(16f, 6f);
            hint.rectTransform.offsetMax = new Vector2(-14f, 38f);

            CreateConfirm();
        }

        /// <summary>How far the station's tab row reaches into the crafting panel, so the workspace starts below it.</summary>
        private static float TopInset(RectTransform crafting, Button tab)
        {
            if (!tab)
            {
                return 0f;
            }
            var corners = new Vector3[4];
            ((RectTransform)tab.transform).GetWorldCorners(corners);
            float lowest = float.MaxValue;
            foreach (var corner in corners)
            {
                lowest = Mathf.Min(lowest, crafting.InverseTransformPoint(corner).y);
            }
            float inset = crafting.rect.yMax - lowest + 4f;
            return inset > 0f && inset < crafting.rect.height * 0.5f ? inset : 0f;
        }

        /// <summary>One cell copied from the bag's own cells (bronze frame, icon, durability bar), but not part of a grid.</summary>
        private static void CreateSlot(InventoryGui gui)
        {
            var prefab = gui.m_playerGrid ? gui.m_playerGrid.m_elementPrefab : null;
            if (!prefab)
            {
                return;
            }
            var go = UnityEngine.Object.Instantiate(prefab, _panel, false);
            go.name = "TargetSlot";
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -44f);
            _slot = go.GetComponent<InventoryElement>();
            if (_slot)
            {
                _slot.Initialize(0, 0);
            }
            var binding = go.transform.Find("binding");
            if (binding)
            {
                binding.gameObject.SetActive(false);
            }
            var input = go.GetComponentInChildren<UIInputHandler>();
            if (input)
            {
                input.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(input.m_onLeftDown,
                    new Action<UIInputHandler>(_ => Safe.Run("use the Riftforge slot", () => OnSlotClicked(true))));
                input.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(input.m_onRightDown,
                    new Action<UIInputHandler>(_ => Safe.Run("use the Riftforge slot", () => OnSlotClicked(false))));
            }
        }

        private static void CreateConfirm()
        {
            _confirm = new GameObject("Confirm", typeof(RectTransform)).GetComponent<RectTransform>();
            _confirm.SetParent(_panel, false);
            Place(_confirm, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 260f));
            var bg = _confirm.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.03f, 0.03f, 0.98f);
            bg.raycastTarget = true;
            _confirmText = Label("Text", 15, Color.white, TextAnchor.UpperLeft, _confirm);
            _confirmText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _confirmText.verticalOverflow = VerticalWrapMode.Truncate;
            _confirmText.rectTransform.anchorMin = Vector2.zero;
            _confirmText.rectTransform.anchorMax = Vector2.one;
            _confirmText.rectTransform.offsetMin = new Vector2(14f, 56f);
            _confirmText.rectTransform.offsetMax = new Vector2(-14f, -12f);
            MakeButton("Confirm", new Vector2(40f, 12f), 150f, OnConfirm, _confirm, bottom: true);
            MakeButton("Cancel", new Vector2(210f, 12f), 150f, CloseConfirm, _confirm, bottom: true);
            _confirm.gameObject.SetActive(false);
        }

        /// <summary>The picked material, drawn at the cursor (never catches clicks).</summary>
        private static void CreateCursor(InventoryGui gui)
        {
            _cursor = new GameObject("Corruption_ForgePick", typeof(RectTransform));
            var rect = (RectTransform)_cursor.transform;
            rect.SetParent(gui.transform, false);
            rect.sizeDelta = new Vector2(48f, 48f);
            rect.pivot = new Vector2(0f, 1f);
            _cursorIcon = _cursor.AddComponent<Image>();
            _cursorIcon.raycastTarget = false;
            _cursorIcon.preserveAspect = true;
            _cursorAmount = Label("Amount", 14, Color.white, TextAnchor.LowerRight, rect);
            Place(_cursorAmount.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(4f, -2f), new Vector2(60f, 20f));
            _cursor.SetActive(false);
        }

        private static Text Label(string name, int size, Color color, TextAnchor anchor, Transform parent = null)
        {
            var gm = GUIManager.Instance;
            var go = gm.CreateText("", parent ? parent : _panel, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, gm.AveriaSerifBold, size,
                color, true, Color.black, 100f, 20f, false);
            go.name = name;
            var text = go.GetComponent<Text>();
            text.alignment = anchor;
            text.supportRichText = true;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button MakeButton(string label, Vector2 position, float width, Action onClick, Transform parent = null, bool bottom = false)
        {
            var anchor = bottom ? new Vector2(0f, 0f) : new Vector2(0f, 1f);
            var go = GUIManager.Instance.CreateButton(label, parent ? parent : _panel, anchor, anchor,
                position + new Vector2(width / 2f, bottom ? 16f : -16f), width, 32f);
            go.name = "Button_" + label.Replace(" ", "");
            var sfx = go.GetComponent<ButtonSfx>();
            if (sfx)
            {
                // Jötunn's buttons also play the select sound on mouse down; keep one click.
                sfx.m_selectSfxPrefab = null;
            }
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => Safe.Run("press a Riftforge button", onClick));
            return button;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        // ------------------------------------------------------------ opening and closing

        private static bool AtRiftforgeStation()
        {
            var player = Player.m_localPlayer;
            var station = player ? player.GetCurrentCraftingStation() : null;
            return station && station.m_name == Texts.RiftforgeName;
        }

        private static void Activate()
        {
            if (!_panel || !AtRiftforgeStation())
            {
                return;
            }
            _active = true;
            _panel.gameObject.SetActive(true);
            _panel.SetAsLastSibling();
            if (_tabButton)
            {
                // The game shows its selected tab as a non-interactable button; the others look unselected.
                _tabButton.interactable = false;
            }
            if (_gui)
            {
                _gui.m_tabCraft.interactable = true;
                _gui.m_tabUpgrade.interactable = true;
            }
            CloseConfirm();
            _detailsDirty = true;
            _nextEligibility = 0f;
        }

        /// <summary>Back to the station's own tabs: the pick is dropped, the target kept while the window is open.</summary>
        private static void Deactivate()
        {
            bool wasActive = _active;
            _active = false;
            ClearPick();
            CloseConfirm();
            if (_panel)
            {
                _panel.gameObject.SetActive(false);
            }
            if (_tabButton)
            {
                _tabButton.interactable = true;
            }
            // Leaving the forge tab some other way than pressing Craft or Upgrade: show the Craft tab again.
            if (wasActive && _gui && _gui.m_tabCraft.interactable && _gui.m_tabUpgrade.interactable)
            {
                if (_gui.m_tabCraft.gameObject.activeSelf)
                {
                    _gui.m_tabCraft.interactable = false;
                }
                else
                {
                    _gui.m_tabUpgrade.interactable = false;
                }
            }
        }

        // ------------------------------------------------------------ target and pick

        private static void SetTarget(ItemDrop.ItemData item, string message)
        {
            _target = item;
            CloseConfirm();
            _detailsDirty = true;
            _nextEligibility = 0f;
            ShowResult(message, true);
        }

        private static void Pick(ItemDrop.ItemData item)
        {
            _pick = item;
            _pickPrefab = item != null && item.m_dropPrefab ? item.m_dropPrefab.name : null;
            _nextEligibility = 0f;
            if (item != null && _target != null)
            {
                // Show at once what this pick would do, or why it can't.
                var options = RiftforgeCrafting.Options(_target, item);
                int chosen = ForgeRules.PickOperation(options.Select(o => o.Allowed).ToList());
                if (chosen < 0)
                {
                    ShowResult(options.Count > 0 ? options[0].Refusal : NothingToDo, false);
                }
                else
                {
                    ShowResult("", true);
                }
            }
        }

        private static void ClearPick()
        {
            _pick = null;
            _pickPrefab = null;
            if (_cursor)
            {
                _cursor.SetActive(false);
            }
        }

        /// <summary>
        ///     The pick, still where it was; or, when that stack is used up or moved, another stack of the same orb (so
        ///     a Shift-held pick carries on through several stacks). Null when there's none left.
        /// </summary>
        private static ItemDrop.ItemData ResolvePick()
        {
            var player = Player.m_localPlayer;
            if (_pick == null || !player)
            {
                return null;
            }
            if (InventoryTabs.SourceOf(player, _pick) != null)
            {
                return _pick;
            }
            if (_pickPrefab == null || _pick.m_shared.m_maxStackSize <= 1)
            {
                // A splinter is one of a kind: once it's used, it's gone.
                return null;
            }
            string prefab = _pickPrefab;
            return InventoryTabs.Find(player, i => i.m_dropPrefab && i.m_dropPrefab.name == prefab, out _);
        }

        private static bool TargetStillHere()
        {
            var player = Player.m_localPlayer;
            return _target != null && player && player.GetInventory().ContainsItem(_target);
        }

        // ------------------------------------------------------------ clicks

        private static void OnSlotClicked(bool left)
        {
            if (!Active)
            {
                return;
            }
            var gui = InventoryGui.instance;
            bool dragging = gui && gui.m_dragGo && gui.m_dragItem != null;
            switch (ForgeRules.TargetClick(left, dragging, _pick != null, _target != null))
            {
                case ForgeClick.Insert:
                    Insert(gui);
                    break;
                case ForgeClick.Apply:
                    ApplyPick(ZInput.GetKey(KeyCode.LeftShift) || ZInput.GetKey(KeyCode.RightShift));
                    break;
                case ForgeClick.Remove:
                    SetTarget(null, "");
                    break;
                case ForgeClick.CancelPick:
                    ClearPick();
                    break;
                default:
                    if (_pick != null && _target == null)
                    {
                        ShowResult(NoTarget, false);
                    }
                    break;
            }
        }

        private static void Insert(InventoryGui gui)
        {
            var player = Player.m_localPlayer;
            var item = gui.m_dragItem;
            var from = gui.m_dragInventory;
            // The drag is only a way to point at the item: it stays where it was, so cancel the drag either way.
            gui.SetupDragItem(null, null, 1);
            if (!player || from != player.GetInventory())
            {
                ShowResult(BagOnly, false);
                return;
            }
            if (!RiftforgeCrafting.IsTarget(item))
            {
                ShowResult(NotATarget, false);
                return;
            }
            SetTarget(item, "");
        }

        private static void ApplyPick(bool shift)
        {
            var material = ResolvePick();
            if (material == null)
            {
                ClearPick();
                ShowResult("You have none of that left.", false);
                return;
            }
            if (!TargetStillHere())
            {
                SetTarget(null, NoTarget);
                return;
            }
            var options = RiftforgeCrafting.Options(_target, material);
            int chosen = ForgeRules.PickOperation(options.Select(o => o.Allowed).ToList());
            if (chosen < 0)
            {
                // Refused: nothing is used up, the pick stays so another target can be tried.
                ShowResult(options.Count > 0 ? options[0].Refusal : NothingToDo, false);
                return;
            }
            var option = options[chosen];
            if (ForgeRules.NeedsConfirmation(option.Id))
            {
                OpenConfirm(option, material);
                return;
            }
            Run(option.Id, material);
            if (!ForgeRules.KeepPick(shift, ResolvePick() != null))
            {
                ClearPick();
            }
        }

        /// <summary>Applies one operation through the rules and shows what happened. Rules re-check everything first.</summary>
        private static void Run(string operationId, ItemDrop.ItemData material)
        {
            var result = RiftforgeCrafting.Apply(_target, material, operationId);
            ShowResult(result.Message, result.Done);
            if (result.Done && (result.Item == null || !TargetStillHere()))
            {
                // Destroyed (shattered, or bricked by a shard): the slot empties; the message stays.
                _target = null;
            }
            _detailsDirty = true;
            _nextEligibility = 0f;
        }

        private static void OnShatterPressed()
        {
            if (!Active || !TargetStillHere())
            {
                return;
            }
            var option = RiftforgeCrafting.Options(_target, null).FirstOrDefault(o => o.Id == "shatter");
            if (option == null)
            {
                ShowResult(ShatterNotBuilt, false);
                return;
            }
            if (!option.Allowed)
            {
                ShowResult(option.Refusal, false);
                return;
            }
            ClearPick();
            OpenConfirm(option, null);
        }

        private static void OpenConfirm(RiftforgeOption option, ItemDrop.ItemData material)
        {
            _confirmOperation = option.Id;
            _confirmMaterial = material;
            var text = new System.Text.StringBuilder();
            text.Append("<color=#FF8060><b>").Append(option.Name).Append(option.Id == "shatter" ? ": " + TargetName() + " will be destroyed."
                : ": this can destroy " + TargetName() + ".").Append("</b></color>\n\n");
            // The chances come from the rules' own preview (their data tables); a random result is never predicted.
            foreach (var line in option.Preview)
            {
                text.Append(line).Append('\n');
            }
            _confirmText.text = Localization.instance.Localize(text.ToString());
            _confirm.gameObject.SetActive(true);
            _confirm.SetAsLastSibling();
        }

        private static void OnConfirm()
        {
            string id = _confirmOperation;
            var material = _confirmMaterial;
            CloseConfirm();
            if (id == null || !Active || !TargetStillHere())
            {
                return;
            }
            if (material != null)
            {
                material = ResolvePick() ?? material;
            }
            Run(id, material);
            ClearPick();
        }

        private static void CloseConfirm()
        {
            _confirmOperation = null;
            _confirmMaterial = null;
            if (_confirm)
            {
                _confirm.gameObject.SetActive(false);
            }
        }

        private static void ShowResult(string message, bool good)
        {
            if (!_result)
            {
                return;
            }
            _result.text = string.IsNullOrEmpty(message) ? "" : Localization.instance.Localize(message);
            _result.color = good ? GoodColor : BadColor;
        }

        private static string TargetName()
        {
            if (_target == null)
            {
                return "";
            }
            return Loot.ColoredName(_target) ?? Localization.instance.Localize(_target.m_shared.m_name);
        }

        /// <summary>
        ///     Right-click on a material in the forge tab: pick it (true: the click is handled). A shard or splinter whose
        ///     operation isn't built says so and isn't picked.
        /// </summary>
        private static bool OnRightClick(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                ClearPick();
                return true;
            }
            // Materials are used from your own bag and tabs only: a chest's orb is never picked.
            bool own = InventoryTabs.SourceOf(Player.m_localPlayer, item) != null;
            if (own && RiftforgeCrafting.IsMaterial(item))
            {
                Pick(item);
                return true;
            }
            string prefab = item.m_dropPrefab ? item.m_dropPrefab.name : "";
            if (own && (prefab == ShardRules.ShardPrefab || TabRules.TabFor(prefab) == InventoryTab.Splinters))
            {
                ClearPick();
                ShowResult(prefab == ShardRules.ShardPrefab ? ShardNotBuilt : SplinterNotBuilt, false);
                return true;
            }
            if (_pick != null)
            {
                // A right-click elsewhere while holding a pick only drops the pick (it never equips or uses anything).
                ClearPick();
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------ every frame

        private static void Tick(InventoryGui gui)
        {
            if (_tabButton)
            {
                bool here = AtRiftforgeStation();
                if (_tabButton.gameObject.activeSelf != here)
                {
                    _tabButton.gameObject.SetActive(here);
                }
                if (!here && _active)
                {
                    Deactivate();
                }
            }
            if (!Active)
            {
                return;
            }
            if (_target != null && !TargetStillHere())
            {
                // Dropped, moved to a chest, destroyed some other way.
                SetTarget(null, "");
            }
            if (_pick != null)
            {
                var resolved = ResolvePick();
                if (resolved == null)
                {
                    ClearPick();
                }
                else
                {
                    _pick = resolved;
                }
            }
            UpdateSlot();
            UpdatePickLine();
            UpdateShatter();
            UpdateCursor();
            if (_detailsDirty || Time.time >= _nextDetails)
            {
                UpdateDetails();
            }
        }

        private static void UpdateSlot()
        {
            if (!_slot || !_slot.m_icon)
            {
                return;
            }
            bool has = _target != null;
            _slot.m_icon.enabled = has;
            if (has)
            {
                _slot.m_icon.sprite = _target.GetIcon();
                _slot.m_icon.color = Color.white;
            }
            bool worn = has && _target.m_shared.m_useDurability && _target.m_durability < _target.GetMaxDurability();
            if (_slot.m_durability)
            {
                _slot.m_durability.gameObject.SetActive(worn);
                if (worn)
                {
                    _slot.m_durability.SetValue(_target.GetDurabilityPercentage());
                }
            }
            if (_slot.m_quality)
            {
                _slot.m_quality.enabled = has && _target.m_shared.m_maxQuality > 1;
                if (_slot.m_quality.enabled)
                {
                    _slot.m_quality.text = _target.m_quality.ToString();
                }
            }
            if (_slot.m_amount)
            {
                _slot.m_amount.enabled = false;
            }
            foreach (var image in new[] { _slot.m_equiped, _slot.m_queued, _slot.m_noteleport, _slot.m_food })
            {
                if (image)
                {
                    image.enabled = false;
                }
            }
            if (_slot.m_tooltip)
            {
                // The details beside the slot already show the tooltip.
                _slot.m_tooltip.m_text = has ? "" : "Drag gear from your Bag here";
                _slot.m_tooltip.m_topic = has ? "" : "Riftforge";
            }
        }

        private static void UpdatePickLine()
        {
            if (!_pickLine)
            {
                return;
            }
            if (_pick == null)
            {
                _pickLine.text = _target == null ? "Drag gear here from your Bag." : "Right-click an orb to pick it.";
                _pickLine.color = new Color(0.8f, 0.8f, 0.8f);
                return;
            }
            int left = CountOf(_pickPrefab);
            _pickLine.text = Localization.instance.Localize(_pick.m_shared.m_name) + $" ({left} left)\nLeft-click the slot to use it.";
            _pickLine.color = Color.white;
        }

        private static void UpdateShatter()
        {
            if (!_shatter)
            {
                return;
            }
            _takeOut.interactable = _target != null;
            // Material-free operations only (no orb is asked), so this is cheap enough every frame.
            var option = _target != null ? RiftforgeCrafting.Options(_target, null).FirstOrDefault(o => o.Id == "shatter") : null;
            if (_target == null)
            {
                _shatter.interactable = false;
                _shatterNote.text = "";
                return;
            }
            if (option == null)
            {
                _shatter.interactable = false;
                _shatterNote.text = ShatterNotBuilt;
                return;
            }
            _shatter.interactable = option.Allowed;
            _shatterNote.text = option.Allowed ? "Destroys the item; shows the chances first." : option.Refusal;
        }

        private static void UpdateCursor()
        {
            if (!_cursor)
            {
                return;
            }
            bool show = _pick != null && Active;
            if (_cursor.activeSelf != show)
            {
                _cursor.SetActive(show);
            }
            if (!show)
            {
                return;
            }
            _cursor.transform.SetAsLastSibling();
            _cursor.transform.position = (Vector3)ZInput.pointerPosition + new Vector3(14f, -10f, 0f);
            _cursorIcon.sprite = _pick.GetIcon();
            int left = CountOf(_pickPrefab);
            _cursorAmount.text = left > 1 ? left.ToString() : "";
        }

        private static void UpdateDetails()
        {
            _detailsDirty = false;
            _nextDetails = Time.time + DetailsInterval;
            if (!_details)
            {
                return;
            }
            if (_target == null)
            {
                _details.text = "";
                return;
            }
            string tooltip = "";
            Safe.Run("show the Riftforge target's tooltip", () => tooltip = _target.GetTooltip());
            _details.text = "<size=120%><b>" + TargetName() + "</b></size>\n" + Localization.instance.Localize(tooltip);
        }

        /// <summary>How many of a material the player has, in the bag and the tabs together.</summary>
        private static int CountOf(string prefab)
        {
            var player = Player.m_localPlayer;
            if (!player || prefab == null)
            {
                return 0;
            }
            int count = 0;
            foreach (var item in player.GetInventory().GetAllItems())
            {
                if (item.m_dropPrefab && item.m_dropPrefab.name == prefab)
                {
                    count += item.m_stack;
                }
            }
            foreach (var tab in TabRules.StoredTabs)
            {
                var inventory = InventoryTabs.Get(tab);
                if (inventory == null)
                {
                    continue;
                }
                foreach (var item in inventory.GetAllItems())
                {
                    if (item.m_dropPrefab && item.m_dropPrefab.name == prefab)
                    {
                        count += item.m_stack;
                    }
                }
            }
            return count;
        }

        /// <summary>
        ///     Called after the inventory grids update each frame (<see cref="InventoryTabsUi" />): highlights the picked
        ///     material's cell, and tints materials that can't be used on the target (red), so "can't be used here" looks
        ///     different from "none left" (dimmed, with a 0).
        /// </summary>
        public static void DecorateGrids(InventoryGui gui, InventoryGrid tabGrid)
        {
            if (!Active || gui == null)
            {
                return;
            }
            try
            {
                if (Time.time >= _nextEligibility)
                {
                    _nextEligibility = Time.time + EligibilityInterval;
                    Usable.Clear();
                }
                Decorate(tabGrid);
                Decorate(gui.m_playerGrid);
            }
            catch (Exception ex)
            {
                Safe.Report("mark materials at the Riftforge", ex);
            }
        }

        private static void Decorate(InventoryGrid grid)
        {
            if (grid == null || !grid.gameObject.activeInHierarchy || grid.m_inventory == null)
            {
                return;
            }
            int width = grid.m_inventory.GetWidth();
            foreach (var item in grid.m_inventory.GetAllItems())
            {
                var element = grid.GetElement(item.m_gridPos.x, item.m_gridPos.y, width);
                if (element == null)
                {
                    continue;
                }
                if (item == _pick && element.m_selected)
                {
                    element.m_selected.SetActive(true);
                }
                if (_target != null && element.m_icon && RiftforgeCrafting.IsMaterial(item) && !CanUse(item))
                {
                    element.m_icon.color = IncompatibleTint;
                }
            }
        }

        private static bool CanUse(ItemDrop.ItemData material)
        {
            string prefab = material.m_dropPrefab ? material.m_dropPrefab.name : "";
            if (!Usable.TryGetValue(prefab, out bool usable))
            {
                usable = RiftforgeCrafting.Options(_target, material).Any(o => o.Allowed);
                Usable[prefab] = usable;
            }
            return usable;
        }

        // ------------------------------------------------------------ patches

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateCraftingPanel))]
        private static class UpdateCraftingPanel_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                Safe.Run("add the Riftforge workspace tab", () =>
                {
                    EnsureCreated(__instance);
                    if (_active && _panel)
                    {
                        // The game re-lists recipes now and then; the workspace stays on top and its tab selected.
                        _panel.SetAsLastSibling();
                        __instance.m_tabCraft.interactable = true;
                        __instance.m_tabUpgrade.interactable = true;
                    }
                });
            }
        }

        /// <summary>Pressing Craft or Upgrade leaves the workspace (the game sets its own tab state first).</summary>
        [HarmonyPatch]
        private static class StationTab_Patch
        {
            private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.OnTabCraftPressed));
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.OnTabUpgradePressed));
            }

            private static void Prefix()
            {
                Safe.Run("leave the Riftforge workspace", () =>
                {
                    _active = false;
                    ClearPick();
                    CloseConfirm();
                    if (_panel)
                    {
                        _panel.gameObject.SetActive(false);
                    }
                    if (_tabButton)
                    {
                        _tabButton.interactable = true;
                    }
                });
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
        private static class Update_Patch
        {
            /// <summary>Escape drops the pick (or closes the confirm box) instead of closing the inventory.</summary>
            private static void Prefix()
            {
                if (!Active || (_pick == null && _confirmOperation == null) || !ZInput.GetKeyDown(KeyCode.Escape))
                {
                    return;
                }
                _escapeFrame = Time.frameCount;
                ClearPick();
                CloseConfirm();
            }

            private static void Postfix(InventoryGui __instance)
            {
                if (!_tabButton)
                {
                    return;
                }
                try
                {
                    Tick(__instance);
                }
                catch (Exception ex)
                {
                    Safe.Report("update the Riftforge workspace", ex);
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
        private static class Hide_Patch
        {
            private static bool Prefix()
            {
                // The Escape that dropped the pick this frame doesn't also close the window.
                return _escapeFrame != Time.frameCount;
            }

            private static void Postfix(bool __runOriginal)
            {
                if (!__runOriginal)
                {
                    return;
                }
                Safe.Run("close the Riftforge workspace", () =>
                {
                    Deactivate();
                    // A reference only: the item is still in the bag, nothing to give back.
                    _target = null;
                    ShowResult("", true);
                });
            }
        }

        /// <summary>In the forge tab, right-clicking a material picks it instead of using it.</summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem))]
        private static class OnRightClickItem_Patch
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(InventoryGrid __0, ItemDrop.ItemData __1)
            {
                if (!Active)
                {
                    return true;
                }
                try
                {
                    return !OnRightClick(__1);
                }
                catch (Exception ex)
                {
                    Safe.Report("pick a material at the Riftforge", ex);
                    return true;
                }
            }
        }

        /// <summary>
        ///     With a material picked, a left-click on any inventory cell only drops the pick (the "safe cancellation
        ///     click"): it doesn't pick up, move, split or equip anything.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
        private static class OnSelectedItem_Patch
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(InventoryGui __instance)
            {
                if (!Active || _pick == null || __instance.m_dragGo)
                {
                    return true;
                }
                ClearPick();
                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                _gui = null;
                _tabButton = null;
                _panel = null;
                _slot = null;
                _cursor = null;
                _confirm = null;
                _active = false;
                _target = null;
                _pick = null;
            }
        }
    }
}
