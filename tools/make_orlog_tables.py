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
- Orlog/Data/classes.csv: each Diablo 2 class's per-level and per-point gains (data/d2-reference/classes.csv, unchanged)
  and whether it is playable yet (the Amazon first, user 2026-10-30).
- Orlog/Data/amazon_skills.csv: the Amazon's 30 skills: D2's tree (tab, row, column, required level, prerequisites,
  mana) from data/d2-reference/amazon.csv, with our status, kind, weapon and Valheim description from
  docs/class-design.md §5 (user decisions 2026-10-30). Eitr costs are D2's mana: the class eitr pool is D2's mana 1:1.
- Orlog/Data/amazon_passives.csv: the chance per skill level (1-20) of the Amazon's passives, from
  data/d2-reference/amazon_levels.csv (Arreat Summit; D2R the same), with the tooltip text. Penetrate takes Critical
  Strike's curve (user, 2026-11-11: D2's Penetrate raises attack rating, which Valheim doesn't have).
- Orlog/Data/amazon_skill_values.csv: the per-level values (1-20) of the Amazon's built active skills, unchanged from
  data/d2-reference/amazon_levels.csv (D2R where it differs from classic: user 2026-11-11), keyed by skill id.
- Orlog/Data/skill_damage_scale.csv: per character level, how many Valheim damage points one Diablo 2 damage point of a
  skill is worth. It keeps D2's ratio of skill damage to bow damage: the best Valheim bow plus the best arrow obtainable
  in the biome of that level (game dump: items.csv and biome_items.csv, quality 1) divided by the average damage of
  the best D2 bow that drops by that level (data/d2-reference/bows.csv, bows and Amazon bows; available from the
  higher of item level and required level). Proposed 2026-11-11, waiting for the user's approval.
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
# The Amazon's skills in Valheim (docs/class-design.md §5): status, kind (attack = on the skill button, cast = on its
# hotkey, passive = no slot), the weapon it needs, and what it does here.
AMAZON = {
    "Magic Arrow": ("v1", "attack", "bow", "Shoots without using an arrow; its damage turns into spirit damage as it levels"),
    "Fire Arrow": ("v1", "attack", "bow", "Adds fire damage to the shot"),
    "Cold Arrow": ("v1", "attack", "bow", "Adds frost damage; the target is slowed"),
    "Multiple Shot": ("v1", "attack", "bow", "One shot splits into several arrows in a fan; uses one arrow"),
    "Exploding Arrow": ("v1", "attack", "bow", "Fire damage in an area on impact"),
    "Ice Arrow": ("v1", "attack", "bow", "Frost damage and a short freeze; bosses are only slowed"),
    "Guided Arrow": ("later", "attack", "bow", "The arrow homes in on the target"),
    "Strafe": ("later", "attack", "bow", "A volley of arrows at several nearby targets"),
    "Immolation Arrow": ("later", "attack", "bow", "Exploding Arrow plus a burning patch on the ground"),
    "Freezing Arrow": ("v1", "attack", "bow", "Ice Arrow's freeze in an area"),
    "Inner Sight": ("v1", "cast", "", "Nearby enemies glow and take more damage for a while"),
    "Critical Strike": ("v1", "passive", "", "Chance to deal double physical damage"),
    "Dodge": ("v1", "passive", "", "Chance to avoid a melee hit while standing or attacking"),
    "Slow Missiles": ("later", "cast", "", "Enemy projectiles near you slow down"),
    "Avoid": ("v1", "passive", "", "Chance to avoid a projectile while standing or attacking"),
    "Penetrate": ("reworked", "passive", "", "Chance for a hit to ignore the creature's resistance to its damage type"),
    "Decoy": ("later", "cast", "", "A tinted, standing copy of you that creatures attack instead"),
    "Evade": ("v1", "passive", "", "Chance to avoid any hit while moving"),
    "Valkyrie": ("later", "cast", "", "A tinted copy of a player character that fights for you"),
    "Pierce": ("later", "passive", "", "Chance for an arrow or javelin to pass through and hit the next enemy"),
    "Jab": ("v1", "attack", "spear", "A fast series of thrusts"),
    "Power Strike": ("v1", "attack", "spear", "Adds lightning damage to the thrust"),
    "Poison Javelin": ("v1", "attack", "spear", "The javelin (a spectral copy) leaves poison clouds along its path"),
    "Impale": ("v1", "attack", "spear", "One heavy thrust; wears the spear down faster"),
    "Lightning Bolt": ("v1", "attack", "spear", "The javelin (a spectral copy) turns into lightning"),
    "Charged Strike": ("later", "attack", "spear", "The thrust releases small lightning bolts around the target"),
    "Plague Javelin": ("v1", "attack", "spear", "A spreading poison cloud where the javelin (a spectral copy) lands"),
    "Fend": ("later", "attack", "spear", "Thrusts at several nearby enemies in turn"),
    "Lightning Strike": ("later", "attack", "spear", "The thrust releases chain lightning to nearby enemies"),
    "Lightning Fury": ("later", "attack", "spear", "The javelin (a spectral copy) bursts into lightning bolts on impact"),
}

# The Amazon's passives (roadmap C3; user decisions 2026-11-11): skill id -> (effect, the amazon_levels.csv row whose
# per-level chances it takes, tooltip text with {0} for the chance).
PASSIVES = [
    ("CriticalStrike", "crit", ("Critical Strike", "crit_chance"), "{0}% chance to deal double physical damage"),
    ("Dodge", "dodge", ("Dodge", "dodge_chance"), "{0}% chance to dodge a close-range hit while standing still or attacking"),
    ("Avoid", "avoid", ("Avoid", "dodge_missile_chance"), "{0}% chance to avoid a projectile while standing still or attacking"),
    ("Penetrate", "penetrate", ("Critical Strike", "crit_chance"),
     "{0}% chance for a hit to ignore the creature's resistances (not immunities)"),
    ("Evade", "evade", ("Evade", "evade_chance"), "{0}% chance to evade any hit while walking or running"),
]

# The damage types a hit adds up (game dump items.csv columns), for the Valheim side of the damage scale.
DAMAGE_COLUMNS = ["blunt", "slash", "pierce", "fire", "frost", "lightning", "poison", "spirit"]
# Biome order as the game dump writes it in biome_items.csv (Ashlands there, AshLands in Heightmap.Biome).
DUMP_BIOME = {"AshLands": "Ashlands"}

# Playable classes (user, 2026-10-30: the Amazon first; the Barbarian and maybe the Paladin later).
PLAYABLE = {"Amazon"}
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
    classes = read_csv(os.path.join(ROOT, "data", "d2-reference", "classes.csv"))
    with open(os.path.join(OUT, "classes.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        columns = ["str", "dex", "vit", "energy", "life_per_lvl", "stamina_per_lvl", "life_per_vit", "stamina_per_vit"]
        w.writerow(["class", "status"] + columns)
        for c in classes:
            w.writerow([c["class"], "playable" if c["class"] in PLAYABLE else "later"] + [c[k] for k in columns])
    skills = read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon.csv"))
    tabs = []
    with open(os.path.join(OUT, "amazon_skills.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "name", "tab", "row", "col", "req_level", "prereqs", "status", "kind", "needs", "eitr", "eitr_per_level",
                    "description", "d2_synergies"])
        for s in skills:
            if s["tab"] not in tabs:
                tabs.append(s["tab"])
            status, kind, needs, text = AMAZON[s["name"]]
            prereqs = "" if s["prerequisites"] == "none" else "+".join(p.replace(" ", "") for p in s["prerequisites"].split("+"))
            eitr = s["mana_base"] if kind != "passive" else "0"
            w.writerow([s["name"].replace(" ", ""), s["name"], tabs.index(s["tab"]), s["row"], s["col"], s["req_level"], prereqs,
                        status, kind, needs, eitr, s["mana_per_lvl"] if kind != "passive" else "0", text,
                        "" if s["synergies"] == "none" else s["synergies"]])
    per_level = {(r["skill"], r["value"]): r for r in read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon_levels.csv"))}
    with open(os.path.join(OUT, "amazon_passives.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "effect", "text"] + ["l%d" % i for i in range(1, 21)] + ["source"])
        for skill_id, effect, (name, value), text in PASSIVES:
            row = per_level[(name, value)]
            w.writerow([skill_id, effect, text] + [row["l%d" % i] for i in range(1, 21)] + ["%s %s (%s)" % (name, value, row["source"])])
    built_active = [s["name"] for s in skills if AMAZON[s["name"]][0] in ("v1", "reworked") and AMAZON[s["name"]][1] != "passive"]
    with open(os.path.join(OUT, "amazon_skill_values.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "value", "unit"] + ["l%d" % i for i in range(1, 21)] + ["source"])
        for r in read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon_levels.csv")):
            if r["skill"] in built_active:
                w.writerow([r["skill"].replace(" ", ""), r["value"], r["unit"]] + [r["l%d" % i] for i in range(1, 21)] + [r["source"]])
    write_damage_scale()
    print("Wrote levels.csv, creature_xp.csv, biome_levels.csv, classes.csv, amazon_skills.csv, amazon_passives.csv, "
          "amazon_skill_values.csv and skill_damage_scale.csv to", OUT)


def write_damage_scale():
    items = {r["prefab"]: r for r in read_csv(os.path.join(DUMP, "items.csv"))}
    by_biome = {}
    for r in read_csv(os.path.join(DUMP, "biome_items.csv")):
        by_biome.setdefault(r["biome"], []).append(r["item"])

    def total(prefab):
        return sum(float(items[prefab][c] or 0) for c in DAMAGE_COLUMNS)

    # Best bow and best arrow obtainable by each biome (earlier biomes' gear counts too). Crossbows use bolts and
    # "Uncooked" casts aren't weapons, so both are left out.
    best, bow, arrow = {}, 0.0, 0.0
    for biome, _, _ in GUIDE:
        for prefab in by_biome.get(DUMP_BIOME.get(biome, biome), []):
            if prefab not in items:
                continue
            kind = items[prefab]["type"]
            if kind == "Bow" and not prefab.startswith("Crossbow") and "Uncooked" not in prefab:
                bow = max(bow, total(prefab))
            elif kind == "Ammo" and prefab.startswith("Arrow"):
                arrow = max(arrow, total(prefab))
        best[biome] = (bow, arrow)
    d2_bows = [(max(int(r["item_level"]), int(r["level_req"])), (float(r["min_damage"]) + float(r["max_damage"])) / 2, r["name"])
               for r in read_csv(os.path.join(ROOT, "data", "d2-reference", "bows.csv")) if r["type"] in ("bow", "abow")]
    biome_of = []
    previous = 1
    for biome, _, boss_level in GUIDE:
        biome_of.append((previous, boss_level - 1, biome))
        previous = boss_level
    with open(os.path.join(OUT, "skill_damage_scale.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["level", "biome", "valheim_bow_arrow", "d2_bow", "d2_bow_name", "scale"])
        for level in range(1, MAX_LEVEL + 1):
            biome = next((b for lo, hi, b in biome_of if lo <= level <= hi), GUIDE[-1][0])
            valheim = sum(best[biome])
            d2 = max((b for b in d2_bows if b[0] <= level), key=lambda b: b[1])
            w.writerow([level, biome, valheim, d2[1], d2[2], round(valheim / d2[1], 3)])


if __name__ == "__main__":
    main()
