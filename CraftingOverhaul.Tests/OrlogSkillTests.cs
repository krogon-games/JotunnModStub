using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogSkillTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "amazon_skills.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static SkillTable Amazon()
        {
            var errors = new List<string>();
            var table = SkillTable.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", "amazon_skills.csv")), "amazon_skills.csv", errors);
            Assert.True(errors.Count == 0, "amazon_skills.csv has problems:\n" + string.Join("\n", errors));
            return table;
        }

        private static string Blocked(SkillTable t, string id, int level, Dictionary<string, int> points, int unspent = 5, bool pace = true)
        {
            return SkillRules.SpendBlocked(t.Find(id), level, points, unspent, 20, pace);
        }

        private static Dictionary<string, int> Points(params (string Id, int N)[] p) =>
            p.ToDictionary(x => x.Id, x => x.N, StringComparer.OrdinalIgnoreCase);

        [Fact]
        public void TheRealTableIsTheAmazonsNinePerTab()
        {
            // Krogon, 2026-11-15: every tab has exactly nine skills, on a three-by-three grid.
            var t = Amazon();
            Assert.Equal(27, t.Skills.Count);
            for (int tab = 0; tab < SkillTable.Tabs; tab++)
            {
                var skills = t.Skills.Where(s => s.Tab == tab).ToList();
                Assert.Equal(SkillTable.SkillsPerTab, skills.Count);
                Assert.Equal(Enumerable.Range(0, 9), skills.Select(s => (s.Row - 1) * 3 + s.Col - 1).OrderBy(x => x));
            }
            // The bow tab's list, in his order.
            Assert.Equal(new[] { "ForkedLightning", "WindLance", "Snipe", "MultipleShot", "Strafe", "FreezingArrow", "ImmolationArrow", "VenomStrike", "ShackleShot" },
                t.Skills.Where(s => s.Tab == 0).Select(s => s.Id));
            // Removed by the overhaul.
            foreach (var gone in new[] { "MagicArrow", "FireArrow", "ColdArrow", "ExplodingArrow", "IceArrow", "GuidedArrow", "SlowMissiles", "PowerStrike" })
            {
                Assert.Null(t.Find(gone));
            }
            // Built: Multishot and Freezing Arrow; Inner Sight and the five passives; the five spear skills whose
            // numbers are in (they act with roadmap C6).
            Assert.Equal(13, t.Skills.Count(s => s.Built));
            Assert.Equal(14, t.Skills.Count(s => s.Status == "later"));
            Assert.Equal("reworked", t.Find("Penetrate").Status);
            // Renamed (Krogon, 2026-11-15); the id stays, so saved points and slots still find it.
            Assert.Equal("Multishot", t.Find("MultipleShot").Name);
        }

        [Fact]
        public void KindsAndWeaponsFollowTheDesign()
        {
            var t = Amazon();
            Assert.All(t.Skills.Where(s => s.Tab == 0), s => Assert.Equal("bow", s.Needs));
            Assert.All(t.Skills.Where(s => s.Tab == 2), s => Assert.Equal("spear", s.Needs));
            Assert.Equal(new[] { "Avoid", "CriticalStrike", "Dodge", "Evade", "Penetrate", "Pierce" },
                t.Skills.Where(s => s.Kind == "passive").Select(s => s.Id).OrderBy(x => x));
            Assert.Equal(new[] { "Decoy", "InnerSight", "Valkyrie" },
                t.Skills.Where(s => s.Kind == "cast").Select(s => s.Id).OrderBy(x => x));
        }

        [Fact]
        public void AnyBuiltSkillCanBeLearnedAtLevelOne()
        {
            // Krogon, 2026-11-15: "Players will be able to learn whichever skill they want even at level 1."
            var t = Amazon();
            foreach (var skill in t.Skills.Where(s => s.Built))
            {
                Assert.Equal("", Blocked(t, skill.Id, 1, Points()));
            }
            // Nothing else has to be learned first: Freezing Arrow with no other points.
            Assert.Equal("", Blocked(t, "FreezingArrow", 1, Points()));
            Assert.Equal("", Blocked(t, "Evade", 1, Points(("Dodge", 0))));
        }

        [Fact]
        public void SkillsNotBuiltYetCannotTakePoints()
        {
            var t = Amazon();
            foreach (var skill in t.Skills.Where(s => !s.Built))
            {
                Assert.Equal("Coming later", Blocked(t, skill.Id, 99, Points()));
            }
        }

        [Fact]
        public void Diablo2sPaceIsOnePointPerLevel()
        {
            var t = Amazon();
            // With every skill open at level 1, a skill holds at most as many points as the character has levels.
            Assert.Equal("Needs level 2", Blocked(t, "Dodge", 1, Points(("Dodge", 1))));
            Assert.Equal("", Blocked(t, "Dodge", 2, Points(("Dodge", 1))));
            Assert.Equal("Needs level 8", Blocked(t, "Dodge", 7, Points(("Dodge", 7))));
            // Without the pace rule, only points and the cap count.
            Assert.Equal("", Blocked(t, "Dodge", 1, Points(("Dodge", 5)), pace: false));
        }

        [Fact]
        public void PointsInRemovedSkillsComeBack()
        {
            // A character saved before the overhaul: 3 in Magic Arrow and 1 in Fire Arrow (both removed), 2 in Multishot.
            var t = Amazon();
            var saved = SkillRules.Parse("MagicArrow:3;FireArrow:1;MultipleShot:2");
            var known = SkillRules.KnownOnly(saved, t);
            Assert.Equal(new[] { "MultipleShot" }, known.Keys);
            Assert.Equal(2, known["MultipleShot"]);
            // Level 10 has earned 9 points: 3 unspent with the old skills counted, 7 once they're handed back.
            Assert.Equal(3, SkillRules.Unspent(10, 1, saved));
            Assert.Equal(7, SkillRules.Unspent(10, 1, known));
            Assert.Equal("MultipleShot:2", SkillRules.Format(known));
            // Without a table (no class) nothing is dropped.
            Assert.Equal(3, SkillRules.KnownOnly(saved, null).Count);
        }

        [Fact]
        public void CapAndPoints()
        {
            var t = Amazon();
            Assert.Equal("Maxed", Blocked(t, "Jab", 50, Points(("Jab", 20))));
            Assert.Equal("No skill points", Blocked(t, "Jab", 50, Points(("Jab", 3)), unspent: 0));
            Assert.Equal(9, SkillRules.Unspent(11, 1, Points(("Jab", 1))));
            Assert.Equal(0, SkillRules.Unspent(2, 1, Points(("Jab", 5))));
        }

        [Fact]
        public void EitrCostIsDiablo2sMana()
        {
            var t = Amazon();
            // Arreat / D2R: Multishot 4 mana at level 1, +1 per level; Decoy 19, -0.75 per level; never below 0.
            Assert.Equal(4f, SkillRules.EitrCost(t.Find("MultipleShot"), 1), 3);
            Assert.Equal(8f, SkillRules.EitrCost(t.Find("MultipleShot"), 5), 3);
            Assert.Equal(19f, SkillRules.EitrCost(t.Find("Decoy"), 1), 3);
            Assert.Equal(4.75f, SkillRules.EitrCost(t.Find("Decoy"), 20), 3);
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("Decoy"), 40), 3);
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("Dodge"), 5), 3);
            // The new skills have no cost until their numbers are agreed.
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("WindLance"), 1), 3);
        }

        [Fact]
        public void SavedPointsRoundTrip()
        {
            var points = SkillRules.Parse("MagicArrow:3;FireArrow:1;Bad;Zero:0");
            Assert.Equal(2, points.Count);
            Assert.Equal(3, points["magicarrow"]);
            Assert.Equal("FireArrow:1;MagicArrow:3", SkillRules.Format(points));
            Assert.Empty(SkillRules.Parse(null));
        }

        [Fact]
        public void BrokenTablesAreReported()
        {
            var errors = new List<string>();
            SkillTable.Parse("id,name,tab,row,col,status,kind,needs,eitr,eitr_per_level,description\n" +
                             "A,A,0,2,1,maybe,attack,,1,0,x\nB,B,0,2,1,v1,passive,,0,0,y\nC,C,0,4,1,v1,attack,,0,0,z\n", "t.csv", errors);
            Assert.Contains(errors, e => e.Contains("status"));
            Assert.Contains(errors, e => e.Contains("place another skill"));
            Assert.Contains(errors, e => e.Contains("outside the grid"));
            Assert.Contains(errors, e => e.Contains("tab 0 has 3 skills; every tab has exactly 9"));
        }
    }
}
