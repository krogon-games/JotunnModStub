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

    /// <summary>
    ///     Where a slot goes, in pixels from the panel's top-left (y grows downward): its tile (the framed square with
    ///     the label under it) and, inside that, the top-left corner of EquipmentAndQuickSlots' cell.
    /// </summary>
    internal struct SlotPlacement
    {
        public string Id;
        public SheetRect Tile;
        public float CellX;
        public float CellY;
        /// <summary>Equipment and custom slots get a label under the cell; quick slots show their hotkey instead.</summary>
        public bool Labelled;
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
        public SheetRect Title;
        public SheetRect Close;
        /// <summary>The arched frame between the two columns, for the character picture.</summary>
        public SheetRect Picture;
        /// <summary>The parchment stats box under the columns.</summary>
        public SheetRect Stats;
        /// <summary>The name ribbon across the top edge of the stats box.</summary>
        public SheetRect Ribbon;
    }

    /// <summary>
    ///     Lays out the character sheet like an RPG paper doll (the user's concept image, 2026-10-29): the armour slots
    ///     down the left, the trinket and the slots other mods add (talismans, later rings) down the right, the
    ///     character in an arch in the middle, a parchment stats box underneath and the quick slots in a row at the
    ///     bottom. Pure: no Unity, unit-tested.
    ///
    ///     EquipmentAndQuickSlots' slot table (3.1.3): quick slots are indices 0–5, equipment slots 8–15 in the order
    ///     Helmet, Chest, Legs, Shoulder, Utility, Trinket, Utility2, Utility3; custom slots take the reserved indices
    ///     6, 7 and 16–23 in the order they were added.
    ///
    ///     The art in <c>Assets/Sheet</c> (made by <c>tools/make_sheet_ui.py</c>) is drawn for these sizes.
    /// </summary>
    internal static class SheetLayout
    {
        /// <summary>Size of one inventory cell (the game's slot element).</summary>
        public const float Cell = 64f;
        public const float Margin = 18f;
        public const float Gap = 14f;

        public const float TitleInset = 10f;
        public const float TitleTop = 8f;
        public const float TitleHeight = 56f;
        public const float CloseSize = 36f;

        public const float ColumnsTop = 72f;
        public const float TileWidth = 84f;
        public const float TileHeight = 96f;
        public const float TilePitch = 102f;
        /// <summary>Cell's top inside a labelled tile; the label fills the rest under it.</summary>
        public const float CellTop = 6f;
        public const float PictureWidth = 270f;

        public const float StatsHeight = 160f;
        public const float RibbonWidth = 300f;
        public const float RibbonHeight = 40f;
        /// <summary>How far the ribbon rises above the stats box's top edge.</summary>
        public const float RibbonRise = 14f;

        public const float QuickTile = 70f;
        public const float QuickGap = 8f;

        /// <summary>Both columns are at least this long, so the picture has room even with few slots.</summary>
        public const int MinRows = 5;

        public const float Width = Margin + TileWidth + Gap + PictureWidth + Gap + TileWidth + Margin;

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
            // Trinket first, then the custom slots in the order they were added (their table order).
            right.Sort((a, b) => RightOrder(a).CompareTo(RightOrder(b)));
            quick.Sort((a, b) => a.Index.CompareTo(b.Index));

            int rows = Math.Max(MinRows, Math.Max(left.Count, right.Count));
            float columnsHeight = rows * TilePitch - (TilePitch - TileHeight);
            float leftX = Margin;
            float rightX = Width - Margin - TileWidth;

            var plan = new SheetPlan();
            var placements = new List<SlotPlacement>();
            for (int i = 0; i < left.Count; i++)
            {
                placements.Add(Labelled(left[i].Id, leftX, ColumnsTop + i * TilePitch));
            }
            for (int i = 0; i < right.Count; i++)
            {
                placements.Add(Labelled(right[i].Id, rightX, ColumnsTop + i * TilePitch));
            }

            plan.Title = new SheetRect(TitleInset, TitleTop, Width - 2 * TitleInset, TitleHeight);
            plan.Close = new SheetRect(Width - TitleInset - 10f - CloseSize, TitleTop + (TitleHeight - CloseSize) / 2f, CloseSize, CloseSize);
            plan.Picture = new SheetRect(leftX + TileWidth + Gap, ColumnsTop, PictureWidth, columnsHeight);
            float statsTop = ColumnsTop + columnsHeight + Gap + RibbonRise;
            plan.Stats = new SheetRect(Margin, statsTop, Width - 2 * Margin, StatsHeight);
            plan.Ribbon = new SheetRect((Width - RibbonWidth) / 2f, statsTop - RibbonRise, RibbonWidth, RibbonHeight);

            float height = statsTop + StatsHeight + Gap;
            if (quick.Count > 0)
            {
                float rowWidth = quick.Count * QuickTile + (quick.Count - 1) * QuickGap;
                float rowLeft = (Width - rowWidth) / 2f;
                for (int i = 0; i < quick.Count; i++)
                {
                    float x = rowLeft + i * (QuickTile + QuickGap);
                    placements.Add(new SlotPlacement
                    {
                        Id = quick[i].Id,
                        Tile = new SheetRect(x, height, QuickTile, QuickTile),
                        CellX = x + (QuickTile - Cell) / 2f,
                        CellY = height + (QuickTile - Cell) / 2f,
                        Labelled = false
                    });
                }
                height += QuickTile + Margin;
            }
            else
            {
                height += Margin - Gap;
            }

            plan.Width = Width;
            plan.Height = height;
            plan.Slots = placements.ToArray();
            return plan;
        }

        private static SlotPlacement Labelled(string id, float x, float y)
        {
            return new SlotPlacement
            {
                Id = id,
                Tile = new SheetRect(x, y, TileWidth, TileHeight),
                CellX = x + (TileWidth - Cell) / 2f,
                CellY = y + CellTop,
                Labelled = true
            };
        }

        private static int RightOrder(SheetSlot slot)
        {
            return slot.Kind == SlotKind.Equipment ? -1 : slot.Index;
        }

        /// <summary>
        ///     The label under a slot, in capitals as in the concept image. The shoulder slot is "Cape" (it also takes
        ///     AdventureBackpacks' backpacks); slots we don't know keep the name their mod gave them.
        /// </summary>
        public static string LabelFor(string id, string fallbackName)
        {
            string label;
            if (id == "Helmet") label = "Head";
            else if (id == "Shoulder") label = "Cape";
            else if (id == "Chest" || id == "Legs" || id == "Trinket") label = id;
            else if (id != null && id.StartsWith("Utility", StringComparison.Ordinal)) label = "Utility";
            else if (id != null && id.StartsWith("CorruptionTalisman", StringComparison.Ordinal)) label = "Talisman";
            else label = string.IsNullOrEmpty(fallbackName) ? id ?? "" : fallbackName;
            return label.ToUpperInvariant();
        }

        /// <summary>
        ///     A vanilla item whose icon is drawn faintly in the slot while it's empty, showing what goes there
        ///     (null for slots we don't know). Prefab names from the 1.0.20 game dump (<c>items.csv</c>).
        /// </summary>
        public static string SilhouetteFor(string id)
        {
            if (id == null) return null;
            if (id == "Helmet") return "HelmetBronze";
            if (id == "Chest") return "ArmorBronzeChest";
            if (id == "Legs") return "ArmorBronzeLegs";
            if (id == "Shoulder") return "CapeDeerHide";
            if (id == "Trinket") return "TrinketBronzeStamina";
            if (id.StartsWith("Utility", StringComparison.Ordinal)) return "BeltStrength";
            // A talisman's icon is its creature family's trophy.
            if (id.StartsWith("CorruptionTalisman", StringComparison.Ordinal)) return "TrophyDeer";
            return null;
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
