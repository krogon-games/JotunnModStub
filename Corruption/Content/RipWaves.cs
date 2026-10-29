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
    ///     the heart stands. Spawned creatures are maddened and can have up to <c>MaxStars</c> stars (design tier 1).
    /// </summary>
    internal class RipWaves : MonoBehaviour
    {
        public const string KeyStarted = "corruption_rip_waves_started";
        public const string KeyFromRip = "corruption_rip_spawned_by";

        // Vanilla raid spawn rules (SpawnSystem.FindBaseSpawnPoint): groups appear 40-80 m from a player unless the
        // raid entry sets its own radius.
        private const float DefaultSpawnRadiusMin = 40f;
        private const float DefaultSpawnRadiusMax = 80f;

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
            var boss = ZNetScene.instance.GetPrefab(essence.Boss);
            string defeatKey = boss ? boss.GetComponent<Character>()?.m_defeatSetGlobalKey : null;
            var events = RandEventSystem.instance.m_events.Where(e => e != null).ToList();
            var names = RipRules.ArmyRaids(events.Select(e => new RipRules.RaidInfo
            {
                Name = e.m_name,
                Enabled = e.m_enabled,
                NotRequiredKeys = e.m_notRequiredGlobalKeys,
                SpawnEntries = e.m_spawn?.Count ?? 0
            }), defeatKey);
            m_raids.AddRange(events.Where(e => names.Contains(e.m_name)));
            m_range = m_raids.Count > 0 ? m_raids.Max(r => r.m_eventRange) : 0f;
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
                if ((!entry.m_spawnAtDay && EnvMan.IsDay()) || (!entry.m_spawnAtNight && EnvMan.IsNight()))
                {
                    break;
                }
                int alive = CountAlive(entry.m_prefab.name);
                if (maxSpawned > 0 && alive >= maxSpawned)
                {
                    break;
                }
                if (!FindSpawnCenter(entry, out var center))
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
                    Spawn(entry, point);
                }
            }
        }

        private bool FindSpawnCenter(SpawnSystem.SpawnData entry, out Vector3 center)
        {
            float min = entry.m_spawnRadiusMin > 0f ? entry.m_spawnRadiusMin : DefaultSpawnRadiusMin;
            float max = entry.m_spawnRadiusMax > 0f ? entry.m_spawnRadiusMax : DefaultSpawnRadiusMax;
            float water = ZoneSystem.instance.m_waterLevel;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var player = m_players[Random.Range(0, m_players.Count)];
                var direction = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
                var point = player.transform.position + direction * Random.Range(min, max);
                point.y = ZoneSystem.instance.GetGroundHeight(point);
                if (point.y > water && !ZoneSystem.instance.IsBlocked(point))
                {
                    center = point;
                    return true;
                }
            }
            center = Vector3.zero;
            return false;
        }

        private void Spawn(SpawnSystem.SpawnData entry, Vector3 point)
        {
            var go = Instantiate(entry.m_prefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            var nview = go.GetComponent<ZNetView>();
            nview?.GetZDO()?.Set(KeyFromRip, m_nview.GetZDO().m_uid.ToString());
            var character = go.GetComponent<Character>();
            if (!character)
            {
                return;   // e.g. the Ashlands army's charred spawner stones
            }
            if (entry.m_huntPlayer)
            {
                character.GetBaseAI()?.SetHuntPlayer(true);
            }
            // Stars as vanilla rolls them (SpawnSystem.Spawn), up to the design's tier 1 limit.
            int maxLevel = Mathf.Max(entry.m_maxLevel, CorruptionConfig.RipMaxStars.Value + 1);
            int level = entry.m_minLevel;
            float chance = SpawnSystem.GetLevelUpChance(point, entry);
            while (level < maxLevel && Random.Range(0f, 100f) <= chance)
            {
                level++;
            }
            if (level > 1)
            {
                character.SetLevel(level);
            }
            Maddened.Madden(character);
        }

        /// <summary>This rip's living spawns of one prefab (spawned creatures carry the heart's id).</summary>
        private int CountAlive(string prefabName)
        {
            string id = m_nview.GetZDO().m_uid.ToString();
            int count = 0;
            foreach (var character in Character.GetAllCharacters())
            {
                var zdo = character.m_nview ? character.m_nview.GetZDO() : null;
                if (zdo != null && !character.IsDead() && zdo.GetString(KeyFromRip, "") == id &&
                    Utils.GetPrefabName(character.gameObject) == prefabName)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
