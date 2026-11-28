using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Rimeburst (ice explosion on death) and Guarded (a corrupted boss guards the heart), corrupted-only map affixes of
    ///     #97 (Krogon's decisions 1-18 of 2026-11-28, claude/wizardly-wozniak-4aix76).
    /// </summary>
    public class IceBurstGuardianTests
    {
        private static readonly string[] Four = { "glorious", "teeming", "brutal", "frenzied" };

        private static MapTables Tables()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "map_affixes.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var errors = new List<string>();
            var t = new MapTables
            {
                Tiers = MapTierTable.Parse(File.ReadAllText(Path.Combine(dir, "Corruption", "Data", "map_tiers.csv")), errors),
                Affixes = MapAffixTable.Parse(File.ReadAllText(Path.Combine(dir, "Corruption", "Data", "map_affixes.csv")), errors)
            };
            Assert.Empty(errors);
            return t;
        }

        private static MapState Full(MapTables t)
        {
            var s = new MapState();
            foreach (var id in Four)
            {
                s.Affixes.Add(id);
                s.Values[id] = t.Affixes.Find(id).Min;
            }
            s.Refresh(t.Tiers);
            return s;
        }

        [Fact]
        public void TheRowsMatchTheDecisions()
        {
            var a = Tables().Affixes;
            var ice = a.Find("rimeburst");
            Assert.Equal("ice_burst", ice.Effect);
            Assert.Equal(50.0, ice.Min);    // decision 7
            Assert.Equal(100.0, ice.Max);
            Assert.True(ice.CorruptedOnly);
            Assert.False(ice.Earns);
            Assert.Equal("Enemies explode in ice on death.", ice.Danger);   // decision 10: no number shown
            var boss = a.Find("guarded");
            Assert.Equal("guardian", boss.Effect);
            Assert.True(boss.CorruptedOnly);
            Assert.False(boss.Earns);
            Assert.Equal("A corrupted boss guards the heart.", boss.Danger);   // decision 18
            Assert.DoesNotContain(a.Rollable, x => x.Id == "rimeburst" || x.Id == "guarded");
        }

        [Fact]
        public void ThornsRimeburstAndGuardedAreEachOneInThree()
        {
            var t = Tables();
            var rng = new Random(7);
            var counts = new Dictionary<string, int> { { "thorns", 0 }, { "rimeburst", 0 }, { "guarded", 0 } };
            const int n = 6000;
            for (int i = 0; i < n; i++)
            {
                var done = MapCorruptRules.Apply(Full(t), t, out _, () => rng.NextDouble());
                Assert.Equal(5, done.Affixes.Count);
                counts[done.Affixes.Last()]++;
            }
            foreach (var c in counts.Values)
            {
                Assert.InRange(c, n / 3 - 300, n / 3 + 300);
            }
        }

        [Fact]
        public void RimeburstRollsItsValueAndGuardedHasNone()
        {
            var t = Tables();
            t.Affixes.SetDisabled("thorns,guarded");
            var rng = new Random(1);
            var seen = new HashSet<double>();
            for (int i = 0; i < 300; i++)
            {
                var done = MapCorruptRules.Apply(Full(t), t, out _, () => rng.NextDouble());
                Assert.Equal("rimeburst", done.Affixes.Last());
                Assert.InRange(done.Values["rimeburst"], 50.0, 100.0);
                seen.Add(done.Values["rimeburst"]);
            }
            Assert.Contains(50.0, seen);
            Assert.Contains(100.0, seen);

            t.Affixes.SetDisabled("thorns,rimeburst");
            var boss = MapCorruptRules.Apply(Full(t), t, out _, () => 0.4);
            Assert.Equal("guarded", boss.Affixes.Last());
            Assert.True(RipTierRules.HasGuardian(boss.Rolls(t.Affixes)));
            Assert.False(RipTierRules.HasGuardian(Full(t).Rolls(t.Affixes)));
        }

        [Fact]
        public void TheServerKeepsEitherOnACorruptedFullMapOnly()
        {
            var t = Tables();
            var ok = MapData.Sanitize("glorious:60;teeming;brutal:30;frenzied:40;rimeburst:70", 6, t, null);
            Assert.Equal(5, ok.Affixes.Count);
            Assert.Equal(70.0, ok.Values["rimeburst"]);
            var clamped = MapData.Sanitize("glorious:60;teeming;brutal:30;frenzied:40;rimeburst:500", 6, t, null);
            Assert.Equal(100.0, clamped.Values["rimeburst"]);
            var guarded = MapData.Sanitize("glorious:60;teeming;brutal:30;frenzied:40;guarded", 6, t, null);
            Assert.Contains("guarded", guarded.Affixes);
            var notCorrupted = MapData.Sanitize("glorious;teeming;brutal;frenzied;guarded", 5, t, null);
            Assert.DoesNotContain("guarded", notCorrupted.Affixes);
        }

        [Fact]
        public void BothLinesAreLastUnderFaceInRed()
        {
            var t = Tables();
            foreach (var id in new[] { "rimeburst", "guarded" })
            {
                var s = Full(t);
                s.Corrupted = true;
                s.Affixes.Add(id);
                s.Values[id] = t.Affixes.Find(id).Min;
                s.Refresh(t.Tiers);
                var row = t.Tiers.Row(s.Tier);
                var face = MapText.FaceLines(row, s.Rolls(t.Affixes), t.Affixes, MapGlyphs.Fancy);
                Assert.Contains(t.Affixes.Find(id).Danger, face[face.Count - 1]);
                string tooltip = MapText.Tooltip(row, "Meadows", s.Rolls(t.Affixes), t.Affixes, 2.5, MapGlyphs.Fancy, "", "");
                Assert.Contains("<color=" + MapText.CorruptedColor + ">" + t.Affixes.Find(id).Danger + "</color>", tooltip);
            }
        }

        [Fact]
        public void TheBurstIsAShareOfTheStrongestHit()
        {
            var t = Tables();
            var rolls = new List<MapAffixRoll> { new MapAffixRoll { Affix = t.Affixes.Find("rimeburst"), Value = 80 } };
            Assert.Equal(0.8, RipTierRules.IceBurstShare(rolls), 6);
            Assert.Equal(0.0, RipTierRules.IceBurstShare(null));
            // Stars: the game's 1 + 0.5 per level above 1.
            Assert.Equal(1.0, RipTierRules.LevelDamageFactor(1));
            Assert.Equal(1.5, RipTierRules.LevelDamageFactor(2));
            Assert.Equal(2.0, RipTierRules.LevelDamageFactor(3));
            Assert.Equal(1.0, RipTierRules.LevelDamageFactor(0));
            // The strongest weapon (20 vs 35), a 2-star (x1.5), Brutal +20% (x1.2): 63.
            Assert.Equal(63.0, RipTierRules.StrongestHit(new[] { 20.0, 35.0 }, 2, 1.2), 6);
            Assert.Equal(0.0, RipTierRules.StrongestHit(new double[0], 2, 1.2));
            Assert.Equal(50.4, RipTierRules.IceBurstDamage(0.8, 63.0), 6);
            Assert.Equal(0.0, RipTierRules.IceBurstDamage(0.5, 0.1));   // below the game's 0.1
            Assert.Equal(0.0, RipTierRules.IceBurstDamage(0.0, 63.0));
            // Decisions 6 and 8.
            Assert.Equal(1.5, RipTierRules.IceBurstDelay);
            Assert.InRange(RipTierRules.IceBurstRadius, 3.01, 3.02);
        }

        [Fact]
        public void TheGuardianIsTheBiomesBossAtItsFirstPhase()
        {
            Assert.Equal("Eikthyr", RipTierRules.GuardianPrefab("Eikthyr"));
            Assert.Equal("gd_king", RipTierRules.GuardianPrefab("gd_king"));
            Assert.Equal("FrozenKing", RipTierRules.GuardianPrefab("FrozenKing_p3"));
            Assert.Null(RipTierRules.GuardianPrefab(""));
            Assert.True(RipTierRules.HeartCanBeDamaged(false, false));
            Assert.True(RipTierRules.HeartCanBeDamaged(true, false));
            Assert.False(RipTierRules.HeartCanBeDamaged(true, true));
        }

        [Fact]
        public void AValueLessEffectMustHaveNoRange()
        {
            var errors = new List<string>();
            MapAffixTable.Parse("id,status,name,danger,weight,effect,min,max,group,section,notes,pool\n" +
                                "a,approved,A,Danger,100,guardian,1,2,,face,x,corrupted\n", errors);
            Assert.Contains(errors, e => e.Contains("guardian has no value"));
        }
    }
}
