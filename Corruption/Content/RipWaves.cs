using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     A rip's waves (design phase 4, step B), run by the game that owns the rip heart, like vanilla raids run on
    ///     a player's game. The waves are the rip biome's boss army: the game's own raid(s) for that boss, with their
    ///     own creatures, counts, intervals, chances and group sizes (<see cref="RipRules.ArmyRaids" />). They start
    ///     when a player comes within the raid's range, maddening the creatures already there, and keep coming while
    ///     the heart stands. Spawned creatures are maddened and have <c>MinStars</c> to <c>MaxStars</c> stars (user; design tier 1).
    /// </summary>
    internal class RipWaves : MonoBehaviour
    {
        public const string KeyStarted = "corruption_rip_waves_started";
        public const string KeyFromRip = "corruption_rip_spawned_by";

        private ZNetView m_nview;
        private readonly List<RandomEvent> m_raids = new List<RandomEvent>();
        private readonly Dictionary<SpawnSystem.SpawnData, float> m_timers = new Dictionary<SpawnSystem.SpawnData, float>();
        private readonly List<Player> m_players = new List<Player>();
        private float m_range;
        private bool m_resolved;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            if (!m_nview || m_nview.GetZDO() == null)
            {
                enabled = false;
                return;
            }
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            Safe.Run("run rip waves", () =>
            {
                if (!m_nview.IsValid() || !m_nview.IsOwner() || Rips.IsExpired(m_nview.GetZDO()))
                {
                    return;
                }
                if (!m_resolved)
                {
                    Resolve();
                }
                if (m_raids.Count == 0)
                {
                    return;
                }
                m_players.Clear();
                Player.GetPlayersInRange(transform.position, m_range, m_players);
                if (m_players.Count == 0)
                {
                    return;   // like vanilla raids: paused while nobody is there
                }
                var zdo = m_nview.GetZDO();
                if (!zdo.GetBool(KeyStarted))
                {
                    zdo.Set(KeyStarted, true);
                    Begin();
                }
                foreach (var raid in m_raids)
                {
                    foreach (var entry in raid.m_spawn.Where(s => s != null && s.m_enabled && s.m_prefab))
                    {
                        UpdateEntry(entry);
                    }
                }
            });
        }

        /// <summary>Finds the rip biome's army raids: the boss whose essence makes this biome's maps.</summary>
        private void Resolve()
        {
            m_resolved = true;
            string biome = m_nview.GetZDO().GetString(Rips.KeyBiome, "");
            var map = MapItems.ByItem.Values.FirstOrDefault(m => m.Biome == biome);
            if (map == null || !EssenceItems.ByLook.TryGetValue(map.Essence, out var essence) || !RandEventSystem.instance)
            {
                Jotunn.Logger.LogWarning($"Rip waves: no boss army found for biome '{biome}'; this rip has no waves.");
                return;
            }
            m_raids.AddRange(BossArmies.Find(essence.Boss, out m_range));
            if (m_raids.Count == 0)
            {
                Jotunn.Logger.LogWarning($"Rip waves: {essence.Boss} has no army raid in this game version; this rip has no waves.");
            }
        }

        /// <summary>The rip tears open: the raid's message, and every creature already nearby turns maddened and hunts.</summary>
        private void Begin()
        {
            string message = m_raids.Select(r => r.m_startMessage).FirstOrDefault(m => !string.IsNullOrEmpty(m));
            foreach (var player in m_players)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    player.Message(MessageHud.MessageType.Center, message);
                }
            }
            foreach (var character in Character.GetAllCharacters().ToList())
            {
                if (character.IsPlayer() || character.IsTamed() || character.IsBoss() ||
                    Vector3.Distance(character.transform.position, transform.position) > m_range)
                {
                    continue;
                }
                Maddened.Madden(character);
                character.GetBaseAI()?.SetHuntPlayer(true);
            }
        }

        /// <summary>One raid entry, following SpawnSystem.UpdateSpawnList: spawns per elapsed interval, chance, count limit, groups.</summary>
        private void UpdateEntry(SpawnSystem.SpawnData entry)
        {
            // The raid made busier (user, 2026-10-29): more alive at once, shorter interval.
            float multiplier = Mathf.Max(1f, CorruptionConfig.RipWaveMultiplier.Value);
            int maxSpawned = entry.m_maxSpawned > 0 ? Mathf.RoundToInt(entry.m_maxSpawned * multiplier) : 0;
            float interval = Mathf.Max(entry.m_spawnInterval / multiplier, 1f);
            m_timers.TryGetValue(entry, out float elapsed);
            elapsed += 1f;
            int due = Mathf.Min(maxSpawned == 0 ? 1 : maxSpawned, (int)(elapsed / interval));
            m_timers[entry] = due > 0 ? 0f : elapsed;
            for (int i = 0; i < due; i++)
            {
                if (Random.Range(0f, 100f) > entry.m_spawnChance)
                {
                    continue;
                }
                if (!BossArmies.CanSpawnNow(entry))
                {
                    break;
                }
                int alive = BossArmies.CountAlive(KeyFromRip, m_nview.GetZDO().m_uid.ToString(), entry.m_prefab.name);
                if (maxSpawned > 0 && alive >= maxSpawned)
                {
                    break;
                }
                if (!BossArmies.FindSpawnCenter(entry, m_players, out var center))
                {
                    continue;
                }
                int group = Mathf.Min(Random.Range(entry.m_groupSizeMin, entry.m_groupSizeMax + 1),
                    maxSpawned > 0 ? maxSpawned - alive : 100);
                float radius = group > 1 ? entry.m_groupRadius : 0f;
                for (int g = 0; g < group; g++)
                {
                    var offset = Random.insideUnitCircle * radius;
                    var point = center + new Vector3(offset.x, 0f, offset.y);
                    point.y = ZoneSystem.instance.GetGroundHeight(point);
                    // MinStars to MaxStars: user, 2026-10-29 (every rip spawn at least 1 star); design tier 1.
                    BossArmies.Spawn(entry, point, KeyFromRip, m_nview.GetZDO().m_uid.ToString(),
                        CorruptionConfig.RipMinStars.Value, CorruptionConfig.RipMaxStars.Value);
                }
            }
        }
    }
}
