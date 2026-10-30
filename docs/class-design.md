# Classes, levels and skill trees (Diablo 2 style)

**Status:** Proposed, design only. No code. Branch `claude/class-skills`.

**User brief (2026-10-30):** add Diablo 2 class skill trees as a separate part of our mods. The **Barbarian** first, because it fits Valheim best; expand to the **Amazon** and maybe the **Paladin**. Don't implement everything about a class, only as much as works without breaking the game. Add **character levels**, with points to spend on **stats and skills** each level. **New animations are now allowed where necessary.** Look at EpicMMOSystem first.

D2 reference data (skill trees, prerequisites, synergies, class stats, general rules, sources): [`data/d2-reference/`](../data/d2-reference/README.md). Numbers for our version will go in data tables in the plugin's `Data/` folder, as in Corruption; none are set yet.

## 1. EpicMMOSystem: depend on it, or build our own levels?

Researched 2026-10-30 from its Thunderstore page and source (https://github.com/Wacky-Mole/WackyEpicMMOSystem, MIT-style licence).

**What it has:** levels to 100 (each level needs the previous amount × 1.05 + 300 XP); XP per creature from server-synced JSON files; 5 attribute points per level; six attributes of its own (Strength, Dexterity, Intellect, Endurance, Vigour, Specializing), each with fixed effects per point (e.g. Strength +0.2% physical damage, +2 carry weight; Specializing gives crit chance and gathering bonuses); respec for coins or an item; group XP through the Groups mod; an XP death penalty; replacement HP/stamina/eitr bars and its own stats window. Actively maintained (1.9.74 on GitHub, 2026-10-21). Whether it runs on Valheim 1.0.20 is unverified. Every player must have it. XP is applied by the clients, not the server.

**Why it doesn't fit as a base for D2 classes:**
- Its attributes aren't D2's (Strength, Dexterity, Vitality, Energy) and their effects are fixed in its code. Our class bonuses would stack on top of them.
- Its API is thin: local player only, level, XP and attributes, and **no level-up event**. CaptainSkillTree, a class mod built on it, had to use reflection into its internals and un-patch its crit system. That approach breaks with its frequent releases.
- It patches many of the methods we patch or will patch (`Character.Damage`, `Character.RPC_Damage`, `InventoryGui.Awake/Show/Hide`, item tooltips, `CharacterDrop.GenerateDropList`, the enemy HUD) and replaces the HUD bars our character sheet reads from.
- Its creature-level, level-gap damage and loot-removal rules overlap Corruption's own scaling (stars, rips, corrupted bosses).

**Recommendation (Proposed):** build our own levels, XP and D2 attributes, and don't run EpicMMOSystem alongside them. Take its proven pieces as reference: a geometric XP curve, XP per creature in a server-synced table, group sharing by distance, a respec item. Its per-creature XP values are a possible source for our XP numbers (an alternative is deriving XP from creature health in `data/game-dump-1.0.20/creatures.csv`).

**Other class mods looked at** (none to depend on): Valheim Legends (12 classes, 3 abilities each; deprecated, no licence), CaptainSkillTree (weapon trees with Berserker and Paladin jobs, Whirlwind), AlmanacClasses (talent tree, custom animations; deprecated, no licence), Ways of the Gods (deprecated, no source), TalentTree, MagicHeim. Ideas worth copying: cooldowns as status effects (they show in the vanilla HUD), one ability bar that greys out skills the equipped weapon can't use, warcries cast as emotes (emotes already sync to other players), cost messages in the top-left.

## 2. Where it lives

**Proposed:** a third plugin, separate from CraftingOverhaul and Corruption (the Corruption design already called the skill rework "a separate optional module").
- Network rule `EveryoneMustHaveMod`: warcries and auras touch other players and creatures, and new animations must play on every client.
- Works without Corruption. With Corruption installed, its gear can roll "+N to <class> skills" and "+N to <skill>" affixes, and rings (already planned to raise skills) can raise class skills. Corruption's boss essences could craft the respec token (D2's Token of Absolution is made from boss essences too).
- The character sheet (CraftingOverhaul) can show level, XP and attributes by reading them from the player, without a hard dependency.
- Name: Norse; to choose (§9).

## 3. Levels and points

D2 base rules (reference): max level 99; 5 stat points and 1 skill point per level; extra points from quests (12 skill points, 15 stat points across three difficulties); at most 20 points per skill; a skill takes one more point per level past its required level; respec once per difficulty or with a Token of Absolution.

Proposed Valheim version (structure only; every number still to set, §9):
- **XP from kills**, more for starred creatures, bosses and corrupted creatures. Players near the kill share it, as in a D2 party (Valheim has no parties). D2's quest points → **each boss's first kill** gives bonus skill points (and maybe stat points).
- **Per level:** stat points for the D2 attributes, 1 skill point.
- **Vanilla skills stay** and keep levelling by use (they set how well you use a weapon). The class adds to them; it doesn't replace them. This is less invasive than the original "skill rework" idea of replacing use-to-level.
- **Respec:** an item, as in D2. With Corruption, crafted from boss essences.
- **Saved** in the player's custom data (`Player.m_customData`, already used by the corrupted wishbone), so it stays with the character. Level shown to others through the player's ZDO. XP is decided by the creature owner's game and sent to the players who share it. That trusts clients, which is fine for four friends.

**Attributes.** D2's four, mapped onto Valheim stats (proposed mapping; values to set):

| D2 | D2 effect | Valheim effect |
|---|---|---|
| Strength | Melee damage, item requirements | Physical damage %, carry weight |
| Dexterity | Bow damage, attack rating, block | Bow and throwing damage %; Valheim has no attack rating |
| Vitality | Life and stamina (per class: Barbarian 4 life/point, Amazon and Paladin 3) | Max health and stamina |
| Energy | Mana | Max eitr (if skills use eitr, §9) |

D2's item stat requirements are left out: Valheim gates gear by biome materials already.

## 4. How skills work in Valheim

- **Tree:** D2's three tabs of 10 per class, with D2's tiers (levels 1/6/12/18/24/30), prerequisites, 20-point cap and synergies, read from a data table. Rules (can I put a point here, the effective level with +skills, synergy bonus) in pure, tested code.
- **Skills we can't do well are left out or reworked**, not forced in (user: "as much as we can without breaking the game"). Each skill has a status in the table: `v1`, `later`, `reworked`, `dropped`.
- **Casting:** hotkeys on an ability bar (Jötunn `ButtonConfig`, with gamepad), cooldowns as status effects. Whether a selected combat skill replaces the weapon's secondary attack (closer to D2's right-click skill) is open (§9).
- **Cost:** D2 uses mana; open (§9).
- **Damage types:** D2 magic and holy damage → Valheim **spirit** (strong against undead, as holy is in D2); fire, cold → frost, lightning and poison map directly. Valheim has no attack rating or critical hits; crits would be a new mechanic (Amazon Critical Strike, Barbarian masteries).
- **Weapon skills:** Valheim's `m_skillType` decides which mastery applies (Swords and Knives → Blade, Axes, Clubs → Mace, Polearms, Spears).
- **Multiplayer:** your own game runs your attacks and movement. Buffs on other players go to their games. Effects on creatures go to the creature owner's game (the talisman pattern). Players in range stand in for a D2 party.

### Animations (now allowed where necessary)

In order of preference:
1. **Vanilla animation triggers**, as Valheim Legends does: `ZSyncAnimation.SetTrigger` syncs to other players. Known triggers include `axe_secondary`, `sword_secondary`, `mace_secondary`, `atgeir_secondary` (the 360° spin), `battleaxe_secondary`, `greatsword_secondary`, `spear_throw`, `spear_poke`, `knife_secondary`, `dual_knives`, `dualaxes`, `throw_bomb`, `kick`, `gpower`. This list comes from a third-party mod's source; check it by adding `m_attackAnimation` to `co_dump` first. Warcries use the vanilla emotes (`Roar`, `Challenge`, `Flex`, `Cheer` exist in the game code); emotes sync to other players.
2. **Looping or re-timing a vanilla animation**, e.g. Whirlwind as the atgeir spin replayed while the key is held, with movement allowed (the approach of sighsorry's SecondaryAttacks mod; it's GPL-3, so we write our own). `Animator.Play` and speed changes don't sync, so other players need our own message.
3. **New clips** from an asset bundle, swapped in with an `AnimatorOverrideController` (the standard method, KG's `AnimationReplaceManager`, BSD-3). Jötunn has no animation helper. Risks: the clip swap fights other mods that replace the player's animator (none in the group's current mod list); every player needs the mod; our `JotunnModUnity` project is Unity 2020.3, while Valheim has used Unity 2022.3 since 0.217.46, so the project needs an upgrade before it can build bundles (Valheim 1.0's exact Unity version to check on the user's PC).

## 5. Barbarian (first class)

| Tab | Skill | Valheim version | Animation | Status |
|---|---|---|---|---|
| Combat Masteries | Blade / Axe / Mace / Polearm / Spear Mastery | Damage % for weapons of that Valheim skill (D2's attack rating has no counterpart; crit only if crits are added) | — | v1 |
| | Throwing Mastery | Damage % for thrown spears (and bombs?) | — | v1 |
| | Increased Stamina | Max stamina | — | v1 |
| | Iron Skin | Armour % | — | v1 |
| | Increased Speed | Movement speed % (within the 35% cap in `affix_caps.csv`) | — | v1 |
| | Natural Resistance | Fire, frost, lightning and poison resistance % (the % resistance system planned for affixes, 75% cap) | — | v1 |
| Warcries | Howl | Nearby creatures flee for a while; bosses immune | Emote | v1 |
| | Taunt | A creature targets you and deals less damage (talisman provoke code) | Emote | v1 |
| | Shout | Armour buff for players in range | Emote | v1 |
| | Battle Cry | Nearby enemies take more and deal less damage | Emote | v1 |
| | Battle Orders | Max health, stamina and eitr for players in range | Emote | v1 |
| | Battle Command | +1 to all class skills for players in range | Emote | v1 |
| | War Cry | Damages and staggers nearby enemies | Emote | v1 |
| | Grim Ward | A placed totem (tinted vanilla prop) that scares creatures away; D2 needs a corpse, Valheim bodies vanish | Emote | reworked |
| | Find Potion, Find Item | D2 uses them on corpses; Valheim has none. Possible rework: an extra roll on the drop table of the next kill | — | reworked or dropped |
| Combat Skills | Bash | Strong hit with extra damage and knockback | Weapon's own secondary | v1 |
| | Stun | Hit always staggers | Weapon's attack | v1 |
| | Concentrate | Attacks can't be interrupted; extra damage and armour while attacking | Weapon's attack | v1 |
| | Berserk | Damage turns into spirit damage; armour drops to 0 for a few seconds | Weapon's attack | v1 |
| | Leap | Jump to the aimed spot; knock back enemies on landing | Vanilla first (`knife_secondary` slowed, plus a push), new clip later | v1 |
| | Leap Attack | Leap plus an area hit on landing | As Leap, then a heavy secondary | v1 |
| | Whirlwind | Hold to spin through enemies while moving, hitting everything; drains the skill resource | Looped `atgeir_secondary` first; new clip if it looks wrong with other weapons | v1 (signature skill) |
| | Double Swing, Frenzy | Need two weapons. Valheim has no dual wielding, but Berserkir axes and Skoll and Hati are dual items: limit both skills to them. Frenzy: each hit raises attack and movement speed for a while | Their own attacks | later |
| | Double Throw | Throw two spears | `spear_throw` | later |

## 6. Amazon (second class)

Fits Valheim well: bows and spears are both strong vanilla weapon lines, and Valheim already has fire, frost and poison arrows to borrow effects from.

| Tab | Valheim version | Hard parts |
|---|---|---|
| Bow and Crossbow | Elemental arrows add fire/frost to any arrow; Magic Arrow shoots without ammo for the resource; Multiple Shot and Strafe fire several projectiles; Exploding/Immolation reuse fire and bomb effects; Guided Arrow homes in; Ice/Freezing Arrow freeze (beyond vanilla's frost slow) | Homing projectiles, freezing in place |
| Javelin and Spear | Uses Valheim's spear thrusts and throws (Spears skill). Jab, Fend: rapid or multi-target thrusts. Power/Charged/Lightning Strike: lightning on melee hits. Poison/Plague Javelin: poison clouds. Lightning Bolt/Fury: the thrown spear becomes lightning | Chain lightning, cloud trails |
| Passive and Magic | Dodge, Avoid, Evade: chance to avoid hits. Critical Strike: needs crits (new mechanic). Pierce: arrows pass through. Inner Sight: enemies highlighted and take more damage. Slow Missiles. Decoy and Valkyrie: summons (Valheim has summoning staffs; a fighting Valkyrie needs a model and AI, so it may come last or be dropped). Penetrate: attack rating, so reworked or dropped | Decoy, Valkyrie |

## 7. Paladin (maybe, third)

- **Auras** (one active at a time, as in D2): a status effect in a radius. Buffs for players (Might, Defiance, Prayer, Vigor, Meditation, the resist auras, Salvation) are straightforward; pulses on enemies (Holy Fire, Holy Freeze, Holy Shock, Sanctuary, Conviction) reuse elemental damage and effects.
- **Holy damage = spirit**, which Valheim's undead are weak to already.
- **Combat:** Zeal (rapid strikes), Charge (dash and hit), Sacrifice, Vengeance (elemental damage on hits), Holy Bolt (spirit projectile that heals allies), Blessed Hammer (spiralling projectile, new effect), Fist of the Heavens (lightning from the sky), Holy Shield. **Smite** is a shield bash: Valheim has none, so it needs a new animation. **Conversion** can use Valheim's taming. **Redemption** needs corpses: rework or drop.

## 8. Build order (proposed)

Each step is play-tested before the next.
1. **Levels:** XP from kills, sharing, level-up, D2 attributes with their effects, a level/XP bar and spending window, respec item. Shown on the character sheet.
2. **Class choice and the skill tree panel**, with the rules in tested pure code, and the Barbarian's **Combat Masteries** (passives only: no casting, animations or networking).
3. **Warcries:** ability bar, cost and cooldowns, emotes, buffs on players in range, debuffs through creature owners.
4. **Barbarian combat skills:** Bash, Stun, Concentrate, Berserk, then Leap, Leap Attack and Whirlwind (first the vanilla-animation versions, then new clips where they look wrong).
5. **Amazon.** 6. **Paladin** (if wanted).

Before step 1: check `docs/mod-compatibility.md` for every game method we'll patch (`Character.Damage` and `Character.RPC_Damage` are already patched by Corruption).

## 9. Open questions (for the user)

1. **Own levels vs EpicMMOSystem:** confirm building our own (recommended, §1).
2. **Skill resource:** stamina (the Valheim warrior's resource), eitr (Valheim's mana, but only magic food gives it), or a new bar (e.g. rage)?
3. **Numbers:** follow D2's progressions (per-level values, XP curve shape, 5 + 1 points per level), scaled to Valheim stats from the game dump? Max level 99 or lower? XP per creature from EpicMMOSystem's tables or derived from creature health?
4. **Choosing a class:** at a station or altar, with an item (Valheim Legends uses Eikthyr's altar), or with a console command for now? Can a character change class?
5. **Casting:** hotkeys only, or can a combat skill replace the weapon's secondary attack?
6. **Names:** keep D2 class and skill names (as the orbs keep PoE names, because the group knows them), with a Norse plugin name? "Berserkir" is taken by vanilla axes; "Úlfheðnar" (wolf-warriors) is free. The Amazon could be a Valkyrie or Shieldmaiden (*skjaldmær*).
7. **Vanilla skills:** keep levelling by use alongside class levels (recommended), or replace use-to-level as in the original skill rework idea?
8. **Death penalty:** lose XP on death (D2 Nightmare/Hell, EpicMMOSystem), or rely on Valheim's own skill loss?
