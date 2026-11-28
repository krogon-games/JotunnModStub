# Handoff: `claude/optimistic-ramanujan-acdyf8`

**Written:** 2026-11-28, at the start of the round. Started from `master` (`3d8a80b`); no pull request yet.

## Stage

Asking Krogon questions.

## Purpose

C6 ([#41](https://github.com/krogon-games/JotunnModStub/issues/41)): Gungnir's Throw (the basic ghost javelin throw, no skill points), then the spear and javelin skills Krogon picks for this round.

## Done

Nothing yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **2026-11-28: this round is C6, on this branch** ("Yes, go ahead"): Gungnir's Throw first, then the spear skills Krogon picks.
2. **2026-11-28: Gungnir's Throw takes one of the four skill slots** ("It takes a slot"): it shows in the skills panel and goes into a slot like any other skill, fired from right click with a spear in hand; over "right click with no spear skill chosen" (no slot used) and its own key. Still no skill points (decision of 2026-11-13).
3. **2026-11-28: the ghost spear glows gold** ("Golden glow (Gungnir)"): see-through, warm gold, a soft glow and a faint trail; over a pale blue spirit glow and plain see-through. A preview picture comes before it is built.
4. **2026-11-28: the Abyssal Harpoon's Gungnir's Throw doesn't pull** ("No pull"): damage only; the pull stays on the game's own throw. Over copying the game's throw exactly.
5. **2026-11-28: this round builds Lightning Bolt and Plague Javelin with Gungnir's Throw** (both recommended options picked); Poison Javelin, Jab and Impale wait for a later round (#41 stays open for them).
6. **2026-11-28: Gungnir's Throw is a "Basic" card above the skill tabs** (recommended): always there and always learned, dragged into a slot from there; later classes' basic skills go there too. Over a tenth card on the spear tab.
7. **2026-11-28: a Gungnir's Throw hit raises the Spears skill exactly as the game's throw does** (recommended), over raising nothing.
8. **2026-11-28: the ghost spear sticks where it lands, then fades out over about a second** (recommended); nothing to pick up. Over vanishing at once with a gold flash.
9. **2026-11-28: Lightning Bolt and Plague Javelin cost eitr plus the throw's stamina** (recommended; the game's throw: flint spear 8, Nord spear 18, `item_details.csv` `secondary_attack_stamina`), as the bow skills cost eitr plus the draw. Over eitr only.

## Playtest

Nothing to test yet. Checklist: `docs/playtests/claude-optimistic-ramanujan-acdyf8.md` (to be written).

## Open questions for Krogon

1. Gungnir's Throw's last details, then Lightning Bolt's and Plague Javelin's numbers, behaviour and look.

## Next steps

1. Krogon picks this round's spear skills; agree each one's numbers, behaviour, multiplayer and look.
2. Preview picture of the gold ghost spear.
3. Build Gungnir's Throw, then the chosen skills; tests; docs.
4. Play-test checklist (training dummy), on the phone page too.

## Checks

Not run yet (`/handoff` runs them).

## Merge notes

Not ready: nothing built yet.
