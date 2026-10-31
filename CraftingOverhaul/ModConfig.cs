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
        private const string SheetSection = "3 - Character Sheet";
        private const string HudSection = "4 - HUD";
        private const string ClassesSection = "5 - Classes UI";

        public static ConfigEntry<GroupingMode> Grouping;
        public static ConfigEntry<bool> StartCollapsed;
        public static ConfigEntry<float> HeaderFontSize;
        public static ConfigEntry<bool> ShowSearchBar;
        public static ConfigEntry<bool> SearchIngredients;
        public static ConfigEntry<bool> ClearSearchOnClose;
        public static ConfigEntry<string> BiomeMaterialOverrides;

        public static ConfigEntry<bool> ContainersEnabled;
        public static ConfigEntry<float> ContainerRange;
        public static ConfigEntry<bool> ContainersForBuilding;
        public static ConfigEntry<string> ExcludedContainers;

        public static ConfigEntry<bool> SheetEnabled;
        public static ConfigEntry<UnityEngine.Vector2> SheetPosition;
        public static ConfigEntry<bool> SheetStats;
        public static ConfigEntry<bool> SheetQuickSlots;
        public static ConfigEntry<bool> SheetPortrait;

        public static ConfigEntry<bool> HudBars;
        public static ConfigEntry<float> HudBarScale;
        public static ConfigEntry<float> HudBarMinLength;
        public static ConfigEntry<float> HudBarMaxLength;
        public static ConfigEntry<float> HudBarsHeight;

        public static ConfigEntry<KeyboardShortcut> SkillsPanelKey;
        public static ConfigEntry<UnityEngine.Vector2> SkillCrossPosition;
        public static ConfigEntry<bool> ClassesDemo;

        public static void Bind(ConfigFile config)
        {
            Grouping = config.Bind(RecipeListSection, "Grouping", GroupingMode.ItemType,
                "How recipes are grouped into collapsible categories. Can also be changed in game with the button next to the search bar.");
            StartCollapsed = config.Bind(RecipeListSection, "StartCollapsed", false,
                "Start each session with every category collapsed.");
            HeaderFontSize = config.Bind(RecipeListSection, "HeaderFontSize", 0f,
                new ConfigDescription("Font size of the category headers. 0 uses the same size as the recipe names.",
                    new AcceptableValueRange<float>(0f, 40f)));
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

            SheetEnabled = config.Bind(SheetSection, "Enabled", true,
                "Show a World of Warcraft style character sheet in the inventory: EquipmentAndQuickSlots' equipment slots " +
                "down two columns round a character frame, with a stats box. Needs EquipmentAndQuickSlots; off with Auga.");
            SheetPosition = config.Bind(SheetSection, "Position", new UnityEngine.Vector2(655f, 28f),
                "Top-left corner of the sheet, relative to the inventory grid. Drag the sheet by its wood in game to move it.");
            SheetStats = config.Bind(SheetSection, "ShowStats", true,
                "Show the stats box (armor, weight, health, stamina, eitr, speed from gear, active effects) under the character.");
            SheetQuickSlots = config.Bind(SheetSection, "ShowQuickSlots", true,
                "Put EquipmentAndQuickSlots' quick slots in a row at the bottom of the sheet. Off: that mod places them.");
            SheetPortrait = config.Bind(SheetSection, "ShowCharacter", true,
                "Show a live picture of your character, with its gear, in the sheet's arch. Drag it sideways to turn it.");

            HudBars = config.Bind(HudSection, "Bars", true,
                "Health, eitr and stamina as three bars at the bottom centre (health on top), with your food beside them, " +
                "replacing the game's own displays. Off: the game's displays come back.");
            HudBarScale = config.Bind(HudSection, "BarScale", 2.56f,
                new ConfigDescription("Bar length in pixels per point of health, eitr or stamina. The game's own bars use 1.28; " +
                                      "the default is twice that.", new AcceptableValueRange<float>(0.5f, 10f)));
            HudBarMinLength = config.Bind(HudSection, "BarMinLength", 120f,
                new ConfigDescription("Shortest a bar gets, in pixels.", new AcceptableValueRange<float>(20f, 800f)));
            HudBarMaxLength = config.Bind(HudSection, "BarMaxLength", 600f,
                new ConfigDescription("Longest a bar gets, in pixels (the bars grow with your maximum up to this).", new AcceptableValueRange<float>(100f, 1600f)));
            HudBarsHeight = config.Bind(HudSection, "BarsHeight", 96f,
                new ConfigDescription("How far above the bottom of the screen the bars sit, in pixels. While building or sailing they " +
                                      "move up out of the way, as the game's own bars do.", new AcceptableValueRange<float>(0f, 1000f)));

            SkillsPanelKey = config.Bind(ClassesSection, "SkillsPanelKey", new KeyboardShortcut(UnityEngine.KeyCode.K),
                "Opens and closes the skills panel (needs the Orlog plugin, or the demo). K is free in Valheim's default keys.");
            SkillCrossPosition = config.Bind(ClassesSection, "SkillCrossPosition", new UnityEngine.Vector2(24f, 180f),
                "Bottom-left corner of the skill cross, in pixels from the bottom-left of the screen.");
            ClassesDemo = config.Bind(ClassesSection, "Demo", false,
                "Development: show the class screens with built-in sample data (an Amazon at level 12) instead of Orlog's, so " +
                "they can be tried before classes exist. Spending points and filling slots change the sample only.");

            BiomeMaterialOverrides.SettingChanged += (_, __) =>
            {
                BiomeResolver.Invalidate();
                RecipeCategories.ClearCache();
                RecipeListOrganizer.Refresh();
            };
            Grouping.SettingChanged += (_, __) => RecipeListOrganizer.Refresh();
            HeaderFontSize.SettingChanged += (_, __) => RecipeListOrganizer.Refresh();
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
