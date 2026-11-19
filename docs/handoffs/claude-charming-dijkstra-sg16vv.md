# Handoff: `claude/charming-dijkstra-sg16vv`

**Written:** 2026-11-19 (session start). Branch started from `master` at `cb49ad6`; no PR yet.

## Stage

Asking Krogon questions.

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

## Playtest

Nothing to test yet.

## Open questions for Krogon

Asked next: Exalted's and Mirror's rates on the same scale; bosses and chests; Splintering's radius; what Stonecutter's and Resinous become.

## Next steps

1. Propose orb sources and rates from the game's drop tables; get Krogon's approval.
2. Propose Splintering's new reach and the Stonecutter's/Resinous rework options.
3. Build, test, write the play-test checklist (file and phone page).

## Checks

Not run yet.

## Merge notes

Not ready: nothing built yet.
