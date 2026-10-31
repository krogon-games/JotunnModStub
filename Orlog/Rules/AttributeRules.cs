using System;
using System.Collections.Generic;
using System.Linq;

namespace Orlog.Rules
{
    /// <summary>The four Diablo 2 attributes, in D2's order.</summary>
    internal enum Attribute
    {
        Strength,
        Dexterity,
        Vitality,
        Energy
    }

    /// <summary>
    ///     The classes and their Diablo 2 gains (<c>Orlog/Data/classes.csv</c>, from <c>data/d2-reference/classes.csv</c>).
    ///     No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class ClassTable
    {
        public sealed class Class
        {
            public string Name;
            public bool Playable;
            /// <summary>D2 starting attributes, by <see cref="Attribute" />.</summary>
            public int[] Start = new int[4];
            public float LifePerLevel;
            public float StaminaPerLevel;
            public float LifePerVitality;
            public float StaminaPerVitality;
        }

        private readonly List<Class> _classes = new List<Class>();

        public IEnumerable<Class> All => _classes;

        public IEnumerable<Class> Playable => _classes.Where(c => c.Playable);

        public Class Find(string name)
        {
            return string.IsNullOrEmpty(name) ? null : _classes.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static ClassTable Parse(string csv, List<string> errors)
        {
            var result = new ClassTable();
            var rows = Table.Read(csv, "classes.csv", new[]
            {
                "class", "status", "str", "dex", "vit", "energy", "life_per_lvl", "stamina_per_lvl", "life_per_vit", "stamina_per_vit"
            }, errors);
            foreach (var row in rows)
            {
                string status = row["status"].Trim().ToLowerInvariant();
                if (status != "playable" && status != "later")
                {
                    errors.Add($"classes.csv: {row["class"]} has status '{status}' (playable or later).");
                }
                var c = new Class
                {
                    Name = row["class"].Trim(),
                    Playable = status == "playable",
                    Start = new[] { Table.Int(row, "str", errors), Table.Int(row, "dex", errors), Table.Int(row, "vit", errors), Table.Int(row, "energy", errors) },
                    LifePerLevel = Table.Float(row, "life_per_lvl", errors),
                    StaminaPerLevel = Table.Float(row, "stamina_per_lvl", errors),
                    LifePerVitality = Table.Float(row, "life_per_vit", errors),
                    StaminaPerVitality = Table.Float(row, "stamina_per_vit", errors)
                };
                if (c.Name.Length == 0 || result.Find(c.Name) != null)
                {
                    errors.Add($"classes.csv: class '{c.Name}' is empty or listed twice.");
                    continue;
                }
                result._classes.Add(c);
            }
            return result;
        }
    }

    /// <summary>How Diablo 2 numbers turn into Valheim ones (admin settings; see <c>OrlogConfig</c>).</summary>
    internal struct AttributeScales
    {
        /// <summary>Valheim health per D2 life point.</summary>
        public float Health;
        /// <summary>Valheim stamina per D2 stamina point.</summary>
        public float Stamina;
        /// <summary>Eitr a class starts with, gains per level and per Energy point (user, 2026-10-30: 15, 1.5, 1.5).</summary>
        public float EitrStart;
        public float EitrPerLevel;
        public float EitrPerEnergy;
        /// <summary>Damage bonus per Strength point (melee and thrown) and per Dexterity point (bows), e.g. 0.01 = 1%.</summary>
        public float DamagePerStrength;
        public float DamagePerDexterity;
    }

    /// <summary>What a character's class, level and spent points add on top of vanilla.</summary>
    internal struct AttributeBonuses
    {
        public float Health;
        public float Stamina;
        public float Eitr;
        /// <summary>Extra physical damage as a fraction (0.25 = +25%).</summary>
        public float MeleeDamage;
        public float RangedDamage;
    }

    /// <summary>Points and bonuses from attributes. No game dependencies, so it is unit-tested.</summary>
    internal static class AttributeRules
    {
        /// <summary>Stat points earned by <paramref name="level" /> (D2: none at level 1, then per level).</summary>
        public static int Earned(int level, int perLevel)
        {
            return Math.Max(0, level - 1) * Math.Max(0, perLevel);
        }

        /// <summary>Unspent points; never below 0 (a lowered setting can leave more spent than earned).</summary>
        public static int Unspent(int level, int perLevel, int[] spent)
        {
            return Math.Max(0, Earned(level, perLevel) - spent.Sum());
        }

        /// <summary>
        ///     The bonuses of a character of <paramref name="cls" /> at <paramref name="level" /> with
        ///     <paramref name="spent" /> points per attribute. A level-1 character with nothing spent gets only the starting
        ///     eitr: the class's starting attributes match vanilla's starting health, stamina and damage. Health and stamina
        ///     grow by D2's gains per level and per Vitality point, scaled to Valheim; eitr by the user's 15 + 1.5 per level
        ///     + 1.5 per Energy point; damage by Strength and Dexterity points. No class = no bonuses.
        /// </summary>
        public static AttributeBonuses Bonuses(ClassTable.Class cls, int level, int[] spent, AttributeScales scales)
        {
            var bonuses = new AttributeBonuses();
            if (cls == null)
            {
                return bonuses;
            }
            int levelsGained = Math.Max(0, level - 1);
            int vitality = Math.Max(0, spent[(int)Attribute.Vitality]);
            bonuses.Health = scales.Health * (cls.LifePerLevel * levelsGained + cls.LifePerVitality * vitality);
            bonuses.Stamina = scales.Stamina * (cls.StaminaPerLevel * levelsGained + cls.StaminaPerVitality * vitality);
            bonuses.Eitr = scales.EitrStart + scales.EitrPerLevel * levelsGained + scales.EitrPerEnergy * Math.Max(0, spent[(int)Attribute.Energy]);
            bonuses.MeleeDamage = scales.DamagePerStrength * Math.Max(0, spent[(int)Attribute.Strength]);
            bonuses.RangedDamage = scales.DamagePerDexterity * Math.Max(0, spent[(int)Attribute.Dexterity]);
            return bonuses;
        }
    }
}
