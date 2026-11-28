using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The skill button (roadmap C5; user 2026-10-30): for a character with a class, the skill button (right click by
    ///     default) fires the attack skill chosen for the weapon in hand, and block moves to its own key. It works through
    ///     the game's own controls (<c>Player.SetControls</c>): holding the skill button holds "attack", so a bow draws and
    ///     shoots as usual; the attack it starts is marked as a skill attack and pays the skill's eitr
    ///     (<c>Humanoid.StartAttack</c>). With no skill chosen, the skill button does nothing (user 2026-11-11). Keyboard and
    ///     mouse only; with a gamepad nothing changes.
    ///     <para>
    ///         A charged skill (Wind Lance; user 2026-11-16) must be held past full draw for its hold time
    ///         (<c>hold_s</c>); released earlier it is a normal shot and costs nothing. A crossbow has no draw: the skill
    ///         button holds its shot back until the release, and the hold counts from the press.
    ///         Snipe (Krogon, 2026-11-18) charges the same way over <c>charge_s</c>, but fires on any charge, its damage
    ///         growing with it (<see cref="Snipe" />).
    ///     </para>
    ///     <para>
    ///         Strafe (Krogon, 2026-11-17) streams arrows from full draw while held and pays as it goes (<see cref="Strafe" />);
    ///         released before full draw it is a normal shot and costs nothing.
    ///     </para>
    ///     <para>
    ///         Gungnir's Throw and the javelin skills (Krogon, 2026-11-28, C6) hold the spear's own throw (the game's
    ///         secondary attack) instead: one throw per click, or one after another while held, as the game's throw does.
    ///         The throw becomes a ghost javelin (<see cref="Javelins" />); the spear stays in hand.
    ///     </para>
    /// </summary>
    internal static class SkillButton
    {
        /// <summary>A skill attack: the attack, the skill and its level.</summary>
        internal sealed class Shot
        {
            public Attack Attack;
            public SkillTable.Skill Skill;
            public int Level;
            /// <summary>Snipe: how full its charge was at the release, 0 to 1 (-1 for other skills).</summary>
            public float Charge = -1f;
        }

        /// <summary>The skill being drawn or held on the skill button (until its attack starts).</summary>
        private static SkillTable.Skill s_pending;
        private static float s_pendingSince;
        private static bool s_wasHeld;
        private static bool s_wasBlockHeld;
        private static float s_lastRefusal;
        /// <summary>A charged skill: the seconds held past full draw, whether that is enough, and (crossbow) a shot held back until the release.</summary>
        private static float s_charge;
        private static bool s_chargeReady;
        private static bool s_heldBack;
        /// <summary>For the draw circle: when the charge was last counted, its hold time, and whether the weapon draws like a bow.</summary>
        private static float s_chargeSeen = -10f;
        private static float s_chargeHold;
        private static bool s_chargeDraws;
        private static string s_chargeSkill;

        /// <summary>How far the charged shot being held has come (0 to 1), or -1 when none is being held.</summary>
        public static float ChargeProgress => Time.time - s_chargeSeen < 0.1f ? ActiveSkillRules.ChargeProgress(s_charge, s_chargeHold) : -1f;

        /// <summary>Whether the weapon charging draws like a bow (the game shows its draw circle) or not (a crossbow).</summary>
        public static bool ChargeDraws => s_chargeDraws;

        /// <summary>The skill whose charge is being held (while <see cref="ChargeProgress" /> is 0 or more).</summary>
        public static string ChargingSkillId => s_chargeSkill;

        /// <summary>The local player's current skill attack, if its attack is still the one running.</summary>
        public static Shot Current { get; private set; }

        /// <summary>The skill the next attack (or bow draw) is for, if the skill button started it.</summary>
        public static SkillTable.Skill Pending => s_pending != null && Time.time - s_pendingSince < 2f ? s_pending : null;

        private static bool SamePath(string a, string b)
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) && string.Equals(a.Trim(), b.Trim(), System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True when the game's Block is bound to the same key as the skill button (it is by default: right click).</summary>
        private static bool BlockIsSkillButton()
        {
            string block = ZInput.instance?.GetButtonDef("Block")?.GetActionPath();
            return SamePath(block, ZInput.KeyCodeToPath(OrlogConfig.SkillButton.Value, false));
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
        private static class Controls
        {
            private static void Prefix(Player __instance, ref bool attack, ref bool attackHold, ref bool secondaryAttack, ref bool secondaryAttackHold,
                ref bool block, ref bool blockHold, ref bool run)
            {
                if (__instance != Player.m_localPlayer || !Progress.Ready || Attributes.ClassOf(__instance) == null)
                {
                    return;
                }
                try
                {
                    Apply(__instance, ref attack, ref attackHold, ref secondaryAttack, ref secondaryAttackHold, ref block, ref blockHold);
                    // A crossbow's Strafe stream: no running, as with a drawn bow (Krogon, 2026-11-17).
                    if (Strafe.NoRunning)
                    {
                        run = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("read the skill button", ex);
                }
            }

            private static void Apply(Player player, ref bool attack, ref bool attackHold, ref bool secondaryAttack, ref bool secondaryAttackHold,
                ref bool block, ref bool blockHold)
            {
                if (!SkillBar.TakesInput(player))
                {
                    if (Strafe.HoldsRelease)
                    {
                        Strafe.Release(player);
                    }
                    s_wasHeld = false;
                    s_heldBack = false;
                    return;
                }
                bool held = SkillBar.KeyHeld(OrlogConfig.SkillButton.Value);
                bool pressed = held && !s_wasHeld;
                bool released = !held && s_wasHeld;
                s_wasHeld = held;
                bool firesHeldBack = false;
                if (released)
                {
                    // Strafe: after a stream, letting go cancels the draw (the game shoots nothing more).
                    Strafe.Release(player);
                    // A crossbow's charged shot was held back: the release fires it (charged or not: StartAttack decides).
                    if (s_heldBack && s_pending != null)
                    {
                        s_pendingSince = Time.time;
                        attack = true;
                        firesHeldBack = true;
                    }
                    s_heldBack = false;
                }

                // Block: the game's key, unless it is the skill button; then our block key.
                bool ourBlock = SkillBar.KeyHeld(OrlogConfig.BlockKey.Value);
                bool ourBlockPressed = ourBlock && !s_wasBlockHeld;
                s_wasBlockHeld = ourBlock;
                if (BlockIsSkillButton())
                {
                    block = ourBlockPressed;
                    blockHold = ourBlock;
                }
                else
                {
                    block |= ourBlockPressed;
                    blockHold |= ourBlock;
                }

                // A normal attack (the game's attack button) clears a skill that never fired; the shot we release
                // ourselves (a crossbow's held-back charged shot) is the skill's own.
                if ((attack || secondaryAttack) && !held && !firesHeldBack)
                {
                    s_pending = null;
                }
                if (!held)
                {
                    return;
                }
                var skill = SkillBar.SelectedSkill(player);
                if (Strafe.HoldsRelease)
                {
                    if (skill == null || skill.Id != Strafe.Id)
                    {
                        Strafe.Release(player);
                    }
                    else
                    {
                        // A stream runs (or stopped during this hold): keep holding; it pays its own eitr as it goes.
                        s_pendingSince = Time.time;
                        block = false;
                        blockHold = false;
                        // A bow stays drawn; a crossbow's own shot stays held back.
                        bool crossbow = Strafe.IsCrossbow(player.GetCurrentWeapon());
                        attack &= !crossbow;
                        attackHold = !crossbow;
                        Strafe.Hold(player, skill);
                        return;
                    }
                }
                if (skill == null)
                {
                    if (pressed)
                    {
                        SkillBar.LogIgnored("Skill button", "no skill on it for the weapon in hand (" + (SkillBar.WeaponKind(player).Length > 0 ? SkillBar.WeaponKind(player) : "none") + ")");
                    }
                    return;
                }
                int slot = SkillBar.SelectedSlot(player);
                var state = SkillBar.StateOf(player, slot);
                if (!state.Usable)
                {
                    // Eitr ran short during a skill draw: keep drawing; the release shoots a normal arrow ("Not enough eitr").
                    if (!pressed && state.Blocked == "eitr" && s_pending != null && !ActiveSkillRules.Throws(skill.Id))
                    {
                        s_pendingSince = Time.time;
                        attackHold = true;
                        block = false;
                        blockHold = false;
                        return;
                    }
                    if (pressed && Time.time - s_lastRefusal > 0.5f)
                    {
                        s_lastRefusal = Time.time;
                        player.Message(MessageHud.MessageType.Center, ActiveSkillRules.BlockedMessage(state));
                    }
                    return;
                }
                // A cast skill on the skill button (quick cast off) casts on the press; no attack.
                if (skill.Kind == "cast")
                {
                    if (pressed)
                    {
                        CastSkills.Cast(player, skill, SkillBar.LevelOf(player, skill.Id), state.EitrCost);
                    }
                    block = false;
                    blockHold = false;
                    return;
                }
                if (pressed || s_pending == null)
                {
                    s_pending = skill;
                }
                if (pressed)
                {
                    s_charge = 0f;
                    s_chargeReady = false;
                }
                s_pendingSince = Time.time;
                block = false;
                blockHold = false;
                if (ActiveSkillRules.Throws(skill.Id))
                {
                    // The spear's own throw, made a ghost javelin when it starts (StartAttack). Held: throw after throw.
                    attack = false;
                    attackHold = false;
                    secondaryAttack |= pressed;
                    secondaryAttackHold = true;
                    return;
                }
                if (ActiveSkillRules.Charges(skill.Id))
                {
                    Charge(player, skill, pressed, ref attack, ref attackHold);
                    return;
                }
                if (skill.Id == Strafe.Id && Strafe.IsCrossbow(player.GetCurrentWeapon()))
                {
                    // A crossbow: hold its own shot back; the stream starts once it is loaded (Krogon, 2026-11-17).
                    attack = false;
                    attackHold = false;
                    Strafe.Hold(player, skill);
                    return;
                }
                attack |= pressed;
                attackHold = true;
                if (skill.Id == Strafe.Id)
                {
                    // At full draw the stream starts (Krogon, 2026-11-17).
                    Strafe.Hold(player, skill);
                }
            }

            /// <summary>The skill button is held on a charged skill: count the hold past full draw and mark it ready.</summary>
            private static void Charge(Player player, SkillTable.Skill skill, bool pressed, ref bool attack, ref bool attackHold)
            {
                var weapon = player.GetCurrentWeapon();
                bool draws = weapon != null && weapon.m_shared.m_attack != null && weapon.m_shared.m_attack.m_bowDraw;
                var values = SkillBar.ValuesFor(Attributes.ClassOf(player));
                float hold = values?.Get(skill.Id, ActiveSkillRules.ChargeTimeValue(skill.Id), SkillBar.LevelOf(player, skill.Id)) ?? 0f;
                bool snipe = ActiveSkillRules.IsSnipe(skill.Id);
                if (draws)
                {
                    // A bow: the game draws it as usual; the hold counts once it is fully drawn, and starts again if the
                    // draw is lost (out of stamina).
                    attack |= pressed;
                    attackHold = true;
                    bool fullDraw = player.GetAttackDrawPercentage() >= 1f;
                    // Snipe: the bow's drawing rate of stamina while it charges (Krogon, 2026-11-18).
                    if (fullDraw && snipe)
                    {
                        Snipe.ChargeStep(player, weapon, true, s_charge >= hold, Time.deltaTime);
                    }
                    s_charge = fullDraw ? s_charge + Time.deltaTime : 0f;
                }
                else
                {
                    // A crossbow: no draw. Hold the shot back until the release; the hold counts while it is loaded.
                    s_heldBack = true;
                    attack = false;
                    attackHold = false;
                    bool loaded = weapon != null && (weapon.m_shared.m_attack == null || !weapon.m_shared.m_attack.m_requiresReload || player.IsWeaponLoaded());
                    // Snipe on a crossbow pays the bows' drawing rate; out of stamina, its charge stops growing.
                    bool stamina = !loaded || !snipe || Snipe.ChargeStep(player, weapon, false, s_charge >= hold, Time.deltaTime);
                    s_charge = loaded ? (stamina ? s_charge + Time.deltaTime : s_charge) : 0f;
                }
                s_chargeSeen = Time.time;
                s_chargeHold = hold;
                s_chargeDraws = draws;
                s_chargeSkill = skill.Id;
                bool ready = ActiveSkillRules.ChargeReady(s_charge, hold);
                if (ready && snipe)
                {
                    // Snipe's charge is full: a ring sounds once (the circle pulses, Hud.UpdateCrosshair).
                    if (!s_chargeReady)
                    {
                        Snipe.FullCue(player);
                    }
                }
                else if (ready)
                {
                    // The wind at the bow lasts while this keeps being called; the sound plays the moment the charge is ready.
                    WindLance.ReadyCue(player, !s_chargeReady);
                }
                s_chargeReady = ready;
            }
        }

        /// <summary>A skill attack starts: mark it and pay its eitr. A skill that can't be paid shoots as a normal attack.</summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
        private static class StartAttack
        {
            private static void Postfix(Humanoid __instance, bool secondaryAttack, bool __result)
            {
                if (!__result || __instance != Player.m_localPlayer)
                {
                    return;
                }
                var skill = Pending;
                // A throw skill rides the spear's throw (the secondary attack), every other skill the main attack. The
                // game's own throw (middle mouse) and a plain swing stay the game's.
                bool throws = skill != null && ActiveSkillRules.Throws(skill.Id);
                if (secondaryAttack != throws)
                {
                    if (secondaryAttack)
                    {
                        return;
                    }
                    skill = null;
                }
                s_pending = null;
                Current = null;
                if (skill == null)
                {
                    return;
                }
                Safe.Run("start a skill attack", () =>
                {
                    var player = (Player)__instance;
                    // A charged skill released before it was ready: a normal shot, no eitr (user 2026-11-16). Snipe fires
                    // on any charge, with the charge it reached (Krogon, 2026-11-18).
                    float charge = -1f;
                    if (ActiveSkillRules.Charges(skill.Id))
                    {
                        bool fires = ActiveSkillRules.ChargeFires(skill.Id, s_charge, s_chargeHold);
                        if (ActiveSkillRules.IsSnipe(skill.Id))
                        {
                            charge = ActiveSkillRules.ChargeProgress(s_charge, s_chargeHold);
                        }
                        s_chargeReady = false;
                        s_charge = 0f;
                        if (!fires)
                        {
                            Jotunn.Logger.LogInfo("Orlog: " + skill.Name + (ActiveSkillRules.IsSnipe(skill.Id)
                                ? " released at full draw with no charge: a normal shot."
                                : " released before its charge was ready: a normal shot."));
                            return;
                        }
                    }
                    // Strafe let go before full draw (a stream never starts the game's attack): a normal shot, no eitr.
                    if (skill.Id == Strafe.Id)
                    {
                        Jotunn.Logger.LogInfo("Orlog: Strafe released before full draw: a normal shot.");
                        return;
                    }
                    float cost = SkillBar.CostOf(player, skill);
                    if (cost > 0f && !player.HaveEitr(cost))
                    {
                        player.Message(MessageHud.MessageType.Center, "Not enough eitr");
                        if (!throws)
                        {
                            return;
                        }
                        // The throw has started: it still never drops the spear. It flies as Gungnir's Throw, which costs no eitr.
                        skill = SkillPoints.TableOf(player)?.Find(ActiveSkillRules.GungnirsThrowId) ?? skill;
                        cost = 0f;
                    }
                    player.UseEitr(cost);
                    if (throws)
                    {
                        Javelins.Prepare(player.m_currentAttack, skill.Id, SkillBar.LevelOf(player, skill.Id));
                    }
                    Current = new Shot
                    {
                        Attack = player.m_currentAttack,
                        Skill = skill,
                        Level = SkillBar.LevelOf(player, skill.Id),
                        Charge = charge
                    };
                });
            }
        }

        /// <summary>The skill attack an attack belongs to, or null.</summary>
        public static Shot For(Attack attack)
        {
            return attack != null && Current != null && Current.Attack == attack ? Current : null;
        }
    }
}
