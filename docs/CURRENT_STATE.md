# Current state

**Last updated:** 2026-10-29 (character sheet tested and merged into `master`) · **Branch:** `claude/ui-overhaul` (UI work; other sessions on their own branches, see `CLAUDE.md`) · **Game:** Valheim 1.0.20

Snapshot for picking up work in a new session. Per-feature detail (config, known issues, patch targets) is in [`FEATURES.md`](../FEATURES.md); reasons in [`DECISIONS.md`](DECISIONS.md).

## Build health (checked 2026-10-28)

- Both plugins compile against Valheim reference assemblies (cloud session, stand-in install from NuGet, `ExecutePrebuild=false`).
- `dotnet test CraftingOverhaul.Tests`: **129 passed, 0 failed** (2026-10-29) (biome solver, shard rules, talisman families and slots, affix, cap, drop, corruption outcome, shattering, essence, map and rip loot tables, rip rules, heart placement and army raids, gem model parser, character sheet layout and JSON reader).
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
| Talismans: two charms plus a vanilla trinket, same charm refused twice, provoking for ~30 s, bosses unaffected | Corruption | 2026-10-28 play-test log |
| Boss essences: all eight looks, icons, glow and add-ons; 1 per kill on every kill; Kall's from the last phase | Corruption | 2026-10-28 play-test log |
| Maps and rips step A: opening, landing beside the heart, heart → exit rip home, Plains map, map recipe at the Riftforge | Corruption | 2026-10-28 play-test log (two bugs found, fixed and confirmed) |
| Corrupted bosses core: red tint, harder hits, slower kill, 2–3 essences and 3–5 shards | Corruption | 2026-10-28 play-test log |
| Craft from chests with AdventureBackpacks: backpack alone, backpack + chest, no double charge | CraftingOverhaul | 2026-10-28 play-test log |
| Category headers line up; config reload on save | CraftingOverhaul, both | 2026-10-28 play-test log |
| Character sheet: EquipmentAndQuickSlots' slots in a WoW-style sheet with labelled tiles, silhouettes, live character picture (drag to turn), parchment stats, close button, drag to move, fits with a chest and the backpack open | CraftingOverhaul | 2026-10-29, three play-tests with screenshots ("looking much better"), then the remaining checks |
| `co_dump` with the seven new files | CraftingOverhaul | 2026-10-28 dump, now in `data/game-dump-1.0.20/` |

### Built, not verified

Step-by-step session for all of these: [`PLAYTEST.md`](PLAYTEST.md).

| Item | What to check |
|---|---|
| Rip heart search and corrupted wishbone (2026-10-29) | Passed in single player 2026-10-29; ping colour changed to mostly red (re-check). Still to test: a second player sees the pings |
| Rip waves pacing (2026-10-29) | Passed 2026-10-29: busier waves feel right; maddened creatures fight each other |
| PlantEasily bulk planting with seeds from a chest | `docs/mod-compatibility.md` → How to test, step 3 |
| Character sheet in multiplayer: other players never appear in the picture | `PLAYTEST.md` → Session 2, step 5 (everything else verified 2026-10-29) |
| Anything in multiplayer | `PLAYTEST.md` → Session 2: craft from chests (`docs/multiplayer-test.md`), talismans, rips, essences |

## Not built (proposed)

Phase 5, phase 6 offerings and summons, phases 7–10 ( map affixes, corrupted bosses, loot/affixes/orbs/uniques, scars and world events, power wheel and inventory, skill rework), storage and quests, combat mechanics. Status per system: [`GAME_DESIGN.md`](GAME_DESIGN.md#tracks-and-status). Affixes ([`AFFIX_RULES.md`](AFFIX_RULES.md)): gathering tool pools specified by the user, general gear pool still a draft; both are built with the loot system (phase 7).

## Outstanding bugs and open issues

- Corruption's shard and essences have no biome in recipe grouping: they come from our drop patches, which the biome detection doesn't see. Harmless today (map recipes still group by their biome material).
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

0. **Corrupted offerings** (phase 6): icons for all eight made from the user's renders (`tools/make_offering_icons.py`); next, build the items and altar logic. Map icons merged (PR #5). **Rip waves at least 1 star** (`MinStars`, 2026-10-29): tested, "definitely more challenging"; PR into `master`.
1. **Rips step B and the heart search passed in single player (2026-10-29)**; merged into `master` (PR #4). Ping colour re-checked (more red, stops when the heart dies); multiplayer tests with the group. Still open from the design: checking the landing area once loaded (locations, water).
2. **Gear affix ranges** for the remaining families, with the user's rule, from `status_effects.csv` and `item_details.csv`.
3. **Corrupted offerings** (phase 6) from `altars.csv`, and the corrupted Sealbreaker.
4. Play-test the rest of `PLAYTEST.md` (PlantEasily, Session 2 with a friend).
5. Then open a PR into `master` (user, after the play-test).

## UI overhaul (branch `claude/ui-overhaul`, started 2026-10-29)

The user wants the inventory screen to look like an RPG character screen (concept image, 2026-10-29). Done and to do, in order:

1. **Character sheet**: built; first play-test 2026-10-29 worked (slots, stats) but looked too plain. Reworked the same day: generated art (`tools/make_sheet_ui.py`, mock-up `docs/images/sheet_preview.png`), labelled tiles with silhouettes, and the **live character picture** (brought forward from step 2). Decisions in `DECISIONS.md` (2026-10-29): keep EquipmentAndQuickSlots, no hand or feet slots, no tabs. Second play-test (2026-10-29): the look matches the mock-up and the live picture works (turning it by dragging works); the feet stood above a thin stone and a talisman kept the blue backing, fixed the same day. Third play-test: "looking much better" (user). **Merged into `master` 2026-10-29.**
2. **Revisit later (user, 2026-10-29): make the sheet's look consistent with the rest of the game's UI.** Its darker wood, iron frame and parchment stand out next to the vanilla panels (inventory grid, crafting). Not now; ask the user what to change when picking it up. Tweak the numbers in `tools/make_sheet_ui.py` and `SheetLayout.cs`, rerun the script, and check the mock-up before asking for a test.
3. **Currency and Maps tabs** off the inventory (design phase 9; the concept image's icon tabs). Separate feature; Affix Splinters depend on it.
4. The recipe list redesign and the crafting screen with AdventureBackpacks' backpack panel open (user's to-do, 2026-10-28): ask what they want changed before starting.

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
