using System.Collections.Generic;
using System.Linq;
using CraftingOverhaul.CharacterSheet;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class SheetLayoutTests
    {
        // EquipmentAndQuickSlots 3.1.3's default table: 3 quick slots, 6 equipment slots, plus our 2 talisman slots.
        private static List<SheetSlot> Defaults()
        {
            return new List<SheetSlot>
            {
                new SheetSlot("QuickSlot1", SlotKind.Quick, 0),
                new SheetSlot("QuickSlot2", SlotKind.Quick, 1),
                new SheetSlot("QuickSlot3", SlotKind.Quick, 2),
                new SheetSlot("Helmet", SlotKind.Equipment, 8),
                new SheetSlot("Chest", SlotKind.Equipment, 9),
                new SheetSlot("Legs", SlotKind.Equipment, 10),
                new SheetSlot("Shoulder", SlotKind.Equipment, 11),
                new SheetSlot("Utility", SlotKind.Equipment, 12),
                new SheetSlot("Trinket", SlotKind.Equipment, 13),
                new SheetSlot("CorruptionTalisman1", SlotKind.Custom, 6),
                new SheetSlot("CorruptionTalisman2", SlotKind.Custom, 7)
            };
        }

        private static SlotPlacement At(SheetPlan plan, string id)
        {
            return plan.Slots.Single(p => p.Id == id);
        }

        private static string ItemsCsv()
        {
            string dir = System.AppContext.BaseDirectory;
            while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "data", "game-dump-1.0.20", "items.csv")))
            {
                dir = System.IO.Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return System.IO.Path.Combine(dir, "data", "game-dump-1.0.20", "items.csv");
        }

        private static float Bottom(SheetRect r)
        {
            return r.Y + r.Height;
        }

        [Fact]
        public void ArmourGoesDownTheLeftColumnInBodyOrder()
        {
            var plan = SheetLayout.Plan(Defaults());
            string[] order = { "Helmet", "Chest", "Legs", "Shoulder", "Utility" };
            for (int i = 0; i < order.Length; i++)
            {
                var p = At(plan, order[i]);
                Assert.Equal(SheetLayout.Margin, p.Tile.X);
                Assert.Equal(SheetLayout.ColumnsTop + i * SheetLayout.TilePitch, p.Tile.Y);
                Assert.True(p.Labelled);
            }
        }

        [Fact]
        public void CellsSitCentredAtTheTopOfTheirTile()
        {
            var p = At(SheetLayout.Plan(Defaults()), "Chest");
            Assert.Equal(p.Tile.X + (SheetLayout.TileWidth - SheetLayout.Cell) / 2f, p.CellX);
            Assert.Equal(p.Tile.Y + SheetLayout.CellTop, p.CellY);
            // Room for the label under the cell, inside the tile.
            Assert.True(p.CellY + SheetLayout.Cell + 16f <= Bottom(p.Tile));
        }

        [Fact]
        public void TrinketThenCustomSlotsGoDownTheRightColumn()
        {
            var plan = SheetLayout.Plan(Defaults());
            var trinket = At(plan, "Trinket");
            var t1 = At(plan, "CorruptionTalisman1");
            var t2 = At(plan, "CorruptionTalisman2");
            Assert.Equal(SheetLayout.Width - SheetLayout.Margin - SheetLayout.TileWidth, trinket.Tile.X);
            Assert.Equal(trinket.Tile.X, t1.Tile.X);
            Assert.Equal(trinket.Tile.X, t2.Tile.X);
            Assert.Equal(At(plan, "Helmet").Tile.Y, trinket.Tile.Y);
            Assert.Equal(trinket.Tile.Y + SheetLayout.TilePitch, t1.Tile.Y);
            Assert.Equal(trinket.Tile.Y + 2 * SheetLayout.TilePitch, t2.Tile.Y);
        }

        [Fact]
        public void CustomSlotsKeepTheirAddedOrderAcrossReservedIndices()
        {
            // EAQS hands out reserved indices 6, 7, 16, 17... in the order slots are added.
            var slots = Defaults();
            slots.Add(new SheetSlot("Ring", SlotKind.Custom, 16));
            slots.Add(new SheetSlot("CorruptionTalisman3", SlotKind.Custom, 17));
            var plan = SheetLayout.Plan(slots);
            Assert.True(At(plan, "Ring").Tile.Y > At(plan, "CorruptionTalisman2").Tile.Y);
            Assert.True(At(plan, "CorruptionTalisman3").Tile.Y > At(plan, "Ring").Tile.Y);
        }

        [Fact]
        public void ExtraUtilitySlotsFollowTheFirstUtilitySlot()
        {
            var slots = Defaults();
            slots.Add(new SheetSlot("Utility2", SlotKind.Equipment, 14));
            slots.Add(new SheetSlot("Utility3", SlotKind.Equipment, 15));
            var plan = SheetLayout.Plan(slots);
            var u = At(plan, "Utility");
            Assert.Equal(u.Tile.X, At(plan, "Utility2").Tile.X);
            Assert.Equal(u.Tile.Y + SheetLayout.TilePitch, At(plan, "Utility2").Tile.Y);
            Assert.Equal(u.Tile.Y + 2 * SheetLayout.TilePitch, At(plan, "Utility3").Tile.Y);
        }

        [Fact]
        public void QuickSlotsFormACentredUnlabelledRowAtTheBottom()
        {
            var plan = SheetLayout.Plan(Defaults());
            var q = new[] { At(plan, "QuickSlot1"), At(plan, "QuickSlot2"), At(plan, "QuickSlot3") };
            Assert.All(q, p => Assert.False(p.Labelled));
            Assert.Equal(q[0].Tile.Y, q[1].Tile.Y);
            Assert.Equal(q[0].Tile.X + SheetLayout.QuickTile + SheetLayout.QuickGap, q[1].Tile.X);
            Assert.True(q[0].Tile.Y > Bottom(plan.Stats));
            float rowWidth = q[2].Tile.X + SheetLayout.QuickTile - q[0].Tile.X;
            Assert.Equal((plan.Width - rowWidth) / 2f, q[0].Tile.X, 3);
            Assert.Equal(q[0].Tile.X + (SheetLayout.QuickTile - SheetLayout.Cell) / 2f, q[0].CellX);
            Assert.Equal(Bottom(q[0].Tile) + SheetLayout.Margin, plan.Height);
        }

        [Fact]
        public void NoQuickSlotsMeansNoBottomRow()
        {
            var plan = SheetLayout.Plan(Defaults().Where(s => s.Kind != SlotKind.Quick).ToList());
            Assert.Equal(Bottom(plan.Stats) + SheetLayout.Margin, plan.Height);
            Assert.Equal(8, plan.Slots.Length);
        }

        [Fact]
        public void ColumnsAreAtLeastFiveRowsAndGrowWithTheLongerColumn()
        {
            float Rows(int n) => n * SheetLayout.TilePitch - (SheetLayout.TilePitch - SheetLayout.TileHeight);
            var few = SheetLayout.Plan(new List<SheetSlot> { new SheetSlot("Helmet", SlotKind.Equipment, 8) });
            Assert.Equal(Rows(SheetLayout.MinRows), few.Picture.Height);

            var slots = Defaults();
            for (int i = 0; i < 5; i++)
            {
                slots.Add(new SheetSlot("Extra" + i, SlotKind.Custom, 16 + i));
            }
            var many = SheetLayout.Plan(slots);
            // Right column: trinket + 2 talismans + 5 extras = 8 rows.
            Assert.Equal(Rows(8), many.Picture.Height);
            Assert.True(many.Ribbon.Y > Bottom(At(many, "Extra4").Tile));
        }

        [Fact]
        public void PictureSitsBetweenTheColumnsAndTheStatsSpanThePanel()
        {
            var plan = SheetLayout.Plan(Defaults());
            Assert.Equal(At(plan, "Helmet").Tile.X + SheetLayout.TileWidth + SheetLayout.Gap, plan.Picture.X);
            Assert.Equal(plan.Picture.X + plan.Picture.Width + SheetLayout.Gap, At(plan, "Trinket").Tile.X);
            Assert.Equal(SheetLayout.Margin, plan.Stats.X);
            Assert.Equal(plan.Width - 2 * SheetLayout.Margin, plan.Stats.Width);
            Assert.Equal(At(plan, "Trinket").Tile.X + SheetLayout.TileWidth + SheetLayout.Margin, plan.Width);
        }

        [Fact]
        public void RibbonStraddlesTheTopOfTheStatsBoxAndTheCloseButtonSitsInTheTitle()
        {
            var plan = SheetLayout.Plan(Defaults());
            Assert.Equal(plan.Stats.Y - SheetLayout.RibbonRise, plan.Ribbon.Y);
            Assert.Equal(plan.Width / 2f, plan.Ribbon.X + plan.Ribbon.Width / 2f, 3);
            Assert.True(plan.Ribbon.Y > Bottom(plan.Picture));
            Assert.True(plan.Close.X + plan.Close.Width <= plan.Title.X + plan.Title.Width);
            Assert.True(plan.Close.Y >= plan.Title.Y && Bottom(plan.Close) <= Bottom(plan.Title));
            Assert.True(Bottom(plan.Title) <= SheetLayout.ColumnsTop);
        }

        [Theory]
        [InlineData("Helmet", "Head", "HEAD")]
        [InlineData("Shoulder", "Shoulders", "CAPE")]
        [InlineData("Chest", "Chest", "CHEST")]
        [InlineData("Utility2", "Utility", "UTILITY")]
        [InlineData("Trinket", "Trinket", "TRINKET")]
        [InlineData("CorruptionTalisman2", "$corruption_slot_talisman", "TALISMAN")]
        [InlineData("SomeModRing", "Ring", "RING")]
        public void LabelsFollowTheConceptAndKeepOtherModsNames(string id, string fallback, string expected)
        {
            Assert.Equal(expected, SheetLayout.LabelFor(id, fallback));
        }

        [Fact]
        public void SilhouettesExistInTheGameDumpForEveryKnownSlot()
        {
            var prefabs = new HashSet<string>(System.IO.File.ReadAllLines(ItemsCsv()).Skip(1).Select(l => l.Split(',')[0]));
            foreach (string id in new[] { "Helmet", "Chest", "Legs", "Shoulder", "Utility", "Utility3", "Trinket", "CorruptionTalisman1" })
            {
                string prefab = SheetLayout.SilhouetteFor(id);
                Assert.NotNull(prefab);
                Assert.Contains(prefab, prefabs);
            }
            Assert.Null(SheetLayout.SilhouetteFor("SomeModRing"));
        }

        [Fact]
        public void IndexHelpersMatchTheEquipmentTable()
        {
            Assert.True(SheetLayout.IsEquipmentIndex(8));
            Assert.True(SheetLayout.IsEquipmentIndex(15));
            Assert.False(SheetLayout.IsEquipmentIndex(7));
            Assert.False(SheetLayout.IsEquipmentIndex(16));
            Assert.True(SheetLayout.IsExtraUtilityIndex(14));
            Assert.False(SheetLayout.IsExtraUtilityIndex(12));
        }
    }

    public class FlatJsonTests
    {
        private const string Info = "{\"id\":\"CorruptionTalisman1\",\"index\":6,\"nameToken\":\"$corruption_slot_talisman\",\"active\":true," +
                                    "\"gridX\":6,\"gridY\":4,\"isQuickSlot\":false,\"isEquipmentSlot\":false,\"isCustomSlot\":true," +
                                    "\"ownerPluginGuid\":\"com.krogongames.corruption\",\"occupied\":false}";

        [Fact]
        public void ReadsStringsNumbersAndBooleans()
        {
            Assert.Equal("CorruptionTalisman1", FlatJson.Get(Info, "id"));
            Assert.Equal(6, FlatJson.GetInt(Info, "index", -1));
            Assert.Equal(4, FlatJson.GetInt(Info, "gridY", -1));
            Assert.True(FlatJson.GetBool(Info, "active", false));
            Assert.False(FlatJson.GetBool(Info, "isQuickSlot", true));
            Assert.True(FlatJson.GetBool(Info, "isCustomSlot", false));
            Assert.Equal("com.krogongames.corruption", FlatJson.Get(Info, "ownerPluginGuid"));
        }

        [Fact]
        public void MissingKeysGiveFallbacks()
        {
            Assert.Null(FlatJson.Get(Info, "missing"));
            Assert.Equal(-1, FlatJson.GetInt(Info, "missing", -1));
            Assert.True(FlatJson.GetBool(Info, "missing", true));
            Assert.Null(FlatJson.Get(null, "id"));
        }

        [Fact]
        public void KeyIsNotConfusedWithAValueOfTheSameText()
        {
            string json = "{\"name\":\"index\",\"index\":3}";
            Assert.Equal(3, FlatJson.GetInt(json, "index", -1));
            Assert.Equal("index", FlatJson.Get(json, "name"));
        }

        [Fact]
        public void UnescapesStrings()
        {
            string json = "{\"a\":\"say \\\"hi\\\" \\\\ \\u0041\"}";
            Assert.Equal("say \"hi\" \\ A", FlatJson.Get(json, "a"));
        }

        [Fact]
        public void ReadsStringArrays()
        {
            Assert.Equal(new[] { "QuickSlot1", "Helmet", "CorruptionTalisman1" }, FlatJson.Strings("[\"QuickSlot1\",\"Helmet\",\"CorruptionTalisman1\"]"));
            Assert.Empty(FlatJson.Strings("[]"));
            Assert.Empty(FlatJson.Strings(null));
            Assert.Empty(FlatJson.Strings("{\"a\":1}"));
        }
    }
}
