using System.Collections.Generic;
using UnityEngine;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>Crafted items need their ingredients and the crafting station they are made at.</summary>
    internal sealed class RecipeCollector : ISourceCollector
    {
        public string Name => "Recipes";

        public void Collect(List<ItemSource> sources)
        {
            if (!ObjectDB.instance)
            {
                return;
            }

            var stations = FindStationPieces();

            foreach (var recipe in ObjectDB.instance.m_recipes)
            {
                if (!recipe || !recipe.m_enabled || !recipe.m_item)
                {
                    continue;
                }
                var source = new ItemSource
                {
                    Name = recipe.m_item.gameObject.name,
                    Collector = Name,
                    Origin = "Recipe " + recipe.name
                };
                foreach (var req in recipe.m_resources)
                {
                    if (req.m_resItem && req.m_amount > 0)
                    {
                        source.RequiredItems.Add(req.m_resItem.gameObject.name);
                    }
                }
                if (recipe.m_craftingStation && stations.TryGetValue(recipe.m_craftingStation.m_name, out var station))
                {
                    PieceUtil.AddResources(source.RequiredItems, station);
                    source.Origin += " @ " + station.name;
                }
                if (source.RequiredItems.Count > 0)
                {
                    sources.Add(source);
                }
            }
        }

        private static Dictionary<string, Piece> FindStationPieces()
        {
            var stations = new Dictionary<string, Piece>();
            if (!ZNetScene.instance)
            {
                return stations;
            }
            foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
            {
                var station = prefab ? prefab.GetComponent<CraftingStation>() : null;
                var piece = prefab ? prefab.GetComponent<Piece>() : null;
                if (station && piece && !stations.ContainsKey(station.m_name))
                {
                    stations[station.m_name] = piece;
                }
            }
            return stations;
        }
    }
}
