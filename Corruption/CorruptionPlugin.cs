using System;
using System.Linq;
using BepInEx;
using Corruption.Content;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Corruption
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    // Adds items and pieces, so the server and every player must run the same version.
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CorruptionPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.krogongames.corruption";
        public const string PluginName = "Corruption";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        private void Awake()
        {
            CorruptionConfig.Bind(Config);
            Texts.Register();
            PrefabManager.OnVanillaPrefabsAvailable += RegisterContent;

            _harmony = new Harmony(PluginGUID);
            PatchAllSafely();

            Jotunn.Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        /// <summary>Items and pieces are cloned from vanilla prefabs, which only exist once the game has loaded them.</summary>
        private static void RegisterContent()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterContent;
            Safe.Run("register the corruption shard", ShardItem.Register);
            Safe.Run("register the Riftforge", Riftforge.Register);
            Safe.Run("register talismans", Talismans.Register);
        }

        /// <summary>Applies each patch class on its own, so one broken by a game update only disables that feature.</summary>
        private void PatchAllSafely()
        {
            var patchClasses = typeof(CorruptionPlugin).Assembly.GetTypes()
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
