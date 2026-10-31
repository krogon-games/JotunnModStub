# Roadmap: the next stretch of development

**Written:** 2026-11-01, after every session's branch was merged into `master` (PRs #1–#16; 225 tests pass). **For:** the user and the next round of Claude sessions. Each session reads its own part below before starting, then follows `CLAUDE.md` (branch from the latest `master`, one branch per session, PR when a feature is finished).

The facts here come from `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `FEATURES.md`, `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/AFFIX_RULES.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`, `docs/DECISIONS.md` and the code. Where this file and those disagree, those win: fix this file.

## 1. Where we stand

| Track | Built and verified in single player | Not built |
|---|---|---|
| CraftingOverhaul | Recipe search and categories, biome detection, craft and build from chests, character sheet, HUD bars, classes UI (on demo data), `co_dump` | Riftforge crafting window (see §4), recipe list redesign (user's to-do) |
| Corruption: core loop | Shards, Riftforge, talismans, essences, maps and rips (A and B), heart search, corrupted offerings, bosses and summons, Currency and Maps tabs, Odin unlock | Phase 5 (map tiers and affixes, 3-star), phase 8 (scars, terror zones, elites, Svartalfheim), boss particles |
| Corruption: loot (phase 7) | Magic and Rare drops, names, tooltips, 28 item-stat and player-stat affix effects | 33 gathering affix effects, 10 proc affixes, orbs, shard corruption and shattering at the Riftforge, Affix Splinters, uniques, ring slot, personal-loot locks, loot beams |
| Orlog | Levels and XP, attributes, class eitr pool, `Orlog.Api` for the character | Orlog stone, skill trees, every skill, skill button and slots, respec item, boss bonus points, eitr potions |

**Untested anywhere:** multiplayer, for every feature (`PLAYTEST.md` → Session 2). Small single-player leftovers: the adrenaline bar, weapon-skill and life-steal affixes, crafting an Odin item, PlantEasily bulk planting from chests, the corrupted-summons re-check.

**Already settled with the user** (don't reopen): see `DECISIONS.md`. In short: own loot system, own levels, D2 rules in config, the Amazon first, the HUD and panel look, every affix range, the tabs, EquipmentAndQuickSlots and AdventureBackpacks as dependencies.

## 2. The goal of this stretch: playable together

Everything so far was built and checked alone. The next stretch makes the mods something the four friends can play on the server, in this order of importance:

1. **The group plays what exists.** Only the user can run the multiplayer session; the sessions prepare it (§6 Housekeeping) and fix what it finds.
2. **Close the loot loop.** Shards have no gear sink yet and the Riftforge makes only maps, offerings and the wishbone. Orbs, corruption, shattering and uniques turn drops into a crafting game. Needs one new screen, the Riftforge window.
3. **Make the Amazon playable.** The group voted for her. The stone, the tree, her passives, then the skill button and the bow skills.
4. **Deepen rips.** Map tiers and affixes are the shard sink on the map side, and the step where 3-star creatures and elite packs arrive.

**Deferred past this stretch:** phase 8 (scars, terror zones, roaming elites, Svartalfheim), storage and quests, combat mechanics, the wheel of powers, changing class at the stone, server-synced data tables, Norse renaming. **Other classes are extra content after everything above** (§8).

## 3. Sessions

Four sessions, the same areas and branch names as before, each started fresh from `master`:

| Session | Branch | Owns | Start with |
|---|---|---|---|
| Classes | `claude/class-skills` | `Orlog/` | The Orlog stone and class choice |
| Loot | `claude/affixes-loot` | `Corruption/` loot, orbs, Riftforge crafting rules | Gathering affix effects |
| Rips | `claude/rips-bosses` | `Corruption/` rips, bosses, server messages | Map tiers and affixes |
| UI | `claude/ui-overhaul` | `CraftingOverhaul/` screens; the Riftforge window | Play-test fixes, then the Riftforge window |

Rules that keep four sessions from colliding:
- Stay in your files. `CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Texts.cs`, `FEATURES.md`, `CURRENT_STATE.md`, `DECISIONS.md` and `PLAYTEST.md` are shared: add, never rewrite, merge `master` often, keep both sides in conflicts.
- A game method several sessions patch (`Character.Damage`, `Character.RPC_Damage`, `Player.GetTotalFoodValue`, `SEMan.ModifyAttack`, `Character.OnDeath`) gets its new patch recorded in `docs/mod-compatibility.md` with its Harmony priority, so the order is deliberate.
- Contracts between sessions are written down before either side builds (`classes-ui-brief.md` is the model). Changes go into the brief in `master`.
- Every PR keeps `dotnet test CraftingOverhaul.Tests` green and updates `FEATURES.md` and `PLAYTEST.md` in the same commit.

## 4. Session plans

### 4.1 Classes (`claude/class-skills`, Orlog)

Follows `class-design.md` §8 (build order). Step 1 (levels, attributes) is done except the respec item and the boss bonus points.

| # | Task | Done when | Notes |
|---|---|---|---|
| C1 | **Orlog stone and class choice** (`class-design.md` §5a) | A character with no class uses the stone at the spawn area, the existing class choice window opens (no demo), the Amazon is chosen and kept after relogging; a classless character gets a raven hint once | Stone cloned from the start area's own stone prefabs, placed once per world beside the trophy stones as a persistent object (as the rip heart is), position from the world's `StartTemple` location. Fill `GetClassStoneRequest` and `ChooseClass`. `orlog_class` stays as a cheat. Changing class: later (user) |
| C2 | **Skill tree data and rules** (`class-design.md` §4) | The skills panel shows the Amazon's real tree, spending works and refusals give their reason, points survive relogging, `orlog_respec` returns skill points too | `Orlog/Data/amazon_skills.csv` made by `tools/make_orlog_tables.py` from `data/d2-reference/amazon.csv` with the statuses of `class-design.md` §5; pure `SkillRules` (level tiers 1/6/12/18/24/30, prerequisites count only while built, 20 points, one point per level past the required level, synergies from hard points, effective level); `GetSkillIdsJson`, `GetSkillInfoJson` with the description text and numbers filled in, `SpendSkillPoint`. `GetSkillIcon` may stay null: the UI has placeholder emblems |
| C3 | **Amazon passives**: Critical Strike, Dodge, Avoid, Evade | Each shows its chance in the tooltip and works in a fight; a test covers the rules | Crits are a new mechanic (double physical damage on the attacker's game); the avoids run on the player's own game. `Character.Damage` and `RPC_Damage` already carry Corruption's prefixes: agree the order in `mod-compatibility.md`. **Data gap:** `data/d2-reference/amazon.csv` has no per-level values (chance per rank); add them from the sources in its README before building, never guess them |
| C4 | **Boss bonus points and the respec item** | A boss's first kill gives the configured points; a craftable item resets points | Decided rules (D2's quest points → each boss's first kill) with no numbers yet: ask the user. Respec item: with Corruption, from boss essences (D2's Token); without, a plain recipe |
| C5 | **Skill button, slots and the bow skills** (`class-design.md` §4) | Right click fires the chosen skill with any weapon, block has moved to a free rebindable key, the four slots have hotkeys and show cost, cooldown and "need bow"; Magic Arrow shoots without an arrow; then Fire, Cold, Multiple, Exploding, Ice and Freezing Arrow and Inner Sight | Prototype the input first (drawing a bow on a non-attack button). Default keys read from the game's and the group's mods' bindings. Fill `GetSkillBarJson`, `GetSlotStateJson`, `SetSkillSlot`. Cooldowns as status effects. Costs in eitr |
| C6 | **Spear and javelin skills** | Jab, Power Strike, Impale; Poison Javelin, Lightning Bolt, Plague Javelin | Javelins throw a spectral copy (decided) |
| C7 | Later: the Amazon's `later` skills (Guided Arrow, Strafe, Immolation Arrow, Pierce, Slow Missiles, Decoy, Valkyrie, Charged Strike, Fend, Lightning Strike, Lightning Fury) | Each one play-tested on its own | Decoy and Valkyrie need the player-clone prototype (`class-design.md` §5). Only the Amazon is worked on in this stretch; other classes are extra content (§8) |

**Ask the user before C3–C5:** how D2 skill values scale to Valheim (propose per skill from the data, as the attributes were); boss XP (today a boss counts as one creature of its level; D2 gives 9–65×); eitr regen without eitr food (vanilla's 5 per second today: keep or set); bonus points per boss first kill; whether Penetrate also breaks immunity; eitr potions for the early biomes (materials, and that they belong to Orlog).

**Meets:** the UI session through `classes-ui-brief.md` §3 (add to its table in `master`; the `needs` field is already an addition); the loot session later for "+N to skills" affixes and the respec token recipe; the rips session on whether maddened and rip creatures give more XP (the design says corrupted creatures give more; `KillXp` only reads stars today).

### 4.2 Loot (`claude/affixes-loot`, Corruption phase 7)

| # | Task | Done when | Notes |
|---|---|---|---|
| L1 | **Gathering affix effects**: the 33 approved rows still shown as "(not active yet)" (universal, mining, woodcutting pools) | No gathering line is grey; each has a `PLAYTEST.md` step | Rules in `AFFIX_RULES.md` → Interpretations and Multiplayer: hit changes on the striker's game, extra drops by an owner-to-striker message. Mod compatibility: AdventureBackpacks' `GetWeight` (Light: multiply), EquipmentAndQuickSlots' `AutoPickup` (Magnetic, Keen-eyed), PlantEasily bulk harvests |
| L2 | **Proc and utility affixes**: Shieldwall, Rallying, Skald's, Steadfast, Reprisal, Wrath, Skadi, Fenrir, Bloodforged, Deathless, Foraging | Same | `AFFIX_RULES.md` → Proc and utility affixes. Deathless with EquipmentAndQuickSlots' keep-on-death (`Character.CheckDeath`); ally buffs through the game's status-effect message; kills reported back to the killer as for harvests |
| L3 | **Currency orbs** | The design's ten orbs exist, drop from their sources, land in the Currency tab and stack | Art is made for nine (`Corruption/Assets/Orbs/`, `tools/make_orbs.py`; the Mirror of Kalandra has none yet; add `Assets\Orbs\*` to the csproj). New table `orbs.csv` (effect, sources, rates). Add their prefab names to `TabRules.TabFor` |
| L4 | **Riftforge crafting rules and the window brief** | Pure, tested rules for every operation, and `docs/riftforge-ui-brief.md` for the UI session | Operations: apply an orb (Transmutation … Mirror, `corruption-design.md` → Currency orbs), corrupt with a shard (`CorruptionOutcomes`, built), shatter (`ShatterRules`, built) into Affix Splinters (one item, affix and value in custom data, stack 1), apply a splinter, later the rips session's map rolling. The brief says what the window shows for an item put in: the operations the rules allow, why one is refused, the preview (new lines, the 25/25/50 chances), the result message. Written first, like `classes-ui-brief.md`, so the UI session can build while this session finishes L1–L3 |
| L5 | **Uniques and the ring slot** | Boss uniques drop from their boss, world uniques from the biome-gated pool, with orange names and fixed lines; rings have a slot | `uniques.csv` from the design's six boss uniques; the world pool's contents are an open question for the user. Item level = biome (as `loot_bases.csv`). Only Divine and Jeweller's orbs touch them; never shattered or corrupted. The ring slot comes through EquipmentAndQuickSlots' custom-slot API, as the talisman slots do (three boss uniques are rings) |
| L6 | **Personal-loot lock (60 s) and loot beams** | A drop is the roller's for a minute; Rares and uniques have a pillar of light and a sound | Vanilla lights and particles |
| L7 | Later: server-synced data tables (the design wants the server's copy to rule) | | |

**Ask the user:** orb drop sources and rates (open question in `GAME_DESIGN.md`); the world unique pool's contents; the Riftforge window (§4.4 U3): confirm the split, and whether they want mock-ups first as with the classes UI.

**Meets:** the UI session (L4's brief); the rips session (map rolling as a Riftforge operation; `rip_tier_*` drop modifiers; unique chances in rips); the classes session ("+N to skills" affixes once trees exist; the respec token from essences).

### 4.3 Rips and bosses (`claude/rips-bosses`, Corruption phases 5 and 8, server messages)

| # | Task | Done when | Notes |
|---|---|---|---|
| R1 | **Map tiers and affixes (phase 5)** | A map rolled at the Riftforge shows its tier and affixes in its tooltip; a tier-4 Rabid Swarming map is the near-death run the design describes | `corruption-design.md` → Map affixes and tiers: tiers 1–4 by extra shards (tier 4 plus a boss trophy and a maddened mini-boss), the seven affixes each reusing a vanilla status effect, with their danger and reward; affix data on the map item's custom data; tier feeds `drop_modifiers.csv` (`rip_tier_2`–`4` exist) and the shard yield. New table `map_affixes.csv`. Rolling happens in the Riftforge window (L4/U3): agree the operation with the loot session; until the window exists, roll from a console command |
| R2 | **3-star creatures** | A 3-star creature shows three stars and the HUD doesn't break | `Character.SetLevel(4)` works; `EnemyHud` has icons for two stars only (review point 5) |
| R3 | **Elite and champion packs** | Designed with the user, then built | Decided 2026-10-30: everything above 2 stars is an elite or champion (Diablo and PoE style). Pack sizes, modifiers, names and loot are undecided: ask first, write the design into `corruption-design.md`, then build |
| R4 | **Dedicated-server review** | `PLAYTEST.md` → Session 2 lists what to watch, and anything clearly wrong is fixed | Everything was tested in single player, where the "server" is the player's own game. Read the paths that run on a dedicated server with no local player (the rip request handler and `Game.Start` RPC registration, persistent objects placed far away, `KillXp`'s owner logic, the summons' per-player limit) and the paths on a non-owner client (pings, tints, maddened effects). This session owns the server messages (`ARCHITECTURE.md` → Server messages) |
| R5 | **Landing area check once loaded** and corrupted boss **particles** | Rips never land in a location or water; corrupted bosses carry the design's particles | Open since step A (locations, water); particles from the design (Bonemass cloud, Ashlands embers). Escalation after repeated corrupted kills is optional: ask |
| R6 | **Phase 8**: scars, then terror zones, roaming rare elites, Svartalfheim | | After the group has played rips with tiers. Scars first (the others build on their pool) |

**Ask the user:** elite and champion packs (R3); scars permanent or expiring (the design says a few in-game days); whether a map is consumed per group or per player; the shard yield per tier; the minimum rip distance from bases (today `BaseDistance`, a ward's radius).

**Meets:** the loot session (map rolling, tier modifiers); the UI session (the map tooltip and the window's map operation); the classes session (XP for maddened and rip creatures).

### 4.4 UI (`claude/ui-overhaul`, CraftingOverhaul screens and the Riftforge window)

| # | Task | Done when | Notes |
|---|---|---|---|
| U1 | **Fixes from the 2026-10-31 play-tests** | The + buttons play one click sound; the adrenaline bar is checked (`PLAYTEST.md` → Classes UI); the "check in-game" layout items in `mod-compatibility.md` are ticked with Orlog's real data | |
| U2 | **Real data instead of the demo** | Each screen works from `Orlog.Api` as the classes session fills it (stone request, skills, slots); the demo branches and `co_classes_stone` go on the day Orlog serves everything | Contract: `classes-ui-brief.md` §3. Placeholder icons stay until Orlog returns sprites or art is asked for |
| U3 | **Riftforge window** (in the Corruption plugin, new files under `Corruption/Content/`) | From the loot session's `docs/riftforge-ui-brief.md`: put an item in, see the operations the rules allow, a preview, confirm, the result | Proposed split (as the classes UI and Orlog): the loot session owns the rules, this session owns the screen; the rips session adds map rolling. It lives in Corruption because everyone has Corruption and not everyone has CraftingOverhaul, so it can't use CraftingOverhaul's embedded art: copy the game's own panel sprite at runtime (as `StyleAsGamePanel` does) and embed only the few sprites it needs in Corruption. Mock-up first from a tested layout class (`SkillsPanelLayout` → `tools/preview_skills_panel.py` pattern); the user picks. Where it opens (a tab on the Riftforge's crafting panel, or a window when using the station) is this session's call. **If the user prefers one owner, the loot session builds the screen too** |
| U4 | **Recipe list redesign and the crafting screen with AdventureBackpacks' backpack panel open** (user's to-do since 2026-10-28) | | Ask what they want changed before starting |
| U5 | Art on request: class and skill icons, unique and map-affix icons | | Only when the user asks (`CLAUDE.md`); orb icons exist |

**Meets:** every other session (the window serves loot and rips; the screens serve Orlog). Don't touch rules in `Orlog/` or `Corruption/Rules/`.

## 5. Dependencies between sessions

- C1 → U2 (the real stone replaces `co_classes_stone`). C2 → U2 (the real tree). C5 → U2 (real slot states).
- L4 (rules and brief) → U3 (window) → L3, L4 and R1 usable in game. Until then: console commands.
- R2 and R3 need the user's elite design before stars go above 2.
- The multiplayer session (user) → every session fixes its findings (R4 prepares it).

## 6. Housekeeping (small; any session, early)

- `docs/PLAYER_SETUP.md` doesn't mention **Orlog**: add it (everyone and the server need it, the same version rule as Corruption) and its server-synced settings. AdventureBackpacks' backpack sizes are already in it.
- `docs/GAME_DESIGN.md` → Tracks and status, and `CURRENT_STATE.md` → Not built: several rows lag behind the play-tests (talismans, essences, rips step B, classes). Bring them in line with `FEATURES.md`.
- `PLAYTEST.md` → Session 2: fold `multiplayer-test.md` and every feature's multiplayer check (XP sharing, personal loot rolls, levels seen by others, pings, tints, tabs) into one evening's script for the group.
- The release for the group: one build of the three DLLs from `master` (`publish.ps1` makes the packages on the user's PC) and an r2modman profile. The "before release" switches (`CURRENT_STATE.md`) stay on while developing.
- `data/game-dump-1.0.20/` is from 1.0.20; the user now runs 1.0.21: a fresh `co_dump` when convenient (numbers are derived from the dump).

## 7. Waiting on the user

- **The multiplayer play-test with a friend** (`PLAYTEST.md` → Session 2). The biggest unknown.
- The open questions listed per session above (§4.1–§4.4): answer them at each session's start.
- **Unity 6 upgrade** of `JotunnModUnity/` (Valheim 1.0.21 runs Unity 6000.0.75) on the user's PC, before any new animation clip is needed (maybe the Amazon's `later` skills; the Barbarian's Whirlwind and Leap, §8).
- Art when wanted: class and skill icons, uniques, map affixes, the Riftforge window's look.

## 8. Extra content: other classes (after everything above)

**Only the Amazon is being worked on** (user, 2026-11-01). The other classes come after this stretch, as extra content, once the Amazon is complete and play-tested by the group. Nothing here is started, briefed or asked about until the user says so.

| Class | Order | What's known | Before it can start |
|---|---|---|---|
| Barbarian | Second (group vote 2026-10-30) | Skill list and statuses in `class-design.md` §6 (masteries, warcries as emotes, Whirlwind as a looped atgeir spin first, Leap); D2 data in `data/d2-reference/barbarian.csv` | The Amazon done; `JotunnModUnity/` on Unity 6 for Whirlwind's and Leap's clips if the vanilla animations look wrong |
| Paladin | Maybe, third | Notes in `class-design.md` §7 (one aura at a time, holy = spirit, Smite needs a new animation, Conversion through taming, Redemption reworked or dropped); D2 data in `data/d2-reference/paladin.csv` | The Barbarian done, and the user's decision to build it at all |

The tree rules, the skill button and slots, cooldowns, costs and the class choice window are built once for the Amazon and reused; a new class is its data table, its skills and its icons.

