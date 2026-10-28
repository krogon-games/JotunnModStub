# Game design overview

Entry point to the design. It summarises what has been agreed and where each system stands; the detail lives in the linked documents. Build status here must match [`CURRENT_STATE.md`](CURRENT_STATE.md) and [`FEATURES.md`](../FEATURES.md).

**Audience:** a group of four friends on a dedicated Valheim 1.0 server. Choice beats scarcity; tuning is for that group, not a public release.

## Design documents

| Document | Covers | Author |
|---|---|---|
| [`corruption-design.md`](corruption-design.md) | Corruption endgame: shards, essences, Riftforge, maps, rips, scars, corrupted bosses, exploration events, talismans, loot, uniques, orbs, build order | User (converted from PDF), with dated changes |
| [`corruption-design-review.md`](corruption-design-review.md) | Where the design and Valheim's code disagree, simplifications, build-vs-depend options | Technical review, 2026-10-15 |
| [`AFFIX_RULES.md`](AFFIX_RULES.md) | Gear prefixes/suffixes, tiers, twin resonance, range display | Draft, awaiting review |
| [`storage-quests-design.md`](storage-quests-design.md) | Chest tiers, quest-unlocked super chest, Mistlands bank/vault | User brief + notes |
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
| Phase 3: boss essences | Corruption | Proposed (next) | design doc |
| Phase 4: maps and rips | Corruption | Proposed | design doc + review 1, 2, 6 |
| Phase 5: map affixes and tiers, 3-star | Corruption | Proposed | design doc + review 5 |
| Phase 6: corrupted bosses | Corruption | Proposed | design doc + review 4 |
| Phase 7: loot, rarity, gear and gathering tool affixes, orbs, uniques | Corruption | Proposed (gathering tool pools specified by the user; general gear pool a draft) | design doc, `AFFIX_RULES.md` |
| Phase 8: scars, terror zones, rare elites, Svartalfheim | Corruption | Proposed | design doc |
| Phase 9: wheel of powers, inventory slots/tabs | Corruption | Proposed | design doc |
| Phase 10: skill rework (optional module) | — | Proposed | design doc |
| Storage and quests | Undecided plugin | Proposed, saved for later | `storage-quests-design.md` |
| Combat mechanics (parry, posture, combos) | — | Ideas only | `FEATURES.md` → Combat mechanics |

## Agreed by the user

- Design pillars: the world is the dungeon; extend vanilla, don't fight it; reuse before you create; risk buys reward; Norse names.
- v1 constraints: no new animations, attacks or 3D models; tints and reused particles only; vanilla pickup-unlock rules; must work on a dedicated server.
- Core loop per biome: farm → summon boss → essence → craft map → open rip → loot and escape. Each boss's essence crafts maps for the next biome; no skipping biomes.
- Riftforge recipe: Fine wood, Corruption shard, Eikthyr trophy, Bronze nails, Surtling cores (5 ingredients; leather scraps dropped for the 5-ingredient limit).
- Rip return: heart-gated; both portals close after 1 in-game day; no safety fallback.
- Talismans: full repel of one creature family; provoked creatures fight back; bosses never; one talisman at a time; rings get their own slot later and raise skills.
- Uniques: unlimited copies; not craftable; ~1 per 20 hours of play (base 1 in 2,500, small source multipliers).
- Personal loot (60 s owner lock) and loot beams.
- Loot system (rarity, affixes, orbs, uniques) is our own, not Epic Loot (decided 2026-10-27).
- Workbench and forge craft Normal (vanilla) items only; orb crafting at the Riftforge upgrades them; monsters drop items with random rolls. Magic = up to 1 prefix + 1 suffix, Rare = up to 2 + 2 (2026-10-27).
- Gathering tools get their own affix pools (universal, mining, woodcutting) plus 7 low-impact affixes. Every one-handed axe (Jotun Bane, Thunderblood and Frostfire included) is both a tool and a weapon: it rolls gathering and weapon affixes. Pickaxes roll gathering affixes only (2026-10-27).
- Flat-value affixes: a % of the item's matching stat where one exists, otherwise flat with a smaller span (2026-10-27).
- Affix ranges are the same for every item, tools and gear alike; the base item carries the progression. To play-test (2026-10-27).
- Monster drops start at Magic 1 in 50 and Rare 1 in 100 per kill; uniques have separate chances; all chances in a tunable table, multiplied by corruption zones and similar systems (2026-10-27).
- Odin's Grace is the powerful rare gathering affix: whole tree, or the hit deposit chunk plus every chunk touching it (2026-10-27).
- Only the shard can brick gear (25%); orbs never brick; corrupted items are locked.

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
| Spawning, teleporting and status changes are server-authoritative | The owning player's game simulates creatures; the server only relays (review 2). Proposal: server decides, owner spawns | Needs the user's decision before phase 4 |
| Rip waves through the vanilla raid system | One raid at a time world-wide (review 1). Proposal: own wave spawner | Needs decision before phase 4 |
| Validate the landing spot after teleporting | Biome, height and explored state can be checked before (review 6) | Proposal |
| Corrupted look is a purple tint | Shard and Riftforge use red `#B3170D` at the user's request | Decide whether corrupted bosses/gear follow red |
| Talismans in "one new slot"; stave icon with creature tint | Built on the vanilla Trinket slot (review 7) with the trophy as icon | Implemented; dedicated slot can come with phase 9 |
| Corrupted Queen via offering | The Queen has no altar (game knowledge, unverified) | Needs a different trigger |

## Open design questions

From the design doc: inventory slots built or from a mod; scars permanent or expiring; one map per group or per player; minimum rip distance from bases; shard yield scaling with map tier; Mistlands/Ashlands/Deep North map materials and Fader's summons.
Added since: runewords (sockets are a placeholder); the world-unique pool contents; orb drop rates; which plugin hosts storage/quests; whether to rename the Corruption plugin to a Norse name.
