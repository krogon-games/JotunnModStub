using System;
using System.Collections.Generic;
using Corruption.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Corruption.Content
{
    /// <summary>What one Forge confirmation says. The rules supply the rows and chances; this only carries them.</summary>
    internal sealed class ForgeDialogContent
    {
        public string Title;
        public string ItemName;
        public ItemDrop.ItemData Item;
        public string Cost;
        public string Warning;
        public string ListHeading;
        public string Explanation;
        public string ConfirmLabel;

        /// <summary>The left column, rich text (affix lines as the Forge shows them, or a shard's outcomes).</summary>
        public readonly List<string> Rows = new List<string>();

        /// <summary>The right column, one per row ("50%").</summary>
        public readonly List<string> Chances = new List<string>();
    }

    /// <summary>
    ///     The Forge's confirmation dialog, one design for Shatter and for Corrupt (Krogon, 2026-11-10): title, the
    ///     item's icon and name, the cost, the warning once, a two-column list with right-aligned chances, a short
    ///     explanation, a "Don't warn me again" tickbox and a footer with Cancel and the action. It is drawn on the
    ///     game's normal panel (the inventory's background) with the message box's heading font, body font and buttons.
    ///     Positions and the height come from <see cref="ForgeDialogLayout" />, fed with the measured heights of the
    ///     wrapped texts; the whole dialog shrinks when the screen is too small. Only shows and reports the click; the
    ///     rules run in <see cref="RiftforgeCrafting" />.
    /// </summary>
    internal static class ForgeDialog
    {
        public const string DontWarnLabel = "Don't warn me again";
        public const string ChanceHeading = "Chance";

        private const string WarningColor = "#FF8060";
        private static readonly Color Body = new Color(0.93f, 0.89f, 0.81f);
        private static readonly Color Secondary = new Color(0.72f, 0.68f, 0.62f);
        private static readonly Color Heading = new Color(1f, 0.631f, 0.235f);
        private static readonly Color Rule = new Color(1f, 1f, 1f, 0.14f);
        // Used only when the game's own panel art can't be found: the dialog is never left without a background.
        private static readonly Color PlainPanel = new Color(0.13f, 0.15f, 0.16f, 0.98f);

        private static GameObject _root;
        private static GameObject _tick;

        public static bool IsOpen => _root;

        /// <summary>Whether "Don't warn me again" is ticked. Read it when the action is confirmed; cancelling ignores it.</summary>
        public static bool DontWarn { get; private set; }

        /// <summary>Opens the dialog. False when it can't be built (the caller then asks another way).</summary>
        public static bool Open(InventoryGui gui, ForgeDialogContent content, Action onConfirm, Action onCancel)
        {
            Close();
            var style = gui ? gui.m_recipeDecription : null;
            var canvas = gui ? gui.GetComponentInParent<Canvas>() : null;
            if (!style || !canvas || content == null || content.Item == null)
            {
                return false;
            }
            var popup = UnifiedPopup.instance;
            var bodyStyle = popup && popup.bodyText ? popup.bodyText : style;
            var headingStyle = popup && popup.headerText ? popup.headerText : (gui.m_craftingStationName ? gui.m_craftingStationName : gui.m_recipeName);
            var screen = (RectTransform)canvas.rootCanvas.transform;
            // Sizes are written for 16-point body text; follow the game's own size.
            float u = style.fontSize > 0f ? style.fontSize / 16f : 1f;

            // Everything behind the dialog is dimmed and takes no clicks while it is open.
            _root = new GameObject("Corruption_ForgeDialog", typeof(RectTransform), typeof(Image));
            var rootRt = (RectTransform)_root.transform;
            rootRt.SetParent(screen, false);
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;
            rootRt.SetAsLastSibling();
            _root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            var panel = (RectTransform)panelGo.transform;
            panel.SetParent(rootRt, false);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            var background = panelGo.GetComponent<Image>();
            // The game's normal panel: the inventory's own background ("Bkg"), the one the character sheet copies and
            // Krogon has seen in game. (Play-test 2026-11-10: guessing the message box's panel picked up a loading-screen
            // picture instead, a troll behind the text.)
            var bkg = gui.m_player ? gui.m_player.Find("Bkg") : null;
            var art = bkg ? bkg.GetComponent<Image>() : null;
            if (art && art.sprite)
            {
                background.sprite = art.sprite;
                background.type = art.type;
                background.material = art.material;
                background.pixelsPerUnitMultiplier = art.pixelsPerUnitMultiplier;
                background.color = art.color;
            }
            else
            {
                background.color = PlainPanel;
                Jotunn.Logger.LogWarning("Forge dialog: the inventory's panel background wasn't found; using a plain dark panel.");
            }

            // A plain dark sheet under the art, a little inside its edge: whatever the art does, text is never left
            // floating over the inventory (play-test 2026-11-10: the first dialog drew no background).
            var backing = new GameObject("Backing", typeof(RectTransform), typeof(Image));
            backing.transform.SetParent(rootRt, false);
            var backingRt = (RectTransform)backing.transform;
            backingRt.anchorMin = backingRt.anchorMax = backingRt.pivot = new Vector2(0.5f, 0.5f);
            backing.GetComponent<Image>().color = PlainPanel;
            backing.transform.SetSiblingIndex(0);

            TMP_Text Make(string objectName, string text, float size, Color color, TextAlignmentOptions alignment)
            {
                var go = new GameObject(objectName, typeof(RectTransform));
                go.transform.SetParent(panel, false);
                // Switched off while the text component is added: a text that wakes up without a font asks for the
                // engine's default one, which the game doesn't ship, and logs a warning each time (Krogon's log, 2026-11-11).
                go.SetActive(false);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.font = bodyStyle.font;
                tmp.fontSharedMaterial = bodyStyle.fontSharedMaterial;
                tmp.fontSize = size * u;
                tmp.color = color;
                tmp.alignment = alignment;
                tmp.richText = true;
                tmp.raycastTarget = false;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.text = text ?? "";
                go.SetActive(true);
                return tmp;
            }

            float Measure(TMP_Text tmp, float width)
            {
                return tmp.GetPreferredValues(tmp.text, width * u, 0f).y / u;
            }

            void Place(Component part, ForgeDialogLayout.Box box)
            {
                var rt = (RectTransform)part.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(box.X * u, -box.Y * u);
                rt.sizeDelta = new Vector2(box.W * u, box.H * u);
            }

            Image Block(string objectName, Color color, Transform parent = null)
            {
                var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent ? parent : panel, false);
                var image = go.GetComponent<Image>();
                image.color = color;
                image.raycastTarget = false;
                return image;
            }

            // The texts first, so their wrapped heights can be measured before anything is placed.
            var nameText = Make("Name", content.ItemName, 21f, Body, TextAlignmentOptions.MidlineLeft);
            var costText = Make("Cost", content.Cost, 13.5f, Secondary, TextAlignmentOptions.TopLeft);
            var warningText = Make("Warning", "<color=" + WarningColor + ">" + content.Warning + "</color>", 16f, Body, TextAlignmentOptions.TopLeft);
            var listHeading = Make("ListHeading", content.ListHeading, 14f, Heading, TextAlignmentOptions.BottomLeft);
            var chanceHeading = Make("ChanceHeading", ChanceHeading, 14f, Heading, TextAlignmentOptions.BottomRight);
            var explanation = Make("Explanation", content.Explanation, 14f, Secondary, TextAlignmentOptions.TopLeft);
            var tickLabel = Make("DontWarnLabel", DontWarnLabel, 14f, Body, TextAlignmentOptions.MidlineLeft);
            var descriptions = new List<TMP_Text>();
            var chances = new List<TMP_Text>();
            var heights = new List<float>();
            for (int i = 0; i < content.Rows.Count; i++)
            {
                var description = Make("Row" + i, content.Rows[i], 16f, Body, TextAlignmentOptions.TopLeft);
                descriptions.Add(description);
                heights.Add(Measure(description, ForgeDialogLayout.DescriptionWidth));
                chances.Add(Make("Chance" + i, i < content.Chances.Count ? content.Chances[i] : "", 16f, Body, TextAlignmentOptions.TopRight));
            }
            // One line of body text: a row is never shorter than its chance.
            float lineHeight = chances.Count > 0 ? Measure(chances[0], ForgeDialogLayout.ChanceWidth) : 22f;
            var layout = ForgeDialogLayout.Compute(Measure(nameText, ForgeDialogLayout.NameWidth), Measure(warningText, ForgeDialogLayout.ContentWidth),
                heights, Measure(explanation, ForgeDialogLayout.ContentWidth), lineHeight, screen.rect.height / u);

            panel.sizeDelta = new Vector2(ForgeDialogLayout.Width * u, layout.Height * u);
            panel.localScale = Vector3.one * layout.Scale;
            backingRt.sizeDelta = panel.sizeDelta - new Vector2(24f * u, 24f * u);
            backingRt.localScale = panel.localScale;

            // The title in the message box's heading style.
            var title = Make("Title", content.Title, 28f, Heading, TextAlignmentOptions.Center);
            if (headingStyle)
            {
                title.font = headingStyle.font;
                title.fontSharedMaterial = headingStyle.fontSharedMaterial;
                title.color = headingStyle.color;
            }
            title.textWrappingMode = TextWrappingModes.NoWrap;
            Place(title, layout.Title);

            Place(Block("IconBack", new Color(0f, 0f, 0f, 0.35f)), layout.Icon);
            var icon = Block("Icon", Color.white);
            icon.sprite = content.Item.GetIcon();
            icon.preserveAspect = true;
            Place(icon, layout.Icon);
            Place(nameText, layout.Name);
            Place(costText, layout.Cost);
            Place(warningText, layout.Warning);
            Place(listHeading, layout.ListHeader);
            Place(chanceHeading, layout.ChanceHeader);
            Place(Block("Divider", Rule), layout.Divider);
            for (int i = 0; i < descriptions.Count; i++)
            {
                Place(descriptions[i], layout.Descriptions[i]);
                Place(chances[i], layout.Chances[i]);
            }
            Place(explanation, layout.Explanation);

            // The tickbox: its own row, left-aligned. The box and its label both take the click.
            DontWarn = false;
            var box = Block("DontWarnBox", new Color(0.78f, 0.72f, 0.47f));
            Place(box, layout.Tick);
            var inside = Block("Inside", new Color(0.1f, 0.09f, 0.08f), box.transform);
            Stretch(inside.rectTransform, 2f * u);
            _tick = Block("Tick", Heading, box.transform).gameObject;
            Stretch((RectTransform)_tick.transform, 5f * u);
            _tick.SetActive(false);
            Place(tickLabel, layout.TickLabel);
            var hit = Block("DontWarnHit", new Color(0f, 0f, 0f, 0f));
            hit.raycastTarget = true;
            Place(hit, new ForgeDialogLayout.Box
            {
                X = layout.Tick.X, Y = layout.TickLabel.Y, H = layout.TickLabel.H,
                W = ForgeDialogLayout.TickSize + ForgeDialogLayout.TickGap + Mathf.Min(layout.TickLabel.W, tickLabel.GetPreferredValues().x / u)
            });
            hit.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                DontWarn = !DontWarn;
                if (_tick)
                {
                    _tick.SetActive(DontWarn);
                }
            });

            // The footer's own band, marked off from the text.
            Place(Block("FooterRule", Rule), new ForgeDialogLayout.Box
            {
                X = ForgeDialogLayout.Padding, Y = layout.Footer.Y, W = ForgeDialogLayout.ContentWidth, H = 2f
            });

            void AddButton(Button original, string label, ForgeDialogLayout.Box at, Action onClick)
            {
                var button = ForgeView.CopyButton(original ? original : gui.m_craftButton, panel, label, onClick);
                if (!button)
                {
                    return;
                }
                button.interactable = true;
                Place(button, at);
                // Only the label stays: the message box's buttons carry a gamepad key hint, which copied here showed
                // "MISSING BUTTON DEF" down the dialog (play-test 2026-11-10).
                var labelText = button.GetComponentInChildren<TMP_Text>(true);
                foreach (var other in button.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (other != labelText)
                    {
                        other.gameObject.SetActive(false);
                    }
                }
                foreach (var component in button.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component && component.GetType().Name == "UIGamePad")
                    {
                        var hint = component.GetType().GetField("m_hint")?.GetValue(component) as GameObject;
                        if (hint)
                        {
                            hint.SetActive(false);
                        }
                    }
                }
                var tip = button.GetComponent<UITooltip>();
                if (tip)
                {
                    tip.enabled = false;
                }
            }

            AddButton(popup ? popup.buttonLeft : null, "Cancel", layout.Cancel, onCancel);
            AddButton(popup ? popup.buttonRight : null, content.ConfirmLabel, layout.Confirm, onConfirm);
            return true;
        }

        private static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        public static void Close()
        {
            if (_root)
            {
                UnityEngine.Object.Destroy(_root);
            }
            _root = null;
            _tick = null;
            DontWarn = false;
        }
    }
}
