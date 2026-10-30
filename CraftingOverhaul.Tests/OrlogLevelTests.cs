using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogLevelTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "levels.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static string Data(string file) => File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", file));

        private static T NoErrors<T>(Func<List<string>, T> parse, string file)
        {
            var errors = new List<string>();
            var result = parse(errors);
            Assert.True(errors.Count == 0, file + " has problems:\n" + string.Join("\n", errors));
            return result;
        }

        private static LevelTable Levels() => NoErrors(e => LevelTable.Parse(Data("levels.csv"), e), "levels.csv");
        private static CreatureXpTable CreatureXp() => NoErrors(e => CreatureXpTable.Parse(Data("creature_xp.csv"), e), "creature_xp.csv");
        private static BiomeLevels Biomes() => NoErrors(e => BiomeLevels.Parse(Data("biome_levels.csv"), e), "biome_levels.csv");

        [Fact]
        public void LevelsAreDiablo2sExperienceTable()
        {
            var levels = Levels();
            Assert.Equal(99, levels.TopLevel);
            // Arreat Summit, "Experience Levels".
            Assert.Equal(0, levels.TotalFor(1));
            Assert.Equal(500, levels.TotalFor(2));
            Assert.Equal(7875, levels.TotalFor(5));
            Assert.Equal(47116709, levels.TotalFor(50));
            Assert.Equal(3520485254, levels.TotalFor(99));
            // Arreat: a level 70 character gets 95.31% of the experience.
            Assert.Equal(0.9531f, levels.HighLevelRatio(70), 3);
            Assert.Equal(1f, levels.HighLevelRatio(69));
        }

        [Fact]
        public void LevelForXpStopsAtTheMaxLevel()
        {
            var levels = Levels();
            Assert.Equal(1, levels.LevelFor(0, 99));
            Assert.Equal(1, levels.LevelFor(499, 99));
            Assert.Equal(2, levels.LevelFor(500, 99));
            Assert.Equal(99, levels.LevelFor(long.MaxValue, 99));
            Assert.Equal(60, levels.LevelFor(long.MaxValue, 60));
            Assert.Equal(500, levels.ToNext(1, 99));
            Assert.Equal(0, levels.ToNext(60, 60));
            Assert.Equal(0, levels.ToNext(99, 99));
        }

        [Fact]
        public void CreatureXpCoversEveryLevelAndGrows()
        {
            var xp = CreatureXp();
            Assert.Equal(99, xp.TopLevel);
            for (int level = 2; level <= 98; level++)
            {
                Assert.True(xp.For(level) >= xp.For(level - 1), $"level {level} gives less XP than level {level - 1}");
            }
            // Classic WoW at level 1: 400 XP to level, 50 XP a kill = 8 kills. D2's level 1 needs 500, so 62 a kill.
            Assert.Equal(62, xp.For(1));
        }

        [Fact]
        public void BossesHaveTheUsersGuideLevels()
        {
            var biomes = Biomes();
            Assert.Equal(5, biomes.BossLevel("Eikthyr"));
            Assert.Equal(11, biomes.BossLevel("gd_king"));
            Assert.Equal(20, biomes.BossLevel("Bonemass"));
            Assert.Equal(35, biomes.BossLevel("Dragon"));
            Assert.Equal(50, biomes.BossLevel("GoblinKing"));
            Assert.Equal(62, biomes.BossLevel("SeekerQueen"));
            Assert.Equal(76, biomes.BossLevel("Fader"));
            Assert.Equal(90, biomes.BossLevel("FrozenKing"));
            Assert.Equal(90, biomes.BossLevel("FrozenKing_p3"));
            Assert.Equal(0, biomes.BossLevel("Boar"));
        }

        [Fact]
        public void BossesAreBossesInTheGameData()
        {
            var bosses = new HashSet<string>(File.ReadLines(Path.Combine(RepoRoot(), "data", "game-dump-1.0.20", "creatures.csv"))
                .Skip(1).Select(l => l.Split(',')).Where(c => c.Length > 5 && c[5] == "True").Select(c => c[0]));
            foreach (var biome in Biomes().All)
            {
                foreach (var boss in biome.Bosses)
                {
                    Assert.Contains(boss, bosses);
                }
            }
        }

        [Fact]
        public void BiomeRangesFollowEachOtherUpToTheirBoss()
        {
            var biomes = Biomes().All.OrderBy(b => b.MinLevel).ToList();
            Assert.Equal(8, biomes.Count);
            Assert.Equal(1, biomes[0].MinLevel);
            for (int i = 0; i < biomes.Count; i++)
            {
                Assert.Equal(biomes[i].BossLevel - 1, biomes[i].MaxLevel);
                if (i > 0)
                {
                    Assert.Equal(biomes[i - 1].BossLevel, biomes[i].MinLevel);
                }
            }
        }

        [Fact]
        public void CreatureLevelFollowsHealthInsideTheBiome()
        {
            var biomes = Biomes();
            // Meadows 1-4 from 5 to 20 health.
            Assert.Equal(1, biomes.CreatureLevel("Meadows", 5f));
            Assert.Equal(4, biomes.CreatureLevel("Meadows", 20f));
            Assert.Equal(4, biomes.CreatureLevel("Meadows", 100f));
            Assert.Equal(1, biomes.CreatureLevel("Meadows", 1f));
            // A draugr (100 health) is a low Swamp creature but a strong Meadows one.
            int swamp = biomes.CreatureLevel("Swamp", 100f);
            Assert.InRange(swamp, 11, 15);
            // An abomination (800) tops the Swamp.
            Assert.Equal(19, biomes.CreatureLevel("Swamp", 800f));
            // No range for the ocean: the caller decides.
            Assert.Equal(0, biomes.CreatureLevel("Ocean", 400f));
            Assert.Equal(0, biomes.CreatureLevel(null, 400f));
        }

        [Theory]
        // Lower creatures, D2 (Arreat Summit, "Experience"): 5 levels or fewer below = 100%, then 81% down to 5%.
        [InlineData(10, 10, 1)]
        [InlineData(10, 5, 1)]
        [InlineData(10, 4, 0.81)]
        [InlineData(10, 1, 0.24)]
        [InlineData(20, 10, 0.05)]
        [InlineData(20, 1, 0.05)]
        [InlineData(30, 24, 0.81)]
        // At or above the player: no cut.
        [InlineData(10, 20, 1)]
        public void LowerCreaturesFollowDiablo2(int player, int creature, double expected)
        {
            Assert.Equal(expected, ExperienceRules.LowerCreatureFactor(player, creature), 4);
        }

        [Theory]
        // Higher creatures, classic WoW (vmangos Formulas.h): +5% per level above, at most +20%.
        [InlineData(10, 10, 1.0)]
        [InlineData(10, 11, 1.05)]
        [InlineData(10, 13, 1.15)]
        [InlineData(10, 14, 1.2)]
        [InlineData(10, 40, 1.2)]
        [InlineData(10, 5, 1.0)]
        public void HigherCreaturesFollowClassicWow(int player, int creature, double expected)
        {
            Assert.Equal(expected, ExperienceRules.HigherCreatureBonus(player, creature), 4);
        }

        // A creature's XP by level for the share tests: 100 per level.
        private static long Flat(int level) => 100L * level;

        [Fact]
        public void WorthUsesTheLowerOfTheTwoLevels()
        {
            // Higher creature: the player's own level's XP + WoW's bonus, not the creature's (much bigger) XP.
            Assert.Equal(115, ExperienceRules.Worth(Flat, 1, 4), 6);
            // Same level: the creature's XP.
            Assert.Equal(1000, ExperienceRules.Worth(Flat, 10, 10), 6);
            // Lower creature: its own XP, cut by D2's factor (6 below = 81%).
            Assert.Equal(400 * 0.81, ExperienceRules.Worth(Flat, 10, 4), 6);
        }

        [Fact]
        public void PlayTestPaceIsFixed()
        {
            // Play-test 2026-10-30: at level 1, three greydwarves (level 4 in the Meadows, 178 XP) reached level 2 (500 XP).
            // Now a level-4 creature is worth level 1's XP (62) + 15%: 8 kills, close to classic WoW's 8 for level 2.
            var xp = CreatureXp();
            long greydwarf = ExperienceRules.Share(xp.For, 4, new[] { 1 }, 0.35f)[0];
            Assert.Equal(71, greydwarf);
            Assert.Equal(8, (int)Math.Ceiling(500.0 / greydwarf));
        }

        [Fact]
        public void SoloPlayerGetsWhatTheKillIsWorth()
        {
            Assert.Equal(new long[] { 1000 }, ExperienceRules.Share(Flat, 10, new[] { 10 }, 0.35f));
            Assert.Equal(new long[] { 324 }, ExperienceRules.Share(Flat, 4, new[] { 10 }, 0.35f));
            Assert.Equal(new long[] { 1200 }, ExperienceRules.Share(Flat, 20, new[] { 10 }, 0.35f));
        }

        [Fact]
        public void PartyGetsTheBonusSplitByLevel()
        {
            // Two level-10 players on a level-10 creature: 1000 * 1.35, half each.
            Assert.Equal(new long[] { 675, 675 }, ExperienceRules.Share(Flat, 10, new[] { 10, 10 }, 0.35f));
            // Four: 1000 * 2.05, a quarter each.
            Assert.All(ExperienceRules.Share(Flat, 10, new[] { 10, 10, 10, 10 }, 0.35f), s => Assert.Equal(513, s));
            // A level-30 beside a level-10, on a level-10 creature: 1000 * 1.35 split 1:3; the level-30's share is cut to
            // 5% (20 levels above the creature).
            var mixed = ExperienceRules.Share(Flat, 10, new[] { 10, 30 }, 0.35f);
            Assert.Equal(338, mixed[0]);
            Assert.Equal(51, mixed[1]);
        }

        [Fact]
        public void HighLevelRatioAppliesWhenGiven()
        {
            var levels = Levels();
            var shares = ExperienceRules.Share(Flat, 80, new[] { 80 }, 0.35f, levels.HighLevelRatio);
            Assert.Equal((long)Math.Round(8000 * levels.HighLevelRatio(80)), shares[0]);
        }

        [Fact]
        public void NoXpForNothing()
        {
            Assert.Equal(new long[] { 0 }, ExperienceRules.Share(level => 0, 10, new[] { 10 }, 0.35f));
            Assert.Empty(ExperienceRules.Share(Flat, 10, new int[0], 0.35f));
        }

        [Fact]
        public void BrokenTablesAreReported()
        {
            var errors = new List<string>();
            LevelTable.Parse("level,xp_total,exp_ratio\n1,0,1024\n2,0,1024\n", errors);
            Assert.Contains(errors, e => e.Contains("more XP"));
            errors.Clear();
            BiomeLevels.Parse("biome,min_level\nMeadows,1\n", errors);
            Assert.Contains(errors, e => e.Contains("missing column"));
        }
    }
}
