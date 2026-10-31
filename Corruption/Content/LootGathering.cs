using System.Collections.Generic;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Gathering-tool affixes that change a strike on the striker's own game, before the hit is sent to the
    ///     object's owner (AFFIX_RULES.md → Gathering tool affixes → Multiplayer). On-harvest effects (Bountiful,
    ///     Artisan, Odin's Grace on trees…) are applied by the object's owner: <see cref="LootHarvest" />.
    /// </summary>
    internal static class LootGathering
    {
        /// <summary>Damage of a "harvest in one strike" hit (Odin's Grace): more than any rock or ore part has.</summary>
        private const float KillingDamage = 1e6f;

        /// <summary>
        ///     Margin added to a deposit part's bounds when looking for the parts touching it, so parts that share a
        ///     face count (AFFIX_RULES: "touching" = the chunks' colliders overlap the hit chunk's bounds).
        /// </summary>
        private const float TouchMargin = 0.05f;

        private static readonly HashSet<string> StumpPrefabs = new HashSet<string>();
        private static readonly GatheringRules.HitCounter Veinbreaker = new GatheringRules.HitCounter();
        private static readonly System.Func<double> Random01 = () => UnityEngine.Random.value * 0.99999f;

        /// <summary>The local player's current melee swing, while <c>Attack.DoMeleeAttack</c> runs.</summary>
        private static class Swing
        {
            public static bool Active;
            public static ItemDrop.ItemData Tool;
            public static readonly HashSet<HitData> Processed = new HashSet<HitData>();
            public static bool DepositRolled;
            public static bool Fifth;
            public static bool Seismic;
            public static bool OdinsGrace;
            public static bool SplashRolled;
            public static float DurabilityBefore;
        }

        /// <summary>Stumps are the prefabs felled trees leave (<c>TreeBase.m_stubPrefab</c>), read from the game's trees.</summary>
        public static void CollectStumps()
        {
            StumpPrefabs.Clear();
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(TreeBase)).Values)
            {
                var tree = obj as TreeBase;
                if (tree && tree.m_stubPrefab)
                {
                    StumpPrefabs.Add(tree.m_stubPrefab.name);
                }
            }
            Jotunn.Logger.LogInfo($"Gathering affixes: {StumpPrefabs.Count} stump prefabs from the game's trees.");
        }

        private static float Total(string effect) => Loot.Total(Swing.Tool, effect);

        /// <summary>The tool of the local player's melee swing in progress, or null (read by <see cref="LootHarvest" />).</summary>
        internal static ItemDrop.ItemData SwingTool => Swing.Active ? Swing.Tool : null;

        /// <summary>Counts the local player's melee swings, so once-per-swing rolls elsewhere know when a new one starts.</summary>
        internal static int SwingNumber { get; private set; }

        /// <summary>
        ///     How deep we are in the five targets' <c>Damage</c> calls. When the striker also owns the object, the
        ///     owner's code runs inside the striker's call and can hit another object with a deserialized copy of the
        ///     same hit (a rock breaking into a copper deposit, <c>Destructible.Destroy</c>): such nested calls are the
        ///     owner's, already scaled, and are left alone.
        /// </summary>
        private static int _damageDepth;

        /// <summary>
        ///     True the first time this hit is seen during the local player's swing, outside any nested target
        ///     <c>Damage</c> call; marks it as handled.
        /// </summary>
        private static bool Take(HitData hit)
        {
            return _damageDepth == 0 && Swing.Active && hit != null && Swing.Tool != null && Swing.Processed.Add(hit);
        }

        private static void Enter() => _damageDepth++;

        private static void Leave() => _damageDepth = System.Math.Max(0, _damageDepth - 1);

        // ---- The swing: who is striking, with what ----

        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        private static class MeleeSwing
        {
            private static void Prefix(Attack __instance)
            {
                if (__instance.m_character != Player.m_localPlayer || __instance.m_weapon == null)
                {
                    return;
                }
                Swing.Active = true;
                _damageDepth = 0;
                SwingNumber++;
                Swing.Tool = __instance.m_weapon;
                Swing.Processed.Clear();
                Swing.DepositRolled = Swing.Fifth = Swing.Seismic = Swing.OdinsGrace = Swing.SplashRolled = false;
                Swing.DurabilityBefore = __instance.m_weapon.m_durability;
            }

            private static void Finalizer(Attack __instance)
            {
                if (!Swing.Active)
                {
                    return;
                }
                Safe.Run("apply Careful", () =>
                {
                    // Careful: a chance the hit costs no durability (vanilla drains it at the end of DoMeleeAttack).
                    var tool = Swing.Tool;
                    if (tool.m_durability < Swing.DurabilityBefore && GatheringRules.Roll(Total("durability_save_chance"), Random01))
                    {
                        tool.m_durability = Swing.DurabilityBefore;
                    }
                });
                Swing.Active = false;
                Swing.Tool = null;
                Swing.Processed.Clear();
            }
        }

        /// <summary>
        ///     The rolls a mining swing makes once (one swing can hit several deposit parts): Veinbreaker's count,
        ///     Seismic, Odin's Grace.
        /// </summary>
        private static void RollDepositSwing()
        {
            if (Swing.DepositRolled)
            {
                return;
            }
            Swing.DepositRolled = true;
            if (Total("every_fifth_hit_damage") > 0f)
            {
                Swing.Fifth = Veinbreaker.NextSwing(Swing.Tool);
            }
            Swing.Seismic = GatheringRules.Roll(Total("deposit_area_hit"), Random01);
            Swing.OdinsGrace = GatheringRules.Roll(Total("harvest_instantly"), Random01);
        }

        /// <summary>Prospector's and Veinbreaker's on a hit against rock or ore.</summary>
        private static void ScaleDepositHit(HitData hit)
        {
            RollDepositSwing();
            float scale = 1f + Total("pickaxe_damage_deposits") / 100f;
            if (Swing.Fifth)
            {
                scale *= 1f + Total("every_fifth_hit_damage") / 100f;
            }
            hit.m_damage.m_pickaxe *= scale;
        }

        private static HitData KillingHit(HitData hit)
        {
            var kill = hit.Clone();
            kill.m_damage = new HitData.DamageTypes { m_pickaxe = KillingDamage };
            kill.m_radius = 0f;
            Swing.Processed.Add(kill);
            return kill;
        }

        private static List<int> TouchingAreas(Collider hitArea, IList<Collider> areas)
        {
            var touching = new List<int>();
            if (!hitArea)
            {
                return touching;
            }
            var bounds = hitArea.bounds;
            bounds.Expand(TouchMargin * 2f);
            for (int i = 0; i < areas.Count; i++)
            {
                var area = areas[i];
                if (area && area != hitArea && area.enabled && area.gameObject.activeInHierarchy && bounds.Intersects(area.bounds))
                {
                    touching.Add(i);
                }
            }
            return touching;
        }

        // ---- Rock and ore ----

        [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Damage))]
        private static class MultiPartDeposit
        {
            private static void Prefix(HitData hit, out bool __state)
            {
                __state = Take(hit);
                Enter();
                if (__state)
                {
                    ScaleDepositHit(hit);
                }
            }

            private static void Finalizer() => Leave();

            /// <summary>Seismic and Odin's Grace: extra hits on the parts touching the one struck, sent after the original.</summary>
            private static void Postfix(MineRock5 __instance, HitData hit, bool __state)
            {
                if (!__state || !(Swing.Seismic || Swing.OdinsGrace))
                {
                    return;
                }
                var tool = Swing.Tool;
                bool seismic = Swing.Seismic, odin = Swing.OdinsGrace;
                Swing.Seismic = Swing.OdinsGrace = false;
                Safe.Run("apply Seismic and Odin's Grace on a deposit", () =>
                {
                    if (__instance.m_nview == null || !__instance.m_nview.IsValid() || __instance.m_hitAreas == null)
                    {
                        return;
                    }
                    int hitIndex = hit.m_hitCollider ? __instance.GetAreaIndex(hit.m_hitCollider) : -1;
                    if (hitIndex < 0)
                    {
                        return;
                    }
                    var colliders = new List<Collider>();
                    foreach (var area in __instance.m_hitAreas)
                    {
                        colliders.Add(area.m_collider);
                    }
                    var touching = TouchingAreas(colliders[hitIndex], colliders);
                    if (odin)
                    {
                        __instance.m_nview.InvokeRPC("RPC_Damage", KillingHit(hit), hitIndex);
                    }
                    foreach (int index in touching)
                    {
                        HitData extra;
                        if (odin)
                        {
                            extra = KillingHit(hit);
                        }
                        else
                        {
                            extra = hit.Clone();
                            extra.m_radius = 0f;
                            Swing.Processed.Add(extra);
                        }
                        __instance.m_nview.InvokeRPC("RPC_Damage", extra, index);
                    }
                });
            }
        }

        [HarmonyPatch(typeof(MineRock), nameof(MineRock.Damage))]
        private static class SinglePartDeposit
        {
            private static void Prefix(HitData hit, out bool __state)
            {
                __state = Take(hit);
                Enter();
                if (__state)
                {
                    ScaleDepositHit(hit);
                }
            }

            private static void Finalizer() => Leave();

            private static void Postfix(MineRock __instance, HitData hit, bool __state)
            {
                if (!__state || !(Swing.Seismic || Swing.OdinsGrace))
                {
                    return;
                }
                bool odin = Swing.OdinsGrace;
                Swing.Seismic = Swing.OdinsGrace = false;
                Safe.Run("apply Seismic and Odin's Grace on a rock", () =>
                {
                    if (__instance.m_nview == null || !__instance.m_nview.IsValid() || __instance.m_hitAreas == null)
                    {
                        return;
                    }
                    int hitIndex = hit.m_hitCollider ? __instance.GetAreaIndex(hit.m_hitCollider) : -1;
                    if (hitIndex < 0)
                    {
                        return;
                    }
                    if (odin)
                    {
                        __instance.m_nview.InvokeRPC("Hit", KillingHit(hit), hitIndex);
                    }
                    foreach (int index in TouchingAreas(__instance.m_hitAreas[hitIndex], __instance.m_hitAreas))
                    {
                        var extra = odin ? KillingHit(hit) : hit.Clone();
                        Swing.Processed.Add(extra);
                        __instance.m_nview.InvokeRPC("Hit", extra, index);
                    }
                });
            }
        }

        /// <summary>Rocks that are one piece (Destructible, type Default), and stumps (Destructible, type Tree).</summary>
        [HarmonyPatch(typeof(Destructible), nameof(Destructible.Damage))]
        private static class DestructibleHit
        {
            private static void Finalizer() => Leave();

            private static void Prefix(Destructible __instance, HitData hit)
            {
                bool take = Take(hit);
                Enter();
                if (!take)
                {
                    return;
                }
                Safe.Run("apply gathering affixes to a rock or stump", () =>
                {
                    if (__instance.GetDestructibleType() == DestructibleType.Tree)
                    {
                        if (StumpPrefabs.Contains(Utils.GetPrefabName(__instance.gameObject)))
                        {
                            hit.m_damage.m_chop *= 1f + Total("chop_damage_stumps") / 100f;
                        }
                        return;
                    }
                    if (hit.m_damage.m_pickaxe <= 0f)
                    {
                        return;
                    }
                    ScaleDepositHit(hit);
                    if (Swing.OdinsGrace)
                    {
                        Swing.OdinsGrace = false;
                        hit.m_damage.m_pickaxe = KillingDamage;
                    }
                });
            }
        }

        // ---- Trees and logs ----

        [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.Damage))]
        private static class StandingTree
        {
            private static void Prefix(HitData hit, out bool __state)
            {
                __state = Take(hit);
                Enter();
                if (__state)
                {
                    hit.m_damage.m_chop *= 1f + Total("chop_damage_trees") / 100f;
                }
            }

            private static void Finalizer() => Leave();

            private static void Postfix(TreeBase __instance, HitData hit, bool __state)
            {
                if (__state)
                {
                    Splinter(__instance.gameObject, hit);
                }
            }
        }

        [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Damage))]
        private static class FallenLog
        {
            private static void Prefix(HitData hit, out bool __state)
            {
                __state = Take(hit);
                Enter();
                if (__state)
                {
                    hit.m_damage.m_chop *= 1f + Total("chop_damage_logs") / 100f;
                }
            }

            private static void Finalizer() => Leave();

            private static void Postfix(TreeLog __instance, HitData hit, bool __state)
            {
                if (__state)
                {
                    Splinter(__instance.gameObject, hit);
                }
            }
        }

        /// <summary>
        ///     Splintering: once per swing, a chance the same hit also lands on the nearest other tree or log within the
        ///     axe's own attack range (AFFIX_RULES → Interpretations).
        /// </summary>
        private static void Splinter(GameObject struck, HitData hit)
        {
            if (Swing.SplashRolled)
            {
                return;
            }
            Swing.SplashRolled = true;
            if (!GatheringRules.Roll(Total("chop_splash"), Random01))
            {
                return;
            }
            var tool = Swing.Tool;
            Safe.Run("apply Splintering", () =>
            {
                float range = tool.m_shared.m_attack != null ? tool.m_shared.m_attack.m_attackRange : 1.5f;
                IDestructible nearest = null;
                float best = float.MaxValue;
                foreach (var collider in Physics.OverlapSphere(hit.m_point, range))
                {
                    var tree = collider.GetComponentInParent<TreeBase>();
                    var log = tree ? null : collider.GetComponentInParent<TreeLog>();
                    var target = tree ? tree.gameObject : log ? log.gameObject : null;
                    if (!target || target == struck)
                    {
                        continue;
                    }
                    float distance = Vector3.Distance(hit.m_point, collider.ClosestPoint(hit.m_point));
                    if (distance < best)
                    {
                        best = distance;
                        nearest = tree ? (IDestructible)tree : log;
                    }
                }
                if (nearest == null)
                {
                    return;
                }
                var copy = hit.Clone();
                copy.m_hitCollider = null;
                Swing.Processed.Add(copy);
                nearest.Damage(copy);
            });
        }

        // ---- Digging ----

        /// <summary>Excavator's: a deeper and wider dig (a scaled copy of the pickaxe's own terrain change).</summary>
        [HarmonyPatch(typeof(Attack), nameof(Attack.SpawnOnHitTerrain))]
        private static class Digging
        {
            private sealed class Saved
            {
                public TerrainOp Op;
                public float LevelOffset, LevelRadius, RaiseRadius, RaiseDelta, SmoothRadius, PaintRadius;
            }

            private static void Prefix(GameObject prefab, Character character, ItemDrop.ItemData weapon, out Saved __state)
            {
                __state = null;
                if (character != Player.m_localPlayer || weapon == null || !prefab)
                {
                    return;
                }
                float more = Loot.Total(weapon, "terrain_dig") / 100f;
                var op = more > 0f ? prefab.GetComponentInChildren<TerrainOp>() : null;
                if (!op)
                {
                    return;
                }
                var s = op.m_settings;
                __state = new Saved
                {
                    Op = op, LevelOffset = s.m_levelOffset, LevelRadius = s.m_levelRadius, RaiseRadius = s.m_raiseRadius,
                    RaiseDelta = s.m_raiseDelta, SmoothRadius = s.m_smoothRadius, PaintRadius = s.m_paintRadius
                };
                float scale = 1f + more;
                s.m_levelOffset *= scale;
                s.m_levelRadius *= scale;
                s.m_raiseRadius *= scale;
                s.m_raiseDelta *= scale;
                s.m_smoothRadius *= scale;
                s.m_paintRadius *= scale;
            }

            private static void Finalizer(Saved __state)
            {
                if (__state == null || !__state.Op)
                {
                    return;
                }
                var s = __state.Op.m_settings;
                s.m_levelOffset = __state.LevelOffset;
                s.m_levelRadius = __state.LevelRadius;
                s.m_raiseRadius = __state.RaiseRadius;
                s.m_raiseDelta = __state.RaiseDelta;
                s.m_smoothRadius = __state.SmoothRadius;
                s.m_paintRadius = __state.PaintRadius;
            }
        }

        // ---- Stamina, noise, weight, pickup, speed, skill ----

        private static bool _skipStamina;
        private static bool _inLocalAttack;

        /// <summary>
        ///     Fit: a chance a swing costs no stamina. Vanilla spends the swing's stamina in <c>Attack.Update</c> on the
        ///     first frame of the attack; the roll happens there and <c>UseStamina</c> is skipped for that call.
        ///     The same window marks the swing's noise for Hushed.
        /// </summary>
        [HarmonyPatch(typeof(Attack), nameof(Attack.Update))]
        private static class AttackFrame
        {
            private static void Prefix(Attack __instance)
            {
                if (__instance.m_character != Player.m_localPlayer || __instance.m_weapon == null)
                {
                    return;
                }
                _inLocalAttack = true;
                if (__instance.m_character.InAttack() && !__instance.m_wasInAttack)
                {
                    _skipStamina = GatheringRules.Roll(Loot.Total(__instance.m_weapon, "stamina_save_chance"), Random01);
                }
            }

            private static void Finalizer()
            {
                _skipStamina = false;
                _inLocalAttack = false;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
        private static class FreeSwing
        {
            private static bool Prefix(Player __instance)
            {
                if (_skipStamina && __instance == Player.m_localPlayer)
                {
                    _skipStamina = false;
                    return false;
                }
                return true;
            }
        }

        /// <summary>Hushed: quieter strikes. Scales the noise the local player's own swings and hits make.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.AddNoise))]
        private static class QuietStrikes
        {
            private static void Prefix(Character __instance, ref float range)
            {
                if ((_inLocalAttack || Swing.Active) && __instance == Player.m_localPlayer)
                {
                    var tool = Swing.Tool ?? Player.m_localPlayer.GetCurrentWeapon();
                    range *= Mathf.Max(0f, 1f - Loot.Total(tool, "hit_noise_reduction") / 100f);
                }
            }
        }

        /// <summary>Light: a lighter tool. Multiplies, so AdventureBackpacks' own weight postfix still applies.</summary>
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
        private static class LighterTool
        {
            private static void Postfix(ItemDrop.ItemData __instance, ref float __result)
            {
                float lighter = Loot.Total(__instance, "item_weight_reduction");
                if (lighter > 0f)
                {
                    __result *= Mathf.Max(0f, 1f - lighter / 100f);
                }
            }
        }

        /// <summary>Magnetic and Keen-eyed: a larger pickup radius while the tool is held (field swapped for the call).</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.AutoPickup))]
        private static class PickupRadius
        {
            private static void Prefix(Player __instance, out float __state)
            {
                __state = -1f;
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                var tool = __instance.GetCurrentWeapon();
                float set = Loot.Total(tool, "pickup_radius_set");
                float add = Loot.Total(tool, "pickup_range");
                if (set <= 0f && add <= 0f)
                {
                    return;
                }
                __state = __instance.m_autoPickupRange;
                float cap = AffixTotals.Cap(float.MaxValue, "magnetic", Loot.Caps);
                __instance.m_autoPickupRange = GatheringRules.PickupRadius(__state, set, add, cap < float.MaxValue ? cap : 0f);
            }

            private static void Finalizer(Player __instance, float __state)
            {
                if (__state >= 0f)
                {
                    __instance.m_autoPickupRange = __state;
                }
            }
        }

        /// <summary>
        ///     Swift: a faster swing. The game has no attack-speed stat; animation clips set the animator speed through
        ///     <c>CharacterAnimEvent.Speed</c>, and it rests at 1 otherwise. Both are scaled while the local player
        ///     attacks with the tool, capped with the other attack-speed affixes (affix_caps.csv → attack_speed).
        /// </summary>
        [HarmonyPatch]
        private static class FasterSwing
        {
            private static float Factor(CharacterAnimEvent anim)
            {
                if (anim.m_character != Player.m_localPlayer || !anim.m_character.InAttack())
                {
                    return 1f;
                }
                float percent = AffixTotals.Cap(Loot.Total(Player.m_localPlayer.GetCurrentWeapon(), "attack_speed"), "attack_speed", Loot.Caps);
                return 1f + percent / 100f;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterAnimEvent), nameof(CharacterAnimEvent.Speed))]
            private static void ClipSpeed(CharacterAnimEvent __instance, ref float speedScale)
            {
                speedScale *= Factor(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharacterAnimEvent), nameof(CharacterAnimEvent.CustomFixedUpdate))]
            private static void RestingSpeed(CharacterAnimEvent __instance)
            {
                if (__instance.m_animator && Mathf.Approximately(__instance.m_animator.speed, 1f))
                {
                    float factor = Factor(__instance);
                    if (factor > 1f)
                    {
                        __instance.m_animator.speed = factor;
                    }
                }
            }
        }

        /// <summary>Scholar's: more Woodcutting and Pickaxes skill XP from the tool's swings.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.RaiseSkill))]
        private static class GatheringXp
        {
            private static void Prefix(Player __instance, Skills.SkillType skill, ref float value)
            {
                if (__instance == Player.m_localPlayer &&
                    (skill == Skills.SkillType.WoodCutting || skill == Skills.SkillType.Pickaxes))
                {
                    value *= 1f + Loot.Total(__instance.GetCurrentWeapon(), "gathering_skill_xp") / 100f;
                }
            }
        }
    }
}
