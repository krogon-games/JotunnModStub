using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>Renamed affixes (Krogon, 2026-11-19, decisions 12-13): saves made before a rename keep working.</summary>
    public class AffixRenamesTests
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

        private static List<AffixTable.Affix> RealAffixes()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors);
            Assert.Empty(errors);
            return affixes;
        }

        [Fact]
        public void TheOrbFindersHaveTheirNewIdsAndNames()
        {
            var affixes = RealAffixes().ToDictionary(a => a.Id);
            Assert.Equal("Hoardfinder's", affixes["hoardfinders"].ItemName);
            Assert.Equal("orb_find_mining", affixes["hoardfinders"].Family);
            Assert.Equal("orb_find_mining", affixes["hoardfinders"].Effect);
            Assert.Equal("of Hidden Riches", affixes["hidden_riches"].ItemName);
            Assert.Equal("orb_find_woodcutting", affixes["hidden_riches"].Family);
            Assert.Equal("orb_find_woodcutting", affixes["hidden_riches"].Effect);
            Assert.False(affixes.ContainsKey("stonecutters"));
            Assert.False(affixes.ContainsKey("resinous"));
        }

        [Fact]
        public void EveryRenameLeadsToAnAffixThatExistsAndNoOldNameIsStillInUse()
        {
            var affixes = RealAffixes();
            Assert.Equal(AffixRenames.OldIds.Length, AffixRenames.NewIds.Length);
            Assert.Equal(AffixRenames.OldFamilies.Length, AffixRenames.NewFamilies.Length);
            foreach (var id in AffixRenames.NewIds)
            {
                Assert.Contains(affixes, a => a.Id == id);
            }
            foreach (var family in AffixRenames.NewFamilies)
            {
                Assert.Contains(affixes, a => a.Family == family);
            }
            Assert.DoesNotContain(affixes, a => AffixRenames.OldIds.Contains(a.Id) || AffixRenames.OldFamilies.Contains(a.Family));
        }

        [Fact]
        public void GearSavedWithTheOldIdsReadsAsTheNewOnes()
        {
            var saved = new Dictionary<string, string>
            {
                { LootItem.RarityKey, "magic" },
                { LootItem.AffixesKey, "stonecutters=15;swift=7" }
            };
            var item = LootItem.Read(saved);
            Assert.NotNull(item);
            Assert.Equal(new[] { "hoardfinders" }, item.Lines[0].Ids);
            Assert.Equal(15f, item.Lines[0].Value);
            Assert.Equal(new[] { "swift" }, item.Lines[1].Ids);
        }

        [Fact]
        public void SplintersSavedWithTheOldIdReadAsTheNewOne()
        {
            var saved = new Dictionary<string, string>();
            SplinterRules.Write(new LootItem.Line { Ids = new[] { "resinous" }, Value = 20f }, saved);
            var line = SplinterRules.Read(saved);
            Assert.Equal(new[] { "hidden_riches" }, line.Ids);
            Assert.Equal(20f, line.Value);
        }

        [Fact]
        public void TheOldSplinterItemNameIsKnownForTheRenamedFamily()
        {
            Assert.Equal("extra_stone", AffixRenames.OldFamily("orb_find_mining"));
            Assert.Equal("extra_resin", AffixRenames.OldFamily("orb_find_woodcutting"));
            Assert.Null(AffixRenames.OldFamily("gem_find"));
            Assert.Equal("CorruptionAffixSplinter_tool_extra_stone", SplinterLooks.PrefabFor("gathering", "extra_stone"));
        }

        [Fact]
        public void OtherIdsAreUnchanged()
        {
            Assert.Equal("swift", AffixRenames.Id("swift"));
            Assert.Equal("hoardfinders", AffixRenames.Id("hoardfinders"));
        }
    }
}
