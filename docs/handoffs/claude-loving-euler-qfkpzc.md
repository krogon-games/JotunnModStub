# Handoff: `claude/loving-euler-qfkpzc`

**Written:** 2026-11-08 by `/handoff`. 11 commits ahead of `master`, 0 behind; everything pushed; no PR yet.

## Stage

Merged into `master` (PR #58, 2026-11-08) after Krogon confirmed the play-test; the coordinator raised Corruption to 0.5.0, copied the decisions into `docs/DECISIONS.md` and closed #28.

## Purpose

Loot round: Krogon's orb rules and affix defaults ([#28](https://github.com/krogon-games/JotunnModStub/issues/28)), then the currency orbs with their Riftforge rules (first part of ROADMAP L5, [#29](https://github.com/krogon-games/JotunnModStub/issues/29)) and Stonecutter's/Resinous per hit.

## Done

Built and confirmed by Krogon in single player (2026-11-08):
- **Currency orbs:** ten items in the Currency tab (orb art embedded; the Mirror a placeholder gem), no drops yet. Every orb's rules through `co_craft` (the orb taken from the Currency tab), the Mirror with corrupted and mirrored items, Corrupted/Mirrored tooltip lines, all kept after relogging.
- **Stonecutter's and Resinous per damaging hit**, nothing on a "too hard" hit.
- No red errors (a repeated `GetBiome error` warning is the game's own, at a biome border).

## Half-done and known issues

- **Skipped by Krogon:** the "Use this at a Riftforge" refusal more than 10 m away; check it with the window (U3).
- **Not built (rest of L5):** corrupting with a shard, shattering, Affix Splinters. L4: orb drops, sources and rates. The Mirror has no art.
- **Claude's interim choices** (easy to change): orb stack 50, weight 0.2 (the shard's); Alchemy/Chance use the drop affix counts; Annulment picks each affix equally; Seismic/Splintering copies count as hits; "at a Riftforge" = 10 m build range.
- **For the window (U3):** materials must come from the Currency tab (it pulls currency out of the bag); brief §4 says so.

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

`docs/playtests/claude-loving-euler-qfkpzc.md`: orbs snapped back to the Currency tab on the first try (fixed); the re-test passed A–D but one skipped step.

## Open questions for Krogon

None.

## Next steps

1. ~~Open a PR into `master`; Krogon merges it; the coordinator raises Corruption's version.~~ Done (PR #58; Corruption 0.5.0).
2. Next loot round: L5's shard corruption, shattering and Affix Splinters; L4's orb drops (needs Krogon); the Stonecutter's/Resinous rework (decision 8).

## Checks

- Build: PASS (`dotnet test CraftingOverhaul.Tests` 394 passed; `Corruption` builds against the stand-in game folder).
- Version: gameplay changed: version bump needed at merge (Corruption). This branch changed no version number.
- Multiplayer sync: read in code, not tested. Orbs registered in `OnVanillaPrefabsAvailable`; locks in the item's saved data; extra stone/resin spawned by the object's owner (existing chances message); no new config.
- Assets and names: PASS. Orb art and `orb_chance.csv` embedded; vanilla names in the game dump; orb texts added in `OrbItems.cs`, as the essences do.
- FEATURES.md: PASS (row, section, patch targets, changelog).
- In-game: passed in Krogon's single-player play-test (2026-11-08): "Besides that, all steps pass."

## Merge notes

- **Ready to merge: yes.** Final play-test passed (one step skipped by Krogon), no check failed, `master` has nothing this branch lacks, all pushed.
- **Gameplay changed: version bump needed at merge: Corruption.**
- No other remote branch has unmerged commits, so no overlaps. Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `Corruption/CorruptionPlugin.cs`, design docs.
- Coordinator: copy Decisions 1–20 into `docs/DECISIONS.md`; close #28; #29 is partly done.
