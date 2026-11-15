using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogBossPointTests
    {
        private static BiomeLevels Biomes()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "biome_levels.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var errors = new List<string>();
            var b = BiomeLevels.Parse(File.ReadAllText(Path.Combine(dir, "Orlog", "Data", "biome_levels.csv")), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return b;
        }

        [Fact]
        public void TwelvePointsInAllOneEachThenTwoEach()
        {
            // User 2026-11-11: D2's 12 quest points; 1 each for the first four bosses, 2 each for the last four.
            var b = Biomes();
            Assert.Equal(12, SkillRules.BossBonus(b.All.Select(x => x.Name), b));
            Assert.Equal(new[] { 1, 1, 1, 1, 2, 2, 2, 2 }, b.All.OrderBy(x => x.BossLevel).Select(x => x.BossSkillPoints));
        }

        [Fact]
        public void BossXpFollowsDiablo2sActBosses()
        {
            var b = Biomes();
            // Andariel 916 / 97 and Baal 6460 / 97 (data/d2-reference/act_bosses.csv); the others in between, rising.
            Assert.Equal(9.4f, b.BossXpMultiplier("Eikthyr"), 2);
            var order = b.All.OrderBy(x => x.BossLevel).Select(x => x.BossXp).ToArray();
            Assert.Equal(new[] { 9.4f, 13.8f, 17.8f, 20.6f, 25.9f, 33.7f, 48.9f, 66.6f }, order);
            Assert.Equal(1f, b.BossXpMultiplier("Boar"));
            Assert.Equal(1f, b.BossXpMultiplier(null));
        }

        [Fact]
        public void KallsPhasesShareHisXpOnce()
        {
            var b = Biomes();
            float whole = b.BossXpMultiplier("FrozenKing") + b.BossXpMultiplier("FrozenKing_p2") + b.BossXpMultiplier("FrozenKing_p3");
            Assert.Equal(66.6f, whole, 1);
            // By base health (game dump: 10000, 7000, 30000): the last phase the most.
            Assert.True(b.BossXpMultiplier("FrozenKing_p3") > b.BossXpMultiplier("FrozenKing"));
        }

        [Fact]
        public void BadBossXpIsReported()
        {
            var errors = new List<string>();
            BiomeLevels.Parse("biome,min_level,max_level,health_min,health_max,boss_level,bosses,boss_xp,boss_xp_shares\n" +
                              "Meadows,1,4,5,20,5,A;B,0.5,0.5;0.2\n", errors);
            Assert.Equal(2, errors.Count);
        }

        [Fact]
        public void KallsThreePhasesAreOneBoss()
        {
            var b = Biomes();
            Assert.Equal("DeepNorth", b.BiomeOfBoss("FrozenKing").Name);
            Assert.Same(b.BiomeOfBoss("FrozenKing"), b.BiomeOfBoss("FrozenKing_p3"));
            Assert.Equal("Meadows", b.BiomeOfBoss("Eikthyr").Name);
            Assert.Null(b.BiomeOfBoss("Boar"));
        }

        [Fact]
        public void KillsSaveOncePerBiome()
        {
            var b = Biomes();
            var kills = SkillRules.ParseBossKills("Swamp;Meadows;;meadows");
            Assert.Equal(2, kills.Count);
            Assert.Equal("Meadows;Swamp", SkillRules.FormatBossKills(kills));
            Assert.Equal(2, SkillRules.BossBonus(kills, b));
            Assert.Equal(0, SkillRules.BossBonus(null, b));
        }

        [Fact]
        public void BonusAddsToUnspentPoints()
        {
            var spent = new Dictionary<string, int> { { "MagicArrow", 3 } };
            // Level 5 earns 4 points (from level 2); 3 spent; +2 from bosses.
            Assert.Equal(1, SkillRules.Unspent(5, 1, spent));
            Assert.Equal(3, SkillRules.Unspent(5, 1, spent, 2));
            Assert.Equal(1, SkillRules.Unspent(5, 1, spent, -7));
        }
    }
}
