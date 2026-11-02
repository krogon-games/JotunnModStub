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
    ///             Look (user, 2026-11-02): a copy of the game's Vegvisir standing stone with every vanilla interaction
    ///             removed, twice as tall as a boss trophy stone, its runes glowing in the classes UI's rune blue, with blue
    ///             mist drifting round its base. If no Vegvisir is found, the trophy stone is the fallback.
    ///         </item>
    ///         <item>
    ///             Placed once per world (user, 2026-11-02): behind the Eikthyr trophy stone, on the line from the spawn
    ///             area's centre, a gap of <c>StoneGap</c> metres between the two stones, turned the way the Eikthyr stone
    ///             is. The player's game finds the spot; the server confirms no stone exists yet. A stone placed by an
    ///             earlier version is moved to the spot.
    ///         </item>
    ///         <item>
    ///             Using it without a class raises the request the classes UI (CraftingOverhaul) opens its class choice
    ///             window on; without that UI, using it twice chooses the only playable class. With a class, it says so.
    ///         </item>
    ///         <item>A raven on the ground beside it explains it the first time a player comes near.</item>
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
        private static bool s_built;
        // The stone's footprint radius in metres, at its final size.
        private static float s_footprint;

        // ------------------------------------------------------------------ the prefab

        /// <summary>
        ///     Builds the stone's prefab once, when the game's locations are available: a Vegvisir only exists inside
        ///     location prefabs, so they're searched (loaded one at a time, as the biome detection does).
        /// </summary>
        public static void Build()
        {
            if (s_built)
            {
                return;
            }
            var bossStone = PrefabManager.Cache.GetPrefabs(typeof(GameObject)).Values.OfType<GameObject>()
                .Where(p => p && p.GetComponent<BossStone>()).OrderBy(p => p.name).FirstOrDefault();
            var source = FindVegvisir(out string label);
            if (!source)
            {
                if (!bossStone)
                {
                    Jotunn.Logger.LogError("Orlog stone: no Vegvisir or trophy stone found in the game; the stone won't exist.");
                    return;
                }
                Jotunn.Logger.LogWarning("Orlog stone: no Vegvisir found in the game; using the trophy stone instead.");
                source = bossStone;
                label = bossStone.name;
            }
            s_built = true;

            var prefab = PrefabManager.Instance.CreateClonedPrefab(Prefab, source);
            prefab.SetActive(true);
            Strip(prefab);
            var nview = prefab.GetComponent<ZNetView>() ?? prefab.AddComponent<ZNetView>();
            nview.m_persistent = true;

            // Twice as tall as a trophy stone (user, 2026-11-02; StoneSize).
            float height = Bounds(prefab).size.y;
            float scale = StonePlacement.Scale(height, bossStone ? Bounds(bossStone).size.y : 0f, OrlogConfig.StoneSize.Value);
            prefab.transform.localScale *= scale;
            var bounds = Bounds(prefab);
            s_footprint = Mathf.Max(bounds.extents.x, bounds.extents.z);

            Glow(prefab, bounds);
            prefab.AddComponent<OrlogStoneObject>();
            float footprintUnscaled = s_footprint / Mathf.Max(0.01f, scale);
            AddMist(prefab, footprintUnscaled);
            AddRaven(prefab, footprintUnscaled + 1f / Mathf.Max(0.01f, scale));

            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
            if (ZNetScene.instance)
            {
                PrefabManager.Instance.RegisterToZNetScene(prefab);
            }
            Jotunn.Logger.LogInfo($"Orlog stone made from {label}, {bounds.size.y:0.0} m tall (x{scale:0.00}), footprint {s_footprint:0.0} m.");
        }

        /// <summary>A Vegvisir: from the loaded assets, else from the first location (by name) that has one.</summary>
        private static GameObject FindVegvisir(out string label)
        {
            var cached = PrefabManager.Cache.GetPrefabs(typeof(Vegvisir)).Values.OfType<Vegvisir>().Where(v => v)
                .OrderBy(v => v.name).FirstOrDefault();
            if (cached)
            {
                label = cached.name;
                return cached.gameObject;
            }
            label = "";
            if (!ZoneSystem.instance)
            {
                return null;
            }
            foreach (var location in ZoneSystem.instance.m_locations.Where(l => l.m_prefab.IsValid).OrderBy(l => l.m_prefabName))
            {
                GameObject root = null;
                try
                {
                    root = location.m_prefab.Load() == SoftReferenceableAssets.LoadResult.Succeeded ? location.m_prefab.Asset : null;
                }
                catch (System.Exception ex)
                {
                    Jotunn.Logger.LogDebug($"Orlog stone: could not load {location.m_prefabName}: {ex.Message}");
                }
                var vegvisir = root ? root.GetComponentInChildren<Vegvisir>(true) : null;
                if (vegvisir)
                {
                    // Kept loaded: the copy shares the location's meshes and materials.
                    label = $"{vegvisir.name} (in {location.m_prefabName})";
                    return vegvisir.gameObject;
                }
                if (root)
                {
                    location.m_prefab.Release();
                }
            }
            return null;
        }

        /// <summary>Removes every vanilla behaviour: hover and use (Vegvisir, rune stone lore), trophy stands, ravens, nested network objects.</summary>
        private static void Strip(GameObject prefab)
        {
            foreach (var stand in prefab.GetComponentsInChildren<ItemStand>(true))
            {
                if (stand && stand.gameObject != prefab)
                {
                    Object.DestroyImmediate(stand.gameObject);
                }
            }
            foreach (var boss in prefab.GetComponentsInChildren<BossStone>(true))
            {
                if (boss.m_activeEffect && boss.m_activeEffect != prefab)
                {
                    Object.DestroyImmediate(boss.m_activeEffect);
                }
                Object.DestroyImmediate(boss);
            }
            foreach (var behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is Hoverable || behaviour is Interactable || behaviour is GuidePoint)
                {
                    Object.DestroyImmediate(behaviour);
                }
            }
            foreach (var nview in prefab.GetComponentsInChildren<ZNetView>(true))
            {
                if (nview.gameObject != prefab)
                {
                    Object.DestroyImmediate(nview);
                }
            }
        }

        /// <summary>The model's bounds from its meshes, in metres, relative to its root (any scale on the root included).</summary>
        private static Bounds Bounds(GameObject root)
        {
            var bounds = new Bounds();
            bool any = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter.sharedMesh)
                {
                    continue;
                }
                var local = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = local.center + Vector3.Scale(local.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var point = filter.transform.TransformPoint(corner) - root.transform.position;
                    if (any)
                    {
                        bounds.Encapsulate(point);
                    }
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        any = true;
                    }
                }
            }
            return bounds;
        }

        /// <summary>Glowing blue runes: every glowing material turns rune blue at its own brightness; lights and particles too; plus a soft blue light.</summary>
        private static void Glow(GameObject prefab, Bounds bounds)
        {
            int glowing = 0;
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                var materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (!material || !material.HasProperty("_EmissionColor"))
                    {
                        continue;
                    }
                    var emission = material.GetColor("_EmissionColor");
                    float brightness = Mathf.Max(emission.r, Mathf.Max(emission.g, emission.b));
                    if (brightness <= 0.01f)
                    {
                        continue;
                    }
                    // A copy: the vanilla stones keep their own colour.
                    var copy = new Material(material) { name = material.name + "_orlog" };
                    copy.EnableKeyword("_EMISSION");
                    copy.SetColor("_EmissionColor", Rune * brightness);
                    materials[i] = copy;
                    changed = true;
                    glowing++;
                }
                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }
            foreach (var light in prefab.GetComponentsInChildren<Light>(true))
            {
                light.color = Rune;
            }
            foreach (var particles in prefab.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.startColor = new Color(Rune.r, Rune.g, Rune.b, main.startColor.color.a);
            }
            // A soft blue light at mid-height, reaching about the stone's own height around it.
            var lightObject = new GameObject("OrlogLight");
            lightObject.transform.SetParent(prefab.transform, false);
            lightObject.transform.position = prefab.transform.position + bounds.center;
            var glow = lightObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = Rune;
            glow.intensity = 1f;
            glow.range = Mathf.Max(2f, bounds.size.y);
            Jotunn.Logger.LogInfo($"Orlog stone: {glowing} glowing material(s) turned rune blue.");
        }

        /// <summary>Blue mist drifting round the base: the game's own smoke material, tinted rune blue.</summary>
        private static void AddMist(GameObject prefab, float footprintUnscaled)
        {
            var smoke = PrefabManager.Cache.GetPrefabs(typeof(ParticleSystemRenderer)).Values.OfType<ParticleSystemRenderer>()
                .Select(r => r ? r.sharedMaterial : null).Where(m => m && m.name.ToLowerInvariant().Contains("smoke"))
                .OrderBy(m => m.name).FirstOrDefault();
            if (!smoke)
            {
                Jotunn.Logger.LogWarning("Orlog stone: no smoke material found in the game; the stone has no mist.");
                return;
            }
            var mist = new GameObject("OrlogMist");
            mist.transform.SetParent(prefab.transform, false);
            var particles = mist.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startColor = new Color(Rune.r, Rune.g, Rune.b, 0.3f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            // The circle follows the stone's size; the puffs keep their own size.
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.gravityModifier = -0.01f;
            var emission = particles.emission;
            emission.rateOverTime = 8f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = footprintUnscaled;
            // Lay the circle flat on the ground, puffs rising.
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            mist.GetComponent<ParticleSystemRenderer>().sharedMaterial = smoke;
            Jotunn.Logger.LogInfo($"Orlog stone: mist made with the game's {smoke.name}.");
        }

        /// <summary>A raven guide point on the ground beside the stone (user: reachable), with the vanilla raven.</summary>
        private static void AddRaven(GameObject prefab, float sideUnscaled)
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
            point.transform.localPosition = new Vector3(sideUnscaled, 0f, 0f);
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

        // ------------------------------------------------------------------ placing it once per world

        /// <summary>
        ///     Every frame (cheap): once the spawn area is loaded around the local player, place the stone if the world has
        ///     none, or move one placed elsewhere by an earlier version.
        /// </summary>
        public static void Tick()
        {
            if (!s_built || s_placed || s_pending || Time.time < s_nextCheck)
            {
                return;
            }
            s_nextCheck = Time.time + 2f;
            Safe.Run("place the Orlog stone", () =>
            {
                var player = Player.m_localPlayer;
                if (!player || !ZoneSystem.instance || !Game.instance || !ZNetScene.instance || ZRoutedRpc.instance == null || !Progress.Ready)
                {
                    return;
                }
                if (!ZoneSystem.instance.GetLocationIcon(Game.instance.m_StartLocation, out Vector3 center) ||
                    FlatDistance(player.transform.position, center) > StartAreaRadius)
                {
                    return;
                }
                var eikthyr = FirstBossStone(center);
                if (!eikthyr)
                {
                    return;
                }
                float distance = StonePlacement.Distance(Footprint(eikthyr.gameObject), s_footprint, OrlogConfig.StoneGap.Value);
                var spot = StonePlacement.Behind(center.x, center.z, eikthyr.transform.position.x, eikthyr.transform.position.z, distance);
                if (!spot.HasValue)
                {
                    return;
                }
                var position = new Vector3(spot.Value.X, 0f, spot.Value.Z);
                position.y = ZoneSystem.instance.GetGroundHeight(position);
                // Turned the way the Eikthyr stone is, so it faces the same way (StoneTurn adds degrees).
                float yaw = eikthyr.transform.eulerAngles.y + OrlogConfig.StoneTurn.Value;

                var existing = OrlogStoneObject.All.FirstOrDefault(s => s && FlatDistance(s.transform.position, center) < StartAreaRadius + distance);
                if (existing)
                {
                    MoveTo(existing, position, yaw);
                    s_placed = true;
                    return;
                }
                var package = new ZPackage();
                package.Write(position);
                package.Write(yaw);
                s_pending = true;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), RpcPlace, package);
            });
        }

        /// <summary>
        ///     The trophy stone of the levelling guide's first boss (Eikthyr): the loaded trophy stone near the centre whose
        ///     stand takes that boss's trophy (<c>Trophy&lt;boss&gt;</c>), else the one named after the boss.
        /// </summary>
        private static BossStone FirstBossStone(Vector3 center)
        {
            var first = Progress.Biomes.All.OrderBy(b => b.BossLevel).FirstOrDefault(b => b.Bosses.Length > 0);
            if (first == null)
            {
                return null;
            }
            string boss = first.Bosses[0];
            string trophy = "Trophy" + boss;
            var stones = Object.FindObjectsByType<BossStone>(FindObjectsSortMode.None)
                .Where(b => b && FlatDistance(b.transform.position, center) < StartAreaRadius).ToList();
            return stones.FirstOrDefault(b => b.m_itemStand && b.m_itemStand.m_supportedItems != null &&
                                              b.m_itemStand.m_supportedItems.Any(i => i && i.name == trophy))
                   ?? stones.FirstOrDefault(b => b.name.IndexOf(boss, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>A placed object's footprint radius in metres, from its renderers.</summary>
        private static float Footprint(GameObject placed)
        {
            float radius = 0f;
            foreach (var renderer in placed.GetComponentsInChildren<Renderer>())
            {
                if (renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                var b = renderer.bounds;
                radius = Mathf.Max(radius, Mathf.Max(Mathf.Abs(b.max.x - placed.transform.position.x), Mathf.Abs(b.min.x - placed.transform.position.x)));
                radius = Mathf.Max(radius, Mathf.Max(Mathf.Abs(b.max.z - placed.transform.position.z), Mathf.Abs(b.min.z - placed.transform.position.z)));
            }
            return radius;
        }

        /// <summary>Moves a stone from an earlier version to the spot (claiming it first, as every change to an object must).</summary>
        private static void MoveTo(OrlogStoneObject stone, Vector3 position, float yaw)
        {
            var nview = stone.GetComponent<ZNetView>();
            if (!nview || !nview.IsValid())
            {
                return;
            }
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            if (Vector3.Distance(stone.transform.position, position) < 0.5f && Quaternion.Angle(stone.transform.rotation, rotation) < 2f)
            {
                return;
            }
            nview.ClaimOwnership();
            stone.transform.SetPositionAndRotation(position, rotation);
            nview.GetZDO().SetPosition(position);
            nview.GetZDO().SetRotation(rotation);
            Jotunn.Logger.LogInfo($"Orlog stone moved to {position} behind the Eikthyr stone.");
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

    /// <summary>The stone in the world: its hover text and use.</summary>
    internal class OrlogStoneObject : MonoBehaviour, Hoverable, Interactable
    {
        public static readonly List<OrlogStoneObject> All = new List<OrlogStoneObject>();

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
