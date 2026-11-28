namespace Corruption.Rules
{
    /// <summary>
    ///     Who corrupted creatures fight (Krogon's decisions 20-22 of 2026-11-28, branch claude/wizardly-wozniak-4aix76;
    ///     replaces the design's "maddened creatures treat every other creature and player as an enemy"). "Corrupted" is a
    ///     maddened creature (a rip's waves and the creatures near the heart when they start), a corrupted boss or its
    ///     summons. Corrupted creatures are never enemies of each other, nor of normal creatures; they fight players and
    ///     players' tamed creatures (decision 22), whatever a talisman says. Between two creatures that aren't corrupted the
    ///     game's own answer (with the talisman) stands. The game uses the same check for targeting and for who a hit can
    ///     damage, so it holds both ways. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class CorruptedTargeting
    {
        public struct Side
        {
            public bool Corrupted;
            public bool Player;
            public bool Tamed;
        }

        public static bool IsEnemy(Side a, Side b, bool vanilla)
        {
            if (!a.Corrupted && !b.Corrupted)
            {
                return vanilla;
            }
            if (a.Corrupted && b.Corrupted)
            {
                return false;
            }
            var other = a.Corrupted ? b : a;
            return other.Player || other.Tamed;
        }
    }
}
