using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;

namespace Corruption.Content
{
    /// <summary>
    ///     Map tiers and affixes in the game (Krogon's rips round, 2026-11-26, decisions 6-12): loads
    ///     <c>map_tiers.csv</c> and <c>map_affixes.csv</c>, reads a map item's tier and affixes, and keeps a rip's tier and
    ///     affixes on its objects (entry and exit rips, heart) and on its wave creatures, so every player's game and every
    ///     later owner of those objects reads the same values. When the tables don't load, rips play as before (tier 1).
    /// </summary>
    internal static class MapTiers
    {
        /// <summary>Object data on the rip portals, the heart and each rip creature: the map's tier (int, 1 when missing).</summary>
        public const string KeyTier = "corruption_rip_tier";

        /// <summary>Object data on the rip portals, the heart and each rip creature: the map's affixes as "id:value", joined by <see cref="MapData.Separator" />.</summary>
        public const string KeyAffixes = "corruption_rip_affixes";

        /// <summary>
        ///     Object data on each stamped rip creature (only when it isn't 1): the experience factor its affixes give
        ///     (<see cref="RipTierRules.ExperienceFactor" />, float). <b>Orlog</b> reads this same key by name
        ///     (<c>Orlog/Content/KillXp.cs</c>, which has no reference to Corruption): change both together.
        /// </summary>
        public const string KeyExperience = "corruption_rip_xp";

        /// <summary>Null until <see cref="Load" /> succeeds.</summary>
        public static MapTables Tables { get; private set; }

        public static bool Loaded => Tables != null && Tables.Tiers != null && Tables.Affixes != null && Tables.Tiers.Rows.Count > 0;

        /// <summary>Reads both tables. Problems are logged and leave maps without tiers (every rip plays as tier 1, as before).</summary>
        public static void Load()
        {
            var errors = new List<string>();
            var tiers = MapTierTable.Parse(DataFiles.Read("map_tiers.csv"), errors);
            var affixes = MapAffixTable.Parse(DataFiles.Read("map_affixes.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("The map tier tables have problems, so maps have no tiers or affixes and every rip plays as tier 1:\n" +
                                       string.Join("\n", errors));
                Tables = null;
                return;
            }
            Tables = new MapTables { Tiers = tiers, Affixes = affixes };
            ApplyDisabled();
            Jotunn.Logger.LogInfo($"Map tiers: {tiers.Rows.Count} tiers, {affixes.Affixes.Count} affixes " +
                                  $"({string.Join(", ", affixes.Rollable.Select(a => a.Id).ToArray())} roll).");
        }

        private static bool _watchingDisabled;

        /// <summary>
        ///     Reads <c>[5 - Rips] DisabledMapAffixes</c> into the affix table, and again whenever it changes (the server's
        ///     value arrives through Jötunn's config sync as a setting change), so every game rolls from the same pool.
        /// </summary>
        private static void ApplyDisabled()
        {
            var entry = CorruptionConfig.RipDisabledMapAffixes;
            if (entry == null || Tables == null)
            {
                return;
            }
            var unknown = Tables.Affixes.SetDisabled(entry.Value);
            if (unknown.Count > 0)
            {
                Jotunn.Logger.LogWarning($"DisabledMapAffixes: unknown affix id(s) {string.Join(", ", unknown.ToArray())} ignored.");
            }
            if (Tables.Affixes.Disabled.Count > 0)
            {
                Jotunn.Logger.LogInfo($"Map affixes switched off (never roll): {string.Join(", ", Tables.Affixes.Disabled.ToArray())}.");
            }
            if (!_watchingDisabled)
            {
                _watchingDisabled = true;
                entry.SettingChanged += (sender, args) => Safe.Run("apply DisabledMapAffixes", ApplyDisabled);
            }
        }

        /// <summary>A map item's tier and affixes (tier 1 with none for old maps, or when the tables didn't load).</summary>
        public static MapState StateOf(ItemDrop.ItemData item)
        {
            if (!Loaded)
            {
                return new MapState();
            }
            var unknown = new List<string>();
            var state = MapData.Read(item?.m_customData, Tables.Tiers, Tables.Affixes, unknown);
            if (unknown.Count > 0)
            {
                Jotunn.Logger.LogWarning($"Map {item?.m_shared?.m_name}: unknown affix id(s) {string.Join(", ", unknown.ToArray())} ignored.");
            }
            return state;
        }

        /// <summary>Writes a map's state into the item (custom data). Does nothing when the tables didn't load.</summary>
        public static void Write(ItemDrop.ItemData item, MapState state)
        {
            if (!Loaded || item == null || state == null)
            {
                return;
            }
            if (item.m_customData == null)
            {
                item.m_customData = new Dictionary<string, string>();
            }
            MapData.Write(item.m_customData, state, Tables.Tiers);
        }

        /// <summary>Puts a rip's tier and affixes on an object (rip, heart, creature). The caller must own the object.</summary>
        /// <param name="affixes">The affixes as "id:value;id:value" (<see cref="MapData.JoinRolls(MapState)" />); a bare id reads as the affix's Min.</param>
        public static void Stamp(ZDO zdo, int tier, string affixes)
        {
            if (zdo == null)
            {
                return;
            }
            zdo.Set(KeyTier, tier < 1 ? 1 : tier);
            zdo.Set(KeyAffixes, affixes ?? "");
            // Orlog works out kill XP without knowing Corruption, so the factor travels on the object itself.
            double experience = Loaded ? RipTierRules.ExperienceFactor(RollsOf(affixes)) : 1.0;
            if (experience > 1.0)
            {
                zdo.Set(KeyExperience, (float)experience);
            }
            else
            {
                zdo.RemoveFloat(KeyExperience);
            }
        }

        /// <summary>Copies the tier and affixes from one object to another (heart → exit rip, heart → creature).</summary>
        public static void CopyStamp(ZDO from, ZDO to)
        {
            if (from == null || to == null)
            {
                return;
            }
            Stamp(to, TierOf(from), from.GetString(KeyAffixes, ""));
        }

        /// <summary>Whether the object carries a rip's stamp (wave creatures do; corrupted-boss summons and wild creatures don't).</summary>
        public static bool IsStamped(ZDO zdo) => zdo != null && zdo.GetInt(KeyTier, 0) > 0;

        /// <summary>The tier on an object; 1 when it has none.</summary>
        public static int TierOf(ZDO zdo)
        {
            int tier = zdo != null ? zdo.GetInt(KeyTier, 1) : 1;
            return tier < 1 ? 1 : tier;
        }

        /// <summary>The affix ids on an object (none when it has none).</summary>
        public static List<string> AffixIdsOf(ZDO zdo)
        {
            return zdo != null ? MapData.Split(zdo.GetString(KeyAffixes, "")) : new List<string>();
        }

        /// <summary>The tier's row for an object (null when the tables didn't load).</summary>
        public static MapTierTable.TierRow RowOf(ZDO zdo)
        {
            return Loaded ? Tables.Tiers.Row(TierOf(zdo)) : null;
        }

        /// <summary>The affixes on an object that the table knows, with their values (a bare id reads as Min, values are clamped); unknown ids are skipped.</summary>
        public static List<MapAffixRoll> AffixesOf(ZDO zdo)
        {
            return zdo == null ? new List<MapAffixRoll>() : RollsOf(zdo.GetString(KeyAffixes, ""));
        }

        /// <summary>The same for a stamp's text ("id:value;id:value"); none when the tables didn't load.</summary>
        public static List<MapAffixRoll> RollsOf(string joined)
        {
            var list = new List<MapAffixRoll>();
            if (!Loaded)
            {
                return list;
            }
            foreach (var entry in MapData.ParseEntries(joined))
            {
                var affix = Tables.Affixes.Find(entry.Id);
                if (affix != null && !list.Any(r => r.Affix == affix))
                {
                    list.Add(new MapAffixRoll { Affix = affix, Value = affix.Clamp(entry.HasValue ? entry.Value : affix.Min) });
                }
            }
            return list;
        }

        /// <summary>"glorious 75, brutal 40" for logs and <c>co_rip</c>: each affix with the value this rip rolled.</summary>
        public static string DescribeValues(IEnumerable<MapAffixRoll> rolls)
        {
            var parts = new List<string>();
            foreach (var roll in rolls ?? Enumerable.Empty<MapAffixRoll>())
            {
                parts.Add(roll.Affix.Id + " " + MapData.FormatValue(roll.Value));
            }
            return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "no affixes";
        }

        /// <summary>Whether an object carries a given affix id.</summary>
        public static bool Has(ZDO zdo, string affixId)
        {
            return AffixIdsOf(zdo).Contains(affixId);
        }

        /// <summary>"Tier 3 · Brutal, Teeming" for logs and the console (the names from the table; the rips' hover shows <see cref="MapText.Hover" /> instead).</summary>
        public static string Describe(int tier, IEnumerable<string> affixIds)
        {
            var names = new List<string>();
            foreach (string id in affixIds ?? Enumerable.Empty<string>())
            {
                var affix = Loaded ? Tables.Affixes.Find(id) : null;
                names.Add(affix != null ? affix.Name : id);
            }
            return names.Count > 0 ? $"Tier {tier} · {string.Join(", ", names.ToArray())}" : $"Tier {tier}";
        }
    }
}
