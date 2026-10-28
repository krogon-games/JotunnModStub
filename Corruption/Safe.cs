using System;
using System.Collections.Generic;

namespace Corruption
{
    /// <summary>
    ///     Runs mod code inside game methods so an error only disables that bit of the mod: it is logged (once per
    ///     action) and the game carries on as vanilla.
    /// </summary>
    internal static class Safe
    {
        private static readonly HashSet<string> Reported = new HashSet<string>();

        public static void Run(string action, Action body)
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                Report(action, ex);
            }
        }

        public static void Report(string action, Exception ex)
        {
            if (Reported.Add(action))
            {
                Jotunn.Logger.LogError($"Corruption could not {action}; using vanilla behaviour instead. {ex}");
            }
        }
    }
}
