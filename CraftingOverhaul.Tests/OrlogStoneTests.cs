using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogStoneTests
    {
        [Fact]
        public void GoesStraightBehindTheStoneAwayFromTheCentre()
        {
            // Centre (0, 0), Eikthyr stone at (10, 0): 6 m further out is (16, 0).
            var spot = StonePlacement.Behind(0f, 0f, 10f, 0f, 6f).Value;
            Assert.Equal(16f, spot.X, 3);
            Assert.Equal(0f, spot.Z, 3);
        }

        [Fact]
        public void WorksAtAnyAngleAndCentre()
        {
            // Centre (100, 50), stone 5 m away diagonally: 5 m further along the same line.
            var spot = StonePlacement.Behind(100f, 50f, 103f, 54f, 5f).Value;
            Assert.Equal(106f, spot.X, 3);
            Assert.Equal(58f, spot.Z, 3);
        }

        [Fact]
        public void NoDirectionWhenTheStoneIsOnTheCentre()
        {
            Assert.Null(StonePlacement.Behind(5f, 5f, 5f, 5f, 3f));
        }

        [Fact]
        public void DistanceKeepsTheGapBetweenTheStones()
        {
            // User, 2026-11-02: 3 m between them. Footprints 1.2 m and 2 m: centres 6.2 m apart.
            Assert.Equal(6.2f, StonePlacement.Distance(1.2f, 2f, 3f), 3);
            Assert.Equal(3f, StonePlacement.Distance(-1f, 0f, 3f), 3);
        }
    }
}
