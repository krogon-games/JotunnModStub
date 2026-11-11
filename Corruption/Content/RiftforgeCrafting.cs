using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Rules;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>One operation the Riftforge window can list (riftforge-ui-brief.md §4).</summary>
    internal sealed class RiftforgeOption
    {
        public string Id;            // "orb:regal", later "corrupt", "shatter", "splinter", "map:roll", ...
        public string Name;          // "Regal Orb", ...
        public bool Allowed;
        public string Refusal = "";  // "" when allowed, else the text in the brief's §2
        public List<string> Preview = new List<string>();
    }

    /// <summary>What <see cref="RiftforgeCrafting.Apply" /> did (riftforge-ui-brief.md §4).</summary>
    internal sealed class RiftforgeResult
    {
        public bool Done;                 // false: refused, nothing changed
        public string Message = "";       // the result message, or the refusal
        public ItemDrop.ItemData Item;    // the item afterwards, or null when it was destroyed
        public List<ItemDrop.ItemData> Created = new List<ItemDrop.ItemData>(); // the mirror copy, recovered splinters

        /// <summary>
        ///     What kind of thing happened, for the window's headline: "" for orbs, else "shattered", "splinter", or a
        ///     shard's "bricked" / "unchanged" / "affix". "unchanged" still changed the item: it is now locked.
        /// </summary>
        public string Outcome = "";
    }

    /// <summary>Operations another part of the mod adds to the Riftforge (the rips session's map rolling, ids <c>map:*</c>).</summary>
    internal interface IRiftforgeOperations
    {
        bool IsMaterial(ItemDrop.ItemData item);

        IEnumerable<RiftforgeOption> Options(ItemDrop.ItemData item, ItemDrop.ItemData material);

        /// <summary>Null when the id isn't one of these operations.</summary>
        RiftforgeResult Apply(ItemDrop.ItemData item, ItemDrop.ItemData material, string operationId);
    }

    /// <summary>
    ///     The Riftforge's crafting rules, the contract the UI session's window calls (riftforge-ui-brief.md §4, API
    ///     version 1): the currency orbs (<see cref="OrbRules" />, 2026-11-08), and since 2026-11-10 (issue #29)
    ///     corrupting with a shard (<see cref="CorruptRules" />), shattering (<see cref="ShatterRules" />) and Affix
    ///     Splinters (<see cref="SplinterRules" />). Everything works on the local player and items in their
    ///     inventory; <c>co_craft</c> exercises it without the window (brief §5).
    /// </summary>
    internal static class RiftforgeCrafting
    {
        public const int ApiVersion = 1;

        /// <summary>Item custom data: corrupted by a shard (locked for every orb but the Mirror).</summary>
        public const string CorruptedKey = "corruption.corrupted";

        /// <summary>Item custom data: a Mirror of Kalandra's copy (can't be changed or mirrored).</summary>
        public const string MirroredKey = "corruption.mirrored";

        public const string NotAtRiftforge = "Use this at a Riftforge";
        public const string Unreadable = "This item's affixes can't be read";
        public const string NoRoomForCopy = "No room in your inventory for the copy";

        public const string CorruptedColor = "#E04040";
        public const string MirroredColor = "#C8D8FF";

        private static readonly System.Func<double> Random01 = () => UnityEngine.Random.value * 0.99999f;
        private static readonly List<IRiftforgeOperations> Registered = new List<IRiftforgeOperations>();
        private static OrbChanceTable _chance;
        private static CorruptionOutcomes _outcomes;
        private static ShatterRules _shatter;

        public const string TablesMissing = "The loot tables didn't load; see the log";
        public const string SplinterItemMissing = "The Affix Splinter item isn't available, so nothing was shattered (see the log)";

        /// <summary>Adds another session's operations (map rolling) to the window, so neither edits the other's code.</summary>
        public static void Register(IRiftforgeOperations operations)
        {
            if (operations != null && !Registered.Contains(operations))
            {
                Registered.Add(operations);
            }
        }

        /// <summary>Reads <c>orb_chance.csv</c>. Problems are logged and switch the Orb of Chance off.</summary>
        public static void Load()
        {
            var errors = new List<string>();
            var table = OrbChanceTable.Parse(DataFiles.Read("orb_chance.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("orb_chance.csv has problems, so the Orb of Chance is off:\n" + string.Join("\n", errors));
                return;
            }
            _chance = table;
        }

        /// <summary>Reads <c>corruption_outcomes.csv</c> and <c>shattering.csv</c>. Problems are logged and switch that operation off.</summary>
        public static void LoadForgeTables()
        {
            var errors = new List<string>();
            var outcomes = CorruptionOutcomes.Parse(DataFiles.Read("corruption_outcomes.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("corruption_outcomes.csv has problems, so shards can't corrupt gear:\n" + string.Join("\n", errors));
            }
            else
            {
                _outcomes = outcomes;
            }
            errors = new List<string>();
            var shatter = ShatterRules.Parse(DataFiles.Read("shattering.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("shattering.csv has problems, so shattering is off:\n" + string.Join("\n", errors));
            }
            else
            {
                _shatter = shatter;
            }
        }

        /// <summary>The chance each affix line of a shattered item becomes a splinter, as the dialog shows it ("50%"), from shattering.csv.</summary>
        public static string ShatterChanceText()
        {
            return _shatter == null ? "" : _shatter.SplinterChance.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>A shard's three outcomes with their chances from corruption_outcomes.csv, for the confirmation (empty when the table didn't load).</summary>
        public static List<CorruptRules.OutcomeLine> CorruptOutcomeLines()
        {
            return _outcomes == null ? new List<CorruptRules.OutcomeLine>() : CorruptRules.OutcomeLines(_outcomes);
        }

        public static bool IsShard(ItemDrop.ItemData item) => PrefabName(item) == ShardRules.ShardPrefab;

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new CraftCommand());
        }

        public static bool IsCorrupted(ItemDrop.ItemData item) => Flag(item, CorruptedKey);

        public static bool IsMirrored(ItemDrop.ItemData item) => Flag(item, MirroredKey);

        private static bool Flag(ItemDrop.ItemData item, string key)
        {
            return item?.m_customData != null && item.m_customData.TryGetValue(key, out var value) && value == "1";
        }

        private static string PrefabName(ItemDrop.ItemData item)
        {
            return item != null && item.m_dropPrefab ? item.m_dropPrefab.name : null;
        }

        /// <summary>Whether an inventory item can go into the material slot (the orbs; later shards, splinters, map materials).</summary>
        public static bool IsMaterial(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return false;
            }
            if (OrbRules.OrbOf(PrefabName(item)) != null || IsShard(item) || SplinterItem.IsSplinter(item))
            {
                return true;
            }
            bool other = false;
            foreach (var operations in Registered)
            {
                Safe.Run("ask another Riftforge operation about a material", () => other |= operations.IsMaterial(item));
            }
            return other;
        }

        /// <summary>Every operation this item and material could mean, allowed or not, in the order to list them.</summary>
        public static List<RiftforgeOption> Options(ItemDrop.ItemData item, ItemDrop.ItemData material)
        {
            var options = new List<RiftforgeOption>();
            if (item == null)
            {
                return options;
            }
            string orb = OrbRules.OrbOf(PrefabName(material));
            if (orb != null)
            {
                options.Add(OrbOption(orb, item));
            }
            else if (material == null)
            {
                options.Add(ShatterOption(item));
            }
            else if (IsShard(material))
            {
                options.Add(CorruptOption(item));
            }
            else if (SplinterItem.IsSplinter(material))
            {
                options.Add(SplinterOption(item, material));
            }
            foreach (var operations in Registered)
            {
                Safe.Run("list another Riftforge operation", () =>
                {
                    var more = operations.Options(item, material);
                    if (more != null)
                    {
                        options.AddRange(more);
                    }
                });
            }
            return options;
        }

        private static RiftforgeOption OrbOption(string orb, ItemDrop.ItemData item)
        {
            var option = new RiftforgeOption { Id = OrbRules.OperationId(orb), Name = OrbRules.Name(orb) };
            var craft = ToCraft(item, out string unreadable);
            var context = Context();
            option.Refusal = !AtRiftforge() ? NotAtRiftforge
                : unreadable.Length > 0 ? unreadable
                : context == null ? "The loot tables didn't load; see the log"
                : OrbRules.Refusal(orb, craft, context);
            option.Allowed = option.Refusal.Length == 0;
            if (context != null && unreadable.Length == 0)
            {
                option.Preview = OrbRules.Preview(orb, craft, context);
            }
            return option;
        }

        /// <summary>The checks every operation shares, before its own rules: at a Riftforge, readable affixes, tables loaded.</summary>
        private static string CommonRefusal(string unreadable, bool tablesLoaded)
        {
            return !AtRiftforge() ? NotAtRiftforge : unreadable.Length > 0 ? unreadable : !tablesLoaded ? TablesMissing : "";
        }

        /// <summary>
        ///     Shatter (no material): the rules' refusal, then room for every splinter that could come out, checked
        ///     before anything is rolled (Krogon, 2026-11-10). The preview lists each line's chance from the data.
        /// </summary>
        private static RiftforgeOption ShatterOption(ItemDrop.ItemData item)
        {
            var option = new RiftforgeOption { Id = ShatterRules.OperationId, Name = "Shatter" };
            var craft = ToCraft(item, out string unreadable);
            bool loaded = Loot.Loaded && _shatter != null;
            option.Refusal = CommonRefusal(unreadable, loaded);
            if (option.Refusal.Length == 0)
            {
                option.Refusal = ShatterRules.Refusal(craft);
            }
            if (option.Refusal.Length == 0 && !RoomForSplinters(item, craft))
            {
                option.Refusal = ShatterRules.MakeRoom(ShatterRules.MaxSplinters(craft));
            }
            option.Allowed = option.Refusal.Length == 0;
            if (loaded && unreadable.Length == 0)
            {
                option.Preview = _shatter.Preview(craft, Loot.Affixes);
            }
            return option;
        }

        /// <summary>
        ///     Room for the most splinters the item can give: free cells in the Splinters tab and the Bag, plus the
        ///     item's own Bag cell when it sits in the Bag grid (it is freed before the splinters arrive).
        /// </summary>
        private static bool RoomForSplinters(ItemDrop.ItemData item, CraftItem craft)
        {
            var player = Player.m_localPlayer;
            var bag = player ? player.GetInventory() : null;
            if (bag == null)
            {
                return false;
            }
            var tab = InventoryTabs.Get(InventoryTab.Splinters);
            bool tabsOn = InventoryTabs.TabFor(SplinterTemplate()) != null;
            int inTab = tab != null && tabsOn ? tab.GetEmptySlots() : 0;
            return ShatterRules.HasRoom(ShatterRules.MaxSplinters(craft), inTab, bag.GetEmptySlots(), InBagGrid(bag, item));
        }

        /// <summary>Whether the item occupies a cell of the Bag grid itself (not an equipment or quick slot another mod adds).</summary>
        private static bool InBagGrid(Inventory bag, ItemDrop.ItemData item)
        {
            return bag.ContainsItem(item) && item.m_gridPos.x >= 0 && item.m_gridPos.x < bag.GetWidth() && item.m_gridPos.y >= 0 &&
                   item.m_gridPos.y < bag.GetHeight() && bag.GetItemAt(item.m_gridPos.x, item.m_gridPos.y) == item;
        }

        private static ItemDrop.ItemData SplinterTemplate()
        {
            var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(ShatterRules.SplinterPrefab) : null;
            var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
            if (!drop)
            {
                return null;
            }
            // The tab recognises an item by its prefab; the registered item's own data has none set.
            drop.m_itemData.m_dropPrefab = prefab;
            return drop.m_itemData;
        }

        /// <summary>Corrupt (a Corruption shard): the rules' refusal; the preview is the risk, with the chances from the data.</summary>
        private static RiftforgeOption CorruptOption(ItemDrop.ItemData item)
        {
            var option = new RiftforgeOption { Id = CorruptRules.OperationId, Name = "Corrupt" };
            var craft = ToCraft(item, out string unreadable);
            bool loaded = Loot.Loaded && _outcomes != null;
            option.Refusal = CommonRefusal(unreadable, loaded);
            if (option.Refusal.Length == 0)
            {
                option.Refusal = CorruptRules.Refusal(craft, Loot.Affixes);
            }
            option.Allowed = option.Refusal.Length == 0;
            if (loaded)
            {
                option.Preview = CorruptRules.Preview(_outcomes);
            }
            return option;
        }

        /// <summary>Apply splinter (an Affix Splinter): the rules' refusal; the preview is the exact line the item ends with.</summary>
        private static RiftforgeOption SplinterOption(ItemDrop.ItemData item, ItemDrop.ItemData splinter)
        {
            var option = new RiftforgeOption { Id = SplinterRules.OperationId, Name = "Affix Splinter" };
            var craft = ToCraft(item, out string unreadable);
            var line = SplinterItem.LineOf(splinter);
            option.Refusal = CommonRefusal(unreadable, Loot.Loaded);
            if (option.Refusal.Length == 0)
            {
                option.Refusal = SplinterRules.Refusal(craft, line, Loot.Affixes);
            }
            option.Allowed = option.Refusal.Length == 0;
            if (option.Allowed)
            {
                var outcome = SplinterRules.Outcome(craft, line, Loot.Affixes, out bool twin);
                option.Preview.Add((twin ? "Completes a twin: " : "Adds ") + LootItem.LineText(outcome, Loot.Affixes));
                if (craft.Rarity == "normal")
                {
                    option.Preview.Add("Becomes Magic");
                }
            }
            return option;
        }

        /// <summary>Does the operation. Checks everything again first; never throws (failures come back as a refusal).</summary>
        public static RiftforgeResult Apply(ItemDrop.ItemData item, ItemDrop.ItemData material, string operationId)
        {
            var result = new RiftforgeResult { Item = item };
            try
            {
                foreach (var operations in Registered)
                {
                    var theirs = operations.Apply(item, material, operationId);
                    if (theirs != null)
                    {
                        return theirs;
                    }
                }
                var player = Player.m_localPlayer;
                var inventory = player ? player.GetInventory() : null;
                if (operationId == ShatterRules.OperationId)
                {
                    if (inventory == null || item == null || !inventory.ContainsItem(item))
                    {
                        result.Message = "The item must be in your bag";
                        return result;
                    }
                    return ApplyShatter(item, player, inventory, result);
                }
                var materialSource = player ? SourceOf(player, material) : null;
                if (operationId == CorruptRules.OperationId || operationId == SplinterRules.OperationId)
                {
                    bool corrupt = operationId == CorruptRules.OperationId;
                    if (inventory == null || item == null || !inventory.ContainsItem(item) || materialSource == null ||
                        ReferenceEquals(item, material) || (corrupt ? !IsShard(material) : !SplinterItem.IsSplinter(material)))
                    {
                        result.Message = corrupt
                            ? "The item must be in your bag and the shard in your bag or Currencies tab"
                            : "The item must be in your bag and the splinter in your bag or Splinters tab";
                        return result;
                    }
                    return corrupt
                        ? ApplyCorrupt(item, material, player, inventory, materialSource, result)
                        : ApplySplinter(item, material, inventory, materialSource, result);
                }
                string orb = OrbRules.OrbOf(PrefabName(material));
                if (orb == null || operationId != OrbRules.OperationId(orb))
                {
                    result.Message = "Put the matching orb in the material slot";
                    return result;
                }
                if (inventory == null || item == null || !inventory.ContainsItem(item) || materialSource == null ||
                    ReferenceEquals(item, material))
                {
                    result.Message = "The item must be in your bag and the orb in your bag or Currencies tab";
                    return result;
                }
                var option = OrbOption(orb, item);
                if (!option.Allowed)
                {
                    result.Message = option.Refusal;
                    return result;
                }
                return ApplyOrb(orb, item, material, inventory, materialSource, result);
            }
            catch (System.Exception ex)
            {
                Safe.Report($"apply {operationId} at the Riftforge", ex);
                result.Done = false;
                result.Message = "Something went wrong; nothing was used up (see the log)";
                return result;
            }
        }

        /// <summary>
        ///     Where a material is: the bag, or the inventory tab it's in. Orbs live in Currencies (the tab pulls currency
        ///     out of the bag twice a second), so the window and <c>co_craft</c> take them from there; one that waits in
        ///     the bag because its tab was full is taken from the bag.
        /// </summary>
        private static Inventory SourceOf(Player player, ItemDrop.ItemData material)
        {
            return InventoryTabs.SourceOf(player, material);
        }

        /// <summary>
        ///     Whether an item can go in the Riftforge's target slot (riftforge-ui-brief.md §1): gear that can carry
        ///     affixes (a loot base, or an item that already has affixes). Materials and everything else can't. Map
        ///     targets come with the rips session's operations (#31).
        /// </summary>
        public static bool IsTarget(ItemDrop.ItemData item)
        {
            if (item == null || IsMaterial(item) || item.m_shared == null || item.m_shared.m_maxStackSize > 1)
            {
                return false;
            }
            string prefab = PrefabName(item);
            if (item.m_customData != null && item.m_customData.ContainsKey(LootItem.AffixesKey))
            {
                return true;
            }
            return prefab != null && Loot.Bases != null && Loot.Bases.Bases.Any(b => b.Prefab == prefab);
        }

        /// <summary>Whether the local player is close enough to a Riftforge to craft (the rules refuse otherwise).</summary>
        public static bool IsAtRiftforge() => AtRiftforge();

        /// <summary>The first orb of this kind in the bag or the Currency tab, or null.</summary>
        public static ItemDrop.ItemData FindOrb(Player player, string orb)
        {
            return InventoryTabs.Find(player, i => OrbRules.OrbOf(PrefabName(i)) == orb, out _);
        }

        private static RiftforgeResult ApplyOrb(string orb, ItemDrop.ItemData item, ItemDrop.ItemData material, Inventory inventory,
            Inventory materialSource, RiftforgeResult result)
        {
            var craft = ToCraft(item, out _);
            string before = DisplayName(item);
            var outcome = OrbRules.Apply(orb, craft, Context(), Random01);
            if (!outcome.Done)
            {
                result.Message = outcome.Refusal;
                return result;
            }

            if (outcome.Copy)
            {
                if (!inventory.HaveEmptySlot())
                {
                    result.Message = NoRoomForCopy;
                    return result;
                }
                var copy = item.Clone();
                copy.m_equipped = false;
                copy.m_stack = 1;
                copy.m_customData[MirroredKey] = "1";
                if (!inventory.AddItem(copy))
                {
                    result.Message = NoRoomForCopy;
                    return result;
                }
                result.Created.Add(copy);
                materialSource.RemoveItem(material, 1);
                result.Done = true;
                result.Message = $"Mirrored {before}: the copy is in your inventory and can never be changed";
                return result;
            }

            outcome.Loot.Rarity = outcome.Rarity;
            outcome.Loot.Write(item.m_customData);
            // A lost durability affix lowers the most it can hold; a new one doesn't repair it.
            item.m_durability = Mathf.Min(item.m_durability, item.GetMaxDurability());
            materialSource.RemoveItem(material, 1);
            inventory.Changed();
            result.Done = true;
            result.Message = Message(orb, before, item, outcome);
            return result;
        }

        /// <summary>
        ///     Shatters the item as one operation (Krogon, 2026-11-10): everything is checked again and the room is
        ///     counted before the roll; then the item goes and every recovered splinter is delivered (Splinters tab first,
        ///     Bag as overflow, the tabs' own routing). If a splinter can't be delivered after all, everything is put
        ///     back: the item returns, no splinter stays, nothing is dropped on the ground.
        /// </summary>
        private static RiftforgeResult ApplyShatter(ItemDrop.ItemData item, Player player, Inventory bag, RiftforgeResult result)
        {
            var option = ShatterOption(item);
            if (!option.Allowed)
            {
                result.Message = option.Refusal;
                return result;
            }
            var craft = ToCraft(item, out _);
            string before = DisplayName(item);
            // Rolled only now, after the checks: storage can't be used to turn down a roll.
            var lines = _shatter.Shatter(craft, Random01);
            var splinters = lines.Select(SplinterItem.Create).ToList();
            if (splinters.Any(s => s == null))
            {
                result.Message = SplinterItemMissing;
                return result;
            }

            if (item.m_equipped)
            {
                player.UnequipItem(item, false);
            }
            bag.RemoveItem(item);
            var delivered = new List<ItemDrop.ItemData>();
            bool failed = bag.ContainsItem(item);
            foreach (var splinter in splinters)
            {
                if (failed || !bag.AddItem(splinter))
                {
                    failed = true;
                    break;
                }
                delivered.Add(splinter);
            }
            if (failed)
            {
                foreach (var splinter in delivered)
                {
                    SourceOf(player, splinter)?.RemoveItem(splinter);
                }
                if (!bag.ContainsItem(item))
                {
                    bag.m_inventory.Add(item);
                }
                bag.Changed();
                Jotunn.Logger.LogWarning("Shatter: a recovered splinter couldn't be delivered, so the item was put back and nothing was shattered.");
                result.Message = "The splinters couldn't be stored, so nothing was shattered";
                return result;
            }

            result.Done = true;
            result.Item = null;
            result.Outcome = "shattered";
            result.Created.AddRange(splinters);
            result.Message = splinters.Count == 0
                ? $"Shattered {before}: no splinters recovered"
                : $"Shattered {before}: {splinters.Count} splinter{(splinters.Count == 1 ? "" : "s")} ({string.Join("; ", lines.Select(SplinterItem.LineText))})";
            return result;
        }

        /// <summary>
        ///     Uses one Corruption shard on the item: destroyed, locked with nothing added, or one corrupted affix and
        ///     locked. A completed attempt uses exactly one shard, also when the item is destroyed; a refusal uses none.
        /// </summary>
        private static RiftforgeResult ApplyCorrupt(ItemDrop.ItemData item, ItemDrop.ItemData shard, Player player, Inventory bag,
            Inventory shardSource, RiftforgeResult result)
        {
            var option = CorruptOption(item);
            if (!option.Allowed)
            {
                result.Message = option.Refusal;
                return result;
            }
            string before = DisplayName(item);
            var outcome = CorruptRules.Apply(ToCraft(item, out _), Loot.Affixes, _outcomes, Random01);
            if (!outcome.Done)
            {
                result.Message = outcome.Refusal;
                return result;
            }
            switch (outcome.Outcome)
            {
                case "bricked":
                    if (item.m_equipped)
                    {
                        player.UnequipItem(item, false);
                    }
                    bag.RemoveItem(item);
                    result.Item = null;
                    result.Message = $"Destroyed: the shard consumed {before}.";
                    break;
                case "unchanged":
                    item.m_customData[CorruptedKey] = "1";
                    result.Message = $"{before}: {CorruptRules.UnchangedMessage}";
                    break;
                default:
                    outcome.Loot.Write(item.m_customData);
                    item.m_customData[CorruptedKey] = "1";
                    result.Message = $"{DisplayName(item)}: corrupted and locked; gained " +
                                     (outcome.CompletedTwin ? "a resonant twin, " : "") + LootItem.LineText(outcome.Line, Loot.Affixes) + ".";
                    break;
            }
            shardSource.RemoveItem(shard, 1);
            bag.Changed();
            result.Done = true;
            result.Outcome = outcome.Outcome;
            return result;
        }

        /// <summary>Puts a splinter's line on the item with its exact value (a completed twin: the sum plus the resonance bonus) and uses the splinter up.</summary>
        private static RiftforgeResult ApplySplinter(ItemDrop.ItemData item, ItemDrop.ItemData splinter, Inventory bag, Inventory splinterSource,
            RiftforgeResult result)
        {
            var option = SplinterOption(item, splinter);
            if (!option.Allowed)
            {
                result.Message = option.Refusal;
                return result;
            }
            var outcome = SplinterRules.Apply(ToCraft(item, out _), SplinterItem.LineOf(splinter), Loot.Affixes);
            if (!outcome.Done)
            {
                result.Message = outcome.Refusal;
                return result;
            }
            outcome.Loot.Write(item.m_customData);
            splinterSource.RemoveItem(splinter);
            bag.Changed();
            result.Done = true;
            result.Outcome = "splinter";
            result.Message = $"{DisplayName(item)} " + (outcome.CompletedTwin ? "completed a twin: " : "gained ") +
                             LootItem.LineText(outcome.Line, Loot.Affixes);
            return result;
        }

        /// <summary>The first Corruption shard in the bag or the Currencies tab, or null.</summary>
        public static ItemDrop.ItemData FindShard(Player player)
        {
            return InventoryTabs.Find(player, IsShard, out _);
        }

        /// <summary>The result message (brief §3): "Grim Fang gained +12% fire damage".</summary>
        private static string Message(string orb, string before, ItemDrop.ItemData item, OrbResult outcome)
        {
            string after = DisplayName(item);
            string Line(LootItem.Line l) => LootItem.LineText(l, Loot.Affixes);
            var text = new StringBuilder();
            switch (orb)
            {
                case "transmutation":
                case "alchemy":
                case "chance":
                    text.Append(before).Append(" became ").Append(after).Append(": ").Append(string.Join("; ", outcome.Gained.Select(Line)));
                    break;
                case "regal":
                    text.Append(before).Append(" became ").Append(after).Append(" and gained ").Append(string.Join("; ", outcome.Gained.Select(Line)));
                    break;
                case "divine":
                    text.Append(after).Append(" rerolled: ").Append(string.Join("; ", outcome.Gained.Select(Line)));
                    break;
                case "annulment":
                    text.Append(after).Append(" lost ").Append(string.Join("; ", outcome.Lost.Select(Line)));
                    if (outcome.Gained.Count > 0)
                    {
                        text.Append("; kept ").Append(string.Join("; ", outcome.Gained.Select(Line)));
                    }
                    break;
                default:
                    text.Append(after).Append(" gained ").Append(string.Join("; ", outcome.Gained.Select(Line)));
                    break;
            }
            return text.ToString();
        }

        private static string DisplayName(ItemDrop.ItemData item)
        {
            return Loot.ColoredName(item) ?? Localization.instance.Localize(item.m_shared.m_name);
        }

        private static bool AtRiftforge()
        {
            var player = Player.m_localPlayer;
            return player && CraftingStation.HaveBuildStationInRange(Texts.RiftforgeName, player.transform.position);
        }

        private static OrbContext Context()
        {
            if (!Loot.Loaded)
            {
                return null;
            }
            // Uniques don't exist yet (ROADMAP L6), so none can come from an Orb of Chance (always Rare, Krogon 2026-11-08).
            return new OrbContext { Affixes = Loot.Affixes, Rules = Loot.Rules, Chance = _chance, UniqueAvailable = false };
        }

        /// <summary>The item as the rules see it. Unreadable saved affixes are refused rather than overwritten.</summary>
        private static CraftItem ToCraft(ItemDrop.ItemData item, out string unreadable)
        {
            unreadable = "";
            var loot = Loot.Of(item);
            if (loot == null && item.m_customData != null && item.m_customData.ContainsKey(LootItem.AffixesKey))
            {
                unreadable = Unreadable;
            }
            var shared = item.m_shared;
            string prefab = PrefabName(item);
            var row = prefab != null && Loot.Bases != null ? Loot.Bases.Bases.FirstOrDefault(b => b.Prefab == prefab) : null;
            return new CraftItem
            {
                Rarity = loot?.Rarity ?? "normal",
                Loot = loot,
                Corrupted = IsCorrupted(item),
                Mirrored = IsMirrored(item),
                Classes = row?.Classes,
                Mechanics = LootBases.Mechanics(shared.m_skillType.ToString(), shared.m_attack != null ? shared.m_attack.m_attackChainLevels : 0,
                    shared.m_blockPower)
            };
        }

        /// <summary>
        ///     co_craft: the Riftforge's rules without the window (brief §5). The item goes in hotbar slot 1; the orb is
        ///     taken from the bag or the Currency tab (it can't stay in the hotbar: the tab pulls currency back).
        /// </summary>
        private sealed class CraftCommand : ConsoleCommand
        {
            public override string Name => "co_craft";

            public override string Help =>
                "Corruption: co_craft [list | <orb> | corrupt | shatter | splinter | give [count] | testcorrupt] - Riftforge crafting on the item in " +
                "hotbar slot 1, with an orb or shard from your Currencies tab. list: what each orb you carry would do. <orb>: e.g. regal or orb:regal. " +
                "corrupt: uses a Corruption shard. shatter: shatters it (no confirmation). splinter: applies the first splinter you carry. " +
                "give: every orb and shards (default 5 each). testcorrupt: marks or unmarks the slot-1 item as corrupted (testing only)";

            public override bool IsCheat => true;

            public override List<string> CommandOptionList()
            {
                return new[] { "list", "corrupt", "shatter", "splinter", "give", "testcorrupt" }.Concat(OrbRules.Orbs).ToList();
            }

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player)
                {
                    Console.instance.Print("co_craft needs a loaded world.");
                    return;
                }
                var inventory = player.GetInventory();
                string command = args.Length > 0 ? args[0].ToLowerInvariant() : "list";
                var item = inventory.GetItemAt(0, 0);

                if (command == "give")
                {
                    int count = args.Length > 1 && int.TryParse(args[1], out var n) && n > 0 ? n : 5;
                    foreach (var orb in OrbRules.Orbs)
                    {
                        var prefab = ObjectDB.instance.GetItemPrefab(OrbRules.Prefab(orb));
                        if (prefab)
                        {
                            inventory.AddItem(prefab, count);
                        }
                    }
                    var shardPrefab = ObjectDB.instance.GetItemPrefab(ShardRules.ShardPrefab);
                    if (shardPrefab)
                    {
                        inventory.AddItem(shardPrefab, count);
                    }
                    Console.instance.Print($"Added {count} of each orb and {count} Corruption shards to your Currencies tab.");
                    return;
                }
                if (command == "testcorrupt")
                {
                    if (item == null)
                    {
                        Console.instance.Print("Put an item in hotbar slot 1.");
                        return;
                    }
                    if (IsCorrupted(item))
                    {
                        item.m_customData.Remove(CorruptedKey);
                    }
                    else
                    {
                        item.m_customData[CorruptedKey] = "1";
                    }
                    inventory.Changed();
                    Console.instance.Print($"{DisplayName(item)} is {(IsCorrupted(item) ? "now corrupted (test mark)" : "no longer corrupted")}.");
                    return;
                }
                if (item == null)
                {
                    Console.instance.Print("Put the item in hotbar slot 1.");
                    return;
                }
                if (command == "list")
                {
                    // One line per orb you carry (bag or Currency tab), as if it were in the material slot.
                    var options = new List<RiftforgeOption>();
                    foreach (var orb in OrbRules.Orbs)
                    {
                        var found = FindOrb(player, orb);
                        if (found != null)
                        {
                            options.AddRange(Options(item, found));
                        }
                    }
                    options.AddRange(Options(item, null));
                    var shard = FindShard(player);
                    if (shard != null)
                    {
                        options.AddRange(Options(item, shard));
                    }
                    var carried = InventoryTabs.Find(player, SplinterItem.IsSplinter, out _);
                    if (carried != null)
                    {
                        options.AddRange(Options(item, carried));
                    }
                    Console.instance.Print($"Item: {DisplayName(item)}" + (AtRiftforge() ? "" : " (you're not at a Riftforge)"));
                    if (options.Count <= 1)
                    {
                        Console.instance.Print("You carry no orbs or shards: co_craft give.");
                    }
                    foreach (var option in options)
                    {
                        Console.instance.Print($"{option.Id} ({option.Name}): {(option.Allowed ? "allowed" : "refused: " + option.Refusal)}");
                        foreach (var line in option.Preview)
                        {
                            Console.instance.Print("    " + line);
                        }
                    }
                    return;
                }

                if (command == "shatter" || command == "corrupt" || command == "splinter")
                {
                    var with = command == "corrupt" ? FindShard(player)
                        : command == "splinter" ? InventoryTabs.Find(player, SplinterItem.IsSplinter, out _) : null;
                    if (command != "shatter" && with == null)
                    {
                        Console.instance.Print(command == "corrupt" ? "You have no Corruption shard (co_craft give)." : "You carry no Affix Splinter.");
                        return;
                    }
                    var done = Apply(item, with, command);
                    Console.instance.Print(done.Done ? done.Message : "Refused: " + done.Message);
                    if (done.Done)
                    {
                        player.Message(MessageHud.MessageType.Center, done.Message);
                    }
                    return;
                }
                string id = command.Contains(":") ? command : OrbRules.OperationId(command);
                string wanted = id.StartsWith("orb:") ? id.Substring(4) : null;
                if (wanted == null || !OrbRules.Orbs.Contains(wanted))
                {
                    Console.instance.Print($"'{command}' isn't an orb. Orbs: {string.Join(", ", OrbRules.Orbs)}.");
                    return;
                }
                var material = FindOrb(player, wanted);
                if (material == null)
                {
                    Console.instance.Print($"You have no {OrbRules.Name(wanted)} (co_craft give).");
                    return;
                }
                var result = Apply(item, material, id);
                Console.instance.Print(result.Done ? result.Message : "Refused: " + result.Message);
                if (result.Done)
                {
                    player.Message(MessageHud.MessageType.Center, result.Message);
                }
            }
        }
    }
}
