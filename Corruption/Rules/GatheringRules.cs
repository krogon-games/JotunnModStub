using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Rules for the gathering-tool affixes that change a strike on the striker's game (AFFIX_RULES.md → Gathering
    ///     tool affixes). No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class GatheringRules
    {
        /// <summary>Veinbreaker's: every 5th mining hit (AFFIX_RULES: "Every 5th mining hit deals extra damage").</summary>
        public const int VeinbreakerEvery = 5;

        /// <summary>A percent chance roll: true with <paramref name="percent" />% probability.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static bool Roll(float percent, Func<double> random)
        {
            return percent > 0f && random() * 100.0 < percent;
        }

        /// <summary>
        ///     The pickup radius while the tool is held: Magnetic sets it (if larger than vanilla's), Keen-eyed adds on
        ///     top (AFFIX_RULES: "Keen-eyed still adds on top"), the total capped (affix_caps.csv → pickup_radius).
        /// </summary>
        public static float PickupRadius(float vanilla, float magneticSet, float keenAdd, float cap)
        {
            float radius = Math.Max(vanilla, magneticSet) + Math.Max(0f, keenAdd);
            return cap > 0f ? Math.Min(radius, Math.Max(cap, vanilla)) : radius;
        }

        /// <summary>
        ///     Counts mining swings for Veinbreaker's. The count lives on the striker's game and starts over when the tool
        ///     changes or is unequipped, for any reason (AFFIX_RULES → Interpretations; Krogon, 2026-11-08: hiding the
        ///     tool, swimming and eating unequip it too).
        /// </summary>
        public sealed class HitCounter
        {
            private int _count;
            private object _tool;

            /// <summary>Counts a mining swing with this tool; true on every 5th.</summary>
            public bool NextSwing(object tool)
            {
                if (!ReferenceEquals(tool, _tool))
                {
                    _tool = tool;
                    _count = 0;
                }
                _count++;
                if (_count < VeinbreakerEvery)
                {
                    return false;
                }
                _count = 0;
                return true;
            }

            /// <summary>An item left the player's hands: the count starts over if it is the counted tool.</summary>
            public void Unequipped(object item)
            {
                if (item != null && ReferenceEquals(item, _tool))
                {
                    _tool = null;
                    _count = 0;
                }
            }
        }

        // ---- Seismic and Odin's Grace on multi-part deposits ----

        /// <summary>A deposit part's collider bounds as they were before the hit, and whether the part was still there.</summary>
        public struct AreaBox
        {
            public float MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
            public bool Live;

            public AreaBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ, bool live)
            {
                MinX = minX;
                MinY = minY;
                MinZ = minZ;
                MaxX = maxX;
                MaxY = maxY;
                MaxZ = maxZ;
                Live = live;
            }
        }

        /// <summary>One extra hit Seismic or Odin's Grace sends: the part it goes to, and whether it is a killing hit.</summary>
        public struct ExtraHit
        {
            public int Area;
            public bool Killing;

            public ExtraHit(int area, bool killing)
            {
                Area = area;
                Killing = killing;
            }
        }

        /// <summary>
        ///     The parts touching the struck one: still there, and their bounds overlap the struck part's bounds grown by
        ///     <paramref name="margin" /> on every side (as Unity's <c>Bounds.Expand(2 * margin)</c> and
        ///     <c>Bounds.Intersects</c>, edges included). Must be read <b>before</b> the hit is sent: when the striker also
        ///     owns the deposit, the game handles the hit at once and hides a part it breaks, so its bounds are gone after.
        ///     A struck part that is already gone has no neighbours.
        /// </summary>
        public static List<int> TouchingAreas(int hitIndex, IList<AreaBox> areas, float margin)
        {
            var touching = new List<int>();
            if (areas == null || hitIndex < 0 || hitIndex >= areas.Count)
            {
                return touching;
            }
            var hit = areas[hitIndex];
            if (!hit.Live)
            {
                return touching;
            }
            for (int i = 0; i < areas.Count; i++)
            {
                var area = areas[i];
                if (i == hitIndex || !area.Live)
                {
                    continue;
                }
                if (hit.MinX - margin <= area.MaxX && hit.MaxX + margin >= area.MinX &&
                    hit.MinY - margin <= area.MaxY && hit.MaxY + margin >= area.MinY &&
                    hit.MinZ - margin <= area.MaxZ && hit.MaxZ + margin >= area.MinZ)
                {
                    touching.Add(i);
                }
            }
            return touching;
        }

        /// <summary>
        ///     The extra hits a swing sends after its original hit: Odin's Grace a killing hit on the struck part, then one on
        ///     each touching part; Seismic (without Odin's Grace) a copy of the hit on each touching part. A part already
        ///     broken by then takes nothing (the game's own "already destroyed" check), so nothing drops twice.
        /// </summary>
        public static List<ExtraHit> ExtraHits(int hitIndex, IList<int> touching, bool seismic, bool odin)
        {
            var hits = new List<ExtraHit>();
            if (hitIndex < 0 || !(seismic || odin))
            {
                return hits;
            }
            if (odin)
            {
                hits.Add(new ExtraHit(hitIndex, true));
            }
            if (touching != null)
            {
                foreach (int index in touching)
                {
                    hits.Add(new ExtraHit(index, odin));
                }
            }
            return hits;
        }
    }
}
