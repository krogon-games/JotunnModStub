using System;

namespace Orlog.Rules
{
    /// <summary>One point of a wind strand. X is right, Y is up, Z is along the axis (metres); Alpha is 0 to 1.</summary>
    internal struct SwirlPoint
    {
        public float X, Y, Z, Width, Alpha;
    }

    /// <summary>
    ///     The shape of one set of wind strands: helices around an axis, running from a head (where the strands are
    ///     thin and tight) back to a tail (where they open up and fade). The strands are given as numbers so the look can
    ///     be unit-tested and drawn outside the game (<c>tools/preview_wind_lance.py</c>).
    /// </summary>
    internal sealed class SwirlShape
    {
        /// <summary>Strands, evenly spaced around the axis.</summary>
        public int Ribbons;
        /// <summary>Points per strand.</summary>
        public int Points;
        /// <summary>Metres along the axis, from the head (u = 0) to the tail (u = 1).</summary>
        public float Length;
        /// <summary>Z of the head; the strand runs towards negative Z (behind) from there.</summary>
        public float HeadZ;
        /// <summary>Metres from the axis at the head and at the tail (linear in between).</summary>
        public float RadiusHead, RadiusTail;
        /// <summary>Full turns around the axis from head to tail.</summary>
        public float Turns;
        /// <summary>Revolutions per second: the whole helix turns with time.</summary>
        public float SpinPerSecond;
        /// <summary>Strand width in metres at the head and at the tail (linear in between).</summary>
        public float WidthHead, WidthTail;
        /// <summary>Fraction of the strand over which alpha rises from 0 to 1 at the head (0 = no fade-in).</summary>
        public float FadeIn;
    }

    /// <summary>
    ///     The look of Wind Lance (user 2026-11-16: "wind particles around the arrow like a tornado ... much tighter around
    ///     the arrow, and make the arrow bigger"). Pure maths with no game dependencies, so it is unit-tested; the game
    ///     side is <c>WindLanceLook</c>.
    /// </summary>
    internal static class WindSwirlRules
    {
        /// <summary>The swirl riding on the lance's arrow. Never leaves a 0.32 m tube, so it hugs the arrow.</summary>
        public static readonly SwirlShape Lance = new SwirlShape
        {
            Ribbons = 3, Points = 40, Length = 4.0f, HeadZ = 0.6f, RadiusHead = 0.08f, RadiusTail = 0.32f,
            Turns = 3.0f, SpinPerSecond = 5.0f, WidthHead = 0.09f, WidthTail = 0.02f, FadeIn = 0.06f,
        };

        /// <summary>
        ///     The wake left hanging in the air along the lance's path. Its length follows the path, so only the per-metre
        ///     numbers are here (<see cref="WakeTurnsPerMetre" />, <see cref="WakePointsPerMetre" />).
        /// </summary>
        public static readonly SwirlShape Wake = new SwirlShape
        {
            Ribbons = 2, RadiusHead = 0.22f, RadiusTail = 0.22f, WidthHead = 0.07f, WidthTail = 0.07f,
            SpinPerSecond = 1.5f, FadeIn = 0f,
        };

        public const float WakeTurnsPerMetre = 0.35f;
        public const float WakePointsPerMetre = 5f;
        /// <summary>How long the wake takes to fade after the lance is gone.</summary>
        public const float WakeSeconds = 0.7f;
        /// <summary>The wake's radius multiplies up to this as it fades.</summary>
        public const float WakeGrow = 1.8f;

        /// <summary>The swirl around the nocked arrow while the charged shot is ready.</summary>
        public static readonly SwirlShape Charge = new SwirlShape
        {
            Ribbons = 2, Points = 24, Length = 0.9f, HeadZ = 0.45f, RadiusHead = 0.10f, RadiusTail = 0.16f,
            Turns = 1.5f, SpinPerSecond = 3.0f, WidthHead = 0.04f, WidthTail = 0.015f, FadeIn = 0.15f,
        };

        /// <summary>The ring of wind at the bow on release.</summary>
        public const float BurstLife = 0.35f, BurstStart = 0.15f, BurstEnd = 0.9f;
        /// <summary>The ring of wind where the lance passes through an enemy.</summary>
        public const float PuffLife = 0.3f, PuffStart = 0.25f, PuffEnd = 1.0f;
        public const int RingPoints = 28;

        /// <summary>The arrow model is drawn three times as large.</summary>
        public const float ArrowScale = 3f;
        /// <summary>A bright straight streak along the arrow, so it reads as a javelin even if the arrow mesh is small.</summary>
        public const float CoreLength = 2.6f, CoreWidth = 0.16f;

        /// <summary>
        ///     Fills <c>into[0 .. Points-1]</c> with one strand at a moment in time. Does nothing for bad input (fewer than
        ///     2 points, no array, an array too short, a strand number outside the shape).
        /// </summary>
        public static void Ribbon(SwirlShape shape, int ribbon, float time, SwirlPoint[] into)
        {
            if (shape == null || into == null || shape.Points < 2 || shape.Ribbons < 1 || into.Length < shape.Points)
            {
                return;
            }
            float tau = 2f * (float)Math.PI;
            for (int i = 0; i < shape.Points; i++)
            {
                float u = i / (float)(shape.Points - 1);
                float r = Lerp(shape.RadiusHead, shape.RadiusTail, u);
                float angle = tau * (ribbon / (float)shape.Ribbons + shape.Turns * u - shape.SpinPerSecond * time);
                float fade = shape.FadeIn > 0f ? Math.Min(1f, u / shape.FadeIn) : 1f;
                into[i] = new SwirlPoint
                {
                    X = r * (float)Math.Cos(angle),
                    Y = r * (float)Math.Sin(angle),
                    Z = shape.HeadZ - u * shape.Length,
                    Width = Lerp(shape.WidthHead, shape.WidthTail, u),
                    Alpha = fade * (float)Math.Pow(1f - u, 1.5),
                };
            }
        }

        /// <summary>Radius of an expanding ring of wind: it grows fast, then slows (ease-out).</summary>
        public static float RingRadius(float age, float life, float startRadius, float endRadius)
        {
            float t = Clamp01(life > 0f ? age / life : 1f);
            float k = 1f - t;
            return startRadius + (endRadius - startRadius) * (1f - k * k);
        }

        /// <summary>How visible the ring is: full at age 0, gone at the end of its life (and 0 after).</summary>
        public static float RingAlpha(float age, float life)
        {
            if (life <= 0f || age >= life)
            {
                return 0f;
            }
            float k = 1f - Clamp01(age / life);
            return k * k;
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
