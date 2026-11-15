# Play-tests: `claude/laughing-mccarthy-fnz0to`

## Round: the skill tree overhaul and the skill cards panel (2026-11-15)

**Build:** the three DLLs from this branch. The log lines should read `Orlog 0.3.0 loaded (build: claude/laughing-mccarthy-fnz0to <commit>)` and `CraftingOverhaul 0.4.0 loaded (build: claude/laughing-mccarthy-fnz0to <commit>)` (the versions are raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`. Use the character you tested the Amazon with last time (step 1 needs its old skill points). `orlog_class Amazon` if it has no class.

What it should look like: `docs/images/skills_panel_preview.png` (bow tab), `skills_panel_preview_passive.png`, `skills_panel_preview_spear.png`.

### Checklist

**Your old character**

1. [x] Before opening anything, type `orlog_info` → the Skills line no longer lists Magic Arrow, Fire Arrow, Cold Arrow, Ice Arrow or Exploding Arrow, and the points you had in them are back in "skill points to spend". A skill slot that held one of them is empty on the skill cross. (Skip if the character never had points in those.)

**The look**

2. [x] Press `K` → the skills panel opens, wider than before. Left side as before (level, XP, attributes, the skill's details). Right side: three tabs, **nine cards, three by three**, and under them a strip with the four skill slots on the left and the **Learn** button on the right. No tier rows ("Lv 1 … Lv 30"), no lines between skills.
3. [x] Bow and Crossbow reads, row by row: Forked Lightning, Wind Lance, Snipe / Multiple Shot, Strafe, Freezing Arrow / Immolation Arrow, Venom Strike, Shackle Shot. Seven of them are darker and say "Coming later"; Multiple Shot and Freezing Arrow are the two that can be learned. Every card has an icon.
4. [x] Passive and Magic: Inner Sight, Critical Strike, Penetrate / Dodge, Avoid, Evade / Decoy, Valkyrie, Pierce. Javelin and Spear: Jab, Impale, Fend / Charged Strike, Lightning Strike, Lightning Bolt / Poison Javelin, Plague Javelin, Lightning Fury.
5. [x] **Your verdict on the look:** do the cards fit the rest of the UI? Is every name readable (none cut off or running into the rank line)? Does the cards' wood background look right in game? A screenshot of anything that looks off helps.

**Learning: any skill from level 1**

6. [x] `orlog_respec`, then `orlog_level 2` → "Skill points: 1" at the top right; the cards that can be learned say "Not learned" in green and their icons glow green.
7. [x] Click **Freezing Arrow** (it needed level 30 before) → a gold glow around the card, its details on the left, and the button at the bottom right says "Learn (+1 rank)". Click it → the card says "Rank 1 / 20" with a purple bar, turns brighter, and its icon glows blue. No "Needs level …" or "Needs Ice Arrow".
8. [x] With no points left, click Multiple Shot → the button says "No skill points" in red and can't be pressed; the unlearned cards say "Not learned" in grey.
9. [x] Click Forked Lightning (or any "Coming later" card) → its details say what it will do and "Not in the game yet"; the button says "Coming later".
10. [x] `orlog_level 30`. **Double-click** Multiple Shot → it gains a rank each time (double-click = Learn). Then `orlog_skill Dodge 20` → on the Passive and Magic tab Dodge says "Rank 20 / 20" with a full gold bar and a gold glow; the button says "Mastered".
11. [x] Point at a card without clicking → the left pane shows that skill; move away → it shows the selected card again.

**Slots and the skills that stay**

12. [x] Drag the Multiple Shot card onto slot F1 in the strip, and Freezing Arrow onto F2 → their icons appear there and on the skill cross. Drag Dodge → "Passive skills don't take a slot". Drag Strafe → "Coming later". Right-click a slot → it empties; drag the skill back.
13. [x] Close the panel, take a bow, press F1 and hold right click → Multiple Shot fires its fan as before. F2 → Freezing Arrow bursts and freezes as before.
14. [x] Inner Sight still casts from its slot key, and a learned passive still triggers (`orlog_passive dodge 100`, let a greyling hit you while standing still → "Dodge").

**Log**

15. [x] F5 shows no red errors; `BepInEx/LogOutput.log` has no "Orlog could not …" or "CraftingOverhaul could not …" lines.

**With another player:** nothing new. The removed skills leave fewer two-player checks in `PLAYTEST.md` → Session 2, 2j.

### Your feedback (2026-11-15)

Read from the phone page (ids `cards-1` to `cards-15`, marked 10:43–10:48 UTC): **all 15 steps Pass, no notes.** In the chat: "i filled out the playtest for this session. im really happy with it. well done!"

### What changed after the feedback

Nothing: no step failed, so there was nothing to fix.

### Final play-test (2026-11-15)

**Passed** (single player): the first pass stands as the final one, since nothing changed after it. Not covered: anything with two players (nothing new for the group evening), and the seven bow skills that don't work yet. The round's steps are off the phone page.

## Round: the Multishot name and the attribute icons (2026-11-15)

Two small changes Krogon asked for after the round above passed.

**Build:** the three DLLs from this branch again (a newer commit in the log lines than the first round's).
**Setup:** as above. What it should look like: `docs/images/skills_panel_preview.png`.

### Checklist

1. [x] Press `K` → each attribute row has an icon left of its name: an arm (Strength), crossed arrows (Dexterity), a heart (Vitality), a bolt (Energy). The "what it gives" line is still under each name and doesn't run into the + button.
2. [x] The bow tab's card and the details pane say **Multishot**. Your points in it are still there, a slot that held it still does, and it still fires its fan from the skill button.
3. [x] F5 shows no red errors.

### Your feedback (2026-11-15)

Read from the phone page (ids `cards2-1` to `cards2-3`, marked 20:12 UTC): **all 3 steps Pass, no notes.** In the chat: "i did the re-check, all passed."

### What changed after the feedback

Nothing.

### Final play-test (2026-11-15)

**Passed** (single player). The steps are off the phone page.
