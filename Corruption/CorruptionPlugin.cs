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
    // Talisman slots live in EquipmentAndQuickSlots' equipment panel; load after it when it's installed.
    [BepInDependency(TalismanSlots.EaqsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    // Adds items and pieces, so the server and every player must run the same version.
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CorruptionPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.krogongames.corruption";
        public const string PluginName = "Corruption";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;
        private KrogonGames.Shared.ConfigWatcher _configWatcher;

        private void Awake()
        {
            CorruptionConfig.Bind(Config);
            _configWatcher = new KrogonGames.Shared.ConfigWatcher(Config, Jotunn.Logger.LogInfo);
            Texts.Register();
            PrefabManager.OnVanillaPrefabsAvailable += RegisterContent;

            _harmony = new Harmony(PluginGUID);
            PatchAllSafely();
            Safe.Run("add talisman slots", TalismanSlots.Register);
            Safe.Run("add the co_corrupt command", CorruptedBosses.Register);
            Safe.Run("read the loot tables", Loot.Load);
            Safe.Run("add the co_loot command", Loot.Register);

            Jotunn.Logger.LogInfo($"{PluginName} {PluginVersion} loaded (build: {BuildStamp()})");
        }

        /// <summary>The git branch and commit this DLL was built from (stamped by Corruption.csproj).</summary>
        internal static string BuildStamp()
        {
            var stamp = typeof(CorruptionPlugin).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), false)
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

        /// <summary>Items and pieces are cloned from vanilla prefabs, which only exist once the game has loaded them.</summary>
        private static void RegisterContent()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterContent;
            Safe.Run("register the corruption shard", ShardItem.Register);
            Safe.Run("register the Riftforge", Riftforge.Register);
            Safe.Run("register talismans", Talismans.Register);
            Safe.Run("register boss essences", EssenceItems.Register);
            // After essences: map recipes and colours use them.
            Safe.Run("register maps", MapItems.Register);
            Safe.Run("register rips", RipPrefabs.Register);
            Safe.Run("register the corrupted wishbone", CorruptedWishbone.Register);
            Safe.Run("register corrupted offerings", CorruptedOfferings.Register);
            Safe.Run("register the maddened effect", Maddened.RegisterEffect);
            Safe.Run("read the rip loot table", RipLoot.Load);
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
