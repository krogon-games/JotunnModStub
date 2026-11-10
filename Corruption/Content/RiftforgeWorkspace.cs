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
    ///     Riftforge's crafting panel, "Forge", next to Craft and Upgrade, with one forge slot. Its look is
    ///     <see cref="ForgeView" />'s (the game's own panel parts); this class is the behaviour.
    ///
    ///     - The gear in the slot is a reference: it never leaves the bag, so closing the window can't lose it. Drag
    ///       gear from the Bag onto the slot to put it in; right-click the slot (or Remove) to take it out. It's cleared
    ///       when the item is destroyed or leaves the bag.
    ///     - Right-click a material (an orb; later a Corruption shard or an Affix Splinter) in any inventory tab to pick
    ///       it: it follows the cursor and its cell is highlighted. Left-click the slot to use it once; hold Shift to
    ///       keep the pick for the next click. Picking another material replaces the pick; Escape, a right-click or any
    ///       other inventory click drops it (and does nothing else).
    ///     - The gear's normal tooltip fills the description beside the bench, refreshed after every use; the cost
    ///       slots show what one click uses; the last result stays on the bench.
    ///     - Shatter (and corrupting with a shard) destroy gear, so they ask first in the game's own Yes/No box, with the
    ///       rules' warning and chances. Both are #29's rules and aren't built yet: Shatter says so and nothing is faked.
    ///
    ///     Every rule check is <see cref="RiftforgeCrafting" />'s (Options, Apply, IsMaterial and registered operations),
    ///     re-checked by Apply on every use. Outside this tab the inventory behaves exactly as before.
    /// </summary>
    internal static class RiftforgeWorkspace
    {
        private const string TabLabel = "Forge";
        private const float DetailsInterval = 0.5f;
        private const float EligibilityInterval = 1f;

        private const string Intro =
            "Work your gear with orbs here.\n\n" +
            "<color=orange>1.</color> Drag a piece of gear from your Bag onto the forge.\n" +
            "<color=orange>2.</color> Right-click an orb in Currencies to pick it up.\n" +
            "<color=orange>3.</color> Click the forge to use it once. Hold Shift to keep the orb for the next click.\n\n" +
            "Esc or a right-click puts the orb back. Orbs that can't be used on the gear turn red.";

        private const string ShatterNotBuilt = "Shattering isn't built yet: it comes with the Affix Splinters.";
        private const string ShardNotBuilt = "Corrupting gear with a shard isn't built yet.";
        private const string SplinterNotBuilt = "Affix Splinters can't be applied yet.";
        private const string BagOnly = "Put in gear from your Bag.";
        private const string NotATarget = "This item can't be worked at the Riftforge.";
        private const string NoTarget = "Put gear in the forge first: drag it from your Bag.";
        private const string NothingToDo = "This material does nothing to this item.";
        private const string NoConfirm = "The confirmation box isn't available, so nothing was done.";

        private static readonly Color IncompatibleTint = new Color(1f, 0.45f, 0.45f, 0.9f);

        private static InventoryGui _gui;
        private static Button _tabButton;
        private static bool _failed;
        private static GameObject _cursor;
        private static Image _cursorIcon;
        private static TMP_Text _cursorAmount;

        private static bool _active;
        private static ItemDrop.ItemData _target;
        private static ItemDrop.ItemData _pick;
        private static string _pickPrefab;
        private static string _confirmOperation;
        private static ItemDrop.ItemData _confirmMaterial;
        private static bool _popupOpen;
        private static int _escapeFrame = -1;
        private static bool _detailsDirty;
        private static float _nextDetails;
        private static float _nextEligibility;
        private static readonly Dictionary<string, bool> Usable = new Dictionary<string, bool>();
        /// <summary>The affix lines the last use changed (indexes into the gear's lines), highlighted for a few seconds.</summary>
        private static List<int> _changed = new List<int>();
        private static float _changedUntil;
        private const float HighlightSeconds = 6f;

        /// <summary>The forge tab is open (the only place crafting clicks exist).</summary>
        public static bool Active => _active && ForgeView.Built;

        // ------------------------------------------------------------ building the tab

        private static void EnsureCreated(InventoryGui gui)
        {
            if (_tabButton || _failed || GUIManager.IsHeadless() || !gui.m_tabUpgrade || !gui.m_crafting)
            {
                return;
            }
            _gui = gui;
            if (!ForgeView.Build(gui, left => OnSlotClicked(left), () => SetTarget(null, ""), OnShatterPressed))
            {
                _failed = true;
                return;
            }
            CreateTabButton(gui);
            CreateCursor(gui);
            Deactivate();
            Jotunn.Logger.LogInfo("Riftforge Forge tab added.");
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
                if (component && (component.GetType().Name == "Localize" || component.GetType().Name == "UIGamePad"))
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
            var text = go.GetComponentInChildren<TMP_Text>(true);
            var craftText = gui.m_tabCraft.GetComponentInChildren<TMP_Text>(true);
            if (text)
            {
                // Same style as CRAFT and UPGRADE: capitals when they are.
                text.text = ForgeView.UpperLike(craftText ? craftText.text : text.text, TabLabel);
            }
            _tabButton = go.GetComponent<Button>();
            // Drop the Upgrade button's own click handlers.
            _tabButton.onClick = new Button.ButtonClickedEvent();
            _tabButton.onClick.AddListener(Activate);
            _tabButton.interactable = true;
            go.SetActive(false);
        }

        /// <summary>The picked material, drawn at the cursor like the game's own dragged item (never catches clicks).</summary>
        private static void CreateCursor(InventoryGui gui)
        {
            _cursor = new GameObject("Corruption_ForgePick", typeof(RectTransform));
            var rect = (RectTransform)_cursor.transform;
            rect.SetParent(gui.transform, false);
            float size = gui.m_playerGrid ? gui.m_playerGrid.m_elementSpace * 0.8f : 48f;
            rect.sizeDelta = new Vector2(size, size);
            rect.pivot = new Vector2(0f, 1f);
            _cursorIcon = _cursor.AddComponent<Image>();
            _cursorIcon.raycastTarget = false;
            _cursorIcon.preserveAspect = true;
            var amountGo = new GameObject("Amount", typeof(RectTransform));
            var amountRect = (RectTransform)amountGo.transform;
            amountRect.SetParent(rect, false);
            amountRect.anchorMin = amountRect.anchorMax = new Vector2(1f, 0f);
            amountRect.pivot = new Vector2(1f, 0f);
            amountRect.sizeDelta = new Vector2(60f, 22f);
            var amount = amountGo.AddComponent<TextMeshProUGUI>();
            var style = gui.m_playerGrid && gui.m_playerGrid.m_elementPrefab
                ? gui.m_playerGrid.m_elementPrefab.GetComponent<InventoryElement>()?.m_amount
                : null;
            if (style)
            {
                amount.font = style.font;
                amount.fontSharedMaterial = style.fontSharedMaterial;
                amount.fontSize = style.fontSize;
            }
            amount.alignment = TextAlignmentOptions.BottomRight;
            amount.raycastTarget = false;
            _cursorAmount = amount;
            _cursor.SetActive(false);
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
            if (!ForgeView.Built || !AtRiftforgeStation())
            {
                return;
            }
            _active = true;
            ForgeView.SetVisible(true);
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

        /// <summary>Back to the station's own tabs: the pick is dropped, the gear kept while the window is open.</summary>
        private static void Deactivate()
        {
            bool wasActive = _active;
            _active = false;
            ClearPick();
            CloseConfirm();
            ForgeView.SetVisible(false);
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
            _changed.Clear();
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
            var target = _target;
            var before = Loot.Of(target);
            string materialName = material != null ? Localization.instance.Localize(material.m_shared.m_name) : "";
            var result = RiftforgeCrafting.Apply(target, material, operationId);
            if (!result.Done)
            {
                ShowResult(result.Message, false);
            }
            else if (result.Item == null || !TargetStillHere())
            {
                // Destroyed (shattered, or bricked by a shard): the slot empties; the rules' message says what happened.
                _target = null;
                _changed.Clear();
                ShowResult(result.Message, true);
            }
            else if (result.Created.Count > 0)
            {
                // The Mirror's copy: the gear itself is unchanged.
                _changed.Clear();
                ForgeView.SetResult(materialName + ": copied. The copy is in your bag and can never be changed.", true);
            }
            else
            {
                ShowChange(materialName, before, Loot.Of(target));
            }
            _detailsDirty = true;
            _nextEligibility = 0f;
        }

        /// <summary>
        ///     A short result (Krogon, 2026-11-10): one headline ("Regal Orb: now Rare, 1 affix added"), then each changed
        ///     line, + new in green, − gone in red; the new lines are also highlighted in the affix block for a few
        ///     seconds. Nothing waits on it: the next click works at once.
        /// </summary>
        private static void ShowChange(string materialName, LootItem before, LootItem after)
        {
            var beforeKeys = Keys(before);
            var afterKeys = Keys(after);
            ForgeRules.Diff(beforeKeys, afterKeys, out var added, out var removed);
            var text = new System.Text.StringBuilder(ForgeRules.Summary(materialName, before?.Rarity ?? "normal", after?.Rarity ?? "normal",
                added.Count, removed.Count));
            foreach (int i in added)
            {
                text.Append("\n<color=#B8F08A>+ ").Append(ShortLine(after.Lines[i])).Append("</color>");
            }
            foreach (int i in removed)
            {
                text.Append("\n<color=#FF8A70>− ").Append(ShortLine(before.Lines[i])).Append("</color>");
            }
            ForgeView.SetResult(text.ToString(), true);
            _changed = added;
            _changedUntil = Time.time + HighlightSeconds;
        }

        private static List<string> Keys(LootItem loot)
        {
            var keys = new List<string>();
            if (loot != null)
            {
                foreach (var line in loot.Lines)
                {
                    keys.Add(string.Join("+", line.Ids) + "=" + LootItem.Format(line.Value));
                }
            }
            return keys;
        }

        /// <summary>An affix line without its range or bonus, e.g. "+12% attack speed".</summary>
        private static string ShortLine(LootItem.Line line)
        {
            if (Loot.Affixes.TryGetValue(line.Ids[0], out var affix))
            {
                return affix.Text.Replace("{0}", LootItem.Format(line.Value)) + (line.Resonant ? " ◆" : "");
            }
            return string.Join("+", line.Ids) + " " + LootItem.Format(line.Value);
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

        /// <summary>
        ///     Destructive operations ask first, in the game's own Yes/No box: the warning, then the chances from the
        ///     rules' preview (their data tables). A random result is never predicted.
        /// </summary>
        private static void OpenConfirm(RiftforgeOption option, ItemDrop.ItemData material)
        {
            if (!UnifiedPopup.IsAvailable())
            {
                ShowResult(NoConfirm, false);
                return;
            }
            CloseConfirm();
            _confirmOperation = option.Id;
            _confirmMaterial = material;
            var text = new System.Text.StringBuilder();
            text.Append("<color=#FF8060>").Append(option.Id == "shatter" ? TargetName() + " will be destroyed."
                : "This can destroy " + TargetName() + ".").Append("</color>\n\n");
            foreach (var line in option.Preview)
            {
                text.Append(line).Append('\n');
            }
            _popupOpen = true;
            UnifiedPopup.Push(new YesNoPopup(option.Name, Localization.instance.Localize(text.ToString()),
                () => Safe.Run("confirm at the Riftforge", () =>
                {
                    PopOurs();
                    OnConfirm();
                }),
                () => Safe.Run("cancel at the Riftforge", CloseConfirm), false));
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
            PopOurs();
        }

        /// <summary>Closes our Yes/No box if it's still the one showing.</summary>
        private static void PopOurs()
        {
            if (!_popupOpen)
            {
                return;
            }
            _popupOpen = false;
            if (UnifiedPopup.IsVisible())
            {
                UnifiedPopup.Pop();
            }
        }

        private static void ShowResult(string message, bool good)
        {
            ForgeView.SetResult(string.IsNullOrEmpty(message) ? "" : Localization.instance.Localize(message), good);
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
            if (_popupOpen && !UnifiedPopup.IsVisible())
            {
                // Closed some other way.
                _popupOpen = false;
                _confirmOperation = null;
                _confirmMaterial = null;
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
            bool usable = _pick != null && _target != null && CanUse(_pick);
            ForgeView.SetSlot(_target, usable, "Drag gear from your Bag here");
            if (_changed.Count > 0 && Time.time >= _changedUntil)
            {
                // The highlight of the last change runs out.
                _changed.Clear();
                _detailsDirty = true;
            }
            UpdateButtons();
            UpdateCursor();
            if (_detailsDirty || Time.time >= _nextDetails)
            {
                UpdateDetails();
            }
        }

        /// <summary>Under the forge slot: the gear's name and its affix block, the lines the last use changed highlighted.</summary>
        private static void UpdateBench()
        {
            if (_target == null)
            {
                ForgeView.SetBench("Drag gear from your Bag here", "");
                return;
            }
            bool highlight = Time.time < _changedUntil;
            var text = new System.Text.StringBuilder();
            Safe.Run("show the forge gear's affixes", () =>
            {
                // The same section the hover tooltips show, the lines the last use changed highlighted.
                string section = LootTooltip.Section(_target, _target.m_quality, _target.m_worldLevel, highlight ? _changed : null);
                if (section.Length == 0)
                {
                    text.Append("<color=#B0A898>No affixes yet</color>").Append(ForgeRules.CentredMarks(LootTooltip.LocksOf(_target)));
                }
                else
                {
                    text.Append(section);
                }
            });
            ForgeView.SetBench(TargetName(), Localization.instance.Localize(text.ToString()));
        }

        private static void UpdateButtons()
        {
            // Material-free operations only (no orb is asked), so this is cheap enough every frame.
            var option = _target != null ? RiftforgeCrafting.Options(_target, null).FirstOrDefault(o => o.Id == "shatter") : null;
            string tip = _target == null ? "Put gear in the forge first"
                : option == null ? ShatterNotBuilt
                : option.Allowed ? "Destroys the item for Affix Splinters; shows the chances first"
                : option.Refusal;
            ForgeView.SetButtons(_target != null, option != null && option.Allowed && _confirmOperation == null, tip);
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
            _cursor.transform.position = (Vector3)ZInput.pointerPosition + new Vector3(12f, -8f, 0f);
            _cursorIcon.sprite = _pick.GetIcon();
            int left = CountOf(_pickPrefab);
            _cursorAmount.text = left > 1 ? left.ToString() : "";
        }

        private static void UpdateDetails()
        {
            _detailsDirty = false;
            _nextDetails = Time.time + DetailsInterval;
            UpdateBench();
            if (_target == null)
            {
                ForgeView.SetSheet(Localization.instance.Localize("$piece_riftforge"), Localization.instance.Localize(Intro));
                return;
            }
            // The routine stats: the tooltip without the affix block, which the bench shows.
            string tooltip = "";
            Safe.Run("show the forge gear's tooltip", () =>
            {
                LootTooltip.OmitAffixes = true;
                try
                {
                    tooltip = _target.GetTooltip();
                }
                finally
                {
                    LootTooltip.OmitAffixes = false;
                }
            });
            ForgeView.SetSheet(TargetName(), Localization.instance.Localize(tooltip));
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
                    if (Active)
                    {
                        // The game re-lists recipes now and then; the Forge tab stays the selected one.
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
                    ForgeView.SetVisible(false);
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
            /// <summary>Escape drops the pick (or closes the Yes/No box) instead of closing the inventory.</summary>
            private static void Prefix()
            {
                if (!Active || (_pick == null && !_popupOpen) || !ZInput.GetKeyDown(KeyCode.Escape))
                {
                    return;
                }
                _escapeFrame = Time.frameCount;
                if (_popupOpen)
                {
                    CloseConfirm();
                }
                else
                {
                    ClearPick();
                }
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
                ForgeView.Forget();
                _gui = null;
                _tabButton = null;
                _failed = false;
                _cursor = null;
                _active = false;
                _target = null;
                _pick = null;
                _popupOpen = false;
            }
        }
    }
}
