# Play-test: `claude/lucid-carson-hs5g89`

## Round: six inventory tabs and the Riftforge's Forge tab (2026-11-09)

**Build:** the build stamp in the log line `Corruption … loaded (build: claude/lucid-carson-hs5g89 …)`.
**Setup:** single player, **your usual character** (so the old Currency tab's contents are there), EquipmentAndQuickSlots and AdventureBackpacks loaded as usual, `devcommands`. Open the console with F5 to type commands and read messages. Have a **Riftforge** nearby (build one if needed). `co_craft give` puts 5 of each orb in Currencies; `co_loot magic` drops a Magic item; `spawn SwordBronze` a plain one.

**What isn't built yet (so it can't be tested):** corrupting gear with a shard, shattering and Affix Splinters are the loot session's next round (#29). The window says so where they'd be. Runes don't exist at all.

### A. The six tabs, and nothing lost (about 5 minutes)
1. [ ] Open the inventory → six buttons on top: **Bag · Currencies · Splinters · Maps · Runes · Essences**, in that order, all inside the panel, none hiding the equipment slots or the crafting panel. "Currencies" and "Essences" are readable.
2. [ ] Before anything else: everything you had in the old **Currency** tab is still there. Orbs and shards are in **Currencies**; **boss essences are now in Essences** (same counts as before). Maps are in **Maps** as before. Note anything missing.
3. [ ] Log out and back in → every tab has the same contents and counts.
4. [ ] `co_tabs_misplace 2` → two Eikthyr essences go into Currencies and, within half a second, **move to Essences** (the essence count there goes up by 2). Do it twice: nothing doubles, nothing vanishes.
5. [ ] At the Riftforge's **Craft** tab, craft a map (it takes shards from Currencies and an essence from **Essences**) → the map appears in Maps; the counts go down by the recipe's amounts.

### B. Currencies: fixed places and counts (about 5 minutes)
1. [ ] In **Currencies**: the shard in the first cell, then Transmutation, Augmentation, Alchemy, Regal, Exalted, Divine, Annulment (top row), then Chance, Jeweller's and Mirror. Each shows its count.
2. [ ] An orb you have **none** of still shows its cell: its picture **dimmed**, a **0**, and hovering says "You have none left". (Drop all of one orb on the ground to see it, then pick them up again: they go back to that cell.)
3. [ ] Drag an orb to another cell → once dropped, it goes **back to its own cell**.
4. [ ] `co_craft give 60` (more than one stack of 50) → each orb's first stack sits in its own cell and the extra stacks go to the empty cells further down. Nothing ends up in the bag while Currencies has room.
5. [ ] **Essences** looks the same way: one fixed cell per boss essence, dimmed with 0 for those you haven't got.

### C. Splinters and Runes (about 3 minutes)
1. [ ] **Splinters**: an empty grid of 8 × 4 cells, like the bag, no scroll bar or search box. Try to drag an orb or a sword into it → refused (it stays where it was). (Real splinters come with #29; then: one per cell, never stacked, and to and from a chest.)
2. [ ] **Runes**: no grid, only "Runes are not available yet."
3. [ ] Pick up any item (left-click it in the Bag), switch to Runes and click in the middle of the note → nothing happens: the item isn't put anywhere and isn't dropped on the ground. Switch back and put it down.
4. [ ] Pick things up from the ground, take all from a chest, craft something → nothing ever appears in Runes.

### D. The Forge tab: put gear in, craft, take it out (about 10 minutes)
1. [ ] Use the **Riftforge** (E) → besides Craft and Upgrade there's a third tab, **Forge**. At a workbench or forge there's **no** Forge tab.
2. [ ] Click **Forge** → the workspace: "Riftforge", an empty slot, buttons Take out and Shatter, a hint line at the bottom. Craft and Upgrade still work; clicking them leaves the Forge tab.
3. [ ] `spawn SwordBronze`. In the **Bag**, left-click the sword to pick it up, then click the Forge slot → the sword's picture is in the slot, it's **still in your bag** (in its old cell), and its normal tooltip is shown beside the slot.
4. [ ] Switch the inventory to **Currencies** → the slot and the tooltip stay visible. With the plain sword in, orbs that can't be used on it look **red** (e.g. Augmentation, Regal); orbs you have none of stay **dimmed with 0** (they look different).
5. [ ] **Right-click** an Orb of Transmutation → it follows the cursor with your count, its cell is highlighted, the line under the slot says "Orb of Transmutation (N left)". Nothing is used yet.
6. [ ] **Left-click the slot** → the sword becomes Magic (blue): the tooltip beside it updates **at once**, the result line says what it gained, Transmutation's count goes down by 1, the pick is gone, the sword is still in the slot.
7. [ ] Right-click **Augmentation**, then **hold Shift and left-click** the slot → one affix added and the pick **stays**; Shift-click again → refused "This item has no room for another affix", in red, and **no orb is used** (count unchanged).
8. [ ] With a pick, right-click another orb → the pick changes to that orb. Press **Escape** → the pick is dropped and the **inventory stays open**. Press Escape again → the inventory closes normally.
9. [ ] With a pick, left-click any item in a tab or the bag → only the pick is dropped (nothing picked up, moved or equipped). With no pick, everything in the inventory works as usual (equip, drag, split).
10. [ ] Right-click the **Jeweller's Orb** → "Sockets come later", in red. Clicking the slot with it uses nothing.
11. [ ] Right-click a **Corruption shard** → "Corrupting gear with a shard isn't built yet". The **Shatter** button is greyed and says "Shattering isn't built yet".
12. [ ] **Mirror of Kalandra** on a Magic or Rare item → the copy appears in your bag; the original stays in the slot.
13. [ ] Press **Take out** (or right-click the slot) → the slot empties; the sword is where it always was in your bag.
14. [ ] Put the sword in again and close the inventory with the sword in the slot → nothing goes missing; reopening shows an empty slot (the sword in your bag).
15. [ ] Put the sword in, then drop it on the ground from the bag (or move it into a chest) → the slot empties by itself.
16. [ ] Pick up an item from **Currencies** or from a chest and click the slot → refused "Put in gear from your Bag". An orb or wood from the bag → "This item can't be worked at the Riftforge".
17. [ ] Walk away from the Riftforge with the inventory open (or open the inventory away from it) → no Forge tab; right-clicking an orb does what it always did (nothing special). `co_craft list` more than 10 m away → "Use this at a Riftforge".

### E. Death, gravestone, reconnect (about 5 minutes)
1. [ ] Carry something in **Currencies, Maps and Essences**, then die (a fall from a cliff, or let a creature kill you). Go to the gravestone → its contents include them, in rows under the bag's.
2. [ ] **Take all** → orbs and shards back in Currencies (each in its own cell), essences in Essences, maps in Maps; nothing left behind, nothing extra in the bag.
3. [ ] Log out and back in → all still there. No red errors in the console during A–E.

### F. Later, with #29 (shattering and splinters), not now
- Put a Magic or Rare item in the Forge slot, press **Shatter** → a box warns the item will be destroyed and shows each line's chance; Cancel keeps it; Confirm destroys it, the splinters arrive in **Splinters**, the slot empties and the result is shown.
- Right-click a splinter, click a Normal or Magic item → the line is added; refused where the rules say.
- A shard on an item → a warning with the chances and Confirm before anything happens.
- Splinters move to and from a chest with their exact values.

### Your feedback
(waiting for your play-test)
