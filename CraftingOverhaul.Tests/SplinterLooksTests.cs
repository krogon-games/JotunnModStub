using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The Affix Splinters' looks (Krogon, 2026-11-10): a shape per kind, a colour per stat, one row per family.</summary>
    public class SplinterLooksTests
    {
        private static string Root()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", "splinter_looks.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static string DataFile(string name) => File.ReadAllText(Path.Combine(Root(), "Corruption", "Data", name));

        private static List<AffixTable.Affix> Affixes()
        {
            var errors = new List<string>();
            var affixes = AffixTable.Parse(DataFile("affixes.csv"), errors);
            Assert.Empty(errors);
            return affixes;
        }

        [Fact]
        public void EveryAffixHasALookAndEveryLookAnAffix()
        {
            var errors = new List<string>();
            var looks = SplinterLooks.Parse(DataFile("splinter_looks.csv"), errors, Affixes());
            Assert.True(errors.Count == 0, "splinter_looks.csv has problems:\n" + string.Join("\n", errors));
            Assert.All(SplinterLooks.Kinds, kind => Assert.Contains(looks.Looks, l => l.Kind == kind));
        }

        [Fact]
        public void EveryKindHasItsModelAndEveryLookItsTexture()
        {
            var errors = new List<string>();
            var looks = SplinterLooks.Parse(DataFile("splinter_looks.csv"), errors);
            string assets = Path.Combine(Root(), "Corruption", "Assets", "Splinters");
            foreach (var kind in SplinterLooks.Kinds)
            {
                Assert.True(File.Exists(Path.Combine(assets, $"splinter_{kind}.obj")), $"no model for {kind} (run tools/make_splinters.py)");
                Assert.True(File.Exists(Path.Combine(assets, $"splinter_{kind}_emission.rgba")), $"no glow mask for {kind}");
                var mesh = ObjMesh.Parse(File.ReadAllText(Path.Combine(assets, $"splinter_{kind}.obj")));
                Assert.NotNull(mesh);
                Assert.True(mesh.Triangles.Count >= 3 * 100, $"{kind}: too few triangles");
            }
            foreach (var look in looks.Looks)
            {
                Assert.True(File.Exists(Path.Combine(assets, look.Prefab + "_albedo.rgba")), $"no texture for {look.Prefab} (run tools/make_splinters.py)");
            }
        }

        [Fact]
        public void ItemNamesAreStableAndGoToTheSplintersTab()
        {
            var errors = new List<string>();
            var looks = SplinterLooks.Parse(DataFile("splinter_looks.csv"), errors);
            var affixes = Affixes();
            var fire = affixes.First(a => a.Family == "fire_damage");
            Assert.Equal("CorruptionAffixSplinter_fire_damage", looks.For(fire).Prefab);
            var toolSwift = affixes.First(a => a.Pool.StartsWith("gathering") && a.Family == "attack_speed");
            Assert.Equal("CorruptionAffixSplinter_tool_attack_speed", looks.For(toolSwift).Prefab);
            Assert.Equal("gathering", looks.For(toolSwift).Kind);
            Assert.All(looks.Looks, l => Assert.Equal(InventoryTab.Splinters, TabRules.TabFor(l.Prefab)));
            Assert.Equal(looks.Looks.Count, looks.Looks.Select(l => l.Prefab).Distinct().Count());
        }

        [Fact]
        public void NamesItsMistakes()
        {
            var errors = new List<string>();
            SplinterLooks.Parse("pool,family,status,kind,colour\ngear,fire_damage,maybe,blob,red\nhell,x-y,approved,attack,#112233\n" +
                                "gear,fire_damage,approved,attack,#112233\ngear,fire_damage,approved,attack,#112233\n", errors);
            Assert.Contains(errors, e => e.Contains("status 'maybe'"));
            Assert.Contains(errors, e => e.Contains("kind 'blob'"));
            Assert.Contains(errors, e => e.Contains("colour 'red'"));
            Assert.Contains(errors, e => e.Contains("pool 'hell'"));
            Assert.Contains(errors, e => e.Contains("letters, digits and _"));
            Assert.Contains(errors, e => e.Contains("listed twice"));

            errors.Clear();
            SplinterLooks.Parse("pool,family,status,kind,colour\n", errors, Affixes());
            Assert.Contains(errors, e => e.Contains("no row for gear fire_damage"));
        }
    }
}
