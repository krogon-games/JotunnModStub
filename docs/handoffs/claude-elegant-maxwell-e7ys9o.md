# Handoff: `claude/elegant-maxwell-e7ys9o`

**Written:** 2026-11-17, at the start of the round. Restarted from `master` (`a7e0764`) after the coordinator's PR #75 merged; 0 behind. No pull request yet.

## Stage

**Asking Krogon questions.**

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

## Playtest

Nothing to test yet.

## Open questions for Krogon

Being asked: fire rate, damage growth with level, arrows used, moving while firing, the arrow's own damage type.

## Next steps

1. Finish the questions.
2. Build the new Strafe; tests; docs; play-test checklist on the phone page.

## Checks

Not run yet.

## Merge notes

Not ready. Gameplay will change: version bump needed at merge (Orlog).
