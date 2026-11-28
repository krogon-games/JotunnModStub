# Handoff: `claude/wizardly-wozniak-4aix76`

**Written:** 2026-11-28. Started from `claude/nifty-hypatia-afm8ez` at `608bac6` (not `master`), as Krogon asked.

## Stage

Built, waiting for Krogon's play-test.

## Purpose

More corrupted-only map affixes ([#97](https://github.com/krogon-games/JotunnModStub/issues/97), ROADMAP R1d): the Ice explosion on death and a corrupted boss guarding the heart, from Krogon's list in `docs/handoffs/claude-kind-einstein-8sn8e7.md`. Builds on Thorns and the "5th affix on a corrupted map" rule of `claude/nifty-hypatia-afm8ez`.

## Done

Nothing yet.

## Half-done and known issues

- **Built, not play-tested** (806 tests; Corruption builds against the stand-in game folder): **Rimeburst** (`rimeburst`, 50–100%) and **Guarded** (`guarded`) rows in `map_affixes.csv` (`pool` = corrupted); rules in `RipTierRules` (unit-tested, `IceBurstGuardianTests`); game code in `Corruption/Content/CorruptedMapAffixes.cs`. New patches: `Character.OnDeath` prefix (Rimeburst), `Game.Start` postfix (its message), `Destructible.RPC_Damage` prefix (the guarded heart). Game code decompiled (1.0.14 assemblies of the stand-in) for the star damage factor and the weapons' damage.
- **Claude's choices** (changeable): names Rimeburst and Guarded (hidden on the UI, used in `co_map` and logs); the burst has no stagger and no push and can be blocked and rolled through; "its strongest hit" = its strongest weapon's fighting damage (no chop or pickaxe) × that attack's multiplier × the game's star factor × Brutal; every death of a stamped creature bursts, whoever killed it (the guardian too); the boss appears 6 m from the heart on the side away from the players; it leaves once the rip closes, like the leftovers; Kall starts at his first phase and his later phases keep guarding.
- **Unknown until played:** the Queen, Kall and Fader outside their arenas; the boss's spawn on uneven ground.
- With Thorns, each of the three corrupted-only affixes is 1 in 3 on a corrupted map (they all weigh 100); before this round Thorns was the only one (100%).

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
11. **The guardian is the biome's own boss, corrupted** (2026-11-28): "The biome's own boss". The boss whose essence makes the map (Meadows Eikthyr, Black Forest the Elder, Swamp Bonemass, Mountains Moder, Plains Yagluth, Mistlands the Queen, Ashlands Fader, Deep North Kall), corrupted as at the altars.
12. **It spawns at the heart and charges the players** (2026-11-28): "It spawns at the heart and charges towards the players" (his own answer). Claude's reading: it appears when the waves start (a player within 96 m) and hunts the players at once.
13. **The heart is invulnerable while the boss lives** (2026-11-28): "No, invulnerable".
14. **It gives a corrupted boss's rewards** (2026-11-28): "Same as a corrupted boss". Its vanilla drops, 2–3 essences, 3–5 shards, and each player's boss shard and orb.
15. **It appears when the waves start** (2026-11-28): "Yes, when waves start". At the heart, as soon as a player comes within 96 m, and hunts the players at once.
16. **It keeps its summons** (2026-11-28): "Keep summons". 2–3 of its army every 30 s on top of the rip's waves, as a corrupted boss anywhere.
17. **The map's other affixes apply to it** (2026-11-28): "Yes, like the waves". It carries the rip's stamp like every rip creature.
18. **Its red line** (2026-11-28): "Guards the heart". "A corrupted boss guards the heart."

## Playtest

Solo checklist `docs/playtests/claude-wizardly-wozniak-4aix76.md` (`cg-1` … `cg-18`), also on the phone page after the `claude/nifty-hypatia-afm8ez` section. Not started.

## Open questions for Krogon

None.

## Next steps

1. Krogon's solo play-test (`cg-1` … `cg-18`; `la-1` … `la-20` of the base branch work on this build too).
2. Fix what fails; final play-test; `/handoff`.

## Merge notes

**Must merge after `claude/nifty-hypatia-afm8ez`** (this branch was started from it and carries its commits). Gameplay changed: version bump needed at merge (Corruption). Shared docs changed: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/corruption-design.md`, `docs/mod-compatibility.md`, `Corruption/Data/README.md`.
