# Diablo 2 reference data (Barbarian, Amazon, Paladin)

Design reference for [`docs/class-design.md`](../../docs/class-design.md). Collected 2026-10-30 from the D2R game tables, cross-checked with the Arreat Summit (classic 1.10–1.13) and Maxroll's 2.4 patch notes. This is Blizzard's game data: fine in this private repository for design, but never ship it. Our own tunable tables will live under the classes plugin's `Data/` folder.

## Files

| File | Contents |
|---|---|
| `classes.csv` | Starting attributes, starting life/stamina/mana, gains per level and per point of Vitality/Energy (charstats values ÷ 4, matching Arreat's class pages) |
| `experience.csv` | XP needed to reach each level 1–99 and the high-level XP ratio (out of 1024; 976 = 95.31% at level 70). From D2R's `experience.txt` (d2data `experience.json`), checked against Arreat's "Experience Levels" page. Used by `tools/make_orlog_tables.py` |
| `barbarian.csv`, `amazon.csv`, `paladin.csv` | All 30 skills per class: tab, tree position (`row` 1–6 = level tiers 1/6/12/18/24/30, `col` 1–3), required level, direct prerequisites (`+` = both), type, mana at level 1 and per level, synergies (bonus per hard point in the named skill), one-line description |

Mana cost formula: `(mana + lvlmana*(slvl-1)) * 2^manashift / 256` (checked against Arreat: Whirlwind 12.5 / 13 / 13.5).

## General rules

| Rule | Value |
|---|---|
| Max level | 99 |
| Points per level | 5 stat points, 1 skill point |
| Quest skill points (per difficulty) | Den of Evil +1, Radament's Lair +1, The Fallen Angel (Izual) +2: 4 per difficulty, 12 in total |
| Quest stat points (per difficulty) | Lam Esen's Tome +5, 15 in total |
| Totals at level 99 | 110 skill points, 505 stat points |
| Hard points per skill | At most 20; beyond that only from items, Battle Command (+1) or shrines |
| Pace | A skill can take one more point per character level past its required level |
| +skills from items | A specific +skill grants the skill even at 0 points (prerequisites stay locked). +class, +all and +tab skills only raise skills that have at least 1 point. Other classes' +skills (oskills) cap at +3 for your own class's skills |
| Synergies (since 1.10) | Only hard points count; several synergies on one skill add into one % bonus |
| Respec | Akara's reset (once per difficulty) or a Token of Absolution: 4 essences, one from each Hell act boss group (Andariel/Duriel, Mephisto, Diablo, Baal) |

## Version notes

- Current D2R data (after Reign of the Warlock; exact patch unverified). Prerequisites and required levels are the same as classic for all 90 skills.
- Barbarian 2.4 changes: Sword Mastery renamed Blade Mastery; Berserk's Shout synergy became Battle Orders; Grim Ward's and Frenzy's Increased Stamina synergies added; Leap Attack mana 9 → 10; War Cry mana per level 1 → 0.75. Unverified: Find Item's Find Potion synergy; Frenzy 1.5 and Double Throw 1 mana (Arreat: 3 and 2).
- Amazon: classic synergies differed (Power/Charged/Lightning Strike +10%/+8% from each lightning skill; Multiple Shot, Guided Arrow and Strafe had none). Valkyrie takes the Amazon's hard levels in Critical Strike, Dodge, Avoid and Evade.
- Paladin: classic Holy Bolt synergies Blessed Hammer/FoH +50%, Prayer +15%; Holy Fire Resist Fire +18%, Salvation +6%. Sanctuary's mana value of 1 in the data is read as 0 (auras cost no mana; unverified).

## Experience rules (Arreat Summit, "Experience")

- Below character level 25, a monster's XP by (monster level − character level): ±5 levels 100%; +6 88%, +7 68%, +8 36%, +9 15%, +10 2%, more than 10 above 2%; −6 81%, −7 62%, −8 43%, −9 24%, −10 or lower 5%.
- From level 25: any higher monster gives XP × (character level ÷ monster level); lower monsters as above.
- From level 70 the XP is cut by the ratio in `experience.csv` (95.31% at 70 down to 0.49% at 99).
- Party: the total XP grows by 35% when a party member of the killer is in the same area, is shared by level (your level ÷ the sum of levels), then each share gets the level-difference factor. Only members within 2 screens get any.
- Only the killing blow's player gets XP when not in a party. Hirelings' and minions' kills count for their owner.

## Sources

- D2R tables: https://github.com/blizzhackers/d2data (`json/skills.json`, `skilldesc.json`, `charstats.json`, `allstrings-eng.json`)
- Arreat Summit: https://classic.battle.net/diablo2exp/ (class pages, skill tab pages, `skills/basics.shtml`, `quests/rewards.shtml`, `basics/characters.shtml`, `basics/levels.shtml`, `basics/experience.shtml`)
- D2R 2.4 patch notes: https://maxroll.gg/d2/news/d2r-patch-2-4-final-patch-notes
