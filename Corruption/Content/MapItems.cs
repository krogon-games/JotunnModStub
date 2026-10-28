using System.Collections.Generic;
using Corruption.Rules;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Maps: one item per row of <c>Corruption/Data/maps.csv</c>, crafted at the Riftforge from shards, the biome
    ///     boss's essence and a biome material. Using one opens a rip (<see cref="Rips" />). Register after essences.
    /// </summary>
    internal static class MapItems
    {
        // Placeholder look until the user asks for map art: a bundle of leather, tinted with the essence's colour.
        private const string BasePrefab = "LeatherScraps";

        /// <summary>Registered maps by item prefab name.</summary>
        public static readonly Dictionary<string, MapList.Map> ByItem = new Dictionary<string, MapList.Map>();

        public static void Register()
        {
            var errors = new List<string>();
            var maps = MapList.Parse(DataFiles.Read("maps.csv"), errors);
            foreach (var error in errors)
            {
                Jotunn.Logger.LogWarning(error);
            }

            var localization = LocalizationManager.Instance.GetLocalization();
            foreach (var map in maps)
            {
                Safe.Run($"register {map.Item}", () => RegisterOne(map, localization));
            }
        }

        /// <summary>The map table row for an item, or null if it isn't a map.</summary>
        public static MapList.Map Find(ItemDrop.ItemData item)
        {
            var prefab = item?.m_dropPrefab;
            return prefab && ByItem.TryGetValue(prefab.name, out var map) ? map : null;
        }

        private static void RegisterOne(MapList.Map map, CustomLocalization localization)
        {
            string token = "item_" + map.Item.ToLowerInvariant();
            localization.AddTranslation("English", new Dictionary<string, string>
            {
                { token, map.Name },
                { token + "_description", map.Description }
            });

            var config = new ItemConfig
            {
                Name = "$" + token,
                Description = "$" + token + "_description",
                StackSize = 1,
                Weight = 0.5f,
                CraftingStation = Texts.RiftforgePrefab,
                Amount = 1
            };
            config.AddRequirement(Texts.ShardPrefab, map.Shards);
            config.AddRequirement(map.EssenceItem, 1);
            config.AddRequirement(map.Material, map.MaterialAmount);

            var item = new CustomItem(map.Item, BasePrefab, config);
            var color = new Color(0.7f, 0.09f, 0.05f);
            if (EssenceItems.ByLook.TryGetValue(map.Essence, out var essence))
            {
                ColorUtility.TryParseHtmlString(essence.Glow, out color);
            }
            else
            {
                Jotunn.Logger.LogWarning($"maps.csv: {map.Item} uses essence '{map.Essence}', which isn't in essences.csv.");
            }
            Corrupted.Tint(item.ItemPrefab, color, 0.6f);

            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_subtitle = Texts.MapSubtitle;
            shared.m_teleportable = true;
            shared.m_value = 0;
            Safe.Run($"render the {map.Item} icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                }
            });

            ItemManager.Instance.AddItem(item);
            ByItem[map.Item] = map;
        }
    }
}
