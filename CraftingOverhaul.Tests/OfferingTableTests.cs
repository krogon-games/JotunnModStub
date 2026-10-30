using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OfferingTableTests
    {
        private static string Root()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "offerings.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static string Dump(string file) => Path.Combine(Root(), "data", "game-dump-1.0.20", file);

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            var rows = Csv.ReadRows(File.ReadAllText(path));
            var header = rows[0];
            return rows.Skip(1).Where(r => r.Count == header.Count)
                .Select(r => header.Select((h, i) => new KeyValuePair<string, string>(h, r[i])).ToDictionary(k => k.Key, k => k.Value))
                .ToList();
        }

        private static List<OfferingTable.Offering> ParseReal()
        {
            var errors = new List<string>();
            var offerings = OfferingTable.Parse(File.ReadAllText(Path.Combine(Root(), "Corruption", "Data", "offerings.csv")), errors);
            Assert.True(errors.Count == 0, "offerings.csv has problems:\n" + string.Join("\n", errors));
            return offerings;
        }

        [Fact]
        public void OneOfferingPerEssenceBoss()
        {
            // Every boss with an essence can be corrupted; Kall is summoned as FrozenKing and his essence drops from FrozenKing_p3.
            var errors = new List<string>();
            var bosses = EssenceTable.Parse(File.ReadAllText(Path.Combine(Root(), "Corruption", "Data", "essences.csv")), errors)
                .Select(e => e.Boss == "FrozenKing_p3" ? "FrozenKing" : e.Boss).OrderBy(b => b);
            Assert.Equal(bosses, ParseReal().Select(o => o.Boss).OrderBy(b => b));
        }

        [Fact]
        public void MatchesTheGamesAltars()
        {
            var altars = ReadCsv(Dump("altars.csv"));
            var items = new HashSet<string>(ReadCsv(Dump("items.csv")).Select(i => i["prefab"]));
            foreach (var o in ParseReal())
            {
                Assert.Contains(o.Item, items);
                if (o.Use == OfferingTable.UseDoor)
                {
                    continue;   // the Queen's door isn't an altar
                }
                var altar = altars.FirstOrDefault(a => a["boss"] == o.Boss);
                Assert.True(altar != null, $"{o.Boss} has no altar in altars.csv");
                if (o.Use == OfferingTable.UseAltar)
                {
                    Assert.Equal("False", altar["use_item_stands"]);
                    Assert.Equal(o.Item, altar["offering"]);
                    Assert.Equal(int.Parse(altar["offering_count"]), o.Amount);
                }
                else
                {
                    Assert.Equal("True", altar["use_item_stands"]);
                    Assert.Contains($"[{o.Item}]", altar["item_stands"]);
                }
            }
        }

        [Fact]
        public void EveryOfferingHasItsIcon()
        {
            foreach (var o in ParseReal())
            {
                string file = Path.Combine(Root(), "Corruption", "Assets", $"offering_icon_{o.Item}.rgba");
                Assert.True(File.Exists(file), $"missing {file}");
                Assert.Equal(8 + 128 * 128 * 4, new FileInfo(file).Length);
            }
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            OfferingTable.Parse(
                "boss,item,use,amount,name,description\n" +
                "Eikthyr,TrophyDeer,offer,2,Name,Desc\n" +
                "Dragon,DragonEgg,stand,3,Name,Desc\n" +
                "Dragon,DragonEgg,stand,1,,Desc\n" +
                "Bonemass,WitheredBone,altar,none,Name,Desc\n", errors);
            Assert.Contains(errors, e => e.Contains("use 'offer'"));
            Assert.Contains(errors, e => e.Contains("amount must be 1"));
            Assert.Contains(errors, e => e.Contains("listed twice"));
            Assert.Contains(errors, e => e.Contains("name can't be empty"));
            Assert.Contains(errors, e => e.Contains("whole number"));
        }
    }
}
