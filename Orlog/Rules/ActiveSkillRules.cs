using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orlog.Rules
{
    /// <summary>
    ///     Per-level values of a class's active skills (<c>Orlog/Data/&lt;class&gt;_skill_values.csv</c>, Diablo 2's numbers
    ///     from <c>data/d2-reference/amazon_levels.csv</c>). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class SkillValueTable
    {
        public const int Levels = 20;

        private readonly Dictionary<string, float[]> _values = new Dictionary<string, float[]>(StringComparer.OrdinalIgnoreCase);

        private static string Key(string id, string value) => id + "|" + value;

        public bool Has(string id, string value) => _values.ContainsKey(Key(id, value));

        /// <summary>The value at a skill level (0 below level 1 or when missing; past the table, its last value).</summary>
        public float Get(string id, string value, int level)
        {
            if (level < 1 || !_values.TryGetValue(Key(id, value), out var row))
            {
                return 0f;
            }
            return row[Math.Min(level, Levels) - 1];
        }

        public IEnumerable<string> SkillIds => _values.Keys.Select(k => k.Substring(0, k.IndexOf('|'))).Distinct(StringComparer.OrdinalIgnoreCase);

        public static SkillValueTable Parse(string csv, string file, List<string> errors)
        {
            var columns = new List<string> { "id", "value" };
            for (int i = 1; i <= Levels; i++)
            {
                columns.Add("l" + i.ToString(CultureInfo.InvariantCulture));
            }
            var table = new SkillValueTable();
            foreach (var row in Table.Read(csv, file, columns.ToArray(), errors))
            {
                string id = row["id"].Trim(), value = row["value"].Trim();
                if (id.Length == 0 || value.Length == 0 || table._values.ContainsKey(Key(id, value)))
                {
                    errors.Add($"{file}: '{id}' '{value}' is empty or listed twice.");
                    continue;
                }
                var levels = new float[Levels];
                for (int i = 0; i < Levels; i++)
                {
                    levels[i] = Table.Float(row, "l" + (i + 1).ToString(CultureInfo.InvariantCulture), errors);
                }
                table._values[Key(id, value)] = levels;
            }
            return table;
        }
    }

    /// <summary>Why a skill slot can't be used right now (<c>classes-ui-brief.md</c> §3, <c>GetSlotStateJson</c>).</summary>
    internal struct SlotState
    {
        public bool Usable;
        /// <summary><c>""</c>, <c>weapon</c>, <c>eitr</c> or <c>cooldown</c>.</summary>
        public string Blocked;
        public string Needs;
        public float EitrCost;
    }

    /// <summary>The four skill slots, the skill button and the active skills' numbers (roadmap C5). Unit-tested.</summary>
    internal static class ActiveSkillRules
    {
        public const int Slots = 4;

        /// <summary>One D2 yard in metres.</summary>
        public const float MetresPerYard = 0.9144f;

        /// <summary>Multishot's fan: 5° between arrows, at most 60° in all (agreed by Krogon, 2026-11-15).</summary>
        public const float FanSpacing = 5f;
        public const float FanMax = 60f;

        // ------------------------------------------------------------ slots

        /// <summary>The saved slots, <c>id0;id1;id2;id3</c>; always 4 entries ("" = empty).</summary>
        public static string[] ParseSlots(string saved)
        {
            var slots = new string[Slots];
            var parts = (saved ?? "").Split(';');
            for (int i = 0; i < Slots; i++)
            {
                slots[i] = i < parts.Length ? parts[i].Trim() : "";
            }
            return slots;
        }

        /// <summary>
        ///     The slots with every skill the character has no points in emptied (after a respec the cross kept the old
        ///     skills: Krogon, 2026-11-15).
        /// </summary>
        public static string[] OnlyLearned(string[] slots, Func<string, int> levelOf)
        {
            var result = ParseSlots(FormatSlots(slots));
            for (int i = 0; i < Slots; i++)
            {
                if (result[i].Length > 0 && levelOf(result[i]) < 1)
                {
                    result[i] = "";
                }
            }
            return result;
        }

        public static string FormatSlots(string[] slots)
        {
            return string.Join(";", Enumerable.Range(0, Slots).Select(i => slots != null && i < slots.Length ? slots[i] ?? "" : ""));
        }

        /// <summary>Why <paramref name="skill" /> can't go in a slot, "" if it can.</summary>
        public static string SlotRefusal(SkillTable.Skill skill, int points)
        {
            if (skill == null)
            {
                return "Unknown skill";
            }
            if (skill.Kind == "passive")
            {
                return "Passive skills don't take a slot";
            }
            if (!skill.Works)
            {
                return "Coming later";
            }
            return points > 0 || skill.IsBasic ? "" : "Not learned yet";
        }

        /// <summary>The slots with <paramref name="id" /> in <paramref name="slot" /> ("" clears it); a skill in another slot moves.</summary>
        public static string[] Place(string[] slots, int slot, string id)
        {
            var result = ParseSlots(FormatSlots(slots));
            if (slot < 0 || slot >= Slots)
            {
                return result;
            }
            id = id ?? "";
            for (int i = 0; i < Slots; i++)
            {
                if (id.Length > 0 && string.Equals(result[i], id, StringComparison.OrdinalIgnoreCase))
                {
                    result[i] = "";
                }
            }
            result[slot] = id;
            return result;
        }

        /// <summary>
        ///     Whether a slot's skill can be used now. In order: the weapon it needs isn't in hand (attack skills only;
        ///     <paramref name="weaponKind" /> is <c>bow</c>, <c>spear</c> or ""), then not enough eitr.
        /// </summary>
        public static SlotState State(SkillTable.Skill skill, string weaponKind, float eitr, float cost)
        {
            var state = new SlotState { Blocked = "", Needs = skill?.Needs ?? "", EitrCost = cost };
            if (skill == null)
            {
                state.Blocked = "weapon";
                return state;
            }
            if (skill.Needs.Length > 0 && !string.Equals(skill.Needs, weaponKind, StringComparison.OrdinalIgnoreCase))
            {
                state.Blocked = "weapon";
            }
            else if (cost > 0f && eitr < cost)
            {
                state.Blocked = "eitr";
            }
            state.Usable = state.Blocked.Length == 0;
            return state;
        }

        /// <summary>The message for a blocked skill button press.</summary>
        public static string BlockedMessage(SlotState state)
        {
            switch (state.Blocked)
            {
                case "weapon":
                    return state.Needs.Length > 0 ? "Need " + state.Needs : "Wrong weapon";
                case "eitr":
                    return "Not enough eitr";
                case "cooldown":
                    return "Not ready yet";
                default:
                    return "";
            }
        }

        /// <summary>The slot each weapon kind last had on the skill button, saved as <c>bow:0;spear:2</c> (user, 2026-11-11).</summary>
        public static Dictionary<string, int> ParseSelected(string saved)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in (saved ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = part.IndexOf(':');
                if (colon > 0 && int.TryParse(part.Substring(colon + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int slot)
                    && slot >= -1 && slot < Slots)
                {
                    result[part.Substring(0, colon).Trim()] = slot;
                }
            }
            return result;
        }

        public static string FormatSelected(IDictionary<string, int> selected)
        {
            return string.Join(";", selected.OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => p.Key + ":" + p.Value.ToString(CultureInfo.InvariantCulture)));
        }

        // ------------------------------------------------------------ quick cast (user, 2026-11-13)

        /// <summary>Only a skill that needs no weapon can quick cast (cast from its slot key); one that needs a weapon goes on right click.</summary>
        public static bool CanQuickCast(SkillTable.Skill skill)
        {
            return skill != null && skill.Kind == "cast" && skill.Needs.Length == 0;
        }

        /// <summary>Quick cast is on unless the player turned it off for that skill (saved as the skills with it off).</summary>
        public static bool IsQuickCast(SkillTable.Skill skill, ICollection<string> quickCastOff)
        {
            return CanQuickCast(skill) && (quickCastOff == null || !quickCastOff.Contains(skill.Id));
        }

        /// <summary>Whether a slot's skill can be on the skill button: an attack skill, or a cast skill with quick cast off.</summary>
        public static bool OnSkillButton(SkillTable.Skill skill, ICollection<string> quickCastOff)
        {
            return skill != null && (skill.Kind == "attack" || (skill.Kind == "cast" && !IsQuickCast(skill, quickCastOff)));
        }

        public static HashSet<string> ParseIds(string saved)
        {
            return new HashSet<string>((saved ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        public static string FormatIds(IEnumerable<string> ids)
        {
            return string.Join(";", ids.OrderBy(i => i, StringComparer.Ordinal));
        }

        // ------------------------------------------------------------ numbers

        /// <summary>The eitr cost: D2's mana per level from the values table (D2R where it differs), else the tree's formula.</summary>
        public static float EitrCost(SkillTable.Skill skill, SkillValueTable values, int level)
        {
            if (skill == null || skill.Kind == "passive")
            {
                return 0f;
            }
            if (values != null && values.Has(skill.Id, "mana_cost"))
            {
                return Math.Max(0f, values.Get(skill.Id, "mana_cost", Math.Max(1, level)));
            }
            return SkillRules.EitrCost(skill, level);
        }

        /// <summary>
        ///     The damage a skill adds, as a share of the shot's own damage (bow + arrow, as the game worked it out for
        ///     this shot): <paramref name="percent" /> from the values table × the admin's multiplier. Krogon, 2026-11-16
        ///     (#72): "All scale off the weapon"; Diablo 2's fixed points and the table that converted them are gone.
        /// </summary>
        public static float ShareOfShot(float shotDamage, float percent, float multiplier = 1f)
        {
            return Math.Max(0f, shotDamage) * Math.Max(0f, percent) / 100f * Math.Max(0f, multiplier);
        }

        /// <summary>Multishot's angle for arrow <paramref name="index" /> of <paramref name="count" />, centred on the aim.</summary>
        public static float FanAngle(int index, int count, float spacing = FanSpacing, float max = FanMax)
        {
            if (count <= 1)
            {
                return 0f;
            }
            float step = Math.Min(spacing, max / (count - 1));
            return (index - (count - 1) / 2f) * step;
        }

        // ------------------------------------------------------------ the ghost javelin (Krogon, 2026-11-28, C6)

        public const string GungnirsThrowId = "GungnirsThrow";
        public const string LightningBoltId = "LightningBolt";
        public const string PlagueJavelinId = "PlagueJavelin";

        /// <summary>How long a ghost spear stays where it stuck, fading out (decision 8: "Sticks, then fades", about a second).</summary>
        public const float GhostFadeSeconds = 1f;

        /// <summary>
        ///     A skill thrown as a ghost copy of the spear from the spear's own throw (the game's secondary attack): Gungnir's
        ///     Throw and the javelin skills built so far (Lightning Bolt, Plague Javelin). The spear stays in hand.
        /// </summary>
        public static bool Throws(string skillId)
        {
            return string.Equals(skillId, GungnirsThrowId, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(skillId, LightningBoltId, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(skillId, PlagueJavelinId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Plague Javelin's cloud: its radius <paramref name="elapsed" /> seconds after the javelin landed, spreading
        ///     evenly from nothing to <paramref name="maxRadius" /> over the cloud's <paramref name="duration" /> (decision 14:
        ///     "Grows to 4 m", over D2's poison time). The full radius with no duration.
        /// </summary>
        public static float SpreadRadius(float maxRadius, float elapsed, float duration)
        {
            if (maxRadius <= 0f)
            {
                return 0f;
            }
            if (duration <= 0f)
            {
                return maxRadius;
            }
            return maxRadius * Math.Max(0f, Math.Min(1f, elapsed / duration));
        }

        /// <summary>How visible a stuck ghost spear is, <paramref name="sinceStuck" /> seconds after it stuck: 1 → 0 over <paramref name="fade" />.</summary>
        public static float GhostAlpha(float sinceStuck, float fade = GhostFadeSeconds)
        {
            if (sinceStuck <= 0f)
            {
                return 1f;
            }
            return fade <= 0f ? 0f : Math.Max(0f, 1f - sinceStuck / fade);
        }

        // ------------------------------------------------------------ Wind Lance and Forked Lightning (user, 2026-11-16)

        /// <summary>
        ///     A skill whose shot is charged: held on the skill button past full draw (Wind Lance; Snipe since
        ///     2026-11-18).
        /// </summary>
        public static bool Charges(string skillId)
        {
            return string.Equals(skillId, "WindLance", StringComparison.OrdinalIgnoreCase) || IsSnipe(skillId);
        }

        public static bool IsSnipe(string skillId)
        {
            return string.Equals(skillId, "Snipe", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     The values-table row with a charged skill's charge time: Wind Lance must be held <c>hold_s</c>; Snipe's
        ///     charge fills over <c>charge_s</c>.
        /// </summary>
        public static string ChargeTimeValue(string skillId)
        {
            return IsSnipe(skillId) ? "charge_s" : "hold_s";
        }

        /// <summary>
        ///     Whether a charged shot released now is the skill's shot (and pays its eitr), or a normal one for free.
        ///     Wind Lance: only once its hold is complete (user, 2026-11-16). Snipe: any charge at all (Krogon,
        ///     2026-11-18: "pay on any charge"); let go at full draw with no charge, it is a normal arrow.
        /// </summary>
        public static bool ChargeFires(string skillId, float heldPastFullDraw, float holdSeconds)
        {
            return IsSnipe(skillId) ? heldPastFullDraw > 0f : ChargeReady(heldPastFullDraw, holdSeconds);
        }

        // ------------------------------------------------------------ Snipe (Krogon, 2026-11-18)

        /// <summary>
        ///     Snipe's damage, in % of the shot's own, at a charge of <paramref name="charge" /> (0 to 1): a square curve
        ///     from a normal arrow (100%) to the full share (decisions 5-6: "Slow start, big finish", "Square curve"),
        ///     so half a charge gives a quarter of the extra.
        /// </summary>
        public static float SnipeDamagePercent(float charge, float fullPercent)
        {
            float c = Math.Max(0f, Math.Min(1f, charge));
            return 100f + (fullPercent - 100f) * c * c;
        }

        /// <summary>
        ///     Snipe's arrow speed, in % of a normal arrow's, at a charge (0 to 1): rising evenly to the full charge's
        ///     (decision 10: "Faster with the charge"; evenly, not on the damage's curve, is Claude's choice).
        /// </summary>
        public static float SnipeSpeedPercent(float charge, float fullPercent)
        {
            float c = Math.Max(0f, Math.Min(1f, charge));
            return 100f + (fullPercent - 100f) * c;
        }

        /// <summary>
        ///     The camera's view angle, in degrees, for Snipe's zoom (decision 7: "3× at full charge"): things look
        ///     <c>1 + (zoom − 1) × charge</c> times as close, so the zoom closes in evenly as the charge fills.
        ///     A magnification m narrows a view of angle a to <c>2·atan(tan(a/2) / m)</c> (65° at 3× is about 24°).
        /// </summary>
        public static float ZoomFov(float baseFov, float zoom, float charge)
        {
            float c = Math.Max(0f, Math.Min(1f, charge));
            float magnification = 1f + (Math.Max(1f, zoom) - 1f) * c;
            double half = Math.Tan(baseFov * Math.PI / 360.0) / magnification;
            return (float)(Math.Atan(half) * 360.0 / Math.PI);
        }

        // ------------------------------------------------------------ Shackle Shot (Krogon, 2026-11-18)

        /// <summary>
        ///     Shackle Shot's anchor (decisions 11 and 15, after Dota 2's Shackleshot): of the candidates (enemies and
        ///     trees), given by their offset on the ground from the struck creature, the nearest one behind it, at most
        ///     <paramref name="reach" /> away and within <paramref name="coneDegrees" /> of the line from the shooter
        ///     through it (<paramref name="dirX" />, <paramref name="dirZ" />, on the ground). -1 when none is.
        ///     On a tie, the earlier candidate wins (the caller lists enemies before trees).
        /// </summary>
        public static int ShackleAnchor(IList<float> offsetX, IList<float> offsetZ, float dirX, float dirZ, float reach, float coneDegrees)
        {
            float length = (float)Math.Sqrt(dirX * dirX + dirZ * dirZ);
            if (offsetX == null || offsetZ == null || length < 1e-4f || reach <= 0f)
            {
                return -1;
            }
            dirX /= length;
            dirZ /= length;
            double cosCone = Math.Cos(Math.Max(0f, Math.Min(180f, coneDegrees)) * Math.PI / 180.0);
            int best = -1;
            float bestDistance = float.MaxValue;
            int count = Math.Min(offsetX.Count, offsetZ.Count);
            for (int i = 0; i < count; i++)
            {
                float x = offsetX[i], z = offsetZ[i];
                float distance = (float)Math.Sqrt(x * x + z * z);
                if (distance < 1e-3f || distance > reach)
                {
                    continue;
                }
                double cos = (x * dirX + z * dirZ) / distance;
                if (cos < cosCone - 1e-6 || distance >= bestDistance)
                {
                    continue;
                }
                best = i;
                bestDistance = distance;
            }
            return best;
        }

        /// <summary>
        ///     Whether a charged shot is ready: held for <paramref name="holdSeconds" /> after the bow reached full draw
        ///     (a crossbow has no draw, so from the press). A skill with no hold time is ready at once.
        /// </summary>
        public static bool ChargeReady(float heldPastFullDraw, float holdSeconds)
        {
            return holdSeconds <= 0f || heldPastFullDraw >= holdSeconds;
        }

        /// <summary>How far a charged shot's hold has come, 0 to 1 (1 with no hold time).</summary>
        public static float ChargeProgress(float heldPastFullDraw, float holdSeconds)
        {
            return holdSeconds <= 0f ? 1f : Math.Max(0f, Math.Min(1f, heldPastFullDraw / holdSeconds));
        }

        /// <summary>
        ///     The bow's draw circle while a charged shot is held (Krogon, 2026-11-16: "Just like the yellow circle
        ///     narrows in normally when you do a charge on the bow, it should continue to narrow in and maybe turn a
        ///     different colour"). The game shrinks the circle from 1 to <see cref="DrawCircleEnd" /> over the draw
        ///     (<c>Hud.UpdateCrosshair</c>); the charge shrinks it on to <see cref="ChargeCircleEnd" /> (our choice). A
        ///     crossbow has no draw, so its circle runs the whole way from 1 during the hold.
        /// </summary>
        public const float DrawCircleEnd = 0.15f;
        public const float ChargeCircleEnd = 0.06f;

        public static float ChargeCircleScale(float progress, bool bowDraws)
        {
            float from = bowDraws ? DrawCircleEnd : 1f;
            float t = Math.Max(0f, Math.Min(1f, progress));
            return from + (ChargeCircleEnd - from) * t;
        }

        /// <summary>What the ground does to a Wind Lance on its next step.</summary>
        public enum LanceGround
        {
            /// <summary>It flies on straight, clear of the ground.</summary>
            Free,
            /// <summary>It runs along the ground (or water) at its floor height.</summary>
            Skim,
            /// <summary>The ground is in its way (a dive or a rise steeper than it can follow): it flies on straight and strikes it.</summary>
            Blocked
        }

        /// <summary>
        ///     The steepest ground a Wind Lance follows, in degrees: the slope a player can stand on before sliding
        ///     (<c>Character.GetSlideAngle</c>: 38 for players, game code).
        /// </summary>
        public const float LanceMaxSlopeDegrees = 38f;

        /// <summary>
        ///     Where the ground lets a Wind Lance go on its next step (Krogon, 2026-11-16: "Run along the ground": where
        ///     it would dig in, it levels out and rushes on just above the ground, following gentle slopes; steep
        ///     hillsides still stop it; shot at something in the air it flies straight).
        ///     <paramref name="y" /> is its height now, <paramref name="straightNextY" /> its height after a straight
        ///     step, <paramref name="floorY" /> the lowest it may fly at the next spot (ground or water plus the gust's
        ///     half width), <paramref name="flatStep" /> the step's horizontal length. A lance in free flight that would
        ///     dip under the floor starts skimming, unless it dives, or the ground rises, more steeply than
        ///     <paramref name="maxSlopeDegrees" />. A skimming lance follows the floor up and down, at most that steeply.
        /// </summary>
        public static LanceGround LanceGroundStep(bool skimming, float y, float straightNextY, float floorY, float flatStep, float maxSlopeDegrees, out float nextY)
        {
            float reach = Math.Max(0f, (float)Math.Tan(maxSlopeDegrees * Math.PI / 180.0) * flatStep);
            if (!skimming)
            {
                nextY = straightNextY;
                if (straightNextY >= floorY)
                {
                    return LanceGround.Free;
                }
                if (y - straightNextY > reach || floorY - y > reach)
                {
                    return LanceGround.Blocked;
                }
                nextY = floorY;
                return LanceGround.Skim;
            }
            if (floorY - y > reach)
            {
                nextY = y;
                return LanceGround.Blocked;
            }
            nextY = Math.Max(floorY, y - reach);
            return LanceGround.Skim;
        }

        /// <summary>
        ///     Whether a breakable thing growing in the world is a small plant a Wind Lance passes through (Krogon,
        ///     2026-11-16: "Pass through bushes"; trees, logs, rocks and buildings still stop it). Told from the game's own
        ///     data: the game doesn't class it as a tree (its destructible type), and everything it drops is something
        ///     trees drop (wood and the like), so not a rock. Something that drops nothing isn't one (the game's bare
        ///     rocks and cliffs drop nothing too).
        /// </summary>
        public static bool IsSmallPlant(bool gameCallsItATree, IEnumerable<string> drops, ICollection<string> treeDrops)
        {
            if (gameCallsItATree || drops == null || treeDrops == null)
            {
                return false;
            }
            bool any = false;
            foreach (string drop in drops)
            {
                if (string.IsNullOrEmpty(drop) || !treeDrops.Contains(drop))
                {
                    return false;
                }
                any = true;
            }
            return any;
        }

        /// <summary>
        ///     Forked Lightning's targets: of the enemies at these distances from the struck enemy's body, the nearest
        ///     <paramref name="forks" /> within <paramref name="reach" />, nearest first (their indexes). One fork each.
        /// </summary>
        public static List<int> ForkTargets(IList<float> distances, float reach, int forks)
        {
            var result = new List<int>();
            if (distances == null || forks < 1)
            {
                return result;
            }
            for (int i = 0; i < distances.Count; i++)
            {
                if (distances[i] <= reach)
                {
                    result.Add(i);
                }
            }
            // Nearest first; equal distances keep their order.
            var ordered = result.OrderBy(i => distances[i]).ThenBy(i => i).Take(forks).ToList();
            return ordered;
        }

        // ------------------------------------------------------------ Strafe, Immolation Arrow and Venom Strike (user, 2026-11-16)

        /// <summary>
        ///     Seconds between the hits of an area on the ground (Immolation Arrow's burning ground, Venom Strike's cloud):
        ///     Krogon, 2026-11-16, "every second".
        /// </summary>
        public const float AreaInterval = 1f;

        /// <summary>
        ///     How many times an area on the ground hits what stands in it: one hit every <paramref name="interval" /> seconds
        ///     for <paramref name="duration" /> seconds (4 s at 1 s = 4). 0 when either is not positive.
        /// </summary>
        public static int AreaTicks(float duration, float interval = AreaInterval)
        {
            if (duration <= 0f || interval <= 0f)
            {
                return 0;
            }
            return (int)Math.Floor(duration / interval + 1e-4f);
        }

        /// <summary>
        ///     Strafe, remade after Diablo 3's Rapid Fire (Krogon, 2026-11-17): once the bow is fully drawn and the skill
        ///     button is still held, arrows stream out at <paramref name="perSecond" /> a second. How many arrows of the
        ///     stream are due after <paramref name="firingSeconds" /> seconds of firing: the first at once, then one each
        ///     1 / <paramref name="perSecond" /> seconds (6 a second: 1 at 0 s, 6 by 0.84 s, 7 at 1 s). 0 before the stream
        ///     starts (negative time); with no rate, only the first.
        /// </summary>
        public static int StreamDue(float firingSeconds, float perSecond)
        {
            if (firingSeconds < 0f)
            {
                return 0;
            }
            if (perSecond <= 0f)
            {
                return 1;
            }
            return (int)Math.Floor(firingSeconds * perSecond + 1e-4f) + 1;
        }

        /// <summary>
        ///     Strafe: whether arrow <paramref name="index" /> of the stream (0 = the first) takes an arrow from the quiver,
        ///     at <paramref name="ammoPerSecond" /> from the quiver a second while <paramref name="arrowsPerSecond" /> fly
        ///     (Krogon, 2026-11-17: "One per second"). The first always does; after it, the one that starts each new
        ///     second's share (6 a second, 1 from the quiver: arrows 0, 6, 12 …). With no stream rate every arrow does.
        /// </summary>
        public static bool StreamTakesAmmo(int index, float arrowsPerSecond, float ammoPerSecond)
        {
            if (index <= 0 || arrowsPerSecond <= 0f)
            {
                return index >= 0;
            }
            if (ammoPerSecond <= 0f)
            {
                return false;
            }
            float at = index / arrowsPerSecond, before = (index - 1) / arrowsPerSecond;
            return StreamDue(at, ammoPerSecond) > StreamDue(before, ammoPerSecond);
        }

        private static string N(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        /// <summary>A skill's share of the shot's damage as the tooltip shows it: the table's % × the admin's multiplier.</summary>
        private static string Share(SkillValueTable v, string id, string value, int level, float multiplier)
        {
            return N(ShareOfShot(100f, v.Get(id, value, level), multiplier)) + "%";
        }

        /// <summary>
        ///     Inner Sight's extra damage taken, in %: <paramref name="basePercent" /> at level 1 plus
        ///     <paramref name="perLevel" /> for each level above it, with no cap, so +skills can raise it past 20's
        ///     (Krogon, 2026-11-15: "start at 5% and add 1% per level"; 5% → 24% at 20).
        /// </summary>
        public static float InnerSightPercent(int level, float basePercent, float perLevel)
        {
            return level < 1 ? 0f : Math.Max(0f, basePercent + perLevel * (level - 1));
        }

        /// <summary>
        ///     The tooltip line of an active skill at a level (<paramref name="damageMultiplier" /> = the admin's
        ///     <c>SkillDamageMultiplier</c>, applied to the shares of the shot's damage), or null if the skill has no text here.
        /// </summary>
        public static string Describe(string id, SkillValueTable v, int level, float damageMultiplier, float innerSightPercent)
        {
            if (v == null || level < 1)
            {
                return null;
            }
            switch (id)
            {
                case "MultipleShot":
                    return $"Fires {N(v.Get(id, "arrows", level))} arrows in a fan for one arrow, each at {N(v.Get(id, "weapon_damage_pct", level))}% damage";
                case "FreezingArrow":
                    return $"Explodes on impact: frost damage equal to {Share(v, id, "blast_damage_pct", level, damageMultiplier)} of the shot's damage within {N(v.Get(id, "radius_yards", level) * MetresPerYard)} m, " +
                           $"freezing for {N(v.Get(id, "freeze_s", level))} s (bosses are only slowed)";
                case "ForkedLightning":
                    return $"Lightning strikes the enemy the arrow hits and forks to up to {N(v.Get(id, "forks", level))} other enemies within {N(v.Get(id, "reach_m", level))} m: " +
                           $"each strike deals {Share(v, id, "fork_damage_pct", level, damageMultiplier)} of the shot's damage as lightning";
                case "WindLance":
                    return $"Hold {N(v.Get(id, "hold_s", level))} s past full draw, then release: a gust of wind {N(v.Get(id, "width_m", level) * 2f)} m wide at {N(v.Get(id, "weapon_damage_pct", level))}% damage " +
                           $"rushes {N(v.Get(id, "range_m", level))} m at {N(v.Get(id, "speed_pct", level) / 100f)}× an arrow's speed, along the ground and through every enemy, " +
                           $"staggering them and knocking them back (force {N(v.Get(id, "knockback", level))}). Released early: a normal shot, no eitr";
                case "Strafe":
                    return $"Hold to rapid-fire: after one full draw, fires {N(v.Get(id, "arrows_per_s", level))} arrows a second where you aim for as long as you hold " +
                           $"(a loaded crossbow: {N(v.Get(id, "crossbow_per_s", level))} bolts a second), each at {N(v.Get(id, "weapon_damage_pct", level))}% damage. " +
                           $"Costs {N(v.Get(id, "mana_cost", level))} eitr to start and {N(v.Get(id, "eitr_per_s", level))} eitr a second while firing; " +
                           $"uses {N(v.Get(id, "ammo_per_s", level))} arrow or bolt a second; you walk and lose stamina as with a drawn bow";
                case "Snipe":
                    return $"Hold past full draw to charge for up to {N(v.Get(id, "charge_s", level))} s, then release: the longer the charge, the harder and faster the arrow, " +
                           $"up to {N(v.Get(id, "weapon_damage_pct", level))}% damage at {N(v.Get(id, "speed_pct", level) / 100f)}× an arrow's speed. " +
                           $"The view zooms in up to {N(v.Get(id, "zoom_x", level))}× while charging, and stamina drains as while drawing. Released at full draw with no charge: a normal shot, no eitr";
                case "ShackleShot":
                    return $"The arrow deals {N(v.Get(id, "weapon_damage_pct", level))}% damage and shackles the enemy it hits to another enemy or a tree up to " +
                           $"{N(v.Get(id, "reach_m", level))} m behind it: both are stunned for {N(v.Get(id, "shackle_s", level))} s (with nothing behind it, {N(v.Get(id, "alone_s", level))} s). " +
                           "Bosses are only slowed";
                case "ImmolationArrow":
                    return $"Explodes on impact: fire damage equal to {Share(v, id, "blast_damage_pct", level, damageMultiplier)} of the shot's damage within {N(v.Get(id, "radius_m", level))} m; " +
                           $"the ground there burns for {N(v.Get(id, "burn_duration_s", level))} s, dealing {Share(v, id, "burn_damage_pct", level, damageMultiplier)} of the shot's damage each second to enemies in it";
                case "VenomStrike":
                    return $"Bursts into a poison cloud reaching {N(v.Get(id, "radius_m", level))} m around where it lands for {N(v.Get(id, "cloud_duration_s", level))} s: " +
                           $"everything in it is poisoned for {Share(v, id, "cloud_damage_pct", level, damageMultiplier)} of the shot's damage, again each second it stays";
                case GungnirsThrowId:
                    return "Throws a gold ghost copy of your spear where you aim, for the damage and stamina of the spear's own throw; the spear stays in your hand. No eitr, no skill points";
                case LightningBoltId:
                    return $"Throws a ghost spear wrapped in lightning at {N(v.Get(id, "speed_pct", level) / 100f)}× a thrown spear's speed: {N(v.Get(id, "weapon_damage_pct", level))}% of the throw's damage, " +
                           "all of it lightning, to the first enemy it hits. Costs the throw's stamina too; the spear stays in your hand";
                case PlagueJavelinId:
                    return $"Throws a ghost spear for {N(v.Get(id, "weapon_damage_pct", level))}% of the throw's damage; where it lands a poison cloud spreads out to {N(v.Get(id, "radius_m", level))} m " +
                           $"over {N(v.Get(id, "poison_duration_s", level))} s, poisoning everything in it for {Share(v, id, "cloud_damage_pct", level, damageMultiplier)} of the throw's damage each second. " +
                           "Costs the throw's stamina too; the spear stays in your hand";
                case "InnerSight":
                    return $"Enemies within {N(v.Get(id, "radius_yards", level) * MetresPerYard)} m glow and take {N(innerSightPercent)}% more damage for {N(v.Get(id, "duration_s", level))} s";
                default:
                    return null;
            }
        }
    }
}
