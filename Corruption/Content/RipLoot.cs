using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     What a destroyed rip heart spills on the ground (no chest is placed any more; Krogon, 2026-11-26). Each player
    ///     near the rip gets their own rolls, at the rip multiplier: one roll of the orb table and one Magic/Rare gear
    ///     roll (2026-11-19 and 2026-11-23), locked to them for the first seconds. The shared part is dealt among those
    ///     players (<see cref="PersonalLootRules.Deal" />): the vanilla loot of one of the rip biome's own chests (from
    ///     <c>Corruption/Data/rip_loot.csv</c>, weighted by how often each appears in that biome), corruption shards
    ///     (user: 1-3, 2026-10-28) and one guaranteed map of the rip's biome (2026-11-23: each rip pays back its map).
    ///     Every dealt share is locked to the player it went to; an odd number may leave someone with less, which is fine.
    ///     Shards and maps carry their loot beam.
    /// </summary>
    internal static class RipLoot
    {
        private static List<RipLootTable.Chest> s_chests = new List<RipLootTable.Chest>();

        public static void Load()
        {
            var errors = new List<string>();
            s_chests = RipLootTable.Parse(DataFiles.Read("rip_loot.csv"), errors);
            Jotunn.Logger.LogInfo($"Rip loot: {s_chests.Count} chest entries loaded.");
            foreach (var error in errors)
            {
                Jotunn.Logger.LogWarning(error);
            }
        }

        /// <summary>One thing in the shared part: how many units there are to deal, and what a unit is.</summary>
        private sealed class SharedKind
        {
            public ItemDrop.ItemData Item;
            public int Units;
            public string BeamRow;
            public string Label;
        }

        /// <summary>What one player got, for the log.</summary>
        private sealed class Seat
        {
            public long Owner;
            public string Name = "nobody";
            public readonly List<string> Orbs = new List<string>();
            public readonly List<string> Gear = new List<string>();
            public readonly List<string> Shared = new List<string>();
        }

        /// <summary>The heart was destroyed (on its owner's game): spill everything around it.</summary>
        public static void Spill(string biome, Vector3 heartPosition)
        {
            Spill(null, biome, heartPosition);
        }

        /// <summary>
        ///     The same, for a heart with a tier (rips round, decisions 4, 4b, 5, 6e): the tier's drop rates, its gear floor
        ///     and its shards. A null heart, or tables that didn't load, spills as tier 1 as before.
        /// </summary>
        public static void Spill(ZDO heart, string biome, Vector3 heartPosition)
        {
            var row = heart != null ? MapTiers.RowOf(heart) : null;
            int tier = row != null ? MapTiers.TierOf(heart) : 1;
            // "Players near the rip": the game's own nearby-players range, as for kills.
            Jotunn.Logger.LogInfo($"Rip loot: tier {tier}{(row == null ? " (no tier data)" : "")}.");
            var nearby = new List<Player>();
            Player.GetPlayersInRange(heartPosition, Game.instance ? Game.instance.m_difficultyScaleRange : 100f, nearby);
            Jotunn.Logger.LogInfo($"Rip loot: {nearby.Count} player(s) near the heart.");
            var origin = heartPosition + Vector3.up;
            int total = 0;
            var seats = new List<Seat>();
            for (int s = 0; s < System.Math.Max(1, nearby.Count); s++)
            {
                var seat = new Seat();
                if (s < nearby.Count && nearby[s])
                {
                    seat.Owner = nearby[s].GetPlayerID();
                    seat.Name = nearby[s].GetPlayerName();
                }
                seats.Add(seat);
            }

            // Each player's own rolls.
            Safe.Run("spill each player's orbs and gear", () =>
            {
                if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
                {
                    return;
                }
                var sources = (row != null ? RipTierRules.DropSources(row) : new List<string> { "rip_tier_1" }).ToArray();
                int rolls = DeathLoot.RollsFor(nearby.Count);
                for (int i = 0; i < rolls; i++)
                {
                    var seat = seats[System.Math.Min(i, seats.Count - 1)];
                    foreach (var orb in OrbDrops.Roll(sources))
                    {
                        var prefab = OrbDrops.Prefabs(new[] { orb }).FirstOrDefault();
                        if (prefab)
                        {
                            OrbDrops.DropPersonal(prefab, origin, seat.Owner);
                            seat.Orbs.Add(orb);
                            total++;
                        }
                    }
                    // The same Magic/Rare roll as a kill, per player (Krogon, 2026-11-23); uniques come with ROADMAP L6.
                    // The best of the Magic/Rare rolls (Rare wins), lifted to the tier's floor; none when both are nothing.
                    string rolled = null;
                    foreach (var r in Loot.Drops.Roll(sources, () => Random.value * 0.99999f))
                    {
                        if (r == "rare" || (r == "magic" && rolled == null))
                        {
                            rolled = r;
                        }
                    }
                    var result = row != null ? RipTierRules.HeartGear(rolled, row.HeartFloor) : rolled;
                    foreach (var rarity in new[] { result })
                    {
                        if (rarity != "magic" && rarity != "rare")
                        {
                            continue;
                        }
                        var item = Loot.Create(rarity, biome);
                        if (item != null)
                        {
                            PersonalLoot.Tag(Loot.Drop(item, origin), seat.Owner, PersonalLoot.BeamRowFor(item));
                            seat.Gear.Add(rarity);
                            total++;
                        }
                    }
                }
            });

            // The shared part, dealt among the players near the heart.
            Safe.Run("deal the rip heart's shared loot", () =>
            {
                var kinds = SharedKinds(biome, row);
                if (kinds.Count == 0)
                {
                    return;
                }
                var dealt = PersonalLootRules.Deal(nearby.Count, kinds.Select(k => k.Units).ToList(), n => Random.Range(0, n));
                for (int k = 0; k < kinds.Count; k++)
                {
                    for (int s = 0; s < seats.Count; s++)
                    {
                        int units = dealt[k][s];
                        if (units <= 0)
                        {
                            continue;
                        }
                        total += DropUnits(kinds[k], units, origin, seats[s].Owner);
                        seats[s].Shared.Add($"{units}x {kinds[k].Label}");
                    }
                }
            });

            foreach (var seat in seats)
            {
                Jotunn.Logger.LogInfo($"Rip loot (tier {tier}): player {seat.Name} ({seat.Owner}): {seat.Orbs.Count} orb(s) ({string.Join(", ", seat.Orbs.ToArray())}), " +
                                      $"{seat.Gear.Count} gear ({string.Join(", ", seat.Gear.ToArray())}), shared: {(seat.Shared.Count > 0 ? string.Join(", ", seat.Shared.ToArray()) : "nothing")}.");
            }
            Jotunn.Logger.LogInfo($"Rip loot (tier {tier}): spilled {total} drop(s) at {origin}.");
        }

        /// <summary>The shared part as kinds to deal: the chosen chest's vanilla loot, the shards, the one map.</summary>
        private static List<SharedKind> SharedKinds(string biome, MapTierTable.TierRow row)
        {
            var kinds = new List<SharedKind>();
            Safe.Run("roll the rip chest's vanilla loot", () =>
            {
                string chest = RipLootTable.Pick(s_chests, biome, () => Random.value * 0.99999f);
                var prefab = chest != null && ZNetScene.instance ? ZNetScene.instance.GetPrefab(chest) : null;
                var container = prefab ? prefab.GetComponent<Container>() : null;
                if (!container)
                {
                    Jotunn.Logger.LogWarning($"Rip loot: no chest loot for biome '{biome}' ({(chest ?? "none in rip_loot.csv")}" +
                                             (prefab && !container ? " has no Container" : "") + ").");
                    return;
                }
                // The game reuses the list it returns, so it's copied at once.
                var items = new List<ItemDrop.ItemData>(container.m_defaultItems.GetDropListItems());
                foreach (var item in items)
                {
                    if (item != null && item.m_dropPrefab && item.m_stack > 0)
                    {
                        kinds.Add(new SharedKind
                        {
                            Item = item, Units = item.m_stack, BeamRow = PersonalLoot.BeamRowFor(item),
                            Label = Localization.instance.Localize(item.m_shared.m_name)
                        });
                    }
                }
                Jotunn.Logger.LogInfo($"Rip loot: the {chest} gave {items.Count} kind(s) of vanilla loot.");
            });
            Safe.Run("roll the rip heart's shards", () =>
            {
                var shard = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab) : null;
                int min = CorruptionConfig.RipChestShardsMin.Value;
                int max = CorruptionConfig.RipChestShardsMax.Value;
                if (row != null)
                {
                    RipTierRules.ShardRange(row, out min, out max);   // the tier's range; the config only when the tables didn't load
                }
                int shards = Random.Range(min, Mathf.Max(min, max) + 1);
                if (shard && shards > 0)
                {
                    kinds.Add(new SharedKind { Item = FromPrefab(shard), Units = shards, BeamRow = LootBeamTable.Shard, Label = "corruption shard" });
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Rip loot: no shards (shard item {(shard ? "found" : "missing")}, {shards} rolled).");
                }
            });
            Safe.Run("find the rip heart's map", () =>
            {
                var map = MapDrops.Prefab(MapDrops.MapFor(biome));
                if (map)
                {
                    kinds.Add(new SharedKind { Item = FromPrefab(map), Units = 1, BeamRow = LootBeamTable.Map, Label = map.name });
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Rip loot: no map item for biome '{biome}'.");
                }
            });
            return kinds;
        }

        private static ItemDrop.ItemData FromPrefab(GameObject prefab)
        {
            var item = prefab.GetComponent<ItemDrop>().m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_worldLevel = (byte)Game.m_worldLevel;
            return item;
        }

        /// <summary>
        ///     Drops <paramref name="units" /> of a kind for one owner: a stackable kind as stacks up to its stack size,
        ///     anything else as separate drops. Returns how many drops were made.
        /// </summary>
        private static int DropUnits(SharedKind kind, int units, Vector3 origin, long owner)
        {
            int stackSize = System.Math.Max(1, kind.Item.m_shared.m_maxStackSize);
            int made = 0;
            while (units > 0)
            {
                int stack = System.Math.Min(units, stackSize);
                PersonalLoot.Tag(Loot.Drop(kind.Item, origin, stack), owner, kind.BeamRow);
                units -= stack;
                made++;
            }
            return made;
        }
    }
}
