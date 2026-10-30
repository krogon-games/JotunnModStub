using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class AffixTotalsTests
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

        private static LootItem Item(string id, int value)
        {
            var item = new LootItem();
            item.Lines.Add(new LootItem.Line { Ids = new[] { id }, Value = value });
            return item;
        }

        [Fact]
        public void SumsAnEffectOverEquippedItems()
        {
            var affixes = Affixes();
            var items = new[] { Item("carry_weight", 30), Item("carry_weight", 45), null, Item("max_health_prefix", 10) };
            Assert.Equal(75, AffixTotals.Sum(items, "carry_weight", affixes));
            Assert.Equal(10, AffixTotals.Sum(items, "max_health", affixes));
            Assert.Equal(0, AffixTotals.Sum(items, "max_stamina", affixes));
        }

        [Fact]
        public void MovementAddsWornAndHeldUnderTheCap()
        {
            var affixes = Affixes();
            var caps = Caps();
            float cap = caps.Caps.First(c => c.Id == "movement_speed").Max;
            var few = new[] { Item("movement_speed", 4), Item("fleet", 5) };
            Assert.Equal(9f, AffixTotals.MovementPercent(few, affixes, caps));
            var many = Enumerable.Range(0, 20).Select(_ => Item("movement_speed", 5)).ToList();
            Assert.Equal(cap, AffixTotals.MovementPercent(many, affixes, caps));
        }

        [Fact]
        public void ResistanceAddsAllResistancesPerElementUnderTheCap()
        {
            var affixes = Affixes();
            var caps = Caps();
            var items = new[] { Item("fire_resistance", 40), Item("all_resistances", 8) };
            Assert.Equal(48f, AffixTotals.ResistancePercent("fire", items, affixes, caps));
            Assert.Equal(8f, AffixTotals.ResistancePercent("frost", items, affixes, caps));
            var stacked = new[] { Item("frost_resistance", 50), Item("frost_resistance", 50) };
            Assert.Equal(75f, AffixTotals.ResistancePercent("frost", stacked, affixes, caps));
        }

        [Fact]
        public void UncappedFamiliesPassThrough()
        {
            Assert.Equal(250f, AffixTotals.Cap(250f, "carry_weight", Caps()));
        }
    }
}
