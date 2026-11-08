using System.Collections.Generic;

namespace Corruption.Rules
{
    /// <summary>
    ///     Whether a creature's death earns Magic and Rare rolls (no game types, so it's tested). The game has two death
    ///     paths, both on the creature owner's game only (<c>Character.OnDeath</c> returns early elsewhere):
    ///     <list type="number">
    ///         <item>No ragdoll that carries loot: <c>CharacterDrop.OnDeath</c> drops the loot at once.</item>
    ///         <item>
    ///             A ragdoll with <c>m_dropItems</c>: <c>Ragdoll.Setup</c> stores the loot list on the corpse, then the game
    ///             calls <c>CharacterDrop.SetDropsEnabled(false)</c> so <c>CharacterDrop.OnDeath</c> doesn't drop it twice;
    ///             the corpse drops it when it fades. Read in the 1.0.14 assemblies (the cloud stand-in); most creatures
    ///             take this path.
    ///         </item>
    ///     </list>
    ///     Both earn the rolls. A creature whose drops were already off before it died (another mod, a future game
    ///     rule) earns none, as do players and tamed creatures (checked by the caller).
    ///     The game calls, in order: <see cref="BeginDeath" /> (prefix on <c>Character.OnDeath</c>),
    ///     <see cref="RagdollSetup" /> (postfix on <c>Ragdoll.Setup</c>), <see cref="TakeRolls" /> (postfix on
    ///     <c>CharacterDrop.OnDeath</c>), <see cref="EndDeath" /> (finalizer on <c>Character.OnDeath</c>).
    /// </summary>
    internal sealed class DeathLoot
    {
        private sealed class Death
        {
            public object Drop;
            public bool WasEnabled;
            public bool RagdollTookDrops;
            public bool Rolled;
        }

        private readonly List<Death> _dying = new List<Death>();

        /// <summary>A character starts dying; <paramref name="drop" /> is its CharacterDrop (null if none).</summary>
        public void BeginDeath(object drop, bool dropsEnabled)
        {
            _dying.Add(new Death { Drop = drop, WasEnabled = drop != null && dropsEnabled });
        }

        /// <summary>A ragdoll was set up during the current death; <paramref name="dropItems" /> = it carries the loot.</summary>
        public void RagdollSetup(bool dropItems)
        {
            var death = Current();
            if (death != null && dropItems && death.Drop != null)
            {
                death.RagdollTookDrops = true;
            }
        }

        /// <summary>
        ///     <c>CharacterDrop.OnDeath</c> ran for <paramref name="drop" />: true exactly once per death that earns rolls.
        ///     Outside a tracked death (our <c>Character.OnDeath</c> prefix didn't run), the game's own flag decides.
        /// </summary>
        public bool TakeRolls(object drop, bool dropsEnabledNow)
        {
            Death death = null;
            for (int i = _dying.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_dying[i].Drop, drop))
                {
                    death = _dying[i];
                    break;
                }
            }
            if (death == null)
            {
                return dropsEnabledNow;
            }
            if (death.Rolled)
            {
                return false;
            }
            death.Rolled = true;
            return death.WasEnabled && (dropsEnabledNow || death.RagdollTookDrops);
        }

        public void EndDeath()
        {
            if (_dying.Count > 0)
            {
                _dying.RemoveAt(_dying.Count - 1);
            }
        }

        /// <summary>Personal loot: one independent roll per player in range; at least one (the killer may be out of range).</summary>
        public static int RollsFor(int playersInRange) => playersInRange < 1 ? 1 : playersInRange;

        private Death Current() => _dying.Count > 0 ? _dying[_dying.Count - 1] : null;
    }
}
