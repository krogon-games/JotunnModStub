using System.Globalization;
using System.Text;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Writes the flat JSON objects and string arrays of the classes API (<c>docs/classes-ui-brief.md</c> §3), so the
    ///     demo answers exactly like the Orlog plugin and the UI reads both with <see cref="CharacterSheet.FlatJson" />.
    ///     Pure: no Unity.
    /// </summary>
    internal sealed class JsonOut
    {
        private readonly StringBuilder _sb = new StringBuilder("{");

        public JsonOut Add(string name, string value)
        {
            Key(name);
            Quote(_sb, value ?? "");
            return this;
        }

        public JsonOut Add(string name, long value)
        {
            Key(name);
            _sb.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public JsonOut Add(string name, float value)
        {
            Key(name);
            _sb.Append(value.ToString("0.###", CultureInfo.InvariantCulture));
            return this;
        }

        public JsonOut Add(string name, bool value)
        {
            Key(name);
            _sb.Append(value ? "true" : "false");
            return this;
        }

        public override string ToString()
        {
            return _sb.ToString() + "}";
        }

        public static string Array(System.Collections.Generic.IEnumerable<string> values)
        {
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (string value in values)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                first = false;
                Quote(sb, value ?? "");
            }
            return sb.Append(']').ToString();
        }

        private void Key(string name)
        {
            if (_sb.Length > 1)
            {
                _sb.Append(',');
            }
            Quote(_sb, name);
            _sb.Append(':');
        }

        private static void Quote(StringBuilder sb, string value)
        {
            sb.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
