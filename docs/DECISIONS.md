# Decisions

Why things are the way they are, newest first within each group. Each entry: the decision, why, what was rejected, and its status.

**Status values:** **Decided** (user agreed or asked for it) · **Implemented** (in code) · **Proposed** (recommended, awaiting the user) · **Superseded**.

Add an entry whenever a choice would be expensive to rediscover: a rejected approach, a game limit that forced a design, or a user preference. Don't rewrite old entries; mark them Superseded and add a new one.

## Process and tooling

| Date | Decision | Why | Rejected | Status |
|---|---|---|---|---|
| 2026-10-27 | Handover docs: `CLAUDE.md` (rules) + `docs/CURRENT_STATE.md`, `DECISIONS.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`, `AFFIX_RULES.md` | Work spans many sessions; context is lost on compaction | Keeping state only in chat transcripts | Decided |
| 2026-10-26 | Build fails if a plugin references `System.ValueTuple`, `System.Memory` or `System.Buffers`; no C# tuples | Valheim's Mono doesn't ship them; tuples broke the inventory UI and `co_dump` in the first play-test | Bundling `System.ValueTuple.dll` | Implemented |
| 2026-10-26 | Each Harmony patch class applied separately; game-method code wrapped in `Safe.Run` (log once, fall back to vanilla) | One bad patch or exception mustn't break the game's UI | `Harmony.PatchAll()` | Implemented |
| 2026-10-26 | Game data dump saved in `data/game-dump-1.0.20/` as test and design reference | Lets tests and design use real prefab names and stats | Hand-written tables | Implemented |
| 2026-09-29 | Pure rule files + xunit tests (`CraftingOverhaul.Tests`, net8.0, files linked in) | Game code can't be loaded outside Valheim; rules can | Testing only in-game | Implemented |
| 2026-09-28 | `FEATURES.md` updated in the same commit as any feature change, with exact Harmony patch targets | User's request; patch lists spot conflicts with Valheim Plus and others | — | Decided |
| 2026-09-27 | BepInEx + Jötunn + Harmony; SDK-style net462 project; publicized assemblies | Jötunn removes item/piece/recipe boilerplate | — | Implemented |

## Architecture

| Date | Decision | Why | Rejected | Status |
|---|---|---|---|---|
| 2026-10-27 | Gathering tools (pickaxes and one-handed woodcutting axes) get their own affix pools: universal, mining, woodcutting. Part of the loot system, not a separate build | User's spec | Two-handed combat axes as gathering tools | Decided by user |
| 2026-10-27 | Magic and Rare drops from monsters start at 1 in 50 and 1 in 100 per kill; uniques and later rarities have separate chances; all chances in a data/config table, multiplied by corruption zones, scars, terror zones and map tiers | Low rates keep finds special; must stay easy to rebalance | Hard-coded rates | Decided by user |
| 2026-10-27 | Odin's Grace is a powerful rare affix: the whole tree (from standing tree, log or stump), or the hit chunk plus every touching chunk | User wants it to be the fun rare roll | Single object or chunk only (earlier proposal) | Decided by user |
| 2026-10-27 | Gathering affix pools favour variety, with low-impact options; prefix/suffix counts needn't be equal | Not every item should be awesome | Equal prefix/suffix split | Decided by user |
| 2026-10-27 | Drop modifiers approved: stars and rip tier 1 raise Magic and Rare by the same multiplier as uniques; the proposed rip tier 2–4, scar and terror zone values. A kill that succeeds on both Magic and Rare drops only the Rare; uniques roll on top | User approved the proposals | Stars affecting uniques only | Decided by user (numbers proposed by Claude) |
| 2026-10-27 | Drop chances in two tables: `drop_chances.csv` (base "1 in N" per rarity) and `drop_modifiers.csv` (multipliers per source, blank = x1); final chance = base × every applicable multiplier, capped at 100% | User wants drop rates easy to rebalance and raised by corruption zones and similar systems | Hard-coded rates; one mixed table | Implemented (tables and calculation; not loaded in game yet) |
| 2026-10-27 | All affixes in one table, `Corruption/Data/affixes.csv`: one row per affix, item-class columns decide where it rolls (no separate lists). The single home for affix numbers; docs hold rules only. A test validates the real file | Easy to edit in any spreadsheet, readable diffs in git, renders on GitHub; the industry pattern (Diablo 2's MagicPrefix/MagicSuffix tables, PoE's mod tables): designers edit sheets, the game loads exported data | Separate lists per item type (duplication); numbers in Markdown; a binary .xlsx (no diffs); JSON/YAML (harder to edit by hand for flat rows) | Implemented (table and validator; not loaded in game yet) |
| 2026-10-27 | Every one-handed axe (incl. Jotun Bane, Thunderblood, Frostfire) is both a tool and a weapon: rolls gathering (universal + woodcutting) and weapon affixes. Pickaxes: gathering only; two-handed axes: weapon only | User: one-handed axes chop and fight | One-handed axes as tools only (earlier the same day) | Decided by user |
| 2026-10-27 | Low-impact gathering affixes approved: Light, Careful, Hushed, Keen-eyed, Stonecutter's, Resinous, Forester's | Variety; not every item is awesome | — | Decided by user (proposed by Claude) |
| 2026-10-27 | Flat-value affixes under the one-range rule: % of the item's matching stat where it has one; otherwise flat with a smaller span. Biome tier names become roll-quality names | Keeps the base item carrying progression | Flat values with the full I–IV span | Decided by user (proposed by Claude); numbers proposed |
| 2026-10-27 | One affix range for every item, gear included; the base item carries the progression. To play-test | User: a well-rolled flint axe must never match a Nord axe, yet every affix rolls its full range | Biome tiers I–IV (general draft) | Decided by user; supersedes the tool-only row below |
| 2026-10-27 | Gathering affix ranges are the same for every tool; the base tool's stats and tool tier carry the progression | User: every affix rolls its full range on any tool, yet a flint axe never matches a Nord axe | Ranges scaled by the tool's biome; ranges identical with nothing else separating tools | Superseded by the all-items row above |
| 2026-10-27 | Both Bountifuls (and Artisan) stack | User's choice | Merging or renaming them | Decided by user |
| 2026-10-27 | Crafting makes Normal (vanilla) items only; orbs upgrade; monsters drop randomly rolled items; Magic = 1 prefix + 1 suffix, Rare = 2 + 2 | User's loot model | Affixes rolled on craft | Decided by user |
| 2026-10-27 | Gathering affixes that change the hit run on the striker's machine; on-harvest affixes use an owner-to-striker message | Drops are spawned by the object's owner, who can't see the striker's tool data | Owner-side rolls (no access to the affixes) | Proposed |
| 2026-10-27 | Build our own loot system rather than depend on Epic Loot | Design (PoE tiers, orbs, corruption lock, biome-gated uniques, affix draft) differs from Epic Loot's rarity ladder and enchanting (review, 2026-10-15) | Depending on Epic Loot 0.14.13 | Decided by user |
| 2026-10-27 | Gear affixes on items via `ItemData.m_customData`; affix table in a data file; pure roll function | Saved with the item; tunable without rebuild; testable | — | Proposed (in `AFFIX_RULES.md`) |
| 2026-10-26 | Two plugins in one repo: CraftingOverhaul (`VersionCheckOnly`) and Corruption (`EveryoneMustHaveMod`) | Different multiplayer needs: QoL works if only some players have it; new items need everyone (review) | One plugin | Implemented |
| 2026-10-26 | Gameplay config admin-only (server-synced); look config per player | Server decides balance; looks are personal | — | Implemented |
| 2026-10-15 | Rips: server decides (ID, spot, tier, affixes, loot); the activating player's game spawns and simulates | Valheim has no server simulation (review 2); matches vanilla raids | "Server-authoritative spawning" as written in the design doc | Proposed |
| 2026-10-15 | Rip waves: own spawner driven by the rip heart | `RandEventSystem` runs one event world-wide (review 1) | Custom raid event | Proposed |
| 2026-10-15 | Validate landing spots before teleporting (`WorldGenerator`, `Minimap.IsExplored`); base-distance check on the server | Works without loading the area (review 6) | Teleport, then re-roll | Proposed |
| 2026-10-15 | Inventory: vanilla Utility/Trinket slots for v1; test against ExtraSlots / EquipmentAndQuickSlots before building tabs | Avoid slot code until phase 9 | Building slots now | Proposed |
| 2026-09-29 | Biome of each material derived from game data (collectors + solver); config overrides only for exceptions | User: "you should know the materials list… check game files"; covers Deep North and modded items | Hand-written material list (removed) | Implemented |

## Features

| Date | Decision | Why | Rejected | Status |
|---|---|---|---|---|
| 2026-10-26 | Talismans use the vanilla Trinket slot | No slot code needed (review 7) | A new slot in phase 2 (design doc) | Implemented; trade-off: replaces a vanilla trinket |
| 2026-10-26 | Talisman families from game data: base creature + prefab names extending it, monster AI only, no bosses, wildcard exclusions; trophy = base's own or the members' most common | User prefers data-driven; covers modded creatures | Hand-written family table (review 9) | Implemented |
| 2026-10-26 | Repel via `BaseAI.IsEnemy` postfix that only flips true→false for creature-asks-about-wearer; `BaseAI.OnDamaged` provokes for `ProvokeSeconds` | `IsEnemy` also gates player attacks, so the player's side must stay untouched | Faction changes; patching targeting only | Implemented |
| 2026-10-26 | Riftforge recipe has 5 ingredients; registration refuses more than 5 | `Hud.SetupPieceInfo` crashes on 6 | Leather scraps (dropped) | Implemented |
| 2026-10-26 | Riftforge raven uses its own key (`corruption_riftforge`) with `m_alwaysSpawn = false` | The clone repeated the Galdr table's text, then kept respawning | Removing the guide point | Implemented |
| 2026-10-26 | Corrupted look is red `#B3170D`; shard on the Bloodstone (`GemstoneRed`) model; tint every colour property, particles and lights | User wanted PoE/Diablo red; partial tint looked pale blue / pink | Purple tint on `Crystal` (design doc's purple) | Implemented |
| 2026-10-26 | Shards: no drops from tamed creatures; boss shards on first kill per world via the boss's defeat key | Prevent farming starred pens; key covers multi-phase bosses | — | Implemented |
| 2026-10-15 | Riftforge needs Eikthyr's trophy (+ nails, cores) | "Any boss trophy" can't be one vanilla ingredient (review 3) | `m_requireOnlyOneIngredient`; patching the requirement check | Decided by user |
| 2026-09-29 | Don't write our own networking; recommend NetworkPerformanceSystem server-only | Works with vanilla and PS5 clients; own code would conflict with networking mods | Own networking; SkadiNet (version lock); Steam send-rate mods (no effect on crossplay) | Proposed, untested |
| 2026-09-27 | Nearby chests: never ships, carts or tombstones; respect private chests, wards and chests in use; claim ownership before taking | Taking from a ship/cart makes you its owner and disrupts the driver | — | Implemented |

## Design (user decisions, from `corruption-design.md`)

- Rip return is heart-gated; portals close after 1 in-game day; no fallback.
- Unlimited copies of uniques; uniques never craftable.
- Map affixes never brick a map; only the shard bricks gear (25%).
- No downsides on loot.
- One talisman at a time; rings get their own slot.

## Corrections (things we got wrong before)

| Date | Wrong | Right |
|---|---|---|
| 2026-10-26 | `resetknownitems` replays raven messages | `tutorialreset` does |
| 2026-10-26 | `GetComponentInParent<ScrollRect>()` finds the recipe list | It skips inactive objects; use `GetComponentInParent<ScrollRect>(true)` |
| 2026-10-26 | Spawn entries with biome "all" place drops in Meadows | Entries covering every land biome are ignored |
| 2026-10-15 | A dedicated server can own/simulate spawns | The owning client simulates; the server relays |
| 2026-09-29 | Early-access bandwidth figures still apply | Current code uses 150 KB/s on Steam; crossplay uses PlayFab, where the Steam rate doesn't apply |
