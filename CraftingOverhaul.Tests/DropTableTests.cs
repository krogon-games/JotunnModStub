using System;
using System.Collections.Generic;
using System.IO;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class DropTableTests
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

        private static DropTable ParseReal()
        {
            var errors = new List<string>();
            var table = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.True(errors.Count == 0, "The drop tables have problems:\n" + string.Join("\n", errors));
            return table;
        }

        [Fact]
        public void RealFilesHaveNoProblems()
        {
            var table = ParseReal();
            Assert.Equal(DropTable.Rarities.Length, table.OneIn.Count);
            Assert.NotEmpty(table.Modifiers);
        }

        [Fact]
        public void BaseChancesMatchTheAgreedRates()
        {
            var table = ParseReal();
            Assert.Equal(1f / 50f, table.Chance("magic", null), 6);
            Assert.Equal(1f / 100f, table.Chance("rare", null), 6);
            Assert.Equal(1f / 2500f, table.Chance("unique", null), 6);
        }

        private const string Chances = "rarity,status,one_in\nmagic,approved,50\nrare,approved,100\nunique,approved,2500";

        [Fact]
        public void ModifiersMultiplyAndBlankMeansNoChange()
        {
            var errors = new List<string>();
            var table = DropTable.Parse(Chances,
                "source,status,magic,rare,unique\nstar_2,approved,,,1.5\nscar,proposed,2,2,\n", errors);
            Assert.Empty(errors);
            Assert.Equal(1.5f / 2500f, table.Chance("unique", new[] { "star_2", "scar" }), 8);
            Assert.Equal(2f / 50f, table.Chance("magic", new[] { "star_2", "scar" }), 6);
            Assert.Equal(1f / 100f, table.Chance("rare", new[] { "unknown_source" }), 6);
        }

        [Fact]
        public void ChanceIsCappedAtCertain()
        {
            var errors = new List<string>();
            var table = DropTable.Parse("rarity,status,one_in\nmagic,approved,2\nrare,approved,100\nunique,approved,2500",
                "source,status,magic,rare,unique\nhuge,draft,10,,\n", errors);
            Assert.Empty(errors);
            Assert.Equal(1f, table.Chance("magic", new[] { "huge" }));
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            DropTable.Parse(
                "rarity,status,one_in\nmagic,approved,0.5\nmagic,approved,50\nepic,maybe,10",
                "source,status,magic,rare,unique\nStar 2,approved,,,1.5\nscar,approved,0,x,\nscar,approved,,,",
                errors);
            Assert.Contains(errors, e => e.Contains("one_in must be 1 or more"));
            Assert.Contains(errors, e => e.Contains("rarity is listed twice"));
            Assert.Contains(errors, e => e.Contains("rarity must be one of"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("no row for rare"));
            Assert.Contains(errors, e => e.Contains("lowercase letters"));
            Assert.Contains(errors, e => e.Contains("must be above 0"));
            Assert.Contains(errors, e => e.Contains("'x' is not a number"));
            Assert.Contains(errors, e => e.Contains("source is listed twice"));
        }
    }
}
