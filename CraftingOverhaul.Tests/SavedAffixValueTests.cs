using System.Collections.Generic;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     Saved affix values (<see cref="LootItem.Read" />): anything that isn't a finite number makes the item give no
    ///     effects, without changing its saved text; finite values are kept as saved, even outside today's ranges.
    /// </summary>
    public class SavedAffixValueTests
    {
        private static Dictionary<string, string> Saved(string affixes) => new Dictionary<string, string>
        {
            [LootItem.RarityKey] = "rare",
            [LootItem.NameKey] = "Grim Fang",
            [LootItem.AffixesKey] = affixes
        };

        [Theory]
        [InlineData("swift=NaN")]
        [InlineData("swift=Infinity")]
        [InlineData("swift=-Infinity")]
        [InlineData("swift=∞")]
        [InlineData("swift=1e39")]
        [InlineData("swift=-1e39")]
        [InlineData("swift=12;keen_eyed=NaN")]
        [InlineData("sharp+of_embers=Infinity;swift=12")]
        public void NonFiniteValuesMakeTheItemUnreadableAndKeepItsData(string affixes)
        {
            var data = Saved(affixes);
            var before = new Dictionary<string, string>(data);
            Assert.Null(LootItem.Read(data));
            Assert.Equal(before, data);
        }

        [Theory]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("1e39")]
        [InlineData("")]
        [InlineData("twelve")]
        public void TypedValuesMustBeFinite(string text)
        {
            Assert.False(LootItem.TryParseValue(text, out _));
        }

        [Fact]
        public void FiniteValuesAreKeptAsSavedOutsideTodaysRanges()
        {
            // An older roll far above swift's range, a negative one, and a resonant line above its resonant top.
            var item = LootItem.Read(Saved("swift=999;keen_eyed=-3.5;sharp+of_embers=123.45"));
            Assert.NotNull(item);
            Assert.Equal(999f, item.Lines[0].Value);
            Assert.Equal(-3.5f, item.Lines[1].Value);
            Assert.True(item.Lines[2].Resonant);
            Assert.Equal(123.45f, item.Lines[2].Value);
            Assert.True(LootItem.TryParseValue("3.4e38", out float big));
            Assert.Equal(3.4e38f, big);
        }
    }
}
