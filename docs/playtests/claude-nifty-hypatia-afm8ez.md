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
- **New test command:** `co_riploot [kills] [tier] [affixes]` pretends to kill that many rip creatures in the biome you're standing in and prints what would have dropped. It drops nothing.

Chance is random, so `co_riploot`'s numbers will be a little different each time. "About double" means within roughly 10–15% of twice the first number.

### Checklist

**A. Tooltips**

| Step | What to do | What you should see | Pass / Fail | Note |
|---|---|---|---|---|
| la-1 | Read the top of the log. | No red errors. The log has `Map tiers: 6 tiers, 27 affixes (…, stormcalled, bountiful, glittering, bulwarked, mailed, honed, fletched roll).` Runic isn't in the roll list. | | |
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

### Your feedback

(none yet)
