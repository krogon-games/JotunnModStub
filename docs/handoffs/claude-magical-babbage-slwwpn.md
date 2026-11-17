# Handoff: `claude/magical-babbage-slwwpn`

**Written:** 2026-11-17 by `/handoff`. 9 commits ahead of `master` with this one, 0 behind. No pull request yet.

## Stage

**Ready to merge.**

## Purpose

The skill damage round ([#72](https://github.com/krogon-games/JotunnModStub/issues/72)): remake how the Amazon's skills work out their damage. Widened by Krogon to three of the bow skills still to build ([#70](https://github.com/krogon-games/JotunnModStub/issues/70)): Strafe, Immolation Arrow and Venom Strike.

## Done

Krogon's single-player play-tests (2026-11-17, "testing has passed"; 26 steps, all Pass, no notes):
- **Skill damage scales off the weapon:** Freezing Arrow's blast 50% → 150% and each of Forked Lightning's forks 25% → 100% of the shot's own damage; the damage scale table is gone. Forked Lightning's two steps left over from PR #71 passed with it.
- **Strafe, Immolation Arrow and Venom Strike.**

## Half-done and known issues

Detail in `FEATURES.md` → Amazon skill bar and bow skills.
- **Left open by the code review, not looked at in the play-test (no log sent):** the game's poison doesn't add up in the cloud; fire arrives as burning; Corruption's on-hit affixes run on every area hit, with the weapon in hand (`docs/mod-compatibility.md`); a Strafe volley on one enemy is up to seven arrows at 75%.
- Written without the game's code open (the decompiler couldn't be installed).
- Claude's choices, open to change: Strafe has no check for walls and no lead; the areas' look.
- Not built: Snipe, Shackle Shot; the spear skills' shares (#41).
- Nothing tried with two players (`PLAYTEST.md` → Session 2, 2j step 8).

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
12. **2026-11-17: both play-tests passed; end the session** ("testing has passed. lets move towards ending session"; typed with `/handoff`). On the phone page all 26 steps are marked Pass with no notes: `r72-1` … `r72-12` (skill damage) and `r70-1` … `r70-14` (Strafe, Immolation Arrow, Venom Strike).

## Playtest

`docs/playtests/claude-magical-babbage-slwwpn.md`: **passed**, both rounds (12 + 14 steps). The rounds' steps are off the phone page; the two-player step is on it (`mpe-2j-8`).

## Open questions for Krogon

1. Should the burning ground's and the cloud's hits count for Corruption's on-hit affixes (life steal and the rest)?
2. Still open from PR #71: should large roots stop a Wind Lance?
3. Snipe's and Shackle Shot's details, in their round.

## Next steps

1. Pull request into `master`; Krogon merges.
2. Coordinator: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–12), close #72, update #70 (two left).
3. Next classes rounds: Snipe and Shackle Shot (#70), then the spear skills (#41).

## Checks

- Build: PASS (593 tests; Orlog builds, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested (no new setting, message or saved key; hits use the game's own damage message).
- Assets and names: PASS (CSVs embedded by wildcard; the four vanilla effect items are in the game dump; no new tokens).
- FEATURES.md: PASS (no patch target added or removed).
- In-game: PASSED in Krogon's single-player play-tests; two players untested.

## Merge notes

**Ready to merge: yes** (final play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog.** No other remote branch has unmerged commits. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/class-design.md`, `docs/mod-compatibility.md`.
