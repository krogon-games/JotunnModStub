using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class TabRulesTests
    {
        private static string Data(string file)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", file)))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir, "Corruption", "Data", file));
        }

        [Fact]
        public void EveryRealMapGoesToTheMapsTab()
        {
            var errors = new List<string>();
            var maps = MapList.Parse(Data("maps.csv"), errors);
            Assert.Empty(errors);
            Assert.NotEmpty(maps);
            Assert.All(maps, m => Assert.Equal(InventoryTab.Maps, TabRules.TabFor(m.Item)));
        }

        [Fact]
        public void TheShardAndEveryRealEssenceGoToTheCurrencyTab()
        {
            var errors = new List<string>();
            var essences = EssenceTable.Parse(Data("essences.csv"), errors);
            Assert.Empty(errors);
            Assert.NotEmpty(essences);
            Assert.All(essences, e => Assert.Equal(InventoryTab.Currency, TabRules.TabFor(e.Item)));
            Assert.Equal(InventoryTab.Currency, TabRules.TabFor(ShardRules.ShardPrefab));
        }

        [Theory]
        [InlineData("Wood")]
        [InlineData("CorruptionTalisman_Greydwarf")]
        [InlineData("CorruptedWishbone")]
        [InlineData("piece_riftforge")]
        [InlineData("CorruptionShardX")]
        [InlineData("")]
        [InlineData(null)]
        public void EverythingElseStaysInTheBag(string prefab)
        {
            Assert.Equal(InventoryTab.None, TabRules.TabFor(prefab));
        }

        [Theory]
        [InlineData(5, 10, 5)]
        [InlineData(5, 3, 3)]
        [InlineData(5, 0, 0)]
        [InlineData(0, 10, 0)]
        [InlineData(-1, 10, 0)]
        public void TabsPayFirstButNeverMoreThanTheyHoldOrIsOwed(int amount, int inTabs, int expected)
        {
            Assert.Equal(expected, TabRules.TakeFromTabs(amount, inTabs));
        }

        [Fact]
        public void GraveRowsGoUnderTheInventoryWithoutOverlap()
        {
            // An 8-wide bag of 7 rows (4 + EquipmentAndQuickSlots' 3): tab items start on row 7.
            var seen = new HashSet<string>();
            int lowest = 0;
            for (int i = 0; i < 2 * TabRules.Width * TabRules.Height; i++)
            {
                TabRules.GravePosition(i, 8, 7, out int x, out int y);
                Assert.InRange(x, 0, 7);
                Assert.True(y >= 7);
                Assert.True(seen.Add(x + "," + y));
                lowest = Math.Max(lowest, y);
            }
            // Both full tabs (64 items) take 8 extra rows.
            Assert.Equal(14, lowest);
            Assert.Equal(15, TabRules.GraveHeight(7, lowest));
            Assert.Equal(7, TabRules.GraveHeight(7, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => TabRules.GravePosition(0, 0, 0, out _, out _));
        }
    }
}
