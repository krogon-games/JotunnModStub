using System;
using System.Collections.Generic;
using System.Globalization;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The corrupted wishbone (user, 2026-10-29): a utility item crafted at the Riftforge from a Wishbone and 1 shard
    ///     (like every corrupted item: the normal item plus 1 shard). Worn, it pings like the vanilla Wishbone, but
    ///     towards the heart of the rip you came through, in red and purple, faster the closer you get. Everyone nearby
    ///     sees and hears the pings (user). Register after the Riftforge and the shard.
    /// </summary>
    internal static class CorruptedWishbone
    {
        public const string ItemPrefab = "CorruptedWishbone";
        public const string StatusEffectName = "SE_CorruptedWishbone";
        private const string VanillaItem = "Wishbone";

        private static readonly Color Red = new Color(0.7f, 0.09f, 0.05f);
        private static readonly Color Purple = new Color(0.45f, 0.1f, 0.65f);
        // Mostly red with bits of purple (user). An even mix looked mostly purple in the 2026-10-29 play-test, as
        // purple reads brighter; now only the last fifth of the colour range shades into purple.
        private const float PurpleShare = 0.2f;
        // The near, medium and far lists can share effects; each is cloned once.
        private static readonly Dictionary<string, GameObject> Clones = new Dictionary<string, GameObject>();

        public static void Register()
        {
            var vanilla = PrefabManager.Instance.GetPrefab(VanillaItem);
            var finder = vanilla ? vanilla.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_equipStatusEffect as SE_Finder : null;

            var effect = ScriptableObject.CreateInstance<SE_RipFinder>();
            effect.name = StatusEffectName;
            effect.m_name = "$se_corrupted_wishbone";
            effect.m_tooltip = "$se_corrupted_wishbone_tooltip";
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = "$se_corrupted_wishbone_start";
            if (finder)
            {
                // The vanilla Wishbone's pacing (0.5 s up close, 5 s far away in the 1.0.20 dump) and its pings, recoloured.
                effect.m_icon = finder.m_icon;
                effect.m_closeFrequency = finder.m_closeFrequency;
                effect.m_distantFrequency = finder.m_distantFrequency;
                RipCompass.SetEffects(Recolor(finder.m_pingEffectNear), Recolor(finder.m_pingEffectMed), Recolor(finder.m_pingEffectFar));
                Jotunn.Logger.LogInfo($"Corrupted wishbone: pings every {effect.m_closeFrequency}-{effect.m_distantFrequency} s " +
                                      $"(from the vanilla Wishbone), shared with other players by {(RipCompass.EffectsAreNetworked ? "the game" : "our message")}.");
            }
            else
            {
                Jotunn.Logger.LogWarning("Corrupted wishbone: the vanilla Wishbone's finder effect wasn't found; it won't ping.");
            }
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));

            var config = new ItemConfig
            {
                Name = "$item_corruptedwishbone",
                Description = "$item_corruptedwishbone_description",
                CraftingStation = Texts.RiftforgePrefab,
                Amount = 1
            };
            config.AddRequirement(VanillaItem, 1);
            config.AddRequirement(Texts.ShardPrefab, 1);
            var item = new CustomItem(ItemPrefab, VanillaItem, config);
            Corrupted.Tint(item.ItemPrefab, Red, 0.6f);
            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_equipStatusEffect = effect;
            Safe.Run("render the corrupted wishbone icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                    effect.m_icon = icon;
                }
            });
            ItemManager.Instance.AddItem(item);
        }

        /// <summary>A copy of an effect list with red-and-purple clones of its effects.</summary>
        private static EffectList Recolor(EffectList list)
        {
            var copy = new EffectList();
            if (list?.m_effectPrefabs == null)
            {
                return copy;
            }
            var memberwiseClone = AccessTools.Method(typeof(object), "MemberwiseClone");
            var effects = new List<EffectList.EffectData>();
            foreach (var data in list.m_effectPrefabs)
            {
                if (data == null || !data.m_prefab)
                {
                    continue;
                }
                var clone = (EffectList.EffectData)memberwiseClone.Invoke(data, null);
                if (!Clones.TryGetValue(data.m_prefab.name, out var prefab))
                {
                    prefab = PrefabManager.Instance.CreateClonedPrefab("corruption_" + data.m_prefab.name, data.m_prefab);
                    Corrupted.TintTwo(prefab, Red, Purple, PurpleShare);
                    PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, false));
                    Clones[data.m_prefab.name] = prefab;
                }
                clone.m_prefab = prefab;
                effects.Add(clone);
            }
            copy.m_effectPrefabs = effects.ToArray();
            return copy;
        }
    }

    /// <summary>
    ///     Where the heart of the player's rip is, and the pings towards it. Entering a rip remembers its heart in the
    ///     player's own saved data, so it survives logging out inside the rip; it's forgotten when the heart is gone,
    ///     the rip closes, or the player goes home.
    /// </summary>
    internal static class RipCompass
    {
        private const string DataKey = "corruption_rip_heart";
        private const string RpcPing = "Corruption_RipPing";
        // Checks (one a second) the heart's loaded area must be without it before it counts as destroyed:
        // objects can arrive from the server a moment after their area has loaded.
        private const int MissingChecks = 5;

        private static readonly EffectList[] Effects = { new EffectList(), new EffectList(), new EffectList() };
        private static int s_missing;

        /// <summary>The ping effects have their own network object: the game shows them to everyone already.</summary>
        public static bool EffectsAreNetworked { get; private set; }

        public static void SetEffects(EffectList near, EffectList medium, EffectList far)
        {
            Effects[0] = near;
            Effects[1] = medium;
            Effects[2] = far;
            EffectsAreNetworked = false;
            foreach (var list in Effects)
            {
                foreach (var data in list.m_effectPrefabs)
                {
                    EffectsAreNetworked |= data.m_prefab && data.m_prefab.GetComponent<ZNetView>();
                }
            }
        }

        public static void Remember(Player player, Vector3 heart, long closeMs)
        {
            player.m_customData[DataKey] = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}", heart.x, heart.z, closeMs);
            s_missing = 0;
        }

        public static void Forget(Player player)
        {
            player.m_customData.Remove(DataKey);
        }

        /// <summary>The remembered heart, while it still stands. Call about once a second.</summary>
        public static bool TryGetHeart(Player player, out Vector3 heart)
        {
            heart = Vector3.zero;
            if (!player.m_customData.TryGetValue(DataKey, out string text))
            {
                return false;
            }
            var parts = text.Split(';');
            if (parts.Length != 3 ||
                !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float z) ||
                !long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long closeMs))
            {
                Forget(player);
                return false;
            }
            if (closeMs > 0 && ZNet.instance && ZNet.instance.GetTimeSeconds() * 1000.0 > closeMs)
            {
                Forget(player);
                return false;
            }
            heart = new Vector3(x, player.transform.position.y, z);
            // Once the heart's area has loaded, the heart is there unless it was destroyed.
            if (ZNetScene.instance && ZNetScene.instance.IsAreaReady(heart))
            {
                if (RipHeart.IsNear(heart, 2f))
                {
                    s_missing = 0;
                }
                else if (++s_missing >= MissingChecks)
                {
                    Forget(player);
                    return false;
                }
            }
            return true;
        }

        /// <summary>A ping on the player: 0 near, 1 medium, 2 far. Everyone who has the player loaded sees it.</summary>
        public static void Ping(Player player, int size)
        {
            if (EffectsAreNetworked)
            {
                Create(player.transform, size, player.GetZDOID());
            }
            else
            {
                // Sent to everybody, this game included.
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcPing, player.GetZDOID(), size);
            }
        }

        private static void Create(Transform on, int size, ZDOID owner)
        {
            Effects[Mathf.Clamp(size, 0, 2)].Create(on.position, on.rotation, on, 1f, -1, owner);
        }

        private static void RPC_Ping(long sender, ZDOID player, int size)
        {
            Safe.Run("show a wishbone ping", () =>
            {
                var instance = ZNetScene.instance ? ZNetScene.instance.FindInstance(player) : null;
                if (instance)
                {
                    Create(instance.transform, size, Player.m_localPlayer && Player.m_localPlayer.GetZDOID() == player ? player : ZDOID.None);
                }
            });
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpc
        {
            private static void Postfix()
            {
                Safe.Run("register the wishbone ping", () => ZRoutedRpc.instance.Register<ZDOID, int>(RpcPing, RPC_Ping));
            }
        }
    }

    /// <summary>
    ///     The corrupted wishbone's effect: <see cref="SE_Finder" />'s pacing and ping sizes, aimed at the rip's heart
    ///     instead of the nearest beacon. Pings when within <see cref="RipRules.HeartMaxDistance" /> of the heart (the
    ///     farthest a player lands from it), like the vanilla Wishbone within a beacon's range.
    /// </summary>
    internal class SE_RipFinder : StatusEffect
    {
        public float m_closeFrequency = 0.5f;
        public float m_distantFrequency = 5f;

        private float m_checkTimer = 1f;
        private float m_pingTimer;
        private bool m_hasHeart;
        private Vector3 m_heart;

        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            var player = m_character as Player;
            if (!player || player != Player.m_localPlayer)
            {
                return;
            }
            try
            {
                m_checkTimer += dt;
                if (m_checkTimer >= 1f)
                {
                    m_checkTimer = 0f;
                    m_hasHeart = RipCompass.TryGetHeart(player, out m_heart);
                }
                if (!m_hasHeart)
                {
                    return;
                }
                float t = Utils.DistanceXZ(player.transform.position, m_heart) / RipRules.HeartMaxDistance;
                if (t > 1f)
                {
                    m_pingTimer = 0f;
                    return;
                }
                m_pingTimer += dt;
                if (m_pingTimer > Mathf.Lerp(m_closeFrequency, m_distantFrequency, t))
                {
                    m_pingTimer = 0f;
                    // The vanilla split: near under 20% of the range, medium under 60%, far beyond.
                    RipCompass.Ping(player, t < 0.2f ? 0 : t < 0.6f ? 1 : 2);
                }
            }
            catch (Exception ex)
            {
                Safe.Report("ping towards the rip heart", ex);
            }
        }
    }
}
