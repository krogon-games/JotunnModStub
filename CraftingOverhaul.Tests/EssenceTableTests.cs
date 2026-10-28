using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class EssenceTableTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "essences.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static List<EssenceTable.Essence> ParseReal()
        {
            var errors = new List<string>();
            var essences = EssenceTable.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Corruption", "Data", "essences.csv")), errors);
            Assert.True(errors.Count == 0, "essences.csv has problems:\n" + string.Join("\n", errors));
            return essences;
        }

        [Fact]
        public void RealFileHasNoProblems()
        {
            Assert.Equal(8, ParseReal().Count);
        }

        [Fact]
        public void EveryBossIsABossInTheGameData()
        {
            var bosses = new HashSet<string>(File.ReadLines(Path.Combine(RepoRoot(), "data", "game-dump-1.0.20", "creatures.csv"))
                .Skip(1).Select(l => l.Split(',')).Where(c => c.Length > 5 && c[5] == "True").Select(c => c[0]));
            foreach (var e in ParseReal())
            {
                Assert.True(bosses.Contains(e.Boss), $"{e.Boss} isn't a boss in the 1.0.20 game data.");
            }
        }

        [Fact]
        public void EveryLookHasItsModelFiles()
        {
            string assets = Path.Combine(RepoRoot(), "Corruption", "Assets");
            Assert.True(File.Exists(Path.Combine(assets, "essence.obj")));
            foreach (var e in ParseReal())
            {
                Assert.True(File.Exists(Path.Combine(assets, $"essence_{e.Look}_albedo.rgba")), $"missing essence_{e.Look}_albedo.rgba");
                Assert.True(File.Exists(Path.Combine(assets, $"essence_{e.Look}_emission.rgba")), $"missing essence_{e.Look}_emission.rgba");
                if (e.Addon.Length > 0)
                {
                    Assert.True(File.Exists(Path.Combine(assets, e.Addon + ".obj")), $"missing {e.Addon}.obj");
                }
            }
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            EssenceTable.Parse(
                "boss,status,look,name,glow,addon,addon_color,per_kill,maps_into,description\n" +
                "Eikthyr,approved,Eik Thyr,Essence,#GG0000,root,red,0,Black Forest,Desc\n" +
                "Eikthyr,maybe,eik,,#FF0000,,,1,Swamp,\n", errors);
            Assert.Contains(errors, e => e.Contains("look must be lowercase"));
            Assert.Contains(errors, e => e.Contains("glow '#GG0000'"));
            Assert.Contains(errors, e => e.Contains("addon_color 'red'"));
            Assert.Contains(errors, e => e.Contains("per_kill must be"));
            Assert.Contains(errors, e => e.Contains("listed twice"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("can't be empty"));
        }
    }
}
