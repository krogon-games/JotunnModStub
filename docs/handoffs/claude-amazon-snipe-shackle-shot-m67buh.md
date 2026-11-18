# Handoff: `claude/amazon-snipe-shackle-shot-m67buh`

**Written:** 2026-11-18 by `/handoff`. Started from `master` (`f2b0ca0`); 21 commits ahead with this one, 0 behind. No pull request yet.

## Stage

**Merged** (PR #78, 2026-11-18).

**Coordinator update (2026-11-18):** merged into `master` as **PR #78** on Krogon's word ("Merge branch claude/amazon-snipe-shackle-shot-m67buh into master using a pull request"). `master` was already in the branch (no conflicts). The coordinator raised Orlog and Corruption to **0.8.0** (on this branch, the PR's last commit), copied decisions 1–21 into `docs/DECISIONS.md`, and #70 is closed (by the PR, with a closing note).

## Purpose

The Amazon's last two bow skills, Snipe and Shackle Shot ([#70](https://github.com/krogon-games/JotunnModStub/issues/70), roadmap C5b), and the two questions carried over from PR #71 and PR #74.

## Done

Krogon's single-player play-test (2026-11-18): all 22 steps Pass on the phone page, no notes; "all playtesting is done and passed" (decision 21).
- **Snipe:** a charged shot, 3.5 s → 1.5 s to full charge, 100% rising on a square curve to 150% → 300% of the shot, up to twice an arrow's speed, 3× zoom, stamina at the bow's drawing rate, 9 + 0.25 eitr; crossbows too.
- **Shackle Shot:** 100% → 150% of the shot; ties the struck enemy to the nearest enemy or standing tree up to 8 m behind (23°), both stunned 1.75 s → 4 s (alone 0.6 s), bosses slowed; 9 + 0.25 eitr; crossbows too.
- **The burning ground's and the cloud's ticks** trigger no Corruption on-hit affixes; the blast still does.

## Half-done and known issues

Detail: `FEATURES.md` → Amazon skill bar and bow skills.
- Written with the game's 1.0.14 code open (decompiled with `ilspycmd` 8.2, as `docs/ARCHITECTURE.md` describes). **New patch: `Character.CanMove` postfix** (the stun); Corruption's two `Character.Damage` prefixes changed in body only.
- Claude's choices, open to change: Snipe's speed rises evenly with the charge; its extra stamina stops once the charge is full; mouse sensitivity unchanged while zoomed; Snipe's circle looks like Wind Lance's (no wind, a ring at full); the rope and swirls' look (shooter's game only); Dota's 23° read as each side of the line; the stun also carries Immobilized's speed so swimmers stop.
- Limits: a re-shackle takes the new time (can cut a longer stun short); the anchor search is on the ground's plane; only standing trees count. Nothing tried with two players (`PLAYTEST.md` → Session 2, 2j step 9).

## Decisions

1. **2026-11-18: this round is Snipe and Shackle Shot, both, on this branch** ("Yes, both skills"), with the two carried-over questions (area hits and Corruption's on-hit affixes; large roots and Wind Lance); over Snipe only and Shackle Shot only.
2. **2026-11-18: Snipe's charge counts from full draw, the damage grows until the charge is full (let go any time), and the full charge gets quicker with skill level** ("Grows with skill level"), over a fixed 3 s (Wind Lance's first hold) and a fixed 1.5 s (Wind Lance's hold).
3. **2026-11-18: a full Snipe charge takes 3.5 s at skill level 1 and 1.5 s at level 20** (his own answer: "3.5s lvl1 - 1.5s lvl 20"), over 3 → 1.5 s, 1.5 → 0.75 s and 4.5 → 1.5 s. Even steps between (Claude's assumption, as the other skills' tables).
4. **2026-11-18: a fully charged Snipe deals 150% of the shot's own damage at skill level 1, 300% at 20** ("150% → 300%"), over 200% → 400%, Wind Lance's 100% → 233% and 300% → 600%.
5. **2026-11-18: Snipe's damage climbs slowly at first and fastest near the end of the charge** ("Slow start, big finish"), over an even climb from 100% and an even climb with a minimum charge. The exact curve and what an early release costs: decision 6.
6. **2026-11-18: Snipe's damage follows a square curve, and any charge fires Snipe and pays its eitr** ("Square curve; pay on any charge"): damage = 100% + (full share − 100%) × charge², where charge runs 0 → 1 over the charge time (level 1: 100% at full draw, 112% at half charge, 150% at full). Let go at full draw with no charge: a normal arrow, no eitr. Over paying only from a full charge and paying in proportion to the charge.
7. **2026-11-18: Snipe zooms 3× at full charge** ("3× at full charge"): the view (the game's 65°) closes in slowly as the charge fills, to about 24° at full charge, with the game's own smooth camera zoom, and springs back when you shoot or let go (as put in the question). Over 2×, 1.5× and growing with skill level.
8. **2026-11-18: Snipe drains stamina at the bow's drawing rate while charging** ("14 a second"): the game's bows drain 14 a second while drawing (Strafe's log, 2026-11-17; less with Bows skill, `ItemData.GetDrawStaminaDrain`) and halve it once fully drawn; during Snipe's charge it doesn't halve. About 49 stamina for a full level-1 charge, 21 at level 20. Over 7 a second (a drawn bow's) and 21 a second.
9. **2026-11-18: Snipe costs 9 eitr, +0.25 per level** ("9, +0.25 per level", Diablo 2's Freezing Arrow's mana: 9 → 13.75 at level 20), over Guided Arrow's 8 − 0.25 and Diablo 2's Strafe's 11.
10. **2026-11-18: Snipe's arrow flies faster with the charge** ("Faster with the charge"): up to twice a fully drawn arrow's speed at full charge (Wind Lance's `speed_pct` 200), a normal arrow's speed with no charge; still the game's own arrow and drop, so it drops less and reaches further. Over a normal arrow's flight and dead straight to 100 m. Claude's choice, open to change: the speed rises evenly with the charge (not on the damage's square curve), so the arc changes predictably.
11. **2026-11-18: Shackle Shot ties the struck creature to another enemy or a tree behind it** ("Another enemy or a tree"), Dota 2's rule: the mod looks behind the struck creature along the arrow's path in a narrow cone (Dota's 23°); trees are what the game counts as trees (standing trees, not stumps or logs). Over "enemy, tree, rock or building" and "another enemy only".
12. **2026-11-18: a shackled creature can't move or attack, and shows it's stunned** (his own answer: "My moving mow attacking an invisible stunned animation how most other games displayed the sort of swirls above the head", read by Claude as "no moving, no attacking, and a visible stunned animation: the swirls above the head most other games show"). Dota 2's stun: no moving and no new attacks (an attack already swinging finishes, as put in the question); over "no moving only" (the game's Immobilized). The swirls: no vanilla effect does this, so they are made from vanilla particles (no new art).
13. **2026-11-18: the shackle holds 1.75 s at skill level 1, 4 s at level 20, and 0.6 s when nothing is behind the target** ("1.75 s → 4 s, 0.6 s alone"): Dota 2's 1.75 s and 3.25 s with its +0.75 s talent folded into level 20, and its 0.6 s fail stun (Valve's hero data, `stun_duration`, `fail_stun_duration`, `special_bonus_unique_windranger_6`). Over Dota's 1.75 → 3.25 s and 2 → 4 s.
14. **2026-11-18: bosses are only slowed by Shackle Shot** ("Bosses only slowed"), Freezing Arrow's rule: a boss in the shackle is slowed instead of held, for the same time; the other end of the shackle is held as usual. Over "bosses immune" (the game harpoon's rule) and "held for half the time".
15. **2026-11-18: the shackle reaches 8 m behind the struck creature** ("8 m"), the game's harpoon line (`Harpooned`: `m_breakDistance` 8, game dump), at every level, inside Dota's 23° cone. Over Forked Lightning's 4 m and growing with level.
16. **2026-11-18: Shackle Shot's arrow deals 100% of the shot's own damage at skill level 1, 150% at 20** ("100% → 150%"), to the struck creature only; the one behind takes no damage, only the hold (as put in the question). Over 100% at every level (closest to Dota 2, which deals none) and 50% → 100%.
17. **2026-11-18: Shackle Shot costs 9 eitr, +0.25 per level, with no cooldown** ("9, +0.25 per level", Diablo 2's Freezing Arrow's mana: 9 → 13.75 at level 20), over Ice Arrow's 4 + 0.25, Strafe's 11 and adding a cooldown.
18. **2026-11-18: both skills work with crossbows too, with the same numbers** ("Both work with crossbows"), over Shackle Shot only, Snipe only and bows only. As put in the question: Snipe on a loaded crossbow charges as Wind Lance does on one (hold right click, the bolt is held back while the charge, zoom and stamina drain build, fires on release); Shackle Shot is the crossbow's normal shot. A crossbow has no draw drain, so Snipe's charge drains the bows' drawing rate (14 a second, as Strafe's crossbow stream uses the bows' drain; Claude's reading of decision 8).
19. **2026-11-18 (carried over from PR #74): Immolation Arrow's blast counts for Corruption's on-hit affixes as a hit of the bow; the burning ground's and the poison cloud's ticks trigger none** ("Blast counts, ticks don't"), as in Path of Exile, where damage over time never leeches or procs on-hit. Over "all count, as the bow's" and "none count". (Before: each tick used the affixes of the weapon in hand at that moment; the code review's find of 2026-11-17.)
20. **2026-11-18 (carried over from PR #71): large roots don't stop a Wind Lance** ("Leave it as it is"): the gust keeps going through `HugeRoot1`, `Greydwarf_Root` and crows, unharmed, as since 2026-11-16; over "large roots stop it".
21. **2026-11-18: the play-test passed; end the session** (typed with `/handoff`: "all playtesting is done and passed. wrap up session"). On the phone page all 22 steps are marked Pass with no notes (`sn-1` … `sn-10`, `sh-1` … `sh-10`, `dot-1`, `dot-2`).

## Playtest

`docs/playtests/claude-amazon-snipe-shackle-shot-m67buh.md`: "Snipe and Shackle Shot", 22 steps: **passed** (decision 21). Its steps are off the phone page again; the two-player step is on it (`mpe-2j-9`).

## Open questions for Krogon

1. None open. (His verdict step `sh-10` passed with no note, so the rope and swirls' look and Claude's other choices stand.)

## Next steps

1. ~~Pull request into `master`; Krogon merges.~~ Done (PR #78).
2. ~~Coordinator: raise Orlog and Corruption, `STATUS.md`, `DECISIONS.md` (decisions 1–21), close #70.~~ Done 2026-11-18 (Orlog 0.8.0, Corruption 0.8.0).
3. Next classes round: C6, Gungnir's Throw and the spear and javelin skills (#41).

## Checks

- Build: PASS (601 tests; Orlog, Corruption, CraftingOverhaul build, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog, Corruption). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested. No new setting, message or saved key; the stun and slow go to the creature's owner through the game's own `SEMan.AddStatusEffect`, and every player has Orlog; the rope and swirls are local by design.
- Assets and names: PASS (CSVs embedded by wildcard; `Harpooned`, `Immobilized`'s numbers and `TreeBase` are the game's own; no new `$` tokens).
- FEATURES.md: PASS (the new `Character.CanMove` target is listed).
- In-game: PASSED in Krogon's single-player play-test (decision 21); two players untested.

## Merge notes

**Ready to merge: yes** (play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog and Corruption.** The other remote branches (`claude/elegant-maxwell-e7ys9o`, `claude/modest-curie-y050l4`) have no unmerged commits. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md`, `docs/GAME_DESIGN.md`, `docs/class-design.md`, `docs/mod-compatibility.md`.
