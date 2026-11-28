# Handoff: `claude/nifty-hypatia-afm8ez`

**Written:** 2026-11-28 (start of round). From `master` at `1612586`; no PR yet.

## Stage

Asking Krogon questions.

## Purpose

The loot map affixes ([#96](https://github.com/krogon-games/JotunnModStub/issues/96), ROADMAP R1c): the drop affixes from Krogon's list, each one doubling its drop chances (map affix overhaul, decision 7).

## Done

Nothing yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **Round and branch confirmed** (2026-11-28): "Yes, #96 here". This round is #96, the loot map affixes, on `claude/nifty-hypatia-afm8ez`.
2. **The rune affix is a `later` row** (2026-11-28): "Listed as 'later'". It goes in `map_affixes.csv` but never rolls, like the weather affixes, until a round adds runes.
3. **"More items" covers Corruption's own drops only** (2026-11-28): "Our drops only". It doubles the chances of Corruption's magic and rare gear, orbs and maps. The game's own creature drops (leather, trophies…) are unchanged.
4. **The ×2s multiply** (2026-11-28): "Multiply". The ×2 multiplies with the tier's and the stars' multipliers (tier 3 orbs: 1.75 × 2 = ×3.5), and two loot affixes on one map multiply too ("more items" and "more orbs": orbs ×4).

## Playtest

None yet.

## Open questions for Krogon

Asking now: the heart's spill, how the shield/armour/melee/ranged affixes double, which items count as armour and ranged, names and copy.

## Next steps

1. Krogon's remaining answers.
2. Build the affixes (data rows, pure rules with tests, the drop hooks).
3. Write the play-test checklist (here and on the phone page).
