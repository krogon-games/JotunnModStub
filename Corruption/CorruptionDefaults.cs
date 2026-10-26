namespace Corruption
{
    /// <summary>Default values shared by the config and the unit tests (no game dependencies).</summary>
    internal static class CorruptionDefaults
    {
        /// <summary>Quest and named minibosses talismans must never repel.</summary>
        public const string ExcludedCreatures = "*Hildir*, GoblinBruteBros*, Charred_Melee_Dyrnwyn";
    }
}
