# Game data dump: Valheim 1.0.31

Written by `co_dump` on 2026-11-27 at 21:48 (English), with Jötunn 2.30.2, Corruption 0.10.0, CraftingOverhaul 0.6.0, Orlog 0.9.0, EquipmentAndQuickSlots 3.1.3 and AdventureBackpacks 2.4.6 loaded, so modded items (backpacks, our shard, essences, maps, orbs, splinters and corrupted offerings) appear too ([#86](https://github.com/krogon-games/JotunnModStub/issues/86); Krogon's decision 16 of the rips round, `claude/festive-gates-79y1yr`). It replaces the 1.0.21 dump of 2026-11-18 (git history has it, and the 1.0.20 and earlier ones before it). Reference data for designing and checking mod features, e.g. the corruption mod's trophy, creature and material tables.

- `biome_sources.csv` (4 MB, every raw biome source) is left out; re-run `co_dump` for it.
- Biome columns: `Wisp` resolves to Plains (the Wisp Fountain needs Yagluth's drop and has no biome check in the code) and `FaderEmber` to the Ashlands (Eternal Pyre). The only unplaced recipe ingredients are Corruption's own shard and essences, which come from our drop patches the resolver doesn't see, and Malicious Blood (`HatefulBlood`).
- `biome_items.csv` also lists creature attack items (e.g. `troll_punch`). They count as tools for tool-tier checks; they aren't obtainable items.
- **No weather data:** the dump doesn't say which weather applies which status effect, or how much damage Smoked deals (`status_effects.csv` has Cold, Freezing, Wet and Smoked with their simple fields only). The weather map affixes need `co_dump` extended first.
- This is Iron Gate's game data: fine in this private repository for development, but never ship it with a mod.

## What changed from 1.0.21 (compared 2026-11-27)

Identical: `creatures.csv`, `events.csv`, `event_spawns.csv`, `altars.csv`, `biome_unresolved.csv`. Re-running `tools/make_orlog_tables.py` on this dump gives the same tables. `tools/make_loot_bases.py` gives three changes, applied (see below).

Game changes seen in the data:
- **Chests** (`containers.csv`, 22 changed): e.g. forest crypt chests hold 3–5 items (were 2–4), Dvergr tower chests 3–4 (were 2–3, now with needle arrows), `Morkhalla_ChestAncient` fewer ancient coins (5–11, were 11–55).
- **Rocks:** `Rock_7_meadows` health 200 → 50, `Rock_7_deepnorth` 200 → 100. Eight item-stand and tankard props are no longer listed as destructibles.
- **Items:** the Blackmetal battleaxes (`BattleaxeGold` and its two variants) chop 40 → 90; linen thread weighs 0.5 (was 2); the upgrade materials' (`Upgrader0`–`3` Armor/Weapon) `m_breakReturnIngreientsAmount` is 0.65 (was 0.35).
- **Recipes** (30 changed, 2 new): 20 need a different upgrade material (e.g. the flint axe `Upgrader0Weapon`, was `Upgrader1Weapon`; the Headdress of the Bear `Upgrader1Armor`, was `Upgrader5Armor`); 8 show a different item name, 3 a different biome category; new recipes for `HelmetRootCrown` and `HelmetSweatBand`.
- **Upgrade materials found in later biomes:** the chests that hold them moved (`Upgrader1` from a Meadows house to Black Forest ruins, `Upgrader2` to the Swamp, `Upgrader3` to the Mountains), so items made with them resolve to later biomes in `biome_items.csv` (9 items).
- **Locations** (`locations.csv`): fewer per world of some, e.g. goblin huts 10 each (were 20–30), Deep North villages 50 (were 135), mountain wells 10 (were 25); new entries `Forestlantern`, `TarPit1_1`–`TarPit3_1`.
- **Spawns:** the Deep North Jotun melee and witch patrols no longer hunt the player (`m_huntPlayer` False).
- **Pieces:** the Grausten chest costs Charred Skulls ×5 (was the Charred warrior trophy); the 67° darkwood beam has its name.

Ours or other mods', not the game's:
- Two new Affix Splinters (`CorruptionAffixSplinter_tool_orb_find_mining`, `_woodcutting`), the renamed maps ("Meadows map", was "Corrupted Meadows map"), the Riftforge's recipe, Corruption's `SE_CorruptionRabid`; AdventureBackpacks' Plains backpack effect is no longer listed.

**`Corruption/Data/loot_bases.csv` regenerated** from this dump (249 items, were 248), because the loot-base test found it no longer matching the game: the Headdress of the Bear Plains → Black Forest and the Bone Tower Shield Meadows → Black Forest (both made with `Upgrader1Armor`, now found in the Black Forest), and the Serpent Scale Shield now Mountain (`Upgrader3Armor`; it counted as Ocean before, which drops no loot).
