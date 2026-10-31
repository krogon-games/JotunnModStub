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
        private static List<SkillView> AllSkills(DemoClasses demo)
        {
            return FlatJson.Strings(demo.GetSkillIdsJson(DemoClasses.AmazonId)).Select(id => SkillView.Parse(demo.GetSkillInfoJson(id))).ToList();
        }

        [Fact]
        public void DemoCharacterMatchesTheBriefsExample()
        {
            var json = new DemoClasses().GetCharacterJson();
            Assert.Equal("Amazon", FlatJson.Get(json, "class"));
            Assert.Equal(12, FlatJson.GetInt(json, "level", 0));
            Assert.Equal(4410, FlatJson.GetLong(json, "xp", 0));
            Assert.Equal(3720, FlatJson.GetLong(json, "xpLevelStart", 0));
            Assert.Equal(4925, FlatJson.GetLong(json, "xpNextLevel", 0));
            Assert.Equal(5, FlatJson.GetInt(json, "statPoints", 0));
            Assert.Equal(1, FlatJson.GetInt(json, "skillPoints", 0));
            Assert.Equal(40, FlatJson.GetInt(json, "dexterity", 0));
        }

        [Fact]
        public void DemoSkillsMatchTheD2ReferenceTable()
        {
            // The demo copies data/d2-reference/amazon.csv (never shipped); this keeps the copy honest.
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "data", "d2-reference", "amazon.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var rows = File.ReadAllLines(Path.Combine(dir, "data", "d2-reference", "amazon.csv")).Skip(1).Select(l => l.Split(',')).ToList();
            var demo = new DemoClasses();
            string[] tabs = { "Bow and Crossbow", "Passive and Magic", "Javelin and Spear" };
            Assert.Equal(rows.Count, demo.Skills.Count);
            foreach (var r in rows)
            {
                var skill = demo.Find(DemoClasses.IdOf(r[3]));
                Assert.NotNull(skill);
                Assert.Equal(Array.IndexOf(tabs, r[0]), skill.Tab);
                Assert.Equal(int.Parse(r[1]), skill.Row);
                Assert.Equal(int.Parse(r[2]), skill.Col);
                Assert.Equal(int.Parse(r[4]), skill.ReqLevel);
                string prereqs = r[5] == "none" ? "" : string.Join("+", r[5].Split('+').Select(DemoClasses.IdOf));
                Assert.Equal(prereqs, skill.Prereqs);
                Assert.Equal(r[6] == "passive", skill.Kind == "passive");
            }
        }

        [Fact]
        public void SkillInfoParsesBackWithEveryField()
        {
            var demo = new DemoClasses();
            var view = SkillView.Parse(demo.GetSkillInfoJson("MultipleShot"));
            Assert.Equal("Multiple Shot", view.Name);
            Assert.Equal(0, view.Tab);
            Assert.Equal(2, view.Row);
            Assert.Equal(2, view.Col);
            Assert.Equal(new[] { "MagicArrow" }, view.Prereqs);
            Assert.Equal("attack", view.Kind);
            Assert.Equal(3, view.Points);
            Assert.True(view.CanSpend);
            Assert.Equal(4f, view.EitrCost);
            Assert.Null(SkillView.Parse(""));
            Assert.Null(SkillView.Parse(demo.GetSkillInfoJson("NoSuchSkill")));
        }

        [Fact]
        public void SpendingFollowsTheTreeRules()
        {
            var demo = new DemoClasses();
            Assert.Equal("Needs level 18", demo.SpendSkillPoint("IceArrow"));
            Assert.Equal("Coming later", demo.SpendSkillPoint("Strafe"));
            // Dodge (level 6, no prerequisite) is open; Avoid needs a point in Dodge first.
            Assert.Equal("Needs Dodge", demo.SpendSkillPoint("Avoid"));
            int before = demo.ChangeCounter;
            Assert.Equal("", demo.SpendSkillPoint("Dodge"));
            Assert.Equal(before + 1, demo.ChangeCounter);
            Assert.Equal(0, demo.SkillPoints);
            Assert.Equal("No skill points", demo.SpendSkillPoint("FireArrow"));
            Assert.Equal(1, demo.Find("Dodge").Points);
        }

        [Fact]
        public void APrerequisiteThatIsntBuiltDoesntBlock()
        {
            // Class-design §4: Strafe needs Guided Arrow, which is "later"; once Strafe is built it opens by level alone.
            var demo = new DemoClasses();
            demo.Level = 30;
            var strafe = demo.Find("Strafe");
            strafe.Status = "v1";
            Assert.Equal("", demo.SpendBlocked(strafe));
        }

        [Fact]
        public void StatPointsRaiseAttributesUntilGone()
        {
            var demo = new DemoClasses();
            Assert.Equal("+40% bow damage", FlatJson.Get(demo.GetAttributeInfoJson("dexterity"), "effect"));
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal("", demo.SpendStatPoint("vitality"));
            }
            Assert.Equal("No stat points", demo.SpendStatPoint("vitality"));
            Assert.Equal(30, FlatJson.GetInt(demo.GetAttributeInfoJson("vitality"), "value", 0));
            Assert.Equal("+90 health", FlatJson.Get(demo.GetAttributeInfoJson("vitality"), "effect"));
            Assert.Equal("Unknown attribute", demo.SpendStatPoint("luck"));
            Assert.Equal("", demo.GetAttributeInfoJson("luck"));
        }

        [Fact]
        public void SlotsTakeLearnedActiveSkillsAndMoveThem()
        {
            var demo = new DemoClasses();
            Assert.Equal("Passive skills don't take a slot", demo.SetSkillSlot(2, "CriticalStrike"));
            Assert.Equal("Learn it first", demo.SetSkillSlot(2, "Jab"));
            Assert.Equal("Coming later", demo.SetSkillSlot(2, "Decoy"));
            // Fire Arrow is in slot 3; putting it in slot 2 moves it.
            Assert.Equal("", demo.SetSkillSlot(2, "FireArrow"));
            var bar = demo.GetSkillBarJson();
            Assert.Equal("FireArrow", FlatJson.Get(bar, "slot2"));
            Assert.Equal("", FlatJson.Get(bar, "slot3"));
            Assert.Equal("", demo.SetSkillSlot(0, ""));
            Assert.Equal("", FlatJson.Get(demo.GetSkillBarJson(), "slot0"));
            Assert.Equal("No such slot", demo.SetSkillSlot(4, "FireArrow"));
        }

        [Fact]
        public void SlotStatesFollowTheWeaponAndEitr()
        {
            var demo = new DemoClasses();
            // Slots: Multiple Shot (bow), Inner Sight (cast, no weapon), empty, Fire Arrow (bow).
            Assert.True(FlatJson.GetBool(demo.GetSlotStateJson(0, 0f, "bow", 50f), "usable", false));
            var noBow = demo.GetSlotStateJson(0, 0f, "spear", 50f);
            Assert.Equal("weapon", FlatJson.Get(noBow, "blocked"));
            Assert.Equal("bow", FlatJson.Get(noBow, "needs"));
            Assert.Equal("eitr", FlatJson.Get(demo.GetSlotStateJson(3, 0f, "bow", 1f), "blocked"));
            // Inner Sight needs no weapon; its cooldown shows on slot 1 for 3 s of every 8.
            var cooling = demo.GetSlotStateJson(1, 1f, "", 50f);
            Assert.Equal("cooldown", FlatJson.Get(cooling, "blocked"));
            Assert.Equal(2f, FlatJson.GetFloat(cooling, "cooldown", 0f), 3);
            Assert.True(FlatJson.GetBool(demo.GetSlotStateJson(1, 5f, "", 50f), "usable", false));
            Assert.Equal("", FlatJson.Get(demo.GetSlotStateJson(1, 5f, "", 50f), "needs"));
            Assert.Equal("", demo.GetSlotStateJson(2, 0f, "bow", 50f)); // empty slot
        }

        [Fact]
        public void TheStoneClearsTheClassAndChoosingSetsIt()
        {
            var demo = new DemoClasses();
            Assert.Equal("You already have a class", demo.ChooseClass("Amazon"));
            demo.UseStone();
            Assert.Equal(1, demo.StoneRequest);
            Assert.Equal("", FlatJson.Get(demo.GetCharacterJson(), "class"));
            Assert.Equal("", demo.GetSkillBarJson());
            Assert.Equal("Unknown class", demo.ChooseClass("Necromancer"));
            Assert.Equal("", demo.ChooseClass("Amazon"));
            Assert.Equal("Amazon", FlatJson.Get(demo.GetCharacterJson(), "class"));
            Assert.Equal(new[] { "Amazon" }, FlatJson.Strings(demo.GetClassIdsJson()));
            Assert.Equal("Javelin and Spear", FlatJson.Get(demo.GetClassInfoJson("Amazon"), "tab2"));
        }

        [Fact]
        public void TreeShowsTenSkillsPerTabInTheirTierAndCol()
        {
            var skills = AllSkills(new DemoClasses());
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
            var skills = AllSkills(new DemoClasses());
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
            var demo = new DemoClasses();
            demo.Find("Dodge").Status = "dropped";
            var magic = SkillTreeLayout.Visible(AllSkills(demo), 1);
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
