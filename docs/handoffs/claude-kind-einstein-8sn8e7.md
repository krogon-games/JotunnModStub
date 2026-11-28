# Handoff: `claude/kind-einstein-8sn8e7`

**Written:** 2026-11-28 (`/handoff`). From `master` at `cce2ba7`; 20 commits ahead, 0 behind; no PR yet.

## Stage

**Fixing feedback** (one re-test, `ma-6b`) (this round: the creature and player affixes; decisions 1–18 below).

## Purpose

The map affix overhaul ([#90](https://github.com/krogon-games/JotunnModStub/issues/90), ROADMAP R1b): Krogon's new list of map affixes, some of the four existing ones removed.

## Done

Confirmed by Krogon alone on Valheim 1.0.31 (2026-11-28, phone page, 29 of 30 steps): all 17 affixes act in rips, rolled values and the Divine reroll, the groups rule, `DisabledMapAffixes`, the Rip curse, life steal on Ironskin, a corrupted tier 6 map, the rip hover (decision 15), Glorious under "What you'll earn" (decision 16), `co_map`/`co_rip`. No LogOutput.log sent.

## Half-done and known issues

- **Built, not play-tested** (756 tests; Corruption and Orlog build): the 17 affixes of Krogon's list (4 old ones removed), rolled values (`id:value`), Divine rerolling map values, the groups rule, the "Rip curse" within 96 m, `DisabledMapAffixes`, `co_map id:value`. Reviewed by Opus twice (game code; multiplayer and dedicated server); their fixes are in (`FEATURES.md` changelog).
- **Claude's choices** (changeable): the 17 affix names (Glorious, Teeming, Brutal, Frenzied, Ruthless, Sturdy, Mighty, Fleet, Unyielding, Ironskin, Warded, Withering, Exhausting, Exposed, Venomous, Searing, Stormcalled) and their one-line copy; all equally likely; "elemental" = fire, frost, lightning, poison (not spirit); Exposed lowers Corruption's gear resistances only; a switched-off affix still acts on maps that carry it; old maps lose their removed affixes and drop in tier.
- **Audit (2026-11-27, Opus, read-only):** every link of the maps mechanic checked against the decisions; its fixes are in (`FEATURES.md` changelog), its open points became decisions 14–17. 765 tests.
- **Known limits:** Fleet adds its full bonus while swimming; a creature already hurt when a rip starts keeps its health points while its maximum rises; Unyielding also stops parry staggers, guard breaks and adrenaline from staggers.
- The game code was decompiled for this round (decision 9); every patch target was checked against Valheim 1.0.31.

## Decisions

**Krogon's brief for this round** (2026-11-27, given before the confirmation; his words):
- "Use subagents."
- "New round: #90, the map affix overhaul (ROADMAP R1b). I already have a list of affixes in mind: ask me for it first, don't propose your own list." He sent the list with the brief (below).
- "Ask me for my list of new affixes and which existing ones (Swarming, Hardened, Rabid, Smouldering) to remove." "Go through my list one affix at a time." "Settle the open questions in #90 … Keep the round small enough to finish and play-test; offer to split it if my list is long."
- Building: "Every affix stays danger only, the tier pays (decision 8b), unless I change that." "Numbers in the data tables (map_affixes.csv and any new table), rules in pure unit-tested files, nothing guessed." "Each affix's tooltip text is its danger line in the decision 15 layout … keep it short and the numbers in it matching the data." Harmony patches by a Sonnet subagent, reviewed by Opus against the game code and `docs/mod-compatibility.md`, and a separate Opus review for multiplayer and the dedicated server. "A dev command or co_map support so I can test every affix alone." "If the UI changes, show me a preview picture first."
- No version changes; no edits to `STATUS.md` or `docs/DECISIONS.md`.
- "One note regarding affixes. The corrupted ones you can rewrite the copy so that it fits better on the tooltip and other UI elements."
- "Im now going to sleep. Do as much as you can without needing my decisions."
- "Im aware that some of the affixes i proposed dont have the mechanics implemented yet such as the named elites and champion packs etc, so just make the option to disable some affixes from being able to roll - until we catch up and create the mechanics in the future that allow these affixes to function as intended."

**Krogon's list** (2026-11-27, `new_map_affixes.txt`, verbatim):

```
Monsters Grant +50-100% Experience
+50% Base Monster Density
Monsters Deal +20%-100% Damage
Monsters have +25-100% Increased Attack Speed
Monsters have +10-50% Chance to deal Double Damage
Monsters have + 25-50% Extra Health
Monsters have +50-100% Extra Health
Monsters have +25% Increased Speed
Monsters cannot be Staggered
Monsters are immune to Physical Damage
Monsters are immune to Elemental Damage
Players have -5 Drain Life per Second
Players have -5 Drain Stamina per Second
Players have -25%-50% to All Resistances
Monsters have +25-75% of Physical Damage as extra Poison Damage over 5 seconds
Monsters have +25-75% of Physical Damage as extra Fire Damage over 5 seconds
Monsters have +25-75% of Physical Damage as extra Lightning Damage
Monsters have +100% Chance to Drop Additional Items
Monsters have +100% Chance to Drop Orbs
Monsters have +100% Chance to Drop Runes
Monsters have +10% Chance to Drop Shields
Monsters have +10% Chance to Drop Helmets and Armors
Monsters have +10% Chance to Drop Melee Weapons
Monsters have +10% Chance to Drop Ranged Weapons

CORRUPTED AFFIXES:
// These affixes do not need to be displayed with all this text, but these descriptions are required to make sense of what the affixes do to the map and how they change the gameplay or what mechanic they add as they are a little more complex in nature.

Slain wave enemies feed the heart. Its veins light up as it charges. At full charge, it releases a shockwave and summons a champion. (Will tie together when we have named elites and champion monsters.

Two burial stones rise on either side of the heart (5 meters each side) when the heart is first damaged. Break one to wake its guardian and sink the other. One guardian hunts you through the waves. The other holds ground and shields nearby enemies. Each stone tells you which fight it offers. While the guardian is alive, the heart is invulnerable.

Some slain enemies leave a glowing ember that drifts toward the heart. Destroy an ember before it arrives or the heart raises a copy of that enemy beside itself.

A corrupted boss is Guarding the Heart.

Elemental Meteors rain from the sky and leave an elemental fire on the ground. (As in Yagluth fight)

A Treasure Fallen appears and flees from attacks, dropping some of their treasures each time they take significant damage. They can drop coins, runes (when implemented), maps, orbs and shards. Up to 3 treasure fallen may appear.

A chance of a unique item is dramatically increased.

A portal opens to a Treasure Vault with 15-20 super chests.The area is filled with deadly, invulnerable drauger spawners that spawn constant elite or champion monsters.

Monsters trigger an explosion of Ice on death.

Monsters have thorns.
```

**Krogon's answers to the research questions** (2026-11-27, his words; the round and branch taken as confirmed by his answering and "Split rounds, agreed"):

1. **The four old affixes all go** (2026-11-27): "Remove each one. Use new affix pool I provided." Swarming, Hardened, Rabid and Smouldering are removed; the pool is his list.
2. **Decision 8b is overwritten: affixes can give rewards** (2026-11-27): "overwrite decision 8b, affixes can now provide rewards too". Replaces the rips round's 8b ("every map affix is danger only; the tier pays").
3. **Affixes roll a value from their range** (2026-11-27): "Yes that's why I provided the ranges, so that it can roll." Each map rolls its own number within the range on his list. Replaces decision 6b's "Divine does nothing (map affixes have no ranges)": Claude's reading is that the Divine Orb now rerolls a map's values, as on gear.
4. **More than 4 affixes only by corrupting** (2026-11-27): "A map can only hold more then 4 affixes via means of corrupting a 5th affix on." Claude's reading: the 5th is the corrupted-only affix of decision 6d, built in the corrupted-affix round; until then tier 6 keeps 4.
5. **Rules against unwinnable maps; immune = 95% less** (2026-11-27): "Sure, create those rules. And yes, immune means takes 95% reduced damage." No map rolls both immunities, nor both health affixes; "immune to physical/elemental damage" means 95% less damage of that kind.
6. **The round is split** (2026-11-27): "Split rounds, agreed". This round: the creature and player affixes of his list. Next round: the loot affixes. After that: the corrupted-only affixes. The weather affixes still wait for `co_dump` (#86).
7. **The drop affixes raise chances, never guarantee** (2026-11-27, for the loot round): "Additional items means increased drop rates for everything. I thought it was supposed to say 100% for shields, maybe I made a mistake. All the map affixes that say drop extra items just means the chances for those drops are increased by 100%, not that they are guaranteed drops from every kill." So every drop affix is a ×2 on its drop chances, the shields, armour, melee and ranged ones included (+100%, not +10%).
8. **"In the rip" is 96 m from the heart** (2026-11-27): "Sure use the 96m". The player drains (life, stamina, resistances) act within 96 m of the rip's heart, the waves' range.
9. **Decompiling the game is allowed** (2026-11-27): "Youre allowed to decompile game code for the purpose of modding."

10. **Affix names are not shown on the map** (2026-11-27): "The names of affixes do not need to be shown on the tooltip, it uses up already limited ui space. Either remove them or hide them, if you need the names for the affixes to function in the background correctly." The tooltip and the Forge show each affix's line only; the names stay in the table for the code, logs and console.
11. **Life steal steals what the hit really deals** (2026-11-27): "Lifesteal should steal the amount it damages for." On Ironskin and Warded maps life steal counts the damage after their 95% cut.
12. **All affixes equally likely** (2026-11-27): "All are equal for now."
13. **All life steal should count the damage after armour and resistances, later** (2026-11-27): "Yes I think so. It would balance it better. But if the task is large you can open a ticket and we can deal with it later." It needs the share carried with the hit to the target's owner and the heal sent back, so it's a ticket of its own: [#93](https://github.com/krogon-games/JotunnModStub/issues/93). This round keeps decision 11 (Ironskin and Warded only).

14. **Old maps stay as they are** (2026-11-27, after Claude's audit found old maps with removed affixes, corrupted tier 6 ones showing tier 6 but opening tier 1 and locked at the Forge): "Since everything is still in alpha essentially and we're not playing for real, it's fine. Leave them be as is and I'll delete whatever copies I have left." No migration; the play-test step for old maps goes.
15. **The rip's hover shows the tier and the affix lines** (2026-11-27): "Tier and the lines". The portal's and heart's hover text shows the tier and each affix's line with its number, like the tooltip, instead of the affix names.
16. **Reward affixes go under "What you'll earn"** (2026-11-27): "Move to earn (Recommended)". Glorious now (and the loot affixes next round) is listed under "What you'll earn"; danger affixes stay under "What you'll face". Changes decision 15's layout (where every affix was under "face").
17. **Venomous keeps the game's own poison length** (2026-11-27): "Game's own (Recommended)". The game sets poison's length from the hit's size; the line names no length.

18. **Divine rerolls shown in gold** (2026-11-27): "Yes rerolled gold colour is fine". The Forge shows a Divine Orb's rerolled lines with a gold "~" (the Forge's highlight colour).

## Playtest

Solo checklist `docs/playtests/claude-kind-einstein-8sn8e7.md`. 2026-11-28: 29 passed; **`ma-6` failed** (an Ironskin + Warded map was kept, "the server answered tier 2 for a tier 3 map … the map is kept", instead of opening as tier 2): the audit's "keep the map" check also caught a tier the server lowers on purpose. Fixed (the opener's game runs the same check on what it sent); **re-test `ma-6b`** on the phone page. Group steps: `PLAYTEST.md` → Session 2, 2m (`mpe-2m-4` updated to the new affixes).

## Open questions for Krogon

None.

## Next steps

1. Krogon re-tests `ma-6b` (final play-test).
2. `/handoff`, then a PR into `master` when Krogon asks.
3. Next rips rounds: the loot affixes (decision 7), then the corrupted-only affixes; the heart's distance and look ([#94](https://github.com/krogon-games/JotunnModStub/issues/94)); life steal after armour ([#93](https://github.com/krogon-games/JotunnModStub/issues/93)).

## Checks

Run 2026-11-28 by `/handoff`:
- **Build: PASS:** 766 tests; Corruption and Orlog build, 0 errors (stand-in game files).
- **Version:** gameplay changed: version bump needed at merge (Corruption, Orlog). None changed on this branch.
- **Multiplayer sync: PASS** (read in code, not tested; two Opus reviews of 2026-11-27): `DisabledMapAffixes` is admin-only (server-synced); affix values travel in the item, the rip request (the server checks them) and the creatures' ZDO stamp; Glorious reaches Orlog through the creature's ZDO; the Rip curse runs on each player's own game.
- **Assets and names: PASS:** no new data files; the five new `$corruption_se_ripcurse*` texts are in `Texts.cs`; the Rip curse effect is registered in `OnVanillaPrefabsAvailable`.
- **FEATURES.md: PASS.**
- **In-game: PASS for 29 of 30** by Krogon's play-test of 2026-11-28 (phone page); **`ma-6b` (the `ma-6` fix) needs Krogon's re-test.**

## Merge notes

**Ready to merge: no**, waiting for the re-test `ma-6b`. 0 behind `master`; no other remote branch has unmerged commits.
- **Gameplay changed: version bump needed at merge (Corruption, Orlog):** the map's stored affixes and the rip request changed format (`id:value`); Orlog reads `corruption_rip_xp`.
- **Shared files:** `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (`mpe-2m-4` updated to the new affixes), `Corruption/CorruptionPlugin.cs`, `CorruptionConfig.cs`, `Content/Texts.cs`, `LootPatches.cs`.
- **`docs/DECISIONS.md`:** decisions 1–18; 2 overwrites the rips round's 8b, 3 changes 6b (Divine rerolls map values), 10 and 16 change decision 15's layout (no names; rewards under "earn").
- New issues opened this session: [#93](https://github.com/krogon-games/JotunnModStub/issues/93) (life steal after armour), [#94](https://github.com/krogon-games/JotunnModStub/issues/94) (the heart's distance and look).
