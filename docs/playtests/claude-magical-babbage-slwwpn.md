# Play-tests: `claude/magical-babbage-slwwpn`

## Round: skill damage scales off the weapon (2026-11-16)

**Build:** the three DLLs from this branch (only Orlog changed). The log line should read `Orlog 0.5.0 loaded (build: claude/magical-babbage-slwwpn <commit>)` (the version is raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`, `god` helps. `orlog_class Amazon` if the character has no class, then `orlog_level 30` (for the eitr and the points). `raiseskill Bows 100`, so every shot does its full damage and the numbers below are easy to compare. The starter bow: `spawn Bow`, `spawn ArrowWood 100`. For step 5 also `spawn BowDraugrFang` and `spawn ArrowNeedle 100`.

**What changed:** Freezing Arrow's frost blast and Forked Lightning's forks no longer use fixed Diablo 2 numbers. Each deals a share of **your shot's own damage** (bow + arrow): the blast 50% at skill level 1 and 150% at 20, a fork 25% at skill level 1 and 100% at 20. With the starter bow and a wood arrow a full shot is about 40 (the game varies it a little), so a level-1 fork should be about 10 and a level-1 blast about 20. Nothing else changed: Multishot, Wind Lance and Inner Sight work as before.

This round was written without the game's code open. The thing most likely to be off is the size of the numbers (steps 3, 6 and 8). If one looks wrong, tell me what numbers you saw and send `BepInEx/LogOutput.log`: every forked shot and every blast now writes a line there with its sums.

**Phone page:** the same 12 steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: skill damage scales off the weapon" (step ids `r72-1` … `r72-12`).

### Checklist

**The cards**

1. [ ] (`r72-1`) Press `K`, Bow and Crossbow tab. Click **Forked Lightning** → its details read "Lightning strikes the enemy the arrow hits and forks to up to 3 other enemies within 4 m: each strike deals 25% of the shot's damage as lightning". Click **Freezing Arrow** → "Explodes on impact: frost damage equal to 50% of the shot's damage within 3 m, freezing for 2 s (bosses are only slowed)". No big fixed damage numbers anywhere.
2. [ ] (`r72-2`) Learn one rank of each (`orlog_respec` first if your points are spent), drag Forked Lightning onto slot F1 and Freezing Arrow onto F2.

**Forked Lightning**

3. [ ] (`r72-3`) Your test from last time: `spawn Greyling 5 5` (five 4-star greylings, 100 health each). Starter bow and wood arrows, press F1, hold right click to a full draw and shoot one → the one you hit takes the arrow (about 40) and one small lightning number (about 10); three others next to it take about 10 lightning each. **None of them is close to dying from the lightning.**
4. [ ] (`r72-4`) Shoot again without drawing fully (a quick tap of right click) → a weak arrow and weaker forks: the lightning numbers shrink with the arrow's.
5. [ ] (`r72-5`) Swap to the Draugr fang bow with needle arrows and shoot a fresh group (`spawn Greydwarf_Elite 4`) at full draw → the arrow hits much harder, and so does each fork (still about a quarter of the arrow's number). A better bow makes the skill better.
6. [ ] (`r72-6`) Rank 20, the step left over from last round (`r16-8`): `orlog_skill ForkedLightning 19` → the details say "up to 7 other enemies", "100% of the shot's damage" and cost 8.75 eitr. `spawn Greydwarf_Elite 9`, shoot one in the middle → the one you hit and up to seven of the others within reach are struck (never more than seven others), each fork about as hard as the arrow itself.
7. [ ] (`r72-7`) The other step left over from last round (`r16-7`): a troll with greylings at its feet (`spawn Troll 1`, `spawn Greyling 3`), shoot the troll's body → the greylings touching or beside it are struck (the 4 m counts from its body, not its feet).

**Freezing Arrow**

8. [ ] (`r72-8`) Starter bow and wood arrows again. `spawn Greydwarf_Elite 4` (brutes, 150 health), press F2 and shoot one at full draw → the one you hit takes the arrow (about 40); it and every brute within 3 m also take a frost number about half the arrow's (about 20) and stand frozen for 2 seconds. Nobody dies from one shot. Eitr drops by 9.
9. [ ] (`r72-9`) `orlog_skill FreezingArrow 19` (rank 20) → the details say "150% of the shot's damage"; shoot a fresh group → the frost number is about one and a half times the arrow's (about 60 with the starter bow).

**The rest still works**

10. [ ] (`r72-10`) Put Multishot and Wind Lance on slots and fire each once → Multishot fans out its arrows, a charged Wind Lance flies through a group, both as before.
11. [ ] (`r72-11`) No red error lines in the console (F5) while doing the steps above.

**Your verdict**

12. [ ] (`r72-12`) How do the two skills feel now: at skill level 1 (steps 3 and 8), and at skill level 20 (steps 6 and 9)? Too weak, too strong, or about right? If a number should change, say which way and I'll change the table.

### Your feedback

(waiting)
