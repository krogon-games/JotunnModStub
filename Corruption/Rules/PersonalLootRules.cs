using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>How a personal drop looks to the player viewing it.</summary>
    public enum LootLockState
    {
        /// <summary>Nobody owns it (or the lock is over): everyone sees it and can pick it up.</summary>
        Free,

        /// <summary>Locked, and the viewer is the owner.</summary>
        Mine,

        /// <summary>Locked to someone else: the viewer can't see it or pick it up.</summary>
        Theirs
    }

    /// <summary>
    ///     Personal loot (corruption-design.md → Loot and uniques → Drops): "only the roller can pick a drop up for the
    ///     first 60 seconds, then anyone can". During the lock, other players can't see the drop at all (no item, no
    ///     beam, no hover) and can't pick it up; then it appears for everyone. Logging out changes nothing: the 60
    ///     seconds run on the network clock. No game dependencies, so it is unit-tested.
    /// </summary>
    public static class PersonalLootRules
    {
        /// <summary>ZDO long: the owner's player id (0 or missing = nobody).</summary>
        public const string OwnerKey = "corruption_loot_owner";

        /// <summary>ZDO long: network time (DateTime ticks) when it dropped.</summary>
        public const string TimeKey = "corruption_loot_time";

        /// <summary>ZDO string: the <c>loot_beams.csv</c> row.</summary>
        public const string BeamKey = "corruption_loot_beam";

        public const long TicksPerSecond = 10000000L;

        /// <summary>
        ///     The beam's brightness factor, so it pulses (Krogon, 2026-11-26: "Would look better if the beam pulsated very
        ///     slowly"). Moves smoothly (a cosine) between 1 and 1 - <paramref name="strength" /> once per period: 1 at the
        ///     start of each period when <paramref name="phase" /> is 0. <paramref name="phase" /> (0..1, a share of the
        ///     period) offsets the cycle so neighbouring beams don't pulse together. No strength or no period = steady (1).
        /// </summary>
        public static float PulseFactor(double timeSeconds, double periodSeconds, double strength, double phase)
        {
            if (strength <= 0.0 || periodSeconds <= 0.0)
            {
                return 1f;
            }
            double cycle = timeSeconds / periodSeconds + phase;
            double wave = 0.5 + 0.5 * Math.Cos(2.0 * Math.PI * cycle); // 1 .. 0
            return (float)(1.0 - strength * (1.0 - wave));
        }

        /// <summary>
        ///     How opaque the beam is at a point across its width, as a share of its middle (a soft edge, Krogon, 2026-11-26:
        ///     "too solid looking"). <paramref name="across01" /> runs 0..1 from one edge to the other (outside is clamped).
        ///     Symmetric; 1 in the middle. <paramref name="softness" /> (0..1): 0 = a hard edge (1 all the way across);
        ///     above 0 the fade starts at (1 - softness) of the way from the middle to the edge and eases (smoothstep) to 0
        ///     exactly at the edges, so 1 starts fading from the very middle.
        /// </summary>
        public static double EdgeAlpha(double across01, double softness)
        {
            double a = Math.Max(0.0, Math.Min(1.0, across01));
            double s = Math.Max(0.0, Math.Min(1.0, softness));
            if (s <= 0.0)
            {
                return 1.0;
            }
            double t = Math.Abs(2.0 * a - 1.0); // 0 in the middle, 1 at the edges
            double start = 1.0 - s;
            if (t <= start)
            {
                return 1.0;
            }
            double u = (t - start) / s; // 0..1
            return 1.0 - u * u * (3.0 - 2.0 * u);
        }

        /// <summary>Seconds since the drop. Never negative: the network clock can step back (about 2 s) when the server corrects it.</summary>
        public static double AgeSeconds(long dropTicks, long nowTicks) => Math.Max(0.0, (nowTicks - dropTicks) / (double)TicksPerSecond);

        /// <summary>Seconds of lock left, from 0 to <paramref name="lockSeconds" />.</summary>
        public static double SecondsLeft(long dropTicks, long nowTicks, double lockSeconds) =>
            Math.Max(0.0, Math.Min(lockSeconds, lockSeconds - AgeSeconds(dropTicks, nowTicks)));

        /// <summary>
        ///     Free when there is no owner, when the drop time is missing (never lock forever) or when the lock is over.
        ///     Otherwise Mine for the owner and Theirs for everybody else; an unknown viewer (0: a dedicated server or a
        ///     loading game) counts as someone else.
        /// </summary>
        public static LootLockState StateFor(long owner, long dropTicks, long nowTicks, long viewer, double lockSeconds)
        {
            if (owner == 0 || dropTicks <= 0 || AgeSeconds(dropTicks, nowTicks) >= lockSeconds)
            {
                return LootLockState.Free;
            }
            return viewer != 0 && viewer == owner ? LootLockState.Mine : LootLockState.Theirs;
        }

        public static bool CanPickUp(LootLockState state) => state != LootLockState.Theirs;

        public static bool Visible(LootLockState state) => state != LootLockState.Theirs;

        /// <summary>The landing sound plays for the owner only, only for a row that has a sound, and only if the drop is new (within the window).</summary>
        public static bool PlaySound(LootLockState state, double ageSeconds, double windowSeconds, bool rowHasSound) =>
            rowHasSound && state == LootLockState.Mine && ageSeconds <= windowSeconds;

        /// <summary>A drop time such that <paramref name="wantedSecondsLeft" /> (clamped to 0..lockSeconds) of the lock remain. For a dev command.</summary>
        public static long BackdatedTicks(long nowTicks, double lockSeconds, double wantedSecondsLeft)
        {
            double left = Math.Max(0.0, Math.Min(lockSeconds, wantedSecondsLeft));
            return nowTicks - (long)Math.Round((lockSeconds - left) * TicksPerSecond);
        }

        /// <summary>
        ///     Vanilla merges nearby stacks into one and destroys the other drop, which would lose its owner or beam, so a
        ///     merge happens only when neither drop carries our tags.
        /// </summary>
        public static bool MayAutoStack(bool thisTagged, bool otherTagged) => !thisTagged && !otherTagged;

        /// <summary>
        ///     Deals the shared part of a destroyed rip heart's loot among the players present. Players sit in a random
        ///     order (Fisher-Yates, once); every single unit of kind 0 is dealt one at a time to the next seat in turn,
        ///     then kind 1, and so on, the turn carrying on between kinds (so 10 coins for 3 players go 4/3/3 and the next
        ///     kind starts with the next player, so leftovers rotate). "If an odd number rolls, someone may miss out and
        ///     that's fine." Returns result[kind][player] = units, where player is an index into the caller's player list.
        ///     With no players, seat 0 gets everything (the caller treats it as "nobody").
        /// </summary>
        /// <param name="randomBelow">Returns 0..n-1 for the given n.</param>
        public static int[][] Deal(int players, IList<int> unitsPerKind, Func<int, int> randomBelow)
        {
            int seats = Math.Max(1, players);
            var result = new int[unitsPerKind.Count][];
            for (int k = 0; k < result.Length; k++)
            {
                result[k] = new int[seats];
            }
            if (players <= 0)
            {
                for (int k = 0; k < result.Length; k++)
                {
                    result[k][0] = Math.Max(0, unitsPerKind[k]);
                }
                return result;
            }
            var order = new int[seats];
            for (int i = 0; i < seats; i++)
            {
                order[i] = i;
            }
            for (int i = seats - 1; i > 0; i--)
            {
                int j = randomBelow(i + 1);
                int swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }
            int turn = 0;
            for (int k = 0; k < result.Length; k++)
            {
                for (int u = 0; u < unitsPerKind[k]; u++)
                {
                    result[k][order[turn]]++;
                    turn = (turn + 1) % seats;
                }
            }
            return result;
        }
    }
}
