# Game data dump: Valheim 1.0.21

Written by `co_dump` on 2026-11-18 at 21:22 (English), with Jötunn, Corruption 0.8.0, CraftingOverhaul 0.5.0, Orlog 0.8.0, EquipmentAndQuickSlots and AdventureBackpacks loaded, so modded items (backpacks, our shard, essences, maps, orbs, splinters and corrupted offerings) appear too ([#42](https://github.com/krogon-games/JotunnModStub/issues/42)). It replaces the 1.0.20 dump of 2026-10-28 (git history has it, and the 2026-10-26 one before it). Reference data for designing and checking mod features, e.g. the corruption mod's trophy, creature and material tables.

- `biome_sources.csv` (4 MB, every raw biome source) is left out; re-run `co_dump` for it.
- Biome columns: `Wisp` resolves to Plains (the Wisp Fountain needs Yagluth's drop and has no biome check in the code) and `FaderEmber` to the Ashlands (Eternal Pyre). The only unplaced recipe ingredients are Corruption's own shard and essences, which come from our drop patches the resolver doesn't see, and Malicious Blood (`HatefulBlood`, below).
- `biome_items.csv` also lists creature attack items (e.g. `troll_punch`). They count as tools for tool-tier checks; they aren't obtainable items.
- This is Iron Gate's game data: fine in this private repository for development, but never ship it with a mod.

## What changed from 1.0.20 (compared 2026-11-18)

Identical: `creatures.csv`, `spawns.csv`, `events.csv`, `event_spawns.csv`, `containers.csv`, `locations.csv`, `altars.csv`, `pieces.csv`. Every vanilla item's numbers in `item_details.csv` are the same. Re-running `tools/make_loot_bases.py` and `tools/make_orlog_tables.py` on this dump gives the same tables, and the affix ranges, drop tables, rip waves and loot, and altar offerings taken from the 1.0.20 dump still hold.

Game changes seen in the data:
- **Three cauldron foods moved to the food preparation table:** uncooked Baked Poteitr (was cauldron level 7), Kale Chips and Oven Pancake (were cauldron level 6), now prep table level 1.
- **Mage hood upgrade** (`Recipe_HelmetGoldMage`): the moose trophy no longer goes up by one per quality level (always 1).
- **Snow troll attacks** (`trollsnow_punch`, `_r`): tool tier 6 → 4. Bloodgold ore (`GoldOre`, "Petrified Tissue") still resolves to the Deep North, now through the snow troll's remains (a tier-6 destructible of any tool kind, reached by the Berserkir axes and the Deep North axes and crossbows) instead of the tier-6 rocks, which no pickaxe reaches.
- **Four new destructibles:** `WallSnow1`–`WallSnow4` (100 health, tier 0, no drops).

Ours or other mods', not the game's:
- Cape, Hood and Mead Horn of Oden show `m_dlc` empty: Corruption's `BetaItems` unlocks them (added after the 1.0.20 dump).
- New items, recipes and status effects: Corruption's orbs, Affix Splinters, corrupted offerings and affix effects; Orlog's status effects; AdventureBackpacks now lists its Plains backpack's effect instead of the Meadows one.
- `HatefulBlood` (Malicious Blood, Kall's offering) is now listed as unplaced because Corruption's corrupted malicious blood recipe uses it. The game data shows no source for it in either version (no drop, chest or destructible).
- 14 items' `origin` in `biome_items.csv` names a different winning source; none changed biome.
