# Valheim mods: CraftingOverhaul, Corruption and Orlog

Valheim 1.0 mods (BepInEx + Jötunn + Harmony) for a group of four friends on a dedicated server. Three plugins in one solution:
- `CraftingOverhaul/`: quality of life (recipe search and categories, biome grouping, craft/build from nearby chests). Works when only some players have it.
- `Corruption/`: Diablo/Path of Exile-style endgame. The server and every player need it.
- `Orlog/`: Diablo 2-style classes, levels and skill trees ([`docs/class-design.md`](docs/class-design.md)). The server and every player need it.

## Session routine (user, 2026-11-05)

I'm not a developer: explain things in plain language. Each session runs **one decision round**: you ask me questions, I decide, you build it and write me a play-test checklist, I play-test and give feedback, you fix it, I do a final play-test, then it's merged into `master`.

**At the start:**
1. Read [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) and the design files it links to that this round touches, [`STATUS.md`](STATUS.md), [`docs/DECISIONS.md`](docs/DECISIONS.md), the task list ([GitHub issues](https://github.com/krogon-games/JotunnModStub/issues), mapped to roadmap IDs in [`docs/ROADMAP.md`](docs/ROADMAP.md)) and this branch's handoff (`docs/handoffs/<branch>.md`) if there is one.
2. Bring in the latest `master` (`git fetch origin master && git merge origin/master`).
3. **Confirm the decision round and the branch with me** before doing anything else.

**During the round:**
- **One decision round per session, one branch per round.** A couple of related tasks per round is fine.
- **Every time I make a decision**, add it to the **Decisions** section of this branch's handoff file straight away, and commit.
- **Play-test checklists and my feedback** go in `docs/playtests/<branch>.md` (see [`docs/playtests/README.md`](docs/playtests/README.md)). **Put every round's solo checklist on the phone page too** (user, 2026-11-11): the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), linked from `docs/PLAYTEST.md`. Add the steps when you write the checklist, read my marks from it, and take the steps off again once the round has passed.

**At the end:** I type **`/handoff`** ([`.claude/skills/handoff/SKILL.md`](.claude/skills/handoff/SKILL.md)). It checks the branch, runs the checks, updates the handoff file ([`docs/handoffs/README.md`](docs/handoffs/README.md) has the sections), commits, pushes, and tells me whether the branch is **ready to merge** (and if not, what's missing). If I end a session without it, remind me to run it.

**Only the coordinator, working on `master`, edits `STATUS.md` and `docs/DECISIONS.md`.** Feature branches never touch them, so merges don't clash. The coordinator copies the handoffs' decisions into `docs/DECISIONS.md`.

File names: `<branch>` is the branch name with `/` replaced by `-` (e.g. `claude/affixes-loot` → `docs/handoffs/claude-affixes-loot.md`, `docs/playtests/claude-affixes-loot.md`).

## Read first

| File | Use it for |
|---|---|
| [`STATUS.md`](STATUS.md) | Where every branch stands, what's half-finished, what's waiting on the user, the top priorities. Coordinator only |
| [`docs/handoffs/`](docs/handoffs/README.md) | Each branch's handoff: stage, the user's decisions, open questions, next steps, merge notes |
| [`docs/CURRENT_STATE.md`](docs/CURRENT_STATE.md) | What's verified, built, broken, waiting on the user, and the next tasks |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | The plan for the current stretch: each session's tasks in order, the questions to ask the user first, where sessions meet. **Read your session's part before starting** |
| [`docs/PLAYTEST.md`](docs/PLAYTEST.md) | Older untested steps and the group evening's script (Session 2). New rounds write their checklist in `docs/playtests/<branch>.md`; add a step to Session 2 only for what needs the group |
| [`docs/playtests/`](docs/playtests/README.md) | Each round's play-test checklist and the user's feedback, one file per branch |
| [`FEATURES.md`](FEATURES.md) | Every feature: behaviour, config, known issues, exact Harmony patch targets |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Code layout, patterns to follow, game limits the code relies on |
| [`docs/DECISIONS.md`](docs/DECISIONS.md) | Every user decision (date, area, question, answer), then the technical log: rejected approaches, corrected facts. Coordinator only |
| [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) | Design overview and status, linking the full design docs |
| [`docs/AFFIX_RULES.md`](docs/AFFIX_RULES.md) | Affix rules and reasoning |
| [`docs/mod-compatibility.md`](docs/mod-compatibility.md) | Other mods the group runs (EquipmentAndQuickSlots, PlantEasily, AdventureBackpacks), where they patch the same game methods as us, and what to test. **Check it before patching a game method** |
| [`Corruption/Data/`](Corruption/Data/README.md) | **Tunable numbers**: `affixes.csv` (every affix), `affix_caps.csv` (stat caps), `drop_chances.csv` and `drop_modifiers.csv` (loot drop rates). Edit numbers here, never in the docs; `dotnet test` validates them |
| [`Orlog/Data/`](Orlog/Data/README.md) | Levelling tables (XP per level, XP per creature level, biome level ranges), made by `tools/make_orlog_tables.py` |

Game reference data: `data/game-dump-1.0.20/` (from `co_dump`; development only, never ship it). Diablo 2 class data for the class design: `data/d2-reference/` (never ship it).

## Build and test

- Tests (no game needed): `dotnet test CraftingOverhaul.Tests`
- Build: open `CraftingOverhaul.sln` on Windows (see `README.MD`), or
  `dotnet build <Project>/<Project>.csproj -p:VALHEIM_INSTALL=<Valheim folder> -p:ExecutePrebuild=false`
  (needs Valheim's managed assemblies; without a real install, a folder with the reference assemblies works).
- Cloud sessions: the "Valheim Mod" environment's setup script ([`tools/cloud-setup.sh`](tools/cloud-setup.sh)) installs the .NET 8 SDK and a stand-in Valheim folder at `/opt/valheim-standin`, and the environment sets `VALHEIM_INSTALL` to it, so the commands above work as they are. **Never commit Valheim game files or `.dll`s** (`.gitignore` blocks them).
- Only the user can play-test. Never mark anything "Tested in-game" without their report.

## Hard constraints

- **No assemblies Valheim doesn't ship:** no C# tuples (`System.ValueTuple`), `System.Memory`, `System.Buffers`. The build fails on them; use small structs or parallel arrays.
- **Building pieces: at most 5 ingredients** (`Hud.SetupPieceInfo` crashes on more).
- Register items and pieces in `PrefabManager.OnVanillaPrefabsAvailable`.
- Apply each patch class separately (`PatchAllSafely`); wrap code running inside game methods in `Safe.Run` / `Safe.Report` so failures fall back to vanilla.
- Put game-independent logic in pure files (no `UnityEngine`) linked into `CraftingOverhaul.Tests`, and test it.
- Data-driven over hand-written lists: derive from game data; config overrides only for real exceptions.
- Multiplayer: the owning client simulates creatures and objects; the dedicated server only relays. Claim ownership before changing an object.
- Gameplay config is admin-only (server-synced); visual config is per player.
- Content: clone and tint vanilla prefabs. **New animations are allowed where necessary** (user, 2026-10-30): use a vanilla animation or emote when one fits, a new one (asset bundle from `JotunnModUnity/`) only when none does. New models only where the user asks (so far the shard and essences), generated by scripts in `tools/` and embedded in the DLL. Norse names.

## Documentation rules

### FEATURES.md is mandatory

`FEATURES.md` at the repo root tracks every feature the mod adds or changes.

- Any commit that adds, changes, fixes or removes a feature must update `FEATURES.md` in the same commit: the summary table row (status, config options, files touched, last updated), the feature's section (behaviour, usage, config defaults, known issues, Harmony patch targets), and a dated Changelog entry, newest first.
- When the user reports play-test results, update the feature's status (`Tested in-game` or `Broken`) and known issues to match.
- Keep the Harmony patch target lists exact. They are used to spot conflicts with Valheim Plus and other mods.

### Keep the handover docs current

When a significant feature is completed, changed, deferred or play-tested, follow the update routine at the end of `docs/CURRENT_STATE.md` in the same commit (CURRENT_STATE, the handoff's Decisions section, and GAME_DESIGN / AFFIX_RULES / ARCHITECTURE where affected). Feature branches don't edit `docs/DECISIONS.md` or `STATUS.md`. Separate verified results from assumptions; don't record guesses as facts.

Don't duplicate: design detail lives in `docs/corruption-design.md`, `docs/AFFIX_RULES.md` and `docs/storage-quests-design.md`; tunable numbers in data tables under `Corruption/Data/`; feature detail in `FEATURES.md`. Other docs link to them.

## Branches and pull requests (user, 2026-10-28)

The user runs several Claude sessions at once, each on a different part of the mods. `master` is the shared, play-tested base.

- **One branch per session**, started from the latest `master`: `claude/<area>` (e.g. `claude/rips-bosses`, `claude/affixes-loot`). Never push to another session's branch.
- **Before starting a task**, bring in the latest `master` (`git fetch origin master && git merge origin/master`) so you build on the other sessions' merged work.
- **When a feature is finished** (built, tests pass, docs updated, ideally play-tested), open a pull request into `master` with a plain summary and the play-test status. The user merges it on GitHub.
- The shared docs (`FEATURES.md` changelog, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`) collide most often. Add to them, never rewrite another session's entries, and resolve merge conflicts by keeping both sides. `STATUS.md` and `docs/DECISIONS.md` are the coordinator's: in a conflict, keep `master`'s version.
- **Version numbers (user, 2026-11-06):** feature branches never change a version. `/handoff` flags "gameplay changed: version bump needed at merge" in the handoff and its report. **The coordinator raises the version once, when it merges gameplay changes into `master`**: the second number (`0.1.0` → `0.2.0`) of each plugin whose gameplay (code, data, assets) changed, in its `.csproj` (`<Version>`), its main `.cs` (`PluginVersion`) and `Package/manifest.json`. Jötunn compares only the first two numbers, so this is what stops players joining with different builds.
- `master`'s `docs/corruption-design.md` is the current design. Older branches carry an older copy; when merging, `master`'s version wins (see `STATUS.md` → Merge notes).

## Working style

- Plain, concise English; short answers when asked.
- Ask before changing an agreed design; record the user's decisions in the branch's handoff (Decisions section) as soon as they're made.
- **Never guess numbers or choices.** Take them from the design docs or derive them from the game data (`data/game-dump-1.0.20/`, the game code), and write down the source. Ask the user only when neither answers.
