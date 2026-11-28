using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     What a rip's loot affixes add to its drops ([#96](https://github.com/krogon-games/JotunnModStub/issues/96), ROADMAP
    ///     R1c; Krogon's decisions of 2026-11-28 on <c>claude/nifty-hypatia-afm8ez</c>). Each affix's value is a percent more
    ///     (100 = x2, map affix overhaul decision 7), never a guarantee:
    ///     <list type="bullet">
    ///         <item><c>drop_items</c> (Bountiful): Corruption's own drops only, gear of every rarity, orbs and maps (decision 3).</item>
    ///         <item><c>drop_orbs</c> (Glittering): every orb's chance.</item>
    ///         <item>
    ///             <c>drop_shields</c>, <c>drop_armour</c>, <c>drop_melee</c>, <c>drop_ranged</c>: tilt which item a gear drop
    ///             becomes (decision 5): an item of that kind weighs (1 + v/100) in the pick; the number of drops is unchanged.
    ///         </item>
    ///     </list>
    ///     Everything multiplies (decision 4): with the tier and the stars, and with other loot affixes on the same map. The
    ///     same factors apply to kills and to the heart's spill (decision 8). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class MapLootBoost
    {
        /// <summary>No loot affixes: every factor x1.</summary>
        public static readonly MapLootBoost None = new MapLootBoost();

        /// <summary>Factor on the gear rolls (magic, rare, unique) and on the map roll.</summary>
        public double Items = 1.0;

        /// <summary>Factor on every orb's chance (Bountiful times Glittering).</summary>
        public double Orbs = 1.0;

        public double Shield = 1.0;
        public double Armour = 1.0;
        public double Melee = 1.0;
        public double Ranged = 1.0;

        /// <summary>Whether any kind is tilted (the gear pick then needs weights).</summary>
        public bool TiltsPick => Shield != 1.0 || Armour != 1.0 || Melee != 1.0 || Ranged != 1.0;

        /// <summary>The pick weight of an item of this kind (<see cref="MapLootRules.KindOf" />); 1 for the rest.</summary>
        public double WeightOf(string kind)
        {
            switch (kind)
            {
                case MapLootRules.KindShield: return Shield;
                case MapLootRules.KindArmour: return Armour;
                case MapLootRules.KindMelee: return Melee;
                case MapLootRules.KindRanged: return Ranged;
                default: return 1.0;
            }
        }
    }

    internal static class MapLootRules
    {
        public const string KindShield = "shield";
        public const string KindArmour = "armour";
        public const string KindMelee = "melee";
        public const string KindRanged = "ranged";

        /// <summary>The worn armour classes of <c>loot_bases.csv</c> (decision 6: helmet, chest, legs, cape; backpacks are capes; not jewellery).</summary>
        private static readonly string[] ArmourClasses = { "helmet", "chest", "legs", "cape" };

        /// <summary>The game's skills of ranged weapons (decision 7: bows, crossbows and magic staffs).</summary>
        private static readonly string[] RangedSkills = { "Bows", "Crossbows", "ElementalMagic", "BloodMagic" };

        /// <summary>The weapon classes of <c>loot_bases.csv</c> (pickaxes count as melee, decision 7).</summary>
        private static readonly string[] WeaponClasses = { "weapon", "pickaxe" };

        /// <summary>
        ///     The loot affixes' kind of a loot base: <see cref="KindShield" />, <see cref="KindArmour" />,
        ///     <see cref="KindMelee" />, <see cref="KindRanged" />, or "" (jewellery). <paramref name="skill" /> is the
        ///     game's skill type name (<c>Skills.SkillType</c>), which tells ranged weapons from melee ones.
        /// </summary>
        public static string KindOf(IEnumerable<string> classes, string skill)
        {
            var list = (classes ?? Enumerable.Empty<string>()).ToList();
            if (list.Contains("shield"))
            {
                return KindShield;
            }
            if (list.Any(c => ArmourClasses.Contains(c)))
            {
                return KindArmour;
            }
            if (list.Any(c => WeaponClasses.Contains(c)))
            {
                return RangedSkills.Contains(skill ?? "") ? KindRanged : KindMelee;
            }
            return "";
        }

        /// <summary>The map's loot factors from its affixes (each value v gives x(1 + v/100); several multiply).</summary>
        public static MapLootBoost Of(IEnumerable<MapAffixRoll> rolls)
        {
            var list = (rolls ?? Enumerable.Empty<MapAffixRoll>()).Where(r => r != null && r.Affix != null).ToList();
            double Product(string effect) => list.Where(r => r.Affix.Effect == effect).Aggregate(1.0, (p, r) => p * (1 + r.Value / 100.0));
            var boost = new MapLootBoost
            {
                Items = Product(MapAffixTable.Effects.DropItems),
                Shield = Product(MapAffixTable.Effects.DropShields),
                Armour = Product(MapAffixTable.Effects.DropArmour),
                Melee = Product(MapAffixTable.Effects.DropMelee),
                Ranged = Product(MapAffixTable.Effects.DropRanged)
            };
            boost.Orbs = boost.Items * Product(MapAffixTable.Effects.DropOrbs);
            return boost;
        }
    }
}
