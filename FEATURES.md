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
| [Game data dump and biome inspector](#game-data-dump-and-biome-inspector) | Dev tools | Tested in-game | — (console commands `co_dump`, `co_biome`) | `Dump/*`, `Biomes/Collectors/LocationCollector.cs`, `CraftingOverhaul.cs` | 2026-10-28 |
| [Config reload on save](#config-reload-on-save) | Dev tools | Built, untested | Both plugins: `0 - Development` → `ReloadConfigOnFileChange` | `Shared/ConfigWatcher.cs`, `CraftingOverhaul/CraftingOverhaul.cs`, `Corruption/CorruptionPlugin.cs`, both `.csproj` | 2026-10-28 |
| [Corruption shards](#corruption-shards) | Loot | Tested in-game | Corruption: `1 - Corruption Shards` → `OneStarChance`, `TwoStarChance`, `BossShardsMin`, `BossShardsMax`, `BossShardsFirstKillOnly`; `2 - Look` → `ShardCustomModel`, `ShardGlowIntensity`, `ShardGlowRange`; `2 - Look` → `ShardBaseItem`, `ShardColor`, `ShardTintStrength` | `Corruption/Content/ShardItem.cs`, `Corruption/Content/Corrupted.cs`, `Corruption/Content/ShardDrops.cs`, `Corruption/Rules/ShardRules.cs`, `Corruption/CorruptionConfig.cs`, `CraftingOverhaul.Tests/ShardRulesTests.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Rules/ObjMesh.cs`, `Corruption/Assets/*`, `tools/make_shard.py` | 2026-10-28 |
| [Riftforge](#riftforge) | Crafting | Tested in-game | Corruption: `2 - Look` → `RiftforgeColor`, `RiftforgeTintStrength` | `Corruption/Content/Riftforge.cs`, `Corruption/Content/Corrupted.cs`, `Corruption/Content/Texts.cs` | 2026-10-26 |
| [Talismans](#talismans) | Combat | Tested in-game (single player) | Corruption: `3 - Talismans` → `Enabled`, `Slots`, `ProvokeSeconds`, `ExcludedCreatures` | `Corruption/Content/Talismans.cs`, `Corruption/Content/TalismanSlots.cs`, `Corruption/Content/TalismanRepel.cs`, `Corruption/Rules/TalismanFamilies.cs`, `Corruption/Rules/TalismanSlotRules.cs`, `Corruption/CorruptionDefaults.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Content/Texts.cs`, `CraftingOverhaul.Tests/TalismanFamiliesTests.cs`, `CraftingOverhaul.Tests/TalismanSlotRulesTests.cs` | 2026-10-28 |
| [Boss essences](#boss-essences) | Loot | Built, untested | Corruption: `4 - Essences` → `Enabled`; table `Corruption/Data/essences.csv`; glow uses `2 - Look` → `ShardGlowIntensity`, `ShardGlowRange` | `Corruption/Content/EssenceItems.cs`, `Corruption/Content/EssenceDrops.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Rules/EssenceTable.cs`, `Corruption/Data/essences.csv`, `Corruption/Assets/essence*`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `tools/make_essences.py`, `CraftingOverhaul.Tests/EssenceTableTests.cs` | 2026-10-28 |
| [Maps and rips (step A)](#maps-and-rips-step-a) | Corruption | Built, untested | Corruption: `5 - Rips` → `Enabled`, `OpenDays`, `BaseDistance`, `HeartHealth`; table `Corruption/Data/maps.csv` | `Corruption/Content/MapItems.cs`, `Corruption/Content/Rips.cs`, `Corruption/Content/RipObjects.cs`, `Corruption/Content/RipPrefabs.cs`, `Corruption/Content/DataFiles.cs`, `Corruption/Content/GemModel.cs`, `Corruption/Content/Texts.cs`, `Corruption/Rules/MapList.cs`, `Corruption/Rules/RipRules.cs`, `Corruption/Data/maps.csv`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs`, `Corruption/Corruption.csproj`, `CraftingOverhaul.Tests/MapListTests.cs` | 2026-10-28 |
| [Corrupted bosses (core)](#corrupted-bosses-core) | Combat | Built, untested | Corruption: `6 - Corrupted Bosses` → `DamageDealt`, `DamageTaken`, `EssencesMin`, `EssencesMax`, `ShardsMin`, `ShardsMax`; `2 - Look` → `CorruptedBossTintStrength`; console `co_corrupt` | `Corruption/Content/CorruptedBosses.cs`, `Corruption/Content/EssenceDrops.cs`, `Corruption/Content/ShardDrops.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/CorruptionPlugin.cs` | 2026-10-28 |
| [Loot system](#loot-system) | Loot | Planned | — | — | 2026-10-27 |
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
- Play-test 2026-10-26: collapsing, expanding and both grouping modes work. Header text was smaller than the recipe names and indented too far; headers now start where the item icon sits and use the recipe names' font size (adjustable with `HeaderFontSize`). Awaiting re-test. The first header styled logs the row's font settings (`Recipe row font: ...`) in case the size needs tuning.

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
| `item_details.csv` | Every item's full shared data, field by field (upgrade amounts, knockback, backstab, parry, set bonuses, equip effects, modifiers), plus attack stamina and per-level damage |
| `recipes.csv` | Every recipe: output, station and level, ingredients (with per-level amounts), type and biome category |
| `pieces.csv` | Every buildable piece: category, station, build cost |
| `creatures.csv` | Every creature: health, faction, boss flag, defeat key, spawn biomes, drops with amounts and chances |
| `locations.csv` | Every location: biome, quantity, unique |
| `spawns.csv` | The world spawn tables: every entry with all its settings (biome, max spawned, interval, chance, group size, levels, day/night, required keys) |
| `events.csv`, `event_spawns.csv` | Random events (raids): duration, biomes, required keys, messages; and each raid's spawn entries with all their settings |
| `status_effects.csv` | Every status effect: class and every number it sets |
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
- Names in the CSVs use the game's current language.
- The dump is game data owned by Iron Gate. It's fine for development, but don't ship it with the mod.

**Vanilla behaviour overridden:** None. Registers the two console commands through Jötunn's `CommandManager`.

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
- Untested in-game (81 tests pass). Risks to check: textures or the add-on parts not showing, essences lying oddly (they keep Bloodstone's collider), icons.
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
- **Maps.** Eight maps, one per biome (Corrupted Meadows map … Corrupted Deep North map), crafted at the **Riftforge** from 1 corruption shard, 1 of that biome's boss essence and a stack of a biome material (Leather scraps 5, Greydwarf eye 5, Guck 2, Freeze gland 3, Needle 5, Carapace 16, Charred bone 8, Ice 5). Where each comes from: [Data table](#maps-and-rips-step-a) below. Like vanilla recipes, a map shows up once you've picked up all three. Maps don't stack, go through portals and can't be sold. Placeholder look: a bundle of leather tinted in the essence's colour.
- **Opening a rip.** Use the map (right-click it in your inventory, or from the hotbar). Your game looks for land of that biome **you haven't explored** (your map, including what you've added from a cartography table), above water and as flat as the game requires for placing a location; the server then picks the first spot outside a ward's protected radius (`BaseDistance`) from **anyone's** bed, ward or portal. The map is used up and a red stone portal, the **rip**, opens 4 m in front of you. If nothing suitable is found you're told why and keep the map.
- **Through the rip.** Walk in (or press E) to travel to the landing spot. Vanilla portal rules apply: no ores or metals. Anyone can follow while it's open. You land beside the **rip heart**, a giant glowing corruption shard.
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
- Untested in-game (89 tests pass). Riskiest parts: the far-away heart appearing at the landing spot, landing on the ground (not inside a rock or on a tree), the stone portal's look and swirl without its vanilla logic, the heart's model on the greydwarf-nest base.
- The landing spot comes from world generation. Locations (villages, altars) aren't avoided yet; step B adds checks once the area has loaded.
- The map look is a placeholder until map art is asked for.
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
- Untested in-game. The only way to make a corrupted boss for now is `co_corrupt`.
- The design's extra particles (Bonemass poison cloud, Ashlands embers) aren't added yet: which particle prefab to reuse comes from the game data with the summons.

**Vanilla behaviour overridden (Harmony patch targets):**

| Target | Patch | Purpose |
|---|---|---|
| `Character.Awake` | Postfix | Adds a watcher to bosses that tints them red once they're corrupted |
| `Character.RPC_Damage` | Prefix | Scales hits on and from corrupted bosses |
| `CharacterDrop.GenerateDropList` | Postfix (the existing shard and essence ones) | Corrupted bosses: 3–5 shards and 2–3 essences |

---

## Loot system

**Status:** Planned. Not started.

**Ideas so far:**
- Rarity tiers for dropped gear, with randomly rolled bonus stats.
- Stats stored on the item (`ItemDrop.ItemData.m_customData`), shown in tooltips and synced in multiplayer.
- Likely patch points: `CharacterDrop` / `DropTable` for drops, and item tooltips.

Decided 2026-10-27: we build our own loot system rather than depend on Epic Loot.

**Design:** now covered by the Corruption mod design in `docs/corruption-design.md`:
- corruption shards, maps and rips;
- corrupted bosses;
- rarity tiers, currency orbs and uniques;
- gear prefixes and suffixes (draft), and the gathering tool affixes that come first: `docs/AFFIX_RULES.md`.

The technical review is in `docs/corruption-design-review.md`. It suggests building the corruption mod as a separate plugin in this repository.

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

### 2026-10-28
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
