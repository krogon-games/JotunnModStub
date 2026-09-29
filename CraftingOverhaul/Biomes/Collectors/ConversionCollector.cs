using System.Collections.Generic;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Processing stations: smelters, kilns, spinning wheel, windmill, eitr refinery, fermenter and cooking
    ///     stations. The output needs the input, the fuel and the station itself.
    /// </summary>
    internal sealed class ConversionCollector : ISourceCollector
    {
        public string Name => "Conversions";

        public void Collect(List<ItemSource> sources)
        {
            if (!ZNetScene.instance)
            {
                return;
            }
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                if (!prefab)
                {
                    continue;
                }
                var piece = prefab.GetComponent<Piece>();

                var smelter = prefab.GetComponent<Smelter>();
                if (smelter && smelter.m_conversion != null)
                {
                    foreach (var c in smelter.m_conversion)
                    {
                        Add(sources, c.m_from, c.m_to, smelter.m_fuelItem, piece, prefab.name);
                    }
                }

                var fermenter = prefab.GetComponent<Fermenter>();
                if (fermenter && fermenter.m_conversion != null)
                {
                    foreach (var c in fermenter.m_conversion)
                    {
                        Add(sources, c.m_from, c.m_to, null, piece, prefab.name);
                    }
                }

                var cooking = prefab.GetComponent<CookingStation>();
                if (cooking && cooking.m_conversion != null)
                {
                    foreach (var c in cooking.m_conversion)
                    {
                        Add(sources, c.m_from, c.m_to, cooking.m_fuelItem, piece, prefab.name);
                    }
                }
            }
        }

        private void Add(List<ItemSource> sources, ItemDrop from, ItemDrop to, ItemDrop fuel, Piece station, string stationName)
        {
            if (!from || !to)
            {
                return;
            }
            var source = new ItemSource
            {
                Name = to.gameObject.name,
                Collector = Name,
                Origin = $"{stationName}: {from.gameObject.name} > {to.gameObject.name}"
            };
            source.RequiredItems.Add(from.gameObject.name);
            if (fuel)
            {
                source.RequiredItems.Add(fuel.gameObject.name);
            }
            PieceUtil.AddResources(source.RequiredItems, station);
            sources.Add(source);
        }
    }

    internal static class PieceUtil
    {
        /// <summary>Adds the items needed to build a piece (its level-1 cost).</summary>
        public static void AddResources(List<string> into, Piece piece)
        {
            if (!piece || piece.m_resources == null)
            {
                return;
            }
            foreach (var req in piece.m_resources)
            {
                if (req.m_resItem && req.m_amount > 0)
                {
                    into.Add(req.m_resItem.gameObject.name);
                }
            }
        }
    }
}
