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

## Tracks and status

Legend: **Verified** = tested in-game by the user · **Built** = code exists, not verified in-game · **Proposed** = designed, no code.

| Track / system | Plugin | Status | Detail |
|---|---|---|---|
| Recipe search, collapsible categories, biome grouping | CraftingOverhaul | Verified (single player) | `FEATURES.md` |
| Craft and build from nearby chests | CraftingOverhaul | Verified single player; multiplayer untested | `FEATURES.md`, `multiplayer-test.md` |
| Phase 1: corruption shards | Corruption | Built (spawned item verified; drops untested) | `FEATURES.md` |
| Phase 1: Riftforge station | Corruption | Verified (no recipes yet) | `FEATURES.md` |
| Phase 2: talismans | Corruption | Built, untested | `FEATURES.md` |
| Phase 3: boss essences | Corruption | Built, untested (8 bosses; Hive left out) | `FEATURES.md`, `Corruption/Data/essences.csv` |
| Phase 4: maps and rips | Corruption | Step A tested in-game (single player); step B built, untested (waves, maddened, loot) | design doc + review 1, 2, 6 |
| Phase 5: map affixes and tiers, 3-star | Corruption | Proposed | design doc + review 5 |
| Phase 6: corrupted bosses | Corruption | Core tested (multipliers, loot, tint, `co_corrupt`); corrupted offerings tested 2026-10-30; summons and particles to come | `FEATURES.md`, design doc + review 4 |
| Phase 7: loot, rarity, gear and gathering tool affixes, orbs, uniques | Corruption | Step 1 tested in single player (Magic/Rare drops, rolled affixes, names, tooltips, item-stat affixes; branch `claude/affixes-loot`); every gear range settled; orbs, uniques, other effects to come (orb art made, PoE names kept) | design doc, `AFFIX_RULES.md` |
| Phase 8: scars, terror zones, rare elites, Svartalfheim | Corruption | Proposed | design doc |
| Phase 9: wheel of powers, inventory slots/tabs | Corruption | Proposed. Backpacks come from AdventureBackpacks and equipment slots from EquipmentAndQuickSlots (2026-10-28). The slots are shown in a WoW-style **character sheet** (CraftingOverhaul, tested 2026-10-29); **Currency and Maps tabs** built 2026-10-29 (Corruption, untested) | design doc, `mod-compatibility.md`, `FEATURES.md` → Character sheet, Currency and Maps tabs |
| Phase 10: skill rework (optional module) | — | Proposed; now part of classes and levels (2026-10-30) | design doc |
| Classes, levels and skill trees (D2 Amazon first) | Orlog | Levels and XP built, untested (2026-10-30); classes, attributes and skills proposed | `class-design.md`, `data/d2-reference/` |
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
- Only the shard can brick gear; orbs never brick; corrupted items are locked. Corrupting Normal, Magic or Rare gear: 25% bricked, 25% unchanged but locked, 50% one corrupted affix on top of the normal limit, rolled from the normal pool; it may repeat a family, resonates with a twin and ignores the caps. Corrupted gear never drops; it only comes from a shard (2026-10-28).
- Affix Splinters (2026-10-28): shattering a Magic or Rare item at the Riftforge is free (not uniques or corrupted items); each affix line has a 50% chance to become a splinter that keeps its exact value, a resonant line's splinter keeps the combined value; splinters go on Normal and Magic items with fewer than 2 affixes; they don't stack, so they need the Currency tab.

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
| Talismans in "one new slot"; stave icon with creature tint | Built on the vanilla Trinket slot (review 7) with the trophy as icon | Implemented; dedicated slot can come with phase 9 |
| Corrupted Queen via offering | The Queen has no altar (game knowledge, unverified) | Decided 2026-10-28: a corrupted Sealbreaker opens her door and makes her corrupted |

## Open design questions

From the design doc: inventory slots built or from a mod; scars permanent or expiring; one map per group or per player; minimum rip distance from bases; shard yield scaling with map tier; Mistlands/Ashlands/Deep North map materials and Fader's summons.
Added since: runewords (sockets are a placeholder); the world-unique pool contents; orb drop rates; which plugin hosts storage/quests; whether to rename the Corruption plugin to a Norse name.
