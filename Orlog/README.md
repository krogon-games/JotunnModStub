# Orlog

Diablo 2-style classes for Valheim. Full design: `docs/class-design.md` in the repository.

**Status: levels and XP only.** Classes, attributes and skills come next. Every player and the server need the mod.

## Features so far

- **Character levels from XP**, up to level 99. Kills give XP: more for tougher creatures (by biome and health), bosses (Eikthyr is level 5 … Kall Fimbulbringer 90) and starred creatures (×2, ×3). Creatures far below your level give little, as in Diablo 2.
- **Shared XP:** players within 70 m of a kill share it, with a bonus for each extra player, split by level.
- XP stays with the character, like vanilla skills. Vanilla skills keep levelling by use. There is no XP loss on death.
- "+N XP" in the top-left corner for each kill (can be turned off); "Level N" and the skill level-up effect on a new level.
- Stat and skill points are counted (5 and 1 per level) but can't be spent yet.

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

## Console commands (need `devcommands`, except `orlog_info`)

- `orlog_info`: your level and XP, and the creature levels in the biome you're in.
- `orlog_xp <amount>`: add XP.
- `orlog_level <level>`: set your level.
