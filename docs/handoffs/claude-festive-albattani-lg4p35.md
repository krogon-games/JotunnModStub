# Handoff: `claude/festive-albattani-lg4p35`

**Written:** 2026-11-08 · **Branch:** ahead of `master` (`a6fda65`), 0 behind; no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

Second set of code-review fixes for the gathering affixes: Seismic and Odin's Grace on rock and ore, Veinbreaker's count, leftover tool chances on trees and logs, and a check (no fix yet) of Odin's Grace on one tree hit by two players at once.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built, untested in game:
- **Seismic and Odin's Grace on your own deposits:** the touching chunks are read before the hit is sent. Before, when your game ran the deposit, a hit that broke the chunk found no neighbours. Tool tier still applies; no chunk drops twice (tests simulate both owners).
- **Veinbreaker's** count starts over on any unequip of the tool (`Humanoid.UnequipItem` prefix).
- **Only your swings:** the owner applies cached tool chances only to a player's hit; physics and creature breaks give normal drops; the copper-deposit hand-on is kept. The owner logs each skipped case.
- **Odin's Grace, two games on one tree (suspected, not reproduced):** both may harvest the whole tree. A log line per whole-tree harvest and the test command `co_own` were added for the check; no fix until it's confirmed.
- Game code read in the cloud's 1.0.14 assemblies only; no 1.0.21 code with method bodies is available here.
- `/workspace/.setup/activate.sh` doesn't exist in this cloud session; ran `dotnet restore` once, then the given build and test commands.

## Decisions

- 2026-11-08: a tree, log or rock that breaks without your tool hitting it (a log rolling and smashing, a falling tree knocking another apart) gives **normal drops**: only your own swings get your tool's affixes. A rock that turns into a copper deposit still passes the pickaxe's affixes to the deposit ("No: only your swings").
- 2026-11-08: Veinbreaker's count starts over **whenever the tool is unequipped**, including putting it away with the hide-weapons key, swimming and eating with it in hand ("Yes, any unequip resets").

## Playtest

[`docs/playtests/claude-festive-albattani-lg4p35.md`](../playtests/claude-festive-albattani-lg4p35.md): A–C alone (about 20 minutes), D on the server (also `PLAYTEST.md` → Session 2, 2f). No feedback yet.

## Open questions for Krogon

None. D3 (two players, one tree) decides whether the whole-tree harvest needs coordinating.

## Next steps

1. Krogon's solo play-test (A–C); fix what fails.
2. The server check D, especially D3; if both games harvest one tree, make the tree's harvest happen once (coordinated before rewards spawn).
3. `/handoff`, then a PR into `master`.

## Checks

Build PASS (0 errors) · tests PASS (315) · in-game: needs Krogon's playtest. Full `/handoff` checks not run yet.

## Merge notes

Not ready (no play-test). Gameplay changed in Corruption: version bump needed at merge. Shared docs touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `Corruption/CorruptionPlugin.cs` (one command line), `docs/AFFIX_RULES.md`, `docs/ARCHITECTURE.md`, `docs/mod-compatibility.md`.
