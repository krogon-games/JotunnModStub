# Handoff: `claude/admiring-babbage-rk1w8d`

**Written:** 2026-11-07 · **Branch:** started from `master` `e56e91f` (up to date with it); 6 commits ahead; no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

Fixes from a code review, in risk order: safe loading of the inventory tabs, Magic/Rare loot on both death paths, exclusive chest access for craft-from-chests, and arrows using their own bow's affixes. Plus a check (no fix) of rip wave limits after the heart changes hands.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

All built and unit-tested (304 tests pass), **none tried in game**:
- **Tabs:** a tab whose save can't be read completely (damaged, or an item whose prefab is missing) is locked for the session and its saved data kept untouched; the other tab works. Test command `co_tabs_damage`.
- **Loot:** creatures whose body carries their loot (most) never rolled Magic/Rare: the game turns their drops off before our roll. Now both death paths roll, once per death, owner only. The game code was read in the cloud's 1.0.14 assemblies, **not 1.0.21** (no 1.0.21 game code here; a fresh `co_dump` won't show code either).
- **Chests:** a chest is paid from only after its owner hands it over through the game's own chest handshake and it's marked in use; all-or-nothing payment; a refused chest means nothing crafted or taken unless what you carry covers it. Building holds the chests a selected piece needs, for 3 s after the last need. AdventureBackpacks' stock isn't visible when Craft is pressed, so chests may be reserved needlessly (payment order unchanged).
- **Arrows:** knockback, backstab, life steal and Skadi follow the item the hit came from (direct hit, splash, spawned area effects); procs carry their own item. Hits from other mods still use the weapon in hand (Orlog skills should tag theirs).
- **Rip waves (not fixed):** a suspected undercount when the heart passes to another player's game while survivors are far away. Only two log lines added; needs the 3-player check (playtest step H) before any fix.
- 3-second network timeout and 3-second build linger are technical, not gameplay numbers.

## Decisions

None this round (the review gave the fixes; no new balance or design choice was needed).

## Playtest

Checklist: [`docs/playtests/claude-admiring-babbage-rk1w8d.md`](../playtests/claude-admiring-babbage-rk1w8d.md): A–D alone (about 40 min), E–H on the dedicated server with 2–3 players. No feedback yet.

## Open questions for Krogon

None new. Step H's result decides whether rip wave counting gets fixed next round.

## Next steps

1. Krogon's solo play-test (A–D), then the server one (E–H).
2. Fix what fails.
3. If step H confirms the undercount: fix it, keeping the per-encounter limits.
4. Final play-test, then a PR into `master`.

## Checks

- Build: PASS (solution builds against the cloud stand-in; `dotnet test`: 304 passed).
- Version: gameplay changed: version bump needed at merge (CraftingOverhaul, Corruption). No version changed on this branch.
- Multiplayer sync: read in code, not tested. No new settings or synced data; chest access uses the game's own chest messages, so CraftingOverhaul still works when only some players have it.
- Assets and names: new text token `corruption_tab_locked` is in `Texts.cs`; prefab `CorruptionShard` exists (ours).
- FEATURES.md: updated with each fix.
- In game: NEEDS KROGON'S PLAYTEST.

## Merge notes

Not ready to merge: no play-test yet. Gameplay changed: version bump needed at merge (CraftingOverhaul, Corruption). Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/mod-compatibility.md`, `Corruption/CorruptionPlugin.cs` (one line), `Corruption/Content/Texts.cs` (one line), `Corruption/Content/RipWaves.cs` (two log lines), `Corruption/Content/LootPatches.cs`, `LootProcs.cs`, `InventoryTabs.cs`. Add, keep both sides in conflicts.
