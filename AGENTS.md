# Subagents: who does what

**Asked for by Krogon, 2026-11-15**, after the skill tree overhaul round (`claude/laughing-mccarthy-fnz0to`) was built by the main session alone on the most expensive model: "wasnt a very efficient use of tokens".

**When to use it:** when Krogon says "use subagents" at the start of a round. The main session then keeps the decisions and the checking, and hands the mechanical work to cheaper models as below. Without his word, the main session works alone as before.

## The split

| Work | Who | Why |
|---|---|---|
| The session routine's reading, the questions to Krogon, the rules and layout decisions, the handoff's Decisions | Main session | The judgement calls; a mistake here costs a whole round |
| Finding every place that mentions the thing being changed (code, tests, data, docs) | Haiku, as a search agent (`Explore`) | Pure searching; it returns a list and the big files stay out of the main session's memory |
| Rules, data tables, the table generators (`tools/make_*.py`) and their unit tests | Sonnet | Well specified once the decisions are made |
| Anything that adds, changes or removes a Harmony patch | Sonnet, then a review by Opus | Small, but it hooks into the game's code; `docs/mod-compatibility.md` first |
| UI code from exact layout numbers and states | Sonnet | The main session supplies the numbers; the look is judged from the preview picture |
| Icons, preview pictures and other art scripts in `tools/` | Sonnet, beside the code work | Independent of everything else |
| `FEATURES.md`, the design docs, `CURRENT_STATE.md` and the other status docs | Sonnet, from a fact sheet the main session writes | Long files and a lot of reading |
| The play-test checklist file and the phone page | Haiku or Sonnet, from the main session's step outline | Formatting and publishing |

## Rules for handing work out

- **Give each subagent everything it needs in its prompt:** the decision in Krogon's words, the files to touch, the files not to touch, and what "done" looks like. A subagent starts cold and has not read this conversation.
- **Tiny jobs stay with the main session.** Every subagent re-reads `CLAUDE.md` and its files, so a five-minute edit costs more handed out than done.
- **No two subagents in the same file at once.** The shared docs (`FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`) and the tests' project file collide: run the docs work after the code work, not beside it.
- **Subagents never ask Krogon anything and never record a decision.** Questions come back to the main session.
- **The hard constraints in `CLAUDE.md` apply to every subagent** (no tuples, numbers from the data and never guessed, `Safe.Run` around code in game methods, and the rest). Say so in the prompt.

## How the main session checks the work

1. **Tests and builds, run by the main session itself:** `dotnet test CraftingOverhaul.Tests` and a build of each changed plugin. A subagent's "tests pass" is not taken on its word.
2. **Read every diff.** Reading a change costs far less than writing it. Look hardest at Harmony patches, saved-data changes (points, slots, custom data keys) and anything multiplayer.
3. **Leftover search:** when something is removed or renamed, search the code and the docs for its old name; anything still found is a miss.
4. **Patch list check:** the Harmony patch targets in `FEATURES.md` and `docs/mod-compatibility.md` compared against the `[HarmonyPatch]` lines in the code.
5. **Look at every picture:** icon sheets and preview images, opened by the main session before Krogon sees them.
6. **Quote check:** Krogon's decisions in the handoff and the design docs compared word for word with what he typed. A cheaper model paraphrasing him is the likeliest quiet error.
7. **Krogon's play-test stays the final gate.** Nothing is "Tested in-game" without his report.
