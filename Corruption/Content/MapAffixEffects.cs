using Corruption.Rules;
using HarmonyLib;

namespace Corruption.Content
{
    /// <summary>
    ///     The map affixes that change how rip creatures fight (rips round, 2026-11-26, decisions 8 and 10): Hardened
    ///     (they take less damage) and Smouldering (their hits also burn). Both read the rip's stamp on the creature
    ///     itself (<see cref="MapTiers.KeyTier" />, <see cref="MapTiers.KeyAffixes" />), never the heart, so they hold
    ///     wherever the creature goes. Swarming is in <see cref="RipWaves" />, Rabid in <see cref="Maddened" />.
    /// </summary>
    internal static class MapAffixEffects
    {
        private static ZDO StampOf(Character character)
        {
            if (!character || !character.m_nview || !character.m_nview.IsValid())
            {
                return null;
            }
            var zdo = character.m_nview.GetZDO();
            return MapTiers.IsStamped(zdo) ? zdo : null;
        }

        /// <summary>
        ///     Hardened: a stamped creature takes the affix's share of every hit. On the game that owns the creature, where
        ///     vanilla applies damage (as the corrupted boss's reduction does); players are never changed.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class Hardened
        {
            private static void Prefix(Character __instance, HitData hit)
            {
                if (hit == null || !MapTiers.Loaded)
                {
                    return;
                }
                try
                {
                    if (__instance.IsPlayer() || !__instance.m_nview || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner())
                    {
                        return;
                    }
                    var zdo = StampOf(__instance);
                    if (zdo == null)
                    {
                        return;
                    }
                    double taken = RipTierRules.DamageTaken(MapTiers.AffixesOf(zdo));
                    if (taken < 1.0)
                    {
                        hit.ApplyModifier((float)taken);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("apply Hardened damage", ex);
                }
            }
        }

        /// <summary>
        ///     Smouldering: a stamped creature's hit also carries the affix's share of its combat damage as fire, which the game
        ///     turns into burning. On the attacker's game, before the hit is sent to the victim's owner. Damage-over-time hits
        ///     are skipped so burning never feeds itself; the creature never burns itself.
        ///     Priority Low: after Orlog's crit (VeryHigh), Corruption's on-hit affixes (High) and life steal (Normal), which
        ///     only act on a player's hits anyway; they see the hit as before and the fire is added last, from the final damage.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static class Smouldering
        {
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Character __instance, HitData hit)
            {
                if (hit == null || !MapTiers.Loaded || hit.m_attacker.IsNone() || HitSources.IsDamageOverTime(hit))
                {
                    return;
                }
                Safe.Run("add Smouldering fire", () =>
                {
                    // Each hit is built once, on whichever game makes it (melee and projectiles on the creature's owner, some
                    // area hits on the victim's owner), so no owner check (Opus review, 2026-11-26).
                    var attacker = hit.GetAttacker();
                    if (!attacker || attacker == __instance || attacker.IsPlayer())
                    {
                        return;
                    }
                    var zdo = StampOf(attacker);
                    if (zdo == null)
                    {
                        return;
                    }
                    double share = RipTierRules.FireShare(MapTiers.AffixesOf(zdo));
                    if (share > 0.0)
                    {
                        // The hit's combat damage, as life steal counts it: not its chop and pickaxe damage, which only trees,
                        // rocks and buildings take (a goblin brute's hit would otherwise burn for twice the share).
                        var d = hit.m_damage;
                        float combat = d.m_blunt + d.m_slash + d.m_pierce + d.m_fire + d.m_frost + d.m_lightning + d.m_poison + d.m_spirit;
                        hit.m_damage.m_fire += (float)(share * combat);
                    }
                });
            }
        }
    }
}
