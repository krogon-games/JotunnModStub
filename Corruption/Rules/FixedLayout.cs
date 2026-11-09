using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Fixed places for materials in a tab (user, 2026-11-09, #30): in Currencies each orb and the Corruption shard,
    ///     in Essences each boss essence, has its own home cell, and keeps it when the player has none left (the cell
    ///     then shows the material dimmed). Pure: no Unity, unit-tested.
    ///
    ///     Capacity is never reduced (user: "do not silently reduce currency capacity"): every cell still holds an
    ///     item. A material's first stack sits in its home cell; further stacks, and anything without a home, go to the
    ///     cells no material owns, and when those are full, to the home cells of materials the player has none of.
    ///     Only positions change, never items or counts, so arranging can't lose or duplicate anything.
    /// </summary>
    internal static class FixedLayout
    {
        /// <summary>
        ///     The home cell (row-major index) of each material in <paramref name="homes" />, in that order, within a
        ///     tab of <paramref name="cells" /> cells; -1 for any past the end. Cell <c>i</c> is the home of material <c>i</c>.
        /// </summary>
        public static int HomeOf(IList<string> homes, string key, int cells)
        {
            if (homes == null || key == null)
            {
                return -1;
            }
            int index = homes.IndexOf(key);
            return index >= 0 && index < cells ? index : -1;
        }

        /// <summary>
        ///     Where each item should be. Items are given in parallel lists: <paramref name="keys" /> (prefab names),
        ///     <paramref name="cells" /> (current row-major cell), <paramref name="stacks" /> (stack sizes, the biggest
        ///     stack of a material goes home) and <paramref name="pinned" /> (items that mustn't move, such as one being
        ///     dragged). Returns the new cell for each item. Items that already sit well don't move.
        /// </summary>
        public static int[] Arrange(int width, int height, IList<string> homes, IList<string> keys, IList<int> cells, IList<int> stacks,
            IList<bool> pinned)
        {
            int total = width * height;
            int count = keys.Count;
            var result = new int[count];
            var taken = new bool[total];
            var placed = new bool[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = cells[i];
                if (pinned != null && pinned[i] && InRange(cells[i], total))
                {
                    taken[cells[i]] = true;
                    placed[i] = true;
                }
            }

            // Materials the player has some of: their home cells are spoken for.
            var present = new bool[total];
            for (int i = 0; i < count; i++)
            {
                int home = HomeOf(homes, keys[i], total);
                if (home >= 0)
                {
                    present[home] = true;
                }
            }

            // 1. Each present material's biggest stack goes home (one already there stays, even if smaller).
            for (int home = 0; home < total; home++)
            {
                if (!present[home] || taken[home])
                {
                    continue;
                }
                int best = -1;
                for (int i = 0; i < count; i++)
                {
                    if (placed[i] || HomeOf(homes, keys[i], total) != home)
                    {
                        continue;
                    }
                    if (cells[i] == home)
                    {
                        best = i;
                        break;
                    }
                    if (best < 0 || stacks[i] > stacks[best])
                    {
                        best = i;
                    }
                }
                if (best >= 0)
                {
                    result[best] = home;
                    taken[home] = true;
                    placed[best] = true;
                }
            }

            // 2. Everything else keeps its cell when that cell is free and no material owns it.
            int owned = homes == null ? 0 : homes.Count;
            for (int i = 0; i < count; i++)
            {
                int cell = cells[i];
                if (!placed[i] && InRange(cell, total) && cell >= owned && !taken[cell])
                {
                    taken[cell] = true;
                    placed[i] = true;
                }
            }

            // 3. The rest go to free cells no material owns, so the homes of materials the player has none of stay
            //    visible, dimmed.
            for (int i = 0; i < count; i++)
            {
                if (!placed[i])
                {
                    int cell = FirstFree(taken, homes, total, ownedOnly: false);
                    if (cell >= 0)
                    {
                        result[i] = cell;
                        taken[cell] = true;
                        placed[i] = true;
                    }
                }
            }

            // 4. Only when those are full: the homes of materials the player has none of (the item's own cell first),
            //    so no capacity is lost; and if even that fails (an over-full tab, which the game doesn't allow), it
            //    stays where it was.
            for (int i = 0; i < count; i++)
            {
                if (placed[i])
                {
                    continue;
                }
                int cell = cells[i];
                if (!(InRange(cell, total) && !taken[cell] && !present[cell]))
                {
                    cell = FirstFree(taken, homes, total, ownedOnly: true, present: present);
                }
                if (cell >= 0)
                {
                    result[i] = cell;
                    taken[cell] = true;
                }
                placed[i] = true;
            }
            return result;
        }

        private static bool InRange(int cell, int total) => cell >= 0 && cell < total;

        private static int FirstFree(bool[] taken, IList<string> homes, int total, bool ownedOnly, bool[] present = null)
        {
            int owned = homes == null ? 0 : homes.Count;
            for (int cell = 0; cell < total; cell++)
            {
                if (taken[cell])
                {
                    continue;
                }
                bool isHome = cell < owned;
                if (!ownedOnly && !isHome)
                {
                    return cell;
                }
                if (ownedOnly && isHome && (present == null || !present[cell]))
                {
                    return cell;
                }
            }
            return -1;
        }
    }
}
