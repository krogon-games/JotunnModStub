# Corruption

A Diablo/Path of Exile-style endgame layer for Valheim. Full design: `docs/corruption-design.md` in the repository.

**Status: phases 1–2 of 10.** Every player and the server need the mod.

## Features so far

- **Corruption shards.** The mod's currency: a deep-red gem.
  - 1-star creatures sometimes drop one (2%), 2-star creatures a bit more often (5%).
  - Every boss kill gives each player nearby 1 shard, plus 1–3 the first time it's defeated in a world (3–5 from a corrupted boss).
  - 1 in 20 vanilla dungeon and location chests holds one; a destroyed rip heart spills more.
- **Riftforge.** A new crafting station, built with the hammer next to a workbench from 20 fine wood, 10 bronze nails, 5 Surtling cores and 10 amber. It crafts maps, corrupted offerings and the corrupted wishbone, and its **Forge tab** is where gear is changed with orbs and shards: corrupt an item with a shard, shatter Magic and Rare gear into Affix Splinters, put splinters on other gear.
- **Magic and Rare gear.** Creatures drop weapons, armour and tools with rolled affixes (blue Magic with 1–2, yellow Rare with 3–4), mostly from their own biome. Gathering tools get gathering affixes (more drops, whole-tree harvests, orb finders and more).
- **Currency orbs** (Path of Exile's ten, the Mirror of Kalandra included). Any kill can drop any of them (each player nearby rolls for themselves; stars and rips raise the chance), and so can vanilla dungeon chests and a destroyed rip heart. About 6 orbs an hour of fighting, mostly the common ones; the Divine Orb about once in 10,000 kills. Used in the Riftforge's Forge tab.
- **Personal loot and loot beams.** A drop rolled for one player (their kill loot, their boss shard, the orbs their Hoardfinder's or "of Hidden Riches" tool finds, their share of a rip heart's loot) is theirs for 60 seconds: other players don't see it and can't pick it up; then it appears for everyone. Items a player drops from their own inventory are never locked. Every Corruption drop but boss essences has a soft, slowly pulsing beam of light in its colour (Magic blue, Rare yellow, unique orange-gold, orbs white, shards red, maps green), tall for Rare gear, maps and the rare orbs (with a sound for their owner), short for the rest.
- **Inventory tabs.** Six inventory tabs (Bag, Currencies, Splinters, Maps, Runes, Essences); the mod's items go to their own tab on pickup and weigh nothing.
- **Talismans** ("<creature> charm"). Craft at the workbench from 3 of a creature's trophies, 1 chain and 10 leather scraps. Placed in a Talisman slot, a charm stops that creature family (e.g. greydwarfs, brutes and shamans) from attacking you, unless you hit them first. Bosses are never repelled. You get 2 talisman slots on top of the vanilla trinket slot. The slots appear in the equipment panel of [EquipmentAndQuickSlots](https://thunderstore.io/c/valheim/p/RandyKnapp/EquipmentAndQuickSlots/), which talismans need.
- **Boss essences.** Every boss drops 1 essence per kill, each a glowing gem in its boss's colours, used at the Riftforge to craft maps into that boss's biome (Eikthyr: Meadows, up to Kall Fimbulbringer: Deep North).
- **Maps and rips.** Craft a "{biome} map" at the Riftforge from a shard, a boss essence and a biome material, then use it: a rip opens to unexplored land of that biome, away from everyone's bases. Inside, waves of the biome boss's army attack while the rip's heart stands, 96–128 m away (a corrupted wishbone points to it); destroy it to open the way home: its loot spills on the ground (no chest): each player's own orb and Magic/Rare gear rolls, and the shared part (the biome's vanilla chest loot, 1–3 shards and one new map) dealt out among the players near it. Once a world's first rip is cleared, creatures can drop maps of their own biome (1 in 830, after that biome's boss is beaten). Rips close after an in-game day.
- **Corrupted bosses.** Make a corrupted offering at the Riftforge (the boss's normal offering plus a shard) and offer it to summon a red, corrupted version: it hits 1.5× harder, takes 30% less damage, calls in its biome's army and drops 2–3 essences, 3–5 shards and one guaranteed orb for each player nearby.

## Configuration

`BepInEx/config/com.krogongames.corruption.cfg`. These settings are server-synced: the server's values apply to everyone.

| Setting | Default | Description |
|---|---|---|
| Corruption Shards / OneStarChance | 0.02 | Chance a 1-star creature drops a shard |
| Corruption Shards / TwoStarChance | 0.05 | Chance a 2-star creature drops a shard |
| Corruption Shards / BossShardsMin | 1 | Fewest shards from a boss |
| Corruption Shards / BossShardsMax | 3 | Most shards from a boss |
| Corruption Shards / BossShardsFirstKillOnly | true | Bosses drop shards only on their first defeat per world |
| Talismans / Enabled | true | Charms repel their creature family |
| Talismans / Slots | 2 | How many talisman slots work (1–3) |
| Talismans / ProvokeSeconds | 30 | How long a creature you hit fights back |
| Talismans / ExcludedCreatures | \*Hildir\*, GoblinBruteBros\*, Charred_Melee_Dyrnwyn | Creatures never repelled |
| Essences / Enabled | true | Bosses drop their essence |
| Rips / Enabled | true | Maps open rips |
| Rips / OpenDays | 1 | How long a rip stays open (in-game days) |
| Rips / BaseDistance | 0 | Rips never open this close (m) to a bed, ward or portal; 0 = a ward's radius |
| Rips / HeartHealth | 0 | Health of a rip's heart; 0 = a greydwarf nest's health |
| Rips / MinStars | 1 | Fewest stars a rip wave creature has |
| Rips / MaxStars | 2 | Most stars a rip creature can have |
| Rips / WaveMultiplier | 2 | Rip waves: twice as many alive at once, twice as often, as the vanilla raid |
| Rips / ChestShardsMin–Max | 1–3 | Shards a destroyed rip heart spills (shared among the players near it) |
| Rips / KillShardMultiplier | 2 | Rip creatures drop shards this many times as often |
| Corrupted Bosses / DamageDealt, DamageTaken | 1.5, 0.7 | Corrupted boss damage multipliers |
| Corrupted Bosses / EssencesMin–Max, ShardsMin–Max | 2–3, 3–5 | Corrupted boss drops |

Look settings (per player, not synced, restart to apply):

| Setting | Default | Description |
|---|---|---|
| Look / ShardBaseItem | GemstoneRed | Vanilla item whose model the shard uses |
| Look / ShardColor | #B3170D | Tint colour |
| Look / ShardTintStrength | 0.35 | 0 = base colours, 1 = fully the tint colour |
| Look / ShardCustomModel | true | The shard's own crystal model; off = tinted Bloodstone |
| Look / ShardGlowIntensity | 2 | Red glow around a dropped shard; 0 = off |
| Look / ShardGlowRange | 2.5 | How far the glow reaches (m) |
| Look / RiftforgeColor | #B3170D | Riftforge tint colour |
| Look / RiftforgeTintStrength | 0.6 | 0 = Galdr table colours, 1 = fully the tint colour |

## Changelog
- 0.10.0 (2026-11-26): loot round (#35): **personal loot**: a drop rolled for one player (kill loot, each player's boss shard, the orb finders' orbs, their share of a rip heart's loot) is invisible to the others and theirs alone for 60 s, then free for everyone; items dropped from an inventory are never locked. **Loot beams** on every Corruption drop (colour by kind, tall or short, soft, tapered, pulsing, with smoke-like wisps; the level-up sound for the owner on tall beams); numbers in `personal_loot.csv` and `loot_beams.csv`. **No rip chest any more:** the heart spills its loot, each player's own rolls theirs and the shared part dealt among the players near it. New console command `co_personal` (testing). A network message changed, so everyone and the server need this build. Tested alone on Valheim 1.0.30.
- 0.9.0 (2026-11-26): loot round (#33): **currency orbs drop** from every kill (per player nearby), from vanilla dungeon chests (once, when the game fills them) and from the rip chest (per player); rates in `orbs.csv`, Path of Exile's order (Transmutation 1 in 50 … Divine 1 in 10,000, Mirror 1 in 21,400). **Stonecutter's and Resinous are renamed Hoardfinder's and "of Hidden Riches" and become orb finders**: a 1–5% chance per rock or tree of one orb (old tools and splinters load with the new names). Splintering reaches 4 m. **Loop fixes:** maps are named "{biome} map"; every rip chest holds one map and rolls Magic/Rare gear per player; creatures drop their biome's map (1 in 830) once a rip is cleared and that biome's boss is beaten; every boss kill gives each player nearby 1 shard; corrupted bosses give each player one orb; 1 in 20 dungeon chests holds a shard; the Riftforge costs 20 fine wood, 10 bronze nails, 5 Surtling cores and 10 amber. New console commands `co_orbroll` and `co_mapdrops` (testing). Tested on Valheim 1.0.30.
- 0.8.0 (2026-11-18): area damage over time (Orlog's Immolation Arrow burning ground and Venom Strike cloud) no longer triggers on-hit affixes such as life steal; Immolation Arrow's blast still counts as a hit of the bow (Path of Exile's rule; Krogon's decision of 2026-11-18, PR for #70).
- 0.7.0 (2026-11-11): loot round (#29, PR #61): at the Riftforge's Forge tab, corrupt gear with a Corruption shard (destroyed, locked, or one corrupted affix that counts beyond the stat caps), shatter Magic and Rare gear into Affix Splinters (room checked first, all or nothing) and put splinters on Normal and Magic gear with their exact values (a completed twin: both values plus 25%); 67 splinter looks in nine shapes; one confirmation dialog for Shatter and Corrupt, each with a remembered "Don't warn me again". Passed in single player; multiplayer untested.
- 0.6.0 (2026-11-10): UI round (#30): six inventory tabs (Bag, Currencies, Splinters, Maps, Runes, Essences; essences move to their own tab, currencies keep fixed places, Runes is an empty placeholder) and a Forge tab on the Riftforge panel (put gear in, right-click an orb, click to use it; Shatter greyed out until #29); loot tooltips everywhere use the Forge bench's bulleted affix layout.
- 0.5.0 (2026-11-08): loot round (PR #58): ten currency orbs in the Currency tab with their Riftforge rules (tried with the `co_craft` test command; no drops yet; Jeweller's greyed out), Corrupted and Mirrored item lines, the Mirror of Kalandra can copy corrupted items; Stonecutter's and Resinous roll on each damaging gathering hit.
- 0.4.0 (2026-11-08): review fixes (PR #57): a corrupted boss summon stays one corrupted boss when more offerings arrive during the spawn delay (one corrupted item used); a saved affix value that isn't a finite number is refused; the talisman's fight-back time survives a change of owner.
- 0.3.0 (2026-11-08): gathering review fixes (PR #55): Seismic and Odin's Grace break the touching chunks on your own deposits too, Veinbreaker's count starts over whenever the tool is unequipped, only your own swings get your tool's on-harvest affixes; test command `co_own`.
- 0.2.0 (2026-11-08): everything merged into `master` since 0.1.0, up to the code-review fixes (PR #53): safe tab loading, Magic/Rare loot on both death paths, arrows use their own bow's affixes.
- 0.1.0 (unreleased): corruption shards, the Riftforge, talismans, boss essences, maps and rips (travel only).
