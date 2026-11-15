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
    /// </summary>
    internal static class SkillButton
    {
        /// <summary>A skill attack: the attack, the skill and its level, the damage scale when it started.</summary>
        internal sealed class Shot
        {
            public Attack Attack;
            public SkillTable.Skill Skill;
            public int Level;
            public float Scale;
        }

        /// <summary>The skill being drawn or held on the skill button (until its attack starts).</summary>
        private static SkillTable.Skill s_pending;
        private static float s_pendingSince;
        private static bool s_wasHeld;
        private static bool s_wasBlockHeld;
        private static float s_lastRefusal;

        /// <summary>The local player's current skill attack, if its attack is still the one running.</summary>
        public static Shot Current { get; private set; }

        /// <summary>The skill the next attack (or bow draw) is for, if the skill button started it.</summary>
        public static SkillTable.Skill Pending => s_pending != null && Time.time - s_pendingSince < 2f ? s_pending : null;

        public static bool IsPending(string skillId) => Pending != null && Pending.Id == skillId;

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
            private static void Prefix(Player __instance, ref bool attack, ref bool attackHold, ref bool block, ref bool blockHold)
            {
                if (__instance != Player.m_localPlayer || !Progress.Ready || Attributes.ClassOf(__instance) == null)
                {
                    return;
                }
                try
                {
                    Apply(__instance, ref attack, ref attackHold, ref block, ref blockHold);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("read the skill button", ex);
                }
            }

            private static void Apply(Player player, ref bool attack, ref bool attackHold, ref bool block, ref bool blockHold)
            {
                if (!SkillBar.TakesInput(player))
                {
                    s_wasHeld = false;
                    return;
                }
                bool held = SkillBar.KeyHeld(OrlogConfig.SkillButton.Value);
                bool pressed = held && !s_wasHeld;
                s_wasHeld = held;

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

                // A normal attack (the game's attack button) clears a skill that never fired.
                if (attack && !held)
                {
                    s_pending = null;
                }
                if (!held)
                {
                    return;
                }
                var skill = SkillBar.SelectedSkill(player);
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
                    if (!pressed && state.Blocked == "eitr" && s_pending != null)
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
                s_pendingSince = Time.time;
                attack |= pressed;
                attackHold = true;
                block = false;
                blockHold = false;
            }
        }

        /// <summary>A skill attack starts: mark it and pay its eitr. A skill that can't be paid shoots as a normal attack.</summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
        private static class StartAttack
        {
            private static void Postfix(Humanoid __instance, bool secondaryAttack, bool __result)
            {
                if (!__result || secondaryAttack || __instance != Player.m_localPlayer)
                {
                    return;
                }
                var skill = Pending;
                s_pending = null;
                Current = null;
                if (skill == null)
                {
                    return;
                }
                Safe.Run("start a skill attack", () =>
                {
                    var player = (Player)__instance;
                    float cost = SkillBar.CostOf(player, skill);
                    if (cost > 0f && !player.HaveEitr(cost))
                    {
                        player.Message(MessageHud.MessageType.Center, "Not enough eitr");
                        return;
                    }
                    player.UseEitr(cost);
                    Current = new Shot
                    {
                        Attack = player.m_currentAttack,
                        Skill = skill,
                        Level = SkillBar.LevelOf(player, skill.Id),
                        Scale = SkillBar.DamageScaleFor(player)
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
