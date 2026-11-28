using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>Krogon's decisions 20-22 of 2026-11-28: corrupted creatures fight players and tames only.</summary>
    public class CorruptedTargetingTests
    {
        private static readonly CorruptedTargeting.Side Corrupt = new CorruptedTargeting.Side { Corrupted = true };
        private static readonly CorruptedTargeting.Side Normal = new CorruptedTargeting.Side();
        private static readonly CorruptedTargeting.Side Player = new CorruptedTargeting.Side { Player = true };
        private static readonly CorruptedTargeting.Side Tame = new CorruptedTargeting.Side { Tamed = true };

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CorruptedNeverFightEachOtherOrNormalCreatures(bool vanilla)
        {
            Assert.False(CorruptedTargeting.IsEnemy(Corrupt, Corrupt, vanilla));
            Assert.False(CorruptedTargeting.IsEnemy(Corrupt, Normal, vanilla));
            Assert.False(CorruptedTargeting.IsEnemy(Normal, Corrupt, vanilla));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CorruptedFightPlayersAndTamesBothWays(bool vanilla)
        {
            Assert.True(CorruptedTargeting.IsEnemy(Corrupt, Player, vanilla));   // a talisman's "false" doesn't count
            Assert.True(CorruptedTargeting.IsEnemy(Player, Corrupt, vanilla));
            Assert.True(CorruptedTargeting.IsEnemy(Corrupt, Tame, vanilla));
            Assert.True(CorruptedTargeting.IsEnemy(Tame, Corrupt, vanilla));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OtherwiseTheGameDecides(bool vanilla)
        {
            Assert.Equal(vanilla, CorruptedTargeting.IsEnemy(Normal, Player, vanilla));
            Assert.Equal(vanilla, CorruptedTargeting.IsEnemy(Normal, Normal, vanilla));
            Assert.Equal(vanilla, CorruptedTargeting.IsEnemy(Tame, Normal, vanilla));
        }
    }
}
