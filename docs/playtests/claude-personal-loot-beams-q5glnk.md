# Play-tests: `claude/personal-loot-beams-q5glnk`

## Round: personal loot and loot beams (2026-11-26)

**Build:** the three DLLs from this branch (only Corruption changed). The log line should read `Corruption 0.9.0 loaded (build: claude/personal-loot-beams-q5glnk <commit>)` (versions are raised when the branch merges).
**Setup:** single player on Valheim 1.0.30, a test world, EquipmentAndQuickSlots and AdventureBackpacks 2.4.8 loaded (PlantEasily off). F5, `devcommands`, `god`. Somewhere in the Meadows or Black Forest with open ground.

**What's new (your decisions 1–12):**
- **Personal loot.** A drop rolled for you is yours for its first 60 seconds: other players don't see it at all (no item, no beam, no hover text) and can't pick it up. After 60 s it appears for everyone and anyone can take it. It covers each player's kill loot (Magic and Rare gear, orbs, maps, the corrupted boss's orb), each player's boss shard, and the orbs your Hoardfinder's or "of Hidden Riches" tool finds. Shared drops stay shared (vanilla loot, essences, the first-kill and corrupted-boss bonus shards, creature shards, dungeon chests). Items a player drops from their own inventory are never locked, so trading stays free.
- **Loot beams** on every Corruption drop: Magic gear blue, Rare yellow, unique orange-gold (no uniques exist yet), every orb white, shards red, maps green. Short and faint for Magic gear, shards and the common orbs (Transmutation, Augmentation, Alchemy, Regal, Annulment); tall and bright for Rare gear, uniques, maps and the rare orbs (Exalted, Divine, Chance, Mirror). The beam stays until the item is picked up.
- **A sound** when a tall-beam drop lands, for its owner only. **For now it's a stand-in, the game's skill-level-up sound:** you pick the real ones by ear in step 13.
- **No rip chest any more.** Destroying the rip heart spills its loot on the ground: each player's own orb and gear rolls are theirs, and the shared part (the biome chest's vanilla loot, the 1–3 shards, the map) is dealt among the players near the heart, one item at a time in turn (alone, it's all yours).
- **Test command** `co_personal` (type it alone for the list). `co_personal drop … other` makes a drop that belongs to a made-up other player, so you can see what a friend's locked drop is like for you: invisible until its lock ends.

**Numbers to judge while you play:** the beam sizes are first guesses (tall 10 m high and 0.35 m wide with a small light, short 2.5 m and 0.15 m with no light, both in `Corruption/Data/personal_loot.csv`). Tell me if the tall ones should be taller or brighter, or the short ones fainter.

**Phone page:** the same steps are the first section of the [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht), "This round: personal loot and loot beams" (step ids `pl-1` … `pl-15`).

### Checklist

**Start**

1. [ ] (`pl-1`) Start the game and open `BepInEx/LogOutput.log` → it has `Personal loot: 14 beam rows, a drop is locked to its owner for 60 s.` and no red errors at startup.

**Your own drops and the beams**

2. [ ] (`pl-2`) F5, `co_personal drop rare` → a Rare item lands in front of you with a **tall yellow beam**, and you hear the stand-in sound (the skill-up chime). Pick it up with E → it goes into your bag as usual and the beam is gone.
3. [ ] (`pl-3`) `co_personal beams` → 14 drops in a row in front of you, each with its beam (the console names them left to right). Check the colours: Magic blue, Rare yellow, unique orange-gold, shard red, map green, every orb white. Check the sizes: tall for Rare, unique, map, Exalted, Divine, Chance, Mirror; short for the rest.
4. [ ] (`pl-4`) Walk (or `fly`) about 50 m away and look back → the tall beams are still easy to see; the short ones may fade. Note anything you'd change (height, width, brightness).
5. [ ] (`pl-5`) Pick all 14 up (auto-pickup or E) → each goes where that item normally goes (orbs to Currencies, the shard to Currencies, the map to Maps) and its beam disappears with it.

**Someone else's drop**

6. [ ] (`pl-6`) `co_personal drop rare other` → the console says it dropped for "the test owner", but **you see nothing**: no item, no beam. `co_personal list` → one line: owner "the test owner", about 60 s of lock left, "your game HIDES it", 1–2 m away.
7. [ ] (`pl-7`) Stand right where it landed and wait a few seconds → auto-pickup doesn't take it, pressing E there does nothing, no hover text shows up, and your crosshair still finds things behind it (drop a stone just past the spot first and aim at it).
8. [ ] (`pl-8`) `co_personal drop orb exalted other 10` (10 seconds of lock) → nothing to see; after about 10 seconds a **tall white beam** appears with the orb (no sound: it isn't yours), and now you can pick it up.
9. [ ] (`pl-9`) **Backpack:** wear an AdventureBackpacks backpack that already holds a Transmutation Orb (its auto-store then wants more). `co_personal drop orb transmutation other 15` and stand on the spot → nothing for 15 s. When it appears, auto-pickup takes it → it ends up where a Transmutation Orb from a kill would go with that backpack on (compare with `co_personal drop orb transmutation`).
10. [ ] (`pl-10`) **Relog:** `co_personal drop rare other` and `co_personal drop magic`, then log out to the menu and back in within the minute → the Magic item is still there with its blue beam (yours); the other one is still hidden until its minute is up (`co_personal list`), then appears.

**Real drops**

11. [ ] (`pl-11`) **Kills:** `spawn Greyling 20`, kill them all, twice → some orbs (and maybe a Magic or Rare item) drop with their beams; `co_personal list` shows them as owner "you". Vanilla drops (resin, greydwarf eyes) have no beam. **Boss:** `spawn Eikthyr` and kill him → your shard has a short red beam and is yours in `co_personal list`; essences have no beam; if it's this world's first Eikthyr kill, the 1–3 bonus shards have red beams with owner "nobody".
12. [ ] (`pl-12`) **Mining orbs:** hold a pickaxe, `co_affix hoardfinders 100`, break a few chunks of a copper deposit → each chunk drops an orb where you hit it, with its beam, owner "you" in `co_personal list`. (Then `co_affix hoardfinders 0` or swap tools.)
13. [ ] (`pl-13`) **Rip heart:** wear the Corrupted wishbone, use a Meadows map (`spawn CorruptionMap_Meadows`), clear the rip and destroy the heart → **no chest**; the loot spills around where the heart was: the Meadows chest's vanilla items, 1–3 shards (red beams), the Meadows map (green beam), and now and then an orb or a Magic/Rare item. The exit rip still opens and takes you home. The log has `Rip loot: 1 player(s) near the heart.`, a `Rip loot: player <your name> …` line listing what you got, and `Rip loot: spilled N drop(s) …`.

**Sounds (your pick)**

14. [ ] (`pl-14`) `co_personal sounds` → a short list of candidate sounds, each "found" or "missing". Try them with `co_personal play <name>` (e.g. `co_personal play sfx_coins_pile_placed`); `co_personal sounds coin` (or `gem`, `chest`, `bell`, `gui`…) lists more. When you like one, `co_personal sound rare <name>` and `co_personal drop rare` to hear it on a real drop. Choose one for Rare gear, one for uniques, one for maps and one for the rare orbs (they can be the same), and write the four names in the note.

**Finish**

15. [ ] (`pl-15`) Drop an orb from your own inventory (drag it out) → no beam, and you pick it up again normally. F5 shows no red errors from the whole session. Send `BepInEx/LogOutput.log`.

### Your feedback (2026-11-26, phone page; build `claude/personal-loot-beams-q5glnk 21b7a26`, Valheim 1.0.30)

- Step 1: not marked; the log has the line (`Personal loot: 14 beam rows, a drop is locked to its owner for 60 s.`) and no errors at startup.
- Step 2: passed. "beam is massive though, way too big. shrink to half the size."
- Step 3: passed.
- Step 4: passed. "they are all too one dimensional so to say. i think they would look better if they flare. thick at base and thinner towards the top. all the taller ones are just too tall. Would look better if the beam pulsated very slowly"
- Steps 5–13: passed.
- Step 14: passed. "fine with the level up sound for now. maybe we can make our own custom sounds similar to diablo 4 drops sounds."
- Step 15: passed.
- The log (Claude's reading): every patch applied on 1.0.30 (no `Failed to apply patch` line), the beam's shader was found, no errors from this round. The heart's log lines are as expected (`Rip loot: 1 player(s) near the heart.`, then your share: 4 Feathers, 4 Torch, 3 Amber, 31 Coins, 1 shard, the Meadows map; 9 drops). The only warnings are older ones (Orlog's F2 panel on 1.0.30, #86; the game's own).

### What changed after the feedback

- Steps 2 and 4: the tall beams are half the size (5 m high, 0.175 m at the base; they were 10 m and 0.35 m). Every beam, short and tall, now narrows from its base to a quarter of that width at the top, and pulses slowly: it dims by up to about a third and back once every 4 seconds, each drop out of step with its neighbours. The numbers are in `Corruption/Data/personal_loot.csv` (`tall_height`, `tall_width`, `top_width_fraction`, `pulse_seconds`, `pulse_strength`). Short beams keep their size (2.5 m, 0.15 m).
- Step 14: no change; the level-up sound stays for every tall beam. Custom drop sounds like Diablo 4's are noted for later.

### Final play-test

**Build:** the three DLLs from this branch again (the log line names the newer commit). Same setup as above.

1. [ ] (`plf-1`) `co_personal beams` → the 14 drops again. The tall beams (Rare, unique, map, Exalted, Divine, Chance, Mirror) are about half as tall and half as wide as before. **Every** beam is thick at the base and thin at the top, and pulses slowly (about one slow dim and back every 4 seconds), not all at the same moment. Note anything you'd still change (taller or shorter, thinner, faster or slower pulse, a deeper or softer dim).
2. [ ] (`plf-2`) Walk or `fly` about 50 m away → the tall beams are still easy to find; note if they're now too small to spot.
3. [ ] (`plf-3`) `co_personal drop rare` → the Rare beam is a comfortable size and pulses; you hear the level-up sound. `co_personal drop orb exalted other 10` → after about 10 s its beam appears, tapered and pulsing.
4. [ ] (`plf-4`) Kill `spawn Greyling 20` → real drops have the new beams; pick them up → the beams go with them. F5: no red errors. Send `BepInEx/LogOutput.log`.

### Final play-test feedback (2026-11-26, phone page, with a screenshot)

- Step `plf-1`: failed. "they look better but still not quite right. attached screenshot. Maybe they need more opacity or smokelike effect. they are too solid looking." (The screenshot shows the 14 beams of `co_personal beams`, tapered, with hard edges and bright solid cores.)
- Steps `plf-2`, `plf-3`, `plf-4`: passed.

### What changed after the final play-test

- Step `plf-1`: the beams are now soft-edged and see-through (they fade to nothing at their sides instead of a hard edge, and the core is dimmer), with a faint, wider glow around each and slow smoke-like wisps rising up it. Still tapered, pulsing, same colours and sizes. Every number is in `Corruption/Data/personal_loot.csv`, and you can change any of them live with `co_personal look`.

### Look test (after the final play-test)

**Build:** the three DLLs from this branch again. Same setup; dusk or night shows the beams best, then check daylight too.

1. [ ] (`plg-1`) `co_personal beams` → every beam is soft at its sides and see-through, with a faint haze around it and a few slow wisps drifting up it; still thick at the base and thin at the top, pulsing slowly, same colours. Compare with your screenshot.
2. [ ] (`plg-2`) **Tune it yourself:** `co_personal look` lists the settings. Try for example `co_personal look beam_alpha 0.3` (fainter core) or `0.6` (stronger), `co_personal look glow_alpha 0.25` (more haze), `co_personal look smoke_per_second 6` (more wisps), `co_personal look smoke_alpha 0.4` (stronger wisps), `co_personal look edge_softness 0.9` (softer sides). Every change redraws the beams at once and prints a line like `personal_loot.csv: beam_alpha = 0.3`. Paste the lines you like into the note; `co_personal look reset` goes back.
3. [ ] (`plg-3`) In daylight and from about 50 m → the tall beams are still easy to find.
4. [ ] (`plg-4`) Kill `spawn Greyling 20` near the line of beams → the game runs as smoothly as without them; F5 shows no red errors. Send `BepInEx/LogOutput.log`.

