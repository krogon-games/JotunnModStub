using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>Personal loot rules: the 60 second lock, who sees and picks up a drop, the sound, and dealing the shared rip loot.</summary>
    public class PersonalLootRulesTests
    {
        private const long T = PersonalLootRules.TicksPerSecond;
        private const long Drop = 1000 * T;
        private const double Lock = 60;
        private const long Me = 11, Other = 22;

        private static LootLockState State(long owner, long drop, double secondsLater, long viewer) =>
            PersonalLootRules.StateFor(owner, drop, Drop + (long)(secondsLater * T), viewer, Lock);

        [Fact]
        public void NoOwnerIsFree() => Assert.Equal(LootLockState.Free, State(0, Drop, 1, Me));

        [Fact]
        public void TheOwnerSeesItAsMineWhileLocked() => Assert.Equal(LootLockState.Mine, State(Me, Drop, 10, Me));

        [Fact]
        public void SomeoneElseSeesItAsTheirsWhileLocked() => Assert.Equal(LootLockState.Theirs, State(Me, Drop, 10, Other));

        [Fact]
        public void ItIsFreeExactlyAtTheEndOfTheLock() => Assert.Equal(LootLockState.Free, State(Me, Drop, 60, Other));

        [Fact]
        public void ItIsStillLockedJustBeforeTheEnd() => Assert.Equal(LootLockState.Theirs, State(Me, Drop, 59.9, Other));

        [Fact]
        public void ItIsFreeAfterTheLock() => Assert.Equal(LootLockState.Free, State(Me, Drop, 300, Other));

        [Fact]
        public void AMissingDropTimeNeverLocksForever()
        {
            Assert.Equal(LootLockState.Free, State(Me, 0, 1, Other));
            Assert.Equal(LootLockState.Free, State(Me, -5, 1, Other));
        }

        [Fact]
        public void AClockThatSteppedBackStillCountsAsLocked()
        {
            Assert.Equal(LootLockState.Theirs, State(Me, Drop, -2, Other));
            Assert.Equal(LootLockState.Mine, State(Me, Drop, -2, Me));
        }

        [Fact]
        public void AnUnknownViewerSeesALockedDropAsTheirs()
        {
            Assert.Equal(LootLockState.Theirs, State(Me, Drop, 10, 0));
            Assert.Equal(LootLockState.Free, State(Me, Drop, 70, 0));
        }

        [Fact]
        public void SecondsLeftIsClamped()
        {
            Assert.Equal(60, PersonalLootRules.SecondsLeft(Drop, Drop - 2 * T, Lock));
            Assert.Equal(60, PersonalLootRules.SecondsLeft(Drop, Drop, Lock));
            Assert.Equal(40, PersonalLootRules.SecondsLeft(Drop, Drop + 20 * T, Lock), 6);
            Assert.Equal(0, PersonalLootRules.SecondsLeft(Drop, Drop + 500 * T, Lock));
        }

        [Fact]
        public void AgeIsNeverNegative()
        {
            Assert.Equal(0, PersonalLootRules.AgeSeconds(Drop, Drop - 2 * T));
            Assert.Equal(5, PersonalLootRules.AgeSeconds(Drop, Drop + 5 * T), 6);
        }

        [Theory]
        [InlineData(30, 30)]
        [InlineData(0, 0)]
        [InlineData(60, 60)]
        [InlineData(-10, 0)]
        [InlineData(100, 60)]
        public void BackdatedTicksLeaveTheWantedSecondsLeft(double wanted, double expected)
        {
            long now = 5000 * T;
            long drop = PersonalLootRules.BackdatedTicks(now, Lock, wanted);
            Assert.Equal(expected, PersonalLootRules.SecondsLeft(drop, now, Lock), 6);
        }

        [Theory]
        [InlineData(LootLockState.Free, true)]
        [InlineData(LootLockState.Mine, true)]
        [InlineData(LootLockState.Theirs, false)]
        public void PickUpAndVisibleOnlyHideTheirs(LootLockState state, bool allowed)
        {
            Assert.Equal(allowed, PersonalLootRules.CanPickUp(state));
            Assert.Equal(allowed, PersonalLootRules.Visible(state));
        }

        [Theory]
        [InlineData(LootLockState.Mine, 0.0, true, true)]
        [InlineData(LootLockState.Mine, 5.0, true, true)]
        [InlineData(LootLockState.Mine, 5.1, true, false)]
        [InlineData(LootLockState.Mine, 1.0, false, false)]
        [InlineData(LootLockState.Free, 1.0, true, false)]
        [InlineData(LootLockState.Theirs, 1.0, true, false)]
        public void TheSoundPlaysForTheOwnerOnlyWithinTheWindowAndOnlyWithASound(LootLockState state, double age, bool hasSound, bool expected) =>
            Assert.Equal(expected, PersonalLootRules.PlaySound(state, age, 5, hasSound));

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        public void AutoStackNeverMergesTaggedStacks(bool a, bool b, bool expected) =>
            Assert.Equal(expected, PersonalLootRules.MayAutoStack(a, b));

        // A fake random source: always picks the last candidate (no real shuffling), or follows a script.
        private static Func<int, int> Seeded(int seed)
        {
            var rng = new Random(seed);
            return n => rng.Next(n);
        }

        private static int Total(int[][] result, int player) => result.Sum(kind => kind[player]);

        [Fact]
        public void TenCoinsForThreePlayersGoFourThreeThree()
        {
            var result = PersonalLootRules.Deal(3, new[] { 10 }, Seeded(1));
            Assert.Equal(new[] { 3, 3, 4 }, result[0].OrderBy(x => x));
        }

        [Fact]
        public void TheTurnCarriesOnBetweenKinds()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var three = PersonalLootRules.Deal(3, new[] { 1, 1, 1 }, Seeded(seed));
                for (int p = 0; p < 3; p++)
                {
                    Assert.Equal(1, Total(three, p));
                }
                // 10 coins then 1 item: the item goes to a player who got 3 coins, not the one who got 4.
                var coins = PersonalLootRules.Deal(3, new[] { 10, 1 }, Seeded(seed));
                int winner = Array.IndexOf(coins[1], 1);
                Assert.Equal(3, coins[0][winner]);
            }
        }

        [Fact]
        public void LeftoversRotateBetweenKinds()
        {
            // 1 unit of each of 3 kinds, 3 players: everyone gets exactly one, in different kinds.
            var result = PersonalLootRules.Deal(3, new[] { 1, 1, 1 }, Seeded(7));
            Assert.Equal(3, Enumerable.Range(0, 3).Select(k => Array.IndexOf(result[k], 1)).Distinct().Count());
        }

        [Fact]
        public void NothingIsLostOrInventedAndTheSpreadIsAtMostOne()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var rng = new Random(seed);
                int players = rng.Next(1, 6);
                var kinds = Enumerable.Range(0, rng.Next(0, 6)).Select(_ => rng.Next(0, 15)).ToArray();
                var result = PersonalLootRules.Deal(players, kinds, Seeded(seed));
                Assert.Equal(kinds.Length, result.Length);
                for (int k = 0; k < kinds.Length; k++)
                {
                    Assert.Equal(players, result[k].Length);
                    Assert.Equal(kinds[k], result[k].Sum());
                    Assert.True(result[k].Max() - result[k].Min() <= 1 || kinds[k] < 0);
                }
                var totals = Enumerable.Range(0, players).Select(p => Total(result, p)).ToArray();
                Assert.True(totals.Max() - totals.Min() <= 1);
            }
        }

        [Fact]
        public void OnePlayerGetsEverything()
        {
            var result = PersonalLootRules.Deal(1, new[] { 7, 2 }, Seeded(3));
            Assert.Equal(7, result[0][0]);
            Assert.Equal(2, result[1][0]);
        }

        [Fact]
        public void NoPlayersGivesSeatZeroEverything()
        {
            foreach (int players in new[] { 0, -1 })
            {
                var result = PersonalLootRules.Deal(players, new[] { 4, 0, 1 }, Seeded(3));
                Assert.Equal(new[] { 4 }, result[0]);
                Assert.Equal(new[] { 0 }, result[1]);
                Assert.Equal(new[] { 1 }, result[2]);
            }
        }

        [Fact]
        public void KindsWithNoUnitsGiveAllZeroRows()
        {
            var result = PersonalLootRules.Deal(3, new[] { 0, 2, 0 }, Seeded(2));
            Assert.All(result[0], x => Assert.Equal(0, x));
            Assert.All(result[2], x => Assert.Equal(0, x));
            Assert.Equal(2, result[1].Sum());
        }

        [Fact]
        public void TheRandomOrderIsActuallyUsed()
        {
            // randomBelow always 0: Fisher-Yates rotates the seats one way; always n-1 leaves them as they were.
            var asIs = PersonalLootRules.Deal(3, new[] { 1 }, n => n - 1);
            var shuffled = PersonalLootRules.Deal(3, new[] { 1 }, n => 0);
            Assert.NotEqual(Array.IndexOf(asIs[0], 1), Array.IndexOf(shuffled[0], 1));
        }

        [Fact]
        public void EverySeatCanComeFirst()
        {
            var first = new HashSet<int>();
            for (int seed = 0; seed < 100; seed++)
            {
                first.Add(Array.IndexOf(PersonalLootRules.Deal(3, new[] { 1 }, Seeded(seed))[0], 1));
            }
            Assert.Equal(3, first.Count);
        }

        [Fact]
        public void PulseFactorCyclesBetweenOneAndOneMinusStrength()
        {
            // Krogon, 2026-11-26: "would look better if the beam pulsated very slowly".
            Assert.Equal(1f, PersonalLootRules.PulseFactor(0, 4, 0.35, 0), 4);
            Assert.Equal(0.65f, PersonalLootRules.PulseFactor(2, 4, 0.35, 0), 4);
            Assert.Equal(1f, PersonalLootRules.PulseFactor(4, 4, 0.35, 0), 4);
            Assert.Equal(1f, PersonalLootRules.PulseFactor(40, 4, 0.35, 0), 4);
        }

        [Fact]
        public void PulseFactorPhaseShiftsTheCycle()
        {
            Assert.Equal(0.65f, PersonalLootRules.PulseFactor(0, 4, 0.35, 0.5), 4);
            Assert.Equal(PersonalLootRules.PulseFactor(1, 4, 0.35, 0), PersonalLootRules.PulseFactor(0, 4, 0.35, 0.25), 4);
        }

        [Fact]
        public void PulseFactorStaysInRangeAndSteadyWithoutStrength()
        {
            for (double t = 0; t < 20; t += 0.37)
            {
                float f = PersonalLootRules.PulseFactor(t, 4, 0.35, 0.3);
                Assert.InRange(f, 0.65f - 1e-5f, 1f + 1e-5f);
                Assert.Equal(1f, PersonalLootRules.PulseFactor(t, 4, 0, 0.3));
                Assert.Equal(1f, PersonalLootRules.PulseFactor(t, 0, 0.35, 0.3));
            }
        }
    }
}
