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

    /// <summary>
    ///     How many Valheim damage points one Diablo 2 damage point of a skill is worth, per character level
    ///     (<c>Orlog/Data/skill_damage_scale.csv</c>: the best Valheim bow and arrow of that level's biome ÷ the best D2 bow
    ///     by that level, so a skill adds as much compared with the bow's own damage as in D2).
    /// </summary>
    internal sealed class DamageScale
    {
        private readonly SortedDictionary<int, float> _byLevel = new SortedDictionary<int, float>();

        public int Count => _byLevel.Count;

        /// <summary>The scale at a character level (clamped to the table's levels; 1 for an empty table).</summary>
        public float At(int level)
        {
            if (_byLevel.Count == 0)
            {
                return 1f;
            }
            if (_byLevel.TryGetValue(level, out float scale))
            {
                return scale;
            }
            return level < _byLevel.Keys.First() ? _byLevel.Values.First() : _byLevel.Values.Last();
        }

        public static DamageScale Parse(string csv, string file, List<string> errors)
        {
            var table = new DamageScale();
            foreach (var row in Table.Read(csv, file, new[] { "level", "scale" }, errors))
            {
                int level = Table.Int(row, "level", errors);
                float scale = Table.Float(row, "scale", errors);
                if (level < 1 || scale <= 0f || table._byLevel.ContainsKey(level))
                {
                    errors.Add($"{file}: level {level} has scale {scale} or is listed twice.");
                    continue;
                }
                table._byLevel[level] = scale;
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

        /// <summary>
        ///     Exploding Arrow's blast radius in D2 yards. D2's tables give none (its explosion missile has no radius
        ///     parameter); Claude's interim choice (2026-11-11): Freezing Arrow's 3.3 yards.
        /// </summary>
        public const float ExplodingRadiusYards = 3.3f;

        /// <summary>Multiple Shot's fan (Claude's interim choice, 2026-11-11): 5° between arrows, at most 60° in all.</summary>
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
            if (!skill.Built)
            {
                return "Coming later";
            }
            return points > 0 ? "" : "Not learned yet";
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

        /// <summary>A random value between D2's min and max damage (<paramref name="roll" /> 0–1), in Valheim points.</summary>
        public static float Damage(float d2Min, float d2Max, float roll, float scale)
        {
            return (d2Min + (d2Max - d2Min) * Math.Max(0f, Math.Min(1f, roll))) * scale;
        }

        /// <summary>Multiple Shot's angle for arrow <paramref name="index" /> of <paramref name="count" />, centred on the aim.</summary>
        public static float FanAngle(int index, int count, float spacing = FanSpacing, float max = FanMax)
        {
            if (count <= 1)
            {
                return 0f;
            }
            float step = Math.Min(spacing, max / (count - 1));
            return (index - (count - 1) / 2f) * step;
        }

        /// <summary>The share of physical damage moved to another type (<paramref name="percent" />, clamped 0–100).</summary>
        public static float Converted(float physical, float percent)
        {
            return physical * Math.Max(0f, Math.Min(100f, percent)) / 100f;
        }

        private static string N(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string Range(SkillValueTable v, string id, string kind, int level, float scale)
        {
            return N(v.Get(id, kind + "_min", level) * scale) + "–" + N(v.Get(id, kind + "_max", level) * scale);
        }

        /// <summary>
        ///     The tooltip line of an active skill at a level, with Valheim numbers (<paramref name="scale" /> = the damage
        ///     scale at the character's level × the admin's multiplier), or null if the skill has no text here.
        /// </summary>
        public static string Describe(string id, SkillValueTable v, int level, float scale, float innerSightPercent)
        {
            if (v == null || level < 1)
            {
                return null;
            }
            switch (id)
            {
                case "MagicArrow":
                    return $"A magic arrow that needs none from your quiver: +{N(v.Get(id, "damage_flat", level) * scale)} damage, " +
                           $"{N(v.Get(id, "phys_to_magic_pct", level))}% of the physical damage becomes spirit";
                case "FireArrow":
                    return $"Adds {Range(v, id, "fire_damage", level, scale)} fire damage; {N(v.Get(id, "phys_to_elemental_pct", level))}% of the physical damage becomes fire";
                case "ColdArrow":
                    return $"Adds {Range(v, id, "cold_damage", level, scale)} frost damage, which slows; {N(v.Get(id, "phys_to_elemental_pct", level))}% of the physical damage becomes frost";
                case "MultipleShot":
                    return $"Fires {N(v.Get(id, "arrows", level))} arrows in a fan for one arrow, each at {N(v.Get(id, "weapon_damage_pct", level))}% damage";
                case "ExplodingArrow":
                    return $"Explodes on impact: {Range(v, id, "fire_damage", level, scale)} fire damage within {N(ExplodingRadiusYards * MetresPerYard)} m";
                case "IceArrow":
                    return $"Adds {Range(v, id, "cold_damage", level, scale)} frost damage and freezes for {N(v.Get(id, "freeze_s", level))} s (bosses are only slowed)";
                case "FreezingArrow":
                    return $"Explodes on impact: {Range(v, id, "cold_damage", level, scale)} frost damage within {N(v.Get(id, "radius_yards", level) * MetresPerYard)} m, " +
                           $"freezing for {N(v.Get(id, "freeze_s", level))} s (bosses are only slowed)";
                case "InnerSight":
                    return $"Enemies within {N(v.Get(id, "radius_yards", level) * MetresPerYard)} m glow and take {N(innerSightPercent)}% more damage for {N(v.Get(id, "duration_s", level))} s";
                default:
                    return null;
            }
        }
    }
}
