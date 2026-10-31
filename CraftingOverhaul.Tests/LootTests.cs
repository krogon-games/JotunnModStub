using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class LootTests
    {
        private static string RepoFile(params string[] parts)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(new[] { dir }.Concat(parts).ToArray())))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(new[] { dir }.Concat(parts).ToArray()));
        }

        private static string DataFile(string name) => RepoFile("Corruption", "Data", name);

        private static List<Dictionary<string, string>> DumpRows(string file)
        {
            var rows = Csv.ReadRows(RepoFile("data", "game-dump-1.0.20", file));
            var header = rows[0];
            return rows.Skip(1).Where(r => r.Count == header.Count)
                .Select(r => header.Select((h, i) => new KeyValuePair<string, string>(h, r[i])).ToDictionary(p => p.Key, p => p.Value))
                .ToList();
        }

        private static Func<double> Sequence(params double[] values)
        {
            int i = 0;
            return () => values[Math.Min(i++, values.Length - 1)];
        }

        private static LootBases RealBases()
        {
            var errors = new List<string>();
            var bases = LootBases.Parse(DataFile("loot_bases.csv"), errors);
            Assert.True(errors.Count == 0, "loot_bases.csv has problems:\n" + string.Join("\n", errors));
            return bases;
        }

        private static LootRules RealRules()
        {
            var errors = new List<string>();
            var rules = LootRules.Parse(DataFile("loot_rules.csv"), DataFile("rare_names.csv"), errors);
            Assert.True(errors.Count == 0, "Loot rule tables have problems:\n" + string.Join("\n", errors));
            return rules;
        }

        private static List<AffixTable.Affix> RealAffixes()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return affixes;
        }

        // ---- Base items ----

        [Fact]
        public void BaseTableMatchesTheGameDump()
        {
            var bases = RealBases();
            var items = DumpRows("items.csv").ToDictionary(r => r["prefab"]);
            var craftable = new HashSet<string>(DumpRows("recipes.csv").Where(r => r["enabled"] == "True").Select(r => r["output"]));
            // DLC items (m_dlc set, e.g. the Odin "beta" items) can't be used without the DLC, so they never drop.
            var dlc = new HashSet<string>(DumpRows("item_details.csv").Where(r => r["m_dlc"].Length > 0).Select(r => r["prefab"]));
            Assert.NotEmpty(dlc);

            foreach (var item in bases.Bases)
            {
                Assert.True(items.ContainsKey(item.Prefab), $"{item.Prefab} isn't in the dump.");
                Assert.True(craftable.Contains(item.Prefab), $"{item.Prefab} has no enabled recipe.");
                Assert.Equal(items[item.Prefab]["biome"], item.Biome);
            }

            // Every craftable item the classifier accepts (from a progression biome) is in the table, with the same classes.
            var byPrefab = bases.Bases.ToDictionary(b => b.Prefab);
            foreach (var prefab in craftable.Where(items.ContainsKey))
            {
                var row = items[prefab];
                var classes = LootBases.Classify(row["type"], row["skill"], int.Parse(row["max_stack"]));
                if (classes.Count == 0 || !LootBases.Biomes.Contains(row["biome"]) || dlc.Contains(prefab))
                {
                    Assert.False(byPrefab.ContainsKey(prefab), $"{prefab} shouldn't be a loot base.");
                    continue;
                }
                Assert.True(byPrefab.ContainsKey(prefab), $"{prefab} is missing from loot_bases.csv (rerun tools/make_loot_bases.py).");
                Assert.Equal(classes, byPrefab[prefab].Classes);
            }
            Assert.All(LootBases.Biomes, biome => Assert.Contains(bases.Bases, b => b.Biome == biome));
        }

        [Fact]
        public void ClassifyFollowsTheAffixItemClasses()
        {
            Assert.Equal(new[] { "weapon", "axe_1h" }, LootBases.Classify("OneHandedWeapon", "Axes", 1));
            Assert.Equal(new[] { "weapon" }, LootBases.Classify("TwoHandedWeapon", "Axes", 1));
            Assert.Equal(new[] { "pickaxe" }, LootBases.Classify("TwoHandedWeapon", "Pickaxes", 1));
            Assert.Equal(new[] { "weapon" }, LootBases.Classify("Bow", "Crossbows", 1));
            Assert.Equal(new[] { "weapon" }, LootBases.Classify("TwoHandedWeaponLeft", "BloodMagic", 1));
            Assert.Equal(new[] { "cape" }, LootBases.Classify("Shoulder", "None", 1));
            Assert.Equal(new[] { "jewellery" }, LootBases.Classify("Trinket", "Swords", 1));
            Assert.Empty(LootBases.Classify("OneHandedWeapon", "None", 10)); // bombs stack
            Assert.Empty(LootBases.Classify("Bow", "None", 1)); // grappling hook
            Assert.Empty(LootBases.Classify("TwoHandedWeapon", "Farming", 1)); // scythe
            Assert.Empty(LootBases.Classify("Torch", "Clubs", 1));
            Assert.Empty(LootBases.Classify("Utility", "Swords", 1));
        }

        [Fact]
        public void MechanicsComeFromSkillChainAndBlock()
        {
            Assert.Equal(new[] { "melee", "combo", "block" }, LootBases.Mechanics("Swords", 3, 20f));
            Assert.Equal(new[] { "ranged" }, LootBases.Mechanics("Bows", 1, 0f));
            Assert.Equal(new[] { "magic", "block" }, LootBases.Mechanics("ElementalMagic", 0, 10f));
        }

        [Fact]
        public void PickUsesTheBiomeOrTheOneBefore()
        {
            var bases = RealBases();
            var own = bases.Pick("Swamp", 20f, null, Sequence(0.5, 0.0));
            Assert.Equal("Swamp", own.Biome);
            var before = bases.Pick("Swamp", 20f, null, Sequence(0.1, 0.0));
            Assert.Equal("BlackForest", before.Biome);
            var meadows = bases.Pick("Meadows", 100f, null, Sequence(0.0, 0.0));
            Assert.Equal("Meadows", meadows.Biome);
            Assert.Equal("Ashlands", bases.Pick("AshLands", 0f, null, Sequence(0.5, 0.0)).Biome);
            Assert.Null(bases.Pick("Ocean", 20f, null, Sequence(0.5)));
            Assert.Null(bases.Pick("Swamp", 0f, prefab => false, Sequence(0.5, 0.5)));
        }

        // ---- Rules and names ----

        [Fact]
        public void RulesMatchTheUsersAnswers()
        {
            var rules = RealRules();
            Assert.Equal(20f, rules.PreviousBiomePercent);
            Assert.Equal(1, rules.DropQuality);
            Assert.Equal(1, rules.AffixCount("magic", () => 0.6));
            Assert.Equal(2, rules.AffixCount("magic", () => 0.7));
            Assert.Equal(3, rules.AffixCount("rare", () => 0.6));
            Assert.Equal(4, rules.AffixCount("rare", () => 0.7));
        }

        [Fact]
        public void RareNamesUseTheListForTheKindOfItem()
        {
            var rules = RealRules();
            Assert.Equal("Grim Bite", rules.RareName(new[] { "weapon", "axe_1h" }, () => 0.0));
            Assert.Equal("Grim Wall", rules.RareName(new[] { "shield" }, () => 0.0));
            Assert.Equal("Grim Shroud", rules.RareName(new[] { "cape" }, () => 0.0));
            Assert.Equal("Grim Eye", rules.RareName(new[] { "jewellery" }, () => 0.0));
            Assert.Equal("Ember Maw", rules.RareName(new[] { "pickaxe" }, () => 0.9999));
        }

        [Fact]
        public void RuleTablesReportProblems()
        {
            var errors = new List<string>();
            LootRules.Parse("setting,status,value\nprevious_biome_percent,approved,150\n", "list,word\nfirst,Grim\nodd,Word\n", errors);
            Assert.Contains(errors, e => e.Contains("no row for drop_quality"));
            Assert.Contains(errors, e => e.Contains("list must be one of"));
            Assert.Contains(errors, e => e.Contains("no words for weapon"));
        }

        // ---- Rolling ----

        private static AffixTable.Affix Affix(string id, string side, string family, float min, float max, float weight = 50f,
            string requires = "", string itemName = "", string classes = "weapon")
        {
            var affix = new AffixTable.Affix
            {
                Id = id, Side = side, Family = family, Name = id, ItemName = itemName.Length > 0 ? itemName : id, Text = "+{0} " + id,
                Min = min, Max = max, Weight = weight, Requires = requires, Effect = id, Unit = "flat"
            };
            foreach (var c in classes.Split(';'))
            {
                affix.Classes.Add(c);
            }
            return affix;
        }

        [Fact]
        public void MagicGetsAtMostOnePerSide()
        {
            var table = new[] { Affix("a", "prefix", "fa", 1, 5), Affix("b", "prefix", "fb", 1, 5), Affix("c", "suffix", "fc", 1, 5) };
            var item = AffixRoller.Roll("magic", 2, table, new[] { "weapon" }, new string[0], Sequence(0.0, 0.0, 0.0, 0.0));
            Assert.Equal(2, item.Lines.Count);
            Assert.Equal("a", item.Lines[0].Ids[0]);
            Assert.Equal("c", item.Lines[1].Ids[0]);
        }

        [Fact]
        public void OneAffixPerFamilyPerSideAndStopsWhenNothingFits()
        {
            var table = new[] { Affix("a", "prefix", "f", 1, 5), Affix("b", "prefix", "f", 1, 5) };
            var item = AffixRoller.Roll("rare", 4, table, new[] { "weapon" }, new string[0], () => 0.0);
            Assert.Single(item.Lines);
        }

        [Fact]
        public void ClassesAndRequirementsFilterTheAffixes()
        {
            var table = new[]
            {
                Affix("shield_only", "prefix", "f1", 1, 5, classes: "shield"),
                Affix("needs_block", "prefix", "f2", 1, 5, requires: "block"),
                Affix("plain", "suffix", "f3", 1, 5)
            };
            var item = AffixRoller.Roll("rare", 4, table, new[] { "weapon" }, new[] { "melee" }, () => 0.0);
            Assert.Equal(new[] { "plain" }, item.Lines.Select(l => l.Ids[0]));
            item = AffixRoller.Roll("rare", 4, table, new[] { "weapon" }, new[] { "block" }, () => 0.0);
            Assert.Equal(new[] { "needs_block", "plain" }, item.Lines.Select(l => l.Ids[0]));
        }

        [Fact]
        public void TwinHalvesMergeIntoAResonantLine()
        {
            var table = new[] { Affix("hp_p", "prefix", "hp", 5, 20), Affix("hp_s", "suffix", "hp", 5, 20) };
            // Suffix first, then the prefix: the line lists the prefix first and rolls 10 to 50 (40 x 1.25).
            var item = AffixRoller.Roll("magic", 2, table, new[] { "weapon" }, new string[0], Sequence(0.9, 0.0, 0.0, 0.9999));
            Assert.Single(item.Lines);
            Assert.Equal(new[] { "hp_p", "hp_s" }, item.Lines[0].Ids);
            Assert.Equal(50, item.Lines[0].Value);
            var byId = table.ToDictionary(a => a.Id);
            LootItem.Range(item.Lines[0], byId, out int min, out int max);
            Assert.Equal(10, min);
            Assert.Equal(50, max);
            Assert.Equal("+50 hp_p [10–50] ◆ resonant", LootItem.LineText(item.Lines[0], byId));
        }

        [Fact]
        public void ValuesAreWholeNumbersInRange()
        {
            Assert.Equal(5, AffixRoller.RollValue(5, 20, () => 0.0));
            Assert.Equal(20, AffixRoller.RollValue(5, 20, () => 0.9999));
            Assert.Equal(7, AffixRoller.RollValue(7, 7, () => 0.5));
        }

        [Fact]
        public void RealTableRollsEveryClassWithoutProblems()
        {
            var affixes = RealAffixes();
            var rules = RealRules();
            var random = new Random(1234);
            Func<double> next = random.NextDouble;
            var byId = affixes.ToDictionary(a => a.Id);
            foreach (var classes in new[] { new[] { "weapon" }, new[] { "weapon", "axe_1h" }, new[] { "pickaxe" }, new[] { "shield" },
                         new[] { "helmet" }, new[] { "chest" }, new[] { "legs" }, new[] { "cape" }, new[] { "jewellery" } })
            {
                foreach (var rarity in new[] { "magic", "rare" })
                {
                    for (int i = 0; i < 200; i++)
                    {
                        int count = rules.AffixCount(rarity, next);
                        var item = AffixRoller.Roll(rarity, count, affixes, classes, new[] { "melee", "combo", "block" }, next);
                        Assert.NotEmpty(item.Lines);
                        int slots = item.Lines.Sum(l => l.Ids.Length);
                        Assert.True(slots <= count, $"{rarity} {string.Join("+", classes)} rolled {slots} affixes for {count}.");
                        foreach (var line in item.Lines)
                        {
                            LootItem.Range(line, byId, out int min, out int max);
                            Assert.InRange(line.Value, min, max);
                        }
                        var prefixes = item.Lines.SelectMany(l => l.Ids).Count(id => byId[id].IsPrefix);
                        Assert.True(prefixes <= AffixRoller.SideLimit(rarity));
                    }
                }
            }
        }

        // ---- Storage and text ----

        [Fact]
        public void StorageRoundTrips()
        {
            var item = new LootItem { Rarity = "rare", Name = "Grim Fang" };
            item.Lines.Add(new LootItem.Line { Ids = new[] { "a" }, Value = 12 });
            item.Lines.Add(new LootItem.Line { Ids = new[] { "hp_p", "hp_s" }, Value = 27 });
            var data = new Dictionary<string, string>();
            item.Write(data);
            Assert.Equal("a=12;hp_p+hp_s=27", data[LootItem.AffixesKey]);

            var back = LootItem.Read(data);
            Assert.Equal("rare", back.Rarity);
            Assert.Equal("Grim Fang", back.Name);
            Assert.Equal(2, back.Lines.Count);
            Assert.True(back.Lines[1].Resonant);
            Assert.Equal(27, back.Lines[1].Value);

            Assert.Null(LootItem.Read(new Dictionary<string, string>()));
            Assert.Null(LootItem.Read(new Dictionary<string, string> { { LootItem.RarityKey, "magic" }, { LootItem.AffixesKey, "a=x" } }));
        }

        [Fact]
        public void MagicNamesUseRollQualityNames()
        {
            var table = new[]
            {
                Affix("dmg", "prefix", "dmg", 5, 40, itemName: "Sharp/Fine/Cruel/Merciless"),
                Affix("fire", "suffix", "fire", 10, 50, itemName: "of Embers/of Flame/of Fire/of Muspelheim")
            };
            var byId = table.ToDictionary(a => a.Id);
            var item = new LootItem { Rarity = "magic" };
            item.Lines.Add(new LootItem.Line { Ids = new[] { "dmg" }, Value = 5 });
            item.Lines.Add(new LootItem.Line { Ids = new[] { "fire" }, Value = 50 });
            Assert.Equal("Sharp Bronze Sword of Muspelheim", item.DisplayName("Bronze Sword", byId));
            item.Lines[0].Value = 30;
            Assert.Equal("Cruel Bronze Sword of Muspelheim", item.DisplayName("Bronze Sword", byId));
            Assert.Equal("Grim Fang", new LootItem { Rarity = "rare", Name = "Grim Fang" }.DisplayName("Bronze Sword", byId));
        }

        [Fact]
        public void LineTextShowsValueAndRange()
        {
            var table = new[] { Affix("dmg", "prefix", "dmg", 5, 40) };
            var line = new LootItem.Line { Ids = new[] { "dmg" }, Value = 24 };
            Assert.Equal("+24 dmg [5–40]", LootItem.LineText(line, table.ToDictionary(a => a.Id)));
            Assert.Equal("+24 dmg (+14) [5–40]", LootItem.LineText(line, table.ToDictionary(a => a.Id), "+14"));
        }

        [Fact]
        public void BonusTextRoundsLikeTheTooltip()
        {
            Assert.Equal("+23", LootItem.BonusText(22.79f));
            Assert.Equal("+0.3", LootItem.BonusText(0.3f));
            Assert.Equal("+273", LootItem.BonusText(273.4f));
            Assert.Equal("+0.6x", LootItem.BonusText(0.6f, "x"));
            Assert.Equal("−1.6", LootItem.BonusText(-1.6f));
            Assert.Null(LootItem.BonusText(0.01f));
        }
    }
}
