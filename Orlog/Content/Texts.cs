using System.Collections.Generic;
using Jotunn.Managers;

namespace Orlog.Content
{
    /// <summary>English text for everything the mod shows. Values are filled in with $1, $2... (Localization.InsertWords).</summary>
    internal static class Texts
    {
        public const string LevelUp = "$orlog_levelup";
        public const string XpGain = "$orlog_xp_gain";

        public static void Register()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "orlog_levelup", "Level $1" },
                { "orlog_xp_gain", "+$1 XP" }
            });
        }
    }
}
