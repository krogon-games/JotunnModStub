using UnityEngine;
using UnityEngine.Rendering;

namespace Corruption.Content
{
    /// <summary>The "corrupted" look: a colour tint on a cloned prefab's own copies of its materials.</summary>
    internal static class Corrupted
    {
        /// <summary>
        ///     Recolours every colour property of every material (base colour, emission, glow...), keeping each
        ///     property's brightness. Vanilla shaders use different property names, so all of them are changed.
        /// </summary>
        /// <param name="strength">0 = unchanged, 1 = fully the target colour.</param>
        public static void Tint(GameObject prefab, Color target, float strength)
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
                    TintMaterial(copies[i], target, strength);
                }
                renderer.sharedMaterials = copies;
            }
        }

        private static void TintMaterial(Material material, Color target, float strength)
        {
            var shader = material.shader;
            int count = shader.GetPropertyCount();
            for (int p = 0; p < count; p++)
            {
                if (shader.GetPropertyType(p) != ShaderPropertyType.Color)
                {
                    continue;
                }
                string name = shader.GetPropertyName(p);
                var original = material.GetColor(name);
                material.SetColor(name, Recolor(original, target, strength));
            }
        }

        /// <summary>Same brightness and alpha, hue moved towards the target.</summary>
        private static Color Recolor(Color original, Color target, float strength)
        {
            float brightness = Mathf.Max(original.r, Mathf.Max(original.g, original.b));
            float targetMax = Mathf.Max(target.r, Mathf.Max(target.g, target.b, 0.0001f));
            var tinted = new Color(target.r / targetMax * brightness, target.g / targetMax * brightness,
                target.b / targetMax * brightness, original.a);
            var result = Color.Lerp(original, tinted, strength);
            result.a = original.a;
            return result;
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
