# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** rebuild both mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1c: solo (2 minutes)

### 1. Bulk planting from chests
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Session 1d: character sheet (5 minutes, branch `claude/ui-overhaul`)

Needs the CraftingOverhaul build from that branch and EquipmentAndQuickSlots. First test (2026-10-29): slots and stats worked; it looked too plain. This is the reworked version; compare it with `docs/images/sheet_preview.png`.

0. **If you tested the first version:** your config still has the old position. Open `BepInEx/config/com.krogongames.craftingoverhaul.cfg`, delete the `Position = ...` line under `[3 - Character Sheet]` (or just drag the sheet later).
1. Open the inventory (Tab). Right of the inventory grid: an iron-framed dark wood panel titled **CHARACTER**, framed slot tiles labelled HEAD, CHEST, LEGS, CAPE, UTILITY on the left and TRINKET, TALISMAN, TALISMAN on the right, **your character standing on a stone in the arch** in the middle, a parchment with your name on a ribbon, your quick slots at the bottom. It should no longer cover the inventory's weight number.
2. **The picture:** is the whole character in view, feet on the stone? Drag the picture sideways: it turns. Change your helmet or cape: the picture changes. Check at night and in a dark place: still visible. Other players or creatures nearby shouldn't appear in it. Close the inventory: nothing odd happens to your character in the world.
3. Empty slots show a faint picture of what goes there (helmet, tunic, legs, cape, belt, pendant, trophy); equipped items no longer have a blue background in the sheet.
4. Drag a helmet from the bag into HEAD: it equips. Drag it back out. Right-click a chest piece in the bag: it goes into CHEST. Hover a slot: the tooltip shows. Put a charm in a TALISMAN slot: works as before. A quick-slot hotkey still uses its item.
5. Stats: armor and weight match the inventory's own numbers; "Rested" (with its icon) appears on the right after sleeping; a Troll armour piece makes speed from gear negative (red).
6. The X button closes the inventory. Drag the sheet by its wood: it moves and stays there next time.
7. Open a chest and the AdventureBackpacks backpack: the sheet shouldn't overlap either panel. Does it fit on your screen top to bottom?
8. If the picture misbehaves, set `ShowCharacter = false` and save (config reload) and send the BepInEx log.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
