# Handoffs

One file per branch: `docs/handoffs/<branch>.md`, where `<branch>` is the branch name with `/` replaced by `-` (e.g. `claude/affixes-loot` → `claude-affixes-loot.md`). The session working on that branch owns the file; the coordinator reads it to update [`../../STATUS.md`](../../STATUS.md) and [`../DECISIONS.md`](../DECISIONS.md).

Write it from the real branch state (`git status`, commits ahead of and behind `master`), at the end of every session, and add to **Decisions** the moment the user decides something.

Sections, in this order (see the existing files for examples):

1. **Header:** date written, branch state (ahead/behind `master`, open PR), test count.
2. **Purpose:** what this branch is for.
3. **Stage:** asking questions · building · waiting for the play-test · fixing · waiting for the final play-test · ready to merge.
4. **Done:** built and tested in-game by the user.
5. **Half-done.**
6. **Broken or unknown:** untested, known limits, assumptions.
7. **Decisions:** every decision the user made, with the date and their words where they matter. The coordinator copies these into `docs/DECISIONS.md`.
8. **Playtest feedback:** short summary; the full checklist and feedback live in `docs/playtests/<branch>.md`.
9. **Open questions** for the user.
10. **Next steps**, most important first.
11. **Merge notes:** ready to merge yes/no and why; shared files and game methods touched.
