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

        /// <summary>
        ///     The <see cref="Run" /> section the main thread is inside right now (null: none), and the last one it finished.
        ///     Read by Corruption's freeze watch (<c>Corruption.FreezeWatch</c>, by reflection) from its own thread, so a log
        ///     line can name the mod code a frozen game was running (Krogon, 2026-11-28).
        /// </summary>
        public static volatile string Current;
        public static volatile string LastFinished;

        public static void Run(string action, Action body)
        {
            string outer = Current;
            Current = action;
            try
            {
                body();
            }
            catch (Exception ex)
            {
                Report(action, ex);
            }
            finally
            {
                Current = outer;
                LastFinished = action;
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
