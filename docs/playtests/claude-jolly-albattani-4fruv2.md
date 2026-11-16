# Play-tests: `claude/jolly-albattani-4fruv2`

## Round: Forked Lightning and Wind Lance (2026-11-16)

**Build:** the three DLLs from this branch (only Orlog changed). The log line should read `Orlog 0.4.0 loaded (build: claude/jolly-albattani-4fruv2 <commit>)` (the version is raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`, `god` helps. `orlog_class Amazon` if the character has no class, then `orlog_level 30` (about 58 eitr). A bow and arrows (`spawn BowFineWood`, `spawn ArrowFlint 100`); for step 17 also `spawn ArrowFire 20`, for step 18 `spawn CrossbowArbalest` and `spawn BoltBone 50`.

Both skills were written without the game's code open this time, so steps 9 (the 3-second hold) and 11 (the lance flying on through enemies) are the ones most likely to show a problem. If one fails, tell me what you saw and send `BepInEx/LogOutput.log`.

**Phone page:** the same 21 steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: Forked Lightning and Wind Lance" (step ids `r16-1` … `r16-21`).

### Checklist

**The cards**

1. [ ] Press `K`, Bow and Crossbow tab → **Forked Lightning** and **Wind Lance** no longer say "Coming later" (five others still do: Snipe, Strafe, Immolation Arrow, Venom Strike, Shackle Shot).
2. [ ] Click Forked Lightning → its details read "Lightning strikes the enemy the arrow hits and forks to up to 3 other enemies within 4 m: … lightning damage each", cost 4 eitr. Click Wind Lance → "Hold 3 s past full draw, then release: a lance of wind at 100% damage flies 50 m dead straight at 2× an arrow's speed, through every enemy, knocking them back (force 100). Released early: a normal shot, no eitr", cost 8 eitr.
3. [ ] Learn one rank of each and drag Forked Lightning onto slot F1 and Wind Lance onto F2.

**Forked Lightning**

4. [ ] `spawn Greydwarf_Elite 4` (brutes: they survive a few hits) so they stand close together. Bow in hand, press F1, hold right click and shoot one of them → the arrow hits as usual; lightning bursts on that brute, and thin jagged lines of light jump from it to up to three others next to it, each with its own burst and damage number. Eitr drops by 4.
5. [ ] One enemy alone (`spawn Greydwarf_Elite 1`, or a troll) → the arrow's damage and one lightning hit on it (two damage numbers), no lines.
6. [ ] Shoot the ground or a tree beside a group → nothing extra happens (the eitr is still spent). A creature standing well away from the one you hit (more than two floor tiles, 4 m) is not struck.
7. [ ] A troll with greylings at its feet (`spawn Troll 1`, `spawn Greyling 3`): shoot the troll's body → the greylings touching or beside it are struck (the 4 m counts from its body, not its feet).
8. [ ] `orlog_skill ForkedLightning 19` (rank 20) → the details say "up to 7 other enemies" and cost 8.75 eitr; `spawn Greydwarf_Elite 9`, shoot one in the middle → the one you hit and up to seven of the others within reach are struck (never more than seven others).

**Wind Lance**

9. [ ] Press F2. Hold right click: the bow draws as usual. **Keep holding for 3 seconds after it is fully drawn** → a ring sounds and a pale light appears at your bow hand. Release → a whoosh, and a large pale arrow flies very fast and dead straight. Eitr drops by 8.
10. [ ] Hold right click, but release before the ring → a normal arrow, and no eitr is used.
11. [ ] `spawn Greydwarf 4`, get them roughly in a line (or let them run at you in a row) and fire a charged lance through them → **every greydwarf in its path is hit and thrown back**, and the lance flies on behind them.
12. [ ] Aim straight at something far away on level ground (about 40 m) without aiming above it → the lance hits where you aimed (no drop). Fire one out over open water or into the sky → it vanishes after about 50 m and doesn't land far away.
13. [ ] Fire a lance into a tree, a rock and the ground → it stops there like an arrow.
14. [ ] A lance on a troll → it is hit but barely moves (like a sledge hit); a boar or greyling flies back several metres.
15. [ ] Stamina: holding the charge drains stamina the way holding a drawn bow does. With low stamina the game drops the draw before the ring (no shot). **Your verdict:** does 3 seconds feel right with the stamina it costs?
16. [ ] `orlog_level 50` (for the points), then `orlog_skill WindLance 19` (rank 20) → the details say 233% damage, force 200, cost 3.25 eitr; the lance clearly hits and pushes harder than at rank 1.
17. [ ] With fire arrows in the quiver, a lance still sets what it hits on fire.
18. [ ] Crossbow in hand (loaded), F2: hold right click → nothing fires while you hold; after 3 seconds the ring sounds; release → the bolt flies as a lance. Released before the ring → a normal bolt, no eitr.

**Your verdict on the feel**

19. [ ] The look and sound: the lance (a big pale arrow with a trail), the "ready" ring, the whoosh, the lightning lines and bursts. What should change? A screenshot or clip of anything that looks off helps.

**The older skills**

20. [ ] Multishot and Freezing Arrow still work from their slots, and switching between the four skills with F1–F4 changes what right click fires. A normal left-click shot is still a normal arrow.

**Log**

21. [ ] F5 shows no red errors; `BepInEx/LogOutput.log` has no "Orlog could not …" lines.

### Your feedback (2026-11-16)

Build tested: `Orlog 0.4.0 loaded (build: claude/jolly-albattani-4fruv2 b7f9297)` (the log you sent; it is from a later two-minute start, so it holds no lines from the test itself, and no errors).

- Steps 1–6 (the cards; Forked Lightning on a group, alone, on a miss): **passed** (phone page).
- **Forked Lightning:** "The forked lightning works well it seems, i tested it before windlance. Altough it does way too much damage at level 1. I spawned 5 level 5 greylings and almost one shot all of them with 1 arrow. Thats way too strong. we can make a balancing pass another session, just letting you know it works but is too strong."
- **Wind Lance (steps 9, 11, 14, 19): failed.** "Windlance feels underwhelming in look and feel. Maybe because it didnt quite come through in the game as we imagined. It doesnt seem to penetrate targets, i was trying my best to line greylings up in a line and then shoot the first one and get the ones behind him too but only the first one got damaged. I think out of a dozen shots, only 1 of them pushed the enemy back." "when you release the charged shot, from the players perspective it just looks like a mostly normal arrow. It could have wind particles around the arrow like a tornado. This is a shot from elden ring, i would make it much tighter around the arrow. and make the arrow bigger in size." (picture: `docs/images/wind_lance_reference.webp`)
- "theres a few more steps to playtest but lets fix windlance first."

### What changed after the feedback

Krogon's answers the same day (handoff decisions 17–21): "I suspect it was my error for not holding the charge long enough. I tested again and it works fine now, the charge is LOOONG. lets reduce it to 1.5 seconds. Also we need an indicator of some kind. Just like the yellow circle narrows in normally when you do a charge on the bow, it should continue to narrow in and maybe turn a different colour?"; the ground: "Run along the ground"; the width: "0.75 m each side"; the knockback: "Keep 100 to 200, add stagger".

- **Why it didn't go through enemies** (read in the game's code): the lance was as thin as an arrow and dead straight, so, aimed down at a small creature, it struck the ground a step behind it and stopped. Now it flies itself: a gust 0.75 m wide each side that runs along the ground and water instead of stopping there (trees, rocks, buildings and steep hillsides still stop it).
- **Why it rarely pushed:** the game throws a creature a distance that grows with the square of the push, so rank 1's 100 is a quarter of a sledge's throw. The numbers stay (his choice); every enemy struck is now also staggered (not bosses).
- **The hold is 1.5 s** (was 3), and **the bow's draw circle shows it**: it keeps narrowing past full draw and turns pale blue; ready = bright and pulsing.
- **The look:** the arrow three times as large with a bright core, wind spiralling tightly round it, a wake along its path, a ring at the bow, a puff at each enemy (`docs/images/wind_lance_preview.png`).
- Found while reviewing against the game's code, before any test: a crossbow's charged shot would always have left as a plain bolt (fixed).
- Each lance now writes two lines to the log, for the next test.

## Round 2: Wind Lance reworked (2026-11-16)

**Build:** pull this branch again and rebuild. The log line should read `Orlog 0.4.0 loaded (build: claude/jolly-albattani-4fruv2 <commit>)` with a newer commit than `b7f9297`.
**Setup:** as round 1 (single player, a test world, `devcommands`, `god`; `orlog_class Amazon` if needed, `orlog_level 30`; `spawn BowFineWood`, `spawn ArrowFlint 100`, `spawn ArrowFire 20`). Wind Lance on slot F2, Forked Lightning on F1.
**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (the ids in brackets).

### Checklist

**Wind Lance: the charge and its circle**

1. [x] (`r16b-1`) Slot F2 (Wind Lance), bow in hand. Hold right click: the yellow circle narrows as usual. **Keep holding: the circle keeps narrowing and turns pale blue.** After 1.5 seconds it turns bright and pulses, a ring sounds, and wind swirls at your bow hand.
2. [x] (`r16b-2`) Let go while the circle is still narrowing (before it pulses): a normal arrow, and no eitr is used.
3. [x] (`r16b-3`) Let go once it pulses: the lance fires with a whoosh and eitr drops by 8. **Your verdict:** is 1.5 seconds right, with the stamina it costs? Is the circle clear enough?

**Wind Lance: the look**

4. [x] (`r16b-4`) **The look.** The arrow is about three times as large with a bright streak; wind spirals tightly round it; a corkscrew of wind hangs along its path for a moment after it has passed; a ring of wind opens at the bow. Is this the tornado you had in mind? What should change (size, tightness, brightness, how long the trail hangs)? A screenshot or clip helps a lot.

**Wind Lance: through enemies and along the ground**

5. [x] (`r16b-5`) `spawn Greyling 5`, let them run at you, aim at the nearest and fire a charged lance: **every one in its path, or within about a step either side of it, is hit, reels (staggers) and is pushed back**, with a puff of wind at each. You no longer need them in a perfect line.
6. [x] (`r16b-6`) On flat land, aim at the ground some metres in front of you and fire: the gust does not stop in the dirt; it levels out and runs along the ground (the trail shows it) for its 50 m.
7. [x] (`r16b-7`) From a shore, fire out across the water: it runs over the water. Up or down a gentle slope: it follows the ground.
8. [x] (`r16b-8`) It still stops at a tree, a big rock and a building wall, at a steep hillside or cliff, and when you shoot steeply into the ground at your feet.
9. [x] (`r16b-9`) Aim well above the horizon (at a bird or the sky): it flies straight and is gone after about 50 m.
10. [x] (`r16b-10`) A troll: it is hit and reels, but barely moves. A boar or greyling is thrown back. (If you have a boss handy: it is hit but does not reel.)

**Wind Lance: the rest, and the log**

11. [x] (`r16b-11`) `orlog_level 50`, then `orlog_skill WindLance 19` (rank 20): the details say 233% damage, force 200, cost 3.25 eitr; the gust clearly hits and pushes harder than at rank 1.
12. [x] (`r16b-12`) With fire arrows in the quiver, the gust still sets what it hits on fire.
13. [x] (`r16b-13`) Crossbow in hand (loaded; `spawn CrossbowArbalest`, `spawn BoltBone 50`), F2: hold right click: nothing fires while you hold, and a circle appears and narrows; after 1.5 seconds it pulses; release: the bolt leaves as a lance. Released before that: a normal bolt, no eitr.
14. [x] (`r16b-14`) Multishot, Freezing Arrow and Forked Lightning still work from their slots; switching with F1–F4 changes what right click fires; a normal left-click shot is a normal arrow.
15. [x] (`r16b-15`) F5 shows no red errors. **Before you start the game again**, send `BepInEx/LogOutput.log`: each lance now writes "Wind Lance fired …" and "Wind Lance flew N m, struck K enemies, ended at …", which tells me exactly what happened (the next start of the game overwrites the file).

**Forked Lightning: two steps left**

16. [ ] (`r16-7`) Left over from round 1. A troll with greylings at its feet (`spawn Troll 1`, `spawn Greyling 3`), slot F1 (Forked Lightning): shoot the troll's body: the greylings touching or beside it are struck (the 4 m counts from its body, not its feet).
17. [ ] (`r16-8`) Left over from round 1. `orlog_skill ForkedLightning 19` (rank 20): the details say "up to 7 other enemies" and cost 8.75 eitr; `spawn Greydwarf_Elite 9`, shoot one in the middle: the one you hit and up to seven of the others within reach are struck (never more than seven others).

### Your feedback (2026-11-16, evening)

Build tested: `Orlog 0.4.0 loaded (build: claude/jolly-albattani-4fruv2 9fc528b)`, Valheim 1.0.21 (the log he sent).

- **Wind Lance, steps 1–15 (`r16b-1` … `r16b-15`): all passed** on the phone page. His one note, on step 3 (is 1.5 seconds right, is the circle clear enough?): "yes". His message: "see playtest results and log."
- **Forked Lightning's two steps (`r16-7`, `r16-8`): not marked.**
- **The log:** no errors and no "Orlog could not …" lines. 13 lances, all at rank 20; 4 early releases, each a normal shot. The wind is drawn with `Sprites/Default`. A bow's lance flies at 100 m/s, a crossbow's at 400 m/s. Most flew their 50 m along the ground; one ended at a bush (`Bush01`) after 32 m, one at a rock (`Rock_7`) after 48 m. **Two ended at the ground at once ("flew 0 m, struck 2 enemies, ended at Terrain"; "flew 0 m, struck 1 enemy, ended at Terrain")**: with enemies right beside him he aimed down more steeply than the 38° the lance follows, so it struck the ground (as built), after hitting them.

### What changed after the feedback

Krogon's answers (handoff decisions 23–25): a steep downward shot: "Leave it as it is"; bushes: "Pass through bushes"; Forked Lightning's two steps: "Leave them for the balancing session".

- **Bushes and other small plants let the gust through, unharmed:** the world's vegetation that the game doesn't class as a tree and that drops only what trees drop (`Bush01`, `shrub_2`, …), anything that can be picked, and saplings and crops still growing. The log lists them once ("Wind Lance passes through N kinds of small plants: …"). Trees (young thin ones too), logs, stumps, rocks and buildings still stop it.

## Round 3: one check (2026-11-16)

**Build:** pull the branch again and rebuild (a newer commit than `9fc528b`).

1. [x] (`r16c-1`) Fire a charged Wind Lance through a bush (the low round Meadows bushes, a berry bush): it flies on behind it and the bush is unharmed. Fire one into a tree and a rock: it still stops. Send `BepInEx/LogOutput.log` before starting the game again (it now lists the plants the lance passes through).

### Final play-test

- Wind Lance: passed (2026-11-16, all 15 steps); the bush change after it has one step open (round 3). Forked Lightning: steps 1–6 passed; `r16-7` and `r16-8` go to its balancing session (Krogon's choice).

### Your feedback (2026-11-16, late evening)

Build tested: `Orlog 0.4.0 loaded (build: claude/jolly-albattani-4fruv2 00c7b57)`.

- `r16c-1`: **passed** (phone page; "done, passed.").
- **The log:** no errors. Three lances: one flew its 50 m along the ground; one struck an enemy and ended at a rock (`Rock_7`) after 32 m; one ended at a birch (`Birch1`) after 14 m. The list it wrote: "Wind Lance passes through 11 kinds of small plants …: BlueberryBush, Bush01, Bush01_deepnorth, Bush01_heath, Crow, Greydwarf_Root, HugeRoot1, LingonberryBush, RaspberryBush, shrub_2, shrub_2_heath." Three of those aren't small plants (a crow and two kinds of large root); the gust passes them harmlessly. Left as it is unless he wants it changed.

### Final play-test (the round)

- **Passed.** Wind Lance: 15 steps (build `9fc528b`) and the bush check (build `00c7b57`). Forked Lightning: steps 1–6; its last two steps and its balance go to a later session (his choice).
