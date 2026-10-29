using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     The items monsters can drop as Magic or Rare loot (<c>Corruption/Data/loot_bases.csv</c>, built from the game
    ///     dump by <c>tools/make_loot_bases.py</c>), the item-class rules, and picking a base item for a kill.
    ///     No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class LootBases
    {
        /// <summary>Biomes in progression order, as the game names them.</summary>
        public static readonly string[] Biomes =
            { "Meadows", "BlackForest", "Swamp", "Mountain", "Plains", "Mistlands", "Ashlands", "DeepNorth" };

        private static readonly string[] WeaponTypes = { "OneHandedWeapon", "TwoHandedWeapon", "TwoHandedWeaponLeft", "Bow" };

        private static readonly string[] WeaponSkills =
            { "Swords", "Knives", "Clubs", "Polearms", "Spears", "Axes", "Unarmed", "Bows", "Crossbows", "ElementalMagic", "BloodMagic" };

        private static readonly string[] MeleeSkills = { "Swords", "Knives", "Clubs", "Polearms", "Spears", "Axes", "Unarmed", "Pickaxes" };
        private static readonly string[] RangedSkills = { "Bows", "Crossbows" };
        private static readonly string[] MagicSkills = { "ElementalMagic", "BloodMagic" };

        private static readonly Dictionary<string, string> ArmourClasses = new Dictionary<string, string>
        {
            { "Shield", "shield" }, { "Helmet", "helmet" }, { "Chest", "chest" }, { "Legs", "legs" },
            { "Shoulder", "cape" }, { "Trinket", "jewellery" }
        };

        public sealed class Base
        {
            public string Prefab;
            public string Name;
            public string Biome;
            public List<string> Classes = new List<string>();
        }

        public readonly List<Base> Bases = new List<Base>();

        /// <summary>
        ///     The affix item classes of an item (see <c>AffixTable.ItemClasses</c>), from its game data: its item type,
        ///     skill and stack size. Empty if it can't carry affixes (tools, bombs, ammo, food, materials).
        ///     <c>tools/make_loot_bases.py</c> uses the same rules.
        /// </summary>
        public static List<string> Classify(string itemType, string skill, int maxStack)
        {
            var classes = new List<string>();
            if (maxStack != 1)
            {
                return classes;
            }
            if (WeaponTypes.Contains(itemType))
            {
                if (skill == "Pickaxes")
                {
                    classes.Add("pickaxe");
                }
                else if (WeaponSkills.Contains(skill))
                {
                    classes.Add("weapon");
                    if (skill == "Axes" && itemType == "OneHandedWeapon")
                    {
                        classes.Add("axe_1h");
                    }
                }
                return classes;
            }
            if (ArmourClasses.TryGetValue(itemType, out var armourClass))
            {
                classes.Add(armourClass);
            }
            return classes;
        }

        /// <summary>
        ///     The mechanics an affix can require (see <c>AffixTable.Requirements</c>): melee, ranged or magic from the
        ///     skill, combo when the primary attack chains 2+ hits, block when the item has block power.
        /// </summary>
        public static List<string> Mechanics(string skill, int attackChainLevels, float blockPower)
        {
            var mechanics = new List<string>();
            if (MeleeSkills.Contains(skill))
            {
                mechanics.Add("melee");
            }
            if (RangedSkills.Contains(skill))
            {
                mechanics.Add("ranged");
            }
            if (MagicSkills.Contains(skill))
            {
                mechanics.Add("magic");
            }
            if (attackChainLevels > 1)
            {
                mechanics.Add("combo");
            }
            if (blockPower > 0f)
            {
                mechanics.Add("block");
            }
            return mechanics;
        }

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static LootBases Parse(string csv, List<string> errors)
        {
            var table = new LootBases();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("loot_bases.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = new[] { "prefab", "name", "biome", "classes" }.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"loot_bases.csv: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return table;
            }

            var seen = new HashSet<string>();
            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i];
                if (cells.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string Cell(string column)
                {
                    int index = header.IndexOf(column);
                    return index >= 0 && index < cells.Count ? cells[index].Trim() : "";
                }

                var item = new Base
                {
                    Prefab = Cell("prefab"),
                    Name = Cell("name"),
                    Biome = Cell("biome"),
                    Classes = Cell("classes").Split(';').Select(c => c.Trim()).Where(c => c.Length > 0).ToList()
                };
                string where = $"loot_bases.csv line {i + 1} ({(item.Prefab.Length > 0 ? item.Prefab : "no prefab")})";
                if (item.Prefab.Length == 0 || !seen.Add(item.Prefab))
                {
                    errors.Add($"{where}: prefab is empty or listed twice.");
                    continue;
                }
                if (!Biomes.Contains(item.Biome))
                {
                    errors.Add($"{where}: biome '{item.Biome}' must be one of {string.Join(", ", Biomes)}.");
                    continue;
                }
                if (item.Classes.Count == 0 || item.Classes.Any(c => !AffixTable.ItemClasses.Contains(c)))
                {
                    errors.Add($"{where}: classes '{Cell("classes")}' must be item classes from affixes.csv, separated by ;.");
                    continue;
                }
                table.Bases.Add(item);
            }
            return table;
        }

        /// <summary>
        ///     Picks the base item for a Magic or Rare drop from a creature that died in <paramref name="biome" />: with
        ///     <paramref name="previousBiomePercent" /> chance an item from the biome just before it, otherwise one from
        ///     its own biome, each item equally likely. <paramref name="available" /> filters out items the game doesn't
        ///     have (e.g. backpacks without AdventureBackpacks). Null when the biome isn't on the progression list (the
        ///     Ocean) or has no items.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public Base Pick(string biome, float previousBiomePercent, Func<string, bool> available, Func<double> random)
        {
            // Case-insensitive: the game's biome enum spells the Ashlands "AshLands".
            int index = Array.FindIndex(Biomes, b => string.Equals(b, biome, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }
            if (index > 0 && random() * 100.0 < previousBiomePercent)
            {
                index--;
            }
            var choices = Bases.Where(b => b.Biome == Biomes[index] && (available == null || available(b.Prefab))).ToList();
            if (choices.Count == 0)
            {
                return null;
            }
            return choices[Math.Min(choices.Count - 1, (int)(random() * choices.Count))];
        }
    }
}
