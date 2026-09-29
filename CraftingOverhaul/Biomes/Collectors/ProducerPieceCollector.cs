using System.Collections.Generic;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Buildable pieces that produce items: planted crops (via the grown pickable), beehives and sap collectors.
    ///     The produce needs whatever it takes to build the piece, e.g. carrot seeds for carrots.
    /// </summary>
    internal sealed class ProducerPieceCollector : ISourceCollector
    {
        public string Name => "Producers";

        public void Collect(List<ItemSource> sources)
        {
            if (!ZNetScene.instance)
            {
                return;
            }
            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                var piece = prefab ? prefab.GetComponent<Piece>() : null;
                if (!piece)
                {
                    continue;
                }

                var plant = prefab.GetComponent<Plant>();
                if (plant && plant.m_grownPrefabs != null)
                {
                    foreach (var grown in plant.m_grownPrefabs)
                    {
                        if (!grown)
                        {
                            continue;
                        }
                        foreach (var found in PrefabWalker.Walk(grown))
                        {
                            Add(sources, found, piece, prefab.name);
                        }
                    }
                }

                var hive = prefab.GetComponent<Beehive>();
                if (hive && hive.m_honeyItem)
                {
                    Add(sources, new ItemSource { Name = hive.m_honeyItem.gameObject.name, Origin = "Beehive" }, piece, prefab.name);
                }

                var sap = prefab.GetComponent<SapCollector>();
                if (sap && sap.m_spawnItem)
                {
                    Add(sources, new ItemSource { Name = sap.m_spawnItem.gameObject.name, Origin = "SapCollector" }, piece, prefab.name);
                }
            }
        }

        private void Add(List<ItemSource> sources, ItemSource source, Piece piece, string pieceName)
        {
            source.Collector = Name;
            source.Origin = pieceName + " > " + source.Origin;
            PieceUtil.AddResources(source.RequiredItems, piece);
            sources.Add(source);
        }
    }
}
