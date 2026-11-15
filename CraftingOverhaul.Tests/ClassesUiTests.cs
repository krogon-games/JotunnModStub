using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CraftingOverhaul.CharacterSheet;
using CraftingOverhaul.Classes;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ClassesUiTests
    {
        private static List<SkillView> AllSkills() => ClassesFixture.AmazonSkills();

        [Fact]
        public void SkillInfoParsesBackWithEveryField()
        {
            // The brief's example answer (classes-ui-brief.md §3, GetSkillInfoJson).
            string json = new JsonOut().Add("id", "MultipleShot").Add("name", "Multiple Shot").Add("tab", 0L).Add("row", 2L).Add("col", 2L)
                .Add("reqLevel", 6L).Add("prereqs", "MagicArrow").Add("kind", "attack").Add("status", "v1").Add("maxPoints", 20L)
                .Add("points", 3L).Add("level", 5L).Add("canSpend", true).Add("spendBlocked", "").Add("description", "Fires 4 arrows in a fan")
                .Add("nextDescription", "Fires 5 arrows in a fan").Add("eitrCost", 5f).ToString();
            var view = SkillView.Parse(json);
            Assert.Equal("Multiple Shot", view.Name);
            Assert.Equal(0, view.Tab);
            Assert.Equal(2, view.Row);
            Assert.Equal(2, view.Col);
            Assert.Equal(new[] { "MagicArrow" }, view.Prereqs);
            Assert.Equal("attack", view.Kind);
            Assert.Equal(3, view.Points);
            Assert.True(view.CanSpend);
            Assert.Equal(5f, view.EitrCost);
            Assert.Null(SkillView.Parse(""));
            Assert.Null(SkillView.Parse("{\"name\":\"No id\"}"));
        }
        [Fact]
        public void TreeShowsTenSkillsPerTabInTheirTierAndCol()
        {
            var skills = AllSkills();
            for (int tab = 0; tab < 3; tab++)
            {
                var shown = SkillTreeLayout.Visible(skills, tab);
                Assert.Equal(10, shown.Count);
                Assert.Equal(shown.Count, shown.Select(s => s.Row * 10 + s.Col).Distinct().Count());
                Assert.All(shown, s => Assert.Equal(SkillTreeLayout.TierLevels[s.Row - 1], s.ReqLevel));
            }
            SkillTreeLayout.TilePosition(1, 1, out float x0, out float y0);
            SkillTreeLayout.TilePosition(6, 3, out float x1, out float y1);
            // Medallions are centred in their cells, clear of the tier labels and inside the grid.
            float margin = (SkillTreeLayout.ColPitch - SkillTreeLayout.Tile) / 2f;
            Assert.Equal(SkillTreeLayout.LabelWidth + margin, x0, 3);
            Assert.Equal((SkillTreeLayout.RowPitch - SkillTreeLayout.Tile) / 2f, y0, 3);
            Assert.Equal(SkillTreeLayout.GridWidth - margin - SkillTreeLayout.Tile, x1, 3);
            Assert.Equal(SkillTreeLayout.GridHeight - (SkillTreeLayout.RowPitch - SkillTreeLayout.Tile) / 2f - SkillTreeLayout.Tile, y1, 3);
        }

        [Fact]
        public void LinesRunFromEachPrerequisiteDownToItsSkill()
        {
            var skills = AllSkills();
            var bow = SkillTreeLayout.Visible(skills, 0);
            var lines = SkillTreeLayout.Lines(bow);
            // Bow tab prerequisites: Multiple Shot←Magic Arrow; Exploding←Fire, Multiple; Ice←Cold; Guided←Cold, Multiple;
            // Strafe←Guided; Immolation←Exploding; Freezing←Ice: 9 lines.
            Assert.Equal(9, lines.Count);
            Assert.All(lines, l => Assert.True(l.Y2 > l.Y1));
            // Same column: straight down from the rim of one medallion to the rim of the next.
            var straight = lines.Single(l => l.From == "MagicArrow" && l.To == "MultipleShot");
            Assert.Equal(straight.X1, straight.X2, 3);
            Assert.Equal(-90f, straight.AngleDegrees, 3);
            Assert.Equal(SkillTreeLayout.RowPitch - SkillTreeLayout.Tile - 2f * SkillTreeLayout.PathGap, straight.Length, 3);
            // Another column: aimed from centre to centre, starting and ending just outside the rims.
            var slanted = lines.Single(l => l.From == "MultipleShot" && l.To == "ExplodingArrow");
            SkillTreeLayout.CentrePosition(2, 2, out float ax, out float ay);
            SkillTreeLayout.CentrePosition(3, 3, out float bx, out float by);
            float rim = SkillTreeLayout.Tile / 2f + SkillTreeLayout.PathGap;
            Assert.Equal(rim, Distance(ax, ay, slanted.X1, slanted.Y1), 3);
            Assert.Equal(rim, Distance(bx, by, slanted.X2, slanted.Y2), 3);
            Assert.True(slanted.X2 > slanted.X1 && slanted.Y2 > slanted.Y1);
        }

        private static float Distance(float x1, float y1, float x2, float y2)
        {
            return (float)System.Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        [Fact]
        public void DroppedSkillsLeaveAnEmptyPlaceAndNoLines()
        {
            var skills = AllSkills();
            skills.Single(x => x.Id == "Dodge").Status = "dropped";
            var magic = SkillTreeLayout.Visible(skills, 1);
            Assert.Equal(9, magic.Count);
            Assert.DoesNotContain(SkillTreeLayout.Lines(magic), l => l.From == "Dodge");
        }

        [Fact]
        public void JsonOutEscapesAndReadsBack()
        {
            string json = new JsonOut().Add("a", "say \"hi\"\n").Add("n", 3L).Add("f", 1.5f).Add("b", true).ToString();
            Assert.Equal("say \"hi\"\n", FlatJson.Get(json, "a"));
            Assert.Equal(3, FlatJson.GetInt(json, "n", 0));
            Assert.Equal(1.5f, FlatJson.GetFloat(json, "f", 0f));
            Assert.True(FlatJson.GetBool(json, "b", false));
            Assert.Equal(new[] { "x", "y\"z" }, FlatJson.Strings(JsonOut.Array(new[] { "x", "y\"z" })));
            Assert.Equal(3520485254L, FlatJson.GetLong("{\"xp\":3520485254}", "xp", 0));
        }
    
        [Fact]
        public void CooldownShowsTimeLeftAndNothingWhenReady()
        {
            Assert.Equal("", CooldownDisplay.Label(0f));
            Assert.Equal("1", CooldownDisplay.Label(0.2f));
            Assert.Equal("3", CooldownDisplay.Label(2.01f));
            Assert.Equal("59", CooldownDisplay.Label(59f));
            Assert.Equal("1:05", CooldownDisplay.Label(64.5f));
            Assert.Equal("20:00", CooldownDisplay.Label(1200f));
            Assert.Equal(0f, CooldownDisplay.Sweep(0f, 8f));
            Assert.Equal(0.5f, CooldownDisplay.Sweep(4f, 8f), 3);
            Assert.Equal(1f, CooldownDisplay.Sweep(9f, 8f));
            Assert.Equal(0f, CooldownDisplay.Sweep(3f, 0f));
        }

        [Fact]
        public void ReadyFlashStartsWhenTheCooldownEndsAndFades()
        {
            Assert.True(CooldownDisplay.BecameReady(0.1f, 0f));
            Assert.False(CooldownDisplay.BecameReady(0f, 0f));
            Assert.False(CooldownDisplay.BecameReady(2f, 1.5f));
            Assert.Equal(1f, CooldownDisplay.Flash(0f), 3);
            Assert.True(CooldownDisplay.Flash(0.4f) < 1f && CooldownDisplay.Flash(0.4f) > 0f);
            Assert.Equal(0f, CooldownDisplay.Flash(CooldownDisplay.FlashSeconds));
            Assert.Equal(0f, CooldownDisplay.Flash(-1f));
        }
    }
}
