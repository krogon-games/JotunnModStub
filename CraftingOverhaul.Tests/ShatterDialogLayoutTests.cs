using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>The Shatter confirmation's layout (Krogon's brief, 2026-11-10): height from content, nothing overlaps, a reserved footer.</summary>
    public class ShatterDialogLayoutTests
    {
        private const float Line = 22f;

        private static ShatterDialogLayout Layout(float screen, params float[] rows)
        {
            return ShatterDialogLayout.Compute(Line, Line, rows, 2 * Line, Line, screen);
        }

        private static List<ShatterDialogLayout.Box> InOrder(ShatterDialogLayout l)
        {
            var boxes = new List<ShatterDialogLayout.Box> { l.Title, l.Name, l.Cost, l.Warning, l.ListHeader };
            boxes.AddRange(l.Descriptions);
            boxes.Add(l.Explanation);
            boxes.Add(l.Footer);
            return boxes;
        }

        [Theory]
        [InlineData(1, 1)]      // one short affix
        [InlineData(4, 1)]      // the most lines a shatterable item has (a full Rare), all short
        [InlineData(4, 3)]      // ...all long: three wrapped lines each
        [InlineData(2, 5)]
        public void NothingOverlapsAndEverythingIsInsideTheDialog(int rowCount, int linesEach)
        {
            var l = Layout(0f, Enumerable.Repeat(linesEach * Line, rowCount).ToArray());
            var boxes = InOrder(l);
            for (int i = 1; i < boxes.Count; i++)
            {
                Assert.True(boxes[i].Y >= boxes[i - 1].Bottom, $"box {i} starts at {boxes[i].Y}, above the end of box {i - 1} ({boxes[i - 1].Bottom})");
            }
            foreach (var box in boxes.Concat(l.Chances).Concat(new[] { l.Icon, l.ChanceHeader, l.Cancel, l.Confirm }))
            {
                Assert.True(box.X >= 0f && box.Right <= ShatterDialogLayout.Width + 0.01f && box.Y >= 0f && box.Bottom <= l.Height + 0.01f);
            }
            Assert.Equal(l.Footer.Bottom, l.Height);
        }

        [Fact]
        public void TheHeightComesFromTheContent()
        {
            float one = Layout(0f, Line).Height;
            float four = Layout(0f, Line, Line, Line, Line).Height;
            float fourLong = Layout(0f, 3 * Line, 3 * Line, 3 * Line, 3 * Line).Height;
            Assert.Equal(3 * (Line + ShatterDialogLayout.RowGap), four - one, 3);
            Assert.Equal(4 * 2 * Line, fourLong - four, 3);
            // A long item name pushes everything under it down too.
            var tallName = ShatterDialogLayout.Compute(80f, Line, new[] { Line }, 2 * Line, Line, 0f);
            Assert.Equal(one + (80f - ShatterDialogLayout.IconSize), tallName.Height, 3);
        }

        [Fact]
        public void TheButtonsHaveTheirOwnFooterClearOfTheText()
        {
            var l = Layout(0f, 3 * Line, Line, 2 * Line);
            Assert.True(l.Footer.Y >= l.Explanation.Bottom + ShatterDialogLayout.AfterExplanation - 0.01f);
            Assert.True(l.Cancel.Y >= l.Footer.Y && l.Cancel.Bottom <= l.Footer.Bottom);
            Assert.True(l.Confirm.Y >= l.Footer.Y && l.Confirm.Bottom <= l.Footer.Bottom);
            Assert.True(l.Cancel.Right + ShatterDialogLayout.ButtonGap <= l.Confirm.X + 0.01f);   // Cancel left, Shatter right, apart
            Assert.Equal(ShatterDialogLayout.Width / 2f, (l.Cancel.X + l.Confirm.Right) / 2f, 3);   // centred as a pair
        }

        [Fact]
        public void TheChanceColumnIsNarrowRightAlignedAndNeverUnderADescription()
        {
            var l = Layout(0f, 3 * Line, Line, 2 * Line, Line);
            Assert.Equal(4, l.Chances.Count);
            Assert.All(l.Chances, c => Assert.Equal(l.Chances[0].X, c.X));
            Assert.All(l.Chances, c => Assert.Equal(ShatterDialogLayout.Width - ShatterDialogLayout.Padding, c.Right, 3));
            for (int i = 0; i < 4; i++)
            {
                Assert.True(l.Descriptions[i].Right + ShatterDialogLayout.ColumnGap <= l.Chances[i].X + 0.01f);
                Assert.Equal(l.Descriptions[i].Y, l.Chances[i].Y);   // on the affix's first line
            }
            Assert.Equal(l.Chances[0].X, l.ChanceHeader.X);
            Assert.True(ShatterDialogLayout.DescriptionWidth > 5 * ShatterDialogLayout.ChanceWidth);
        }

        [Fact]
        public void ATallDialogShrinksToFitTheScreenInsteadOfClipping()
        {
            var roomy = Layout(1080f, Line, Line);
            Assert.Equal(1f, roomy.Scale);
            // A large UI scale leaves few canvas units of height: the whole dialog scales, proportions kept.
            var tight = Layout(500f, 3 * Line, 3 * Line, 3 * Line, 3 * Line);
            Assert.True(tight.Scale < 1f);
            Assert.Equal(500f * ShatterDialogLayout.MaxScreenShare, tight.Height * tight.Scale, 2);
        }
    }
}
