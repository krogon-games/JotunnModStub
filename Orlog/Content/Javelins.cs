using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The ghost javelin (roadmap C6, #41; Krogon 2026-11-13 and 2026-11-28, <c>docs/handoffs/claude-optimistic-ramanujan-acdyf8.md</c>):
    ///     Gungnir's Throw, Lightning Bolt and Plague Javelin fire from the skill button as the spear's own throw (the
    ///     game's secondary attack, <see cref="SkillButton" />), but the projectile is a gold ghost copy of the spear and the
    ///     spear stays in hand.
    ///     <list type="bullet">
    ///         <item>
    ///             For every spear whose throw has a projectile, three copies of that projectile are registered (ghost, bolt,
    ///             plague): they glow gold and see-through (<see cref="GhostSpearLook" />), never turn back into the spear,
    ///             stick where they land and fade over a second (decision 8), and the harpoon's rope is gone (decision 4).
    ///             They are networked prefabs, so every player sees the ghost.
    ///         </item>
    ///         <item>The attack the skill button starts is changed (it is the game's own copy for this throw only): the ghost
    ///         projectile, no spear used up, and for Lightning Bolt twice the speed (decision 11).</item>
    ///         <item>Damage: Gungnir's Throw is the throw's own (2026-11-13); Lightning Bolt 75% → 200% of it, all lightning
    ///         (decision 10); Plague Javelin 100% of it, then a spreading poison cloud (decisions 13-14, <see cref="GroundArea" />).</item>
    ///         <item>Stamina and the Spears skill: the game's throw's own (decisions 7 and 9); eitr: the skill's (SkillButton).</item>
    ///     </list>
    ///     Like the bow skills, the hit and the cloud are handled on the thrower's game, which runs the projectile.
    /// </summary>
    internal static class Javelins
    {
        /// <summary>The ghost projectile kinds; the index into each spear's copies.</summary>
        internal enum Variant
        {
            Ghost = 0,
            Bolt = 1,
            Plague = 2
        }

        private static readonly string[] Prefixes = { "Orlog_GhostSpear_", "Orlog_BoltSpear_", "Orlog_PlagueSpear_" };
        /// <summary>The ghost copies of each spear projectile, by the game projectile's name.</summary>
        private static readonly Dictionary<string, GameObject[]> Ghosts = new Dictionary<string, GameObject[]>();
        private static readonly HashSet<string> s_loggedMissing = new HashSet<string>();

        public static Variant VariantOf(string skillId)
        {
            return skillId == ActiveSkillRules.LightningBoltId ? Variant.Bolt : skillId == ActiveSkillRules.PlagueJavelinId ? Variant.Plague : Variant.Ghost;
        }

        // ------------------------------------------------------------ the ghost projectiles

        /// <summary>Registers the ghost copies of every spear's throw projectile (once the game's items exist).</summary>
        public static void Build()
        {
            if (!ObjectDB.instance)
            {
                return;
            }
            int spears = 0;
            foreach (var item in ObjectDB.instance.m_items)
            {
                var drop = item ? item.GetComponent<ItemDrop>() : null;
                var shared = drop ? drop.m_itemData?.m_shared : null;
                var source = shared != null && shared.m_skillType == Skills.SkillType.Spears ? shared.m_secondaryAttack?.m_attackProjectile : null;
                if (!source || Ghosts.ContainsKey(source.name) || !source.GetComponent<Projectile>())
                {
                    continue;
                }
                var copies = new GameObject[Prefixes.Length];
                for (int i = 0; i < Prefixes.Length; i++)
                {
                    copies[i] = MakeGhost(source, (Variant)i);
                }
                Ghosts[source.name] = copies;
                spears++;
            }
            Jotunn.Logger.LogInfo($"Orlog: made the ghost javelins for {spears} spear throw{(spears == 1 ? "" : "s")}.");
        }

        private static GameObject MakeGhost(GameObject source, Variant variant)
        {
            var prefab = PrefabManager.Instance.CreateClonedPrefab(Prefixes[(int)variant] + source.name, source);
            var projectile = prefab.GetComponent<Projectile>();
            // Never the spear again (no pick-up), nothing else spawned where it lands.
            projectile.m_respawnItemOnHit = false;
            projectile.m_spawnItem = null;
            projectile.m_spawnOnHit = null;
            projectile.m_randomSpawnOnHit?.Clear();
            // Sticks where it lands, in the ground or the enemy, then fades (decision 8).
            projectile.m_stayAfterHitStatic = true;
            projectile.m_stayAfterHitDynamic = true;
            projectile.m_stayTTL = ActiveSkillRules.GhostFadeSeconds;
            // The harpoon's rope back to the thrower (decision 4: no pull).
            foreach (var rope in prefab.GetComponentsInChildren<LineConnect>(true))
            {
                Object.DestroyImmediate(rope);
            }
            if (variant == Variant.Bolt)
            {
                // Bursts with the game's lightning on impact (decision 12): the lightning staff's hit, as Forked Lightning's.
                AddHitEffects(projectile, "StaffLightning");
            }
            prefab.AddComponent<GhostSpearLook>().Kind = (int)variant;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
            if (ZNetScene.instance)
            {
                PrefabManager.Instance.RegisterToZNetScene(prefab);
            }
            return prefab;
        }

        private static void AddHitEffects(Projectile projectile, string item)
        {
            var prefab = ObjectDB.instance.GetItemPrefab(item);
            var other = prefab ? prefab.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_attack.m_attackProjectile : null;
            var effects = other ? other.GetComponent<Projectile>()?.m_hitEffects : null;
            if (effects == null || !effects.HasEffects())
            {
                Jotunn.Logger.LogInfo($"Orlog: no {item} hit effect found for Lightning Bolt; it bursts with the spear's own.");
                return;
            }
            var list = new List<EffectList.EffectData>(projectile.m_hitEffects?.m_effectPrefabs ?? new EffectList.EffectData[0]);
            list.AddRange(effects.m_effectPrefabs);
            projectile.m_hitEffects = new EffectList { m_effectPrefabs = list.ToArray() };
        }

        /// <summary>The ghost copy of a spear's throw projectile for a skill, or null if there is none.</summary>
        private static GameObject GhostFor(GameObject projectile, Variant variant)
        {
            if (!projectile)
            {
                return null;
            }
            if (Ghosts.TryGetValue(projectile.name, out var copies) && copies[(int)variant])
            {
                return copies[(int)variant];
            }
            if (s_loggedMissing.Add(projectile.name))
            {
                Jotunn.Logger.LogWarning($"Orlog: no ghost javelin for {projectile.name}; this throw flies as the game's own projectile (no spear is dropped).");
            }
            return null;
        }

        // ------------------------------------------------------------ the throw

        /// <summary>
        ///     A skill throw has started (<see cref="SkillButton" />): the attack is the game's own copy of the spear's
        ///     throw for this throw only. It becomes the ghost: its projectile, no spear used up, Lightning Bolt faster.
        /// </summary>
        public static void Prepare(Attack attack, string skillId, int level)
        {
            if (attack == null)
            {
                return;
            }
            // First, so the spear can never leave the hand, whatever happens below.
            attack.m_consumeItem = false;
            var variant = VariantOf(skillId);
            var ghost = GhostFor(attack.m_attackProjectile, variant);
            if (ghost)
            {
                attack.m_attackProjectile = ghost;
            }
            if (variant == Variant.Bolt)
            {
                var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
                float speed = (v?.Get(skillId, "speed_pct", level) ?? 100f) / 100f;
                if (speed > 0f)
                {
                    attack.m_projectileVel *= speed;
                    attack.m_projectileVelMin *= speed;
                }
            }
        }

        /// <summary>The ghost projectile's damage for its skill, after the game set it up from the throw.</summary>
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
        private static class GhostDamage
        {
            private static void Postfix(Projectile __instance, Character owner, HitData hitData)
            {
                var tag = __instance ? __instance.GetComponent<BowSkills.ShotTag>() : null;
                bool ghost = __instance && __instance.GetComponent<GhostSpearLook>();
                if (!ghost && (tag == null || !ActiveSkillRules.Throws(tag.SkillId)))
                {
                    return;
                }
                // Whatever else happens, a skill throw never drops a spear.
                __instance.m_spawnItem = null;
                __instance.m_respawnItemOnHit = false;
                if (tag == null || owner != Player.m_localPlayer)
                {
                    return;
                }
                Safe.Run("set a ghost javelin's damage", () => Apply(__instance, tag, hitData));
            }

            private static void Apply(Projectile projectile, BowSkills.ShotTag tag, HitData hitData)
            {
                // The Abyssal Harpoon's pull stays on the game's own throw (decision 4).
                if (projectile.m_statusEffectHash != 0)
                {
                    var effect = ObjectDB.instance ? ObjectDB.instance.GetStatusEffect(projectile.m_statusEffectHash) : null;
                    if (effect && effect.name.IndexOf("Harpoon", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        projectile.m_statusEffectHash = 0;
                        projectile.m_statusEffect = "";
                    }
                }
                var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
                if (v == null)
                {
                    return;
                }
                var damage = projectile.m_damage;
                switch (tag.SkillId)
                {
                    case ActiveSkillRules.LightningBoltId:
                    {
                        // All of it lightning: the share of the throw's damage (decision 10); tool damage kept as it was.
                        float pct = v.Get(tag.SkillId, "weapon_damage_pct", tag.Level);
                        var lightning = new HitData.DamageTypes
                        {
                            m_lightning = ActiveSkillRules.ShareOfShot(tag.ShotDamage, pct),
                            m_chop = damage.m_chop,
                            m_pickaxe = damage.m_pickaxe
                        };
                        Set(projectile, hitData, lightning);
                        Jotunn.Logger.LogInfo($"Orlog: Lightning Bolt thrown (rank {tag.Level}): {pct:0}% of the throw's damage {tag.ShotDamage:0.#} = {lightning.m_lightning:0.#} lightning.");
                        break;
                    }
                    case ActiveSkillRules.PlagueJavelinId:
                    {
                        float pct = v.Get(tag.SkillId, "weapon_damage_pct", tag.Level);
                        damage.Modify(pct / 100f);
                        Set(projectile, hitData, damage);
                        Jotunn.Logger.LogInfo($"Orlog: Plague Javelin thrown (rank {tag.Level}): the throw's damage {tag.ShotDamage:0.#}, {pct:0}% on the hit.");
                        break;
                    }
                    default:
                        Jotunn.Logger.LogInfo($"Orlog: Gungnir's Throw: the throw's own damage {tag.ShotDamage:0.#}.");
                        break;
                }
            }

            private static void Set(Projectile projectile, HitData hitData, HitData.DamageTypes damage)
            {
                projectile.m_damage = damage;
                if (hitData != null)
                {
                    hitData.m_damage = damage;
                }
            }
        }

        /// <summary>Plague Javelin's cloud where the ghost lands (decisions 13-14), on the thrower's game.</summary>
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static class GhostImpact
        {
            private static void Postfix(Projectile __instance, Vector3 hitPoint)
            {
                var tag = __instance ? __instance.GetComponent<BowSkills.ShotTag>() : null;
                if (tag == null || tag.Exploded || tag.SkillId != ActiveSkillRules.PlagueJavelinId)
                {
                    return;
                }
                tag.Exploded = true;
                Safe.Run("start Plague Javelin's cloud", () => StartCloud(__instance.m_owner, tag, hitPoint));
            }

            private static void StartCloud(Character owner, BowSkills.ShotTag tag, Vector3 point)
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
                string id = tag.SkillId;
                float pct = v.Get(id, "cloud_damage_pct", tag.Level);
                GroundArea.Begin(owner, "Plague Javelin", tag.Level, point, v.Get(id, "radius_m", tag.Level), v.Get(id, "poison_duration_s", tag.Level),
                    ActiveSkillRules.ShareOfShot(tag.ShotDamage, pct, SkillBar.DamageMultiplier()), true, false, tag.ShotDamage, pct,
                    true, Skills.SkillType.Spears);
            }
        }
    }
}
