# Valheim Storage and Quest Systems — Developer Brief

27 Oct 2026 · @Krogon

> Saved for later exploration; nothing here is planned or built yet. The brief is kept as written. Notes from the codebase and game data are in a separate section at the end.

## Overview

Three storage and progression features for a four-player Valheim mod: chest tiers with more slots, a super chest that auto-sorts deposits, and a Mistlands bank and vault. Each builds on the one before it, so build them in this order.

## Feature 1: Chest tiers add slots

Each chest tier adds more slots instead of bigger stacks. Item stack sizes stay the same everywhere, so the player inventory and merging logic are unchanged.

- Set slot count per chest tier (for example, a basic chest at 4×4, the next at 6×6, the top at 8×8).
- Reduce the material cost of each tier slightly, since the larger capacity makes each chest more valuable.
- Do not change `maxStackSize` on items for this feature.

## Feature 2: Super chest unlocked by quest

A quest unlocks the super chest. When a player deposits items into it, the chest sorts each item into nearby chests that already contain that item.

- Trigger sorting on deposit, not on a timer, to keep performance steady.
- Search a fixed radius for matching chests. Radius to be confirmed (see open questions).
- Fill matching chests that have room. Choose the nearest chest first, then the least full.
- Overflow: items that do not fit stay in the super chest. Confirm this rule before build.

## Feature 3: Mistlands bank and vault

A Mistlands quest recruits a Dvergr banker. Once recruited, the player can build a bank and a vault. The vault is one large virtual storage space, and its capacity is the combined slots of all chests in the base.

- Use a virtual storage model: the vault is a data inventory, and chests supply only capacity.
- Deposits into the bank fill existing stacks of the same item first, then open slots.
- Vault capacity updates when chests are added, upgraded, or destroyed.
- If chests are destroyed and stored items exceed the new capacity, follow the overflow rule confirmed in open questions.

## Multiplayer and persistence

The server owns all storage state. Clients only request actions and receive updates.

- Sorting and vault deposits run on the server. Clients show the result.
- Concurrent deposits must not corrupt counts. Serialize writes per container or vault.
- Sync changed container contents to all clients after each sort or deposit.
- Save vault and super chest state with the world save, and reload it on server start.
- All four players must run the mod. Mismatched versions will desync stack limits.

## Open questions

- [ ] Overflow rule for the super chest: stay in the chest, go to the player inventory, or drop at the base?
- [ ] Search radius for matching chests, and whether they must be on the same base.
- [ ] Vault shrink rule: block removal of a chest, drop overflow at the base, or hold excess until space frees?
- [ ] Vault access: per player or shared across the group (default: shared)?
- [ ] Quest trigger for the Dvergr banker, and how quest progress is tracked without a built-in quest system.
- [ ] Confirm the current trader count and chest material costs in the target Valheim build. *(Partly answered below.)*

---

## Notes from the codebase and game data (2026-10-27)

**Answers from the Valheim 1.0.20 dump (`data/game-dump-1.0.20/`):**

- **Traders: 3.** Haldor (`Vendor_BlackForest`, Black Forest), Hildir (`Hildir_camp`, Meadows) and the Bog Witch (`BogWitch_Camp`, Swamp).
- **Chest costs** (`pieces.csv`):

| Chest | Prefab | Cost |
|---|---|---|
| Chest | `piece_chest_wood` | Wood ×10 |
| Barrel | `piece_chest_barrel` | Wood ×10, Barrel rings ×1 |
| Reinforced chest | `piece_chest` | Fine wood ×10, Iron ×2 |
| Personal chest | `piece_chest_private` | Fine wood ×10, Iron ×8 |
| Black metal chest | `piece_chest_blackmetal` | Wood ×10, Tar ×2, Black metal ×6 |
| Wardrobe | `piece_chest_warderobe` | Frostwood ×10, Tar ×2, Black metal ×6 |
| Grausten chest | `piece_chest_grausten` | Grausten ×10, Charred warrior trophy ×5, Flametal ×2 |
| Treasure chest | `piece_chest_treasure` | Coins ×99, Ruby ×5, Silver necklace ×2, Fine wood ×8, Silver ×2 |

- **Slot counts aren't in the dump yet.** Adding container width × height to `co_dump`'s `pieces.csv` is a small change, worth making before Feature 1.

**Things to settle before building:**

- **"The server owns all storage state" needs adjusting for how Valheim works.** In vanilla, a chest's contents are owned by whichever player's game currently owns that chest, not by the dedicated server (see `docs/coop-networking.md`). Two practical options:
  - make the server authoritative only for the vault, a new data-only store that we control;
  - for real chests (super-chest sorting), take ownership of the chest just before writing, as CraftingOverhaul's craft-from-chests already does.
- **Reuse:** CraftingOverhaul's nearby-chest code (`Containers/NearbyContainers.cs`) already finds accessible chests in a radius. It skips private chests, chests inside wards, chests in use, ships and carts. It takes items from them safely. Super-chest sorting is the same work in the other direction.
- **Which plugin?** This needs every player to run the mod. That fits the Corruption plugin's multiplayer model rather than CraftingOverhaul's, which works with only some players installed. Or it could be a third plugin.
- **Quest tracking:** Valheim has no quest system, but world-wide progress can be stored the way the game already does it, as global keys (as the bosses and Hildir's quests do), or per player as player keys. Hildir's quests use items handed in to a trader, which the biome resolver already reads.
