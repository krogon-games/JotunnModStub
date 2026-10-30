using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The local player's XP and level. XP is saved in the character's custom data (<c>orlog_xp</c>), so it stays with
    ///     the character across worlds, like vanilla skills; the level is worked out from it, so a changed MaxLevel or level
    ///     table applies at once. The level is also published on the player's ZDO (<c>orlog_level</c>) so every game can
    ///     see it (XP sharing needs everyone's level).
    /// </summary>
    internal static class Progress
    {
        public const string XpKey = "orlog_xp";
        public const string LevelKey = "orlog_level";
        private const string ResourcePrefix = "Orlog.Data.";

        public static LevelTable Levels { get; private set; }
        public static CreatureXpTable CreatureXp { get; private set; }
        public static BiomeLevels Biomes { get; private set; }

        /// <summary>Goes up whenever something the UI shows changes (read by <see cref="Api.GetChangeCounter" />).</summary>
        public static int ChangeCounter { get; private set; }

        public static bool Ready => Levels != null && CreatureXp != null && Biomes != null;

        public static void LoadTables()
        {
            var errors = new List<string>();
            Levels = LevelTable.Parse(ReadData("levels.csv"), errors);
            CreatureXp = CreatureXpTable.Parse(ReadData("creature_xp.csv"), errors);
            Biomes = BiomeLevels.Parse(ReadData("biome_levels.csv"), errors);
            foreach (var error in errors)
            {
                Jotunn.Logger.LogError("Orlog data: " + error);
            }
        }

        private static string ReadData(string file)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + file))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogError($"Embedded data table {file} not found.");
                    return "";
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static int MaxLevel => Mathf.Clamp(OrlogConfig.MaxLevel.Value, 1, Levels?.TopLevel ?? 1);

        public static long GetXp(Player player)
        {
            if (player && player.m_customData.TryGetValue(XpKey, out string text) &&
                long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long xp))
            {
                return xp < 0 ? 0 : xp;
            }
            return 0;
        }

        public static int GetLevel(Player player)
        {
            return Ready ? Levels.LevelFor(GetXp(player), MaxLevel) : 1;
        }

        /// <summary>Any player's level: the local player's from its XP, others' from what their game published.</summary>
        public static int LevelOf(Player player)
        {
            if (!player)
            {
                return 1;
            }
            if (player == Player.m_localPlayer)
            {
                return GetLevel(player);
            }
            var zdo = player.m_nview ? player.m_nview.GetZDO() : null;
            return zdo != null ? Mathf.Max(1, zdo.GetInt(LevelKey, 1)) : 1;
        }

        /// <summary>Adds XP to the local player (capped at the max level), with the level-up message and effect.</summary>
        public static void AddXp(Player player, long amount)
        {
            if (!player || player != Player.m_localPlayer || amount <= 0 || !Ready)
            {
                return;
            }
            int before = GetLevel(player);
            long cap = Levels.TotalFor(MaxLevel);
            long xp = GetXp(player);
            if (xp >= cap)
            {
                return;
            }
            SetXp(player, System.Math.Min(cap, xp + amount));
            if (OrlogConfig.ShowXpGains.Value)
            {
                player.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize(Texts.XpGain, amount.ToString(CultureInfo.InvariantCulture)));
            }
            int after = GetLevel(player);
            if (after > before)
            {
                OnLevelUp(player, after);
            }
        }

        public static void SetXp(Player player, long xp)
        {
            player.m_customData[XpKey] = System.Math.Max(0, xp).ToString(CultureInfo.InvariantCulture);
            Publish(player);
            ChangeCounter++;
        }

        private static void OnLevelUp(Player player, int level)
        {
            player.Message(MessageHud.MessageType.Center, Localization.instance.Localize(Texts.LevelUp, level.ToString(CultureInfo.InvariantCulture)));
            // The vanilla skill level-up effect.
            Transform head = player.m_head ? player.m_head : player.transform;
            player.m_skillLevelupEffects.Create(head.position, head.rotation, head);
            Jotunn.Logger.LogInfo($"Reached level {level}.");
        }

        /// <summary>Puts the local player's level on its ZDO for the other games.</summary>
        public static void Publish(Player player)
        {
            var zdo = player && player.m_nview ? player.m_nview.GetZDO() : null;
            if (zdo != null && player.m_nview.IsOwner())
            {
                zdo.Set(LevelKey, GetLevel(player));
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        private static class PublishOnSpawn
        {
            private static void Postfix(Player __instance)
            {
                Safe.Run("publish the player's level", () =>
                {
                    if (__instance == Player.m_localPlayer)
                    {
                        Publish(__instance);
                        ChangeCounter++;
                    }
                });
            }
        }
    }
}
