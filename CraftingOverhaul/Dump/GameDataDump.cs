using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using CraftingOverhaul.Biomes;
using CraftingOverhaul.Biomes.Collectors;
using CraftingOverhaul.RecipeList;
using UnityEngine;

namespace CraftingOverhaul.Dump
{
    /// <summary>
    ///     Writes the loaded game data (vanilla plus any mods) to CSV files, for checking mod logic against real
    ///     data: items, recipes, pieces, creatures, locations, spawn tables, raids, status effects, chest and
    ///     destructible loot, boss altars, and the biome resolver's inputs and results.
    /// </summary>
    internal static class GameDataDump
    {
        public static string Write()
        {
            string dir = Path.Combine(Paths.BepInExRootPath, "CraftingOverhaul", "dump-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(dir);

            BiomeResolver.EnsureResolved();

            WriteMeta(Path.Combine(dir, "meta.txt"));
            WriteItems(Path.Combine(dir, "items.csv"));
            WriteItemDetails(Path.Combine(dir, "item_details.csv"));
            WriteRecipes(Path.Combine(dir, "recipes.csv"));
            WritePieces(Path.Combine(dir, "pieces.csv"));
            WriteCreatures(Path.Combine(dir, "creatures.csv"));
            WriteLocations(Path.Combine(dir, "locations.csv"));
            WriteSpawns(Path.Combine(dir, "spawns.csv"));
            WriteEvents(Path.Combine(dir, "events.csv"), Path.Combine(dir, "event_spawns.csv"));
            WriteStatusEffects(Path.Combine(dir, "status_effects.csv"));
            WriteContainers(Path.Combine(dir, "containers.csv"));
            WriteDestructibles(Path.Combine(dir, "destructibles.csv"));
            WriteAltars(Path.Combine(dir, "altars.csv"));
            WriteBiomeSources(Path.Combine(dir, "biome_sources.csv"));
            WriteBiomeResolution(Path.Combine(dir, "biome_items.csv"));
            WriteUnresolved(Path.Combine(dir, "biome_unresolved.csv"));
            return dir;
        }

        private static string L(string token)
        {
            return string.IsNullOrEmpty(token) ? "" : Localization.instance.Localize(token);
        }

        private static void WriteMeta(string path)
        {
            var lines = new List<string>
            {
                "Game version: " + global::Version.GetVersionString(),
                "CraftingOverhaul: " + CraftingOverhaul.PluginVersion,
                "Written: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "Language: " + Localization.instance.GetSelectedLanguage(),
                "",
                "Loaded plugins:"
            };
            lines.AddRange(Chainloader.PluginInfos.Values
                .Select(p => $"  {p.Metadata.GUID} {p.Metadata.Version} ({p.Metadata.Name})")
                .OrderBy(s => s));
            File.WriteAllLines(path, lines);
        }

        private static void WriteItems(string path)
        {
            using (var csv = new CsvWriter(path, "prefab", "token", "name", "type", "skill", "max_stack", "weight",
                       "max_quality", "value", "teleportable", "tool_tier", "durability", "armor", "armor_per_level",
                       "block", "movement_mod", "damage", "blunt", "slash", "pierce", "chop", "pickaxe", "fire", "frost",
                       "lightning", "poison", "spirit", "food_health", "food_stamina", "food_eitr", "food_duration",
                       "biome", "biome_via"))
            {
                foreach (var prefab in ObjectDB.instance.m_items.Where(p => p).OrderBy(p => p.name))
                {
                    var drop = prefab.GetComponent<ItemDrop>();
                    if (!drop)
                    {
                        continue;
                    }
                    var s = drop.m_itemData.m_shared;
                    var d = s.m_damages;
                    BiomeResolver.TryGet(prefab.name, out var biome);
                    csv.Row(prefab.name, s.m_name, L(s.m_name), s.m_itemType, s.m_skillType, s.m_maxStackSize,
                        s.m_weight, s.m_maxQuality, s.m_value, s.m_teleportable, s.m_toolTier, s.m_maxDurability,
                        s.m_armor, s.m_armorPerLevel, s.m_blockPower, s.m_movementModifier, d.m_damage, d.m_blunt,
                        d.m_slash, d.m_pierce, d.m_chop, d.m_pickaxe, d.m_fire, d.m_frost, d.m_lightning, d.m_poison,
                        d.m_spirit, s.m_food, s.m_foodStamina, s.m_foodEitr, s.m_foodBurnTime,
                        biome?.Biome.ToString(), Describe(biome));
                }
            }
        }

        /// <summary>
        ///     Every item's shared data, field by field (upgrade amounts, knockback, backstab, parry, set bonuses, equip
        ///     effects, regen and stamina modifiers...), plus attack stamina and per-level damage. Wide, for analysis.
        /// </summary>
        private static void WriteItemDetails(string path)
        {
            var fields = SimpleFields(typeof(ItemDrop.ItemData.SharedData));
            var header = new[] { "prefab" }.Concat(fields.Select(f => f.Name))
                .Concat(new[] { "attack_stamina", "secondary_attack_stamina", "damage_per_level" }).ToArray();
            using (var csv = new CsvWriter(path, header))
            {
                foreach (var prefab in ObjectDB.instance.m_items.Where(p => p).OrderBy(p => p.name))
                {
                    var drop = prefab.GetComponent<ItemDrop>();
                    if (!drop)
                    {
                        continue;
                    }
                    var s = drop.m_itemData.m_shared;
                    var perLevel = s.m_damagesPerLevel;
                    float perLevelTotal = perLevel.m_damage + perLevel.m_blunt + perLevel.m_slash + perLevel.m_pierce +
                                          perLevel.m_fire + perLevel.m_frost + perLevel.m_lightning + perLevel.m_poison + perLevel.m_spirit;
                    csv.Row(new object[] { prefab.name }.Concat(fields.Select(f => Value(f, s)))
                        .Concat(new object[] { s.m_attack?.m_attackStamina, s.m_secondaryAttack?.m_attackStamina, perLevelTotal })
                        .ToArray());
                }
            }
        }

        private static void WriteRecipes(string path)
        {
            using (var csv = new CsvWriter(path, "recipe", "output", "output_name", "amount", "enabled", "station",
                       "min_station_level", "ingredients", "type_category", "biome_category"))
            {
                foreach (var recipe in ObjectDB.instance.m_recipes.Where(r => r && r.m_item).OrderBy(r => r.name))
                {
                    csv.Row(recipe.name, recipe.m_item.gameObject.name, L(recipe.m_item.m_itemData.m_shared.m_name),
                        recipe.m_amount, recipe.m_enabled,
                        recipe.m_craftingStation ? recipe.m_craftingStation.m_name : "", recipe.m_minStationLevel,
                        Requirements(recipe.m_resources),
                        RecipeCategories.Get(recipe, GroupingMode.ItemType).Label,
                        RecipeCategories.Get(recipe, GroupingMode.Biome).Label);
                }
            }
        }

        private static void WritePieces(string path)
        {
            using (var csv = new CsvWriter(path, "prefab", "token", "name", "category", "enabled", "station", "resources"))
            {
                foreach (var prefab in ZNetScene.instance.m_prefabs.Where(p => p).OrderBy(p => p.name))
                {
                    var piece = prefab.GetComponent<Piece>();
                    if (!piece)
                    {
                        continue;
                    }
                    csv.Row(prefab.name, piece.m_name, L(piece.m_name), piece.m_category, piece.m_enabled,
                        piece.m_craftingStation ? piece.m_craftingStation.m_name : "", Requirements(piece.m_resources));
                }
            }
        }

        private static void WriteCreatures(string path)
        {
            var spawnBiomes = new Dictionary<GameObject, Heightmap.Biome>();
            foreach (var list in SpawnCollector.FindSpawnLists())
            {
                foreach (var spawn in list.m_spawners.Where(s => s.m_enabled && s.m_prefab))
                {
                    spawnBiomes.TryGetValue(spawn.m_prefab, out var flags);
                    spawnBiomes[spawn.m_prefab] = flags | spawn.m_biome;
                }
            }

            using (var csv = new CsvWriter(path, "prefab", "token", "name", "health", "faction", "boss",
                       "defeat_key", "spawn_biomes", "drops"))
            {
                foreach (var prefab in ZNetScene.instance.m_prefabs.Where(p => p).OrderBy(p => p.name))
                {
                    var character = prefab.GetComponent<Character>();
                    if (!character || character is Player)
                    {
                        continue;
                    }
                    var drop = prefab.GetComponent<CharacterDrop>();
                    var drops = drop?.m_drops == null
                        ? ""
                        : CsvWriter.Join(drop.m_drops.Where(x => x.m_prefab).Select(x =>
                            $"{x.m_prefab.name} {x.m_amountMin}-{x.m_amountMax} @{x.m_chance:0.##}"));
                    spawnBiomes.TryGetValue(prefab, out var biomes);
                    csv.Row(prefab.name, character.m_name, L(character.m_name), character.m_health,
                        character.m_faction, character.m_boss, character.m_defeatSetGlobalKey,
                        biomes == Heightmap.Biome.None ? "" : biomes.ToString(), drops);
                }
            }
        }

        private static void WriteLocations(string path)
        {
            using (var csv = new CsvWriter(path, "location", "prefab", "enabled", "biome", "quantity", "unique"))
            {
                foreach (var location in ZoneSystem.instance.m_locations.OrderBy(l => l.m_prefabName))
                {
                    csv.Row(location.m_name, location.m_prefabName, location.m_enable, location.m_biome,
                        location.m_quantity, location.m_unique);
                }
            }
        }

        // ------------------------------------------------------------------ raw game tables, by reflection
        // Written field by field (every public simple field), so the files keep working when a game update adds
        // or renames fields.

        /// <summary>World spawn tables: what spawns in each biome, how many, how often, at which levels.</summary>
        private static void WriteSpawns(string path)
        {
            var fields = SimpleFields(typeof(SpawnSystem.SpawnData));
            using (var csv = new CsvWriter(path, new[] { "list" }.Concat(fields.Select(f => f.Name)).ToArray()))
            {
                foreach (var list in SpawnCollector.FindSpawnLists())
                {
                    foreach (var spawn in list.m_spawners)
                    {
                        csv.Row(new object[] { list.name }.Concat(fields.Select(f => Value(f, spawn))).ToArray());
                    }
                }
            }
        }

        /// <summary>Random events (raids) and each one's spawn list.</summary>
        private static void WriteEvents(string eventsPath, string spawnsPath)
        {
            var events = RandEventSystem.instance ? RandEventSystem.instance.m_events : new List<RandomEvent>();
            var eventFields = SimpleFields(typeof(RandomEvent));
            using (var csv = new CsvWriter(eventsPath, eventFields.Select(f => f.Name).Concat(new[] { "spawn_entries" }).ToArray()))
            {
                foreach (var e in events.Where(e => e != null))
                {
                    csv.Row(eventFields.Select(f => Value(f, e)).Concat(new object[] { e.m_spawn?.Count ?? 0 }).ToArray());
                }
            }
            var spawnFields = SimpleFields(typeof(SpawnSystem.SpawnData));
            using (var csv = new CsvWriter(spawnsPath, new[] { "event" }.Concat(spawnFields.Select(f => f.Name)).ToArray()))
            {
                foreach (var e in events.Where(e => e?.m_spawn != null))
                {
                    foreach (var spawn in e.m_spawn)
                    {
                        csv.Row(new object[] { e.m_name }.Concat(spawnFields.Select(f => Value(f, spawn))).ToArray());
                    }
                }
            }
        }

        /// <summary>Every status effect with its class and the numbers it sets (fields left at zero/empty are skipped).</summary>
        private static void WriteStatusEffects(string path)
        {
            using (var csv = new CsvWriter(path, "name", "token", "display_name", "class", "fields"))
            {
                foreach (var se in ObjectDB.instance.m_StatusEffects.Where(x => x).OrderBy(x => x.name))
                {
                    var values = SimpleFields(se.GetType())
                        .Select(f => new KeyValuePair<string, object>(f.Name, Value(f, se)))
                        .Where(kv => !IsEmpty(kv.Value))
                        .Select(kv => $"{kv.Key}={Format(kv.Value)}");
                    csv.Row(se.name, se.m_name, L(se.m_name), se.GetType().Name, CsvWriter.Join(values));
                }
            }
        }

        /// <summary>Chests and other containers that come with loot (dungeon and location chests).</summary>
        private static void WriteContainers(string path)
        {
            using (var csv = new CsvWriter(path, "prefab", "name", "drop_min", "drop_max", "drop_chance", "one_of_each", "drops"))
            {
                foreach (var prefab in ZNetScene.instance.m_prefabs.Where(p => p).OrderBy(p => p.name))
                {
                    var container = prefab.GetComponent<Container>();
                    var table = container ? container.m_defaultItems : null;
                    if (table?.m_drops == null || table.m_drops.Count == 0)
                    {
                        continue;
                    }
                    csv.Row(prefab.name, L(container.m_name), table.m_dropMin, table.m_dropMax, table.m_dropChance,
                        table.m_oneOfEach, DropList(table));
                }
            }
        }

        /// <summary>Destructible objects (nests, rocks, crates): health, tool tier and what they drop.</summary>
        private static void WriteDestructibles(string path)
        {
            using (var csv = new CsvWriter(path, "prefab", "health", "min_tool_tier", "type", "drop_min", "drop_max", "drop_chance", "drops"))
            {
                foreach (var prefab in ZNetScene.instance.m_prefabs.Where(p => p).OrderBy(p => p.name))
                {
                    var destructible = prefab.GetComponent<Destructible>();
                    if (!destructible)
                    {
                        continue;
                    }
                    var table = prefab.GetComponent<DropOnDestroyed>()?.m_dropWhenDestroyed;
                    csv.Row(prefab.name, destructible.m_health, destructible.m_minToolTier, destructible.m_destructibleType,
                        table?.m_dropMin, table?.m_dropMax, table?.m_dropChance, table == null ? "" : DropList(table));
                }
            }
        }

        /// <summary>
        ///     Boss altars (offering bowls) in every location: the offering item and count, the boss they summon, and
        ///     for item-stand altars the stands and the items they take.
        /// </summary>
        private static void WriteAltars(string path)
        {
            using (var csv = new CsvWriter(path, "location", "biome", "altar", "offering", "offering_count", "boss",
                       "use_item_stands", "item_stand_prefix", "item_stands", "set_global_key", "spawn_delay"))
            {
                foreach (var location in ZoneSystem.instance.m_locations.Where(l => l.m_enable && l.m_prefab.IsValid))
                {
                    var prefab = LocationCollector.Load(location.m_prefab);
                    if (!prefab)
                    {
                        continue;
                    }
                    try
                    {
                        foreach (var bowl in prefab.GetComponentsInChildren<OfferingBowl>(true))
                        {
                            var stands = bowl.m_useItemStands
                                ? prefab.GetComponentsInChildren<ItemStand>(true)
                                    .Where(st => string.IsNullOrEmpty(bowl.m_itemStandPrefix) || st.gameObject.name.StartsWith(bowl.m_itemStandPrefix))
                                    .Select(st => st.gameObject.name + " [" + string.Join("|", (st.m_supportedItems ?? new List<ItemDrop>())
                                        .Where(it => it).Select(it => it.gameObject.name)) + "]")
                                : Enumerable.Empty<string>();
                            csv.Row(location.m_prefabName, location.m_biome, bowl.gameObject.name,
                                bowl.m_bossItem ? bowl.m_bossItem.gameObject.name : "", bowl.m_bossItems,
                                bowl.m_bossPrefab ? bowl.m_bossPrefab.name : "", bowl.m_useItemStands, bowl.m_itemStandPrefix,
                                CsvWriter.Join(stands), bowl.m_setGlobalKey, bowl.m_spawnBossDelay);
                        }
                    }
                    finally
                    {
                        location.m_prefab.Release();
                    }
                }
            }
        }

        private static string DropList(DropTable table)
        {
            return CsvWriter.Join(table.m_drops.Where(d => d.m_item).Select(d =>
                $"{d.m_item.name} {d.m_stackMin}-{d.m_stackMax} w{d.m_weight:0.##}"));
        }

        private static FieldInfo[] SimpleFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => IsSimple(f.FieldType)).ToArray();
        }

        private static bool IsSimple(Type t)
        {
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Vector3) || t == typeof(Color) ||
                   typeof(UnityEngine.Object).IsAssignableFrom(t) || t == typeof(List<string>);
        }

        private static object Value(FieldInfo field, object owner)
        {
            object value = field.GetValue(owner);
            switch (value)
            {
                case UnityEngine.Object unityObject:
                    return unityObject ? unityObject.name : "";
                case List<string> list:
                    return string.Join("|", list);
                default:
                    return value;
            }
        }

        private static bool IsEmpty(object value)
        {
            switch (value)
            {
                case null: return true;
                case string s: return s.Length == 0;
                case bool b: return !b;
                case float f: return f == 0f;
                case int i: return i == 0;
                case long l: return l == 0;
                default: return false;
            }
        }

        private static string Format(object value)
        {
            return value is float f ? f.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) : value.ToString();
        }

        private static void WriteBiomeSources(string path)
        {
            using (var csv = new CsvWriter(path, "target", "name", "collector", "base_biome", "required_items",
                       "tool", "required_key", "origin"))
            {
                foreach (var s in BiomeResolver.Sources.OrderBy(s => s.Name).ThenBy(s => s.Collector))
                {
                    csv.Row(s.Target, s.Name, s.Collector, s.Base?.ToString(), CsvWriter.Join(s.RequiredItems),
                        s.Tool.ToString(), s.RequiredKey, s.Origin);
                }
            }
        }

        private static void WriteBiomeResolution(string path)
        {
            using (var csv = new CsvWriter(path, "item", "name", "biome", "override", "collector", "origin"))
            {
                foreach (var entry in BiomeResolver.AllItems().OrderBy(e => e.Value.Biome).ThenBy(e => e.Key))
                {
                    var prefab = ObjectDB.instance.GetItemPrefab(entry.Key);
                    var name = ItemName(prefab);
                    csv.Row(entry.Key, name, entry.Value.Biome, entry.Value.IsOverride,
                        entry.Value.Source?.Collector, entry.Value.Source?.Origin);
                }
            }
        }

        /// <summary>Recipe ingredients the resolver could not place: candidates for BiomeMaterialOverrides.</summary>
        private static void WriteUnresolved(string path)
        {
            var usedBy = new Dictionary<string, List<string>>();
            foreach (var recipe in ObjectDB.instance.m_recipes.Where(r => r && r.m_enabled && r.m_item))
            {
                foreach (var req in recipe.m_resources.Where(r => r.m_resItem))
                {
                    string name = req.m_resItem.gameObject.name;
                    if (BiomeResolver.TryGet(name, out _))
                    {
                        continue;
                    }
                    if (!usedBy.TryGetValue(name, out var list))
                    {
                        usedBy[name] = list = new List<string>();
                    }
                    list.Add(recipe.m_item.gameObject.name);
                }
            }

            using (var csv = new CsvWriter(path, "item", "name", "sources_found", "used_in_recipes"))
            {
                foreach (var entry in usedBy.OrderBy(e => e.Key))
                {
                    var prefab = ObjectDB.instance.GetItemPrefab(entry.Key);
                    var name = ItemName(prefab);
                    int sources = BiomeResolver.Sources.Count(s => s.Target == SourceTarget.Item &&
                                                                   string.Equals(s.Name, entry.Key, StringComparison.OrdinalIgnoreCase));
                    csv.Row(entry.Key, name, sources, CsvWriter.Join(entry.Value.Distinct()));
                }
            }
        }

        private static string ItemName(GameObject prefab)
        {
            var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
            return drop ? L(drop.m_itemData.m_shared.m_name) : "";
        }

        private static string Requirements(Piece.Requirement[] resources)
        {
            if (resources == null)
            {
                return "";
            }
            return CsvWriter.Join(resources.Where(r => r.m_resItem).Select(r =>
                $"{r.m_resItem.gameObject.name} x{r.m_amount}" + (r.m_amountPerLevel > 0 ? $" (+{r.m_amountPerLevel}/lvl)" : "")));
        }

        private static string Describe(BiomeResolution resolution)
        {
            if (resolution == null)
            {
                return "";
            }
            return resolution.IsOverride ? "override" : $"{resolution.Source.Collector}: {resolution.Source.Origin}";
        }
    }
}
