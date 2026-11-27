using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Writes the map text of four sample maps as JSON for the preview picture (tools/). Does nothing unless the
    ///     environment variable MAP_UI_PREVIEW_OUT names the file to write.
    /// </summary>
    public class MapUiPreviewExport
    {
        private static string DataFile(string name)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Corruption", "Data", name)))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir, "Corruption", "Data", name));
        }

        private static string Q(string s)
        {
            var b = new StringBuilder("\"");
            foreach (char c in s ?? "")
            {
                switch (c)
                {
                    case '"': b.Append("\\\""); break;
                    case '\\': b.Append("\\\\"); break;
                    case '\n': b.Append("\\n"); break;
                    case '\r': b.Append("\\r"); break;
                    case '\t': b.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                        {
                            b.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            b.Append(c);
                        }
                        break;
                }
            }
            return b.Append('"').ToString();
        }

        // The marks as LootTooltip.Locks builds them (colours: RiftforgeCrafting.CorruptedColor / MirroredColor).
        private const string Corrupted = "\n<color=#E04040>Corrupted</color>";
        private const string Mirrored = "\n<color=#C8D8FF>Mirrored: can't be changed</color>";

        [Fact]
        public void WritesThePreviewJson()
        {
            string output = Environment.GetEnvironmentVariable("MAP_UI_PREVIEW_OUT");
            if (string.IsNullOrEmpty(output))
            {
                return;
            }
            var errors = new List<string>();
            var tiers = MapTierTable.Parse(DataFile("map_tiers.csv"), errors);
            var affixes = MapAffixTable.Parse(DataFile("map_affixes.csv"), errors);
            var drops = DropTable.Parse(DataFile("drop_chances.csv"), DataFile("drop_modifiers.csv"), errors);
            Assert.Empty(errors);

            var cases = new List<string>();
            // Fixed values (each affix's lowest) so the picture is stable.
            AddCase(cases, tiers, affixes, drops, "tier1", "Tier 1, no affixes", 1, new string[0], "", "", true, "");
            AddCase(cases, tiers, affixes, drops, "tier3", "Tier 3, brutal and teeming, just added teeming", 3, new[] { "brutal", "teeming" }, "",
                "Orb of Augmentation: now tier 3\n+ " + MapText.AffixLine(affixes.Find("teeming"), affixes.Find("teeming").Min), true, "teeming");
            AddCase(cases, tiers, affixes, drops, "tier5", "Tier 5, four affixes", 5, new[] { "glorious", "ironskin", "withering", "searing" }, "", "", true, "");
            AddCase(cases, tiers, affixes, drops, "tier6", "Tier 6, corrupted and mirrored", 6, new[] { "glorious", "ironskin", "withering", "searing" },
                Corrupted + Mirrored, "Corrupted: now tier 6", true, "");
            File.WriteAllText(output, "{\"cases\":[" + string.Join(",", cases) + "]}", new UTF8Encoding(false));
        }

        private static void AddCase(List<string> cases, MapTierTable tiers, MapAffixTable affixes, DropTable drops, string id, string label, int tier,
            string[] ids, string marks, string result, bool good, string highlighted)
        {
            var row = tiers.Row(tier);
            double multiplier = drops.Multiplier("magic", new[] { row.DropSource });
            string color = MapText.ColorOf(row);
            string name = MapText.Name("Meadows map", row);
            var glyphs = MapGlyphs.Fancy;
            var rolls = ids.Select(i => new MapAffixRoll { Affix = affixes.Find(i), Value = affixes.Find(i).Min }).ToList();
            string tooltip = MapText.Tooltip(row, "Meadows", rolls, affixes, multiplier, glyphs, marks, "Weight: <color=orange>0.5</color>");
            var highlight = highlighted.Length > 0 ? new[] { highlighted } : null;
            string sheet = MapText.ForgeSheet(row, "Meadows", rolls, affixes, multiplier, glyphs, highlight);
            cases.Add("{\"id\":" + Q(id) + ",\"label\":" + Q(label) + ",\"biome\":\"Meadows\",\"tier\":" + tier + ",\"tierColor\":" + Q(color) +
                      ",\"tooltip\":{\"title\":" + Q(name) + ",\"titleColor\":" + Q(color) + ",\"text\":" + Q(tooltip) + "}" +
                      ",\"forge\":{\"title\":" + Q(name) + ",\"titleColor\":" + Q(color) + ",\"caption\":" + Q(MapText.ForgeCaption(row)) +
                      ",\"benchText\":" + Q(MapText.ForgeBench(marks)) + ",\"sheet\":" + Q(sheet) + ",\"result\":" + Q(result) +
                      ",\"resultGood\":" + (good ? "true" : "false") + "}}");
        }
    }
}
