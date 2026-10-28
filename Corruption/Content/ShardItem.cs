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

            if (!ColorUtility.TryParseHtmlString(CorruptionConfig.ShardColor.Value, out var color))
            {
                Jotunn.Logger.LogWarning($"Shard colour '{CorruptionConfig.ShardColor.Value}' isn't a valid HTML colour; using the default red.");
                color = new Color(0.7f, 0.09f, 0.05f);
            }

            // Our own crystal model and texture; the base item only provides the item, physics and networking parts.
            // Falls back to the tinted base item if the custom look is off or fails: a broken look must never stop
            // the shard from being registered (an unregistered item vanishes from every inventory and chest).
            bool custom = false;
            if (CorruptionConfig.ShardCustomModel.Value)
            {
                try
                {
                    custom = GemModel.Apply(item.ItemPrefab, GemModel.Shard, color);
                }
                catch (System.Exception ex)
                {
                    Jotunn.Logger.LogError($"Custom shard model failed; using the tinted base item instead. {ex}");
                }
            }
            if (!custom)
            {
                Corrupted.Tint(item.ItemPrefab, color, CorruptionConfig.ShardTintStrength.Value);
            }

            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_teleportable = true;
            Safe.Run("render the shard icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                }
            });
            // After the icon render, so the light doesn't colour the icon.
            Safe.Run("add the shard glow", () => GemModel.AddGlow(item.ItemPrefab, color));

            ItemManager.Instance.AddItem(item);
        }
    }
}
