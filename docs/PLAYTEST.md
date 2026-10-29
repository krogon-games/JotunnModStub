# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** rebuild both mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1c: solo (2 minutes)

### 1. Bulk planting from chests
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Session 1d: character sheet (5 minutes, branch `claude/ui-overhaul`)

Needs the CraftingOverhaul build from that branch and EquipmentAndQuickSlots.

1. Open the inventory (Tab). Instead of EAQS's panel there should be a wood panel titled **Character** right of the inventory grid: Head, Chest, Legs, Shoulders, Utility down the left; Trinket, Talisman, Talisman down the right; a dark frame in the middle; a stats box; your 3 quick slots in a row at the bottom. The log says `Character sheet created with N slots`.
2. **Does it fit?** Note whether it overlaps the crafting panel on the right or runs off the bottom of the screen. Drag it by its wood to a good place; tell Claude roughly where it ended up (the saved value is `Position` in `com.krogongames.craftingoverhaul.cfg`), so the default can be fixed.
3. Drag a helmet from the bag into Head: it equips. Drag it back out. Right-click a chest piece in the bag: it goes into Chest. Hover a slot: the tooltip shows.
4. Put a charm in a Talisman slot: works as before. Press a quick-slot hotkey: it still uses the item.
5. Stats box: armor and weight match the numbers vanilla shows on the inventory panel; health/stamina change when you eat; "Rested" appears in Effects after sleeping; put on a Troll armour piece or a heavy cape: speed from gear goes negative.
6. Open a chest and the AdventureBackpacks backpack: the sheet shouldn't overlap either panel.
7. Set `Enabled = false` in the cfg and save (config reload): EAQS's old panel comes back; set it to true again: the sheet returns.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
