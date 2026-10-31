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

## Odin items (2 minutes)

1. `co_loot rare CapeOdin` (or `spawn CapeOdin`, `HelmetOdin`, `TankardOdin`): the tooltip no longer says "DLC item", and you can equip them; the character picture shows no "DLC required".
2. Their recipes show at the workbench once you've picked up the materials, and crafting works.
3. The log lists `Unlocked CapeOdin (was tagged DLC "beta")` and the other two.

## Session 3b: player-stat affixes (loot step 2, about 10 minutes)

Build from **`claude/affixes-loot`**. Roll items until you have the affix (`co_loot rare ArmorLeatherChest`, `co_loot rare CapeDeerHide`, `co_loot rare SwordBronze`…); these lines are no longer grey.

1. **Max health / stamina / eitr:** equip an item with "+N max health" (or stamina, eitr): the health/stamina bar and the character sheet go up by N; unequip: back down.
2. **Carry weight:** "+N carry weight": the weight limit (inventory and character sheet) rises by N.
3. **Movement speed:** "+N% movement speed" (legs, cape, trinket) or Fleet "while held" (weapon or shield in hand): "Speed from gear" on the character sheet rises by N%; you run faster. Fleet stops counting when you sheathe.
4. **Regen:** "+N% health regen" / "+N% stamina regen": regen is visibly faster (compare with the item off).
5. **Resistances:** "N% less fire damage taken" (or frost, lightning, poison, all resistances): stand in a fire (`spawn Surtling` or a campfire) with and without the item: less damage with it.
6. **Weapon skill:** "+N to the weapon's skill" on a weapon: with it in hand, the skill in the Skills tab shows +N; sheathed: back to normal.
7. **Life steal:** "N% of damage dealt returned as health": take some damage, hit creatures: small green heals.
8. No red errors in the console.

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

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. Your friend sees and hears your corrupted wishbone's pings. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
5. **Character sheet:** with your friend standing next to you, open your inventory: only you appear in the character picture, never them.
