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
        ///     (<c>m_eventRange</c> 96 in <c>data/game-dump-1.0.21/events.csv</c>), so players land in quiet and the
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

        /// <summary>
        ///     Rips never open (heart or landing spot) closer than this to anything a player placed (metres): the waves'
        ///     range, the army raids' <c>m_eventRange</c> 96 in <c>data/game-dump-1.0.21/events.csv</c> (Krogon, 2026-11-26,
        ///     rips round decision 12). The config's <c>BaseDistance</c> overrides it when above 0.
        /// </summary>
        public const float DefaultBaseDistance = 96f;

        /// <summary>The distance to keep from placed objects: the configured one when above 0, else <see cref="DefaultBaseDistance" />.</summary>
        public static float BaseDistance(float configured)
        {
            return configured > 0f ? configured : DefaultBaseDistance;
        }

        /// <summary>
        ///     Finds which rip candidates are too close to anything a player placed, without testing every candidate
        ///     against every placed object: candidate spots (heart and landing) go into a grid of cells one
        ///     <c>distance</c> wide, so each placed object only meets the spots in its own and the eight neighbouring cells.
        ///     Horizontal distance, strictly less than <c>distance</c> counts as near (as <see cref="IsAwayFromBases" />).
        /// </summary>
        public sealed class PlacedScan
        {
            private readonly IList<Placement> m_candidates;
            private readonly bool[] m_rejected;
            private readonly float m_distance;
            private readonly float m_limit;
            private readonly Dictionary<long, List<int>> m_cells = new Dictionary<long, List<int>>();
            private int m_alive;

            public PlacedScan(IList<Placement> candidates, float distance)
            {
                m_candidates = candidates;
                m_distance = distance < 1f ? 1f : distance;
                m_limit = distance * distance;
                m_rejected = new bool[candidates.Count];
                m_alive = candidates.Count;
                for (int i = 0; i < candidates.Count; i++)
                {
                    long heart = CellKey(candidates[i].Heart.X, candidates[i].Heart.Z);
                    long landing = CellKey(candidates[i].Landing.X, candidates[i].Landing.Z);
                    AddToCell(heart, i);
                    if (landing != heart)
                    {
                        AddToCell(landing, i);
                    }
                }
            }

            /// <summary>How many candidates no placed object has rejected yet.</summary>
            public int Alive => m_alive;

            public bool AllRejected => m_alive == 0;

            /// <summary>Whether a placed object at (x, z) is near a candidate that is still alive.</summary>
            public bool Touches(float x, float z)
            {
                return Visit(x, z, false) > 0;
            }

            /// <summary>Rejects every live candidate with its heart or landing spot near (x, z); returns how many.</summary>
            public int Reject(float x, float z)
            {
                return Visit(x, z, true);
            }

            /// <summary>The first candidate (in the given order) that nothing has rejected, or null.</summary>
            public Placement? First()
            {
                for (int i = 0; i < m_candidates.Count; i++)
                {
                    if (!m_rejected[i])
                    {
                        return m_candidates[i];
                    }
                }
                return null;
            }

            private int Visit(float x, float z, bool reject)
            {
                int cx = Cell(x), cz = Cell(z);
                int hits = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        List<int> indices;
                        if (!m_cells.TryGetValue(Key(cx + dx, cz + dz), out indices))
                        {
                            continue;
                        }
                        foreach (int i in indices)
                        {
                            if (m_rejected[i])
                            {
                                continue;
                            }
                            var c = m_candidates[i];
                            if (!Near(c.Heart, x, z) && !Near(c.Landing, x, z))
                            {
                                continue;
                            }
                            if (!reject)
                            {
                                return 1;
                            }
                            m_rejected[i] = true;
                            m_alive--;
                            hits++;
                        }
                    }
                }
                return hits;
            }

            private bool Near(Spot spot, float x, float z)
            {
                float dx = spot.X - x;
                float dz = spot.Z - z;
                return dx * dx + dz * dz < m_limit;
            }

            private void AddToCell(long key, int index)
            {
                List<int> list;
                if (!m_cells.TryGetValue(key, out list))
                {
                    list = new List<int>();
                    m_cells[key] = list;
                }
                list.Add(index);
            }

            private int Cell(float v)
            {
                return (int)Math.Floor(v / m_distance);
            }

            private long CellKey(float x, float z)
            {
                return Key(Cell(x), Cell(z));
            }

            private static long Key(int cx, int cz)
            {
                return ((long)cx << 32) ^ (uint)cz;
            }
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
