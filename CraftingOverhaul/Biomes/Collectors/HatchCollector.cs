using System.Collections.Generic;

namespace CraftingOverhaul.Biomes.Collectors
{
    /// <summary>
    ///     Items that hatch into creatures (e.g. chicken eggs): everything the creature gives, including what it
    ///     grows up into and its own offspring, needs the item.
    /// </summary>
    internal sealed class HatchCollector : ISourceCollector
    {
        public string Name => "Hatching";

        public void Collect(List<ItemSource> sources)
        {
            if (!ObjectDB.instance)
            {
                return;
            }
            foreach (var prefab in ObjectDB.instance.m_items)
            {
                var egg = prefab ? prefab.GetComponent<EggGrow>() : null;
                if (!egg || !egg.m_grownPrefab)
                {
                    continue;
                }
                foreach (var found in PrefabWalker.Walk(egg.m_grownPrefab))
                {
                    found.Collector = Name;
                    found.Origin = prefab.name + " hatches > " + found.Origin;
                    found.RequiredItems.Add(prefab.name);
                    sources.Add(found);
                }
            }
        }
    }
}
