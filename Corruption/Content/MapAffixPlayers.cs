using System.Linq;
using System.Text;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The map affixes that act on players near a live rip heart (Krogon, 2026-11-27, decision 8: "Sure use the 96m"):
    ///     Withering (health lost each second), Exhausting (stamina lost each second) and Exposed (less resistance). Each
    ///     player's own game checks, for its own player only (nothing runs on a dedicated server, which has no local
    ///     player), whether a live heart stands within <see cref="Range" /> metres, reads the heart's stamp, and shows a
    ///     status effect (<see cref="SE_RipCurse" />) while any of them applies. The status effect also does the work: it
    ///     lives on the local player and is updated every frame by the game.
    ///     Cost: nothing but a timer while no heart is loaded; every 0.5 s a pass over the loaded hearts (a static list,
    ///     normally empty or one entry; never a scene search); one stamp parse when the heart's stamp changes.
    /// </summary>
    internal static class RipCurse
    {
        /// <summary>
        ///     Metres (ground distance to the heart) within which the affixes act: the waves' range, the army raids'
        ///     <c>m_eventRange</c> 96 in <c>data/game-dump-1.0.31/events.csv</c> (Krogon, rips round decision 8).
        /// </summary>
        public const float Range = 96f;

        private const float ScanInterval = 0.5f;
        public const string EffectName = "SE_CorruptionRipCurse";
        private const string IconSource = "Poison";   // the vanilla poison effect's icon

        private static readonly int EffectHash = EffectName.GetStableHashCode();
        private static StatusEffect s_effect;
        private static bool s_iconCopied;
        private static Player s_player;
        private static float s_timer;
        private static string s_raw;

        /// <summary>Health points lost each second, 0 when none applies.</summary>
        public static float LifeDrain { get; private set; }

        /// <summary>Stamina points lost each second, 0 when none applies.</summary>
        public static float StaminaDrain { get; private set; }

        /// <summary>Percentage points taken off the player's gear resistances to fire, frost, lightning and poison, 0 when none applies.</summary>
        public static float ResistancePenalty { get; private set; }

        public static bool Active => LifeDrain > 0f || StaminaDrain > 0f || ResistancePenalty > 0f;

        /// <summary>Registers the status effect on every game. Register after ObjectDB is ready (the plugin's content step).</summary>
        public static void RegisterEffect()
        {
            var effect = ScriptableObject.CreateInstance<SE_RipCurse>();
            effect.name = EffectName;
            effect.m_name = "$corruption_se_ripcurse";
            effect.m_tooltip = "$corruption_se_ripcurse_tooltip";
            effect.m_ttl = 0f;   // lasts while the heart's affixes apply (SE_RipCurse.IsDone)
            s_effect = effect;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));
        }

        /// <summary>The vanilla icon, copied in a loaded world: the game's status effects aren't available at startup (see <see cref="Maddened" />).</summary>
        private static void CopyIcon()
        {
            if (s_iconCopied || !s_effect || !ObjectDB.instance || ObjectDB.instance.m_StatusEffects == null)
            {
                return;
            }
            s_iconCopied = true;
            var source = ObjectDB.instance.m_StatusEffects.FirstOrDefault(se => se && se.name == IconSource);
            if (source)
            {
                s_effect.m_icon = source.m_icon;
            }
            else
            {
                Jotunn.Logger.LogWarning($"Rip curse: no icon ({IconSource} status effect not found).");
            }
        }

        private static void Clear()
        {
            s_raw = null;
            LifeDrain = 0f;
            StaminaDrain = 0f;
            ResistancePenalty = 0f;
        }

        /// <summary>Called every fixed update for the local player: every <see cref="ScanInterval" /> seconds, looks for a heart and adds or updates the effect.</summary>
        public static void Update(Player player, float dt)
        {
            if (!ReferenceEquals(player, s_player))
            {
                s_player = player;   // a new character (or world): nothing is known yet
                Clear();
                s_timer = 0f;
            }
            s_timer -= dt;
            if (s_timer > 0f)
            {
                return;
            }
            s_timer = ScanInterval;
            if (!MapTiers.Loaded || player.IsDead())
            {
                Clear();
                return;
            }
            var heart = RipHeart.NearestData(player.transform.position, Range);
            string raw = heart != null ? heart.GetString(MapTiers.KeyAffixes, "") : null;
            if (!string.Equals(raw, s_raw))
            {
                s_raw = raw;
                var rolls = raw != null ? MapTiers.RollsOf(raw) : null;
                LifeDrain = rolls != null ? (float)RipTierRules.LifeDrainPerSecond(rolls) : 0f;
                StaminaDrain = rolls != null ? (float)RipTierRules.StaminaDrainPerSecond(rolls) : 0f;
                ResistancePenalty = rolls != null ? (float)RipTierRules.ResistancePenaltyPercent(rolls) : 0f;
            }
            var seman = player.GetSEMan();
            if (Active && seman != null && !seman.HaveStatusEffect(EffectHash))
            {
                CopyIcon();
                seman.AddStatusEffect(EffectHash);
            }
        }

        /// <summary>
        ///     Withering and Exhausting, from the status effect's own update (every frame while it is on the local player).
        ///     Withering: every second a hit of <see cref="LifeDrain" /> damage, made the way the game's own health tick
        ///     makes it (SE_Stats.cs:222-229: a HitData with m_damage.m_damage, a point, a hit type, Character.Damage).
        ///     Exhausting: <see cref="StaminaDrain" /> x dt taken straight from the stamina, not through
        ///     <c>Player.UseStamina</c> / <c>RPC_UseStamina</c>, which also restart the regeneration delay (Player.cs:4665):
        ///     the drain works against the regeneration instead of stopping it.
        /// </summary>
        public static void Tick(Player player, float dt, ref float lifeTimer)
        {
            if (!player || player != Player.m_localPlayer || player.IsDead() || player.InIntro() || player.IsTeleporting())
            {
                return;
            }
            if (StaminaDrain > 0f && player.m_stamina > 0f)
            {
                player.m_stamina = Mathf.Max(0f, player.m_stamina - StaminaDrain * dt);
            }
            if (LifeDrain > 0f)
            {
                lifeTimer += dt;
                if (lifeTimer >= 1f)
                {
                    lifeTimer -= 1f;
                    var hit = new HitData();
                    hit.m_damage.m_damage = LifeDrain;
                    hit.m_point = player.GetTopPoint();
                    hit.m_hitType = HitData.HitType.Undefined;
                    player.Damage(hit);
                }
            }
        }

        /// <summary>The status effect's tooltip: what applies right now, with the numbers.</summary>
        public static string Tooltip()
        {
            var loc = Localization.instance;
            var text = new StringBuilder(loc.Localize("$corruption_se_ripcurse_tooltip"));
            if (LifeDrain > 0f)
            {
                text.Append('\n').Append(loc.Localize("$corruption_se_ripcurse_life", MapData.FormatValue(LifeDrain)));
            }
            if (StaminaDrain > 0f)
            {
                text.Append('\n').Append(loc.Localize("$corruption_se_ripcurse_stamina", MapData.FormatValue(StaminaDrain)));
            }
            if (ResistancePenalty > 0f)
            {
                text.Append('\n').Append(loc.Localize("$corruption_se_ripcurse_resist", MapData.FormatValue(ResistancePenalty)));
            }
            return text.ToString();
        }

        /// <summary>Finds the local player's fixed update (Player.cs:839: its owner's game only) and runs the scan.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateStats), typeof(float))]
        private static class Scan
        {
            private static void Postfix(Player __instance, float dt)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                try
                {
                    Update(__instance, dt);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("look for a rip heart's curse", ex);
                }
            }
        }
    }

    /// <summary>
    ///     The "Rip curse" status effect on the local player while a rip heart's Withering, Exhausting or Exposed applies.
    ///     It ends itself when none does (<see cref="RipCurse.Active" />, refreshed twice a second).
    /// </summary>
    internal class SE_RipCurse : StatusEffect
    {
        private float m_lifeTimer;

        public override bool IsDone()
        {
            return !RipCurse.Active;
        }

        public override string GetTooltipString()
        {
            return RipCurse.Tooltip();
        }

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            try
            {
                RipCurse.Tick(m_character as Player, dt, ref m_lifeTimer);
            }
            catch (System.Exception ex)
            {
                Safe.Report("drain a player near a rip heart", ex);
            }
        }
    }
}
