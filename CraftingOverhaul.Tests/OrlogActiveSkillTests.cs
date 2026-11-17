using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orlog.Rules;
using Xunit;

namespace CraftingOverhaul.Tests
{
    public class OrlogActiveSkillTests
    {
        private static string RepoRoot()
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "Orlog", "Data", "amazon_skill_values.csv")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            Assert.NotNull(dir);
            return dir;
        }

        private static string Data(string file) => File.ReadAllText(Path.Combine(RepoRoot(), "Orlog", "Data", file));

        private static SkillValueTable Values()
        {
            var errors = new List<string>();
            var t = SkillValueTable.Parse(Data("amazon_skill_values.csv"), "amazon_skill_values.csv", errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        private static SkillTable Tree()
        {
            var errors = new List<string>();
            var t = SkillTable.Parse(Data("amazon_skills.csv"), "amazon_skills.csv", errors);
            Assert.True(errors.Count == 0, string.Join("\n", errors));
            return t;
        }

        [Fact]
        public void EveryBuiltActiveSkillHasValuesAndText()
        {
            var v = Values();
            var built = Tree().Skills.Where(s => s.Built && s.Kind != "passive").ToList();
            Assert.Equal(built.Select(s => s.Id).OrderBy(x => x), v.SkillIds.OrderBy(x => x));
            // The bow skills and Inner Sight (C5) have their tooltip text; the spear skills come with C6.
            foreach (var id in new[] { "MultipleShot", "FreezingArrow", "InnerSight", "ForkedLightning", "WindLance", "Strafe", "ImmolationArrow", "VenomStrike" })
            {
                Assert.False(string.IsNullOrEmpty(ActiveSkillRules.Describe(id, v, 1, 1f, 25f)), id);
            }
        }

        [Fact]
        public void ValuesAreD2sNumbers()
        {
            // data/d2-reference/amazon_levels.csv, levels 1 and 20.
            var v = Values();
            Assert.Equal(2f, v.Get("MultipleShot", "arrows", 1));
            Assert.Equal(21f, v.Get("MultipleShot", "arrows", 20));
            Assert.Equal(2f, v.Get("FreezingArrow", "freeze_s", 1));
            Assert.Equal(3.3f, v.Get("FreezingArrow", "radius_yards", 5));
            Assert.Equal(84f, v.Get("InnerSight", "duration_s", 20));
            Assert.Equal(0f, v.Get("MultipleShot", "arrows", 0));
            Assert.Equal(21f, v.Get("MultipleShot", "arrows", 30));
            Assert.Equal(0f, v.Get("Nope", "arrows", 3));
        }

        [Fact]
        public void EitrCostUsesD2RsManaWhereTheTableHasIt()
        {
            var v = Values();
            var t = Tree();
            // Freezing Arrow (D2R): 9 at level 1, +0.25 per level.
            Assert.Equal(9f, ActiveSkillRules.EitrCost(t.Find("FreezingArrow"), v, 1));
            Assert.Equal(13.75f, ActiveSkillRules.EitrCost(t.Find("FreezingArrow"), v, 20));
            Assert.Equal(4f, ActiveSkillRules.EitrCost(t.Find("MultipleShot"), v, 1));
            Assert.Equal(23f, ActiveSkillRules.EitrCost(t.Find("MultipleShot"), v, 20));
            Assert.Equal(0f, ActiveSkillRules.EitrCost(t.Find("Dodge"), v, 5));
            // Without a values row, the tree's formula.
            Assert.Equal(SkillRules.EitrCost(t.Find("Jab"), 3), ActiveSkillRules.EitrCost(t.Find("Jab"), new SkillValueTable(), 3));
        }

        [Fact]
        public void SkillDamageIsAShareOfTheShot()
        {
            // Krogon, 2026-11-16 (#72): "All scale off the weapon". Freezing Arrow's blast "50% at level 1, 150% at 20";
            // each of Forked Lightning's forks "25% at level 1, 100% at 20"; even steps between, whole numbers.
            var v = Values();
            Assert.Equal(50f, v.Get("FreezingArrow", "blast_damage_pct", 1));
            Assert.Equal(97f, v.Get("FreezingArrow", "blast_damage_pct", 10));
            Assert.Equal(150f, v.Get("FreezingArrow", "blast_damage_pct", 20));
            Assert.Equal(25f, v.Get("ForkedLightning", "fork_damage_pct", 1));
            Assert.Equal(61f, v.Get("ForkedLightning", "fork_damage_pct", 10));
            Assert.Equal(100f, v.Get("ForkedLightning", "fork_damage_pct", 20));
            foreach (var row in new[] { new[] { "FreezingArrow", "blast_damage_pct" }, new[] { "ForkedLightning", "fork_damage_pct" } })
            {
                for (int level = 2; level <= 20; level++)
                {
                    Assert.True(v.Get(row[0], row[1], level) > v.Get(row[0], row[1], level - 1), row[0] + " level " + level);
                }
            }
            // Diablo 2's fixed damage points of the built skills are gone from the table.
            foreach (string gone in new[] { "cold_damage_min", "cold_damage_max" })
            {
                Assert.False(v.Has("FreezingArrow", gone), gone);
            }
            foreach (string gone in new[] { "lightning_damage_min", "lightning_damage_max" })
            {
                Assert.False(v.Has("ForkedLightning", gone), gone);
            }
            // A crude bow and a wood arrow (22 + 22 pierce, game dump): a level-1 fork is 11, a level-1 blast 22.
            Assert.Equal(11f, ActiveSkillRules.ShareOfShot(44f, v.Get("ForkedLightning", "fork_damage_pct", 1)), 3);
            Assert.Equal(22f, ActiveSkillRules.ShareOfShot(44f, v.Get("FreezingArrow", "blast_damage_pct", 1)), 3);
            Assert.Equal(44f, ActiveSkillRules.ShareOfShot(44f, 100f), 3);
            // The admin's multiplier scales it; nothing goes below zero.
            Assert.Equal(22f, ActiveSkillRules.ShareOfShot(44f, 25f, 2f), 3);
            Assert.Equal(0f, ActiveSkillRules.ShareOfShot(44f, 25f, 0f));
            Assert.Equal(0f, ActiveSkillRules.ShareOfShot(-5f, 25f));
            Assert.Equal(0f, ActiveSkillRules.ShareOfShot(44f, -25f));
            Assert.Equal(0f, ActiveSkillRules.ShareOfShot(44f, 25f, -1f));
        }

        [Fact]
        public void InnerSightGrowsOnePercentPerLevel()
        {
            Assert.Equal(0f, ActiveSkillRules.InnerSightPercent(0, 5f, 1f));
            Assert.Equal(5f, ActiveSkillRules.InnerSightPercent(1, 5f, 1f));
            Assert.Equal(14f, ActiveSkillRules.InnerSightPercent(10, 5f, 1f));
            Assert.Equal(24f, ActiveSkillRules.InnerSightPercent(20, 5f, 1f));
            Assert.Equal(27f, ActiveSkillRules.InnerSightPercent(23, 5f, 1f)); // +skills later: no cap
        }

        [Fact]
        public void SlotsKeepOnlyLearnedSkills()
        {
            var slots = ActiveSkillRules.ParseSlots("MultipleShot;InnerSight;;FreezingArrow");
            var levels = new System.Collections.Generic.Dictionary<string, int> { { "InnerSight", 3 } };
            Assert.Equal(new[] { "", "InnerSight", "", "" },
                ActiveSkillRules.OnlyLearned(slots, id => levels.TryGetValue(id, out int l) ? l : 0));
            Assert.Equal(new[] { "", "", "", "" }, ActiveSkillRules.OnlyLearned(slots, id => 0));
        }

        [Fact]
        public void SlotsSaveAndPlace()
        {
            var slots = ActiveSkillRules.ParseSlots("MultipleShot;;FreezingArrow");
            Assert.Equal(new[] { "MultipleShot", "", "FreezingArrow", "" }, slots);
            Assert.Equal("MultipleShot;;FreezingArrow;", ActiveSkillRules.FormatSlots(slots));
            // A skill already in another slot moves.
            Assert.Equal(new[] { "", "", "FreezingArrow", "MultipleShot" }, ActiveSkillRules.Place(slots, 3, "MultipleShot"));
            Assert.Equal(new[] { "MultipleShot", "", "", "" }, ActiveSkillRules.Place(slots, 2, ""));
            Assert.Equal(slots, ActiveSkillRules.Place(slots, 7, "InnerSight"));
            Assert.Equal(4, ActiveSkillRules.ParseSlots(null).Length);
        }

        [Fact]
        public void SlotRefusals()
        {
            var t = Tree();
            Assert.Equal("Passive skills don't take a slot", ActiveSkillRules.SlotRefusal(t.Find("Dodge"), 3));
            Assert.Equal("Coming later", ActiveSkillRules.SlotRefusal(t.Find("Snipe"), 0));
            Assert.Equal("Not learned yet", ActiveSkillRules.SlotRefusal(t.Find("MultipleShot"), 0));
            Assert.Equal("", ActiveSkillRules.SlotRefusal(t.Find("MultipleShot"), 1));
            Assert.Equal("Unknown skill", ActiveSkillRules.SlotRefusal(null, 1));
        }

        [Fact]
        public void SlotStateChecksWeaponThenEitr()
        {
            var t = Tree();
            var bow = t.Find("MultipleShot");
            var s = ActiveSkillRules.State(bow, "", 50f, 3f);
            Assert.False(s.Usable);
            Assert.Equal("weapon", s.Blocked);
            Assert.Equal("bow", s.Needs);
            Assert.Equal("Need bow", ActiveSkillRules.BlockedMessage(s));
            s = ActiveSkillRules.State(bow, "bow", 2f, 3f);
            Assert.Equal("eitr", s.Blocked);
            Assert.Equal("Not enough eitr", ActiveSkillRules.BlockedMessage(s));
            Assert.True(ActiveSkillRules.State(bow, "bow", 3f, 3f).Usable);
            // A cast skill needs no weapon.
            Assert.True(ActiveSkillRules.State(t.Find("InnerSight"), "", 5f, 5f).Usable);
            // A free skill works with no eitr at all.
            Assert.True(ActiveSkillRules.State(bow, "bow", 0f, 0f).Usable);
        }

        [Fact]
        public void SelectedSlotIsRememberedPerWeaponKind()
        {
            var sel = ActiveSkillRules.ParseSelected("bow:2;spear:-1;junk;axe:9");
            Assert.Equal(2, sel["bow"]);
            Assert.Equal(-1, sel["spear"]);
            Assert.False(sel.ContainsKey("axe"));
            Assert.Equal("bow:2;spear:-1", ActiveSkillRules.FormatSelected(sel));
        }

        [Fact]
        public void FanIsEvenAndCentred()
        {
            Assert.Equal(0f, ActiveSkillRules.FanAngle(0, 1));
            Assert.Equal(-2.5f, ActiveSkillRules.FanAngle(0, 2));
            Assert.Equal(2.5f, ActiveSkillRules.FanAngle(1, 2));
            // 21 arrows: 60° in all, 3° apart.
            Assert.Equal(-30f, ActiveSkillRules.FanAngle(0, 21), 3);
            Assert.Equal(30f, ActiveSkillRules.FanAngle(20, 21), 3);
            Assert.Equal(0f, ActiveSkillRules.FanAngle(10, 21), 3);
        }

        [Fact]
        public void ForkedLightningTakesChargedStrikesNumbers()
        {
            // Krogon, 2026-11-16: "Charged Strike's"; forks reach "4 m".
            var v = Values();
            var t = Tree();
            Assert.Equal(3f, v.Get("ForkedLightning", "forks", 1));
            Assert.Equal(4f, v.Get("ForkedLightning", "forks", 5));
            Assert.Equal(7f, v.Get("ForkedLightning", "forks", 20));
            Assert.Equal(4f, v.Get("ForkedLightning", "reach_m", 1));
            Assert.Equal(4f, v.Get("ForkedLightning", "reach_m", 20));
            Assert.Equal(4f, ActiveSkillRules.EitrCost(t.Find("ForkedLightning"), v, 1));
            Assert.Equal(8.75f, ActiveSkillRules.EitrCost(t.Find("ForkedLightning"), v, 20));
            Assert.True(t.Find("ForkedLightning").Built);
            Assert.Equal("", ActiveSkillRules.SlotRefusal(t.Find("ForkedLightning"), 1));
        }

        [Fact]
        public void WindLanceTakesGuidedArrowsNumbers()
        {
            // Krogon, 2026-11-16: "Guided Arrow's numbers"; the push "Grows with skill level" (100 to 200); "Twice as
            // fast"; "50 m". After his first play-test: "lets reduce it to 1.5 seconds" (it was 3) and "0.75 m each side".
            var v = Values();
            var t = Tree();
            Assert.Equal(100f, v.Get("WindLance", "weapon_damage_pct", 1));
            Assert.Equal(107f, v.Get("WindLance", "weapon_damage_pct", 2));
            Assert.Equal(233f, v.Get("WindLance", "weapon_damage_pct", 20));
            Assert.Equal(100f, v.Get("WindLance", "knockback", 1));
            Assert.Equal(200f, v.Get("WindLance", "knockback", 20));
            for (int level = 2; level <= 20; level++)
            {
                Assert.True(v.Get("WindLance", "knockback", level) > v.Get("WindLance", "knockback", level - 1), "level " + level);
            }
            Assert.Equal(1.5f, v.Get("WindLance", "hold_s", 1));
            Assert.Equal(1.5f, v.Get("WindLance", "hold_s", 20));
            Assert.Equal(0.75f, v.Get("WindLance", "width_m", 1));
            Assert.Equal(200f, v.Get("WindLance", "speed_pct", 1));
            Assert.Equal(50f, v.Get("WindLance", "range_m", 20));
            Assert.Equal(8f, ActiveSkillRules.EitrCost(t.Find("WindLance"), v, 1));
            Assert.Equal(3.25f, ActiveSkillRules.EitrCost(t.Find("WindLance"), v, 20));
            Assert.True(t.Find("WindLance").Built);
        }

        [Fact]
        public void OnlyWindLanceChargesAndItIsReadyAfterItsHold()
        {
            Assert.True(ActiveSkillRules.Charges("WindLance"));
            Assert.False(ActiveSkillRules.Charges("ForkedLightning"));
            Assert.False(ActiveSkillRules.Charges("MultipleShot"));
            Assert.False(ActiveSkillRules.Charges(null));
            Assert.False(ActiveSkillRules.ChargeReady(0f, 3f));
            Assert.False(ActiveSkillRules.ChargeReady(2.99f, 3f));
            Assert.True(ActiveSkillRules.ChargeReady(3f, 3f));
            Assert.True(ActiveSkillRules.ChargeReady(10f, 3f));
            // No hold time: ready at once.
            Assert.True(ActiveSkillRules.ChargeReady(0f, 0f));
            // How far the hold has come, for the draw circle.
            Assert.Equal(0f, ActiveSkillRules.ChargeProgress(0f, 1.5f));
            Assert.Equal(0.5f, ActiveSkillRules.ChargeProgress(0.75f, 1.5f), 3);
            Assert.Equal(1f, ActiveSkillRules.ChargeProgress(4f, 1.5f));
            Assert.Equal(1f, ActiveSkillRules.ChargeProgress(0f, 0f));
        }

        [Fact]
        public void TheDrawCircleKeepsNarrowingDuringTheCharge()
        {
            // Krogon, 2026-11-16: "it should continue to narrow in". A bow's circle is at the game's 0.15 at full draw.
            Assert.Equal(0.15f, ActiveSkillRules.ChargeCircleScale(0f, true), 3);
            Assert.Equal(ActiveSkillRules.ChargeCircleEnd, ActiveSkillRules.ChargeCircleScale(1f, true), 3);
            Assert.True(ActiveSkillRules.ChargeCircleScale(0.5f, true) < 0.15f);
            Assert.True(ActiveSkillRules.ChargeCircleScale(0.5f, true) > ActiveSkillRules.ChargeCircleEnd);
            // A crossbow has no draw: its circle runs the whole way during the hold.
            Assert.Equal(1f, ActiveSkillRules.ChargeCircleScale(0f, false), 3);
            Assert.Equal(ActiveSkillRules.ChargeCircleEnd, ActiveSkillRules.ChargeCircleScale(1f, false), 3);
            Assert.Equal(ActiveSkillRules.ChargeCircleEnd, ActiveSkillRules.ChargeCircleScale(5f, false), 3);
        }

        [Fact]
        public void TheLanceRunsAlongTheGroundInsteadOfDiggingIn()
        {
            // Krogon, 2026-11-16: "Run along the ground". Steps of 2.4 m (120 m/s at 50 steps a second); a player can
            // stand on 38 degrees, so the lance follows up to tan(38°) × 2.4 = 1.875 m of height per step.
            const float slope = ActiveSkillRules.LanceMaxSlopeDegrees;
            float y;
            // Clear of the ground: straight on.
            Assert.Equal(ActiveSkillRules.LanceGround.Free, ActiveSkillRules.LanceGroundStep(false, 10f, 9.8f, 5f, 2.4f, slope, out y));
            Assert.Equal(9.8f, y, 3);
            // Aimed down at a small creature: where it would dip under its floor it levels out at the floor.
            Assert.Equal(ActiveSkillRules.LanceGround.Skim, ActiveSkillRules.LanceGroundStep(false, 1.0f, 0.5f, 0.75f, 2.4f, slope, out y));
            Assert.Equal(0.75f, y, 3);
            // Shot steeply into the ground (steeper than 38 degrees): it strikes it.
            Assert.Equal(ActiveSkillRules.LanceGround.Blocked, ActiveSkillRules.LanceGroundStep(false, 3f, 0.5f, 0.75f, 2.4f, slope, out y));
            Assert.Equal(0.5f, y, 3);
            // Flying level into a hillside that rises more steeply than it can follow: it strikes it.
            Assert.Equal(ActiveSkillRules.LanceGround.Blocked, ActiveSkillRules.LanceGroundStep(false, 1f, 1f, 4f, 2.4f, slope, out y));
            // Running along the ground: up a gentle rise, down a gentle fall, never below the floor.
            Assert.Equal(ActiveSkillRules.LanceGround.Skim, ActiveSkillRules.LanceGroundStep(true, 1f, 1f, 2f, 2.4f, slope, out y));
            Assert.Equal(2f, y, 3);
            Assert.Equal(ActiveSkillRules.LanceGround.Skim, ActiveSkillRules.LanceGroundStep(true, 2f, 2f, 1.2f, 2.4f, slope, out y));
            Assert.Equal(1.2f, y, 3);
            // Off a cliff edge: it comes down at its steepest, not straight to the bottom.
            Assert.Equal(ActiveSkillRules.LanceGround.Skim, ActiveSkillRules.LanceGroundStep(true, 20f, 20f, 0.75f, 2.4f, slope, out y));
            Assert.Equal(20f - 1.875f, y, 2);
            // A cliff face ahead: it strikes it and keeps its height until then.
            Assert.Equal(ActiveSkillRules.LanceGround.Blocked, ActiveSkillRules.LanceGroundStep(true, 1f, 1f, 6f, 2.4f, slope, out y));
            Assert.Equal(1f, y, 3);
            // Straight up or down has no ground to follow.
            Assert.Equal(ActiveSkillRules.LanceGround.Blocked, ActiveSkillRules.LanceGroundStep(false, 3f, 0.6f, 0.75f, 0f, slope, out y));
        }

        [Fact]
        public void ForksGoToTheNearestEnemiesWithinReach()
        {
            // Five enemies at these distances from the struck one; reach 4 m.
            var distances = new List<float> { 3.5f, 9f, 1f, 4f, 2f };
            Assert.Equal(new[] { 2, 4, 0 }, ActiveSkillRules.ForkTargets(distances, 4f, 3));
            // More forks than enemies in reach: everyone in reach, once each (4 m itself counts).
            Assert.Equal(new[] { 2, 4, 0, 3 }, ActiveSkillRules.ForkTargets(distances, 4f, 7));
            Assert.Equal(new[] { 2 }, ActiveSkillRules.ForkTargets(distances, 4f, 1));
            // Nobody in reach, no forks, or no list: none.
            Assert.Empty(ActiveSkillRules.ForkTargets(distances, 0.5f, 3));
            Assert.Empty(ActiveSkillRules.ForkTargets(distances, 4f, 0));
            Assert.Empty(ActiveSkillRules.ForkTargets(null, 4f, 3));
            // Equal distances keep their order.
            Assert.Equal(new[] { 0, 1 }, ActiveSkillRules.ForkTargets(new List<float> { 2f, 2f, 2f }, 4f, 2));
        }

        [Fact]
        public void TheLancePassesThroughBushesButNotTreesOrRocks()
        {
            // Krogon, 2026-11-16: "Pass through bushes" (a Bush01 had stopped a lance). What trees and logs drop, as the
            // game dump lists it; the things below are rows of data/game-dump-1.0.20/destructibles.csv.
            var treeDrops = new HashSet<string> { "Wood", "FineWood", "Resin", "BeechSeeds", "FirCone", "Frostwood" };
            // Bush01, shrub_2: not a tree to the game, drop wood.
            Assert.True(ActiveSkillRules.IsSmallPlant(false, new[] { "Wood" }, treeDrops));
            // Beech_small1, FirTree_small, a stump: the game's own type says tree.
            Assert.False(ActiveSkillRules.IsSmallPlant(true, new[] { "Wood", "Resin" }, treeDrops));
            // Rock_7 drops stone; a bare rock or cliff drops nothing.
            Assert.False(ActiveSkillRules.IsSmallPlant(false, new[] { "Stone" }, treeDrops));
            Assert.False(ActiveSkillRules.IsSmallPlant(false, new string[0], treeDrops));
            // Wood and something else (a barrel of loot): not a plant.
            Assert.False(ActiveSkillRules.IsSmallPlant(false, new[] { "Wood", "Coins" }, treeDrops));
            Assert.False(ActiveSkillRules.IsSmallPlant(false, null, treeDrops));
            Assert.False(ActiveSkillRules.IsSmallPlant(false, new[] { "Wood" }, null));
        }

        [Fact]
        public void StrafeImmolationArrowAndVenomStrikeAreBuiltWithTheAgreedNumbers()
        {
            // Krogon, 2026-11-16 (docs/handoffs/claude-magical-babbage-slwwpn.md, decisions 7-11).
            var v = Values();
            var t = Tree();
            foreach (var id in new[] { "Strafe", "ImmolationArrow", "VenomStrike" })
            {
                Assert.True(t.Find(id).Built, id);
            }
            Assert.Equal(7, t.Skills.Count(s => s.Tab == 0 && s.Built));
            Assert.Equal(2, t.Skills.Count(s => s.Tab == 0 && !s.Built));

            // Strafe: D2's counts and mana, 75% of the shot each, 16.5 m; no D2 damage rows.
            foreach (int level in new[] { 1, 10, 20 })
            {
                Assert.Equal(11f, v.Get("Strafe", "mana_cost", level));
                Assert.Equal(75f, v.Get("Strafe", "weapon_damage_pct", level));
                Assert.Equal(16.5f, v.Get("Strafe", "range_m", level));
            }
            Assert.Equal(5f, v.Get("Strafe", "max_targets", 1));
            Assert.Equal(10f, v.Get("Strafe", "max_targets", 20));
            Assert.Equal(2f, v.Get("Strafe", "min_arrows", 1));
            Assert.Equal(7f, v.Get("Strafe", "min_arrows", 20));
            Assert.Equal(30f, v.Get("Strafe", "attack_rating_pct", 1));
            Assert.False(v.Has("Strafe", "damage_pct"));

            // Immolation Arrow: blast 25 -> 100, burn 10 -> 30 a second, 3 m, burns 4 s.
            Assert.Equal(25f, v.Get("ImmolationArrow", "blast_damage_pct", 1));
            Assert.Equal(100f, v.Get("ImmolationArrow", "blast_damage_pct", 20));
            Assert.Equal(61f, v.Get("ImmolationArrow", "blast_damage_pct", 10));
            Assert.Equal(10f, v.Get("ImmolationArrow", "burn_damage_pct", 1));
            Assert.Equal(30f, v.Get("ImmolationArrow", "burn_damage_pct", 20));
            Assert.Equal(19f, v.Get("ImmolationArrow", "burn_damage_pct", 10));
            foreach (int level in new[] { 1, 10, 20 })
            {
                Assert.Equal(3f, v.Get("ImmolationArrow", "radius_m", level));
                Assert.Equal(4f, v.Get("ImmolationArrow", "burn_duration_s", level));
            }
            foreach (var gone in new[] { "explosion_fire_damage_min", "explosion_fire_damage_max", "burn_fire_dps_min", "burn_fire_dps_max" })
            {
                Assert.False(v.Has("ImmolationArrow", gone), gone);
            }

            // Venom Strike: cloud 25 -> 100 a second, 4 s, 3 m.
            Assert.Equal(25f, v.Get("VenomStrike", "cloud_damage_pct", 1));
            Assert.Equal(100f, v.Get("VenomStrike", "cloud_damage_pct", 20));
            Assert.Equal(61f, v.Get("VenomStrike", "cloud_damage_pct", 10));
            foreach (int level in new[] { 1, 10, 20 })
            {
                Assert.Equal(4f, v.Get("VenomStrike", "cloud_duration_s", level));
                Assert.Equal(3f, v.Get("VenomStrike", "radius_m", level));
            }

            // The share rows rise every level.
            foreach (var row in new[] { "ImmolationArrow|blast_damage_pct", "ImmolationArrow|burn_damage_pct", "VenomStrike|cloud_damage_pct" })
            {
                var parts = row.Split('|');
                for (int level = 2; level <= 20; level++)
                {
                    Assert.True(v.Get(parts[0], parts[1], level) > v.Get(parts[0], parts[1], level - 1), row + " level " + level);
                }
            }
        }

        [Fact]
        public void NewBowSkillsCostEitrFromD2sMana()
        {
            var v = Values();
            var t = Tree();
            Assert.Equal(11f, ActiveSkillRules.EitrCost(t.Find("Strafe"), v, 1));
            Assert.Equal(11f, ActiveSkillRules.EitrCost(t.Find("Strafe"), v, 20));
            Assert.Equal(6f, ActiveSkillRules.EitrCost(t.Find("ImmolationArrow"), v, 1));
            Assert.Equal(10.75f, ActiveSkillRules.EitrCost(t.Find("ImmolationArrow"), v, 20));
            Assert.Equal(7f, ActiveSkillRules.EitrCost(t.Find("VenomStrike"), v, 1));
            Assert.Equal(16.5f, ActiveSkillRules.EitrCost(t.Find("VenomStrike"), v, 20));
        }

        [Fact]
        public void AreaTicksCountOneHitEverySecond()
        {
            Assert.Equal(4, ActiveSkillRules.AreaTicks(4f));
            Assert.Equal(4, ActiveSkillRules.AreaTicks(4f, 1f));
            Assert.Equal(2, ActiveSkillRules.AreaTicks(4f, 2f));
            Assert.Equal(3, ActiveSkillRules.AreaTicks(3.5f, 1f));
            Assert.Equal(0, ActiveSkillRules.AreaTicks(0f));
            Assert.Equal(0, ActiveSkillRules.AreaTicks(-4f));
            Assert.Equal(0, ActiveSkillRules.AreaTicks(4f, 0f));
            Assert.Equal(0, ActiveSkillRules.AreaTicks(4f, -1f));
            Assert.Equal(1f, ActiveSkillRules.AreaInterval);
        }

        [Fact]
        public void StrafeSharesTheVolleyOutOverTheNearestEnemies()
        {
            // No enemies: the arrows fly straight (-1), at least one.
            Assert.Equal(new[] { -1, -1 }, ActiveSkillRules.StrafeTargets(new float[0], 5, 2));
            Assert.Equal(new[] { -1 }, ActiveSkillRules.StrafeTargets(new float[0], 5, 0));
            Assert.Equal(new[] { -1, -1 }, ActiveSkillRules.StrafeTargets(null, 5, 2));
            // 3 enemies, max 5, min 2: three arrows, one each, nearest the aim first.
            Assert.Equal(new[] { 1, 2, 0 }, ActiveSkillRules.StrafeTargets(new[] { 30f, 5f, 10f }, 5, 2));
            // One enemy, min 2: both arrows at it.
            Assert.Equal(new[] { 0, 0 }, ActiveSkillRules.StrafeTargets(new[] { 12f }, 5, 2));
            // 7 enemies, max 5, min 2: five arrows at the five smallest angles.
            Assert.Equal(new[] { 3, 6, 1, 5, 0 }, ActiveSkillRules.StrafeTargets(new[] { 40f, 20f, 70f, 1f, 80f, 30f, 10f }, 5, 2));
            // 2 enemies, min 7: seven arrows alternating.
            Assert.Equal(new[] { 0, 1, 0, 1, 0, 1, 0 }, ActiveSkillRules.StrafeTargets(new[] { 3f, 9f }, 5, 7));
            // Equal angles keep their order.
            Assert.Equal(new[] { 0, 1, 2 }, ActiveSkillRules.StrafeTargets(new[] { 7f, 7f, 7f }, 5, 1));
            Assert.Equal(new[] { 0, 1 }, ActiveSkillRules.StrafeTargets(new[] { 7f, 7f, 7f }, 2, 1));
            // Max targets below 1 counts as 1.
            Assert.Equal(new[] { 1, 1 }, ActiveSkillRules.StrafeTargets(new[] { 9f, 2f }, 0, 2));
        }

        [Fact]
        public void NewBowSkillTooltips()
        {
            var v = Values();
            Assert.Equal("Fires a volley for one arrow: up to 5 enemies in view within 16.5 m each get an arrow flying straight at them, " +
                         "at 75% damage; at least 2 arrows, shared out when there are fewer enemies", ActiveSkillRules.Describe("Strafe", v, 1, 10f, 25f));
            Assert.Contains("up to 10 enemies", ActiveSkillRules.Describe("Strafe", v, 20, 1f, 25f));
            Assert.Equal("Explodes on impact: fire damage equal to 25% of the shot's damage within 3 m; " +
                         "the ground there burns for 4 s, dealing 10% of the shot's damage each second to enemies in it",
                ActiveSkillRules.Describe("ImmolationArrow", v, 1, 1f, 25f));
            Assert.Contains("100%", ActiveSkillRules.Describe("ImmolationArrow", v, 20, 1f, 25f));
            Assert.Contains("30%", ActiveSkillRules.Describe("ImmolationArrow", v, 20, 1f, 25f));
            Assert.Contains("20% of the shot's damage each second", ActiveSkillRules.Describe("ImmolationArrow", v, 1, 2f, 25f));
            Assert.Equal("Bursts into a poison cloud reaching 3 m around where it lands for 4 s: " +
                         "everything in it is poisoned for 25% of the shot's damage, again each second it stays",
                ActiveSkillRules.Describe("VenomStrike", v, 1, 1f, 25f));
            Assert.Contains("100%", ActiveSkillRules.Describe("VenomStrike", v, 20, 1f, 25f));
        }

        [Fact]
        public void TooltipsUseValheimNumbers()
        {
            var v = Values();
            Assert.Equal("Explodes on impact: frost damage equal to 50% of the shot's damage within 3 m, freezing for 2 s (bosses are only slowed)",
                ActiveSkillRules.Describe("FreezingArrow", v, 1, 1f, 25f));
            Assert.Contains("150% of the shot's damage", ActiveSkillRules.Describe("FreezingArrow", v, 20, 1f, 25f));
            // The admin's multiplier shows in the share (and only there).
            Assert.Contains("100% of the shot's damage", ActiveSkillRules.Describe("FreezingArrow", v, 1, 2f, 25f));
            Assert.Equal("Fires 2 arrows in a fan for one arrow, each at 75% damage", ActiveSkillRules.Describe("MultipleShot", v, 1, 10f, 25f));
            Assert.Contains("3 m", ActiveSkillRules.Describe("FreezingArrow", v, 1, 1f, 25f));
            Assert.Contains("5% more damage for 8 s", ActiveSkillRules.Describe("InnerSight", v, 1, 1f, ActiveSkillRules.InnerSightPercent(1, 5f, 1f)));
            Assert.Null(ActiveSkillRules.Describe("MultipleShot", v, 0, 1f, 25f));
            Assert.Equal("Lightning strikes the enemy the arrow hits and forks to up to 3 other enemies within 4 m: each strike deals 25% of the shot's damage as lightning",
                ActiveSkillRules.Describe("ForkedLightning", v, 1, 1f, 25f));
            Assert.Contains("100% of the shot's damage", ActiveSkillRules.Describe("ForkedLightning", v, 20, 1f, 25f));
            Assert.Equal("Hold 1.5 s past full draw, then release: a gust of wind 1.5 m wide at 100% damage rushes 50 m at 2× an arrow's speed, " +
                         "along the ground and through every enemy, staggering them and knocking them back (force 100). Released early: a normal shot, no eitr",
                ActiveSkillRules.Describe("WindLance", v, 1, 10f, 25f));
            Assert.Contains("233% damage", ActiveSkillRules.Describe("WindLance", v, 20, 1f, 25f));
            Assert.Contains("up to 7 other enemies", ActiveSkillRules.Describe("ForkedLightning", v, 20, 1f, 25f));
            // A removed skill has no text.
            Assert.Null(ActiveSkillRules.Describe("FireArrow", v, 1, 1f, 25f));
        }

        [Fact]
        public void QuickCastOnlyForSkillsWithoutAWeapon()
        {
            // User 2026-11-13: per skill, on by default; a weapon skill goes on right click.
            var t = Tree();
            var inner = t.Find("InnerSight");
            var fire = t.Find("MultipleShot");
            Assert.True(ActiveSkillRules.CanQuickCast(inner));
            Assert.False(ActiveSkillRules.CanQuickCast(fire));
            Assert.False(ActiveSkillRules.CanQuickCast(t.Find("Dodge")));
            Assert.False(ActiveSkillRules.CanQuickCast(null));
            var off = ActiveSkillRules.ParseIds("");
            Assert.True(ActiveSkillRules.IsQuickCast(inner, off));
            Assert.False(ActiveSkillRules.OnSkillButton(inner, off));
            off = ActiveSkillRules.ParseIds("InnerSight;;innersight");
            Assert.Single(off);
            Assert.False(ActiveSkillRules.IsQuickCast(inner, off));
            Assert.True(ActiveSkillRules.OnSkillButton(inner, off));
            Assert.True(ActiveSkillRules.OnSkillButton(fire, off));
            Assert.False(ActiveSkillRules.OnSkillButton(t.Find("Dodge"), off));
            Assert.Equal("A;B", ActiveSkillRules.FormatIds(new[] { "B", "A" }));
        }

        [Fact]
        public void BrokenTablesAreReported()
        {
            string header = "id,value," + string.Join(",", Enumerable.Range(1, 20).Select(i => "l" + i)) + "\n";
            string row = string.Join(",", Enumerable.Range(1, 20));
            var errors = new List<string>();
            SkillValueTable.Parse(header + "A,x," + row + "\nA,x," + row + "\n", "v.csv", errors);
            Assert.Contains(errors, e => e.Contains("listed twice"));
        }
    }
}
