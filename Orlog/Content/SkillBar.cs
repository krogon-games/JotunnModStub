using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Orlog.Rules;
using UnityEngine;

namespace Orlog.Content
{
    /// <summary>
    ///     The local player's four skill slots and the skill on the skill button (roadmap C5, <c>class-design.md</c> §4).
    ///     Slots are saved in the character's custom data (<c>orlog_slots</c>, <c>id0;id1;id2;id3</c>); the slot on the skill
    ///     button is remembered per weapon kind (<c>orlog_selected</c>, <c>bow:0;spear:2</c>; user 2026-11-11). Each slot's
    ///     hotkey puts an attack skill on the skill button or casts a cast skill. The numbers come from
    ///     <c>Orlog/Data/&lt;class&gt;_skill_values.csv</c> and <c>skill_damage_scale.csv</c>.
    /// </summary>
    internal static class SkillBar
    {
        public const string SlotsKey = "orlog_slots";
        public const string SelectedKey = "orlog_selected";
        public const string QuickCastOffKey = "orlog_quickcast_off";

        private static readonly Dictionary<string, SkillValueTable> Values = new Dictionary<string, SkillValueTable>(System.StringComparer.OrdinalIgnoreCase);
        private static DamageScale s_scale;

        // ------------------------------------------------------------ data

        public static SkillValueTable ValuesFor(ClassTable.Class cls)
        {
            if (cls == null)
            {
                return null;
            }
            if (!Values.TryGetValue(cls.Name, out var table))
            {
                string file = cls.Name.ToLowerInvariant() + "_skill_values.csv";
                string csv = Progress.ReadData(file);
                var errors = new List<string>();
                table = csv.Length > 0 ? SkillValueTable.Parse(csv, file, errors) : null;
                Report(errors);
                Values[cls.Name] = table;
            }
            return table;
        }

        public static DamageScale Scale()
        {
            if (s_scale == null)
            {
                var errors = new List<string>();
                s_scale = DamageScale.Parse(Progress.ReadData("skill_damage_scale.csv"), "skill_damage_scale.csv", errors);
                Report(errors);
            }
            return s_scale;
        }

        private static void Report(List<string> errors)
        {
            foreach (var error in errors)
            {
                Jotunn.Logger.LogError("Orlog data: " + error);
            }
        }

        /// <summary>D2 damage points → Valheim points for the player: the scale at their level × the admin's multiplier.</summary>
        public static float DamageScaleFor(Player player)
        {
            return Scale().At(Progress.GetLevel(player)) * OrlogConfig.SkillDamageMultiplier.Value;
        }

        /// <summary>The player's level in a skill (the hard points: no +skills yet).</summary>
        public static int LevelOf(Player player, string skillId)
        {
            return SkillRules.PointsIn(SkillPoints.Points(player), skillId);
        }

        public static float CostOf(Player player, SkillTable.Skill skill)
        {
            return ActiveSkillRules.EitrCost(skill, ValuesFor(Attributes.ClassOf(player)), LevelOf(player, skill?.Id));
        }

        /// <summary>The tooltip line of an active skill at a level, or null.</summary>
        public static string Describe(Player player, string skillId, int level)
        {
            return ActiveSkillRules.Describe(skillId, ValuesFor(Attributes.ClassOf(player)), level, DamageScaleFor(player),
                OrlogConfig.InnerSightDamageTaken.Value);
        }

        /// <summary><c>bow</c> for a bow or crossbow in hand, <c>spear</c> for a spear, else "".</summary>
        public static string WeaponKind(Player player)
        {
            var type = player ? player.GetCurrentWeapon()?.m_shared?.m_skillType ?? Skills.SkillType.None : Skills.SkillType.None;
            return type == Skills.SkillType.Bows || type == Skills.SkillType.Crossbows ? "bow" : type == Skills.SkillType.Spears ? "spear" : "";
        }

        // ------------------------------------------------------------ slots

        public static string[] Slots(Player player)
        {
            return ActiveSkillRules.ParseSlots(player && player.m_customData.TryGetValue(SlotsKey, out string saved) ? saved : "");
        }

        /// <summary>The skills this character turned quick cast off for (user, 2026-11-13: on by default).</summary>
        public static HashSet<string> QuickCastOff(Player player)
        {
            return ActiveSkillRules.ParseIds(player && player.m_customData.TryGetValue(QuickCastOffKey, out string saved) ? saved : "");
        }

        /// <summary>Turns quick cast on or off for a slot's skill (right click on the skill cross); "" on success, otherwise why not.</summary>
        public static string ToggleQuickCast(Player player, int slot)
        {
            if (slot < 0 || slot >= ActiveSkillRules.Slots)
            {
                return "No such slot";
            }
            var skill = SkillPoints.TableOf(player)?.Find(Slots(player)[slot]);
            if (skill == null)
            {
                return "Empty slot";
            }
            if (!ActiveSkillRules.CanQuickCast(skill))
            {
                return skill.Needs.Length > 0 ? "Needs a " + skill.Needs + ": it fires from the skill button" : "Fires from the skill button";
            }
            var off = QuickCastOff(player);
            bool nowOn = off.Contains(skill.Id);
            if (nowOn)
            {
                off.Remove(skill.Id);
            }
            else
            {
                off.Add(skill.Id);
            }
            player.m_customData[QuickCastOffKey] = ActiveSkillRules.FormatIds(off);
            // A skill whose quick cast comes back on leaves the skill button.
            if (nowOn)
            {
                var selected = Selected(player);
                foreach (var kind in new List<string>(selected.Keys))
                {
                    if (selected[kind] == slot)
                    {
                        selected[kind] = -1;
                    }
                }
                player.m_customData[SelectedKey] = ActiveSkillRules.FormatSelected(selected);
            }
            Progress.NotifyChanged();
            player.Message(MessageHud.MessageType.TopLeft, skill.Name + (nowOn ? ": quick cast on" : ": quick cast off, it goes on the skill button"));
            return "";
        }

        private static Dictionary<string, int> Selected(Player player)
        {
            return ActiveSkillRules.ParseSelected(player && player.m_customData.TryGetValue(SelectedKey, out string saved) ? saved : "");
        }

        /// <summary>The slot on the skill button for the weapon in hand (-1 for none).</summary>
        public static int SelectedSlot(Player player)
        {
            var selected = Selected(player);
            if (!selected.TryGetValue(WeaponKind(player), out int slot) || slot < 0 || slot >= ActiveSkillRules.Slots)
            {
                return -1;
            }
            var skill = SkillPoints.TableOf(player)?.Find(Slots(player)[slot]);
            return ActiveSkillRules.OnSkillButton(skill, QuickCastOff(player)) ? slot : -1;
        }

        /// <summary>The attack skill on the skill button for the weapon in hand, or null.</summary>
        public static SkillTable.Skill SelectedSkill(Player player)
        {
            int slot = SelectedSlot(player);
            return slot < 0 ? null : SkillPoints.TableOf(player)?.Find(Slots(player)[slot]);
        }

        /// <summary>Puts a skill in a slot ("" clears it); "" on success, otherwise why not.</summary>
        public static string SetSlot(Player player, int slot, string skillId)
        {
            if (slot < 0 || slot >= ActiveSkillRules.Slots)
            {
                return "No such slot";
            }
            var table = SkillPoints.TableOf(player);
            if (table == null)
            {
                return "Choose a class at the Orlog stone first";
            }
            if (!string.IsNullOrEmpty(skillId))
            {
                var skill = table.Find(skillId);
                string refused = ActiveSkillRules.SlotRefusal(skill, LevelOf(player, skill?.Id));
                if (refused.Length > 0)
                {
                    return refused;
                }
                // Only skills that already act can go in a slot (the spear skills come with roadmap C6).
                if (Describe(player, skill.Id, 1) == null)
                {
                    return "Doesn't work yet";
                }
                skillId = skill.Id;
            }
            player.m_customData[SlotsKey] = ActiveSkillRules.FormatSlots(ActiveSkillRules.Place(Slots(player), slot, skillId));
            Progress.NotifyChanged();
            return "";
        }

        public static SlotState StateOf(Player player, int slot)
        {
            var skill = SkillPoints.TableOf(player)?.Find(Slots(player)[slot]);
            return ActiveSkillRules.State(skill, WeaponKind(player), player.GetEitr(), CostOf(player, skill));
        }

        /// <summary>A slot's hotkey: an attack skill goes on the skill button for its weapon kind, a cast skill casts.</summary>
        public static void Use(Player player, int slot)
        {
            var skill = SkillPoints.TableOf(player)?.Find(Slots(player)[slot]);
            if (skill == null || LevelOf(player, skill.Id) < 1)
            {
                return;
            }
            if (ActiveSkillRules.IsQuickCast(skill, QuickCastOff(player)))
            {
                var state = StateOf(player, slot);
                if (!state.Usable)
                {
                    player.Message(MessageHud.MessageType.Center, ActiveSkillRules.BlockedMessage(state));
                    return;
                }
                CastSkills.Cast(player, skill, LevelOf(player, skill.Id), state.EitrCost);
                return;
            }
            // On the skill button: an attack skill for its weapon; a cast skill (quick cast off) for the weapon in hand.
            var selected = Selected(player);
            selected[skill.Needs.Length > 0 ? skill.Needs : WeaponKind(player)] = slot;
            player.m_customData[SelectedKey] = ActiveSkillRules.FormatSelected(selected);
            Progress.NotifyChanged();
        }

        /// <summary>Whether the local player's keys go to the game (no chat, console, menu, map or inventory open).</summary>
        public static bool TakesInput(Player player)
        {
            var controller = player ? player.GetComponent<PlayerController>() : null;
            return controller && controller.TakeInput() && !controller.InInventoryEtc() && !Hud.InRadial() && !ZInput.IsGamepadActive();
        }

        /// <summary>Called every frame: the slot hotkeys.</summary>
        public static void Tick()
        {
            var player = Player.m_localPlayer;
            if (!player || !Progress.Ready || Attributes.ClassOf(player) == null || !TakesInput(player))
            {
                return;
            }
            for (int i = 0; i < ActiveSkillRules.Slots; i++)
            {
                if (ZInput.GetKeyDown(OrlogConfig.SlotKeys[i].Value, false))
                {
                    Safe.Run("use a skill slot", () => Use(player, i));
                }
            }
        }

        // ------------------------------------------------------------ the UI's answers (classes-ui-brief.md §3)

        public static string BarJson(Player player)
        {
            var slots = Slots(player);
            var json = new StringBuilder("{");
            for (int i = 0; i < ActiveSkillRules.Slots; i++)
            {
                json.Append("\"slot").Append(i).Append("\":\"").Append(slots[i]).Append("\",");
                json.Append("\"key").Append(i).Append("\":\"").Append(KeyLabel(OrlogConfig.SlotKeys[i].Value)).Append("\",");
            }
            var table = SkillPoints.TableOf(player);
            var off = QuickCastOff(player);
            for (int i = 0; i < ActiveSkillRules.Slots; i++)
            {
                var skill = table?.Find(slots[i]);
                json.Append("\"quick").Append(i).Append("\":").Append(ActiveSkillRules.IsQuickCast(skill, off) ? "true" : "false").Append(',');
                json.Append("\"quickable").Append(i).Append("\":").Append(ActiveSkillRules.CanQuickCast(skill) ? "true" : "false").Append(',');
            }
            json.Append("\"skillButton\":\"").Append(KeyLabel(OrlogConfig.SkillButton.Value)).Append("\",");
            json.Append("\"selected\":").Append(SelectedSlot(player).ToString(CultureInfo.InvariantCulture));
            return json.Append('}').ToString();
        }

        public static string SlotStateJson(Player player, int slot)
        {
            if (slot < 0 || slot >= ActiveSkillRules.Slots || Slots(player)[slot].Length == 0)
            {
                return "";
            }
            var state = StateOf(player, slot);
            var json = new StringBuilder("{");
            json.Append("\"usable\":").Append(state.Usable ? "true" : "false").Append(',');
            json.Append("\"blocked\":\"").Append(state.Blocked).Append("\",");
            json.Append("\"needs\":\"").Append(state.Needs).Append("\",");
            // D2's bow skills and Inner Sight have no cooldown.
            json.Append("\"cooldown\":0,\"cooldownTotal\":0,");
            json.Append("\"eitrCost\":").Append(state.EitrCost.ToString("0.###", CultureInfo.InvariantCulture));
            return json.Append('}').ToString();
        }

        private static string KeyLabel(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Mouse0:
                    return "Left click";
                case KeyCode.Mouse1:
                    return "Right click";
                case KeyCode.Mouse2:
                    return "Middle click";
                case KeyCode.Mouse3:
                    return "Mouse 5";
                case KeyCode.Mouse4:
                    return "Mouse 4";
                case KeyCode.None:
                    return "none";
                default:
                    return key.ToString();
            }
        }
    }
}
