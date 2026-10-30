using System.Globalization;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Orlog.Content
{
    /// <summary>Console commands for testing levels (cheats: need devcommands).</summary>
    internal static class Commands
    {
        public static void Register()
        {
            CommandManager.Instance.AddConsoleCommand(new XpCommand());
            CommandManager.Instance.AddConsoleCommand(new LevelCommand());
            CommandManager.Instance.AddConsoleCommand(new InfoCommand());
        }

        private static Player LocalPlayer()
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                Console.instance.Print("This needs a loaded world.");
                return null;
            }
            return player;
        }

        private sealed class XpCommand : ConsoleCommand
        {
            public override string Name => "orlog_xp";

            public override string Help => "Orlog: add XP to your character, e.g. orlog_xp 500";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                if (args.Length < 1 || !long.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long amount) || amount <= 0)
                {
                    Console.instance.Print("Usage: orlog_xp <amount>");
                    return;
                }
                Progress.AddXp(player, amount);
                Print(player);
            }
        }

        private sealed class LevelCommand : ConsoleCommand
        {
            public override string Name => "orlog_level";

            public override string Help => "Orlog: set your character's level (XP set to the start of it), e.g. orlog_level 10";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) || level < 1)
                {
                    Console.instance.Print("Usage: orlog_level <level>");
                    return;
                }
                Progress.SetXp(player, Progress.Levels.TotalFor(System.Math.Min(level, Progress.MaxLevel)));
                Print(player);
            }
        }

        private sealed class InfoCommand : ConsoleCommand
        {
            public override string Name => "orlog_info";

            public override string Help => "Orlog: show your level and XP, and the level creatures have here";

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                Print(player);
                var position = player.transform.position;
                string biome = WorldGenerator.instance != null ? WorldGenerator.instance.GetBiome(position).ToString() : "?";
                foreach (var b in Progress.Biomes.All)
                {
                    if (b.Name == biome)
                    {
                        Console.instance.Print($"Biome {biome}: creatures level {b.MinLevel}-{b.MaxLevel}, boss level {b.BossLevel}.");
                        return;
                    }
                }
                Console.instance.Print($"Biome {biome}: no level range; creatures count as the players' level.");
            }
        }

        private static void Print(Player player)
        {
            int level = Progress.GetLevel(player);
            long xp = Progress.GetXp(player);
            long start = Progress.Levels.TotalFor(level);
            long toNext = Progress.Levels.ToNext(level, Progress.MaxLevel);
            Console.instance.Print(toNext > 0
                ? $"Level {level}: {xp - start} / {toNext} XP to level {level + 1} ({xp} total)."
                : $"Level {level} (the max): {xp} XP in total.");
        }
    }
}
