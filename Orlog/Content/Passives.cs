using System.Collections.Generic;
using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The Amazon's passives (roadmap C3; rules in <see cref="PassiveRules" />, chances in
    ///     <c>Orlog/Data/&lt;class&gt;_passives.csv</c>). Each runs on the game that knows what it needs:
    ///     <list type="bullet">
    ///         <item>
    ///             Critical Strike and Penetrate roll on the attacker's game (<c>Character.Damage</c>, where the hit is made). A
    ///             crit doubles the physical damage there; Penetrate marks the hit, and the creature owner's game, where
    ///             resistances are applied (<c>Character.RPC_Damage</c> → <c>HitData.ApplyResistance</c>), lifts them.
    ///         </item>
    ///         <item>
    ///             Dodge, Avoid and Evade roll on the player's own game (it owns the player and runs <c>RPC_Damage</c> for
    ///             hits on them); an avoided hit is dropped before vanilla applies it.
    ///         </item>
    ///         <item>
    ///             Area blasts and breath can't be avoided. Aoe hits carry their radius (<c>HitData.m_radius</c>); the
    ///             creature's own game gives a radius to hits from its area attacks (<c>Attack.DoAreaAttack</c>) and
    ///             breath projectiles, so the player's game can tell. Vanilla only reads <c>m_radius</c> on rock hits.
    ///         </item>
    ///     </list>
    /// </summary>
    internal static class Passives
    {
        private static readonly Dictionary<string, PassiveTable> Tables = new Dictionary<string, PassiveTable>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>The radius given to hits from a creature's area attack or breath (any value above 0 marks them).</summary>
        private const float AreaRadius = 1f;

        /// <summary>The creature area attack running on this game (set during <c>Attack.DoAreaAttack</c>).</summary>
        private static Attack s_areaAttack;

        /// <summary>The projectile whose hit is running on this game (set during <c>Projectile.OnHit</c>).</summary>
        private static Projectile s_projectile;

        /// <summary>The hit Penetrate is lifting resistances for (set during <c>Character.RPC_Damage</c>).</summary>
        private static HitData s_penetrating;

        /// <summary>The class's passives table, read once from its embedded table; null if the class has none.</summary>
        public static PassiveTable TableFor(ClassTable.Class cls)
        {
            if (cls == null)
            {
                return null;
            }
            if (!Tables.TryGetValue(cls.Name, out var table))
            {
                string file = cls.Name.ToLowerInvariant() + "_passives.csv";
                string csv = Progress.ReadData(file);
                var errors = new List<string>();
                table = csv.Length > 0 ? PassiveTable.Parse(csv, file, errors) : null;
                foreach (var error in errors)
                {
                    Jotunn.Logger.LogError("Orlog data: " + error);
                }
                Tables[cls.Name] = table;
            }
            return table;
        }

        /// <summary>Chances set with <c>orlog_passive</c> for testing (this session only, never saved).</summary>
        private static readonly Dictionary<string, float> TestChances = new Dictionary<string, float>(System.StringComparer.OrdinalIgnoreCase);

        public static void SetTestChance(string effect, float chance)
        {
            TestChances[effect] = chance;
        }

        public static bool HasTestChance(string effect)
        {
            return TestChances.ContainsKey(effect);
        }

        public static void ClearTestChances()
        {
            TestChances.Clear();
        }

        /// <summary>The local player's chance in percent for a passive effect (0 without the class or points).</summary>
        public static float Chance(Player player, string effect)
        {
            if (!player || !Progress.Ready)
            {
                return 0f;
            }
            if (TestChances.TryGetValue(effect, out float test))
            {
                return test;
            }
            var passive = TableFor(Attributes.ClassOf(player))?.ForEffect(effect);
            if (passive == null)
            {
                return 0f;
            }
            // No +skills yet (items, Battle Command): the skill level is the hard points.
            return PassiveRules.Chance(passive, SkillRules.PointsIn(SkillPoints.Points(player), passive.Id));
        }

        /// <summary>The tooltip line for a passive skill at a level, or null if the skill isn't a passive with a chance.</summary>
        public static string Describe(ClassTable.Class cls, string skillId, int level)
        {
            var passive = TableFor(cls)?.Find(skillId);
            return passive == null ? null : PassiveRules.Describe(passive, level);
        }

        private static bool Roll(float chance)
        {
            return PassiveRules.Rolls(chance, Random.value);
        }

        /// <summary>With devcommands on (testing), a top-left message for each passive that triggers.</summary>
        private static void Tell(string text)
        {
            if (Terminal.m_cheat && Player.m_localPlayer)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, text);
            }
        }

        /// <summary>
        ///     The attacker's side of a hit. Runs before Corruption's <c>Character.Damage</c> prefixes (Rallying and Skadi at
        ///     High, life steal at Normal; <c>docs/mod-compatibility.md</c>) so they count a crit's damage.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static class AttackerSide
        {
            [HarmonyPriority(Priority.VeryHigh)]
            private static void Prefix(Character __instance, HitData hit)
            {
                if (hit == null || !__instance)
                {
                    return;
                }
                Safe.Run("apply Critical Strike and Penetrate", () =>
                {
                    var player = Player.m_localPlayer;
                    var attacker = hit.GetAttacker();
                    if (player && attacker == player && !__instance.IsPlayer() && hit.m_hitType == HitData.HitType.PlayerHit)
                    {
                        OwnHit(player, __instance, hit);
                    }
                    else if (attacker && !attacker.IsPlayer() && __instance.IsPlayer() && hit.m_radius <= 0f && IsAreaOrBreath(attacker))
                    {
                        hit.m_radius = AreaRadius;
                    }
                });
            }

            /// <summary>Critical Strike and Penetrate on a hit the local player makes on a creature.</summary>
            private static void OwnHit(Player player, Character target, HitData hit)
            {
                var d = hit.m_damage;
                if (d.m_blunt + d.m_slash + d.m_pierce > 0f && Roll(Chance(player, "crit")))
                {
                    hit.m_damage.m_blunt *= PassiveRules.CritMultiplier;
                    hit.m_damage.m_slash *= PassiveRules.CritMultiplier;
                    hit.m_damage.m_pierce *= PassiveRules.CritMultiplier;
                    target.m_critHitEffects.Create(hit.m_point, Quaternion.identity, target.transform);
                    Tell("Critical Strike");
                }
                if (Roll(Chance(player, "penetrate")))
                {
                    hit.m_eitrAdd = PassiveRules.Mark(hit.m_eitrAdd);
                    Tell("Penetrate");
                }
            }

            /// <summary>A creature's hit from its area attack or a breath projectile, on the creature's own game.</summary>
            private static bool IsAreaOrBreath(Character attacker)
            {
                if (s_areaAttack != null && s_areaAttack.m_character == attacker)
                {
                    return true;
                }
                var item = s_projectile ? (s_projectile.m_weapon ?? s_projectile.m_ammo) : null;
                return item != null && item.m_dropPrefab && PassiveRules.IsBreath(item.m_dropPrefab.name);
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.DoAreaAttack))]
        private static class AreaAttack
        {
            private static void Prefix(Attack __instance)
            {
                s_areaAttack = __instance;
            }

            private static void Finalizer()
            {
                s_areaAttack = null;
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class ProjectileHit
        {
            private static void Prefix(Projectile __instance)
            {
                s_projectile = __instance;
            }

            private static void Finalizer()
            {
                s_projectile = null;
            }
        }

        /// <summary>
        ///     The receiving side, on the game that owns the character hit. Runs first, so an avoided hit never reaches
        ///     vanilla or Corruption's damage changes, and Penetrate's marker is taken off before anything reads the hit.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class ReceiverSide
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(Character __instance, HitData __1)
            {
                var hit = __1;
                if (hit == null || !__instance)
                {
                    return true;
                }
                if (PassiveRules.IsMarked(hit.m_eitrAdd))
                {
                    hit.m_eitrAdd = PassiveRules.Unmark(hit.m_eitrAdd);
                    if (!__instance.IsPlayer())
                    {
                        s_penetrating = hit;
                    }
                }
                return Safe.Try("apply Dodge, Avoid and Evade", () => !(__instance == Player.m_localPlayer && Avoided(Player.m_localPlayer, hit)), true);
            }

            private static void Finalizer()
            {
                s_penetrating = null;
            }

            /// <summary>Rolls the avoid that can meet this hit; true (and the word and whoosh) if it is avoided.</summary>
            private static bool Avoided(Player player, HitData hit)
            {
                if (player.IsDead() || (hit.m_dodgeable && player.IsDodgeInvincible()))
                {
                    return false;
                }
                var attacker = hit.GetAttacker();
                bool creatureHit = hit.m_hitType == HitData.HitType.EnemyHit && attacker && !attacker.IsPlayer();
                bool moving = player.GetMoveDir().magnitude > 0.1f && !player.InAttack();
                // Vanilla's block takes a blockable hit from the front with a blocker in hand (Humanoid.BlockAttack).
                bool blockTakesIt = player.IsBlocking() && hit.m_blockable && player.GetCurrentBlocker() != null
                    && Vector3.Dot(hit.m_dir, player.transform.forward) <= 0f;
                var kind = PassiveRules.Which(creatureHit, hit.m_dodgeable, hit.m_radius > 0f, hit.m_ranged, moving, blockTakesIt);
                if (kind == AvoidKind.None || !Roll(Chance(player, PassiveRules.Effect(kind))))
                {
                    return false;
                }
                string word = PassiveRules.Word(kind);
                DamageText.instance?.ShowText(DamageText.TextType.Normal, hit.m_point == Vector3.zero ? player.GetTopPoint() : hit.m_point, word);
                player.m_dodgeEffects.Create(player.transform.position, Quaternion.identity, player.transform);
                Tell(word);
                return true;
            }
        }

        /// <summary>Penetrate: on the creature owner's game, a marked hit's resistances become full damage (not immunity).</summary>
        [HarmonyPatch(typeof(HitData), nameof(HitData.ApplyResistance))]
        private static class Penetrate
        {
            private static void Prefix(HitData __instance, ref HitData.DamageModifiers __0)
            {
                if (s_penetrating == null || __instance != s_penetrating)
                {
                    return;
                }
                var m = __0;
                Lift(ref m.m_blunt);
                Lift(ref m.m_slash);
                Lift(ref m.m_pierce);
                Lift(ref m.m_chop);
                Lift(ref m.m_pickaxe);
                Lift(ref m.m_fire);
                Lift(ref m.m_frost);
                Lift(ref m.m_lightning);
                Lift(ref m.m_poison);
                Lift(ref m.m_spirit);
                __0 = m;
            }

            private static void Lift(ref HitData.DamageModifier modifier)
            {
                if (PassiveRules.PenetrateLifts(modifier.ToString()))
                {
                    modifier = HitData.DamageModifier.Normal;
                }
            }
        }
    }
}
