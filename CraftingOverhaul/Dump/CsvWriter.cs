using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CraftingOverhaul.Dump
{
    /// <summary>Minimal RFC 4180 CSV writer (quotes fields containing commas, quotes or newlines).</summary>
    internal sealed class CsvWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        public CsvWriter(string path, params string[] header)
        {
            _writer = new StreamWriter(path, false, new UTF8Encoding(false));
            Row(header);
        }

        public void Row(params object[] fields)
        {
            _writer.WriteLine(string.Join(",", fields.Select(Escape)));
        }

        public static string Join(IEnumerable<string> values)
        {
            return string.Join("; ", values.Where(v => !string.IsNullOrEmpty(v)));
        }

        private static string Escape(object value)
        {
            string s;
            switch (value)
            {
                case null: s = ""; break;
                case float f: s = f.ToString("0.###", CultureInfo.InvariantCulture); break;
                case double d: s = d.ToString("0.###", CultureInfo.InvariantCulture); break;
                case IFormattable formattable: s = formattable.ToString(null, CultureInfo.InvariantCulture); break;
                default: s = value.ToString(); break;
            }
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            {
                s = "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
