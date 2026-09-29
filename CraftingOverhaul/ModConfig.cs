using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using CraftingOverhaul.Biomes;
using CraftingOverhaul.RecipeList;
using Jotunn.Configs;

namespace CraftingOverhaul
{
    internal enum GroupingMode
    {
        None,
        ItemType,
        Biome
    }

    internal static class ModConfig
    {
        private const string RecipeListSection = "1 - Recipe List";
        private const string ContainerSection = "2 - Nearby Containers";

        public static ConfigEntry<GroupingMode> Grouping;
        public static ConfigEntry<bool> StartCollapsed;
        public static ConfigEntry<bool> ShowSearchBar;
        public static ConfigEntry<bool> SearchIngredients;
        public static ConfigEntry<bool> ClearSearchOnClose;
        public static ConfigEntry<string> BiomeMaterialOverrides;

        public static ConfigEntry<bool> ContainersEnabled;
        public static ConfigEntry<float> ContainerRange;
        public static ConfigEntry<bool> ContainersForBuilding;
        public static ConfigEntry<string> ExcludedContainers;

        public static void Bind(ConfigFile config)
        {
            Grouping = config.Bind(RecipeListSection, "Grouping", GroupingMode.ItemType,
                "How recipes are grouped into collapsible categories. Can also be changed in game with the button next to the search bar.");
            StartCollapsed = config.Bind(RecipeListSection, "StartCollapsed", false,
                "Start each session with every category collapsed.");
            ShowSearchBar = config.Bind(RecipeListSection, "ShowSearchBar", true,
                "Show a search box above the recipe list. Requires a restart.");
            SearchIngredients = config.Bind(RecipeListSection, "SearchIngredients", true,
                "Search also matches ingredient names, e.g. typing 'bronze' lists everything made with bronze.");
            ClearSearchOnClose = config.Bind(RecipeListSection, "ClearSearchOnClose", true,
                "Clear the search box whenever the inventory is closed.");
            BiomeMaterialOverrides = config.Bind(RecipeListSection, "BiomeMaterialOverrides", "",
                "Biome grouping works out each material's biome from the game's own world data. Use this only for items it " +
                "gets wrong or can't place (run 'co_biome <item>' in the console to see why), as comma separated " +
                "PrefabName:Biome pairs, e.g. 'SomeModdedOre:Mountain'. Biomes: " + string.Join(", ", Enum.GetNames(typeof(Biome))) + ".");

            ContainersEnabled = config.Bind(ContainerSection, "Enabled", true,
                AdminOnly("Use materials stored in nearby containers when crafting."));
            ContainerRange = config.Bind(ContainerSection, "Range", 20f,
                AdminOnly("Radius in metres around the crafting station (or you, when building) to search for containers.",
                    new AcceptableValueRange<float>(1f, 100f)));
            ContainersForBuilding = config.Bind(ContainerSection, "UseForBuilding", true,
                AdminOnly("Also use materials in nearby containers when building with the hammer, hoe or cultivator."));
            ExcludedContainers = config.Bind(ContainerSection, "ExcludedContainers", "",
                AdminOnly("Comma separated container prefab names that are never used, e.g. 'piece_chest_private'."));

            BiomeMaterialOverrides.SettingChanged += (_, __) =>
            {
                BiomeResolver.Invalidate();
                RecipeCategories.ClearCache();
                RecipeListOrganizer.Refresh();
            };
            Grouping.SettingChanged += (_, __) => RecipeListOrganizer.Refresh();
        }

        public static HashSet<string> ExcludedContainerNames()
        {
            return new HashSet<string>(
                ExcludedContainers.Value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        private static ConfigDescription AdminOnly(string description, AcceptableValueBase range = null)
        {
            // Synced from the server when the mod is installed there, so server admins decide.
            return new ConfigDescription(description, range, new ConfigurationManagerAttributes { IsAdminOnly = true });
        }
    }
}
