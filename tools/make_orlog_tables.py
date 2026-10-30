"""Builds Orlog's levelling tables in Orlog/Data/ from reference data. Rerun after changing the sources or the rules.

Sources (all in this repository):
- data/d2-reference/experience.csv: Diablo 2's XP needed for each level (Arreat Summit / D2R experience table).
- data/wow-reference/xp_for_level.csv: classic WoW's XP per level (cmangos mangos-classic `player_xp_for_level`).
- data/game-dump-1.0.20/creatures.csv and spawns.csv: creature health and the world spawn lists.
- The user's levelling guide (docs/class-design.md, 2026-10-30): the level at each boss.

Outputs:
- Orlog/Data/levels.csv: level, total XP to reach it and D2's high-level XP ratio (out of 1024), D2's table unchanged.
- Orlog/Data/creature_xp.csv: the XP a creature of each level gives a player of the same level. It is D2's XP to the
  next level divided by classic WoW's kills per level (WoW's XP to next level / XP of a same-level mob, level*5+45),
  with WoW's levels 1-59 spread over our levels 1-89 (our level 90 = Kall Fimbulbringer = WoW's cap of 60). Levels
  90-98 keep WoW's last value. So a player needs as many same-level kills per level as classic WoW without quests.
  The XP never drops from one level to the next (it would at levels 8 and 10, by 1-6 XP).
- Orlog/Data/biome_levels.csv: each biome's creature level range (from the guide) and the health span of the creatures
  the world spawns there without a boss key (from the dump). Creatures get their level inside the range by health.

Usage: python3 tools/make_orlog_tables.py
"""

import csv
import math
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DUMP = os.path.join(ROOT, "data", "game-dump-1.0.20")
OUT = os.path.join(ROOT, "Orlog", "Data")

# The user's levelling guide (2026-10-30): biome, its boss prefabs, the boss's level. Biome names are Heightmap.Biome's.
GUIDE = [
    ("Meadows", ["Eikthyr"], 5),
    ("BlackForest", ["gd_king"], 11),
    ("Swamp", ["Bonemass"], 20),
    ("Mountain", ["Dragon"], 35),
    ("Plains", ["GoblinKing"], 50),
    ("Mistlands", ["SeekerQueen"], 62),
    ("AshLands", ["Fader"], 76),
    ("DeepNorth", ["FrozenKing", "FrozenKing_p2", "FrozenKing_p3"], 90),
]
MAX_LEVEL = 99
# Our level that matches WoW's level-60 cap (Kall Fimbulbringer).
WOW_CAP_LEVEL_OURS = 90


def read_csv(path):
    with open(path, encoding="utf-8") as f:
        return list(csv.DictReader(f))


def levels():
    rows = read_csv(os.path.join(ROOT, "data", "d2-reference", "experience.csv"))
    return {int(r["level"]): int(r["xp_total"]) for r in rows}, {int(r["level"]): int(r["exp_ratio"]) for r in rows}


def wow_kills_per_level():
    rows = read_csv(os.path.join(ROOT, "data", "wow-reference", "xp_for_level.csv"))
    # Classic WoW: a mob of the player's level gives level*5+45 (vmangos Formulas.h BaseGain).
    return {int(r["level"]): int(r["xp_to_next"]) / (int(r["level"]) * 5 + 45) for r in rows}


def kills_at(level, wow):
    """WoW's kills per level at the WoW level matching our level (linear between WoW levels)."""
    if level >= WOW_CAP_LEVEL_OURS - 1:
        return wow[59]
    w = 1 + (level - 1) * (59 - 1) / (WOW_CAP_LEVEL_OURS - 1 - 1)
    lo = int(math.floor(w))
    hi = min(lo + 1, 59)
    t = w - lo
    return wow[lo] * (1 - t) + wow[hi] * t


def biome_spans():
    creatures = {r["prefab"]: r for r in read_csv(os.path.join(DUMP, "creatures.csv"))}
    bosses = {b for _, names, _ in GUIDE for b in names}
    spans = {}
    for row in read_csv(os.path.join(DUMP, "spawns.csv")):
        if row["m_enabled"] != "True" or row["m_devDisabled"] == "True" or row["m_requiredGlobalKey"]:
            continue
        creature = creatures.get(row["m_prefab"])
        if not creature or creature["prefab"] in bosses or creature["faction"] in ("Players", "TrainingDummy"):
            continue
        health = float(creature["health"])
        for biome in (b.strip() for b in row["m_biome"].split(",")):
            lo, hi = spans.get(biome, (health, health))
            spans[biome] = (min(lo, health), max(hi, health))
    return spans


def main():
    os.makedirs(OUT, exist_ok=True)
    xp, ratio = levels()
    wow = wow_kills_per_level()

    with open(os.path.join(OUT, "levels.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["level", "xp_total", "exp_ratio"])
        for level in range(1, MAX_LEVEL + 1):
            w.writerow([level, xp[level], ratio[level]])

    with open(os.path.join(OUT, "creature_xp.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["level", "xp", "kills_per_level"])
        best = 0
        for level in range(1, MAX_LEVEL + 1):
            to_next = xp[level + 1] - xp[level] if level < MAX_LEVEL else xp[MAX_LEVEL] - xp[MAX_LEVEL - 1]
            kills = kills_at(level, wow)
            # A stronger creature never gives less: WoW's kill counts rise faster than D2's table at levels 8 and 10.
            best = max(best, max(1, int(round(to_next / kills))))
            w.writerow([level, best, round(kills, 1)])

    spans = biome_spans()
    with open(os.path.join(OUT, "biome_levels.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["biome", "min_level", "max_level", "health_min", "health_max", "boss_level", "bosses"])
        previous = 1
        for biome, bosses, boss_level in GUIDE:
            lo, hi = spans[biome]
            # Creatures fill the range up to one below the boss; the next biome starts at the boss's level.
            w.writerow([biome, previous, boss_level - 1, int(lo), int(hi), boss_level, ";".join(bosses)])
            previous = boss_level
    print("Wrote levels.csv, creature_xp.csv and biome_levels.csv to", OUT)


if __name__ == "__main__":
    main()
