using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The loot map affixes (#96, Krogon's decisions of 2026-11-28 on claude/nifty-hypatia-afm8ez).</summary>
    public class MapLootRulesTests
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

        private static MapAffixTable Affixes()
        {
            var errors = new List<string>();
            var table = MapAffixTable.Parse(DataFile("map_affixes.csv"), errors);
            Assert.Empty(errors);
            return table;
        }

        private static List<MapAffixRoll> Rolls(params string[] ids)
        {
            var table = Affixes();
            return ids.Select(id => new MapAffixRoll { Affix = table.Find(id), Value = table.Find(id).Min }).ToList();
        }

        [Fact]
        public void NoLootAffixesChangeNothing()
        {
            foreach (var boost in new[] { MapLootRules.Of(null), MapLootRules.Of(Rolls("brutal", "glorious", "teeming")) })
            {
                Assert.Equal(1.0, boost.Items);
                Assert.Equal(1.0, boost.Orbs);
                Assert.False(boost.TiltsPick);
                Assert.Equal(1.0, boost.WeightOf(MapLootRules.KindShield));
            }
        }

        [Fact]
        public void BountifulDoublesGearMapsAndOrbs()
        {
            // Decision 3: Corruption's own drops (gear, orbs, maps); overhaul decision 7: x2.
            var boost = MapLootRules.Of(Rolls("bountiful"));
            Assert.Equal(2.0, boost.Items);
            Assert.Equal(2.0, boost.Orbs);
            Assert.False(boost.TiltsPick);
        }

        [Fact]
        public void GlitteringDoublesOrbsOnly()
        {
            var boost = MapLootRules.Of(Rolls("glittering"));
            Assert.Equal(1.0, boost.Items);   // the map roll and gear stay
            Assert.Equal(2.0, boost.Orbs);
        }

        [Fact]
        public void LootAffixesMultiplyWithEachOtherAndTheTier()
        {
            // Decision 4: "Multiply". Bountiful + Glittering = orbs x4; tier 3 (x1.75) makes it x7.
            var boost = MapLootRules.Of(Rolls("bountiful", "glittering"));
            Assert.Equal(4.0, boost.Orbs);
            Assert.Equal(2.0, boost.Items);

            var errors = new List<string>();
            var drops = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.Empty(errors);
            var tier3 = new[] { "rip_tier_3" };
            Assert.Equal(1.0 / 50 * 1.75 * 2, drops.Chance("magic", tier3, boost.Items), 6);
            Assert.Equal(1.0 / 100 * 1.75 * 2, drops.Chance("rare", tier3, boost.Items), 6);
            Assert.Equal(1.0 / 50 * 1.75, drops.Chance("magic", tier3), 6);   // no factor: as before
            Assert.Equal(1.75 * 4, drops.Multiplier(DropTable.OrbColumn, tier3) * boost.Orbs, 5);
            Assert.Equal(1f, drops.Chance("magic", tier3, 1000.0));            // still capped at 1
        }

        [Fact]
        public void TheKindAffixesTiltThePick()
        {
            var boost = MapLootRules.Of(Rolls("bulwarked", "fletched"));
            Assert.True(boost.TiltsPick);
            Assert.Equal(1.0, boost.Items);   // decision 5: the number of drops is unchanged
            Assert.Equal(2.0, boost.WeightOf(MapLootRules.KindShield));
            Assert.Equal(2.0, boost.WeightOf(MapLootRules.KindRanged));
            Assert.Equal(1.0, boost.WeightOf(MapLootRules.KindArmour));
            Assert.Equal(1.0, boost.WeightOf(MapLootRules.KindMelee));
            Assert.Equal(1.0, boost.WeightOf(""));
            Assert.Equal(2.0, MapLootRules.Of(Rolls("mailed")).Armour);
            Assert.Equal(2.0, MapLootRules.Of(Rolls("honed")).Melee);
        }

        [Theory]
        [InlineData("shield", "Blocking", "shield")]
        [InlineData("helmet", "Swords", "armour")]
        [InlineData("chest", "Swords", "armour")]
        [InlineData("legs", "Swords", "armour")]
        [InlineData("cape", "None", "armour")]          // backpacks (decision 6)
        [InlineData("jewellery", "Swords", "")]         // not armour (decision 6)
        [InlineData("weapon", "Bows", "ranged")]
        [InlineData("weapon", "Crossbows", "ranged")]
        [InlineData("weapon", "ElementalMagic", "ranged")]   // staffs (decision 7)
        [InlineData("weapon", "BloodMagic", "ranged")]
        [InlineData("weapon", "Swords", "melee")]
        [InlineData("weapon", "Spears", "melee")]
        [InlineData("weapon", "Unarmed", "melee")]
        [InlineData("weapon;axe_1h", "Axes", "melee")]
        [InlineData("pickaxe", "Pickaxes", "melee")]    // decision 7
        public void EveryLootClassHasItsKind(string classes, string skill, string kind)
        {
            Assert.Equal(kind, MapLootRules.KindOf(classes.Split(';'), skill));
        }

        [Fact]
        public void EveryLootBaseClassIsCovered()
        {
            var errors = new List<string>();
            var bases = LootBases.Parse(DataFile("loot_bases.csv"), errors);
            Assert.Empty(errors);
            foreach (var b in bases.Bases)
            {
                string kind = MapLootRules.KindOf(b.Classes, "Swords");
                Assert.True(kind.Length > 0 || b.Classes.Contains("jewellery"), b.Prefab + " has no loot affix kind");
            }
        }

        [Fact]
        public void AWeightedPickFollowsTheWeights()
        {
            var errors = new List<string>();
            var bases = LootBases.Parse("prefab,name,biome,classes\nShieldWood,Wood shield,Meadows,shield\nClub,Club,Meadows,weapon\n", errors);
            Assert.Empty(errors);
            Func<LootBases.Base, double> weight = b => b.Prefab == "ShieldWood" ? 2.0 : 1.0;
            // Total weight 3: rolls below 2/3 give the shield.
            Assert.Equal("ShieldWood", bases.Pick("Meadows", 0f, null, () => 0.6, weight).Prefab);
            Assert.Equal("Club", bases.Pick("Meadows", 0f, null, () => 0.7, weight).Prefab);
            // No weights: equal, as before.
            Assert.Equal("ShieldWood", bases.Pick("Meadows", 0f, null, () => 0.4).Prefab);
            Assert.Equal("Club", bases.Pick("Meadows", 0f, null, () => 0.6).Prefab);
            // A weight of 0 is never picked.
            Assert.Equal("Club", bases.Pick("Meadows", 0f, null, () => 0.0, b => b.Prefab == "Club" ? 1.0 : 0.0).Prefab);
        }

        [Fact]
        public void TheLootAffixesAreRewardsAndRunicWaits()
        {
            var table = Affixes();
            foreach (var id in new[] { "bountiful", "glittering", "bulwarked", "mailed", "honed", "fletched" })
            {
                var a = table.Find(id);
                Assert.True(a.Approved);
                Assert.True(a.Earns);              // decision 16 of the overhaul: under "What you'll earn"
                Assert.Equal(100.0, a.Min);
                Assert.False(a.Ranged);
                Assert.Contains("{value}%", a.Danger);
            }
            var runic = table.Find("runic");
            Assert.False(runic.Approved);          // decision 2: later, never rolls
            Assert.DoesNotContain(runic, table.Rollable);
        }
    }
}
