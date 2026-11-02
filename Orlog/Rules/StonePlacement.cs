using System;

namespace Orlog.Rules
{
    /// <summary>
    ///     Where the Orlog stone goes in the spawn area (user, 2026-11-02): behind the Eikthyr trophy stone, on the line
    ///     from the start location's centre through that stone, a set distance further out. Works on the ground plane
    ///     (x, z). No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class StonePlacement
    {
        public struct Spot
        {
            public float X;
            public float Z;
        }

        /// <summary>
        ///     The spot <paramref name="distance" /> metres beyond the stone at (<paramref name="stoneX" />,
        ///     <paramref name="stoneZ" />), away from the centre (<paramref name="centerX" />, <paramref name="centerZ" />).
        ///     Null when the stone stands on the centre (no direction to go).
        /// </summary>
        public static Spot? Behind(float centerX, float centerZ, float stoneX, float stoneZ, float distance)
        {
            double dx = stoneX - centerX;
            double dz = stoneZ - centerZ;
            double length = Math.Sqrt(dx * dx + dz * dz);
            if (length < 1e-3)
            {
                return null;
            }
            return new Spot { X = (float)(stoneX + dx / length * distance), Z = (float)(stoneZ + dz / length * distance) };
        }

        /// <summary>
        ///     How far behind the stone's centre the Orlog stone's centre goes so that <paramref name="gap" /> metres stay
        ///     between them: both stones' footprint radii plus the gap.
        /// </summary>
        public static float Distance(float stoneRadius, float orlogRadius, float gap)
        {
            return Math.Max(0f, stoneRadius) + Math.Max(0f, orlogRadius) + Math.Max(0f, gap);
        }
    }
}
