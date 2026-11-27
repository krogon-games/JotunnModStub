# Handoff: `claude/kind-einstein-8sn8e7`

**Written:** 2026-11-27 (start of the round). From `master` at `cce2ba7`; no PR.

## Stage

**Asking Krogon questions** (he is asleep; questions below wait for him).

## Purpose

The map affix overhaul ([#90](https://github.com/krogon-games/JotunnModStub/issues/90), ROADMAP R1b): Krogon's new list of map affixes, some of the four existing ones removed.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

- The round and the branch are not yet confirmed by Krogon (the session routine's step 3): he gave the brief and went to sleep. The branch is the one this cloud session was given.

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

## Playtest

Nothing to test yet. Checklist: `docs/playtests/claude-kind-einstein-8sn8e7.md` once built.

## Open questions for Krogon

See the chat message of 2026-11-27 and the list below (filled in as the research finishes).

## Next steps

1. Krogon confirms the round and branch, answers the questions.
2. Build the agreed affixes; play-test checklist; reviews.

## Checks

Not run yet.

## Merge notes

Not ready to merge.
