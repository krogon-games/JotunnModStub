using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ForgeRulesTests
    {
        [Theory]
        [InlineData("shatter", true)]
        [InlineData("corrupt", true)]
        [InlineData("orb:regal", false)]
        [InlineData("orb:mirror", false)]
        [InlineData("splinter", false)]
        [InlineData("map:roll", false)]
        [InlineData(null, false)]
        public void OnlyDestructiveOperationsAskFirst(string id, bool expected)
        {
            Assert.Equal(expected, ForgeRules.NeedsConfirmation(id));
        }

        [Theory]
        // left, dragging, picked, target
        [InlineData(true, true, false, false, "Insert")]
        [InlineData(true, true, true, true, "Insert")]
        [InlineData(true, false, true, true, "Apply")]
        [InlineData(true, false, true, false, "Nothing")]
        [InlineData(true, false, false, true, "Nothing")]
        [InlineData(false, false, true, true, "CancelPick")]
        [InlineData(false, false, false, true, "Remove")]
        [InlineData(false, false, false, false, "Nothing")]
        public void TargetSlotClicks(bool left, bool dragging, bool picked, bool target, string expected)
        {
            Assert.Equal((ForgeClick)System.Enum.Parse(typeof(ForgeClick), expected), ForgeRules.TargetClick(left, dragging, picked, target));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void ThePickStaysOnlyWithShiftAndMoreLeft(bool shift, bool more, bool expected)
        {
            Assert.Equal(expected, ForgeRules.KeepPick(shift, more));
        }

        [Fact]
        public void AClickUsesTheFirstAllowedOperation()
        {
            Assert.Equal(1, ForgeRules.PickOperation(new[] { false, true, true }));
            Assert.Equal(-1, ForgeRules.PickOperation(new[] { false, false }));
            Assert.Equal(-1, ForgeRules.PickOperation(new bool[0]));
            Assert.Equal(-1, ForgeRules.PickOperation(null));
        }

        [Theory]
        [InlineData("CRAFT", "Forge", "FORGE")]
        [InlineData("Craft", "Forge", "Forge")]
        [InlineData("", "Remove", "Remove")]
        [InlineData(null, "Remove", "Remove")]
        [InlineData("123", "Remove", "Remove")]
        public void LabelsFollowTheGamesCapitals(string game, string label, string expected)
        {
            Assert.Equal(expected, ForgeRules.UpperLike(game, label));
        }
    }
}
