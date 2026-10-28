using System;
using System.IO;
using System.Reflection;
using Corruption.Rules;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The shard's own look: a crystal model and textures made by tools/make_shard.py and embedded in the DLL.
    ///     Replaces every visual of the item the shard was cloned from (which only provides the item, physics and
    ///     networking parts); the base item's colliders stay so it still lies on the ground the same way.
    /// </summary>
    internal static class ShardModel
    {
        private const string ResourcePrefix = "Corruption.Assets.";

        /// <summary>Swaps the prefab's visuals for the custom shard. Returns false (and changes nothing) if anything is missing.</summary>
        public static bool Apply(GameObject prefab, Color glowColor)
        {
            var template = FindTemplateMaterial(prefab);
            var mesh = LoadMesh();
            var albedo = LoadTexture("shard_albedo.rgba");
            var emission = LoadTexture("shard_emission.rgba");
            if (!template || !mesh || !albedo || !emission)
            {
                Jotunn.Logger.LogWarning("Custom shard model unavailable (missing asset or material); keeping the base item's look.");
                return false;
            }

            // The game's own shader (from the base item's material), with our textures, so it lights like other items.
            var material = new Material(template) { name = "CorruptionShard" };
            SetTexture(material, "_MainTex", albedo);
            SetTexture(material, "_BaseMap", albedo);
            SetColor(material, "_Color", Color.white);
            SetTexture(material, "_EmissionMap", emission);
            SetColor(material, "_EmissionColor", glowColor * 1.6f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            RemoveVisuals(prefab);

            var model = new GameObject("CorruptionShardModel");
            model.transform.SetParent(prefab.transform, false);
            model.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = model.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return true;
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
                UnityEngine.Object.DestroyImmediate(particles.gameObject.GetComponent<ParticleSystemRenderer>());
                UnityEngine.Object.DestroyImmediate(particles);
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

        private static Mesh LoadMesh()
        {
            string text = ReadResourceText("shard.obj");
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
            var mesh = new Mesh { name = "CorruptionShard" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = obj.Triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        ///     Raw RGBA32 pixels written by tools/make_shard.py (8-byte width/height header, rows bottom to top), so no
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
            var pixels = new byte[width * height * 4];
            Buffer.BlockCopy(bytes, 8, pixels, 0, pixels.Length);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true) { name = "CorruptionShard_" + file };
            texture.LoadRawTextureData(pixels);
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
