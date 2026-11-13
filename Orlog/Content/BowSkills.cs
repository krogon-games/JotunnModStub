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
    ///         <item>Magic Arrow: needs no arrow (a copy of the cheapest arrow for the bow's ammo type flies instead), adds damage and turns part of the physical damage into spirit.</item>
    ///         <item>Fire Arrow, Cold Arrow: add fire or frost and turn part of the physical damage into it.</item>
    ///         <item>Multiple Shot: several arrows in an even fan for one arrow, each at 75% damage.</item>
    ///         <item>Ice Arrow: adds frost and freezes the creature hit (<see cref="FreezeName" />); bosses are only slowed by the frost.</item>
    ///         <item>Exploding Arrow, Freezing Arrow: on impact, fire or frost (and a freeze) on every enemy within the radius.</item>
    ///     </list>
    ///     A shot's arrows carry a <see cref="ShotTag" />; impacts are handled on the shooter's game, which runs its projectiles.
    /// </summary>
    internal static class BowSkills
    {
        /// <summary>Our freeze: a copy of the game's Immobilized (speed −1000), lasting the skill's freeze time.</summary>
        public const string FreezeName = "Orlog_Freeze";
        public static readonly int FreezeHash = FreezeName.GetStableHashCode();

        private static SkillButton.Shot s_firing;
        private static int s_fanIndex;
        private static int s_fanCount;
        private static Projectile s_hitting;
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

        /// <summary>The freeze and Inner Sight's mark last the seconds carried in the hit's skill level (see where they're added).</summary>
        [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.SetLevel))]
        private static class Duration
        {
            private static void Postfix(StatusEffect __instance, float skillLevel)
            {
                if (__instance && skillLevel > 0f && (__instance.NameHash() == FreezeHash || __instance.NameHash() == CastSkills.InnerSightHash))
                {
                    __instance.m_ttl = skillLevel;
                }
            }
        }

        // ------------------------------------------------------------ Magic Arrow: no arrow needed

        /// <summary>The cheapest arrow (or bolt) for a weapon's ammo type, as a loose copy that is never in an inventory.</summary>
        private static ItemDrop.ItemData FreeAmmo(ItemDrop.ItemData weapon)
        {
            string type = weapon?.m_shared?.m_ammoType;
            if (string.IsNullOrEmpty(type) || !ObjectDB.instance)
            {
                return null;
            }
            var best = ObjectDB.instance.m_items
                .Select(go => go ? go.GetComponent<ItemDrop>() : null)
                .Where(d => d && d.m_itemData.m_shared.m_ammoType == type && d.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo
                            && d.m_itemData.m_shared.m_attack.m_attackProjectile)
                .OrderBy(d => d.m_itemData.m_shared.m_damages.GetTotalDamage())
                .FirstOrDefault();
            if (!best)
            {
                return null;
            }
            var copy = best.m_itemData.Clone();
            copy.m_dropPrefab = best.gameObject;
            copy.m_stack = 1;
            return copy;
        }

        private static bool MagicArrowFor(Humanoid character)
        {
            return character && character == Player.m_localPlayer && SkillButton.IsPending("MagicArrow");
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
        private static class HaveAmmo
        {
            private static bool Prefix(Humanoid character, ref bool __result)
            {
                if (!MagicArrowFor(character))
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.EquipAmmoItem))]
        private static class EquipAmmo
        {
            private static bool Prefix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
            {
                // With no arrows at all the game's own code would fail; Magic Arrow needs none.
                if (!MagicArrowFor(character) || character.GetInventory().GetAmmoItem(weapon.m_shared.m_ammoType) != null)
                {
                    return true;
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo))]
        private static class UseAmmo
        {
            private static bool Prefix(Attack __instance, ref ItemDrop.ItemData ammoItem, ref bool __result)
            {
                var shot = SkillButton.For(__instance);
                if (shot == null || shot.Skill.Id != "MagicArrow")
                {
                    return true;
                }
                var free = Safe.Try("make Magic Arrow's arrow", () => FreeAmmo(__instance.m_weapon), null);
                if (free == null)
                {
                    return true;
                }
                ammoItem = free;
                __instance.m_ammoItem = free;
                __result = true;
                return false;
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
                    case "MagicArrow":
                        hit.m_damage.m_pierce += v.Get(id, "damage_flat", level) * shot.Scale;
                        Convert(ref hit.m_damage, v.Get(id, "phys_to_magic_pct", level), ref hit.m_damage.m_spirit);
                        break;
                    case "FireArrow":
                        hit.m_damage.m_fire += Roll(v, id, "fire_damage", level, shot.Scale);
                        Convert(ref hit.m_damage, v.Get(id, "phys_to_elemental_pct", level), ref hit.m_damage.m_fire);
                        break;
                    case "ColdArrow":
                        hit.m_damage.m_frost += Roll(v, id, "cold_damage", level, shot.Scale);
                        Convert(ref hit.m_damage, v.Get(id, "phys_to_elemental_pct", level), ref hit.m_damage.m_frost);
                        break;
                    case "IceArrow":
                        hit.m_damage.m_frost += Roll(v, id, "cold_damage", level, shot.Scale);
                        break;
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

        /// <summary>Moves a share of the physical damage (blunt, slash, pierce) into another type.</summary>
        private static void Convert(ref HitData.DamageTypes d, float percent, ref float into)
        {
            float blunt = ActiveSkillRules.Converted(d.m_blunt, percent);
            float slash = ActiveSkillRules.Converted(d.m_slash, percent);
            float pierce = ActiveSkillRules.Converted(d.m_pierce, percent);
            d.m_blunt -= blunt;
            d.m_slash -= slash;
            d.m_pierce -= pierce;
            into += blunt + slash + pierce;
        }

        // ------------------------------------------------------------ impacts

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class Impact
        {
            private static void Prefix(Projectile __instance)
            {
                s_hitting = __instance;
            }

            private static void Postfix(Projectile __instance, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<ShotTag>() : null;
                if (tag == null || tag.Exploded || (tag.SkillId != "ExplodingArrow" && tag.SkillId != "FreezingArrow"))
                {
                    return;
                }
                tag.Exploded = true;
                Safe.Run("explode a skill arrow", () => Explode(__instance.m_owner, tag, hitPoint));
            }

            private static void Finalizer()
            {
                s_hitting = null;
            }
        }

        /// <summary>Ice Arrow's direct hit freezes (not bosses), on the shooter's game before the hit is sent.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static class DirectHit
        {
            private static void Prefix(Character __instance, HitData hit)
            {
                var tag = s_hitting ? s_hitting.GetComponent<ShotTag>() : null;
                if (tag == null || tag.SkillId != "IceArrow" || hit == null || !__instance || __instance.IsPlayer() || __instance.IsBoss())
                {
                    return;
                }
                var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
                AddFreeze(hit, v?.Get("IceArrow", "freeze_s", tag.Level) ?? 0f);
            }
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

        private static void Explode(Character owner, ShotTag tag, Vector3 point)
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
            bool fire = tag.SkillId == "ExplodingArrow";
            float radius = (fire ? ActiveSkillRules.ExplodingRadiusYards : v.Get(tag.SkillId, "radius_yards", tag.Level)) * ActiveSkillRules.MetresPerYard;
            float freeze = fire ? 0f : v.Get(tag.SkillId, "freeze_s", tag.Level);
            ImpactEffect(fire ? "StaffFireball" : "StaffIceShards", fire ? "ArrowFire" : "ArrowFrost", point);
            s_inRange.Clear();
            Character.GetCharactersInRange(point, radius, s_inRange);
            foreach (var target in s_inRange)
            {
                if (!target || target == owner || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(owner, target))
                {
                    continue;
                }
                var hit = new HitData();
                if (fire)
                {
                    hit.m_damage.m_fire = Roll(v, tag.SkillId, "fire_damage", tag.Level, tag.Scale);
                }
                else
                {
                    hit.m_damage.m_frost = Roll(v, tag.SkillId, "cold_damage", tag.Level, tag.Scale);
                    if (!target.IsBoss())
                    {
                        AddFreeze(hit, freeze);
                    }
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
        ///     The impact effect of a vanilla projectile, played where an arrow explodes: the staff's own (fireball, ice
        ///     shards), else the elemental arrow's.
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
