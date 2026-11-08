# Handoff: `claude/hopeful-thompson-eyncm8`

**Written:** 2026-11-08 · **Branch:** ahead of `master` (`ced0575`), 0 behind; PR into `master` opened 2026-11-08.

## Stage

Merged into `master` (PR #57, 2026-11-08) after Krogon confirmed the play-test; the coordinator raised Corruption to 0.4.0 and Orlog to 0.2.0, and copied the decisions into `docs/DECISIONS.md`.

## Purpose

Third set of code-review fixes (baseline `e56e91f`): corrupted summons during the spawn delay, non-finite saved affix values, overflow-safe point totals, talisman fight-back time across an owner change.

## Done

Confirmed by Krogon alone (2026-11-08, "A–D passed alone"; no log sent):
- **Corrupted summons:** at Eikthyr and Moder, normal, wrong or repeated corrupted offerings during the delay → one corrupted boss, one corrupted item used; a normal summon stays normal.
- **Affix values:** `co_affix` refuses NaN and infinity; a finite 500% is kept; existing loot unchanged.
- **Points:** spending and relogging work; a lowered `StatPointsPerLevel` keeps the spent points.
- **Talisman fight-back:** 30 s after a hit, refreshed by the next hit; other creatures unaffected.

## Half-done and known issues

- **Multiplayer untested** (left for later by Krogon): the owner logging out mid fight-back, two players offering at one altar. `PLAYTEST.md` → Session 2, 2g.
- Damaged saves themselves (NaN in a save, huge point counts) can't be made in game; covered by the tests only.
- **Game code read in 1.0.14** (the cloud has only body-less 1.0.21 references). The corrupted summon repeats `OfferingBowl.RPC_SpawnBoss`'s steps with the game's own methods: if 1.0.21 changed that body, it isn't picked up.
- Known limit: a corrupted item dropped before the owner's answer arrives isn't taken (vanilla has the same gap).
- Runtime compatibility and performance profiling are still outstanding (review note).

## Decisions

- 2026-11-08: none needed for this round: the four corrections follow the review, which says no new balance decision is required. (Krogon asked for the fixes directly.)
- 2026-11-08: finish the round on the solo play-test; the multiplayer part (E) comes later ("A–D passed alone, multiplayer later").

## Playtest

[`docs/playtests/claude-hopeful-thompson-eyncm8.md`](../playtests/claude-hopeful-thompson-eyncm8.md): A–D passed alone; E (server) left for later, also `PLAYTEST.md` → Session 2, 2g.

## Open questions for Krogon

None.

## Next steps

1. ~~Coordinator: merge into `master` and raise Corruption's and Orlog's versions.~~ Done (PR #57; Corruption 0.4.0, Orlog 0.2.0).
2. On the group evening: Session 2, 2g.

## Checks

- Build: PASS (`/handoff`: 366 tests pass; Corruption and Orlog build, 0 errors).
- Version: gameplay changed: version bump needed at merge (Corruption, Orlog). This branch changed no version.
- Multiplayer sync: PASS, read in code, not tested. Two new altar RPCs (registered on every game, both plugins required everywhere); fight-back time on the creature's ZDO, written only by its owner.
- Assets and names: SKIPPED (no new assets, items or text tokens; new ZDO key `corruption_provoked_<player id>`).
- FEATURES.md: PASS.
- In-game: passed alone (Krogon, 2026-11-08); multiplayer needs Krogon's playtest.

## Merge notes

Ready to merge: yes. Krogon's solo play-test passed, no check failed, pushed; multiplayer deferred by his decision. Up to date with `master`; no other remote branch, so no file clashes. **Gameplay changed: version bump needed at merge (Corruption, Orlog).** Shared docs touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/mod-compatibility.md`. Patch list change: `OfferingBowl.RPC_RemoveBossSpawnInventoryItems` prefix removed, `OfferingBowl.Start` postfix added.
