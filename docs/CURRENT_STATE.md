# Current state

**Last updated:** 2026-10-27 · **Branch:** `claude/valheim-modding-exploration-izv3l8` · **Game:** Valheim 1.0.20

Snapshot for picking up work in a new session. Per-feature detail (config, known issues, patch targets) is in [`FEATURES.md`](../FEATURES.md); reasons in [`DECISIONS.md`](DECISIONS.md).

## Build health (checked 2026-10-27)

- Both plugins compile against Valheim reference assemblies (cloud session, stand-in install from NuGet, `ExecutePrebuild=false`).
- `dotnet test CraftingOverhaul.Tests`: **71 passed, 0 failed** (biome solver, shard rules, talisman families, affix, cap and drop tables).
- Last play-test: 2026-10-26, single player, by the user.

## Implemented

### Verified in-game (user, single player)

| Feature | Plugin | Evidence |
|---|---|---|
| Recipe search box | CraftingOverhaul | Third play-test |
| Collapsible categories, grouping button (type / biome / none) | CraftingOverhaul | Second and third play-tests |
| Data-driven biome detection | CraftingOverhaul | Second dump: 913 items placed, 2 unplaced recipe ingredients |
| Craft from nearby chests; build from nearby chests | CraftingOverhaul | Third play-test; "building from chests works well" |
| `co_dump`, `co_biome` | CraftingOverhaul | Two dumps received |
| `spawn CorruptionShard` (name, description, weight) | Corruption | 2026-10-26 |
| Riftforge: unlock, build, opens as "Riftforge", red glow, raven lore plays once | Corruption | 2026-10-26 ("fixed, crow works now") |
| No build-menu errors after the 5-ingredient fix | Corruption | 2026-10-26 |

### Built, not verified

| Item | What to check |
|---|---|
| Talismans (whole feature) | Craft a charm, equip, family ignores you; hitting one provokes it for 30 s; other players unaffected; bosses unaffected |
| Shard drops (1-star, 2-star, boss first kill) | Raise `OneStarChance` temporarily to see drops; kill a boss in a fresh world |
| Shard red look (Bloodstone + tint) | Not explicitly confirmed after the change |
| Category header alignment and `HeaderFontSize` | Headers line up with recipe icons, one line, readable size |
| `Wisp` biome after the wisp-fountain rule | `co_biome Wisp`: if Plains, add `Wisp:Mistlands` override |
| Craft-from-chests fixes for AdventureBackpacks (2026-10-28) | With AdventureBackpacks: craft from the backpack only, then backpack + chest, and check nothing is charged twice. Steps: `docs/mod-compatibility.md` → How to test |
| Talismans with EquipmentAndQuickSlots | Wear a talisman in the EAQS trinket slot |
| Anything in multiplayer | `docs/multiplayer-test.md` (11 steps, needs a second player) |

## Not built (proposed)

Corruption phases 3–10 (essences, maps and rips, map affixes, corrupted bosses, loot/affixes/orbs/uniques, scars and world events, power wheel and inventory, skill rework), storage and quests, combat mechanics. Status per system: [`GAME_DESIGN.md`](GAME_DESIGN.md#tracks-and-status). Affixes ([`AFFIX_RULES.md`](AFFIX_RULES.md)): gathering tool pools specified by the user, general gear pool still a draft; both are built with the loot system (phase 7).

## Outstanding bugs and open issues

- `FaderEmber` (Embers) has no source in the game data; needs a `BiomeMaterialOverrides` entry once its in-game source is known.
- Chest race: two players taking from the same chest at the same instant, last write wins (same as Valheim Plus).
- Any-one-ingredient recipes don't use chest contents.
- Talisman families group widely by name (one Blob charm covers every ooze; Troll covers frost trolls).
- Riftforge has no recipes (expected until phase 4); ingredient amounts are first guesses.
- A boss killed before the Corruption mod was installed won't drop first-kill shards.
- Location loading for biome detection may cause a short hitch on spawn.

## Assumptions not yet verified

- Bloodgold ore is mined by Snow Troll attacks (inferred from tool tiers in the dump).
- The Queen has no summoning altar (game knowledge).
- NetworkPerformanceSystem works on a crossplay server and alongside craft-from-chests (reading of its README; untested).
- Location prefabs load safely through soft references outside world generation (worked in the dumps; not stress-tested).

## Waiting on the user

- **Backpacks and affixes:** AdventureBackpacks backpacks are cape-slot items. Should they roll cape affixes?
- **Gathering tool affixes:** confirm the prefix/suffix split, roll weight classes, one combined pool for one-handed axes (gathering + weapon families), and the proposed interpretations ([`AFFIX_RULES.md`](AFFIX_RULES.md#gathering-tool-affixes)).
- **General gear affixes:** review the proposed one-range numbers ([`AFFIX_RULES.md`](AFFIX_RULES.md#general-gear-pool-draft)); twin interpretation still open.
- General gear affix draft: approve; confirm the twin interpretation (prefix + suffix sharing a stat).
- Before phase 4: accept "server decides, owner spawns" and an own wave spawner instead of the raid system.
- Corrupted bosses/gear: red like the shard, or the design doc's purple.
- Rename the Corruption plugin to a Norse name? (offered, undecided)
- Play-test results for everything under "Built, not verified".

## Next actionable tasks

1. Play-test talismans, shard drops and the shard look, with EquipmentAndQuickSlots, PlantEasily and AdventureBackpacks installed (the group's setup); run the compatibility checks in `docs/mod-compatibility.md`. Record results in `FEATURES.md` and here.
2. Run `co_biome Wisp`; add overrides for `Wisp` (if needed) and `FaderEmber` once its source is known.
3. **Phase 3: boss essences.** One new drop per boss (1 on a normal kill), cloned item + red tint, `CharacterDrop` hook like `ShardDrops`, boss list from game data (creatures with a defeat key), pure rule + tests.
4. Run the multiplayer checklist when a second player is available.
5. Settle the open decisions above before phase 4 (maps and rips).

## Update routine

Do this in the same commit whenever a significant feature is completed, changed, deferred or play-tested:

1. `FEATURES.md`: table row, section, dated changelog entry (mandatory, see `CLAUDE.md`).
2. This file: move items between *Verified* / *Built, not verified* / *Not built*; update bugs, assumptions, waiting-on-user and next tasks; bump **Last updated** and the test count.
3. `DECISIONS.md`: add a row for any new decision, rejected approach or correction.
4. `GAME_DESIGN.md` status table (and `AFFIX_RULES.md` or the design doc) if a design changed.
5. `ARCHITECTURE.md` if a new pattern, project, game limit or build step appeared.

Only the user's play-test reports move something to *Verified*. Mark anything inferred as an assumption.
