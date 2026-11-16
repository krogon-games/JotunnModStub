# Handoff: `claude/jolly-albattani-4fruv2`

**Written:** 2026-11-16 (round start; `/handoff` rewrites this at the end). Level with `master` (0 behind). No pull request yet.

## Stage

**Asking Krogon questions.**

## Purpose

The first two of the Amazon's seven bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): **Forked Lightning** and **Wind Lance**.

## Done

Nothing yet.

## Half-done and known issues

Nothing built yet.

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

## Playtest

None yet. The checklist will be `docs/playtests/claude-jolly-albattani-4fruv2.md` and on the phone page.

## Open questions for Krogon

1. Forked Lightning: where its numbers come from (fork count, damage, eitr), how far a fork reaches, what it does to a lone enemy.
2. Wind Lance: how the charge works, whether it passes through enemies, its damage and eitr cost, its knockback and range.

## Next steps

1. Krogon's answers on the two skills.
2. Build both (Orlog), with tests and `FEATURES.md`.
3. Play-test checklist, in `docs/playtests/` and on the phone page.

## Checks

Round start, 2026-11-16: `master` merged (already level); 573 tests pass; no open pull requests; the two old `claude/…` branches on GitHub are fully in `master`.

## Merge notes

Not ready to merge (nothing built).
