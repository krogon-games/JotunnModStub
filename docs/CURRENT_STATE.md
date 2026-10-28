# Current state

**Last updated:** 2026-10-28 · **Branch:** `claude/valheim-modding-exploration-izv3l8` · **Game:** Valheim 1.0.20

Snapshot for picking up work in a new session. Per-feature detail (config, known issues, patch targets) is in [`FEATURES.md`](../FEATURES.md); reasons in [`DECISIONS.md`](DECISIONS.md).

## Build health (checked 2026-10-28)

- Both plugins compile against Valheim reference assemblies (cloud session, stand-in install from NuGet, `ExecutePrebuild=false`).
- `dotnet test CraftingOverhaul.Tests`: **94 passed, 0 failed** (biome solver, shard rules, talisman families and slots, affix, cap, drop, corruption outcome, essence and map tables, rip rules, gem model parser).
- Last play-test: 2026-10-28, single player, with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily loaded (talismans working as intended after two charm fixes).

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
| Shard drops from 1-star and 2-star creatures and from a boss's first kill | Corruption | 2026-10-28: forced chances with `spawn Greydwarf`; Eikthyr |
| Shard's own ruby model, icon and red ground glow | Corruption | 2026-10-28 screenshots ("all tested, works") |
| Riftforge: unlock, build, opens as "Riftforge", red glow, raven lore plays once | Corruption | 2026-10-26 ("fixed, crow works now") |
| No build-menu errors after the 5-ingredient fix | Corruption | 2026-10-26 |
| Talismans: recipes, charm in an EquipmentAndQuickSlots Talisman slot, tooltip without trinket bonuses, repel | Corruption | 2026-10-28 with all mods loaded ("working as intended now") |

### Built, not verified

Step-by-step session for all of these: [`PLAYTEST.md`](PLAYTEST.md).

| Item | What to check |
|---|---|
| Talismans: the rest | Two charms at once plus a vanilla trinket; hitting a repelled creature provokes it for 30 s; bosses unaffected; on a friend's game too |
| Category header alignment and `HeaderFontSize` | Headers line up with recipe icons, one line, readable size |
| `Wisp` biome after the wisp-fountain rule | `co_biome Wisp`: if Plains, add `Wisp:Mistlands` override |
| Craft-from-chests fixes for AdventureBackpacks (2026-10-28) | With AdventureBackpacks: craft from the backpack only, then backpack + chest, and check nothing is charged twice. Steps: `docs/mod-compatibility.md` → How to test |
| Boss essences (phase 3, 2026-10-28) | `spawn CorruptionEssence_eikthyr` (and the other seven, including `_kall`): look, icon, glow, add-on parts on the Elder, Yagluth and Kall essences; each boss drops 1 per kill (Kall: from the final phase) |
| Maps and rips, step A (2026-10-28) | `spawn CorruptionMap_Meadows`, right-click it: rip opens, travel, land on the ground beside the heart, destroy the heart, exit rip leads home, both close after a day. Crafting a map at the Riftforge. Then with a friend: following through the rip, rip near someone's base refused. Steps: `FEATURES.md` → Maps and rips |
| Corrupted bosses core (2026-10-28) | `spawn Eikthyr`, `co_corrupt`: red, harder hits, slower to kill, 2–3 essences and 3–5 shards. Steps: `PLAYTEST.md` |
| Anything in multiplayer | `docs/multiplayer-test.md` (11 steps, needs a second player) |

## Not built (proposed)

Corruption phase 4 step B (waves, maddened, rip loot), phase 5, phase 6 offerings and summons, phases 7–10 ( map affixes, corrupted bosses, loot/affixes/orbs/uniques, scars and world events, power wheel and inventory, skill rework), storage and quests, combat mechanics. Status per system: [`GAME_DESIGN.md`](GAME_DESIGN.md#tracks-and-status). Affixes ([`AFFIX_RULES.md`](AFFIX_RULES.md)): gathering tool pools specified by the user, general gear pool still a draft; both are built with the loot system (phase 7).

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

- **Run `co_dump` once in a world (2026-10-28) and send the new files** (`item_details.csv`, `spawns.csv`, `events.csv`, `event_spawns.csv`, `status_effects.csv`, `containers.csv`, `destructibles.csv`, `altars.csv`). Phase 4 step B takes its wave, maddened and loot numbers from them, phase 6 its altar offerings, and the gear affixes their remaining ranges, instead of guessing.

- **General gear affixes:** the range rule is decided (2026-10-28); the remaining ranges are computed from the next `co_dump` ([`AFFIX_RULES.md`](AFFIX_RULES.md#general-gear-pool-draft)).
- Kall Fimbulbringer's essence look (made without a concept render; status `proposed` in `essences.csv`).
- Rename the Corruption plugin to a Norse name? (offered, undecided)
- Play-test results for everything under "Built, not verified".
- **Pull request:** nothing since PR #1 (merged 2026-09-29); `master` lacks everything Corruption. The user wants to revisit this after the first play-test session (2026-10-28): open a PR from the branch into `master` then.

## Next actionable tasks

1. Play-test the rest of talismans (see the table above), with EquipmentAndQuickSlots, PlantEasily and AdventureBackpacks installed (the group's setup); run the compatibility checks in `docs/mod-compatibility.md`. Record results in `FEATURES.md` and here.
2. Run `co_biome Wisp`; add overrides for `Wisp` (if needed) and `FaderEmber` once its source is known.
3. Play-test boss essences (above).
4. Run the multiplayer checklist when a second player is available.
5. Play-test phase 4 step A, then build **step B**: our own wave spawner driven by the heart, maddened status effect, the end-of-rip loot, and checking the landing area once loaded (locations, water).

## Before release

Development aids to switch off or remove before sharing a public release:
- **Config reload on save** (`Shared/ConfigWatcher.cs`, added 2026-10-28): set `ReloadConfigOnFileChange` to default `false`, or remove the watcher from both plugins. Cost is negligible on Windows; on a Linux dedicated server Mono's file watcher may fall back to polling the config folder.
- The `co_dump` / `co_biome` console commands stay (useful for admins), but `data/game-dump-*` must never ship.

## Update routine

Do this in the same commit whenever a significant feature is completed, changed, deferred or play-tested:

0. `PLAYTEST.md`: add steps for anything new and untested; remove what the user has tested.

1. `FEATURES.md`: table row, section, dated changelog entry (mandatory, see `CLAUDE.md`).
2. This file: move items between *Verified* / *Built, not verified* / *Not built*; update bugs, assumptions, waiting-on-user and next tasks; bump **Last updated** and the test count.
3. `DECISIONS.md`: add a row for any new decision, rejected approach or correction.
4. `GAME_DESIGN.md` status table (and `AFFIX_RULES.md` or the design doc) if a design changed.
5. `ARCHITECTURE.md` if a new pattern, project, game limit or build step appeared.

Only the user's play-test reports move something to *Verified*. Mark anything inferred as an assumption.
