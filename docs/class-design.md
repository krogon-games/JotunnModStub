# Classes, levels and skill trees (Diablo 2 style)

**Status:** Proposed, design only. No code. Branch `claude/class-skills`.

**User brief (2026-10-30):** add Diablo 2 class skill trees as a separate part of our mods. ~~The Barbarian first~~ **The Amazon first** (the group voted for her, 2026-10-30), then the **Barbarian**, maybe the **Paladin**. Don't implement everything about a class, only as much as works without breaking the game. Add **character levels**, with points to spend on **stats and skills** each level. **New animations are now allowed where necessary.** Look at EpicMMOSystem first.

D2 reference data (skill trees, prerequisites, synergies, class stats, general rules, sources): [`data/d2-reference/`](../data/d2-reference/README.md). Numbers for our version will go in data tables in the plugin's `Data/` folder, as in Corruption; none are set yet.

## 1. EpicMMOSystem: depend on it, or build our own levels?

Researched 2026-10-30 from its Thunderstore page and source (https://github.com/Wacky-Mole/WackyEpicMMOSystem, MIT-style licence).

**What it has:** levels to 100 (each level needs the previous amount × 1.05 + 300 XP); XP per creature from server-synced JSON files; 5 attribute points per level; six attributes of its own (Strength, Dexterity, Intellect, Endurance, Vigour, Specializing), each with fixed effects per point (e.g. Strength +0.2% physical damage, +2 carry weight; Specializing gives crit chance and gathering bonuses); respec for coins or an item; group XP through the Groups mod; an XP death penalty; replacement HP/stamina/eitr bars and its own stats window. Actively maintained (1.9.74 on GitHub, 2026-10-21). Whether it runs on Valheim 1.0.20 is unverified. Every player must have it. XP is applied by the clients, not the server.

**Why it doesn't fit as a base for D2 classes:**
- Its attributes aren't D2's (Strength, Dexterity, Vitality, Energy) and their effects are fixed in its code. Our class bonuses would stack on top of them.
- Its API is thin: local player only, level, XP and attributes, and **no level-up event**. CaptainSkillTree, a class mod built on it, had to use reflection into its internals and un-patch its crit system. That approach breaks with its frequent releases.
- It patches many of the methods we patch or will patch (`Character.Damage`, `Character.RPC_Damage`, `InventoryGui.Awake/Show/Hide`, item tooltips, `CharacterDrop.GenerateDropList`, the enemy HUD) and replaces the HUD bars our character sheet reads from.
- Its creature-level, level-gap damage and loot-removal rules overlap Corruption's own scaling (stars, rips, corrupted bosses).

**Decided (user, 2026-10-30):** build our own levels, XP and D2 attributes, and don't run EpicMMOSystem alongside them. Take its proven pieces as reference: a geometric XP curve, XP per creature in a server-synced table, group sharing by distance, a respec item. Its per-creature XP values are a possible source for our XP numbers (an alternative is deriving XP from creature health in `data/game-dump-1.0.20/creatures.csv`).

**Other class mods looked at** (none to depend on): Valheim Legends (12 classes, 3 abilities each; deprecated, no licence), CaptainSkillTree (weapon trees with Berserker and Paladin jobs, Whirlwind), AlmanacClasses (talent tree, custom animations; deprecated, no licence), Ways of the Gods (deprecated, no source), TalentTree, MagicHeim. Ideas worth copying: cooldowns as status effects (they show in the vanilla HUD), one ability bar that greys out skills the equipped weapon can't use, warcries cast as emotes (emotes already sync to other players), cost messages in the top-left.

## 2. Where it lives

**Proposed:** a third plugin, separate from CraftingOverhaul and Corruption (the Corruption design already called the skill rework "a separate optional module").
- Network rule `EveryoneMustHaveMod`: warcries and auras touch other players and creatures, and new animations must play on every client.
- Works without Corruption. With Corruption installed, its gear can roll "+N to <class> skills" and "+N to <skill>" affixes, and rings (already planned to raise skills) can raise class skills. Corruption's boss essences could craft the respec token (D2's Token of Absolution is made from boss essences too).
- The character sheet (CraftingOverhaul) can show level, XP and attributes by reading them from the player, without a hard dependency.
- Name: not important to the user (2026-10-30); working name **Orlog**, after the class stone.

## 3. Levels and points

D2 base rules (reference): max level 99; 5 stat points and 1 skill point per level; extra points from quests (12 skill points, 15 stat points across three difficulties); at most 20 points per skill; a skill takes one more point per level past its required level; respec once per difficulty or with a Token of Absolution.

**Decided (user, 2026-10-30): follow D2's rules, with every rule in admin config** (server-synced) so it can be changed: max level (default 99), stat points per level (5), skill points per level (1), bonus points from bosses, the 20-point cap per skill, the one-point-per-level pace. The XP needed per level is a table, D2's experience table by default, in the plugin's `Data/` folder, with a config multiplier. Still open: how much XP each creature gives (§10).

Valheim version:
- **Levelling guide (user, 2026-10-30):** the level a player should be at each boss: Eikthyr 5, The Elder 11, Bonemass 20, Moder 35, Yagluth 50, The Queen 62, Fader 76, Kall Fimbulbringer 90. This also gives each biome's creature level range for the XP formula below: Meadows 1–5, Black Forest 5–11, Swamp 11–20, Mountains 20–35, Plains 35–50, Mistlands 50–62, Ashlands 62–76, Deep North 76–90; levels 91–99 are endgame (rips, corrupted bosses). How creatures spread inside their biome's range (for example by health, from the game dump) is to propose.
- **Pace (user, 2026-10-30): slow, like classic World of Warcraft.** ~~Proposed: WoW's per-kill formula (level × 5 + 45) and grey mobs.~~ **Built 2026-10-30** (Orlog; details in `FEATURES.md` → Levels and XP and `Orlog/Data/README.md`): D2's monster XP is made for hundreds of kills an hour and can't reach the guide in Valheim, and WoW's per-kill XP doesn't fit D2's level table. So the XP per creature level is **D2's XP to the next level ÷ classic WoW's kills per level** (WoW 1–59 spread over our 1–89): the same number of same-level kills per level as classic WoW without quests. Creature level = boss level from the guide, or the range of the biome it dies in, placed by its base health. Stars ×2/×3 (the game's health multiplier). D2's level-difference and party rules (not WoW's grey mobs and group rates), except that a creature above the player gives the player's own level's XP + WoW's up to +20% (changed after the 2026-10-30 play-test: early levels were 2–3× too fast). Totals at the default pace: about 60 same-level kills to Eikthyr's level, 1,050 to Bonemass's, 6,600 to Yagluth's, 24,600 to Kall's; `XpMultiplier` tunes it after play-testing. Open: boss XP (now one creature of the boss's level; D2's act bosses give 9–65× a normal monster).
- **XP from kills**, more for starred creatures, bosses and corrupted creatures. Players near the kill share it, as in a D2 party (Valheim has no parties). D2's quest points → **each boss's first kill** gives bonus skill points (and maybe stat points).
- **Per level:** stat points for the D2 attributes, 1 skill point.
- **Vanilla skills stay** (decided, user 2026-10-30) and keep levelling by use (they set how well you use a weapon). The class adds to them; it doesn't replace them. This is less invasive than the original "skill rework" idea of replacing use-to-level.
- **Death:** no XP or level loss (user, 2026-10-30). Valheim's own skill loss applies as now. From the game code: each skill drops by `Skills.m_DeathLowerFactor` × the world's skill reduction rate (`Game.m_skillReductionRate`, the "Death penalty" world modifier, 1 by default). The factor's code default is 0.25, but the player prefab may set its own value, so the real percentage (the user remembers 10%) is unverified until `co_dump` reads it.
- **Respec:** an item, as in D2. With Corruption, crafted from boss essences.
- **Saved** in the player's custom data (`Player.m_customData`, already used by the corrupted wishbone), so it stays with the character. Level shown to others through the player's ZDO. XP is decided by the creature owner's game and sent to the players who share it. That trusts clients, which is fine for four friends.

**Attributes.** D2's four, mapped onto Valheim stats. **Built 2026-10-30** (`FEATURES.md` → Attributes and class eitr): Strength +1% physical damage per point (melee, thrown), Dexterity +1% (bows), Vitality and levels give D2's life and stamina scaled by Valheim's starting values (×0.5, ×0.89), Energy +1.5 eitr; the starting attributes count as vanilla. Carry weight from Strength dropped (not D2). Original proposal:

| D2 | D2 effect | Valheim effect |
|---|---|---|
| Strength | Melee damage, item requirements | Physical damage %, carry weight |
| Dexterity | Bow damage, attack rating, block | Bow and throwing damage %; Valheim has no attack rating |
| Vitality | Life and stamina (per class: Barbarian 4 life/point, Amazon and Paladin 3) | Max health and stamina |
| Energy | Mana | Max eitr (if skills use eitr, §10) |

D2's item stat requirements are left out: Valheim gates gear by biome materials already.

## 4. How skills work in Valheim

- **Tree (decided, user 2026-10-30):** D2's three tabs of 10 per class, with D2's 20-point cap and synergies, read from a data table. Rules (can I put a point here, the effective level with +skills, synergy bonus) in pure, tested code. Two ideas taken from Diablo 4:
  1. **Level tiers instead of prerequisite chains.** A skill opens at its D2 level (1/6/12/18/24/30). Its D2 prerequisites only count when they are built (status `v1` or `reworked`); a prerequisite that is `later` or `dropped` is skipped, so the dependent opens by level alone and no chain breaks. A dropped skill leaves an empty slot that a Valheim-flavoured skill can fill later.
  2. **Upgrade choices on the big skills, later:** at set point counts (for example 10 and 20) a signature skill offers one of two variants. That is where reworked skills get their Valheim twists without changing D2's layout.
- **Why not one Diablo 4 tree:** our point economy is D2's (about 110 skill points by level 99, 20 per skill), which fits 30 skills in three themed tabs; D4's tree is built for about 60 points and skills capped at 5, so most of it would get filled and every character would end up alike.
- **Skills we can't do well are left out or reworked**, not forced in (user: "as much as we can without breaking the game"). Each skill has a status in the table: `v1`, `later`, `reworked`, `dropped`.
- **Casting (decided, user 2026-10-30): D2's layout.** The normal attack stays the weapon's own; the **chosen skill** goes on the skill button.
  - **Skill bar: 4 skill slots**, a new UI element styled like the rest of the UI (built by the UI session, §9). The number keys 1–0 stay the vanilla hotbar for weapons, tools and items. Players drag skills from the skills panel into the slots; to change a skill, drag another one onto the slot. Passives don't take a slot. Slots are saved per character.
  - **Each slot has its own hotkey** (rebindable). Pressing it: an attack skill (an arrow mode, a spear or javelin skill) becomes the skill on the skill button, and its slot lights up; a cast skill (Inner Sight, later Decoy and Valkyrie) casts at once.
  - **Skill button (decided, user 2026-10-30): right click for every weapon**, as in D2, and rebindable (mouse side buttons included). **Block and parry move to another key**, also rebindable. Valheim's own controls menu already lets players rebind Block; the mod only changes its default away from right click (to a free key, chosen from the game's and the group's mods' bindings). Valheim's dodge is Block + Jump (its settings text, `$settings_dodge`), so the new block key must be comfortable together with Space; a side button suits. The weapon's vanilla secondary attack (middle mouse) is unchanged. With no skill chosen, right click does nothing (proposed).
  - Proposed details: the chosen skill is remembered per weapon kind (the last bow skill comes back when you take the bow out again); a slot greys out when the weapon in hand can't use it, or when you can't pay its cost; cost and cooldown show on the slot.
  - Default slot hotkeys are picked from keys that neither Valheim nor the group's mods (EquipmentAndQuickSlots, AdventureBackpacks, PlantEasily) bind by default, read from their code and config before building.
  - Technical: drawing a bow on a button other than the attack button needs a patch to the player's input; prove it in the first prototype.
- **Cost (decided, user 2026-10-30): skills that cost mana in D2 cost eitr.** Skills without a mana cost (passives, Paladin auras) stay free.

### Eitr: making it usable for classes

Today eitr is punishing for anyone who isn't a late-game mage (game dump): a character has **no eitr at all** until eitr food, and almost all eitr food and both eitr meads come from the Mistlands or later (Magecap 25, Yggdrasil porridge 80, Seeker aspic 85; Minor eitr mead +125 eitr over 10 s, Lingering eitr mead eitr regen ×1.25 for 5 minutes). An Amazon in the Meadows couldn't cast once. Proposed redesign (user asked for it; numbers still to set):
- **A base eitr pool from the class** (decided, user 2026-10-30; **built 2026-10-30**, vanilla regen at 5/s refills it): every class starts with **15 eitr** and gains **1.5 per level** and **1.5 per Energy point** (D2's Amazon and Paladin values, `data/d2-reference/classes.csv`), scaled to Valheim. Eitr food adds on top. Without a class nothing changes. Note for the scale: at 1:1 a level-90 character has about 149 eitr before Energy, and late eitr foods give 80–110 each, so 1:1 may already fit (to confirm when building).
- **Eitr regeneration that works without eitr food** (decided, user 2026-10-30; rate to set). Vanilla regen only refills the pool that food gives.
- **Eitr affixes on loot** (user, 2026-10-30): the Magic and Rare affixes being designed in the loot work add max eitr (already a twin family in `AFFIX_RULES.md`) and eitr regeneration on top of the class pool.
- **Eitr potions from the early biomes**, like D2's mana potions: craftable with early materials, modelled on the vanilla minor eitr mead (instant or over a few seconds) and lingering mead (regen). Players can carry them in the EquipmentAndQuickSlots quick slots.
- Vanilla staffs use the same eitr, so the class pool also makes a mage build easier. That's intended for classes; a character without a class keeps vanilla eitr.
- **Damage types:** D2 magic and holy damage → Valheim **spirit** (strong against undead, as holy is in D2); fire, cold → frost, lightning and poison map directly. Valheim has no attack rating or critical hits; crits would be a new mechanic (Amazon Critical Strike, Barbarian masteries).
- **Weapon skills:** Valheim's `m_skillType` decides which mastery applies (Swords and Knives → Blade, Axes, Clubs → Mace, Polearms, Spears).
- **Multiplayer:** your own game runs your attacks and movement. Buffs on other players go to their games. Effects on creatures go to the creature owner's game (the talisman pattern). Players in range stand in for a D2 party.

### Animations (now allowed where necessary)

In order of preference:
1. **Vanilla animation triggers**, as Valheim Legends does: `ZSyncAnimation.SetTrigger` syncs to other players. Known triggers include `axe_secondary`, `sword_secondary`, `mace_secondary`, `atgeir_secondary` (the 360° spin), `battleaxe_secondary`, `greatsword_secondary`, `spear_throw`, `spear_poke`, `knife_secondary`, `dual_knives`, `dualaxes`, `throw_bomb`, `kick`, `gpower`. This list comes from a third-party mod's source; check it by adding `m_attackAnimation` to `co_dump` first. Warcries use the vanilla emotes (`Roar`, `Challenge`, `Flex`, `Cheer` exist in the game code); emotes sync to other players.
2. **Looping or re-timing a vanilla animation**, e.g. Whirlwind as the atgeir spin replayed while the key is held, with movement allowed (the approach of sighsorry's SecondaryAttacks mod; it's GPL-3, so we write our own). `Animator.Play` and speed changes don't sync, so other players need our own message.
3. **New clips** from an asset bundle, swapped in with an `AnimatorOverrideController` (the standard method, KG's `AnimationReplaceManager`, BSD-3). Jötunn has no animation helper. Risks: the clip swap fights other mods that replace the player's animator (none in the group's current mod list); every player needs the mod; our `JotunnModUnity` project is Unity 2020.3, while Valheim 1.0.21 runs **Unity 6000.0.75** (Unity 6; from the user's `LogOutput.log`, 2026-11-01), so the project needs an upgrade to Unity 6 before it can build bundles.

## 5. Amazon (first class)

Chosen by the group (2026-10-30). She fits Valheim well: bows and spears are vanilla weapon lines in almost every biome (bows from the Meadows to the Deep North; spears from the Meadows, except the Plains; crossbows from the Mistlands), and the game already has what several of her skills need (from `data/game-dump-1.0.20/`):
- Fire, frost, poison and silver (spirit) arrows: their damage and effects for Fire, Cold and Magic Arrow.
- Status effects `Burning`, `Frost` (slow), `Poison`, `Lightning`, `Immobilized` (speed −1000, 5 s: a freeze) and `Tared` (speed −50%).
- Staffs that fire several projectiles (`StaffIceShards`), explode (`StaffClusterbomb`) or throw lightning (`StaffLightning`): effects to reuse for Multiple Shot, Exploding Arrow and the lightning javelins.
- A summon system with a friendly creature (`Skeleton_Friendly`, faction Players) for Decoy and Valkyrie.

**How her skills are used (proposed):** D2 puts the chosen skill on the right mouse button; Valheim's bow has no secondary attack and the spear throws on its secondary. So:
- **Bow skills fire on the skill button** (§4): the attack button shoots a normal arrow; the skill button draws and shoots the chosen skill and pays its cost. Example: Multiple Shot chosen, the skill button fans out several arrows for one arrow from the quiver; Magic Arrow works with an empty quiver.
- **Spear skills ride the spear's own attacks:** thrust skills (Jab, Power Strike, Impale, Charged and Lightning Strike, Fend) on the primary attack, javelin skills (Poison, Plague, Lightning Bolt, Lightning Fury) on the throw.
- **Javelin skills throw a spectral copy** of the spear, so the real spear stays in hand (a vanilla throw leaves it on the ground to pick up, which doesn't suit casting). Decided (user, 2026-10-30).
- **Passives** (Critical Strike, Dodge, Avoid, Evade, Pierce) need no casting.

| Tab | Skill | Valheim version | Status |
|---|---|---|---|
| Bow and Crossbow | Magic Arrow | Shoots without using an arrow, for the skill's cost; the damage turns into spirit damage as it levels | v1 |
| | Fire Arrow | Adds fire damage to the shot (fire arrow effects) | v1 |
| | Cold Arrow | Adds frost damage; the target is slowed (`Frost`) | v1 |
| | Multiple Shot | One shot splits into several arrows in a fan; uses one arrow | v1 |
| | Exploding Arrow | Fire damage in an area on impact | v1 |
| | Ice Arrow | Frost damage and a short freeze (`Immobilized`); bosses only slowed | v1 |
| | Freezing Arrow | Ice Arrow's freeze in an area | v1 |
| | Guided Arrow | The arrow homes in on the target (our own steering of the projectile) | later |
| | Strafe | A volley of arrows at several nearby targets | later |
| | Immolation Arrow | Exploding Arrow plus a burning patch on the ground | later |
| Passive and Magic | Critical Strike | Chance to deal double physical damage (crits are a new mechanic; the Barbarian's masteries reuse it) | v1 |
| | Dodge | Chance to avoid a melee hit while standing or attacking | v1 |
| | Avoid | Chance to avoid a projectile while standing or attacking | v1 |
| | Evade | Chance to avoid any hit while moving | v1 |
| | Inner Sight | Cast: nearby enemies glow and take more damage for a while | v1 |
| | Penetrate | Reworked (user, 2026-10-30): chance for a hit to ignore the creature's resistance to its damage type (a resistant or very resistant creature takes normal damage). Whether it also breaks immunity is open | reworked |
| | Pierce | Chance for an arrow or javelin to pass through and hit the next enemy | later |
| | Slow Missiles | Enemy projectiles near you slow down (they belong to their shooter's game, so it needs messages) | later |
| | Decoy | A tinted, standing copy of you that creatures attack instead | later |
| | Valkyrie | A tinted copy of a player character that fights for you | later |
| Javelin and Spear | Jab | A fast series of thrusts | v1 |
| | Power Strike | Adds lightning damage to the thrust | v1 |
| | Impale | One heavy thrust; wears the spear down faster | v1 |
| | Poison Javelin | The javelin leaves poison clouds along its path | v1 |
| | Lightning Bolt | The javelin turns into lightning: all its damage becomes lightning | v1 |
| | Plague Javelin | A spreading poison cloud where the javelin lands | v1 |
| | Charged Strike | The thrust releases small lightning bolts around the target | later |
| | Fend | Thrusts at several nearby enemies in turn | later |
| | Lightning Strike | The thrust releases chain lightning to nearby enemies | later |
| | Lightning Fury | The javelin bursts into lightning bolts on impact | later |

19 skills in v1, 10 later, none dropped. With the tree rule in §4, a skill built before its prerequisite (for example Strafe before Guided Arrow) opens by level alone.

**Decoy and Valkyrie look (user, 2026-10-30): a tinted clone of the player character.** Proposed approach, to prove in a prototype: clone the player prefab without the `Player` part, as a creature on the players' side (like `Skeleton_Friendly`) with monster AI; its equipment visuals (`VisEquipment`, synced through the object's ZDO) copy the caster's body, hair and gear, and a tint marks it (`Corrupted.Tint`'s method). The player model already plays every humanoid attack, so the Valkyrie can fight with the vanilla attacks of the weapon she holds. Risk: parts of the player prefab may expect the `Player` component; other mods have built human NPCs on the player model (unverified which and how).

Animations: thrusts, throws, bow shots and the cast all exist in vanilla (`spear_poke`, `spear_throw`, the bow's own draw and shot, `gpower` or an emote for Inner Sight). Jab needs the thrust replayed faster; the speed change doesn't sync, so it needs our own message, as for Whirlwind (§4).

## 5a. Choosing a class: the class stone

Decided (user, 2026-10-30): **a class is chosen at the spawn area** (the `StartTemple` location in the Meadows, where the boss trophies go), at **a new stone structure that blends in** with the area's standing stones.
- Name (user, 2026-10-30): the **Orlog stone** (*örlög*, fate).
- Proposed build: cloned from the start area's own stone prefabs (same mesh and material, no new model), placed once per world beside the trophy stones by the server (the start temple's position comes from the world's location list; the stone is a persistent object like the rip heart).
- Interact (E) to open the class choice (UI from the UI session); a character without a class gets a raven hint the first time it spawns, as the Riftforge's raven does.
- **Changing class** (user, 2026-10-30): possible at the Orlog stone, for a cost. Details later; not part of the first build.

## 6. Barbarian (later)

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

## 7. Paladin (maybe, third)

- **Auras** (one active at a time, as in D2): a status effect in a radius. Buffs for players (Might, Defiance, Prayer, Vigor, Meditation, the resist auras, Salvation) are straightforward; pulses on enemies (Holy Fire, Holy Freeze, Holy Shock, Sanctuary, Conviction) reuse elemental damage and effects.
- **Holy damage = spirit**, which Valheim's undead are weak to already.
- **Combat:** Zeal (rapid strikes), Charge (dash and hit), Sacrifice, Vengeance (elemental damage on hits), Holy Bolt (spirit projectile that heals allies), Blessed Hammer (spiralling projectile, new effect), Fist of the Heavens (lightning from the sky), Holy Shield. **Smite** is a shield bash: Valheim has none, so it needs a new animation. **Conversion** can use Valheim's taming. **Redemption** needs corpses: rework or drop.

## 8. Build order (proposed)

Each step is play-tested before the next.
1. **Levels:** XP from kills, sharing, level-up, D2 attributes with their effects, respec item. The XP bar and spending window come from the UI session (§9).
2. **Class choice, the tree rules and the Amazon's passives:** Critical Strike, Dodge, Avoid, Evade (no casting, animations or networking).
3. **Arrow modes:** the ability bar, cost and cooldowns, and the v1 bow skills; Inner Sight.
4. **Spear skills:** Jab, Power Strike, Impale, then the javelins (Poison, Lightning Bolt, Plague).
5. **The Amazon's `later` skills**, one by one. 6. **Barbarian.** 7. **Paladin** (if wanted).

Before step 1: check `docs/mod-compatibility.md` for every game method we'll patch (`Character.Damage` and `Character.RPC_Damage` are already patched by Corruption).

## 9. UI split (user, 2026-10-30)

The **XP bar and the skills panel** are built in the UI overhaul session (`claude/ui-overhaul`, CraftingOverhaul). This plugin owns the rules and the data and gives the UI what it needs through a small public API read by reflection (the same pattern as the character sheet reading EquipmentAndQuickSlots' JSON API), so CraftingOverhaul has no hard dependency:
- **Read:** class, level, XP and XP for the next level, unspent stat and skill points, attribute values, the skill table (tab, row, column, name, icon, required level, prerequisites, max points, status), each skill's hard points and effective level (with +skills and synergies).
- **Skill bar:** the 4 slots' skills and hotkeys, which slot is on the skill button, each slot's usable/cost/cooldown state.
- **Do:** spend a stat point, spend a skill point (refused with a reason when the rules say no), respec, put a skill in a slot or clear it.
- **Events:** level-up and points-changed, so the panel can refresh without polling.

**The full brief for the UI session, with the exact API contract (version 1) and a demo-data mode: [`classes-ui-brief.md`](classes-ui-brief.md).** Changes to the contract go there, in `master`.

### HUD layout (user, 2026-10-30)

Reference images (Elden Ring): `docs/images/hud_reference_bars.jpg` (bars), `docs/images/hud_reference_skills.jpg` (four slots in a cross).
- **Health, eitr and stamina bars together, horizontal, at the bottom centre** of the screen, Dark Souls / Elden Ring style: health on top, eitr in the middle, stamina at the bottom, in Valheim's colours (health red, eitr blue, stamina yellow). A general HUD change for every player; the class only makes eitr matter.
- **The 4 skill slots at the bottom left**, where vanilla's health display was, laid out as a cross like the reference.
- Proposed details for the UI session to settle: bar length grows with the maximum (as in the reference), up to a cap; the eitr bar shows only when there is eitr (always, for a character with a class); a new place for the food icons, which sit beside vanilla's health display; the slot on right click is highlighted; cost, cooldown and greyed-out state on each slot; the slot's hotkey on it. To check against what vanilla already shows near the bottom centre (e.g. the sailing controls, the mount's stamina) and against EquipmentAndQuickSlots' own quick-slot bar on the HUD.

## 10. Open questions (for the user)

1. ~~Own levels vs EpicMMOSystem~~ **Answered 2026-10-30:** our own.
2. ~~Skill resource~~ **Answered 2026-10-30:** eitr for every skill that costs mana in D2, with the eitr redesign in §4. Open: its numbers.
3. **Numbers:** ~~D2 rules or not~~ **answered 2026-10-30:** D2's rules, all changeable in config. XP: slow, like classic WoW, with the user's levelling guide per boss (§3). Still open: how attribute and skill values scale from D2 to Valheim stats.
4. ~~Choosing a class~~ **Answered 2026-10-30:** at the Orlog stone at the spawn area (§5a). Changing class: at the stone for a cost, details later.
5. ~~Casting~~ **Answered 2026-10-30:** D2 layout, a 4-slot skill bar with its own hotkeys; right click fires the skill for every weapon; block and parry move to a rebindable key; the skill button is rebindable too (§4).
6. ~~Names~~ **Answered 2026-10-30:** D2's class and skill names; the class stone is the Orlog stone; the plugin name doesn't matter (working name Orlog).
7. ~~Vanilla skills~~ **Answered 2026-10-30:** keep levelling by use.
8. ~~Death penalty~~ **Answered 2026-10-30:** Valheim's own skill loss only; no XP loss.
9. ~~Tree layout~~ **Answered 2026-10-30:** D2's three tabs, with level tiers instead of broken prerequisite chains and upgrade choices later (§4).
10. ~~Javelins~~ **Answered 2026-10-30:** spectral copy.
11. ~~Penetrate~~ **Answered 2026-10-30:** ignore the creature's resistance to the hit. Open: immunity too?
12. ~~Decoy and Valkyrie looks~~ **Answered 2026-10-30:** a tinted clone of the player character (feasibility to prove).
13. ~~Arrow modes~~ **Answered 2026-10-30:** no modes; the chosen skill fires on the skill button, normal shots stay on the attack button (§4).
