using System;
using System.Collections.Generic;
using System.Globalization;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads a triangulated Wavefront .obj model (positions, UVs, normals, triangle faces) into flat arrays ready
    ///     for a Unity mesh. Only what tools/make_shard.py writes is supported. No game dependencies, so it is
    ///     unit-tested.
    /// </summary>
    internal sealed class ObjMesh
    {
        /// <summary>x, y, z per vertex.</summary>
        public readonly List<float> Positions = new List<float>();

        /// <summary>u, v per vertex.</summary>
        public readonly List<float> Uvs = new List<float>();

        /// <summary>x, y, z per vertex.</summary>
        public readonly List<float> Normals = new List<float>();

        /// <summary>Three vertex indices per triangle.</summary>
        public readonly List<int> Triangles = new List<int>();

        public int VertexCount => Positions.Count / 3;

        public static ObjMesh Parse(string text)
        {
            var v = new List<float[]>();
            var vt = new List<float[]>();
            var vn = new List<float[]>();
            var mesh = new ObjMesh();
            var corners = new Dictionary<string, int>();

            string[] lines = text.Split('\n');
            for (int lineNo = 0; lineNo < lines.Length; lineNo++)
            {
                string line = lines[lineNo].Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }
                string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                switch (parts[0])
                {
                    case "v":
                        v.Add(Floats(parts, 3, lineNo));
                        break;
                    case "vt":
                        vt.Add(Floats(parts, 2, lineNo));
                        break;
                    case "vn":
                        vn.Add(Floats(parts, 3, lineNo));
                        break;
                    case "f":
                        if (parts.Length != 4)
                        {
                            throw new FormatException($"Line {lineNo + 1}: only triangles are supported.");
                        }
                        for (int c = 1; c <= 3; c++)
                        {
                            if (!corners.TryGetValue(parts[c], out int index))
                            {
                                index = mesh.VertexCount;
                                corners[parts[c]] = index;
                                AddCorner(mesh, parts[c], v, vt, vn, lineNo);
                            }
                            mesh.Triangles.Add(index);
                        }
                        break;
                }
            }
            return mesh;
        }

        /// <summary>
        ///     Adds a back face for every triangle (same corners, reversed order, flipped normals), so the model shows
        ///     from both sides whatever the renderer's culling and winding conventions.
        /// </summary>
        public void AddBackFaces()
        {
            int vertexCount = VertexCount;
            int triangleCount = Triangles.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                Positions.Add(Positions[i * 3]);
                Positions.Add(Positions[i * 3 + 1]);
                Positions.Add(Positions[i * 3 + 2]);
                Uvs.Add(Uvs[i * 2]);
                Uvs.Add(Uvs[i * 2 + 1]);
                Normals.Add(-Normals[i * 3]);
                Normals.Add(-Normals[i * 3 + 1]);
                Normals.Add(-Normals[i * 3 + 2]);
            }
            for (int t = 0; t < triangleCount; t += 3)
            {
                Triangles.Add(Triangles[t] + vertexCount);
                Triangles.Add(Triangles[t + 2] + vertexCount);
                Triangles.Add(Triangles[t + 1] + vertexCount);
            }
        }

        private static void AddCorner(ObjMesh mesh, string corner, List<float[]> v, List<float[]> vt, List<float[]> vn, int lineNo)
        {
            string[] ids = corner.Split('/');
            float[] p = Lookup(v, ids, 0, lineNo, "position");
            float[] uv = ids.Length > 1 && ids[1].Length > 0 ? Lookup(vt, ids, 1, lineNo, "UV") : new[] { 0f, 0f };
            float[] n = ids.Length > 2 && ids[2].Length > 0 ? Lookup(vn, ids, 2, lineNo, "normal") : new[] { 0f, 1f, 0f };
            mesh.Positions.AddRange(p);
            mesh.Uvs.AddRange(uv);
            mesh.Normals.AddRange(n);
        }

        private static float[] Lookup(List<float[]> list, string[] ids, int slot, int lineNo, string what)
        {
            if (!int.TryParse(ids[slot], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ||
                id < 1 || id > list.Count)
            {
                throw new FormatException($"Line {lineNo + 1}: {what} index '{ids[slot]}' is out of range.");
            }
            return list[id - 1];
        }

        private static float[] Floats(string[] parts, int count, int lineNo)
        {
            if (parts.Length < count + 1)
            {
                throw new FormatException($"Line {lineNo + 1}: expected {count} numbers.");
            }
            var values = new float[count];
            for (int i = 0; i < count; i++)
            {
                if (!float.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                {
                    throw new FormatException($"Line {lineNo + 1}: '{parts[i + 1]}' is not a number.");
                }
            }
            return values;
        }
    }
}
