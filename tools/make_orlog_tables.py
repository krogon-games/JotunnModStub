"""Builds Orlog's levelling tables in Orlog/Data/ from reference data. Rerun after changing the sources or the rules.

Sources (all in this repository):
- data/d2-reference/experience.csv: Diablo 2's XP needed for each level (Arreat Summit / D2R experience table).
- data/wow-reference/xp_for_level.csv: classic WoW's XP per level (cmangos mangos-classic `player_xp_for_level`).
- data/game-dump-1.0.31/creatures.csv and spawns.csv: creature health and the world spawn lists.
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
- Orlog/Data/amazon_skills.csv: the Amazon's 27 skills, nine per tab in card order (user, 2026-11-15: no tiers, no
  prerequisites), D2's mana from data/d2-reference/amazon.csv where D2 has the skill, with our status, kind, weapon and
  Valheim description from docs/class-design.md §5. Eitr costs are D2's mana: the class eitr pool is D2's mana 1:1.
- Orlog/Data/amazon_passives.csv: the chance per skill level (1-20) of the Amazon's passives, from
  data/d2-reference/amazon_levels.csv (Arreat Summit; D2R the same), with the tooltip text. Penetrate takes Critical
  Strike's curve (user, 2026-11-11: D2's Penetrate raises attack rating, which Valheim doesn't have).
- Orlog/Data/amazon_skill_values.csv: the per-level values (1-20) of the Amazon's built active skills, from
  data/d2-reference/amazon_levels.csv (D2R where it differs from classic: user 2026-11-11), keyed by skill id. A skill
  D2 doesn't have (Forked Lightning, Wind Lance) takes the numbers the user agreed for it (NEW_SKILLS).
  Skill damage scales off the weapon (user, 2026-11-16, issue #72): a skill's damage is a share of the shot's own
  damage (bow + arrow), in a `..._pct` row; Diablo 2's fixed damage points of a built skill are left out
  (WEAPON_SHARES). The table that turned D2 points into Valheim points (skill_damage_scale.csv) is gone.
- Orlog/Data/biome_levels.csv: each biome's creature level range (from the guide) and the health span of the creatures
  the world spawns there without a boss key (from the dump). Creatures get their level inside the range by health.

Usage: python3 tools/make_orlog_tables.py
"""

import csv
import math
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DUMP = os.path.join(ROOT, "data", "game-dump-1.0.31")
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
# Skill points for a character's first kill of each biome's boss, in GUIDE order (user, 2026-11-11: D2's 12 quest points,
# 1 each for the first four bosses, 2 each for the last four).
BOSS_SKILL_POINTS = [1, 1, 1, 1, 2, 2, 2, 2]
# Boss XP (user, 2026-11-15: "follow diablo 2 style"): a boss gives ×N an ordinary creature of its level, N = a D2 act
# boss's Exp ÷ an ordinary monster's (median 97, data/d2-reference/README.md). D2 has 5 act bosses, Valheim 8: the first
# and last match, the others are read off the line through D2's five by position.
D2_ORDINARY_EXP = 97


def boss_xp_multipliers():
    exp = [float(r["exp"]) for r in sorted(read_csv(os.path.join(ROOT, "data", "d2-reference", "act_bosses.csv")),
                                            key=lambda r: int(r["act"]))]
    d2 = [e / D2_ORDINARY_EXP for e in exp]
    result = []
    for i in range(len(GUIDE)):
        p = i * (len(d2) - 1) / (len(GUIDE) - 1)
        a = min(int(p), len(d2) - 2)
        result.append(round(d2[a] + (d2[a + 1] - d2[a]) * (p - a), 1))
    return result


def boss_xp_shares(bosses):
    """A boss with several phases (Kall) pays all its XP on the final phase's kill, the earlier phases nothing
    (user, 2026-11-15: "for Kall all the xp should be awarded on final kill only"). GUIDE lists the phases in order."""
    return ";".join("1" if i == len(bosses) - 1 else "0" for i in range(len(bosses)))
# The Amazon's skills (docs/class-design.md §5). Since the skill tree overhaul (user, 2026-11-15) a tab is not a tree:
# no level tiers, no prerequisites, exactly nine skills per tab, shown as cards three by three in the order below.
# Each: name, status, kind (attack = on the skill button, cast = on its hotkey, passive = no slot), the weapon it needs,
# what it does here. A skill Diablo 2 also has (the same name in data/d2-reference/amazon.csv) takes D2's mana as its
# eitr cost; a new one takes the numbers the user agreed for it (NEW_SKILLS below), and one not agreed yet has no cost
# until the round that builds it.
AMAZON = [
    ("Bow and Crossbow", [
        ("Forked Lightning", "v1", "attack", "bow", "The arrow hits an enemy, then lightning forks from its body to nearby enemies"),
        ("Wind Lance", "v1", "attack", "bow", "A charged shot that turns the arrow into a javelin of wind: fast and dead straight with no drop, passing through enemies and knocking them back"),
        ("Snipe", "v1", "attack", "bow", "Charge a powerful shot: the longer the charge, the more damage. Drains stamina while charging and slowly zooms in on the target"),
        ("Multishot", "v1", "attack", "bow", "One shot splits into several arrows in a fan; uses one arrow"),
        ("Strafe", "v1", "attack", "bow", "Hold to rapid-fire: the bow draws once (a crossbow needs only to be loaded), then keeps firing where you aim for as long as you hold"),
        ("Freezing Arrow", "v1", "attack", "bow", "Frost damage and a short freeze in an area on impact; bosses are only slowed"),
        ("Immolation Arrow", "v1", "attack", "bow", "Explodes in fire on impact and leaves the ground burning"),
        ("Venom Strike", "v1", "attack", "bow", "The arrow bursts into a poison cloud where it lands, poisoning everything that stands in it"),
        ("Shackle Shot", "v1", "attack", "bow", "Shackles the target to an enemy or a tree behind it, stunning both; with nothing behind it, only briefly"),
    ]),
    ("Passive and Magic", [
        ("Inner Sight", "v1", "cast", "", "Nearby enemies glow and take more damage for a while"),
        ("Critical Strike", "v1", "passive", "", "Chance to deal double physical damage"),
        ("Penetrate", "reworked", "passive", "", "Chance for a hit to ignore the creature's resistance to its damage type"),
        ("Dodge", "v1", "passive", "", "Chance to avoid a melee hit while standing or attacking"),
        ("Avoid", "v1", "passive", "", "Chance to avoid a projectile while standing or attacking"),
        ("Evade", "v1", "passive", "", "Chance to avoid any hit while moving"),
        ("Decoy", "later", "cast", "", "A tinted, standing copy of you that creatures attack instead"),
        ("Valkyrie", "later", "cast", "", "A tinted copy of a player character that fights for you"),
        ("Pierce", "later", "passive", "", "Chance for an arrow or javelin to pass through and hit the next enemy"),
    ]),
    ("Javelin and Spear", [
        ("Jab", "v1", "attack", "spear", "A fast series of thrusts"),
        ("Impale", "v1", "attack", "spear", "One heavy thrust; wears the spear down faster"),
        ("Fend", "later", "attack", "spear", "Thrusts at several nearby enemies in turn"),
        ("Charged Strike", "later", "attack", "spear", "The thrust releases small lightning bolts around the target"),
        ("Lightning Strike", "later", "attack", "spear", "The thrust releases chain lightning to nearby enemies"),
        ("Lightning Bolt", "v1", "attack", "spear", "The javelin (a spectral copy) turns into lightning"),
        ("Poison Javelin", "v1", "attack", "spear", "The javelin (a spectral copy) leaves poison clouds along its path"),
        ("Plague Javelin", "v1", "attack", "spear", "A spreading poison cloud where the javelin (a spectral copy) lands"),
        ("Lightning Fury", "later", "attack", "spear", "The javelin (a spectral copy) bursts into lightning bolts on impact"),
    ]),
]
# Our name for a Diablo 2 skill where it differs (user, 2026-11-15: "Change the name to Multishot"). The id, the mana and
# the per-level values stay the D2 skill's, so saved characters keep their points and slots.
D2_NAMES = {"Multishot": "Multiple Shot"}
SKILLS_PER_TAB = 9
SKILL_COLS = 3

# Skills Diablo 2 doesn't have, with the numbers the user agreed (2026-11-16, docs/handoffs/claude-jolly-albattani-4fruv2.md).
# name -> the D2 skill whose mana it takes (eitr, 1:1), then its rows of amazon_skill_values.csv:
#   (value, unit, ("d2", D2 skill, D2 value, add)) copies that row of data/d2-reference/amazon_levels.csv, plus `add`;
#   (value, unit, ("flat", number)) is the same at every level;
#   (value, unit, ("line", level 1, level 20)) rises evenly from level 1 to level 20, to whole numbers;
#   (value, unit, ("line", level 1, level 20, date, decimals)) the same, rounded to that many decimals.
NEW_SKILLS = {
    # Decisions 3-5: Charged Strike's numbers ("Charged Strike's"), forks reach 4 m ("4 m"), and the struck enemy
    # takes one fork itself.
    "Forked Lightning": ("Charged Strike", [
        ("mana_cost", "flat", ("d2", "Charged Strike", "mana_cost", 0)),
        # 2026-11-16, issue #72 (docs/handoffs/claude-magical-babbage-slwwpn.md, decisions 3 and 5): each fork deals a
        # share of the shot's damage, "25% at level 1, 100% at 20", instead of Charged Strike's 1-30 to 1-322 points.
        ("fork_damage_pct", "%", ("line", 25, 100)),
        ("forks", "count", ("d2", "Charged Strike", "bolts", 0)),
        ("reach_m", "m", ("flat", 4)),
    ]),
    # Decisions 6-12: Guided Arrow's numbers ("Guided Arrow's numbers": the arrow's own damage, +7% per level, D2R);
    # the push from 100 (the Staff of Embers' m_attackForce) to 200 (the iron sledge's; game dump item_details.csv)
    # ("Grows with skill level"; kept after the play-test, with a stagger added: "Keep 100 to 200, add stagger");
    # twice a fully drawn arrow's speed ("Twice as fast"); gone after 50 m ("50 m").
    # After the first play-test (decisions 17 and 20): 1.5 s of holding past full draw ("lets reduce it to 1.5
    # seconds"; it was 3), and a gust 0.75 m wide each side of its centre line ("0.75 m each side").
    "Wind Lance": ("Guided Arrow", [
        ("mana_cost", "flat", ("d2", "Guided Arrow", "mana_cost", 0)),
        ("weapon_damage_pct", "%", ("d2", "Guided Arrow", "damage_pct", 100)),
        ("knockback", "flat", ("line", 100, 200)),
        ("hold_s", "s", ("flat", 1.5)),
        ("width_m", "m", ("flat", 0.75)),
        ("speed_pct", "%", ("flat", 200)),
        ("range_m", "m", ("flat", 50)),
    ]),
    # Strafe, remade after Diablo 3's Rapid Fire (user, 2026-11-17, docs/handoffs/claude-elegant-maxwell-e7ys9o.md,
    # decisions 1-14; it replaced the volley of decision 8 in claude-magical-babbage-slwwpn.md). Hold the skill button:
    # the bow draws once, then fires while held ("Draw once, then rapid fire"), "6 a second", each arrow "25% -> 50% at
    # level 20" of the shot's damage, the quiver losing "One per second". Eitr: 10 to start and 3 a second while
    # firing (Rapid Fire's "10 initially, and an additional 3 while channeling", "3 eitr per second"), both growing
    # ("Both grow a little": start 10 -> 15, drain 3 -> 5 at level 20). Not a D2 skill any more: no D2 rows, own eitr.
    "Strafe": (None, [
        ("mana_cost", "flat", ("line", 10, 15, "2026-11-17")),
        ("eitr_per_s", "flat", ("line", 3, 5, "2026-11-17")),
        ("weapon_damage_pct", "%", ("line", 25, 50, "2026-11-17")),
        ("arrows_per_s", "count", ("flat", 6, "2026-11-17")),
        # Crossbows too (decisions 15-16): "Fewer, same share", 3 bolts a second.
        ("crossbow_per_s", "count", ("flat", 3, "2026-11-17")),
        ("ammo_per_s", "count", ("flat", 1, "2026-11-17")),
    ]),
    # Snipe (user, 2026-11-18, docs/handoffs/claude-amazon-snipe-shackle-shot-m67buh.md, decisions 2-10): a charged
    # shot whose charge counts from full draw and fills over "3.5s lvl1 - 1.5s lvl 20" (charge_s); its damage at full
    # charge "150% -> 300%" of the shot's, climbing on a square curve from a normal arrow (100%) at no charge ("Slow
    # start, big finish", "Square curve; pay on any charge"); "3x at full charge" zoom; up to twice an arrow's speed at
    # full charge ("Faster with the charge", Wind Lance's speed_pct); eitr "9, +0.25 per level" = Freezing Arrow's
    # mana. Its stamina drain (the bow's drawing rate, decision 8) comes from the game's bows, not a table.
    "Snipe": ("Freezing Arrow", [
        ("mana_cost", "flat", ("d2", "Freezing Arrow", "mana_cost", 0)),
        ("weapon_damage_pct", "%", ("line", 150, 300, "2026-11-18")),
        ("charge_s", "s", ("line", 3.5, 1.5, "2026-11-18", 1)),
        ("zoom_x", "x", ("flat", 3, "2026-11-18")),
        ("speed_pct", "%", ("flat", 200, "2026-11-18")),
    ]),
    # Shackle Shot, after Dota 2's Windranger's Shackleshot (user, 2026-11-18, same handoff, decisions 11-17): the
    # struck creature is shackled to "Another enemy or a tree" behind it, within reach_m ("8 m", the game's harpoon
    # line: Harpooned m_breakDistance) and cone_deg of the line from the shooter (Dota's shackle_angle 23); both are
    # stunned for shackle_s ("1.75 s -> 4 s": Dota's stun_duration 1.75 at its level 1, 3.25 + its 0.75 s talent at
    # our 20), or the struck one alone for alone_s (Dota's fail_stun_duration 0.6; Valve's hero data). Bosses are only
    # slowed ("Bosses only slowed"). The arrow deals "100% -> 150%" of the shot's damage; eitr "9, +0.25 per level".
    "Shackle Shot": ("Freezing Arrow", [
        ("mana_cost", "flat", ("d2", "Freezing Arrow", "mana_cost", 0)),
        ("weapon_damage_pct", "%", ("line", 100, 150, "2026-11-18")),
        ("shackle_s", "s", ("line", 1.75, 4, "2026-11-18", 2)),
        ("alone_s", "s", ("flat", 0.6, "2026-11-18")),
        ("reach_m", "m", ("flat", 8, "2026-11-18")),
        ("cone_deg", "deg", ("flat", 23, "2026-11-18")),
    ]),
    # Venom Strike is the user's own skill (2026-11-16, docs/handoffs/claude-magical-babbage-slwwpn.md, decisions 10 and 11):
    # "25% at level 1, 100% at 20"; eitr "7, +0.5 per level" = Plague Javelin's mana. A poison cloud 3 m in every
    # direction hangs for 4 seconds where the arrow lands; everything inside is poisoned and re-poisoned each second.
    "Venom Strike": ("Plague Javelin", [
        ("mana_cost", "flat", ("d2", "Plague Javelin", "mana_cost", 0)),
        ("cloud_damage_pct", "%", ("line", 25, 100)),
        ("cloud_duration_s", "s", ("flat", 4)),
        ("radius_m", "m", ("flat", 3)),
    ]),
}


# Diablo 2 skills whose damage is a share of the shot's own damage instead of D2's fixed points (user, 2026-11-16, issue
# #72: "All scale off the weapon"; docs/handoffs/claude-magical-babbage-slwwpn.md, decisions 3 and 4).
# name -> (the D2 rows left out, the rows added: as in NEW_SKILLS).
WEAPON_SHARES = {
    # Decision 4: the frost blast deals "50% at level 1, 150% at 20" of the shot's damage.
    "Freezing Arrow": (["cold_damage_min", "cold_damage_max"], [
        ("blast_damage_pct", "%", ("line", 50, 150)),
    ]),
    # Immolation Arrow (decision 9): "Blast 25%->100%, fire 10%->30% a second". Explodes in fire on impact within 3 m,
    # then the ground there burns for 4 seconds (D2's burn_duration_s, kept) and hurts enemies in it every second.
    "Immolation Arrow": (["explosion_fire_damage_min", "explosion_fire_damage_max", "burn_fire_dps_min", "burn_fire_dps_max"], [
        ("blast_damage_pct", "%", ("line", 25, 100)),
        ("burn_damage_pct", "%", ("line", 10, 30)),
        ("radius_m", "m", ("flat", 3)),
    ]),
}


def number(value):
    """A number as the tables write it: whole when it is whole."""
    return str(int(value)) if float(value) == int(value) else ("%g" % value)


def own_eitr(name):
    """A new skill with no D2 skill behind its cost (NEW_SKILLS[name][0] is None): its eitr at level 1 and the average
    rise per level, from its own mana_cost row (the row is what the game uses; these columns are only a fallback)."""
    rule = [r for v, _, r in NEW_SKILLS[name][1] if v == "mana_cost"][0]
    first, last = (rule[1], rule[1]) if rule[0] == "flat" else (rule[1], rule[2])
    return {"mana_base": number(first), "mana_per_lvl": number(round((last - first) / 19.0, 3))}


def new_skill_rows(name, per_level, rules=None):
    """The amazon_skill_values.csv rows of a skill in NEW_SKILLS (or the given rules, for WEAPON_SHARES)."""
    rows = []
    for value, unit, rule in (rules if rules is not None else NEW_SKILLS[name][1]):
        if rule[0] == "d2":
            source = per_level[(rule[1], rule[2])]
            levels = [number(float(source["l%d" % i]) + rule[3]) for i in range(1, 21)]
            note = "%s %s (%s)%s" % (rule[1], rule[2], source["source"], " +%d" % rule[3] if rule[3] else "")
        elif rule[0] == "flat":
            levels = [number(rule[1])] * 20
            note = "user " + (rule[2] if len(rule) > 2 else "2026-11-16")
        else:
            decimals = rule[4] if len(rule) > 4 else 0
            levels = [number(round(rule[1] + (rule[2] - rule[1]) * (i - 1) / 19.0, decimals) if decimals else round(rule[1] + (rule[2] - rule[1]) * (i - 1) / 19.0))
                      for i in range(1, 21)]
            note = "user " + (rule[3] if len(rule) > 3 else "2026-11-16")
        rows.append([name.replace(" ", ""), value, unit] + levels + [note])
    return rows

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

# Each class's line in the class choice window (the classes UI demo's text, seen in the play-tests since 2026-10-30;
# moved here when the demo was removed, user 2026-11-13). Classes without one show none.
CLASS_DESCRIPTIONS = {
    "Amazon": "A huntress of bow and javelin. Fast, precise and hard to hit.",
}

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
        w.writerow(["biome", "min_level", "max_level", "health_min", "health_max", "boss_level", "bosses", "boss_skill_points",
                    "boss_xp", "boss_xp_shares"])
        boss_xp = boss_xp_multipliers()
        previous = 1
        for i, (biome, bosses, boss_level) in enumerate(GUIDE):
            lo, hi = spans[biome]
            # Creatures fill the range up to one below the boss; the next biome starts at the boss's level.
            w.writerow([biome, previous, boss_level - 1, int(lo), int(hi), boss_level, ";".join(bosses), BOSS_SKILL_POINTS[i],
                        boss_xp[i], boss_xp_shares(bosses)])
            previous = boss_level
    classes = read_csv(os.path.join(ROOT, "data", "d2-reference", "classes.csv"))
    with open(os.path.join(OUT, "classes.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        columns = ["str", "dex", "vit", "energy", "life_per_lvl", "stamina_per_lvl", "life_per_vit", "stamina_per_vit"]
        w.writerow(["class", "status"] + columns + ["description", "tab0", "tab1", "tab2"])
        for c in classes:
            # The skill tree's three tab names in D2's order (data/d2-reference/<class>.csv), where the tree is collected.
            tabs = []
            tree_file = os.path.join(ROOT, "data", "d2-reference", c["class"].lower() + ".csv")
            if os.path.exists(tree_file):
                for s in read_csv(tree_file):
                    if s["tab"] not in tabs:
                        tabs.append(s["tab"])
            tabs = (tabs + ["", "", ""])[:3]
            w.writerow([c["class"], "playable" if c["class"] in PLAYABLE else "later"] + [c[k] for k in columns]
                       + [CLASS_DESCRIPTIONS.get(c["class"], "")] + tabs)
    d2 = {s["name"]: s for s in read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon.csv"))}
    d2_tabs = []
    for s in d2.values():
        if s["tab"] not in d2_tabs:
            d2_tabs.append(s["tab"])
    assert [tab for tab, _ in AMAZON] == d2_tabs, "the Amazon's tabs keep Diablo 2's names and order"
    built_active = []
    with open(os.path.join(OUT, "amazon_skills.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "name", "tab", "row", "col", "status", "kind", "needs", "eitr", "eitr_per_level", "description"])
        for tab, (_, tab_skills) in enumerate(AMAZON):
            assert len(tab_skills) == SKILLS_PER_TAB, "every tab has exactly nine skills (user, 2026-11-15)"
            for i, (name, status, kind, needs, text) in enumerate(tab_skills):
                d2_name = D2_NAMES.get(name, name)
                source = (own_eitr(name) if NEW_SKILLS[name][0] is None else d2.get(NEW_SKILLS[name][0])) if name in NEW_SKILLS else d2.get(d2_name)
                costs = kind != "passive" and source is not None
                w.writerow([d2_name.replace(" ", ""), name, tab, i // SKILL_COLS + 1, i % SKILL_COLS + 1, status, kind, needs,
                            source["mana_base"] if costs else "0", source["mana_per_lvl"] if costs else "0", text])
                if status in ("v1", "reworked") and kind != "passive":
                    built_active.append(d2_name)
    per_level = {(r["skill"], r["value"]): r for r in read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon_levels.csv"))}
    with open(os.path.join(OUT, "amazon_passives.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "effect", "text"] + ["l%d" % i for i in range(1, 21)] + ["source"])
        for skill_id, effect, (name, value), text in PASSIVES:
            row = per_level[(name, value)]
            w.writerow([skill_id, effect, text] + [row["l%d" % i] for i in range(1, 21)] + ["%s %s (%s)" % (name, value, row["source"])])
    with open(os.path.join(OUT, "amazon_skill_values.csv"), "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f, lineterminator="\n")
        w.writerow(["id", "value", "unit"] + ["l%d" % i for i in range(1, 21)] + ["source"])
        for r in read_csv(os.path.join(ROOT, "data", "d2-reference", "amazon_levels.csv")):
            if r["skill"] in built_active and r["skill"] not in NEW_SKILLS and r["value"] not in WEAPON_SHARES.get(r["skill"], ([], []))[0]:
                w.writerow([r["skill"].replace(" ", ""), r["value"], r["unit"]] + [r["l%d" % i] for i in range(1, 21)] + [r["source"]])
        for name, (_, added) in WEAPON_SHARES.items():
            if name in built_active:
                w.writerows(new_skill_rows(name, per_level, added))
        for name in NEW_SKILLS:
            if name in built_active:
                w.writerows(new_skill_rows(name, per_level))
    print("Wrote levels.csv, creature_xp.csv, biome_levels.csv, classes.csv, amazon_skills.csv, amazon_passives.csv "
          "and amazon_skill_values.csv to", OUT)


if __name__ == "__main__":
    main()
