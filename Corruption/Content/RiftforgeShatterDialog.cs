using System;
using System.Collections.Generic;
using Corruption.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Corruption.Content
{
    /// <summary>
    ///     The Shatter confirmation (Krogon's brief, 2026-11-10): its own dialog instead of the game's Yes/No box, in the
    ///     game's panel art, fonts and buttons. Title, the item's icon and name, "Cost: Free", the warning once, a
    ///     two-column list (affixes in the Forge's formatting, their chance right-aligned), a short explanation, and a
    ///     footer with Cancel and Shatter. Positions and the height come from <see cref="ShatterDialogLayout" />, fed with
    ///     the measured heights of the wrapped texts, so the dialog is as tall as its content and shrinks as a whole when
    ///     the screen is too small. It asks every time: there is no "don't warn me again" here. Only shows and reports
    ///     the click; the rules run in <see cref="RiftforgeCrafting" />.
    /// </summary>
    internal static class ShatterDialog
    {
        public const string Title = "Shatter item";
        public const string Warning = "This permanently destroys the item.";
        public const string Cost = "Cost: Free";
        public const string ListHeading = "Potential splinters";
        public const string ChanceHeading = "Chance";
        public const string Explanation = "Each affix rolls independently; you may recover none.\nRecovered splinters keep their exact values.";

        private const string WarningColor = "#FF8060";
        private static readonly Color Body = new Color(0.93f, 0.89f, 0.81f);
        private static readonly Color Secondary = new Color(0.72f, 0.68f, 0.62f);
        private static readonly Color Heading = new Color(1f, 0.631f, 0.235f);
        private static readonly Color Rule = new Color(1f, 1f, 1f, 0.14f);

        private static GameObject _root;

        public static bool IsOpen => _root;

        /// <summary>
        ///     Opens the dialog. <paramref name="rows" /> are the affix lines as the Forge shows them (bulleted, coloured);
        ///     <paramref name="chance" /> is the recovery chance from the rules, as text ("50%"). False when the game's
        ///     panel parts can't be found (the caller then asks another way).
        /// </summary>
        public static bool Open(InventoryGui gui, ItemDrop.ItemData item, string name, IList<string> rows, string chance, Action onConfirm, Action onCancel)
        {
            Close();
            var style = gui ? gui.m_recipeDecription : null;
            var canvas = gui ? gui.GetComponentInParent<Canvas>() : null;
            var wood = gui && gui.m_crafting ? ForgeView.LargestImage(gui.m_crafting) : null;
            if (!style || !canvas || !wood || !gui.m_craftButton || item == null)
            {
                return false;
            }
            var screen = (RectTransform)canvas.rootCanvas.transform;
            // Sizes are written for 16-point body text; follow the game's own size.
            float u = style.fontSize > 0f ? style.fontSize / 16f : 1f;

            // Everything behind the dialog is dimmed and takes no clicks while it is open.
            _root = new GameObject("Corruption_ShatterDialog", typeof(RectTransform), typeof(Image));
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
            background.sprite = wood.sprite;
            background.type = wood.type;
            background.material = wood.material;
            background.color = wood.color;
            background.pixelsPerUnitMultiplier = wood.pixelsPerUnitMultiplier;

            TMP_Text Make(string objectName, string text, float size, Color color, TextAlignmentOptions alignment)
            {
                var go = new GameObject(objectName, typeof(RectTransform));
                go.transform.SetParent(panel, false);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.font = style.font;
                tmp.fontSharedMaterial = style.fontSharedMaterial;
                tmp.fontSize = size * u;
                tmp.color = color;
                tmp.alignment = alignment;
                tmp.richText = true;
                tmp.raycastTarget = false;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.text = text;
                return tmp;
            }

            float Measure(TMP_Text tmp, float width)
            {
                return tmp.GetPreferredValues(tmp.text, width * u, 0f).y / u;
            }

            void Place(Component part, ShatterDialogLayout.Box box)
            {
                var rt = (RectTransform)part.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(box.X * u, -box.Y * u);
                rt.sizeDelta = new Vector2(box.W * u, box.H * u);
            }

            Image Block(string objectName, Color color)
            {
                var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(panel, false);
                var image = go.GetComponent<Image>();
                image.color = color;
                image.raycastTarget = false;
                return image;
            }

            // The texts first, so their wrapped heights can be measured before anything is placed.
            var nameText = Make("Name", name, 21f, Body, TextAlignmentOptions.MidlineLeft);
            var costText = Make("Cost", Cost, 13.5f, Secondary, TextAlignmentOptions.TopLeft);
            var warningText = Make("Warning", "<color=" + WarningColor + ">" + Warning + "</color>", 16f, Body, TextAlignmentOptions.TopLeft);
            var listHeading = Make("ListHeading", ListHeading, 14f, Heading, TextAlignmentOptions.BottomLeft);
            var chanceHeading = Make("ChanceHeading", ChanceHeading, 14f, Heading, TextAlignmentOptions.BottomRight);
            var explanation = Make("Explanation", Explanation, 14f, Secondary, TextAlignmentOptions.TopLeft);
            var descriptions = new List<TMP_Text>();
            var chances = new List<TMP_Text>();
            var heights = new List<float>();
            for (int i = 0; i < rows.Count; i++)
            {
                var description = Make("Affix" + i, rows[i], 16f, Body, TextAlignmentOptions.TopLeft);
                descriptions.Add(description);
                heights.Add(Measure(description, ShatterDialogLayout.DescriptionWidth));
                chances.Add(Make("Chance" + i, chance, 16f, Body, TextAlignmentOptions.TopRight));
            }
            // One line of body text: a row is never shorter than its chance.
            float lineHeight = chances.Count > 0 ? Measure(chances[0], ShatterDialogLayout.ChanceWidth) : 22f;
            var layout = ShatterDialogLayout.Compute(Measure(nameText, ShatterDialogLayout.NameWidth), Measure(warningText, ShatterDialogLayout.ContentWidth),
                heights, Measure(explanation, ShatterDialogLayout.ContentWidth), lineHeight, screen.rect.height / u);

            panel.sizeDelta = new Vector2(ShatterDialogLayout.Width * u, layout.Height * u);
            panel.localScale = Vector3.one * layout.Scale;

            // The title in the game's heading style (the station's name above the crafting panel).
            var headingStyle = gui.m_craftingStationName ? gui.m_craftingStationName : gui.m_recipeName;
            var title = Make("Title", ForgeRules.UpperLike(headingStyle ? headingStyle.text : "", Title), 26f, Heading, TextAlignmentOptions.Center);
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
            icon.sprite = item.GetIcon();
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
            // The footer's own band, marked off from the text.
            Place(Block("FooterRule", Rule), new ShatterDialogLayout.Box
            {
                X = ShatterDialogLayout.Padding, Y = layout.Footer.Y, W = ShatterDialogLayout.ContentWidth, H = 2f
            });

            void AddButton(string label, ShatterDialogLayout.Box box, Action onClick)
            {
                var button = ForgeView.CopyButton(gui.m_craftButton, panel, label, onClick);
                if (!button)
                {
                    return;
                }
                button.interactable = true;
                Place(button, box);
                var tip = button.GetComponent<UITooltip>();
                if (tip)
                {
                    tip.enabled = false;
                }
            }

            AddButton("Cancel", layout.Cancel, onCancel);
            AddButton("Shatter", layout.Confirm, onConfirm);
            return true;
        }

        public static void Close()
        {
            if (_root)
            {
                UnityEngine.Object.Destroy(_root);
            }
            _root = null;
        }
    }
}
