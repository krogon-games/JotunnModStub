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

        private static void Swings(GatheringRules.HitCounter counter, object tool, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                Assert.False(counter.NextSwing(tool));
            }
        }

        [Fact]
        public void VeinbreakerUninterruptedBoostsEveryFifthSwing()
        {
            var counter = new GatheringRules.HitCounter();
            var pick = new object();
            for (int round = 0; round < 3; round++)
            {
                Swings(counter, pick, 4);
                Assert.True(counter.NextSwing(pick));
            }
        }

        [Fact]
        public void VeinbreakerUnequipAndReequipNeedsFiveFreshSwings()
        {
            var counter = new GatheringRules.HitCounter();
            var pick = new object();
            Swings(counter, pick, 4);
            counter.Unequipped(pick);
            // Re-equipped: the same tool starts from zero.
            Swings(counter, pick, 4);
            Assert.True(counter.NextSwing(pick));
        }

        [Fact]
        public void VeinbreakerSwitchingThroughAnUnaffixedToolResets()
        {
            var counter = new GatheringRules.HitCounter();
            var pick = new object();
            var plainAxe = new object();
            Swings(counter, pick, 4);
            // Equipping the axe unequips the pickaxe; the axe has no Veinbreaker's, so its swings aren't counted.
            counter.Unequipped(pick);
            counter.Unequipped(plainAxe);
            Swings(counter, pick, 4);
            Assert.True(counter.NextSwing(pick));
        }

        [Fact]
        public void VeinbreakerIgnoresOtherItemsLeavingTheHands()
        {
            var counter = new GatheringRules.HitCounter();
            var pick = new object();
            Swings(counter, pick, 4);
            counter.Unequipped(new object());
            counter.Unequipped(null);
            Assert.True(counter.NextSwing(pick));
        }

        private static GatheringRules.AreaBox Box(float minX, float maxX, bool live = true) =>
            new GatheringRules.AreaBox(minX, 0f, 0f, maxX, 1f, 1f, live);

        [Fact]
        public void TouchingAreasUseTheMarginAndSkipBrokenParts()
        {
            var areas = new[]
            {
                Box(0f, 1f), // shares a face with the struck part
                Box(1f, 2f), // struck
                Box(2.04f, 3f), // 4 cm gap: inside the 5 cm margin
                Box(2.2f, 3f), // 20 cm gap: not touching
                Box(1.5f, 2.5f, live: false) // overlaps, but already broken
            };
            Assert.Equal(new[] { 0, 2 }, GatheringRules.TouchingAreas(1, areas, 0.05f));
            Assert.Empty(GatheringRules.TouchingAreas(-1, areas, 0.05f));
            Assert.Empty(GatheringRules.TouchingAreas(9, areas, 0.05f));
            areas[1].Live = false;
            Assert.Empty(GatheringRules.TouchingAreas(1, areas, 0.05f));
        }

        [Fact]
        public void ExtraHitsOdinKillsTheStruckPartFirstThenItsNeighbours()
        {
            var odin = GatheringRules.ExtraHits(1, new[] { 0, 2 }, seismic: true, odin: true);
            Assert.Equal(new[] { 1, 0, 2 }, odin.ConvertAll(h => h.Area));
            Assert.All(odin, h => Assert.True(h.Killing));

            var seismic = GatheringRules.ExtraHits(1, new[] { 0, 2 }, seismic: true, odin: false);
            Assert.Equal(new[] { 0, 2 }, seismic.ConvertAll(h => h.Area));
            Assert.All(seismic, h => Assert.False(h.Killing));

            Assert.Empty(GatheringRules.ExtraHits(1, new[] { 0, 2 }, false, false));
            Assert.Empty(GatheringRules.ExtraHits(-1, new[] { 0, 2 }, true, true));
        }
    }
}
