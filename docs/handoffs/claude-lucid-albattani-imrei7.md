# Handoff: `claude/lucid-albattani-imrei7`

**Written:** 2026-11-11. Started from `master` at PR #63 (level with it). No pull request yet.

## Stage

**Building** (C3, the Amazon's passives).

## Purpose

C3 from `docs/ROADMAP.md` §4.1 (issue #32): the Amazon's passives Critical Strike, Dodge, Avoid, Evade and Penetrate, so her skill points finally do something. The session started as a coordinator review (top 3 priorities agreed; `STATUS.md` is updated at `/handoff`).

## Done

Nothing confirmed in game yet.

## Half-done and known issues

- Building C3.
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

## Playtest

Nothing yet. The checklist will go in `docs/playtests/claude-lucid-albattani-imrei7.md` and on the phone page.

## Open questions for Krogon

1. The reset item's recipe for players without Corruption (C4, not needed this round).

## Next steps

1. Build C3: pure rules and tests, the patches, tooltips, the debug commands.
2. Write the play-test checklist (repo file and phone page).
3. `/handoff`: `STATUS.md`, comment on #26.

## Checks

Not run yet.

## Merge notes

Not ready. Gameplay will change (Orlog): version bump needed at merge.
