# Handoff: `claude/nifty-hypatia-afm8ez`

**Written:** 2026-11-28 (`/handoff`). From `master` at `1612586`; 9 commits ahead, 0 behind; no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

The loot map affixes ([#96](https://github.com/krogon-games/JotunnModStub/issues/96), ROADMAP R1c): the drop affixes from Krogon's list, each one doubling its drop chances (map affix overhaul, decision 7). Also the first corrupted-only affix, Thorns, and the "5th affix on a corrupted map" rule ([#97](https://github.com/krogon-games/JotunnModStub/issues/97), decision 11).

## Done

Nothing yet.

## Half-done and known issues

- **Built, not play-tested** (Corruption and Orlog build): six loot affixes in `map_affixes.csv` (Bountiful, Glittering, Bulwarked, Mailed, Honed, Fletched; Runic `later`), rules in `Corruption/Rules/MapLootRules.cs`, hooked into the kill loot (`LootPatches`) and the heart's spill (`RipLoot`); `co_riploot` to check the rates. No new Harmony patch.
- **Built, not play-tested** (798 tests): corrupted-only affixes (a `pool` column; the shard adds one as a corrupted map's 5th; the server keeps one on a corrupted full map) and **Thorns** (5–25%, `Character.RPC_Damage` prefix + postfix, `MapAffixEffects.Thorns`). Game code decompiled (Krogon allowed it) to check where health is applied.
- **Open bug, not investigated:** Krogon's game crashed when he opened the Riftforge during his first testing (2026-11-28). The cause isn't known yet, and it may not come from this branch. Needed: the build, what was in the forge or bag, and `LogOutput.log` (details in the play-test file).
- Claude's choices on Thorns (changeable): the returned hit is blunt, has no attacker (so Brutal and the game's per-player enemy scaling don't raise it), no stagger and no push; a killing blow counts what the creature had left.
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

Solo checklist `docs/playtests/claude-nifty-hypatia-afm8ez.md` (`la-1` … `la-20`; D, Thorns, needs a build after the Thorns commit), also on the phone page.

Not started. Krogon wants a new session to work on more affixes first (2026-11-28); Claude gave him a prompt for it (more corrupted-only affixes from #97, on a new branch started from this one).

## Open questions for Krogon

None.

## Next steps

1. Investigate the Riftforge crash (Krogon's log and steps first), then Krogon's solo play-test (`la-1` … `la-20`).
2. Fix what fails; final play-test; `/handoff`.
3. A PR into `master`. If the follow-up affix session branched from this one, this branch merges first.

## Checks

- Build: **PASS**, 798 tests pass; Corruption builds (stand-in game folder). Orlog unchanged.
- Version: gameplay changed: version bump needed at merge (Corruption). No version changed on this branch.
- Multiplayer sync (read in code, not tested): no new config. The loot affixes and Thorns read the map's existing stamp on the creature and heart ZDOs. Thorns runs on the creature's owner and sends the hit to the player with `Character.Damage` (a game RPC). A map's stored affixes can now hold a 5th, and the server's request check (`MapData.Sanitize`) accepts it on a corrupted map only. Tables are compiled in, so they sync through the version bump.
- Assets and names: **PASS**. No new data file (`map_affixes.csv` already embedded); no new `$` tokens; no vanilla prefab names added.
- FEATURES.md: **PASS**, updated (Map tiers and affixes, its patch table, changelog).
- In-game: **NEEDS KROGON'S PLAYTEST**.

## Merge notes

Ready to merge: **no**: an open crash at the Riftforge, and the play-test isn't done. Gameplay changed: **version bump needed at merge: Corruption**. Not behind `master`. No other remote branch has unmerged commits, so no file clashes. Shared docs changed: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (the phone-page line), `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/mod-compatibility.md`, `Corruption/Data/README.md`. A later branch started from this one must merge after it.
