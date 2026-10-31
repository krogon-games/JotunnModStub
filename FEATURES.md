# CraftingOverhaul features

Tracks everything the mod adds or changes. It's updated in the same commit as any feature change, and when play-test results come in.

**Status values:** Planned · In progress · Built, untested · Tested in-game · Broken

**Areas:** Crafting · Loot · Combat · UI · Dev tools (console commands for development, not gameplay)

| Feature | Area | Status | Config option | Files touched | Last updated |
|---|---|---|---|---|---|
| [Recipe search box](#recipe-search-box) | UI | Tested in-game | `1 - Recipe List` → `ShowSearchBar`, `SearchIngredients`, `ClearSearchOnClose` | `RecipeList/SearchBar.cs`, `RecipeList/RecipeListOrganizer.cs`, `ModConfig.cs` | 2026-10-26 |
| [Collapsible recipe categories](#collapsible-recipe-categories) | UI | Tested in-game | `1 - Recipe List` → `Grouping`, `StartCollapsed`, `HeaderFontSize` | `RecipeList/RecipeListOrganizer.cs`, `RecipeList/RecipeCategories.cs`, `RecipeList/SearchBar.cs`, `ModConfig.cs` | 2026-10-26 |
| [Data-driven biome detection](#data-driven-biome-detection) | UI | Tested in-game | `1 - Recipe List` → `BiomeMaterialOverrides` | `Biomes/*`, `Biomes/Collectors/*` (incl. `HatchCollector.cs`), `RecipeList/RecipeCategories.cs`, `ModConfig.cs`, `CraftingOverhaul.Tests/*` | 2026-10-26 |
| [Craft and build from nearby chests](#craft-and-build-from-nearby-chests) | Crafting | Tested in-game | `2 - Nearby Containers` → `Enabled`, `Range`, `UseForBuilding`, `ExcludedContainers` | `Containers/NearbyContainers.cs`, `Containers/ContainerPatches.cs`, `ModConfig.cs` | 2026-10-28 |
| [Game data dump and biome inspector](#game-data-dump-and-biome-inspector) | Dev tools | Tested in-game | — (console commands `co_dump`, `co_biome`) | `Dump/*`, `Biomes/Collectors/LocationCollector.cs`, `CraftingOverhaul.cs` | 2026-10-29 |
| [Config reload on save](#config-reload-on-save) | Dev tools | Tested in-game | Both plugins: `0 - Development` → `ReloadConfigOnFileChange` | `Shared/ConfigWatcher.cs`, `CraftingOverhaul/CraftingOverhaul.cs`, `Corruption/CorruptionPlugin.cs`, both `.csproj` | 2026-10-28 |
| [Corruption shards](#corruption-shards) | Loot | Tested in-game | Corruption: `1 - Corruption Shards` → `OneStarChance`, `TwoStarChance`, `BossShardsMin`, `BossShardsMax`, `BossShardsFirstKillOnly`; `2 - Look` → `ShardCustomModel`, `ShardGlowIntensity`, `ShardGlowRange`; `2 - Look` → `ShardBaseItem`, `ShardColor`, `ShardTintStrength` | `Corruption/Content/ShardItem.cs`, `Corruption/Content/Corrupted.cs`, `Corruption/Content/ShardDrops.cs`, `Corruption/Rules/ShardRules.cs`, `Corruption/CorruptionConfig.cs`, `CraftingOverhaul.Tests/ShardRulesTests.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Rules/ObjMesh.cs`, `Corruption/Assets/*`, `tools/make_shard.py` | 2026-10-28 |
| [Riftforge](#riftforge) | Crafting | Tested in-game | Corruption: `2 - Look` → `RiftforgeColor`, `RiftforgeTintStrength` | `Corruption/Content/Riftforge.cs`, `Corruption/Content/Corrupted.cs`, `Corruption/Content/Texts.cs` | 2026-10-26 |
| [Talismans](#talismans) | Combat | Tested in-game (single player) | Corruption: `3 - Talismans` → `Enabled`, `Slots`, `ProvokeSeconds`, `ExcludedCreatures` | `Corruption/Content/Talismans.cs`, `Corruption/Content/TalismanSlots.cs`, `Corruption/Content/TalismanRepel.cs`, `Corruption/Rules/TalismanFamilies.cs`, `Corruption/Rules/TalismanSlotRules.cs`, `Corruption/CorruptionDefaults.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Content/Texts.cs`, `CraftingOverhaul.Tests/TalismanFamiliesTests.cs`, `CraftingOverhaul.Tests/TalismanSlotRulesTests.cs` | 2026-10-28 |
| [Boss essences](#boss-essences) | Loot | Tested in-game (single player) | Corruption: `4 - Essences` → `Enabled`; table `Corruption/Data/essences.csv`; glow uses `2 - Look` → `ShardGlowIntensity`, `ShardGlowRange` | `Corruption/Content/EssenceItems.cs`, `Corruption/Content/EssenceDrops.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Rules/EssenceTable.cs`, `Corruption/Data/essences.csv`, `Corruption/Assets/essence*`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `tools/make_essences.py`, `CraftingOverhaul.Tests/EssenceTableTests.cs` | 2026-10-28 |
| [Maps and rips (step A)](#maps-and-rips-step-a) | Corruption | Tested in-game (single player) | Corruption: `5 - Rips` → `Enabled`, `OpenDays`, `BaseDistance`, `HeartHealth`; table `Corruption/Data/maps.csv` | `Corruption/Content/MapItems.cs`, `Corruption/Content/Rips.cs`, `Corruption/Content/RipObjects.cs`, `Corruption/Content/RipPrefabs.cs`, `Corruption/Content/DataFiles.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Content/Texts.cs`, `Corruption/Rules/MapList.cs`, `Corruption/Rules/RipRules.cs`, `Corruption/Data/maps.csv`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/MapListTests.cs`, `Corruption/Assets/map_icon_*.rgba`, `tools/make_map_icons.py` | 2026-10-29 |
| [Rip waves and loot (step B)](#rip-waves-and-loot-step-b) | Corruption | Tested in-game (single player) | Corruption: `5 - Rips` → `MinStars`, `MaxStars`, `WaveMultiplier`, `ChestShardsMin`, `ChestShardsMax`, `KillShardMultiplier`; table `Corruption/Data/rip_loot.csv` | `Corruption/Content/RipWaves.cs`, `Corruption/Content/Maddened.cs`, `Corruption/Content/RipLoot.cs`, `Corruption/Content/Rips.cs`, `Corruption/Content/RipPrefabs.cs`, `Corruption/Content/ShardDrops.cs`, `Corruption/Content/Texts.cs`, `Corruption/Rules/RipRules.cs`, `Corruption/Rules/RipLootTable.cs`, `Corruption/Data/rip_loot.csv`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/RipWaveTests.cs` | 2026-10-29 |
| [Rip heart search and corrupted wishbone](#rip-heart-search-and-corrupted-wishbone) | Corruption | Tested in-game (single player) | — (heart distance from the game data and the user; recipe in code) | `Corruption/Content/CorruptedWishbone.cs`, `Corruption/Content/Rips.cs`, `Corruption/Content/RipObjects.cs`, `Corruption/Content/Corrupted.cs`, `Corruption/Content/Texts.cs`, `Corruption/Rules/RipRules.cs`, `Corruption/CorruptionPlugin.cs`, `CraftingOverhaul.Tests/MapListTests.cs`, `CraftingOverhaul.Tests/RipWaveTests.cs` | 2026-10-29 |
| [Corrupted bosses (core)](#corrupted-bosses-core) | Combat | Tested in-game (single player) | Corruption: `6 - Corrupted Bosses` → `DamageDealt`, `DamageTaken`, `EssencesMin`, `EssencesMax`, `ShardsMin`, `ShardsMax`; `2 - Look` → `CorruptedBossTintStrength`; console `co_corrupt` | `Corruption/Content/CorruptedBosses.cs`, `Corruption/Content/EssenceDrops.cs`, `Corruption/Content/ShardDrops.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs` | 2026-10-28 |
| [Corrupted offerings](#corrupted-offerings) | Combat | Tested in-game (single player) | — (table `Corruption/Data/offerings.csv`) | `Corruption/Content/CorruptedOfferings.cs`, `Corruption/Content/CorruptedBosses.cs`, `Corruption/Content/Texts.cs`, `Corruption/Rules/OfferingTable.cs`, `Corruption/Data/offerings.csv`, `Corruption/Assets/offering_icon_*.rgba`, `tools/make_offering_icons.py`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/OfferingTableTests.cs` | 2026-10-30 |
| [Character sheet](#character-sheet) | UI | Tested in-game (single player) | `3 - Character Sheet` → `Enabled`, `Position`, `ShowStats`, `ShowQuickSlots`, `ShowCharacter` | `CharacterSheet/CharacterSheetPanel.cs`, `CharacterSheet/CharacterPortrait.cs`, `CharacterSheet/SheetArt.cs`, `CharacterSheet/SheetLayout.cs`, `CharacterSheet/FlatJson.cs`, `Assets/Sheet/*`, `CraftingOverhaul.csproj`, `ModConfig.cs`, `CraftingOverhaul.cs`, `tools/make_sheet_ui.py`, `CraftingOverhaul.Tests/SheetLayoutTests.cs` | 2026-10-29 |
| [Health, eitr and stamina bars](#health-eitr-and-stamina-bars) | UI | Tested in-game (single player, demo); bar order and fixed bars changed 2026-10-31, re-check | `4 - HUD` → `Bars`, `BarScale`, `BarMinLength`, `BarMaxLength`, `BarsHeight` | `Classes/VitalBars.cs`, `Classes/MovedHudPanels.cs`, `Classes/UiKit.cs`, `Classes/ClassesSource.cs`, `CharacterSheet/SheetArt.cs`, `Assets/Sheet/bar*`, `tools/make_sheet_ui.py`, `ModConfig.cs` | 2026-10-31 |
| [Classes UI](#classes-ui) | UI | Tested in-game (single player, demo data; Orlog's API is still a stub); skill paths reworked 2026-10-31, re-check | `5 - Classes UI` → `SkillsPanelKey`, `SkillCrossPosition`, `Demo`; console `co_classes_stone` | `Classes/*`, `CharacterSheet/FlatJson.cs`, `CraftingOverhaul.cs`, `ModConfig.cs`, `CraftingOverhaul.Tests/ClassesUiTests.cs`, `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj` | 2026-10-31 |
| [Currency and Maps tabs](#currency-and-maps-tabs) | UI | Tested in-game (single player) | Corruption: `9 - Inventory Tabs` → `Enabled` | `Corruption/Content/InventoryTabs.cs`, `Corruption/Content/InventoryTabsUi.cs`, `Corruption/Rules/TabRules.cs`, `Corruption/Rules/ShardRules.cs`, `Corruption/Content/Rips.cs`, `Corruption/Content/Texts.cs`, `Corruption/CorruptionConfig.cs`, `CraftingOverhaul.Tests/TabRulesTests.cs` | 2026-10-29 |
| [Loot system](#loot-system) | Loot | Tested in-game (single player; step 1: drops, tooltips, item stats) | Corruption: `7 - Loot` → `Enabled`; console `co_loot`; tables `Corruption/Data/affixes.csv`, `loot_bases.csv`, `loot_rules.csv`, `rare_names.csv`, `drop_chances.csv`, `drop_modifiers.csv` | `Corruption/Content/Loot.cs`, `Corruption/Content/LootPatches.cs`, `Corruption/Rules/LootBases.cs`, `Corruption/Rules/LootRules.cs`, `Corruption/Rules/LootItem.cs`, `Corruption/Rules/AffixRoller.cs`, `Corruption/Data/loot_bases.csv`, `Corruption/Data/loot_rules.csv`, `Corruption/Data/rare_names.csv`, `tools/make_loot_bases.py`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/LootTests.cs` | 2026-10-29 |
| [Combat mechanics](#combat-mechanics) | Combat | Planned | — | — | 2026-10-15 |
| [Levels and XP](#levels-and-xp) | Combat | Tested in-game (single player) | Orlog: `1 - Levels` → `MaxLevel`, `StatPointsPerLevel`, `SkillPointsPerLevel`, `XpMultiplier`, `ShareRange`, `PartyBonusPerPlayer`, `HighLevelPenalty`; `2 - Display` → `ShowXpGains`; tables `Orlog/Data/*.csv`; console `orlog_info`, `orlog_xp`, `orlog_level` | `Orlog/OrlogPlugin.cs`, `Orlog/OrlogConfig.cs`, `Orlog/Api.cs`, `Orlog/Content/Progress.cs`, `Orlog/Content/KillXp.cs`, `Orlog/Content/Commands.cs`, `Orlog/Content/Texts.cs`, `Orlog/Rules/LevelTables.cs`, `Orlog/Rules/ExperienceRules.cs`, `Orlog/Data/*.csv`, `Orlog/Orlog.csproj`, `tools/make_orlog_tables.py`, `CraftingOverhaul.Tests/OrlogLevelTests.cs` | 2026-10-30 |
| [Classes, levels and skill trees](#classes-levels-and-skill-trees) | Combat | Planned | — | — (design: `docs/class-design.md`) | 2026-10-30 |

There are three plugins: **CraftingOverhaul** (quality of life; works when only some players have it), **Corruption** (the endgame mod from `docs/corruption-design.md`; the server and every player need it) and **Orlog** (Diablo 2-style classes and levels from `docs/class-design.md`; the server and every player need it). File paths are relative to `CraftingOverhaul/` unless they start with `Corruption/`, `Orlog/` or `CraftingOverhaul.Tests/`. Config settings are in `BepInEx/config/com.krogongames.craftingoverhaul.cfg`, `com.krogongames.corruption.cfg` and `com.krogongames.orlog.cfg`.

---

## Recipe search box

**What it does in-game:** Adds a text box above the recipe list at every crafting station, and on the inventory's own crafting panel. Typing filters the list as you type. It matches the recipe name, the category name (e.g. "armor") and, optionally, ingredient names (e.g. "silver" shows everything made with silver). While a search is active, every category is expanded so no match is hidden.

**How to use it:** Click the box and type. Clear it to see the full list again. While the box has focus, keys go only to the box, so WASD, E, Tab and the hotbar keys don't act in the game. Click elsewhere, or press Enter or Esc, to give keys back to the game.

**Config settings:**

| Setting | Default | Description |
|---|---|---|
| `ShowSearchBar` | `true` | Show the search box. Requires a restart. |
| `SearchIngredients` | `true` | Also match ingredient names. |
| `ClearSearchOnClose` | `true` | Clear the search when the inventory closes. |

**Known issues:**
- Play-test 2026-10-26: the search box first didn't appear, because the scroll-view lookup skipped inactive objects. Fixed, and confirmed in-game: the search box and the grouping button both show.
- The box goes in space taken from the top of the recipe list. Its position is worked out from the game's layout at runtime, so it may be misaligned at some resolutions or UI scales. This is the most likely thing to need fixing after the first play-test.
- If the recipe list's scroll view can't be found (e.g. another mod rebuilt that UI), the box isn't created. A warning is logged, and grouping still works.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `InventoryGui.Show` | Postfix | Creates the search bar the first time the inventory opens. Also shrinks the recipe list's scroll view (and its scrollbar) from the top to make room. |
| `InventoryGui.Hide` | Postfix | Clears the search if `ClearSearchOnClose` is on. |
| `InventoryGui.UpdateRecipeList` | Postfix | Removes non-matching recipes from the list. Shared with the categories feature. |

Also uses Jötunn's `GUIManager.BlockInput` while the box has focus. Any mod that also resizes or rebuilds the crafting panel's recipe list may conflict with the layout change.

---

## Collapsible recipe categories

**What it does in-game:** Splits the recipe list into categories under headers you can click. Each header shows its open/closed state and how many recipes in it you can craft right now, e.g. `[-] Armor (3/11)`. Inside each category, the game's own sort order is kept, so craftable recipes still come first.

Grouping modes:
- **By type:** Melee Weapons, Bows & Crossbows, Magic, Ammo, Shields, Armor, Tools, Utility & Trinkets, Food & Potions, Materials, Other. Pickaxes and fishing rods count as Tools; staffs count as Magic.
- **By biome:** Meadows, Black Forest, Swamp, Ocean, Mountain, Plains, Mistlands, Ashlands, Deep North, Other. A recipe goes under the latest biome of any of its ingredients. Ingredient biomes come from [data-driven biome detection](#data-driven-biome-detection). Recipes whose ingredients can't be placed go under Other.
- **No groups:** the vanilla flat list.

**How to use it:** Click a header to collapse or expand it. The button to the right of the search box cycles By type → By biome → No groups; the choice is saved to the config. Works in both the Craft and Upgrade tabs.

**Config settings:**

| Setting | Default | Description |
|---|---|---|
| `Grouping` | `ItemType` | `None`, `ItemType` or `Biome`. |
| `StartCollapsed` | `false` | Start each session with every category collapsed. |
| `HeaderFontSize` | `0` | Font size of the category headers; `0` uses the recipe names' size. |

Biome overrides are covered under [data-driven biome detection](#data-driven-biome-detection).

**Known issues:**
- Which categories are collapsed is remembered only until the game closes.
- Collapsing the category that holds the selected recipe moves the selection to the first visible recipe.
- Category labels are English only.
- Play-test 2026-10-26: collapsing, expanding and both grouping modes work. Header text was smaller than the recipe names and indented too far; headers now start where the item icon sits and use the recipe names' font size (adjustable with `HeaderFontSize`).
- Play-test 2026-10-28: headers line up and are readable. The user wants a **UI redesign of the recipe list later** (see `docs/CURRENT_STATE.md` → Later). The first header styled logs the row's font settings (`Recipe row font: ...`) in case the size needs tuning.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `InventoryGui.UpdateRecipeList` | Postfix | Regroups and repositions the recipe rows, inserts header rows, and drops the rows of collapsed categories from `m_availableRecipes` so the selection and gamepad navigation only see visible recipes. Also resizes the list. |
| `InventoryGui.Awake` | Postfix | Resets header, category and biome caches on each world load. |

Any mod that also re-sorts or re-lays out `InventoryGui.m_availableRecipes` or the recipe rows will likely conflict.

---

## Craft and build from nearby chests

**What it does in-game:** Materials in chests near the crafting station count toward recipe requirements (the ingredient counts and whether a recipe can be crafted), and are used when you craft. Your own inventory is used first, then the closest chests. Optionally the same applies to building with the hammer, hoe and cultivator; then chests near you are used instead of chests near a station.

Chests are skipped when:
- they're another player's private chest;
- they're inside a ward you don't have access to;
- someone else has them open;
- they're listed in `ExcludedContainers`.

Ship and cart storage and tombstones are never used. Taking from a ship or cart would make you its network owner, which disrupts whoever is sailing or pulling it.

**How to use it:** Just craft or build near your chests. Nothing to click.

**Config settings** (admin-only: when the mod is on a server, the server's values apply to everyone):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Use materials from nearby containers. |
| `Range` | `20` | Search radius in metres (1–100), around the crafting station, or around you when building. |
| `UseForBuilding` | `true` | Also use containers when building. |
| `ExcludedContainers` | *(empty)* | Comma separated container prefab names never to use, e.g. `piece_chest_private`. |

**Known issues:**
- Play-test 2026-10-26: crafting at the workbench and building with materials from nearby chests both work in single player. Multiplayer is untested; see `docs/multiplayer-test.md`.
- Play-test 2026-10-28 with AdventureBackpacks: crafting from the backpack alone, and from backpack + chest, charge the right amounts (the double-payment fix works). With the backpack panel open the layout still works, but **needs UI work later** (user). Bulk planting with PlantEasily from chests not yet tested.
- If two players take from the same chest at the same moment, the last save wins and one of them may keep or lose items incorrectly. Valheim Plus's version has the same race.
- Recipes that accept any one of several ingredients don't use chest contents. The game picks the item from your inventory directly, so chest stock isn't shown for them either.
- Chest contents are re-checked every 0.5 s, so a chest someone just changed may be briefly out of date in the counts.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Inventory.CountItems(string, int, bool)` | Postfix | Adds nearby chest stock to the player inventory's count, only inside the crafting/building code paths below. |
| `Inventory.RemoveItem(string, int, int, bool)` | Prefix, low priority (changes `amount`) | When paying a cost, takes the shortfall from nearby chests, then lets vanilla remove the rest from the player. Runs after other mods' prefixes and does nothing if one of them already skipped the original (it paid). |
| `InventoryGui.UpdateCraftingPanel`, `InventoryGui.UpdateRecipe`, `InventoryGui.DoCrafting` | Prefix + Finalizer | Mark "crafting in progress" so the two inventory patches apply. |
| `Player.UpdatePlacement`, `Hud.UpdatePieceBuildStatus`, `Hud.UpdatePieceBuildStatusAll`, `Hud.SetupPieceInfo` | Prefix + Finalizer | Mark "building in progress" (when `UseForBuilding` is on). |
| `Player.HaveRequirementItems` | Prefix + Finalizer | Turns chest counting off for any-one-ingredient recipes. |
| `Container.Awake` | Postfix | Tracks every container so nearby ones can be found without physics queries. |

**Compatibility with mods we run** (details in `docs/mod-compatibility.md`):
- **AdventureBackpacks** craft-from-backpack works alongside this feature: counts add up (inventory + backpack + chests) and costs are paid from the backpack first, then chests, then your inventory. Fixed 2026-10-28: when the backpack alone paid, chests were charged again. The backpack's hidden container is never treated as a chest. Untested in-game.
- **PlantEasily** bulk planting can use seeds from nearby chests; its replant-on-harvest uses only what you carry.
- **EquipmentAndQuickSlots**: no overlap in how items are counted or taken.

**Conflicts:** Valheim Plus (and mods like Craft From Containers or AzuCraftyBoxes) have their own craft-from-chests features. If one of those is also on, chest contents may be counted twice, and crafting can fail or take the wrong amounts. Use only one: turn off either this feature (`Enabled = false`) or the other mod's.

Networking mods that move object ownership around, such as NetworkPerformanceSystem (see `docs/coop-networking.md`), also touch chest ownership. This feature claims ownership of a chest right before taking from it, which should work alongside them, but the combination is untested.

---

## Data-driven biome detection

**What it does in-game:** Works out which biome each material first becomes available in, from the game's own data, for [biome grouping](#collapsible-recipe-categories). There is no hand-written material list, so the Deep North and items added by other mods are placed automatically.

**How it works:** Collectors read the game's data and turn it into *sources*, i.e. ways to get an item:

| Collector | Reads | Example |
|---|---|---|
| Vegetation | World-generation vegetation per biome: trees, rocks, ore deposits, pickables | Copper deposits → copper ore, Black Forest |
| Spawns | World spawn tables per biome, and each creature's drops | Deer → deer hide, Meadows |
| Locations | Locations per biome (loaded on demand): boss altars, traders, camps, villages, and the rooms of their dungeons | Sunken crypt rooms → iron scrap, Swamp |
| Conversions | Smelters, kilns, spinning wheel, windmill, eitr refinery, fermenter, cooking stations | Copper ore + coal at a smelter → copper |
| Producers | Planted crops, beehives, sap collectors, wisp fountains | Carrot seeds → carrots |
| Hatching | Items that hatch into creatures | Chicken egg → chicken → chicken meat |
| Recipes | Crafting recipes, including the crafting station's build cost | Copper + tin at a forge → bronze |

Rules (in `Biomes/BiomeSolver.cs`):
- A source's biome is the **latest** of:
  - the biome it's found in;
  - the items it requires, i.e. ingredients, fuel, the station's build cost, or a boss's summoning item;
  - the cheapest tool that can harvest it (e.g. fine wood needs a tier-2 axe, so it's Black Forest even though birches grow in the Meadows);
  - the biome where its required boss key is unlocked (e.g. trader stock that appears after a boss).
- An item's biome is its **earliest** source. This repeats until nothing changes.
- Random chest loot is *weak* evidence: it's only used for items nothing else can place, so a stray item in an early chest doesn't pull a whole group earlier.
- Overrides from the config are fixed, and everything that depends on them builds on them.

The rules are covered by unit tests (`CraftingOverhaul.Tests`, run with `dotnet test`); the collectors can only be checked in-game.

**How to use it:** Nothing to do. To check or correct a result:
- Run `co_biome <item prefab>` in the console to see why an item got its biome, e.g. `co_biome Bronze`.
- Run `co_dump` to list every source, result and unplaced ingredient.
- If something is wrong or unplaced, add it to `BiomeMaterialOverrides`.

**Config settings:**

| Setting | Default | Description |
|---|---|---|
| `BiomeMaterialOverrides` | *(empty)* | Comma separated `PrefabName:Biome` pairs, only for items the detection gets wrong or can't place. Biomes: Meadows, BlackForest, Swamp, Ocean, Mountain, Plains, Mistlands, Ashlands, DeepNorth. |

**Known issues:**
- The Locations collector loads every location and dungeon-room prefab once, when you spawn with biome grouping on (or on first use). This may cause a short hitch; the time taken is logged as `Resolved biomes for N items from M sources in X ms`.
- Items only obtainable in ways the collectors don't read will go unplaced (e.g. things only given by quests or events). `co_dump` lists them in `biome_unresolved.csv`.
- **First in-game dump (1.0.20, 2026-10-26):** 747 items placed. The key materials all landed in the right biome: iron Swamp, silver Mountain, black metal Plains, eitr Mistlands, flametal Ashlands, and 117 Deep North items. Problems found, fixed in the next build, awaiting a new dump:
  - Krigen, Hexen and Elaking drops (molds, keys, Nornathread) landed in Meadows, because some spawn entries apply to every biome. Entries covering every land biome are now ignored.
  - The Deep North boss's later phases weren't followed, so the Crown Jewel and the Bog Witch's seasoning went unplaced. Things spawned on a creature's death are now followed.
  - Bloodgold (and so most Deep North gear) was unplaced: its ore needs tool tier 6, and no player pickaxe reaches it; only the Snow Troll's attacks do. Non-boss creatures' attacks now count as tools in their creature's biome. This is an inference from the data: confirm in-game how Bloodgold ore is mined.
  - Chicken meat and wisps were unplaced. Eggs hatching, creatures growing up and breeding, and wisp spawners are now followed.
  - Hildir's quest rewards weren't placed: items handed in to a trader to set a quest key are now followed.
  - Still expected to need overrides: Embers (`FaderEmber`, no source found in the data).
- **Second dump (2026-10-26, 23:27): the fixes worked.** 913 items placed (up from 747) and unplaced recipe ingredients down from 19 to 2:
  - Every Deep North drop moved out of Meadows.
  - Bloodgold resolves through the Snow Troll's punch, which places about 100 Nord/Bloodgold items.
  - The boss phases give the Crown Jewel.
  - Chickens, Hildir's quest gear and fireworks are placed.
  - Remaining:
    - `Wisp`: the Wisp Fountain is a building piece, which the producer rule now covers. Its build cost (Yagluth's drop) may place wisps in Plains although the fountain only works in the Mistlands; add `Wisp:Mistlands` to the overrides if so.
    - `FaderEmber` (Embers): no source in the game data; needs an override once its in-game source is known.
- Data now considered: breeding, growing up, egg hatching, on-death spawns, creature attacks as tools, wisp spawners, trader hand-ins (in addition to the collectors listed above).
- Ocean is treated as its own biome between Swamp and Mountain, so serpent-scale recipes group under Ocean.
- Loading location prefabs through the game's soft-reference system outside its normal world generation hasn't been tried in-game yet. If it fails, it's logged and those locations are skipped.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Player.OnSpawned` | Postfix | Resolves biomes right after the local player spawns, when grouping is `Biome`. Reads game data only. |

Also shares the `InventoryGui.Awake` postfix from the categories feature to reset its cache.

---

## Config reload on save

*Both plugins. Development aid; switch off or remove before release (see `docs/CURRENT_STATE.md` → Before release).*

**What it does:** when you save `com.krogongames.craftingoverhaul.cfg` or `com.krogongames.corruption.cfg` while the game runs, the mod reloads it about half a second later, so settings apply without a restart. The log says `Reloaded <file>.`. Settings marked "restart to apply" (looks, excluded creatures, search bar on/off) still need a restart.

**Not while connected to another server:** there the server's values for admin-only settings apply, and reloading your local file would replace them; the log says the change wasn't reloaded. In a local world, or on the server itself, it reloads.

**Config settings** (per player):

| Setting | Default | Description |
|---|---|---|
| `ReloadConfigOnFileChange` | `true` | Reload the file when it's saved |

**Cost:** the operating system reports the file change, so nothing is polled; each frame checks one flag. Negligible, but it's a development aid, so turn it off by default for release.

**Vanilla behaviour overridden:** none (a `FileSystemWatcher` and each plugin's `Update`).

---

## Game data dump and biome inspector

**What it does:** Developer console commands for checking mod logic against real game data (vanilla plus any installed mods). Nothing changes in the game.

**How to use it:** Open the console (F5; start the game with `-console` if it doesn't open) while in a world.

- `co_dump` writes these files to `BepInEx/CraftingOverhaul/dump-<date>-<time>/`:

| File | Contents |
|---|---|
| `meta.txt` | Game and mod version, language, every loaded plugin |
| `items.csv` | Every item: type, skill, weight, stack, tool tier, durability, armor, block, all damage types, food values, resolved biome |
| `item_details.csv` | Every item's full shared data, field by field (upgrade amounts, knockback, backstab, parry, set bonuses, equip effects, modifiers), plus attack stamina and per-level damage; resistances in `m_damageModifiers` (e.g. `Frost:Resistant`, added 2026-10-29) |
| `recipes.csv` | Every recipe: output, station and level, ingredients (with per-level amounts), type and biome category |
| `pieces.csv` | Every buildable piece: category, station, build cost |
| `creatures.csv` | Every creature: health, faction, boss flag, defeat key, spawn biomes, drops with amounts and chances |
| `locations.csv` | Every location: biome, quantity, unique |
| `spawns.csv` | The world spawn tables: every entry with all its settings (biome, max spawned, interval, chance, group size, levels, day/night, required keys) |
| `events.csv`, `event_spawns.csv` | Random events (raids): duration, biomes, required keys, messages; and each raid's spawn entries with all their settings |
| `status_effects.csv` | Every status effect: class and every number it sets, including its resistances (`m_mods`, e.g. `Frost:Resistant`; added 2026-10-29) |
| `containers.csv` | Chests and containers that come with loot: drop counts, chance and the weighted drop list |
| `destructibles.csv` | Destructible objects (nests, rocks, crates): health, tool tier and drops |
| `altars.csv` | Boss altars in every location: offering item and count, the boss, item stands and the items they take, the key set on summoning |
| `biome_sources.csv` | Every source the biome collectors found, with requirements and origin |
| `biome_items.csv` | Each item's resolved biome and the source that decided it |
| `biome_unresolved.csv` | Recipe ingredients that couldn't be placed, and which recipes use them |

- `co_biome <item prefab>` explains one item's biome as a chain, e.g. `co_biome Bronze`.
- `co_biome` with no argument shows item counts per biome.
- `co_biome refresh` re-runs detection, e.g. after changing overrides or installing a mod.

**Config settings:** None.

**Known issues:**
- Play-test 2026-10-28: saving a changed setting while in game reloads it (user).
- Names in the CSVs use the game's current language.
- The dump is game data owned by Iron Gate. It's fine for development, but don't ship it with the mod.

**Vanilla behaviour overridden:** None. Registers the two console commands through Jötunn's `CommandManager`.

---

## Character sheet

*CraftingOverhaul. Needs EquipmentAndQuickSlots (EAQS); per player.*

**What it does:** turns EquipmentAndQuickSlots' equipment panel into a World of Warcraft style character sheet in the inventory screen, modelled on the user's concept image (2026-10-29). Mock-up: `docs/images/sheet_preview.png`.

- **Look (restyled 2026-10-30, user's mix of the classes UI mock-ups):** the game's own panel background (the inventory's, copied at runtime), "CHARACTER" in the game's Norse font, a bronze close button (closes the inventory). The slot tiles, the arch and the close button are bronze with rivets, matching the skill slots. The art is generated by `tools/make_sheet_ui.py` and embedded in the DLL. *(First look, 2026-10-29: dark wood planks, iron frame, rope-knot title band, parchment and ribbon.)*
- **Left column**, top to bottom: HEAD, CHEST, LEGS, CAPE (the shoulder slot: cape or AdventureBackpacks backpack), UTILITY, then EAQS's extra utility slots if its utility count is raised.
- **Right column**: TRINKET, then every slot other mods add through EAQS's API in the order they were added: our TALISMAN slots (2 by default), and later the Ring slot. Unknown slots keep their mod's name.
- Each slot is a recessed tile in a bronze, riveted rim with its label underneath. An empty slot shows a faint picture of what goes there (the icon of a vanilla item: Bronze helmet, Bronze plate tunic and leggings, Deer hide cape, Megingjord, Bronze pendant, and the deer trophy for talismans). EAQS's own labels and the blue "equipped" backing are hidden in these slots.
- **Middle: a live picture of your character** in an arch, standing on a stone pedestal: your real model, gear and idle animation, filmed from the front by a camera of our own. **Drag it sideways to turn the view** round the character. Lit by a light of our own as well as the world's, so it shows at night.
- **Stats** in a darker box under the columns, headed by your name (light text; values in brighter health/stamina/eitr colours): armor, weight (red when over), max health, stamina and eitr (eitr only once you have some; values in the game's health/stamina/eitr colours), movement speed from gear (sum of your equipment's movement modifiers, as the game applies it), and on the right your active status effects with their icons (up to 6; "+N more" beyond). Refreshed four times a second.
- **Quick slots** in a centred row of tiles at the bottom (EAQS's, 0–6 as it is configured), with their hotkeys.
- The sheet sits next to the inventory grid where EAQS's panel was; drag it by its wood to move it (saved to `Position`). EAQS's own backgrounds and paperdoll are hidden while the sheet is on.
- Not in the sheet (decided 2026-10-29): hands (Valheim has no weapon slots; weapons are swapped from the hotbar), feet (no boots in Valheim), tabs (the Skills, Trophies and Compendium buttons stay where vanilla puts them).

**How it works:**
- Slots: the slots are EAQS's: cells of hidden inventory rows that EAQS moves into its panel every frame (its postfix on `InventoryGrid.UpdateGui`). Our postfix runs after it and moves the same cells onto our tiles, so dragging, equipping, tooltips, keep-on-death, quick-slot hotkeys and the Talisman slots keep working exactly as before, with no change to how items are stored. Which slots exist and where their cells are comes from EAQS's public API (`GetSlotIdsJson`, `GetSlotInfoJson`), re-read once a second, so slots that appear or disappear (a config change, a mod adding one) re-plan the sheet. Without EAQS, or with Auga installed, nothing changes (logged at the first inventory open).
- Picture (`CharacterPortrait`): a second camera renders into a texture shown in the arch, only while the inventory is open. For just the length of that camera's render, the local player's renderers move to an unused layer (the only layer it sees) and our light switches on; both go back before any other camera renders, with a per-frame safety net. Other players, creatures and the world never appear in the picture.

**Config settings** (per player, `3 - Character Sheet`; all apply live with config reload):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Show the sheet. Off: EAQS's own panel comes back |
| `Position` | `655, 28` | Top-left corner of the sheet, relative to the inventory grid. Set by dragging. *(Was `615, 28`, which covered the inventory's weight number; a config file from the first test keeps its old value until you drag the sheet or delete the line.)* |
| `ShowStats` | `true` | Show the stats box |
| `ShowQuickSlots` | `true` | Put the quick slots in the row at the bottom. Off: EAQS places them where it always did |
| `ShowCharacter` | `true` | Show the live picture of your character. Off: the arch shows an empty night-blue backdrop |

The layout, labels and silhouettes are unit-tested (`SheetLayoutTests`, `FlatJsonTests`: 24 tests).

**Known issues:**
- Play-test 2026-10-29 (first version, single player, with EAQS, AdventureBackpacks and PlantEasily): the sheet appeared with every slot in place and the stats right, but looked too plain next to the concept image, the character frame was empty, and at `615, 28` the sheet covered the inventory's weight number. **Reworked the same day** (art, labels, silhouettes, live picture, new default position); awaiting re-test.
- **Play-test 2026-10-29 (second version):** the look matches the mock-up (frame, title, labelled tiles, silhouettes, parchment, ribbon) and the live picture works, (the side-on view in the screenshot was the user turning it by dragging, which works). Its feet stood a little above a stone that was too thin, and a talisman kept the blue backing. **Fixed the same day** (awaiting re-test): the picture starts from the front each time the inventory opens; the character is a little smaller with a bigger, lighter stone under its feet; the light is brighter and comes from one side; no blue backing in any labelled slot.
- **Play-test 2026-10-29 (third version): "looking much better" (user).** The character faces front, whole body in view, feet on the stone, lit at night; slots, labels, silhouettes and stats as designed.
- ~~To revisit later (user, 2026-10-29): the sheet's look is a bit inconsistent with the rest of the game's UI.~~ **Restyled 2026-10-30** with the classes UI (user: "Let's use Valheim's own UI for that part. Make the character panel match"): the game's own panel and a darker stats box instead of the wood, frame, parchment and ribbon; bronze riveted tiles and arch. Untested in-game.
- Play-test 2026-10-29 (user): the X button closes the inventory, dragging the sheet works, and the layout works with a chest and the AdventureBackpacks backpack open. Not yet checked: multiplayer (other players never appear in the picture). Things to watch: the character too small/large or off the pedestal, too dark or too bright, or not appearing at all (then `ShowCharacter = false` and tell Claude what the log says).
- Panel size is about 500×860 UI units with 3 quick slots; on small screens drag it, or turn off `ShowQuickSlots`.
- The picture camera has none of the game's post-processing, so colours may look a little flatter than in the world.
- `Enabled` off at runtime re-shows EAQS's panel, but EAQS's panel position config is its own.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `InventoryGrid.UpdateGui` | Postfix, low priority, after `randyknapp.mods.equipmentandquickslots` | Player grid only: creates the sheet the first time, moves EAQS's slot cells onto it, hides EAQS's labels, backgrounds and the equipped backing in equipment slots, shows silhouettes, refreshes the stats |
| `InventoryGui.OnDestroy` | Postfix | Forgets the sheet's objects (the GUI is rebuilt per world) |

Not a patch, but worth knowing for other mods: the picture subscribes to Unity's `Camera.onPreCull` / `Camera.onPostRender` and acts only for its own camera. It briefly changes the layer of the local player's renderer objects during that camera's render.

Also reads EquipmentAndQuickSlots' public API (`GetSlotIdsJson`, `GetSlotInfoJson`, API version 2); no patches on that mod. Finds its objects by name (`EaqsSlotRoot`, `EaqsEquipmentBkg`, `EaqsQuickSlotBkg`, `EaqsCustomSlotBkg`, its `PanelDragHandle` script, the cells' `binding` label), checked against EAQS 3.1.3.

**Conflicts:** anything else that repositions EAQS's slot cells (Auga does; the sheet switches itself off when Auga is installed). AdventureBackpacks' backpack panel opens as a container on the left, under the inventory grid, so it shouldn't overlap; check in-game. Mods that also film the player with their own camera (photo modes) might see the player missing for one frame if they render in the middle of ours; none of the group's mods do.

---

## Health, eitr and stamina bars

*CraftingOverhaul; per player. Works without Orlog.* Brief: [`docs/classes-ui-brief.md`](docs/classes-ui-brief.md) §2.1, §2.3; reference `docs/images/hud_reference_bars.jpg`.

**What it does:** replaces the game's health, stamina, eitr and food displays with three long bars together at the bottom centre (user, 2026-10-30, after Elden Ring): **health on top, stamina in the middle, eitr at the bottom** (only once you have eitr; order changed by the user 2026-10-31 so gaining eitr doesn't push stamina down), in the game's colours.
- Each bar grows with its maximum (`BarScale` pixels per point, between `BarMinLength` and `BarMaxLength`), all starting at the same left edge, in a thin bronze rim with knot points at both ends and the value as "current / max" just past its end. A pale trail behind the fill shows what you just lost and catches up after a moment.
- **Food:** your three food slots (round bronze, like the skill medallions) sit to the left of the bars, with the time left under each (minutes, then seconds, as the game shows it); a slot pulses when that food can be eaten again.
- **XP bar** (only with Orlog or the classes demo): a thin purple bar under the bars, with "Level N" on its left and, when points wait to be spent, a pulsing "N stat, M skill points (K)" on its right (the key only once you have a class).
- **They stay put while building or sailing** (user, 2026-10-31). Instead the game's **selected-piece box** (the "Repair" box with the piece's name, description and cost, while a hammer, hoe or cultivator is out) and its **ship display** (sail and rudder) move to the **bottom right**, above the key hints. Each is moved as a whole, by the distance between what it draws and that corner, every frame it shows; the build menu itself stays where the game puts it. Turning `Bars` off puts both back.
- Values come straight from the game (`GetHealth`, `GetMaxHealth`, `GetStamina`, `GetEitr` and their maxima), so food, gear and Orlog's eitr pool all show.

**Config settings** (per player, `4 - HUD`):

| Setting | Default | Description |
|---|---|---|
| `Bars` | `true` | Use the new bars. Off: the game's own displays come back |
| `BarScale` | `2.56` | Pixels per point. The game's own bars use 1.28 (32 px per 25 points, `Hud.UpdateHealth`); proposed default twice that |
| `BarMinLength` | `120` | Shortest bar, in pixels (proposed) |
| `BarMaxLength` | `600` | Longest bar, in pixels (proposed) |
| `BarsHeight` | `96` | Height of the bars above the bottom of the screen (proposed) |

**How it works:** our bars live under the HUD's root (`Hud.m_rootObject`), so they hide with the HUD. The game's displays (`m_healthPanel`, `m_staminaBar2Root`, `m_eitrBarRoot`, `m_foodBarRoot`, the food icons and times) are scaled to zero in a `LateUpdate` (the game's animators set them after `Hud.Update`), and scaled back when `Bars` is turned off.

**Known issues:**
- **Play-test 2026-10-30 (first look, single player, with the demo):** bars, food slots and the XP bar show and work; the game's displays are gone. The user wanted the XP bar "a little bit lower": lowered.
- **Play-test 2026-10-31 (new look):** bars, food and XP bar pass ("looks really good"). Changed after it, to re-check: eitr and stamina swapped (eitr was pushing stamina down when it appeared); the bars no longer move up while building or sailing ("not good design"), the game's selected-piece box and ship display move to the bottom right instead (the user asked for the box; the ship display follows the same rule, Claude's choice).
- To check: nothing else the game shows lives under `m_healthPanel` (if something vanishes from the HUD, tell Claude); the mount's stamina panel still shows when riding.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Hud.Update` | Postfix | Creates and updates the bars, food slots and XP bar |
| `Hud.OnDestroy` | Postfix | Forgets the bars (the HUD is rebuilt per world) |
| `Hud.Update` | Postfix (`MovedHudPanels`) | While the bars are on, moves the game's selected-piece box (the highest parent of `Hud.m_buildSelection` under `m_buildHud` that holds neither `m_pieceSelectionWindow` nor `m_snappingIcon`) and `m_shipHudRoot` to the bottom right; puts them back when the bars are off |
| `Hud.OnDestroy` | Postfix (`MovedHudPanels`) | Forgets them |

---

## Classes UI

*CraftingOverhaul; per player. Needs the Orlog plugin's API (`Orlog.Api`, version 1), or the `Demo` setting.* Brief: [`docs/classes-ui-brief.md`](docs/classes-ui-brief.md); design: `docs/class-design.md`.

**What it does:** everything on screen for the classes (Orlog does the rules). Until Orlog's API is filled in, try it with `5 - Classes UI` → `Demo` = `true`: an Amazon at level 12 with 5 stat points and 1 skill point, her 30 skills from `data/d2-reference/amazon.csv` with the statuses in `class-design.md` §5, vanilla item icons as placeholders. Spending and filling slots change the sample only.
- **Skill cross** (HUD, bottom left, reference `docs/images/hud_reference_skills.jpg`): four slots in a cross (top, left, right, bottom = slots 1–4), each with the skill's icon and its key; the slot on the skill button has a gold outline; a dark sweep shows the cooldown; a slot that can't be used is greyed with "Weapon" or "Eitr". Under it, which button uses the selected skill. Hidden without a class.
- **Skills panel** (`K`, or the **Skills** button above the top right of the inventory): the class's three skill tabs, each a 6 × 3 grid by level tier (Lv 1, 6, 12, 18, 24, 30) with lines from each prerequisite (gold once it has points). Each skill shows its icon, hard points, a green "+" when a point can be spent, grey when locked, gold frame when maxed, "(later)" for skills not built yet; dropped skills aren't shown. Click a skill to spend a point; the reason shows at the bottom when refused. Hover for the tooltip (points, kind, required level, eitr cost, description now and next level, why it's locked). **Drag a skill onto one of the panel's four skill slots or onto the HUD cross** to put it there; drag a slot onto another to move it, or off the slots (or right-click it) to empty it. Passives can't be dragged.
- **Look** (user's mix, 2026-10-30; mock-up `docs/images/classes_ui_mockup.png`): the game's own panel background; skills as round bronze medallions that glow eitr-blue once they have points (green when a point can be spent, gold when maxed); learned prerequisite paths glow eitr-blue like runes and branch like a tree (straight down, across just above the next row, down into the skill; 2026-10-31), names on small dark plaques; bronze riveted slots, with an eitr-blue glow on the slot on the skill button; bars with bronze knot points at their ends and the numbers past them; round bronze food slots; tabs are the game's own buttons.
- **Attribute points**, in a darker box down the left of the skills panel (widened to 290 px, user): level and XP, unspent stat points, and Strength, Dexterity, Vitality and Energy with their value, what they give (text from Orlog) and a + button while points are unspent.
- **Class choice window**: opens when the player uses the Orlog stone (Orlog's `GetClassStoneRequest` goes up) and has no class. Lists the classes (icon, name, description); Confirm chooses, or shows the reason it was refused. With the demo, the console command `co_classes_stone` does what the stone will do (removes the sample class and opens the window).
- Escape or the X closes the panel and the window; the game's input is blocked (cursor shown) while either is open.

**How it works:** `ClassesSource` finds `Orlog.Api` through BepInEx's plugin list (`com.krogongames.orlog`, soft dependency) and binds its static methods by reflection, checking `GetApiVersion() == 1`; answers are flat JSON read with `FlatJson`. It re-reads when `GetChangeCounter()` changes (the HUD also every 60 frames as a fallback), and the cross polls `GetSlotStateJson` each frame. `Demo` on overrides Orlog. The sample data (`DemoClasses`), the tree layout (`SkillTreeLayout`) and the JSON writer (`JsonOut`) are pure and tested (`ClassesUiTests`, 13 tests). Windows go on Jötunn's `CustomGUIFront` canvas.

**Config settings** (per player, `5 - Classes UI`):

| Setting | Default | Description |
|---|---|---|
| `SkillsPanelKey` | `K` | Opens and closes the skills panel. K is free in Valheim's defaults and the group's mods (EAQS Z/V/B, AdventureBackpacks I) |
| `SkillCrossPosition` | `24, 180` | Bottom-left corner of the cross, in pixels from the bottom left of the screen (proposed) |
| `Demo` | `false` | Development: sample data instead of Orlog's. **Off before release** |

**Known issues:**
- **Play-test 2026-10-30 (first look, screenshots, single player, with the demo):** the panel opens, the three tabs switch, spending skill and stat points works, and dragging skills onto the slots fills the panel row and the HUD cross. Broken in the screenshots, **fixed the same day** (re-check): an empty cross slot showed a white square; the tab names ran into each other; long skill names (and "(later)") overlapped or ran off the panel; prerequisite lines crossed the names; locked skills' icons were too dark to see; the selected slot's gold outline barely showed; "Skill button: Right mouse" overlapped the Forsaken power's name (now above the cross). The look was "very basic" (user): restyled the same day (see Look).
- **Play-test 2026-10-31 (new look, screenshots and phone log, single player, demo):** passes: re-check of the first-look fixes, the new look, the cross, the panel's tabs, tooltips, spending, attributes, slots and dragging, closing (K, Escape, X), the class choice window, no errors. The off switches weren't tried. The learned paths "need adjusting how they connect and branch" (user): **reworked the same day** (re-check) to run like tree branches (down, across just above the next row, down), names on small plaques the paths run behind; sketch `docs/images/skill_paths_preview.png`.
- With Orlog installed (and `Demo` off) nothing class-related shows yet: its API only answers the character so far, with no class. To check: the cross next to EAQS's quick slots (EAQS puts them at 216, 150 from the bottom left by default; the cross ends at x 210); the panel (about 644 × 794) fits the screen.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Hud.Update` | Postfix (×3: cross, panel, choice window) | Updates the cross; the panel key and the inventory's Skills button; opens the class choice window on a stone request |
| `Hud.OnDestroy` | Postfix (×3) | Forgets the cross and closes and destroys the windows |
| `Menu.Update` | Prefix | Only on the frame Escape is pressed while a class window is open: closes it and skips the menu's update, so the game menu doesn't open as well |

---

## Corruption shards

*Corruption plugin, design phase 1.*

**What it does in-game:** Adds the **Corruption shard**, the corruption mod's currency: a deep-red gem, styled after the Path of Exile/Diablo shards. It stacks to 50 and can go through portals. By default it uses the Bloodstone model with a red tint; the look is configurable. Phase 1 drop sources:
- 1-star creatures: 2% chance of a shard.
- 2-star creatures: 5% chance.
- Bosses: 1–3 shards the first time each boss is defeated in a world. The check is the boss's own defeat key, so it also covers the Deep North boss's final phase.

Tamed creatures never drop shards, so a pen of starred wolves can't be farmed. Rip waves, rip chests and vanilla dungeon chests (design doc) come in later phases.

**How to use it:** Kill starred creatures and bosses. Shards are an ingredient of the [Riftforge](#riftforge) for now, and later of maps and corrupted offerings.

**Config settings** (admin-only, synced from the server):

| Setting | Default | Description |
|---|---|---|
| `OneStarChance` | `0.02` | Chance a 1-star creature drops a shard |
| `TwoStarChance` | `0.05` | Chance a 2-star (or higher) creature drops a shard |
| `BossShardsMin` / `BossShardsMax` | `1` / `3` | Shards from a boss |
| `BossShardsFirstKillOnly` | `true` | Only on a boss's first defeat per world; off = every kill |
| `ShardBaseItem` (Look) | `GemstoneRed` | Vanilla item whose model the shard uses (e.g. `GemstoneRed` = Bloodstone, `Crystal`, `Ruby`). Per player, restart to apply |
| `ShardColor` (Look) | `#B3170D` | Tint colour (HTML). Per player, restart to apply |
| `ShardCustomModel` (Look) | `true` | Use the shard's own crystal model; off = the `ShardBaseItem` model with the tint. Per player, restart to apply |
| `ShardGlowIntensity` (Look) | `2` | Brightness of the red glow a dropped shard casts on the ground; 0 = off. Per player, restart to apply |
| `ShardGlowRange` (Look) | `2.5` | How far the glow reaches, in metres. Per player, restart to apply |
| `ShardTintStrength` (Look) | `0.35` | 0 = base item's own colours, 1 = fully the tint colour. Per player, restart to apply |

The drop rules are unit-tested (`ShardRulesTests`).

**Known issues:**
- **Play-test 2026-10-28: the first build of the new look broke the shard entirely** (no spawning, no drops, existing shards gone): loading the textures threw an error during registration, so the item was never added, and the game drops items it doesn't know when loading inventories and chests. Fixed: textures load another way, and any failure of the custom look now falls back to the tinted Bloodstone instead of blocking the item. Shards lost in that session are gone unless restored from a backup.
- Play-test 2026-10-28 (after the fix): the custom shard works in game. The user's reference picture showed a different look: a rough-cut, egg-shaped faceted ruby, not a tall spiky crystal. Remade to match (below).
- Play-test 2026-10-28: **the ruby look, icon and ground glow work in game** ("all tested, works"): red faceted gem in the inventory and on the ground, with a clear red glow around it.
- **Current look (2026-10-28):** a rough-cut ruby-like gem, about 20 cm, egg-shaped with diamond facets and a flat top, vivid red with lighter inner reflections and a soft inner glow (preview: `docs/images/shard_preview.png`), made by `tools/make_shard.py`; nothing of the base item's look is used. Risks to check: the texture or glow not showing (the game's item shader may name its texture slots differently; then the base item's texture would show on our model), the shard lying sideways or sinking (it keeps the base item's collider). `ShardCustomModel = false` brings back the tinted Bloodstone.
- Play-test 2026-10-28: 1-star creatures (`OneStarChance` = 1) and 2-star creatures (`TwoStarChance` = 1) drop shards, Eikthyr drops shards on first kill, and the shard is red. The normal 2% / 5% rates haven't been judged in normal play yet.
- The user found dropped shards easy to miss, so they now cast a small red glow on the ground (`ShardGlowIntensity`, `ShardGlowRange`). Glow untested in-game.
- Play-test 2026-10-26: the mod loads and `spawn CorruptionShard` works (name, description and weight correct). The first version, a crystal with a purple tint, came out pale blue: the tint only changed the main colour, not the crystal's glow colours. The tint now changes every colour property, and the shard uses the naturally red Bloodstone model. Awaiting re-test. Drops from creatures and bosses are untested.
- The icon is rendered at startup from the tinted model. If that fails, the vanilla crystal icon is used and a warning is logged.
- A boss killed before installing the mod counts as already defeated, so it won't drop shards unless `BossShardsFirstKillOnly` is turned off.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `CharacterDrop.GenerateDropList` | Postfix | Adds shards to the drop list of starred creatures and bosses |

New item registered through Jötunn: `CorruptionShard` (cloned from the `ShardBaseItem` setting, default `GemstoneRed`).

---

## Riftforge

*Corruption plugin, design phase 1.*

**What it does in-game:** A new crafting station: a red-tinted Galdr table (matching the shard) with its own station name, so later recipes (maps, corrupted offerings) can target it. It has **no recipes yet**.

**How to use it:** Hammer → Crafting tab, built next to a workbench. Recipe: Fine wood ×10, Corruption shard ×1, Eikthyr trophy ×1, Bronze nails ×10, Surtling core ×2. That is 5 ingredients, the most a building piece supports. As with any vanilla piece, it appears in the build menu once you've picked up every ingredient; the shard is normally the last one.

**Config settings** (per player, restart to apply):

| Setting | Default | Description |
|---|---|---|
| `RiftforgeColor` (Look) | `#B3170D` | Tint colour (HTML) |
| `RiftforgeTintStrength` (Look) | `0.6` | 0 = Galdr table colours, 1 = fully the tint colour |

**Known issues:**
- Play-test 2026-10-26: unlocks once every ingredient has been picked up (vanilla rule), builds next to a workbench, and opens as "Riftforge" with an empty recipe list (expected until phase 4).
  - Two issues found and fixed (awaiting re-test):
    - On first build the raven explained the Galdr table: the cloned guide point now has its own key and Riftforge lore.
    - The glow stayed pink-magenta, because particles and lights weren't tinted. They are now.
- Play-test 2026-10-26: the red glow now works. The raven kept coming back after being heard, because the replacement raven text kept the game's default "always show" setting. It now copies the vanilla guide point's settings and switches "always show" off (awaiting re-test). To hear the message again, run `tutorialreset` in the console; it resets every raven message.
- Play-test 2026-10-26: the console filled with `IndexOutOfRangeException` in `Hud.SetupPieceInfo` while the build menu showed the Riftforge. The game's build menu shows a piece's cost in a fixed set of slots and always uses the slot after the last ingredient, so pieces can have at most 5 ingredients; the recipe had 6. Leather scraps removed. Registration now refuses a piece with more than 5 and logs an error.
- Ingredient amounts are first guesses.
- Interacting with it shows an empty recipe list until phase 4 adds maps.

**Vanilla behaviour overridden:** none. New piece registered through Jötunn: `piece_riftforge` (cloned from `piece_magetable`). The clone's raven guide point is changed to its own text (key `corruption_riftforge`); the Galdr table's own raven is untouched.

---

## Talismans

*Corruption plugin, design phase 2.*

**What it does in-game:** Trophy talismans ("<creature> charm", lore: Ægishjálmur, the Helm of Awe).
- **Effect:** while worn, a talisman stops one creature family from targeting, chasing or hitting you. You can still hit them. Hitting one **provokes** it: that creature fights back for 30 s after your last hit; the rest of the family keeps ignoring you.
- **Scope:** only the wearer is protected; other players are unaffected. Bosses are never repelled.
- **Families** come from game data, not a hand-written list:
  - a base creature plus every creature whose prefab name extends it (Greydwarf → Greydwarf_Elite, _Shaman, _Frozen; Goblin → GoblinArcher, GoblinBrute, GoblinShaman; Skeleton → every skeleton variant);
  - only creatures with monster AI (no deer or hares);
  - quest and named minibosses excluded (configurable).
  - About 40 families in vanilla 1.0; the full list is logged at startup (`Registered N talismans: ...`).
- **Recipe** (per family): 3 of the family's trophy, 1 Chain, 10 Leather scraps, at the workbench. Like any vanilla recipe it appears once you've picked up all three. Chain comes from the Swamp, so talismans arrive mid-game.
- **Slots (changed 2026-10-28):** talismans have their own slots, **2 by default** (admin setting, 1–3), so you can wear **2 talismans and 1 vanilla trinket** at the same time. A talisman works while it sits in a talisman slot.
  - The slots are added to **EquipmentAndQuickSlots**' equipment panel through its slot API. Without that mod, talismans have no slot and do nothing (a warning is logged at startup).
  - The same talisman can't go into two slots (a copy adds nothing); different families stack.
- **Item:** not equippable (cloned from a vanilla trinket for its model, then made a plain item with no bonuses), so it never takes the vanilla trinket slot. The icon is the family's trophy.
- **Multiplayer:** the repel check runs on the machine that simulates the creature. Other games can't see what's in your talisman slots, so your game writes the talismans you wear onto your player (synced to everyone) twice a second, and the check reads them from there.
- Talismans not working inside rips (design) comes with phase 4.

**How to use it:** Craft the charm for a creature family at the workbench and drag it into a Talisman slot in the EquipmentAndQuickSlots equipment panel.

**Config settings** (admin-only, synced from the server):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Off: charms can be crafted but do nothing |
| `Slots` | `2` | How many talisman slots work (1–3). Needs EquipmentAndQuickSlots |
| `ProvokeSeconds` | `30` | How long a creature you hit fights back after your last hit |
| `ExcludedCreatures` | `*Hildir*, GoblinBruteBros*, Charred_Melee_Dyrnwyn` | Creature prefab names never repelled (`*` wildcard). Restart to apply |

The family rules are unit-tested against the real 1.0 creature list (`TalismanFamiliesTests`).

**Known issues:**
- Play-test 2026-10-28 (with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily loaded): the two **Talisman** slots appear in the EquipmentAndQuickSlots equipment panel next to the Trinket slot, and the charm recipes show at the workbench. Two bugs found and fixed (awaiting re-test):
  - the charm's tooltip said "Trinket": that was the subtitle line copied from the vanilla trinket; it now says "Talisman: place in a Talisman slot";
  - the charm kept the trinket's **Adrenaline 50**: its bonuses were stripped using a list the game only fills once a player spawns, after charms are made, so nothing was stripped. Now stripped by field name.
  - Also fixed before it showed: charms picked up from the ground (or spawned with cheats) weren't recognised by the slots until relogging, because such items carry their own copy of the item data. Talismans are now recognised by the prefab they came from.
- Play-test 2026-10-28, after the fixes: **working as intended** (user). A Greydwarf charm in a Talisman slot shows the new subtitle and no Adrenaline, and the user confirmed the behaviour. Not yet covered: two charms at once plus a vanilla trinket, provoking, and multiplayer.
- **Play-test 2026-10-28 (single player): passed.** Two different charms plus a vanilla trinket work together; the same charm can't go in both slots; a hit creature fights back for about 30 s, then ignores you again; bosses ignore charms. Multiplayer still untested.
- In game, talismans are called "<creature> charm" (lore: Ægishjálmur charms); "talisman" is the slot and system name.
- Untested in-game.
- Families are by name, so some group widely: one Blob charm covers every ooze (tar, frost, lava...), and the Troll charm also covers Deep North frost trolls. Exclusions can split off individual creatures.
- A creature that already targets you when you put the charm on may keep following you, but can't hit you.
- Charm names use the creature's name in the game language at startup, with English text around it ("Greydwarf charm").

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `BaseAI.IsEnemy(Character, Character)` | Postfix | Returns false when a repelled creature asks about a wearer (and it isn't provoked). Never changes the answer when the player is the one asking, so player attacks are unaffected |
| `BaseAI.OnDamaged` | Prefix | Marks a creature as provoked when the wearer hits it (runs before monsters pick their target) |
| `Player.Update` | Postfix | Local player only, twice a second: writes the talismans in its talisman slots to its player object, for other games to read |

Also calls EquipmentAndQuickSlots' public API (`AddSlot`, `TryGetSlotItem`) to add and read the talisman slots; no patches on that mod.

**Conflicts:** other mods that patch `BaseAI.IsEnemy` (taming, faction or "friendly creature" mods) may interact. The postfix only ever turns `true` into `false` for this one case.

---

## Boss essences

*Corruption plugin, design phase 3.*

**What it does in-game:** Every boss drops **1 essence per kill** (every kill, not only the first). Essences are the map ingredient of the core loop: each boss's essence will craft maps into the corrupted version of its own biome at the Riftforge (phase 4; until then they have no use). They stack to 20, weigh 0.5, go through portals and can't be sold.

| Boss | Item | Look | Maps into |
|---|---|---|---|
| Eikthyr | `CorruptionEssence_eikthyr` | Pale ice-blue gem with lightning veins | Corrupted Meadows |
| The Elder (`gd_king`) | `CorruptionEssence_elder` | Amber sap gem with a wooden root wrapped round it | Corrupted Black Forest |
| Bonemass | `CorruptionEssence_bonemass` | Toxic green gem with bone-white flecks | Corrupted Swamp |
| Moder (`Dragon`) | `CorruptionEssence_moder` | Frosted white-blue gem with ice cracks | Corrupted Mountains |
| Yagluth (`GoblinKing`) | `CorruptionEssence_yagluth` | Ember-orange gem with a gold ring | Corrupted Plains |
| The Queen (`SeekerQueen`) | `CorruptionEssence_queen` | Violet chitin gem | Corrupted Mistlands |
| Fader | `CorruptionEssence_fader` | Molten orange-black gem | Corrupted Ashlands |
| Kall Fimbulbringer (`FrozenKing_p3`, the final phase) | `CorruptionEssence_kall` | Midnight-blue ice with glowing frost runes and a silver crown | Corrupted Deep North |

All eight share one egg-shaped gem model (`essence.obj`) with the boss's own texture, glow colour and, for the Elder, Yagluth and Kall, an extra part. The first seven follow the user's concept render (`docs/images/essences_reference.jpg`); Kall's look is ours (not in the render, awaiting the user's opinion); the in-game preview is `docs/images/essences_preview.png`, both made by `tools/make_essences.py`.

**How to use it:** Kill a boss. Test without fighting: `spawn CorruptionEssence_eikthyr` (or any item above).

**Data table:** [`Corruption/Data/essences.csv`](Corruption/Data/essences.csv): boss, name, description, glow colour, add-on part, `per_kill`, target biome. Compiled into the DLL, so **rebuild after editing**. `dotnet test` checks the file, that every boss is a boss in the 1.0.20 game data, and that every look has its model files. A new look needs its textures made by `tools/make_essences.py`.

**Config settings:**

| Setting | Default | Description |
|---|---|---|
| `Enabled` (`4 - Essences`) | `true` | Admin-only. Bosses drop essences; off = no drops (essences already found stay) |
| `ShardGlowIntensity`, `ShardGlowRange` (`2 - Look`) | `2`, `2.5` | Shared with the shard: the ground glow of a dropped essence. Per player, restart to apply |

**Known issues:**
- **Play-test 2026-10-28 (single player): all passed.** All eight essences look right on the ground and in the inventory (models, icons, glow, the Elder's root, Yagluth's ring, Kall's crown); Eikthyr drops 1 essence on every kill; Kall's essence drops from the last phase only.
- The Hive (an unused, console-only boss in the game files) is deliberately left out (user, 2026-10-28).
- Kall Fimbulbringer's earlier phases (`FrozenKing`, `FrozenKing_p2`) don't drop it; only the final phase does, like vanilla's drops.
- If a custom look fails, that essence falls back to a tinted Bloodstone and logs an error; the item is always registered.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `CharacterDrop.GenerateDropList` | Postfix | Adds the boss's essence (`per_kill`) to its drop list. A second postfix next to the shard one |

New items registered through Jötunn: `CorruptionEssence_<look>` for each table row (cloned from `GemstoneRed`).

---

## Maps and rips (step A)

*Corruption plugin, design phase 4, step A: travel only. Waves, maddened creatures and the end loot come in step B.*

**What it does in-game:**
- **Maps.** Eight maps, one per biome (Corrupted Meadows map … Corrupted Deep North map), crafted at the **Riftforge** from 1 corruption shard, 1 of that biome's boss essence and a stack of a biome material (Leather scraps 5, Greydwarf eye 5, Guck 2, Freeze gland 3, Needle 5, Carapace 16, Charred bone 8, Ice 5). Where each comes from: [Data table](#maps-and-rips-step-a) below. Like vanilla recipes, a map shows up once you've picked up all three. Maps don't stack, go through portals and can't be sold. **Icons** (2026-10-29): each map has its own icon, cut from the user's concept render (`docs/images/maps_reference.jpg`) by `tools/make_map_icons.py`: a hide in the biome's colour with one bold silhouette, a red tear and a bead in the essence's colour. On the ground a map is still a bundle of leather tinted in the essence's colour.
- **Opening a rip.** Use the map (right-click it in your inventory, or from the hotbar). Your game looks for land of that biome **you haven't explored** (your map, including what you've added from a cartography table), above water and as flat as the game requires for placing a location; the server then picks the first spot outside a ward's protected radius (`BaseDistance`) from **anyone's** bed, ward or portal. The map is used up and a red stone portal, the **rip**, opens 4 m in front of you. If nothing suitable is found you're told why and keep the map.
- **Through the rip.** Walk in (or press E) to travel to the landing spot. Vanilla portal rules apply: no ores or metals. Anyone can follow while it's open. *Changed 2026-10-29:* the **rip heart** (a giant glowing corruption shard) is no longer beside the landing spot but 96–128 m away: see [Rip heart search and corrupted wishbone](#rip-heart-search-and-corrupted-wishbone).
- **Getting home.** Destroy the heart (it has a greydwarf nest's health unless `HeartHealth` is set) to open a second rip beside it that leads back to the first. Both rips and the heart close after `OpenDays` (1 in-game day, as in the design; sleeping skips time). No other way back: bring a portal kit or walk.

**How to test:**
- `spawn CorruptionMap_Meadows` (or `_BlackForest`, `_Swamp`, `_Mountain`, `_Plains`, `_Mistlands`, `_AshLands`, `_DeepNorth`), then right-click it.
- Crafting: a Riftforge, a shard, an essence and the material.

**Data table:** [`Corruption/Data/maps.csv`](Corruption/Data/maps.csv): recipe per biome. Compiled into the DLL, so **rebuild after editing**. `dotnet test` checks it, that every essence exists and every material is a game item. Where the numbers come from (2026-10-28, from the 1.0.20 game data):
- **Shards and essence:** 1 each, from the design doc's map recipe.
- **Material:** the one the design doc names (Black Forest, Swamp, Mountains, Plains). Where it offers two, the one that drops earliest, as the design asks: Carapace (guaranteed from Seekers) over Soft tissue (25% from Dvergers); Charred bone (guaranteed from the Charred) over Flametal (mining and smelting). Where it names none (Meadows, Deep North): the guaranteed non-food drop of the weakest creature found only in that biome: Leather scraps (Boar), Ice (Frozen greydwarf, Deep North skeleton).
- **Amount:** the median amount of that material in vanilla recipes and building pieces, rounded up: what the game itself typically asks for.

**Config settings** (admin-only, synced from the server):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Maps open rips; off = using a map does nothing (the map is kept) |
| `OpenDays` | `1` | How long a rip stays open, in in-game days |
| `BaseDistance` | `0` | Rips never open within this many metres of a bed, ward or portal. 0 = the game's ward radius (read from the ward) |
| `HeartHealth` | `0` | Health of the rip heart. 0 = the greydwarf nest's own health |

**Known issues:**
- **Play-test 2026-10-28 (single player):** opening a rip, landing on the ground beside the heart, destroying the heart, the exit rip leading home, a second biome (Plains) and the map recipe at the Riftforge all work. Two bugs, **fixed, to re-test**: the portal hover showed `{0}` instead of the biome and time left (our texts used `{0}`; the game fills in `$1`), and a red console error (`NullReferenceException` in `LightFlicker.Awake`) when the heart loaded (the nest's light-flicker script stayed after its light was removed). Not yet tested: anything in multiplayer.
- **Re-test 2026-10-28: both fixes confirmed** (hover shows the biome and time left; no errors). A `GetBiome error Meadows -> BlackForest` warning in the log is the game's own, logged when standing on a biome border.
- **Rips last a full in-game day:** sleeping only skips the night, so a rip opened in the evening is still open the next morning (seen 2026-10-28; the user is fine with it).
- The landing spot comes from world generation. Locations (villages, altars) aren't avoided yet; step B adds checks once the area has loaded.
- **Map icons tested in game 2026-10-29:** all eight show in the inventory and look good (user). The look on the ground is still the tinted leather bundle.
- With AdventureBackpacks' backpack open, right-clicking a map moves it (AB's quick transfer) instead of using it. Close the backpack first.
- A rip opened by a player who then leaves: its objects close when someone is next near them after the time is up.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Humanoid.UseItem` | Prefix | Using a map from the player's own inventory opens a rip instead of vanilla "use on what?". Steps aside when another mod already handled the use |
| `Player.UpdateTeleport` | Postfix | After a rip teleport finishes, puts the player on the ground at the destination |
| `Game.Start` | Postfix | Registers the rip messages between players' games and the server (`Corruption_RipRequest`, `Corruption_RipReply`) |

New items: `CorruptionMap_<Biome>` (cloned from `LeatherScraps`). New objects: `corruption_rip` (cloned from `portal_stone`, falling back to `portal_wood`; vanilla portal logic, building and damage removed) and `corruption_rip_heart` (cloned from `Spawner_GreydwarfNest`; spawning and drops removed, the shard model at 6× size).

---

## Rip waves and loot (step B)

*Corruption plugin, design phase 4, step B. Every number comes from the game data or the user (sources below).*

**What it does in-game:**
- **Waves.** When a player comes within 96 m of a rip heart (the raids' own range), the rip "tears open": the raid's start message shows, and every creature already nearby turns **maddened** and hunts you. Then waves keep coming **while the heart stands**.
- **Which creatures and how many:** the biome boss's own **army raid**, exactly as the game defines it: Meadows = Eikthyr's army (boars, necks), Black Forest = the Elder's (greydwarfs, greylings, shamans, brutes), Swamp = Bonemass's (draugr, skeletons), Mountains = Moder's (drakes), Plains = Yagluth's (fulings, brutes, shamans), Mistlands = the Queen's (gjall, ticks, seekers), Ashlands = Fader's (charred and their spawner stones), Deep North = Kall's (elakingar, Jotuns). Each raid entry's own limit on how many are alive, spawn interval, chance and group size apply; groups appear 40–80 m from a player, as in vanilla raids.
- **Stars:** every wave creature has **at least 1 star** (user, 2026-10-29), and up to 2 (design: tier 1 maps). The second star rolls with the game's own level-up chance.
- **Maddened:** red, damage ×1.2 (the game's own Dvergr power buff), speed +15% (the Tonic of Ratatosk), never flee, and treat every creature and player as an enemy, so they fight each other too. **Charms don't repel them** (user).
- **End loot:** destroying the heart leaves a chest next to the exit rip: one of that biome's own vanilla chests with its normal loot (weighted by how often each appears in the biome) plus **1–3 corruption shards**.
- **Shards from rip creatures:** twice the normal starred chances (1-star 4%, 2-star 10%).

**How to test:** `spawn CorruptionMap_Meadows`, use it, go through: "The ground is shaking" (Eikthyr's army message), red boars and necks attack in waves; destroy the heart: exit rip plus a Meadows chest with shards.

**Data table:** [`Corruption/Data/rip_loot.csv`](Corruption/Data/rip_loot.csv): chests per biome and their weights, worked out from the 1.0.20 dump (each location's chest matched to its chest prefab by drop list). Compiled into the DLL. `dotnet test` checks every chest is a real container and every biome has one, and that every essence boss has an army raid.

**Config settings** (admin-only, `5 - Rips`):

| Setting | Default | Description |
|---|---|---|
| `MinStars` | `1` | Fewest stars a rip wave creature has (user, 2026-10-29: waves weren't challenging enough). 0 = as vanilla rolls them |
| `MaxStars` | `2` | Most stars a rip creature can roll (design tier 1) |
| `WaveMultiplier` | `2` | Each raid entry's most-alive-at-once ×2 and spawn interval ÷2 in rips (user, 2026-10-29). 1 = the vanilla raid |
| `ChestShardsMin` / `ChestShardsMax` | `1` / `3` | Shards in the rip chest (user) |
| `KillShardMultiplier` | `2` | Multiplies the starred shard chances for rip creatures (user) |

**Known issues:**
- Untested in-game (103 tests pass). **Play-test 2026-10-29 didn't run step B:** no waves, no chest and no errors, because the build came from `master`, which doesn't have step B yet (it's on `claude/rips-bosses`). The log now names the build's branch and commit, so the next test can check.
- **Play-test 2026-10-29, right build:** waves start with Eikthyr's army message, creatures spawn, a 2-star dropped a shard. On a later run, destroying the heart opened the exit and left a Meadows chest with corruption shards (the log shows each step); on the first run the user saw no chest, not reproduced since.
- **Fixed 2026-10-29:** maddened creatures got **no damage or speed bonus**: the vanilla effects they copy were read at startup, before the game makes them available (log: "SE_Dvergr_buff not found"). They're now read in the world, the first time a creature is maddened, and the log says what was copied (`Maddened: damage ×1.2 …`). To re-test.
- **Play-test 2026-10-29 (build 65e6271): passed.** Maddened bonuses read correctly (log: `Maddened: damage ×1.2 (from SE_Dvergr_buff), speed +15% (from Potion_hasty)`); charms don't stop rip creatures; Meadows and Swamp rips each left a chest with shards. **Waves felt too slow** (Meadows: at most 4 alive, one every 20 s): rips now use `WaveMultiplier` 2 (user). Whether maddened creatures fight each other couldn't be judged yet. The user wants the heart moved away from the landing spot and found with a corrupted wishbone (to design).
- A `Failed to find rpc method 327122920` warning when first opening the chest is the game's own: vanilla chests call `discovered` but register `RPC_Discovered`. Harmless, happens with every vanilla treasure chest.
- Maddened creatures stay after the heart is destroyed (the waves stop; the ones alive keep fighting). Vanilla raid creatures despawn when the raid ends; rips have no such end yet.
- **Play-test 2026-10-29:** the busier waves (`WaveMultiplier` 2) passed, and maddened creatures were seen fighting each other, as the design says.
- **Play-test 2026-10-29, after the busier waves:** still not challenging enough (user: too few or too weak). Wave creatures now have at least 1 star (`MinStars` 1). **Re-tested 2026-10-29: "definitely more challenging"** (user). With every creature starred, 1-star shard drops (4% in rips) apply to all of them, so rips give more shards, and 2 stars come one level-up roll sooner (10% instead of about 1%).
- Map tiers (more waves, more stars) come with phase 5.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Character.Awake` | Postfix | Restores maddened behaviour on creatures loaded with the flag |
| `BaseAI.IsEnemy(Character, Character)` | Postfix, last priority | Maddened creatures are everyone's enemy; runs after the talisman patch so charms don't repel them |
| `CharacterDrop.GenerateDropList` | Postfix (the existing shard one) | Double starred shard chances for rip creatures |

New status effect `SE_CorruptionMaddened` (built from `SE_Dvergr_buff` and `Potion_hasty` at startup). The rip heart gets a wave spawner; the chest is the vanilla chest prefab itself.

---

## Rip heart search and corrupted wishbone

*Corruption plugin, after the step B play-test (user, 2026-10-29): "the heart is literally right next to the portal; the players need to go find it", guided by a corrupted wishbone.*

**What it does in-game:**
- **The heart is out of sight.** Players land 96–128 m from the rip heart, in a random direction. 96 m is the boss army raids' own range (every `army_*` raid in the 1.0.20 dump), so the landing spot is quiet and **the waves start as you close in on the heart**; 128 m is the user's choice. Heart and landing spot both pass the old checks (the map's biome, above water, flat, unexplored, away from bases).
- **Corrupted wishbone.** Crafted at the Riftforge from 1 Wishbone and 1 corruption shard (the design's rule for every corrupted item: the normal item plus 1 shard). Worn in the utility slot it works like the vanilla Wishbone, but points at **the heart of the rip you came through**: red-and-purple pings, every 5 s at the edge of its range and every 0.5 s on top of the heart (the vanilla Wishbone's own pacing and its near/medium/far pings), within 128 m of the heart (the farthest you can land from it). **Everyone nearby sees and hears the pings** (user).
- Your game remembers the heart when you go through the entry rip, in your character's saved data, so it still works after logging out inside the rip. It forgets it when the heart is destroyed, the rip closes, or you go home through the exit rip.
- Without the wishbone you can still find the heart: the waves start within 96 m of it.

**How to test:** `spawn Wishbone`, `spawn CorruptionShard`, craft the Corrupted wishbone at the Riftforge (or `spawn CorruptedWishbone`) and wear it. `spawn CorruptionMap_Meadows`, use it, go through: no heart in sight, no waves; red/purple pings, faster as you walk towards the heart; waves start on the way; destroy the heart, pings stop. A second player nearby sees your pings.

**Config settings:** none. The distances come from the game data and the user (`RipRules.HeartMinDistance` / `HeartMaxDistance`; a test checks every army raid's range still fits).

**Known issues:**
- **Play-test 2026-10-29 (single player): works.** The heart is out of sight on landing, the wishbone's pings lead to it. The pings looked **mostly purple** with some red; the user wants mostly red, so now only about a fifth of the colour range shades into purple (`PurpleShare` in `CorruptedWishbone.cs`). **Re-checked 2026-10-29: more red now, and the pings stop when the heart dies** (user).
- Not yet tested with a second player (whether they see the pings). The log says how the pings reach other players (`shared with other players by …`).
- It only finds rip hearts, not buried treasure; keep the vanilla Wishbone for that.
- Rips opened before this update still have the heart beside the landing spot.
- The landing spot still doesn't avoid locations (villages, altars), as before.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Game.Start` | Postfix | Registers the ping message (`Corruption_RipPing`) |

New item `CorruptedWishbone` (cloned from `Wishbone`, tinted). New status effect `SE_CorruptedWishbone` (class `SE_RipFinder`: the vanilla `SE_Finder` behaviour aimed at the remembered heart). New effect prefabs `corruption_<vanilla ping effect>` (tinted clones of the Wishbone's pings). The heart's position is stored on the entry rip (`corruption_rip_heart`) and in the player's custom data (`corruption_rip_heart`).

---

## Corrupted offerings

*Corruption plugin, design phase 6: summoning corrupted bosses. Which item each boss takes and how many come from the game data (`altars.csv`); the rules from the user (2026-10-28 and 2026-10-30).*

**What it does in-game:** each boss's vanilla offering has a corrupted version, crafted at the **Riftforge** and shown with its own icon (the vanilla icon recoloured dark red with a glowing crack, from the user's renders). How it's used follows the boss's own summoning:

| Boss | Corrupted item | Recipe | How |
|---|---|---|---|
| Eikthyr | Corrupted deer trophy | 2 Deer trophies + 1 shard | Offer it at the altar like the trophies: the one item is the whole offering |
| The Elder | Corrupted ancient seed | 3 Ancient seeds + 1 shard | Same |
| Bonemass | Corrupted withered bone | 10 Withered bones + 1 shard | Same |
| Moder | Corrupted dragon egg | 1 Dragon egg + 1 shard | Put one in **every** egg cup (3), then use the altar |
| Yagluth | Corrupted fuling totem | 1 Fuling totem + 1 shard | One on **every** totem stand (5) |
| The Queen | Corrupted sealbreaker | 1 Sealbreaker + 1 shard | Open her door with it: bosses in that location turn corrupted, now or when they appear |
| Fader | Corrupted bell | 1 Bell + 1 shard | One in **every** bell holder (3) |
| Kall Fimbulbringer | Corrupted malicious blood | 3 Malicious blood + 1 shard | Offer it at his altar; his later phases stay corrupted |

- At a stand altar, a **mix** of corrupted and normal items is refused ("Every offering here must be corrupted, or none."), so no corrupted item is used up on a normal fight.
- Pressing Use on an empty stand, or on the Queen's door, uses a **corrupted** item first if you carry one. Carry only normal ones for a normal fight.
- A corrupted item offered at the wrong altar gets the game's own "wrong offering" message.
- Corrupted items are only found in your **main inventory**, not in an AdventureBackpacks backpack (unlike normal keys, which AB lets you use from the backpack).

**How to test:** `devcommands`, then `spawn CorruptedTrophyDeer` (or `CorruptedAncientSeed`, `CorruptedWitheredBone`, `CorruptedDragonEgg`, `CorruptedGoblinTotem`, `CorruptedDvergrKey`, `CorruptedBell`, `CorruptedHatefulBlood`), go to the boss's altar (`goto` or the map) and offer it. The boss arrives red and corrupted; the log says `Corrupted offering: Eikthyr summoned corrupted.`

**Data table:** [`Corruption/Data/offerings.csv`](Corruption/Data/offerings.csv): boss, vanilla item, how it's used (`altar`, `stand`, `door`), vanilla items per corrupted item, name and description. Compiled into the DLL. `dotnet test` checks every row against the game's altars (item, count, stand altars) and that each has its icon.

**Known issues:**
- **Play-test 2026-10-30 (single player):** Eikthyr (one corrupted trophy summons him corrupted), Moder (all egg cups corrupted; a mix refused) and the Queen (her door with the corrupted Sealbreaker) passed. **Bug, fixed:** the Ancient seed and Malicious blood icons were swapped (the renders were matched to the wrong items when saved). **Kall's phases stay corrupted** (tested 2026-10-30 with `spawn FrozenKing`, `co_corrupt`, `killall`: phases 2 and 3 came up red).
- Kall's phases: the next phase is corrupted when a corrupted phase dies and a boss named like its next phase (`FrozenKing_p2`, `_p3`) appears on the same game.
- The fixed seed and blood icons are still to be seen in game.
- The Queen: assumes she is in the same game location as her door (checked with the game's own location lookup). The log says how many bosses were corrupted when the door opened.
- The corrupted bosses' summons (maddened adds) and extra particles come later.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `OfferingBowl.UseItem` | Prefix | A corrupted offering at its altar summons the boss with the one item |
| `OfferingBowl.RPC_RemoveBossSpawnInventoryItems` | Prefix | Takes the one corrupted item instead of the full offering's count |
| `OfferingBowl.Interact` | Prefix | Stand altars: all stands corrupted = corrupted summon; a mix is refused |
| `OfferingBowl.DelayedSpawnBoss` | Postfix | Corrupts the boss just summoned |
| `ItemStand.IsSupported` | Postfix | Stands that take an offering take its corrupted version |
| `ItemStand.Interact` | Prefix | Use on an empty offering stand places a carried corrupted item first |
| `Door.Interact`, `Door.UseItem` | Prefix | A corrupted key opens the door and marks it |
| `Character.OnDeath` | Prefix | Remembers that a corrupted boss died, for its next phase |
| `Character.Awake` | Postfix | Corrupts a corrupted boss's next phase, and bosses behind a corrupted door |

New items `Corrupted<Item>` (cloned from the vanilla item, tinted red). Door data key `corruption_corrupted_door`.

---

## Corrupted bosses (core)

*Corruption plugin, design phase 6, the parts the design fully specifies. Corrupted offerings at the altars (and the corrupted Sealbreaker for the Queen) follow once `altars.csv` from the next `co_dump` shows each altar's real offering; the summons follow phase 4 step B's maddened creatures.*

**What it does in-game:** a corrupted boss is the vanilla boss, tinted red, that:
- deals 1.5× damage and takes 0.7× damage (health unchanged), as the design's starting values;
- drops 2–3 of its essence instead of 1, and 3–5 corruption shards on every kill, plus its normal vanilla drops.

The flag is stored on the boss itself, so every player sees the same corrupted boss.

**How to test:** `devcommands`, `spawn Eikthyr`, then `co_corrupt` (corrupts the nearest boss within 50 m): it turns red, hits harder, takes longer to kill and drops 2–3 essences and 3–5 shards.

**Config settings** (admin-only unless noted; defaults from `corruption-design.md` → Corrupted bosses):

| Setting | Default | Description |
|---|---|---|
| `DamageDealt` | `1.5` | Damage multiplier on the boss's attacks |
| `DamageTaken` | `0.7` | Damage multiplier on hits the boss takes |
| `EssencesMin` / `EssencesMax` | `2` / `3` | Essences per corrupted kill (replaces the normal amount) |
| `ShardsMin` / `ShardsMax` | `3` / `5` | Shards per corrupted kill, every kill |
| `CorruptedBossTintStrength` (Look) | `0.6` | Per player. The Riftforge's tint strength, which was tested in-game |

**Known issues:**
- **Play-test 2026-10-28 (single player): passed.** `co_corrupt` turns Eikthyr red; it hits harder, takes longer to kill and drops 2–3 essences and 3–5 shards. Since 2026-10-30, corrupted offerings summon them too (see [Corrupted offerings](#corrupted-offerings)).
- The design's extra particles (Bonemass poison cloud, Ashlands embers) aren't added yet: which particle prefab to reuse comes from the game data with the summons.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Character.Awake` | Postfix | Adds a watcher to bosses that tints them red once they're corrupted |
| `Character.RPC_Damage` | Prefix | Scales hits on and from corrupted bosses |
| `CharacterDrop.GenerateDropList` | Postfix (the existing shard and essence ones) | Corrupted bosses: 3–5 shards and 2–3 essences |

---

## Currency and Maps tabs

*Corruption plugin, design phase 9. Behaviour decided by the user, 2026-10-29.*

**What it does:** two extra tabs on the player inventory, so currency and maps don't take bag space.

- **Tab buttons** **Bag | Currency | Maps** sit on top of the inventory panel. Currency or Maps shows that tab in place of the bag (the character sheet and equipment slots stay put); the inventory opens on Bag each time.
- **Currency tab** (8 × 4): corruption shards and boss essences; later orbs, Affix Splinters and runes. **Maps tab** (8 × 4): rip maps. Items are recognised by their prefab name (`CorruptionShard`, `CorruptionEssence_*`, `CorruptionMap_*`), so new essences and maps from the data tables go to their tab without code changes.
- **Straight to the tab:** picking up (by hand or auto-pickup) puts currency and maps in their tab, even when the bag is full. Only a full tab sends them to the bag. Anything that still lands in the bag (crafted at the Riftforge, dragged in, taken from a chest, maps from before this update) moves to its tab within half a second. EquipmentAndQuickSlots' quick and equipment slots are left alone.
- **Weightless:** tab contents don't count towards carry weight.
- **Still usable:** the tabs count and pay as part of your inventory for every recipe (Riftforge maps, building costs, boss altars), and pay before the bag. Right-click a map in the Maps tab to open its rip. Drag between a tab and a chest, shift-click to send an item to an open chest, split stacks, hover for tooltips, as in the bag. A tab only takes its own items.
- **Death:** tab contents go into the gravestone with the rest of your inventory, in rows under the bag's (world settings that keep or delete items on death apply to them too). Taking everything from the gravestone puts them back in their tabs; they don't count against the gravestone's "room and weight" check.
- Saved with the character (in its custom data), so they come with you between worlds and servers like the bag. Only you see your tabs.

**Config settings** (admin-only, synced from the server):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Off: the tab buttons are hidden and the tabs' contents are kept (not lost) but can't be reached or used until it's turned on again |

Tab size (8 × 4) is fixed by the user's choice; which item goes where and the gravestone layout are unit-tested (`TabRulesTests`, against the real `maps.csv` and `essences.csv`).

**Known issues:**
- **Play-test 2026-10-30 (single player):** the tab buttons and switching work (step 1 passed), and the bag's maps moved to the Maps tab. But the tab grids were broken: every cell after the first showed a placeholder (a "1/2" icon on yellow, a "Tooltip" text), nothing could be dragged, dropped or moved out of a tab, and a crafted map seemed to vanish (it was in the Maps tab, just not drawn). Cause: the copied grid lacked six of the eight callbacks the game gives its own grids; the missing drop check (`CanDropDragOntoItem`) threw a NullReferenceException right after the first item on every draw. **Fixed the same day** (all callbacks wired; awaiting re-test). Also changed: tabs fill from the top-left (the game puts materials in the bottom row first).
- **Re-test 2026-10-30 after the fix:** the grids draw correctly and fill from the top-left; crafting a map from Currency-tab shards and an essence puts the map in the Maps tab; right-clicking a map there opens its rip; dragging to and from a chest, shift-click to a chest and refusing wood work; no errors. Pickup into the tabs with a full bag and backpack works. **Still failing then:** left-click couldn't pick up an item in a tab unless a container (the backpack) was open (Ctrl+click to drop worked). Cause: with no container open, the game cancels every frame any drag whose item isn't in the bag (`InventoryGui.UpdateContainer`, also `CloseContainer`). The first fix guarded `UpdateInventory` by mistake and changed nothing (second re-test, same day: still failing); now `UpdateContainer`. **Third re-test 2026-10-30: passed** (pick up, move within a tab, drop on the ground with nothing open). Every Session 1e step has passed in single player. **Second re-test 2026-10-30: death and gravestone, relogging, and picking up with a full bag pass.**
- Right-clicking a map in the Maps tab reached the rip code; the rip didn't open only because the test world had no unexplored Meadows left (the map then stays, as designed). Re-test in a world with unexplored land (or after `resetmap`).
- Mouse only: the tab grid isn't part of the game's gamepad navigation.
- Auto-pickup still checks the item's weight against your carry limit, so when you're overloaded it skips currency (picking up with E works).
- With a tab full, extra items stay in the bag; when space frees up they move over on their own.
- A dropped item can't swap a non-tab item into a tab (the drop is refused).
- If a character's saved tabs can't be read, they're never saved over (the log says so); tell Claude.

**Vanilla behaviour overridden (Harmony patch targets):** all act only on the local player's own inventory.

| Target | Patch | Purpose |
|---|---|---|
| `Player.Load` | Postfix | Reads the tabs from the character's custom data (`corruption_tab_currency`, `corruption_tab_maps`) |
| `Player.Save` | Prefix | Writes the tabs into the character's custom data |
| `Inventory.AddItem(ItemData)` | Prefix, high priority (skips the original) | Puts currency and maps in their tab when it has room (pickup, take from a chest) |
| `Inventory.CanAddItem(ItemData, int)` | Postfix, low priority | A full bag can still take currency and maps while their tab has room |
| `Inventory.MoveAll` | Prefix | "Take all" (gravestone, chest): currency and maps to their tabs first |
| `Player.Update` | Postfix | Local player, twice a second: moves currency and maps from the bag's rows to their tab |
| `Inventory.CountItems(string, int, bool)` | Postfix | Adds the tabs' stock |
| `Inventory.HaveItem(string, bool)` | Postfix | Also true when only a tab has the item |
| `Inventory.RemoveItem(string, int, int, bool)` | Prefix, high priority (changes `amount`) | Pays from the tabs first; does nothing if an earlier prefix already skipped the original |
| `Player.CreateTombStone` | Prefix | Moves the tabs' items into rows under the bag, so the gravestone takes them like everything else |
| `Inventory.MoveInventoryToGrave` | Postfix | Gives the gravestone enough rows for them |
| `TombStone.EasyFitInInventory` | Postfix | Leaves tab items (which go to the tabs and weigh nothing) out of the room and weight check |
| `InventoryGui.Show` / `Hide` / `UpdateInventory` / `OnDestroy` | Postfix | Creates the tab buttons and grid, resets to Bag on close, keeps the shown tab's grid current |
| `InventoryGrid.DropItem` | Prefix | Refuses items that don't belong in the tab, and swaps that would put a non-tab item in a tab |
| `Inventory.TopFirst` | Postfix | Tabs fill from the top-left |
| `InventoryGui.UpdateContainer`, `InventoryGui.CloseContainer` | Prefix + Finalizer | Mark the two places where the game drops drags from non-bag inventories when no container is open |
| `InventoryGui.SetupDragItem` | Prefix (skips the original) | Within those two, keeps a drag that came from a tab |
| `InventoryGui.OnSelectedItem` | Prefix (our grid only) | Shift-click in a tab with a chest open moves the item to the chest |

Also changed: using a map (`Humanoid.UseItem` prefix in `Rips.cs`) accepts maps in the Maps tab, and opening the rip takes the map from the bag or the tab.

**Conflicts:** see `docs/mod-compatibility.md`. EquipmentAndQuickSlots also patches `AddItem`, `CanAddItem`, `MoveInventoryToGrave`, `EasyFitInInventory` and the gravestone's height; AdventureBackpacks and CraftingOverhaul's craft-from-chests also patch `CountItems` and `RemoveItem`. Payment order: tabs, then AdventureBackpacks' backpack, then nearby chests, then the bag.

---

## Loot system

*Corruption plugin, design phase 7, step 1 of several: drops, tooltips and item-stat affixes. Orbs, uniques, personal-loot locks, loot beams, player-stat, gathering and proc effects come in later steps.*

**What it does in-game:**
- **Magic and Rare drops.** Every creature kill (not tamed creatures or players) rolls for loot, once **per player within the game's nearby-players range** (`Game.m_difficultyScaleRange`, the range the game counts players in to scale creature difficulty; 100 m in the 1.0 code). Chances come from `drop_chances.csv` (Magic 1 in 50, Rare 1 in 100) times `drop_modifiers.csv`: stars (`star_1`–`star_3`), rip creatures (`rip_tier_1`; maps have no tiers yet), corrupted bosses (uniques only). If Magic and Rare both succeed, only the Rare drops (`DropTable.Roll`). Unique rolls are ignored until uniques exist.
- **Which item:** a random player-craftable weapon, shield, armour piece, cape (backpacks included), trinket or pickaxe **from the biome the creature died in**; with **20%** chance one from the biome just before it (user, 2026-10-29). Meadows always gives Meadows items; the Ocean gives none. The list is `loot_bases.csv`, built from the game dump by `tools/make_loot_bases.py` (248 items; the biome is the item's own from the dump, e.g. gem-socketed Ashlands weapons count as Deep North because their recipe needs a Deep North material).
- **Upgrade level 1**, full durability, like a freshly crafted item (user). Upgrading at the workbench or forge keeps the affixes.
- **Affix count:** Magic 1 or 2 (1 twice as often), Rare 3 or 4 (3 twice as often) (user: fewer is likelier). Magic holds at most 1 prefix + 1 suffix, Rare 2 + 2.
- **Rolling** (`AffixRoller`): each affix is picked by its `weight` among every affix that fits the item's classes and mechanics (`requires`), one per family per side; values are whole numbers anywhere in the range (every range in the table is whole). Both halves of a twin merge into one **resonant** line rolled from the resonant range (+25% on the combined top). **Every affix in `affixes.csv` rolls, including rows still marked `draft`** (user, 2026-10-29: test with the draft numbers while the ranges are settled).
- **Names:** Magic items take their affixes' names around the base name, the roll deciding which of a family's names (e.g. *Sharp/Fine/Cruel/Merciless*): "Cruel Bronze Sword of Embers". Rare items get a two-word name from `rare_names.csv` ("Grim Fang"). Names are blue (Magic, `#8888FF`) or yellow (Rare, `#FFFF77`), the Path of Exile colours, in inventory tooltips and when hovering over the item on the ground.
- **Tooltips:** after vanilla's lines, the rarity (and for Rares the base item), then one line per affix: its value, the bonus it gives this item at its current level (item-stat affixes, e.g. `(+14)`), and its roll range in brackets, e.g. `+24% damage (+14) [5–40]`; resonant lines end in `◆ resonant`. Affixes this step doesn't apply yet are grey and say "(not active yet)".
- **Affixes that work in this step** (through the game's own stat getters, so vanilla tooltips, combat and repairs see them): enhanced damage (every combat damage type, not chop or pickaxe damage), fire/frost/lightning/poison/spirit damage (a % of the enhanced physical damage), armour, durability, block power, attack stamina (also the tools' "per swing" version), knockback and backstab (raised on the attacker's game when the hit is made), parry bonus.
- **Storage:** `ItemData.m_customData` keys `corruption.rarity`, `corruption.name` (Rares) and `corruption.affixes` (`id=value;prefix+suffix=value`), saved and synced with the item by the game.

**How to test:** `co_loot` drops a Rare item of the biome you stand in; `co_loot magic` a Magic one; `co_loot rare SwordBronze` a chosen base item (any prefab in `loot_bases.csv`). Kill starred creatures for natural drops (1 in 50 / 1 in 100 per kill is slow; `co_loot` is the quick way).

**Config settings** (admin-only, `7 - Loot`):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Creatures drop Magic and Rare items. Off: no new drops; items already found keep their affixes |

**Data tables** (compiled into the DLL; rebuild after editing): `affixes.csv`, `loot_bases.csv` (rerun `tools/make_loot_bases.py` after a new game dump), `loot_rules.csv` (previous-biome chance, drop level, affix-count weights), `rare_names.csv`, `drop_chances.csv`, `drop_modifiers.csv`. `dotnet test` checks all of them, and that `loot_bases.csv` matches the dump.

**Known issues:**
- Tested in single player 2026-10-29/30 (179 tests pass); not yet with several players.
- **First look 2026-10-29 (user's screenshots):** `co_loot` Rares work: "Night Ward" (Rare Leather Trousers: +15% armour, armour 2 → 2.3) and "Skull Song" (Rare Bronze Sword: +37% damage, slash 35 → 48), yellow titles, affix lines with ranges, inactive lines grey. Magic items work too: "Shieldwall Bronze Atgeir", "Seismic Antler Pickaxe" (gathering affix on a pickaxe), "Crackling Club" (+19% of physical as lightning: blunt 12 adds lightning 2), blue names. Still to check: ground hover names, durability, block, repair, upgrade, natural drops, combat affixes.
- **Play-test 2026-10-29 (phone log):** build check, natural drops from kills, combat affixes (no errors), a resonant line and no errors all passed. **Ground hover name failed** (showed the base name): the game localizes the hover text before our patch sees it, so the patch looked for the untranslated name. **Fixed 2026-10-30; re-test passed.** **Upgrading passed 2026-10-30:** a Rare Bronze Sword taken to level 4 kept its name and affixes; its numbers follow the level (slash 53 = 35 + 3×6; spirit 23 = 43% of 53; durability 623 = 350 × 1.78). **Durability and repair passed 2026-10-30.** Every Session 3 step has passed; multiplayer (personal rolls for each player) not tested yet.
- Gear ranges: 16 set from the game data and approved (2026-10-29); max health, stamina, eitr and life steal are drafts by choice; resistances settled 2026-10-30 (fire, frost, poison, lightning 10–50%; all resistances 2–10%). No caps on stacking carry weight, regen or max health/stamina/eitr (user, for now).
- Personal loot is only the separate rolls: dropped items aren't locked to a player yet (the 60 s owner lock and loot beams come later).
- The tables are compiled in; the design wants the server's copy to be the one everybody uses (later step).
- Knockback and backstab use the weapon in hand when the hit lands; switching weapons while an arrow flies uses the new one.
- Vanilla tooltip lines for knockback, backstab, parry and stamina show the base item's numbers; the affix line shows the bonus.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `CharacterDrop.OnDeath` | Postfix | Magic and Rare drops, rolled per nearby player |
| `ItemDrop.ItemData.GetTooltip(ItemData, int, bool, float, int, bool)` | Postfix | Appends rarity and affix lines (AdventureBackpacks also postfixes it; both append) |
| `InventoryGrid.CreateItemTooltip` | Postfix | Tooltip title in the rarity colour |
| `ItemDrop.GetHoverText` | Postfix | Ground hover name in the rarity colour |
| `ItemDrop.ItemData.GetDamage(int, float)` | Postfix | Enhanced and elemental damage |
| `ItemDrop.ItemData.GetArmor(int, float)` | Postfix | Enhanced armour |
| `ItemDrop.ItemData.GetMaxDurability(int)` | Postfix | Durability |
| `ItemDrop.ItemData.GetBaseBlockPower(int)` | Postfix | Block power |
| `Attack.GetAttackStamina` | Postfix | Less stamina per attack |
| `Character.Damage(HitData)` | Prefix | Knockback and backstab on the local player's hits |
| `SEMan.ModifyTimedBlockBonus` | Postfix | Parry bonus |

**Orb art (2026-10-28):** models and textures for the nine currency orbs exist (`tools/make_orbs.py`, `Corruption/Assets/Orbs/`, preview `docs/images/orbs_preview.png`), after the user's concept render (`docs/images/orbs_reference.jpg`). They are not in the mod yet: they get embedded and registered as items when the orbs are built. The orbs keep their Path of Exile names (user, 2026-10-28).

---

## Combat mechanics

**Status:** Planned. Not started.

**Ideas so far:**
- Perfect-parry timing windows, combos, a posture/stagger bar, dodge i-frames and weapon special moves.
- Likely patch points: `Humanoid`, `Attack`, `Character.Damage`.
- New animations need asset-bundle work in the `JotunnModUnity` project. They are allowed where necessary (user, 2026-10-30).

The Corruption mod design (`docs/corruption-design.md`) makes fights harder (maddened creatures, corrupted bosses) without new attacks or animations. These combat ideas remain a separate track.

## Levels and XP

*Orlog (new plugin, `com.krogongames.orlog`). The server and every player need it.*

**What it does:** characters gain XP from kills and level up to 99, Diablo 2 style. Built 2026-10-30; not yet tested in-game.
- **XP needed per level:** Diablo 2's experience table (500 for level 2, 7,875 for level 5 … 3.5 billion for 99), `Orlog/Data/levels.csv`.
- **Creature levels:** bosses have the user's levelling guide level (Eikthyr 5, The Elder 11, Bonemass 20, Moder 35, Yagluth 50, The Queen 62, Fader 76, Kall Fimbulbringer 90, all three phases). Any other creature takes the level range of the **biome it dies in** (Meadows 1–4, Black Forest 5–10, Swamp 11–19, Mountains 20–34, Plains 35–49, Mistlands 50–61, Ashlands 62–75, Deep North 76–89) and is placed in it by its base health on a log scale between the biome's weakest and strongest creature (from the game dump's spawn lists). A draugr is a strong Meadows creature but a weak Swamp one. The ocean has no range: its creatures count as the players' average level.
- **XP per kill:** `Orlog/Data/creature_xp.csv` at the creature's level: D2's XP to the next level divided by **classic WoW's kills per level** (WoW's XP table ÷ level×5+45), so a player needs about as many same-level kills per level as in classic WoW without quests (8 at level 1, about 60 at level 11, 290 at 51, 617 from level 89). At the default pace that is about 60 same-level kills to reach Eikthyr's level, 320 to the Elder's, 1,050 to Bonemass's, 3,200 to Moder's, 6,600 to Yagluth's, 10,600 to the Queen's, 16,800 to Fader's and 24,600 to Kall's (from the tables; real play mixes in weaker and starred creatures). `XpMultiplier` changes the pace.
- **Stars:** a 1-star creature gives ×2, a 2-star ×3 (its health is base × its game level, from the game code).
- **Level difference:** a creature **at or below** your level gives its own XP, cut by Diablo 2's rule when it's far below (within 5 levels full XP; 6–10 below 81% down to 5%). A creature **above** your level gives the XP of a creature of *your* level plus classic WoW's bonus, +5% per level above, at most +20% (changed 2026-10-30 after the play-test, user's choice: D2 paid a higher creature's full XP, which made early levels 2–3× too fast). Level 2 now takes about 8 Meadows greydwarves (was 3), Eikthyr's level about 60 kills of the Meadows' toughest creatures.
- **Sharing:** every player within `ShareRange` (70 m) of the kill shares it, D2 party style: +35% per extra player, split by level, each share taken of what the kill is worth to that player (level difference above). Only kills by a player or a creature on the players' side (tamed, summoned) give XP; tamed creatures and the training dummy give none.
- XP is kept with the character (custom data `orlog_xp`), like vanilla skills, so it carries across worlds. Vanilla skills keep levelling by use. No XP loss on death (user).
- On each kill "+N XP" in the top-left (per-player setting); on a new level "Level N" in the centre with the vanilla skill level-up effect.
- Stat and skill points are counted (5 and 1 per level by default) for the UI, but can't be spent yet (attributes and skills come next).

**How it works:** the creature's owner (the game that ran the fight) works out the XP in a prefix on `Character.OnDeath` and sends each player's share in the routed message `Orlog_Xp` to that player's game. Every game publishes its player's level on the player's ZDO (`orlog_level`) so the owner can split by level and apply the level difference. The UI reads level and XP through `Orlog.Api` (`docs/classes-ui-brief.md`).

**Config settings** (`1 - Levels` admin-only, server-synced; `2 - Display` per player):

| Setting | Default | Description |
|---|---|---|
| `MaxLevel` | `99` | Highest level (D2) |
| `StatPointsPerLevel` | `5` | Attribute points per level (D2) |
| `SkillPointsPerLevel` | `1` | Skill points per level (D2) |
| `XpMultiplier` | `1` | Multiplies all kill XP; 1 = classic WoW's pace without quests |
| `ShareRange` | `70` | Metres within which players share a kill (EpicMMOSystem's group range) |
| `PartyBonusPerPlayer` | `0.35` | Extra XP per additional sharing player (D2) |
| `HighLevelPenalty` | `false` | D2's XP cut from level 70; off because it would put level 90 by Kall out of reach |
| `ShowXpGains` | `true` | "+N XP" per kill |

Console commands: `orlog_info` (level, XP, the biome's creature levels), `orlog_xp <amount>` and `orlog_level <level>` (cheats, need `devcommands`).

The tables, the level difference and party rules are unit-tested (`OrlogLevelTests`: 28 tests).

**Known issues / not yet:**
- **Play-test 2026-10-30 (user, single player):** `orlog_info`, "+N XP" per kill, ×2 XP from a 1-star, level-up message and effect, 6 XP for a boar at level 20 (far below), level and XP kept after relogging, no console errors: all work. Not yet tested: sharing with a friend.
- **Pace too fast (user, 2026-10-30):** 3 greydwarves and 1–2 necks reached level 2. Why: the pace assumed same-level kills, but most Meadows enemies (greylings, greydwarves, skeletons, 20+ health) sit at the top of the Meadows range, level 4 (178 XP), and D2 pays a higher creature's full XP within 5 levels; a neck (level 1) gives 62. **Fixed the same day** (user chose option B): higher creatures now give your own level's XP + WoW's up to +20%; a greydwarf at level 1 gives 71. **Re-tested 2026-10-30: "pace feels better" (user).** `XpMultiplier` stays for fine-tuning.
- D2's party bonus is read as +35% per extra player (Arreat: "+35% when a party member is in the area").
- A kill only counts when the last hit came from a player or a creature on their side; a creature finished by fall damage or another creature gives nothing.
- Modded creatures work (level from the biome they die in); modded bosses aren't in the boss list and count as their biome's creatures.
- **Bosses give the XP of one creature of their level** (Eikthyr ≈ a level-5 creature). D2's act bosses give 9–65× a normal monster (`monstats` Exp%: Andariel 916%, Baal 6460%, a zombie 111%); how much more ours should give is open (asked the user).
- Kall Fimbulbringer's three phases (`FrozenKing`, `_p2`, `_p3`) each count as a level-90 kill.

**Vanilla behaviour overridden (Harmony patch targets):**
- `Character.OnDeath` (prefix): works out and sends the kill's XP (owner only; reads `m_lastHit`, `m_health`, the star level).
- `Player.OnSpawned` (postfix): publishes the local player's level on its ZDO.
- `Game.Start` (postfix): registers the `Orlog_Xp` routed message.

## Classes, levels and skill trees

**Status:** Planned. Design only, no code (branch `claude/class-skills`).

Diablo 2-style classes in a separate plugin: the Amazon first (group vote, 2026-10-30), then the Barbarian, maybe the Paladin. Character levels from XP, with stat points (D2's Strength, Dexterity, Vitality, Energy) and skill points each level, spent in D2's three-tab trees (skills open by level; prerequisites only where both skills are built). Only the skills that work in Valheim; the rest are reworked or dropped. Vanilla skills keep levelling by use. EpicMMOSystem was compared and is not used as a base. Design, open questions and build order: `docs/class-design.md`; D2 reference data: `data/d2-reference/`.

**Harmony patch targets:** none yet.

---

## Changelog

### 2026-10-31
- **Classes UI play-tested** (single player, demo): bars, food, XP bar, skill cross, skills panel, attributes, slots, closing and the class choice window pass; the new look passes. Changed after it (user): bar order is now health, stamina, eitr; the bars stay put while building or sailing and the game's selected-piece box and ship display move to the bottom right instead; the skill paths branch like a tree with the names on plaques. Re-check pending.

### 2026-10-30
- **New look for the classes UI and the character sheet** (user's mix of four mock-ups): mock-up D's bronze medallion skills, eitr-blue rune paths, bronze riveted slots, bars and cross, on the game's own panel background (copied from the inventory) instead of dark wood and parchment; the character sheet restyled to match; skills panel wider. Untested in-game.
- **Classes UI, first look** (screenshots): panel, tabs, spending, attributes and drag-to-slot work; food and XP bar pass. Fixed: white empty cross slot, overlapping tab and skill names, lines through names, too-dark locked icons, faint selected outline and points hint, cross label over the Forsaken power; XP bar lowered (user). A polish pass for the look is to be agreed.
- **Classes UI built** (untested, from `docs/classes-ui-brief.md`): new health/eitr/stamina bars at the bottom centre with food beside them and an XP bar; the skill cross; the skills panel with the three skill tabs, prerequisite lines, spending, tooltips, drag-to-slot and the attribute points; the class choice window for the Orlog stone. Reads Orlog's API by reflection; a `Demo` setting serves a sample Amazon until Orlog's API is filled in (207 tests).
- **Currency and Maps tabs tested in-game** (single player): every step passed after the drag fix.
- **Currency and Maps tabs, second re-test:** death and gravestone, relogging and full-bag pickup pass. The drag fix guarded the wrong method (`UpdateInventory`); the cancel is in `UpdateContainer`. Fixed; re-test.
- **Currency and Maps tabs, re-test:** grids, crafting from the tabs, maps from the Maps tab, chests and full-bag pickup pass. Fixed: left-click couldn't pick up a tab item unless a container was open (the game cancels drags from non-bag inventories then); a tab drag is now kept.
- **Currency and Maps tabs, first play-test:** buttons and switching work; the tab grids failed to draw past the first item (missing game callbacks on the copied grid, a NullReferenceException), so items couldn't be moved and a crafted map looked lost. Fixed: every callback wired; tabs now fill from the top-left. Awaiting re-test.
- **Loot tooltips show the actual bonus** (user): item-stat affix lines add what they give this item at its current level before the roll range, e.g. `+43% of physical damage as spirit (+23) [10–50]`; the number follows upgrades. Untested in-game.
- **Resistance ranges settled** (user): fire, frost, poison and lightning 10–50%, all resistances 2–10%. Every gear affix now has its range.
- **Loot: ground names and upgrading tested in-game.** Fire, frost and poison resistance ranges 10–50% from the new `co_dump` (gear that makes you Resistant; proposed); lightning and all resistances have no gear source (asked). Dump files `item_details.csv` and `status_effects.csv` refreshed with resistances.
- **Fix: loot names on the ground** showed the base item name: the game localizes the hover text before our patch, which looked for the untranslated name. To re-test. Play-test results for loot step 1 recorded (drops, combat affixes, resonant lines, no errors passed).
- **Corrupted offerings play-tested:** Eikthyr, Moder, the Queen and Kall's phases work. Fixed the swapped Ancient seed and Malicious blood icons.
- **Corrupted offerings (new, untested):** every boss's offering has a corrupted version, crafted at the Riftforge (the full offering + 1 shard at altars, 1 item + 1 shard per stand at Moder's, Yagluth's and Fader's altars, Sealbreaker + 1 shard for the Queen's door), with icons from the user's renders. They summon the boss corrupted; Kall's later phases stay corrupted.
- **Levels: pace fix re-tested** (single player): "pace feels better". Only sharing with a friend is left to test.
- **Levels: slower early levels** (after the play-test, user's choice): a creature above your level now gives your own level's XP + classic WoW's bonus (+5% per level, max +20%) instead of its full XP (D2). Level 2 takes about 8 greydwarves instead of 3. Lower creatures keep D2's cut. Untested in-game (158 tests).
- **Levels and XP play-tested** (single player): XP per kill, stars, level-up, the level-difference cut and saving all work. Levelling felt too fast (level 2 after 3 greydwarves and 1–2 necks); cause found (most Meadows enemies are level 4 and D2 pays their full XP); fix being decided with the user.
- **Levels and XP built** (new plugin Orlog, branch `claude/class-skills`): XP from kills up to level 99 with D2's level table, level difference and party rules; creature levels from the user's levelling guide and the biome they die in; classic WoW's kills per level for the pace; XP shared within 70 m; level published for other players; `Orlog.Api` level and XP calls for the UI; console commands `orlog_info`, `orlog_xp`, `orlog_level`. Untested in-game (155 tests).
- **Classes: the Amazon comes first** (group vote), with 19 of her 30 skills planned for v1 and the rest later. Tree layout decided: D2's three tabs, level tiers instead of broken prerequisite chains, upgrade choices on big skills later. Levels follow D2's rules, all in admin config; no XP loss on death. Design only.
- **Classes, levels and skill trees planned** (design only): `docs/class-design.md`, D2 reference tables in `data/d2-reference/`. EpicMMOSystem researched; own levels recommended. No mod code changed.
- **New animations allowed where necessary** (user): the v1 "no new animations" rule is lifted; vanilla animations and emotes still come first.

### 2026-10-29
- **Currency and Maps tabs** (Corruption, branch `claude/ui-overhaul`): Bag | Currency | Maps tabs on the inventory, 8 × 4 each. Shards and essences go to Currency and maps to Maps straight from pickup; tab contents weigh nothing, pay for recipes first, open rips from the tab, and go into the gravestone on death (all decided by the user). Untested in-game.
- **Gear affix ranges from the game data** (loot; approved by the user): enhanced damage 4–19%, armour 10–50%, durability 11–55%, block 4–18%, backstab, knockback and parry 20–100%, attack stamina 4–20%, health regen 6–30%, stamina regen 8–40%, carry weight 10–50 (user cut it from 30–150), weapon skill 4–20; max health/stamina/eitr and life steal keep their drafts; no stacking caps for now, under the user's rules (gear-only sources; a full forge upgrade where vanilla has no +% source). **`co_dump` now writes resistances** (item `m_damageModifiers`, status effect `m_mods`) for the resistance affixes.
- **Loot system, step 1 (new, untested):** creatures drop Magic (1 in 50) and Rare (1 in 100) items, rolled per nearby player: a craftable item of the biome they died in (20% the biome before), level 1, with 1–2 or 3–4 rolled affixes, Magic names from the affixes and two-word Rare names, coloured names, and tooltip lines with [ranges]. Damage, elemental damage, armour, durability, block, attack stamina, knockback, backstab and parry affixes work; the rest show as "not active yet". `co_loot` spawns one. New tables `loot_bases.csv` (from the game dump), `loot_rules.csv`, `rare_names.csv`; admin setting `7 - Loot` → `Enabled`.
- **Character sheet, last single-player checks passed:** the close button, dragging the sheet, and the layout with a chest and the AdventureBackpacks backpack open. Only multiplayer is left (`PLAYTEST.md` → Session 2).
- **Rip waves at least 1 star:** new setting `MinStars` (default 1); every wave creature spawns with at least one star (user: waves still not challenging enough). Tested in game: definitely more challenging. Corrupted wishbone colour re-check passed.
- **Map icons:** the eight maps have their own icons, cut from the user's concept render (`tools/make_map_icons.py`), replacing the icon rendered from the tinted leather bundle. Tested in game: they look good.
- **Rip heart search tested in-game; pings mostly red:** finding the heart with the corrupted wishbone works; the pings now shade into purple only for about a fifth of their colours (they looked mostly purple). Busier waves and maddened creatures fighting each other also passed.
- **Rip heart search and corrupted wishbone (new, untested):** the heart now stands 96–128 m from the landing spot (outside the waves' range), and a Corrupted wishbone (Wishbone + 1 shard at the Riftforge) pings towards it in red and purple, faster the closer you get; everyone nearby sees the pings.
- **Rip waves tested in-game; busier waves:** new setting `WaveMultiplier` (default 2): each raid entry's most-alive-at-once doubled and its interval halved in rips (Meadows: 8 alive, one every 10 s), after the play-test found the vanilla pacing too slow.
- **Fix: maddened bonuses** (damage ×1.2, speed +15%) were missing because the vanilla effects were read too early; now read in the world. **Build stamp** now reads the branch and commit from the `.git` folder, so it works without the git program (the user's PC has none). Rip chest with shards confirmed in-game.
- **Rip end diagnostics:** play-test found no chest after destroying a rip heart; the heart, exit and chest steps now write to the log.
- **Build stamp:** the Corruption log line now says which git branch and commit the DLL was built from (`Corruption 0.1.0 loaded (build: claude/rips-bosses 79279a1)`), after a play-test ran a `master` build that didn't have the feature being tested.
- **Character sheet tested in-game** (single player, third version): looks and works as intended. Noted for later: make its look more consistent with the vanilla UI. Merged into `master`.
- **Character sheet, second play-test:** look confirmed close to the mock-up; the live picture works and turns by dragging. Changed: the picture resets to the front on each open; feet now on a bigger stone; brighter side light; no blue backing on talismans either.
- **Character sheet reworked after the first play-test** (it worked, but looked too plain): generated art closer to the concept image (iron-framed dark wood, rope-knot title, framed slot tiles with labels and faint silhouettes, arched frame with a stone pedestal, parchment stats with a name ribbon, close button), a **live picture of the character** (drag to turn), stats in the game's colours with effect icons, and a default position that no longer covers the inventory's weight. New setting `ShowCharacter`. Untested in-game (124 tests).
- **Character sheet** (CraftingOverhaul, branch `claude/ui-overhaul`): EquipmentAndQuickSlots' slots rearranged into a World of Warcraft style sheet (armour left, trinket and talismans right, character frame in the middle, stats box, quick slots row), from the user's concept image. The slots stay EAQS's; only their cells move. Decided: no hand slots, no feet, no tabs; a live character render comes as step 2. Untested in-game (114 tests).

### 2026-10-28
- **Rip waves and loot (phase 4 step B):** each rip spawns its biome boss's army raid while the heart stands; maddened creatures (Dvergr power damage, Ratatosk speed, no fleeing, enemies of all, unaffected by charms); up to 2 stars; the heart leaves a vanilla biome chest plus 1–3 shards; rip creatures drop shards at double chances. Table `rip_loot.csv`. Untested in-game (103 tests).
- **Rip fixes confirmed in-game;** maps and rips step A now tested in single player. Rips last a full in-game day; sleeping skips only the night.
- **Affix Splinters and shattering designed** (from Last Epoch; no gameplay change yet): free shattering of Magic and Rare items at the Riftforge (not uniques or corrupted), 50% per affix line to get a splinter that keeps its exact value (a resonant line keeps the combined value); splinters go on Normal and Magic items with fewer than 2 affixes; they need the Currency tab. Chance in `Corruption/Data/shattering.csv`, rules in `Corruption/Rules/ShatterRules.cs`, with tests (100 tests).
- **Play-test results (single player):** boss essences, corrupted bosses, the rest of talismans, config reload and craft-from-chests with AdventureBackpacks pass; maps and rips work apart from two bugs. **Fixed:** rip hover texts showed `{0}` (now `$1`, the game's placeholder); a console error from the rip heart's leftover light-flicker script (custom looks now remove flicker scripts with the lights, also on the shard and essences). UI work noted for later.
- **Shard corruption rules completed** (design only): the corrupted affix may repeat a family, resonates when it completes a twin and ignores the affix caps; corrupted gear only comes from a shard (the first draft's corrupted drops are removed).
- **Shard corruption outcomes decided** (no gameplay change yet; loot system): 25% bricked, 25% unchanged but locked, 50% one corrupted affix on top of the normal prefix/suffix limit; Normal, Magic and Rare items can be corrupted; the affix rolls from the normal pool. Chances in `Corruption/Data/corruption_outcomes.csv` with a reader and roll (`Corruption/Rules/CorruptionOutcomes.cs`) and tests (94 tests).
- **Gathering affix behaviour decided** (no code yet): both Bountifuls are a 5–15% chance to double the harvest (or its wood); Determined, Gemseeker's pool, Prospector's scope; Seismic, Splintering and Forester's derived from the game data.
- **Affix decisions** (no code yet; loot system): gathering affix sides, roll weights, the combined one-handed axe pool, the twin reading (Magic items resonate too) and five gathering twin pairs, recorded in `affixes.csv` and `docs/AFFIX_RULES.md`.
- **Corrupted bosses (core):** corrupted bosses deal 1.5× and take 0.7× damage, drop 2–3 essences and 3–5 shards, and turn red (design starting values). `co_corrupt` corrupts the nearest boss for testing. Offerings and summons follow. Decided: corrupted offerings use the full normal offering + 1 shard; the Queen uses a corrupted Sealbreaker. Untested in-game.
- **`co_dump` writes `item_details.csv`** (every item field, for the gear affix ranges).
- **Gear affix range rule** (user): top = strongest vanilla source, bottom = a fifth of the top; elemental damage now 10–50%, movement speed 1–5%.
- **`co_dump` writes six more files:** world spawn tables, raids and their spawns, status effects, container loot, destructibles and boss altars, so rip waves, maddened creatures and rip loot (phase 4 step B) can be based on the game's own numbers. Untested in-game.
- **Meadows and Deep North map materials approved** by the user (Leather scraps, Ice).
- **Maps and rips, step A:** eight maps crafted at the Riftforge; using one opens a rip to unexplored land of its biome (server keeps rips away from bases); destroying the rip's heart opens the way home; rips close after an in-game day. Table `maps.csv`, settings `5 - Rips`. Untested in-game (89 tests).
- **Map recipes and rip numbers from game data, not guesses** (user, 2026-10-28): materials follow the design doc or, where it names none, the biome's weakest native creature's guaranteed drop; amounts are the material's median vanilla recipe amount; the base distance defaults to the ward radius, the heart to the nest's health, and landing flatness uses the game's location placement test.
- **Currency orb art** (no feature yet): nine orb models and textures generated from the user's concept render, kept out of the DLL until the orbs are built (phase 7).
- **Maps redesigned:** each essence now crafts maps into the corrupted version of its own boss's biome (Eikthyr → Meadows … Fader → Ashlands). **Kall Fimbulbringer essence** added (Deep North, dropped by the final phase; frost-crown look, not from the render). The Hive is left out. Untested in-game.
- **Boss essences (phase 3):** every boss drops 1 essence per kill. Seven essences with their own looks after the user's concept render, defined in `Corruption/Data/essences.csv` (validated by tests). Setting `4 - Essences` → `Enabled`. The shard's model code is now shared (`GemModel.cs`). Untested in-game (81 tests).
- **Corruption shard look tested in-game:** the rough-cut ruby model, its icon and the ground glow all work.
- **Corruption shard reshaped** after the user's reference picture: a rough-cut, egg-shaped faceted ruby replaces the tall spiky crystal. Untested in-game.
- **Fix: shards disappeared with the new look.** Texture loading threw during registration (raw data without mip levels), so the shard item didn't exist. Textures now load with `SetPixels32`, and a failing custom look falls back to the tinted Bloodstone instead of blocking registration.
- **Corruption shard: its own model and texture.** A jagged red crystal with glowing veins replaces the Bloodstone look (generated by `tools/make_shard.py`, embedded in the DLL). Setting `ShardCustomModel`. Untested in-game (77 tests).
- **Corruption shards tested in-game:** 1-star, 2-star and boss (Eikthyr) drops work and the shard is red. Dropped shards now glow red on the ground so they're easier to spot (configurable, untested).
- **Config reload on save** (dev aid, both plugins): saving a `.cfg` file while the game runs reloads it. Off switch: `0 - Development` → `ReloadConfigOnFileChange`. Marked for removal or default-off before release.
- **Corruption shards:** 1-star creature drops confirmed in-game.
- **Talismans tested in-game (single player):** working as intended with all mods loaded after the fixes.
- **Talisman fixes after the first play-test with all mods:** charms no longer keep the base trinket's Adrenaline 50 or its "Trinket" subtitle, and charms picked up from the ground are recognised by the talisman slots. The slots themselves showed up correctly.
- **Talismans: 2 talisman slots plus the vanilla trinket.** Talismans are no longer trinkets; they work from their own slots (default 2, admin setting `Slots`), added to EquipmentAndQuickSlots' equipment panel through its API, and are published to other players through the player object. Without EquipmentAndQuickSlots they do nothing. Untested in-game (74 tests).
- **Mod compatibility** (`docs/mod-compatibility.md`): checked EquipmentAndQuickSlots 3.1.3, PlantEasily 2.3.1 and AdventureBackpacks 2.4.6 against our patches. Fixed in craft-from-chests: with AdventureBackpacks' craft-from-backpack, a cost the backpack covered was charged to nearby chests too; and the backpack's hidden container could be counted as a chest. Untested in-game.
- Affix numbers for the new affixes approved. Caps moved into `Corruption/Data/affix_caps.csv` with a validator (`Corruption/Rules/AffixCaps.cs`); movement-speed cap raised to 35% (71 tests). No gameplay change yet.
- Affix table: added Magnetic, Charring, Fleet, Foraging, Bloodforged, Deathless, Shieldwall, Rallying, Skald's, Steadfast, Reprisal, Wrath, Skadi and Fenrir (79 affixes); new `requires` and `lower_is_better` columns and an `any` pool; name clashes fixed and now rejected by the validator (68 tests). Not used in game yet; no gameplay change.

### 2026-10-27
- Drop tables approved: stars and rip tier 1 now raise Magic and Rare chances too; proposed rip, scar and terror-zone values accepted. If a kill rolls both Magic and Rare, only the Rare drops (`DropTable.Roll`, 67 tests). No gameplay change yet.
- Added the drop chance tables (`Corruption/Data/drop_chances.csv`, `drop_modifiers.csv`) with a reader and chance calculation (`Corruption/Rules/DropTable.cs`) and tests (65 tests), plus a guide to the data folder. Not used in game yet; no gameplay change.
- Added `Corruption/Data/affixes.csv`, the single table of every affix (gathering and gear), with a validator (`Corruption/Rules/AffixTable.cs`) and tests that check the real file (60 tests). Not used in game yet; no gameplay change.
- Added handover docs: `docs/CURRENT_STATE.md`, `DECISIONS.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`, and a rewritten `CLAUDE.md` with an update routine.
- Decided: our own loot system, not Epic Loot.
- Gathering tool affixes specified (universal, mining and woodcutting pools for pickaxes and one-handed woodcutting axes) in `docs/AFFIX_RULES.md`, with eligible tools from the game data and how each affix maps onto the game code. Loot rules agreed: crafting makes vanilla items, orbs upgrade them, monsters drop rolled items; Magic 1+1, Rare 2+2 affixes; tool affix ranges are the same for every tool. Then: Magic 1 in 50 and Rare 1 in 100 monster drops (tunable); the same-range rule extended to all gear; Odin's Grace made a whole-tree / touching-chunks harvest; Jotun Bane, Thunderblood and Frostfire axes eligible; seven low-impact affixes approved for variety; every one-handed axe rolls both gathering and weapon affixes; flat-value affixes become a % of the item's stat where possible, with one-range numbers proposed for general gear. Not built (loot system, phase 7).
- Drafted the gear affix pool (prefixes, suffixes, biome tiers, twin resonance, range display), now in `docs/AFFIX_RULES.md`.
- Saved the storage and quest systems brief (`docs/storage-quests-design.md`) for later.
- No mod code changed.

### 2026-10-26
- **Talismans (design phase 2):** one charm per creature family, worked out from game data. It repels that family from the wearer unless provoked. The family rules and exclusions are unit-tested against the real 1.0 creature list (53 tests in total). Not yet tested in-game.
- Fixed the raven repeatedly returning to the Riftforge: its replacement text kept the game's "always show" default.
- Fixed constant `IndexOutOfRangeException` errors from the build menu: the Riftforge recipe had 6 ingredients, and building pieces support at most 5. Leather scraps removed.
- **Riftforge tested in-game.** The raven now gives Riftforge/Ginnungagap lore instead of the Galdr table's, and particle effects and lights are tinted too, so the glow turns red rather than pink.
- The Riftforge is now red to match the shard (was purple); its colour and strength are configurable.
- **Corruption play-test:** the mod loads and the shard spawns. The shard is now deep red (Bloodstone model with a red tint, configurable look). The tint now covers every colour property, which also fixes glow colours staying unchanged.
- **Corruption plugin started (design phase 1):** new `Corruption` project with the corruption shard (drops from starred creatures and on first boss kills) and the Riftforge station. Shard drop rules are unit-tested; nothing has been tested in-game yet.
- Building with materials from nearby chests confirmed working in-game. Added `docs/multiplayer-test.md` for the two-player test.
- **Second dump analysed:** the biome fixes worked (913 items placed, 2 unplaced). Wisp fountains added to the producer rule. Reference data in `data/game-dump-1.0.20/` replaced with this corrected dump.
- **Third play-test:** the search box, the grouping button and crafting from nearby chests all work in-game. Category headers now line up with the recipe icons and use the recipe names' font size, and there's a new `HeaderFontSize` setting.
- **First in-game dump analysed** (Valheim 1.0.20). Biome detection works for the main materials. Fixed the gaps it showed: spawn entries that apply everywhere, boss phases, creature-only tool tiers (Bloodgold), eggs, wisps and trader quest keys. Details under data-driven biome detection. The dump's reference data is saved in `data/game-dump-1.0.20/`.
- **Second play-test:** the inventory and crafting screens work, categories collapse and expand, and `co_dump` writes its files. The search box didn't appear (see its known issues). Fixed the likely cause and stopped header labels from wrapping. Chest crafting and biome grouping haven't been tested yet.
- **First play-test (Valheim 1.0.20): the inventory and crafting screens were broken** (empty panels, placeholder text, no mouse cursor) and `co_dump` failed. Cause: the biome code used C# tuples, which need `System.ValueTuple.dll`, and Valheim doesn't ship it. The first recipe-list update crashed inside the game's own "open inventory" code. Fixed by removing the tuples. All features stay "Built, untested" until re-tested.
- The build now fails if the mod references an assembly Valheim doesn't ship (`System.ValueTuple`, `System.Memory`, `System.Buffers`).
- Mod code running inside game methods (recipe list, search bar, biome detection, chest counting and taking) now catches its own errors: it logs them once and falls back to vanilla behaviour instead of breaking the game's UI.

### 2026-10-15
- Added the Corruption mod design (`docs/corruption-design.md`) and its technical review (`docs/corruption-design-review.md`), and linked them from the planned Loot system and Combat mechanics entries. No mod code changed.

### 2026-09-29
- Biome grouping now works out material biomes from the game's own data (vegetation, spawns, locations and dungeon rooms, stations, crops, recipes), replacing the hand-written material list. Covers the Deep North and modded items. Added an Ocean group.
- `BiomeMaterialOverrides` is now only for corrections; it no longer extends a built-in list.
- Added `co_dump` (game data to CSV) and `co_biome` (explain an item's biome) console commands.
- Added `CraftingOverhaul.Tests` with unit tests for the biome rules.
- Added `docs/coop-networking.md` on co-op lag and desync. No mod code changed for this.

### 2026-09-28
- Added FEATURES.md to track features, status, config and patch targets.

### 2026-09-27: 0.1.0
- Turned the Jötunn mod stub into CraftingOverhaul (SDK-style project, JotunnLib 2.30.2, publicized game assemblies).
- Added the recipe search box, collapsible recipe categories (by item type or biome) and crafting/building from nearby chests. Compiled against current game reference assemblies; not yet tested in-game.
