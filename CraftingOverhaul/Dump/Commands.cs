using System;
using System.Collections.Generic;
using System.Linq;
using CraftingOverhaul.Biomes;
using Jotunn.Entities;

namespace CraftingOverhaul.Dump
{
    /// <summary>co_dump: writes the loaded game data to CSV files under BepInEx/CraftingOverhaul.</summary>
    internal sealed class DumpCommand : ConsoleCommand
    {
        public override string Name => "co_dump";

        public override string Help => "CraftingOverhaul: write items, recipes, creatures and biome data to CSV (BepInEx/CraftingOverhaul)";

        public override void Run(string[] args)
        {
            if (!ObjectDB.instance || !ZNetScene.instance || !ZoneSystem.instance)
            {
                Out.Print("co_dump needs a loaded world.");
                return;
            }
            try
            {
                Out.Print("Dump written to " + GameDataDump.Write());
            }
            catch (Exception ex)
            {
                Out.Print("co_dump failed: " + ex.Message);
                Jotunn.Logger.LogError(ex);
            }
        }
    }

    /// <summary>
    ///     co_biome [item|refresh]: explains how an item's biome was worked out, or re-runs the resolver.
    /// </summary>
    internal sealed class BiomeCommand : ConsoleCommand
    {
        public override string Name => "co_biome";

        public override string Help => "CraftingOverhaul: 'co_biome <item prefab>' explains an item's biome; 'co_biome refresh' recalculates";

        public override void Run(string[] args)
        {
            if (!ObjectDB.instance || !ZoneSystem.instance)
            {
                Out.Print("co_biome needs a loaded world.");
                return;
            }

            if (args.Length == 0)
            {
                var counts = BiomeResolver.AllItems().GroupBy(e => e.Value.Biome).OrderBy(g => g.Key);
                Out.Print("Items per biome: " + string.Join(", ", counts.Select(g => $"{g.Key} {g.Count()}")));
                Out.Print("Usage: co_biome <item prefab>, e.g. co_biome Bronze. Unplaced recipe ingredients are listed by co_dump.");
                return;
            }

            if (args[0].Equals("refresh", StringComparison.OrdinalIgnoreCase))
            {
                BiomeResolver.Invalidate();
                RecipeList.RecipeCategories.ClearCache();
                BiomeResolver.EnsureResolved();
                RecipeList.RecipeListOrganizer.Refresh();
                Out.Print("Biomes recalculated.");
                return;
            }

            Explain(args[0], 0, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public override List<string> CommandOptionList()
        {
            return ObjectDB.instance ? ObjectDB.instance.m_items.Where(p => p).Select(p => p.name).ToList() : new List<string>();
        }

        private static void Explain(string item, int depth, HashSet<string> seen)
        {
            string indent = new string(' ', depth * 2);
            if (!seen.Add(item) || depth > 6)
            {
                return;
            }
            if (!BiomeResolver.TryGet(item, out var r))
            {
                Out.Print($"{indent}{item}: no biome found");
                return;
            }
            if (r.IsOverride)
            {
                Out.Print($"{indent}{item}: {r.Biome} (override)");
                return;
            }

            var s = r.Source;
            Out.Print($"{indent}{item}: {r.Biome} <- {s.Collector}: {s.Origin}" + (s.Base != null ? $" [found in {s.Base}]" : ""));
            if (s.Tool.Tier > 0)
            {
                Out.Print($"{indent}  needs {s.Tool}");
            }
            if (!string.IsNullOrEmpty(s.RequiredKey))
            {
                string key = BiomeResolver.TryGetKey(s.RequiredKey, out var k) ? k.Biome.ToString() : "?";
                Out.Print($"{indent}  needs global key {s.RequiredKey} ({key})");
            }
            foreach (var req in s.RequiredItems.Distinct())
            {
                Explain(req, depth + 1, seen);
            }
        }
    }

    internal static class Out
    {
        public static void Print(string message)
        {
            Jotunn.Logger.LogInfo(message);
            if (global::Console.instance)
            {
                global::Console.instance.Print(message);
            }
        }
    }
}
