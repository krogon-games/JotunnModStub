using Jotunn.Entities;
using Jotunn.Managers;

namespace Corruption.Content
{
    /// <summary>Prefab names and English text for everything the mod adds.</summary>
    internal static class Texts
    {
        public const string ShardPrefab = "CorruptionShard";
        public const string RiftforgePrefab = "piece_riftforge";
        public const string RipPrefab = "corruption_rip";
        public const string RipHeartPrefab = "corruption_rip_heart";

        public const string ShardName = "$item_corruptionshard";
        public const string ShardDescription = "$item_corruptionshard_description";
        public const string RiftforgeName = "$piece_riftforge";
        public const string RiftforgeDescription = "$piece_riftforge_description";
        public const string TalismanSubtitle = "$corruption_talisman_subtitle";
        public const string RavenTopic = "$raven_riftforge_topic";
        public const string RavenLabel = "$raven_riftforge_label";
        public const string RavenText = "$raven_riftforge_text";
        public const string MapSubtitle = "$corruption_map_subtitle";

        // Values are filled in with $1, $2... (Localization.InsertWords), not {0}.
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
                { "corruption_map_subtitle", "Map: use it to open a rip" },
                { "corruption_rip", "Rip" },
                { "corruption_rip_heart", "Rip heart" },
                { "corruption_maddened", "Maddened" },
                { "corruption_maddened_tooltip", "Driven mad by the rip: hits harder, moves faster, never flees." },
                { "corruption_rip_enter", "Enter" },
                { "corruption_rip_to", "Leads into the corrupted $1" },
                { "corruption_rip_home", "Leads home" },
                { "corruption_rip_closes", "Closes in $1" },
                { "corruption_rip_heart_hint", "Destroy it to open the way home" },
                { "corruption_offering_mixed", "Every offering here must be corrupted, or none." },
                { "item_corruptedwishbone", "Corrupted wishbone" },
                { "item_corruptedwishbone_description", "A wishbone steeped in the void. Inside a rip it tugs towards the rip's heart, pulsing faster the closer you get." },
                { "se_corrupted_wishbone", "Corrupted wishbone" },
                { "se_corrupted_wishbone_tooltip", "Pulses towards the heart of the rip you came through, faster the closer you get." },
                { "se_corrupted_wishbone_start", "The wishbone strains towards the rip's heart" },
                { "corruption_rip_closed", "The rip has closed." },
                { "corruption_rip_searching", "The map stirs, searching for unexplored land..." },
                { "corruption_rip_busy", "The map is still searching." },
                { "corruption_rip_disabled", "Rips are turned off on this server." },
                { "corruption_rip_not_here", "The map won't open a rip here." },
                { "corruption_rip_no_land", "There is no unexplored $1 left for this map to reach." },
                { "corruption_rip_only_bases", "Every unexplored $1 the map found is too close to someone's base. Try again." },
                { "corruption_rip_no_answer", "The map falls silent. Try again." },
                { "corruption_rip_opened", "A rip tears open into the corrupted $1." },
                { "corruption_rip_home_opened", "The heart shatters. A way home opens." },
                { "raven_riftforge_topic", "The Riftforge" },
                { "raven_riftforge_label", "Riftforge" },
                { "raven_riftforge_text", "You have built a forge for the void itself. The shards you carry are splinters of Ginnungagap, the yawning gap that was before the world. Bind them with a slain guardian's essence, and the Riftforge will tear open a path to lands you have never seen. Go prepared, warrior. The way back must be earned." }
            });
        }
    }
}
