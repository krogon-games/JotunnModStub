using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class AffixTableTests
    {
        private const string Header =
            "id,status,pool,side,family,name,item_name,text,min,max,unit,weight,effect," +
            "weapon,shield,helmet,chest,legs,cape,jewellery,pickaxe,axe_1h,notes";

        private static string RealFile()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "affixes.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir, "Corruption", "Data", "affixes.csv"));
        }

        private static List<AffixTable.Affix> ParseReal()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(RealFile(), errors);
            Assert.True(errors.Count == 0, "affixes.csv has problems:\n" + string.Join("\n", errors));
            return affixes;
        }

        [Fact]
        public void RealFileHasNoProblems()
        {
            Assert.NotEmpty(ParseReal());
        }

        [Fact]
        public void PickaxesRollOnlyGatheringAffixes()
        {
            var affixes = ParseReal();
            var pickaxe = new[] { "pickaxe" };
            var all = AffixTable.Candidates(affixes, pickaxe, true).Concat(AffixTable.Candidates(affixes, pickaxe, false));
            Assert.All(all, a => Assert.StartsWith("gathering-", a.Pool));
            Assert.DoesNotContain(all, a => a.Pool == "gathering-woodcutting");
        }

        [Fact]
        public void OneHandedAxesRollGatheringAndWeaponAffixes()
        {
            var affixes = ParseReal();
            var axe = new[] { "weapon", "axe_1h" };
            var pools = AffixTable.Candidates(affixes, axe, true).Concat(AffixTable.Candidates(affixes, axe, false))
                .Select(a => a.Pool).Distinct().ToList();
            Assert.Contains("gathering-universal", pools);
            Assert.Contains("gathering-woodcutting", pools);
            Assert.Contains("gear", pools);
            Assert.DoesNotContain("gathering-mining", pools);
        }

        [Fact]
        public void TwoHandedAxesRollOnlyWeaponAffixes()
        {
            var affixes = ParseReal();
            var battleaxe = new[] { "weapon" };
            Assert.All(AffixTable.Candidates(affixes, battleaxe, true), a => Assert.Equal("gear", a.Pool));
            Assert.All(AffixTable.Candidates(affixes, battleaxe, false), a => Assert.Equal("gear", a.Pool));
        }

        [Fact]
        public void ReadsQuotedCellsAndDecimals()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(
                Header + "\r\nkeen,approved,gathering-universal,suffix,pickup,\"Keen, eyed\",of Eyes,+{0} m,0.5,2,m,100,pickup_range,,,,,,,,x,x,\"say \"\"hi\"\"\"",
                errors);
            Assert.Empty(errors);
            var a = Assert.Single(affixes);
            Assert.Equal("Keen, eyed", a.Name);
            Assert.Equal(0.5f, a.Min);
            Assert.Equal("say \"hi\"", a.Notes);
            Assert.True(a.Classes.SetEquals(new[] { "pickaxe", "axe_1h" }));
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            AffixTable.Parse(
                Header + "\n" +
                "dup,approved,gathering-mining,prefix,f1,A,A,+{0}%,20,10,%,50,e,,,,,,,,x,,\n" +
                "dup,maybe,gathering-mining,middle,f1,B,B,no value,1,2,flat,0,e,,,,,,,,yes,,\n" +
                "Bad Id,approved,gathering-mining,prefix,f2,C,C,+{0}%,1,two,%,50,e,x,,,,,,,,,",
                errors);
            Assert.Contains(errors, e => e.Contains("min is larger than max"));
            Assert.Contains(errors, e => e.Contains("id is used twice"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("side 'middle'"));
            Assert.Contains(errors, e => e.Contains("needs {0}"));
            Assert.Contains(errors, e => e.Contains("weight must be above 0"));
            Assert.Contains(errors, e => e.Contains("should be x or empty"));
            Assert.Contains(errors, e => e.Contains("mixes units"));
            Assert.Contains(errors, e => e.Contains("lowercase letters"));
            Assert.Contains(errors, e => e.Contains("'two' is not a number"));
            Assert.Contains(errors, e => e.Contains("can't roll on weapon"));
        }

        [Fact]
        public void ReportsMissingColumns()
        {
            var errors = new List<string>();
            AffixTable.Parse("id,status\nx,approved", errors);
            Assert.Contains(errors, e => e.Contains("Missing column 'side'"));
        }
    }
}
