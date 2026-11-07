using System.Collections.Generic;

namespace CraftingOverhaul.Containers
{
    /// <summary>Where a chest stands in the exclusive-access handshake (<see cref="LeaseBook{TKey}" />).</summary>
    internal enum LeaseState
    {
        /// <summary>Not asked for, or given back.</summary>
        None,

        /// <summary>Asked the chest's owner; no answer yet.</summary>
        Pending,

        /// <summary>The owner said yes; waiting until this game sees itself as the chest's owner (the chest's latest contents come with that).</summary>
        Granted,

        /// <summary>Ours: we own it and have marked it in use, so nobody else can open it, pay from it or be handed it.</summary>
        Held,

        /// <summary>The owner said no (in use, private), or no answer in time, or we lost it.</summary>
        Failed
    }

    /// <summary>What a set of chests needed for one payment is waiting on.</summary>
    internal enum LeaseOutcome
    {
        AllHeld,
        Waiting,
        Failed
    }

    /// <summary>
    ///     Exclusive access to chests before paying from them (no game types, so it's tested). The game's own chest
    ///     handshake does the networking: ask the chest's owner (<c>Container.RPC_RequestOpen</c>); if it isn't in use the
    ///     owner sends the chest to us, makes us its owner and says yes (<c>RPC_OpenResponse</c>). A chest is only
    ///     <see cref="LeaseState.Held" /> once that yes has arrived <b>and</b> this game sees itself as the owner, because
    ///     the chest's contents travel with the ownership change: only then are they the latest. Works whether or not the
    ///     chest's owner has the mod.
    /// </summary>
    internal sealed class LeaseBook<TKey> where TKey : class
    {
        /// <summary>
        ///     How long to wait for the owner's answer and the ownership change. Not a gameplay number: well above a
        ///     round trip through the dedicated server (the game sends routed messages at once and object updates many
        ///     times a second), short enough that a lost answer only delays a craft briefly.
        /// </summary>
        public const double TimeoutSeconds = 3.0;

        private sealed class Lease
        {
            public LeaseState State;
            public double AskedAt;
        }

        private readonly Dictionary<TKey, Lease> _leases = new Dictionary<TKey, Lease>();

        public LeaseState State(TKey key) => _leases.TryGetValue(key, out var lease) ? lease.State : LeaseState.None;

        /// <summary>Asks for a chest unless it's already asked for or ours. True: send the request now.</summary>
        public bool Request(TKey key, double now)
        {
            if (_leases.TryGetValue(key, out var lease) && (lease.State == LeaseState.Pending || lease.State == LeaseState.Granted ||
                                                            lease.State == LeaseState.Held))
            {
                return false;
            }
            _leases[key] = new Lease { State = LeaseState.Pending, AskedAt = now };
            return true;
        }

        /// <summary>The owner's answer. Ignored unless we're waiting for it (a late answer after a timeout changes nothing).</summary>
        public bool OnResponse(TKey key, bool granted)
        {
            if (!_leases.TryGetValue(key, out var lease) || lease.State != LeaseState.Pending)
            {
                return false;
            }
            lease.State = granted ? LeaseState.Granted : LeaseState.Failed;
            return true;
        }

        /// <summary>
        ///     Moves a chest along: granted and now owned → held (true: mark it in use now); waiting too long → failed;
        ///     held but no longer ours (another game took it, or someone closed it) → failed.
        /// </summary>
        public bool Tick(TKey key, bool weOwnIt, bool markedInUse, double now)
        {
            if (!_leases.TryGetValue(key, out var lease))
            {
                return false;
            }
            switch (lease.State)
            {
                case LeaseState.Pending:
                    if (now - lease.AskedAt > TimeoutSeconds)
                    {
                        lease.State = LeaseState.Failed;
                    }
                    return false;
                case LeaseState.Granted:
                    if (weOwnIt)
                    {
                        lease.State = LeaseState.Held;
                        return true;
                    }
                    if (now - lease.AskedAt > TimeoutSeconds)
                    {
                        lease.State = LeaseState.Failed;
                    }
                    return false;
                case LeaseState.Held:
                    if (!weOwnIt || !markedInUse)
                    {
                        lease.State = LeaseState.Failed;
                    }
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>Gives a chest back (or forgets a failed one). True if it was held: clear its in-use mark.</summary>
        public bool Release(TKey key)
        {
            if (!_leases.TryGetValue(key, out var lease))
            {
                return false;
            }
            _leases.Remove(key);
            return lease.State == LeaseState.Held;
        }

        public bool IsHeld(TKey key) => State(key) == LeaseState.Held;

        /// <summary>All of <paramref name="keys" /> held, any failed, or still waiting.</summary>
        public LeaseOutcome Outcome(IEnumerable<TKey> keys)
        {
            bool waiting = false;
            foreach (var key in keys)
            {
                switch (State(key))
                {
                    case LeaseState.Held:
                        break;
                    case LeaseState.Failed:
                    case LeaseState.None:
                        return LeaseOutcome.Failed;
                    default:
                        waiting = true;
                        break;
                }
            }
            return waiting ? LeaseOutcome.Waiting : LeaseOutcome.AllHeld;
        }

        public List<TKey> Keys() => new List<TKey>(_leases.Keys);
    }

    /// <summary>Which nearby chests a payment needs (no game types, so it's tested).</summary>
    internal static class PayPlan
    {
        /// <summary>
        ///     For each cost line, what's carried (bag, inventory tabs, backpack) is used first; the shortfall comes
        ///     from the chests, closest first. <paramref name="chestCounts" />[chest][line] is what each chest holds,
        ///     the chests already in distance order. Returns the chests to hold, closest first; with
        ///     <paramref name="everyChestWithStock" />, every chest holding a short line's item (building: a grid
        ///     planter may need more than one piece's worth). Empty when nothing is short. <paramref name="enough" /> is
        ///     whether carried plus all these chests cover every line.
        /// </summary>
        public static List<int> ChestsNeeded(int[] needed, int[] carried, IList<int[]> chestCounts, bool everyChestWithStock,
            out bool enough)
        {
            var chests = new List<int>();
            var chosen = new bool[chestCounts.Count];
            enough = true;
            for (int line = 0; line < needed.Length; line++)
            {
                int missing = needed[line] - carried[line];
                if (missing <= 0)
                {
                    continue;
                }
                for (int c = 0; c < chestCounts.Count && (missing > 0 || everyChestWithStock); c++)
                {
                    int stock = chestCounts[c][line];
                    if (stock <= 0)
                    {
                        continue;
                    }
                    missing -= stock;
                    if (!chosen[c])
                    {
                        chosen[c] = true;
                        chests.Add(c);
                    }
                }
                if (missing > 0)
                {
                    enough = false;
                }
            }
            chests.Sort();
            return chests;
        }

        /// <summary>
        ///     Taking <paramref name="amount" /> from held chests, all or nothing: how much each gives (closest first), or
        ///     null when together they hold less, so no chest is charged for a payment that can't complete.
        /// </summary>
        public static int[] Take(int amount, IList<int> heldStock)
        {
            int total = 0;
            foreach (int stock in heldStock)
            {
                total += stock;
            }
            if (amount <= 0 || total < amount)
            {
                return amount <= 0 ? new int[heldStock.Count] : null;
            }
            var take = new int[heldStock.Count];
            int left = amount;
            for (int i = 0; i < heldStock.Count && left > 0; i++)
            {
                take[i] = heldStock[i] < left ? heldStock[i] : left;
                left -= take[i];
            }
            return take;
        }
    }
}
