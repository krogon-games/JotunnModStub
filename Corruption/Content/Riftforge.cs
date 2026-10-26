using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

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

            Corrupted.Tint(piece.PiecePrefab, Corrupted.Purple, 0.6f);
            var icon = Corrupted.RenderIcon(piece.PiecePrefab);
            if (icon)
            {
                piece.Piece.m_icon = icon;
            }

            PieceManager.Instance.AddPiece(piece);
        }
    }
}
