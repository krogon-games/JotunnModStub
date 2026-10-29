using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Game-independent parts of opening a rip: sampling heart and landing spots, rejecting spots near bases, timing.
    ///     The game checks (biome, height, flatness, explored) are passed in, so this is unit-tested.
    /// </summary>
    internal static class RipRules
    {
        /// <summary>The playable world is a disc of about this radius (metres); the edge beyond is ocean.</summary>
        public const float WorldRadius = 9500f;

        public struct Spot
        {
            public float X;
            public float Z;

            public Spot(float x, float z)
            {
                X = x;
                Z = z;
            }
        }

        /// <summary>
        ///     Random spots spread evenly over the world disc that pass <paramref name="accept" />, up to
        ///     <paramref name="maxResults" />. <paramref name="random" /> returns numbers in [0, 1).
        /// </summary>
        public static List<Spot> Sample(Func<double> random, int samples, int maxResults, Func<float, float, bool> accept)
        {
            var spots = new List<Spot>();
            for (int i = 0; i < samples && spots.Count < maxResults; i++)
            {
                double r = WorldRadius * Math.Sqrt(random());
                double a = 2 * Math.PI * random();
                float x = (float)(r * Math.Cos(a));
                float z = (float)(r * Math.Sin(a));
                if (accept(x, z))
                {
                    spots.Add(new Spot(x, z));
                }
            }
            return spots;
        }

        /// <summary>
        ///     The heart is at least this far from the landing spot (metres): the range of every boss army raid
        ///     (<c>m_eventRange</c> 96 in <c>data/game-dump-1.0.20/events.csv</c>), so players land in quiet and the
        ///     waves start as they close in on the heart. A test checks the game data still agrees.
        /// </summary>
        public const float HeartMinDistance = 96f;

        /// <summary>The heart is at most this far from the landing spot (user, 2026-10-29: "just past 96 m", 96–128 m).</summary>
        public const float HeartMaxDistance = 128f;

        /// <summary>Where a rip's heart stands and where its travellers land.</summary>
        public struct Placement
        {
            public Spot Heart;
            public Spot Landing;

            public Placement(Spot heart, Spot landing)
            {
                Heart = heart;
                Landing = landing;
            }
        }

        /// <summary>
        ///     A random spot between <paramref name="min" /> and <paramref name="max" /> metres from
        ///     <paramref name="centre" /> that passes <paramref name="accept" />, trying up to <paramref name="tries" />
        ///     directions; null if none passes.
        /// </summary>
        public static Spot? SpotAround(Spot centre, Func<double> random, float min, float max, int tries, Func<float, float, bool> accept)
        {
            for (int i = 0; i < tries; i++)
            {
                double a = 2 * Math.PI * random();
                double d = min + (max - min) * random();
                float x = (float)(centre.X + d * Math.Cos(a));
                float z = (float)(centre.Z + d * Math.Sin(a));
                if (accept(x, z))
                {
                    return new Spot(x, z);
                }
            }
            return null;
        }

        /// <summary>The first candidate whose heart and landing spot both have no base marker within <paramref name="distance" /> metres, or null.</summary>
        public static Placement? FirstAwayFromBases(IList<Placement> candidates, IList<Spot> bases, float distance)
        {
            foreach (var c in candidates)
            {
                if (IsAwayFromBases(c.Heart, bases, distance) && IsAwayFromBases(c.Landing, bases, distance))
                {
                    return c;
                }
            }
            return null;
        }

        public static bool IsAwayFromBases(Spot spot, IList<Spot> bases, float distance)
        {
            float limit = distance * distance;
            foreach (var b in bases)
            {
                float dx = spot.X - b.X;
                float dz = spot.Z - b.Z;
                if (dx * dx + dz * dz < limit)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>One random event (raid) as the game defines it, reduced to what choosing a rip's army needs.</summary>
        public struct RaidInfo
        {
            public string Name;
            public bool Enabled;
            /// <summary>Global keys that stop the raid (it can't happen once any of them is set).</summary>
            public IList<string> NotRequiredKeys;
            public int SpawnEntries;
        }

        /// <summary>
        ///     A rip's waves are its biome boss's army: the game's raids that stop once that boss is defeated (the raid
        ///     lists the boss's defeat key among its "not required" keys), e.g. army_eikthyr for Eikthyr. Disabled raids
        ///     and raids without spawns are skipped. Returns the raids' names in the game's order.
        /// </summary>
        public static List<string> ArmyRaids(IEnumerable<RaidInfo> raids, string bossDefeatKey)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(bossDefeatKey))
            {
                return result;
            }
            foreach (var raid in raids)
            {
                if (raid.Enabled && raid.SpawnEntries > 0 && raid.NotRequiredKeys != null && raid.NotRequiredKeys.Contains(bossDefeatKey))
                {
                    result.Add(raid.Name);
                }
            }
            return result;
        }

        /// <summary>Time left until a rip closes, as "1 h 05 min" / "12 min" / "less than a minute".</summary>
        public static string TimeLeft(double secondsLeft)
        {
            if (secondsLeft < 60)
            {
                return "less than a minute";
            }
            int minutes = (int)(secondsLeft / 60);
            return minutes >= 60 ? $"{minutes / 60} h {minutes % 60:00} min" : $"{minutes} min";
        }
    }
}
