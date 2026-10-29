using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The rip's two objects: the rip portal (a red stone portal, used both ways) and the rip heart (a giant
    ///     corruption shard on a destructible base). Both are spawned by <see cref="Rips" />, never built.
    /// </summary>
    internal static class RipPrefabs
    {
        private static readonly Color Red = new Color(0.7f, 0.09f, 0.05f);

        public static void Register()
        {
            Safe.Run("register the rip portal", RegisterPortal);
            Safe.Run("register the rip heart", RegisterHeart);
        }

        private static void RegisterPortal()
        {
            string basePrefab = PrefabManager.Instance.GetPrefab("portal_stone") ? "portal_stone" : "portal_wood";
            var prefab = PrefabManager.Instance.CreateClonedPrefab(Texts.RipPrefab, basePrefab);
            var portal = prefab.AddComponent<RipPortal>();

            // Keep the vanilla portal's model and swirl, but replace its logic (tags, pairing) with ours.
            var vanilla = prefab.GetComponent<TeleportWorld>();
            if (vanilla)
            {
                portal.m_targetFound = vanilla.m_target_found;
                portal.m_model = vanilla.m_model;
            }
            foreach (var trigger in prefab.GetComponentsInChildren<TeleportWorldTrigger>(true))
            {
                var go = trigger.gameObject;
                Object.DestroyImmediate(trigger);
                go.AddComponent<RipTrigger>();
            }
            if (vanilla)
            {
                Object.DestroyImmediate(vanilla);
            }
            // Not a building: it can't be damaged, repaired or taken down with the hammer.
            var wear = prefab.GetComponent<WearNTear>();
            if (wear)
            {
                Object.DestroyImmediate(wear);
            }
            var piece = prefab.GetComponent<Piece>();
            if (piece)
            {
                Object.DestroyImmediate(piece);
            }

            Corrupted.Tint(prefab, Red, 0.6f);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
        }

        private static void RegisterHeart()
        {
            var prefab = PrefabManager.Instance.CreateClonedPrefab(Texts.RipHeartPrefab, "Spawner_GreydwarfNest");
            var destructible = prefab.GetComponent<Destructible>();
            if (!destructible)
            {
                Jotunn.Logger.LogError("Rip heart: the base object has no Destructible; rip hearts won't work.");
                return;
            }
            // Only the destructible shell stays: no greydwarf spawning, no nest drops, our own hover text.
            foreach (var spawner in prefab.GetComponentsInChildren<SpawnArea>(true))
            {
                Object.DestroyImmediate(spawner);
            }
            foreach (var drop in prefab.GetComponentsInChildren<DropOnDestroyed>(true))
            {
                Object.DestroyImmediate(drop);
            }
            foreach (var hover in prefab.GetComponentsInChildren<HoverText>(true))
            {
                Object.DestroyImmediate(hover);
            }
            destructible.m_minToolTier = 0;
            destructible.m_spawnWhenDestroyed = null;
            destructible.m_spawnWhenDamaged = null;
            prefab.AddComponent<RipHeart>();
            prefab.AddComponent<RipWaves>();

            bool custom = false;
            try
            {
                custom = GemModel.Apply(prefab, new GemModel.Look
                {
                    Name = "CorruptionRipHeart", Mesh = "shard.obj", Albedo = "shard_albedo.rgba",
                    Emission = "shard_emission.rgba", Scale = 6f
                }, Red);
            }
            catch (System.Exception ex)
            {
                Jotunn.Logger.LogError($"Rip heart model failed; using the tinted nest instead. {ex}");
            }
            if (!custom)
            {
                Corrupted.Tint(prefab, Red, 0.6f);
            }
            GemModel.AddGlow(prefab, Red, 3f, 10f, 1f);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
        }
    }
}
