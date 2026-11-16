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
    ///         <item>Multishot: several arrows in an even fan for one arrow, each at 75% damage.</item>
    ///         <item>Freezing Arrow: on impact, frost and a freeze (<see cref="FreezeName" />) on every enemy within the radius; bosses are only slowed by the frost.</item>
    ///         <item>Forked Lightning (user 2026-11-16): the arrow hits as usual; the struck enemy takes one fork of lightning and
    ///         forks go to the nearest other enemies within reach of its body (Charged Strike's numbers).</item>
    ///         <item>Wind Lance (user 2026-11-16): a charged shot (<see cref="SkillButton" />). The arrow flies dead straight at a
    ///         multiple of its speed, through every enemy (each hit once, at full damage, with the skill's knockback), and
    ///         is gone after its range; terrain, trees and buildings stop it as they stop an arrow.</item>
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
            /// <summary>Wind Lance: the creatures it has passed through (each is hit once), and whether the shot raised the bow skill yet.</summary>
            public readonly HashSet<Character> Pierced = new HashSet<Character>();
            public bool RaisedSkill;
        }

        /// <summary>Wind Lance: the lance is gone once it has flown its range (on the shooter's game, which runs it).</summary>
        internal sealed class LanceFlight : MonoBehaviour
        {
            public Vector3 From;
            public float Range;

            private void Update()
            {
                if (Range <= 0f || (transform.position - From).sqrMagnitude < Range * Range)
                {
                    return;
                }
                var view = GetComponent<ZNetView>();
                if (view && view.IsValid() && view.IsOwner() && ZNetScene.instance)
                {
                    enabled = false;
                    ZNetScene.instance.Destroy(gameObject);
                }
            }
        }

        /// <summary>The lance's look: the arrow drawn longer and thicker (Claude's choice; no new model), pale, with a light.</summary>
        private static readonly Vector3 LanceSize = new Vector3(2f, 2f, 3f);
        private static readonly Color WindColour = new Color(0.8f, 0.95f, 1f);
        private static readonly Color LightningColour = new Color(0.7f, 0.85f, 1f);

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
                    case "WindLance":
                        hit.m_damage.Modify(v.Get(id, "weapon_damage_pct", level) / 100f);
                        hit.m_pushForce = v.Get(id, "knockback", level);
                        velocity *= v.Get(id, "speed_pct", level) / 100f;
                        projectile.m_gravity = 0f;
                        var flight = projectile.gameObject.AddComponent<LanceFlight>();
                        flight.From = projectile.transform.position;
                        flight.Range = v.Get(id, "range_m", level);
                        DressLance(projectile);
                        // The wind: the game's dodge whoosh, at the shooter.
                        Player.m_localPlayer.m_dodgeEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity);
                        break;
                }
            }
        }

        /// <summary>Makes a skill arrow look like a lance of wind (on the shooter's game only; other players see a fast arrow).</summary>
        private static void DressLance(Projectile projectile)
        {
            projectile.transform.localScale = Vector3.Scale(projectile.transform.localScale, LanceSize);
            foreach (var trail in projectile.GetComponentsInChildren<TrailRenderer>(true))
            {
                trail.widthMultiplier *= LanceSize.x;
                trail.startColor = WindColour;
                trail.endColor = new Color(WindColour.r, WindColour.g, WindColour.b, 0f);
            }
            var glow = new GameObject("Orlog_LanceGlow");
            glow.transform.SetParent(projectile.transform, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = WindColour;
            light.range = 4f;
            light.intensity = 2f;
        }

        // ------------------------------------------------------------ Wind Lance: the charge cue

        /// <summary>A pale light at the bow hand while a charged shot is ready, on the shooter's game; it goes out when the cue stops.</summary>
        private sealed class ChargeGlow : MonoBehaviour
        {
            public float Until;

            private void Update()
            {
                if (Time.time > Until)
                {
                    Destroy(gameObject);
                }
            }
        }

        private static ChargeGlow s_chargeGlow;

        /// <summary>
        ///     The charged shot is ready: a light at the bow hand for as long as this is called every frame, and on the
        ///     first call the game's perfect-block ring as the "ready" sound.
        /// </summary>
        public static void ChargeReadyCue(Player player, bool first)
        {
            if (!player)
            {
                return;
            }
            Safe.Run("show the charge cue", () =>
            {
                Transform hand = player.m_visEquipment ? player.m_visEquipment.m_leftHand : null;
                if (!s_chargeGlow)
                {
                    var go = new GameObject("Orlog_ChargeGlow");
                    go.transform.SetParent(hand ? hand : player.transform, false);
                    if (!hand)
                    {
                        go.transform.position = player.GetCenterPoint();
                    }
                    var light = go.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = WindColour;
                    light.range = 3f;
                    light.intensity = 3f;
                    s_chargeGlow = go.AddComponent<ChargeGlow>();
                }
                s_chargeGlow.Until = Time.time + 0.25f;
                if (first)
                {
                    player.m_perfectBlockEffect.Create(hand ? hand.position : player.GetCenterPoint(), Quaternion.identity);
                }
            });
        }

        private static float Roll(SkillValueTable v, string id, string kind, int level, float scale)
        {
            return ActiveSkillRules.Damage(v.Get(id, kind + "_min", level), v.Get(id, kind + "_max", level), Random.value, scale);
        }

        // ------------------------------------------------------------ impacts

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class Impact
        {
            /// <summary>
            ///     Wind Lance passes through enemies: the lance hits the enemy itself here and the game's own hit (which
            ///     would stop the arrow) is skipped. Anything else (terrain, trees, buildings, players, tamed animals) is
            ///     left to the game.
            /// </summary>
            private static bool Prefix(Projectile __instance, Collider collider, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<ShotTag>() : null;
                if (tag == null || tag.SkillId != "WindLance" || !collider)
                {
                    return true;
                }
                try
                {
                    return !PassThrough(__instance, tag, collider, hitPoint);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("pass a Wind Lance through an enemy", ex);
                    return true;
                }
            }

            private static void Postfix(Projectile __instance, Collider collider, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<ShotTag>() : null;
                if (tag == null || tag.Exploded)
                {
                    return;
                }
                if (IsBlast(tag))
                {
                    tag.Exploded = true;
                    Safe.Run("explode a skill arrow", () =>
                    {
                        var hitObject = collider ? Projectile.FindHitObject(collider) : null;
                        Explode(__instance.m_owner, tag, hitPoint, hitObject ? hitObject.GetComponent<Character>() : null);
                    });
                }
                else if (tag.SkillId == "ForkedLightning")
                {
                    Safe.Run("fork lightning from a skill arrow", () =>
                    {
                        var hitObject = collider ? Projectile.FindHitObject(collider) : null;
                        var struck = hitObject ? hitObject.GetComponent<Character>() : null;
                        var owner = __instance.m_owner;
                        // Only an arrow that hit an enemy forks; one in the ground, a tree or a friend does nothing more.
                        if (!struck || !owner || owner != Player.m_localPlayer || struck == owner || struck.IsPlayer() || !BaseAI.IsEnemy(owner, struck))
                        {
                            return;
                        }
                        tag.Exploded = true;
                        Fork(owner, tag, struck);
                    });
                }
            }
        }

        // ------------------------------------------------------------ Wind Lance: through every enemy

        /// <summary>
        ///     True when the lance met an enemy and flies on (the game's own hit is skipped): the enemy takes the lance's
        ///     hit once, as the game's arrow hit would give it. False leaves the hit to the game.
        /// </summary>
        private static bool PassThrough(Projectile lance, ShotTag tag, Collider collider, Vector3 hitPoint)
        {
            var owner = lance.m_owner;
            var hitObject = Projectile.FindHitObject(collider);
            var target = hitObject ? hitObject.GetComponent<Character>() : null;
            if (!target || !owner || owner != Player.m_localPlayer || target == owner || target.IsPlayer() || !BaseAI.IsEnemy(owner, target))
            {
                return false;
            }
            if (target.IsDead() || !tag.Pierced.Add(target))
            {
                return true;
            }
            var hit = new HitData();
            hit.m_hitCollider = collider;
            hit.m_damage = lance.m_damage;
            hit.m_pushForce = lance.m_attackForce;
            hit.m_backstabBonus = lance.m_backstabBonus;
            hit.m_point = hitPoint;
            hit.m_dir = lance.m_vel.sqrMagnitude > 0.01f ? lance.m_vel.normalized : lance.transform.forward;
            hit.m_statusEffectHash = lance.m_statusEffectHash;
            hit.m_skillLevel = owner.GetSkillLevel(lance.m_skill);
            hit.m_dodgeable = lance.m_dodgeable;
            hit.m_blockable = lance.m_blockable;
            hit.m_ranged = true;
            hit.m_skill = lance.m_skill;
            hit.m_hitType = HitData.HitType.PlayerHit;
            hit.SetAttacker(owner);
            target.Damage(hit);
            lance.m_hitEffects.Create(hitPoint, Quaternion.identity);
            // The bow skill rises once per lance, as for one arrow.
            if (!tag.RaisedSkill && owner is Player player)
            {
                tag.RaisedSkill = true;
                player.RaiseSkill(lance.m_skill, lance.m_raiseSkillAmount);
            }
            return true;
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

        // ------------------------------------------------------------ Forked Lightning

        private static readonly List<Character> s_forkCandidates = new List<Character>();
        private static readonly List<float> s_forkDistances = new List<float>();

        /// <summary>
        ///     The struck enemy takes one fork; forks go to the nearest other enemies within reach of its body, one each
        ///     (user 2026-11-16). Each fork rolls its own lightning damage.
        /// </summary>
        private static void Fork(Character owner, ShotTag tag, Character struck)
        {
            var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
            if (v == null)
            {
                return;
            }
            int forks = Mathf.RoundToInt(v.Get(tag.SkillId, "forks", tag.Level));
            float reach = v.Get(tag.SkillId, "reach_m", tag.Level);
            Vector3 from = struck.GetCenterPoint();
            // Body to body: the gap between the struck enemy's collider and the other's.
            s_forkCandidates.Clear();
            s_forkDistances.Clear();
            foreach (var c in Character.GetAllCharacters())
            {
                if (!c || c == struck || c == owner || c.IsPlayer() || c.IsDead() || !BaseAI.IsEnemy(owner, c))
                {
                    continue;
                }
                Vector3 onOther = c.m_collider ? c.m_collider.ClosestPoint(from) : c.transform.position;
                Vector3 onStruck = struck.m_collider ? struck.m_collider.ClosestPoint(onOther) : from;
                s_forkCandidates.Add(c);
                s_forkDistances.Add(Vector3.Distance(onOther, onStruck));
            }
            if (!struck.IsDead())
            {
                ForkHit(owner, tag, v, struck, from, from);
            }
            else
            {
                LightningEffect(from);
            }
            foreach (int i in ActiveSkillRules.ForkTargets(s_forkDistances, reach, forks))
            {
                ForkHit(owner, tag, v, s_forkCandidates[i], from, s_forkCandidates[i].GetCenterPoint());
            }
        }

        private static void ForkHit(Character owner, ShotTag tag, SkillValueTable v, Character target, Vector3 from, Vector3 to)
        {
            var hit = new HitData();
            hit.m_damage.m_lightning = Roll(v, tag.SkillId, "lightning_damage", tag.Level, tag.Scale);
            hit.m_point = to;
            hit.m_dir = (to - from).sqrMagnitude > 0.01f ? (to - from).normalized : Vector3.up;
            hit.m_ranged = true;
            hit.m_skill = Skills.SkillType.Bows;
            hit.m_hitType = HitData.HitType.PlayerHit;
            hit.SetAttacker(owner);
            target.Damage(hit);
            LightningEffect(to);
            ForkLine.Draw(from, to);
        }

        /// <summary>A burst of lightning at a point: the Dundr staff's impact, else a flash of our own.</summary>
        private static void LightningEffect(Vector3 point)
        {
            if (!ImpactEffect("StaffLightning", null, point))
            {
                ForkLine.Flash(point);
            }
        }

        /// <summary>
        ///     A fork of lightning drawn from the struck enemy to another, for a moment (on the shooter's game only; the
        ///     burst at each enemy is the game's own effect and shows for everyone).
        /// </summary>
        private sealed class ForkLine : MonoBehaviour
        {
            private const float Seconds = 0.25f;
            private static Material s_material;
            private static bool s_noMaterial;
            private float m_until;

            public static void Draw(Vector3 from, Vector3 to)
            {
                if ((to - from).sqrMagnitude < 0.04f)
                {
                    return;
                }
                var material = Material();
                if (!material)
                {
                    return;
                }
                var go = new GameObject("Orlog_Fork");
                var line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = material;
                line.useWorldSpace = true;
                line.startWidth = 0.12f;
                line.endWidth = 0.04f;
                line.startColor = Color.white;
                line.endColor = LightningColour;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                // A jagged path: the straight line with its inner points pushed sideways.
                const int points = 7;
                line.positionCount = points;
                float jag = Mathf.Min(0.35f, Vector3.Distance(from, to) * 0.12f);
                for (int i = 0; i < points; i++)
                {
                    Vector3 p = Vector3.Lerp(from, to, i / (points - 1f));
                    if (i > 0 && i < points - 1)
                    {
                        p += Random.insideUnitSphere * jag;
                    }
                    line.SetPosition(i, p);
                }
                var light = go.AddComponent<Light>();
                go.transform.position = Vector3.Lerp(from, to, 0.5f);
                light.type = LightType.Point;
                light.color = LightningColour;
                light.range = 6f;
                light.intensity = 4f;
                go.AddComponent<ForkLine>().m_until = Time.time + Seconds;
            }

            /// <summary>A flash of light alone, where the game's lightning effect isn't found.</summary>
            public static void Flash(Vector3 point)
            {
                var go = new GameObject("Orlog_ForkFlash");
                go.transform.position = point;
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = LightningColour;
                light.range = 6f;
                light.intensity = 4f;
                go.AddComponent<ForkLine>().m_until = Time.time + Seconds;
            }

            /// <summary>A plain unlit material from a shader every Unity build has; without one the fork has no line.</summary>
            private static Material Material()
            {
                if (s_material || s_noMaterial)
                {
                    return s_material;
                }
                foreach (string name in new[] { "Sprites/Default", "UI/Default", "Legacy Shaders/Particles/Additive" })
                {
                    var shader = Shader.Find(name);
                    if (shader)
                    {
                        s_material = new Material(shader);
                        return s_material;
                    }
                }
                s_noMaterial = true;
                Jotunn.Logger.LogInfo("Orlog: no plain shader found for Forked Lightning's lines; the forks show only their bursts.");
                return null;
            }

            private void Update()
            {
                if (Time.time > m_until)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        ///     The impact effect of a vanilla projectile, played where an arrow explodes: the staff's own (ice shards,
        ///     lightning), else the elemental arrow's. False when neither has one.
        /// </summary>
        private static bool ImpactEffect(string item, string fallback, Vector3 point)
        {
            foreach (string name in new[] { item, fallback })
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                var prefab = ObjectDB.instance.GetItemPrefab(name);
                var projectile = prefab ? prefab.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_attack.m_attackProjectile : null;
                var effects = projectile ? projectile.GetComponent<Projectile>()?.m_hitEffects : null;
                if (effects != null && effects.HasEffects())
                {
                    effects.Create(point, Quaternion.identity);
                    return true;
                }
            }
            return false;
        }
    }
}
