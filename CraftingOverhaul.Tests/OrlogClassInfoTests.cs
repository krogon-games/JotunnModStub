using System;
using System.Collections.Generic;
using System.IO;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogClassInfoTests
    {
        [Fact]
        public void ClassesCarryTheirDescriptionAndTabNames()
        {
            // Moved from the classes UI demo when it was removed (user, 2026-11-13); tabs are D2's.
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "classes.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var errors = new List<string>();
            var t = ClassTable.Parse(File.ReadAllText(Path.Combine(dir, "Orlog", "Data", "classes.csv")), errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            var amazon = t.Find("Amazon");
            Assert.Equal("A huntress of bow and javelin. Fast, precise and hard to hit.", amazon.Description);
            Assert.Equal(new[] { "Bow and Crossbow", "Passive and Magic", "Javelin and Spear" }, amazon.Tabs);
            Assert.Equal("Warcries", t.Find("Barbarian").Tabs[2]);
        }

        [Fact]
        public void OldTablesWithoutTheColumnsStillRead()
        {
            var errors = new List<string>();
            var t = ClassTable.Parse("class,status,str,dex,vit,energy,life_per_lvl,stamina_per_lvl,life_per_vit,stamina_per_vit\nAmazon,playable,20,25,20,15,2,1,3,1\n", errors);
            Assert.Empty(errors);
            Assert.Equal("", t.Find("Amazon").Description);
            Assert.Equal(new[] { "", "", "" }, t.Find("Amazon").Tabs);
        }
    }
}
