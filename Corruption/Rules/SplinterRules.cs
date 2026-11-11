using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>What applying a splinter did. Nothing changes on the item until the caller writes <see cref="Loot" /> back.</summary>
    internal sealed class SplinterResult
    {
        public bool Done;

        /// <summary>Why nothing happened ("" when done).</summary>
        public string Refusal = "";

        /// <summary>The item's rarity afterwards (a Normal item becomes Magic).</summary>
        public string Rarity;

        /// <summary>The item's affixes afterwards (a new object; the input is left as it was).</summary>
        public LootItem Loot;

        /// <summary>The line the splinter added, or the resonant line it completed.</summary>
        public LootItem.Line Line;

        /// <summary>Whether the splinter completed a twin on the item.</summary>
        public bool CompletedTwin;
    }

    /// <summary>
    ///     Affix Splinters (corruption-design.md → Affix Splinters and shattering; issue #29): what a splinter holds, how
    ///     it is saved on the splinter item, which items take it and what it does. A splinter moves one affix line and
    ///     its exact value. Completing a twin is deterministic (Krogon, 2026-11-10): the resonant line is the item's
    ///     value plus the splinter's, plus the resonance bonus (+25%), with no roll. An already-resonant splinter
    ///     transfers its exact value. No game dependencies, so it is unit-tested.
    /// </summary>
    internal static class SplinterRules
    {
        /// <summary>The splinter item's custom data: the line it holds, saved as a line on gear is ("id=12", "prefix+suffix=40").</summary>
        public const string LineKey = "corruption.splinter";

        public const string OperationId = "splinter";

        // Refusal texts (riftforge-ui-brief.md §2).
        public const string OnlyNormalOrMagic = "Splinters only go on Normal and Magic items";
        public const string NoRoom = "This item has no room for this splinter";
        public const string CantRoll = "This affix can't roll on this item";
        public const string SameKind = "This item already has that kind of affix";
        public const string UnreadableSplinter = "This splinter's affix can't be read";

        /// <summary>Writes the held line into a splinter item's custom data. A splinter's line is never "corrupted".</summary>
        public static void Write(LootItem.Line line, IDictionary<string, string> customData)
        {
            customData[LineKey] = LootItem.LineToText(new LootItem.Line { Ids = line.Ids, Value = line.Value });
        }

        /// <summary>The line a splinter item holds, or null when it has none or it can't be read (the saved text is left alone).</summary>
        public static LootItem.Line Read(IDictionary<string, string> customData)
        {
            if (customData == null || !customData.TryGetValue(LineKey, out var text))
            {
                return null;
            }
            var line = LootItem.LineFromText(text);
            return line == null || line.Corrupted ? null : line;
        }

        /// <summary>The splinter a shattered line becomes: the same affix (or twin) and the exact value.</summary>
        public static LootItem.Line FromShattered(LootItem.Line line)
        {
            return new LootItem.Line { Ids = line.Ids.ToArray(), Value = line.Value };
        }

        /// <summary>Why the splinter can't go on the item, or "" when it can. The first reason that applies (brief §2).</summary>
        public static string Refusal(CraftItem item, LootItem.Line splinter, IDictionary<string, AffixTable.Affix> affixes)
        {
            if (splinter == null || splinter.Ids.Any(id => !affixes.ContainsKey(id)))
            {
                return UnreadableSplinter;
            }
            if (item.Corrupted || item.Rarity != "normal" && item.Rarity != "magic")
            {
                return OnlyNormalOrMagic;
            }
            if (item.Mirrored)
            {
                return OrbRules.MirroredLocked;
            }
            if (!ShatterRules.CanTakeSplinter(item.Rarity, item.Corrupted, item.AffixCount, splinter.Resonant))
            {
                return NoRoom;
            }
            var held = splinter.Ids.Select(id => affixes[id]).ToList();
            if (item.Classes == null || held.Any(a => !a.Classes.Any(item.Classes.Contains) ||
                                                      a.Requires.Length > 0 && !item.Mechanics.Contains(a.Requires)))
            {
                return CantRoll;
            }
            var present = item.Loot == null
                ? new List<AffixTable.Affix>()
                : item.Loot.Lines.SelectMany(l => l.Ids).Where(affixes.ContainsKey).Select(id => affixes[id]).ToList();
            foreach (var affix in held)
            {
                // Magic's limit: one prefix and one suffix. The same family on that side reads as "already has that kind".
                if (present.Any(p => p.IsPrefix == affix.IsPrefix && p.Family == affix.Family))
                {
                    return SameKind;
                }
                if (present.Any(p => p.IsPrefix == affix.IsPrefix))
                {
                    return NoRoom;
                }
            }
            return "";
        }

        /// <summary>The line the item would end with: what the window shows before the click, and what <see cref="Apply" /> writes.</summary>
        public static LootItem.Line Outcome(CraftItem item, LootItem.Line splinter, IDictionary<string, AffixTable.Affix> affixes, out bool completesTwin)
        {
            completesTwin = false;
            var twin = Twin(item.Loot, splinter, affixes);
            if (twin == null)
            {
                return new LootItem.Line { Ids = splinter.Ids.ToArray(), Value = splinter.Value };
            }
            completesTwin = true;
            var affix = affixes[splinter.Ids[0]];
            var merged = new LootItem.Line { Ids = affix.IsPrefix ? new[] { affix.Id, twin.Ids[0] } : new[] { twin.Ids[0], affix.Id } };
            merged.Value = ResonantSum(twin.Value, splinter.Value, LootItem.Decimals(merged, affixes));
            return merged;
        }

        /// <summary>
        ///     A twin completed by a splinter (Krogon, 2026-11-10): the two exact values added, plus the resonance bonus
        ///     (+25%, the same bonus a rolled resonant line's top gets), rounded as the affix rolls. Hale +14 and of the
        ///     Bear +18 give +40. Nothing is rolled.
        /// </summary>
        public static float ResonantSum(float onItem, float onSplinter, int decimals)
        {
            return (float)Math.Round((onItem + onSplinter) * LootItem.ResonanceBonus, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>The item's line the splinter would resonate with: the other half of its twin, not already resonant.</summary>
        private static LootItem.Line Twin(LootItem loot, LootItem.Line splinter, IDictionary<string, AffixTable.Affix> affixes)
        {
            if (loot == null || splinter.Resonant || !affixes.TryGetValue(splinter.Ids[0], out var affix))
            {
                return null;
            }
            return loot.Lines.FirstOrDefault(l => !l.Resonant && !l.Corrupted && affixes.TryGetValue(l.Ids[0], out var other) &&
                                                  other.Family == affix.Family && other.IsPrefix != affix.IsPrefix);
        }

        /// <summary>Puts the splinter on the item. Checks <see cref="Refusal" /> first; never changes <paramref name="item" />; rolls nothing.</summary>
        public static SplinterResult Apply(CraftItem item, LootItem.Line splinter, IDictionary<string, AffixTable.Affix> affixes)
        {
            var result = new SplinterResult { Rarity = item.Rarity, Refusal = Refusal(item, splinter, affixes) };
            if (result.Refusal.Length > 0)
            {
                return result;
            }
            var loot = new LootItem { Rarity = "magic" };
            if (item.Loot != null)
            {
                loot.Name = item.Loot.Name;
                loot.Lines.AddRange(item.Loot.Lines.Select(l => new LootItem.Line { Ids = l.Ids.ToArray(), Value = l.Value, Corrupted = l.Corrupted }));
            }
            var line = Outcome(item, splinter, affixes, out bool completed);
            if (completed)
            {
                int index = item.Loot.Lines.IndexOf(Twin(item.Loot, splinter, affixes));
                loot.Lines[index] = line;
            }
            else
            {
                loot.Lines.Add(line);
            }
            // Prefixes first, as the tooltip reads; a resonant line counts as a prefix.
            var ordered = loot.Lines.OrderBy(l => affixes.TryGetValue(l.Ids[0], out var a) && a.IsPrefix ? 0 : 1).ToList();
            loot.Lines.Clear();
            loot.Lines.AddRange(ordered);
            result.Rarity = "magic";
            result.Loot = loot;
            result.Line = line;
            result.CompletedTwin = completed;
            result.Done = true;
            return result;
        }
    }
}
