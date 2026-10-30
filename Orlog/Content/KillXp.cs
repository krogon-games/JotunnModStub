using System.Collections.Generic;
using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     XP for kills. The creature's owner (whose game ran the fight) works out the kill's XP when the creature dies and
    ///     sends each player's share to that player's game, which adds it.
    ///     <list type="bullet">
    ///         <item>Only kills by a player, or by a tamed or summoned creature on the players' side, give XP.</item>
    ///         <item>Every player within <c>ShareRange</c> of the kill shares it (Diablo 2's party rule).</item>
    ///         <item>
    ///             Creature level: a boss has its level from the levelling guide; any other creature is placed in the level
    ///             range of the biome it died in, by its base health. Where a biome has no range (the ocean) the creature
    ///             counts as the players' average level.
    ///         </item>
    ///         <item>A starred creature gives its game level times the XP (it has that many times the health).</item>
    ///         <item>A creature above a player's level is worth that player's own level's XP plus WoW's bonus; at or below, its own XP with D2's cut (<see cref="ExperienceRules.Worth" />).</item>
    ///     </list>
    /// </summary>
    internal static class KillXp
    {
        private const string RpcXp = "Orlog_Xp";
        private static readonly List<Player> Nearby = new List<Player>();

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpc
        {
            private static void Postfix()
            {
                Safe.Run("register the XP message", () => ZRoutedRpc.instance.Register<ZPackage>(RpcXp, RPC_Xp));
            }
        }

        // Prefix: OnDeath ends by destroying the creature's network object, after which its ZDO is gone.
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        private static class AwardOnDeath
        {
            private static void Prefix(Character __instance)
            {
                Safe.Run("award XP for a kill", () => Award(__instance));
            }
        }

        private static void Award(Character creature)
        {
            if (!Progress.Ready || !creature || creature.IsPlayer() || !creature.m_nview || !creature.m_nview.IsValid() ||
                !creature.m_nview.IsOwner() || creature.IsTamed() || OnPlayersSide(creature))
            {
                return;
            }
            var attacker = creature.m_lastHit != null ? creature.m_lastHit.GetAttacker() : null;
            if (!attacker || !(attacker.IsPlayer() || attacker.IsTamed() || OnPlayersSide(attacker)))
            {
                return;
            }

            Vector3 position = creature.transform.position;
            Nearby.Clear();
            Player.GetPlayersInRange(position, OrlogConfig.ShareRange.Value, Nearby);
            if (Nearby.Count == 0)
            {
                return;
            }
            var levels = new int[Nearby.Count];
            int levelSum = 0;
            for (int i = 0; i < Nearby.Count; i++)
            {
                levels[i] = Progress.LevelOf(Nearby[i]);
                levelSum += levels[i];
            }

            int creatureLevel = CreatureLevel(creature, position);
            if (creatureLevel <= 0)
            {
                creatureLevel = Mathf.Max(1, Mathf.RoundToInt((float)levelSum / Nearby.Count));
            }
            double perLevel = Mathf.Max(1, creature.GetLevel()) * OrlogConfig.XpMultiplier.Value;
            var shares = ExperienceRules.Share(level => (long)System.Math.Round(Progress.CreatureXp.For(level) * perLevel), creatureLevel, levels,
                OrlogConfig.PartyBonusPerPlayer.Value, OrlogConfig.HighLevelPenalty.Value ? (System.Func<int, float>)Progress.Levels.HighLevelRatio : null);

            for (int i = 0; i < Nearby.Count; i++)
            {
                var zdo = Nearby[i].m_nview ? Nearby[i].m_nview.GetZDO() : null;
                if (shares[i] <= 0 || zdo == null)
                {
                    continue;
                }
                var package = new ZPackage();
                package.Write(Nearby[i].GetZDOID());
                package.Write(shares[i]);
                ZRoutedRpc.instance.InvokeRoutedRPC(zdo.GetOwner(), RpcXp, package);
            }
        }

        /// <summary>The creature's level for XP (0 = no range here; the caller uses the players' level).</summary>
        public static int CreatureLevel(Character creature, Vector3 position)
        {
            int boss = Progress.Biomes.BossLevel(Utils.GetPrefabName(creature.gameObject));
            if (boss > 0)
            {
                return boss;
            }
            string biome = WorldGenerator.instance != null ? WorldGenerator.instance.GetBiome(position).ToString() : null;
            return Progress.Biomes.CreatureLevel(biome, creature.m_health);
        }

        private static bool OnPlayersSide(Character character)
        {
            string faction = character.m_faction.ToString();
            return faction == "Players" || faction == "PlayerSpawned" || faction == "TrainingDummy";
        }

        /// <summary>A share of a kill's XP, arriving at the game of the player it's for.</summary>
        private static void RPC_Xp(long sender, ZPackage package)
        {
            Safe.Run("add XP from a kill", () =>
            {
                ZDOID target = package.ReadZDOID();
                long amount = package.ReadLong();
                var player = Player.m_localPlayer;
                if (player && player.GetZDOID() == target)
                {
                    Progress.AddXp(player, amount);
                }
            });
        }
    }
}
