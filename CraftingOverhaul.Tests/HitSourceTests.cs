using System;
using Corruption.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     <see cref="HitSourceScopes{TItem}" /> driven like <c>HitSources</c>' patches: a hit-making game method pushes
    ///     its item before it runs and pops it in a finalizer; the hit affixes ask for the source while the hit is made.
    ///     Read in the 1.0.14 assemblies: the bow's shot sets up the arrow with the weapon (<c>Projectile.Setup(...,
    ///     m_weapon, ...)</c>); the arrow builds its hit in <c>Projectile.OnHit</c>, its splash in <c>DoAOE</c>; spawns on
    ///     hit get the arrow's item, except random spawns, which get none.
    /// </summary>
    public class HitSourceTests
    {
        private sealed class Item
        {
            public readonly string Name;
            public Item(string name) => Name = name;
        }

        private static readonly Item Bow = new Item("bow");
        private static readonly Item Sword = new Item("sword");
        private static readonly Item Shield = new Item("shield");

        /// <summary>A game method that makes hits: push, run, pop in a finalizer (even if it throws).</summary>
        private static void During(HitSourceScopes<Item> scopes, Item item, Action body)
        {
            scopes.Push(item);
            try
            {
                body();
            }
            finally
            {
                scopes.Pop();
            }
        }

        private static Item SourceOf(HitSourceScopes<Item> scopes, Item inHand, Item tag = null, bool tagged = false)
        {
            return scopes.Resolve(tagged, tag, () => inHand);
        }

        [Fact]
        public void ArrowLandsAfterASwapToASword_StillTheBow()
        {
            var scopes = new HitSourceScopes<Item>();
            Item arrowWeapon = Bow;   // Projectile.m_weapon, set when it was fired
            Item inHand = Sword;      // switched during the flight
            Item direct = null, splash = null;
            During(scopes, arrowWeapon, () =>
            {
                direct = SourceOf(scopes, inHand);                                      // Projectile.OnHit → Character.Damage
                During(scopes, arrowWeapon, () => splash = SourceOf(scopes, inHand));    // DoAOE inside OnHit
            });
            Assert.Same(Bow, direct);
            Assert.Same(Bow, splash);
            Assert.Equal(0, scopes.Depth);
        }

        [Fact]
        public void SwappingToABow_DoesNotGiveASwordSwingTheBowsAffixes()
        {
            var scopes = new HitSourceScopes<Item>();
            Item source = null;
            During(scopes, Sword, () => source = SourceOf(scopes, inHand: Bow)); // Attack.DoMeleeAttack with m_weapon = sword
            Assert.Same(Sword, source);
        }

        [Fact]
        public void ProjectileFiredWithNoItem_HasNoItemEffects_NotTheWeaponInHand()
        {
            var scopes = new HitSourceScopes<Item>();
            Item source = Bow;
            During(scopes, null, () => source = SourceOf(scopes, inHand: Sword));
            Assert.Null(source);
        }

        [Fact]
        public void AreaEffectSpawnedWithoutAnItem_RemembersTheSpawningShot()
        {
            var scopes = new HitSourceScopes<Item>();
            Item remembered = null;
            bool known = false;
            // Projectile.OnHit spawns a random Aoe with Setup(..., item: null, ...).
            During(scopes, Bow, () => known = scopes.SpawnSource(null, out remembered));
            Assert.True(known);
            Assert.Same(Bow, remembered);

            // Later, the Aoe hits (Aoe.OnHit pushes its own item, or the remembered one); the player now holds a sword.
            Item source = null;
            During(scopes, remembered, () => source = SourceOf(scopes, inHand: Sword));
            Assert.Same(Bow, source);
        }

        [Fact]
        public void SpawnWithItsOwnItem_OrOutsideAnyMethod_RemembersNothing()
        {
            var scopes = new HitSourceScopes<Item>();
            Assert.False(scopes.SpawnSource(null, out _));
            During(scopes, Sword, () => Assert.False(scopes.SpawnSource(Bow, out _)));
        }

        [Fact]
        public void ProcHits_UseTheirExplicitSource_InsideASwing()
        {
            var scopes = new HitSourceScopes<Item>();
            Item wrath = null, reprisal = null;
            During(scopes, Sword, () =>
            {
                wrath = SourceOf(scopes, inHand: Bow, tag: Sword, tagged: true);
                reprisal = SourceOf(scopes, inHand: Bow, tag: Shield, tagged: true);
            });
            Assert.Same(Sword, wrath);
            Assert.Same(Shield, reprisal);
        }

        [Fact]
        public void HitOutsideEveryMethod_FallsBackToTheWeaponInHand()
        {
            var scopes = new HitSourceScopes<Item>();
            Assert.Same(Sword, SourceOf(scopes, inHand: Sword));
        }

        [Fact]
        public void MethodThatThrows_LeavesNoScopeBehind()
        {
            var scopes = new HitSourceScopes<Item>();
            Assert.Throws<InvalidOperationException>(() => During(scopes, Bow, () => throw new InvalidOperationException()));
            Assert.False(scopes.Active);
            Assert.Same(Sword, SourceOf(scopes, inHand: Sword));
        }
    }
}
