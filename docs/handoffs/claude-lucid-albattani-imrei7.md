# Handoff: `claude/lucid-albattani-imrei7`

**Written:** 2026-11-11. Started from `master` at PR #63 (level with it). No pull request yet.

## Stage

**Waiting for Krogon's playtest**: C3 (passives), C5 (skill bar, bow skills, Inner Sight) and C4's boss points, all built 2026-11-11.

## Purpose

The Amazon (`docs/ROADMAP.md` §4.1): C3, her passives (issue #32); then, at Krogon's request, C5 (skill button, four slots, the bow skills and Inner Sight, issue #37) and C4's boss bonus points (issue #36). The session started as a coordinator review (top 3 priorities agreed; `STATUS.md` is updated at `/handoff`).

## Done

Nothing confirmed in game yet.

## Half-done and known issues

All built 2026-11-11, **untested in game**, read in the cloud's 1.0.14 game code (not 1.0.21). Detail: `FEATURES.md` → Amazon passives, Amazon skill bar and bow skills, Boss bonus skill points.
- **Crossing games:** Penetrate's marker rides in the hit's `m_eitrAdd`; creature area and breath hits get `m_radius` so they can't be avoided. Breath is found by name (assumption: the attack item's prefab name is set).
- **Proposals waiting for Krogon:** the damage scale (`skill_damage_scale.csv`: D2's skill-to-bow ratio; a creature-health scale came out 7–10× lower mid-game); boss XP ×9.4 (Eikthyr) … ×66.6 (Kall), from D2R's act bosses (`monstats` Exp ÷ the median normal monster's 97), spread by order.
- **Claude's interim choices:** Multiple Shot's fan 5°/60°; Exploding Arrow's radius = Freezing Arrow's 3.3 yards; Inner Sight +25%.
- **Limits:** no gamepad; PvP untouched; the freeze stops movement only; Inner Sight's glow only on the caster's game. From the code review: blast hits count for Corruption's life steal per creature.
- **Not built:** the respec item, the boss XP multiplier (C4); spear skills (C6).
- **Coordinator, for `/handoff`:** loot 3c marks on the phone page (8 of 10 passed; Splintering "needs a little bigger radius/hit detection range") go into #26 and `STATUS.md`.

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

Krogon: rounds 1 (passives, 12 steps) and 2 (skill bar, bow skills, Inner Sight, boss points, 16 steps) in `docs/playtests/claude-lucid-albattani-imrei7.md`, also on the phone page. With others: `PLAYTEST.md` → Session 2, 2j. No feedback yet.

## Open questions for Krogon

1. **Damage scale:** the proposed D2 skill-to-bow ratio (strong), or the creature-health one (7–10× lower mid-game)? Round 2 step 15 helps; `SkillDamageMultiplier` tunes it meanwhile.
2. **Boss XP:** ×9.4 (Eikthyr) … ×66.6 (Kall) as proposed?
3. **Inner Sight:** 25% flat, or growing with its level?
4. **Interim choices:** Multiple Shot's fan, Exploding Arrow's 3 m.
5. **Reset item** recipe without Corruption (C4).

## Next steps

1. Krogon's play-test; fix what fails.
2. His answers; then boss XP and the respec item (C4).
3. `/handoff`: `STATUS.md` and the #26 comment (agreed this session).
4. C6, the spear and javelin skills (first settle how they fire: the design says on the spear's own attacks, the skill button says right click).

## Checks

Not run by `/handoff` yet: 549 tests pass; Orlog builds against the stand-in assemblies.

## Merge notes

Not ready (play-test). **Gameplay changed: version bump needed at merge (Orlog).** Shared docs touched, add-only: `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/PLAYER_SETUP.md`, `docs/ROADMAP.md`, `docs/mod-compatibility.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`, `docs/GAME_DESIGN.md`, the tests' `.csproj`. New reference data `data/d2-reference/bows.csv` (never ship). No Corruption or CraftingOverhaul code changed.
