using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CraftingOverhaul.Classes;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    /// <summary>
    ///     The Amazon's real skill tree (Orlog's <c>amazon_skills.csv</c>) as the classes UI sees it, for the UI's layout
    ///     tests. Replaced the classes UI demo's sample data when the demo was removed (user, 2026-11-13).
    /// </summary>
    internal static class ClassesFixture
    {
        public static List<SkillView> AmazonSkills()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "amazon_skills.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            var errors = new List<string>();
            var table = SkillTable.Parse(File.ReadAllText(Path.Combine(dir, "Orlog", "Data", "amazon_skills.csv")), "amazon_skills.csv", errors);
            Assert.Empty(errors);
            return table.Skills.Select(s => new SkillView
            {
                Id = s.Id,
                Name = s.Name,
                Tab = s.Tab,
                Row = s.Row,
                Col = s.Col,
                ReqLevel = s.ReqLevel,
                Prereqs = s.Prereqs,
                Kind = s.Kind,
                Status = s.Status,
                MaxPoints = 20,
                Description = s.Description,
                NextDescription = "",
                SpendBlocked = ""
            }).ToList();
        }
    }
}
