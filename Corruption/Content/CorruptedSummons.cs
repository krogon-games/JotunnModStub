using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     A corrupted boss's summons (design phase 6), run by the game that owns the boss: every <c>SummonInterval</c>
    ///     seconds while players are within the army raid's range (96 m), 2-3 creatures of the boss's own army raid
    ///     (the same as its biome's rip waves; user, 2026-10-30) arrive like a raid, 40-80 m from a player. They are
    ///     maddened and have 1-2 stars (user: at least 1). At most <c>SummonsPerPlayer</c> are alive per player present
    ///     (user: 4). Summons fight players only (<see cref="Maddened.IsSummon" />). Every boss gets this component; it
    ///     does nothing unless the boss is corrupted.
    /// </summary>
    internal class CorruptedSummons : MonoBehaviour
    {
        /// <summary>On a summon's ZDO: the base prefab name of the boss that called it (shared by Kall's phases).</summary>
        public const string KeySummonedBy = "corruption_summoned_by";

        private Character m_boss;
        private string m_baseName;
        private readonly List<RandomEvent> m_raids = new List<RandomEvent>();
        private readonly List<SpawnSystem.SpawnData> m_entries = new List<SpawnSystem.SpawnData>();
        private readonly List<Player> m_players = new List<Player>();
        private float m_range;
        private bool m_resolved;
        private float m_elapsed;

        private void Awake()
        {
            m_boss = GetComponent<Character>();
            InvokeRepeating(nameof(Tick), 1f, 1f);
        }

        private void Tick()
        {
            Safe.Run("run a corrupted boss's summons", () =>
            {
                if (!m_boss || m_boss.IsDead() || !m_boss.m_nview || !m_boss.m_nview.IsValid() || !m_boss.m_nview.IsOwner() ||
                    !CorruptedBosses.IsCorrupted(m_boss))
                {
                    return;
                }
                float interval = CorruptionConfig.SummonInterval.Value;
                if (interval <= 0f)
                {
                    return;
                }
                if (!m_resolved)
                {
                    Resolve();
                }
                if (m_entries.Count == 0)
                {
                    return;
                }
                m_players.Clear();
                Player.GetPlayersInRange(transform.position, m_range, m_players);
                if (m_players.Count == 0)
                {
                    return;   // like raids: paused while nobody is near
                }
                m_elapsed += 1f;
                if (m_elapsed < interval)
                {
                    return;
                }
                m_elapsed = 0f;
                Summon();
            });
        }

        private void Resolve()
        {
            m_resolved = true;
            string prefab = Utils.GetPrefabName(gameObject);
            m_baseName = BossArmies.BaseName(prefab);
            m_raids.AddRange(BossArmies.FindForBoss(prefab, out m_range));
            // Only creatures: a raid can also spawn objects (the Ashlands army's charred spawner stones), which
            // couldn't be counted towards the limit.
            m_entries.AddRange(m_raids.SelectMany(r => r.m_spawn)
                .Where(s => s != null && s.m_enabled && s.m_prefab && s.m_prefab.GetComponent<Character>()));
            if (m_entries.Count == 0)
            {
                Jotunn.Logger.LogWarning($"Corrupted boss {prefab}: no army raid creatures found; it has no summons.");
            }
            else
            {
                Jotunn.Logger.LogInfo($"Corrupted boss {prefab}: summons from {string.Join(", ", m_raids.Select(r => r.m_name))} " +
                                      $"({string.Join(", ", m_entries.Select(e => e.m_prefab.name).Distinct())}).");
            }
        }

        private void Summon()
        {
            int limit = CorruptionConfig.SummonsPerPlayer.Value * m_players.Count;
            int alive = BossArmies.CountAlive(KeySummonedBy, m_baseName);
            int min = Mathf.Min(CorruptionConfig.SummonCountMin.Value, CorruptionConfig.SummonCountMax.Value);
            int count = Mathf.Min(Random.Range(min, CorruptionConfig.SummonCountMax.Value + 1), limit - alive);
            var now = m_entries.Where(BossArmies.CanSpawnNow).ToList();
            int summoned = 0;
            for (int i = 0; i < count && now.Count > 0; i++)
            {
                var entry = now[Random.Range(0, now.Count)];
                if (!BossArmies.FindSpawnCenter(entry, m_players, out var point))
                {
                    continue;
                }
                if (BossArmies.Spawn(entry, point, KeySummonedBy, m_baseName,
                        CorruptionConfig.SummonMinStars.Value, CorruptionConfig.SummonMaxStars.Value))
                {
                    summoned++;
                }
            }
            if (summoned > 0)
            {
                Jotunn.Logger.LogInfo($"Corrupted boss {m_baseName}: summoned {summoned} ({alive + summoned} alive, limit {limit}).");
            }
        }
    }
}
