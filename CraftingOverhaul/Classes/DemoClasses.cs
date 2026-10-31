using System;
using System.Collections.Generic;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     Sample data for building and play-testing the classes UI before the Orlog plugin has classes
    ///     (<c>docs/classes-ui-brief.md</c> §4): an Amazon at level 12 with points to spend. It answers every call of the
    ///     classes API (version 1) with the same JSON the Orlog plugin will give, and its actions change the sample data
    ///     locally, so the skills panel, skill cross and attribute buttons can be tried for real. Development aid, used
    ///     only with the <c>Demo</c> setting on and Orlog's classes missing (see CURRENT_STATE → Before release).
    ///
    ///     The 30 skills (tab, tree position, level, prerequisites, D2 mana cost as the eitr cost, one-line text) are
    ///     the Diablo 2 Amazon's from <c>data/d2-reference/amazon.csv</c>; their statuses are from
    ///     <c>docs/class-design.md</c> §5. Copied in here so the CSV never ships. Pure: no Unity, unit-tested.
    /// </summary>
    internal sealed class DemoClasses
    {
        public const int ApiVersion = 1;
        public const string AmazonId = "Amazon";
        public const int Slots = 4;
        public const int MaxSkillPoints = 20;

        public sealed class Skill
        {
            public string Id;
            public string Name;
            public int Tab;
            public int Row;
            public int Col;
            public int ReqLevel;
            /// <summary>Prerequisite skill ids joined by <c>+</c>; "" for none.</summary>
            public string Prereqs;
            /// <summary><c>attack</c>, <c>cast</c> or <c>passive</c> (brief §3).</summary>
            public string Kind;
            /// <summary><c>v1</c>, <c>reworked</c>, <c>later</c> or <c>dropped</c> (class-design §5).</summary>
            public string Status;
            public float EitrCost;
            public string Description;
            public int Points;

            public bool Built => Status == "v1" || Status == "reworked";
        }

        private static readonly string[] Tabs = { "Bow and Crossbow", "Passive and Magic", "Javelin and Spear" };
        private static readonly string[] AttributeIds = { "strength", "dexterity", "vitality", "energy" };

        private readonly List<Skill> _skills = new List<Skill>();
        private readonly Dictionary<string, Skill> _byId = new Dictionary<string, Skill>(StringComparer.Ordinal);
        private readonly int[] _attributes = { 20, 40, 25, 15 };
        private readonly string[] _slots = { "MultipleShot", "InnerSight", "", "FireArrow" };

        public string ClassId { get; private set; } = AmazonId;
        public int Level = 12;
        public int MaxLevel = 99;
        public long Xp = 4410;
        public long XpLevelStart = 3720;
        public long XpNextLevel = 4925;
        public int StatPoints = 5;
        public int SkillPoints = 1;
        public int Selected;
        public int ChangeCounter { get; private set; }
        public int StoneRequest { get; private set; }

        public DemoClasses()
        {
            // tab, row, col, name, level, prerequisites (names), kind, status, eitr, text, points
            Add(0, 1, 2, "Magic Arrow", 1, "", "attack", "v1", 1.5f, "Creates a magic arrow from mana; no ammo used", 3);
            Add(0, 1, 3, "Fire Arrow", 1, "", "attack", "v1", 3f, "Adds fire damage to an arrow", 1);
            Add(0, 2, 1, "Cold Arrow", 6, "", "attack", "v1", 3.5f, "Adds cold damage that slows the target", 1);
            Add(0, 2, 2, "Multiple Shot", 6, "Magic Arrow", "attack", "v1", 4f, "Splits one arrow into several", 3);
            Add(0, 3, 3, "Exploding Arrow", 12, "Fire Arrow+Multiple Shot", "attack", "v1", 5f, "Arrow explodes for area fire damage", 0);
            Add(0, 4, 1, "Ice Arrow", 18, "Cold Arrow", "attack", "v1", 4f, "Cold arrow that freezes the target", 0);
            Add(0, 4, 2, "Guided Arrow", 18, "Cold Arrow+Multiple Shot", "attack", "later", 8f, "Arrow homes in on target and always hits", 0);
            Add(0, 5, 2, "Strafe", 24, "Guided Arrow", "attack", "later", 11f, "Volley of arrows at multiple nearby targets", 0);
            Add(0, 5, 3, "Immolation Arrow", 24, "Exploding Arrow", "attack", "later", 6f, "Exploding arrow leaves a burning patch on ground", 0);
            Add(0, 6, 1, "Freezing Arrow", 30, "Ice Arrow", "attack", "v1", 9f, "Arrow freezes all enemies in an area", 0);
            Add(1, 1, 1, "Inner Sight", 1, "", "cast", "v1", 5f, "Lights up nearby enemies and lowers their defense", 1);
            Add(1, 1, 3, "Critical Strike", 1, "", "passive", "v1", 0f, "Chance to deal double physical damage", 1);
            Add(1, 2, 2, "Dodge", 6, "", "passive", "v1", 0f, "Chance to dodge melee attacks while standing/attacking", 0);
            Add(1, 3, 1, "Slow Missiles", 12, "Inner Sight", "cast", "later", 5f, "Slows enemy projectiles in an area", 0);
            Add(1, 3, 2, "Avoid", 12, "Dodge", "passive", "v1", 0f, "Chance to dodge missiles while standing/attacking", 0);
            Add(1, 4, 3, "Penetrate", 18, "Critical Strike", "passive", "reworked", 0f, "Chance for a hit to ignore the creature's resistance", 0);
            Add(1, 5, 1, "Decoy", 24, "Slow Missiles", "cast", "later", 19f, "Creates a decoy copy that distracts enemies", 0);
            Add(1, 5, 2, "Evade", 24, "Avoid", "passive", "v1", 0f, "Chance to dodge melee/missiles while moving", 0);
            Add(1, 6, 1, "Valkyrie", 30, "Decoy+Evade", "cast", "later", 25f, "Summons a Valkyrie warrior to fight for you", 0);
            Add(1, 6, 3, "Pierce", 30, "Penetrate", "passive", "later", 0f, "Chance for missiles to pass through enemies", 0);
            Add(2, 1, 1, "Jab", 1, "", "attack", "v1", 2f, "Rapid series of spear/javelin thrusts", 0);
            Add(2, 2, 2, "Power Strike", 6, "Jab", "attack", "v1", 2f, "Adds lightning damage to a melee thrust", 0);
            Add(2, 2, 3, "Poison Javelin", 6, "", "attack", "v1", 4f, "Thrown javelin leaves a trail of poison clouds", 0);
            Add(2, 3, 1, "Impale", 12, "Jab", "attack", "v1", 3f, "Powerful thrust; uses extra weapon durability", 0);
            Add(2, 3, 3, "Lightning Bolt", 12, "Poison Javelin", "attack", "v1", 6f, "Javelin turns into a lightning bolt", 0);
            Add(2, 4, 2, "Charged Strike", 18, "Power Strike+Lightning Bolt", "attack", "later", 4f, "Melee hit releases charged bolts", 0);
            Add(2, 4, 3, "Plague Javelin", 18, "Lightning Bolt", "attack", "v1", 7f, "Javelin bursts into an expanding poison cloud", 0);
            Add(2, 5, 1, "Fend", 24, "Impale", "attack", "later", 5f, "Rapidly attacks several nearby targets", 0);
            Add(2, 6, 2, "Lightning Strike", 30, "Charged Strike", "attack", "later", 9f, "Melee hit releases chain lightning", 0);
            Add(2, 6, 3, "Lightning Fury", 30, "Plague Javelin", "attack", "later", 10f, "Javelin bursts into many lightning bolts on impact", 0);
        }

        private void Add(int tab, int row, int col, string name, int level, string prereqs, string kind, string status, float eitr,
            string text, int points)
        {
            var parts = prereqs.Length == 0 ? new string[0] : prereqs.Split('+');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = IdOf(parts[i]);
            }
            var skill = new Skill
            {
                Id = IdOf(name), Name = name, Tab = tab, Row = row, Col = col, ReqLevel = level, Prereqs = string.Join("+", parts),
                Kind = kind, Status = status, EitrCost = eitr, Description = text, Points = points
            };
            _skills.Add(skill);
            _byId[skill.Id] = skill;
        }

        /// <summary>A skill's id: its name without spaces ("Multiple Shot" → "MultipleShot"), as in the brief's examples.</summary>
        public static string IdOf(string name)
        {
            return name.Replace(" ", "");
        }

        public IList<Skill> Skills => _skills;

        public Skill Find(string id)
        {
            return id != null && _byId.TryGetValue(id, out var skill) ? skill : null;
        }

        private void Changed()
        {
            ChangeCounter++;
        }

        // ------------------------------------------------------------ reads (brief §3)

        public string GetCharacterJson()
        {
            return new JsonOut()
                .Add("class", ClassId).Add("level", Level).Add("maxLevel", MaxLevel).Add("xp", Xp)
                .Add("xpLevelStart", XpLevelStart).Add("xpNextLevel", XpNextLevel)
                .Add("statPoints", StatPoints).Add("skillPoints", SkillPoints)
                .Add("strength", _attributes[0]).Add("dexterity", _attributes[1])
                .Add("vitality", _attributes[2]).Add("energy", _attributes[3]).ToString();
        }

        public string GetAttributeInfoJson(string attribute)
        {
            int index = Array.IndexOf(AttributeIds, attribute);
            if (index < 0)
            {
                return "";
            }
            int value = _attributes[index];
            string name = char.ToUpperInvariant(attribute[0]) + attribute.Substring(1);
            return new JsonOut().Add("name", name).Add("value", value).Add("effect", Effect(index, value)).ToString();
        }

        /// <summary>
        ///     Demo text only (Orlog will own the real numbers). D2's Amazon: +3 life per Vitality and +1.5 mana per
        ///     Energy (<c>classes.csv</c>); +1% damage per point of Strength (melee) and Dexterity (bows).
        /// </summary>
        private static string Effect(int index, int value)
        {
            switch (index)
            {
                case 0: return "+" + value + "% melee damage";
                case 1: return "+" + value + "% bow damage";
                case 2: return "+" + value * 3 + " health";
                default: return "+" + (value * 1.5f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " eitr";
            }
        }

        public string GetClassIdsJson()
        {
            return JsonOut.Array(new[] { AmazonId });
        }

        public string GetClassInfoJson(string classId)
        {
            if (classId != AmazonId)
            {
                return "";
            }
            return new JsonOut().Add("id", AmazonId).Add("name", "Amazon")
                .Add("description", "A huntress of bow and javelin. Fast, precise and hard to hit.")
                .Add("tab0", Tabs[0]).Add("tab1", Tabs[1]).Add("tab2", Tabs[2]).ToString();
        }

        public string GetSkillIdsJson(string classId)
        {
            if (classId != AmazonId)
            {
                return "[]";
            }
            var ids = new List<string>();
            foreach (var skill in _skills)
            {
                ids.Add(skill.Id);
            }
            return JsonOut.Array(ids);
        }

        public string GetSkillInfoJson(string skillId)
        {
            var skill = Find(skillId);
            if (skill == null)
            {
                return "";
            }
            string blocked = SpendBlocked(skill);
            return new JsonOut()
                .Add("id", skill.Id).Add("name", skill.Name).Add("tab", skill.Tab).Add("row", skill.Row).Add("col", skill.Col)
                .Add("reqLevel", skill.ReqLevel).Add("prereqs", skill.Prereqs).Add("kind", skill.Kind).Add("status", skill.Status)
                .Add("maxPoints", MaxSkillPoints).Add("points", skill.Points).Add("level", skill.Points)
                .Add("canSpend", blocked.Length == 0).Add("spendBlocked", blocked)
                .Add("description", skill.Points > 0 ? skill.Description : "Not learned yet. " + skill.Description)
                .Add("nextDescription", skill.Points < MaxSkillPoints ? "Level " + (skill.Points + 1) + ": " + skill.Description : "")
                .Add("eitrCost", skill.EitrCost).ToString();
        }

        /// <summary>
        ///     Why a point can't go into a skill, "" if it can. Class-design §4: a prerequisite only counts when it's
        ///     built (v1 or reworked); then it needs at least one point.
        /// </summary>
        public string SpendBlocked(Skill skill)
        {
            if (!skill.Built)
            {
                return "Coming later";
            }
            if (skill.Points >= MaxSkillPoints)
            {
                return "Maxed";
            }
            if (Level < skill.ReqLevel)
            {
                return "Needs level " + skill.ReqLevel;
            }
            if (skill.Prereqs.Length > 0)
            {
                foreach (string id in skill.Prereqs.Split('+'))
                {
                    var prereq = Find(id);
                    if (prereq != null && prereq.Built && prereq.Points == 0)
                    {
                        return "Needs " + prereq.Name;
                    }
                }
            }
            return SkillPoints <= 0 ? "No skill points" : "";
        }

        public string GetSkillBarJson()
        {
            if (ClassId.Length == 0)
            {
                return "";
            }
            return new JsonOut()
                .Add("slot0", _slots[0]).Add("slot1", _slots[1]).Add("slot2", _slots[2]).Add("slot3", _slots[3])
                .Add("key0", "1").Add("key1", "2").Add("key2", "3").Add("key3", "4")
                .Add("skillButton", "Right mouse").Add("selected", Selected).ToString();
        }

        /// <summary>The weapon a skill needs, for the slot state's <c>needs</c>: bow skills a bow, javelin and spear skills a spear.</summary>
        public static string NeededWeapon(Skill skill)
        {
            if (skill == null)
            {
                return "";
            }
            return skill.Tab == 0 ? "bow" : skill.Tab == 2 ? "spear" : "";
        }

        /// <summary>
        ///     A slot's state as the player would really see it (user, 2026-10-31: fixed "Weapon" and "Eitr" states were
        ///     confusing): blocked when the held weapon (<paramref name="held" />: "bow", "spear" or anything else) isn't
        ///     the one the skill needs, or when <paramref name="eitr" /> is below its cost. Slot 1 also runs a 3 s cooldown
        ///     every 8 s so the cooldown look can be seen. <paramref name="time" /> in seconds.
        /// </summary>
        public string GetSlotStateJson(int slot, float time, string held, float eitr)
        {
            if (slot < 0 || slot >= Slots || _slots[slot].Length == 0)
            {
                return "";
            }
            var skill = Find(_slots[slot]);
            float cooldown = 0f;
            string blocked = "";
            string needs = NeededWeapon(skill);
            float cost = skill != null ? skill.EitrCost : 0f;
            if (slot == 1)
            {
                float phase = time % 8f;
                cooldown = phase < 3f ? 3f - phase : 0f;
            }
            if (cooldown > 0f)
            {
                blocked = "cooldown";
            }
            else if (needs.Length > 0 && held != needs)
            {
                blocked = "weapon";
            }
            else if (eitr < cost)
            {
                blocked = "eitr";
            }
            return new JsonOut().Add("usable", blocked.Length == 0).Add("blocked", blocked).Add("needs", needs)
                .Add("cooldown", cooldown).Add("cooldownTotal", 3f).Add("eitrCost", cost).ToString();
        }

        // ------------------------------------------------------------ actions ("" = done, else the reason)

        public string SpendStatPoint(string attribute)
        {
            int index = Array.IndexOf(AttributeIds, attribute);
            if (index < 0)
            {
                return "Unknown attribute";
            }
            if (StatPoints <= 0)
            {
                return "No stat points";
            }
            StatPoints--;
            _attributes[index]++;
            Changed();
            return "";
        }

        public string SpendSkillPoint(string skillId)
        {
            var skill = Find(skillId);
            if (skill == null)
            {
                return "Unknown skill";
            }
            string blocked = SpendBlocked(skill);
            if (blocked.Length > 0)
            {
                return blocked;
            }
            SkillPoints--;
            skill.Points++;
            Changed();
            return "";
        }

        public string SetSkillSlot(int slot, string skillId)
        {
            if (slot < 0 || slot >= Slots)
            {
                return "No such slot";
            }
            if (string.IsNullOrEmpty(skillId))
            {
                _slots[slot] = "";
                Changed();
                return "";
            }
            var skill = Find(skillId);
            if (skill == null)
            {
                return "Unknown skill";
            }
            if (skill.Kind == "passive")
            {
                return "Passive skills don't take a slot";
            }
            if (!skill.Built)
            {
                return "Coming later";
            }
            if (skill.Points == 0)
            {
                return "Learn it first";
            }
            for (int i = 0; i < Slots; i++)
            {
                if (_slots[i] == skill.Id)
                {
                    _slots[i] = "";
                }
            }
            _slots[slot] = skill.Id;
            Changed();
            return "";
        }

        public string ChooseClass(string classId)
        {
            if (ClassId.Length > 0)
            {
                return "You already have a class";
            }
            if (classId != AmazonId)
            {
                return "Unknown class";
            }
            ClassId = classId;
            Changed();
            return "";
        }

        /// <summary>Demo of using the Orlog stone: the character loses its class and the class choice window opens.</summary>
        public void UseStone()
        {
            ClassId = "";
            StoneRequest++;
            Changed();
        }
    }
}
