using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ProcRulesTests
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

        private static Dictionary<string, AffixTable.Affix> Affixes()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors).ToDictionary(a => a.Id);
            Assert.Empty(errors);
            return affixes;
        }

        private static AffixCaps Caps()
        {
            var errors = new List<string>();
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            Assert.Empty(errors);
            return caps;
        }

        private static LootItem Item(params object[] idValue)
        {
            var item = new LootItem();
            for (int i = 0; i < idValue.Length; i += 2)
            {
                item.Lines.Add(new LootItem.Line { Ids = new[] { (string)idValue[i] }, Value = Convert.ToSingle(idValue[i + 1]) });
            }
            return item;
        }

        private static string N(float value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>The code's numbers are the ones the rows' text and notes give (the table is where they're decided).</summary>
        [Fact]
        public void NumbersMatchTheAffixTable()
        {
            var a = Affixes();
            string Row(string id) => a[id].Text + " " + a[id].Notes;
            Assert.Contains($"for {N(ProcRules.BuffSeconds)} s", Row("shieldwall"));
            Assert.Contains($"lasts {N(ProcRules.BuffSeconds)} s", Row("rallying"));
            Assert.Contains($"for {N(ProcRules.BuffSeconds)} s", Row("skalds"));
            Assert.Contains($"for {N(ProcRules.BuffSeconds)} s", Row("fenrir"));
            foreach (var id in new[] { "shieldwall", "rallying", "steadfast" })
            {
                Assert.Contains($"within {N(ProcRules.AllyRange)} m", Row(id));
            }
            Assert.Contains($"{N(ProcRules.ReprisalBlockShare * 100f)}% of the blocking item's block power", Row("reprisal"));
            Assert.Contains($"within {N(ProcRules.WrathRadius)} m", Row("wrath"));
            Assert.Contains($"up to {ProcRules.SkadiMaxStacks} stacks", Row("skadi"));
            Assert.Contains($"{N(ProcRules.SkadiResetSeconds)} s without a hit", Row("skadi"));
            Assert.Contains($"up to {ProcRules.FenrirMaxStacks} stacks", Row("fenrir"));
            Assert.Contains($"for {N(ProcRules.ForagingSeconds)} s", Row("foraging"));
        }

        [Fact]
        public void StacksStopAtTheCap()
        {
            Assert.Equal(1, ProcRules.AddStack(0, 3));
            Assert.Equal(3, ProcRules.AddStack(2, 3));
            Assert.Equal(3, ProcRules.AddStack(3, 3));
        }

        [Fact]
        public void SkadiStacksOnOneTargetAndResets()
        {
            var stacks = new ProcRules.TargetStacks();
            object wolf = new object(), boar = new object(), sword = new object(), axe = new object();
            Assert.Equal(1, stacks.Hit(wolf, sword, 0));
            Assert.Equal(2, stacks.Hit(wolf, sword, 1));
            for (int i = 2; i < 10; i++)
            {
                stacks.Hit(wolf, sword, i);
            }
            Assert.Equal(ProcRules.SkadiMaxStacks, stacks.Hit(wolf, sword, 10));
            Assert.Equal(1, stacks.Hit(boar, sword, 11));                                  // changed target
            Assert.Equal(2, stacks.Hit(boar, sword, 11 + ProcRules.SkadiResetSeconds));    // just in time
            Assert.Equal(1, stacks.Hit(boar, sword, 15 + ProcRules.SkadiResetSeconds));    // too long without a hit
            Assert.Equal(1, stacks.Hit(boar, axe, 16 + ProcRules.SkadiResetSeconds));      // another weapon
        }

        [Fact]
        public void SkadiFrostIsAShareOfPhysicalPerStack()
        {
            Assert.Equal(6f, ProcRules.SkadiFrost(100f, 2f, 3), 3);
            Assert.Equal(25f, ProcRules.SkadiFrost(100f, 5f, 9), 3);   // capped at 5 stacks
            Assert.Equal(0f, ProcRules.SkadiFrost(0f, 5f, 2));
        }

        [Fact]
        public void ExtraBonusOnlyFillsWhatTheCapLeaves()
        {
            var caps = Caps();
            float cap = caps.Caps.First(c => c.Id == "movement_speed").Max;
            Assert.Equal(10f, ProcRules.ExtraUnderCap(5f, 10f, "foraging", caps), 3);
            Assert.Equal(5f, ProcRules.ExtraUnderCap(cap - 5f, 10f, "foraging", caps), 3);
            Assert.Equal(0f, ProcRules.ExtraUnderCap(cap + 10f, 10f, "foraging", caps), 3);
            float attackCap = caps.Caps.First(c => c.Id == "attack_speed").Max;
            Assert.Equal(attackCap - 12f, ProcRules.ExtraUnderCap(12f, 100f, "fenrir", caps), 3);
        }

        [Fact]
        public void DeathlessUsesTheShortestCooldown()
        {
            var a = Affixes();
            var items = new[] { Item("deathless", 18), null, Item("deathless", 12, "steadfast", 9), Item("steadfast", 5) };
            Assert.Equal(12f, ProcRules.Lowest(items, "cheat_death", a));
            Assert.Equal(0f, ProcRules.Lowest(new[] { Item("steadfast", 5) }, "cheat_death", a));
        }

        [Fact]
        public void CooldownCountsDownFromItsStart()
        {
            Assert.Equal(0, ProcRules.CooldownLeft(0, 500, 600));          // never used
            Assert.Equal(600, ProcRules.CooldownLeft(1000, 1000, 600));
            Assert.Equal(100, ProcRules.CooldownLeft(1000, 1500, 600));
            Assert.Equal(0, ProcRules.CooldownLeft(1000, 1600, 600));
            Assert.Equal(0, ProcRules.CooldownLeft(1000, 900, 600));       // clock behind (another world)
            Assert.Equal(0, ProcRules.CooldownLeft(1000, 1100, 0));        // no Deathless worn
        }
    }
}
