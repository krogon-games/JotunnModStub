using System;
using System.Collections.Generic;
using System.IO;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class CorruptionOutcomesTests
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

        private const string Header = "outcome,status,chance\n";

        [Fact]
        public void RealFileMatchesTheAgreedOdds()
        {
            var errors = new List<string>();
            var table = CorruptionOutcomes.Parse(DataFile("corruption_outcomes.csv"), errors);
            Assert.True(errors.Count == 0, "corruption_outcomes.csv has problems:\n" + string.Join("\n", errors));
            Assert.Equal(25f, table.Percent["bricked"]);
            Assert.Equal(25f, table.Percent["unchanged"]);
            Assert.Equal(50f, table.Percent["affix"]);
        }

        [Fact]
        public void RollFollowsTheChances()
        {
            var errors = new List<string>();
            var table = CorruptionOutcomes.Parse(Header + "bricked,approved,25\nunchanged,approved,25\naffix,approved,50\n", errors);
            Assert.Empty(errors);
            Assert.Equal("bricked", table.Roll(() => 0.0));
            Assert.Equal("bricked", table.Roll(() => 0.2499));
            Assert.Equal("unchanged", table.Roll(() => 0.25));
            Assert.Equal("unchanged", table.Roll(() => 0.4999));
            Assert.Equal("affix", table.Roll(() => 0.5));
            Assert.Equal("affix", table.Roll(() => 0.9999));
        }

        [Fact]
        public void ChancesMustAddUpTo100()
        {
            var errors = new List<string>();
            CorruptionOutcomes.Parse(Header + "bricked,approved,25\nunchanged,approved,25\naffix,approved,40\n", errors);
            Assert.Contains(errors, e => e.Contains("add up to 90"));
        }

        [Fact]
        public void ReportsMissingDuplicateAndUnknownOutcomes()
        {
            var errors = new List<string>();
            CorruptionOutcomes.Parse(Header + "bricked,approved,25\nbricked,approved,25\nlucky,approved,50\n", errors);
            Assert.Contains(errors, e => e.Contains("listed twice"));
            Assert.Contains(errors, e => e.Contains("outcome must be one of"));
            Assert.Contains(errors, e => e.Contains("no row for unchanged"));
            Assert.Contains(errors, e => e.Contains("no row for affix"));
        }

        [Fact]
        public void ReportsBadChanceAndStatus()
        {
            var errors = new List<string>();
            CorruptionOutcomes.Parse(Header + "bricked,maybe,25\nunchanged,approved,-5\naffix,approved,abc\n", errors);
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("chance '-5'"));
            Assert.Contains(errors, e => e.Contains("chance 'abc'"));
        }
    }
}
