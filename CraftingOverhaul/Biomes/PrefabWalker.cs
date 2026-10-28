using System.Collections.Generic;
using UnityEngine;

namespace CraftingOverhaul.Biomes
{
    /// <summary>
    ///     Walks a world prefab (vegetation, creature, location, dungeon room) and reports everything a player can
    ///     get from it: pickables, drop tables, tree logs, mined rock, creature drops, boss altars and traders.
    ///     Requirements picked up along the way (tool tier, summoning item, global key) are carried down the chain.
    ///
    ///     Each game component is handled in its own method, so rules can be inspected or changed one at a time.
    /// </summary>
    internal sealed class PrefabWalker
    {
        private const int MaxDepth = 8;

        /// <summary>Everything found, as sources without a base biome. The caller sets the biome and collector.</summary>
        public readonly List<ItemSource> Found = new List<ItemSource>();

        private readonly HashSet<GameObject> _visited = new HashSet<GameObject>();

        private sealed class Context
        {
            public ToolRequirement Tool = ToolRequirement.None;
            public readonly List<string> RequiredItems = new List<string>();
            public string RequiredKey;
            public bool IsWeak;
            public string Via;

            public Context With(string via)
            {
                var next = new Context { Tool = Tool, RequiredKey = RequiredKey, IsWeak = IsWeak, Via = Via + " > " + via };
                next.RequiredItems.AddRange(RequiredItems);
                return next;
            }
        }

        public static List<ItemSource> Walk(GameObject prefab)
        {
            var walker = new PrefabWalker();
            walker.Visit(prefab, new Context { Via = prefab ? prefab.name : "?" }, 0);
            return walker.Found;
        }

        private void Visit(GameObject go, Context ctx, int depth)
        {
            if (!go || depth > MaxDepth || !_visited.Add(go))
            {
                return;
            }

            // Spawned prefabs that are items themselves (fish, dropped items).
            if (go.GetComponent<ItemDrop>())
            {
                Yield(go, ctx);
            }

            foreach (var c in go.GetComponentsInChildren<Pickable>(true)) VisitPickable(c, ctx);
            foreach (var c in go.GetComponentsInChildren<PickableItem>(true)) VisitPickableItem(c, ctx);
            foreach (var c in go.GetComponentsInChildren<DropOnDestroyed>(true)) VisitTable(c.m_dropWhenDestroyed, ctx.With("DropOnDestroyed"), depth);
            foreach (var c in go.GetComponentsInChildren<MineRock>(true)) VisitMineRock(c.m_dropItems, c.m_minToolTier, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<MineRock5>(true)) VisitMineRock(c.m_dropItems, c.m_minToolTier, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<TreeBase>(true)) VisitTree(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<TreeLog>(true)) VisitLog(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<Destructible>(true)) VisitDestructible(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<Container>(true)) VisitContainer(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<CharacterDrop>(true)) VisitCharacterDrop(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<Character>(true)) VisitCharacter(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<CreatureSpawner>(true)) Visit(c.m_creaturePrefab, ctx.With("CreatureSpawner"), depth + 1);
            foreach (var c in go.GetComponentsInChildren<SpawnArea>(true)) VisitSpawnArea(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<OfferingBowl>(true)) VisitOfferingBowl(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<Trader>(true)) VisitTrader(c, ctx);
            foreach (var c in go.GetComponentsInChildren<Procreation>(true)) Visit(c.m_offspring, ctx.With("Offspring"), depth + 1);
            foreach (var c in go.GetComponentsInChildren<Growup>(true)) VisitGrowup(c, ctx, depth);
            foreach (var c in go.GetComponentsInChildren<WispSpawner>(true)) Visit(c.m_wispPrefab, ctx.With("WispSpawner"), depth + 1);
        }

        private void VisitPickable(Pickable pickable, Context ctx)
        {
            var next = ctx.With("Pickable");
            Yield(pickable.m_itemPrefab, next);
            VisitTableItems(pickable.m_extraDrops, next);
        }

        private void VisitPickableItem(PickableItem pickable, Context ctx)
        {
            var next = ctx.With("PickableItem");
            if (pickable.m_itemPrefab)
            {
                Yield(pickable.m_itemPrefab.gameObject, next);
            }
            if (pickable.m_randomItemPrefabs != null)
            {
                foreach (var random in pickable.m_randomItemPrefabs)
                {
                    if (random.m_itemPrefab)
                    {
                        Yield(random.m_itemPrefab.gameObject, next);
                    }
                }
            }
        }

        private void VisitMineRock(DropTable table, int minToolTier, Context ctx, int depth)
        {
            var next = ctx.With("MineRock");
            next.Tool = next.Tool.Max(minToolTier, ToolKind.Pickaxe);
            VisitTable(table, next, depth);
        }

        private void VisitTree(TreeBase tree, Context ctx, int depth)
        {
            var next = ctx.With("Tree");
            next.Tool = next.Tool.Max(tree.m_minToolTier, ToolKind.Chop);
            VisitTable(tree.m_dropWhenDestroyed, next, depth);
            Visit(tree.m_logPrefab, next, depth + 1);
        }

        private void VisitLog(TreeLog log, Context ctx, int depth)
        {
            var next = ctx.With("Log");
            next.Tool = next.Tool.Max(log.m_minToolTier, ToolKind.Chop);
            VisitTable(log.m_dropWhenDestroyed, next, depth);
            Visit(log.m_subLogPrefab, next, depth + 1);
        }

        private void VisitDestructible(Destructible destructible, Context ctx, int depth)
        {
            if (!destructible.m_spawnWhenDestroyed)
            {
                return;
            }
            var next = ctx.With("Destructible");
            next.Tool = next.Tool.Max(destructible.m_minToolTier, ToolKind.Any);
            Visit(destructible.m_spawnWhenDestroyed, next, depth + 1);
        }

        /// <summary>Random chest loot is weak evidence (see <see cref="ItemSource.IsWeak" />).</summary>
        private void VisitContainer(Container container, Context ctx, int depth)
        {
            var next = ctx.With("Container");
            next.IsWeak = true;
            VisitTable(container.m_defaultItems, next, depth);
        }

        private void VisitCharacterDrop(CharacterDrop drop, Context ctx, int depth)
        {
            if (drop.m_drops == null)
            {
                return;
            }
            var next = ctx.With("CharacterDrop");
            foreach (var d in drop.m_drops)
            {
                VisitDropped(d.m_prefab, next, depth);
            }
        }

        /// <summary>Killing a boss sets its global key; record which biome and summoning items that takes.</summary>
        private void VisitCharacter(Character character, Context ctx, int depth)
        {
            // Things spawned on death, e.g. a boss's next phase.
            if (character.m_deathEffects?.m_effectPrefabs != null)
            {
                foreach (var effect in character.m_deathEffects.m_effectPrefabs)
                {
                    if (effect != null && effect.m_enabled && effect.m_prefab)
                    {
                        Visit(effect.m_prefab, ctx.With("On death"), depth + 1);
                    }
                }
            }

            if (!character.m_boss && character is Humanoid humanoid)
            {
                VisitCreatureAttacks(humanoid, ctx);
            }

            if (string.IsNullOrEmpty(character.m_defeatSetGlobalKey))
            {
                return;
            }
            var source = MakeSource(ctx.With("Defeat " + character.name));
            source.Target = SourceTarget.GlobalKey;
            source.Name = character.m_defeatSetGlobalKey;
            Found.Add(source);
        }

        /// <summary>
        ///     A creature's attacks are items with a tool tier. Some resources can only be broken by them
        ///     (no player pickaxe reaches the tier), so they count as tools available where the creature lives.
        ///     Bosses are skipped: their attacks are not a practical way to harvest anything.
        /// </summary>
        private void VisitCreatureAttacks(Humanoid humanoid, Context ctx)
        {
            var next = ctx.With("Creature attack " + humanoid.name);
            YieldAttacks(humanoid.m_defaultItems, next);
            YieldAttacks(humanoid.m_randomWeapon, next);
            if (humanoid.m_randomSets != null)
            {
                foreach (var set in humanoid.m_randomSets)
                {
                    YieldAttacks(set?.m_items, next);
                }
            }
        }

        private void YieldAttacks(GameObject[] items, Context ctx)
        {
            if (items == null)
            {
                return;
            }
            foreach (var item in items)
            {
                var drop = item ? item.GetComponent<ItemDrop>() : null;
                var damages = drop ? drop.m_itemData.m_shared.m_damages : default(HitData.DamageTypes);
                if (drop && drop.m_itemData.m_shared.m_toolTier > 0 && (damages.m_pickaxe > 0f || damages.m_chop > 0f))
                {
                    Yield(item, ctx);
                }
            }
        }

        private void VisitGrowup(Growup growup, Context ctx, int depth)
        {
            var next = ctx.With("Grows up");
            Visit(growup.m_grownPrefab, next, depth + 1);
            if (growup.m_altGrownPrefabs != null)
            {
                foreach (var alt in growup.m_altGrownPrefabs)
                {
                    Visit(alt?.m_prefab, next, depth + 1);
                }
            }
        }

        private void VisitSpawnArea(SpawnArea area, Context ctx, int depth)
        {
            if (area.m_prefabs == null)
            {
                return;
            }
            var next = ctx.With("SpawnArea");
            foreach (var spawn in area.m_prefabs)
            {
                Visit(spawn.m_prefab, next, depth + 1);
            }
        }

        /// <summary>Boss altars: the boss (and anything the bowl gives) requires the summoning item.</summary>
        private void VisitOfferingBowl(OfferingBowl bowl, Context ctx, int depth)
        {
            var next = ctx.With("OfferingBowl");
            if (bowl.m_bossItem)
            {
                next.RequiredItems.Add(bowl.m_bossItem.gameObject.name);
            }
            Visit(bowl.m_bossPrefab, next, depth + 1);
            if (bowl.m_itemPrefab)
            {
                Yield(bowl.m_itemPrefab.gameObject, next);
            }
        }

        /// <summary>Trader stock is available where the trader is, once its required global key is set.</summary>
        private void VisitTrader(Trader trader, Context ctx)
        {
            // Quest hand-ins: giving the trader an item sets a global key (e.g. Hildir's chests).
            if (trader.m_useItems != null)
            {
                foreach (var use in trader.m_useItems)
                {
                    if (use == null || !use.m_prefab || string.IsNullOrEmpty(use.m_setsGlobalKey))
                    {
                        continue;
                    }
                    var source = MakeSource(ctx.With("Hand in to " + trader.name));
                    source.Target = SourceTarget.GlobalKey;
                    source.Name = use.m_setsGlobalKey;
                    source.RequiredItems.Add(use.m_prefab.gameObject.name);
                    Found.Add(source);
                }
            }

            if (trader.m_items == null)
            {
                return;
            }
            foreach (var item in trader.m_items)
            {
                if (!item.m_prefab)
                {
                    continue;
                }
                var next = ctx.With("Trader " + trader.name);
                if (!string.IsNullOrEmpty(item.m_requiredGlobalKey))
                {
                    next.RequiredKey = item.m_requiredGlobalKey;
                }
                Yield(item.m_prefab.gameObject, next);
            }
        }

        private void VisitTable(DropTable table, Context ctx, int depth)
        {
            if (table?.m_drops == null)
            {
                return;
            }
            foreach (var d in table.m_drops)
            {
                VisitDropped(d.m_item, ctx, depth);
            }
        }

        /// <summary>Drop tables without recursion, for things like a pickable's bonus drops.</summary>
        private void VisitTableItems(DropTable table, Context ctx)
        {
            if (table?.m_drops == null)
            {
                return;
            }
            foreach (var d in table.m_drops)
            {
                Yield(d.m_item, ctx);
            }
        }

        /// <summary>A dropped prefab is usually an item; otherwise it is something that spawns (e.g. a creature).</summary>
        private void VisitDropped(GameObject prefab, Context ctx, int depth)
        {
            if (!prefab)
            {
                return;
            }
            if (prefab.GetComponent<ItemDrop>())
            {
                Yield(prefab, ctx);
            }
            else
            {
                Visit(prefab, ctx, depth + 1);
            }
        }

        private void Yield(GameObject prefab, Context ctx)
        {
            if (!prefab || !prefab.GetComponent<ItemDrop>())
            {
                return;
            }
            var source = MakeSource(ctx);
            source.Name = prefab.name;
            Found.Add(source);
        }

        private static ItemSource MakeSource(Context ctx)
        {
            var source = new ItemSource
            {
                Tool = ctx.Tool,
                RequiredKey = ctx.RequiredKey,
                IsWeak = ctx.IsWeak,
                Origin = ctx.Via
            };
            source.RequiredItems.AddRange(ctx.RequiredItems);
            return source;
        }
    }
}
