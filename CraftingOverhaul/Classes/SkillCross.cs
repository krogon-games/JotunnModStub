using System;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The four skill slots as a cross at the bottom left of the HUD (brief §2.2, reference
    ///     <c>docs/images/hud_reference_skills.jpg</c>), where the game's health display was: slot 0 on top, 1 left,
    ///     2 right, 3 at the bottom. Each shows the skill's icon and hotkey; the slot on the skill button is outlined in
    ///     gold; a dark sweep shows the cooldown; a slot that can't be used is greyed with the reason. Hidden without a
    ///     class. Only shows state: the Orlog plugin handles the keys and the skill button. Filled by dragging skills from
    ///     the skills panel (<see cref="SkillsPanel" />).
    /// </summary>
    internal static class SkillCross
    {
        public const float Tile = 58f;
        private const float Gap = 6f;

        private static RectTransform _root;
        private static readonly Slot[] SlotViews = new Slot[DemoClasses.Slots];
        private static Text _button;
        private static int _lastCounter = -1;
        private static string _bar = "";

        private sealed class Slot
        {
            public RectTransform Root;
            public Image Highlight;
            public Image Icon;
            public Image Cooldown;
            public Text Key;
            public Text Blocked;
            public string SkillId = "";
        }

        /// <summary>The slot under a screen point, or -1. Used when a skill is dragged out of the skills panel.</summary>
        public static int SlotAt(Vector2 screenPoint)
        {
            if (!_root || !_root.gameObject.activeInHierarchy)
            {
                return -1;
            }
            var canvas = _root.GetComponentInParent<Canvas>();
            Camera camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            for (int i = 0; i < SlotViews.Length; i++)
            {
                if (SlotViews[i] != null && RectTransformUtility.RectangleContainsScreenPoint(SlotViews[i].Root, screenPoint, camera))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>Cross positions: top, left, right, bottom, in tile units from the cross's bottom-left.</summary>
        private static readonly Vector2[] Cells = { new Vector2(1f, 2f), new Vector2(0f, 1f), new Vector2(2f, 1f), new Vector2(1f, 0f) };

        private static void EnsureCreated(Hud hud)
        {
            if (_root || !hud.m_rootObject)
            {
                return;
            }
            _root = UiKit.Rect("CraftingOverhaul_SkillCross", hud.m_rootObject.transform);
            float size = 3f * Tile + 2f * Gap;
            UiKit.Place(_root, Vector2.zero, Vector2.zero, ModConfig.SkillCrossPosition.Value, new Vector2(size, size));
            for (int i = 0; i < SlotViews.Length; i++)
            {
                var view = new Slot();
                view.Highlight = UiKit.Image("Highlight" + i, _root, SheetArt.Tile, Image.Type.Sliced);
                view.Highlight.color = new Color(1f, 0.78f, 0.3f, 0.95f);
                var frame = UiKit.Image("Slot" + i, _root, SheetArt.Tile, Image.Type.Sliced);
                if (!frame.sprite)
                {
                    frame.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
                }
                view.Root = frame.rectTransform;
                Vector2 cell = Cells[i] * (Tile + Gap);
                UiKit.Place(view.Root, Vector2.zero, Vector2.zero, cell, new Vector2(Tile, Tile));
                UiKit.Place(view.Highlight.rectTransform, Vector2.zero, Vector2.zero, cell - new Vector2(4f, 4f), new Vector2(Tile + 8f, Tile + 8f));

                view.Icon = UiKit.Image("Icon", view.Root, null);
                view.Icon.preserveAspect = true;
                UiKit.Stretch(view.Icon.rectTransform, 9f);

                view.Cooldown = UiKit.Image("Cooldown", view.Root, SheetArt.BarFill);
                view.Cooldown.color = new Color(0f, 0f, 0f, 0.65f);
                view.Cooldown.type = Image.Type.Filled;
                view.Cooldown.fillMethod = Image.FillMethod.Radial360;
                view.Cooldown.fillOrigin = (int)Image.Origin360.Top;
                view.Cooldown.fillClockwise = false;
                UiKit.Stretch(view.Cooldown.rectTransform, 7f);

                view.Key = UiKit.Text("Key", view.Root, 13, Color.white, TextAnchor.UpperLeft);
                UiKit.Place(view.Key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(7f, -4f), new Vector2(30f, 16f));
                view.Blocked = UiKit.Text("Blocked", view.Root, 11, new Color(1f, 0.55f, 0.45f), TextAnchor.LowerCenter);
                UiKit.Place(view.Blocked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(Tile, 14f));
                SlotViews[i] = view;
            }
            _button = UiKit.Text("SkillButton", _root, 12, UiKit.Gold, TextAnchor.UpperCenter);
            UiKit.Place(_button.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(size + 60f, 16f));
            Jotunn.Logger.LogInfo("Skill cross added.");
        }

        private static void Update(Hud hud)
        {
            if (!ClassesSource.Available || !Player.m_localPlayer)
            {
                if (_root)
                {
                    _root.gameObject.SetActive(false);
                }
                return;
            }
            int counter = ClassesSource.ChangeCounter;
            if (counter != _lastCounter || Time.frameCount % 60 == 0)
            {
                _lastCounter = counter;
                _bar = ClassesSource.SkillBar();
            }
            bool show = !string.IsNullOrEmpty(_bar);
            if (!show)
            {
                if (_root)
                {
                    _root.gameObject.SetActive(false);
                }
                return;
            }
            EnsureCreated(hud);
            if (!_root)
            {
                return;
            }
            _root.gameObject.SetActive(true);
            _root.anchoredPosition = ModConfig.SkillCrossPosition.Value;

            int selected = FlatJson.GetInt(_bar, "selected", -1);
            for (int i = 0; i < SlotViews.Length; i++)
            {
                var view = SlotViews[i];
                string id = FlatJson.Get(_bar, "slot" + i) ?? "";
                if (id != view.SkillId)
                {
                    view.SkillId = id;
                    var icon = id.Length > 0 ? ClassesSource.SkillIcon(id) : null;
                    view.Icon.sprite = icon;
                    view.Icon.enabled = icon;
                }
                UiKit.SetText(view.Key, FlatJson.Get(_bar, "key" + i) ?? "");
                view.Highlight.enabled = i == selected && id.Length > 0;
                ShowState(view, id.Length > 0 ? ClassesSource.SlotState(i) : "");
            }
            string button = FlatJson.Get(_bar, "skillButton") ?? "";
            UiKit.SetText(_button, button.Length > 0 ? "Skill button: " + button : "");
        }

        private static void ShowState(Slot view, string state)
        {
            if (string.IsNullOrEmpty(state))
            {
                view.Cooldown.fillAmount = 0f;
                UiKit.SetText(view.Blocked, "");
                view.Icon.color = Color.white;
                return;
            }
            bool usable = FlatJson.GetBool(state, "usable", true);
            string blocked = FlatJson.Get(state, "blocked") ?? "";
            float cooldown = FlatJson.GetFloat(state, "cooldown", 0f);
            float total = FlatJson.GetFloat(state, "cooldownTotal", 0f);
            view.Cooldown.fillAmount = total > 0f ? Mathf.Clamp01(cooldown / total) : 0f;
            // Cooldown shows as the sweep; other reasons grey the icon and say why.
            view.Icon.color = usable || blocked == "cooldown" ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.9f);
            UiKit.SetText(view.Blocked, blocked == "weapon" ? "Weapon" : blocked == "eitr" ? "Eitr" : "");
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Update))]
        private static class Update_Patch
        {
            private static void Postfix(Hud __instance)
            {
                try
                {
                    Update(__instance);
                }
                catch (Exception ex)
                {
                    Safe.Report("show the skill cross", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                _root = null;
                _lastCounter = -1;
                for (int i = 0; i < SlotViews.Length; i++)
                {
                    SlotViews[i] = null;
                }
            }
        }
    }
}
