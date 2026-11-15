# Handoff: `claude/laughing-mccarthy-fnz0to`

**Written:** 2026-11-15 by `/handoff`. 10 commits ahead of `master` with this one, 0 behind. No pull request yet.

## Stage

**Ready to merge.**

## Purpose

The Amazon's skill tree overhaul ([#66](https://github.com/krogon-games/JotunnModStub/issues/66)): no branching, any skill from level 1, nine skills per tab with a new bow list, and the skills panel as skill cards after Krogon's mock-up (`docs/images/skills_panel_cards_mockup.webp`).

## Done

Krogon's single-player play-tests passed (2026-11-15): all 15 steps of the overhaul (points in removed skills handed back; nine cards per tab; any skill from level 1; Learn and its refusals; slots; the skills that stay; no errors), then the 3-step re-check (the Multishot name, attribute icons).

## Half-done and known issues

Detail in `FEATURES.md`.
- **Seven bow skills don't work yet** ("Coming later"): Forked Lightning, Wind Lance, Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot. The five new ones have no eitr cost until agreed.
- **Removed with their code and four Harmony patches:** Magic, Fire, Cold, Ice and Exploding Arrow (last commit with them: `6090e71`; Exploding Arrow's blast is the start for Immolation Arrow). Off the lists: Guided Arrow, Slow Missiles, Power Strike.
- **Multishot's id stays `MultipleShot`** (saved points, slots, `orlog_skill`).
- **Claude's choices, open to change:** D2's pace stays (a skill holds at most as many points as you have levels); the card order on the passive and spear tabs; attribute icons all gold.
- **Not tried:** anything with two players (nothing new).
- **New:** `AGENTS.md` (the subagent split), linked from `CLAUDE.md`.

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
17. **2026-11-15: the re-check of the Multishot name and the attribute icons passed** ("i did the re-check, all passed"; 3 steps Pass on the phone page).
18. **2026-11-15: `AGENTS.md`'s subagent split stays on request only** ("lets keep the agents default to be as me asking for subagents to be used for now at least").

## Playtest

`docs/playtests/claude-laughing-mccarthy-fnz0to.md`: both rounds **passed** in single player (2026-11-15), no notes. Its steps are off the phone page.

## Open questions for Krogon

1. Every new skill's details (cost, damage, range, Wind Lance's and Snipe's charge, Shackle Shot against Dota 2's), in the round that builds them.

## Next steps

1. Pull request into `master`; Krogon merges.
2. Coordinator: version bump (Orlog, CraftingOverhaul), `STATUS.md`, `DECISIONS.md` (decisions 1–18), close #66, update #41 and #44 (their skill lists changed).
3. Next classes round, new branch: Forked Lightning and Wind Lance, then the other five bow skills; C6 (#41) after.

## Checks

- Build: PASS (573 tests; Orlog and CraftingOverhaul build).
- Version: gameplay changed: version bump needed at merge (Orlog, CraftingOverhaul). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested (no new setting, message or saved key; the skill table is compiled in and syncs through the version bump).
- Assets and names: PASS (nine new icons, eight removed; no new prefab names or tokens).
- FEATURES.md: PASS.
- In-game: PASSED in Krogon's single-player play-tests (2026-11-15); two players untested.

## Merge notes

**Ready to merge: yes** (final play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog, CraftingOverhaul.** No other remote branch has unmerged commits. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`; also `CLAUDE.md` (one line).
