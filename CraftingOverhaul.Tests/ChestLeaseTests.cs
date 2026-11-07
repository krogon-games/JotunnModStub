using System.Collections.Generic;
using System.Linq;
using CraftingOverhaul.Containers;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Craft-from-chests payment: <see cref="LeaseBook{TKey}" /> (exclusive access) and <see cref="PayPlan" />, and a
    ///     simulation of the game's chest handshake (<c>Container.RPC_RequestOpen</c> as read in the 1.0.14 assemblies:
    ///     only the owner answers; it refuses when the chest is in use by someone else; otherwise it hands the chest
    ///     over, makes the asker the owner and says yes) with two players paying from the same chest at once.
    /// </summary>
    public class ChestLeaseTests
    {
        private sealed class Chest
        {
        }

        // ------------------------------------------------------------ LeaseBook

        [Fact]
        public void Held_OnlyAfterTheYesAndTheOwnershipChange()
        {
            var book = new LeaseBook<Chest>();
            var chest = new Chest();
            Assert.True(book.Request(chest, 0));
            Assert.False(book.Request(chest, 0.1)); // already asked: no second message
            Assert.True(book.OnResponse(chest, true));
            Assert.False(book.Tick(chest, weOwnIt: false, markedInUse: false, now: 0.2));
            Assert.Equal(LeaseState.Granted, book.State(chest));
            Assert.True(book.Tick(chest, weOwnIt: true, markedInUse: false, now: 0.3));
            Assert.True(book.IsHeld(chest));
            Assert.Equal(LeaseOutcome.AllHeld, book.Outcome(new[] { chest }));
        }

        [Fact]
        public void Refused_Fails()
        {
            var book = new LeaseBook<Chest>();
            var chest = new Chest();
            book.Request(chest, 0);
            book.OnResponse(chest, false);
            Assert.Equal(LeaseOutcome.Failed, book.Outcome(new[] { chest }));
            Assert.False(book.Release(chest)); // nothing to clear: it was never ours
        }

        [Fact]
        public void NoAnswer_OrNoOwnershipChange_TimesOut()
        {
            var book = new LeaseBook<Chest>();
            var silent = new Chest();
            var stuck = new Chest();
            book.Request(silent, 0);
            book.Request(stuck, 0);
            book.OnResponse(stuck, true);
            book.Tick(silent, false, false, LeaseBook<Chest>.TimeoutSeconds - 0.1);
            Assert.Equal(LeaseState.Pending, book.State(silent));
            book.Tick(silent, false, false, LeaseBook<Chest>.TimeoutSeconds + 0.1);
            book.Tick(stuck, false, false, LeaseBook<Chest>.TimeoutSeconds + 0.1);
            Assert.Equal(LeaseState.Failed, book.State(silent));
            Assert.Equal(LeaseState.Failed, book.State(stuck));
        }

        [Fact]
        public void LateAnswer_AfterTheTimeout_ChangesNothing()
        {
            var book = new LeaseBook<Chest>();
            var chest = new Chest();
            book.Request(chest, 0);
            book.Tick(chest, false, false, 10);
            Assert.False(book.OnResponse(chest, true));
            Assert.Equal(LeaseState.Failed, book.State(chest));
        }

        [Fact]
        public void HeldThenLost_Fails()
        {
            var book = new LeaseBook<Chest>();
            var chest = new Chest();
            book.Request(chest, 0);
            book.OnResponse(chest, true);
            book.Tick(chest, true, false, 0.1);
            book.Tick(chest, true, true, 0.2);
            Assert.True(book.IsHeld(chest));
            book.Tick(chest, weOwnIt: false, markedInUse: true, now: 0.3);
            Assert.Equal(LeaseState.Failed, book.State(chest));
        }

        [Fact]
        public void OneOfTwoChestsRefused_TheWholePaymentFails()
        {
            var book = new LeaseBook<Chest>();
            var a = new Chest();
            var b = new Chest();
            book.Request(a, 0);
            book.Request(b, 0);
            book.OnResponse(a, true);
            book.Tick(a, true, false, 0.1);
            Assert.Equal(LeaseOutcome.Waiting, book.Outcome(new[] { a, b }));
            book.OnResponse(b, false);
            Assert.Equal(LeaseOutcome.Failed, book.Outcome(new[] { a, b }));
            Assert.True(book.Release(a)); // a was held: its in-use mark must be cleared
        }

        // ------------------------------------------------------------ PayPlan

        [Fact]
        public void NothingShort_NoChests()
        {
            var chests = PayPlan.ChestsNeeded(new[] { 5 }, new[] { 5 }, new List<int[]> { new[] { 10 } }, false, out bool enough);
            Assert.Empty(chests);
            Assert.True(enough);
        }

        [Fact]
        public void Shortfall_FromTheClosestChestsOnly()
        {
            var counts = new List<int[]> { new[] { 0, 2 }, new[] { 3, 0 }, new[] { 9, 9 } };
            // Needs 5 wood (carry 1) and 2 stone (carry 0): chest 1 gives 3 wood, chest 2 the last wood; chest 0 the stone.
            var chests = PayPlan.ChestsNeeded(new[] { 5, 2 }, new[] { 1, 0 }, counts, false, out bool enough);
            Assert.Equal(new[] { 0, 1, 2 }, chests);
            Assert.True(enough);
            chests = PayPlan.ChestsNeeded(new[] { 3, 0 }, new[] { 1, 0 }, counts, false, out _);
            Assert.Equal(new[] { 1 }, chests);
        }

        [Fact]
        public void Building_EveryChestWithAShortItem()
        {
            var counts = new List<int[]> { new[] { 5 }, new[] { 1 }, new[] { 0 } };
            var chests = PayPlan.ChestsNeeded(new[] { 2 }, new[] { 0 }, counts, true, out _);
            Assert.Equal(new[] { 0, 1 }, chests);
        }

        [Fact]
        public void NotEnoughAnywhere_SaysSo()
        {
            PayPlan.ChestsNeeded(new[] { 10 }, new[] { 2 }, new List<int[]> { new[] { 3 } }, false, out bool enough);
            Assert.False(enough);
        }

        [Fact]
        public void Take_AllOrNothing()
        {
            Assert.Null(PayPlan.Take(5, new[] { 2, 2 }));
            Assert.Equal(new[] { 2, 1 }, PayPlan.Take(3, new[] { 2, 2 }));
            Assert.Equal(new[] { 0, 0 }, PayPlan.Take(0, new[] { 2, 2 }));
        }

        // ------------------------------------------------------------ the handshake, simulated

        /// <summary>A chest as the game's network sees it: one owner, an in-use mark and its stock.</summary>
        private sealed class NetChest
        {
            public long Owner;
            public bool InUse;
            public int Stock;
        }

        /// <summary>A player's game: its own lease book and what it crafted.</summary>
        private sealed class Game
        {
            public readonly long Id;
            public readonly LeaseBook<NetChest> Book = new LeaseBook<NetChest>();
            public int Crafted;

            public Game(long id) => Id = id;

            /// <summary>Asks the chest's owner (as this game believes it to be); the owner runs RPC_RequestOpen.</summary>
            public void Ask(NetChest chest, long believedOwner, double now)
            {
                if (!Book.Request(chest, now))
                {
                    return;
                }
                // RPC_RequestOpen on the believed owner's game.
                if (chest.Owner != believedOwner)
                {
                    return; // "but im not the owner": no answer at all
                }
                if (chest.InUse && Id != chest.Owner)
                {
                    Book.OnResponse(chest, false);
                    return;
                }
                chest.Owner = Id;
                Book.OnResponse(chest, true);
            }

            public void Tick(NetChest chest, double now)
            {
                if (Book.Tick(chest, chest.Owner == Id, chest.InUse && chest.Owner == Id, now))
                {
                    chest.InUse = true;
                }
            }

            /// <summary>The craft goes ahead only when the chest is held, the game's own check passes, and payment is all or nothing.</summary>
            public bool TryCraft(NetChest chest, int cost)
            {
                if (!Book.IsHeld(chest) || chest.Stock < cost)
                {
                    return false;
                }
                var take = PayPlan.Take(cost, new[] { chest.Stock });
                if (take == null)
                {
                    return false;
                }
                chest.Stock -= take[0];
                Crafted++;
                return true;
            }

            public void Release(NetChest chest)
            {
                if (Book.Release(chest) && chest.Owner == Id)
                {
                    chest.InUse = false;
                }
            }
        }

        [Fact]
        public void TwoPlayersAtOnce_OnlyOnePaysAndTheOtherGetsNothing()
        {
            var chest = new NetChest { Owner = 1, Stock = 3 }; // owned by A's game; enough for one craft of 3
            var a = new Game(1);
            var b = new Game(2);

            // Both press Craft in the same instant, both believing A's game owns the chest.
            a.Ask(chest, believedOwner: 1, now: 0);
            b.Ask(chest, believedOwner: 1, now: 0);
            a.Tick(chest, 0.05);
            b.Tick(chest, 0.05);

            // Exactly one holds it.
            Assert.NotEqual(a.Book.IsHeld(chest), b.Book.IsHeld(chest));
            var winner = a.Book.IsHeld(chest) ? a : b;
            var loser = winner == a ? b : a;

            Assert.True(winner.TryCraft(chest, 3));
            Assert.False(loser.TryCraft(chest, 3));
            loser.Tick(chest, LeaseBook<NetChest>.TimeoutSeconds + 1);
            Assert.Equal(LeaseOutcome.Failed, loser.Book.Outcome(new[] { chest }));

            Assert.Equal(0, chest.Stock);
            Assert.Equal(1, a.Crafted + b.Crafted); // one payment, one item: none free, none paid twice
        }

        [Fact]
        public void SecondPlayerAsksTheNewOwner_RefusedWhileInUse_ThenServedAfterRelease()
        {
            var chest = new NetChest { Owner = 1, Stock = 6 };
            var a = new Game(1);
            var b = new Game(2);
            a.Ask(chest, 1, 0);
            a.Tick(chest, 0.05);
            Assert.True(a.Book.IsHeld(chest));

            b.Ask(chest, believedOwner: 1, now: 0.1);
            Assert.Equal(LeaseState.Failed, b.Book.State(chest));
            Assert.False(b.TryCraft(chest, 3));

            Assert.True(a.TryCraft(chest, 3));
            a.Release(chest);
            Assert.False(chest.InUse);

            b.Release(chest);
            b.Ask(chest, believedOwner: 1, now: 5);
            b.Tick(chest, 5.05);
            Assert.True(b.TryCraft(chest, 3));
            Assert.Equal(0, chest.Stock);
            Assert.Equal(2, a.Crafted + b.Crafted);
        }

        [Fact]
        public void StockChangedBeforeTheChestWasHeld_NoCraftAndNoPartialCharge()
        {
            var chest = new NetChest { Owner = 2, Stock = 5 };
            var a = new Game(1);
            a.Ask(chest, believedOwner: 2, now: 0); // B's game owns it and grants...
            chest.Stock = 2;                          // ...after B took three for itself
            a.Tick(chest, 0.05);
            Assert.True(a.Book.IsHeld(chest));
            Assert.False(a.TryCraft(chest, 3));       // revalidated against the real stock
            Assert.Equal(2, chest.Stock);
            Assert.Equal(0, a.Crafted);
        }
    }
}
