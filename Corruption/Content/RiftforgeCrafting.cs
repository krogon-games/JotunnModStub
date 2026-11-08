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
        public List<ItemDrop.ItemData> Created = new List<ItemDrop.ItemData>(); // the mirror copy (later splinters)
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
    ///     version 1). This round (2026-11-08): the currency orbs (<see cref="OrbRules" />). Corrupting with a shard,
    ///     shattering and Affix Splinters come later (ROADMAP L5). Everything works on the local player and items in
    ///     their inventory; <c>co_craft</c> exercises it until the window exists (brief §5).
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
            if (OrbRules.OrbOf(PrefabName(item)) != null)
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
                string orb = OrbRules.OrbOf(PrefabName(material));
                if (orb == null || operationId != OrbRules.OperationId(orb))
                {
                    result.Message = "Put the matching orb in the material slot";
                    return result;
                }
                var player = Player.m_localPlayer;
                var inventory = player ? player.GetInventory() : null;
                if (inventory == null || item == null || !inventory.ContainsItem(item) || !inventory.ContainsItem(material) ||
                    ReferenceEquals(item, material))
                {
                    result.Message = "The item and the orb must both be in your inventory";
                    return result;
                }
                var option = OrbOption(orb, item);
                if (!option.Allowed)
                {
                    result.Message = option.Refusal;
                    return result;
                }
                return ApplyOrb(orb, item, material, inventory, result);
            }
            catch (System.Exception ex)
            {
                Safe.Report($"apply {operationId} at the Riftforge", ex);
                result.Done = false;
                result.Message = "Something went wrong; nothing was used up (see the log)";
                return result;
            }
        }

        private static RiftforgeResult ApplyOrb(string orb, ItemDrop.ItemData item, ItemDrop.ItemData material, Inventory inventory,
            RiftforgeResult result)
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
                inventory.RemoveItem(material, 1);
                result.Done = true;
                result.Message = $"Mirrored {before}: the copy is in your inventory and can never be changed";
                return result;
            }

            outcome.Loot.Rarity = outcome.Rarity;
            outcome.Loot.Write(item.m_customData);
            // A lost durability affix lowers the most it can hold; a new one doesn't repair it.
            item.m_durability = Mathf.Min(item.m_durability, item.GetMaxDurability());
            inventory.RemoveItem(material, 1);
            inventory.Changed();
            result.Done = true;
            result.Message = Message(orb, before, item, outcome);
            return result;
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
        ///     co_craft: the Riftforge's rules without the window (brief §5). The item goes in hotbar slot 1, the orb in
        ///     slot 2.
        /// </summary>
        private sealed class CraftCommand : ConsoleCommand
        {
            public override string Name => "co_craft";

            public override string Help =>
                "Corruption: co_craft [list | <operation> | give [count] | testcorrupt] - Riftforge crafting with the item in hotbar slot 1 " +
                "and the orb in slot 2. <operation>: e.g. orb:regal or just regal. give: every orb (default 5 each). " +
                "testcorrupt: marks or unmarks the slot-1 item as corrupted (testing only; shards can't corrupt yet)";

            public override bool IsCheat => true;

            public override List<string> CommandOptionList()
            {
                return new[] { "list", "give", "testcorrupt" }.Concat(OrbRules.Orbs).ToList();
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
                var material = inventory.GetItemAt(1, 0);

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
                    Console.instance.Print($"Added {count} of each orb to your Currency tab. Drag one to hotbar slot 2.");
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
                    Console.instance.Print("Put the item in hotbar slot 1 and the orb in slot 2.");
                    return;
                }
                if (command == "list")
                {
                    var options = Options(item, material);
                    Console.instance.Print($"Item: {DisplayName(item)}. Material: {(material != null ? DisplayName(material) : "none")}" +
                                           (AtRiftforge() ? "" : " (you're not at a Riftforge)"));
                    if (options.Count == 0)
                    {
                        Console.instance.Print("No operations: put an orb in slot 2.");
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

                string id = command.Contains(":") ? command : OrbRules.OperationId(command);
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
