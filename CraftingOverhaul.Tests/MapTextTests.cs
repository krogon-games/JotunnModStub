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
        public void AffixLinesAreTheDangerOnly()
        {
            var lines = MapText.AffixLines(Rolls("teeming", "nonsense", "brutal", "ironskin", "searing"));
            Assert.Equal(new[]
            {
                "50% more enemies can fight at once and reinforcements arrive faster.",
                "Enemies deal 20% more damage.",
                "Enemies take 95% less physical damage.",
                "Enemy hits add 25% of their physical damage as fire that burns over 5 s."
            }, lines.ToArray());
        }

        [Fact]
        public void AffixLineReplacesTheValueWithAWholeNumber()
        {
            var brutal = Affixes().Find("brutal");
            Assert.Equal("Enemies deal 63% more damage.", MapText.AffixLine(brutal, 63));
            Assert.Equal("Enemies deal 63% more damage.", MapText.Danger(brutal, 62.6));
            Assert.Equal("Enemies can't be staggered.", MapText.Danger(Affixes().Find("unyielding"), 0));
            // The face line is the danger alone (decision 10: no names), the same with either glyph set.
            var row = Tiers().Row(2);
            var rolls = new[] { new MapAffixRoll { Affix = brutal, Value = 63 } };
            Assert.Equal("Enemies deal 63% more damage.", MapText.FaceLines(row, rolls, Affixes(), MapGlyphs.Fancy)[1]);
            Assert.Equal("Enemies deal 63% more damage.", MapText.FaceLines(row, rolls, Affixes(), MapGlyphs.Plain)[1]);
        }

        [Fact]
        public void EveryDangerTextUsesOnlyCharactersTheFontHas()
        {
            foreach (var affix in Affixes().Affixes.Where(a => a.Approved))   // the later rows are notes, not copy yet
            {
                foreach (char c in affix.Danger)
                {
                    Assert.True(char.IsLetterOrDigit(c) && c < 128 || " .,'%{}-".IndexOf(c) >= 0, affix.Id + ": '" + c + "'");
                }
                Assert.True(affix.Danger.Replace("{value}", "").IndexOfAny(new[] { '{', '}' }) < 0, affix.Id);
            }
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

        // Rolled in a different order than the table's, at their lowest values.
        private static readonly string[] FourAffixes = { "searing", "withering", "glorious", "ironskin" };

        /// <summary>The affixes at their lowest values; ids the table doesn't know are skipped (as a map's unknown ids are).</summary>
        private static List<MapAffixRoll> Rolls(params string[] ids)
        {
            var table = Affixes();
            return ids.Where(id => table.Find(id) != null).Select(id => new MapAffixRoll { Affix = table.Find(id), Value = table.Find(id).Min }).ToList();
        }

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
            return MapText.Tooltip(row, label, Rolls(ids), Affixes(), multiplier, glyphs ?? MapGlyphs.Fancy, marks, secondary);
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
                "• Enemies grant 50% more experience.",
                "• Enemies take 95% less physical damage.",
                "• You lose 5 health per second near the heart.",
                "• Enemy hits add 25% of their physical damage as fire that burns over 5 s.",
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
                MapText.FaceLines(row, Rolls(), Affixes(), MapGlyphs.Fancy).ToArray());
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
            var face = MapText.FaceLines(row, Rolls("teeming", "brutal"), Affixes(), MapGlyphs.Fancy);
            Assert.Equal("Each enemy spawned in a wave has at least a 50% chance of two stars.", face[0]);
            Assert.Equal(new[] { "50% more enemies can fight at once and reinforcements arrive faster.", "Enemies deal 20% more damage." },
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
            // Decision 10: no names; the line itself is in the rarity colour.
            Assert.Contains("<color=#FFFF77>Enemies grant 50% more experience.</color>", text);
            Assert.DoesNotContain("Glorious", text);
            Assert.DoesNotContain("<color=#E04040>Enemies grant", text);
            Assert.Contains("<b>6–14</b>", text);
            Assert.Contains("<b>2.5×</b>", text);
            Assert.Contains("<align=center>" + marks.TrimStart('\n') + "</align>", text);
            Assert.Equal(new[] { "Corrupted", "Mirrored: can't be changed" }, Plain(text).Skip(Plain(text).Length - 2).ToArray());
        }

        [Fact]
        public void PlainGlyphsAreUsedWhenTheFontLacksTheFancyOnes()
        {
            var lines = Plain(Tooltip(5, new[] { "glorious" }, glyphs: MapGlyphs.Plain));
            Assert.Contains("What you'll face", lines);
            Assert.Contains("What you'll earn", lines);
            Assert.Contains("• Enemies grant 50% more experience.", lines);
            Assert.Contains("• Corruption loot is 2.25x as likely to drop during the rip.", lines);
        }

        [Fact]
        public void UnknownAffixesAreSkippedAndTheDropLineNeedsAMultiplier()
        {
            var row = Tiers().Row(2);
            var face = MapText.FaceLines(row, Rolls("nonsense", "brutal"), Affixes(), MapGlyphs.Fancy);
            Assert.Equal(2, face.Count);
            Assert.StartsWith("", face[1]);
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
            string sheet = MapText.ForgeSheet(row, "Meadows", Rolls("teeming", "brutal"), Affixes(), Drops(row.DropSource), MapGlyphs.Fancy,
                new[] { "brutal" });
            Assert.DoesNotContain("Tier 3", sheet);
            Assert.DoesNotContain("Weight", sheet);
            Assert.StartsWith("<color=#EDE3CF>Opens a rip into the Meadows.", sheet);
            var lines = sheet.Split('\n');
            Assert.Single(lines, l => l.Contains("<mark=#E8C66A40>"));
            Assert.Contains("Enemies deal", lines.Single(l => l.Contains("<mark=#E8C66A40>")));
            Assert.DoesNotContain("<mark", lines.Single(l => l.Contains("more enemies can fight")));
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
