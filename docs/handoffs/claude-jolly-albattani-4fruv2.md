# Handoff: `claude/jolly-albattani-4fruv2`

**Written:** 2026-11-16 by `/handoff`. 15 commits ahead of `master` with this one, 0 behind. No pull request yet.

## Stage

**Ready to merge.**

## Purpose

The first two of the Amazon's seven bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): Forked Lightning and Wind Lance.

## Done

Krogon's single-player play-tests (2026-11-16):
- **Wind Lance:** the rework's 15 steps (build `9fc528b`), then the bush check (build `00c7b57`; "done, passed.").
- **Forked Lightning:** steps 1–6 ("works well it seems").

## Half-done and known issues

Detail in `FEATURES.md` → Amazon skill bar and bow skills.
- **Forked Lightning is far too strong at level 1** (the damage scale, ×22 at character level 1). Its balance and its last two steps (a troll with greylings; rank 20) wait for a later session.
- **The small-plant list has three odd members** (his log): a crow and two kinds of large root (`Greydwarf_Root`, `HugeRoot1`) drop only wood too, so the gust passes them, harmlessly.
- **Claude's choices, open to change:** the gust runs 0.75 m above the ground and follows slopes up to 38°; it skips creatures that are neutral until attacked; the look's sizes and colours; the draw circle narrows to 0.06 and turns pale blue; a crossbow charges with no stamina cost; Forked Lightning's struck-enemy fork is on top of the 3 → 7.
- Other players see the lance as a fast arrow without the wind, and the forks without the lines. Nothing was tried with two players (`PLAYTEST.md` → Session 2, 2j step 7).
- Subagents: a Sonnet helper built the look (checked by the main session); the Opus review stopped at a usage limit, so the main session reviewed the flight against the game's code itself.
- Five bow skills still say "Coming later": Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot.

## Decisions

1. **2026-11-16: this round is Forked Lightning and Wind Lance** (issue #70's first two), chosen over "three bow skills" and "the spear round instead" ("Forked Lightning + Wind Lance").
2. **2026-11-16: the round runs on this branch**, `claude/jolly-albattani-4fruv2` ("Yes, that branch").
3. **2026-11-16: Forked Lightning takes Diablo 2's Charged Strike's numbers** ("Charged Strike's", over Lightning Fury's and Lightning Strike's): forks 3 at skill level 1 rising to 7 at 20; each fork 1–30 lightning at level 1 up to 1–322 at 20 (D2 points, × the damage scale); eitr 4, +0.25 per level (`data/d2-reference/amazon_levels.csv`). The arrow itself hits normally first.
4. **2026-11-16: a fork reaches 4 m** from the enemy the arrow hit ("4 m", over 6 m and 8 m).
5. **2026-11-16: the struck enemy always takes one fork itself** ("It takes one fork itself"), on top of the arrow, so the skill isn't wasted on a lone enemy or a boss; the forks to other enemies come on top.
6. **2026-11-16: Wind Lance charges by holding past full draw** ("Hold past full draw", over "full draw only" and "any draw, weaker early"): after the bow is fully drawn the player keeps holding for an extra moment before the lance is ready (how long: asked).
7. **2026-11-16: the lance flies through every enemy in its path** ("Through every enemy"), hitting and knocking back each, at full damage each; it stops at terrain, trees and buildings.
8. **2026-11-16: Wind Lance takes Diablo 2's Guided Arrow's numbers** ("Guided Arrow's numbers"): the arrow's own damage (no wind damage type; the arrow's element carries) ×100% at skill level 1, +7% per level (233% at 20; D2R); eitr 8 at level 1, falling 0.25 per level to 3.25 at 20 (`data/d2-reference/amazon_levels.csv`).
9. **2026-11-16: the knockback grows with skill level** ("Grows with skill level"): from 100 at skill level 1 to 200 at 20 (the game's push numbers: the Staff of Embers 100, an iron sledge 200; `data/game-dump-1.0.20/item_details.csv`, `m_attackForce`).
10. **2026-11-16: the extra hold is 3 seconds** after full draw (Krogon's own number, "3 seconds", over the offered 0.5, 1 and 1.5 s). Stamina keeps draining at the bow's rate while holding; a sound and a glow mark the lance as ready; released earlier it is a normal arrow and costs no eitr (Claude's proposal in the question, not objected to).
11. **2026-11-16: the lance flies twice as fast** as a fully drawn arrow from the same bow ("Twice as fast", over 1.5× and 3×).
12. **2026-11-16: the lance fades after 50 m** ("50 m", over 30 m and no limit).
13. **2026-11-16: fix Wind Lance before the rest of the play-test** ("theres a few more steps to playtest but lets fix windlance first. it feels underwhelming"). His report: "It doesnt seem to penetrate targets, i was trying my best to line greylings up in a line and then shoot the first one and get the ones behind him too but only the first one got damaged. I think out of a dozen shots, only 1 of them pushed the enemy back."
14. **2026-11-16: Wind Lance's look: wind swirling tightly around a bigger arrow** ("when you release the charged shot, from the players perspective it just looks like a mostly normal arrow. It could have wind particles around the arrow like a tornado. This is a shot from elden ring, i would make it much tighter around the arrow. and make the arrow bigger in size"; his picture: `docs/images/wind_lance_reference.webp`).
15. **2026-11-16: Forked Lightning works but is far too strong at level 1; balancing is for another session** ("The forked lightning works well it seems… Altough it does way too much damage at level 1. I spawned 5 level 5 greylings and almost one shot all of them with 1 arrow. Thats way too strong. we can make a balancing pass another session, just letting you know it works but is too strong").
16. **2026-11-16: subagents may be used for this fix** ("use subagents and whatever tools required").
17. **2026-11-16: the hold is 1.5 seconds, not 3** (replaces decision 10's number): "I suspect it was my error for not holding the charge long enough. I tested again and it works fine now, the charge is LOOONG. lets reduce it to 1.5 seconds."
18. **2026-11-16: the charge needs an indicator on the bow's own draw circle:** "Also we need an indicator of some kind. Just like the yellow circle narrows in normally when you do a charge on the bow, it should continue to narrow in and maybe turn a different colour?"
19. **2026-11-16: the ground no longer stops the lance; it runs along it** ("Run along the ground": where it would dig in, it levels out and rushes on just above the ground and over water, following gentle slopes, to its 50 m; trees, rocks, buildings and steep hillsides still stop it; shot at something in the air it flies straight). Replaces decision 7's "stops at terrain".
20. **2026-11-16: the gust is 0.75 m wide each side of its centre line** ("0.75 m each side", over 0.5 m, 1 m and arrow-thin).
21. **2026-11-16: the knockback stays 100 → 200 and every enemy it passes through is staggered** ("Keep 100 to 200, add stagger"; bosses excepted, as offered in the question), over 200 → 400.
22. **2026-11-16 (evening): Wind Lance's rework passed** (all 15 steps Pass on the phone page; on "is 1.5 seconds right, is the circle clear enough?": "yes"; "see playtest results and log").
23. **2026-11-16: a steep downward shot still ends at the ground** ("Leave it as it is": a lance aimed down more steeply than 38° strikes the dirt, after hitting the enemies right there), over "always level out".
24. **2026-11-16: bushes and small plants let the gust through** ("Pass through bushes": bushes, saplings and other small plants no longer stop it and are not damaged; trees, logs, rocks and buildings still stop it).
25. **2026-11-16: Forked Lightning's last two steps go to the balancing session** ("Leave them for the balancing session": a troll with greylings at its feet; rank 20 with nine brutes); this round can be merged without them.
26. **2026-11-16 (late evening): the bush check passed** ("done, passed."; `r16c-1` Pass on the phone page; build `00c7b57`). The round's play-tests are complete.

## Playtest

`docs/playtests/claude-jolly-albattani-4fruv2.md`: **passed.** Round 1: Forked Lightning's steps 1–6 passed, Wind Lance failed. Round 2: Wind Lance reworked, all 15 steps passed. Round 3: the lance through a bush passed. Nothing of this round is left on the phone page.

## Open questions for Krogon

1. Forked Lightning's balance and the damage scale at low levels, with its two open steps: a later session (his word).
2. Should large roots stop the lance (they are on its pass-through list)?
3. The other five bow skills' details, in the rounds that build them.

## Next steps

1. Pull request into `master`; Krogon merges.
2. Coordinator: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–26), update #70 (two of seven done), a task for Forked Lightning's balance and the damage scale.
3. Next classes round, new branch: more of #70's bow skills, or the balancing pass.

## Checks

- Build: PASS (589 tests; Orlog builds, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested (no new setting, message or saved key; hits and the stagger go through the game's own messages; the look is local; the skill tables are compiled in and sync through the version bump).
- Assets and names: PASS (no new data file; new code files are picked up by the project; `StaffLightning` is in the game dump; no new text tokens).
- FEATURES.md: PASS (patch list matches the code: one new `Hud.UpdateCrosshair` postfix).
- In-game: PASSED in Krogon's single-player play-tests (above); two players untested.

## Merge notes

**Ready to merge: yes** (final play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog.** No other remote branch has unmerged commits. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/mod-compatibility.md`, `docs/class-design.md`; also `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj` (one linked file).
