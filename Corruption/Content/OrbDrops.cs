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
    ///     a vanilla dungeon or location chest rolls it once when the game fills it (<c>dungeon_chest</c>); the chest a
    ///     rip heart leaves rolls it once per player near the rip (<see cref="RipLoot" />); Stonecutter's and Resinous
    ///     roll it on a harvest (<see cref="LootHarvest" />). Chances: <c>orbs.csv</c> times <c>drop_modifiers.csv</c>'s
    ///     orb column (<see cref="OrbDropTable" />).
    /// </summary>
    internal static class OrbDrops
    {
        private static readonly System.Func<double> Random01 = () => Random.value * 0.99999f;

        /// <summary>
        ///     While true, a chest the game fills doesn't roll as a dungeon chest: the rip chest rolls its own orbs,
        ///     per player, after it's placed.
        /// </summary>
        [System.ThreadStatic] public static bool PlacingRipChest;

        /// <summary>One roll of the orb table with these sources (null for none). Empty when the loot tables didn't load.</summary>
        public static List<string> Roll(IEnumerable<string> sources)
        {
            if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
            {
                return new List<string>();
            }
            return Loot.Orbs.Roll(Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources), Random01);
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

        /// <summary>Drops the orbs at a position, scattered like a creature's drops.</summary>
        public static void Drop(IEnumerable<string> orbs, Vector3 position)
        {
            foreach (var prefab in Prefabs(orbs))
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
            }
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
        ///     game (<c>Container.Awake</c>): one roll of the orb table at the <c>dungeon_chest</c> multiplier. Chests
        ///     players build have no default loot and are left alone.
        /// </summary>
        [HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems))]
        private static class ChestOrbs
        {
            private static void Postfix(Container __instance)
            {
                if (PlacingRipChest || !__instance || __instance.m_defaultItems == null || __instance.m_defaultItems.m_drops == null ||
                    __instance.m_defaultItems.m_drops.Count == 0)
                {
                    return;
                }
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
