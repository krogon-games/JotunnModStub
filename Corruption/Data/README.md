# Corruption data tables

Tunable numbers for the Corruption mod, kept as spreadsheet-friendly tables. These files are the **only** place the numbers live; the design docs explain the rules and link here.

| File | What it holds |
|---|---|
| [`affixes.csv`](affixes.csv) | Every affix: names, prefix/suffix, ranges, roll weights, which items it rolls on. Column guide: [`docs/AFFIX_RULES.md` → The affix table](../../docs/AFFIX_RULES.md#the-affix-table) |
| [`drop_chances.csv`](drop_chances.csv) | Base chance of each rarity dropping |
| [`drop_modifiers.csv`](drop_modifiers.csv) | Multipliers from where the drop happens (stars, rip tiers, corruption zones, bosses, chests) |

None of them is loaded by the game yet; they are used when the loot system is built (design phase 7). Then the server's copy will be the one everybody plays with, so the group can rebalance without a new mod version.

## How to edit

1. Open the file in Excel, Google Sheets or LibreOffice (or edit it on GitHub).
2. Change values, add or delete rows. Use a dot for decimals (`1.5`, not `1,5`).
3. Save as **CSV UTF-8** (keeps names like Jötunn's intact).
4. Run `dotnet test CraftingOverhaul.Tests` (or ask Claude to). The tests read the real files and list every mistake with its file and line.

Every table has a `status` column: `approved` (the user agreed), `proposed` (awaiting review) or `draft` (starting numbers, untested).

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
