using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class TalismanSlotRulesTests
    {
        [Fact]
        public void ACopyCantGoIntoASecondSlot()
        {
            Assert.True(TalismanSlotRules.CanSlot(11, new[] { 0, 22 }));
            Assert.False(TalismanSlotRules.CanSlot(22, new[] { 0, 22 }));
            Assert.False(TalismanSlotRules.CanSlot(0, new int[0]));
        }

        [Fact]
        public void WornSkipsEmptySlotsAndCopies()
        {
            Assert.Equal(new[] { 11, 22, 0 }, TalismanSlotRules.Worn(new[] { 0, 11, 11, 22 }, 4, 3));
        }

        [Fact]
        public void OnlyActiveSlotsCount()
        {
            // Server set 2 slots: whatever sits in a third doesn't work.
            Assert.Equal(new[] { 11, 22, 0 }, TalismanSlotRules.Worn(new[] { 11, 22, 33 }, 2, 3));
            Assert.Equal(new[] { 0, 0, 0 }, TalismanSlotRules.Worn(new[] { 11, 22 }, 0, 3));
        }
    }
}
