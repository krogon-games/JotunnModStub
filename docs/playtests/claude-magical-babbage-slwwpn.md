# Play-tests: `claude/magical-babbage-slwwpn`

## Round: skill damage scales off the weapon (2026-11-16)

**Build:** the three DLLs from this branch (only Orlog changed). The log line should read `Orlog 0.5.0 loaded (build: claude/magical-babbage-slwwpn <commit>)` (the version is raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`, `god` helps. `orlog_class Amazon` if the character has no class, then `orlog_level 30` (for the eitr and the points). `raiseskill Bows 100`, so every shot does its full damage and the numbers below are easy to compare. The starter bow: `spawn Bow`, `spawn ArrowWood 100`. For step 5 also `spawn BowDraugrFang` and `spawn ArrowNeedle 100`.

**What changed:** Freezing Arrow's frost blast and Forked Lightning's forks no longer use fixed Diablo 2 numbers. Each deals a share of **your shot's own damage** (bow + arrow): the blast 50% at skill level 1 and 150% at 20, a fork 25% at skill level 1 and 100% at 20. With the starter bow and a wood arrow a full shot is about 40 (the game varies it a little), so a level-1 fork should be about 10 and a level-1 blast about 20. Nothing else changed: Multishot, Wind Lance and Inner Sight work as before.

This round was written without the game's code open. The thing most likely to be off is the size of the numbers (steps 3, 6 and 8). If one looks wrong, tell me what numbers you saw and send `BepInEx/LogOutput.log`: every forked shot and every blast now writes a line there with its sums.

**Phone page:** the same 12 steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: skill damage scales off the weapon" (step ids `r72-1` … `r72-12`).

### Checklist

**The cards**

1. [x] (`r72-1`) Press `K`, Bow and Crossbow tab. Click **Forked Lightning** → its details read "Lightning strikes the enemy the arrow hits and forks to up to 3 other enemies within 4 m: each strike deals 25% of the shot's damage as lightning". Click **Freezing Arrow** → "Explodes on impact: frost damage equal to 50% of the shot's damage within 3 m, freezing for 2 s (bosses are only slowed)". No big fixed damage numbers anywhere.
2. [x] (`r72-2`) Learn one rank of each (`orlog_respec` first if your points are spent), drag Forked Lightning onto slot F1 and Freezing Arrow onto F2.

**Forked Lightning**

3. [x] (`r72-3`) Your test from last time: `spawn Greyling 5 5` (five 4-star greylings, 100 health each). Starter bow and wood arrows, press F1, hold right click to a full draw and shoot one → the one you hit takes the arrow (about 40) and one small lightning number (about 10); three others next to it take about 10 lightning each. **None of them is close to dying from the lightning.**
4. [x] (`r72-4`) Shoot again without drawing fully (a quick tap of right click) → a weak arrow and weaker forks: the lightning numbers shrink with the arrow's.
5. [x] (`r72-5`) Swap to the Draugr fang bow with needle arrows and shoot a fresh group (`spawn Greydwarf_Elite 4`) at full draw → the arrow hits much harder, and so does each fork (still about a quarter of the arrow's number). A better bow makes the skill better.
6. [x] (`r72-6`) Rank 20, the step left over from last round (`r16-8`): `orlog_skill ForkedLightning 19` → the details say "up to 7 other enemies", "100% of the shot's damage" and cost 8.75 eitr. `spawn Greydwarf_Elite 9`, shoot one in the middle → the one you hit and up to seven of the others within reach are struck (never more than seven others), each fork about as hard as the arrow itself.
7. [x] (`r72-7`) The other step left over from last round (`r16-7`): a troll with greylings at its feet (`spawn Troll 1`, `spawn Greyling 3`), shoot the troll's body → the greylings touching or beside it are struck (the 4 m counts from its body, not its feet).

**Freezing Arrow**

8. [x] (`r72-8`) Starter bow and wood arrows again. `spawn Greydwarf_Elite 4` (brutes, 150 health), press F2 and shoot one at full draw → the one you hit takes the arrow (about 40); it and every brute within 3 m also take a frost number about half the arrow's (about 20) and stand frozen for 2 seconds. Nobody dies from one shot. Eitr drops by 9.
9. [x] (`r72-9`) `orlog_skill FreezingArrow 19` (rank 20) → the details say "150% of the shot's damage"; shoot a fresh group → the frost number is about one and a half times the arrow's (about 60 with the starter bow).

**The rest still works**

10. [x] (`r72-10`) Put Multishot and Wind Lance on slots and fire each once → Multishot fans out its arrows, a charged Wind Lance flies through a group, both as before.
11. [x] (`r72-11`) No red error lines in the console (F5) while doing the steps above.

**Your verdict**

12. [x] (`r72-12`) How do the two skills feel now: at skill level 1 (steps 3 and 8), and at skill level 20 (steps 6 and 9)? Too weak, too strong, or about right? If a number should change, say which way and I'll change the table.

### Your feedback (2026-11-17)

- Steps 1–12 (`r72-1` … `r72-12`): **all Pass** on the phone page (marked 2026-11-17, no notes). In chat: "testing has passed."
- Step 12, his verdict on how the two skills feel at skill level 1 and 20: Pass, no note.

### What changed after the feedback
- Nothing: no step failed.

### Final play-test (2026-11-17)
- Passed (single player). Forked Lightning's two steps left over from PR #71 (`r72-6`, `r72-7`) passed with it. Two players: not tried.

## Round: Strafe, Immolation Arrow and Venom Strike (2026-11-16)

**Build:** the three DLLs from this branch (only Orlog changed). The log line should read `Orlog 0.5.0 loaded (build: claude/magical-babbage-slwwpn <commit>)` (the version is raised when it merges).
**Setup:** the same as the first round: single player, a test world, the group's mods loaded. F5, `devcommands`, `god` helps. `orlog_class Amazon` if the character has no class, then `orlog_level 30`. `raiseskill Bows 100`. The starter bow: `spawn Bow`, `spawn ArrowWood 100`. The two rounds can be done in one sitting.

**What's new:** three more bow skills. **Strafe:** one arrow becomes a volley, each arrow flying straight at a different enemy you can see within 16.5 m, at 75% of the shot's damage. **Immolation Arrow:** explodes where it hits (fire, 25% of the shot's damage at level 1, 100% at 20) and the ground burns for 4 seconds (10% a second at level 1, 30% at 20). **Venom Strike:** a poison cloud hangs where it lands for 4 seconds and poisons everything in it (25% of the shot's damage at level 1, 100% at 20). Eitr: 11, 6 + 0.25 per level, 7 + 0.5 per level. The ring on the ground and the glow are a first version with no new art; tell me how they look. This was written without the game's code open, so the numbers and the aim are the likeliest things to be off: send `BepInEx/LogOutput.log` (every volley, blast and area writes a line with its numbers).

**Phone page:** the same 14 steps are the second section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: Strafe, Immolation Arrow and Venom Strike" (step ids `r70-1` … `r70-14`).

### Checklist

**The cards**

1. [x] (`r70-1`) Press `K`, Bow and Crossbow tab: Strafe, Immolation Arrow and Venom Strike no longer say "Coming later" (Snipe and Shackle Shot still do). Their details read: Strafe "Fires a volley for one arrow: up to 5 enemies in view within 16.5 m each get an arrow flying straight at them, at 75% damage; at least 2 arrows, shared out when there are fewer enemies" (11 eitr); Immolation Arrow "Explodes on impact: fire damage equal to 25% of the shot's damage within 3 m; the ground there burns for 4 s, dealing 10% of the shot's damage each second to enemies in it" (6 eitr); Venom Strike "Bursts into a poison cloud reaching 3 m around where it lands for 4 s: everything in it is poisoned for 25% of the shot's damage, again each second it stays" (7 eitr).
2. [x] (`r70-2`) Learn one rank of each and drag them onto slots.

**Strafe**

3. [x] (`r70-3`) `spawn Greydwarf_Elite 4`, back off about 10 m so all four are on screen, press Strafe's slot key, hold right click to a full draw and release → four arrows leave at once, one flying straight at each brute. Each hit is a little weaker than a normal arrow (about 30 with the starter bow). One arrow leaves the quiver. Eitr drops by 11.
4. [x] (`r70-4`) One enemy alone → two arrows, both at it.
5. [x] (`r70-5`) With no enemy on screen (or all further than about 16 m) → two arrows fly where you aim, like normal arrows.
6. [x] (`r70-6`) With enemies in front of you and behind you → only the ones you can see on screen get an arrow.
7. [x] (`r70-7`) `orlog_skill Strafe 19` (rank 20) → the details say "up to 10 enemies" and "at least 7 arrows". `spawn Greyling 12` in front of you → ten of them get an arrow each. A troll alone (`spawn Troll 1`) takes seven arrows.

**Immolation Arrow**

8. [x] (`r70-8`) `spawn Greydwarf_Elite 4`, shoot one → the arrow hits as usual and there is a burst of fire. Every brute within 3 m takes a fire number about a quarter of the arrow's (about 10 with the starter bow) and burns. Then for 4 seconds an orange ring and glow mark the burning ground and the brutes standing in it take a small fire hit each second (about 4). One that walks out stops being hit. Eitr drops by 6.
9. [x] (`r70-9`) Shoot the ground between a group → the blast and the burning ground appear there.
10. [x] (`r70-10`) `orlog_skill ImmolationArrow 19` → the details say 100% and 30%. The blast is about as hard as the arrow itself.

**Venom Strike**

11. [x] (`r70-11`) `spawn Greydwarf_Elite 4`, shoot one (or the ground between them) → the arrow hits as usual. A green ring and glow hang there for 4 seconds. Everything inside is poisoned (green numbers ticking down, about 10 in all with the starter bow). A creature that walks in during the 4 seconds is poisoned too. Eitr drops by 7.
12. [x] (`r70-12`) `orlog_skill VenomStrike 19` → the details say 100% and cost 16.5 eitr. The poison is clearly stronger.

**Safety and your verdict**

13. [x] (`r70-13`) Stand in your own burning ground and poison cloud → you take no damage (a tamed animal neither).
14. [x] (`r70-14`) Your verdict: do the three skills feel right at skill level 1 and at 20, and how do the ring, the glow and the effect each second look and sound? No red error lines in the console (F5). Send `BepInEx/LogOutput.log` if anything looks off.

### Your feedback (2026-11-17)

- Steps 1–14 (`r70-1` … `r70-14`): **all Pass** on the phone page (marked 2026-11-17, no notes). In chat: "testing has passed."
- Step 14, his verdict on the feel and on the ring, glow and effects: Pass, no note. No log was sent.

### What changed after the feedback
- Nothing: no step failed. (Before his play-test, a code review led to three fixes: Strafe skips enemies behind the bow; the burning ground lies on the ground under the hit; a small arrow impact each second instead of an explosion. `FEATURES.md` → changelog, 2026-11-17.)

### Final play-test (2026-11-17)
- Passed (single player). Left open by the code review and not looked at separately (no log): the game's poison not adding up in the cloud, fire arriving as burning, Corruption's on-hit affixes on area hits. Two players: `PLAYTEST.md` → Session 2, 2j step 8.
