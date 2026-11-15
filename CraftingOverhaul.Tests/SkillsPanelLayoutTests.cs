using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CraftingOverhaul.CharacterSheet;
using CraftingOverhaul.Classes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class SkillsPanelLayoutTests
    {
        private static readonly Dictionary<string, SheetRect> Rects =
            SkillsPanelLayout.Describe().ToDictionary(p => p.Key, p => p.Value);

        private static bool Inside(SheetRect inner, SheetRect outer) =>
            inner.X >= outer.X - 0.01f && inner.Y >= outer.Y - 0.01f &&
            inner.X + inner.Width <= outer.X + outer.Width + 0.01f && inner.Y + inner.Height <= outer.Y + outer.Height + 0.01f;

        private static bool Overlap(SheetRect a, SheetRect b) =>
            a.X < b.X + b.Width && b.X < a.X + a.Width && a.Y < b.Y + b.Height && b.Y < a.Y + a.Height;

        [Fact]
        public void EverythingFitsInThePanel()
        {
            var panel = Rects["panel"];
            Assert.All(Rects.Values, r => Assert.True(Inside(r, panel)));
            Assert.All(Rects.Values, r => Assert.True(r.Width > 0f && r.Height > 0f));
        }

        [Fact]
        public void TheMainBoxesDoNotOverlap()
        {
            var boxes = new[] { "character", "details", "tab0", "tab1", "tab2", "tablet", "slotsLabel", "slot0", "slot1", "slot2", "slot3", "slotsDivider", "learn", "close", "skillPoints" };
            for (int i = 0; i < boxes.Length; i++)
            {
                for (int j = i + 1; j < boxes.Length; j++)
                {
                    Assert.False(Overlap(Rects[boxes[i]], Rects[boxes[j]]), boxes[i] + " overlaps " + boxes[j]);
                }
            }
        }

        [Fact]
        public void ContentsSitInTheirBoxes()
        {
            Assert.True(Inside(Rects["cards"], Rects["tablet"]));
            // The strip under the tablet holds the slots and the Learn button (the mock-up's bottom bar).
            foreach (var name in new[] { "slotsLabel", "slot0", "slot1", "slot2", "slot3", "slotsDivider", "learn" })
            {
                Assert.True(Inside(Rects[name], Rects["slotsRow"]), name);
            }
            Assert.False(Overlap(Rects["slotsRow"], Rects["tablet"]));
            for (int i = 0; i < 4; i++)
            {
                Assert.True(Inside(Rects["attributePlus" + i], Rects["character"]));
                Assert.True(Inside(Rects["attributeName" + i], Rects["character"]));
                // The icon sits left of the name and the "what it gives" line; that line stops short of the + button.
                Assert.True(Inside(Rects["attributeIcon" + i], Rects["character"]));
                Assert.True(Rects["attributeIcon" + i].X < Rects["attributeName" + i].X);
                Assert.Equal(Rects["attributeName" + i].X, Rects["attributeEffect" + i].X);
                Assert.False(Overlap(Rects["attributeEffect" + i], Rects["attributePlus" + i]));
                Assert.False(Overlap(Rects["attributeName" + i], Rects["attributeValue" + i]));
                Assert.False(Overlap(Rects["attributeValue" + i], Rects["attributePlus" + i]));
            }
            foreach (var name in new[] { "detailsIcon", "detailsName", "detailsRank", "detailsKind", "detailsText" })
            {
                Assert.True(Inside(Rects[name], Rects["details"]), name);
            }
            Assert.True(Rects["detailsText"].Height > 160f);
        }

        [Fact]
        public void EveryCardSitsOnTheTablet()
        {
            var cards = Rects["cards"];
            for (int row = 1; row <= SkillCardsLayout.Rows; row++)
            {
                for (int col = 1; col <= SkillCardsLayout.Cols; col++)
                {
                    var c = SkillCardsLayout.Card(row, col);
                    Assert.True(Inside(new SheetRect(cards.X + c.X, cards.Y + c.Y, c.Width, c.Height), cards));
                }
            }
        }

        /// <summary>
        ///     Writes the layout (panel rectangles and the cards of the Amazon's three tabs) as JSON for
        ///     <c>tools/preview_skills_panel.py</c> when CO_PANEL_LAYOUT names a file; does nothing otherwise.
        /// </summary>
        [Fact]
        public void WritesTheLayoutForThePreviewWhenAsked()
        {
            string path = Environment.GetEnvironmentVariable("CO_PANEL_LAYOUT");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var inv = CultureInfo.InvariantCulture;
            string F(float v) => v.ToString("0.###", inv);
            string R(SheetRect r) => "[" + F(r.X) + "," + F(r.Y) + "," + F(r.Width) + "," + F(r.Height) + "]";
            var sb = new StringBuilder("{\n\"rects\": {");
            sb.Append(string.Join(",", Rects.Select(p => "\n  \"" + p.Key + "\": " + R(p.Value))));
            sb.Append("\n},\n\"card\": {\"icon\": ").Append(R(SkillCardsLayout.Icon)).Append(", \"name\": ").Append(R(SkillCardsLayout.Name))
                .Append(", \"rank\": ").Append(R(SkillCardsLayout.Rank)).Append(", \"rankBar\": ").Append(R(SkillCardsLayout.RankBar)).Append("}");
            var skills = ClassesFixture.AmazonSkills();
            sb.Append(",\n\"tabs\": [");
            for (int tab = 0; tab < 3; tab++)
            {
                var shown = SkillCardsLayout.Visible(skills, tab);
                sb.Append(tab > 0 ? "," : "").Append("\n {\"skills\": [");
                sb.Append(string.Join(",", shown.Select(s =>
                {
                    var c = SkillCardsLayout.Card(s.Row, s.Col);
                    return "\n  {\"id\":\"" + s.Id + "\",\"name\":\"" + s.Name + "\",\"x\":" + F(c.X) + ",\"y\":" + F(c.Y) + ",\"w\":" + F(c.Width) + ",\"h\":" + F(c.Height) +
                           ",\"status\":\"" + s.Status + "\",\"kind\":\"" + s.Kind + "\",\"description\":\"" + s.Description.Replace("\"", "'") + "\"}";
                })));
                sb.Append("]}");
            }
            sb.Append("\n]\n}\n");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
