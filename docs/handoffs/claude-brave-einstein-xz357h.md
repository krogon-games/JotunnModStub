# Handoff: `claude/brave-einstein-xz357h`

**Written:** 2026-11-10 (round start; `/handoff` rewrites this at the end).

## Stage

Asking Krogon questions.

## Purpose

Loot round for [#29](https://github.com/krogon-games/JotunnModStub/issues/29): corrupting gear with a shard, shattering and Affix Splinters in the Riftforge's Forge tab.

## Done

Nothing confirmed in game yet.

## Half-done and known issues

Nothing built yet.

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

## Playtest

Nothing to test yet. The checklist will be `docs/playtests/claude-brave-einstein-xz357h.md`.

## Open questions for Krogon

1. Is the tickbox's choice remembered between sessions (Claude recommends yes, with a setting to switch the warnings back on)?
2. Claude's reading of three points (corrupted affix completing a twin rerolls in the resonant range; caps count across all worn items with corrupted affixes on top; the freed Bag slot counts only when the item is in the Bag grid): confirm or correct.
3. Which splinter designs, and what decides a splinter's shape and colour.

## Next steps

1. Splinter design drafts for Krogon to pick from.
2. Build the rules (pure, tested), then the Forge tab's operations.
3. Docs and the play-test checklist.

## Checks

Not run yet.

## Merge notes

- Ready to merge: no (nothing built).
