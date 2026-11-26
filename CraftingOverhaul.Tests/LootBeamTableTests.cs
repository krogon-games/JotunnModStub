using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The loot beam table (loot_beams.csv) and the personal loot settings (personal_loot.csv).</summary>
    public class LootBeamTableTests
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

        private static LootBeamTable ParseReal()
        {
            var errors = new List<string>();
            var table = LootBeamTable.Parse(DataFile("loot_beams.csv"), errors);
            Assert.True(errors.Count == 0, "loot_beams.csv has problems:\n" + string.Join("\n", errors));
            return table;
        }

        private static List<string> RealOrbRows()
        {
            var errors = new List<string>();
            var orbs = OrbDropTable.Parse(DataFile("orbs.csv"), errors);
            Assert.Empty(errors);
            return orbs.OneIn.Keys.Select(LootBeamTable.ForOrb).ToList();
        }

        private static string Colour(LootBeamTable table, string drop) => table.Find(drop).Colour.ToUpperInvariant();

        [Fact]
        public void TheRealTableHasExactlyTheDropsThatExist()
        {
            // Rows: magic, rare, unique, shard, map, and one per orb that drops (the Jeweller's Orb has a blank one_in).
            var table = ParseReal();
            var expected = new List<string> { "magic", "rare", "unique", "shard", "map" };
            expected.AddRange(RealOrbRows());
            Assert.Equal(expected.OrderBy(x => x), table.Rows.Select(r => r.Drop).OrderBy(x => x));
            Assert.Equal(9, RealOrbRows().Count);
            Assert.Null(table.Find("orb_jeweller"));
            Assert.All(table.Rows, r => Assert.Equal("approved", r.Status));
        }

        [Fact]
        public void EveryOrbThatDropsHasARowAndNoOtherOrbRowsExist()
        {
            var table = ParseReal();
            var orbRows = table.Rows.Select(r => r.Drop).Where(d => d.StartsWith("orb_")).OrderBy(x => x).ToList();
            Assert.Equal(RealOrbRows().OrderBy(x => x), orbRows);
        }

        [Fact]
        public void TheTallAndShortSetsAreWhatKrogonDecided()
        {
            // Decision: short and faint for magic, shard, transmutation, augmentation, alchemy, regal and annulment; tall and bright for rare, unique, map, exalted, divine, chance and mirror.
            var table = ParseReal();
            var tall = new[] { "rare", "unique", "map", "orb_exalted", "orb_divine", "orb_chance", "orb_mirror" };
            var short_ = new[] { "magic", "shard", "orb_transmutation", "orb_augmentation", "orb_alchemy", "orb_regal", "orb_annulment" };
            Assert.Equal(tall.OrderBy(x => x), table.Rows.Where(r => r.Tall).Select(r => r.Drop).OrderBy(x => x));
            Assert.Equal(short_.OrderBy(x => x), table.Rows.Where(r => !r.Tall).Select(r => r.Drop).OrderBy(x => x));
        }

        [Fact]
        public void OnlyTallBeamsHaveASound()
        {
            // Decision: a sound plays when the drop lands, for tall-beam drops only; until the user picks them by ear, every tall row is the placeholder 'default'.
            var table = ParseReal();
            Assert.All(table.Rows.Where(r => r.Tall), r => Assert.Equal(LootBeamTable.DefaultSound, r.Sound));
            Assert.All(table.Rows.Where(r => !r.Tall), r => Assert.False(r.HasSound, r.Drop));
        }

        [Fact]
        public void GearColoursAreTheirNameColours()
        {
            // Decision: Magic gear blue (its name colour #8888FF), Rare gear yellow (#FFFF77, its name colour).
            var table = ParseReal();
            Assert.Equal("#8888FF", Colour(table, "magic"));
            Assert.Equal("#FFFF77", Colour(table, "rare"));
        }

        [Fact]
        public void EveryOrbIsWhite()
        {
            // Decision: every orb white (#FFFFFF).
            var table = ParseReal();
            Assert.All(table.Rows.Where(r => r.Drop.StartsWith("orb_")), r => Assert.Equal("#FFFFFF", r.Colour.ToUpperInvariant()));
        }

        [Fact]
        public void ShardMapAndUniqueColours()
        {
            // Decisions: shards red (the shard's own default #B3170D); maps green #18FF00 (Krogon, 2026-11-26); unique orange-gold #FFAC00.
            var table = ParseReal();
            Assert.Equal("#B3170D", Colour(table, LootBeamTable.Shard));
            Assert.Equal("#18FF00", Colour(table, LootBeamTable.Map));
            Assert.Equal("#FFAC00", Colour(table, "unique"));
        }

        [Fact]
        public void EveryRowNamesItsSource()
        {
            Assert.All(ParseReal().Rows, r => Assert.False(string.IsNullOrWhiteSpace(r.Notes), r.Drop));
        }

        [Fact]
        public void ColoursAreParsedToFloats()
        {
            var row = ParseReal().Find("MAP");
            Assert.NotNull(row);
            Assert.Equal(0x18 / 255f, row.R, 5);
            Assert.Equal(1f, row.G, 5);
            Assert.Equal(0f, row.B, 5);
        }

        [Fact]
        public void NameHelpers()
        {
            Assert.Equal("magic", LootBeamTable.ForGear("magic"));
            Assert.Equal("rare", LootBeamTable.ForGear("rare"));
            Assert.Equal("unique", LootBeamTable.ForGear("unique"));
            Assert.Null(LootBeamTable.ForGear("normal"));
            Assert.Null(LootBeamTable.ForGear(null));
            Assert.Equal("orb_regal", LootBeamTable.ForOrb("Regal"));
            Assert.Null(LootBeamTable.ForOrb(null));
            Assert.Null(LootBeamTable.ForOrb(""));
        }

        private static List<string> Errors(string csv)
        {
            var errors = new List<string>();
            LootBeamTable.Parse(csv, errors);
            return errors;
        }

        private const string Header = "drop,status,colour,size,sound,notes\n";

        [Fact]
        public void BadInputsGiveErrors()
        {
            Assert.Contains(Errors(Header + "magic,approved,#8888FF,huge,,x\n"), e => e.Contains("size"));
            Assert.Contains(Errors(Header + "magic,approved,8888FF,short,,x\n"), e => e.Contains("colour"));
            Assert.Contains(Errors(Header + "magic,approved,#88FF,short,,x\n"), e => e.Contains("colour"));
            Assert.Contains(Errors(Header + "magic,approved,#GGGGGG,short,,x\n"), e => e.Contains("colour"));
            Assert.Contains(Errors(Header + "magic,approved,#8888FF,short,,x\nmagic,approved,#8888FF,short,,x\n"), e => e.Contains("twice"));
            Assert.Contains(Errors(Header + ",approved,#8888FF,short,,x\n"), e => e.Contains("empty"));
            Assert.Contains(Errors("drop,status,colour\nmagic,approved,#8888FF\n"), e => e.Contains("missing column"));
            Assert.Contains(Errors(""), e => e.Contains("empty"));
            Assert.Empty(Errors(Header + "magic,approved,#8888ff,short,,x\n"));
        }

        [Fact]
        public void FindIsCaseInsensitiveAndNullSafe()
        {
            var table = LootBeamTable.Parse(Header + "magic,approved,#8888FF,short,,x\n", new List<string>());
            Assert.NotNull(table.Find("MAGIC"));
            Assert.Null(table.Find("rare"));
            Assert.Null(table.Find(null));
        }

        // --- personal_loot.csv ---

        private static PersonalLootSettings ParseSettingsReal()
        {
            var errors = new List<string>();
            var settings = PersonalLootSettings.Parse(DataFile("personal_loot.csv"), errors);
            Assert.True(errors.Count == 0, "personal_loot.csv has problems:\n" + string.Join("\n", errors));
            return settings;
        }

        [Fact]
        public void TheRealSettingsParseAndTheLockIsSixtySeconds()
        {
            // Design (corruption-design.md → Loot and uniques → Drops): "only the roller can pick a drop up for the first 60 seconds, then anyone can".
            var s = ParseSettingsReal();
            Assert.Equal(60.0, s.LockSeconds);
            Assert.True(s.SoundWindowSeconds >= 0);
            Assert.True(s.RefreshSeconds > 0);
            Assert.True(s.TallHeight > s.ShortHeight);
            Assert.Equal(0f, s.ShortLightRange);
            Assert.Equal(0f, s.ShortLightIntensity);
        }

        private const string AllSettings =
            "setting,value,notes\nlock_seconds,60,\nsound_window_seconds,5,\nrefresh_seconds,0.25,\ntall_height,10,\ntall_width,0.35,\n" +
            "tall_light_range,4,\ntall_light_intensity,1.5,\nshort_height,2.5,\nshort_width,0.15,\nshort_light_range,0,\nshort_light_intensity,0,\n";

        private static List<string> SettingErrors(string csv)
        {
            var errors = new List<string>();
            PersonalLootSettings.Parse(csv, errors);
            return errors;
        }

        [Fact]
        public void SettingsParseWithInvariantCulture()
        {
            var errors = new List<string>();
            var s = PersonalLootSettings.Parse(AllSettings, errors);
            Assert.Empty(errors);
            Assert.Equal(0.25, s.RefreshSeconds);
            Assert.Equal(0.35f, s.TallWidth);
        }

        [Fact]
        public void BadSettingsGiveErrors()
        {
            Assert.Contains(SettingErrors(AllSettings + "colour_boost,3,\n"), e => e.Contains("unknown"));
            Assert.Contains(SettingErrors(AllSettings.Replace("tall_width,0.35", "tall_width,-1")), e => e.Contains("negative"));
            Assert.Contains(SettingErrors(AllSettings.Replace("lock_seconds,60", "lock_seconds,0")), e => e.Contains("more than 0"));
            Assert.Contains(SettingErrors(AllSettings.Replace("refresh_seconds,0.25", "refresh_seconds,0")), e => e.Contains("more than 0"));
            Assert.Contains(SettingErrors(AllSettings.Replace("short_width,0.15,\n", "")), e => e.Contains("missing setting 'short_width'"));
            Assert.Contains(SettingErrors(AllSettings.Replace("tall_height,10", "tall_height,tall")), e => e.Contains("number"));
            Assert.Contains(SettingErrors("setting,value\n"), e => e.Contains("missing setting"));
            Assert.Contains(SettingErrors(""), e => e.Contains("empty"));
        }

        [Fact]
        public void EverySettingNamesItsSource()
        {
            foreach (var row in Csv.ReadRows(DataFile("personal_loot.csv")).Skip(1))
            {
                Assert.False(string.IsNullOrWhiteSpace(row[2]), row[0]);
            }
        }

        [Fact]
        public void OrbOfReversesPrefabForEveryOrb()
        {
            foreach (var orb in OrbRules.Orbs)
            {
                Assert.Equal(orb, OrbRules.OrbOf(OrbRules.Prefab(orb)));
            }
            Assert.Null(OrbRules.OrbOf("Sword"));
            Assert.Null(OrbRules.OrbOf(null));
        }
    }
}
