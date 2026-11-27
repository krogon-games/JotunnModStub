# Play-tests: `claude/festive-gates-79y1yr`

## Round: map tiers, map affixes and the explored-land fallback (2026-11-27)

**Build:** the three DLLs from this branch (only Corruption changed). The log line should read `Corruption 0.10.0 loaded (build: claude/festive-gates-79y1yr <commit>)` (versions are raised when the branch merges).
**Setup:** single player on Valheim 1.0.30, a test world, EquipmentAndQuickSlots and AdventureBackpacks stay loaded as usual (PlantEasily off). F5, `devcommands`, `god`. At the start the log (`BepInEx/LogOutput.log`) has the line `Corruption 0.10.0 loaded (build: claude/festive-gates-79y1yr …)`, then `Map tiers: 6 tiers, 7 affixes (swarming, hardened, rabid, smouldering roll).`, and no red errors. A world with a **Riftforge** (build one with the hammer next to a workbench, or use the one you already have). Stand next to it, then type these (each puts the items in your bag; the orbs and the shard move to the Currencies tab within half a second of opening the inventory):

```
spawn CorruptionOrb_Transmutation 3
spawn CorruptionOrb_Augmentation 2
spawn CorruptionOrb_Regal 2
spawn CorruptionOrb_Exalted 2
spawn CorruptionOrb_Annulment 2
spawn CorruptionOrb_Divine 1
spawn CorruptionOrb_Mirror 1
spawn CorruptionOrb_Alchemy 1
spawn CorruptionShard 5
```

**What's new (your decisions 1–13):**
- **Maps have tiers 1 to 6.** A white map is tier 1; Magic with 1 or 2 affixes is tier 2 or 3; Rare with 3 or 4 affixes is tier 4 or 5; a tier 5 map corrupted with a shard is tier 6 (red, locked, never breaks). You raise a map's tier at the Riftforge with orbs, as on gear.
- **Four map affixes:** Swarming (twice as many creatures), Hardened (rip creatures take 30% less damage), Rabid (rip creatures hit 40% harder and move 30% faster), Smouldering (every hit also burns). Every affix is danger only; the tier pays you back (more waves, more two-star creatures, more shards at the heart, a gear floor, higher drop rates).
- **The tier and affixes show** in the map's tooltip (and a small tier number in the corner of its icon) and when you hover the rip's portal and heart.
- **Explored-land fallback (#84):** when no unexplored land of the biome is left, the rip opens on land you've explored. Every rip (unexplored or not) stays at least 96 m from anything a player built.
- **Test commands:** `co_map <biome> [tier] [affix ...]` makes a map of any tier (biomes: Meadows, BlackForest, Swamp, Mountain, Plains, Mistlands, AshLands, DeepNorth; affixes: swarming, hardened, rabid, smouldering; with only a tier, the affixes are random); `co_rip` shows the nearest rip's tier and affixes; `co_rip fallback on|off` forces the explored-land fallback. `spawn CorruptionMap_Meadows` gives an ordinary map (it counts as tier 1, like any old map).

**Phone page:** the same steps are in the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: map tiers, map affixes and the explored-land fallback" (step ids `mt-1` … `mt-21`). The two-player steps are in `PLAYTEST.md` → Session 2.

**How the Forge tab works (as for gear):** open the inventory next to the Riftforge and click the **Forge** tab. Drag a map from the Maps tab (or the bag) onto the forge slot, or right-click the map. Right-click an orb or the shard in **Currencies** to pick it up, then click the forge slot to use it once. Esc or a right-click puts the pick back. Orbs that can't be used on the map are red.

### Checklist

**A. Maps, the tooltip and the Forge tab**

1. [ ] (`mt-1`) F5, `co_map Meadows` → the console says it put a Meadows map in your inventory (tier 1). Open the inventory → it's in the **Maps** tab, with a small white **1** in the corner of its icon. Hover it → the tooltip has **Tier 1 · Normal map** and three dim lines: `Waves x2 · no guaranteed two-stars`, `The heart: 1–4 shards · gear by chance` and `Drops x1.25 in the rip`.
2. [ ] (`mt-2`) At the Riftforge, open the **Forge** tab and drag that map from the Maps tab onto the forge slot (or right-click it) → the bench shows **Meadows map** and **Tier 1 · Normal map**. Right-clicking the map did **not** open a rip (the map is still in the Maps tab when you take it out again with Remove). Now right-click each orb and the shard in Currencies in turn: those that can't be used on a white map are **red** (Augmentation, Regal, Exalted, Annulment, Divine, the shard), while Transmutation, Alchemy, Chance and the Mirror are not red (Jeweller's is red too). The **Shatter** button is greyed out and says "Maps can't be shattered".
3. [ ] (`mt-3`) With the map in the forge, pick up an **Orb of Transmutation** and click the forge → a green line **Orb of Transmutation: now tier 2** and a green **+ <Affix>: <what it does>** line (for example "+ Hardened: Rip creatures take 30% less damage"). The number in the map's icon corner is now a **blue 2**, and the map's name and header in the tooltip are blue (Magic).
4. [ ] (`mt-4a`) Pick up an **Orb of Augmentation**, click the forge → **now tier 3**, a second affix is added, the number is a blue **3**.
5. [ ] (`mt-4b`) **Regal Orb** → **now tier 4**, a third affix, the number and the name turn **yellow** (Rare).
6. [ ] (`mt-4c`) **Exalted Orb** → **now tier 5**: all four affixes (Swarming, Hardened, Rabid, Smouldering, in whatever order they came). The description panel's tier lines change at every step; at tier 5 they read `Waves x4 · every creature two-star`, `The heart: 5–12 shards · gear at least Rare` and `Drops x2.25 in the rip`.
7. [ ] (`mt-5`) Pick up the **Corruption Shard** and click the forge → a **Corrupt Map** dialog: it says the map becomes tier 6 (Corrupted Meadows map), it never breaks but is locked; there is **no** "destroys the item" warning and **no** chance table; it has its own "Don't warn me again" tick box. Press **Corrupt** → **Corrupted: now tier 6**, the name is **Corrupted Meadows map** in red, a **red 6** in the icon corner, and a red **Corrupted** mark in the tooltip. Now **every orb except the Mirror of Kalandra is red** (right-click them to look).
8. [ ] (`mt-6`) **Refusals.** `co_map Meadows 3` and put that map in the forge, then pick up the shard and click the forge → "Only a tier 5 map (4 affixes) can be corrupted." Pick up the **Divine Orb** (on any map) → "Map affixes have no values to reroll." To get an Affix Splinter: `co_loot magic`, put that item in the forge, press Shatter (confirm), take the splinter from the Splinters tab, then put the map in the forge and click with the splinter → "Affix Splinters don't work on maps". (The refusal text shows where the Forge normally says why something can't be used; tell me if any of them shows nowhere.)
9. [ ] (`mt-7`) With the tier 3 map (`co_map Meadows 3`) in the forge, use an **Orb of Annulment** → it drops to **tier 2** and a red **− <Affix>** line names the affix that was removed.
10. [ ] (`mt-8`) `co_map Meadows` (a new white map), **Orb of Alchemy** on it → **tier 4** with **3 affixes** (a yellow, Rare map).
11. [ ] (`mt-9`) Put a map with affixes in the forge (the tier 4 one) and use the **Mirror of Kalandra** → "Mirror of Kalandra: copied. The copy is in your inventory and can never be changed."; the copy is in your Maps tab (or the bag if it was full), with the same tier and affixes. Try any orb on the copy (and the Mirror again) → every one is refused ("Mirrored items can't be changed" / "Mirrored items can't be copied").
12. [ ] (`mt-10`) Gear still works: `spawn AxeStone` (a Normal item), put it in the forge and use an Orb of Transmutation on it as before (`spawn CorruptionOrb_Transmutation` if you have none left) → it works as it always did (one quick check).
13. [ ] (`mt-11`) `spawn CorruptionMap_Meadows` (an "old" map with no tier stored) → it shows as **Tier 1 · Normal map**, works in the forge (try a Transmutation Orb on it) and, if you use it, opens a rip (step `mt-12`).

**B. Rips by tier** (Meadows is quickest; any biome works). Stand in open Meadows ground, away from your base.

14. [ ] (`mt-12`) Use a white map (`co_map Meadows`, then use it from the Maps tab the way you always have) → the rip opens as before. Hover the entry rip and, later, the heart → both show a **Tier 1** line. The log has `Rip opened: Meadows, Tier 1, heart …, landing …, unexplored land.` Waves come as you know them. Destroy the heart → 1–4 shards and a white map (the Meadows chest's loot too); the log has `Rip loot: tier 1.` **Also check this once:** hold one white map and one tier 3 map (`co_map Meadows 3` first) and use the tier 3 one → it is the tier 3 map that's used up, not the white one.
15. [ ] (`mt-13`) `co_map Meadows 3 swarming hardened` → tooltip **Tier 3 · Swarming, Hardened**. Use it. Hover the rip → **Tier 3 · Swarming, Hardened**; `co_rip` standing near the rip prints a line like `Rip entry portal, … m away: Meadows, Tier 3 · Swarming, Hardened. Closes in …`. The log has `Rip waves: this game now runs the waves of rip … : Tier 3 · Swarming, Hardened, waves x6, 50% guaranteed 2-stars.` (tier 3's x3 times Swarming's x2). About half of the creatures are 2-star; they take noticeably more hits to kill (Hardened). Destroy the heart → 3–8 shards and your own gear item of at least Magic quality. The log has `Rip loot: tier 3.`
16. [ ] (`mt-14`) `co_map Meadows 4 rabid smouldering hardened` → use it. The log has a `Maddened (Rabid, factor 2): damage ×1.4, speed +30%.` line (it can come a little after the rip opens). The creatures are visibly faster and hit hard; a hit from one **sets you burning** (a fire icon on your status bar), and a **Fire Resistance Barley Wine** makes the burning weaker. The heart gives 4–10 shards and your own gear item of at least Rare.
17. [ ] (`mt-15`) `co_map Meadows 6` → tooltip **Tier 6 · Swarming, Hardened, Rabid, Smouldering** (any order) and a red Corrupted mark. Use it → every creature is a 2-star, and there are many (Meadows tier 6 with Swarming: up to 18 boars and 18 necks alive at once). Note any lag. The heart gives 6–14 shards and your own gear item of at least Rare; the log has `Rip loot: tier 6.`
18. [ ] (`mt-16`) The map the heart spills (after any of the rips above) is always a **white tier 1** map with no affixes.

**C. The explored-land fallback (#84)**

19. [ ] (`mt-17`) `co_rip fallback on`, then use a map (any, `co_map Meadows`) → the message **No unexplored Meadows is left: the rip opens on land you've explored, away from anything players built.** shows in the middle of the screen, and then once more under **A rip tears open into the corrupted Meadows.** The log has `Rip request: N candidate(s) (explored-land fallback), M away from player-placed objects (96 m); checked … in X ms` and `Rip opened: …, explored-land fallback.` (A map with no explored land at all may say "The map finds no flat Meadows to open a rip on, explored or not. Try again."; try again from another spot.)
20. [ ] (`mt-18`) Go through the rip. The landing spot and the heart are on land you have already been to, and **at least 96 m from anything you built** (the minimap and map (M) show your base's pieces: nothing you built is within about 96 m, roughly 15 seconds of running, of the heart or of where you landed), also from your base. To check it properly, build a campfire a good way from your base first, then use another map: the new rip keeps away from the campfire too.
21. [ ] (`mt-19`) `co_rip fallback off` → "Explored-land fallback test: off." Then `co_rip` far from any rip → "No rip portal or heart within 200 m." Use a map again → the rip is back on unexplored land as before (log: `unexplored land`, no fallback message).
22. [ ] (`mt-20`) **Leftovers:** let a rip close (it closes one in-game day after opening: sleep in a bed to skip), then go back to the heart's spot → the red rip creatures that were still alive have left or are leaving (they despawn once nobody fights them, like a vanilla raid's). Tell me if they stay forever.
23. [ ] (`mt-21`) F5 shows no red errors from the whole session. Send `BepInEx/LogOutput.log`. The `Rip request … in X ms` numbers matter (how long the server's check took, the fallback one especially).

### Your feedback (<date>)
- Step 1: 

### What changed after the feedback
- 

### Final play-test (<date>)
- 
