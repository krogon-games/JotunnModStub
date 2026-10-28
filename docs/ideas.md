# Mod ideas backlog

Ideas saved for later exploration. Nothing here is planned or built yet. When an idea is picked up, it moves to `FEATURES.md` as **Planned**.

**Update 2026-10-15:** all of these ideas are now worked out in the Corruption mod design, [`corruption-design.md`](corruption-design.md), with a technical review in [`corruption-design-review.md`](corruption-design-review.md). Each idea below links to its section there.

## Inventory and equipment

- **Backpack**, World of Warcraft style: extra bag storage you carry with you. → [Inventory and equipment slots](corruption-design.md#inventory-and-equipment-slots). *Covered by the AdventureBackpacks mod (2026-10-28).*
- **Equipment slots and inventory tabs** → [Inventory and equipment slots](corruption-design.md#inventory-and-equipment-slots):
  - dedicated slots for gear instead of keeping everything in the main grid;
  - tabs to organise the inventory.

## Powers

- **Power wheel**: a radial menu for picking active powers. → [Wheel of powers](corruption-design.md#wheel-of-powers)
  - Includes powers granted by items, e.g. a belt of strength giving unlimited carry weight.

## Loot and endgame

- **Corruption shards**: an item that corrupts other items. → [Corruption shards](corruption-design.md#corruption-shards)
- **Custom dungeons**: like Diablo IV's dungeons and Diablo II's maps. → [Rips](corruption-design.md#rips)
- **Custom crafting bench** for making those dungeon maps and corrupted items. → [Map bench and map recipes](corruption-design.md#map-bench-and-map-recipes)

## Progression

- **Skill system redesign**: instead of a skill levelling up automatically as you use it, you earn levels and stat points and choose where to spend them. → [Skill rework](corruption-design.md#skill-rework)

## Creatures

- **Anti-aggro talismans** → [Talismans](corruption-design.md#talismans):
  - Crafted at a workbench from a specific monster's trophies.
  - While equipped, that monster is repelled.
- **Open questions** (answered in the design: two talismans plus a vanilla trinket (changed 2026-10-28); rings get their own slot and raise skills; Norse names throughout):
  - Allow two talismans equipped at once?
  - Should magic rings work the same way, or be a separate item type?
  - Theme talismans and rings on Viking lore?

## Storage and quests (added 2026-10-27)

Full brief: [`storage-quests-design.md`](storage-quests-design.md). Build in this order:

- **Chest tiers add slots:** bigger chests per tier (e.g. 4×4, 6×6, 8×8), no change to stack sizes.
- **Super chest** (unlocked by a quest): deposits sort themselves into nearby chests that already hold that item.
- **Mistlands bank and vault** (a Dvergr banker recruited by quest): one shared virtual store whose capacity is the base's total chest slots.
