using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The ghost javelin's look (Krogon, 2026-11-28, decisions 3, 8, 12, 15 and 16; <c>docs/images/javelins_preview.png</c>),
    ///     on the ghost projectile prefabs (<see cref="Javelins" />), so it shows on every player's game: the spear's own
    ///     model, see-through warm gold, with a soft gold light and a faint trail. Lightning Bolt's crackles with
    ///     blue-white lightning along it; Plague Javelin's drips green and trails green. Once it stops (stuck in the
    ///     ground or an enemy) it fades out over <see cref="ActiveSkillRules.GhostFadeSeconds" />. No new art: engine parts
    ///     only, like Wind Lance's look.
    /// </summary>
    internal sealed class GhostSpearLook : MonoBehaviour
    {
        private static readonly Color Gold = new Color(1f, 0.78f, 0.35f);
        private static readonly Color BoltColour = new Color(0.75f, 0.88f, 1f);
        private static readonly Color PlagueColour = new Color(0.47f, 0.82f, 0.35f);
        private const float GhostAlpha = 0.55f;
        private const int BoltStrands = 3;
        private const int BoltPoints = 9;
        private static Material s_ghostMaterial;
        private static bool s_searched;

        /// <summary><see cref="Javelins.Variant" /> as a number (serialised on the prefab).</summary>
        public int Kind;

        private Projectile m_projectile;
        private Renderer[] m_renderers;
        private Material m_fadeMaterial;
        private Light m_light;
        private float m_lightIntensity;
        private TrailRenderer m_trail;
        private LineRenderer[] m_bolts;
        private ParticleSystem m_drips;
        private float m_length = 2f;
        private float m_nextJag;
        private Vector3 m_lastPosition;
        private float m_stillSince = -1f;
        private float m_stuckAt = -1f;
        private float m_born;

        private Javelins.Variant Variant => (Javelins.Variant)Kind;

        private void Awake()
        {
            m_born = Time.time;
            m_projectile = GetComponent<Projectile>();
            m_lastPosition = transform.position;
            Safe.Run("dress a ghost javelin", Dress);
        }

        private void Dress()
        {
            var material = GhostMaterial();
            var bounds = new Bounds(transform.position, Vector3.zero);
            bool any = false;
            var renderers = GetComponentsInChildren<Renderer>(true);
            var meshes = new System.Collections.Generic.List<Renderer>();
            foreach (var r in renderers)
            {
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer))
                {
                    continue;
                }
                meshes.Add(r);
                if (material)
                {
                    var shared = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < shared.Length; i++)
                    {
                        shared[i] = material;
                    }
                    r.sharedMaterials = shared;
                }
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                bounds = any ? Encapsulate(bounds, r.bounds) : r.bounds;
                any = true;
            }
            m_renderers = meshes.ToArray();
            if (any)
            {
                m_length = Mathf.Clamp(bounds.size.magnitude, 0.8f, 3f);
            }

            Color tint = Variant == Javelins.Variant.Bolt ? Color.Lerp(Gold, BoltColour, 0.5f) : Gold;
            m_light = gameObject.AddComponent<Light>();
            m_light.type = LightType.Point;
            m_light.color = tint;
            m_light.range = Variant == Javelins.Variant.Bolt ? 4f : 3f;
            m_light.intensity = Variant == Javelins.Variant.Bolt ? 2f : 1.4f;
            m_lightIntensity = m_light.intensity;

            var lineMaterial = BowSkills.ForkLine.Material();
            if (!lineMaterial)
            {
                return;
            }
            m_trail = gameObject.AddComponent<TrailRenderer>();
            m_trail.sharedMaterial = lineMaterial;
            m_trail.time = 0.3f;
            m_trail.startWidth = 0.12f;
            m_trail.endWidth = 0f;
            m_trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_trail.receiveShadows = false;
            Color trail = Variant == Javelins.Variant.Bolt ? BoltColour : Variant == Javelins.Variant.Plague ? PlagueColour : Gold;
            m_trail.startColor = new Color(trail.r, trail.g, trail.b, 0.55f);
            m_trail.endColor = new Color(trail.r, trail.g, trail.b, 0f);

            if (Variant == Javelins.Variant.Bolt)
            {
                m_bolts = new LineRenderer[BoltStrands];
                for (int i = 0; i < BoltStrands; i++)
                {
                    var go = new GameObject("Orlog_BoltStrand");
                    go.transform.SetParent(transform, false);
                    var line = go.AddComponent<LineRenderer>();
                    line.sharedMaterial = lineMaterial;
                    line.useWorldSpace = false;
                    line.positionCount = BoltPoints;
                    line.startWidth = 0.05f;
                    line.endWidth = 0.03f;
                    line.startColor = Color.white;
                    line.endColor = BoltColour;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.receiveShadows = false;
                    m_bolts[i] = line;
                }
                Jag();
            }
            else if (Variant == Javelins.Variant.Plague)
            {
                m_drips = Drips(lineMaterial);
            }
        }

        private static Bounds Encapsulate(Bounds a, Bounds b)
        {
            a.Encapsulate(b);
            return a;
        }

        /// <summary>The see-through gold the ghost is drawn in (one material for every ghost; per ghost only while it fades).</summary>
        private static Material GhostMaterial()
        {
            if (s_ghostMaterial || s_searched)
            {
                return s_ghostMaterial;
            }
            s_searched = true;
            foreach (var name in new[] { "Sprites/Default", "Legacy Shaders/Particles/Alpha Blended", "UI/Default" })
            {
                var shader = Shader.Find(name);
                if (shader)
                {
                    s_ghostMaterial = new Material(shader) { name = "Orlog_GhostSpear", color = new Color(Gold.r, Gold.g, Gold.b, GhostAlpha) };
                    Jotunn.Logger.LogInfo("Orlog: the ghost javelin is drawn with " + shader.name + ".");
                    return s_ghostMaterial;
                }
            }
            Jotunn.Logger.LogInfo("Orlog: no see-through material found for the ghost javelin; it keeps the spear's look with its gold light and trail.");
            return null;
        }

        /// <summary>Plague Javelin's green drips: small drops falling from the spear in flight (engine particles).</summary>
        private ParticleSystem Drips(Material material)
        {
            var go = new GameObject("Orlog_PlagueDrips");
            go.transform.SetParent(transform, false);
            var drips = go.AddComponent<ParticleSystem>();
            drips.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = drips.main;
            main.loop = true;
            main.startLifetime = 0.6f;
            main.startSpeed = 0f;
            main.startSize = 0.07f;
            main.startColor = new Color(PlagueColour.r, PlagueColour.g, PlagueColour.b, 0.9f);
            main.gravityModifier = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;
            var emission = drips.emission;
            emission.rateOverTime = 30f;
            var shape = drips.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.05f, 0.05f, m_length * 0.6f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            drips.Play();
            return drips;
        }

        /// <summary>Lightning Bolt's strands: jagged lines along the spear, redrawn a few times a second.</summary>
        private void Jag()
        {
            if (m_bolts == null)
            {
                return;
            }
            float half = m_length * 0.5f;
            foreach (var line in m_bolts)
            {
                if (!line)
                {
                    continue;
                }
                for (int i = 0; i < BoltPoints; i++)
                {
                    float z = Mathf.Lerp(-half, half * 0.8f, i / (BoltPoints - 1f));
                    Vector2 off = i == 0 || i == BoltPoints - 1 ? Vector2.zero : Random.insideUnitCircle * 0.14f;
                    line.SetPosition(i, new Vector3(off.x, off.y, z));
                }
            }
            m_nextJag = Time.time + 0.05f;
        }

        private void Update()
        {
            if (m_stuckAt < 0f)
            {
                if (m_bolts != null && Time.time >= m_nextJag)
                {
                    Jag();
                }
                if (Stopped())
                {
                    m_stuckAt = Time.time;
                    Stick();
                }
                return;
            }
            Fade(ActiveSkillRules.GhostAlpha(Time.time - m_stuckAt));
        }

        /// <summary>
        ///     Whether the ghost has stopped: the thrower's game knows (the projectile hit); on the others' games it is the
        ///     synced position standing still.
        /// </summary>
        private bool Stopped()
        {
            if (m_projectile && m_projectile.m_didHit)
            {
                return true;
            }
            Vector3 position = transform.position;
            bool moved = (position - m_lastPosition).sqrMagnitude > 0.0004f;
            m_lastPosition = position;
            if (moved || Time.time - m_born < 0.3f)
            {
                m_stillSince = -1f;
                return false;
            }
            if (m_stillSince < 0f)
            {
                m_stillSince = Time.time;
            }
            return Time.time - m_stillSince > 0.15f;
        }

        private void Stick()
        {
            if (m_trail)
            {
                m_trail.emitting = false;
            }
            if (m_drips)
            {
                m_drips.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            if (m_bolts != null)
            {
                foreach (var line in m_bolts)
                {
                    if (line)
                    {
                        line.enabled = false;
                    }
                }
            }
            if (s_ghostMaterial && m_renderers != null)
            {
                m_fadeMaterial = new Material(s_ghostMaterial) { name = "Orlog_GhostSpearFading" };
                foreach (var r in m_renderers)
                {
                    if (!r)
                    {
                        continue;
                    }
                    var shared = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < shared.Length; i++)
                    {
                        shared[i] = m_fadeMaterial;
                    }
                    r.sharedMaterials = shared;
                }
            }
        }

        private void Fade(float alpha)
        {
            if (m_fadeMaterial)
            {
                m_fadeMaterial.color = new Color(Gold.r, Gold.g, Gold.b, GhostAlpha * alpha);
            }
            else if (alpha <= 0f && m_renderers != null)
            {
                foreach (var r in m_renderers)
                {
                    if (r)
                    {
                        r.enabled = false;
                    }
                }
            }
            if (m_light)
            {
                m_light.intensity = m_lightIntensity * alpha;
            }
        }

        private void OnDestroy()
        {
            if (m_fadeMaterial)
            {
                Destroy(m_fadeMaterial);
            }
        }
    }
}
