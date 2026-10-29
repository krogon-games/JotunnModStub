using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Magic and Rare drops from kills. Runs on the machine that simulates the creature, where vanilla drops its loot.
    ///     Personal loot (design): every player within the game's own "nearby players" range (the range it counts
    ///     players in to scale creature difficulty, <c>Game.m_difficultyScaleRange</c>) rolls separately.
    /// </summary>
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.OnDeath))]
    internal static class LootDrops
    {
        private static readonly List<Player> Nearby = new List<Player>();

        private static void Postfix(CharacterDrop __instance)
        {
            Safe.Run("drop Magic and Rare loot", () =>
            {
                var character = __instance.m_character;
                if (!__instance.m_dropsEnabled || !Loot.Loaded || !CorruptionConfig.LootEnabled.Value || !character ||
                    character.IsPlayer() || character.IsTamed())
                {
                    return;
                }

                var sources = new List<string>();
                int stars = character.GetLevel() - 1;
                if (stars > 0)
                {
                    sources.Add("star_" + Mathf.Min(stars, 3));
                }
                // Maps have no tiers yet, so every rip counts as tier 1.
                if (Maddened.IsMaddened(character))
                {
                    sources.Add("rip_tier_1");
                }
                if (character.IsBoss() && CorruptedBosses.IsCorrupted(character))
                {
                    sources.Add("corrupted_boss");
                }

                Vector3 position = character.GetCenterPoint();
                string biome = Loot.BiomeAt(position);
                float range = Game.instance ? Game.instance.m_difficultyScaleRange : 100f;
                Nearby.Clear();
                Player.GetPlayersInRange(position, range, Nearby);
                int players = Mathf.Max(1, Nearby.Count);
                for (int i = 0; i < players; i++)
                {
                    foreach (var rarity in Loot.Drops.Roll(sources, () => Random.value * 0.99999f))
                    {
                        // Uniques come with a later step.
                        if (rarity != "magic" && rarity != "rare")
                        {
                            continue;
                        }
                        var item = Loot.Create(rarity, biome);
                        if (item != null)
                        {
                            Loot.Drop(item, position);
                        }
                    }
                }
            });
        }
    }

    /// <summary>Affix lines at the end of the item tooltip; vanilla's own text and other mods' lines stay.</summary>
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip),
        typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(bool))]
    internal static class LootTooltip
    {
        private const string InactiveColor = "#808080";

        private static void Postfix(ItemDrop.ItemData item, ref string __result)
        {
            var loot = Loot.Of(item);
            if (loot == null)
            {
                return;
            }
            string result = __result;
            Safe.Run("add affix lines to a tooltip", () =>
            {
                var text = new StringBuilder(result);
                string color = Loot.ColorFor(loot.Rarity);
                text.Append("\n\n<color=").Append(color).Append('>').Append(loot.Rarity == "rare" ? "Rare" : "Magic");
                if (loot.Rarity == "rare")
                {
                    text.Append(" ").Append(Localization.instance.Localize(item.m_shared.m_name));
                }
                text.Append("</color>");
                foreach (var line in loot.Lines)
                {
                    bool active = Loot.Affixes.TryGetValue(line.Ids[0], out var affix) && Loot.ActiveEffects.Contains(affix.Effect);
                    text.Append("\n<color=").Append(active ? color : InactiveColor).Append('>')
                        .Append(LootItem.LineText(line, Loot.Affixes));
                    if (!active)
                    {
                        text.Append(" (not active yet)");
                    }
                    text.Append("</color>");
                }
                result = text.ToString();
            });
            __result = result;
        }
    }

    /// <summary>Inventory tooltips: the title shows the rolled name in its rarity colour.</summary>
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
    internal static class LootTooltipTitle
    {
        private static void Postfix(InventoryGrid __instance, ItemDrop.ItemData item, UITooltip tooltip)
        {
            Safe.Run("colour a loot item's tooltip title", () =>
            {
                string name = Loot.ColoredName(item);
                if (name != null && tooltip)
                {
                    tooltip.Set(name, item.GetTooltip(), __instance.m_tooltipAnchor);
                }
            });
        }
    }

    /// <summary>Items on the ground: the hover text shows the rolled name in its rarity colour.</summary>
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText))]
    internal static class LootHoverText
    {
        private static void Postfix(ItemDrop __instance, ref string __result)
        {
            string result = __result;
            Safe.Run("colour a loot item's hover text", () =>
            {
                var item = __instance.m_itemData;
                string name = Loot.ColoredName(item);
                if (name != null && result.StartsWith(item.m_shared.m_name))
                {
                    result = name + result.Substring(item.m_shared.m_name.Length);
                }
            });
            __result = result;
        }
    }

    /// <summary>
    ///     Item-stat affixes, through the game's own stat getters, so tooltips, combat and repairs all see them:
    ///     enhanced damage and elemental damage (<c>GetDamage</c>), armour, durability and block power.
    /// </summary>
    [HarmonyPatch]
    internal static class LootStatEffects
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDamage), typeof(int), typeof(float))]
        private static void Damage(ItemDrop.ItemData __instance, ref HitData.DamageTypes __result)
        {
            var loot = Loot.Of(__instance);
            if (loot == null)
            {
                return;
            }
            var damage = __result;
            Safe.Run("apply damage affixes", () =>
            {
                // Enhanced damage raises every combat damage type; chop and pickaxe (tool) damage stay as they are.
                float enhanced = 1f + loot.Total("weapon_damage", Loot.Affixes) / 100f;
                damage.m_blunt *= enhanced;
                damage.m_slash *= enhanced;
                damage.m_pierce *= enhanced;
                damage.m_fire *= enhanced;
                damage.m_frost *= enhanced;
                damage.m_lightning *= enhanced;
                damage.m_poison *= enhanced;
                damage.m_spirit *= enhanced;

                // Elemental affixes add a share of the (enhanced) physical damage (AFFIX_RULES: % of physical).
                float physical = damage.m_blunt + damage.m_slash + damage.m_pierce;
                damage.m_fire += physical * loot.Total("fire_damage_percent", Loot.Affixes) / 100f;
                damage.m_frost += physical * loot.Total("frost_damage_percent", Loot.Affixes) / 100f;
                damage.m_lightning += physical * loot.Total("lightning_damage_percent", Loot.Affixes) / 100f;
                damage.m_poison += physical * loot.Total("poison_damage_percent", Loot.Affixes) / 100f;
                damage.m_spirit += physical * loot.Total("spirit_damage_percent", Loot.Affixes) / 100f;
            });
            __result = damage;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetArmor), typeof(int), typeof(float))]
        private static void Armor(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "armour_percent") / 100f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetMaxDurability), typeof(int))]
        private static void Durability(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "max_durability") / 100f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetBaseBlockPower), typeof(int))]
        private static void BlockPower(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "block_percent") / 100f;
        }
    }

    /// <summary>Attack stamina affixes: less stamina per attack (or per swing, on tools) with the item.</summary>
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    internal static class LootAttackStamina
    {
        private static void Postfix(Attack __instance, ref float __result)
        {
            if (__result > 0f && __instance.m_weapon != null)
            {
                __result *= Mathf.Max(0f, 1f - Loot.Total(__instance.m_weapon, "attack_stamina_reduction") / 100f);
            }
        }
    }

    /// <summary>
    ///     Knockback and backstab affixes. A hit's push force and backstab bonus come from the weapon's shared data, so
    ///     they are raised on the attacker's game, where the hit is made, before it is sent to the target's owner.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    internal static class LootHitEffects
    {
        private static void Prefix(HitData hit)
        {
            if (hit == null || !Player.m_localPlayer || hit.GetAttacker() != Player.m_localPlayer)
            {
                return;
            }
            Safe.Run("apply knockback and backstab affixes", () =>
            {
                var weapon = Player.m_localPlayer.GetCurrentWeapon();
                hit.m_pushForce *= 1f + Loot.Total(weapon, "knockback_percent") / 100f;
                hit.m_backstabBonus *= 1f + Loot.Total(weapon, "backstab_percent") / 100f;
            });
        }
    }

    /// <summary>Parry affix: a bigger parry bonus with the blocking item (only called when a block is a parry).</summary>
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyTimedBlockBonus))]
    internal static class LootParry
    {
        private static void Postfix(SEMan __instance, ref float timedBlockBonus)
        {
            if (__instance.m_character is Humanoid humanoid && humanoid.IsPlayer())
            {
                timedBlockBonus *= 1f + Loot.Total(humanoid.GetCurrentBlocker(), "parry_percent") / 100f;
            }
        }
    }
}
