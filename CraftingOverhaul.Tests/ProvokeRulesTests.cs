using System.Collections.Generic;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     <see cref="ProvokeRules" /> as <c>TalismanRepel</c> uses it: the creature's owner writes each player's time on
    ///     the creature's ZDO (modelled here as one dictionary per creature, which every game shares), and whichever game
    ///     owns the creature reads it against the shared world time.
    /// </summary>
    public class ProvokeRulesTests
    {
        private const long Alice = 1001;
        private const long Bob = 2002;
        private const float Seconds = 30f;

        private static void Hit(Dictionary<string, long> creature, long player, double now) =>
            creature[ProvokeRules.Key(player)] = ProvokeRules.Until(now, Seconds);

        private static bool FightsBack(Dictionary<string, long> creature, long player, double now) =>
            ProvokeRules.IsProvoked(creature.TryGetValue(ProvokeRules.Key(player), out long until) ? until : 0L, now);

        [Fact]
        public void LastsTheSettingAfterTheHit()
        {
            var wolf = new Dictionary<string, long>();
            Assert.False(FightsBack(wolf, Alice, 1000d));
            Hit(wolf, Alice, 1000d);
            Assert.True(FightsBack(wolf, Alice, 1000d));
            Assert.True(FightsBack(wolf, Alice, 1029.9d));
            Assert.False(FightsBack(wolf, Alice, 1030d));
        }

        [Fact]
        public void AnOwnerChangeKeepsTheRemainingTime()
        {
            // The owner hits at 1000 s and logs out at 1010 s; the new owner reads the same ZDO at 1020 s: still 10 s left.
            var wolf = new Dictionary<string, long>();
            Hit(wolf, Alice, 1000d);
            var newOwnersCopy = new Dictionary<string, long>(wolf);
            Assert.True(FightsBack(newOwnersCopy, Alice, 1020d));
            Assert.True(FightsBack(newOwnersCopy, Alice, 1029.5d));
            Assert.False(FightsBack(newOwnersCopy, Alice, 1030.5d));
        }

        [Fact]
        public void ALaterHitRefreshesIt()
        {
            var wolf = new Dictionary<string, long>();
            Hit(wolf, Alice, 1000d);
            Hit(wolf, Alice, 1025d);
            Assert.True(FightsBack(wolf, Alice, 1050d));
            Assert.False(FightsBack(wolf, Alice, 1055d));
        }

        [Fact]
        public void OnlyThePlayerWhoHitAndOnlyThatCreature()
        {
            var wolf = new Dictionary<string, long>();
            var otherWolf = new Dictionary<string, long>();
            Hit(wolf, Alice, 1000d);
            Assert.False(FightsBack(wolf, Bob, 1001d));
            Assert.False(FightsBack(otherWolf, Alice, 1001d));
            Assert.NotEqual(ProvokeRules.Key(Alice), ProvokeRules.Key(Bob));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-5f)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void AnUnusableSettingProvokesNothing(float seconds)
        {
            Assert.Equal(0L, ProvokeRules.Until(1000d, seconds));
            Assert.False(ProvokeRules.IsProvoked(ProvokeRules.Until(1000d, seconds), 1000d));
        }

        [Fact]
        public void UnusableTimesAreNeverProvoked()
        {
            Assert.Equal(0L, ProvokeRules.ToMs(double.NaN));
            Assert.Equal(0L, ProvokeRules.ToMs(double.PositiveInfinity));
            Assert.Equal(0L, ProvokeRules.ToMs(-1d));
            Assert.Equal(long.MaxValue, ProvokeRules.ToMs(1e300));
            Assert.False(ProvokeRules.IsProvoked(0L, 1000d));
            Assert.False(ProvokeRules.IsProvoked(-1L, 1000d));
            // World time far into a long-running world still has millisecond steps.
            Assert.True(ProvokeRules.IsProvoked(ProvokeRules.Until(5_000_000d, Seconds), 5_000_029.9d));
        }
    }
}
