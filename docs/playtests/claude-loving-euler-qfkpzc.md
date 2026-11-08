# Play-test: `claude/loving-euler-qfkpzc`

## Round: the currency orbs at the Riftforge, and Stonecutter's/Resinous per hit (2026-11-08)

**Build:** the build stamp in the log line `Corruption … loaded (build: claude/loving-euler-qfkpzc …)`.
**Setup:** single player, `devcommands`. Stand at a **Riftforge** (build one if needed). The orb commands use **hotbar slot 1** (the item) and **hotbar slot 2** (the orb). Open the console with F5 to type commands and read the messages. All orbs: `co_craft give` puts 5 of each in your **Currency tab**; drag one at a time into slot 2. `co_craft list` shows what the orb in slot 2 can do to the item in slot 1, or why not.

### A. The orb items (about 3 minutes)
1. [ ] `co_craft give` → 10 orbs appear in the **Currency tab**, each with its name, a description and an icon. Nine use the orb art; the **Mirror of Kalandra** is a plain pale gem for now (no art yet).
2. [ ] Drop one orb on the ground → it glows in its own colour and can be picked up again (back into the Currency tab).

### B. Using orbs (about 15 minutes)
1. [ ] Walk away from the Riftforge, `spawn SwordBronze`, put the sword in slot 1 and an Orb of Transmutation in slot 2, `co_craft list` → "refused: Use this at a Riftforge". Walk back.
2. [ ] **Transmutation:** `co_craft transmutation` → a message says the sword became a blue Magic item with **1** affix; the orb stack goes down by 1.
3. [ ] **Augmentation:** orb in slot 2, `co_craft augmentation` → **2** affixes, still blue. Do it again → refused: "This item has no room for another affix", and **no orb is used**.
4. [ ] **Regal** on a Magic item with **1** affix (make a new one: `spawn SwordBronze`, Transmutation) → refused: "Only on Magic items with 2 affixes". On the 2-affix sword from step 3 → it turns **yellow (Rare)** with a two-word name and **3** affixes.
5. [ ] **Exalted** on the Rare → **4** affixes. Again → refused (no room).
6. [ ] **Divine** → the numbers change, the brackets (ranges) stay the same.
7. [ ] **Annulment** on the Rare → one affix gone; it **stays Rare** and keeps its name.
8. [ ] **Annulment** on a Magic item with 1 affix → it stays **blue with no affixes**. Then **Augmentation** on it → it gets 1 affix again.
9. [ ] **Alchemy** on a new plain item → Rare with 3 or 4 affixes.
10. [ ] **Chance** on 3 new plain items → each becomes **Rare** (always Rare until uniques exist).
11. [ ] **Jeweller's Orb** → refused: "Sockets come later"; no orb used.
12. [ ] A stack of Wood in slot 1 with Transmutation → refused: "This item can't take affixes".

### C. Mirror and corrupted items (about 5 minutes)
1. [ ] Rare item in slot 1, **Mirror** in slot 2, `co_craft mirror` → a copy appears in your bag; its tooltip ends with "Mirrored: can't be changed". The original is unchanged.
2. [ ] The copy in slot 1: Mirror → refused "Mirrored items can't be copied"; Divine → refused "Mirrored items can't be changed".
3. [ ] The original in slot 1, `co_craft testcorrupt` → its tooltip now ends with a red **Corrupted** (a test mark: shards can't corrupt items yet). Exalted or Divine → refused "Corrupted items can't be changed". **Mirror** → allowed: the copy says both **Corrupted** and **Mirrored**.
4. [ ] Log out and back in → every item keeps its affixes, names and marks.

### D. Stonecutter's and Resinous, now on every hit (about 5 minutes)
1. [ ] Fresh pickaxe (`spawn PickaxeAntler`), in your right hand, `co_affix stonecutters 100` → its tooltip says "100% chance per hit of one stone". Hit a rock or copper → **every hit drops one stone** where you hit, before anything breaks. Same on tin (which drops no stone itself).
2. [ ] With that Antler pickaxe, hit a Mountains **silver vein** (or `spawn silvervein`) → "Too hard", **no stone**.
3. [ ] Fresh axe (`spawn AxeBronze`), `co_affix resinous 100` → every chop on a standing tree, a log or a stump drops **one resin**.
4. [ ] No red errors in the console during A–D.

### Your feedback (2026-11-08)
- Step B1: "put the sword in slot 1 and an Orb of Transmutation in slot 2 does not work. After half a second it teleports back to the currency tab." Krogon thinks that's intended (the Currency tab's design).

What it means: correct, the Currency tab moves any currency in the bag back to the tab twice a second (Currency and Maps tabs, by design), so an orb can't stay in hotbar slot 2 and `co_craft` never sees it. The checklist's mistake, not a tab bug. Steps A–C can't be done as written; D (Stonecutter's/Resinous) doesn't use orbs.

### What changed after the feedback
-

### Final play-test
-
