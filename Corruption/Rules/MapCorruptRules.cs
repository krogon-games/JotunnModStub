namespace Corruption.Rules
{
    /// <summary>
    ///     A Corruption Shard on a map (Krogon's rips round decisions 6 and 6c, 2026-11-26): only a full tier 5 map (4 affixes)
    ///     can be corrupted; it always becomes tier 6, locked, and is never destroyed. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class MapCorruptRules
    {
        /// <summary>Why the shard can't be used on the map, or null when it can.</summary>
        public static string Refusal(MapState state, MapTables tables)
        {
            if (state.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            if (state.Corrupted)
            {
                return "This map is already corrupted.";
            }
            if (state.Affixes.Count != tables.Tiers.MaxAffixes)
            {
                return "Only a tier " + tables.Tiers.TierFor(tables.Tiers.MaxAffixes, false) + " map (" + tables.Tiers.MaxAffixes +
                       " affixes) can be corrupted.";
            }
            return null;
        }

        /// <summary>The corrupted map (a new object), or null when <see cref="Refusal" /> says no. The map is never destroyed.</summary>
        public static MapState Apply(MapState state, MapTables tables, out string headline)
        {
            string refusal = Refusal(state, tables);
            if (refusal != null)
            {
                headline = refusal;
                return null;
            }
            var next = state.Copy();
            next.Corrupted = true;
            next.Refresh(tables.Tiers);
            headline = "Corrupted: now tier " + next.Tier;
            return next;
        }
    }
}
