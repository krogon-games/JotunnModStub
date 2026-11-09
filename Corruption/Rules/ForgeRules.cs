using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>What a click on the Riftforge's target slot does.</summary>
    internal enum ForgeClick
    {
        /// <summary>Nothing happens (a hint may be shown).</summary>
        Nothing,

        /// <summary>The item being dragged from the bag becomes the target (it stays in the bag).</summary>
        Insert,

        /// <summary>The picked material is used on the target, once.</summary>
        Apply,

        /// <summary>The target is taken out of the slot (it never left the bag).</summary>
        Remove,

        /// <summary>The material pick is dropped; nothing else happens.</summary>
        CancelPick
    }

    /// <summary>
    ///     The Riftforge workspace's click rules (user, 2026-11-09, #30): right-click a material to pick it, left-click
    ///     the target to use it once, Shift keeps the pick for the next click, every use needs its own click, destructive
    ///     operations ask first. Only the interaction lives here; what an operation does and when it's allowed is
    ///     <c>RiftforgeCrafting</c>'s. Pure: no Unity, unit-tested.
    /// </summary>
    internal static class ForgeRules
    {
        /// <summary>
        ///     Operations that can destroy the item: they show their warning and chances and need a separate Confirm
        ///     (shattering always destroys it; a Corruption shard can, 25%). Ordinary orbs never destroy an item, so they
        ///     apply on the click itself.
        /// </summary>
        public static bool NeedsConfirmation(string operationId)
        {
            return operationId == "shatter" || operationId == "corrupt";
        }

        /// <summary>A click on the target slot.</summary>
        public static ForgeClick TargetClick(bool leftButton, bool dragging, bool materialPicked, bool hasTarget)
        {
            if (!leftButton)
            {
                // Right-click: drop the pick first; with no pick, take the target out.
                return materialPicked ? ForgeClick.CancelPick : hasTarget ? ForgeClick.Remove : ForgeClick.Nothing;
            }
            if (dragging)
            {
                return ForgeClick.Insert;
            }
            return materialPicked && hasTarget ? ForgeClick.Apply : ForgeClick.Nothing;
        }

        /// <summary>
        ///     Whether the material pick stays after a use: only while Shift is held, and only while there's more of it
        ///     (the window then picks the next stack of the same material).
        /// </summary>
        public static bool KeepPick(bool shiftHeld, bool moreLeft)
        {
            return shiftHeld && moreLeft;
        }

        /// <summary>
        ///     Which of a material's operations a click uses: the first one allowed, or -1 when none is (the click then
        ///     shows the first refusal and uses nothing). An orb has exactly one; registered operations (map rolling)
        ///     may add more.
        /// </summary>
        public static int PickOperation(IList<bool> allowed)
        {
            if (allowed == null)
            {
                return -1;
            }
            for (int i = 0; i < allowed.Count; i++)
            {
                if (allowed[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Our label in capitals when the game's own label next to it is in capitals ("CRAFT", "UPGRADE"), so the
        ///     Forge tab and buttons match the station's.
        /// </summary>
        public static string UpperLike(string gameText, string label)
        {
            if (label == null)
            {
                return "";
            }
            return !string.IsNullOrEmpty(gameText) && gameText == gameText.ToUpperInvariant() && gameText != gameText.ToLowerInvariant()
                ? label.ToUpperInvariant()
                : label;
        }
    }
}
