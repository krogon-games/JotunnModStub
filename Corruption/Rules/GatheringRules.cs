using System;

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
        ///     Counts mining swings for Veinbreaker's. The count lives on the striker's game and resets when the tool
        ///     changes (AFFIX_RULES → Interpretations).
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
        }
    }
}
