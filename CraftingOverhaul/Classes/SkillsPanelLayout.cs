using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Where everything in the skills panel goes (redesign 2026-10-31, after the user found the first versions "not
    ///     acceptable"), in panel pixels from its top-left, y down. The panel and the preview image
    ///     (<c>tools/preview_skills_panel.py</c>, fed by <see cref="Describe" />) both use these numbers, so the preview
    ///     shows what the game draws. Pure: no Unity, unit-tested.
    ///
    ///     Left column: the character box (level, XP, stat points, the four attributes) and under it the skill details
    ///     pane (the hovered or selected skill: name, rank, cost, what it does now and next, why it's locked, Learn).
    ///     Right column: the three tabs, the tree on a dark tablet (icons only, as Diablo 2's), and the four skill slots.
    /// </summary>
    internal static class SkillsPanelLayout
    {
        public const float Margin = 24f;
        public const float ColumnGap = 16f;
        public const float BoxGap = 12f;
        public const float HeaderHeight = 64f;
        public const float CloseSize = 36f;
        public const float LeftWidth = 320f;
        public const float TabHeight = 34f;
        public const float TabGap = 6f;
        public const float TabletPadding = 16f;
        public const float SlotSize = 56f;
        public const float SlotGap = 18f;
        public const float SlotsHeight = 80f;
        public const float SlotsLabelWidth = 150f;

        // Character box contents.
        public const float AttributesTop = 116f;
        public const float AttributeRowHeight = 42f;
        public const float PlusSize = 30f;
        public const float BoxPadding = 18f;

        // Details pane contents.
        public const float DetailIcon = 72f;
        public const float LearnHeight = 40f;

        public static float RightX => Margin + LeftWidth + ColumnGap;
        public static float RightWidth => SkillTreeLayout.GridWidth + 2f * TabletPadding;
        public static float Width => RightX + RightWidth + Margin;

        public static SheetRect Tablet => new SheetRect(RightX, HeaderHeight + TabHeight + TabGap, RightWidth,
            SkillTreeLayout.GridHeight + 2f * TabletPadding);

        public static SheetRect Tree => new SheetRect(Tablet.X + TabletPadding, Tablet.Y + TabletPadding,
            SkillTreeLayout.GridWidth, SkillTreeLayout.GridHeight);

        public static SheetRect SlotsRow => new SheetRect(RightX, Bottom(Tablet) + BoxGap, RightWidth, SlotsHeight);

        public static float Height => Bottom(SlotsRow) + Margin;

        public static SheetRect Title => new SheetRect(0f, 12f, Width, 40f);
        public static SheetRect Close => new SheetRect(Width - Margin - CloseSize, 14f, CloseSize, CloseSize);
        public static SheetRect SkillPoints => new SheetRect(RightX, 17f, RightWidth - CloseSize - 14f, 32f);
        /// <summary>Refusals ("No skill points") show here, along the bottom edge.</summary>
        public static SheetRect Message => new SheetRect(RightX, Height - Margin + 1f, RightWidth, 20f);

        public static float TabWidth => (RightWidth - 2f * TabGap) / 3f;
        public static SheetRect Tab(int index) => new SheetRect(RightX + index * (TabWidth + TabGap), HeaderHeight, TabWidth, TabHeight);

        public static SheetRect Character
        {
            get
            {
                float height = AttributesTop + 4 * AttributeRowHeight + 8f;
                return new SheetRect(Margin, HeaderHeight, LeftWidth, height);
            }
        }

        public static SheetRect Details
        {
            get
            {
                float top = Bottom(Character) + BoxGap;
                return new SheetRect(Margin, top, LeftWidth, Height - Margin - top);
            }
        }

        /// <summary>A skill slot in the row under the tablet, after its label.</summary>
        public static SheetRect Slot(int index)
        {
            var row = SlotsRow;
            return new SheetRect(row.X + SlotsLabelWidth + index * (SlotSize + SlotGap), row.Y + 2f, SlotSize, SlotSize);
        }

        public static SheetRect SlotsLabel => new SheetRect(SlotsRow.X + 4f, SlotsRow.Y + 2f, SlotsLabelWidth - 8f, SlotSize);

        // ---- inside the character box (relative to it)

        public static SheetRect Level => new SheetRect(BoxPadding, 12f, LeftWidth - 2f * BoxPadding, 32f);
        public static SheetRect XpBar => new SheetRect(BoxPadding, 50f, LeftWidth - 2f * BoxPadding, 10f);
        public static SheetRect XpText => new SheetRect(BoxPadding, 64f, LeftWidth - 2f * BoxPadding, 18f);
        public static SheetRect StatPoints => new SheetRect(BoxPadding, 86f, LeftWidth - 2f * BoxPadding, 20f);

        public static SheetRect AttributeName(int i) => new SheetRect(BoxPadding, AttributesTop + i * AttributeRowHeight, 150f, 22f);
        public static SheetRect AttributeEffect(int i) => new SheetRect(BoxPadding, AttributesTop + i * AttributeRowHeight + 21f, 220f, 17f);

        public static SheetRect AttributeValue(int i) =>
            new SheetRect(LeftWidth - BoxPadding - PlusSize - 12f - 60f, AttributesTop + i * AttributeRowHeight, 60f, 22f);

        public static SheetRect AttributePlus(int i) =>
            new SheetRect(LeftWidth - BoxPadding - PlusSize, AttributesTop + i * AttributeRowHeight + 2f, PlusSize, PlusSize);

        // ---- inside the details pane (relative to it)

        public static SheetRect DetailsIcon => new SheetRect(BoxPadding, BoxPadding, DetailIcon, DetailIcon);
        public static SheetRect DetailsName => new SheetRect(BoxPadding + DetailIcon + 14f, BoxPadding + 4f, LeftWidth - 2f * BoxPadding - DetailIcon - 14f, 28f);
        public static SheetRect DetailsRank => new SheetRect(DetailsName.X, DetailsName.Y + 30f, DetailsName.Width, 20f);
        public static SheetRect DetailsKind => new SheetRect(DetailsName.X, DetailsRank.Y + 20f, DetailsName.Width, 18f);

        /// <summary>Description, next level and the lock reason, one block of wrapped text.</summary>
        public static SheetRect DetailsText
        {
            get
            {
                float top = BoxPadding + DetailIcon + 14f;
                return new SheetRect(BoxPadding, top, LeftWidth - 2f * BoxPadding, Details.Height - top - LearnHeight - 2f * BoxPadding);
            }
        }

        public static SheetRect Learn => new SheetRect(BoxPadding, Details.Height - BoxPadding - LearnHeight, LeftWidth - 2f * BoxPadding, LearnHeight);

        public static float Bottom(SheetRect r) => r.Y + r.Height;

        /// <summary>Every named rectangle, in panel space, for the preview image and the tests.</summary>
        public static List<KeyValuePair<string, SheetRect>> Describe()
        {
            var list = new List<KeyValuePair<string, SheetRect>>();
            void Add(string name, SheetRect r) => list.Add(new KeyValuePair<string, SheetRect>(name, r));
            void AddIn(string name, SheetRect box, SheetRect r) => Add(name, new SheetRect(box.X + r.X, box.Y + r.Y, r.Width, r.Height));

            Add("panel", new SheetRect(0f, 0f, Width, Height));
            Add("title", Title);
            Add("close", Close);
            Add("skillPoints", SkillPoints);
            Add("message", Message);
            for (int i = 0; i < 3; i++)
            {
                Add("tab" + i, Tab(i));
            }
            Add("tablet", Tablet);
            Add("tree", Tree);
            Add("slotsLabel", SlotsLabel);
            for (int i = 0; i < 4; i++)
            {
                Add("slot" + i, Slot(i));
            }
            var character = Character;
            Add("character", character);
            AddIn("level", character, Level);
            AddIn("xpBar", character, XpBar);
            AddIn("xpText", character, XpText);
            AddIn("statPoints", character, StatPoints);
            for (int i = 0; i < 4; i++)
            {
                AddIn("attributeName" + i, character, AttributeName(i));
                AddIn("attributeEffect" + i, character, AttributeEffect(i));
                AddIn("attributeValue" + i, character, AttributeValue(i));
                AddIn("attributePlus" + i, character, AttributePlus(i));
            }
            var details = Details;
            Add("details", details);
            AddIn("detailsIcon", details, DetailsIcon);
            AddIn("detailsName", details, DetailsName);
            AddIn("detailsRank", details, DetailsRank);
            AddIn("detailsKind", details, DetailsKind);
            AddIn("detailsText", details, DetailsText);
            AddIn("learn", details, Learn);
            return list;
        }
    }
}
