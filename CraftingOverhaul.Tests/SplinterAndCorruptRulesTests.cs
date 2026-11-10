using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Issue #29: Affix Splinters (exact transfer, deterministic twins), shattering (room check, one roll per line)
    ///     and corrupting with a shard (three outcomes, corrupted lines beyond the caps), with Krogon's rules of 2026-11-10.
    /// </summary>
    public class SplinterAndCorruptRulesTests
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

        private static Func<double> Sequence(params double[] values)
        {
            int i = 0;
            return () => values[Math.Min(i++, values.Length - 1)];
        }

        private static AffixTable.Affix Affix(string id, string side, string family, float min, float max, string cls = "weapon", string requires = "")
        {
            var affix = new AffixTable.Affix
            {
                Id = id, Side = side, Family = family, Name = id, ItemName = id, Text = "+{0} " + id, Min = min, Max = max,
                Weight = 50f, Requires = requires, Effect = family, Unit = "flat"
            };
            affix.Classes.Add(cls);
            return affix;
        }

        /// <summary>Health is a twin (5–20 each side, as max health in affixes.csv); the rest are single.</summary>
        private static Dictionary<string, AffixTable.Affix> Table()
        {
            return new[]
            {
                Affix("p_health", "prefix", "health", 5, 20), Affix("s_health", "suffix", "health", 5, 20),
                Affix("p_damage", "prefix", "damage", 10, 50), Affix("s_speed", "suffix", "speed", 1, 5),
                Affix("s_block", "suffix", "block", 1, 5, requires: "block"), Affix("s_helmet", "suffix", "warm", 1, 5, "helmet")
            }.ToDictionary(a => a.Id);
        }

        private static LootItem.Line Line(string ids, float value, bool corrupted = false)
        {
            return new LootItem.Line { Ids = ids.Split('+'), Value = value, Corrupted = corrupted };
        }

        private static CraftItem Item(string rarity, params LootItem.Line[] lines)
        {
            var item = new CraftItem { Rarity = rarity, Classes = new List<string> { "weapon" } };
            if (rarity != "normal" || lines.Length > 0)
            {
                item.Loot = new LootItem { Rarity = rarity, Name = rarity == "rare" ? "Grim Fang" : "" };
                item.Loot.Lines.AddRange(lines);
            }
            return item;
        }

        private static CorruptionOutcomes Outcomes()
        {
            var errors = new List<string>();
            var table = CorruptionOutcomes.Parse(DataFile("corruption_outcomes.csv"), errors);
            Assert.Empty(errors);
            return table;
        }

        private static ShatterRules Shattering()
        {
            var errors = new List<string>();
            var rules = ShatterRules.Parse(DataFile("shattering.csv"), errors);
            Assert.Empty(errors);
            return rules;
        }

        // ---- Splinters: exact transfer ----

        [Fact]
        public void ASplinterMovesItsExactValueAndMakesANormalItemMagic()
        {
            var result = SplinterRules.Apply(Item("normal"), Line("p_damage", 37), Table());
            Assert.True(result.Done);
            Assert.Equal("magic", result.Rarity);
            Assert.Equal("magic", result.Loot.Rarity);
            var line = Assert.Single(result.Loot.Lines);
            Assert.Equal(new[] { "p_damage" }, line.Ids);
            Assert.Equal(37f, line.Value);
            Assert.False(result.CompletedTwin);
        }

        [Fact]
        public void ApplyingNeverRollsAndNeverChangesTheInput()
        {
            var item = Item("magic", Line("p_damage", 12));
            var result = SplinterRules.Apply(item, Line("s_speed", 4), Table());
            Assert.True(result.Done);
            Assert.Equal(new[] { 12f, 4f }, result.Loot.Lines.Select(l => l.Value));
            Assert.Single(item.Loot.Lines);            // the input is untouched until the caller writes the result
            Assert.Equal("magic", result.Rarity);
        }

        [Fact]
        public void ASplinterThatCompletesATwinIsTheSumPlusTheResonanceBonus()
        {
            // Krogon's example (2026-11-10): Hale +14 and a splinter of the Bear +18 give one resonant line of +40.
            var result = SplinterRules.Apply(Item("magic", Line("p_health", 14)), Line("s_health", 18), Table());
            Assert.True(result.Done);
            Assert.True(result.CompletedTwin);
            var line = Assert.Single(result.Loot.Lines);
            Assert.Equal(new[] { "p_health", "s_health" }, line.Ids);   // it fills the prefix and the suffix place
            Assert.True(line.Resonant);
            Assert.Equal(40f, line.Value);

            // The same either way round, and the same every time: nothing is rolled.
            var reversed = SplinterRules.Apply(Item("magic", Line("s_health", 18)), Line("p_health", 14), Table());
            Assert.Equal(new[] { "p_health", "s_health" }, reversed.Loot.Lines[0].Ids);
            Assert.Equal(40f, reversed.Loot.Lines[0].Value);
        }

        [Fact]
        public void TwoTopHalvesReachTheResonantRangesTopAndNoHigher()
        {
            var table = Table();
            var result = SplinterRules.Apply(Item("magic", Line("p_health", 20)), Line("s_health", 20), table);
            LootItem.Range(result.Line, table, out _, out float max);
            Assert.Equal(50f, result.Line.Value);
            Assert.Equal(max, result.Line.Value);
            Assert.Equal(12.5f, SplinterRules.ResonantSum(5f, 5f, 1));
            Assert.Equal(13f, SplinterRules.ResonantSum(5f, 5f, 0));   // rounded as the affix rolls
        }

        [Fact]
        public void AnAlreadyResonantSplinterKeepsItsExactValueAndOnlyFitsAnEmptyItem()
        {
            var table = Table();
            var splinter = Line("p_health+s_health", 43);
            var result = SplinterRules.Apply(Item("normal"), splinter, table);
            Assert.True(result.Done);
            Assert.False(result.CompletedTwin);
            var line = Assert.Single(result.Loot.Lines);
            Assert.True(line.Resonant);
            Assert.Equal(43f, line.Value);                           // no second bonus

            Assert.Equal(SplinterRules.NoRoom, SplinterRules.Refusal(Item("magic", Line("p_damage", 12)), splinter, table));
            Assert.Equal("", SplinterRules.Refusal(Item("magic"), splinter, table));   // a Magic item emptied by Annulment
        }

        [Fact]
        public void SplinterRefusalsFollowTheRules()
        {
            var table = Table();
            var damage = Line("p_damage", 20);
            Assert.Equal(SplinterRules.OnlyNormalOrMagic, SplinterRules.Refusal(Item("rare", Line("s_speed", 2)), damage, table));
            Assert.Equal(SplinterRules.OnlyNormalOrMagic, SplinterRules.Refusal(Item("unique"), damage, table));
            var corrupted = Item("magic");
            corrupted.Corrupted = true;
            Assert.Equal(SplinterRules.OnlyNormalOrMagic, SplinterRules.Refusal(corrupted, damage, table));
            var mirrored = Item("magic");
            mirrored.Mirrored = true;
            Assert.Equal(OrbRules.MirroredLocked, SplinterRules.Refusal(mirrored, damage, table));

            // Two affixes already: full.
            Assert.Equal(SplinterRules.NoRoom, SplinterRules.Refusal(Item("magic", Line("p_health", 9), Line("s_speed", 2)), damage, table));
            // The prefix place is taken by another family; by the same family.
            Assert.Equal(SplinterRules.NoRoom, SplinterRules.Refusal(Item("magic", Line("p_health", 9)), damage, table));
            Assert.Equal(SplinterRules.SameKind, SplinterRules.Refusal(Item("magic", Line("p_damage", 9)), damage, table));
            // The item's class or mechanics can't roll it.
            Assert.Equal(SplinterRules.CantRoll, SplinterRules.Refusal(Item("normal"), Line("s_helmet", 2), table));
            Assert.Equal(SplinterRules.CantRoll, SplinterRules.Refusal(Item("normal"), Line("s_block", 2), table));
            var noBase = Item("normal");
            noBase.Classes = null;
            Assert.Equal(SplinterRules.CantRoll, SplinterRules.Refusal(noBase, damage, table));
            // An affix the table no longer has.
            Assert.Equal(SplinterRules.UnreadableSplinter, SplinterRules.Refusal(Item("normal"), Line("p_gone", 2), table));
            Assert.Equal(SplinterRules.UnreadableSplinter, SplinterRules.Refusal(Item("normal"), null, table));

            // A refusal changes nothing.
            var refused = SplinterRules.Apply(Item("rare", Line("s_speed", 2)), damage, table);
            Assert.False(refused.Done);
            Assert.Null(refused.Loot);
        }

        [Fact]
        public void SplinterDataSurvivesSavingExactly()
        {
            // Saving, trading, chests and recovery all carry the item's custom data as text: what is written must read back the same.
            foreach (var line in new[] { Line("p_damage", 37), Line("s_speed", 1.5f), Line("p_health+s_health", 43), Line("p_damage", 0.25f) })
            {
                var data = new Dictionary<string, string>();
                SplinterRules.Write(line, data);
                var copy = new Dictionary<string, string>(data);   // what a save or a transfer keeps
                var read = SplinterRules.Read(copy);
                Assert.Equal(line.Ids, read.Ids);
                Assert.Equal(line.Value, read.Value);
                Assert.False(read.Corrupted);
            }
            Assert.Null(SplinterRules.Read(new Dictionary<string, string>()));
            Assert.Null(SplinterRules.Read(new Dictionary<string, string> { { SplinterRules.LineKey, "p_damage=NaN" } }));
            Assert.Null(SplinterRules.Read(null));
        }

        // ---- Shattering ----

        [Fact]
        public void ShatterRefusals()
        {
            Assert.Equal(ShatterRules.OnlyMagicOrRare, ShatterRules.Refusal(Item("normal")));
            Assert.Equal(ShatterRules.NoUniquesOrCorrupted, ShatterRules.Refusal(Item("unique", Line("p_damage", 9))));
            var corrupted = Item("rare", Line("p_damage", 9));
            corrupted.Corrupted = true;
            Assert.Equal(ShatterRules.NoUniquesOrCorrupted, ShatterRules.Refusal(corrupted));
            var mirrored = Item("rare", Line("p_damage", 9));
            mirrored.Mirrored = true;
            Assert.Equal(OrbRules.MirroredLocked, ShatterRules.Refusal(mirrored));   // locked like a corrupted item
            Assert.Equal(OrbRules.NoAffixes, ShatterRules.Refusal(Item("magic")));
            Assert.Equal("", ShatterRules.Refusal(Item("magic", Line("p_damage", 9))));
        }

        [Fact]
        public void EveryPossibleSplinterNeedsAPlaceBeforeAnythingIsRolled()
        {
            var item = Item("rare", Line("p_health+s_health", 43), Line("p_damage", 20), Line("s_speed", 2));
            Assert.Equal(3, ShatterRules.MaxSplinters(item));           // a resonant line is one possible splinter

            Assert.True(ShatterRules.HasRoom(3, 3, 0, false));          // all in Splinters
            Assert.True(ShatterRules.HasRoom(3, 1, 2, false));          // overflow into the Bag
            Assert.True(ShatterRules.HasRoom(3, 0, 2, true));           // the shattered item's own Bag cell counts
            Assert.False(ShatterRules.HasRoom(3, 0, 2, false));         // ...but not when it isn't in the Bag grid
            Assert.False(ShatterRules.HasRoom(3, 1, 1, false));
            Assert.True(ShatterRules.HasRoom(1, 0, 0, true));
            Assert.False(ShatterRules.HasRoom(1, -5, 0, false));

            Assert.Equal("Make room for up to 3 splinters.", ShatterRules.MakeRoom(3));
            Assert.Equal("Make room for up to 1 splinter.", ShatterRules.MakeRoom(1));
        }

        [Fact]
        public void ShatteringKeepsExactValuesOneRollPerDisplayedLine()
        {
            var rules = Shattering();
            var item = Item("rare", Line("p_health+s_health", 43), Line("p_damage", 20), Line("s_speed", 2));
            int rolls = 0;
            var values = new[] { 0.1, 0.9, 0.49 };
            var splinters = rules.Shatter(item, () => values[rolls++]);
            Assert.Equal(3, rolls);
            Assert.Equal(2, splinters.Count);
            Assert.Equal(new[] { "p_health", "s_health" }, splinters[0].Ids);
            Assert.Equal(43f, splinters[0].Value);                     // the combined value, kept
            Assert.Equal(new[] { "s_speed" }, splinters[1].Ids);
            Assert.Equal(2f, splinters[1].Value);
            Assert.Equal(3, item.Loot.Lines.Count);                    // the input is untouched

            Assert.Empty(rules.Shatter(item, () => 0.99));
            Assert.Equal(3, rules.Shatter(item, () => 0.0).Count);
        }

        [Fact]
        public void ShatterPreviewShowsTheChanceFromTheDataAndTheDestruction()
        {
            var preview = Shattering().Preview(Item("magic", Line("p_damage", 20), Line("s_speed", 2)), Table());
            Assert.Equal(3, preview.Count);
            Assert.StartsWith("50% · ", preview[0]);
            Assert.StartsWith("50% · ", preview[1]);
            Assert.Contains("destroyed", preview[2]);
        }

        [Fact]
        public void ShatterThenApplyRoundTripsALine()
        {
            var table = Table();
            var splinter = Shattering().Shatter(Item("magic", Line("p_damage", 33)), () => 0.0).Single();
            var data = new Dictionary<string, string>();
            SplinterRules.Write(splinter, data);
            var result = SplinterRules.Apply(Item("normal"), SplinterRules.Read(data), table);
            Assert.Equal(33f, result.Loot.Lines.Single().Value);
        }

        // ---- Corrupting with a shard ----

        [Fact]
        public void CorruptRefusalsUseNoRoll()
        {
            var table = Table();
            var corrupted = Item("magic", Line("p_damage", 9));
            corrupted.Corrupted = true;
            Assert.Equal(CorruptRules.AlreadyCorrupted, CorruptRules.Refusal(corrupted, table));
            Assert.Equal(CorruptRules.NoUniques, CorruptRules.Refusal(Item("unique"), table));
            var mirrored = Item("magic", Line("p_damage", 9));
            mirrored.Mirrored = true;
            Assert.Equal(OrbRules.MirroredLocked, CorruptRules.Refusal(mirrored, table));
            var noBase = Item("magic", Line("p_damage", 9));
            noBase.Classes = null;
            Assert.Equal(OrbRules.CantTakeAffixes, CorruptRules.Refusal(noBase, table));
            Assert.Equal("", CorruptRules.Refusal(Item("normal"), table));

            int rolls = 0;
            var result = CorruptRules.Apply(corrupted, table, Outcomes(), () => { rolls++; return 0.0; });
            Assert.False(result.Done);
            Assert.Equal(0, rolls);
        }

        [Fact]
        public void TheThreeOutcomesFollowTheTable()
        {
            var table = Table();
            var item = Item("magic", Line("p_damage", 9));

            var bricked = CorruptRules.Apply(item, table, Outcomes(), Sequence(0.24));
            Assert.True(bricked.Done);
            Assert.Equal("bricked", bricked.Outcome);
            Assert.Null(bricked.Loot);

            var unchanged = CorruptRules.Apply(item, table, Outcomes(), Sequence(0.25));
            Assert.Equal("unchanged", unchanged.Outcome);
            Assert.Null(unchanged.Loot);                                 // nothing added; the caller still locks it
            Assert.Null(unchanged.Line);

            var affix = CorruptRules.Apply(item, table, Outcomes(), Sequence(0.5, 0.0, 0.0));
            Assert.Equal("affix", affix.Outcome);
            Assert.Equal(2, affix.Loot.Lines.Count);
            Assert.True(affix.Line.Corrupted);
            Assert.False(affix.Loot.Lines[0].Corrupted);                 // the ordinary affix isn't marked
            Assert.Equal("magic", affix.Loot.Rarity);                    // rarity stays
            Assert.Single(item.Loot.Lines);                              // the input is untouched
        }

        [Fact]
        public void TheCorruptedAffixGoesOnTopOfTheLimitAndMayRepeatAFamily()
        {
            var table = new[] { Affix("p_damage", "prefix", "damage", 10, 50) }.ToDictionary(a => a.Id);
            // A full Magic item whose prefix is already damage: the only candidate repeats the family on the same side.
            var item = Item("magic", Line("p_damage", 9), Line("s_gone", 1));
            var result = CorruptRules.Apply(item, table, Outcomes(), Sequence(0.9, 0.0, 0.999));
            Assert.Equal("affix", result.Outcome);
            Assert.Equal(3, result.Loot.Lines.Count);
            Assert.Equal(new[] { "p_damage" }, result.Line.Ids);
            Assert.Equal(50f, result.Line.Value);
            Assert.False(result.CompletedTwin);

            // A Normal item ends with one affix and stays Normal.
            var normal = CorruptRules.Apply(Item("normal"), table, Outcomes(), Sequence(0.9, 0.0, 0.0));
            Assert.Equal("normal", normal.Loot.Rarity);
            Assert.True(Assert.Single(normal.Loot.Lines).Corrupted);
        }

        [Fact]
        public void ACorruptedAffixThatCompletesATwinAddsBothValuesPlusTheBonus()
        {
            var table = new[] { Affix("p_health", "prefix", "health", 5, 20), Affix("s_health", "suffix", "health", 5, 20) }
                .ToDictionary(a => a.Id);
            // The pick lands on the suffix half (second of two equal weights), which rolls 20 (its top):
            // (14 + 20) × 1.25 = 42.5 → 43 (health rolls in whole numbers). Krogon, 2026-11-10: like a splinter.
            var result = CorruptRules.Apply(Item("magic", Line("p_health", 14)), table, Outcomes(), Sequence(0.9, 0.75, 0.999));
            Assert.True(result.CompletedTwin);
            var line = Assert.Single(result.Loot.Lines);
            Assert.True(line.Resonant);
            Assert.True(line.Corrupted);
            Assert.Equal(new[] { "p_health", "s_health" }, line.Ids);
            Assert.Equal(43f, line.Value);

            // The bottom roll: (14 + 5) × 1.25 = 23.75 → 24. It never goes below what the item had.
            var low = CorruptRules.Apply(Item("magic", Line("p_health", 14)), table, Outcomes(), Sequence(0.9, 0.75, 0.0));
            Assert.Equal(24f, low.Loot.Lines.Single().Value);

            // The same half again: no twin, its own corrupted line.
            var same = CorruptRules.Apply(Item("magic", Line("p_health", 14)), table, Outcomes(), Sequence(0.9, 0.25, 0.0));
            Assert.False(same.CompletedTwin);
            Assert.Equal(2, same.Loot.Lines.Count);
            Assert.Equal(14f, same.Loot.Lines[0].Value);
        }

        [Fact]
        public void ThePreviewShowsTheChancesFromTheDataAndTheLock()
        {
            var preview = CorruptRules.Preview(Outcomes());
            Assert.Equal(4, preview.Count);
            Assert.StartsWith("25% destroyed", preview[0]);
            Assert.StartsWith("25% unchanged and locked", preview[1]);
            Assert.StartsWith("50% gains one corrupted affix", preview[2]);
            Assert.Contains("locked", preview[3]);
            Assert.Contains("Mirror", preview[3]);

            var errors = new List<string>();
            var other = CorruptionOutcomes.Parse("outcome,status,chance\nbricked,approved,10\nunchanged,approved,30\naffix,approved,60\n", errors);
            Assert.StartsWith("10% destroyed", CorruptRules.Preview(other)[0]);   // read from the table, not written in
        }

        // ---- Corrupted lines: saving and the caps ----

        [Fact]
        public void CorruptedLinesSurviveSavingAndOldItemsStillRead()
        {
            var loot = new LootItem { Rarity = "magic" };
            loot.Lines.Add(Line("p_damage", 12));
            loot.Lines.Add(Line("p_health+s_health", 44, true));
            loot.Lines.Add(Line("s_speed", 2.5f, true));
            var data = new Dictionary<string, string>();
            loot.Write(data);
            Assert.Equal("p_damage=12;~p_health+s_health=44;~s_speed=2.5", data[LootItem.AffixesKey]);
            var read = LootItem.Read(data);
            Assert.Equal(new[] { false, true, true }, read.Lines.Select(l => l.Corrupted));
            Assert.Equal(new[] { 12f, 44f, 2.5f }, read.Lines.Select(l => l.Value));

            // Items saved before this round have no marks and read as before.
            var old = LootItem.Read(new Dictionary<string, string> { { LootItem.RarityKey, "rare" }, { LootItem.AffixesKey, "p_damage=12;s_speed=3" } });
            Assert.All(old.Lines, l => Assert.False(l.Corrupted));
            Assert.Null(LootItem.Read(new Dictionary<string, string> { { LootItem.RarityKey, "rare" }, { LootItem.AffixesKey, "~=3" } }));
        }

        [Fact]
        public void OrdinaryAffixesStayCappedAndTheCorruptedOneAddsOnTop()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors).ToDictionary(a => a.Id);
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            Assert.Empty(errors);
            string movement = affixes.Values.First(a => a.Effect == "movement_speed").Id;
            float cap = caps.For("movement_speed").Max;

            LootItem Worn(float value, bool corrupted)
            {
                var item = new LootItem();
                item.Lines.Add(new LootItem.Line { Ids = new[] { movement }, Value = value, Corrupted = corrupted });
                return item;
            }

            // Over everything worn: the ordinary lines together are capped.
            var ordinary = new[] { Worn(cap, false), Worn(10, false) };
            Assert.Equal(cap, AffixTotals.MovementPercent(ordinary, affixes, caps));

            // A corrupted line adds beyond the cap...
            var withCorrupted = new[] { Worn(cap, false), Worn(10, false), Worn(7, true) };
            Assert.Equal(cap + 7, AffixTotals.MovementPercent(withCorrupted, affixes, caps));

            // ...without uncapping the ordinary ones, and without being needed to fill the cap first.
            var below = new[] { Worn(5, false), Worn(7, true) };
            Assert.Equal(12f, AffixTotals.MovementPercent(below, affixes, caps));
            Assert.Equal(cap + 7 + 3, AffixTotals.MovementPercent(withCorrupted.Concat(new[] { Worn(3, true) }), affixes, caps));

            Assert.Equal(cap, AffixTotals.Capped(cap + 50, 0, "movement_speed", caps));
            Assert.Equal(60f, AffixTotals.Capped(40, 20, "not_capped", caps));
        }

        [Fact]
        public void CorruptedResistanceGoesPastTheCapButNeverPastAHundred()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors).ToDictionary(a => a.Id);
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            string fire = affixes.Values.First(a => a.Effect == "fire_resistance").Id;
            float cap = caps.For("fire_resistance").Max;

            LootItem Worn(float value, bool corrupted)
            {
                var item = new LootItem();
                item.Lines.Add(new LootItem.Line { Ids = new[] { fire }, Value = value, Corrupted = corrupted });
                return item;
            }

            Assert.Equal(cap, AffixTotals.ResistancePercent("fire", new[] { Worn(cap + 20, false) }, affixes, caps));
            Assert.Equal(cap + 10, AffixTotals.ResistancePercent("fire", new[] { Worn(cap + 20, false), Worn(10, true) }, affixes, caps));
            Assert.Equal(100f, AffixTotals.ResistancePercent("fire", new[] { Worn(cap, false), Worn(60, true) }, affixes, caps));
            Assert.Equal(0f, AffixTotals.ResistancePercent("frost", new[] { Worn(cap, false), Worn(60, true) }, affixes, caps));
        }

        [Fact]
        public void PlayersSeeEachCapAndHowCloseTheyAre()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors).ToDictionary(a => a.Id);
            var caps = AffixCaps.Parse(DataFile("affix_caps.csv"), errors);
            var movement = affixes.Values.First(a => a.Effect == "movement_speed");
            var fire = affixes.Values.First(a => a.Effect == "fire_resistance");
            var all = affixes.Values.First(a => a.Family == "all_resistances");
            var frost = affixes.Values.First(a => a.Effect == "frost_resistance");

            LootItem Worn(AffixTable.Affix affix, float value, bool corrupted = false)
            {
                var item = new LootItem();
                item.Lines.Add(new LootItem.Line { Ids = new[] { affix.Id }, Value = value, Corrupted = corrupted });
                return item;
            }

            Assert.Null(AffixTotals.Progress("max_health", new LootItem[0], affixes, caps));   // not a capped stat

            var worn = new[] { Worn(movement, 10), Worn(movement, 18), Worn(movement, 7, true) };
            var progress = AffixTotals.Progress(movement.Family, worn, affixes, caps);
            Assert.Equal(caps.For("movement_speed").Max, progress.Max);
            Assert.Equal(28f, progress.Ordinary);
            Assert.Equal(7f, progress.Corrupted);
            Assert.Equal("Cap 35%: 28% worn, +7% corrupted beyond the cap", progress.Text());
            Assert.Equal("Cap 35%: 0% worn", AffixTotals.Progress(movement.Family, new LootItem[0], affixes, caps).Text());
            Assert.Equal("Cap 35%: reached (40% worn)",
                AffixTotals.Progress(movement.Family, new[] { Worn(movement, 40) }, affixes, caps).Text());
            Assert.Equal("Cap 35%: reached", AffixTotals.Progress(movement.Family, new[] { Worn(movement, 35) }, affixes, caps).Text());

            // Resistances are capped per element: fire counts fire plus all resistances, not frost.
            var resist = new[] { Worn(fire, 30), Worn(all, 8), Worn(frost, 50) };
            Assert.Equal(38f, AffixTotals.Progress(fire.Family, resist, affixes, caps).Ordinary);
            // "All resistances" shows the element that is furthest along.
            Assert.Equal(58f, AffixTotals.Progress(all.Family, resist, affixes, caps).Ordinary);
        }

        [Fact]
        public void CorruptingKeepsAMagicItemsName()
        {
            var table = Table();
            var loot = new LootItem { Rarity = "magic" };
            loot.Lines.Add(Line("p_health", 9));
            string before = loot.DisplayName("Sword", table);
            loot.Lines.Add(Line("p_damage", 30, true));
            Assert.Equal(before, loot.DisplayName("Sword", table));
            loot.Lines.Add(Line("s_speed", 3, true));
            Assert.Equal(before + " s_speed", loot.DisplayName("Sword", table));   // an empty side takes the corrupted affix's name
        }
    }
}
