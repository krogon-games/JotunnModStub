using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CraftingOverhaul.Biomes
{
    /// <summary>
    ///     What the saved biomes depend on, as named hashes (see <see cref="BiomeCacheFile" />). Anything that can
    ///     change the result changes a part: the game version, every loaded mod and its version, this DLL's build,
    ///     the overrides, and a fingerprint of the game data the collectors read (items, recipes, prefabs,
    ///     vegetation, locations, spawn tables). All of it is in memory, so building the key takes a few milliseconds.
    ///     A mod setting that changes drops inside a prefab is not seen; <c>co_biome refresh</c> covers that.
    /// </summary>
    internal static class BiomeCacheKey
    {
        public static IList<KeyValuePair<string, string>> Build()
        {
            return new List<KeyValuePair<string, string>>
            {
                Part("game", Version.GetVersionString(false)),
                Part("mods", Mods()),
                Part("overrides", ModConfig.BiomeMaterialOverrides.Value),
                Part("items", Items()),
                Part("recipes", Recipes()),
                Part("prefabs", Prefabs()),
                Part("world", World())
            };
        }

        private static KeyValuePair<string, string> Part(string name, params string[] values)
        {
            return Part(name, (IEnumerable<string>)values);
        }

        private static KeyValuePair<string, string> Part(string name, IEnumerable<string> values)
        {
            return new KeyValuePair<string, string>(name, BiomeCacheFile.Hash(values));
        }

        private static IEnumerable<string> Mods()
        {
            var mods = BepInEx.Bootstrap.Chainloader.PluginInfos.Values
                .Where(p => p?.Metadata != null)
                .Select(p => p.Metadata.GUID + " " + p.Metadata.Version)
                .OrderBy(s => s, System.StringComparer.Ordinal)
                .ToList();
            // Dev builds share a version number; the build stamp tells them apart.
            mods.Add("build " + CraftingOverhaul.BuildStamp());
            return mods;
        }

        private static IEnumerable<string> Items()
        {
            return ObjectDB.instance.m_items.Where(p => p).Select(p => p.name).OrderBy(s => s, System.StringComparer.Ordinal);
        }

        private static IEnumerable<string> Recipes()
        {
            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                if (!recipe)
                {
                    continue;
                }
                yield return recipe.name;
                yield return recipe.m_enabled ? "on" : "off";
                yield return recipe.m_item ? recipe.m_item.name : "";
                yield return recipe.m_craftingStation ? recipe.m_craftingStation.m_name : "";
                if (recipe.m_resources == null)
                {
                    continue;
                }
                foreach (var req in recipe.m_resources)
                {
                    yield return (req.m_resItem ? req.m_resItem.name : "") + " " + req.m_amount.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>Every prefab's name, and what each buildable piece costs (stations and producers need their cost).</summary>
        private static IEnumerable<string> Prefabs()
        {
            var lines = new List<string>();
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (!prefab)
                {
                    continue;
                }
                var piece = prefab.GetComponent<Piece>();
                if (!piece || piece.m_resources == null)
                {
                    lines.Add(prefab.name);
                    continue;
                }
                var line = new System.Text.StringBuilder(prefab.name);
                foreach (var req in piece.m_resources)
                {
                    line.Append(' ').Append(req.m_resItem ? req.m_resItem.name : "").Append(' ')
                        .Append(req.m_amount.ToString(CultureInfo.InvariantCulture));
                }
                lines.Add(line.ToString());
            }
            lines.Sort(System.StringComparer.Ordinal);
            return lines;
        }

        /// <summary>Vegetation, locations and spawn tables: what is placed in which biome. Sorted, as mods' lists come in any order.</summary>
        private static IEnumerable<string> World()
        {
            var lines = WorldLines().ToList();
            lines.Sort(System.StringComparer.Ordinal);
            return lines;
        }

        private static IEnumerable<string> WorldLines()
        {
            foreach (var veg in ZoneSystem.instance.m_vegetation)
            {
                yield return "v " + (veg.m_prefab ? veg.m_prefab.name : "") + " " + (int)veg.m_biome + (veg.m_enable ? "" : " off");
            }
            foreach (var location in ZoneSystem.instance.m_locations)
            {
                yield return "l " + location.m_prefabName + " " + (int)location.m_biome + (location.m_enable ? "" : " off");
            }
            foreach (var list in Collectors.SpawnCollector.FindSpawnLists())
            {
                foreach (var spawn in list.m_spawners)
                {
                    yield return "s " + (spawn.m_prefab ? spawn.m_prefab.name : "") + " " + (int)spawn.m_biome + " " +
                                 spawn.m_requiredGlobalKey + (spawn.m_enabled ? "" : " off");
                }
            }
        }
    }
}
