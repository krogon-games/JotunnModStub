using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Corruption.Rules
{
    /// <summary>
    ///     How each Affix Splinter looks (Krogon, 2026-11-10, after Path of Exile's catalysts): a shape per kind of affix
    ///     and a colour per stat. Reads and checks <c>Corruption/Data/splinter_looks.csv</c>, one row per affix family
    ///     and pool group (gear, any, gathering). Every splinter of one family is its own item,
    ///     <c>CorruptionAffixSplinter_&lt;family&gt;</c> (gathering ones <c>CorruptionAffixSplinter_tool_&lt;family&gt;</c>),
    ///     so saved splinters keep their item even if the table's colours change. No game dependencies, so it is unit-tested.
    /// </summary>
    internal sealed class SplinterLooks
    {
        /// <summary>The nine shapes (models <c>splinter_&lt;kind&gt;.obj</c>, made by <c>tools/make_splinters.py</c>).</summary>
        public static readonly string[] Kinds =
            { "attack", "elemental", "defence", "vitality", "resistance", "utility", "mining", "woodcutting", "gathering" };

        /// <summary>Pool groups: gear, any (held items), gathering (every gathering-* pool).</summary>
        public static readonly string[] Pools = { "gear", "any", "gathering" };

        private static readonly string[] RequiredColumns = { "pool", "family", "status", "kind", "colour" };
        private static readonly string[] Statuses = { "approved", "proposed", "draft" };

        public sealed class Look
        {
            public string Pool;
            public string Family;
            public string Kind;

            /// <summary>"#RRGGBB".</summary>
            public string Colour;

            /// <summary>The splinter item's prefab name for this look.</summary>
            public string Prefab => PrefabFor(Pool, Family);

            /// <summary>The colour as 0–1 channels.</summary>
            public float R => Channel(1);
            public float G => Channel(3);
            public float B => Channel(5);

            private float Channel(int at) => int.Parse(Colour.Substring(at, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255f;
        }

        public readonly List<Look> Looks = new List<Look>();

        /// <summary>The pool group an affix's pool belongs to ("gathering-mining" → "gathering").</summary>
        public static string PoolGroup(string pool)
        {
            return pool != null && pool.StartsWith("gathering", StringComparison.Ordinal) ? "gathering" : pool;
        }

        /// <summary>The splinter item for a family: <c>CorruptionAffixSplinter_fire_damage</c>, tools' <c>CorruptionAffixSplinter_tool_bountiful</c>.</summary>
        public static string PrefabFor(string poolGroup, string family)
        {
            return ShatterRules.SplinterPrefab + "_" + (poolGroup == "gathering" ? "tool_" : "") + family;
        }

        /// <summary>The look of a splinter holding this affix, or null when the table has no row for it.</summary>
        public Look For(AffixTable.Affix affix)
        {
            if (affix == null)
            {
                return null;
            }
            string pool = PoolGroup(affix.Pool);
            return Looks.FirstOrDefault(l => l.Pool == pool && l.Family == affix.Family);
        }

        /// <summary>
        ///     Parses the CSV text. Every problem is added to <paramref name="errors" />, naming the line. With
        ///     <paramref name="affixes" />, also checks every affix has a row and every row an affix.
        /// </summary>
        public static SplinterLooks Parse(string csv, List<string> errors, IEnumerable<AffixTable.Affix> affixes = null)
        {
            var table = new SplinterLooks();
            var rows = Csv.ReadRows(csv);
            if (rows.Count == 0)
            {
                errors.Add("splinter_looks.csv is empty.");
                return table;
            }
            var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
            var missing = RequiredColumns.Where(c => !header.Contains(c)).ToList();
            foreach (var column in missing)
            {
                errors.Add($"splinter_looks.csv: missing column '{column}'.");
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

                var look = new Look { Pool = Cell("pool").ToLowerInvariant(), Family = Cell("family"), Kind = Cell("kind").ToLowerInvariant(), Colour = Cell("colour") };
                string where = $"splinter_looks.csv line {i + 1} ({(look.Family.Length > 0 ? look.Family : "no family")})";
                bool ok = true;
                if (!Statuses.Contains(Cell("status").ToLowerInvariant()))
                {
                    errors.Add($"{where}: status '{Cell("status")}' must be one of {string.Join(", ", Statuses)}.");
                }
                if (!Pools.Contains(look.Pool))
                {
                    errors.Add($"{where}: pool '{look.Pool}' must be one of {string.Join(", ", Pools)}.");
                    ok = false;
                }
                if (look.Family.Length == 0 || look.Family.Any(c => !(char.IsLetterOrDigit(c) || c == '_')))
                {
                    errors.Add($"{where}: family must be letters, digits and _ (it becomes part of an item name).");
                    ok = false;
                }
                if (!Kinds.Contains(look.Kind))
                {
                    errors.Add($"{where}: kind '{look.Kind}' must be one of {string.Join(", ", Kinds)}.");
                    ok = false;
                }
                if (look.Colour.Length != 7 || look.Colour[0] != '#' ||
                    !int.TryParse(look.Colour.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                {
                    errors.Add($"{where}: colour '{look.Colour}' must be like #C8402E.");
                    ok = false;
                }
                if (ok && table.Looks.Any(l => l.Pool == look.Pool && l.Family == look.Family))
                {
                    errors.Add($"{where}: {look.Pool} {look.Family} is listed twice.");
                    ok = false;
                }
                if (ok)
                {
                    table.Looks.Add(look);
                }
            }
            if (affixes != null)
            {
                var list = affixes.ToList();
                foreach (var affix in list.Where(a => table.For(a) == null).GroupBy(a => PoolGroup(a.Pool) + " " + a.Family))
                {
                    errors.Add($"splinter_looks.csv: no row for {affix.Key} (affix {affix.First().Id}); every affix family needs a look.");
                }
                foreach (var look in table.Looks.Where(l => !list.Any(a => PoolGroup(a.Pool) == l.Pool && a.Family == l.Family)))
                {
                    errors.Add($"splinter_looks.csv: {look.Pool} {look.Family} matches no affix in affixes.csv.");
                }
            }
            return table;
        }
    }
}
