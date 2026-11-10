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

        [Fact]
        public void DiffFindsTheNewAndTheGoneLines()
        {
            ForgeRules.Diff(new[] { "a=1", "b=2" }, new[] { "a=1", "b=2", "c=3" }, out var added, out var removed);
            Assert.Equal(new[] { 2 }, added);
            Assert.Empty(removed);

            ForgeRules.Diff(new[] { "a=1", "b=2", "c=3" }, new[] { "a=1", "c=3" }, out added, out removed);
            Assert.Empty(added);
            Assert.Equal(new[] { 1 }, removed);

            // A reroll: the same affix with a new value is one added and one removed.
            ForgeRules.Diff(new[] { "a=1", "b=2" }, new[] { "a=5", "b=2" }, out added, out removed);
            Assert.Equal(new[] { 0 }, added);
            Assert.Equal(new[] { 0 }, removed);
        }

        [Fact]
        public void DiffCountsIdenticalLinesSeparately()
        {
            ForgeRules.Diff(new[] { "a=1" }, new[] { "a=1", "a=1" }, out var added, out var removed);
            Assert.Equal(new[] { 1 }, added);
            Assert.Empty(removed);
            ForgeRules.Diff(null, null, out added, out removed);
            Assert.Empty(added);
            Assert.Empty(removed);
        }

        [Theory]
        [InlineData("Regal Orb", "magic", "rare", 1, 0, "Regal Orb: now Rare, 1 affix added")]
        [InlineData("Orb of Transmutation", "normal", "magic", 1, 0, "Orb of Transmutation: now Magic, 1 affix added")]
        [InlineData("Divine Orb", "rare", "rare", 3, 3, "Divine Orb: 3 affixes rerolled")]
        [InlineData("Orb of Annulment", "rare", "rare", 0, 1, "Orb of Annulment: 1 affix removed")]
        [InlineData("Orb of Annulment", "rare", "rare", 1, 2, "Orb of Annulment: 1 affix rerolled, 1 affix removed")]
        [InlineData("Exalted Orb", "rare", "rare", 0, 0, "Exalted Orb: no change")]
        [InlineData("", "rare", "rare", 2, 0, "2 affixes added")]
        public void TheHeadlineIsShort(string material, string before, string after, int added, int removed, string expected)
        {
            Assert.Equal(expected, ForgeRules.Summary(material, before, after, added, removed));
        }

        [Fact]
        public void AffixesGetABulletAndAHangingIndent()
        {
            Assert.Equal("<size=125%><color=#C8B878>•</color></size><indent=1.1em><color=#8888FF>+12% attack speed [5–15]</color></indent>",
                ForgeRules.BulletLine("<color=#8888FF>+12% attack speed [5–15]</color>", false));
        }

        [Fact]
        public void AResonantAffixIsGreenWithADiamondInsteadOfTheWord()
        {
            string line = ForgeRules.BulletLine("<color=#FFFF77>+30% fire damage [10–60] ◆ resonant</color>", true);
            Assert.StartsWith("<size=70%><color=#5BD16A>◆</color></size>", line);
            Assert.DoesNotContain("resonant", line);
            Assert.DoesNotContain("#FFFF77", line);
            Assert.Contains("<color=#5BD16A>+30% fire damage [10–60]</color>", line);
        }

        [Fact]
        public void AnInactiveResonantAffixStaysGrey()
        {
            string line = ForgeRules.BulletLine("<color=#808080>+30% fire damage ◆ resonant (not active yet)</color>", true);
            Assert.Contains("<color=#808080>+30% fire damage (not active yet)</color>", line);
        }

        [Fact]
        public void TheMarksAreCentred()
        {
            Assert.Equal("\n<align=center><color=#E04040>Corrupted</color></align>", ForgeRules.CentredMarks("\n<color=#E04040>Corrupted</color>"));
            Assert.Equal("", ForgeRules.CentredMarks(""));
            Assert.Equal("", ForgeRules.CentredMarks(null));
        }
    }
}
