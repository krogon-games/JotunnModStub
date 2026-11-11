# Handoff: `claude/relaxed-cray-8dkrr2`

**Written:** 2026-11-11. Started from `master` (`bb353f4`), 0 commits behind it; ahead by its own document commits. No pull request yet.

## Stage

**Ready to merge** (documents only; nothing to play-test).

## Purpose

Coordinator session: review `STATUS.md` and the open issues, agree the top 3 priorities with Krogon, and bring the play-test checklists in line.

## Done

- `STATUS.md`: the top 3 priorities Krogon agreed; the deleted branch's row removed.
- The phone play-test page brought in line with `master` (today's version numbers, Session 2's 2g–2i, the two small checks of #27, the new solo-on-server session).
- `docs/PLAYTEST.md`: "Session 2-solo: alone on the dedicated server", and the two small checks under Session 1c.
- The phone page rule in `CLAUDE.md` and `docs/playtests/README.md`.

## Half-done and known issues

- The solo-on-server steps are taken from Session 2 and the handoffs; none has been run.
- `STATUS.md` says the "Use this at a Riftforge" refusal away from the station was never reported on, while step 17 of `docs/playtests/claude-lucid-carson-hs5g89.md` is ticked. Not settled here.

## Decisions

1. **2026-11-11: every round's solo checklist goes on the phone page from now on** ("i want every rounds solo checklist on the phone page going forward").
2. **2026-11-11: the top 3 priorities are agreed** ("Yes to the top three"): the release build now and playing together (#25, #24); orb drops as the next build round (#33), with the loot steps 3–4 play-test (#26) out of the priority; the Amazon's passives (#32). Map tiers (#31) stay fourth.
3. **2026-11-11: Krogon will carry out the solo play-tests.**

## Playtest

Nothing from this branch (no game code changed). The checklists it wrote are for `master`.

## Open questions for Krogon

1. Should this branch get a pull request into `master`? `STATUS.md` and `docs/DECISIONS.md` only count once they are there.
2. The four questions on the Amazon's passives (#32) and the map tiers table (#31) are still open.

## Next steps

1. Merge this branch (documents only).
2. Krogon: the release build (#25), then Session 2-solo alone on the dedicated server.
3. New sessions: orb drops (#33); the Amazon's passives (#32) once the four questions are answered.

## Checks

- Build: PASS. `dotnet test CraftingOverhaul.Tests`: 498 passed. No plugin code changed.
- Version: SKIPPED (no shipped file changed). No version bump needed.
- Multiplayer sync, assets and names, FEATURES.md: SKIPPED (no feature changed).
- In-game: nothing to test.

## Merge notes

- **Ready to merge: yes.** Documents only.
- **No version bump needed.**
- Touches the coordinator's files (`STATUS.md`, `docs/DECISIONS.md`) as the coordinator, plus `CLAUDE.md`, `docs/PLAYTEST.md` and `docs/playtests/README.md`. No other branch is open.
