# Handoff: `claude/optimistic-ramanujan-acdyf8`

**Written:** 2026-11-28, at the start of the round. Started from `master` (`3d8a80b`); no pull request yet.

## Stage

Waiting for Krogon's playtest.

## Purpose

C6 ([#41](https://github.com/krogon-games/JotunnModStub/issues/41)): Gungnir's Throw (the basic ghost javelin throw, no skill points), then the spear and javelin skills Krogon picks for this round.

## Done

Nothing yet.

## Half-done and known issues

Built 2026-11-28, **not play-tested** (821 tests pass; Orlog and CraftingOverhaul build). Detail: `FEATURES.md` → Gungnir's Throw and the javelin skills.
- Gungnir's Throw (Basic card above the tabs, no points), Lightning Bolt and Plague Javelin, all from right click with a spear; the ghost is three networked copies of each spear's throw projectile.
- New patches: `Projectile.Setup` and `Projectile.OnHit` postfixes; `Player.SetControls` prefix and `Humanoid.StartAttack` postfix extended to the spear's throw.
- Claude's choices, open to change: holding right click keeps throwing (as the game's throw); the cloud poisons once a second from 1 s after landing; the Basic card's look; if eitr runs short as the throw starts, it flies as Gungnir's Throw.
- Limits: the cloud and ring run on the thrower's game only (as Venom Strike's); untested with two players. Jab, Impale and Poison Javelin still do nothing (#41 stays open).

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

## Playtest

Round 1, 18 steps: `docs/playtests/claude-optimistic-ramanujan-acdyf8.md`, also on the phone page (`gt-1` … `gt-8`, `lb-1` … `lb-4`, `pj-1` … `pj-4`, `c6-1`, `c6-2`). Waiting for Krogon.

## Open questions for Krogon

1. None open.

## Next steps

1. Krogon's play-test (phone page); fix what it finds; final play-test.
2. `/handoff`, then a pull request into `master` (gameplay changed: Orlog and CraftingOverhaul).
3. Later rounds for #41: Jab, Impale, Poison Javelin.

## Checks

Not run yet (`/handoff` runs them).

## Merge notes

Not ready: nothing built yet.
