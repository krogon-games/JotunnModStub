# Handoff: `claude/elegant-maxwell-e7ys9o`

**Written:** 2026-11-17, at the start of the round. Restarted from `master` (`a7e0764`) after the coordinator's PR #75 merged; 0 behind. No pull request yet.

## Stage

**Building.**

## Purpose

Remake Strafe as a Diablo 3 Rapid Fire-style skill: hold the skill button and the bow keeps firing at where you aim, instead of Diablo 2's volley at several enemies (Multishot already covers that). Earlier in this session, on the same branch: the coordinator's merge of PR #74 and its paperwork (PR #75, merged).

## Done

Nothing built yet this round.

## Half-done and known issues

None yet.

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
14. **2026-11-17: both parts of the cost grow a little** ("Both grow a little"): the start 10 at skill level 1 → 15 at 20 (+0.25 a level, Immolation Arrow's pace), the drain 3 → 5 eitr a second; over only the drain growing and only the start growing.
13. **2026-11-17: the arrows fly like normal arrows** ("Like normal arrows"): normal flight and drop, as from a fully drawn shot; over straight with a little spread and dead straight.

## Playtest

Nothing to test yet.

## Open questions for Krogon

None for now.

## Next steps

1. Build the new Strafe; tests; docs; play-test checklist on the phone page.

## Checks

Not run yet.

## Merge notes

Not ready. Gameplay will change: version bump needed at merge (Orlog).
