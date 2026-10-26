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

        public static void Register()
        {
            CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
            localization.AddTranslation("English", new System.Collections.Generic.Dictionary<string, string>
            {
                { "item_corruptionshard", "Corruption shard" },
                { "item_corruptionshard_description", "A splinter of Ginnungagap, the void before the world. It hums against your palm." },
                { "piece_riftforge", "Riftforge" },
                { "piece_riftforge_description", "A forge that works the void itself. Here maps into the unknown are made." }
            });
        }
    }
}
