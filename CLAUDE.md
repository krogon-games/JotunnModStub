# CraftingOverhaul

Valheim 1.0 mod (BepInEx + Jötunn + Harmony). Source is in `CraftingOverhaul/`; see `README.MD` for build setup.

## FEATURES.md is mandatory

`FEATURES.md` at the repo root tracks every feature the mod adds or changes.

- Any commit that adds, changes, fixes or removes a feature must update `FEATURES.md` in the same commit: the summary table row (status, config options, files touched, last updated), the feature's section (behaviour, usage, config defaults, known issues, Harmony patch targets), and a dated Changelog entry, newest first.
- When the user reports play-test results, update the feature's status (`Tested in-game` or `Broken`) and known issues to match.
- Keep the Harmony patch target lists exact. They are used to spot conflicts with Valheim Plus and other mods.
