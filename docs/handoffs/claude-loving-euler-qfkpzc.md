# Handoff: `claude/loving-euler-qfkpzc`

**Written:** 2026-11-08. Branch started from `master` at `96f22e3`; 0 behind `master`, all commits pushed; no PR yet.

## Stage

**Waiting for Krogon's playtest** (`docs/playtests/claude-loving-euler-qfkpzc.md`).

## Purpose

Loot round: settle the Riftforge orb rules and the gathering and proc affix defaults ([#28](https://github.com/krogon-games/JotunnModStub/issues/28)), then build them: the currency orbs with their Riftforge rules (first part of ROADMAP L5, [#29](https://github.com/krogon-games/JotunnModStub/issues/29)) and Stonecutter's/Resinous per hit.

## Done

Nothing play-tested yet.

## Half-done and known issues

Built, untested (394 tests pass; Corruption builds):
- **Currency orbs:** ten items (`OrbItems.cs`, the orb art now embedded), going to the Currency tab; **no drops** (L4 needs sources and rates). Rules in `Rules/OrbRules.cs` (unit-tested), the brief's API in `Content/RiftforgeCrafting.cs`, `co_craft list | <orb> | give | testcorrupt` (the orb comes from the bag or the Currency tab). Tooltips show Corrupted / Mirrored. New table `orb_chance.csv`.
- **Stonecutter's and Resinous per damaging hit** (`HarvestRules.PerHit`, postfixes on the five owner-side hit methods in `LootHarvest.cs`); affix text says "per hit".
- **Already built, confirmed by decisions 7, 9–13 (no change):** Gemseeker's, life steal, Wrath, Foraging, Rallying, Deathless.

Claude's interim choices (not Krogon's decisions; easy to change):
- Orb stack 50 and weight 0.2, copied from the shard, until L4's `orbs.csv`.
- The Mirror has no art: a tinted vanilla gem (`GemstoneBlue`) for now.
- Alchemy and Chance roll the same affix counts as drops (`loot_rules.csv`: Magic 1–2, Rare 3–4).
- Annulment picks each affix equally (a resonant line is 2 affixes).
- Copies of a swing (Seismic, Splintering, a breaking rock's hit on its copper deposit) count as hits for Stonecutter's, as they do for "only your swings"; an Odin's Grace whole-tree harvest counts as one hit.
- "At a Riftforge" = within its build range. Extra refusals: "Mirrored items can't be changed", "This item can't take affixes", "This item's affixes can't be read" (brief §2 updated).
- Not built yet in L5: corrupting with a shard, shattering, Affix Splinters.

## Decisions

All from Krogon on 2026-11-08, answering #28:

1. **Orb of Annulment:** removes one random affix and never changes the rarity, even at 0 affixes left. This also avoids turning a Rare into a Magic item whose remaining affixes break Magic's prefix/suffix limits.
2. **Orb of Augmentation:** only on a Magic item with exactly 1 affix; adds a second; stays Magic.
3. **Regal Orb:** only on a Magic item with exactly 2 affixes; adds a third and makes it Rare. The brief, which allowed any Magic item, is corrected.
4. **Orb of Chance** on a Normal item: Magic 50%, Rare 30%, Unique 20%. When no unique can come from that item, the result is always Rare (100%).
5. **Mirror of Kalandra:** can copy corrupted items. The copy keeps the original's affixes and corruption, can't be modified or mirrored; the original stays as it was. The one exception to "no orb works on corrupted items".
6. **Jeweller's Orb:** shown greyed out until sockets and runewords are designed; can't be used or used up while disabled.
7. **Gemseeker's:** every eligible gem equally likely; the biome rules stay.
8. **Stonecutter's and Resinous:** roll on each valid gathering hit instead of each harvested piece. Stonecutter's: 5–25% chance per valid mining hit of one stone. Resinous: 5–25% per valid woodcutting hit of one resin. Both work even on targets that don't normally drop stone or resin. **Both flagged for a rework:** the effect feels boring and the reward negligible; per-hit is the interim design.
9. **Life steal:** keeps healing from Reprisal and Wrath damage.
10. **Wrath:** the shockwave also damages the enemy the combo finisher struck, as well as nearby enemies.
11. **Foraging:** wild plants only; not flint, loose stones or branches.
12. **Rallying:** the bonus applies to every enemy the empowered attack hits.
13. **Deathless:** several equipped Deathless items share one cooldown, the shortest rolled one; extra copies give no extra activations.

Later on 2026-11-08, answering this branch's questions:

14. **This round builds** the Stonecutter's/Resinous per-hit change **and** the first Riftforge orb rules with `co_craft` (ROADMAP L5).
15. ~~Orb of Augmentation on Normal items~~: withdrawn by Krogon the same day; Krogon asked why a 0-affix Magic item is stuck instead (open question 2). Decision 2 stands until answered.
16. **Orb of Chance always giving a Rare until uniques exist** (no uniques yet, ROADMAP L6): fine for now.
17. **A valid hit for Stonecutter's and Resinous** is any hit with the right tool (pickaxe on rock or deposit, axe on tree, log or stump) that does damage.
18. **Orb of Augmentation (design change, replaces decision 2):** works on a Magic item with 0 or 1 affix (adds one; stays Magic), so a Magic item emptied by Annulment can be brought back. Not on Normal items (that's Transmutation's job). Krogon's pick of Claude's option A over "Annulment turns an emptied Magic item Normal" and "leave it".

Later on 2026-11-08, after the first play-test try:

19. **Fix `co_craft` and test the orbs now** rather than wrapping up and testing them once the Riftforge window exists (Krogon left the choice to Claude, who recommended the fix). The orbs couldn't stay in hotbar slot 2: the Currency tab pulls them back.

20. **Skip the "Use this at a Riftforge" check away from the station** for now: players can't craft without the Riftforge window anyway (`co_craft` is a cheat), so it's checked with the window (U3).

## Playtest

Checklist: `docs/playtests/claude-loving-euler-qfkpzc.md` (A orb items, B using orbs, C Mirror and corrupted, D Stonecutter's/Resinous; about 30 minutes, alone). First try: orbs snapped back from hotbar slot 2 to the Currency tab, so `co_craft` now takes them from the tab; A–C rewritten, waiting for the re-test.

## Open questions for Krogon

None (decisions 14–18 answered them).

## Next steps

1. Krogon play-tests A–D and reports.
2. Fix what fails; final play-test.
3. `/handoff`, then a PR into `master`.
4. Next L5 part: corrupting with a shard, shattering and Affix Splinters; L4: orb drop sources and rates (needs Krogon).

## Checks

Not run yet (`/handoff` runs them). So far: `dotnet test CraftingOverhaul.Tests` 394 passed; Corruption builds against the stand-in game assemblies.

## Merge notes

- Ready to merge: no, waiting for the play-test.
- **Gameplay changed: version bump needed at merge** for Corruption (new items, orb rules, Stonecutter's/Resinous behaviour, `affixes.csv` text, new `orb_chance.csv`).
- Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (Session 3d steps 6–7 point here now), `Corruption/CorruptionPlugin.cs`, `docs/mod-compatibility.md`, `docs/AFFIX_RULES.md`, `docs/riftforge-ui-brief.md`, `docs/corruption-design.md` (orb table), `docs/GAME_DESIGN.md`.
- The coordinator should copy the Decisions into `docs/DECISIONS.md` and close #28.
