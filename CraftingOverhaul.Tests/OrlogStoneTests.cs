using System;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogStoneTests
    {
        private static (float[] X, float[] Z) Ring(float radius, params double[] degrees)
        {
            var x = new float[degrees.Length];
            var z = new float[degrees.Length];
            for (int i = 0; i < degrees.Length; i++)
            {
                x[i] = (float)(radius * Math.Cos(degrees[i] * Math.PI / 180));
                z[i] = (float)(radius * Math.Sin(degrees[i] * Math.PI / 180));
            }
            return (x, z);
        }

        [Fact]
        public void GoesInTheWidestGapFacingTheCentre()
        {
            // Stones at 0, 90 and 180 degrees: the widest gap is 180-360, its middle at 270 degrees = (0, -10).
            var (x, z) = Ring(10f, 0, 90, 180);
            var spot = StonePlacement.Place(0f, 0f, x, z).Value;
            Assert.Equal(0f, spot.X, 3);
            Assert.Equal(-10f, spot.Z, 3);
            // Facing the centre = facing +z.
            Assert.Equal(0f, spot.Yaw, 3);
        }

        [Fact]
        public void TheGapCanWrapAround()
        {
            // Stones at 90, 180 and 270: the widest gap is 270-450 (through 0), middle at 0 degrees = (10, 0), facing -x.
            var (x, z) = Ring(10f, 90, 180, 270);
            var spot = StonePlacement.Place(0f, 0f, x, z).Value;
            Assert.Equal(10f, spot.X, 3);
            Assert.Equal(0f, spot.Z, 3);
            Assert.Equal(270f, spot.Yaw, 3);
        }

        [Fact]
        public void UsesTheAverageDistanceAndTheRealCentre()
        {
            // An even ring of eight stones at 12 m round (100, 50), one missing: the stone fills the missing place.
            var (x, z) = Ring(12f, 0, 45, 90, 135, 180, 225, 270);
            for (int i = 0; i < x.Length; i++)
            {
                x[i] += 100f;
                z[i] += 50f;
            }
            var spot = StonePlacement.Place(100f, 50f, x, z).Value;
            double expected = 315 * Math.PI / 180;
            Assert.Equal(100f + (float)(12 * Math.Cos(expected)), spot.X, 2);
            Assert.Equal(50f + (float)(12 * Math.Sin(expected)), spot.Z, 2);
        }

        [Fact]
        public void NeedsAtLeastTwoStones()
        {
            Assert.Null(StonePlacement.Place(0f, 0f, new[] { 1f }, new[] { 1f }));
            Assert.Null(StonePlacement.Place(0f, 0f, new float[0], new float[0]));
        }
    }
}
