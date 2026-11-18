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
    ///     Released before full draw, Strafe is a normal shot and costs no eitr (as Wind Lance released early).
    ///     <para>
    ///         Crossbows too (Krogon, 2026-11-17, decisions 15–18): with the skill button held the game's own shot is held
    ///         back, and the stream starts as soon as the crossbow is loaded, <c>crossbow_per_s</c> bolts a second at the same
    ///         share. A crossbow has no draw, so what a drawn bow does by itself is done here: no running (the controls'
    ///         run is turned off, as <c>Humanoid.CheckRun</c> does for a drawn bow) and the held bow's stamina drain
    ///         (<see cref="HeldBowDrain" />). Bolts that take from the quiver go through <c>Attack.UseAmmo</c> and
    ///         <c>Attack.ProjectileAttackTriggered</c>, not <c>OnAttackTrigger</c>, which would unload the crossbow after the
    ///         first bolt (and push you back each time); when the stream ends the crossbow is unloaded and reloads.
    ///     </para>
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
        private static bool s_crossbow;
        /// <summary>The game's bows' draw stamina drain (the most common one), found once; -1 until then.</summary>
        private static float s_bowDrawDrain = -1f;
        /// <summary>A stream ran and stopped during this hold (out of eitr, arrows or stamina): nothing more until the release.</summary>
        private static bool s_stopped;
        /// <summary>Full draw was reached without the eitr to start: said once per hold.</summary>
        private static bool s_refused;

        /// <summary>True while arrows are streaming.</summary>
        public static bool Running => s_attack != null;

        /// <summary>True while a crossbow streams: the player can't run (a drawn bow's rule).</summary>
        public static bool NoRunning => s_attack != null && s_crossbow;

        /// <summary>A crossbow: a weapon that reloads instead of drawing.</summary>
        public static bool IsCrossbow(ItemDrop.ItemData weapon)
        {
            var attack = weapon?.m_shared.m_attack;
            return attack != null && !attack.m_bowDraw && attack.m_requiresReload;
        }

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
                bool bowReady = weapon != null && weapon.m_shared.m_attack != null && weapon.m_shared.m_attack.m_bowDraw && player.GetAttackDrawPercentage() >= 1f;
                bool crossbowReady = IsCrossbow(weapon) && player.IsWeaponLoaded() && !player.InMinorAction() && !player.InAttack();
                if (bowReady || crossbowReady)
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
            s_crossbow = IsCrossbow(weapon);
            s_attack = weapon.m_shared.m_attack.Clone();
            if (s_crossbow)
            {
                // What Attack.StartWithoutAnimation sets, without its trigger (which would unload the crossbow).
                s_attack.m_character = player;
                s_attack.m_baseAI = null;
                s_attack.m_body = player.m_body;
                s_attack.m_visEquipment = player.m_visEquipment;
                s_attack.m_weapon = weapon;
                s_attack.m_attackDrawPercentage = 1f;
                weapon.m_lastAttackTime = Time.time;
            }
            s_shot = new SkillButton.Shot { Attack = s_attack, Skill = skill, Level = level };
            s_weapon = weapon;
            s_started = Time.time;
            s_fired = 0;
            s_fromQuiver = 0;
            s_eitrUsed = cost;
            s_rate = v.Get(Id, s_crossbow ? "crossbow_per_s" : "arrows_per_s", level);
            s_ammoRate = v.Get(Id, "ammo_per_s", level);
            s_drain = v.Get(Id, "eitr_per_s", level);
            Jotunn.Logger.LogInfo($"Orlog: Strafe (level {level}) firing{(s_crossbow ? " a crossbow" : "")}: {s_rate:0.#} a second at {v.Get(Id, "weapon_damage_pct", level):0.#}% of the shot, " +
                                  $"{s_ammoRate:0.#} from the quiver a second; {cost:0.#} eitr paid, {s_drain:0.#} a second while firing.");
            FireNext(player);
        }

        private static void Tick(Player player, ItemDrop.ItemData weapon)
        {
            // The draw was lost (blocking, staggered, a minor action) or the weapon changed.
            bool held = s_crossbow ? player.IsWeaponLoaded() && !player.IsBlocking() && !player.InMinorAction() : player.IsDrawingBow();
            if (weapon == null || weapon != s_weapon || !held || player.IsStaggering())
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
            if (s_crossbow)
            {
                // The drawn bow drains stamina by itself; a crossbow's stream drains the same (decision 17).
                player.UseStamina(HeldBowDrain(player, s_weapon) * Time.fixedDeltaTime);
            }
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
                if (s_crossbow)
                {
                    // OnAttackTrigger's own steps without its reload reset and recoil: the bolt, then the shot.
                    if (!attack.UseAmmo(out attack.m_lastUsedAmmo))
                    {
                        Stop(player, "out of bolts");
                        return;
                    }
                    if (attack.m_attackUseAdrenaline > 0f)
                    {
                        player.AddAdrenaline(attack.m_attackUseAdrenaline);
                    }
                    attack.ProjectileAttackTriggered();
                }
                else if (s_fired == 0)
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
            if (s_crossbow)
            {
                // Empty after the stream, as after any crossbow shot: the game reloads it (decision 18).
                player.ResetLoadedWeapon();
            }
            else
            {
                CancelDraw(player);
            }
            s_crossbow = false;
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

        /// <summary>
        ///     The stamina a fully drawn bow costs a second (<c>Player.UpdateAttackBowDraw</c>, 1.0.14): the bow's draw drain,
        ///     less up to a third with skill (here the weapon's own skill), halved once fully drawn, then the gear's attack
        ///     stamina modifier and status effects. A crossbow has no draw drain, so the game's bows' is used (the most
        ///     common value among them, found once and logged); a bow uses its own (for Snipe's extra drain).
        /// </summary>
        internal static float HeldBowDrain(Player player, ItemDrop.ItemData weapon)
        {
            if (s_bowDrawDrain < 0f)
            {
                s_bowDrawDrain = 0f;
                var counts = new System.Collections.Generic.Dictionary<float, int>();
                foreach (var prefab in ObjectDB.instance.m_items)
                {
                    var shared = prefab ? prefab.GetComponent<ItemDrop>()?.m_itemData?.m_shared : null;
                    var a = shared?.m_attack;
                    if (a == null || !a.m_bowDraw || shared.m_skillType != Skills.SkillType.Bows || a.m_drawStaminaDrain <= 0f)
                    {
                        continue;
                    }
                    counts.TryGetValue(a.m_drawStaminaDrain, out int n);
                    counts[a.m_drawStaminaDrain] = n + 1;
                }
                int best = 0;
                foreach (var pair in counts)
                {
                    if (pair.Value > best || (pair.Value == best && pair.Key < s_bowDrawDrain))
                    {
                        best = pair.Value;
                        s_bowDrawDrain = pair.Key;
                    }
                }
                Jotunn.Logger.LogInfo($"Orlog: a crossbow's Strafe stream and Snipe charge drain stamina as a bow: the bows' draw drain {s_bowDrawDrain:0.##} ({best} bows of {counts.Count} values).");
            }
            // A bow uses its own draw drain (Snipe, 2026-11-18); a crossbow, which has none, the bows' (Strafe, Snipe).
            var own = weapon?.m_shared?.m_attack;
            float drain = own != null && own.m_bowDraw && own.m_drawStaminaDrain > 0f ? own.m_drawStaminaDrain : s_bowDrawDrain;
            if (drain <= 0f)
            {
                return 0f;
            }
            drain -= drain * 0.33f * player.GetSkillFactor(weapon.m_shared.m_skillType);
            drain *= 0.5f;
            drain += drain * player.GetEquipmentAttackStaminaModifier();
            player.m_seman.ModifyAttackStaminaUsage(drain, ref drain);
            return drain;
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
