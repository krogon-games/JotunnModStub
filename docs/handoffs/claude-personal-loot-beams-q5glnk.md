# Handoff: `claude/personal-loot-beams-q5glnk`

**Written:** 2026-11-26 (started; `/handoff` rewrites this header). Started 2026-11-26 from `master` at `e99092b`; no PR yet.

## Stage

**Asking Krogon questions.**

## Purpose

Personal loot ([#35](https://github.com/krogon-games/JotunnModStub/issues/35), ROADMAP L7): a drop rolled for one player is that player's for its first 60 seconds, and Rare (later unique) drops get a loot beam and a sound.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Nothing built yet.

## Decisions

1. **This round and branch** (2026-11-26): #35, personal loot and loot beams, on `claude/personal-loot-beams-q5glnk`.
2. **Locked drops are invisible to other players** (2026-11-26), over "everyone sees it": only the owner sees a drop rolled for them during its 60 seconds; other players see nothing (no item, no beam, no hover) until the lock ends, then it appears for everyone.
3. **Nothing special when the owner logs out or dies** (2026-11-26): the 60 s run on the network's clock whether the owner is there or not; when they end anyone can take the drop. A relog within the minute keeps it (the lock is on the character's ID).
4. **No hover text** (2026-11-26), over "owner and seconds", "owner's name only" and "Locked". Krogon: "Nothing. Since drops are not visible to the player they dropped for. Once 60 seconds is up, it doesn't matter, everyone sees it. No owner name needed to display since loot is unlocked." (He means the players they *weren't* dropped for: decision 2.)
5. **What becomes personal** (2026-11-26): all three offered: **kill loot** (each nearby player's Magic and Rare gear, orbs, maps, and the corrupted boss's guaranteed orb), **each player's boss shard** (the 1 per player from every boss kill), and **orbs from Hoardfinder's and of Hidden Riches** (locked to whoever swung the tool). Drops rolled once for everyone stay shared: vanilla loot, boss essences, the first-kill and corrupted-boss bonus shards, dungeon chests.
6. **No rip chest: the heart spills the loot, divided among the players** (2026-11-26), over "spill beside the chest", "at each player's feet" and "keep them in the chest". Krogon: "We should make a change to the heart and chest mechanic. There should be no chest at all. And all loot should spill on killing the heart. Make all loot divided amongst the amount of players present. If an odd number rolls, someone may miss out and that's fine."
7. **Beams on every Corruption drop** (2026-11-26): Krogon picked rare orbs, maps and the Regal Orb, and added: "All orbs, shards, and magic, rare and unique loot". Rare gear stays yellow and uniques orange-gold (the design).
8. **Krogon picks the sound by ear** (2026-11-26): a few of the game's own sounds wired in, a console command to play each; he chooses in the play-test. Rare and unique can get different ones.
9. **At the heart, each player's own rolls stay theirs** (2026-11-26), over "deal everything like cards" and "whole stacks at random": each player's orb and gear rolls spill as their own drops; only the shared part (the biome chest's vanilla loot, the 1–3 shards, the map) is divided among the players near the heart. (How the shared part is dealt: Claude's reading of decision 6, to confirm in the play-test: one item at a time in turn, players in a random order, the turn carrying on between kinds, so 10 coins for 3 players go 4/3/3 and leftovers rotate; each player's share is theirs for the 60 s.)
10. **Beam colours by kind** (2026-11-26), over "gear colours, the rest white": Magic gear blue (as its name), Rare yellow, unique orange-gold, every orb white, shards red (their own colour), maps green.
11. **Beam size by rarity** (2026-11-26), over "all tall": short and faint for Magic gear, shards and the common and uncommon orbs (Transmutation, Augmentation, Alchemy, Regal, Annulment); tall and bright, seen from about 50 m, for Rare gear, uniques, maps and the rare orbs (Exalted, Divine, Chance, Mirror).
12. **The sound plays for tall-beam drops only** (2026-11-26), over "every beam drop" and "Rare gear and uniques": Rare gear, uniques, maps and the rare orbs, for the owner only; common drops stay silent.

## Playtest

Not yet: the checklist will be in `docs/playtests/claude-personal-loot-beams-q5glnk.md`.

## Open questions for Krogon

Still being asked: which drops lock, the rip chest, beams and sound.

## Next steps

1. Finish the questions.
2. Build the rules and data table, then the lock, the hiding and the beams.

## Checks

Not run yet.

## Merge notes

Not ready. Gameplay will change (Corruption): version bump needed at merge.
