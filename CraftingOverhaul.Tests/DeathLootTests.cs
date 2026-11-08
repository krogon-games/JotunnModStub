using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     <see cref="DeathLoot" /> driven in the order the game's death code calls our patches (read in the 1.0.14
    ///     assemblies: <c>Character.OnDeath</c> → <c>Ragdoll.Setup</c> → <c>CharacterDrop.SetDropsEnabled(false)</c> →
    ///     <c>m_onDeath</c> → <c>CharacterDrop.OnDeath</c>; the non-owner returns before the ragdoll and <c>m_onDeath</c>).
    /// </summary>
    public class DeathLootTests
    {
        private sealed class Drop
        {
            public bool Enabled = true;
        }

        /// <summary>One creature death on this game; returns whether it earned Magic/Rare rolls.</summary>
        private static bool Die(DeathLoot deaths, Drop drop, bool owner = true, bool ragdoll = false, bool ragdollDropsItems = true,
            int dropCalls = 1)
        {
            bool rolled = false;
            deaths.BeginDeath(drop, drop != null && drop.Enabled);
            try
            {
                if (!owner)
                {
                    return false; // Character.OnDeath returns early: no ragdoll, no m_onDeath
                }
                if (ragdoll)
                {
                    deaths.RagdollSetup(ragdollDropsItems);
                    if (drop != null && ragdollDropsItems)
                    {
                        drop.Enabled = false; // the game: component.SetDropsEnabled(false)
                    }
                }
                for (int i = 0; i < dropCalls && drop != null; i++)
                {
                    rolled |= deaths.TakeRolls(drop, drop.Enabled);
                }
            }
            finally
            {
                deaths.EndDeath();
            }
            return rolled;
        }

        [Fact]
        public void DropsAtOnce_Rolls()
        {
            Assert.True(Die(new DeathLoot(), new Drop()));
        }

        [Fact]
        public void DropsHandedToTheRagdoll_StillRolls()
        {
            Assert.True(Die(new DeathLoot(), new Drop(), ragdoll: true));
        }

        [Fact]
        public void RagdollWithoutLoot_DropsAtOnceAndRolls()
        {
            Assert.True(Die(new DeathLoot(), new Drop(), ragdoll: true, ragdollDropsItems: false));
        }

        [Fact]
        public void DropsOffBeforeDeath_NoRolls()
        {
            // A genuine no-drop creature (another mod turned its drops off), with or without a ragdoll.
            Assert.False(Die(new DeathLoot(), new Drop { Enabled = false }));
            Assert.False(Die(new DeathLoot(), new Drop { Enabled = false }, ragdoll: true));
        }

        [Fact]
        public void NotTheOwner_NoRolls()
        {
            Assert.False(Die(new DeathLoot(), new Drop(), owner: false, ragdoll: true));
        }

        [Fact]
        public void OnDeathCalledTwice_RollsOnce()
        {
            var deaths = new DeathLoot();
            var drop = new Drop();
            deaths.BeginDeath(drop, true);
            Assert.True(deaths.TakeRolls(drop, true));
            Assert.False(deaths.TakeRolls(drop, true));
            deaths.EndDeath();
        }

        [Fact]
        public void EachDeathIsSeparate_AndTheTrackerEmpties()
        {
            var deaths = new DeathLoot();
            var drop = new Drop();
            Assert.True(Die(deaths, drop, ragdoll: true));
            drop.Enabled = true; // a new creature of the same kind reuses nothing, but even the same object rolls again
            Assert.True(Die(deaths, drop));
            // Untracked (our Character.OnDeath prefix didn't run): the game's own flag decides, as before.
            Assert.True(deaths.TakeRolls(new Drop(), true));
            Assert.False(deaths.TakeRolls(new Drop(), false));
        }

        [Fact]
        public void NestedDeath_RagdollBelongsToTheInnerDeath()
        {
            var deaths = new DeathLoot();
            var outer = new Drop { Enabled = false };
            var inner = new Drop();
            deaths.BeginDeath(outer, outer.Enabled);
            Assert.True(Die(deaths, inner, ragdoll: true));
            Assert.False(deaths.TakeRolls(outer, outer.Enabled));
            deaths.EndDeath();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(3, 3)]
        public void OneRollPerPlayerInRange(int players, int rolls)
        {
            Assert.Equal(rolls, DeathLoot.RollsFor(players));
        }
    }
}
