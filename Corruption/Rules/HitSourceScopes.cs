using System;
using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Which item a hit comes from, for the hit affixes (knockback, backstab, life steal, Skadi), with no game types so
    ///     it's tested. The game builds a player's hits in a handful of methods, each of which knows its item: the swing
    ///     (<c>Attack.DoMeleeAttack</c> / <c>DoAreaAttack</c>: the attack's weapon), a projectile's hit and its area
    ///     damage (<c>Projectile.OnHit</c> / <c>DoAOE</c>: the item it was fired from) and an area effect
    ///     (<c>Aoe.OnHit</c>: the item it was set up with). While one runs, its item is the source of every hit it makes,
    ///     whatever the player holds by then (a bow swapped for a sword mid-flight). Procs name their source explicitly.
    ///     A hit made outside all of these (another mod's) falls back to the weapon in hand, as before.
    /// </summary>
    internal sealed class HitSourceScopes<TItem> where TItem : class
    {
        private readonly List<TItem> _items = new List<TItem>();

        /// <summary>A hit-making method starts; <paramref name="item" /> may be null (a known source with no item: no item effects).</summary>
        public void Push(TItem item) => _items.Add(item);

        public void Pop()
        {
            if (_items.Count > 0)
            {
                _items.RemoveAt(_items.Count - 1);
            }
        }

        public bool Active => _items.Count > 0;

        public int Depth => _items.Count;

        /// <summary>The innermost running method's item (null outside, or for a source with no item).</summary>
        public TItem Current => _items.Count > 0 ? _items[_items.Count - 1] : null;

        /// <summary>The hit's source: an explicit tag first (procs), then the innermost running method, then <paramref name="inHand" />.</summary>
        public TItem Resolve(bool tagged, TItem taggedItem, Func<TItem> inHand)
        {
            if (tagged)
            {
                return taggedItem;
            }
            return Active ? Current : inHand();
        }

        /// <summary>
        ///     What a projectile or area effect being set up should count as its source when the game gives it no item
        ///     (some spawns pass none): the item of the method spawning it. False: nothing to remember.
        /// </summary>
        public bool SpawnSource(TItem given, out TItem source)
        {
            source = null;
            if (given != null || !Active)
            {
                return false;
            }
            source = Current;
            return source != null;
        }
    }
}
