# Handoff: `claude/admiring-babbage-rk1w8d`

**Written:** 2026-11-08 · **Branch:** ahead of `master`, 0 behind (`master` `ddc1c28` merged in); PR opened 2026-11-08.

## Stage

Ready to merge.

## Purpose

Fixes from a code review: safe loading of the inventory tabs, Magic/Rare loot on both death paths, exclusive chest access for craft-from-chests, and arrows using their own bow's affixes. Plus a check (no fix) of rip wave limits after the heart changes hands.

## Done

Confirmed by Krogon in single player (2026-11-07/08):
- **Tabs:** a tab whose save can't be read is locked and kept untouched; the other works; the shortened message fits. Test command `co_tabs_damage`.
- **Loot:** creatures whose body carries their loot now roll Magic/Rare (about 5 from 120 greydwarfs, the expected rate).
- **Chests alone:** crafting and building from chests, mixed payment with bag and backpack.
- **Arrows:** life steal and Skadi follow the bow when switching mid-flight (Skadi checked with its devcommands-only "Skadi: stack N" message).

## Half-done and known issues

- **Multiplayer untested** (skipped by Krogon): two players on one chest (the main point of the chest fix), loot per player, arrows on the server. Moved to `PLAYTEST.md` → Session 2, 2e.
- **Rip waves (not fixed):** a suspected undercount when the heart passes to another player's game while survivors are far away. Two log lines added; needs the 3-player check (2e step 4) before any fix.
- The death code was read in the cloud's 1.0.14 assemblies, not 1.0.21 (Krogon's 1.0.21 play-test passed).
- AdventureBackpacks' stock isn't visible when Craft is pressed, so chests may be reserved needlessly (payment order unchanged).
- Hits from other mods still use the weapon in hand (Orlog skills should tag theirs with `HitSources.Tag`).

## Decisions

- 2026-11-08: skip the multiplayer play-test (steps E–H) at this stage; finish the round on the solo play-test ("skipping multiplayer testing at this stage").

## Playtest

[`docs/playtests/claude-admiring-babbage-rk1w8d.md`](../playtests/claude-admiring-babbage-rk1w8d.md): A–D passed (Skadi after fixing the checklist's wrong "blue numbers" step); E–H skipped.

## Open questions for Krogon

None. The rip-wave check (2e step 4) decides whether that counting gets fixed later.

## Next steps

1. Coordinator: merge into `master` and bump the versions.
2. Group evening: Session 2, 2e (chests with two players, loot per player, arrows, rip waves with three players).
3. If the rip-wave undercount is confirmed: fix it, keeping the per-encounter limits.

## Checks

- Build: PASS (CraftingOverhaul and Corruption build against the cloud stand-in; `dotnet test`: 304 passed).
- Version: gameplay changed: version bump needed at merge (CraftingOverhaul, Corruption). No version changed on this branch.
- Multiplayer sync: read in code, not tested. Nothing new to sync; chest access uses the game's own chest messages (works when only some players have CraftingOverhaul).
- Assets and names: no new assets; new token `corruption_tab_locked` is in `Texts.cs`; `CorruptionShard` is ours.
- FEATURES.md: PASS (updated with each change).
- In game: solo passed (Krogon: "Everything seems to work as intended", "c5 and c6 checks out"); multiplayer skipped.

## Merge notes

Ready to merge: yes (solo play-test passed, no check failed, 0 behind `master`, pushed; multiplayer skipped by Krogon). **Gameplay changed: version bump needed at merge (CraftingOverhaul, Corruption).** No overlap with the other open branches (`claude/awesome-ptolemy-gjigjh`, `claude/relaxed-einstein-kdvz4t`). Shared files touched (add, keep both sides): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/mod-compatibility.md`, `Corruption/CorruptionPlugin.cs` (one line), `Corruption/Content/Texts.cs` (one entry).
