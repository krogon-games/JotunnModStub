using System;
using System.Globalization;

namespace Corruption.Rules
{
    /// <summary>
    ///     How long a talisman-repelled creature fights back after the wearer hits it, as stored on the creature's ZDO so
    ///     every game agrees: one value per player (<see cref="Key" /> with the character's player id), holding the world
    ///     time (<c>ZNet.GetTimeSeconds</c>, the same on every game) in milliseconds until which it fights back. Kept on the
    ///     creature, the time survives the creature changing owner (the owner logging out); kept per player, it only
    ///     concerns the player who hit. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class ProvokeRules
    {
        public const string KeyPrefix = "corruption_provoked_";

        /// <summary>The ZDO key for one player's provocation of a creature (the game stores it by this name's hash).</summary>
        public static string Key(long playerId)
        {
            return KeyPrefix + playerId.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>World seconds as stored: whole milliseconds; 0 for anything unusable (never provoked).</summary>
        public static long ToMs(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0d)
            {
                return 0L;
            }
            double ms = Math.Floor(seconds * 1000d);
            return ms >= long.MaxValue ? long.MaxValue : (long)ms;
        }

        /// <summary>
        ///     What a hit at world time <paramref name="nowSeconds" /> stores: until <paramref name="provokeSeconds" />
        ///     after it. Each hit stores its own time, so a later hit refreshes it. 0 (no provocation) when the setting is
        ///     0, negative or not a number.
        /// </summary>
        public static long Until(double nowSeconds, float provokeSeconds)
        {
            if (float.IsNaN(provokeSeconds) || float.IsInfinity(provokeSeconds) || provokeSeconds <= 0f)
            {
                return 0L;
            }
            return ToMs(nowSeconds + provokeSeconds);
        }

        /// <summary>Whether a stored time (0 = none) still makes the creature fight back at <paramref name="nowSeconds" />.</summary>
        public static bool IsProvoked(long untilMs, double nowSeconds)
        {
            return untilMs > 0L && ToMs(nowSeconds) < untilMs;
        }
    }
}
