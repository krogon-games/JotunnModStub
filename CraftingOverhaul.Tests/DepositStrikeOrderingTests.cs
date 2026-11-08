using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Seismic and Odin's Grace on a multi-part deposit, with the order of events of the game's code (1.0.14 decompiled:
    ///     <c>MineRock5.Damage</c> → <c>RPC_Damage</c> → <c>DamageArea</c>, <c>MineRock.Damage</c> → <c>RPC_Hit</c>;
    ///     <c>ZRoutedRpc.InvokeRoutedRPC</c> runs a message to oneself at once). The striker's patch plans the extra hits in
    ///     the prefix, the game sends the original hit, the postfix sends the extras. On the striker's own deposit every
    ///     message is handled at once and a broken part is hidden straight away; on another player's deposit they queue
    ///     and arrive in order. Both must break the same parts, each at most once.
    /// </summary>
    public class DepositStrikeOrderingTests
    {
        private const float Margin = 0.05f;
        private const float Killing = 1e6f;

        /// <summary>The owner's side of a deposit: each part's health, the game's checks, and what dropped.</summary>
        private sealed class Deposit
        {
            public readonly float[] Health;
            public readonly bool[] Shown;
            public readonly int[] Drops;
            public readonly List<int> Damaged = new List<int>();
            private readonly GatheringRules.AreaBox[] _boxes;
            private readonly int _minToolTier;

            public Deposit(int minToolTier, float health, params GatheringRules.AreaBox[] boxes)
            {
                _boxes = boxes;
                _minToolTier = minToolTier;
                Health = boxes.Select(_ => health).ToArray();
                Shown = boxes.Select(_ => true).ToArray();
                Drops = new int[boxes.Length];
            }

            /// <summary>The owner handling one hit on one part (<c>MineRock5.DamageArea</c>, <c>MineRock.RPC_Hit</c>).</summary>
            public void Hit(int area, float damage, int toolTier)
            {
                if (Health[area] <= 0f)
                {
                    return; // "Already destroyed"
                }
                if (toolTier < _minToolTier)
                {
                    return; // "Too hard"
                }
                Health[area] -= damage;
                Damaged.Add(area);
                if (Health[area] <= 0f)
                {
                    Drops[area]++;
                    Shown[area] = false; // RPC_SetAreaHealth / RPC_Hide: the collider's object is deactivated
                }
            }

            /// <summary>
            ///     The parts as the striker's game sees them now. A deactivated collider reports empty bounds at the
            ///     origin (Unity), and is not live.
            /// </summary>
            public GatheringRules.AreaBox[] Read()
            {
                return _boxes.Select((b, i) => Shown[i] ? b : new GatheringRules.AreaBox(0f, 0f, 0f, 0f, 0f, 0f, false)).ToArray();
            }
        }

        /// <summary>Messages to the owner: handled at once on the striker's own game, else queued and delivered in order.</summary>
        private sealed class Network
        {
            private readonly bool _strikerOwns;
            private readonly Queue<Action> _queue = new Queue<Action>();

            public Network(bool strikerOwns)
            {
                _strikerOwns = strikerOwns;
            }

            public void Send(Action message)
            {
                if (_strikerOwns)
                {
                    message();
                }
                else
                {
                    _queue.Enqueue(message);
                }
            }

            public void Deliver()
            {
                while (_queue.Count > 0)
                {
                    _queue.Dequeue()();
                }
            }
        }

        /// <summary>Four parts in a row, away from the world's origin: 0 and 2 share a face with 1; 3 is far off.</summary>
        private static Deposit Copper(int minToolTier = 0, float health = 50f)
        {
            return new Deposit(minToolTier, health,
                new GatheringRules.AreaBox(100f, 30f, 200f, 101f, 31f, 201f, true),
                new GatheringRules.AreaBox(101f, 30f, 200f, 102f, 31f, 201f, true),
                new GatheringRules.AreaBox(102f, 30f, 200f, 103f, 31f, 201f, true),
                new GatheringRules.AreaBox(105f, 30f, 200f, 106f, 31f, 201f, true));
        }

        /// <summary>One swing at part <paramref name="struck" />, in the patch's order: plan, original hit, extras.</summary>
        private static Deposit Strike(bool strikerOwns, int struck, float damage, int toolTier, bool seismic, bool odin,
            int minToolTier = 0)
        {
            var deposit = Copper(minToolTier);
            var network = new Network(strikerOwns);
            // Prefix: before the game sends the hit.
            var plan = GatheringRules.ExtraHits(struck, GatheringRules.TouchingAreas(struck, deposit.Read(), Margin), seismic, odin);
            // The game's Damage: the original hit.
            network.Send(() => deposit.Hit(struck, damage, toolTier));
            // Postfix: the extra hits.
            foreach (var extra in plan)
            {
                var e = extra;
                network.Send(() => deposit.Hit(e.Area, e.Killing ? Killing : damage, toolTier));
            }
            network.Deliver();
            return deposit;
        }

        private static void SameOutcome(Deposit own, Deposit remote)
        {
            Assert.Equal(remote.Drops, own.Drops);
            Assert.Equal(remote.Health, own.Health);
            Assert.Equal(remote.Damaged.Distinct().OrderBy(i => i), own.Damaged.Distinct().OrderBy(i => i));
            Assert.All(own.Drops, d => Assert.InRange(d, 0, 1));
            Assert.All(remote.Drops, d => Assert.InRange(d, 0, 1));
        }

        [Fact]
        public void SeismicLethalHitBreaksTheSameNeighboursOnEitherOwner()
        {
            var own = Strike(strikerOwns: true, struck: 1, damage: 60f, toolTier: 2, seismic: true, odin: false);
            var remote = Strike(strikerOwns: false, struck: 1, damage: 60f, toolTier: 2, seismic: true, odin: false);
            SameOutcome(own, remote);
            Assert.Equal(new[] { 1, 1, 1, 0 }, own.Drops);
        }

        [Fact]
        public void SeismicGlancingHitDamagesTheSameNeighboursOnEitherOwner()
        {
            var own = Strike(true, 1, 20f, 2, seismic: true, odin: false);
            var remote = Strike(false, 1, 20f, 2, seismic: true, odin: false);
            SameOutcome(own, remote);
            Assert.Equal(new[] { 30f, 30f, 30f, 50f }, own.Health);
        }

        [Fact]
        public void OdinsGraceBreaksTheStruckPartAndNeighboursOnceOnEitherOwner()
        {
            // A glancing original hit (the killing hit then breaks the struck part) and a lethal one (the killing hit
            // finds it already broken): either way each part drops once.
            foreach (float damage in new[] { 20f, 60f })
            {
                var own = Strike(true, 1, damage, 2, seismic: false, odin: true);
                var remote = Strike(false, 1, damage, 2, seismic: false, odin: true);
                SameOutcome(own, remote);
                Assert.Equal(new[] { 1, 1, 1, 0 }, own.Drops);
            }
        }

        [Fact]
        public void ToolTierStillAppliesToEveryExtraHit()
        {
            foreach (bool owns in new[] { true, false })
            {
                var deposit = Strike(owns, 1, 60f, toolTier: 1, seismic: true, odin: true, minToolTier: 2);
                Assert.Empty(deposit.Damaged);
                Assert.Equal(new[] { 0, 0, 0, 0 }, deposit.Drops);
            }
        }

        /// <summary>
        ///     The bug this ordering fixes: reading the touching parts after the original hit (the old postfix) finds none
        ///     on the striker's own deposit, because the struck part is already hidden there.
        /// </summary>
        [Fact]
        public void ReadingTheNeighboursAfterTheHitMissesThemOnTheStrikersOwnDeposit()
        {
            var deposit = Copper();
            var network = new Network(strikerOwns: true);
            network.Send(() => deposit.Hit(1, 60f, 2));
            Assert.Empty(GatheringRules.TouchingAreas(1, deposit.Read(), Margin));
        }
    }
}
