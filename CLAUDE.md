# Valheim mods: CraftingOverhaul and Corruption

Valheim 1.0 mods (BepInEx + Jötunn + Harmony) for a group of four friends on a dedicated server. Two plugins in one solution:
- `CraftingOverhaul/`: quality of life (recipe search and categories, biome grouping, craft/build from nearby chests). Works when only some players have it.
- `Corruption/`: Diablo/Path of Exile-style endgame. The server and every player need it.

## Read first

| File | Use it for |
|---|---|
| [`docs/CURRENT_STATE.md`](docs/CURRENT_STATE.md) | What's verified, built, broken, waiting on the user, and the next tasks |
| [`FEATURES.md`](FEATURES.md) | Every feature: behaviour, config, known issues, exact Harmony patch targets |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Code layout, patterns to follow, game limits the code relies on |
| [`docs/DECISIONS.md`](docs/DECISIONS.md) | Why things are as they are; rejected approaches; corrected facts |
| [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) | Design overview and status, linking the full design docs |
| [`docs/AFFIX_RULES.md`](docs/AFFIX_RULES.md) | Affix rules and reasoning |
| [`Corruption/Data/`](Corruption/Data/README.md) | **Tunable numbers**: `affixes.csv` (every affix), `drop_chances.csv` and `drop_modifiers.csv` (loot drop rates). Edit numbers here, never in the docs; `dotnet test` validates them |

Game reference data: `data/game-dump-1.0.20/` (from `co_dump`; development only, never ship it).

## Build and test

- Tests (no game needed): `dotnet test CraftingOverhaul.Tests`
- Build: open `CraftingOverhaul.sln` on Windows (see `README.MD`), or
  `dotnet build <Project>/<Project>.csproj -p:VALHEIM_INSTALL=<Valheim folder> -p:ExecutePrebuild=false`
  (needs Valheim's managed assemblies; without a real install, a folder with the reference assemblies works).
- Only the user can play-test. Never mark anything "Tested in-game" without their report.

## Hard constraints

- **No assemblies Valheim doesn't ship:** no C# tuples (`System.ValueTuple`), `System.Memory`, `System.Buffers`. The build fails on them; use small structs or parallel arrays.
- **Building pieces: at most 5 ingredients** (`Hud.SetupPieceInfo` crashes on more).
- Register items and pieces in `PrefabManager.OnVanillaPrefabsAvailable`.
- Apply each patch class separately (`PatchAllSafely`); wrap code running inside game methods in `Safe.Run` / `Safe.Report` so failures fall back to vanilla.
- Put game-independent logic in pure files (no `UnityEngine`) linked into `CraftingOverhaul.Tests`, and test it.
- Data-driven over hand-written lists: derive from game data; config overrides only for real exceptions.
- Multiplayer: the owning client simulates creatures and objects; the dedicated server only relays. Claim ownership before changing an object.
- Gameplay config is admin-only (server-synced); visual config is per player.
- v1 content: clone and tint vanilla prefabs; no new models or animations. Norse names.

## Documentation rules

### FEATURES.md is mandatory

`FEATURES.md` at the repo root tracks every feature the mod adds or changes.

- Any commit that adds, changes, fixes or removes a feature must update `FEATURES.md` in the same commit: the summary table row (status, config options, files touched, last updated), the feature's section (behaviour, usage, config defaults, known issues, Harmony patch targets), and a dated Changelog entry, newest first.
- When the user reports play-test results, update the feature's status (`Tested in-game` or `Broken`) and known issues to match.
- Keep the Harmony patch target lists exact. They are used to spot conflicts with Valheim Plus and other mods.

### Keep the handover docs current

When a significant feature is completed, changed, deferred or play-tested, follow the update routine at the end of `docs/CURRENT_STATE.md` in the same commit (CURRENT_STATE, DECISIONS, and GAME_DESIGN / AFFIX_RULES / ARCHITECTURE where affected). Separate verified results from assumptions; don't record guesses as facts.

Don't duplicate: design detail lives in `docs/corruption-design.md`, `docs/AFFIX_RULES.md` and `docs/storage-quests-design.md`; tunable numbers in data tables under `Corruption/Data/`; feature detail in `FEATURES.md`. Other docs link to them.

## Working style

- Plain, concise English; short answers when asked.
- Ask before changing an agreed design; record the user's design decisions in `DECISIONS.md`.
