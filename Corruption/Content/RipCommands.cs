using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Cheat console commands for testing rips alone (Krogon, rips round brief): <c>co_map</c> makes a map of any tier
    ///     and affixes, <c>co_rip</c> shows the nearest rip object's tier and affixes, and <c>co_rip fallback on</c> forces the
    ///     explored-land fallback without exploring a whole biome.
    /// </summary>
    internal static class RipCommands
    {
        public static void Register()
        {
            CommandManager.Instance.AddConsoleCommand(new MapCommand());
            CommandManager.Instance.AddConsoleCommand(new RipCommand());
        }

        private sealed class MapCommand : ConsoleCommand
        {
            public override string Name => "co_map";

            public override string Help =>
                "Corruption: co_map <biome> [tier] [affix ...] - put a map in your inventory. Biomes: " + string.Join(", ", MapList.Biomes) +
                ". Affixes are ids from map_affixes.csv" + AffixIds() + ", each alone as \"id\" (rolls its value) or \"id:value\" " +
                "(e.g. co_map meadows brutal:60 searing:50 tests two affixes at chosen strengths); they are used exactly as given, so one " +
                "affix alone can be tested. With only a tier, random affixes that aren't switched off for it (tier 6 = a full map, corrupted); no tier = a plain map.";

            /// <summary>": glorious, teeming, ..." when the tables have loaded (the help may be read before they do).</summary>
            private static string AffixIds()
            {
                return MapTiers.Loaded
                    ? " (" + string.Join(", ", MapTiers.Tables.Affixes.Affixes.Where(a => a.Approved).Select(a => a.Id).ToArray()) + ")"
                    : "";
            }

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var print = Console.instance;
                var player = Player.m_localPlayer;
                if (!player || !ObjectDB.instance)
                {
                    print.Print("co_map needs a loaded character.");
                    return;
                }
                string biome = args.Length > 0 ? MapList.Biomes.FirstOrDefault(b => string.Equals(b, args[0], StringComparison.OrdinalIgnoreCase)) : null;
                if (biome == null)
                {
                    print.Print("Usage: co_map <biome> [tier] [affix[:value] ...]. Biomes: " + string.Join(", ", MapList.Biomes) +
                                ". Affixes" + AffixIds() + ", e.g. co_map meadows brutal:60 searing:50.");
                    return;
                }
                var prefab = ObjectDB.instance.GetItemPrefab(MapList.ItemPrefix + biome);
                var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (!drop)
                {
                    print.Print($"The {biome} map isn't registered (see the log).");
                    return;
                }

                var state = new MapState();
                var clamped = new List<string>();
                int index = 1;
                int tier = 0;
                if (args.Length > 1 && int.TryParse(args[1], out tier))
                {
                    index = 2;
                }
                // Each argument is an affix id, or "id:value" to set the value (an id alone rolls one; a random roll skips groups already on the map).
                var entries = MapData.ParseEntries(string.Join(MapData.Separator.ToString(), args.Skip(index).Select(a => a.ToLowerInvariant()).ToArray()));
                var affixIds = entries.Select(e => e.Id).ToList();
                if (!MapTiers.Loaded)
                {
                    if (tier > 1 || affixIds.Count > 0)
                    {
                        print.Print("The map tier tables didn't load (see the log), so maps have no tiers. Made a plain map.");
                    }
                }
                else
                {
                    var tables = MapTiers.Tables;
                    var random = new System.Random();
                    if (affixIds.Count > 0)
                    {
                        var unknown = affixIds.Where(a => tables.Affixes.Find(a) == null).ToList();
                        if (unknown.Count > 0)
                        {
                            print.Print($"Unknown affix id(s): {string.Join(", ", unknown.ToArray())}. Known: {string.Join(", ", tables.Affixes.Affixes.Where(a => a.Approved).Select(a => a.Id).ToArray())}.");
                            return;
                        }
                        // A 'later' affix is designed but not built: it does nothing and the server drops it from a rip.
                        var notBuilt = affixIds.Where(a => !tables.Affixes.Find(a).Approved).ToList();
                        if (notBuilt.Count > 0)
                        {
                            print.Print($"Not built yet, so a map can't carry: {string.Join(", ", notBuilt.ToArray())}. Built: {string.Join(", ", tables.Affixes.Affixes.Where(a => a.Approved).Select(a => a.Id).ToArray())}.");
                            return;
                        }
                        // Explicit ids may break the group rule (dev tool).
                        foreach (var entry in entries)
                        {
                            if (state.Affixes.Contains(entry.Id))
                            {
                                continue;
                            }
                            var affix = tables.Affixes.Find(entry.Id);
                            state.Affixes.Add(entry.Id);
                            state.Values[entry.Id] = entry.HasValue ? affix.Clamp(entry.Value) : affix.Roll(random.NextDouble);
                            if (entry.HasValue && state.Values[entry.Id] != entry.Value)
                            {
                                clamped.Add($"{entry.Id} {MapData.FormatValue(entry.Value)} -> {MapData.FormatValue(state.Values[entry.Id])} (its range is {MapData.FormatValue(affix.Min)}-{MapData.FormatValue(affix.Max)})");
                            }
                        }
                        var row = tier > 0 ? tables.Tiers.Row(tier) : null;
                        state.Corrupted = row != null && row.Corrupted;
                    }
                    else if (tier > 1)
                    {
                        var row = tables.Tiers.Row(tier);
                        MapOrbRules.AddRandom(state, tables, random.NextDouble, row.Affixes, new List<string>());
                        state.Corrupted = row.Corrupted;
                    }
                }

                var item = drop.m_itemData.Clone();
                item.m_dropPrefab = prefab;
                item.m_stack = 1;
                MapTiers.Write(item, state);
                if (!player.GetInventory().AddItem(item))
                {
                    print.Print("Your inventory is full.");
                    return;
                }
                print.Print($"Put a {biome} map in your inventory: {MapTiers.Describe(state.Tier, state.Affixes)}{(state.Corrupted ? ", corrupted" : "")}. " +
                            "The Maps tab takes it from the bag within half a second.");
                if (MapTiers.Loaded)
                {
                    // The final values (after any clamping), like co_rip's "Values:".
                    print.Print("Values: " + MapTiers.DescribeValues(state.Rolls(MapTiers.Tables.Affixes)));
                    if (clamped.Count > 0)
                    {
                        print.Print("Clamped into range: " + string.Join("; ", clamped.ToArray()) + ".");
                    }
                }
            }
        }

        private sealed class RipCommand : ConsoleCommand
        {
            public override string Name => "co_rip";

            public override string Help =>
                "Corruption: co_rip - the nearest rip portal or heart within 200 m: biome, tier, affixes, time left, heart, owner. " +
                "co_rip fallback on|off - while on, maps skip the unexplored search so the explored-land fallback can be tested (off by default, not saved).";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var print = Console.instance;
                if (args.Length > 0 && args[0].Equals("fallback", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > 1)
                    {
                        Rips.SkipUnexplored = args[1].Equals("on", StringComparison.OrdinalIgnoreCase);
                    }
                    print.Print($"Explored-land fallback test: {(Rips.SkipUnexplored ? "ON, maps skip the unexplored search" : "off")}.");
                    return;
                }
                var player = Player.m_localPlayer;
                if (!player)
                {
                    print.Print("co_rip needs a loaded character.");
                    return;
                }
                ZNetView best = null;
                string kind = "";
                float bestDistance = 200f;
                void Consider(Component c, string name)
                {
                    var view = c.GetComponent<ZNetView>();
                    if (!view || !view.IsValid())
                    {
                        return;
                    }
                    float d = Vector3.Distance(c.transform.position, player.transform.position);
                    if (d <= bestDistance)
                    {
                        bestDistance = d;
                        best = view;
                        kind = name;
                    }
                }
                foreach (var portal in UnityEngine.Object.FindObjectsOfType<RipPortal>())
                {
                    Consider(portal, "portal");
                }
                foreach (var heart in UnityEngine.Object.FindObjectsOfType<RipHeart>())
                {
                    Consider(heart, "heart");
                }
                if (best == null)
                {
                    print.Print("No rip portal or heart within 200 m.");
                    return;
                }
                var zdo = best.GetZDO();
                if (kind == "portal" && zdo.GetBool(Rips.KeyExit))
                {
                    kind = "exit portal";
                }
                else if (kind == "portal")
                {
                    kind = "entry portal";
                }
                var heartAt = kind == "heart" ? best.transform.position : zdo.GetVec3(Rips.KeyHeart, Vector3.zero);
                long closeMs = zdo.GetLong(Rips.KeyClose, 0L);
                string left = closeMs > 0 && ZNet.instance ? RipRules.TimeLeft(closeMs / 1000.0 - ZNet.instance.GetTimeSeconds()) : "unknown";
                print.Print($"Rip {kind}, {bestDistance:0} m away: {zdo.GetString(Rips.KeyBiome, "?")}, " +
                            $"{MapTiers.Describe(MapTiers.TierOf(zdo), MapTiers.AffixIdsOf(zdo))}. " +
                            $"Values: {MapTiers.DescribeValues(MapTiers.AffixesOf(zdo))}. " +
                            $"Closes in {left}. Heart: {(heartAt == Vector3.zero ? "not known from here" : heartAt.ToString("0"))}. " +
                            $"This game owns it: {(best.IsOwner() ? "yes" : "no")}.");
            }
        }
    }
}
