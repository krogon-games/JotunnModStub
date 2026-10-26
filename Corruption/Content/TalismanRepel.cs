using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Makes a talisman's creature family ignore the wearer.
    ///
    ///     <see cref="BaseAI.IsEnemy(Character, Character)" /> decides both whom creatures target and whom attacks can
    ///     hit. Only the creature's side is changed (creature asking about the wearer), so the wearer can still hit
    ///     those creatures. Hitting one provokes it: it fights back for <c>ProvokeSeconds</c> after the last hit.
    ///
    ///     All of this runs on the machine that simulates the creature. Other players' trinkets are read from their
    ///     synced equipment, so it works in multiplayer.
    /// </summary>
    internal static class TalismanRepel
    {
        private struct Pair
        {
            public ZDOID Creature;
            public ZDOID Player;
        }

        private sealed class PairComparer : IEqualityComparer<Pair>
        {
            public bool Equals(Pair a, Pair b) => a.Creature == b.Creature && a.Player == b.Player;
            public int GetHashCode(Pair p) => p.Creature.GetHashCode() * 397 ^ p.Player.GetHashCode();
        }

        /// <summary>Creature + wearer → time until which the creature fights back.</summary>
        private static readonly Dictionary<Pair, float> ProvokedUntil = new Dictionary<Pair, float>(new PairComparer());

        public static bool IsRepelled(Character creature, Character target)
        {
            if (!(target is Player player) || creature is Player || Talismans.RepelledBy.Count == 0 ||
                !CorruptionConfig.TalismansEnabled.Value || creature.IsBoss() || creature.IsTamed())
            {
                return false;
            }

            var vis = player.m_visEquipment;
            if (!vis || vis.m_currentTrinketItemHash == 0 ||
                !Talismans.RepelledBy.TryGetValue(vis.m_currentTrinketItemHash, out var family))
            {
                return false;
            }

            var zdo = creature.m_nview ? creature.m_nview.GetZDO() : null;
            if (zdo == null || !family.Contains(zdo.GetPrefab()))
            {
                return false;
            }

            return !IsProvoked(creature, player);
        }

        private static bool IsProvoked(Character creature, Character player)
        {
            var key = new Pair { Creature = creature.GetZDOID(), Player = player.GetZDOID() };
            return ProvokedUntil.TryGetValue(key, out float until) && Time.time < until;
        }

        private static void Provoke(Character creature, Character player)
        {
            if (ProvokedUntil.Count > 256)
            {
                var expired = new List<Pair>();
                foreach (var entry in ProvokedUntil)
                {
                    if (Time.time >= entry.Value)
                    {
                        expired.Add(entry.Key);
                    }
                }
                foreach (var key in expired)
                {
                    ProvokedUntil.Remove(key);
                }
            }
            ProvokedUntil[new Pair { Creature = creature.GetZDOID(), Player = player.GetZDOID() }] =
                Time.time + CorruptionConfig.ProvokeSeconds.Value;
        }

        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IsEnemy), typeof(Character), typeof(Character))]
        private static class IsEnemy_Patch
        {
            private static void Postfix(Character a, Character b, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }
                try
                {
                    if (IsRepelled(a, b))
                    {
                        __result = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("check talismans", ex);
                }
            }
        }

        /// <summary>The wearer struck first: this creature may fight back (it checks IsEnemy right after this).</summary>
        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.OnDamaged))]
        private static class OnDamaged_Patch
        {
            private static void Prefix(BaseAI __instance, Character __1)
            {
                try
                {
                    var creature = __instance.m_character;
                    if (__1 is Player && creature && IsRepelledIgnoringProvocation(creature, __1))
                    {
                        Provoke(creature, __1);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("provoke a repelled creature", ex);
                }
            }

            private static bool IsRepelledIgnoringProvocation(Character creature, Character player)
            {
                var vis = ((Player)player).m_visEquipment;
                if (!vis || !Talismans.RepelledBy.TryGetValue(vis.m_currentTrinketItemHash, out var family))
                {
                    return false;
                }
                var zdo = creature.m_nview ? creature.m_nview.GetZDO() : null;
                return zdo != null && family.Contains(zdo.GetPrefab());
            }
        }
    }
}
