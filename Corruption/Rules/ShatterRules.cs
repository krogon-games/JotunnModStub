using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Shattering items into Affix Splinters and which items can take one. Reads and checks
    ///     <c>Corruption/Data/shattering.csv</c>. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class ShatterRules
    {
        /// <summary>
        ///     The Affix Splinter item's prefab name, or the start of it (the Splinters tab recognises splinters by it).
        ///     The item itself is #29's (ROADMAP L5) and isn't built yet; it must use this name.
        /// </summary>
        public const string SplinterPrefab = "CorruptionAffixSplinter";

        /// <summary>Most affixes an item can have and still take a splinter, counting the splinter (the Magic limit).</summary>
        public const int MaxAffixesWithSplinters = 2;

        private static readonly string[] Settings = { "splinter_chance" };
        private static readonly string[] RequiredColumns = { "setting", "status", "value" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Chance in percent that each affix line of a shattered item becomes a splinter.</summary>
        public float SplinterChance;

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static ShatterRules Parse(string csv, List<string> errors)
        {
            var rules = new ShatterRules();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("shattering.csv is empty.");
                return rules;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"shattering.csv: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return rules;
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

                string setting = Cell("setting").ToLowerInvariant();
                string where = $"shattering.csv line {i + 1} ({(setting.Length > 0 ? setting : "no setting")})";
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Settings.Contains(setting))
                {
                    errors.Add($"{where}: setting must be one of {string.Join(", ", Settings)}.");
                    continue;
                }
                if (!seen.Add(setting))
                {
                    errors.Add($"{where}: setting is listed twice.");
                    continue;
                }
                if (!float.TryParse(Cell("value"), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < 0f || value > 100f)
                {
                    errors.Add($"{where}: value '{Cell("value")}' must be a percentage from 0 to 100 (use a dot for decimals).");
                    continue;
                }
                rules.SplinterChance = value;
            }
            foreach (var setting in Settings.Where(s => !seen.Contains(s)))
            {
                errors.Add($"shattering.csv: no row for {setting}.");
            }
            return rules;
        }

        /// <summary>Uniques and corrupted items can't be shattered; Magic and Rare items can.</summary>
        public static bool CanShatter(string rarity, bool corrupted)
        {
            return !corrupted && (rarity == "magic" || rarity == "rare");
        }

        public const string OperationId = "shatter";

        // Refusal texts (riftforge-ui-brief.md §2).
        public const string OnlyMagicOrRare = "Only Magic and Rare items shatter";
        public const string NoUniquesOrCorrupted = "Uniques and corrupted items can't be shattered";

        /// <summary>Why the item can't be shattered, or "" when it can. The first reason that applies.</summary>
        public static string Refusal(CraftItem item)
        {
            if (item.Rarity == "unique" || item.Corrupted)
            {
                return NoUniquesOrCorrupted;
            }
            if (item.Rarity != "magic" && item.Rarity != "rare")
            {
                return OnlyMagicOrRare;
            }
            // A Mirror's copy is locked like a corrupted item (Krogon, 2026-11-10).
            if (item.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            return MaxSplinters(item) > 0 ? "" : OrbRules.NoAffixes;
        }

        /// <summary>The most splinters a shatter can give: one per displayed affix line (a resonant line is one).</summary>
        public static int MaxSplinters(CraftItem item)
        {
            return item.Loot == null ? 0 : item.Loot.Lines.Count;
        }

        /// <summary>
        ///     Whether every possible splinter has a place before anything is rolled (Krogon, 2026-11-10): free cells in
        ///     the Splinters tab plus free Bag cells, plus the Bag cell the shattered item frees when it is in the Bag
        ///     grid. Checked against the most that can come out, so storage never depends on the roll.
        /// </summary>
        public static bool HasRoom(int maxSplinters, int freeSplinterCells, int freeBagCells, bool sourceInBag)
        {
            return Math.Max(0, freeSplinterCells) + Math.Max(0, freeBagCells) + (sourceInBag ? 1 : 0) >= maxSplinters;
        }

        /// <summary>The refusal when there isn't room: "Make room for up to 3 splinters."</summary>
        public static string MakeRoom(int maxSplinters)
        {
            return $"Make room for up to {maxSplinters} splinter{(maxSplinters == 1 ? "" : "s")}.";
        }

        /// <summary>What the window shows before confirming: each line with its chance, and that the item is destroyed.</summary>
        public List<string> Preview(CraftItem item, IDictionary<string, AffixTable.Affix> affixes)
        {
            var lines = new List<string>();
            string chance = SplinterChance.ToString("0.##", CultureInfo.InvariantCulture) + "%";
            if (item.Loot != null)
            {
                lines.AddRange(item.Loot.Lines.Select(l => chance + " · " + LootItem.LineText(l, affixes)));
            }
            lines.Add("The item is destroyed. Each line has that chance to become an Affix Splinter with its exact value.");
            return lines;
        }

        /// <summary>
        ///     Shatters the item's lines: the splinters that come out, each the line's affix (or twin) and exact value.
        ///     Rolls once per displayed line. Never changes <paramref name="item" />.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public List<LootItem.Line> Shatter(CraftItem item, Func<double> random)
        {
            var splinters = new List<LootItem.Line>();
            if (item.Loot == null)
            {
                return splinters;
            }
            var kept = RollSplinters(item.Loot.Lines.Count, random);
            for (int i = 0; i < kept.Length; i++)
            {
                if (kept[i])
                {
                    splinters.Add(SplinterRules.FromShattered(item.Loot.Lines[i]));
                }
            }
            return splinters;
        }

        /// <summary>
        ///     Rolls a shattered item's affix lines: true where that line becomes a splinter. A resonant twin line is one
        ///     line (one roll) and gives one splinter with the combined value.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public bool[] RollSplinters(int affixLines, Func<double> random)
        {
            var result = new bool[Math.Max(0, affixLines)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = random() * 100.0 < SplinterChance;
            }
            return result;
        }

        /// <summary>
        ///     Whether an item can take a splinter: Normal or Magic, not corrupted, and still within 2 affixes in total
        ///     once the splinter is added. A resonant splinter (prefix and suffix together) counts as 2 affixes.
        ///     Slot, family and item-class checks are separate.
        /// </summary>
        public static bool CanTakeSplinter(string rarity, bool corrupted, int affixCount, bool resonantSplinter)
        {
            if (corrupted || rarity != "normal" && rarity != "magic")
            {
                return false;
            }
            return affixCount + (resonantSplinter ? 2 : 1) <= MaxAffixesWithSplinters;
        }
    }
}
