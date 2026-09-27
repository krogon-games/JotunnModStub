# CraftingOverhaul

Makes crafting stations less of a chore.

## Features

### Organised recipe list
- **Collapsible categories.** Recipes are grouped under headers you can click to collapse or expand. Each header shows how many recipes you can craft right now, e.g. `[-] Armor (3/11)`.
- **Group by item type or biome.** The button next to the search box cycles between:
  - **By type**: Melee Weapons, Bows & Crossbows, Magic, Ammo, Shields, Armor, Tools, Utility & Trinkets, Food & Potions, Materials, Other.
  - **By biome**: Meadows through Ashlands and the Deep North, based on the most advanced material in the recipe.
  - **No groups**: the vanilla list.
- **Search.** Type in the box above the recipe list to filter it. Search matches recipe names, category names and (optionally) ingredients, so typing `silver` lists everything made with silver. While you are typing, your keys go to the search box only.

### Craft from nearby containers
- Materials in chests near the crafting station count towards recipes, and are used when you craft. Your own inventory is used first, then the nearest chests.
- Works for building with the hammer, hoe and cultivator too (optional).
- Skips chests you can't open: other players' private chests, chests inside wards you don't have access to, and chests someone else currently has open. Ships, carts and tombstones are never used.

## Configuration

Settings are in `BepInEx/config/com.krogongames.craftingoverhaul.cfg`, or in game with a configuration manager mod.

| Setting | Default | Description |
|---|---|---|
| Recipe List / Grouping | ItemType | `None`, `ItemType` or `Biome` |
| Recipe List / StartCollapsed | false | Start every session with all categories collapsed |
| Recipe List / ShowSearchBar | true | Show the search box |
| Recipe List / SearchIngredients | true | Search also matches ingredient names |
| Recipe List / ClearSearchOnClose | true | Clear the search when the inventory closes |
| Recipe List / BiomeMaterialOverrides | | Extra `PrefabName:Biome` pairs for biome grouping, e.g. modded materials |
| Nearby Containers / Enabled | true | Use materials from nearby containers |
| Nearby Containers / Range | 20 | Search radius in metres |
| Nearby Containers / UseForBuilding | true | Also use containers when building |
| Nearby Containers / ExcludedContainers | | Container prefab names to never use |

The container settings are admin-only: when the mod is installed on a server, the server's values apply to everyone.

## Installation (manual)

1. Install [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).
2. Copy `CraftingOverhaul.dll` into `<Valheim>/BepInEx/plugins/`.

## Known issues
- Biome grouping uses a built-in list of vanilla materials. Materials it doesn't know are ignored, so new Deep North or modded items may land in an earlier biome until they are added to `BiomeMaterialOverrides`.
- If two players take from the same unopened chest in the same instant, the last write wins.

## Changelog
- 0.1.0: First version: search, collapsible categories, type/biome grouping, crafting and building from nearby containers.
