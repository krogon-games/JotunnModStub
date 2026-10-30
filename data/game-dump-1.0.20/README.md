# Game data dump: Valheim 1.0.20

Written by `co_dump` on 2026-10-28 at 22:52 (English), with Jötunn, Corruption, CraftingOverhaul, EquipmentAndQuickSlots and AdventureBackpacks loaded, so modded items (backpacks, our shard, essences and maps) appear too. Earlier version: 2026-10-26, vanilla plus Jötunn (git history). Reference data for designing and checking mod features, e.g. the corruption mod's trophy, creature and material tables.

- `biome_sources.csv` (4 MB, every raw biome source) is left out; re-run `co_dump` for it.
- Biome columns: `Wisp` resolves to Plains (the Wisp Fountain needs Yagluth's drop and has no biome check in the code) and `FaderEmber` to the Ashlands (Eternal Pyre). The only unplaced recipe ingredients are Corruption's own shard and essences, which come from our drop patches the resolver doesn't see.
- `item_details.csv` and `status_effects.csv` re-dumped 2026-10-30 with resistances: item `m_damageModifiers` and status effect `m_mods` (e.g. `Frost:Resistant`).
- New in this dump: `item_details.csv`, `spawns.csv`, `events.csv`, `event_spawns.csv`, `status_effects.csv`, `containers.csv`, `destructibles.csv`, `altars.csv` (see `FEATURES.md` → Game data dump).
- `biome_items.csv` also lists creature attack items (e.g. `troll_punch`). They count as tools for tool-tier checks; they aren't obtainable items.
- This is Iron Gate's game data: fine in this private repository for development, but never ship it with a mod.
