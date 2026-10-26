using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ShardRulesTests
    {
        private static readonly ShardRules.Settings Defaults = new ShardRules.Settings
        {
            OneStarChance = 0.02f,
            TwoStarChance = 0.05f,
            BossMin = 1,
            BossMax = 3,
            BossFirstKillOnly = true
        };

        private static int Roll(bool boss, int level, bool defeated, double random, ShardRules.Settings s = null)
        {
            return ShardRules.ShardsFor(boss, level, defeated, s ?? Defaults, () => random);
        }

        [Fact]
        public void NoStarCreaturesNeverDrop()
        {
            Assert.Equal(0, Roll(false, 1, false, 0.0));
        }

        [Fact]
        public void OneStarUsesOneStarChance()
        {
            Assert.Equal(1, Roll(false, 2, false, 0.019));
            Assert.Equal(0, Roll(false, 2, false, 0.021));
        }

        [Fact]
        public void TwoStarUsesTwoStarChance()
        {
            Assert.Equal(1, Roll(false, 3, false, 0.049));
            Assert.Equal(0, Roll(false, 3, false, 0.051));
        }

        [Fact]
        public void HigherLevelsCountAsTwoStar()
        {
            Assert.Equal(1, Roll(false, 4, false, 0.049));
        }

        [Fact]
        public void BossFirstKillDropsWithinRange()
        {
            Assert.Equal(1, Roll(true, 1, false, 0.0));
            Assert.Equal(2, Roll(true, 1, false, 0.5));
            Assert.Equal(3, Roll(true, 1, false, 0.9999));
        }

        [Fact]
        public void BossRepeatKillDropsNothingByDefault()
        {
            Assert.Equal(0, Roll(true, 1, true, 0.5));
        }

        [Fact]
        public void BossRepeatKillDropsWhenFirstKillOnlyIsOff()
        {
            var everyKill = new ShardRules.Settings { BossMin = 1, BossMax = 3, BossFirstKillOnly = false };
            Assert.Equal(2, Roll(true, 1, true, 0.5, everyKill));
        }

        [Fact]
        public void MaxBelowMinIsTreatedAsMin()
        {
            var odd = new ShardRules.Settings { BossMin = 4, BossMax = 2, BossFirstKillOnly = true };
            Assert.Equal(4, Roll(true, 1, false, 0.9, odd));
        }
    }
}
