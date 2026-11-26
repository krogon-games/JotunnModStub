using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Corruption.Content
{
    /// <summary>
    ///     Personal loot and loot beams (Krogon, 2026-11-26; rules in <see cref="PersonalLootRules" />, data in
    ///     <c>personal_loot.csv</c> and <c>loot_beams.csv</c>). A Corruption drop rolled for one player carries that
    ///     player's id and its drop time in its ZDO: for the first <c>lock_seconds</c> (network clock) only that player
    ///     sees it (no item, no beam, no hover) and can pick it up. Every Corruption drop can also carry a beam row, owned
    ///     or shared. Items players drop from their own inventory are never tagged. If the tables fail to load the whole
    ///     feature turns off and drops behave as vanilla. The hiding and the beam are drawn by <see cref="PersonalLootView" />
    ///     on every game except a dedicated server; the pickup rules are in <see cref="PersonalLootPatches" />.
    /// </summary>
    internal static class PersonalLoot
    {
        /// <summary>A made-up other player, for <c>co_personal</c>: their drops stay invisible to everyone until the lock ends.</summary>
        public const long TestOwner = 1;

        public static bool Enabled;
        public static PersonalLootSettings Settings;
        public static LootBeamTable Beams;

        /// <summary>Sounds picked with <c>co_personal sound</c>, until the game quits: beam row name to sound.</summary>
        private static readonly Dictionary<string, string> SoundOverrides = new Dictionary<string, string>();

        /// <summary>Above zero while the game is dropping a creature's loot (<c>CharacterDrop.DropItems</c>).</summary>
        public static int CreatureDropDepth;

        /// <summary>Reads both tables. Any problem is logged and turns the feature off (vanilla drops).</summary>
        public static void Load()
        {
            Enabled = false;
            var errors = new List<string>();
            var settings = PersonalLootSettings.Parse(DataFiles.Read("personal_loot.csv"), errors);
            var beams = LootBeamTable.Parse(DataFiles.Read("loot_beams.csv"), errors);
            if (errors.Count > 0)
            {
                Jotunn.Logger.LogError("Personal loot tables have problems, so personal loot and loot beams are off:\n" + string.Join("\n", errors));
                return;
            }
            Settings = settings;
            Beams = beams;
            Enabled = true;
            Jotunn.Logger.LogInfo($"Personal loot: {Beams.Rows.Count} beam rows, a drop is locked to its owner for {Settings.LockSeconds:0.#} s.");
        }

        /// <summary>
        ///     The local player's id: the character's profile id (the same value <c>Player.GetPlayerID</c> reads from the
        ///     player's ZDO; it works while dead or loading). 0 when there is no game or profile (a dedicated server).
        /// </summary>
        public static long LocalPlayerId
        {
            get
            {
                try
                {
                    if (ZNet.instance && ZNet.instance.IsDedicated())
                    {
                        return 0L;
                    }
                    var profile = Game.instance ? Game.instance.GetPlayerProfile() : null;
                    return profile != null ? profile.GetPlayerID() : 0L;
                }
                catch (System.Exception ex)
                {
                    Safe.Report("read the local player's id", ex);
                    return 0L;
                }
            }
        }

        /// <summary>Whether this game draws things (not a dedicated server, not a headless run).</summary>
        public static bool CanRender =>
            ZNet.instance && !ZNet.instance.IsDedicated() && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;

        /// <summary>Whether the ZDO carries one of our keys.</summary>
        public static bool HasTags(ZDO zdo)
        {
            return zdo != null && (zdo.GetLong(PersonalLootRules.OwnerKey, 0L) != 0L || !string.IsNullOrEmpty(zdo.GetString(PersonalLootRules.BeamKey, "")));
        }

        public static bool IsTagged(ItemDrop drop)
        {
            return drop && drop.m_nview && drop.m_nview.IsValid() && HasTags(drop.m_nview.GetZDO());
        }

        /// <summary>
        ///     Marks a drop the creating game has just made: the owner (only if not 0), the drop time on the network clock
        ///     and the beam row (only if the table has it), then starts the view. Skipped silently when the feature is off,
        ///     the drop isn't ours to write, or there is nothing to tag. Returns the drop.
        /// </summary>
        public static ItemDrop Tag(ItemDrop drop, long owner, string beamRow)
        {
            try
            {
                if (!Enabled || !drop || !drop.m_nview || !drop.m_nview.IsValid() || !drop.m_nview.IsOwner() || !ZNet.instance)
                {
                    return drop;
                }
                bool hasRow = Beams.Find(beamRow) != null;
                if (owner == 0L && !hasRow)
                {
                    return drop;
                }
                var zdo = drop.m_nview.GetZDO();
                if (owner != 0L)
                {
                    zdo.Set(PersonalLootRules.OwnerKey, owner);
                }
                zdo.Set(PersonalLootRules.TimeKey, ZNet.instance.GetTime().Ticks);
                if (hasRow)
                {
                    zdo.Set(PersonalLootRules.BeamKey, Beams.Find(beamRow).Drop);
                }
                Attach(drop);
            }
            catch (System.Exception ex)
            {
                Safe.Report("tag a personal drop", ex);
            }
            return drop;
        }

        /// <summary>Adds the view to a tagged drop, on games that draw things.</summary>
        public static void Attach(ItemDrop drop)
        {
            if (Enabled && drop && CanRender && !drop.GetComponent<PersonalLootView>())
            {
                drop.gameObject.AddComponent<PersonalLootView>();
            }
        }

        /// <summary>How the drop looks to the viewer: from the ZDO keys, the network clock and the lock length.</summary>
        public static LootLockState StateOf(ItemDrop drop, long viewer)
        {
            if (!Enabled || !drop || !drop.m_nview || !drop.m_nview.IsValid() || !ZNet.instance)
            {
                return LootLockState.Free;
            }
            var zdo = drop.m_nview.GetZDO();
            return PersonalLootRules.StateFor(zdo.GetLong(PersonalLootRules.OwnerKey, 0L), zdo.GetLong(PersonalLootRules.TimeKey, 0L),
                ZNet.instance.GetTime().Ticks, viewer, Settings.LockSeconds);
        }

        /// <summary>Whether the local player can't see or take this drop now.</summary>
        public static bool IsHiddenFromMe(ItemDrop drop) => StateOf(drop, LocalPlayerId) == LootLockState.Theirs;

        // ---- Which beam row a drop gets ----

        /// <summary>The beam row of an item: gear by its rolled rarity, otherwise by its prefab. Null for none.</summary>
        public static string BeamRowFor(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return null;
            }
            var loot = Loot.Of(item);
            if (loot != null)
            {
                return LootBeamTable.ForGear(loot.Rarity);
            }
            return item.m_dropPrefab ? BeamRowForPrefab(item.m_dropPrefab.name) : null;
        }

        /// <summary>The beam row of an item prefab: an orb's, the shard's, a map's; null for anything else (essences, vanilla items).</summary>
        public static string BeamRowForPrefab(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return null;
            }
            string orb = OrbRules.OrbOf(prefabName);
            if (orb != null)
            {
                return LootBeamTable.ForOrb(orb);
            }
            if (prefabName == Texts.ShardPrefab)
            {
                return LootBeamTable.Shard;
            }
            return MapItems.ByItem.ContainsKey(prefabName) ? LootBeamTable.Map : null;
        }

        // ---- Sound ----

        /// <summary>The sound a beam row plays when its drop lands: the one picked with the dev command, else the table's. Null for none.</summary>
        public static string SoundFor(LootBeamRow row)
        {
            if (row == null)
            {
                return null;
            }
            return SoundOverrides.TryGetValue(row.Drop, out var picked) ? picked : (row.HasSound ? row.Sound : null);
        }

        public static void OverrideSound(string row, string sound) => SoundOverrides[row] = sound;

        /// <summary>
        ///     Plays a sound at a position: <c>default</c> is the game's skill-level-up sound (only the sound parts of its
        ///     effects); anything else is a sound prefab's name. Returns a problem text, or null when it played.
        /// </summary>
        public static string PlaySound(string sound, Vector3 position)
        {
            if (string.IsNullOrEmpty(sound))
            {
                return "no sound given";
            }
            if (sound == LootBeamTable.DefaultSound)
            {
                var effects = SkillUpEffects();
                if (effects == null)
                {
                    return "the game's skill-up sound isn't available here";
                }
                bool played = false;
                foreach (var data in effects.m_effectPrefabs)
                {
                    if (data != null && data.m_enabled && data.m_prefab && HasSoundParts(data.m_prefab) && !data.m_prefab.GetComponent<ZNetView>())
                    {
                        Spawn(data.m_prefab, position, true);
                        played = true;
                    }
                }
                return played ? null : "the skill-up effects carry no sound";
            }
            var prefab = FindPrefab(sound);
            if (!prefab)
            {
                return $"no prefab called {sound}";
            }
            if (prefab.GetComponent<ZNetView>())
            {
                return $"{sound} is a networked object, not a plain sound";
            }
            Spawn(prefab, position, false);
            return null;
        }

        private static EffectList SkillUpEffects()
        {
            if (Player.m_localPlayer)
            {
                return Player.m_localPlayer.m_skillLevelupEffects;
            }
            var prefab = Game.instance ? Game.instance.m_playerPrefab : null;
            var player = prefab ? prefab.GetComponent<Player>() : null;
            return player ? player.m_skillLevelupEffects : null;
        }

        private static bool HasSoundParts(GameObject prefab)
        {
            return prefab.GetComponentInChildren<ZSFX>(true) || prefab.GetComponentInChildren<AudioSource>(true);
        }

        public static GameObject FindPrefab(string name)
        {
            var prefab = ZNetScene.instance ? ZNetScene.instance.GetPrefab(name) : null;
            return prefab ? prefab : PrefabManager.Cache.GetPrefab<GameObject>(name);
        }

        /// <summary>Makes the effect; with <paramref name="soundOnly" /> its looks (renderers, particles, lights) are switched off.</summary>
        private static void Spawn(GameObject prefab, Vector3 position, bool soundOnly)
        {
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            if (soundOnly)
            {
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = false;
                }
                foreach (var particles in go.GetComponentsInChildren<ParticleSystem>(true))
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                foreach (var light in go.GetComponentsInChildren<Light>(true))
                {
                    light.enabled = false;
                }
            }
            // Effect prefabs remove themselves; this only catches one that doesn't.
            Object.Destroy(go, 15f);
        }

        // ---- Dev command ----

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new PersonalCommand());
        }

        private sealed class PersonalCommand : ConsoleCommand
        {
            public override string Name => "co_personal";

            public override string Help => "Corruption: co_personal - test personal loot and loot beams (type it alone for the list of commands)";

            public override bool IsCheat => true;

            private const string HelpText =
                "co_personal commands (all drop things in front of you):\n" +
                "  co_personal drop <magic|rare|shard|map|orb ID> [other] [seconds]  - drop one thing. It is yours; with 'other' it belongs to a made-up other player, " +
                "so you can't see it until its lock ends. 'seconds' = lock time left (default: the full lock).\n" +
                "  co_personal beams [other]  - one of every beam in a row in front of you, to compare looks (and sounds)\n" +
                "  co_personal list [range]  - every personal drop within range (default 30 m): whose it is, time left, and whether you see it\n" +
                "  co_personal sounds [filter]  - the game's sound names (filter = part of a name); with no filter, a short list of candidates\n" +
                "  co_personal play <sound|default>  - play a sound here ('default' = the skill-up sound)\n" +
                "  co_personal sound <row|tall> <sound|default>  - use that sound for one beam row (e.g. rare) or for every tall row, until you quit the game\n" +
                "Orb ids: " + "transmutation, augmentation, alchemy, regal, exalted, divine, annulment, chance, mirror (see co_orbroll for the full list)";

            public override void Run(string[] args)
            {
                if (args.Length == 0)
                {
                    Print(HelpText);
                    return;
                }
                if (!Enabled)
                {
                    Print("Personal loot is off: its tables didn't load (see the log).");
                    return;
                }
                switch (args[0].ToLowerInvariant())
                {
                    case "drop": Drop(args); break;
                    case "beams": BeamRow(args); break;
                    case "list": List(args); break;
                    case "sounds": Sounds(args); break;
                    case "play": Play(args); break;
                    case "sound": Sound(args); break;
                    default: Print(HelpText); break;
                }
            }

            private static void Print(string text) => Console.instance.Print(text);

            private static string OwnerText(long owner)
            {
                return owner == 0L ? "nobody" : owner == LocalPlayerId ? "you" : owner == TestOwner ? "the test owner" : "player " + owner;
            }

            /// <summary>Makes one drop at a position (no scatter), or null with a reason in <paramref name="problem" />.</summary>
            private static ItemDrop Make(string kind, string orb, Vector3 position, out string problem, out string what)
            {
                problem = null;
                what = kind;
                var rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
                switch (kind)
                {
                    case "magic":
                    case "rare":
                    case "unique":
                    {
                        if (!Loot.Loaded)
                        {
                            problem = "Loot tables didn't load; see the log.";
                            return null;
                        }
                        // No uniques exist yet: the unique row is previewed on a Rare item.
                        var item = Loot.Create(kind == "magic" ? "magic" : "rare", Loot.BiomeAt(position));
                        if (item == null)
                        {
                            problem = $"No loot items for the biome here ({Loot.BiomeAt(position)}).";
                            return null;
                        }
                        what = Loot.ColoredName(item) ?? kind;
                        return ItemDrop.DropItem(item, 1, position, rotation);
                    }
                    case "shard":
                        return Prefab(Texts.ShardPrefab, "the corruption shard", position, rotation, out problem, out what);
                    case "map":
                    {
                        var map = MapDrops.MapFor(Loot.BiomeAt(position)) ?? MapItems.ByItem.Values.FirstOrDefault();
                        if (map == null)
                        {
                            problem = "No map items are registered.";
                            return null;
                        }
                        return Prefab(map.Item, map.Name, position, rotation, out problem, out what);
                    }
                    case "orb":
                        if (orb == null || !OrbRules.Orbs.Contains(orb))
                        {
                            problem = "Which orb? e.g. co_personal drop orb exalted";
                            return null;
                        }
                        return Prefab(OrbRules.Prefab(orb), OrbRules.Name(orb), position, rotation, out problem, out what);
                    default:
                        problem = $"Don't know '{kind}'. Use magic, rare, shard, map or orb <id>.";
                        return null;
                }
            }

            private static ItemDrop Prefab(string prefabName, string name, Vector3 position, Quaternion rotation, out string problem, out string what)
            {
                problem = null;
                what = name;
                var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
                if (!prefab)
                {
                    problem = $"No item {prefabName} in this game.";
                    return null;
                }
                var go = Object.Instantiate(prefab, position, rotation);
                ItemDrop.OnCreateNew(go);
                return go.GetComponent<ItemDrop>();
            }

            private static void Drop(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player)
                {
                    Print("co_personal needs a loaded world.");
                    return;
                }
                if (args.Length < 2)
                {
                    Print("co_personal drop <magic|rare|shard|map|orb ID> [other] [seconds]");
                    return;
                }
                string kind = args[1].ToLowerInvariant();
                int next = 2;
                string orb = null;
                if (kind == "orb" && args.Length > 2)
                {
                    orb = args[2].ToLowerInvariant();
                    next = 3;
                }
                bool other = false;
                double seconds = Settings.LockSeconds;
                for (int i = next; i < args.Length; i++)
                {
                    if (args[i].ToLowerInvariant() == "other")
                    {
                        other = true;
                    }
                    else if (double.TryParse(args[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var s))
                    {
                        seconds = s;
                    }
                    else
                    {
                        Print($"Don't understand '{args[i]}': use 'other' or a number of seconds.");
                        return;
                    }
                }
                var position = player.transform.position + player.transform.forward * 1.5f + Vector3.up;
                var drop = Make(kind, orb, position, out string problem, out string what);
                if (drop == null)
                {
                    Print(problem);
                    return;
                }
                long owner = other ? TestOwner : LocalPlayerId;
                string row = kind == "unique" ? "unique" : BeamRowFor(drop.m_itemData);
                Tag(drop, owner, row);
                // The lock time is written after Tag so the chosen seconds win.
                if (drop.m_nview && drop.m_nview.IsValid() && drop.m_nview.IsOwner() && ZNet.instance)
                {
                    drop.m_nview.GetZDO().Set(PersonalLootRules.TimeKey,
                        PersonalLootRules.BackdatedTicks(ZNet.instance.GetTime().Ticks, Settings.LockSeconds, seconds));
                }
                Print($"Dropped {what} for {OwnerText(owner)}, beam: {row ?? "none"}, lock left: {System.Math.Min(seconds, Settings.LockSeconds):0.#} s." +
                      (other ? " You can't see it until the lock ends; use 'co_personal list' to find it." : ""));
            }

            private static void BeamRow(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player)
                {
                    Print("co_personal needs a loaded world.");
                    return;
                }
                bool other = args.Length > 1 && args[1].ToLowerInvariant() == "other";
                long owner = other ? TestOwner : LocalPlayerId;
                var rows = Beams.Rows;
                var centre = player.transform.position + player.transform.forward * 4f + Vector3.up;
                var side = player.transform.right;
                int placed = 0;
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    string kind = row.Drop == "magic" || row.Drop == "rare" || row.Drop == "unique" ? row.Drop :
                        row.Drop == LootBeamTable.Shard ? "shard" : row.Drop == LootBeamTable.Map ? "map" : "orb";
                    string orb = kind == "orb" && row.Drop.StartsWith("orb_") ? row.Drop.Substring(4) : null;
                    var position = centre + side * (1.5f * (i - (rows.Count - 1) / 2f));
                    var drop = Make(kind, orb, position, out string problem, out string what);
                    if (drop == null)
                    {
                        Print($"{row.Drop}: {problem}");
                        continue;
                    }
                    Tag(drop, owner, row.Drop);
                    placed++;
                }
                Print($"Placed {placed} of {rows.Count} beams in a row in front of you, for {OwnerText(owner)} (left to right: " +
                      $"{string.Join(", ", rows.Select(r => r.Drop).ToArray())}). The unique beam is shown on a Rare item; no uniques exist yet.");
            }

            private static void List(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player)
                {
                    Print("co_personal needs a loaded world.");
                    return;
                }
                float range = args.Length > 1 && float.TryParse(args[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 30f;
                long me = LocalPlayerId;
                var found = new List<KeyValuePair<float, string>>();
                foreach (var drop in ItemDrop.s_instances.ToArray())
                {
                    if (!IsTagged(drop))
                    {
                        continue;
                    }
                    float distance = Vector3.Distance(drop.transform.position, player.transform.position);
                    if (distance > range)
                    {
                        continue;
                    }
                    var zdo = drop.m_nview.GetZDO();
                    long owner = zdo.GetLong(PersonalLootRules.OwnerKey, 0L);
                    long time = zdo.GetLong(PersonalLootRules.TimeKey, 0L);
                    double left = time > 0 ? PersonalLootRules.SecondsLeft(time, ZNet.instance.GetTime().Ticks, Settings.LockSeconds) : 0.0;
                    var state = StateOf(drop, me);
                    string name = Loot.ColoredName(drop.m_itemData) ?? Localization.instance.Localize(drop.m_itemData.m_shared.m_name);
                    string beam = zdo.GetString(PersonalLootRules.BeamKey, "");
                    found.Add(new KeyValuePair<float, string>(distance,
                        $"  {name}: beam {(beam.Length > 0 ? beam : "none")}, owner {OwnerText(owner)}, {(owner != 0L && left > 0 ? left.ToString("0") + " s of lock left" : "no lock")}, " +
                        $"your game {(state == LootLockState.Theirs ? "HIDES it" : "shows it")}, {distance:0} m away"));
                }
                Print(found.Count == 0 ? $"No personal drops within {range:0} m." : $"{found.Count} personal drop(s) within {range:0} m (nearest first):");
                foreach (var line in found.OrderBy(f => f.Key).Take(40))
                {
                    Print(line.Value);
                }
            }

            private static readonly string[] Candidates =
                { "default", "sfx_coins_pile_placed", "sfx_coins_placed", "sfx_chest_open", "sfx_fader_bell", "sfx_bowl_AddItem" };

            private static List<string> SoundNames()
            {
                var names = new HashSet<string>();
                foreach (var sfx in Resources.FindObjectsOfTypeAll<ZSFX>())
                {
                    if (sfx && sfx.transform.parent == null && !sfx.GetComponent<ZNetView>())
                    {
                        names.Add(Utils.GetPrefabName(sfx.gameObject));
                    }
                }
                return names.OrderBy(n => n, System.StringComparer.OrdinalIgnoreCase).ToList();
            }

            private static void Sounds(string[] args)
            {
                var names = SoundNames();
                string filter = args.Length > 1 ? args[1] : null;
                if (filter == null)
                {
                    Print("Some sounds to try (found = this game has it):");
                    foreach (var candidate in Candidates)
                    {
                        bool found = candidate == "default" || names.Contains(candidate);
                        Print($"  {candidate}: {(found ? "found" : "missing")}");
                    }
                    Print($"This game has {names.Count} sound prefabs. List them with: co_personal sounds <part of a name>, e.g. co_personal sounds coin. " +
                          "Try one with: co_personal play <name>");
                    return;
                }
                var matching = names.Where(n => n.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                Print($"{matching.Count} sound(s) with '{filter}' in the name" + (matching.Count > 80 ? " (showing the first 80; use a longer filter)" : "") + ":");
                foreach (var name in matching.Take(80))
                {
                    Print("  " + name);
                }
            }

            private static void Play(string[] args)
            {
                var player = Player.m_localPlayer;
                if (!player || args.Length < 2)
                {
                    Print(player ? "co_personal play <sound|default>" : "co_personal needs a loaded world.");
                    return;
                }
                string problem = PlaySound(args[1], player.transform.position + Vector3.up);
                Print(problem ?? $"Played {args[1]}.");
            }

            private static void Sound(string[] args)
            {
                if (args.Length < 3)
                {
                    Print("co_personal sound <row|tall> <sound|default>  (rows: " + string.Join(", ", Beams.Rows.Select(r => r.Drop).ToArray()) + ")");
                    return;
                }
                string target = args[1].ToLowerInvariant();
                string sound = args[2];
                var rows = target == "tall" ? Beams.Rows.Where(r => r.Tall).ToList() : Beams.Rows.Where(r => r.Drop == target).ToList();
                if (rows.Count == 0)
                {
                    Print($"No beam row '{args[1]}'. Use 'tall' or one of: {string.Join(", ", Beams.Rows.Select(r => r.Drop).ToArray())}");
                    return;
                }
                if (sound != LootBeamTable.DefaultSound && !FindPrefab(sound))
                {
                    Print($"No prefab called {sound} in this game (see co_personal sounds).");
                    return;
                }
                foreach (var row in rows)
                {
                    OverrideSound(row.Drop, sound);
                }
                Print($"Using {sound} for {string.Join(", ", rows.Select(r => r.Drop).ToArray())} until you quit the game. Drop one with co_personal drop to hear it.\n" +
                      $"To keep it, tell Claude: \"sound {target} = {sound}\"");
            }
        }
    }

    /// <summary>
    ///     The local look of one tagged drop: hides the item (and its beam) while it is locked to someone else, shows
    ///     a beam while it isn't, and plays the landing sound once for its owner. Added on every game except a dedicated
    ///     server. Nothing here is saved or sent to other games; the keys it reads are in the drop's ZDO.
    /// </summary>
    internal sealed class PersonalLootView : MonoBehaviour
    {
        private ItemDrop _drop;
        private LootBeamRow _row;
        private readonly HashSet<Renderer> _hiddenRenderers = new HashSet<Renderer>();
        private readonly HashSet<Light> _hiddenLights = new HashSet<Light>();
        private GameObject _beam;
        private LineRenderer _line;
        private Material _material;
        private float _height;
        private float _next;
        private bool _hidden;
        private bool _changedAutoPickup;
        private bool _autoPickupWas;
        private bool _played;
        private bool _failed;
        private static bool _shaderReported;

        private void Awake()
        {
            Safe.Run("start a personal drop's view", () =>
            {
                _drop = GetComponent<ItemDrop>();
                if (!_drop || !_drop.m_nview || !_drop.m_nview.IsValid())
                {
                    _failed = true;
                    return;
                }
                // The beam is built first; the item's own renderers and lights are listed on each refresh, leaving the beam out.
                BuildBeam();
                Refresh();
            });
        }

        private void BuildBeam()
        {
            string rowName = _drop.m_nview.GetZDO().GetString(PersonalLootRules.BeamKey, "");
            _row = PersonalLoot.Beams.Find(rowName);
            if (_row == null)
            {
                return;
            }
            var shader = Shader.Find("Sprites/Default");
            if (!shader)
            {
                if (!_shaderReported)
                {
                    _shaderReported = true;
                    Jotunn.Logger.LogWarning("Personal loot: the Sprites/Default shader isn't in this game, so loot beams are off.");
                }
                return;
            }
            var settings = PersonalLoot.Settings;
            float width = _row.Tall ? settings.TallWidth : settings.ShortWidth;
            _height = _row.Tall ? settings.TallHeight : settings.ShortHeight;
            float lightRange = _row.Tall ? settings.TallLightRange : settings.ShortLightRange;
            float lightIntensity = _row.Tall ? settings.TallLightIntensity : settings.ShortLightIntensity;
            var colour = new Color(_row.R, _row.G, _row.B, 1f);

            _beam = new GameObject("CorruptionLootBeam") { hideFlags = HideFlags.DontSave };
            _beam.transform.SetParent(transform, false);
            _line = _beam.AddComponent<LineRenderer>();
            _material = new Material(shader) { hideFlags = HideFlags.DontSave };
            _line.sharedMaterial = _material;
            _line.useWorldSpace = true;
            _line.positionCount = 2;
            _line.alignment = LineAlignment.View;
            _line.startWidth = width;
            _line.endWidth = width;
            _line.shadowCastingMode = ShadowCastingMode.Off;
            _line.receiveShadows = false;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(colour, 0f), new GradientColorKey(colour, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            _line.colorGradient = gradient;
            if (lightIntensity > 0f && lightRange > 0f)
            {
                var lightObject = new GameObject("CorruptionLootBeamLight") { hideFlags = HideFlags.DontSave };
                lightObject.transform.SetParent(_beam.transform, false);
                lightObject.transform.position = transform.position + Vector3.up * 0.3f;
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = colour;
                light.range = lightRange;
                light.intensity = lightIntensity;
                light.shadows = LightShadows.None;
            }
            PositionBeam();
        }

        private void PositionBeam()
        {
            if (_line == null)
            {
                return;
            }
            var bottom = transform.position + Vector3.up * 0.1f;
            _line.SetPosition(0, bottom);
            _line.SetPosition(1, bottom + Vector3.up * _height);
            var light = _beam.GetComponentInChildren<Light>();
            if (light)
            {
                light.transform.position = bottom + Vector3.up * 0.2f;
            }
        }

        private void Update()
        {
            if (_failed)
            {
                return;
            }
            try
            {
                if (Time.time >= _next)
                {
                    _next = Time.time + (float)PersonalLoot.Settings.RefreshSeconds;
                    Refresh();
                }
            }
            catch (System.Exception ex)
            {
                Fail("update a personal drop's view", ex);
            }
        }

        private void LateUpdate()
        {
            if (_failed || _hidden || _line == null)
            {
                return;
            }
            try
            {
                PositionBeam();
            }
            catch (System.Exception ex)
            {
                Fail("move a loot beam", ex);
            }
        }

        private void Fail(string action, System.Exception ex)
        {
            // Back to vanilla: everything we changed is put back, and the view stops.
            _failed = true;
            Safe.Report(action, ex);
            Safe.Run("restore a personal drop", () => Show());
        }

        private void Refresh()
        {
            if (!PersonalLoot.Enabled || !_drop || !_drop.m_nview || !_drop.m_nview.IsValid())
            {
                return;
            }
            long me = PersonalLoot.LocalPlayerId;
            var state = PersonalLoot.StateOf(_drop, me);
            if (state == LootLockState.Theirs)
            {
                Hide();
                return;
            }
            Show();
            if (!_played && _row != null && !_failed)
            {
                var zdo = _drop.m_nview.GetZDO();
                long time = zdo.GetLong(PersonalLootRules.TimeKey, 0L);
                double age = time > 0 ? PersonalLootRules.AgeSeconds(time, ZNet.instance.GetTime().Ticks) : double.MaxValue;
                string sound = PersonalLoot.SoundFor(_row);
                if (PersonalLootRules.PlaySound(state, age, PersonalLoot.Settings.SoundWindowSeconds, sound != null))
                {
                    _played = true;
                    string problem = PersonalLoot.PlaySound(sound, transform.position);
                    if (problem != null)
                    {
                        Jotunn.Logger.LogWarning($"Personal loot: couldn't play the {_row.Drop} sound ({sound}): {problem}.");
                    }
                }
            }
        }

        private void Hide()
        {
            _hidden = true;
            // Renderers and lights are listed again each time: the game can add or switch some on after we first looked.
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer && renderer.enabled && !IsBeamPart(renderer.transform))
                {
                    renderer.enabled = false;
                    _hiddenRenderers.Add(renderer);
                }
            }
            foreach (var light in GetComponentsInChildren<Light>(true))
            {
                if (light && light.enabled && !IsBeamPart(light.transform))
                {
                    light.enabled = false;
                    _hiddenLights.Add(light);
                }
            }
            if (_beam && _beam.activeSelf)
            {
                _beam.SetActive(false);
            }
            // The game skips an item with this local flag when it pulls things towards the player or asks for ownership.
            if (_drop && !_changedAutoPickup)
            {
                _changedAutoPickup = true;
                _autoPickupWas = _drop.m_autoPickup;
            }
            if (_drop)
            {
                _drop.m_autoPickup = false;
            }
        }

        private void Show()
        {
            _hidden = false;
            foreach (var renderer in _hiddenRenderers)
            {
                if (renderer)
                {
                    renderer.enabled = true;
                }
            }
            _hiddenRenderers.Clear();
            foreach (var light in _hiddenLights)
            {
                if (light)
                {
                    light.enabled = true;
                }
            }
            _hiddenLights.Clear();
            if (_beam && !_beam.activeSelf && !_failed)
            {
                _beam.SetActive(true);
                PositionBeam();
            }
            if (_changedAutoPickup)
            {
                _changedAutoPickup = false;
                if (_drop)
                {
                    _drop.m_autoPickup = _autoPickupWas;
                }
            }
        }

        private bool IsBeamPart(Transform t) => _beam && t.IsChildOf(_beam.transform);

        private void OnDestroy()
        {
            if (_material)
            {
                Destroy(_material);
            }
        }
    }
}
