using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CraftingOverhaul.Containers
{
    /// <summary>
    ///     Exclusive access to nearby chests before paying from them (rules and timing in <see cref="LeaseBook{TKey}" />).
    ///     Two players crafting from the same chest at the same moment used to both take from their own copy of it: one
    ///     payment could be lost (a free craft) or items could be taken twice. Now a chest is only paid from once it's
    ///     <b>held</b>: its owner has handed it over through the game's own chest handshake (the one that opens a
    ///     chest, so it works when the owner doesn't have this mod), this game sees itself as the owner (the chest's
    ///     latest contents come with that), and it's marked in use, so nobody else can open it or be handed it until
    ///     it's given back.
    ///     <list type="bullet">
    ///         <item>Crafting: pressing Craft asks for the chests the recipe needs; when the craft bar finishes, the craft goes ahead only if they're all held (the game's own check then counts only them); otherwise it waits for them, and if one is refused or doesn't answer, nothing is crafted and nothing is taken.</item>
    ///         <item>Building: while a selected piece needs chest materials, the chests holding them are asked for and kept for a few seconds after the last need, so placing pays from held chests only.</item>
    ///     </list>
    /// </summary>
    internal static class ChestLeases
    {
        /// <summary>How long chests stay held after building stops needing them, so placing piece after piece doesn't ask again each time. Not a gameplay number.</summary>
        private const double BuildLingerSeconds = 3.0;
        private const float BuildPollSeconds = 0.25f;

        private static readonly LeaseBook<Container> Book = new LeaseBook<Container>();
        /// <summary>Chests whose in-use mark we set (so we clear only our own).</summary>
        private static readonly HashSet<Container> Marked = new HashSet<Container>();
        private static readonly Dictionary<Container, double> KeepUntil = new Dictionary<Container, double>();
        /// <summary>When we last asked each chest, so its answer never opens the chest window (late answers too).</summary>
        private static readonly Dictionary<Container, double> AskedAt = new Dictionary<Container, double>();
        private static readonly List<Container> CraftChests = new List<Container>();
        /// <summary>A chest that refused or didn't answer isn't asked again before this time (no message spam to its owner).</summary>
        private static readonly Dictionary<Container, double> RetryAfter = new Dictionary<Container, double>();

        private static CraftTicket _ticket;
        private static bool _replaying;
        private static bool _paid;
        private static float _nextBuildPoll;

        private static double Now => Time.realtimeSinceStartup;

        /// <summary>Whether a payment may take from this chest: held by us, or open in our own chest window.</summary>
        public static bool IsHeld(Container container)
        {
            if (!container || !container.m_nview || !container.m_nview.IsValid() || !container.m_nview.IsOwner() || !container.m_inUse)
            {
                return false;
            }
            return Book.IsHeld(container) || IsOpenInOurWindow(container);
        }

        private static bool IsOpenInOurWindow(Container container)
        {
            return InventoryGui.instance && InventoryGui.instance.m_currentContainer == container;
        }

        private static void Ask(IEnumerable<Container> chests, double keepFor)
        {
            var player = Player.m_localPlayer;
            if (!player)
            {
                return;
            }
            double now = Now;
            long playerId = Game.instance.GetPlayerProfile().GetPlayerID();
            foreach (var chest in chests)
            {
                if ((IsOpenInOurWindow(chest) && IsHeld(chest)) ||
                    (Book.State(chest) == LeaseState.None && RetryAfter.TryGetValue(chest, out double retry) && retry > now))
                {
                    continue;
                }
                KeepUntil[chest] = KeepUntil.TryGetValue(chest, out double until) && until > now + keepFor ? until : now + keepFor;
                if (Book.Request(chest, now))
                {
                    AskedAt[chest] = now;
                    chest.m_nview.InvokeRPC(nameof(Container.RPC_RequestOpen), playerId);
                }
            }
        }

        private static void Release(Container chest)
        {
            Book.Release(chest);
            KeepUntil.Remove(chest);
            if (Marked.Remove(chest) && chest && chest.m_nview && chest.m_nview.IsValid() && !IsOpenInOurWindow(chest))
            {
                // No open/close sound: nobody opened it as far as the player is concerned. Cleared even if another game
                // owns it by now, so a stale local mark can't refuse others if it comes back to us; only the owner
                // writes the shared flag (UpdateUseVisual).
                chest.m_inUse = false;
                chest.UpdateUseVisual();
            }
        }

        private static void ReleaseAll()
        {
            foreach (var chest in Book.Keys())
            {
                Release(chest);
            }
            foreach (var chest in new List<Container>(Marked))
            {
                Release(chest);
            }
            CraftChests.Clear();
            _ticket = null;
            RetryAfter.Clear();
        }

        private static void ReleaseCraftChests()
        {
            foreach (var chest in CraftChests)
            {
                if (_ticket == null || !_ticket.Chests.Contains(chest))
                {
                    Release(chest);
                }
            }
            CraftChests.Clear();
        }

        /// <summary>Every frame (from the plugin's Update): moves leases along, replays a waiting craft, polls building, gives chests back.</summary>
        public static void Update()
        {
            if (!Player.m_localPlayer || !ZNet.instance)
            {
                if (Book.Keys().Count > 0 || Marked.Count > 0 || _ticket != null)
                {
                    ReleaseAll();
                }
                AskedAt.Clear();
                return;
            }
            double now = Now;
            foreach (var chest in Book.Keys())
            {
                if (!chest || !chest.m_nview || !chest.m_nview.IsValid())
                {
                    Book.Release(chest);
                    KeepUntil.Remove(chest);
                    Marked.Remove(chest);
                    continue;
                }
                bool wasHeld = Book.IsHeld(chest);
                if (!Advance(chest, now) && wasHeld && !Book.IsHeld(chest))
                {
                    // Lost it (someone else's game took it, or our chest window closed it): forget, ask again when needed.
                    Release(chest);
                }
            }
            foreach (var pair in new List<KeyValuePair<Container, double>>(AskedAt))
            {
                if (now - pair.Value > 2 * LeaseBook<Container>.TimeoutSeconds)
                {
                    AskedAt.Remove(pair.Key);
                }
            }

            if (_ticket != null)
            {
                Safe.Run("finish a craft that waited for nearby chests", ResolveTicket);
            }
            if (Time.time >= _nextBuildPoll)
            {
                _nextBuildPoll = Time.time + BuildPollSeconds;
                Safe.Run("reserve nearby chests for building", PollBuilding);
            }

            // A craft that was started and then abandoned (window closed, another recipe chosen): give its chests back.
            if (CraftChests.Count > 0 && _ticket == null && (!InventoryGui.IsVisible() || !InventoryGui.instance || InventoryGui.instance.m_craftTimer < 0f))
            {
                ReleaseCraftChests();
            }

            foreach (var chest in Book.Keys())
            {
                bool forCraft = CraftChests.Contains(chest) || (_ticket != null && _ticket.Chests.Contains(chest));
                bool failed = Book.State(chest) == LeaseState.Failed;
                if (!forCraft && (failed || !KeepUntil.TryGetValue(chest, out double until) || until < now))
                {
                    Release(chest);
                    if (failed)
                    {
                        RetryAfter[chest] = now + LeaseBook<Container>.TimeoutSeconds;
                        if (Player.m_localPlayer.InPlaceMode())
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft,
                                "A nearby chest is in use by someone else: its materials can't be used for building right now.");
                        }
                    }
                }
            }
            foreach (var pair in new List<KeyValuePair<Container, double>>(RetryAfter))
            {
                if (!pair.Key || pair.Value < now)
                {
                    RetryAfter.Remove(pair.Key);
                }
            }
        }

        /// <summary>
        ///     Granted and ours: read the contents that came with the ownership, then mark it in use (the game refuses to
        ///     reload a chest that's in use, and refuses it to anyone else). True when it just became held.
        /// </summary>
        private static bool Advance(Container chest, double now)
        {
            if (!Book.Tick(chest, chest.m_nview.IsOwner(), chest.m_inUse, now))
            {
                return false;
            }
            if (!chest.m_inUse)
            {
                chest.Load();
                chest.m_inUse = true;
                chest.UpdateUseVisual();
                Marked.Add(chest);
            }
            return true;
        }

        // ------------------------------------------------------------ what a cost needs

        /// <summary>The usable nearby chests a cost needs (closest first), from what's carried and what each chest holds.</summary>
        private static List<Container> Plan(Player player, Piece.Requirement[] requirements, int qualityLevel, int multiplier,
            bool everyChestWithStock)
        {
            var result = new List<Container>();
            if (requirements == null)
            {
                return result;
            }
            var station = player.GetCurrentCraftingStation();
            var names = new List<string>();
            var needed = new List<int>();
            foreach (var requirement in requirements)
            {
                // The same lines Player.ConsumeResources pays.
                if (!requirement.m_resItem || (station && station.m_upgrader != requirement.m_upgraderResource) ||
                    (!station && requirement.m_upgraderResource))
                {
                    continue;
                }
                int amount = requirement.GetAmount(qualityLevel) * multiplier;
                if (amount > 0)
                {
                    names.Add(requirement.m_resItem.m_itemData.m_shared.m_name);
                    needed.Add(amount);
                }
            }
            if (names.Count == 0)
            {
                return result;
            }
            var carried = new int[names.Count];
            for (int i = 0; i < names.Count; i++)
            {
                carried[i] = ContainerPatches.CountCarried(player.GetInventory(), names[i]);
            }
            var chests = NearbyContainers.Usable();
            var counts = new List<int[]>(chests.Count);
            foreach (var chest in chests)
            {
                var stock = new int[names.Count];
                for (int i = 0; i < names.Count; i++)
                {
                    stock[i] = chest.GetInventory().CountItems(names[i]);
                }
                counts.Add(stock);
            }
            foreach (int index in PayPlan.ChestsNeeded(needed.ToArray(), carried, counts, everyChestWithStock, out _))
            {
                result.Add(chests[index]);
            }
            return result;
        }

        private static bool AllHeld(List<Container> chests)
        {
            foreach (var chest in chests)
            {
                if (!IsHeld(chest))
                {
                    return false;
                }
            }
            return true;
        }

        // ------------------------------------------------------------ crafting

        private sealed class CraftTicket
        {
            public Recipe Recipe;
            public ItemDrop.ItemData Upgrade;
            public int Variant;
            public bool Multi;
            public int MultiAmount;
            public CraftingStation Station;
            public List<Container> Chests;

            public static CraftTicket Of(InventoryGui gui, Player player)
            {
                return new CraftTicket
                {
                    Recipe = gui.m_craftRecipe, Upgrade = gui.m_craftUpgradeItem, Variant = gui.m_craftVariant,
                    Multi = gui.m_multiCrafting, MultiAmount = gui.m_multiCraftAmount, Station = player.GetCurrentCraftingStation()
                };
            }

            public bool StillWanted(InventoryGui gui, Player player)
            {
                return InventoryGui.IsVisible() && gui.m_craftRecipe == Recipe && gui.m_craftUpgradeItem == Upgrade &&
                       gui.m_craftVariant == Variant && gui.m_multiCrafting == Multi && (!Multi || gui.m_multiCraftAmount == MultiAmount) &&
                       player.GetCurrentCraftingStation() == Station && !player.IsDead();
            }

            public int Quality => Upgrade == null ? 1 : Upgrade.m_quality + 1;
            public int Multiplier => Multi ? MultiAmount : 1;
        }

        private static bool CraftCostsNothing(Player player)
        {
            return player.NoCostCheat() || (ZoneSystem.instance && ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost));
        }

        private static void DropTicket()
        {
            if (_ticket == null)
            {
                return;
            }
            foreach (var chest in _ticket.Chests)
            {
                if (!CraftChests.Contains(chest))
                {
                    Release(chest);
                }
            }
            _ticket = null;
        }

        /// <summary>
        ///     A craft waiting for chests: once they're all held, it's crafted. If one is refused or doesn't answer, the
        ///     craft is still tried with what is held, so a cost the bag, tabs and backpack can pay on their own still
        ///     works; the game's own check (counting only held chests) decides, so a short cost makes nothing and
        ///     takes nothing.
        /// </summary>
        private static void ResolveTicket()
        {
            var ticket = _ticket;
            var gui = InventoryGui.instance;
            var player = Player.m_localPlayer;
            if (!gui || !ticket.StillWanted(gui, player))
            {
                DropTicket();
                return;
            }
            LeaseOutcome outcome = AllHeld(ticket.Chests) ? LeaseOutcome.AllHeld : Book.Outcome(ticket.Chests);
            if (outcome == LeaseOutcome.Waiting)
            {
                return;
            }
            _ticket = null;
            CraftChests.Clear();
            CraftChests.AddRange(ticket.Chests);
            _replaying = true;
            _paid = false;
            try
            {
                gui.DoCrafting(player);
            }
            finally
            {
                _replaying = false;
                ReleaseCraftChests();
            }
            if (!_paid && outcome == LeaseOutcome.Failed)
            {
                player.Message(MessageHud.MessageType.Center, "A chest you need is in use by someone else: nothing was crafted or taken.");
            }
        }

        /// <summary>The craft went through (the game pays after making the item): for the "nothing crafted" message.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources))]
        private static class Paid
        {
            private static void Prefix()
            {
                if (_replaying)
                {
                    _paid = true;
                }
            }
        }

        /// <summary>Pressing Craft: ask for the chests now, so they're usually held by the time the craft bar finishes.</summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnCraftPressed))]
        private static class CraftPressed
        {
            private static void Postfix(InventoryGui __instance)
            {
                if (!ModConfig.ContainersEnabled.Value || __instance.m_craftTimer < 0f || !__instance.m_craftRecipe || !Player.m_localPlayer)
                {
                    return;
                }
                Safe.Run("reserve nearby chests for crafting", () =>
                {
                    var player = Player.m_localPlayer;
                    var ticket = CraftTicket.Of(__instance, player);
                    if (ticket.Recipe.m_requireOnlyOneIngredient || CraftCostsNothing(player))
                    {
                        return;
                    }
                    // A new craft replaces one still waiting for chests. These are kept through the craft bar and
                    // given back when the craft is done or abandoned.
                    DropTicket();
                    var chests = Plan(player, ticket.Recipe.m_resources, ticket.Quality, ticket.Multiplier, false);
                    ReleaseCraftChests();
                    CraftChests.AddRange(chests);
                    Ask(chests, 0.0);
                });
            }
        }

        /// <summary>
        ///     The craft itself: if it needs chests that aren't all held yet, it waits for them (skipping the game's
        ///     method, so nothing is made or taken); otherwise the game crafts, counting and paying only held chests.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
        private static class Craft
        {
            [HarmonyPriority(Priority.High)]
            private static bool Prefix(InventoryGui __instance, Player __0)
            {
                if (_replaying || !ModConfig.ContainersEnabled.Value || !__instance.m_craftRecipe || !__0 || __0 != Player.m_localPlayer)
                {
                    return true;
                }
                try
                {
                    var ticket = CraftTicket.Of(__instance, __0);
                    if (ticket.Recipe.m_requireOnlyOneIngredient || CraftCostsNothing(__0))
                    {
                        return true;
                    }
                    DropTicket();
                    var chests = Plan(__0, ticket.Recipe.m_resources, ticket.Quality, ticket.Multiplier, false);
                    foreach (var chest in chests)
                    {
                        if (!CraftChests.Contains(chest))
                        {
                            CraftChests.Add(chest);
                        }
                    }
                    if (chests.Count == 0 || AllHeld(chests))
                    {
                        return true;
                    }
                    ticket.Chests = chests;
                    _ticket = ticket;
                    Ask(chests, 0.0);
                    __0.Message(MessageHud.MessageType.TopLeft, "Waiting for the nearby chests…");
                    return false;
                }
                catch (System.Exception ex)
                {
                    Safe.Report("check nearby chests before crafting", ex);
                    return true;
                }
            }

            private static void Finalizer()
            {
                if (!_replaying && _ticket == null)
                {
                    ReleaseCraftChests();
                }
            }
        }

        // ------------------------------------------------------------ building

        private static void PollBuilding()
        {
            var player = Player.m_localPlayer;
            if (!ModConfig.ContainersEnabled.Value || !ModConfig.ContainersForBuilding.Value || !player.InPlaceMode() || player.m_noPlacementCost)
            {
                return;
            }
            var piece = player.GetSelectedPiece();
            if (!piece || (ZoneSystem.instance && ZoneSystem.instance.GetGlobalKey(piece.FreeBuildKey())))
            {
                return;
            }
            Ask(Plan(player, piece.m_resources, 0, 1, true), BuildLingerSeconds);
        }

        // ------------------------------------------------------------ the chest's answer

        /// <summary>Our request's answer: taken here, so it never opens the chest window or says "in use".</summary>
        [HarmonyPatch(typeof(Container), nameof(Container.RPC_OpenResponse))]
        private static class OpenResponse
        {
            private static bool Prefix(Container __instance, bool __1)
            {
                if (!__instance || !AskedAt.ContainsKey(__instance))
                {
                    return true;
                }
                var state = Book.State(__instance);
                if (state == LeaseState.Granted || state == LeaseState.Held)
                {
                    // Already answered: this is the player opening the chest themselves.
                    return true;
                }
                // Our answer, or a late one after we stopped waiting.
                AskedAt.Remove(__instance);
                if (Book.OnResponse(__instance, __1) && __1 && __instance.m_nview && __instance.m_nview.IsValid() && __instance.m_nview.IsOwner())
                {
                    // Already ours (we asked ourselves): mark it in use at once, before anyone else's request is handled.
                    Advance(__instance, Now);
                }
                return false;
            }
        }
    }
}
