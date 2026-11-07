# Play-tests: review fixes (`claude/admiring-babbage-rk1w8d`)

## Round: four review fixes (2026-11-07)

**Build:** the build stamp in the BepInEx log: `Corruption 0.1.0 loaded (build: claude/admiring-babbage-rk1w8d <commit>)` and the same for `CraftingOverhaul`. Every player and the server need this build.
**Setup:** your usual mods (EquipmentAndQuickSlots, AdventureBackpacks, PlantEasily). A test world where you can use `devcommands` (press F5, type `devcommands`). A workbench with 2 chests within 20 m.

What changed, in short:
1. **Inventory tabs:** if a tab's saved contents can't be read, that tab is locked (and kept safe in the save) instead of half-working and losing things.
2. **Magic and Rare drops:** creatures whose loot pops out of the body a few seconds after they die (most of them) now roll Magic and Rare too. Before, they never did.
3. **Crafting from chests:** a chest is reserved for you before you pay from it, so two players can't both be served from it at once.
4. **Arrow affixes:** an arrow uses the affixes of the bow it was shot from, even if you switch weapons while it flies.

### Checklist: alone (about 40 minutes)

**A. Inventory tabs (10 min).** Have a few shards in the Currency tab and at least one map in the Maps tab. Write down how many shards you have.
1. [ ] F5: `co_tabs_damage maps missing`. → The console says the tab data was damaged and the tab locked.
2. [ ] Log out to the main menu and back in. → A message in the middle of the screen: "Your Maps tab couldn't be loaded, so it's locked this session. Nothing in it was lost…". Clicking **Maps** shows the same message and stays on the bag. **Currency** works and has the same number of shards.
3. [ ] Spend one shard (craft a map at the Riftforge, or drop a shard and leave it). Log out and back in. → One shard fewer than before, not the old number.
4. [ ] Craft a map at the Riftforge. → It goes into the **bag** (the Maps tab is locked). Log out and back in. → The map is still in the bag.
5. [ ] F5: `co_tabs_damage maps undo`, then log out and back in. → No message; the Maps tab opens with your original maps; within a second the map from step 4 moves from the bag into the tab.
6. [ ] F5: `co_tabs_damage currency garbled`, log out and in. → The message names the Currency tab; Maps works. Then `co_tabs_damage currency undo`, log out and in. → All your shards are back.

**B. Magic and Rare drops from bodies (10 min).**
1. [ ] Somewhere open: `spawn Boar 20 2` (twenty 2-star boars), kill them all, three times (60 boars). Watch where loot appears. → Their normal loot pops out of the body when it fades, as always. Now and then a **coloured (Magic or Rare) item** drops right where a boar died, at the moment it dies. Expect a few (usually 2–4 in 60 kills: about a 4.5% chance per 2-star kill, from `drop_chances.csv` and `drop_modifiers.csv`). Creatures whose loot comes out of the fading body never dropped Magic or Rare before this fix.
2. [ ] Optional: kill a tamed animal. → Never a Magic or Rare drop from tamed animals.

**C. Arrow affixes (10 min).** You need a bow and arrows, and an axe or sword. Skadi's frost shows as extra frost (blue) damage numbers.
1. [ ] Equip **only the bow** (nothing in the other hand). F5: `co_affix skadi 5`. → "Added skadi …" to the bow.
2. [ ] Shoot a creature from far away and, while the arrow flies, switch to the axe. → The hit still shows frost damage (blue number). *(Wrong expectation, corrected 2026-11-07: see "What changed after the feedback" and C5–C6.)*
3. [ ] Equip the axe, F5: `co_affix skadi 5` (now on the axe). Equip a **different** bow with no affixes, shoot from far, switch to the axe while the arrow flies. → No frost damage on the arrow's hit. *(Same correction.)*
4. [ ] Optional, life steal: take some damage, put `co_affix life_steal 10` on the bow, shoot, switch to a plain weapon mid-flight. → Your health still goes up a little when the arrow hits.

**D. Crafting and building from chests, alone (10 min).** Put wood only in one chest near the workbench.
1. [ ] Craft something that needs the wood. → Crafts normally, with no waiting; the wood leaves the chest. The chest's lid may open for a moment.
2. [ ] Craft something that needs a bit from your bag and a bit from the chest. → Takes from your bag first, then the chest; amounts right.
3. [ ] With AdventureBackpacks: some of the cost in the backpack, the rest in a chest. → Backpack first, then chest; amounts right.
4. [ ] Build a wood piece with the hammer, wood only in the chest. → Builds; wood leaves the chest. Put the hammer away; after about 3 seconds the chest's lid closes.
5. [ ] Open the chest yourself while crafting from it. → It opens normally; crafting still works.
6. [ ] F5, no red errors during any of this.

### Checklist: on the dedicated server (about 40 minutes, 2–3 players)

Everyone on the new build (check the build stamp lines in each log).

**E. Two players, one chest.** A workbench and one chest within 20 m. Put in the chest **exactly enough wood for one craft** of something (e.g. a club needs 6 wood: put 6).
1. [ ] Players A and B: count down and press **Craft** at the same moment. → Only **one** of you gets the item. The other sees "A chest you need is in use by someone else: nothing was crafted or taken." The chest is empty; nobody lost anything from their bag.
2. [ ] Repeat a few times with enough wood for **two** crafts. → Both get one item (one may first see "Waiting for the nearby chests…" for a moment); the chest loses exactly two crafts' worth.
3. [ ] Player A holds the hammer with a piece that needs wood from the chest. Player B tries to open that chest. → B sees "in use". A puts the hammer away; after about 3 seconds B can open it.
4. [ ] Player A opens the chest and keeps it open. Player B crafts something needing its wood. → B gets the "in use by someone else" message and nothing is taken; once A closes it, B can craft.
5. [ ] If a friend doesn't have CraftingOverhaul installed: let them stand nearest the chest (so their game runs it). You craft from it. → Still works.

**F. Loot per player.** Two or more players together, kill about 20 starred creatures (`spawn Greydwarf 10 2`, twice).
1. [ ] → Magic and Rare items drop at the death spots; with two players near, about twice as many as alone. No item drops twice for one kill on two screens.

**G. Arrows on the server.** Repeat step C2 on the server. → Same result.

**H. Rip waves after the heart changes hands (needs 3 players; a check, not a fix).** This checks a suspected problem: the game running a rip's waves might count too few creatures after another player's game takes over. Each player: F5 → keep the BepInEx log open (or read it afterwards).
1. [ ] Player A opens a rip and stays by the heart until waves have spawned for a minute. Players B and C stand by too.
2. [ ] Player B leads a few wave creatures far away (about 150 m) and keeps them alive there.
3. [ ] Player A walks far away (over 200 m) from the heart. Player C stays by the heart.
4. [ ] Everyone: look in your log for `Rip waves: this game now runs the waves of rip …` (whose game took over) and the `alive (counted here) X of Y; spawning Z` lines after it. Count roughly how many wave creatures are alive (C near the heart, B far away).
5. [ ] Send Claude the log lines from the game that took over, plus your counts. If that game's "alive" number was lower than the real number and it spawned more than the limit (Y), the problem is confirmed and gets fixed next round.

### Your feedback (2026-11-07, single player)
- **A. Inventory tabs, steps 1–6:** passed: "1 through to 6 all tested and works". One issue: "the yellow text doesn't fit in the screen and goes out of bounds" (the locked-tab message).
- **B. Magic and Rare drops, step 1:** passed: "Killed over 100 greydwarves, about 60 1 star and 60 2 star. Only had around 5 items drop but at least that works now." Step 2 (tamed animal, optional) not reported.
- **C–H:** not done yet ("I'll continue the rest of the playtesting later").

### What changed after the feedback
- **A, the message:** it was one long line, and the game's centre message doesn't wrap. Now three short lines ("Your Maps tab couldn't be loaded and is locked." / "Nothing in it was lost; it's kept in your save." / "New pickups go to your bag."). To re-check: repeat A1–A2 and A5 (undo) and see that the whole message fits on screen.
- **B, "only around 5":** that's the expected rate, not a bug. From `drop_chances.csv` and `drop_modifiers.csv`: a 1-star kill drops Magic or Rare about 3.7% of the time, a 2-star kill about 4.5%, so 60 + 60 kills give about 5 on average. Raising it would be a balance choice for the loot round (the numbers are approved), so nothing changed.

### Your feedback (2026-11-07, second run, single player)
- **A–D:** "Everything seems to work as intended." The locked-tab message "is showing good" (fits now). "Lifesteal working well" (C4).
- **C2–C3, Skadi:** failed as written: "hitting anything with the skadi bow did not show any blue damage in numbers. I tried fire arrows and flint arrows."

### What changed after the second feedback
- **Skadi:** the checklist was wrong, not (as far as the code shows) Skadi. The game shows **one number per hit**, the total of every damage type, coloured only for weak or resistant targets; there is no separate blue frost number (read in the game code). Skadi's frost is added into that total. Its slowing frost effect lasts frost damage ÷ the target's max health × 10 seconds, so a few points of frost slow for a blink: nothing to see. So there was no way to see Skadi in play. **Added a test-only message:** while `devcommands` is on, each Skadi hit shows "Skadi: stack N, +X frost (weapon name)" at the top left. Re-test with C5–C6:
  - C5. [ ] `devcommands` on. Bow with Skadi (C1). Shoot the same creature several times in a row, quickly. → Top left: "Skadi: stack 1 …", then stack 2, 3, 4, 5 (it stays at 5), with the frost number growing. Wait 3 s or hit another creature: back to stack 1.
  - C6. [ ] Shoot, and switch to an axe without Skadi while the arrow flies. → The message still appears (the bow's Skadi). Then put Skadi on the axe only (C3), shoot a plain bow and switch to the axe mid-flight. → No Skadi message for that arrow; hitting with the axe itself shows it.

### Final play-test (date)
