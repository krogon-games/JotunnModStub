using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Where everything sits in the Forge's two confirmations, Shatter and Corrupt (Krogon's brief, 2026-11-10; the
    ///     same design for both): a centred title; the item's icon and name; the cost; the warning; a two-column list
    ///     (a wide description column, a narrow right-aligned chance column); a short explanation; the "Don't warn me
    ///     again" tickbox; and a footer reserved for Cancel (left) and the action (right). The height
    ///     comes from the content: the game measures each wrapped text at the widths given here and this class stacks
    ///     them. When the dialog would be taller than the screen allows, the whole dialog is scaled down, so nothing
    ///     clips and nothing overlaps. Units are canvas units at 16-point body text; the dialog multiplies them by the
    ///     game's own text size. Y runs down from the dialog's top. No game dependencies, so it is unit-tested;
    ///     <c>tools/mockup_forge_dialogs.py</c> draws the same numbers.
    /// </summary>
    internal sealed class ForgeDialogLayout
    {
        public const float Width = 600f;
        public const float Padding = 34f;
        public const float TitleTop = 26f;
        public const float TitleHeight = 34f;
        public const float AfterTitle = 20f;
        public const float IconSize = 56f;
        public const float IconGap = 14f;
        public const float AfterTarget = 8f;
        public const float CostHeight = 20f;
        public const float AfterCost = 16f;
        public const float AfterWarning = 20f;
        public const float HeaderHeight = 22f;
        public const float AfterHeader = 12f;
        public const float RowGap = 12f;
        public const float AfterRows = 18f;
        public const float AfterExplanation = 18f;
        public const float TickSize = 20f;
        public const float TickGap = 10f;
        public const float AfterTick = 20f;
        public const float FooterHeight = 78f;
        public const float ButtonWidth = 160f;
        public const float ButtonHeight = 42f;
        public const float ButtonGap = 48f;
        public const float ChanceWidth = 72f;
        public const float ColumnGap = 18f;

        /// <summary>Most of the screen's height the dialog may take before it is scaled down.</summary>
        public const float MaxScreenShare = 0.9f;

        public const float ContentWidth = Width - 2f * Padding;

        /// <summary>The width affix descriptions wrap in.</summary>
        public const float DescriptionWidth = ContentWidth - ChanceWidth - ColumnGap;

        /// <summary>The width the item's name wraps in, beside the icon.</summary>
        public const float NameWidth = ContentWidth - IconSize - IconGap;

        public struct Box
        {
            public float X, Y, W, H;

            public float Bottom => Y + H;
            public float Right => X + W;
        }

        public Box Title, Icon, Name, Cost, Warning, ListHeader, ChanceHeader, Divider, Explanation, Tick, TickLabel, Footer, Cancel, Confirm;
        public readonly List<Box> Descriptions = new List<Box>();
        public readonly List<Box> Chances = new List<Box>();
        public float Height;

        /// <summary>1, or less when the dialog has to shrink to fit the screen.</summary>
        public float Scale = 1f;

        /// <summary>
        ///     Stacks the dialog. Heights are the measured heights of the wrapped texts: the name at
        ///     <see cref="NameWidth" />, the warning and explanation at <see cref="ContentWidth" />, each affix at
        ///     <see cref="DescriptionWidth" />. <paramref name="lineHeight" /> is one line of body text (a row is never
        ///     shorter). <paramref name="availableHeight" /> is the screen's height in the same units (0: don't scale).
        /// </summary>
        public static ForgeDialogLayout Compute(float nameHeight, float warningHeight, IList<float> rowHeights, float explanationHeight,
            float lineHeight, float availableHeight)
        {
            var l = new ForgeDialogLayout();
            float x = Padding, y = TitleTop;
            l.Title = new Box { X = x, Y = y, W = ContentWidth, H = TitleHeight };
            y = l.Title.Bottom + AfterTitle;

            float targetHeight = Math.Max(IconSize, nameHeight);
            l.Icon = new Box { X = x, Y = y + (targetHeight - IconSize) / 2f, W = IconSize, H = IconSize };
            l.Name = new Box { X = x + IconSize + IconGap, Y = y + (targetHeight - nameHeight) / 2f, W = NameWidth, H = nameHeight };
            y += targetHeight + AfterTarget;

            l.Cost = new Box { X = x, Y = y, W = ContentWidth, H = CostHeight };
            y = l.Cost.Bottom + AfterCost;

            l.Warning = new Box { X = x, Y = y, W = ContentWidth, H = Math.Max(lineHeight, warningHeight) };
            y = l.Warning.Bottom + AfterWarning;

            float chanceX = x + ContentWidth - ChanceWidth;
            l.ListHeader = new Box { X = x, Y = y, W = DescriptionWidth, H = HeaderHeight };
            l.ChanceHeader = new Box { X = chanceX, Y = y, W = ChanceWidth, H = HeaderHeight };
            l.Divider = new Box { X = x, Y = l.ListHeader.Bottom + 3f, W = ContentWidth, H = 2f };
            y = l.ListHeader.Bottom + AfterHeader;

            for (int i = 0; i < rowHeights.Count; i++)
            {
                float h = Math.Max(lineHeight, rowHeights[i]);
                l.Descriptions.Add(new Box { X = x, Y = y, W = DescriptionWidth, H = h });
                // The chance sits on the row's first line, right-aligned in its own column.
                l.Chances.Add(new Box { X = chanceX, Y = y, W = ChanceWidth, H = lineHeight });
                y += h + (i < rowHeights.Count - 1 ? RowGap : 0f);
            }
            y += AfterRows;

            l.Explanation = new Box { X = x, Y = y, W = ContentWidth, H = explanationHeight };
            y = l.Explanation.Bottom + AfterExplanation;

            // "Don't warn me again": its own row, left-aligned with the text, above the footer.
            float tickRow = Math.Max(TickSize, lineHeight);
            l.Tick = new Box { X = x, Y = y + (tickRow - TickSize) / 2f, W = TickSize, H = TickSize };
            l.TickLabel = new Box { X = x + TickSize + TickGap, Y = y, W = ContentWidth - TickSize - TickGap, H = tickRow };
            y += tickRow + AfterTick;

            // The footer is its own band: the buttons never share space with text.
            l.Footer = new Box { X = 0f, Y = y, W = Width, H = FooterHeight };
            float buttonsLeft = (Width - (2f * ButtonWidth + ButtonGap)) / 2f;
            float buttonY = y + (FooterHeight - ButtonHeight) / 2f - 4f;
            l.Cancel = new Box { X = buttonsLeft, Y = buttonY, W = ButtonWidth, H = ButtonHeight };
            l.Confirm = new Box { X = buttonsLeft + ButtonWidth + ButtonGap, Y = buttonY, W = ButtonWidth, H = ButtonHeight };
            l.Height = l.Footer.Bottom;

            if (availableHeight > 0f && l.Height > availableHeight * MaxScreenShare)
            {
                l.Scale = availableHeight * MaxScreenShare / l.Height;
            }
            return l;
        }
    }
}
