using System.Globalization;
using System.Text;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>
    ///     Reads values out of the flat JSON objects and string arrays EquipmentAndQuickSlots' API returns
    ///     (<c>GetSlotInfoJson</c>, <c>GetSlotIdsJson</c>). Only what those need: one level, string, number and
    ///     boolean values, standard escapes. Pure and unit-tested; keeps a JSON library out of the plugin.
    /// </summary>
    internal static class FlatJson
    {
        /// <summary>The raw text of <paramref name="key" />'s value (unquoted and unescaped for strings), or null.</summary>
        public static string Get(string json, string key)
        {
            if (json == null || key == null)
            {
                return null;
            }
            int pos = 0;
            while (pos < json.Length)
            {
                int quote = json.IndexOf('"', pos);
                if (quote < 0)
                {
                    return null;
                }
                int end;
                string name = ReadString(json, quote, out end);
                if (name == null)
                {
                    return null;
                }
                int colon = SkipSpaces(json, end);
                if (colon < json.Length && json[colon] == ':')
                {
                    int valueStart = SkipSpaces(json, colon + 1);
                    int valueEnd;
                    string value = ReadValue(json, valueStart, out valueEnd);
                    if (name == key)
                    {
                        return value;
                    }
                    pos = valueEnd;
                }
                else
                {
                    pos = end;
                }
            }
            return null;
        }

        public static bool GetBool(string json, string key, bool fallback)
        {
            string value = Get(json, key);
            if (value == "true") return true;
            if (value == "false") return false;
            return fallback;
        }

        public static int GetInt(string json, string key, int fallback)
        {
            string value = Get(json, key);
            int result;
            return value != null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : fallback;
        }

        /// <summary>The strings of a JSON array of strings, e.g. <c>["Helmet","Chest"]</c>. Anything else gives an empty array.</summary>
        public static string[] Strings(string json)
        {
            var result = new System.Collections.Generic.List<string>();
            if (json == null)
            {
                return result.ToArray();
            }
            int pos = json.IndexOf('[');
            if (pos < 0)
            {
                return result.ToArray();
            }
            pos++;
            while (pos < json.Length)
            {
                pos = SkipSpaces(json, pos);
                if (pos >= json.Length || json[pos] == ']')
                {
                    break;
                }
                if (json[pos] == ',')
                {
                    pos++;
                    continue;
                }
                if (json[pos] != '"')
                {
                    break;
                }
                int end;
                string s = ReadString(json, pos, out end);
                if (s == null)
                {
                    break;
                }
                result.Add(s);
                pos = end;
            }
            return result.ToArray();
        }

        private static int SkipSpaces(string json, int pos)
        {
            while (pos < json.Length && char.IsWhiteSpace(json[pos]))
            {
                pos++;
            }
            return pos;
        }

        private static string ReadValue(string json, int pos, out int end)
        {
            if (pos < json.Length && json[pos] == '"')
            {
                return ReadString(json, pos, out end);
            }
            int start = pos;
            while (pos < json.Length && json[pos] != ',' && json[pos] != '}' && !char.IsWhiteSpace(json[pos]))
            {
                pos++;
            }
            end = pos;
            return json.Substring(start, pos - start);
        }

        /// <summary>Reads the quoted string starting at <paramref name="quote" />; <paramref name="end" /> is just past its closing quote.</summary>
        private static string ReadString(string json, int quote, out int end)
        {
            var sb = new StringBuilder();
            int pos = quote + 1;
            while (pos < json.Length)
            {
                char c = json[pos];
                if (c == '"')
                {
                    end = pos + 1;
                    return sb.ToString();
                }
                if (c == '\\' && pos + 1 < json.Length)
                {
                    pos++;
                    char e = json[pos];
                    switch (e)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'u':
                            if (pos + 4 < json.Length)
                            {
                                int code;
                                if (int.TryParse(json.Substring(pos + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                                {
                                    sb.Append((char)code);
                                    pos += 4;
                                }
                            }
                            break;
                        default: sb.Append(e); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
                pos++;
            }
            end = json.Length;
            return null;
        }
    }
}
