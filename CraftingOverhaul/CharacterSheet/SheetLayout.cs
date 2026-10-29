using System;
using System.Collections.Generic;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>What kind of EquipmentAndQuickSlots slot a cell is (its three kinds, from its slot API).</summary>
    internal enum SlotKind
    {
        Equipment,
        Custom,
        Quick
    }

    /// <summary>One active slot as EquipmentAndQuickSlots reports it: its public id and its index in that mod's slot table.</summary>
    internal struct SheetSlot
    {
        public string Id;
        public SlotKind Kind;
        public int Index;

        public SheetSlot(string id, SlotKind kind, int index)
        {
            Id = id;
            Kind = kind;
            Index = index;
        }
    }

    /// <summary>Where a slot's cell goes: its top-left corner, in pixels from the panel's top-left (y grows downward).</summary>
    internal struct SlotPlacement
    {
        public string Id;
        public float X;
        public float Y;

        public SlotPlacement(string id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }

    /// <summary>A rectangle in panel space: top-left corner and size, y growing downward.</summary>
    internal struct SheetRect
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public SheetRect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    /// <summary>The computed layout of the character sheet, in panel space (see <see cref="SlotPlacement" />).</summary>
    internal sealed class SheetPlan
    {
        public float Width;
        public float Height;
        public SlotPlacement[] Slots = new SlotPlacement[0];
        /// <summary>The area between the two columns, for the character picture.</summary>
        public SheetRect Picture;
        /// <summary>The stats box under the columns.</summary>
        public SheetRect Stats;
        /// <summary>Where the title goes (top band).</summary>
        public SheetRect Title;
    }

    /// <summary>
    ///     Lays out the character sheet like an RPG paper doll: the armour slots down the left, trinket and the slots other
    ///     mods add (talismans, later rings) down the right, the character in the middle, stats underneath and the quick
    ///     slots in a row at the bottom. Pure: no Unity, unit-tested.
    ///
    ///     EquipmentAndQuickSlots' slot table (3.1.3): quick slots are indices 0–5, equipment slots 8–15 in the order
    ///     Helmet, Chest, Legs, Shoulder, Utility, Trinket, Utility2, Utility3; custom slots take the reserved indices
    ///     6, 7 and 16–23 in the order they were added.
    /// </summary>
    internal static class SheetLayout
    {
        /// <summary>Size of one inventory cell, as the game's grid spaces them.</summary>
        public const float Cell = 70f;
        /// <summary>Distance between slots in a column.</summary>
        public const float Pitch = 80f;
        public const float Margin = 20f;
        public const float TitleHeight = 40f;
        public const float PictureWidth = 220f;
        public const float StatsHeight = 150f;
        public const float QuickGap = 10f;
        /// <summary>Both columns are at least this long, so the picture has room even with few slots.</summary>
        public const int MinRows = 5;

        private const int FirstEquipmentIndex = 8;
        private const int TrinketIndex = 13;
        private const int FirstExtraUtilityIndex = 14;

        /// <summary>Equipment slots that go down the left column, top to bottom, by EquipmentAndQuickSlots index.</summary>
        private static readonly int[] LeftColumn = { 8, 9, 10, 11, 12, 14, 15 };

        public static SheetPlan Plan(IList<SheetSlot> activeSlots)
        {
            var left = new List<SheetSlot>();
            var right = new List<SheetSlot>();
            var quick = new List<SheetSlot>();
            foreach (var slot in activeSlots)
            {
                switch (slot.Kind)
                {
                    case SlotKind.Quick:
                        quick.Add(slot);
                        break;
                    case SlotKind.Equipment:
                        if (slot.Index == TrinketIndex)
                        {
                            right.Add(slot);
                        }
                        else
                        {
                            left.Add(slot);
                        }
                        break;
                    default:
                        right.Add(slot);
                        break;
                }
            }
            left.Sort((a, b) => Array.IndexOf(LeftColumn, a.Index).CompareTo(Array.IndexOf(LeftColumn, b.Index)));
            // Trinket first (lowest index of the right column), then the custom slots in the order they were added.
            right.Sort((a, b) => RightOrder(a).CompareTo(RightOrder(b)));
            quick.Sort((a, b) => a.Index.CompareTo(b.Index));

            int rows = Math.Max(MinRows, Math.Max(left.Count, right.Count));
            float columnsTop = TitleHeight;
            float columnsHeight = rows * Pitch - (Pitch - Cell);
            float leftX = Margin;
            float rightX = Margin + Cell + Margin + PictureWidth + Margin;
            float width = rightX + Cell + Margin;

            var plan = new SheetPlan();
            var placements = new List<SlotPlacement>();
            for (int i = 0; i < left.Count; i++)
            {
                placements.Add(new SlotPlacement(left[i].Id, leftX, columnsTop + i * Pitch));
            }
            for (int i = 0; i < right.Count; i++)
            {
                placements.Add(new SlotPlacement(right[i].Id, rightX, columnsTop + i * Pitch));
            }

            plan.Title = new SheetRect(0f, 0f, width, TitleHeight);
            plan.Picture = new SheetRect(leftX + Cell + Margin, columnsTop, PictureWidth, columnsHeight);
            float statsTop = columnsTop + columnsHeight + Margin;
            plan.Stats = new SheetRect(Margin, statsTop, width - 2 * Margin, StatsHeight);

            float height = statsTop + StatsHeight + Margin;
            if (quick.Count > 0)
            {
                float rowWidth = quick.Count * Cell + (quick.Count - 1) * QuickGap;
                float rowLeft = (width - rowWidth) / 2f;
                for (int i = 0; i < quick.Count; i++)
                {
                    placements.Add(new SlotPlacement(quick[i].Id, rowLeft + i * (Cell + QuickGap), height));
                }
                height += Cell + Margin;
            }

            plan.Width = width;
            plan.Height = height;
            plan.Slots = placements.ToArray();
            return plan;
        }

        private static int RightOrder(SheetSlot slot)
        {
            // The trinket sits above everything other mods add; custom slots keep their table order (added order).
            return slot.Kind == SlotKind.Equipment ? -1 : slot.Index;
        }

        /// <summary>True for the EquipmentAndQuickSlots indices that are equipment slots.</summary>
        public static bool IsEquipmentIndex(int index)
        {
            return index >= FirstEquipmentIndex && index < FirstEquipmentIndex + 8;
        }

        /// <summary>True for the extra utility slots EquipmentAndQuickSlots adds when its utility count is raised.</summary>
        public static bool IsExtraUtilityIndex(int index)
        {
            return index >= FirstExtraUtilityIndex && index < FirstEquipmentIndex + 8;
        }
    }
}
