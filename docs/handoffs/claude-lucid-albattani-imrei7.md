# Handoff: `claude/lucid-albattani-imrei7`

**Written:** 2026-11-15 by `/handoff`. 34 commits ahead of `master` with this one, 0 behind. No pull request yet.

## Stage

**Ready to merge.**

## Purpose

The Amazon (`ROADMAP.md` §4.1): passives (C3, #32), skill button, slots, bow skills, Inner Sight (C5, #37), boss points and boss XP (C4 part, #36), quick cast, demo removed (U2, #38).

## Done

Krogon's single-player play-test passed (2026-11-15): all five passives; slots, F1–F4, the skill button; Multiple Shot, Freezing Arrow; Inner Sight (5% → 24%, no Forsaken power); quick cast; tooltips; boss points and boss XP on Eikthyr; respec clears the cross; F2 for skills (Ctrl+F2: the game's stats window); no red errors.

## Half-done and known issues

Detail in `FEATURES.md`.
- **Not tested, to be reworked in the skill overhaul (decision 32):** Magic Arrow without arrows, Fire, Cold and Ice Arrow, Exploding Arrow on a group.
- **Not tested:** the block default (`Mouse4`; decision 31: a key-binding screen comes later); Kall's boss XP on the final phase; anything with two players (`PLAYTEST.md` → 2j).
- **Limits:** no gamepad; PvP untouched; the freeze stops movement only; Inner Sight's glow only on the caster's game.
- **Not built:** the respec item (low priority, decision 26); Gungnir's Throw and the spear skills (C6).
- **For the coordinator (on `master`):** `STATUS.md` and `docs/DECISIONS.md` (decisions 1–32); #32, #36, #37, #38 and the demo item of #50 at merge. Other rounds' marks from today: see the play-test file's last section.
- Game code in the cloud: `ilspycmd` 8.2.0.7535 (newer needs a newer SDK), `DOTNET_ROLL_FORWARD=Major`; `ZInput` is in `assembly_utils.dll`.

## Decisions

1. **2026-11-11: the top 3 stay as agreed this morning, plus two small changes** ("Yes, agree"): (1) release build and playing together (#25, #24); (2) orb drops (#33) as the next loot round, carrying Splintering's reach and the Stonecutter's/Resinous rework; (3) the Amazon's passives (#32), unblocked by answering here. Map tiers (#31) fourth. The "Use this at a Riftforge" refusal counts as passed (step 17 of `docs/playtests/claude-lucid-carson-hs5g89.md` is ticked).
2. **2026-11-11: `STATUS.md` and the comment on #26 wait for `/handoff`** at the end of the session.
3. **2026-11-11: this round is C3, the Amazon's passives, on this branch** ("start on Amazon's passives (#32)"; "Continue with your recommended suggestions").
4. **2026-11-11: Krogon took every recommendation** ("Continue with your recommended suggestions"):
   1. **Chances: Diablo 2's unchanged.** Critical Strike 16→68%, Dodge 18→56%, Avoid 24→65%, Evade 18→56% (skill levels 1→20; `data/d2-reference/amazon_levels.csv`, Arreat Summit, D2R the same).
   2. **A crit doubles physical damage only** (blunt, slash, pierce; not fire, frost, poison, lightning, spirit), with any weapon, against creatures only (not trees, rocks or buildings).
   3. **Crits stack with the game's sneak attack and stagger bonuses** (a crit sneak shot with a bow ×6, a crit on a staggered creature ×4). One simple rule.
   4. **A crit shows the game's own critical-hit effect** (the one a staggered creature's hit plays).
   5. **Diablo 2's split:** Dodge = close-range hits while standing still or attacking; Avoid = projectiles while standing still or attacking; Evade = any hit while walking or running.
   6. **Unavoidable:** area blasts and breath, burning and poison ticks, falls and traps. A boss's ordinary swings and projectiles can be avoided.
   7. **While holding block there is no dodge roll;** the block or parry handles the hit (so parry affixes keep working).
   8. **A dodge shows a floating "Dodge" / "Avoid" / "Evade" and a short whoosh, no animation.**
   9. **Penetrate uses Critical Strike's curve** (16→68%).
   10. **Penetrate turns any resistance (slight ¾, resistant ½, very resistant ¼) into full damage, but not immunity.** Weaknesses unchanged.
   11. **D2R's numbers** where D2 Resurrected changed a skill (the bow skills' eitr costs and a few damage numbers; the passives are the same in both).
   12. **Boss XP (C4):** Claude derives a multiplier per boss from Diablo 2's act bosses, for Krogon to approve.
   13. **Bonus skill points (C4):** 12 in total, 1 each for the first four bosses (Eikthyr, the Elder, Bonemass, Moder) and 2 each for the last four (Yagluth, the Queen, Fader, Kall Fimbulbringer).
   14. **The reset item with Corruption (C4):** one essence each from 4 different bosses. Without Corruption: still open.
   15. **Eitr regen without eitr food:** keep the game's 5 per second for now, adjust after play-testing.
   16. **Early eitr potions:** Orlog owns them; Claude proposes recipes from early materials, modelled on the game's eitr meads, for approval.
   17. **Right click with no skill chosen (C5):** does nothing.
   18. **The chosen skill is remembered per weapon kind (C5):** the last bow skill and the last spear skill separately.
5. **2026-11-11: build the rest of the Amazon in this session too** ("I'd suggest that you continue building out the Amazon with her other skills as well"). Built: C5 and C4's boss points; numbers not yet decided were proposed (above) and wait for Krogon.
6. **2026-11-12: skill use simplified** ("left click would always be your basic attack… right click becomes the selectable character skill button, whether it's throw a javelin or multi-shot or strafe or any of the other skills"; the 4-slot skill cross stays). This replaces `class-design.md` §5's "spear skills ride the spear's own attacks": spear and javelin skills fire from right click too.
7. **2026-11-13: quick cast per skill** ("quick cast per skill would be awesome"): a skill that needs no weapon casts straight from its skill cross hotkey when quick cast is on; one that needs a weapon goes on right click. Right-clicking a slot on the skill cross toggles quick cast; a quick-cast slot shows a rotating shimmer around it ("exactly like the dota 2 autocast animation").
8. **2026-11-13: block is a rebindable key** (`6 - Skill Keys` → `BlockKey`), **blank by default for now** ("are you able to use a blank keybind for the block? i cant think of a good alternative at the moment"). So with a class, blocking needs a key set there, or Block set to another key in the game's controls.
9. **2026-11-13: keep remembering the right-click skill per weapon kind** ("hopefully that will work. will need to consider edgecases").
10. **2026-11-13: the game's own throw (middle mouse) stays exactly as it is; we don't touch it.** Instead a new skill copies the throw with a perk: the weapon never leaves the player's hand, a ghost copy flies at what the player aims at, so nothing litters the ground ("most players dont use the throw skill because you have to go fetch your weapon after… unfun").
11. **2026-11-13: the ghost throw, Claude's three suggestions** ("use your suggestions for all three questions"): a basic skill outside the tree like Diablo 2's own Throw, named **Gungnir's Throw**, for every character with a class, no skill points; it costs **stamina** like the game's throw and does **the same damage** as the game's throw with that spear; it aims where the camera points. The Amazon's javelin skills throw the same ghost copy (roadmap C6).
12. **2026-11-13: remove the classes UI demo** ("yes go ahead, remove the demo"; roadmap U2, issue #38): Orlog fills the class description and tab names first, then the demo's code, setting and `co_classes_stone` go.
13. **2026-11-13: make the skills panel bigger** ("please enlarge the skills panel window, its hard to read"), then **10% smaller than that** ("Skills panel is much better now. maybe even tone it down by 10%"): it fills 79% of the screen's height (or 83% of its width); `SkillsPanelSize` fixes a size.
14. **2026-11-13: block on the mouse's side button is the mouse software, not the mod** ("block holds fine on another key, it's my mouse software").
15. **2026-11-13: wrap up; the play-test waits for a fresh session** ("Save the playtesting for another session so its fresh").
16. **2026-11-15: end the session; the play-test and bug fixes continue in the next session,** with a longer handoff allowed ("Ill allow you to increase the limit on the handoff to make sure everything important carries accross").
17. **2026-11-15 (new session): this round is fixing the two bugs, then play-test rounds 1 and 2, on this branch** ("Yes to both"). Open questions asked as they come up.
18. **2026-11-15: Claude may install `ilspycmd` in the cloud session to read the game's code** ("go ahead and install ilspycmd"). Version 8.2.0.7535 (the newest needs a newer .NET SDK), in the session's scratchpad only.
19. **2026-11-15: the blast arrows' fix passed** ("That works now"; Freezing and Exploding Arrow on a troll's head and body).
20. **2026-11-15: F1–F4 are the skill keys; the game's F2 stats window must not open with them** ("We need the F1 F2 F3 F4 buttons for skills"). Claude's choice, open to change: Ctrl+F2 still opens the window.
21. **2026-11-15: the Amazon's skill tree will be overhauled** (Krogon's direction, details to agree): "Skill will not need to branch anymore. Players will be able to learn whichever skill they want even at level 1. We will also be getting rid of some of the cold and fire skills, spending a skill point should change how you use the bow, especially when ordinary ammunition already supplies fire and frost damage. Strafe and Multishot have clear identities; 'adds fire damage' doesn't have that same pull." Skill ideas he likes: **Forked Lightning** ("The arrow hits an enemy, then lightning forks outward from their body toward nearby enemies") and **Wind Lance** ("a charged elemental attack that turns your arrow into a javelin, and shoots it straight with no arrow drop like other arrow. It travels fast and in a straight line, forcing and knocking enemies back… may need some more tweaking, but thats a rough idea"). This replaces parts of `class-design.md` §4–§5 (tiers by level, prerequisites, the D2 skill list) once agreed.
22. **2026-11-15: keep the proposed skill damage scale for now** ("for now it feels fine. not 100% confident. will do balancing once mod is finished and we can play the game from scratch and progress through biomes. vanilla play may feel too easy, but we will have maps and corrupted bosses too").
23. **2026-11-15: Inner Sight's extra damage grows with its level:** 5% at level 1, +1% per level (24% at 20), no cap ("start at 5% and add 1% per level. we will have bonus skill points as well later to add extra % over 24% from hard points").
24. **2026-11-15: keep Multiple Shot's fan (5° apart, at most 60°) and Exploding Arrow's 3 m radius** (Claude's interim numbers, now agreed).
25. **2026-11-15: the skill tree overhaul (decision 21) is the next round,** in a fresh session on a new branch, after this play-test and merge. The fire and cold arrow steps of this play-test can be skipped, as those skills may go.
26. **2026-11-15: the reset item is low priority, a future feature** ("Skip reset item for now, place it on low priority. we can add this as a feature in the future"). The recipe without Corruption stays open until then.
27. **2026-11-15: block's default key is the mouse's back side button, rebindable** ("i think the block key needs to be on the mouse. but it really depends on how players want to play. put the keybind on mouse side button 4. but make sure its rebindable to something else"). `BlockKey` = `Mouse4` (the game's name for the back button).
28. **2026-11-15: boss XP: follow Diablo 2, and don't let players level too fast** ("work out the full boss XP list, follow diablo 2 style, maybe look at the new wow forever xp as well. I dont want players levelling too fast like diablo 4"). Claude's list follows (proposal 2026-11-15), waiting for approval.

   **Boss XP proposal (2026-11-15), for decision 28.** A boss gives ×N the XP of an ordinary creature of its level. Source: D2R `monstats.json` (d2data, fetched 2026-11-15): a monster's XP = its `Exp` rating × the XP for its level, so a boss's multiplier = its `Exp` ÷ an ordinary monster's. The median `Exp` of the 512 ordinary killable monsters (not bosses, not NPCs) is 97; the act bosses' are Andariel 916 (×9.4), Duriel 1655 (×17.1), Mephisto 2131 (×22.0), Diablo 3462 (×35.7), Baal 6460 (×66.6). D2 has 5 act bosses, Valheim 8: the first and last match, the six between are read off the line through D2's five, by position. "Share of a level" = the multiplier ÷ `creature_xp.csv`'s kills per level at the boss's level (a player of that level, alone; with four players each gets about half, by the +35% per extra player rule).

   | Boss | Level | XP | Share of a level, alone | Each of 4 players |
   |---|---|---|---|---|
   | Eikthyr | 5 | ×9.4 | 33% | 17% |
   | The Elder | 11 | ×13.8 | 23% | 12% |
   | Bonemass | 20 | ×17.8 | 16% | 8% |
   | Moder | 35 | ×20.6 | 11% | 6% |
   | Yagluth | 50 | ×25.9 | 9% | 5% |
   | The Queen | 62 | ×33.7 | 9% | 4% |
   | Fader | 76 | ×48.9 | 10% | 5% |
   | Kall Fimbulbringer | 90 | ×66.6 | 11% | 6% |

   Compared: classic WoW gives a dungeon boss ×2.5 an ordinary creature (elite in a dungeon, `data/wow-reference/README.md`); WoW Forever (launched 2026-11-04) keeps Classic's pace (Blizzard's beta data: 20.5 h to level 20 against Classic's 20.4 h) and raised dungeon kill XP by about 20–50% (guide sites, unconfirmed); no source found for its elite or boss multiplier.
29. **2026-11-15: the D2 boss XP list above is approved** ("approve the D2 boss XP list, build it"). Built: `biome_levels.csv` → `boss_xp` and `boss_xp_shares` (Kall's phases: see 30).
30. **2026-11-15: Kall pays all his boss XP on the final kill only** ("no, for Kall all the xp should be awarded on final kill only"; replaces Claude's split by phase health). His first two phases give no XP; `boss_xp_shares` 0;0;1.
31. **2026-11-15: a key-binding screen comes later** ("i think we will create a UI keybindings overhaul later on. so players can rebind whatever keys they want"); the block default's step was skipped for it.
32. **2026-11-15: the skills to be reworked aren't play-tested now** ("doesnt matter, will rework skill later": Magic Arrow without arrows, Fire, Cold, Ice and Exploding Arrow on a group).

## Playtest

Rounds 1–3 in `docs/playtests/claude-lucid-albattani-imrei7.md`: **passed** in single player (2026-11-15), apart from the skipped steps above. This branch's steps are off the phone page; two-player checks stay in `PLAYTEST.md` → 2j for the group evening.

## Open questions for Krogon

1. **Reset item without Corruption:** later (decision 26).
2. **The skill tree overhaul** (decision 21): which skills stay, go or come; level tiers and prerequisites; Wind Lance's details. Next round.

## Next steps

1. Merge into `master` (coordinator: version bump, `STATUS.md`, `DECISIONS.md`, issues).
2. Next round, new branch: plan and build the skill tree overhaul.
3. C6: Gungnir's Throw, spear and javelin skills.
4. Later: the respec item; a key-binding screen.

## Checks

- Build: PASS (561 tests; CraftingOverhaul and Orlog build).
- Version: gameplay changed: version bump needed at merge (Orlog, CraftingOverhaul). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested (Inner Sight settings admin-only, its % travels in the game's status-effect message; `BlockKey` per player; boss XP in the compiled table).
- Assets and names: PASS (no new shipped files or tokens).
- FEATURES.md: PASS.
- In-game: PASSED in Krogon's single-player play-test (2026-11-15); two players untested.

## Merge notes

**Ready to merge: yes** (play-test passed alone, checks pass, level with `master`). **Gameplay changed: version bump needed at merge: Orlog, CraftingOverhaul.** `claude/biome-load-freeze` also changes `CraftingOverhaul/CraftingOverhaul.cs`, the tests' `.csproj`, `FEATURES.md`, `docs/ARCHITECTURE.md`, `docs/CURRENT_STATE.md`: keep both sides. `data/d2-reference/` never ships.
