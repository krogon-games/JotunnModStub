using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Player-stat affixes, summed over everything the local player has equipped (<see cref="Loot.Equipped" />) and
    ///     capped by <c>affix_caps.csv</c> where a cap exists. Only the local player's own game knows its items' affixes,
    ///     and it is also the game that works out its health, stamina, speed, carry weight and the damage it takes.
    /// </summary>
    [HarmonyPatch]
    internal static class LootPlayerStats
    {
        /// <summary>Max health, stamina and eitr: added to what food gives, like vanilla's base values.</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        private static void MaxStats(Player __instance, ref float hp, ref float stamina, ref float eitr)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }
            hp += Loot.EquippedTotal(__instance, "max_health");
            stamina += Loot.EquippedTotal(__instance, "max_stamina");
            eitr += Loot.EquippedTotal(__instance, "max_eitr");
        }

        /// <summary>Health regen: +X% adds X/100 to the regen multiplier, as vanilla's regen bonuses do (SE_Stats).</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]
        private static void HealthRegen(SEMan __instance, ref float regenMultiplier)
        {
            if (__instance.m_character is Player player)
            {
                regenMultiplier += Loot.EquippedTotal(player, "health_regen_percent") / 100f;
            }
        }

        /// <summary>Stamina regen, the same way.</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyStaminaRegen))]
        private static void StaminaRegen(SEMan __instance, ref float staminaMultiplier)
        {
            if (__instance.m_character is Player player)
            {
                staminaMultiplier += Loot.EquippedTotal(player, "stamina_regen_percent") / 100f;
            }
        }

        /// <summary>
        ///     Movement speed: added to the gear movement modifier (vanilla's +5% Crown of Valheim works the same way, so
        ///     it also eases jump and dodge stamina as vanilla does). Worn and held (Fleet) together, under the cap.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMovementModifier))]
        private static void Movement(Player __instance, ref float __result)
        {
            if (__instance == Player.m_localPlayer)
            {
                __result += AffixTotals.MovementPercent(Loot.Equipped(__instance), Loot.Affixes, Loot.Caps) / 100f;
            }
        }

        /// <summary>Carry weight: added to the limit, scaled by the world's carry-weight setting like the rest.</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.GetMaxCarryWeight))]
        private static void CarryWeight(Player __instance, ref float __result)
        {
            if (__instance == Player.m_localPlayer)
            {
                __result += Loot.EquippedTotal(__instance, "carry_weight") * Game.m_carryWeightRate;
            }
        }

        /// <summary>Weapon skill: extra levels in the skill of the weapon in hand (vanilla trinkets add levels the same way).</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifySkillLevel))]
        private static void WeaponSkill(SEMan __instance, Skills.SkillType skill, ref float level)
        {
            if (__instance.m_character is Player player && player == Player.m_localPlayer)
            {
                var weapon = player.GetCurrentWeapon();
                if (weapon != null && weapon.m_shared.m_skillType == skill)
                {
                    level += Loot.Total(weapon, "weapon_skill_levels");
                }
            }
        }
    }

    /// <summary>
    ///     Resistance affixes: less fire, frost, lightning and poison damage taken, on top of vanilla's resistances.
    ///     Hits on a player arrive at that player's own game (the owner), so they are reduced there.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    internal static class LootResistances
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (hit == null || !(__instance is Player player) || player != Player.m_localPlayer)
            {
                return;
            }
            Safe.Run("apply resistance affixes", () =>
            {
                var items = Loot.Equipped(player);
                hit.m_damage.m_fire *= 1f - AffixTotals.ResistancePercent("fire", items, Loot.Affixes, Loot.Caps) / 100f;
                hit.m_damage.m_frost *= 1f - AffixTotals.ResistancePercent("frost", items, Loot.Affixes, Loot.Caps) / 100f;
                hit.m_damage.m_lightning *= 1f - AffixTotals.ResistancePercent("lightning", items, Loot.Affixes, Loot.Caps) / 100f;
                hit.m_damage.m_poison *= 1f - AffixTotals.ResistancePercent("poison", items, Loot.Affixes, Loot.Caps) / 100f;
            });
        }
    }
}
