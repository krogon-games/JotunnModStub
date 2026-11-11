# Handoff: `claude/lucid-albattani-imrei7`

**Written:** 2026-11-11. Started from `master` at PR #63 (level with it). No pull request yet.

## Stage

**Waiting for Krogon's playtest**: C3 (passives), C5 (skill bar, bow skills, Inner Sight) and C4's boss points, all built 2026-11-11.

## Purpose

The Amazon (`docs/ROADMAP.md` §4.1): C3, her passives (issue #32); then, at Krogon's request, C5 (skill button, four slots, the bow skills and Inner Sight, issue #37) and C4's boss bonus points (issue #36). The session started as a coordinator review (top 3 priorities agreed; `STATUS.md` is updated at `/handoff`).

## Done

Nothing confirmed in game yet.

## Half-done and known issues

- **C3 built, untested in game** (`FEATURES.md` → Amazon passives): Critical Strike, Dodge, Avoid, Evade, Penetrate with Krogon's rules; tooltips show the chance; `orlog_passive <effect> <chance>` for sure procs. Read in the cloud's 1.0.14 game code, not checked against 1.0.21.
- **How it crosses games:** Penetrate's marker rides in the hit's eitr field (`m_eitrAdd`, `-1000 - original`); creature area and breath hits on players get a radius (`m_radius`) on the creature's game so the player's game knows they can't be avoided. Both fields are sent with every hit and otherwise unused on creature/player hits (game code).
- **Assumption:** breath is found by the attack item's prefab name ("breath"); if the game leaves the name empty for creature attacks, breath could be avoided (play-test step 10).
- PvP hits are neither crit, penetrated nor avoided (creatures only).
- **C5 built, untested in game** (`FEATURES.md` → Amazon skill bar and bow skills): right click = skill button through `Player.SetControls`; block on the mouse's back button while the game's Block is right click; F1–F4 (read from the three mods' decompiled code: none uses them). Magic Arrow, Fire, Cold, Ice, Exploding and Freezing Arrow, Multiple Shot, Inner Sight. The skill bar API for the classes UI is real. No gamepad support yet.
- **Damage scale is a proposal** (`Orlog/Data/skill_damage_scale.csv`): Valheim bow + arrow of the level's biome ÷ D2's best bow by that level (D2's bows from D2R's `weapons.txt`, `data/d2-reference/bows.csv`). It keeps D2's skill-to-bow ratio (×22 at level 1, ×13 mid-game, ×4–6 late). A creature-health-based scale (D2 monster life per level from `monlvl`/`monstats` vs Orlog's biome health) came out 7–10× lower mid-game; noted as the alternative.
- **Claude's interim choices (easy to change):** Multiple Shot's fan 5° between arrows, at most 60°; Exploding Arrow's radius = Freezing Arrow's 3.3 yards (D2 gives none); Inner Sight +25% damage taken (the game's "slightly weak" step; config `InnerSightDamageTaken`).
- **C4 boss points built, untested:** 12 in all from first boss kills; the respec item and boss XP multiplier are not built.
- **Boss XP proposal (not built):** D2's act bosses give ×9.4 (Andariel), ×17.1, ×22.0, ×35.7, ×66.6 (Baal) a normal monster's XP of their level (D2R `monstats` Exp ÷ the median normal monster's, 97). Spread over our eight bosses by order: Eikthyr ×9.4, Elder ×13.2, Bonemass ×17.7, Moder ×20.5, Yagluth ×25.3, Queen ×33.3, Fader ×46.6, Kall ×66.6.
- Loot 3c play-test marks read from the phone page (8 of 10 passed on 2026-11-11, Splintering "needs a little bigger radius/hit detection range"): to record in issue #26 and `STATUS.md` at `/handoff`.

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

## Playtest

Krogon: the 12 solo steps in `docs/playtests/claude-lucid-albattani-imrei7.md`, also on the phone page ("The Amazon's passives"). Multiplayer steps: `PLAYTEST.md` → Session 2, 2j (and the phone page's 2j). No feedback yet.

## Open questions for Krogon

1. **The damage scale:** keep the proposed one (D2's ratio of skill damage to bow damage; skills feel like D2, strong), or the creature-health one (7–10× lower mid-game; Valheim's own weapons stay the main damage)? Play-test step 15 of round 2 helps; `SkillDamageMultiplier` tunes it in the meantime.
2. **Boss XP multipliers** as proposed (Eikthyr ×9.4 … Kall ×66.6)?
3. **Inner Sight's extra damage:** 25% flat, or growing with its level?
4. **The interim choices:** Multiple Shot's fan, Exploding Arrow's 3 m radius.
5. The reset item's recipe for players without Corruption (C4).

## Next steps

1. Krogon's play-test of rounds 1 and 2 (`docs/playtests/claude-lucid-albattani-imrei7.md`, also on the phone page); fix what fails.
2. Krogon's answers to the open questions; then the boss XP multiplier and the respec item (C4).
3. `/handoff`: `STATUS.md` (the coordinator update agreed this session), comment on #26 with the loot 3c marks.
4. Next: C6, the spear and javelin skills.

## Checks

Not run by `/handoff` yet. So far: `dotnet test CraftingOverhaul.Tests` 549 passed; Orlog builds against the stand-in game assemblies.

## Merge notes

Not ready (waiting for the play-test). **Gameplay changed: version bump needed at merge (Orlog).** Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/mod-compatibility.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`, `docs/GAME_DESIGN.md`, `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj`. New reference data `data/d2-reference/bows.csv` (never ship it). No Corruption or CraftingOverhaul code changed.
