using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Rimeburst (corrupted-only, #97; Krogon's decisions 4-10 of 2026-11-28, branch claude/wizardly-wozniak-4aix76): a
    ///     stamped rip creature bursts into ice when it dies. Its owner (who decides the death) works out the frost (the
    ///     map's share of the creature's own strongest hit, <see cref="RipTierRules.StrongestHit" />) and tells every game
    ///     where; each game draws the 1.5 s warning (a pale blue ring at the 3 m radius and a growing light, the game's frost
    ///     arrow hit at the start) and then the burst (the Staff of Frost's hit effect). At the burst each game checks its own
    ///     player: inside the radius and alive, it takes the frost as a hit with no attacker (so no per-player enemy scaling
    ///     and no Brutal again: Brutal is already in the number), which armour, frost resistance, blocking and dodging reduce
    ///     like any hit, and the game's frost slows it. Players only (decision 9): enemies are never hit, so bursts can't chain.
    /// </summary>
    internal static class Rimeburst
    {
        private const string RpcBurst = "Corruption_Rimeburst";
        private static readonly Color Ice = new Color(0.55f, 0.85f, 1f);
        private static bool s_registered;
        private static bool s_loggedLook;

        /// <summary>Registers the burst message, once per game session (also on a dedicated server, which ignores it).</summary>
        public static void Register()
        {
            if (ZRoutedRpc.instance == null)
            {
                return;
            }
            ZRoutedRpc.instance.Register<Vector3, float>(RpcBurst, RPC_Burst);
            s_registered = true;
        }

        /// <summary>The frost the burst deals: the share of the creature's strongest hit (its weapons, its stars, Brutal).</summary>
        private static float FrostOf(Character creature, CreatureAffixes affixes)
        {
            var damages = new List<double>();
            if (creature is Humanoid humanoid && humanoid.GetInventory() != null)
            {
                foreach (var item in humanoid.GetInventory().GetAllItems())
                {
                    if (item == null || !item.IsWeapon())
                    {
                        continue;
                    }
                    var d = item.GetDamage();
                    // Fighting damage only: chop and pickaxe are for breaking buildings and rocks.
                    double total = d.m_damage + d.m_blunt + d.m_slash + d.m_pierce + d.m_fire + d.m_frost + d.m_lightning + d.m_poison + d.m_spirit;
                    var shared = item.m_shared;
                    double multiplier = shared.m_attack != null ? shared.m_attack.m_damageMultiplier : 1.0;
                    if (shared.m_secondaryAttack != null && !string.IsNullOrEmpty(shared.m_secondaryAttack.m_attackAnimation))
                    {
                        multiplier = System.Math.Max(multiplier, shared.m_secondaryAttack.m_damageMultiplier);
                    }
                    damages.Add(total * multiplier);
                }
            }
            double hit = RipTierRules.StrongestHit(damages, creature.GetLevel(), affixes.Damage);
            return (float)RipTierRules.IceBurstDamage(affixes.IceBurst, hit);
        }

        /// <summary>
        ///     On the dying creature's owner, before the game removes it: a stamped creature with Rimeburst sends its burst.
        ///     Reads only. Normal priority, never skips (the other <c>Character.OnDeath</c> prefixes also only read).
        ///     Game: Character.OnDeath (Character.cs, 1.0.14 code).
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        private static class Burst
        {
            private static void Prefix(Character __instance)
            {
                if (!__instance || __instance is Player)
                {
                    return;
                }
                try
                {
                    if (!s_registered || !RipCreatures.IsOwner(__instance))
                    {
                        return;
                    }
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes == null || affixes.IceBurst <= 0f)
                    {
                        return;
                    }
                    float frost = FrostOf(__instance, affixes);
                    Vector3 at = __instance.transform.position;
                    Jotunn.Logger.LogInfo($"Rimeburst: {Utils.GetPrefabName(__instance.gameObject)} (level {__instance.GetLevel()}) bursts in " +
                                          $"{RipTierRules.IceBurstDelay} s for {frost:0.#} frost ({affixes.IceBurst * 100f:0}% of its hit).");
                    if (frost <= 0f)
                    {
                        return;
                    }
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcBurst, at, frost);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("send a Rimeburst", ex);
                }
            }
        }

        /// <summary>The game's message registration, once a world starts.</summary>
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpc
        {
            private static void Postfix()
            {
                Safe.Run("register the Rimeburst message", Register);
            }
        }

        private static void RPC_Burst(long sender, Vector3 at, float frost)
        {
            Safe.Run("show a Rimeburst", () =>
            {
                if (!Player.m_localPlayer)
                {
                    return;   // a dedicated server: nothing to see and nobody to hit
                }
                if (Vector3.Distance(Player.m_localPlayer.transform.position, at) > 100f)
                {
                    return;   // too far to see or be hit
                }
                var go = new GameObject("Corruption_Rimeburst");
                go.transform.position = at;
                go.AddComponent<Warning>().Begin(at, frost);
            });
        }

        /// <summary>The impact effect of a vanilla projectile (the staff's or the arrow's), as Orlog's Freezing Arrow plays it.</summary>
        private static bool ImpactEffect(string item, Vector3 point)
        {
            var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(item) : null;
            var projectile = prefab ? prefab.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_attack.m_attackProjectile : null;
            var effects = projectile ? projectile.GetComponent<Projectile>()?.m_hitEffects : null;
            if (effects == null || !effects.HasEffects())
            {
                return false;
            }
            effects.Create(point, Quaternion.identity);
            return true;
        }

        private static Material s_material;
        private static bool s_noMaterial;

        /// <summary>A plain unlit material from a shader every Unity build has (Orlog's Forked Lightning uses the same).</summary>
        private static Material LineMaterial()
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
            return null;
        }

        /// <summary>The warning on one game: a ring and a growing light for 1.5 s, then the burst and its hit on the local player.</summary>
        private sealed class Warning : MonoBehaviour
        {
            private Vector3 m_at;
            private float m_frost;
            private float m_start;
            private Light m_light;
            private LineRenderer m_ring;
            private bool m_burst;

            public void Begin(Vector3 at, float frost)
            {
                m_at = at;
                m_frost = frost;
                m_start = Time.time;
                float radius = (float)RipTierRules.IceBurstRadius;
                bool arrow = ImpactEffect("ArrowFrost", at);
                m_light = gameObject.AddComponent<Light>();
                m_light.type = LightType.Point;
                m_light.color = Ice;
                m_light.range = radius * 2f;
                m_light.intensity = 0.5f;
                var material = LineMaterial();
                if (material)
                {
                    m_ring = gameObject.AddComponent<LineRenderer>();
                    m_ring.sharedMaterial = material;
                    m_ring.useWorldSpace = true;
                    m_ring.loop = true;
                    m_ring.startWidth = 0.12f;
                    m_ring.endWidth = 0.12f;
                    m_ring.startColor = Ice;
                    m_ring.endColor = Ice;
                    m_ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    m_ring.receiveShadows = false;
                    const int points = 32;
                    m_ring.positionCount = points;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = i * Mathf.PI * 2f / points;
                        var p = at + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                        p.y = ZoneSystem.instance ? ZoneSystem.instance.GetGroundHeight(p) + 0.1f : at.y + 0.1f;
                        m_ring.SetPosition(i, p);
                    }
                }
                if (!s_loggedLook)
                {
                    s_loggedLook = true;
                    Jotunn.Logger.LogInfo($"Rimeburst: the warning shows {(m_ring ? "a ring" : "no ring (no plain shader)")}, a light" +
                                          $"{(arrow ? " and the frost arrow's hit" : "")}.");
                }
            }

            private void Update()
            {
                float t = Time.time - m_start;
                float delay = (float)RipTierRules.IceBurstDelay;
                if (!m_burst)
                {
                    if (m_light)
                    {
                        m_light.intensity = Mathf.Lerp(0.5f, 4f, t / delay);
                    }
                    if (m_ring)
                    {
                        // Pulses faster as the burst nears.
                        float a = 0.55f + 0.45f * Mathf.Sin(t * (6f + 10f * t / delay));
                        var c = new Color(Ice.r, Ice.g, Ice.b, a);
                        m_ring.startColor = c;
                        m_ring.endColor = c;
                    }
                    if (t >= delay)
                    {
                        m_burst = true;
                        Safe.Run("burst a Rimeburst", Explode);
                    }
                    return;
                }
                if (m_light)
                {
                    m_light.intensity = Mathf.Max(0f, m_light.intensity - Time.deltaTime * 12f);
                }
                if (t > delay + 0.5f)
                {
                    Destroy(gameObject);
                }
            }

            private void Explode()
            {
                if (m_ring)
                {
                    m_ring.enabled = false;
                }
                if (m_light)
                {
                    m_light.intensity = 6f;
                }
                if (!ImpactEffect("StaffIceShards", m_at))
                {
                    ImpactEffect("ArrowFrost", m_at);
                }
                var player = Player.m_localPlayer;
                if (!player || player.IsDead())
                {
                    return;
                }
                float radius = (float)RipTierRules.IceBurstRadius;
                Vector3 nearest = player.m_collider ? player.m_collider.ClosestPoint(m_at) : player.transform.position;
                if ((nearest - m_at).sqrMagnitude > radius * radius)
                {
                    return;
                }
                var hit = new HitData();
                hit.m_damage.m_frost = m_frost;
                hit.m_point = player.GetCenterPoint();
                Vector3 dir = player.transform.position - m_at;
                dir.y = 0f;
                hit.m_dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : -player.transform.forward;
                hit.m_pushForce = 0f;
                hit.m_staggerMultiplier = 0f;
                hit.m_blockable = true;
                hit.m_dodgeable = true;
                hit.m_radius = radius;
                hit.m_hitType = HitData.HitType.EnemyHit;
                player.Damage(hit);
                Jotunn.Logger.LogInfo($"Rimeburst: caught in a burst of {m_frost:0.#} frost (before armour and resistance).");
            }
        }
    }

    /// <summary>
    ///     Guarded (corrupted-only, #97; Krogon's decisions 11-18 of 2026-11-28, branch claude/wizardly-wozniak-4aix76): the
    ///     biome's own boss, corrupted, appears at the heart when the waves start and hunts the players; the heart takes no
    ///     damage while it lives. It carries the rip's stamp (the map's other affixes act on it, decision 17), is a corrupted
    ///     boss in every way (red, 1.5x / 0.7x, summons, the corrupted boss's drops: decisions 14 and 16), and is marked
    ///     with the heart it guards (<see cref="KeyGuardianOf" />). Its later phases (Kall's) carry the mark on
    ///     (<see cref="CarryToNextPhase" />). When the rip closes it leaves like the other rip creatures.
    /// </summary>
    internal static class RipGuardian
    {
        /// <summary>On the heart: the guardian has been spawned (bool). Once per rip.</summary>
        public const string KeySpawned = "corruption_rip_guardian_spawned";

        /// <summary>On the guardian boss: the heart it guards (the heart's ZDO id as text).</summary>
        public const string KeyGuardianOf = "corruption_rip_guardian_of";

        /// <summary>How far from the heart the boss appears, in metres (on the far side from the players; Claude's choice).</summary>
        private const float SpawnOffset = 6f;

        /// <summary>Called by the heart's owner once, when the waves start.</summary>
        public static void OnWavesStart(ZNetView heart, List<Player> players)
        {
            Safe.Run("summon the heart's guardian", () =>
            {
                if (!MapTiers.Loaded || !heart || !heart.IsValid() || !heart.IsOwner())
                {
                    return;
                }
                var zdo = heart.GetZDO();
                if (zdo.GetBool(KeySpawned) || !RipTierRules.HasGuardian(MapTiers.AffixesOf(zdo)))
                {
                    return;
                }
                zdo.Set(KeySpawned, true);
                string biome = zdo.GetString(Rips.KeyBiome, "");
                var map = MapItems.ByItem.Values.FirstOrDefault(m => m.Biome == biome);
                EssenceTable.Essence essence = null;
                if (map == null || !EssenceItems.ByLook.TryGetValue(map.Essence, out essence))
                {
                    Jotunn.Logger.LogWarning($"Guarded: no boss found for biome '{biome}'; this heart has no guardian.");
                    return;
                }
                string prefabName = RipTierRules.GuardianPrefab(essence.Boss);
                var prefab = ZNetScene.instance ? ZNetScene.instance.GetPrefab(prefabName) : null;
                if (!prefab)
                {
                    Jotunn.Logger.LogWarning($"Guarded: the boss '{prefabName}' isn't in this game version; this heart has no guardian.");
                    return;
                }
                Vector3 heartAt = heart.transform.position;
                Vector3 away = Vector3.forward;
                if (players != null && players.Count > 0 && players[0])
                {
                    away = heartAt - players[0].transform.position;
                    away.y = 0f;
                    away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
                }
                Vector3 at = heartAt + away * SpawnOffset;
                at.y = ZoneSystem.instance.GetGroundHeight(at);
                var go = Object.Instantiate(prefab, at, Quaternion.LookRotation(-away));
                var boss = go.GetComponent<Character>();
                if (!boss || !boss.m_nview || !boss.m_nview.IsValid())
                {
                    Jotunn.Logger.LogWarning($"Guarded: '{prefabName}' has no character; this heart has no guardian.");
                    return;
                }
                CorruptedBosses.Corrupt(boss);
                var bossZdo = boss.m_nview.GetZDO();
                MapTiers.CopyStamp(zdo, bossZdo);
                bossZdo.Set(Rips.KeyClose, zdo.GetLong(Rips.KeyClose, 0L));
                bossZdo.Set(KeyGuardianOf, zdo.m_uid.ToString());
                MapAffixEffects.OnStamped(boss);
                boss.GetBaseAI()?.SetHuntPlayer(true);
                if (players != null && players.Count > 0 && players[0] && boss.GetBaseAI() is MonsterAI ai)
                {
                    ai.SetTarget(players[0]);
                }
                Jotunn.Logger.LogInfo($"Guarded: a corrupted {prefabName} guards rip {zdo.m_uid}; the heart takes no damage while it lives.");
            });
        }

        /// <summary>Whether the heart's guardian lives (loaded on this game). False for a heart without Guarded.</summary>
        public static bool IsGuarded(ZDO heart)
        {
            if (heart == null || !MapTiers.Loaded || !RipTierRules.HasGuardian(MapTiers.AffixesOf(heart)))
            {
                return false;
            }
            string id = heart.m_uid.ToString();
            foreach (var character in Character.GetAllCharacters())
            {
                if (!character || !character.IsBoss() || character.IsDead() || !character.m_nview || !character.m_nview.IsValid())
                {
                    continue;
                }
                if (character.m_nview.GetZDO().GetString(KeyGuardianOf, "") == id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     The heart takes no damage while its guardian lives (decision 13). On the heart's owner, where the game applies
        ///     the damage; the hit is dropped (the original skipped) and nothing else happens. Any other object is untouched.
        ///     Priority First so the heart's hit is refused before anything else counts it.
        ///     Game: Destructible.RPC_Damage (Destructible.cs, 1.0.14 code). Prefix; skips the original only for a guarded heart.
        /// </summary>
        [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
        private static class ProtectHeart
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(Destructible __instance)
            {
                try
                {
                    if (!__instance || !__instance.GetComponent<RipHeart>() || !__instance.m_nview || !__instance.m_nview.IsValid() ||
                        !__instance.m_nview.IsOwner())
                    {
                        return true;
                    }
                    var zdo = __instance.m_nview.GetZDO();
                    bool has = MapTiers.Loaded && RipTierRules.HasGuardian(MapTiers.AffixesOf(zdo));
                    return RipTierRules.HeartCanBeDamaged(has, has && IsGuarded(zdo));
                }
                catch (System.Exception ex)
                {
                    Safe.Report("protect a guarded heart", ex);
                    return true;
                }
            }
        }

        // ---- Kall's phases: the mark goes on to the next phase ----

        private sealed class Mark
        {
            public string Heart;
            public int Tier;
            public string Affixes;
            public long Close;
        }

        private static readonly Dictionary<string, Mark> s_pending = new Dictionary<string, Mark>();
        private static readonly System.Text.RegularExpressions.Regex Phase = new System.Text.RegularExpressions.Regex(@"^(.+?)(_p\d+)?$");

        private static string BaseName(Character c) => Phase.Match(Utils.GetPrefabName(c.gameObject)).Groups[1].Value;

        /// <summary>Called by the corrupted boss phase hooks (<see cref="CorruptedOfferings" />) as a boss dies on its owner.</summary>
        public static void RememberForNextPhase(Character boss)
        {
            var zdo = boss.m_nview.GetZDO();
            string heart = zdo.GetString(KeyGuardianOf, "");
            if (heart.Length == 0)
            {
                return;
            }
            s_pending[BaseName(boss)] = new Mark
            {
                Heart = heart, Tier = MapTiers.TierOf(zdo), Affixes = zdo.GetString(MapTiers.KeyAffixes, ""), Close = zdo.GetLong(Rips.KeyClose, 0L)
            };
        }

        /// <summary>Called as a boss's next phase appears (on the same game): it guards the same heart, with the same stamp.</summary>
        public static void CarryToNextPhase(Character boss)
        {
            string name = Utils.GetPrefabName(boss.gameObject);
            if (!Phase.Match(name).Groups[2].Success || !s_pending.TryGetValue(BaseName(boss), out var mark))
            {
                return;
            }
            s_pending.Remove(BaseName(boss));
            boss.m_nview.ClaimOwnership();
            var zdo = boss.m_nview.GetZDO();
            MapTiers.Stamp(zdo, mark.Tier, mark.Affixes);
            zdo.Set(Rips.KeyClose, mark.Close);
            zdo.Set(KeyGuardianOf, mark.Heart);
            MapAffixEffects.OnStamped(boss);
            boss.GetBaseAI()?.SetHuntPlayer(true);
            Jotunn.Logger.LogInfo($"Guarded: {name} goes on guarding rip {mark.Heart}.");
        }
    }

    /// <summary>
    ///     A guardian boss leaves once its rip has closed, like the other rip creatures (the game's event-creature despawn when
    ///     no player is near). Added to every boss by <see cref="CorruptedBosses" />'s watcher; acts only on a guardian.
    /// </summary>
    internal sealed class GuardianLeaves : MonoBehaviour
    {
        private Character m_character;

        private void Awake()
        {
            m_character = GetComponent<Character>();
            InvokeRepeating(nameof(Tick), 5f, 5f);
        }

        private void Tick()
        {
            Safe.Run("let a guardian boss leave", () =>
            {
                if (!m_character || !m_character.m_nview || !m_character.m_nview.IsValid() || !m_character.m_nview.IsOwner())
                {
                    return;
                }
                var zdo = m_character.m_nview.GetZDO();
                if (zdo.GetString(RipGuardian.KeyGuardianOf, "").Length == 0 || zdo.GetLong(Rips.KeyClose, 0L) <= 0 || !Rips.IsExpired(zdo))
                {
                    return;
                }
                var ai = GetComponent<MonsterAI>();
                if (ai && !ai.IsEventCreature())
                {
                    ai.SetEventCreature(true);
                }
            });
        }
    }
}
