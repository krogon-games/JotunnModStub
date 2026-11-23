using System.Linq;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Maps as loot (Krogon, 2026-11-23, decisions 18-19 and 31-32; <see cref="MapDropRules" />): one guaranteed
    ///     map in every rip chest (<see cref="RipLoot" />), and creatures dropping a map of the biome they die in, once
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

        /// <summary>One player's map roll for a creature that died in <paramref name="biome" />: the map's prefab, or null.</summary>
        public static GameObject Roll(string biome, System.Collections.Generic.IEnumerable<string> sources)
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
            float multiplier = Loot.Drops.Multiplier(Corruption.Rules.DropTable.OrbColumn, sources);
            return MapDropRules.Roll(Loot.Rules.MapDropOneIn, multiplier, Random01) ? Prefab(map) : null;
        }
    }
}
