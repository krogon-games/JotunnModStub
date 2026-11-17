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
    ///     <c>Orlog/Data/amazon_skill_values.csv</c>. Skill damage scales off the weapon (Krogon, 2026-11-16, #72): a
    ///     blast or a fork deals a share of the shot's own damage (<see cref="ShotTag.ShotDamage" />), never fixed points.
    ///     <list type="bullet">
    ///         <item>Multishot: several arrows in an even fan for one arrow, each at 75% damage.</item>
    ///         <item>Freezing Arrow: on impact, frost (50% → 150% of the shot's damage) and a freeze (<see cref="FreezeName" />) on every enemy within the radius; bosses are only slowed by the frost.</item>
    ///         <item>Forked Lightning (user 2026-11-16): the arrow hits as usual; the struck enemy takes one fork of lightning and
    ///         forks go to the nearest other enemies within reach of its body (Charged Strike's fork counts and eitr; each fork
    ///         25% → 100% of the shot's damage as lightning).</item>
    ///         <item>Wind Lance (user 2026-11-16): a charged shot (<see cref="SkillButton" />) flown by <see cref="WindLance" />.</item>
    ///         <item>Strafe: remade after Diablo 3's Rapid Fire (Krogon, 2026-11-17): hold to keep firing after one full draw
    ///         (<see cref="Strafe" />); each arrow of the stream is cut to its share here. The volley of 2026-11-16 is removed.</item>
    ///         <item>Immolation Arrow (Krogon, 2026-11-16): a fire blast on impact like Freezing Arrow's, then the ground there
    ///         burns and hurts enemies standing in it every second (<see cref="GroundArea" />).</item>
    ///         <item>Venom Strike (Krogon, 2026-11-16): the arrow hits as usual and a poison cloud hangs where it landed,
    ///         poisoning everything inside every second (<see cref="GroundArea" />).</item>
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

        /// <summary>On a skill arrow: which skill shot it, at what level, and the shot's own damage.</summary>
        internal sealed class ShotTag : MonoBehaviour
        {
            public string SkillId;
            public int Level;
            /// <summary>
            ///     The shot's own damage as the game worked it out when the arrow was made: bow + arrow, every damage
            ///     type added up (tool damage left out), after the draw, the bow skill, Dexterity and status effects.
            ///     The skills' shares are taken from this.
            /// </summary>
            public float ShotDamage;
            public bool Exploded;
        }

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
                s_firing = SkillButton.For(__instance) ?? Strafe.ShotFor(__instance);
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
                tag.ShotDamage = TotalDamage(hit.m_damage);
                switch (id)
                {
                    case "MultipleShot":
                        hit.m_damage.Modify(v.Get(id, "weapon_damage_pct", level) / 100f);
                        velocity = Quaternion.AngleAxis(ActiveSkillRules.FanAngle(s_fanIndex++, s_fanCount), Vector3.up) * velocity;
                        break;
                    case "WindLance":
                        WindLance.Launch(projectile, level, v, ref velocity, hit);
                        break;
                    case "Strafe":
                        // Each arrow of the stream: its share of the shot (25% -> 50%, Krogon 2026-11-17). Its push and the
                        // Bows skill it trains shrink with it (Claude's choice), so 6 arrows a second don't push six times
                        // as hard or train the skill six times as fast as single shots.
                        float share = v.Get(id, "weapon_damage_pct", level) / 100f;
                        hit.m_damage.Modify(share);
                        hit.m_pushForce *= share;
                        hit.m_skillRaiseAmount *= share;
                        break;
                }
            }
        }

        /// <summary>A hit's damage with every combat type added up (chop and pickaxe damage left out).</summary>
        private static float TotalDamage(HitData.DamageTypes d)
        {
            return d.m_damage + d.m_blunt + d.m_slash + d.m_pierce + d.m_fire + d.m_frost + d.m_lightning + d.m_poison + d.m_spirit;
        }

        /// <summary>The damage a skill arrow adds: the table's share (<paramref name="value" />, in %) of the shot's own damage.</summary>
        private static float Share(SkillValueTable v, ShotTag tag, string value)
        {
            return ActiveSkillRules.ShareOfShot(tag.ShotDamage, v.Get(tag.SkillId, value, tag.Level), SkillBar.DamageMultiplier());
        }

        // ------------------------------------------------------------ impacts

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class Impact
        {
            private static void Postfix(Projectile __instance, Collider collider, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<ShotTag>() : null;
                if (tag == null || tag.Exploded)
                {
                    return;
                }
                if (tag.SkillId == "VenomStrike")
                {
                    tag.Exploded = true;
                    Safe.Run("start Venom Strike's cloud", () => StartCloud(__instance.m_owner, tag, hitPoint));
                }
                else if (IsBlast(tag))
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

        private static bool IsBlast(ShotTag tag)
        {
            return tag.SkillId == "FreezingArrow" || tag.SkillId == "ImmolationArrow";
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
        internal static void CharactersInBlast(Vector3 point, float radius, Character struck, List<Character> into)
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
            bool fire = tag.SkillId == "ImmolationArrow";
            float radius = fire
                ? v.Get(tag.SkillId, "radius_m", tag.Level)
                : v.Get(tag.SkillId, "radius_yards", tag.Level) * ActiveSkillRules.MetresPerYard;
            float freeze = fire ? 0f : v.Get(tag.SkillId, "freeze_s", tag.Level);
            float frost = Share(v, tag, "blast_damage_pct");
            if (fire)
            {
                ImpactEffect("StaffFireball", "ArrowFire", point);
            }
            else
            {
                ImpactEffect("StaffIceShards", "ArrowFrost", point);
            }
            CharactersInBlast(point, radius, struck, s_inRange);
            int caught = 0;
            foreach (var target in s_inRange)
            {
                if (!target || target == owner || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(owner, target))
                {
                    continue;
                }
                var hit = new HitData();
                if (fire)
                {
                    hit.m_damage.m_fire = frost;
                }
                else
                {
                    hit.m_damage.m_frost = frost;
                }
                caught++;
                if (!fire && !target.IsBoss())
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
            // For the play-test (2026-11-16, #72): the numbers behind a blast.
            Jotunn.Logger.LogInfo($"Orlog: {(fire ? "Immolation Arrow" : "Freezing Arrow")} (level {tag.Level}): the shot's damage {tag.ShotDamage:0.#}, blast " +
                                  $"{v.Get(tag.SkillId, "blast_damage_pct", tag.Level):0.#}% = {frost:0.#} {(fire ? "fire" : "frost")} on {caught} enemies within {radius:0.#} m.");
            if (fire)
            {
                // Then the ground there burns (Krogon, 2026-11-16): the first hit a second after the impact.
                GroundArea.Begin(owner, "Immolation Arrow", tag.Level, GroundBelow(point), radius, v.Get(tag.SkillId, "burn_duration_s", tag.Level),
                    Share(v, tag, "burn_damage_pct"), false, false, tag.ShotDamage, v.Get(tag.SkillId, "burn_damage_pct", tag.Level));
            }
        }

        /// <summary>
        ///     The ground (or floor, rock, building) straight under a hit point: an arrow that struck a troll's chest or a
        ///     flying creature leaves its burning ground below, not in the air. Creatures' own bodies are looked through;
        ///     with nothing solid found under it, the point itself.
        /// </summary>
        private static Vector3 GroundBelow(Vector3 point)
        {
            // Start a little above, so a hit that is already on the ground finds that ground.
            const float lift = 0.5f;
            const float reach = 100f;
            Vector3 found = point;
            float nearest = float.MaxValue;
            foreach (var below in Physics.RaycastAll(point + Vector3.up * lift, Vector3.down, reach, Projectile.s_rayMaskSolids))
            {
                if (!below.collider || below.collider.GetComponentInParent<Character>() || below.distance >= nearest)
                {
                    continue;
                }
                nearest = below.distance;
                found = below.point;
            }
            return found;
        }

        /// <summary>Venom Strike: a poison cloud where the arrow landed, first hit at once (Krogon, 2026-11-16).</summary>
        private static void StartCloud(Character owner, ShotTag tag, Vector3 point)
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
            GroundArea.Begin(owner, "Venom Strike", tag.Level, point, v.Get(tag.SkillId, "radius_m", tag.Level),
                v.Get(tag.SkillId, "cloud_duration_s", tag.Level), Share(v, tag, "cloud_damage_pct"), true, true,
                tag.ShotDamage, v.Get(tag.SkillId, "cloud_damage_pct", tag.Level));
        }

        // ------------------------------------------------------------ Forked Lightning

        private static readonly List<Character> s_forkCandidates = new List<Character>();
        private static readonly List<float> s_forkDistances = new List<float>();

        /// <summary>
        ///     The struck enemy takes one fork; forks go to the nearest other enemies within reach of its body, one each
        ///     (user 2026-11-16). Each fork deals the same share of the shot's damage as lightning (#72).
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
            float lightning = Share(v, tag, "fork_damage_pct");
            int struckCount = 0;
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
                ForkHit(owner, lightning, struck, from, from);
                struckCount++;
            }
            else
            {
                LightningEffect(from);
            }
            foreach (int i in ActiveSkillRules.ForkTargets(s_forkDistances, reach, forks))
            {
                ForkHit(owner, lightning, s_forkCandidates[i], from, s_forkCandidates[i].GetCenterPoint());
                struckCount++;
            }
            // For the play-test (2026-11-16, #72): the numbers behind a shot's forks.
            Jotunn.Logger.LogInfo($"Orlog: Forked Lightning (level {tag.Level}): the shot's damage {tag.ShotDamage:0.#}, each fork " +
                                  $"{v.Get(tag.SkillId, "fork_damage_pct", tag.Level):0.#}% = {lightning:0.#} lightning, {struckCount} enemies struck " +
                                  $"(up to {forks} others within {reach:0.#} m).");
        }

        private static void ForkHit(Character owner, float lightning, Character target, Vector3 from, Vector3 to)
        {
            var hit = new HitData();
            hit.m_damage.m_lightning = lightning;
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
        internal sealed class ForkLine : MonoBehaviour
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
            internal static Material Material()
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
        internal static bool ImpactEffect(string item, string fallback, Vector3 point)
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
