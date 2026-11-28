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
19. **A freeze watch** (2026-11-28, after the game froze during the play-test): "Yes, add it". If the game stops drawing frames for 10 s, the log and `BepInEx/corruption_freeze.txt` name the mod code that was running (or none of ours). No gameplay change.
20. **Corrupted creatures fight players only** (2026-11-28, after the play-test, changes the design's "maddened creatures treat every other creature and player as an enemy"): "The corrupted creatures should NOT attack each other or the corrupted boss. The normal creatures around should NOT attack the corrupted creatures and behave normally until agro'ed by a player. The corrupted creatures should not attack normal creatures, but instead target the player." So corrupted creatures (maddened wave creatures, a corrupted boss and its summons) are never enemies of each other or of normal creatures, and normal creatures behave as vanilla towards players.
21. **Creatures near the heart still turn corrupted when the waves start** (2026-11-28): "Keep turning them red". They join the corrupted side and hunt players only.
22. **Tamed creatures fight the corrupted ones** (2026-11-28): "Yes, tames fight them". Corrupted creatures attack players and players' tames, and tames can hurt them.
23. **A map's forge result shows on the left** (2026-11-28, with a screenshot): "underneath the map icon in the forge, theres empty space now, unlike when gear is placed in forge … move that green text to be displayed on the left for maps only." In the Forge tab, a map's last result (e.g. "Orb of Alchemy: now tier 4" and the added lines) shows under the map's tier line on the left; gear keeps it under the description.
24. **No result text in the Forge panel** (2026-11-28, replaces 23's result on the left): "I think we can remove the green text entirely. because the affixes get momentarily highlighted when theyre added, the green text is redundant". On refusals and messages a highlight can't show: "a lot of the orbs that cant be used are already red when not usable. for mirror and for a corruption that deletes an item - use the ingame toast text in the middle of the screen. \"Your item was destroyed\" in red for failed corruption. and \"Your item was mirrored\" in orange. remove all text from the panel."
25. **Gear in the Forge looks like a map** (2026-11-28, his mockup): "the vanilla stats are not required when crafting with orbs. vanilla stats can be easily seen when hovering over the item anyway. and this will bring the design of maps and gear in line together." Claude's reading, confirmed ("Yes, that's it"): the right side shows the gear's name, then its affix list (moved from the left, with the highlight); the left side shows the icon, the name, the rarity line ("Rare Black Metal Sword") and then the item's description; the vanilla stats are gone; Shatter stays.
26. **The other cases without panel text** (2026-11-28): "Yes, all four". (1) Shatter → orange toast "Your item was shattered"; (2) a shard on gear that adds no affix → red toast "Your item was corrupted"; (3) a refusal only found out by clicking (no free bag slot for the Mirror's copy, the item isn't in the bag, no orbs left, splinters couldn't be stored) → its reason as the game's small top-left message; (4) an Annulment that removes an affix → no message. With decision 24's "Your item was destroyed" (red) and "Your item was mirrored" (orange), all in the middle of the screen.
27. **A shard corrupts a map of any tier** (2026-11-28, after the Forge play-test; changes the rips round's 6/6c "only a full tier 5 map can be corrupted"): "Corruption of maps should be doable to any tier of maps. Corrupting a map always adds a corruption affix, and always bumps up the tier of the map to the next tier. This adds more options to gameplay and choice, which is better design overall." Tier 5 → 6 as before; tier 1 → 2 with only the corrupted affix.
28. **A corrupted map stays locked** (2026-11-28): "Keep it locked". No orb changes it afterwards; the Mirror can still copy it.
29. **Every corrupted map is "Corrupted {biome} map" in red** (2026-11-28): "\"Corrupted\", red". Whatever its tier; the icon's number and the tier line show the tier.

## Playtest

Solo checklist `docs/playtests/claude-wizardly-wozniak-4aix76.md`. **2026-11-28: `cg-1` … `cg-18` passed** (Rimeburst and Guarded); one freeze while idling earlier (cause unknown, freeze watch added, not seen again). Note on `cg-5`: Rimeburst's damage feels low, its slow too weak, its look to improve later. Then decisions 20–22 (who corrupted creatures fight): **final play-test `cg-20` … `cg-23`**: Krogon, 2026-11-28: "Playtesting done" (no marks on the phone page; taken as passed on his word). Then decisions 23–26 (the Forge tab: no result text, gear laid out like a map, toasts): steps `cg-24` … `cg-27`: Krogon, 2026-11-28: "Playtesting done. All works." (he found a tier 4 map couldn't be corrupted, by the old rule: decision 27). Then decisions 27–29 (corrupt maps of any tier): steps `cg-28` … `cg-30`, waiting for Krogon.

## Open questions for Krogon

None.

## Next steps

1. Krogon's solo play-test (`cg-1` … `cg-18`; `la-1` … `la-20` of the base branch work on this build too).
2. Fix what fails; final play-test; `/handoff`.

## Merge notes

**Must merge after `claude/nifty-hypatia-afm8ez`** (this branch was started from it and carries its commits). *Done:* that branch merged as PR #100 (Corruption 0.13.0), and `master` was merged into this branch on 2026-11-28 (conflicts in the shared docs kept both sides). Gameplay changed: version bump needed at merge (Corruption). Shared docs changed: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (the phone-page line), `docs/corruption-design.md`, `docs/mod-compatibility.md`, `Corruption/Data/README.md`.
