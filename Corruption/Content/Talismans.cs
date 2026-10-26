using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Corruption.Rules;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Trophy talismans (Ægishjálmur charms): one per creature family, worked out from game data. Worn in the
    ///     vanilla trinket slot; the matching creatures won't target or attack the wearer (see <see cref="TalismanRepel" />).
    /// </summary>
    internal static class Talismans
    {
        public const string PrefabPrefix = "CorruptionTalisman_";

        // A vanilla trinket, so equipping, the slot and the worn model all work as in vanilla. Its bonuses are removed.
        private const string BasePrefab = "TrinketBronzeHealth";

        /// <summary>Talisman prefab hash → prefab hashes of the creatures it repels.</summary>
        public static readonly Dictionary<int, HashSet<int>> RepelledBy = new Dictionary<int, HashSet<int>>();

        public static void Register()
        {
            var creatures = CollectCreatures(out var characters);
            var families = TalismanFamilies.Build(creatures,
                TalismanFamilies.Excluder(CorruptionConfig.TalismanExcludedCreatures.Value));

            foreach (var family in families)
            {
                if (!PrefabManager.Instance.GetPrefab(family.Trophy))
                {
                    continue;
                }
                Safe.Run("register the " + family.Primary + " talisman", () => RegisterTalisman(family, characters));
            }

            Jotunn.Logger.LogInfo($"Registered {RepelledBy.Count} talismans: " +
                                  string.Join("; ", families.Select(f => f.Primary + " (" + string.Join(", ", f.Members) + ")")));
        }

        private static List<TalismanFamilies.Creature> CollectCreatures(out Dictionary<string, Character> characters)
        {
            characters = new Dictionary<string, Character>();
            var creatures = new List<TalismanFamilies.Creature>();
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(Character)).Values)
            {
                var character = obj as Character;
                if (!character || character is Player || characters.ContainsKey(character.gameObject.name))
                {
                    continue;
                }
                characters[character.gameObject.name] = character;

                var creature = new TalismanFamilies.Creature
                {
                    Prefab = character.gameObject.name,
                    IsBoss = character.m_boss,
                    // Monster AI = creatures that can hunt the player; deer and hares use AnimalAI.
                    IsMonster = character.GetComponent<MonsterAI>()
                };
                var drops = character.GetComponent<CharacterDrop>();
                if (drops?.m_drops != null)
                {
                    foreach (var drop in drops.m_drops)
                    {
                        var item = drop.m_prefab ? drop.m_prefab.GetComponent<ItemDrop>() : null;
                        if (item && item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy)
                        {
                            creature.Trophies.Add(drop.m_prefab.name);
                        }
                    }
                }
                creatures.Add(creature);
            }
            return creatures;
        }

        private static void RegisterTalisman(TalismanFamilies.Family family, Dictionary<string, Character> characters)
        {
            string creatureName = Localization.instance.Localize(characters[family.Primary].m_name);
            string prefabName = PrefabPrefix + family.Primary;

            var item = new CustomItem(prefabName, BasePrefab, new ItemConfig
            {
                // Plain text rather than tokens: the names are built from the creature's localized name.
                Name = $"{creatureName} charm",
                Description = $"An Ægishjálmur, the Helm of Awe, carved around three {creatureName} trophies. " +
                              $"{creatureName}s and their kin will not attack you while you wear it, unless you strike first.",
                CraftingStation = CraftingStations.Workbench,
                Requirements = new[]
                {
                    new RequirementConfig(family.Trophy, 3),
                    new RequirementConfig("Chain", 1),
                    new RequirementConfig("LeatherScraps", 10)
                }
            });

            StripBonuses(item.ItemDrop.m_itemData.m_shared);

            // The trophy's icon shows at a glance which creatures the charm repels.
            var trophy = PrefabManager.Instance.GetPrefab(family.Trophy).GetComponent<ItemDrop>();
            if (trophy && trophy.m_itemData.m_shared.m_icons?.Length > 0)
            {
                item.ItemDrop.m_itemData.m_shared.m_icons = new[] { trophy.m_itemData.m_shared.m_icons[0] };
            }
            if (ColorUtility.TryParseHtmlString(CorruptionConfig.ShardColor.Value, out var color))
            {
                Corrupted.Tint(item.ItemPrefab, color, 0.5f);
            }

            ItemManager.Instance.AddItem(item);

            RepelledBy[prefabName.GetStableHashCode()] =
                new HashSet<int>(family.Members.Select(m => m.GetStableHashCode()));
        }

        /// <summary>The base trinket's own effects (health, adrenaline power, resistances) don't belong on a charm.</summary>
        private static void StripBonuses(ItemDrop.ItemData.SharedData shared)
        {
            foreach (FieldInfo field in Player.s_equipmentModifierSourceFields ?? new FieldInfo[0])
            {
                if (field.FieldType == typeof(float) && field.DeclaringType == typeof(ItemDrop.ItemData.SharedData))
                {
                    field.SetValue(shared, 0f);
                }
            }
            shared.m_fullAdrenalineSE = null;
            shared.m_equipStatusEffect = null;
            shared.m_setStatusEffect = null;
            shared.m_setName = "";
            shared.m_damageModifiers = new List<HitData.DamageModPair>();
            shared.m_armor = 0f;
            shared.m_armorPerLevel = 0f;
        }
    }
}
