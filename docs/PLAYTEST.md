# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** since 2026-11-01 every branch is merged, so build from `master` (a session's new work: from its branch, it will say so); rebuild all three mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

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
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Odin items (2 minutes)

~~Equip and log check~~ passed 2026-10-31 (the log also shows the recipes unlocked). Left: craft one.

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
6. **Stonecutter's:** changed 2026-11-08 to roll on every hit, tested in `docs/playtests/claude-loving-euler-qfkpzc.md` instead.
7. **Resinous:** changed 2026-11-08 to roll on every hit, tested in `docs/playtests/claude-loving-euler-qfkpzc.md` instead. **Forester's:** now and then a standing tree drops its seed (beech seeds, pine cone, birch seeds, fir cone, acorn).
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

Steps 2–4 passed 2026-11-08 (PR #55, including Veinbreaker's reset when the tool is unequipped), apart from Odin's Grace on a one-piece rock. Left: 1, that part of 4, and 5–14.

Build from **`master`** (merged; the loot branch is deleted). Roll tools with the affix you want (`co_loot rare PickaxeAntler`, `co_loot rare AxeFlint`, `co_loot magic PickaxeBronze`…), or `co_affix <id> 100` on the tool in your hand. These lines are no longer grey; the on-harvest ones (Bountiful, Artisan, Gemseeker's, Stonecutter's, Resinous, Forester's, Charring, Determined) are tested in Session 3d.

1. **Prospector's** on a pickaxe: rocks and copper/tin take more hits off per swing than with a plain pickaxe of the same kind (watch the damage numbers).
2. ~~**Veinbreaker's:** every 5th swing at rock or ore shows a bigger damage number.~~ Passed 2026-11-08.
3. ~~**Seismic:** now and then a swing at a copper or tin deposit knocks off the chunks next to the one you hit too.~~ Passed 2026-11-08.
4. **Odin's Grace:** ~~now and then a swing breaks the chunk you hit and every chunk touching it at once; iron (tier) still needs the right pickaxe~~ passed 2026-11-08 (deposits, tin, silver too hard for a lower pickaxe). Left: a single one-piece rock breaks in one hit.
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

~~Max health/stamina/eitr, carry weight, movement, regen, resistances, no errors~~ passed 2026-10-31. Left: steps 6 (weapon skill: failed 2026-10-31, now a check for bug #56) and 7 (life steal; passed on a bow 2026-11-07, not yet with other weapons).

Build from `master` (merged 2026-10-31). Roll items until you have the affix (`co_loot rare ArmorLeatherChest`, `co_loot rare CapeDeerHide`, `co_loot rare SwordBronze`…); these lines are no longer grey.

1. **Max health / stamina / eitr:** equip an item with "+N max health" (or stamina, eitr): the health/stamina bar and the character sheet go up by N; unequip: back down.
2. **Carry weight:** "+N carry weight": the weight limit (inventory and character sheet) rises by N.
3. **Movement speed:** "+N% movement speed" (legs, cape, trinket) or Fleet "while held" (weapon or shield in hand): "Speed from gear" on the character sheet rises by N%; you run faster. Fleet stops counting when you sheathe.
4. **Regen:** "+N% health regen" / "+N% stamina regen": regen is visibly faster (compare with the item off).
5. **Resistances:** "N% less fire damage taken" (or frost, lightning, poison, all resistances): stand in a fire (`spawn Surtling` or a campfire) with and without the item: less damage with it.
6. **Weapon skill:** "+N to the weapon's skill" on a weapon: with it in hand, the skill in the Skills tab shows +N; sheathed: back to normal. **Check for [#56](https://github.com/krogon-games/JotunnModStub/issues/56)** (2026-10-31 the panel showed +6 for a +7 axe; suspected a display quirk after a death): equip an axe, type `co_affix weapon_skill_prefix 7`, then `raiseskill Axes 0.6` (the console prints the exact level, e.g. `Skill Axes = 25.6`): the Skills tab should show **+6**. Then `raiseskill Axes 0.4` (back to a whole level): it should show **+7**. If both show +7, tell Claude (the suspected cause is wrong). Send the console lines either way.
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

## Orlog stone and class choice

All passed 2026-11-02 (single player), size included. A custom design comes later.

## Orlog skill tree and skill points

All passed 2026-11-02 (single player).

## Classes UI (merged into `master`, 2 minutes)

All passed 2026-10-31 ("everything looks really good now"); ~~the adrenaline bar~~ passed the same day (phone log).

## Session 2: with a friend

**One evening for the four of us on the dedicated server** (about 2½ hours). Nothing here has been tried with more than one player yet. Do the parts in order; each step says **who** does it, **what to do** and **what should happen**. Note ✅/❌ and what you saw for each step. Krogon = the admin (server files, `devcommands`); player A and player B = any two of you.

**Question for the user:** does `devcommands` (and so `spawn`, `co_corrupt`, `orlog_level`) work for you on the dedicated server? If not, Part 0 needs the real items (shards, an essence, a map, trophies) brought in from play.

### Part 0: before the evening (Krogon)

1. Build all three DLLs from `master` and send the same `Corruption.dll`, `Orlog.dll` and `CraftingOverhaul.dll` to everyone. Install them on the server too ([`PLAYER_SETUP.md`](PLAYER_SETUP.md) → For the server admin).
2. Gameplay settings come from the server: leave its config files at the defaults unless a step below says otherwise.
3. For the quick checks, put on the server or bring: 3 chests and a workbench at the base (Part 2a), 3 Greydwarf trophies, 1 Chain and 10 Leather scraps (or `spawn CorruptionTalisman_Greydwarf`), a few shards, a Corrupted wishbone and a `CorruptionMap_Meadows` (Part 3), and 2 Deer trophies (Part 4).

### Part 1: setup and build check (everyone, 10 minutes)

1. **Each player:** start with r2modman's **Start modded** and join the server. Expect: no "incompatible version" message.
2. **Each player and Krogon (also the server's own log):** open `BepInEx/LogOutput.log` and find the three lines. Expect, the **same** on every machine:
   - `Corruption 0.6.0 loaded (build: master <commit>)`
   - `Orlog 0.2.0 loaded (build: master <commit>)`, the same commit
   - `CraftingOverhaul 0.3.0 loaded (build: master <commit>)`, the same commit
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
9. **One player:** bulk-plant a PlantEasily grid with the seeds only in a nearby chest. Expect: it plants and takes the seeds from the chest (not yet tried even alone).

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

### Part 3: a rip run (about 45 minutes)

On a dedicated server the rip request goes to the server and its answer comes back; alone this was all one game.

1. **One player (the opener):** wear the Corrupted wishbone, use `CorruptionMap_Meadows` near the base. Expect: a red rip opens 4 m in front of them. If it says no spot was found, note the message. If a rip ever opens near anyone's bed, ward or portal, tell Claude (the server should prevent that).
2. **Everyone:** go through. Expect: all land at the same spot, no heart in sight, no waves yet.
3. **Everyone else:** watch and listen. Expect: you see and hear the opener's red pings (the log says how they reach other players: `shared with other players by …`).
4. **Everyone:** walk towards the heart together. Expect: within 96 m the army message shows and waves come. Every player sees the creatures red, also the ones that were already standing there when the waves started (fixed 2026-11-01); they attack everyone and each other; charms don't help.
4b. **Everyone:** split up, 100 m or more apart around the heart. Expect: the waves don't pile up beyond their limit near any one of you (fixed 2026-11-01: limits now count creatures far from the game that spawned them).
5. **Everyone:** destroy the heart. Expect: the exit rip opens and a chest with Meadows loot and 1–3 shards appears; the pings stop; the waves stop.
6. **Everyone:** go home through the exit rip. Expect: everyone ends at the first rip.
7. **Ask:** did anything differ between the opener and the others (the heart's place, the chest, the portal hover text)?

### Part 4: a boss (about 30 minutes)

1. **Everyone at Eikthyr:** summon and kill him normally. Expect: **1** essence in total (not one each), plus 1–3 shards if this is the world's first Eikthyr kill; everyone within 70 m gets XP.
2. **One player:** craft a Corrupted deer trophy at the Riftforge (2 Deer trophies + 1 shard, or `spawn CorruptedTrophyDeer`) and offer it at the altar. Expect: Eikthyr arrives red **for everyone**.
3. **Everyone near him:** fight. Expect: every 30 s 2–3 red creatures of his army arrive; at most 4 alive **per player** near him (16 with four of us); they go for players only, never Eikthyr or each other. Summons run by another player's game behave the same.
4. **Kill him.** Expect: 2–3 essences and 3–5 shards in total; summons stop coming.

### Part 5: before you log off

1. Optional ([`multiplayer-test.md`](multiplayer-test.md) step 10): **Krogon** sets `[2 - Nearby Containers] Enabled = false` in the server's `com.krogongames.craftingoverhaul.cfg` and restarts it; **a player** tries crafting from a chest. Expect: they can't (the server's setting wins). Set it back.
2. Optional (step 11): **one player** joins without `CraftingOverhaul.dll` while another crafts from chests. Expect: they play normally and see the chest contents update.
3. **Everyone:** no red errors in F5. Copy your `BepInEx/LogOutput.log` right away (the next start overwrites it) and send it to Krogon, with the ✅/❌ notes.
