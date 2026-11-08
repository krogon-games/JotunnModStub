# Status

**Last updated:** 2026-11-08 by the coordinator, from the real state of every branch on GitHub (`git fetch`, then each branch compared with `master`, after PR #53).

**Only the coordinator, working on `master`, edits this file.** Feature branches never touch it, so merges don't clash. Feature sessions report through their handoff file (`docs/handoffs/<branch>.md`).

Related: [`docs/DECISIONS.md`](docs/DECISIONS.md) (every decision you've made) · [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) (the design and where it lives) · the task list: [GitHub issues](https://github.com/krogon-games/JotunnModStub/issues) · [`docs/ROADMAP.md`](docs/ROADMAP.md) (the plan per session).

## In one paragraph

**Everything is in `master`.** Every pull request (up to #53) is merged and no branch has work that `master` lacks. So nothing is waiting to be merged today. What *is* waiting: your play-tests (above all the first evening with the group), and your answers to the questions each session asked before its next round.

## Branches

"Behind" = commits on `master` the branch doesn't have; "Ahead" = the branch's own commits not yet in `master`.

| Branch | Purpose | Stage | Behind / ahead | Ready to merge? |
|---|---|---|---|---|
| `master` | The shared, play-tested base. **Versions: Corruption 0.3.0, CraftingOverhaul 0.2.0, Orlog 0.1.0** (Corruption bumped again 2026-11-08 after PR #55) | — | — | — |
| `claude/admiring-babbage-rk1w8d` | Code-review fixes: safe tab loading, Magic/Rare loot on both death paths, one player at a time on a chest for craft-from-chests, arrows use their own bow's affixes | **Merged** (PR #53, 2026-11-08). Solo play-test passed; multiplayer skipped by you for now | 1 / 0 | Nothing left to merge. Safe to delete |
| `claude/affixes-loot` | Loot: Magic/Rare drops, affixes, orbs, Riftforge crafting rules, uniques | Waiting for your play-test of loot steps 3–4 (merged untested) and your answers on the orb rules | 50 / 0 | Nothing to merge (PRs #11, #13, #18). Its handoff still says PR #18 is open; it was merged 2026-11-02. Safe to delete |
| `claude/class-skills` | Classes (Orlog): levels, attributes, the Orlog stone, the Amazon's skill tree and skills | Asking you questions before C3 (the Amazon's passives) | 32 / 0 | Nothing to merge (PRs #8, #9, #16, #20, #21). Safe to delete |
| `claude/rips-bosses` | Maps, rips, waves, corrupted bosses, server messages | Asking you to approve the map tiers and affixes table (R1) | 25 / 0 | Nothing to merge (PRs #4–#6, #10, #14, #22, #23). Safe to delete |
| `claude/ui-overhaul` | Every screen: character sheet, HUD bars, classes UI, tabs, next the Riftforge window | Asking where the Riftforge window opens; waiting for a small play-test | 43 / 0 | Nothing to merge (PRs #3, #7, #12, #15, #19). Its handoff still says PR #19 is open; it was merged 2026-11-02. Safe to delete |
| `claude/busy-wozniak-c0j9kc` | One-off session that wrote the roadmap (PR #17) | Finished; no handoff file | 59 / 0 | Nothing to merge. Safe to delete |
| `claude/valheim-modding-exploration-izv3l8` | The first session (PRs #1, #2) | Finished; no handoff file | 178 / 0 | Nothing to merge. Safe to delete |
| `claude/awesome-ptolemy-gjigjh` | Build-test session for the cloud setup (2026-11-06) | Finished; handoff merged 2026-11-08 | 20 / 0 | Nothing to merge. Safe to delete |
| `claude/relaxed-einstein-kdvz4t` | This coordinator session | Handoff merged 2026-11-08 | 15 / 0 | Nothing to merge. Keep while the coordinator session is in use |

Every branch above holds nothing `master` lacks (checked 2026-11-08), so deleting any of them loses nothing. Cloud sessions can't delete branches; you do it on GitHub (Branches → the bin icon). New rounds start new branches from `master`.

### Merge notes

- **⚠ `docs/corruption-design.md`: `master`'s version always wins.** These branches carry an **older** copy of it (without your 2026-11-01 decisions: one map per group, shards per tier, scars expire after 3 days): `claude/affixes-loot`, `claude/ui-overhaul`, `claude/busy-wozniak-c0j9kc`, `claude/class-skills`, `claude/valheim-modding-exploration-izv3l8`. Whenever one of them is merged (or work restarts on it), merge `master` into it first; if `docs/corruption-design.md` conflicts or changes, keep `master`'s version and re-apply only genuinely new lines on top. `claude/rips-bosses` already has the current copy.
- **Start every round from the latest `master`** (`git fetch origin master && git merge origin/master`). All the old branches are behind; a session that skips this works on stale docs and code.
- **Version bump at merge (user, 2026-11-06).** Feature branches never change version numbers; their handoff says "gameplay changed: version bump needed at merge" and names the plugins. When the coordinator merges such a branch into `master`, it raises each named plugin's second number once (`0.1.0` → `0.2.0`) in three places that must agree: `<Version>` in the `.csproj`, `PluginVersion` in the main `.cs`, `version_number` in `Package/manifest.json`. Several branches merged in one go share one bump. First bump done 2026-11-08 after PR #53: Corruption and CraftingOverhaul 0.1.0 → 0.2.0 (Orlog unchanged, 0.1.0). Second, 2026-11-08 after PR #55: Corruption 0.2.0 → 0.3.0. The group evening's build check (`PLAYTEST.md` → Session 2) and `PLAYER_SETUP.md` expect these numbers.
- **Files only the coordinator edits:** `STATUS.md`, `docs/DECISIONS.md`. If a feature branch changed them, keep `master`'s version.
- **Shared files that still collide** (add, never rewrite; keep both sides in conflicts): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/mod-compatibility.md`, `Corruption/CorruptionPlugin.cs`, `Corruption/CorruptionConfig.cs`, `Corruption/Content/Texts.cs`.

## Half-finished

Built and merged, but not play-tested (the code is in `master`):
- **Loot steps 3–4** (gathering-tool affixes, on-harvest effects, Odin's Grace on trees, the proc and utility affixes): never tried in game. [#26](https://github.com/krogon-games/JotunnModStub/issues/26)
- **Two dedicated-server fixes** for rips and bosses (PR #22): read in the code, never run on a server. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #53's multiplayer steps** (skipped by you on 2026-11-08): two players on one chest, loot per player, arrows on the server, and a check of a suspected rip-wave undercount when the heart passes to another player's game (needs 3 players). In `PLAYTEST.md` → Session 2, 2e. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **Small leftovers:** the adrenaline bar, one click per button, the build stamp, summons fighting players only, weapon-skill and life-steal affixes, crafting an Odin item, PlantEasily planting from chests. [#27](https://github.com/krogon-games/JotunnModStub/issues/27)
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
2. **Loot steps 3–4**, about an hour alone. [#26](https://github.com/krogon-games/JotunnModStub/issues/26)
3. **The small leftovers.** [#27](https://github.com/krogon-games/JotunnModStub/issues/27)

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

**Today: nothing to merge.** Every branch is already in `master` and there are no open pull requests.

For the next round's pull requests, merge **safest first**:

1. **Coordinator and docs-only PRs.** They change no game code.
2. **Classes** (`claude/class-skills`). Orlog lives in its own folder and plugin; no other session edits its files. Its one risk, the damage patch order with Corruption, is agreed in `docs/mod-compatibility.md` before building.
3. **Loot** (`claude/affixes-loot`). It touches shared Corruption files (`CorruptionPlugin.cs`, `Texts.cs`), and the UI and rips work builds on its Riftforge rules, so it should land before them.
4. **UI** (`claude/ui-overhaul`). The Riftforge window calls the loot rules, so it goes in after them. Its own screens are self-contained.
5. **Rips** (`claude/rips-bosses`) last. Map tiers need a hook in the loot session's `LootPatches.cs` and map rolling in the Riftforge window, so it depends on both.

Before each merge: the branch has merged the latest `master`, `dotnet test CraftingOverhaul.Tests` passes, and you have play-tested it (the round's last step). Merge one PR, then have the next branch merge `master` again before you merge it. After merging gameplay changes, the coordinator raises the version (Merge notes above) before the next release build.
