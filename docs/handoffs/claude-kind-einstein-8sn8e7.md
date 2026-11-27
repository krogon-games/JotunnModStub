# Handoff: `claude/kind-einstein-8sn8e7`

**Written:** 2026-11-27 (start of the round). From `master` at `cce2ba7`; no PR.

## Stage

**Building** (this round: the creature and player affixes; decisions 1–9 below).

## Purpose

The map affix overhaul ([#90](https://github.com/krogon-games/JotunnModStub/issues/90), ROADMAP R1b): Krogon's new list of map affixes, some of the four existing ones removed.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

- The round and the branch are not yet confirmed by Krogon (the session routine's step 3): he gave the brief and went to sleep. The branch is the one this cloud session was given.
- **Built, not play-tested:** the admin setting `[5 - Rips] DisabledMapAffixes` (his "option to disable some affixes from being able to roll"): listed ids never roll; maps already carrying one keep it and it still acts (Claude's choice). 743 tests pass; Corruption builds.
- **Not checked against the game code:** decompiling `assembly_valheim.dll` was refused by this session's permission check, so the game-method names for attack speed, stagger, creature max health and the stamina regen number are unverified; they need that permission or a `co_dump` extension.

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

## Playtest

Nothing to test yet. Checklist: `docs/playtests/claude-kind-einstein-8sn8e7.md` once built.

## Open questions for Krogon

Research of 2026-11-27 (two Sonnet subagents over the code and the 1.0.31 dump; summary in the chat message of that day):
1. Confirm the round (#90) and the branch `claude/kind-einstein-8sn8e7`.
2. Which of Swarming, Hardened, Rabid, Smouldering go. Overlaps: Swarming ≈ "+50% density", Hardened ≈ "extra health", Rabid ≈ "+damage" and "+25% speed", Smouldering ≈ "+25-75% fire".
3. Decision 8b: experience, every "chance to drop" affix, Treasure Fallen, the unique chance and the Treasure Vault are rewards. Keep 8b (drop them, or keep them `later`) or allow reward affixes?
4. Ranges: map affixes have no values today (decision 6b: Divine does nothing on maps). Roll a value per map (Divine then rerolls it), or one fixed number per affix?
5. More than 4 affixes per map? Some affixes rarer than others (the `weight` column)?
6. Rules against unwinnable maps: never both immunities on one map; the two health affixes never together. Full immunity to physical leaves a player without elemental damage unable to hurt anything: full immunity or "takes N% less"?
7. Scope: Claude proposes this round = the 16 creature and player affixes (all danger, buildable now); loot affixes next round; corrupted-only ones after (meteors need the dump extended; heart charge, unique chance and the vault wait for champions and uniques; runes don't exist). Weather still waits for `co_dump`.
8. Per-affix questions: what "additional items" means (vanilla drops?); "+10% shields" etc. has no base chance to add to (an extra 10% roll per kill for that kind?); poison's length is set by the game from the hit's size, not a fixed 5 s; "-5 stamina per second" against the game's regen; where "in the rip" ends for the player drains (the waves' 96 m?).

## Next steps

1. Krogon confirms the round and branch, answers the questions above.
2. Build the agreed affixes; play-test checklist; reviews.

## Checks

Not run yet.

## Merge notes

Not ready to merge.
