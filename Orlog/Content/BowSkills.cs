using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The Amazon's bow skills (roadmap C5; <c>class-design.md</c> §5), applied to the arrows a skill attack fires on
    ///     the shooter's game (<see cref="SkillButton" />). Numbers per skill level from
    ///     <c>Orlog/Data/amazon_skill_values.csv</c>; added damage in D2 points × <see cref="SkillBar.DamageScaleFor" />.
    ///     <list type="bullet">
    ///         <item>Multiple Shot: several arrows in an even fan for one arrow, each at 75% damage.</item>
    ///         <item>Freezing Arrow: on impact, frost and a freeze (<see cref="FreezeName" />) on every enemy within the radius; bosses are only slowed by the frost.</item>
    ///     </list>
    ///     A shot's arrows carry a <see cref="ShotTag" />; impacts are handled on the shooter's game, which runs its projectiles.
    ///     The skill tree overhaul (Krogon, 2026-11-15) removed Magic Arrow, Fire Arrow, Cold Arrow, Ice Arrow and Exploding
    ///     Arrow with their patches (the last version with them: commit 6090e71; Exploding Arrow's fire blast is the start for
    ///     Immolation Arrow).
    /// </summary>
    internal static class BowSkills
    {
        /// <summary>Our freeze: a copy of the game's Immobilized (speed −1000), lasting the skill's freeze time.</summary>
        public const string FreezeName = "Orlog_Freeze";
        public static readonly int FreezeHash = FreezeName.GetStableHashCode();

        private static SkillButton.Shot s_firing;
        private static int s_fanIndex;
        private static int s_fanCount;
        private static readonly List<Character> s_inRange = new List<Character>();

        /// <summary>On a skill arrow: which skill shot it, at what level and damage scale.</summary>
        internal sealed class ShotTag : MonoBehaviour
        {
            public string SkillId;
            public int Level;
            public float Scale;
            public bool Exploded;
        }

        // ------------------------------------------------------------ the freeze status effect

        /// <summary>Registers the freeze (and Inner Sight's mark) once the game's status effects exist.</summary>
        public static void RegisterStatusEffects()
        {
            // A copy of the game's Immobilized when ObjectDB has it here (on 1.0.21 it wasn't found at this point:
            // Krogon's log, 2026-11-13), else our own with the same effect (speed −1000, no jumping).
            var immobilized = ObjectDB.instance.m_StatusEffects.FirstOrDefault(se => se &&
                (se.name == "Immobilized" || se.name.StartsWith("Immobilized", System.StringComparison.OrdinalIgnoreCase)) && se is SE_Stats);
            SE_Stats freeze;
            if (immobilized)
            {
                freeze = (SE_Stats)ScriptableObject.Instantiate(immobilized);
                Jotunn.Logger.LogInfo($"Orlog: the freeze copies the game's {immobilized.name}.");
            }
            else
            {
                freeze = ScriptableObject.CreateInstance<SE_Stats>();
                freeze.m_speedModifier = -1000f;
                freeze.m_jumpModifier = new Vector3(-1f, -1f, -1f);
                freeze.m_ttl = 2f;
                Jotunn.Logger.LogInfo("Orlog: the game's Immobilized isn't loaded yet; the freeze is Orlog's own (speed −1000, no jumping).");
            }
            freeze.name = FreezeName;
            freeze.m_name = "Frozen";
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(freeze, false));
            CastSkills.RegisterStatusEffects();
        }

        /// <summary>
        ///     The freeze and Inner Sight's mark last the seconds carried in the skill level (see where they're added);
        ///     Inner Sight's mark also takes its extra damage % from the item level.
        /// </summary>
        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.SetLevel))]
        private static class Duration
        {
            private static void Postfix(StatusEffect __instance, int itemLevel, float skillLevel)
            {
                if (__instance && skillLevel > 0f && (__instance.NameHash() == FreezeHash || __instance.NameHash() == CastSkills.InnerSightHash))
                {
                    __instance.m_ttl = skillLevel;
                }
                // Inner Sight's % arrives as the item level (hundredths of a %); see CastSkills.Cast.
                if (__instance is CastSkills.InnerSightMark mark)
                {
                    mark.ExtraDamagePercent = itemLevel / 100f;
                }
            }
        }

        // ------------------------------------------------------------ firing

        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        private static class Fire
        {
            private static void Prefix(Attack __instance, out int __state)
            {
                __state = __instance.m_projectiles;
                s_firing = SkillButton.For(__instance);
                s_fanIndex = 0;
                s_fanCount = 1;
                if (s_firing != null && s_firing.Skill.Id == "MultipleShot")
                {
                    var values = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
                    s_fanCount = Mathf.Max(1, Mathf.RoundToInt(values?.Get("MultipleShot", "arrows", s_firing.Level) ?? 1f));
                    __instance.m_projectiles = s_fanCount;
                }
            }

            private static void Finalizer(Attack __instance, int __state)
            {
                __instance.m_projectiles = __state;
                s_firing = null;
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
        private static class Setup
        {
            private static void Prefix(Projectile __instance, Character owner, ref Vector3 velocity, HitData hitData)
            {
                var shot = s_firing;
                if (shot == null || owner != Player.m_localPlayer || hitData == null)
                {
                    return;
                }
                try
                {
                    Apply(__instance, shot, ref velocity, hitData);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("apply a bow skill", ex);
                }
            }

            private static void Apply(Projectile projectile, SkillButton.Shot shot, ref Vector3 velocity, HitData hit)
            {
                var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
                if (v == null)
                {
                    return;
                }
                string id = shot.Skill.Id;
                int level = shot.Level;
                var tag = projectile.gameObject.AddComponent<ShotTag>();
                tag.SkillId = id;
                tag.Level = level;
                tag.Scale = shot.Scale;
                switch (id)
                {
                    case "MultipleShot":
                        hit.m_damage.Modify(v.Get(id, "weapon_damage_pct", level) / 100f);
                        velocity = Quaternion.AngleAxis(ActiveSkillRules.FanAngle(s_fanIndex++, s_fanCount), Vector3.up) * velocity;
                        break;
                }
            }
        }

        private static float Roll(SkillValueTable v, string id, string kind, int level, float scale)
        {
            return ActiveSkillRules.Damage(v.Get(id, kind + "_min", level), v.Get(id, kind + "_max", level), Random.value, scale);
        }

        // ------------------------------------------------------------ impacts

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class Impact
        {
            private static void Postfix(Projectile __instance, Collider collider, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<ShotTag>() : null;
                if (tag == null || tag.Exploded || !IsBlast(tag))
                {
                    return;
                }
                tag.Exploded = true;
                Safe.Run("explode a skill arrow", () =>
                {
                    var hitObject = collider ? Projectile.FindHitObject(collider) : null;
                    Explode(__instance.m_owner, tag, hitPoint, hitObject ? hitObject.GetComponent<Character>() : null);
                });
            }
        }

        private static bool IsBlast(ShotTag tag)
        {
            return tag.SkillId == "FreezingArrow";
        }

        /// <summary>The hit adds the freeze; the seconds travel in the hit's skill level (read in <see cref="Duration" />).</summary>
        private static void AddFreeze(HitData hit, float seconds)
        {
            if (seconds > 0f && ObjectDB.instance.GetStatusEffect(FreezeHash))
            {
                hit.m_statusEffectHash = FreezeHash;
                hit.m_skillLevel = seconds;
            }
        }

        /// <summary>
        ///     The creatures a blast at <paramref name="point" /> reaches: any part of their body (their collider) within
        ///     the radius, and always the creature the arrow struck. The game's <c>Character.GetCharactersInRange</c>
        ///     measures to the feet, so a blast on a troll's chest, 5 m up, missed the troll (Krogon's log, 2026-11-15).
        /// </summary>
        private static void CharactersInBlast(Vector3 point, float radius, Character struck, List<Character> into)
        {
            into.Clear();
            if (struck)
            {
                into.Add(struck);
            }
            foreach (var c in Character.GetAllCharacters())
            {
                if (!c || c == struck)
                {
                    continue;
                }
                Vector3 nearest = c.m_collider ? c.m_collider.ClosestPoint(point) : c.transform.position;
                if ((nearest - point).sqrMagnitude <= radius * radius)
                {
                    into.Add(c);
                }
            }
        }

        private static void Explode(Character owner, ShotTag tag, Vector3 point, Character struck)
        {
            if (!owner || owner != Player.m_localPlayer)
            {
                return;
            }
            var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
            if (v == null)
            {
                return;
            }
            float radius = v.Get(tag.SkillId, "radius_yards", tag.Level) * ActiveSkillRules.MetresPerYard;
            float freeze = v.Get(tag.SkillId, "freeze_s", tag.Level);
            ImpactEffect("StaffIceShards", "ArrowFrost", point);
            CharactersInBlast(point, radius, struck, s_inRange);
            foreach (var target in s_inRange)
            {
                if (!target || target == owner || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(owner, target))
                {
                    continue;
                }
                var hit = new HitData();
                hit.m_damage.m_frost = Roll(v, tag.SkillId, "cold_damage", tag.Level, tag.Scale);
                if (!target.IsBoss())
                {
                    AddFreeze(hit, freeze);
                }
                hit.m_point = target.GetCenterPoint();
                hit.m_dir = (target.transform.position - point).normalized;
                hit.m_radius = radius;
                hit.m_ranged = true;
                hit.m_skill = Skills.SkillType.Bows;
                hit.m_hitType = HitData.HitType.PlayerHit;
                hit.SetAttacker(owner);
                target.Damage(hit);
            }
        }

        /// <summary>
        ///     The impact effect of a vanilla projectile, played where an arrow explodes: the staff's own (ice shards),
        ///     else the elemental arrow's.
        /// </summary>
        private static void ImpactEffect(string item, string fallback, Vector3 point)
        {
            foreach (string name in new[] { item, fallback })
            {
                var prefab = ObjectDB.instance.GetItemPrefab(name);
                var projectile = prefab ? prefab.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_attack.m_attackProjectile : null;
                var effects = projectile ? projectile.GetComponent<Projectile>()?.m_hitEffects : null;
                if (effects != null && effects.HasEffects())
                {
                    effects.Create(point, Quaternion.identity);
                    return;
                }
            }
        }
    }
}
