using CraftingOverhaul.Classes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class SkillsPanelScaleTests
    {
        [Fact]
        public void FillsMostOfTheScreenByDefault()
        {
            float h = SkillsPanelLayout.Height, w = SkillsPanelLayout.Width;
            float s = SkillsPanelLayout.Scale(1920f, 1080f, 0f);
            // As large as fits: one side reaches its share, neither goes past it.
            Assert.True(h * s <= 1080f * SkillsPanelLayout.FitHeight + 0.01f);
            Assert.True(w * s <= 1920f * SkillsPanelLayout.FitWidth + 0.01f);
            Assert.True(System.Math.Abs(h * s - 1080f * SkillsPanelLayout.FitHeight) < 0.01f || System.Math.Abs(w * s - 1920f * SkillsPanelLayout.FitWidth) < 0.01f || s == 1f);
        }

        [Fact]
        public void NeverSmallerThanTheOriginalNorHuge()
        {
            Assert.Equal(1f, SkillsPanelLayout.Scale(400f, 300f, 0f));
            Assert.Equal(3f, SkillsPanelLayout.Scale(100000f, 100000f, 0f));
            Assert.Equal(1f, SkillsPanelLayout.Scale(0f, 0f, 0f));
        }

        [Fact]
        public void ASetSizeWins()
        {
            Assert.Equal(1.5f, SkillsPanelLayout.Scale(1920f, 1080f, 1.5f));
        }
    }
}
