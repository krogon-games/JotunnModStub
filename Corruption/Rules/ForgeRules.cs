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

        /// <summary>
        ///     Which affix lines a use changed: <paramref name="added" /> are indexes into <paramref name="after" /> of
        ///     lines that are new or have a new value, <paramref name="removed" /> indexes into <paramref name="before" />
        ///     of lines that are gone or had their old value. Lines are compared by key ("ids=value"), so an untouched
        ///     line matches itself even when others moved, and two identical lines count separately.
        /// </summary>
        public static void Diff(IList<string> before, IList<string> after, out List<int> added, out List<int> removed)
        {
            added = new List<int>();
            removed = new List<int>();
            before = before ?? new string[0];
            after = after ?? new string[0];
            var unmatched = new List<int>();
            for (int i = 0; i < before.Count; i++)
            {
                unmatched.Add(i);
            }
            for (int j = 0; j < after.Count; j++)
            {
                int match = unmatched.FindIndex(i => before[i] == after[j]);
                if (match >= 0)
                {
                    unmatched.RemoveAt(match);
                }
                else
                {
                    added.Add(j);
                }
            }
            removed.AddRange(unmatched);
        }

        /// <summary>
        ///     The short headline of a use, e.g. "Regal Orb: now Rare, 1 affix added", "Divine Orb: 3 affixes rerolled",
        ///     "Orb of Annulment: 1 affix removed". The changed lines themselves are listed under it.
        /// </summary>
        public static string Summary(string material, string rarityBefore, string rarityAfter, int added, int removed)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(rarityAfter) && rarityAfter != rarityBefore)
            {
                parts.Add("now " + char.ToUpperInvariant(rarityAfter[0]) + rarityAfter.Substring(1));
            }
            int rerolled = System.Math.Min(added, removed);
            if (rerolled > 0)
            {
                parts.Add(Count(rerolled, "rerolled"));
            }
            if (added > rerolled)
            {
                parts.Add(Count(added - rerolled, "added"));
            }
            if (removed > rerolled)
            {
                parts.Add(Count(removed - rerolled, "removed"));
            }
            string what = parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "no change";
            return string.IsNullOrEmpty(material) ? what : material + ": " + what;
        }

        private static string Count(int n, string verb) => n + (n == 1 ? " affix " : " affixes ") + verb;

        /// <summary>What <c>LootItem.LineText</c> adds to a resonant line; the bench shows a diamond instead.</summary>
        public const string ResonantSuffix = " ◆ resonant";

        public const string BulletColor = "#C8B878";
        public const string ResonantColor = "#5BD16A";

        /// <summary>
        ///     One affix on the bench as a bulleted item (Krogon, 2026-11-10: the affixes read as "a wall of text"): a
        ///     round bullet, or a green diamond for a resonant line, then the text with its wrapped lines indented
        ///     under the first word, so each affix starts clearly. TextMeshPro's indent keeps the hanging indent.
        /// </summary>
        public static string BulletLine(string line, bool resonant)
        {
            line = (line ?? "").Replace(ResonantSuffix, "");
            string bullet = resonant ? "<color=" + ResonantColor + ">◆</color>" : "<color=" + BulletColor + ">•</color>";
            return bullet + "<indent=1.1em>" + line + "</indent>";
        }
    }
}
