# Play-test: `claude/hopeful-thompson-eyncm8`

## Round: review fixes: corrupted summons, damaged saves, talisman fight-back (2026-11-08)

**Build:** the log lines `Corruption … loaded (build: claude/hopeful-thompson-eyncm8 …)` and `Orlog … loaded (build: claude/hopeful-thompson-eyncm8 …)`, the same commit.
**Setup:** `devcommands`. Single player first (A–D), then the server with a friend (E). Open the console (F5) to read the lines quoted below; `BepInEx/LogOutput.log` has them too.

### A. Corrupted summon at Eikthyr's altar (alone)
Bring: `spawn CorruptedTrophyDeer 2`, `spawn TrophyDeer 4`, `spawn Wood 5`. Put all three on the hotbar. Eikthyr takes **12 s** to arrive after an offering (game data), so there's time.
1. [x] Offer one **corrupted** deer trophy (look at the altar, press its hotbar number). Within the 12 s, offer the **normal** trophies, then **wood**, then the **second corrupted** trophy, a few times each → **one** Eikthyr arrives, red. You have **1** corrupted trophy left, all 4 normal ones, all the wood. The log shows `Corrupted offering: Eikthyr summon started.` and `Corrupted offering: Eikthyr summoned corrupted.` **once each**.
2. [x] Kill him (or `killall`). Offer **2 normal** trophies, and within the 12 s offer the corrupted one → a **normal** Eikthyr; you keep your corrupted trophy.
3. [x] Kill him. Offer the last corrupted trophy with nothing else → red Eikthyr, as before the fix.

### B. Corrupted summon at a stand altar (alone)
At Moder's altar (Mountains; `spawn CorruptedDragonEgg 3`, `spawn DragonEgg 3`).
1. [x] One corrupted egg in **every** cup, use the altar, then use it again a few times during the delay → **one** red Moder; the cups empty.
2. [x] A mix (2 corrupted + 1 normal) → "Every offering here must be corrupted, or none." and nothing is used up.

### C. Damaged affix values (alone)
Hold a fresh sword (`spawn SwordBronze`).
1. [x] `co_affix enhanced_damage NaN`, then `co_affix enhanced_damage Infinity` → each says `'…' isn't a finite number.`; the sword's tooltip is unchanged.
2. [x] `co_affix enhanced_damage 500` → added (a finite value is kept even far outside the table's range): the tooltip shows +500% and the damage numbers are about six times a plain bronze sword's. Drop and pick it up, relog → still there.
3. [x] Your existing Magic and Rare items show their names and affix lines as before; no red errors.

### D. Stat and skill points, and the talisman fight-back (alone)
1. [x] `orlog_class Amazon`, `orlog_level 20`, `orlog_stat vitality 10`, `orlog_skill MagicArrow 2`, then `orlog_info` → it lists Vitality +10 and the points left; no error. Relog → the same.
2. [x] Lowered setting (admin): in Orlog's config file in `BepInEx/config/` set `StatPointsPerLevel` to `0` and save (it reloads), then `orlog_info` → **0** stat points to spend, and Vitality still **30** (the Amazon's 20 + the 10 spent: nothing refunded or removed). Set it back to `5` → the points to spend come back.
3. [x] Wear a Greydwarf charm. Hit one greydwarf once → only it fights back; the log says `Talisman: Greydwarf fights back at <you> for 30 s (run by this game).` About 30 s after the hit it ignores you again; other greydwarfs ignore you throughout.
4. [x] Hit it again at about 20 s → it keeps fighting for 30 s after **that** hit (a new log line).

### E. With a friend (on the server)
Also in [`../PLAYTEST.md`](../PLAYTEST.md) → Session 2, 2g. Player A and player B. The game that runs a creature or altar is usually the player who was there first.
1. [ ] **Fight-back survives the owner logging out.** B goes to a greydwarf group **alone first** (B's game runs them). A arrives wearing a Greydwarf charm, no charm on B. A hits one greydwarf → **B's** log has the `Talisman: … fights back at <A> … (run by this game)` line. Within 10 s, **B logs out**. Expect: the greydwarf keeps attacking A until about 30 s after A's hit, then ignores A. (Before the fix it stopped at once.)
2. [ ] **A new hit restarts it after the change.** Still alone at the group, A hits the same greydwarf again → **A's** log now has the line (A's game took over); it fights A for 30 s after that hit.
3. [ ] **Only the player who hit, only that creature.** B rejoins, both wear a Greydwarf charm. A hits one greydwarf → it attacks A, **not B**; the other greydwarfs ignore both.
4. [ ] **Two players, one corrupted summon.** Both carry a corrupted deer trophy, a normal trophy pair and wood. A offers the corrupted trophy at Eikthyr. During the 12 s, **B** offers a corrupted trophy, then normal trophies, then wood. Expect: **one** red Eikthyr, red for both; **A** lost one corrupted trophy; **B** lost nothing.
5. [ ] **The other way round.** A does a normal summon (2 trophies); during the delay **B** offers a corrupted trophy → a **normal** Eikthyr; B keeps the corrupted trophy.
6. [ ] Swap who goes to the altar first and repeat step 4 once → the same result.
7. [ ] No red errors on either game (F5).

### Your feedback (2026-11-08)
- A, B, C, D: "A–D passed alone".
- E: "multiplayer later" (in `PLAYTEST.md` → Session 2, 2g).
- No log or build stamp sent with it.

### What changed after the feedback
- Nothing needed.

### Final play-test (2026-11-08)
- Passed alone (A–D). E (two players) waits for the group evening.
