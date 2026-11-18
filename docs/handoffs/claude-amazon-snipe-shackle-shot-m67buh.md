# Handoff: `claude/amazon-snipe-shackle-shot-m67buh`

**Written:** 2026-11-18, at the start of the round. Started from `master` (`f2b0ca0`); no pull request yet.

## Stage

Asking Krogon questions.

## Purpose

The Amazon's last two bow skills, Snipe and Shackle Shot ([#70](https://github.com/krogon-games/JotunnModStub/issues/70), roadmap C5b), and the two questions carried over from PR #71 and PR #74.

## Done

Nothing play-tested yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **2026-11-18: this round is Snipe and Shackle Shot, both, on this branch** ("Yes, both skills"), with the two carried-over questions (area hits and Corruption's on-hit affixes; large roots and Wind Lance); over Snipe only and Shackle Shot only.
2. **2026-11-18: Snipe's charge counts from full draw, the damage grows until the charge is full (let go any time), and the full charge gets quicker with skill level** ("Grows with skill level"), over a fixed 3 s (Wind Lance's first hold) and a fixed 1.5 s (Wind Lance's hold).
3. **2026-11-18: a full Snipe charge takes 3.5 s at skill level 1 and 1.5 s at level 20** (his own answer: "3.5s lvl1 - 1.5s lvl 20"), over 3 → 1.5 s, 1.5 → 0.75 s and 4.5 → 1.5 s. Even steps between (Claude's assumption, as the other skills' tables).
4. **2026-11-18: a fully charged Snipe deals 150% of the shot's own damage at skill level 1, 300% at 20** ("150% → 300%"), over 200% → 400%, Wind Lance's 100% → 233% and 300% → 600%.
5. **2026-11-18: Snipe's damage climbs slowly at first and fastest near the end of the charge** ("Slow start, big finish"), over an even climb from 100% and an even climb with a minimum charge. The exact curve and what an early release costs: decision 6.
6. **2026-11-18: Snipe's damage follows a square curve, and any charge fires Snipe and pays its eitr** ("Square curve; pay on any charge"): damage = 100% + (full share − 100%) × charge², where charge runs 0 → 1 over the charge time (level 1: 100% at full draw, 112% at half charge, 150% at full). Let go at full draw with no charge: a normal arrow, no eitr. Over paying only from a full charge and paying in proportion to the charge.
7. **2026-11-18: Snipe zooms 3× at full charge** ("3× at full charge"): the view (the game's 65°) closes in slowly as the charge fills, to about 24° at full charge, with the game's own smooth camera zoom, and springs back when you shoot or let go (as put in the question). Over 2×, 1.5× and growing with skill level.
8. **2026-11-18: Snipe drains stamina at the bow's drawing rate while charging** ("14 a second"): the game's bows drain 14 a second while drawing (Strafe's log, 2026-11-17; less with Bows skill, `ItemData.GetDrawStaminaDrain`) and halve it once fully drawn; during Snipe's charge it doesn't halve. About 49 stamina for a full level-1 charge, 21 at level 20. Over 7 a second (a drawn bow's) and 21 a second.
9. **2026-11-18: Snipe costs 9 eitr, +0.25 per level** ("9, +0.25 per level", Diablo 2's Freezing Arrow's mana: 9 → 13.75 at level 20), over Guided Arrow's 8 − 0.25 and Diablo 2's Strafe's 11.
10. **2026-11-18: Snipe's arrow flies faster with the charge** ("Faster with the charge"): up to twice a fully drawn arrow's speed at full charge (Wind Lance's `speed_pct` 200), a normal arrow's speed with no charge; still the game's own arrow and drop, so it drops less and reaches further. Over a normal arrow's flight and dead straight to 100 m. Claude's choice, open to change: the speed rises evenly with the charge (not on the damage's square curve), so the arc changes predictably.

## Playtest

Not written yet: `docs/playtests/claude-amazon-snipe-shackle-shot-m67buh.md` once built.

## Open questions for Krogon

1. Snipe: the charge (how long, how the damage grows, the zoom, the stamina drain), eitr cost, damage share at levels 1 and 20, range.
2. Shackle Shot: what it ties an enemy to, how long the hold lasts, bosses, eitr cost, damage share, range.
3. Whether both skills work with crossbows too.
4. Carried over: should the burning ground's and the poison cloud's hits count for Corruption's on-hit affixes? Should large roots stop a Wind Lance?

## Next steps

1. Ask the questions above, one at a time.
2. Build both skills, with tests; update `FEATURES.md`, `docs/class-design.md` §5, `docs/CURRENT_STATE.md`.
3. Write the play-test checklist here and on the phone page.

## Checks

Not run yet (`/handoff` runs them).

## Merge notes

Not ready to merge: nothing built yet.
