# Handoff: `claude/jolly-clarke-4kx6t6`

Written 2026-11-18 (`/handoff`). **Merged into `master` as PR #80 the same day** (no version change; #42 closed). The branch was then restarted from `master` for the coordinator's update.

## Stage

Merged (PR #80, 2026-11-18).

## Purpose

[#42](https://github.com/krogon-games/JotunnModStub/issues/42): replace the 1.0.20 game data with a fresh `co_dump` from Valheim 1.0.21 and check whether any number we derived from the game data changes.

## Done

Nothing here needs testing in game (reference data only), so Done lists the finished work:

- Krogon's `co_dump` (2026-11-18, 1.0.21, his usual mods) is now `data/game-dump-1.0.21/`, replacing `data/game-dump-1.0.20/` (git history keeps it). `biome_sources.csv` is left out, as before.
- Compared file by file with 1.0.20 (`data/game-dump-1.0.21/README.md` lists every difference). Creatures, spawns, raids, chests, locations, altars, pieces and every vanilla item's numbers are unchanged. Game changes: three cauldron foods moved to the prep table; the mage hood's upgrade needs one moose trophy at every level; the snow troll's attacks are tool tier 4 (was 6); four new snow wall destructibles.
- Nothing we derived changes: `tools/make_loot_bases.py` and `tools/make_orlog_tables.py` give identical tables on the new dump; affix ranges, drop chances, rip waves and loot, and altar offerings come from unchanged files.
- The tests, `tools/`, code comments and live docs point at the new folder. 601 tests pass. No mod code changed.

## Half-done and known issues

- Malicious Blood (`HatefulBlood`, Kall's offering) has no source in the game data (1.0.20 too); it shows as unplaced now because the corrupted malicious blood recipe uses it. Not new, nothing to fix here.
- The old folder is replaced (git history keeps it); docs saying "from the 1.0.20 dump" still name the right source.

## Decisions

1. **2026-11-18, round and branch:** this round is the fresh `co_dump` (#42) and checking our numbers against it, on `claude/jolly-clarke-4kx6t6`. Krogon confirmed by sending the dump after being asked.

## Playtest

Nothing to play-test: no mod code, data table or asset changed. `docs/playtests/claude-jolly-clarke-4kx6t6.md` says so; no steps on the phone page.

## Open questions for Krogon

- None.

## Next steps

1. Nothing: merged as PR #80, #42 closed.

## Checks

- Build: PASS (601 tests pass; Corruption builds against the stand-in game folder, 0 errors).
- Version: SKIPPED for gameplay: the only shipped files changed are two code comments in Corruption (`Rules/HarvestRules.cs`, `Rules/RipRules.cs`, the dump path); no version changed on this branch.
- Multiplayer sync: SKIPPED (nothing new to sync; read in code).
- Assets, prefabs and names: SKIPPED (no new assets, prefabs or text).
- FEATURES.md: PASS (changelog entry, the dump feature's known issues and date).
- In-game: not needed (no behaviour changed).

## Merge notes

- **Merged** (PR #80). No version bump: no gameplay change (two Corruption code comments only).
- Shared docs touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `CLAUDE.md`, the `/handoff` skill (dump path). A branch still reading `data/game-dump-1.0.20/` in a new test needs the path changed after merging this.
