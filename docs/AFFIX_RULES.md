# Affix rules

**Status: draft proposal, not built** (drafted 2026-10-27, awaiting the user's review). Every name, number and pool membership is a starting point to edit. Implementation is design phase 7 (see [`GAME_DESIGN.md`](GAME_DESIGN.md)).

Brief from the user: "Diablo 2-style prefixes and suffixes. Some share the same mod. A rare combination that includes both can roll an item with ranges above normal value. Show in brackets at the end of each mod in the UI what the range is."

**Related rules elsewhere** (not repeated here):
- Rarity tiers (Normal / Magic 1–2 affixes / Rare 3–4 / Unique fixed), currency orbs, corruption lock and the shard's 25% brick chance: [`corruption-design.md` → Currency orbs](corruption-design.md#currency-orbs) and [Loot and uniques](corruption-design.md#loot-and-uniques).
- Map affixes (Frostbitten, Swarming, ...) are a separate system that modifies rips, not gear: [Map affixes and tiers](corruption-design.md#map-affixes-and-tiers).
- Balance rules that bind affixes: loot never carries downsides; boosts, not bypasses ([Balance rules](corruption-design.md#balance-rules-and-open-questions)).

## Open questions

- **Twin interpretation (unconfirmed).** The brief said "both same suffix and same suffix". The draft reads it as *one prefix and one suffix that share a stat* (a twin). Needs the user's confirmation.
- Can a **Magic** item (one prefix + one suffix) resonate, or only Rares? The draft doesn't restrict it.
- **Name collisions to fix:** "of Warding" is both Block tier II and All resistances tier I; "of the Lox" is both Max stamina III and Carry weight III; "of Sleipnir" (stamina IV) vs "of Sleipnir's Stride" (movement IV); prefixes "Hardened" (durability II) and "Smouldering" (fire I) reuse map-affix names.
- Woodcutting / mining has no per-tier names yet.
- Roll weights per family and per tier aren't set (only "twin halves have half weight").

## How an item rolls

- **Prefixes and suffixes.** Like Diablo 2, every affix is either a prefix (named before the item) or a suffix (named after it): *Hale Bronze Sword of the Bear*.
  - **Magic:** one prefix, one suffix, or both (1–2 affixes, as in the rarity table).
  - **Rare:** up to 2 prefixes and 2 suffixes, 3–4 affixes in total. It gets a random two-word name instead (see [Rare names](#rare-names)).
- **One affix per family.** An item can't roll the same family twice on the same side (no two "enhanced damage" prefixes).
- **Tiers follow the item's biome.** Each family has four tiers: I = Meadows/Black Forest, II = Swamp/Mountain (and Ocean), III = Plains/Mistlands, IV = Ashlands/Deep North.
  - An item rolls the tier of its biome, like Diablo 2's item level. A bronze sword (Black Forest) rolls tier I; a Flametal sword rolls tier IV.
  - The biome comes from CraftingOverhaul's data-driven biome detection, so modded gear gets the right tier too.
  - Map tier and rarity floors (from the loot section) decide *how many* affixes; the item's biome decides *how strong*.
- **Slots.** Each family lists which item classes it can roll on: **W** weapon, **S** shield, **A** armour (helmet, chest, legs, cape), **J** jewellery (rings, trinkets, talismans), **T** tool (axe, pickaxe, and so on).

## Twin mods and resonance

Some stats exist as both a prefix and a suffix: **twin mods**. When an item rolls **both halves of a twin**, the two merge into one line, and that line is **resonant**: it rolls from a wider range whose top is 25% above what the two could normally reach together.

- **Normal combined range:** prefix min + suffix min to prefix max + suffix max.
- **Resonant range:** the same minimum, with the maximum raised by 25%.
- **Example:** tier II *Robust* (+10–15 max health) and tier II *of the Bear* (+10–15 max health):
  - normal combined range +20–30;
  - **resonant range +20–37**.
- **Rarity:** twin halves have half the normal roll weight, so a resonant item is a find rather than a given. On a rare item with 2 prefixes and 2 suffixes, two resonant lines are possible but very unlikely.
- **Orbs:**
  - A Divine Orb rerolls the value within the resonant range.
  - Annulment can remove one half, which ends the resonance; the remaining half re-rolls within its own range.

## Showing ranges

Every affix line ends with its possible range, for the item's tier, in brackets. Twin lines show the resonant range and a marker.

```
Grim Fang                                  (rare, yellow)
Bronze Sword
+24% damage [20–30]
+7 fire damage [5–10]
+27 max health [20–37] ◆ resonant
-12% attack stamina [10–15]
```

- **Bracket:** the tier's range, not every tier's, so it answers "how good is this roll?".
- **Twin lines:** shown merged, as Diablo 2 does. Hovering with a modifier key (e.g. Shift) could show the two halves separately.
- **Uniques:** they show their own fixed ranges the same way.

## Prefixes

Ranges are per tier: I / II / III / IV.

| Family | Names (tier I → IV) | Effect | I | II | III | IV | Slots | Twin |
|---|---|---|---|---|---|---|---|---|
| Enhanced damage | Sharp → Fine → Cruel → Merciless | +% weapon damage | 5–10% | 10–20% | 20–30% | 30–40% | W | — |
| Fire damage | Smouldering → Burning → Blazing → Infernal | +flat fire damage | 2–5 | 5–10 | 10–18 | 18–28 | W | of Embers |
| Frost damage | Chilling → Freezing → Glacial → Rimeborn | +flat frost damage | 2–5 | 5–10 | 10–18 | 18–28 | W | of Rime |
| Lightning damage | Crackling → Sparking → Storming → Thunderous | +flat lightning damage | 2–5 | 5–10 | 10–18 | 18–28 | W | of Thunder |
| Poison damage | Venomous → Toxic → Pestilent → Plaguebound | +flat poison damage over time | 3–6 | 6–12 | 12–22 | 22–34 | W | of the Serpent |
| Spirit damage | Hallowed → Blessed → Sanctified → Einherjar's | +flat spirit damage | 2–5 | 5–10 | 10–18 | 18–28 | W | of the Valkyrie |
| Weapon skill | Skilled → Adept → Expert → Master's | +levels to the weapon's skill | 1–2 | 2–4 | 3–5 | 4–7 | W | of Mastery |
| Backstab | Sly → Cunning → Treacherous → Assassin's | +backstab multiplier | +0.25–0.5x | +0.5–1x | +1–1.5x | +1.5–2x | W | — |
| Enhanced armour | Sturdy → Strong → Glorious → Godly | +% armour | 5–10% | 10–20% | 20–30% | 30–40% | S A | — |
| Max health | Hale → Robust → Stalwart → Giant's | +max health | 5–10 | 10–15 | 15–25 | 25–35 | A J S | of the Bear |
| Max stamina | Vigorous → Tireless → Indomitable → Jötunn's | +max stamina | 5–10 | 10–15 | 15–25 | 25–35 | A J | of the Wolf |
| Max eitr | Mystic → Seer's → Völva's → Norn's | +max eitr | 5–10 | 10–15 | 15–25 | 25–35 | A J | of the Raven |
| Health regen | Mending → Restoring → Renewing → Undying | +% health regen | 5–10% | 10–15% | 15–20% | 20–25% | A J | of Regrowth |
| Durability | Solid → Hardened → Unyielding → Dwarven | +% max durability | 20–40% | 40–60% | 60–80% | 80–100% | W S A T | of Ages |
| Fire resistance | Red → Crimson → Garnet → Ruby | % less fire damage | 5–10% | 10–15% | 15–20% | 20–25% | S A J | — |
| Frost resistance | Azure → Lapis → Cobalt → Sapphire | % less frost damage | 5–10% | 10–15% | 15–20% | 20–25% | S A J | — |
| Lightning resistance | Ochre → Amber → Coral → Topaz | % less lightning damage | 5–10% | 10–15% | 15–20% | 20–25% | S A J | — |
| Poison resistance | Beryl → Jade → Viridian → Emerald | % less poison damage | 5–10% | 10–15% | 15–20% | 20–25% | S A J | — |
| Block | Bulwark → Rampart → Bastion → Citadel | +% block power | 10–20% | 20–30% | 30–40% | 40–50% | S | of Deflection |
| Woodcutting / mining | Woodsman's → Miner's style by tool | +% chop / pickaxe damage | 10–20% | 20–30% | 30–40% | 40–50% | T | — |

## Suffixes

| Family | Names (tier I → IV) | Effect | I | II | III | IV | Slots | Twin |
|---|---|---|---|---|---|---|---|---|
| Fire damage | of Embers → of Flame → of Fire → of Muspelheim | +flat fire damage | 2–5 | 5–10 | 10–18 | 18–28 | W | Smouldering… |
| Frost damage | of Frost → of Ice → of Rime → of Niflheim | +flat frost damage | 2–5 | 5–10 | 10–18 | 18–28 | W | Chilling… |
| Lightning damage | of Sparks → of Lightning → of Thunder → of Thor | +flat lightning damage | 2–5 | 5–10 | 10–18 | 18–28 | W | Crackling… |
| Poison damage | of Venom → of the Adder → of the Serpent → of Jörmungandr | +flat poison damage | 3–6 | 6–12 | 12–22 | 22–34 | W | Venomous… |
| Spirit damage | of Light → of the Valkyrie → of Asgard → of Odin | +flat spirit damage | 2–5 | 5–10 | 10–18 | 18–28 | W | Hallowed… |
| Weapon skill | of Practice → of Skill → of Mastery → of Legends | +levels to the weapon's skill | 1–2 | 2–4 | 3–5 | 4–7 | W | Skilled… |
| Max health | of the Boar → of the Bear → of the Troll → of the Jötunn | +max health | 5–10 | 10–15 | 15–25 | 25–35 | A J S | Hale… |
| Max stamina | of the Deer → of the Wolf → of the Lox → of Sleipnir | +max stamina | 5–10 | 10–15 | 15–25 | 25–35 | A J | Vigorous… |
| Max eitr | of Runes → of the Raven → of Galdr → of Mímir | +max eitr | 5–10 | 10–15 | 15–25 | 25–35 | A J | Mystic… |
| Health regen | of Regrowth → of Renewal → of Idunn → of Yggdrasil | +% health regen | 5–10% | 10–15% | 15–20% | 20–25% | A J | Mending… |
| Durability | of Ages → of Eras → of Aeons → of Eternity | +% max durability | 20–40% | 40–60% | 60–80% | 80–100% | W S A T | Solid… |
| Block | of Deflection → of Warding → of the Wall → of the Shieldmaiden | +% block power | 10–20% | 20–30% | 30–40% | 40–50% | S | Bulwark… |
| Life steal | of the Leech → of the Bat → of the Draugr → of the Vampire | % of damage dealt returned as health | 1–2% | 2–3% | 3–4% | 4–5% | W | — |
| Attack stamina | of Ease → of Endurance → of Grace → of the Berserker | % less stamina per attack | 5–10% | 10–15% | 15–20% | 20–25% | W | — |
| Stamina regen | of Breath → of the Lungs → of the Wind → of the Storm | +% stamina regen | 5–10% | 10–15% | 15–20% | 20–25% | A J | — |
| Movement speed | of Haste → of Speed → of the Hare → of Sleipnir's Stride | +% movement speed | 2–4% | 4–6% | 6–8% | 8–10% | A (legs, cape) J | — |
| Carry weight | of the Mule → of the Ox → of the Lox → of the Mountain | +carry weight | 10–20 | 20–30 | 30–40 | 40–50 | A J | — |
| Knockback | of Force → of the Ram → of the Hammer → of Mjölnir | +% knockback | 10–20% | 20–30% | 30–40% | 40–50% | W S | — |
| Parry | of Parrying → of Riposte → of the Duelist → of Tyr | +parry bonus | +0.25x | +0.5x | +0.75x | +1x | S W | — |
| All resistances | of Warding → of the Elements → of the Aesir → of the Gods | % less fire, frost, lightning and poison damage | 3–5% | 5–8% | 8–11% | 11–14% | S A J | — |

**Twins** (resonate when both halves roll): fire, frost, lightning, poison and spirit damage; weapon skill; max health; max stamina; max eitr; health regen; durability; block. Twelve twins in all, so resonance is possible on every item class.

## Caps and guards

- **Resistances:** the total from all gear is capped at 75%.
- **Movement speed:** gear adds at most +20%.
- **Life steal:** gear adds at most 10%.
- **No item downsides**, in line with "loot never carries downsides".
- **Carry weight** stays a boost, not a bypass: there's no "unlimited" affix, so the "boosts, not bypasses" balance rule holds.

## Rare names

Two random words, like Diablo 2's "Grim Fang", from Norse-flavoured lists:

- **First word:** Grim, Raven, Wolf, Storm, Blood, Ash, Frost, Doom, Night, Iron, Bone, Rune, Skull, Wyrm, Ember.
- **Second word, by class:**
  - weapons: Bite, Fang, Edge, Song, Wail, Maw;
  - armour: Shroud, Hide, Ward, Mantle, Coat;
  - shields: Wall, Guard, Aegis;
  - jewellery: Eye, Heart, Coil, Knot.

## Technical notes

- **Storage:** affixes live on the item in `ItemDrop.ItemData.m_customData`. It exists in 1.0 and is saved and synced with the item. Each affix is stored as family, tier and rolled value, so tooltips can show the bracket range and Divine Orbs can reroll in place.
- **Effects:** applied by patches on the stat getters the game already uses: `ItemData.GetDamage`, `GetArmor`, `GetMaxDurability`, the player's max health/stamina/eitr, regen and movement modifiers, and damage taken for resistances.
- **Tooltips:** a postfix on `ItemDrop.ItemData.GetTooltip` adds the affix lines with their bracket ranges. Rarity colours the item name.
- **Data:** the affix table lives in a config/data file, not code, so new families, tiers and twins can be added without a rebuild, as planned for the world-unique pool.
- **Roll logic:** a pure, unit-tested function, as with shard drops and talisman families. Its inputs are rarity, item class, tier and a random source, and it outputs the affix list including resonance.
