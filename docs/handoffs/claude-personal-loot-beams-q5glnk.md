# Handoff: `claude/personal-loot-beams-q5glnk`

**Written:** 2026-11-26 (started; `/handoff` rewrites this header). Started 2026-11-26 from `master` at `e99092b`; no PR yet.

## Stage

**Fixing feedback.**

## Purpose

Personal loot ([#35](https://github.com/krogon-games/JotunnModStub/issues/35), ROADMAP L7): a drop rolled for one player is that player's for its first 60 seconds, invisible to everyone else; loot beams on every Corruption drop, with a sound for the rare ones; the rip heart spills its loot instead of leaving a chest.

## Done

Passed Krogon's solo play-test on Valheim 1.0.30 (2026-11-26, steps `pl-2` … `pl-15`; `pl-1` unmarked but its log line is there; clean log, every patch applied): his own drops and their beams, someone else's drop invisible and untouchable (auto-pickup, E, hover, crosshair), the reveal after the lock, the backpack, relogging, kill loot, boss shards, mining orbs, the rip heart's spill, the sound command, player-dropped items. Not yet tried with two players.

## Half-done and known issues

Built 2026-11-26 (feature detail: `FEATURES.md` → Personal loot and loot beams):
- Kill loot, each player's boss shard and Hoardfinder's / of Hidden Riches orbs carry their owner's player id and drop time (network clock) in the ZDO, written on the game that makes the drop; others don't see them (item, lights, beam hidden; no hover; crosshair looks through) or pick them up for 60 s.
- Beams on every Corruption drop (`loot_beams.csv`), sizes first guesses (`personal_loot.csv`); the sound is a placeholder (the skill-up sound) until Krogon picks.
- The rip heart spills its loot (no chest); the shared part is dealt among the players near it.
- Dev command `co_personal` (drop for a made-up other player, list, beams, sounds).
- Reviewed by Opus: the Harmony patches against the game code and the group's mods, then multiplayer; the fixes from both are in. A short third review of the two lock patches the multiplayer review added found nothing to fix; its one hardening (a refused pickup also ends the game's pickup retry loop) is in.
- **Game methods relied on, confirmed on Krogon's 1.0.30 build and log (2026-11-26: no `Failed to apply patch` line, the features worked)** (the cloud reads 1.0.14 code, #86): `Humanoid.Pickup` and `CharacterDrop.DropItems` (patched by name), `ItemDrop.Interact`, `ItemDrop.RequestOwn`, `ItemDrop.CanPickup`, `ItemDrop.AutoStackItems`, `ItemDrop.Awake`, `ItemDrop.GetHoverText`, `Player.FindHoverObject` (our copy of its 1.0.14 loop), `Player.AutoPickup` skipping `m_autoPickup == false`, `DropTable.GetDropListItems`, `ZNet.GetTime`, `Player.GetPlayerID`, `Player.m_skillLevelupEffects`, `LightLod`. If a patch fails, the log says `Failed to apply patch … PersonalLootPatches+<name>`.
- Known limits: each game reveals a drop by its own copy of the network clock (a second early or late); on a dedicated server the clock, and so a lock, pauses while nobody is online. Sleeping moves the clock on, so it ends every running lock. A game on an older 0.9.0 build joins without a warning but shows everyone's locked drops: same build everywhere for the group test.

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

## Playtest

Solo: `docs/playtests/claude-personal-loot-beams-q5glnk.md` (15 steps, `pl-1` … `pl-15`, also on the phone page). Group: `PLAYTEST.md` → Session 2, 2l, and Session 2-short step 4 (on the phone page `duo-4b`). Solo feedback 2026-11-26: 14 of 15 passed, the beams' look to change (decisions 13–14); details in the playtest file.

## Open questions for Krogon

- How the heart's shared loot is dealt between players (one item at a time in turn) is Claude's reading of decision 6; alone it all goes to you, so it shows only with two players (Session 2, 2l step 4).
- Later, not this round: custom drop sounds like Diablo 4's (decision 15).

## Next steps

1. Fix the beams' look (decisions 13–14).
2. Krogon's final play-test, then the PR.
3. Group checks (Session 2, 2l) at the next friend evening.

## Checks

Not run yet (`/handoff` runs them). By hand on 2026-11-26: `dotnet test CraftingOverhaul.Tests` 684 passed; Corruption builds against the cloud stand-in.

## Merge notes

Not ready (no play-test yet). **Gameplay changed: version bump needed at merge** (Corruption only; also a network message changed: `Corruption_HarvestChances` carries the striker's player id). Shares the shared docs (`FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`) and `Corruption/CorruptionPlugin.cs`, `CorruptionConfig.cs` (two descriptions). **For the coordinator at merge:** the phone page's master-build steps that still describe the rip chest (`srv-8`, `duo-7`, `mpe-2k-3`, `mpe-rip`) and the boss-shard steps (`srv-9`, `mpe-boss`, `mpe-corrupt`) need the new wording once this merges (`PLAYTEST.md` on this branch already has it); this round's group steps on the page are marked "after the personal-loot round merges".
