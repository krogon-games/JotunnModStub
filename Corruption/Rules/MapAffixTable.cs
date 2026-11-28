using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Reads and checks the map affix table (<c>Corruption/Data/map_affixes.csv</c>, Krogon's affix list of 2026-11-27,
    ///     which replaced the rips round decisions 7-10, and decision 16). Each affix has a danger line and a <c>section</c>: <c>face</c> (listed under
    ///     "What you'll face") or <c>earn</c> (a reward, listed under "What you'll earn"). <c>approved</c> rows roll on maps;
    ///     <c>later</c> rows are designed but not built and never roll. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class MapAffixTable
    {
        private static readonly string[] RequiredColumns =
            { "id", "status", "name", "danger", "weight", "effect", "min", "max", "group", "section" };

        private static readonly string[] Sections = { "face", "earn" };

        private static readonly string[] Statuses = { "approved", "later" };

        /// <summary>What an affix does to the rip (the <c>effect</c> column). Krogon's list, 2026-11-27.</summary>
        public static class Effects
        {
            public const string Experience = "experience";
            public const string Density = "density";
            public const string Damage = "damage";
            public const string AttackSpeed = "attack_speed";
            public const string DoubleDamage = "double_damage";
            public const string Health = "health";
            public const string MoveSpeed = "move_speed";
            public const string NoStagger = "no_stagger";
            public const string PhysicalTaken = "physical_taken";
            public const string ElementalTaken = "elemental_taken";
            public const string LifeDrain = "life_drain";
            public const string StaminaDrain = "stamina_drain";
            public const string Resistances = "resistances";
            public const string PoisonShare = "poison_share";
            public const string FireShare = "fire_share";
            public const string LightningShare = "lightning_share";
            // The loot affixes (#96): see MapLootRules.
            public const string DropItems = "drop_items";
            public const string DropOrbs = "drop_orbs";
            public const string DropShields = "drop_shields";
            public const string DropArmour = "drop_armour";
            public const string DropMelee = "drop_melee";
            public const string DropRanged = "drop_ranged";

            public static readonly string[] All =
            {
                Experience, Density, Damage, AttackSpeed, DoubleDamage, Health, MoveSpeed, NoStagger, PhysicalTaken,
                ElementalTaken, LifeDrain, StaminaDrain, Resistances, PoisonShare, FireShare, LightningShare, DropItems, DropOrbs,
                DropShields, DropArmour, DropMelee, DropRanged
            };
        }

        public sealed class Affix
        {
            public string Id;
            public string Status;
            public string Name;
            /// <summary>What it does to the rip, for the tooltip. <c>{value}</c> stands for the map's rolled number.</summary>
            public string Danger;
            /// <summary>Roll weight among the affixes an orb can add.</summary>
            public double Weight;
            /// <summary>One of <see cref="Effects.All" />; blank only on a <c>later</c> row.</summary>
            public string Effect = "";
            /// <summary>Lowest rolled value (percent, or points per second for the drains); 0 for a value-less effect.</summary>
            public double Min;
            /// <summary>Highest rolled value; equal to <see cref="Min" /> for a fixed affix.</summary>
            public double Max;
            /// <summary>Affixes with the same non-empty group never share a map (they would make it unwinnable).</summary>
            public string Group = "";
            /// <summary><c>face</c> (the default) or <c>earn</c>: where the affix's line is listed (decision 16, "Move to earn").</summary>
            public string Section = "face";

            /// <summary>A reward: listed under "What you'll earn" instead of "What you'll face".</summary>
            public bool Earns => Section == "earn";

            public bool Approved => Status == "approved";

            /// <summary>Whether each map rolls its own number (Max above Min).</summary>
            public bool Ranged => Max > Min;

            /// <summary>Keeps a stored value inside [Min, Max].</summary>
            public double Clamp(double value)
            {
                // NaN or infinity (a tampered item) reads as Min (multiplayer review, 2026-11-27).
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    return Min;
                }
                return value < Min ? Min : value > Max ? Max : value;
            }

            /// <summary>A whole number from Min to Max, both included (fixed affixes give Min).</summary>
            public double Roll(System.Func<double> random)
            {
                return Ranged ? AffixRoller.RollValue((int)System.Math.Ceiling(Min), (int)System.Math.Floor(Max), random) : Min;
            }
        }

        public readonly List<Affix> Affixes = new List<Affix>();

        /// <summary>
        ///     Affix ids an admin has switched off (Krogon, 2026-11-27: "make the option to disable some affixes from being able
        ///     to roll - until we catch up and create the mechanics"). They never roll; maps that already carry one keep it.
        /// </summary>
        public readonly HashSet<string> Disabled = new HashSet<string>();

        /// <summary>The affixes orbs can roll: approved rows that aren't switched off.</summary>
        public IEnumerable<Affix> Rollable => Affixes.Where(a => a.Approved && !Disabled.Contains(a.Id));

        /// <summary>
        ///     Replaces <see cref="Disabled" /> from a comma- or space-separated list of ids (case and spaces ignored).
        ///     Returns the ids that aren't in the table, so the caller can warn about typos.
        /// </summary>
        public List<string> SetDisabled(string list)
        {
            Disabled.Clear();
            var unknown = new List<string>();
            foreach (var raw in (list ?? "").Split(new[] { ',', ';', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string id = raw.Trim().ToLowerInvariant();
                if (id.Length == 0)
                {
                    continue;
                }
                if (Find(id) == null)
                {
                    unknown.Add(id);
                }
                else
                {
                    Disabled.Add(id);
                }
            }
            return unknown;
        }

        /// <summary>
        ///     Whether the affix may join a map that holds <paramref name="onMap" /> (ids): not already on it, and no affix of
        ///     its non-empty group on it (Krogon, 2026-11-27: no unwinnable maps).
        /// </summary>
        public bool FitsWith(Affix affix, IEnumerable<string> onMap)
        {
            foreach (string id in onMap)
            {
                if (id == affix.Id)
                {
                    return false;
                }
                var held = Find(id);
                if (held != null && affix.Group.Length > 0 && held.Group == affix.Group)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>The affix with this id (any status, so a <c>later</c> row is found too), or null. Use <see cref="Affix.Approved" /> to tell them apart.</summary>
        public Affix Find(string id)
        {
            return id == null ? null : Affixes.FirstOrDefault(a => a.Id == id);
        }

        public static MapAffixTable Parse(string csv, List<string> errors)
        {
            var table = new MapAffixTable();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("map_affixes.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"map_affixes.csv: missing column '{column}'.");
            }
            if (missing.Count > 0)
            {
                return table;
            }

            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i];
                if (cells.All(c => c.Trim().Length == 0))
                {
                    continue;
                }
                string Cell(string column)
                {
                    int index = header.IndexOf(column);
                    return index >= 0 && index < cells.Count ? cells[index].Trim() : "";
                }

                var a = new Affix
                {
                    Id = Cell("id"),
                    Status = Cell("status").ToLowerInvariant(),
                    Name = Cell("name"),
                    Danger = Cell("danger")
                };
                string where = $"map_affixes.csv line {i + 1} ({(a.Id.Length > 0 ? a.Id : "no id")})";
                if (a.Id.Length == 0 || a.Id.Any(c => !(c >= 'a' && c <= 'z')))
                {
                    errors.Add($"{where}: id must be lowercase letters only.");
                }
                else if (table.Affixes.Any(x => x.Id == a.Id))
                {
                    errors.Add($"{where}: id is listed twice.");
                }
                if (!Statuses.Contains(a.Status))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)} (approved = rolls on maps, later = designed, never rolls).");
                }
                if (a.Name.Length == 0 || a.Danger.Length == 0)
                {
                    errors.Add($"{where}: name and danger can't be empty.");
                }
                string weight = Cell("weight");
                if (weight.Length > 0)
                {
                    if (!Number(weight, out a.Weight) || a.Weight <= 0)
                    {
                        errors.Add($"{where}: weight must be a number above 0.");
                    }
                }
                else if (a.Status == "approved")
                {
                    errors.Add($"{where}: an approved affix needs a weight above 0.");
                }
                a.Effect = Cell("effect").ToLowerInvariant();
                a.Group = Cell("group");
                string section = Cell("section").ToLowerInvariant();
                if (section.Length > 0)
                {
                    a.Section = section;
                    if (!Sections.Contains(section))
                    {
                        errors.Add($"{where}: section '{Cell("section")}' must be one of {string.Join(", ", Sections)}.");
                    }
                }
                else if (a.Approved)
                {
                    errors.Add($"{where}: an approved affix needs a section (face or earn).");
                }
                if (a.Group.Length > 0 && a.Group.Any(c => !(c >= 'a' && c <= 'z')))
                {
                    errors.Add($"{where}: group must be lowercase letters only (blank = no group).");
                }
                string minText = Cell("min"), maxText = Cell("max");
                bool hasMin = minText.Length > 0, hasMax = maxText.Length > 0;
                bool minOk = !hasMin || Number(minText, out a.Min);
                bool maxOk = !hasMax || Number(maxText, out a.Max);
                if (!minOk || !maxOk)
                {
                    errors.Add($"{where}: min and max must be numbers (or blank).");
                }
                if (a.Effect.Length > 0 && !Effects.All.Contains(a.Effect))
                {
                    errors.Add($"{where}: effect '{a.Effect}' must be one of {string.Join(", ", Effects.All)}.");
                }
                else if (a.Approved)
                {
                    if (a.Effect.Length == 0)
                    {
                        errors.Add($"{where}: an approved affix needs an effect.");
                    }
                    else if (a.Effect == Effects.NoStagger)
                    {
                        if (hasMin || hasMax)
                        {
                            errors.Add($"{where}: no_stagger has no value, so min and max must be blank.");
                        }
                    }
                    else if (!hasMin || !hasMax || !minOk || !maxOk)
                    {
                        errors.Add($"{where}: an approved {a.Effect} affix needs a numeric min and max.");
                    }
                    else if (a.Min <= 0 || a.Max < a.Min)
                    {
                        errors.Add($"{where}: min must be above 0 and at most max.");
                    }
                    else if ((a.Effect == Effects.PhysicalTaken || a.Effect == Effects.ElementalTaken) && a.Max > 100)
                    {
                        errors.Add($"{where}: {a.Effect} is a percentage of damage removed, so max must be at most 100.");
                    }
                }
                else if (hasMin && hasMax && minOk && maxOk && a.Max < a.Min)
                {
                    errors.Add($"{where}: min must be at most max.");
                }
                if (!hasMax && hasMin && minOk)
                {
                    a.Max = a.Min;
                }
                if (!hasMin && hasMax && maxOk)
                {
                    a.Min = a.Max;
                }
                table.Affixes.Add(a);
            }
            return table;
        }

        private static bool Number(string value, out double number)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }
    }
}
