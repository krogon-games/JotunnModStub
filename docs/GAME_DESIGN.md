# Game design overview

Entry point to the design. It summarises what has been agreed and where each system stands; the detail lives in the linked documents. Build status here must match [`CURRENT_STATE.md`](CURRENT_STATE.md) and [`FEATURES.md`](../FEATURES.md).

**Audience:** a group of four friends on a dedicated Valheim 1.0 server. Choice beats scarcity; tuning is for that group, not a public release.

## Design documents

| Document | Covers | Author |
|---|---|---|
| [`corruption-design.md`](corruption-design.md) | Corruption endgame: shards, essences, Riftforge, maps, rips, scars, corrupted bosses, exploration events, talismans, loot, uniques, orbs, build order | User (converted from PDF), with dated changes |
| [`corruption-design-review.md`](corruption-design-review.md) | Where the design and Valheim's code disagree, simplifications, build-vs-depend options | Technical review, 2026-10-15 |
| [`AFFIX_RULES.md`](AFFIX_RULES.md) | Gear prefixes/suffixes, tiers, twin resonance, range display | Draft, awaiting review |
| [`class-design.md`](class-design.md) | Diablo 2-style classes (Amazon first, then Barbarian, maybe Paladin), character levels, stat and skill points, EpicMMOSystem comparison | User brief + Claude's proposal, 2026-10-30 |
| [`classes-ui-brief.md`](classes-ui-brief.md) | Classes UI for the UI session: HUD bars, skill cross, XP bar, skills panel, class choice, and the API contract between the plugins | Classes session, 2026-10-30 |
| [`storage-quests-design.md`](storage-quests-design.md) | Chest tiers, quest-unlocked super chest, Mistlands bank/vault | User brief + notes |
| [`art-prompts.md`](art-prompts.md) | Image-model prompts for concept renders (boss essences, currency orbs) | Draft |
| [`ideas.md`](ideas.md) | Original idea backlog, each linked to its design section | — |
| [`coop-networking.md`](coop-networking.md) | Co-op lag research (not a mod feature) | Research, 2026-09-29 |

The two full designs are **[`corruption-design.md`](corruption-design.md)** (the whole Corruption endgame; `master`'s copy is the current one, with the user's 2026-11-01 decisions) and **[`storage-quests-design.md`](storage-quests-design.md)** (chests and quests). Everything else in this file summarises or points.

## Supporting notes

| Document | Use it for |
|---|---|
| [`../STATUS.md`](../STATUS.md) | Where every branch stands, what's half-finished, what's waiting on the user, the top priorities (coordinator only) |
| [`DECISIONS.md`](DECISIONS.md) | Every user decision in one table, plus the technical log (coordinator only) |
| [`ROADMAP.md`](ROADMAP.md) | The current stretch's plan per session; the task list is the GitHub issues it links |
| [`CURRENT_STATE.md`](CURRENT_STATE.md) | What's verified, built, broken; build health |
| [`riftforge-ui-brief.md`](riftforge-ui-brief.md) | The Riftforge window's contract between the loot, UI and rips sessions |
| [`mod-compatibility.md`](mod-compatibility.md) | The other mods the group runs, shared game methods and patch order |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | Code layout, patterns, server messages, game limits |
| [`PLAYTEST.md`](PLAYTEST.md) | The open single-player steps and the group evening's script (Session 2) |
| [`playtests/`](playtests/README.md) | Each round's play-test checklist and the user's feedback, one file per branch |
| [`handoffs/`](handoffs/README.md) | Each branch's handoff: stage, decisions, open questions, merge notes |
| [`PLAYER_SETUP.md`](PLAYER_SETUP.md) | Installing the mods and server settings for the group |
| [`multiplayer-test.md`](multiplayer-test.md) | The older multiplayer test steps (folded into `PLAYTEST.md` → Session 2) |
| [`../Corruption/Data/`](../Corruption/Data/README.md), [`../Orlog/Data/`](../Orlog/Data/README.md) | Tunable numbers (affixes, caps, drop rates, levelling tables) |

## Tracks and status

Legend: **Verified** = tested in-game by the user · **Built** = code exists, not verified in-game · **Proposed** = designed, no code.

| Track / system | Plugin | Status | Detail |
|---|---|---|---|
| Recipe search, collapsible categories, biome grouping | CraftingOverhaul | Verified (single player) | `FEATURES.md` |
| Craft and build from nearby chests | CraftingOverhaul | Verified single player; multiplayer untested | `FEATURES.md`, `multiplayer-test.md` |
| Phase 1: corruption shards | Corruption | Verified (spawned item, own model, drops from 1- and 2-star creatures and a boss's first kill) | `FEATURES.md` |
| Phase 1: Riftforge station | Corruption | Verified, with its map recipes (2026-10-28); also makes corrupted offerings and the corrupted wishbone; orb crafting in its Forge tab built 2026-11-09 (one gear slot, materials used straight from the inventory; passed in single player 2026-11-10, PR #59); shard corruption, shattering and Affix Splinters built 2026-11-10 (#29; untested; splinter models approved: a shape per kind of affix, a colour per stat) | `FEATURES.md` → Riftforge workspace |
| Phase 2: talismans | Corruption | Verified (single player); multiplayer untested | `FEATURES.md` |
| Phase 3: boss essences | Corruption | Verified (single player; 8 bosses; Hive left out); multiplayer untested | `FEATURES.md`, `Corruption/Data/essences.csv` |
| Phase 4: maps and rips | Corruption | Steps A and B (waves, maddened, loot) and the heart search with the corrupted wishbone verified (single player); multiplayer untested | design doc + review 1, 2, 6 |
| Phase 5: map affixes and tiers, 3-star | Corruption | Proposed | design doc + review 5 |
| Phase 6: corrupted bosses | Corruption | Core tested (multipliers, loot, tint, `co_corrupt`); corrupted offerings tested 2026-10-30; summons tested 2026-10-31 (single player, the fix making them the boss's allies re-checked the same day); multiplayer untested; particles to come | `FEATURES.md`, design doc + review 4 |
| Phase 7: loot, rarity, gear and gathering tool affixes, orbs, uniques | Corruption | Step 1 tested in single player (Magic/Rare drops, rolled affixes, names, tooltips, item-stat affixes); step 2 (player-stat affixes) tested in single player apart from weapon skill and life steal; steps 3 (gathering-tool affixes) and 4 (proc and utility affixes) built, step 3 partly verified (single player, 2026-11-08, PR #55: Seismic, Odin's Grace on rock and ore, Veinbreaker's, Bountiful; only the player's own swings get the tool's affixes), the rest untested; multiplayer (each player's own roll, another player's game running the tree or rock) untested; every gear range settled; the currency orbs and their Riftforge rules (Krogon's #28 rules; `co_craft` until the window exists; no drops yet) and Stonecutter's and Resinous per hit tested in single player 2026-11-08 (PR #58; the refusal away from the Riftforge left for the window); corruption with a shard, shattering, splinters, orb drops and uniques to come (PoE names kept) | design doc, `AFFIX_RULES.md` |
| Phase 8: scars, terror zones, rare elites, Svartalfheim | Corruption | Proposed | design doc |
| Phase 9: wheel of powers, inventory slots/tabs | Corruption | Proposed. Backpacks come from AdventureBackpacks and equipment slots from EquipmentAndQuickSlots (2026-10-28). The slots are shown in a WoW-style **character sheet** (CraftingOverhaul, tested 2026-10-29); **Currency and Maps tabs** (Corruption, tested 2026-10-30), six tabs since 2026-11-09 (Bag · Currencies · Splinters · Maps · Runes · Essences; untested) | design doc, `mod-compatibility.md`, `FEATURES.md` → Character sheet, Inventory tabs |
| Phase 10: skill rework (optional module) | — | Proposed; now part of classes and levels (2026-10-30) | design doc |
| Classes, levels and skill trees (D2 Amazon first) | Orlog | Levels and XP (2026-10-30) and attributes with the class eitr pool (2026-10-31) verified (single player; XP sharing untested); the Orlog stone, classes, skill trees and skills proposed. Its UI (HUD bars, skill cross, skills panel, class choice) in CraftingOverhaul verified with demo data (single player, 2026-10-31, the adrenaline bar included) | `class-design.md`, `data/d2-reference/` |
| Storage and quests | Undecided plugin | Proposed, saved for later | `storage-quests-design.md` |
| Combat mechanics (parry, posture, combos) | — | Ideas only | `FEATURES.md` → Combat mechanics |

## Agreed by the user

- Design pillars: the world is the dungeon; extend vanilla, don't fight it; reuse before you create; risk buys reward; Norse names.
- v1 constraints: ~~no new animations or attacks~~ new animations and attacks allowed where necessary, vanilla ones first (changed 2026-10-30); new models only where the user asks (shard, essences); otherwise tints and reused particles; vanilla pickup-unlock rules; must work on a dedicated server.
- Core loop per biome: farm → summon boss → essence → craft map → open rip → loot and escape. Each boss's essence crafts maps into the corrupted version of its own biome (Eikthyr → corrupted Meadows … Kall Fimbulbringer → corrupted Deep North); no skipping biomes (changed 2026-10-28). The Hive is left out.
- Riftforge recipe: Fine wood, Corruption shard, Eikthyr trophy, Bronze nails, Surtling cores (5 ingredients; leather scraps dropped for the 5-ingredient limit).
- Rip return: heart-gated; both portals close after 1 in-game day; no safety fallback.
- Talismans: full repel of one creature family; provoked creatures fight back; bosses never; two talisman slots plus the vanilla trinket slot (changed 2026-10-28); rings get their own slot later and raise skills.
- Uniques: unlimited copies; not craftable; ~1 per 20 hours of play (base 1 in 2,500, small source multipliers).
- Personal loot (60 s owner lock) and loot beams.
- Loot drops (2026-10-29): a craftable item of the biome the creature died in, 20% from the biome before; level 1; Magic 1 affix twice as often as 2, Rare 3 twice as often as 4.
- Loot system (rarity, affixes, orbs, uniques) is our own, not Epic Loot (decided 2026-10-27).
- Workbench and forge craft Normal (vanilla) items only; orb crafting at the Riftforge upgrades them; monsters drop items with random rolls. Magic = up to 1 prefix + 1 suffix, Rare = up to 2 + 2 (2026-10-27).
- Gathering tools get their own affix pools (universal, mining, woodcutting) plus 7 low-impact affixes. Every one-handed axe (Jotun Bane, Thunderblood and Frostfire included) is both a tool and a weapon: it rolls gathering and weapon affixes. Pickaxes roll gathering affixes only (2026-10-27).
- Affix sides, weights and twins (2026-10-28): gathering prefixes = stronger swing, suffixes = fortune and endurance; weights filler 100 / normal 50 / rare 10 / Deathless 5 / twin halves 25; one combined pool on one-handed axes; twins (a prefix and a suffix on the same stat) resonate on Magic and Rare items; gathering twins for resource damage, durability, skill XP and stamina per swing.
- Flat-value affixes: a % of the item's matching stat where one exists, otherwise flat with a smaller span (2026-10-27).
- Affix ranges are the same for every item, tools and gear alike; the base item carries the progression. To play-test (2026-10-27).
- Monster drops start at Magic 1 in 50 and Rare 1 in 100 per kill; uniques have separate chances; all chances in a tunable table, multiplied by corruption zones and similar systems (2026-10-27).
- Odin's Grace is the powerful rare gathering affix: whole tree, or the hit deposit chunk plus every chunk touching it (2026-10-27).
- Gathering affixes apply only to the player's own swings: something that breaks without the player's hit (a rolling log, a falling tree, a creature) gives normal drops; a rock that turns into a copper deposit passes them on. Veinbreaker's count resets whenever the tool is unequipped (2026-11-08).
- Only the shard can brick gear; orbs never brick; corrupted items are locked. Corrupting Normal, Magic or Rare gear: 25% bricked, 25% unchanged but locked, 50% one corrupted affix on top of the normal limit, rolled from the normal pool; it may repeat a family, resonates with a twin and ignores the caps. Corrupted gear never drops; it only comes from a shard (2026-10-28). The Mirror of Kalandra is the one orb that works on a corrupted item; its copy keeps the corruption (2026-11-08).
- Affix Splinters (2026-10-28): shattering a Magic or Rare item at the Riftforge is free (not uniques or corrupted items); each affix line has a 50% chance to become a splinter that keeps its exact value, a resonant line's splinter keeps the combined value; splinters go on Normal and Magic items with fewer than 2 affixes; they don't stack, so they have their own Splinters tab, the size of the bag (user, 2026-11-09: limited on purpose; collections go in chests). **2026-11-10:** a splinter that completes a twin gives the two values added plus 25%, with no roll; shattering needs room for every possible splinter first; a Mirror's copy is locked like a corrupted item; shard and shatter confirmations have a remembered "Don't warn me again" tickbox; caps count over everything worn and will be shown on a stats sheet (#60), not in tooltips; a shard's corrupted affix completing a twin adds both values plus 25%, like a splinter; splinters have their own models (a shape per kind of affix, a colour per stat, rough lumps; approved and built).

## Balance rules

- Map rewards scale to the map's biome, not the player's gear.
- Shards drop fastest from high-tier rips; first-boss-kill and starred-creature shards only start the loop (current defaults: 2% 1-star, 5% 2-star, 1–3 per boss first kill; tamed creatures never drop).
- Boosts, not bypasses: nothing removes portal, carry-weight or boat constraints outright.
- Loot never carries downsides; danger lives in maps and fights.
- Essences have no use except maps and can't be sold.
- Affix caps (movement, attack speed, pickup radius, resistances, life steal): [`Corruption/Data/affix_caps.csv`](../Corruption/Data/affix_caps.csv).
- Every number in the design is a starting point; all rates and multipliers go in config so tuning needs no rebuild.

## Where design and implementation or review disagree (to reconcile)

| Design doc says | Reality / review | Status |
|---|---|---|
| Spawning, teleporting and status changes are server-authoritative | The owning player's game simulates creatures; the server only relays (review 2) | Decided 2026-10-28: server decides, owner spawns |
| Rip waves through the vanilla raid system | One raid at a time world-wide (review 1) | Decided 2026-10-28: own wave spawner |
| Validate the landing spot after teleporting | Biome, height and explored state can be checked before (review 6) | Proposal |
| Corrupted look is a purple tint | Shard and Riftforge use red `#B3170D` at the user's request | Decided 2026-10-28: everything corrupted is red |
| Talismans in "one new slot"; stave icon with creature tint | First built on the vanilla Trinket slot (review 7) with the trophy as icon; since 2026-10-28 in their own Talisman slots (2 by default) through EquipmentAndQuickSlots' slot API (`FEATURES.md` → Talismans) | Implemented |
| Corrupted Queen via offering | The Queen has no altar (game knowledge, unverified) | Decided 2026-10-28: a corrupted Sealbreaker opens her door and makes her corrupted |

## Open design questions

The live list of questions waiting on the user is [`STATUS.md`](../STATUS.md) → Waiting on you (and the GitHub issues labelled `needs decision`). Answered since this list was written: inventory slots (EquipmentAndQuickSlots and AdventureBackpacks, 2026-10-28); every biome's map material (`maps.csv`, all approved); Fader's summons (every boss summons its army raid, 2026-10-30); scars expire after 3 in-game days, one map per group, shard yield per tier (all 2026-11-01).

From the design doc, still open: the minimum rip distance from bases (today `BaseDistance`, a ward's radius).
Added since: runewords (sockets are a placeholder); the world-unique pool contents; orb drop rates; which plugin hosts storage/quests; whether to rename the Corruption plugin to a Norse name.
