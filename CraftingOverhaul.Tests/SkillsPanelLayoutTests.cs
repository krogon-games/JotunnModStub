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
            var boxes = new[] { "character", "details", "tab0", "tab1", "tab2", "tablet", "slotsLabel", "slot0", "slot1", "slot2", "slot3", "close", "skillPoints" };
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
            Assert.True(Inside(Rects["tree"], Rects["tablet"]));
            for (int i = 0; i < 4; i++)
            {
                Assert.True(Inside(Rects["attributePlus" + i], Rects["character"]));
                Assert.True(Inside(Rects["attributeName" + i], Rects["character"]));
                Assert.False(Overlap(Rects["attributeValue" + i], Rects["attributePlus" + i]));
            }
            foreach (var name in new[] { "detailsIcon", "detailsName", "detailsRank", "detailsKind", "detailsText", "learn" })
            {
                Assert.True(Inside(Rects[name], Rects["details"]), name);
            }
            Assert.False(Overlap(Rects["detailsText"], Rects["learn"]));
            Assert.True(Rects["detailsText"].Height > 120f);
        }

        [Fact]
        public void EveryMedallionSitsInTheTree()
        {
            var tree = Rects["tree"];
            for (int row = 1; row <= SkillTreeLayout.Rows; row++)
            {
                for (int col = 1; col <= SkillTreeLayout.Cols; col++)
                {
                    SkillTreeLayout.TilePosition(row, col, out float x, out float y);
                    var tile = new SheetRect(tree.X + x, tree.Y + y, SkillTreeLayout.Tile, SkillTreeLayout.Tile);
                    Assert.True(Inside(tile, tree));
                }
            }
        }

        /// <summary>
        ///     Writes the layout (panel rectangles, medallions and paths of the demo's tabs) as JSON for
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
            sb.Append("\n},\n\"tile\": ").Append(F(SkillTreeLayout.Tile));
            sb.Append(",\n\"rowCentres\": [").Append(string.Join(",", Enumerable.Range(1, SkillTreeLayout.Rows).Select(r => F(SkillTreeLayout.RowCentre(r))))).Append("]");
            sb.Append(",\n\"tiers\": [").Append(string.Join(",", SkillTreeLayout.TierLevels)).Append("]");
            var demo = new DemoClasses();
            var skills = FlatJson.Strings(demo.GetSkillIdsJson("Amazon")).Select(id => SkillView.Parse(demo.GetSkillInfoJson(id))).ToList();
            sb.Append(",\n\"tabs\": [");
            for (int tab = 0; tab < 3; tab++)
            {
                var shown = SkillTreeLayout.Visible(skills, tab);
                sb.Append(tab > 0 ? "," : "").Append("\n {\"skills\": [");
                sb.Append(string.Join(",", shown.Select(s =>
                {
                    SkillTreeLayout.TilePosition(s.Row, s.Col, out float x, out float y);
                    return "\n  {\"id\":\"" + s.Id + "\",\"name\":\"" + s.Name + "\",\"x\":" + F(x) + ",\"y\":" + F(y) + ",\"points\":" + s.Points +
                           ",\"canSpend\":" + (s.CanSpend ? "true" : "false") + ",\"status\":\"" + s.Status + "\",\"kind\":\"" + s.Kind + "\"}";
                })));
                sb.Append("], \"lines\": [");
                sb.Append(string.Join(",", SkillTreeLayout.Lines(shown).Select(l =>
                    "\n  {\"from\":\"" + l.From + "\",\"x1\":" + F(l.X1) + ",\"y1\":" + F(l.Y1) + ",\"x2\":" + F(l.X2) + ",\"y2\":" + F(l.Y2) + "}")));
                sb.Append("]}");
            }
            sb.Append("\n]\n}\n");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
