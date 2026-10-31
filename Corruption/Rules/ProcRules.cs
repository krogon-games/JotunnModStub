using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Rules for the proc and utility affixes (AFFIX_RULES.md → Proc and utility affixes): stacks, cooldowns and caps.
    ///     No game dependencies, so it is unit-tested. The numbers come from the rows' text and notes in
    ///     <c>Corruption/Data/affixes.csv</c>; <c>ProcRulesTests</c> checks the table still says them.
    /// </summary>
    internal static class ProcRules
    {
        /// <summary>Shieldwall, Rallying, Skald's ("for 10 s", "lasts 10 s") and Fenrir ("for 10 s").</summary>
        public const float BuffSeconds = 10f;

        /// <summary>Parry buffs reach allies within this range (notes: "Allies within 10 m"; AFFIX_RULES: "within 10 m").</summary>
        public const float AllyRange = 10f;

        /// <summary>Reprisal's lightning: this share of the blocking item's block power (notes: "50% of the blocking item's block power").</summary>
        public const float ReprisalBlockShare = 0.5f;

        /// <summary>Wrath's shockwave radius (text: "within 3 m").</summary>
        public const float WrathRadius = 3f;

        /// <summary>Skadi: at most this many stacks (text: "up to 5 stacks").</summary>
        public const int SkadiMaxStacks = 5;

        /// <summary>Skadi's stacks reset after this long without a hit (notes: "3 s without a hit").</summary>
        public const float SkadiResetSeconds = 3f;

        /// <summary>Fenrir: at most this many stacks (text: "up to 3 stacks").</summary>
        public const int FenrirMaxStacks = 3;

        /// <summary>Foraging's boost lasts this long after a pick (text: "for 8 s").</summary>
        public const float ForagingSeconds = 8f;

        /// <summary>One more stack, up to <paramref name="max" />.</summary>
        public static int AddStack(int stacks, int max)
        {
            return Math.Min(max, Math.Max(0, stacks) + 1);
        }

        /// <summary>
        ///     What an extra bonus adds under a shared cap: the capped total with it minus the capped total without it. So an
        ///     extra source (Foraging's boost, a parry or kill buff) never takes a total past the cap the other affixes of
        ///     that cap already use.
        /// </summary>
        public static float ExtraUnderCap(float baseTotal, float extra, string family, AffixCaps caps)
        {
            if (extra <= 0f)
            {
                return 0f;
            }
            return Math.Max(0f, AffixTotals.Cap(baseTotal + extra, family, caps) - AffixTotals.Cap(baseTotal, family, caps));
        }

        /// <summary>
        ///     The lowest value of one effect over several items (0 when none has it). Deathless "never stacks: several
        ///     Deathless items share one cooldown", and a lower cooldown is better, so the shortest one counts.
        /// </summary>
        public static float Lowest(IEnumerable<LootItem> items, string effect, IDictionary<string, AffixTable.Affix> affixes)
        {
            float lowest = 0f;
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                foreach (var line in item.Lines)
                {
                    if (line.Value > 0f && affixes.TryGetValue(line.Ids[0], out var affix) && affix.Effect == effect &&
                        (lowest <= 0f || line.Value < lowest))
                    {
                        lowest = line.Value;
                    }
                }
            }
            return lowest;
        }

        /// <summary>
        ///     Seconds left on a cooldown that started at <paramref name="usedAt" /> (world time, seconds); 0 when ready.
        ///     Never used (0 or less), or a start in the future (another world, whose clock is behind), counts as ready.
        /// </summary>
        public static double CooldownLeft(double usedAt, double now, double cooldownSeconds)
        {
            if (usedAt <= 0 || cooldownSeconds <= 0 || now < usedAt)
            {
                return 0;
            }
            return Math.Max(0, usedAt + cooldownSeconds - now);
        }

        /// <summary>Skadi's extra frost: each stack adds the rolled share of the hit's physical damage.</summary>
        public static float SkadiFrost(float physical, float percent, int stacks)
        {
            if (physical <= 0f || percent <= 0f || stacks <= 0)
            {
                return 0f;
            }
            return physical * percent / 100f * Math.Min(stacks, SkadiMaxStacks);
        }

        /// <summary>
        ///     Skadi's stacks on the striker's game: each hit on the same target adds one, up to the cap; another target,
        ///     another weapon or <see cref="SkadiResetSeconds" /> without a hit starts again at one.
        /// </summary>
        public sealed class TargetStacks
        {
            private object _target;
            private object _weapon;
            private double _lastHit = double.NegativeInfinity;
            private int _stacks;

            /// <summary>Counts a hit; returns the stacks it deals with (1 for the first hit on a target).</summary>
            public int Hit(object target, object weapon, double time)
            {
                if (!ReferenceEquals(target, _target) || !ReferenceEquals(weapon, _weapon) || time - _lastHit > SkadiResetSeconds)
                {
                    _target = target;
                    _weapon = weapon;
                    _stacks = 0;
                }
                _lastHit = time;
                _stacks = AddStack(_stacks, SkadiMaxStacks);
                return _stacks;
            }
        }
    }
}
