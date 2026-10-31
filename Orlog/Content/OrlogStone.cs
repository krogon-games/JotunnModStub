using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The Orlog stone (<c>class-design.md</c> §5a): where a character chooses its class, in the spawn area.
    ///     <list type="bullet">
    ///         <item>
    ///             Look: a clone of the game's boss trophy stone (the start area's own stones), without the trophy stand,
    ///             glowing in the classes UI's rune blue as brightly as a boss stone glows once its boss is beaten.
    ///         </item>
    ///         <item>
    ///             Placed once per world, by the first game that loads the spawn area: in the widest gap of the ring of
    ///             trophy stones (<see cref="StonePlacement" />), after the server confirms no stone exists yet. A
    ///             persistent object, like the rip heart.
    ///         </item>
    ///         <item>
    ///             Using it without a class raises the request the classes UI (CraftingOverhaul) opens its class choice
    ///             window on; without that UI, using it twice chooses the only playable class. With a class, it says so.
    ///         </item>
    ///         <item>A raven explains it the first time a player comes near, as vanilla ravens do.</item>
    ///     </list>
    /// </summary>
    internal static class OrlogStone
    {
        public const string Prefab = "orlog_stone";
        private const string RpcPlace = "Orlog_PlaceStone";
        private const string RpcPlaceReply = "Orlog_PlaceStoneReply";
        private const string UiPluginGuid = "com.krogongames.craftingoverhaul";
        // The classes UI's rune colour (CraftingOverhaul UiKit.Rune), so the stone matches the class screens.
        private static readonly Color Rune = new Color(0.43f, 0.78f, 1f);
        // How close the trophy stones and the player must be to the start location's centre (metres). The ring is
        // found among the loaded trophy stones; this only keeps far-away ones out.
        private const float StartAreaRadius = 50f;

        /// <summary>Goes up each time the local player uses the stone without a class (<see cref="Api.GetClassStoneRequest" />).</summary>
        public static int Requests { get; private set; }

        private static Vector3 s_usedAt;
        private static bool s_used;
        private static float s_confirmUntil;
        private static bool s_placed;
        private static bool s_pending;
        private static float s_nextCheck;
        private static bool s_serverGranted;

        // ------------------------------------------------------------------ the prefab

        public static void Register()
        {
            var bossStone = PrefabManager.Cache.GetPrefabs(typeof(GameObject)).Values.OfType<GameObject>()
                .Where(p => p && p.GetComponent<BossStone>() && p.GetComponent<ZNetView>()).OrderBy(p => p.name).FirstOrDefault();
            if (!bossStone)
            {
                Jotunn.Logger.LogError("Orlog stone: no boss trophy stone found in the game's prefabs; the stone won't exist.");
                return;
            }
            var prefab = PrefabManager.Instance.CreateClonedPrefab(Prefab, bossStone.name);
            var vanilla = prefab.GetComponent<BossStone>();
            var stone = prefab.AddComponent<OrlogStoneObject>();
            stone.m_mesh = vanilla.m_mesh;
            stone.m_emissiveMaterialIndex = vanilla.m_emissiveMaterialIndex;
            Color active = vanilla.m_activeEmissiveColor;
            float brightness = Mathf.Max(active.r, Mathf.Max(active.g, active.b));
            stone.m_glow = Rune * (brightness > 0f ? brightness : 1f);

            // Only the stone stays: no trophy stand, no "boss defeated" effects.
            foreach (var stand in prefab.GetComponentsInChildren<ItemStand>(true))
            {
                if (stand && stand.gameObject != prefab)
                {
                    Object.DestroyImmediate(stand.gameObject);
                }
            }
            if (vanilla.m_activeEffect && vanilla.m_activeEffect != prefab)
            {
                Object.DestroyImmediate(vanilla.m_activeEffect);
            }
            Object.DestroyImmediate(vanilla);

            AddRaven(prefab, stone.m_mesh);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
            Jotunn.Logger.LogInfo($"Orlog stone made from {bossStone.name}.");
        }

        /// <summary>A raven guide point on top of the stone, with the vanilla raven from any existing guide point.</summary>
        private static void AddRaven(GameObject prefab, Renderer mesh)
        {
            var source = PrefabManager.Cache.GetPrefabs(typeof(GameObject)).Values.OfType<GameObject>()
                .SelectMany(p => p ? p.GetComponentsInChildren<GuidePoint>(true) : new GuidePoint[0])
                .FirstOrDefault(g => g && g.m_ravenPrefab);
            if (!source)
            {
                Jotunn.Logger.LogWarning("Orlog stone: no raven found in the game's prefabs; the stone has no raven hint.");
                return;
            }
            var point = new GameObject("GuidePoint");
            point.transform.SetParent(prefab.transform, false);
            point.transform.localPosition = TopOf(prefab, mesh) + Vector3.up * 0.2f;
            var guide = point.AddComponent<GuidePoint>();
            guide.m_ravenPrefab = source.m_ravenPrefab;
            guide.m_text = new Raven.RavenText
            {
                m_key = "orlog_stone",
                m_topic = Texts.RavenTopic,
                m_label = Texts.RavenLabel,
                m_text = Texts.RavenText,
                m_priority = source.m_text?.m_priority ?? 0,
                m_isTutorial = true,
                m_munin = false,
                // Once per player, like the Riftforge's raven.
                m_alwaysSpawn = false
            };
        }

        /// <summary>The top of the stone's mesh in the prefab's own space (for the raven to sit on).</summary>
        private static Vector3 TopOf(GameObject prefab, Renderer mesh)
        {
            var filter = mesh ? mesh.GetComponent<MeshFilter>() : null;
            if (!filter || !filter.sharedMesh)
            {
                return Vector3.up * 3f;
            }
            var bounds = filter.sharedMesh.bounds;
            var top = mesh.transform.TransformPoint(bounds.center + Vector3.up * bounds.extents.y);
            return prefab.transform.InverseTransformPoint(top);
        }

        // ------------------------------------------------------------------ placing it once per world

        /// <summary>Every frame (cheap): once the spawn area is loaded around the local player, place the stone if the world has none.</summary>
        public static void Tick()
        {
            if (s_placed || s_pending || Time.time < s_nextCheck)
            {
                return;
            }
            s_nextCheck = Time.time + 2f;
            Safe.Run("place the Orlog stone", () =>
            {
                var player = Player.m_localPlayer;
                if (!player || !ZoneSystem.instance || !Game.instance || !ZNetScene.instance || ZRoutedRpc.instance == null)
                {
                    return;
                }
                if (!ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out Vector3 center) ||
                    FlatDistance(player.transform.position, center) > StartAreaRadius)
                {
                    return;
                }
                if (OrlogStoneObject.All.Any(s => s && FlatDistance(s.transform.position, center) < StartAreaRadius))
                {
                    s_placed = true;
                    return;
                }
                var stones = Object.FindObjectsByType<BossStone>(FindObjectsSortMode.None).Where(b => FlatDistance(b.transform.position, center) < StartAreaRadius).ToList();
                var spot = StonePlacement.Place(center.x, center.z, stones.Select(b => b.transform.position.x).ToList(),
                    stones.Select(b => b.transform.position.z).ToList());
                if (!spot.HasValue)
                {
                    return;
                }
                // The trophy stones stand on the temple floor: stand on the same height.
                float y = stones.Average(b => b.transform.position.y);
                var package = new ZPackage();
                package.Write(new Vector3(spot.Value.X, y, spot.Value.Z));
                package.Write(spot.Value.Yaw);
                s_pending = true;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), RpcPlace, package);
            });
        }

        /// <summary>Server: allow the first request of a world with no stone; refuse the rest.</summary>
        private static void RPC_Place(long sender, ZPackage package)
        {
            if (!ZNet.instance || !ZNet.instance.IsServer())
            {
                return;
            }
            Safe.Run("decide where the Orlog stone goes", () =>
            {
                var position = package.ReadVector3();
                float yaw = package.ReadSingle();
                var zdos = new List<ZDO>();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(Prefab, zdos, ref index))
                {
                }
                bool place = zdos.Count == 0 && !s_serverGranted;
                s_serverGranted |= place;
                var reply = new ZPackage();
                reply.Write(place);
                reply.Write(position);
                reply.Write(yaw);
                ZRoutedRpc.instance.InvokeRoutedRPC(sender, RpcPlaceReply, reply);
            });
        }

        private static void RPC_PlaceReply(long sender, ZPackage package)
        {
            Safe.Run("place the Orlog stone", () =>
            {
                bool place = package.ReadBool();
                var position = package.ReadVector3();
                float yaw = package.ReadSingle();
                s_pending = false;
                s_placed = true;
                if (!place)
                {
                    return;
                }
                // Persistent like any placed object: it stays in the world when this game leaves the area.
                Object.Instantiate(ZNetScene.instance.GetPrefab(Prefab), position, Quaternion.Euler(0f, yaw, 0f));
                Jotunn.Logger.LogInfo($"Orlog stone placed at {position} in the spawn area.");
            });
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static class RegisterRpcs
        {
            private static void Postfix()
            {
                // A new world or server: look again.
                s_placed = false;
                s_pending = false;
                s_serverGranted = false;
                s_used = false;
                Safe.Run("register the Orlog stone messages", () =>
                {
                    ZRoutedRpc.instance.Register<ZPackage>(RpcPlace, RPC_Place);
                    ZRoutedRpc.instance.Register<ZPackage>(RpcPlaceReply, RPC_PlaceReply);
                });
            }
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        // ------------------------------------------------------------------ using it

        private static bool UiPresent => Chainloader.PluginInfos.ContainsKey(UiPluginGuid);

        public static void Use(Player player, OrlogStoneObject stone)
        {
            var cls = Attributes.ClassOf(player);
            if (cls != null)
            {
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize(Texts.StoneHasClass, cls.Name));
                return;
            }
            s_used = true;
            s_usedAt = stone.transform.position;
            if (UiPresent)
            {
                // The classes UI opens its class choice window when this goes up.
                Requests++;
                Progress.NotifyChanged();
                return;
            }
            // Without the classes UI: with one playable class, using the stone twice chooses it.
            var only = Attributes.Classes.Playable.ToList();
            if (only.Count != 1)
            {
                player.Message(MessageHud.MessageType.Center, Texts.StoneNeedsUi);
                return;
            }
            if (Time.time < s_confirmUntil)
            {
                s_confirmUntil = 0f;
                Choose(only[0].Name);
                return;
            }
            s_confirmUntil = Time.time + 10f;
            player.Message(MessageHud.MessageType.Center, Localization.instance.Localize(Texts.StoneConfirm, only[0].Name));
        }

        /// <summary>
        ///     Gives the local player <paramref name="classId" />: only without a class, for a playable class, and after
        ///     using the stone and staying near it (twice the game's interaction reach). "" on success, otherwise why not.
        /// </summary>
        public static string Choose(string classId)
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready)
            {
                return "No character";
            }
            if (Attributes.ClassOf(player) != null)
            {
                return "You already have a class";
            }
            var cls = Attributes.Classes.Find(classId);
            if (cls == null || !cls.Playable)
            {
                return "Not a playable class";
            }
            if (!s_used || Vector3.Distance(player.transform.position, s_usedAt) > player.m_maxInteractDistance * 2f)
            {
                return "Use the Orlog stone to choose a class";
            }
            Attributes.SetClass(player, cls);
            s_used = false;
            player.Message(MessageHud.MessageType.Center, Localization.instance.Localize(Texts.ClassChosen, cls.Name));
            Transform head = player.m_head ? player.m_head : player.transform;
            player.m_skillLevelupEffects.Create(head.position, head.rotation, head);
            Jotunn.Logger.LogInfo($"Chose the class {cls.Name} at the Orlog stone.");
            return "";
        }
    }

    /// <summary>The stone in the world: its glow, hover text and use.</summary>
    internal class OrlogStoneObject : MonoBehaviour, Hoverable, Interactable
    {
        public static readonly List<OrlogStoneObject> All = new List<OrlogStoneObject>();

        public MeshRenderer m_mesh;
        public int m_emissiveMaterialIndex;
        public Color m_glow;

        private void Awake()
        {
            Safe.Run("light the Orlog stone", () =>
            {
                if (!m_mesh)
                {
                    return;
                }
                var materials = m_mesh.materials;
                if (m_emissiveMaterialIndex >= 0 && m_emissiveMaterialIndex < materials.Length)
                {
                    materials[m_emissiveMaterialIndex].EnableKeyword("_EMISSION");
                    materials[m_emissiveMaterialIndex].SetColor("_EmissionColor", m_glow);
                }
            });
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize(Texts.StoneName);
        }

        public float GetHoverOffset()
        {
            return 0f;
        }

        public string GetHoverText()
        {
            var player = Player.m_localPlayer;
            var cls = player ? Attributes.ClassOf(player) : null;
            string line = cls != null
                ? Localization.instance.Localize(Texts.StoneHasClass, cls.Name)
                : Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + Texts.StoneUse);
            return GetHoverName() + "\n" + line;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || !(user is Player player) || player != Player.m_localPlayer || !Progress.Ready)
            {
                return false;
            }
            Safe.Run("use the Orlog stone", () => OrlogStone.Use(player, this));
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
