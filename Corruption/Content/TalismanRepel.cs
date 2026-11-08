using System.Collections.Generic;
using Corruption.Rules;
using HarmonyLib;

namespace Corruption.Content
{
    /// <summary>
    ///     Makes a talisman's creature family ignore the wearer.
    ///
    ///     <see cref="BaseAI.IsEnemy(Character, Character)" /> decides both whom creatures target and whom attacks can
    ///     hit. Only the creature's side is changed (creature asking about the wearer), so the wearer can still hit
    ///     those creatures. Hitting one provokes it: it fights back at that player for <c>ProvokeSeconds</c> after the
    ///     last hit. The time is kept on the creature's ZDO in world time (<see cref="ProvokeRules" />), so it lasts as
    ///     long when the creature changes owner (its owner logs out) and only concerns the player who hit.
    ///
    ///     All of this runs on the machine that simulates the creature. Each player's game publishes the talismans
    ///     in its talisman slots on its player object (<see cref="TalismanSlots" />), so it works in multiplayer.
    /// </summary>
    internal static class TalismanRepel
    {
        /// <summary>The ZDO key hash of each player's provocation (<see cref="ProvokeRules.Key" />), worked out once per player.</summary>
        private static readonly Dictionary<long, int> KeyHashes = new Dictionary<long, int>();

        private static int KeyHash(long playerId)
        {
            if (!KeyHashes.TryGetValue(playerId, out int hash))
            {
                hash = ProvokeRules.Key(playerId).GetStableHashCode();
                KeyHashes[playerId] = hash;
            }
            return hash;
        }

        /// <summary>Whether the player wears a talisman (in any talisman slot) that repels this creature.</summary>
        private static bool Wears(Player player, Character creature)
        {
            var zdo = creature.m_nview ? creature.m_nview.GetZDO() : null;
            if (zdo == null)
            {
                return false;
            }
            var playerZdo = player.m_nview ? player.m_nview.GetZDO() : null;
            if (playerZdo == null)
            {
                return false;
            }
            int prefab = zdo.GetPrefab();
            // A plain loop: this runs whenever a creature looks for targets, so it mustn't allocate.
            for (int i = 0; i < TalismanSlots.WornKeys.Length; i++)
            {
                int worn = TalismanSlots.WornAt(playerZdo, i);
                if (worn != 0 && Talismans.RepelledBy.TryGetValue(worn, out var family) && family.Contains(prefab))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsRepelled(Character creature, Character target)
        {
            if (!(target is Player player) || creature is Player || Talismans.RepelledBy.Count == 0 ||
                !CorruptionConfig.TalismansEnabled.Value || creature.IsBoss() || creature.IsTamed())
            {
                return false;
            }

            return Wears(player, creature) && !IsProvoked(creature, player);
        }

        /// <summary>
        ///     Whether the creature is still fighting back at this player: the time on the creature's ZDO (synced, so a
        ///     new owner sees it) against the world time every game shares.
        /// </summary>
        private static bool IsProvoked(Character creature, Player player)
        {
            var zdo = creature.m_nview ? creature.m_nview.GetZDO() : null;
            long playerId = player.GetPlayerID();
            if (zdo == null || playerId == 0L || !ZNet.instance)
            {
                return false;
            }
            return ProvokeRules.IsProvoked(zdo.GetLong(KeyHash(playerId)), ZNet.instance.GetTimeSeconds());
        }

        /// <summary>
        ///     On the creature's owner (the game runs <c>BaseAI.OnDamaged</c> only there): it fights back at this player
        ///     until <c>ProvokeSeconds</c> after this hit, written on the creature's ZDO for this player only.
        /// </summary>
        private static void Provoke(Character creature, Player player)
        {
            var nview = creature.m_nview;
            long playerId = player.GetPlayerID();
            if (!nview || !nview.IsValid() || !nview.IsOwner() || playerId == 0L || !ZNet.instance)
            {
                return;
            }
            long until = ProvokeRules.Until(ZNet.instance.GetTimeSeconds(), CorruptionConfig.ProvokeSeconds.Value);
            if (until > 0L)
            {
                nview.GetZDO().Set(KeyHash(playerId), until);
                // For the multiplayer check: which game runs the creature, and that each hit restarts the time.
                Jotunn.Logger.LogInfo($"Talisman: {Utils.GetPrefabName(creature.gameObject)} fights back at {player.GetPlayerName()} " +
                                      $"for {CorruptionConfig.ProvokeSeconds.Value:0.#} s (run by this game).");
            }
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
                    if (__1 is Player player && creature && Wears(player, creature))
                    {
                        Provoke(creature, player);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("provoke a repelled creature", ex);
                }
            }
        }
    }
}
