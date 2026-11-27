using System;
using System.Collections.Generic;
using System.Globalization;

namespace Corruption.Rules
{
    /// <summary>
    ///     The words and colours of a map: its name and colour by rarity (rips round decision 11), the "Tier 3 · Magic map"
    ///     line, and the summary both the inventory tooltip and the Riftforge's Forge tab show (decision 15, 2026-11-27:
    ///     the approved mockup <c>docs/images/map_ui_approved_mockup.png</c> and Krogon's copy): the usage line, "What
    ///     you'll face" (the two-star line, then the map's affixes in table order) and "What you'll earn" (the heart's
    ///     shards, the gear floor, the drop rate). Every number comes from map_tiers.csv, map_affixes.csv and
    ///     drop_modifiers.csv. No game dependencies, so it is unit-tested; <c>tools/mockup_map_ui.py</c> draws it.
    /// </summary>
    internal static class MapText
    {
        // The same colours as Loot.MagicColor / Loot.RareColor / Loot.NormalColor and RiftforgeCrafting.CorruptedColor
        // (those need the game, so they can't be used here).
        public const string NormalColor = "#FFFFFF";
        public const string MagicColor = "#8888FF";
        public const string RareColor = "#FFFF77";
        public const string CorruptedColor = "#E04040";

        public const string NoShatter = "Maps can't be shattered";
        public const string NoSplinter = "Affix Splinters don't work on maps";

        /// <summary>"Meadows map"; a corrupted (tier 6) map is "Corrupted Meadows map" (decision 11).</summary>
        public static string Name(string mapName, MapTierTable.TierRow row)
        {
            return row != null && row.Corrupted ? "Corrupted " + mapName : mapName;
        }

        /// <summary>The map's colour: white, Magic, Rare, or red when corrupted. Used for the name, the header and the number on the icon.</summary>
        public static string ColorOf(MapTierTable.TierRow row)
        {
            return row == null ? NormalColor : row.Corrupted ? CorruptedColor : RarityColor(row.Rarity);
        }

        /// <summary>The colour of a rarity (a tier 6 map's affix lines keep its Rare yellow, as in the picture).</summary>
        public static string RarityColor(string rarity)
        {
            return rarity == "rare" ? RareColor : rarity == "magic" ? MagicColor : NormalColor;
        }

        /// <summary>The colour of the affix lines: the rarity's, also on a corrupted map.</summary>
        public static string LineColorOf(MapTierTable.TierRow row)
        {
            return row == null ? NormalColor : RarityColor(row.Rarity);
        }

        /// <summary>"Tier 3 · Magic map", "Tier 1 · Normal map", "Tier 6 · Corrupted map".</summary>
        public static string Header(MapTierTable.TierRow row)
        {
            if (row == null)
            {
                return "Tier 1 · Normal map";
            }
            string kind = row.Corrupted ? "Corrupted" : row.Rarity == "rare" ? "Rare" : row.Rarity == "magic" ? "Magic" : "Normal";
            return "Tier " + row.Tier.ToString(CultureInfo.InvariantCulture) + " · " + kind + " map";
        }

        /// <summary>The danger text with <c>{value}</c> replaced by the map's rolled number, as a whole number: "Enemies deal 63% more damage."</summary>
        public static string Danger(MapAffixTable.Affix affix, double value)
        {
            return affix.Danger.Replace("{value}", Math.Round(value).ToString("0", CultureInfo.InvariantCulture));
        }

        /// <summary>"Brutal: Enemies deal 63% more damage." (the plain one-line form, e.g. for the Forge's message).</summary>
        public static string AffixLine(MapAffixTable.Affix affix, double value)
        {
            return affix.Name + ": " + Danger(affix, value);
        }

        /// <summary>One line per roll, in the map's order.</summary>
        public static List<string> AffixLines(IEnumerable<MapAffixRoll> rolls)
        {
            var lines = new List<string>();
            foreach (var roll in rolls)
            {
                lines.Add(AffixLine(roll.Affix, roll.Value));
            }
            return lines;
        }

        // ------------------------------------------------------------ the summary (Krogon's decision 15, 2026-11-27)

        /// <summary>Headings: sampled from the approved mockup (docs/images/map_ui_approved_mockup.png).</summary>
        public const string HeadingColor = "#F8BC42";

        /// <summary>Descriptions and bullet text: the Forge dialogs' Body colour (RiftforgeDialog.Body = 0.93, 0.89, 0.81).</summary>
        public const string BodyColor = "#EDE3CF";

        /// <summary>Weight and the rest of the game's own tooltip: the Forge dialogs' Secondary colour (RiftforgeDialog.Secondary = 0.72, 0.68, 0.62).</summary>
        public const string SecondaryColor = "#B8AD9E";

        /// <summary>A half-line gap (the same spacer the Forge's affix blocks use).</summary>
        public const string Spacer = "\n<size=45%> </size>";

        /// <summary>The highlight of the affix lines the last orb use added (the Forge's existing style).</summary>
        private const string MarkOpen = "<mark=#E8C66A40><b>";
        private const string MarkClose = "</b></mark>";

        /// <summary>"Opens a rip into the Black Forest. The map crumbles once used." (label: maps.csv).</summary>
        public static string Usage(string label)
        {
            return "Opens a rip into the " + label + ". The map crumbles once used.";
        }

        /// <summary>The two-star line from the tier's two_star_percent: 100 every enemy, 0 by chance, else at least N%.</summary>
        public static string TwoStarLine(double percent)
        {
            if (percent >= 100)
            {
                return "Every enemy spawned in a wave has two stars.";
            }
            if (percent <= 0)
            {
                return "Enemies spawned in a wave may have two stars by chance.";
            }
            // "At least": creatures that miss the roll still get the game's own level-up chance.
            return "Each enemy spawned in a wave has at least a " + Number(percent) + "% chance of two stars.";
        }

        /// <summary>The "What you'll face" bullets as plain text (no tags, no bullet): the two-star line, then one line per affix on the map in map_affixes.csv order.</summary>
        public static List<string> FaceLines(MapTierTable.TierRow row, IEnumerable<MapAffixRoll> rolls, MapAffixTable affixes, MapGlyphs glyphs)
        {
            var lines = new List<string>();
            foreach (string line in Face(row, rolls, affixes, glyphs, null, false))
            {
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>The "What you'll earn" bullets as plain text; the drop line is left out without a multiplier.</summary>
        public static List<string> EarnLines(MapTierTable.TierRow row, double? dropMultiplier, MapGlyphs glyphs)
        {
            return Earn(row, dropMultiplier, glyphs, false);
        }

        private static List<string> Face(MapTierTable.TierRow row, IEnumerable<MapAffixRoll> rolls, MapAffixTable affixes, MapGlyphs glyphs,
            ICollection<string> highlight, bool rich)
        {
            var lines = new List<string> { rich ? Bullet(TwoStarLine(row.TwoStarPercent)) : TwoStarLine(row.TwoStarPercent) };
            var present = new Dictionary<string, double>();
            foreach (var roll in rolls ?? new MapAffixRoll[0])
            {
                present[roll.Affix.Id] = roll.Value;
            }
            string nameColor = LineColorOf(row);
            // Table order, not the order the affixes were rolled in; ids the table doesn't know are skipped.
            foreach (var affix in affixes.Affixes)
            {
                double value;
                if (!present.TryGetValue(affix.Id, out value))
                {
                    continue;
                }
                if (!rich)
                {
                    lines.Add(affix.Name + " " + glyphs.Dash + " " + Danger(affix, value));
                    continue;
                }
                string line = Bullet("<color=" + nameColor + ">" + affix.Name + "</color><color=" + BodyColor + "> " + glyphs.Dash + " " + Danger(affix, value) + "</color>");
                lines.Add(highlight != null && highlight.Contains(affix.Id) ? MarkOpen + line + MarkClose : line);
            }
            return lines;
        }

        private static List<string> Earn(MapTierTable.TierRow row, double? dropMultiplier, MapGlyphs glyphs, bool rich)
        {
            string b = rich ? "<b>" : "", e = rich ? "</b>" : "";
            string shards = row.ShardsMin == row.ShardsMax
                ? row.ShardsMin.ToString(CultureInfo.InvariantCulture)
                : row.ShardsMin.ToString(CultureInfo.InvariantCulture) + "–" + row.ShardsMax.ToString(CultureInfo.InvariantCulture);
            var lines = new List<string>
            {
                "Destroy the heart for " + b + shards + e + (row.ShardsMax == 1 ? " Corruption Shard" : " Corruption Shards") + " total.",
                row.HeartFloor == "rare" ? "Each eligible player gets Rare gear or better."
                    : row.HeartFloor == "magic" ? "Each eligible player gets Magic gear or better."
                    : "The heart may also drop gear."
            };
            if (dropMultiplier.HasValue)
            {
                lines.Add("Corruption loot is " + b + Number(dropMultiplier.Value) + glyphs.Times + e + " as likely to drop during the rip.");
            }
            if (rich)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = Bullet(lines[i]);
                }
            }
            return lines;
        }

        private static string Bullet(string text)
        {
            return ForgeRules.BulletLine("<color=" + BodyColor + ">" + text + "</color>", false);
        }

        private static string Heading(string text)
        {
            return "<size=120%><b><color=" + HeadingColor + ">" + text + "</color></b></size>";
        }

        /// <summary>
        ///     The block both views share: the usage line, "What you'll face" and "What you'll earn", with the tags of
        ///     TextMeshPro. <paramref name="highlight" /> lists affix ids to mark (the Forge's last-use highlight).
        /// </summary>
        public static string Summary(MapTierTable.TierRow row, string label, IEnumerable<MapAffixRoll> rolls, MapAffixTable affixes,
            double? dropMultiplier, MapGlyphs glyphs, ICollection<string> highlight)
        {
            var text = new System.Text.StringBuilder();
            text.Append("<color=").Append(BodyColor).Append('>').Append(Usage(label)).Append("</color>");
            text.Append(Spacer).Append('\n').Append(Heading("What you" + glyphs.Apostrophe + "ll face"));
            foreach (string line in Face(row, rolls, affixes, glyphs, highlight, true))
            {
                text.Append('\n').Append(line);
            }
            text.Append(Spacer).Append('\n').Append(Heading("What you" + glyphs.Apostrophe + "ll earn"));
            foreach (string line in Earn(row, dropMultiplier, glyphs, true))
            {
                text.Append('\n').Append(line);
            }
            return text.ToString();
        }

        /// <summary>The inventory tooltip: tier line, summary, the Corrupted/Mirrored marks (centred), then the game's own text (weight) in the secondary colour.</summary>
        public static string Tooltip(MapTierTable.TierRow row, string label, IEnumerable<MapAffixRoll> rolls, MapAffixTable affixes,
            double? dropMultiplier, MapGlyphs glyphs, string marks, string secondary)
        {
            var text = new System.Text.StringBuilder(ForgeCaption(row));
            text.Append('\n').Append(Summary(row, label, rolls, affixes, dropMultiplier, glyphs, null));
            string centred = ForgeRules.CentredMarks(marks);
            if (centred.Length > 0)
            {
                // The same half-line gap as between the sections, so the marks don't touch the last bullet.
                text.Append(Spacer).Append(centred);
            }
            secondary = (secondary ?? "").Trim('\n');
            if (secondary.Length > 0)
            {
                text.Append(Spacer).Append("\n<color=").Append(SecondaryColor).Append('>').Append(secondary).Append("</color>");
            }
            return text.ToString();
        }

        /// <summary>The Forge's right column: the summary (no tier line, no weight), the last use's added affixes marked.</summary>
        public static string ForgeSheet(MapTierTable.TierRow row, string label, IEnumerable<MapAffixRoll> rolls, MapAffixTable affixes,
            double? dropMultiplier, MapGlyphs glyphs, ICollection<string> highlight)
        {
            return Summary(row, label, rolls, affixes, dropMultiplier, glyphs, highlight);
        }

        /// <summary>The coloured tier line ("Tier 5 · Rare map"): under the Forge slot and at the top of the tooltip.</summary>
        public static string ForgeCaption(MapTierTable.TierRow row)
        {
            return "<color=" + ColorOf(row) + ">" + Header(row) + "</color>";
        }

        /// <summary>The Forge's bench text under the slot: only the Corrupted/Mirrored marks (empty when neither).</summary>
        public static string ForgeBench(string marks)
        {
            return ForgeRules.CentredMarks(marks).TrimStart('\n');
        }

        /// <summary>A number without trailing zeros, with a dot: 3, 4.5, 1.75.</summary>
        public static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    ///     The special characters the map text uses. The game's font may lack them (it already logs a missing "…"), so
    ///     the game side picks <see cref="Fancy" /> only for a font that has all three, else <see cref="Plain" />.
    /// </summary>
    internal sealed class MapGlyphs
    {
        public string Dash, Times, Apostrophe;

        public static readonly MapGlyphs Fancy = new MapGlyphs { Dash = "—", Times = "×", Apostrophe = "’" };
        public static readonly MapGlyphs Plain = new MapGlyphs { Dash = "-", Times = "x", Apostrophe = "'" };
    }
}
