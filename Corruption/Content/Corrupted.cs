using UnityEngine;

namespace Corruption.Content
{
    /// <summary>The "corrupted" look: a purple tint on a cloned prefab's own copies of its materials.</summary>
    internal static class Corrupted
    {
        public static readonly Color Purple = new Color(0.55f, 0.2f, 0.85f, 1f);

        /// <param name="strength">0 = unchanged, 1 = fully purple.</param>
        public static void Tint(GameObject prefab, float strength = 0.6f)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                var shared = renderer.sharedMaterials;
                var copies = new Material[shared.Length];
                for (int i = 0; i < shared.Length; i++)
                {
                    if (!shared[i])
                    {
                        continue;
                    }
                    // Copies, so the vanilla item using the same material keeps its colour.
                    copies[i] = new Material(shared[i]);
                    if (copies[i].HasProperty("_Color"))
                    {
                        copies[i].color = Color.Lerp(copies[i].color, Purple, strength);
                    }
                }
                renderer.sharedMaterials = copies;
            }
        }

        /// <summary>Renders an icon from the tinted prefab; falls back to the cloned item's icon if rendering fails.</summary>
        public static Sprite RenderIcon(GameObject prefab)
        {
            try
            {
                return Jotunn.Managers.RenderManager.Instance.Render(prefab);
            }
            catch (System.Exception ex)
            {
                Jotunn.Logger.LogWarning($"Could not render an icon for {prefab.name}: {ex.Message}");
                return null;
            }
        }
    }
}
