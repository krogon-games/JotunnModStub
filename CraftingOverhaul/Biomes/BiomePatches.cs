using HarmonyLib;

namespace CraftingOverhaul.Biomes
{
    internal static class BiomePatches
    {
        /// <summary>
        ///     Resolving loads every location and dungeon room once, which takes a while. Start it in the background
        ///     right after spawning (when biome grouping is on), so it is usually ready before a crafting station is
        ///     opened; it does not freeze the game.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        private static class OnSpawned_Patch
        {
            private static void Postfix(Player __instance)
            {
                if (__instance == Player.m_localPlayer && ModConfig.Grouping.Value == GroupingMode.Biome)
                {
                    Safe.Run("work out item biomes", BiomeResolver.StartResolving);
                }
            }
        }
    }
}
