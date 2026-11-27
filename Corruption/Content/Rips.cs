using System;
using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Opening and travelling through rips (design phase 4, step A). Using a map: the player's game samples
    ///     unexplored spots of the map's biome from world generation (explored land only when none is left), the
    ///     server picks the first one away from everything a player placed (only it knows every object) and sets the
    ///     closing time, then the player's game consumes the map and spawns the entry rip next to the player and the
    ///     heart 96-128 m from the landing spot. The map's tier and affixes travel with the request and the reply.
    /// </summary>
    internal static class Rips
    {
        public const string KeyTarget = "corruption_rip_target";
        public const string KeyClose = "corruption_rip_close";
        public const string KeyBiome = "corruption_rip_biome";
        public const string KeyExit = "corruption_rip_exit";
        public const string KeyHome = "corruption_rip_home";
        public const string KeyGrounded = "corruption_rip_grounded";
        public const string KeyHeart = "corruption_rip_heart";

        private const string RpcRequest = "Corruption_RipRequest";
        private const string RpcReply = "Corruption_RipReply";

        // How many random world spots to test, and how many matches to send to the server.
        private const int Samples = 8000;
        private const int Candidates = 30;
        // Heart and landing spots must be flat within this radius (metres), the area players arrive on.
        private const float FlatRadius = 6f;
        // Directions tried around each heart spot for a landing spot.
        private const int LandingTries = 16;
        // The explored-land fallback has a smaller chance per spot (explored land is mostly near buildings), so it tries more.
        private const int FallbackSamples = 16000;
        private const int FallbackCandidates = 80;

        /// <summary>Dev command (co_rip fallback on): skip the unexplored search, as if no unexplored land were left. Not saved.</summary>
        public static bool SkipUnexplored;

        private static int s_requestId;
        private static int s_pendingId;
        private static float s_pendingSince;
        private static string s_pendingMap;
        private static ItemDrop.ItemData s_pendingItem;
        private static MapState s_pendingState;
        private static bool s_pendingFallback;
        private static Vector3? s_snapTarget;

        // ------------------------------------------------------------------ opening

        /// <summary>The local player used a map: find candidate spots and ask the server to pick one.</summary>
        public static void TryOpen(Player player, MapList.Map map, ItemDrop.ItemData mapItem)
        {
            if (!CorruptionConfig.RipsEnabled.Value)
            {
                player.Message(MessageHud.MessageType.Center, "$corruption_rip_disabled");
                return;
            }
            if (s_pendingId != 0 && Time.time - s_pendingSince < 15f)
            {
                player.Message(MessageHud.MessageType.Center, "$corruption_rip_busy");
                return;
            }
            if (player.IsTeleporting() || player.InInterior() || WorldGenerator.instance == null || ZRoutedRpc.instance == null)
            {
                player.Message(MessageHud.MessageType.Center, "$corruption_rip_not_here");
                return;
            }

            var biome = (Heightmap.Biome)Enum.Parse(typeof(Heightmap.Biome), map.Biome);
            var world = WorldGenerator.instance;
            float water = ZoneSystem.instance ? ZoneSystem.instance.m_waterLevel : 30f;
            var minimap = Minimap.instance;
            // The game's own flatness test for placing locations: height change within a radius, from world generation.
            float maxDelta = new ZoneSystem.ZoneLocation().m_maxTerrainDelta;
            var random = new System.Random();
            bool requireUnexplored = true;
            bool Suitable(float x, float z)
            {
                if (world.GetBiome(x, z) != biome)
                {
                    return false;
                }
                float h = world.GetHeight(x, z);
                if (h <= water)
                {
                    return false;
                }
                world.GetTerrainDelta(new Vector3(x, h, z), FlatRadius, out float delta, out _);
                if (delta > maxDelta)
                {
                    return false;
                }
                return !requireUnexplored || !minimap || !minimap.IsExplored(new Vector3(x, h, z));
            }
            // The heart stands 96-128 m from where players land, so they have to go and find it (user, 2026-10-29).
            List<RipRules.Placement> Search(int samples, int candidates)
            {
                var found = new List<RipRules.Placement>();
                foreach (var heart in RipRules.Sample(random.NextDouble, samples, candidates, Suitable))
                {
                    var landing = RipRules.SpotAround(heart, random.NextDouble, RipRules.HeartMinDistance, RipRules.HeartMaxDistance, LandingTries, Suitable);
                    if (landing.HasValue)
                    {
                        found.Add(new RipRules.Placement(heart, landing.Value));
                    }
                }
                return found;
            }

            // Unexplored land stays the first choice (rips round decision 12, #84); explored land only when none turns up.
            var spots = SkipUnexplored ? new List<RipRules.Placement>() : Search(Samples, Candidates);
            bool fallback = false;
            if (spots.Count == 0)
            {
                requireUnexplored = false;
                fallback = true;
                spots = Search(FallbackSamples, FallbackCandidates);
                if (spots.Count > 0)
                {
                    player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$corruption_rip_fallback", map.Label));
                }
            }
            if (spots.Count == 0)
            {
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$corruption_rip_no_land", map.Label));
                return;
            }

            var state = MapTiers.StateOf(mapItem);
            s_pendingId = ++s_requestId;
            s_pendingSince = Time.time;
            s_pendingMap = map.Item;
            s_pendingItem = mapItem;
            s_pendingState = state;
            s_pendingFallback = fallback;
            var package = new ZPackage();
            package.Write(s_pendingId);
            package.Write(map.Item);
            package.Write(spots.Count);
            foreach (var spot in spots)
            {
                package.Write(spot.Heart.X);
                package.Write(spot.Heart.Z);
                package.Write(spot.Landing.X);
                package.Write(spot.Landing.Z);
            }
            // The map's tier and affixes, after the candidates (the server checks them and echoes them in its reply).
            package.Write(state.Tier);
            package.Write(MapData.Join(state.Affixes));
            package.Write(fallback);
            // With the fallback, its note stays on screen instead of "searching" (it is shown again when the rip opens).
            if (!fallback)
            {
                player.Message(MessageHud.MessageType.Center, "$corruption_rip_searching");
            }
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), RpcRequest, package);
        }

        /// <summary>Server: pick the first candidate whose heart and landing spot are away from every base; reply with it and the closing time.</summary>
        private static void RPC_Request(long sender, ZPackage package)
        {
            if (!ZNet.instance || !ZNet.instance.IsServer())
            {
                return;
            }
            Safe.Run("choose a rip landing spot", () =>
            {
                int id = package.ReadInt();
                string mapItem = package.ReadString();
                int count = package.ReadInt();
                var candidates = new List<RipRules.Placement>();
                for (int i = 0; i < count; i++)
                {
                    var heart = new RipRules.Spot(package.ReadSingle(), package.ReadSingle());
                    var landing = new RipRules.Spot(package.ReadSingle(), package.ReadSingle());
                    candidates.Add(new RipRules.Placement(heart, landing));
                }

                // Read only when present: a player on an older build of this round sends none (Opus review, 2026-11-26).
                int tier = 1;
                string affixText = "";
                bool fallback = false;
                if (package.GetPos() < package.Size())
                {
                    tier = package.ReadInt();
                    affixText = package.ReadString();
                    fallback = package.ReadBool();
                }

                // The tier and affixes come from a player's game: keep them inside the tables.
                var affixIds = new List<string>();
                if (MapTiers.Loaded)
                {
                    var tiers = MapTiers.Tables.Tiers;
                    foreach (string affix in MapData.Split(affixText))
                    {
                        if (MapTiers.Tables.Affixes.Find(affix) == null)
                        {
                            Jotunn.Logger.LogWarning($"Rip request: unknown map affix '{affix}' dropped.");
                        }
                        else if (!affixIds.Contains(affix) && affixIds.Count < tiers.MaxAffixes)
                        {
                            affixIds.Add(affix);
                        }
                    }
                    // The tier follows from the affixes (decision 6); only a full map can be the corrupted tier (MP review, 2026-11-27).
                    bool corrupted = tier >= tiers.CorruptTier && affixIds.Count >= tiers.MaxAffixes;
                    tier = tiers.TierFor(affixIds.Count, corrupted);
                }
                else
                {
                    tier = 1;
                }

                // Neither the heart nor the landing spot may be near anything a player placed (rips round decision 12).
                float distance = RipRules.BaseDistance(CorruptionConfig.RipBaseDistance.Value);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var scan = new RipRules.PlacedScan(candidates, distance);
                int zdosSeen = 0, placedChecked = 0;
                // All the world's objects (the server holds every one); sector searches changed between game versions.
                foreach (var zdo in ZDOMan.instance.m_objectsByID.Values)
                {
                    zdosSeen++;
                    var at = zdo.GetPosition();
                    // Placed by a player (the game's builder id), or a crop grown from one (the grown plant has no builder id).
                    if (!scan.Touches(at.x, at.z) || (zdo.GetLong(ZDOVars.s_creator, 0L) == 0L && !GrownCrops().Contains(zdo.GetPrefab())))
                    {
                        continue;
                    }
                    placedChecked++;
                    scan.Reject(at.x, at.z);
                    if (scan.AllRejected)
                    {
                        break;
                    }
                }
                var spot = scan.First();
                watch.Stop();
                Jotunn.Logger.LogInfo($"Rip request: {candidates.Count} candidate(s){(fallback ? " (explored-land fallback)" : "")}, " +
                                      $"{scan.Alive} away from player-placed objects ({distance:0} m); checked {placedChecked} placed object(s) near them " +
                                      $"of {zdosSeen} in {watch.ElapsedMilliseconds} ms.");
                double dayLength = EnvMan.instance ? EnvMan.instance.m_dayLengthSec : 1800.0;
                double closeAt = ZNet.instance.GetTimeSeconds() + CorruptionConfig.RipOpenDays.Value * dayLength;

                var reply = new ZPackage();
                reply.Write(id);
                reply.Write(mapItem);
                reply.Write(spot.HasValue);
                reply.Write(spot.HasValue ? spot.Value.Heart.X : 0f);
                reply.Write(spot.HasValue ? spot.Value.Heart.Z : 0f);
                reply.Write(spot.HasValue ? spot.Value.Landing.X : 0f);
                reply.Write(spot.HasValue ? spot.Value.Landing.Z : 0f);
                reply.Write(closeAt);
                reply.Write(tier);
                reply.Write(MapData.Join(affixIds));
                ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcReply, reply);
            });
        }

        private static HashSet<int> s_grownCrops;

        /// <summary>
        ///     The prefabs a planted crop grows into (a Plant's grown Pickables, e.g. Pickable_Barley; not the wild ones such
        ///     as Pickable_Barley_Wild): the grown plant carries no builder id, but it is still something players made (MP
        ///     review, 2026-11-27). Grown trees can't be told from wild ones, so they don't count.
        /// </summary>
        private static HashSet<int> GrownCrops()
        {
            if (s_grownCrops == null)
            {
                s_grownCrops = new HashSet<int>();
                foreach (var prefab in ZNetScene.instance.m_prefabs)
                {
                    var plant = prefab ? prefab.GetComponent<Plant>() : null;
                    if (plant == null || plant.m_grownPrefabs == null)
                    {
                        continue;
                    }
                    foreach (var grown in plant.m_grownPrefabs)
                    {
                        if (grown && grown.GetComponent<Pickable>())
                        {
                            s_grownCrops.Add(grown.name.GetStableHashCode());
                        }
                    }
                }
                Jotunn.Logger.LogInfo($"Rip requests: {s_grownCrops.Count} grown-crop kinds count as player-made.");
            }
            return s_grownCrops;
        }

        /// <summary>The player's game: consume the map and open the rip, or say why not.</summary>
        private static void RPC_Reply(long sender, ZPackage package)
        {
            Safe.Run("open a rip", () =>
            {
                int id = package.ReadInt();
                string mapItem = package.ReadString();
                bool found = package.ReadBool();
                float heartX = package.ReadSingle();
                float heartZ = package.ReadSingle();
                float landingX = package.ReadSingle();
                float landingZ = package.ReadSingle();
                double closeAt = package.ReadDouble();
                // Read only when present: a server on an older build sends none, and the rip opens as tier 1.
                int tier = 1;
                var affixIds = new List<string>();
                bool serverKnowsTiers = package.GetPos() < package.Size();
                if (serverKnowsTiers)
                {
                    tier = package.ReadInt();
                    affixIds = MapData.Split(package.ReadString());
                }
                if (id != s_pendingId || mapItem != s_pendingMap)
                {
                    return;
                }
                s_pendingId = 0;
                var usedItem = s_pendingItem;
                var usedState = s_pendingState;
                bool fallback = s_pendingFallback;
                s_pendingItem = null;
                s_pendingState = null;

                var player = Player.m_localPlayer;
                if (!player || !MapItems.ByItem.TryGetValue(mapItem, out var map))
                {
                    return;
                }
                if (!found)
                {
                    player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$corruption_rip_only_bases", map.Label));
                    return;
                }
                // A server on an older build answers without tiers: a tiered map would open as tier 1 and be lost, so keep it
                // (MP review, 2026-11-27). Same-version players can't hit this.
                if (!serverKnowsTiers && usedState != null && (usedState.Tier > 1 || usedState.Affixes.Count > 0))
                {
                    Jotunn.Logger.LogWarning("Rip request: the server's Corruption has no map tiers (an older build); the tiered map is kept.");
                    player.Message(MessageHud.MessageType.Center, "$corruption_rip_old_server");
                    return;
                }
                // Use up the very map that was used: it is in the bag or in the Maps tab. If it has moved or is gone, another
                // map of the same kind with the same tier, affixes and locks; if there is none, open nothing.
                var item = usedItem;
                // Only while it is still the map that was sent: one changed at the Forge during the wait isn't used up at
                // its old tier (Opus review, 2026-11-27).
                var inventory = item != null && SameMap(MapTiers.StateOf(item), usedState) ? InventoryTabs.SourceOf(player, item) : null;
                if (inventory == null)
                {
                    item = InventoryTabs.Find(player, i => MapItems.Find(i) == map && SameMap(MapTiers.StateOf(i), usedState), out inventory);
                }
                if (item == null || inventory == null)
                {
                    player.Message(MessageHud.MessageType.Center, "$corruption_rip_map_gone");
                    return;
                }
                inventory.RemoveOneItem(item);

                var landing = new Vector3(landingX, WorldGenerator.instance.GetHeight(landingX, landingZ), landingZ);
                var heartPosition = new Vector3(heartX, WorldGenerator.instance.GetHeight(heartX, heartZ), heartZ);
                var forward = player.transform.forward;
                forward.y = 0f;
                forward = forward.sqrMagnitude > 0.01f ? forward.normalized : Vector3.forward;
                var entryPosition = player.transform.position + forward * 4f;
                entryPosition.y = ZoneSystem.instance.GetGroundHeight(entryPosition);
                // The portal faces the player.
                var entryRotation = Quaternion.LookRotation(-forward);
                long closeMs = (long)(closeAt * 1000.0);

                var entry = Spawn(Texts.RipPrefab, entryPosition, entryRotation);
                entry.Set(KeyTarget, landing);
                // For the corrupted wishbone: travellers remember where the heart is.
                entry.Set(KeyHeart, heartPosition);
                entry.Set(KeyClose, closeMs);
                entry.Set(KeyBiome, map.Biome);
                MapTiers.Stamp(entry, tier, affixIds);

                var heart = Spawn(Texts.RipHeartPrefab, heartPosition, Quaternion.identity);
                // Exit travellers arrive just in front of the entry rip, back home.
                heart.Set(KeyHome, entryPosition + forward * -2.5f);
                heart.Set(KeyClose, closeMs);
                heart.Set(KeyBiome, map.Biome);
                MapTiers.Stamp(heart, tier, affixIds);

                Jotunn.Logger.LogInfo($"Rip opened: {map.Biome}, {MapTiers.Describe(tier, affixIds)}, heart {heartPosition}, landing {landing}, " +
                                      $"{(fallback ? "explored-land fallback" : "unexplored land")}.");
                string opened = Localization.instance.Localize("$corruption_rip_opened", map.Label);
                if (fallback)
                {
                    opened += "\n" + Localization.instance.Localize("$corruption_rip_fallback", map.Label);
                }
                player.Message(MessageHud.MessageType.Center, opened);
            });
        }

        /// <summary>Whether two map states are the same map for using it up: the same affixes (in order), corrupted and mirrored.</summary>
        private static bool SameMap(MapState a, MapState b)
        {
            return a != null && b != null && a.Corrupted == b.Corrupted && a.Mirrored == b.Mirrored &&
                   MapData.Join(a.Affixes) == MapData.Join(b.Affixes);
        }

        /// <summary>The heart was destroyed (on its owner's game): open the exit rip beside it, leading home.</summary>
        public static void OpenExit(ZDO heart, Vector3 position)
        {
            Jotunn.Logger.LogInfo($"Rip heart destroyed at {position}.");
            if (heart == null || IsExpired(heart))
            {
                Jotunn.Logger.LogWarning(heart == null ? "Rip heart: no data, no exit." : "Rip heart: the rip had already closed, no exit.");
                return;
            }
            var home = heart.GetVec3(KeyHome, Vector3.zero);
            if (home == Vector3.zero)
            {
                Jotunn.Logger.LogWarning("Rip heart: no way home stored, no exit.");
                return;
            }
            var spot = position + new Vector3(0f, 0f, 3f);
            spot.y = ZoneSystem.instance.GetGroundHeight(spot);
            var exit = Spawn(Texts.RipPrefab, spot, Quaternion.identity);
            exit.Set(KeyTarget, home);
            exit.Set(KeyClose, heart.GetLong(KeyClose, 0L));
            exit.Set(KeyBiome, heart.GetString(KeyBiome, ""));
            exit.Set(KeyExit, true);
            MapTiers.CopyStamp(heart, exit);
            Jotunn.Logger.LogInfo($"Rip exit opened at {spot}, leading to {home}.");
            Safe.Run("record the first destroyed rip heart", MapDrops.MarkRipCleared);
            Safe.Run("spill the rip loot", () => RipLoot.Spill(heart, heart.GetString(KeyBiome, ""), position));
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$corruption_rip_home_opened");
        }

        private static ZDO Spawn(string prefabName, Vector3 position, Quaternion rotation)
        {
            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            // Objects placed far away keep their ZDO (both are persistent) and appear when someone gets there.
            var instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            return instance.GetComponent<ZNetView>().GetZDO();
        }

        // ------------------------------------------------------------------ travelling

        public static void Enter(ZDO rip, Player player)
        {
            if (IsExpired(rip))
            {
                player.Message(MessageHud.MessageType.Center, "$corruption_rip_closed");
                return;
            }
            if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoPortals))
            {
                player.Message(MessageHud.MessageType.Center, "$msg_blocked");
                return;
            }
            // Portal rules apply: no ores or metals through a rip either.
            if (!player.IsTeleportable(false))
            {
                player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                return;
            }
            var target = rip.GetVec3(KeyTarget, Vector3.zero);
            if (target == Vector3.zero)
            {
                return;
            }
            var destination = target + Vector3.up * 1.5f;
            if (player.TeleportTo(destination, player.transform.rotation, true))
            {
                s_snapTarget = destination;
                if (rip.GetBool(KeyExit))
                {
                    RipCompass.Forget(player);
                }
                else
                {
                    var heart = rip.GetVec3(KeyHeart, Vector3.zero);
                    if (heart != Vector3.zero)
                    {
                        RipCompass.Remember(player, heart, rip.GetLong(KeyClose, 0L));
                    }
                }
            }
        }

        public static bool IsExpired(ZDO rip)
        {
            long closeMs = rip.GetLong(KeyClose, 0L);
            return closeMs > 0 && ZNet.instance && ZNet.instance.GetTimeSeconds() * 1000.0 > closeMs;
        }

        /// <summary>The highest ground or rock at a spot (not trees or buildings), once its area has loaded.</summary>
        public static bool TryFindGround(Vector3 near, out Vector3 ground)
        {
            int mask = LayerMask.GetMask("terrain", "static_solid");
            if (Physics.Raycast(new Vector3(near.x, near.y + 200f, near.z), Vector3.down, out var hit, 600f, mask))
            {
                ground = hit.point;
                return true;
            }
            ground = near;
            return false;
        }

        public static string PortalHoverText(ZDO rip)
        {
            string where = rip.GetBool(KeyExit)
                ? "$corruption_rip_home"
                : Localization.instance.Localize("$corruption_rip_to", BiomeLabel(rip.GetString(KeyBiome, "")));
            return Localization.instance.Localize($"$corruption_rip\n{where}\n{TierText(rip)}\n{ClosesText(rip)}\n[<color=yellow><b>$KEY_Use</b></color>] $corruption_rip_enter");
        }

        public static string HeartHoverText(ZDO heart)
        {
            return Localization.instance.Localize($"$corruption_rip_heart\n$corruption_rip_heart_hint\n{TierText(heart)}\n{ClosesText(heart)}");
        }

        /// <summary>"Tier 5 · Swarming, Hardened, ..." from the object, so the whole group sees what they walk into.</summary>
        private static string TierText(ZDO rip)
        {
            return MapTiers.Describe(MapTiers.TierOf(rip), MapTiers.AffixIdsOf(rip));
        }

        private static string ClosesText(ZDO rip)
        {
            long closeMs = rip.GetLong(KeyClose, 0L);
            if (closeMs <= 0 || !ZNet.instance)
            {
                return "";
            }
            double left = closeMs / 1000.0 - ZNet.instance.GetTimeSeconds();
            return Localization.instance.Localize("$corruption_rip_closes", RipRules.TimeLeft(left));
        }

        private static string BiomeLabel(string biome)
        {
            var map = MapItems.ByItem.Values.FirstOrDefault(m => m.Biome == biome);
            return map != null ? map.Label : biome;
        }

        // ------------------------------------------------------------------ patches

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpcs
        {
            private static void Postfix()
            {
                Safe.Run("register rip messages", () =>
                {
                    ZRoutedRpc.instance.Register<ZPackage>(RpcRequest, RPC_Request);
                    ZRoutedRpc.instance.Register<ZPackage>(RpcReply, RPC_Reply);
                });
            }
        }

        /// <summary>Using a map (right-click in the inventory, or from the hotbar) opens a rip instead of the vanilla "use on what?".</summary>
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
        private static class UseMap
        {
            private static bool Prefix(Humanoid __instance, Inventory __0, ItemDrop.ItemData __1, bool __runOriginal)
            {
                if (!__runOriginal || __instance != Player.m_localPlayer)
                {
                    return __runOriginal;
                }
                try
                {
                    var map = MapItems.Find(__1);
                    // Only maps in the player's own inventory or Maps tab (not in a chest or backpack): that's where the map is taken from.
                    if (map == null || (__0 != null && __0 != Player.m_localPlayer.GetInventory() && !InventoryTabs.IsTab(__0)))
                    {
                        return true;
                    }
                    // As vanilla: the item must really be in the inventory it was used from (Opus review, 2026-11-26).
                    if (!(__0 ?? Player.m_localPlayer.GetInventory()).ContainsItem(__1))
                    {
                        return true;
                    }
                    TryOpen(Player.m_localPlayer, map, __1);
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("use a map", ex);
                    return true;
                }
            }
        }

        /// <summary>
        ///     After a rip teleport, put the player on the ground: the destination came from world generation heights,
        ///     and the real ground (rocks, terrain edits) can differ.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        private static class SnapAfterTeleport
        {
            private static void Postfix(Player __instance)
            {
                if (__instance != Player.m_localPlayer)
                {
                    return;
                }
                // A map that got no answer in 15 s (a lost message, a server on another build): say so and free it.
                if (s_pendingId != 0 && Time.time - s_pendingSince > 15f)
                {
                    s_pendingId = 0;
                    s_pendingItem = null;
                    s_pendingState = null;
                    Jotunn.Logger.LogWarning("Rip request: no answer from the server within 15 s.");
                    __instance.Message(MessageHud.MessageType.Center, "$corruption_rip_no_answer");
                }
                if (!s_snapTarget.HasValue || __instance.IsTeleporting())
                {
                    return;
                }
                var target = s_snapTarget.Value;
                s_snapTarget = null;
                Safe.Run("land after a rip", () =>
                {
                    if (TryFindGround(target, out var ground))
                    {
                        var position = new Vector3(target.x, ground.y + 0.3f, target.z);
                        __instance.transform.position = position;
                        __instance.m_maxAirAltitude = position.y;
                    }
                });
            }
        }
    }
}
