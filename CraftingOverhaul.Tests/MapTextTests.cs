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
        public void EveryTierRowHasItsHeaderAndColours()
        {
            var tiers = Tiers();
            string[] header =
            {
                "Tier 1 · Normal map", "Tier 2 · Magic map", "Tier 3 · Magic map", "Tier 4 · Rare map", "Tier 5 · Rare map",
                "Tier 6 · Corrupted map"
            };
            string[] colors = { "#FFFFFF", "#8888FF", "#8888FF", "#FFFF77", "#FFFF77", "#E04040" };
            string[] lineColors = { "#FFFFFF", "#8888FF", "#8888FF", "#FFFF77", "#FFFF77", "#FFFF77" };
            Assert.Equal(6, tiers.Rows.Count);
            foreach (var row in tiers.Rows)
            {
                int i = row.Tier - 1;
                Assert.Equal(header[i], MapText.Header(row));
                Assert.Equal(colors[i], MapText.ColorOf(row));
                Assert.Equal(lineColors[i], MapText.LineColorOf(row));
                Assert.Equal(row.Tier == 6 ? "Corrupted Meadows map" : "Meadows map", MapText.Name("Meadows map", row));
            }
        }

        [Fact]
        public void NumbersAreWorded()
        {
            Assert.Equal("3", MapText.Number(3.0));
            Assert.Equal("4.5", MapText.Number(4.5));
            Assert.Equal("1.75", MapText.Number(1.75));
        }

        [Fact]
        public void TwoStarLineFollowsThePercent()
        {
            Assert.Equal("Every enemy spawned in a wave has two stars.", MapText.TwoStarLine(100));
            Assert.Equal("Enemies spawned in a wave may have two stars by chance.", MapText.TwoStarLine(0));
            Assert.Equal("Each enemy spawned in a wave has at least a 25% chance of two stars.", MapText.TwoStarLine(25));
            Assert.Equal("Each enemy spawned in a wave has at least a 50% chance of two stars.", MapText.TwoStarLine(50));
            Assert.Equal("Each enemy spawned in a wave has at least a 75% chance of two stars.", MapText.TwoStarLine(75));
            Assert.Equal("Each enemy spawned in a wave has at least a 12.5% chance of two stars.", MapText.TwoStarLine(12.5));
        }

        [Fact]
        public void UsageNamesTheBiomeLabel()
        {
            Assert.Equal("Opens a rip into the Black Forest. The map crumbles once used.", MapText.Usage("Black Forest"));
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
                "Swarming: More enemies can fight at once, and reinforcements arrive faster.",
                "Hardened: Enemies take 30% less damage.",
                "Rabid: Enemies hit 40% harder and move 30% faster.",
                "Smouldering: Enemy hits deal 25% extra damage as fire."
            }, lines.ToArray());
        }

        [Fact]
        public void AMissingRowReadsAsANormalMap()
        {
            Assert.Equal("Tier 1 · Normal map", MapText.Header(null));
            Assert.Equal("Meadows map", MapText.Name("Meadows map", null));
            Assert.Equal("#FFFFFF", MapText.ColorOf(null));
        }

        private static MapAffixTable Affixes()
        {
            var errors = new List<string>();
            var affixes = MapAffixTable.Parse(DataFile("map_affixes.csv"), errors);
            Assert.Empty(errors);
            return affixes;
        }

        private static readonly string[] FourAffixes = { "smouldering", "rabid", "swarming", "hardened" };

        /// <summary>The text as the game shows it: tags gone, the bullet followed by a space, one entry per line, half-line spacers dropped.</summary>
        private static string[] Plain(string rich)
        {
            string text = System.Text.RegularExpressions.Regex.Replace(rich, "<[^>]*>", "").Replace("•", "• ");
            return text.Split('\n').Where(l => l.Trim().Length > 0).ToArray();
        }

        private static string Tooltip(int tier, string[] ids, string label = "Meadows", double? drop = -1, MapGlyphs glyphs = null,
            string marks = "", string secondary = "")
        {
            var row = Tiers().Row(tier);
            double? multiplier = drop.HasValue && drop.Value < 0 ? Drops(row.DropSource) : drop;
            return MapText.Tooltip(row, label, ids, Affixes(), multiplier, glyphs ?? MapGlyphs.Fancy, marks, secondary);
        }

        [Fact]
        public void Tier5TooltipReadsAsTheApprovedCopyWhateverOrderTheAffixesWereRolledIn()
        {
            string[] expected =
            {
                "Tier 5 · Rare map",
                "Opens a rip into the Meadows. The map crumbles once used.",
                "What you’ll face",
                "• Every enemy spawned in a wave has two stars.",
                "• Swarming — More enemies can fight at once, and reinforcements arrive faster.",
                "• Hardened — Enemies take 30% less damage.",
                "• Rabid — Enemies hit 40% harder and move 30% faster.",
                "• Smouldering — Enemy hits deal 25% extra damage as fire.",
                "What you’ll earn",
                "• Destroy the heart for 5–12 Corruption Shards total.",
                "• Each eligible player gets Rare gear or better.",
                "• Corruption loot is 2.25× as likely to drop during the rip."
            };
            Assert.Equal(expected, Plain(Tooltip(5, FourAffixes)));
        }

        [Fact]
        public void Tier1HasNoAffixesAndSaysTheHeartMayDropGear()
        {
            var row = Tiers().Row(1);
            Assert.Equal(new[] { "Enemies spawned in a wave may have two stars by chance." },
                MapText.FaceLines(row, new string[0], Affixes(), MapGlyphs.Fancy).ToArray());
            Assert.Equal(new[]
            {
                "Destroy the heart for 1–4 Corruption Shards total.",
                "The heart may also drop gear.",
                "Corruption loot is 1.25× as likely to drop during the rip."
            }, MapText.EarnLines(row, Drops(row.DropSource), MapGlyphs.Fancy).ToArray());
        }

        [Fact]
        public void Tier3SaysFiftyPercentAndMagicGear()
        {
            var row = Tiers().Row(3);
            var face = MapText.FaceLines(row, new[] { "swarming", "hardened" }, Affixes(), MapGlyphs.Fancy);
            Assert.Equal("Each enemy spawned in a wave has at least a 50% chance of two stars.", face[0]);
            Assert.Equal(new[] { "Swarming — More enemies can fight at once, and reinforcements arrive faster.", "Hardened — Enemies take 30% less damage." },
                face.Skip(1).ToArray());
            Assert.Equal(new[]
            {
                "Destroy the heart for 3–8 Corruption Shards total.",
                "Each eligible player gets Magic gear or better.",
                "Corruption loot is 1.75× as likely to drop during the rip."
            }, MapText.EarnLines(row, Drops(row.DropSource), MapGlyphs.Fancy).ToArray());
        }

        [Fact]
        public void Tier6IsCorruptedRedButItsAffixNamesStayRareYellow()
        {
            var row = Tiers().Row(6);
            string marks = "\n<color=#E04040>Corrupted</color>\n<color=#C8D8FF>Mirrored: can't be changed</color>";
            string text = Tooltip(6, FourAffixes, marks: marks);
            Assert.StartsWith("<color=#E04040>Tier 6 · Corrupted map</color>", text);
            Assert.Contains("<color=#FFFF77>Swarming</color>", text);
            Assert.Contains("<color=#FFFF77>Smouldering</color>", text);
            Assert.DoesNotContain("<color=#E04040>Swarming", text);
            Assert.Contains("<b>6–14</b>", text);
            Assert.Contains("<b>2.5×</b>", text);
            Assert.Contains("<align=center>" + marks.TrimStart('\n') + "</align>", text);
            Assert.Equal(new[] { "Corrupted", "Mirrored: can't be changed" }, Plain(text).Skip(Plain(text).Length - 2).ToArray());
        }

        [Fact]
        public void PlainGlyphsAreUsedWhenTheFontLacksTheFancyOnes()
        {
            var lines = Plain(Tooltip(5, new[] { "swarming" }, glyphs: MapGlyphs.Plain));
            Assert.Contains("What you'll face", lines);
            Assert.Contains("What you'll earn", lines);
            Assert.Contains("• Swarming - More enemies can fight at once, and reinforcements arrive faster.", lines);
            Assert.Contains("• Corruption loot is 2.25x as likely to drop during the rip.", lines);
        }

        [Fact]
        public void UnknownAffixesAreSkippedAndTheDropLineNeedsAMultiplier()
        {
            var row = Tiers().Row(2);
            var face = MapText.FaceLines(row, new[] { "nonsense", "rabid" }, Affixes(), MapGlyphs.Fancy);
            Assert.Equal(2, face.Count);
            Assert.StartsWith("Rabid — ", face[1]);
            var earn = MapText.EarnLines(row, null, MapGlyphs.Fancy);
            Assert.Equal(2, earn.Count);
            Assert.DoesNotContain("Corruption loot", string.Join("\n", earn));
            Assert.Contains("Opens a rip into the Black Forest.", Tooltip(2, new string[0], label: "Black Forest"));
        }

        [Fact]
        public void SingularShardAndEqualRange()
        {
            var row = new MapTierTable.TierRow { Tier = 1, Rarity = "normal", ShardsMin = 1, ShardsMax = 1, HeartFloor = "none" };
            Assert.Equal("Destroy the heart for 1 Corruption Shard total.", MapText.EarnLines(row, null, MapGlyphs.Fancy)[0]);
            row.ShardsMin = 6;
            row.ShardsMax = 6;
            Assert.Equal("Destroy the heart for 6 Corruption Shards total.", MapText.EarnLines(row, null, MapGlyphs.Fancy)[0]);
        }

        [Fact]
        public void TheTooltipEndsWithTheSecondaryBlockInItsColour()
        {
            string text = Tooltip(1, new string[0], secondary: "\nWeight: <color=orange>0.5</color>\n");
            Assert.EndsWith("<color=#B8AD9E>Weight: <color=orange>0.5</color></color>", text);
            Assert.DoesNotContain("#B8AD9E", Tooltip(1, new string[0]));
        }

        [Fact]
        public void TheForgeSheetHasNoTierLineNoWeightAndMarksTheAddedAffix()
        {
            var row = Tiers().Row(3);
            string sheet = MapText.ForgeSheet(row, "Meadows", new[] { "swarming", "hardened" }, Affixes(), Drops(row.DropSource), MapGlyphs.Fancy,
                new[] { "hardened" });
            Assert.DoesNotContain("Tier 3", sheet);
            Assert.DoesNotContain("Weight", sheet);
            Assert.StartsWith("<color=#EDE3CF>Opens a rip into the Meadows.", sheet);
            var lines = sheet.Split('\n');
            Assert.Single(lines, l => l.Contains("<mark=#E8C66A40>"));
            Assert.Contains("Hardened", lines.Single(l => l.Contains("<mark=#E8C66A40>")));
            Assert.DoesNotContain("<mark", lines.Single(l => l.Contains("Swarming")));
        }

        [Fact]
        public void TheForgeCaptionAndBenchAreTheTierLineAndTheMarks()
        {
            Assert.Equal("<color=#8888FF>Tier 2 · Magic map</color>", MapText.ForgeCaption(Tiers().Row(2)));
            Assert.Equal("", MapText.ForgeBench(""));
            Assert.Equal("<align=center>Corrupted</align>", MapText.ForgeBench("\nCorrupted"));
        }
    }
}
