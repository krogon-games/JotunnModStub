# Play-test: `claude/festive-albattani-lg4p35`

## Round: gathering review fixes (2026-11-08)

**Build:** the build stamp in the log line `Corruption … loaded (build: claude/festive-albattani-lg4p35 …)`.
**Setup:** single player, `devcommands`. A Black Forest spot with a **copper deposit** (or a big copper rock that turns into one), **tin** on a shore, and some trees; a Mountains **silver vein** for step A4 (or `spawn silvervein`). Bring an Antler or Bronze pickaxe and a plain axe. `co_affix <id> <value>` adds an affix to the tool in your right hand: use a **fresh tool** for each affix (`spawn PickaxeBronze`, `spawn AxeBronze`) so they don't mix. Open the console log (F5) to read the lines quoted below.

### A. Seismic and Odin's Grace on rock and ore (alone, so your game runs the deposit)
1. [ ] Pickaxe with `co_affix seismic 100`. Hit one copper chunk until it breaks → on **every** swing the chunks touching it show damage numbers too, **including the swing that breaks it**. (Before the fix, that last swing hit nothing else.)
2. [ ] Same on a **tin** rock (a different kind of rock in the game's code) → same as step 1.
3. [ ] Fresh pickaxe with `co_affix odins_grace 100`. One swing at a copper chunk → that chunk **and every chunk touching it** break at once. Each broken chunk drops its ore once (no double piles). Try one swing on tin too.
4. [ ] Odin's Grace pickaxe (Antler or Bronze) on a **silver vein** → "Too hard", nothing breaks (the tool tier still applies).

### B. Veinbreaker's count
1. [ ] Fresh pickaxe with `co_affix veinbreakers 100`. Hit rock without stopping → swings 5, 10 and 15 show a clearly bigger damage number.
2. [ ] Hit 4 times, put the pickaxe away (hide-weapons key, R by default), take it out again → swings 1–4 normal again, the **5th** big.
3. [ ] Hit 4 times, switch to the plain axe, chop once, switch back → again the 5th swing after switching back is the big one.
4. [ ] Hit 4 times, eat something with the pickaxe in hand, carry on → the 5th swing after eating is the big one (you chose "any unequip resets").

### C. Only your swings get your tool's affixes
1. [ ] Axe with `co_affix bountiful 100`: chop a log to pieces → every piece drops **twice** (control: affixes still work).
2. [ ] Chop a fallen log once **without** breaking it, then fell another tree (plain axe) onto it, or let it roll down a slope until it smashes → its wood drops **once** (normal), and the log shows `Harvest affixes: <name> was hit without a player's swing … cached tool chances not used.`
3. [ ] Pickaxe with `co_affix bountiful 100` on a **big copper rock** that turns into a copper deposit → the deposit's chunks still drop **twice** (the rock passes your affixes on).
4. [ ] No red errors in the console.

### D. With a friend (on the server)
Also in [`../PLAYTEST.md`](../PLAYTEST.md) → Session 2, 2f.
1. [ ] **Other player runs the deposit:** player B looks at a copper deposit and types `co_own` ("your game runs it now"). Player A repeats A1 and A3 on it → same results as alone.
2. [ ] **Other player runs the log:** B types `co_own` at a log; A repeats C2 on it → normal drops, and **B's** log shows the "without a player's swing" line.
3. [ ] **Odin's Grace on one tree from two games (checking a suspected bug, not fixed yet):** fell a tree with a plain axe. A looks at the log and types `co_own`; B looks at the stump and types `co_own`. Both take an axe with `co_affix odins_grace 100`. Count down and **swing at the same moment** (A at the log, B at the stump). Note how much wood drops: one tree's worth (fine) or about twice that (bug). Send Claude the `Odin's Grace: whole-tree harvest of tree …` lines from **both** logs. Try up to 5 trees.

### Your feedback (date)
- …

### What changed after the feedback
- …

### Final play-test (date)
- …
