using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogPassiveTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "amazon_passives.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static PassiveTable Amazon()
        {
            var errors = new List<string>();
            var table = PassiveTable.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", "amazon_passives.csv")), "amazon_passives.csv", errors);
            Assert.True(errors.Count == 0, "amazon_passives.csv has problems:\n" + string.Join("\n", errors));
            return table;
        }

        [Fact]
        public void TheTableHasD2sChancesUnchanged()
        {
            // User 2026-11-11: Diablo 2's chances unchanged (data/d2-reference/amazon_levels.csv, levels 1 and 20).
            var t = Amazon();
            Assert.Equal(5, t.Passives.Count());
            Assert.Equal(16f, PassiveRules.Chance(t.Find("CriticalStrike"), 1));
            Assert.Equal(68f, PassiveRules.Chance(t.Find("CriticalStrike"), 20));
            Assert.Equal(18f, PassiveRules.Chance(t.Find("Dodge"), 1));
            Assert.Equal(56f, PassiveRules.Chance(t.Find("Dodge"), 20));
            Assert.Equal(24f, PassiveRules.Chance(t.Find("Avoid"), 1));
            Assert.Equal(65f, PassiveRules.Chance(t.Find("Avoid"), 20));
            Assert.Equal(18f, PassiveRules.Chance(t.Find("Evade"), 1));
            Assert.Equal(56f, PassiveRules.Chance(t.Find("Evade"), 20));
        }

        [Fact]
        public void PenetrateTakesCriticalStrikesCurve()
        {
            var t = Amazon();
            Assert.Equal(t.Find("CriticalStrike").Chances, t.Find("Penetrate").Chances);
        }

        [Fact]
        public void EveryPassiveInTheTreeHasARowAndTheIdsMatch()
        {
            var t = Amazon();
            var errors = new List<string>();
            var tree = SkillTable.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", "amazon_skills.csv")), "amazon_skills.csv", errors);
            var builtPassives = tree.Skills.Where(s => s.Kind == "passive" && s.Built).Select(s => s.Id).OrderBy(x => x);
            Assert.Equal(builtPassives, t.Passives.Select(p => p.Id).OrderBy(x => x));
            Assert.Equal("CriticalStrike", t.ForEffect("crit").Id);
            Assert.Equal("Penetrate", t.ForEffect("penetrate").Id);
        }

        [Fact]
        public void ChanceIsZeroWithoutPointsAndKeepsTheLastValuePastTwenty()
        {
            var crit = Amazon().Find("CriticalStrike");
            Assert.Equal(0f, PassiveRules.Chance(crit, 0));
            Assert.Equal(0f, PassiveRules.Chance(crit, -3));
            Assert.Equal(68f, PassiveRules.Chance(crit, 25));
            Assert.Equal(0f, PassiveRules.Chance(null, 5));
        }

        [Fact]
        public void TooltipShowsTheChance()
        {
            var t = Amazon();
            Assert.Equal("25% chance to deal double physical damage", PassiveRules.Describe(t.Find("CriticalStrike"), 2));
            Assert.Contains("not immunities", PassiveRules.Describe(t.Find("Penetrate"), 1));
        }

        [Fact]
        public void RollsInsideTheChanceOnly()
        {
            Assert.True(PassiveRules.Rolls(25f, 0.2499f));
            Assert.False(PassiveRules.Rolls(25f, 0.25f));
            Assert.False(PassiveRules.Rolls(0f, 0f));
            Assert.True(PassiveRules.Rolls(100f, 0.9999f));
        }

        [Theory]
        // Standing still or attacking: Dodge for close-range hits, Avoid for projectiles.
        [InlineData(true, true, false, false, false, false, "Dodge")]
        [InlineData(true, true, false, true, false, false, "Avoid")]
        // Moving: Evade, for either.
        [InlineData(true, true, false, false, true, false, "Evade")]
        [InlineData(true, true, false, true, true, false, "Evade")]
        // Not a creature's hit (falls, traps, burning and poison ticks, players).
        [InlineData(false, true, false, false, false, false, "None")]
        // The game says a dodge roll can't escape it.
        [InlineData(true, false, false, false, false, false, "None")]
        // Area blasts and breath.
        [InlineData(true, true, true, true, true, false, "None")]
        // Holding block against a hit the block takes: the block or parry handles it.
        [InlineData(true, true, false, false, false, true, "None")]
        public void AvoidFollowsDiablo2sSplit(bool creature, bool dodgeable, bool area, bool ranged, bool moving, bool block, string expected)
        {
            Assert.Equal(expected, PassiveRules.Which(creature, dodgeable, area, ranged, moving, block).ToString());
        }

        [Fact]
        public void AvoidWordsAndEffects()
        {
            Assert.Equal("Dodge", PassiveRules.Word(AvoidKind.Dodge));
            Assert.Equal("evade", PassiveRules.Effect(AvoidKind.Evade));
            Assert.Equal("", PassiveRules.Word(AvoidKind.None));
            var t = Amazon();
            foreach (var kind in new[] { AvoidKind.Dodge, AvoidKind.Avoid, AvoidKind.Evade })
            {
                Assert.NotNull(t.ForEffect(PassiveRules.Effect(kind)));
            }
        }

        [Theory]
        [InlineData("dragon_coldbreath", true)]
        [InlineData("Fader_Flamebreath", true)]
        [InlineData("BonemawSerpent_breath", true)]
        [InlineData("aspect_dragon_coldbreath", true)]
        [InlineData("troll_groundslam", false)]
        [InlineData(null, false)]
        public void BreathAttacksAreFoundByName(string prefab, bool breath)
        {
            Assert.Equal(breath, PassiveRules.IsBreath(prefab));
        }

        [Theory]
        [InlineData("SlightlyResistant", true)]
        [InlineData("Resistant", true)]
        [InlineData("VeryResistant", true)]
        [InlineData("Immune", false)]
        [InlineData("Normal", false)]
        [InlineData("Weak", false)]
        [InlineData("VeryWeak", false)]
        [InlineData("SlightlyWeak", false)]
        [InlineData("Ignore", false)]
        public void PenetrateLiftsResistancesNotImmunity(string modifier, bool lifted)
        {
            Assert.Equal(lifted, PassiveRules.PenetrateLifts(modifier));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(5f)]
        [InlineData(37.5f)]
        public void PenetratesMarkKeepsTheOriginalEitr(float eitrAdd)
        {
            float marked = PassiveRules.Mark(eitrAdd);
            Assert.True(PassiveRules.IsMarked(marked));
            Assert.True(marked <= 0f, "vanilla only reads m_eitrAdd above 0");
            Assert.Equal(eitrAdd, PassiveRules.Unmark(marked));
            Assert.Equal(marked, PassiveRules.Mark(marked));
            Assert.False(PassiveRules.IsMarked(eitrAdd));
            Assert.Equal(eitrAdd, PassiveRules.Unmark(eitrAdd));
        }

        [Fact]
        public void BrokenRowsAreReported()
        {
            string header = "id,effect,text," + string.Join(",", Enumerable.Range(1, 20).Select(i => "l" + i)) + "\n";
            string up = string.Join(",", Enumerable.Range(1, 20));
            var errors = new List<string>();
            PassiveTable.Parse(header + "A,crit,no chance," + up + "\nB,crit,{0}%," + up + "\nC,wobble,{0}%," + up + "\nD,dodge,{0}%,"
                + string.Join(",", Enumerable.Range(1, 20).Reverse()) + "\n", "x.csv", errors);
            Assert.Contains(errors, e => e.Contains("no {0}"));
            Assert.Contains(errors, e => e.Contains("'crit' is listed twice"));
            Assert.Contains(errors, e => e.Contains("wobble"));
            Assert.Contains(errors, e => e.Contains("drops at level 2"));
        }
    }
}
