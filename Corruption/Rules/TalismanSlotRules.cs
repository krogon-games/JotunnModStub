using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Which talismans count as worn, from the contents of the talisman slots. No game dependencies, so it is
    ///     unit-tested. Talismans are identified by their prefab hash; 0 means an empty slot.
    /// </summary>
    internal static class TalismanSlotRules
    {
        /// <summary>A talisman can go into a slot unless the same talisman already sits in another one (a copy adds nothing).</summary>
        public static bool CanSlot(int talisman, IList<int> otherSlots)
        {
            return talisman != 0 && !otherSlots.Contains(talisman);
        }

        /// <summary>
        ///     The talismans that work: the first <paramref name="activeSlots" /> slots, empty ones and copies skipped,
        ///     padded with 0 to <paramref name="length" /> so the result can be written to fixed keys.
        /// </summary>
        public static int[] Worn(IList<int> slots, int activeSlots, int length)
        {
            var worn = new int[length];
            int count = 0;
            for (int i = 0; i < slots.Count && i < activeSlots && count < length; i++)
            {
                int talisman = slots[i];
                if (talisman != 0 && System.Array.IndexOf(worn, talisman, 0, count) < 0)
                {
                    worn[count++] = talisman;
                }
            }
            return worn;
        }
    }
}
