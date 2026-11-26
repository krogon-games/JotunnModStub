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
