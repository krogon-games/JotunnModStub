using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class FixedLayoutTests
    {
        private static readonly List<string> Homes = new List<string> { "shard", "transmutation", "augmentation", "alchemy", "regal" };

        private static int[] Arrange(string[] keys, int[] cells, int[] stacks = null, bool[] pinned = null, int width = 8, int height = 4)
        {
            stacks = stacks ?? keys.Select(_ => 1).ToArray();
            return FixedLayout.Arrange(width, height, Homes, keys, cells, stacks, pinned);
        }

        [Fact]
        public void EachMaterialGoesToItsHomeCell()
        {
            var result = Arrange(new[] { "regal", "shard", "alchemy" }, new[] { 10, 11, 12 });
            Assert.Equal(new[] { 4, 0, 3 }, result);
        }

        [Fact]
        public void ItemsAlreadyHomeDoNotMove()
        {
            var result = Arrange(new[] { "shard", "regal" }, new[] { 0, 4 });
            Assert.Equal(new[] { 0, 4 }, result);
        }

        [Fact]
        public void ExtraStacksGoToCellsNoMaterialOwns()
        {
            // Two regal stacks: the bigger goes home, the other to the first unowned cell (5).
            var result = Arrange(new[] { "regal", "regal" }, new[] { 0, 1 }, new[] { 10, 50 });
            // Cell 0 is the shard's home: an extra stack doesn't hide it while unowned cells are free.
            Assert.Equal(5, result[0]);
            Assert.Equal(4, result[1]);
        }

        [Fact]
        public void AStackAlreadyInItsUnownedCellStays()
        {
            var result = Arrange(new[] { "regal", "regal" }, new[] { 4, 20 }, new[] { 50, 50 });
            Assert.Equal(new[] { 4, 20 }, result);
        }

        [Fact]
        public void ItemsWithoutAHomeStayWhereTheyAreUnlessThatIsSomeonesHome()
        {
            // An old essence sitting on the shard's home cell moves out when there's a shard; a map at 9 stays.
            var result = Arrange(new[] { "essence", "shard", "map" }, new[] { 0, 7, 9 });
            Assert.Equal(0, result[1]);
            Assert.Equal(5, result[0]);
            Assert.Equal(9, result[2]);
        }

        [Fact]
        public void AnEmptyHomeCellCanHoldOverflowSoNoCapacityIsLost()
        {
            // A tiny 2 x 3 tab (6 cells, 5 homes): six stacks of one material still fit.
            var keys = Enumerable.Repeat("regal", 6).ToArray();
            var cells = Enumerable.Range(0, 6).ToArray();
            var result = Arrange(keys, cells, Enumerable.Repeat(50, 6).ToArray(), null, 3, 2);
            Assert.Equal(6, result.Distinct().Count());
            Assert.All(result, c => Assert.InRange(c, 0, 5));
            Assert.Contains(4, result);
        }

        [Fact]
        public void AFullTabOfMixedItemsKeepsEveryItemInItsOwnCell()
        {
            var keys = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                keys.Add(i % 3 == 0 ? "regal" : i % 3 == 1 ? "shard" : "unknown" + i);
            }
            var cells = Enumerable.Range(0, 32).Reverse().ToArray();
            var result = Arrange(keys.ToArray(), cells);
            Assert.Equal(32, result.Distinct().Count());
            Assert.All(result, c => Assert.InRange(c, 0, 31));
            Assert.Equal("shard", keys[System.Array.IndexOf(result, 0)]);
            Assert.Equal("regal", keys[System.Array.IndexOf(result, 4)]);
        }

        [Fact]
        public void APinnedItemIsNeverMovedAndItsCellIsRespected()
        {
            // A dragged alchemy orb sits on regal's home; the regal orb waits elsewhere instead of overlapping.
            var result = Arrange(new[] { "alchemy", "regal" }, new[] { 4, 10 }, null, new[] { true, false });
            Assert.Equal(4, result[0]);
            Assert.Equal(10, result[1]);
        }

        [Fact]
        public void ArrangingTwiceChangesNothingTheSecondTime()
        {
            var keys = new[] { "regal", "regal", "shard", "map", "alchemy" };
            var first = Arrange(keys, new[] { 30, 31, 29, 0, 3 }, new[] { 5, 9, 1, 1, 1 });
            var second = Arrange(keys, first, new[] { 5, 9, 1, 1, 1 });
            Assert.Equal(first, second);
        }

        [Fact]
        public void HomesPastTheEndOfTheTabAreIgnored()
        {
            Assert.Equal(-1, FixedLayout.HomeOf(Homes, "regal", 3));
            Assert.Equal(4, FixedLayout.HomeOf(Homes, "regal", 32));
            Assert.Equal(-1, FixedLayout.HomeOf(Homes, "nothing", 32));
            Assert.Equal(-1, FixedLayout.HomeOf(null, "regal", 32));
        }
    }
}
