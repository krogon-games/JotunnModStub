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
            return new SkillView
            {
                Id = id,
                Name = FlatJson.Get(json, "name") ?? id,
                Tab = FlatJson.GetInt(json, "tab", 0),
                Row = FlatJson.GetInt(json, "row", 1),
                Col = FlatJson.GetInt(json, "col", 1),
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

    /// <summary>How a skill's card is drawn (<see cref="SkillCardsLayout.StateOf" />).</summary>
    internal enum CardState
    {
        /// <summary>Not in the game yet: dark, "Coming later".</summary>
        Later,
        /// <summary>No points in it and none can go in now (no skill points left).</summary>
        NotLearned,
        /// <summary>No points in it yet, and a point can go in.</summary>
        CanLearn,
        /// <summary>Has points.</summary>
        Learned,
        /// <summary>Has every point it can take.</summary>
        Maxed
    }

    /// <summary>
    ///     Where each skill goes in the skills panel since the skill tree overhaul (Krogon's mock-up, 2026-11-15:
    ///     <c>docs/images/skills_panel_cards_mockup.webp</c>): one tab at a time, nine cards three by three, each with the
    ///     skill's medallion, its name, its rank and a rank bar. No tiers and no paths: any skill can be learned from
    ///     level 1. Positions in grid pixels from the grid's top-left, y down. Pure: no Unity, unit-tested.
    /// </summary>
    internal static class SkillCardsLayout
    {
        public const int Rows = 3;
        public const int Cols = 3;
        public const float CardWidth = 236f;
        public const float CardHeight = 138f;
        public const float Gap = 12f;
        /// <summary>A skill's medallion on its card, in pixels.</summary>
        public const float Medallion = 56f;

        public static float GridWidth => Cols * CardWidth + (Cols - 1) * Gap;
        public static float GridHeight => Rows * CardHeight + (Rows - 1) * Gap;

        /// <summary>The card of the skill at (row, col), both 1-based (clamped to the grid).</summary>
        public static SheetRect Card(int row, int col)
        {
            row = Math.Max(1, Math.Min(Rows, row));
            col = Math.Max(1, Math.Min(Cols, col));
            return new SheetRect((col - 1) * (CardWidth + Gap), (row - 1) * (CardHeight + Gap), CardWidth, CardHeight);
        }

        // ---- inside a card (relative to it)

        public static SheetRect Icon => new SheetRect((CardWidth - Medallion) / 2f, 12f, Medallion, Medallion);
        public static SheetRect Name => new SheetRect(8f, 72f, CardWidth - 16f, 26f);
        public static SheetRect Rank => new SheetRect(8f, 98f, CardWidth - 16f, 18f);
        /// <summary>The rank bar, shown once the skill has points.</summary>
        public static SheetRect RankBar => new SheetRect(30f, 119f, CardWidth - 60f, 9f);

        /// <summary>The skills of one tab that are shown (not dropped), in table order.</summary>
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

        public static CardState StateOf(SkillView skill)
        {
            if (skill.Later)
            {
                return CardState.Later;
            }
            if (skill.Maxed)
            {
                return CardState.Maxed;
            }
            if (skill.Points > 0)
            {
                return CardState.Learned;
            }
            return skill.CanSpend ? CardState.CanLearn : CardState.NotLearned;
        }

        /// <summary>The line under a card's name: "Coming later", "Not learned" or "Rank 3 / 20".</summary>
        public static string RankLabel(SkillView skill)
        {
            if (skill.Later)
            {
                return "Coming later";
            }
            return skill.Points > 0 ? "Rank " + skill.Points + " / " + skill.MaxPoints : "Not learned";
        }

        /// <summary>How full the rank bar is, 0 to 1.</summary>
        public static float RankFill(SkillView skill)
        {
            return skill.MaxPoints <= 0 ? 0f : Math.Max(0f, Math.Min(1f, (float)skill.Points / skill.MaxPoints));
        }
    }
}
