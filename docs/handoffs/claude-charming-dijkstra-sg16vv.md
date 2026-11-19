# Handoff: `claude/charming-dijkstra-sg16vv`

**Written:** 2026-11-19 (session start). Branch started from `master` at `cb49ad6`; no PR yet.

## Stage

Building.

## Purpose

Loot round for [#33](https://github.com/krogon-games/JotunnModStub/issues/33): currency orbs drop in normal play (sources and rates in a new `orbs.csv`). Also carries Splintering's reach and the Stonecutter's/Resinous rework.

## Done

Nothing yet.

## Half-done and known issues

Nothing yet.

## Decisions

1. **This round's scope** (2026-11-19): all three items: orb drops (#33), Splintering's bigger reach, and the Stonecutter's/Resinous rework.
2. **Branch** (2026-11-19): `claude/charming-dijkstra-sg16vv` (not the old `claude/affixes-loot` named in #33).
3. **Every orb can drop from all content** (2026-11-19): any creature kill, everywhere, each player rolling for themselves, with stars and rips raising the chance; chests too. Exalted and Mirror included: "Why are exalted orbs only from corrupted bosses? All orbs should have a chance to drop from all content." / "Why is this gated late game?" **Replaces** the design's sources for the Exalted Orb (tier 3–4 rips, corrupted bosses) and the Mirror (tier 4 rips, corrupted Queen or Fader) in `corruption-design.md` → Currency orbs.
4. **Orb rates follow Path of Exile's rarity order, squeezed between our Magic rate (1 in 50) and Unique rate (1 in 2,500)** (2026-11-19). Per kill, per player, before stars and rips: Transmutation 1 in 50, Augmentation 1 in 77, Chance 1 in 112, Alchemy 1 in 172, Annulment 1 in 172 (not in PoE's table; placed with Alchemy, both "uncommon" in the design), Regal 1 in 830, Divine 1 in 2,500. Jeweller's Orb doesn't drop until sockets exist. Source: PoE's leaked 2014 currency table (quoted at pathofexile.com/forum/view-thread/1848036), mapped on a log scale so Transmutation = 1 in 50 and Divine = 1 in 2,500.
5. **Exalted Orb 1 in 1,865 and Mirror of Kalandra 1 in 21,400 per kill, per player** (2026-11-19), on decision 4's scale (Path of Exile: Exalted 0.055%, between Regal and Divine; Mirror "under 0.001%", so 1 in 21,400 is its most generous place). About 1 Exalted every 15 hours and 1 Mirror every 170 hours per player at 125 kills an hour.
6. **A boss rolls the orb table once, like any kill** (2026-11-19), over "as many rolls as its XP is worth" and fixed boss orbs. Stars, rips and corrupted bosses change the chances as for gear.
7. **Chests give orbs** (2026-11-19): vanilla dungeon and location chests roll the orb table once when the game fills them, at x1.5 (`drop_modifiers.csv` → `dungeon_chest`); the chest a rip heart leaves rolls once per player near the rip, at the rip multiplier.
8. **Splintering reaches 4 m** from the spot hit (2026-11-19), twice the axe's own reach of about 2 m (over 3 m and 6 m). Krogon's play-test note of 2026-11-11: "needs a little bigger radius/ hit detection range".
9. **Stonecutter's becomes an orb finder** (2026-11-19): "X% chance a rock or ore chunk you break rolls the orb table", same 5–25% range, over the biome's special stone and double stone. **Replaces** "5–25% chance per hit of one stone" (2026-11-08) and ends its rework flag.
10. **Resinous becomes an orb finder** (2026-11-19): "X% chance a tree, log or stump you fell rolls the orb table", same 5–25% range, over the biome's special wood and 5 resin per tree. **Replaces** "5–25% chance per hit of one resin" (2026-11-08) and ends its rework flag.

## Playtest

Nothing to test yet.

## Open questions for Krogon

None yet.

## Next steps

1. Propose orb sources and rates from the game's drop tables; get Krogon's approval.
2. Propose Splintering's new reach and the Stonecutter's/Resinous rework options.
3. Build, test, write the play-test checklist (file and phone page).

## Checks

Not run yet.

## Merge notes

Not ready: nothing built yet.
