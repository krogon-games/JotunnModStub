# Roadmap: the next stretch of development

**Written:** 2026-11-01, after every session's branch was merged into `master` (PRs #1–#16; 225 tests pass). **For:** the user and the next round of Claude sessions. Each session reads its own part below before starting, then follows `CLAUDE.md` (branch from the latest `master`, one branch per session, PR when a feature is finished).

**Update 2026-11-16 (classes session, `claude/magical-babbage-slwwpn`, [#70](https://github.com/krogon-games/JotunnModStub/issues/70)):** Krogon had no time to play-test and asked to use the five-hour session limit: "Start building out the other Amazon skills use sub agents." He chose "Only three bow skills": **Strafe, Immolation Arrow and Venom Strike are built** (75% of the shot per Strafe arrow; Immolation Arrow's blast 25% → 100% and burning 10% → 30% a second; Venom Strike's poison 25% → 100%, eitr 7 + 0.5 per level), untested in-game. Snipe and Shackle Shot wait for another round and still say "Coming later"; #70 stays open for them. A second play-test round is written (14 steps, `r70-1` … `r70-14`, `docs/playtests/claude-magical-babbage-slwwpn.md` and the phone page). 593 tests. `FEATURES.md` → Amazon skill bar and bow skills.

**Update 2026-11-16 (classes session, `claude/magical-babbage-slwwpn`, #72):** skill damage is remade with Krogon's decisions of that day: every damage skill deals a share of the shot's own damage (bow + arrow) that grows with skill level, and Diablo 2's fixed damage points and the damage scale table are gone. Freezing Arrow's blast is 50% → 150%, each of Forked Lightning's forks 25% → 100%; Multishot and Wind Lance already worked this way. Built, waiting for his play-test. **The skills still to build (#70, #41, #44) take a share of the weapon's damage too, agreed in their own rounds.**

**Update 2026-11-16 (coordinator, after PR #71):** Forked Lightning and Wind Lance are merged, after passing Krogon's single-player play-tests the same day (Wind Lance in its reworked form; the classes session's note below was written before that). Orlog is at 0.5.0 (Corruption 0.7.0, CraftingOverhaul 0.5.0). #70 stays open for the five bow skills left (Krogon: "keep #70 open"). New: **#72, skill damage** (C5c): the balancing pass Krogon put off, and his note that all skills' damage may be remade, perhaps scaling off the equipped weapon; nothing is decided, and the damage numbers of the skills still to build (#70, #41, #44) would follow it.

**Update 2026-11-16 (classes session, `claude/jolly-albattani-4fruv2`, #70):** Forked Lightning and Wind Lance are built with Krogon's numbers of that day (Charged Strike's and Guided Arrow's; `FEATURES.md` → Amazon skill bar and bow skills) and wait for his play-test (the same day: Forked Lightning works but is too strong at level 1, balancing later; Wind Lance failed and was reworked: it flies itself along the ground as a gust, with a 1.5 s hold). Five bow skills are left in C5b: Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot.

**Update 2026-11-15 (coordinator, after PR #68):** the skill tree overhaul is merged and #66 is closed; Orlog is at 0.4.0, CraftingOverhaul at 0.5.0 (Corruption 0.7.0). Multiple Shot is now called **Multishot** (Krogon, the same day; the note below was written before the rename). New: **#70, the seven bow skills still to build** (Forked Lightning and Wind Lance first, then Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot); it is the next classes round and takes Strafe and Immolation Arrow over from C7. C6 (#41) follows it with the spear tab's nine (Power Strike is out). C7 (#44) keeps Decoy, Valkyrie, Pierce, Fend, Charged Strike, Lightning Strike and Lightning Fury (Guided Arrow and Slow Missiles are gone). `AGENTS.md` is new: how a round's work is split across subagents, only when Krogon asks for it.

**Update 2026-11-15 (classes session, `claude/laughing-mccarthy-fnz0to`, #66):** the skill tree overhaul is built and passed Krogon's single-player play-test (all 15 steps): no level tiers or prerequisites, exactly nine skills per tab for every class, the skills panel as skill cards. The bow tab is Krogon's list (Forked Lightning, Wind Lance, Snipe, Multiple Shot, Strafe, Freezing Arrow, Immolation Arrow, Venom Strike, Shackle Shot); only Multiple Shot and Freezing Arrow work. **Next for classes:** build the other seven bow skills, a few per round, each after its details are agreed; C6 (#41) after that, with a spear tab of nine (Power Strike is out). #44's list changes with it: Guided Arrow and Slow Missiles are gone.

**Update 2026-11-15 (coordinator, after PRs #64 and #65):** C3, C5 and C4's boss half are done: the Amazon's five passives, the skill button with four slots on F1–F4, the bow skills, Inner Sight, quick cast, a skill point for each boss's first kill and boss XP from Diablo 2's act bosses (passed alone, two players left for the group evening; #32, #36, #37 closed). U2 is done too: the classes UI demo is removed (#38 closed). CraftingOverhaul's biome detection no longer freezes the game after spawning (PR #65). Orlog is at 0.3.0, CraftingOverhaul at 0.4.0. Skill use changed from §4.1's plan by Krogon's decisions (`DECISIONS.md`, 2026-11-12 and 2026-11-13): right click fires every chosen skill, spear and javelin skills too; quick cast per skill; the game's throw is untouched and a new basic skill, Gungnir's Throw, comes with C6. **Next for classes: the skill tree overhaul (#66)**, which replaces parts of `class-design.md` §4–§5 once agreed; then C6 (#41). The respec item is split off as #67 (low priority).

**Update 2026-11-11 (coordinator, after PR #61):** L5 is done: corrupting gear with a shard, shattering and Affix Splinters work in the Forge tab (passed alone, multiplayer left for the group evening; #29 closed; Corruption is at 0.7.0). The rules follow Krogon's round brief and answers of 2026-11-10 (`DECISIONS.md`). New: #60, a detailed character stats sheet that shows the stat caps. Next for loot: L4's orb drops (#33).

**Update 2026-11-10 (coordinator, after PR #59):** U3 is done: the Riftforge window became a **Forge tab** on the Riftforge panel, with six inventory tabs (Bag · Currencies · Splinters · Maps · Runes · Essences); passed alone, multiplayer left for the group evening; #30 closed; Corruption is at 0.6.0, CraftingOverhaul at 0.3.0. Its design follows Krogon's round brief of 2026-11-09 (`DECISIONS.md`), which replaced the two-slot, preview-and-confirm window described in U3 below. L5's remaining part (#29: shard corruption, shattering, Affix Splinters) plugs into that tab.

**Update 2026-11-08 (coordinator, after PR #58):** every PR up to #58 is merged; loot L2 and L3 were built (PR #18) and L2's review fixes were play-tested alone (PR #55); a third set of review fixes passed alone (PR #57); the orb rules were decided (#28, closed) and L5's first part, the currency orbs and their Riftforge rules with `co_craft`, passed alone (PR #58); Corruption is at 0.5.0, Orlog at 0.2.0. The old session branches are deleted on GitHub, so each session's next round starts a **new** branch from `master` (the branch names in §3 and §4 are where the earlier rounds ran). Current state of each: `STATUS.md`.

The facts here come from `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `FEATURES.md`, `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/AFFIX_RULES.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`, `docs/DECISIONS.md` and the code. Where this file and those disagree, those win: fix this file.

## Task list (2026-11-05)

The tasks below are tracked as **[GitHub issues](https://github.com/krogon-games/JotunnModStub/issues)**, labelled by area (`loot`, `UI`, `rips`, `maps`, `classes`, `multiplayer`, `release`, `housekeeping`, `storage`, `design`), priority (`priority: 1 high`, `priority: 2 medium`, `priority: 3 later`) and `needs decision` / `needs playtest`. The issue is the place to tick progress; this table only maps the roadmap's IDs to them. Priorities and what's waiting on the user: [`STATUS.md`](../STATUS.md).

| Priority | Issues |
|---|---|
| 1 high | #24 group evening · #25 release build · #26 play-test loot steps 3–4 · #27 small play-test leftovers · ~~#28 orb rules (decision)~~ closed · ~~#29 L5 Riftforge rules~~ closed (PR #58, PR #61) · ~~#30 U3 Riftforge window~~ closed (PR #59) · #31 R1 map tiers and affixes · ~~#32 C3 Amazon passives~~ closed (PR #64) · ~~#66 the Amazon's skill tree overhaul~~ closed (PR #68) |
| 2 medium | #33 L4 orbs · #34 L6 uniques and ring slot · #35 L7 loot lock and beams · ~~#36 C4 boss points and respec~~ closed (PR #64; the respec item is #67) · ~~#37 C5 skill button and bow skills~~ closed (PR #64) · ~~#38 U2 real Orlog data~~ closed (PR #64) · #39 R3 + R2 elites and 3 stars · #40 R5 landing check and particles · #41 C6 spear and javelin skills · #42 fresh `co_dump` · #60 character stats sheet (caps) · #70 the bow skills still to build (five left; Forked Lightning and Wind Lance done, PR #71) · #72 skill damage: a balancing pass, maybe a remake |
| 3 later | #43 U4 recipe list · #44 C7 later Amazon skills · #45 R6 phase 8 · #46 storage and quests · #47 Orlog stone redesign · #48 Unity 6 · #49 L8 synced tables · #50 before a public release · #51 other classes · #52 parked ideas · #67 the respec item |

## 1. Where we stand

| Track | Built and verified in single player | Not built |
|---|---|---|
| CraftingOverhaul | Recipe search and categories, biome detection, craft and build from chests, character sheet, HUD bars, classes UI (on Orlog's data since 2026-11-13), `co_dump` | Riftforge crafting window (see §4), recipe list redesign (user's to-do) |
| Corruption: core loop | Shards, Riftforge, talismans, essences, maps and rips (A and B), heart search, corrupted offerings, bosses and summons, Currency and Maps tabs, Odin unlock | Phase 5 (map tiers and affixes, 3-star), phase 8 (scars, terror zones, elites, Svartalfheim), boss particles |
| Corruption: loot (phase 7) | Magic and Rare drops, names, tooltips, 28 item-stat and player-stat affix effects; gathering: Seismic, Odin's Grace on rock and ore, Veinbreaker's, Bountiful (2026-11-08, PR #55). The other gathering and the proc affix effects are built (PR #18) but untested ([#26](https://github.com/krogon-games/JotunnModStub/issues/26)) | Orbs, shard corruption and shattering at the Riftforge, Affix Splinters, uniques, ring slot, personal-loot locks, loot beams |
| Orlog | Levels and XP, attributes, class eitr pool, `Orlog.Api` for the character | Orlog stone, skill trees, every skill, skill button and slots, respec item, boss bonus points, eitr potions |

**Untested anywhere:** multiplayer, for every feature (`PLAYTEST.md` → Session 2; PR #55's gathering checks, including a suspected double whole-tree harvest with Odin's Grace, are step 2f). Small single-player leftovers: weapon-skill and life-steal affixes, crafting an Odin item, PlantEasily bulk planting from chests (the adrenaline bar and the corrupted-summons re-check passed 2026-10-31).

**Already settled with the user** (don't reopen): see `DECISIONS.md`. In short: own loot system, own levels, D2 rules in config, the Amazon first, the HUD and panel look, every affix range, the tabs, EquipmentAndQuickSlots and AdventureBackpacks as dependencies.

## 2. The goal of this stretch: playable together

Everything so far was built and checked alone. The next stretch makes the mods something the four friends can play on the server, in this order of importance:

1. **The group plays what exists.** Only the user can run the multiplayer session; the sessions prepare it (§6 Housekeeping) and fix what it finds.
2. **Close the loot loop.** Shards have no gear sink yet and the Riftforge makes only maps, offerings and the wishbone. Orbs, corruption, shattering and uniques turn drops into a crafting game. Needs one new screen, the Riftforge window.
3. **Make the Amazon playable.** The group voted for her. The stone, the tree, her passives, then the skill button and the bow skills.
4. **Deepen rips.** Map tiers and affixes are the shard sink on the map side, and the step where 3-star creatures and elite packs arrive.

**Deferred past this stretch:** phase 8 (scars, terror zones, roaming elites, Svartalfheim), storage and quests, combat mechanics, the wheel of powers, changing class at the stone, server-synced data tables, Norse renaming. **Other classes are extra content after everything above** (§8).

## 3. Sessions

Four sessions, the same areas as before, each started fresh from `master` (the branches below were deleted after their work was merged; a new round starts a new branch):

| Session | Branch | Owns | Start with |
|---|---|---|---|
| Classes | `claude/class-skills` | `Orlog/` | The Orlog stone and class choice |
| Loot | `claude/affixes-loot` | `Corruption/` loot, orbs, Riftforge crafting rules | The Riftforge window brief, then the gathering affix effects |
| Rips | `claude/rips-bosses` | `Corruption/` rips, bosses, server messages | Map tiers and affixes |
| UI | `claude/ui-overhaul` | `CraftingOverhaul/` screens; the Riftforge window | Play-test fixes, then the Riftforge window |

Rules that keep four sessions from colliding:
- Stay in your files. `CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Texts.cs`, `FEATURES.md`, `CURRENT_STATE.md` and `PLAYTEST.md` are shared: add, never rewrite, merge `master` often, keep both sides in conflicts. `STATUS.md` and `DECISIONS.md` are edited only by the coordinator on `master` (2026-11-05); sessions record decisions in their handoff.
- A game method several sessions patch (`Character.Damage`, `Character.RPC_Damage`, `Player.GetTotalFoodValue`, `SEMan.ModifyAttack`, `Character.OnDeath`) gets its new patch recorded in `docs/mod-compatibility.md` with its Harmony priority, so the order is deliberate.
- Contracts between sessions are written down before either side builds (`classes-ui-brief.md` is the model). Changes go into the brief in `master`.
- Every PR keeps `dotnet test CraftingOverhaul.Tests` green and updates `FEATURES.md` and `PLAYTEST.md` in the same commit.

## 4. Session plans

### 4.1 Classes (`claude/class-skills`, Orlog)

Follows `class-design.md` §8 (build order). Step 1 (levels, attributes) is done except the respec item and the boss bonus points.

| # | Task | Done when | Notes |
|---|---|---|---|
| C1 | **Orlog stone and class choice** (`class-design.md` §5a). **Done 2026-11-02: works after three play-tests; its look will be redesigned later (user)** (`FEATURES.md` → Orlog stone) | A character with no class uses the stone at the spawn area, the existing class choice window opens (no demo), the Amazon is chosen and kept after relogging; a classless character gets a raven hint once | Stone cloned from the start area's own stone prefabs, placed once per world beside the trophy stones as a persistent object (as the rip heart is), position from the world's `StartTemple` location. Fill `GetClassStoneRequest` and `ChooseClass`. `orlog_class` stays as a cheat. Changing class: later (user) |
| C2 | **Skill tree data and rules** (`class-design.md` §4). **Built 2026-11-01, tested 2026-11-02** (`FEATURES.md` → Skill tree and skill points); the per-level data gap for C3 is filled (`data/d2-reference/amazon_levels.csv`) | The skills panel shows the Amazon's real tree, spending works and refusals give their reason, points survive relogging, `orlog_respec` returns skill points too | `Orlog/Data/amazon_skills.csv` made by `tools/make_orlog_tables.py` from `data/d2-reference/amazon.csv` with the statuses of `class-design.md` §5; pure `SkillRules` (level tiers 1/6/12/18/24/30, prerequisites count only while built, 20 points, one point per level past the required level, synergies from hard points, effective level); `GetSkillIdsJson`, `GetSkillInfoJson` with the description text and numbers filled in, `SpendSkillPoint`. `GetSkillIcon` may stay null: the UI has placeholder emblems |
| C3 | **Amazon passives**: Critical Strike, Dodge, Avoid, Evade (and Penetrate). **Built 2026-11-11 on `claude/lucid-albattani-imrei7` with Krogon's answers; passed alone 2026-11-15, merged in PR #64** (`FEATURES.md` → Amazon passives) | Each shows its chance in the tooltip and works in a fight; a test covers the rules | Crits are a new mechanic (double physical damage on the attacker's game); the avoids run on the player's own game. `Character.Damage` and `RPC_Damage` already carry Corruption's prefixes: agree the order in `mod-compatibility.md`. **Data gap:** filled (`data/d2-reference/amazon_levels.csv` → `Orlog/Data/amazon_passives.csv`) |
| C4 | **Boss bonus points and the respec item**. **Boss points built 2026-11-11** (`claude/lucid-albattani-imrei7`; 12 in all, Krogon's numbers); **boss XP built 2026-11-15** (D2's act bosses, ×9.4 Eikthyr … ×66.6 Kall; Krogon approved); **the respec item is low priority, a future feature** (Krogon, 2026-11-15; #67). Boss points and boss XP passed alone on Eikthyr 2026-11-15, merged in PR #64 | A boss's first kill gives the configured points; a craftable item resets points | Decided rules (D2's quest points → each boss's first kill) with no numbers yet: ask the user. Respec item: with Corruption, from boss essences (D2's Token); without, a plain recipe |
| C5 | **Skill button, slots and the bow skills** (`class-design.md` §4). **Built 2026-11-11 on `claude/lucid-albattani-imrei7`; passed alone 2026-11-15 apart from the skills the overhaul will rework (#66), merged in PR #64; the damage scale is kept for now** (`FEATURES.md` → Amazon skill bar and bow skills) | Right click fires the chosen skill with any weapon, block has moved to a free rebindable key, the four slots have hotkeys and show cost, cooldown and "need bow"; Magic Arrow shoots without an arrow; then Fire, Cold, Multiple, Exploding, Ice and Freezing Arrow and Inner Sight | Prototype the input first (drawing a bow on a non-attack button). Default keys read from the game's and the group's mods' bindings. Fill `GetSkillBarJson`, `GetSlotStateJson`, `SetSkillSlot`. Cooldowns as status effects. Costs in eitr |
| C5b | **The seven bow skills still to build** (#70, new 2026-11-15, after the skill tree overhaul, #66, PR #68): Forked Lightning and Wind Lance first, then Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot. **The next classes round.** **Forked Lightning and Wind Lance done 2026-11-16** (`claude/jolly-albattani-4fruv2`; passed alone, merged in PR #71, Orlog 0.5.0; two players left for the group evening). **Five left** until later the same day: **Strafe, Immolation Arrow and Venom Strike built 2026-11-16** (`claude/magical-babbage-slwwpn`; untested in-game); **two are left** (Snipe, Shackle Shot) | Each works from right click, shows its numbers in its card and tooltip, and is play-tested on its own; "Coming later" is gone from the bow tab | Ask Krogon first: each skill's cost, damage and range, Wind Lance's and Snipe's charge, Shackle Shot against Dota 2's. Exploding Arrow's removed blast code (last in commit `6090e71`) is the start for Immolation Arrow |
| C5c | **Skill damage: a balancing pass, maybe a remake** (#72, new 2026-11-16, after PR #71). Forked Lightning is far too strong at level 1, "so are a few other skills as well" (Krogon); "we may do a remake on the damage of all skills, perhaps to make the skills scale off the weapon equiped". Carries Forked Lightning's last two play-test steps | Krogon is happy with skill damage at low and high levels; the tables and the docs follow what he decided | Ask Krogon first: which skills are too strong; a balancing pass or a remake; whether skills scale off the equipped weapon (Wind Lance already uses the arrow's own damage); before or between the #70 and #41 rounds. ~~Nothing decided yet~~ **Decided and built 2026-11-16 (`claude/magical-babbage-slwwpn`): a remake, "All scale off the weapon"; Freezing Arrow's blast 50% → 150% and Forked Lightning's forks 25% → 100% of the shot's damage; waiting for his play-test** |
| C6 | **Spear and javelin skills** (#41). The spear tab is nine since the overhaul (2026-11-15): Jab, Impale, Fend, Charged Strike, Lightning Strike, Poison Javelin, Lightning Bolt, Plague Javelin, Lightning Fury; Power Strike is out (Krogon); the rest can still change in this round | Gungnir's Throw (the basic ghost throw, no skill points); Jab, Impale; Poison Javelin, Lightning Bolt, Plague Javelin | Javelins throw a spectral copy (decided); skills fire from right click (Krogon, 2026-11-12) |
| C7 | Later: the Amazon's `later` skills that aren't bow skills (#44): Pierce, Decoy, Valkyrie, Charged Strike, Fend, Lightning Strike, Lightning Fury. Guided Arrow and Slow Missiles left the lists in the overhaul (2026-11-15); Strafe and Immolation Arrow moved to #70 | Each one play-tested on its own | Decoy and Valkyrie need the player-clone prototype (`class-design.md` §5). Only the Amazon is worked on in this stretch; other classes are extra content (§8) |

**Ask the user before C3–C5:** how D2 skill values scale to Valheim (propose per skill from the data, as the attributes were); boss XP (today a boss counts as one creature of its level; D2 gives 9–65×); eitr regen without eitr food (vanilla's 5 per second today: keep or set); bonus points per boss first kill; whether Penetrate also breaks immunity; eitr potions for the early biomes (materials, and that they belong to Orlog).

**Meets:** the UI session through `classes-ui-brief.md` §3 (add to its table in `master`; the `needs` field is already an addition); the loot session later for "+N to skills" affixes and the respec token recipe; the rips session on whether maddened and rip creatures give more XP (the design says corrupted creatures give more; `KillXp` only reads stars today).

### 4.2 Loot (`claude/affixes-loot`, Corruption phase 7)

| # | Task | Done when | Notes |
|---|---|---|---|
| L1 | **The Riftforge window brief**, [`docs/riftforge-ui-brief.md`](riftforge-ui-brief.md), written first (**draft written 2026-11-01**; open questions in its §6) | The UI session can start the window from it | A short document, like `classes-ui-brief.md`: for an item put into the window, the operations the rules allow and why one is refused, what each shows before confirming (the new lines, the shard's 25/25/50 chances), the result message, and the plain C# calls the screen makes into the rules (same plugin, no reflection). Operations: apply an orb (Transmutation … Mirror, `corruption-design.md` → Currency orbs), corrupt with a shard (`CorruptionOutcomes`, built), shatter (`ShatterRules`, built) into Affix Splinters, apply a splinter, and the rips session's map rolling (R1). Done first so the UI session isn't waiting on L2–L4 |
| L2 | **Gathering affix effects**: the 33 approved rows still shown as "(not active yet)" (universal, mining, woodcutting pools) (**built 2026-11-01**, PR #18; review fixes in PR #55, 2026-11-08: Seismic and Odin's Grace on your own deposits, Veinbreaker's reset on any unequip, only your own swings get the tool's affixes; passed alone, multiplayer in Session 2, 2f) | No gathering line is grey; each has a `PLAYTEST.md` step | Rules in `AFFIX_RULES.md` → Interpretations and Multiplayer: hit changes on the striker's game, extra drops by an owner-to-striker message. Mod compatibility: AdventureBackpacks' `GetWeight` (Light: multiply), EquipmentAndQuickSlots' `AutoPickup` (Magnetic, Keen-eyed), PlantEasily bulk harvests |
| L3 | **Proc and utility affixes**: Shieldwall, Rallying, Skald's, Steadfast, Reprisal, Wrath, Skadi, Fenrir, Bloodforged, Deathless, Foraging (**built 2026-11-01**, PR #18; untested apart from Skadi, passed 2026-11-08 through PR #53) | Same | `AFFIX_RULES.md` → Proc and utility affixes. Deathless with EquipmentAndQuickSlots' keep-on-death (`Character.CheckDeath`); ally buffs through the game's status-effect message; kills reported back to the killer as for harvests |
| L4 | **Currency orbs** | The design's ten orbs exist, drop from their sources, land in the Currency tab and stack | Art is made for nine (`Corruption/Assets/Orbs/`, `tools/make_orbs.py`; the Mirror of Kalandra has none yet; add `Assets\Orbs\*` to the csproj). New table `orbs.csv` (effect, sources, rates). Add their prefab names to `TabRules.TabFor` |
| L5 | **Riftforge crafting rules** behind the brief | Pure, tested rules for every operation in L1; the window calls them | The Affix Splinter item: one item, affix and value in custom data, stack 1. Until the window exists, a console command exercises the rules |
| L6 | **Uniques and the ring slot** | Boss uniques drop from their boss, world uniques from the biome-gated pool, with orange names and fixed lines; rings have a slot | `uniques.csv` from the design's six boss uniques; the world pool's contents are an open question for the user. Item level = biome (as `loot_bases.csv`). Only Divine and Jeweller's orbs touch them; never shattered or corrupted. The ring slot comes through EquipmentAndQuickSlots' custom-slot API, as the talisman slots do (three boss uniques are rings) |
| L7 | **Personal-loot lock (60 s) and loot beams** | A drop is the roller's for a minute; Rares and uniques have a pillar of light and a sound | Vanilla lights and particles |
| L8 | Later: server-synced data tables (the design wants the server's copy to rule) | | |

**Ask the user:** orb drop sources and rates (open question in `GAME_DESIGN.md`); the world unique pool's contents.

**Meets:** the UI session (L1's brief, L5's rules); the rips session (map rolling as a Riftforge operation; `rip_tier_*` drop modifiers; unique chances in rips); the classes session ("+N to skills" affixes once trees exist; the respec token from essences).

### 4.3 Rips and bosses (`claude/rips-bosses`, Corruption phases 5 and 8, server messages)

| # | Task | Done when | Notes |
|---|---|---|---|
| R1 | **Map tiers and affixes (phase 5)** | A map rolled at the Riftforge shows its tier and affixes in its tooltip; a tier-4 Rabid Swarming map is the near-death run the design describes | `corruption-design.md` → Map affixes and tiers: tiers 1–4 by extra shards (tier 4 plus a boss trophy and a maddened mini-boss), the seven affixes each reusing a vanilla status effect, with their danger and reward; affix data on the map item's custom data; tier feeds `drop_modifiers.csv` (`rip_tier_2`–`4` exist) and the shard yield. New table `map_affixes.csv`. Rolling happens in the Riftforge window (L1/U3): agree the operation with the loot session; until the window exists, roll from a console command |
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
| U1 | **Fixes from the 2026-10-31 play-tests** | The + buttons play one click sound; the adrenaline bar is checked (`PLAYTEST.md` → Classes UI; passed 2026-10-31); the "check in-game" layout items in `mod-compatibility.md` are ticked with Orlog's real data | |
| U2 | **Real data instead of the demo**. **Done 2026-11-13 on `claude/lucid-albattani-imrei7`:** Orlog serves everything (class description and tab names added), the demo is removed | Each screen works from `Orlog.Api` as the classes session fills it (stone request, skills, slots); the demo branches and `co_classes_stone` go on the day Orlog serves everything | Contract: `classes-ui-brief.md` §3. Placeholder icons stay until Orlog returns sprites or art is asked for |
| U3 | **Riftforge window** (in the Corruption plugin, new files under `Corruption/Content/`). **Done 2026-11-10 (PR #59)** as the Forge tab; what follows is the original plan | From the loot session's `docs/riftforge-ui-brief.md`: put an item in, see the operations the rules allow, a preview, confirm, the result | **Decided 2026-11-01 (Claude, the user delegated it):** this session builds the screen, as it built the Currency and Maps tabs inside Corruption; the loot session owns the rules behind it (L5) and writes the brief first (L1); the rips session adds map rolling (R1). Why: this session has the mock-up → layout class → preview pipeline and the user's style choices, and has room now, while the loot session's list is long and the window would otherwise hold up orbs, corruption, shattering and map rolling. It lives in Corruption because everyone has Corruption and not everyone has CraftingOverhaul, so it can't use CraftingOverhaul's embedded art: copy the game's own panel sprite at runtime (as `StyleAsGamePanel` does) and embed only the few sprites it needs in Corruption. Mock-up first from a tested layout class (`SkillsPanelLayout` → `tools/preview_skills_panel.py` pattern); the user picks. Where it opens (a tab on the Riftforge's crafting panel, or a window when using the station) is this session's call. Keep to its own files; the one hook in `CorruptionPlugin.cs` is the expected merge point |
| U4 | **Recipe list redesign and the crafting screen with AdventureBackpacks' backpack panel open** (user's to-do since 2026-10-28) | | Ask what they want changed before starting |
| U5 | Art on request: class and skill icons, unique and map-affix icons | | Only when the user asks (`CLAUDE.md`); orb icons exist |

**Meets:** every other session (the window serves loot and rips; the screens serve Orlog). Don't touch rules in `Orlog/` or `Corruption/Rules/`.

## 5. Dependencies between sessions

- C1 → U2 (the real stone replaces `co_classes_stone`). C2 → U2 (the real tree). C5 → U2 (real slot states).
- L1 (brief) → U3 (window, against L5's rules) → L4, L5 and R1 usable in game. Until then: console commands.
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

