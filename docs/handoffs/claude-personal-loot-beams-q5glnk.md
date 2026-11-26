# Handoff: `claude/personal-loot-beams-q5glnk`

**Written:** 2026-11-26 (`/handoff`). From `master` at `e99092b`; 24 commits ahead, 0 behind; no PR yet.

## Stage

**Merged** (PR #88, 2026-11-26).

**Coordinator update (2026-11-26):** merged into `master` as **PR #88** on Krogon's word ("Merge branch claude/personal-loot-beams-q5glnk into master using a pull request"). `master` was already in the branch (no conflicts). The coordinator raised **Corruption to 0.10.0** (on this branch, the PR's last commit; Corruption's README feature list, config table and changelog brought up to date with it), copied decisions 1–16 into `docs/DECISIONS.md`, put the new wording of the rip and boss steps on the phone page and in `PLAYTEST.md`, and #35 is closed (by the PR). With the bump, a 0.9.0 game can no longer join, so the "older game sees locked drops" limit below is gone.

## Purpose

Personal loot ([#35](https://github.com/krogon-games/JotunnModStub/issues/35), ROADMAP L7): a drop rolled for one player is theirs for 60 s and invisible to others; loot beams on every Corruption drop; the rip heart spills its loot instead of leaving a chest.

## Done

Passed Krogon's solo play-tests on 1.0.30 (2026-11-26, clean log; "playtest done. ready to finalise."):
- Kill loot, each player's boss shard and Hoardfinder's / of Hidden Riches orbs locked to their roller for 60 s, invisible to others, then free; player-dropped items never locked.
- Beams: colour by kind, tall or short, tapered, soft with a glow, slowly pulsing, smoke-like wisps (`personal_loot.csv`, live `co_personal look`); the level-up sound on tall beams for the owner.
- No rip chest: the heart's loot spills, own rolls per player, the shared part dealt.

## Half-done and known issues

- **Not tried with two players** (Session 2, 2l): hiding on another game, ownership handover, the heart's dealing, a dedicated server.
- Three Opus reviews, all fixes in; the game methods relied on are confirmed on 1.0.30 by Krogon's log.
- Limits: each game reveals a drop by its own network clock (±1 s); a dedicated server's locks pause while nobody is on; sleeping ends running locks; ~~an older 0.9.0 game joins without a warning but sees locked drops~~ (gone with the 0.10.0 bump: Jötunn turns 0.9.0 away).

## Decisions

1. **This round and branch** (2026-11-26): #35, personal loot and loot beams, on `claude/personal-loot-beams-q5glnk`.
2. **Locked drops are invisible to other players** (2026-11-26), over "everyone sees it": only the owner sees a drop rolled for them during its 60 seconds; other players see nothing (no item, no beam, no hover) until the lock ends, then it appears for everyone.
3. **Nothing special when the owner logs out or dies** (2026-11-26): the 60 s run on the network's clock whether the owner is there or not; when they end anyone can take the drop. A relog within the minute keeps it (the lock is on the character's ID).
4. **No hover text** (2026-11-26), over "owner and seconds", "owner's name only" and "Locked". Krogon: "Nothing. Since drops are not visible to the player they dropped for. Once 60 seconds is up, it doesn’t matter, everyone sees it. No owner name needed to display since loot is unlocked." (He means the players they *weren't* dropped for: decision 2.)
5. **What becomes personal** (2026-11-26): all three offered: **kill loot** (each nearby player's Magic and Rare gear, orbs, maps, and the corrupted boss's guaranteed orb), **each player's boss shard** (the 1 per player from every boss kill), and **orbs from Hoardfinder's and of Hidden Riches** (locked to whoever swung the tool). Drops rolled once for everyone stay shared: vanilla loot, boss essences, the first-kill and corrupted-boss bonus shards, dungeon chests.
6. **No rip chest: the heart spills the loot, divided among the players** (2026-11-26), over "spill beside the chest", "at each player's feet" and "keep them in the chest". Krogon: "We should make a change to the heart and chest mechanic. There should be no chest at all. And all loot should spill on killing the heart. Make all loot divided amongst the amount of players present. If an odd number rolls, someone may miss out and that’s fine."
7. **Beams on every Corruption drop** (2026-11-26): Krogon picked rare orbs, maps and the Regal Orb, and added: "All orbs, shards, and magic, rare and unique loot". Rare gear stays yellow and uniques orange-gold (the design).
8. **Krogon picks the sound by ear** (2026-11-26): a few of the game's own sounds wired in, a console command to play each; he chooses in the play-test. Rare and unique can get different ones.
9. **At the heart, each player's own rolls stay theirs** (2026-11-26), over "deal everything like cards" and "whole stacks at random": each player's orb and gear rolls spill as their own drops; only the shared part (the biome chest's vanilla loot, the 1–3 shards, the map) is divided among the players near the heart. (How the shared part is dealt: Claude's reading of decision 6, to confirm in the play-test: one item at a time in turn, players in a random order, the turn carrying on between kinds, so 10 coins for 3 players go 4/3/3 and leftovers rotate; each player's share is theirs for the 60 s.)
10. **Beam colours by kind** (2026-11-26), over "gear colours, the rest white": Magic gear blue (as its name), Rare yellow, unique orange-gold, every orb white, shards red (their own colour), maps green.
11. **Beam size by rarity** (2026-11-26), over "all tall": short and faint for Magic gear, shards and the common and uncommon orbs (Transmutation, Augmentation, Alchemy, Regal, Annulment); tall and bright, seen from about 50 m, for Rare gear, uniques, maps and the rare orbs (Exalted, Divine, Chance, Mirror).
12. **The sound plays for tall-beam drops only** (2026-11-26), over "every beam drop" and "Rare gear and uniques": Rare gear, uniques, maps and the rare orbs, for the owner only; common drops stay silent.

Krogon's play-test feedback (2026-11-26, phone page notes):

13. **The beams are too big: half the size** (step `pl-2`, the Rare beam): "beam is massive though, way too big. shrink to half the size." With step `pl-4`'s "all the taller ones are just too tall", Claude applies it to the tall beams (height and width halved); the short beams keep their size.
14. **Beams taper and pulse slowly** (step `pl-4`): "they are all too one dimensional so to say. i think they would look better if they flare. thick at base and thinner towards the top. all the taller ones are just too tall. Would look better if the beam pulsated very slowly". Every beam, short and tall.
15. **Keep the level-up sound for now** (step `pl-14`): "fine with the level up sound for now. maybe we can make our own custom sounds similar to diablo 4 drops sounds." The `default` sound stays for every tall row; custom sounds are an idea for later, not this round.
16. **The beams are too solid: softer, see-through, smoke-like** (final play-test step `plf-1`, failed; steps `plf-2` … `plf-4` passed): "they look better but still not quite right. attached screenshot. Maybe they need more opacity or smokelike effect. they are too solid looking." Claude's reading: less opaque, not more ("too solid looking"); soft fading edges, a faint glow, a dimmer core and slow smoke-like wisps rising up the beam, with a `co_personal look` command to tune them live; Krogon sends the numbers he likes.

## Playtest

Solo: passed (playtest file: `pl-*`, `plf-*`, `plg-*`). Group: `PLAYTEST.md` → Session 2, 2l and Session 2-short step 4, after the merge.

## Open questions for Krogon

- The heart's dealing (one item at a time in turn) is Claude's reading of decision 6; it shows only with two players (2l step 4).
- Later: custom drop sounds like Diablo 4's (decision 15).

## Next steps

1. PR and merge; the coordinator raises Corruption's version.
2. Session 2, 2l at the next friend evening.
3. Later loot work: uniques and the ring slot (L6, #34).

## Checks

- Build: PASS (698 tests; Corruption builds; Orlog and CraftingOverhaul unchanged).
- Version: gameplay changed: version bump needed at merge (Corruption); no version changed here.
- Multiplayer sync: PASS (read in code): owner, time and beam on the drop's ZDO; the striker's id in `Corruption_HarvestChances` (read tolerantly); no new config.
- Assets and names, FEATURES.md: PASS.
- In-game: PASS alone on 1.0.30 (Krogon, 2026-11-26); two players not yet.

## Merge notes

**Ready to merge: yes** (final play-test passed, checks pass, 0 behind `master`, pushed). **Gameplay changed: version bump needed at merge (Corruption)**; a network message changed, so everyone and the server need the new build. No other open branch has unmerged commits. Shares the shared docs and `CorruptionPlugin.cs`, `CorruptionConfig.cs`. **Coordinator:** the phone page's master steps about the rip chest (`srv-8`, `duo-7`, `mpe-2k-3`, `mpe-rip`) and boss shards (`srv-9`, `mpe-boss`, `mpe-corrupt`) need the new wording (`PLAYTEST.md` here has it); this round's group steps are on the page, marked "after the personal-loot round merges".
