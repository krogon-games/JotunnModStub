using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Corrupted bosses (design phase 6), the parts the design fully specifies: a corrupted boss hits harder,
    ///     takes less damage, drops more essences and guaranteed shards, and is tinted red. The flag lives on the
    ///     boss's ZDO, so every player's game agrees. Corrupted offerings (<see cref="CorruptedOfferings" />) summon
    ///     them; <c>co_corrupt</c> corrupts the nearest boss for testing. Their summons: <see cref="CorruptedSummons" />.
    /// </summary>
    internal static class CorruptedBosses
    {
        public const string KeyCorrupted = "corruption_corrupted";

        private static readonly Color Red = new Color(0.7f, 0.09f, 0.05f);

        public static bool IsCorrupted(Character character)
        {
            if (!character || !character.m_nview || !character.m_nview.IsValid())
            {
                return false;
            }
            return character.m_nview.GetZDO().GetBool(KeyCorrupted);
        }

        /// <summary>Marks a boss as corrupted. Takes ownership first, as only the owner may change its data.</summary>
        public static void Corrupt(Character boss)
        {
            if (!boss || !boss.m_nview || !boss.m_nview.IsValid())
            {
                return;
            }
            boss.m_nview.ClaimOwnership();
            boss.m_nview.GetZDO().Set(KeyCorrupted, true);
        }

        public static void Register()
        {
            CommandManager.Instance.AddConsoleCommand(new CorruptCommand());
        }

        /// <summary>Every boss gets a small watcher that tints it once it's corrupted, on every player's game, and the summoner.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
        private static class AddLook
        {
            private static void Postfix(Character __instance)
            {
                Safe.Run("watch a boss for corruption", () =>
                {
                    if (__instance.IsBoss() && !__instance.GetComponent<CorruptedLook>())
                    {
                        __instance.gameObject.AddComponent<CorruptedLook>();
                    }
                    if (__instance.IsBoss() && !__instance.GetComponent<CorruptedSummons>())
                    {
                        __instance.gameObject.AddComponent<CorruptedSummons>();
                    }
                });
            }
        }

        /// <summary>
        ///     Damage multipliers, on the game that owns the creature being hit (where vanilla applies damage):
        ///     hits on a corrupted boss are reduced, hits from one are increased.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class Damage
        {
            private static void Prefix(Character __instance, HitData __1)
            {
                if (__1 == null)
                {
                    return;
                }
                try
                {
                    if (IsCorrupted(__instance))
                    {
                        __1.ApplyModifier(CorruptionConfig.CorruptedDamageTaken.Value);
                    }
                    var attacker = __1.GetAttacker();
                    if (attacker && attacker != __instance && IsCorrupted(attacker))
                    {
                        __1.ApplyModifier(CorruptionConfig.CorruptedDamageDealt.Value);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("apply corrupted boss damage", ex);
                }
            }
        }

        private sealed class CorruptedLook : MonoBehaviour
        {
            private Character m_character;

            private void Awake()
            {
                m_character = GetComponent<Character>();
                InvokeRepeating(nameof(Check), 0.5f, 2f);
            }

            private void Check()
            {
                Safe.Run("tint a corrupted boss", () =>
                {
                    if (!IsCorrupted(m_character))
                    {
                        return;
                    }
                    CancelInvoke(nameof(Check));
                    Corrupted.Tint(gameObject, Red, CorruptionConfig.CorruptedTintStrength.Value);
                });
            }
        }

        /// <summary>co_corrupt: corrupts the nearest boss within 50 m (testing aid until corrupted offerings exist).</summary>
        private sealed class CorruptCommand : ConsoleCommand
        {
            public override string Name => "co_corrupt";

            public override string Help => "Corruption: make the nearest boss (within 50 m) corrupted, for testing";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player)
                {
                    Console.instance.Print("co_corrupt needs a loaded world.");
                    return;
                }
                Character nearest = null;
                float best = 50f;
                foreach (var character in Character.GetAllCharacters())
                {
                    float distance = Vector3.Distance(character.transform.position, player.transform.position);
                    if (character.IsBoss() && distance < best)
                    {
                        best = distance;
                        nearest = character;
                    }
                }
                if (!nearest)
                {
                    Console.instance.Print("No boss within 50 m. Spawn one first, e.g. spawn Eikthyr.");
                    return;
                }
                Corrupt(nearest);
                Console.instance.Print($"{Localization.instance.Localize(nearest.m_name)} is now corrupted.");
            }
        }
    }
}
