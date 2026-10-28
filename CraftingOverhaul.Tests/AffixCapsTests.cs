using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class AffixCapsTests
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

        [Fact]
        public void RealFileHasNoProblemsAndOnlyNamesRealFamilies()
        {
            var errors = new List<string>();
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors);
            Assert.True(errors.Count == 0, "Problems:\n" + string.Join("\n", errors));

            var families = new HashSet<string>(affixes.Select(a => a.Family));
            foreach (var cap in caps.Caps)
            {
                foreach (var family in cap.Families)
                {
                    Assert.True(families.Contains(family), $"affix_caps.csv: {cap.Id} names family '{family}', which isn't in affixes.csv.");
                }
                var units = affixes.Where(a => cap.Families.Contains(a.Family)).Select(a => a.Unit).Distinct().ToList();
                Assert.True(units.Count == 1 && units[0] == cap.Unit, $"{cap.Id}: unit {cap.Unit} doesn't match its affixes ({string.Join(", ", units)}).");
            }
        }

        [Fact]
        public void MovementIsCappedAt35()
        {
            var errors = new List<string>();
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            Assert.Empty(errors);
            Assert.Equal(35f, caps.Limit("held_movement", 50f));
            Assert.Equal(20f, caps.Limit("foraging", 20f));
            Assert.Equal(999f, caps.Limit("enhanced_damage", 999f));
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            AffixCaps.Parse("cap,status,max,unit,families\nspeed,approved,-5,%,a;b\nspeed,maybe,ten,%,b\nempty,approved,5,%,", errors);
            Assert.Contains(errors, e => e.Contains("must be a number above 0"));
            Assert.Contains(errors, e => e.Contains("used twice"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("already limited by speed"));
            Assert.Contains(errors, e => e.Contains("list the affix families"));
        }
    }
}
