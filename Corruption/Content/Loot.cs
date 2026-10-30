using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Corruption.Rules;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The loot system's data (affixes, base items, loot rules, drop chances; all compiled-in tables under
    ///     <c>Corruption/Data/</c>) and the bridge from game items to their rolled affixes. Design: corruption-design.md →
    ///     Loot and uniques; rules: AFFIX_RULES.md.
    /// </summary>
    internal static class Loot
    {
        public const string MagicColor = "#8888FF";
        public const string RareColor = "#FFFF77";

        public static Dictionary<string, AffixTable.Affix> Affixes = new Dictionary<string, AffixTable.Affix>();
        public static LootBases Bases;
        public static LootRules Rules;
        public static Corruption.Rules.DropTable Drops;

        /// <summary>Affix effects this build applies. Lines with other effects show in tooltips as not active yet.</summary>
        public static readonly HashSet<string> ActiveEffects = new HashSet<string>
        {
            "weapon_damage", "fire_damage_percent", "frost_damage_percent", "lightning_damage_percent", "poison_damage_percent",
            "spirit_damage_percent", "armour_percent", "max_durability", "block_percent", "attack_stamina_reduction",
            "knockback_percent", "backstab_percent", "parry_percent"
        };

        private sealed class Cached
        {
            public string Raw;
            public LootItem Item;
        }

        // Stat getters run often (every tooltip, every hit), so each item's parsed affixes are kept until its data changes.
        private static readonly ConditionalWeakTable<ItemDrop.ItemData, Cached> Cache = new ConditionalWeakTable<ItemDrop.ItemData, Cached>();

        /// <summary>While true, the stat patches return vanilla numbers (used to work out an affix's bonus for tooltips).</summary>
        [System.ThreadStatic] public static bool Bypass;

        public static bool Loaded => Bases != null && Rules != null && Drops != null && Affixes.Count > 0;

        /// <summary>Reads the tables. Problems are logged and switch loot off rather than rolling from broken data.</summary>
        public static void Load()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFiles.Read("affixes.csv"), errors);
            var bases = LootBases.Parse(DataFiles.Read("loot_bases.csv"), errors);
            var rules = LootRules.Parse(DataFiles.Read("loot_rules.csv"), DataFiles.Read("rare_names.csv"), errors);
            var drops = Corruption.Rules.DropTable.Parse(DataFiles.Read("drop_chances.csv"), DataFiles.Read("drop_modifiers.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("Loot tables have problems, so loot is off:\n" + string.Join("\n", errors));
                return;
            }
            Affixes = affixes.ToDictionary(a => a.Id);
            Bases = bases;
            Rules = rules;
            Drops = drops;
            Jotunn.Logger.LogInfo($"Loot: {Affixes.Count} affixes, {Bases.Bases.Count} base items.");
        }

        /// <summary>The item's rolled affixes, or null for a Normal item.</summary>
        public static LootItem Of(ItemDrop.ItemData item)
        {
            if (item == null || item.m_customData == null || !item.m_customData.TryGetValue(LootItem.AffixesKey, out var raw))
            {
                return null;
            }
            var cached = Cache.GetOrCreateValue(item);
            if (cached.Raw != raw)
            {
                cached.Raw = raw;
                cached.Item = LootItem.Read(item.m_customData);
            }
            return cached.Item;
        }

        /// <summary>Total of one affix effect on the item (0 for Normal items, or while <see cref="Bypass" /> is on).</summary>
        public static int Total(ItemDrop.ItemData item, string effect)
        {
            var loot = Bypass ? null : Of(item);
            return loot == null ? 0 : loot.Total(effect, Affixes);
        }

        public static string ColorFor(string rarity) => rarity == "rare" ? RareColor : MagicColor;

        /// <summary>The item's name in its rarity colour, or null for a Normal item.</summary>
        public static string ColoredName(ItemDrop.ItemData item)
        {
            var loot = Of(item);
            if (loot == null)
            {
                return null;
            }
            string baseName = Localization.instance.Localize(item.m_shared.m_name);
            return $"<color={ColorFor(loot.Rarity)}>{loot.DisplayName(baseName, Affixes)}</color>";
        }

        /// <summary>
        ///     Makes a new Magic or Rare item: a random base item for the biome, then its affixes. Null when the biome has
        ///     no loot (the Ocean) or nothing fits.
        /// </summary>
        public static ItemDrop.ItemData Create(string rarity, string biome, string prefabOverride = null)
        {
            if (!Loaded || !ObjectDB.instance)
            {
                return null;
            }
            GameObject prefab;
            List<string> classes;
            if (prefabOverride != null)
            {
                prefab = ObjectDB.instance.GetItemPrefab(prefabOverride);
                var row = Bases.Bases.FirstOrDefault(b => b.Prefab == prefabOverride);
                if (!prefab || row == null)
                {
                    return null;
                }
                classes = row.Classes;
            }
            else
            {
                var pick = Bases.Pick(biome, Rules.PreviousBiomePercent, p => ObjectDB.instance.GetItemPrefab(p), () => Random.value * 0.99999f);
                if (pick == null)
                {
                    return null;
                }
                prefab = ObjectDB.instance.GetItemPrefab(pick.Prefab);
                classes = pick.Classes;
            }

            var template = prefab.GetComponent<ItemDrop>();
            if (!template)
            {
                return null;
            }
            var item = template.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = 1;
            item.m_quality = Mathf.Clamp(Rules.DropQuality, 1, Mathf.Max(1, item.m_shared.m_maxQuality));
            item.m_worldLevel = (byte)Game.m_worldLevel;

            var shared = item.m_shared;
            var mechanics = LootBases.Mechanics(shared.m_skillType.ToString(), shared.m_attack != null ? shared.m_attack.m_attackChainLevels : 0,
                shared.m_blockPower);
            System.Func<double> random = () => Random.value * 0.99999f;
            var loot = AffixRoller.Roll(rarity, Rules.AffixCount(rarity, random), Affixes.Values, classes, mechanics, random);
            if (loot.Lines.Count == 0)
            {
                return null;
            }
            if (rarity == "rare")
            {
                loot.Name = Rules.RareName(classes, random);
            }
            loot.Write(item.m_customData);
            item.m_durability = item.GetMaxDurability();
            return item;
        }

        /// <summary>Drops a new loot item at a position, like a creature drop.</summary>
        public static ItemDrop Drop(ItemDrop.ItemData item, Vector3 position)
        {
            var offset = Random.insideUnitSphere * 0.5f;
            var drop = ItemDrop.DropItem(item, 1, position + offset, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
            var body = drop ? drop.GetComponent<Rigidbody>() : null;
            if (body)
            {
                var push = Random.insideUnitSphere;
                push.y = Mathf.Abs(push.y);
                body.AddForce(push * 5f, ForceMode.VelocityChange);
            }
            return drop;
        }

        /// <summary>The biome as the loot tables name it (the game's enum spells the Ashlands "AshLands").</summary>
        public static string BiomeAt(Vector3 position)
        {
            if (WorldGenerator.instance == null)
            {
                return "";
            }
            return WorldGenerator.instance.GetBiome(position).ToString();
        }

        public static void Register()
        {
            CommandManager.Instance.AddConsoleCommand(new LootCommand());
        }

        /// <summary>co_loot: spawns a rolled item in front of the player, for testing.</summary>
        private sealed class LootCommand : ConsoleCommand
        {
            public override string Name => "co_loot";

            public override string Help =>
                "Corruption: co_loot [magic|rare] [item prefab] - drop a rolled Magic or Rare item (default: rare, a random item of the biome you stand in)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player || !Loaded)
                {
                    Console.instance.Print(Loaded ? "co_loot needs a loaded world." : "Loot tables didn't load; see the log.");
                    return;
                }
                string rarity = args.Length > 0 && args[0].ToLowerInvariant() == "magic" ? "magic" : "rare";
                string prefab = args.Length > 1 ? args[1] : null;
                string biome = BiomeAt(player.transform.position);
                var item = Create(rarity, biome, prefab);
                if (item == null)
                {
                    Console.instance.Print(prefab != null
                        ? $"{prefab} isn't a loot base item (see Corruption/Data/loot_bases.csv)."
                        : $"No loot items for the biome here ({biome}).");
                    return;
                }
                Drop(item, player.transform.position + player.transform.forward * 1.5f + Vector3.up);
                Console.instance.Print($"Dropped {ColoredName(item)}.");
            }
        }
    }
}
