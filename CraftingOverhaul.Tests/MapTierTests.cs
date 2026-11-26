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
            s.Refresh(t.Tiers);
            return s;
        }

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
        public void AffixTableHoldsKrogonsNumbers()
        {
            var a = Tables().Affixes;
            Assert.Equal(7, a.Affixes.Count);
            Assert.Equal(new[] { "swarming", "hardened", "rabid", "smouldering" }, a.Rollable.Select(x => x.Id).ToArray());
            Assert.All(a.Rollable, x => Assert.Equal(100.0, x.Weight));
            Assert.Equal(2.0, a.Find("swarming").WaveMultiplier);
            Assert.Equal(0.7, a.Find("hardened").DamageTaken);
            Assert.Equal(2.0, a.Find("rabid").MaddenedFactor);
            Assert.Equal(0.25, a.Find("smouldering").FireShare);
            foreach (var id in new[] { "frostbitten", "drowned", "choking" })
            {
                var later = a.Find(id);
                Assert.Equal("later", later.Status);
                Assert.False(later.Approved);
                Assert.DoesNotContain(later, a.Rollable);
            }
            Assert.Equal("Twice as many creatures in every wave", a.Find("swarming").Danger);
            Assert.Equal("Rip creatures take 30% less damage", a.Find("hardened").Danger);
            Assert.Equal("Rip creatures hit 40% harder and move 30% faster", a.Find("rabid").Danger);
            Assert.Equal("Every hit from a rip creature adds 25% of its damage as fire", a.Find("smouldering").Danger);
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

        private const string AffixHeader = "id,status,name,danger,weight,wave_multiplier,damage_taken,maddened_factor,fire_share,notes\n";

        private static List<string> AffixErrors(string rows)
        {
            var errors = new List<string>();
            MapAffixTable.Parse(AffixHeader + rows, errors);
            return errors;
        }

        [Fact]
        public void AffixTableMistakesAreReported()
        {
            Assert.Empty(AffixErrors("a,approved,A,Danger,100,,,,,x\nb,later,B,Danger,,,,,,x\n"));
            Assert.NotEmpty(AffixErrors("A1,approved,A,Danger,100,,,,,x\n")); // id
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,,,,x\na,approved,A,Danger,100,,,,,x\n")); // twice
            Assert.NotEmpty(AffixErrors("a,proposed,A,Danger,100,,,,,x\n")); // status
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,,,,,,x\n")); // no weight
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,0,,,,,x\n")); // weight 0
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,-1,,,,x\n")); // waves
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,1.5,,,x\n")); // damage taken above 1
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,0,,,x\n")); // damage taken 0
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,,0.5,,x\n")); // maddened below 1
            Assert.NotEmpty(AffixErrors("a,approved,A,Danger,100,,,,11,x\n")); // fire above 10
            Assert.NotEmpty(AffixErrors("a,approved,A,,100,,,,,x\n")); // no danger
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
            var data = new Dictionary<string, string> { { MapData.TierKey, "6" }, { MapData.AffixesKey, "swarming;rabid" } };
            Assert.Equal(3, MapData.Read(data, t.Tiers, t.Affixes, null).Tier);
            data[MapData.CorruptedKey] = "1";
            Assert.Equal(6, MapData.Read(data, t.Tiers, t.Affixes, null).Tier);
        }

        [Fact]
        public void UnknownAffixIdsAreDroppedAndReported()
        {
            var t = Tables();
            var unknown = new List<string>();
            var data = new Dictionary<string, string> { { MapData.AffixesKey, "swarming;gone;swarming;;hardened" } };
            var s = MapData.Read(data, t.Tiers, t.Affixes, unknown);
            Assert.Equal(new[] { "swarming", "hardened" }, s.Affixes.ToArray());
            Assert.Equal(new[] { "gone" }, unknown.ToArray());
            Assert.Equal(3, s.Tier);
        }

        [Fact]
        public void WriteKeepsTheKeysConsistentAndRoundTrips()
        {
            var t = Tables();
            var data = new Dictionary<string, string>();
            var s = State(t, true, true, "swarming", "hardened", "rabid", "smouldering");
            MapData.Write(data, s, t.Tiers);
            Assert.Equal("swarming;hardened;rabid;smouldering", data[MapData.AffixesKey]);
            Assert.Equal("6", data[MapData.TierKey]);
            Assert.Equal("1", data["corruption.corrupted"]);
            Assert.Equal("1", data["corruption.mirrored"]);
            var back = MapData.Read(data, t.Tiers, t.Affixes, null);
            Assert.Equal(s.Affixes, back.Affixes);
            Assert.True(back.Corrupted);
            Assert.True(back.Mirrored);
            Assert.Equal(6, back.Tier);

            // a white map writes nothing: the keys are removed
            MapData.Write(data, State(t), t.Tiers);
            Assert.Empty(data);

            Assert.Equal("a;b", MapData.Join(new[] { "a", "b" }));
            Assert.Equal(new[] { "a", "b" }, MapData.Split("a;b;").ToArray());
            Assert.Empty(MapData.Split(null));
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
            Assert.Equal(new[] { "swarming" }, r.Added.ToArray());
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

            var divine = MapOrbRules.Refusal("divine", aug.State, t);
            Assert.Equal("Map affixes have no values to reroll.", divine);
            Assert.NotNull(MapOrbRules.Refusal("jeweller", white, t));
            Assert.NotNull(MapOrbRules.Refusal("nonsense", white, t));
        }

        [Fact]
        public void AnnulmentRemovesOneAndTheTierFallsDownToWhite()
        {
            var t = Tables();
            var full = State(t, false, false, "swarming", "hardened", "rabid", "smouldering");
            Assert.Equal(5, full.Tier);
            var a = MapOrbRules.Apply("annulment", full, t, Sequence(0.5));
            Assert.Equal(new[] { "rabid" }, a.Removed.ToArray());
            Assert.Equal(4, a.State.Tier);
            Assert.Equal(4, full.Affixes.Count);

            var one = State(t, false, false, "hardened");
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
            Assert.Equal("smouldering", last.State.Affixes[0]);
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
            var corrupted = State(t, true, false, "swarming", "hardened", "rabid", "smouldering");
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
            var copy = State(t, false, true, "swarming");
            foreach (var orb in OrbRules.Orbs)
            {
                Assert.NotNull(MapOrbRules.Refusal(orb, copy, t));
            }
            var corruptedCopy = State(t, true, true, "swarming", "hardened", "rabid", "smouldering");
            Assert.NotNull(MapOrbRules.Refusal("mirror", corruptedCopy, t));
            // the Mirror works on a white map too
            Assert.Null(MapOrbRules.Refusal("mirror", State(t), t));
        }

        // ---- the shard ----

        [Fact]
        public void TheShardCorruptsOnlyATierFiveMap()
        {
            var t = Tables();
            var full = State(t, false, false, "swarming", "hardened", "rabid", "smouldering");
            string headline;
            var done = MapCorruptRules.Apply(full, t, out headline);
            Assert.NotNull(done);
            Assert.True(done.Corrupted);
            Assert.Equal(6, done.Tier);
            Assert.Equal(4, done.Affixes.Count); // never destroyed, nothing lost
            Assert.Equal("Corrupted: now tier 6", headline);
            Assert.False(full.Corrupted);

            for (int n = 0; n < 4; n++)
            {
                var low = State(t, false, false, new[] { "swarming", "hardened", "rabid" }.Take(n).ToArray());
                Assert.Null(MapCorruptRules.Apply(low, t, out headline));
                Assert.Equal("Only a tier 5 map (4 affixes) can be corrupted.", headline);
            }
            Assert.Equal("This map is already corrupted.", MapCorruptRules.Refusal(done, t));
            Assert.NotNull(MapCorruptRules.Refusal(State(t, false, true, "swarming", "hardened", "rabid", "smouldering"), t));
        }

        // ---- what a rip's tier changes ----

        [Fact]
        public void WavesMultiplyTheTierAndSwarming()
        {
            var t = Tables();
            Assert.Equal(2.5, RipTierRules.WaveMultiplier(t.Tiers.Row(2), null));
            Assert.Equal(2.5, RipTierRules.WaveMultiplier(t.Tiers.Row(2), new[] { t.Affixes.Find("hardened") }));
            Assert.Equal(5.0, RipTierRules.WaveMultiplier(t.Tiers.Row(2), new[] { t.Affixes.Find("swarming") }));
            Assert.Equal(9.0, RipTierRules.WaveMultiplier(t.Tiers.Row(6), new[] { t.Affixes.Find("swarming") }));
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

            var none = new List<MapAffixTable.Affix>();
            Assert.Equal(1.0, RipTierRules.MaddenedFactor(none));
            Assert.Equal(1.0, RipTierRules.DamageTaken(none));
            Assert.Equal(0.0, RipTierRules.FireShare(none));
            var all = t.Affixes.Rollable.ToList();
            Assert.Equal(2.0, RipTierRules.MaddenedFactor(all));
            Assert.Equal(0.7, RipTierRules.DamageTaken(all));
            Assert.Equal(0.25, RipTierRules.FireShare(all));
        }
    }
}
