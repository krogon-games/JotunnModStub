using System;
using System.IO;
using System.Reflection;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Our own gem looks (corruption shard, boss essences): models and textures made by the scripts in tools/ and
    ///     embedded in the DLL. Replaces every visual of the item the gem was cloned from (which only provides the
    ///     item, physics and networking parts); the base item's colliders stay so it still lies on the ground the same way.
    /// </summary>
    internal static class GemModel
    {
        private const string ResourcePrefix = "Corruption.Assets.";

        /// <summary>Embedded asset names for one gem.</summary>
        public sealed class Look
        {
            public string Name;
            public string Mesh;
            public string Albedo;
            public string Emission;

            /// <summary>Optional extra part in a plain colour (the Elder's root, Yagluth's ring).</summary>
            public string AddonMesh;
            public Color AddonColor;
        }

        public static readonly Look Shard = new Look
        {
            Name = "CorruptionShard", Mesh = "shard.obj", Albedo = "shard_albedo.rgba", Emission = "shard_emission.rgba"
        };

        /// <summary>Swaps the prefab's visuals for the gem. Returns false (and changes nothing) if anything is missing.</summary>
        public static bool Apply(GameObject prefab, Look look, Color glowColor)
        {
            var template = FindTemplateMaterial(prefab);
            var mesh = LoadMesh(look.Mesh);
            var albedo = LoadTexture(look.Albedo);
            var emission = LoadTexture(look.Emission);
            Mesh addonMesh = string.IsNullOrEmpty(look.AddonMesh) ? null : LoadMesh(look.AddonMesh);
            if (!template || !mesh || !albedo || !emission || (!string.IsNullOrEmpty(look.AddonMesh) && !addonMesh))
            {
                Jotunn.Logger.LogWarning($"Custom model for {look.Name} unavailable (missing asset or material); keeping the base item's look.");
                return false;
            }

            // The game's own shader (from the base item's material), with our textures, so it lights like other items.
            var material = new Material(template) { name = look.Name };
            SetTexture(material, "_MainTex", albedo);
            SetTexture(material, "_BaseMap", albedo);
            SetColor(material, "_Color", Color.white);
            SetTexture(material, "_EmissionMap", emission);
            SetColor(material, "_EmissionColor", glowColor * 1.6f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            Material addonMaterial = null;
            if (addonMesh)
            {
                addonMaterial = new Material(template) { name = look.Name + "_addon" };
                var plain = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var pixels = new Color32[16];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = look.AddonColor;
                }
                plain.SetPixels32(pixels);
                plain.Apply();
                SetTexture(addonMaterial, "_MainTex", plain);
                SetTexture(addonMaterial, "_BaseMap", plain);
                SetColor(addonMaterial, "_Color", Color.white);
                SetColor(addonMaterial, "_EmissionColor", Color.black);
                addonMaterial.DisableKeyword("_EMISSION");
            }

            RemoveVisuals(prefab);
            AddPart(prefab, look.Name + "Model", mesh, material);
            if (addonMesh)
            {
                AddPart(prefab, look.Name + "Addon", addonMesh, addonMaterial);
            }
            return true;
        }

        /// <summary>
        ///     A small light on the dropped item, so it lights the ground around it and is easy to spot. Only items
        ///     lying in the world have it (items in inventories have no object). Add after rendering the icon.
        /// </summary>
        public static void AddGlow(GameObject prefab, Color color)
        {
            float intensity = CorruptionConfig.ShardGlowIntensity.Value;
            if (intensity <= 0f)
            {
                return;
            }
            var glow = new GameObject("CorruptionGlow");
            glow.transform.SetParent(prefab.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = CorruptionConfig.ShardGlowRange.Value;
            light.shadows = LightShadows.None;
        }

        private static void AddPart(GameObject prefab, string name, Mesh mesh, Material material)
        {
            var part = new GameObject(name);
            part.transform.SetParent(prefab.transform, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private static Material FindTemplateMaterial(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.sharedMaterial)
                {
                    return renderer.sharedMaterial;
                }
            }
            return null;
        }

        /// <summary>Removes the base item's meshes, particles and LOD setup; keeps colliders, rigidbody and item logic.</summary>
        private static void RemoveVisuals(GameObject prefab)
        {
            foreach (var lod in prefab.GetComponentsInChildren<LODGroup>(true))
            {
                UnityEngine.Object.DestroyImmediate(lod);
            }
            foreach (var particles in prefab.GetComponentsInChildren<ParticleSystem>(true))
            {
                // The system first: Unity won't remove a renderer its particle system still needs.
                var particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
                UnityEngine.Object.DestroyImmediate(particles);
                if (particleRenderer)
                {
                    UnityEngine.Object.DestroyImmediate(particleRenderer);
                }
            }
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                var filter = renderer.GetComponent<MeshFilter>();
                UnityEngine.Object.DestroyImmediate(renderer);
                if (filter)
                {
                    UnityEngine.Object.DestroyImmediate(filter);
                }
            }
            foreach (var light in prefab.GetComponentsInChildren<Light>(true))
            {
                UnityEngine.Object.DestroyImmediate(light);
            }
        }

        private static Mesh LoadMesh(string file)
        {
            string text = ReadResourceText(file);
            if (text == null)
            {
                return null;
            }
            var obj = ObjMesh.Parse(text);
            obj.AddBackFaces();

            int count = obj.VertexCount;
            var vertices = new Vector3[count];
            var normals = new Vector3[count];
            var uvs = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(obj.Positions[i * 3], obj.Positions[i * 3 + 1], obj.Positions[i * 3 + 2]);
                normals[i] = new Vector3(obj.Normals[i * 3], obj.Normals[i * 3 + 1], obj.Normals[i * 3 + 2]);
                uvs[i] = new Vector2(obj.Uvs[i * 2], obj.Uvs[i * 2 + 1]);
            }
            var mesh = new Mesh { name = file };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = obj.Triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        ///     Raw RGBA32 pixels written by tools/make_shard.py and make_essences.py (8-byte width/height header, rows bottom to top), so no
        ///     image decoder is needed.
        /// </summary>
        private static Texture2D LoadTexture(string file)
        {
            byte[] bytes = ReadResource(file);
            if (bytes == null || bytes.Length < 8)
            {
                return null;
            }
            int width = BitConverter.ToInt32(bytes, 0);
            int height = BitConverter.ToInt32(bytes, 4);
            if (width <= 0 || height <= 0 || bytes.Length != 8 + width * height * 4)
            {
                Jotunn.Logger.LogWarning($"Embedded texture '{file}' has the wrong size.");
                return null;
            }
            // SetPixels32 fills the top level and Apply builds the smaller mip levels. (LoadRawTextureData would need
            // every mip level's data and throws without it, which broke the shard's registration on 2026-10-28.)
            var pixels = new Color32[width * height];
            for (int i = 0, b = 8; i < pixels.Length; i++, b += 4)
            {
                pixels[i] = new Color32(bytes[b], bytes[b + 1], bytes[b + 2], bytes[b + 3]);
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true) { name = "Corruption_" + file };
            texture.SetPixels32(pixels);
            texture.Apply(true, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Trilinear;
            return texture;
        }

        private static string ReadResourceText(string file)
        {
            byte[] bytes = ReadResource(file);
            return bytes == null ? null : System.Text.Encoding.UTF8.GetString(bytes);
        }

        private static byte[] ReadResource(string file)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + file))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogWarning($"Embedded asset '{file}' not found.");
                    return null;
                }
                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
            {
                material.SetTexture(property, texture);
            }
        }

        private static void SetColor(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, color);
            }
        }
    }
}
