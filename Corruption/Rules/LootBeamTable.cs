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

    /// <summary>
    ///     The settings of <c>Corruption/Data/personal_loot.csv</c>: the lock, the sound window and the beam look. One setting
    ///     is checked and stored by <see cref="TrySet" />, which both the table parser and the live <c>co_personal look</c>
    ///     command use, so their rules can't drift.
    /// </summary>
    public sealed class PersonalLootSettings
    {
        public double LockSeconds, SoundWindowSeconds, RefreshSeconds, PulseSeconds;
        public float TopWidthFraction, PulseStrength;
        public float TallHeight, TallWidth, TallLightRange, TallLightIntensity;
        public float ShortHeight, ShortWidth, ShortLightRange, ShortLightIntensity;

        /// <summary>The core's opacity at its middle, 0..1.</summary>
        public float BeamAlpha;

        /// <summary>How soft the beam's edges are: 0 = hard, 1 = fades out from the very middle.</summary>
        public float EdgeSoftness;

        /// <summary>The glow's width as a multiple of the core's width, and its opacity as a share of <see cref="BeamAlpha" />.</summary>
        public float GlowWidth, GlowAlpha;

        /// <summary>The smoke wisps: how many a second (0 = none), how long each lives, how big, how opaque.</summary>
        public float SmokePerSecond, SmokeSeconds, SmokeSize, SmokeAlpha;

        /// <summary>Wisp shape: sideways drift (a share of the beam's base width, per second), size at birth and at the end (x smoke_size), and the share of its life spent fading in (0..1, under 1).</summary>
        public float SmokeDrift, SmokeStartScale, SmokeEndScale, SmokeFadeIn;

        private static readonly string[] Names =
        {
            "lock_seconds", "sound_window_seconds", "refresh_seconds",
            "tall_height", "tall_width", "tall_light_range", "tall_light_intensity",
            "short_height", "short_width", "short_light_range", "short_light_intensity",
            "top_width_fraction", "pulse_seconds", "pulse_strength",
            "beam_alpha", "edge_softness", "glow_width", "glow_alpha",
            "smoke_per_second", "smoke_seconds", "smoke_size", "smoke_alpha",
            "smoke_drift", "smoke_start_scale", "smoke_end_scale", "smoke_fade_in"
        };

        /// <summary>Every setting name, in table order.</summary>
        public static string[] AllNames() => (string[])Names.Clone();

        private static bool MoreThanZero(string name) =>
            name == "lock_seconds" || name == "refresh_seconds" || name == "pulse_seconds" ||
            name == "glow_width" || name == "smoke_seconds" || name == "smoke_size" ||
            name == "smoke_start_scale" || name == "smoke_end_scale";

        private static bool ZeroToOne(string name) =>
            name == "top_width_fraction" || name == "pulse_strength" || name == "beam_alpha" || name == "edge_softness" ||
            name == "glow_alpha" || name == "smoke_alpha" || name == "smoke_fade_in";

        /// <summary>
        ///     Checks and stores one setting. Returns false (and a plain message, with no table line in it) when the name is
        ///     unknown or the value isn't a valid number in that setting's range; nothing changes then.
        /// </summary>
        public bool TrySet(string name, string value, out string error)
        {
            error = null;
            name = (name ?? "").Trim().ToLowerInvariant();
            string text = (value ?? "").Trim();
            if (!Names.Contains(name))
            {
                error = "unknown setting.";
                return false;
            }
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) ||
                double.IsNaN(number) || double.IsInfinity(number))
            {
                error = $"value '{text}' must be a number.";
                return false;
            }
            if (number < 0)
            {
                error = $"value {text} can't be negative.";
                return false;
            }
            if (number <= 0 && MoreThanZero(name))
            {
                error = "value must be more than 0.";
                return false;
            }
            if (number > 1 && ZeroToOne(name))
            {
                error = $"value {text} must be between 0 and 1.";
                return false;
            }
            if (name == "smoke_fade_in" && number >= 1)
            {
                error = $"value {text} must be between 0 and 1, and less than 1.";
                return false;
            }
            float f = (float)number;
            switch (name)
            {
                case "lock_seconds": LockSeconds = number; break;
                case "sound_window_seconds": SoundWindowSeconds = number; break;
                case "refresh_seconds": RefreshSeconds = number; break;
                case "tall_height": TallHeight = f; break;
                case "tall_width": TallWidth = f; break;
                case "tall_light_range": TallLightRange = f; break;
                case "tall_light_intensity": TallLightIntensity = f; break;
                case "short_height": ShortHeight = f; break;
                case "short_width": ShortWidth = f; break;
                case "short_light_range": ShortLightRange = f; break;
                case "short_light_intensity": ShortLightIntensity = f; break;
                case "top_width_fraction": TopWidthFraction = f; break;
                case "pulse_seconds": PulseSeconds = number; break;
                case "pulse_strength": PulseStrength = f; break;
                case "beam_alpha": BeamAlpha = f; break;
                case "edge_softness": EdgeSoftness = f; break;
                case "glow_width": GlowWidth = f; break;
                case "glow_alpha": GlowAlpha = f; break;
                case "smoke_per_second": SmokePerSecond = f; break;
                case "smoke_seconds": SmokeSeconds = f; break;
                case "smoke_size": SmokeSize = f; break;
                case "smoke_alpha": SmokeAlpha = f; break;
                case "smoke_drift": SmokeDrift = f; break;
                case "smoke_start_scale": SmokeStartScale = f; break;
                case "smoke_end_scale": SmokeEndScale = f; break;
                case "smoke_fade_in": SmokeFadeIn = f; break;
            }
            return true;
        }

        /// <summary>A setting's current value; 0 for an unknown name.</summary>
        public double Get(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "lock_seconds": return LockSeconds;
                case "sound_window_seconds": return SoundWindowSeconds;
                case "refresh_seconds": return RefreshSeconds;
                case "tall_height": return TallHeight;
                case "tall_width": return TallWidth;
                case "tall_light_range": return TallLightRange;
                case "tall_light_intensity": return TallLightIntensity;
                case "short_height": return ShortHeight;
                case "short_width": return ShortWidth;
                case "short_light_range": return ShortLightRange;
                case "short_light_intensity": return ShortLightIntensity;
                case "top_width_fraction": return TopWidthFraction;
                case "pulse_seconds": return PulseSeconds;
                case "pulse_strength": return PulseStrength;
                case "beam_alpha": return BeamAlpha;
                case "edge_softness": return EdgeSoftness;
                case "glow_width": return GlowWidth;
                case "glow_alpha": return GlowAlpha;
                case "smoke_per_second": return SmokePerSecond;
                case "smoke_seconds": return SmokeSeconds;
                case "smoke_size": return SmokeSize;
                case "smoke_alpha": return SmokeAlpha;
                case "smoke_drift": return SmokeDrift;
                case "smoke_start_scale": return SmokeStartScale;
                case "smoke_end_scale": return SmokeEndScale;
                case "smoke_fade_in": return SmokeFadeIn;
                default: return 0.0;
            }
        }

        /// <summary>A setting's value as text for a console line or a CSV row (the shortest form that reads back the same).</summary>
        public string GetText(string name) => Get(name).ToString("0.######", CultureInfo.InvariantCulture);

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
            var seen = new HashSet<string>();
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
                if (Names.Contains(name) && !seen.Add(name))
                {
                    errors.Add($"{where}: setting is listed twice.");
                    continue;
                }
                if (!settings.TrySet(name, text, out string error))
                {
                    errors.Add($"{where}: {error}");
                    seen.Add(name); // already reported: no extra "missing" line for it
                }
            }
            foreach (var name in Names.Where(n => !seen.Contains(n)))
            {
                errors.Add($"personal_loot.csv: missing setting '{name}'.");
            }
            return settings;
        }
    }
}
