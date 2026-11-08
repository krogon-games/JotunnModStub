# Play-test: `claude/loving-euler-qfkpzc`

## Round: the currency orbs at the Riftforge, and Stonecutter's/Resinous per hit (2026-11-08)

**Build:** the build stamp in the log line `Corruption … loaded (build: claude/loving-euler-qfkpzc …)`.
**Setup:** single player, `devcommands`. Stand at a **Riftforge** (build one if needed). Put the item to craft in **hotbar slot 1**; the orbs stay in your **Currency tab** (changed after your first try: `co_craft` takes the orb from there). Open the console with F5 to type commands and read the messages. `co_craft give` puts 5 of each orb in the Currency tab. `co_craft list` shows, for every orb you carry, what it would do to the item in slot 1, or why not. `co_craft <orb>` (e.g. `co_craft regal`) uses one.

### A. The orb items (about 3 minutes)
1. [x] `co_craft give` → 10 orbs appear in the **Currency tab**, each with its name, a description and an icon. Nine use the orb art; the **Mirror of Kalandra** is a plain pale gem for now (no art yet).
2. [x] Drop one orb on the ground → it glows in its own colour and can be picked up again (back into the Currency tab).

### B. Using orbs (about 15 minutes)
1. [ ] Walk away from the Riftforge, `spawn SwordBronze`, put the sword in slot 1, `co_craft list` → every orb says "refused: Use this at a Riftforge". Walk back and `co_craft list` again → Transmutation, Alchemy and Chance say "allowed"; the others say why not.
2. [x] **Transmutation:** `co_craft transmutation` → a message says the sword became a blue Magic item with **1** affix; the Currency tab has one Orb of Transmutation fewer.
3. [x] **Augmentation:** `co_craft augmentation` → **2** affixes, still blue. Again → refused: "This item has no room for another affix", and **no orb is used**.
4. [x] **Regal** on a Magic item with **1** affix (make a new one: `spawn SwordBronze` in slot 1, `co_craft transmutation`) → refused: "Only on Magic items with 2 affixes". Put the 2-affix sword from step 3 back in slot 1, `co_craft regal` → it turns **yellow (Rare)** with a two-word name and **3** affixes.
5. [x] **Exalted** on the Rare → **4** affixes. Again → refused (no room).
6. [x] **Divine** → the numbers change, the brackets (ranges) stay the same.
7. [x] **Annulment** on the Rare → one affix gone; it **stays Rare** and keeps its name.
8. [x] **Annulment** on a Magic item with 1 affix → it stays **blue with no affixes**. Then **Augmentation** on it → it gets 1 affix again.
9. [x] **Alchemy** on a new plain item → Rare with 3 or 4 affixes.
10. [x] **Chance** on 3 new plain items → each becomes **Rare** (always Rare until uniques exist).
11. [x] **Jeweller's Orb** (`co_craft jeweller`) → refused: "Sockets come later"; no orb used.
12. [x] A stack of Wood in slot 1, `co_craft transmutation` → refused: "This item can't take affixes".

### C. Mirror and corrupted items (about 5 minutes)
1. [x] Rare item in slot 1, `co_craft mirror` → a copy appears in your bag; its tooltip ends with "Mirrored: can't be changed". The original is unchanged.
2. [x] The copy in slot 1: `co_craft mirror` → refused "Mirrored items can't be copied"; `co_craft divine` → refused "Mirrored items can't be changed".
3. [x] The original in slot 1, `co_craft testcorrupt` → its tooltip now ends with a red **Corrupted** (a test mark: shards can't corrupt items yet). `co_craft exalted` or `divine` → refused "Corrupted items can't be changed". `co_craft mirror` → allowed: the copy says both **Corrupted** and **Mirrored**.
4. [x] Log out and back in → every item keeps its affixes, names and marks.

### D. Stonecutter's and Resinous, now on every hit (about 5 minutes)
1. [x] Fresh pickaxe (`spawn PickaxeAntler`), in your right hand, `co_affix stonecutters 100` → its tooltip says "100% chance per hit of one stone". Hit a rock or copper → **every hit drops one stone** where you hit, before anything breaks. Same on tin (which drops no stone itself).
2. [x] With that Antler pickaxe, hit a Mountains **silver vein** (or `spawn silvervein`) → "Too hard", **no stone**.
3. [x] Fresh axe (`spawn AxeBronze`), `co_affix resinous 100` → every chop on a standing tree, a log or a stump drops **one resin**.
4. [x] No red errors in the console during A–D.

### Your feedback (2026-11-08)
- Step B1: "put the sword in slot 1 and an Orb of Transmutation in slot 2 does not work. After half a second it teleports back to the currency tab." Krogon thinks that's intended (the Currency tab's design).

What it means: correct, the Currency tab moves any currency in the bag back to the tab twice a second (Currency and Maps tabs, by design), so an orb can't stay in hotbar slot 2 and `co_craft` never sees it. The checklist's mistake, not a tab bug. Steps A–C can't be done as written; D (Stonecutter's/Resinous) doesn't use orbs.

### Your feedback, re-test (2026-11-08)
- Krogon: "running co_craft list doesnt matter if its in riftforge window or not, returns same result. some orbs are allowed. others refused because item doesnt meet conditions." (Step B1: the "Use this at a Riftforge" refusal wasn't seen.)
- "No red errors in console", but repeated warnings: `GetBiome error BlackForest -> Meadows (486.04, 34.29, 228.98) -> 1064, 1026`, about once a second.
- "Besides that, all steps pass."

What it means:
- B1: open. `co_craft` counts you as at the Riftforge anywhere within its **build range, 10 m** (the game's `m_rangeBuild` for stations), with or without its window open. Waiting for Krogon: how far away was the test?
- The `GetBiome error` warning is the game's own (`Player.UpdateBiome`, once a second): two of the game's biome lookups disagree at the spot you stand on, here a Black Forest / Meadows border. None of our mods patch biome lookups; harmless.

### What changed after the feedback
- Step B1: `co_craft` now takes the orb from your bag or Currency tab (`co_craft regal` finds a Regal Orb itself); only the item goes in hotbar slot 1. `co_craft list` shows every orb you carry. Checklist A–C rewritten for this; nothing re-tested yet.

### Final play-test
-
