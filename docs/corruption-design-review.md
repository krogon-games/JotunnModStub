# Corruption mod design: technical review

Review of [`corruption-design.md`](corruption-design.md), 2026-10-15.

**How it was checked:**
- **Verified:** checked against the Valheim 1.0 reference assemblies (signatures) and the decompiled 0.221 game code.
- **From game knowledge:** not checked against code; to confirm in-game with `co_dump`.
- Mod availability is from the Thunderstore listing of 2026-09-29.

The design fits Valheim well. Reusing pickup unlocks, boss gating, status effects and prefab cloning is the right call. The points below are where the game works differently from what the doc assumes, plus a few places where the existing code makes things easier.

## Needs a design change

### 1. Rip waves can't run on the vanilla raid system (verified)
- The raid system runs **one event at a time for the whole world**: `RandEventSystem` keeps a single `m_randomEvent` / `m_activeEvent`.
- A rip that starts a raid would cancel, or be blocked by, a base raid elsewhere, or another player's rip.
- **Suggestion:** a small wave spawner of our own, driven by the rip's heart object. It can still reuse the raid's on-screen message style and the biome spawn lists.

### 2. "Server-authoritative spawning" doesn't match how Valheim runs (verified)
- A dedicated server doesn't load the world or run creatures. The player's game that owns an area does both. This is the same mechanism behind the co-op lag in `coop-networking.md`.
- Creatures spawned "by the server" would still be simulated by a player's game. Status effects such as maddened run on that game too.
- **Suggestion:**
  - The server *decides*: rip ID, landing spot, tier, affixes, timers, loot rolls, scars and terror-zone state.
  - The activating player's game *spawns and simulates*, as vanilla raids already do.
  - This also keeps rips in line with whichever networking mod the group uses.

### 3. "A boss trophy of any kind" can't be one bench ingredient (verified) — resolved

**Resolved 2026-10-15:** the bench recipe now uses Eikthyr's trophy, plus Bronze nails, Leather scraps and Surtling cores (see the design doc).

- Vanilla only has an all-or-nothing switch: `Piece.m_requireOnlyOneIngredient` makes *every* listed ingredient an alternative. With it on, fine wood alone would build the bench.
- **Options:**
  - name one trophy (Eikthyr's is the natural one, since the shard loop starts after the first boss); or
  - patch the requirement check for this one piece.

### 4. Corrupted offerings need an altar patch, and not every boss has an altar
- **Verified:**
  - Each altar (`OfferingBowl`) accepts exactly one item type (`m_bossItem`, plus a count). Accepting the corrupted version needs a patch on `OfferingBowl.UseItem` / `Interact`.
  - The same altar code also handles item-stand altars (`m_useItemStands`), so one patch covers bosses summoned by placing items on stands, as well as the bowls.
- **From game knowledge:** the Queen isn't summoned. She waits in her dungeon behind a door opened with a Sealbreaker. A corrupted Queen needs a different trigger, such as a corrupted Sealbreaker or a corrupted spawn of the existing Queen.

### 5. 3-star creatures need HUD work (verified)
- The enemy health bar only has star icons for 1 and 2 stars (`EnemyHud` has `m_level2`, `m_level3`).
- A 3-star creature works in code (`Character.SetLevel(4)`), but it shows as 2-star without a UI patch.
- Small, but it belongs in phase 5's scope.

## Simplifications the code allows

### 6. Validate landing spots before teleporting (verified)
- World generation is deterministic and available on every client without loading the area: `WorldGenerator.GetBiome(x, z)` and `GetHeight(x, z)`.
- Wrong biome, ocean and too-steep spots can be rejected **before** anyone teleports. Only objects at the spot (locations, rocks) need checking after the area loads.
- The explored check also works without loading (`Minimap.IsExplored(position)`).
- The **base-distance check belongs on the server**. A client only has nearby objects loaded, but the server holds every ward and bed in the world.
- **Flow:**
  1. The client proposes unexplored spots in the right biome.
  2. The server rejects ones near bases and picks one.

### 7. Talismans can use a vanilla slot in v1 (verified)
- 1.0 already has a **Utility** slot (Megingjord) and a **Trinket** slot (`Humanoid.m_utilityItem`, `m_trinketItem`).
- Making talismans Trinkets means phase 2 needs no slot code at all, just the AI patch.
- The trade-off is that a talisman then competes with vanilla trinkets.
- A dedicated slot can come later with the inventory work, or from an inventory mod (see open questions).

### 8. Reuse the biome resolver we already have
CraftingOverhaul already works out each item's earliest biome from game data. It can answer three of the doc's needs directly:
- the open point on Mistlands and Ashlands map materials ("pick whichever drops earliest in that biome"): `co_dump`'s `biome_items.csv` lists them;
- each unique's item level and the "map rewards scale to the map's biome" rule;
- the rarity floor for corrupted gear by biome.

### 9. Talisman families need a data table
- The game has no "creature family" data, and each variant has its own trophy (greydwarf, brute and shaman trophies are different items).
- A small config table, family → creatures and accepted trophies, covers it and lets modded creatures be added without code.

## To confirm in-game with `co_dump`

- **Deep North:** the 1.0 code has new dungeon themes (`NorthVillage`, `MorkHalla`), so Deep North content probably shipped. A dump would settle the Fader row: its creatures, materials and any boss.
- **Altars and the Queen:** `biome_sources.csv` shows how each boss is reached (an altar entry vs a creature spawner in a dungeon room), confirming point 4.
- **Exact prefab names** for trophies, essences' summon lists and talisman families.

## Open questions: current options

| Question | Existing mods (Thunderstore, 2026-09-29) | Suggestion |
|---|---|---|
| Corrupted gear: build or depend on Epic Loot? | RandyKnapp-EpicLoot 0.14.13, updated 2026-09-24, ~2.26M downloads | Build our own. The doc's rules (PoE tiers, orbs, corruption lock, brick chance, biome-gated uniques) differ from Epic Loot's own rarity ladder and enchanting, so depending on it means adopting its design. **Decided 2026-10-27: build our own.** |
| Inventory slots and tabs: build or depend? | RandyKnapp-EquipmentAndQuickSlots 3.1.3 (~1.52M), shudnal-ExtraSlots 1.2.14 (~1.13M), Vapok-AdventureBackpacks 2.2.2 (~0.99M); all updated September 2026 | Neither for v1: use the vanilla Utility/Trinket slots (point 7). When the Maps and Currency tabs are built, test against ExtraSlots and EquipmentAndQuickSlots, since many players will already run one. |
| Skill rework vs EpicMMOSystem | WackyMole-WackyEpicMMOSystem 1.9.67 (~591k), updated 2026-09-13 (the maintained EpicMMOSystem) | Compare against this one when the time comes, as the doc says. |

## Packaging: a separate plugin, same repository

- **Two plugins.** Build this as a second plugin next to CraftingOverhaul, sharing the solution, build setup and `co_dump` tooling.
- **Why separate:** they have different multiplayer requirements.
  - CraftingOverhaul is quality-of-life and works when only some players have it.
  - The corruption mod adds items and creatures, so the server and every player must run it.
- **Consoles:** players on console can't install mods, so they couldn't join a server running it. Worth knowing if anyone in the group plays on console through crossplay.

## Suggested first step

1. Run `co_dump` once in-game. It gives the real trophy, creature, altar and Deep North data for the essence, talisman and summon tables.
2. Then build phases 1–2 as the doc orders them (shard and bench, then talismans), using the Trinket slot from point 7. That first build exercises items, recipes, pieces, drop hooks and AI patching before any of the riskier rip work.
