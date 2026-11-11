# Play-test checklists

One file per branch: `docs/playtests/<branch>.md`, where `<branch>` is the branch name with `/` replaced by `-` (e.g. `claude/affixes-loot` → `claude-affixes-loot.md`). Started 2026-11-05; older steps that are still untested stay in [`../PLAYTEST.md`](../PLAYTEST.md).

Each round adds a dated section to its branch's file:

```markdown
## Round: <what was built> (<date>)

**Build:** the build stamp from the log (so we know the right DLLs were tested)
**Setup:** world, console commands, mods to have loaded

### Checklist
1. [ ] Do this … → you should see that …
2. [ ] …

### Your feedback (<date>)
- Step 1: passed / failed: what you saw, in your words
- …

### What changed after the feedback
- Step 1: the fix, and whether the re-test passed

### Final play-test (<date>)
- Passed / what's left
```

Rules:
- Plain steps a non-developer can follow; say what should happen at each step.
- Write the user's feedback in their own words, then what changed. Never mark a step passed without their report.
- **Every round's solo checklist also goes on the phone page** (Krogon, 2026-11-11): the Corruption Play-test Log linked at the top of [`../PLAYTEST.md`](../PLAYTEST.md). Add the steps to the page when you write them here (one step id each, so Krogon's Pass / Fail / Skip and notes are saved per step), read his marks from it with `ArtifactData`, and copy them here as his feedback. Take a round's steps off the page once its play-test has passed. The page went stale between 2026-11-08 and 2026-11-11 because four rounds wrote only here.
- When a step needs other players, also add it to `PLAYTEST.md` → Session 2 (the group evening's script) and to the phone page's Session 2.
- When a step passes, update `FEATURES.md` (status, known issues) in the same commit.
