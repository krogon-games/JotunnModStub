# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** switch your copy of the code to the branch the checks below are for (now `claude/rips-bosses`), rebuild both mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1c: solo (about 20 minutes)

### 1. Rip waves (step B)
0. Check the build: `BepInEx/LogOutput.log` has `Corruption 0.1.0 loaded (build: claude/rips-bosses …)`. A build from `master` doesn't have step B yet.
1. ~~Waves start with the army message; a 2-star dropped a shard; the heart leaves a chest with shards~~ (passed 2026-10-29).
1a. ~~Maddened bonuses in the log, red creatures; charms ignored; chests in Meadows and Swamp~~ (passed 2026-10-29).
1b. ~~Busier waves (`WaveMultiplier` 2); maddened creatures fight each other; heart search with the corrupted wishbone~~ (passed 2026-10-29).
1c. ~~Wishbone pings mostly red; stop when the heart dies~~ (passed 2026-10-29).
1e. ~~Every wave creature at least 1 star: more challenging~~ (passed 2026-10-29).
1d. ~~Map icons~~ (passed 2026-10-29).
2. Wear a Boar or Neck charm: rip creatures still attack you.
3. Watch whether maddened creatures also fight each other.
4. Destroy the heart: the exit rip opens and a chest appears with Meadows chest loot plus 1–3 corruption shards; the waves stop.
5. Try one later biome, e.g. `spawn CorruptionMap_Swamp` (draugr and skeletons) or `CorruptionMap_Plains` (fulings).
6. No red errors in the console.

### 1f. Corrupted offerings
Passed 2026-10-30: Eikthyr, Moder (and a mix refused), the Queen, Kall's phases.
1. **Icons re-check:** the Corrupted ancient seed is the red three-pointed seed, the Corrupted malicious blood the red blob (they were swapped).

### 2. Bulk planting from chests
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Session 3: loot system step 1 (about 15 minutes)

Build from branch **`claude/affixes-loot`** (the log line says `build: claude/affixes-loot …`). The log should also say `Loot: 84 affixes, 248 base items.`

1. ~~Rares from `co_loot`: names, tooltip lines with ranges, grey inactive lines; armour and damage numbers rise~~ (passed 2026-10-29, screenshots; Magic items and lightning damage too). Still to check below: hover names, durability, block, repair, upgrade, drops, combat.
1b. ~~Build check, natural drops, combat affixes, a resonant line, no errors~~ (passed 2026-10-29). ~~Re-test step 2 (ground names) and step 6 (upgrade)~~ (passed 2026-10-30). ~~Step 5 (durability and repair)~~ passed 2026-10-30. Only step 10 (bigger backpacks, after the server setting) is left.
1c. **New (2026-10-30): the actual bonus** next to item-stat lines, e.g. `+43% of physical damage as spirit (+23) [10–50]`. It matches the vanilla number above (Spirit 23), and goes up when you upgrade the item; the bracket stays the same.
1a. **Spawn loot:** `co_loot magic` and `co_loot rare SwordBronze`. Each drops an item in front of you; the console names it in blue (Magic) or yellow (Rare).
2. **Names:** hover over the item on the ground: its name is coloured (Magic: e.g. "Cruel Bronze Sword of Embers"; Rare: two words, e.g. "Grim Fang"). Pick it up: the inventory tooltip title has the same coloured name.
3. **Tooltip:** at the bottom, "Magic" or "Rare Bronze Sword", then one line per affix with its range, e.g. `+24% damage (+14) [5–40]`. Lines the game doesn't apply yet are grey with "(not active yet)". Look out for a `◆ resonant` line (both halves of a twin).
4. **Stats change:** compare with a plain crafted copy of the same item: an item with enhanced damage or elemental damage shows higher damage numbers (or a fire/frost/… line); enhanced armour, durability and block show in armour, durability and block power.
5. **Durability:** a durability affix raises the max; the item starts full. Repair it at the workbench: it goes back to the raised max.
6. **Upgrade:** upgrade a dropped item at its station: it keeps its name and affixes.
7. **Natural drops:** kill some starred creatures (`spawn Boar 10 2` spawns ten 1-star boars). Drops are rare (1 in 50 Magic, 1 in 100 Rare; stars raise it), so this mostly checks nothing breaks. Items come from the biome you're in, and 1 in 5 from the biome before it (Meadows: always Meadows items).
8. **Combat:** hit something with a knockback or backstab item and parry with a parry item: no errors (the effect is subtle).
9. No red errors in the console or `LogOutput.log`.
10. **Bigger backpacks** (after setting the sizes in `PLAYER_SETUP.md` → For the server admin): the satchel opens with 2 rows (6 slots); items already in a backpack are kept; a 5-row backpack (`spawn BackpackPlains`) fits on screen.
## Orlog levels and XP (branch `claude/class-skills`, about 15 minutes)

New plugin: build `Orlog` too (`CraftingOverhaul.sln` builds all three) and check `BepInEx/LogOutput.log` has `Orlog 0.1.0 loaded (build: claude/class-skills …)`.
1–6, 9. ~~`orlog_info`, XP per kill, 1-star ×2, level-up, low creatures give little, kept after relogging, no errors~~ (passed 2026-10-30).
7. ~~Pace~~: too fast at first; fixed and re-tested 2026-10-30 ("pace feels better"). `XpMultiplier` in `com.krogongames.orlog.cfg` tunes it later (reloads on save).
8. With a friend (Session 2): both within 70 m of a kill each get XP; the lower-level player gets the bigger share of a low creature.
9. No red errors in the console.

## Classes UI (branch `claude/ui-overhaul`, about 15 minutes)

Build `CraftingOverhaul` from **`claude/ui-overhaul`**. In `BepInEx/config/com.krogongames.craftingoverhaul.cfg` set `[5 - Classes UI]` → `Demo = true` (sample Amazon at level 12; works with or without Orlog installed). The log says `Skill cross added.` once you're in the world.

1. **Bars:** health (red, top), eitr (blue, middle, only once you have eitr: eat Yggdrasil porridge or another eitr food), stamina (yellow, bottom) together at the bottom centre with "current / max" at their ends. The game's own round health display, stamina bar, eitr bar and food icons are gone. Take a hit: a pale trail shows what you lost and catches up. Sprint: stamina drains.
2. **Food:** eat 1–3 foods: they appear in slots left of the bars with minutes left; the bars get longer as your maximum rises. A food about to run out pulses.
3. **Building and sailing:** equip the hammer, then sit at a ship's helm: the bars move up out of the way, as the game's did. Ride a lox if you can: its stamina still shows.
4. **XP bar:** a thin purple bar under the bars with "Level 12" on the left and a pulsing "5 stat, 1 skill point (K)" on the right.
5. **Skill cross:** bottom left, four slots in a cross: top Multiple Shot (gold outline: on the skill button), left Inner Sight with a dark cooldown sweep every 8 seconds, right empty, bottom Fire Arrow greyed with "Weapon". Keys 1–4 in the corners; "Skill button: Right mouse" under it. Check it doesn't overlap EquipmentAndQuickSlots' quick slots or the Forsaken power icon.
6. **Skills panel:** press **K** (and also try the **Skills** button above the top right of the inventory). The Amazon's three tabs; the Bow and Crossbow tab shows 10 skills in rows Lv 1–30 with lines between linked skills (gold from skills with points). Hover a skill: tooltip with points, required level, eitr cost, now and next level. Skills marked "(later)" are grey.
7. **Spend:** click Magic Arrow (green +): its points go up and "Skill points: 0". Click another: "No skill points" shows at the bottom. Click a skill from a higher row (Lv 18 or more): "Needs level 18" (or its level).
8. **Attributes:** on the left, Strength, Dexterity, Vitality, Energy with values and what they give. Press + on Dexterity: the value goes up and the stat points go down (the XP bar hint follows); at 0 points the + buttons disappear.
9. **Slots:** drag Cold Arrow (it has a point) onto the panel's empty third slot: it appears there and on the HUD cross. Drag a skill straight onto the HUD cross too. Drag a slot onto another to move it; drag one off the slots (or right-click it) to empty it. Try dragging Critical Strike (a passive, in the middle tab): "Passive skills don't take a slot". A skill with no points: "Learn it first".
10. **Close:** K, Escape and the X close the panel; Escape doesn't open the game menu as well. While it's open you can't move or swing the camera.
11. **Class choice:** open the console (F5) and type `co_classes_stone`: the cross disappears and a "Choose your class" window lists the Amazon with its icon and description. Confirm: "You are now an Amazon", the window closes and the cross comes back. K before confirming says "Choose a class at the Orlog stone first".
12. **Without the demo:** set `Demo = false` (the config reloads on save): the cross, XP bar and panel go; the bars stay. Set `[4 - HUD]` → `Bars = false`: the game's own displays come back.
13. No red errors in the console or `LogOutput.log`.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. Your friend sees and hears your corrupted wishbone's pings. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
5. **Character sheet:** with your friend standing next to you, open your inventory: only you appear in the character picture, never them.
