# Orlog data tables

Tunable numbers for levels, XP and classes. Compiled into the DLL: rebuild after editing. `dotnet test CraftingOverhaul.Tests` checks them (`OrlogLevelTests`). All of them are made by `tools/make_orlog_tables.py`; edit the script's sources or rules and rerun it, or edit a table by hand for a one-off change.

| Table | One row per | Columns | Source |
|---|---|---|---|
| `levels.csv` | Level 1–99 | `xp_total` (XP to reach the level), `exp_ratio` (D2's high-level XP ratio out of 1024, used only with `HighLevelPenalty`) | Diablo 2's experience table (`data/d2-reference/experience.csv`) |
| `creature_xp.csv` | Creature level 1–99 | `xp` (what a creature of that level gives a player of the same level, before stars, sharing and the level-difference factor), `kills_per_level` (for reading only) | D2's XP to the next level ÷ classic WoW's kills per level at the matching WoW level (WoW 1–59 spread over our 1–89; 90–98 keep WoW's last value). Never lower than the level before |
| `classes.csv` | Class | `status` (`playable` or `later`), D2's starting `str`, `dex`, `vit`, `energy`, and `life_per_lvl`, `stamina_per_lvl`, `life_per_vit`, `stamina_per_vit` (D2 numbers; the scale to Valheim is in the `3 - Attributes` settings) | `data/d2-reference/classes.csv` (Arreat Summit / D2R `charstats`); the Amazon playable first (user) |
| `biome_levels.csv` | Biome | `min_level`, `max_level` (creature levels), `health_min`, `health_max` (base health of the weakest and strongest creature the world spawns there without a boss key), `boss_level`, `bosses` (prefab names, `;`-separated) | The user's levelling guide (2026-10-30) and the game dump (`creatures.csv`, `spawns.csv`) |

How a kill's XP is worked out (code: `Orlog/Rules/ExperienceRules.cs`, `Orlog/Content/KillXp.cs`):
1. **Creature level:** a listed boss has its `boss_level`. Any other creature takes the range of the biome it died in, placed by its base health on a log scale between `health_min` and `health_max` (clamped). The ocean has no range: the creature counts as the sharing players' average level.
2. **XP by level:** `creature_xp.csv` × the creature's star level (1 star = ×2, 2 stars = ×3: its health is base × level in the game code) × the `XpMultiplier` setting.
3. **Sharing (Diablo 2):** players within `ShareRange` share it; with more than one, the total grows by `PartyBonusPerPlayer` per extra player (Arreat says +35% "when a party member is in the area"; per extra player is our reading) and is split by level.
4. **Level difference:** each share is of what the kill is worth to that player. A creature at or below the player's level: its own XP × D2's factor (100% within 5 levels, then 81% down to 5%; `data/d2-reference/README.md`). A creature above: the XP of a creature of the player's level × classic WoW's bonus (+5% per level above, max +20%; `data/wow-reference/README.md`). Changed 2026-10-30: D2's full XP for higher creatures made early levels 2–3× too fast.
