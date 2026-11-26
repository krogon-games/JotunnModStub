using System;
using System.Collections.Generic;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class RipFallbackTests
    {
        private static RipRules.Placement P(float hx, float hz, float lx, float lz)
        {
            return new RipRules.Placement(new RipRules.Spot(hx, hz), new RipRules.Spot(lx, lz));
        }

        [Fact]
        public void DefaultDistanceIs96AndMatchesTheWavesRange()
        {
            Assert.Equal(96f, RipRules.DefaultBaseDistance);
            Assert.Equal(RipRules.HeartMinDistance, RipRules.DefaultBaseDistance);
            Assert.Equal(96f, RipRules.BaseDistance(0f));
            Assert.Equal(96f, RipRules.BaseDistance(-5f));
            Assert.Equal(150f, RipRules.BaseDistance(150f));
        }

        [Fact]
        public void HeartOrLandingNearAPlacedObjectRejectsTheCandidate()
        {
            var candidates = new List<RipRules.Placement>
            {
                P(0, 0, 100, 0),      // heart near the object
                P(500, 0, 0, 0),      // landing near the object
                P(1000, 1000, 1100, 1000), // far from everything
            };
            var scan = new RipRules.PlacedScan(candidates, 96f);
            Assert.Equal(3, scan.Alive);
            Assert.True(scan.Touches(50, 0));
            Assert.Equal(2, scan.Reject(50, 0));
            Assert.Equal(1, scan.Alive);
            Assert.False(scan.AllRejected);
            var first = scan.First();
            Assert.True(first.HasValue);
            Assert.Equal(1000f, first.Value.Heart.X);
        }

        [Fact]
        public void DistanceIsHorizontalAndStrict()
        {
            var scan = new RipRules.PlacedScan(new[] { P(0, 0, 500, 500) }, 96f);
            Assert.False(scan.Touches(96f, 0f));
            Assert.True(scan.Touches(95.9f, 0f));
            Assert.True(scan.Touches(0f, -95.9f));
            Assert.False(scan.Touches(68f, 68f)); // 96.17 m diagonally
            Assert.True(scan.Touches(67f, 67f));  // 94.75 m
        }

        [Fact]
        public void RejectedCandidatesStopTouching()
        {
            var scan = new RipRules.PlacedScan(new[] { P(0, 0, 200, 0) }, 96f);
            Assert.Equal(1, scan.Reject(10, 10));
            Assert.True(scan.AllRejected);
            Assert.False(scan.Touches(10, 10));
            Assert.Null(scan.First());
        }

        [Fact]
        public void NegativeCoordinatesAndCellEdgesWork()
        {
            var scan = new RipRules.PlacedScan(new[] { P(-1, -1, -300, -300) }, 96f);
            Assert.True(scan.Touches(1, 1));      // across the cell boundary at 0
            Assert.True(scan.Touches(-300, -380));
            Assert.False(scan.Touches(-300, -397));
        }

        [Fact]
        public void GridMatchesBruteForce()
        {
            var rng = new Random(7);
            var candidates = new List<RipRules.Placement>();
            for (int i = 0; i < 200; i++)
            {
                float hx = (float)(rng.NextDouble() * 4000 - 2000), hz = (float)(rng.NextDouble() * 4000 - 2000);
                candidates.Add(P(hx, hz, hx + 110, hz + 20));
            }
            var placed = new List<RipRules.Spot>();
            for (int i = 0; i < 300; i++)
            {
                placed.Add(new RipRules.Spot((float)(rng.NextDouble() * 4000 - 2000), (float)(rng.NextDouble() * 4000 - 2000)));
            }
            var scan = new RipRules.PlacedScan(candidates, 96f);
            foreach (var p in placed)
            {
                scan.Reject(p.X, p.Z);
            }
            int expected = 0;
            foreach (var c in candidates)
            {
                bool away = RipRules.IsAwayFromBases(c.Heart, placed, 96f) && RipRules.IsAwayFromBases(c.Landing, placed, 96f);
                if (away)
                {
                    expected++;
                }
            }
            Assert.Equal(expected, scan.Alive);
        }
    }
}
