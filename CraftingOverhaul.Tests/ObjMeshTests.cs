using System;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class ObjMeshTests
    {
        private static string ShardObj()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Assets", "shard.obj")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir, "Corruption", "Assets", "shard.obj"));
        }

        [Fact]
        public void TheShardModelLoads()
        {
            var mesh = ObjMesh.Parse(ShardObj());
            Assert.Equal(64 * 3, mesh.Triangles.Count);
            Assert.All(mesh.Triangles, i => Assert.InRange(i, 0, mesh.VertexCount - 1));
            Assert.Equal(mesh.VertexCount * 2, mesh.Uvs.Count);
            Assert.Equal(mesh.VertexCount * 3, mesh.Normals.Count);

            // About 34 cm tall, standing on the ground (y from about 0), so it fits a dropped item.
            var ys = Enumerable.Range(0, mesh.VertexCount).Select(i => mesh.Positions[i * 3 + 1]).ToList();
            Assert.InRange(ys.Max() - ys.Min(), 0.3f, 0.45f);
            Assert.InRange(ys.Min(), -0.1f, 0.05f);

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                float x = mesh.Normals[i * 3], y = mesh.Normals[i * 3 + 1], z = mesh.Normals[i * 3 + 2];
                Assert.InRange(Math.Sqrt(x * x + y * y + z * z), 0.99, 1.01);
            }
        }

        [Fact]
        public void BackFacesDoubleTheMeshWithReversedTriangles()
        {
            var mesh = ObjMesh.Parse("v 0 0 0\nv 1 0 0\nv 0 1 0\nvt 0 0\nvt 1 0\nvt 0 1\nvn 0 0 1\nf 1/1/1 2/2/1 3/3/1\n");
            mesh.AddBackFaces();
            Assert.Equal(6, mesh.VertexCount);
            Assert.Equal(new[] { 0, 1, 2, 3, 5, 4 }, mesh.Triangles);
            Assert.Equal(-1f, mesh.Normals[3 * 3 + 2]);
        }

        [Fact]
        public void ReportsBadFiles()
        {
            Assert.Throws<FormatException>(() => ObjMesh.Parse("v 0 0 0\nf 1 2 3\n"));
            Assert.Throws<FormatException>(() => ObjMesh.Parse("v 0 0 x\n"));
            Assert.Throws<FormatException>(() => ObjMesh.Parse("v 0 0 0\nv 1 0 0\nv 0 1 0\nv 1 1 0\nf 1 2 3 4\n"));
        }
    }
}
