using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Corruption
{
    /// <summary>
    ///     Freeze watch (Krogon, 2026-11-28, after a play-test freeze that no log explained): the main thread ticks
    ///     <see cref="Beat" /> every frame; a background thread checks once a second and, when the game has been in a world
    ///     and the frames stop for <see cref="StallSeconds" />, writes one report naming the mod code the main thread is inside
    ///     (each plugin's <c>Safe.Current</c>, read by reflection) and the last one it finished. The report goes to the
    ///     BepInEx log and, in case the log isn't written out before Windows closes the game, to
    ///     <c>BepInEx/corruption_freeze.txt</c>. Reads only; changes nothing in the game.
    /// </summary>
    internal static class FreezeWatch
    {
        private const double StallSeconds = 10.0;

        private static long s_lastBeat;
        private static volatile bool s_inWorld;
        private static Thread s_thread;
        private static FieldInfo[] s_current;
        private static FieldInfo[] s_finished;
        private static string[] s_names;

        /// <summary>Called every frame on the main thread.</summary>
        public static void Beat(bool inWorld)
        {
            Interlocked.Exchange(ref s_lastBeat, Stopwatch.GetTimestamp());
            s_inWorld = inWorld;
        }

        public static void Start()
        {
            if (s_thread != null)
            {
                return;
            }
            var plugins = new[] { "Corruption", "Orlog", "CraftingOverhaul" };
            s_names = plugins;
            s_current = new FieldInfo[plugins.Length];
            s_finished = new FieldInfo[plugins.Length];
            Beat(false);
            s_thread = new Thread(Watch) { IsBackground = true, Name = "Corruption freeze watch", Priority = ThreadPriority.BelowNormal };
            s_thread.Start();
        }

        /// <summary>Finds each plugin's Safe class once it is loaded (Orlog and CraftingOverhaul may load after Corruption).</summary>
        private static void FindFields()
        {
            for (int i = 0; i < s_names.Length; i++)
            {
                if (s_current[i] != null)
                {
                    continue;
                }
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name != s_names[i])
                    {
                        continue;
                    }
                    var safe = assembly.GetType(s_names[i] + ".Safe");
                    if (safe != null)
                    {
                        s_current[i] = safe.GetField("Current", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        s_finished[i] = safe.GetField("LastFinished", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                }
            }
        }

        private static void Watch()
        {
            bool reported = false;
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    double stalled = (Stopwatch.GetTimestamp() - Interlocked.Read(ref s_lastBeat)) / (double)Stopwatch.Frequency;
                    if (stalled < StallSeconds)
                    {
                        reported = false;
                        continue;
                    }
                    if (reported || !s_inWorld)
                    {
                        continue;
                    }
                    reported = true;
                    FindFields();
                    string report = $"FREEZE WATCH: the game has not drawn a frame for {stalled:0} s ({DateTime.Now:yyyy-MM-dd HH:mm:ss}). ";
                    string inside = "";
                    string finished = "";
                    for (int i = 0; i < s_names.Length; i++)
                    {
                        string current = s_current[i]?.GetValue(null) as string;
                        string last = s_finished[i]?.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(current))
                        {
                            inside += $" {s_names[i]}: \"{current}\";";
                        }
                        finished += $" {s_names[i]}: \"{last ?? "none"}\";";
                    }
                    report += inside.Length > 0
                        ? "The main thread is inside our mods' code:" + inside
                        : "The main thread is not inside any of our mods' marked code (so likely the game itself, another mod or the graphics driver).";
                    report += " Last finished:" + finished;
                    try
                    {
                        File.AppendAllText(Path.Combine(BepInEx.Paths.BepInExRootPath, "corruption_freeze.txt"), report + Environment.NewLine);
                    }
                    catch
                    {
                        // The log line below still goes out.
                    }
                    Jotunn.Logger.LogWarning(report);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch
                {
                    // Never let the watch itself break anything.
                }
            }
        }
    }
}
