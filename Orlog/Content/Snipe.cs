using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     Snipe (Krogon, 2026-11-18; <c>docs/handoffs/claude-amazon-snipe-shackle-shot-m67buh.md</c>, decisions 2-10 and
    ///     18): a charged shot on the skill button (<see cref="SkillButton" /> counts the charge, as for Wind Lance). The
    ///     charge counts from full draw and fills over <c>charge_s</c> (3.5 s at skill level 1, 1.5 s at 20); released, the
    ///     arrow deals 100% of the shot at no charge rising on a square curve to the full share (150% → 300%) and flies up
    ///     to twice as fast (<see cref="BowSkills" />). Any charge pays the eitr; let go at full draw with none, it is a
    ///     normal arrow. While charging:
    ///     <list type="bullet">
    ///         <item>the view zooms in evenly, to 3× at full charge, with the game's own smooth camera zoom
    ///         (<c>GameCamera.SetTempFOV</c>, as the grappling code uses), and springs back when the charge ends;</item>
    ///         <item>stamina drains at the bow's drawing rate: the game halves a bow's drain once it is fully drawn, and
    ///         Snipe pays the other half on top until the charge is full. A crossbow has no draw drain, so it pays the
    ///         game's bows' drawing rate in full (as Strafe's crossbow stream uses the bows' drain).</item>
    ///     </list>
    ///     A crossbow charges as for Wind Lance: the bolt is held back while the button is held on a loaded crossbow.
    /// </summary>
    internal static class Snipe
    {
        public const string Id = "Snipe";

        /// <summary>How quickly the view follows the zoom each frame, and how quickly it springs back (the game's FOV inertia).</summary>
        private const float ZoomInertia = 0.2f;
        private const float SpringInertia = 0.25f;

        private static bool s_zooming;
        private static float s_baseFov;

        /// <summary>
        ///     One physics step of a Snipe charge (from the skill button): the extra stamina. False when there's no stamina
        ///     left to charge on (a crossbow's charge then stops growing; a bow is let go by the game itself).
        /// </summary>
        public static bool ChargeStep(Player player, ItemDrop.ItemData weapon, bool bowDraws, bool full, float dt)
        {
            if (!player || weapon == null || full)
            {
                return true;
            }
            float drain = Strafe.HeldBowDrain(player, weapon);
            // A bow: the game already pays the half; a crossbow: pay the drawing rate in full.
            float extra = bowDraws ? drain : drain * 2f;
            if (extra <= 0f)
            {
                return true;
            }
            if (!player.HaveStamina())
            {
                return false;
            }
            player.UseStamina(extra * dt);
            return true;
        }

        /// <summary>The charge is full: the game's perfect-block ring sounds once (no wind, which is Wind Lance's).</summary>
        public static void FullCue(Player player)
        {
            if (!player)
            {
                return;
            }
            Safe.Run("sound Snipe's full charge", () =>
            {
                Transform hand = player.m_visEquipment ? player.m_visEquipment.m_leftHand : null;
                player.m_perfectBlockEffect.Create(hand ? hand.position : player.GetCenterPoint(), Quaternion.identity);
            });
        }

        /// <summary>Every frame (from the plugin): the zoom follows the Snipe charge being held, and springs back after it.</summary>
        public static void Tick()
        {
            var camera = GameCamera.instance;
            var player = Player.m_localPlayer;
            if (!camera)
            {
                s_zooming = false;
                return;
            }
            float progress = SkillButton.ChargeProgress;
            bool charging = player && progress >= 0f && ActiveSkillRules.IsSnipe(SkillButton.ChargingSkillId);
            if (charging)
            {
                if (!s_zooming)
                {
                    // The unzoomed view: the game keeps it in m_fovBase once any temporary zoom has run (65°, the
                    // game's fixed view; nothing in 1.0.14 sets it from the settings), else the view now. Not the
                    // view now when it is still springing back from a zoom.
                    s_baseFov = camera.m_fovBase > 0f ? camera.m_fovBase : camera.m_fov;
                    s_zooming = true;
                }
                var values = SkillBar.ValuesFor(Attributes.ClassOf(player));
                float zoom = values?.Get(Id, "zoom_x", SkillBar.LevelOf(player, Id)) ?? 1f;
                camera.SetTempFOV(ActiveSkillRules.ZoomFov(s_baseFov, zoom, progress), ZoomInertia);
            }
            else if (s_zooming)
            {
                s_zooming = false;
                camera.SetTempFOV(s_baseFov, SpringInertia);
            }
        }
    }
}
