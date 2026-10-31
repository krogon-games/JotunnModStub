using CraftingOverhaul.CharacterSheet;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>Small helpers to build the HUD and class screens with the character sheet's art and fonts.</summary>
    internal static class UiKit
    {
        public static readonly Color Gold = new Color(0.91f, 0.80f, 0.59f);
        public static readonly Color Ink = new Color(0.23f, 0.16f, 0.10f);
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.35f);

        // The game's own colours for the three bars (Hud: health red, stamina yellow, eitr blue).
        public static readonly Color Health = new Color(0.78f, 0.13f, 0.10f);
        public static readonly Color Eitr = new Color(0.27f, 0.48f, 0.90f);
        public static readonly Color Stamina = new Color(0.95f, 0.76f, 0.18f);
        public static readonly Color Xp = new Color(0.62f, 0.40f, 0.85f);
        /// <summary>The game's orange for adrenaline.</summary>
        public static readonly Color Adrenaline = new Color(0.98f, 0.55f, 0.12f);

        public static RectTransform Rect(string name, Transform parent)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static Image Image(string name, Transform parent, Sprite sprite, Image.Type type = UnityEngine.UI.Image.Type.Simple, bool raycast = false)
        {
            var image = Rect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite ? type : UnityEngine.UI.Image.Type.Simple;
            image.raycastTarget = raycast;
            return image;
        }

        public static Text Text(string name, Transform parent, int size, Color colour, TextAnchor alignment, bool outline = true, Font font = null)
        {
            var gm = GUIManager.Instance;
            var go = gm.CreateText("", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                font ? font : gm.AveriaSerifBold, size, colour, outline, Color.black, 100f, 20f, false);
            go.name = name;
            var text = go.GetComponent<Text>();
            text.alignment = alignment;
            text.supportRichText = true;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        /// <summary>Anchors and pivots a rect at one corner or edge point of its parent (0–1 each way), then sizes it.</summary>
        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt, float inset = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        /// <summary>A window on the game's own panel background (see <see cref="SheetArt.StyleAsGamePanel" />).</summary>
        public static RectTransform Panel(string name, Transform parent, Vector2 size)
        {
            var back = Image(name, parent, null, UnityEngine.UI.Image.Type.Simple, true);
            SheetArt.StyleAsGamePanel(back);
            var rt = back.rectTransform;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>A darker box on a panel, for groups such as the attributes.</summary>
        public static Image Inset(string name, Transform parent, bool raycast = false)
        {
            var box = Image(name, parent, null, UnityEngine.UI.Image.Type.Simple, raycast);
            SheetArt.StyleAsGamePanel(box, true);
            return box;
        }

        /// <summary>Text on the game's panels: warm off-white, as the game's own.</summary>
        public static readonly Color Light = new Color(0.93f, 0.89f, 0.81f);
        public static readonly Color LightDim = new Color(0.78f, 0.72f, 0.63f);
        /// <summary>Learned skills, their paths and the selected slot glow eitr-blue, like runes (mock-up D).</summary>
        public static readonly Color Rune = new Color(0.43f, 0.78f, 1f);
        public static readonly Color CanSpend = new Color(0.55f, 0.94f, 0.43f);

        public static Button Button(string label, Transform parent, Vector2 size, System.Action onClick)
        {
            var go = GUIManager.Instance.CreateButton(label, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size.x, size.y);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => onClick());
            return button;
        }

        public static void SetText(Text text, string value)
        {
            if (text && text.text != value)
            {
                text.text = value;
            }
        }
    }
}
