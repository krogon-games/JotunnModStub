using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>Maps as loot and the boss and chest shards (Krogon, 2026-11-23, decisions 17-33).</summary>
    public class MapDropRulesTests
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
        public void MapsDropOnlyAfterTheFirstRipAndTheirBoss()
        {
            Assert.True(MapDropRules.CanDrop(true, true, true));
            Assert.False(MapDropRules.CanDrop(false, true, true));  // no rip heart destroyed yet in this world
            Assert.False(MapDropRules.CanDrop(true, true, false));  // that biome's boss isn't beaten
            Assert.False(MapDropRules.CanDrop(true, false, true));  // the biome has no map (the Ocean)
        }

        [Fact]
        public void TheMapDropIsAsCommonAsARegalOrb()
        {
            var errors = new List<string>();
            var rules = LootRules.Parse(DataFile("loot_rules.csv"), DataFile("rare_names.csv"), errors);
            Assert.Empty(errors);
            var orbs = OrbDropTable.Parse(DataFile("orbs.csv"), errors);
            Assert.Empty(errors);
            Assert.Equal(orbs.OneIn["regal"], rules.MapDropOneIn);
            Assert.Equal(1f / 830f, MapDropRules.Chance(rules.MapDropOneIn, 1f), 7);
        }

        [Fact]
        public void MultipliersRaiseTheChanceAndZeroSwitchesItOff()
        {
            Assert.Equal(1.5f / 830f, MapDropRules.Chance(830f, 1.5f), 7);
            Assert.Equal(1f, MapDropRules.Chance(2f, 4f));
            Assert.Equal(0f, MapDropRules.Chance(0f, 1f));
            Assert.False(MapDropRules.Roll(0f, 1f, () => 0.0));
            Assert.True(MapDropRules.Roll(830f, 1f, () => 0.0011));
            Assert.False(MapDropRules.Roll(830f, 1f, () => 0.0013));
        }

        [Fact]
        public void EveryBiomeMapIsJustTheBiomesName()
        {
            // Decision 17: "{biome} map"; "corrupted" maps come with the map tiers.
            var errors = new List<string>();
            var maps = MapList.Parse(DataFile("maps.csv"), errors);
            Assert.Empty(errors);
            foreach (var map in maps)
            {
                Assert.Equal(map.Label + " map", map.Name);
            }
        }

        [Fact]
        public void EveryBossKillGivesEachPlayerAShard()
        {
            Assert.Equal(1, ShardRules.BossPerPlayer(1, 1));
            Assert.Equal(4, ShardRules.BossPerPlayer(4, 1));
            Assert.Equal(1, ShardRules.BossPerPlayer(0, 1));  // the killer out of range of the game running the boss
            Assert.Equal(0, ShardRules.BossPerPlayer(4, 0));  // switched off
        }

        [Fact]
        public void ADungeonChestHoldsAShardOneTimeInTwenty()
        {
            Assert.Equal(1, ShardRules.ChestShards(0.05f, () => 0.049));
            Assert.Equal(0, ShardRules.ChestShards(0.05f, () => 0.051));
            Assert.Equal(0, ShardRules.ChestShards(0f, () => 0.0));
        }
    }
}
