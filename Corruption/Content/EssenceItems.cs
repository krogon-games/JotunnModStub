using System.Collections.Generic;
using Corruption.Rules;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Boss essences: one item per row of <c>Corruption/Data/essences.csv</c>. Each uses the shared essence gem
    ///     model with the boss's own texture, glow colour and optional add-on part. Drop-only; used for maps later.
    /// </summary>
    internal static class EssenceItems
    {
        /// <summary>Essences that were registered, by boss prefab name. Read by <see cref="EssenceDrops" />.</summary>
        public static readonly Dictionary<string, EssenceTable.Essence> ByBoss = new Dictionary<string, EssenceTable.Essence>();

        /// <summary>Every essence in the table, by look id (registered or not). Read by <see cref="MapItems" />.</summary>
        public static readonly Dictionary<string, EssenceTable.Essence> ByLook = new Dictionary<string, EssenceTable.Essence>();

        public static void Register()
        {
            var errors = new List<string>();
            var essences = EssenceTable.Parse(DataFiles.Read("essences.csv"), errors);
            foreach (var error in errors)
            {
                Jotunn.Logger.LogWarning(error);
            }

            var localization = LocalizationManager.Instance.GetLocalization();
            foreach (var essence in essences)
            {
                ByLook[essence.Look] = essence;
                Safe.Run($"register {essence.Item}", () => RegisterOne(essence, localization));
            }
        }

        private static void RegisterOne(EssenceTable.Essence essence, CustomLocalization localization)
        {
            if (!PrefabManager.Instance.GetPrefab(essence.Boss))
            {
                Jotunn.Logger.LogWarning($"essences.csv: boss '{essence.Boss}' isn't in this game version; {essence.Item} is still registered but never drops.");
            }

            string token = "item_" + essence.Item.ToLowerInvariant();
            localization.AddTranslation("English", new Dictionary<string, string>
            {
                { token, essence.Name },
                { token + "_description", essence.Description }
            });

            var item = new CustomItem(essence.Item, "GemstoneRed", new ItemConfig
            {
                Name = "$" + token,
                Description = "$" + token + "_description",
                StackSize = 20,
                Weight = 0.5f
            });

            ColorUtility.TryParseHtmlString(essence.Glow, out var glow);
            Color addonColor = Color.white;
            if (essence.Addon.Length > 0)
            {
                ColorUtility.TryParseHtmlString(essence.AddonColor, out addonColor);
            }
            var look = new GemModel.Look
            {
                Name = essence.Item,
                Mesh = "essence.obj",
                Albedo = $"essence_{essence.Look}_albedo.rgba",
                Emission = $"essence_{essence.Look}_emission.rgba",
                AddonMesh = essence.Addon.Length > 0 ? essence.Addon + ".obj" : null,
                AddonColor = addonColor
            };

            // A broken look must never stop the item from registering (unregistered items vanish from inventories).
            bool custom = false;
            try
            {
                custom = GemModel.Apply(item.ItemPrefab, look, glow);
            }
            catch (System.Exception ex)
            {
                Jotunn.Logger.LogError($"Custom model for {essence.Item} failed; using a tinted gem instead. {ex}");
            }
            if (!custom)
            {
                Corrupted.Tint(item.ItemPrefab, glow, 0.6f);
            }

            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_teleportable = true;
            // Essences can't be sold (design balance rule).
            shared.m_value = 0;
            Safe.Run($"render the {essence.Item} icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                }
            });
            // After the icon render, so the light doesn't colour the icon.
            Safe.Run($"add the {essence.Item} glow", () => GemModel.AddGlow(item.ItemPrefab, glow));

            ItemManager.Instance.AddItem(item);
            ByBoss[essence.Boss] = essence;
        }
    }
}
