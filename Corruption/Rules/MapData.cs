using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>One affix on a map with the number this map rolled for it (Krogon, 2026-11-27: each map rolls its own value).</summary>
    internal sealed class MapAffixRoll
    {
        public MapAffixTable.Affix Affix;
        public double Value;
    }

    /// <summary>A map's tier, affixes and locks, as read from (or to be written to) the map item's custom data.</summary>
    internal sealed class MapState
    {
        /// <summary>The affix ids, in the order they were rolled.</summary>
        public List<string> Affixes = new List<string>();

        /// <summary>The value each affix rolled, by id. An id without an entry reads as the affix's Min (<see cref="Rolls" />).</summary>
        public Dictionary<string, double> Values = new Dictionary<string, double>();
        public bool Corrupted;
        public bool Mirrored;

        /// <summary>Derived from the affix count and <see cref="Corrupted" /> by <see cref="MapTierTable.TierFor" />; call <see cref="Refresh" /> after changing them.</summary>
        public int Tier = 1;

        public MapState Copy()
        {
            return new MapState { Affixes = new List<string>(Affixes), Values = new Dictionary<string, double>(Values), Corrupted = Corrupted, Mirrored = Mirrored, Tier = Tier };
        }

        /// <summary>The map's affixes with their values, in roll order; ids the table doesn't know are skipped, values are clamped into the affix's range.</summary>
        public List<MapAffixRoll> Rolls(MapAffixTable table)
        {
            var rolls = new List<MapAffixRoll>();
            foreach (string id in Affixes)
            {
                var affix = table.Find(id);
                if (affix == null)
                {
                    continue;
                }
                double value;
                rolls.Add(new MapAffixRoll { Affix = affix, Value = affix.Clamp(Values.TryGetValue(id, out value) ? value : affix.Min) });
            }
            return rolls;
        }

        public void Refresh(MapTierTable tiers)
        {
            Tier = tiers.TierFor(Affixes.Count, Corrupted);
        }
    }

    /// <summary>
    ///     The map item's custom data (an <c>IDictionary&lt;string,string&gt;</c> like <c>ItemDrop.ItemData.m_customData</c>).
    ///     Rips round decision 5 and the brief: tier and affixes live in the item and travel with the rip request. Old maps
    ///     with none of these keys are tier 1 with no affixes. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapData
    {
        /// <summary>The affixes joined by <see cref="Separator" />, each as "id:value" (a bare "id" reads as the affix's Min).</summary>
        public const string AffixesKey = "corruption.map_affixes";

        /// <summary>The tier, written for readability and the rip request. On read the tier is derived, never trusted alone.</summary>
        public const string TierKey = "corruption.map_tier";

        /// <summary>
        ///     The same strings as <c>RiftforgeCrafting.CorruptedKey</c> and <c>MirroredKey</c> (Corruption/Content/RiftforgeCrafting.cs),
        ///     so the Forge's existing corrupted and mirrored locks apply to maps as they do to gear.
        /// </summary>
        public const string CorruptedKey = "corruption.corrupted";
        public const string MirroredKey = "corruption.mirrored";

        public const char Separator = ';';

        /// <summary>Joins affix ids (no values) for custom data and network packages.</summary>
        public static string Join(IEnumerable<string> ids)
        {
            return ids == null ? "" : string.Join(Separator.ToString(), ids);
        }

        /// <summary>The value after the colon in an "id:value" entry.</summary>
        public const char ValueSeparator = ':';

        /// <summary>One entry of a joined list: the id and, when written, its value.</summary>
        public sealed class Entry
        {
            public string Id;
            public bool HasValue;
            public double Value;
        }

        /// <summary>Joins the map's affixes as "id:value;id:value" (an id with no rolled value is written bare).</summary>
        public static string JoinRolls(MapState state)
        {
            if (state == null)
            {
                return "";
            }
            var parts = new List<string>();
            foreach (string id in state.Affixes)
            {
                double value;
                parts.Add(state.Values.TryGetValue(id, out value) ? id + ValueSeparator + FormatValue(value) : id);
            }
            return string.Join(Separator.ToString(), parts);
        }

        /// <summary>Joins rolls as "id:value;id:value".</summary>
        public static string JoinRolls(IEnumerable<MapAffixRoll> rolls)
        {
            return rolls == null ? "" : string.Join(Separator.ToString(), rolls.Select(r => r.Affix.Id + ValueSeparator + FormatValue(r.Value)));
        }

        /// <summary>A value without decimals when whole, else up to two, always with a dot.</summary>
        public static string FormatValue(double value)
        {
            return value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>Parses a joined list of "id" and "id:value" entries; empty parts are dropped, a value that isn't a number reads as no value.</summary>
        public static List<Entry> ParseEntries(string joined)
        {
            var entries = new List<Entry>();
            if (string.IsNullOrEmpty(joined))
            {
                return entries;
            }
            foreach (string part in joined.Split(Separator))
            {
                string text = part.Trim();
                if (text.Length == 0)
                {
                    continue;
                }
                var entry = new Entry();
                int colon = text.IndexOf(ValueSeparator);
                if (colon < 0)
                {
                    entry.Id = text;
                }
                else
                {
                    entry.Id = text.Substring(0, colon).Trim();
                    entry.HasValue = double.TryParse(text.Substring(colon + 1).Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out entry.Value);
                    if (entry.HasValue && (double.IsNaN(entry.Value) || double.IsInfinity(entry.Value)))
                    {
                        entry.HasValue = false;
                    }
                }
                if (entry.Id.Length > 0)
                {
                    entries.Add(entry);
                }
            }
            return entries;
        }

        /// <summary>Splits a joined list into ids; empty parts are dropped and any ":value" is cut off.</summary>
        public static List<string> Split(string joined)
        {
            return ParseEntries(joined).Select(e => e.Id).ToList();
        }

        /// <summary>
        ///     Reads a map's state. Missing keys give tier 1 with no affixes. Ids not in <paramref name="affixes" /> are
        ///     dropped (and added to <paramref name="unknown" /> when given); repeated ids count once.
        /// </summary>
        public static MapState Read(IDictionary<string, string> data, MapTierTable tiers, MapAffixTable affixes, List<string> unknown)
        {
            var state = new MapState();
            if (data != null)
            {
                string value;
                if (data.TryGetValue(AffixesKey, out value))
                {
                    foreach (var entry in ParseEntries(value))
                    {
                        var affix = affixes.Find(entry.Id);
                        if (affix == null)
                        {
                            if (unknown != null)
                            {
                                unknown.Add(entry.Id);
                            }
                        }
                        else if (!state.Affixes.Contains(entry.Id))
                        {
                            state.Affixes.Add(entry.Id);
                            // No value (old maps, fixed affixes) reads as Min; a stored value outside the range is clamped.
                            state.Values[entry.Id] = affix.Clamp(entry.HasValue ? entry.Value : affix.Min);
                        }
                    }
                }
                state.Corrupted = data.TryGetValue(CorruptedKey, out value) && value == "1";
                state.Mirrored = data.TryGetValue(MirroredKey, out value) && value == "1";
            }
            state.Refresh(tiers);
            return state;
        }

        /// <summary>Writes the state to the data, keeping the keys consistent: empty ones are removed, the tier is rewritten from the state.</summary>
        public static void Write(IDictionary<string, string> data, MapState state, MapTierTable tiers)
        {
            state.Refresh(tiers);
            SetOrRemove(data, AffixesKey, state.Affixes.Count > 0 ? JoinRolls(state) : null);
            SetOrRemove(data, TierKey, state.Tier > 1 ? state.Tier.ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
            SetOrRemove(data, CorruptedKey, state.Corrupted ? "1" : null);
            SetOrRemove(data, MirroredKey, state.Mirrored ? "1" : null);
        }

        private static void SetOrRemove(IDictionary<string, string> data, string key, string value)
        {
            if (value == null)
            {
                data.Remove(key);
            }
            else
            {
                data[key] = value;
            }
        }
    }
}
