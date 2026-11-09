# Handoff: `claude/lucid-carson-hs5g89`

**Written:** 2026-11-09. Branch made from `master` (`47dfd9b`, nothing newer on `master`); no PR yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

UI round for [#30](https://github.com/krogon-games/JotunnModStub/issues/30): six inventory tabs (Bag · Currencies · Splinters · Maps · Runes · Essences) and the Riftforge's crafting workspace (one gear slot, materials applied straight from the inventory). The rules behind shard corruption, shattering and splinters are [#29](https://github.com/krogon-games/JotunnModStub/issues/29)'s and aren't built yet.

## Done

Nothing play-tested yet.

## Half-done and known issues

Built 2026-11-09, untested in game (compiled against the game's decompiled 1.0 code; 446 tests pass):
- **Six tabs** (`FEATURES.md` → Inventory tabs): Bag · Currencies · Splinters · Maps · Runes · Essences. Currencies and Essences have fixed cells (`FixedLayout`, tested), dimmed with 0 when empty, no capacity lost (extra stacks use free cells, then the empty homes). Essences move out of Currencies on load and every half second (only between usable tabs, when they fit). Runes has no inventory; its save key is never read or written. New save keys `corruption_tab_splinters`, `corruption_tab_essences`; the old two unchanged. Test command `co_tabs_misplace`.
- **Forge tab** (`FEATURES.md` → Riftforge workspace): one target slot (a reference to bag gear), right-click to pick an orb, left-click the slot to use it, Shift keeps the pick, Escape drops it; red tint = can't be used here; the target's tooltip beside it. Mock-ups `docs/images/riftforge_workspace*.png`.
- **Not built, #29's:** shard corruption, shattering, the Affix Splinter item, applying splinters. The window says so and fakes nothing; when `RiftforgeCrafting` offers `shatter`/`corrupt`, the window asks for Confirm with the rules' preview. The splinter must be named `CorruptionAffixSplinter…`.
- **Claude's choices** (easy to change): tab label "Forge"; gear goes in by dragging it from the Bag onto the slot; the slot empties when the inventory closes; positions and sizes of the workspace are first guesses at the panel size.
- Not checked against EAQS/AB's DLLs (not in the cloud session); `docs/mod-compatibility.md` lists what to test.

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

## Playtest

`docs/playtests/claude-lucid-carson-hs5g89.md`: A (six tabs, nothing lost), B (fixed currency places), C (Splinters, Runes), D (the Forge tab), E (death, gravestone, relog); F waits for #29. No feedback yet.

## Open questions for Krogon

1. Corruption Shard confirmation (decision 13): Claude's recommendation is to be written into the brief; Krogon to confirm when #29 builds corruption.

## Next steps

1. Krogon's play-test (checklist A–E), then fix what it finds.
2. Final play-test, then `/handoff` and a PR into `master`.
3. #29 (loot session): shard corruption, shattering, splinters against brief §4; then checklist F.

## Checks

Run by `/handoff` at the end of the round. So far: `dotnet test CraftingOverhaul.Tests` 446 passed; Corruption builds against the stand-in game folder.

## Merge notes

Not ready: waiting for Krogon's play-test. Gameplay changed: version bump needed at merge (Corruption). Shared files touched: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/GAME_DESIGN.md`, `docs/corruption-design.md`, `docs/mod-compatibility.md`, `docs/riftforge-ui-brief.md`, `Corruption/Content/Texts.cs`.
