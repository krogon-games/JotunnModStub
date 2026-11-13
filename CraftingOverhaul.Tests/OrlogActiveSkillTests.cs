using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogActiveSkillTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "amazon_skill_values.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static string Data(string file) => File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", file));

        private static SkillValueTable Values()
        {
            var errors = new List<string>();
            var t = SkillValueTable.Parse(Data("amazon_skill_values.csv"), "amazon_skill_values.csv", errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        private static SkillTable Tree()
        {
            var errors = new List<string>();
            var t = SkillTable.Parse(Data("amazon_skills.csv"), "amazon_skills.csv", errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        private static DamageScale Scale()
        {
            var errors = new List<string>();
            var t = DamageScale.Parse(Data("skill_damage_scale.csv"), "skill_damage_scale.csv", errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        [Fact]
        public void EveryBuiltActiveSkillHasValuesAndText()
        {
            var v = Values();
            var built = Tree().Skills.Where(s => s.Built && s.Kind != "passive").ToList();
            Assert.Equal(built.Select(s => s.Id).OrderBy(x => x), v.SkillIds.OrderBy(x => x));
            // The bow skills and Inner Sight (C5) have their tooltip text; the spear skills come with C6.
            foreach (var id in new[] { "MagicArrow", "FireArrow", "ColdArrow", "MultipleShot", "ExplodingArrow", "IceArrow", "FreezingArrow", "InnerSight" })
            {
                Assert.False(string.IsNullOrEmpty(ActiveSkillRules.Describe(id, v, 1, 1f, 25f)), id);
            }
        }

        [Fact]
        public void ValuesAreD2sNumbers()
        {
            // data/d2-reference/amazon_levels.csv, levels 1 and 20.
            var v = Values();
            Assert.Equal(2f, v.Get("MultipleShot", "arrows", 1));
            Assert.Equal(21f, v.Get("MultipleShot", "arrows", 20));
            Assert.Equal(1f, v.Get("FireArrow", "fire_damage_min", 1));
            Assert.Equal(70f, v.Get("FireArrow", "fire_damage_max", 20));
            Assert.Equal(2f, v.Get("IceArrow", "freeze_s", 1));
            Assert.Equal(3.3f, v.Get("FreezingArrow", "radius_yards", 5));
            Assert.Equal(84f, v.Get("InnerSight", "duration_s", 20));
            Assert.Equal(0f, v.Get("MultipleShot", "arrows", 0));
            Assert.Equal(21f, v.Get("MultipleShot", "arrows", 30));
            Assert.Equal(0f, v.Get("Nope", "arrows", 3));
        }

        [Fact]
        public void EitrCostUsesD2RsManaWhereTheTableHasIt()
        {
            var v = Values();
            var t = Tree();
            // Magic Arrow (D2R): 1.5 at level 1, free from level 13.
            Assert.Equal(1.5f, ActiveSkillRules.EitrCost(t.Find("MagicArrow"), v, 1));
            Assert.Equal(0f, ActiveSkillRules.EitrCost(t.Find("MagicArrow"), v, 13));
            Assert.Equal(4f, ActiveSkillRules.EitrCost(t.Find("MultipleShot"), v, 1));
            Assert.Equal(23f, ActiveSkillRules.EitrCost(t.Find("MultipleShot"), v, 20));
            Assert.Equal(0f, ActiveSkillRules.EitrCost(t.Find("Dodge"), v, 5));
            // Without a values row, the tree's formula.
            Assert.Equal(SkillRules.EitrCost(t.Find("Jab"), 3), ActiveSkillRules.EitrCost(t.Find("Jab"), new SkillValueTable(), 3));
        }

        [Fact]
        public void DamageScaleKeepsD2sRatioToTheBow()
        {
            var s = Scale();
            Assert.Equal(99, s.Count);
            // Level 1: crude bow 22 + fire arrow 33 (game dump) over D2's short bow 1-4 (2.5).
            Assert.Equal(22f, s.At(1), 2);
            Assert.Equal(s.At(1), s.At(-5));
            Assert.Equal(s.At(99), s.At(150));
            Assert.All(Enumerable.Range(1, 99), l => Assert.True(s.At(l) > 0f));
        }

        [Fact]
        public void DamageRollsBetweenMinAndMaxAndScales()
        {
            Assert.Equal(10f, ActiveSkillRules.Damage(1f, 4f, 0f, 10f));
            Assert.Equal(40f, ActiveSkillRules.Damage(1f, 4f, 1f, 10f));
            Assert.Equal(25f, ActiveSkillRules.Damage(1f, 4f, 0.5f, 10f));
            Assert.Equal(40f, ActiveSkillRules.Damage(1f, 4f, 7f, 10f));
        }

        [Fact]
        public void SlotsSaveAndPlace()
        {
            var slots = ActiveSkillRules.ParseSlots("MagicArrow;;FireArrow");
            Assert.Equal(new[] { "MagicArrow", "", "FireArrow", "" }, slots);
            Assert.Equal("MagicArrow;;FireArrow;", ActiveSkillRules.FormatSlots(slots));
            // A skill already in another slot moves.
            Assert.Equal(new[] { "", "", "FireArrow", "MagicArrow" }, ActiveSkillRules.Place(slots, 3, "MagicArrow"));
            Assert.Equal(new[] { "MagicArrow", "", "", "" }, ActiveSkillRules.Place(slots, 2, ""));
            Assert.Equal(slots, ActiveSkillRules.Place(slots, 7, "InnerSight"));
            Assert.Equal(4, ActiveSkillRules.ParseSlots(null).Length);
        }

        [Fact]
        public void SlotRefusals()
        {
            var t = Tree();
            Assert.Equal("Passive skills don't take a slot", ActiveSkillRules.SlotRefusal(t.Find("Dodge"), 3));
            Assert.Equal("Coming later", ActiveSkillRules.SlotRefusal(t.Find("Strafe"), 0));
            Assert.Equal("Not learned yet", ActiveSkillRules.SlotRefusal(t.Find("MagicArrow"), 0));
            Assert.Equal("", ActiveSkillRules.SlotRefusal(t.Find("MagicArrow"), 1));
            Assert.Equal("Unknown skill", ActiveSkillRules.SlotRefusal(null, 1));
        }

        [Fact]
        public void SlotStateChecksWeaponThenEitr()
        {
            var t = Tree();
            var bow = t.Find("FireArrow");
            var s = ActiveSkillRules.State(bow, "", 50f, 3f);
            Assert.False(s.Usable);
            Assert.Equal("weapon", s.Blocked);
            Assert.Equal("bow", s.Needs);
            Assert.Equal("Need bow", ActiveSkillRules.BlockedMessage(s));
            s = ActiveSkillRules.State(bow, "bow", 2f, 3f);
            Assert.Equal("eitr", s.Blocked);
            Assert.Equal("Not enough eitr", ActiveSkillRules.BlockedMessage(s));
            Assert.True(ActiveSkillRules.State(bow, "bow", 3f, 3f).Usable);
            // A cast skill needs no weapon.
            Assert.True(ActiveSkillRules.State(t.Find("InnerSight"), "", 5f, 5f).Usable);
            // A free skill works with no eitr at all.
            Assert.True(ActiveSkillRules.State(t.Find("MagicArrow"), "bow", 0f, 0f).Usable);
        }

        [Fact]
        public void SelectedSlotIsRememberedPerWeaponKind()
        {
            var sel = ActiveSkillRules.ParseSelected("bow:2;spear:-1;junk;axe:9");
            Assert.Equal(2, sel["bow"]);
            Assert.Equal(-1, sel["spear"]);
            Assert.False(sel.ContainsKey("axe"));
            Assert.Equal("bow:2;spear:-1", ActiveSkillRules.FormatSelected(sel));
        }

        [Fact]
        public void FanIsEvenAndCentred()
        {
            Assert.Equal(0f, ActiveSkillRules.FanAngle(0, 1));
            Assert.Equal(-2.5f, ActiveSkillRules.FanAngle(0, 2));
            Assert.Equal(2.5f, ActiveSkillRules.FanAngle(1, 2));
            // 21 arrows: 60° in all, 3° apart.
            Assert.Equal(-30f, ActiveSkillRules.FanAngle(0, 21), 3);
            Assert.Equal(30f, ActiveSkillRules.FanAngle(20, 21), 3);
            Assert.Equal(0f, ActiveSkillRules.FanAngle(10, 21), 3);
        }

        [Fact]
        public void ConversionIsClamped()
        {
            Assert.Equal(5f, ActiveSkillRules.Converted(100f, 5f));
            Assert.Equal(100f, ActiveSkillRules.Converted(100f, 150f));
            Assert.Equal(0f, ActiveSkillRules.Converted(100f, -5f));
        }

        [Fact]
        public void TooltipsUseValheimNumbers()
        {
            var v = Values();
            Assert.Equal("Adds 10–40 fire damage; 3% of the physical damage becomes fire", ActiveSkillRules.Describe("FireArrow", v, 1, 10f, 25f));
            Assert.Equal("Fires 2 arrows in a fan for one arrow, each at 75% damage", ActiveSkillRules.Describe("MultipleShot", v, 1, 10f, 25f));
            Assert.Contains("3 m", ActiveSkillRules.Describe("FreezingArrow", v, 1, 1f, 25f));
            Assert.Contains("25% more damage for 8 s", ActiveSkillRules.Describe("InnerSight", v, 1, 1f, 25f));
            Assert.Null(ActiveSkillRules.Describe("FireArrow", v, 0, 1f, 25f));
        }

        [Fact]
        public void QuickCastOnlyForSkillsWithoutAWeapon()
        {
            // User 2026-11-13: per skill, on by default; a weapon skill goes on right click.
            var t = Tree();
            var inner = t.Find("InnerSight");
            var fire = t.Find("FireArrow");
            Assert.True(ActiveSkillRules.CanQuickCast(inner));
            Assert.False(ActiveSkillRules.CanQuickCast(fire));
            Assert.False(ActiveSkillRules.CanQuickCast(t.Find("Dodge")));
            Assert.False(ActiveSkillRules.CanQuickCast(null));
            var off = ActiveSkillRules.ParseIds("");
            Assert.True(ActiveSkillRules.IsQuickCast(inner, off));
            Assert.False(ActiveSkillRules.OnSkillButton(inner, off));
            off = ActiveSkillRules.ParseIds("InnerSight;;innersight");
            Assert.Single(off);
            Assert.False(ActiveSkillRules.IsQuickCast(inner, off));
            Assert.True(ActiveSkillRules.OnSkillButton(inner, off));
            Assert.True(ActiveSkillRules.OnSkillButton(fire, off));
            Assert.False(ActiveSkillRules.OnSkillButton(t.Find("Dodge"), off));
            Assert.Equal("A;B", ActiveSkillRules.FormatIds(new[] { "B", "A" }));
        }

        [Fact]
        public void BrokenTablesAreReported()
        {
            string header = "id,value," + string.Join(",", Enumerable.Range(1, 20).Select(i => "l" + i)) + "\n";
            string row = string.Join(",", Enumerable.Range(1, 20));
            var errors = new List<string>();
            SkillValueTable.Parse(header + "A,x," + row + "\nA,x," + row + "\n", "v.csv", errors);
            Assert.Contains(errors, e => e.Contains("listed twice"));
            errors.Clear();
            DamageScale.Parse("level,scale\n1,2\n1,3\n2,0\n", "s.csv", errors);
            Assert.Equal(2, errors.Count);
        }
    }
}
