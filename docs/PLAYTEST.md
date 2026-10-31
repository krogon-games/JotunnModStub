# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** since 2026-11-01 every branch is merged, so build from `master` (a session's new work: from its branch, it will say so); rebuild all three mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

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
~~Eikthyr, Moder (and a mix refused), the Queen, Kall's phases, the fixed seed and blood icons~~ (passed 2026-10-30).

### 1g. Corrupted boss summons
Passed 2026-10-31: summons arrive, 1–2 stars, limit 4 alone, stop when the boss dies.
1. **Re-check after the fix:** `spawn Eikthyr`, `co_corrupt`, then walk away (out of their reach) and watch: the summons and Eikthyr don't attack each other, and the summons ignore other creatures. They still come for you.

### 2. Bulk planting from chests
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Odin items (2 minutes)

~~Equip and log check~~ passed 2026-10-31 (the log also shows the recipes unlocked). Left: craft one.

1. `co_loot rare CapeOdin` (or `spawn CapeOdin`, `HelmetOdin`, `TankardOdin`): the tooltip no longer says "DLC item", and you can equip them; the character picture shows no "DLC required".
2. Their recipes show at the workbench once you've picked up the materials, and crafting works.
3. The log lists `Unlocked CapeOdin (was tagged DLC "beta")` and the other two.

## Session 3e: proc and utility affixes (loot step 4, about 25 minutes)

Build from **`claude/affixes-loot`**. Give yourself the affix you want with `co_affix <id>` (best value; `co_affix deathless 10 chest` puts Deathless on your chest piece): ids `shieldwall`, `rallying`, `skalds`, `steadfast`, `reprisal` (on a shield or a weapon that blocks), `wrath` (a weapon with a combo, e.g. a sword), `skadi`, `fenrir`, `bloodforged` (weapons), `deathless`, `foraging` (armour: `helmet`, `chest`, `legs`, `cape`). Parry = raise the block just before the hit lands (the game's perfect-block effect plays). Check the startup log for the line "Foraging: N wild plants" and "Proc affixes: Reprisal shows …" (after the first Reprisal or Wrath).

1. **Shieldwall:** parry: a buff icon appears for 10 s and the armour number in the inventory goes up by the %.
2. **Rallying:** parry, then hit a creature: the first hit's damage number is clearly bigger; the icon goes after that hit (or after 10 s).
3. **Skald's:** parry, then attack: the swing is visibly faster for 10 s.
4. **Steadfast:** let the stamina bar drop, parry: it jumps up by the value.
5. **Reprisal** (`co_affix reprisal 100` for a sure proc): parry: lightning strikes the attacker and it takes lightning damage.
6. **Wrath:** with a sword, swing the full combo into a group of greylings: the last hit sends a shockwave that damages and pushes back everyone within about 3 m.
7. **Skadi:** keep hitting one creature: the frost damage number grows over 5 hits; switch target or wait 3 s and it starts small again.
8. **Fenrir:** kill three weak creatures quickly: the icon shows x2, x3 and the swing gets faster; it ends 10 s after the last kill.
9. **Bloodforged:** get hurt, kill a creature: health jumps up by the % of max health.
10. **Deathless:** `co_affix deathless 10 chest`, then die (fall from a cliff, or let a troll hit you at low health): you stay at 1 health with "Deathless! You cheat death." and a "Deathless spent" icon counting down 10 minutes. Die again during it: you really die. Relog during the cooldown: the icon comes back with the time left. With EquipmentAndQuickSlots' keep-on-death on, a real death still keeps your gear as before.
11. **Foraging:** pick a mushroom, berries or thistle: an icon for 8 s and you run faster; picking flint, stones or branches, or your own crops, doesn't trigger it (tell Claude if it does). A PlantEasily bulk harvest of wild plants gives one 8 s boost, not a stack.
12. **With a friend** (Session 2): your parry buffs (steps 1–4) also reach a friend standing within 10 m, shown on their screen; kills you make on creatures your friend's game controls still trigger Fenrir and Bloodforged.
13. No red errors in the console.

## Session 3c: gathering-tool affixes (loot step 3, about 20 minutes)

Build from **`claude/affixes-loot`**. Roll tools with the affix you want (`co_loot rare PickaxeAntler`, `co_loot rare AxeFlint`, `co_loot magic PickaxeBronze`…). These lines are no longer grey; the on-harvest ones (Bountiful, Artisan, Gemseeker's, Stonecutter's, Resinous, Forester's, Charring, Determined) still are.

1. **Prospector's** on a pickaxe: rocks and copper/tin take more hits off per swing than with a plain pickaxe of the same kind (watch the damage numbers).
2. **Veinbreaker's:** every 5th swing at rock or ore shows a bigger damage number.
3. **Seismic:** now and then a swing at a copper or tin deposit knocks off the chunks next to the one you hit too.
4. **Odin's Grace:** now and then a swing breaks the chunk you hit and every chunk touching it at once; a single rock breaks in one hit. Iron (tier) still needs the right pickaxe.
5. **Excavator's:** digging the ground with the pickaxe makes a deeper, wider hole than a plain one.
6. **Lumberjack's / Timberfall / Rootbreaker** on an axe: standing trees / fallen logs / stumps take more damage per chop.
7. **Splintering:** now and then a chop also damages the nearest other tree or log right next to you.
8. **Fit:** now and then a swing costs no stamina (the bar doesn't drop). **Careful:** now and then a hit costs no durability.
9. **Hushed:** creatures notice your chopping later (hard to see; just check nothing breaks).
10. **Light:** the tool's weight in the tooltip is lower than a plain one's.
11. **Keen-eyed / Magnetic:** with the tool in hand, items on the ground fly to you from further away (Magnetic: 3–6 m; vanilla 2 m).
12. **Swift:** the swing animation is visibly faster.
13. **Scholar's:** Woodcutting or Pickaxes skill rises faster (Skills tab).
14. No red errors in the console.

## Session 3b: player-stat affixes (loot step 2, about 10 minutes)

~~Max health/stamina/eitr, carry weight, movement, regen, resistances, no errors~~ passed 2026-10-31. Left: steps 6 (weapon skill) and 7 (life steal).

Build from `master` (merged 2026-10-31). Roll items until you have the affix (`co_loot rare ArmorLeatherChest`, `co_loot rare CapeDeerHide`, `co_loot rare SwordBronze`…); these lines are no longer grey.

1. **Max health / stamina / eitr:** equip an item with "+N max health" (or stamina, eitr): the health/stamina bar and the character sheet go up by N; unequip: back down.
2. **Carry weight:** "+N carry weight": the weight limit (inventory and character sheet) rises by N.
3. **Movement speed:** "+N% movement speed" (legs, cape, trinket) or Fleet "while held" (weapon or shield in hand): "Speed from gear" on the character sheet rises by N%; you run faster. Fleet stops counting when you sheathe.
4. **Regen:** "+N% health regen" / "+N% stamina regen": regen is visibly faster (compare with the item off).
5. **Resistances:** "N% less fire damage taken" (or frost, lightning, poison, all resistances): stand in a fire (`spawn Surtling` or a campfire) with and without the item: less damage with it.
6. **Weapon skill:** "+N to the weapon's skill" on a weapon: with it in hand, the skill in the Skills tab shows +N; sheathed: back to normal.
7. **Life steal:** "N% of damage dealt returned as health": take some damage, hit creatures: small green heals.
8. No red errors in the console.

## Session 3: loot system step 1 (about 15 minutes)

Build from `master` (merged 2026-10-31; the log line names the branch and commit). The log should also say `Loot: 84 affixes, 248 base items.`

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
10. **Bigger backpacks** (after setting the sizes in `PLAYER_SETUP.md` → For the server admin): the satchel opens with 2 rows (6 slots); items already in a backpack are kept; a 5-row backpack (`spawn BackpackPlains`) fits on screen.
## Orlog levels and XP (merged into `master`, about 15 minutes)

New plugin: build `Orlog` too (`CraftingOverhaul.sln` builds all three) and check `BepInEx/LogOutput.log` has `Orlog 0.1.0 loaded (build: claude/class-skills …)`.
1–6, 9. ~~`orlog_info`, XP per kill, 1-star ×2, level-up, low creatures give little, kept after relogging, no errors~~ (passed 2026-10-30).
7. ~~Pace~~: too fast at first; fixed and re-tested 2026-10-30 ("pace feels better"). `XpMultiplier` in `com.krogongames.orlog.cfg` tunes it later (reloads on save).
8. With a friend (Session 2): both within 70 m of a kill each get XP; the lower-level player gets the bigger share of a low creature.
9. No red errors in the console.

## Orlog attributes and class eitr

All passed 2026-10-31 (single player). Left: nothing (sharing XP is in Session 2).

## Classes UI (merged into `master`, 2 minutes)

Everything else passed 2026-10-31 ("everything looks really good now"). Left: with `[5 - Classes UI]` → `Demo = true`:
1. **Adrenaline:** with a trinket on, block or parry, or use a power: an orange bar appears above health in the same style; nothing shows under the bars.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. Your friend sees and hears your corrupted wishbone's pings. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
5. **Character sheet:** with your friend standing next to you, open your inventory: only you appear in the character picture, never them.
