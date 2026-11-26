# Handoff: `claude/charming-dijkstra-sg16vv`

**Written:** 2026-11-19 (session start). Branch started from `master` at `cb49ad6`; no PR yet.

## Stage

Building (Krogon's answers to the gameplay-loop review, 2026-11-23); the play-test waits for this.

## Purpose

Loot round for [#33](https://github.com/krogon-games/JotunnModStub/issues/33): currency orbs drop in normal play (sources and rates in a new `orbs.csv`). Also carries Splintering's reach and the Stonecutter's/Resinous rework.

## Done

Nothing yet.

## Half-done and known issues

Built 2026-11-19, untested in game (623 tests pass; Corruption builds against the stand-in game folder):
- **Orb drops** (`OrbDrops.cs`, `OrbDropTable.cs`, `orbs.csv`, the `orb` column of `drop_modifiers.csv`): kills per nearby player; vanilla chests with default loot once when the game fills them (new `Container.AddDefaultItems` postfix); the rip chest once per player near the rip; `co_orbroll` to check rates.
- **Hoardfinder's and of Hidden Riches** (were Stonecutter's and Resinous; decisions 11–13): a 1–5% chance per harvest of one orb, weighted like kills' orbs (the per-hit postfixes are gone). Ids `hoardfinders` / `hidden_riches`, families `orb_find_mining` / `orb_find_woodcutting`; saves with the old ids load through `AffixRenames`, and the two old splinter items stay registered. Tools rolled before keep their old 5–25% value until a Divine Orb rerolls it. 
- **Splintering** reaches 4 m (`loot_rules.csv` → `splintering_reach_m`).
- **Loop fixes (decisions 17–33, 2026-11-23):** maps named "{biome} map"; the rip chest holds one map of its biome and rolls Magic/Rare gear per player; destroying a rip heart sets the world key `corruption_rip_cleared`, after which creatures drop their biome's map 1 in 830 per player once that biome's boss is beaten (`MapDrops.cs`, `MapDropRules.cs`, `co_mapdrops`); every boss kill adds 1 shard per nearby player (`BossShardsPerPlayer`); corrupted bosses give each player one picked orb; dungeon chests hold a shard 1 in 20 (`DungeonChestShardChance`); Riftforge 20 fine wood, 10 nails, 5 cores, 10 amber. Issues: new #84 (explored-land fallback, decision 22), #35 raised to high (decision 23), notes on #31 and #33; ROADMAP, GAME_DESIGN and the design doc updated.

**Valheim 1.0.30 (2026-11-26):** Steam updated the game. Krogon's first start: Orlog's `ConnectPanel.m_root` and CraftingOverhaul's `SoftReference.Name` are gone (fixed in code, untested); PlantEasily 2.3.1 is broken on 1.0.30 (not ours, disabled). Our build references are still 1.0.21's, so other changes may show only when their code runs: a build against 1.0.30's own files is the full check.

Claude's interim choices (easy to change): the splinters of the two orb finders keep their stone-grey and resin-amber colours (`splinter_looks.csv`, `proposed`); orbs aren't locked to the player who rolled them, like gear (L7); chests the world made before this build get no orbs (the game fills each chest once). At 5%: about 1.5 orbs per copper deposit, 1 per about 5 trees; at 1%: 1 per about 3 deposits.

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
11. **Stonecutter's and Resinous drop a guaranteed orb when they trigger** (2026-11-19, Krogon's idea after seeing the wording: "changing the wording to a guaranteed orb drop every x percentage … heavily weighted towards the more common orbs but still a chance to drop a higher rarity"). "X% chance a rock or ore chunk you break drops an orb" / "X% chance a tree, log or stump you fell drops an orb"; X rolls **1–5%** (over a flat 1%, 1–3% and 1–10%; the top is about one kill's worth of orbs, 5.6%). The orb is picked with the orb table's own odds (Transmutation 36% … Mirror about 1 in 1,200; never the Jeweller's). **Replaces** decisions 9–10's "rolls the orb table" and their 5–25% range.
12. **New names for the two orb finders** (2026-11-19; Krogon: "We definitely need to change the name of the affix"): the pickaxe prefix **Stonecutter's → Hoardfinder's** ("Hoardfinder's Iron Pickaxe"; over Andvari's and Dvergr's), the axe suffix **Resinous / "of Resin" → "of Hidden Riches"** ("Iron Axe of Hidden Riches"; over "of the Norns" and "of Yggdrasil"). Sides unchanged; internal ids kept so saved gear and splinters load.
13. **Rename the records too, no new splinter colours** (2026-11-19; Krogon: "I don't think re-colouring the splinters is necessary however I do think making sure all of the records and relevant information is updated appropriately such as the ids and everything else"): the affix ids, effect and family names, splinter look rows, console ids and docs take the new names. **Replaces** decision 12's "internal ids kept". (Correction, 2026-11-19: "saved tools and splinters with the old ids keep working" was Claude's addition, not Krogon's words; decision 14 settles it.)
14. **Old saves and later affix work: Claude decides** (2026-11-19; Krogon, asked whether new affixes with the old ones disabled would have been simpler, and then whether to keep or strip the old-save support: "I am not a developer. You are carrying the main role of developer. I just don't want these issues coming up in the future when we're doing any further affix work"). Claude's choice: keep the old-save support (`AffixRenames`, the two old splinter items) and add a guard: rows of `affixes.csv` are never deleted (a new `retired` status stops one rolling but keeps it readable), renames go through `AffixRenames`, and `shipped_affixes.csv` with two tests makes a missing id or family fail the tests with a plain fix-it message (`docs/ARCHITECTURE.md`, code pattern 10).
15. **The Divine Orb is rarer: 1 in 10,000 kills** (2026-11-19; Krogon, after the per-biome table: "the projected divine orb drops are too common. They are super strong orbs and should be quite rare"), over 1 in 5,000 and 1 in 15,000: about one every 80 hours of fighting, a 10% chance of one by Bonemass. The Exalted stays 1 in 1,865 (Krogon: keep it); nothing else changes. **Replaces** decision 4's Divine (1 in 2,500); the other orbs keep decision 4's scale.
16. **The Orb of Chance is rarer than Alchemy, permanently: 1 in 2,000 kills** (2026-11-20; Krogon: "make Chance rarer than Alchemy permanently. One in 2000 chance"). Why: while no uniques exist every Orb of Chance gives a Rare, the same as Alchemy, and it dropped more often (1 in 112 against 1 in 172); once uniques exist its 20% unique chance would also have outpaced unique drops. It now sits just below the Exalted (1 in 1,865). About 6 orbs an hour per player in all (was 7). **Replaces** decision 4's Chance (1 in 112).

Krogon's answers to Claude's review of the gameplay loop (2026-11-23; the review's ten points, his words quoted):

17. **Maps lose "Corrupted" in their name** (point 1): "Change name from 'corrupted {biome} map' to '{biome} map'. There is no need to include 'corrupted' right away", because corrupted maps come with the map tiers.
18. **The rip chest always holds a map** (point 1): "Include a map in the loot at final chest when you kill the heart. Guaranteed drop."
19. **Maps drop from monsters** (point 1): "Make the drop rate as common as a regal orb" (1 in 830); "Only enable the drop once the first rip heart / map has been completed/destroyed"; the map is always of the biome the drop rolls in ("If the drop rolls in meadows, it should be a meadows map … Never a different biome").
20. **The Queen limit** (point 2) is solved by decisions 18–19: "same solution will fix this issue."
21. **Every boss kill gives shards** (point 3): "Let's just make boss killing more rewarding overall. Guarantee a shard drop each boss kill. 1 per player."
22. **Rips fall back to explored land** when no unexplored land of the biome is left (point 4): "Fall back to explored land that's far from any player built structures."
23. **Personal loot moves up** (point 5): "Loot drops should be individual. Move up priority on this issue" (ROADMAP L7, #35).
24. **No tooltip hint about saving rare orbs** (point 6): "overthinking it … Leave as is."
25. **The rip chest rolls Magic and Rare gear** (point 7): other loot such as Magic and Rare gear should be able to roll in the chest; "if that's not correct, it should be implemented this way". (It wasn't: the rip chest had its vanilla loot, shards and orbs, no rolled gear.)
26. **Corrupted bosses give each player a guaranteed orb** (point 8): "a guaranteed 1 roll per player using the existing drop table": each nearby player gets one orb, picked with orbs.csv's odds (as Hoardfinder's does). Normal bosses keep one ordinary roll (decision 6). **Replaces** decision 6 for corrupted bosses.
27. **No session summary in the log** (point 9): "Let's just see how it played out and what the player feedback is."
28. **Vanilla dungeon chests become a shard source** (point 10): "Then let's build it" (the design's rare chest source, never built).
29. **The Riftforge's recipe is final** (point 10): "20 fine wood, 10 bronze nails, 5 surtling cores, 10 amber." Replaces the first-guess amounts (10 fine wood, 10 nails, 2 cores, plus 1 shard and Eikthyr's trophy).
30. **Boss shards come on top** (2026-11-23, clarifying 21): every boss kill gives each nearby player 1 shard, plus the first-kill 1–3 and the corrupted boss's 3–5 as before (over replacing the first-kill bonus, or both).
31. **A biome's map drops only once its boss is beaten** (2026-11-23, clarifying 19): monster map drops start once the world's first rip heart is destroyed, and a biome's map drops only after that biome's boss has been defeated in the world (over "any biome"). Keeps the rule that each biome's maps need its boss.
32. **One map per rip chest** (2026-11-23, clarifying 18), not one per player: each rip pays back its map; monster drops and essences add more.
33. **Dungeon chests hold a shard 1 time in 20** (2026-11-23, clarifying 28): 5%, the same as a 2-star creature (over 1 in 50 and 1 in 10), once per chest when the game fills it.

## Playtest

`docs/playtests/claude-charming-dijkstra-sg16vv.md` (19 solo steps), also the first section of the phone page (`orb-1` … `orb-12`, `loop-1` … `loop-7`). Group steps: `PLAYTEST.md` → Session 2, 2k (and the page's `mpe-2k-1` … `mpe-2k-3`).

## Open questions for Krogon

None yet.

## Next steps

1. Krogon's solo play-test; fix what it finds.
2. Final play-test, then `/handoff` and a PR into `master`.
3. The group evening: Session 2, 2k.

## Checks

Not run yet.

## Merge notes

Not ready: nothing built yet.
