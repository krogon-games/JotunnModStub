using System;
using System.Collections.Generic;
using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     Wind Lance (Krogon, 2026-11-16; <c>docs/class-design.md</c> §5): a charged bow shot (<see cref="SkillButton" />)
    ///     that leaves as a gust of wind. Numbers per skill level from <c>Orlog/Data/amazon_skill_values.csv</c>.
    ///     <para>
    ///         The game's arrow can't do what the lance does, so the lance flies itself (<see cref="LanceFlight" />): the
    ///         shot's own arrow has its <see cref="Projectile" /> switched off (its flight, fall and hit test stop) and is
    ///         moved from here, on the shooter's game, which runs its projectiles:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>It flies straight at a multiple of the arrow's speed, without falling.</item>
    ///         <item>The ground doesn't stop it: where it would dig in it levels out and runs just above the ground or
    ///         water, following slopes a player can stand on (<see cref="ActiveSkillRules.LanceGroundStep" />). A steeper
    ///         dive or hillside, a tree, a rock or a building stops it, through the game's own arrow hit.</item>
    ///         <item>It is a gust, not a line: every enemy within its half width of the path is hit once, through the
    ///         game's own arrow hit (<c>Projectile.OnHit</c> with the game's "only stop on terrain" switch, so Corruption's
    ///         and our own hit patches see an ordinary bow hit), then staggered (not bosses).</item>
    ///         <item>It is gone after its range.</item>
    ///     </list>
    ///     The first build (the same day) left the flight to the game and only skipped its hit on enemies: aimed down at
    ///     a small creature, the straight, arrow-thin line struck the ground a step behind it, so it never reached a
    ///     second enemy (Krogon's play-test).
    /// </summary>
    internal static class WindLance
    {
        /// <summary>A lance that never ends (it shouldn't happen) is removed after this long.</summary>
        private const float MaxSeconds = 3f;
        /// <summary>Water is looked for this far below the gust's lower edge.</summary>
        private const float WaterProbe = 0.5f;

        private static readonly List<RaycastHit> s_hits = new List<RaycastHit>();

        // ------------------------------------------------------------ small plants

        private static HashSet<string> s_smallPlants;

        /// <summary>
        ///     The prefabs of the world's small plants, which a lance passes through without harming them (Krogon,
        ///     2026-11-16; a bush had stopped one in his play-test): of the vegetation the game grows in the world
        ///     (<c>ZoneSystem.m_vegetation</c>), the breakable things the game doesn't class as trees and that drop only
        ///     what trees drop (<see cref="ActiveSkillRules.IsSmallPlant" />). Worked out once per world, from the
        ///     game's data, and listed in the log.
        /// </summary>
        private static HashSet<string> SmallPlants()
        {
            if (s_smallPlants != null)
            {
                return s_smallPlants;
            }
            var plants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!ZNetScene.instance || !ZoneSystem.instance)
            {
                return plants;
            }
            s_smallPlants = plants;
            Safe.Run("list the small plants a Wind Lance passes through", () =>
            {
                var treeDrops = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var prefab in ZNetScene.instance.m_prefabs)
                {
                    if (!prefab)
                    {
                        continue;
                    }
                    var tree = prefab.GetComponent<TreeBase>();
                    var log = prefab.GetComponent<TreeLog>();
                    AddDrops(tree ? tree.m_dropWhenDestroyed : null, treeDrops);
                    AddDrops(log ? log.m_dropWhenDestroyed : null, treeDrops);
                }
                var drops = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var vegetation in ZoneSystem.instance.m_vegetation)
                {
                    var prefab = vegetation != null ? vegetation.m_prefab : null;
                    var destructible = prefab ? prefab.GetComponent<Destructible>() : null;
                    if (!destructible)
                    {
                        continue;
                    }
                    drops.Clear();
                    var dropper = prefab.GetComponent<DropOnDestroyed>();
                    AddDrops(dropper ? dropper.m_dropWhenDestroyed : null, drops);
                    if (ActiveSkillRules.IsSmallPlant(destructible.m_destructibleType == DestructibleType.Tree, drops, treeDrops))
                    {
                        plants.Add(prefab.name);
                    }
                }
                var names = new List<string>(plants);
                names.Sort(StringComparer.OrdinalIgnoreCase);
                Jotunn.Logger.LogInfo($"Orlog: Wind Lance passes through {names.Count} kinds of small plants (and anything that can be picked or is still growing): {string.Join(", ", names)}.");
            });
            return plants;
        }

        private static void AddDrops(DropTable table, HashSet<string> into)
        {
            if (table == null || table.m_drops == null)
            {
                return;
            }
            foreach (var drop in table.m_drops)
            {
                if (drop.m_item)
                {
                    into.Add(drop.m_item.name);
                }
            }
        }

        /// <summary>A bush or other small plant: one of <see cref="SmallPlants" />, something that can be picked, or a sapling or crop still growing.</summary>
        private static bool IsSmallPlant(Collider collider)
        {
            var hitObject = Projectile.FindHitObject(collider);
            if (!hitObject)
            {
                return false;
            }
            return hitObject.GetComponent<Pickable>() || hitObject.GetComponent<Plant>() || SmallPlants().Contains(Utils.GetPrefabName(hitObject));
        }

        // ------------------------------------------------------------ the launch

        /// <summary>
        ///     Turns the arrow a charged shot is firing into a lance (from <c>Projectile.Setup</c>, before the game reads
        ///     the hit and the velocity).
        /// </summary>
        public static void Launch(Projectile projectile, int level, SkillValueTable v, ref Vector3 velocity, HitData hit)
        {
            const string id = "WindLance";
            float damage = v.Get(id, "weapon_damage_pct", level) / 100f;
            hit.m_damage.Modify(damage);
            hit.m_pushForce = v.Get(id, "knockback", level);
            velocity *= v.Get(id, "speed_pct", level) / 100f;
            projectile.m_gravity = 0f;
            // The lance flies itself (LanceFlight): the game's own flight and hit test are switched off for this arrow.
            projectile.enabled = false;
            var flight = projectile.gameObject.AddComponent<LanceFlight>();
            flight.Lance = projectile;
            flight.Range = v.Get(id, "range_m", level);
            flight.HalfWidth = v.Get(id, "width_m", level);
            flight.Born = Time.time;

            Vector3 direction = velocity.sqrMagnitude > 0.01f ? velocity.normalized : projectile.transform.forward;
            WindLanceLook.Dress(projectile);
            WindLanceLook.Burst(projectile.transform.position, direction);
            // The wind's sound: the game's dodge whoosh, at the shooter.
            var player = Player.m_localPlayer;
            if (player)
            {
                player.m_dodgeEffects.Create(player.transform.position, Quaternion.identity);
            }
            Jotunn.Logger.LogInfo($"Orlog: Wind Lance fired (rank {level}: {damage * 100f:0}% damage, push {hit.m_pushForce:0}, {velocity.magnitude:0} m/s, " +
                                  $"{flight.Range:0} m, {flight.HalfWidth * 2f:0.##} m wide).");
        }

        // ------------------------------------------------------------ the charge

        /// <summary>
        ///     The charged shot is ready: wind swirls at the bow hand for as long as this is called every frame, and on the
        ///     first call the game's perfect-block ring sounds.
        /// </summary>
        public static void ReadyCue(Player player, bool first)
        {
            if (!player)
            {
                return;
            }
            WindLanceLook.ChargeSwirl(player);
            if (first)
            {
                Safe.Run("sound the charge cue", () =>
                {
                    Transform hand = player.m_visEquipment ? player.m_visEquipment.m_leftHand : null;
                    player.m_perfectBlockEffect.Create(hand ? hand.position : player.GetCenterPoint(), Quaternion.identity);
                });
            }
        }

        /// <summary>The colour the draw circle turns while the charge builds, and when it is ready.</summary>
        private static readonly Color ChargingColour = new Color(0.55f, 0.9f, 1f);
        private static readonly Color ReadyColour = new Color(0.85f, 1f, 1f);

        /// <summary>
        ///     The charge on the bow's own draw circle (Krogon, 2026-11-16): past full draw the yellow circle keeps
        ///     narrowing and turns from yellow to pale blue; when the shot is ready it is bright and pulses. A crossbow
        ///     has no draw circle of its own, so it is shown for the whole hold.
        /// </summary>
        [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair))]
        private static class DrawCircle
        {
            private static void Postfix(Hud __instance, Player player)
            {
                if (!__instance || !player || player != Player.m_localPlayer)
                {
                    return;
                }
                float progress = SkillButton.ChargeProgress;
                bool draws = SkillButton.ChargeDraws;
                // A bow still drawing shows the game's own circle; ours takes over once the hold is counting.
                if (progress < 0f || (draws && progress <= 0f))
                {
                    return;
                }
                Safe.Run("show the charge on the draw circle", () =>
                {
                    var circle = __instance.m_crosshairBow;
                    if (!circle)
                    {
                        return;
                    }
                    float scale = ActiveSkillRules.ChargeCircleScale(progress, draws);
                    Color colour;
                    if (progress >= 1f)
                    {
                        colour = ReadyColour;
                        scale *= 1f + 0.25f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
                    }
                    else
                    {
                        colour = Color.Lerp(Color.yellow, ChargingColour, progress);
                    }
                    circle.gameObject.SetActive(true);
                    circle.transform.localScale = new Vector3(scale, scale, scale);
                    circle.color = colour;
                });
            }
        }

        // ------------------------------------------------------------ the flight

        /// <summary>Flies one lance, on the shooter's game (see <see cref="WindLance" />).</summary>
        internal sealed class LanceFlight : MonoBehaviour
        {
            public Projectile Lance;
            public float Range;
            public float HalfWidth;
            public float Born;

            private readonly HashSet<Character> m_struck = new HashSet<Character>();
            private float m_travelled;
            private bool m_skimming;
            private bool m_first = true;
            private bool m_done;

            private void FixedUpdate()
            {
                if (m_done)
                {
                    return;
                }
                try
                {
                    Step(Time.fixedDeltaTime);
                }
                catch (Exception ex)
                {
                    Safe.Report("fly a Wind Lance", ex);
                    End("an error");
                }
            }

            private void Step(float dt)
            {
                var view = Lance ? Lance.m_nview : null;
                if (!view || !view.IsValid() || !view.IsOwner())
                {
                    return;
                }
                if (Time.time - Born > MaxSeconds)
                {
                    End("its time");
                    return;
                }
                float speed = Lance.m_vel.magnitude;
                if (speed < 0.1f || Range - m_travelled <= 0.001f)
                {
                    End("its range");
                    return;
                }
                Vector3 direction = Lance.m_vel / speed;
                Vector3 position = transform.position;
                float step = Mathf.Min(speed * dt, Range - m_travelled);
                Vector3 next = position + direction * step;

                // The ground: fly free, run along it, or strike it.
                bool groundStops = true;
                Vector3 flat = new Vector3(direction.x, 0f, direction.z);
                float flatLength = flat.magnitude;
                if (Floor(next, out float floor))
                {
                    var ground = ActiveSkillRules.LanceGroundStep(m_skimming, position.y, next.y, floor, flatLength * step,
                        ActiveSkillRules.LanceMaxSlopeDegrees, out float nextY);
                    if (ground == ActiveSkillRules.LanceGround.Skim && flatLength > 0.001f)
                    {
                        // Level from here on: the same speed, along the ground.
                        m_skimming = true;
                        flat /= flatLength;
                        Lance.m_vel = flat * speed;
                        next = position + flat * step;
                        next.y = nextY;
                        groundStops = false;
                    }
                }

                Vector3 travel = next - position;
                float length = travel.magnitude;
                if (length < 0.0001f)
                {
                    End("its range");
                    return;
                }
                travel /= length;
                transform.rotation = Quaternion.LookRotation(travel);

                // What stops it: the first solid thing on its centre line that isn't a creature (and, while it runs along
                // the ground, isn't the ground itself). The first step starts at the shooter, as the game's arrows do, so
                // a wall right in front of the bow still stops it.
                Vector3 from = position;
                if (m_first && Lance.m_haveStartPoint)
                {
                    from = Lance.m_startPoint;
                }
                m_first = false;
                Vector3 line = next - from;
                float lineLength = line.magnitude;
                bool stopped = false;
                RaycastHit stop = default(RaycastHit);
                float sweep = length;
                if (lineLength > 0.0001f)
                {
                    Sorted(Physics.RaycastAll(from, line / lineLength, lineLength, Projectile.s_rayMaskSolids));
                    foreach (var hit in s_hits)
                    {
                        // Creatures never stop it (enemies are struck below, friends are passed), nor do the fallen,
                        // nor bushes and other small plants.
                        if (!hit.collider || CharacterOf(hit.collider) || hit.collider.GetComponentInParent<Ragdoll>() || IsSmallPlant(hit.collider))
                        {
                            continue;
                        }
                        if (!groundStops && hit.collider.GetComponent<Heightmap>())
                        {
                            continue;
                        }
                        stopped = true;
                        stop = hit;
                        sweep = Mathf.Clamp(Vector3.Dot(hit.point - position, travel), 0f, length);
                        break;
                    }
                }

                // The gust: every enemy within its half width of the path, as far as it gets this step.
                var owner = Lance.m_owner;
                Sorted(Physics.SphereCastAll(position, HalfWidth, travel, sweep, Projectile.s_rayMaskSolids));
                for (int i = 0; i < s_hits.Count; i++)
                {
                    var hit = s_hits[i];
                    var target = hit.collider ? CharacterOf(hit.collider) : null;
                    if (!target || !owner || target == owner || target.IsPlayer() || target.IsDead() || !BaseAI.IsEnemy(owner, target) || !m_struck.Add(target))
                    {
                        continue;
                    }
                    // A collider already inside the gust when the step starts comes back with no hit point.
                    Vector3 point = hit.distance <= 0f ? target.GetCenterPoint() : hit.point;
                    Strike(target, hit.collider, point, travel);
                    if (m_done || !Lance)
                    {
                        return;
                    }
                }

                if (stopped)
                {
                    transform.position = position + travel * sweep;
                    // The game's own arrow hit: its damage to a tree, rock or building, and its impact effect.
                    Lance.m_onlyStopOnTerrain = false;
                    string what = stop.collider ? Utils.GetPrefabName(Projectile.FindHitObject(stop.collider)) : "something";
                    Lance.OnHit(stop.collider, stop.point, false, stop.normal);
                    End(what);
                    return;
                }
                transform.position = next;
                m_travelled += step;
                if (Range - m_travelled <= 0.001f)
                {
                    End("its range");
                }
            }

            /// <summary>
            ///     One enemy in the gust: the game's own arrow hit (the lance's damage and push, along its flight), told not
            ///     to stop or stick, then a stagger (not bosses).
            /// </summary>
            private void Strike(Character target, Collider collider, Vector3 point, Vector3 direction)
            {
                Lance.m_onlyStopOnTerrain = true;
                Lance.m_stayAfterHitDynamic = true;
                Lance.m_stayAfterHitStatic = true;
                Lance.m_attachToClosestBone = false;
                Lance.m_attachToRigidBody = false;
                Lance.OnHit(collider, point, false, -direction);
                if (target && !target.IsDead() && !target.IsBoss())
                {
                    target.Stagger(direction);
                }
                WindLanceLook.Puff(point, direction);
            }

            /// <summary>The lowest the gust's centre may fly above a spot: the ground or the water there, plus its half width.</summary>
            private bool Floor(Vector3 at, out float floor)
            {
                floor = 0f;
                bool found = false;
                if (ZoneSystem.instance && ZoneSystem.instance.GetGroundHeight(at, out float ground))
                {
                    floor = ground;
                    found = true;
                }
                float water = Floating.GetLiquidLevel(new Vector3(at.x, at.y - HalfWidth - WaterProbe, at.z));
                if (water > -1000f && (!found || water > floor))
                {
                    floor = water;
                    found = true;
                }
                floor += HalfWidth;
                return found;
            }

            private static Character CharacterOf(Collider collider)
            {
                var hitObject = Projectile.FindHitObject(collider);
                return hitObject ? hitObject.GetComponent<Character>() : null;
            }

            /// <summary>The hits nearest first, in <see cref="s_hits" />.</summary>
            private static void Sorted(RaycastHit[] hits)
            {
                s_hits.Clear();
                s_hits.AddRange(hits);
                s_hits.Sort(ByDistance);
            }

            private static readonly Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

            /// <summary>The lance is over: a last puff of wind, a line for the log, and the arrow is removed (wind leaves none behind).</summary>
            private void End(string at)
            {
                if (m_done)
                {
                    return;
                }
                m_done = true;
                Jotunn.Logger.LogInfo($"Orlog: Wind Lance flew {m_travelled:0.#} m{(m_skimming ? " (along the ground)" : "")}, struck {m_struck.Count} " +
                                      $"enem{(m_struck.Count == 1 ? "y" : "ies")}, ended at {at}.");
                Safe.Run("end a Wind Lance", () =>
                {
                    WindLanceLook.Puff(transform.position, transform.forward);
                    var view = Lance ? Lance.m_nview : null;
                    if (view && view.IsValid() && view.IsOwner() && ZNetScene.instance)
                    {
                        ZNetScene.instance.Destroy(gameObject);
                    }
                    else if (this)
                    {
                        Destroy(gameObject);
                    }
                });
            }
        }
    }
}
