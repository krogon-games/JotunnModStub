using Jotunn.Entities;
using Jotunn.Managers;

namespace Corruption.Content
{
    /// <summary>Prefab names and English text for everything the mod adds.</summary>
    internal static class Texts
    {
        public const string ShardPrefab = "CorruptionShard";
        public const string RiftforgePrefab = "piece_riftforge";

        public const string ShardName = "$item_corruptionshard";
        public const string ShardDescription = "$item_corruptionshard_description";
        public const string RiftforgeName = "$piece_riftforge";
        public const string RiftforgeDescription = "$piece_riftforge_description";
        public const string TalismanSubtitle = "$corruption_talisman_subtitle";
        public const string RavenTopic = "$raven_riftforge_topic";
        public const string RavenLabel = "$raven_riftforge_label";
        public const string RavenText = "$raven_riftforge_text";

        public static void Register()
        {
            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddTranslation("English", new System.Collections.Generic.Dictionary<string, string>
            {
                { "item_corruptionshard", "Corruption shard" },
                { "item_corruptionshard_description", "A splinter of Ginnungagap, the void before the world. It hums against your palm." },
                { "piece_riftforge", "Riftforge" },
                { "corruption_slot_talisman", "Talisman" },
                { "corruption_talisman_subtitle", "Talisman: place in a Talisman slot" },
                { "piece_riftforge_description", "A forge that works the void itself. Here maps into the unknown are made." },
                { "raven_riftforge_topic", "The Riftforge" },
                { "raven_riftforge_label", "Riftforge" },
                { "raven_riftforge_text", "You have built a forge for the void itself. The shards you carry are splinters of Ginnungagap, the yawning gap that was before the world. Bind them with a slain guardian's essence, and the Riftforge will tear open a path to lands you have never seen. Go prepared, warrior. The way back must be earned." }
            });
        }
    }
}
