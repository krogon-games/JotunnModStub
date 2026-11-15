using System;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The quick-cast shimmer's path (user, 2026-11-13: a rotating shimmer round a quick-cast slot, "like the dota 2
    ///     autocast animation"): two glowing sparks, each with a fading trail, run clockwise round the slot's square edge,
    ///     opposite each other. Pure maths, unit-tested; <see cref="SkillCross" /> draws it.
    /// </summary>
    internal static class QuickCastShimmer
    {
        /// <summary>Laps per second.</summary>
        public const float Speed = 0.45f;

        public const int Sparks = 2;

        /// <summary>Dots in each spark's trail, the spark first.</summary>
        public const int Trail = 8;

        /// <summary>Distance between trail dots, as a share of one lap.</summary>
        public const float TrailStep = 0.014f;

        /// <summary>Where a dot is on the edge, from 0 to 1 of a lap.</summary>
        public static float Progress(float time, int spark, int dot)
        {
            float p = time * Speed + (float)spark / Sparks - dot * TrailStep;
            return p - (float)Math.Floor(p);
        }

        /// <summary>
        ///     The point at <paramref name="progress" /> round a square of half-size <paramref name="half" /> centred on 0,
        ///     clockwise from the top-left corner (y up).
        /// </summary>
        public static void Point(float progress, float half, out float x, out float y)
        {
            float p = progress - (float)Math.Floor(progress);
            float d = p * 4f;
            float side = 2f * half;
            if (d < 1f)
            {
                x = -half + d * side;
                y = half;
            }
            else if (d < 2f)
            {
                x = half;
                y = half - (d - 1f) * side;
            }
            else if (d < 3f)
            {
                x = half - (d - 2f) * side;
                y = -half;
            }
            else
            {
                x = -half;
                y = -half + (d - 3f) * side;
            }
        }

        /// <summary>A trail dot's opacity: the spark full, fading along the trail.</summary>
        public static float Alpha(int dot)
        {
            if (dot < 0 || dot >= Trail)
            {
                return 0f;
            }
            float t = 1f - (float)dot / Trail;
            return t * t;
        }

        /// <summary>A trail dot's size as a share of the spark's.</summary>
        public static float Size(int dot)
        {
            return dot < 0 || dot >= Trail ? 0f : 1f - 0.6f * dot / Trail;
        }
    }
}
