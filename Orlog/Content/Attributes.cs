using System.Globalization;
using HarmonyLib;
using Orlog.Rules;

namespace Orlog.Content
{
    /// <summary>
    ///     The local player's class and spent attribute points, saved in the character's custom data (<c>orlog_class</c>,
    ///     <c>orlog_str</c>, <c>orlog_dex</c>, <c>orlog_vit</c>, <c>orlog_ene</c>), and what they add: max health, stamina
    ///     and eitr (through the game's own total of food values, so vanilla regen refills them, eitr without eitr food) and
    ///     physical damage (through the attacker's attack modifiers).
    /// </summary>
    internal static class Attributes
    {
        public const string ClassKey = "orlog_class";
        private static readonly string[] SpentKeys = { "orlog_str", "orlog_dex", "orlog_vit", "orlog_ene" };

        public static string[] Names => new[] { "strength", "dexterity", "vitality", "energy" };

        public static ClassTable Classes => Progress.Classes;

        public static ClassTable.Class ClassOf(Player player)
        {
            return player && Classes != null && player.m_customData.TryGetValue(ClassKey, out string name) ? Classes.Find(name) : null;
        }

        public static void SetClass(Player player, ClassTable.Class cls)
        {
            if (cls == null)
            {
                player.m_customData.Remove(ClassKey);
            }
            else
            {
                player.m_customData[ClassKey] = cls.Name;
            }
            Changed();
        }

        public static int[] Spent(Player player)
        {
            var spent = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (player && player.m_customData.TryGetValue(SpentKeys[i], out string text) &&
                    int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value > 0)
                {
                    spent[i] = value;
                }
            }
            return spent;
        }

        public static int Unspent(Player player)
        {
            return AttributeRules.Unspent(Progress.GetLevel(player), OrlogConfig.StatPointsPerLevel.Value, Spent(player));
        }

        /// <summary>The attribute's value as D2 shows it: the class's starting value plus spent points.</summary>
        public static int Value(Player player, Attribute attribute)
        {
            var cls = ClassOf(player);
            return (cls != null ? cls.Start[(int)attribute] : 0) + Spent(player)[(int)attribute];
        }

        public static bool TryParse(string name, out Attribute attribute)
        {
            for (int i = 0; i < 4; i++)
            {
                if (string.Equals(Names[i], name, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Names[i].Substring(0, 3), name, System.StringComparison.OrdinalIgnoreCase))
                {
                    attribute = (Attribute)i;
                    return true;
                }
            }
            attribute = Attribute.Strength;
            return false;
        }

        /// <summary>Spends <paramref name="count" /> points; "" on success, otherwise why not.</summary>
        public static string Spend(Player player, Attribute attribute, int count)
        {
            if (ClassOf(player) == null)
            {
                return "Choose a class at the Orlog stone first";
            }
            if (count <= 0 || Unspent(player) < count)
            {
                return "No stat points";
            }
            var spent = Spent(player);
            player.m_customData[SpentKeys[(int)attribute]] = (spent[(int)attribute] + count).ToString(CultureInfo.InvariantCulture);
            Changed();
            return "";
        }

        /// <summary>Gives back every spent point (D2's respec).</summary>
        public static void Respec(Player player)
        {
            foreach (var key in SpentKeys)
            {
                player.m_customData.Remove(key);
            }
            Changed();
        }

        public static AttributeScales Scales()
        {
            return new AttributeScales
            {
                Health = OrlogConfig.HealthPerD2Life.Value,
                Stamina = OrlogConfig.StaminaPerD2Stamina.Value,
                EitrStart = OrlogConfig.EitrStart.Value,
                EitrPerLevel = OrlogConfig.EitrPerLevel.Value,
                EitrPerEnergy = OrlogConfig.EitrPerEnergy.Value,
                DamagePerStrength = OrlogConfig.DamagePerStrength.Value / 100f,
                DamagePerDexterity = OrlogConfig.DamagePerDexterity.Value / 100f
            };
        }

        public static AttributeBonuses BonusesOf(Player player)
        {
            return AttributeRules.Bonuses(ClassOf(player), Progress.GetLevel(player), Spent(player), Scales());
        }

        // The bonuses are read every frame (food totals, every attack): cached until anything changes.
        private static int s_cachedAt = -1;
        private static AttributeBonuses s_cached;

        private static AttributeBonuses LocalBonuses()
        {
            var player = Player.m_localPlayer;
            int stamp = Progress.ChangeCounter;
            if (s_cachedAt != stamp)
            {
                s_cached = BonusesOf(player);
                s_cachedAt = stamp;
            }
            return s_cached;
        }

        private static void Changed()
        {
            Progress.NotifyChanged();
        }

        /// <summary>Health, stamina and eitr from the class go into the game's own total (base values plus food).</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        private static class AddToMaxValues
        {
            private static void Postfix(Player __instance, ref float __0, ref float __1, ref float __2)
            {
                if (__instance != Player.m_localPlayer || !Progress.Ready)
                {
                    return;
                }
                try
                {
                    var bonuses = LocalBonuses();
                    __0 += bonuses.Health;
                    __1 += bonuses.Stamina;
                    __2 += bonuses.Eitr;
                }
                catch (System.Exception ex)
                {
                    Safe.Report("add class health, stamina and eitr", ex);
                }
            }
        }

        /// <summary>Strength and Dexterity raise the physical damage of the local player's attacks (melee, thrown, arrows).</summary>
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyAttack))]
        private static class AddDamage
        {
            private static void Postfix(SEMan __instance, Skills.SkillType __0, ref HitData __1)
            {
                if (__1 == null || __instance.m_character == null || __instance.m_character != Player.m_localPlayer || !Progress.Ready)
                {
                    return;
                }
                try
                {
                    var bonuses = LocalBonuses();
                    float bonus;
                    switch (__0)
                    {
                        case Skills.SkillType.Bows:
                        case Skills.SkillType.Crossbows:
                            bonus = bonuses.RangedDamage;
                            break;
                        case Skills.SkillType.ElementalMagic:
                        case Skills.SkillType.BloodMagic:
                            bonus = 0f;
                            break;
                        default:
                            bonus = bonuses.MeleeDamage;
                            break;
                    }
                    if (bonus > 0f)
                    {
                        float factor = 1f + bonus;
                        __1.m_damage.m_blunt *= factor;
                        __1.m_damage.m_slash *= factor;
                        __1.m_damage.m_pierce *= factor;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("add attribute damage", ex);
                }
            }
        }
    }
}
