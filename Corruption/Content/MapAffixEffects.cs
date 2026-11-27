using System.Runtime.CompilerServices;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>What a rip creature's stamp adds up to, worked out once and kept until the stamp changes (see <see cref="RipCreatures" />).</summary>
    internal sealed class CreatureAffixes
    {
        /// <summary>The stamp text these numbers were worked out from.</summary>
        public string Raw;

        public float Damage = 1f;
        public float DoubleChance;
        public float PoisonShare;
        public float FireShare;
        public float LightningShare;
        public float PhysicalTaken = 1f;
        public float ElementalTaken = 1f;
        public float Health = 1f;
        public float MoveBonus;
        public float AttackSpeed = 1f;
        public bool NoStagger;

        /// <summary>Whether the creature's own hits are changed at all.</summary>
        public bool ChangesHits => Damage != 1f || DoubleChance > 0f || PoisonShare > 0f || FireShare > 0f || LightningShare > 0f;
    }

    /// <summary>
    ///     Reads a rip creature's stamp (<see cref="MapTiers.KeyTier" />, <see cref="MapTiers.KeyAffixes" />) for the hooks
    ///     below. The stamp text is parsed once per creature and again only when it changes: a hook that runs every frame
    ///     costs two ZDO reads (tier, then the stamp text, compared by reference first) and no allocation; a creature
    ///     without a stamp costs one. Creatures that carry no stamp (wild ones, a corrupted boss's summons) and players
    ///     give <c>null</c>.
    /// </summary>
    internal static class RipCreatures
    {
        private static readonly int HashTier = MapTiers.KeyTier.GetStableHashCode();
        private static readonly int HashAffixes = MapTiers.KeyAffixes.GetStableHashCode();
        private static readonly ConditionalWeakTable<Character, CreatureAffixes> Cache = new ConditionalWeakTable<Character, CreatureAffixes>();

        public static CreatureAffixes Of(Character character)
        {
            if (!MapTiers.Loaded || !character || character is Player)
            {
                return null;
            }
            var view = character.m_nview;
            var zdo = view ? view.GetZDO() : null;
            if (zdo == null || zdo.GetInt(HashTier, 0) <= 0)
            {
                return null;
            }
            string raw = zdo.GetString(HashAffixes, "");
            if (Cache.TryGetValue(character, out var known) && string.Equals(known.Raw, raw))
            {
                return known;
            }
            var rolls = MapTiers.RollsOf(raw);
            var affixes = new CreatureAffixes
            {
                Raw = raw,
                Damage = (float)RipTierRules.DamageFactor(rolls),
                DoubleChance = (float)RipTierRules.DoubleDamageChance(rolls),
                PoisonShare = (float)RipTierRules.PoisonShare(rolls),
                FireShare = (float)RipTierRules.FireShare(rolls),
                LightningShare = (float)RipTierRules.LightningShare(rolls),
                PhysicalTaken = (float)RipTierRules.PhysicalTaken(rolls),
                ElementalTaken = (float)RipTierRules.ElementalTaken(rolls),
                Health = (float)RipTierRules.HealthFactor(rolls),
                MoveBonus = (float)RipTierRules.MoveSpeedBonus(rolls),
                AttackSpeed = (float)RipTierRules.AttackSpeedFactor(rolls),
                NoStagger = RipTierRules.NoStagger(rolls)
            };
            Cache.Remove(character);
            Cache.Add(character, affixes);
            return affixes;
        }

        /// <summary>Whether this game owns the creature (it simulates its movement, attacks and health).</summary>
        public static bool IsOwner(Character character)
        {
            return character.m_nview && character.m_nview.IsValid() && character.m_nview.IsOwner();
        }
    }

    /// <summary>
    ///     The map affixes that change how rip creatures fight (rips round, 2026-11-26, decisions 8 and 10; Krogon's affix
    ///     list 2026-11-27). Each reads the rip's stamp on the creature itself, never the heart, so it holds wherever the
    ///     creature goes. Density (Teeming) is in <see cref="RipWaves" />, experience (Glorious) is read by Orlog from the
    ///     stamp's <see cref="MapTiers.KeyExperience" />, the affixes that act on players near the heart are in
    ///     <see cref="RipCurse" />. Every patch class below is applied on its own, so one broken by a game update only
    ///     disables its own affix. Hooks that run every frame avoid closures (no <see cref="Safe.Run" /> allocation) and report
    ///     through <see cref="Safe.Report" />.
    /// </summary>
    internal static class MapAffixEffects
    {
        /// <summary>
        ///     Called right after a creature gets its stamp (the game that stamped it owns it): the Health affixes raise its
        ///     maximum, and a creature that was at full health starts at the new full health.
        /// </summary>
        public static void OnStamped(Character character)
        {
            Safe.Run("raise a stamped creature's health", () =>
            {
                if (!character || !RipCreatures.IsOwner(character))
                {
                    return;
                }
                var affixes = RipCreatures.Of(character);
                if (affixes != null && affixes.Health != 1f)
                {
                    character.SetupMaxHealth();   // base x level, then RaiseHealth below
                }
            });
        }

        /// <summary>
        ///     Damage, Ruthless (double damage) and the poison, fire and lightning shares: one prefix on a stamped creature's
        ///     hit, replacing the old Smouldering patch. On the attacker's game, before the hit is sent to the victim's owner.
        ///     In order: the share of the hit's physical damage (blunt, slash, pierce) is added as poison, fire and
        ///     lightning, then the whole hit is multiplied by the damage factor, and by 2 on the Ruthless roll. The roll
        ///     happens once per hit, on whichever game builds it (melee and projectiles on the creature's owner, some area
        ///     hits on the victim's owner), so there is no owner check (Opus review, 2026-11-26). Damage-over-time hits
        ///     (<see cref="HitSources.IsDamageOverTime" />) are skipped so burning and poison never feed themselves; the
        ///     creature never hurts itself; players are never changed.
        ///     Priority Low: after Orlog's crit (VeryHigh), Corruption's on-hit affixes (High) and life steal (Normal), which
        ///     only act on a player's hits anyway; they see the hit as before and ours is added last, from the final damage.
        ///     Game: Character.Damage, Character.cs:2233. Can't skip the original.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static class RipCreatureHits
        {
            [HarmonyPriority(Priority.Low)]
            private static void Prefix(Character __instance, HitData hit)
            {
                if (hit == null || hit.m_attacker.IsNone() || HitSources.IsDamageOverTime(hit))
                {
                    return;
                }
                try
                {
                    var attacker = hit.GetAttacker();
                    if (!attacker || attacker == __instance || attacker.IsPlayer())
                    {
                        return;
                    }
                    var affixes = RipCreatures.Of(attacker);
                    if (affixes == null || !affixes.ChangesHits)
                    {
                        return;
                    }
                    var d = hit.m_damage;
                    float physical = d.m_blunt + d.m_slash + d.m_pierce;
                    if (physical > 0f)
                    {
                        hit.m_damage.m_poison += physical * affixes.PoisonShare;
                        hit.m_damage.m_fire += physical * affixes.FireShare;
                        hit.m_damage.m_lightning += physical * affixes.LightningShare;
                    }
                    float factor = affixes.Damage;
                    if (affixes.DoubleChance > 0f && Random.value < affixes.DoubleChance)
                    {
                        factor *= 2f;
                    }
                    if (factor != 1f)
                    {
                        hit.ApplyModifier(factor);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("change a rip creature's hit", ex);
                }
            }
        }

        /// <summary>
        ///     Ironskin and Warded: a stamped creature takes the affix's share of physical (blunt, slash, pierce) or elemental
        ///     (fire, frost, lightning, poison) damage; spirit, chop and pickaxe are untouched. On the game that owns the
        ///     creature, where vanilla applies damage, so the fire and poison that turn into burning and poison are cut too.
        ///     Players are never changed. Normal priority, never skips (replaces the old Hardened patch).
        ///     Game: Character.RPC_Damage, Character.cs:2242; poison and fire become damage over time at Character.cs:2400-2412.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        private static class RipCreatureTaken
        {
            private static void Prefix(Character __instance, HitData hit)
            {
                if (hit == null || __instance is Player)
                {
                    return;
                }
                try
                {
                    if (!RipCreatures.IsOwner(__instance))
                    {
                        return;
                    }
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes == null)
                    {
                        return;
                    }
                    if (affixes.PhysicalTaken != 1f)
                    {
                        hit.m_damage.m_blunt *= affixes.PhysicalTaken;
                        hit.m_damage.m_slash *= affixes.PhysicalTaken;
                        hit.m_damage.m_pierce *= affixes.PhysicalTaken;
                    }
                    if (affixes.ElementalTaken != 1f)
                    {
                        hit.m_damage.m_fire *= affixes.ElementalTaken;
                        hit.m_damage.m_frost *= affixes.ElementalTaken;
                        hit.m_damage.m_lightning *= affixes.ElementalTaken;
                        hit.m_damage.m_poison *= affixes.ElementalTaken;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("reduce a rip creature's damage taken", ex);
                }
            }
        }

        /// <summary>
        ///     Sturdy and Mighty: after the game works out a creature's maximum health (<c>Awake</c> when its owner sees it at
        ///     full health, and <c>SetLevel</c>), a stamped creature's maximum is base x level x the health factor. The maximum
        ///     is kept in the creature's ZDO, so it survives reloads; the <c>Awake</c> path recomputes only at full health
        ///     and gets the same number through this postfix. On the creature's owner only.
        ///     Game: Character.SetupMaxHealth, Character.cs:748 (called at 703 and 782). Postfix, can't skip.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.SetupMaxHealth))]
        private static class RaiseHealth
        {
            private static void Postfix(Character __instance)
            {
                if (__instance is Player)
                {
                    return;
                }
                try
                {
                    if (!RipCreatures.IsOwner(__instance))
                    {
                        return;
                    }
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes == null || affixes.Health == 1f)
                    {
                        return;
                    }
                    __instance.SetMaxHealth(__instance.GetMaxHealthBase() * __instance.GetLevel() * affixes.Health);
                    // The game only writes a changed health to the creature's data when it differs from this remembered value
                    // (Character.cs:3026); after the maximum moved, it must start from the creature's real health.
                    __instance.m_lastHealth = __instance.GetHealth();
                }
                catch (System.Exception ex)
                {
                    Safe.Report("raise a rip creature's health", ex);
                }
            }
        }

        /// <summary>
        ///     Fleet: a stamped creature's walk, run, swim and turn speeds rise by the affix's share of the base speed. It is
        ///     added the way a speed status effect adds it (<c>SE_Stats.ModifySpeed</c>, SE_Stats.cs:436: speed += base x
        ///     modifier), so with the maddened effect's +15% the two add up (+40%), never multiply. Flying speed isn't
        ///     passed through here: <see cref="FleetFlying" />. The creature's owner moves it, so only it needs this.
        ///     Game: SEMan.ApplyStatusEffectSpeedMods, SEMan.cs:69 (called from Character.cs:1494, 1544, 1584, 1683, 1819, 3258).
        ///     Prefix remembers the base speed, postfix adds; neither skips.
        /// </summary>
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ApplyStatusEffectSpeedMods))]
        private static class Fleet
        {
            private static void Prefix(float speed, out float __state)
            {
                __state = speed;
            }

            private static void Postfix(SEMan __instance, ref float speed, float __state)
            {
                var character = __instance.m_character;
                if (character is Player)
                {
                    return;
                }
                try
                {
                    var affixes = RipCreatures.Of(character);
                    if (affixes != null && affixes.MoveBonus > 0f)
                    {
                        speed += __state * affixes.MoveBonus;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("speed up a rip creature", ex);
                }
            }
        }

        /// <summary>Flying creatures' two speeds as the method started, put back after it.</summary>
        private struct FlySpeeds
        {
            public bool Changed;
            public float Slow;
            public float Fast;
        }

        /// <summary>
        ///     Fleet for flying creatures (deathsquitos, bats): their speed isn't passed through the status effects, so the
        ///     creature's own two flight speeds (per-creature fields, not the shared prefab's) are raised for the length of
        ///     <c>UpdateFlying</c> and put back in a finalizer.
        ///     Game: Character.UpdateFlying, Character.cs:1565 (m_flySlowSpeed, m_flyFastSpeed at 144 and 147).
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.UpdateFlying))]
        private static class FleetFlying
        {
            private static void Prefix(Character __instance, out FlySpeeds __state)
            {
                __state = new FlySpeeds();
                if (__instance is Player)
                {
                    return;
                }
                try
                {
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes != null && affixes.MoveBonus > 0f)
                    {
                        __state.Changed = true;
                        __state.Slow = __instance.m_flySlowSpeed;
                        __state.Fast = __instance.m_flyFastSpeed;
                        __instance.m_flySlowSpeed *= 1f + affixes.MoveBonus;
                        __instance.m_flyFastSpeed *= 1f + affixes.MoveBonus;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("speed up a flying rip creature", ex);
                }
            }

            private static void Finalizer(Character __instance, FlySpeeds __state)
            {
                if (__state.Changed)
                {
                    __instance.m_flySlowSpeed = __state.Slow;
                    __instance.m_flyFastSpeed = __state.Fast;
                }
            }
        }

        /// <summary>
        ///     Frenzied, the animation half: a stamped creature's attack animation plays faster. The game has no attack-speed
        ///     stat; clips set the animator's speed through <c>CharacterAnimEvent.Speed</c> and it rests at 1 otherwise, so
        ///     both are scaled while the creature is attacking, the way the player's Swift is (<see cref="LootGathering" />).
        ///     The owner simulates the attack and the animator's speed is synced to the other games (ZSyncAnimation.cs:106-145).
        ///     Game: CharacterAnimEvent.Speed (CharacterAnimEvent.cs:214) prefix and CustomFixedUpdate (:161) postfix, neither
        ///     skips; both run for every character, so they test the cheap things first (not a player, in an attack).
        /// </summary>
        [HarmonyPatch]
        private static class FasterAttacks
        {
            private static float Factor(CharacterAnimEvent anim)
            {
                var character = anim.m_character;
                if (character is Player || !character || !character.InAttack() || !RipCreatures.IsOwner(character))
                {
                    return 1f;
                }
                var affixes = RipCreatures.Of(character);
                return affixes != null ? affixes.AttackSpeed : 1f;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CharacterAnimEvent), nameof(CharacterAnimEvent.Speed))]
            private static void ClipSpeed(CharacterAnimEvent __instance, ref float speedScale)
            {
                try
                {
                    speedScale *= Factor(__instance);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("speed up a rip creature's attack clip", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CharacterAnimEvent), nameof(CharacterAnimEvent.CustomFixedUpdate))]
            private static void RestingSpeed(CharacterAnimEvent __instance)
            {
                try
                {
                    if (__instance.m_animator && Mathf.Approximately(__instance.m_animator.speed, 1f))
                    {
                        float factor = Factor(__instance);
                        if (factor > 1f)
                        {
                            __instance.m_animator.speed = factor;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("speed up a rip creature's attack", ex);
                }
            }
        }

        /// <summary>
        ///     Frenzied, the waiting half: the AI starts its next attack once <c>aiAttackInterval</c> seconds have passed since
        ///     the weapon's last attack (Humanoid.cs:706, MonsterAI.cs:461). The weapon's last-attack time is the creature's own
        ///     copy of the item (default items are instantiated per creature, Inventory.AddItem), never the shared
        ///     <c>m_shared</c>, so right after an attack starts it is set back by interval x (1 - 1/factor): the wait becomes
        ///     interval / factor, as the animation is factor times quicker. The creature's owner only.
        ///     Game: Attack.Start, Attack.cs:356 (sets m_lastAttackTime at :451). Postfix, can't skip.
        /// </summary>
        [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
        private static class ShorterWait
        {
            private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, bool __result)
            {
                if (!__result || weapon == null || character is Player)
                {
                    return;
                }
                try
                {
                    if (!RipCreatures.IsOwner(character))
                    {
                        return;
                    }
                    var affixes = RipCreatures.Of(character);
                    if (affixes != null && affixes.AttackSpeed > 1f)
                    {
                        weapon.m_lastAttackTime = Time.time - weapon.m_shared.m_aiAttackInterval * (1f - 1f / affixes.AttackSpeed);
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("shorten a rip creature's wait between attacks", ex);
                }
            }
        }

        /// <summary>
        ///     Frenzied, the AI's minimum gap: <c>MonsterAI.m_minAttackInterval</c> (usually 0) is compared with the time since
        ///     the last attack (MonsterAI.cs:462); that time runs factor times faster for a stamped creature.
        ///     Game: Humanoid.GetTimeSinceLastAttack, Humanoid.cs:356. Postfix, can't skip.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetTimeSinceLastAttack))]
        private static class ShorterGap
        {
            private static void Postfix(Humanoid __instance, ref float __result)
            {
                if (__instance is Player)
                {
                    return;
                }
                try
                {
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes != null && affixes.AttackSpeed > 1f)
                    {
                        __result *= affixes.AttackSpeed;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("shorten a rip creature's attack gap", ex);
                }
            }
        }

        /// <summary>
        ///     Unyielding: a stamped creature builds up no stagger. Anything that staggers it directly is stopped too: see
        ///     <see cref="NoStaggerNow" />. Priority Last (the original is skipped, so other prefixes should run first).
        ///     Game: Character.AddStaggerDamage, Character.cs:2614 (called from ApplyDamage, Character.cs:2470, and a blocking
        ///     Humanoid, Humanoid.cs:1794). Prefix, skips the original for a stamped creature with the affix.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.AddStaggerDamage))]
        private static class NoStaggerDamage
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(Character __instance, ref bool __result)
            {
                if (__instance is Player)
                {
                    return true;
                }
                try
                {
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes != null && affixes.NoStagger)
                    {
                        __result = false;
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("stop a rip creature's stagger build-up", ex);
                }
                return true;
            }
        }

        /// <summary>
        ///     Unyielding, direct staggers: the game staggers a creature outright from a hit with a stagger multiplier of 100 or
        ///     more (Character.cs:2264), a perfect parry (Humanoid.cs:1831, <c>attacker.Stagger</c>), a trap (Trap.cs:247) and
        ///     Orlog's Wind Lance (Orlog/Content/WindLance.cs:409). All end up here, so for a stamped creature with the affix
        ///     none of them works: the parry still blocks the hit but doesn't stagger the attacker.
        ///     Priority Last. Game: Character.Stagger, Character.cs:2664. Prefix, skips the original for such a creature
        ///     (runs on whichever game calls it, the stamp is on the creature's data everywhere).
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Stagger))]
        private static class NoStaggerNow
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(Character __instance)
            {
                if (__instance is Player)
                {
                    return true;
                }
                try
                {
                    var affixes = RipCreatures.Of(__instance);
                    if (affixes != null && affixes.NoStagger)
                    {
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    Safe.Report("stop a rip creature's stagger", ex);
                }
                return true;
            }
        }
    }
}
