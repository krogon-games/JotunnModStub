using System;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     How a skill or power slot shows its cooldown (user, 2026-10-31: "when the skill is off cooldown the player
    ///     can't mistake that"). While cooling down: the icon is dark, a dark sweep covers the time still to wait and the
    ///     seconds left are written large across it. Ready: the icon is in full colour with nothing over it, and the moment
    ///     it becomes ready the slot flashes once. Pure: no Unity, unit-tested.
    /// </summary>
    internal static class CooldownDisplay
    {
        /// <summary>How long the "ready" flash lasts, in seconds.</summary>
        public const float FlashSeconds = 0.8f;

        public static bool OnCooldown(float remaining) => remaining > 0f;

        /// <summary>Share of the slot the dark sweep covers: the part of the cooldown still to wait.</summary>
        public static float Sweep(float remaining, float total)
        {
            if (remaining <= 0f || total <= 0f)
            {
                return 0f;
            }
            return Math.Min(1f, remaining / total);
        }

        /// <summary>The time left, as the game writes it: whole seconds rounded up, "m:ss" from a minute; "" when ready.</summary>
        public static string Label(float remaining)
        {
            if (remaining <= 0f)
            {
                return "";
            }
            int seconds = (int)Math.Ceiling(remaining);
            int minutes = seconds / 60;
            return minutes > 0 ? minutes + ":" + (seconds % 60).ToString("00") : seconds.ToString();
        }

        /// <summary>True on the frame a cooldown runs out, which starts the flash.</summary>
        public static bool BecameReady(float previous, float remaining) => previous > 0f && remaining <= 0f;

        /// <summary>The flash's strength, 1 when it starts, fading to 0 after <see cref="FlashSeconds" />.</summary>
        public static float Flash(float secondsSinceReady)
        {
            if (secondsSinceReady < 0f || secondsSinceReady >= FlashSeconds)
            {
                return 0f;
            }
            float t = secondsSinceReady / FlashSeconds;
            return (1f - t) * (1f - t);
        }
    }
}
