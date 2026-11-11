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

## Round 2: the skill bar, the bow skills, Inner Sight and boss points (2026-11-11)

**Build:** the same branch, newer commit (Orlog's log line names `claude/lucid-albattani-imrei7`). Round 1 (the passives) and this round can be played in one go.
**Setup:** single player, test world, the group's mods loaded, CraftingOverhaul's classes screens on (its demo off). F5, `devcommands`; no `god` for the fights. `orlog_class Amazon` if needed, `orlog_level 30`, then spend: `orlog_skill MagicArrow`, `orlog_skill FireArrow`, `orlog_skill ColdArrow`, `orlog_skill MultipleShot`, `orlog_skill InnerSight`, `orlog_skill ExplodingArrow`, `orlog_skill IceArrow`, `orlog_skill FreezingArrow`. `spawn BowFineWood`, `spawn ArrowWood 50`, `spawn ShieldWood`.

**New keys:** right click = the skill button; block = the mouse's **back side button (Mouse 4)**; F1–F4 = the four skill slots. All can be changed in the Orlog config, section `6 - Skill Keys`.

### Checklist

**Slots and keys**
1. [ ] Skills panel: drag Magic Arrow onto slot 1, Fire Arrow onto slot 2, Multiple Shot onto slot 3, Inner Sight onto slot 4 → the cross shows them with F1–F4. Drag Dodge onto a slot → "Passive skills don't take a slot". (If dragging fails: `orlog_slot 1 MagicArrow` and so on.)
2. [ ] Shield in hand: right click **no longer blocks**; the back side button blocks (and parries); block + jump still dodges.

**The skill button**

3. [ ] Bow in hand, press F1 → slot 1 lights up. Hold right click (the bow draws), release → an arrow flies; the eitr bar drops a little (1.5). Left click still shoots a normal arrow.
4. [ ] **Magic Arrow needs no arrows:** put all your arrows in a chest; right click still shoots. With arrows back in the inventory, shooting Magic Arrow doesn't use any (count them).
5. [ ] **Fire Arrow:** F2 and shoot a boar → it burns; the damage numbers are bigger than a normal arrow's.
6. [ ] **Multiple Shot:** F3 and shoot → 2 arrows fan out (at skill level 1), for one arrow from the quiver. `orlog_skill MultipleShot 4` → 6 arrows.
7. [ ] **Wrong weapon:** take out an axe and right click → nothing (a "Need bow" message, or nothing). Take out the bow again → slot 3 (the last bow skill) is lit again.
8. [ ] **Out of eitr:** shoot Fire Arrow until the eitr bar is empty → "Not enough eitr", the slot greys out; it comes back as eitr refills.

**Frost and explosions** (put Cold Arrow, Ice Arrow, Exploding Arrow and Freezing Arrow in the slots as needed)

9. [ ] **Cold Arrow:** the boar hit is slowed (frost). **Ice Arrow:** the boar hit can't move for about 2 seconds ("Frozen").
10. [ ] **Exploding Arrow:** `spawn Greyling 5`, shoot the middle of the group → a fiery burst; every greyling nearby takes fire damage.
11. [ ] **Freezing Arrow:** shoot a group → a frosty burst; the greylings nearby take frost damage and freeze for 2 seconds.

**Inner Sight**

12. [ ] F4 near a few creatures → your character does the "power" gesture, "Inner Sight: N marked", the creatures glow pale blue for 8 seconds, and your hits on them show about a quarter more damage than before.

**Tooltips and boss points**

13. [ ] The skills panel's tooltip for each bow skill shows its numbers (e.g. "Adds … fire damage; …% of the physical damage becomes fire") and the next level's.
14. [ ] **Boss points:** note the skill points to spend (`orlog_info`). `spawn Eikthyr` and kill him → "First victory over this foe: +1 skill point", one more point to spend. Kill another Eikthyr → no more. Relog → the point is still there.

**How it feels and the log**

15. [ ] Your impression of the skills' damage next to a normal arrow: too strong, about right, too weak? (The numbers follow the proposed scale; one setting, `SkillDamageMultiplier`, turns it up or down.)
16. [ ] F5 shows no red errors; `LogOutput.log` has no "Orlog could not …" lines.

**With another player** (`PLAYTEST.md` → Session 2, 2j): a freeze and Inner Sight on creatures the other player's game runs, both players getting a boss's points.

### Your feedback

(none yet)
