# Handoff: `claude/nifty-hypatia-afm8ez`

**Written:** 2026-11-28 (start of round). From `master` at `1612586`; no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

The loot map affixes ([#96](https://github.com/krogon-games/JotunnModStub/issues/96), ROADMAP R1c): the drop affixes from Krogon's list, each one doubling its drop chances (map affix overhaul, decision 7).

## Done

Nothing yet.

## Half-done and known issues

- **Built, not play-tested** (790 tests; Corruption and Orlog build): six loot affixes in `map_affixes.csv` (Bountiful, Glittering, Bulwarked, Mailed, Honed, Fletched; Runic `later`), rules in `Corruption/Rules/MapLootRules.cs`, hooked into the kill loot (`LootPatches`) and the heart's spill (`RipLoot`); `co_riploot` to check the rates. No new Harmony patch.
- Claude's reading: Bountiful also raises the (not yet built) unique roll, since uniques are Corruption gear.

## Decisions

1. **Round and branch confirmed** (2026-11-28): "Yes, #96 here". This round is #96, the loot map affixes, on `claude/nifty-hypatia-afm8ez`.
2. **The rune affix is a `later` row** (2026-11-28): "Listed as 'later'". It goes in `map_affixes.csv` but never rolls, like the weather affixes, until a round adds runes.
3. **"More items" covers Corruption's own drops only** (2026-11-28): "Our drops only". It doubles the chances of Corruption's magic and rare gear, orbs and maps. The game's own creature drops (leather, trophies…) are unchanged.
4. **The ×2s multiply** (2026-11-28): "Multiply". The ×2 multiplies with the tier's and the stars' multipliers (tier 3 orbs: 1.75 × 2 = ×3.5), and two loot affixes on one map multiply too ("more items" and "more orbs": orbs ×4).
5. **The shield, armour, melee and ranged affixes tilt the pick** (2026-11-28): "Tilt the pick". The number of gear drops stays the same. When gear drops, items of that kind weigh ×2 in the pick (shields go from about 8% to about 15% of gear drops, and other gear drops a little less often).
6. **"Helmets and Armors" = helmet, chest, legs and cape** (2026-11-28): "Helmet, chest, legs, cape". Backpacks count (they are capes in `loot_bases.csv`); jewellery doesn't.
7. **Ranged = bows, crossbows and magic staffs** (2026-11-28): "Bows, crossbows, staffs". Every other weapon (pickaxes included) is melee.
8. **The loot affixes act on kills and on the heart's spill** (2026-11-28): "Kills and the spill". The heart's per-player orbs and gear get the same ×2s.
9. **Names and copy as Claude proposed** (2026-11-28): "Use these". Bountiful ("Enemies are {value}% more likely to drop gear, orbs and maps."), Glittering (orbs), Runic (runes, `later`), Bulwarked ("Gear drops are {value}% more likely to be shields."), Mailed (helmets and armour), Honed (melee weapons), Fletched (ranged weapons). Value 100 (decision 7 of the map affix overhaul), under "What you'll earn".
10. **The weather affixes are off the priority list** (2026-11-28): "Frostbitten, Drowned, Choking can be removed from priority at this stage, i dont think we will build them. but do not delete them entirely, just disable." They stay in `map_affixes.csv` as `later` rows, which never roll (already the case), and nothing else is done for them. #86 (`co_dump` weather data) is no longer needed for map affixes.
11. **One corrupted-only affix joins this round** (2026-11-28): "lets add one corrupted affix to this round now. pick the easiest." Claude picked **Thorns** ("Monsters have thorns.", #97): it needs no new creatures, models or chests.
12. **Thorns rolls 5–25%** (2026-11-28): "5-25%" (his own range, over Claude's 10–20 / 20–40 / fixed 25). That share of the damage a player's hit deals comes back to that player.
13. **Thorns punishes every hit** (2026-11-28): "Every hit". Melee, arrows, staffs and Orlog skills; damage over time (burning, poison ticks) doesn't count.
14. **Armour reduces the returned damage** (2026-11-28): "Yes, like a normal hit". It arrives as a physical hit, so armour and blocking help.
15. **The corrupted affix shows last under "What you'll face", in red** (2026-11-28): "Last under 'face', in red". For example: "Enemies return 15% of the damage you deal to them."

## Playtest

Solo checklist `docs/playtests/claude-nifty-hypatia-afm8ez.md` (`la-1` … `la-13`), also on the phone page.

## Open questions for Krogon

None.

## Next steps

1. Krogon's solo play-test (`la-1` … `la-13`).
2. Fix what fails; final play-test.
3. `/handoff`, then a PR into `master` (gameplay changed: Corruption version bump at merge).
