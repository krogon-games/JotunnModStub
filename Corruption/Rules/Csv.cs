using System.Collections.Generic;
using System.Text;

namespace Corruption.Rules
{
    /// <summary>Minimal CSV reader for the data tables in <c>Corruption/Data/</c>. No game dependencies.</summary>
    internal static class Csv
    {
        /// <summary>Splits CSV text into rows of cells. Handles quoted cells with commas, quotes and line breaks.</summary>
        public static List<List<string>> ReadRows(string text)
        {
            text = text.TrimStart('\uFEFF');
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cellText = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cellText.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        quoted = false;
                    }
                    else
                    {
                        cellText.Append(c);
                    }
                }
                else if (c == '"')
                {
                    quoted = true;
                }
                else if (c == ',')
                {
                    row.Add(cellText.ToString());
                    cellText.Clear();
                }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                    row.Add(cellText.ToString());
                    cellText.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    cellText.Append(c);
                }
            }
            if (cellText.Length > 0 || row.Count > 0)
            {
                row.Add(cellText.ToString());
                rows.Add(row);
            }
            return rows;
        }
    }
}
