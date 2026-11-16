# Handoff: `claude/magical-babbage-slwwpn`

**Written:** 2026-11-16, during the round (not yet by `/handoff`). Ahead of `master`, 0 behind. No pull request yet.

## Stage

**Building** (three more bow skills; the skill damage remake waits for Krogon's playtest).

## Purpose

The skill damage round ([#72](https://github.com/krogon-games/JotunnModStub/issues/72)): remake how the Amazon's skills work out their damage, so low levels stop being far too strong, and finish Forked Lightning's last two play-test steps.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built, untested (detail in `FEATURES.md` → Amazon skill bar and bow skills):
- **Skill damage scales off the weapon.** Freezing Arrow's frost blast deals 50% → 150% of the shot's own damage and each of Forked Lightning's forks 25% → 100% (whole percentages per skill level, rising evenly). Multishot and Wind Lance already worked this way and are unchanged.
- **The damage scale is removed:** `Orlog/Data/skill_damage_scale.csv`, its code, its generator and its two tests. `SkillDamageMultiplier` (admin, default 1) now multiplies the blast's and the forks' share.
- **Assumption, not checked in the game's code this round** (no copy to read in this session): the damage the game hands over as the arrow is made already includes the draw and the Bows skill. Wind Lance and Multishot rely on the same number and passed their play-tests. Each forked shot and each blast writes its sums to the log, so the play-test shows it.
- **Claude's choices, open to change:** every fork of one shot deals the same damage (no separate roll per fork); "the shot's damage" adds up every damage type of bow and arrow (a fire arrow's fire counts); a half-drawn shot gives weaker forks and a weaker blast.
- **The spear skills' table rows still hold Diablo 2's damage points** (Poison Javelin, Lightning Bolt, Plague Javelin; not built). Their shares are for the spear round (#41).
- No Harmony patch added, changed or removed. Nothing tried with two players.

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

## Playtest

`docs/playtests/claude-magical-babbage-slwwpn.md`: 12 steps (`r72-1` … `r72-12`), also the first section of the phone page. They include Forked Lightning's two steps left over from PR #71 (`r72-6`: rank 20 with nine brutes; `r72-7`: a troll with greylings). No feedback yet.

## Open questions for Krogon

1. After the play-test: do the two skills feel right at skill level 1 and at 20 (step `r72-12`)?
2. Still open from PR #71: should large roots stop a Wind Lance?

## Next steps

1. Krogon play-tests; Claude reads his marks from the phone page and fixes what he finds.
2. Final play-test, then `/handoff` and a pull request into `master`.
3. Coordinator at merge: version bump (Orlog), `STATUS.md`, `DECISIONS.md` (decisions 1–5), close #72.
4. Next classes rounds (#70, then #41): each new skill's share of the weapon's damage is agreed there.

## Checks

Run during the round (2026-11-16), not yet by `/handoff`:
- Build: PASS (588 tests; Orlog and CraftingOverhaul build, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog). The branch changed no version.
- In-game: needs Krogon's playtest.
- The others: not run yet (`/handoff` runs them).

## Merge notes

**Not ready to merge:** waiting for Krogon's play-test. **Gameplay changed: version bump needed at merge: Orlog.** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/class-design.md`. One data table deleted: `Orlog/Data/skill_damage_scale.csv`.
