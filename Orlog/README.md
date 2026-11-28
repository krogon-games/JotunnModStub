# Orlog

Diablo 2-style classes for Valheim. Full design: `docs/class-design.md` in the repository.

**Status: levels, XP, attributes, the Orlog stone and the Amazon's passives, bow skills and Inner Sight.** Her spear skills come next. Every player and the server need the mod.

## Features so far

- **Character levels from XP**, up to level 99. Kills give XP: more for tougher creatures (by biome and health), bosses (Eikthyr is level 5 … Kall Fimbulbringer 90) and starred creatures (×2, ×3). Creatures far below your level give little, as in Diablo 2.
- **Shared XP:** players within 70 m of a kill share it, with a bonus for each extra player, split by level.
- XP stays with the character, like vanilla skills. Vanilla skills keep levelling by use. There is no XP loss on death.
- "+N XP" in the top-left corner for each kill (can be turned off); "Level N" and the skill level-up effect on a new level.
- **Attributes** (with a class; only the Amazon so far): spend 5 stat points per level on Strength (+1% melee and thrown damage each), Dexterity (+1% bow damage), Vitality (+1.5 health, +0.89 stamina) and Energy (+1.5 eitr). Each level also gives +1 health, +0.89 stamina and +1.5 eitr.
- **Eitr without eitr food:** a class starts with 15 eitr, which vanilla regeneration refills.
- **Skills:** the Amazon's 27 skills in three tabs of nine; spend 1 skill point per level on the 13 built ones. Any skill can be learned from level 1: no tiers, no prerequisites (at most 20 points per skill, and no more points in one skill than you have levels). Each boss's first kill gives bonus skill points (12 in all). The tooltips show each skill's numbers.
- **Passives:** Critical Strike (double physical damage), Dodge, Avoid and Evade (a creature's hit misses), Penetrate (ignores resistances, not immunities), with Diablo 2's chances.
- **Skill slots and the skill button:** drag skills into four slots; F1–F4 choose one, right click fires it (hold to draw the bow). Block has a key of its own (`BlockKey`, the mouse's back side button by default). Skills without a weapon cast straight from their key (quick cast; right-click the slot to toggle). Skills cost eitr.
- **Bow skills:** all nine work (Multishot, Freezing Arrow, Forked Lightning, Wind Lance, Strafe, Immolation Arrow, Venom Strike, Snipe and Shackle Shot), with bows and crossbows; each deals a share of the shot's own damage. **Inner Sight** marks nearby enemies: they take more damage. The spear and javelin skills can be learned but do nothing yet.

- **The Orlog stone** in the spawn area, among the boss trophy stones: use it to choose your class (only the Amazon so far). With CraftingOverhaul a window opens; without it, use the stone twice.

## Configuration

`BepInEx/config/com.krogongames.orlog.cfg`. The Levels settings are server-synced: the server's values apply to everyone.

| Setting | Default | Description |
|---|---|---|
| Levels / MaxLevel | 99 | Highest level |
| Levels / StatPointsPerLevel | 5 | Attribute points per level |
| Levels / SkillPointsPerLevel | 1 | Skill points per level |
| Levels / XpMultiplier | 1 | Multiplies all XP; 1 = classic WoW's pace without quests |
| Levels / ShareRange | 70 | Metres within which players share a kill's XP |
| Levels / PartyBonusPerPlayer | 0.35 | Extra XP per additional player sharing a kill |
| Levels / HighLevelPenalty | false | Diablo 2's XP cut from level 70 |
| Display / ShowXpGains | true | Show "+N XP" per kill (per player) |
| Attributes / HealthPerD2Life | 0.5 | Valheim health per Diablo 2 life point |
| Attributes / StaminaPerD2Stamina | 0.89 | Valheim stamina per Diablo 2 stamina point |
| Attributes / EitrStart, EitrPerLevel, EitrPerEnergy | 15, 1.5, 1.5 | The class eitr pool |
| Attributes / DamagePerStrength, DamagePerDexterity | 1, 1 | % damage per point |
| Skills / SkillDamageMultiplier | 1 | Multiplies the damage skills add |
| Skills / InnerSightDamageBase | 5 | % more damage enemies under Inner Sight take at skill level 1 |
| Skills / InnerSightDamagePerLevel | 1 | Inner Sight's extra % per level above 1 (24% at 20, no cap) |
| Skill Keys / SkillButton, BlockKey | Mouse1, Mouse4 | The skill button (right click) and block's key (the mouse's back side button; any key, `None` turns it off; per player) |
| Skill Keys / Slot1Key … Slot4Key | F1 … F4 | The four skill slots (per player) |

## Console commands (need `devcommands`, except `orlog_info`)

- `orlog_info`: your level and XP, the creature levels in the biome you're in, your skill slots, passive chances and boss first kills.
- `orlog_xp <amount>`: add XP.
- `orlog_level <level>`: set your level.
- `orlog_class Amazon` (or `none`): set the class directly (a cheat; players use the Orlog stone).
- `orlog_stat <strength|dexterity|vitality|energy> [points]`: spend stat points, until the UI exists (no `devcommands` needed).
- `orlog_skill <skill id> [points]`: spend skill points (no `devcommands` needed).
- `orlog_slot <1-4> [skill id]`: put a skill in a slot, or clear it (no `devcommands` needed).
- `orlog_passive <crit|penetrate|dodge|avoid|evade> <chance>` or `orlog_passive off`: test a passive with a set chance.
- `orlog_respec`: take back all spent stat and skill points.

## Changelog
- 0.10.0 (2026-11-28): a rip's **Glorious** map affix (Corruption 0.12.0) raises the experience its creatures give (up to 10×, read from the creature's data). Everyone and the server need 0.10.0.
- 0.9.0 (2026-11-26): works with Valheim 1.0.30. The game removed the network panel's `m_root`, which made the F2 skill-slot patch throw an error every frame; it now checks for it and, when it's missing, F2 opens the network panel as well as choosing the slot (one warning in the log). No other change.
- 0.8.0 (2026-11-18): **the last two bow skills work** (#70). **Snipe**: hold right click past full draw to charge (3.5 s to full at skill level 1, 1.5 s at 20); the damage climbs slowly then fast, from a normal shot to 150% of it at full charge (300% at 20), the arrow flies up to twice as fast, the view zooms up to 3×, stamina drains at the bows' drawing rate; 9 eitr, +0.25 per level; let go at full draw with no charge for a normal arrow and no eitr. **Shackle Shot**: 100% → 150% of the shot's damage; ties the struck enemy to the nearest enemy or standing tree up to 8 m behind it, both stunned 1.75 s (4 s at level 20; 0.6 s with nothing behind), bosses only slowed; 9 eitr, +0.25 per level. Both work with crossbows. No bow skill shows "Coming later" any more. Known: nothing tried with two players; the rope and swirls show on the shooter's game only.
- 0.7.0 (2026-11-17): **Strafe remade as rapid fire** (after Diablo 3's Rapid Fire; #70). Hold right click: the bow draws once, then streams 6 arrows a second where you aim for as long as you hold (a loaded crossbow: 3 bolts a second, no draw, empty and reloading after), each at 25% of the shot's damage at skill level 1, 50% at 20, with the arrow's own element. 10 eitr to start and 3 a second while firing (15 and 5 at level 20); one arrow or bolt a second from the quiver; you walk slowly and lose stamina as with a drawn bow. Let go before full draw and it's a normal arrow with no eitr. Replaces 0.6.0's volley at several enemies. Known: nothing tried with two players.
- 0.6.0 (2026-11-17): skill damage scales off the weapon (#72, PR #74) and three more bow skills work (#70, PR #74). Freezing Arrow's frost blast deals 50% of the shot's own damage at skill level 1, 150% at 20; each of Forked Lightning's forks 25% → 100% (Diablo 2's fixed damage points and the damage scale are gone; Multishot and Wind Lance are unchanged). **Strafe**: one arrow becomes a volley, each arrow at a different enemy in view within 16.5 m (up to 5 enemies at level 1, 10 from level 6; at least 2 arrows, 7 at level 20), 75% of the shot's damage each; 11 eitr. **Immolation Arrow**: explodes on its first hit, fire 25% → 100% of the shot's damage within 3 m, then the ground burns for 4 s at 10% → 30% a second; 6 eitr, +0.25 per level. **Venom Strike**: a poison cloud reaching 3 m hangs for 4 s where the arrow lands, poisoning for 25% → 100% of the shot's damage at once and each second; 7 eitr, +0.5 per level. Snipe and Shackle Shot still show "Coming later". Known: nothing tried with two players; Strafe doesn't check for walls or trees between you and an enemy.
- 0.5.0 (2026-11-16): two more bow skills work (#70, PR #71). **Forked Lightning**: the arrow hits as usual, then lightning strikes that enemy and forks to up to 3 other enemies within 4 m (7 at skill level 20); 4 eitr, +0.25 per level. **Wind Lance**: hold 1.5 s past full draw (the draw circle keeps narrowing, turns pale blue and pulses when ready), then release: a gust 0.75 m wide each side flies 50 m at twice an arrow's speed along the ground, through every enemy, with the arrow's damage (100% at level 1, 233% at 20), a knockback (force 100 to 200) and a stagger (not bosses); bushes and small plants let it through, trees, rocks and buildings stop it; released early it is a normal shot and costs no eitr; 8 eitr at level 1, 3.25 at 20. Works with crossbows. Five bow skills still show "Coming later". Known: Forked Lightning is too strong at low levels (a balancing pass comes later); other players don't see the wind or the fork lines.
- 0.4.0 (2026-11-15): the skill tree overhaul (#66, PR #68): no level tiers or prerequisites, so any working skill can be learned from level 1; every tab has exactly nine skills. The bow tab's new list: Forked Lightning, Wind Lance, Snipe, Multishot (Multiple Shot renamed), Strafe, Freezing Arrow, Immolation Arrow, Venom Strike, Shackle Shot; only Multishot and Freezing Arrow work so far, the other seven show "Coming later". Magic, Fire, Cold, Ice and Exploding Arrow are removed with their code; Guided Arrow, Slow Missiles and Power Strike leave the lists. Points spent in a removed skill come back as unspent points, and a slot that held one is emptied.
- 0.3.0 (2026-11-15): the Amazon: her five passives (Critical Strike, Penetrate, Dodge, Avoid, Evade) with Diablo 2's chances; right click fires the chosen skill, F1–F4 use the four skill slots, block moves to the mouse's back side button (`BlockKey`, rebindable); the bow skills and Inner Sight; quick cast per skill; a skill point for each boss's first kill (12 in all); bosses give more XP (×9.4 for Eikthyr up to ×66.6 for Kall Fimbulbringer). F2 opens the game's stats window only with Ctrl held, for a character with a class.
- 0.2.0 (2026-11-08): review fixes (PR #57): stat and skill point totals can't overflow, and a lowered `StatPointsPerLevel` keeps the points already spent.
- 0.1.0 (unreleased): first version.
