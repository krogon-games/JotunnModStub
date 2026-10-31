using System.Globalization;
using System.Linq;
using Jotunn.Entities;
using Orlog.Rules;
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
            CommandManager.Instance.AddConsoleCommand(new ClassCommand());
            CommandManager.Instance.AddConsoleCommand(new StatCommand());
            CommandManager.Instance.AddConsoleCommand(new RespecCommand());
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

            public override string Help => "Orlog: show your level, XP, class and attributes, and the level creatures have here";

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

        private sealed class ClassCommand : ConsoleCommand
        {
            public override string Name => "orlog_class";

            public override string Help => "Orlog: set your class for testing until the Orlog stone exists, e.g. orlog_class Amazon (or none)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                if (args.Length < 1)
                {
                    Console.instance.Print("Usage: orlog_class <class|none>. Playable: " + string.Join(", ", Attributes.Classes.Playable.Select(c => c.Name)));
                    return;
                }
                if (string.Equals(args[0], "none", System.StringComparison.OrdinalIgnoreCase))
                {
                    Attributes.SetClass(player, null);
                    Print(player);
                    return;
                }
                var cls = Attributes.Classes.Find(args[0]);
                if (cls == null || !cls.Playable)
                {
                    Console.instance.Print("Not a playable class. Playable: " + string.Join(", ", Attributes.Classes.Playable.Select(c => c.Name)));
                    return;
                }
                Attributes.SetClass(player, cls);
                Print(player);
            }
        }

        private sealed class StatCommand : ConsoleCommand
        {
            public override string Name => "orlog_stat";

            public override string Help => "Orlog: spend stat points until the UI exists, e.g. orlog_stat vitality 5 (str, dex, vit, ene)";

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                int count = 1;
                if (args.Length < 1 || !Attributes.TryParse(args[0], out var attribute) ||
                    (args.Length > 1 && (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out count) || count < 1)))
                {
                    Console.instance.Print("Usage: orlog_stat <strength|dexterity|vitality|energy> [points]");
                    return;
                }
                string refused = Attributes.Spend(player, attribute, count);
                Console.instance.Print(refused.Length > 0 ? refused : "Spent.");
                Print(player);
            }
        }

        private sealed class RespecCommand : ConsoleCommand
        {
            public override string Name => "orlog_respec";

            public override string Help => "Orlog: take back every spent stat point (until the respec item exists)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = LocalPlayer();
                if (!player)
                {
                    return;
                }
                Attributes.Respec(player);
                Print(player);
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
            var cls = Attributes.ClassOf(player);
            if (cls == null)
            {
                Console.instance.Print("No class yet (orlog_class Amazon to test).");
                return;
            }
            var bonuses = Attributes.BonusesOf(player);
            Console.instance.Print($"{cls.Name}: Strength {Attributes.Value(player, Attribute.Strength)}, Dexterity {Attributes.Value(player, Attribute.Dexterity)}, " +
                                   $"Vitality {Attributes.Value(player, Attribute.Vitality)}, Energy {Attributes.Value(player, Attribute.Energy)}; " +
                                   $"{Attributes.Unspent(player)} stat points to spend.");
            Console.instance.Print($"From the class: +{bonuses.Health:0.#} health, +{bonuses.Stamina:0.#} stamina, +{bonuses.Eitr:0.#} eitr, " +
                                   $"+{bonuses.MeleeDamage * 100f:0.#}% melee and thrown damage, +{bonuses.RangedDamage * 100f:0.#}% bow damage.");
        }
    }
}
