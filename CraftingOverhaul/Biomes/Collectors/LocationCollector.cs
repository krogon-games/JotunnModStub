using System;
using System.Collections;
using System.Collections.Generic;
using SoftReferenceableAssets;
using UnityEngine;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Points of interest: dungeons (including their generated rooms), boss altars, traders, camps and villages.
    ///     Location and room prefabs live in asset bundles: they are loaded in the background a few at a time,
    ///     walked, and released again, so the game keeps running while this works (see <see cref="LoadEach" />).
    /// </summary>
    internal sealed class LocationCollector : ISourceCollector
    {
        /// <summary>
        ///     How many prefabs load in the background at once. A few keep the loader busy between frames without
        ///     holding many bundles in memory.
        /// </summary>
        private const int LoadsInFlight = 4;

        public string Name => "Locations";

        private sealed class Place
        {
            public Biome Biome;
            public string PrefabName;
            public string Label;
            public readonly List<Room.Theme> DungeonThemes = new List<Room.Theme>();
        }

        public IEnumerable Collect(List<ItemSource> sources, CollectRun run)
        {
            if (!ZoneSystem.instance)
            {
                yield break;
            }

            var places = new List<Place>();
            var placeRefs = new List<SoftReference<GameObject>>();
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
                places.Add(new Place { Biome = biome.Value, PrefabName = location.m_prefabName });
                placeRefs.Add(location.m_prefab);
            }

            // 1. Each location's own contents, and the dungeon themes it generates rooms from.
            var allThemes = Room.Theme.None;
            foreach (var step in LoadEach(placeRefs, run, (i, prefab) =>
                     {
                         var place = places[i];
                         place.Label = "Location " + (string.IsNullOrEmpty(place.PrefabName) ? prefab.name : place.PrefabName);
                         CollectorUtil.AddAll(sources, Prefix(PrefabWalker.Walk(prefab), place.Label), place.Biome, Name);
                         foreach (var generator in prefab.GetComponentsInChildren<DungeonGenerator>(true))
                         {
                             if (generator.m_themes != Room.Theme.None)
                             {
                                 place.DungeonThemes.Add(generator.m_themes);
                                 allThemes |= generator.m_themes;
                             }
                         }
                     }))
            {
                yield return step;
            }

            if (allThemes == Room.Theme.None)
            {
                yield break;
            }

            // 2. Every room those dungeons can use, each loaded once.
            var rooms = new List<DungeonDB.RoomData>();
            var roomRefs = new List<SoftReference<GameObject>>();
            foreach (var room in DungeonDB.GetRooms())
            {
                if (room != null && room.m_enabled && (room.m_theme & allThemes) != 0 && room.m_prefab.IsValid)
                {
                    rooms.Add(room);
                    roomRefs.Add(room.m_prefab);
                }
            }
            var roomSources = new List<ItemSource>[rooms.Count];
            foreach (var step in LoadEach(roomRefs, run, (i, prefab) => roomSources[i] = Prefix(PrefabWalker.Walk(prefab), "Room")))
            {
                yield return step;
            }

            // 3. Each dungeon gets copies of its rooms' sources, in its own biome.
            foreach (var place in places)
            {
                if (place.Label == null)
                {
                    continue;
                }
                foreach (var themes in place.DungeonThemes)
                {
                    for (int r = 0; r < rooms.Count; r++)
                    {
                        if ((rooms[r].m_theme & themes) != 0 && roomSources[r] != null)
                        {
                            CollectorUtil.AddAll(sources, Copy(roomSources[r], place.Label), place.Biome, Name);
                        }
                    }
                }
                yield return null;
            }
        }

        /// <summary>
        ///     Loads each prefab, hands it to <paramref name="visit" /> in order and releases it again. Up to
        ///     <see cref="LoadsInFlight" /> load in the background at once; while the next one is still loading this
        ///     yields <see cref="SlicedWork.WaitFrame" />. Run synchronously, it waits for each load instead.
        ///     Every reference taken is released, also when the work is stopped half-way (disposed).
        /// </summary>
        private static IEnumerable LoadEach(List<SoftReference<GameObject>> refs, CollectRun run, Action<int, GameObject> visit)
        {
            var held = new bool[refs.Count];
            int next = 0;
            try
            {
                for (int i = 0; i < refs.Count; i++)
                {
                    for (; next < refs.Count && next < i + LoadsInFlight; next++)
                    {
                        held[next] = StartLoad(refs[next], run.Synchronous);
                    }
                    if (!held[i])
                    {
                        continue;
                    }

                    while (refs[i].IsLoading)
                    {
                        if (run.Synchronous)
                        {
                            refs[i].WaitForLoadToComplete();
                            break;
                        }
                        yield return SlicedWork.WaitFrame;
                    }

                    var prefab = refs[i].IsLoaded ? refs[i].Asset : null;
                    if (prefab)
                    {
                        Visit(visit, i, prefab, refs[i].Name);
                    }
                    held[i] = false;
                    refs[i].Release();
                    yield return null;
                }
            }
            finally
            {
                for (int i = 0; i < held.Length; i++)
                {
                    if (held[i])
                    {
                        refs[i].Release();
                    }
                }
            }
        }

        /// <summary>Starts loading; true when a reference is now held (and must be released).</summary>
        private static bool StartLoad(SoftReference<GameObject> reference, bool synchronous)
        {
            try
            {
                if (synchronous)
                {
                    reference.Load();
                }
                else
                {
                    reference.LoadAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogDebug($"Could not load {reference.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>One broken prefab (e.g. from another mod) skips that prefab, not the rest.</summary>
        private static void Visit(Action<int, GameObject> visit, int index, GameObject prefab, string name)
        {
            try
            {
                visit(index, prefab);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"Biome collector Locations skipped {name}: {ex.Message}");
            }
        }

        /// <summary>Loads a prefab right away (for the data dump). The caller releases it.</summary>
        internal static GameObject Load(SoftReference<GameObject> reference)
        {
            try
            {
                return reference.Load() == LoadResult.Succeeded ? reference.Asset : null;
            }
            catch (Exception ex)
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
