using System.Collections.Generic;
using System.Linq;
using CraftingOverhaul.Biomes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class BiomeSolverTests
    {
        private static ItemSource World(string item, Biome biome, int toolTier = 0, ToolKind kind = ToolKind.Any, string key = null)
        {
            return new ItemSource { Name = item, Base = biome, Tool = new ToolRequirement(toolTier, kind), RequiredKey = key, Collector = "Test" };
        }

        private static ItemSource Made(string item, params string[] from)
        {
            var s = new ItemSource { Name = item, Collector = "Test" };
            s.RequiredItems.AddRange(from);
            return s;
        }

        private static ItemSource KeyFrom(string key, Biome biome, params string[] summonItems)
        {
            var s = new ItemSource { Target = SourceTarget.GlobalKey, Name = key, Base = biome, Collector = "Test" };
            s.RequiredItems.AddRange(summonItems);
            return s;
        }

        private static BiomeSolver Solve(List<ItemSource> sources, List<Tool> tools = null, Dictionary<string, Biome> overrides = null)
        {
            return new BiomeSolver(sources, overrides ?? new Dictionary<string, Biome>(), tools ?? new List<Tool>()).Solve();
        }

        /// <summary>A cut-down early game: bronze needs copper ore (needs an antler pickaxe from Eikthyr) and a forge.</summary>
        private static (List<ItemSource> Sources, List<Tool> Tools) EarlyGame()
        {
            var sources = new List<ItemSource>
            {
                World("Wood", Biome.BlackForest),
                World("Wood", Biome.Meadows),
                World("Stone", Biome.Meadows),
                World("TrophyDeer", Biome.Meadows),
                World("HardAntler", Biome.Meadows),
                World("SurtlingCore", Biome.BlackForest),
                World("CopperOre", Biome.BlackForest, 2, ToolKind.Pickaxe),
                World("TinOre", Biome.BlackForest),
                World("FineWood", Biome.Meadows, 2, ToolKind.Chop),
                Made("AxeFlint", "Wood"),
                Made("PickaxeAntler", "Wood", "HardAntler"),
                Made("Coal", "Wood", "Stone", "SurtlingCore"),              // kiln
                Made("Copper", "CopperOre", "Coal", "Stone", "SurtlingCore"), // smelter
                Made("Tin", "TinOre", "Coal", "Stone", "SurtlingCore"),
                Made("Bronze", "Copper", "Tin"),
                Made("AxeBronze", "Wood", "Bronze")
            };
            var tools = new List<Tool>
            {
                new Tool("AxeFlint", 1, ToolKind.Chop),
                new Tool("PickaxeAntler", 2, ToolKind.Pickaxe),
                new Tool("AxeBronze", 2, ToolKind.Chop)
            };
            return (sources, tools);
        }

        [Fact]
        public void ItemTakesItsEarliestSource()
        {
            var solver = Solve(new List<ItemSource> { World("Wood", Biome.BlackForest), World("Wood", Biome.Meadows) });
            Assert.Equal(Biome.Meadows, solver.Items["Wood"].Biome);
        }

        [Fact]
        public void CraftedItemTakesItsLatestIngredient()
        {
            var (sources, tools) = EarlyGame();
            var solver = Solve(sources, tools);
            Assert.True(solver.Settled);
            Assert.Equal(Biome.Meadows, solver.Items["PickaxeAntler"].Biome);
            Assert.Equal(Biome.BlackForest, solver.Items["Copper"].Biome);
            Assert.Equal(Biome.BlackForest, solver.Items["Bronze"].Biome);
        }

        [Fact]
        public void ToolGatedResourceWaitsForTheTool()
        {
            // Fine wood grows in the Meadows but needs a bronze axe (tier 2 chop), which is Black Forest.
            var (sources, tools) = EarlyGame();
            var solver = Solve(sources, tools);
            Assert.Equal(Biome.BlackForest, solver.Items["FineWood"].Biome);
        }

        [Fact]
        public void ToolKindMustMatch()
        {
            // The antler pickaxe is tier 2 but can't chop, so it must not unlock fine wood.
            var (sources, tools) = EarlyGame();
            tools.RemoveAll(t => t.Item == "AxeBronze");
            var solver = Solve(sources, tools);
            Assert.False(solver.Items.ContainsKey("FineWood"));
        }

        [Fact]
        public void UnknownRequirementLeavesItemUnresolved()
        {
            var solver = Solve(new List<ItemSource> { World("Wood", Biome.Meadows), Made("Mystery", "Wood", "NeverFound") });
            Assert.False(solver.Items.ContainsKey("Mystery"));
        }

        [Fact]
        public void OverrideIsFixedAndPropagates()
        {
            var sources = new List<ItemSource>
            {
                World("Wood", Biome.Meadows),
                World("TraderThing", Biome.Meadows), // an early source that the override must win over
                Made("Gadget", "Wood", "TraderThing")
            };
            var solver = Solve(sources, overrides: new Dictionary<string, Biome> { ["TraderThing"] = Biome.Plains });
            Assert.True(solver.Items["TraderThing"].IsOverride);
            Assert.Equal(Biome.Plains, solver.Items["TraderThing"].Biome);
            Assert.Equal(Biome.Plains, solver.Items["Gadget"].Biome);
        }

        [Fact]
        public void TraderStockWaitsForItsBossKey()
        {
            // Sold by a Black Forest trader once Bonemass (Swamp, summoned with withered bone) is dead.
            var sources = new List<ItemSource>
            {
                World("WitheredBone", Biome.Swamp),
                KeyFrom("defeated_bonemass", Biome.Swamp, "WitheredBone"),
                World("Wishbone", Biome.Swamp),
                World("MegingjordLike", Biome.BlackForest, key: "defeated_bonemass"),
                World("CheapThing", Biome.BlackForest)
            };
            var solver = Solve(sources);
            Assert.Equal(Biome.Swamp, solver.Keys["defeated_bonemass"].Biome);
            Assert.Equal(Biome.Swamp, solver.Items["MegingjordLike"].Biome);
            Assert.Equal(Biome.BlackForest, solver.Items["CheapThing"].Biome);
        }

        [Fact]
        public void KeyThatIsNeverUnlockedBlocksTheSource()
        {
            var solver = Solve(new List<ItemSource> { World("Locked", Biome.Meadows, key: "defeated_nobody") });
            Assert.False(solver.Items.ContainsKey("Locked"));
        }

        [Fact]
        public void CycleWithoutAnyWorldSourceStaysUnresolved()
        {
            var solver = Solve(new List<ItemSource> { Made("A", "B"), Made("B", "A") });
            Assert.Empty(solver.Items);
            Assert.True(solver.Settled);
        }

        [Fact]
        public void CycleWithAWorldSourceResolves()
        {
            var solver = Solve(new List<ItemSource> { Made("A", "B"), Made("B", "A"), World("B", Biome.Swamp) });
            Assert.Equal(Biome.Swamp, solver.Items["A"].Biome);
            Assert.Equal(Biome.Swamp, solver.Items["B"].Biome);
        }

        [Fact]
        public void ResultDoesNotDependOnSourceOrder()
        {
            var (sources, tools) = EarlyGame();
            var expected = Solve(sources, tools).Items.ToDictionary(e => e.Key, e => e.Value.Biome);

            var reversed = Enumerable.Reverse(sources).ToList();
            var actual = Solve(reversed, tools).Items.ToDictionary(e => e.Key, e => e.Value.Biome);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ChestLootDoesNotPullAPlacedItemEarlier()
        {
            var loot = World("IronNails", Biome.Meadows);
            loot.IsWeak = true;
            var solver = Solve(new List<ItemSource> { World("Iron", Biome.Swamp), Made("IronNails", "Iron"), loot });
            Assert.Equal(Biome.Swamp, solver.Items["IronNails"].Biome);
        }

        [Fact]
        public void ChestLootPlacesItemsNothingElseCan()
        {
            var loot = World("Amber", Biome.BlackForest);
            loot.IsWeak = true;
            var solver = Solve(new List<ItemSource> { loot, Made("AmberNecklace", "Amber") });
            Assert.Equal(Biome.BlackForest, solver.Items["Amber"].Biome);
            Assert.Equal(Biome.BlackForest, solver.Items["AmberNecklace"].Biome);
        }

        [Fact]
        public void ResolutionRecordsTheWinningSource()
        {
            var early = World("Wood", Biome.Meadows);
            var solver = Solve(new List<ItemSource> { World("Wood", Biome.Swamp), early });
            Assert.Same(early, solver.Items["Wood"].Source);
        }
    }
}
