using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Where everything in the skills panel goes (redesign 2026-10-31; skill cards since 2026-11-15, after Krogon's
    ///     mock-up <c>docs/images/skills_panel_cards_mockup.webp</c>), in panel pixels from its top-left, y down. The panel and the preview image
    ///     (<c>tools/preview_skills_panel.py</c>, fed by <see cref="Describe" />) both use these numbers, so the preview
    ///     shows what the game draws. Pure: no Unity, unit-tested.
    ///
    ///     Left column: the character box (level, XP, stat points, the four attributes) and under it the skill details
    ///     pane (the hovered or selected skill: name, rank, cost, what it does now and next).
    ///     Right column: a row of basic skill cards (Gungnir's Throw; always learned, no points), the three tabs, the tab's nine skill cards on a dark tablet (<see cref="SkillCardsLayout" />),
    ///     and a bottom strip with the four skill slots and the Learn button.
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
        /// <summary>The row of basic skill cards above the tabs (Gungnir's Throw; Krogon, 2026-11-28).</summary>
        public const float BasicHeight = 60f;
        public const float BasicMedallion = 44f;
        public const float TabGap = 6f;
        public const float TabletPadding = 16f;
        public const float SlotSize = 56f;
        public const float SlotGap = 18f;
        public const float SlotsHeight = 80f;
        public const float SlotsLabelWidth = 150f;
        public const float LearnWidth = 260f;

        // Character box contents.
        public const float AttributesTop = 116f;
        public const float AttributeRowHeight = 42f;
        public const float PlusSize = 30f;
        public const float BoxPadding = 18f;

        // Details pane contents.
        public const float DetailIcon = 72f;
        public const float LearnHeight = 44f;

        public static float RightX => Margin + LeftWidth + ColumnGap;
        public static float RightWidth => SkillCardsLayout.GridWidth + 2f * TabletPadding;
        public static float Width => RightX + RightWidth + Margin;

        /// <summary>
        ///     Share of the screen the panel may fill when it scales itself (user, 2026-11-13: "please enlarge the skills panel";
        ///     then "maybe even tone it down by 10%": 88% and 92% × 0.9).
        /// </summary>
        public const float FitHeight = 0.79f;
        public const float FitWidth = 0.83f;

        /// <summary>
        ///     The panel's scale on a canvas of the given size: <paramref name="setting" /> when above 0, else as large as fits
        ///     (<see cref="FitHeight" /> of the height, <see cref="FitWidth" /> of the width), never below 1 or above 3.
        /// </summary>
        public static float Scale(float canvasWidth, float canvasHeight, float setting)
        {
            if (setting > 0f)
            {
                return setting;
            }
            if (canvasWidth <= 0f || canvasHeight <= 0f)
            {
                return 1f;
            }
            float fit = System.Math.Min(canvasHeight * FitHeight / Height, canvasWidth * FitWidth / Width);
            return System.Math.Max(1f, System.Math.Min(3f, fit));
        }

        /// <summary>The top of the tabs: under the row of basic skill cards.</summary>
        public static float TabsTop => HeaderHeight + BasicHeight + TabGap;

        /// <summary>A basic skill's card (outside the tabs, always learned), left to right above the tabs.</summary>
        public static SheetRect BasicCard(int index) =>
            new SheetRect(RightX + index * (SkillCardsLayout.CardWidth + SkillCardsLayout.Gap), HeaderHeight, SkillCardsLayout.CardWidth, BasicHeight);

        // ---- inside a basic card (relative to it)

        public static SheetRect BasicIcon => new SheetRect(10f, (BasicHeight - BasicMedallion) / 2f, BasicMedallion, BasicMedallion);
        public static SheetRect BasicName => new SheetRect(BasicMedallion + 20f, 8f, SkillCardsLayout.CardWidth - BasicMedallion - 28f, 24f);
        public static SheetRect BasicLabel => new SheetRect(BasicMedallion + 20f, 32f, SkillCardsLayout.CardWidth - BasicMedallion - 28f, 18f);

        public static SheetRect Tablet => new SheetRect(RightX, TabsTop + TabHeight + TabGap, RightWidth,
            SkillCardsLayout.GridHeight + 2f * TabletPadding);

        /// <summary>The card grid on the tablet.</summary>
        public static SheetRect Cards => new SheetRect(Tablet.X + TabletPadding, Tablet.Y + TabletPadding,
            SkillCardsLayout.GridWidth, SkillCardsLayout.GridHeight);

        public static SheetRect SlotsRow => new SheetRect(RightX, Bottom(Tablet) + BoxGap, RightWidth, SlotsHeight);

        public static float Height => Bottom(SlotsRow) + Margin;

        public static SheetRect Title => new SheetRect(0f, 12f, Width, 40f);
        public static SheetRect Close => new SheetRect(Width - Margin - CloseSize, 14f, CloseSize, CloseSize);
        public static SheetRect SkillPoints => new SheetRect(RightX, 17f, RightWidth - CloseSize - 14f, 32f);
        /// <summary>Refusals ("No skill points") show here, along the bottom edge.</summary>
        public static SheetRect Message => new SheetRect(RightX, Height - Margin + 1f, RightWidth, 20f);

        public static float TabWidth => (RightWidth - 2f * TabGap) / 3f;
        public static SheetRect Tab(int index) => new SheetRect(RightX + index * (TabWidth + TabGap), TabsTop, TabWidth, TabHeight);

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

        /// <summary>A skill slot in the strip under the tablet, after its label.</summary>
        public static SheetRect Slot(int index)
        {
            var row = SlotsRow;
            return new SheetRect(row.X + BoxPadding + SlotsLabelWidth + index * (SlotSize + SlotGap), row.Y + (row.Height - SlotSize) / 2f, SlotSize, SlotSize);
        }

        public static SheetRect SlotsLabel => new SheetRect(SlotsRow.X + BoxPadding, SlotsRow.Y + (SlotsRow.Height - SlotSize) / 2f, SlotsLabelWidth - 8f, SlotSize);

        /// <summary>The Learn button, at the right end of the strip (the mock-up's place for it); it also says why a point can't go in.</summary>
        public static SheetRect Learn => new SheetRect(SlotsRow.X + SlotsRow.Width - BoxPadding - LearnWidth, SlotsRow.Y + (SlotsRow.Height - LearnHeight) / 2f,
            LearnWidth, LearnHeight);

        /// <summary>A thin line between the slots and the Learn button.</summary>
        public static SheetRect SlotsDivider
        {
            get
            {
                float left = Slot(3).X + SlotSize, right = Learn.X;
                return new SheetRect((left + right) / 2f, SlotsRow.Y + 14f, 1f, SlotsRow.Height - 28f);
            }
        }

        // ---- inside the character box (relative to it)

        public static SheetRect Level => new SheetRect(BoxPadding, 12f, LeftWidth - 2f * BoxPadding, 32f);
        public static SheetRect XpBar => new SheetRect(BoxPadding, 50f, LeftWidth - 2f * BoxPadding, 10f);
        public static SheetRect XpText => new SheetRect(BoxPadding, 64f, LeftWidth - 2f * BoxPadding, 18f);
        public static SheetRect StatPoints => new SheetRect(BoxPadding, 86f, LeftWidth - 2f * BoxPadding, 20f);

        /// <summary>
        ///     The attribute's icon, left of its name (Krogon's mock-up, 2026-11-15). The art fills about three quarters of
        ///     its square (make_skill_icons.py's content circle), so the square hangs a little into the box's padding and
        ///     the drawing itself lines up with the text above.
        /// </summary>
        public const float AttributeIconSize = 42f;
        public const float AttributeTextX = BoxPadding + 34f;

        public static SheetRect AttributeIcon(int i) =>
            new SheetRect(BoxPadding - 7f, AttributesTop + i * AttributeRowHeight + (AttributeRowHeight - AttributeIconSize) / 2f - 1f, AttributeIconSize, AttributeIconSize);

        public static SheetRect AttributeName(int i) => new SheetRect(AttributeTextX, AttributesTop + i * AttributeRowHeight, 140f, 22f);
        public static SheetRect AttributeEffect(int i) => new SheetRect(AttributeTextX, AttributesTop + i * AttributeRowHeight + 21f, 214f, 17f);

        public static SheetRect AttributeValue(int i) =>
            new SheetRect(LeftWidth - BoxPadding - PlusSize - 12f - 60f, AttributesTop + i * AttributeRowHeight, 60f, 22f);

        public static SheetRect AttributePlus(int i) =>
            new SheetRect(LeftWidth - BoxPadding - PlusSize, AttributesTop + i * AttributeRowHeight + 2f, PlusSize, PlusSize);

        // ---- inside the details pane (relative to it)

        public static SheetRect DetailsIcon => new SheetRect(BoxPadding, BoxPadding, DetailIcon, DetailIcon);
        public static SheetRect DetailsName => new SheetRect(BoxPadding + DetailIcon + 14f, BoxPadding + 4f, LeftWidth - 2f * BoxPadding - DetailIcon - 14f, 28f);
        public static SheetRect DetailsRank => new SheetRect(DetailsName.X, DetailsName.Y + 30f, DetailsName.Width, 20f);
        public static SheetRect DetailsKind => new SheetRect(DetailsName.X, DetailsRank.Y + 20f, DetailsName.Width, 18f);

        /// <summary>Description, next rank and how it's used, one block of wrapped text down to the bottom of the pane.</summary>
        public static SheetRect DetailsText
        {
            get
            {
                float top = BoxPadding + DetailIcon + 14f;
                return new SheetRect(BoxPadding, top, LeftWidth - 2f * BoxPadding, Details.Height - top - BoxPadding);
            }
        }

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
            var basic = BasicCard(0);
            Add("basic0", basic);
            AddIn("basicIcon", basic, BasicIcon);
            AddIn("basicName", basic, BasicName);
            AddIn("basicLabel", basic, BasicLabel);
            for (int i = 0; i < 3; i++)
            {
                Add("tab" + i, Tab(i));
            }
            Add("tablet", Tablet);
            Add("cards", Cards);
            Add("slotsRow", SlotsRow);
            Add("slotsLabel", SlotsLabel);
            Add("slotsDivider", SlotsDivider);
            Add("learn", Learn);
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
                AddIn("attributeIcon" + i, character, AttributeIcon(i));
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
            return list;
        }
    }
}
