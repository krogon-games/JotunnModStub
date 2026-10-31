using Jotunn.Managers;

namespace Corruption.Content
{
    /// <summary>
    ///     Unlocks the items the game gates behind a Steam entitlement (<c>SharedData.m_dlc</c>; in 1.0.20 only "beta":
    ///     the Odin cape, helmet and tankard). The game checks the tag before equipping, listing a recipe and crafting
    ///     (<c>Humanoid.EquipItem</c>, <c>Player.GetAvailableRecipes</c>, <c>InventoryGui</c>), so clearing it makes the
    ///     items work like any other for everyone: equip, craft and loot (user, 2026-10-31: "unlock fully").
    /// </summary>
    internal static class BetaItems
    {
        public static void Unlock()
        {
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(ItemDrop)).Values)
            {
                var shared = (obj as ItemDrop)?.m_itemData?.m_shared;
                if (shared == null || string.IsNullOrEmpty(shared.m_dlc))
                {
                    continue;
                }
                Jotunn.Logger.LogInfo($"Unlocked {((ItemDrop)obj).name} (was tagged DLC \"{shared.m_dlc}\").");
                shared.m_dlc = "";
            }
        }
    }
}
