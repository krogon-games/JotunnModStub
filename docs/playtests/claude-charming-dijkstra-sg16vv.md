# Play-tests: `claude/charming-dijkstra-sg16vv`

## Round: orb drops, Hoardfinder's and of Hidden Riches as orb finders, Splintering's reach (2026-11-19)

**Build:** the three DLLs from this branch (only Corruption changed). The log line should read `Corruption 0.8.0 loaded (build: claude/charming-dijkstra-sg16vv <commit>)` (versions are raised when the branch merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`, `god`. Best in the Black Forest (trees, copper, greydwarfs).

**What's new (your decisions 3–10):**
- **Orbs drop.** Every kill rolls for orbs, once for each player nearby (bosses too: a boss is one kill). Every orb but the Jeweller's can drop, Exalted and Mirror included. On average that's one orb every 21 kills, mostly Transmutation and Augmentation: about 6 an hour at a normal pace. Stars, rips and corrupted bosses raise the chance.
- **Chests.** A vanilla dungeon or location chest rolls once when the game first creates it, at ×1.5. **Chests the world made before this build already have their loot, so they get no orbs.** The chest a rip heart leaves rolls once for each player near the rip.
- **Hoardfinder's** (pickaxe, was Stonecutter's): "X% chance a rock or ore chunk you break drops an orb". **of Hidden Riches** (axe, was Resinous / "of Resin"): "X% chance a tree, log or stump you fell drops an orb". X rolls 1–5% (your decision 11). When it triggers, one orb always drops: Transmutation 43% of the time, Augmentation 28%, Alchemy and Annulment 12% each, Regal 2.6%, Exalted and Chance about 1% each, Divine 0.2%, the Mirror about 1 in 1,000. No more extra stone or resin per hit. A test tool rolled before this build keeps its old 5–25% number until a Divine Orb rerolls it.
- **Splintering** reaches 4 m from the spot you hit (it was the axe's own reach, about 2 m).
- **New test command** `co_orbroll [rolls] [source …]`: rolls the orb table many times and prints the counts next to what's expected. It drops nothing.

**A number to look at while you play:** at the best roll (5%), a copper deposit (about 30 chunks) gives about 1.5 orbs and every 5 trees about one; at 1%, one orb per 3 deposits. If that feels too small or too big, tell me.

**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: orb drops" (step ids `orb-1` … `orb-12`, then `loop-1` … `loop-7`).

### Checklist

**Rates**

1. [ ] (`orb-1`) Start the game and open `BepInEx/LogOutput.log` → it has `Loot: … affixes, … base items, 9 orbs that drop.` No red errors at startup.
2. [ ] (`orb-2`) F5, `co_orbroll 100000` → ten lines, one per orb. Each count is close to its "expected" number: Transmutation about 2,000, Augmentation about 1,300, Chance about 50, Divine about 10, Mirror about 5 (0–15 is normal for the last two). The Jeweller's Orb says "never drops".
3. [ ] (`orb-3`) `co_orbroll 100000 star_2 rip_tier_1` → the first line says "orb multiplier x1.88", and every count is about 1.9 times step 2's.

**Kills**

4. [ ] (`orb-4`) `spawn Greyling 20`, kill them all, twice (40 kills) → about two orbs lie among the drops, most likely Transmutation or Augmentation. None at all happens about 1 time in 7: then kill 20 more.
5. [ ] (`orb-5`) Pick the orbs up → each goes to its own fixed cell in **Currencies** and stacks with orbs you already have. At a Riftforge, use one in the Forge tab on a Normal item → it works like an orb from `co_craft give`.

**Chests**

6. [ ] (`orb-6`) `spawn TreasureChest_meadows 30` (thirty new chests) and open them all → their normal loot, and about 2 of them also hold an orb (none or 5 happens too). The log has one `Orb drops: N orb(s) in TreasureChest_meadows (…)` line for each chest that got orbs.
7. [ ] (`orb-7`) Build a plain wooden chest → it's empty: chests you build never get orbs.
8. [ ] (`orb-8`) Rip chest: use `CorruptionMap_Meadows`, clear the rip and destroy the heart → the chest has its shards as before, and the log has `Rip chest: 1 orb roll(s), N orb(s) added`. N is usually 0 (an orb about 1 time in 17); the line is what counts.

**Gathering**

9. [ ] (`orb-9`) **Hoardfinder's** (was Stonecutter's): hold a pickaxe, `co_affix hoardfinders 100` (100% only for the test; real tools roll 1–5%) → the tooltip reads "100% chance a rock or ore chunk you break drops an orb". Break a whole copper deposit → **every** chunk that breaks drops an orb, mostly Transmutation and Augmentation. No extra stone drops while you mine. Then `co_affix hoardfinders 5` and break another deposit → one or two orbs. The pickaxe's name starts with "Hoardfinder's" (a Magic one: "Hoardfinder's Antler Pickaxe"). A pickaxe that rolled Stonecutter's before this build shows the new name and text too.
10. [ ] (`orb-10`) **of Hidden Riches** (was Resinous, "of Resin"): hold an axe, `co_affix hidden_riches 100` → the tooltip reads "100% chance a tree, log or stump you fell drops an orb". Fell a tree and chop up its logs and stump → **each** piece drops an orb along with its wood. No extra resin drops per hit. A Magic axe's name ends in "of Hidden Riches".

**Splintering**

11. [ ] (`orb-11`) Splintering: hold an axe, `co_affix splintering 100`. Chop a tree that has another tree about 3–4 m away → the other tree shakes and takes the same chop. A tree more than about 4 m away is never hit. Then send me `BepInEx/LogOutput.log`, even if everything passed.

12. [ ] (`orb-12`) **Old saves:** if a world or character has a pickaxe or axe with the old Stonecutter's or Resinous, or a splinter of either, load it → the tool's name and line show Hoardfinder's / of Hidden Riches, and each splinter is still there and shows the new name. (Skip if you have none.)

**Loop fixes (added 2026-11-23, your answers to the gameplay-loop review)**

13. [ ] (`loop-1`) **Riftforge recipe:** hammer → Crafting → Riftforge: it costs **20 fine wood, 10 bronze nails, 5 surtling cores, 10 amber**, nothing else (no shard, no Eikthyr trophy).
14. [ ] (`loop-2`) **Map names:** `spawn CorruptionMap_Meadows` → its name is **"Meadows map"** (not "Corrupted Meadows map"). The Riftforge's recipe list shows "Meadows map" too.
15. [ ] (`loop-3`) **Boss shards:** `spawn Eikthyr` and kill him alone → besides his trophy and essence, his drops include at least 1 shard, every kill. In a world where he was never killed before, the first kill gives 2–4 (the old 1–3 plus your new 1).
16. [ ] (`loop-4`) **Corrupted boss orb:** summon a corrupted Eikthyr (a Corrupted deer trophy at his altar, or `co_corrupt` near him) and kill him → **one orb always drops** (mostly Transmutation or Augmentation), plus 2–3 essences and 4–6 shards.
17. [ ] (`loop-5`) **Rip chest:** use a Meadows map, clear the rip, destroy the heart, open the chest → it holds a **Meadows map**, plus shards and vanilla loot as before; now and then also a Magic or Rare item. The log has `Rip chest: added a CorruptionMap_Meadows` and `Rip chest: 1 gear roll(s), N item(s)`.
18. [ ] (`loop-6`) **Map drops:** in the Meadows, after step 17 and with Eikthyr beaten, type `co_mapdrops` → "A rip heart destroyed in this world: yes. This biome's boss beaten: yes." and about 120 maps from 100,000 rolls (1 in 830). Then fly to the Black Forest and type it again: with the Elder never killed in this world it says "This biome's boss beaten: no … So creatures here drop no maps yet."
19. [ ] (`loop-7`) **Dungeon chest shards:** with step 6's thirty `TreasureChest_meadows`, about 1 or 2 also hold a **shard** (none or 4 happens too); the log has a `Chest shards: 1 shard in …` line for each.

### Your feedback

(not yet)
