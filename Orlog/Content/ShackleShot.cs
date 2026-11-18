using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     Shackle Shot (Krogon, 2026-11-18; <c>docs/handoffs/claude-amazon-snipe-shackle-shot-m67buh.md</c>, decisions
    ///     11-18), after Dota 2's Windranger's Shackleshot. The arrow hits for its share of the shot
    ///     (<see cref="BowSkills" />); then, on the shooter's game, the enemy it struck is shackled to the nearest other
    ///     enemy or standing tree behind it (within <c>reach_m</c> and <c>cone_deg</c> of the line from the shooter,
    ///     <see cref="ActiveSkillRules.ShackleAnchor" />). Both creatures are stunned for <c>shackle_s</c>; with nothing
    ///     behind it the struck one alone for <c>alone_s</c>. Bosses are only slowed, for the same time.
    ///     <para>
    ///         The stun is a status effect of ours (<see cref="StunName" />) added through the game's
    ///         <c>SEMan.AddStatusEffect</c>, which sends it to the game that runs the creature (as Inner Sight's mark);
    ///         there, <c>Character.CanMove</c> answers false while it lasts. The game's own code then stops the creature
    ///         walking, flying and turning, and <c>Humanoid.StartAttack</c> refuses new attacks (an attack already
    ///         swinging finishes). Swimming reads status-effect speed instead, so the stun also carries the game's
    ///         Immobilized speed (−1000). The boss's slow is the game's Tarred slow (−50% speed) on its own.
    ///     </para>
    ///     <para>
    ///         The look (Claude's first version, no new art): a rope between the two (the game's harpoon line's material
    ///         where it is found) and gold swirls circling above each stunned head. Drawn on the shooter's game only,
    ///         like the burning ground's ring: other players see the creatures stand still, not the rope or the swirls.
    ///     </para>
    /// </summary>
    internal static class ShackleShot
    {
        public const string Id = "ShackleShot";

        public const string StunName = "Orlog_Shackled";
        public const string SlowName = "Orlog_ShackleSlow";
        public static readonly int StunHash = StunName.GetStableHashCode();
        public static readonly int SlowHash = SlowName.GetStableHashCode();

        /// <summary>A boss's slow (decision 14): the game's Tarred speed (−50%, game dump status_effects.csv).</summary>
        private const float BossSlow = -0.5f;

        private static readonly List<Character> s_enemies = new List<Character>();
        private static readonly List<Transform> s_trees = new List<Transform>();
        private static readonly List<float> s_offsetX = new List<float>();
        private static readonly List<float> s_offsetZ = new List<float>();
        private static readonly HashSet<TreeBase> s_seenTrees = new HashSet<TreeBase>();
        private static readonly Collider[] s_overlap = new Collider[128];
        private static int s_treeMask;

        public static bool IsOurs(int hash)
        {
            return hash == StunHash || hash == SlowHash;
        }

        /// <summary>Registers the stun and the boss's slow (their seconds come with each shackle: <c>StatusEffect.SetLevel</c>).</summary>
        public static void RegisterStatusEffects()
        {
            var stun = ScriptableObject.CreateInstance<SE_Stats>();
            stun.name = StunName;
            stun.m_name = "Shackled";
            // The game's Immobilized speed too: swimming (Character.UpdateSwimming) reads status-effect speed, not CanMove.
            stun.m_speedModifier = -1000f;
            stun.m_jumpModifier = new Vector3(-1f, -1f, -1f);
            stun.m_ttl = 2f;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(stun, false));

            var slow = ScriptableObject.CreateInstance<SE_Stats>();
            slow.name = SlowName;
            slow.m_name = "Shackled";
            slow.m_speedModifier = BossSlow;
            slow.m_ttl = 2f;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(slow, false));
        }

        /// <summary>The arrow struck <paramref name="struck" /> (from <c>Projectile.OnHit</c>, on the shooter's game).</summary>
        public static void Shackle(Character owner, BowSkills.ShotTag tag, Character struck)
        {
            if (!owner || owner != Player.m_localPlayer)
            {
                return;
            }
            // Only an arrow that hit a living enemy shackles; one in the ground, a tree or a friend does nothing more.
            if (!struck || struck == owner || struck.IsPlayer() || struck.IsDead() || !BaseAI.IsEnemy(owner, struck))
            {
                Jotunn.Logger.LogInfo("Orlog: Shackle Shot hit no living enemy: no shackle.");
                return;
            }
            var v = SkillBar.ValuesFor(Attributes.ClassOf(Player.m_localPlayer));
            if (v == null)
            {
                return;
            }
            float reach = v.Get(Id, "reach_m", tag.Level);
            float cone = v.Get(Id, "cone_deg", tag.Level);
            Vector3 from = struck.transform.position;
            Vector3 line = from - owner.transform.position;

            // The candidates: enemies first, then standing trees (a tie goes to an enemy).
            s_enemies.Clear();
            s_trees.Clear();
            s_offsetX.Clear();
            s_offsetZ.Clear();
            foreach (var c in Character.GetAllCharacters())
            {
                if (!c || c == struck || c == owner || c.IsPlayer() || c.IsDead() || !BaseAI.IsEnemy(owner, c))
                {
                    continue;
                }
                Vector3 offset = c.transform.position - from;
                if (offset.x * offset.x + offset.z * offset.z > reach * reach)
                {
                    continue;
                }
                s_enemies.Add(c);
                s_offsetX.Add(offset.x);
                s_offsetZ.Add(offset.z);
            }
            if (s_treeMask == 0)
            {
                s_treeMask = LayerMask.GetMask("Default", "static_solid", "Default_small");
            }
            s_seenTrees.Clear();
            int found = Physics.OverlapSphereNonAlloc(from, reach, s_overlap, s_treeMask);
            for (int i = 0; i < found; i++)
            {
                var tree = s_overlap[i] ? s_overlap[i].GetComponentInParent<TreeBase>() : null;
                if (!tree || !s_seenTrees.Add(tree))
                {
                    continue;
                }
                Vector3 offset = tree.transform.position - from;
                s_trees.Add(tree.transform);
                s_offsetX.Add(offset.x);
                s_offsetZ.Add(offset.z);
            }
            int anchor = ActiveSkillRules.ShackleAnchor(s_offsetX, s_offsetZ, line.x, line.z, reach, cone);
            Character anchorEnemy = anchor >= 0 && anchor < s_enemies.Count ? s_enemies[anchor] : null;
            Transform anchorTree = anchor >= s_enemies.Count ? s_trees[anchor - s_enemies.Count] : null;

            float seconds = anchor >= 0 ? v.Get(Id, "shackle_s", tag.Level) : v.Get(Id, "alone_s", tag.Level);
            Hold(struck, seconds);
            if (anchorEnemy)
            {
                Hold(anchorEnemy, seconds);
            }
            ShackleLook.Show(struck, anchorEnemy, anchorTree, seconds);
            string to = anchorEnemy ? "to " + anchorEnemy.m_name : anchorTree ? "to a tree (" + Utils.GetPrefabName(anchorTree.gameObject) + ")" : "alone (nothing behind it)";
            Jotunn.Logger.LogInfo($"Orlog: Shackle Shot (level {tag.Level}): {struck.m_name}{(struck.IsBoss() ? " (boss: slowed)" : "")} shackled {to}" +
                                  $"{(anchorEnemy && anchorEnemy.IsBoss() ? " (boss: slowed)" : "")} for {seconds:0.##} s " +
                                  $"({s_enemies.Count} enemies and {s_trees.Count} trees within {reach:0.#} m, cone {cone:0}°).");
        }

        /// <summary>Stuns a creature (a boss: slows it) for some seconds, on the game that runs it.</summary>
        private static void Hold(Character c, float seconds)
        {
            var seman = c ? c.GetSEMan() : null;
            if (seman == null || seconds <= 0f)
            {
                return;
            }
            seman.AddStatusEffect(c.IsBoss() ? SlowHash : StunHash, true, 0, seconds);
        }

        /// <summary>
        ///     A shackled creature can't move: the game then stops its walking, flying and turning, and refuses
        ///     its new attacks (<c>Humanoid.StartAttack</c> checks <c>CanMove</c>). Runs on the game that runs the creature.
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.CanMove))]
        private static class Stunned
        {
            private static void Postfix(Character __instance, ref bool __result)
            {
                if (!__result || __instance is Player)
                {
                    return;
                }
                var seman = __instance.m_seman;
                if (seman != null && seman.HaveStatusEffect(StunHash))
                {
                    __result = false;
                }
            }
        }

        // ------------------------------------------------------------ the look

        /// <summary>The rope between the shackled two and the swirls above the stunned, on the shooter's game.</summary>
        internal sealed class ShackleLook : MonoBehaviour
        {
            private const int RopePoints = 10;
            private const float RopeSag = 0.2f;
            private const int Swirls = 3;
            private const int SwirlPoints = 6;
            private static readonly Color RopeColour = new Color(0.55f, 0.42f, 0.28f);
            private static readonly Color SwirlColour = new Color(1f, 0.85f, 0.45f);
            private static Material s_rope;
            private static bool s_ropeLooked;
            private static bool s_ropeIsHarpoon;

            private Character m_struck;
            private Character m_anchor;
            private Transform m_tree;
            private float m_treeHeight;
            private float m_until;
            private LineRenderer m_line;
            private readonly List<LineRenderer> m_swirlsStruck = new List<LineRenderer>();
            private readonly List<LineRenderer> m_swirlsAnchor = new List<LineRenderer>();

            public static void Show(Character struck, Character anchor, Transform tree, float seconds)
            {
                var go = new GameObject("Orlog_Shackle");
                var look = go.AddComponent<ShackleLook>();
                look.m_struck = struck;
                look.m_anchor = anchor;
                look.m_tree = tree;
                look.m_treeHeight = struck.GetCenterPoint().y - struck.transform.position.y;
                look.m_until = Time.time + seconds;
                if (anchor || tree)
                {
                    look.m_line = look.NewLine(RopeMaterial(), 0.07f, s_ropeIsHarpoon ? Color.white : RopeColour, RopePoints);
                    if (s_ropeIsHarpoon)
                    {
                        look.m_line.textureMode = LineTextureMode.Tile;
                    }
                }
                if (!struck.IsBoss())
                {
                    look.AddSwirls(look.m_swirlsStruck);
                }
                if (anchor && !anchor.IsBoss())
                {
                    look.AddSwirls(look.m_swirlsAnchor);
                }
            }

            /// <summary>The game's harpoon line's material (Harpooned's start effect), else a plain one.</summary>
            private static Material RopeMaterial()
            {
                if (s_ropeLooked)
                {
                    return s_rope;
                }
                s_ropeLooked = true;
                Safe.Run("find the harpoon line's material", () =>
                {
                    var harpooned = ObjectDB.instance ? ObjectDB.instance.GetStatusEffect("Harpooned".GetStableHashCode()) : null;
                    var prefabs = harpooned != null && harpooned.m_startEffects != null ? harpooned.m_startEffects.m_effectPrefabs : null;
                    if (prefabs != null)
                    {
                        foreach (var effect in prefabs)
                        {
                            var line = effect != null && effect.m_prefab ? effect.m_prefab.GetComponentInChildren<LineRenderer>(true) : null;
                            if (line && line.sharedMaterial)
                            {
                                s_rope = line.sharedMaterial;
                                s_ropeIsHarpoon = true;
                                break;
                            }
                        }
                    }
                });
                if (!s_rope)
                {
                    s_rope = BowSkills.ForkLine.Material();
                }
                Jotunn.Logger.LogInfo("Orlog: Shackle Shot's rope is drawn with " + (s_ropeIsHarpoon ? "the harpoon line's material (" + s_rope.name + ")." : "a plain material (the harpoon's line wasn't found)."));
                return s_rope;
            }

            private LineRenderer NewLine(Material material, float width, Color colour, int points)
            {
                var go = new GameObject("Orlog_ShackleLine");
                go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = material;
                line.useWorldSpace = true;
                line.widthMultiplier = width;
                line.startColor = colour;
                line.endColor = colour;
                line.positionCount = points;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                return line;
            }

            private void AddSwirls(List<LineRenderer> into)
            {
                var material = BowSkills.ForkLine.Material();
                if (!material)
                {
                    return;
                }
                for (int i = 0; i < Swirls; i++)
                {
                    into.Add(NewLine(material, 0.05f, SwirlColour, SwirlPoints));
                }
            }

            private void LateUpdate()
            {
                if (Time.time > m_until || !m_struck || m_struck.IsDead() || (m_anchor && m_anchor.IsDead()))
                {
                    Destroy(gameObject);
                    return;
                }
                if (m_line)
                {
                    Vector3 a = m_struck.GetCenterPoint();
                    Vector3 b = m_anchor ? m_anchor.GetCenterPoint() : m_tree ? m_tree.position + Vector3.up * m_treeHeight : a;
                    for (int i = 0; i < RopePoints; i++)
                    {
                        float t = i / (RopePoints - 1f);
                        // A little slack: the middle hangs lowest.
                        m_line.SetPosition(i, Vector3.Lerp(a, b, t) + Vector3.down * (RopeSag * 4f * t * (1f - t)));
                    }
                }
                PlaceSwirls(m_struck, m_swirlsStruck);
                if (m_anchor)
                {
                    PlaceSwirls(m_anchor, m_swirlsAnchor);
                }
            }

            /// <summary>Short gold arcs circling above the head, the usual "stunned" sign.</summary>
            private static void PlaceSwirls(Character c, List<LineRenderer> swirls)
            {
                if (swirls.Count == 0)
                {
                    return;
                }
                Bounds body = c.m_collider ? c.m_collider.bounds : new Bounds(c.transform.position + Vector3.up, Vector3.one * 2f);
                float radius = Mathf.Clamp(Mathf.Max(body.extents.x, body.extents.z) * 0.7f, 0.25f, 1.5f);
                Vector3 centre = new Vector3(body.center.x, body.max.y + 0.25f, body.center.z);
                float turn = Time.time * 300f;
                for (int s = 0; s < swirls.Count; s++)
                {
                    float start = turn + s * 360f / swirls.Count;
                    for (int p = 0; p < SwirlPoints; p++)
                    {
                        float angle = (start + p * 12f) * Mathf.Deg2Rad;
                        float bob = Mathf.Sin(angle * 2f) * 0.05f;
                        swirls[s].SetPosition(p, centre + new Vector3(Mathf.Cos(angle) * radius, bob, Mathf.Sin(angle) * radius));
                    }
                }
            }
        }
    }
}
