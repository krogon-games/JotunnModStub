# Handoff: `claude/affixes-loot`

**Written:** 2026-11-02, from the real branch state (`git status` clean, in sync with `origin`). The branch is 7 commits ahead of `master` (`dd93cb8`); `master` has nothing the branch lacks. **PR #18** is open from this branch into `master`, mergeable (clean), at the same commit (`7712290`). Earlier work from this branch is already in `master` through PR #11 (loot step 1) and PR #13 (player stats, Odin unlock).

Detail lives elsewhere; this file points to it: `FEATURES.md` → Loot system, `docs/CURRENT_STATE.md`, `docs/DECISIONS.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md` §4.2, `docs/riftforge-ui-brief.md`, `docs/AFFIX_RULES.md`.

## Purpose

The loot session's branch for the Corruption mod's loot system (design phase 7): Magic and Rare drops with rolled affixes, the affix effects, and the Riftforge crafting rules (orbs, corruption, shattering, Affix Splinters, uniques).

## Stage

**Waiting for your playtest** (Sessions 3c, 3d and 3e), with open questions for you (below).

## Done (built and tested in-game, single player)

- **Loot step 1** (in `master`, PR #11): Magic/Rare drops from kills, base items by biome (20% the biome before), level-1 drops, rolled affixes and twins, Magic and Rare names in colour, tooltip lines with `[ranges]`, item-stat affixes (damage, elemental, armour, durability, block, knockback, backstab, parry, attack stamina), ground hover names (after a fix), upgrades keep affixes, durability and repair, `co_loot`.
- **The actual bonus next to item-stat lines**, e.g. `(+23)` (in `master`).
- **Loot step 2** (in `master`, PR #13): max health/stamina/eitr, carry weight, movement, health and stamina regen, resistances.
- **Odin items unlocked** (in `master`, PR #13): equip passed, recipes announced.
- All gear affix ranges settled from the game data with you (`affixes.csv`, sources in `AFFIX_RULES.md`).

## Half-done

- **Riftforge crafting (ROADMAP L5):** the brief is written (`docs/riftforge-ui-brief.md`, PR #18); the rules (`RiftforgeCrafting.Options`/`Apply`) and the `co_craft` command aren't built. The pure pieces that exist: `CorruptionOutcomes` (25/25/50) and `ShatterRules` (50% per line, which items take a splinter).
- **Currency orbs (L4):** art for nine orbs exists (`Corruption/Assets/Orbs/`, not embedded); no items, no drops (needs your drop rates).
- **Personal loot:** each nearby player rolls separately, but drops aren't locked to the roller and there are no loot beams yet (L7).

## Broken or unknown

- **Not play-tested (in PR #18):** loot step 3, gathering tools: striker side (`PLAYTEST.md` → Session 3c) and on-harvest effects with Odin's Grace on trees (3d); loot step 4, proc and utility affixes (3e). All three are in the phone play-test log.
- **Not tried from earlier steps:** weapon skill levels, life steal, crafting an Odin item, natural drops with several players, the bigger backpacks (server config not changed yet).
- **Multiplayer:** nothing in this branch has been tested with another player. The on-harvest affixes and parry buffs depend on it most.
- **Known limits** (see `FEATURES.md` → Loot system): life steal uses damage before the target's armour; Hushed covers your own swing and hit noise only; the whole-tree harvest skips the vanilla tree statistics; taking off a Deathless item hides its cooldown icon until relog.
- 248 unit tests pass; Corruption, CraftingOverhaul and Orlog compile against stand-in game assemblies (the game itself isn't available in the cloud session).

## Decisions (yours, this session)

Every row is also in `docs/DECISIONS.md`.

- **Shard corruption:** 25% bricked, 25% unchanged but locked, 50% one corrupted affix on top of the normal limit (a Rare ends with 5). Normal and Magic items can be corrupted too. The corrupted affix rolls from the normal pool for now, may repeat a family, resonates with a twin, and ignores the caps. Corrupted gear only comes from a shard (the first draft's "drops with 1–2 bonus affixes" line was deleted).
- **Affix Splinters and shattering** (from Last Epoch): named Affix Splinters; shattering a Magic or Rare item at the Riftforge is free; 50% per affix line; uniques and corrupted items don't shatter; a resonant twin line gives one splinter with the combined value and counts as 2 affixes (only fits a Normal item); splinters go on Normal and Magic items with fewer than 2 affixes; they don't stack, so they need the Currency/Maps tabs.
- **Drops:** the base item is mostly from the creature's own biome, 20% from the biome just before; dropped at upgrade level 1; Magic 1 affix twice as often as 2, Rare 3 twice as often as 4.
- **Range rules:** "strongest vanilla source" counts gear only; item stats with no vanilla % source use what a full forge upgrade adds; resistances from a new `co_dump`. Then: carry weight cut by 100 to 10–50; max health/stamina/eitr and life steal keep their drafts; the other derived ranges approved; lightning resistance matches the other elements (10–50%); all resistances 2–10%.
- **No stacking caps** on carry weight, regen or max health/stamina/eitr for now: "our mod will be significantly harder than vanilla".
- **Bigger backpacks:** AdventureBackpacks' own sizes, one more row on every level (values in `docs/PLAYER_SETUP.md`), not a slot affix.
- **Show the actual bonus** next to item-stat affix lines, because the unchanging `[10–50]` read like a damage range.
- **Odin items: unlock fully** (equip, craft, loot). I had filtered them out of the loot without asking; you reversed it.
- **Process:** work on `claude/affixes-loot`, merge `master` before each task, open a PR when a feature is finished; follow the roadmap order (L1 brief first, then L2, L3); agents may be used.

## Playtest feedback (and what changed)

- Rares and Magic items from `co_loot` looked and worked right (screenshots) → recorded as passed.
- Ground hover name showed the base item → fixed (the game localizes before our patch); re-test passed.
- Upgrading worked, but the `[10–50]` didn't seem to follow upgrades → explained (the bracket is the roll range) and added the actual bonus `(+N)`; passed.
- "DLC required" on a Rare Odin cape → you chose to unlock the Odin items fully; equip passed.
- The Meadows satchel holds too little → bigger AdventureBackpacks sizes (server config; not applied yet).
- Player stats: max health/stamina/eitr, carry weight, movement, regen and resistances passed; weapon skill and life steal skipped.
- Logs: no errors from our mods.

## Open questions

Riftforge and orbs (decide L5's orb behaviour; also `riftforge-ui-brief.md` §6):
1. Orb of Annulment: when the last affixes come off, does the item drop a rarity or keep it?
2. Orb of Chance: the odds of Magic, Rare and Unique.
3. Mirror of Kalandra: can it copy a corrupted item?
4. Jeweller's Orb: stay greyed until sockets and runewords are designed?
5. Orb drop sources and rates (L4). Claude can propose numbers from the drop tables.
6. World unique pool contents (L6). Claude can draft a Norse starter list.

Gathering and procs (working defaults in place, recorded in `DECISIONS.md`):
7. Gemseeker's: Amber, Amber pearl and Ruby equally likely? (yes for now)
8. Stonecutter's and Resinous: roll on every rock/tree piece, even ones that drop no stone or resin? (yes for now)
9. Should life steal heal from Reprisal and Wrath damage? (yes for now)
10. Does Wrath's shockwave also hit the enemy you struck? (yes for now)
11. Foraging: wild plants only, or also flint, stones and branches? (plants only for now)
12. Rallying: bonus on every enemy that swing hits, or only the first? (every one for now)
13. Deathless: with several worn, the shortest cooldown applies? (yes for now)

## Next steps

1. **Play-test Sessions 3c–3e** (phone log or `PLAYTEST.md`), report results; fix what fails.
2. **Answer the open questions**, especially 1–4.
3. **L5: Riftforge crafting rules** behind the brief (`RiftforgeCrafting`, the Affix Splinter item, corruption and shattering in game) with `co_craft` until the UI session's window exists.
4. **L4: currency orbs** (items, Currency tab, drops) once the drop rates are agreed.
5. **L6–L7:** uniques with the ring slot; personal-loot locks and loot beams. Then the multiplayer evening (`PLAYTEST.md` → Session 2).

## Merge notes

- **Ready to merge?** Builds and tests pass and PR #18 is clean, but steps 3 and 4 are **not play-tested**. Suggestion: merge after the 3c–3e play-test, unless the UI session needs `docs/riftforge-ui-brief.md` in `master` first.
- **Shared with other sessions** (ROADMAP §3): `Corruption/CorruptionPlugin.cs`, `Corruption/Content/Texts.cs`, `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/DECISIONS.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/mod-compatibility.md`. Keep both sides in conflicts.
- **Game methods other sessions patch too:** `Character.Damage` (our prefix at `Priority.High` for Rallying and Skadi, plus life steal at Normal; Orlog's planned crits will patch it too), `Character.RPC_Damage` (resistances, next to the corrupted-boss prefix), `Character.OnDeath` (kill messages, next to offerings and Orlog XP), `Player.GetTotalFoodValue` (max health/stamina/eitr). Priorities are recorded in `docs/mod-compatibility.md`.
- The Riftforge window itself is the UI session's (ROADMAP U3); map rolling is the rips session's (R1), registered through the brief's API.
