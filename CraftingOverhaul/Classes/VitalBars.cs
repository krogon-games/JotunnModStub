using System;
using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Health, stamina and eitr as three long bars together at the bottom centre (user, 2026-10-30, after Elden Ring:
    ///     <c>docs/images/hud_reference_bars.jpg</c>): health on top, stamina under it, eitr at the bottom (only once you
    ///     have any; order changed by the user 2026-10-31 so gaining eitr doesn't move stamina), in the game's colours.
    ///     They grow with the maximum up to a cap, have a fading "damage taken" trail and the numbers past their end. Your
    ///     food sits in three round slots to their left (time left under each), and with Orlog (or the classes demo) a
    ///     thin XP bar runs under them, with the level and a hint when points wait to be spent. The game's own health,
    ///     stamina, eitr and food displays are hidden while they're on.
    ///
    ///     Values come straight from the game (<c>Player.GetHealth</c>, <c>GetMaxHealth</c> and so on), so anything that
    ///     raises a maximum (food, Orlog's eitr pool) shows up. Unlike the game's bars they stay put while building or
    ///     sailing; <see cref="MovedHudPanels" /> moves the game's build and ship displays out of their way instead.
    /// </summary>
    internal static class VitalBars
    {
        private const float BarHeight = 14f;
        private const float BarGap = 4f;
        private const float XpHeight = 8f;
        /// <summary>Space between the lowest bar and the XP bar (user, play-test 2026-10-30: "drop it down a little bit lower").</summary>
        private const float XpGap = 20f;
        private const float FoodSize = 34f;
        private const float FoodGap = 6f;
        private const int ValueFontSize = 12;

        private static RectTransform _root;
        private static Bar _health;
        private static Bar _eitr;
        private static Bar _stamina;
        private static Bar _xp;
        private static Text _level;
        private static Text _points;
        private static Image[] _foodIcons;
        private static Text[] _foodTimes;
        private static bool _vanillaHidden;

        private sealed class Bar
        {
            public RectTransform Root;
            public Image Trail;
            public Image Fill;
            public Text Value;
            public float Shown = 1f;
            public float TrailValue = 1f;
        }

        private static void EnsureCreated(Hud hud)
        {
            if (_root || !hud.m_rootObject)
            {
                return;
            }
            _root = UiKit.Rect("CraftingOverhaul_VitalBars", hud.m_rootObject.transform);
            _root.gameObject.AddComponent<LateHider>();
            UiKit.Place(_root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(10f, 10f));

            _health = NewBar("Health", UiKit.Health, true);
            _eitr = NewBar("Eitr", UiKit.Eitr, true);
            _stamina = NewBar("Stamina", UiKit.Stamina, true);
            _xp = NewBar("Xp", UiKit.Xp, false);
            _xp.Value.fontSize = 10;

            _level = UiKit.Text("Level", _root, 13, UiKit.Gold, TextAnchor.MiddleRight);
            _points = UiKit.Text("Points", _root, 13, UiKit.Gold, TextAnchor.MiddleLeft);

            _foodIcons = new Image[3];
            _foodTimes = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                // Round bronze slots, like the skill medallions (mock-up D, user 2026-10-30).
                var slot = UiKit.Image("Food" + i, _root, SheetArt.Medallion);
                if (!slot.sprite)
                {
                    slot.color = new Color(0.08f, 0.07f, 0.06f, 0.85f);
                }
                UiKit.Place(slot.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(FoodSize, FoodSize));
                _foodIcons[i] = UiKit.Image("Icon", slot.rectTransform, null);
                UiKit.Stretch(_foodIcons[i].rectTransform, 6f);
                _foodIcons[i].preserveAspect = true;
                _foodTimes[i] = UiKit.Text("Time", slot.rectTransform, 10, Color.white, TextAnchor.LowerCenter);
                UiKit.Place(_foodTimes[i].rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -1f), new Vector2(FoodSize, 12f));
            }
            Jotunn.Logger.LogInfo("Health, eitr and stamina bars added.");
        }

        private static Bar NewBar(string name, Color colour, bool vital)
        {
            var frame = UiKit.Image(name, _root, SheetArt.Bar, Image.Type.Sliced);
            if (!frame.sprite)
            {
                frame.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);
            }
            var bar = new Bar { Root = frame.rectTransform };
            bar.Trail = UiKit.Image("Trail", bar.Root, SheetArt.BarFill);
            bar.Trail.color = new Color(1f, 0.95f, 0.8f, 0.55f);
            bar.Fill = UiKit.Image("Fill", bar.Root, SheetArt.BarFill);
            bar.Fill.color = colour;
            foreach (var image in new[] { bar.Trail, bar.Fill })
            {
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Horizontal;
                image.fillOrigin = (int)Image.OriginHorizontal.Left;
                UiKit.Stretch(image.rectTransform, 3f);
            }
            if (!vital)
            {
                // The XP bar: its numbers inside, as the level and points hint sit at its ends.
                bar.Value = UiKit.Text("Value", bar.Root, ValueFontSize, Color.white, TextAnchor.MiddleRight);
                UiKit.Stretch(bar.Value.rectTransform);
                bar.Value.rectTransform.offsetMax = new Vector2(-6f, 0f);
                return bar;
            }
            // Bronze knot points at both ends and the numbers just past the right one (mock-up D, user 2026-10-30).
            for (int side = 0; side < 2; side++)
            {
                var cap = UiKit.Image(side == 0 ? "CapLeft" : "CapRight", bar.Root, SheetArt.BarCap);
                cap.enabled = cap.sprite;
                var rt = cap.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(side, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(10f, 20f);
                rt.anchoredPosition = new Vector2(side == 0 ? 3f : -3f, 0f);
                rt.localScale = side == 0 ? new Vector3(-1f, 1f, 1f) : Vector3.one;
            }
            bar.Value = UiKit.Text("Value", bar.Root, ValueFontSize, UiKit.Light, TextAnchor.MiddleLeft);
            UiKit.Place(bar.Value.rectTransform, new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(90f, 16f));
            return bar;
        }

        private static float Length(float maximum)
        {
            return Mathf.Clamp(maximum * ModConfig.HudBarScale.Value, ModConfig.HudBarMinLength.Value, ModConfig.HudBarMaxLength.Value);
        }

        private static void Update(Hud hud, Player player)
        {
            bool on = ModConfig.HudBars.Value && player && !GUIManager.IsHeadless();
            if (!on)
            {
                SetVanilla(hud, true);
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
            // The bars stay put while building or sailing (user, 2026-10-31); the game's build and ship displays move
            // out of their way instead (MovedHudPanels).
            _root.anchoredPosition = new Vector2(0f, ModConfig.HudBarsHeight.Value);

            float maxLength = ModConfig.HudBarMaxLength.Value;
            float left = -maxLength / 2f;
            float maxEitr = player.GetMaxEitr();
            bool eitr = maxEitr > 0f;
            bool xp = ClassesSource.Available;

            // Health on top, stamina under it, eitr at the bottom (user, 2026-10-31: gaining eitr must not push stamina
            // down). Health and stamina never move; the XP bar sits under the lowest bar.
            float rowStep = BarHeight + BarGap;
            float eitrY = 0f;
            float staminaY = rowStep;
            float healthY = 2f * rowStep;
            Layout(_health, left, healthY, player.GetHealth(), player.GetMaxHealth());
            Layout(_stamina, left, staminaY, player.GetStamina(), player.GetMaxStamina());
            SetActive(_eitr, eitr);
            if (eitr)
            {
                Layout(_eitr, left, eitrY, player.GetEitr(), maxEitr);
            }
            if (xp)
            {
                UpdateXp(left, maxLength, (eitr ? eitrY : staminaY) - XpGap - XpHeight);
            }
            SetActive(_xp, xp);
            _level.gameObject.SetActive(xp);
            _points.gameObject.SetActive(xp);

            UpdateFood(player, left, staminaY, healthY + BarHeight);
        }

        private static void SetActive(Bar bar, bool active)
        {
            if (bar.Root.gameObject.activeSelf != active)
            {
                bar.Root.gameObject.SetActive(active);
            }
        }

        private static void Layout(Bar bar, float left, float y, float value, float maximum)
        {
            UiKit.Place(bar.Root, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(left, y), new Vector2(Length(maximum), BarHeight));
            Show(bar, maximum > 0f ? value / maximum : 0f);
            UiKit.SetText(bar.Value, Mathf.CeilToInt(value) + " / " + Mathf.CeilToInt(maximum));
        }

        /// <summary>The fill follows at once; the pale trail behind it catches up after a moment, showing what was lost.</summary>
        private static void Show(Bar bar, float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            float dt = Time.deltaTime;
            bar.Shown = fraction;
            bar.TrailValue = fraction >= bar.TrailValue ? fraction : Mathf.MoveTowards(bar.TrailValue, fraction, dt * 0.5f);
            bar.Fill.fillAmount = bar.Shown;
            bar.Trail.fillAmount = bar.TrailValue;
        }

        private static int _lastCounter = -1;
        private static string _character = "";

        private static void UpdateXp(float left, float width, float y)
        {
            int counter = ClassesSource.ChangeCounter;
            if (counter != _lastCounter || Time.frameCount % 60 == 0)
            {
                _lastCounter = counter;
                _character = ClassesSource.Character();
            }
            UiKit.Place(_xp.Root, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(left, y), new Vector2(width, XpHeight));
            if (string.IsNullOrEmpty(_character))
            {
                Show(_xp, 0f);
                UiKit.SetText(_xp.Value, "");
                UiKit.SetText(_level, "");
                UiKit.SetText(_points, "");
                return;
            }
            int level = FlatJson.GetInt(_character, "level", 1);
            int maxLevel = FlatJson.GetInt(_character, "maxLevel", 99);
            long xp = FlatJson.GetLong(_character, "xp", 0);
            long start = FlatJson.GetLong(_character, "xpLevelStart", 0);
            long next = FlatJson.GetLong(_character, "xpNextLevel", start + 1);
            bool max = level >= maxLevel || next <= start;
            Show(_xp, max ? 1f : (float)(xp - start) / (next - start));
            UiKit.SetText(_xp.Value, max ? "" : (xp - start) + " / " + (next - start));

            UiKit.Place(_level.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0.5f), new Vector2(left - 8f, y + XpHeight / 2f), new Vector2(80f, 16f));
            UiKit.SetText(_level, "Level " + level);

            int stat = FlatJson.GetInt(_character, "statPoints", 0);
            int skill = FlatJson.GetInt(_character, "skillPoints", 0);
            bool hasClass = !string.IsNullOrEmpty(FlatJson.Get(_character, "class"));
            string hint = "";
            if (stat > 0 || skill > 0)
            {
                // Pulses like the game's "can eat again" food icons, so it's noticed.
                float alpha = 0.875f + Mathf.Sin(Time.time * 4f) * 0.125f;
                string key = ModConfig.SkillsPanelKey.Value.ToString();
                hint = (stat > 0 ? stat + " stat" : "") + (stat > 0 && skill > 0 ? ", " : "") + (skill > 0 ? skill + " skill" : "") +
                       (stat + skill == 1 ? " point" : " points") + (hasClass ? " (" + key + ")" : "");
                _points.color = new Color(UiKit.Gold.r, UiKit.Gold.g, UiKit.Gold.b, alpha);
            }
            UiKit.Place(_points.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 0.5f), new Vector2(left + width + 8f, y + XpHeight / 2f), new Vector2(200f, 16f));
            UiKit.SetText(_points, hint);
        }

        private static void UpdateFood(Player player, float left, float bottom, float top)
        {
            List<Player.Food> foods = player.GetFoods();
            float height = top - bottom;
            float total = 3 * FoodSize + 2 * FoodGap;
            // Three slots in a row left of the bars, centred on them.
            float y = bottom + (height - FoodSize) / 2f;
            for (int i = 0; i < 3; i++)
            {
                var slot = (RectTransform)_foodIcons[i].transform.parent;
                float x = left - FoodGap * 2f - (2 - i) * (FoodSize + FoodGap);
                slot.anchoredPosition = new Vector2(x, y);
                if (i < foods.Count)
                {
                    var food = foods[i];
                    _foodIcons[i].enabled = true;
                    _foodIcons[i].sprite = food.m_item.GetIcon();
                    // As the game: pulsing when you can eat it again.
                    _foodIcons[i].color = food.CanEatAgain() ? new Color(1f, 1f, 1f, 0.7f + Mathf.Sin(Time.time * 5f) * 0.3f) : Color.white;
                    float seconds = food.m_time / Game.m_foodRate;
                    UiKit.SetText(_foodTimes[i], seconds >= 60f ? Mathf.CeilToInt(seconds / 60f) + "m" : Mathf.FloorToInt(seconds) + "s");
                }
                else
                {
                    _foodIcons[i].enabled = false;
                    UiKit.SetText(_foodTimes[i], "");
                }
            }
        }

        /// <summary>
        ///     Hides (or shows again) the game's health panel with its food, and its stamina and eitr bars. They're
        ///     shrunk to nothing rather than switched off, because the game drives their animators every frame.
        /// </summary>
        private static void SetVanilla(Hud hud, bool visible)
        {
            if (!hud || (visible && !_vanillaHidden))
            {
                return;
            }
            _vanillaHidden = !visible;
            Vector3 scale = visible ? Vector3.one : Vector3.zero;
            foreach (var t in new Transform[] { hud.m_healthPanel, hud.m_staminaBar2Root, hud.m_eitrBarRoot, hud.m_foodBarRoot })
            {
                if (t)
                {
                    t.localScale = scale;
                }
            }
            if (hud.m_foodIcons != null)
            {
                foreach (var icon in hud.m_foodIcons)
                {
                    if (icon)
                    {
                        icon.transform.localScale = scale;
                    }
                }
            }
            if (hud.m_foodTime != null)
            {
                foreach (var text in hud.m_foodTime)
                {
                    if (text)
                    {
                        text.transform.localScale = scale;
                    }
                }
            }
        }

        /// <summary>The game animates its bars after <c>Hud.Update</c>; hiding them in LateUpdate keeps them hidden.</summary>
        private sealed class LateHider : MonoBehaviour
        {
            private void LateUpdate()
            {
                try
                {
                    SetVanilla(Hud.instance, false);
                }
                catch (Exception ex)
                {
                    Safe.Report("hide the game's health and stamina displays", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Update))]
        private static class Update_Patch
        {
            private static void Postfix(Hud __instance)
            {
                try
                {
                    Update(__instance, Player.m_localPlayer);
                }
                catch (Exception ex)
                {
                    Safe.Report("show the health, eitr and stamina bars", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                _root = null;
                _vanillaHidden = false;
                _lastCounter = -1;
            }
        }
    }
}
