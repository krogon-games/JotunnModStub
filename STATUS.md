# Status

**Last updated:** 2026-11-08 by the coordinator, from the real state of GitHub (every branch fetched and compared with `master`, open pull requests listed), after PR #57 and the bump of Corruption to 0.4.0 and Orlog to 0.2.0.

**Only the coordinator, working on `master`, edits this file.** Feature branches never touch it, so merges don't clash. Feature sessions report through their handoff file (`docs/handoffs/<branch>.md`).

Related: [`docs/DECISIONS.md`](docs/DECISIONS.md) (every decision you've made) · [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) (the design and where it lives) · the task list: [GitHub issues](https://github.com/krogon-games/JotunnModStub/issues) · [`docs/ROADMAP.md`](docs/ROADMAP.md) (the plan per session).

## In one paragraph

**Everything is in `master`.** Every pull request (up to #57) is merged and none is open. So nothing is waiting to be merged today. The only other branch, `claude/hopeful-thompson-eyncm8`, is merged (PR #57) and can be deleted; every other branch has been deleted on GitHub; their handoff files stay in `docs/handoffs/`. What *is* waiting: your play-tests (above all the first evening with the group), and your answers to the questions each session asked before its next round.

## Branches

"Behind" = commits on `master` the branch doesn't have; "Ahead" = the branch's own commits not yet in `master`.

| Branch | Purpose | Stage | Behind / ahead | Ready to merge? |
|---|---|---|---|---|
| `master` | The shared, play-tested base. **Versions: Corruption 0.4.0, CraftingOverhaul 0.2.0, Orlog 0.2.0** (Corruption and Orlog bumped 2026-11-08 for PR #57) | — | — | — |

Checked 2026-11-08 after PR #57: besides `master`, GitHub has only `claude/hopeful-thompson-eyncm8`, fully merged (0 ahead); please delete it. No open pull requests. Cloud sessions can't delete branches; you do it on GitHub (Branches → the bin icon).

**Areas without a branch now** (their old branches were deleted; each next round starts a new branch from `master`, and its handoff file in `docs/handoffs/` says where it stopped):

| Area | Last handoff | Where it stopped |
|---|---|---|
| Loot (Magic/Rare drops, affixes, orbs, Riftforge rules, uniques) | `claude-affixes-loot.md` | Waiting for your play-test of loot steps 3–4 (partly done through PR #55) and your answers on the orb rules. |
| Classes (Orlog: levels, attributes, the Orlog stone, the Amazon) | `claude-class-skills.md` | Asking you questions before C3 (the Amazon's passives) |
| Rips and bosses (maps, rips, waves, corrupted bosses) | `claude-rips-bosses.md` | Asking you to approve the map tiers and affixes table (R1) |
| UI (character sheet, HUD bars, classes UI, tabs, next the Riftforge window) | `claude-ui-overhaul.md` | Asking where the Riftforge window opens; waiting for a small play-test. |
| Code-review fixes, round 3: corrupted summons during the spawn delay, unreadable affix values, overflow-safe point totals, talisman fight-back across an owner change | `claude-hopeful-thompson-eyncm8.md` | Merged (PR #57, Corruption 0.4.0, Orlog 0.2.0); solo play-test passed (A–D), multiplayer steps left for the group evening (Session 2, 2g) |
| Code-review fixes, round 2: gathering (Seismic and Odin's Grace on your own deposits, Veinbreaker's reset, only your own swings) | `claude-festive-albattani-lg4p35.md` | Merged (PR #55, Corruption 0.3.0); solo play-test passed, multiplayer steps left for the group evening (Session 2, 2f) |
| Code-review fixes, round 1 | `claude-admiring-babbage-rk1w8d.md` | Merged (PR #53); multiplayer steps left for the group evening |
| Coordinator, cloud build test | `claude-relaxed-einstein-kdvz4t.md`, `claude-awesome-ptolemy-gjigjh.md` | Finished |

### Merge notes

- **`docs/corruption-design.md`: `master`'s version always wins.** The branches that carried an older copy are deleted; new branches start from `master` and have the current one.
- **Start every round from the latest `master`** (`git fetch origin master && git merge origin/master`). A session that skips this works on stale docs and code.
- **Version bump at merge (user, 2026-11-06).** Feature branches never change version numbers; their handoff says "gameplay changed: version bump needed at merge" and names the plugins. When the coordinator merges such a branch into `master`, it raises each named plugin's second number once (`0.1.0` → `0.2.0`) in three places that must agree: `<Version>` in the `.csproj`, `PluginVersion` in the main `.cs`, `version_number` in `Package/manifest.json`. Several branches merged in one go share one bump. First bump done 2026-11-08 after PR #53: Corruption and CraftingOverhaul 0.1.0 → 0.2.0 (Orlog unchanged, 0.1.0). Second, 2026-11-08 after PR #55: Corruption 0.2.0 → 0.3.0. Third, 2026-11-08 for PR #57 (done on the branch before merging): Corruption 0.3.0 → 0.4.0, Orlog 0.1.0 → 0.2.0. The group evening's build check (`PLAYTEST.md` → Session 2) and `PLAYER_SETUP.md` expect these numbers.
- **Files only the coordinator edits:** `STATUS.md`, `docs/DECISIONS.md`. If a feature branch changed them, keep `master`'s version.
- **Shared files that still collide** (add, never rewrite; keep both sides in conflicts): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/mod-compatibility.md`, `docs/AFFIX_RULES.md`, `Corruption/CorruptionPlugin.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/Content/Texts.cs`.

## Half-finished

Known bug: **the weapon-skill affix shows one less in the Skills tab** (+6 for a +7 axe, play-test 2026-10-31). Probably only the number on screen (suspected from the game code); the 2-minute check decides. [#56](https://github.com/krogon-games/JotunnModStub/issues/56)

Built and merged, but not play-tested (the code is in `master`):
- **Loot steps 3–4** (gathering-tool affixes, on-harvest effects, Odin's Grace on trees, the proc and utility affixes): partly tried. PR #55's solo play-test (2026-11-08) passed Seismic, Odin's Grace on rock and ore, Veinbreaker's and Bountiful; the rest never tried in game. [#26](https://github.com/krogon-games/JotunnModStub/issues/26)
- **Two dedicated-server fixes** for rips and bosses (PR #22): read in the code, never run on a server. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #53's multiplayer steps** (skipped by you on 2026-11-08): two players on one chest, loot per player, arrows on the server, and a check of a suspected rip-wave undercount when the heart passes to another player's game (needs 3 players). In `PLAYTEST.md` → Session 2, 2e. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #55's multiplayer steps** (left for later by you on 2026-11-08): another player's game running the deposit or log, and a check of a suspected double whole-tree harvest when two players' games each run part of one tree and both swing Odin's Grace at once (not fixed until it's seen; send Claude both players' `Odin's Grace: whole-tree harvest …` lines). In `PLAYTEST.md` → Session 2, 2f. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #57's multiplayer steps** (left for later by you on 2026-11-08): a greydwarf keeps fighting back after its owner logs out; two players offering at one altar give one corrupted boss and use one item. In `PLAYTEST.md` → Session 2, 2g. The corrupted summon copies the game's own summon steps as read in 1.0.14; if 1.0.21 changed them, it isn't picked up. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **Small leftovers:** one click per button, the build stamp, life steal with a melee weapon, crafting an Odin item, PlantEasily planting from chests. [#27](https://github.com/krogon-games/JotunnModStub/issues/27)
- **Multiplayer:** nothing at all has been tried with more than one player. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)

Started but not finished:
- **Riftforge crafting:** the brief is written and two rule pieces exist (corruption 25/25/50, shattering 50% per line), but no orbs, no `co_craft` command and no window yet. [#29](https://github.com/krogon-games/JotunnModStub/issues/29), [#30](https://github.com/krogon-games/JotunnModStub/issues/30)
- **Riftforge window:** mock-ups made; they need lining up with the brief, and your pick of where it opens. [#30](https://github.com/krogon-games/JotunnModStub/issues/30)
- **Currency orbs:** art for nine; no items or drops. [#33](https://github.com/krogon-games/JotunnModStub/issues/33)
- **The Amazon's skills:** points can be spent, but no skill does anything yet. [#32](https://github.com/krogon-games/JotunnModStub/issues/32)
- **Classes UI:** still partly on sample ("demo") data until Orlog serves skills and slots. [#38](https://github.com/krogon-games/JotunnModStub/issues/38)
- **Map tiers and affixes:** your first answers recorded; the numbers table waits for your approval; no code. [#31](https://github.com/krogon-games/JotunnModStub/issues/31)
- **Elite and champion packs:** the rule is decided (above 2 stars), the design isn't started. [#39](https://github.com/krogon-games/JotunnModStub/issues/39)
- **Personal loot:** each player rolls their own, but drops aren't locked to them and there are no loot beams. [#35](https://github.com/krogon-games/JotunnModStub/issues/35)
- **The Orlog stone's look:** works; you want a redesign later. [#47](https://github.com/krogon-games/JotunnModStub/issues/47)

## Waiting on you

Play-tests (only you can run them):
1. **The group evening** on the dedicated server, after the release build. [#24](https://github.com/krogon-games/JotunnModStub/issues/24), [#25](https://github.com/krogon-games/JotunnModStub/issues/25)
2. **Loot steps 3–4**, about an hour alone (the gathering parts PR #55 covered are done). [#26](https://github.com/krogon-games/JotunnModStub/issues/26)
3. **The small leftovers.** [#27](https://github.com/krogon-games/JotunnModStub/issues/27)
4. **The weapon-skill check** (2 minutes: two console commands, `PLAYTEST.md` → Session 3b step 6). It tells whether the +6 you saw for a +7 axe is only a display quirk. [#56](https://github.com/krogon-games/JotunnModStub/issues/56)

Questions, by session (each session asks them at the start of its round):
- **Loot** ([#28](https://github.com/krogon-games/JotunnModStub/issues/28)): Orb of Annulment at 0 affixes (drop a rarity or keep it?); Orb of Chance odds; can the Mirror copy a corrupted item?; Jeweller's Orb greyed until sockets exist? Also confirm seven working defaults for gathering and proc affixes. Later: orb drop rates ([#33](https://github.com/krogon-games/JotunnModStub/issues/33)), the world unique pool ([#34](https://github.com/krogon-games/JotunnModStub/issues/34)).
- **Rips** ([#31](https://github.com/krogon-games/JotunnModStub/issues/31)): approve the map tiers and affixes table (waves per tier, rolling costs, seven affixes with their rewards). Later: elite packs ([#39](https://github.com/krogon-games/JotunnModStub/issues/39)); escalation after repeated corrupted kills, wanted? ([#40](https://github.com/krogon-games/JotunnModStub/issues/40)); the minimum rip distance from bases.
- **Classes** ([#32](https://github.com/krogon-games/JotunnModStub/issues/32)): D2's passive chances unchanged?; Penetrate's chance and whether it breaks immunity; can crits double backstabs?; D2R or older numbers? Then: boss XP multiplier and bonus skill points ([#36](https://github.com/krogon-games/JotunnModStub/issues/36)); eitr regen rate and early eitr potions ([#37](https://github.com/krogon-games/JotunnModStub/issues/37)).
- **UI** ([#30](https://github.com/krogon-games/JotunnModStub/issues/30)): the Riftforge window as its own window or a tab (Claude recommends the tab)? Later: the ship display at the bottom right, and the recipe list redesign you asked to be reminded of ([#43](https://github.com/krogon-games/JotunnModStub/issues/43)).
- **Group:** does `devcommands` work for you on the dedicated server? ([#24](https://github.com/krogon-games/JotunnModStub/issues/24))
- **Your PC:** a fresh `co_dump` from 1.0.21 ([#42](https://github.com/krogon-games/JotunnModStub/issues/42)); the Unity 6 upgrade before any new animation ([#48](https://github.com/krogon-games/JotunnModStub/issues/48)).
- **Parked:** storage and quests ([#46](https://github.com/krogon-games/JotunnModStub/issues/46)); wheel of powers, combat mechanics, a Norse plugin name ([#52](https://github.com/krogon-games/JotunnModStub/issues/52)).

## Top 3 priorities

Following the roadmap's goal for this stretch, "playable together":

1. **Play together.** Make the release build ([#25](https://github.com/krogon-games/JotunnModStub/issues/25)) and run the group evening ([#24](https://github.com/krogon-games/JotunnModStub/issues/24)). Every feature is verified only alone; this is the biggest unknown, and its findings decide what each session fixes first.
2. **Close the loot loop.** Play-test loot steps 3–4 ([#26](https://github.com/krogon-games/JotunnModStub/issues/26)), answer the orb questions ([#28](https://github.com/krogon-games/JotunnModStub/issues/28)), then the Riftforge rules ([#29](https://github.com/krogon-games/JotunnModStub/issues/29)) and window ([#30](https://github.com/krogon-games/JotunnModStub/issues/30)). Until this exists, shards have no use on gear and loot can't be crafted.
3. **Make the Amazon do something.** Answer the four passive questions and build her passives ([#32](https://github.com/krogon-games/JotunnModStub/issues/32)). The group voted for her, and today her skill points have no effect.

Map tiers and affixes ([#31](https://github.com/krogon-games/JotunnModStub/issues/31)) come right after: they are the roadmap's fourth goal.

## Recommended merge order

**Today: nothing to merge.** PR #57 was the last; no pull request is open.

For the next round's pull requests, merge **safest first**:

1. **Coordinator and docs-only PRs.** They change no game code.
2. **Classes** (Orlog). Orlog lives in its own folder and plugin; no other session edits its files. Its one risk, the damage patch order with Corruption, is agreed in `docs/mod-compatibility.md` before building.
3. **Loot**. It touches shared Corruption files (`CorruptionPlugin.cs`, `Texts.cs`), and the UI and rips work builds on its Riftforge rules, so it should land before them.
4. **UI**. The Riftforge window calls the loot rules, so it goes in after them. Its own screens are self-contained.
5. **Rips** last. Map tiers need a hook in the loot session's `LootPatches.cs` and map rolling in the Riftforge window, so it depends on both.

Before each merge: the branch has merged the latest `master`, `dotnet test CraftingOverhaul.Tests` passes, and you have play-tested it (the round's last step). Merge one PR, then have the next branch merge `master` again before you merge it. After merging gameplay changes, the coordinator raises the version (Merge notes above) before the next release build.
