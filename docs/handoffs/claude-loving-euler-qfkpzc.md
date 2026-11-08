# Handoff: `claude/loving-euler-qfkpzc`

**Written:** 2026-11-08. Branch started from `master` at `96f22e3` (0 behind); no PR yet.

## Stage

**Building:** the orb rules and affix defaults are decided ([#28](https://github.com/krogon-games/JotunnModStub/issues/28)); building the Stonecutter's/Resinous change and the first Riftforge orb rules.

## Purpose

Loot round: settle the Riftforge orb rules and the gathering and proc affix defaults (#28), then build what they change.

## Done

Nothing built or play-tested yet. Documentation only: the decisions below are written into `docs/riftforge-ui-brief.md` §2 and §6 (the Regal Orb row corrected) and the orb table in `docs/corruption-design.md`.

## Half-done and known issues

- **Already built, now confirmed (no code change):** life steal from Reprisal and Wrath, Wrath's shockwave hitting the struck enemy, Rallying on every enemy the empowered attack hits, Deathless using the shortest cooldown, Foraging from wild plants only, Gemseeker's gems equally likely (all in `master`, `Corruption/Content/LootProcs.cs`, `LootHarvest.cs`).
- **Needs code:** Stonecutter's and Resinous rolling per valid gathering hit instead of per harvested piece (`LootHarvest.cs`, `Rules/HarvestRules.cs`, affix text in `affixes.csv`, `FEATURES.md`, `AFFIX_RULES.md`).
- **Needs code (ROADMAP L5):** the orbs themselves; none exist as items yet (`RiftforgeCrafting`, `co_craft`).

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

## Playtest

Nothing to test yet. Checklist will go in `docs/playtests/claude-loving-euler-qfkpzc.md`.

## Open questions for Krogon

None right now (all answered on 2026-11-08: decisions 14–18).

## Next steps

1. Krogon confirms this round's build (question 1).
2. Build it, with tests; play-test checklist.
3. Krogon play-tests; fix; final play-test.

## Checks

Not run yet (`/handoff` runs them).

## Merge notes

- Ready to merge: no, nothing built yet.
- So far docs only (no version bump needed); a Stonecutter's/Resinous change would make it "gameplay changed: version bump needed at merge" for Corruption.
- The coordinator should copy the Decisions into `docs/DECISIONS.md` and close #28.
