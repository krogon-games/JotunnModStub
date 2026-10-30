using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the loot settings (<c>Corruption/Data/loot_rules.csv</c>) and the Rare name words
    ///     (<c>rare_names.csv</c>). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class LootRules
    {
        private static readonly string[] Settings =
        {
            "previous_biome_percent", "drop_quality", "magic_1_affix_weight", "magic_2_affix_weight", "rare_3_affix_weight",
            "rare_4_affix_weight"
        };

        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Rare name word lists: the first word, then the second word by kind of item.</summary>
        public static readonly string[] NameLists = { "first", "weapon", "armour", "shield", "jewellery" };

        public float PreviousBiomePercent;
        public int DropQuality = 1;
        public float Magic1Weight, Magic2Weight, Rare3Weight, Rare4Weight;

        /// <summary>List → words.</summary>
        public readonly Dictionary<string, List<string>> Names = new Dictionary<string, List<string>>();

        /// <summary>Parses both tables. Every problem found is added to <paramref name="errors" />, naming the file and line.</summary>
        public static LootRules Parse(string rulesCsv, string namesCsv, List<string> errors)
        {
            var rules = new LootRules();
            var values = new Dictionary<string, float>();
            ReadTable(rulesCsv, "loot_rules.csv", new[] { "setting", "status", "value" }, errors, (cell, where) =>
            {
                string setting = cell("setting").ToLowerInvariant();
                if (!Statuses.Contains(cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Settings.Contains(setting))
                {
                    errors.Add($"{where}: setting must be one of {string.Join(", ", Settings)}.");
                    return;
                }
                if (values.ContainsKey(setting))
                {
                    errors.Add($"{where}: setting is listed twice.");
                    return;
                }
                if (!float.TryParse(cell("value"), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < 0f)
                {
                    errors.Add($"{where}: value '{cell("value")}' must be a number of 0 or more (use a dot for decimals).");
                    return;
                }
                values[setting] = value;
            });
            foreach (var setting in Settings.Where(s => !values.ContainsKey(s)))
            {
                errors.Add($"loot_rules.csv: no row for {setting}.");
            }
            float Get(string setting) => values.TryGetValue(setting, out var v) ? v : 0f;
            rules.PreviousBiomePercent = Get("previous_biome_percent");
            rules.DropQuality = Math.Max(1, (int)Get("drop_quality"));
            rules.Magic1Weight = Get("magic_1_affix_weight");
            rules.Magic2Weight = Get("magic_2_affix_weight");
            rules.Rare3Weight = Get("rare_3_affix_weight");
            rules.Rare4Weight = Get("rare_4_affix_weight");
            if (values.Count == Settings.Length)
            {
                if (rules.PreviousBiomePercent > 100f)
                {
                    errors.Add("loot_rules.csv: previous_biome_percent can't be above 100.");
                }
                if (rules.Magic1Weight + rules.Magic2Weight <= 0f || rules.Rare3Weight + rules.Rare4Weight <= 0f)
                {
                    errors.Add("loot_rules.csv: each rarity needs at least one affix-count weight above 0.");
                }
            }

            ReadTable(namesCsv, "rare_names.csv", new[] { "list", "word" }, errors, (cell, where) =>
            {
                string list = cell("list").ToLowerInvariant();
                string word = cell("word");
                if (!NameLists.Contains(list))
                {
                    errors.Add($"{where}: list must be one of {string.Join(", ", NameLists)}.");
                    return;
                }
                if (word.Length == 0)
                {
                    errors.Add($"{where}: word is empty.");
                    return;
                }
                if (!rules.Names.TryGetValue(list, out var words))
                {
                    rules.Names[list] = words = new List<string>();
                }
                words.Add(word);
            });
            foreach (var list in NameLists.Where(l => !rules.Names.ContainsKey(l)))
            {
                errors.Add($"rare_names.csv: no words for {list}.");
            }
            return rules;
        }

        /// <summary>How many affixes a new Magic (1–2) or Rare (3–4) item gets.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public int AffixCount(string rarity, Func<double> random)
        {
            bool rare = rarity == "rare";
            float low = rare ? Rare3Weight : Magic1Weight;
            float high = rare ? Rare4Weight : Magic2Weight;
            int fewer = rare ? 3 : 1;
            return random() * (low + high) < low ? fewer : fewer + 1;
        }

        /// <summary>
        ///     A Rare item's two-word name: a first word, then a second word for its kind (weapons and tools, armour,
        ///     shields, jewellery).
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public string RareName(ICollection<string> itemClasses, Func<double> random)
        {
            string kind = itemClasses.Contains("shield") ? "shield"
                : itemClasses.Contains("jewellery") ? "jewellery"
                : itemClasses.Any(c => c == "helmet" || c == "chest" || c == "legs" || c == "cape") ? "armour"
                : "weapon";
            return Word("first", random) + " " + Word(kind, random);
        }

        private string Word(string list, Func<double> random)
        {
            if (!Names.TryGetValue(list, out var words) || words.Count == 0)
            {
                return "";
            }
            return words[Math.Min(words.Count - 1, (int)(random() * words.Count))];
        }

        private static void ReadTable(string csv, string file, string[] required, List<string> errors,
            Action<Func<string, string>, string> readRow)
        {
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add($"{file} is empty.");
                return;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = required.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"{file}: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return;
            }
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
                readRow(Cell, $"{file} line {i + 1}");
            }
        }
    }
}
