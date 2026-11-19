using System;
using System.Collections.Generic;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class HarvestRulesTests
    {
        private static HarvestItems<string> Items(string seed = null, params string[] gems)
        {
            return new HarvestItems<string>
            {
                IsWood = s => s == "Wood" || s == "FineWood",
                IsOre = s => s == "CopperOre" || s == "TinOre",
                Seed = seed,
                Coal = "Coal",
                FindOrb = () => "CorruptionOrb_Transmutation",
                Gems = new List<string>(gems)
            };
        }

        /// <summary>Returns the given values in turn, then fails the test if more are asked for.</summary>
        private static Func<double> Rolls(params double[] values)
        {
            int next = 0;
            return () =>
            {
                Assert.True(next < values.Length, "more random rolls than expected");
                return values[next++];
            };
        }

        [Fact]
        public void NoChancesKeepTheVanillaDrops()
        {
            var drops = new List<string> { "Wood", "Resin" };
            Assert.Equal(drops, HarvestRules.Apply(drops, new HarvestChances(), true, Items(), Rolls()));
        }

        [Fact]
        public void AnEmptyDropIsNotAHarvest()
        {
            var chances = new HarvestChances { Bounty = 100f, Stone = 100f, Resin = 100f };
            Assert.Empty(HarvestRules.Apply(new List<string>(), chances, false, Items(), Rolls()));
        }

        [Fact]
        public void BountifulDropsEverythingOnceMore()
        {
            var drops = new List<string> { "CopperOre", "Stone" };
            var chances = new HarvestChances { Bounty = 10f };
            Assert.Equal(new[] { "CopperOre", "Stone", "CopperOre", "Stone" }, HarvestRules.Apply(drops, chances, false, Items(), Rolls(0.05)));
            Assert.Equal(drops, HarvestRules.Apply(drops, chances, false, Items(), Rolls(0.10)));
        }

        [Fact]
        public void BothBountifulsGiveTripleWood()
        {
            var drops = new List<string> { "Wood", "Resin" };
            var chances = new HarvestChances { Bounty = 15f, WoodBounty = 15f };
            Assert.Equal(new[] { "Wood", "Resin", "Wood", "Resin", "Wood" }, HarvestRules.Apply(drops, chances, true, Items(), Rolls(0.0, 0.0)));
            // The woodcutting Bountiful alone repeats only the wood.
            Assert.Equal(new[] { "Wood", "Resin", "Wood" }, HarvestRules.Apply(drops, chances, true, Items(), Rolls(0.5, 0.0)));
        }

        [Fact]
        public void ArtisanAddsOneOreOfTheKindThatDropped()
        {
            var chances = new HarvestChances { Ore = 15f };
            Assert.Equal(new[] { "Stone", "TinOre", "TinOre" },
                HarvestRules.Apply(new List<string> { "Stone", "TinOre" }, chances, false, Items(), Rolls(0.0)));
            // No ore dropped: nothing to add.
            Assert.Equal(new[] { "Stone" }, HarvestRules.Apply(new List<string> { "Stone" }, chances, false, Items(), Rolls(0.0)));
        }

        [Fact]
        public void MiningExtrasOnlyOnMiningAndWoodExtrasOnlyOnWood()
        {
            var chances = new HarvestChances { Ore = 100f, Seed = 100f };
            Assert.Equal(new[] { "CopperOre", "CopperOre" },
                HarvestRules.Apply(new List<string> { "CopperOre" }, chances, false, Items("PineCone"), Rolls(0.0)));
            Assert.Equal(new[] { "Wood", "PineCone" },
                HarvestRules.Apply(new List<string> { "Wood" }, chances, true, Items("PineCone"), Rolls(0.0)));
        }

        [Fact]
        public void HoardfindersDropsAnOrbOnAMiningHarvest()
        {
            // Krogon, 2026-11-19: "X% chance a rock or ore chunk you break drops an orb".
            var chances = new HarvestChances { Stone = 25f };
            Assert.Equal(new[] { "CopperOre", "CorruptionOrb_Transmutation" },
                HarvestRules.Apply(new List<string> { "CopperOre" }, chances, false, Items(), Rolls(0.24)));
            Assert.Equal(new[] { "CopperOre" }, HarvestRules.Apply(new List<string> { "CopperOre" }, chances, false, Items(), Rolls(0.25)));
        }

        [Fact]
        public void HiddenRichesDropsAnOrbOnAWoodcuttingHarvest()
        {
            // Krogon, 2026-11-19: "X% chance a tree, log or stump you fell drops an orb".
            var chances = new HarvestChances { Resin = 5f };
            Assert.Equal(new[] { "Wood", "CorruptionOrb_Transmutation" },
                HarvestRules.Apply(new List<string> { "Wood" }, chances, true, Items(), Rolls(0.04)));
            Assert.Equal(new[] { "Wood" }, HarvestRules.Apply(new List<string> { "Wood" }, chances, true, Items(), Rolls(0.05)));
        }

        [Fact]
        public void EachOrbFinderOnlyRollsOnItsOwnKindOfHarvest()
        {
            // Hoardfinder's is a pickaxe affix and of Hidden Riches an axe affix: neither rolls on the other's harvest.
            Assert.Equal(new[] { "Wood" }, HarvestRules.Apply(new List<string> { "Wood" }, new HarvestChances { Stone = 100f }, true, Items(), Rolls()));
            Assert.Equal(new[] { "Stone" }, HarvestRules.Apply(new List<string> { "Stone" }, new HarvestChances { Resin = 100f }, false, Items(), Rolls()));
        }

        [Fact]
        public void NoOrbThatCanDropAddsNothing()
        {
            var items = Items();
            items.FindOrb = () => null;
            Assert.Equal(new[] { "Stone" }, HarvestRules.Apply(new List<string> { "Stone" }, new HarvestChances { Stone = 100f }, false, items, Rolls(0.0)));
        }

        [Fact]
        public void OrbsAreNotDoubledByBountifulOrTurnedIntoCoal()
        {
            // The orbs are added after Bountiful's copy; Charring only touches wood.
            var chances = new HarvestChances { Bounty = 100f, Resin = 100f, Charring = 100f };
            Assert.Equal(new[] { "Coal", "Coal", "CorruptionOrb_Transmutation" },
                HarvestRules.Apply(new List<string> { "Wood" }, chances, true, Items(), Rolls(0.0, 0.0, 0.0, 0.0)));
        }

        [Fact]
        public void ForestersNeedsASeedInTheTreesTable()
        {
            var chances = new HarvestChances { Seed = 100f };
            Assert.Equal(new[] { "Wood" }, HarvestRules.Apply(new List<string> { "Wood" }, chances, true, Items(), Rolls()));
        }

        [Fact]
        public void GemseekerPicksFromThePool()
        {
            var chances = new HarvestChances { Gem = 15f };
            var items = Items(null, "Amber", "AmberPearl", "Ruby");
            Assert.Equal(new[] { "Stone", "AmberPearl" }, HarvestRules.Apply(new List<string> { "Stone" }, chances, false, items, Rolls(0.1, 0.5)));
            Assert.Equal(new[] { "Stone", "Ruby" }, HarvestRules.Apply(new List<string> { "Stone" }, chances, false, items, Rolls(0.1, 0.99)));
            // No gems for the biome: nothing rolls.
            Assert.Equal(new[] { "Stone" }, HarvestRules.Apply(new List<string> { "Stone" }, chances, false, Items(), Rolls()));
        }

        [Fact]
        public void CharringReplacesEachWoodItemThatRolls()
        {
            var drops = new List<string> { "Wood", "Resin", "FineWood" };
            var chances = new HarvestChances { Charring = 15f };
            Assert.Equal(new[] { "Coal", "Resin", "FineWood" }, HarvestRules.Apply(drops, chances, true, Items(), Rolls(0.1, 0.5)));
        }

        [Fact]
        public void CharringAlsoTurnsTheDoubledWood()
        {
            var drops = new List<string> { "Wood" };
            var chances = new HarvestChances { WoodBounty = 100f, Charring = 15f };
            Assert.Equal(new[] { "Wood", "Coal" }, HarvestRules.Apply(drops, chances, true, Items(), Rolls(0.0, 0.5, 0.1)));
        }

        [Fact]
        public void GemPoolFollowsTheDepositsBiome()
        {
            Assert.Equal(new[] { "Amber", "AmberPearl" }, HarvestRules.GemPool("Meadows"));
            Assert.Equal(new[] { "Amber", "AmberPearl", "Ruby" }, HarvestRules.GemPool("BlackForest"));
            Assert.Equal(new[] { "Amber", "AmberPearl", "Ruby" }, HarvestRules.GemPool("AshLands"));
            Assert.Equal(new[] { "Amber", "AmberPearl", "Ruby" }, HarvestRules.GemPool("Ocean"));
            Assert.Empty(HarvestRules.GemPool("None"));
        }

        [Fact]
        public void AnyAndWithoutOdin()
        {
            Assert.False(new HarvestChances().Any);
            Assert.True(new HarvestChances { Odin = true }.Any);
            var chances = new HarvestChances { Odin = true, Stamina = 5f, Striker = 7 };
            var copy = chances.WithoutOdin();
            Assert.False(copy.Odin);
            Assert.Equal(5f, copy.Stamina);
            Assert.Equal(7, copy.Striker);
            Assert.True(chances.Odin);
        }
    }
}
