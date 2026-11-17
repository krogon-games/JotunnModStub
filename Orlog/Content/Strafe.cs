using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     Strafe, remade after Diablo 3's Rapid Fire (Krogon, 2026-11-17; <c>docs/handoffs/claude-elegant-maxwell-e7ys9o.md</c>,
    ///     decisions 1–14). Hold the skill button: the bow draws as usual; once it is fully drawn and the button is still
    ///     held, arrows stream out where you aim, <c>arrows_per_s</c> a second, until you let go or run out of eitr, arrows
    ///     or stamina.
    ///     <para>
    ///         How it works (read in the game's code, 1.0.14: <c>Player.UpdateAttackBowDraw</c>, <c>Attack</c>): while the
    ///         attack button is held the game keeps the bow drawn, so its drawing pose, the drawn bow's slowed walk and its
    ///         stamina drain (half the draw rate once fully drawn) carry on by themselves (decisions 10 and 11); the game
    ///         only shoots when the button is let go. The stream fires through its own copy of the bow's attack:
    ///         <c>Attack.StartWithoutAnimation</c> for the first arrow and <c>Attack.OnAttackTrigger</c> for each one that
    ///         takes an arrow from the quiver (the game's own ammo, bow wear and release sound; one a second, decision 8),
    ///         and <c>Attack.FireProjectileBurst</c> for the others (the game's own arrow: full draw speed, accuracy and
    ///         damage, its normal flight, decision 13). Each arrow's damage is cut to the skill's share in
    ///         <see cref="BowSkills" /> (<c>Projectile.Setup</c>). When the stream has run, letting go cancels the draw so
    ///         the game shoots nothing more, and the game's own shot is held back while a stream runs
    ///         (<see cref="HoldRelease" />).
    ///     </para>
    ///     Released before full draw, Strafe is a normal shot and costs no eitr (as Wind Lance released early). Bows only:
    ///     a crossbow, which has no draw, shoots a normal bolt.
    /// </summary>
    internal static class Strafe
    {
        public const string Id = "Strafe";

        /// <summary>At most this many arrows in one physics step, so a hitch doesn't empty a burst at once.</summary>
        private const int MaxCatchUp = 2;

        private static Attack s_attack;
        private static SkillButton.Shot s_shot;
        private static ItemDrop.ItemData s_weapon;
        private static float s_started;
        private static int s_fired;
        private static int s_fromQuiver;
        private static float s_eitrUsed;
        private static float s_rate;
        private static float s_ammoRate;
        private static float s_drain;
        /// <summary>A stream ran and stopped during this hold (out of eitr, arrows or stamina): nothing more until the release.</summary>
        private static bool s_stopped;
        /// <summary>Full draw was reached without the eitr to start: said once per hold.</summary>
        private static bool s_refused;

        /// <summary>True while arrows are streaming.</summary>
        public static bool Running => s_attack != null;

        /// <summary>True while a stream runs or has stopped during this hold: the game's own release shot is held back.</summary>
        public static bool HoldsRelease => s_attack != null || s_stopped;

        /// <summary>The skill shot an attack belongs to, if it is the stream's own.</summary>
        public static SkillButton.Shot ShotFor(Attack attack)
        {
            return attack != null && attack == s_attack ? s_shot : null;
        }

        /// <summary>
        ///     Each physics step while the skill button is held with Strafe chosen (from <see cref="SkillButton" />'s controls,
        ///     before the game reads them): start the stream at full draw, then keep it firing and paying.
        /// </summary>
        public static void Hold(Player player, SkillTable.Skill skill)
        {
            if (s_stopped)
            {
                return;
            }
            var weapon = player.GetCurrentWeapon();
            if (s_attack == null)
            {
                if (weapon != null && weapon.m_shared.m_attack != null && weapon.m_shared.m_attack.m_bowDraw && player.GetAttackDrawPercentage() >= 1f)
                {
                    Begin(player, skill, weapon);
                }
                return;
            }
            Tick(player, weapon);
        }

        /// <summary>
        ///     The skill button was let go (or input was taken away, a menu opened, another skill chosen): after a stream,
        ///     the draw is cancelled so the game shoots nothing more. Before full draw nothing changes: the game shoots a
        ///     normal arrow.
        /// </summary>
        public static void Release(Player player)
        {
            if (s_attack != null)
            {
                End(player, "released");
            }
            s_stopped = false;
            s_refused = false;
        }

        private static void Begin(Player player, SkillTable.Skill skill, ItemDrop.ItemData weapon)
        {
            var v = SkillBar.ValuesFor(Attributes.ClassOf(player));
            if (v == null)
            {
                return;
            }
            int level = SkillBar.LevelOf(player, skill.Id);
            float cost = SkillBar.CostOf(player, skill);
            if (cost > 0f && !player.HaveEitr(cost))
            {
                // Not enough to start: keep the draw; the release shoots a normal arrow.
                if (!s_refused)
                {
                    s_refused = true;
                    player.Message(MessageHud.MessageType.Center, "Not enough eitr");
                }
                return;
            }
            // No arrows: no stream and no eitr (the game says "out of arrows").
            if (!Attack.HaveAmmo(player, weapon))
            {
                s_stopped = true;
                CancelDraw(player);
                return;
            }
            player.UseEitr(cost);
            s_attack = weapon.m_shared.m_attack.Clone();
            s_shot = new SkillButton.Shot { Attack = s_attack, Skill = skill, Level = level };
            s_weapon = weapon;
            s_started = Time.time;
            s_fired = 0;
            s_fromQuiver = 0;
            s_eitrUsed = cost;
            s_rate = v.Get(Id, "arrows_per_s", level);
            s_ammoRate = v.Get(Id, "ammo_per_s", level);
            s_drain = v.Get(Id, "eitr_per_s", level);
            Jotunn.Logger.LogInfo($"Orlog: Strafe (level {level}) firing: {s_rate:0.#} arrows a second at {v.Get(Id, "weapon_damage_pct", level):0.#}% of the shot, " +
                                  $"{s_ammoRate:0.#} from the quiver a second; {cost:0.#} eitr paid, {s_drain:0.#} a second while firing.");
            FireNext(player);
        }

        private static void Tick(Player player, ItemDrop.ItemData weapon)
        {
            // The draw was lost (blocking, staggered, a minor action) or the weapon changed.
            if (weapon == null || weapon != s_weapon || !player.IsDrawingBow() || player.IsStaggering())
            {
                Stop(player, "the draw was lost");
                return;
            }
            float drain = s_drain * Time.fixedDeltaTime;
            if (drain > 0f && !player.HaveEitr(drain))
            {
                player.Message(MessageHud.MessageType.Center, "Not enough eitr");
                Stop(player, "out of eitr");
                return;
            }
            // Out of stamina the game lets a drawn bow go: end the stream first, so no extra shot leaves.
            if (!player.HaveStamina())
            {
                Stop(player, "out of stamina");
                return;
            }
            player.UseEitr(drain);
            s_eitrUsed += drain;
            int due = ActiveSkillRules.StreamDue(Time.time - s_started, s_rate);
            for (int n = 0; n < MaxCatchUp && s_attack != null && s_fired < due; n++)
            {
                FireNext(player);
            }
        }

        /// <summary>The next arrow of the stream, through the game's own attack code.</summary>
        private static void FireNext(Player player)
        {
            var attack = s_attack;
            if (ActiveSkillRules.StreamTakesAmmo(s_fired, s_rate, s_ammoRate))
            {
                // The game says "out of arrows" itself.
                if (!Attack.HaveAmmo(player, s_weapon))
                {
                    Stop(player, "out of arrows");
                    return;
                }
                if (s_fired == 0)
                {
                    attack.StartWithoutAnimation(player, player.m_body, player.m_visEquipment, s_weapon, 1f);
                }
                else
                {
                    attack.OnAttackTrigger();
                }
                s_fromQuiver++;
            }
            else
            {
                attack.GetProjectileSpawnPoint(out var spawnPoint, out var aimDir);
                s_weapon.m_shared.m_triggerEffect.Create(spawnPoint, Quaternion.LookRotation(aimDir), null, 1f, -1, player.GetZDOID());
                attack.FireProjectileBurst();
            }
            s_fired++;
        }

        /// <summary>The stream stopped by itself while the button is still held: cancel the draw and wait for the release.</summary>
        private static void Stop(Player player, string why)
        {
            End(player, why);
            s_stopped = true;
        }

        private static void End(Player player, string why)
        {
            float seconds = Time.time - s_started;
            Jotunn.Logger.LogInfo($"Orlog: Strafe stopped ({why}): {s_fired} arrows in {seconds:0.0} s, {s_fromQuiver} from the quiver, {s_eitrUsed:0.#} eitr.");
            CancelDraw(player);
            s_attack = null;
            s_shot = null;
            s_weapon = null;
        }

        /// <summary>
        ///     Ends the bow's draw without a shot: the game's draw time at -1 means "wait for the button to be let go"
        ///     (<c>Player.UpdateAttackBowDraw</c>), and the drawing pose is put down.
        /// </summary>
        private static void CancelDraw(Player player)
        {
            if (!player)
            {
                return;
            }
            player.m_attackDrawTime = -1f;
            var weapon = player.GetCurrentWeapon();
            string state = weapon?.m_shared.m_attack?.m_drawAnimationState;
            if (!string.IsNullOrEmpty(state) && player.m_zanim)
            {
                player.m_zanim.SetBool(state, false);
            }
        }

        /// <summary>While a stream runs or has stopped during this hold, the game's own bow release starts no attack.</summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
        private static class HoldRelease
        {
            private static bool Prefix(Humanoid __instance, ref bool __result)
            {
                if (__instance != Player.m_localPlayer || !HoldsRelease)
                {
                    return true;
                }
                __result = false;
                return false;
            }
        }
    }
}
