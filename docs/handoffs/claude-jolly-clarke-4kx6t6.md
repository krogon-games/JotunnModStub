# Handoff: `claude/jolly-clarke-4kx6t6`

Written 2026-11-18. Branch: up to date with `master`; not pushed as a PR yet.

## Stage

Waiting for Krogon's go-ahead (nothing to play-test, see Playtest).

## Purpose

[#42](https://github.com/krogon-games/JotunnModStub/issues/42): replace the 1.0.20 game data with a fresh `co_dump` from Valheim 1.0.21 and check whether any number we derived from the game data changes.

## Done

- Krogon's `co_dump` (2026-11-18 21:22, 1.0.21, our three mods plus EquipmentAndQuickSlots and AdventureBackpacks) is now `data/game-dump-1.0.21/`, replacing `data/game-dump-1.0.20/` (git history keeps it). `biome_sources.csv` is left out, as before.
- Compared file by file with 1.0.20 (`data/game-dump-1.0.21/README.md` lists every difference). Creatures, spawns, raids, chests, locations, altars, pieces and every vanilla item's numbers are unchanged. Game changes: three cauldron foods moved to the prep table; the mage hood's upgrade needs one moose trophy at every level; the snow troll's attacks are tool tier 4 (was 6); four new snow wall destructibles.
- Nothing we derived changes: `tools/make_loot_bases.py` and `tools/make_orlog_tables.py` give identical tables on the new dump; affix ranges, drop chances, rip waves and loot, and altar offerings come from unchanged files.
- The tests, `tools/`, code comments and live docs point at the new folder. 601 tests pass. No mod code changed.

## Half-done and known issues

- Malicious Blood (`HatefulBlood`, Kall's offering) has no source in the game data (1.0.20 too); it shows as unplaced now because the corrupted malicious blood recipe uses it. Not new, nothing to fix here.
- Technical choice (not Krogon's): the old folder is replaced, not kept beside the new one; older docs that say "from the 1.0.20 dump" still name the right source, since that data is unchanged.

## Decisions

1. **2026-11-18, round and branch:** this round is the fresh `co_dump` (#42) and checking our numbers against it, on `claude/jolly-clarke-4kx6t6`. Krogon confirmed by sending the dump after being asked.

## Playtest

Nothing to play-test: no mod code, data table or asset changed. `docs/playtests/claude-jolly-clarke-4kx6t6.md` says so; no steps on the phone page.

## Open questions for Krogon

- None.

## Next steps

1. Krogon runs `/handoff`; then a PR into `master` that closes #42.

## Checks

Not run yet (`/handoff`). Tests: 601 pass.

## Merge notes

- No version bump needed: no gameplay change (reference data, tests, tools and docs only).
- Shared docs touched: `FEATURES.md` (changelog entry, the dump feature's known issues and date), `docs/CURRENT_STATE.md` (Last updated, Game line, two rows), `docs/ROADMAP.md` (#42 marked done), `CLAUDE.md` and `.claude/skills/handoff/SKILL.md` (dump path). A branch still reading `data/game-dump-1.0.20/` in a new test needs the path changed after merging this.
