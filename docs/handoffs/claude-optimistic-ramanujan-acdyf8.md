# Handoff: `claude/optimistic-ramanujan-acdyf8`

**Written:** 2026-11-29 by `/handoff`. Started from `master` (`3d8a80b`); 12 commits ahead with this one, 0 behind. No pull request yet.

## Stage

**Merged** (PR #106, 2026-11-29).

**Coordinator update (2026-11-29):** merged into `master` as **PR #106** on Krogon's word ("Merge this branch into master using a pull request"). The branch was level with `master` (no conflicts). The coordinator raised **Orlog to 0.11.0 and CraftingOverhaul to 0.7.0** (on this branch, before the PR), copied decisions 1–19 into `docs/DECISIONS.md` with Krogon's new call on Poison Javelin, added `PLAYTEST.md` → Session 2, 2j step 10 (`mpe-2j-10`, also on the phone page), and left #41 open for Jab, Impale and Poison Javelin's replacement.

## Purpose

C6 ([#41](https://github.com/krogon-games/JotunnModStub/issues/41)): Gungnir's Throw (the basic ghost javelin throw, no skill points), then the spear and javelin skills Krogon picks for this round.

## Done

Krogon's single-player play-tests (2026-11-28/29): round 1 (12 of 18), the fixes' re-test (8 of 8) and the final icon check (`fin-1`) all passed. Detail: `FEATURES.md` → Gungnir's Throw and the javelin skills.
- **Gungnir's Throw:** a Basic card above the skill tabs (always learned, no points, its own gold-spear icon); right click with a spear throws a see-through gold ghost copy with the throw's stamina and damage, raising Spears; it sticks, then fades; the spear stays in hand. The Abyssal Harpoon uses its left-click throw, no rope or pull.
- **Lightning Bolt:** twice as fast, stops at the first hit, 75% → 200% of the throw as lightning; crackling look.
- **Plague Javelin:** 100% hit, then a soft green cloud spreading to 4 m over 3 → 6.8 s, poisoning 25% → 100% a second; thin, faint ring.
- The freezes: the BepInEx console's text selection, documented (decision 19).

## Half-done and known issues

- Two players untested (the ghost is a networked prefab; the cloud and ring run on the thrower's game only, like Venom Strike's). Add to `PLAYTEST.md` → Session 2 when the coordinator merges (not done on this branch).
- Patches: new `Projectile.Setup` and `Projectile.OnHit` postfixes; `Player.SetControls` prefix and `Humanoid.StartAttack` postfix extended to the spear's throw.
- Claude's choices that passed without notes: holding right click keeps throwing; the cloud poisons from 1 s after landing; the Basic card's look; short eitr makes the throw a plain Gungnir's Throw.
- Jab, Impale and Poison Javelin still do nothing; #41 stays open. The ring's design for all area skills: #105.

## Decisions

1. **2026-11-28: this round is C6, on this branch** ("Yes, go ahead"): Gungnir's Throw first, then the spear skills Krogon picks.
2. **2026-11-28: Gungnir's Throw takes one of the four skill slots** ("It takes a slot"): it shows in the skills panel and goes into a slot like any other skill, fired from right click with a spear in hand; over "right click with no spear skill chosen" (no slot used) and its own key. Still no skill points (decision of 2026-11-13).
3. **2026-11-28: the ghost spear glows gold** ("Golden glow (Gungnir)"): see-through, warm gold, a soft glow and a faint trail; over a pale blue spirit glow and plain see-through. A preview picture comes before it is built.
4. **2026-11-28: the Abyssal Harpoon's Gungnir's Throw doesn't pull** ("No pull"): damage only; the pull stays on the game's own throw. Over copying the game's throw exactly.
5. **2026-11-28: this round builds Lightning Bolt and Plague Javelin with Gungnir's Throw** (both recommended options picked); Poison Javelin, Jab and Impale wait for a later round (#41 stays open for them).
6. **2026-11-28: Gungnir's Throw is a "Basic" card above the skill tabs** (recommended): always there and always learned, dragged into a slot from there; later classes' basic skills go there too. Over a tenth card on the spear tab.
7. **2026-11-28: a Gungnir's Throw hit raises the Spears skill exactly as the game's throw does** (recommended), over raising nothing.
8. **2026-11-28: the ghost spear sticks where it lands, then fades out over about a second** (recommended); nothing to pick up. Over vanishing at once with a gold flash.
9. **2026-11-28: Lightning Bolt and Plague Javelin cost eitr plus the throw's stamina** (recommended; the game's throw: flint spear 8, Nord spear 18, `item_details.csv` `secondary_attack_stamina`), as the bow skills cost eitr plus the draw. Over eitr only.
10. **2026-11-28: Lightning Bolt deals 75% of the ghost throw's damage at skill level 1, 200% at 20, all as lightning, in even steps** ("75% → 200%"; 75% is Diablo 2 Resurrected's weapon damage share, `amazon_skill_values.csv` `weapon_damage_pct`). Over 100% → 200%, 100% → 233% and 100% at every level. Eitr: Diablo 2's 6, +0.25 per level (6 → 10.75), plus the throw's stamina (decision 9).
11. **2026-11-28: Lightning Bolt flies twice as fast as a thrown spear and stops at the first enemy it hits** ("Twice as fast, stops at first hit"; Wind Lance's 200% speed). Over the throw's speed and passing through enemies.
12. **2026-11-28: Lightning Bolt looks like the gold ghost spear wrapped in crackling blue-white lightning**, bursting with the game's lightning hit effect (recommended), over a pure lightning streak. A preview picture comes first.
13. **2026-11-28: Plague Javelin's ghost spear hits for 100% of the throw's damage, then its cloud poisons for 25% → 100% of it** (recommended; Venom Strike's share, by the game's own poison), over a 50% → 150% cloud with or without the hit. Eitr: Diablo 2's 7, +0.5 per level (7 → 16.5), plus the throw's stamina (decision 9).
14. **2026-11-28: the cloud spreads from where the javelin lands out to 4 m across its time, and lasts 3 s at skill level 1, 6.8 s at 20** (recommended; the time is Diablo 2 Resurrected's poison duration, `amazon_skill_values.csv`; the 4 m was Claude's proposal, a little over Venom Strike's 3 m because it spreads). Over Venom Strike's 3 m and 4 s, and 3 → 6 m with level.
15. **2026-11-28: Plague Javelin looks like the gold ghost spear with green drips in flight, and Venom Strike's green cloud rolls outward where it lands** (recommended), over the plain gold spear. A preview picture comes first.
16. **2026-11-28: the preview of the three looks is approved** ("Approved, build it"; `docs/images/javelins_preview.png`, drawn by `tools/preview_javelins.py`).
17. **2026-11-28 (play-test): Plague Javelin's cloud grows in size, and its ring stays but blends in** (his note on `c6-2`: "it would be better if the poison cloud grew in size instead … keep the ring for now, maybe give it a slightly less intrusive and blended feel"). Built: soft green puffs fill the cloud's reach as it spreads; the ring is thinner and half see-through, the light dimmer. Venom Strike's and Immolation Arrow's rings are unchanged.
18. **2026-11-28 (play-test): the glowing ground ring's design is to be revisited for every area skill later**, as its own ticket ("add it as a ticket to work on in the future"): [#105](https://github.com/krogon-games/JotunnModStub/issues/105).
19. **2026-11-29: the freezes were the BepInEx console window's text selection; don't fix it, document it** ("that was most definitely the source of the glitch. no need to do anything to fix it. just document and move on. this should not happen in normal gameplay … the freeze/hang resumes the game as soon as you switch to the bepinex log window and press Esc to clear selection"). Documented in `FEATURES.md` → Freeze watch.

## Playtest

`docs/playtests/claude-optimistic-ramanujan-acdyf8.md`: round 1 (18 steps; 12 passed; the ghost was never made, the harpoon didn't throw, the cloud and ring changed), round 2 (8 of 8 passed, with the freeze found to be the BepInEx console), final check `fin-1` (the icon) passed 2026-11-28 on the phone page. All passed.

## Open questions for Krogon

1. None open.

## Next steps

1. ~~Pull request into `master`; Krogon merges.~~ Done (PR #106).
2. ~~Coordinator: versions, `DECISIONS.md`, `PLAYTEST.md` 2j step 10, the phone page.~~ Done 2026-11-29.
3. Next classes round (#41): Jab and Impale, and a new skill in Poison Javelin's place (Krogon, 2026-11-29: "too similar to plague javelin").

## Checks

- Build: PASS (821 tests; CraftingOverhaul and Orlog build, 0 errors). Corruption not changed.
- Version: gameplay changed: version bump needed at merge (Orlog, CraftingOverhaul). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested. No new setting or message; the ghost prefabs are registered through Jötunn on every game (every player has Orlog); damage goes through the game's own hits; tables sync through the version bump; CraftingOverhaul without Orlog shows no classes UI, as before.
- Assets and names: PASS (`GungnirsThrow.rgba` and the CSVs embedded by wildcard; `StaffLightning`, `SpearChitin` in the game dump; no new `$` tokens).
- FEATURES.md: PASS (section, table row, changelog).
- In-game: PASSED in Krogon's single-player play-tests (round 1, round 2, `fin-1`); two players untested.

## Merge notes

**Ready to merge: yes** (final play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog, CraftingOverhaul.** No other remote branch has unmerged commits. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/class-design.md`, `docs/mod-compatibility.md`.
