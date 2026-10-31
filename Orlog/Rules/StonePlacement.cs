using System;
using System.Collections.Generic;
using System.Linq;

namespace Orlog.Rules
{
    /// <summary>
    ///     Where the Orlog stone goes in the spawn area: in the widest gap of the ring of boss trophy stones, at their
    ///     average distance from the centre, facing the centre, so it reads as one more stone of the ring. Works on the
    ///     ground plane (x, z). No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class StonePlacement
    {
        public struct Spot
        {
            public float X;
            public float Z;
            /// <summary>Rotation about the vertical axis in degrees (Unity: 0 = facing +z, clockwise seen from above).</summary>
            public float Yaw;
        }

        /// <summary>
        ///     The spot, or null with fewer than two stones (no ring to read). <paramref name="stonesX" /> and
        ///     <paramref name="stonesZ" /> are the trophy stones' positions, <paramref name="centerX" /> and
        ///     <paramref name="centerZ" /> the start location's centre.
        /// </summary>
        public static Spot? Place(float centerX, float centerZ, IList<float> stonesX, IList<float> stonesZ)
        {
            int count = Math.Min(stonesX.Count, stonesZ.Count);
            if (count < 2)
            {
                return null;
            }
            var angles = new List<double>();
            double radius = 0;
            for (int i = 0; i < count; i++)
            {
                double dx = stonesX[i] - centerX;
                double dz = stonesZ[i] - centerZ;
                angles.Add(Math.Atan2(dz, dx));
                radius += Math.Sqrt(dx * dx + dz * dz);
            }
            radius /= count;
            angles.Sort();

            // The widest gap between neighbouring stones, going round the circle (the last gap wraps to the first).
            double bestGap = -1, bestMiddle = 0;
            for (int i = 0; i < angles.Count; i++)
            {
                double from = angles[i];
                double to = i + 1 < angles.Count ? angles[i + 1] : angles[0] + 2 * Math.PI;
                double gap = to - from;
                if (gap > bestGap + 1e-9)
                {
                    bestGap = gap;
                    bestMiddle = from + gap / 2;
                }
            }

            double x = centerX + radius * Math.Cos(bestMiddle);
            double z = centerZ + radius * Math.Sin(bestMiddle);
            // Face the centre: Unity's yaw is measured from +z towards +x.
            double yaw = Math.Atan2(centerX - x, centerZ - z) * 180.0 / Math.PI;
            return new Spot { X = (float)x, Z = (float)z, Yaw = (float)((yaw + 360.0) % 360.0) };
        }
    }
}
