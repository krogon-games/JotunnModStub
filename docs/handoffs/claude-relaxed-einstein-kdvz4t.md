# Handoff: `claude/relaxed-einstein-kdvz4t` (coordinator)

**Written:** 2026-11-06. Level with `master` before this file. The coordinator's work went straight to `master` (`c15c1a2`, `defc95d`, `e56e91f`) at Krogon's request; only this file isn't in `master`.

## Stage

Ready to merge (docs only).

## Purpose

Coordinator session: set up how the sessions work together.

## Done

- `STATUS.md`: branches, half-finished work, questions for Krogon, top 3 priorities, merge order.
- `docs/DECISIONS.md`: one table of every decision Krogon made, from all handoffs; the old technical log kept below it.
- `docs/GAME_DESIGN.md`: links to both full designs and every supporting note.
- Task list: GitHub issues #24–#52, labelled by area and priority; `docs/ROADMAP.md` maps them.
- `CLAUDE.md`: Session routine, coordinator-only files, version rule, cloud build.
- The `/handoff` skill (`.claude/skills/handoff/SKILL.md`); one handoff format (`docs/handoffs/README.md`); the play-test format (`docs/playtests/README.md`).
- Cloud build: the "Valheim Mod" environment's setup script (`tools/cloud-setup.sh`) and `VALHEIM_INSTALL`. **Confirmed by Krogon** with a fresh session's output: "all 262 passed", and all three plugins "Built, 0 errors".
- `.gitignore` blocks `.dll`s and Valheim/BepInEx folders.

## Half-done and known issues

- No GitHub Project board: no tool here can make one. Krogon can create it (Projects → New project → Board, then add the repo's issues).
- `/handoff` has run only here, on a branch with no code changes. Its first run on a feature branch with real changes is untested.
- Two old handoffs (`claude-affixes-loot.md`, `claude-ui-overhaul.md`) say their PRs are open; both were merged on 2026-11-02. Their sessions should fix this on their next `/handoff`.
- Build warnings in cloud builds: version-mismatch warnings from the stand-in game files (harmless), an unused variable at `CraftingOverhaul/Classes/VitalBars.cs:288`, and an unused field in `DemoClasses.cs` (removed with the demo, #38).

## Decisions

- 2026-11-05: One decision round per session, one branch per round; the session routine in `CLAUDE.md`.
- 2026-11-05: Only the coordinator, on `master`, edits `STATUS.md` and `docs/DECISIONS.md`.
- 2026-11-05: `master`'s `docs/corruption-design.md` wins over the older copies on other branches.
- 2026-11-05: The task list lives in GitHub issues if possible, otherwise in `ROADMAP.md` (issues were made).
- 2026-11-06: Every session ends with Krogon typing `/handoff`; it runs only when typed.
- 2026-11-06: Branches never bump versions; `/handoff` flags "version bump needed at merge"; the coordinator raises the version once when merging gameplay changes.
- 2026-11-06: Set up the cloud environment so the mod builds in cloud sessions (approved); Valheim game files are never committed.

## Playtest

Nothing to play-test in game: no gameplay changed. The cloud build check is done (above).

## Open questions for Krogon

1. Delete the two finished branches `claude/busy-wozniak-c0j9kc` and `claude/valheim-modding-exploration-izv3l8`? They hold nothing `master` lacks.
2. Do you want the GitHub Project board? If yes, create it yourself (see above).

## Next steps

1. Release build (#25), then the group evening (#24).
2. Start the next decision rounds, beginning with the loot orb rules (#28) and the Amazon's passives (#32).
3. After each merge, the coordinator copies the handoff's decisions into `docs/DECISIONS.md`, updates `STATUS.md`, and raises versions where gameplay changed.
4. Fold in the test session's handoff (`claude/awesome-ptolemy-gjigjh`) or delete that branch.

## Checks

- Build: **PASS**. Tests 262/262; no plugin changed (all three built earlier here and in the fresh session).
- Version: **SKIPPED** (no plugin changed).
- Multiplayer sync: **SKIPPED** (nothing new to sync).
- Assets and names: **SKIPPED** (none added).
- FEATURES.md: **SKIPPED** (no feature changed).
- In-game: **SKIPPED** (no gameplay change).

## Merge notes

Ready to merge: yes (this file only; docs). No version bump needed. Not behind `master`. The one other branch with unmerged work, `claude/awesome-ptolemy-gjigjh`, changes only its own handoff file; no overlap.
