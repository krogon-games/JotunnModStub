using System.IO;
using System.Reflection;

namespace Corruption.Content
{
    /// <summary>The data tables compiled into the DLL (<c>Corruption/Data/*.csv</c>, see <c>Corruption.csproj</c>).</summary>
    internal static class DataFiles
    {
        private const string ResourcePrefix = "Corruption.Data.";

        /// <summary>The file's text, or "" (logged) if it isn't embedded.</summary>
        public static string Read(string file)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + file))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogError($"Embedded data table {file} not found.");
                    return "";
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
