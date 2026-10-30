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

    /// <summary>A line between two skills in the tree, in panel pixels (y down): from the prerequisite to the skill.</summary>
    internal struct TreeLine
    {
        public string From;
        public string To;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;

        public float Length => (float)Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1));
        /// <summary>Angle of the line in degrees, counter-clockwise from the +x axis on screen (y up).</summary>
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
        public const float Tile = 64f;
        public const float ColGap = 56f;
        public const float RowGap = 36f;
        /// <summary>The skill's name under its tile; lines start below it so they don't run through the text.</summary>
        public const float NameHeight = 16f;
        /// <summary>Room left of the grid for the level tier labels.</summary>
        public const float LabelWidth = 44f;

        public static readonly int[] TierLevels = { 1, 6, 12, 18, 24, 30 };

        public static float GridWidth => LabelWidth + Cols * Tile + (Cols - 1) * ColGap;
        public static float GridHeight => Rows * Tile + (Rows - 1) * RowGap;

        /// <summary>Top-left of the tile for a skill at (row, col), both 1-based, in grid pixels.</summary>
        public static void TilePosition(int row, int col, out float x, out float y)
        {
            row = Math.Max(1, Math.Min(Rows, row));
            col = Math.Max(1, Math.Min(Cols, col));
            x = LabelWidth + (col - 1) * (Tile + ColGap);
            y = (row - 1) * (Tile + RowGap);
        }

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
        ///     Lines for the prerequisites of the shown skills: from the bottom of the prerequisite's tile to the top of
        ///     the skill's. Prerequisites in another tab or not shown get no line.
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
                    TilePosition(prereq.Row, prereq.Col, out float px, out float py);
                    TilePosition(skill.Row, skill.Col, out float sx, out float sy);
                    lines.Add(new TreeLine
                    {
                        From = prereq.Id,
                        To = skill.Id,
                        X1 = px + Tile / 2f,
                        Y1 = py + Tile + NameHeight,
                        X2 = sx + Tile / 2f,
                        Y2 = sy
                    });
                }
            }
            return lines;
        }
    }
}
