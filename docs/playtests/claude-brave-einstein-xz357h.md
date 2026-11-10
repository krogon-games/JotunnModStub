# Play-test: `claude/brave-einstein-xz357h`

## Round: shard corruption, shattering and Affix Splinters at the Riftforge (#29) (2026-11-10)

**Build:** the Corruption DLL built from this branch (the log's build stamp at start-up). Corruption still says 0.6.0: the coordinator raises it at merge.
**Setup:** single player, your test world, `devcommands` on, a Riftforge built. EquipmentAndQuickSlots and AdventureBackpacks loaded as usual. About 30 minutes.
**Not final:** every Affix Splinter looks the same for now (a pale green crystal). The real designs wait for your pick (`docs/images/splinters_draft2.png`).

**Console commands you'll use** (F5):
- `co_craft give` → 5 of every orb **and 5 Corruption shards** in Currencies.
- `co_loot rare` / `co_loot magic` → a Rare / Magic item at your feet (repeat for more).
- `spawn SwordBronze` → a Normal item.

### Checklist

#### A. Shattering (about 8 minutes)
1. [ ] Get 3–4 Rare and Magic items (`co_loot rare`, `co_loot magic`). Open the Riftforge → **Forge** tab. Drag a Rare item onto the forge slot → the **Shatter** button is lit.
2. [ ] Press **Shatter** → a Yes/No box: the item's name "will be destroyed", "Cost: nothing", one line per affix starting with **50%**, and a **Don't warn me again** tickbox under the text. Press **No** → nothing changed, the item is still in the slot.
3. [ ] Press **Shatter**, then **Yes** → the item is gone, the slot is empty, and the result says "Shattered …: N splinters (…)" or "… no splinters recovered". The inventory stays on the tab you were on.
4. [ ] Open the **Splinters** tab → one splinter per line the message listed. Hover one → "Affix Splinter", the affix with its number and range, "Prefix" or "Suffix", and "Fits: …". The numbers match what the item had.
5. [ ] Shatter two more items. Splinters never stack, even identical ones.
6. [ ] Put a **Normal** item in the slot → Shatter is grey; its tooltip says "Only Magic and Rare items shatter".
7. [ ] If you get an item with a green ◆ resonant line: shatter it until that line survives → **one** splinter holding the combined number, marked resonant, "needs an item with no affixes".

#### B. Full storage (about 5 minutes)
Splinters go to the Splinters tab first, then the Bag. The refusal needs **both** full.
1. [ ] Fill the **Splinters** tab (32 cells): keep doing `co_loot rare` and shattering. (Long-winded; if you'd rather skip this section, say so and I'll add a test command that fills the tab.)
2. [ ] Fill the **Bag** with junk so no cell is free, keeping one Rare item with 3 or 4 affixes in it. Put that item in the forge slot.
3. [ ] Press **Shatter** → no box; the result says **"Make room for up to X splinters."** (X = the item's number of affix lines). The item is untouched.
4. [ ] A Magic item with **1** affix still shatters with everything full: its own bag cell counts as the one free place.
5. [ ] Free enough cells → Shatter works again. No splinter ever lies on the ground.

#### C. Applying splinters (about 6 minutes)
1. [ ] `spawn SwordBronze`, put it in the forge slot. Open **Splinters**, right-click a splinter → it follows the cursor. Splinters that can't go on this item are tinted red.
2. [ ] Click the forge slot → **no box**; the sword becomes **Magic** and has that affix with **exactly the splinter's number**. The splinter is used up. The result says what was gained.
3. [ ] Apply a second splinter of the other side (a suffix if the first was a prefix) → works; the item now has 2 affixes.
4. [ ] Try a third → refused: "This item has no room for this splinter". Nothing used.
5. [ ] Try a splinter on a **Rare** item → "Splinters only go on Normal and Magic items".
6. [ ] Try an armour-only splinter on the sword (or a weapon one on armour) → "This affix can't roll on this item".
7. [ ] **Twin** (if you have both halves of one stat, e.g. a max-health prefix and a max-health suffix as splinters, or one on an item and the other as a splinter): apply the second half → one green ◆ resonant line whose number is **the two added, plus a quarter** (14 and 18 → 40). Same result every time.

#### D. Corrupting with a shard (about 8 minutes)
1. [ ] Put a Magic or Rare item in the slot. In **Currencies**, right-click the **Corruption shard**, click the slot → a box: "This can destroy <name>", "Cost: 1 Corruption shard (you have N)", three lines with **25% / 25% / 50%**, a line saying a surviving item is locked for good except for the Mirror, and a **Don't warn me again** tickbox.
2. [ ] Press **No** → nothing changed; you still have N shards.
3. [ ] Do it again and press **Yes**. Repeat on several items until you have seen all three results. Each time exactly **one** shard is used.
   - [ ] **Destroyed:** the item is gone, the slot empties, the result says "Destroyed: the shard consumed …".
   - [ ] **Unchanged:** the result says **"Corrupted and locked; no affix added."** (not "no change"); the item shows a red centred **Corrupted**.
   - [ ] **Corrupted affix:** one extra **red** line ending in "(corrupted)", highlighted for a few seconds; the item shows **Corrupted**. A full Rare has 5 lines.
   - [ ] (Rare luck, skip if not seen) If the corrupted affix is the other half of a twin the item has, the two merge into one resonant line worth **more** than the item's half: the two added, plus a quarter.
4. [ ] On a corrupted item: an orb → "Corrupted items can't be changed"; a shard → "Already corrupted"; a splinter → refused; Shatter is grey ("Uniques and corrupted items can't be shattered").
5. [ ] `spawn SwordBronze`, corrupt it until it gets an affix → it stays a normal (white) item with one red corrupted line.
6. [ ] **Mirror:** use a Mirror of Kalandra on a corrupted item → the copy appears. On the **copy**: a shard, a splinter and Shatter are all refused ("Mirrored items can't be changed").

#### E. "Don't warn me again" (about 3 minutes)
1. [ ] In the shard's box, tick **Don't warn me again** and press **No** → next shard click still asks (the tick only counts with Yes).
2. [ ] Tick it and press **Yes** → from now on a shard corrupts with one click, no box. **Shatter still asks** (its tickbox is separate).
3. [ ] In the Shatter box, tick it and press Yes → Shatter now works with one press.
4. [ ] Quit to the menu and come back → both are still off.
5. [ ] To get the warnings back: BepInEx config `com.krogongames.corruption.cfg` (or the in-game config manager) → section `10 - Riftforge` → set `WarnBeforeCorrupting` and `WarnBeforeShattering` to `true`.

#### F. Tooltips stay short (1 minute)
1. [ ] Hover an item with a movement speed, resistance, attack speed, pickup or life steal affix → no extra "Cap …" line (you asked for that; caps come with the stats sheet, issue #60).

#### G. Reconnect and storage (about 4 minutes)
1. [ ] With some splinters in Splinters, one corrupted item and one twin-completed item: log out to the menu and back in → everything is as it was (same numbers, red corrupted line, Corrupted mark, splinters in their tab).
2. [ ] Move two splinters into a chest and back → same affix and numbers.
3. [ ] Drop a splinter on the ground and pick it up → it goes back to Splinters with the same numbers.
4. [ ] Die with splinters → they're in the gravestone and come back.

#### H. Look out for
- [ ] No red errors in the log (BepInEx console) during any of this.
- [ ] Anything that reads as confusing: wording of the boxes, the result lines, the cap lines.

### Your feedback
(none yet)
