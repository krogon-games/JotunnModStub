# Status

**Last updated:** 2026-11-11 by the coordinator (branch `claude/relaxed-cray-8dkrr2`), from the real state of GitHub (branches and open pull requests listed, the 28 open issues read, tests run), after PR #61 and the bump of Corruption to 0.7.0. This update: the top 3 priorities you agreed, the phone play-test page brought in line, and a play-test you can run alone on the dedicated server.

**Only the coordinator, working on `master`, edits this file.** Feature branches never touch it, so merges don't clash. Feature sessions report through their handoff file (`docs/handoffs/<branch>.md`).

Related: [`docs/DECISIONS.md`](docs/DECISIONS.md) (every decision you've made) · [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) (the design and where it lives) · the task list: [GitHub issues](https://github.com/krogon-games/JotunnModStub/issues) · [`docs/ROADMAP.md`](docs/ROADMAP.md) (the plan per session).

## In one paragraph

**Everything is in `master`.** Every pull request (up to #61) is merged and none is open. The last round's branch, `claude/brave-einstein-xz357h` (PR #61: shard corruption, shattering and Affix Splinters at the Riftforge), is merged and deleted; the handoff files stay in `docs/handoffs/`. The only branch besides `master` is the coordinator's `claude/relaxed-cray-8dkrr2` (documents only: this file, the decisions, the play-test checklist). What *is* waiting: your play-tests (above all the first evening with the group), and your answers to the questions each session asked before its next round.

## Branches

"Behind" = commits on `master` the branch doesn't have; "Ahead" = the branch's own commits not yet in `master`.

| Branch | Purpose | Stage | Behind / ahead | Ready to merge? |
|---|---|---|---|---|
| `master` | The shared, play-tested base. **Versions: Corruption 0.7.0, CraftingOverhaul 0.3.0, Orlog 0.2.0** (Corruption bumped 2026-11-11 for PR #61, CraftingOverhaul 2026-11-10 for PR #59, Orlog 2026-11-08 for PR #57) | — | — | — |
| `claude/relaxed-cray-8dkrr2` | Coordinator: status review, the phone play-test page, the solo-on-server checklist. Documents only, no game code | Ready once you merge its pull request | 0 behind; ahead by its own document commits | Yes: no play-test needed, no version bump |

Checked 2026-11-11: no open pull requests; `claude/brave-einstein-xz357h` is deleted on GitHub, so `master` and the coordinator's branch are the only ones. Cloud sessions can't delete branches; you do it on GitHub (Branches → the bin icon).

**Areas without a branch now** (their old branches were deleted; each next round starts a new branch from `master`, and its handoff file in `docs/handoffs/` says where it stopped):

| Area | Last handoff | Where it stopped |
|---|---|---|
| Loot (Magic/Rare drops, affixes, orbs, Riftforge rules, uniques) | `claude-brave-einstein-xz357h.md` (before it `claude-loving-euler-qfkpzc.md`, `claude-affixes-loot.md`) | Merged (PR #61, Corruption 0.7.0): corrupting gear with a shard, shattering, Affix Splinters with their own models, one confirmation dialog for both with a remembered "Don't warn me again", corrupted affixes beyond the caps (#29, closed); passed alone, multiplayer left for the group evening (Session 2, 2i). The Riftforge loop is complete apart from drops. Next round: L4's orb drops (needs your rates, #33); the Stonecutter's/Resinous rework; later uniques (#34). Loot steps 3–4 still wait for your play-test (#26) |
| Classes (Orlog: levels, attributes, the Orlog stone, the Amazon) | `claude-class-skills.md` | Asking you questions before C3 (the Amazon's passives) |
| Rips and bosses (maps, rips, waves, corrupted bosses) | `claude-rips-bosses.md` | Asking you to approve the map tiers and affixes table (R1) |
| UI (character sheet, HUD bars, classes UI, six inventory tabs, the Riftforge's Forge tab) | `claude-lucid-carson-hs5g89.md` (before it `claude-ui-overhaul.md`) | Merged (PR #59, Corruption 0.6.0, CraftingOverhaul 0.3.0): six tabs (Bag · Currencies · Splinters · Maps · Runes · Essences), the Forge tab for orb crafting, the same affix layout in every tooltip, the Skills button removed (#30, closed); passed alone (A–E), multiplayer left for the group evening (Session 2, 2h); its checklist F was covered by #29's play-test (PR #61). Open: where the skills panel's button goes; the detailed character stats sheet that shows the caps (#60) |
| Code-review fixes, round 3: corrupted summons during the spawn delay, unreadable affix values, overflow-safe point totals, talisman fight-back across an owner change | `claude-hopeful-thompson-eyncm8.md` | Merged (PR #57, Corruption 0.4.0, Orlog 0.2.0); solo play-test passed (A–D), multiplayer steps left for the group evening (Session 2, 2g) |
| Code-review fixes, round 2: gathering (Seismic and Odin's Grace on your own deposits, Veinbreaker's reset, only your own swings) | `claude-festive-albattani-lg4p35.md` | Merged (PR #55, Corruption 0.3.0); solo play-test passed, multiplayer steps left for the group evening (Session 2, 2f) |
| Code-review fixes, round 1 | `claude-admiring-babbage-rk1w8d.md` | Merged (PR #53); multiplayer steps left for the group evening |
| Coordinator, cloud build test | `claude-relaxed-einstein-kdvz4t.md`, `claude-awesome-ptolemy-gjigjh.md` | Finished |

### Merge notes

- **`docs/corruption-design.md`: `master`'s version always wins.** The branches that carried an older copy are deleted; new branches start from `master` and have the current one.
- **Start every round from the latest `master`** (`git fetch origin master && git merge origin/master`). A session that skips this works on stale docs and code.
- **Version bump at merge (user, 2026-11-06).** Feature branches never change version numbers; their handoff says "gameplay changed: version bump needed at merge" and names the plugins. When the coordinator merges such a branch into `master`, it raises each named plugin's second number once (`0.1.0` → `0.2.0`) in three places that must agree: `<Version>` in the `.csproj`, `PluginVersion` in the main `.cs`, `version_number` in `Package/manifest.json`. Several branches merged in one go share one bump. First bump done 2026-11-08 after PR #53: Corruption and CraftingOverhaul 0.1.0 → 0.2.0 (Orlog unchanged, 0.1.0). Second, 2026-11-08 after PR #55: Corruption 0.2.0 → 0.3.0. Third, 2026-11-08 for PR #57 (done on the branch before merging): Corruption 0.3.0 → 0.4.0, Orlog 0.1.0 → 0.2.0. Fourth, 2026-11-08 for PR #58 (on the branch before merging): Corruption 0.4.0 → 0.5.0. Fifth, 2026-11-10 for PR #59 (on the branch before merging): Corruption 0.5.0 → 0.6.0, CraftingOverhaul 0.2.0 → 0.3.0. Sixth, 2026-11-11 for PR #61 (on the branch before merging): Corruption 0.6.0 → 0.7.0. The group evening's build check (`PLAYTEST.md` → Session 2) and `PLAYER_SETUP.md` expect these numbers.
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
- **PR #58's leftovers** (orbs passed alone 2026-11-08): the "Use this at a Riftforge" refusal more than 10 m away, skipped by you until the Riftforge window existed, and still not reported on; orbs with other players never tried. [#27](https://github.com/krogon-games/JotunnModStub/issues/27), [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #59's leftovers** (six tabs and the Forge tab passed alone 2026-11-10): nothing tried with other players (left for later by you; `PLAYTEST.md` → Session 2, 2h); checklist F (shatter, shard corruption, splinters in the Forge tab) passed with #29 (PR #61); not checked against EquipmentAndQuickSlots' and AdventureBackpacks' code, though you played with both loaded. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **PR #61's leftovers** (shards, shattering and splinters passed alone 2026-11-11): nothing tried with other players (`PLAYTEST.md` → Session 2, 2i); the caps aren't shown to players until the stats sheet ([#60](https://github.com/krogon-games/JotunnModStub/issues/60)); the last log had five of the game's own "Trying to add item to occupied slot -1, -1" lines whose cause isn't pinned down (not a failed shatter, not the tabs); the "Use this at a Riftforge" refusal away from the station wasn't reported on. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)
- **Small leftovers:** one click per button, the build stamp, life steal with a melee weapon, crafting an Odin item, PlantEasily planting from chests. [#27](https://github.com/krogon-games/JotunnModStub/issues/27)
- **Multiplayer:** nothing at all has been tried with more than one player. [#24](https://github.com/krogon-games/JotunnModStub/issues/24)

Started but not finished:
- **Riftforge crafting:** complete in the Forge tab (orbs PR #58 and #59; shards, shattering and Affix Splinters PR #61, #29 closed). What's missing is anything to craft with in normal play: nothing drops orbs yet (next line).
- **Currency orbs:** the ten items exist (PR #58; the Mirror of Kalandra has placeholder art) but nothing drops them yet. [#33](https://github.com/krogon-games/JotunnModStub/issues/33)
- **The Amazon's skills:** points can be spent, but no skill does anything yet. [#32](https://github.com/krogon-games/JotunnModStub/issues/32)
- **Classes UI:** still partly on sample ("demo") data until Orlog serves skills and slots. [#38](https://github.com/krogon-games/JotunnModStub/issues/38)
- **Map tiers and affixes:** your first answers recorded; the numbers table waits for your approval; no code. [#31](https://github.com/krogon-games/JotunnModStub/issues/31)
- **Elite and champion packs:** the rule is decided (above 2 stars), the design isn't started. [#39](https://github.com/krogon-games/JotunnModStub/issues/39)
- **Personal loot:** each player rolls their own, but drops aren't locked to them and there are no loot beams. [#35](https://github.com/krogon-games/JotunnModStub/issues/35)
- **The Orlog stone's look:** works; you want a redesign later. [#47](https://github.com/krogon-games/JotunnModStub/issues/47)

## Waiting on you

Play-tests (only you can run them). The phone page ([Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht)) has every step below; it was brought in line on 2026-11-11 after standing still since 2026-11-08, and from now on every round's solo checklist goes on it too (your decision, 2026-11-11).
1. **You alone on the dedicated server** (`PLAYTEST.md` → Session 2-solo, new 2026-11-11), after the release build. It checks that the three mods load on the server, that `devcommands` works there, the Orlog stone, a rip and a corrupted boss through the server. It can't check anything that needs a second player's game.
2. **The group evening** on the dedicated server, after the release build. One friend is enough for nearly all of it: only the rip-wave check after the heart changes hands needs three players. [#24](https://github.com/krogon-games/JotunnModStub/issues/24), [#25](https://github.com/krogon-games/JotunnModStub/issues/25)
3. **Loot steps 3–4**, about an hour alone (the gathering parts PR #55 covered are done). [#26](https://github.com/krogon-games/JotunnModStub/issues/26)
4. **The small leftovers.** [#27](https://github.com/krogon-games/JotunnModStub/issues/27)
5. **The weapon-skill check** (2 minutes: two console commands, `PLAYTEST.md` → Session 3b step 6). It tells whether the +6 you saw for a +7 axe is only a display quirk. [#56](https://github.com/krogon-games/JotunnModStub/issues/56)

Questions, by session (each session asks them at the start of its round):
- **Loot:** the orb rules are answered ([#28](https://github.com/krogon-games/JotunnModStub/issues/28), closed). Next: orb drop sources and rates ([#33](https://github.com/krogon-games/JotunnModStub/issues/33)); what Stonecutter's and Resinous should become (you flagged them for a rework); later the world unique pool ([#34](https://github.com/krogon-games/JotunnModStub/issues/34)).
- **Rips** ([#31](https://github.com/krogon-games/JotunnModStub/issues/31)): approve the map tiers and affixes table (waves per tier, rolling costs, seven affixes with their rewards). Later: elite packs ([#39](https://github.com/krogon-games/JotunnModStub/issues/39)); escalation after repeated corrupted kills, wanted? ([#40](https://github.com/krogon-games/JotunnModStub/issues/40)); the minimum rip distance from bases.
- **Classes** ([#32](https://github.com/krogon-games/JotunnModStub/issues/32)): D2's passive chances unchanged?; Penetrate's chance and whether it breaks immunity; can crits double backstabs?; D2R or older numbers? Then: boss XP multiplier and bonus skill points ([#36](https://github.com/krogon-games/JotunnModStub/issues/36)); eitr regen rate and early eitr potions ([#37](https://github.com/krogon-games/JotunnModStub/issues/37)).
- **UI:** the Riftforge window is answered and built ([#30](https://github.com/krogon-games/JotunnModStub/issues/30), closed). Next: where the skills panel's button or tab goes (you removed it from the inventory); the detailed character stats sheet (which stats, where it opens; it shows the caps; [#60](https://github.com/krogon-games/JotunnModStub/issues/60)). Later: the ship display at the bottom right, and the recipe list redesign you asked to be reminded of ([#43](https://github.com/krogon-games/JotunnModStub/issues/43)).
- **Group:** does `devcommands` work for you on the dedicated server? ([#24](https://github.com/krogon-games/JotunnModStub/issues/24))
- **Your PC:** a fresh `co_dump` from 1.0.21 ([#42](https://github.com/krogon-games/JotunnModStub/issues/42)); the Unity 6 upgrade before any new animation ([#48](https://github.com/krogon-games/JotunnModStub/issues/48)).
- **Parked:** storage and quests ([#46](https://github.com/krogon-games/JotunnModStub/issues/46)); wheel of powers, combat mechanics, a Norse plugin name ([#52](https://github.com/krogon-games/JotunnModStub/issues/52)).

## Top 3 priorities

Agreed with you on 2026-11-11, following the roadmap's goal for this stretch, "playable together":

1. **Play together, from today's `master`.** Make the release build now ([#25](https://github.com/krogon-games/JotunnModStub/issues/25)): `master` is clean (no open pull request, the three version numbers agree, 498 tests pass) and the evening's script already expects 0.7.0 / 0.2.0 / 0.3.0. Don't wait for another feature: seven merged pull requests (#22, #53, #55, #57, #58, #59, #61) have multiplayer steps waiting, and every round adds more. Then, in this order: **you alone on the dedicated server** (`PLAYTEST.md` → Session 2-solo; no friends needed), **one friend** for the two-player steps (nearly all of Session 2 is written for players A and B), and the **group evening** ([#24](https://github.com/krogon-games/JotunnModStub/issues/24)).
2. **Orb drops as the next build round** ([#33](https://github.com/krogon-games/JotunnModStub/issues/33)). The Riftforge is done (window [#30](https://github.com/krogon-games/JotunnModStub/issues/30), PR #59; rules [#29](https://github.com/krogon-games/JotunnModStub/issues/29), PR #61), but nothing drops orbs, so its orb half only works through console commands. One short round; the session proposes the drop sources and rates from the drop tables for you to approve. The loot steps 3–4 play-test ([#26](https://github.com/krogon-games/JotunnModStub/issues/26)) is no longer part of this priority: it blocks nothing, so do it when you have a spare hour.
3. **Make the Amazon do something** ([#32](https://github.com/krogon-games/JotunnModStub/issues/32)). Answer the four passive questions (asked 2026-11-02), then her passives are built. She has the longest road ahead (passives, then the skill button, then the bow skills), the group voted for her, and today her skill points have no effect. Orlog is its own plugin, so this round can run beside the loot one.

Map tiers and affixes ([#31](https://github.com/krogon-games/JotunnModStub/issues/31)) come right after: the table has waited for your approval since 2026-11-01, and only tiers 1–2 can be built until elites are designed ([#39](https://github.com/krogon-games/JotunnModStub/issues/39)).

**Where the work went:** the last six rounds (PRs #53–#61) were loot, UI and review fixes. Classes last moved on 2026-11-02 and rips on 2026-11-05; both wait only on your answers.

## Recommended merge order

**Today:** only the coordinator's documents branch `claude/relaxed-cray-8dkrr2` (no game code, no play-test, no version bump). PR #61 was the last feature pull request.

For the next round's pull requests, merge **safest first**:

1. **Coordinator and docs-only PRs.** They change no game code.
2. **Classes** (Orlog). Orlog lives in its own folder and plugin; no other session edits its files. Its one risk, the damage patch order with Corruption, is agreed in `docs/mod-compatibility.md` before building.
3. **Loot**. It touches shared Corruption files (`CorruptionPlugin.cs`, `Texts.cs`), and the UI and rips work builds on its Riftforge rules, so it should land before them.
4. **UI**. The Riftforge window calls the loot rules, so it goes in after them. Its own screens are self-contained.
5. **Rips** last. Map tiers need a hook in the loot session's `LootPatches.cs` and map rolling in the Riftforge window, so it depends on both.

Before each merge: the branch has merged the latest `master`, `dotnet test CraftingOverhaul.Tests` passes, and you have play-tested it (the round's last step). Merge one PR, then have the next branch merge `master` again before you merge it. After merging gameplay changes, the coordinator raises the version (Merge notes above) before the next release build.
