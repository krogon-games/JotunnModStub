using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace CraftingOverhaul.Containers
{
    /// <summary>
    ///     Makes nearby containers count as part of the player's inventory, but only while the game is checking
    ///     or paying crafting/building costs. Everywhere else (weight, death, trading...) the inventory is untouched.
    ///
    ///     Every cost check in the game goes through <see cref="Inventory.CountItems" /> and every payment through
    ///     <see cref="Inventory.RemoveItem(string,int,int,bool)" /> on the player's inventory, so patching those two
    ///     inside the crafting/building code paths covers the UI counts, "can craft" checks and payment.
    /// </summary>
    internal static class ContainerPatches
    {
        private static int _craftingDepth;
        private static int _buildingDepth;
        private static int _suppressDepth;
        private static bool _bypass;

        private static bool IsActiveFor(Inventory inventory)
        {
            if (_bypass || _suppressDepth > 0 || !ModConfig.ContainersEnabled.Value)
            {
                return false;
            }
            if (_craftingDepth <= 0 && (_buildingDepth <= 0 || !ModConfig.ContainersForBuilding.Value))
            {
                return false;
            }
            var player = Player.m_localPlayer;
            return player && inventory == player.GetInventory();
        }

        [HarmonyPatch]
        private static class CraftingScope
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.UpdateCraftingPanel));
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe));
                yield return AccessTools.Method(typeof(InventoryGui), nameof(InventoryGui.DoCrafting));
            }

            private static void Prefix() => _craftingDepth++;
            private static void Finalizer() => _craftingDepth--;
        }

        [HarmonyPatch]
        private static class BuildingScope
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Player), nameof(Player.UpdatePlacement));
                yield return AccessTools.Method(typeof(Hud), nameof(Hud.UpdatePieceBuildStatus));
                yield return AccessTools.Method(typeof(Hud), nameof(Hud.UpdatePieceBuildStatusAll));
                yield return AccessTools.Method(typeof(Hud), nameof(Hud.SetupPieceInfo));
            }

            private static void Prefix() => _buildingDepth++;
            private static void Finalizer() => _buildingDepth--;
        }

        /// <summary>
        ///     "Any one of these ingredients" recipes pick the actual item from the player's inventory,
        ///     so don't advertise container stock for them.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirementItems))]
        private static class OneIngredientRecipes
        {
            private static void Prefix(Recipe __0, out bool __state)
            {
                __state = __0 && __0.m_requireOnlyOneIngredient;
                if (__state)
                {
                    _suppressDepth++;
                }
            }

            private static void Finalizer(bool __state)
            {
                if (__state)
                {
                    _suppressDepth--;
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.CountItems), typeof(string), typeof(int), typeof(bool))]
        private static class CountItems_Patch
        {
            private static void Postfix(Inventory __instance, string __0, int __1, bool __2, ref int __result)
            {
                if (__0 != null && IsActiveFor(__instance))
                {
                    try
                    {
                        __result += NearbyContainers.Count(__0, __1, __2);
                    }
                    catch (System.Exception ex)
                    {
                        Safe.Report("count items in nearby containers", ex);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(string), typeof(int), typeof(int), typeof(bool))]
        private static class RemoveItem_Patch
        {
            /// <summary>
            ///     Take from the player's own inventory first, and the shortfall from nearby containers.
            ///     Runs after other mods' prefixes (low priority). If one of them already paid the whole cost and
            ///     skipped the original (e.g. AdventureBackpacks' craft-from-backpack), take nothing, or the player
            ///     would pay twice.
            /// </summary>
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Inventory __instance, string __0, ref int __1, int __2, bool __3, bool __runOriginal)
            {
                if (!__runOriginal || !IsActiveFor(__instance))
                {
                    return;
                }

                int carried;
                _bypass = true;
                try
                {
                    carried = __instance.CountItems(__0, __2, __3);
                }
                finally
                {
                    _bypass = false;
                }

                int missing = __1 - carried;
                if (missing > 0)
                {
                    try
                    {
                        __1 -= NearbyContainers.Remove(__0, missing, __2, __3);
                    }
                    catch (System.Exception ex)
                    {
                        Safe.Report("take items from nearby containers", ex);
                    }
                }
            }
        }
    }
}
