# Play-tests: `claude/nifty-hypatia-afm8ez`

## Round: loot map affixes (#96, 2026-11-28)

**Build:** the DLLs from this branch (Corruption, Orlog and CraftingOverhaul). The log's first Corruption line should read `Corruption <version> loaded (build: claude/nifty-hypatia-afm8ez <commit>)`. The version isn't raised on this branch; it changes when the branch merges.
**Setup:** single player, Valheim 1.0.31, a test world, the group's mods loaded as usual. Stand in the **Meadows**. Press **F5**, type `devcommands` (this turns on `co_map`, `co_rip` and the new `co_riploot`), then `god`. Keep `BepInEx/LogOutput.log` open; the lines quoted below come from it.

**What changed:**
- **Six new map affixes, all rewards** (listed under "What you'll earn"):
  - **Bountiful:** enemies are 100% more likely to drop gear, orbs and maps.
  - **Glittering:** orbs only.
  - **Bulwarked, Mailed, Honed, Fletched:** when gear drops, it is twice as likely to be a shield, helmet or armour, melee weapon, or ranged weapon. The number of gear drops stays the same.
- **Runic** (runes) is listed but never rolls until runes exist.
- Every ×2 multiplies with the tier and the stars, and with the other loot affixes on the same map. They work on kills in the rip and on the heart's spill.
- **Thorns, the first corrupted-only affix** (added to the round, #97): when a shard corrupts a full tier 5 map, it now also gets a 5th affix, "Enemies return N% of the damage you deal to them." (N from 5 to 25), shown last under "What you'll face" in red. Every hit you land on a rip creature sends that share of the damage it really did back to you, as a normal hit that armour and blocking reduce. Burning and poison ticks don't count. Orbs never roll it.
- **The weather affixes** (Frostbitten, Drowned, Choking) stay listed but switched off, and are off the plan.
- **New test command:** `co_riploot [kills] [tier] [affixes]` pretends to kill that many rip creatures in the biome you're standing in and prints what would have dropped. It drops nothing.

Chance is random, so `co_riploot`'s numbers will be a little different each time. "About double" means within roughly 10–15% of twice the first number.

### Checklist

**A. Tooltips**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-1 | Read the top of the log. | No red errors. The log has `Map tiers: 6 tiers, 28 affixes (…, stormcalled, bountiful, glittering, bulwarked, mailed, honed, fletched roll; corrupted-only: thorns).` Runic isn't in the roll list. | | |
| la-2 | `co_map meadows 5 bountiful glittering bulwarked mailed`. Open the inventory, Maps tab, and hover the map. | Under "What you'll earn", after the tier's lines: `Enemies are 100% more likely to drop gear, orbs and maps.`, `Enemies are 100% more likely to drop orbs.`, `Gear drops are 100% more likely to be shields.`, `Gear drops are 100% more likely to be helmets and armour.` Nothing new under "What you'll face". | | |
| la-3 | `co_map meadows 3 honed fletched`. Hover it. | Under "What you'll earn": `Gear drops are 100% more likely to be melee weapons.` and `Gear drops are 100% more likely to be ranged weapons.` | | |
| la-4 | `co_map meadows 2 runic`. | Refused: `Not built yet, so a map can't carry: runic.` | | |
| la-5 | Make 5 or more fresh maps with an Orb of Alchemy each (`co_map meadows`, then Alchemy at the Forge tab). | Some of them get loot affixes next to danger ones: about 1 affix in 4 is a loot one. Runic never appears. | | |

**B. The numbers (`co_riploot`, standing in the Meadows)**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-6 | `co_riploot 10000 1`, then `co_riploot 10000 1 bountiful`. | The first: Magic about 250, Rare about 125, orbs about 590, map chance `1 in 664`. The second: **Magic, Rare and orbs about double**, map chance `1 in 332`. The first line says `gear x2, orbs x2`. | | |
| la-7 | `co_riploot 10000 1 glittering`. | Magic and Rare about the same as la-6's first line. **Orbs about double** (about 1,180), map chance still `1 in 664`. | | |
| la-8 | `co_riploot 10000 3 bountiful glittering`. | The first line says `gear x2, orbs x4`. Orbs about 3,300 (tier 3's ×1.75, then ×4), Magic about 680, Rare about 350. | | |
| la-9 | `co_riploot 10000 1`, then `co_riploot 10000 1 bulwarked`. Read the "Kinds of gear" line of each. | Shields go from **about 12% to about 22%**. Magic and Rare stay about the same. | | |
| la-10 | `co_riploot 10000 1 mailed`, then `honed`, then `fletched`. | Against la-9's first line: armour about 44% → **about 61%**; melee about 38% → **about 55%**; ranged about 6% → **about 12%**. | | |

**C. In a real rip**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-11 | `co_map meadows 2 bountiful`, open its rip and fight the waves, then destroy the heart. | Creatures drop gear and orbs as usual (more than usual if you keep count). No red errors. When the heart breaks, the log has `Rip loot: tier 2.` and `Rip loot: loot affixes, gear and maps x2, orbs x2, shields x1, armour x1, melee x1, ranged x1.` | | |
| la-12 | `co_map meadows 5 glittering bulwarked mailed honed`, open its rip and destroy the heart. | The heart's log line: `… gear and maps x1, orbs x2, shields x2, armour x2, melee x2, ranged x1.` Your heart gear is still Rare or better (tier 5). No red errors. | | |
| la-13 | `co_map meadows 2 brutal`, open its rip and destroy the heart. | No `loot affixes` line in the log (a map without loot affixes changes nothing). | | |

**D. Thorns (corrupted-only)**

**Needs a build from this branch made after the Thorns commit** (if you built before it, rebuild first; A–C work on either). Get a shard first: `spawn CorruptionShard 2`, then stand at a Riftforge.

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-14 | `co_map meadows 5 brutal fleet sturdy teeming`. Put it in the Forge tab's slot, pick up a shard in Currencies and click the slot. | "Corrupted: now tier 6". The map has **5 lines**: the four, then last under "What you'll face" **in red**: `Enemies return N% of the damage you deal to them.`, N from 5 to 25. | | |
| la-15 | Pick up any orb except the Mirror and click the corrupted map. | Refused: the map is corrupted and locked. A second shard is refused too ("This map is already corrupted."). | | |
| la-16 | `co_map meadows 6`, four times (random tier 6 maps). Hover each. | Every one has 5 lines; the last is the red Thorns line, with a number from 5 to 25. Thorns never appears on a map you make with orbs (la-5). | | |
| la-17 | `co_map meadows 6 brutal fleet sturdy teeming thorns:25`. Open its rip, hover the heart. Read the log. | The hover shows "Tier 6" and five lines, Thorns in red. No `map affix 'thorns' dropped` line in the log. | | |
| la-18 | In that rip, type `god` again to turn god mode **off** (wear good armour, eat). Hit a creature a few times with a melee weapon and watch your health. | Each hit that lands costs you about a quarter of the damage number shown on the creature, a bit less thanks to your armour. A killing blow costs you too. You don't stagger from it. | | |
| la-19 | Shoot a creature with a bow (and, if you're an Amazon, use a skill on one). Then hit one with fire arrows (or another burning attack) and watch while it burns. | Arrows and skills cost you a share too. The burning ticks afterwards don't. | | |
| la-20 | In the config (`[5 - Rips] DisabledMapAffixes`), set `thorns` and save. Make a new tier 5 map and corrupt it with a shard. | Still "Corrupted: now tier 6", but the map has only 4 lines (no Thorns). Set the setting back to blank afterwards. | | |

### Your feedback

- 2026-11-28, Krogon: "did some testing, game crashed when accessing the riftforge. please take note, may need to investigate later". **Not investigated yet.** Still needed: which build (this branch or `master`, which commit), what was in the forge or the bag (a corrupted map? a map with Thorns?), which tab, and `BepInEx/LogOutput.log` from that run (copy it before the game starts again, which overwrites it). Steps marked on the phone page so far: not read yet.
- 2026-11-28, Krogon, details: build from `claude/nifty-hypatia-afm8ez` (`608bac6` in the log); the inventory was open on the **Maps tab**; nothing in the forge slot; "i approached the riftforge, clicked E to use it and the game froze." So a **freeze** (the game stopped responding), not an error. His `LogOutput.log`: every plugin loaded with no errors (`Map tiers: 6 tiers, 28 affixes (… corrupted-only: thorns)`); before it, six `Drop item $item_corruptionmap_meadows` / `drop all 1 1` lines (20:43:59–20:44:02); the last line is `Setting selected recipe 0` at 20:46:12, the game opening the crafting window, then nothing: no exception, no Corruption line. **Claude's first look (code only):** no loop in the code this branch changed runs when the Riftforge opens with nothing in the forge, so the cause may be older. Next: check whether it also freezes on `master`'s build, read Unity's own log of that run (`Player-prev.log` in `AppData/LocalLow/IronGate/Valheim`, written before the next start replaced `Player.log`), and try opening the Riftforge with the Bag tab instead of Maps.
- 2026-11-28, phone page: **all 20 steps marked Pass** (`la-1` … `la-20`). la-18's note: "its really difficult to tell if i take damage from hitting another creature with thorns on. we could potentially add a buildable dummy to the game to test damage and affixes on. an immobile target we can spawn or build". So Thorns' returned damage isn't really confirmed. The Riftforge freeze didn't happen again (decision 17).

### What changed after the feedback

- la-18: a test command gives the creature you look at any map affixes (for the game's training dummy), and each Thorns return prints its numbers. Re-test: `la-18b` below.

### Re-test (2026-11-28)

**Build:** this branch, rebuilt after the `co_stamp` commit. Single player, `devcommands`, **`god` off**, good armour and food. Stand somewhere flat.

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-18b | `spawn TrainingDummy`, look at it, `co_stamp thorns:25`. Hit it several times with a melee weapon, then shoot it with a bow. | The console says the dummy is stamped with Thorns 25. After each hit that lands, the console prints `Thorns: your hit took N health, M comes back (25%, before your armour).`, with M a quarter of N, and your health bar drops a little (less than M, thanks to your armour). Arrows print the same. `co_stamp clear` then stops it: no more lines, no damage back. | | |
