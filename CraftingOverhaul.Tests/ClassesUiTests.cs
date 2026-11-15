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
            string json = new JsonOut().Add("id", "MultipleShot").Add("name", "Multiple Shot").Add("tab", 0L).Add("row", 2L).Add("col", 1L)
                .Add("kind", "attack").Add("status", "v1").Add("maxPoints", 20L)
                .Add("points", 3L).Add("level", 5L).Add("canSpend", true).Add("spendBlocked", "").Add("description", "Fires 4 arrows in a fan")
                .Add("nextDescription", "Fires 5 arrows in a fan").Add("eitrCost", 5f).ToString();
            var view = SkillView.Parse(json);
            Assert.Equal("Multiple Shot", view.Name);
            Assert.Equal(0, view.Tab);
            Assert.Equal(2, view.Row);
            Assert.Equal(1, view.Col);
            Assert.Equal("attack", view.Kind);
            Assert.Equal(3, view.Points);
            Assert.True(view.CanSpend);
            Assert.Equal(5f, view.EitrCost);
            Assert.Null(SkillView.Parse(""));
            Assert.Null(SkillView.Parse("{\"name\":\"No id\"}"));
        }
        [Fact]
        public void EveryTabShowsNineCardsThreeByThree()
        {
            // Krogon, 2026-11-15: exactly nine skills per tab, as cards.
            var skills = AllSkills();
            for (int tab = 0; tab < 3; tab++)
            {
                var shown = SkillCardsLayout.Visible(skills, tab);
                Assert.Equal(9, shown.Count);
                Assert.Equal(9, shown.Select(s => s.Row * 10 + s.Col).Distinct().Count());
                Assert.All(shown, s => Assert.InRange(s.Row, 1, SkillCardsLayout.Rows));
                Assert.All(shown, s => Assert.InRange(s.Col, 1, SkillCardsLayout.Cols));
            }
            // The bow tab reads in his order, row by row.
            Assert.Equal(new[] { "Forked Lightning", "Wind Lance", "Snipe", "Multiple Shot", "Strafe", "Freezing Arrow", "Immolation Arrow", "Venom Strike", "Shackle Shot" },
                SkillCardsLayout.Visible(skills, 0).OrderBy(s => s.Row * 10 + s.Col).Select(s => s.Name));
        }

        [Fact]
        public void CardsFillTheGridWithoutOverlapping()
        {
            var first = SkillCardsLayout.Card(1, 1);
            var last = SkillCardsLayout.Card(3, 3);
            Assert.Equal(0f, first.X);
            Assert.Equal(0f, first.Y);
            Assert.Equal(SkillCardsLayout.GridWidth, last.X + last.Width, 3);
            Assert.Equal(SkillCardsLayout.GridHeight, last.Y + last.Height, 3);
            var next = SkillCardsLayout.Card(1, 2);
            var below = SkillCardsLayout.Card(2, 1);
            Assert.Equal(SkillCardsLayout.Gap, next.X - (first.X + first.Width), 3);
            Assert.Equal(SkillCardsLayout.Gap, below.Y - (first.Y + first.Height), 3);
            // Everything on a card sits inside it, top to bottom: medallion, name, rank, rank bar.
            var parts = new[] { SkillCardsLayout.Icon, SkillCardsLayout.Name, SkillCardsLayout.Rank, SkillCardsLayout.RankBar };
            float y = 0f;
            foreach (var part in parts)
            {
                Assert.True(part.X >= 0f && part.X + part.Width <= SkillCardsLayout.CardWidth);
                Assert.True(part.Y >= y && part.Y + part.Height <= SkillCardsLayout.CardHeight);
                y = part.Y + part.Height;
            }
        }

        [Fact]
        public void CardStatesAndRankLines()
        {
            var skill = new SkillView { Id = "A", Status = "v1", MaxPoints = 20, Points = 0, CanSpend = false };
            Assert.Equal(CardState.NotLearned, SkillCardsLayout.StateOf(skill));
            Assert.Equal("Not learned", SkillCardsLayout.RankLabel(skill));
            Assert.Equal(0f, SkillCardsLayout.RankFill(skill));
            skill.CanSpend = true;
            Assert.Equal(CardState.CanLearn, SkillCardsLayout.StateOf(skill));
            skill.Points = 3;
            Assert.Equal(CardState.Learned, SkillCardsLayout.StateOf(skill));
            Assert.Equal("Rank 3 / 20", SkillCardsLayout.RankLabel(skill));
            Assert.Equal(0.15f, SkillCardsLayout.RankFill(skill), 3);
            skill.Points = 20;
            Assert.Equal(CardState.Maxed, SkillCardsLayout.StateOf(skill));
            Assert.Equal(1f, SkillCardsLayout.RankFill(skill));
            skill.Status = "later";
            skill.Points = 0;
            Assert.Equal(CardState.Later, SkillCardsLayout.StateOf(skill));
            Assert.Equal("Coming later", SkillCardsLayout.RankLabel(skill));
        }

        [Fact]
        public void DroppedSkillsLeaveAnEmptyPlace()
        {
            var skills = AllSkills();
            skills.Single(x => x.Id == "Dodge").Status = "dropped";
            Assert.Equal(8, SkillCardsLayout.Visible(skills, 1).Count);
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
