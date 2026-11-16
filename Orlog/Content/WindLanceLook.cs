using System.Collections.Generic;
using Orlog.Rules;
using UnityEngine;
using UnityEngine.Rendering;

namespace Orlog.Content
{
    /// <summary>
    ///     How Wind Lance looks (Krogon, 2026-11-16: "wind particles around the arrow like a tornado ... much tighter
    ///     around the arrow, and make the arrow bigger"; reference <c>docs/images/wind_lance_reference.webp</c>). Built in
    ///     code from engine parts only (<see cref="LineRenderer" />, <see cref="Light" />, a bigger arrow), because the
    ///     mod has no particle material or asset bundle for it. The shapes are in <see cref="WindSwirlRules" />;
    ///     <c>tools/preview_wind_lance.py</c> draws them outside the game.
    ///     Everything here is only a look: if no material is found the strands are left out and the larger arrow and its
    ///     light still show.
    /// </summary>
    internal static class WindLanceLook
    {
        private static readonly Color Head = Color.white;
        private static readonly Color Tail = new Color(0.8f, 0.95f, 1f);
        private static readonly Color TailGone = new Color(0.8f, 0.95f, 1f, 0f);
        private static readonly Color LightColour = new Color(0.8f, 0.95f, 1f);

        private static Material s_material;
        private static bool s_searched;
        private static Gradient s_lanceGradient, s_chargeGradient;
        private static AnimationCurve s_lanceWidth, s_chargeWidth;
        private static ChargeSwirlEffect s_charge;

        /// <summary>
        ///     Makes a skill arrow look like a lance of wind, on the shooter's game: the arrow three times as large, a
        ///     bright core streak, a pale light, the swirl riding on it, and a wake left along its path.
        /// </summary>
        public static void Dress(Projectile lance)
        {
            if (!lance)
            {
                return;
            }
            Safe.Run("dress the Wind Lance arrow", () =>
            {
                var material = FindMaterial(lance);
                lance.transform.localScale *= WindSwirlRules.ArrowScale;

                foreach (var trail in lance.GetComponentsInChildren<TrailRenderer>(true))
                {
                    trail.widthMultiplier *= WindSwirlRules.ArrowScale;
                    trail.startColor = Color.white;
                    trail.endColor = TailGone;
                }

                // The arrow's children are three times as large now; this child cancels that so our metres stay metres.
                var scale = lance.transform.localScale;
                var holder = new GameObject("Orlog_WindLance");
                holder.transform.SetParent(lance.transform, false);
                holder.transform.localScale = new Vector3(Inverse(scale.x), Inverse(scale.y), Inverse(scale.z));

                var light = holder.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = LightColour;
                light.range = 5f;
                light.intensity = 2.5f;

                if (!material)
                {
                    return;
                }

                // The core: a bright straight streak, so the arrow reads as a javelin.
                var core = NewLine(holder.transform, "Core", material, false);
                core.positionCount = 2;
                core.SetPosition(0, new Vector3(0f, 0f, 0.4f));
                core.SetPosition(1, new Vector3(0f, 0f, 0.4f - WindSwirlRules.CoreLength));
                core.startWidth = WindSwirlRules.CoreWidth;
                core.endWidth = 0f;
                core.startColor = new Color(1f, 1f, 1f, 0.85f);
                core.endColor = new Color(1f, 1f, 1f, 0f);

                // The swirl riding on the arrow.
                var shape = WindSwirlRules.Lance;
                var swirl = holder.AddComponent<LanceSwirl>();
                swirl.Lines = new LineRenderer[shape.Ribbons];
                for (int r = 0; r < shape.Ribbons; r++)
                {
                    var line = NewLine(holder.transform, "Swirl" + r, material, false);
                    line.positionCount = shape.Points;
                    ApplyShape(line, shape, ref s_lanceGradient, ref s_lanceWidth);
                    swirl.Lines[r] = line;
                }

                // The wake stays in the air after the lance is gone, so it is not a child of the lance.
                var wake = new GameObject("Orlog_WindWake");
                var behaviour = wake.AddComponent<WakeEffect>();
                behaviour.Begin(lance.transform, material);
            });
        }

        /// <summary>A ring of wind at the bow as the lance is released; <paramref name="direction" /> is the shot's direction.</summary>
        public static void Burst(Vector3 position, Vector3 direction)
        {
            Safe.Run("show the Wind Lance release ring", () =>
            {
                Ring(position, direction, WindSwirlRules.BurstLife, WindSwirlRules.BurstStart, WindSwirlRules.BurstEnd, true);
            });
        }

        /// <summary>A smaller ring of wind where the lance passes through an enemy (or where it ends).</summary>
        public static void Puff(Vector3 position, Vector3 direction)
        {
            Safe.Run("show the Wind Lance puff", () =>
            {
                Ring(position, direction, WindSwirlRules.PuffLife, WindSwirlRules.PuffStart, WindSwirlRules.PuffEnd, false);
            });
        }

        /// <summary>
        ///     Call every frame while the charged shot is ready: wind swirls around the bow hand and a pale light glows; it
        ///     removes itself 0.25 s after the last call.
        /// </summary>
        public static void ChargeSwirl(Player player)
        {
            if (!player)
            {
                return;
            }
            Safe.Run("show the Wind Lance charge swirl", () =>
            {
                if (!s_charge)
                {
                    var hand = player.m_visEquipment ? player.m_visEquipment.m_leftHand : null;
                    var go = new GameObject("Orlog_WindCharge");
                    if (hand)
                    {
                        go.transform.SetParent(hand, false);
                    }
                    else
                    {
                        go.transform.SetParent(player.transform, false);
                        go.transform.position = player.GetCenterPoint();
                    }
                    var light = go.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = LightColour;
                    light.range = 3f;
                    light.intensity = 3f;

                    s_charge = go.AddComponent<ChargeSwirlEffect>();
                    var material = FindMaterial(null);
                    var shape = WindSwirlRules.Charge;
                    s_charge.Lines = new LineRenderer[material ? shape.Ribbons : 0];
                    for (int r = 0; r < s_charge.Lines.Length; r++)
                    {
                        var line = NewLine(go.transform, "Swirl" + r, material, true);
                        line.positionCount = shape.Points;
                        ApplyShape(line, shape, ref s_chargeGradient, ref s_chargeWidth);
                        s_charge.Lines[r] = line;
                    }
                }
                s_charge.Player = player;
                s_charge.Until = Time.time + 0.25f;
            });
        }

        private static float Inverse(float v) => Mathf.Abs(v) < 1e-4f ? 1f : 1f / v;

        // ---------------------------------------------------------------- material

        /// <summary>
        ///     The material all the wind is drawn with, found once: the soft trail material a vanilla arrow carries, else a
        ///     plain see-through one from a shader the game ships. Null when neither is found.
        /// </summary>
        private static Material FindMaterial(Projectile lance)
        {
            if (s_material)
            {
                return s_material;
            }
            if (s_searched && !lance)
            {
                return null;
            }
            if (lance)
            {
                foreach (var trail in lance.GetComponentsInChildren<TrailRenderer>(true))
                {
                    if (trail.sharedMaterial)
                    {
                        s_material = trail.sharedMaterial;
                        s_searched = true;
                        Jotunn.Logger.LogInfo("Orlog: Wind Lance's wind is drawn with " + s_material.name + ".");
                        return s_material;
                    }
                }
            }
            if (s_searched)
            {
                return null;
            }
            s_searched = true;
            foreach (var name in new[] { "Sprites/Default", "UI/Default", "Legacy Shaders/Particles/Additive", "Particles/Standard Unlit" })
            {
                var shader = Shader.Find(name);
                if (shader)
                {
                    s_material = new Material(shader) { name = "Orlog_WindLance" };
                    Jotunn.Logger.LogInfo("Orlog: Wind Lance's wind is drawn with " + shader.name + ".");
                    return s_material;
                }
            }
            Jotunn.Logger.LogInfo("Orlog: no material found for Wind Lance's wind; it shows only the larger arrow and its light.");
            return null;
        }

        // ---------------------------------------------------------------- line helpers

        private static LineRenderer NewLine(Transform parent, string name, Material material, bool worldSpace)
        {
            var go = new GameObject("Orlog_Wind" + name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = worldSpace;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.numCapVertices = 2;
            return line;
        }

        /// <summary>
        ///     Gives a strand its width and fade from the shape's numbers (the same for every strand of the shape and for
        ///     every moment, so the gradient and curve are built once and shared).
        /// </summary>
        private static void ApplyShape(LineRenderer line, SwirlShape shape, ref Gradient gradient, ref AnimationCurve width)
        {
            if (gradient == null || width == null)
            {
                var points = new SwirlPoint[shape.Points];
                WindSwirlRules.Ribbon(shape, 0, 0f, points);
                var colours = new[] { new GradientColorKey(Head, 0f), new GradientColorKey(Tail, 1f) };
                const int steps = 8;
                var alphas = new GradientAlphaKey[steps];
                for (int k = 0; k < steps; k++)
                {
                    float u = k / (steps - 1f);
                    alphas[k] = new GradientAlphaKey(0.9f * points[Mathf.RoundToInt(u * (shape.Points - 1))].Alpha, u);
                }
                gradient = new Gradient();
                gradient.SetKeys(colours, alphas);
                var keys = new Keyframe[shape.Points];
                for (int i = 0; i < shape.Points; i++)
                {
                    keys[i] = new Keyframe(i / (shape.Points - 1f), points[i].Width);
                }
                width = new AnimationCurve(keys);
            }
            line.colorGradient = gradient;
            line.widthCurve = width;
            line.widthMultiplier = 1f;
        }

        private static void SetColours(LineRenderer line, float alpha)
        {
            var head = Head;
            head.a = alpha;
            var tail = Tail;
            tail.a = alpha;
            line.startColor = head;
            line.endColor = tail;
        }

        // ---------------------------------------------------------------- rings

        private static void Ring(Vector3 position, Vector3 direction, float life, float start, float end, bool withLight)
        {
            var go = new GameObject(withLight ? "Orlog_WindBurst" : "Orlog_WindPuff");
            go.transform.position = position;
            go.transform.rotation = direction.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(direction) : Quaternion.identity;
            var ring = go.AddComponent<RingEffect>();
            var material = FindMaterial(null);
            ring.Lines = new LineRenderer[material ? 2 : 0];
            for (int i = 0; i < ring.Lines.Length; i++)
            {
                var line = NewLine(go.transform, "Ring" + i, material, false);
                line.positionCount = WindSwirlRules.RingPoints;
                line.startWidth = 0.08f;
                line.endWidth = 0.02f;
                ring.Lines[i] = line;
            }
            if (withLight)
            {
                ring.Glow = go.AddComponent<Light>();
                ring.Glow.type = LightType.Point;
                ring.Glow.color = LightColour;
                ring.Glow.range = 4f;
                ring.Glow.intensity = 3f;
            }
            ring.Begin(life, start, end);
        }

        /// <summary>A ring of wind that grows and fades, turning about its axis, then removes itself.</summary>
        private sealed class RingEffect : MonoBehaviour
        {
            public LineRenderer[] Lines;
            public Light Glow;
            private readonly Vector3[] m_points = new Vector3[WindSwirlRules.RingPoints];
            private Quaternion m_base;
            private float m_born, m_life, m_start, m_end;

            public void Begin(float life, float start, float end)
            {
                m_born = Time.time;
                m_life = life;
                m_start = start;
                m_end = end;
                m_base = transform.rotation;
                Apply(0f);
            }

            private void Update()
            {
                float age = Time.time - m_born;
                if (age >= m_life)
                {
                    Destroy(gameObject);
                    return;
                }
                Apply(age);
            }

            private void Apply(float age)
            {
                float radius = WindSwirlRules.RingRadius(age, m_life, m_start, m_end);
                float alpha = WindSwirlRules.RingAlpha(age, m_life);
                transform.rotation = m_base * Quaternion.Euler(0f, 0f, 360f * age);
                if (Lines != null)
                {
                    for (int l = 0; l < Lines.Length; l++)
                    {
                        if (!Lines[l])
                        {
                            continue;
                        }
                        for (int i = 0; i < m_points.Length; i++)
                        {
                            float f = i / (m_points.Length - 1f);
                            float angle = 2f * Mathf.PI * f + (l == 1 ? Mathf.PI : 0f);
                            float r = radius * (0.85f + 0.15f * f);
                            m_points[i] = new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle), 0f);
                        }
                        Lines[l].SetPositions(m_points);
                        SetColours(Lines[l], 0.8f * alpha);
                    }
                }
                if (Glow)
                {
                    Glow.intensity = 3f * alpha;
                }
            }
        }

        // ---------------------------------------------------------------- the swirl on the arrow

        /// <summary>Turns the strands riding on the lance (in the lance's own space; +Z is the flight direction).</summary>
        private sealed class LanceSwirl : MonoBehaviour
        {
            public LineRenderer[] Lines;
            private readonly SwirlPoint[] m_swirl = new SwirlPoint[WindSwirlRules.Lance.Points];
            private readonly Vector3[] m_positions = new Vector3[WindSwirlRules.Lance.Points];

            private void LateUpdate()
            {
                if (Lines == null)
                {
                    return;
                }
                for (int r = 0; r < Lines.Length; r++)
                {
                    if (!Lines[r])
                    {
                        continue;
                    }
                    WindSwirlRules.Ribbon(WindSwirlRules.Lance, r, Time.time, m_swirl);
                    for (int i = 0; i < m_swirl.Length; i++)
                    {
                        m_positions[i] = new Vector3(m_swirl[i].X, m_swirl[i].Y, m_swirl[i].Z);
                    }
                    Lines[r].SetPositions(m_positions);
                }
            }
        }

        // ---------------------------------------------------------------- the wake

        /// <summary>
        ///     Wind left hanging along the lance's path: helices around the path so far. When the lance is gone it widens
        ///     and fades over <see cref="WindSwirlRules.WakeSeconds" />, then removes itself.
        /// </summary>
        private sealed class WakeEffect : MonoBehaviour
        {
            private const float Step = 0.2f;
            private const int MaxPoints = 300;
            private const float WakeAlpha = 0.6f;
            private const float GiveUp = 5f;

            private readonly List<Vector3> m_path = new List<Vector3>();
            private readonly Vector3[] m_positions = new Vector3[MaxPoints];
            private LineRenderer[] m_lines;
            private Transform m_lance;
            private Vector3 m_lastSeen;
            private float m_born;
            private float m_goneAt = -1f;

            public void Begin(Transform lance, Material material)
            {
                m_lance = lance;
                m_born = Time.time;
                m_lastSeen = lance.position;
                m_path.Add(m_lastSeen);
                var shape = WindSwirlRules.Wake;
                m_lines = new LineRenderer[shape.Ribbons];
                for (int r = 0; r < shape.Ribbons; r++)
                {
                    var line = NewLine(transform, "Wake" + r, material, true);
                    line.positionCount = 0;
                    line.startWidth = shape.WidthHead;
                    line.endWidth = shape.WidthTail;
                    m_lines[r] = line;
                }
            }

            private void LateUpdate()
            {
                if (m_lines == null)
                {
                    Destroy(gameObject);
                    return;
                }
                float now = Time.time;
                float alpha = WakeAlpha, grow = 1f;
                if (m_lance)
                {
                    if (now - m_born > GiveUp)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    m_lastSeen = m_lance.position;
                    Append(m_lastSeen, Step);
                }
                else
                {
                    if (m_goneAt < 0f)
                    {
                        m_goneAt = now;
                        Append(m_lastSeen, 0.01f);   // the lance's last spot, so the wake reaches where it ended
                    }
                    float t = (now - m_goneAt) / WindSwirlRules.WakeSeconds;
                    if (t >= 1f)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    alpha = WakeAlpha * (1f - t);
                    grow = Mathf.Lerp(1f, WindSwirlRules.WakeGrow, t);
                }
                Rebuild(now, alpha, grow);
            }

            private void Append(Vector3 position, float minDistance)
            {
                if ((position - m_path[m_path.Count - 1]).sqrMagnitude >= minDistance * minDistance)
                {
                    m_path.Add(position);
                }
            }

            /// <summary>Helices around the path polyline, sampled every 1/WakePointsPerMetre metres (the newest 300 points).</summary>
            private void Rebuild(float now, float alpha, float grow)
            {
                var shape = WindSwirlRules.Wake;
                float total = 0f;
                for (int i = 1; i < m_path.Count; i++)
                {
                    total += Vector3.Distance(m_path[i - 1], m_path[i]);
                }
                float spacing = 1f / WindSwirlRules.WakePointsPerMetre;
                float first = Mathf.Max(0f, total - (MaxPoints - 1) * spacing);
                int count = total < 0.01f ? 0 : Mathf.Min(MaxPoints, Mathf.FloorToInt((total - first) / spacing) + 1);
                float radius = shape.RadiusHead * grow;

                for (int r = 0; r < m_lines.Length; r++)
                {
                    var line = m_lines[r];
                    if (!line)
                    {
                        continue;
                    }
                    int seg = 0;
                    float segStart = 0f;
                    for (int k = 0; k < count; k++)
                    {
                        float s = first + k * spacing;
                        // Advance to the segment holding arc length s.
                        while (seg < m_path.Count - 2)
                        {
                            float segLength = Vector3.Distance(m_path[seg], m_path[seg + 1]);
                            if (s <= segStart + segLength)
                            {
                                break;
                            }
                            segStart += segLength;
                            seg++;
                        }
                        Vector3 a = m_path[seg], b = m_path[seg + 1];
                        float length = Vector3.Distance(a, b);
                        Vector3 dir = length > 1e-4f ? (b - a) / length : Vector3.forward;
                        Vector3 p = a + dir * Mathf.Clamp(s - segStart, 0f, length);
                        Vector3 right = Vector3.Cross(Vector3.up, dir);
                        right = right.sqrMagnitude < 1e-4f ? Vector3.right : right.normalized;
                        Vector3 up = Vector3.Cross(dir, right);
                        float angle = 2f * Mathf.PI * (r / (float)shape.Ribbons + WindSwirlRules.WakeTurnsPerMetre * s - shape.SpinPerSecond * now);
                        m_positions[k] = p + radius * (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up);
                    }
                    line.positionCount = count;
                    if (count > 0)
                    {
                        line.SetPositions(m_positions);
                    }
                    SetColours(line, alpha);
                }
            }
        }

        // ---------------------------------------------------------------- the charge

        /// <summary>
        ///     Wind around the nocked arrow while the charged shot is ready. Rebuilt in world space each frame around the
        ///     aim direction from the bow hand; removes itself 0.25 s after the last call to <see cref="ChargeSwirl" />.
        /// </summary>
        private sealed class ChargeSwirlEffect : MonoBehaviour
        {
            public LineRenderer[] Lines;
            public Player Player;
            public float Until;
            private readonly SwirlPoint[] m_swirl = new SwirlPoint[WindSwirlRules.Charge.Points];
            private readonly Vector3[] m_positions = new Vector3[WindSwirlRules.Charge.Points];

            private void LateUpdate()
            {
                if (Time.time > Until || !Player)
                {
                    Destroy(gameObject);
                    return;
                }
                if (Lines == null)
                {
                    return;
                }
                Vector3 hand = transform.position;
                Vector3 dir = Player.GetLookDir();
                if (dir.sqrMagnitude < 1e-6f)
                {
                    dir = Vector3.forward;
                }
                dir.Normalize();
                Vector3 right = Vector3.Cross(Vector3.up, dir);
                right = right.sqrMagnitude < 1e-4f ? Vector3.right : right.normalized;
                Vector3 up = Vector3.Cross(dir, right);
                for (int r = 0; r < Lines.Length; r++)
                {
                    if (!Lines[r])
                    {
                        continue;
                    }
                    WindSwirlRules.Ribbon(WindSwirlRules.Charge, r, Time.time, m_swirl);
                    for (int i = 0; i < m_swirl.Length; i++)
                    {
                        m_positions[i] = hand + right * m_swirl[i].X + up * m_swirl[i].Y + dir * m_swirl[i].Z;
                    }
                    Lines[r].SetPositions(m_positions);
                }
            }
        }
    }
}
