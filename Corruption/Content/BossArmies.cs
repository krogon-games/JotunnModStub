using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     A boss's army: the game's own raid(s) that stop once that boss is defeated (<see cref="RipRules.ArmyRaids" />),
    ///     and spawning their creatures the way vanilla raids do. Used by rip waves and corrupted boss summons.
    /// </summary>
    internal static class BossArmies
    {
        // Vanilla raid spawn rules (SpawnSystem.FindBaseSpawnPoint): groups appear 40-80 m from a player unless the
        // raid entry sets its own radius.
        private const float DefaultSpawnRadiusMin = 40f;
        private const float DefaultSpawnRadiusMax = 80f;

        private static readonly Regex Phase = new Regex(@"^(.+)_p\d+$");

        /// <summary>A boss prefab's name without a phase suffix (FrozenKing_p2 -> FrozenKing).</summary>
        public static string BaseName(string prefab)
        {
            var match = Phase.Match(prefab ?? "");
            return match.Success ? match.Groups[1].Value : prefab ?? "";
        }

        /// <summary>
        ///     The army raids of the boss that drops an essence (by its prefab name, e.g. Eikthyr, FrozenKing_p3), and the
        ///     largest raid range among them. Empty when the game has none.
        /// </summary>
        public static List<RandomEvent> Find(string essenceBoss, out float range)
        {
            range = 0f;
            var raids = new List<RandomEvent>();
            var boss = ZNetScene.instance ? ZNetScene.instance.GetPrefab(essenceBoss) : null;
            string defeatKey = boss ? boss.GetComponent<Character>()?.m_defeatSetGlobalKey : null;
            if (!RandEventSystem.instance || string.IsNullOrEmpty(defeatKey))
            {
                return raids;
            }
            var events = RandEventSystem.instance.m_events.Where(e => e != null).ToList();
            var names = RipRules.ArmyRaids(events.Select(e => new RipRules.RaidInfo
            {
                Name = e.m_name,
                Enabled = e.m_enabled,
                NotRequiredKeys = e.m_notRequiredGlobalKeys,
                SpawnEntries = e.m_spawn?.Count ?? 0
            }), defeatKey);
            raids.AddRange(events.Where(e => names.Contains(e.m_name)));
            range = raids.Count > 0 ? raids.Max(r => r.m_eventRange) : 0f;
            return raids;
        }

        /// <summary>The army raids of a boss as it appears in the world (any phase): through its essence's boss.</summary>
        public static List<RandomEvent> FindForBoss(string bossPrefab, out float range)
        {
            string baseName = BaseName(bossPrefab);
            var essence = EssenceItems.ByBoss.Values.FirstOrDefault(e => BaseName(e.Boss) == baseName);
            range = 0f;
            return essence != null ? Find(essence.Boss, out range) : new List<RandomEvent>();
        }

        /// <summary>Whether a raid entry may spawn now (its own day and night rules).</summary>
        public static bool CanSpawnNow(SpawnSystem.SpawnData entry)
        {
            return (entry.m_spawnAtDay || !EnvMan.IsDay()) && (entry.m_spawnAtNight || !EnvMan.IsNight());
        }

        /// <summary>A spawn point the raid entry's distance from a random one of the players, on dry, unblocked ground.</summary>
        public static bool FindSpawnCenter(SpawnSystem.SpawnData entry, IList<Player> players, out Vector3 center)
        {
            float min = entry.m_spawnRadiusMin > 0f ? entry.m_spawnRadiusMin : DefaultSpawnRadiusMin;
            float max = entry.m_spawnRadiusMax > 0f ? entry.m_spawnRadiusMax : DefaultSpawnRadiusMax;
            float water = ZoneSystem.instance.m_waterLevel;
            for (int attempt = 0; attempt < 20 && players.Count > 0; attempt++)
            {
                var player = players[Random.Range(0, players.Count)];
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

        /// <summary>
        ///     Spawns one creature of a raid entry, tagged (<paramref name="tagKey" /> = <paramref name="tagValue" /> on
        ///     its ZDO) so its spawner can count it, maddened, with <paramref name="minStars" /> to <paramref name="maxStars" />
        ///     stars rolled as vanilla rolls them (SpawnSystem.Spawn). Returns the creature, or null for non-creatures
        ///     (e.g. the Ashlands army's charred spawner stones).
        /// </summary>
        public static Character Spawn(SpawnSystem.SpawnData entry, Vector3 point, string tagKey, string tagValue, int minStars, int maxStars)
        {
            var go = Object.Instantiate(entry.m_prefab, point, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            go.GetComponent<ZNetView>()?.GetZDO()?.Set(tagKey, tagValue);
            var character = go.GetComponent<Character>();
            if (!character)
            {
                return null;
            }
            if (entry.m_huntPlayer)
            {
                character.GetBaseAI()?.SetHuntPlayer(true);
            }
            int maxLevel = Mathf.Max(entry.m_maxLevel, maxStars + 1);
            int level = Mathf.Min(Mathf.Max(entry.m_minLevel, minStars + 1), maxLevel);
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
            return character;
        }

        /// <summary>Living loaded creatures carrying a tag, optionally only of one prefab.</summary>
        public static int CountAlive(string tagKey, string tagValue, string prefabName = null)
        {
            int count = 0;
            foreach (var character in Character.GetAllCharacters())
            {
                var zdo = character.m_nview ? character.m_nview.GetZDO() : null;
                if (zdo != null && !character.IsDead() && zdo.GetString(tagKey, "") == tagValue &&
                    (prefabName == null || Utils.GetPrefabName(character.gameObject) == prefabName))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
