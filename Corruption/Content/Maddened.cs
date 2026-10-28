using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Maddened creatures (rip waves, and creatures near a rip when it opens): tinted red, a status effect that
    ///     raises damage and speed, no fleeing, and they treat every other creature and player as an enemy, so they
    ///     fight each other too (design). Talismans don't repel them (user, 2026-10-28). The flag lives on the
    ///     creature's ZDO, so every player's game agrees and it survives reloads.
    /// </summary>
    internal static class Maddened
    {
        public const string KeyMaddened = "corruption_maddened";
        public const string StatusEffectName = "SE_CorruptionMaddened";

        // Vanilla sources of the effect's numbers, read when the effect is made (game data, not our guesses).
        private const string DamageSource = "SE_Dvergr_buff";   // Dvergr power: the game's own creature damage buff
        private const string SpeedSource = "Potion_hasty";      // Tonic of Ratatosk: the strongest vanilla speed buff

        private static readonly Color Red = new Color(0.7f, 0.09f, 0.05f);
        private static readonly HashSet<Character> Active = new HashSet<Character>();
        private static int s_effectHash;

        public static bool IsMaddened(Character character)
        {
            return character && Active.Contains(character);
        }

        /// <summary>Makes a creature maddened. Takes ownership first: only the owner may change its data.</summary>
        public static void Madden(Character character)
        {
            if (!character || character.IsPlayer() || character.IsBoss() || !character.m_nview || !character.m_nview.IsValid())
            {
                return;
            }
            character.m_nview.ClaimOwnership();
            character.m_nview.GetZDO().Set(KeyMaddened, true);
            if (!character.GetComponent<MaddenedCreature>())
            {
                character.gameObject.AddComponent<MaddenedCreature>();
            }
        }

        /// <summary>The status effect, built from the vanilla effects named above. Register after ObjectDB is ready.</summary>
        public static void RegisterEffect()
        {
            var effect = ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = StatusEffectName;
            effect.m_name = "$corruption_maddened";
            effect.m_tooltip = "$corruption_maddened_tooltip";
            effect.m_ttl = 0f;   // lasts until the creature dies
            var damage = ObjectDB.instance.GetStatusEffect(DamageSource.GetStableHashCode()) as SE_Stats;
            if (damage)
            {
                effect.m_modifyAttackSkill = damage.m_modifyAttackSkill;
                effect.m_damageModifier = damage.m_damageModifier;
            }
            else
            {
                Jotunn.Logger.LogWarning($"Maddened: {DamageSource} not found; maddened creatures get no damage bonus.");
            }
            var speed = ObjectDB.instance.GetStatusEffect(SpeedSource.GetStableHashCode()) as SE_Stats;
            if (speed)
            {
                effect.m_speedModifier = speed.m_speedModifier;
            }
            else
            {
                Jotunn.Logger.LogWarning($"Maddened: {SpeedSource} not found; maddened creatures get no speed bonus.");
            }
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));
            s_effectHash = StatusEffectName.GetStableHashCode();
        }

        /// <summary>Creatures loaded with the flag (another player maddened them, or after a reload) get the behaviour too.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
        private static class AddOnLoad
        {
            private static void Postfix(Character __instance)
            {
                Safe.Run("restore a maddened creature", () =>
                {
                    var zdo = __instance.m_nview ? __instance.m_nview.GetZDO() : null;
                    if (zdo != null && zdo.GetBool(KeyMaddened) && !__instance.GetComponent<MaddenedCreature>())
                    {
                        __instance.gameObject.AddComponent<MaddenedCreature>();
                    }
                });
            }
        }

        /// <summary>
        ///     Maddened creatures are everyone's enemy and everyone is theirs. Runs after the talisman patch, so a
        ///     talisman can't make a maddened creature ignore its wearer.
        /// </summary>
        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IsEnemy), typeof(Character), typeof(Character))]
        private static class Enemies
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(Character a, Character b, ref bool __result)
            {
                if (__result || a == b || !a || !b)
                {
                    return;
                }
                if (IsMaddened(a) || IsMaddened(b))
                {
                    __result = true;
                }
            }
        }

        private sealed class MaddenedCreature : MonoBehaviour
        {
            private Character m_character;

            private void Awake()
            {
                m_character = GetComponent<Character>();
                Active.Add(m_character);
                Safe.Run("make a creature maddened", () =>
                {
                    Corrupted.Tint(gameObject, Red, CorruptionConfig.CorruptedTintStrength.Value);
                    // No fleeing (design): the flee settings are per instance.
                    var monster = GetComponent<MonsterAI>();
                    if (monster)
                    {
                        monster.m_fleeIfHurtWhenTargetCantBeReached = false;
                        monster.m_fleeIfLowHealth = 0f;
                        monster.m_fleeIfNotAlerted = false;
                    }
                });
                InvokeRepeating(nameof(KeepEffect), 0.2f, 2f);
            }

            private void OnDestroy()
            {
                Active.Remove(m_character);
            }

            /// <summary>The owner (who simulates the creature) keeps the effect on, also after ownership changes.</summary>
            private void KeepEffect()
            {
                Safe.Run("keep a creature maddened", () =>
                {
                    if (s_effectHash == 0 || !m_character || !m_character.m_nview || !m_character.m_nview.IsOwner())
                    {
                        return;
                    }
                    var seman = m_character.GetSEMan();
                    if (seman != null && !seman.HaveStatusEffect(s_effectHash))
                    {
                        seman.AddStatusEffect(s_effectHash, true);
                    }
                });
            }
        }
    }
}
