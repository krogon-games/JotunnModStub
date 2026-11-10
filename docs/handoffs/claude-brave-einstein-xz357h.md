# Handoff: `claude/brave-einstein-xz357h`

**Written:** 2026-11-10, mid-round (`/handoff` rewrites this at the end). Everything pushed; no PR.

## Stage

Waiting for Krogon's playtest (and his pick of a splinter design).

## Purpose

Loot round for [#29](https://github.com/krogon-games/JotunnModStub/issues/29): corrupting gear with a shard, shattering and Affix Splinters in the Riftforge's Forge tab.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Built 2026-11-10, **not tested in game**:
- **Shard corruption, shattering, Affix Splinters** in the Forge tab, with Krogon's rules below; `co_craft corrupt / shatter / splinter`.
- **Corrupted affixes beyond the caps**; tooltips show each cap and what worn gear uses.
- **"Don't warn me again"** on the shard's box (setting `10 - Riftforge`); **the Shatter dialog** (decisions 22–24): built, seen only as preview pictures (`docs/images/shatter_dialog_preview.png`), waiting for Krogon's look in game.
- **Splinter models** (approved designs): 9 shapes, a colour per family for all 67 (`splinter_looks.csv`, `tools/make_splinters.py`, preview `docs/images/splinters_preview.png`). Colours are Claude's picks within the approved idea.
- **Shard twin completion** adds both values plus 25% (decision 20); **cap lines removed** from tooltips; the stats sheet is [#60](https://github.com/krogon-games/JotunnModStub/issues/60).

Claude's interim choices (easy to change): a Magic item with 0 affixes can't be shattered; splinter weight 0.2; resistance never above 100%; Skald's and Fenrir buffs stay under the cap even from a corrupted line; caps aren't shown anywhere until #60's stats sheet (decision 21).

## Decisions

From Krogon's round brief (2026-11-10). Recorded as given; not to be asked again.

1. **A splinter that completes a twin is deterministic:** the resonant line's value is the item's existing value plus the splinter's stored value, exactly. No reroll, no extra multiplier. The line takes both the prefix and the suffix place. An already-resonant splinter transfers its exact combined value. Randomly rolled affixes and ordinary orb crafting keep today's resonance (the wider range). **Replaces** "a splinter that completes a twin resonates as usual" (2026-10-28).
2. **Storage is checked before shattering and before any recovery roll.** Recovered splinters go to Splinters, overflowing into the Bag (the existing routing). Needed room = one splinter per displayed affix line (a resonant line counts as one), across Splinters and Bag, counting the Bag slot the destroyed item frees. Not enough: refuse with "Make room for up to X splinters." Checked before rolling and again on confirming; storage can never be used to reject a random outcome.
3. **Shattering is one operation:** the source is destroyed and every recovered splinter delivered, or nothing happens. A failure keeps the source and gives no partial output. Splinters are never silently lost or dropped on the ground.
4. **Every shard use asks first**, in the existing confirmation box: the target's name, the cost (one shard), and the chances read from the rules (today 25% destroyed · 25% unchanged and locked · 50% one corrupted affix, locked), saying that every surviving outcome locks the item, apart from the Mirror exception. Separate from the immediate orb loop. Cancelling changes and uses nothing. A completed attempt uses exactly one shard, also when the item is destroyed.
5. **A tickbox on the confirmation** lets the player switch the warning off ("Ignore future warnings" or similar).
6. **The corrupted affix really works:** ordinary affixes stay under their caps; the corrupted affix adds beyond the cap; corrupting never uncaps the item's ordinary affixes; the rules for duplicate families, resonance and mirrored items stay.
7. **Every corruption outcome is reported in words**, e.g. "Corrupted and locked; no affix added." The unchanged-but-locked outcome must never read as "no change". Target, tooltip, eligibility, material count and result refresh at once; the target clears when destroyed.
8. **Other settled rules stay:** shattering is free, 50% per displayed line read from data, exact values kept (resonant lines too); splinters don't stack and use the bag-sized Splinters tab; eligibility, item class, family, prefix/suffix and the two-affix limit apply.
9. **Tests** cover exact transfer and twin completion, resonant splinters, capacity (overflow, freed slot), refusals and failures keeping items and materials, every corruption outcome with the caps, and splinter data surviving saving, trading, chests and recovery where testable. Automated results are reported separately from Krogon's in-game checks.

Krogon's answers (2026-11-10):

10. **The "ignore future warnings" tickbox is separate for shattering and for shards:** each confirmation has its own, and ticking one doesn't silence the other.
11. **A Mirror's copy is locked like a corrupted item:** it can't be shattered, corrupted or take a splinter.
12. **Affix Splinters get their own models**, not a tinted vanilla item: a variety of shapes and colours, inspired by Path of Exile's catalysts. **Drafts or designs first**; nothing final is built until Krogon picks.

Krogon's second answers (2026-11-10):

13. **The tickboxes' choices are remembered** between play sessions (with a setting to switch the warnings back on).
14. **Caps count across everything worn:** ordinary affixes on all worn items are added up and capped; corrupted affixes add on top.
15. **Players must be able to see each cap and how close they are to it.**
16. **The Bag slot freed by shattering counts as room only when the shattered item is in the Bag grid.**
17. **A splinter that completes a twin gets the resonance bonus: (item's value + splinter's value) + 25%.** Example: Hale +14 and a splinter of the Bear +18 give one resonant line of +40 (32 + 25%). Still no random roll. "This rewards the player for saving shatters." **Replaces** decision 1's "no extra multiplier". An already-resonant splinter still transfers its exact value.
18. **Splinter designs:** "shape by kind, colour by stat is a fantastic idea"; the shapes should be rough, organic lumps rather than clean cut crystals.

Krogon's third answers (2026-11-10):

19. **Splinter designs approved** (draft 2: rough organic lumps, one shape per kind of affix, one colour per stat). Build them.
20. **A shard's corrupted affix that completes a twin works like a splinter:** the two values are added, then the 25% bonus on top. No reroll. **Replaces** the rolled resonant range for this case (Claude's interim choice, and "resonates like a normal twin", 2026-10-28).
21. **No cap lines in item tooltips** ("the tooltip is already overly crowded"). Caps and progress belong on a **detailed character sheet** with the character's stats (added damage, resistances, movement speed, attack speed, …), to be made later through its own GitHub issue. **Replaces** where decision 15 is shown; players still need to see caps, on that sheet.

Krogon's brief for the Shatter confirmation (2026-11-10, during the play-test):

22. **The Shatter confirmation gets its own structured dialog**, matching the Forge and the game's UI, rules unchanged. In order: title "Shatter item" (centred, the game's heading style); the target's icon and rarity-coloured name; "Cost: Free" as a small secondary detail; the warning "This permanently destroys the item." once, in the warning colour; "Potential splinters" as a two-column list (a wide affix column in the Forge's affix formatting with hanging indents, a narrow right-aligned chance column, chances from the rules); a short secondary explanation ("Each affix rolls independently; you may recover none." / "Recovered splinters keep their exact values."); buttons **Cancel** (left) and **Shatter** (right). Content left-aligned. Height from the content; a reserved footer for the buttons; text never overlaps controls; native fonts, wood background, borders and buttons; not everything bold.
23. **Every shatter asks: the Shatter dialog has no "Don't warn me again".** **Replaces** decisions 10 and 13 for shattering (the shard's tickbox is unchanged).
24. **The dialog is shown to Krogon before the polish counts as done**, with short and long affix descriptions, resonant lines and the most lines, checked for clipping, chance-column alignment and button separation.

Krogon, after seeing the dialogs in game (2026-11-10, four screenshots):

25. **Both confirmations keep a "Don't warn me again" tickbox** ("this action will be done many many times by players. they need to have an option to hide the panel once they get used to the mechanics"). **Replaces** decision 23; decisions 10 and 13 stand again (separate, remembered).
26. **The shard's confirmation uses the same structured design as the Shatter dialog** ("needs to be consistent in design as well").

## Playtest

`docs/playtests/claude-brave-einstein-xz357h.md`: checklist A–H written 2026-11-10 (shattering, full storage, splinters, shards, tickboxes, caps, reconnect). No feedback yet.

## Open questions for Krogon

None open.

## Next steps

1. Krogon's play-test (checklist A–H); fix what it finds.
2. `/handoff`, then a PR into `master`.
3. Later, its own round: the stats sheet ([#60](https://github.com/krogon-games/JotunnModStub/issues/60)).

## Checks

- Build: PASS (`dotnet test CraftingOverhaul.Tests` 498 passed; Corruption builds against the stand-in game folder).
- In-game: needs Krogon's playtest.
- The rest: run by `/handoff`.

## Merge notes

- Ready to merge: no (not play-tested).
- **Gameplay changed: version bump needed at merge (Corruption).** This branch changed no version number.
- Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/mod-compatibility.md`, `docs/AFFIX_RULES.md`, `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/riftforge-ui-brief.md`, `Corruption/CorruptionPlugin.cs`, `Corruption/CorruptionConfig.cs`.
