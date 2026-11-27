# Play-tests: `claude/kind-einstein-8sn8e7`

## Round: map affix overhaul (2026-11-27)

**Build:** the DLLs from this branch (Corruption, Orlog and CraftingOverhaul). The log's first Corruption line should read `Corruption <version> loaded (build: claude/kind-einstein-8sn8e7 <commit>)`. The version is not raised on this branch; it changes when the branch merges.
**Setup:** single player, Valheim 1.0.31, a test world, the group's mods loaded as usual. Press **F5** to open the console and type `devcommands` (this turns on the test commands `co_map` and `co_rip`), then `god`. Stand next to a **Riftforge** (build one, or use the one you have) and type these. Each puts the items in your bag; the orbs and the shard move to the Currencies tab within half a second of opening the inventory:

```
spawn CorruptionOrb_Transmutation 3
spawn CorruptionOrb_Augmentation 2
spawn CorruptionOrb_Regal 2
spawn CorruptionOrb_Exalted 4
spawn CorruptionOrb_Annulment 2
spawn CorruptionOrb_Divine 1
spawn CorruptionOrb_Mirror 1
spawn CorruptionOrb_Alchemy 4
spawn CorruptionShard 2
```

Use a **fresh** white map for each test (`co_map meadows`), so one map's affixes don't mix into the next. Open the log (`BepInEx/LogOutput.log`) while you play; the lines quoted below are from it.

**What changed:**
- **Seventeen map affixes now have real values.** Glorious, Teeming, Brutal, Frenzied, Ruthless, Sturdy, Mighty, Fleet, Unyielding, Ironskin, Warded, Withering, Exhausting, Exposed, Venomous, Searing and Stormcalled, all from Krogon's list. Each one's strength is shown on the map's tooltip under "What you'll face".
- **The old four are gone:** Swarming, Hardened, Rabid and Smouldering. Old maps that carry them lose those lines, and their tier goes down.
- **Some pairs never share a map:** Ironskin with Warded, and Sturdy with Mighty.
- **Frostbitten, Drowned and Choking are listed but do nothing yet** (they wait for weather data).
- **An admin can switch affixes off** with `DisabledMapAffixes` in the Rips section of the config.

### Checklist

**A. Start, tooltips and the Forge tab**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| ma-1 | F5, `devcommands`, `god`. Open `BepInEx/LogOutput.log` and read the top of it. | No red errors. The log has `Map tiers: 6 tiers, 20 affixes (glorious, teeming, brutal, frenzied, ruthless, sturdy, mighty, fleet, unyielding, ironskin, warded, withering, exhausting, exposed, venomous, searing, stormcalled roll).` Frostbitten, drowned and choking are counted but not in the roll list. | | |
| ma-2 | `co_map meadows 2 brutal:100`. Open the inventory, Maps tab, and hover the map. | A Magic tier 2 Meadows map (blue name). Under "What you'll face": `Brutal — Enemies deal 100% more damage.` | | |
| ma-3 | `co_map meadows 3 brutal sturdy`. Put the map in the Forge tab. Pick up a **Divine Orb** in Currencies and click the forge slot. Repeat with more Divine Orbs if needed. | Brutal's number changes to something from 20 to 100. Sturdy's changes to something from 25 to 50. The map stays tier 3. | | |
| ma-4 | `co_map meadows 2 teeming`. Put it in the forge and use a Divine Orb. | Refused: "Map affixes have no values to reroll." Teeming has one fixed value, so nothing changes. | | |
| ma-5 | Make at least 4 new maps: `co_map meadows`, then an Orb of Alchemy (tier 4, 3 affixes); for some, an Exalted Orb as well (tier 5, 4 affixes). Look at each map's affix list. | No map ever shows both Ironskin and Warded, or both Sturdy and Mighty. If you see one, write which map and its tooltip in the Note. | | |
| ma-6 | `co_map meadows 3 ironskin warded`. Use the map (from the Maps tab) to open its rip. Read the log, then hover the rip. | Log: `Rip request: map affix 'warded' dropped (its group is already on the map).` The rip is **Tier 2**, with Ironskin only. | | |

**B. One rip per affix**

Each step: `co_map meadows 2 <affix>` (a tier 2 Meadows map with that one affix). Use it from the Maps tab and walk into the rip. Stand in open Meadows ground, away from your base.

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| ma-7 | `co_map meadows 2 glorious:100`. Kill creatures in the rip. Compare the XP each kill gives with a white rip (`co_map meadows`). | Each kill gives clearly more Orlog XP than on the white rip. Glorious at 100 doubles it. | | |
| ma-8 | `co_map meadows 2 teeming`. Use it and count the creatures alive near the heart. | Noticeably more enemies alive at once than on a tier 2 map with another affix. | | |
| ma-9 | `co_map meadows 2 brutal:100`. Use it. Let an enemy hit you. | Enemy hits take about twice as much health as a normal hit. | | |
| ma-10 | `co_map meadows 2 frenzied:100`. Use it. Watch the enemies attack. | Enemies swing visibly faster and attack again sooner. | | |
| ma-11 | `co_map meadows 2 ruthless:50`. Use it. Watch the enemy hits. | Some hits do about double damage. | | |
| ma-12 | `co_map meadows 2 sturdy:50`. Use it. Hit one enemy with the same weapon you normally use, and count the hits. | The enemy takes about 1.5 times as many hits as normal (50% more health). | | |
| ma-13 | `co_map meadows 2 mighty:100`. Use it. Hit one enemy and count the hits. | The enemy takes about twice as many hits as normal. Its health bar still starts full, but it takes twice as long to empty. | | |
| ma-14 | `co_map meadows 2 fleet`. Use it. If you have a **Freezing Arrow**, freeze one enemy. | Enemies run noticeably faster. A frozen enemy stops moving. (No Freezing Arrow: write "no arrow" in the Note.) | | |
| ma-15 | `co_map meadows 2 unyielding`. Use it. Land perfect parries or heavy hits on one enemy. | The enemies can't be staggered: the stagger bar never fills. | | |
| ma-16 | `co_map meadows 2 ironskin`. Use it. Hit an enemy with a sword, axe or bow, then with fire arrows or a fire staff. | Sword, axe and bow barely hurt it (about 1/20 of normal). Fire, frost and poison damage hurts normally. | | |
| ma-17 | `co_map meadows 2 warded`. Use it. Hit an enemy with fire arrows or a frost staff, then with a sword or axe. | The opposite of Ironskin: elemental damage barely hurts, and the physical hit works normally. | | |
| ma-18 | `co_map meadows 2 withering`. Use it. Stand near the heart (within about 96 m), then walk far away. | Near the heart you lose 5 health a second, and the **Rip curse** status icon appears (its tooltip lists the effect). Far away, the drain and the icon go. | | |
| ma-19 | `co_map meadows 2 exhausting`. Use it. Stand still within about 96 m of the heart for 10 seconds. | Stamina drains 5 a second while you stand (almost no regen). Far away, stamina works as normal. | | |
| ma-20 | `co_map meadows 2 exposed:50`. Use it. | The Rip curse icon shows Exposed. Elemental hits hurt more than usual. | | |
| ma-21 | `co_map meadows 2 venomous:75`. Use it. Let an enemy hit you. | Enemy hits also poison you (the poison status). | | |
| ma-22 | `co_map meadows 2 searing:75`. Use it. Let an enemy hit you. | Enemy hits also set you burning for about 5 seconds (fire icon). | | |
| ma-23 | `co_map meadows 2 stormcalled:75`. Use it. Let an enemy hit you. | Enemy hits also add lightning damage. | | |

**C. Console, settings and old maps**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| ma-24 | Inside a rip (any one above), type `co_rip`. | The console prints the rip's tier and affixes, then `Values: …` with the numbers. | | |
| ma-25 | 1. `co_map meadows 2 brutal:100`, and keep that map. 2. Open `BepInEx/config/com.krogongames.corruption.cfg`, find `[5 - Rips]` and set `DisabledMapAffixes = brutal`, then save (or set it in the config manager, if you use one). 3. Make 4 new maps with Alchemy. 4. Use the kept Brutal map. 5. Afterwards set `DisabledMapAffixes` back to empty and save. | The new maps never get Brutal. The kept map still shows Brutal, and its rip still hits harder. The log has `Map affixes switched off (never roll): brutal.` after the save. | | |
| ma-26 | **Skip if you have none.** An old map from before this update that had Swarming, Hardened, Rabid or Smouldering. Hover it, then use it. | Those lines are gone from the tooltip, and its tier is lower by one for each removed affix. The log has `Map <name>: unknown affix id(s) … ignored.` | | |
| ma-27 | Play to the end, then quit. Check the console (F5) and the log. | No red errors in the console at any point. | | |

### Feedback

After the play-test, send Claude your LogOutput.log (BepInEx folder).

### Your feedback (Krogon)

