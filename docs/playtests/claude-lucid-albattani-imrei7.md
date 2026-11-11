# Play-tests: `claude/lucid-albattani-imrei7`

## Round: the Amazon's passives (2026-11-11)

**Build:** the three DLLs from this branch. The log line should read `Orlog 0.2.0 loaded (build: claude/lucid-albattani-imrei7 <commit>)` (the version is raised when it merges).
**Setup:** single player, a test world, the group's mods loaded. F5, `devcommands`. **Don't use `god`** for steps 5–11: whether your health drops is the test. `orlog_class Amazon` if your character has no class, then `orlog_level 20`.

`orlog_passive crit 100` (also `penetrate`, `dodge`, `avoid`, `evade`; any chance 0–100) makes a passive trigger for sure until you quit; `orlog_passive off` goes back to your real chances; `orlog_passive` alone lists them. With `devcommands` on, every passive that triggers also says so at the top left.

### Checklist

**Tooltips**
1. [ ] `orlog_skill CriticalStrike 2`, then open the skills panel and point at Critical Strike → "25% chance to deal double physical damage", and the next level "Level 3: 32% chance …". Point at Dodge (no points) → "Not learned yet …", next "Level 1: 18% chance to dodge a close-range hit while standing still or attacking".
2. [ ] Type `orlog_passive` → crit 25%, the others 0%.

**Critical Strike**

3. [ ] `spawn Boar 3`. Hit one with a club or sword a few times and note the damage numbers. Then `orlog_passive crit 100` and hit another → about **twice** the numbers, the game's critical-hit effect on each hit, and "Critical Strike" at the top left. `orlog_passive crit 0`: back to normal.
4. [ ] With `orlog_passive crit 100`, crouch and sneak up behind a boar that hasn't seen you, and hit it with a bow → the game's sneak-attack effect and a much bigger hit than step 3 (about 6× a normal hit).

**Penetrate**

5. [ ] Try your weapons on a few creatures until a hit shows **grey** numbers (grey means the creature resists it). Then `orlog_passive penetrate 100` and hit it the same way → **white**, bigger numbers. `orlog_passive penetrate 0`: grey again.

**Dodge, Avoid, Evade** (`orlog_passive off` first, then only the one each step names)

6. [ ] **Dodge:** `orlog_passive dodge 100`. `spawn Greyling`, stand still and let it hit you → "Dodge" floats up, a whoosh, no damage. Swing at it while it attacks → still "Dodge". Walk around while it hits you → you take damage.
7. [ ] **Avoid:** `orlog_passive avoid 100` (dodge back to 0: `orlog_passive dodge 0`). `spawn Greydwarf`, stand still at a distance until it throws rocks → "Avoid", no damage. Let it hit you up close → you take damage.
8. [ ] **Evade:** `orlog_passive off`, then `orlog_passive evade 100`. Keep walking or running while greydwarfs hit and throw at you → "Evade", no damage. Stand still → you take damage.
9. [ ] **Blocking:** `orlog_passive dodge 100`, shield in hand. Hold block facing a greyling → the game's normal block ("Blocked"), no "Dodge"; a parry still parries. Turn your back to it while holding block → "Dodge".
10. [ ] **Unavoidable:** `orlog_passive dodge 100`, `orlog_passive avoid 100`, `orlog_passive evade 100`. Stand in a campfire, fall from a height → you take damage. Optional, in the Black Forest: `spawn Troll`: its punches are dodged, its **ground slam still hurts**. Optional, Mountains: `spawn Dragon`: Moder's **breath still hurts**.
11. [ ] **Real chances:** `orlog_passive off`. Put points in Dodge (`orlog_skill Dodge 5`) and fight a few greydwarfs standing still → now and then a "Dodge", not every time.

**Log**

12. [ ] F5 shows no red errors; `BepInEx/LogOutput.log` has no "Orlog could not …" lines.

**With another player** (group evening: `PLAYTEST.md` → Session 2, 2j): Penetrate when the other player's game runs the creature, and a ground slam that still hurts when the other player's game runs the troll.

### Your feedback

(none yet)
