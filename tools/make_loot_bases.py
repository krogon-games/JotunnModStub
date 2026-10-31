"""Builds Corruption/Data/loot_bases.csv: the items monsters can drop as Magic or Rare loot.

Reads the game dump (data/game-dump-1.0.20/items.csv, recipes.csv and item_details.csv) and keeps every
player-craftable item (an enabled recipe) that belongs to an affix item class and needs no DLC (m_dlc empty;
the Odin items are tagged "beta" and can't be used without it). The class rules match LootBases.Classify in
Corruption/Rules/LootBases.cs; a unit test checks the two agree on every dumped item.

Run from the repository root:  python3 tools/make_loot_bases.py
"""
import csv
import os

DUMP = os.path.join("data", "game-dump-1.0.20")
OUT = os.path.join("Corruption", "Data", "loot_bases.csv")

# Biomes in progression order; items from any other biome (Ocean) are left out.
BIOMES = ["Meadows", "BlackForest", "Swamp", "Mountain", "Plains", "Mistlands", "Ashlands", "DeepNorth"]

WEAPON_TYPES = {"OneHandedWeapon", "TwoHandedWeapon", "TwoHandedWeaponLeft", "Bow"}
WEAPON_SKILLS = {"Swords", "Knives", "Clubs", "Polearms", "Spears", "Axes", "Unarmed", "Bows", "Crossbows",
                 "ElementalMagic", "BloodMagic"}
ARMOUR = {"Shield": "shield", "Helmet": "helmet", "Chest": "chest", "Legs": "legs", "Shoulder": "cape",
          "Trinket": "jewellery"}


def classify(item_type, skill, max_stack):
    if max_stack != 1:
        return []
    if item_type in WEAPON_TYPES:
        if skill == "Pickaxes":
            return ["pickaxe"]
        if skill in WEAPON_SKILLS:
            return ["weapon", "axe_1h"] if skill == "Axes" and item_type == "OneHandedWeapon" else ["weapon"]
        return []
    return [ARMOUR[item_type]] if item_type in ARMOUR else []


def main():
    with open(os.path.join(DUMP, "items.csv"), encoding="utf-8-sig") as f:
        items = {r["prefab"]: r for r in csv.DictReader(f)}
    with open(os.path.join(DUMP, "recipes.csv"), encoding="utf-8-sig") as f:
        craftable = {r["output"] for r in csv.DictReader(f) if r["enabled"] == "True"}
    with open(os.path.join(DUMP, "item_details.csv"), encoding="utf-8-sig") as f:
        dlc = {r["prefab"] for r in csv.DictReader(f) if r["m_dlc"]}

    rows = []
    for prefab in craftable:
        item = items.get(prefab)
        if not item or prefab in dlc:
            continue
        classes = classify(item["type"], item["skill"], int(item["max_stack"]))
        if not classes or item["biome"] not in BIOMES:
            continue
        rows.append((BIOMES.index(item["biome"]), item["biome"], prefab, item["name"], ";".join(classes)))
    rows.sort()

    with open(OUT, "w", encoding="utf-8-sig", newline="") as f:
        writer = csv.writer(f, lineterminator="\n")
        writer.writerow(["prefab", "name", "biome", "classes"])
        for _, biome, prefab, name, classes in rows:
            writer.writerow([prefab, name, biome, classes])
    print(f"Wrote {len(rows)} items to {OUT}")


if __name__ == "__main__":
    main()
