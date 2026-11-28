using System.Linq;
using Corruption.Rules;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Maps as loot (Krogon, 2026-11-23, decisions 18-19 and 31-32; <see cref="MapDropRules" />): one guaranteed
    ///     map in the loot a destroyed rip heart spills (<see cref="RipLoot" />), and creatures dropping a map of the biome they die in, once
    ///     the world's first rip heart is destroyed and that biome's boss is beaten (<see cref="LootDrops" />).
    /// </summary>
    internal static class MapDrops
    {
        private static readonly System.Func<double> Random01 = () => Random.value * 0.99999f;

        /// <summary>The map of a biome, as the game names biomes ("AshLands"), or null (the Ocean has none).</summary>
        public static MapList.Map MapFor(string biome)
        {
            return string.IsNullOrEmpty(biome)
                ? null
                : MapItems.ByItem.Values.FirstOrDefault(m => string.Equals(m.Biome, biome, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The map's item prefab, or null when it isn't registered.</summary>
        public static GameObject Prefab(MapList.Map map)
        {
            return map != null && ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(map.Item) : null;
        }

        /// <summary>Whether any rip heart has been destroyed in this world.</summary>
        public static bool RipCleared => ZoneSystem.instance && ZoneSystem.instance.GetGlobalKey(MapDropRules.RipClearedKey);

        /// <summary>Records that a rip heart was destroyed (a world key, sent to the server and shared with everyone).</summary>
        public static void MarkRipCleared()
        {
            if (ZoneSystem.instance && !RipCleared)
            {
                ZoneSystem.instance.SetGlobalKey(MapDropRules.RipClearedKey);
                Jotunn.Logger.LogInfo("Map drops: the world's first rip heart is destroyed; creatures can drop maps now.");
            }
        }

        /// <summary>
        ///     Whether the boss whose essence makes this map is beaten in the world (its own defeat key, read from the
        ///     boss's prefab: essences.csv names the boss).
        /// </summary>
        public static bool BossDefeated(MapList.Map map)
        {
            if (map == null || !EssenceItems.ByLook.TryGetValue(map.Essence, out var essence) || !ZNetScene.instance || !ZoneSystem.instance)
            {
                return false;
            }
            var boss = ZNetScene.instance.GetPrefab(essence.Boss);
            string key = boss ? boss.GetComponent<Character>()?.m_defeatSetGlobalKey : null;
            return !string.IsNullOrEmpty(key) && ZoneSystem.instance.GetGlobalKey(key);
        }

        /// <summary>
        ///     One player's map roll for a creature that died in <paramref name="biome" />: the map's prefab, or null.
        ///     <paramref name="extra" /> is a further factor (a rip's loot affixes, <see cref="Corruption.Rules.MapLootBoost.Items" />).
        /// </summary>
        public static GameObject Roll(string biome, System.Collections.Generic.IEnumerable<string> sources, double extra = 1.0)
        {
            if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
            {
                return null;
            }
            var map = MapFor(biome);
            if (!MapDropRules.CanDrop(RipCleared, map != null, map != null && BossDefeated(map)))
            {
                return null;
            }
            float multiplier = (float)(Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources) * extra);
            return MapDropRules.Roll(Loot.Rules.MapDropOneIn, multiplier, Random01) ? Prefab(map) : null;
        }

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new MapDropsCommand());
        }

        /// <summary>
        ///     co_mapdrops: whether creatures where you stand can drop a map, and why not; then rolls the map drop many
        ///     times (as an ordinary kill) and prints the count. Drops nothing.
        /// </summary>
        private sealed class MapDropsCommand : ConsoleCommand
        {
            public override string Name => "co_mapdrops";

            public override string Help => "Corruption: co_mapdrops [rolls] - can creatures here drop a map, and how often (default 100000 rolls; drops nothing)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player || !Loot.Loaded)
                {
                    Console.instance.Print(Loot.Loaded ? "co_mapdrops needs a loaded world." : "Loot tables didn't load; see the log.");
                    return;
                }
                string biome = Loot.BiomeAt(player.transform.position);
                var map = MapFor(biome);
                bool cleared = RipCleared, boss = map != null && BossDefeated(map);
                Console.instance.Print($"Biome here: {biome}. Map: {(map != null ? map.Name : "none for this biome")}. " +
                                       $"A rip heart destroyed in this world: {(cleared ? "yes" : "no")}. " +
                                       $"This biome's boss beaten: {(map == null ? "-" : boss ? "yes" : "no")}.");
                if (!MapDropRules.CanDrop(cleared, map != null, boss))
                {
                    Console.instance.Print("So creatures here drop no maps yet.");
                    return;
                }
                int rolls = args.Length > 0 && int.TryParse(args[0], out var n) ? Mathf.Clamp(n, 1, 1000000) : 100000;
                int hits = 0;
                for (int i = 0; i < rolls; i++)
                {
                    if (MapDropRules.Roll(Loot.Rules.MapDropOneIn, 1f, Random01))
                    {
                        hits++;
                    }
                }
                float chance = MapDropRules.Chance(Loot.Rules.MapDropOneIn, 1f);
                Console.instance.Print($"{rolls} rolls as an ordinary kill: {hits} maps (expected {chance * rolls:0.#}, 1 in {(chance > 0f ? 1f / chance : 0f):0}).");
            }
        }
    }
}
