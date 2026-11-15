# Handoff: `claude/laughing-mccarthy-fnz0to`

**Written:** 2026-11-15 (the round's build is done; `/handoff` not run yet). Ahead of `master`, 0 behind; no pull request yet.

## Stage

**Fixing feedback** (after the passed play-test: the Multishot name, attribute icons, `AGENTS.md`).

## Purpose

The Amazon's skill tree overhaul ([#66](https://github.com/krogon-games/JotunnModStub/issues/66)): no branching, any skill from level 1, nine skills per tab with a new bow list, and the skills panel redrawn as skill cards from Krogon's mock-up (`docs/images/skills_panel_cards_mockup.webp`).

## Done

Krogon's single-player play-test passed (2026-11-15, all 15 steps, no notes): points in removed skills handed back; nine cards per tab; any skill from level 1; the Learn button and its refusals; a maxed skill; slots; Multiple Shot, Freezing Arrow, Inner Sight and a passive still work; no errors.

## Half-done and known issues

Built and passed alone (detail in `FEATURES.md` → Skill tree and skill points, Classes UI, Amazon skill bar and bow skills):
- **Rules (Orlog):** no tiers or prerequisites; exactly nine skills per tab (the table refuses any other count); points in removed skills are handed back; `reqLevel` and `prereqs` left the API.
- **Removed with their code and four Harmony patches:** Magic, Fire, Cold, Ice and Exploding Arrow (last commit with them: `6090e71`; Exploding Arrow's blast is the start for Immolation Arrow). Also gone from the lists: Guided Arrow, Slow Missiles, Power Strike.
- **Panel (CraftingOverhaul):** nine cards per tab, Learn beside the slots, 1148 × 690; five new placeholder icons; previews `docs/images/skills_panel_preview*.png`.
- **Seven bow skills don't work yet** ("Coming later"): Forked Lightning, Wind Lance, Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot. The five new ones have no eitr cost until agreed.
- **Claude's choices, open to change:** the name stays "Multiple Shot"; D2's pace stays (a skill holds at most as many points as you have levels); the attribute rows keep their "what it gives" lines and have no icons (the mock-up has icons, no lines); the card order on the passive and spear tabs; the rank bar is the XP bar's purple; unlearned cards that can take a point show "Not learned" in green.
- **Can't be judged outside the game:** the cards use the game's own panel background; the preview uses a look-alike.

## Decisions

1. **2026-11-15: this round is issue #66, the Amazon's skill tree overhaul, on this branch** ("Yes this is the right round and branch").
2. **2026-11-15: the redesign covers all three Amazon skill tabs and any future class** ("The redesign is for all amazon skill tabs and any future new characters").
3. **2026-11-15: the skills panel shows skills as cards, after Krogon's mock-up** (`docs/images/skills_panel_cards_mockup.webp`: a grid of cards with icon, name, rank and a rank bar; the selected skill's details on the left; the slots and the Learn button along the bottom). The mock-up's look differs from the rest of the UI, so Claude builds it in the existing UI's style ("Lets implement skill cards look. the mock up deviates away in design look quite a bit. So your role is to implement it with consistency to the rest of the UI design").
4. **2026-11-15: the bow tab's final list of 9** (the mock-up's list was only an example): "Final list is Forked Lightning, Wind Lance, Snipe, Multi Shot, Strafe, Freezing Arrow, Immolation Arrow, Venom Strike, Shackle Shot (dota 2 windranger ability)". So Magic Arrow, Fire Arrow, Cold Arrow, Exploding Arrow, Ice Arrow and Guided Arrow go; Snipe, Venom Strike and Shackle Shot are new, with Forked Lightning and Wind Lance.
5. **2026-11-15: this round builds the panel and the rules only** ("Panel and rules only"): the cards panel, no level tiers or prerequisites, the new skill list. Skills that don't work yet show as "Coming later"; a later round builds them.
6. **2026-11-15: every tab has exactly 9 skills** ("Always exactly 9"), for every class. One skill leaves the Passive and Magic tab and one the Javelin and Spear tab (which: asked).
7. **2026-11-15: the fourth attribute stays "Energy"** ("Keep Energy"); the mock-up's "Eitr" isn't taken over.
8. **2026-11-15: Slow Missiles leaves the Passive and Magic tab** ("Slow Missiles"). Its 9: Inner Sight, Critical Strike, Dodge, Avoid, Evade, Penetrate, Decoy, Valkyrie, Pierce.
9. **2026-11-15: Power Strike leaves the Javelin and Spear tab** ("Power Strike"). Its 9: Jab, Impale, Fend, Charged Strike, Lightning Strike, Poison Javelin, Lightning Bolt, Plague Javelin, Lightning Fury (the rest can still change in the spear round, #41).
10. **2026-11-15: Snipe, in Krogon's words:** "Charge a powerful shot, increasing damage the longer the charge. Drains stamina while charging and slowly zooms in on the target."
11. **2026-11-15: Venom Strike is a poison cloud** ("Poison cloud": the arrow bursts into a poison cloud where it lands, poisoning everything that stands in it).
12. **2026-11-15: the play-test passed and the look is approved** ("i filled out the playtest for this session. im really happy with it. well done!"; all 15 steps Pass on the phone page). The cards panel and the new rules stand as built, Claude's own choices included (listed above), unless he says otherwise.
13. **2026-11-15: Multiple Shot is renamed "Multishot"** ("Change the name to Multishot").
14. **2026-11-15: the attribute rows get icons, and keep their "what it gives" lines** ("Add the icons, 'what it gives' lines should stay"), as in his mock-up.
15. **2026-11-15: the subagent split goes in an `AGENTS.md` note** ("maybe you should make a note in agents.md file the breakdown you did previously, of which model you would assign which tasks"): which model gets which kind of work, and how the main session checks it.
16. **2026-11-15: the panel in game "is looking really good"** (his screenshot: `docs/images/skills_panel_cards_ingame.webp`).

## Playtest

`docs/playtests/claude-laughing-mccarthy-fnz0to.md`: **passed** (2026-11-15, single player, all 15 steps, no notes; nothing to fix, so it is also the final play-test). Its steps are off the phone page. Nothing new for two players.

## Open questions for Krogon

1. "Multi Shot" or "Multiple Shot"?
2. Attribute icons as in the mock-up, and keep or drop the "what it gives" lines?
3. Every new skill's details (cost, damage, range, Wind Lance's and Snipe's charge, Shackle Shot against Dota 2's), in the round that builds them.

## Next steps

1. `/handoff`, then a pull request into `master`.
2. Coordinator at merge: version bump (Orlog, CraftingOverhaul), `STATUS.md`, `DECISIONS.md` (decisions 1–12), close #66, update #41 and #44 (their lists changed).
3. Next rounds: the seven bow skills, a few at a time, each after its details are agreed; then C6 (#41).

## Checks

- Build: PASS (573 tests; Orlog and CraftingOverhaul build).
- Version: gameplay changed: version bump needed at merge (Orlog, CraftingOverhaul). The branch changed no version.
- Multiplayer sync: not checked yet (`/handoff`). The skill table is compiled into the DLL; everyone needs the same build.
- Assets and names: five new icon files, eight removed (`CraftingOverhaul/Assets/Skills/`); no new tokens.
- FEATURES.md: updated in the same commits.
- In-game: PASSED in Krogon's single-player play-test (2026-11-15); two players untested (nothing new).

## Merge notes

**Ready to merge: after `/handoff`** (play-test passed; its checks not run yet). **Gameplay changed: version bump needed at merge: Orlog, CraftingOverhaul.** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/GAME_DESIGN.md`, `docs/mod-compatibility.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`. `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj`: one linked file renamed (`SkillTreeLayout.cs` → `SkillCardsLayout.cs`).
