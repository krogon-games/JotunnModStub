# Riftforge window brief (for the UI session)

**From:** the loot session (`claude/affixes-loot`), 2026-11-01. **For:** the UI session (`claude/ui-overhaul`), which builds the window (ROADMAP §4.4 U3), and the rips session, which adds map rolling (ROADMAP §4.3 R1).
**Status:** draft; the orb questions in §6 were answered by the user on 2026-11-08 (§2 updated). The rules behind it (ROADMAP L5) are built by the loot session against this contract.

The Riftforge is where loot is crafted: orbs, corrupting with a shard, shattering, Affix Splinters and (later) rolling maps. This brief is the contract between the screen and the rules, so both sides can be built at the same time. Design and reasons: [`corruption-design.md`](corruption-design.md) → Currency orbs, Corrupted gear, Affix Splinters and shattering, Map affixes and tiers; affix rules: [`AFFIX_RULES.md`](AFFIX_RULES.md).

## Who builds what

| Loot session (rules, `Corruption/Rules/` and `Corruption/Content/Riftforge*.cs`) | UI session (the window, new files under `Corruption/Content/`) |
|---|---|
| Which operations an item and a material allow, and why one is refused | Where the window opens and how it looks (mock-up first, the user picks) |
| The preview text and the chances; the result message | Item and material slots, the operation list, the preview, the confirm button, the result |
| Applying the operation: changing the item, using up the material, adding created items to the inventory | Refreshing after an operation; showing the result |
| The items themselves: orbs, the Affix Splinter, the item tooltips | Nothing in `Corruption/Rules/` |

Map rolling is the rips session's: it adds its operations to the same API (§4), so the window shows them with no extra work.

## 1. The window

- **Where:** the UI session's call (ROADMAP U3): a tab on the Riftforge's crafting panel, or a window when using the station. It only works while the player is at a Riftforge (the rules check too).
- **Two slots**, both pointing at items in the player's own inventory (Bag or Currency tab). Nothing is moved into the window, so nothing can be lost if it closes:
  - **Item:** the gear (or map) being worked on.
  - **Material:** an orb, a Corruption shard or an Affix Splinter. Empty for shattering.
- **Operation list:** what this item and material allow (§2), each enabled or greyed with its reason.
- **Preview** of the chosen operation, then **Confirm**, then the **result message**. The item slot keeps the item after an operation (so a player can keep crafting); it empties when the item is destroyed (bricked or shattered).
- Item tooltips are the normal ones (the loot tooltip code); the window doesn't draw its own affix lines.

## 2. Operations

The material decides the operation; shattering has no material. "Refused" lists the reasons in the order they're checked; the first that applies is shown. Every operation is also refused when the player isn't at a Riftforge ("Use this at a Riftforge").

| Operation (id) | Material | Allowed on | Refused when (text) | Preview shows | Result |
|---|---|---|---|---|---|
| Orb of Transmutation (`orb:transmutation`) | the orb | Normal items | not Normal ("Only on Normal items"); corrupted ("Corrupted items can't be changed"); unique ("Uniques only take Divine and Jeweller's orbs") | "Becomes Magic with 1 random affix" | The item, now Magic |
| Orb of Augmentation (`orb:augmentation`) | the orb | Magic items with 0 or 1 affix (user, 2026-11-08) | not Magic ("Only on Magic items"); 2 affixes ("This item has no room for another affix"); corrupted; unique | "Adds 1 random affix; stays Magic" | The item with one more affix, still Magic |
| Orb of Alchemy (`orb:alchemy`) | the orb | Normal items | as Transmutation | "Becomes Rare with 3–4 random affixes" | The item, now Rare with a Rare name |
| Regal Orb (`orb:regal`) | the orb | Magic items with exactly 2 affixes (user, 2026-11-08; was "any Magic item") | not Magic; not exactly 2 affixes ("Only on Magic items with 2 affixes"); corrupted; unique | "Becomes Rare, keeps its 2 affixes and adds 1" | The item, now Rare with 3 affixes |
| Exalted Orb (`orb:exalted`) | the orb | Rare items with room | not Rare ("Only on Rare items"); full; corrupted; unique | "Adds 1 random affix" | The item with one more affix |
| Divine Orb (`orb:divine`) | the orb | Magic, Rare, unique | Normal ("This item has no affixes"); corrupted | Each line with its range: "+24% damage [4–19] → rerolled" | The item with new values |
| Orb of Annulment (`orb:annulment`) | the orb | Magic, Rare | no affixes; corrupted; unique | "Removes 1 random affix; the item keeps its rarity"; a resonant line loses one half and the other half rerolls in its own range | The item with one affix fewer, same rarity even at 0 affixes (user, 2026-11-08) |
| Orb of Chance (`orb:chance`) | the orb | Normal items | as Transmutation | "50% Magic · 30% Rare · 20% Unique"; when no unique can come from this item: "Becomes Rare" (user, 2026-11-08) | The item at its new rarity |
| Jeweller's Orb (`orb:jeweller`) | the orb | — | always ("Sockets come later") | — | Shown greyed until sockets and runewords are designed; can't be used or used up (user, 2026-11-08) |
| Mirror of Kalandra (`orb:mirror`) | the orb | Magic, Rare, corrupted ones too (user, 2026-11-08) | Normal; already mirrored ("Mirrored items can't be copied"); unique (the design: uniques only take Divine and Jeweller's orbs) | "Makes an exact copy; the copy can never be changed or mirrored" | The item, unchanged, plus its copy in the inventory; the copy keeps the affixes and any corruption |
| Corrupt (`corrupt`) | a Corruption shard | Normal, Magic, Rare | already corrupted ("Already corrupted"); unique ("Uniques can't be corrupted") | The chances from `corruption_outcomes.csv`: "25% destroyed · 25% unchanged, locked · 50% gains a corrupted affix" | One of the three; the message says which. Destroyed: the item is gone |
| Shatter (`shatter`) | none | Magic, Rare | Normal ("Only Magic and Rare items shatter"); unique or corrupted ("Uniques and corrupted items can't be shattered") | Each line with "50%": the chance it becomes a splinter (`shattering.csv`); "The item is destroyed" | The item is gone; 0–4 Affix Splinters in the inventory, listed in the message |
| Apply splinter (`splinter`) | an Affix Splinter | Normal and Magic items with room | Rare, unique or corrupted ("Splinters only go on Normal and Magic items"); 2 affixes already, or a resonant splinter on an item with any ("This item has no room for this splinter"); the item can't roll that affix ("This affix can't roll on this item"); the same family already on that side ("This item already has that kind of affix") | The exact line it adds, e.g. "+12% attack speed [3–15]"; "Becomes Magic" for a Normal item | The item with the line; the splinter is used up |
| Map rolling (`map:*`) | the rips session's | maps | the rips session's | the rips session's | the rips session's |

General rules (from the design): orbs never destroy an item; only the shard can. A corrupted item is locked: no operation works on it, except the Mirror of Kalandra (user, 2026-11-08). Every operation uses up one material item; shattering uses none. Divine and Annulment on a resonant line follow `AFFIX_RULES.md` → Twin mods and resonance.

## 3. What a player sees

- **Greyed operations** show their refusal text, so the player learns the rules from the window.
- **Random results** can't be previewed exactly: the preview says what kind of change happens and, where there's a table, the chances.
- **The result message** is one or two lines in the rarity colour, e.g. "Grim Fang gained +12% fire damage", "The shard shattered the Bronze Sword", "Shattered: 2 splinters (Swift +12% attack speed; of Embers +30% fire)".

## 4. API contract (version 1)

Same plugin (Corruption), so plain C# calls, no reflection. Static class **`Corruption.Content.RiftforgeCrafting`**; everything works on the **local player** and items in that player's inventory.

```csharp
internal static class RiftforgeCrafting
{
    public const int ApiVersion = 1;

    /// Every operation this item and material could mean, allowed or not, in the order to list them.
    /// material may be null (then only material-free operations, e.g. shatter).
    public static List<RiftforgeOption> Options(ItemDrop.ItemData item, ItemDrop.ItemData material);

    /// Does the operation: changes or removes the item, uses up the material, adds created items to the inventory.
    /// Checks everything again first; never throws (failures come back as a refusal).
    public static RiftforgeResult Apply(ItemDrop.ItemData item, ItemDrop.ItemData material, string operationId);

    /// Whether an inventory item can go into the material slot (orbs, the Corruption shard, Affix Splinters, map materials).
    public static bool IsMaterial(ItemDrop.ItemData item);
}

internal sealed class RiftforgeOption
{
    public string Id;            // "orb:regal", "corrupt", "shatter", "splinter", "map:roll", ...
    public string Name;          // "Regal Orb", "Corrupt", "Shatter", ...
    public bool Allowed;
    public string Refusal;       // "" when allowed, else the text in §2
    public List<string> Preview; // the lines in §2 "Preview shows", rich text allowed (colours)
}

internal sealed class RiftforgeResult
{
    public bool Done;                 // false: refused, nothing changed
    public string Message;            // the result message, or the refusal
    public ItemDrop.ItemData Item;    // the item afterwards, or null when it was destroyed
    public List<ItemDrop.ItemData> Created = new List<ItemDrop.ItemData>(); // splinters, the mirror copy
}
```

- **Changes without events:** after `Apply`, re-read `Options` for the new state. Items changed by other means (the player drops one) are the window's to notice, as for any inventory slot.
- **Map rolling:** the rips session adds its operations inside `Options` and `Apply` (ids `map:*`), in its own file, through a small registration call the loot session provides (`RiftforgeCrafting.Register(IRiftforgeOperations)`), so neither session edits the other's code.
- **Anything the window needs that isn't here:** add it to this section in `master`; bump `ApiVersion` only for changes that break existing callers.

## 5. Until the window exists

A cheat console command uses the same calls, so the rules can be play-tested first:
- `co_craft list`: the options for the item in hotbar slot 1 and the material in slot 2.
- `co_craft <operation id>`: applies it and prints the result message.

## 6. Open questions (user)

Answered by the user on 2026-11-08 (§2 has the rules):
1. ~~Removing the last affixes~~: the Orb of Annulment never changes the rarity, even at 0 affixes.
2. ~~Orb of Chance odds~~: 50% Magic, 30% Rare, 20% Unique; Rare every time when no unique can come from the item.
3. ~~Mirror on corrupted items~~: allowed; the copy keeps the corruption and can't be changed or mirrored.
4. ~~Jeweller's Orb~~: greyed until sockets and runewords are designed.

Still open:
5. Orb drop sources and rates (ROADMAP L4) aren't needed for the window, but are for orbs to exist in play.
