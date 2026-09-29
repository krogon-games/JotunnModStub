using System;
using System.Collections.Generic;
using CraftingOverhaul.Biomes;

namespace CraftingOverhaul.RecipeList
{
    internal sealed class RecipeCategory
    {
        public readonly string Key;
        public readonly string Label;
        public readonly int Order;

        public RecipeCategory(string key, string label, int order)
        {
            Key = key;
            Label = label;
            Order = order;
        }
    }

    /// <summary>Assigns recipes to a category for the current grouping mode.</summary>
    internal static class RecipeCategories
    {
        // Item type categories
        private static readonly RecipeCategory Melee = new RecipeCategory("type.melee", "Melee Weapons", 0);
        private static readonly RecipeCategory Ranged = new RecipeCategory("type.ranged", "Bows & Crossbows", 1);
        private static readonly RecipeCategory Magic = new RecipeCategory("type.magic", "Magic", 2);
        private static readonly RecipeCategory Ammo = new RecipeCategory("type.ammo", "Ammo", 3);
        private static readonly RecipeCategory Shields = new RecipeCategory("type.shields", "Shields", 4);
        private static readonly RecipeCategory Armor = new RecipeCategory("type.armor", "Armor", 5);
        private static readonly RecipeCategory Tools = new RecipeCategory("type.tools", "Tools", 6);
        private static readonly RecipeCategory Utility = new RecipeCategory("type.utility", "Utility & Trinkets", 7);
        private static readonly RecipeCategory Consumables = new RecipeCategory("type.consumables", "Food & Potions", 8);
        private static readonly RecipeCategory Materials = new RecipeCategory("type.materials", "Materials", 9);
        private static readonly RecipeCategory Misc = new RecipeCategory("type.misc", "Other", 10);

        private static readonly RecipeCategory[] BiomeCategories;
        private static readonly RecipeCategory UnknownBiome;

        private static readonly Dictionary<Recipe, RecipeCategory> BiomeCache = new Dictionary<Recipe, RecipeCategory>();

        static RecipeCategories()
        {
            var biomes = (Biome[])Enum.GetValues(typeof(Biome));
            BiomeCategories = new RecipeCategory[biomes.Length];
            foreach (var biome in biomes)
            {
                BiomeCategories[(int)biome] = new RecipeCategory("biome." + biome, BiomeMapping.Label(biome), (int)biome);
            }
            UnknownBiome = new RecipeCategory("biome.unknown", "Other", biomes.Length);
        }

        public static RecipeCategory Get(Recipe recipe, GroupingMode mode)
        {
            return mode == GroupingMode.Biome ? GetBiomeCategory(recipe) : GetTypeCategory(recipe);
        }

        private static RecipeCategory GetTypeCategory(Recipe recipe)
        {
            var shared = recipe.m_item.m_itemData.m_shared;

            switch (shared.m_skillType)
            {
                case Skills.SkillType.ElementalMagic:
                case Skills.SkillType.BloodMagic:
                    return Magic;
                case Skills.SkillType.Pickaxes:
                case Skills.SkillType.Fishing:
                    return Tools;
            }

            switch (shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                    return Melee;
                case ItemDrop.ItemData.ItemType.Bow:
                    return Ranged;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                    return Ammo;
                case ItemDrop.ItemData.ItemType.Shield:
                    return Shields;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Shoulder:
                case ItemDrop.ItemData.ItemType.Hands:
                    return Armor;
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch:
                    return Tools;
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket:
                    return Utility;
                case ItemDrop.ItemData.ItemType.Consumable:
                    return Consumables;
                case ItemDrop.ItemData.ItemType.Material:
                    return Materials;
                default:
                    return Misc;
            }
        }

        /// <summary>
        ///     A recipe belongs to the latest biome of its ingredients, as worked out from game data by
        ///     <see cref="BiomeResolver" />. Ingredients it can't place are ignored.
        /// </summary>
        private static RecipeCategory GetBiomeCategory(Recipe recipe)
        {
            if (BiomeCache.TryGetValue(recipe, out var cached) && cached != null)
            {
                return cached;
            }

            int latest = -1;
            foreach (var req in recipe.m_resources)
            {
                if (!req.m_resItem || (req.m_amount <= 0 && req.m_amountPerLevel <= 0))
                {
                    continue;
                }
                if (BiomeResolver.TryGet(req.m_resItem.gameObject.name, out var resolution))
                {
                    latest = Math.Max(latest, (int)resolution.Biome);
                }
            }

            var category = latest >= 0 ? BiomeCategories[latest] : UnknownBiome;
            BiomeCache[recipe] = category;
            return category;
        }

        public static void ClearCache()
        {
            BiomeCache.Clear();
        }
    }
}
