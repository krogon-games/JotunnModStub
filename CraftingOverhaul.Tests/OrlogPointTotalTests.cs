using System;
using System.Collections.Generic;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Saved stat and skill point totals that a damaged or edited save could make huge: reading, totals and spending
    ///     checks don't throw or wrap, and spent points are never refunded or removed.
    /// </summary>
    public class OrlogPointTotalTests
    {
        [Fact]
        public void TotalsSaturateInsteadOfThrowing()
        {
            Assert.Equal(int.MaxValue, AttributeRules.Total(new[] { int.MaxValue, int.MaxValue, 5, 1 }));
            Assert.Equal(0, AttributeRules.Total(new[] { -5, int.MinValue, 0 }));
            Assert.Equal(0, AttributeRules.Total(null));
            Assert.Equal(12, AttributeRules.Total(new[] { 5, -3, 7 }));
        }

        [Fact]
        public void HugeSpentTotalsLeaveNothingToSpend()
        {
            Assert.Equal(0, AttributeRules.Unspent(99, 5, new[] { int.MaxValue, int.MaxValue, 0, 0 }));
            var skills = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Jab"] = int.MaxValue, ["Dodge"] = int.MaxValue };
            Assert.Equal(0, SkillRules.Unspent(99, 1, skills));
            Assert.Equal(98, SkillRules.Unspent(99, 1, null));   // nothing saved: all earned points to spend
        }

        [Fact]
        public void EarnedPointsSaturate()
        {
            Assert.Equal(int.MaxValue, AttributeRules.Earned(int.MaxValue, int.MaxValue));
            Assert.Equal(0, AttributeRules.Earned(int.MinValue, 5));
            Assert.Equal(0, AttributeRules.Earned(50, -5));
        }

        [Fact]
        public void LoweredSettingsKeepTheSpentPoints()
        {
            // 50 spent, the setting lowered so only 10 are earned: nothing to spend, and the 50 stay spent.
            var spent = new[] { 20, 10, 15, 5 };
            Assert.Equal(0, AttributeRules.Unspent(11, 1, spent));
            Assert.Equal(new[] { 20, 10, 15, 5 }, spent);
            var points = SkillRules.Parse("Jab:20;Dodge:5");
            Assert.Equal(0, SkillRules.Unspent(11, 1, points));
            Assert.Equal("Dodge:5;Jab:20", SkillRules.Format(points));
        }

        [Theory]
        [InlineData("12", 12)]
        [InlineData(" 7 ", 7)]
        [InlineData("+3", 3)]
        [InlineData("2147483647", int.MaxValue)]
        [InlineData("2147483648", int.MaxValue)]
        [InlineData("99999999999999999999999999", int.MaxValue)]
        public void SavedCountsTooBigForAnIntReadAsTheMost(string text, int expected)
        {
            Assert.True(AttributeRules.TryParsePoints(text, out int points));
            Assert.Equal(expected, points);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-4")]
        [InlineData("-99999999999999999999")]
        [InlineData("000")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("ten")]
        [InlineData("1.5")]
        public void SavedCountsThatArentPointsAreIgnored(string text)
        {
            Assert.False(AttributeRules.TryParsePoints(text, out int points));
            Assert.Equal(0, points);
        }

        [Fact]
        public void HugeSavedSkillsStayAndBlockFurtherSpending()
        {
            var points = SkillRules.Parse("Jab:99999999999;Dodge:2");
            Assert.Equal(int.MaxValue, points["Jab"]);
            Assert.Equal(2, points["Dodge"]);
            Assert.Equal(0, SkillRules.Unspent(30, 1, points));
        }

        [Fact]
        public void AdditionHoldsItsRange()
        {
            Assert.Equal(int.MaxValue, AttributeRules.AddClamped(int.MaxValue, 25));
            Assert.Equal(int.MinValue, AttributeRules.AddClamped(int.MinValue, -1));
            Assert.Equal(45, AttributeRules.AddClamped(20, 25));
        }

        [Fact]
        public void HugeSkillPointsDontBreakTheSpendingCheck()
        {
            // A level-30 skill holding int.MaxValue - 1 points: the pace rule's "needs level" (30 + points) doesn't wrap.
            var errors = new List<string>();
            var table = SkillTable.Parse(
                "id,name,tab,row,col,req_level,prereqs,status,kind,needs,eitr,eitr_per_level,description\n" +
                "Fury,Fury,0,6,1,30,,v1,attack,spear,2,0.25,A test skill.\n", "test.csv", errors);
            Assert.Empty(errors);
            var skill = table.Find("Fury");
            var points = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Fury"] = int.MaxValue - 1 };
            Assert.Equal("Needs level 2147483676", SkillRules.SpendBlocked(table, skill, 30, points, 5, int.MaxValue, true));
            Assert.Equal("Maxed", SkillRules.SpendBlocked(table, skill, 30, points, 5, 20, true));
            Assert.Equal("No skill points", SkillRules.SpendBlocked(table, skill, 30, points, SkillRules.Unspent(30, 1, points), int.MaxValue, false));
        }
    }
}
