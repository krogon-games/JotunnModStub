using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Maps at the Riftforge (Krogon's rips round, 2026-11-26, decisions 6b, 6c, 11 and 13): orbs and the Corruption
    ///     shard work on a map as the rules in <see cref="MapOrbRules" /> and <see cref="MapCorruptRules" /> say, through
    ///     the operations the Riftforge window already lists (ids <c>map:orb:&lt;orb&gt;</c> and <c>map:corrupt</c>). Also
    ///     the map's words for tooltips, the bench and the name (<see cref="MapText" />), and the tier number in the
    ///     corner of its icon. For a map target <see cref="RiftforgeCrafting" /> asks only this class, so a gear refusal
    ///     is never shown for a map.
    /// </summary>
    internal sealed class MapForge : IRiftforgeOperations
    {
        public const string OrbPrefix = "map:orb:";
        public const string CorruptId = "map:corrupt";

        private static readonly Func<double> Random01 = () => UnityEngine.Random.value * 0.99999f;

        // ------------------------------------------------------------ what a map looks like

        /// <summary>A map item as the words need it.</summary>
        private sealed class View
        {
            public MapState State;
            public MapTierTable.TierRow Row;
            public string BaseName;
        }

        public static bool IsMap(ItemDrop.ItemData item) => MapItems.Find(item) != null;

        /// <summary>The map's tier, affixes and locks, read without logging (this runs every frame for every map in a grid).</summary>
        public static MapState StateOf(ItemDrop.ItemData item, out bool unreadable)
        {
            unreadable = false;
            if (!MapTiers.Loaded)
            {
                return new MapState();
            }
            var unknown = new List<string>();
            var state = MapData.Read(item?.m_customData, MapTiers.Tables.Tiers, MapTiers.Tables.Affixes, unknown);
            unreadable = unknown.Count > 0;
            return state;
        }

        private static View ViewOf(ItemDrop.ItemData item)
        {
            if (!IsMap(item) || !MapTiers.Loaded)
            {
                return null;
            }
            var state = StateOf(item, out _);
            return new View
            {
                State = state,
                Row = MapTiers.Tables.Tiers.Row(state.Tier),
                BaseName = Localization.instance.Localize(item.m_shared.m_name)
            };
        }

        /// <summary>The map's name in its colour ("Corrupted Meadows map" in red); null for anything else, and for a plain white map.</summary>
        public static string ColoredName(ItemDrop.ItemData item)
        {
            var view = ViewOf(item);
            if (view == null)
            {
                return null;
            }
            string color = MapText.ColorOf(view.Row);
            string name = MapText.Name(view.BaseName, view.Row);
            return color == MapText.NormalColor ? null : "<color=" + color + ">" + name + "</color>";
        }

        /// <summary>The tier's drop multiplier for the "Corruption loot is x times as likely" line; null while the drop table isn't loaded.</summary>
        private static double? DropMultiplierOrNull(MapTierTable.TierRow row)
        {
            return Loot.Drops != null ? (double?)DropMultiplier(row) : null;
        }

        private static string LabelOf(ItemDrop.ItemData item)
        {
            var map = MapItems.Find(item);
            return map != null ? map.Label : "";
        }

        /// <summary>
        ///     The inventory tooltip of a map (decision 15): tier line, summary, marks, then the game's own text (weight, other
        ///     mods' lines) without the map's description. <paramref name="gameText" /> is the game's tooltip so far; null when the
        ///     map can't be read (the caller keeps the game's text).
        /// </summary>
        public static string Tooltip(ItemDrop.ItemData item, string gameText)
        {
            var view = ViewOf(item);
            if (view == null)
            {
                return null;
            }
            return MapText.Tooltip(view.Row, LabelOf(item), view.State.Rolls(MapTiers.Tables.Affixes), MapTiers.Tables.Affixes, DropMultiplierOrNull(view.Row),
                MapGlyphSet.Current(), LootTooltip.LocksOf(item), WithoutDescription(item, gameText));
        }

        /// <summary>The game's tooltip text minus the map's description (the raw token first, else its translation); the whole text if neither is found.</summary>
        private static string WithoutDescription(ItemDrop.ItemData item, string gameText)
        {
            string text = gameText ?? "";
            string token = item.m_shared.m_description;
            if (!string.IsNullOrEmpty(token))
            {
                int at = text.IndexOf(token, StringComparison.Ordinal);
                if (at >= 0)
                {
                    return (text.Substring(0, at) + text.Substring(at + token.Length)).Trim('\n');
                }
                string localised = Localization.instance.Localize(token);
                if (!string.IsNullOrEmpty(localised) && localised != token)
                {
                    at = text.IndexOf(localised, StringComparison.Ordinal);
                    if (at >= 0)
                    {
                        return (text.Substring(0, at) + text.Substring(at + localised.Length)).Trim('\n');
                    }
                }
            }
            return text.Trim('\n');
        }

        /// <summary>The coloured tier line under the Forge slot; empty for anything but a map.</summary>
        public static string ForgeCaption(ItemDrop.ItemData item)
        {
            var view = ViewOf(item);
            return view == null ? "" : MapText.ForgeCaption(view.Row);
        }

        /// <summary>The Forge's bench text for a map: only the Corrupted/Mirrored marks.</summary>
        public static string ForgeBench(ItemDrop.ItemData item)
        {
            return ViewOf(item) == null ? "" : MapText.ForgeBench(LootTooltip.LocksOf(item));
        }

        /// <summary>The Forge's right column for a map: usage, what you'll face, what you'll earn; <paramref name="highlight" /> lists the affix ids the last use added.</summary>
        public static string ForgeSheet(ItemDrop.ItemData item, ICollection<string> highlight)
        {
            var view = ViewOf(item);
            if (view == null)
            {
                return "";
            }
            return MapText.ForgeSheet(view.Row, LabelOf(item), view.State.Rolls(MapTiers.Tables.Affixes), MapTiers.Tables.Affixes, DropMultiplierOrNull(view.Row),
                MapGlyphSet.Current(), highlight);
        }

        private static double DropMultiplier(MapTierTable.TierRow row)
        {
            return Loot.Drops != null ? Loot.Drops.Multiplier("magic", new[] { row.DropSource }) : 1.0;
        }

        /// <summary>The tier and its colour for the corner of the icon; false for anything but a map (or while the tables are missing).</summary>
        public static bool BadgeOf(ItemDrop.ItemData item, out string tier, out Color color)
        {
            tier = "";
            color = Color.white;
            if (item == null || !MapTiers.Loaded)
            {
                return false;
            }
            // Runs every frame for every cell on screen (Opus review, 2026-11-27): whether the item is a map is worked out
            // once per item, and its tier again only when its affixes or corrupted mark change.
            var badge = Badges.GetValue(item, NewBadge);
            if (!badge.IsMap)
            {
                return false;
            }
            string affixes = null, corrupted = null;
            if (item.m_customData != null)
            {
                item.m_customData.TryGetValue(MapData.AffixesKey, out affixes);
                item.m_customData.TryGetValue(MapData.CorruptedKey, out corrupted);
            }
            if (badge.Tier == null || badge.Affixes != affixes || badge.Corrupted != corrupted)
            {
                var row = MapTiers.Tables.Tiers.Row(StateOf(item, out _).Tier);
                badge.Tier = row.Tier.ToString(System.Globalization.CultureInfo.InvariantCulture);
                badge.Color = ParseColor(MapText.ColorOf(row));
                badge.Affixes = affixes;
                badge.Corrupted = corrupted;
            }
            tier = badge.Tier;
            color = badge.Color;
            return true;
        }

        /// <summary>An item's tier badge, kept with the item (the table forgets it when the item is gone).</summary>
        private sealed class Badge
        {
            public bool IsMap;
            public string Affixes, Corrupted, Tier;
            public Color Color;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ItemDrop.ItemData, Badge> Badges =
            new System.Runtime.CompilerServices.ConditionalWeakTable<ItemDrop.ItemData, Badge>();

        private static Badge NewBadge(ItemDrop.ItemData item) => new Badge { IsMap = IsMap(item) };

        private static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();

        private static Color ParseColor(string hex)
        {
            if (!Colors.TryGetValue(hex, out var color))
            {
                if (!ColorUtility.TryParseHtmlString(hex, out color))
                {
                    color = Color.white;
                }
                Colors[hex] = color;
            }
            return color;
        }

        // ------------------------------------------------------------ the Riftforge operations

        public bool IsMaterial(ItemDrop.ItemData item) => false;

        public IEnumerable<RiftforgeOption> Options(ItemDrop.ItemData item, ItemDrop.ItemData material)
        {
            var options = new List<RiftforgeOption>();
            if (!IsMap(item))
            {
                return options;
            }
            string orb = OrbRules.OrbOf(PrefabName(material));
            if (material == null)
            {
                // Always listed, so the Shatter button says why it is greyed out.
                options.Add(new RiftforgeOption { Id = ShatterRules.OperationId, Name = "Shatter", Refusal = MapText.NoShatter });
            }
            else if (orb != null)
            {
                options.Add(OrbOption(orb, item));
            }
            else if (RiftforgeCrafting.IsShard(material))
            {
                options.Add(CorruptOption(item));
            }
            else if (SplinterItem.IsSplinter(material))
            {
                options.Add(new RiftforgeOption { Id = SplinterRules.OperationId, Name = "Affix Splinter", Refusal = MapText.NoSplinter });
            }
            return options;
        }

        /// <summary>The checks every map operation shares, before its own rules: at a Riftforge, tables loaded, affixes readable.</summary>
        private static string CommonRefusal(ItemDrop.ItemData item)
        {
            if (!RiftforgeCrafting.IsAtRiftforge())
            {
                return RiftforgeCrafting.NotAtRiftforge;
            }
            if (!MapTiers.Loaded)
            {
                return RiftforgeCrafting.TablesMissing;
            }
            StateOf(item, out bool unreadable);
            return unreadable ? RiftforgeCrafting.Unreadable : "";
        }

        private static RiftforgeOption OrbOption(string orb, ItemDrop.ItemData item)
        {
            var option = new RiftforgeOption { Id = OrbPrefix + orb, Name = OrbRules.Name(orb) };
            string refusal = CommonRefusal(item);
            if (refusal.Length == 0)
            {
                refusal = MapOrbRules.Refusal(orb, StateOf(item, out _), MapTiers.Tables) ?? "";
            }
            option.Refusal = refusal;
            option.Allowed = refusal.Length == 0;
            if (option.Allowed)
            {
                option.Preview.AddRange(OrbPreview(orb, StateOf(item, out _)));
            }
            return option;
        }

        private static List<string> OrbPreview(string orb, MapState state)
        {
            var tiers = MapTiers.Tables.Tiers;
            int count = state.Affixes.Count;
            switch (orb)
            {
                case "mirror":
                    return new List<string> { "A copy that can never be changed" };
                case "annulment":
                    return new List<string> { "Removes one random affix: tier " + state.Tier + " → " + tiers.TierFor(count - 1, false) };
                case "alchemy":
                case "chance":
                    return new List<string> { "Adds 3 random affixes: tier " + state.Tier + " → " + tiers.TierFor(count + 3, false) };
                default:
                    return new List<string> { "Adds 1 random affix: tier " + state.Tier + " → " + tiers.TierFor(count + 1, false) };
            }
        }

        private static RiftforgeOption CorruptOption(ItemDrop.ItemData item)
        {
            var option = new RiftforgeOption { Id = CorruptId, Name = "Corrupt" };
            string refusal = CommonRefusal(item);
            if (refusal.Length == 0)
            {
                refusal = MapCorruptRules.Refusal(StateOf(item, out _), MapTiers.Tables) ?? "";
            }
            option.Refusal = refusal;
            option.Allowed = refusal.Length == 0;
            if (option.Allowed)
            {
                var tiers = MapTiers.Tables.Tiers;
                option.Preview.Add("Becomes tier " + tiers.CorruptTier + ": it never breaks, but it is locked");
            }
            return option;
        }

        /// <summary>Null when the item isn't a map (the gear rules go ahead); for a map every operation is answered here.</summary>
        public RiftforgeResult Apply(ItemDrop.ItemData item, ItemDrop.ItemData material, string operationId)
        {
            if (!IsMap(item))
            {
                return null;
            }
            var result = new RiftforgeResult { Item = item };
            var player = Player.m_localPlayer;
            if (operationId == ShatterRules.OperationId)
            {
                result.Message = MapText.NoShatter;
                return result;
            }
            if (operationId == SplinterRules.OperationId)
            {
                result.Message = MapText.NoSplinter;
                return result;
            }
            // co_craft and older callers say "orb:regal" and "corrupt"; for a map they mean the map operations.
            if (operationId == CorruptRules.OperationId)
            {
                operationId = CorruptId;
            }
            else if (operationId != null && operationId.StartsWith("orb:", StringComparison.Ordinal))
            {
                operationId = OrbPrefix + operationId.Substring(4);
            }
            var itemSource = player ? InventoryTabs.SourceOf(player, item) : null;
            var materialSource = player ? InventoryTabs.SourceOf(player, material) : null;
            if (itemSource == null || materialSource == null || ReferenceEquals(item, material))
            {
                result.Message = "The map must be in your bag or Maps tab, and the orb or shard in your bag or Currencies tab";
                return result;
            }
            if (operationId == CorruptId)
            {
                return ApplyCorrupt(item, material, itemSource, materialSource, result);
            }
            string orb = operationId != null && operationId.StartsWith(OrbPrefix, StringComparison.Ordinal) ? operationId.Substring(OrbPrefix.Length) : null;
            if (orb == null || OrbRules.OrbOf(PrefabName(material)) != orb)
            {
                result.Message = "Put the matching orb in the material slot";
                return result;
            }
            return ApplyOrb(orb, item, material, player.GetInventory(), itemSource, materialSource, result);
        }

        private static RiftforgeResult ApplyOrb(string orb, ItemDrop.ItemData item, ItemDrop.ItemData material, Inventory bag,
            Inventory itemSource, Inventory materialSource, RiftforgeResult result)
        {
            var option = OrbOption(orb, item);
            if (!option.Allowed)
            {
                result.Message = option.Refusal;
                return result;
            }
            var state = StateOf(item, out _);
            var outcome = MapOrbRules.Apply(orb, state, MapTiers.Tables, Random01);
            if (!outcome.Done)
            {
                result.Message = outcome.Refusal;
                return result;
            }
            if (outcome.Copy)
            {
                // Into the map's own inventory (the Maps tab), else the bag; without room nothing is used up.
                var destination = itemSource.HaveEmptySlot() ? itemSource : bag.HaveEmptySlot() ? bag : null;
                if (destination == null)
                {
                    result.Message = RiftforgeCrafting.NoRoomForCopy;
                    return result;
                }
                var copy = item.Clone();
                copy.m_equipped = false;
                copy.m_stack = 1;
                copy.m_customData[MapData.MirroredKey] = "1";
                if (!destination.AddItem(copy))
                {
                    result.Message = RiftforgeCrafting.NoRoomForCopy;
                    return result;
                }
                result.Created.Add(copy);
                materialSource.RemoveItem(material, 1);
                result.Done = true;
                result.Outcome = "map";
                result.Message = OrbRules.Name(orb) + ": copied. The copy is in your inventory and can never be changed.";
                return result;
            }

            MapTiers.Write(item, outcome.State);
            materialSource.RemoveItem(material, 1);
            itemSource.Changed();
            result.Done = true;
            result.Outcome = "map";
            // "id:value" entries: the Forge shows each with its rolled number (a Divine Orb's rerolled affixes show as rerolled).
            result.MapAdded = outcome.Added.Select(id => id + MapData.ValueSeparator + MapData.FormatValue(outcome.State.Values[id])).ToList();
            result.MapRerolled = outcome.Rerolled.Select(id => id + MapData.ValueSeparator + MapData.FormatValue(outcome.State.Values[id])).ToList();
            result.MapRemoved = outcome.Removed.Select(id =>
            {
                double old;
                return state.Values.TryGetValue(id, out old) ? id + MapData.ValueSeparator + MapData.FormatValue(old) : id;
            }).ToList();
            result.Message = outcome.Headline;
            return result;
        }

        private static RiftforgeResult ApplyCorrupt(ItemDrop.ItemData item, ItemDrop.ItemData shard, Inventory itemSource,
            Inventory shardSource, RiftforgeResult result)
        {
            if (!RiftforgeCrafting.IsShard(shard))
            {
                result.Message = "Put a Corruption shard in the material slot";
                return result;
            }
            var option = CorruptOption(item);
            if (!option.Allowed)
            {
                result.Message = option.Refusal;
                return result;
            }
            var next = MapCorruptRules.Apply(StateOf(item, out _), MapTiers.Tables, out string headline,
                () => UnityEngine.Random.value * 0.99999f);
            if (next == null)
            {
                result.Message = headline;
                return result;
            }
            MapTiers.Write(item, next);
            shardSource.RemoveItem(shard, 1);
            itemSource.Changed();
            result.Done = true;
            result.Outcome = "map";
            result.Message = headline;
            return result;
        }

        private static string PrefabName(ItemDrop.ItemData item)
        {
            return item != null && item.m_dropPrefab ? item.m_dropPrefab.name : null;
        }

        /// <summary>The affix line of an "id:value" entry ("Enemies deal 63% more damage.", no name: decision 10), or the id when the table doesn't know it.</summary>
        public static string AffixText(string entryText)
        {
            var entry = MapData.ParseEntries(entryText).FirstOrDefault();
            var affix = MapTiers.Loaded && entry != null ? MapTiers.Tables.Affixes.Find(entry.Id) : null;
            return affix != null ? MapText.AffixLine(affix, affix.Clamp(entry.HasValue ? entry.Value : affix.Min)) : entryText;
        }
    }

    /// <summary>
    ///     The tier number in the corner of a map's icon (decision 11 (1)), where the game shows an item's quality: in
    ///     every inventory grid (bag, Maps tab, chests) and in the Forge slot, in the map's colour.
    /// </summary>
    internal static class MapTierBadge
    {
        private static Color? _plain;

        /// <summary>
        ///     Shows the tier on a map's cell and returns true; for any other item restores the quality text's own colour
        ///     (a cell that held a map keeps its colour otherwise) and returns false.
        /// </summary>
        public static bool Set(InventoryElement element, ItemDrop.ItemData item)
        {
            if (element == null || !element.m_quality)
            {
                return false;
            }
            if (_plain == null)
            {
                _plain = element.m_quality.color;
            }
            if (item != null && MapForge.BadgeOf(item, out string tier, out Color color))
            {
                element.m_quality.enabled = true;
                if (element.m_quality.text != tier)
                {
                    element.m_quality.text = tier;
                }
                element.m_quality.color = color;
                return true;
            }
            if (element.m_quality.color != _plain.Value)
            {
                element.m_quality.color = _plain.Value;
            }
            return false;
        }

        /// <summary>
        ///     After the game has drawn a grid (its own, EquipmentAndQuickSlots' and our character sheet's postfixes on the
        ///     same method don't touch the quality text), a map's cell shows its tier in the quality's place. Only the
        ///     quality text of cells holding a map (and a stale colour of a cell that held one) is touched; nothing is
        ///     skipped or replaced, so every other postfix on <c>InventoryGrid.UpdateGui</c> runs as before.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
        private static class UpdateGui_Patch
        {
            private static void Postfix(InventoryGrid __instance)
            {
                try
                {
                    var inventory = __instance.m_inventory;
                    if (inventory == null || !MapTiers.Loaded)
                    {
                        return;
                    }
                    int width = inventory.GetWidth();
                    foreach (var item in inventory.GetAllItems())
                    {
                        var element = __instance.GetElement(item.m_gridPos.x, item.m_gridPos.y, width);
                        if (element != null)
                        {
                            Set(element, item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Safe.Report("show a map's tier on its icon", ex);
                }
            }
        }
    }
}

namespace Corruption.Content
{
    /// <summary>
    ///     Picks the glyph set of the map text once per font: the fancy "—", "×" and "’" only if the game's description font
    ///     has all three (a missing glyph shows as a box and the game logs it), else the plain "-", "x" and "'".
    /// </summary>
    internal static class MapGlyphSet
    {
        private static TMP_FontAsset _font;
        private static MapGlyphs _glyphs;

        public static MapGlyphs Current()
        {
            TMP_FontAsset font = null;
            var gui = InventoryGui.instance;
            if (gui && gui.m_recipeDecription)
            {
                font = gui.m_recipeDecription.font;
            }
            if (!font)
            {
                font = TMP_Settings.defaultFontAsset;
            }
            if (_glyphs != null && (font ? font == _font : _font == null))
            {
                return _glyphs;
            }
            _font = font;
            _glyphs = font && font.HasCharacter('—', true, true) && font.HasCharacter('×', true, true) && font.HasCharacter('’', true, true)
                ? MapGlyphs.Fancy
                : MapGlyphs.Plain;
            return _glyphs;
        }
    }
}
