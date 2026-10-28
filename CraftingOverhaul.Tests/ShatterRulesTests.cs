using System;
using System.Collections.Generic;
using System.IO;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ShatterRulesTests
    {
        private static string DataFile(string name)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", name)))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir, "Corruption", "Data", name));
        }

        private const string Header = "setting,status,value\n";

        [Fact]
        public void RealFileMatchesTheAgreedChance()
        {
            var errors = new List<string>();
            var rules = ShatterRules.Parse(DataFile("shattering.csv"), errors);
            Assert.True(errors.Count == 0, "shattering.csv has problems:\n" + string.Join("\n", errors));
            Assert.Equal(50f, rules.SplinterChance);
        }

        [Fact]
        public void ReportsBadRows()
        {
            var errors = new List<string>();
            ShatterRules.Parse(Header + "splinter_chance,maybe,150\nluck,approved,5\n", errors);
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("value '150'"));
            Assert.Contains(errors, e => e.Contains("setting must be one of"));

            errors.Clear();
            ShatterRules.Parse(Header + "luck,approved,5\n", errors);
            Assert.Contains(errors, e => e.Contains("no row for splinter_chance"));
        }

        [Fact]
        public void EachLineRollsAgainstTheChance()
        {
            var errors = new List<string>();
            var rules = ShatterRules.Parse(Header + "splinter_chance,approved,50\n", errors);
            Assert.Empty(errors);
            var values = new[] { 0.0, 0.4999, 0.5, 0.9999 };
            int i = 0;
            Assert.Equal(new[] { true, true, false, false }, rules.RollSplinters(4, () => values[i++]));
            Assert.Empty(rules.RollSplinters(0, () => 0.0));
        }

        [Fact]
        public void OnlyMagicAndRareItemsShatter()
        {
            Assert.True(ShatterRules.CanShatter("magic", false));
            Assert.True(ShatterRules.CanShatter("rare", false));
            Assert.False(ShatterRules.CanShatter("rare", true));
            Assert.False(ShatterRules.CanShatter("unique", false));
            Assert.False(ShatterRules.CanShatter("normal", false));
        }

        [Fact]
        public void SplintersStopAtTwoAffixes()
        {
            Assert.True(ShatterRules.CanTakeSplinter("normal", false, 0, false));
            Assert.True(ShatterRules.CanTakeSplinter("magic", false, 1, false));
            Assert.False(ShatterRules.CanTakeSplinter("magic", false, 2, false));
            Assert.False(ShatterRules.CanTakeSplinter("rare", false, 0, false));
            Assert.False(ShatterRules.CanTakeSplinter("unique", false, 0, false));
            Assert.False(ShatterRules.CanTakeSplinter("magic", true, 1, false));
        }

        [Fact]
        public void ResonantSplinterNeedsAnItemWithNoAffixes()
        {
            Assert.True(ShatterRules.CanTakeSplinter("normal", false, 0, true));
            Assert.False(ShatterRules.CanTakeSplinter("magic", false, 1, true));
        }
    }
}
