using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>
    ///     The art of the character sheet and the class screens, made by <c>tools/make_sheet_ui.py</c> and embedded in the DLL as raw pixels
    ///     (<c>Assets/Sheet/*.rgba</c>: 8-byte width/height header, rows bottom to top; Unity's image decoder can't be
    ///     used from a net462 build, see ARCHITECTURE.md). Loaded once, on first use. A missing piece gives null, and
    ///     the sheet then draws without it.
    /// </summary>
    internal static class SheetArt
    {
        private const string ResourcePrefix = "CraftingOverhaul.Assets.Sheet.";
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Tile => Get("tile", new Vector4(18, 18, 18, 18));
        public static Sprite Close => Get("close", Vector4.zero);
        public static Sprite Bar => Get("bar", new Vector4(6, 4, 6, 4));
        public static Sprite BarCap => Get("bar_cap", Vector4.zero);
        public static Sprite BarFill => Get("bar_fill", Vector4.zero);
        public static Sprite Panel => Get("panel", new Vector4(20, 20, 20, 20));
        public static Sprite Inset => Get("inset", new Vector4(10, 10, 10, 10));
        public static Sprite Medallion => Get("medallion", Vector4.zero);
        public static Sprite GlowRing => Get("glow_ring", Vector4.zero);
        public static Sprite GlowLine => Get("glow_line", Vector4.zero);
        public static Sprite GlowBox => Get("glow_box", new Vector4(24, 24, 24, 24));

        /// <summary>The glow ring's ring sits on a medallion's rim when drawn this many times the medallion's size.</summary>
        public const float GlowRingScale = 128f / 96f;

        /// <summary>The slot glow hugs a slot when drawn this much bigger on every side (make_sheet_ui.py GLOW_BOX_INSET).</summary>
        public const float GlowBoxInset = 14f;

        /// <summary>
        ///     Gives an image the game's own panel background: the inventory's <c>Bkg</c> (the object EquipmentAndQuickSlots
        ///     also copies), with its sprite, material and colour (user, 2026-10-30: "use Valheim's own UI" for panels). A
        ///     darker copy for boxes on a panel (<paramref name="inset" />). Our look-alike art when the game's can't be found.
        /// </summary>
        public static void StyleAsGamePanel(UnityEngine.UI.Image image, bool inset = false)
        {
            UnityEngine.UI.Image source = null;
            var gui = InventoryGui.instance;
            if (gui && gui.m_player)
            {
                var bkg = gui.m_player.Find("Bkg");
                source = bkg ? bkg.GetComponent<UnityEngine.UI.Image>() : null;
            }
            if (source && source.sprite)
            {
                image.sprite = source.sprite;
                image.type = source.type;
                image.material = source.material;
                image.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
                var c = source.color;
                image.color = inset ? new Color(c.r * 0.45f, c.g * 0.42f, c.b * 0.4f, Mathf.Min(1f, c.a * 1.1f)) : c;
                return;
            }
            image.sprite = inset ? Inset : Panel;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.material = null;
            image.color = image.sprite ? Color.white : inset ? new Color(0.09f, 0.06f, 0.04f, 0.72f) : new Color(0.2f, 0.15f, 0.1f, 0.9f);
        }

        /// <summary>The arch pieces stretch only along their straight sides: the arch on top and the pedestal stay fixed.</summary>
        public static readonly Vector4 ArchBorder = new Vector4(134, 72, 134, 147);
        public static Sprite ArchBack => Get("arch_back", ArchBorder);
        public static Sprite ArchRim => Get("arch_rim", ArchBorder);

        private static Sprite Get(string name, Vector4 border, bool repeat = false)
        {
            if (Cache.TryGetValue(name, out var cached))
            {
                return cached;
            }
            Sprite sprite = null;
            try
            {
                var texture = Load(name, repeat);
                if (texture)
                {
                    // 100 pixels per unit, as the game's canvas: one texel is one UI unit.
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0,
                        SpriteMeshType.FullRect, border);
                    sprite.name = "CraftingOverhaul_" + name;
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Could not load the character sheet's '{name}' art: {ex.Message}");
            }
            Cache[name] = sprite;
            return sprite;
        }

        private static Texture2D Load(string name, bool repeat)
        {
            byte[] bytes;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + name + ".rgba"))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogWarning($"Character sheet art '{name}' is missing from the DLL.");
                    return null;
                }
                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    bytes = memory.ToArray();
                }
            }
            int width = BitConverter.ToInt32(bytes, 0);
            int height = BitConverter.ToInt32(bytes, 4);
            if (width <= 0 || height <= 0 || bytes.Length != 8 + width * height * 4)
            {
                Jotunn.Logger.LogWarning($"Character sheet art '{name}' has the wrong size.");
                return null;
            }
            var pixels = new Color32[width * height];
            for (int i = 0, b = 8; i < pixels.Length; i++, b += 4)
            {
                pixels[i] = new Color32(bytes[b], bytes[b + 1], bytes[b + 2], bytes[b + 3]);
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "CraftingOverhaul_" + name };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            texture.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            return texture;
        }
    }
}
