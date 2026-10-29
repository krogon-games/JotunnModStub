using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class RipWaveTests
    {
        private static string Root()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "rip_loot.csv")))
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

        [Fact]
        public void EveryEssenceBossHasAnArmyInTheGameData()
        {
            var raids = ReadCsv(Dump("events.csv")).Select(e => new RipRules.RaidInfo
            {
                Name = e["m_name"],
                Enabled = e["m_enabled"] == "True",
                NotRequiredKeys = e["m_notRequiredGlobalKeys"].Split('|'),
                SpawnEntries = int.Parse(e["spawn_entries"])
            }).ToList();
            var defeatKeys = ReadCsv(Dump("creatures.csv")).ToDictionary(c => c["prefab"], c => c["defeat_key"]);
            var errors = new List<string>();
            var essences = EssenceTable.Parse(File.ReadAllText(Path.Combine(Root(), "Corruption", "Data", "essences.csv")), errors);
            var expected = new Dictionary<string, string[]>
            {
                { "Eikthyr", new[] { "army_eikthyr" } },
                { "gd_king", new[] { "army_theelder" } },
                { "Bonemass", new[] { "army_bonemass" } },
                { "Dragon", new[] { "army_moder" } },
                { "GoblinKing", new[] { "army_goblin" } },
                { "SeekerQueen", new[] { "army_gjall", "army_seekers" } },
                { "Fader", new[] { "army_charred", "army_charredspawners" } },
                { "FrozenKing_p3", new[] { "army_elakingar", "army_jotuns" } }
            };
            foreach (var essence in essences)
            {
                var armies = RipRules.ArmyRaids(raids, defeatKeys[essence.Boss]);
                Assert.Equal(expected[essence.Boss], armies);
            }
        }

        [Fact]
        public void PlayersLandOutsideEveryArmyRaidsRange()
        {
            // Waves start when a player is within the raid's range of the heart; the landing spot must be outside it.
            var armies = ReadCsv(Dump("events.csv")).Where(e => e["m_name"].StartsWith("army_")).ToList();
            Assert.NotEmpty(armies);
            Assert.All(armies, e => Assert.True(float.Parse(e["m_eventRange"], System.Globalization.CultureInfo.InvariantCulture) <= RipRules.HeartMinDistance,
                $"{e["m_name"]} has range {e["m_eventRange"]}, more than the heart's minimum distance {RipRules.HeartMinDistance}."));
        }

        [Fact]
        public void RipLootTableIsValidAndUsesRealChests()
        {
            var errors = new List<string>();
            var chests = RipLootTable.Parse(File.ReadAllText(Path.Combine(Root(), "Corruption", "Data", "rip_loot.csv")), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var real = new HashSet<string>(ReadCsv(Dump("containers.csv")).Select(c => c["prefab"]));
            Assert.All(chests, c => Assert.Contains(c.Prefab, real));
            foreach (var biome in MapList.Biomes)
            {
                Assert.Contains(chests, c => c.Biome == biome);
            }
        }

        [Fact]
        public void PickFollowsTheWeights()
        {
            var chests = new List<RipLootTable.Chest>
            {
                new RipLootTable.Chest { Biome = "Swamp", Prefab = "a", Weight = 3 },
                new RipLootTable.Chest { Biome = "Swamp", Prefab = "b", Weight = 1 },
                new RipLootTable.Chest { Biome = "Plains", Prefab = "c", Weight = 1 }
            };
            Assert.Equal("a", RipLootTable.Pick(chests, "Swamp", () => 0.0));
            Assert.Equal("a", RipLootTable.Pick(chests, "Swamp", () => 0.74));
            Assert.Equal("b", RipLootTable.Pick(chests, "Swamp", () => 0.76));
            Assert.Null(RipLootTable.Pick(chests, "Mistlands", () => 0.5));
        }
    }
}
