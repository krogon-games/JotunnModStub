# Affix rules

**[Gathering tool affixes](#gathering-tool-affixes)** (pickaxes and one-handed woodcutting axes) were specified by the user on 2026-10-27. They are part of the loot system (design phase 7), not a separate build.

The [general gear pool](#general-gear-pool-draft) further down is still a **draft proposal, not built** (drafted 2026-10-27). Every name, number and pool membership there is a starting point to edit. Full loot is design phase 7 (see [`GAME_DESIGN.md`](GAME_DESIGN.md)).

**All affix numbers live in one table: [`Corruption/Data/affixes.csv`](../Corruption/Data/affixes.csv).** This document holds the rules and the reasoning; the table holds names, sides, ranges, weights and which items each affix rolls on. Don't copy numbers back into this document.

## The affix table

One row per affix, for every item type. There are no separate lists: an affix's **item class columns** say what it can roll on, and an item can belong to several classes. A one-handed axe is both `weapon` and `axe_1h`, so it rolls weapon affixes *and* universal and woodcutting gathering affixes; a pickaxe is only `pickaxe`; a battleaxe only `weapon`. The `pool` column is just a label for sorting and filtering.

**How to edit:** open the file in Excel, Google Sheets or LibreOffice (or GitHub's web editor), change cells, and save it as **CSV UTF-8**. Then run `dotnet test CraftingOverhaul.Tests`: a test reads the real file and lists every mistake with its line number. GitHub shows the file as a searchable table.

| Column | Meaning |
|---|---|
| `id` | Permanent key, stored on items. Lowercase, digits, `_`. **Never rename one that has shipped**, or existing items lose the affix |
| `status` | `approved` (user agreed), `proposed` (awaiting review), `draft` (starting numbers, untested) |
| `pool` | `gathering-universal`, `gathering-mining`, `gathering-woodcutting`, `gear`, or `any` (every item class, e.g. Fleet on anything held). The test checks it matches the item classes |
| `side` | `prefix` or `suffix` |
| `family` | Affixes with the same family can't both roll on one side of an item. A prefix and suffix sharing a family are **twins** and resonate |
| `name` | Label for the tooltip line |
| `item_name` | Word added to the item's name. Several separated by `/` = roll-quality names, worst quarter first. **Every name must be unique** across the table (the test checks) |
| `text` | Tooltip text; `{0}` is replaced by the rolled value |
| `min`, `max` | The roll range, the same on every item. Use a dot for decimals |
| `unit` | `%`, `flat`, `m`, `levels` or `minutes` |
| `lower_is_better` | `x` when a lower roll is better (e.g. Deathless's cooldown); used for roll-quality names and how the bracket reads |
| `weight` | How often it rolls compared with the others. **The stronger the affix, the lower the weight** (user rule): 100 = filler, 50 = standard, 25 = strong (procs, twin halves), 10 = very strong, 5 = build-defining |
| `effect` | Which piece of code applies it. Several affixes can share one effect with different numbers; a **new** effect needs code |
| `requires` | Optional mechanic the item must have, worked out from its game data: `melee`, `ranged`, `magic` (from its skill), `combo` (its attack chains 2+ hits), `block` (it can block, so it can parry). Blank = none |
| `weapon` … `axe_1h` | `x` = can roll on that item class |
| `notes` | Free text |

What needs a developer: a new `effect`, a new `requires` mechanic, or a new item class column. Everything else (numbers, names, weights, adding an affix that reuses an existing effect, removing one) is a table edit.

## Background

Brief from the user for the general pool: "Diablo 2-style prefixes and suffixes. Some share the same mod. A rare combination that includes both can roll an item with ranges above normal value. Show in brackets at the end of each mod in the UI what the range is."

**Related rules elsewhere** (not repeated here):
- Rarity tiers (Normal / Magic 1–2 affixes / Rare 3–4 / Unique fixed), currency orbs, corruption lock and the shard's outcomes (brick / unchanged / one corrupted affix on top of the limit): [`corruption-design.md` → Currency orbs](corruption-design.md#currency-orbs) and [Loot and uniques](corruption-design.md#loot-and-uniques).
- Map affixes (Frostbitten, Swarming, ...) are a separate system that modifies rips, not gear: [Map affixes and tiers](corruption-design.md#map-affixes-and-tiers).
- Balance rules that bind affixes: loot never carries downsides; boosts, not bypasses ([Balance rules](corruption-design.md#balance-rules-and-open-questions)).

## Gathering tool affixes

*Specified by the user, 2026-10-27. Status: designed, not built; built with the loot system (phase 7).* Names, effects and ranges in the three pool tables are the user's; everything marked **proposed** is an interpretation awaiting confirmation.

### Rules agreed (user, 2026-10-27)

- **Where affixed tools come from** (applies to all loot, not just tools):
  - Crafting at the normal workbench or forge makes **Normal** items only: the vanilla version, no affixes.
  - **PoE-style orb crafting** at the Riftforge is the path to better items after that.
  - **Monsters drop items with random rolls**, at low rates: **Magic 1 in 50, Rare 1 in 100** per kill (starting values). Uniques and later rarities get their own separate chances.
  - Drop chances live in a data/config table so they can be tuned without a rebuild, and other systems multiply them (corruption zones, scars, terror zones, map tiers). Detail: [`corruption-design.md` → Drops](corruption-design.md#drops).
- **Affix count:** **Magic** = up to 1 prefix and 1 suffix; **Rare** = up to 2 prefixes and 2 suffixes. Same as the general gear pool.
- **Ranges are the same for every tool.** Every affix can roll anywhere in its range on any eligible tool. A perfectly rolled flint axe still never matches a Nord axe, because:
  - percentage affixes scale the tool's own stats (+15% of a flint axe's 30 chop is 4.5; of a Nord axe's 90 it's 13.5);
  - the tool tier still decides what the tool can harvest at all.
  - The same rule now applies to general gear too. It must be **play-tested** before the numbers are trusted.
- **Odin's Grace is the fun rare affix and is meant to be powerful.** On a deposit it breaks the chunk you hit **and every chunk touching it**. On a tree it harvests **the whole tree**, whether you hit the standing tree, a log or the stump.
- **Variety:** prefixes and suffixes don't need an equal split. Add more affixes, including **low-impact** ones, so not every item is awesome.
- **Both Bountifuls stack.** The universal Bountiful (bonus resources) and the woodcutting Bountiful (additional wood) are different affixes and can both roll on one axe. Artisan (one extra ore) stacks with the universal Bountiful on a pickaxe in the same way.

### Eligible tools

- **Pickaxes:** every player-craftable item that uses the Pickaxes skill.
- **Axes:** every **one-handed axe**: player-craftable, one-handed, Axes skill. Two-handed axes (battleaxes, Berserkir axes, Early Axes) never roll gathering affixes.
- **One-handed axes are both tools and weapons** (user, 2026-10-27): they roll from the universal and woodcutting pools **and** the general pool's weapon (**W**) families. Pickaxes roll from the gathering pools only; two-handed axes from the weapon families only.
  - **One combined pool per item** (user, 2026-10-28), so a Magic one-handed axe could be *Lumberjack's Iron Axe of Embers*. Roll weights decide how often each kind appears.
  - The gathering pool's Durable and the weapon families' Durability raise the same stat, so they count as one family on an axe and can't both roll.
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
| Jotun Bane | `AxeJotunBane` | Mistlands | 4 | Yes (user, 2026-10-27) |
| Nord axe | `AxeGold` | Deep North | 6 | Yes |
| Thunderblood axe, Frostfire axe | `AxeGold_BloodLightning`, `AxeGold_FrostFire` | Deep North | 6 | Yes (user, 2026-10-27) |
| Wooden axe | `AxeWood` | — | 1 | No: found in chests only, 1 damage |

Vanilla has no pickaxe after the Mistlands and no one-handed axe from the Ashlands.

### 1. Universal pool (pickaxes and axes)

| Affix | Effect | How it works (checked in the 1.0 game code) |
|---|---|---|
| Odin's Grace | The Allfather favours your strike: chance to harvest the resource in a single strike. Deposit: the chunk hit plus every chunk touching it. Tree: the whole tree (standing tree, log or stump) | Deposit: killing hits on the touching chunks (`MineRock5` hit areas). Tree: the drops of the tree, every log and log piece, and the stump are all known from the prefabs (`TreeBase.m_logPrefab`, `TreeLog.m_subLogPrefab`, `m_stubPrefab`), so they can be spawned at once and the tree removed. Done by the object's owner, or by the striker after claiming ownership. The tool tier check still applies |
| Swift | Attack speed | Speeds up the player's animation while swinging the tool. Local only: other players see the normal swing speed (cosmetic) |
| Efficient | Reduced gathering stamina | Lowers the tool's attack stamina (`Attack.GetAttackStamina`) |
| Bountiful | Chance of bonus resources, on the hit that breaks the tree or ore | Needs the harvest message (see [Multiplayer](#multiplayer-drops-happen-on-the-owners-machine)) |
| Durable | Tool durability | `ItemData.GetMaxDurability` |
| Scholar's | Gathering skill XP | `Player.RaiseSkill` for Woodcutting and Pickaxes |
| Determined | Small stamina recovery when a resource is harvested | Needs the harvest message |

### 2. Mining pool (pickaxes only)

| Affix | Effect | How it works |
|---|---|---|
| Prospector's | Pickaxe damage against mineral deposits | Scale the pickaxe damage of hits on rocks and ore (`MineRock`, `MineRock5`, `Destructible`) |
| Artisan | Chance to generate one additional ore | Needs the harvest message |
| Excavator's | Pickaxe damage against terrain | Digging isn't damage: each hit applies a fixed terrain operation. A deeper and wider dig per hit (a scaled copy of the pickaxe's dig) |
| Veinbreaker's | Every 5th mining hit deals extra damage | Per-player hit counter on the striker's machine |
| Fit | Chance to conserve stamina on a swing | Roll once per swing; that swing costs no stamina |
| Gemseeker's | Small chance to find a gemstone or rare mineral from deposits | Needs the harvest message. Pool: Amber, Amber pearl, Ruby, by biome (see [Interpretations](#interpretations-decided-2026-10-28)) |
| Seismic | Chance for a mining strike to damage nearby parts of the same deposit | Multi-part deposits (`MineRock5`) already accept area hits (`HitData.m_radius`); give that hit a radius |

### 3. Woodcutting pool (axes only)

| Affix | Effect | How it works |
|---|---|---|
| Lumberjack's | Chop damage against trees | Scale chop damage on standing trees (`TreeBase`) |
| Bountiful | Chance of additional wood | Needs the harvest message. **Same name as the universal Bountiful** (see questions) |
| Splintering | Chance for a successful chop to damage another nearby tree or log | Find the nearest other tree or log within a few metres and send it a copy of the hit |
| Timberfall | Damage against fallen logs | Scale damage on logs (`TreeLog`) |
| Rootbreaker | Chop damage against stumps | Stumps are the prefab a felled tree leaves (`TreeBase.m_stubPrefab`). From game knowledge they are `Destructible` objects; confirm with a dump |

### Multiplayer: drops happen on the owner's machine

Verified in the 1.0 code: hitting a tree, log, stump, rock or deposit only sends the hit to whoever owns that object. The owner's game applies the damage, decides whether it breaks and spawns the drops. The owner can't see the striker's tool affixes, because an item's custom data isn't part of the synced equipment.

- **Changes to the hit** (Odin's Grace on deposits, Prospector's, Excavator's, Veinbreaker's, Seismic, Lumberjack's, Splintering, Timberfall, Rootbreaker) happen on the striker's machine before the hit is sent, so they work in multiplayer as they are.
- **Odin's Grace on trees** removes the tree, log or stump and spawns all its wood at once. That needs the object's owner: either the striker claims ownership first (as craft-from-chests does with chests), or the owner's game does it on a message from the striker. To choose when building.
- **Striker-only effects** (Swift, Efficient, Fit, Durable, Scholar's) never involve the owner.
- **On-harvest effects** (both Bountifuls, Artisan, Gemseeker's, Determined) need a small "harvested" message from the owner's game back to the striker's, which then rolls and spawns the extra items next to the object. Every machine needs the mod for this, which fits the Corruption plugin (`EveryoneMustHaveMod`).

### Prefix/suffix split (user, 2026-10-28)

Magic and Rare items need each affix to be a prefix or a suffix: **prefixes make the swing stronger, suffixes bring fortune and endurance.** The split doesn't need to be equal (user). Each affix's side is the `side` column of the table.

**Gathering twins (user, 2026-10-28).** As on gear, some stats exist as both a prefix and a suffix, so a tool can roll the stat twice and resonate (see [Twin mods and resonance](#twin-mods-and-resonance)). Following the gear rule, only plain stat boosts are twins; chance procs (Bountiful, Artisan, Odin's Grace…) and capped stats (Swift) stay single:

| Stat | Prefix | Suffix |
|---|---|---|
| Damage to rocks and ore | Prospector's | of the Prospector |
| Chop damage to trees | Lumberjack's | of the Lumberjack |
| Durability | Durable | of the Anvil |
| Gathering skill XP | Studious | of the Scholar |
| Stamina per swing | Unwearied | of Efficiency |

Each half keeps the original affix's range; both halves have half the normal roll weight (25), as for gear twins. The new halves' names are Claude's; rename freely in the table.


- **Names on the item** follow the general pool: prefix before, suffix after with "of" (*Swift Iron Pickaxe of the Artisan*), from the `item_name` column. Rares get a random two-word name instead.
- **Tooltip lines** use the affix names above; the two Bountifuls need distinct line text ("bonus resources" vs "additional wood").

### Low-impact affixes

*Proposed by Claude and approved by the user, 2026-10-27, for variety.* Every hook below exists in the 1.0 game code.

**Roll weights (approved by the user, 2026-10-28):** low-impact filler rolls most often (weight 100), most of the user's affixes are normal (50), Odin's Grace is rare (10), Deathless rarest (5); twin halves 25. A tool full of filler is a normal find; a rare-weight affix is a treat.

| Affix | Pool | Effect | Hook |
|---|---|---|---|
| Light | Universal | Lighter tool | `ItemData.GetWeight` |
| Careful | Universal | Chance a hit costs no durability | Durability drain per hit in `Attack` |
| Hushed | Universal | Quieter strikes (less noise drawing creatures) | Attack hit noise |
| Keen-eyed | Universal | Larger item pickup radius while the tool is held. Adds on top of Magnetic | `Player.m_autoPickupRange` |
| Stonecutter's | Mining | Chance of 1 extra stone from rocks and deposits | Harvest message |
| Resinous | Woodcutting | Chance of 1 extra resin from trees | Harvest message |
| Forester's | Woodcutting | Chance a harvested tree also drops its seed (pine cone, beech seed, birch seed, fir cone, acorn) | Harvest message; the seed is the one in the tree's own drop table (confirmed in the 1.0.20 data) |

### More gathering affixes (user, 2026-10-28)

| Affix | Pool | Effect | Hook |
|---|---|---|---|
| Magnetic | Universal | Draws drops to you from further away: sets the pickup radius. The stronger, rarer relative of Keen-eyed, which still adds on top (radius capped, see caps) | `Player.m_autoPickupRange` |
| Charring | Woodcutting | Chance that harvested wood turns into coal. **Replaces** the wood (user's choice, 2026-10-28). The user's "Ember", renamed because it clashed with "of Embers" | Harvest message |

### Interpretations (decided 2026-10-28)

**Decided by the user:**
- **Universal Bountiful:** a 5–15% chance that a harvest's drops all drop once more. **Woodcutting Bountiful:** a 5–15% chance that its wood drops once more. Both can trigger on one harvest (triple wood). Chosen over a small percentage added to every harvest: the same amount on average, but visible moments. **Artisan:** 1 extra ore of the kind that dropped (the user's spec).
- **Determined:** each harvest restores 5–15% of max stamina.
- **Gemseeker's** pool: the trader valuables Amber, Amber pearl and Ruby (1.0.20 data: the only sellable gems), limited to items whose biome (biome detection) is no later than the deposit's: Meadows deposits give Amber or Amber pearl, Black Forest and later also Ruby.
- **Prospector's** (and its twin) covers everything a pickaxe breaks: stone rocks and ore deposits. **Excavator's** covers digging the ground.

**Derived from the user's spec and the game code (no new numbers):**
- **Odin's Grace** (the user's spec: the chunk hit plus every chunk touching it; the whole tree from tree, log or stump). "Touching" = the chunks' colliders overlap the hit chunk's bounds. The game doesn't link a fallen log to its stump, so the mod tags them: when a tree falls or a log splits, the pieces and the stump get a shared tree ID (stored on each object), and Odin's Grace harvests every loaded piece with that ID plus the pieces each log would still split into. Trees felled before the mod was installed have no ID: there it harvests the piece hit and everything it would split into.
- **Seismic** uses the same "touching" rule: the hit also lands on the touching chunks. **Splintering** sends a copy of the same hit to the nearest other tree or log within the axe's own attack range (read from the item). Neither needs a radius or damage share of its own.
- **Harvest event:** every object or deposit part that drops resources when it breaks counts once (a tree, each log, a stump, a rock, each chunk of a copper deposit). On-harvest effects roll per event.
- **Lumberjack's** covers standing trees, **Timberfall** fallen logs, **Rootbreaker** stumps: the user's three separate affixes.
- **Forester's:** the seed is the one the tree's own vanilla drop table already lists (1.0.20: Beech → Beech seeds, Pine → Pine cone, Birch → Birch seeds, Fir → Fir cone, Oak → Acorn, Deep North fir → its fir cone).
- **Veinbreaker's** counts hits on rocks and deposits only; the count resets when the tool is unequipped.
- **Efficient and Fit stack** on a pickaxe: one lowers each swing's cost, the other sometimes makes it free.

### Open questions (gathering tools)

None left: sides, weights, the axe pool, twins and the interpretations above were decided 2026-10-28. Rootbreaker's assumption that stumps are `Destructible` objects is checked against `destructibles.csv` from the next `co_dump`.

## Proc and utility affixes (user, 2026-10-28)

New gear affixes that trigger on events. Numbers are Claude's proposals (status `proposed` in the table), based on the existing ranges.

| Affix | Side | Rolls on | Effect |
|---|---|---|---|
| Fleet | Prefix | Anything held: weapons, shields, tools | Movement speed while the item is in your hand. Added at the user's request for a held-item movement affix |
| Foraging | Suffix | Armour only (helmet, chest, legs, cape) | Short movement-speed boost after picking a wild plant (mushrooms, berries, thistle…). Encourages foraging while exploring (user) |
| Bloodforged | Prefix | Weapons | Killing an enemy restores a share of max health (a %, so it grows with the player) |
| Deathless | Prefix | Armour, shields, jewellery | Survive a fatal blow with 1 health, then a long cooldown. The user's "Undying", renamed. Build-defining, so the rarest weight (5); several never stack |
| Shieldwall, Rallying, Skald's, Steadfast | Prefix | Weapons and shields that can block | Parrying buffs you and allies within 10 m: armour, damage on the next hit, attack speed, or stamina. The user's "Warding", renamed and split into variants with different effects; each is its own family, so several can roll on one item |
| Reprisal | Suffix | Weapons and shields that can block | Chance a parry calls lightning on the attacker; damage is a % of the item's block power |
| Wrath | Suffix | Weapons with a combo | The last hit of a combo sends a knockback shockwave |
| Skadi | Suffix | Weapons | Repeated hits on one target add stacking frost damage, up to a cap |
| Fenrir | Suffix | Weapons | Kills give stacking attack speed for a short time |

**Rules for these:**
- **Parry** = a block within 0.25 s of the hit (the game's own perfect-block window). You parry with whatever is blocking: the shield, or the weapon if you have none.
- **Kills:** a creature's death is handled on the machine that owns the creature, which may not be the killer's. Bloodforged and Fenrir use the same "you killed it" message back to the killer as the harvest affixes.
- **Buffs on allies** use the game's own status-effect message (`SEMan.RPC_AddStatusEffect`), so they reach other players' games.
- **Damage procs scale with the item** (Reprisal from block power, Wrath and Skadi from the hit or weapon damage), following the one-range rule.
- **Effects reuse vanilla visuals** (lightning, area hits), per the v1 "no new art" constraint.

## General gear pool (draft)

Pickaxes roll only from the [gathering tool pools](#gathering-tool-affixes); one-handed axes roll from those and the weapon (**W**) families. The general pool's Woodcutting / mining row and tool (**T**) slot entries are replaced by the gathering pools.

**Range rule (user, 2026-10-27): one range per affix for every item; the base item carries the progression.** It replaces the draft's biome tiers I–IV (in git history before 2026-10-27's table). To play-test.

**Flat values (approved 2026-10-27):**
- Where the item has a matching stat, the affix is a **percentage of that stat**, so it grows with the base item (elemental damage as a % of the weapon's physical damage; knockback, parry and block as a % of the item's own value).
- Stats the item doesn't have (max health, stamina, eitr, carry weight, skill levels, movement, resistances) stay **flat, with a smaller span** than the draft's tier I–IV spread.
- **Tier names become roll-quality names:** the four names of a family mark which quarter of the range the roll landed in (*Sharp* = bottom quarter of Enhanced damage, *Merciless* = top quarter).

**How big a range is (user, 2026-10-28):** the **top** of an affix's range is the **strongest vanilla source** of that stat in the game data (an item, a set bonus, a mead, Megingjord…); the **bottom** is **a fifth of the top**. Twins and Rares stacking several affixes can go beyond vanilla; the caps still limit totals. Applied so far:

| Affix | Range | Source (1.0.20 data) |
|---|---|---|
| Fire, frost, lightning, poison, spirit damage (each half) | 10–50% of physical | One top for all elements (user): the median of each element's strongest vanilla player weapon: fire 18% (Frostfire bow), lightning 47% (Himminafl), poison 50% (Jotun Bane), spirit 57% (Silver mace), frost 129% (Frostfire bow) |
| Movement speed | 1–5% | Crown of Valheim +5% (Fenring pieces +3%) |

The rest wait for the next `co_dump`: max health, stamina and eitr, regen, carry weight, skills and resistances come from set bonuses, meads, Megingjord and trinkets (`status_effects.csv`); armour, durability, block, knockback, parry, backstab and attack stamina from item details (`item_details.csv`). Affixes the user already approved with their numbers (Fleet, Foraging, the proc affixes) keep them.

**Ranges** are in the table (`pool` = `gear`; rows still marked `draft` haven't been through the rule yet).

Caps and resonance (+25% on a twin's combined top) are unchanged.

### Open questions

- ~~Twin interpretation~~ **confirmed 2026-10-28:** a twin is one prefix and one suffix that share a stat, and **Magic items resonate too**, not only Rares.
- ~~Name collisions~~ fixed 2026-10-28 by renaming the old placeholder roll-quality names: of Warding → of Protection (all resistances) and of Bracing (block), of the Lox → of the Elk and of Sleipnir → of Vidar (stamina), Hardened → Tempered (durability), Smouldering → Kindled (fire), of Light → of Radiance (spirit), Undying → Everlasting (health regen). The test now rejects any repeated item name.

### How an item rolls

- **Prefixes and suffixes.** Like Diablo 2, every affix is either a prefix (named before the item) or a suffix (named after it): *Hale Bronze Sword of the Bear*.
  - **Magic:** one prefix, one suffix, or both (1–2 affixes, as in the rarity table).
  - **Rare:** up to 2 prefixes and 2 suffixes, 3–4 affixes in total. It gets a random two-word name instead (see [Rare names](#rare-names)).
- **One affix per family.** An item can't roll the same family twice on the same side (no two "enhanced damage" prefixes). A shard's corrupted affix is the exception: it may repeat a family, and it resonates if it completes a twin (user, 2026-10-28).
- ~~**Tiers follow the item's biome.**~~ *Superseded 2026-10-27 by the one-range rule above.* The draft gave each family four biome tiers (I = Meadows/Black Forest … IV = Ashlands/Deep North) with the item rolling its biome's tier.
  - Map tier and rarity floors (from the loot section) still decide *how many* affixes.
- **Item classes.** The table's columns: weapon, shield, helmet, chest, legs, cape, jewellery (rings, trinkets, talismans), pickaxe, axe_1h (one-handed axes, which are also weapons). **AdventureBackpacks backpacks count as capes** (they're shoulder items) and follow the same rules as every item (user, 2026-10-28).

### Twin mods and resonance

Some stats exist as both a prefix and a suffix: **twin mods**. When an item rolls **both halves of a twin**, the two merge into one line, and that line is **resonant**: it rolls from a wider range whose top is 25% above what the two could normally reach together.

- **Normal combined range:** prefix min + suffix min to prefix max + suffix max.
- **Resonant range:** the same minimum, with the maximum raised by 25%.
- **Example:** *Hale/Robust/…* (+5–20 max health) and *of the Bear* (+5–20 max health):
  - normal combined range +10–40;
  - **resonant range +10–50**.
- **Rarity:** twin halves have half the normal roll weight, so a resonant item is a find rather than a given. On a rare item with 2 prefixes and 2 suffixes, two resonant lines are possible but very unlikely.
- **Orbs:**
  - A Divine Orb rerolls the value within the resonant range.
  - Annulment can remove one half, which ends the resonance; the remaining half re-rolls within its own range.

### Showing ranges

Every affix line ends with its possible range in brackets. Twin lines show the resonant range and a marker.

```
Grim Fang                                  (rare, yellow)
Bronze Sword
+24% damage [5–40]
+12% damage as fire [5–20]
+27 max health [10–50] ◆ resonant
-12% attack stamina [5–25]
```

- **Bracket:** the affix's range, the same on every item, so it answers "how good is this roll?". Percentage lines can also show the resulting value (e.g. "+24% damage (+14)") so the base item's role is visible.
- **Twin lines:** shown merged, as Diablo 2 does. Hovering with a modifier key (e.g. Shift) could show the two halves separately.
- **Uniques:** they show their own fixed ranges the same way.

### Twins

Twin families (resonate when both halves roll): fire, frost, lightning, poison and spirit damage; weapon skill; max health; max stamina; max eitr; health regen; durability; block. In the table, a twin is a prefix and a suffix with the same `family`.

### Caps and guards

The most a stat can gain from all affixes together. **Numbers are in [`Corruption/Data/affix_caps.csv`](../Corruption/Data/affix_caps.csv)**; each cap lists the affix families it limits. Capped today: movement speed (raised by the user on 2026-10-28, to tune down later if needed), attack speed, pickup radius, each resistance, life steal. A shard's corrupted affix is not capped: it adds on top of the capped total (user, 2026-10-28; [Corrupted gear](corruption-design.md#corrupted-gear)).

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

- **Storage:** affixes live on the item in `ItemDrop.ItemData.m_customData`. It exists in 1.0 and is saved and synced with the item. Each affix is stored as family and rolled value, so tooltips can show the bracket range and Divine Orbs can reroll in place.
- **Effects:** applied by patches on the stat getters the game already uses: `ItemData.GetDamage`, `GetArmor`, `GetMaxDurability`, the player's max health/stamina/eitr, regen and movement modifiers, and damage taken for resistances.
- **Tooltips:** a postfix on `ItemDrop.ItemData.GetTooltip` adds the affix lines with their bracket ranges. Rarity colours the item name.
- **Data:** [`Corruption/Data/affixes.csv`](../Corruption/Data/affixes.csv), read and checked by `Corruption/Rules/AffixTable.cs` (unit-tested against the real file). Not loaded by the game yet: when loot is built, the server's copy becomes the one everyone uses, so the group can tune it without a rebuild.
- **Roll logic:** a pure, unit-tested function, as with shard drops and talisman families. Its inputs are rarity, item class (including which pools the item uses) and a random source, and it outputs the affix list including resonance.
