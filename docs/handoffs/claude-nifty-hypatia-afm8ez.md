# Handoff: `claude/nifty-hypatia-afm8ez`

**Written:** 2026-11-28 (`/handoff`, final). From `master` at `1612586`; 15 commits ahead, 0 behind; no PR yet.

## Stage

Ready to merge.

## Purpose

The loot map affixes ([#96](https://github.com/krogon-games/JotunnModStub/issues/96), ROADMAP R1c): the drop affixes from Krogon's list, each one doubling its drop chances (map affix overhaul, decision 7). Also the first corrupted-only affix, Thorns, and the "5th affix on a corrupted map" rule ([#97](https://github.com/krogon-games/JotunnModStub/issues/97), decision 11).

## Done

Confirmed by Krogon alone on Valheim 1.0.31 (2026-11-28, phone page, `la-1` … `la-20` all Pass, then `la-18b`): the six loot affixes (tooltips, `co_riploot` rates, kills and the heart's spill), Runic refused, the corrupted 5th affix from the shard and on random tier 6 maps, Thorns in red on the map and the hover, the server keeping it, `DisabledMapAffixes`, and Thorns' returned damage on the training dummy with `co_stamp` (his log: each hit returns 25% of the health it took).

## Half-done and known issues

- **Tested alone (see Done)**; two players not tried: six loot affixes in `map_affixes.csv` (Bountiful, Glittering, Bulwarked, Mailed, Honed, Fletched; Runic `later`), rules in `Corruption/Rules/MapLootRules.cs`, hooked into the kill loot (`LootPatches`) and the heart's spill (`RipLoot`); `co_riploot` to check the rates. No new Harmony patch.
- **Tested alone (see Done)**; two players not tried (798 tests): corrupted-only affixes (a `pool` column; the shard adds one as a corrupted map's 5th; the server keeps one on a corrupted full map) and **Thorns** (5–25%, `Character.RPC_Damage` prefix + postfix, `MapAffixEffects.Thorns`). Game code decompiled (Krogon allowed it) to check where health is applied.
- **Freeze, didn't come back (decision 17):** Krogon's game **froze** when he pressed E on the Riftforge with the inventory on the Maps tab and nothing in the forge (2026-11-28, this branch's build `608bac6`). His log ends at the game opening the crafting window, with no error. The cause isn't known; no loop in this branch's changes runs there, so it may be older. Next: try `master`'s build, Unity's `Player-prev.log`, and the Bag tab (details in the play-test file).
- **Balance, open:** Thorns may be too weak. Krogon: "i saw damage numbers all below 1, such as 0.3 0.7 0.4 etc. i was using an iron axe. may need to buff thorns". It works as built, but the game's armour cuts small hits to almost nothing (a 5-damage hit against his armour lands as about 0.3). For a balance round: a bigger share, or damage that ignores armour (decision 14 chose armour).
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
16. **A test command for Thorns on this branch** (2026-11-28): "Yes, on this branch". After his la-18 note ("its really difficult to tell if i take damage from hitting another creature with thorns on. we could potentially add a buildable dummy to the game to test damage and affixes on."), Claude pointed to the game's own training dummy (T.W.I.G., or `spawn TrainingDummy`). The new piece is a console command that gives the creature you look at any map affixes, and prints each Thorns return. Krogon re-tests la-18 with it before the merge.
17. **The Riftforge freeze didn't happen again** (2026-11-28): "It didn't happen again". It's left as a note, not a blocker; open a ticket if it comes back.
18. **The training dummy is the test target from now on** (2026-11-28): "i like the training dummy. please remember we can use this in the future for testing all kinds of affixes and mechanics." Written into `CLAUDE.md` (Build and test) and `docs/playtests/README.md`.

## Playtest

Solo checklist `docs/playtests/claude-nifty-hypatia-afm8ez.md` (`la-1` … `la-20`; D, Thorns, needs a build after the Thorns commit), also on the phone page.

Not started. Krogon wants a new session to work on more affixes first (2026-11-28); Claude gave him a prompt for it (more corrupted-only affixes from #97, on a new branch started from this one).

## Open questions for Krogon

- Thorns' strength (above): leave it, raise the 5–25%, or let it ignore armour? Not needed for the merge.

## Next steps

1. The coordinator merges this branch into `master` (PR), with the Corruption version bump.
2. Then `claude/wizardly-wozniak-4aix76`, which was started from this branch, merges after it.
3. Later: Thorns' balance; two-player checks (Thorns on a creature another game runs, the loot affixes' personal drops).

## Checks

- Build: **PASS**, 798 tests pass; Corruption builds (stand-in game folder). Orlog unchanged.
- Version: gameplay changed: version bump needed at merge (Corruption). No version changed on this branch.
- Multiplayer sync (read in code, not tested): no new config. The loot affixes and Thorns read the map's existing stamp on the creature and heart ZDOs. Thorns runs on the creature's owner and sends the hit to the player with `Character.Damage` (a game RPC). A map's stored affixes can now hold a 5th, and the server's request check (`MapData.Sanitize`) accepts it on a corrupted map only. Tables are compiled in, so they sync through the version bump.
- Assets and names: **PASS**. No new data file (`map_affixes.csv` already embedded); no new `$` tokens; no vanilla prefab names added.
- FEATURES.md: **PASS**, updated (Map tiers and affixes, its patch table, changelog).
- In-game: **PASS (alone)**: Krogon's phone page, all 20 steps and `la-18b` Pass (2026-11-28). Two players untested.

## Merge notes

Ready to merge: **no**: la-18 (Thorns damage) is to be re-tested with the new test command. Gameplay changed: **version bump needed at merge: Corruption**. Not behind `master`. No other remote branch has unmerged commits, so no file clashes. Shared docs changed: `CLAUDE.md` (the training-dummy line), `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (the phone-page line), `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/mod-compatibility.md`, `Corruption/Data/README.md`. `claude/wizardly-wozniak-4aix76` (more corrupted-only affixes) was started from this branch: it merges after this one. It shares 32 files with this branch, all from being built on it. This branch's last commits (`co_stamp`, the play-test results, the `CLAUDE.md` test-target line) came after it branched, so it should merge this branch or `master` again before its own merge. Play-test 2026-11-28: all 20 steps Pass; la-18b (Thorns on the training dummy with `co_stamp`) is the one re-test left.
