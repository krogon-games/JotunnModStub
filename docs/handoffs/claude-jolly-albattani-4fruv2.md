# Handoff: `claude/jolly-albattani-4fruv2`

**Written:** 2026-11-16 (during the round; `/handoff` rewrites this at the end). Ahead of `master`, 0 behind. No pull request yet.

## Stage

**Waiting for Krogon's playtest.**

## Purpose

The first two of the Amazon's seven bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): **Forked Lightning** and **Wind Lance**.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Detail in `FEATURES.md` → Amazon skill bar and bow skills.
- **Forked Lightning and Wind Lance are built, untested in-game** (577 tests pass, the three plugins build). Only Orlog's code changed.
- **Written without the game's code open:** the cloud session wasn't allowed to install the decompiler this time, so both skills rest on how `Projectile.FixedUpdate`, `Projectile.OnHit` and the bow draw worked when read in earlier sessions. The riskiest part: the lance flying on after an enemy (a `Projectile.OnHit` prefix skips the game's hit).
- **Claude's choices, open to change:** the struck enemy's fork is on top of the 3 → 7 forks to others; the lance's look (the arrow drawn larger, pale trail and light) and sounds (the game's perfect-block ring for "ready", the dodge whoosh on release); a crossbow charges by holding 3 s with the bolt held back, no stamina cost; stamina while holding is the bow's own hold drain; the bow skill rises once per lance.
- Other players see the lance as a fast arrow and the forks as bursts without the lines.
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

## Playtest

`docs/playtests/claude-jolly-albattani-4fruv2.md`: 21 solo steps (the cards, Forked Lightning, Wind Lance, feel, older skills, log), also the first section of the phone page (ids `r16-1` … `r16-21`). One two-player step added to `PLAYTEST.md` → Session 2, 2j (step 7). No feedback yet.

## Open questions for Krogon

1. After the play-test: does 3 seconds past full draw feel right with the stamina it costs? The lance's look and sounds? The lightning's look?
2. The other five bow skills' details, in the rounds that build them.

## Next steps

1. Krogon builds from this branch and runs the 21 steps (phone page).
2. Fix what the play-test finds; final play-test.
3. `/handoff`, then a pull request into `master`.
4. Coordinator at merge: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–12), update #70 (two of seven done).

## Checks

2026-11-16, after the build: 577 tests pass; Orlog, CraftingOverhaul and Corruption build; no version changed; `FEATURES.md`, `mod-compatibility.md` and the data README updated. In-game: needs Krogon's playtest. (`/handoff` runs the full list at the end.)

## Merge notes

**Not ready to merge** (no play-test yet). **Gameplay changed: version bump needed at merge: Orlog.** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/mod-compatibility.md`, `docs/class-design.md`.
