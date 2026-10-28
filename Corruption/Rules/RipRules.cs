using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Game-independent parts of opening a rip: sampling landing spots, rejecting spots near bases, timing.
    ///     The game checks (biome, height, flatness, explored) are passed in, so this is unit-tested.
    /// </summary>
    internal static class RipRules
    {
        /// <summary>The playable world is a disc of about this radius (metres); the edge beyond is ocean.</summary>
        public const float WorldRadius = 9500f;

        public struct Spot
        {
            public float X;
            public float Z;

            public Spot(float x, float z)
            {
                X = x;
                Z = z;
            }
        }

        /// <summary>
        ///     Random spots spread evenly over the world disc that pass <paramref name="accept" />, up to
        ///     <paramref name="maxResults" />. <paramref name="random" /> returns numbers in [0, 1).
        /// </summary>
        public static List<Spot> Sample(Func<double> random, int samples, int maxResults, Func<float, float, bool> accept)
        {
            var spots = new List<Spot>();
            for (int i = 0; i < samples && spots.Count < maxResults; i++)
            {
                double r = WorldRadius * Math.Sqrt(random());
                double a = 2 * Math.PI * random();
                float x = (float)(r * Math.Cos(a));
                float z = (float)(r * Math.Sin(a));
                if (accept(x, z))
                {
                    spots.Add(new Spot(x, z));
                }
            }
            return spots;
        }

        /// <summary>The first candidate with no base marker within <paramref name="distance" /> metres, or null.</summary>
        public static Spot? FirstAwayFromBases(IList<Spot> candidates, IList<Spot> bases, float distance)
        {
            float limit = distance * distance;
            foreach (var c in candidates)
            {
                bool near = false;
                foreach (var b in bases)
                {
                    float dx = c.X - b.X;
                    float dz = c.Z - b.Z;
                    if (dx * dx + dz * dz < limit)
                    {
                        near = true;
                        break;
                    }
                }
                if (!near)
                {
                    return c;
                }
            }
            return null;
        }

        /// <summary>Time left until a rip closes, as "1 h 05 min" / "12 min" / "less than a minute".</summary>
        public static string TimeLeft(double secondsLeft)
        {
            if (secondsLeft < 60)
            {
                return "less than a minute";
            }
            int minutes = (int)(secondsLeft / 60);
            return minutes >= 60 ? $"{minutes / 60} h {minutes % 60:00} min" : $"{minutes} min";
        }
    }
}
