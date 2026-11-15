# Handoff: `claude/laughing-mccarthy-fnz0to`

**Written:** 2026-11-15 (round started). Level with `master`; no pull request yet.

## Stage

**Building.**

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
8. **2026-11-15: Slow Missiles leaves the Passive and Magic tab** ("Slow Missiles"). Its 9: Inner Sight, Critical Strike, Dodge, Avoid, Evade, Penetrate, Decoy, Valkyrie, Pierce.
9. **2026-11-15: Power Strike leaves the Javelin and Spear tab** ("Power Strike"). Its 9: Jab, Impale, Fend, Charged Strike, Lightning Strike, Poison Javelin, Lightning Bolt, Plague Javelin, Lightning Fury (the rest can still change in the spear round, #41).
10. **2026-11-15: Snipe, in Krogon's words:** "Charge a powerful shot, increasing damage the longer the charge. Drains stamina while charging and slowly zooms in on the target."
11. **2026-11-15: Venom Strike is a poison cloud** ("Poison cloud": the arrow bursts into a poison cloud where it lands, poisoning everything that stands in it).

## Playtest

Nothing to test yet.

## Open questions for Krogon

1. "Multi Shot" or "Multiple Shot" (today's name, kept until he says)?
2. Every new skill's details (cost, damage, range, Wind Lance's and Snipe's charge, Shackle Shot against Dota 2's), in the round that builds them.

## Next steps

1. Krogon's answers.
2. Build the cards panel and the new tree rules; show a preview image.
3. Play-test checklist (file and phone page).

## Checks

Not run yet.

## Merge notes

**Ready to merge: no** (nothing built).
