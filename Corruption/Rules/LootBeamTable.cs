using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>One row of <c>loot_beams.csv</c>: how a kind of drop's beam looks and sounds.</summary>
    public sealed class LootBeamRow
    {
        public string Drop;
        public string Status;

        /// <summary>The colour as written, <c>#RRGGBB</c>.</summary>
        public string Colour;

        /// <summary>The colour as 0..1 red, green and blue.</summary>
        public float R, G, B;

        /// <summary>True for a tall, bright beam; false for a short, faint one.</summary>
        public bool Tall;

        /// <summary>The landing sound: empty for none, <see cref="LootBeamTable.DefaultSound" /> for the game's own.</summary>
        public string Sound;

        public string Notes;

        public bool HasSound => !string.IsNullOrEmpty(Sound);
    }

    /// <summary>
    ///     Reads <c>Corruption/Data/loot_beams.csv</c>: the beam colour, size and landing sound of every Corruption
    ///     drop. No game dependencies, so it is unit-tested.
    /// </summary>
    public sealed class LootBeamTable
    {
        /// <summary>The placeholder sound value: the game's skill-level-up sound.</summary>
        public const string DefaultSound = "default";

        public const string Shard = "shard";
        public const string Map = "map";

        private static readonly string[] Columns = { "drop", "status", "colour", "size", "sound", "notes" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        private readonly List<LootBeamRow> rows = new List<LootBeamRow>();

        public IList<LootBeamRow> Rows => rows;

        /// <summary>Parses the table. Every problem found is added to <paramref name="errors" />, naming the file and line.</summary>
        public static LootBeamTable Parse(string csv, List<string> errors)
        {
            var table = new LootBeamTable();
            var all = Csv.ReadRows(csv);
            if (all.Count == 0)
            {
                errors.Add("loot_beams.csv is empty.");
                return table;
            }
            var header = all[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            bool complete = true;
            foreach (var column in Columns.Where(c => !header.Contains(c)))
            {
                errors.Add($"loot_beams.csv: missing column '{column}'.");
                complete = false;
            }
            if (!complete)
            {
                return table;
            }
            string Cell(List<string> row, string column)
            {
                int index = header.IndexOf(column);
                return index < row.Count ? row[index].Trim() : "";
            }

            var seen = new HashSet<string>();
            for (int i = 1; i < all.Count; i++)
            {
                var row = all[i];
                if (row.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string drop = Cell(row, "drop").ToLowerInvariant();
                string where = $"loot_beams.csv line {i + 1} ({drop})";
                if (drop.Length == 0)
                {
                    errors.Add($"{where}: drop is empty.");
                    continue;
                }
                if (!seen.Add(drop))
                {
                    errors.Add($"{where}: drop is listed twice.");
                    continue;
                }
                string status = Cell(row, "status");
                if (!Statuses.Contains(status.ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{status}' must be one of {string.Join(", ", Statuses)}.");
                }
                string size = Cell(row, "size").ToLowerInvariant();
                if (size != "short" && size != "tall")
                {
                    errors.Add($"{where}: size '{Cell(row, "size")}' must be 'short' or 'tall'.");
                }
                string colour = Cell(row, "colour");
                bool colourOk = TryParseColour(colour, out float r, out float g, out float b);
                if (!colourOk)
                {
                    errors.Add($"{where}: colour '{colour}' must be #RRGGBB.");
                }
                table.rows.Add(new LootBeamRow
                {
                    Drop = drop,
                    Status = status,
                    Colour = colour,
                    R = r,
                    G = g,
                    B = b,
                    Tall = size == "tall",
                    Sound = Cell(row, "sound"),
                    Notes = Cell(row, "notes")
                });
            }
            return table;
        }

        private static bool TryParseColour(string text, out float r, out float g, out float b)
        {
            r = g = b = 0f;
            if (text.Length != 7 || text[0] != '#')
            {
                return false;
            }
            if (!int.TryParse(text.Substring(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int value))
            {
                return false;
            }
            r = ((value >> 16) & 0xFF) / 255f;
            g = ((value >> 8) & 0xFF) / 255f;
            b = (value & 0xFF) / 255f;
            return true;
        }

        /// <summary>The row for a drop, or null (case-insensitive).</summary>
        public LootBeamRow Find(string drop) =>
            string.IsNullOrEmpty(drop) ? null : rows.FirstOrDefault(r => string.Equals(r.Drop, drop, StringComparison.OrdinalIgnoreCase));

        /// <summary>The row name for a gear rarity: magic, rare or unique; null for anything else (Normal gear has no beam).</summary>
        public static string ForGear(string rarity)
        {
            switch (rarity)
            {
                case "magic":
                case "rare":
                case "unique":
                    return rarity;
                default:
                    return null;
            }
        }

        /// <summary>The row name for an orb id ("orb_regal"); null for none.</summary>
        public static string ForOrb(string orbId) => string.IsNullOrEmpty(orbId) ? null : "orb_" + orbId.ToLowerInvariant();
    }

    /// <summary>The settings of <c>Corruption/Data/personal_loot.csv</c>: the lock, the sound window and the beam look.</summary>
    public sealed class PersonalLootSettings
    {
        public double LockSeconds, SoundWindowSeconds, RefreshSeconds, PulseSeconds;
        public float TopWidthFraction, PulseStrength;
        public float TallHeight, TallWidth, TallLightRange, TallLightIntensity;
        public float ShortHeight, ShortWidth, ShortLightRange, ShortLightIntensity;

        private static readonly string[] Names =
        {
            "lock_seconds", "sound_window_seconds", "refresh_seconds",
            "tall_height", "tall_width", "tall_light_range", "tall_light_intensity",
            "short_height", "short_width", "short_light_range", "short_light_intensity",
            "top_width_fraction", "pulse_seconds", "pulse_strength"
        };

        /// <summary>Parses the table. Every setting is required. Problems are added to <paramref name="errors" />.</summary>
        public static PersonalLootSettings Parse(string csv, List<string> errors)
        {
            var settings = new PersonalLootSettings();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("personal_loot.csv is empty.");
                return settings;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            int nameIndex = header.IndexOf("setting");
            int valueIndex = header.IndexOf("value");
            if (nameIndex < 0 || valueIndex < 0)
            {
                errors.Add("personal_loot.csv: needs the columns 'setting' and 'value'.");
                return settings;
            }
            var values = new Dictionary<string, double>();
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string name = nameIndex < row.Count ? row[nameIndex].Trim().ToLowerInvariant() : "";
                string text = valueIndex < row.Count ? row[valueIndex].Trim() : "";
                string where = $"personal_loot.csv line {i + 1} ({name})";
                if (!Names.Contains(name))
                {
                    errors.Add($"{where}: unknown setting.");
                    continue;
                }
                if (values.ContainsKey(name))
                {
                    errors.Add($"{where}: setting is listed twice.");
                    continue;
                }
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
                {
                    errors.Add($"{where}: value '{text}' must be a number.");
                    continue;
                }
                if (number < 0)
                {
                    errors.Add($"{where}: value {text} can't be negative.");
                    continue;
                }
                if (number <= 0 && (name == "lock_seconds" || name == "refresh_seconds" || name == "pulse_seconds"))
                {
                    errors.Add($"{where}: value must be more than 0.");
                    continue;
                }
                if (number > 1 && (name == "top_width_fraction" || name == "pulse_strength"))
                {
                    errors.Add($"{where}: value {text} must be between 0 and 1.");
                    continue;
                }
                values[name] = number;
            }
            foreach (var name in Names.Where(n => !values.ContainsKey(n)))
            {
                errors.Add($"personal_loot.csv: missing setting '{name}'.");
            }
            double V(string name) => values.TryGetValue(name, out var v) ? v : 0.0;
            settings.LockSeconds = V("lock_seconds");
            settings.SoundWindowSeconds = V("sound_window_seconds");
            settings.RefreshSeconds = V("refresh_seconds");
            settings.TallHeight = (float)V("tall_height");
            settings.TallWidth = (float)V("tall_width");
            settings.TallLightRange = (float)V("tall_light_range");
            settings.TallLightIntensity = (float)V("tall_light_intensity");
            settings.ShortHeight = (float)V("short_height");
            settings.ShortWidth = (float)V("short_width");
            settings.ShortLightRange = (float)V("short_light_range");
            settings.ShortLightIntensity = (float)V("short_light_intensity");
            settings.TopWidthFraction = (float)V("top_width_fraction");
            settings.PulseSeconds = V("pulse_seconds");
            settings.PulseStrength = (float)V("pulse_strength");
            return settings;
        }
    }
}
