# Handoffs

One file per branch: `docs/handoffs/<branch>.md`, where `<branch>` is the branch name with `/` replaced by `-` (e.g. `claude/affixes-loot` → `claude-affixes-loot.md`). Never a shared file. The session working on that branch owns it; the coordinator reads it to update [`../../STATUS.md`](../../STATUS.md) and [`../DECISIONS.md`](../DECISIONS.md).

**Every session ends by typing `/handoff`** ([`.claude/skills/handoff/SKILL.md`](../../.claude/skills/handoff/SKILL.md)): it checks the branch, runs the checks, writes this file, commits and pushes. Add to **Decisions** the moment Krogon decides something, not only at the end.

Under 500 words; only the Decisions list may go past that (decisions are never dropped). Sections, in this order:

1. **Header line:** date written; branch state (commits ahead of and behind `master`, open PR if any).
2. **Stage:** one of *asking Krogon questions* · *building* · *waiting for Krogon's playtest* · *fixing feedback* · *ready to merge*.
3. **Purpose:** what the branch is for, in one or two sentences.
4. **Done:** only work that is built **and** confirmed by Krogon in game.
5. **Half-done and known issues:** built but untested, unfinished, known limits, assumptions.
6. **Decisions:** every decision Krogon has made on this branch, with the date and what he chose. Keep earlier ones, add new ones, never invent one. The coordinator copies them into `docs/DECISIONS.md`.
7. **Playtest:** what Krogon needs to test next, and his feedback so far. The full checklists live in `docs/playtests/<branch>.md`.
8. **Open questions for Krogon.**
9. **Next steps:** the next 3–5 tasks, most important first.
10. **Checks:** the results of `/handoff`'s checks (build, version, multiplayer sync, assets and names, FEATURES.md, in-game), each PASS / FAIL / SKIPPED / NOT SET UP, or "needs Krogon's playtest".
11. **Merge notes:** ready to merge yes/no and why; files this branch shares with other open branches or with the shared docs (`FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `Corruption/CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Content/Texts.cs`).

**Ready to merge** means: Krogon's final play-test passed, no check FAILED, the branch has the latest `master` merged in, and everything is pushed.

Handoffs written before 2026-11-06 use an older, longer layout; `/handoff` moves them to this one the next time it runs on that branch, keeping every decision and open question.
