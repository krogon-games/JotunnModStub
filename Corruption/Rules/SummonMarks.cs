using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Which altars on this game are about to summon a corrupted boss. Only the altar's owner keeps marks, and only
    ///     for a summon a corrupted offering actually started (<see cref="TryStart" />); the mark is taken when the boss
    ///     appears (<see cref="Take" />). Nothing else clears a mark, so a normal or wrong item offered during the spawn
    ///     delay can't turn the corrupted boss into a normal one, and a second offering during the delay starts nothing
    ///     (and so costs nothing). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class SummonMarks<TAltar> where TAltar : class
    {
        private readonly HashSet<TAltar> _marked = new HashSet<TAltar>();

        public int Count => _marked.Count;

        public bool IsMarked(TAltar altar) => altar != null && _marked.Contains(altar);

        /// <summary>
        ///     On the altar's owner, a corrupted offering asks to summon. Refused while a summon is already queued at
        ///     the altar (<paramref name="alreadyQueued" />). Otherwise the altar is marked and <paramref name="start" />
        ///     runs (the game's own summon: it finds a spawn point, queues the boss and takes the offering); if it queued
        ///     nothing (no spawn point found), the mark goes again. True when this request queued the boss.
        /// </summary>
        public bool TryStart(TAltar altar, bool alreadyQueued, Func<bool> start)
        {
            if (altar == null || alreadyQueued)
            {
                return false;
            }
            _marked.Add(altar);
            bool started = false;
            try
            {
                started = start();
            }
            finally
            {
                if (!started)
                {
                    _marked.Remove(altar);
                }
            }
            return started;
        }

        /// <summary>The boss appears: true (once) if a corrupted offering started this summon.</summary>
        public bool Take(TAltar altar)
        {
            return altar != null && _marked.Remove(altar);
        }

        /// <summary>Drops the marks of altars that are gone (unloaded before their boss appeared, so it never will).</summary>
        public int Prune(Predicate<TAltar> gone)
        {
            return _marked.RemoveWhere(gone);
        }
    }
}
