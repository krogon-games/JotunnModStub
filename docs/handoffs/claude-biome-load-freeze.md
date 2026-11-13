# Handoff: `claude/biome-load-freeze`

**Written:** 2026-11-13. Branch from `master` at `7bae804` (PR #63); no PR yet.

## Stage

**Waiting for Krogon's playtest.**

## Purpose

Remove the 18–22 s freeze right after spawning (Krogon's 1.0.21 logs, 2026-11-13: `Resolved biomes for 970 items from 39595 sources in 18348 ms`), caused by CraftingOverhaul's biome detection running all at once on the game's main thread.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built, untested:
- **Saved result:** the solved biomes go to `BepInEx/cache/CraftingOverhaul.biomes.txt` with a key (game version, mods and versions, this DLL's build, overrides, a fingerprint of items, recipes, prefabs and building costs, vegetation, locations, spawn tables). A matching key → used at once on the next load.
- **Background work** when there's no match: the collectors run as small steps, about 4 ms per frame; location and room prefabs load with `SoftReference.LoadAsync`, 4 at a time, and are released after reading; the solver runs on a thread-pool thread. The recipe list keeps the game's order without biome headers until it's ready, then refreshes by itself.
- `co_biome` and `co_dump` finish any background work on the spot; `co_biome refresh` ignores and rewrites the saved file.
- The log line now gives the time per collector, the longest frame and the longest step.
- Location rooms are now loaded once each (as before) but in a separate pass after all locations; one broken prefab now skips only that prefab.

Assumptions (not measured in game):
- Where the 18 s went: not measured (the old log only had the total). The solver alone on 40,000 made-up sources took about 0.1 s in .NET 8, so loading locations and rooms is assumed to be most of it. The new log line will show it.
- How long the background work takes in wall time, and whether background asset loads cause stutters, is unknown until the play-test.
- The saved file can go stale if a mod changes drops inside a prefab through its own settings without a version change; `co_biome refresh` fixes it.

## Decisions

No decisions by Krogon yet on this branch. The task came with its goal and options (background work, saved result, faster solver); both of the first two were built, the solver was left as it was (it now runs off the main thread).

## Playtest

Checklist: [`docs/playtests/claude-biome-load-freeze.md`](../playtests/claude-biome-load-freeze.md), also on the phone page (steps `bio-1` to `bio-6`). Single player is enough: it only runs on each player's own game, not on the server.

## Open questions for Krogon

None.

## Next steps

1. Krogon play-tests the checklist and sends the `Resolved biomes for …` log line.
2. Fix what the play-test finds (e.g. a stutter: lower `LoadsInFlight` or the 4 ms budget).
3. Run `/handoff`, then a PR into `master`.

## Checks

- Build: PASS (CraftingOverhaul against the stand-in install). `dotnet test CraftingOverhaul.Tests`: 510 passed (12 new: saved-file format and frame slicing).
- Version: not changed (feature branches never do).
- Multiplayer sync: not affected (per-player client work, no network messages, no config added).
- FEATURES.md: updated (biome detection section and row, recipe categories row, changelog).
- In-game: needs Krogon's playtest.

## Merge notes

- Not ready to merge: needs the play-test.
- **Gameplay changed: version bump needed at merge** for CraftingOverhaul (code only; no data or assets).
- Shared files touched: `FEATURES.md` (changelog, two table rows, biome section), `docs/CURRENT_STATE.md` (one line under known issues), `docs/ARCHITECTURE.md` (biome bullet, pattern 9), `CraftingOverhaul/CraftingOverhaul.cs` (one line in `Awake`), `CraftingOverhaul/RecipeList/RecipeListOrganizer.cs`.
