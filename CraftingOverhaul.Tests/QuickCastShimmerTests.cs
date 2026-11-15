using CraftingOverhaul.Classes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class QuickCastShimmerTests
    {
        [Theory]
        [InlineData(0f, -10f, 10f)]
        [InlineData(0.125f, 0f, 10f)]
        [InlineData(0.25f, 10f, 10f)]
        [InlineData(0.5f, 10f, -10f)]
        [InlineData(0.75f, -10f, -10f)]
        [InlineData(0.875f, -10f, 0f)]
        [InlineData(1.25f, 10f, 10f)]
        public void RunsClockwiseRoundTheEdge(float progress, float x, float y)
        {
            QuickCastShimmer.Point(progress, 10f, out float px, out float py);
            Assert.Equal(x, px, 3);
            Assert.Equal(y, py, 3);
        }

        [Fact]
        public void TwoSparksOppositeAndTrailsBehind()
        {
            float a = QuickCastShimmer.Progress(1.3f, 0, 0);
            float b = QuickCastShimmer.Progress(1.3f, 1, 0);
            float d = b - a;
            Assert.Equal(0.5f, d < 0 ? d + 1f : d, 3);
            float tail = QuickCastShimmer.Progress(1.3f, 0, 3);
            float back = a - tail;
            Assert.Equal(3 * QuickCastShimmer.TrailStep, back < 0 ? back + 1f : back, 3);
            Assert.InRange(QuickCastShimmer.Progress(-5.7f, 0, 7), 0f, 1f);
        }

        [Fact]
        public void TrailFades()
        {
            Assert.Equal(1f, QuickCastShimmer.Alpha(0));
            Assert.True(QuickCastShimmer.Alpha(3) < QuickCastShimmer.Alpha(2));
            Assert.Equal(0f, QuickCastShimmer.Alpha(QuickCastShimmer.Trail));
            Assert.True(QuickCastShimmer.Size(7) < QuickCastShimmer.Size(0));
        }
    }
}
