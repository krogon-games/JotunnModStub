using System.Runtime.CompilerServices;
using Corruption.Rules;
using HarmonyLib;

namespace Corruption.Content
{
    /// <summary>
    ///     The item a hit comes from (rules in <see cref="HitSourceScopes{TItem}" />), so knockback, backstab, life
    ///     steal and Skadi use the bow an arrow was shot from, not whatever the player holds when it lands. Every patch
    ///     here only records; nothing changes the game's own behaviour.
    /// </summary>
    internal static class HitSources
    {
        private sealed class Source
        {
            public ItemDrop.ItemData Item;
        }

        private static readonly HitSourceScopes<ItemDrop.ItemData> Scopes = new HitSourceScopes<ItemDrop.ItemData>();
        /// <summary>Explicit sources (proc hits): the hit object itself carries it.</summary>
        private static readonly ConditionalWeakTable<HitData, Source> Tags = new ConditionalWeakTable<HitData, Source>();
        /// <summary>Projectiles and area effects the game set up without an item, spawned while a hit-making method ran.</summary>
        private static readonly ConditionalWeakTable<object, Source> Spawned = new ConditionalWeakTable<object, Source>();

        /// <summary>Marks a hit our own code makes (procs) with its source item (may be null: no item effects).</summary>
        public static void Tag(HitData hit, ItemDrop.ItemData item)
        {
            if (hit == null)
            {
                return;
            }
            Tags.Remove(hit);
            Tags.Add(hit, new Source { Item = item });
        }

        /// <summary>The item <paramref name="hit" /> comes from, for the local player's own hits.</summary>
        public static ItemDrop.ItemData For(HitData hit, Player player)
        {
            Source found = null;
            bool tagged = hit != null && Tags.TryGetValue(hit, out found);
            return Scopes.Resolve(tagged, found?.Item, () => player ? player.GetCurrentWeapon() : null);
        }

        private static ItemDrop.ItemData OwnOrSpawned(object instance, ItemDrop.ItemData own)
        {
            if (own != null)
            {
                return own;
            }
            return Spawned.TryGetValue(instance, out var source) ? source.Item : null;
        }

        private static void Remember(object instance, ItemDrop.ItemData given)
        {
            if (instance != null && Scopes.SpawnSource(given, out var source))
            {
                Spawned.Remove(instance);
                Spawned.Add(instance, new Source { Item = source });
            }
        }

        // ---- The methods that make a player's hits ----

        [HarmonyPatch]
        private static class AttackScope
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
            private static void Melee(Attack __instance) => Scopes.Push(__instance.m_weapon);

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
            private static void MeleeDone() => Scopes.Pop();

            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Attack), nameof(Attack.DoAreaAttack))]
            private static void Area(Attack __instance) => Scopes.Push(__instance.m_weapon);

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Attack), nameof(Attack.DoAreaAttack))]
            private static void AreaDone() => Scopes.Pop();

            /// <summary>Firing: a projectile the game sets up without an item (none do in 1.0.14) still knows the weapon.</summary>
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
            private static void Fire(Attack __instance) => Scopes.Push(__instance.m_weapon);

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
            private static void FireDone() => Scopes.Pop();
        }

        [HarmonyPatch]
        private static class ProjectileScope
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
            private static void Hit(Projectile __instance) => Scopes.Push(OwnOrSpawned(__instance, __instance.m_weapon));

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
            private static void HitDone() => Scopes.Pop();

            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Projectile), nameof(Projectile.DoAOE))]
            private static void AreaHit(Projectile __instance) => Scopes.Push(OwnOrSpawned(__instance, __instance.m_weapon));

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Projectile), nameof(Projectile.DoAOE))]
            private static void AreaHitDone() => Scopes.Pop();

            /// <summary>Set up without an item (random spawns on hit): remember the spawning method's item.</summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
            private static void Setup(Projectile __instance, ItemDrop.ItemData __4) => Remember(__instance, __4);
        }

        [HarmonyPatch]
        private static class AoeScope
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.First)]
            [HarmonyPatch(typeof(Aoe), nameof(Aoe.OnHit))]
            private static void Hit(Aoe __instance) => Scopes.Push(OwnOrSpawned(__instance, __instance.m_itemData));

            [HarmonyFinalizer]
            [HarmonyPatch(typeof(Aoe), nameof(Aoe.OnHit))]
            private static void HitDone() => Scopes.Pop();

            /// <summary>Set up without an item (an attack's trigger effect, random spawns on hit): remember the spawning method's item.</summary>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Aoe), nameof(Aoe.Setup))]
            private static void Setup(Aoe __instance, ItemDrop.ItemData __4) => Remember(__instance, __4);
        }
    }
}
