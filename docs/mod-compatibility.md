# Compatibility with other mods

Mods the group plays with alongside ours, what they change, and where they meet our code. Checked 2026-10-28 by decompiling each mod's DLL and comparing its Harmony patch targets with ours (`FEATURES.md`).

**Status values:** **Verified in code** (read their code and ours; no in-game test yet) · **Tested in-game** (user's report) · **Risk** (needs a fix or a test).

**Rule for new work:** before patching a game method, check the tables below for mods that patch it too, and add our new targets here when they overlap.

## The mods

| Mod | Version checked | What it does | Needed on |
|---|---|---|---|
| [RandyKnapp-EquipmentAndQuickSlots](https://thunderstore.io/c/valheim/p/RandyKnapp/EquipmentAndQuickSlots/) (EAQS) | 3.1.3 | Dedicated slots for equipped items (head, chest, legs, shoulder, utility, trinket), up to 6 quick slots, optional extra utility slots, keep-on-death options. Has an API for other mods to add custom slots | Clients; server optional (syncs its balance settings) |
| [Advize-PlantEasily](https://thunderstore.io/c/valheim/p/Advize/PlantEasily/) | 2.3.1 | Plant crops in grids, snap to grids, bulk harvest pickables and beehives, replant on harvest | Clients |
| [Vapok-AdventureBackpacks](https://thunderstore.io/c/valheim/p/Vapok/AdventureBackpacks/) (AB) | 2.4.6 | Craftable backpacks per biome (own models and textures) with their own inventory, carry-weight and movement modifiers and perks; craft and build from the backpack; auto-store loot | Everyone (adds items) |

EAQS and AB know about each other (AB adjusts a UI bar when EAQS is present) and neither declares the other incompatible. EAQS refuses to load next to other slot mods (AzuExtendedPlayerInventory, ExtendedPlayerInventory, ExtraSlots, ComfyQuickSlots).

## CraftingOverhaul

| Our feature | Overlap | Status |
|---|---|---|
| **Craft and build from nearby chests** vs **AB craft-from-backpack** | Both patch `Inventory.CountItems(string,int,bool)`, `Inventory.RemoveItem(string,int,int,bool)` and `Player.HaveRequirementItems`, and both mark "crafting in progress" around the same UI methods. Counting adds up correctly: vanilla inventory + AB's backpack (minus equipped items) + our chests. **Paying had a bug:** when the backpack covered the whole cost, AB paid and skipped the original method, and our patch then took the full cost from chests again (Harmony runs every prefix even after one skips the original). **Fixed 2026-10-28:** our prefix now runs after other mods' (low priority) and does nothing when another mod already skipped the original (`__runOriginal`). Partial payment works in this order: backpack first, then chests, then the player's inventory | Fixed in code; **to test in-game** |
| Nearby chests vs **AB's backpack container** | AB opens the backpack through a hidden container attached to the player (`AB_BackpackProxy`). Our chest finder tracks every container, so it could have counted the backpack a second time. **Fixed 2026-10-28:** containers carried by a player are never used | Fixed in code; to test |
| Nearby chests vs **PlantEasily** bulk planting | PlantEasily raises the seed cost for a whole grid, checks it with `Player.HaveRequirements` and pays with `ConsumeResources` inside `Player.UpdatePlacement`, which is inside our building scope. So bulk planting can use seeds from nearby chests, consistently | Verified in code; to test |
| Nearby chests vs PlantEasily **replant on harvest** | Replanting pays inside `Player.Interact`, outside our building scope, so replanting only uses seeds you carry | Known limit (harmless) |
| Nearby chests vs **EAQS** | EAQS keeps equipped items in extra rows of the player inventory; vanilla counting already includes them, as without EAQS. EAQS's `Container.Awake` patch is for gravestones, which we never use | Verified in code |
| Recipe search and categories | AB patches `InventoryGui.UpdateRecipeList`/`UpdateRecipe` only to mark crafting in progress (no list changes). AB can open the backpack panel next to a crafting station, and EAQS adds an equipment panel to the inventory screen; our search bar sits above the recipe list | Verified in code; **check the layout in-game** |
| Biome detection | No overlap | — |

## Corruption (built)

| Our feature | Overlap | Status |
|---|---|---|
| Talismans (trinket slot) | EAQS has a Trinket slot and equips through the vanilla methods (`Humanoid.EquipItem`), so the game's trinket field and the synced trinket shown to other players stay as vanilla. Our repel check reads exactly those | Verified in code; to test with EAQS |
| Talismans (`BaseAI.IsEnemy`, `BaseAI.OnDamaged`) | None of the three patch these | — |
| Shard drops (`CharacterDrop.GenerateDropList`) | No overlap | — |
| Riftforge | No overlap | — |

## Corruption (planned): keep in mind when building

| Planned feature | Overlap | Note |
|---|---|---|
| Affix tooltips (`ItemData.GetTooltip` postfix) | AB also postfixes `GetTooltip` (backpack contents) | Several postfixes are fine; append our lines, don't replace the text |
| Light affix (`ItemData.GetWeight`) | AB postfixes `GetWeight` (backpack weight) | Light only applies to tools; multiply, don't overwrite |
| Magnetic / Keen-eyed (pickup radius) | EAQS patches `Player.AutoPickup` (fills quick slots) | We change the radius field, not the method; compatible |
| Deathless (cheat death) | EAQS patches `Character.CheckDeath` and `Player.OnDeath` | Hook the damage before death, test with EAQS keep-on-death |
| Parry buffs on allies (`SEMan.AddStatusEffect`) | AB patches `SEMan.AddStatusEffect` | Test that our buffs still apply with AB |
| Foraging affix (picking plants) | PlantEasily bulk-harvests with `Player.Interact` / `Pickable.SetPicked` | Treat a bulk harvest as one pick (the boost refreshes, doesn't stack) |
| Affixes on capes | AB backpacks are shoulder items (the cape slot) | Decide whether backpacks can roll cape affixes |
| Movement-speed cap (35%) | AB backpacks have their own movement modifiers | Our cap only limits affixes; backpack modifiers come on top |
| Equipment slots for rings, inventory tabs (design phase 9) | EAQS already has equipment slots and an API for custom slots | Use the EAQS API for a ring slot instead of building our own |
| WoW-style backpack (design phase 9) | AB provides it | **Use AdventureBackpacks instead of building our own** (user, 2026-10-28) |

## How to test (when playing)

1. Craft something whose ingredients are only in the backpack, with the same items in a nearby chest: the backpack alone should pay.
2. Then split the cost: some in the backpack, the rest in a chest. Check the totals after crafting.
3. Build with the hammer from backpack + chest, and bulk-plant a PlantEasily grid with seeds only in a chest.
4. Open a crafting station with the backpack panel open: the search bar and categories should still line up.
5. With EAQS: wear a talisman in the EAQS trinket slot and check creatures of its family ignore you (and that a friend sees the same).
