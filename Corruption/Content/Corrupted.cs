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
            // Particles and lights carry most of a station's glow, so they are tinted too.
            foreach (var particles in prefab.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.startColor = Recolor(main.startColor, target, strength);
            }
            foreach (var light in prefab.GetComponentsInChildren<Light>(true))
            {
                light.color = Recolor(light.color, target, strength);
            }

            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
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

        private static ParticleSystem.MinMaxGradient Recolor(ParticleSystem.MinMaxGradient value, Color target, float strength)
        {
            switch (value.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(Recolor(value.color, target, strength));
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(Recolor(value.colorMin, target, strength),
                        Recolor(value.colorMax, target, strength));
                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(Recolor(value.gradient, target, strength));
                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(Recolor(value.gradientMin, target, strength),
                        Recolor(value.gradientMax, target, strength));
                default:
                    return value;
            }
        }

        private static Gradient Recolor(Gradient gradient, Color target, float strength)
        {
            if (gradient == null)
            {
                return null;
            }
            var keys = gradient.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].color = Recolor(keys[i].color, target, strength);
            }
            var copy = new Gradient { mode = gradient.mode };
            copy.SetKeys(keys, gradient.alphaKeys);
            return copy;
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
