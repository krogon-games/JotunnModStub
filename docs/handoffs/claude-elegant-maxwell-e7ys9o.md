# Handoff: `claude/elegant-maxwell-e7ys9o`

**Written:** 2026-11-17, at the start of the round. Restarted from `master` (`a7e0764`) after the coordinator's PR #75 merged; 0 behind. No pull request yet.

## Stage

**Waiting for Krogon's playtest.**

## Purpose

Remake Strafe as a Diablo 3 Rapid Fire-style skill: hold the skill button and the bow keeps firing at where you aim, instead of Diablo 2's volley at several enemies (Multishot already covers that). Earlier in this session, on the same branch: the coordinator's merge of PR #74 and its paperwork (PR #75, merged).

## Done

Nothing confirmed in game yet this round.

## Half-done and known issues

- **Built, untested:** Strafe as rapid fire, with bows and (decisions 15–18) crossbows (`Orlog/Content/Strafe.cs`; numbers in `amazon_skill_values.csv`; rules `StreamDue`, `StreamTakesAmmo`, tested; 595 tests). Written with the game's 1.0.14 code open (decompiled on Krogon's word, "yes, decompile the game's code for this"; the first try was refused by the session's safety check, and the tool needed .NET roll-forward). New patch: `Humanoid.StartAttack` prefix.
- Claude's choices, open to change: each arrow's push and Bows-skill training shrink with its share; the bow wears once a second (with the quiver's arrow); let go before full draw = one normal arrow, no eitr (as Wind Lance); a crossbow's stream doesn't recoil each bolt; a crossbow's stamina drain is the game's bows' draw drain (the most common value), held-bow rules; the bow stays in its drawn pose while the arrows leave it.
- Rounding (Claude's slip in decision 14's question: "10 → 15 (+0.25 a level)" doesn't add up; 10 + 0.25 × 19 = 14.75): the table follows the endpoints, in whole numbers: start 10, 10, 11 … 15; drain 3 (levels 1–5), 4 (6–15), 5 (16–20).
- Not tried with two players (`PLAYTEST.md` → Session 2, 2j step 8, updated).

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
19. **2026-11-17: first play-test of the rapid fire: "absolutely loving it"** (with his log, build `afcb420`). Only `rf-1` is marked on the phone page; the log shows bow and crossbow streams at the agreed rates. Whether the rest of the checklist passed: asked.

## Playtest

`docs/playtests/claude-elegant-maxwell-e7ys9o.md`: round "Strafe as rapid fire", 15 steps, also on the phone page (`rf-1` … `rf-15`). Waiting for Krogon.

## Open questions for Krogon

2. Carried over: should the burning ground's and the cloud's hits count for Corruption's on-hit affixes? Should large roots stop a Wind Lance?

## Next steps

1. Krogon's play-test; fix what he finds.
2. `/handoff`, then the pull request.
3. Next classes round: Snipe and Shackle Shot (#70).

## Checks

Not run yet.

## Merge notes

Not ready. Gameplay will change: version bump needed at merge (Orlog).
