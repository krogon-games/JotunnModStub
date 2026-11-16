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

### Your feedback

(not tested yet)
