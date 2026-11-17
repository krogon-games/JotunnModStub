using System.Collections.Generic;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     A patch of ground that hurts the enemies standing in it, hit by hit (Krogon, 2026-11-16): Immolation Arrow's
    ///     burning ground (fire, first hit a second after the impact) and Venom Strike's poison cloud (poison, first hit
    ///     at once). Lives on the shooter's own game only, which is where the arrow's impact is handled; it isn't
    ///     networked. The damage is a share of the shot's own damage worked out by the caller; no new art: a vanilla
    ///     impact effect at each hit, a light and a ring on the ground.
    /// </summary>
    internal sealed class GroundArea : MonoBehaviour
    {
        private static readonly Color FireColour = new Color(1f, 0.5f, 0.1f);
        private static readonly Color PoisonColour = new Color(0.35f, 0.9f, 0.2f);
        private static bool s_loggedFire;
        private static bool s_loggedPoison;
        private static readonly List<Character> s_inArea = new List<Character>();

        private Character m_owner;
        private string m_skill;
        private int m_level;
        private Vector3 m_point;
        private float m_radius;
        private float m_duration;
        private float m_damagePerHit;
        private bool m_poison;
        private bool m_firstAtOnce;
        private float m_shotDamage;
        private float m_percent;
        private int m_hits;
        private int m_hitsDone;
        private int m_enemyHits;
        private float m_start;
        private Light m_light;
        private float m_lightIntensity;

        public static void Begin(Character owner, string skillName, int level, Vector3 point, float radius, float duration,
            float damagePerHit, bool poison, bool firstHitAtOnce, float shotDamage, float percent)
        {
            if (!owner || duration <= 0f || radius <= 0f)
            {
                return;
            }
            var go = new GameObject("Orlog_GroundArea");
            go.transform.position = point;
            var area = go.AddComponent<GroundArea>();
            area.m_owner = owner;
            area.m_skill = skillName;
            area.m_level = level;
            area.m_point = point;
            area.m_radius = radius;
            area.m_duration = duration;
            area.m_damagePerHit = damagePerHit;
            area.m_poison = poison;
            area.m_firstAtOnce = firstHitAtOnce;
            area.m_shotDamage = shotDamage;
            area.m_percent = percent;
            area.m_hits = ActiveSkillRules.AreaTicks(duration, ActiveSkillRules.AreaInterval);
            area.m_start = Time.time;
            area.Dress();
        }

        private Color AreaColour
        {
            get { return m_poison ? PoisonColour : FireColour; }
        }

        /// <summary>The area's look: the vanilla effect at the start, a light for the whole duration, a ring at the radius.</summary>
        private void Dress()
        {
            Safe.Run("dress a ground area", () =>
            {
                // The cloud bursts with its own effect; the burning ground follows a blast that has just played one.
                if (m_poison)
                {
                    Effect(true);
                }
                var light = gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = AreaColour;
                light.range = m_radius * 2f;
                light.intensity = 2f;
                m_light = light;
                m_lightIntensity = light.intensity;

                var material = BowSkills.ForkLine.Material();
                if (material)
                {
                    var line = gameObject.AddComponent<LineRenderer>();
                    line.sharedMaterial = material;
                    line.useWorldSpace = true;
                    line.loop = true;
                    line.startWidth = 0.1f;
                    line.endWidth = 0.1f;
                    line.startColor = AreaColour;
                    line.endColor = AreaColour;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.receiveShadows = false;
                    const int points = 32;
                    line.positionCount = points;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = i * Mathf.PI * 2f / points;
                        line.SetPosition(i, m_point + new Vector3(Mathf.Cos(angle) * m_radius, 0.1f, Mathf.Sin(angle) * m_radius));
                    }
                }
            });
        }

        /// <summary>
        ///     A vanilla impact effect at the area: the cloud's burst at its start (the ooze bomb's), and at each later hit
        ///     the small one of a poison or fire arrow (an explosion every second would be too much). The first one of each
        ///     kind is logged, once per session.
        /// </summary>
        private void Effect(bool start)
        {
            bool found = m_poison
                ? (start ? BowSkills.ImpactEffect("BombOoze", "ArrowPoison", m_point) : BowSkills.ImpactEffect("ArrowPoison", null, m_point))
                : BowSkills.ImpactEffect("ArrowFire", null, m_point);
            if (m_poison ? s_loggedPoison : s_loggedFire)
            {
                return;
            }
            if (m_poison)
            {
                s_loggedPoison = true;
            }
            else
            {
                s_loggedFire = true;
            }
            Jotunn.Logger.LogInfo(found
                ? $"Orlog: the {(m_poison ? "poison cloud" : "burning ground")} plays the vanilla {(m_poison ? "BombOoze / ArrowPoison" : "ArrowFire")} impact effect."
                : $"Orlog: no vanilla impact effect found for the {(m_poison ? "poison cloud" : "burning ground")}; it shows only its light and ring.");
        }

        private float HitTime(int index)
        {
            return (m_firstAtOnce ? index : index + 1) * ActiveSkillRules.AreaInterval;
        }

        private void Update()
        {
            if (!m_owner || m_owner.IsDead())
            {
                End("the shooter is gone");
                return;
            }
            float elapsed = Time.time - m_start;
            while (m_hitsDone < m_hits && elapsed >= HitTime(m_hitsDone))
            {
                m_hitsDone++;
                Safe.Run("hurt enemies in a ground area", Hit);
                if (m_hitsDone > 1 || !m_firstAtOnce)
                {
                    Safe.Run("show a ground area's hit", () => Effect(false));
                }
            }
            if (m_light && m_duration > 0f)
            {
                float left = m_duration - elapsed;
                m_light.intensity = m_lightIntensity * Mathf.Clamp01(left);
            }
            if (m_hitsDone >= m_hits && elapsed >= m_duration)
            {
                End("its time");
            }
        }

        private void Hit()
        {
            BowSkills.CharactersInBlast(m_point, m_radius, null, s_inArea);
            foreach (var target in s_inArea)
            {
                if (!target || target == m_owner || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(m_owner, target))
                {
                    continue;
                }
                var hit = new HitData();
                if (m_poison)
                {
                    hit.m_damage.m_poison = m_damagePerHit;
                }
                else
                {
                    hit.m_damage.m_fire = m_damagePerHit;
                }
                hit.m_point = target.GetCenterPoint();
                hit.m_dir = Vector3.up;
                // Marked as an area hit, like the blast's (Explode).
                hit.m_radius = m_radius;
                hit.m_ranged = true;
                hit.m_skill = Skills.SkillType.Bows;
                hit.m_skillRaiseAmount = 0f;
                hit.m_hitType = HitData.HitType.PlayerHit;
                hit.SetAttacker(m_owner);
                target.Damage(hit);
                m_enemyHits++;
            }
        }

        private void End(string why)
        {
            // For the play-test (2026-11-16): the numbers behind an area.
            Jotunn.Logger.LogInfo($"Orlog: {m_skill} (level {m_level}): the shot's damage {m_shotDamage:0.#}, {m_percent:0.#}% = {m_damagePerHit:0.#} " +
                                  $"{(m_poison ? "poison" : "fire")} per hit; {m_hitsDone} hits over {m_duration:0.#} s on {m_enemyHits} enemy-hits within {m_radius:0.#} m" +
                                  $"{(why == "its time" ? "" : " (ended early: " + why + ")")}.");
            Destroy(gameObject);
        }
    }
}
