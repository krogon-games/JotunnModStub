# Classic World of Warcraft reference data

Design reference for Orlog's levelling pace ("slow, like classic WoW", user 2026-10-30). Used by `tools/make_orlog_tables.py`; never shipped.

| File | Contents | Source |
|---|---|---|
| `xp_for_level.csv` | XP from each level to the next, levels 1–59 (classic 1.12) | cmangos mangos-classic, `sql/base/mangos.sql`, table `player_xp_for_level` (https://github.com/cmangos/mangos-classic) |

Formulas (vmangos `src/game/Formulas.h`, https://github.com/vmangos/core/blob/development/src/game/Formulas.h; covers patches 1.2–1.12, not checked against a 1.12 client):
- A mob of the player's level gives `level * 5 + 45` XP.
- Higher mobs: `× (1 + 0.05 × min(mob − player, 4))`. Lower mobs: `× (ZD + mob − player) / ZD`, ZD from 5 (below level 8) to 17 (level 60); nothing at or below the gray level (`player − 5 − player/10` up to 39, `player − 1 − player/5` from 40; none up to level 5).
- Elite ×2 (×2.5 in a dungeon). Group bonus: 3 members ×1.166, 4 ×1.3, 5 ×1.4.

Orlog only takes WoW's **kills per level** (XP to next level ÷ XP of a same-level mob) for its pace; the level-difference and party rules are Diablo 2's.
