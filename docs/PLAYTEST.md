# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes. Last brought in line: 2026-11-29 (coordinator, after PR #106: the build checks expect Orlog 0.11.0 and CraftingOverhaul 0.7.0 (Corruption 0.14.0) and a release build made after PR #106; 2j has a new step 10, `mpe-2j-10`, the ghost javelins with two players; the javelin round's final check `fin-1` passed and came off, so no "This round" section is on the page); before that the same day (`claude/optimistic-ramanujan-acdyf8`: the re-test passed (8 of 8) and came off; the page's first section is its final check, "This round: final check, Gungnir's Throw's icon (#41)" (`fin-1`)); before that 2026-11-28 (`claude/optimistic-ramanujan-acdyf8`: round 1 passed 12 of 18 and came off; its re-test is the page's first section, "This round: re-test, the ghost javelin's look (#41)" (`r2-1` … `r2-7`)); before that the same day (`claude/optimistic-ramanujan-acdyf8`: its round, Gungnir's Throw, Lightning Bolt and Plague Javelin (#41), is the page's first section, "This round: Gungnir's Throw and the javelin skills (#41)" (18 steps, `gt-1` … `gt-8`, `lb-1` … `lb-4`, `pj-1` … `pj-4`, `c6-1`, `c6-2`)); before that the same day (coordinator, after PR #102: the build checks expect Corruption 0.14.0 and a release build made after PR #102; 2m has a new step 10, `mpe-2m-10`, Rimeburst, Guarded and corrupted targeting with two players); before that the same day (`claude/wizardly-wozniak-4aix76`: its final steps passed and the round's section came off the page); before that the same day (`claude/wizardly-wozniak-4aix76`: Rimeburst and Guarded passed (`cg-1` … `cg-18`) and came off; the page's first section is now its final play-test, "This round: final play-test, corrupted creatures fight players only (#97)" (`cg-19` … `cg-23`)); before that the same day (`claude/wizardly-wozniak-4aix76`: its round, Rimeburst and Guarded (#97), is the page's first section, "This round: Rimeburst and Guarded, more corrupted-only map affixes (#97)" (19 steps, `cg-1` … `cg-19`; `cg-19` only if the game freezes); before that the same day (coordinator, after PR #100: the build checks expect Corruption 0.13.0 (Orlog 0.10.0, CraftingOverhaul 0.6.0) and a release build made after PR #100; 2m has a new step 9, `mpe-2m-9`, the loot affixes and Thorns with two players; `claude/wizardly-wozniak-4aix76`'s section is on the page); before that the same day (`claude/nifty-hypatia-afm8ez`: its round, the loot map affixes (#96), was the page's first section, "This round: loot map affixes (#96) and Thorns (#97)" (20 steps, `la-1` … `la-20`); all 20 passed and came off, then the re-test `la-18b` passed and came off too, so nothing of that round is on the page); before that the same day (coordinator, after PR #95: the build checks expect Corruption 0.12.0 and Orlog 0.10.0 (CraftingOverhaul 0.6.0) and a release build made after PR #95; `mpe-2m-4` uses the new affixes; the map affix overhaul's solo steps were taken off after its play-test passed); before that 2026-11-27 (coordinator, after PR #91: the build checks expect Corruption 0.11.0 (Orlog 0.9.0, CraftingOverhaul 0.6.0) and a release build made after PR #91; the rip steps `srv-8`, `duo-7` and `mpe-rip` follow the 96 m rule and 1–4 shards; the rips round's solo steps were taken off after its play-test passed); before that 2026-11-26 (coordinator, after PR #88: the build checks expect Corruption 0.10.0 (Orlog 0.9.0, CraftingOverhaul 0.6.0) and a release build made after PR #88; the rip steps (`srv-8`, `duo-7`, `mpe-2k-3`, `mpe-rip`) expect the heart's spilled loot instead of a chest, and the boss steps (`srv-9`, `mpe-2k-4`, `mpe-boss`, `mpe-corrupt`) each player's own shard and orb; 2l is titled with PR #88; `duo-4b` no longer waits for the merge; the three steps passed that morning came off: `g3c-11` and `g3c-12` (Session 3c is done, so the section is now "Sessions 3d–3e") and `stat-skill-56` (Session 3b is gone from the page)); before that the same day (coordinator, after PR #85: the build checks expect Corruption 0.9.0, Orlog 0.9.0, CraftingOverhaul 0.6.0, a release build made after PR #85 and Valheim 1.0.30 on every PC and the server; the PlantEasily steps wait for its 1.0.30 update; the rip and boss steps expect the new loot (a map in the rip chest, a shard per player from every boss, an orb per player from a corrupted boss); 2k is titled with PR #85); before that 2026-11-23 (`claude/charming-dijkstra-sg16vv`: the loop fixes add `loop-1` … `loop-7` to its round's section; 2k's text covers the rip chest map and gear and boss shards per player); before that 2026-11-19 (`claude/charming-dijkstra-sg16vv`: its round, orb drops, Stonecutter's and Resinous as orb finders and Splintering's reach, is the page's first section, "This round: orb drops" (12 steps, `orb-1` … `orb-12`); Session 2 has a new 2k group, `mpe-2k-1` … `mpe-2k-3`; Session 3d's old Stonecutter's/Resinous step `h3d-6` is off the page, replaced by `orb-9` and `orb-10`); before that 2026-11-18 (coordinator: Session 2-short, one friend for 20–30 minutes, is a new section of the page, ids `duo-1` … `duo-8`); before that the same day (coordinator, after PR #78: the build checks expect Corruption 0.8.0 and Orlog 0.8.0 and a release build made after PR #78; step `mpe-2j-9` names PR #78); before that the same day (`claude/amazon-snipe-shackle-shot-m67buh`: its round, Snipe and Shackle Shot and the area hits' on-hit affixes (22 steps, `sn-1` … `sn-10`, `sh-1` … `sh-10`, `dot-1`, `dot-2`), was the page's first section and passed the same day (all 22 Pass, no notes); it is off the page again, and 2j's new step 9, both skills with two players, is on the page as `mpe-2j-9`); before that 2026-11-17 (coordinator, after PR #76: the build checks expect Orlog 0.7.0 and a release build made after PR #76; step `mpe-2j-8` names PR #76 and the crossbow); before that the same day (`claude/elegant-maxwell-e7ys9o`: its round, Strafe as rapid fire (15 steps, `rf-1` … `rf-15`), passed and is off the page again; `mpe-2j-8`'s Strafe part follows the remake); before that the same day (coordinator, after PR #74: the build checks expect Orlog 0.6.0 and a release build made after PR #74; step `mpe-2j-8` names PR #74); before that the same day (`claude/magical-babbage-slwwpn`: both of its rounds passed, 26 steps all Pass, and are off the page again; 2j's new step 8, Strafe, Immolation Arrow and Venom Strike with two players, is on the page as `mpe-2j-8`); before that 2026-11-16 (`claude/magical-babbage-slwwpn`, #70: the second round's 14 steps `r70-1` … `r70-14` (Strafe, Immolation Arrow, Venom Strike) are the page's second section, "This round: Strafe, Immolation Arrow and Venom Strike"); before that 2026-11-16 (`claude/magical-babbage-slwwpn`, #72: the skill damage round's 12 steps, `r72-1` … `r72-12`, are the page's first section, "This round: skill damage scales off the weapon"; they include Forked Lightning's two steps left over from PR #71, as `r72-6` and `r72-7`); before that the same day (coordinator, after PR #71: the build checks expect Orlog 0.5.0; 2j's step 7, Forked Lightning and Wind Lance with two players, is on the page as `mpe-2j-7`; the page's intro no longer points at a "This round" section); before that the same day (`claude/jolly-albattani-4fruv2`: evening: Wind Lance's 15 round-2 steps passed and are off; Forked Lightning's `r16-7` and `r16-8` are off too, left for its balancing session (Krogon); the last step, `r16c-1` (the lance through a bush), passed and is off: nothing of this round is left on the page); before that 2026-11-15, evening (coordinator, after PR #68: the build checks expect Orlog 0.4.0 and CraftingOverhaul 0.5.0; Session 3c's eight steps passed on 2026-11-11 came off the page); before that the same day (`claude/laughing-mccarthy-fnz0to`: its round "the skill tree overhaul and skill cards", ids `cards-1` to `cards-15`, was added and taken off again the same day after all 15 steps passed; so was its 3-step re-check (the Multishot name, attribute icons; ids `cards2-1` to `cards2-3`), passed the same evening; 2j's freeze step follows the removed skills); before that 2026-11-11 (the build check's version numbers, Session 2's 2g–2i, the two small checks of Session 1c); before that 2026-11-08. The rounds since 2026-11-05 keep their own solo checklist in `docs/playtests/<branch>.md`, and **every round's solo checklist also goes on the phone page while its round is open** (Krogon, 2026-11-11), so the page is always the place to tick; when the round's play-test has passed, its steps come off the page again.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** since 2026-11-01 every branch is merged, so build from `master` (a session's new work: from its branch, it will say so); rebuild all three mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots and AdventureBackpacks installed (the group's setup; **PlantEasily stays off** until its author updates it for Valheim 1.0.30, which broke 2.3.1), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1c: solo (about 5 minutes; only bulk planting left)

### 1. Rip waves (step B)
1. ~~Waves start with the army message; a 2-star dropped a shard; the heart leaves a chest with shards~~ (passed 2026-10-29).
1a. ~~Maddened bonuses in the log, red creatures; charms ignored; chests in Meadows and Swamp~~ (passed 2026-10-29).
1b. ~~Busier waves (`WaveMultiplier` 2); maddened creatures fight each other; heart search with the corrupted wishbone~~ (passed 2026-10-29).
1c. ~~Wishbone pings mostly red; stop when the heart dies~~ (passed 2026-10-29).
1e. ~~Every wave creature at least 1 star: more challenging~~ (passed 2026-10-29).
1d. ~~Map icons~~ (passed 2026-10-29).
2–6. ~~Charms don't stop rip creatures; maddened creatures fight each other; the heart leaves the exit rip and a chest with shards; a later biome; no errors~~ (passed 2026-10-29, phone log).

### 1f. Corrupted offerings
~~Eikthyr, Moder (and a mix refused), the Queen, Kall's phases, the fixed seed and blood icons~~ (passed 2026-10-30).

### 1g. Corrupted boss summons
Passed 2026-10-31: summons arrive, 1–2 stars, limit 4 alone, stop when the boss dies. ~~Re-check after the fix: the summons and Eikthyr leave each other alone~~ (passed the same day, phone log).

### 2. Bulk planting from chests
**Waits for PlantEasily's update for Valheim 1.0.30** (2.3.1 is broken on it; [#86](https://github.com/krogon-games/JotunnModStub/issues/86)).
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

### 3. Small checks ([#27](https://github.com/krogon-games/JotunnModStub/issues/27))
**Both passed 2026-11-15** (phone log).
1. **One click sound:** the + buttons for attribute points, the skills panel's buttons and the inventory's tab buttons play one click, not two (on 2026-10-31 you heard double clicks, most on the first press; fixed the same day, never re-tried).
2. **Build stamp:** `BepInEx/LogOutput.log` has the line `CraftingOverhaul 0.5.0 loaded (build: master <commit>)`, beside Corruption's and Orlog's.

## Odin items (2 minutes)

~~Equip and log check~~ passed 2026-10-31 (the log also shows the recipes unlocked). ~~Crafting one~~ passed 2026-11-15 (phone log). Nothing left.

1. `co_loot rare CapeOdin` (or `spawn CapeOdin`, `HelmetOdin`, `TankardOdin`): the tooltip no longer says "DLC item", and you can equip them; the character picture shows no "DLC required".
2. Their recipes show at the workbench once you've picked up the materials, and crafting works.
3. The log lists `Unlocked CapeOdin (was tagged DLC "beta")` and the other two.

## Session 3d: on-harvest gathering affixes (loot step 3, second half, about 20 minutes)

Step 1 partly passed 2026-11-08 (PR #55: logs and copper deposit chunks drop twice). Left: step 1 on standing trees, stumps and one-piece rocks; steps 2–10.

Build from **`master`** (merged; the loot branch is deleted). Roll tools until they carry the affix (`co_loot rare PickaxeBronze`, `co_loot rare AxeBronze`…), or add it to the tool in your hand with `co_affix <id> 100` for a sure proc (ids in `Corruption/Data/affixes.csv`); its tooltip line is no longer grey. Rolled chances are low (5–25%), so give each one a dozen trees or rock chunks. Only your own swings count: something that breaks by rolling or falling gives normal drops. With a second player, repeat 1, 3 and 9 while the **other** player's game runs the tree or rock (they stand nearest, or they look at it and type `co_own`).

1. **Bountiful** (*of Bounty*): now and then a tree, log, stump, rock or deposit chunk drops everything twice. ~~Logs, deposit chunks~~ passed 2026-11-08.
2. **Bountiful** (*of Plenty*, axe): now and then a log drops its wood twice; with both Bountifuls, sometimes three times.
3. **Determined:** the stamina bar jumps up each time something breaks and drops.
4. **Artisan:** now and then a copper or tin chunk drops one more ore.
5. **Gemseeker's:** now and then a rock or chunk drops Amber or Amber pearl (Meadows), also Ruby (Black Forest and later).
6. **Stonecutter's:** changed 2026-11-08 to roll on every hit, then 2026-11-19 into an orb finder named Hoardfinder's: tested in `docs/playtests/claude-charming-dijkstra-sg16vv.md` instead.
7. **Resinous:** changed 2026-11-08 to roll on every hit, then 2026-11-19 into an orb finder named "of Hidden Riches": tested in `docs/playtests/claude-charming-dijkstra-sg16vv.md` instead. **Forester's:** now and then a standing tree drops its seed (beech seeds, pine cone, birch seeds, fir cone, acorn).
8. **Charring:** now and then wood drops as coal instead.
9. **Odin's Grace** on an axe: now and then one chop at a standing tree drops all its wood, resin and seeds at once, with no log or stump left. Fell a tree normally, then a lucky chop at its log or stump harvests the log, its pieces and the stump together. A tree you can't chop yet (tool tier) still says "too hard".
10. The log lists `Harvest affixes: wood …; seeds …; ores …` at startup: wood should be the wood types, seeds the seeds, ores the ores and scrap. No red errors in the console.
## Session 3e: proc and utility affixes (loot step 4, about 25 minutes)

Step 7 (Skadi) passed 2026-11-08. Left: 1–6, 8–13.

Build from **`master`** (merged; the loot branch is deleted). Give yourself the affix you want with `co_affix <id>` (best value; `co_affix deathless 10 chest` puts Deathless on your chest piece): ids `shieldwall`, `rallying`, `skalds`, `steadfast`, `reprisal` (on a shield or a weapon that blocks), `wrath` (a weapon with a combo, e.g. a sword), `skadi`, `fenrir`, `bloodforged` (weapons), `deathless`, `foraging` (armour: `helmet`, `chest`, `legs`, `cape`). Parry = raise the block just before the hit lands (the game's perfect-block effect plays). Check the startup log for the line "Foraging: N wild plants" and "Proc affixes: Reprisal shows …" (after the first Reprisal or Wrath).

1. **Shieldwall:** parry: a buff icon appears for 10 s and the armour number in the inventory goes up by the %.
2. **Rallying:** parry, then hit a creature: the first hit's damage number is clearly bigger; the icon goes after that hit (or after 10 s).
3. **Skald's:** parry, then attack: the swing is visibly faster for 10 s.
4. **Steadfast:** let the stamina bar drop, parry: it jumps up by the value.
5. **Reprisal** (`co_affix reprisal 100` for a sure proc): parry: lightning strikes the attacker and it takes lightning damage.
6. **Wrath:** with a sword, swing the full combo into a group of greylings: the last hit sends a shockwave that damages and pushes back everyone within about 3 m.
7. ~~**Skadi:** keep hitting one creature: stacks build over 5 hits; switch target or wait 3 s and it starts again~~ passed 2026-11-08 on a bow (PR #53's play-test, with `devcommands` on: the top-left "Skadi: stack N" message; the game shows no separate frost number).
8. **Fenrir:** kill three weak creatures quickly: the icon shows x2, x3 and the swing gets faster; it ends 10 s after the last kill.
9. **Bloodforged:** get hurt, kill a creature: health jumps up by the % of max health.
10. **Deathless:** `co_affix deathless 10 chest`, then die (fall from a cliff, or let a troll hit you at low health): you stay at 1 health with "Deathless! You cheat death." and a "Deathless spent" icon counting down 10 minutes. Die again during it: you really die. Relog during the cooldown: the icon comes back with the time left. With EquipmentAndQuickSlots' keep-on-death on, a real death still keeps your gear as before.
11. **Foraging:** pick a mushroom, berries or thistle: an icon for 8 s and you run faster; picking flint, stones or branches, or your own crops, doesn't trigger it (tell Claude if it does). A PlantEasily bulk harvest of wild plants gives one 8 s boost, not a stack.
12. **With a friend** (Session 2): your parry buffs (steps 1–4) also reach a friend standing within 10 m, shown on their screen; kills you make on creatures your friend's game controls still trigger Fenrir and Bloodforged.
13. No red errors in the console.

## Session 3c: gathering-tool affixes (loot step 3, about 20 minutes)

Steps 2–4 passed 2026-11-08 (PR #55, including Veinbreaker's reset when the tool is unequipped). Steps 1, the rest of 4, and 5–10 **passed 2026-11-11** (phone log; written down here on 2026-11-15). Your note on 7, Splintering: "Yes, works, but i think i probably needs a little bigger radius/ hit detection range" (carried into the orb drops round, [#33](https://github.com/krogon-games/JotunnModStub/issues/33)). **Left: 11–14.**

Build from **`master`** (merged; the loot branch is deleted). Roll tools with the affix you want (`co_loot rare PickaxeAntler`, `co_loot rare AxeFlint`, `co_loot magic PickaxeBronze`…), or `co_affix <id> 100` on the tool in your hand. These lines are no longer grey; the on-harvest ones (Bountiful, Artisan, Gemseeker's, Stonecutter's, Resinous, Forester's, Charring, Determined) are tested in Session 3d.

1. ~~**Prospector's** on a pickaxe: rocks and copper/tin take more hits off per swing than with a plain pickaxe of the same kind (watch the damage numbers).~~ Passed 2026-11-11.
2. ~~**Veinbreaker's:** every 5th swing at rock or ore shows a bigger damage number.~~ Passed 2026-11-08.
3. ~~**Seismic:** now and then a swing at a copper or tin deposit knocks off the chunks next to the one you hit too.~~ Passed 2026-11-08.
4. **Odin's Grace:** ~~now and then a swing breaks the chunk you hit and every chunk touching it at once; iron (tier) still needs the right pickaxe~~ passed 2026-11-08 (deposits, tin, silver too hard for a lower pickaxe). ~~A single one-piece rock breaks in one hit.~~ Passed 2026-11-11.
5. ~~**Excavator's:** digging the ground with the pickaxe makes a deeper, wider hole than a plain one.~~ Passed 2026-11-11.
6. ~~**Lumberjack's / Timberfall / Rootbreaker** on an axe: standing trees / fallen logs / stumps take more damage per chop.~~ Passed 2026-11-11.
7. ~~**Splintering:** now and then a chop also damages the nearest other tree or log right next to you.~~ Passed 2026-11-11 (its reach may need to be bigger: note above). Reach raised to 4 m on 2026-11-19: re-check in `docs/playtests/claude-charming-dijkstra-sg16vv.md`.
8. ~~**Fit:** now and then a swing costs no stamina (the bar doesn't drop). **Careful:** now and then a hit costs no durability.~~ Passed 2026-11-11.
9. ~~**Hushed:** creatures notice your chopping later (hard to see; just check nothing breaks).~~ Passed 2026-11-11.
10. ~~**Light:** the tool's weight in the tooltip is lower than a plain one's.~~ Passed 2026-11-11.
11. ~~**Keen-eyed / Magnetic:** with the tool in hand, items on the ground fly to you from further away (Magnetic: 3–6 m; vanilla 2 m).~~ Passed 2026-11-26 (phone page `g3c-11`).
12. ~~**Swift:** the swing animation is visibly faster.~~ Passed 2026-11-26 (phone page `g3c-12`).
13. ~~**Scholar's:** Woodcutting or Pickaxes skill rises faster (Skills tab).~~ Passed 2026-11-26 (phone page `g3c-12`).
14. No red errors in the console.

## Session 3b: player-stat affixes (loot step 2, about 10 minutes)

~~Max health/stamina/eitr, carry weight, movement, regen, resistances, no errors~~ passed 2026-10-31. Step 7 (life steal) passed on a bow 2026-11-07 and with a melee weapon 2026-11-15 (phone log). Step 6 (weapon skill: failed 2026-10-31, then a check for bug #56, skipped 2026-11-15) **passed 2026-11-26** (phone page `stat-skill-56`): the +6 is only the Skills tab's number. Nothing left in Session 3b.

Build from `master` (merged 2026-10-31). Roll items until you have the affix (`co_loot rare ArmorLeatherChest`, `co_loot rare CapeDeerHide`, `co_loot rare SwordBronze`…); these lines are no longer grey.

1. **Max health / stamina / eitr:** equip an item with "+N max health" (or stamina, eitr): the health/stamina bar and the character sheet go up by N; unequip: back down.
2. **Carry weight:** "+N carry weight": the weight limit (inventory and character sheet) rises by N.
3. **Movement speed:** "+N% movement speed" (legs, cape, trinket) or Fleet "while held" (weapon or shield in hand): "Speed from gear" on the character sheet rises by N%; you run faster. Fleet stops counting when you sheathe.
4. **Regen:** "+N% health regen" / "+N% stamina regen": regen is visibly faster (compare with the item off).
5. **Resistances:** "N% less fire damage taken" (or frost, lightning, poison, all resistances): stand in a fire (`spawn Surtling` or a campfire) with and without the item: less damage with it.
6. ~~**Weapon skill:**~~ (passed 2026-11-26) "+N to the weapon's skill" on a weapon: with it in hand, the skill in the Skills tab shows +N; sheathed: back to normal. **Check for [#56](https://github.com/krogon-games/JotunnModStub/issues/56)** (2026-10-31 the panel showed +6 for a +7 axe; suspected a display quirk after a death): equip an axe, type `co_affix weapon_skill_prefix 7`, then `raiseskill Axes 0.6` (the console prints the exact level, e.g. `Skill Axes = 25.6`): the Skills tab should show **+6**. Then `raiseskill Axes 0.4` (back to a whole level): it should show **+7**. If both show +7, tell Claude (the suspected cause is wrong). Send the console lines either way.
7. **Life steal:** "N% of damage dealt returned as health": take some damage, hit creatures: small green heals.
8. No red errors in the console.

## Session 3: loot system step 1 (about 15 minutes)

Build from `master` (merged 2026-10-31; the log line names the branch and commit). The log should also say `Loot: 84 affixes, 249 base items.` (248 before the 1.0.31 game dump of 2026-11-27)

1. ~~Rares from `co_loot`: names, tooltip lines with ranges, grey inactive lines; armour and damage numbers rise~~ (passed 2026-10-29, screenshots; Magic items and lightning damage too). Still to check below: hover names, durability, block, repair, upgrade, drops, combat.
1b. ~~Build check, natural drops, combat affixes, a resonant line, no errors~~ (passed 2026-10-29). ~~Re-test step 2 (ground names) and step 6 (upgrade)~~ (passed 2026-10-30). ~~Step 5 (durability and repair)~~ passed 2026-10-30. Only step 10 (bigger backpacks, after the server setting) is left.
1c. ~~The actual bonus~~ passed 2026-10-30. **New (2026-10-30): the actual bonus** next to item-stat lines, e.g. `+43% of physical damage as spirit (+23) [10–50]`. It matches the vanilla number above (Spirit 23), and goes up when you upgrade the item; the bracket stays the same.
1a. **Spawn loot:** `co_loot magic` and `co_loot rare SwordBronze`. Each drops an item in front of you; the console names it in blue (Magic) or yellow (Rare).
2. **Names:** hover over the item on the ground: its name is coloured (Magic: e.g. "Cruel Bronze Sword of Embers"; Rare: two words, e.g. "Grim Fang"). Pick it up: the inventory tooltip title has the same coloured name.
3. **Tooltip:** at the bottom, "Magic" or "Rare Bronze Sword", then one line per affix with its range, e.g. `+24% damage (+14) [5–40]`. Lines the game doesn't apply yet are grey with "(not active yet)". Look out for a `◆ resonant` line (both halves of a twin).
4. **Stats change:** compare with a plain crafted copy of the same item: an item with enhanced damage or elemental damage shows higher damage numbers (or a fire/frost/… line); enhanced armour, durability and block show in armour, durability and block power.
5. **Durability:** a durability affix raises the max; the item starts full. Repair it at the workbench: it goes back to the raised max.
6. **Upgrade:** upgrade a dropped item at its station: it keeps its name and affixes.
7. **Natural drops:** kill some starred creatures (`spawn Boar 10 2` spawns ten 1-star boars). Drops are rare (1 in 50 Magic, 1 in 100 Rare; stars raise it), so this mostly checks nothing breaks. Items come from the biome you're in, and 1 in 5 from the biome before it (Meadows: always Meadows items).
8. **Combat:** hit something with a knockback or backstab item and parry with a parry item: no errors (the effect is subtle).
9. No red errors in the console or `LogOutput.log`.
10. **Bigger backpacks** (after setting the sizes in `PLAYER_SETUP.md` → For the server admin): the satchel opens with 2 rows (6 slots); items already in a backpack are kept; a 5-row backpack (`spawn BackpackPlains`) fits on screen. **Passed 2026-11-15** (phone log).
## Orlog levels and XP (merged into `master`, about 15 minutes)

New plugin: build `Orlog` too (`CraftingOverhaul.sln` builds all three) and check `BepInEx/LogOutput.log` has `Orlog 0.1.0 loaded (build: claude/class-skills …)`.
1–6, 9. ~~`orlog_info`, XP per kill, 1-star ×2, level-up, low creatures give little, kept after relogging, no errors~~ (passed 2026-10-30).
7. ~~Pace~~: too fast at first; fixed and re-tested 2026-10-30 ("pace feels better"). `XpMultiplier` in `com.krogongames.orlog.cfg` tunes it later (reloads on save).
8. With a friend (Session 2): both within 70 m of a kill each get XP; the lower-level player gets the bigger share of a low creature.
9. No red errors in the console.

## Orlog attributes and class eitr

All passed 2026-10-31 (single player). Left: nothing (sharing XP is in Session 2).

## Orlog stone and class choice

All passed 2026-11-02 (single player), size included. A custom design comes later.

## Orlog skill tree and skill points

All passed 2026-11-02 (single player).

## Classes UI (merged into `master`, 2 minutes)

All passed 2026-10-31 ("everything looks really good now"); ~~the adrenaline bar~~ passed the same day (phone log).

## Session 2-solo: alone on the dedicated server (before the group evening)

**Krogon alone, joined to the group's dedicated server** (added 2026-11-11). It needs the release build ([#25](https://github.com/krogon-games/JotunnModStub/issues/25)), made from `master` after PR #106 (2026-11-29), on the server and on your PC, nobody else. **The server and your PC must both be on Valheim 1.0.30** (the patch of 2026-11-25 changes crossplay compatibility) and on AdventureBackpacks 2.4.8, its 1.0.30 update; leave PlantEasily off. Every step is taken from Session 2 below or from a handoff's "untested on a server" list; the origin is in brackets.

**What it can find:** everything so far was played in single player, where the "server" is your own game. On the dedicated server the mods load on a machine with no player and no screen, and the rip and Orlog stone requests really travel to the server and back ([`ARCHITECTURE.md`](ARCHITECTURE.md) → Server messages, Dedicated-server review). **What it can't find:** anything that needs a second player's game (who runs a creature or chest, what others see, loot and XP per player). Those stay in Session 2.

1. **The mods load on the server** (Part 1): start the server. Its own log has the three lines `Corruption 0.14.0 loaded (build: master <commit>)`, `Orlog 0.11.0 loaded …` and `CraftingOverhaul 0.7.0 loaded …`, and no red errors from our mods.
2. **Joining** (Part 1): **Start modded** in r2modman and join. Expect: no "incompatible version" message; your own `BepInEx/LogOutput.log` has the same three lines with the same commit; F5 shows no red errors.
3. **`devcommands`** (Session 2's open question): type `devcommands`, then `spawn Boar`, `orlog_info` and `orlog_level 10`. Do they work on the server? If not, note what the console says: the evening's script then needs real items brought in from play.
4. **The server's settings win** (Part 5, step 1): set `[2 - Nearby Containers] Enabled = false` in the server's `com.krogongames.craftingoverhaul.cfg` and restart it; try crafting from a chest. Expect: you can't. Set it back.
5. **The Orlog stone through the server** (classes handoff, untested on a server): in the server's world the stone stands behind the Eikthyr trophy stone, once; E opens the class choice; the class is kept after relogging.
6. **Tabs after relogging** (2b step 3, 2h step 1): the six tabs (Bag · Currencies · Splinters · Maps · Runes · Essences) hold what they held; pick up a shard, relog: still in Currencies.
7. **XP and loot from kills** (2c step 3, 2d step 2): kill starred creatures (`spawn Boar 10 2`). Expect: "+N XP" on each kill; Magic and Rare drops about as often as in single player, never the same drop twice.
8. **A rip run** (Part 3, steps 1, 2, 4, 5, 6): use `CorruptionMap_Meadows` near the base wearing the Corrupted wishbone. Expect: a red rip 4 m in front of you, on land at least 96 m from anything a player built, beds, wards and portals included (since PR #91; if it says no spot was found, note the message); you land with no heart in sight; pings lead to it; within 96 m the army message shows and red waves come; destroying the heart gives the exit rip and spills its loot on the ground, no chest (since PR #88): the Meadows chest's vanilla loot, 1–4 shards (a white map's heart, since PR #91) and one Meadows map, now and then orbs and a Magic or Rare item, each with its loot beam; the exit brings you back to the first rip.
9. **A corrupted boss** (Part 4): offer a Corrupted deer trophy at Eikthyr's altar. Expect: he arrives red; every 30 s 2–3 red creatures of his army, at most 4 alive; they go for you only; the kill gives 2–3 essences, 3–5 shards, 1 more shard and 1 orb for you (one each per player near him, since PR #85; with their loot beams since PR #88) and the summons stop.
10. **Logs:** copy your `BepInEx/LogOutput.log` and the server's log right away (the next start overwrites them) and send both.

## Session 2-short: one friend, 20–30 minutes

**Krogon (player A) and one friend (player B) on the dedicated server** (added 2026-11-18, at Krogon's request: a whole evening is hard to organise). It picks the Session 2 steps where a bug would hurt most: items lost or doubled, loot and XP split wrong, creatures or skills acting differently when the **other** player's game runs them, and one rip run together. Everything else stays in the full Session 2 below; the origin of each step is in brackets, so a pass here counts for those steps there.

**Before B joins (Krogon, about 5 minutes; do Session 2-solo first, so you know the server works and whether `devcommands` does):** both install the same release build ([#25](https://github.com/krogon-games/JotunnModStub/issues/25); [`PLAYER_SETUP.md`](PLAYER_SETUP.md)), both on Valheim 1.0.30. At the base: a workbench with one chest beside it holding **exactly 1 Wood and 1 Resin** (one torch's worth, `data/game-dump-1.0.31/recipes.csv`) and nothing else needed nearby. Bring a Greydwarf charm, a Corrupted wishbone and a Meadows map (`spawn CorruptionTalisman_Greydwarf`, `spawn CorruptedWishbone`, `spawn CorruptionMap_Meadows`). A is an Amazon (`orlog_class Amazon`, `orlog_level 40`, then `orlog_skill FreezingArrow 1`, `orlog_skill ShackleShot 1`, `orlog_skill ImmolationArrow 1`) with the three on the skill slots, a bow and arrows. Type `god` while fighting if you like.

1. **Build check** (Part 1; both, 2 min): B starts with r2modman's **Start modded** and joins → no "incompatible version" message. Both open `BepInEx/LogOutput.log` → the same three lines from the same commit: `Corruption 0.14.0`, `Orlog 0.11.0`, `CraftingOverhaul 0.7.0`. F5: no red errors.
2. **One chest, two crafters** (2a steps 4 and 7, 2e E; 4 min): **A** opens the chest and keeps it open; **B** tries to craft a torch at the workbench → B can't while A has it open. A closes it. Count down from three and **both** craft a torch at the same moment → only one of you gets it; the other sees "A chest you need is in use by someone else…" or simply can't; the chest is empty, nothing doubled. Put 1 Wood and 1 Resin back and try twice more.
3. **Shards and your own screen** (2b; 2 min): **A** drops a shard; **B** picks it up → it goes to B's Currencies tab, A's has one fewer. Standing side by side, each opens the inventory → only yourself in the character picture; take a hit → only your own health bar moves.
4. **Loot and XP for both** (2c step 3, 2d, 2e F; 4 min): B types `orlog_info` and notes the level; A is level 40. Together, kill `spawn Boar 10 2` twice → both get "+N XP" on each kill, B the bigger share; about twice the Magic and Rare drops one player gets alone; no kill drops twice. Tell Claude roughly how many items dropped. **Personal loot (PR #88; 2l steps 1–2):** then, within a minute of the last kill, both type `co_personal list` → your own drops "show", the other's "HIDE"; **B** stands on one of A's drops → nothing happens for B, and A can still take it. After the minute, both see the leftovers.
5. **Talisman** (2c step 1, 2g E3; 3 min): **A** wears the Greydwarf charm. `spawn Greydwarf 4`; **B** stands nearest them (B's game then runs them) → they ignore A and attack B. A hits one → only that one fights A.
6. **A's skills on B's creatures** (2j steps 2, 4, 8, 9; 4 min): with new greydwarfs that B's game runs (B nearest): **A** shoots Freezing Arrow → they take frost damage and freeze. A shoots Shackle Shot at the front one of two in a line → both stand still and don't attack for about 2 s (B sees them stop, without the rope and swirls). A shoots Immolation Arrow at the group → the blast burns them and those on the burning ground keep taking fire. B sees all the damage numbers; B standing in A's fire takes no damage.
7. **A rip run together** (Part 3, steps 1–6; 8 min): **A** wears the Corrupted wishbone and uses the map near the base → a red rip 4 m in front. Both go through → you land at the same spot. B sees and hears A's red pings. Walk to the heart together → the army message and waves come; both see them red. Destroy the heart → the exit rip, and the loot spills around the heart, no chest (since PR #88): 1–4 shards (a white map's heart, since PR #91) and one Meadows map dealt between you (one map, not one each), plus each player's own orbs and gear, each visible only to its owner for a minute; both go home through the exit and end at the first rip. If either gets stuck after a portal, note it: Valheim 1.0.30 fixed the game's own stuck-in-portal bug, so it's more likely ours now. Did anything differ for B?
8. **Logs** (Part 5 step 3; 1 min): both copy `BepInEx/LogOutput.log` right away (the next start overwrites it) and send them to Claude, with what you saw.

**If there's time left:** a corrupted boss together (Part 4, steps 2–4), about 10 minutes.

**Left for the full Session 2:** wards and private chests, a far workbench, PlantEasily from chests, Skadi switching bows, the Odin's Grace and log ownership checks (2f), a talisman after its owner logs out (2g E1–E2), two offerings at one altar, the Riftforge and splinters with two players (2h, 2i), dying with orbs, Penetrate, the avoids, Inner Sight, Forked Lightning, Wind Lance, Strafe, Venom Strike, Snipe, boss points, players 70 m apart, and the rip-wave check that needs three players.

## Session 2: with a friend

**One evening for the four of us on the dedicated server** (about 2½ hours). Nothing here has been tried with more than one player yet. Do the parts in order; each step says **who** does it, **what to do** and **what should happen**. Note ✅/❌ and what you saw for each step. Krogon = the admin (server files, `devcommands`); player A and player B = any two of you.

**Question for the user:** does `devcommands` (and so `spawn`, `co_corrupt`, `orlog_level`) work for you on the dedicated server? If not, Part 0 needs the real items (shards, an essence, a map, trophies) brought in from play.

### Part 0: before the evening (Krogon)

1. Build all three DLLs from `master` and send the same `Corruption.dll`, `Orlog.dll` and `CraftingOverhaul.dll` to everyone. Install them on the server too ([`PLAYER_SETUP.md`](PLAYER_SETUP.md) → For the server admin). Everyone and the server on **Valheim 1.0.30** and AdventureBackpacks 2.4.8; PlantEasily off until it's updated for 1.0.30.
2. Gameplay settings come from the server: leave its config files at the defaults unless a step below says otherwise.
3. For the quick checks, put on the server or bring: 3 chests and a workbench at the base (Part 2a), 3 Greydwarf trophies, 1 Chain and 10 Leather scraps (or `spawn CorruptionTalisman_Greydwarf`), a few shards, a Corrupted wishbone and a `CorruptionMap_Meadows` (Part 3), and 2 Deer trophies (Part 4).

### Part 1: setup and build check (everyone, 10 minutes)

1. **Each player:** start with r2modman's **Start modded** and join the server. Expect: no "incompatible version" message.
2. **Each player and Krogon (also the server's own log):** open `BepInEx/LogOutput.log` and find the three lines. Expect, the **same** on every machine:
   - `Corruption 0.14.0 loaded (build: master <commit>)`
   - `Orlog 0.11.0 loaded (build: master <commit>)`, the same commit
   - `CraftingOverhaul 0.7.0 loaded (build: master <commit>)`, the same commit
3. **Each player:** F5, no red errors.

### Part 2: quick checks together at the base (about 45 minutes)

**2a. Craft and build from chests** (the full table with notes is [`multiplayer-test.md`](multiplayer-test.md); these are its steps 1–9). Setup: a workbench and 3 chests within 20 m, with wood, stone and leather scraps only in the chests.

1. **Player B:** craft something at the workbench from chest materials only. Expect: crafts; the materials leave the chests. **Player A:** open the chests. Expect: nothing missing or duplicated.
2. **Player A:** the same the other way round. Expect: the same.
3. **Player B:** build a wood piece with the hammer, wood only in a chest. Expect: builds; A sees the wood gone from the chest.
4. **Player A:** open and keep open the only chest with wood. **Player B:** try a recipe that needs that wood. Expect: can't craft while A has it open; can once A closes it.
5. **Player A:** put wood in a personal chest (`piece_chest_private`), empty the others. **Player B:** try to craft. Expect: B can't use A's private chest; A can.
6. **Player A:** place and activate a ward by the bench without adding B. **Player B:** craft from chests inside it. Expect: B can't; after A adds B to the ward, B can.
7. **Players A and B:** count down and craft from the same chest at the same moment, a few times. Expect: nothing lost or doubled. With exactly enough for one craft, only one of you gets it and the other sees "A chest you need is in use by someone else…" (fix of 2026-11-07; full steps in `docs/playtests/claude-admiring-babbage-rk1w8d.md` → E).
8. **Player B:** go 30+ m away to a second workbench and craft. Expect: the far chests aren't used.
9. **One player:** bulk-plant a PlantEasily grid with the seeds only in a nearby chest. Expect: it plants and takes the seeds from the chest (not yet tried even alone). **Skip it until PlantEasily is updated for Valheim 1.0.30.**

**2b. Things only you should see** (each player)

1. **Character sheet:** stand next to each other and open your inventory. Expect: only you in the character picture, never the others.
2. **HUD bars and skills panel:** each player sees their own health, stamina and eitr (take a hit: only your bars move). `K` opens your own panel. These settings are per player: changing your `4 - HUD` settings changes only your screen.
3. **Currency and Maps tabs:** **Player A** drops a shard on the ground; **player B** picks it up. Expect: it goes into B's Currency tab; A's tab has one fewer; nobody else's tabs change. Each player's tabs are still there after relogging.

**2c. Things everyone should agree on**

1. **Talismans:** **player A** wears a Greydwarf charm in a Talisman slot. Go to greydwarfs, with **player B** standing nearest them (B's game then runs them). Expect: they ignore A and attack B. A hits one: only that one fights A, for about 30 s.
2. **Loot looks the same to everyone:** **one player** `co_loot rare SwordBronze`; another picks it up. Expect: the same coloured name and affix lines in both players' tooltips.
3. **Each player's own loot roll:** all together, kill a pack of starred creatures (`spawn Boar 10 2`, several times). Each kill rolls loot once for every player within 100 m, so four players get about four times the Magic and Rare drops one player would. Drops aren't locked to anyone yet: anyone can pick them up. Expect: no errors; tell Claude roughly how many items dropped.

**2d. Levels and XP**

1. **Each player:** `orlog_info`, note your level. Make the levels different (e.g. **one player** `orlog_level 10`).
2. **All within 70 m:** kill some creatures. Expect: everyone within 70 m of the kill gets "+N XP", whoever made the kill; the lower-level player gets the bigger share of a low creature. Also try a kill where the creature was run by another player's game (it was fighting someone else): XP still arrives.
3. **One player** stays more than 70 m away. Expect: no XP for them.
4. Other players' levels don't show anywhere on screen yet; they're only used for the split above.

**2e. Review fixes (2026-11-07, multiplayer part skipped for now by Krogon, 2026-11-08).** Full steps in [`playtests/claude-admiring-babbage-rk1w8d.md`](playtests/claude-admiring-babbage-rk1w8d.md):
1. **E:** two players, one chest: only one craft when there's exactly enough; "in use" while someone holds a chest for building or has it open; still works when the chest's owner lacks CraftingOverhaul (2a step 7 is E1).
2. **F:** loot per player: about twice the Magic/Rare drops with two players near; no kill drops twice.
3. **G:** arrow affixes on the server: Skadi (with `devcommands`, the top-left "Skadi: stack N" message) and life steal follow the bow when switching mid-flight.
4. **H (Part 3, 3 players):** rip waves after the heart changes hands: send Claude the `Rip waves: …` log lines from the game that took over, and your creature counts.

**2f. Gathering review fixes (2026-11-08; solo part passed, multiplayer left for later by Krogon).** Full steps in [`playtests/claude-festive-albattani-lg4p35.md`](playtests/claude-festive-albattani-lg4p35.md) → D. `co_own` (look at an object) makes your game run it.
1. **D1:** B runs a copper deposit (`co_own`); A's Seismic and Odin's Grace break the same chunks as alone.
2. **D2:** B runs a log A hit once; it breaks by falling or rolling → normal drops, B's log says "without a player's swing".
3. **D3:** A runs the log, B the stump of one felled tree; both swing Odin's Grace axes at the same moment → one tree's worth of wood or two? Send both players' `Odin's Grace: whole-tree harvest …` lines.

**2g. Review fixes: talisman fight-back and corrupted summons (2026-11-08; solo part passed, multiplayer left for later by Krogon).** Full steps in [`playtests/claude-hopeful-thompson-eyncm8.md`](playtests/claude-hopeful-thompson-eyncm8.md) → E.
1. **E1–E2:** B reaches a greydwarf group first (B's game runs them); A, wearing a Greydwarf charm, hits one; B logs out within 10 s → it keeps attacking A for the rest of the 30 s; A's next hit restarts it (the `Talisman: … (run by this game)` line moves to A's log).
2. **E3:** both wear the charm; A hits one → it attacks A only; the rest ignore both.
3. **E4–E6:** A offers a corrupted deer trophy at Eikthyr; during the 12 s B offers a corrupted trophy, normal trophies and wood → one red Eikthyr, only A's corrupted trophy used. A normal summon with B's corrupted trophy during the delay → normal Eikthyr, B keeps it.

**2h. Six inventory tabs and the Riftforge's Forge tab (2026-11-10; solo play-test passed, multiplayer left for later by Krogon).** Full solo steps in [`playtests/claude-lucid-carson-hs5g89.md`](playtests/claude-lucid-carson-hs5g89.md).
1. **Each player:** open the inventory → six tabs (Bag · Currencies · Splinters · Maps · Runes · Essences); your own essences are in Essences, the counts the same as before the update. Nobody sees anyone else's tabs.
2. **A and B at one Riftforge:** both open it and the Forge tab at the same time; each puts their own gear in the forge and uses an orb → each sees only their own gear and result; the orbs come from each player's own Currencies.
3. **A** forges while **B** uses the Craft tab → B's recipe list is normal; A's Forge view stays as it is.
4. **B** dies with orbs and essences; **B** takes all from the gravestone → back in B's Currencies and Essences; nothing ends up with A.

**2i. Riftforge shards, shattering and Affix Splinters (2026-11-11; solo play-test passed, multiplayer not tried).** Full solo steps in [`playtests/claude-brave-einstein-xz357h.md`](playtests/claude-brave-einstein-xz357h.md).
1. **A** shatters an item and drops a splinter; **B** picks it up → it goes to B's Splinters tab with the same affix and number, and its own shape and colour.
2. **A** gives **B** a corrupted item (with a red corrupted line) and a twin-completed item → B sees the same lines, numbers and the Corrupted mark.
3. **A** puts splinters in a shared chest; **B** takes them → unchanged.
4. **A and B at one Riftforge**, each shattering and corrupting their own gear → no mix-ups; each one's "Don't warn me again" choice is their own.

**2j. The Amazon's passives and skills (2026-11-11, merged in PR #64; solo steps in [`playtests/claude-lucid-albattani-imrei7.md`](playtests/claude-lucid-albattani-imrei7.md)).** A is an Amazon (`orlog_class Amazon`); the creature must be run by **B's** game (B reaches it first and stays nearer).
1. **A** finds a creature whose hits show grey numbers (resisted), types `orlog_passive penetrate 100` and hits it → white, bigger numbers, as alone.
2. **A** types `orlog_passive crit 100` and hits a creature B's game runs → about twice the damage; A sees the critical-hit effect.
3. **A** types `orlog_passive dodge 100`, `orlog_passive avoid 100`, `orlog_passive evade 100` and fights a troll (or a greydwarf group) B's game runs → punches and thrown rocks are dodged, avoided or evaded; the troll's **ground slam still hurts**.
4. **A** shoots Freezing Arrow at a group of creatures B's game runs → they take frost damage and freeze (can't move) for the skill's seconds. (Ice Arrow and Exploding Arrow were removed by the skill tree overhaul, 2026-11-15, `claude/laughing-mccarthy-fnz0to`.)
5. **A** casts Inner Sight near creatures B's game runs → A sees them glow; **B's** hits on them also do more damage (5% more at skill level 1, 24% at level 20; your decision of 2026-11-15).
6. **A and B** kill a boss together (both within 70 m) → both get "First victory over this foe" and the skill points (a classless B gets them once a class is chosen).
7. **(2026-11-16, `claude/jolly-albattani-4fruv2`; solo steps in [`playtests/claude-jolly-albattani-4fruv2.md`](playtests/claude-jolly-albattani-4fruv2.md))** **A** shoots Forked Lightning at one of a group of creatures B's game runs → the struck creature and those next to it take lightning damage; B sees the bursts (the lines of light show only for A). **A** fires a charged Wind Lance (hold 1.5 s past full draw) into a group of creatures B's game runs → each one the gust touches is hit, reels and is pushed back; B sees a fast arrow without the wind.
8. **(2026-11-17, `claude/magical-babbage-slwwpn`; solo steps in [`playtests/claude-magical-babbage-slwwpn.md`](playtests/claude-magical-babbage-slwwpn.md))** **A** holds Strafe at a group of creatures B's game runs → after the draw, arrows stream into them (Strafe is rapid fire since 2026-11-17, `claude/elegant-maxwell-e7ys9o`, PR #76; with a crossbow too: bolts at once); B sees the arrows fly and the damage. **A** shoots Immolation Arrow at the group → the blast burns them and those that stay on the burning ground keep taking fire for 4 seconds. **A** shoots Venom Strike → those in the cloud are poisoned. B sees the damage and the game's own impact effects, but not the ring and glow on the ground (they show only for A). B's own character standing in A's fire or cloud takes no damage.
9. **(2026-11-18, `claude/amazon-snipe-shackle-shot-m67buh`, merged in PR #78; solo steps in [`playtests/claude-amazon-snipe-shackle-shot-m67buh.md`](playtests/claude-amazon-snipe-shackle-shot-m67buh.md))** **A** shoots Shackle Shot at the front one of two greydwarfs in a line that B's game runs → both stand still and don't attack for about 2 seconds (B sees them stop, but not the rope or the swirls, which show on A's game only); with nothing behind, a blink. **A** fires a charged Snipe at a creature B's game runs → B sees the arrow and the bigger damage; A's zoom is A's alone. **A** holds a Venom Strike cloud on creatures B's game runs, with a life-steal bow → A's health doesn't jump while they're poisoned.
10. **(2026-11-29, `claude/optimistic-ramanujan-acdyf8`, merged in PR #106; solo steps in [`playtests/claude-optimistic-ramanujan-acdyf8.md`](playtests/claude-optimistic-ramanujan-acdyf8.md))** **A** (a spear in hand, Gungnir's Throw, Lightning Bolt and Plague Javelin on slots) throws at a group of creatures B's game runs → **B also sees the gold ghost spear** fly, stick and fade (a networked copy), and no real spear lands on the ground for anyone; Lightning Bolt's ghost crackles for B too and its lightning damage shows; Plague Javelin's poison numbers show for B, but its green puffs and ring only on A's screen. B standing in A's cloud takes no damage.

**2k. Currency orb drops (2026-11-19, `claude/charming-dijkstra-sg16vv`, merged in PR #85; solo steps in [`playtests/claude-charming-dijkstra-sg16vv.md`](playtests/claude-charming-dijkstra-sg16vv.md)).**
1. **A and B** kill about 40 creatures together (`spawn Greyling 20`, twice) → about twice as many orbs as alone (each player rolls for themselves); an orb **B** picks up goes to B's Currencies only.
2. **B** looks at a copper deposit and types `co_own`; **A** mines it with `co_affix hoardfinders 100` on the pickaxe → orbs still drop now and then (B's game spawns them).
3. In the rip run (Part 3), the log of the game that ran the heart has `Rip loot: N player(s) near the heart.` with N = the number of players near it, one `Rip loot: player …` line for each (their orb and gear rolls), and one map of the rip's biome among the spilled drops (one, not one each). (Was the rip chest's `Rip chest: N orb roll(s)` until PR #88; see 2l step 4.)
4. **(2026-11-23)** A boss killed together drops 1 shard for each player near it (four players: 4, plus the first-kill 1–3 the first time); a corrupted boss drops one orb for each player. Since PR #88 each player's shard and orb is theirs alone for a minute: see 2l step 8.

**2l. Personal loot and loot beams (2026-11-26, `claude/personal-loot-beams-q5glnk`, merged in PR #88; solo steps in [`playtests/claude-personal-loot-beams-q5glnk.md`](playtests/claude-personal-loot-beams-q5glnk.md)).** A drop rolled for one player is invisible to everyone else for its first 60 s; then everyone sees it and anyone can take it. **Everyone and the server must be on the same build** (Part 1's build check, same commit everywhere): Corruption 0.11.0 since the rips round (0.10.0 when this was written), so a game on an older build can't join. `co_personal list` shows every personal drop nearby, whose it is and whether your game hides it.
1. **A and B**, a few metres apart, kill `spawn Greyling 20` together → each sees beams only on their own drops; each types `co_personal list` → the other's drops say "HIDES it". After a minute the leftovers appear for both.
2. **B** stands on one of A's drops during its minute (A says where; `co_personal list` gives the distance) → B's auto-pickup doesn't take it, E does nothing, and B's crosshair passes through it to whatever is behind. **A** then walks over and takes it.
3. **The other game runs the creatures:** B stands nearest a new group (B's game runs them); **A** kills them → A's drops are visible to A and hidden from B; A picks them up normally.
4. **A rip run** (Part 3): destroy the heart → no chest; the loot spills around the heart; each sees only their own share for a minute (the shards are split between you, the map goes to one of you, each has their own orb and gear rolls). The heart owner's log has `Rip loot: 2 player(s) near the heart.` and one `Rip loot: player …` line for each of you.
5. **Relog:** A leaves one of A's drops on the ground and logs out → B can't see it until its minute is up, then B can take it. A logs back in → if B took it, it's gone for A too (no copy).
6. **Race after the lock:** a drop whose minute is up; A and B stand on it together with auto-pickup → only one of you gets it, no copy.
7. **Backpack auto-store:** B wears a backpack with auto-store on that holds an orb; A's locked orbs of that kind lying at B's feet stay where they are for their minute.
8. **A boss together:** each player's boss shard is their own (short red beam, hidden from the other for a minute); the world's first-kill bonus shards are shared and visible to both.
9. **Mining:** **B** looks at a copper deposit and types `co_own`; **A** mines it with `co_affix hoardfinders 100` → the orbs are A's (B's game spawns them, hidden from B for a minute).
10. **Sound:** only the owner hears a tall-beam drop's sound (the other player hears nothing).
11. **Handover in the middle of a lock** (added after the multiplayer review): A stands nearest a new group (A's game runs them) and kills them; A at once runs more than 200 m away; B then picks up B's own drops → they come to B normally and don't drift towards anyone else. Then swap roles.
12. **Near the world spawn** (the server may run things there itself): kill a few creatures together close to the spawn stones → each gets their own drops, and B can pick up B's.
13. **B relogs in the middle of A's lock:** after B is back, A's drop is still hidden for B until its minute is up, then appears.
14. **Who counts at the heart:** in the rip, B destroys the heart while A stands 50–90 m away → the `Rip loot: N player(s) near the heart.` line (in the log of whichever game ran the heart; check both) says 2.

**2m. Map tiers and the explored-land fallback (rips round, `claude/festive-gates-79y1yr`, #31 and #84; merged in PR #91 (Corruption 0.11.0), solo steps in [`playtests/claude-festive-gates-79y1yr.md`](playtests/claude-festive-gates-79y1yr.md); the map affix overhaul, `claude/kind-einstein-8sn8e7`, #90, merged in PR #95 (Corruption 0.12.0, Orlog 0.10.0), solo steps in [`playtests/claude-kind-einstein-8sn8e7.md`](playtests/claude-kind-einstein-8sn8e7.md)).** A tiered map keeps its tier and affixes when it opens a rip, and every rip keeps 96 m from anything a player placed. **Everyone and the server must be on this build** (Part 1's build check: the log says `Corruption 0.14.0 loaded (build: master …)` and `Orlog 0.10.0` on every game, a build made after PR #100). All of it needs devcommands (admin): `co_map <biome> [tier] [affix ...]` makes a map, `co_rip` shows the nearest rip object (biome, tier, affixes, time left, heart, owner).
1. **Tier for everyone:** **A** types `co_map BlackForest 6` and uses it at home → the server log says `Rip request: … away from player-placed objects (96 m) … in N ms` (note N); A's log says `Rip opened: BlackForest, Tier 6 · …`; both hover the entry and see the same "Tier 6" line.
2. **First owner:** **B** goes through first to the heart; **A** follows a minute later → B's log says `this game now runs the waves … waves x9, 100% guaranteed 2-stars`; both hover the heart and type `co_rip` → the same tier and affixes, "owns it: yes" only for B; every creature is a 2-star.
3. **Handover:** with 50 or more creatures up, **B** runs about 300 m away with creatures chasing (or logs out) → **A**'s log says `this game now runs the waves` with the same x9 and 100%; then A's `… alive (counted here) X of 54` lines against what is really alive. Note any stutter.
4. **Logging in inside the rip:** the rip is opened with explicit new affixes (`co_map BlackForest 6 brutal ironskin searing fleet`, *changed 2026-11-27: Hardened, Rabid and Smouldering were removed, #90; the new affixes passed alone 2026-11-28, PR #95*); **B** logs back in next to the heart → the creatures are red and fast (Fleet), B's sword hits do almost nothing (Ironskin), B burns when hit (Searing).
5. **Heart destroyed by the non-opener:** **B** destroys the heart while B owns it → B's log says `Rip loot: tier 6`, one line per player with gear "rare"; 6 to 14 shards in all, dealt among you; A sees "Tier 6" on the exit.
6. **Fallback near buildings:** **A** builds a workbench in explored Meadows, types `co_rip fallback on`, then `co_map Meadows` and uses it → the server log says `(explored-land fallback)`; the heart and the landing spot are both more than 96 m from any building of either player.
7. **Leftovers:** after the rip closes (one in-game day; sleeping counts), go back → the red creatures have left (they despawn once nobody fights them).
8. **Trading:** a tiered map handed to **B** keeps its tier and affixes (tooltip and the number on the icon).
9. **Loot affixes and Thorns together** (PR #100, Corruption 0.13.0, *added 2026-11-28*): **A** opens `co_map BlackForest 6 bountiful glittering bulwarked mailed thorns:25` with **B** nearest the heart, so B's game runs the waves. **A** turns `god` off and hits creatures B's game runs → A's console prints `Thorns: your hit took N health, M comes back (25%, before your armour).` and A's health drops a little (a training dummy works too: **B** types `spawn TrainingDummy` and `co_stamp thorns:25`, **A** hits it). Destroy the heart → the heart owner's log has `Rip loot: loot affixes, gear and maps x2, orbs x4, shields x2, armour x2 …`; each player sees only their own drops.
10. **Rimeburst, Guarded and corrupted targeting together** (PR #102, Corruption 0.14.0, *added 2026-11-28*): **A** opens `co_map Meadows 6 glorious bountiful glittering bulwarked guarded` with **B** nearest the heart, so B's game runs the waves → a red Eikthyr appears by the heart for both; both see `Guarded: kill the corrupted boss first` on the heart's hover; neither can damage the heart until Eikthyr dies. Red creatures never fight each other or `spawn Boar 3` next to them; **B's** tamed wolf fights them. Then **A** opens `co_map Meadows 6 glorious bountiful glittering bulwarked rimeburst:100`, **B** kills a creature next to **A** → both see the blue ring and the burst, and only the player(s) inside take frost (each game's log: `Rimeburst: caught in a burst …`).

### Part 3: a rip run (about 45 minutes)

On a dedicated server the rip request goes to the server and its answer comes back; alone this was all one game.

1. **One player (the opener):** wear the Corrupted wishbone, use `CorruptionMap_Meadows` near the base. Expect: a red rip opens 4 m in front of them. If it says no spot was found, note the message. If a rip ever opens within 96 m of anything a player built (beds, wards and portals included), tell Claude (the server should prevent that; since PR #91).
2. **Everyone:** go through. Expect: all land at the same spot, no heart in sight, no waves yet. If anyone gets stuck after the portal, note it: Valheim 1.0.30 fixed the game's own stuck-in-portal bug, so it's more likely ours now.
3. **Everyone else:** watch and listen. Expect: you see and hear the opener's red pings (the log says how they reach other players: `shared with other players by …`).
4. **Everyone:** walk towards the heart together. Expect: within 96 m the army message shows and waves come. Every player sees the creatures red, also the ones that were already standing there when the waves started (fixed 2026-11-01); they attack everyone and each other; charms don't help.
4b. **Everyone:** split up, 100 m or more apart around the heart. Expect: the waves don't pile up beyond their limit near any one of you (fixed 2026-11-01: limits now count creatures far from the game that spawned them).
5. **Everyone:** destroy the heart. Expect: the exit rip opens and the loot spills on the ground around the heart, no chest (since PR #88): the Meadows chest's vanilla loot, 1–4 shards and one Meadows map dealt between you, plus each player's own orbs and Magic/Rare gear (2k step 3); each sees only their own share for a minute, then everything is free (2l step 4); the pings stop; the waves stop.
6. **Everyone:** go home through the exit rip. Expect: everyone ends at the first rip.
7. **Ask:** did anything differ between the opener and the others (the heart's place, the spilled loot and who saw what, the portal hover text)?

### Part 4: a boss (about 30 minutes)

1. **Everyone at Eikthyr:** summon and kill him normally. Expect: **1** essence in total (not one each), 1 shard for each player near him (since PR #85; each player's own shard is theirs alone for a minute since PR #88: 2l step 8), plus 1–3 shards if this is the world's first Eikthyr kill; everyone within 70 m gets XP.
2. **One player:** craft a Corrupted deer trophy at the Riftforge (2 Deer trophies + 1 shard, or `spawn CorruptedTrophyDeer`) and offer it at the altar. Expect: Eikthyr arrives red **for everyone**.
3. **Everyone near him:** fight. Expect: every 30 s 2–3 red creatures of his army arrive; at most 4 alive **per player** near him (16 with four of us); they go for players only, never Eikthyr or each other. Summons run by another player's game behave the same.
4. **Kill him.** Expect: 2–3 essences and 3–5 shards in total, plus 1 shard and 1 orb for each player near him (since PR #85; each one is theirs alone for a minute since PR #88: 2l step 8); summons stop coming.

### Part 5: before you log off

1. Optional ([`multiplayer-test.md`](multiplayer-test.md) step 10): **Krogon** sets `[2 - Nearby Containers] Enabled = false` in the server's `com.krogongames.craftingoverhaul.cfg` and restarts it; **a player** tries crafting from a chest. Expect: they can't (the server's setting wins). Set it back.
2. Optional (step 11): **one player** joins without `CraftingOverhaul.dll` while another crafts from chests. Expect: they play normally and see the chest contents update.
3. **Everyone:** no red errors in F5. Copy your `BepInEx/LogOutput.log` right away (the next start overwrites it) and send it to Krogon, with the ✅/❌ notes.
