using System;
using System.Collections;
using System.Collections.Generic;
using CraftingOverhaul.Biomes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class BiomeCacheFileTests
    {
        private static List<KeyValuePair<string, string>> Key(string game = "1.0.21", string mods = "abc")
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("game", BiomeCacheFile.Hash(new[] { game })),
                new KeyValuePair<string, string>("mods", BiomeCacheFile.Hash(new[] { mods }))
            };
        }

        private static Dictionary<string, BiomeResolution> Items()
        {
            var bronze = new ItemSource
            {
                Name = "Bronze", Collector = "Conversions", Origin = "forge: Copper > Bronze",
                Tool = new ToolRequirement(2, ToolKind.Pickaxe), RequiredKey = "defeated_eikthyr"
            };
            bronze.RequiredItems.Add("Copper");
            bronze.RequiredItems.Add("Tin");
            var amber = new ItemSource
            {
                Name = "Amber", Base = Biome.Meadows, Collector = "Locations", IsWeak = true,
                Origin = "Location Dolmen01 > Container"
            };
            return new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase)
            {
                ["Bronze"] = new BiomeResolution { Biome = Biome.BlackForest, Source = bronze },
                ["Amber"] = new BiomeResolution { Biome = Biome.Meadows, Source = amber },
                ["Coins"] = new BiomeResolution { Biome = Biome.Meadows, IsOverride = true }
            };
        }

        private static Dictionary<string, BiomeResolution> Keys()
        {
            var boss = new ItemSource { Target = SourceTarget.GlobalKey, Name = "defeated_eikthyr", Base = Biome.Meadows, Collector = "Locations" };
            boss.RequiredItems.Add("TrophyDeer");
            return new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase)
            {
                ["defeated_eikthyr"] = new BiomeResolution { Biome = Biome.Meadows, Source = boss }
            };
        }

        [Fact]
        public void RoundTripKeepsBiomesAndSources()
        {
            string text = BiomeCacheFile.Write(Key(), Items(), Keys());

            Assert.True(BiomeCacheFile.TryRead(text, Key(), out var items, out var keys, out string reason), reason);
            Assert.Equal(3, items.Count);
            Assert.Single(keys);

            var bronze = items["bronze"]; // case-insensitive, as the solver's table
            Assert.Equal(Biome.BlackForest, bronze.Biome);
            Assert.False(bronze.IsOverride);
            Assert.Equal("Conversions", bronze.Source.Collector);
            Assert.Equal("forge: Copper > Bronze", bronze.Source.Origin);
            Assert.Null(bronze.Source.Base);
            Assert.Equal(2, bronze.Source.Tool.Tier);
            Assert.Equal(ToolKind.Pickaxe, bronze.Source.Tool.Kind);
            Assert.Equal("defeated_eikthyr", bronze.Source.RequiredKey);
            Assert.Equal(new[] { "Copper", "Tin" }, bronze.Source.RequiredItems);

            var amber = items["Amber"];
            Assert.Equal(Biome.Meadows, amber.Source.Base);
            Assert.True(amber.Source.IsWeak);
            Assert.Null(amber.Source.RequiredKey);
            Assert.Empty(amber.Source.RequiredItems);

            Assert.True(items["Coins"].IsOverride);
            Assert.Null(items["Coins"].Source);

            var key = keys["defeated_eikthyr"];
            Assert.Equal(SourceTarget.GlobalKey, key.Source.Target);
            Assert.Equal(new[] { "TrophyDeer" }, key.Source.RequiredItems);
        }

        [Fact]
        public void ChangedKeyPartsAreNamed()
        {
            string text = BiomeCacheFile.Write(Key(), Items(), Keys());

            Assert.False(BiomeCacheFile.TryRead(text, Key(game: "1.0.22", mods: "xyz"), out var items, out _, out string reason));
            Assert.Null(items);
            Assert.Equal("changed: game, mods", reason);
        }

        [Fact]
        public void AddedOrRemovedKeyPartsCountAsChanged()
        {
            string text = BiomeCacheFile.Write(Key(), Items(), Keys());
            var more = Key();
            more.Add(new KeyValuePair<string, string>("overrides", "0"));

            Assert.False(BiomeCacheFile.TryRead(text, more, out _, out _, out string reason));
            Assert.Equal("changed: overrides", reason);
            Assert.False(BiomeCacheFile.TryRead(text, Key().GetRange(0, 1), out _, out _, out reason));
            Assert.Equal("changed: mods", reason);
        }

        [Fact]
        public void TruncatedOrForeignFilesAreIgnored()
        {
            string text = BiomeCacheFile.Write(Key(), Items(), Keys());

            Assert.False(BiomeCacheFile.TryRead(text.Substring(0, text.Length / 2), Key(), out _, out _, out string reason));
            Assert.False(BiomeCacheFile.TryRead(text.Replace("\nend\t4\n", "\n"), Key(), out _, out _, out reason));
            Assert.Equal("incomplete file", reason);
            Assert.False(BiomeCacheFile.TryRead(text.Replace("\tBlackForest\t", "\tVolcano\t"), Key(), out _, out _, out _));
            Assert.False(BiomeCacheFile.TryRead(text.Replace("cache 1", "cache 0"), Key(), out _, out _, out reason));
            Assert.Equal("file from another format", reason);
            Assert.False(BiomeCacheFile.TryRead("", Key(), out _, out _, out _));
            Assert.False(BiomeCacheFile.TryRead(null, Key(), out _, out _, out _));
        }

        [Fact]
        public void WindowsLineEndingsStillRead()
        {
            string text = BiomeCacheFile.Write(Key(), Items(), Keys()).Replace("\n", "\r\n");

            Assert.True(BiomeCacheFile.TryRead(text, Key(), out var items, out _, out string reason), reason);
            Assert.Equal("forge: Copper > Bronze", items["Bronze"].Source.Origin);
        }

        [Fact]
        public void AwkwardCharactersSurvive()
        {
            const string odd = "tab\there|pipe\\slash\nnewline\rcr";
            var source = new ItemSource { Name = odd, Collector = odd, Origin = odd, RequiredKey = odd };
            source.RequiredItems.Add(odd);
            source.RequiredItems.Add("plain");
            var items = new Dictionary<string, BiomeResolution> { [odd] = new BiomeResolution { Biome = Biome.Ashlands, Source = source } };

            string text = BiomeCacheFile.Write(Key(), items, new Dictionary<string, BiomeResolution>());

            Assert.True(BiomeCacheFile.TryRead(text, Key(), out var read, out _, out string reason), reason);
            var s = read[odd].Source;
            Assert.Equal(odd, s.Name);
            Assert.Equal(odd, s.Origin);
            Assert.Equal(odd, s.RequiredKey);
            Assert.Equal(new[] { odd, "plain" }, s.RequiredItems);
        }

        [Fact]
        public void HashIsStableAndSeparatesValues()
        {
            Assert.Equal(BiomeCacheFile.Hash(new[] { "a", "b" }), BiomeCacheFile.Hash(new[] { "a", "b" }));
            Assert.NotEqual(BiomeCacheFile.Hash(new[] { "ab", "c" }), BiomeCacheFile.Hash(new[] { "a", "bc" }));
            Assert.NotEqual(BiomeCacheFile.Hash(new[] { "a", "b" }), BiomeCacheFile.Hash(new[] { "b", "a" }));
            Assert.Equal(16, BiomeCacheFile.Hash(new string[0]).Length);
            // Fixed value: the hash must not change between runs or .NET versions, or every saved file would be ignored.
            Assert.Equal("698f4f20ce028a9e", BiomeCacheFile.Hash(new[] { "Wood", "Bronze" }));
        }

        [Fact]
        public void SolvedResultRoundTrips()
        {
            var sources = new List<ItemSource>
            {
                new ItemSource { Name = "Wood", Base = Biome.Meadows, Collector = "Test" },
                new ItemSource { Name = "CopperOre", Base = Biome.BlackForest, Tool = new ToolRequirement(2, ToolKind.Pickaxe), Collector = "Test" },
                new ItemSource { Name = "HardAntler", Base = Biome.Meadows, Collector = "Test" }
            };
            var pick = new ItemSource { Name = "PickaxeAntler", Collector = "Test" };
            pick.RequiredItems.AddRange(new[] { "Wood", "HardAntler" });
            sources.Add(pick);
            var tools = new List<Tool> { new Tool("PickaxeAntler", 2, ToolKind.Pickaxe) };
            var solver = new BiomeSolver(sources, new Dictionary<string, Biome>(), tools).Solve();

            string text = BiomeCacheFile.Write(Key(), solver.Items, solver.Keys);

            Assert.True(BiomeCacheFile.TryRead(text, Key(), out var items, out _, out string reason), reason);
            Assert.Equal(solver.Items.Count, items.Count);
            foreach (var entry in solver.Items)
            {
                Assert.Equal(entry.Value.Biome, items[entry.Key].Biome);
            }
        }
    }

    public class SlicedWorkTests
    {
        /// <summary>Work whose every step advances a fake clock by the given milliseconds.</summary>
        private static IEnumerator Work(double[] clock, IEnumerable<object> steps, double stepMs)
        {
            foreach (var step in steps)
            {
                clock[0] += stepMs;
                yield return step;
            }
        }

        private static IEnumerable<object> Nulls(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return null;
            }
        }

        [Fact]
        public void StopsWhenTheBudgetIsSpent()
        {
            var clock = new double[1];
            var work = Work(clock, Nulls(10), 1.5);
            var stats = new SliceStats();

            Assert.True(SlicedWork.Pump(work, 4, stats, () => clock[0]));
            Assert.Equal(4.5, clock[0]); // three steps: 1.5, 3.0, then 4.5 is over budget
            Assert.Equal(1, stats.Slices);
            Assert.Equal(4.5, stats.BusyMs);
        }

        [Fact]
        public void WaitFrameEndsTheSliceAtOnce()
        {
            var clock = new double[1];
            var work = Work(clock, new[] { null, SlicedWork.WaitFrame, null }, 0.1);
            var stats = new SliceStats();

            Assert.True(SlicedWork.Pump(work, 100, stats, () => clock[0]));
            Assert.Equal(0.2, clock[0], 6);
            Assert.False(SlicedWork.Pump(work, 100, stats, () => clock[0]));
            Assert.Equal(2, stats.Slices);
        }

        [Fact]
        public void FinishesAndReportsTheLongestStep()
        {
            var clock = new double[1];
            int pumps = 0;
            var work = Work(clock, Nulls(20), 1);
            var stats = new SliceStats();

            while (SlicedWork.Pump(work, 4, stats, () => clock[0]))
            {
                pumps++;
            }

            Assert.Equal(20, clock[0]);
            Assert.Equal(1, stats.LongestStepMs);
            Assert.Equal(4, stats.LongestSliceMs);
            Assert.Equal(20, stats.BusyMs);
            Assert.True(pumps >= 4);
        }

        [Fact]
        public void ALongStepStillFinishes()
        {
            var clock = new double[1];
            var work = Work(clock, Nulls(2), 50);
            var stats = new SliceStats();

            Assert.True(SlicedWork.Pump(work, 4, stats, () => clock[0]));
            Assert.Equal(50, stats.LongestStepMs);
        }
    }
}
