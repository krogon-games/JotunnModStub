# Architecture

How the code is organised and the game facts it relies on. Feature behaviour and exact Harmony patch targets are in [`FEATURES.md`](../FEATURES.md); the reasons behind choices are in [`DECISIONS.md`](DECISIONS.md).

## Stack

- Valheim 1.0.20 (Unity 6, Mono), BepInEx 5, Jötunn (JotunnLib 2.30.2), HarmonyX.
- SDK-style `net462` projects with `Microsoft.NETFramework.ReferenceAssemblies`; game assemblies are publicized by Jötunn's prebuild (`DoPrebuild.props`, `ExecutePrebuild=true`), with `IgnoreAccessModifiers.cs` so private members can be used directly.
- `Directory.Build.props` sets `SolutionDir`; `publish.ps1` runs after a Windows build: Debug copies the DLL to `MOD_DEPLOYPATH`, Release zips a Thunderstore package from `<Project>/Package/`.
- Reference game data: `data/game-dump-1.0.20/` (CSV output of `co_dump`, development only, never shipped).

## Solution layout

| Project | GUID | Network compatibility | Purpose |
|---|---|---|---|
| `CraftingOverhaul/` | `com.krogongames.craftingoverhaul` | `VersionCheckOnly` (works if only some players have it) | Quality of life: recipe search, collapsible categories, biome detection, craft/build from nearby chests, `co_dump` / `co_biome` |
| `Corruption/` | `com.krogongames.corruption` | `EveryoneMustHaveMod` (server and every player) | Endgame mod from [`GAME_DESIGN.md`](GAME_DESIGN.md): shards, Riftforge, talismans so far |
| `CraftingOverhaul.Tests/` | — | — | `net8.0` xunit tests for both plugins' pure code |

### CraftingOverhaul

- `CraftingOverhaul.cs`: plugin entry, `PatchAllSafely()`, console commands.
- `ModConfig.cs`: `1 - Recipe List` (per player), `2 - Nearby Containers` (admin-only, server-synced), `3 - Character Sheet` (per player).
- `RecipeList/`: `RecipeListOrganizer` (postfix on `InventoryGui.UpdateRecipeList`; computes rows first, then mutates the list), `SearchBar`, `RecipeCategories`.
- `CharacterSheet/`: `SheetLayout` (pure, tested: where each EquipmentAndQuickSlots slot goes), `FlatJson` (pure, tested: reads that mod's JSON API answers), `CharacterSheetPanel` (postfix after EAQS's on `InventoryGrid.UpdateGui`: moves EAQS's slot cells onto our wood panel, stats box, drag-to-move). Pattern: **re-use another mod's UI objects instead of replacing them**: EAQS keeps owning the cells and the items; we only set their positions after it did.
- `Biomes/`: data-driven biome detection.
  - `Collectors/` turn game data into *sources* (vegetation, spawns, locations and dungeon rooms, conversions, producers, hatching, recipes); `PrefabWalker` reads drop/harvest components.
  - `BiomeSolver.cs` (pure, tested): an item's biome is its earliest source; a source's biome is the latest of its location, requirements, tool tier and boss key; weak sources (random chest loot) only place items nothing else places; config overrides are fixed points.
  - `BiomeResolver.cs`: orchestration and caching. The Corruption design plans to reuse it (affix tiers, unique item levels, map materials).
- `Containers/`: `NearbyContainers` (tracks containers from `Container.Awake`, filters unusable ones, claims ownership before taking), `ContainerPatches` (count/remove patches scoped to crafting and building code paths).
- `Dump/`: `co_dump`, `co_biome`, CSV writer.

### Corruption

- `CorruptionPlugin.cs`: registers content in `PrefabManager.OnVanillaPrefabsAvailable`, each step inside `Safe.Run`.
- `CorruptionConfig.cs`: gameplay sections admin-only; `2 - Look` per player. `CorruptionDefaults.cs` holds defaults shared with tests.
- `Rules/` (pure, tested): `ShardRules`, `TalismanFamilies`, `AffixTable`, `AffixCaps`, `DropTable`, `CorruptionOutcomes` and `ShatterRules` (read and validate the data tables; `DropTable.Chance` combines base chance and modifiers), `Csv` (shared reader).
- `Data/`: tunable data tables kept as CSV, one row per entry, so they open in any spreadsheet and diff cleanly in git: `affixes.csv`, `affix_caps.csv`, `drop_chances.csv`, `drop_modifiers.csv`, `corruption_outcomes.csv`, `shattering.csv` (guide: `Corruption/Data/README.md`). Each table has a pure reader with validation, and a test that checks the real file.
- `Content/`: `ShardItem`, `Riftforge`, `ShardDrops`, `Talismans`, `TalismanRepel`, `Corrupted` (tint + icon rendering), `Texts` (localisation tokens, raven lore).

### Shared

- `Shared/ConfigWatcher.cs`: linked into both plugin projects (`<Compile Include="..\Shared\..." Link=...>`). Reloads the plugin's `.cfg` on save; development aid, see `CURRENT_STATE.md` → Before release.

### Custom assets

- `tools/make_shard.py` and `tools/make_essences.py` (Python, numpy + Pillow) generate the gem models (`Corruption/Assets/*.obj`), textures (PNG for viewing, raw `.rgba` for the game) and previews in `docs/images/`, from fixed seeds. Rerun after changing them; commit the outputs.
- `tools/make_orbs.py` makes the currency orbs into `Corruption/Assets/Orbs/`, which the wildcard deliberately doesn't cover until the orbs are built (add `Assets\Orbs\*` then). It paints patterns in 3D per texel, so runes don't stretch on the sphere's UV map.
- `Corruption.csproj` embeds every `Assets/*.obj` and `Assets/*.rgba` (wildcard) plus `Data/essences.csv`. `Rules/ObjMesh.cs` (tested) reads the models; `Content/GemModel.cs` builds the Unity mesh and material on the base item's shader, adds optional plain-colour add-on parts and the ground glow, and removes the base item's visuals (colliders stay). A failed look always falls back to a tint, so the item still registers.
- Textures are raw pixels because `Texture2D.LoadImage` (ImageConversion module) references netstandard 2.1, which the net462 build can't compile against. They make up most of the DLL's size (about 4 MB).

### Server messages (rips)

- Requests that need the whole world go to the server with `ZRoutedRpc` (registered in a `Game.Start` postfix): the player's game sends `Corruption_RipRequest` to `GetServerPeerID()`, the server answers the sender with `Corruption_RipReply`. In single player and on a host the "server" is the same game; routed RPCs to yourself are handled locally.
- Only the server holds every object in the world (`ZDOMan.GetAllZDOsWithPrefabIterative` covers them all there), so checks like "near anyone's base" belong on the server.
- Objects for far-away places (the rip heart) are instantiated there directly: their persistent ZDO stays when the instance is removed as out of range, and they appear when someone arrives. Settle them on the ground once the area is loaded.
- Rip state lives on the objects' ZDOs (`corruption_rip_*` keys in `Rips.cs`), so it survives restarts and works for every player.

## Code patterns (follow these)

1. **Isolated patches.** `PatchAllSafely()` applies each `[HarmonyPatch]` class separately inside try/catch, so one broken target doesn't disable the rest.
2. **Never break vanilla.** Code that runs inside game methods goes through `Safe.Run` / `Safe.Report`: log once per action, fall back to vanilla behaviour.
3. **Pure rules + tests.** Anything decidable without Unity goes in a pure file (no `UnityEngine`), linked into `CraftingOverhaul.Tests` by `<Compile Include=... Link=...>`, and unit-tested. Tests may read `data/game-dump-1.0.20/*.csv` for real game data.
4. **Data-driven first.** Derive lists from game data (biomes, talisman families); config overrides only for real exceptions (`BiomeMaterialOverrides`, `ExcludedCreatures`).
5. **Clone, don't import** (exceptions: the shard and essence models, above). New items/pieces are Jötunn clones of vanilla prefabs, recoloured with `Corrupted.Tint` (all shader colour properties, particle start colours, lights). No new models or animations in v1.
6. **Config scope.** Gameplay numbers admin-only (server-synced); visual settings per player.
7. **Robust patch arguments.** Use `__0`/`__1` positional arguments where parameter names may change.
8. **No hard-coded positional limits without a guard** (e.g. `Riftforge` refuses more than 5 ingredients and logs an error).

## Build and test

- Windows: open `CraftingOverhaul.sln` and build; see `README.MD` (Valheim install found via Steam or `Environment.props`).
- Command line (any OS, needs Valheim's managed assemblies at `VALHEIM_INSTALL`):
  `dotnet build Corruption/Corruption.csproj -p:VALHEIM_INSTALL=<path> -p:ExecutePrebuild=false`
- Tests (no game needed): `dotnet test CraftingOverhaul.Tests`.
- Without a Valheim install (e.g. a cloud session), a stand-in install can be built from NuGet: game, Unity, Harmony and BepInEx DLLs from `Pfhoenix.Valheim.ModProjectReferences` 1.0.26 (`lib/net46/`) into `valheim_Data/Managed/`, publicized game DLLs from `Digitalroot.Valheim.Common.References` 1.0.21 renamed to `publicized_assemblies/<name>_publicized.dll`, and `0Harmony.dll` plus `bepinex.dll` (also as `BepInEx.dll`) in `BepInEx/core/`. Both plugins compiled this way on 2026-10-27.
- **Build check `CheckNoUnshippedReferences`**: each plugin's build fails if the DLL references `System.ValueTuple`, `System.Memory` or `System.Buffers`. Valheim's Mono doesn't ship them; C# tuples crashed the game's inventory UI (2026-10-26).
- In-game checks: `co_dump` (CSV of items, recipes, pieces, creatures, locations, biome data) and `co_biome <item>`.

## Game facts the code relies on (verified against 1.0 code unless noted)

Full 1.0.14 game code (method bodies, not just signatures) comes from the NuGet package `Pfhoenix.Valheim.ModProjectReferences` 1.0.26 (`lib/net46/assembly_valheim.dll`), decompiled with `ilspycmd`. Reference-only packages (`ValheimGameLibs`, `Digitalroot.Valheim.Common.References`) have signatures only.

**Multiplayer**
- The player whose game owns an area simulates its creatures, AI and physics; the dedicated server only relays and stores. "Server-authoritative spawning" is not how vanilla works. Details: [`coop-networking.md`](coop-networking.md).
- Harvesting (trees, logs, stumps, rocks, deposits): `Damage()` on the striker's machine only sends the hit by RPC; the object's owner applies it, decides the break and spawns the drops. Changes to the hit must be made striker-side; extra drops need a message back to the striker.
- Items in EquipmentAndQuickSlots custom slots (our talisman slots) live only in the owner's inventory; other games can't see them. What other games need to know (worn talismans) is published on the player's own ZDO, which the player owns and every game receives.
- Taking from a chest requires owning it: `NearbyContainers` claims ownership right before removing items.
- Other players' trinkets are readable on any client via `VisEquipment.m_currentTrinketItemHash` (synced through the ZDO), which is how talismans work for remote players.

**Items, pieces, recipes**
- A building piece supports at most **5 ingredients**: `Hud.SetupPieceInfo` indexes `m_requirementItems[resources.Length]`.
- Pieces appear in the build menu once the station is known and every ingredient is in `m_knownMaterial` (picked up at least once). Recipes follow the same pickup rule.
- `Piece.m_requireOnlyOneIngredient` makes every ingredient an alternative (all or nothing).
- `ItemDrop.ItemData.m_customData` exists in 1.0 and is saved with the item (planned storage for affixes).
- Trinket and Utility slots exist (`Humanoid.m_trinketItem`, `m_utilityItem`).

**Creatures and AI**
- `BaseAI.IsEnemy(Character, Character)` decides both targeting and whether attacks hit, including the player's own attacks, so a patch must only change the creature-asking-about-player direction.
- `EnemyHud` only has star icons for 2 levels (`m_level2`, `m_level3`); 3-star creatures need a HUD patch.
- `RandEventSystem` runs one event for the whole world at a time.
- `OfferingBowl` accepts one `m_bossItem` type; the same code handles item-stand altars (`m_useItemStands`). From game knowledge (unverified): the Queen has no altar.

**World**
- `WorldGenerator.GetBiome(x, z)` / `GetHeight(x, z)` work without loading the area; `Minimap.IsExplored(pos)` too.
- Raven messages: `RavenText.m_alwaysSpawn` defaults to true; `tutorialreset` (not `resetknownitems`) replays them.

**Data notes (from the 1.0.20 dump)**
- Bloodgold ore needs tool tier 6; no player pickaxe reaches it, only creature attacks (inferred: Snow Troll). Unconfirmed in-game.
- `FaderEmber` has no source in the game data.
- 3 traders: Haldor, Hildir, Bog Witch.
