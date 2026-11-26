# Handoff: `claude/festive-gates-79y1yr`

**Written:** 2026-11-26, at the start of the round. From `master` at `1966980` (after PR #88 and its coordinator update #89); 0 behind; no PR.

## Stage

**Asking Krogon questions.** The round and branch wait for Krogon's confirmation; then the map tiers and affixes table, one decision at a time (Open questions below). Nothing is built yet.

## Purpose

The rips round: map tiers and affixes ([#31](https://github.com/krogon-games/JotunnModStub/issues/31), ROADMAP R1: approve the table, then build tiers 1–2 with the seven affixes) and the explored-land fallback ([#84](https://github.com/krogon-games/JotunnModStub/issues/84), ROADMAP R7).

## Done

Nothing built yet. Read for the round: the design, the handoffs of the rips, orb-drops and personal-loot rounds, #31 and #84; four code maps (rips, Forge tab, tier hooks in the data, the rip request and server messages); the game's 1.0.14 code for weather, status effects, spawning and building.

## Half-done and known issues

- Found while mapping (to fix in this round): when the server answers a rip request, the game uses up the **first** map of that biome in the inventory, not necessarily the one that was used. Harmless while all maps of a biome are alike; with tiers and affixes it must take the map that was used.
- The rip heart's spill and kill loot always count as `rip_tier_1` today (`RipLoot.cs`, `LootPatches.cs`); `drop_modifiers.csv` already has `rip_tier_2` … `rip_tier_4`.

## Decisions

**Krogon's brief for this round** (2026-11-26, given before the confirmation; his words):
- "Use subagents."
- The round: "#31 (ROADMAP R1), approve the map tiers and affixes table, then build tiers 1–2 with the seven affixes. It also carries #84 (ROADMAP R7): rips fall back to explored land when no unexplored land of the biome is left."
- Already decided, not to reopen: one map per group, four tiers; tiers 3–4 wait for the elite and champion design (#39); maps named "{biome} map" for now; no rip chest (the heart spills the loot, each player's own rolls stay theirs, the shared part divided among the players near it); creatures drop their biome's map (1 in 830) once the world's first rip is cleared and that biome's boss is beaten; the heart's spill holds one map; for #84, "Fall back to explored land that's far from any player built structures." Unexplored land stays the first choice.
- Building: "The map's tier and affixes live in the item's custom data and travel with the rip request, so the server and every player's game agree. Old maps (no tier) must keep working as tier 1." "Rules in pure, unit-tested files; numbers in data tables under Corruption/Data (a map tiers and map affixes table); nothing guessed." "Tooltip lines for tier and affixes in the same layout as gear affixes." "The Forge tab gets map rolling: the main session sets the layout numbers, a Sonnet subagent builds it, and I see a preview picture first." Harmony patches written by a Sonnet subagent and reviewed by an Opus subagent "against the game code and mod-compatibility.md". Dev console commands "so I can test alone: make a map of any tier and affixes, show a rip's tier and affixes, and force the explored-land fallback without exploring a whole biome." The handoff names the game methods relied on (weather, status effects, the landing search), so his 1.0.30 build and log can confirm them.
- Checks: Claude runs the tests and builds and reads every diff, then "a separate review with an Opus subagent, focused on multiplayer and the dedicated server".
- No version changes; no edits to `STATUS.md` or `docs/DECISIONS.md`. With time to spare, tell him what related rips work could fit (e.g. R5, #40) "before starting anything else. Don't start a second area on your own."

## Playtest

Nothing to test yet. The checklist will be `docs/playtests/claude-festive-gates-79y1yr.md` and the phone page.

## Open questions for Krogon

Asked one at a time in the chat, each with options and a recommendation with its source:
1. The round and the branch.
2. Scope: all seven affixes now, or the three weather ones (Frostbitten, Drowned, Choking) in a later round.
3. Waves per tier (×2 today = tier 1; ×3 at tier 2 proposed) and tier 2's guaranteed 2-stars (1 in 4 proposed).
4. Shards per tier (1–4 / 2–6 / 3–8 / 4–10, decided 2026-11-01): dealt among the players, or per player.
5. The tier of a dropped map (creatures; the heart's map).
6. How a player gets a tier 2 map (the design: 1 extra shard).
7. Rolling affixes: 1 shard for 1–3, each reroll 1 more; how often 1, 2 or 3 affixes.
8. Orbs on maps (Path of Exile's way), or shards only.
9–15. Each affix: Frostbitten, Drowned, Smouldering, Choking, Swarming, Hardened, Rabid (danger, reward with personal loot, multiplayer).
16. Map names for tiers, and the tier on the icon.
17. #84: how far from player-built pieces, and for every rip or only the fallback.

## Next steps

1. Krogon confirms the round and answers the questions; each answer goes into Decisions and is committed at once.
2. Build: pure rules and data tables (Sonnet), Harmony patches (Sonnet, then an Opus review), the Forge tab's map operations after Krogon approves a preview picture.
3. Tests and builds by the main session, every diff read, an Opus review focused on multiplayer and the dedicated server.
4. Solo checklist in `docs/playtests/claude-festive-gates-79y1yr.md` and on the phone page; two-player steps in `PLAYTEST.md` → Session 2.

## Checks

Not run yet (nothing built).

## Merge notes

**Ready to merge: no** (nothing built). Will be "gameplay changed: version bump needed at merge (Corruption)". Expected to share `Corruption/CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Content/Texts.cs`, `Content/LootPatches.cs` and the shared docs.
