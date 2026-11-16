# Handoff: `claude/magical-babbage-slwwpn`

**Written:** 2026-11-16, at the start of the round (not yet by `/handoff`). Level with `master`. No pull request yet.

## Stage

**Asking Krogon questions.**

## Purpose

The skill damage round ([#72](https://github.com/krogon-games/JotunnModStub/issues/72)): remake how the Amazon's skills work out their damage, so low levels stop being far too strong, and finish Forked Lightning's last two play-test steps.

## Done

Nothing yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **2026-11-16: this round is issue #72, on this branch** ("Yes, #72 on this branch"): skill damage only; no new skills are built.
2. **2026-11-16: which skills felt too strong besides Forked Lightning:** "Freezing Arrow, Can't recall." (Multishot, Wind Lance and Inner Sight were offered and not picked.)
3. **2026-11-16: every damage skill scales off the weapon** ("All scale off the weapon"): a remake in which each damage skill deals a percentage of the shot's own damage (bow + arrow), growing with skill level; Diablo 2's fixed damage points and the conversion table (`skill_damage_scale.csv`) go away; the bow and spear skills still to build follow the same rule. Chosen over "keep fixed numbers, fix the table" and "only tone down the worst". Each skill's percentages: to be proposed and approved.

## Playtest

Nothing to test yet. Carried over from PR #71: Forked Lightning's steps `r16-7` (a troll with greylings at its feet) and `r16-8` (rank 20 with nine brutes).

## Open questions for Krogon

1. Each skill's percentages (being proposed).

## Next steps

1. Propose the percentages for Forked Lightning and Freezing Arrow (and the spear skills that have fixed numbers); Krogon approves.
2. Build: tables, rules, tooltips, tests, docs.
3. Play-test checklist (file and phone page), with `r16-7` and `r16-8`.

## Checks

Not run yet.

## Merge notes

Not ready to merge: nothing built.
