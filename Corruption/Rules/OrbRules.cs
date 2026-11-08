using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>One item as the Riftforge sees it: its rarity, affixes, locks and what affixes it can roll.</summary>
    internal sealed class CraftItem
    {
        /// <summary>normal, magic, rare or unique.</summary>
        public string Rarity = "normal";

        /// <summary>The rolled affixes; null for a Normal item.</summary>
        public LootItem Loot;

        /// <summary>Corrupted by a shard: locked for every orb except the Mirror.</summary>
        public bool Corrupted;

        /// <summary>A Mirror of Kalandra's copy: can't be changed or mirrored.</summary>
        public bool Mirrored;

        /// <summary>The item's affix classes (<c>loot_bases.csv</c>); null when it isn't a loot base and so can't roll affixes.</summary>
        public List<string> Classes;

        /// <summary>The item's mechanics (<see cref="LootBases.Mechanics" />): melee, ranged, magic, combo, block.</summary>
        public List<string> Mechanics = new List<string>();

        /// <summary>Affixes on the item; a resonant line counts as 2.</summary>
        public int AffixCount => Loot == null ? 0 : Loot.Lines.Sum(l => l.Ids.Length);
    }

    /// <summary>What an orb did. Nothing changes on the item until the caller writes <see cref="Loot" /> back.</summary>
    internal sealed class OrbResult
    {
        public bool Done;

        /// <summary>Why nothing happened ("" when done).</summary>
        public string Refusal = "";

        /// <summary>The item's rarity afterwards.</summary>
        public string Rarity;

        /// <summary>The item's affixes afterwards (a new object; the input is left as it was).</summary>
        public LootItem Loot;

        /// <summary>Lines that are new or changed (a twin that became resonant, a half left after Annulment, Divine's new values).</summary>
        public List<LootItem.Line> Gained = new List<LootItem.Line>();

        /// <summary>Lines that are gone, as they were before (Annulment).</summary>
        public List<LootItem.Line> Lost = new List<LootItem.Line>();

        /// <summary>Mirror of Kalandra: the caller makes the copy (and marks it mirrored); the item itself is unchanged.</summary>
        public bool Copy;
    }

    /// <summary>The tables and settings an orb needs.</summary>
    internal sealed class OrbContext
    {
        public IDictionary<string, AffixTable.Affix> Affixes;
        public LootRules Rules;
        public OrbChanceTable Chance;

        /// <summary>Whether a unique can come from this item (ROADMAP L6; none exist yet, so false).</summary>
        public bool UniqueAvailable;
    }

    /// <summary>
    ///     The currency orbs at the Riftforge (corruption-design.md → Currency orbs; riftforge-ui-brief.md §2): which
    ///     items each one works on, why it refuses, what it shows before use and what it does. Krogon's rules of
    ///     2026-11-08 (issue #28): Augmentation on a Magic item with 0 or 1 affix; Regal only with exactly 2; Annulment
    ///     never changes the rarity; Chance 50/30/20 from <c>orb_chance.csv</c>, always Rare while no unique fits; the
    ///     Mirror copies corrupted items too; the Jeweller's Orb is greyed until sockets exist. No game dependencies, so
    ///     it is unit-tested.
    /// </summary>
    internal static class OrbRules
    {
        /// <summary>Orb item prefab names start with this (the Currency tab recognises them by it).</summary>
        public const string ItemPrefix = "CorruptionOrb_";

        /// <summary>Every orb id, in the order the window lists them (the design's table).</summary>
        public static readonly string[] Orbs =
            { "transmutation", "augmentation", "alchemy", "regal", "exalted", "divine", "annulment", "chance", "jeweller", "mirror" };

        // Refusal texts (riftforge-ui-brief.md §2).
        public const string OnlyNormal = "Only on Normal items";
        public const string OnlyMagic = "Only on Magic items";
        public const string OnlyMagicWithTwo = "Only on Magic items with 2 affixes";
        public const string OnlyRare = "Only on Rare items";
        public const string OnlyMagicOrRare = "Only on Magic and Rare items";
        public const string NoRoom = "This item has no room for another affix";
        public const string NoAffixes = "This item has no affixes";
        public const string CantTakeAffixes = "This item can't take affixes";
        public const string CorruptedLocked = "Corrupted items can't be changed";
        public const string UniqueOnly = "Uniques only take Divine and Jeweller's orbs";
        public const string MirroredLocked = "Mirrored items can't be changed";
        public const string MirroredCopy = "Mirrored items can't be copied";
        public const string SocketsLater = "Sockets come later";

        /// <summary>The operation id the window and <c>co_craft</c> use ("orb:regal").</summary>
        public static string OperationId(string orb) => "orb:" + orb;

        /// <summary>The orb's item prefab name ("CorruptionOrb_Regal").</summary>
        public static string Prefab(string orb) => ItemPrefix + char.ToUpperInvariant(orb[0]) + orb.Substring(1);

        /// <summary>The orb a prefab name is, or null.</summary>
        public static string OrbOf(string prefab)
        {
            if (string.IsNullOrEmpty(prefab) || !prefab.StartsWith(ItemPrefix, StringComparison.Ordinal))
            {
                return null;
            }
            string id = prefab.Substring(ItemPrefix.Length).ToLowerInvariant();
            return Orbs.Contains(id) ? id : null;
        }

        /// <summary>The orb's name, as the design calls it.</summary>
        public static string Name(string orb)
        {
            switch (orb)
            {
                case "regal": return "Regal Orb";
                case "exalted": return "Exalted Orb";
                case "divine": return "Divine Orb";
                case "jeweller": return "Jeweller's Orb";
                case "mirror": return "Mirror of Kalandra";
                default: return "Orb of " + char.ToUpperInvariant(orb[0]) + orb.Substring(1);
            }
        }

        /// <summary>The orb item's description: what it does (corruption-design.md → The orbs, with Krogon's 2026-11-08 rules).</summary>
        public static string Description(string orb)
        {
            switch (orb)
            {
                case "transmutation": return "Turns a Normal item Magic with 1 random affix. Use it at a Riftforge.";
                case "augmentation": return "Adds a random affix to a Magic item with 0 or 1 affix; it stays Magic. Use it at a Riftforge.";
                case "alchemy": return "Turns a Normal item Rare with 3–4 random affixes. Use it at a Riftforge.";
                case "regal": return "Turns a Magic item with 2 affixes Rare, adding a third. Use it at a Riftforge.";
                case "exalted": return "Adds a random affix to a Rare item with room. Use it at a Riftforge.";
                case "divine": return "Rerolls the numbers on an item's affixes. Use it at a Riftforge.";
                case "annulment": return "Removes one random affix; the item keeps its rarity. Use it at a Riftforge.";
                case "chance": return "Turns a Normal item into a random rarity, sometimes a unique. Use it at a Riftforge.";
                case "jeweller": return "Adds a socket. Sockets come later: it can't be used yet.";
                case "mirror": return "Makes an exact copy of a Magic or Rare item, corrupted ones too. The copy can never be changed. Use it at a Riftforge.";
                default: return "";
            }
        }

        /// <summary>Why the orb can't be used on the item, or "" when it can. The first reason that applies (brief §2).</summary>
        public static string Refusal(string orb, CraftItem item, OrbContext context)
        {
            switch (orb)
            {
                case "jeweller":
                    return SocketsLater;
                case "mirror":
                    if (item.Rarity == "unique")
                    {
                        return UniqueOnly;
                    }
                    if (item.Rarity != "magic" && item.Rarity != "rare")
                    {
                        return OnlyMagicOrRare;
                    }
                    // Corrupted items can be mirrored (Krogon, 2026-11-08): the one exception to the corruption lock.
                    return item.Mirrored ? MirroredCopy : "";
            }

            string rarity = RarityRefusal(orb, item);
            if (rarity.Length > 0)
            {
                return rarity;
            }
            if (item.Corrupted)
            {
                return CorruptedLocked;
            }
            if (item.Mirrored)
            {
                return MirroredLocked;
            }
            switch (orb)
            {
                case "transmutation":
                case "alchemy":
                case "chance":
                    return CanAdd(new LootItem(), orb == "transmutation" ? "magic" : "rare", item, context) ? "" : CantTakeAffixes;
                case "augmentation":
                    if (item.AffixCount >= 2)
                    {
                        return NoRoom;
                    }
                    return CanAdd(item.Loot, "magic", item, context) ? "" : item.Classes == null ? CantTakeAffixes : NoRoom;
                case "regal":
                    if (item.AffixCount != 2)
                    {
                        return OnlyMagicWithTwo;
                    }
                    return CanAdd(item.Loot, "rare", item, context) ? "" : item.Classes == null ? CantTakeAffixes : NoRoom;
                case "exalted":
                    return CanAdd(item.Loot, "rare", item, context) ? "" : item.Classes == null ? CantTakeAffixes : NoRoom;
                case "divine":
                case "annulment":
                    return item.AffixCount > 0 ? "" : NoAffixes;
                default:
                    return "Unknown orb";
            }
        }

        private static string RarityRefusal(string orb, CraftItem item)
        {
            switch (orb)
            {
                case "transmutation":
                case "alchemy":
                case "chance":
                    return item.Rarity == "normal" ? "" : OnlyNormal;
                case "augmentation":
                    return item.Rarity == "magic" ? "" : OnlyMagic;
                case "regal":
                    return item.Rarity == "magic" ? "" : OnlyMagic;
                case "exalted":
                    return item.Rarity == "rare" ? "" : OnlyRare;
                case "divine":
                    return item.Rarity == "normal" ? NoAffixes : "";
                case "annulment":
                    return item.Rarity == "unique" ? UniqueOnly : item.Rarity == "normal" ? NoAffixes : "";
                default:
                    return "";
            }
        }

        /// <summary>Whether one more affix fits on the item at the given rarity.</summary>
        private static bool CanAdd(LootItem loot, string rarity, CraftItem item, OrbContext context)
        {
            return item.Classes != null &&
                   AffixRoller.AddCandidates(loot ?? new LootItem(), rarity, context.Affixes, item.Classes, item.Mechanics).Count > 0;
        }

        /// <summary>The lines the window shows before the player confirms (brief §2 "Preview shows").</summary>
        public static List<string> Preview(string orb, CraftItem item, OrbContext context)
        {
            var lines = new List<string>();
            switch (orb)
            {
                case "transmutation":
                    lines.Add("Becomes Magic with 1 random affix");
                    break;
                case "augmentation":
                    lines.Add("Adds 1 random affix; stays Magic");
                    break;
                case "alchemy":
                    lines.Add("Becomes Rare with 3–4 random affixes");
                    break;
                case "regal":
                    lines.Add("Becomes Rare, keeps its 2 affixes and adds 1");
                    break;
                case "exalted":
                    lines.Add("Adds 1 random affix");
                    break;
                case "divine":
                    if (item.Loot != null)
                    {
                        lines.AddRange(item.Loot.Lines.Select(l => LootItem.LineText(l, context.Affixes) + " → rerolled"));
                    }
                    break;
                case "annulment":
                    lines.Add("Removes 1 random affix; the item keeps its rarity");
                    if (item.Loot != null && item.Loot.Lines.Any(l => l.Resonant))
                    {
                        lines.Add("A resonant line loses one half; the other half rerolls in its own range");
                    }
                    break;
                case "chance":
                    lines.Add(context.UniqueAvailable && context.Chance != null
                        ? string.Join(" · ", OrbChanceTable.Outcomes.Select(o => Percent(context.Chance.Percent[o]) + " " + Capital(o)))
                        : "Becomes Rare (no unique can come from this item yet)");
                    break;
                case "mirror":
                    lines.Add("Makes an exact copy; the copy can never be changed or mirrored");
                    break;
            }
            return lines;
        }

        private static string Percent(float value) => value.ToString("0.##", CultureInfo.InvariantCulture) + "%";

        private static string Capital(string word) => char.ToUpperInvariant(word[0]) + word.Substring(1);

        /// <summary>Uses the orb on the item. Checks <see cref="Refusal" /> first; never changes <paramref name="item" />.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public static OrbResult Apply(string orb, CraftItem item, OrbContext context, Func<double> random)
        {
            var result = new OrbResult { Rarity = item.Rarity, Loot = Copy(item.Loot) };
            result.Refusal = Refusal(orb, item, context);
            if (result.Refusal.Length > 0)
            {
                return result;
            }
            switch (orb)
            {
                case "transmutation":
                    Reroll(result, "magic", 1, item, context, random);
                    break;
                case "alchemy":
                    Reroll(result, "rare", context.Rules.AffixCount("rare", random), item, context, random);
                    break;
                case "chance":
                {
                    string outcome = context.Chance.Roll(random);
                    // No unique can come from the item (none exist yet): always Rare (Krogon, 2026-11-08).
                    if (!context.UniqueAvailable)
                    {
                        outcome = "rare";
                    }
                    if (outcome == "unique")
                    {
                        // Uniques are ROADMAP L6; UniqueAvailable stays false until they exist.
                        result.Refusal = "Uniques don't exist yet";
                        return result;
                    }
                    Reroll(result, outcome, context.Rules.AffixCount(outcome, random), item, context, random);
                    break;
                }
                case "augmentation":
                    result.Gained.Add(AffixRoller.AddOne(result.Loot, "magic", context.Affixes, item.Classes, item.Mechanics, random));
                    break;
                case "regal":
                    result.Rarity = result.Loot.Rarity = "rare";
                    result.Loot.Name = context.Rules.RareName(item.Classes, random);
                    result.Gained.Add(AffixRoller.AddOne(result.Loot, "rare", context.Affixes, item.Classes, item.Mechanics, random));
                    break;
                case "exalted":
                    result.Gained.Add(AffixRoller.AddOne(result.Loot, "rare", context.Affixes, item.Classes, item.Mechanics, random));
                    break;
                case "divine":
                    foreach (var line in result.Loot.Lines)
                    {
                        LootItem.Range(line, context.Affixes, out float min, out float max);
                        line.Value = AffixRoller.RollValue(min, max, LootItem.Decimals(line, context.Affixes), random);
                        result.Gained.Add(line);
                    }
                    break;
                case "annulment":
                    Annul(result, context, random);
                    break;
                case "mirror":
                    result.Copy = true;
                    break;
            }
            result.Gained.RemoveAll(l => l == null);
            result.Done = true;
            return result;
        }

        /// <summary>A Normal item made Magic or Rare: fresh affixes, and a name for a Rare.</summary>
        private static void Reroll(OrbResult result, string rarity, int count, CraftItem item, OrbContext context, Func<double> random)
        {
            result.Rarity = rarity;
            result.Loot = AffixRoller.Roll(rarity, count, context.Affixes.Values, item.Classes, item.Mechanics, random);
            if (rarity == "rare")
            {
                result.Loot.Name = context.Rules.RareName(item.Classes, random);
            }
            result.Gained.AddRange(result.Loot.Lines);
        }

        /// <summary>
        ///     Annulment: one random affix goes, each affix equally likely (a resonant line is 2 affixes). Taking a half of
        ///     a resonant line ends the resonance; the other half stays and rerolls in its own range (AFFIX_RULES → Twin
        ///     mods and resonance). The rarity and a Rare's name stay (Krogon, 2026-11-08).
        /// </summary>
        private static void Annul(OrbResult result, OrbContext context, Func<double> random)
        {
            var loot = result.Loot;
            int total = loot.Lines.Sum(l => l.Ids.Length);
            int pick = Math.Min(total - 1, (int)(random() * total));
            for (int i = 0; i < loot.Lines.Count; i++)
            {
                var line = loot.Lines[i];
                if (pick >= line.Ids.Length)
                {
                    pick -= line.Ids.Length;
                    continue;
                }
                result.Lost.Add(new LootItem.Line { Ids = line.Ids, Value = line.Value });
                if (!line.Resonant)
                {
                    loot.Lines.RemoveAt(i);
                    return;
                }
                var kept = new LootItem.Line { Ids = new[] { line.Ids[1 - pick] } };
                LootItem.Range(kept, context.Affixes, out float min, out float max);
                kept.Value = AffixRoller.RollValue(min, max, LootItem.Decimals(kept, context.Affixes), random);
                loot.Lines[i] = kept;
                result.Gained.Add(kept);
                return;
            }
        }

        private static LootItem Copy(LootItem loot)
        {
            var copy = new LootItem();
            if (loot == null)
            {
                return copy;
            }
            copy.Rarity = loot.Rarity;
            copy.Name = loot.Name;
            copy.Lines.AddRange(loot.Lines.Select(l => new LootItem.Line { Ids = l.Ids.ToArray(), Value = l.Value }));
            return copy;
        }
    }

    /// <summary>
    ///     Reads and checks the Orb of Chance table (<c>Corruption/Data/orb_chance.csv</c>) and rolls its outcome. No game
    ///     dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class OrbChanceTable
    {
        /// <summary>What a Normal item can become. A new outcome needs code.</summary>
        public static readonly string[] Outcomes = { "magic", "rare", "unique" };

        private static readonly string[] RequiredColumns = { "outcome", "status", "chance" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        /// <summary>Outcome → chance in percent. The chances add up to 100.</summary>
        public readonly Dictionary<string, float> Percent = new Dictionary<string, float>();

        /// <summary>Parses the CSV text. Every problem found is added to <paramref name="errors" />, naming the line.</summary>
        public static OrbChanceTable Parse(string csv, List<string> errors)
        {
            var table = new OrbChanceTable();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("orb_chance.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"orb_chance.csv: missing column '{column}'.");
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

                string outcome = Cell("outcome").ToLowerInvariant();
                string where = $"orb_chance.csv line {i + 1} ({(outcome.Length > 0 ? outcome : "no outcome")})";
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Outcomes.Contains(outcome))
                {
                    errors.Add($"{where}: outcome must be one of {string.Join(", ", Outcomes)}.");
                    continue;
                }
                if (table.Percent.ContainsKey(outcome))
                {
                    errors.Add($"{where}: outcome is listed twice.");
                    continue;
                }
                if (!float.TryParse(Cell("chance"), NumberStyles.Float, CultureInfo.InvariantCulture, out var chance) || chance < 0f)
                {
                    errors.Add($"{where}: chance '{Cell("chance")}' must be a percentage of 0 or more (use a dot for decimals).");
                    continue;
                }
                table.Percent[outcome] = chance;
            }

            foreach (var outcome in Outcomes.Where(o => !table.Percent.ContainsKey(o)))
            {
                errors.Add($"orb_chance.csv: no row for {outcome}.");
            }
            float total = table.Percent.Values.Sum();
            if (table.Percent.Count == Outcomes.Length && Math.Abs(total - 100f) > 0.01f)
            {
                errors.Add($"orb_chance.csv: the chances add up to {total.ToString(CultureInfo.InvariantCulture)}, not 100.");
            }
            return table;
        }

        /// <summary>What the Normal item becomes: one of <see cref="Outcomes" />.</summary>
        /// <param name="random">Returns a value in [0, 1).</param>
        public string Roll(Func<double> random)
        {
            double roll = random() * 100.0;
            double upTo = 0.0;
            foreach (var outcome in Outcomes)
            {
                upTo += Percent.TryGetValue(outcome, out var chance) ? chance : 0f;
                if (roll < upTo)
                {
                    return outcome;
                }
            }
            return Outcomes[Outcomes.Length - 1];
        }
    }
}
