using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     Skills cast from their slot's hotkey (roadmap C5). Inner Sight: every enemy within the skill's radius is marked
    ///     for the skill's duration (<see cref="InnerSightName" />, a status effect on the creature, added through the game's
    ///     own message so the creature owner's game has it) and takes <c>InnerSightDamageTaken</c>% more damage there. The
    ///     caster sees the marked creatures glow (a light on their own game only). Plays the game's "gpower" animation.
    /// </summary>
    internal static class CastSkills
    {
        public const string InnerSightName = "Orlog_InnerSight";
        public static readonly int InnerSightHash = InnerSightName.GetStableHashCode();

        private static readonly List<Character> s_inRange = new List<Character>();

        public static void RegisterStatusEffects()
        {
            var mark = ScriptableObject.CreateInstance<SE_Stats>();
            mark.name = InnerSightName;
            mark.m_name = "Inner Sight";
            mark.m_ttl = 8f;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(mark, false));
        }

        public static void Cast(Player player, Rules.SkillTable.Skill skill, int level, float cost)
        {
            if (skill.Id != "InnerSight")
            {
                return;
            }
            var v = SkillBar.ValuesFor(Attributes.ClassOf(player));
            if (v == null)
            {
                return;
            }
            if (cost > 0f && !player.HaveEitr(cost))
            {
                player.Message(MessageHud.MessageType.Center, "Not enough eitr");
                return;
            }
            player.UseEitr(cost);
            player.m_zanim.SetTrigger("gpower");
            float radius = v.Get(skill.Id, "radius_yards", level) * Rules.ActiveSkillRules.MetresPerYard;
            float seconds = v.Get(skill.Id, "duration_s", level);
            s_inRange.Clear();
            Character.GetCharactersInRange(player.transform.position, radius, s_inRange);
            int marked = 0;
            foreach (var target in s_inRange)
            {
                if (!target || target == player || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(player, target))
                {
                    continue;
                }
                // The seconds travel as the skill level; Duration (BowSkills) sets the time from it.
                target.GetSEMan().AddStatusEffect(InnerSightHash, true, 0, seconds);
                Glow.Add(target, seconds);
                marked++;
            }
            player.Message(MessageHud.MessageType.TopLeft, $"Inner Sight: {marked} marked");
        }

        /// <summary>Marked creatures take more damage, on the game that owns them (where the hit is applied).</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class MoreDamage
        {
            private static void Prefix(Character __instance, HitData __1)
            {
                if (__1 == null || !__instance || __instance.IsPlayer())
                {
                    return;
                }
                Safe.Run("apply Inner Sight", () =>
                {
                    var seman = __instance.GetSEMan();
                    if (seman != null && seman.HaveStatusEffect(InnerSightHash))
                    {
                        __1.ApplyModifier(1f + OrlogConfig.InnerSightDamageTaken.Value / 100f);
                    }
                });
            }
        }

        /// <summary>A pale blue light on a marked creature, on the caster's game, for the mark's duration.</summary>
        private sealed class Glow : MonoBehaviour
        {
            private float m_until;

            public static void Add(Character target, float seconds)
            {
                var glow = target.GetComponentInChildren<Glow>();
                if (!glow)
                {
                    var go = new GameObject("Orlog_InnerSightGlow");
                    go.transform.SetParent(target.transform, false);
                    go.transform.position = target.GetCenterPoint();
                    var light = go.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(0.55f, 0.75f, 1f);
                    light.range = Mathf.Max(2f, target.GetRadius() * 4f);
                    light.intensity = 3f;
                    glow = go.AddComponent<Glow>();
                }
                glow.m_until = Mathf.Max(glow.m_until, Time.time + seconds);
            }

            private void Update()
            {
                if (Time.time > m_until)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
