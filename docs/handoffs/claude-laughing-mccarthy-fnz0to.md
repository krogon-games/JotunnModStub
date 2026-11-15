# Handoff: `claude/laughing-mccarthy-fnz0to`

**Written:** 2026-11-15 (round started). Level with `master`; no pull request yet.

## Stage

**Asking Krogon questions.**

## Purpose

The Amazon's skill tree overhaul ([#66](https://github.com/krogon-games/JotunnModStub/issues/66)): no branching, any skill from level 1, a new bow skill list, and the skills panel redrawn as skill cards from Krogon's mock-up (`docs/images/skills_panel_cards_mockup.webp`).

## Done

Nothing yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **2026-11-15: this round is issue #66, the Amazon's skill tree overhaul, on this branch** ("Yes this is the right round and branch").
2. **2026-11-15: the redesign covers all three Amazon skill tabs and any future class** ("The redesign is for all amazon skill tabs and any future new characters").
3. **2026-11-15: the skills panel shows skills as cards, after Krogon's mock-up** (`docs/images/skills_panel_cards_mockup.webp`: a grid of cards with icon, name, rank and a rank bar; the selected skill's details on the left; the slots and the Learn button along the bottom). The mock-up's look differs from the rest of the UI, so Claude builds it in the existing UI's style ("Lets implement skill cards look. the mock up deviates away in design look quite a bit. So your role is to implement it with consistency to the rest of the UI design").
4. **2026-11-15: the bow tab's final list of 9** (the mock-up's list was only an example): "Final list is Forked Lightning, Wind Lance, Snipe, Multi Shot, Strafe, Freezing Arrow, Immolation Arrow, Venom Strike, Shackle Shot (dota 2 windranger ability)". So Magic Arrow, Fire Arrow, Cold Arrow, Exploding Arrow, Ice Arrow and Guided Arrow go; Snipe, Venom Strike and Shackle Shot are new, with Forked Lightning and Wind Lance.
5. **2026-11-15: this round builds the panel and the rules only** ("Panel and rules only"): the cards panel, no level tiers or prerequisites, the new skill list. Skills that don't work yet show as "Coming later"; a later round builds them.
6. **2026-11-15: every tab has exactly 9 skills** ("Always exactly 9"), for every class. One skill leaves the Passive and Magic tab and one the Javelin and Spear tab (which: asked).
7. **2026-11-15: the fourth attribute stays "Energy"** ("Keep Energy"); the mock-up's "Eitr" isn't taken over.

## Playtest

Nothing to test yet.

## Open questions for Krogon

1. Which skill leaves the Passive and Magic tab, and which the Javelin and Spear tab (9 each)?
2. One line each on what Snipe and Venom Strike do (for their cards).
3. "Multi Shot" or "Multiple Shot" (today's name)?
4. Every new skill's details (cost, damage, range, Wind Lance's charge), in the round that builds them.

## Next steps

1. Krogon's answers.
2. Build the cards panel and the new tree rules; show a preview image.
3. Play-test checklist (file and phone page).

## Checks

Not run yet.

## Merge notes

**Ready to merge: no** (nothing built).
