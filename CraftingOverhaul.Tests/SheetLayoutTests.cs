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

        [Fact]
        public void ArmourGoesDownTheLeftColumnInBodyOrder()
        {
            var plan = SheetLayout.Plan(Defaults());
            string[] order = { "Helmet", "Chest", "Legs", "Shoulder", "Utility" };
            for (int i = 0; i < order.Length; i++)
            {
                var p = At(plan, order[i]);
                Assert.Equal(SheetLayout.Margin, p.X);
                Assert.Equal(SheetLayout.TitleHeight + i * SheetLayout.Pitch, p.Y);
            }
        }

        [Fact]
        public void TrinketThenCustomSlotsGoDownTheRightColumn()
        {
            var plan = SheetLayout.Plan(Defaults());
            var trinket = At(plan, "Trinket");
            var t1 = At(plan, "CorruptionTalisman1");
            var t2 = At(plan, "CorruptionTalisman2");
            Assert.Equal(trinket.X, t1.X);
            Assert.Equal(trinket.X, t2.X);
            Assert.True(trinket.X > At(plan, "Helmet").X + SheetLayout.Cell + SheetLayout.PictureWidth);
            Assert.Equal(trinket.Y, At(plan, "Helmet").Y);
            Assert.Equal(trinket.Y + SheetLayout.Pitch, t1.Y);
            Assert.Equal(trinket.Y + 2 * SheetLayout.Pitch, t2.Y);
        }

        [Fact]
        public void CustomSlotsKeepTheirAddedOrderAcrossReservedIndices()
        {
            // EAQS hands out reserved indices 6, 7, 16, 17... in the order slots are added.
            var slots = Defaults();
            slots.Add(new SheetSlot("Ring", SlotKind.Custom, 16));
            slots.Add(new SheetSlot("CorruptionTalisman3", SlotKind.Custom, 17));
            var plan = SheetLayout.Plan(slots);
            Assert.True(At(plan, "Ring").Y > At(plan, "CorruptionTalisman2").Y);
            Assert.True(At(plan, "CorruptionTalisman3").Y > At(plan, "Ring").Y);
        }

        [Fact]
        public void ExtraUtilitySlotsFollowTheFirstUtilitySlot()
        {
            var slots = Defaults();
            slots.Add(new SheetSlot("Utility2", SlotKind.Equipment, 14));
            slots.Add(new SheetSlot("Utility3", SlotKind.Equipment, 15));
            var plan = SheetLayout.Plan(slots);
            var u = At(plan, "Utility");
            Assert.Equal(u.X, At(plan, "Utility2").X);
            Assert.Equal(u.Y + SheetLayout.Pitch, At(plan, "Utility2").Y);
            Assert.Equal(u.Y + 2 * SheetLayout.Pitch, At(plan, "Utility3").Y);
        }

        [Fact]
        public void QuickSlotsFormACentredRowAtTheBottom()
        {
            var plan = SheetLayout.Plan(Defaults());
            var q = new[] { At(plan, "QuickSlot1"), At(plan, "QuickSlot2"), At(plan, "QuickSlot3") };
            Assert.Equal(q[0].Y, q[1].Y);
            Assert.Equal(q[0].X + SheetLayout.Cell + SheetLayout.QuickGap, q[1].X);
            Assert.True(q[0].Y > plan.Stats.Y + plan.Stats.Height);
            float rowWidth = q[2].X + SheetLayout.Cell - q[0].X;
            Assert.Equal((plan.Width - rowWidth) / 2f, q[0].X, 3);
            Assert.Equal(q[0].Y + SheetLayout.Cell + SheetLayout.Margin, plan.Height);
        }

        [Fact]
        public void NoQuickSlotsMeansNoBottomRow()
        {
            var plan = SheetLayout.Plan(Defaults().Where(s => s.Kind != SlotKind.Quick).ToList());
            Assert.Equal(plan.Stats.Y + plan.Stats.Height + SheetLayout.Margin, plan.Height);
            Assert.Equal(8, plan.Slots.Length);
        }

        [Fact]
        public void ColumnsAreAtLeastFiveRowsAndGrowWithTheLongerColumn()
        {
            var few = SheetLayout.Plan(new List<SheetSlot> { new SheetSlot("Helmet", SlotKind.Equipment, 8) });
            Assert.Equal(SheetLayout.MinRows * SheetLayout.Pitch - (SheetLayout.Pitch - SheetLayout.Cell), few.Picture.Height);

            var slots = Defaults();
            for (int i = 0; i < 5; i++)
            {
                slots.Add(new SheetSlot("Extra" + i, SlotKind.Custom, 16 + i));
            }
            var many = SheetLayout.Plan(slots);
            // Right column: trinket + 2 talismans + 5 extras = 8 rows.
            Assert.Equal(8 * SheetLayout.Pitch - (SheetLayout.Pitch - SheetLayout.Cell), many.Picture.Height);
            Assert.True(many.Stats.Y > At(many, "Extra4").Y + SheetLayout.Cell);
        }

        [Fact]
        public void PictureSitsBetweenTheColumnsAndStatsSpanThePanel()
        {
            var plan = SheetLayout.Plan(Defaults());
            Assert.Equal(At(plan, "Helmet").X + SheetLayout.Cell + SheetLayout.Margin, plan.Picture.X);
            Assert.Equal(plan.Picture.X + plan.Picture.Width + SheetLayout.Margin, At(plan, "Trinket").X);
            Assert.Equal(SheetLayout.Margin, plan.Stats.X);
            Assert.Equal(plan.Width - 2 * SheetLayout.Margin, plan.Stats.Width);
            Assert.Equal(plan.Width, plan.Title.Width);
            Assert.Equal(At(plan, "Trinket").X + SheetLayout.Cell + SheetLayout.Margin, plan.Width);
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
