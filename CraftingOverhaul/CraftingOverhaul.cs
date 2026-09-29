using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using CraftingOverhaul.Dump;
using Jotunn.Managers;
using Jotunn.Utils;

namespace CraftingOverhaul
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
    internal class CraftingOverhaul : BaseUnityPlugin
    {
        public const string PluginGUID = "com.krogongames.craftingoverhaul";
        public const string PluginName = "CraftingOverhaul";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        private void Awake()
        {
            ModConfig.Bind(Config);

            _harmony = new Harmony(PluginGUID);
            PatchAllSafely();

            CommandManager.Instance.AddConsoleCommand(new DumpCommand());
            CommandManager.Instance.AddConsoleCommand(new BiomeCommand());

            Jotunn.Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        /// <summary>
        ///     Applies each patch class on its own, so one patch broken by a game update
        ///     disables only that feature instead of the whole mod.
        /// </summary>
        private void PatchAllSafely()
        {
            var patchClasses = typeof(CraftingOverhaul).Assembly.GetTypes()
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
