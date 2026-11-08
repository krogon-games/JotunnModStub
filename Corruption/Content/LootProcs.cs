using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Proc and utility affixes (AFFIX_RULES.md → Proc and utility affixes; numbers in <see cref="ProcRules" />):
    ///     parry buffs for you and allies (Shieldwall, Rallying, Skald's, Steadfast), Reprisal's lightning, Wrath's
    ///     shockwave, Skadi's frost stacks, kill effects (Bloodforged, Fenrir), Deathless and Foraging.
    ///     <list type="bullet">
    ///         <item>Parries, the player's hits and the player's death all happen on the player's own game, so most of this runs there.</item>
    ///         <item>A kill is decided on the creature owner's game, which sends "you killed it" to the killer's game.</item>
    ///         <item>Buffs reach allies through the game's own status-effect message (<c>SEMan.AddStatusEffect</c> → <c>RPC_AddStatusEffect</c>); the rolled value travels as its skill-level number.</item>
    ///     </list>
    /// </summary>
    internal static class LootProcs
    {
        public const string ShieldwallEffect = "SE_CorruptionShieldwall";
        public const string RallyingEffect = "SE_CorruptionRallying";
        public const string SkaldsEffect = "SE_CorruptionSkalds";
        public const string SteadfastEffect = "SE_CorruptionSteadfast";
        public const string FenrirEffect = "SE_CorruptionFenrir";
        public const string ForagingEffect = "SE_CorruptionForaging";
        public const string DeathlessEffect = "SE_CorruptionDeathless";

        private const string RpcKill = "Corruption_LootKill";
        private const string DeathlessKey = "corruption.deathless_used";

        // Vanilla visuals and icons, read from the game's own items and status effects (names checked in the 1.0.20 dump).
        private const string LightningSource = "StaffLightning";        // Dundr: its bolt's hit effect
        private const string ShockwaveSource = "SledgeStagbreaker";     // Stagbreaker: its slam's area effect
        private static readonly string[][] IconSources =
        {
            new[] { ShieldwallEffect, "Staff_shield" },             // Magic barrier
            new[] { RallyingEffect, "SE_Dvergr_buff" },             // Dvergr power (a damage buff)
            new[] { SkaldsEffect, "Potion_bzerker" },               // Berserkir Mead
            new[] { FenrirEffect, "SetEffect_FenringArmor" },       // Fenris blessing
            new[] { ForagingEffect, "SetEffect_HarvesterArmor" },   // Harvester (straw hat set)
            new[] { DeathlessEffect, "SoftDeath" }                  // No skill drain (after death)
        };

        private static readonly Dictionary<string, StatusEffect> Effects = new Dictionary<string, StatusEffect>();
        private static readonly HashSet<string> WildPlants = new HashSet<string>();
        private static readonly HashSet<HitData> ProcHits = new HashSet<HitData>();
        private static readonly ProcRules.TargetStacks Skadi = new ProcRules.TargetStacks();
        private static readonly List<Player> Nearby = new List<Player>();
        private static readonly List<Character> InRange = new List<Character>();
        private static readonly Func<double> Random01 = () => UnityEngine.Random.value * 0.99999f;
        private static bool s_iconsCopied;
        private static bool s_visualsFound;
        private static EffectList s_lightning;
        private static EffectList s_shockwave;

        private static int Hash(string name) => name.GetStableHashCode();

        // ---- Registration ----

        /// <summary>The status effects, registered on every game so allies' games know them. Register after ObjectDB is ready.</summary>
        public static void RegisterEffects()
        {
            Add(Make<SE_ProcArmour>(ShieldwallEffect, ProcRules.BuffSeconds));
            Add(Make<SE_ProcPercent>(RallyingEffect, ProcRules.BuffSeconds, e => e.m_consumeOnHit = true));
            Add(Make<SE_ProcPercent>(SkaldsEffect, ProcRules.BuffSeconds));
            Add(Make<SE_ProcStamina>(SteadfastEffect, 0f));
            Add(Make<SE_ProcStacks>(FenrirEffect, ProcRules.BuffSeconds, e => e.m_maxStacks = ProcRules.FenrirMaxStacks));
            Add(Make<StatusEffect>(ForagingEffect, ProcRules.ForagingSeconds));
            Add(Make<SE_DeathlessCooldown>(DeathlessEffect, 0f));
        }

        private static T Make<T>(string name, float ttl, Action<T> setup = null) where T : StatusEffect
        {
            var effect = ScriptableObject.CreateInstance<T>();
            effect.name = name;
            string key = "corruption_se_" + name.Substring("SE_Corruption".Length).ToLowerInvariant();
            effect.m_name = "$" + key;
            effect.m_tooltip = "$" + key + "_tooltip";
            effect.m_ttl = ttl;
            setup?.Invoke(effect);
            return effect;
        }

        private static void Add(StatusEffect effect)
        {
            Effects[effect.name] = effect;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));
        }

        /// <summary>
        ///     Wild plants for Foraging: pickables that grow back (<c>m_respawnTimeMinutes</c> above 0) and aren't crops
        ///     players grow (<c>Plant.m_grownPrefabs</c>). Read from the game's prefabs; the list goes to the log.
        /// </summary>
        public static void CollectWildPlants()
        {
            WildPlants.Clear();
            var crops = new HashSet<string>();
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(Plant)).Values)
            {
                var plant = obj as Plant;
                if (plant && plant.m_grownPrefabs != null)
                {
                    foreach (var grown in plant.m_grownPrefabs)
                    {
                        if (grown)
                        {
                            crops.Add(grown.name);
                        }
                    }
                }
            }
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(Pickable)).Values)
            {
                var pickable = obj as Pickable;
                if (pickable && pickable.m_respawnTimeMinutes > 0f && !crops.Contains(pickable.gameObject.name))
                {
                    WildPlants.Add(pickable.gameObject.name);
                }
            }
            Jotunn.Logger.LogInfo($"Foraging: {WildPlants.Count} wild plants (pickables that grow back, crops left out): " +
                                  string.Join(", ", WildPlants.OrderBy(n => n)));
        }

        /// <summary>
        ///     Icons from vanilla status effects, copied in a loaded world: the game's status effects aren't available at
        ///     startup (see <see cref="Maddened" />). Done before any of ours is added, so every copy has its icon.
        /// </summary>
        private static void CopyIcons()
        {
            if (s_iconsCopied || !ObjectDB.instance || ObjectDB.instance.m_StatusEffects == null)
            {
                return;
            }
            s_iconsCopied = true;
            foreach (var pair in IconSources)
            {
                var source = ObjectDB.instance.m_StatusEffects.FirstOrDefault(se => se && se.name == pair[1]);
                if (Effects.TryGetValue(pair[0], out var effect) && source)
                {
                    effect.m_icon = source.m_icon;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"Proc affixes: no icon for {pair[0]} ({pair[1]} not found).");
                }
            }
        }

        /// <summary>Reprisal's and Wrath's vanilla visuals, found the first time either is needed.</summary>
        private static void FindVisuals()
        {
            if (s_visualsFound || !ObjectDB.instance)
            {
                return;
            }
            s_visualsFound = true;
            var staff = ObjectDB.instance.GetItemPrefab(LightningSource)?.GetComponent<ItemDrop>();
            var bolt = staff ? staff.m_itemData.m_shared.m_attack?.m_attackProjectile : null;
            var projectile = bolt ? bolt.GetComponent<Projectile>() : null;
            s_lightning = projectile ? projectile.m_hitEffects : null;
            var sledge = ObjectDB.instance.GetItemPrefab(ShockwaveSource)?.GetComponent<ItemDrop>();
            var slam = sledge ? sledge.m_itemData.m_shared.m_attack : null;
            s_shockwave = slam == null ? null : slam.m_triggerEffect != null && slam.m_triggerEffect.HasEffects() ? slam.m_triggerEffect : slam.m_hitEffect;
            Jotunn.Logger.LogInfo($"Proc affixes: Reprisal shows {Describe(s_lightning)} (from {LightningSource}), " +
                                  $"Wrath shows {Describe(s_shockwave)} (from {ShockwaveSource}).");
        }

        private static string Describe(EffectList list)
        {
            if (list?.m_effectPrefabs == null || list.m_effectPrefabs.Length == 0)
            {
                return "nothing (not found)";
            }
            return string.Join(", ", list.m_effectPrefabs.Where(e => e != null && e.m_prefab).Select(e => e.m_prefab.name));
        }

        // ---- Shared helpers ----

        /// <summary>
        ///     Attack speed from parry and kill buffs (Skald's, Fenrir) on the local player, in percent. Swift's attack-speed
        ///     path adds it before the shared cap (affix_caps.csv → attack_speed: "Swift, Skald's and Fenrir together").
        /// </summary>
        public static float BuffAttackSpeedPercent(Player player)
        {
            var seman = player ? player.GetSEMan() : null;
            if (seman == null)
            {
                return 0f;
            }
            float percent = 0f;
            if (seman.GetStatusEffect(Hash(SkaldsEffect)) is SE_ProcPercent skalds)
            {
                percent += skalds.m_percent;
            }
            if (seman.GetStatusEffect(Hash(FenrirEffect)) is SE_ProcStacks fenrir)
            {
                percent += fenrir.Percent;
            }
            return percent;
        }

        /// <summary>A buff on every player within <see cref="ProcRules.AllyRange" /> (the parrier included), through the game's status-effect message.</summary>
        private static void BuffAround(Player player, string effect, float value)
        {
            if (value <= 0f)
            {
                return;
            }
            Nearby.Clear();
            Player.GetPlayersInRange(player.transform.position, ProcRules.AllyRange, Nearby);
            if (!Nearby.Contains(player))
            {
                Nearby.Add(player);
            }
            foreach (var ally in Nearby)
            {
                ally.GetSEMan()?.AddStatusEffect(Hash(effect), true, 0, value);
            }
        }

        /// <summary>
        ///     Sends a proc's own hit. Marked, so Skadi, Rallying and Wrath leave it alone (procs don't trigger procs), and
        ///     tagged with the item it comes from (Wrath: the weapon; Reprisal: the blocking item), whose knockback and life
        ///     steal affixes then apply to it.
        /// </summary>
        private static void ProcHit(Character target, HitData hit, ItemDrop.ItemData source)
        {
            HitSources.Tag(hit, source);
            ProcHits.Add(hit);
            try
            {
                target.Damage(hit);
            }
            finally
            {
                ProcHits.Remove(hit);
            }
        }

        private static HitData NewProcHit(Player player, Character target, Vector3 from)
        {
            var dir = target.transform.position - from;
            dir.y = 0f;
            var hit = new HitData
            {
                m_point = target.GetCenterPoint(),
                m_dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : player.transform.forward,
                m_hitType = HitData.HitType.PlayerHit,
                // Procs never backstab (the knockback/backstab prefix multiplies this, and 0 stays 0).
                m_backstabBonus = 0f
            };
            hit.SetAttacker(player);
            return hit;
        }

        // ---- Parry: Shieldwall, Rallying, Skald's, Steadfast, Reprisal ----

        private static readonly string[] ParryEffects =
            { "parry_buff_armour", "parry_buff_next_hit", "parry_buff_attack_speed", "parry_restore_stamina", "parry_lightning" };

        private static void OnParry(Player player, ItemDrop.ItemData blocker, Character attacker)
        {
            BuffAround(player, ShieldwallEffect, Loot.Total(blocker, "parry_buff_armour"));
            BuffAround(player, RallyingEffect, Loot.Total(blocker, "parry_buff_next_hit"));
            BuffAround(player, SkaldsEffect, Loot.Total(blocker, "parry_buff_attack_speed"));
            BuffAround(player, SteadfastEffect, Loot.Total(blocker, "parry_restore_stamina"));

            if (attacker && !attacker.IsDead() && GatheringRules.Roll(Loot.Total(blocker, "parry_lightning"), Random01))
            {
                FindVisuals();
                var hit = NewProcHit(player, attacker, player.transform.position);
                hit.m_damage.m_lightning = blocker.GetBaseBlockPower(blocker.m_quality) * ProcRules.ReprisalBlockShare;
                hit.m_skill = Skills.SkillType.Blocking;
                s_lightning?.Create(hit.m_point, Quaternion.identity, null, 1f, -1);
                ProcHit(attacker, hit, blocker);
            }
        }

        private sealed class ParryProbe
        {
            public ItemDrop.ItemData Blocker;
            public int StatusEffectHash;
            public bool Done;
        }

        /// <summary>
        ///     A parry is vanilla's perfect block: the blocking item has a parry bonus, the block started under 0.25 s
        ///     ago (<c>Humanoid.BlockAttack</c>'s own test) and the block held (stamina left, guard not broken), the case
        ///     where vanilla plays its perfect-block effect. The game keeps "held" in a local variable; the one visible
        ///     sign is that a held block clears the hit's status effect. So, for a hit in the parry window, the prefix
        ///     puts a marker there and the postfix checks whether vanilla cleared it, then puts the original back.
        /// </summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
        private static class Parry
        {
            private static readonly int Probe = "corruption_parry_probe".GetStableHashCode();

            [HarmonyPriority(Priority.Last)]
            private static void Prefix(Humanoid __instance, HitData hit, Character attacker, out ParryProbe __state)
            {
                __state = null;
                if (hit == null || !attacker || !(__instance is Player player) || player != Player.m_localPlayer)
                {
                    return;
                }
                var blocker = __instance.GetCurrentBlocker();
                if (blocker == null || !(blocker.m_shared.m_timedBlockBonus > 1f && __instance.m_blockTimer != -1f && __instance.m_blockTimer < 0.25f))
                {
                    return;
                }
                if (ParryEffects.All(effect => Loot.Total(blocker, effect) <= 0f))
                {
                    return;
                }
                __state = new ParryProbe { Blocker = blocker, StatusEffectHash = hit.m_statusEffectHash };
                hit.m_statusEffectHash = Probe;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(Humanoid __instance, HitData hit, Character attacker, ParryProbe __state)
            {
                if (__state == null || __state.Done)
                {
                    return;
                }
                __state.Done = true;
                bool held = hit.m_statusEffectHash == 0;
                if (!held)
                {
                    hit.m_statusEffectHash = __state.StatusEffectHash;
                    return;
                }
                var player = (Player)__instance;
                Safe.Run("apply parry affixes", () => OnParry(player, __state.Blocker, attacker));
            }

            /// <summary>Puts the hit's status effect back if the game method failed before the postfix.</summary>
            private static void Finalizer(HitData hit, ParryProbe __state)
            {
                if (__state != null && !__state.Done && hit != null && hit.m_statusEffectHash == Probe)
                {
                    hit.m_statusEffectHash = __state.StatusEffectHash;
                }
            }
        }

        // ---- The local player's hits: Rallying, Skadi, Wrath ----

        private static bool s_wrathSwing;

        /// <summary>Wrath: is this melee swing the last hit of the weapon's combo (vanilla's own test, which doubles that hit)?</summary>
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        private static class ComboSwing
        {
            private static void Prefix(Attack __instance)
            {
                s_wrathSwing = __instance.m_character == Player.m_localPlayer && __instance.m_weapon != null &&
                               __instance.m_attackChainLevels > 1 && __instance.m_currentAttackCainLevel == __instance.m_attackChainLevels - 1 &&
                               Loot.Total(__instance.m_weapon, "combo_shockwave") > 0f;
            }

            private static void Finalizer()
            {
                s_wrathSwing = false;
            }
        }

        private sealed class HitState
        {
            public HitData.DamageTypes Damage;
            public ItemDrop.ItemData Weapon;
        }

        /// <summary>
        ///     Runs before the knockback/backstab/life-steal prefix (higher priority), so life steal counts Rallying's and
        ///     Skadi's damage like any other. Changes the hit on the attacker's game, before it goes to the target's owner.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static class Hits
        {
            [HarmonyPriority(Priority.High)]
            private static void Prefix(Character __instance, HitData hit, out HitState __state)
            {
                __state = null;
                var player = Player.m_localPlayer;
                if (hit == null || !player || __instance == player || ProcHits.Contains(hit) || hit.GetAttacker() != player ||
                    !BaseAI.IsEnemy(player, __instance))
                {
                    return;
                }
                HitState state = null;
                Safe.Run("apply hit affixes", () =>
                {
                    // Rallying: the next hit, every target that swing or shot lands on (one frame).
                    if (player.GetSEMan().GetStatusEffect(Hash(RallyingEffect)) is SE_ProcPercent rally && rally.m_percent > 0f)
                    {
                        hit.m_damage.Modify(1f + rally.m_percent / 100f);
                        if (rally.m_usedFrame < 0)
                        {
                            rally.m_usedFrame = Time.frameCount;
                        }
                    }

                    // The item this hit comes from: an arrow's bow, even if the player has switched weapons since.
                    var weapon = HitSources.For(hit, player);
                    float skadi = Loot.Total(weapon, "frost_stacking");
                    if (skadi > 0f)
                    {
                        int stacks = Skadi.Hit(__instance, weapon, Time.time);
                        var d = hit.m_damage;
                        float frost = ProcRules.SkadiFrost(d.m_blunt + d.m_slash + d.m_pierce, skadi, stacks);
                        hit.m_damage.m_frost += frost;
                        // The game shows one number per hit (frost folded in) and Skadi's small frost slows only for a
                        // blink, so Skadi can't be seen in play. With devcommands on (testing), say what it added.
                        if (Terminal.m_cheat)
                        {
                            player.Message(MessageHud.MessageType.TopLeft,
                                $"Skadi: stack {stacks}, +{frost:0.#} frost ({Loot.ColoredName(weapon) ?? weapon.m_shared.m_name})");
                        }
                    }

                    if (s_wrathSwing)
                    {
                        s_wrathSwing = false;
                        state = new HitState { Damage = hit.m_damage, Weapon = weapon };
                    }
                });
                __state = state;
            }

            /// <summary>Wrath: the shockwave goes out once the combo's last hit has landed.</summary>
            private static void Postfix(Character __instance, HitData hit, HitState __state)
            {
                if (__state == null)
                {
                    return;
                }
                Safe.Run("send Wrath's shockwave", () => Shockwave(Player.m_localPlayer, __state.Weapon, __state.Damage, hit.m_point));
            }
        }

        /// <summary>
        ///     Wrath: every enemy within <see cref="ProcRules.WrathRadius" /> of the hit takes the rolled share of the hit's
        ///     damage, pushed away from it with the weapon's own knockback (the struck enemy too).
        /// </summary>
        private static void Shockwave(Player player, ItemDrop.ItemData weapon, HitData.DamageTypes damage, Vector3 center)
        {
            float percent = Loot.Total(weapon, "combo_shockwave");
            if (!player || weapon == null || percent <= 0f)
            {
                return;
            }
            FindVisuals();
            s_shockwave?.Create(center, Quaternion.identity, null, 1f, -1);
            damage.m_chop = 0f;
            damage.m_pickaxe = 0f;
            damage.Modify(percent / 100f);
            InRange.Clear();
            Character.GetCharactersInRange(center, ProcRules.WrathRadius, InRange);
            foreach (var target in InRange.ToList())
            {
                if (!target || target == player || target.IsDead() || !BaseAI.IsEnemy(player, target))
                {
                    continue;
                }
                var hit = NewProcHit(player, target, center);
                hit.m_damage = damage;
                hit.m_pushForce = weapon.m_shared.m_attackForce;
                hit.m_skill = weapon.m_shared.m_skillType;
                hit.m_blockable = weapon.m_shared.m_blockable;
                hit.m_dodgeable = weapon.m_shared.m_dodgeable;
                ProcHit(target, hit, weapon);
            }
        }

        // ---- Kills: Bloodforged, Fenrir ----

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpc
        {
            private static void Postfix()
            {
                Safe.Run("register the loot kill message", () => ZRoutedRpc.instance.Register<ZPackage>(RpcKill, RPC_Kill));
            }
        }

        /// <summary>
        ///     The creature's owner decides its death; it tells the killer's game (prefix: the creature's network object is
        ///     gone once <c>OnDeath</c> ends). An enemy is any creature that isn't a player or tamed, as for loot drops.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        private static class ReportKill
        {
            private static void Prefix(Character __instance)
            {
                Safe.Run("report a kill to the killer", () =>
                {
                    if (!__instance || __instance.IsPlayer() || __instance.IsTamed() || !__instance.m_nview || !__instance.m_nview.IsValid() ||
                        !__instance.m_nview.IsOwner() || ZRoutedRpc.instance == null)
                    {
                        return;
                    }
                    var killer = __instance.m_lastHit != null ? __instance.m_lastHit.GetAttacker() as Player : null;
                    var zdo = killer && killer.m_nview ? killer.m_nview.GetZDO() : null;
                    if (zdo == null)
                    {
                        return;
                    }
                    var package = new ZPackage();
                    package.Write(killer.GetZDOID());
                    ZRoutedRpc.instance.InvokeRoutedRPC(zdo.GetOwner(), RpcKill, package);
                });
            }
        }

        private static void RPC_Kill(long sender, ZPackage package)
        {
            Safe.Run("apply kill affixes", () =>
            {
                var killer = package.ReadZDOID();
                var player = Player.m_localPlayer;
                if (!player || player.GetZDOID() != killer || player.IsDead())
                {
                    return;
                }
                float heal = Loot.EquippedTotal(player, "heal_on_kill");
                if (heal > 0f)
                {
                    player.Heal(player.GetMaxHealth() * heal / 100f);
                }
                float speed = Loot.EquippedTotal(player, "attack_speed_on_kill");
                if (speed > 0f)
                {
                    player.GetSEMan().AddStatusEffect(Hash(FenrirEffect), true, 0, speed);
                }
            });
        }

        // ---- Deathless ----

        private static Character s_damaged;

        /// <summary>The shortest Deathless cooldown the local player wears, in seconds (0: none).</summary>
        private static double DeathlessCooldown(Player player)
        {
            return ProcRules.Lowest(Loot.Equipped(player), "cheat_death", Loot.Affixes) * 60.0;
        }

        private static double Now() => ZNet.instance ? ZNet.instance.GetTimeSeconds() : 0;

        private static double DeathlessUsedAt(Player player)
        {
            return player.m_customData != null && player.m_customData.TryGetValue(DeathlessKey, out var raw) &&
                   double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var at)
                ? at
                : 0;
        }

        /// <summary>Seconds left before Deathless can save the local player again (0: ready, or no Deathless worn).</summary>
        public static double DeathlessLeft(Player player)
        {
            if (!player || player != Player.m_localPlayer)
            {
                return 0;
            }
            return ProcRules.CooldownLeft(DeathlessUsedAt(player), Now(), DeathlessCooldown(player));
        }

        /// <summary>Marks damage being applied to the local player, so the health it sets can be checked.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
        private static class DamageScope
        {
            private static void Prefix(Character __instance, out Character __state)
            {
                __state = s_damaged;
                if (__instance && __instance == Player.m_localPlayer)
                {
                    s_damaged = __instance;
                }
            }

            private static void Finalizer(Character __state)
            {
                s_damaged = __state;
            }
        }

        /// <summary>
        ///     Deathless: damage that would bring the local player to 0 health leaves 1 instead, before the game's death
        ///     check (<c>Character.CheckDeath</c>, where EquipmentAndQuickSlots' keep-on-death starts) ever sees 0. The
        ///     same place vanilla's god mode keeps a player at 1 health.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.SetHealth))]
        private static class CheatDeath
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(Character __instance, ref float health)
            {
                if (health > 0f || s_damaged == null || __instance != s_damaged || __instance.IsDead())
                {
                    return;
                }
                try
                {
                    var player = (Player)__instance;
                    double cooldown = DeathlessCooldown(player);
                    if (cooldown <= 0 || ProcRules.CooldownLeft(DeathlessUsedAt(player), Now(), cooldown) > 0)
                    {
                        return;
                    }
                    health = 1f;
                    player.m_customData[DeathlessKey] = Now().ToString("R", CultureInfo.InvariantCulture);
                    player.Message(MessageHud.MessageType.Center, "$corruption_deathless_saved");
                    player.GetSEMan().AddStatusEffect(Hash(DeathlessEffect), true);
                }
                catch (Exception ex)
                {
                    Safe.Report("apply Deathless", ex);
                }
            }
        }

        /// <summary>After loading in: copy the icons, and show a Deathless cooldown still running from before.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        private static class Spawned
        {
            private static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                Safe.Run("set up the proc affix effects", () =>
                {
                    CopyIcons();
                    if (DeathlessLeft(__instance) > 0)
                    {
                        __instance.GetSEMan().AddStatusEffect(Hash(DeathlessEffect), true);
                    }
                });
            }
        }

        // ---- Foraging ----

        /// <summary>
        ///     Foraging: picking a wild plant starts (or restarts) the boost. PlantEasily's bulk harvest calls this for
        ///     every plant in the grid; each one restarts the same boost, so a bulk harvest counts as one pick.
        /// </summary>
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        private static class Pick
        {
            private static void Prefix(Pickable __instance, Humanoid character)
            {
                if (!(character is Player player) || player != Player.m_localPlayer || __instance.m_picked || __instance.m_enabled == 0 ||
                    !__instance.m_nview || !__instance.m_nview.IsValid())
                {
                    return;
                }
                Safe.Run("apply Foraging", () =>
                {
                    if (WildPlants.Contains(Utils.GetPrefabName(__instance.gameObject)) && Loot.EquippedTotal(player, "forage_speed_boost") > 0f)
                    {
                        player.GetSEMan().AddStatusEffect(Hash(ForagingEffect), true);
                    }
                });
            }
        }

        /// <summary>Foraging's speed, added to the gear movement modifier under the movement cap the other movement affixes share.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentMovementModifier))]
        private static class ForagingSpeed
        {
            [HarmonyPriority(Priority.Low)]
            private static void Postfix(Player __instance, ref float __result)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                var seman = __instance.GetSEMan();
                if (seman == null || !seman.HaveStatusEffect(Hash(ForagingEffect)))
                {
                    return;
                }
                var items = Loot.Equipped(__instance);
                float worn = AffixTotals.Sum(items, "movement_speed", Loot.Affixes) + AffixTotals.Sum(items, "held_movement_speed", Loot.Affixes);
                float extra = AffixTotals.Sum(items, "forage_speed_boost", Loot.Affixes);
                __result += ProcRules.ExtraUnderCap(worn, extra, "foraging", Loot.Caps) / 100f;
            }
        }

        // ---- co_affix (testing) ----

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new AffixCommand());
        }

        /// <summary>co_affix: adds one affix to an equipped item, to test a proc without rolling for it.</summary>
        private sealed class AffixCommand : ConsoleCommand
        {
            public override string Name => "co_affix";

            public override string Help =>
                "Corruption: co_affix <affix id> [value] [right|left|helmet|chest|legs|cape|utility] - add an affix from affixes.csv to an " +
                "equipped item (default: the best value, the item in your right hand, or left for a shield)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player || !Loot.Loaded || args.Length == 0 || !Loot.Affixes.TryGetValue(args[0], out var affix))
                {
                    Console.instance.Print(args.Length == 0 || !Loot.Loaded ? Help : $"No affix '{args[0]}' in affixes.csv.");
                    return;
                }
                float value = affix.LowerIsBetter ? affix.Min : affix.Max;
                if (args.Length > 1 && !LootItem.TryParseValue(args[1], out value))
                {
                    Console.instance.Print($"'{args[1]}' isn't a finite number.");
                    return;
                }
                string slot = args.Length > 2 ? args[2].ToLowerInvariant() : "";
                ItemDrop.ItemData item;
                switch (slot)
                {
                    case "left": item = player.m_leftItem; break;
                    case "helmet": item = player.m_helmetItem; break;
                    case "chest": item = player.m_chestItem; break;
                    case "legs": item = player.m_legItem; break;
                    case "cape": item = player.m_shoulderItem; break;
                    case "utility": item = player.m_utilityItem; break;
                    default: item = player.m_rightItem ?? player.m_leftItem; break;
                }
                if (item == null)
                {
                    Console.instance.Print("Nothing equipped there.");
                    return;
                }
                var loot = Loot.Of(item);
                if (loot == null && item.m_customData.ContainsKey(LootItem.AffixesKey))
                {
                    // Keep unreadable saved affixes recoverable rather than overwrite them.
                    Console.instance.Print($"{Loot.ColoredName(item)} has unreadable affix data; left unchanged (see the log).");
                    return;
                }
                loot = loot ?? new LootItem { Rarity = "magic" };
                loot.Lines.Add(new LootItem.Line { Ids = new[] { affix.Id }, Value = value });
                loot.Write(item.m_customData);
                Console.instance.Print($"Added {affix.Id} = {value.ToString(CultureInfo.InvariantCulture)} to {Loot.ColoredName(item)}.");
            }
        }
    }

    /// <summary>Shieldwall: +X% armour through the game's own armour multiplier (<c>SE_Stats.ModifyArmorMods</c>).</summary>
    internal class SE_ProcArmour : SE_Stats
    {
        /// <summary>The rolled value arrives as the skill level; a weaker parry refreshes the time but keeps the stronger value.</summary>
        public override void SetLevel(int itemLevel, float skillLevel)
        {
            base.SetLevel(itemLevel, skillLevel);
            m_armorMultiplier = Mathf.Max(m_armorMultiplier, skillLevel / 100f);
        }
    }

    /// <summary>Rallying (+X% damage, ends after the next hit) and Skald's (+X% attack speed).</summary>
    internal class SE_ProcPercent : StatusEffect
    {
        public bool m_consumeOnHit;
        public float m_percent;

        /// <summary>The frame of the hit that used Rallying up (-1: not used); every hit in that frame counts, then it ends.</summary>
        public int m_usedFrame = -1;

        public override void SetLevel(int itemLevel, float skillLevel)
        {
            base.SetLevel(itemLevel, skillLevel);
            m_percent = Mathf.Max(m_percent, skillLevel);
            m_usedFrame = -1;
        }

        public override bool IsDone()
        {
            return base.IsDone() || (m_consumeOnHit && m_usedFrame >= 0 && Time.frameCount != m_usedFrame);
        }
    }

    /// <summary>Steadfast: restores stamina once, on the receiving player's own game, then ends.</summary>
    internal class SE_ProcStamina : StatusEffect
    {
        public override void SetLevel(int itemLevel, float skillLevel)
        {
            base.SetLevel(itemLevel, skillLevel);
            if (skillLevel > 0f && m_character)
            {
                m_character.AddStamina(skillLevel);
            }
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    /// <summary>Fenrir: each kill adds a stack (up to the cap) and restarts the time; every stack gives the latest kill's value.</summary>
    internal class SE_ProcStacks : StatusEffect
    {
        public int m_maxStacks = 1;
        public int m_stacks;
        public float m_percentPerStack;

        public float Percent => m_stacks * m_percentPerStack;

        public override void SetLevel(int itemLevel, float skillLevel)
        {
            base.SetLevel(itemLevel, skillLevel);
            m_stacks = ProcRules.AddStack(m_stacks, m_maxStacks);
            m_percentPerStack = skillLevel;
        }

        public override string GetIconText()
        {
            string time = base.GetIconText();
            return m_stacks > 1 ? "x" + m_stacks + " " + time : time;
        }
    }

    /// <summary>Deathless's cooldown on the HUD: runs while <see cref="LootProcs.DeathlessLeft" /> is above 0.</summary>
    internal class SE_DeathlessCooldown : StatusEffect
    {
        public override bool IsDone()
        {
            return LootProcs.DeathlessLeft(m_character as Player) <= 0;
        }

        public override string GetIconText()
        {
            return GetTimeString((float)LootProcs.DeathlessLeft(m_character as Player));
        }
    }
}
