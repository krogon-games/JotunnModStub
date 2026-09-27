using System;
using System.Collections.Generic;
using UnityEngine;

namespace CraftingOverhaul.RecipeList
{
    /// <summary>Biomes in progression order. The order decides how biome groups are listed.</summary>
    internal enum Biome
    {
        Meadows,
        BlackForest,
        Swamp,
        Mountain,
        Plains,
        Mistlands,
        Ashlands,
        DeepNorth
    }

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

        private static readonly Dictionary<string, Biome> MaterialBiomes = new Dictionary<string, Biome>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<Recipe, RecipeCategory> BiomeCache = new Dictionary<Recipe, RecipeCategory>();

        static RecipeCategories()
        {
            var biomes = (Biome[])Enum.GetValues(typeof(Biome));
            BiomeCategories = new RecipeCategory[biomes.Length];
            foreach (var biome in biomes)
            {
                BiomeCategories[(int)biome] = new RecipeCategory("biome." + biome, BiomeLabel(biome), (int)biome);
            }
            UnknownBiome = new RecipeCategory("biome.unknown", "Other", biomes.Length);
        }

        /// <summary>(Re)builds the material table. Called on startup and when the override setting changes.</summary>
        public static void Init()
        {
            MaterialBiomes.Clear();
            BiomeCache.Clear();

            foreach (var entry in DefaultMaterials)
            {
                foreach (var prefab in entry.Value)
                {
                    MaterialBiomes[prefab] = entry.Key;
                }
            }

            foreach (var pair in ModConfig.BiomeMaterialOverrides.Value.Split(','))
            {
                var parts = pair.Split(':');
                if (parts.Length != 2)
                {
                    continue;
                }
                if (Enum.TryParse(parts[1].Trim(), true, out Biome biome))
                {
                    MaterialBiomes[parts[0].Trim()] = biome;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Unknown biome '{parts[1].Trim()}' in BiomeMaterialOverrides");
                }
            }
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

        /// <summary>A recipe belongs to the latest biome any of its ingredients comes from.</summary>
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
                if (MaterialBiomes.TryGetValue(req.m_resItem.gameObject.name, out var biome))
                {
                    latest = Mathf.Max(latest, (int)biome);
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

        private static string BiomeLabel(Biome biome)
        {
            switch (biome)
            {
                case Biome.BlackForest: return "Black Forest";
                case Biome.DeepNorth: return "Deep North";
                default: return biome.ToString();
            }
        }

        /// <summary>
        ///     Best-effort map of material prefab names to the biome they are first obtained in.
        ///     Unknown materials are ignored; use the BiomeMaterialOverrides setting to add more.
        /// </summary>
        private static readonly Dictionary<Biome, string[]> DefaultMaterials = new Dictionary<Biome, string[]>
        {
            [Biome.Meadows] = new[]
            {
                "Wood", "Stone", "Flint", "Resin", "LeatherScraps", "DeerHide", "Feathers", "HardAntler",
                "Raspberry", "Honey", "Mushroom", "NeckTail", "RawMeat", "DeerMeat", "Dandelion",
                "TrophyDeer", "TrophyBoar", "TrophyNeck", "TrophyEikthyr"
            },
            [Biome.BlackForest] = new[]
            {
                "CopperOre", "Copper", "TinOre", "Tin", "Bronze", "BronzeNails", "TrollHide", "BoneFragments",
                "FineWood", "RoundLog", "SurtlingCore", "GreydwarfEye", "AncientSeed", "Thistle", "Blueberries",
                "CarrotSeeds", "Carrot", "Coal", "MushroomYellow", "TrophyGreydwarf", "TrophyGreydwarfBrute",
                "TrophyGreydwarfShaman", "TrophyForestTroll", "TrophySkeleton", "TrophyTheElder"
            },
            [Biome.Swamp] = new[]
            {
                "IronScrap", "IronOre", "Iron", "IronNails", "Chain", "ElderBark", "Guck", "Bloodbag", "Ooze",
                "Entrails", "Root", "Turnip", "TurnipSeeds", "WitheredBone", "SerpentScale", "SerpentMeat",
                "Wishbone", "TrophyBonemass", "TrophyDraugr", "TrophyDraugrElite", "TrophyLeech", "TrophyBlob",
                "TrophySerpent", "TrophyAbomination", "TrophyWraith"
            },
            [Biome.Mountain] = new[]
            {
                "SilverOre", "Silver", "WolfPelt", "WolfFang", "WolfClaw", "WolfMeat", "Obsidian", "FreezeGland",
                "Crystal", "DragonEgg", "DragonTear", "Onion", "OnionSeeds", "JuteRed", "TrophyWolf",
                "TrophyHatchling", "TrophyFenring", "TrophySGolem", "TrophyCultist", "TrophyUlv",
                "TrophyDragonQueen"
            },
            [Biome.Plains] = new[]
            {
                "BlackMetalScrap", "BlackMetal", "Barley", "BarleyFlour", "Flax", "LinenThread", "Needle",
                "LoxPelt", "LoxMeat", "Tar", "GoblinTotem", "Cloudberry", "YagluthDrop", "TrophyGoblin",
                "TrophyGoblinBrute", "TrophyGoblinShaman", "TrophyLox", "TrophyDeathsquito", "TrophyGrowth",
                "TrophyGoblinKing"
            },
            [Biome.Mistlands] = new[]
            {
                "Eitr", "Sap", "BlackCore", "BlackMarble", "Carapace", "Mandible", "YggdrasilWood", "ScaleHide",
                "Wisp", "Softtissue", "JuteBlue", "RoyalJelly", "GiantBloodSack", "Bilebag", "QueenDrop",
                "MushroomJotunPuffs", "MushroomMagecap", "DvergrNeedle", "DvergrKeyFragment", "BugMeat", "HareMeat",
                "TrophySeeker", "TrophySeekerBrute", "TrophyTick", "TrophyGjall", "TrophyHare", "TrophyDvergr",
                "TrophySeekerQueen"
            },
            [Biome.Ashlands] = new[]
            {
                "FlametalOreNew", "FlametalNew", "FlametalOre", "Flametal", "Grausten", "AskHide", "AskBladder",
                "CharredBone", "MorgenSinew", "MorgenHeart", "CelestialFeather", "CharcoalResin", "SulfurStone",
                "ProustitePowder", "MoltenCore", "BellFragment", "GemstoneBlue", "GemstoneGreen", "GemstoneRed",
                "Blackwood", "Vineberry", "VineberrySeeds", "Fiddleheadfern", "MushroomSmokePuff", "VoltureEgg",
                "AsksvinEgg", "AsksvinMeat", "VoltureMeat", "BonemawSerpentTooth", "BonemawSerpentMeat", "FaderDrop",
                "TrophyFader", "TrophyCharredMelee", "TrophyCharredArcher", "TrophyCharredMage", "TrophyMorgen",
                "TrophyAsksvin", "TrophyVolture", "TrophyFallenValkyrie", "TrophyBonemawSerpent"
            },
            [Biome.DeepNorth] = new string[0]
        };
    }
}
