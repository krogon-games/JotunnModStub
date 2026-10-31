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
            return SkillRules.SpendBlocked(t, t.Find(id), level, points, unspent, 20, pace);
        }

        private static Dictionary<string, int> Points(params (string Id, int N)[] p) =>
            p.ToDictionary(x => x.Id, x => x.N, StringComparer.OrdinalIgnoreCase);

        [Fact]
        public void TheRealTableIsTheAmazonsTree()
        {
            var t = Amazon();
            Assert.Equal(30, t.Skills.Count);
            // class-design.md §5: 19 in the first version (18 v1 + Penetrate reworked), 11 later, none dropped.
            Assert.Equal(19, t.Skills.Count(s => s.Built));
            Assert.Equal(11, t.Skills.Count(s => s.Status == "later"));
            Assert.Equal("reworked", t.Find("Penetrate").Status);
            Assert.Equal(10, t.Skills.Count(s => s.Tab == 0));
            Assert.Equal(10, t.Skills.Count(s => s.Tab == 1));
            Assert.Equal(10, t.Skills.Count(s => s.Tab == 2));
        }

        [Fact]
        public void KindsAndWeaponsFollowTheDesign()
        {
            var t = Amazon();
            Assert.All(t.Skills.Where(s => s.Tab == 0), s => Assert.Equal("bow", s.Needs));
            Assert.All(t.Skills.Where(s => s.Tab == 2), s => Assert.Equal("spear", s.Needs));
            Assert.Equal(new[] { "Avoid", "CriticalStrike", "Dodge", "Evade", "Penetrate", "Pierce" },
                t.Skills.Where(s => s.Kind == "passive").Select(s => s.Id).OrderBy(x => x));
            Assert.Equal(new[] { "Decoy", "InnerSight", "SlowMissiles", "Valkyrie" },
                t.Skills.Where(s => s.Kind == "cast").Select(s => s.Id).OrderBy(x => x));
        }

        [Fact]
        public void PrerequisitesAreDiablo2s()
        {
            var t = Amazon();
            Assert.Equal(new[] { "FireArrow", "MultipleShot" }, t.Find("ExplodingArrow").Prereqs);
            Assert.Equal(new[] { "Decoy", "Evade" }, t.Find("Valkyrie").Prereqs);
            Assert.Empty(t.Find("Jab").Prereqs);
        }

        [Fact]
        public void BuiltPrerequisitesNeedAPoint()
        {
            var t = Amazon();
            Assert.Equal("Needs Magic Arrow", Blocked(t, "MultipleShot", 6, Points()));
            Assert.Equal("", Blocked(t, "MultipleShot", 6, Points(("MagicArrow", 1))));
            // Exploding Arrow needs both.
            Assert.Equal("Needs Multiple Shot", Blocked(t, "ExplodingArrow", 12, Points(("FireArrow", 1), ("MagicArrow", 1))));
        }

        [Fact]
        public void LaterPrerequisitesAreSkipped()
        {
            // class-design.md §4: a prerequisite that isn't built doesn't block. Strafe itself is later, so test the rule
            // with Freezing Arrow's built chain and a later skill's own state.
            var t = Amazon();
            Assert.Equal("Coming later", Blocked(t, "Strafe", 30, Points(("GuidedArrow", 1))));
            // Pierce (later) needs Penetrate (reworked, built): the later skill can't take points either way.
            Assert.Equal("Coming later", Blocked(t, "Pierce", 30, Points(("Penetrate", 1))));
        }

        [Fact]
        public void LevelTiersAndDiablo2sPace()
        {
            var t = Amazon();
            Assert.Equal("Needs level 6", Blocked(t, "Dodge", 5, Points()));
            Assert.Equal("", Blocked(t, "Dodge", 6, Points()));
            // D2: one more point per level past the required level. At level 6, Dodge can hold 1; the 2nd needs level 7.
            Assert.Equal("Needs level 7", Blocked(t, "Dodge", 6, Points(("Dodge", 1))));
            Assert.Equal("", Blocked(t, "Dodge", 7, Points(("Dodge", 1))));
            // Without the pace rule, only points and the cap count.
            Assert.Equal("", Blocked(t, "Dodge", 6, Points(("Dodge", 1)), pace: false));
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
            // Arreat / D2R: Multiple Shot 4 mana at level 1, +1 per level; Magic Arrow 1.5, -0.125 per level, free from level 13.
            Assert.Equal(4f, SkillRules.EitrCost(t.Find("MultipleShot"), 1), 3);
            Assert.Equal(8f, SkillRules.EitrCost(t.Find("MultipleShot"), 5), 3);
            Assert.Equal(1.5f, SkillRules.EitrCost(t.Find("MagicArrow"), 1), 3);
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("MagicArrow"), 13), 3);
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("MagicArrow"), 20), 3);
            Assert.Equal(0f, SkillRules.EitrCost(t.Find("Dodge"), 5), 3);
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
            SkillTable.Parse("id,name,tab,row,col,req_level,prereqs,status,kind,needs,eitr,eitr_per_level,description\n" +
                             "A,A,0,2,1,1,Missing,maybe,attack,,1,0,x\nB,B,0,2,1,6,,v1,passive,,0,0,y\n", "t.csv", errors);
            Assert.Contains(errors, e => e.Contains("status"));
            Assert.Contains(errors, e => e.Contains("row 2 is level 6"));
            Assert.Contains(errors, e => e.Contains("place another skill"));
            Assert.Contains(errors, e => e.Contains("Missing"));
        }
    }
}
