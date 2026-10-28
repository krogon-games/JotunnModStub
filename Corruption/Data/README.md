# Corruption data tables

Tunable numbers for the Corruption mod, kept as spreadsheet-friendly tables. These files are the **only** place the numbers live; the design docs explain the rules and link here.

| File | What it holds |
|---|---|
| [`affixes.csv`](affixes.csv) | Every affix: names, prefix/suffix, ranges, roll weights, which items it rolls on. Column guide: [`docs/AFFIX_RULES.md` → The affix table](../../docs/AFFIX_RULES.md#the-affix-table) |
| [`affix_caps.csv`](affix_caps.csv) | The most a stat can gain from all affixes together (movement speed, attack speed, pickup radius, resistances, life steal) |
| [`drop_chances.csv`](drop_chances.csv) | Base chance of each rarity dropping |
| [`drop_modifiers.csv`](drop_modifiers.csv) | Multipliers from where the drop happens (stars, rip tiers, corruption zones, bosses, chests) |
| [`corruption_outcomes.csv`](corruption_outcomes.csv) | What happens when a shard corrupts an item: bricked, unchanged (locked) or one corrupted affix |
| [`essences.csv`](essences.csv) | Boss essences: which boss drops which essence, how many per kill, its name, description and colours |
| [`maps.csv`](maps.csv) | Maps: one per biome, its Riftforge recipe (shards, essence, material) and text |

`essences.csv` and `maps.csv` are compiled into the mod: **rebuild after editing them**. The others aren't loaded by the game yet; they are used when the loot system is built (design phase 7). Then the server's copy will be the one everybody plays with, so the group can rebalance without a new mod version.

## How to edit

1. Open the file in Excel, Google Sheets or LibreOffice (or edit it on GitHub).
2. Change values, add or delete rows. Use a dot for decimals (`1.5`, not `1,5`).
3. Save as **CSV UTF-8** (keeps names like Jötunn's intact).
4. Run `dotnet test CraftingOverhaul.Tests` (or ask Claude to). The tests read the real files and list every mistake with its file and line.

Every table has a `status` column: `approved` (the user agreed), `proposed` (awaiting review) or `draft` (starting numbers, untested).

## affix_caps.csv

| Column | Meaning |
|---|---|
| `cap` | Name of the cap |
| `max` | The highest total all affixes together can give |
| `unit` | Must match the unit of the affixes it limits (the test checks) |
| `families` | Affix families (from `affixes.csv`) that add up towards this cap, separated by `;`. A family can be in only one cap |
| `status`, `notes` | As above |

## drop_chances.csv

| Column | Meaning |
|---|---|
| `rarity` | `magic`, `rare` or `unique`. A new rarity needs code |
| `one_in` | Base chance as "1 in N": `50` means 1 in 50 (2%). Lower = more drops |
| `status`, `notes` | As above |

## drop_modifiers.csv

| Column | Meaning |
|---|---|
| `source` | A situation that changes drop chances, e.g. `star_2`, `rip_tier_3`, `scar`. Lowercase, digits, `_`. A **new** source needs code to detect it; changing numbers doesn't |
| `magic`, `rare`, `unique` | Multiplier for that rarity: `2` doubles the chance, `0.5` halves it. **Blank = no change** |
| `status`, `notes` | As above |

**How a drop chance is worked out:** base chance × the multiplier of every source that applies, capped at 100%.
Example: a 2-star creature (`star_2`, ×1.5 for every rarity) inside a scar (`scar`, magic and rare ×1.5, unique blank) has a Magic chance of 1/50 × 1.5 × 1.5 = 1 in about 22, and a unique chance of 1/2500 × 1.5 = 1 in about 1,667.

**Roll rules (decided 2026-10-27):** each player near a kill rolls separately (personal loot). Magic and Rare roll separately, but if both succeed only the Rare drops. Uniques roll on their own, on top. Code: `DropTable.Roll` in `Corruption/Rules/DropTable.cs`.

## corruption_outcomes.csv

| Column | Meaning |
|---|---|
| `outcome` | `bricked` (item destroyed), `unchanged` (nothing added, item corrupted and locked) or `affix` (one corrupted affix on top of the normal limit, item locked). One row each; a new outcome needs code |
| `chance` | Percentage. The three must add up to 100 (the test checks) |
| `status`, `notes` | As above |

Rules: [`docs/corruption-design.md` → Corrupted gear](../../docs/corruption-design.md#corrupted-gear). Code: `CorruptionOutcomes.Roll` in `Corruption/Rules/CorruptionOutcomes.cs`.

## essences.csv

| Column | Meaning |
|---|---|
| `boss` | The boss's prefab name, e.g. `Eikthyr`, `gd_king` (the Elder), `Dragon` (Moder), `GoblinKing` (Yagluth), `SeekerQueen`, `Fader`, `FrozenKing_p3` (Kall Fimbulbringer's final phase). The tests check it's a boss in the game data |
| `look` | Short id, lowercase. Names the item (`CorruptionEssence_<look>`) and its texture files; a **new** look needs textures from `tools/make_essences.py` |
| `name`, `description` | In-game text |
| `glow` | Colour of the gem's glow and ground light, like `#8CD9FF` |
| `addon`, `addon_color` | Optional extra part (`essence_root`, `essence_ring`, `essence_crown`) and its colour; leave both blank for none |
| `per_kill` | Essences dropped per boss kill (every kill), 1 or more |
| `maps_into` | Biome the essence will craft maps into (used from phase 4) |
| `status`, `notes` | As above |

## maps.csv

| Column | Meaning |
|---|---|
| `biome` | The game's biome name: `Meadows`, `BlackForest`, `Swamp`, `Mountain`, `Plains`, `Mistlands`, `AshLands`, `DeepNorth`. One row each. Names the item (`CorruptionMap_<biome>`) |
| `label` | The biome's name for players, e.g. `Black Forest` |
| `name`, `description` | In-game text |
| `essence` | The essence in the recipe, by its `look` in `essences.csv` (e.g. `elder`) |
| `material`, `material_amount` | The biome material in the recipe, by item prefab name (e.g. `GreydwarfEye`), and how many. The tests check it's a real item |
| `shards` | Corruption shards in the recipe |

How the current values were chosen (from the 1.0.20 game data, not guessed): shards and essence 1 each from the design doc; the material the design doc names, or where it gives two the earlier drop, or where it names none the guaranteed non-food drop of the weakest creature found only in that biome; the amount is the median amount of that material in vanilla recipes and pieces, rounded up. Each row's `notes` says which applied.
| `status`, `notes` | As above |
