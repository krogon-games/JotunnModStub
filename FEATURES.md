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
| [Character sheet](#character-sheet) | UI | Tested in-game (single player) | `3 - Character Sheet` → `Enabled`, `Position`, `ShowStats`, `ShowQuickSlots`, `ShowCharacter` | `CharacterSheet/CharacterSheetPanel.cs`, `CharacterSheet/CharacterPortrait.cs`, `CharacterSheet/SheetArt.cs`, `CharacterSheet/SheetLayout.cs`, `CharacterSheet/FlatJson.cs`, `Assets/Sheet/*`, `CraftingOverhaul.csproj`, `ModConfig.cs`, `CraftingOverhaul.cs`, `tools/make_sheet_ui.py`, `CraftingOverhaul.Tests/SheetLayoutTests.cs` | 2026-10-29 |
| [Loot system](#loot-system) | Loot | Built, untested (step 1: drops, tooltips, item stats) | Corruption: `7 - Loot` → `Enabled`; console `co_loot`; tables `Corruption/Data/affixes.csv`, `loot_bases.csv`, `loot_rules.csv`, `rare_names.csv`, `drop_chances.csv`, `drop_modifiers.csv` | `Corruption/Content/Loot.cs`, `Corruption/Content/LootPatches.cs`, `Corruption/Rules/LootBases.cs`, `Corruption/Rules/LootRules.cs`, `Corruption/Rules/LootItem.cs`, `Corruption/Rules/AffixRoller.cs`, `Corruption/Data/loot_bases.csv`, `Corruption/Data/loot_rules.csv`, `Corruption/Data/rare_names.csv`, `tools/make_loot_bases.py`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/LootTests.cs` | 2026-10-29 |
| [Combat mechanics](#combat-mechanics) | Combat | Planned | — | — | 2026-10-15 |

There are two plugins: **CraftingOverhaul** (quality of life; works when only some players have it) and **Corruption** (the endgame mod from `docs/corruption-design.md`; the server and every player need it). File paths are relative to `CraftingOverhaul/` unless they start with `Corruption/` or `CraftingOverhaul.Tests/`. Config settings are in `BepInEx/config/com.krogongames.craftingoverhaul.cfg` and `com.krogongames.corruption.cfg`.

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

- **Look:** dark wood planks in an iron frame with riveted corners; a title band with rope knots and "CHARACTER" in the game's Norse font; a close button (closes the inventory). The art is generated by `tools/make_sheet_ui.py` and embedded in the DLL.
- **Left column**, top to bottom: HEAD, CHEST, LEGS, CAPE (the shoulder slot: cape or AdventureBackpacks backpack), UTILITY, then EAQS's extra utility slots if its utility count is raised.
- **Right column**: TRINKET, then every slot other mods add through EAQS's API in the order they were added: our TALISMAN slots (2 by default), and later the Ring slot. Unknown slots keep their mod's name.
- Each slot is a recessed, iron-rimmed tile with its label underneath. An empty slot shows a faint picture of what goes there (the icon of a vanilla item: Bronze helmet, Bronze plate tunic and leggings, Deer hide cape, Megingjord, Bronze pendant, and the deer trophy for talismans). EAQS's own labels and the blue "equipped" backing are hidden in these slots.
- **Middle: a live picture of your character** in an arch, standing on a stone pedestal: your real model, gear and idle animation, filmed from the front by a camera of our own. **Drag it sideways to turn the view** round the character. Lit by a light of our own as well as the world's, so it shows at night.
- **Stats** on a parchment under the columns, with your name on a ribbon: armor, weight (red when over), max health, stamina and eitr (eitr only once you have some; values in the game's health/stamina/eitr colours), movement speed from gear (sum of your equipment's movement modifiers, as the game applies it), and on the right your active status effects with their icons (up to 6; "+N more" beyond). Refreshed four times a second.
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
| `ShowStats` | `true` | Show the parchment stats box |
| `ShowQuickSlots` | `true` | Put the quick slots in the row at the bottom. Off: EAQS places them where it always did |
| `ShowCharacter` | `true` | Show the live picture of your character. Off: the arch shows an empty night-blue backdrop |

The layout, labels and silhouettes are unit-tested (`SheetLayoutTests`, `FlatJsonTests`: 24 tests).

**Known issues:**
- Play-test 2026-10-29 (first version, single player, with EAQS, AdventureBackpacks and PlantEasily): the sheet appeared with every slot in place and the stats right, but looked too plain next to the concept image, the character frame was empty, and at `615, 28` the sheet covered the inventory's weight number. **Reworked the same day** (art, labels, silhouettes, live picture, new default position); awaiting re-test.
- **Play-test 2026-10-29 (second version):** the look matches the mock-up (frame, title, labelled tiles, silhouettes, parchment, ribbon) and the live picture works, (the side-on view in the screenshot was the user turning it by dragging, which works). Its feet stood a little above a stone that was too thin, and a talisman kept the blue backing. **Fixed the same day** (awaiting re-test): the picture starts from the front each time the inventory opens; the character is a little smaller with a bigger, lighter stone under its feet; the light is brighter and comes from one side; no blue backing in any labelled slot.
- **Play-test 2026-10-29 (third version): "looking much better" (user).** The character faces front, whole body in view, feet on the stone, lit at night; slots, labels, silhouettes and stats as designed.
- **To revisit later (user, 2026-10-29):** the sheet's look is a bit inconsistent with the rest of the game's UI (its darker wood, iron and parchment next to the vanilla panels). Not changed for now.
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
- **Play-test 2026-10-28 (single player): passed.** `co_corrupt` turns Eikthyr red; it hits harder, takes longer to kill and drops 2–3 essences and 3–5 shards. The only way to make a corrupted boss for now is `co_corrupt`.
- The design's extra particles (Bonemass poison cloud, Ashlands embers) aren't added yet: which particle prefab to reuse comes from the game data with the summons.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Character.Awake` | Postfix | Adds a watcher to bosses that tints them red once they're corrupted |
| `Character.RPC_Damage` | Prefix | Scales hits on and from corrupted bosses |
| `CharacterDrop.GenerateDropList` | Postfix (the existing shard and essence ones) | Corrupted bosses: 3–5 shards and 2–3 essences |

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
- **Tooltips:** after vanilla's lines, the rarity (and for Rares the base item), then one line per affix with its range in brackets, e.g. `+24% damage [5–40]`; resonant lines end in `◆ resonant`. Affixes this step doesn't apply yet are grey and say "(not active yet)".
- **Affixes that work in this step** (through the game's own stat getters, so vanilla tooltips, combat and repairs see them): enhanced damage (every combat damage type, not chop or pickaxe damage), fire/frost/lightning/poison/spirit damage (a % of the enhanced physical damage), armour, durability, block power, attack stamina (also the tools' "per swing" version), knockback and backstab (raised on the attacker's game when the hit is made), parry bonus.
- **Storage:** `ItemData.m_customData` keys `corruption.rarity`, `corruption.name` (Rares) and `corruption.affixes` (`id=value;prefix+suffix=value`), saved and synced with the item by the game.

**How to test:** `co_loot` drops a Rare item of the biome you stand in; `co_loot magic` a Magic one; `co_loot rare SwordBronze` a chosen base item (any prefab in `loot_bases.csv`). Kill starred creatures for natural drops (1 in 50 / 1 in 100 per kill is slow; `co_loot` is the quick way).

**Config settings** (admin-only, `7 - Loot`):

| Setting | Default | Description |
|---|---|---|
| `Enabled` | `true` | Creatures drop Magic and Rare items. Off: no new drops; items already found keep their affixes |

**Data tables** (compiled into the DLL; rebuild after editing): `affixes.csv`, `loot_bases.csv` (rerun `tools/make_loot_bases.py` after a new game dump), `loot_rules.csv` (previous-biome chance, drop level, affix-count weights), `rare_names.csv`, `drop_chances.csv`, `drop_modifiers.csv`. `dotnet test` checks all of them, and that `loot_bases.csv` matches the dump.

**Known issues:**
- Untested in-game (146 tests pass; both plugins compile against the stand-in game assemblies).
- **First look 2026-10-29 (user's screenshots):** `co_loot` Rares work: "Night Ward" (Rare Leather Trousers: +15% armour, armour 2 → 2.3) and "Skull Song" (Rare Bronze Sword: +37% damage, slash 35 → 48), yellow titles, affix lines with ranges, inactive lines grey. Magic items work too: "Shieldwall Bronze Atgeir", "Seismic Antler Pickaxe" (gathering affix on a pickaxe), "Crackling Club" (+19% of physical as lightning: blunt 12 adds lightning 2), blue names. Still to check: ground hover names, durability, block, repair, upgrade, natural drops, combat affixes.
- 28 gear affixes still have their draft ranges (marked `draft` in `affixes.csv`); ranges from the game data are proposed separately for the user's approval.
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
- New animations would need asset-bundle work in the `JotunnModUnity` project.

The Corruption mod design (`docs/corruption-design.md`) makes fights harder (maddened creatures, corrupted bosses), but adds no new attacks or animations in v1. These combat ideas remain a separate track.

---

## Changelog

### 2026-10-29
- **Gear affix ranges from the game data** (loot; numbers `proposed`): enhanced damage 4–19%, armour 10–50%, durability 11–55%, block 4–18%, backstab, knockback and parry 20–100%, attack stamina 4–20%, health regen 6–30%, stamina regen 8–40%, carry weight 30–150, weapon skill 4–20, under the user's rules (gear-only sources; a full forge upgrade where vanilla has no +% source). **`co_dump` now writes resistances** (item `m_damageModifiers`, status effect `m_mods`) for the resistance affixes.
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
