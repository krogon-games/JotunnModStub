# Play-test: `claude/brave-einstein-xz357h`

## Round: shard corruption, shattering and Affix Splinters at the Riftforge (#29) (2026-11-10)

**Build:** the Corruption DLL built from this branch (the log's build stamp at start-up). Corruption still says 0.6.0: the coordinator raises it at merge.
**Setup:** single player, your test world, `devcommands` on, a Riftforge built. EquipmentAndQuickSlots and AdventureBackpacks loaded as usual. About 30 minutes.
**Splinter looks:** your approved designs, a shape per kind and a colour per stat (all 67: `docs/images/splinters_preview.png`).

**Console commands you'll use** (F5):
- `co_craft give` → 5 of every orb **and 5 Corruption shards** in Currencies.
- `co_loot rare` / `co_loot magic` → a Rare / Magic item at your feet (repeat for more).
- `spawn SwordBronze` → a Normal item.

### Checklist

#### A. Shattering (about 8 minutes)
1. [ ] Get 3–4 Rare and Magic items (`co_loot rare`, `co_loot magic`). Open the Riftforge → **Forge** tab. Drag a Rare item onto the forge slot → the **Shatter** button is lit.
2. [ ] Press **Shatter** → the **Shatter item** dialog (compare `docs/images/forge_dialog_preview.png`): the game's normal wood panel, nothing of the inventory showing through the text: title centred; the item's icon and coloured name; "Cost: Free"; "This permanently destroys the item." once, in red-orange; **Potential splinters** with each affix on the left (bullets, a green ◆ line if resonant, wrapped lines indented) and **50%** on the right in its own column; two small grey lines of explanation; a **Don't warn me again** tickbox on its own line; **Cancel** left and **Shatter** right, clear of the text. Nothing clipped or overlapping. Press **Cancel** (or Escape) → nothing changed, the item is still in the slot.
   - [ ] Try it with an item with a long affix (a parry or kill effect) and with a 4-affix Rare: still nothing clipped, the 50% column still lined up.
   - [ ] If you play at a large UI scale: the whole dialog fits on screen.
3. [ ] Press **Shatter**, then **Shatter** in the dialog → the item is gone, the slot is empty, and the result says "Shattered …: N splinters (…)" or "… no splinters recovered". The inventory stays on the tab you were on.
4. [ ] Open the **Splinters** tab → one splinter per line the message listed, each with its own shape and colour (e.g. fire damage an orange spiky lump, max health a red drop; compare with `splinters_preview.png`). Drop one on the ground: same model, faintly glowing. Hover one → "Affix Splinter", the affix with its number and range, "Prefix" or "Suffix", and "Fits: …". The numbers match what the item had.
5. [ ] Shatter two more items. Splinters never stack, even identical ones.
6. [ ] Put a **Normal** item in the slot → Shatter is grey; its tooltip says "Only Magic and Rare items shatter".
7. [ ] If you get an item with a green ◆ resonant line: shatter it until that line survives → **one** splinter holding the combined number, marked resonant, "needs an item with no affixes".

#### B. Full storage (about 5 minutes)
Splinters go to the Splinters tab first, then the Bag. The refusal needs **both** full.
1. [ ] Fill the **Splinters** tab (32 cells): keep doing `co_loot rare` and shattering. (Long-winded; if you'd rather skip this section, say so and I'll add a test command that fills the tab.)
2. [ ] Fill the **Bag** with junk so no cell is free, keeping one Rare item with 3 or 4 affixes in it. Put that item in the forge slot.
3. [ ] Press **Shatter** → no dialog; the result says **"Make room for up to X splinters."** (X = the item's number of affix lines). The item is untouched.
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
1. [ ] Put a Magic or Rare item in the slot. In **Currencies**, right-click the **Corruption shard**, click the slot → the **Corrupt item** dialog, in the same design as Shatter's: the item's icon and name, "Cost: 1 Corruption shard (you have N)", "This can permanently destroy the item.", **Possible outcomes** with **25% / 25% / 50%** in the right column, two grey lines saying a surviving item is locked for good except for the Mirror, the **Don't warn me again** tickbox on its own line (not on top of any text), **Cancel** and **Corrupt**.
2. [ ] Press **Cancel** → nothing changed; you still have N shards.
3. [ ] Do it again and press **Corrupt**. Repeat on several items until you have seen all three results. Each time exactly **one** shard is used.
   - [ ] **Destroyed:** the item is gone, the slot empties, the result says "Destroyed: the shard consumed …".
   - [ ] **Unchanged:** the result says **"Corrupted and locked; no affix added."** (not "no change"); the item shows a red centred **Corrupted**.
   - [ ] **Corrupted affix:** one extra **red** line ending in "(corrupted)", highlighted for a few seconds; the item shows **Corrupted**. A full Rare has 5 lines.
   - [ ] (Rare luck, skip if not seen) If the corrupted affix is the other half of a twin the item has, the two merge into one resonant line worth **more** than the item's half: the two added, plus a quarter.
4. [ ] On a corrupted item: an orb → "Corrupted items can't be changed"; a shard → "Already corrupted"; a splinter → refused; Shatter is grey ("Uniques and corrupted items can't be shattered").
5. [ ] `spawn SwordBronze`, corrupt it until it gets an affix → it stays a normal (white) item with one red corrupted line.
6. [ ] **Mirror:** use a Mirror of Kalandra on a corrupted item → the copy appears. On the **copy**: a shard, a splinter and Shatter are all refused ("Mirrored items can't be changed").

#### E. "Don't warn me again" (about 3 minutes)
1. [ ] In the Corrupt dialog, tick **Don't warn me again** and press **Cancel** → next shard click still asks (the tick only counts when you confirm).
2. [ ] Tick it and press **Corrupt** → from now on a shard corrupts with one click, no dialog. **Shatter still asks** (its tickbox is separate).
3. [ ] In the Shatter dialog, tick it and press **Shatter** → Shatter now works with one press.
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

### Your feedback (2026-11-10, first try, with a screenshot of the Splinters tab)
- Shattering: "shattered quite a few items successfully".
- Splinters on a white item: the first (a prefix) worked and turned it Magic; a second prefix was refused; a suffix for another item type was refused; a fitting suffix worked (a 2-affix Magic item). "All intended and working well."
- **Problem: "All the splinters look identical"**: every splinter in the tab is the same red gem (the base item's own icon), not the approved shapes and colours.

### Your feedback (2026-11-10, second try: four screenshots of the two confirmations)
- Shatter dialog: the layout is there (icon, name, Cost: Free, warning, two columns, explanation, Cancel / Shatter) but **it has no background panel**: the text floats over the inventory and is hard to read.
- Shard box: still the game's Yes/No box; **the tickbox overlaps the last line of text**. "The shard box needs to be consistent in design as well."
- "I still want a tickbox on both panels … they need to have an option to hide the panel once they get used to the mechanics."
- (The splinter-look fix isn't reported on yet.)

### Your feedback (2026-11-10, third try: screenshots of both dialogs)
- **"The splinters now show their own shapes and colours."** (the missing-files fix worked)
- Both dialogs: layout, columns, tickbox row and buttons are in place, but **"why is there a troll in the background of the panel?"** (a loading-screen picture behind the text). "Please use the normal background like the rest of the UI. Same with the shard panel."
- Seen in the screenshots: a stray column of text, "MISSING BUTTON DEF "ButtonB"" / "ButtonA", running down from each button.

### Your feedback (2026-11-11, fourth try, with the log)
- Both dialogs: "Looking a lot better now. well done."
- "Noticed something going on in the log": a block of yellow warnings, "The LiberationSans SDF Font Asset was not found. There is no Font Asset assigned to Name / Cost / Warning / …", every time a dialog opens.
- The log also confirms the splinters: "67 looks in the table, 68 items registered, 67 with their own model, 68 icons rendered". No red errors from the mods.

### What changed after the fourth feedback
- The warnings were harmless noise: each new piece of text briefly asked for the engine's default font, which Valheim doesn't ship, before the mod gave it the game's font. Text is now created switched off, given its font, then switched on, so nothing is logged (the dialogs, the Forge bench and the picked-material counter). To re-test: open both dialogs and check the log has no "LiberationSans" lines.

### What changed after the third feedback
- Background: the dialog no longer guesses at the message box's art; it copies the inventory's own panel background (the wood panel the character sheet uses, which you've seen in game). To re-test.
- Stray text: it was the gamepad key hint copied with the message box's buttons; hidden. To re-test.

### What changed after the second feedback
- Both confirmations are now one dialog design (decisions 25–26): drawn with the game's message-box panel, heading, body font and buttons, over a plain dark backing so text can't float over the inventory; the shard's "Corrupt item" lists its three outcomes with their chances in the right column; each has its tickbox on a row of its own. Previews: `docs/images/forge_dialog_preview.png`. To re-test.

### What changed after the feedback
- Splinter looks, second report (screenshot of splinters on the ground: small gems, the glows in different colours): **found.** The nine shape files (`Corruption/Assets/Splinters/*.obj`) never reached GitHub: the repository's ignore list skips `.obj` files, with exceptions only for the shard's and the orbs' folders. So Krogon's build had the colours and glows but no shapes, and each splinter fell back to a tinted gem. Fixed: exception added to `.gitignore`, the nine files committed. A start-up log line now reports how many looks, models and icons loaded. To re-test.

### Final play-test (2026-11-11)
- Krogon: the three shard outcomes and their result messages, the tickboxes hiding each dialog and staying hidden after a restart, the full-storage refusal and reconnecting "have all been checked, all working. well done."
- Passed alone. Not re-checked: that the log no longer shows the "LiberationSans" warnings (the fix came after the last log). Not tried: anything with other players (`PLAYTEST.md` → Session 2, 2i).
