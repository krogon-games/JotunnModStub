using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Corruption.Rules
{
    /// <summary>
    ///     A Magic or Rare item's rolled affixes, how they are stored on the item (<c>ItemData.m_customData</c>) and how
    ///     they read in names and tooltips. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class LootItem
    {
        public const string RarityKey = "corruption.rarity";
        public const string NameKey = "corruption.name";
        public const string AffixesKey = "corruption.affixes";

        /// <summary>A resonant line's top is this much above the two halves' combined top (AFFIX_RULES: +25%).</summary>
        public const float ResonanceBonus = 1.25f;

        /// <summary>One tooltip line: one affix, or both halves of a twin merged into a resonant line.</summary>
        public sealed class Line
        {
            /// <summary>Affix ids from <c>affixes.csv</c>: one, or the prefix then the suffix of a resonant twin.</summary>
            public string[] Ids;

            public int Value;

            public bool Resonant => Ids.Length == 2;
        }

        public string Rarity = "magic";

        /// <summary>A Rare item's two-word name; empty for Magic items, whose name comes from their affixes.</summary>
        public string Name = "";

        public readonly List<Line> Lines = new List<Line>();

        /// <summary>Writes the item's rarity, name and affixes into the item's custom data.</summary>
        public void Write(IDictionary<string, string> customData)
        {
            customData[RarityKey] = Rarity;
            if (Name.Length > 0)
            {
                customData[NameKey] = Name;
            }
            else
            {
                customData.Remove(NameKey);
            }
            customData[AffixesKey] = string.Join(";",
                Lines.Select(l => string.Join("+", l.Ids) + "=" + l.Value.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>The item's rolled affixes, or null for an item without them (Normal items, or unreadable data).</summary>
        public static LootItem Read(IDictionary<string, string> customData)
        {
            if (customData == null || !customData.TryGetValue(RarityKey, out var rarity) ||
                !customData.TryGetValue(AffixesKey, out var affixes))
            {
                return null;
            }
            var item = new LootItem { Rarity = rarity };
            if (customData.TryGetValue(NameKey, out var name))
            {
                item.Name = name;
            }
            foreach (var part in affixes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int equals = part.LastIndexOf('=');
                if (equals <= 0 || !int.TryParse(part.Substring(equals + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    return null;
                }
                var ids = part.Substring(0, equals).Split('+');
                if (ids.Length > 2 || ids.Any(id => id.Length == 0))
                {
                    return null;
                }
                item.Lines.Add(new Line { Ids = ids, Value = value });
            }
            return item;
        }

        /// <summary>The range a line rolls in: the affix's own range, or the resonant range for a twin.</summary>
        public static void Range(Line line, IDictionary<string, AffixTable.Affix> affixes, out int min, out int max)
        {
            min = max = 0;
            foreach (var id in line.Ids)
            {
                if (affixes.TryGetValue(id, out var affix))
                {
                    min += (int)affix.Min;
                    max += (int)affix.Max;
                }
            }
            if (line.Resonant)
            {
                max = (int)Math.Round(max * ResonanceBonus, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        ///     The item's display name. Magic: prefix name, base name, suffix name ("Sharp Bronze Sword of Embers").
        ///     Rare: its two-word name ("Grim Fang"); the base name shows below it in the tooltip.
        /// </summary>
        public string DisplayName(string baseName, IDictionary<string, AffixTable.Affix> affixes)
        {
            if (Rarity == "rare" && Name.Length > 0)
            {
                return Name;
            }
            string prefix = "", suffix = "";
            foreach (var line in Lines)
            {
                foreach (var id in line.Ids)
                {
                    if (!affixes.TryGetValue(id, out var affix))
                    {
                        continue;
                    }
                    string word = QualityName(affix, line, affixes);
                    if (affix.IsPrefix)
                    {
                        prefix = word;
                    }
                    else
                    {
                        suffix = word;
                    }
                }
            }
            return string.Join(" ", new[] { prefix, baseName, suffix }.Where(s => s.Length > 0));
        }

        /// <summary>
        ///     The affix's name for this roll. Where <c>item_name</c> lists several names separated by "/", they mark
        ///     which part of the range the roll landed in, worst first (AFFIX_RULES: roll-quality names).
        /// </summary>
        public static string QualityName(AffixTable.Affix affix, Line line, IDictionary<string, AffixTable.Affix> affixes)
        {
            var names = affix.ItemName.Split('/').Select(n => n.Trim()).Where(n => n.Length > 0).ToArray();
            if (names.Length <= 1)
            {
                return names.Length == 1 ? names[0] : affix.Name;
            }
            Range(line, affixes, out int min, out int max);
            float position = max > min ? (float)(line.Value - min) / (max - min) : 1f;
            if (affix.LowerIsBetter)
            {
                position = 1f - position;
            }
            return names[Math.Max(0, Math.Min(names.Length - 1, (int)(position * names.Length)))];
        }

        /// <summary>
        ///     One tooltip line: the affix text with its value, the bonus it gives this item when known (e.g. "(+23)"),
        ///     then its roll range in brackets; resonant lines are marked (AFFIX_RULES → Showing ranges).
        /// </summary>
        public static string LineText(Line line, IDictionary<string, AffixTable.Affix> affixes, string bonus = null)
        {
            if (!affixes.TryGetValue(line.Ids[0], out var affix))
            {
                return string.Join("+", line.Ids) + " " + line.Value.ToString(CultureInfo.InvariantCulture);
            }
            Range(line, affixes, out int min, out int max);
            var text = new StringBuilder(affix.Text.Replace("{0}", line.Value.ToString(CultureInfo.InvariantCulture)));
            if (!string.IsNullOrEmpty(bonus))
            {
                text.Append(" (").Append(bonus).Append(')');
            }
            text.Append(" [").Append(min.ToString(CultureInfo.InvariantCulture)).Append('–')
                .Append(max.ToString(CultureInfo.InvariantCulture)).Append(']');
            if (line.Resonant)
            {
                text.Append(" ◆ resonant");
            }
            return text.ToString();
        }

        /// <summary>
        ///     A bonus amount as the tooltip shows it: signed, whole above 10, one decimal below (armour 2 → "+0.3"),
        ///     with an optional unit ("x" for multipliers). Null when it rounds to nothing.
        /// </summary>
        public static string BonusText(float amount, string unit = "")
        {
            float shown = Math.Abs(amount) >= 10f ? (float)Math.Round(amount) : (float)Math.Round(amount, 1);
            if (shown == 0f)
            {
                return null;
            }
            string number = Math.Abs(shown).ToString(Math.Abs(shown) >= 10f ? "0" : "0.#", CultureInfo.InvariantCulture);
            return (shown > 0f ? "+" : "−") + number + unit;
        }

        /// <summary>The total of one effect over the item's lines (e.g. every <c>fire_damage_percent</c> line).</summary>
        public int Total(string effect, IDictionary<string, AffixTable.Affix> affixes)
        {
            int total = 0;
            foreach (var line in Lines)
            {
                if (affixes.TryGetValue(line.Ids[0], out var affix) && affix.Effect == effect)
                {
                    total += line.Value;
                }
            }
            return total;
        }
    }
}
