# Affix rules

**[Gathering tool affixes](#gathering-tool-affixes)** (pickaxes and one-handed woodcutting axes) were specified by the user on 2026-10-27. They are part of the loot system (design phase 7), not a separate build.

The [general gear pool](#general-gear-pool-draft) further down is still a **draft proposal, not built** (drafted 2026-10-27). Every name, number and pool membership there is a starting point to edit. Full loot is design phase 7 (see [`GAME_DESIGN.md`](GAME_DESIGN.md)).

Brief from the user for the general pool: "Diablo 2-style prefixes and suffixes. Some share the same mod. A rare combination that includes both can roll an item with ranges above normal value. Show in brackets at the end of each mod in the UI what the range is."

**Related rules elsewhere** (not repeated here):
- Rarity tiers (Normal / Magic 1–2 affixes / Rare 3–4 / Unique fixed), currency orbs, corruption lock and the shard's 25% brick chance: [`corruption-design.md` → Currency orbs](corruption-design.md#currency-orbs) and [Loot and uniques](corruption-design.md#loot-and-uniques).
- Map affixes (Frostbitten, Swarming, ...) are a separate system that modifies rips, not gear: [Map affixes and tiers](corruption-design.md#map-affixes-and-tiers).
- Balance rules that bind affixes: loot never carries downsides; boosts, not bypasses ([Balance rules](corruption-design.md#balance-rules-and-open-questions)).

## Gathering tool affixes

*Specified by the user, 2026-10-27. Status: designed, not built; built with the loot system (phase 7).* Names, effects and ranges in the three pool tables are the user's; everything marked **proposed** is an interpretation awaiting confirmation.

### Rules agreed (user, 2026-10-27)

- **Where affixed tools come from** (applies to all loot, not just tools):
  - Crafting at the normal workbench or forge makes **Normal** items only: the vanilla version, no affixes.
  - **PoE-style orb crafting** at the Riftforge is the path to better items after that.
  - **Monsters drop items with random rolls.** Drop rates to be set.
- **Affix count:** **Magic** = up to 1 prefix and 1 suffix; **Rare** = up to 2 prefixes and 2 suffixes. Same as the general gear pool.
- **Ranges are the same for every tool.** Every affix can roll anywhere in its range on any eligible tool. A perfectly rolled flint axe still never matches a Nord axe, because:
  - percentage affixes scale the tool's own stats (+15% of a flint axe's 30 chop is 4.5; of a Nord axe's 90 it's 13.5);
  - the tool tier still decides what the tool can harvest at all.
  - This differs from the general gear pool's biome tiers (I–IV); see the open questions.
- **Both Bountifuls stack.** The universal Bountiful (bonus resources) and the woodcutting Bountiful (additional wood) are different affixes and can both roll on one axe. Artisan (one extra ore) stacks with the universal Bountiful on a pickaxe in the same way.

### Eligible tools

- **Pickaxes:** every player-craftable item that uses the Pickaxes skill.
- **Axes:** only **one-handed woodcutting axes**: player-craftable, one-handed, Axes skill. Two-handed combat axes (battleaxes, Berserkir axes, Early Axes) never roll gathering affixes.
- The rule is data-driven, so modded tools qualify automatically; a config list excludes exceptions.

From the 1.0.20 dump (`data/game-dump-1.0.20/items.csv`):

| Tool | Prefab | Biome | Tool tier | Eligible |
|---|---|---|---|---|
| Antler pickaxe | `PickaxeAntler` | Meadows | 0 | Yes |
| Bronze pickaxe | `PickaxeBronze` | Black Forest | 1 | Yes |
| Iron pickaxe | `PickaxeIron` | Swamp | 2 | Yes |
| Black metal pickaxe | `PickaxeBlackMetal` | Mistlands | 3 | Yes |
| Stone pickaxe | `PickaxeStone` | — | 0 | Recipe disabled in 1.0 |
| Stone axe | `AxeStone` | Meadows | 0 | Yes |
| Flint axe | `AxeFlint` | Meadows | 1 | Yes |
| Bronze axe | `AxeBronze` | Black Forest | 2 | Yes |
| Iron axe | `AxeIron` | Swamp | 3 | Yes |
| Black metal axe | `AxeBlackMetal` | Plains | 4 | Yes |
| Jotun Bane | `AxeJotunBane` | Mistlands | 4 | **To confirm**: the only Mistlands one-handed axe, but has 40 poison damage |
| Nord axe | `AxeGold` | Deep North | 6 | Yes |
| Thunderblood axe, Frostfire axe | `AxeGold_BloodLightning`, `AxeGold_FrostFire` | Deep North | 6 | **To confirm**: elemental combat variants of the Nord axe |
| Wooden axe | `AxeWood` | — | 1 | No: found in chests only, 1 damage |

Vanilla has no pickaxe after the Mistlands and no one-handed axe from the Ashlands.

### 1. Universal pool (pickaxes and axes)

| Affix | Effect | Range | How it works (checked in the 1.0 game code) |
|---|---|---|---|
| Odin's Grace | The Allfather favours your strike: chance to harvest the struck resource in a single strike | 1–5% | Raise the hit's chop/pickaxe damage enough to destroy the object. The tool tier check still applies |
| Swift | Attack speed | +1–15% | Speeds up the player's animation while swinging the tool. Local only: other players see the normal swing speed (cosmetic) |
| Efficient | Reduced gathering stamina | 5–20% | Lowers the tool's attack stamina (`Attack.GetAttackStamina`) |
| Bountiful | Chance of bonus resources, on the hit that breaks the tree or ore | 1–15% | Needs the harvest message (see [Multiplayer](#multiplayer-drops-happen-on-the-owners-machine)) |
| Durable | Tool durability | +20–75% | `ItemData.GetMaxDurability` |
| Scholar's | Gathering skill XP | +10–25% | `Player.RaiseSkill` for Woodcutting and Pickaxes |
| Determined | Small stamina recovery when a resource is harvested | Amount not set | Needs the harvest message |

### 2. Mining pool (pickaxes only)

| Affix | Effect | Range | How it works |
|---|---|---|---|
| Prospector's | Pickaxe damage against mineral deposits | +5–20% | Scale the pickaxe damage of hits on rocks and ore (`MineRock`, `MineRock5`, `Destructible`) |
| Artisan | Chance to generate one additional ore | 5–15% | Needs the harvest message |
| Excavator's | Pickaxe damage against terrain | +10–30% | Digging isn't damage: each hit applies a fixed terrain operation. **Proposed:** a deeper and wider dig per hit (a scaled copy of the pickaxe's dig) |
| Veinbreaker's | Every 5th mining hit deals extra damage | +50–100% | Per-player hit counter on the striker's machine |
| Fit | Chance to conserve stamina on a swing | 1–15% | Roll once per swing; that swing costs no stamina |
| Gemseeker's | Small chance to find a gemstone or rare mineral from deposits | 1–15% | Needs the harvest message. Gem pool to define |
| Seismic | Chance for a mining strike to damage nearby parts of the same deposit | 1–15% | Multi-part deposits (`MineRock5`) already accept area hits (`HitData.m_radius`); give that hit a radius |

### 3. Woodcutting pool (axes only)

| Affix | Effect | Range | How it works |
|---|---|---|---|
| Lumberjack's | Chop damage against trees | +10–30% | Scale chop damage on standing trees (`TreeBase`) |
| Bountiful | Chance of additional wood | 1–15% | Needs the harvest message. **Same name as the universal Bountiful** (see questions) |
| Splintering | Chance for a successful chop to damage another nearby tree or log | 1–15% | Find the nearest other tree or log within a few metres and send it a copy of the hit |
| Timberfall | Damage against fallen logs | +20–50% | Scale damage on logs (`TreeLog`) |
| Rootbreaker | Chop damage against stumps | +25–300% | Stumps are the prefab a felled tree leaves (`TreeBase.m_stubPrefab`). From game knowledge they are `Destructible` objects; confirm with a dump |

### Multiplayer: drops happen on the owner's machine

Verified in the 1.0 code: hitting a tree, log, stump, rock or deposit only sends the hit to whoever owns that object. The owner's game applies the damage, decides whether it breaks and spawns the drops. The owner can't see the striker's tool affixes, because an item's custom data isn't part of the synced equipment.

- **Changes to the hit** (Odin's Grace, Prospector's, Excavator's, Veinbreaker's, Seismic, Lumberjack's, Splintering, Timberfall, Rootbreaker) happen on the striker's machine before the hit is sent, so they work in multiplayer as they are.
- **Striker-only effects** (Swift, Efficient, Fit, Durable, Scholar's) never involve the owner.
- **On-harvest effects** (both Bountifuls, Artisan, Gemseeker's, Determined) need a small "harvested" message from the owner's game back to the striker's, which then rolls and spawns the extra items next to the object. Every machine needs the mod for this, which fits the Corruption plugin (`EveryoneMustHaveMod`).

### Proposed prefix/suffix split (to confirm)

Magic and Rare items need each affix to be a prefix or a suffix. Proposed: **prefixes make the swing stronger, suffixes bring fortune and endurance.** The split gives each tool enough of both for a Rare (2 + 2).

| Pool | Prefixes | Suffixes |
|---|---|---|
| Universal | Swift, Durable | Odin's Grace, Efficient, Bountiful, Scholar's, Determined |
| Mining | Prospector's, Excavator's, Veinbreaker's, Seismic | Artisan, Fit, Gemseeker's |
| Woodcutting | Lumberjack's, Splintering, Timberfall, Rootbreaker | Bountiful (wood) |
| **Pickaxe total** | 6 | 8 |
| **Axe total** | 6 | 6 |

- **Names on the item** follow the general pool: prefix before, suffix after with "of" (*Swift Iron Pickaxe of the Artisan*). Proposed suffix forms: of Odin's Grace, of Efficiency, of Bounty, of the Scholar, of Determination, of the Artisan, of Fitness, of the Gemseeker, of Plenty (woodcutting Bountiful). Rares get a random two-word name instead.
- **Tooltip lines** use the affix names above; the two Bountifuls need distinct line text ("bonus resources" vs "additional wood").

### Proposed interpretations (to confirm)

- **Universal Bountiful** drops the harvested object's resources once more; **woodcutting Bountiful** adds 1 wood of the type that dropped; **Artisan** adds 1 of the ore that dropped.

- **Harvest event:** every object or deposit part that drops resources when it breaks counts once (a tree, each log, a stump, a rock, each chunk of a copper deposit). On-harvest chances roll per event.
- **Odin's Grace** destroys the struck object or deposit chunk, not a whole multi-part deposit (that could give 30+ ore). On a tree it fells the tree; the logs still need chopping.
- **Prospector's** covers everything a pickaxe mines (stone rocks and ore); **Excavator's** covers digging the ground.
- **Lumberjack's** covers standing trees only; logs are Timberfall's and stumps Rootbreaker's.
- **Gemseeker's** pool: gems and rare minerals whose biome (from the biome detection) is no later than the deposit's biome, e.g. Amber, Ruby, Crystal, Obsidian; Bloodstone, Iolite and Jade from the Deep North; the ancient gemstones only there. Exact list to agree.
- **Determined:** proposed 5–15 stamina per harvest event.
- **Seismic:** radius about 2 m; **Splintering:** the copy deals half the hit's damage, within about 4 m. Both proposed.
- **Veinbreaker's** counts hits on rocks and deposits only; the count resets when the tool is unequipped.
- **Efficient and Fit stack** on a pickaxe.

### Open questions (gathering tools)

1. Confirm the prefix/suffix split and suffix names above.
2. Are Jotun Bane and the Thunderblood and Frostfire axes woodcutting axes?
3. Should the general gear pool also use one range for every item (the base item carrying progression) instead of biome tiers I–IV? Today the two pools follow different rules.
4. Drop rates for Magic and Rare items from monsters (gathering tools and gear alike).
5. The proposed interpretations above.

## General gear pool (draft)

Tools (**T**) roll only from the [gathering tool pools](#gathering-tool-affixes); the general pool's Woodcutting / mining and tool-slot entries are replaced by them.

### Open questions

- **Twin interpretation (unconfirmed).** The brief said "both same suffix and same suffix". The draft reads it as *one prefix and one suffix that share a stat* (a twin). Needs the user's confirmation.
- Can a **Magic** item (one prefix + one suffix) resonate, or only Rares? The draft doesn't restrict it.
- **Name collisions to fix:** "of Warding" is both Block tier II and All resistances tier I; "of the Lox" is both Max stamina III and Carry weight III; "of Sleipnir" (stamina IV) vs "of Sleipnir's Stride" (movement IV); prefixes "Hardened" (durability II) and "Smouldering" (fire I) reuse map-affix names.
- Roll weights per family and per tier aren't set (only "twin halves have half weight").

### How an item rolls

- **Prefixes and suffixes.** Like Diablo 2, every affix is either a prefix (named before the item) or a suffix (named after it): *Hale Bronze Sword of the Bear*.
  - **Magic:** one prefix, one suffix, or both (1–2 affixes, as in the rarity table).
  - **Rare:** up to 2 prefixes and 2 suffixes, 3–4 affixes in total. It gets a random two-word name instead (see [Rare names](#rare-names)).
- **One affix per family.** An item can't roll the same family twice on the same side (no two "enhanced damage" prefixes).
- **Tiers follow the item's biome.** Each family has four tiers: I = Meadows/Black Forest, II = Swamp/Mountain (and Ocean), III = Plains/Mistlands, IV = Ashlands/Deep North.
  - An item rolls the tier of its biome, like Diablo 2's item level. A bronze sword (Black Forest) rolls tier I; a Flametal sword rolls tier IV.
  - The biome comes from CraftingOverhaul's data-driven biome detection, so modded gear gets the right tier too.
  - Map tier and rarity floors (from the loot section) decide *how many* affixes; the item's biome decides *how strong*.
- **Slots.** Each family lists which item classes it can roll on: **W** weapon, **S** shield, **A** armour (helmet, chest, legs, cape), **J** jewellery (rings, trinkets, talismans), **T** tool (axe, pickaxe, and so on).

### Twin mods and resonance

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

### Showing ranges

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

### Prefixes

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

### Suffixes

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

### Caps and guards

- **Resistances:** the total from all gear is capped at 75%.
- **Movement speed:** gear adds at most +20%.
- **Life steal:** gear adds at most 10%.
- **No item downsides**, in line with "loot never carries downsides".
- **Carry weight** stays a boost, not a bypass: there's no "unlimited" affix, so the "boosts, not bypasses" balance rule holds.

### Rare names

Two random words, like Diablo 2's "Grim Fang", from Norse-flavoured lists:

- **First word:** Grim, Raven, Wolf, Storm, Blood, Ash, Frost, Doom, Night, Iron, Bone, Rune, Skull, Wyrm, Ember.
- **Second word, by class:**
  - weapons: Bite, Fang, Edge, Song, Wail, Maw;
  - armour: Shroud, Hide, Ward, Mantle, Coat;
  - shields: Wall, Guard, Aegis;
  - jewellery: Eye, Heart, Coil, Knot.

### Technical notes

- **Storage:** affixes live on the item in `ItemDrop.ItemData.m_customData`. It exists in 1.0 and is saved and synced with the item. Each affix is stored as family, tier and rolled value, so tooltips can show the bracket range and Divine Orbs can reroll in place.
- **Effects:** applied by patches on the stat getters the game already uses: `ItemData.GetDamage`, `GetArmor`, `GetMaxDurability`, the player's max health/stamina/eitr, regen and movement modifiers, and damage taken for resistances.
- **Tooltips:** a postfix on `ItemDrop.ItemData.GetTooltip` adds the affix lines with their bracket ranges. Rarity colours the item name.
- **Data:** the affix table lives in a config/data file, not code, so new families, tiers and twins can be added without a rebuild, as planned for the world-unique pool.
- **Roll logic:** a pure, unit-tested function, as with shard drops and talisman families. Its inputs are rarity, item class, tier and a random source, and it outputs the affix list including resonance.
