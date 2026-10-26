# Game data dump: Valheim 1.0.20

Written by `co_dump` on 2026-10-26 at 23:27 (vanilla plus Jötunn, English). Reference data for designing and checking mod features, e.g. the corruption mod's trophy, creature and material tables.

- `biome_sources.csv` (4 MB, every raw biome source) is left out; re-run `co_dump` for it.
- The biome columns come from the resolver after the 2026-10-26 fixes: 913 items placed, 2 recipe ingredients unplaced (`Wisp`, fixed in the next build, and `FaderEmber`, which has no source in the data).
- `biome_items.csv` also lists creature attack items (e.g. `troll_punch`). They count as tools for tool-tier checks; they aren't obtainable items.
- This is Iron Gate's game data: fine in this private repository for development, but never ship it with a mod.
