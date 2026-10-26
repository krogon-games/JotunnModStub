# CraftingOverhaul

Valheim 1.0 mods (BepInEx + Jötunn + Harmony), two plugins in one solution: `CraftingOverhaul/` (quality of life) and `Corruption/` (endgame, design in `docs/corruption-design.md`). See `README.MD` for build setup. Game reference data is in `data/game-dump-1.0.20/`.

The build fails if a plugin references an assembly Valheim doesn't ship (e.g. no C# tuples: they need System.ValueTuple).

## FEATURES.md is mandatory

`FEATURES.md` at the repo root tracks every feature the mod adds or changes.

- Any commit that adds, changes, fixes or removes a feature must update `FEATURES.md` in the same commit: the summary table row (status, config options, files touched, last updated), the feature's section (behaviour, usage, config defaults, known issues, Harmony patch targets), and a dated Changelog entry, newest first.
- When the user reports play-test results, update the feature's status (`Tested in-game` or `Broken`) and known issues to match.
- Keep the Harmony patch target lists exact. They are used to spot conflicts with Valheim Plus and other mods.
