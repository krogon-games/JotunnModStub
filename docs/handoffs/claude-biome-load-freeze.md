# Handoff: `claude/biome-load-freeze`

**Written:** 2026-11-15 (`/handoff`). 6 commits ahead of `master`, 0 behind; no PR yet.

## Stage

**Ready to merge.**

## Purpose

Remove the 18–22 s freeze right after spawning (Krogon's 1.0.21 logs, 2026-11-13), caused by CraftingOverhaul's biome detection running all at once on the game's main thread.

## Done

Confirmed by Krogon's play-test, 2026-11-15 (all six steps, single player, 1.0.21):
- **Saved result:** the biomes go to `BepInEx/cache/CraftingOverhaul.biomes.txt` with a key (game version, mods, this DLL's build, overrides, a game-data fingerprint) and load in 12 ms while it matches.
- **Background work** otherwise: about 4 ms per frame, location and room prefabs loaded with `SoftReference.LoadAsync` 4 at a time, the solver on a thread-pool thread. First load: 28.4 s in the background, 1.2 s of game time, longest frame 87 ms, the same 970 items. The recipe list keeps the game's order until it's ready.
- `co_biome` explanations and `co_biome refresh` work; the log shows the time per collector.

Measured: the location and room loading was the freeze (13.5 of 13.6 s run all at once).

## Half-done and known issues

- `co_biome refresh` and `co_dump` still run all at once: about a 14 s pause (kept, see Decisions).
- The saved file can go stale if a mod changes drops inside a prefab through its own settings without a version change; `co_biome refresh` fixes it.

## Decisions

1. **2026-11-15: `co_biome refresh` keeps running all at once** (about a 14 s pause): "Leave it". It's an admin command, rarely used.

## Playtest

Passed 2026-11-15; details in [`docs/playtests/claude-biome-load-freeze.md`](../playtests/claude-biome-load-freeze.md). The steps are off the phone page.

## Open questions for Krogon

None.

## Next steps

1. A PR into `master`; the coordinator raises CraftingOverhaul's version at merge.

## Checks

- Build: PASS. 510 tests passed (12 new); CraftingOverhaul builds against the stand-in install.
- Version: gameplay changed: version bump needed at merge (CraftingOverhaul). No version number changed on this branch.
- Multiplayer sync: PASS (read in code, not tested). No new config, items or messages; per-player work, so it works when only some players have the mod.
- Assets and names: SKIPPED (none new).
- FEATURES.md: PASS.
- In-game: PASS (Krogon's play-test 2026-11-15).

## Merge notes

- **Ready to merge: yes.** Play-test passed, no check failed, up to date with `master`, pushed.
- **Gameplay changed: version bump needed at merge: CraftingOverhaul.**
- Also changed by the unmerged `claude/lucid-albattani-imrei7`: `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj`, `CraftingOverhaul/CraftingOverhaul.cs` (one line in `Awake` here), `FEATURES.md`, `docs/ARCHITECTURE.md`, `docs/CURRENT_STATE.md`. Keep both sides in a conflict.
