using System;
using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;

namespace CraftingOverhaul.Classes
{
    /// <summary>One skill as the classes API describes it (<c>GetSkillInfoJson</c>, brief §3).</summary>
    internal sealed class SkillView
    {
        public string Id;
        public string Name;
        public int Tab;
        public int Row;
        public int Col;
        public int ReqLevel;
        public string[] Prereqs = new string[0];
        public string Kind;
        public string Status;
        public int MaxPoints;
        public int Points;
        public int Level;
        public bool CanSpend;
        public string SpendBlocked;
        public string Description;
        public string NextDescription;
        public float EitrCost;

        public bool Passive => Kind == "passive";
        public bool Later => Status == "later";
        public bool Dropped => Status == "dropped";
        public bool Maxed => MaxPoints > 0 && Points >= MaxPoints;

        public static SkillView Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            string id = FlatJson.Get(json, "id");
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            string prereqs = FlatJson.Get(json, "prereqs") ?? "";
            return new SkillView
            {
                Id = id,
                Name = FlatJson.Get(json, "name") ?? id,
                Tab = FlatJson.GetInt(json, "tab", 0),
                Row = FlatJson.GetInt(json, "row", 1),
                Col = FlatJson.GetInt(json, "col", 1),
                ReqLevel = FlatJson.GetInt(json, "reqLevel", 1),
                Prereqs = prereqs.Length == 0 ? new string[0] : prereqs.Split('+'),
                Kind = FlatJson.Get(json, "kind") ?? "attack",
                Status = FlatJson.Get(json, "status") ?? "v1",
                MaxPoints = FlatJson.GetInt(json, "maxPoints", 20),
                Points = FlatJson.GetInt(json, "points", 0),
                Level = FlatJson.GetInt(json, "level", 0),
                CanSpend = FlatJson.GetBool(json, "canSpend", false),
                SpendBlocked = FlatJson.Get(json, "spendBlocked") ?? "",
                Description = FlatJson.Get(json, "description") ?? "",
                NextDescription = FlatJson.Get(json, "nextDescription") ?? "",
                EitrCost = FlatJson.GetFloat(json, "eitrCost", 0f)
            };
        }
    }

    /// <summary>
    ///     The path from a prerequisite to its skill: one straight line from the edge of the prerequisite's medallion to
    ///     the edge of the skill's, aimed at its centre, in tree pixels (y down). With names moved to the details pane
    ///     (redesign 2026-10-31) nothing sits between the medallions, so short straight paths read cleanly, as in
    ///     Diablo 2's trees.
    /// </summary>
    internal sealed class TreeLine
    {
        public string From;
        public string To;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;

        public float Length => (float)Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        /// <summary>Angle in degrees, counter-clockwise from +x on screen (y up), for rotating a horizontal image.</summary>
        public float AngleDegrees => (float)(Math.Atan2(-(Y2 - Y1), X2 - X1) * 180.0 / Math.PI);
    }

    /// <summary>
    ///     Where each skill goes in the skills panel: one tab at a time, a grid of 6 rows (the level tiers 1, 6, 12, 18,
    ///     24, 30) by 3 columns, as Diablo 2's trees (brief §2.4); lines from each prerequisite to its skill. Skills with
    ///     status <c>dropped</c> aren't shown (their place stays empty). Pure: no Unity, unit-tested.
    /// </summary>
    internal static class SkillTreeLayout
    {
        public const int Rows = 6;
        public const int Cols = 3;
        /// <summary>A skill's medallion, in pixels.</summary>
        public const float Tile = 56f;
        /// <summary>Distance between column centres and between row centres.</summary>
        public const float ColPitch = 176f;
        public const float RowPitch = 76f;
        /// <summary>Room left of the columns for the level tier labels.</summary>
        public const float LabelWidth = 56f;
        /// <summary>Gap between a medallion's rim and the start or end of a path.</summary>
        public const float PathGap = 3f;

        public static readonly int[] TierLevels = { 1, 6, 12, 18, 24, 30 };

        public static float GridWidth => LabelWidth + Cols * ColPitch;
        public static float GridHeight => Rows * RowPitch;

        /// <summary>Top-left of the medallion for a skill at (row, col), both 1-based, in tree pixels.</summary>
        public static void TilePosition(int row, int col, out float x, out float y)
        {
            CentrePosition(row, col, out float cx, out float cy);
            x = cx - Tile / 2f;
            y = cy - Tile / 2f;
        }

        /// <summary>Centre of the medallion for a skill at (row, col), both 1-based, in tree pixels.</summary>
        public static void CentrePosition(int row, int col, out float x, out float y)
        {
            row = Math.Max(1, Math.Min(Rows, row));
            col = Math.Max(1, Math.Min(Cols, col));
            x = LabelWidth + (col - 0.5f) * ColPitch;
            y = (row - 0.5f) * RowPitch;
        }

        /// <summary>Centre line of a tier row, for its label and divider.</summary>
        public static float RowCentre(int row) => (row - 0.5f) * RowPitch;

        /// <summary>The skills of one tab that are shown (not dropped).</summary>
        public static List<SkillView> Visible(IEnumerable<SkillView> skills, int tab)
        {
            var result = new List<SkillView>();
            foreach (var skill in skills)
            {
                if (skill != null && skill.Tab == tab && !skill.Dropped)
                {
                    result.Add(skill);
                }
            }
            return result;
        }

        /// <summary>
        ///     Paths for the prerequisites of the shown skills (see <see cref="TreeLine" />): from the edge of the
        ///     prerequisite's medallion to the edge of the skill's. Prerequisites in another tab or not shown get none.
        /// </summary>
        public static List<TreeLine> Lines(IList<SkillView> shown)
        {
            var byId = new Dictionary<string, SkillView>(StringComparer.Ordinal);
            foreach (var skill in shown)
            {
                byId[skill.Id] = skill;
            }
            var lines = new List<TreeLine>();
            foreach (var skill in shown)
            {
                foreach (string id in skill.Prereqs)
                {
                    if (!byId.TryGetValue(id, out var prereq))
                    {
                        continue;
                    }
                    CentrePosition(prereq.Row, prereq.Col, out float px, out float py);
                    CentrePosition(skill.Row, skill.Col, out float sx, out float sy);
                    float dx = sx - px, dy = sy - py;
                    float length = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (length <= Tile)
                    {
                        continue;
                    }
                    float ux = dx / length, uy = dy / length;
                    float edge = Tile / 2f + PathGap;
                    lines.Add(new TreeLine
                    {
                        From = prereq.Id,
                        To = skill.Id,
                        X1 = px + ux * edge,
                        Y1 = py + uy * edge,
                        X2 = sx - ux * edge,
                        Y2 = sy - uy * edge
                    });
                }
            }
            return lines;
        }
    }
}
