using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogAttributeTests
    {
        private static ClassTable Classes()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "classes.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var errors = new List<string>();
            var table = ClassTable.Parse(File.ReadAllText(Path.Combine(dir, "Orlog", "Data", "classes.csv")), errors);
            Assert.True(errors.Count == 0, "classes.csv has problems:\n" + string.Join("\n", errors));
            return table;
        }

        // The defaults in OrlogConfig.
        private static readonly AttributeScales Defaults = new AttributeScales
        {
            Health = 0.5f, Stamina = 0.89f, EitrStart = 15f, EitrPerLevel = 1.5f, EitrPerEnergy = 1.5f,
            DamagePerStrength = 0.01f, DamagePerDexterity = 0.01f
        };

        private static int[] Spent(int str = 0, int dex = 0, int vit = 0, int ene = 0) => new[] { str, dex, vit, ene };

        [Fact]
        public void OnlyTheAmazonIsPlayable()
        {
            var classes = Classes();
            Assert.Equal(new[] { "Amazon" }, classes.Playable.Select(c => c.Name));
            Assert.NotNull(classes.Find("barbarian"));
            Assert.Null(classes.Find("Druid"));
        }

        [Fact]
        public void AmazonHasDiablo2sNumbers()
        {
            // Arreat Summit, Amazon: Str 20, Dex 25, Vit 20, Energy 15; +2 life and +1 stamina per level; +3 life, +1 stamina per Vitality.
            var amazon = Classes().Find("Amazon");
            Assert.Equal(new[] { 20, 25, 20, 15 }, amazon.Start);
            Assert.Equal(2f, amazon.LifePerLevel);
            Assert.Equal(1f, amazon.StaminaPerLevel);
            Assert.Equal(3f, amazon.LifePerVitality);
            Assert.Equal(1f, amazon.StaminaPerVitality);
        }

        [Fact]
        public void FreshAmazonOnlyGetsStartingEitr()
        {
            var b = AttributeRules.Bonuses(Classes().Find("Amazon"), 1, Spent(), Defaults);
            Assert.Equal(0f, b.Health);
            Assert.Equal(0f, b.Stamina);
            Assert.Equal(15f, b.Eitr);
            Assert.Equal(0f, b.MeleeDamage);
            Assert.Equal(0f, b.RangedDamage);
        }

        [Fact]
        public void LevelsAndPointsAddUp()
        {
            // Level 11 Amazon with 10 Vitality, 20 Dexterity, 10 Energy, 10 Strength (50 points = 10 levels x 5).
            var b = AttributeRules.Bonuses(Classes().Find("Amazon"), 11, Spent(10, 20, 10, 10), Defaults);
            Assert.Equal(0.5f * (2 * 10 + 3 * 10), b.Health, 3);
            Assert.Equal(0.89f * (1 * 10 + 1 * 10), b.Stamina, 3);
            Assert.Equal(15f + 1.5f * 10 + 1.5f * 10, b.Eitr, 3);
            Assert.Equal(0.10f, b.MeleeDamage, 3);
            Assert.Equal(0.20f, b.RangedDamage, 3);
        }

        [Fact]
        public void NoClassNoBonuses()
        {
            var b = AttributeRules.Bonuses(null, 50, Spent(10, 10, 10, 10), Defaults);
            Assert.Equal(0f, b.Health);
            Assert.Equal(0f, b.Eitr);
        }

        [Fact]
        public void PointsFollowDiablo2()
        {
            Assert.Equal(0, AttributeRules.Earned(1, 5));
            Assert.Equal(5, AttributeRules.Earned(2, 5));
            Assert.Equal(490, AttributeRules.Earned(99, 5));
            Assert.Equal(3, AttributeRules.Unspent(2, 5, Spent(vit: 2)));
            // A lowered setting can leave more spent than earned: never negative.
            Assert.Equal(0, AttributeRules.Unspent(2, 1, Spent(vit: 5)));
        }

        [Fact]
        public void BadClassRowsAreReported()
        {
            var errors = new List<string>();
            ClassTable.Parse("class,status,str,dex,vit,energy,life_per_lvl,stamina_per_lvl,life_per_vit,stamina_per_vit\nAmazon,maybe,1,1,1,1,1,1,1,1\n", errors);
            Assert.Contains(errors, e => e.Contains("status"));
        }
    }
}
