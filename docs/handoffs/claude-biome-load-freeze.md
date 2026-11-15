# Handoff: `claude/biome-load-freeze`

**Written:** 2026-11-13, updated 2026-11-15 after the play-test. Branch from `master` at `7bae804` (PR #63); no PR yet.

## Stage

**Fixing feedback** (play-test passed 2026-11-15; one open question on `co_biome refresh`).

## Purpose

Remove the 18–22 s freeze right after spawning (Krogon's 1.0.21 logs, 2026-11-13: `Resolved biomes for 970 items from 39595 sources in 18348 ms`), caused by CraftingOverhaul's biome detection running all at once on the game's main thread.

## Done

- **Play-test 2026-11-15 passed (all six steps, single player, 1.0.21):** no freeze after spawning; background work 28.4 s using 1.2 s of game time over 2,695 frames, longest frame 87 ms; same 970 items; the saved file loads in 12 ms; `co_biome` explanations and refresh work.

## Half-done and known issues

Built and passed in the play-test:
- **Saved result:** the solved biomes go to `BepInEx/cache/CraftingOverhaul.biomes.txt` with a key (game version, mods and versions, this DLL's build, overrides, a fingerprint of items, recipes, prefabs and building costs, vegetation, locations, spawn tables). A matching key → used at once on the next load.
- **Background work** when there's no match: the collectors run as small steps, about 4 ms per frame; location and room prefabs load with `SoftReference.LoadAsync`, 4 at a time, and are released after reading; the solver runs on a thread-pool thread. The recipe list keeps the game's order without biome headers until it's ready, then refreshes by itself.
- `co_biome` and `co_dump` finish any background work on the spot; `co_biome refresh` ignores and rewrites the saved file.
- The log line now gives the time per collector, the longest frame and the longest step.
- Location rooms are now loaded once each (as before) but in a separate pass after all locations; one broken prefab now skips only that prefab.

Measured 2026-11-15: the location and room loading was the freeze (13.5 of 13.6 s run all at once; every other collector under 20 ms).

Known issues:
- `co_biome refresh` (and `co_dump`) still run all at once: a 14 s pause. Krogon to decide whether that matters (open question).
- The saved file can go stale if a mod changes drops inside a prefab through its own settings without a version change; `co_biome refresh` fixes it.

## Decisions

No decisions by Krogon yet on this branch. The task came with its goal and options (background work, saved result, faster solver); both of the first two were built, the solver was left as it was (it now runs off the main thread).

## Playtest

Checklist: [`docs/playtests/claude-biome-load-freeze.md`](../playtests/claude-biome-load-freeze.md), also on the phone page (steps `bio-1` to `bio-6`). Single player is enough: it only runs on each player's own game, not on the server.

## Open questions for Krogon

1. `co_biome refresh` pauses the game for about 14 s: leave it (an admin command, rarely used), or make it work in the background like at spawn?

## Next steps

1. ~~Krogon play-tests the checklist.~~ Passed 2026-11-15.
2. Krogon answers the refresh question; if "background", build it and re-test step 5.
3. Take the `bio-` steps off the phone page once the round is final; run `/handoff`, then a PR into `master`.

## Checks

- Build: PASS (CraftingOverhaul against the stand-in install). `dotnet test CraftingOverhaul.Tests`: 510 passed (12 new: saved-file format and frame slicing).
- Version: not changed (feature branches never do).
- Multiplayer sync: not affected (per-player client work, no network messages, no config added).
- FEATURES.md: updated (biome detection section and row, recipe categories row, changelog).
- In-game: PASS (Krogon's play-test 2026-11-15, all six steps).

## Merge notes

- Not ready to merge yet: play-test passed; waiting on the refresh question, then `/handoff`.
- **Gameplay changed: version bump needed at merge** for CraftingOverhaul (code only; no data or assets).
- Shared files touched: `FEATURES.md` (changelog, two table rows, biome section), `docs/CURRENT_STATE.md` (one line under known issues), `docs/ARCHITECTURE.md` (biome bullet, pattern 9), `CraftingOverhaul/CraftingOverhaul.cs` (one line in `Awake`), `CraftingOverhaul/RecipeList/RecipeListOrganizer.cs`.
