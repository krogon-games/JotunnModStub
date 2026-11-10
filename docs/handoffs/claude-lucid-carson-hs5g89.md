# Handoff: `claude/lucid-carson-hs5g89`

**Written:** 2026-11-10 by `/handoff`. 12 commits ahead of `master`, 0 behind; everything pushed; no PR yet.

## Stage

Ready to merge.

## Purpose

UI round for [#30](https://github.com/krogon-games/JotunnModStub/issues/30): six inventory tabs (Bag · Currencies · Splinters · Maps · Runes · Essences) and the Riftforge's Forge tab (one gear slot, materials used straight from the inventory). Shard corruption, shattering and splinters are [#29](https://github.com/krogon-games/JotunnModStub/issues/29)'s rules, not built yet.

## Done

Confirmed by Krogon in single player (2026-11-10, checklist A–E, then six looks at the design):
- **Six tabs:** order, nothing lost, essences moved to Essences, fixed currency places with dimmed 0s, Splinters and Runes refusing other items, death and gravestone, relog.
- **Forge tab** with orbs: drag gear in, right-click an orb, click to use, Shift, Escape, refusals, Remove, closing, away from the Riftforge. Built from the game's own panel parts; affixes bulleted under the slot; short result with the changed affix highlighted.
- **Item tooltips everywhere** use the same affix layout.
- **Skills button removed** from above the inventory (CraftingOverhaul; K still opens the panel).

## Half-done and known issues

- **Multiplayer:** not tried (Krogon's choice): `PLAYTEST.md` → Session 2, 2h.
- **Waiting for #29:** Shatter (greyed, says so), shard corruption, the Affix Splinter item (must be named `CorruptionAffixSplinter…`; it then goes to Splinters). Shatter and corrupt will ask in the game's Yes/No box with the rules' preview. Checklist F.
- Not checked against EquipmentAndQuickSlots/AdventureBackpacks' DLLs; Krogon played with both loaded.
- Smaller limits: `FEATURES.md` → Inventory tabs, Riftforge workspace.

## Decisions

All from Krogon's round brief of 2026-11-09 (replacing #30's two-slot, operation-list, preview-and-confirm design). Recorded as given; not to be asked again.

1. **Inventory tabs, in this order:** Bag · Currencies · Splinters · Maps · Runes · Essences.
2. **Currencies** (keep that name): orbs and Corruption Shards, each at a fixed position with a visible count. Exhausted entries keep their place, dimmed. Don't silently reduce currency capacity; one fixed entry may stand for several stacks if spending stays correct.
3. **Splinters:** a plain Bag-style grid of single Affix Splinters. No scrolling, search, filters, collection browser or fixed places per affix. Capacity = the default bag (without equipment or backpack slots). Splinters never stack, even identical rolls. Limited on purpose; collections go in chests (chest transfers keep all affix data; no new chest type).
4. **Maps:** existing storage and behaviour.
5. **Runes:** an empty placeholder saying "Runes are not available yet". Nothing may enter it by any route (placing, dragging, pickup, sweeps, transfers, overflow, migration). No item goes there because of its name. Unexpected saved data in it is kept, never deleted.
6. **Essences:** the boss essences move out of Currencies into their own tab, fixed positions, stacking.
7. **Crafting is Riftforge-only** (the earlier "craft anywhere" idea is withdrawn).
8. **Riftforge workspace:** a third tab on the Riftforge panel with one target-item slot; the station's other functions stay. Its label and styling are Claude's choice. Flow: open the tab, put in gear from the Bag, switch the inventory to Currencies or Splinters, craft repeatedly while looking at the target, take it out when done. The target and its details stay visible while switching inventory tabs. No gear slot inside Currencies or Splinters; no currency drawer.
9. **The target is a reference:** the item stays in the inventory; closing the window never needs recovering it. Cleared when the item is destroyed or gone.
10. **Interaction:** right-click an orb, shard or splinter to pick it (shown at the cursor, its source highlighted); left-click the target to apply it once; hold Shift to keep the pick for more clicks; every use needs its own click; picking another material replaces the pick; Escape or a safe click cancels. No operation list or confirm button for ordinary orbs. Tab changes and other clicks never spend materials; crafting clicks never also equip, move or use items. Outside the forge, the inventory behaves normally and nothing can be applied.
11. **Feedback:** the target's normal tooltip beside it, refreshed straight after each use; counts, eligibility, details and a short result message update after each use; surviving gear stays in; refusals explain and spend nothing; "unavailable quantity" looks different from "can't be used on this item". Jeweller's Orb stays disabled with its explanation.
12. **Shattering** in the same workspace: put in eligible gear, choose Shatter, see the destruction warning and recovery chances, confirm, the item is destroyed and recovered splinters delivered, the slot clears, the result is reported. No forced tab change after crafting or shattering.
13. **Corruption Shards** are in Currencies; their exact confirmation is open. Claude recommends a clear risk preview and a deliberate confirmation, separate from the fast orb loop.
14. **No rule changes this round:** affixes, probabilities and progression as they are (orbs never destroy; corruption 25/25/50; Mirror exception; shattering free on Magic/Rare, not corrupted, 50% per line, exact values, a resonant line one roll and one combined splinter; splinters on Normal/Magic within two affixes, respecting class, family, prefix/suffix and resonance). Read chances from the data; never suggest a random result can be predicted. Use `RiftforgeCrafting.Options`/`Apply`/`IsMaterial` and registered operations; rules stay out of the UI.
15. **Don't fake missing operations:** shard corruption, shattering and splinters are #29's; the window must not simulate them, and the loop isn't reported finished while they're missing. Keep map-operation registration (#31) without inventing its costs.
16. **Migration:** orbs and shards stay in Currencies; essences move to Essences; splinters to Splinters; maps stay; nothing to Runes. Every item, count, custom data, overflow and recovery record kept; safe to repeat and across reconnects; unreadable saved tabs stay protected.

After Krogon's first look (2026-11-09, screenshot; a few orbs tried: "they do work"):

17. **The Forge window must match the station's other tabs** (it "looks very out of place"): bring it in line with the game's crafting panel and give it a polished treatment. The design is settled before the full play-test.
18. **"Take out" is renamed "Remove".**
19. **The Skills button on the inventory is removed** ("it does not belong there at all"; it pushed the Essences tab out of view). Where the skills panel's tab or key goes is decided later; the skills panel still opens with its key.

Krogon's second look (2026-11-10, two screenshots, one annotated): "much better, a lot more consistent"; it feels good, Craft and Upgrade come back as before, all six tabs show and the essences moved to their own tab. Changes asked:

20. **Affixes readable and in a fixed place:** the affix block (rarity line, every affix with its range) moves to the **left panel** under the forge slot, in larger text with more spacing; the routine weapon stats stay on the right as a secondary details area, **at the same text size as the other tabs**.
21. **Result message:** a full-width area, shorter wording, and a brief highlight of the changed affix so a roll is understood without reading a paragraph; it must not hold up the next click.
22. **The five empty boxes** under the details (the copied cost slots) are unclear: remove them and use the space (Claude: for the result message).
23. **Remove the small icon** at the top of the right panel (it repeats the forge slot) and **delete the "Right-click an orb to pick it up" line**.

Krogon's third look and full play-test (2026-11-10, two screenshots, one annotated): "full play-test (A–E) has been done, everything seems to work as intended." Asked:

24. **The last empty box** at the bottom of the right panel must go.
25. **Give the forge slot a nice border.**
26. **Add some spacing** between the rarity line ("Rare Wood Shield") and the affix lines under it.

Krogon's fourth look (2026-11-10, annotated screenshot): "Everything looking good, almost there." One tweak:

27. **The affixes get lost in a wall of text:** each affix gets a bullet with its wrapped lines indented under it, and a resonant affix a green diamond instead of the round bullet (Krogon's mock-up).

Krogon's fifth look (2026-11-10, screenshot):

28. **Bullet and diamond closer in size; the whole resonant affix green**, not only its diamond; **"Corrupted" centred.**

Krogon's sixth look (2026-11-10, two screenshots): the Forge bench is "excellent".

29. **The hover tooltips everywhere** (bag, tabs, chests) use the same affix layout as the bench: rarity line, a little space, bulleted affixes with a hanging indent, resonant affixes green with a diamond, Corrupted/Mirrored centred.

Krogon, closing the round (2026-11-10): "it looks good. well done. solid round. … testing is done, except multiplayer."

30. **Multiplayer testing is left for later** (the group evening, `PLAYTEST.md` → Session 2, 2h).


## Playtest

`docs/playtests/claude-lucid-carson-hs5g89.md`: A–E passed 2026-11-10, after six rounds of look feedback (all recorded there). Multiplayer in `PLAYTEST.md` → Session 2, 2h; F waits for #29.

## Open questions for Krogon

1. Where the skills panel's button or tab goes (decision 19: later).
2. The Corruption shard's exact confirmation (decision 13): Claude recommends the risk preview plus Yes/No; settle when #29 builds it.

## Next steps

1. PR into `master`; Krogon merges; the coordinator raises Corruption and CraftingOverhaul.
2. #29 (loot session): shard corruption, shattering, splinters against `docs/riftforge-ui-brief.md` §4; then checklist F.
3. The group evening: Session 2, 2h.

## Checks

- Build: PASS. `dotnet test CraftingOverhaul.Tests` 464 passed; Corruption and CraftingOverhaul build against the stand-in game folder (warnings only).
- Version: gameplay changed: version bump needed at merge (Corruption, CraftingOverhaul). This branch changed no version number.
- Multiplayer sync: read in code, not tested. No new config, items or pieces; tabs are per player (character data); the Forge tab is local only.
- Assets and names: PASS. Every new `$` token has an entry in `Texts.cs`; no new data files; mock-ups in `docs/images/`.
- FEATURES.md: PASS (rows, sections, patch targets, changelog).
- In-game: passed in Krogon's single-player play-test (2026-11-10): "it looks good. well done. solid round. … testing is done, except multiplayer."

## Merge notes

- **Ready to merge: yes.** Krogon's final play-test passed (multiplayer deliberately left for the group evening), no check failed, the latest `master` is in, everything pushed.
- **Gameplay changed: version bump needed at merge (Corruption, CraftingOverhaul).**
- No other branch is open. Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/mod-compatibility.md`, `Corruption/Content/Texts.cs` and three design docs.
