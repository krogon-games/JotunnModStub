# Architecture

How the code is organised and the game facts it relies on. Feature behaviour and exact Harmony patch targets are in [`FEATURES.md`](../FEATURES.md); the reasons behind choices are in [`DECISIONS.md`](DECISIONS.md).

## Stack

- Valheim 1.0.20 (Unity 6, Mono), BepInEx 5, Jötunn (JotunnLib 2.30.2), HarmonyX.
- SDK-style `net462` projects with `Microsoft.NETFramework.ReferenceAssemblies`; game assemblies are publicized by Jötunn's prebuild (`DoPrebuild.props`, `ExecutePrebuild=true`), with `IgnoreAccessModifiers.cs` so private members can be used directly.
- `Directory.Build.props` sets `SolutionDir`; `publish.ps1` runs after a Windows build: Debug copies the DLL to `MOD_DEPLOYPATH`, Release zips a Thunderstore package from `<Project>/Package/`.
- Reference game data: `data/game-dump-1.0.21/` (CSV output of `co_dump`, development only, never shipped).

## Solution layout

| Project | GUID | Network compatibility | Purpose |
|---|---|---|---|
| `CraftingOverhaul/` | `com.krogongames.craftingoverhaul` | `VersionCheckOnly` (works if only some players have it) | Quality of life: recipe search, collapsible categories, biome detection, craft/build from nearby chests, `co_dump` / `co_biome` |
| `Corruption/` | `com.krogongames.corruption` | `EveryoneMustHaveMod` (server and every player) | Endgame mod from [`GAME_DESIGN.md`](GAME_DESIGN.md): shards, Riftforge, talismans so far |
| `Orlog/` | `com.krogongames.orlog` | `EveryoneMustHaveMod` | Diablo 2-style classes from [`class-design.md`](class-design.md): levels and XP so far |
| `CraftingOverhaul.Tests/` | — | — | `net8.0` xunit tests for every plugin's pure code |

### CraftingOverhaul

- `CraftingOverhaul.cs`: plugin entry, `PatchAllSafely()`, console commands.
- `ModConfig.cs`: `1 - Recipe List` (per player), `2 - Nearby Containers` (admin-only, server-synced), `3 - Character Sheet`, `4 - HUD`, `5 - Classes UI` (per player).
- `RecipeList/`: `RecipeListOrganizer` (postfix on `InventoryGui.UpdateRecipeList`; computes rows first, then mutates the list), `SearchBar`, `RecipeCategories`.
- `CharacterSheet/`: `SheetLayout` (pure, tested: where each EquipmentAndQuickSlots slot goes, labels, silhouettes), `FlatJson` (pure, tested: reads that mod's JSON API answers), `CharacterSheetPanel` (postfix after EAQS's on `InventoryGrid.UpdateGui`: moves EAQS's slot cells onto our panel, stats, drag-to-move), `CharacterPortrait` (live picture: own camera and render texture; swaps the player's renderers to an unused layer only during that camera's render, via `Camera.onPreCull`/`onPostRender`), `SheetArt` (loads the embedded art as sprites, 9-sliced where sizes vary; `StyleAsGamePanel` gives an image the game's own panel background by copying the inventory's `Bkg` image at runtime, as EAQS does, with our look-alike as the fallback). Pattern: **re-use another mod's UI objects instead of replacing them**: EAQS keeps owning the cells and the items; we only set their positions after it did.
- `Classes/`: the HUD and class screens from `docs/classes-ui-brief.md`. `SkillsPanelLayout` (pure, tested) holds every rectangle of the skills panel; a test writes it as JSON when `CO_PANEL_LAYOUT` is set and `tools/preview_skills_panel.py` draws a preview from it with the real art, so **previews come from the same numbers as the game** (pattern for future screens). `ClassesSource` (reads Orlog's `Orlog.Api` by reflection, like the sheet reads EAQS; the demo data was removed 2026-11-13), `SkillCardsLayout` (the tab's nine skill cards since 2026-11-15; before that `SkillTreeLayout`, the tree with tiers and paths) and `JsonOut` (pure, tested), `VitalBars` (HUD bars, food, XP), `SkillCross` (HUD), `SkillsPanel` and `ClassChoiceWindow` (windows on Jötunn's `CustomGUIFront`), `ClassWindows` (input block, Escape), `UiKit` (shared art, text and layout helpers). Pattern: **the UI only shows and asks; the Orlog plugin owns the rules** (actions answer `""` or a reason to show).
- `Biomes/`: data-driven biome detection.
  - `Collectors/` turn game data into *sources* (vegetation, spawns, locations and dungeon rooms, conversions, producers, hatching, recipes); `PrefabWalker` reads drop/harvest components.
  - `BiomeSolver.cs` (pure, tested): an item's biome is its earliest source; a source's biome is the latest of its location, requirements, tool tier and boss key; weak sources (random chest loot) only place items nothing else places; config overrides are fixed points.
  - `BiomeResolver.cs`: orchestration and caching. The Corruption design plans to reuse it (affix tiers, unique item levels, map materials). It never blocks the game (2026-11-13): it reuses the result saved in `BepInEx/cache/CraftingOverhaul.biomes.txt` (`BiomeCacheFile.cs`, pure, tested) while `BiomeCacheKey.cs` (game version, mods, build, overrides, game-data fingerprint) matches; otherwise `BiomeResolveJob.cs` runs the collectors as small steps, about 4 ms per frame (`SlicedWork.cs`, pure, tested), loads location and room prefabs with `SoftReference.LoadAsync` a few at a time, and runs the solver on a thread-pool thread. `TryGet` answers false until the result is ready; console commands call `EnsureResolved()` to finish on the spot.
- `Containers/`: `NearbyContainers` (tracks containers from `Container.Awake`, filters unusable ones, claims ownership before taking), `ContainerPatches` (count/remove patches scoped to crafting and building code paths).
- `Dump/`: `co_dump`, `co_biome`, CSV writer.

### Corruption

- `CorruptionPlugin.cs`: registers content in `PrefabManager.OnVanillaPrefabsAvailable`, each step inside `Safe.Run`.
- `CorruptionConfig.cs`: gameplay sections admin-only; `2 - Look` per player. `CorruptionDefaults.cs` holds defaults shared with tests.
- `Rules/` (pure, tested): `ShardRules`, `TalismanFamilies`, `AffixTable`, `AffixCaps`, `DropTable`, `CorruptionOutcomes`, `ShatterRules`, `LootBases`, `LootRules` (read and validate the data tables; `DropTable.Chance` combines base chance and modifiers), `AffixRoller`, `AffixTotals` (player-stat totals and caps), `HarvestRules` (on-harvest drop-list changes, Gemseeker's pool) and `LootItem` (roll a Magic/Rare item; its `m_customData` format, names and tooltip lines), `Csv` (shared reader).
- `Data/`: tunable data tables kept as CSV, one row per entry, so they open in any spreadsheet and diff cleanly in git: `affixes.csv`, `affix_caps.csv`, `drop_chances.csv`, `drop_modifiers.csv`, `corruption_outcomes.csv`, `shattering.csv` (guide: `Corruption/Data/README.md`). Each table has a pure reader with validation, and a test that checks the real file.
- `Content/`: `ShardItem`, `Riftforge`, `ShardDrops`, `Talismans`, `TalismanRepel`, `Corrupted` (tint + icon rendering), `Texts` (localisation tokens, raven lore), `BetaItems` (clears the Steam-entitlement tag on the Odin items), `Loot` (loot tables, item creation, `co_loot`) `LootPatches` (drops, tooltips, item-stat affixes), `LootGathering` (gathering-tool affixes on the striker's swing), `LootHarvest` (on-harvest gathering affixes and Odin's Grace on trees: chances sent to the object's owner, who changes the drop list), `LootPlayerStats` (player-stat affixes over the local player's equipped items) and `LootProcs` (proc and utility affixes: parry buffs sent to allies as status effects carrying the rolled value as their skill level, the `Corruption_LootKill` owner-to-killer message, Deathless, Foraging, `co_affix`). Pure rules for the procs (stacks, cooldowns, extras under a cap) in `Rules/ProcRules`.

### Orlog

- `OrlogPlugin.cs`, `OrlogConfig.cs` (`1 - Levels` admin-only, `2 - Display` per player), `Safe.cs` (as Corruption's).
- `Rules/` (pure, tested): `LevelTables` (`LevelTable`, `CreatureXpTable`, `BiomeLevels`: read and check the tables), `ExperienceRules` (level difference and party split), `AttributeRules` (`ClassTable`, points and bonuses), `StonePlacement` (the stone's spot behind the Eikthyr stone, distance, scale), `SkillRules` (`SkillTable`, D2's spending rules, eitr cost). Corruption's `Csv` reader is linked in, not copied.
- `Data/`: `levels.csv`, `creature_xp.csv`, `biome_levels.csv`, `classes.csv`, `amazon_skills.csv`, made by `tools/make_orlog_tables.py` from `data/d2-reference/`, `data/wow-reference/` and the game dump (guide: `Orlog/Data/README.md`).
- `Content/`: `Progress` (the local player's XP in custom data `orlog_xp`, level on the player ZDO `orlog_level`, level-up), `KillXp` (creature owner works out the kill in a `Character.OnDeath` prefix, sends shares with the routed message `Orlog_Xp`), `SkillPoints` (skill points in custom data `orlog_skills`, each class's tree), `OrlogStone` (the Orlog stone: a Vegvisir copy built on Jötunn's `OnVanillaLocationsAvailable` from the first location that has one, placed once per world behind the Eikthyr trophy stone through the server messages `Orlog_PlaceStone`/`Orlog_PlaceStoneReply`, opens the class choice), `Attributes` (class and spent points in custom data; health, stamina and eitr added to `Player.GetTotalFoodValue`, damage in `SEMan.ModifyAttack`; bonuses cached until `Progress.ChangeCounter` moves), `Commands`, `Texts`.
- `Api.cs`: the public `Orlog.Api` the UI reads by reflection ([`classes-ui-brief.md`](classes-ui-brief.md)).

### Shared

- `Shared/ConfigWatcher.cs`: linked into every plugin project (`<Compile Include="..\Shared\..." Link=...>`). Reloads the plugin's `.cfg` on save; development aid, see `CURRENT_STATE.md` → Before release.

### Custom assets

- `tools/make_shard.py` and `tools/make_essences.py` (Python, numpy + Pillow) generate the gem models (`Corruption/Assets/*.obj`), textures (PNG for viewing, raw `.rgba` for the game) and previews in `docs/images/`, from fixed seeds. Rerun after changing them; commit the outputs.
- `tools/make_map_icons.py` cuts item icons out of a concept render (flat background removed from each grid cell's edges inwards) into raw textures; `GemModel.LoadIcon` turns one into a `Sprite`. The same route works for any item whose icon should be the art itself.
- `tools/make_orbs.py` makes the currency orbs into `Corruption/Assets/Orbs/`, which the wildcard deliberately doesn't cover until the orbs are built (add `Assets\Orbs\*` then). It paints patterns in 3D per texel, so runes don't stretch on the sphere's UV map.
- `tools/make_sheet_ui.py` makes the character sheet's UI art (CraftingOverhaul) into `CraftingOverhaul/Assets/Sheet/` (embedded by `CraftingOverhaul.csproj`, about 0.7 MB; also the class screens' medallion, glows, bar caps and slots). `tools/make_skill_icons.py` makes the placeholder skill and class icons into `CraftingOverhaul/Assets/Skills/` (31 × 36 KB raw, embedded; contact sheet `docs/images/skill_icons_preview.png`) and a mock-up of the whole sheet, `docs/images/sheet_preview.png`, so the look can be checked without the game. Its sizes mirror `SheetLayout.cs`; change both together.
- `Corruption.csproj` embeds every `Assets/*.obj` and `Assets/*.rgba` (wildcard) plus `Data/essences.csv`. `Rules/ObjMesh.cs` (tested) reads the models; `Content/GemModel.cs` builds the Unity mesh and material on the base item's shader, adds optional plain-colour add-on parts and the ground glow, and removes the base item's visuals (colliders stay). A failed look always falls back to a tint, so the item still registers.
- Textures are raw pixels because `Texture2D.LoadImage` (ImageConversion module) references netstandard 2.1, which the net462 build can't compile against. They make up most of the DLL's size (about 4 MB).

### Server messages (rips)

- Requests that need the whole world go to the server with `ZRoutedRpc` (registered in a `Game.Start` postfix): the player's game sends `Corruption_RipRequest` to `GetServerPeerID()`, the server answers the sender with `Corruption_RipReply`. In single player and on a host the "server" is the same game; routed RPCs to yourself are handled locally. Effects other players should see go to `ZRoutedRpc.Everybody` (which includes your own game), and each game shows them on the sender's character if it has it loaded (`Corruption_RipPing`, the corrupted wishbone).
- Per-character state that must survive logging out (the heart of the rip you entered) goes in `Player.m_customData`, which the game saves with the character.
- Only the server holds every object in the world (`ZDOMan.GetAllZDOsWithPrefabIterative` covers them all there), so checks like "near anyone's base" belong on the server.
- Objects for far-away places (the rip heart) are instantiated there directly: their persistent ZDO stays when the instance is removed as out of range, and they appear when someone arrives. Settle them on the ground once the area is loaded.
- Rip state lives on the objects' ZDOs (`corruption_rip_*` keys in `Rips.cs`), so it survives restarts and works for every player.
- **The rip request and reply carry the map's tier and affixes** (rips round, 2026-11-27; built, not play-tested). Request, in order: the request id, the map item's name, the number of candidate spots and for each its heart and landing x and z, then (new) `int tier`, `string affixes` (the affix ids joined with `;`) and `bool fallback` (the opener's game searched explored land too). Reply: the request id, the map item's name, whether a spot was found, its heart and landing x and z, `closeAt`, then (new) `int tier` and `string affixes`. **Both are read only when present** (`ZPackage.GetPos() < Size()`): an older build's message stops after the candidates or after `closeAt`, and a tiered map used on a server that sends no tier is kept with a message. The server clamps the tier to the table, drops unknown affixes, derives the tier from the affix count (corrupted only with a full set), echoes them, and rejects every candidate whose heart or landing spot is within 96 m of an object a player placed.
- **The server's placed-object scan** (#84): one pass over all the server's objects (`ZDOMan.m_objectsByID`; sector searches changed between game versions). For each object it checks first whether it is within the distance of any candidate (a position test only), and only then its builder id (`ZDOVars.s_creator`, set by `Piece.SetCreator`); an object with no builder id still counts if its prefab is a Plant's grown Pickable that isn't wild vegetation (a set of prefab hashes made once). It logs the time ("… checked K placed object(s) near them of Z in X ms"). Grown trees from saplings can't be told from wild ones.
- **Rip object data keys (2026-11-27):** `corruption_rip_tier` (int) and `corruption_rip_affixes` (the ids joined with `;`) on the entry rip, the heart and every wave creature (and every creature maddened in place); the exit copies them from the entry. `corruption_rip_close` (the rip's closing time) is now also on wave creatures: once it has passed they become the game's event creatures and leave like a vanilla raid's. `MapTiers.IsStamped` is "has a tier"; a creature without it counts as tier 1.
- **Map item custom data (2026-11-27):** `corruption.map_affixes` (the ids, `;`-joined) and `corruption.map_tier`, beside the shared `corruption.corrupted` and `corruption.mirrored` marks that gear uses (a corrupted map is locked, as corrupted gear). A map without them is tier 1. Constants in `Rules/MapData.cs`.
- **A game only loads the area around its player.** Counting creatures with `Character.GetAllCharacters()` misses the ones near a friend far away. Spawners also remember the ZDO ids they spawned (`BossArmies.Spawned`): a client keeps a ZDO it created until the game reports it destroyed, and destroy messages go to every player, so `ZDOMan.GetZDO(id) != null` means "still alive somewhere".
- **A flag set on an object others already have loaded** isn't seen by their `Awake` patches. Either poll it (the corrupted boss look checks its ZDO) or broadcast a message (`Corruption_Maddened` makes every game add the maddened look).

### Dedicated-server review (rips and bosses, 2026-11-01)

Read through the code, not run on a server (the user's group session tests it; `PLAYTEST.md` → Session 2). What runs where:

| Path | Runs on | Verdict |
|---|---|---|
| Rip request (bases check, closing time) | Server (`ZNet.IsServer`); messages registered in `Game.Start`, which a dedicated server runs too | Sound |
| Rip heart and exit placed far away | The opener's game creates them; a client sends every object it creates or changes to the server whatever the distance (`ZDOMan.CreateSyncList`, the client change queue), so a friend arriving later gets them | Sound |
| Waves, summons, chest, essence and shard drops, corrupted summon at an altar | The game that owns the heart, boss, creature or altar (the nearest player, or whoever claimed it); nothing on the server | Sound |
| Wave and summon limits | The owner's game; now counts what it spawned even when out of its loaded area | **Fixed 2026-11-01** (could exceed the limit with players far apart) |
| Maddened look on other players' games | Flag on the creature's ZDO; creatures maddened in place when waves start weren't tinted for players who had them loaded already | **Fixed 2026-11-01** (broadcast) |
| Corrupted boss look, rip portals, heart hover | Every game reads the ZDO | Sound |
| Wishbone pings | The ping effects have their own network object, so the game shows them to everyone (log: `shared with other players by the game`) | Sound |
| Messages to a player (raid start) | `Player.Message` on another player's character is sent to that player's game | Sound |
| Item icons on a headless server | Jötunn's `RenderManager` skips rendering when headless | Sound |
| Admin settings and versions | Server-synced (Jötunn); Corruption and Orlog require the same minor version everywhere | Sound |
| Rip portals and hearts closing after their time | Only their owner's game destroys them, so one far from everyone closes when someone is next near | Known, harmless |

### Build stamp

`Corruption.csproj`, `Orlog.csproj` and (since 2026-11-01) `CraftingOverhaul.csproj` write the git branch and short commit into the DLL (`AssemblyMetadata("GitBuild", …)`, target `StampGitCommit`); the plugin logs it on load. When a play-test result looks like "nothing happened", check that line first.

## Code patterns (follow these)

1. **Isolated patches.** `PatchAllSafely()` applies each `[HarmonyPatch]` class separately inside try/catch, so one broken target doesn't disable the rest.
2. **Never break vanilla.** Code that runs inside game methods goes through `Safe.Run` / `Safe.Report`: log once per action, fall back to vanilla behaviour.
3. **Pure rules + tests.** Anything decidable without Unity goes in a pure file (no `UnityEngine`), linked into `CraftingOverhaul.Tests` by `<Compile Include=... Link=...>`, and unit-tested. Tests may read `data/game-dump-1.0.21/*.csv` for real game data.
4. **Data-driven first.** Derive lists from game data (biomes, talisman families); config overrides only for real exceptions (`BiomeMaterialOverrides`, `ExcludedCreatures`).
5. **Clone, don't import** (exceptions: the shard and essence models, above). New items/pieces are Jötunn clones of vanilla prefabs, recoloured with `Corrupted.Tint` (all shader colour properties, particle start colours, lights). No new models unless the user asks. New animations are allowed where no vanilla animation or emote fits (user, 2026-10-30); they come from asset bundles built in `JotunnModUnity/`.
6. **Config scope.** Gameplay numbers admin-only (server-synced); visual settings per player.
7. **Robust patch arguments.** Use `__0`/`__1` positional arguments where parameter names may change.
8. **No hard-coded positional limits without a guard** (e.g. `Riftforge` refuses more than 5 ingredients and logs an error).
9. **Long work doesn't block a frame.** Work that reads many prefabs is an enumerator of small steps pumped a few milliseconds per frame from the plugin's coroutine (`Biomes/SlicedWork.cs`); asset-bundle prefabs load with `SoftReference.LoadAsync()` (each load holds a reference: always `Release()` it, also in a `finally` for when the work is stopped), and pure computation can go to a thread-pool thread (no Unity calls there). Results that only depend on game data can be saved under `BepInEx/cache` with a key of everything they depend on (`Biomes/BiomeCacheKey.cs`).
10. **Renaming something players have saved** (an affix id, family or item prefab): add the old and new names to `Corruption/Rules/AffixRenames.cs` instead of only editing the tables. Saved lines read the old id as the new one, and a renamed splinter family's old item stays registered, so nothing in a world vanishes (2026-11-19, the orb finders). **Never delete a row from `affixes.csv`:** to stop an affix rolling, set its status to `retired` (it stays readable on saved items). Every id and family ever shipped is listed in `Corruption/Data/shipped_affixes.csv`, and a test fails if one disappears without a rename or a retired row.

## Build and test

- Windows: open `CraftingOverhaul.sln` and build; see `README.MD` (Valheim install found via Steam or `Environment.props`).
- Command line (any OS, needs Valheim's managed assemblies at `VALHEIM_INSTALL`):
  `dotnet build Corruption/Corruption.csproj -p:VALHEIM_INSTALL=<path> -p:ExecutePrebuild=false`
- Tests (no game needed): `dotnet test CraftingOverhaul.Tests`.
- Without a Valheim install (e.g. a cloud session), a stand-in install can be built from NuGet: game, Unity, Harmony and BepInEx DLLs from `Pfhoenix.Valheim.ModProjectReferences` 1.0.26 (`lib/net46/`) into `valheim_Data/Managed/`, publicized game DLLs from `Digitalroot.Valheim.Common.References` 1.0.21 renamed to `publicized_assemblies/<name>_publicized.dll`, and `0Harmony.dll` plus `bepinex.dll` (also as `BepInEx.dll`) in `BepInEx/core/`. Both plugins compiled this way on 2026-10-27. Since 2026-11-06 the cloud environment's setup script, [`tools/cloud-setup.sh`](../tools/cloud-setup.sh), does this at every fresh session (into `/opt/valheim-standin`, outside the repo; `VALHEIM_INSTALL` points there); all three plugins build and the 262 tests pass with it. **The stand-in lags the game:** its compile references are 1.0.21's and its readable code 1.0.14's, while the game is 1.0.30 since 2026-11-25. A cloud build can't show members a newer game removed (1.0.30 removed `ConnectPanel.m_root` and `SoftReference.Name`; only Krogon's own build and log showed it), so after a game update the user's local build is the check (`README.MD` → After a game update). When `Digitalroot.Valheim.Common.References` publishes the new game version, raise it in `tools/cloud-setup.sh` and the environment's setup script ([#86](https://github.com/krogon-games/JotunnModStub/issues/86)).
- **Build check `CheckNoUnshippedReferences`**: each plugin's build fails if the DLL references `System.ValueTuple`, `System.Memory` or `System.Buffers`. Valheim's Mono doesn't ship them; C# tuples crashed the game's inventory UI (2026-10-26).
- In-game checks: `co_dump` (CSV of items, recipes, pieces, creatures, locations, biome data) and `co_biome <item>`.

## Game facts the code relies on (verified against 1.0 code unless noted)

Full 1.0.14 game code (method bodies, not just signatures) comes from the NuGet package `Pfhoenix.Valheim.ModProjectReferences` 1.0.26 (`lib/net46/assembly_valheim.dll`), decompiled with `ilspycmd`. Reference-only packages (`ValheimGameLibs`, `Digitalroot.Valheim.Common.References`) have signatures only.

**Multiplayer**
- The player whose game owns an area simulates its creatures, AI and physics; the dedicated server only relays and stores. "Server-authoritative spawning" is not how vanilla works. Details: [`coop-networking.md`](coop-networking.md).
- Harvesting (trees, logs, stumps, rocks, deposits): `Damage()` on the striker's machine only sends the hit by RPC; the object's owner applies it, decides the break and spawns the drops. Changes to the hit must be made striker-side; extra drops need a message back to the striker.
- When the striker also owns the object, that RPC is handled **at once, inside `Damage()`** (`ZRoutedRpc.InvokeRoutedRPC` calls the handler directly for a message to oneself): a broken deposit chunk is already hidden (`MineRock5.RPC_SetAreaHealth`, `MineRock.RPC_Hide`) by the time a `Damage` postfix runs. On another owner it arrives later. So anything read from the object for a hit (which chunks touch the struck one) is read in the prefix (1.0.14 code, 2026-11-08).
- Hits from physics (`ImpactEffect`: a log landing or rolling, a falling tree hitting something) have no attacker and are sent by whichever game runs the moving object.
- `Player.m_customData` is saved with the character (profile), so per-character data that isn't in the world goes there: the inventory tabs (base64 of `Inventory.Save`), the corrupted wishbone's rip target. A gravestone's inventory keeps items in rows past its height when loaded (`Container.UpdateRows`), which is how tab items fit in it.
- Items in EquipmentAndQuickSlots custom slots (our talisman slots) live only in the owner's inventory; other games can't see them. What other games need to know (worn talismans) is published on the player's own ZDO, which the player owns and every game receives.
- Taking from a chest requires owning it: `NearbyContainers` claims ownership right before removing items.
- Other players' trinkets are readable on any client via `VisEquipment.m_currentTrinketItemHash` (synced through the ZDO), which is how talismans work for remote players.

**Rips, weather and placed objects (1.0.14 code read for the rips round, 2026-11-26; assumed the same in 1.0.30, to confirm from the log)**
- Weather is per client: each game works out its own with `EnvMan.GetEnvironmentOverride`, and a vanilla raid forces its weather per client the same way. The weather affixes (next round) must therefore be set on each player's own game from data it can read, not once on the server. Smoke damage must likewise run on each player's own game.
- What an object's ZDO holds in a game depends on the server's simulation distance: the server sends "normal" objects only within the near area. A rip's heart 96–128 m from the landing spot may not be loaded there, so a client either gets the heart made a "distant" object or remembers the rip's affixes on entering (as `RipCompass` does the heart's position). After the heart dies only the exit carries the stamp.
- `Piece.SetCreator` sets the builder id (`creator`) only for player placements; objects the world makes have none, and a Plant's grown Pickable has none either (so grown crops are matched by prefab name and grown trees can't be).
- A raid's own wave spawn refuses spots inside a player base (an `EffectArea` of type PlayerBase); ours does the same.

**Ground items and pickup (1.0.14 code, decompiled 2026-11-26 for personal loot; assumed the same in 1.0.30)**
- Every pickup path (E, auto-pickup, magnets) ends in `Humanoid.Pickup`, so refusing there stops them all.
- `ItemDrop.CanPickup` means "this game owns the drop"; when it is false the game asks for ownership, so it must not be used to mean "allowed to take".
- `Player.AutoPickup` skips a drop whose `m_autoPickup` is false before it pulls the drop or requests ownership. Setting that flag is how a hidden drop stays out of auto-pickup.
- `ItemDrop.AutoStackItems` merges owned drops with the same name and quality within 4 m, only when more than 200 drops are loaded, and destroys the merged one. A tagged drop must be kept out of it or its owner tag and beam are lost.
- `Player.FindHoverObject` uses the first hit of one ray, so a hidden drop in front would block the hover of whatever is behind it; the cloud's 1.0.21 references have no `RaycastHitComparer`, so 1.0.21 changed its sort (our replacement sorts by distance itself).
- `DropTable.GetDropListItems` reuses one list; copy it before changing it.
- The dedicated server's network clock (`ZNet.GetTime`) runs only while players are on, so a time-based lock pauses when nobody is online. Each game keeps its own copy of the clock (the server corrects it about every 2 s).
- `LightLod` re-enables its light from a coroutine, so hiding a light by disabling it once is not enough.
- `ItemDrop.SaveToZDO` writes only the item-data key; our own ZDO keys on a ground item (`corruption_loot_*`) never enter the item's data, so they don't travel into inventories.

**How the group's mods and the game were read in the cloud (2026-11-26)**
- A mod's DLL comes from Thunderstore at `https://thunderstore.io/package/download/<Team>/<Mod>/<version>/` (a zip). Decompile it with `ilspycmd`: `dotnet tool install -g ilspycmd --version 8.2.0.7535`, run with `DOTNET_ROLL_FORWARD=Major` and `-r /opt/valheim-standin/valheim_Data/Managed`. AdventureBackpacks 2.4.8, EquipmentAndQuickSlots 3.2.1 and PlantEasily 2.3.1 were read this way (results in [`mod-compatibility.md`](mod-compatibility.md)).
- The game itself: `ilspycmd -p -o <dir> /opt/valheim-standin/valheim_Data/Managed/assembly_valheim.dll`.
- Never commit the decompiled code.

**Items, pieces, recipes**
- A building piece supports at most **5 ingredients**: `Hud.SetupPieceInfo` indexes `m_requirementItems[resources.Length]`.
- Pieces appear in the build menu once the station is known and every ingredient is in `m_knownMaterial` (picked up at least once). Recipes follow the same pickup rule.
- `Piece.m_requireOnlyOneIngredient` makes every ingredient an alternative (all or nothing).
- `ItemDrop.ItemData.m_customData` exists in 1.0 and is saved with the item (planned storage for affixes).
- Trinket and Utility slots exist (`Humanoid.m_trinketItem`, `m_utilityItem`).

**Bows (1.0.14 code, decompiled 2026-11-17)**
- `Player.UpdateAttackBowDraw` draws while attack is held (`m_attackDrawTime` grows; stamina drains at the bow's draw rate, half once fully drawn; the drawn bow slows walking via `IsDrawingBow`) and shoots only when the hold ends (`StartAttack`), and only if stamina and eitr are left. Draw time -1 means "wait for the release, then reset" with no shot. Holding a drawn bow therefore never shoots by itself.
- `Attack.FireProjectileBurst` is self-contained once the attack has its character, weapon and draw (`Attack.StartWithoutAnimation` sets them): it reads the current aim, makes the arrow from `m_ammoItem` and takes no ammo. Ammo is taken in `Attack.OnAttackTrigger` (`UseAmmo`), which also wears the weapon and plays the release effect. Strafe's rapid fire uses this.
- A projectile trains the shooter's skill by its hit's `m_skillRaiseAmount` for each hit that does damage (`Projectile.OnHit`).
- Out of stamina while drawing, the game drops the draw and shoots nothing (`UpdateAttackBowDraw`: the release only shoots with stamina and eitr left). The draw drain is `ItemData.GetDrawStaminaDrain` (less up to a third with the bow's skill), halved at full draw (2026-11-18).

**Creatures and the camera (1.0.14 code, decompiled 2026-11-18)**
- `Character.CanMove` false stops walking and turning (`UpdateWalking`) and flying (`UpdateFlying`), and `Humanoid.StartAttack` refuses to start an attack; swimming (`UpdateSwimming`) reads only status-effect speed. Flying ignores status-effect speed, so the game's Immobilized (speed −1000) doesn't hold a flier. Shackle Shot's stun uses both.
- `SEMan.AddStatusEffect` on a creature another game runs sends it to that game (`RPC_AddStatusEffect`); `RPC_Damage` likewise adds a hit's `m_statusEffectHash` on the owner. Status effects live only on the owner's game.
- `GameCamera.SetTempFOV(target, inertia)` moves the view angle toward a target each frame (`UpdateFOV`); `m_fovBase` keeps the unzoomed angle (the game's fixed 65°; nothing sets it from the settings). Snipe's zoom uses it.
- `SE_Harpooned` refuses bosses, and its rope (`LineConnect`) needs a networked object at each end, so a local look draws its own line.

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
