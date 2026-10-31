using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     While our bars are on, the game's selected-piece box (the "Repair" box with the piece's name, description and
    ///     cost while a hammer, hoe or cultivator is out) and its ship display move to the bottom right of the screen, so
    ///     the bars can stay where they are (user, play-test 2026-10-31: "move that element off to the bottom right").
    ///     The game moves its own bars up for them instead.
    ///
    ///     Each panel is moved as a whole by the distance between its drawn content and the target corner, measured every
    ///     frame it shows, so its own layout (the cost slots, a long description) is kept. Turning the bars off puts both
    ///     back where the game had them.
    /// </summary>
    internal static class MovedHudPanels
    {
        private const float RightMargin = 24f;
        /// <summary>Above the row of key hints along the bottom of the screen.</summary>
        private const float BottomMargin = 84f;

        private static RectTransform _buildInfo;
        private static readonly Dictionary<RectTransform, Vector3> Original = new Dictionary<RectTransform, Vector3>();
        private static readonly List<Graphic> Graphics = new List<Graphic>();
        private static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>
        ///     The selected-piece box: the highest parent of the piece name, under the build display, that holds neither
        ///     the build menu nor the snapping icon (which sit elsewhere on screen).
        /// </summary>
        private static RectTransform FindBuildInfo(Hud hud)
        {
            if (!hud.m_buildHud || !hud.m_buildSelection)
            {
                return null;
            }
            Transform buildHud = hud.m_buildHud.transform;
            Transform found = null;
            for (Transform t = hud.m_buildSelection.transform; t && t != buildHud; t = t.parent)
            {
                bool holdsMenu = hud.m_pieceSelectionWindow && hud.m_pieceSelectionWindow.transform.IsChildOf(t);
                bool holdsSnapping = hud.m_snappingIcon && hud.m_snappingIcon.transform.IsChildOf(t);
                if (holdsMenu || holdsSnapping)
                {
                    break;
                }
                found = t;
            }
            return found as RectTransform;
        }

        private static void Update(Hud hud)
        {
            if (!hud.m_rootObject)
            {
                return;
            }
            var root = hud.m_rootObject.transform as RectTransform;
            if (!_buildInfo)
            {
                _buildInfo = FindBuildInfo(hud);
            }
            var ship = hud.m_shipHudRoot ? hud.m_shipHudRoot.transform as RectTransform : null;
            bool on = ModConfig.HudBars.Value;
            Place(root, _buildInfo, on);
            Place(root, ship, on);
        }

        private static void Place(RectTransform root, RectTransform panel, bool move)
        {
            if (!root || !panel)
            {
                return;
            }
            if (!move)
            {
                if (Original.TryGetValue(panel, out Vector3 local))
                {
                    panel.localPosition = local;
                    Original.Remove(panel);
                }
                return;
            }
            if (!panel.gameObject.activeInHierarchy || !Bounds(root, panel, out Rect drawn))
            {
                return;
            }
            if (!Original.ContainsKey(panel))
            {
                Original[panel] = panel.localPosition;
            }
            Rect screen = root.rect;
            var offset = new Vector2(screen.xMax - RightMargin - drawn.xMax, screen.yMin + BottomMargin - drawn.yMin);
            if (offset.sqrMagnitude > 0.25f)
            {
                panel.position += root.TransformVector(offset);
            }
        }

        /// <summary>What the panel draws (its visible images and texts), in the HUD root's space.</summary>
        private static bool Bounds(RectTransform root, RectTransform panel, out Rect drawn)
        {
            drawn = default;
            bool any = false;
            float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            panel.GetComponentsInChildren(false, Graphics);
            foreach (var graphic in Graphics)
            {
                if (!graphic.enabled || graphic.color.a <= 0.01f)
                {
                    continue;
                }
                graphic.rectTransform.GetWorldCorners(Corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = root.InverseTransformPoint(Corners[i]);
                    xMin = Mathf.Min(xMin, p.x);
                    yMin = Mathf.Min(yMin, p.y);
                    xMax = Mathf.Max(xMax, p.x);
                    yMax = Mathf.Max(yMax, p.y);
                }
                any = true;
            }
            Graphics.Clear();
            if (any)
            {
                drawn = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            }
            return any;
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
                    Safe.Report("move the build and ship displays", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                _buildInfo = null;
                Original.Clear();
            }
        }
    }
}
