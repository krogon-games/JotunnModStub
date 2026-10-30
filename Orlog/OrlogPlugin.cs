using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using Orlog.Content;

namespace Orlog
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    // XP is sent between players' games, and later skills affect other players and creatures: everyone needs it.
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class OrlogPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.krogongames.orlog";
        public const string PluginName = "Orlog";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;
        private KrogonGames.Shared.ConfigWatcher _configWatcher;

        private void Awake()
        {
            OrlogConfig.Bind(Config);
            _configWatcher = new KrogonGames.Shared.ConfigWatcher(Config, Jotunn.Logger.LogInfo);
            Texts.Register();
            Safe.Run("read the levelling tables", Progress.LoadTables);
            Safe.Run("add the console commands", Commands.Register);

            _harmony = new Harmony(PluginGUID);
            PatchAllSafely();

            Jotunn.Logger.LogInfo($"{PluginName} {PluginVersion} loaded (build: {BuildStamp()})");
        }

        /// <summary>The git branch and commit this DLL was built from (stamped by Orlog.csproj).</summary>
        internal static string BuildStamp()
        {
            var stamp = typeof(OrlogPlugin).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
                .OfType<System.Reflection.AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "GitBuild");
            return stamp == null || string.IsNullOrWhiteSpace(stamp.Value) ? "unknown" : stamp.Value.Trim();
        }

        private void Update()
        {
            _configWatcher?.Tick();
        }

        private void OnDestroy()
        {
            _configWatcher?.Dispose();
            _harmony?.UnpatchSelf();
        }

        /// <summary>Applies each patch class on its own, so one broken by a game update only disables that feature.</summary>
        private void PatchAllSafely()
        {
            var patchClasses = typeof(OrlogPlugin).Assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), false).Any());

            foreach (var type in patchClasses)
            {
                try
                {
                    _harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    Jotunn.Logger.LogError($"Failed to apply patch {type.FullName}; that feature is disabled. {ex}");
                }
            }
        }
    }
}
