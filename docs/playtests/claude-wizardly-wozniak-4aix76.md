# Play-tests: `claude/wizardly-wozniak-4aix76`

## Round: Rimeburst and Guarded, more corrupted-only map affixes (#97, 2026-11-28)

**Build:** the DLLs from this branch (Corruption, Orlog and CraftingOverhaul). The log's first Corruption line should read `Corruption <version> loaded (build: claude/wizardly-wozniak-4aix76 <commit>)`. This branch contains `claude/nifty-hypatia-afm8ez`, so its steps (`la-1` … `la-20`) can be tested with this same build.
**Setup:** single player, Valheim 1.0.31, a test world, the group's mods loaded as usual. Stand in the **Meadows**. Press **F5**, type `devcommands`, then `god`. Keep `BepInEx/LogOutput.log` open.

**What changed:**
- **Two new corrupted-only affixes.** A corrupted (tier 6) map now gets one of three as its 5th affix, **each 1 in 3**: Thorns, Rimeburst or Guarded.
- **Rimeburst:** "Enemies explode in ice on death." (red, last under "What you'll face", no number shown). 1.5 s after a rip creature dies, a pale blue ring (3 m) pulses on the ground and a light grows; then it bursts. If you're inside, you take frost: 50–100% (rolled per map) of that creature's own strongest hit (bigger for starred and Brutal creatures). Armour, frost resistance, blocking and rolling reduce it; frost slows you. It never hurts other enemies. Preview: `docs/images/rimeburst_preview.png`.
- **Guarded:** "A corrupted boss guards the heart." When the waves start, the biome's own boss (Meadows: Eikthyr), corrupted and red, appears by the heart and comes for you. It carries the map's other affixes and summons its army as a corrupted boss does. **The heart can't be damaged while it lives.** Killing it gives a corrupted boss's drops.

**To test each alone:** the four normal affixes in the commands below are reward affixes (they change no fight), so the 5th is the only danger.

### Checklist

**A. On the map**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| cg-1 | Read the top of the log. | No red errors. The `Map tiers:` line ends `corrupted-only: thorns, rimeburst, guarded).` | | |
| cg-2 | `co_map meadows 6`, eight times. Hover each. | Each has 5 lines; the last, in red, is one of: `Enemies return N% of the damage you deal to them.`, `Enemies explode in ice on death.`, `A corrupted boss guards the heart.` Each turns up about as often as the others. | | |
| cg-3 | `co_map meadows 5 glorious bountiful glittering bulwarked`, then corrupt it with a shard at the Riftforge (`spawn CorruptionShard 1`). Do it with two or three such maps. | "Corrupted: now tier 6", 5 lines, the 5th one of the three red lines. | | |

**B. Rimeburst alone**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| cg-4 | `co_map meadows 6 glorious bountiful glittering bulwarked rimeburst:100`. Open its rip. Turn `god` **off** (good armour, food). | The rip opens. The hover on the heart lists the red line. | | |
| cg-5 | Kill a wave creature with a melee weapon and **stay next to its body**. | Right away: a pale blue ring around the body, pulsing faster, and a growing light. After about 1.5 s a burst of ice; you lose some health and get the Frost slow. Log: `Rimeburst: <creature> (level N) bursts in 1.5 s for X frost (100% of its hit).` then `Rimeburst: caught in a burst of X frost`. | | |
| cg-6 | Kill another one and **step out of the ring** (or roll) before it bursts. | The burst plays; you take nothing (no `caught` line). | | |
| cg-7 | Kill one while other creatures stand next to its body. | They take no damage from the burst. | | |
| cg-8 | Kill a 2-star creature and stay in the ring. | The log's frost number is higher than a 0-star one of the same kind (×1.5 per star level above none). | | |
| cg-9 | Destroy the heart. Then `co_map meadows 6 glorious bountiful glittering bulwarked rimeburst:50`, open it and kill a boar the same way. | The heart works as before. With 50%, the boar's frost in the log is half of its 100% number from before. | | |

**C. Guarded alone**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| cg-10 | `co_map meadows 6 glorious bountiful glittering bulwarked guarded`. Open its rip; keep `god` on for now. Walk towards the heart. | When the waves start, a **red Eikthyr** appears by the heart and comes at you. Log: `Guarded: a corrupted Eikthyr guards rip …`. | | |
| cg-11 | Hover the heart, then hit it (weapon) while Eikthyr lives. | The hover has the red line `Guarded: kill the corrupted boss first`. The heart takes no damage. | | |
| cg-12 | Wait 30 s near Eikthyr. | It summons 2–3 of its army now and then, as at the altars, on top of the waves. | | |
| cg-13 | Kill Eikthyr. | It drops its trophy and antlers, 2–3 Essences of Eikthyr and 3–5 shards, plus rip loot. The hover's guarded line is gone and the heart can now be destroyed. | | |
| cg-14 | `co_map meadows 6 brutal bountiful glittering bulwarked guarded`, open it, turn `god` off and let Eikthyr hit you once. | It hits harder than in cg-10's rip (corrupted ×1.5 and Brutal). Turn `god` back on. | | |
| cg-15 | In a Guarded rip, leave Eikthyr alive and let the rip close (or set `[5 - Rips] OpenDays` low and reload). Walk away and come back. | Eikthyr is gone, like the leftover wave creatures. | | |
| cg-16 | (Optional, a stronger biome) `co_map plains 6 glorious bountiful glittering bulwarked guarded`, `co_map mistlands 6 …guarded` or `co_map deepnorth 6 …guarded`, open them. | Yagluth, the Queen or Kall appears and fights. Tell me if one of them gets stuck or behaves oddly outside its arena. | | |

**D. Switching them off**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| cg-17 | Config `[5 - Rips] DisabledMapAffixes` = `thorns, rimeburst`. Corrupt a new tier 5 map. Then set it to `thorns, rimeburst, guarded` and corrupt another. | The first always gets the Guarded line; the second has only 4 lines. Set it back to blank afterwards. | | |
| cg-18 | Look through the whole log. | No red errors and no `Failed to apply patch`. | | |
| cg-19 | **Only if the game freezes again:** wait until Windows closes it (or close it), then send `BepInEx/LogOutput.log` and `BepInEx/corruption_freeze.txt`. | A `FREEZE WATCH:` line saying whether the game was inside our mods' code, and which part. | | |

## Final play-test: corrupted creatures fight players only (2026-11-28)

**Build:** this branch after the "corrupted creatures fight players and tames only" commit. Same setup (single player, Meadows, `devcommands`).
**What changed** (decisions 20–22): red (corrupted) creatures, a corrupted boss and its summons never fight each other or normal creatures, and normal creatures leave them alone. They hunt you and your tamed creatures. Creatures near the heart still turn red when the waves start.

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| cg-20 | `co_map meadows 6 glorious bountiful glittering bulwarked guarded`, open its rip, keep `god` on and watch the waves near the heart for a minute. | Red creatures never hit each other or the red Eikthyr; they all come for you. | | |
| cg-21 | While they come, `spawn Boar 3` and `spawn Greyling 3` next to them (normal creatures). | The red ones ignore the normal ones and keep coming for you; the normal ones ignore the red ones and behave as usual (the greylings attack you, the boars only if you hit them). | | |
| cg-22 | Tame a wolf or boar (or `spawn Wolf`, then type `tame` next to it), bring it into a rip. | The red creatures attack your tame too, and its bites hurt them. | | |
| cg-23 | At an altar, summon a corrupted boss as usual (corrupted trophy). | Its summons and the boss still come for you only (as before). | | |
| cg-19 | **Only if the game freezes:** send `BepInEx/LogOutput.log` and `BepInEx/corruption_freeze.txt`. | A `FREEZE WATCH:` line. | | |

### Your feedback

- **2026-11-28, during the play-test: the game crashed while idling in town** (Krogon: "the same type of crash as reported in the previous session when accessing the riftforge"). His `LogOutput.log` (build `claude/wizardly-wozniak-4aix76 a4f7de3`, Valheim 1.0.31) just stops at 21:34:13, with no error or exception before it: a crash inside the game engine, which BepInEx's log can't record. The last mod action was a tier 6 Meadows rip with Rimeburst opened at 21:31; he never went through it, no creature died and no Rimeburst or Guarded code ran. The "GetBiome error BlackForest -> Meadows" warning every second is the game's own (`Player.cs`, standing on a biome border) and harmless. Then (his `Player.log` and the Event Viewer): **a freeze, not a crash**. Windows logged "Application Hang … valheim.exe stopped interacting with Windows and was closed" at 21:36:28, so the game stopped drawing frames at about 21:34:10 and hung for two minutes. `Player.log` shows the round working before it: the Rimeburst rip ran (bursts logged, `caught in a burst of 20 frost`), the heart broke with its tier 6 loot, and he came home through the exit about 21:33:30; then 40 s of idling with nothing but the game's biome-border warning. Nothing names the cause. Krogon chose a freeze watch (decision 19) so the next freeze is named: step `cg-19`.
- **2026-11-28: the solo play-test passed** (phone page, `cg-1` … `cg-18` all Pass; `cg-19` not needed: "game did not hang again but will keep an eye on it"). Note on `cg-5` (Rimeburst): "Log prints this, yes. But the damage feels very low and also the slow is not significant enough. Visually, this mechanic leaves much to be desired as well. but the visual part can be at a later stage." Krogon also asked for a design change from playing many rips: corrupted creatures fight players only (handoff decisions 20–22).
