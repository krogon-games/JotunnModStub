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

        /// <summary>How many of <see cref="Affixes" /> are corrupted-only (#97): they don't count towards the tier.</summary>
        public int CorruptedOnly;

        /// <summary>Derived from the affix count and <see cref="Corrupted" /> by <see cref="MapTierTable.TierFor" />; call <see cref="Refresh" /> after changing them.</summary>
        public int Tier = 1;

        public MapState Copy()
        {
            return new MapState { Affixes = new List<string>(Affixes), Values = new Dictionary<string, double>(Values), Corrupted = Corrupted, Mirrored = Mirrored, Tier = Tier, CorruptedOnly = CorruptedOnly };
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
            Tier = tiers.TierFor(Affixes.Count - CorruptedOnly, Corrupted);
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
        ///     Reads a map's state. Missing keys give tier 1 with no affixes. Ids not in <paramref name="affixes" />, and
        ///     <c>later</c> ids (designed but not built; the server drops them from a rip too), are dropped (and added to
        ///     <paramref name="unknown" /> when given); repeated ids count once. Nothing else is cleaned up: an old map
        ///     keeps whatever approved affixes it has (Krogon, decision 14: no migration).
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
                        if (affix == null || !affix.Approved)
                        {
                            if (unknown != null)
                            {
                                unknown.Add(entry.Id);
                            }
                        }
                        else if (!state.Affixes.Contains(entry.Id))
                        {
                            state.Affixes.Add(entry.Id);
                            if (affix.CorruptedOnly)
                            {
                                state.CorruptedOnly++;
                            }
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

        /// <summary>
        ///     The server's check of a rip request's tier and affixes (they come from a player's game): unknown and
        ///     <c>later</c> ids are dropped, a repeat counts once, an affix whose group is already on the map is dropped
        ///     (decision 5), values are clamped into the affix's range (none, NaN or infinity read as the Min), no more than
        ///     <see cref="MapTierTable.MaxAffixes" /> are kept, and a corrupted map is one tier above its normal affixes (decision 27; any tier). A
        ///     corrupted-only affix (#97) doesn't count towards that limit: one is kept, and only on a corrupted map. The tier
        ///     follows from the affixes (decision 6). Switched-off affixes are kept: maps that already carry one keep it.
        ///     Every drop is added to <paramref name="warnings" /> (when given) as a sentence for the log.
        /// </summary>
        /// <param name="affixText">The request's "id:value;id:value" text.</param>
        /// <param name="requestedTier">The tier the player's game claims; only used to ask for the corrupted tier.</param>
        public static MapState Sanitize(string affixText, int requestedTier, MapTables tables, List<string> warnings)
        {
            var state = new MapState();
            var tiers = tables.Tiers;
            int normal = 0;
            string corruptedOnly = null;
            // The normal affixes the player's game sent (before anything is dropped): its tier came from these, so a tier
            // above theirs means the map is corrupted, even when the server then drops one of them.
            var sent = new HashSet<string>();
            foreach (var entry in ParseEntries(affixText))
            {
                var known = tables.Affixes.Find(entry.Id);
                if (known == null || !known.CorruptedOnly)
                {
                    sent.Add(entry.Id);
                }
            }
            foreach (var entry in ParseEntries(affixText))
            {
                var known = tables.Affixes.Find(entry.Id);
                bool held = state.Affixes.Contains(entry.Id);
                if (known == null)
                {
                    warnings?.Add($"unknown map affix '{entry.Id}' dropped.");
                }
                else if (!known.Approved || (!held && !tables.Affixes.FitsWith(known, state.Affixes)))
                {
                    // A 'later' affix does nothing, and two of one group (both immunities, both health affixes) are never rolled.
                    warnings?.Add($"map affix '{entry.Id}' dropped ({(known.Approved ? "its group is already on the map" : "not built yet")}).");
                }
                else if (!held && known.CorruptedOnly)
                {
                    if (corruptedOnly != null)
                    {
                        warnings?.Add($"map affix '{entry.Id}' dropped (a map holds one corrupted-only affix).");
                    }
                    else
                    {
                        corruptedOnly = entry.Id;
                        state.Affixes.Add(entry.Id);
                        state.CorruptedOnly = 1;
                        state.Values[entry.Id] = known.Clamp(entry.HasValue ? entry.Value : known.Min);
                    }
                }
                else if (!held && normal < tiers.MaxAffixes)
                {
                    state.Affixes.Add(entry.Id);
                    state.Values[entry.Id] = known.Clamp(entry.HasValue ? entry.Value : known.Min);
                    normal++;
                }
            }
            // Decision 27: any map can be corrupted, one tier above its normal affixes' tier; the player's game asks for it by
            // claiming that tier.
            state.Corrupted = requestedTier > tiers.TierFor(sent.Count, false) && tiers.TierFor(normal, true) > tiers.TierFor(normal, false);
            if (corruptedOnly != null && !state.Corrupted)
            {
                warnings?.Add($"map affix '{corruptedOnly}' dropped (corrupted-only, and the map isn't corrupted).");
                state.Affixes.Remove(corruptedOnly);
                state.Values.Remove(corruptedOnly);
                state.CorruptedOnly = 0;
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
