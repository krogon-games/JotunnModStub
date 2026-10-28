using System;
using System.Collections.Generic;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     Groups creatures into talisman families from game data, without a hand-written list.
    ///     <list type="bullet">
    ///         <item>A family starts from a base creature (its "primary"): the shortest name of the group.</item>
    ///         <item>
    ///             Every creature whose prefab name extends a primary's name, followed by '_' or a capital letter,
    ///             joins that family: Greydwarf → Greydwarf_Elite, Greydwarf_Shaman; Goblin → GoblinArcher.
    ///             A creature joins the family with the longest matching primary.
    ///         </item>
    ///         <item>A family needs at least one member that drops a trophy; that trophy is the recipe ingredient.</item>
    ///         <item>Bosses, creatures that aren't monsters (e.g. deer) and excluded names never take part.</item>
    ///     </list>
    ///     No game dependencies, so it is unit-tested against the real 1.0 creature list.
    /// </summary>
    internal static class TalismanFamilies
    {
        public sealed class Creature
        {
            public string Prefab;
            public bool IsBoss;
            public bool IsMonster;
            public readonly List<string> Trophies = new List<string>();
        }

        public sealed class Family
        {
            public string Primary;
            public string Trophy;
            public readonly List<string> Members = new List<string>();
        }

        public static List<Family> Build(IEnumerable<Creature> creatures, ICollection<string> excluded)
        {
            return Build(creatures, excluded.Contains);
        }

        public static List<Family> Build(IEnumerable<Creature> creatures, Func<string, bool> isExcluded)
        {
            var eligible = creatures
                .Where(c => c != null && !string.IsNullOrEmpty(c.Prefab) && !c.IsBoss && c.IsMonster &&
                            !isExcluded(c.Prefab))
                .GroupBy(c => c.Prefab).Select(g => g.First())
                .ToList();

            // Shortest names first, so a base creature starts the family before its variants are considered.
            var families = new List<Family>();
            foreach (var c in eligible.OrderBy(c => c.Prefab.Length).ThenBy(c => c.Prefab, StringComparer.Ordinal))
            {
                if (!families.Any(f => Extends(c.Prefab, f.Primary)))
                {
                    families.Add(new Family { Primary = c.Prefab });
                }
            }

            var byPrefab = eligible.ToDictionary(c => c.Prefab);
            foreach (var c in eligible)
            {
                var family = families
                    .Where(f => Extends(c.Prefab, f.Primary))
                    .OrderByDescending(f => f.Primary.Length)
                    .FirstOrDefault();
                family?.Members.Add(c.Prefab);
            }

            // The recipe trophy: the base creature's own, otherwise the one most members drop (the base
            // creature itself may drop nothing, e.g. Skeleton in 1.0). Families without any trophy are dropped.
            foreach (var f in families)
            {
                var own = byPrefab[f.Primary].Trophies;
                f.Trophy = own.Count > 0
                    ? own[0]
                    : f.Members.SelectMany(m => byPrefab[m].Trophies)
                        .GroupBy(t => t)
                        .OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal)
                        .Select(g => g.Key)
                        .FirstOrDefault();
            }
            families.RemoveAll(f => f.Trophy == null);

            foreach (var f in families)
            {
                f.Members.Sort(StringComparer.Ordinal);
            }
            return families;
        }

        /// <summary>Exclusion patterns: exact names, with '*' matching any text (e.g. "*Hildir*").</summary>
        public static Func<string, bool> Excluder(string commaSeparatedPatterns)
        {
            var patterns = (commaSeparatedPatterns ?? "").Split(',')
                .Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            return name => patterns.Any(p => Matches(name, p));
        }

        public static bool Matches(string name, string pattern)
        {
            var parts = pattern.Split('*');
            if (parts.Length == 1)
            {
                return string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase);
            }
            int pos = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.Length == 0)
                {
                    continue;
                }
                int found = name.IndexOf(part, pos, StringComparison.OrdinalIgnoreCase);
                if (found < 0 || (i == 0 && found != 0))
                {
                    return false;
                }
                pos = found + part.Length;
            }
            return parts[parts.Length - 1].Length == 0 || pos == name.Length ||
                   name.EndsWith(parts[parts.Length - 1], StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>True for the primary itself, or "Primary_..." / "PrimaryCapital...".</summary>
        public static bool Extends(string name, string primary)
        {
            if (name == primary)
            {
                return true;
            }
            if (name.Length <= primary.Length || !name.StartsWith(primary, StringComparison.Ordinal))
            {
                return false;
            }
            char next = name[primary.Length];
            return next == '_' || char.IsUpper(next);
        }
    }
}
