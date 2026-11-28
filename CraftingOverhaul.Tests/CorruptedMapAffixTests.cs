using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The corrupted-only map affixes (#97) and Thorns, the first of them (Krogon's decisions 11-15 of 2026-11-28, claude/nifty-hypatia-afm8ez).</summary>
    public class CorruptedMapAffixTests
    {
        private static readonly string[] Four = { "glorious", "teeming", "brutal", "frenzied" };

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

        private static MapTables Tables()
        {
            var errors = new List<string>();
            var tables = new MapTables
            {
                Tiers = MapTierTable.Parse(DataFile("map_tiers.csv"), errors),
                Affixes = MapAffixTable.Parse(DataFile("map_affixes.csv"), errors)
            };
            Assert.Empty(errors);
            return tables;
        }

        /// <summary>The tables with only Thorns rollable among the corrupted-only affixes (the others came in a later round).</summary>
        private static MapTables ThornsOnly()
        {
            var t = Tables();
            t.Affixes.SetDisabled("rimeburst,guarded");
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
        public void TheShardAddsThornsWithARolledValue()
        {
            var t = ThornsOnly();
            var rng = new Random(3);
            var seen = new HashSet<double>();
            for (int i = 0; i < 200; i++)
            {
                var done = MapCorruptRules.Apply(Full(t), t, out _, () => rng.NextDouble());
                Assert.Equal(6, done.Tier);
                Assert.True(done.Corrupted);
                Assert.Equal("thorns", done.Affixes.Last());
                double v = done.Values["thorns"];
                Assert.InRange(v, 5.0, 25.0);       // decision 12
                Assert.Equal(Math.Round(v), v);
                seen.Add(v);
            }
            Assert.Contains(5.0, seen);
            Assert.Contains(25.0, seen);
        }

        [Fact]
        public void WithThornsSwitchedOffTheShardStillCorrupts()
        {
            var t = Tables();
            t.Affixes.SetDisabled("thorns,rimeburst,guarded");
            var done = MapCorruptRules.Apply(Full(t), t, out _, () => 0.5);
            Assert.Equal(6, done.Tier);
            Assert.Equal(Four, done.Affixes.ToArray());
        }

        [Fact]
        public void OrbsNeverRollThorns()
        {
            var t = Tables();
            Assert.DoesNotContain(t.Affixes.Rollable, a => a.Id == "thorns");
            Assert.DoesNotContain(MapOrbRules.Candidates(new MapState(), t), a => a.Id == "thorns");
            var rng = new Random(11);
            for (int i = 0; i < 300; i++)
            {
                var r = MapOrbRules.Apply("alchemy", new MapState(), t, () => rng.NextDouble());
                Assert.DoesNotContain("thorns", r.State.Affixes);
            }
        }

        [Fact]
        public void ACorruptedMapWithThornsCantBeCorruptedAgain()
        {
            var t = Tables();
            var done = MapCorruptRules.Apply(Full(t), t, out _, () => 0.0);
            Assert.Equal("This map is already corrupted.", MapCorruptRules.Refusal(done, t));
            Assert.Null(MapCorruptRules.AddCorruptedAffix(done, t, () => 0.0));   // never a second
        }

        [Fact]
        public void TheServerKeepsThornsOnlyOnACorruptedMap()
        {
            var t = Tables();
            var warnings = new List<string>();
            var ok = MapData.Sanitize("thorns:12;glorious:60;teeming;brutal:30;frenzied:40", 6, t, warnings);
            Assert.Empty(warnings);
            Assert.True(ok.Corrupted);
            Assert.Equal(6, ok.Tier);
            Assert.Equal(5, ok.Affixes.Count);
            Assert.Equal(12.0, ok.Values["thorns"]);

            // Not corrupted (tier 5 asked): Thorns is dropped.
            warnings.Clear();
            var five = MapData.Sanitize("glorious;teeming;brutal;frenzied;thorns:20", 5, t, warnings);
            Assert.False(five.Corrupted);
            Assert.Equal(Four, five.Affixes.ToArray());
            Assert.Contains(warnings, w => w.Contains("thorns") && w.Contains("isn't corrupted"));

            // 3 normal affixes asking for tier 5 (decision 27): corrupted, Thorns kept. Asking for their own tier 4: no Thorns.
            var three = MapData.Sanitize("glorious;teeming;brutal;thorns", 5, t, null);
            Assert.True(three.Corrupted);
            Assert.Equal(5, three.Tier);
            Assert.Contains("thorns", three.Affixes);
            var four = MapData.Sanitize("glorious;teeming;brutal;thorns", 4, t, null);
            Assert.False(four.Corrupted);
            Assert.DoesNotContain("thorns", four.Affixes);

            // Five normal affixes and Thorns: four normal kept, Thorns kept on top; its value clamped.
            var many = MapData.Sanitize("glorious;teeming;brutal;frenzied;ruthless;thorns:99", 6, t, null);
            Assert.Equal(Four.Concat(new[] { "thorns" }).ToArray(), many.Affixes.ToArray());
            Assert.Equal(25.0, many.Values["thorns"]);
        }

        [Fact]
        public void ThornsReturnsItsShareOfTheDamageDealt()
        {
            var t = Tables();
            var thorns = new List<MapAffixRoll> { new MapAffixRoll { Affix = t.Affixes.Find("thorns"), Value = 15 } };
            Assert.Equal(0.15, RipTierRules.ThornsShare(thorns), 6);
            Assert.Equal(0.0, RipTierRules.ThornsShare(null));
            Assert.Equal(15.0, RipTierRules.ThornsDamage(0.15, 300, 200), 6);   // 100 dealt
            Assert.Equal(6.0, RipTierRules.ThornsDamage(0.15, 40, -20), 6);     // a kill: only the 40 it had
            Assert.Equal(0.0, RipTierRules.ThornsDamage(0.15, 100, 100));       // blocked or nothing dealt
            Assert.Equal(0.0, RipTierRules.ThornsDamage(0.05, 100, 99.5));      // below the game's 0.1
            Assert.Equal(0.0, RipTierRules.ThornsDamage(0.0, 100, 50));
        }

        [Fact]
        public void ThornsIsTheLastDangerLineInRed()
        {
            var t = ThornsOnly();
            var done = MapCorruptRules.Apply(Full(t), t, out _, () => 0.5);
            var row = t.Tiers.Row(done.Tier);
            var face = MapText.FaceLines(row, done.Rolls(t.Affixes), t.Affixes, MapGlyphs.Fancy);
            Assert.Contains("of the damage you deal to them.", face[face.Count - 1]);
            string tooltip = MapText.Tooltip(row, "Meadows", done.Rolls(t.Affixes), t.Affixes, 2.5, MapGlyphs.Fancy, "", "");
            Assert.Contains("<color=" + MapText.CorruptedColor + ">Enemies return 15% of the damage you deal to them.</color>", tooltip);
            Assert.DoesNotContain("<color=" + MapText.CorruptedColor + ">Enemies deal", tooltip);
            string hover = MapText.Hover(row, done.Rolls(t.Affixes), t.Affixes);
            Assert.Contains("<color=" + MapText.CorruptedColor + ">Enemies return", hover);
        }

        [Fact]
        public void APoolOtherThanCorruptedIsAnError()
        {
            var errors = new List<string>();
            MapAffixTable.Parse("id,status,name,danger,weight,effect,min,max,group,section,notes,pool\n" +
                                "a,approved,A,Danger {value}%,100,thorns,5,25,,face,x,cursed\n", errors);
            Assert.Contains(errors, e => e.Contains("pool 'cursed'"));
            errors.Clear();
            MapAffixTable.Parse("id,status,name,danger,weight,effect,min,max,group,section,notes,pool\n" +
                                "a,approved,A,Danger {value}%,100,thorns,5,250,,face,x,corrupted\n", errors);
            Assert.Contains(errors, e => e.Contains("at most 100"));
        }
    }
}
