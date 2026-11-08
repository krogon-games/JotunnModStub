# Handoff: `claude/hopeful-thompson-eyncm8`

**Written:** 2026-11-08 · **Branch:** started from `master` `ced0575`, 0 behind; no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

Third set of independent code-review fixes (baseline `e56e91f`): corrupted summons during the spawn delay, saved affix values that aren't finite numbers, overflow-safe stat and skill point totals, and the talisman fight-back time when a creature changes owner.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built, 366 tests pass, untested in game:
- **Corrupted summons:** the altar's owner starts the summon (refused while one is queued) and marks it corrupted only once it started; the offering player's game then takes the one item. Normal or wrong items, or a second corrupted one, during the delay change nothing. The old ownership grab (which let a second player summon a second boss) is gone.
- **Saved affix values:** NaN or infinity makes the item give no affix effects, logged once; the saved text is untouched and `co_affix` won't overwrite it. Finite values are never clamped.
- **Point totals:** sums and additions saturate; a saved count too big for an int reads as int.MaxValue (still spent), never 0; no refunds or removals; a lowered setting behaves as before.
- **Talisman fight-back:** stored on the creature's ZDO per player id, in world time; a log line per provoking hit shows which game runs the creature.
- **Game code read in 1.0.14** (the cloud has only body-less 1.0.21 references). The corrupted summon repeats `OfferingBowl.RPC_SpawnBoss`'s steps with the game's own methods: if 1.0.21 changed that body, it isn't picked up.
- Known limits: the corrupted item is taken when the owner's answer arrives; if the player dropped it meanwhile, nothing is taken (the game's own summon has the same gap). Fight-back entries stay on the creature's saved data (one number per provoking player).
- Runtime compatibility and performance profiling are still outstanding (review note).

## Decisions

- 2026-11-08: none needed for this round: the four corrections follow the review, which says no new balance decision is required. (Krogon asked for the fixes directly; the round and branch weren't separately confirmed.)

## Playtest

[`docs/playtests/claude-hopeful-thompson-eyncm8.md`](../playtests/claude-hopeful-thompson-eyncm8.md): A–D alone, E with a friend (also `PLAYTEST.md` → Session 2, 2g).

## Open questions for Krogon

None.

## Next steps

1. Krogon: play-test A–D alone; E on the group evening.
2. Fix whatever the play-test finds; final play-test.
3. Open the PR into `master`.

## Checks

- Build: PASS (solution builds, 0 errors; 366 tests pass).
- Version: gameplay changed: version bump needed at merge (Corruption, Orlog). This branch changed no version.
- Multiplayer sync: PASS, read in code, not tested. Two new altar RPCs (registered on every game, both plugins required everywhere); fight-back time on the creature's ZDO, written only by its owner.
- Assets and names: SKIPPED (no new assets, items or text tokens; new ZDO key `corruption_provoked_<player id>`).
- FEATURES.md: PASS.
- In-game: needs Krogon's playtest.

## Merge notes

Ready to merge: no (play-test outstanding). **Gameplay changed: version bump needed at merge (Corruption, Orlog).** Shared docs touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/mod-compatibility.md`. Patch list change: `OfferingBowl.RPC_RemoveBossSpawnInventoryItems` prefix removed, `OfferingBowl.Start` postfix added.
