using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Corruption.Content
{
    /// <summary>The corruption shard: the mod's currency. Drop-only, so it has no recipe.</summary>
    internal static class ShardItem
    {
        // Mountain crystal: already a jagged shard shape.
        private const string BasePrefab = "Crystal";

        public static void Register()
        {
            var item = new CustomItem(Texts.ShardPrefab, BasePrefab, new ItemConfig
            {
                Name = Texts.ShardName,
                Description = Texts.ShardDescription,
                StackSize = 50,
                Weight = 0.2f
            });

            Corrupted.Tint(item.ItemPrefab, 0.75f);

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
