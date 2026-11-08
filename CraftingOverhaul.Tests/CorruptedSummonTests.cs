using System;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     <see cref="SummonMarks{TAltar}" /> driven like <c>CorruptedOfferings</c>: the altar's owner starts a corrupted
    ///     summon through the game's own steps (1.0.14 <c>OfferingBowl.RPC_SpawnBoss</c>: refused while
    ///     <c>IsBossSpawnQueued</c>; <c>CanSpawnBoss</c>; <c>SpawnBoss</c> queues <c>DelayedSpawnBoss</c>; the offering is
    ///     taken), and the boss appearing (<c>DelayedSpawnBoss</c>) takes the mark. A normal or wrong item during the delay
    ///     goes to the game, which does nothing while a boss is queued, and never touches the marks.
    /// </summary>
    public class CorruptedSummonTests
    {
        /// <summary>One altar as its owner's game sees it.</summary>
        private sealed class Altar
        {
            public bool Queued;
            public bool HasSpawnPoint = true;
            public int CorruptedTaken;
            public int NormalTaken;
            public int Bosses;
            public int CorruptedBosses;
        }

        private sealed class Owner
        {
            public readonly SummonMarks<Altar> Marks = new SummonMarks<Altar>();

            /// <summary>Any player offers a corrupted item: the request reaches the owner.</summary>
            public bool OfferCorrupted(Altar altar)
            {
                return Marks.TryStart(altar, altar.Queued, () =>
                {
                    if (!altar.HasSpawnPoint)
                    {
                        return false;
                    }
                    altar.Queued = true;
                    altar.CorruptedTaken++;
                    return altar.Queued;
                });
            }

            /// <summary>Any player offers the normal item (or a wrong one): the game's own summon, no marks involved.</summary>
            public void OfferNormal(Altar altar, bool wrongItem = false)
            {
                if (altar.Queued || wrongItem || !altar.HasSpawnPoint)
                {
                    return;
                }
                altar.Queued = true;
                altar.NormalTaken++;
            }

            /// <summary>The spawn delay ends.</summary>
            public void BossAppears(Altar altar)
            {
                if (!altar.Queued)
                {
                    return;
                }
                altar.Queued = false;
                altar.Bosses++;
                if (Marks.Take(altar))
                {
                    altar.CorruptedBosses++;
                }
            }
        }

        [Fact]
        public void NormalAndWrongItemsDuringTheDelayKeepTheSummonCorrupted()
        {
            var owner = new Owner();
            var altar = new Altar();
            Assert.True(owner.OfferCorrupted(altar));
            owner.OfferNormal(altar);
            owner.OfferNormal(altar, wrongItem: true);
            owner.OfferNormal(altar);
            owner.BossAppears(altar);
            Assert.Equal(1, altar.Bosses);
            Assert.Equal(1, altar.CorruptedBosses);
            Assert.Equal(1, altar.CorruptedTaken);
            Assert.Equal(0, altar.NormalTaken);
            Assert.Equal(0, owner.Marks.Count);
        }

        [Fact]
        public void RepeatedCorruptedOfferingsDuringTheDelayStartNothingAndCostNothing()
        {
            var owner = new Owner();
            var altar = new Altar();
            Assert.True(owner.OfferCorrupted(altar));
            Assert.False(owner.OfferCorrupted(altar));   // the same player again
            Assert.False(owner.OfferCorrupted(altar));   // another player
            owner.BossAppears(altar);
            owner.BossAppears(altar);                    // nothing more was queued
            Assert.Equal(1, altar.Bosses);
            Assert.Equal(1, altar.CorruptedBosses);
            Assert.Equal(1, altar.CorruptedTaken);
        }

        [Fact]
        public void NormalSummonsStayNormal()
        {
            var owner = new Owner();
            var altar = new Altar();
            owner.OfferNormal(altar);
            Assert.False(owner.OfferCorrupted(altar));   // corrupted item during a normal summon's delay
            owner.BossAppears(altar);
            Assert.Equal(1, altar.Bosses);
            Assert.Equal(0, altar.CorruptedBosses);
            Assert.Equal(0, altar.CorruptedTaken);
            Assert.Equal(1, altar.NormalTaken);

            // A corrupted summon, then a normal one: only the first is corrupted.
            Assert.True(owner.OfferCorrupted(altar));
            owner.BossAppears(altar);
            owner.OfferNormal(altar);
            owner.BossAppears(altar);
            Assert.Equal(3, altar.Bosses);
            Assert.Equal(1, altar.CorruptedBosses);
        }

        [Fact]
        public void NoSpawnPointLeavesNoMarkAndTakesNothing()
        {
            var owner = new Owner();
            var altar = new Altar { HasSpawnPoint = false };
            Assert.False(owner.OfferCorrupted(altar));
            Assert.False(owner.Marks.IsMarked(altar));
            Assert.Equal(0, altar.CorruptedTaken);

            altar.HasSpawnPoint = true;
            owner.OfferNormal(altar);
            owner.BossAppears(altar);
            Assert.Equal(1, altar.Bosses);
            Assert.Equal(0, altar.CorruptedBosses);
        }

        [Fact]
        public void AFailingStartLeavesNoMark()
        {
            var marks = new SummonMarks<Altar>();
            var altar = new Altar();
            Assert.Throws<InvalidOperationException>(() => marks.TryStart(altar, false, () => throw new InvalidOperationException()));
            Assert.False(marks.IsMarked(altar));
            Assert.False(marks.TryStart(null, false, () => true));
            Assert.False(marks.Take(null));
        }

        [Fact]
        public void AltarsAreSeparateAndGoneOnesArePruned()
        {
            var owner = new Owner();
            var first = new Altar();
            var second = new Altar();
            Assert.True(owner.OfferCorrupted(first));
            owner.OfferNormal(second);
            owner.BossAppears(second);
            Assert.Equal(0, second.CorruptedBosses);
            Assert.True(owner.Marks.IsMarked(first));

            // The first altar unloads before its boss appears: its mark goes, the boss never comes.
            Assert.Equal(1, owner.Marks.Prune(a => a == first));
            Assert.Equal(0, owner.Marks.Count);
        }
    }
}
