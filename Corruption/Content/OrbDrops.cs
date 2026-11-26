using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Currency orbs dropping in normal play (#33, Krogon's decisions of 2026-11-19): every orb can drop from all
    ///     content. Kills roll the orb table once per nearby player, beside the Magic and Rare roll (<see cref="LootDrops" />);
    ///     a vanilla dungeon or location chest rolls it once when the game fills it (<c>dungeon_chest</c>); the loot a
    ///     destroyed rip heart spills rolls it once per player near the rip (<see cref="RipLoot" />); Hoardfinder's and of Hidden Riches
    ///     drop one orb picked with the table's odds when they trigger on a harvest (<see cref="LootHarvest" />). Chances: <c>orbs.csv</c> times <c>drop_modifiers.csv</c>'s
    ///     orb column (<see cref="OrbDropTable" />). Orbs from kills, rips and harvests are personal: the drop is tagged with the
    ///     player it was rolled for and gets its loot beam (<see cref="PersonalLoot" />).
    /// </summary>
    internal static class OrbDrops
    {
        private static readonly System.Func<double> Random01 = () => Random.value * 0.99999f;

        /// <summary>One roll of the orb table with these sources (null for none). Empty when the loot tables didn't load.</summary>
        public static List<string> Roll(IEnumerable<string> sources)
        {
            if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
            {
                return new List<string>();
            }
            return Loot.Orbs.Roll(Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources), Random01);
        }

        /// <summary>
        ///     Hoardfinder's and of Hidden Riches: one orb, always, picked with the table's odds (<see cref="OrbDropTable.Pick" />).
        ///     Null when the loot tables didn't load or loot is off.
        /// </summary>
        public static GameObject FindOne()
        {
            if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
            {
                return null;
            }
            string orb = Loot.Orbs.Pick(Random01);
            return orb == null ? null : Prefabs(new[] { orb }).FirstOrDefault();
        }

        /// <summary>The orbs' item prefabs (orbs whose item is missing are left out, logged).</summary>
        public static List<GameObject> Prefabs(IEnumerable<string> orbs)
        {
            var prefabs = new List<GameObject>();
            foreach (var orb in orbs)
            {
                var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(OrbRules.Prefab(orb)) : null;
                if (prefab)
                {
                    prefabs.Add(prefab);
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Orb drops: no item {OrbRules.Prefab(orb)}.");
                }
            }
            return prefabs;
        }

        /// <summary>Drops the orbs at a position, scattered like a creature's drops, each locked to <paramref name="owner" /> (0 = nobody) and with its beam.</summary>
        public static void Drop(IEnumerable<string> orbs, Vector3 position, long owner = 0L)
        {
            foreach (var prefab in Prefabs(orbs))
            {
                DropPersonal(prefab, position, owner);
            }
        }

        /// <summary>Drops an orb or map prefab for one player (0 = nobody): scattered like a creature's drops, tagged with its owner and beam row.</summary>
        public static ItemDrop DropPersonal(GameObject prefab, Vector3 position, long owner)
        {
            return PersonalLoot.Tag(DropPrefab(prefab, position), owner, PersonalLoot.BeamRowForPrefab(prefab.name));
        }

        /// <summary>Drops one item prefab at a position, scattered and pushed like a creature's drops. Returns the drop (not tagged).</summary>
        public static ItemDrop DropPrefab(GameObject prefab, Vector3 position)
        {
            var go = Object.Instantiate(prefab, position + Random.insideUnitSphere * 0.5f, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
            ItemDrop.OnCreateNew(go);
            var body = go.GetComponent<Rigidbody>();
            if (body)
            {
                var push = Random.insideUnitSphere;
                push.y = Mathf.Abs(push.y);
                body.AddForce(push * 5f, ForceMode.VelocityChange);
            }
            return go.GetComponent<ItemDrop>();
        }

        /// <summary>Puts the orbs into a chest. Returns how many went in.</summary>
        public static int AddTo(Container container, IEnumerable<string> orbs)
        {
            var inventory = container ? container.GetInventory() : null;
            if (inventory == null)
            {
                return 0;
            }
            int added = 0;
            foreach (var prefab in Prefabs(orbs))
            {
                if (inventory.AddItem(prefab, 1))
                {
                    added++;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Orb drops: no room in {container.name} for {prefab.name}.");
                }
            }
            return added;
        }

        /// <summary>
        ///     A vanilla chest with its own loot (dungeons, locations, shipwrecks) filling itself, once, on its owner's
        ///     game (<c>Container.Awake</c>): one roll of the orb table at the <c>dungeon_chest</c> multiplier, and a
        ///     1-in-20 chance of a corruption shard (decision 33). Chests
        ///     players build have no default loot and are left alone.
        /// </summary>
        [HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems))]
        private static class ChestOrbs
        {
            private static void Postfix(Container __instance)
            {
                if (!__instance || __instance.m_defaultItems == null || __instance.m_defaultItems.m_drops == null ||
                    __instance.m_defaultItems.m_drops.Count == 0)
                {
                    return;
                }
                Safe.Run("add a shard to a chest", () =>
                {
                    // Dungeon and location chests: a rare shard source (design; Krogon, 2026-11-23: 1 in 20).
                    var shard = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(Texts.ShardPrefab) : null;
                    if (shard && ShardRules.ChestShards(CorruptionConfig.DungeonChestShardChance.Value, Random01) > 0 &&
                        __instance.GetInventory() != null && __instance.GetInventory().AddItem(shard, 1))
                    {
                        Jotunn.Logger.LogInfo($"Chest shards: 1 shard in {Utils.GetPrefabName(__instance.gameObject)}.");
                    }
                });
                Safe.Run("add orbs to a chest", () =>
                {
                    var orbs = Roll(new[] { "dungeon_chest" });
                    if (orbs.Count > 0)
                    {
                        int added = AddTo(__instance, orbs);
                        Jotunn.Logger.LogInfo($"Orb drops: {added} orb(s) in {Utils.GetPrefabName(__instance.gameObject)} ({string.Join(", ", orbs)}).");
                    }
                });
            }
        }

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new RollCommand());
        }

        /// <summary>
        ///     co_orbroll: rolls the orb table many times and prints what came out, to check the rates without killing
        ///     thousands of creatures. Drops nothing.
        /// </summary>
        private sealed class RollCommand : ConsoleCommand
        {
            public override string Name => "co_orbroll";

            public override string Help =>
                "Corruption: co_orbroll [rolls] [source ...] - roll the orb table (default 10000 times, as an ordinary kill) and print the counts. " +
                "Sources from drop_modifiers.csv, e.g. star_2 rip_tier_1 dungeon_chest corrupted_boss";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                if (!Loot.Loaded)
                {
                    Console.instance.Print("Loot tables didn't load; see the log.");
                    return;
                }
                int rolls = 10000;
                int first = 0;
                if (args.Length > 0 && int.TryParse(args[0], out var n))
                {
                    rolls = Mathf.Clamp(n, 1, 1000000);
                    first = 1;
                }
                var sources = args.Skip(first).Select(a => a.ToLowerInvariant()).ToList();
                float multiplier = Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources);
                var counts = OrbRules.Orbs.ToDictionary(o => o, o => 0);
                for (int i = 0; i < rolls; i++)
                {
                    foreach (var orb in Loot.Orbs.Roll(multiplier, Random01))
                    {
                        counts[orb]++;
                    }
                }
                Console.instance.Print($"{rolls} rolls, orb multiplier x{multiplier:0.##}" +
                                       (sources.Count > 0 ? $" ({string.Join(", ", sources)})" : " (an ordinary kill)") + ":");
                foreach (var orb in OrbRules.Orbs)
                {
                    float chance = Loot.Orbs.Chance(orb, multiplier);
                    Console.instance.Print($"  {OrbRules.Name(orb)}: {counts[orb]} (expected {chance * rolls:0.#}" +
                                           (chance > 0f ? $", 1 in {1f / chance:0})" : ", never drops)"));
                }
            }
        }
    }
}
