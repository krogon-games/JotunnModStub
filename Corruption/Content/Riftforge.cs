using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The Riftforge: the map bench. Built with the hammer next to a workbench; like any vanilla piece it shows
    ///     up once every ingredient has been picked up, so in practice with the first corruption shard.
    ///     It has no recipes yet: maps and corrupted offerings come in later phases.
    /// </summary>
    internal static class Riftforge
    {
        // Galdr table: already a mystical station.
        private const string BasePrefab = "piece_magetable";

        public static void Register()
        {
            var config = new PieceConfig
            {
                Name = Texts.RiftforgeName,
                Description = Texts.RiftforgeDescription,
                PieceTable = PieceTables.Hammer,
                Category = "Crafting",
                CraftingStation = CraftingStations.Workbench
            };
            // Amounts are a first guess; tune during play-testing.
            config.AddRequirement("FineWood", 10);
            config.AddRequirement(Texts.ShardPrefab, 1);
            config.AddRequirement("TrophyEikthyr", 1);
            config.AddRequirement("BronzeNails", 10);
            config.AddRequirement("LeatherScraps", 10);
            config.AddRequirement("SurtlingCore", 2);

            var piece = new CustomPiece(Texts.RiftforgePrefab, BasePrefab, config);

            // Its own station name, so later recipes can target the Riftforge and not the Galdr table.
            var station = piece.PiecePrefab.GetComponent<CraftingStation>();
            if (station)
            {
                station.m_name = Texts.RiftforgeName;
            }

            ReplaceRavenText(piece.PiecePrefab);

            if (ColorUtility.TryParseHtmlString(CorruptionConfig.RiftforgeColor.Value, out var color))
            {
                Corrupted.Tint(piece.PiecePrefab, color, CorruptionConfig.RiftforgeTintStrength.Value);
            }
            else
            {
                Jotunn.Logger.LogWarning($"Riftforge colour '{CorruptionConfig.RiftforgeColor.Value}' isn't a valid HTML colour; tint skipped.");
            }
            var icon = Corrupted.RenderIcon(piece.PiecePrefab);
            if (icon)
            {
                piece.Piece.m_icon = icon;
            }

            PieceManager.Instance.AddPiece(piece);
        }

        /// <summary>
        ///     The Galdr table has a raven guide point that explains the Galdr table. Give the Riftforge's copy its
        ///     own key and lore, so the raven talks about the Riftforge instead (once per player, like vanilla).
        /// </summary>
        private static void ReplaceRavenText(UnityEngine.GameObject prefab)
        {
            foreach (var guide in prefab.GetComponentsInChildren<GuidePoint>(true))
            {
                var old = guide.m_text;
                guide.m_text = new Raven.RavenText
                {
                    m_key = "corruption_riftforge",
                    m_topic = Texts.RavenTopic,
                    m_label = Texts.RavenLabel,
                    m_text = Texts.RavenText,
                    m_priority = old?.m_priority ?? 0,
                    m_isTutorial = old?.m_isTutorial ?? true
                };
            }
        }
    }
}
