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
    ///     Plague Javelin's cloud (Krogon, 2026-11-28) spreads: its reach grows evenly from nothing to the full radius over
    ///     its time (<see cref="ActiveSkillRules.SpreadRadius" />), and its ring grows with it.
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
        private bool m_spreads;
        private Skills.SkillType m_skillType = Skills.SkillType.Bows;
        private LineRenderer m_ring;
        private float m_ringRadius = -1f;
        private ParticleSystem m_cloud;

        public static void Begin(Character owner, string skillName, int level, Vector3 point, float radius, float duration,
            float damagePerHit, bool poison, bool firstHitAtOnce, float shotDamage, float percent, bool spreads = false,
            Skills.SkillType skillType = Skills.SkillType.Bows)
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
            area.m_spreads = spreads;
            area.m_skillType = skillType;
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
                // A spreading cloud (Plague Javelin) glows less: its ring stood out too much (Krogon, 2026-11-28).
                light.intensity = m_spreads ? 1.2f : 2f;
                m_light = light;
                m_lightIntensity = light.intensity;

                var material = BowSkills.ForkLine.Material();
                if (material)
                {
                    var line = gameObject.AddComponent<LineRenderer>();
                    line.sharedMaterial = material;
                    line.useWorldSpace = true;
                    line.loop = true;
                    // A spreading cloud's ring is thinner and half see-through, to blend in (Krogon, 2026-11-28: "keep the
                    // ring for now, maybe give it a slightly less intrusive and blended feel"); the cloud itself shows the spread.
                    float width = m_spreads ? 0.05f : 0.1f;
                    var colour = m_spreads ? new Color(AreaColour.r, AreaColour.g, AreaColour.b, 0.35f) : AreaColour;
                    line.startWidth = width;
                    line.endWidth = width;
                    line.startColor = colour;
                    line.endColor = colour;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.receiveShadows = false;
                    m_ring = line;
                    SetRing(CurrentRadius());
                }
                if (m_spreads)
                {
                    m_cloud = Cloud();
                }
            });
        }

        /// <summary>
        ///     A spreading cloud's body (Krogon, 2026-11-28: "it would be better if the poison cloud grew in size"): soft
        ///     green puffs rolling low over the ground, born anywhere inside the cloud's reach, which grows with it.
        /// </summary>
        private ParticleSystem Cloud()
        {
            var material = GhostSpearLook.SoftMaterial();
            if (!material)
            {
                return null;
            }
            var go = new GameObject("Orlog_PoisonCloud");
            go.transform.position = m_point + Vector3.up * 0.4f;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var cloud = go.AddComponent<ParticleSystem>();
            cloud.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = cloud.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(PoisonColour.r * 0.8f, PoisonColour.g * 0.8f, PoisonColour.b * 0.6f, 0.22f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            var emission = cloud.emission;
            emission.rateOverTime = 20f;
            var shape = cloud.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            var fade = cloud.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            cloud.Play();
            return cloud;
        }

        /// <summary>The cloud's puffs fill its reach as it spreads: more puffs the wider it is.</summary>
        private void GrowCloud(float radius)
        {
            if (!m_cloud)
            {
                return;
            }
            var shape = m_cloud.shape;
            shape.radius = Mathf.Max(0.3f, radius);
            var emission = m_cloud.emission;
            emission.rateOverTime = 20f + radius * radius * 6f;
        }

        /// <summary>The area's reach now: the full radius, or for a spreading cloud how far it has spread.</summary>
        private float CurrentRadius()
        {
            return m_spreads ? ActiveSkillRules.SpreadRadius(m_radius, Time.time - m_start, m_duration) : m_radius;
        }

        private void SetRing(float radius)
        {
            if (!m_ring || Mathf.Abs(radius - m_ringRadius) < 0.02f)
            {
                return;
            }
            m_ringRadius = radius;
            const int points = 32;
            m_ring.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float angle = i * Mathf.PI * 2f / points;
                m_ring.SetPosition(i, m_point + new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius));
            }
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
            if (m_spreads)
            {
                float now = CurrentRadius();
                SetRing(now);
                GrowCloud(now);
            }
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
            float radius = CurrentRadius();
            if (radius <= 0f)
            {
                return;
            }
            BowSkills.CharactersInBlast(m_point, radius, null, s_inArea);
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
                hit.m_radius = radius;
                hit.m_ranged = true;
                hit.m_skill = m_skillType;
                hit.m_skillRaiseAmount = 0f;
                // Damage over time (Krogon, 2026-11-18, decision 19): typed as the game's own burning and poison, so
                // Corruption's on-hit affixes (life steal, Skadi, Rallying, the procs) skip it. Penetrate still applies.
                hit.m_hitType = m_poison ? HitData.HitType.Poisoned : HitData.HitType.Burning;
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
            if (m_cloud)
            {
                // The puffs already out drift and fade instead of vanishing with the area.
                m_cloud.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(m_cloud.gameObject, 2.2f);
            }
            Destroy(gameObject);
        }
    }
}
