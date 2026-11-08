using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The Riftforge's currency orbs (OrbRules), with Krogon's rules of 2026-11-08 (issue #28).</summary>
    public class OrbRulesTests
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

        private static AffixTable.Affix Affix(string id, string side, string family, float min, float max, float weight = 50f)
        {
            var affix = new AffixTable.Affix
            {
                Id = id, Side = side, Family = family, Name = id, ItemName = id, Text = "+{0} " + id, Min = min, Max = max,
                Weight = weight, Requires = "", Effect = id, Unit = "flat"
            };
            affix.Classes.Add("weapon");
            return affix;
        }

        /// <summary>Three prefixes and three suffixes; fire is a twin (a prefix and a suffix with the same family).</summary>
        private static Dictionary<string, AffixTable.Affix> Table()
        {
            return new[]
            {
                Affix("p_damage", "prefix", "damage", 10, 50), Affix("p_fire", "prefix", "fire", 5, 25), Affix("p_armour", "prefix", "armour", 1, 10),
                Affix("s_fire", "suffix", "fire", 5, 25), Affix("s_speed", "suffix", "speed", 1, 5), Affix("s_regen", "suffix", "regen", 1, 10)
            }.ToDictionary(a => a.Id);
        }

        private static OrbContext Context(bool uniques = false)
        {
            var errors = new List<string>();
            var rules = LootRules.Parse(DataFile("loot_rules.csv"), DataFile("rare_names.csv"), errors);
            var chance = OrbChanceTable.Parse(DataFile("orb_chance.csv"), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return new OrbContext { Affixes = Table(), Rules = rules, Chance = chance, UniqueAvailable = uniques };
        }

        private static CraftItem Item(string rarity, params string[] lines)
        {
            var item = new CraftItem { Rarity = rarity, Classes = new List<string> { "weapon" } };
            if (rarity != "normal")
            {
                item.Loot = new LootItem { Rarity = rarity, Name = rarity == "rare" ? "Grim Fang" : "" };
                foreach (var line in lines)
                {
                    item.Loot.Lines.Add(new LootItem.Line { Ids = line.Split('+'), Value = 5 });
                }
            }
            return item;
        }

        private static string Refusal(string orb, CraftItem item) => OrbRules.Refusal(orb, item, Context());

        // ---- The table ----

        [Fact]
        public void RealChanceFileHasTheAgreedOdds()
        {
            var errors = new List<string>();
            var table = OrbChanceTable.Parse(DataFile("orb_chance.csv"), errors);
            Assert.True(errors.Count == 0, "orb_chance.csv has problems:\n" + string.Join("\n", errors));
            Assert.Equal(50f, table.Percent["magic"]);
            Assert.Equal(30f, table.Percent["rare"]);
            Assert.Equal(20f, table.Percent["unique"]);
        }

        [Fact]
        public void ChanceTableNamesItsMistakes()
        {
            var errors = new List<string>();
            OrbChanceTable.Parse("outcome,status,chance\nmagic,approved,50\nrare,approved,30\nlegendary,approved,20\n", errors);
            Assert.Contains(errors, e => e.Contains("legendary") && e.Contains("outcome must be"));
            Assert.Contains(errors, e => e.Contains("no row for unique"));

            errors.Clear();
            OrbChanceTable.Parse("outcome,status,chance\nmagic,approved,50\nrare,approved,30\nunique,approved,30\n", errors);
            Assert.Contains(errors, e => e.Contains("add up to 110"));
        }

        [Fact]
        public void ChanceRollsInTableOrder()
        {
            var table = Context().Chance;
            Assert.Equal("magic", table.Roll(() => 0.49));
            Assert.Equal("rare", table.Roll(() => 0.50));
            Assert.Equal("rare", table.Roll(() => 0.79));
            Assert.Equal("unique", table.Roll(() => 0.80));
        }

        // ---- Names and items ----

        [Fact]
        public void PrefabNamesRoundTrip()
        {
            foreach (var orb in OrbRules.Orbs)
            {
                Assert.Equal(orb, OrbRules.OrbOf(OrbRules.Prefab(orb)));
                Assert.StartsWith(OrbRules.ItemPrefix, OrbRules.Prefab(orb));
                Assert.NotEmpty(OrbRules.Description(orb));
            }
            Assert.Equal("CorruptionOrb_Regal", OrbRules.Prefab("regal"));
            Assert.Null(OrbRules.OrbOf("CorruptionShard"));
            Assert.Null(OrbRules.OrbOf("CorruptionOrb_Nonsense"));
            Assert.Equal("Mirror of Kalandra", OrbRules.Name("mirror"));
            Assert.Equal("Orb of Annulment", OrbRules.Name("annulment"));
        }

        [Fact]
        public void OrbsGoToTheCurrencyTab()
        {
            foreach (var orb in OrbRules.Orbs)
            {
                Assert.Equal(InventoryTab.Currency, TabRules.TabFor(OrbRules.Prefab(orb)));
            }
        }

        // ---- Transmutation, Alchemy, Chance (Normal items) ----

        [Fact]
        public void TransmutationMakesANormalItemMagicWithOneAffix()
        {
            var result = OrbRules.Apply("transmutation", Item("normal"), Context(), Sequence(0.0));
            Assert.True(result.Done);
            Assert.Equal("magic", result.Rarity);
            Assert.Single(result.Loot.Lines);
            Assert.Equal("", result.Loot.Name);
        }

        [Fact]
        public void NormalItemOrbsRefuseOtherItems()
        {
            foreach (var orb in new[] { "transmutation", "alchemy", "chance" })
            {
                Assert.Equal(OrbRules.OnlyNormal, Refusal(orb, Item("magic", "p_damage")));
                Assert.Equal(OrbRules.OnlyNormal, Refusal(orb, Item("rare")));
                Assert.Equal(OrbRules.OnlyNormal, Refusal(orb, new CraftItem { Rarity = "unique" }));
                var corrupted = Item("normal");
                corrupted.Corrupted = true;
                Assert.Equal(OrbRules.CorruptedLocked, Refusal(orb, corrupted));
                // Not a loot base (no affix classes): nothing can roll on it.
                Assert.Equal(OrbRules.CantTakeAffixes, Refusal(orb, new CraftItem()));
            }
        }

        [Fact]
        public void AlchemyMakesANormalItemRareWithANameAndThreeOrFourAffixes()
        {
            var result = OrbRules.Apply("alchemy", Item("normal"), Context(), Sequence(0.0));
            Assert.True(result.Done);
            Assert.Equal("rare", result.Rarity);
            Assert.NotEmpty(result.Loot.Name);
            Assert.InRange(result.Loot.Lines.Sum(l => l.Ids.Length), 3, 4);
        }

        [Fact]
        public void ChanceIsAlwaysRareWhileNoUniqueFits()
        {
            // Krogon, 2026-11-08: without an eligible unique, 100% Rare, even on a roll that would be Magic.
            foreach (double roll in new[] { 0.0, 0.3, 0.6, 0.9 })
            {
                var result = OrbRules.Apply("chance", Item("normal"), Context(), Sequence(roll));
                Assert.True(result.Done);
                Assert.Equal("rare", result.Rarity);
                Assert.NotEmpty(result.Loot.Name);
            }
            Assert.Equal(new[] { "Becomes Rare (no unique can come from this item yet)" },
                OrbRules.Preview("chance", Item("normal"), Context()));
        }

        [Fact]
        public void ChanceUsesTheTableOnceUniquesExist()
        {
            var result = OrbRules.Apply("chance", Item("normal"), Context(true), Sequence(0.2, 0.0));
            Assert.Equal("magic", result.Rarity);
            Assert.Equal(new[] { "50% Magic · 30% Rare · 20% Unique" }, OrbRules.Preview("chance", Item("normal"), Context(true)));
        }

        // ---- Augmentation, Regal, Exalted (adding an affix) ----

        [Fact]
        public void AugmentationWorksOnMagicWithZeroOrOneAffix()
        {
            // Krogon, 2026-11-08: also on 0 affixes, so a Magic item emptied by Annulment can be brought back.
            var empty = OrbRules.Apply("augmentation", Item("magic"), Context(), Sequence(0.0));
            Assert.True(empty.Done);
            Assert.Equal("magic", empty.Rarity);
            Assert.Single(empty.Loot.Lines);

            var one = OrbRules.Apply("augmentation", Item("magic", "p_damage"), Context(), Sequence(0.0));
            Assert.True(one.Done);
            Assert.Equal("magic", one.Rarity);
            Assert.Equal(2, one.Loot.Lines.Count);
            // The side already taken stays full: the new one is a suffix.
            Assert.Equal("prefix", Table()[one.Loot.Lines[0].Ids[0]].Side);
            Assert.Equal("suffix", Table()[one.Loot.Lines[1].Ids[0]].Side);
        }

        [Fact]
        public void AugmentationRefusesFullAndNonMagicItems()
        {
            Assert.Equal(OrbRules.NoRoom, Refusal("augmentation", Item("magic", "p_damage", "s_speed")));
            Assert.Equal(OrbRules.NoRoom, Refusal("augmentation", Item("magic", "p_fire+s_fire")));
            Assert.Equal(OrbRules.OnlyMagic, Refusal("augmentation", Item("normal")));
            Assert.Equal(OrbRules.OnlyMagic, Refusal("augmentation", Item("rare", "p_damage")));
        }

        [Fact]
        public void AugmentationCompletingATwinMakesItResonant()
        {
            var context = Context();
            // The prefix side is full, so one of the three suffixes is added; a roll of 0 picks the first, s_fire, fire's
            // other half, and the two merge into one resonant line (its value rolled by the second roll).
            var item = Item("magic", "p_fire");
            var candidates = AffixRoller.AddCandidates(item.Loot, "magic", context.Affixes, item.Classes, item.Mechanics);
            Assert.Equal(new[] { "s_fire", "s_speed", "s_regen" }, candidates.Select(a => a.Id));
            var result = OrbRules.Apply("augmentation", item, context, Sequence(0.0, 0.99));
            var line = Assert.Single(result.Loot.Lines);
            Assert.True(line.Resonant);
            Assert.Equal(new[] { "p_fire", "s_fire" }, line.Ids);
            // The resonant range: 10 to (25 + 25) × 1.25 = 62.5, rounded to whole numbers (63).
            Assert.Equal(63f, line.Value);
            Assert.Equal(2, result.Loot.Lines.Sum(l => l.Ids.Length));
        }

        [Fact]
        public void RegalNeedsExactlyTwoAffixesAndMakesItRare()
        {
            // Krogon, 2026-11-08: only with exactly 2 (the brief had allowed any Magic item).
            Assert.Equal(OrbRules.OnlyMagicWithTwo, Refusal("regal", Item("magic")));
            Assert.Equal(OrbRules.OnlyMagicWithTwo, Refusal("regal", Item("magic", "p_damage")));
            Assert.Equal(OrbRules.OnlyMagic, Refusal("regal", Item("rare", "p_damage", "s_speed")));
            Assert.Equal("", Refusal("regal", Item("magic", "p_fire+s_fire")));

            var result = OrbRules.Apply("regal", Item("magic", "p_damage", "s_speed"), Context(), Sequence(0.0));
            Assert.True(result.Done);
            Assert.Equal("rare", result.Rarity);
            Assert.Equal("rare", result.Loot.Rarity);
            Assert.NotEmpty(result.Loot.Name);
            Assert.Equal(3, result.Loot.Lines.Sum(l => l.Ids.Length));
            Assert.Single(result.Gained);
        }

        [Fact]
        public void ExaltedAddsToARareWithRoom()
        {
            var result = OrbRules.Apply("exalted", Item("rare", "p_damage", "s_speed"), Context(), Sequence(0.0));
            Assert.True(result.Done);
            Assert.Equal(3, result.Loot.Lines.Count);
            Assert.Equal("Grim Fang", result.Loot.Name);
            // Even a Rare emptied by Annulment can take it.
            Assert.Equal("", Refusal("exalted", Item("rare")));
            Assert.Equal(OrbRules.NoRoom, Refusal("exalted", Item("rare", "p_damage", "p_armour", "s_speed", "s_regen")));
            Assert.Equal(OrbRules.OnlyRare, Refusal("exalted", Item("magic", "p_damage")));
        }

        // ---- Divine, Annulment ----

        [Fact]
        public void DivineRerollsEveryLineInItsRange()
        {
            var item = Item("rare", "p_damage", "p_fire+s_fire", "s_speed");
            var result = OrbRules.Apply("divine", item, Context(), Sequence(0.99));
            Assert.True(result.Done);
            Assert.Equal(new[] { 50f, 63f, 5f }, result.Loot.Lines.Select(l => l.Value));
            Assert.Equal(3, result.Gained.Count);
            Assert.Equal(OrbRules.NoAffixes, Refusal("divine", Item("normal")));
            Assert.Equal(OrbRules.NoAffixes, Refusal("divine", Item("magic")));
        }

        [Fact]
        public void AnnulmentNeverChangesTheRarity()
        {
            // Krogon, 2026-11-08: a Magic item stays Magic with 0 affixes; a Rare stays Rare, with its name.
            var magic = OrbRules.Apply("annulment", Item("magic", "p_damage"), Context(), Sequence(0.0));
            Assert.True(magic.Done);
            Assert.Equal("magic", magic.Rarity);
            Assert.Empty(magic.Loot.Lines);
            Assert.Equal("p_damage", Assert.Single(magic.Lost).Ids[0]);

            var rare = OrbRules.Apply("annulment", Item("rare", "p_damage", "s_speed", "s_regen"), Context(), Sequence(0.5));
            Assert.Equal("rare", rare.Rarity);
            Assert.Equal("Grim Fang", rare.Loot.Name);
            Assert.Equal(new[] { "p_damage", "s_regen" }, rare.Loot.Lines.Select(l => l.Ids[0]));

            Assert.Equal(OrbRules.NoAffixes, Refusal("annulment", Item("magic")));
            Assert.Equal(OrbRules.NoAffixes, Refusal("annulment", Item("normal")));
            Assert.Equal(OrbRules.UniqueOnly, Refusal("annulment", new CraftItem { Rarity = "unique" }));
        }

        [Fact]
        public void AnnulmentOnAResonantLineKeepsTheOtherHalfRerolled()
        {
            // 2 affixes on one line: 0.6 picks the second half (the suffix), so the prefix stays, rolled in 5–25.
            var result = OrbRules.Apply("annulment", Item("magic", "p_fire+s_fire"), Context(), Sequence(0.6, 0.99));
            var line = Assert.Single(result.Loot.Lines);
            Assert.False(line.Resonant);
            Assert.Equal("p_fire", line.Ids[0]);
            Assert.Equal(25f, line.Value);
            Assert.True(Assert.Single(result.Lost).Resonant);
            Assert.Equal(new[] { "A resonant line loses one half; the other half rerolls in its own range" },
                OrbRules.Preview("annulment", Item("magic", "p_fire+s_fire"), Context()).Skip(1));
        }

        // ---- Locks: corrupted, mirrored, Jeweller's ----

        [Fact]
        public void CorruptedItemsTakeOnlyTheMirror()
        {
            var item = Item("rare", "p_damage", "s_speed");
            item.Corrupted = true;
            foreach (var orb in new[] { "exalted", "divine", "annulment" })
            {
                Assert.Equal(OrbRules.CorruptedLocked, Refusal(orb, item));
            }
            // Krogon, 2026-11-08: the Mirror's exception.
            var result = OrbRules.Apply("mirror", item, Context(), Sequence(0.0));
            Assert.True(result.Done);
            Assert.True(result.Copy);
            Assert.Equal(new[] { "p_damage", "s_speed" }, result.Loot.Lines.Select(l => l.Ids[0]));
        }

        [Fact]
        public void MirroredCopiesCantBeChangedOrMirrored()
        {
            var copy = Item("magic", "p_damage");
            copy.Mirrored = true;
            Assert.Equal(OrbRules.MirroredCopy, Refusal("mirror", copy));
            Assert.Equal(OrbRules.MirroredLocked, Refusal("augmentation", copy));
            Assert.Equal(OrbRules.MirroredLocked, Refusal("divine", copy));
            Assert.Equal(OrbRules.MirroredLocked, Refusal("annulment", copy));
        }

        [Fact]
        public void MirrorRefusesNormalItemsAndUniques()
        {
            Assert.Equal(OrbRules.OnlyMagicOrRare, Refusal("mirror", Item("normal")));
            Assert.Equal(OrbRules.UniqueOnly, Refusal("mirror", new CraftItem { Rarity = "unique" }));
        }

        [Fact]
        public void JewellersOrbIsAlwaysGreyed()
        {
            foreach (var item in new[] { Item("normal"), Item("magic", "p_damage"), Item("rare", "p_damage") })
            {
                Assert.Equal(OrbRules.SocketsLater, Refusal("jeweller", item));
                var result = OrbRules.Apply("jeweller", item, Context(), Sequence(0.0));
                Assert.False(result.Done);
            }
        }

        [Fact]
        public void ApplyLeavesTheInputAlone()
        {
            var item = Item("magic", "p_fire");
            OrbRules.Apply("augmentation", item, Context(), Sequence(0.0));
            OrbRules.Apply("annulment", item, Context(), Sequence(0.0));
            OrbRules.Apply("divine", item, Context(), Sequence(0.99));
            var line = Assert.Single(item.Loot.Lines);
            Assert.Equal(new[] { "p_fire" }, line.Ids);
            Assert.Equal(5f, line.Value);
        }

        [Fact]
        public void ARefusalChangesNothing()
        {
            var result = OrbRules.Apply("regal", Item("magic", "p_damage"), Context(), Sequence(0.0));
            Assert.False(result.Done);
            Assert.Equal(OrbRules.OnlyMagicWithTwo, result.Refusal);
            Assert.Equal("magic", result.Rarity);
        }
    }
}
