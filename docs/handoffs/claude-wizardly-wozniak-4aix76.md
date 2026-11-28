# Handoff: `claude/wizardly-wozniak-4aix76`

**Written:** 2026-11-28. Started from `claude/nifty-hypatia-afm8ez` at `608bac6` (not `master`), as Krogon asked.

## Stage

Deciding (one affix at a time).

## Purpose

More corrupted-only map affixes ([#97](https://github.com/krogon-games/JotunnModStub/issues/97), ROADMAP R1d): the Ice explosion on death and a corrupted boss guarding the heart, from Krogon's list in `docs/handoffs/claude-kind-einstein-8sn8e7.md`. Builds on Thorns and the "5th affix on a corrupted map" rule of `claude/nifty-hypatia-afm8ez`.

## Done

Nothing yet.

## Half-done and known issues

None yet.

## Decisions

1. **This round builds two affixes** (2026-11-28, Krogon's brief): "Monsters trigger an explosion of Ice on death" and "A corrupted boss is Guarding the Heart", from his list in `docs/handoffs/claude-kind-einstein-8sn8e7.md`.
2. **Later rounds** (2026-11-28, Krogon's brief): the meteors, the embers, the burial stones and the Treasure Fallen are later rounds.
3. **Round and branch confirmed** (2026-11-28): "Yes, confirmed". #97 on `claude/wizardly-wozniak-4aix76`, started from `claude/nifty-hypatia-afm8ez`, merging after it.
4. **The ice explosion deals a share of the dying enemy's own hit** (2026-11-28): "Share of its own hit". A rolled share of the enemy's strongest attack, as frost, so it grows with the biome and the stars.
5. **The explosion comes after a short warning** (2026-11-28): "Short warning". A frost glow on the body, then the burst, so players can step away.
6. **The blast is 3 m across, like Freezing Arrow's** (2026-11-28): "3 m, like Freezing Arrow" (Orlog's Freezing Arrow radius, 3.3 yards from Diablo 2's data).
7. **The share rolls 50–100%** (2026-11-28): "50–100%". Each map rolls its number in that range.
8. **The warning lasts 1.5 seconds** (2026-11-28): "1.5 seconds".
9. **The burst hurts players only** (2026-11-28): "Players only". Other enemies are untouched, so bursts can't chain. Frost slows as the game's own frost does; armour and frost resistance reduce it.
10. **Its red line is short, with no number** (2026-11-28): "Shorter". "Enemies explode in ice on death." The rolled share still applies; it just isn't shown.

## Playtest

Not written yet.

## Open questions for Krogon

The boss affix (which boss, when, the heart, rewards, red line).

## Next steps

1. Finish the decisions; build; checklist on the phone page.

## Merge notes

**Must merge after `claude/nifty-hypatia-afm8ez`** (this branch was started from it and carries its commits).
