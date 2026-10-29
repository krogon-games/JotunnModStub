using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class MapListTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "maps.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static List<MapList.Map> ParseReal()
        {
            var errors = new List<string>();
            var maps = MapList.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Corruption", "Data", "maps.csv")), errors);
            Assert.True(errors.Count == 0, "maps.csv has problems:\n" + string.Join("\n", errors));
            return maps;
        }

        [Fact]
        public void RealFileHasOneMapPerBiome()
        {
            var maps = ParseReal();
            Assert.Equal(MapList.Biomes.OrderBy(b => b), maps.Select(m => m.Biome).OrderBy(b => b));
        }

        [Fact]
        public void EveryEssenceIsInTheEssenceTable()
        {
            var errors = new List<string>();
            var essences = EssenceTable.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Corruption", "Data", "essences.csv")), errors);
            var looks = new HashSet<string>(essences.Select(e => e.Look));
            foreach (var m in ParseReal())
            {
                Assert.True(looks.Contains(m.Essence), $"{m.Biome}: essence '{m.Essence}' isn't in essences.csv.");
            }
        }

        [Fact]
        public void EveryMaterialIsAGameItem()
        {
            var items = new HashSet<string>(File.ReadLines(Path.Combine(RepoRoot(), "data", "game-dump-1.0.20", "items.csv"))
                .Skip(1).Select(l => l.Split(',')[0]));
            foreach (var m in ParseReal())
            {
                Assert.True(items.Contains(m.Material), $"{m.Biome}: material '{m.Material}' isn't an item in the 1.0.20 game data.");
            }
        }

        [Fact]
        public void ReportsTypicalEditingMistakes()
        {
            var errors = new List<string>();
            MapList.Parse(
                "biome,label,status,name,essence,material,material_amount,shards,description\n" +
                "Meadow,Meadows,approved,Map,eikthyr,DeerHide,0,1,Desc\n" +
                "Swamp,Swamp,approved,Map,,Guck,5,x,Desc\n" +
                "Swamp,Swamp,maybe,,bonemass,Guck,5,1,\n", errors);
            Assert.Contains(errors, e => e.Contains("biome must be one of"));
            Assert.Contains(errors, e => e.Contains("material_amount must be"));
            Assert.Contains(errors, e => e.Contains("essence and material"));
            Assert.Contains(errors, e => e.Contains("shards must be"));
            Assert.Contains(errors, e => e.Contains("listed twice"));
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("can't be empty"));
        }
    }

    public class RipRulesTests
    {
        [Fact]
        public void SampleStaysInsideTheWorldAndRespectsTheFilter()
        {
            var rng = new Random(1);
            var spots = RipRules.Sample(rng.NextDouble, 5000, 1000, (x, z) => x > 0);
            Assert.NotEmpty(spots);
            Assert.All(spots, s =>
            {
                Assert.True(s.X > 0);
                Assert.True(Math.Sqrt(s.X * s.X + s.Z * s.Z) <= RipRules.WorldRadius);
            });
        }

        [Fact]
        public void SampleStopsAtMaxResults()
        {
            var rng = new Random(2);
            Assert.Equal(10, RipRules.Sample(rng.NextDouble, 5000, 10, (x, z) => true).Count);
        }

        [Fact]
        public void PicksTheFirstCandidateWithHeartAndLandingAwayFromBases()
        {
            RipRules.Placement P(float heartX, float landingX) => new RipRules.Placement(new RipRules.Spot(heartX, 0), new RipRules.Spot(landingX, 0));
            var bases = new[] { new RipRules.Spot(50, 0) };
            // Heart near the base; then landing near the base; then both clear.
            var candidates = new[] { P(0, 500), P(500, 120), P(1000, 1100) };
            var pick = RipRules.FirstAwayFromBases(candidates, bases, 100);
            Assert.True(pick.HasValue);
            Assert.Equal(1000, pick.Value.Heart.X);
            Assert.Null(RipRules.FirstAwayFromBases(new[] { P(0, 500) }, bases, 100));
        }

        [Fact]
        public void LandingSpotIsWithinTheHeartDistanceAndAccepted()
        {
            var rng = new Random(3);
            var heart = new RipRules.Spot(100, -200);
            for (int i = 0; i < 200; i++)
            {
                var spot = RipRules.SpotAround(heart, rng.NextDouble, RipRules.HeartMinDistance, RipRules.HeartMaxDistance, 16, (x, z) => x > heart.X);
                Assert.True(spot.HasValue);
                double d = Math.Sqrt(Math.Pow(spot.Value.X - heart.X, 2) + Math.Pow(spot.Value.Z - heart.Z, 2));
                Assert.InRange(d, RipRules.HeartMinDistance - 0.01, RipRules.HeartMaxDistance + 0.01);
                Assert.True(spot.Value.X > heart.X);
            }
            Assert.Null(RipRules.SpotAround(heart, rng.NextDouble, 96, 128, 16, (x, z) => false));
        }

        [Fact]
        public void TimeText()
        {
            Assert.Equal("less than a minute", RipRules.TimeLeft(30));
            Assert.Equal("12 min", RipRules.TimeLeft(12 * 60 + 5));
            Assert.Equal("1 h 05 min", RipRules.TimeLeft(65 * 60));
        }
    }
}
