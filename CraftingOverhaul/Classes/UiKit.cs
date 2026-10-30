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

        /// <summary>The sheet's wood panel with its iron frame, for a window.</summary>
        public static RectTransform Panel(string name, Transform parent, Vector2 size)
        {
            var wood = Image(name, parent, SheetArt.Wood, UnityEngine.UI.Image.Type.Tiled, true);
            if (!wood.sprite)
            {
                wood.color = new Color(0.22f, 0.14f, 0.09f);
            }
            var rt = wood.rectTransform;
            rt.sizeDelta = size;
            var frame = Image("Frame", rt, SheetArt.Frame, UnityEngine.UI.Image.Type.Sliced);
            Stretch(frame.rectTransform);
            return rt;
        }

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
