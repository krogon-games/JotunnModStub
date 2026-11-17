# Handoff: `claude/magical-babbage-slwwpn`

**Written:** 2026-11-17, during the round (not yet by `/handoff`). Ahead of `master`, 0 behind. No pull request yet.

## Stage

**Waiting for Krogon's playtest** (two checklists: the skill damage remake, and three new bow skills).

## Purpose

The skill damage round ([#72](https://github.com/krogon-games/JotunnModStub/issues/72)): remake how the Amazon's skills work out their damage, and finish Forked Lightning's last two play-test steps. Widened by Krogon on 2026-11-16 to three of the bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): Strafe, Immolation Arrow and Venom Strike.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built, untested (detail in `FEATURES.md` → Amazon skill bar and bow skills):
- **Skill damage scales off the weapon (#72).** Freezing Arrow's frost blast deals 50% → 150% of the shot's own damage, each of Forked Lightning's forks 25% → 100%. The damage scale table, its code, generator and tests are removed; `SkillDamageMultiplier` multiplies the skills' shares.
- **Strafe, Immolation Arrow and Venom Strike (#70)**, built by subagents (tables and rules; game code; docs), checked by the main session (tests, builds, every diff read). No Harmony patch added: three existing ones do more. The burning ground and the poison cloud are a new small object on the shooter's game (`Orlog/Content/GroundArea.cs`).
- **Written without the game's code open** (no copy to read in this session; installing the decompiler was blocked). Assumed, to be shown by the play-test and the new log lines: the damage the game hands over as the arrow is made includes the draw and the Bows skill; fire hits add up and poison hits refresh; the vanilla effects named for the fire and poison areas exist.
- **Code review of the three new skills (a second model, 2026-11-17).** Fixed: Strafe skipped no enemy behind the bow; the burning ground hung in the air after a hit on a tall creature; an explosion replayed every second. Left for the play-test (`FEATURES.md` → known issues): the game's poison doesn't add up (about one and a half doses over the cloud's 4 seconds, not four); fire arrives as burning over some seconds; Corruption's on-hit affixes run on every area hit with the weapon in hand; a Strafe volley on one enemy is up to seven arrows at 75%.
- **Claude's choices, open to change:** every fork of one shot deals the same; a half-drawn shot gives weaker skill damage; Strafe picks enemies on screen, nearest the aim first, with no check for walls in the way and no lead on a running enemy; the areas' look (a ring, a glow, a vanilla effect each second); area hits don't train the Bows skill.
- **Not built:** Snipe and Shackle Shot (still "Coming later"); Immolation Arrow's Diablo 2 casting delay (no cooldowns yet). The spear skills' table rows still hold Diablo 2's damage points (#41).
- Nothing tried with two players.

## Decisions

1. **2026-11-16: this round is issue #72, on this branch** ("Yes, #72 on this branch"): skill damage only; no new skills are built.
2. **2026-11-16: which skills felt too strong besides Forked Lightning:** "Freezing Arrow, Can't recall." (Multishot, Wind Lance and Inner Sight were offered and not picked.)
3. **2026-11-16: every damage skill scales off the weapon** ("All scale off the weapon"): a remake in which each damage skill deals a percentage of the shot's own damage (bow + arrow), growing with skill level; Diablo 2's fixed damage points and the conversion table (`skill_damage_scale.csv`) go away; the bow and spear skills still to build follow the same rule. Chosen over "keep fixed numbers, fix the table" and "only tone down the worst". Each skill's percentages: to be proposed and approved.
4. **2026-11-16: Freezing Arrow's frost blast deals 50% of the shot's damage at skill level 1, 150% at 20** ("50% at level 1, 150% at 20", over 100% → 233%, Wind Lance's, and 75% → 200%). The arrow's own hit on its target stays as it is; the 3 m radius and the 2-second freeze are unchanged.
5. **2026-11-16: each of Forked Lightning's forks deals 25% of the shot's damage at skill level 1, 100% at 20** ("25% at level 1, 100% at 20", over Diablo 2's ratio to Freezing Arrow and 50% → 150%). The fork counts (3 → 7 others, plus one on the struck enemy) and the 4 m reach are unchanged.
6. **2026-11-16: the round grows: start building the other Amazon skills, with subagents** ("I don't have a chance to play test at the moment, but we should make use of the five hour session limit. Start building out the other Amazon skills use sub agents.").
7. **2026-11-16: three bow skills now, on this branch** ("Only three bow skills": Strafe, Immolation Arrow and Venom Strike; Snipe and Shackle Shot wait for another round), over all five and over "bow skills, then spear skills".
8. **2026-11-16: Strafe's arrows each deal 75% of the shot's damage** ("75% of your shot each", Multishot's share; over 50% → 100% and Diablo 2's 105% → 200%). As put in the question: on release one arrow from the quiver becomes a volley, each arrow flying by itself at a different enemy in front of the player within 16.5 m (Inner Sight's reach); Diablo 2's counts (up to 5 enemies at skill level 1, 10 from level 6; at least 2 arrows, 7 at level 20, shared between few enemies); 11 eitr.
9. **2026-11-16: Immolation Arrow: blast 25% → 100% of the shot's damage, burning ground 10% → 30% a second** ("Blast 25%→100%, fire 10%→30% a second", over a 50% → 150% blast and over a 25% → 75% patch). As put in the question: the blast reaches 3 m, the ground burns for 4 seconds (Diablo 2's) and hurts enemies standing in it every second; eitr 6 + 0.25 per level (Diablo 2's).
10. **2026-11-16: Venom Strike's cloud poisons for 25% → 100% of the shot's damage** ("25% at level 1, 100% at 20", over 50% → 150% and 100% → 233%). As put in the question: a cloud 3 m in every direction hangs for 4 seconds where the arrow lands; everything inside is poisoned (damage over time) and re-poisoned each second it stays; no blast; the arrow hits as usual.
11. **2026-11-16: Venom Strike costs 7 eitr, +0.5 per level** ("7, +0.5 per level", Diablo 2's Plague Javelin's; over Poison Javelin's 4 + 0.25 and Immolation Arrow's 6 + 0.25).

## Playtest

`docs/playtests/claude-magical-babbage-slwwpn.md`, both rounds also on the phone page (its first two sections):
1. Skill damage: 12 steps (`r72-1` … `r72-12`), with Forked Lightning's two steps left over from PR #71 (`r72-6`, `r72-7`).
2. Strafe, Immolation Arrow, Venom Strike: 14 steps (`r70-1` … `r70-14`).

No feedback yet.

## Open questions for Krogon

1. After the play-tests: do the skills feel right at skill level 1 and at 20 (`r72-12`, `r70-14`), and is the areas' look good enough?
2. Still open from PR #71: should large roots stop a Wind Lance?
3. Snipe's and Shackle Shot's details, in their round.

## Next steps

1. Krogon play-tests both rounds; Claude reads his marks from the phone page and fixes what he finds.
2. Final play-test, then `/handoff` and a pull request into `master`.
3. Coordinator at merge: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–11), close #72, update #70 (two bow skills left).
4. Next classes rounds: Snipe and Shackle Shot (#70), then the spear skills (#41); each new skill's share of the weapon's damage is agreed there.

## Checks

Run during the round, not yet by `/handoff`:
- Build: PASS (593 tests; Orlog and CraftingOverhaul build, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog). The branch changed no version.
- In-game: needs Krogon's playtest.
- The others: not run yet (`/handoff` runs them).

## Merge notes

**Not ready to merge:** waiting for Krogon's play-test. **Gameplay changed: version bump needed at merge: Orlog.** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/class-design.md`. One data table deleted: `Orlog/Data/skill_damage_scale.csv`. One new code file: `Orlog/Content/GroundArea.cs`.
