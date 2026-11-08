# Handoff: `claude/festive-albattani-lg4p35`

**Written:** 2026-11-08 · **Branch:** started from `master` `a6fda65`; no PR yet.

## Stage

Building.

## Purpose

Second set of code-review fixes for the gathering affixes: Seismic and Odin's Grace on rock and ore, Veinbreaker's count, leftover tool chances on trees and logs, and a check (no fix yet) of Odin's Grace on one tree hit by two players at once.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Being built.

## Decisions

- 2026-11-08: a tree, log or rock that breaks without your tool hitting it (a log rolling and smashing, a falling tree knocking another apart) gives **normal drops**: only your own swings get your tool's affixes. A rock that turns into a copper deposit still passes the pickaxe's affixes to the deposit ("No: only your swings").
- 2026-11-08: Veinbreaker's count starts over **whenever the tool is unequipped**, including putting it away with the hide-weapons key, swimming and eating with it in hand ("Yes, any unequip resets").

## Playtest

Checklist to come in [`docs/playtests/claude-festive-albattani-lg4p35.md`](../playtests/claude-festive-albattani-lg4p35.md).

## Open questions for Krogon

None yet.

## Next steps

1. Seismic and Odin's Grace find the touching parts before the hit lands.
2. Veinbreaker's resets on unequip.
3. Leftover tool chances stop applying to breaks without a swing.
4. Two-player check of Odin's Grace on one tree.

## Checks

Not run yet (`/handoff`).

## Merge notes

Not ready. Gameplay changes in Corruption: version bump needed at merge.
