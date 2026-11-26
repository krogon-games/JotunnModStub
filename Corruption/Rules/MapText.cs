using System.Collections.Generic;
using System.Globalization;

namespace Corruption.Rules
{
    /// <summary>
    ///     The words and colours of a map (Krogon's rips round, decisions 11 and 13, 2026-11-26): its name, its colour by
    ///     rarity, the "Tier 3 · Magic map" header, one line per affix and the lines saying what the tier gives. The
    ///     wording is that of the approved pictures (<c>docs/images/forge_map_*.png</c>, drawn by
    ///     <c>tools/mockup_forge_maps.py</c>). No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapText
    {
        // The same colours as Loot.MagicColor / Loot.RareColor / Loot.NormalColor and RiftforgeCrafting.CorruptedColor
        // (those need the game, so they can't be used here).
        public const string NormalColor = "#FFFFFF";
        public const string MagicColor = "#8888FF";
        public const string RareColor = "#FFFF77";
        public const string CorruptedColor = "#E04040";

        /// <summary>The colour of the tier lines under the affixes.</summary>
        public const string DimColor = "#B0A898";

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

        /// <summary>"Swarming: Twice as many creatures in every wave".</summary>
        public static string AffixLine(MapAffixTable.Affix affix)
        {
            return affix.Name + ": " + affix.Danger;
        }

        /// <summary>One line per affix id the table knows, in the map's order (unknown ids are skipped).</summary>
        public static List<string> AffixLines(IEnumerable<string> ids, MapAffixTable affixes)
        {
            var lines = new List<string>();
            foreach (string id in ids)
            {
                var affix = affixes.Find(id);
                if (affix != null)
                {
                    lines.Add(AffixLine(affix));
                }
            }
            return lines;
        }

        /// <summary>The three lines saying what the tier gives. <paramref name="dropMultiplier" /> is the tier's drop rate from drop_modifiers.csv.</summary>
        public static List<string> TierLines(MapTierTable.TierRow row, double dropMultiplier)
        {
            return new List<string>
            {
                "Waves x" + Number(row.WaveMultiplier) + " · " + TwoStars(row.TwoStarPercent),
                "The heart: " + Shards(row.ShardsMin, row.ShardsMax) + " · " + Floor(row.HeartFloor),
                "Drops x" + Number(dropMultiplier) + " in the rip"
            };
        }

        /// <summary>"every creature two-star", "1 in 2 creatures two-star", "no guaranteed two-stars"; other percentages are spelled out.</summary>
        public static string TwoStars(double percent)
        {
            if (percent <= 0)
            {
                return "no guaranteed two-stars";
            }
            if (percent >= 100)
            {
                return "every creature two-star";
            }
            if (percent == 25)
            {
                return "1 in 4 creatures two-star";
            }
            if (percent == 50)
            {
                return "1 in 2 creatures two-star";
            }
            if (percent == 75)
            {
                return "3 in 4 creatures two-star";
            }
            return Number(percent) + "% of creatures two-star";
        }

        /// <summary>"3–8 shards" (an en dash), "6 shards" when the two are the same, "1 shard".</summary>
        public static string Shards(int min, int max)
        {
            string text = min == max ? min.ToString(CultureInfo.InvariantCulture) : min.ToString(CultureInfo.InvariantCulture) + "–" + max.ToString(CultureInfo.InvariantCulture);
            return text + (max == 1 ? " shard" : " shards");
        }

        /// <summary>"gear by chance", "gear at least Magic", "gear at least Rare".</summary>
        public static string Floor(string floor)
        {
            return floor == "rare" ? "gear at least Rare" : floor == "magic" ? "gear at least Magic" : "gear by chance";
        }

        /// <summary>A number without trailing zeros, with a dot: 3, 4.5, 1.75.</summary>
        public static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>The two lines of the corrupt-map confirmation under the cost (the picture's wording).</summary>
        public static string CorruptExplanation(int corruptTier, string corruptedName)
        {
            return "The map becomes tier " + corruptTier.ToString(CultureInfo.InvariantCulture) + ": " + corruptedName + ".\n" +
                   "It never breaks, but it is locked: no orb can change it afterwards. Only a Mirror of Kalandra still works on it.";
        }
    }
}
