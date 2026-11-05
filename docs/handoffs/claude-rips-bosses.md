# Handoff: `claude/rips-bosses`

**Written:** 2026-11-05, from the real branch state. `git status` was clean before writing. The branch was **3 commits ahead of `master` and 27 behind**; `master` was merged in (two conflicts in `FEATURES.md` and `docs/PLAYTEST.md`, both kept). All three plugins build against stand-in game assemblies and **262 tests pass**. **Updated 2026-11-05:** PR #22 (the dedicated-server fixes, your rip decisions and this file) is merged, so **all of this branch's work is in `master`** (PRs #2, #4, #5, #6, #10, #14, #22) and the branch was brought level with it. Start the next session from the latest `master`.

Detail lives elsewhere; this file points to it:
- `FEATURES.md`: Maps and rips (step A), Rip waves and loot (step B), Rip heart search and corrupted wishbone, Corrupted offerings, Corrupted bosses (core, with summons), Boss essences.
- `docs/corruption-design.md`: the design (rips, map tiers and affixes, corrupted bosses, scars).
- `docs/ROADMAP.md` §4.3: tasks R1–R6.
- `docs/DECISIONS.md`: rows dated 2026-10-28 to 2026-11-01.
- `docs/ARCHITECTURE.md` → Server messages and Dedicated-server review.
- `docs/PLAYTEST.md` → Session 2, Parts 3 and 4 (the group's rip run and boss fight).
- Phone checklist: https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht ("Session 1c: solo").

## Purpose

The rips and bosses session's branch for the **Corruption** plugin: maps and rips, rip waves and loot, the corrupted bosses (offerings and summons) and the server messages. Next up in this stretch: map tiers and affixes (phase 5), then elite packs, then scars.

## Stage

**Asking you questions.** The map tiers and affixes (R1) need your approval of a table of proposed numbers before building (see Open questions). The dedicated-server review (R4) is finished and merged (PR #22).

## Done (built and tested in-game by you, single player)

All in `master`:
- **Boss essences** for 8 bosses, the Hive left out; each opens maps into its own biome.
- **Maps and rips, step A:** 8 maps crafted at the Riftforge. A rip opens to unexplored land of the map's biome, away from everyone's bases. Destroying the heart opens the way home.
- **Rip waves and loot, step B:**
  - waves of the biome boss's own army raid, busier than vanilla (`WaveMultiplier` 2);
  - every wave creature has at least 1 star (`MinStars` 1);
  - maddened creatures: red, ×1.2 damage, +15% speed, never flee, ignore charms, fight each other;
  - the end chest is a vanilla biome chest plus 1–3 shards.
- **Heart search:** the heart stands 96–128 m from the landing spot. The **Corrupted wishbone** (Wishbone + 1 shard) pings towards it, mostly red; the pings stop when the heart dies.
- **Map icons:** cut from your render, one per biome.
- **Corrupted offerings:** every boss has one, with your recoloured icons.
  - Altars take the full offering + 1 shard. Stand altars need a corrupted item on every stand; a mix is refused.
  - The corrupted Sealbreaker opens the Queen's door.
  - Kall's phases stay corrupted.
  - Tested: Eikthyr, Moder, the Queen, Kall, the fixed seed and blood icons.
- **Corrupted bosses:** ×1.5 damage dealt, ×0.7 taken, more essences and shards, red tint (`co_corrupt` for testing).
- **Corrupted boss summons:** every 30 s, 2–3 of the boss's army arrive like a raid, with 1–2 stars, at most 4 alive per player. They stop when the boss dies. Tested with Eikthyr and Bonemass.

## Half-done

- **Map tiers and affixes (R1):** your answers are recorded (one map per group, 4 tiers, the chest's shards per tier). The proposal for the missing numbers is waiting for your approval. No code yet.
- **Elite and champion packs (R3):** your rule is recorded (every creature above 2 stars is one). The design itself isn't started.

## Broken or unknown

- **Multiplayer: nothing is tested** with more than one player. The group evening is `PLAYTEST.md` → Session 2.
- **Two multiplayer fixes from the dedicated-server review** (merged in PR #22; untested, read in code but never run on a server):
  - wave and summon limits now count creatures far from the game that spawned them;
  - creatures maddened where they stand now turn red for every player.
- **Summons fight players only:** the fix after your 2026-10-31 test (summons and their boss attacked each other) is in `master`, but your re-check on the phone checklist (`sum-allies`) has no result yet.
- **Known, harmless:**
  - Rips and hearts far from everyone close only when someone is next near them.
  - The yellow `GetBiome error` warnings at biome borders come from the game itself.
- **Open from the design:** rips can still land in or beside a location (village, crypt) or in water (R5). Corrupted bosses have no particles yet (R5).

## Decisions (yours, this session)

Rips and maps:
- **The Hive** is left out of development entirely.
- **Essences:** each boss's essence makes maps into its own biome (Eikthyr → Meadows, …).
- **Map recipes:** 1 shard + 1 essence + the biome material. You approved the Meadows (Leather scraps 5) and Deep North (Ice 5) materials. Amounts are the median of vanilla recipes.
- **Unexplored land:** "I doubt we'll explore most of the Meadows by the time we craft the first portal", so rips only open to unexplored land.
- **Rip tech:** built as proposed. Maps sit in the normal inventory, rips are red, and the feature came in two steps (travel, then waves and loot).
- **Rip step B:** maddened creatures ignore charms; the end chest holds 1–3 shards; rip creatures drop shards at twice the normal starred chances.
- **Waves:** the vanilla pace was too slow ("a lot of the time I was just standing around waiting"), so each raid's most-alive-at-once is doubled and its interval halved (`WaveMultiplier` 2). They then still weren't challenging, so every wave creature has at least 1 star (`MinStars` 1): "definitely more challenging".
- **Heart search:** "the heart is right next to the portal; players need to go find it." The heart is now 96–128 m away. Your corrupted wishbone idea: crafted from the vanilla Wishbone + 1 shard, everyone sees the pings, mostly red with bits of purple (it looked mostly purple at first).
- **Map icons:** the first render looked "too similar, fine details get lost". Each map now has its own colour, one bold silhouette and its own roller material. The ground look stays the leather bundle.

Corrupted bosses:
- **Corrupted offerings:** the full normal offering + 1 shard makes one corrupted item. The Queen gets a corrupted Sealbreaker.
- **Offering icons:** "way too over the top" at first. You chose the vanilla icon recoloured, with a glowing crack and a smoky glow, from your own prompt.
- **Stand altars** (Moder, Yagluth, Fader): every stand must hold a corrupted item, each 1 item + 1 shard.
- **Summons:**
  - the boss's army raid, arriving like a raid (40–80 m from a player);
  - "tough fights": at least 1 star;
  - at most 4 alive per player present (12 max if per-player scaling wasn't possible).
- **Summons are allies:** they target players only, never their boss, each other or other creatures (after your play-test).
- **Elite and champion packs:** any creature above 2 stars is an elite or champion, Diablo/PoE style. That's for later; until then waves and summons stop at 2 stars.

Roadmap answers (2026-11-01):
- **One map, one rip for the group:** one player opens it and everyone goes through.
- **The end chest's shards per tier:** tier 1: 1–4, tier 2: 2–6, tier 3: 3–8, tier 4: 4–10. They replace today's 1–3. **Four tiers**; a fifth was a slip.
- **Scars** expire after 3 in-game days.

Working rules:
- **Never guess numbers or choices:** take them from the design or the game data, or ask.
- **One branch per session**, merge `master` before each task, open a PR when a feature is finished; you merge it.

Earlier, before the roadmap split the work (now owned by the loot session):
- the affix design: twins, weights, one combined pool, gathering twins for all four;
- gathering behaviour ("chance to double");
- the gear affix range rule;
- the PoE names for the orbs, and the orb art.

## Playtest feedback (what you reported, what changed)

| Date | You reported | Changed |
|---|---|---|
| 2026-10-28 | Rip hover showed `{0}`; a red LightFlicker error | Fixed placeholders and the heart's light; re-test passed |
| 2026-10-29 | Waves never started, PC crashed | You'd built `master`, which didn't have step B yet (the crash was a driver fault). Added a build stamp to the log |
| 2026-10-29 | Maddened creatures got no bonus ("not found") | Vanilla numbers are now read in the world; passed |
| 2026-10-29 | Waves too slow | `WaveMultiplier` 2; passed |
| 2026-10-29 | Heart next to the portal | Heart moved 96–128 m away, plus the corrupted wishbone; passed |
| 2026-10-29 | Pings mostly purple | Mostly red now; passed |
| 2026-10-29 | Waves still not challenging | `MinStars` 1; passed |
| 2026-10-29 | Map icons too alike | Second render with bold colours; passed |
| 2026-10-30 | Offering icons over the top | Recoloured vanilla icons; passed |
| 2026-10-30 | Seed and blood icons swapped | Swapped back; passed |
| 2026-10-30 | Kall's phases unknown | Tested with `spawn FrozenKing`, `co_corrupt`, `killall`; passed |
| 2026-10-31 | Summons and Eikthyr attacked each other | Summons fight players only; **re-check open** |

## Open questions (for you)

1. **R1 numbers** (the table in the chat of 2026-11-01). Approve or change:
   - **Waves per tier:** ×2, ×3, ×4, ×5 (tiers 1 to 4).
   - **Tier 2:** 1 in 4 wave creatures is a guaranteed 2-star.
   - **Rolling affixes:** the first roll costs 1 shard and gives 1–3 affixes; each reroll costs 1 more than the last.
   - **Frostbitten:** players in the rip are Freezing; +1 to the chest's shards.
   - **Drowned:** Wet and rain; trophies drop twice as often.
   - **Smouldering:** creatures add fire to their hits; loot affixes roll in the top half of their range.
   - **Choking:** smoke; the end chest gets 1 of the biome boss's essences.
   - **Swarming:** the wave multiplier doubled again; 1.5× item drops.
   - **Hardened:** creatures take 30% less damage; rip drops are at least Magic.
   - **Rabid:** maddened bonuses doubled (×1.4 damage, +30% speed); its "guaranteed 3-star" waits for elites.
   - **Tiers 3–4** wait for the elite design.
2. **Elite and champion packs (R3):** pack sizes, modifiers, names, loot.
3. **Scars:** confirmed (3 days); nothing else needed until R6.
4. **Escalation** after repeated corrupted kills (optional in the design): wanted?
5. From the group script: does `devcommands` work for you on the dedicated server?

## Next steps (most important first)

1. **Get your answers on the R1 table, then build tiers 1–2 and the seven affixes.** The tier and affixes live on the map item; until the UI session's Riftforge window exists, a console command does the rolling. The tooltip shows tier and affixes; the rip carries both. The three loot rewards need one small hook in the loot session's `LootPatches.cs` (today it always adds `rip_tier_1`): agree it with that session.
2. **The group evening** tests the dedicated-server fixes (merged in PR #22): fix what it finds.
3. **Design the elite and champion packs with you (R3)**, then 3-star creatures (R2: the enemy HUD shows only two stars) and tiers 3–4.
4. **R5:** keep rips out of locations and water once the area has loaded; boss particles (Bonemass cloud, Ashlands embers).
5. **After the group evening:** fix whatever Session 2 finds in rips and bosses. Later, R6: scars first (3 days), then terror zones and roaming elites.

## Merge notes

- **Nothing left to merge:** PR #22 (merged 2026-11-05) carried the dedicated-server fixes, your rip decisions and this file. The next work (R1) starts from the latest `master` and gets its own PR.
- **Code files PR #22 changed:**
  - `Corruption/Content/BossArmies.cs`: counts what it spawned, by id;
  - `Corruption/Content/Maddened.cs`: the `Corruption_Maddened` message, registered in a new `Game.Start` postfix.
- **Shared docs touched** (add, keep both sides in conflicts): `FEATURES.md`, `docs/DECISIONS.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ARCHITECTURE.md`, `docs/corruption-design.md`.
- **Shared code to watch in R1:**
  - `Corruption/CorruptionConfig.cs`, `Texts.cs` and `CorruptionPlugin.cs` (shared with the loot and UI sessions);
  - the loot session's `LootPatches.cs` (rip tier source);
  - the UI session's Riftforge window (map rolling as one of its operations; brief: `docs/riftforge-ui-brief.md`).
- **Patched game methods others also patch:** `BaseAI.IsEnemy` (talismans, maddened, summons: our postfixes run last) and `Character.Awake`. Recorded in `docs/mod-compatibility.md`.
