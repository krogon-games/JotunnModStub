# Handoff: `claude/elegant-maxwell-e7ys9o`

**Written:** 2026-11-17 by `/handoff`. Restarted from `master` (`a7e0764`) after the coordinator's PR #75; 13 commits ahead with this one, 0 behind. No pull request yet.

## Stage

**Ready to merge.**

## Purpose

Strafe remade as Diablo 3's Rapid Fire (Krogon's idea), replacing Diablo 2's volley. Earlier on this branch: the coordinator's PR #75 (merged).

## Done

Krogon's single-player play-test (2026-11-17, build `afcb420`, Valheim 1.0.21): "absolutely loving it", then `/handoff` (decision 20). His log shows the agreed rates: a bow streams 6 arrows a second from full draw, a loaded crossbow 3 bolts; one from the quiver a second; 25% → 50% of the shot each; 10 → 15 eitr to start, 3 → 5 a second.

## Half-done and known issues

- Built on the game's 1.0.14 code (decompiled on Krogon's word). New patch: `Humanoid.StartAttack` prefix (`Orlog/Content/Strafe.cs`).
- Not marked one by one, only covered by decision 20: running out of eitr, arrows or stamina; blocking; the crossbow while reloading; rank 20; the damage numbers.
- Claude's choices, open to change: push and skill training shrink with each arrow's share; the bow wears once a second; let go early = a normal arrow, no eitr; no recoil per crossbow bolt; a crossbow drains the bows' held-draw stamina.
- Rounding (Claude's slip in decision 14's question): the table follows its endpoints in whole numbers: start 10 … 15, drain 3, 4 (from level 6), 5 (from 16).
- Not tried with two players (`PLAYTEST.md` → Session 2, 2j step 8).

## Decisions

1. **2026-11-17: Strafe is remade after Diablo 3's Rapid Fire** (Krogon's own idea): "I had a very different idea of how i actually wanted it to be implemented… I was thinking more of a demon hunter skill from diablo 3 [Rapid Fire]. Cost: 10 Eitr initially, and an additional 3 Eitr while channeling. Rapidly fire for 25% weapon damage as Physical. More of a machine gun skill at the target location, instead of shooting at multiple targets at once. We already have multishot." **Replaces** the volley built in PR #74 (`claude-magical-babbage-slwwpn.md` decision 8).
2. **2026-11-17: this round is Strafe only, on this branch** ("Yes, Strafe only"); Snipe and Shackle Shot wait for the next round.
3. **2026-11-17: the name stays "Strafe"** ("Keep \"Strafe\""), with the same id, so spent points and slots stay.
4. **2026-11-17: the bow draws once as usual, then rapid-fires while held** ("Draw once, then rapid fire"), over firing at once with no draw.
5. **2026-11-17: the channel drains 3 eitr per second** ("3 eitr per second"), on top of 10 eitr to start; over 3 eitr per arrow.
6. **2026-11-17: 6 arrows a second** while firing ("6 a second"), over 4 and 8.
7. **2026-11-17: each arrow deals 25% of the shot's damage at skill level 1, 50% at 20** ("25% → 50% at level 20"), growing evenly; over 25% at every level and 25% → 75%.
8. **2026-11-17: the quiver loses one arrow per second of firing** ("One per second"), not one per arrow, nor only the first.
9. **2026-11-17: the arrows carry the arrow's own damage** ("The arrow's own damage"): fire, frost and poison arrows keep their element, as with the other bow skills; over physical only.
10. **2026-11-17: you walk slowly while firing** ("Walk slowly"), at the game's slowed pace for a drawn bow; over standing still and moving freely.
11. **2026-11-17: stamina drains while firing, like holding a drawn bow** ("Like a drawn bow"), on top of the eitr; over no stamina.
12. **2026-11-17: the eitr cost gets more expensive per level** (his own answer: "get more expensive per level."), over the same at every level and getting cheaper. How much: decision 14.
13. **2026-11-17: the arrows fly like normal arrows** ("Like normal arrows"): normal flight and drop, as from a fully drawn shot; over straight with a little spread and dead straight.
14. **2026-11-17: both parts of the cost grow a little** ("Both grow a little"): the start 10 at skill level 1 → 15 at 20 (+0.25 a level, Immolation Arrow's pace), the drain 3 → 5 eitr a second; over only the drain growing and only the start growing.
15. **2026-11-17: Strafe works with crossbows too** ("Crossbows as well"), not bows only (Claude had built it for bows only and asked). How: decisions 16–18.
16. **2026-11-17: a crossbow streams 3 bolts a second, each the same share as a bow's arrow** ("Fewer, same share": 25% → 50% of a normal bolt), over the bow's 6 a second and over 6 a second at half the share. With a crossbow the stream starts as soon as the button is held with a loaded crossbow (no draw), as put in the question.
17. **2026-11-17: a streaming crossbow slows your walk and drains stamina like the bow** ("Like the bow"), over normal walking with eitr only.
18. **2026-11-17: when the crossbow stream stops the crossbow is empty and reloads** ("Empty, reloads"), as after any crossbow shot; over staying loaded.
19. **2026-11-17: first play-test of the rapid fire: "absolutely loving it"** (with his log, build `afcb420`). Only `rf-1` is marked on the phone page; the log shows bow and crossbow streams at the agreed rates. Whether the rest of the checklist passed: asked (decision 20).
20. **2026-11-17: the round passed; end the session.** Asked whether the steps the log doesn't show worked ("If they're fine, type `/handoff`"), Krogon typed `/handoff`. Taken as his word that the round passed; those steps weren't marked one by one.

## Playtest

`docs/playtests/claude-elegant-maxwell-e7ys9o.md`: "Strafe as rapid fire", 15 steps: **passed** (decisions 19–20). Its steps are off the phone page; the two-player step stays (`mpe-2j-8`).

## Open questions for Krogon

1. Carried over: should the burning ground's and the cloud's hits count for Corruption's on-hit affixes? Should large roots stop a Wind Lance?

## Next steps

1. Pull request into `master`; the coordinator raises Orlog's version.
2. Coordinator: `STATUS.md`, `DECISIONS.md` (decisions 1–20; decision 1 replaces `claude-magical-babbage-slwwpn.md` decision 8), update #70.
3. Next classes round: Snipe and Shackle Shot (#70).

## Checks

- Build: PASS (595 tests; Orlog, Corruption, CraftingOverhaul build, 0 errors).
- Version: gameplay changed: version bump needed at merge (Orlog). The branch changed no version.
- Multiplayer sync: PASS, read in code, not tested (nothing new to sync; the arrows are the game's own).
- Assets and names: PASS (CSVs embedded by wildcard; nothing new).
- FEATURES.md: PASS.
- In-game: PASSED in Krogon's single-player play-test ("absolutely loving it"; decision 20); two players untested.

## Merge notes

**Ready to merge: yes** (play-test passed, no check failed, level with `master`, pushed). **Gameplay changed: version bump needed at merge: Orlog.** The only other remote branch, `claude/magical-babbage-slwwpn`, is fully in `master`. Shared docs touched (add-only): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/PLAYTEST.md`, `docs/ARCHITECTURE.md`, `docs/class-design.md`, `docs/mod-compatibility.md`.
