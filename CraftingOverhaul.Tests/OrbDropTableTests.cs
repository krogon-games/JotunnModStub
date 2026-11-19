using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The currency orb drop table (orbs.csv) and the orb column of drop_modifiers.csv (Krogon, 2026-11-19, #33).</summary>
    public class OrbDropTableTests
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

        private static OrbDropTable ParseReal()
        {
            var errors = new List<string>();
            var table = OrbDropTable.Parse(DataFile("orbs.csv"), errors);
            Assert.True(errors.Count == 0, "orbs.csv has problems:\n" + string.Join("\n", errors));
            return table;
        }

        private static DropTable RealModifiers()
        {
            var errors = new List<string>();
            var table = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return table;
        }

        private static string Table(string rows) =>
            "orb,status,one_in,notes\n" + rows + string.Join("", OrbRules.Orbs.Where(o => !rows.Contains(o + ",")).Select(o => o + ",approved,,\n"));

        [Fact]
        public void TheRealTableMatchesKrogonsRates()
        {
            // Decisions 4 and 5 (2026-11-19): PoE's order squeezed between 1 in 50 and 1 in 2,500.
            var table = ParseReal();
            var expected = new Dictionary<string, float>
            {
                { "transmutation", 50 }, { "augmentation", 77 }, { "chance", 112 }, { "alchemy", 172 }, { "annulment", 172 },
                { "regal", 830 }, { "exalted", 1865 }, { "divine", 2500 }, { "mirror", 21400 }
            };
            Assert.Equal(expected.Keys.OrderBy(k => k), table.OneIn.Keys.OrderBy(k => k));
            foreach (var pair in expected)
            {
                Assert.Equal(pair.Value, table.OneIn[pair.Key]);
            }
        }

        [Fact]
        public void TheJewellersOrbDoesNotDropUntilSocketsExist()
        {
            var table = ParseReal();
            Assert.False(table.OneIn.ContainsKey("jeweller"));
            Assert.Equal(0f, table.Chance("jeweller", 100f));
        }

        [Fact]
        public void EveryOrbHasARow()
        {
            var errors = new List<string>();
            OrbDropTable.Parse("orb,status,one_in\ntransmutation,approved,50\n", errors);
            Assert.Contains(errors, e => e.Contains("no row for mirror"));
        }

        [Fact]
        public void BadRowsAreReported()
        {
            var errors = new List<string>();
            OrbDropTable.Parse(Table("transmutation,approved,0.5\nregal,maybe,10\nexalted,approved,x\nfoo,approved,5\nregal,approved,5\n"), errors);
            Assert.Contains(errors, e => e.Contains("transmutation") && e.Contains("1 or more"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("exalted") && e.Contains("one_in 'x'"));
            Assert.Contains(errors, e => e.Contains("orb must be one of"));
            Assert.Contains(errors, e => e.Contains("listed twice"));
        }

        [Fact]
        public void TheRealModifierTableHasAnOrbColumn()
        {
            // Orbs use the gear multipliers for stars and rips, and the unique ones for dungeon chests and corrupted bosses.
            var drops = RealModifiers();
            Assert.Equal(1.5f, drops.Multiplier(DropTable.OrbColumn, new[] { "star_2" }));
            Assert.Equal(1.25f, drops.Multiplier(DropTable.OrbColumn, new[] { "rip_tier_1" }));
            Assert.Equal(1.5f, drops.Multiplier(DropTable.OrbColumn, new[] { "dungeon_chest" }));
            Assert.Equal(3f, drops.Multiplier(DropTable.OrbColumn, new[] { "corrupted_boss" }));
            Assert.Equal(1.875f, drops.Multiplier(DropTable.OrbColumn, new[] { "star_2", "rip_tier_1" }), 4);
            Assert.Equal(1f, drops.Multiplier(DropTable.OrbColumn, null));
        }

        [Fact]
        public void AModifierTableWithoutTheOrbColumnStillLoads()
        {
            var errors = new List<string>();
            var drops = DropTable.Parse("rarity,status,one_in\nmagic,approved,50\nrare,approved,100\nunique,approved,2500",
                "source,status,magic,rare,unique\nstar_2,approved,1.5,1.5,1.5\n", errors);
            Assert.Empty(errors);
            Assert.Equal(1f, drops.Multiplier(DropTable.OrbColumn, new[] { "star_2" }));
        }

        [Fact]
        public void ChanceIsOneInNTimesTheMultiplierCappedAtOne()
        {
            var table = ParseReal();
            Assert.Equal(1f / 50f, table.Chance("transmutation", 1f), 6);
            Assert.Equal(1.5f / 2500f, table.Chance("divine", 1.5f), 8);
            Assert.Equal(1f, table.Chance("transmutation", 100f));
        }

        [Fact]
        public void EachOrbRollsOnItsOwn()
        {
            var errors = new List<string>();
            var table = OrbDropTable.Parse(Table("transmutation,approved,2\nregal,approved,4\n"), errors);
            Assert.Empty(errors);
            // Rolls in OrbRules.Orbs order: transmutation (1 in 2), then regal (1 in 4).
            var rolls = new Queue<double>(new[] { 0.4, 0.2 });
            Assert.Equal(new[] { "transmutation", "regal" }, table.Roll(1f, () => rolls.Dequeue()));
            rolls = new Queue<double>(new[] { 0.6, 0.3 });
            Assert.Empty(table.Roll(1f, () => rolls.Dequeue()));
            // A multiplier of 2 doubles both: transmutation is certain (capped at 100%), regal 1 in 2.
            rolls = new Queue<double>(new[] { 0.99, 0.4 });
            Assert.Equal(new[] { "transmutation", "regal" }, table.Roll(2f, () => rolls.Dequeue()));
        }

        [Fact]
        public void TheRealRatesGiveTheExpectedPace()
        {
            // About 7 orbs an hour per player at 125 kills an hour (the pace drop_chances.csv's Unique rate assumes).
            var table = ParseReal();
            double perKill = OrbRules.Orbs.Sum(o => (double)table.Chance(o, 1f));
            Assert.InRange(perKill * 125, 6.5, 7.5);
        }
    }
}
