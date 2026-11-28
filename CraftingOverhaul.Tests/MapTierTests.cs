using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>Map tiers, map affixes, map orbs, the shard on a map and what a rip's tier changes (Krogon, rips round, 2026-11-26).</summary>
    public class MapTierTests
    {
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

        private static MapState State(MapTables t, bool corrupted = false, bool mirrored = false, params string[] ids)
        {
            var s = new MapState { Affixes = ids.ToList(), Corrupted = corrupted, Mirrored = mirrored };
            foreach (var id in ids)
            {
                s.Values[id] = t.Affixes.Find(id).Min;   // the lowest value, so results are easy to read
            }
            s.Refresh(t.Tiers);
            return s;
        }

        /// <summary>Four affixes that don't share a group: a tier 5 map.</summary>
        private static readonly string[] Four = { "glorious", "teeming", "brutal", "frenzied" };

        private static Func<double> Sequence(params double[] values)
        {
            int i = 0;
            return () => values[Math.Min(i++, values.Length - 1)];
        }

        // ---- the real tables ----

        [Fact]
        public void TierTableHoldsKrogonsNumbers()
        {
            var t = Tables().Tiers;
            Assert.Equal(6, t.Rows.Count);
            // tier, rarity, affixes, corrupted, waves, 2-star %, shards min, max, source, floor
            object[][] expected =
            {
                new object[] { 1, "normal", 0, false, 2.0, 0.0, 1, 4, "rip_tier_1", "none" },
                new object[] { 2, "magic", 1, false, 2.5, 25.0, 2, 6, "rip_tier_2", "magic" },
                new object[] { 3, "magic", 2, false, 3.0, 50.0, 3, 8, "rip_tier_3", "magic" },
                new object[] { 4, "rare", 3, false, 3.5, 75.0, 4, 10, "rip_tier_4", "rare" },
                new object[] { 5, "rare", 4, false, 4.0, 100.0, 5, 12, "rip_tier_5", "rare" },
                new object[] { 6, "rare", 4, true, 4.5, 100.0, 6, 14, "rip_tier_6", "rare" }
            };
            for (int i = 0; i < 6; i++)
            {
                var r = t.Rows[i];
                var e = expected[i];
                Assert.Equal(e[0], r.Tier);
                Assert.Equal(e[1], r.Rarity);
                Assert.Equal(e[2], r.Affixes);
                Assert.Equal(e[3], r.Corrupted);
                Assert.Equal((double)e[4], r.WaveMultiplier);
                Assert.Equal((double)e[5], r.TwoStarPercent);
                Assert.Equal(e[6], r.ShardsMin);
                Assert.Equal(e[7], r.ShardsMax);
                Assert.Equal(e[8], r.DropSource);
                Assert.Equal(e[9], r.HeartFloor);
            }
            Assert.Equal(4, t.MaxAffixes);
            Assert.Equal(6, t.CorruptTier);
        }

        [Fact]
        public void NonNumbersReadAsTheLowestValue()
        {
            // Multiplayer review, 2026-11-27: "NaN" parses as a number and slipped past the clamp.
            var t = Tables();
            var brutal = t.Affixes.Find("brutal");
            Assert.Equal(brutal.Min, brutal.Clamp(double.NaN));
            Assert.Equal(brutal.Min, brutal.Clamp(double.PositiveInfinity));
            var entries = MapData.ParseEntries("brutal:NaN;sturdy:Infinity");
            Assert.All(entries, e => Assert.False(e.HasValue));
        }

        [Fact]
        public void DamageTakenAffixesCantPassAHundred()
        {
            var errors = new List<string>();
            MapAffixTable.Parse("id,status,name,danger,weight,effect,min,max,group,section\nironskin,approved,Ironskin,x,100,physical_taken,95,120,,face", errors);
            Assert.Contains(errors, e => e.Contains("at most 100"));
        }

        [Fact]
        public void AMapWithBothImmunitiesOpensOneTierLower()
        {
            // Krogon's play-test ma-6 (2026-11-28): co_map meadows 3 ironskin warded must open a tier 2 rip. The opener's game
            // runs the same check, so the server's tier 2 is the expected one and the map is used, not kept.
            var t = Tables();
            var expected = MapData.Sanitize("ironskin:95;warded:95", 3, t, null);
            Assert.Equal(2, expected.Tier);
            Assert.Equal(new[] { "ironskin" }, expected.Affixes.ToArray());
        }

        [Fact]
        public void SwitchedOffAffixesNeverRoll()
        {
            // Krogon, 2026-11-27: "make the option to disable some affixes from being able to roll".
            var t = Tables();
            var keep = new[] { "glorious", "teeming" };
            string off = string.Join(", ", t.Affixes.Rollable.Select(x => x.Id).Where(id => !keep.Contains(id)).Select(id => id.ToUpperInvariant()).ToArray());
            var unknown = t.Affixes.SetDisabled(" " + off + ";nosuchaffix ");
            Assert.Equal(new[] { "nosuchaffix" }, unknown.ToArray());
            Assert.Equal(keep, t.Affixes.Rollable.Select(x => x.Id).ToArray());
            // A map that already has Brutal keeps it, but no orb adds a switched-off affix.
            var map = State(t, false, false, "brutal");
            Assert.Equal(keep, MapOrbRules.Candidates(map, t).Select(x => x.Id).ToArray());
            // Two left: Exalted on a tier 4 map with brutal + frenzied + ruthless has nothing to add after the two are taken.
            var full = State(t, false, false, "glorious", "teeming", "brutal");
            Assert.NotNull(MapOrbRules.Refusal("exalted", full, t));
            // Clearing the list brings them back.
            t.Affixes.SetDisabled("");
            Assert.Equal(t.Affixes.Affixes.Count(a => a.Approved), t.Affixes.Rollable.Count());
        }

        [Fact]
        public void AffixTableHoldsKrogonsNumbers()
        {
            var a = Tables().Affixes;
            // id, effect, min, max, group (every approved row of map_affixes.csv, Krogon's list 2026-11-27)
            object[][] expected =
            {
                new object[] { "glorious", "experience", 50.0, 100.0, "" },
                new object[] { "teeming", "density", 50.0, 50.0, "" },
                new object[] { "brutal", "damage", 20.0, 100.0, "" },
                new object[] { "frenzied", "attack_speed", 25.0, 100.0, "" },
                new object[] { "ruthless", "double_damage", 10.0, 50.0, "" },
                new object[] { "sturdy", "health", 25.0, 50.0, "health" },
                new object[] { "mighty", "health", 50.0, 100.0, "health" },
                new object[] { "fleet", "move_speed", 25.0, 25.0, "" },
                new object[] { "unyielding", "no_stagger", 0.0, 0.0, "" },
                new object[] { "ironskin", "physical_taken", 95.0, 95.0, "immunity" },
                new object[] { "warded", "elemental_taken", 95.0, 95.0, "immunity" },
                new object[] { "withering", "life_drain", 5.0, 5.0, "" },
                new object[] { "exhausting", "stamina_drain", 5.0, 5.0, "" },
                new object[] { "exposed", "resistances", 25.0, 50.0, "" },
                new object[] { "venomous", "poison_share", 25.0, 75.0, "" },
                new object[] { "searing", "fire_share", 25.0, 75.0, "" },
                new object[] { "stormcalled", "lightning_share", 25.0, 75.0, "" }
            };
            Assert.Equal(expected.Select(e => (string)e[0]).ToArray(), a.Rollable.Select(x => x.Id).ToArray());
            Assert.All(a.Rollable, x => Assert.Equal(100.0, x.Weight));
            foreach (var e in expected)
            {
                var affix = a.Find((string)e[0]);
                Assert.Equal((string)e[1], affix.Effect);
                Assert.Equal((double)e[2], affix.Min);
                Assert.Equal((double)e[3], affix.Max);
                Assert.Equal((string)e[4], affix.Group);
                Assert.Equal((double)e[3] > (double)e[2], affix.Ranged);
                Assert.Contains(affix.Effect, MapAffixTable.Effects.All);
            }
            foreach (var id in new[] { "frostbitten", "drowned", "choking" })
            {
                var later = a.Find(id);
                Assert.Equal("later", later.Status);
                Assert.False(later.Approved);
                Assert.DoesNotContain(later, a.Rollable);
            }
            Assert.Equal(20, a.Affixes.Count);
            Assert.Equal("Enemies deal {value}% more damage.", a.Find("brutal").Danger);
        }

        [Fact]
        public void RipTierDropRatesGoUpByAQuarterPerTier()
        {
            var errors = new List<string>();
            var drops = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.Empty(errors);
            float[] expected = { 1.25f, 1.5f, 1.75f, 2f, 2.25f, 2.5f };
            for (int tier = 1; tier <= 6; tier++)
            {
                var sources = new[] { "rip_tier_" + tier };
                foreach (var column in new[] { "magic", "rare", "unique", DropTable.OrbColumn })
                {
                    Assert.Equal(expected[tier - 1], drops.Multiplier(column, sources));
                }
            }
            // every tier's drop_source exists in the modifier table
            foreach (var row in Tables().Tiers.Rows)
            {
                Assert.True(drops.Modifiers.ContainsKey(row.DropSource), row.DropSource);
            }
        }

        // ---- editing mistakes ----

        private const string TierHeader = "tier,status,rarity,affixes,corrupted,wave_multiplier,two_star_percent,shards_min,shards_max,drop_source,heart_floor,notes\n";

        private static List<string> TierErrors(string rows)
        {
            var errors = new List<string>();
            MapTierTable.Parse(TierHeader + rows, errors);
            return errors;
        }

        [Fact]
        public void TierTableMistakesAreReported()
        {
            Assert.Empty(TierErrors("1,approved,normal,0,false,2,0,1,4,rip_tier_1,none,x\n2,approved,rare,4,true,3,10,2,6,rip_tier_2,rare,x\n"));
            Assert.NotEmpty(TierErrors("1,approved,normal,0,false,2,0,1,4,rip_tier_1,none,x\n3,approved,rare,4,true,3,10,2,6,rip_tier_2,rare,x\n")); // gap
            Assert.NotEmpty(TierErrors("1,approved,normal,0,false,2,0,1,4,rip_tier_1,none,x\n")); // no corrupted row
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,2,0,1,4,rip_tier_1,none,x\n2,approved,rare,4,false,3,10,2,6,rip_tier_2,rare,x\n")); // corrupted not last
            Assert.NotEmpty(TierErrors("1,approved,epic,0,true,2,0,1,4,rip_tier_1,none,x\n")); // rarity
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,0,0,1,4,rip_tier_1,none,x\n")); // waves
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,2,0,5,4,rip_tier_1,none,x\n")); // shards min > max
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,2,0,1,4,Rip Tier,none,x\n")); // source
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,2,0,1,4,rip_tier_1,epic,x\n")); // floor
            Assert.NotEmpty(TierErrors("1,approved,normal,2,false,2,0,1,4,rip_tier_1,none,x\n2,approved,magic,1,true,2,0,1,4,rip_tier_2,none,x\n")); // affixes fall
            Assert.NotEmpty(TierErrors("1,approved,normal,0,true,2,x,1,4,rip_tier_1,none,x\n")); // not a number
            var errors = new List<string>();
            MapTierTable.Parse("tier,status\n1,approved\n", errors);
            Assert.NotEmpty(errors);
        }

        private const string AffixHeader = "id,status,name,danger,weight,effect,min,max,group,section,notes\n";

        private static List<string> AffixErrors(string rows)
        {
            var errors = new List<string>();
            MapAffixTable.Parse(AffixHeader + rows, errors);
            return errors;
        }

        [Fact]
        public void AffixTableMistakesAreReported()
        {
            Assert.Empty(AffixErrors("a,approved,A,Danger {value},100,damage,20,100,,face,x\nb,later,B,Danger,,,,,,face,x\nc,approved,C,Danger,100,no_stagger,,,grp,face,x\n"));
            Assert.NotEmpty(AffixErrors("A1,approved,A,Danger,100,damage,20,100,,face,x\n")); // id
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,20,100,,face,x\na,approved,A,Danger,100,damage,20,100,,face,x\n")); // twice
            Assert.NotEmpty(AffixErrors("a,proposed,A,Danger,100,damage,20,100,,face,x\n")); // status
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,,damage,20,100,,face,x\n")); // no weight
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,0,damage,20,100,,face,x\n")); // weight 0
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,20,100,,face,x\n")); // approved without an effect
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,sparkle,20,100,,face,x\n")); // unknown effect
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,,,,face,x\n")); // no range
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,x,100,,face,x\n")); // not a number
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,0,100,,face,x\n")); // min 0
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,100,20,,face,x\n")); // min above max
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,no_stagger,20,,,face,x\n")); // no_stagger with a value
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,damage,20,100,Grp1,face,x\n")); // group
            Assert.NotEmpty(AffixErrors("a,approved,A,,100,damage,20,100,,face,x\n")); // no danger
            Assert.NotEmpty(AffixErrors("a,later,A,Danger,,damage,100,20,,face,x\n")); // later with min above max
        }

        // ---- tiers ----

        [Fact]
        public void TierForGivesEveryRowItsTier()
        {
            var t = Tables().Tiers;
            foreach (var row in t.Rows)
            {
                Assert.Equal(row.Tier, t.TierFor(row.Affixes, row.Corrupted));
            }
            Assert.Equal(3, t.Row(4).Affixes);
            Assert.Equal(5, t.TierFor(9, false)); // more than the most counts as the most
            Assert.Equal(6, t.TierFor(0, true));
            Assert.Equal(1, t.Row(0).Tier);
            Assert.Equal(6, t.Row(99).Tier);
        }

        // ---- the map's data ----

        [Fact]
        public void OldMapsWithNoDataAreTierOne()
        {
            var t = Tables();
            var s = MapData.Read(new Dictionary<string, string>(), t.Tiers, t.Affixes, null);
            Assert.Equal(1, s.Tier);
            Assert.Empty(s.Affixes);
            Assert.False(s.Corrupted);
            Assert.False(s.Mirrored);
            Assert.Equal(1, MapData.Read(null, t.Tiers, t.Affixes, null).Tier);
            // an old map that only carries other keys
            Assert.Equal(1, MapData.Read(new Dictionary<string, string> { { "x", "y" } }, t.Tiers, t.Affixes, null).Tier);
        }

        [Fact]
        public void TheTierIsDerivedNeverTrusted()
        {
            var t = Tables();
            var data = new Dictionary<string, string> { { MapData.TierKey, "6" }, { MapData.AffixesKey, "teeming;brutal:60" } };
            Assert.Equal(3, MapData.Read(data, t.Tiers, t.Affixes, null).Tier);
            data[MapData.CorruptedKey] = "1";
            Assert.Equal(6, MapData.Read(data, t.Tiers, t.Affixes, null).Tier);
        }

        [Fact]
        public void UnknownAffixIdsAreDroppedAndReported()
        {
            var t = Tables();
            var unknown = new List<string>();
            var data = new Dictionary<string, string> { { MapData.AffixesKey, "teeming;gone;teeming;;brutal" } };
            var s = MapData.Read(data, t.Tiers, t.Affixes, unknown);
            Assert.Equal(new[] { "teeming", "brutal" }, s.Affixes.ToArray());
            Assert.Equal(new[] { "gone" }, unknown.ToArray());
            Assert.Equal(3, s.Tier);
        }

        [Fact]
        public void LaterAffixesAreDroppedLikeUnknownOnesAndDontCountTowardTheTier()
        {
            var t = Tables();
            var unknown = new List<string>();
            var data = new Dictionary<string, string> { { MapData.AffixesKey, "frostbitten;brutal:60;drowned:1;choking" } };
            var s = MapData.Read(data, t.Tiers, t.Affixes, unknown);
            Assert.Equal(new[] { "brutal" }, s.Affixes.ToArray());
            Assert.Equal(new[] { "frostbitten", "drowned", "choking" }, unknown.ToArray());
            Assert.Equal(2, s.Tier);
        }

        [Fact]
        public void AffixesHaveASectionAndGloriousIsTheOnlyReward()
        {
            var a = Tables().Affixes;
            Assert.Equal(new[] { "glorious" }, a.Affixes.Where(x => x.Earns).Select(x => x.Id).ToArray());
            Assert.All(a.Affixes.Where(x => x.Approved), x => Assert.Contains(x.Section, new[] { "face", "earn" }));
            Assert.Contains(AffixErrors("a,approved,A,Danger,100,damage,20,100,,,x\n"), e => e.Contains("needs a section"));
            Assert.Contains(AffixErrors("a,approved,A,Danger,100,damage,20,100,,loot,x\n"), e => e.Contains("section 'loot'"));
            Assert.Empty(AffixErrors("a,approved,A,Danger,100,damage,20,100,,earn,x\nb,later,B,Danger,,,,,,,x\n"));
            var errors = new List<string>();
            var table = MapAffixTable.Parse(AffixHeader + "a,approved,A,Danger,100,damage,20,100,,earn,x\nb,later,B,Danger,,,,,,,x\n", errors);
            Assert.True(table.Find("a").Earns);
            Assert.False(table.Find("b").Earns);   // blank = face
            var noColumn = new List<string>();
            MapAffixTable.Parse("id,status,name,danger,weight,effect,min,max,group,notes\n", noColumn);
            Assert.Contains(noColumn, e => e.Contains("missing column 'section'"));
        }

        // ---- the server's check of a rip request ----

        [Fact]
        public void SanitizeKeepsAGoodRequestAsIs()
        {
            var t = Tables();
            var warnings = new List<string>();
            var s = MapData.Sanitize("brutal:60;teeming:50", 3, t, warnings);
            Assert.Equal(new[] { "brutal", "teeming" }, s.Affixes.ToArray());
            Assert.Equal(60, s.Values["brutal"]);
            Assert.Equal(3, s.Tier);
            Assert.False(s.Corrupted);
            Assert.Empty(warnings);
        }

        [Fact]
        public void SanitizeDropsUnknownLaterDuplicateAndSecondOfAGroup()
        {
            var t = Tables();
            var warnings = new List<string>();
            var s = MapData.Sanitize("nosuch:5;frostbitten;brutal:60;brutal:90;ironskin;warded;sturdy;mighty", 9, t, warnings);
            Assert.Equal(new[] { "brutal", "ironskin", "sturdy" }, s.Affixes.ToArray());
            Assert.Equal(60, s.Values["brutal"]);   // the repeat doesn't overwrite
            Assert.Equal(4, s.Tier);                // three affixes; a requested corrupted tier on a partial map is ignored
            Assert.False(s.Corrupted);
            Assert.Equal(4, warnings.Count);
            Assert.Contains(warnings, w => w.Contains("unknown map affix 'nosuch'"));
            Assert.Contains(warnings, w => w.Contains("'frostbitten'") && w.Contains("not built yet"));
            Assert.Contains(warnings, w => w.Contains("'warded'") && w.Contains("group"));
            Assert.Contains(warnings, w => w.Contains("'mighty'") && w.Contains("group"));
        }

        [Fact]
        public void SanitizeClampsValuesAndReadsNaNAndNoValueAsTheMin()
        {
            var t = Tables();
            var s = MapData.Sanitize("brutal:9999;frenzied:-5;ruthless:NaN;glorious", 1, t, null);
            Assert.Equal(100, s.Values["brutal"]);
            Assert.Equal(25, s.Values["frenzied"]);
            Assert.Equal(10, s.Values["ruthless"]);
            Assert.Equal(50, s.Values["glorious"]);
        }

        [Fact]
        public void SanitizeKeepsNoMoreThanTheMostAffixes()
        {
            var t = Tables();
            var s = MapData.Sanitize("glorious;teeming;brutal;frenzied;ruthless;fleet", 5, t, null);
            Assert.Equal(Four, s.Affixes.ToArray());
            Assert.Equal(5, s.Tier);
        }

        [Fact]
        public void SanitizeHonoursTheCorruptedTierOnlyOnAFullMap()
        {
            var t = Tables();
            var full = MapData.Sanitize(string.Join(";", Four), 6, t, null);
            Assert.True(full.Corrupted);
            Assert.Equal(6, full.Tier);
            var partial = MapData.Sanitize("glorious;teeming", 6, t, null);
            Assert.False(partial.Corrupted);
            Assert.Equal(3, partial.Tier);
            // A full map that doesn't ask for it isn't corrupted; none at all is tier 1.
            Assert.Equal(5, MapData.Sanitize(string.Join(";", Four), 5, t, null).Tier);
            Assert.Equal(1, MapData.Sanitize("", 6, t, null).Tier);
            // A full map whose fourth affix was dropped is not full.
            Assert.False(MapData.Sanitize("glorious;teeming;brutal;frostbitten", 6, t, null).Corrupted);
        }

        [Fact]
        public void WriteKeepsTheKeysConsistentAndRoundTrips()
        {
            var t = Tables();
            var data = new Dictionary<string, string>();
            var s = State(t, true, true, Four);
            s.Values["brutal"] = 63;
            MapData.Write(data, s, t.Tiers);
            Assert.Equal("glorious:50;teeming:50;brutal:63;frenzied:25", data[MapData.AffixesKey]);
            Assert.Equal("6", data[MapData.TierKey]);
            Assert.Equal("1", data["corruption.corrupted"]);
            Assert.Equal("1", data["corruption.mirrored"]);
            var back = MapData.Read(data, t.Tiers, t.Affixes, null);
            Assert.Equal(s.Affixes, back.Affixes);
            Assert.Equal(63.0, back.Values["brutal"]);
            Assert.True(back.Corrupted);
            Assert.True(back.Mirrored);
            Assert.Equal(6, back.Tier);

            // a white map writes nothing: the keys are removed
            MapData.Write(data, State(t), t.Tiers);
            Assert.Empty(data);

            Assert.Equal("a;b", MapData.Join(new[] { "a", "b" }));
            Assert.Equal(new[] { "a", "b" }, MapData.Split("a;b;").ToArray());
            Assert.Equal(new[] { "a", "b" }, MapData.Split("a:5;b:7.5").ToArray());
            Assert.Empty(MapData.Split(null));
        }

        [Fact]
        public void ValuesAreWrittenInvariantAndReadBackWhateverTheCulture()
        {
            var t = Tables();
            var before = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            try
            {
                Assert.Equal("63", MapData.FormatValue(63));
                Assert.Equal("62.5", MapData.FormatValue(62.5));
                var s = State(t, false, false, "brutal", "exposed");
                s.Values["exposed"] = 33;
                string joined = MapData.JoinRolls(s);
                Assert.Equal("brutal:20;exposed:33", joined);
                var rolls = MapData.JoinRolls(s.Rolls(t.Affixes));
                Assert.Equal(joined, rolls);
                var entries = MapData.ParseEntries("brutal:62.5;exposed");
                Assert.Equal(62.5, entries[0].Value);
                Assert.True(entries[0].HasValue);
                Assert.False(entries[1].HasValue);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = before;
            }
        }

        [Fact]
        public void EntriesWithoutAValueReadAsMinAndStoredValuesAreClamped()
        {
            var t = Tables();
            var unknown = new List<string>();
            var data = new Dictionary<string, string>
            {
                { MapData.AffixesKey, "brutal;frenzied:1000;exposed:abc;teeming:7;swarming:2;unyielding;glorious: 75 " }
            };
            var s = MapData.Read(data, t.Tiers, t.Affixes, unknown);
            Assert.Equal(20.0, s.Values["brutal"]);        // bare id (an old map): Min
            Assert.Equal(100.0, s.Values["frenzied"]);     // above the range: Max
            Assert.Equal(25.0, s.Values["exposed"]);       // unreadable: Min
            Assert.Equal(50.0, s.Values["teeming"]);       // fixed: always its one value
            Assert.Equal(0.0, s.Values["unyielding"]);
            Assert.Equal(75.0, s.Values["glorious"]);
            Assert.Equal(new[] { "swarming" }, unknown.ToArray());
            Assert.Equal(6, s.Affixes.Count);
            Assert.Equal(5, s.Tier);
            var rolls = s.Rolls(t.Affixes);
            Assert.Equal(6, rolls.Count);
            Assert.Equal(100.0, rolls.Single(r => r.Affix.Id == "frenzied").Value);
            // below the range, through the rolls helper
            s.Values["brutal"] = 1;
            Assert.Equal(20.0, s.Rolls(t.Affixes).Single(r => r.Affix.Id == "brutal").Value);
        }

        // ---- orbs ----

        [Fact]
        public void EveryOrbTransitionAndRefusal()
        {
            var t = Tables();
            var white = State(t);

            var r = MapOrbRules.Apply("transmutation", white, t, Sequence(0.0));
            Assert.True(r.Done);
            Assert.Equal(2, r.State.Tier);
            Assert.Equal(new[] { "glorious" }, r.Added.ToArray());
            Assert.Equal(50.0, r.State.Values["glorious"]);   // 0.0 rolls the lowest value
            Assert.Empty(white.Affixes); // the input is untouched
            Assert.Equal("Orb of Transmutation: now tier 2", r.Headline);
            Assert.NotNull(MapOrbRules.Refusal("transmutation", r.State, t));

            var aug = MapOrbRules.Apply("augmentation", r.State, t, Sequence(0.99));
            Assert.Equal(3, aug.State.Tier);
            Assert.Equal(2, aug.State.Affixes.Count);
            Assert.NotNull(MapOrbRules.Refusal("augmentation", white, t));
            Assert.NotNull(MapOrbRules.Refusal("augmentation", aug.State, t)); // 2 affixes

            var regal = MapOrbRules.Apply("regal", aug.State, t, Sequence(0.0));
            Assert.Equal(4, regal.State.Tier);
            Assert.Equal(3, regal.State.Affixes.Count);
            Assert.NotNull(MapOrbRules.Refusal("regal", r.State, t)); // 1 affix
            Assert.NotNull(MapOrbRules.Refusal("regal", white, t));

            var exalted = MapOrbRules.Apply("exalted", regal.State, t, Sequence(0.0));
            Assert.Equal(5, exalted.State.Tier);
            Assert.Equal(4, exalted.State.Affixes.Count);
            Assert.NotNull(MapOrbRules.Refusal("exalted", aug.State, t)); // Magic
            Assert.NotNull(MapOrbRules.Refusal("exalted", exalted.State, t)); // full, and the pool is empty

            foreach (var orb in new[] { "alchemy", "chance" })
            {
                var rare = MapOrbRules.Apply(orb, white, t, Sequence(0.0, 0.0, 0.0));
                Assert.Equal(4, rare.State.Tier);
                Assert.Equal(3, rare.Added.Count);
                Assert.Equal(3, rare.State.Affixes.Distinct().Count());
                Assert.Equal("rare", t.Tiers.Row(rare.State.Tier).Rarity);
                Assert.NotNull(MapOrbRules.Refusal(orb, r.State, t));
            }

            // Divine needs a value to reroll: a white map has none
            Assert.Equal("Map affixes have no values to reroll.", MapOrbRules.Refusal("divine", white, t));
            Assert.NotNull(MapOrbRules.Refusal("jeweller", white, t));
            Assert.NotNull(MapOrbRules.Refusal("nonsense", white, t));
        }

        [Fact]
        public void AnnulmentRemovesOneAndTheTierFallsDownToWhite()
        {
            var t = Tables();
            var full = State(t, false, false, Four);
            Assert.Equal(5, full.Tier);
            var a = MapOrbRules.Apply("annulment", full, t, Sequence(0.5));
            Assert.Equal(new[] { "brutal" }, a.Removed.ToArray());
            Assert.False(a.State.Values.ContainsKey("brutal"));   // its value goes with it
            Assert.True(full.Values.ContainsKey("brutal"));
            Assert.Equal(4, a.State.Tier);
            Assert.Equal(4, full.Affixes.Count);

            var one = State(t, false, false, "brutal");
            var white = MapOrbRules.Apply("annulment", one, t, Sequence(0.0));
            Assert.Equal(1, white.State.Tier);
            Assert.Empty(white.State.Affixes);
            Assert.NotNull(MapOrbRules.Refusal("annulment", white.State, t));
        }

        [Fact]
        public void OrbsPickByWeightAndNeverTheSameAffixTwice()
        {
            var t = Tables();
            // a roll at the very end of the weights picks the last affix
            var last = MapOrbRules.Apply("transmutation", State(t), t, Sequence(0.9999));
            Assert.Equal("stormcalled", last.State.Affixes[0]);
            // many random rolls never duplicate and never give a "later" affix
            var rng = new Random(7);
            for (int i = 0; i < 200; i++)
            {
                var r = MapOrbRules.Apply("alchemy", State(t), t, () => rng.NextDouble());
                Assert.Equal(3, r.State.Affixes.Distinct().Count());
                Assert.All(r.State.Affixes, id => Assert.True(t.Affixes.Find(id).Approved));
            }
            // a pool that has run out refuses with a reason
            var small = new MapTables { Tiers = t.Tiers, Affixes = new MapAffixTable() };
            small.Affixes.Affixes.Add(new MapAffixTable.Affix { Id = "only", Status = "approved", Weight = 1 });
            Assert.Equal(MapOrbRules.TooFewAffixes, MapOrbRules.Refusal("alchemy", State(small), small));
        }

        [Fact]
        public void CorruptedMapsTakeOnlyTheMirror()
        {
            var t = Tables();
            var corrupted = State(t, true, false, Four);
            foreach (var orb in OrbRules.Orbs.Where(o => o != "mirror"))
            {
                Assert.Equal(OrbRules.CorruptedLocked, MapOrbRules.Refusal(orb, corrupted, t));
            }
            var mirror = MapOrbRules.Apply("mirror", corrupted, t, Sequence(0.0));
            Assert.True(mirror.Done);
            Assert.True(mirror.Copy);
            Assert.True(mirror.State.Corrupted);
            Assert.Equal(6, mirror.State.Tier);
        }

        [Fact]
        public void MirrorCopiesRefuseEverything()
        {
            var t = Tables();
            var copy = State(t, false, true, "teeming");
            foreach (var orb in OrbRules.Orbs)
            {
                Assert.NotNull(MapOrbRules.Refusal(orb, copy, t));
            }
            var corruptedCopy = State(t, true, true, Four);
            Assert.NotNull(MapOrbRules.Refusal("mirror", corruptedCopy, t));
            // the Mirror works on a white map too
            Assert.Null(MapOrbRules.Refusal("mirror", State(t), t));
        }

        // ---- the shard ----

        [Fact]
        public void TheShardCorruptsOnlyATierFiveMap()
        {
            var t = Tables();
            var full = State(t, false, false, Four);
            string headline;
            var done = MapCorruptRules.Apply(full, t, out headline);
            Assert.NotNull(done);
            Assert.True(done.Corrupted);
            Assert.Equal(6, done.Tier);
            Assert.Equal(4, done.Affixes.Count); // never destroyed, nothing lost
            Assert.Equal(full.Values, done.Values);
            Assert.Equal("Corrupted: now tier 6", headline);
            Assert.False(full.Corrupted);

            for (int n = 0; n < 4; n++)
            {
                var low = State(t, false, false, Four.Take(n).ToArray());
                Assert.Null(MapCorruptRules.Apply(low, t, out headline));
                Assert.Equal("Only a tier 5 map (4 affixes) can be corrupted.", headline);
            }
            Assert.Equal("This map is already corrupted.", MapCorruptRules.Refusal(done, t));
            Assert.NotNull(MapCorruptRules.Refusal(State(t, false, true, Four), t));
        }

        // ---- what a rip's tier changes ----

        private static MapAffixRoll Roll(MapTables t, string id, double value)
        {
            return new MapAffixRoll { Affix = t.Affixes.Find(id), Value = value };
        }

        [Fact]
        public void WavesMultiplyTheTierAndDensity()
        {
            var t = Tables();
            Assert.Equal(2.5, RipTierRules.WaveMultiplier(t.Tiers.Row(2), null));
            Assert.Equal(2.5, RipTierRules.WaveMultiplier(t.Tiers.Row(2), new[] { Roll(t, "brutal", 60) }));
            Assert.Equal(3.75, RipTierRules.WaveMultiplier(t.Tiers.Row(2), new[] { Roll(t, "teeming", 50) }));
            Assert.Equal(6.75, RipTierRules.WaveMultiplier(t.Tiers.Row(6), new[] { Roll(t, "teeming", 50) }));
        }

        [Fact]
        public void HeartGearIsTheHigherOfTheRollAndTheFloor()
        {
            Assert.Null(RipTierRules.HeartGear(null, "none"));
            Assert.Equal("magic", RipTierRules.HeartGear(null, "magic"));
            Assert.Equal("rare", RipTierRules.HeartGear(null, "rare"));
            Assert.Equal("rare", RipTierRules.HeartGear("rare", "magic"));
            Assert.Equal("rare", RipTierRules.HeartGear("magic", "rare"));
            Assert.Equal("magic", RipTierRules.HeartGear("magic", "none"));
            Assert.Equal("rare", RipTierRules.HeartGear("rare", "none"));
        }

        [Fact]
        public void TwoStarsShardsSourcesAndAffixFactors()
        {
            var t = Tables();
            Assert.False(RipTierRules.IsGuaranteedTwoStar(0, 0.0));
            Assert.True(RipTierRules.IsGuaranteedTwoStar(25, 0.2499));
            Assert.False(RipTierRules.IsGuaranteedTwoStar(25, 0.25));
            Assert.True(RipTierRules.IsGuaranteedTwoStar(100, 0.9999));

            int min, max;
            RipTierRules.ShardRange(t.Tiers.Row(2), out min, out max);
            Assert.Equal(2, min);
            Assert.Equal(6, max);
            Assert.Equal(new[] { "rip_tier_3" }, RipTierRules.DropSources(t.Tiers.Row(3)).ToArray());

        }

        [Fact]
        public void EveryAffixFactorCombinesTwoAffixes()
        {
            var t = Tables();
            var none = new List<MapAffixRoll>();
            Assert.Equal(1.0, RipTierRules.ExperienceFactor(none));
            Assert.Equal(1.0, RipTierRules.PhysicalTaken(null));
            Assert.Equal(0.0, RipTierRules.DoubleDamageChance(none));
            Assert.Equal(0.0, RipTierRules.LifeDrainPerSecond(none));
            Assert.False(RipTierRules.NoStagger(none));

            // two affixes of one effect (the table never rolls this, but explicit maps and future rows can)
            var xp = new[] { Roll(t, "glorious", 50), Roll(t, "glorious", 100) };
            Assert.Equal(1.5 * 2.0, RipTierRules.ExperienceFactor(xp), 9);
            var dmg = new[] { Roll(t, "brutal", 20), Roll(t, "brutal", 50), Roll(t, "frenzied", 100) };
            Assert.Equal(1.2 * 1.5, RipTierRules.DamageFactor(dmg), 9);
            Assert.Equal(2.0, RipTierRules.AttackSpeedFactor(dmg), 9);
            var dbl = new[] { Roll(t, "ruthless", 50), Roll(t, "ruthless", 20) };
            Assert.Equal(1.0 - 0.5 * 0.8, RipTierRules.DoubleDamageChance(dbl), 9);
            var health = new[] { Roll(t, "sturdy", 50), Roll(t, "mighty", 100) };
            Assert.Equal(1.5 * 2.0, RipTierRules.HealthFactor(health), 9);
            var speed = new[] { Roll(t, "fleet", 25), Roll(t, "fleet", 25) };
            Assert.Equal(0.5, RipTierRules.MoveSpeedBonus(speed), 9);
            Assert.True(RipTierRules.NoStagger(new[] { Roll(t, "brutal", 20), Roll(t, "unyielding", 0) }));
            var taken = new[] { Roll(t, "ironskin", 95), Roll(t, "warded", 50), Roll(t, "ironskin", 50) };
            Assert.Equal(0.05 * 0.5, RipTierRules.PhysicalTaken(taken), 9);
            Assert.Equal(0.5, RipTierRules.ElementalTaken(taken), 9);
            var drains = new[] { Roll(t, "withering", 5), Roll(t, "exhausting", 5), Roll(t, "withering", 3) };
            Assert.Equal(8.0, RipTierRules.LifeDrainPerSecond(drains), 9);
            Assert.Equal(5.0, RipTierRules.StaminaDrainPerSecond(drains), 9);
            var exposed = new[] { Roll(t, "exposed", 25), Roll(t, "exposed", 50) };
            Assert.Equal(75.0, RipTierRules.ResistancePenaltyPercent(exposed), 9);
            var shares = new[] { Roll(t, "venomous", 25), Roll(t, "venomous", 25), Roll(t, "searing", 75), Roll(t, "stormcalled", 40) };
            Assert.Equal(0.5, RipTierRules.PoisonShare(shares), 9);
            Assert.Equal(0.75, RipTierRules.FireShare(shares), 9);
            Assert.Equal(0.4, RipTierRules.LightningShare(shares), 9);
            // a mix: each helper reads only its own effect
            var mix = new[] { Roll(t, "brutal", 100), Roll(t, "glorious", 50) };
            Assert.Equal(2.0, RipTierRules.DamageFactor(mix), 9);
            Assert.Equal(1.5, RipTierRules.ExperienceFactor(mix), 9);
            Assert.Equal(1.0, RipTierRules.HealthFactor(mix), 9);
        }

        // ---- values, groups and the Divine Orb (Krogon, 2026-11-27) ----

        [Fact]
        public void OrbsRollWholeValuesInsideTheRange()
        {
            var t = Tables();
            var rng = new Random(3);
            for (int i = 0; i < 300; i++)
            {
                var r = MapOrbRules.Apply("alchemy", State(t), t, () => rng.NextDouble());
                Assert.Equal(3, r.State.Values.Count);
                foreach (var roll in r.State.Rolls(t.Affixes))
                {
                    Assert.InRange(roll.Value, roll.Affix.Min, roll.Affix.Max);
                    Assert.Equal(Math.Floor(roll.Value), roll.Value);
                }
            }
            // the ends of a range are both reachable (brutal 20-100)
            var brutal = t.Affixes.Find("brutal");
            Assert.Equal(20.0, brutal.Roll(() => 0.0));
            Assert.Equal(100.0, brutal.Roll(() => 0.999999));
            Assert.Equal(50.0, t.Affixes.Find("teeming").Roll(() => 0.7));   // fixed
        }

        [Fact]
        public void NoMapRollsTwoAffixesOfAGroup()
        {
            var t = Tables();
            Assert.DoesNotContain("warded", MapOrbRules.Candidates(State(t, false, false, "ironskin"), t).Select(a => a.Id));
            Assert.DoesNotContain("ironskin", MapOrbRules.Candidates(State(t, false, false, "warded"), t).Select(a => a.Id));
            Assert.DoesNotContain("mighty", MapOrbRules.Candidates(State(t, false, false, "sturdy"), t).Select(a => a.Id));
            Assert.DoesNotContain("sturdy", MapOrbRules.Candidates(State(t, false, false, "mighty"), t).Select(a => a.Id));
            Assert.Contains("brutal", MapOrbRules.Candidates(State(t, false, false, "sturdy"), t).Select(a => a.Id));
            // 300 random Alchemy rolls and a full climb with Exalted never hold two of a group
            var rng = new Random(11);
            for (int i = 0; i < 300; i++)
            {
                var r = MapOrbRules.Apply("alchemy", State(t), t, () => rng.NextDouble());
                var ex = MapOrbRules.Apply("exalted", r.State, t, () => rng.NextDouble());
                foreach (var m in new[] { r.State, ex.State })
                {
                    var groups = m.Rolls(t.Affixes).Select(x => x.Affix.Group).Where(g => g.Length > 0).ToList();
                    Assert.Equal(groups.Count, groups.Distinct().Count());
                }
            }
        }

        [Fact]
        public void OrbsRefuseWhenTheGroupsLeaveTooFewAffixes()
        {
            // only ironskin, warded and one free affix are rollable: Alchemy needs three that fit together, and can't have them
            var t = Tables();
            var small = new MapTables { Tiers = t.Tiers, Affixes = new MapAffixTable() };
            foreach (var id in new[] { "ironskin", "warded", "brutal" })
            {
                small.Affixes.Affixes.Add(t.Affixes.Find(id));
            }
            Assert.Equal(MapOrbRules.TooFewAffixes, MapOrbRules.Refusal("alchemy", State(t), small));
            Assert.Null(MapOrbRules.Refusal("transmutation", State(t), small));
            // two free affixes plus a pair in one group is enough for three
            small.Affixes.Affixes.Add(t.Affixes.Find("frenzied"));
            Assert.Null(MapOrbRules.Refusal("alchemy", State(t), small));
            var rng = new Random(5);
            var r = MapOrbRules.Apply("alchemy", State(t), small, () => rng.NextDouble());
            Assert.True(r.Done);
            Assert.Equal(3, r.State.Affixes.Count);
            // a map with ironskin on it has no room left for warded: Exalted on 3 affixes needs 1 candidate
            var three = State(t, false, false, "ironskin", "brutal", "frenzied");
            var only = new MapTables { Tiers = t.Tiers, Affixes = new MapAffixTable() };
            only.Affixes.Affixes.Add(t.Affixes.Find("warded"));
            only.Affixes.Affixes.Add(t.Affixes.Find("ironskin"));
            only.Affixes.Affixes.Add(t.Affixes.Find("brutal"));
            only.Affixes.Affixes.Add(t.Affixes.Find("frenzied"));
            only.Affixes.SetDisabled("ironskin, brutal, frenzied");   // switched off, but the map's ironskin still blocks warded
            Assert.Equal(MapOrbRules.TooFewAffixes, MapOrbRules.Refusal("exalted", three, only));
        }

        [Fact]
        public void DivineRerollsOnlyTheRangedValues()
        {
            var t = Tables();
            var map = State(t, false, false, "teeming", "brutal", "unyielding", "exposed");
            map.Values["brutal"] = 20;
            map.Values["exposed"] = 25;
            Assert.Null(MapOrbRules.Refusal("divine", map, t));
            var r = MapOrbRules.Apply("divine", map, t, Sequence(0.999999, 0.0));
            Assert.True(r.Done);
            Assert.Equal(100.0, r.State.Values["brutal"]);   // first ranged: top of 20-100
            Assert.Equal(25.0, r.State.Values["exposed"]);   // second ranged: bottom of 25-50
            Assert.Equal(50.0, r.State.Values["teeming"]);   // fixed ones untouched
            Assert.Equal(0.0, r.State.Values["unyielding"]);
            Assert.Equal(new[] { "brutal", "exposed" }, r.Rerolled.ToArray());
            Assert.Equal(map.Affixes, r.State.Affixes);
            Assert.Equal(20.0, map.Values["brutal"]);   // the input is untouched
            Assert.Equal(map.Tier, r.State.Tier);
        }

        [Fact]
        public void DivineIsRefusedOnAMapWithNoRangeAndOnCorruptedMaps()
        {
            var t = Tables();
            Assert.Equal(MapOrbRules.NoValues, MapOrbRules.Refusal("divine", State(t, false, false, "teeming", "ironskin", "unyielding"), t));
            Assert.Equal(MapOrbRules.NoValues, MapOrbRules.Refusal("divine", State(t), t));
            Assert.Equal(OrbRules.CorruptedLocked, MapOrbRules.Refusal("divine", State(t, true, false, Four), t));
            Assert.Equal(OrbRules.MirroredLocked, MapOrbRules.Refusal("divine", State(t, false, true, Four), t));
            var none = MapOrbRules.Apply("divine", State(t, false, false, "teeming"), t, Sequence(0.5));
            Assert.False(none.Done);
            Assert.Equal(MapOrbRules.NoValues, none.Refusal);
        }

        [Fact]
        public void MirrorAndTheShardKeepTheValues()
        {
            var t = Tables();
            var map = State(t, false, false, Four);
            map.Values["brutal"] = 77;
            var mirror = MapOrbRules.Apply("mirror", map, t, Sequence(0.0));
            Assert.Equal(77.0, mirror.State.Values["brutal"]);
            string headline;
            Assert.Equal(77.0, MapCorruptRules.Apply(map, t, out headline).Values["brutal"]);
        }
    }
}
