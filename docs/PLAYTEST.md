# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off by telling Claude the result (a screenshot or one line is enough); Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Before you start:** rebuild both mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1: solo (about 45 minutes)

### 1. Game data dump (2 minutes; unblocks rip step B and corrupted bosses)
1. Type `co_dump`.
2. Send the folder it names (`BepInEx/CraftingOverhaul/dump-<date>-<time>/`). New in it: `spawns.csv`, `events.csv`, `event_spawns.csv`, `status_effects.csv`, `containers.csv`, `destructibles.csv`, `altars.csv`.

### 2. Boss essences
1. `spawn CorruptionEssence_eikthyr`, and the same with `elder`, `bonemass`, `moder`, `yagluth`, `queen`, `fader` and `kall` in place of `eikthyr`.
2. Look at each one on the ground and in the inventory: model, icon, coloured glow on the ground. The Elder's has a root, Yagluth's a gold ring, Kall's a spiked crown.
3. `spawn Eikthyr`, kill it: it drops 1 essence. Kill a second one: it drops another (essences drop on every kill).
4. Optional: `spawn FrozenKing` and fight through to the last phase: only the last phase drops Kall's essence.

### 3. Maps and rips (step A)
1. `spawn CorruptionMap_Meadows`, right-click it in the inventory. Expect "The map stirs…", then a red stone portal 4 m in front of you.
2. Hover over it: it says where it leads and when it closes. Walk in.
3. You should land on the ground (not inside a rock, not on a tree) next to a big glowing red crystal, the rip heart.
4. Destroy the heart. A second portal opens next to it; hover: "Leads home". Walk in: you come out in front of the first portal.
5. Try another biome, e.g. `spawn CorruptionMap_Plains`.
6. Crafting: build a Riftforge, then put a shard, Eikthyr's essence and 5 leather scraps in your inventory and stand at the Riftforge: the Corrupted Meadows map recipe is there.
7. Optional: sleep or wait an in-game day: both portals and the heart disappear.
8. Things to note: anything that looks wrong (portal look, heart look, sinking, floating), and any red errors in the console.

### 4. Corrupted bosses
1. `spawn Eikthyr`, then `co_corrupt`: Eikthyr turns red.
2. Fight it: its hits hurt more and it takes longer to kill than a normal Eikthyr.
3. It drops 2–3 essences and 3–5 shards (plus its normal drops).

### 5. Talismans (the parts not yet tested)
1. Wear two different charms in the two Talisman slots and a vanilla trinket in the Trinket slot: creatures of both families ignore you and the trinket works.
2. Try putting the same charm in both slots: it should refuse.
3. Hit a repelled creature: it fights back for about 30 seconds, then ignores you again.
4. A boss ignores charms.

### 6. Craft from chests with AdventureBackpacks
1. Craft something whose ingredients are only in your backpack, with the same items in a nearby chest: only the backpack pays.
2. Split a cost between the backpack and a chest; check the totals afterwards (nothing charged twice).
3. Build with the hammer using backpack + chest materials, and bulk-plant a PlantEasily grid with seeds only in a chest.
4. Open a crafting station with the backpack panel open: the recipe search bar and categories still line up.

### 7. Small checks
1. Recipe list: category headers line up with the recipe icons, on one line, readable.
2. `co_biome Wisp`: tell Claude which biome it says.
3. Config reload: change a setting in a `.cfg` file and save while in game; the log says it reloaded.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
