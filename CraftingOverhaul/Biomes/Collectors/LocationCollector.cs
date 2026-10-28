using System.Collections.Generic;
using SoftReferenceableAssets;
using UnityEngine;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Points of interest: dungeons (including their generated rooms), boss altars, traders, camps and villages.
    ///     Location and room prefabs are loaded on demand and released afterwards.
    /// </summary>
    internal sealed class LocationCollector : ISourceCollector
    {
        public string Name => "Locations";

        private readonly Dictionary<DungeonDB.RoomData, List<ItemSource>> _roomCache =
            new Dictionary<DungeonDB.RoomData, List<ItemSource>>();

        public void Collect(List<ItemSource> sources)
        {
            if (!ZoneSystem.instance)
            {
                return;
            }

            foreach (var location in ZoneSystem.instance.m_locations)
            {
                if (!location.m_enable || !location.m_prefab.IsValid)
                {
                    continue;
                }
                var biome = BiomeMapping.Earliest(location.m_biome);
                if (biome == null)
                {
                    continue;
                }

                var prefab = Load(location.m_prefab);
                if (!prefab)
                {
                    continue;
                }
                try
                {
                    string label = "Location " + (string.IsNullOrEmpty(location.m_prefabName) ? prefab.name : location.m_prefabName);
                    CollectorUtil.AddAll(sources, Prefix(PrefabWalker.Walk(prefab), label), biome, Name);

                    foreach (var generator in prefab.GetComponentsInChildren<DungeonGenerator>(true))
                    {
                        foreach (var room in RoomsFor(generator.m_themes))
                        {
                            CollectorUtil.AddAll(sources, Copy(room, label), biome, Name);
                        }
                    }
                }
                finally
                {
                    location.m_prefab.Release();
                }
            }

            _roomCache.Clear();
        }

        private IEnumerable<List<ItemSource>> RoomsFor(Room.Theme themes)
        {
            if (themes == Room.Theme.None)
            {
                yield break;
            }
            foreach (var room in DungeonDB.GetRooms())
            {
                if (room == null || !room.m_enabled || (room.m_theme & themes) == 0 || !room.m_prefab.IsValid)
                {
                    continue;
                }
                if (!_roomCache.TryGetValue(room, out var found))
                {
                    found = new List<ItemSource>();
                    var prefab = Load(room.m_prefab);
                    if (prefab)
                    {
                        try
                        {
                            found = Prefix(PrefabWalker.Walk(prefab), "Room");
                        }
                        finally
                        {
                            room.m_prefab.Release();
                        }
                    }
                    _roomCache[room] = found;
                }
                yield return found;
            }
        }

        internal static GameObject Load(SoftReference<GameObject> reference)
        {
            try
            {
                return reference.Load() == LoadResult.Succeeded ? reference.Asset : null;
            }
            catch (System.Exception ex)
            {
                Jotunn.Logger.LogDebug($"Could not load {reference.Name}: {ex.Message}");
                return null;
            }
        }

        private static List<ItemSource> Prefix(List<ItemSource> found, string label)
        {
            foreach (var source in found)
            {
                source.Origin = label + " > " + source.Origin;
            }
            return found;
        }

        /// <summary>Room results are shared between locations, so hand out copies before biomes are assigned.</summary>
        private static IEnumerable<ItemSource> Copy(List<ItemSource> found, string label)
        {
            foreach (var s in found)
            {
                var copy = new ItemSource
                {
                    Target = s.Target,
                    Name = s.Name,
                    Tool = s.Tool,
                    RequiredKey = s.RequiredKey,
                    IsWeak = s.IsWeak,
                    Origin = label + " > " + s.Origin
                };
                copy.RequiredItems.AddRange(s.RequiredItems);
                yield return copy;
            }
        }
    }
}
