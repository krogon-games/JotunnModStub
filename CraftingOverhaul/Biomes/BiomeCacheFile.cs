using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CraftingOverhaul.Biomes
{
    /// <summary>
    ///     The solved item and global-key biomes saved to disk, so later world loads can skip the slow collection.
    ///     The file starts with a key: named parts (game version, loaded mods, overrides, a fingerprint of the game
    ///     data...), each a hash. A file is only used when every part matches; otherwise the biomes are worked out
    ///     again and the file is rewritten. Each resolution keeps its winning source, so <c>co_biome</c> can still
    ///     explain an item. Free of game dependencies, so it is tested on its own.
    /// </summary>
    internal static class BiomeCacheFile
    {
        /// <summary>Raise when the file layout or the resolution rules change, so old files are not used.</summary>
        public const int FormatVersion = 1;

        private const string Header = "CraftingOverhaul biome cache";
        private const char Tab = '\t';
        private const char ListSeparator = '|';

        /// <summary>A stable 64-bit FNV-1a hash of the values, in order, as 16 hex digits.</summary>
        public static string Hash(IEnumerable<string> values)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            foreach (var value in values)
            {
                foreach (char c in value ?? "")
                {
                    hash = (hash ^ (byte)c) * prime;
                    hash = (hash ^ (byte)(c >> 8)) * prime;
                }
                // Separator, so ["ab", "c"] and ["a", "bc"] differ.
                hash = (hash ^ 0xFF) * prime;
            }
            return hash.ToString("x16", CultureInfo.InvariantCulture);
        }

        public static string Write(IList<KeyValuePair<string, string>> key,
            IDictionary<string, BiomeResolution> items, IDictionary<string, BiomeResolution> keys)
        {
            var sb = new StringBuilder();
            sb.Append(Header).Append(' ').Append(FormatVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (var part in key)
            {
                sb.Append("key").Append(Tab).Append(Escape(part.Key)).Append(Tab).Append(Escape(part.Value)).Append('\n');
            }
            int count = 0;
            foreach (var entry in items)
            {
                WriteEntry(sb, "item", entry.Key, entry.Value);
                count++;
            }
            foreach (var entry in keys)
            {
                WriteEntry(sb, "globalkey", entry.Key, entry.Value);
                count++;
            }
            // A truncated file (game closed mid-write) has no end line and is ignored.
            sb.Append("end").Append(Tab).Append(count.ToString(CultureInfo.InvariantCulture)).Append('\n');
            return sb.ToString();
        }

        /// <summary>
        ///     Reads a saved file if its key matches. On failure <paramref name="reason" /> says why: the key parts
        ///     that changed (e.g. "game, mods"), or that the file is unreadable.
        /// </summary>
        public static bool TryRead(string text, IList<KeyValuePair<string, string>> key,
            out Dictionary<string, BiomeResolution> items, out Dictionary<string, BiomeResolution> keys, out string reason)
        {
            items = null;
            keys = null;
            if (string.IsNullOrEmpty(text))
            {
                reason = "no saved file";
                return false;
            }

            var lines = text.Split('\n');
            if (lines[0].Trim() != Header + " " + FormatVersion.ToString(CultureInfo.InvariantCulture))
            {
                reason = "file from another format";
                return false;
            }

            var saved = new Dictionary<string, string>();
            var readItems = new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
            var readKeys = new Dictionary<string, BiomeResolution>(StringComparer.OrdinalIgnoreCase);
            int entries = 0;
            bool ended = false;

            for (int i = 1; i < lines.Length && !ended; i++)
            {
                string line = lines[i].TrimEnd('\r');
                if (line.Length == 0)
                {
                    continue;
                }
                var f = line.Split(Tab);
                switch (f[0])
                {
                    case "key" when f.Length == 3:
                        saved[Unescape(f[1])] = Unescape(f[2]);
                        break;
                    case "item":
                    case "globalkey":
                        if (!TryReadEntry(f, out string name, out var resolution))
                        {
                            reason = "unreadable line " + (i + 1);
                            return false;
                        }
                        (f[0] == "item" ? readItems : readKeys)[name] = resolution;
                        entries++;
                        break;
                    case "end" when f.Length == 2:
                        if (!int.TryParse(f[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int expected) || expected != entries)
                        {
                            reason = "incomplete file";
                            return false;
                        }
                        ended = true;
                        break;
                    default:
                        reason = "unreadable line " + (i + 1);
                        return false;
                }
            }
            if (!ended)
            {
                reason = "incomplete file";
                return false;
            }

            var changed = new List<string>();
            foreach (var part in key)
            {
                if (!saved.TryGetValue(part.Key, out string value) || value != part.Value)
                {
                    changed.Add(part.Key);
                }
            }
            foreach (var name in saved.Keys)
            {
                if (!Contains(key, name))
                {
                    changed.Add(name);
                }
            }
            if (changed.Count > 0)
            {
                reason = "changed: " + string.Join(", ", changed.ToArray());
                return false;
            }

            items = readItems;
            keys = readKeys;
            reason = null;
            return true;
        }

        private static bool Contains(IList<KeyValuePair<string, string>> key, string name)
        {
            foreach (var part in key)
            {
                if (part.Key == name)
                {
                    return true;
                }
            }
            return false;
        }

        // Fields: kind, name, biome, override, then the source (empty for overrides): target, name, collector,
        // origin, base biome, tool tier, tool kind, required key, weak, required items.
        private static void WriteEntry(StringBuilder sb, string kind, string name, BiomeResolution r)
        {
            sb.Append(kind).Append(Tab).Append(Escape(name)).Append(Tab).Append(r.Biome.ToString()).Append(Tab)
                .Append(r.IsOverride ? '1' : '0');
            var s = r.Source;
            if (s != null)
            {
                var required = new StringBuilder();
                for (int i = 0; i < s.RequiredItems.Count; i++)
                {
                    if (i > 0)
                    {
                        required.Append(ListSeparator);
                    }
                    required.Append(Escape(s.RequiredItems[i]));
                }
                sb.Append(Tab).Append(s.Target.ToString())
                    .Append(Tab).Append(Escape(s.Name))
                    .Append(Tab).Append(Escape(s.Collector))
                    .Append(Tab).Append(Escape(s.Origin))
                    .Append(Tab).Append(s.Base.HasValue ? s.Base.Value.ToString() : "")
                    .Append(Tab).Append(s.Tool.Tier.ToString(CultureInfo.InvariantCulture))
                    .Append(Tab).Append(s.Tool.Kind.ToString())
                    .Append(Tab).Append(Escape(s.RequiredKey))
                    .Append(Tab).Append(s.IsWeak ? '1' : '0')
                    .Append(Tab).Append(required);
            }
            sb.Append('\n');
        }

        private static bool TryReadEntry(string[] f, out string name, out BiomeResolution resolution)
        {
            name = null;
            resolution = null;
            if ((f.Length != 4 && f.Length != 14) || !TryParseEnum(f[2], out Biome biome))
            {
                return false;
            }
            name = Unescape(f[1]);
            resolution = new BiomeResolution { Biome = biome, IsOverride = f[3] == "1" };
            if (f.Length == 4)
            {
                return resolution.IsOverride;
            }

            if (!TryParseEnum(f[4], out SourceTarget target) ||
                !int.TryParse(f[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tier) ||
                !TryParseEnum(f[10], out ToolKind kind))
            {
                return false;
            }
            Biome? baseBiome = null;
            if (f[8].Length > 0)
            {
                if (!TryParseEnum(f[8], out Biome b))
                {
                    return false;
                }
                baseBiome = b;
            }

            var source = new ItemSource
            {
                Target = target,
                Name = NullIfEmpty(Unescape(f[5])),
                Collector = NullIfEmpty(Unescape(f[6])),
                Origin = NullIfEmpty(Unescape(f[7])),
                Base = baseBiome,
                Tool = new ToolRequirement(tier, kind),
                RequiredKey = NullIfEmpty(Unescape(f[11])),
                IsWeak = f[12] == "1"
            };
            if (f[13].Length > 0)
            {
                foreach (var item in f[13].Split(ListSeparator))
                {
                    source.RequiredItems.Add(Unescape(item));
                }
            }
            resolution.Source = source;
            return true;
        }

        private static bool TryParseEnum<T>(string text, out T value) where T : struct
        {
            // Names only: a bare number would parse too, but would mean something else after an enum change.
            if (text.Length == 0 || char.IsDigit(text[0]) || text[0] == '-')
            {
                value = default(T);
                return false;
            }
            return Enum.TryParse(text, false, out value) && Enum.IsDefined(typeof(T), value);
        }

        private static string NullIfEmpty(string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }

        internal static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case ListSeparator: sb.Append("\\p"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        internal static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0)
            {
                return s;
            }
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != '\\' || i + 1 >= s.Length)
                {
                    sb.Append(c);
                    continue;
                }
                char next = s[++i];
                switch (next)
                {
                    case 't': sb.Append('\t'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'p': sb.Append(ListSeparator); break;
                    default: sb.Append(next); break;
                }
            }
            return sb.ToString();
        }
    }
}
