using System;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The four skill slots as a cross at the bottom left of the HUD (brief §2.2, reference
    ///     <c>docs/images/hud_reference_skills.jpg</c>): slot 0 on top, 1 left, 2 right, 3 at the bottom, spread out so the
    ///     Forsaken power sits in the middle as a round medallion (user, 2026-10-31), in place of the game's power display
    ///     with its name and "Ready" text. Each slot shows the icon and key; the slot on the skill button glows eitr-blue.
    ///     Cooldowns follow <see cref="CooldownDisplay" />: dark icon, sweep and seconds left while waiting, full colour and
    ///     a flash once ready. A skill that can't be used for another reason is tinted red with the reason. Hidden without
    ///     a class (the game's power display then shows as usual). Only shows state: the Orlog plugin handles the keys and
    ///     the skill button. Filled by dragging skills from the skills panel (<see cref="SkillsPanel" />).
    /// </summary>
    internal static class SkillCross
    {
        public const float Tile = 58f;
        /// <summary>Space between the slots, wide enough for the power in the middle (user, 2026-10-31).</summary>
        private const float Gap = 14f;
        private const float PowerSize = 58f;

        private static readonly Color CoolingIcon = new Color(0.32f, 0.32f, 0.36f, 1f);
        private static readonly Color CoolingFrame = new Color(0.55f, 0.55f, 0.55f, 1f);
        private static readonly Color BlockedIcon = new Color(0.6f, 0.3f, 0.28f, 1f);
        private static readonly Color Flash = new Color(1f, 0.93f, 0.7f);

        private static RectTransform _root;
        private static readonly Slot[] SlotViews = new Slot[DemoClasses.Slots];
        private static Slot _power;
        private static int _lastCounter = -1;
        private static string _bar = "";
        private static RectTransform _gameGpRoot;

        /// <summary>One slot (a skill, or the power in the middle) and its cooldown state.</summary>
        private sealed class Slot
        {
            public RectTransform Root;
            public Image Frame;
            public Image Highlight;
            public Image ReadyFlash;
            public Image Icon;
            public Image Sweep;
            public Text Time;
            public Text Key;
            public Text Blocked;
            public string Id = "";
            public float LastRemaining;
            public float ReadyAt = -100f;
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

        /// <summary>Cross positions: top, left, right, bottom, in tile units from the cross's bottom-left; the power is in the middle.</summary>
        private static readonly Vector2[] Cells = { new Vector2(1f, 2f), new Vector2(0f, 1f), new Vector2(2f, 1f), new Vector2(1f, 0f) };

        private static void EnsureCreated(Hud hud)
        {
            if (_root || !hud.m_rootObject)
            {
                return;
            }
            _root = UiKit.Rect("CraftingOverhaul_SkillCross", hud.m_rootObject.transform);
            _root.gameObject.AddComponent<LateHider>();
            float size = 3f * Tile + 2f * Gap;
            UiKit.Place(_root, Vector2.zero, Vector2.zero, ModConfig.SkillCrossPosition.Value, new Vector2(size, size));
            for (int i = 0; i < SlotViews.Length; i++)
            {
                SlotViews[i] = NewSlot("Slot" + i, Cells[i] * (Tile + Gap), Tile, false);
            }
            Vector2 middle = new Vector2(1f, 1f) * (Tile + Gap) + new Vector2((Tile - PowerSize) / 2f, (Tile - PowerSize) / 2f);
            _power = NewSlot("Power", middle, PowerSize, true);
            Jotunn.Logger.LogInfo("Skill cross added.");
        }

        private static Slot NewSlot(string name, Vector2 position, float size, bool round)
        {
            var view = new Slot();
            float glow = round ? size * (SheetArt.GlowRingScale - 1f) / 2f : SheetArt.GlowBoxInset;
            Sprite glowSprite = round ? SheetArt.GlowRing : SheetArt.GlowBox;
            Image.Type glowType = round ? Image.Type.Simple : Image.Type.Sliced;

            // The slot on the skill button glows eitr-blue round its rim (mock-up D, user 2026-10-30).
            view.Highlight = UiKit.Image(name + "Highlight", _root, glowSprite, glowType);
            view.Highlight.color = UiKit.Rune;
            UiKit.Place(view.Highlight.rectTransform, Vector2.zero, Vector2.zero, position - new Vector2(glow, glow), new Vector2(size + 2f * glow, size + 2f * glow));
            view.Highlight.enabled = false;
            view.ReadyFlash = UiKit.Image(name + "Flash", _root, glowSprite, glowType);
            UiKit.Place(view.ReadyFlash.rectTransform, Vector2.zero, Vector2.zero, position - new Vector2(glow, glow), new Vector2(size + 2f * glow, size + 2f * glow));
            view.ReadyFlash.enabled = false;

            view.Frame = UiKit.Image(name, _root, round ? SheetArt.Medallion : SheetArt.Tile, round ? Image.Type.Simple : Image.Type.Sliced);
            if (!view.Frame.sprite)
            {
                view.Frame.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
            }
            view.Root = view.Frame.rectTransform;
            UiKit.Place(view.Root, Vector2.zero, Vector2.zero, position, new Vector2(size, size));

            view.Icon = UiKit.Image("Icon", view.Root, null);
            view.Icon.enabled = false;
            view.Icon.preserveAspect = true;
            UiKit.Stretch(view.Icon.rectTransform, round ? 11f : 5f);

            // A dark pie over the part of the cooldown still to wait, shrinking clockwise from the top (round on the power).
            view.Sweep = UiKit.Image("Sweep", view.Root, round && SheetArt.Disc ? SheetArt.Disc : SheetArt.BarFill);
            view.Sweep.color = new Color(0f, 0f, 0f, 0.6f);
            view.Sweep.type = Image.Type.Filled;
            view.Sweep.fillMethod = Image.FillMethod.Radial360;
            view.Sweep.fillOrigin = (int)Image.Origin360.Top;
            view.Sweep.fillClockwise = false;
            view.Sweep.fillAmount = 0f;
            UiKit.Stretch(view.Sweep.rectTransform, round ? 9f : 7f);

            view.Time = UiKit.Text("Time", view.Root, 22, Color.white, TextAnchor.MiddleCenter);
            UiKit.Stretch(view.Time.rectTransform);

            view.Key = UiKit.Text("Key", view.Root, 13, Color.white, TextAnchor.UpperLeft);
            UiKit.Place(view.Key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(round ? 2f : 7f, -4f), new Vector2(30f, 16f));
            view.Blocked = UiKit.Text("Blocked", view.Root, 11, new Color(1f, 0.6f, 0.5f), TextAnchor.LowerCenter);
            UiKit.Place(view.Blocked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(size, 14f));
            return view;
        }

        private static void Update(Hud hud)
        {
            if (!ClassesSource.Available || !Player.m_localPlayer)
            {
                Hide(hud);
                return;
            }
            int counter = ClassesSource.ChangeCounter;
            if (counter != _lastCounter || Time.frameCount % 60 == 0)
            {
                _lastCounter = counter;
                _bar = ClassesSource.SkillBar();
            }
            if (string.IsNullOrEmpty(_bar))
            {
                Hide(hud);
                return;
            }
            EnsureCreated(hud);
            if (!_root)
            {
                return;
            }
            _root.gameObject.SetActive(true);
            _root.anchoredPosition = ModConfig.SkillCrossPosition.Value;
            _gameGpRoot = hud.m_gpRoot;

            int selected = FlatJson.GetInt(_bar, "selected", -1);
            for (int i = 0; i < SlotViews.Length; i++)
            {
                var view = SlotViews[i];
                string id = FlatJson.Get(_bar, "slot" + i) ?? "";
                if (id != view.Id)
                {
                    view.Id = id;
                    SetIcon(view, id.Length > 0 ? ClassesSource.SkillIcon(id) : null);
                    view.LastRemaining = 0f;
                    view.ReadyAt = -100f;
                }
                UiKit.SetText(view.Key, FlatJson.Get(_bar, "key" + i) ?? "");
                view.Highlight.enabled = i == selected && id.Length > 0;
                string state = id.Length > 0 ? ClassesSource.SlotState(i) : "";
                bool usable = FlatJson.GetBool(state, "usable", true);
                string blocked = FlatJson.Get(state, "blocked") ?? "";
                float remaining = FlatJson.GetFloat(state, "cooldown", 0f);
                float total = FlatJson.GetFloat(state, "cooldownTotal", 0f);
                // Say what's wrong in words the player can act on (user, 2026-10-31: "Weapon" and "Eitr" were confusing).
                string needs = FlatJson.Get(state, "needs") ?? "";
                string reason = usable || blocked == "cooldown" ? ""
                    : blocked == "weapon" ? (needs.Length > 0 ? "Need " + needs : "Wrong weapon")
                    : blocked == "eitr" ? "Low eitr" : "";
                ShowState(view, id.Length > 0, remaining, total, reason);
            }
            UpdatePower(Player.m_localPlayer);
        }

        /// <summary>The Forsaken power in the middle: the game's own state (<c>Player.GetGuardianPowerHUD</c>), drawn our way.</summary>
        private static void UpdatePower(Player player)
        {
            player.GetGuardianPowerHUD(out StatusEffect power, out float remaining);
            bool has = power;
            _power.Root.gameObject.SetActive(has);
            _power.Highlight.enabled = false;
            if (!has)
            {
                _power.ReadyFlash.enabled = false;
                return;
            }
            string id = power.name;
            if (id != _power.Id)
            {
                _power.Id = id;
                SetIcon(_power, power.m_icon);
                _power.LastRemaining = remaining;
                _power.ReadyAt = -100f;
            }
            UiKit.SetText(_power.Key, PowerKey());
            ShowState(_power, true, remaining, power.m_cooldown, "");
        }

        private static string _powerKey;

        private static string PowerKey()
        {
            if (_powerKey == null)
            {
                try
                {
                    _powerKey = Localization.instance.GetBoundKeyString("GP", true) ?? "";
                }
                catch (Exception)
                {
                    _powerKey = "";
                }
            }
            return _powerKey;
        }

        private static void SetIcon(Slot view, Sprite icon)
        {
            view.Icon.sprite = icon;
            view.Icon.enabled = icon;
        }

        private static void ShowState(Slot view, bool filled, float remaining, float total, string blockedReason)
        {
            if (!filled)
            {
                view.Sweep.fillAmount = 0f;
                UiKit.SetText(view.Time, "");
                UiKit.SetText(view.Blocked, "");
                view.Frame.color = view.Frame.sprite ? Color.white : view.Frame.color;
                view.ReadyFlash.enabled = false;
                return;
            }
            bool cooling = CooldownDisplay.OnCooldown(remaining);
            if (CooldownDisplay.BecameReady(view.LastRemaining, remaining))
            {
                view.ReadyAt = UnityEngine.Time.time;
            }
            view.LastRemaining = remaining;

            view.Sweep.fillAmount = CooldownDisplay.Sweep(remaining, total);
            UiKit.SetText(view.Time, CooldownDisplay.Label(remaining));
            UiKit.SetText(view.Blocked, cooling ? "" : blockedReason);
            view.Icon.color = cooling ? CoolingIcon : blockedReason.Length > 0 ? BlockedIcon : Color.white;
            if (view.Frame.sprite)
            {
                view.Frame.color = cooling ? CoolingFrame : Color.white;
            }

            // Ready: one bright flash that fades out (CooldownDisplay.Flash).
            float flash = CooldownDisplay.Flash(UnityEngine.Time.time - view.ReadyAt);
            view.ReadyFlash.enabled = flash > 0f && view.ReadyFlash.sprite;
            if (view.ReadyFlash.enabled)
            {
                view.ReadyFlash.color = new Color(Flash.r, Flash.g, Flash.b, flash);
                float scale = 1f + 0.15f * (1f - flash);
                view.ReadyFlash.rectTransform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static void Hide(Hud hud)
        {
            if (_root && _root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(false);
            }
            // The game's own power display comes back when the cross isn't shown.
            if (hud.m_gpRoot && hud.m_gpRoot.localScale != Vector3.one)
            {
                hud.m_gpRoot.localScale = Vector3.one;
            }
        }

        /// <summary>
        ///     Hides the game's power display (name, icon and "Ready") while the cross shows the power: shrunk to nothing,
        ///     after the game's own update, as the bars do with the game's health display.
        /// </summary>
        private sealed class LateHider : MonoBehaviour
        {
            private void LateUpdate()
            {
                if (_gameGpRoot && _gameGpRoot.localScale != Vector3.zero)
                {
                    _gameGpRoot.localScale = Vector3.zero;
                }
                try
                {
                    PlaceQuickSlots();
                }
                catch (Exception ex)
                {
                    Safe.Report("place the quick slots under the skill cross", ex);
                }
            }
        }

        private const float QuickSlotsGap = 10f;

        /// <summary>
        ///     EquipmentAndQuickSlots' quick-slot bar (its <c>QuickSlotsHotkeyBar</c>) goes centred under the cross while
        ///     the cross shows (user, 2026-10-31). That mod puts the bar back at its configured place in every Update, so
        ///     this runs after, in LateUpdate; when the cross hides, its own placement simply takes over again.
        /// </summary>
        private static void PlaceQuickSlots()
        {
            var hud = Hud.instance;
            if (!hud || !hud.m_rootObject || !_root)
            {
                return;
            }
            var root = hud.m_rootObject.transform as RectTransform;
            var bar = hud.m_rootObject.transform.Find("QuickSlotsHotkeyBar") as RectTransform;
            if (!root || !bar || !bar.gameObject.activeInHierarchy || !MovedHudPanels.Bounds(root, bar, out Rect drawn))
            {
                return;
            }
            Rect cross = _root.rect;
            Vector3 bottomCentre = root.InverseTransformPoint(_root.TransformPoint(new Vector3(cross.center.x, cross.yMin, 0f)));
            var offset = new Vector2(bottomCentre.x - drawn.center.x, bottomCentre.y - QuickSlotsGap - drawn.yMax);
            if (offset.sqrMagnitude > 0.25f)
            {
                bar.position += root.TransformVector(offset);
            }
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
                _power = null;
                _gameGpRoot = null;
                _lastCounter = -1;
                for (int i = 0; i < SlotViews.Length; i++)
                {
                    SlotViews[i] = null;
                }
            }
        }
    }
}
