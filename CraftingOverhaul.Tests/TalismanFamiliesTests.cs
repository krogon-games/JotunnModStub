using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class TalismanFamiliesTests
    {
        private static readonly string[] HildirMinibosses =
        {
            "Skeleton_Hildir", "Skeleton_Hildir_nochest", "Fenring_Cultist_Hildir", "Fenring_Cultist_Hildir_nochest",
            "GoblinBruteBros", "GoblinBruteBros_nochest", "GoblinShaman_Hildir", "GoblinShaman_Hildir_nochest"
        };

        /// <summary>The real creature list from the Valheim 1.0.20 dump (every creature treated as a monster).</summary>
        private static List<TalismanFamilies.Creature> RealCreatures()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "data", "game-dump-1.0.20", "creatures.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);

            var creatures = new List<TalismanFamilies.Creature>();
            foreach (var line in File.ReadLines(Path.Combine(dir, "data", "game-dump-1.0.20", "creatures.csv")).Skip(1))
            {
                // prefab,token,name,health,faction,boss,defeat_key,spawn_biomes,drops (drops last, may be quoted)
                var cols = line.Split(',');
                var c = new TalismanFamilies.Creature { Prefab = cols[0], IsBoss = cols[5] == "True", IsMonster = true };
                string drops = string.Join(",", cols.Skip(8));
                foreach (var drop in drops.Trim('"').Split(';'))
                {
                    var prefab = drop.Trim().Split(' ')[0];
                    if (prefab.StartsWith("Trophy"))
                    {
                        c.Trophies.Add(prefab);
                    }
                }
                creatures.Add(c);
            }
            return creatures;
        }

        private static Dictionary<string, TalismanFamilies.Family> Build(List<TalismanFamilies.Creature> creatures)
        {
            return TalismanFamilies.Build(creatures, new HashSet<string>(HildirMinibosses)).ToDictionary(f => f.Primary);
        }

        [Theory]
        [InlineData("Greydwarf", "Greydwarf_Elite")]
        [InlineData("Greydwarf", "Greydwarf_Shaman")]
        [InlineData("Greydwarf", "Greydwarf_Frozen")]
        [InlineData("Draugr", "Draugr_Elite")]
        [InlineData("Draugr", "Draugr_Ranged")]
        [InlineData("Goblin", "GoblinArcher")]
        [InlineData("Goblin", "GoblinBrute")]
        [InlineData("Goblin", "GoblinShaman")]
        [InlineData("Skeleton", "Skeleton_Poison")]
        [InlineData("Seeker", "SeekerBrute")]
        public void VariantsJoinTheirFamily(string primary, string member)
        {
            var families = Build(RealCreatures());
            Assert.True(families.ContainsKey(primary), primary + " has no family");
            Assert.Contains(member, families[primary].Members);
            Assert.False(families.ContainsKey(member), member + " should not have its own talisman");
        }

        [Fact]
        public void FamilyRecipeUsesThePrimarysTrophy()
        {
            var families = Build(RealCreatures());
            Assert.Equal("TrophyGreydwarf", families["Greydwarf"].Trophy);
            Assert.Equal("TrophyGoblin", families["Goblin"].Trophy);
        }

        [Fact]
        public void BaseCreatureWithoutTrophyUsesTheMembersTrophy()
        {
            // In 1.0 the base Skeleton drops nothing; its variants drop TrophySkeleton.
            var families = Build(RealCreatures());
            Assert.Equal("TrophySkeleton", families["Skeleton"].Trophy);
        }

        [Fact]
        public void FamilyWithoutAnyTrophyIsDropped()
        {
            var lonely = new TalismanFamilies.Creature { Prefab = "Nobody", IsMonster = true };
            Assert.Empty(TalismanFamilies.Build(new[] { lonely }, new HashSet<string>()));
        }

        [Fact]
        public void BossesNeverAppear()
        {
            var families = Build(RealCreatures());
            var everyone = families.Values.SelectMany(f => f.Members).ToList();
            foreach (var boss in new[] { "Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "SeekerQueen", "Fader", "FrozenKing_p3" })
            {
                Assert.DoesNotContain(boss, everyone);
            }
        }

        [Fact]
        public void ExcludedQuestMinibossesAreNotRepelled()
        {
            var everyone = Build(RealCreatures()).Values.SelectMany(f => f.Members).ToList();
            foreach (var miniboss in HildirMinibosses)
            {
                Assert.DoesNotContain(miniboss, everyone);
            }
        }

        [Fact]
        public void DifferentCreaturesWithSimilarNamesStaySeparate()
        {
            var families = Build(RealCreatures());
            Assert.True(families.ContainsKey("JotunWarrior"));
            Assert.True(families.ContainsKey("JotunWitch"));
            Assert.DoesNotContain("JotunWitch", families["JotunWarrior"].Members);
        }

        [Fact]
        public void NonMonstersGetNoTalisman()
        {
            var deer = new TalismanFamilies.Creature { Prefab = "Deer", IsMonster = false };
            deer.Trophies.Add("TrophyDeer");
            Assert.Empty(TalismanFamilies.Build(new[] { deer }, new HashSet<string>()));
        }

        [Fact]
        public void DefaultExclusionPatternsCatchEveryQuestMiniboss()
        {
            var excluded = TalismanFamilies.Excluder(Corruption.CorruptionDefaults.ExcludedCreatures);
            var families = TalismanFamilies.Build(RealCreatures(), excluded).ToDictionary(f => f.Primary);
            var everyone = families.Values.SelectMany(f => f.Members).ToList();
            Assert.DoesNotContain(everyone, m => m.Contains("Hildir"));
            Assert.DoesNotContain("GoblinBruteBros", everyone);
            Assert.DoesNotContain("Charred_Melee_Dyrnwyn", everyone);
            Assert.Contains("GoblinBrute", families["Goblin"].Members);
        }

        [Theory]
        [InlineData("Skeleton_Hildir", "*Hildir*", true)]
        [InlineData("GoblinBruteBros_nochest", "GoblinBruteBros*", true)]
        [InlineData("GoblinBrute", "GoblinBruteBros*", false)]
        [InlineData("Charred_Melee_Dyrnwyn", "Charred_Melee_Dyrnwyn", true)]
        [InlineData("charred_melee_dyrnwyn", "Charred_Melee_Dyrnwyn", true)]
        [InlineData("Troll", "*Hildir*", false)]
        [InlineData("BlobTar", "*Tar", true)]
        [InlineData("BlobTarX", "*Tar", false)]
        public void ExclusionPatterns(string name, string pattern, bool expected)
        {
            Assert.Equal(expected, TalismanFamilies.Matches(name, pattern));
        }

        [Theory]
        [InlineData("Greydwarf_Elite", "Greydwarf", true)]
        [InlineData("GoblinArcher", "Goblin", true)]
        [InlineData("Goblinarcher", "Goblin", false)]
        [InlineData("Ghost", "Gjall", false)]
        [InlineData("Wolf", "Wolf", true)]
        public void ExtendsRule(string name, string primary, bool expected)
        {
            Assert.Equal(expected, TalismanFamilies.Extends(name, primary));
        }
    }
}
