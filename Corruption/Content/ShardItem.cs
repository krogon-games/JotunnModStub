using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>The corruption shard: the mod's currency. Drop-only, so it has no recipe.</summary>
    internal static class ShardItem
    {
        public static void Register()
        {
            // The look is configurable (base item, colour, tint strength), so it can be tuned without a rebuild.
            string basePrefab = CorruptionConfig.ShardBasePrefab.Value;
            if (!PrefabManager.Instance.GetPrefab(basePrefab))
            {
                Jotunn.Logger.LogWarning($"Shard base item '{basePrefab}' not found; using GemstoneRed.");
                basePrefab = "GemstoneRed";
            }

            var item = new CustomItem(Texts.ShardPrefab, basePrefab, new ItemConfig
            {
                Name = Texts.ShardName,
                Description = Texts.ShardDescription,
                StackSize = 50,
                Weight = 0.2f
            });

            if (ColorUtility.TryParseHtmlString(CorruptionConfig.ShardColor.Value, out var color))
            {
                Corrupted.Tint(item.ItemPrefab, color, CorruptionConfig.ShardTintStrength.Value);
            }
            else
            {
                Jotunn.Logger.LogWarning($"Shard colour '{CorruptionConfig.ShardColor.Value}' isn't a valid HTML colour; tint skipped.");
            }

            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_teleportable = true;
            var icon = Corrupted.RenderIcon(item.ItemPrefab);
            if (icon)
            {
                shared.m_icons = new[] { icon };
            }

            ItemManager.Instance.AddItem(item);
        }
    }
}
