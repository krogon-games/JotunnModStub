using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>A map's tier, affixes and locks, as read from (or to be written to) the map item's custom data.</summary>
    internal sealed class MapState
    {
        /// <summary>The affix ids, in the order they were rolled.</summary>
        public List<string> Affixes = new List<string>();
        public bool Corrupted;
        public bool Mirrored;

        /// <summary>Derived from the affix count and <see cref="Corrupted" /> by <see cref="MapTierTable.TierFor" />; call <see cref="Refresh" /> after changing them.</summary>
        public int Tier = 1;

        public MapState Copy()
        {
            return new MapState { Affixes = new List<string>(Affixes), Corrupted = Corrupted, Mirrored = Mirrored, Tier = Tier };
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
        /// <summary>The affix ids joined by <see cref="Separator" />.</summary>
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

        /// <summary>Joins affix ids for custom data and network packages.</summary>
        public static string Join(IEnumerable<string> ids)
        {
            return ids == null ? "" : string.Join(Separator.ToString(), ids);
        }

        /// <summary>Splits a joined id list; empty parts are dropped.</summary>
        public static List<string> Split(string joined)
        {
            if (string.IsNullOrEmpty(joined))
            {
                return new List<string>();
            }
            return joined.Split(Separator).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
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
                    foreach (var id in Split(value))
                    {
                        if (affixes.Find(id) == null)
                        {
                            if (unknown != null)
                            {
                                unknown.Add(id);
                            }
                        }
                        else if (!state.Affixes.Contains(id))
                        {
                            state.Affixes.Add(id);
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
            SetOrRemove(data, AffixesKey, state.Affixes.Count > 0 ? Join(state.Affixes) : null);
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
