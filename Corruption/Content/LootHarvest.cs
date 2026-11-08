using System.Collections.Generic;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     On-harvest gathering affixes: both Bountifuls, Determined, Artisan, Gemseeker's, Stonecutter's, Resinous,
    ///     Forester's, Charring, and Odin's Grace on trees (AFFIX_RULES.md → Gathering tool affixes).
    ///     <para>
    ///         Breaking and dropping happen on the object's owner, who can't see the striker's tool. So before each hit
    ///         the striker sends the owner its tool's chances (<see cref="RpcChances" />; routed messages from one sender
    ///         arrive in order, so it lands before the hit). The owner keeps them per object and sender, and while that
    ///         sender's hit runs (the objects' <c>RPC_Damage</c> / <c>RPC_Hit</c>) it extends the object's drop list
    ///         (<c>DropTable.GetDropList</c>) and spawns everything itself. Determined goes back to the striker
    ///         (<see cref="RpcHarvested" />). Decision: DECISIONS.md → Architecture, 2026-11-01.
    ///     </para>
    /// </summary>
    internal static class LootHarvest
    {
        private const string RpcChances = "Corruption_HarvestChances";
        private const string RpcHarvested = "Corruption_Harvested";

        /// <summary>ZDO key: the tree a log or stump came from (Odin's Grace harvests every loaded piece with the same ID).</summary>
        private const string TreeIdKey = "Corruption_TreeId";

        // The extra items the user's affixes name (AFFIX_RULES: "1 extra stone", "1 extra resin", wood "turns into coal").
        private const string StonePrefab = "Stone";
        private const string ResinPrefab = "Resin";
        private const string CoalPrefab = "Coal";

        private static readonly System.Func<double> Random01 = () => UnityEngine.Random.value * 0.99999f;

        /// <summary>Wood: what the game's fallen logs and stumps drop.</summary>
        private static readonly HashSet<string> Wood = new HashSet<string>();

        /// <summary>Seeds: the items planting a sapling or crop costs (requirements of the pieces that are <c>Plant</c>s).</summary>
        private static readonly HashSet<string> Seeds = new HashSet<string>();

        /// <summary>Ores: what the coal-fired smelters (smelter, blast furnace) take.</summary>
        private static readonly HashSet<string> Ores = new HashSet<string>();

        // ---- Item sets from the game's data ----

        public static void CollectItems()
        {
            Wood.Clear();
            Seeds.Clear();
            Ores.Clear();
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(TreeLog)).Values)
            {
                var log = obj as TreeLog;
                if (log)
                {
                    AddDrops(log.m_dropWhenDestroyed, Wood);
                }
            }
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(TreeBase)).Values)
            {
                var tree = obj as TreeBase;
                var stump = tree && tree.m_stubPrefab ? tree.m_stubPrefab.GetComponent<DropOnDestroyed>() : null;
                if (stump)
                {
                    AddDrops(stump.m_dropWhenDestroyed, Wood);
                }
            }
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(Plant)).Values)
            {
                var piece = obj is Plant plant ? plant.GetComponent<Piece>() : null;
                if (!piece || piece.m_resources == null)
                {
                    continue;
                }
                foreach (var requirement in piece.m_resources)
                {
                    if (requirement != null && requirement.m_resItem)
                    {
                        Seeds.Add(requirement.m_resItem.gameObject.name);
                    }
                }
            }
            foreach (var obj in PrefabManager.Cache.GetPrefabs(typeof(Smelter)).Values)
            {
                var smelter = obj as Smelter;
                if (!smelter || !smelter.m_fuelItem || smelter.m_fuelItem.gameObject.name != CoalPrefab || smelter.m_conversion == null)
                {
                    continue;
                }
                foreach (var conversion in smelter.m_conversion)
                {
                    if (conversion != null && conversion.m_from)
                    {
                        Ores.Add(conversion.m_from.gameObject.name);
                    }
                }
            }
            Jotunn.Logger.LogInfo($"Harvest affixes: wood {string.Join(", ", Wood)}; seeds {string.Join(", ", Seeds)}; ores {string.Join(", ", Ores)}.");
        }

        private static void AddDrops(global::DropTable table, HashSet<string> into)
        {
            if (table?.m_drops == null)
            {
                return;
            }
            foreach (var drop in table.m_drops)
            {
                if (drop.m_item)
                {
                    into.Add(drop.m_item.name);
                }
            }
        }

        // ---- Messages ----

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpcs
        {
            private static void Postfix()
            {
                Safe.Run("register the harvest messages", () =>
                {
                    Pending.Clear();
                    Armed.Clear();
                    ZRoutedRpc.instance.Register<ZPackage>(RpcChances, RPC_Chances);
                    ZRoutedRpc.instance.Register<float>(RpcHarvested, RPC_Harvested);
                });
            }
        }

        private static void Write(ZPackage pkg, HarvestChances c)
        {
            pkg.Write(c.Bounty);
            pkg.Write(c.WoodBounty);
            pkg.Write(c.Stamina);
            pkg.Write(c.Ore);
            pkg.Write(c.Gem);
            pkg.Write(c.Stone);
            pkg.Write(c.Resin);
            pkg.Write(c.Seed);
            pkg.Write(c.Charring);
            pkg.Write(c.Odin);
        }

        private static HarvestChances Read(ZPackage pkg)
        {
            return new HarvestChances
            {
                Bounty = pkg.ReadSingle(), WoodBounty = pkg.ReadSingle(), Stamina = pkg.ReadSingle(), Ore = pkg.ReadSingle(),
                Gem = pkg.ReadSingle(), Stone = pkg.ReadSingle(), Resin = pkg.ReadSingle(), Seed = pkg.ReadSingle(),
                Charring = pkg.ReadSingle(), Odin = pkg.ReadBool()
            };
        }

        /// <summary>Determined, on the striker: the owner reports a harvest.</summary>
        private static void RPC_Harvested(long sender, float percent)
        {
            Safe.Run("apply Determined", () =>
            {
                var player = Player.m_localPlayer;
                if (player && percent > 0f)
                {
                    player.AddStamina(player.GetMaxStamina() * percent / 100f);
                }
            });
        }

        // ---- Striker: send the tool's chances before the hit ----

        /// <summary>Objects this player last sent non-zero chances for, so a later hit without them clears them.</summary>
        private static readonly HashSet<ZDOID> Armed = new HashSet<ZDOID>();

        private static int _treeOdinSwing = -1;

        private static HarvestChances ChancesOf(ItemDrop.ItemData tool, bool woodcutting)
        {
            var c = new HarvestChances
            {
                Bounty = Loot.Total(tool, "harvest_double_chance"),
                WoodBounty = Loot.Total(tool, "wood_double_chance"),
                Stamina = Loot.Total(tool, "stamina_percent_on_harvest"),
                Ore = Loot.Total(tool, "extra_ore"),
                Gem = Loot.Total(tool, "gem_find"),
                Stone = Loot.Total(tool, "extra_stone"),
                Resin = Loot.Total(tool, "extra_resin"),
                Seed = Loot.Total(tool, "tree_seed_drop"),
                Charring = Loot.Total(tool, "wood_to_coal")
            };
            // Odin's Grace on trees: one roll per swing, for the first tree, log or stump it hits (as on deposits,
            // where LootGathering rolls it once per swing).
            if (woodcutting && _treeOdinSwing != LootGathering.SwingNumber)
            {
                _treeOdinSwing = LootGathering.SwingNumber;
                c.Odin = GatheringRules.Roll(Loot.Total(tool, "harvest_instantly"), Random01);
            }
            return c;
        }

        /// <summary>
        ///     Runs on the striker's game in the objects' <c>Damage</c>, before the hit goes to the owner. A gathering hit
        ///     (chop damage on trees, pickaxe damage on rock) from the local player's swing sends the tool's chances; any
        ///     other hit by the local player on an object it armed sends a clear.
        /// </summary>
        private static void BeforeHit(ZNetView view, HitData hit, bool woodcutting)
        {
            var player = Player.m_localPlayer;
            if (hit == null || !player || !view || !view.IsValid() || hit.m_attacker != player.GetZDOID())
            {
                return;
            }
            var tool = LootGathering.SwingTool;
            bool gathering = woodcutting ? hit.m_damage.m_chop > 0f : hit.m_damage.m_pickaxe > 0f;
            var chances = gathering && tool != null ? ChancesOf(tool, woodcutting) : null;
            var zdo = view.GetZDO();
            if (chances != null && chances.Any)
            {
                Armed.Add(zdo.m_uid);
                Send(zdo, chances);
            }
            else if (Armed.Remove(zdo.m_uid))
            {
                Send(zdo, new HarvestChances());
            }
        }

        private static void Send(ZDO zdo, HarvestChances chances)
        {
            var pkg = new ZPackage();
            pkg.Write(zdo.m_uid);
            Write(pkg, chances);
            ZRoutedRpc.instance.InvokeRoutedRPC(zdo.GetOwner(), RpcChances, pkg);
        }

        [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.Damage))]
        private static class StrikeTree
        {
            private static void Prefix(TreeBase __instance, HitData hit) =>
                Safe.Run("send harvest chances (tree)", () => BeforeHit(__instance.m_nview, hit, true));
        }

        [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Damage))]
        private static class StrikeLog
        {
            private static void Prefix(TreeLog __instance, HitData hit) =>
                Safe.Run("send harvest chances (log)", () => BeforeHit(__instance.m_nview, hit, true));
        }

        [HarmonyPatch(typeof(Destructible), nameof(Destructible.Damage))]
        private static class StrikeDestructible
        {
            private static void Prefix(Destructible __instance, HitData hit) =>
                Safe.Run("send harvest chances (rock or stump)",
                    () => BeforeHit(__instance.m_nview, hit, __instance.GetDestructibleType() == DestructibleType.Tree));
        }

        [HarmonyPatch(typeof(MineRock), nameof(MineRock.Damage))]
        private static class StrikeRock
        {
            private static void Prefix(MineRock __instance, HitData hit) =>
                Safe.Run("send harvest chances (rock)", () => BeforeHit(__instance.m_nview, hit, false));
        }

        [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Damage))]
        private static class StrikeDeposit
        {
            private static void Prefix(MineRock5 __instance, HitData hit)
            {
                Safe.Run("send harvest chances (deposit)", () =>
                {
                    BeforeHit(__instance.m_nview, hit, false);
                    // A rock that breaks into a deposit (e.g. rock4_copper → its _frac) hits the new deposit itself, from
                    // the owner's own game: the deposit inherits the chances of the hit that broke the rock.
                    var view = __instance.m_nview;
                    if (_current != null && view && view.IsValid())
                    {
                        var key = new Key(view.GetZDO().m_uid, ZDOMan.GetSessionID());
                        if (!Pending.ContainsKey(key))
                        {
                            Pending[key] = _current.Chances.WithoutOdin();
                        }
                    }
                });
            }
        }

        // ---- co_own (testing) ----

        public static void RegisterCommand()
        {
            CommandManager.Instance.AddConsoleCommand(new OwnCommand());
        }

        /// <summary>
        ///     co_own: your game takes over running the object you're looking at (a tree, log, stump or rock), so a
        ///     two-player test can give each player's game a different piece of one tree (playtest: Odin's Grace on one
        ///     tree from two games). Uses the game's own ownership claim; nothing else changes.
        /// </summary>
        private sealed class OwnCommand : ConsoleCommand
        {
            public override string Name => "co_own";

            public override string Help => "Corruption: co_own - your game takes over running the object you're looking at (testing)";

            public override bool IsCheat => true;

            public override void Run(string[] args)
            {
                var player = Player.m_localPlayer;
                var camera = GameCamera.instance ? GameCamera.instance.transform : null;
                if (!player || !camera)
                {
                    return;
                }
                ZNetView view = null;
                float best = float.MaxValue;
                foreach (var hit in Physics.RaycastAll(camera.position, camera.forward, 30f))
                {
                    var found = hit.collider ? hit.collider.GetComponentInParent<ZNetView>() : null;
                    if (found && found.IsValid() && !found.GetComponent<Character>() && hit.distance < best)
                    {
                        best = hit.distance;
                        view = found;
                    }
                }
                if (!view)
                {
                    Console.instance.Print("Look at a tree, log, stump or rock first.");
                    return;
                }
                bool had = view.IsOwner();
                view.ClaimOwnership();
                Console.instance.Print($"{Utils.GetPrefabName(view.gameObject)}: " +
                                       (had ? "your game already runs it." : "your game runs it now (another player's game ran it)."));
            }
        }

        // ---- Owner: keep the chances, apply them while that sender's hit runs ----

        private struct Key : System.IEquatable<Key>
        {
            public readonly ZDOID Target;
            public readonly long Sender;

            public Key(ZDOID target, long sender)
            {
                Target = target;
                Sender = sender;
            }

            public bool Equals(Key other) => Target == other.Target && Sender == other.Sender;
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => Target.GetHashCode() * 397 ^ Sender.GetHashCode();
        }

        /// <summary>
        ///     The chances each sender last sent for each object. Replaced by the sender's next message for that object,
        ///     removed by a clear, and dropped once the object is gone or owned by someone else.
        /// </summary>
        private static readonly Dictionary<Key, HarvestChances> Pending = new Dictionary<Key, HarvestChances>();

        private static void RPC_Chances(long sender, ZPackage pkg)
        {
            Safe.Run("receive harvest chances", () =>
            {
                var target = pkg.ReadZDOID();
                var chances = Read(pkg);
                var key = new Key(target, sender);
                if (!chances.Any)
                {
                    Pending.Remove(key);
                    return;
                }
                Prune();
                chances.Striker = sender;
                Pending[key] = chances;
            });
        }

        private static readonly List<Key> Stale = new List<Key>();

        private static void Prune()
        {
            Stale.Clear();
            foreach (var key in Pending.Keys)
            {
                var zdo = ZDOMan.instance.GetZDO(key.Target);
                if (zdo == null || !zdo.IsOwner())
                {
                    Stale.Add(key);
                }
            }
            foreach (var key in Stale)
            {
                Pending.Remove(key);
            }
        }

        /// <summary>The hit being handled on the owner's game: whose chances, and which drop table they apply to.</summary>
        private sealed class Context
        {
            public HarvestChances Chances;
            public global::DropTable Table;
            public bool Woodcutting;
            public Vector3 Position;

            /// <summary>The hit this context belongs to (a nested hit on another object has its own).</summary>
            public HitData Hit;

            /// <summary>The hit carries the tool's damage for this object: chop on wood, pickaxe on rock (read before resistances).</summary>
            public bool RightTool;

            /// <summary>The struck object's (or deposit part's) health; negative infinity once it's gone.</summary>
            public System.Func<float> Health;

            public float HealthBefore = float.NegativeInfinity;
        }

        private static Context _current;

        private static int _playerPrefab;

        /// <summary>
        ///     A hit made by a player: a swing, or a copy of one (Seismic, Splintering, Odin's Grace on rock, the hit a
        ///     breaking rock passes to its copper deposit), all of which keep the striker as attacker. Hits with no
        ///     attacker (a log rolling or landing, a falling tree hitting another, <c>ImpactEffect</c>) and creatures' hits
        ///     are not: what they break gives normal drops, even if a player sent chances for it earlier (Krogon,
        ///     2026-11-08: "only your swings"). Without this, such a break used the cached chances only when the striker's
        ///     own game owned the object (the physics hit's sender then matches the striker).
        /// </summary>
        private static bool ByPlayer(HitData hit)
        {
            if (hit == null || hit.m_attacker.IsNone())
            {
                return false;
            }
            if (_playerPrefab == 0 && Game.instance && Game.instance.m_playerPrefab)
            {
                _playerPrefab = Game.instance.m_playerPrefab.name.GetStableHashCode();
            }
            var zdo = ZDOMan.instance != null ? ZDOMan.instance.GetZDO(hit.m_attacker) : null;
            if (zdo != null && _playerPrefab != 0)
            {
                return zdo.GetPrefab() == _playerPrefab;
            }
            return hit.GetAttacker() is Player;
        }

        /// <summary>
        ///     Starts the hit's context when its sender sent chances for this object and the hit is a player's
        ///     (<see cref="ByPlayer" />); returns the previous context.
        /// </summary>
        private static Context Begin(ZNetView view, long sender, HitData hit, global::DropTable table, bool woodcutting, Vector3 position,
            System.Func<float> health)
        {
            var previous = _current;
            Safe.Run("start a harvest", () =>
            {
                // Set even without a drop table: a rock that drops nothing may still break into a deposit that inherits it.
                if (!view || !view.IsValid() || !Pending.TryGetValue(new Key(view.GetZDO().m_uid, sender), out var chances))
                {
                    return;
                }
                if (!ByPlayer(hit))
                {
                    Jotunn.Logger.LogInfo($"Harvest affixes: {Utils.GetPrefabName(view.gameObject)} was hit without a player's swing " +
                                          "(no attacker, or a creature); cached tool chances not used.");
                    return;
                }
                var context = new Context
                {
                    Chances = chances, Table = table, Woodcutting = woodcutting, Position = position, Hit = hit, Health = health,
                    RightTool = woodcutting ? hit.m_damage.m_chop > 0f : hit.m_damage.m_pickaxe > 0f
                };
                context.HealthBefore = health();
                _current = context;
            });
            return previous;
        }

        /// <summary>
        ///     After the owner has applied a hit: Stonecutter's or Resinous roll once if the hit was valid (the right tool,
        ///     and it did damage: the struck object or part lost health or broke). The extra stone or resin drops at the
        ///     hit point, spawned by the owner like the object's own drops (Krogon, 2026-11-08).
        /// </summary>
        private static void AfterHit(HitData hit)
        {
            var context = _current;
            if (context == null || !ReferenceEquals(context.Hit, hit) || context.Health == null)
            {
                return;
            }
            Safe.Run("roll Stonecutter's or Resinous", () =>
            {
                float after = context.Health();
                PerHit(context.Chances, context.Woodcutting, context.RightTool, context.HealthBefore > 0f && after < context.HealthBefore, hit);
            });
        }

        private static void PerHit(HarvestChances chances, bool woodcutting, bool rightTool, bool didDamage, HitData hit)
        {
            if (!ObjectDB.instance)
            {
                return;
            }
            var items = new HarvestItems<GameObject>
            {
                Stone = ObjectDB.instance.GetItemPrefab(StonePrefab),
                Resin = ObjectDB.instance.GetItemPrefab(ResinPrefab)
            };
            var extra = HarvestRules.PerHit(chances, woodcutting, rightTool, didDamage, items, Random01);
            if (!extra)
            {
                return;
            }
            // Just off the surface, towards the striker, so it doesn't spawn inside the rock or trunk.
            var back = hit.m_dir.sqrMagnitude > 0.01f ? -hit.m_dir.normalized * 0.3f : Vector3.zero;
            var go = Object.Instantiate(extra, hit.m_point + back + Vector3.up * 0.3f, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
            ItemDrop.OnCreateNew(go);
        }

        /// <summary>A ZDO-saved health, or negative infinity once the object is gone (it broke or was removed).</summary>
        private static System.Func<float> SavedHealth(ZNetView view, int key, System.Func<float> fallback)
        {
            return () => view && view.IsValid() ? view.GetZDO().GetFloat(key, fallback()) : float.NegativeInfinity;
        }

        /// <summary>Odin's Grace on a tree, log or stump: takes the one-strike flag and harvests the whole tree. True when done.</summary>
        private static bool OdinsGrace(Component piece, ZNetView view, long sender, HitData hit)
        {
            if (!view || !view.IsValid() || !view.IsOwner() || hit == null || hit.m_damage.m_chop <= 0f ||
                !Pending.TryGetValue(new Key(view.GetZDO().m_uid, sender), out var chances) || !chances.Odin || !ByPlayer(hit))
            {
                return false;
            }
            chances.Odin = false;
            bool done = false;
            var outer = _current;
            _current = null;
            try
            {
                done = WholeTree.Harvest(piece, view, hit, chances);
                if (done)
                {
                    // The strike harvested the tree: a damaging chop hit, so Resinous rolls once for it too.
                    PerHit(chances, true, true, true, hit);
                }
            }
            catch (System.Exception ex)
            {
                Safe.Report("harvest a whole tree (Odin's Grace)", ex);
            }
            finally
            {
                _current = outer;
            }
            return done;
        }

        [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage))]
        private static class TreeHit
        {
            private static bool Prefix(TreeBase __instance, long sender, HitData hit, out Context __state)
            {
                __state = _current;
                if (OdinsGrace(__instance, __instance.m_nview, sender, hit))
                {
                    return false;
                }
                var tree = __instance;
                __state = Begin(tree.m_nview, sender, hit, tree.m_dropWhenDestroyed, true, tree.transform.position,
                    SavedHealth(tree.m_nview, ZDOVars.s_health, () => tree.m_health));
                return true;
            }

            private static void Postfix(HitData hit) => AfterHit(hit);

            private static void Finalizer(Context __state) => _current = __state;
        }

        [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.RPC_Damage))]
        private static class LogHit
        {
            private static bool Prefix(TreeLog __instance, long sender, HitData hit, out Context __state)
            {
                __state = _current;
                if (OdinsGrace(__instance, __instance.m_nview, sender, hit))
                {
                    return false;
                }
                var log = __instance;
                __state = Begin(log.m_nview, sender, hit, log.m_dropWhenDestroyed, true, log.transform.position,
                    SavedHealth(log.m_nview, ZDOVars.s_health, () => 0f));
                return true;
            }

            private static void Postfix(HitData hit) => AfterHit(hit);

            private static void Finalizer(Context __state) => _current = __state;
        }

        [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
        private static class DestructibleHit
        {
            private static bool Prefix(Destructible __instance, long sender, HitData hit, out Context __state)
            {
                __state = _current;
                bool tree = false;
                DropOnDestroyed drops = null;
                try
                {
                    tree = __instance.GetDestructibleType() == DestructibleType.Tree;
                    drops = __instance.GetComponent<DropOnDestroyed>();
                }
                catch (System.Exception ex)
                {
                    Safe.Report("read a rock or stump for the harvest affixes", ex);
                    return true;
                }
                if (tree && !__instance.m_destroyed && OdinsGrace(__instance, __instance.m_nview, sender, hit))
                {
                    return false;
                }
                var target = __instance;
                // The default is vanilla's own: health grows with the world level (Destructible.RPC_Damage).
                var saved = SavedHealth(target.m_nview, ZDOVars.s_health,
                    () => target.m_health + Game.m_worldLevel * target.m_health * (Game.instance ? Game.instance.m_worldLevelMineHPMultiplier : 0f));
                __state = Begin(target.m_nview, sender, hit, drops ? drops.m_dropWhenDestroyed : null, tree, target.transform.position,
                    () => target.m_destroyed ? float.NegativeInfinity : saved());
                return true;
            }

            private static void Postfix(HitData hit) => AfterHit(hit);

            private static void Finalizer(Context __state) => _current = __state;
        }

        [HarmonyPatch(typeof(MineRock), nameof(MineRock.RPC_Hit))]
        private static class RockHit
        {
            private static void Prefix(MineRock __instance, long sender, HitData hit, int hitAreaIndex, out Context __state)
            {
                var rock = __instance;
                var saved = SavedHealth(rock.m_nview, ("Health" + hitAreaIndex).GetStableHashCode(), () => rock.GetHealth());
                __state = Begin(rock.m_nview, sender, hit, rock.m_dropItems, false, rock.transform.position,
                    () => rock.GetHitArea(hitAreaIndex) == null ? float.NegativeInfinity : saved());
            }

            private static void Postfix(HitData hit) => AfterHit(hit);

            private static void Finalizer(Context __state) => _current = __state;
        }

        /// <summary>Each part that breaks is a harvest, including parts that fall when the hit leaves them unsupported.</summary>
        [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.RPC_Damage))]
        private static class DepositHit
        {
            private static void Prefix(MineRock5 __instance, long sender, HitData hit, int hitAreaIndex, out Context __state)
            {
                var deposit = __instance;
                __state = Begin(deposit.m_nview, sender, hit, deposit.m_dropItems, false, deposit.transform.position, () =>
                {
                    if (!deposit.m_nview || !deposit.m_nview.IsValid())
                    {
                        return float.NegativeInfinity;
                    }
                    deposit.LoadHealth();
                    var area = deposit.GetHitArea(hitAreaIndex);
                    return area == null ? float.NegativeInfinity : area.m_health;
                });
            }

            /// <summary>Only the part struck counts for Stonecutter's: parts that fall for lack of support aren't hits.</summary>
            private static void Postfix(HitData hit) => AfterHit(hit);

            private static void Finalizer(Context __state) => _current = __state;
        }

        /// <summary>The drop list of the object being broken, while its hit's context is set: one harvest event.</summary>
        [HarmonyPatch(typeof(global::DropTable), nameof(global::DropTable.GetDropList), new System.Type[0])]
        private static class HarvestDrops
        {
            private static void Postfix(global::DropTable __instance, ref List<GameObject> __result)
            {
                var context = _current;
                if (context == null || !ReferenceEquals(__instance, context.Table) || __result == null)
                {
                    return;
                }
                try
                {
                    __result = Harvest(__result, context.Chances, __instance, context.Woodcutting, context.Position);
                }
                catch (System.Exception ex)
                {
                    Safe.Report("apply the harvest affixes", ex);
                }
            }
        }

        /// <summary>One harvest event: the drop list with the chances applied; Determined reported to the striker.</summary>
        private static List<GameObject> Harvest(List<GameObject> drops, HarvestChances chances, global::DropTable table, bool woodcutting,
            Vector3 position)
        {
            if (drops.Count == 0)
            {
                return drops;
            }
            var items = new HarvestItems<GameObject>
            {
                IsWood = go => go && Wood.Contains(go.name),
                IsOre = go => go && Ores.Contains(go.name),
                Stone = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(StonePrefab) : null,
                Resin = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(ResinPrefab) : null,
                Coal = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(CoalPrefab) : null
            };
            if (woodcutting && table.m_drops != null)
            {
                foreach (var drop in table.m_drops)
                {
                    if (drop.m_item && Seeds.Contains(drop.m_item.name))
                    {
                        items.Seed = drop.m_item;
                        break;
                    }
                }
            }
            if (!woodcutting && chances.Gem > 0f && ObjectDB.instance)
            {
                foreach (var gem in HarvestRules.GemPool(Loot.BiomeAt(position)))
                {
                    var prefab = ObjectDB.instance.GetItemPrefab(gem);
                    if (prefab)
                    {
                        items.Gems.Add(prefab);
                    }
                }
            }
            var result = HarvestRules.Apply(drops, chances, woodcutting, items, Random01);
            if (chances.Stamina > 0f)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(chances.Striker, RpcHarvested, chances.Stamina);
            }
            return result;
        }

        // ---- Tree IDs: which logs and stump came from one tree ----

        private static List<ZNetView> _spawned;

        private sealed class Tagging
        {
            public List<ZNetView> Spawned;
            public ZDOID Id;
        }

        /// <summary>Collects the logs and stumps created while a tree falls or a log splits.</summary>
        [HarmonyPatch]
        private static class PieceCreated
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Awake))]
            private static void Log(TreeLog __instance) => _spawned?.Add(__instance.GetComponent<ZNetView>());

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Destructible), nameof(Destructible.Awake))]
            private static void Stump(Destructible __instance) => _spawned?.Add(__instance.GetComponent<ZNetView>());
        }

        private static Tagging StartTagging(ZDOID id)
        {
            var tagging = new Tagging { Spawned = _spawned, Id = id };
            _spawned = new List<ZNetView>();
            return tagging;
        }

        /// <summary>Stamps the tree ID on what was created, and restores the outer collection.</summary>
        private static void FinishTagging(Tagging tagging)
        {
            if (tagging == null)
            {
                return;
            }
            var spawned = _spawned;
            _spawned = tagging.Spawned;
            Safe.Run("tag a tree's logs and stump", () =>
            {
                foreach (var view in spawned)
                {
                    var zdo = view ? view.GetZDO() : null;
                    if (zdo != null && !tagging.Id.IsNone())
                    {
                        zdo.Set(TreeIdKey, tagging.Id);
                    }
                }
            });
        }

        /// <summary>A falling tree: its log and stump get the tree's ID (the standing tree's own ZDOID).</summary>
        [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.SpawnLog))]
        private static class TreeFalls
        {
            private static void Prefix(TreeBase __instance, out Tagging __state)
            {
                __state = null;
                var view = __instance.m_nview;
                if (view && view.IsValid())
                {
                    __state = StartTagging(view.GetZDO().m_uid);
                }
            }

            private static void Finalizer(Tagging __state) => FinishTagging(__state);
        }

        /// <summary>A splitting log: its pieces get its tree ID (or, for a log from before the mod, the log's own ZDOID).</summary>
        [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Destroy))]
        private static class LogSplits
        {
            private static void Prefix(TreeLog __instance, out Tagging __state)
            {
                __state = null;
                var view = __instance.m_nview;
                if (view && view.IsValid())
                {
                    var id = view.GetZDO().GetZDOID(TreeIdKey);
                    __state = StartTagging(id.IsNone() ? view.GetZDO().m_uid : id);
                }
            }

            private static void Finalizer(Tagging __state) => FinishTagging(__state);
        }

        // ---- Odin's Grace on trees ----

        /// <summary>
        ///     Harvests a whole tree at once on the owner's game (AFFIX_RULES → Interpretations): a standing tree drops its
        ///     own drops, its log and everything the log would split into, and its stump; a log or stump harvests every
        ///     loaded piece with its tree ID plus what each log would still split into (only itself and its splits when it
        ///     has no ID). Pieces not spawned yet are worked out from the prefabs. Every piece is one harvest event, and
        ///     the tool tier check applies to each.
        /// </summary>
        private static class WholeTree
        {
            public static bool Harvest(Component piece, ZNetView view, HitData hit, HarvestChances chances)
            {
                if (piece is TreeBase tree)
                {
                    return Standing(tree, view, hit, chances);
                }
                if (!Allowed(piece, hit))
                {
                    return false;
                }
                var id = view.GetZDO().GetZDOID(TreeIdKey);
                var pieces = new List<ZNetView> { view };
                if (!id.IsNone())
                {
                    foreach (var pair in ZNetScene.instance.m_instances)
                    {
                        if (pair.Value && pair.Value != view && pair.Key != null && pair.Key.GetZDOID(TreeIdKey) == id)
                        {
                            pieces.Add(pair.Value);
                        }
                    }
                }
                // Two-player check (playtest: Odin's Grace on one tree from two games): which tree, and how many of its
                // pieces another player's game was running. Both games logging the same tree at once = a double harvest.
                int others = 0;
                foreach (var each in pieces)
                {
                    if (each && each.IsValid() && !each.IsOwner())
                    {
                        others++;
                    }
                }
                Jotunn.Logger.LogInfo($"Odin's Grace: whole-tree harvest of tree {(id.IsNone() ? view.GetZDO().m_uid : id)} from " +
                                      $"{Utils.GetPrefabName(view.gameObject)}: {pieces.Count} loaded piece(s), {others} run by another player's game.");
                foreach (var each in pieces)
                {
                    Real(each, hit, chances);
                }
                return true;
            }

            private static bool Allowed(Component piece, HitData hit)
            {
                if (piece is TreeLog log)
                {
                    return hit.CheckToolTier(log.m_minToolTier, alwaysAllowTierZero: true);
                }
                if (piece is Destructible destructible)
                {
                    return hit.CheckToolTier(destructible.m_minToolTier);
                }
                return false;
            }

            private static bool Standing(TreeBase tree, ZNetView view, HitData hit, HarvestChances chances)
            {
                if (view.GetZDO().GetFloat(ZDOVars.s_health, tree.m_health) <= 0f ||
                    !hit.CheckToolTier(tree.m_minToolTier, alwaysAllowTierZero: true))
                {
                    return false;
                }
                var t = tree.transform;
                var origin = t.position + Vector3.up * tree.m_spawnYOffset;
                Spawn(tree.m_dropWhenDestroyed, chances, t.position, i =>
                {
                    var circle = Random.insideUnitCircle * 0.5f;
                    return origin + new Vector3(circle.x, tree.m_spawnYStep * i, circle.y);
                });
                var ancestry = new HashSet<GameObject>();
                if (tree.m_logPrefab && tree.m_logSpawnPoint)
                {
                    Virtual(tree.m_logPrefab, Matrix4x4.TRS(tree.m_logSpawnPoint.position, tree.m_logSpawnPoint.rotation, t.localScale), hit,
                        chances, ancestry);
                }
                if (tree.m_stubPrefab)
                {
                    Virtual(tree.m_stubPrefab, Matrix4x4.TRS(t.position, t.rotation, t.localScale), hit, chances, ancestry);
                }
                tree.m_destroyedEffect.Create(t.position, t.rotation, t);
                tree.gameObject.SetActive(false);
                view.Destroy();
                return true;
            }

            /// <summary>A loaded log or stump: its drops, a log's splits, then it's removed (ownership claimed first).</summary>
            private static void Real(ZNetView view, HitData hit, HarvestChances chances)
            {
                if (!view || !view.IsValid())
                {
                    return;
                }
                var t = view.transform;
                var log = view.GetComponent<TreeLog>();
                var destructible = log ? null : view.GetComponent<Destructible>();
                if (log)
                {
                    if (!Allowed(log, hit) || view.GetZDO().GetFloat(ZDOVars.s_health, log.m_health) <= 0f)
                    {
                        return;
                    }
                    view.ClaimOwnership();
                    SpawnAlongLog(log.m_dropWhenDestroyed, chances, t.position, t.up, log.m_spawnDistance);
                    var ancestry = new HashSet<GameObject> { Prefab(view) };
                    if (log.m_subLogPrefab)
                    {
                        foreach (var point in log.m_subLogPoints)
                        {
                            if (point)
                            {
                                var rotation = log.m_useSubLogPointRotation ? point.rotation : t.rotation;
                                Virtual(log.m_subLogPrefab, Matrix4x4.TRS(point.position, rotation, t.localScale), hit, chances, ancestry);
                            }
                        }
                    }
                    log.m_destroyedEffect.Create(t.position, t.rotation, t);
                    ZNetScene.instance.Destroy(view.gameObject);
                }
                else if (destructible && destructible.GetDestructibleType() == DestructibleType.Tree && !destructible.m_destroyed)
                {
                    if (!Allowed(destructible, hit))
                    {
                        return;
                    }
                    view.ClaimOwnership();
                    var drops = destructible.GetComponent<DropOnDestroyed>();
                    if (drops)
                    {
                        SpawnLikeDropOnDestroyed(drops, chances, t.position);
                    }
                    var spawn = destructible.m_spawnWhenDestroyed;
                    if (spawn && (spawn.GetComponent<TreeLog>() || spawn.GetComponent<DropOnDestroyed>()))
                    {
                        // What it would break into is harvested too.
                        Virtual(spawn, Matrix4x4.TRS(t.position, t.rotation, t.localScale), hit, chances, new HashSet<GameObject> { Prefab(view) });
                    }
                    else if (spawn)
                    {
                        // Anything else (fragments) spawns as in Destructible.Destroy.
                        var go = Object.Instantiate(spawn, t.position, t.rotation);
                        var spawnedView = go.GetComponent<ZNetView>();
                        if (spawnedView)
                        {
                            spawnedView.SetLocalScale(t.localScale);
                        }
                        var gibber = go.GetComponent<Gibber>();
                        if (gibber)
                        {
                            gibber.Setup(hit.m_point, hit.m_dir);
                        }
                    }
                    destructible.m_destroyedEffect.Create(t.position, t.rotation, t);
                    destructible.m_destroyed = true;
                    ZNetScene.instance.Destroy(view.gameObject);
                }
            }

            /// <summary>
            ///     A piece that isn't spawned yet (the log of a standing tree, its stump, a log's splits), from its prefab
            ///     at the pose it would spawn at. <paramref name="ancestry" /> stops a prefab that splits into itself.
            /// </summary>
            private static void Virtual(GameObject prefab, Matrix4x4 pose, HitData hit, HarvestChances chances, HashSet<GameObject> ancestry)
            {
                if (!prefab || !ancestry.Add(prefab))
                {
                    return;
                }
                Vector3 position = pose.GetColumn(3);
                var log = prefab.GetComponent<TreeLog>();
                if (log)
                {
                    if (Allowed(log, hit))
                    {
                        SpawnAlongLog(log.m_dropWhenDestroyed, chances, position, pose.rotation * Vector3.up, log.m_spawnDistance);
                        if (log.m_subLogPrefab)
                        {
                            var root = prefab.transform.worldToLocalMatrix;
                            foreach (var point in log.m_subLogPoints)
                            {
                                if (!point)
                                {
                                    continue;
                                }
                                var world = pose * (root * point.localToWorldMatrix);
                                var rotation = log.m_useSubLogPointRotation ? world.rotation : pose.rotation;
                                Virtual(log.m_subLogPrefab, Matrix4x4.TRS(world.GetColumn(3), rotation, pose.lossyScale), hit, chances, ancestry);
                            }
                        }
                    }
                }
                else
                {
                    var destructible = prefab.GetComponent<Destructible>();
                    var drops = prefab.GetComponent<DropOnDestroyed>();
                    if (drops && (!destructible || Allowed(destructible, hit)))
                    {
                        SpawnLikeDropOnDestroyed(drops, chances, position);
                    }
                }
                ancestry.Remove(prefab);
            }

            private static GameObject Prefab(ZNetView view)
            {
                return ZNetScene.instance ? ZNetScene.instance.GetPrefab(view.GetZDO().GetPrefab()) : null;
            }

            /// <summary>Spawns like <c>TreeLog.Destroy</c>: along the log, a little higher for each item.</summary>
            private static void SpawnAlongLog(global::DropTable table, HarvestChances chances, Vector3 position, Vector3 up, float spread)
            {
                Spawn(table, chances, position, i => position + up * Random.Range(-spread, spread) + Vector3.up * 0.3f * i);
            }

            /// <summary>Spawns like <c>DropOnDestroyed.OnDestroyed</c>: above the ground, in a small circle, stepping up.</summary>
            private static void SpawnLikeDropOnDestroyed(DropOnDestroyed drops, HarvestChances chances, Vector3 position)
            {
                float ground = ZoneSystem.instance ? ZoneSystem.instance.GetGroundHeight(position) : position.y;
                if (position.y < ground)
                {
                    position.y = ground + 0.1f;
                }
                var origin = position + Vector3.up * drops.m_spawnYOffset;
                Spawn(drops.m_dropWhenDestroyed, chances, position, i =>
                {
                    var circle = Random.insideUnitCircle * 0.5f;
                    return origin + new Vector3(circle.x, drops.m_spawnYStep * i, circle.y);
                });
            }

            private static void Spawn(global::DropTable table, HarvestChances chances, Vector3 biomeAt, System.Func<int, Vector3> place)
            {
                if (table == null)
                {
                    return;
                }
                var drops = LootHarvest.Harvest(table.GetDropList(), chances, table, true, biomeAt);
                for (int i = 0; i < drops.Count; i++)
                {
                    if (drops[i])
                    {
                        var go = Object.Instantiate(drops[i], place(i), Quaternion.Euler(0f, Random.Range(0, 360), 0f));
                        ItemDrop.OnCreateNew(go);
                    }
                }
            }
        }
    }
}
