using System;
using System.Collections.Generic;
using System.Globalization;

namespace Orlog.Rules
{
    /// <summary>
    ///     The chance per skill level of a class's passives (<c>Orlog/Data/&lt;class&gt;_passives.csv</c>, made by
    ///     <c>tools/make_orlog_tables.py</c> from Diablo 2's tables). No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class PassiveTable
    {
        public const int Levels = 20;

        private static readonly string[] Effects = { "crit", "dodge", "avoid", "evade", "penetrate" };

        public sealed class Passive
        {
            public string Id;
            /// <summary><c>crit</c>, <c>dodge</c>, <c>avoid</c>, <c>evade</c> or <c>penetrate</c>.</summary>
            public string Effect;
            /// <summary>Tooltip text; <c>{0}</c> is the chance.</summary>
            public string Text;
            /// <summary>Chance in percent at skill levels 1..20 (index 0 = level 1).</summary>
            public float[] Chances = new float[Levels];
        }

        private readonly Dictionary<string, Passive> _byId = new Dictionary<string, Passive>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Passive> _byEffect = new Dictionary<string, Passive>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<Passive> Passives => _byId.Values;

        public Passive Find(string id)
        {
            return id != null && _byId.TryGetValue(id, out var p) ? p : null;
        }

        public Passive ForEffect(string effect)
        {
            return effect != null && _byEffect.TryGetValue(effect, out var p) ? p : null;
        }

        public static PassiveTable Parse(string csv, string file, List<string> errors)
        {
            var columns = new List<string> { "id", "effect", "text" };
            for (int i = 1; i <= Levels; i++)
            {
                columns.Add("l" + i.ToString(CultureInfo.InvariantCulture));
            }
            var table = new PassiveTable();
            foreach (var row in Table.Read(csv, file, columns.ToArray(), errors))
            {
                var p = new Passive
                {
                    Id = row["id"].Trim(),
                    Effect = row["effect"].Trim().ToLowerInvariant(),
                    Text = row["text"].Trim()
                };
                if (p.Id.Length == 0 || table._byId.ContainsKey(p.Id))
                {
                    errors.Add($"{file}: passive id '{p.Id}' is empty or listed twice.");
                    continue;
                }
                if (Array.IndexOf(Effects, p.Effect) < 0)
                {
                    errors.Add($"{file}: {p.Id} has effect '{p.Effect}' ({string.Join(", ", Effects)}).");
                    continue;
                }
                if (table._byEffect.ContainsKey(p.Effect))
                {
                    errors.Add($"{file}: effect '{p.Effect}' is listed twice.");
                    continue;
                }
                if (!p.Text.Contains("{0}"))
                {
                    errors.Add($"{file}: {p.Id}'s text has no {{0}} for the chance.");
                }
                for (int i = 0; i < Levels; i++)
                {
                    float chance = Table.Float(row, "l" + (i + 1).ToString(CultureInfo.InvariantCulture), errors);
                    if (chance < 0f || chance > 100f)
                    {
                        errors.Add($"{file}: {p.Id} level {i + 1} has a chance of {chance}% (0-100).");
                    }
                    else if (i > 0 && chance < p.Chances[i - 1])
                    {
                        errors.Add($"{file}: {p.Id}'s chance drops at level {i + 1}.");
                    }
                    p.Chances[i] = chance;
                }
                table._byId[p.Id] = p;
                table._byEffect[p.Effect] = p;
            }
            return table;
        }
    }

    /// <summary>Which of the Amazon's avoids a hit can meet (Diablo 2's split, user 2026-11-11).</summary>
    internal enum AvoidKind
    {
        None,
        Dodge,
        Avoid,
        Evade
    }

    /// <summary>The rules of the passives (roadmap C3, user decisions 2026-11-11). No game dependencies, so it is unit-tested.</summary>
    internal static class PassiveRules
    {
        /// <summary>A crit doubles the hit's physical damage (blunt, slash, pierce).</summary>
        public const float CritMultiplier = 2f;

        /// <summary>
        ///     Penetrate's marker in the hit's <c>m_eitrAdd</c>, which the game sends with every hit and only reads when it is
        ///     above 0. A marked hit carries <c>Marker - original</c> (always at or below <see cref="Marker" />), so the
        ///     original value comes back when it is read.
        /// </summary>
        public const float Marker = -1000f;

        /// <summary>The chance in percent at <paramref name="level" /> (0 below level 1; levels past the table keep its last value).</summary>
        public static float Chance(PassiveTable.Passive passive, int level)
        {
            if (passive == null || level < 1)
            {
                return 0f;
            }
            return passive.Chances[Math.Min(level, PassiveTable.Levels) - 1];
        }

        /// <summary>The tooltip line, e.g. "25% chance to deal double physical damage".</summary>
        public static string Describe(PassiveTable.Passive passive, int level)
        {
            return string.Format(CultureInfo.InvariantCulture, passive.Text, Chance(passive, level).ToString("0.#", CultureInfo.InvariantCulture));
        }

        /// <summary>True when a roll (0 inclusive to 1 exclusive) lands inside <paramref name="chancePercent" />.</summary>
        public static bool Rolls(float chancePercent, float roll)
        {
            return chancePercent > 0f && roll < chancePercent / 100f;
        }

        /// <summary>
        ///     Which avoid can meet a hit on the player, or <see cref="AvoidKind.None" />. Only a creature's hit (not a
        ///     player's, a fall, a trap, burning or poison ticks) that the game lets a dodge roll escape
        ///     (<paramref name="dodgeable" />), and not an area blast or breath (<paramref name="area" />). While the player
        ///     blocks a hit the block would take, the block or parry handles it. Moving (walking or running, not attacking):
        ///     Evade, for any hit. Standing still or attacking: Avoid for projectiles, Dodge for the rest.
        /// </summary>
        public static AvoidKind Which(bool creatureHit, bool dodgeable, bool area, bool ranged, bool moving, bool blockTakesIt)
        {
            if (!creatureHit || !dodgeable || area || blockTakesIt)
            {
                return AvoidKind.None;
            }
            if (moving)
            {
                return AvoidKind.Evade;
            }
            return ranged ? AvoidKind.Avoid : AvoidKind.Dodge;
        }

        /// <summary>The word floated above the player when a hit is avoided.</summary>
        public static string Word(AvoidKind kind)
        {
            return kind == AvoidKind.None ? "" : kind.ToString();
        }

        /// <summary>The effect name in the passives table for an avoid.</summary>
        public static string Effect(AvoidKind kind)
        {
            return kind == AvoidKind.None ? "" : kind.ToString().ToLowerInvariant();
        }

        /// <summary>
        ///     Breath attacks can't be avoided. The game's breath attacks are the creature attack items with "breath" in the
        ///     prefab name (game dump 1.0.20, <c>item_details.csv</c>: <c>dragon_coldbreath</c>, <c>Fader_Flamebreath</c>,
        ///     <c>BonemawSerpent_breath</c>, <c>fallenvalkyrie_poisonbreath</c> and their aspect copies).
        /// </summary>
        public static bool IsBreath(string attackPrefab)
        {
            return attackPrefab != null && attackPrefab.IndexOf("breath", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Penetrate lifts every resistance (slight ¾, resistant ½, very resistant ¼) to full damage; not immunity, not weakness.</summary>
        public static bool PenetrateLifts(string damageModifier)
        {
            return damageModifier == "SlightlyResistant" || damageModifier == "Resistant" || damageModifier == "VeryResistant";
        }

        /// <summary>The hit's <c>m_eitrAdd</c> with Penetrate's marker (see <see cref="Marker" />).</summary>
        public static float Mark(float eitrAdd)
        {
            return IsMarked(eitrAdd) ? eitrAdd : Marker - Math.Max(0f, eitrAdd);
        }

        public static bool IsMarked(float eitrAdd)
        {
            return eitrAdd <= Marker;
        }

        /// <summary>The original <c>m_eitrAdd</c> of a marked hit (unmarked values come back unchanged).</summary>
        public static float Unmark(float eitrAdd)
        {
            return IsMarked(eitrAdd) ? Marker - eitrAdd : eitrAdd;
        }
    }
}
