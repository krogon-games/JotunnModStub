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
            CommandManager.Instance.AddConsoleCommand(new RipLootCommand());
            CommandManager.Instance.AddConsoleCommand(new StampCommand());
        }

        /// <summary>
        ///     co_stamp: gives the creature you look at (or the nearest within 10 m) a rip's tier and any map affixes, as if a
        ///     rip had spawned it, so each creature affix can be tried alone on a still target such as the game's training dummy
        ///     (Krogon, 2026-11-28, la-18: "its really difficult to tell if i take damage from hitting another creature with
        ///     thorns on"). Your game takes the creature over first. <c>co_stamp clear</c> takes the stamp off.
        /// </summary>
        private sealed class StampCommand : ConsoleCommand
        {
            public override string Name => "co_stamp";

            public override string Help =>
                "Corruption: co_stamp [tier] <affix[:value] ...> - give the creature you look at (or the nearest within 10 m) a rip's tier " +
                "(default 1) and map affixes, e.g. spawn TrainingDummy, then co_stamp thorns:25. co_stamp clear removes it. Testing only.";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var print = Console.instance;
                var player = Player.m_localPlayer;
                if (!player || !MapTiers.Loaded)
                {
                    print.Print("co_stamp needs a loaded character and the map tables (see the log).");
                    return;
                }
                var target = player.GetHoverCreature();
                if (!target || target is Player)
                {
                    target = null;
                    float best = 10f;
                    foreach (var c in Character.GetAllCharacters())
                    {
                        if (!c || c is Player || c.IsDead())
                        {
                            continue;
                        }
                        float d = Vector3.Distance(c.transform.position, player.transform.position);
                        if (d <= best)
                        {
                            best = d;
                            target = c;
                        }
                    }
                }
                if (!target || !target.m_nview || !target.m_nview.IsValid())
                {
                    print.Print("No creature under your crosshair or within 10 m. Try: spawn TrainingDummy");
                    return;
                }
                var zdo = target.m_nview.GetZDO();
                target.m_nview.ClaimOwnership();
                if (args.Length > 0 && args[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    zdo.Set(MapTiers.KeyTier, 0);
                    zdo.Set(MapTiers.KeyAffixes, "");
                    zdo.RemoveFloat(MapTiers.KeyExperience);
                    target.SetupMaxHealth();
                    print.Print($"{target.m_name}: rip stamp removed.");
                    return;
                }
                int index = 0;
                int tier = 1;
                if (args.Length > 0 && int.TryParse(args[0], out var t))
                {
                    tier = Mathf.Clamp(t, 1, 6);
                    index = 1;
                }
                var tables = MapTiers.Tables;
                var entries = MapData.ParseEntries(string.Join(MapData.Separator.ToString(), args.Skip(index).Select(a => a.ToLowerInvariant()).ToArray()));
                var bad = entries.Where(e => tables.Affixes.Find(e.Id) == null || !tables.Affixes.Find(e.Id).Approved).Select(e => e.Id).ToList();
                if (bad.Count > 0)
                {
                    print.Print($"Unknown or not built: {string.Join(", ", bad.ToArray())}. Built: {string.Join(", ", tables.Affixes.Affixes.Where(a => a.Approved).Select(a => a.Id).ToArray())}.");
                    return;
                }
                var state = new MapState();
                var random = new System.Random();
                foreach (var e in entries)
                {
                    if (state.Affixes.Contains(e.Id))
                    {
                        continue;
                    }
                    var affix = tables.Affixes.Find(e.Id);
                    state.Affixes.Add(e.Id);
                    state.Values[e.Id] = e.HasValue ? affix.Clamp(e.Value) : affix.Roll(random.NextDouble);
                }
                MapTiers.Stamp(zdo, tier, MapData.JoinRolls(state));
                MapAffixEffects.OnStamped(target);
                print.Print($"{target.m_name}: stamped tier {tier}" +
                            (state.Affixes.Count > 0 ? " with " + MapTiers.DescribeValues(state.Rolls(tables.Affixes)) : ", no affixes") +
                            ". Your game runs it now; it stays stamped until co_stamp clear." +
                            (state.Affixes.Contains("thorns") ? " Each Thorns return prints here while devcommands is on." : ""));
            }
        }

        /// <summary>
        ///     co_riploot: rolls many kills of a rip with a chosen tier and affixes in the biome you stand in and prints what
        ///     they would drop, to check the loot affixes (#96) without killing thousands of creatures. Drops nothing.
        /// </summary>
        private sealed class RipLootCommand : ConsoleCommand
        {
            public override string Name => "co_riploot";

            public override string Help =>
                "Corruption: co_riploot [rolls] [tier] [affix ...] - roll a rip creature's kill loot many times (default 10000, tier 1) " +
                "in the biome you stand in and print the gear, orbs, map chance and kinds of gear (e.g. co_riploot 10000 3 bountiful). Drops nothing.";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var print = Console.instance;
                var player = Player.m_localPlayer;
                if (!player || !Loot.Loaded || !MapTiers.Loaded)
                {
                    print.Print("co_riploot needs a loaded character and the loot and map tables (see the log).");
                    return;
                }
                int index = 0;
                int rolls = 10000;
                int tier = 1;
                if (args.Length > index && int.TryParse(args[index], out var n))
                {
                    rolls = Mathf.Clamp(n, 1, 1000000);
                    index++;
                }
                if (args.Length > index && int.TryParse(args[index], out var t))
                {
                    tier = t;
                    index++;
                }
                var tables = MapTiers.Tables;
                var row = tables.Tiers.Row(tier);
                if (row == null)
                {
                    print.Print($"No tier {tier} in map_tiers.csv.");
                    return;
                }
                var entries = MapData.ParseEntries(string.Join(MapData.Separator.ToString(), args.Skip(index).Select(a => a.ToLowerInvariant()).ToArray()));
                var unknown = entries.Where(e => tables.Affixes.Find(e.Id) == null).Select(e => e.Id).ToList();
                if (unknown.Count > 0)
                {
                    print.Print($"Unknown affix id(s): {string.Join(", ", unknown.ToArray())}.");
                    return;
                }
                var affixRolls = entries.Select(e =>
                {
                    var affix = tables.Affixes.Find(e.Id);
                    return new MapAffixRoll { Affix = affix, Value = e.HasValue ? affix.Clamp(e.Value) : affix.Min };
                }).ToList();
                var boost = MapLootRules.Of(affixRolls);
                var sources = RipTierRules.DropSources(row);
                string biome = Loot.BiomeAt(player.transform.position);
                System.Func<double> random = () => UnityEngine.Random.value * 0.99999f;
                System.Func<LootBases.Base, double> weight = null;
                if (boost.TiltsPick)
                {
                    weight = b => boost.WeightOf(Loot.KindOf(b));
                }
                float orbMultiplier = (float)(Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources) * boost.Orbs);
                int magic = 0, rare = 0, orbs = 0;
                var kinds = new Dictionary<string, int>();
                for (int i = 0; i < rolls; i++)
                {
                    orbs += Loot.Orbs.Roll(orbMultiplier, random).Count;
                    foreach (var rarity in Loot.Drops.Roll(sources, random, boost.Items))
                    {
                        if (rarity != "magic" && rarity != "rare")
                        {
                            continue;
                        }
                        if (rarity == "magic") magic++; else rare++;
                        var pick = Loot.Bases.Pick(biome, Loot.Rules.PreviousBiomePercent, p => ObjectDB.instance.GetItemPrefab(p), random, weight);
                        string kind = pick == null ? "none" : Loot.KindOf(pick);
                        if (kind.Length == 0) kind = "jewellery";
                        kinds[kind] = kinds.TryGetValue(kind, out var c) ? c + 1 : 1;
                    }
                }
                float mapChance = MapDropRules.Chance(Loot.Rules.MapDropOneIn, (float)(Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources) * boost.Items));
                print.Print($"{rolls} kills in a tier {tier} {biome} rip" + (affixRolls.Count > 0 ? $" with {MapTiers.DescribeValues(affixRolls)}" : "") +
                            $": gear x{boost.Items:0.##}, orbs x{boost.Orbs:0.##} on top of the tier.");
                print.Print($"  Magic {magic} (expected {Loot.Drops.Chance("magic", sources, boost.Items) * rolls:0.#}), " +
                            $"Rare {rare} (expected {Loot.Drops.Chance("rare", sources, boost.Items) * rolls:0.#}), orbs {orbs}.");
                print.Print($"  Map chance per kill: {(mapChance > 0f ? $"1 in {1f / mapChance:0}" : "none")} (if maps can drop here).");
                int gear = magic + rare;
                if (gear > 0)
                {
                    print.Print("  Kinds of gear: " + string.Join(", ", kinds.OrderByDescending(k => k.Value)
                        .Select(k => $"{k.Key} {k.Value} ({100.0 * k.Value / gear:0}%)").ToArray()) + ".");
                }
            }
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
                    ? " (" + string.Join(", ", MapTiers.Tables.Affixes.Affixes.Where(a => a.Approved).Select(a => a.Id).ToArray()) +
                      "; corrupted-only, for a tier 6 map: " + string.Join(", ", MapTiers.Tables.Affixes.Affixes.Where(a => a.Approved && a.CorruptedOnly).Select(a => a.Id).ToArray()) + ")"
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
                        if (state.Corrupted)
                        {
                            MapCorruptRules.AddCorruptedAffix(state, tables, random.NextDouble);   // the 5th (#97)
                        }
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
