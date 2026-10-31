using System.Collections.Generic;
using Jotunn.Managers;

namespace Orlog.Content
{
    /// <summary>English text for everything the mod shows. Values are filled in with $1, $2... (Localization.InsertWords).</summary>
    internal static class Texts
    {
        public const string LevelUp = "$orlog_levelup";
        public const string XpGain = "$orlog_xp_gain";
        public const string StoneName = "$orlog_stone";
        public const string StoneUse = "$orlog_stone_use";
        public const string StoneHasClass = "$orlog_stone_has_class";
        public const string StoneConfirm = "$orlog_stone_confirm";
        public const string StoneNeedsUi = "$orlog_stone_needs_ui";
        public const string ClassChosen = "$orlog_class_chosen";
        public const string RavenTopic = "$orlog_raven_topic";
        public const string RavenLabel = "$orlog_raven_label";
        public const string RavenText = "$orlog_raven_text";

        public static void Register()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "orlog_levelup", "Level $1" },
                { "orlog_xp_gain", "+$1 XP" },
                { "orlog_stone", "Orlog stone" },
                { "orlog_stone_use", "Choose your path" },
                { "orlog_stone_has_class", "You walk the path of the $1" },
                { "orlog_stone_confirm", "Use the stone again to walk the path of the $1" },
                { "orlog_stone_needs_ui", "The stone's choices need the classes screens (CraftingOverhaul)" },
                { "orlog_class_chosen", "You walk the path of the $1" },
                { "orlog_raven_topic", "The Orlog stone" },
                { "orlog_raven_label", "Orlog" },
                // Örlög: the fate the Norns lay down for every life.
                { "orlog_raven_text", "The Norns weave a fate for every warrior. Lay your hand on the Orlog stone and choose the path you will walk." }
            });
        }
    }
}
