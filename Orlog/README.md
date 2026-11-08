# Orlog

Diablo 2-style classes for Valheim. Full design: `docs/class-design.md` in the repository.

**Status: levels, XP, attributes and the Orlog stone.** Skills come next. Every player and the server need the mod.

## Features so far

- **Character levels from XP**, up to level 99. Kills give XP: more for tougher creatures (by biome and health), bosses (Eikthyr is level 5 … Kall Fimbulbringer 90) and starred creatures (×2, ×3). Creatures far below your level give little, as in Diablo 2.
- **Shared XP:** players within 70 m of a kill share it, with a bonus for each extra player, split by level.
- XP stays with the character, like vanilla skills. Vanilla skills keep levelling by use. There is no XP loss on death.
- "+N XP" in the top-left corner for each kill (can be turned off); "Level N" and the skill level-up effect on a new level.
- **Attributes** (with a class; only the Amazon so far): spend 5 stat points per level on Strength (+1% melee and thrown damage each), Dexterity (+1% bow damage), Vitality (+1.5 health, +0.89 stamina) and Energy (+1.5 eitr). Each level also gives +1 health, +0.89 stamina and +1.5 eitr.
- **Eitr without eitr food:** a class starts with 15 eitr, which vanilla regeneration refills.
- **Skill tree:** the Amazon's 30 skills in Diablo 2's three tabs; spend 1 skill point per level on the 19 built ones (D2's rules: tiers, prerequisites, 20 points, one more per level past the requirement). The skills don't act yet; their effects come one by one.

- **The Orlog stone** in the spawn area, among the boss trophy stones: use it to choose your class (only the Amazon so far). With CraftingOverhaul a window opens; without it, use the stone twice.

## Configuration

`BepInEx/config/com.krogongames.orlog.cfg`. The Levels settings are server-synced: the server's values apply to everyone.

| Setting | Default | Description |
|---|---|---|
| Levels / MaxLevel | 99 | Highest level |
| Levels / StatPointsPerLevel | 5 | Attribute points per level |
| Levels / SkillPointsPerLevel | 1 | Skill points per level |
| Levels / XpMultiplier | 1 | Multiplies all XP; 1 = classic WoW's pace without quests |
| Levels / ShareRange | 70 | Metres within which players share a kill's XP |
| Levels / PartyBonusPerPlayer | 0.35 | Extra XP per additional player sharing a kill |
| Levels / HighLevelPenalty | false | Diablo 2's XP cut from level 70 |
| Display / ShowXpGains | true | Show "+N XP" per kill (per player) |
| Attributes / HealthPerD2Life | 0.5 | Valheim health per Diablo 2 life point |
| Attributes / StaminaPerD2Stamina | 0.89 | Valheim stamina per Diablo 2 stamina point |
| Attributes / EitrStart, EitrPerLevel, EitrPerEnergy | 15, 1.5, 1.5 | The class eitr pool |
| Attributes / DamagePerStrength, DamagePerDexterity | 1, 1 | % damage per point |

## Console commands (need `devcommands`, except `orlog_info`)

- `orlog_info`: your level and XP, and the creature levels in the biome you're in.
- `orlog_xp <amount>`: add XP.
- `orlog_level <level>`: set your level.
- `orlog_class Amazon` (or `none`): set the class directly (a cheat; players use the Orlog stone).
- `orlog_stat <strength|dexterity|vitality|energy> [points]`: spend stat points, until the UI exists (no `devcommands` needed).
- `orlog_skill <skill id> [points]`: spend skill points (no `devcommands` needed).
- `orlog_respec`: take back all spent stat and skill points.

## Changelog
- 0.2.0 (2026-11-08): review fixes (PR #57): stat and skill point totals can't overflow, and a lowered `StatPointsPerLevel` keeps the points already spent.
- 0.1.0 (unreleased): first version.
