using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     The on-harvest chances a gathering tool carries (affixes.csv effects), sent from the striker to the object's
    ///     owner with each hit. Percent values; 0 = the tool doesn't have the affix.
    /// </summary>
    internal sealed class HarvestChances
    {
        /// <summary>Bountiful (universal), <c>harvest_double_chance</c>: chance the harvest's drops all drop once more.</summary>
        public float Bounty;

        /// <summary>Bountiful (woodcutting), <c>wood_double_chance</c>: chance the harvest's wood drops once more.</summary>
        public float WoodBounty;

        /// <summary>Determined, <c>stamina_percent_on_harvest</c>: % of max stamina restored per harvest (not a chance).</summary>
        public float Stamina;

        /// <summary>Artisan, <c>extra_ore</c>.</summary>
        public float Ore;

        /// <summary>Gemseeker's, <c>gem_find</c>.</summary>
        public float Gem;

        /// <summary>Stonecutter's, <c>extra_stone</c>: rolls per damaging mining hit, not per harvest (<see cref="HarvestRules.PerHit{T}" />).</summary>
        public float Stone;

        /// <summary>Resinous, <c>extra_resin</c>: rolls per damaging woodcutting hit, not per harvest (<see cref="HarvestRules.PerHit{T}" />).</summary>
        public float Resin;

        /// <summary>Forester's, <c>tree_seed_drop</c>.</summary>
        public float Seed;

        /// <summary>Charring, <c>wood_to_coal</c>: chance per wood item that it drops as coal instead.</summary>
        public float Charring;

        /// <summary>Odin's Grace on a tree, rolled by the striker: this strike harvests the whole tree.</summary>
        public bool Odin;

        /// <summary>The striker's peer, which Determined's stamina goes to (set by the owner when the message arrives).</summary>
        public long Striker;

        public bool Any =>
            Odin || Bounty > 0f || WoodBounty > 0f || Stamina > 0f || Ore > 0f || Gem > 0f || Stone > 0f || Resin > 0f ||
            Seed > 0f || Charring > 0f;

        /// <summary>A copy without the one-strike Odin's Grace flag (for a deposit that replaces a broken rock).</summary>
        public HarvestChances WithoutOdin()
        {
            var copy = (HarvestChances)MemberwiseClone();
            copy.Odin = false;
            return copy;
        }
    }

    /// <summary>The items a harvest's drop list is checked against and extended with.</summary>
    /// <typeparam name="T">The game's item prefab (a GameObject), or anything in tests.</typeparam>
    internal sealed class HarvestItems<T> where T : class
    {
        /// <summary>Wood: what fallen logs and stumps drop (derived from the game's trees).</summary>
        public Func<T, bool> IsWood = _ => false;

        /// <summary>Ore: what the coal-fired smelters take (derived from the game's smelters).</summary>
        public Func<T, bool> IsOre = _ => false;

        /// <summary>The seed in this tree's own drop table, or null when it has none.</summary>
        public T Seed;

        public T Stone;
        public T Resin;
        public T Coal;

        /// <summary>Gemseeker's pool for this deposit's biome (<see cref="HarvestRules.GemPool" />).</summary>
        public IList<T> Gems = new List<T>();
    }

    /// <summary>
    ///     What a harvest drops with the on-harvest gathering affixes (AFFIX_RULES.md → Gathering tool affixes →
    ///     Interpretations). One harvest event = one object or deposit part that drops resources when it breaks; every
    ///     chance rolls once per event. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class HarvestRules
    {
        /// <summary>
        ///     Biomes in progression order, the order of CraftingOverhaul's biome detection (<c>Biomes/Biome.cs</c>),
        ///     which AFFIX_RULES names for Gemseeker's ("items whose biome (biome detection) is no later than the
        ///     deposit's"). The Ocean sits after the Swamp there.
        /// </summary>
        public static readonly string[] BiomeOrder =
            { "Meadows", "BlackForest", "Swamp", "Ocean", "Mountain", "Plains", "Mistlands", "Ashlands", "DeepNorth" };

        /// <summary>
        ///     Gemseeker's pool (user, 2026-10-28): the trader valuables Amber, Amber pearl and Ruby. Their biomes come
        ///     from the 1.0.20 dump (<c>data/game-dump-1.0.20/biome_items.csv</c>): Amber and AmberPearl Meadows, Ruby
        ///     BlackForest. Parallel arrays: tuples would need System.ValueTuple.
        /// </summary>
        public static readonly string[] GemItems = { "Amber", "AmberPearl", "Ruby" };

        public static readonly string[] GemBiomes = { "Meadows", "Meadows", "BlackForest" };

        /// <summary>A biome's place in <see cref="BiomeOrder" /> (case-insensitive: the game spells it "AshLands"), or -1.</summary>
        public static int BiomeIndex(string biome)
        {
            return Array.FindIndex(BiomeOrder, b => string.Equals(b, biome, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Gemseeker's gems a deposit in <paramref name="depositBiome" /> can give: those whose biome is no later than
        ///     the deposit's. Empty for a biome not on the list.
        /// </summary>
        public static List<string> GemPool(string depositBiome)
        {
            var pool = new List<string>();
            int deposit = BiomeIndex(depositBiome);
            if (deposit < 0)
            {
                return pool;
            }
            for (int i = 0; i < GemItems.Length; i++)
            {
                int gem = BiomeIndex(GemBiomes[i]);
                if (gem >= 0 && gem <= deposit)
                {
                    pool.Add(GemItems[i]);
                }
            }
            return pool;
        }

        /// <summary>
        ///     The drop list of one harvest event with the tool's on-harvest chances applied. Vanilla's own list is
        ///     <paramref name="drops" />; an empty list isn't a harvest and comes back unchanged.
        ///     <list type="bullet">
        ///         <item>Bountiful: everything in the vanilla list once more. Woodcutting Bountiful: its wood once more. Both can trigger (triple wood).</item>
        ///         <item>Mining (rocks and ore): Artisan adds 1 of the first ore that dropped, Gemseeker's 1 gem from the biome's pool (equally likely).</item>
        ///         <item>Woodcutting (trees, logs, stumps): Forester's adds 1 of the tree's own seed.</item>
        ///         <item>Charring last: each wood item in the result rolls on its own and becomes coal (replaces the wood, user's choice).</item>
        ///     </list>
        ///     Stonecutter's and Resinous don't roll here: they roll on every damaging hit (<see cref="PerHit{T}" />).
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static List<T> Apply<T>(IList<T> drops, HarvestChances chances, bool woodcutting, HarvestItems<T> items, Func<double> random)
            where T : class
        {
            var result = new List<T>(drops);
            if (drops.Count == 0 || chances == null)
            {
                return result;
            }
            if (GatheringRules.Roll(chances.Bounty, random))
            {
                result.AddRange(drops);
            }
            if (GatheringRules.Roll(chances.WoodBounty, random))
            {
                foreach (var item in drops)
                {
                    if (items.IsWood(item))
                    {
                        result.Add(item);
                    }
                }
            }
            if (!woodcutting)
            {
                if (GatheringRules.Roll(chances.Ore, random))
                {
                    foreach (var item in drops)
                    {
                        if (items.IsOre(item))
                        {
                            result.Add(item);
                            break;
                        }
                    }
                }
                if (items.Gems.Count > 0 && GatheringRules.Roll(chances.Gem, random))
                {
                    int pick = Math.Min(items.Gems.Count - 1, (int)(random() * items.Gems.Count));
                    result.Add(items.Gems[pick]);
                }
            }
            else
            {
                if (items.Seed != null && GatheringRules.Roll(chances.Seed, random))
                {
                    result.Add(items.Seed);
                }
            }
            if (items.Coal != null && chances.Charring > 0f)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    if (items.IsWood(result[i]) && GatheringRules.Roll(chances.Charring, random))
                    {
                        result[i] = items.Coal;
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Stonecutter's and Resinous (Krogon, 2026-11-08): a chance on every valid gathering hit of one stone (mining)
        ///     or one resin (woodcutting), whether or not the object breaks and whether or not it drops stone or resin
        ///     itself. A valid hit is one made with the right tool (pickaxe damage on rock, chop damage on wood) that does
        ///     damage, i.e. passes the object's tool tier and does more than 0 after its resistances. Null: nothing extra.
        /// </summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static T PerHit<T>(HarvestChances chances, bool woodcutting, bool rightTool, bool didDamage, HarvestItems<T> items,
            Func<double> random) where T : class
        {
            if (chances == null || !rightTool || !didDamage)
            {
                return null;
            }
            float chance = woodcutting ? chances.Resin : chances.Stone;
            T item = woodcutting ? items.Resin : items.Stone;
            return item != null && chance > 0f && GatheringRules.Roll(chance, random) ? item : null;
        }
    }
}
