# Handoff: `claude/ui-overhaul`

**Written:** 2026-11-02, from the real branch state: `git status` clean before this file; the branch was 3 commits ahead of `master` (`dd93cb8`) and 9 behind; `master` (`e0a98b0`, loot steps 3–4 and the Riftforge window brief, PR #18) is now merged in with no lost work (conflicts in `FEATURES.md` and `docs/CURRENT_STATE.md` resolved by keeping both sides). After the merge all three plugins build against stand-in game assemblies and 248 tests pass. **PR #19** was opened from this branch into `master` (the click fix, build stamp, housekeeping docs, Riftforge mock-ups and this file); everything earlier from it is already in `master` (PRs #3, #7, #12, #15).

**Coordinator update (2026-11-08):** PR #19 was **merged** into `master` on 2026-11-02 (merge `68a63e0`), so everything on this branch is in `master`. The branch has since been deleted on GitHub; the next UI round starts a new branch from `master`.

Detail lives elsewhere; this file points to it: `FEATURES.md` (Character sheet, Currency and Maps tabs, Health/stamina/eitr bars, Classes UI), `docs/DECISIONS.md` (Features section, 2026-10-29 to 2026-11-01 rows), `docs/PLAYTEST.md`, `docs/ROADMAP.md` §4.4, `docs/classes-ui-brief.md`, `docs/riftforge-ui-brief.md`.

## Purpose

The UI session's branch: every screen the mods draw. CraftingOverhaul's screens (character sheet, HUD bars, classes UI), the Currency and Maps tabs in Corruption, and next the Riftforge window (ROADMAP U3).

## Stage

**Asking you questions** (where the Riftforge window opens, below) and **waiting for your playtest** of three small changes since your last test.

## Done (built and tested in-game by you, single player)

All of this is in `master` already:
- **Character sheet** (WoW-style, EquipmentAndQuickSlots' slots, live character picture, stats). Restyled 2026-10-30 to the game's own panel with bronze tiles; restyle passed with the classes UI.
- **Currency and Maps tabs** (Corruption): pickup straight to the tab, weightless, pays for recipes, gravestone on death.
- **HUD bars:**
  - **Layout:** health, stamina and eitr at the bottom centre (in that order), food in round slots, and the XP bar with level and points hint.
  - **Building and sailing:** the bars stay put; the game's build box and ship display move to the bottom right.
  - **Vanilla displays:** the game's own displays are hidden, and come back with `Bars = false`.
- **Skill cross:**
  - **Slots:** four slots with the Forsaken power in the middle.
  - **States:** cooldowns can't be mistaken for ready, and blocked slots say "Need bow" or "Low eitr".
  - **Quick slots:** EquipmentAndQuickSlots' quick slots sit under the cross.
- **Skills panel, redesigned:**
  - **Layout:** Diablo 2 style, with a character box and a details pane on the left, and on the right the tree on a tablet with icons and points badges.
  - **Use:** a Learn button, dragging to the slots or the HUD cross, and placeholder icons for every Amazon skill.
- **Class choice window**, opened by the Orlog stone (`co_classes_stone` in the demo); the classes session has since wired the real stone.
- **Both off switches** (`Demo`, `Bars`).

## Half-done

- **Riftforge window (U3):** mock-ups made (`docs/images/riftforge_window_orb.png`, `riftforge_window_shard.png`, `riftforge_tab.png`, from `tools/mockup_riftforge.py`), waiting for you to pick where it opens. The loot session's brief arrived with the merge (`docs/riftforge-ui-brief.md`). Before building, the mock-ups need lining up with it in three ways:
  - **Operation list:** the brief has a list of operations, each greyed with its refusal text; the mock-ups have a material grid.
  - **Tooltips:** the brief uses the game's normal item tooltips instead of drawing its own affix lines.
  - **Slots:** both slots point at items still in your inventory, so nothing can be lost.
  The rules behind it (`RiftforgeCrafting`, ROADMAP L5) aren't built yet; the window can be built against the brief's API meanwhile.
- **Real Orlog data instead of the demo (U2):** the classes session is filling `Orlog.Api` step by step (the stone and attributes are in `master`). The demo stays until Orlog serves skills, slots and slot states. `co_classes_stone` and `DemoClasses.cs` are then removed (the "before release" list in `CURRENT_STATE.md`).

## Broken or unknown

- **Not play-tested:**
  - The **adrenaline bar**: our orange bar above health replacing the game's.
  - The three changes since your last test (all untested): one click sound per button instead of two (ours and Corruption's tab buttons); CraftingOverhaul's new build stamp in the log; the multiplayer-evening docs.
- **Multiplayer:** nothing on this branch has been tried with another player. The evening script (`PLAYTEST.md` → Session 2) covers the per-player screens.
- **Classes UI with real Orlog data:** only checked with the demo. The layout checks in `mod-compatibility.md` that need real data are still open.
- **Known limits:**
  - **Placeholder skill icons:** the weakest are Fend, Strafe, Pierce and Poison Javelin.
  - **Panel background fallback:** the game's panel art is copied at runtime. If it can't be found, our look-alike shows (never seen so far).

## Decisions (yours, this session)

All are rows in `docs/DECISIONS.md`.

**Character sheet** (2026-10-29):
- **Build:** keep EquipmentAndQuickSlots and move its cells. No hand or feet slots, no tabs. A live character picture, brought forward. Generated art closer to your concept image.
- **Look:** revisit it later for consistency with the game's UI; done 2026-10-30.

**Currency and Maps tabs** (2026-10-29):
- **Pickup:** items go straight to their tab.
- **Death:** they go in the gravestone.
- **Weight and size:** weightless, 8 × 4 each.
- **Essences:** go in Currency.

**Bars** (2026-10-30, 2026-10-31):
- **Placement:** together at the bottom centre, Elden Ring style.
- **Order:** health, **stamina, eitr** (you swapped stamina and eitr so gaining eitr doesn't move stamina).
- **XP bar:** a little lower.
- **Building and sailing:** the bars never move. The game's "Repair" box goes to the bottom right ("not good design" to move the bars). The ship display followed the same rule; that was Claude's choice, and it still stands for you to confirm.

**Look** (2026-10-30): a mix of the four mock-ups:
- **From D:** the bronze medallions, eitr-blue rune paths, bronze riveted slots, the bars and the cross.
- **Panels:** the game's own panel in place of dark wood and parchment.
- **Character sheet:** restyled to match.
- **Skills panel:** wider.

**Skill cross** (2026-10-31):
- **Spacing:** spread out, with the boss power in the middle.
- **Removed:** the boss name and "Ready" text, and the "Skill button" text.
- **Cooldowns:** must be unmistakable.
- **Quick slots:** sit under the cross.
- **Blocked labels:** clearer wording ("Need bow", "Low eitr").

**Skills panel:**
- **Redesign (2026-10-31):** after "not acceptable" ("find a different approach"). You approved the result: "looking soooo much better", then "everything looks really good now".

**Process (2026-11-01):**
- **This stretch:** proceed with the roadmap's U1 fixes, the housekeeping and Riftforge mock-ups; use agents where useful; keep the design consistent.
- **Recipe list redesign:** you'll come back to it. Remind you (below).

## Playtest feedback (what you reported → what changed)

- **Sheet, first look** "a lot closer to the reference" wanted → generated art, live picture. Feet above the stone and talisman backing → fixed. "Looking much better".
- **Tabs:** the grid broke past its first item → missing callbacks wired. Left-click pick-up failed → the game's drag cancel was handled. All passed.
- **Classes UI, first look** "very basic", with several broken bits:
  - **Fixed:** the white empty slot, overlapping names, lines through names, dark icons.
  - **Restyle:** four mock-ups led to your mix.
- **New look:**
  - **Paths:** learned paths "look bad" → tree-branch paths, then the full redesign.
  - **Bars:** eitr pushing stamina → bar order swapped.
  - **Building:** bars moving up "not good design" → the build box goes to the bottom right.
- **Cross:**
  - **Layout:** spread it out, power in the middle, no texts → done.
  - **Adrenaline:** an orange bar appeared under the bars after Eikthyr's power (the game's adrenaline bar) → replaced by ours, above health.
- **Build box:** only the name moved → the whole box moves. It touched the XP hint → moved higher.
- **Skills panel, redesign:**
  - **Details line:** ran out of its box → shortened and shrinks to fit.
  - **Lock reason:** cut off → now on the Learn button.
- **Cross labels:** "Weapon"/"Eitr" confusing → "Need bow", "Need spear", "Low eitr", and the demo shows real states. You also asked what the demo is: explained (sample data until Orlog has classes).
- **The + buttons click twice** → one click (untested).

## Open questions

1. **Riftforge window: its own window or a tab in the Riftforge's crafting panel?** Claude recommends the tab: it's how the game's stations work, it keeps the map and offering recipes one click away, and your inventory stays beside it. Anything to change in the mock-ups?
2. **Ship display:** fine at the bottom right too (you named only the build box)?
3. **Recipe list redesign and the crafting screen with the backpack panel open (U4):** what would you like changed? You asked to be reminded.
4. **Dedicated server:** does `devcommands` work for you there? The multiplayer evening's steps use `spawn`, `co_corrupt` and `orlog_level`.
5. **For the loot session, not this one:** the brief's four open questions (§6: Annulment to 0 affixes, Orb of Chance odds, Mirror on corrupted items, Jeweller's greyed).

## Next steps

1. **Riftforge window (U3):**
   - Line the mock-ups up with `docs/riftforge-ui-brief.md` (operation list with refusal texts, the game's own tooltips, slots pointing at inventory items).
   - Show you the chosen placement, and get your pick.
   - Build it in Corruption from a tested layout class with a preview (the skills panel's pattern), against the brief's `RiftforgeCrafting` calls.
2. **The multiplayer evening (you):** run `PLAYTEST.md` → Session 2 with the group, then fix what it finds.
3. **Real Orlog data (U2)** as the classes session fills `Orlog.Api`: drop the demo the day Orlog serves skills, slots and slot states, and re-check the layout items that need real data.
4. **Small play-test:** the adrenaline bar and the one-click buttons.
5. **Recipe list redesign (U4)** once you say what you want.

## Merge notes

- **Ready to merge?** Already merged (PR #19, 2026-11-02): the click fix, the build stamp, the housekeeping docs and the mock-ups are self-contained; nothing is half-built in code. The Riftforge window itself isn't started in code.
- **Files shared with other sessions** (merge carefully, keep both sides):
  - **Docs:** `FEATURES.md` (changelog), `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md` (Session 2 rewritten as the evening script), `docs/GAME_DESIGN.md` (status table), `docs/PLAYER_SETUP.md` (Orlog added), `docs/ARCHITECTURE.md` (build stamp line).
  - **Corruption code:** `Corruption/Content/InventoryTabsUi.cs` (click fix), which the loot session doesn't touch. The Riftforge window will add new files under `Corruption/Content/` and one hook in `CorruptionPlugin.cs`, the expected merge point.
- **Phone play-test log:** private to you; Session 2 is replaced by the evening's short version.
