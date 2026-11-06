---
name: handoff
description: End-of-session handoff for this branch. Checks the real git state, runs the repo's checks, writes docs/handoffs/<branch>.md, commits and pushes, then reports in plain language. Only when Krogon types /handoff.
disable-model-invocation: true
argument-hint: "[optional: Krogon's play-test results or notes]"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash(git status:*)
  - Bash(git diff:*)
  - Bash(git log:*)
  - Bash(git branch:*)
  - Bash(git fetch:*)
  - Bash(git rev-parse:*)
  - Bash(git merge-base:*)
  - Bash(git for-each-ref:*)
  - Bash(dotnet build:*)
  - Bash(dotnet test:*)
---

# /handoff

Krogon (the user) is not a developer: plain language, short sentences. Anything typed after `/handoff` is Krogon's own words for this session: $ARGUMENTS

Do the steps in order. Never invent a result, a decision or a command.

## Hard rules

- Never force-push, rebase, reset, stash, switch branches, merge, or discard changes you didn't make.
- Never edit or commit `STATUS.md` or `docs/DECISIONS.md` on a feature branch (coordinator only, on `master`).
- Never commit secrets or local settings: `Environment.props`, `*.user`, `.env*`, keys, `.claude/settings.local.json`.
- Don't install packages, change dependencies, or weaken a check to make it pass.

## 1. Check the real state

1. `git branch --show-current`. **If it is `master`, stop and ask Krogon** what to do. Change nothing.
2. `git fetch origin` (read-only), then `git status`, `git log --oneline origin/master..HEAD`, commits behind (`git log --oneline HEAD..origin/master`), and `git diff --name-only $(git merge-base HEAD origin/master)` (everything this branch changed, committed or not).
3. Sort the changes: **this session's** (files you edited in this conversation, or commits you made) and **not this session's** (anything else). Leave the second group untouched and list it in the report.

## 2. Run the checks

Mark each **PASS**, **FAIL**, **SKIPPED** (no change of that kind) or **NOT SET UP** (this machine can't run it), with one line why.

1. **Build.** Only the repo's real commands (CLAUDE.md → Build and test). If `dotnet` isn't installed: NOT SET UP. Otherwise run `dotnet test CraftingOverhaul.Tests`, then `dotnet build <Plugin>/<Plugin>.csproj -p:ExecutePrebuild=false` for each changed plugin (`CraftingOverhaul`, `Corruption`, `Orlog`). Add `-p:VALHEIM_INSTALL=$VALHEIM_INSTALL` only if that variable is set. Errors only about missing Valheim/BepInEx/Unity files count as NOT SET UP; any other error is FAIL. (On Windows the build also copies the DLL into the game's plugins folder; that's normal for this repo.)
2. **Version.** For each plugin whose shipped files changed (its folder except `README.md`, or `Shared/` if it links it), its version must be higher than `origin/master`'s in the first two numbers. Jötunn only checks those (`VersionStrictness.Minor`), so that's what makes all 4 players run the same version. The version is in three places, which must agree: `<Version>` in the `.csproj`, `PluginVersion` in the plugin's main `.cs`, and `version_number` in `Package/manifest.json`. If it hasn't been bumped and all three agree, set them to master's `X.(Y+1).0` and report "FIXED". If they disagree: FAIL.
3. **Multiplayer sync** (read the diff; say "read in code, not tested"). Gameplay settings use the admin-only config helper (`IsAdminOnly = true`); visual ones stay per player. New items and pieces are registered in `PrefabManager.OnVanillaPrefabsAvailable`. New data other players or the server need goes on a ZDO or a registered message (`docs/ARCHITECTURE.md` → Server messages), not only local fields. Data tables are compiled into the DLL, so they sync only through the version bump. CraftingOverhaul must still work when only some players have it. Nothing new to sync: SKIPPED.
4. **Assets, prefabs and names.** New `Assets/` or `Data/` files are included in the `.csproj` (`EmbeddedResource`; Corruption lists each CSV by name). Vanilla prefab names used in new code exist in `data/game-dump-1.0.20/`. New `$` text tokens have an entry in `Content/Texts.cs` (Corruption, Orlog).
5. **FEATURES.md.** If a feature changed, `FEATURES.md` changed in the same work (CLAUDE.md rule).
6. **In-game behaviour:** always **NEEDS KROGON'S PLAYTEST**, unless Krogon confirmed it in this session (then quote him).

## 3. Fix only small, obvious mistakes

A typo, a missing `EmbeddedResource` entry, a missing version bump as above. Report anything bigger instead of fixing it.

## 4. Write the handoff

Write or update `docs/handoffs/<branch>.md` (`/` in the branch name becomes `-`), in the sections and stage names of [`docs/handoffs/README.md`](../../../docs/handoffs/README.md). Under 500 words; only the Decisions list may push past that. Read the previous version first:
- **Decisions:** keep every earlier one, add new ones from this conversation with the date and Krogon's choice. Never invent one.
- Keep earlier open questions and unresolved issues unless this session dealt with them.
- **Done** lists only work that's built and that Krogon confirmed in game.
- **Merge notes:** list the files this branch changed that another remote branch with unmerged commits also changes (`git for-each-ref --format='%(refname:short)' refs/remotes/origin`, then `git diff --name-only origin/master...<that branch>` for each), and say if this branch is behind `master`.
- Full play-test checklists and feedback go in `docs/playtests/<branch>.md` (format: [`docs/playtests/README.md`](../../../docs/playtests/README.md)); the handoff summarises.

## 5. Commit and push

1. `git add` only this session's files, by name (never `git add -A` or `git add .`).
2. Commit even if a check failed. Then the message starts with `WIP:` and says what's failing.
3. `git push -u origin <branch>`. If it fails for a network reason, retry up to 4 times (waiting 2, 4, 8, 16 s). Never `--force`.

## 6. Report

```
HANDOFF COMPLETE | PARTIAL | BLOCKED
Branch: … · Commit: <short hash> · Stage: …
Build: PASS/FAIL/NOT SET UP · Failing: … (or "nothing")
Ready to merge: yes/no (why)
Not committed (not from this session): … (or "nothing")
Krogon's next step: … (e.g. "play-test these 4 things" or "answer these 2 questions")
```

- **COMPLETE:** committed and pushed, and no check FAILED.
- **PARTIAL:** committed and pushed, but a check FAILED (a `WIP:` commit).
- **BLOCKED:** on `master`, or the commit or the push failed. Say what's saved where (a commit that wasn't pushed lives only on this machine, which can be wiped).
