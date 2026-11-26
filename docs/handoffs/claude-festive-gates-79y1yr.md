# Handoff: `claude/festive-gates-79y1yr`

**Written:** 2026-11-26, at the start of the round. From `master` at `1966980` (after PR #88 and its coordinator update #89); 0 behind; no PR.

## Stage

**Asking Krogon questions.** Round and branch confirmed (decision 1); the map tiers and affixes table, one decision at a time (Open questions below). Nothing is built yet.

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

1. **This round and branch** (2026-11-26): "Yes" to #31 (bring the table up to date, approve it one question at a time, then build tiers 1–2 with the affixes) and #84 (the explored-land fallback), on `claude/festive-gates-79y1yr` (the rips area's old `claude/rips-bosses` is merged and finished); checklist `docs/playtests/claude-festive-gates-79y1yr.md` and the phone page.
2. **A smaller round** (2026-11-26): "Option a. Keep round smaller." This round builds the tiers (tier and affixes on the map, sent with the rip request, setting waves, stars and shards), the map tooltip, raising a map's tier and rolling its affixes in the Forge tab, the dev commands, the explored-land fallback (#84) and the four affixes that change the creatures: **Swarming, Hardened, Rabid, Smouldering**. The three weather affixes (**Frostbitten, Drowned, Choking**) wait for the next round, after the fresh 1.0.30 `co_dump` (#86) also records what each weather does and the game's smoke damage. Over "all seven now".
3. **Waves and stars per tier** (2026-11-26): "Option a". Tier 1 stays today's ×2 (most alive at once doubled, spawn interval halved; 2026-10-29); **tier 2 ×3** (1.5 times today's crowd, e.g. Black Forest 24 → 36, each kind trying a new group every 2 s instead of 3 s); **at tier 2, 1 in 4 wave creatures is a guaranteed 2-star**, the rest roll as today (1 star, the game's 10% chance of a second), about 1 in 3 two-star in all. Tiers 3 and 4 recorded for later as ×4 and ×5 (not built). Claude's proposal of 2026-11-01 (the design only says "more waves" and "2-star guaranteed on some"), over ×4 at tier 2 and "×2, only the stars change".
4. **Shards dealt; a floor and higher drop rates at tier 2** (2026-11-26): "Part A a, part B b. The higher tier maps / higher difficulty should raise the drop rates as well. At x1.5". (A) The tier's shards (tier 1: 1–4, tier 2: 2–6; decided 2026-11-01) are dealt one at a time among the players within 100 m of the heart, as decision 6 of the personal-loot round, over "each player rolls their own". (B) At tier 2, each player's gear roll at the heart always gives at least a Magic item (the design: "map tier sets the affix count and the rarity floor"), over "nothing more this round". Tier 2's drop rates ×1.5: see 4b.
4b. **Drop rates per tier** (2026-11-26, Krogon: "Tier 1 - everything x1.25 / Tier 2 - everything x1.5 / Tier 3 - everything 1.75 / Tier 4 - everything x2 / And so on"): `drop_modifiers.csv`'s `rip_tier_1` … `rip_tier_4` become ×1.25, ×1.5, ×1.75, ×2 in every column (Magic, Rare, unique, orbs; the orb column also sets creature map drops), +0.25 per tier for any later tier. Changes tier 2's Magic, Rare and orbs from ×1.25 to ×1.5 and tier 3's from ×1.5 to ×1.75 (the uniques and tier 1 and 4 rows were already these; approved 2026-10-27). Claude reads "everything" as those drop-table columns; shards keep their own rules (decision 4A) unless Krogon says otherwise.
5. **Every dropped map is tier 1, with no affixes** (2026-11-26): "Option a". Creature map drops and the one map the heart spills are tier 1; raising a map to tier 2 costs its shard every time (the design: tier 2 "set by 1 extra shard"). Over "the heart's map is the rip's own tier" and "the rip's tier with a chance of one higher". A dropped map never carries affixes.
6. **Design change: a map's tier is its rarity and affix count** (2026-11-26, answering "how does a player get a tier 2 map?"). Krogon: "This is where we make a design change: Tier 1 is a white item / Tier 2 becomes a magic map with 1 affix / Tier 3 magic map with 2 affixes / Tier 4 rare map with 3 affixes / Tier 5 rare map with 4 affixes / New tier 6 corrupted map via a shard on top of a tier 5 map that already has 4 affixes". **Replaces** the design's tier table (tier 2 "1 extra shard", tier 3 "3-star creatures", tier 4 "3 extra shards + a boss trophy") and the "four tiers" of 2026-11-01: there are six tiers, and a map's tier follows from its affixes (and the shard for tier 6). How affixes get onto a map, what corrupting does, what else each tier changes (waves, stars, shards) and which tiers this round builds: asked next.
6b. **Orbs put affixes on maps, exactly as on gear** (2026-11-26): "A". Transmutation: white → tier 2; Augmentation: tier 2 → 3; Alchemy, or Chance (always Rare, as no unique map exists): white → tier 4; Regal: tier 3 → 4; Exalted: tier 4 → 5; Annulment removes one random affix, so the map drops a tier; Divine does nothing (map affixes have no ranges); the Mirror of Kalandra copies a map, as it copies gear; the shard corrupts a full tier 5 map into tier 6 (decision 6). Map affixes have no prefix/suffix sides: any different affixes, up to the tier's count. Over "shards only" and "both". **Replaces** the design's "spend extra shards at the bench to roll modifiers onto a map … a reroll costing one more shard" and the 2026-11-01 proposal of 1 shard for 1–3 affixes (old questions 7 and 8 are settled by this).
6c. **Corrupting a map never breaks it** (2026-11-26): "B. We should make corrupted only affixes that add fun and danger". A shard on a full tier 5 map always makes it tier 6, corrupted and locked (no orb changes it afterwards, as corrupted gear; the Mirror of Kalandra can still copy it, and the copy stays corrupted), following the design's "Rolling map affixes never bricks the map". Over "like gear" (25% destroyed / 25% nothing / 50% a 5th affix) and "always a 5th normal affix". Krogon's new idea: **corrupted-only map affixes** that add fun and danger.
6d. **Corrupted-only affixes come in a later round** (2026-11-26): "Later round". This round a tier 6 map is corrupted and locked, and its tier alone sets its waves, stars, shards and drop rates; once corrupted-only affixes exist, every corrupted map gets one. Over "propose a short list now". Ideas Claude offered for the later list, none chosen: Haunted (the game's ghost weather), Undying (rip creatures rise once more), Fenrir's Hunt (wolves join every wave), Endless Night (always night, night-only creatures come).
6e. **The tier table** (2026-11-26): "Table as proposed. Waves a". **Replaces decision 3's waves and stars** (made for tiers without affixes) and extends decision 4:

    | Tier | Map | Waves | Guaranteed 2-stars | Heart shards | Drop rates | Heart gear (each player's roll) |
    |---|---|---|---|---|---|---|
    | 1 | white | ×2 (today) | none (about 1 in 10 by chance) | 1–4 | ×1.25 | by chance (about 1 in 27) |
    | 2 | magic, 1 affix | ×2.5 | 1 in 4 | 2–6 | ×1.5 | at least Magic |
    | 3 | magic, 2 affixes | ×3 | 1 in 2 | 3–8 | ×1.75 | at least Magic |
    | 4 | rare, 3 affixes | ×3.5 | 3 in 4 | 4–10 | ×2 | at least Rare |
    | 5 | rare, 4 affixes | ×4 | all | 5–12 | ×2.25 | at least Rare |
    | 6 | corrupted | ×4.5 | all | 6–14 | ×2.5 | at least Rare |

    Waves: +0.5 per tier (most alive at once and spawn speed; Black Forest 24 … 54; with Swarming doubling, up to 96 at tier 5 and 108 at tier 6), over +1 per tier and ×2 everywhere. Sources: drop rates = decision 4b; shards = 2026-11-01 for tiers 1–4, the same step (+1 lowest, +2 highest) for 5–6; 2-stars = decision 3's 1 in 4 at tier 2, +1 in 4 per tier (Claude's proposal); heart gear = decision 4B's tier 2 floor, "at least Rare" once the map is Rare (Claude's proposal); waves = Claude's proposal. 3-stars still wait for the elite design (#39). Every number goes in a data table, tunable without a rebuild.
6f. **All six tiers are built this round** (2026-11-26): "A". White, magic and rare maps with the orbs (decision 6b) and the shard's tier 6 (decision 6c), with the table of 6e; with only four affixes this round (decision 2), a tier 5 map carries all four and tier 6 is that map corrupted. Over "tiers 1–3 now" and "tiers 1–2 only". **Replaces** "build tiers 1–2" and "tiers 3–4 wait for the elite and champion design (#39)": only 3-star creatures still wait for #39.
7. **Swarming** (2026-11-26): "Your recommended." Danger: the tier's waves ×2 on top (the design: "Doubles spawn count"; Black Forest most alive at once 60 at tier 2 … 108 at tier 6), tunable in the data table, over "+50% on top". Reward: every Corruption drop in the rip ×1.5 as likely (Magic, Rare and unique gear, orbs, maps; from rip creatures and the heart's rolls; on top of the tier's rate; shards and essences keep their own rules), Claude's 2026-11-01 proposal (1.5 as `star_2`, `scar`, `terror_zone`), over ×1.25 and no extra. With personal loot it applies to each player's own rolls.
8. **Hardened: danger only, no reward of its own** (2026-11-26): "That's old design idea, scrap it. Keep the affix hardened but let's make it one of the affixes that rolls via the orbs or shards when upgrading a map". Rip creatures take 30% less damage (the design); the design's reward "higher rarity floor" is scrapped. Hardened is simply one of the affixes the orbs roll onto a map (decision 6b), and its reward is the tier it adds (decision 6e). Over a floor of its own (raise the heart's floor, Magic → Rare drops in the rip, a second item at the heart).
8b. **Every map affix is danger only; the tier pays** (2026-11-26): "A, every affix danger only. I must have misunderstood decision 7." No affix has a reward of its own: each pays through the tier it adds (decision 6e: drop rates, the heart's shards and floor, more 2-stars). **Replaces decision 7's reward** (Swarming's ×1.5 drops is dropped; its danger, waves ×2 on top, stays) and the design's per-affix rewards (Rabid's "guaranteed 3-star" as a reward, Smouldering's "better gear rolls", and the weather affixes' for the later round). Over "only Hardened".
9. **Rabid** (2026-11-26): "A". The maddened bonuses doubled, as the design says ("Maddened effect doubled"): damage ×1.4 instead of ×1.2 (the game's Dvergr power), speed +30% instead of +15% (the Tonic of Ratatosk). Over "half again" (×1.3, +22.5%). The design's "guaranteed 3-star" waits for the elite design (#39), which decides whether it becomes part of Rabid's danger.
10. **Smouldering** (2026-11-26): "A". Every hit a rip creature makes (on players and on other creatures) also carries **25% of the hit's damage as fire**, which the game spreads over 5 s as burning; the Fire Resistance Barley Wine halves it. No source for the number (the design gives none, the dump has no damage types): Claude's proposal, the smallest step, about maddened's own +20%, tunable in the data table. Over 50% and 100%.
11. **Map names and display** (2026-11-26): "b, yes yes yes". The name stays "{biome} map" ("Meadows map"), coloured by rarity as gear is (white, Magic blue, Rare yellow); only tier 6 becomes "Corrupted {biome} map", in red. Over Path of Exile-style names (affixes in Magic names, random Norse names for Rares) and the tier in the name. Yes to all three extras: (1) the tier as a small number in the corner of the map's icon, where the game shows an item's quality; (2) the rip's portal and heart show its tier and affixes on hover, so the whole group sees what they walk into; (3) the tooltip in the gear style: a "Tier 3 · Magic" line in the rarity colour, each affix as a bullet with its danger, the red "Corrupted" mark at tier 6.

## Playtest

Nothing to test yet. The checklist will be `docs/playtests/claude-festive-gates-79y1yr.md` and the phone page.

## Open questions for Krogon

Asked one at a time in the chat, each with options and a recommendation with its source. The list was checked by an Opus review against the game code and data before asking (it corrected the spawn pace, the weather facts and the frost-resistance gear, and added the missing questions). "Proposal" means Claude's table of 2026-11-01, which has no source in the design or the data.
1. The round and the branch.
2. Scope: recommended to build the tiers, #84 and the four creature-side affixes (Swarming, Hardened, Rabid, Smouldering) now, and the three weather ones (Frostbitten, Drowned, Choking) next round, after the fresh 1.0.30 `co_dump` (#86) also records what each weather does and the game's smoke damage.
3. Waves per tier (×2 today = tier 1; ×3 at tier 2, proposal) and tier 2's guaranteed 2-stars (1 in 4, proposal).
4. Shards per tier (1–4 / 2–6, decided 2026-11-01), dealt as decision 6; whether tier 2 adds anything else at the heart (the design: "map tier sets the affix count and the rarity floor").
5. The tier of a dropped map (creatures; the heart's map).
6. How a player gets a tier 2 map (the design: "1 extra shard").
7. Rolling affixes: the design's "a reroll costing one more shard" (every roll 1 shard, or rising); how often 1, 2 or 3 affixes.
8. Orbs on maps (Path of Exile's way), or shards only.
9–12. Swarming, Hardened, Rabid, Smouldering: danger, reward with personal loot, multiplayer.
13–16. Only if built now: Frostbitten (frost-resistant capes from the Mountains on), Drowned, Choking, and how weather affixes combine and how far the weather reaches.
17. Map names, the tier on the icon, the tier and affixes on the rip's hover text.
18. #84: how far from player-built pieces, for every rip or only the fallback, and whether crops, boats and carts count.

## Next steps

1. Krogon confirms the round and answers the questions; each answer goes into Decisions and is committed at once.
2. Build: pure rules and data tables (Sonnet), Harmony patches (Sonnet, then an Opus review), the Forge tab's map operations after Krogon approves a preview picture.
3. Tests and builds by the main session, every diff read, an Opus review focused on multiplayer and the dedicated server.
4. Solo checklist in `docs/playtests/claude-festive-gates-79y1yr.md` and on the phone page; two-player steps in `PLAYTEST.md` → Session 2.

## Checks

Not run yet (nothing built).

## Merge notes

**Ready to merge: no** (nothing built). Will be "gameplay changed: version bump needed at merge (Corruption)". Expected to share `Corruption/CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Content/Texts.cs`, `Content/LootPatches.cs` and the shared docs.
