using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class GatheringRulesTests
    {
        [Fact]
        public void RollUsesThePercent()
        {
            Assert.True(GatheringRules.Roll(15f, () => 0.149));
            Assert.False(GatheringRules.Roll(15f, () => 0.15));
            Assert.False(GatheringRules.Roll(0f, () => 0.0));
        }

        [Fact]
        public void PickupRadiusSetsAddsAndCaps()
        {
            Assert.Equal(2f, GatheringRules.PickupRadius(2f, 0f, 0f, 8f));
            Assert.Equal(5f, GatheringRules.PickupRadius(2f, 5f, 0f, 8f));
            Assert.Equal(3.5f, GatheringRules.PickupRadius(2f, 0f, 1.5f, 8f));
            Assert.Equal(7f, GatheringRules.PickupRadius(2f, 5f, 2f, 8f));
            Assert.Equal(8f, GatheringRules.PickupRadius(2f, 6f, 4f, 8f));
        }

        [Fact]
        public void VeinbreakerCountsEveryFifthSwingPerTool()
        {
            var counter = new GatheringRules.HitCounter();
            var pick = new object();
            for (int i = 1; i <= 4; i++)
            {
                Assert.False(counter.NextSwing(pick));
            }
            Assert.True(counter.NextSwing(pick));
            Assert.False(counter.NextSwing(pick));
            // Another tool starts the count again.
            var other = new object();
            for (int i = 1; i <= 4; i++)
            {
                Assert.False(counter.NextSwing(other));
            }
            Assert.True(counter.NextSwing(other));
        }
    }
}
