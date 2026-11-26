using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The map's name, colour, header, affix lines and tier lines (the wording of docs/images/forge_map_*.png).</summary>
    public class MapTextTests
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

        private static MapTierTable Tiers()
        {
            var errors = new List<string>();
            var tiers = MapTierTable.Parse(DataFile("map_tiers.csv"), errors);
            Assert.Empty(errors);
            return tiers;
        }

        private static double Drops(string source)
        {
            var errors = new List<string>();
            var drops = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.Empty(errors);
            return drops.Multiplier("magic", new[] { source });
        }

        [Fact]
        public void EveryTierRowReadsAsThePicturesDo()
        {
            var tiers = Tiers();
            string[] header =
            {
                "Tier 1 · Normal map", "Tier 2 · Magic map", "Tier 3 · Magic map", "Tier 4 · Rare map", "Tier 5 · Rare map",
                "Tier 6 · Corrupted map"
            };
            string[][] lines =
            {
                new[] { "Waves x2 · no guaranteed two-stars", "The heart: 1–4 shards · gear by chance", "Drops x1.25 in the rip" },
                new[] { "Waves x2.5 · 1 in 4 creatures two-star", "The heart: 2–6 shards · gear at least Magic", "Drops x1.5 in the rip" },
                new[] { "Waves x3 · 1 in 2 creatures two-star", "The heart: 3–8 shards · gear at least Magic", "Drops x1.75 in the rip" },
                new[] { "Waves x3.5 · 3 in 4 creatures two-star", "The heart: 4–10 shards · gear at least Rare", "Drops x2 in the rip" },
                new[] { "Waves x4 · every creature two-star", "The heart: 5–12 shards · gear at least Rare", "Drops x2.25 in the rip" },
                new[] { "Waves x4.5 · every creature two-star", "The heart: 6–14 shards · gear at least Rare", "Drops x2.5 in the rip" }
            };
            string[] colors = { "#FFFFFF", "#8888FF", "#8888FF", "#FFFF77", "#FFFF77", "#E04040" };
            string[] lineColors = { "#FFFFFF", "#8888FF", "#8888FF", "#FFFF77", "#FFFF77", "#FFFF77" };
            Assert.Equal(6, tiers.Rows.Count);
            foreach (var row in tiers.Rows)
            {
                int i = row.Tier - 1;
                Assert.Equal(header[i], MapText.Header(row));
                Assert.Equal(lines[i], MapText.TierLines(row, Drops(row.DropSource)).ToArray());
                Assert.Equal(colors[i], MapText.ColorOf(row));
                Assert.Equal(lineColors[i], MapText.LineColorOf(row));
                Assert.Equal(row.Tier == 6 ? "Corrupted Meadows map" : "Meadows map", MapText.Name("Meadows map", row));
            }
        }

        [Fact]
        public void TwoStarWording()
        {
            Assert.Equal("no guaranteed two-stars", MapText.TwoStars(0));
            Assert.Equal("1 in 4 creatures two-star", MapText.TwoStars(25));
            Assert.Equal("1 in 2 creatures two-star", MapText.TwoStars(50));
            Assert.Equal("3 in 4 creatures two-star", MapText.TwoStars(75));
            Assert.Equal("every creature two-star", MapText.TwoStars(100));
            Assert.Equal("10% of creatures two-star", MapText.TwoStars(10));
            Assert.Equal("12.5% of creatures two-star", MapText.TwoStars(12.5));
        }

        [Fact]
        public void FloorShardsAndNumbersAreWorded()
        {
            Assert.Equal("gear by chance", MapText.Floor("none"));
            Assert.Equal("gear at least Magic", MapText.Floor("magic"));
            Assert.Equal("gear at least Rare", MapText.Floor("rare"));
            Assert.Equal("6 shards", MapText.Shards(6, 6));
            Assert.Equal("1 shard", MapText.Shards(1, 1));
            Assert.Equal("3–8 shards", MapText.Shards(3, 8));
            Assert.Equal("3", MapText.Number(3.0));
            Assert.Equal("4.5", MapText.Number(4.5));
            Assert.Equal("1.75", MapText.Number(1.75));
        }

        [Fact]
        public void NumbersUseADotWhateverTheCulture()
        {
            var before = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                Assert.Equal("4.5", MapText.Number(4.5));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = before;
            }
        }

        [Fact]
        public void AffixLinesAreNameAndDanger()
        {
            var errors = new List<string>();
            var affixes = MapAffixTable.Parse(DataFile("map_affixes.csv"), errors);
            Assert.Empty(errors);
            var lines = MapText.AffixLines(new[] { "swarming", "hardened", "nonsense", "rabid", "smouldering" }, affixes);
            Assert.Equal(new[]
            {
                "Swarming: Twice as many creatures in every wave",
                "Hardened: Rip creatures take 30% less damage",
                "Rabid: Rip creatures hit 40% harder and move 30% faster",
                "Smouldering: Every hit from a rip creature adds 25% of its damage as fire"
            }, lines.ToArray());
        }

        [Fact]
        public void TheCorruptExplanationNamesTheNewMap()
        {
            Assert.Equal(
                "The map becomes tier 6: Corrupted Meadows map.\nIt never breaks, but it is locked: no orb can change it afterwards. " +
                "Only a Mirror of Kalandra still works on it.", MapText.CorruptExplanation(6, "Corrupted Meadows map"));
        }

        [Fact]
        public void AMissingRowReadsAsANormalMap()
        {
            Assert.Equal("Tier 1 · Normal map", MapText.Header(null));
            Assert.Equal("Meadows map", MapText.Name("Meadows map", null));
            Assert.Equal("#FFFFFF", MapText.ColorOf(null));
        }

        [Fact]
        public void TheCorruptMapDialogHasNoListAndTheExplanationFollowsTheCost()
        {
            var l = ForgeDialogLayout.Compute(20f, 0f, new List<float>(), 40f, 20f, 0f, false);
            Assert.Empty(l.Descriptions);
            Assert.Equal(l.Cost.Bottom + ForgeDialogLayout.AfterCost, l.Explanation.Y);
            var full = ForgeDialogLayout.Compute(20f, 20f, new List<float> { 20f }, 40f, 20f, 0f);
            Assert.True(l.Height < full.Height);
            Assert.True(l.Tick.Y > l.Explanation.Bottom);
            Assert.True(l.Footer.Y > l.Tick.Bottom);
        }
    }
}
