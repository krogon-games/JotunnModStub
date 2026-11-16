# Handoff: `claude/jolly-albattani-4fruv2`

**Written:** 2026-11-16 (during the round; `/handoff` rewrites this at the end). Ahead of `master`, 0 behind. No pull request yet.

## Stage

**Waiting for Krogon's playtest** of Forked Lightning's last two steps (or his word that they can wait); Wind Lance's rework has passed.

## Purpose

The first two of the Amazon's seven bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): **Forked Lightning** and **Wind Lance**.

## Done

- **Wind Lance (reworked) passed** Krogon's single-player play-test, all 15 steps (2026-11-16, build `9fc528b`): the 1.5 s hold on the draw circle ("yes"), the look, through enemies and along the ground, what stops it, a troll, rank 20, fire arrows, a crossbow, the older skills, no errors.
- **Forked Lightning works** (steps 1–6 passed the same day).

## Half-done and known issues

Detail in `FEATURES.md` → Amazon skill bar and bow skills.
- **Wind Lance failed its first play-test, was reworked, and the rework passed (Done, above):** it flies itself (`Orlog/Content/WindLance.cs`) as a gust 0.75 m each side that runs along the ground, strikes every enemy once, staggers (not bosses) and pushes them; 1.5 s hold shown on the bow's draw circle; a wind look (`WindLanceLook.cs`, preview `docs/images/wind_lance_preview.png`). Read against the game's code (1.0.14 bodies, decompiled this session).
- **Forked Lightning is far too strong at level 1** (Krogon): it follows the damage scale (×22 at character level 1); balancing is for another session (decision 15).
- **Claude's choices, open to change:** the gust runs 0.75 m above the ground and follows slopes up to 38° (the slope a player can stand on, game code); it skips creatures that are neutral until attacked; the look's sizes and colours; the circle narrows to 0.06 and turns pale blue; a crossbow charges with no stamina cost; Forked Lightning's struck-enemy fork is on top of the 3 → 7.
- Other players see the lance as a fast arrow without the wind, and the forks without the lines.
- **Subagents:** the look was built by a Sonnet helper (read and checked by the main session); the Opus review of the flight code stopped at a usage limit, so the main session reviewed it against the game's code itself (one bug found and fixed: a crossbow's charged shot left as a plain bolt).
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

## Playtest

`docs/playtests/claude-jolly-albattani-4fruv2.md`. Round 1: Forked Lightning's steps 1–6 passed, Wind Lance failed. **Round 2: Wind Lance passed, all 15 steps** (build `9fc528b`; his log had no errors). Still on the phone page: Forked Lightning's `r16-7` (a troll with greylings) and `r16-8` (rank 20); the passed steps are off.

## Open questions for Krogon

1. Forked Lightning's last two steps: test them, or leave them for the balancing session?
2. From his log: a lance aimed down more steeply than 38° at close enemies ends at the ground at once, after striking them (twice in 13 shots). Should a steep dive level out and run on too? And should bushes let the gust through (one lance ended at `Bush01`)?
3. Forked Lightning's balance and the damage scale at low levels (a later session, his word).
4. The other five bow skills' details, in the rounds that build them.

## Next steps

1. Krogon's answers to open questions 1 and 2 (a small fix round if he wants the dive or the bushes changed).
2. `/handoff`, then a pull request into `master`.
3. Coordinator at merge: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–22), update #70 (two of seven done), a task for the damage-scale balancing.

## Checks

2026-11-16, after the rework: 588 tests pass; Orlog builds; no version changed; `FEATURES.md`, `mod-compatibility.md` (one new patch: `Hud.UpdateCrosshair` postfix; the `Projectile.OnHit` prefix is gone), the data README and the design docs updated. In-game: needs Krogon's playtest. (`/handoff` runs the full list at the end.)

## Merge notes

**Not ready to merge yet:** Wind Lance's final play-test passed; Forked Lightning's last two steps are unmarked and two small questions are open (above). **Gameplay changed: version bump needed at merge: Orlog.** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/mod-compatibility.md`, `docs/class-design.md`; also `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj` (one linked file).
