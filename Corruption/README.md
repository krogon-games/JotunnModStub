# Corruption

A Diablo/Path of Exile-style endgame layer for Valheim. Full design: `docs/corruption-design.md` in the repository.

**Status: phases 1–2 of 10.** Every player and the server need the mod.

## Features so far

- **Corruption shards.** The mod's currency: a deep-red gem.
  - 1-star creatures sometimes drop one, and 2-star creatures a bit more often.
  - Every boss drops 1–3 the first time it's defeated in a world.
- **Riftforge.** A new crafting station, built with the hammer next to a workbench from fine wood, a corruption shard, Eikthyr's trophy, bronze nails and Surtling cores. It appears in the build menu once you've picked up all of those. It has no recipes yet; maps come in a later phase.

- **Talismans** ("<creature> charm"). Craft at the workbench from 3 of a creature's trophies, 1 chain and 10 leather scraps. Placed in a Talisman slot, a charm stops that creature family (e.g. greydwarfs, brutes and shamans) from attacking you, unless you hit them first. Bosses are never repelled. You get 2 talisman slots on top of the vanilla trinket slot. The slots appear in the equipment panel of [EquipmentAndQuickSlots](https://thunderstore.io/c/valheim/p/RandyKnapp/EquipmentAndQuickSlots/), which talismans need.
- **Boss essences.** Every boss drops 1 essence per kill, each a glowing gem in its boss's colours. They will be used at the Riftforge to craft maps into the next biome.

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
- 0.1.0 (unreleased): corruption shards, the Riftforge, talismans and boss essences.
