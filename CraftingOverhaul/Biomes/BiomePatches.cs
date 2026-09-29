using HarmonyLib;

namespace CraftingOverhaul.Biomes
{
    internal static class BiomePatches
    {
        /// <summary>
        ///     Resolving loads every location and dungeon room once, which can take a moment. Do it right after
        ///     spawning (when biome grouping is on) instead of the first time a crafting station is opened.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        private static class OnSpawned_Patch
        {
            private static void Postfix(Player __instance)
            {
                if (__instance == Player.m_localPlayer && ModConfig.Grouping.Value == GroupingMode.Biome)
                {
                    BiomeResolver.EnsureResolved();
                }
            }
        }
    }
}
