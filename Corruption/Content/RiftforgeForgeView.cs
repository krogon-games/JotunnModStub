using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Corruption.Content
{
    /// <summary>
    ///     How the Riftforge's Forge tab looks (<see cref="RiftforgeWorkspace" /> has its behaviour). Built from the
    ///     game's own crafting panel so it matches the Craft and Upgrade tabs (Krogon, 2026-11-09: the first version
    ///     "looks very out of place"):
    ///
    ///     - While Forge is open, the panel's two regions (the recipe list on the left, the item description on the
    ///       right) are hidden, not changed, and come back as they were when you leave the tab.
    ///     - The right region is replaced by a <b>copy of the game's own description panel</b>: same background, name,
    ///       description text and main button (Shatter). It shows the gear's routine stats (its tooltip without the
    ///       affixes) at the game's own size, and, where the cost slots were, the result of the last use.
    ///     - The left region becomes the bench: the forge slot (one of the bag's own cells, larger), the gear's name,
    ///       its affix block in large, well-spaced text (what players read before every click), and a Remove button
    ///       (a copy of the game's button). Krogon's second look, 2026-11-10.
    ///     - Text uses the game's description font. Nothing is drawn from scratch except the layout of the bench.
    ///
    ///     If the panel isn't laid out as expected (another mod rebuilt it), the tab isn't added and the log says why:
    ///     better no Forge tab than a broken one.
    /// </summary>
    internal static class ForgeView
    {
        public const float SlotScale = 1.5f;

        private static readonly Color Orange = new Color(1f, 0.631f, 0.235f);
        private static readonly Color Dim = new Color(0.78f, 0.74f, 0.68f);
        private static readonly Color Good = new Color(0.78f, 0.92f, 0.62f);
        private static readonly Color Bad = new Color(1f, 0.52f, 0.42f);

        private struct Hidden
        {
            public CanvasGroup Group;
            public bool Added;
            public float Alpha;
            public bool Raycasts;
            public bool Interactable;
        }

        private static InventoryGui _gui;
        private static Transform _common;
        private static RectTransform _listRegion;
        private static RectTransform _descRegion;
        private static readonly List<Transform> ExtraHidden = new List<Transform>();
        private static readonly List<Hidden> HiddenNow = new List<Hidden>();

        private static RectTransform _bench;
        private static RectTransform _sheet;
        private static InventoryElement _slot;
        private static TMP_Text _caption;
        private static TMP_Text _affixes;
        private static TMP_Text _result;
        private static Button _remove;

        private static TMP_Text _sheetName;
        private static TMP_Text _sheetText;
        private static Button _shatter;
        private static UITooltip _shatterTip;
        private static string _lastSheet;

        public static bool Built => _bench && _sheet;

        // ------------------------------------------------------------ building

        /// <summary>Builds the view once. False (and logged) when the crafting panel isn't laid out as expected.</summary>
        public static bool Build(InventoryGui gui, Action<bool> onSlotClick, Action onRemove, Action onShatter)
        {
            _gui = gui;
            if (!FindRegions(gui, out string problem))
            {
                Jotunn.Logger.LogError("Riftforge: the crafting panel isn't laid out as expected (" + problem +
                                       "), so the Forge tab isn't added. Another mod may have rebuilt the panel.");
                return false;
            }
            BuildSheet(gui, onShatter);
            BuildBench(gui, onSlotClick, onRemove);
            SetVisible(false);
            return true;
        }

        /// <summary>
        ///     The recipe list and the description: each is the part of the panel that holds the game's own list root or
        ///     recipe name, directly under the part both share. Neither may hold the station's tabs or header.
        /// </summary>
        private static bool FindRegions(InventoryGui gui, out string problem)
        {
            problem = "";
            if (!gui.m_recipeListRoot || !gui.m_recipeName || !gui.m_recipeDecription || !gui.m_craftButton || !gui.m_crafting)
            {
                problem = "a crafting panel part is missing";
                return false;
            }
            _common = CommonParent(gui.m_recipeListRoot, gui.m_recipeName.transform);
            _listRegion = RegionOf(gui.m_recipeListRoot, _common) as RectTransform;
            _descRegion = RegionOf(gui.m_recipeName.transform, _common) as RectTransform;
            if (!_common || !_listRegion || !_descRegion || _listRegion == _descRegion || !_common.IsChildOf(gui.m_crafting))
            {
                problem = "no separate list and description areas";
                return false;
            }
            foreach (var region in new Transform[] { _listRegion, _descRegion })
            {
                if (gui.m_tabCraft && gui.m_tabCraft.transform.IsChildOf(region) ||
                    gui.m_craftingStationName && gui.m_craftingStationName.transform.IsChildOf(region))
                {
                    problem = "the tabs or the station name sit inside the list or description";
                    return false;
                }
            }
            if (!gui.m_recipeDecription.transform.IsChildOf(_descRegion))
            {
                problem = "the description text isn't in the description area";
                return false;
            }
            // The list's scrollbar can sit beside the list rather than in it.
            ExtraHidden.Clear();
            if (gui.m_recipeListScroll && !gui.m_recipeListScroll.transform.IsChildOf(_listRegion))
            {
                var region = RegionOf(gui.m_recipeListScroll.transform, _common);
                if (region && !(gui.m_tabCraft && gui.m_tabCraft.transform.IsChildOf(region)))
                {
                    ExtraHidden.Add(region);
                }
            }
            return true;
        }

        private static Transform CommonParent(Transform a, Transform b)
        {
            var ancestors = new HashSet<Transform>();
            for (var t = a; t; t = t.parent)
            {
                ancestors.Add(t);
            }
            for (var t = b; t; t = t.parent)
            {
                if (ancestors.Contains(t))
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>The ancestor of <paramref name="t" /> (or t itself) whose parent is <paramref name="common" />.</summary>
        private static Transform RegionOf(Transform t, Transform common)
        {
            if (!t || !common || t == common)
            {
                return null;
            }
            while (t && t.parent != common)
            {
                t = t.parent;
            }
            return t;
        }

        /// <summary>The copy of a part of the description panel inside our copy of it (same child positions).</summary>
        private static Transform Twin(Component original)
        {
            if (!original || !_sheet || !original.transform.IsChildOf(_descRegion))
            {
                return null;
            }
            var path = new List<int>();
            for (var t = original.transform; t != _descRegion; t = t.parent)
            {
                path.Add(t.GetSiblingIndex());
            }
            Transform twin = _sheet;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (path[i] >= twin.childCount)
                {
                    return null;
                }
                twin = twin.GetChild(path[i]);
            }
            return twin;
        }

        private static void BuildSheet(InventoryGui gui, Action onShatter)
        {
            var go = UnityEngine.Object.Instantiate(_descRegion.gameObject, _descRegion.parent, false);
            go.name = "Corruption_ForgeSheet";
            _sheet = (RectTransform)go.transform;
            Neutralise(go);

            // The icon repeats the forge slot (Krogon, 2026-11-10: remove it).
            var icon = Twin(gui.m_recipeIcon);
            if (icon)
            {
                icon.gameObject.SetActive(false);
            }
            _sheetName = Twin(gui.m_recipeName)?.GetComponent<TMP_Text>();
            _sheetText = Twin(gui.m_recipeDecription)?.GetComponent<TMP_Text>();

            // The station's extras have no meaning here.
            foreach (var part in new Component[]
                     {
                         gui.m_variantButton, gui.m_qualityPanel, gui.m_minStationLevelIcon, gui.m_minStationLevelText, gui.m_craftCancelButton,
                         gui.m_itemCraftType, gui.m_upgradeItemIcon, gui.m_upgradeItemName, gui.m_upgradeItemQuality, gui.m_upgradeItemNextQuality,
                         gui.m_upgradeItemIndex, gui.m_upgradeItemDurability, gui.m_repairButton
                     })
            {
                var twin = Twin(part);
                if (twin)
                {
                    twin.gameObject.SetActive(false);
                }
            }
            foreach (var part in new[] { gui.m_craftProgressPanel, gui.m_upgradeItemQualityArrow ? gui.m_upgradeItemQualityArrow.transform : null })
            {
                var twin = Twin(part);
                if (twin)
                {
                    twin.gameObject.SetActive(false);
                }
            }

            // The cost slots meant nothing here (Krogon, 2026-11-10: "what are these?"): they go, and the result of the
            // last use takes their place, across the panel's whole width.
            float bandTop = float.MinValue, bandBottom = float.MaxValue;
            var corners = new Vector3[4];
            foreach (var requirement in gui.m_recipeRequirementList)
            {
                var twin = requirement ? Twin(requirement.transform) as RectTransform : null;
                if (!twin)
                {
                    continue;
                }
                twin.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    float y = _sheet.InverseTransformPoint(corner).y;
                    bandTop = Mathf.Max(bandTop, y);
                    bandBottom = Mathf.Min(bandBottom, y);
                }
                twin.gameObject.SetActive(false);
            }
            _result = Text("Result", 16f, Good, TextAlignmentOptions.TopLeft, _sheet);
            var r = _result.rectTransform;
            r.anchorMin = new Vector2(0f, 0.5f);
            r.anchorMax = new Vector2(1f, 0.5f);
            r.pivot = new Vector2(0.5f, 1f);
            if (bandTop > bandBottom)
            {
                // The band in the sheet's local space, measured from its centre (the anchors' height).
                float centre = _sheet.rect.center.y;
                r.offsetMin = new Vector2(14f, bandBottom - centre);
                r.offsetMax = new Vector2(-14f, bandTop - centre);
            }
            else
            {
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(1f, 0f);
                r.pivot = new Vector2(0.5f, 0f);
                r.offsetMin = new Vector2(14f, 64f);
                r.offsetMax = new Vector2(-14f, 150f);
            }
            _result.enableAutoSizing = true;
            _result.fontSizeMin = _result.fontSize * 0.75f;
            _result.fontSizeMax = _result.fontSize;

            var button = Twin(gui.m_craftButton);
            if (button)
            {
                button.gameObject.SetActive(true);
                _shatter = button.GetComponent<Button>();
                _shatterTip = button.GetComponent<UITooltip>();
                SetLabel(button, "Shatter");
                _shatter.onClick.AddListener(() => Safe.Run("press Shatter", onShatter));
            }
            else
            {
                // The game's Craft button lives outside the description: a copy of it, bottom right.
                _shatter = CopyButton(gui.m_craftButton, _sheet, "Shatter", onShatter);
                if (_shatter)
                {
                    var rt = (RectTransform)_shatter.transform;
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.anchoredPosition = new Vector2(-12f, 12f);
                    _shatterTip = _shatter.GetComponent<UITooltip>();
                }
            }
        }

        private static void BuildBench(InventoryGui gui, Action<bool> onSlotClick, Action onRemove)
        {
            _bench = new GameObject("Corruption_ForgeBench", typeof(RectTransform)).GetComponent<RectTransform>();
            _bench.SetParent(_listRegion.parent, false);
            _bench.anchorMin = _listRegion.anchorMin;
            _bench.anchorMax = _listRegion.anchorMax;
            _bench.pivot = _listRegion.pivot;
            _bench.anchoredPosition = _listRegion.anchoredPosition;
            _bench.sizeDelta = _listRegion.sizeDelta;
            _bench.SetSiblingIndex(_listRegion.GetSiblingIndex() + 1);

            // The list's own dark inset, so the bench sits in the same frame the recipe list does.
            var background = _bench.gameObject.AddComponent<Image>();
            var inset = LargestImage(_listRegion);
            if (inset)
            {
                background.sprite = inset.sprite;
                background.type = inset.type;
                background.color = inset.color;
                background.pixelsPerUnitMultiplier = inset.pixelsPerUnitMultiplier;
            }
            else
            {
                background.color = new Color(0f, 0f, 0f, 0f);
            }
            // Catches clicks, so nothing behind the bench (the hidden list, the drop-on-the-ground button) reacts.
            background.raycastTarget = true;

            float width = Mathf.Max(160f, _listRegion.rect.width);
            float slotSize = gui.m_playerGrid ? gui.m_playerGrid.m_elementSpace * SlotScale : 96f;

            var title = Text("Title", 20f, Orange, TextAlignmentOptions.Center);
            Top(title.rectTransform, 10f, 28f);
            title.text = "Forge";

            CreateSlot(gui, onSlotClick, -42f);

            // The gear's name, then its affixes: what players read before every click (Krogon, 2026-11-10), so they
            // sit in a fixed place in larger, well-spaced text; the routine stats are in the description beside it.
            _caption = Text("Caption", 17f, Dim, TextAlignmentOptions.Top);
            Top(_caption.rectTransform, 50f + slotSize, 46f);

            _remove = CopyButton(gui.m_craftButton, _bench, "Remove", onRemove);
            if (_remove)
            {
                var rt = (RectTransform)_remove.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = new Vector2(0f, 12f);
                rt.sizeDelta = new Vector2(Mathf.Min(width - 32f, Mathf.Max(120f, rt.sizeDelta.x)), rt.sizeDelta.y > 10f ? rt.sizeDelta.y : 40f);
            }
            float buttonTop = _remove ? 12f + ((RectTransform)_remove.transform).sizeDelta.y + 10f : 12f;

            _affixes = Text("Affixes", 17f, Color.white, TextAlignmentOptions.TopLeft);
            var a = _affixes.rectTransform;
            a.anchorMin = new Vector2(0f, 0f);
            a.anchorMax = new Vector2(1f, 1f);
            a.pivot = new Vector2(0.5f, 1f);
            a.offsetMin = new Vector2(12f, buttonTop);
            a.offsetMax = new Vector2(-10f, -(98f + slotSize));
            _affixes.lineSpacing = 8f;
            _affixes.paragraphSpacing = 10f;
            // Shrinks only when an item has many long lines, never below a size that still reads easily.
            _affixes.enableAutoSizing = true;
            _affixes.fontSizeMax = _affixes.fontSize;
            _affixes.fontSizeMin = _affixes.fontSize * 0.8f;
        }

        /// <summary>One of the bag's own cells (bronze frame, icon, durability), larger, not part of any grid.</summary>
        private static void CreateSlot(InventoryGui gui, Action<bool> onSlotClick, float y)
        {
            var prefab = gui.m_playerGrid ? gui.m_playerGrid.m_elementPrefab : null;
            if (!prefab)
            {
                return;
            }
            var go = UnityEngine.Object.Instantiate(prefab, _bench, false);
            go.name = "ForgeSlot";
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.localScale = new Vector3(SlotScale, SlotScale, 1f);
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
                    new Action<UIInputHandler>(_ => Safe.Run("use the forge slot", () => onSlotClick(true))));
                input.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(input.m_onRightDown,
                    new Action<UIInputHandler>(_ => Safe.Run("use the forge slot", () => onSlotClick(false))));
            }
        }

        /// <summary>A copy of one of the game's buttons with our label and click, and nothing of the original's behaviour.</summary>
        private static Button CopyButton(Button original, Transform parent, string label, Action onClick)
        {
            if (!original)
            {
                return null;
            }
            var go = UnityEngine.Object.Instantiate(original.gameObject, parent, false);
            go.name = "Forge" + label;
            go.SetActive(true);
            Neutralise(go);
            var button = go.GetComponent<Button>();
            SetLabel(go.transform, label);
            var tip = go.GetComponent<UITooltip>();
            if (tip)
            {
                tip.m_text = "";
            }
            button.onClick.AddListener(() => Safe.Run("press " + label, onClick));
            return button;
        }

        /// <summary>
        ///     A copied part of the game's panel does nothing on its own: its buttons lose their click handlers, and
        ///     gamepad key hints (which would press them) and the game's re-localizing text components are switched off.
        /// </summary>
        private static void Neutralise(GameObject go)
        {
            foreach (var button in go.GetComponentsInChildren<Button>(true))
            {
                button.onClick = new Button.ButtonClickedEvent();
            }
            foreach (var component in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                string type = component ? component.GetType().Name : "";
                if (type == "UIGamePad" || type == "Localize")
                {
                    component.enabled = false;
                }
            }
        }

        private static void SetLabel(Transform button, string label)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text)
            {
                text.text = UpperLike(text.text, label);
            }
        }

        public static string UpperLike(string gameText, string label) => Rules.ForgeRules.UpperLike(gameText, label);

        private static Image LargestImage(Transform root)
        {
            Image best = null;
            float bestArea = 0f;
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (!image.sprite)
                {
                    continue;
                }
                var rect = image.rectTransform.rect;
                float area = rect.width * rect.height;
                if (area > bestArea)
                {
                    best = image;
                    bestArea = area;
                }
            }
            return best;
        }

        /// <summary>Text in the game's description font (copied from its own description).</summary>
        private static TMP_Text Text(string name, float size, Color color, TextAlignmentOptions alignment, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent ? parent : _bench, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            var style = _gui.m_recipeDecription;
            float scale = 1f;
            if (style)
            {
                text.font = style.font;
                text.fontSharedMaterial = style.fontSharedMaterial;
                // Sizes are given for a 16-point description; follow the game's own size.
                scale = style.fontSize > 0f ? style.fontSize / 16f : 1f;
            }
            text.fontSize = size * scale;
            text.color = color;
            text.alignment = alignment;
            text.richText = true;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static void Top(RectTransform rt, float fromTop, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(10f, -fromTop - height);
            rt.offsetMax = new Vector2(-10f, -fromTop);
        }

        // ------------------------------------------------------------ showing

        /// <summary>Shows the Forge view in place of the station's list and description, or puts those back.</summary>
        public static void SetVisible(bool forge)
        {
            if (!Built)
            {
                return;
            }
            _bench.gameObject.SetActive(forge);
            _sheet.gameObject.SetActive(forge);
            if (forge)
            {
                Hide(_listRegion);
                Hide(_descRegion);
                foreach (var extra in ExtraHidden)
                {
                    Hide(extra);
                }
                // CraftingOverhaul's search box and grouping button sit over the recipe list.
                foreach (var t in _common.GetComponentsInChildren<RectTransform>(true))
                {
                    if (t.name.StartsWith("CraftingOverhaul_", StringComparison.Ordinal) && !t.IsChildOf(_listRegion) && !t.IsChildOf(_bench) &&
                        !t.IsChildOf(_sheet))
                    {
                        Hide(t);
                    }
                }
                _bench.SetAsLastSibling();
                _sheet.SetAsLastSibling();
                _lastSheet = null;
            }
            else
            {
                Restore();
            }
        }

        private static void Hide(Transform t)
        {
            if (!t)
            {
                return;
            }
            var group = t.GetComponent<CanvasGroup>();
            var hidden = new Hidden { Group = group, Added = !group };
            if (!group)
            {
                group = t.gameObject.AddComponent<CanvasGroup>();
                hidden.Group = group;
            }
            foreach (var already in HiddenNow)
            {
                if (already.Group == group)
                {
                    return;
                }
            }
            hidden.Alpha = group.alpha;
            hidden.Raycasts = group.blocksRaycasts;
            hidden.Interactable = group.interactable;
            HiddenNow.Add(hidden);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private static void Restore()
        {
            foreach (var hidden in HiddenNow)
            {
                if (!hidden.Group)
                {
                    continue;
                }
                if (hidden.Added)
                {
                    // Back to "no effect" first: Destroy only happens at the end of the frame, and the group may be
                    // picked up again before then.
                    hidden.Group.alpha = 1f;
                    hidden.Group.blocksRaycasts = true;
                    hidden.Group.interactable = true;
                    UnityEngine.Object.Destroy(hidden.Group);
                }
                else
                {
                    hidden.Group.alpha = hidden.Alpha;
                    hidden.Group.blocksRaycasts = hidden.Raycasts;
                    hidden.Group.interactable = hidden.Interactable;
                }
            }
            HiddenNow.Clear();
        }

        // ------------------------------------------------------------ contents

        /// <summary>The forge slot: the gear's icon, durability and quality; a gold frame while a pick can be used on it.</summary>
        public static void SetSlot(ItemDrop.ItemData target, bool armed, string emptyTip)
        {
            if (!_slot || !_slot.m_icon)
            {
                return;
            }
            bool has = target != null;
            _slot.m_icon.enabled = has;
            if (has)
            {
                _slot.m_icon.sprite = target.GetIcon();
                _slot.m_icon.color = Color.white;
            }
            bool worn = has && target.m_shared.m_useDurability && target.m_durability < target.GetMaxDurability();
            if (_slot.m_durability)
            {
                _slot.m_durability.gameObject.SetActive(worn);
                if (worn)
                {
                    _slot.m_durability.SetValue(target.GetDurabilityPercentage());
                }
            }
            if (_slot.m_quality)
            {
                _slot.m_quality.enabled = has && target.m_shared.m_maxQuality > 1;
                if (_slot.m_quality.enabled)
                {
                    _slot.m_quality.text = target.m_quality.ToString();
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
            if (_slot.m_selected)
            {
                _slot.m_selected.SetActive(armed);
            }
            if (_slot.m_tooltip)
            {
                // The description beside the bench already shows the gear's tooltip.
                _slot.m_tooltip.m_topic = has ? "" : "$piece_riftforge";
                _slot.m_tooltip.m_text = has ? "" : emptyTip;
            }
        }

        /// <summary>Under the forge slot: the gear's name, and its affix block (rarity line, affixes, marks).</summary>
        public static void SetBench(string caption, string affixes)
        {
            if (_caption)
            {
                _caption.text = caption ?? "";
            }
            if (_affixes && _affixes.text != (affixes ?? ""))
            {
                _affixes.text = affixes ?? "";
            }
        }

        /// <summary>The last use's result, across the description's width: a short headline, then the changed lines.</summary>
        public static void SetResult(string message, bool good)
        {
            if (_result)
            {
                _result.text = message ?? "";
                _result.color = good ? Good : Bad;
            }
        }

        /// <summary>The description: the gear's name and the rest of its tooltip, or how the forge works while it's empty.</summary>
        public static void SetSheet(string name, string text)
        {
            string key = name + "\n" + text;
            if (key == _lastSheet)
            {
                return;
            }
            _lastSheet = key;
            if (_sheetName)
            {
                _sheetName.enabled = true;
                _sheetName.text = name;
            }
            if (_sheetText)
            {
                _sheetText.enabled = true;
                _sheetText.text = text;
            }
        }

        public static void SetButtons(bool canRemove, bool canShatter, string shatterTip)
        {
            if (_remove)
            {
                _remove.interactable = canRemove;
            }
            if (_shatter)
            {
                _shatter.interactable = canShatter;
            }
            if (_shatterTip)
            {
                _shatterTip.m_text = shatterTip ?? "";
            }
        }

        public static void Forget()
        {
            _gui = null;
            _common = null;
            _listRegion = null;
            _descRegion = null;
            ExtraHidden.Clear();
            HiddenNow.Clear();
            _bench = null;
            _sheet = null;
            _slot = null;
            _caption = null;
            _affixes = null;
            _result = null;
            _remove = null;
            _sheetName = null;
            _sheetText = null;
            _shatter = null;
            _shatterTip = null;
            _lastSheet = null;
        }
    }
}
