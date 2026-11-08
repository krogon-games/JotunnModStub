# Handoff: `claude/relaxed-einstein-kdvz4t` (coordinator)

**Written:** 2026-11-08. Level with `master` before this update. The coordinator's work went straight to `master` at Krogon's request (`c15c1a2`, `defc95d`, `e56e91f`, `ddc1c28`, `9425ce6`).

## Stage

Ready to merge (this file only).

## Purpose

Coordinator session: set up how the sessions work together.

## Done

- `STATUS.md`; `docs/DECISIONS.md` (one table of Krogon's decisions); `docs/GAME_DESIGN.md` links.
- Task list: GitHub issues #24–#52, labelled by area and priority; `docs/ROADMAP.md` maps them.
- `CLAUDE.md`: Session routine, coordinator-only files, version rule, cloud build.
- The `/handoff` skill (`.claude/skills/handoff/SKILL.md`) and the handoff and play-test formats.
- Cloud build: the "Valheim Mod" environment's setup script (`tools/cloud-setup.sh`) and `VALHEIM_INSTALL`. **Confirmed by Krogon** with a fresh session's output: "all 262 passed", and all three plugins "Built, 0 errors".
- `.gitignore` blocks `.dll`s and Valheim/BepInEx folders.
- 2026-11-08: merged the two handoff-only branches (this one, `claude/awesome-ptolemy-gjigjh`) into `master`.
- 2026-11-08, after PR #53 (`claude/admiring-babbage-rk1w8d`): Corruption and CraftingOverhaul 0.1.0 → **0.2.0** (Orlog stays 0.1.0); log checks in the docs, `DECISIONS.md` and `STATUS.md` updated.

## Half-done and known issues

- Every branch except `master` is fully in `master` (checked 2026-11-08), so all are safe to delete. Cloud sessions can't delete branches (the GitHub connection refuses); Krogon deletes them on GitHub. Keep this one while the coordinator session is in use.
- `/handoff` has run only here, on a branch with no code changes. Its first run on a feature branch with real changes is untested.
- Two old handoffs (`claude-affixes-loot.md`, `claude-ui-overhaul.md`) say their PRs are open; both were merged on 2026-11-02. Their sessions should fix this on their next `/handoff`.
- Harmless build warnings: stand-in version mismatches; unused bits in `VitalBars.cs:288` and `DemoClasses.cs` (#38).

## Decisions

- 2026-11-05: One decision round per session, one branch per round; the session routine in `CLAUDE.md`.
- 2026-11-05: Only the coordinator, on `master`, edits `STATUS.md` and `docs/DECISIONS.md`.
- 2026-11-05: `master`'s `docs/corruption-design.md` wins over the older copies on other branches.
- 2026-11-05: The task list lives in GitHub issues if possible, otherwise in `ROADMAP.md` (issues were made).
- 2026-11-06: Every session ends with Krogon typing `/handoff`; it runs only when typed.
- 2026-11-06: Branches never bump versions; `/handoff` flags "version bump needed at merge"; the coordinator raises the version once when merging gameplay changes.
- 2026-11-06: Delete every branch that is fully in `master` and has its handoff on `master`; keep and report any that isn't.
- 2026-11-06: Set up the cloud environment so the mod builds in cloud sessions (approved); Valheim game files are never committed.
- 2026-11-08: Merge the two handoff-only branches into `master`.
- 2026-11-08: Do the post-merge coordinator jobs for PR #53 now (version bump, decisions, `STATUS.md`).

## Playtest

Nothing new from this session. PR #53's multiplayer steps wait for the group evening (`PLAYTEST.md` → Session 2, 2e). On that evening, every log should show Corruption 0.2.0, Orlog 0.1.0 and CraftingOverhaul 0.2.0.

## Open questions for Krogon

None. (The Project board exists: Krogon created it and imported the issues, 2026-11-06.)

## Next steps

1. Release build from the latest `master` (#25), then the group evening (#24).
2. Start the next decision rounds, beginning with the loot orb rules (#28) and the Amazon's passives (#32).
3. After each merge, the coordinator copies the handoff's decisions into `docs/DECISIONS.md`, updates `STATUS.md`, and raises versions where gameplay changed.
4. Krogon deletes the old branches on GitHub.

## Checks

- Build: **PASS**. Tests 304/304; CraftingOverhaul and Corruption build with 0 errors (Orlog too, earlier today).
- Version: this session (coordinator, on `master`) raised Corruption and CraftingOverhaul to 0.2.0 after PR #53, as the rule says. Nothing on this branch needs a bump.
- Multiplayer sync: **SKIPPED** (nothing new to sync).
- Assets and names: **SKIPPED** (none added).
- FEATURES.md: **SKIPPED** (no feature changed).
- In-game: **SKIPPED** (no gameplay change).

## Merge notes

Ready to merge: yes (this file only; docs). No version bump needed. Not behind `master`. No other branch has unmerged work, so no overlap.
