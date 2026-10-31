using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The skills panel (brief §2.4, §2.5), redesigned 2026-10-31 after the user found the first versions "not
    ///     acceptable": Diablo 2's layout on the game's own panel. Left: the character box (level, XP, stat points and the
    ///     four attributes with + buttons) and the skill details pane (the hovered or selected skill: name, rank, kind,
    ///     cost, what it does now and next, why it's locked, a Learn button). Right: the class's three tabs, the tree on a
    ///     bronze-rimmed tablet (medallions with a points badge, no names; tier rows; short straight paths, glowing
    ///     eitr-blue once learned), and the four skill slots. Click a skill to select it, double-click or Learn to spend a
    ///     point, drag it onto a slot here or on the HUD's skill cross. Every position comes from
    ///     <see cref="SkillsPanelLayout" />, which <c>tools/preview_skills_panel.py</c> also draws from.
    ///     Opened with <see cref="ModConfig.SkillsPanelKey" /> or the Skills button on the inventory; everything shown and
    ///     every action goes through <see cref="ClassesSource" />.
    /// </summary>
    internal static class SkillsPanel
    {
        private static readonly string[] Attributes = { "strength", "dexterity", "vitality", "energy" };

        // Path colours (mirrored in tools/preview_skills_panel.py).
        private static readonly Color Groove = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color PathDim = new Color(0.47f, 0.36f, 0.24f, 0.9f);
        private static readonly Color PathLit = new Color(0.85f, 0.95f, 1f);
        private static readonly Color Divider = new Color(0.66f, 0.47f, 0.24f, 0.24f);
        private static readonly Color SelectedRing = new Color(1f, 0.94f, 0.78f, 1f);

        private static RectTransform _root;
        private static RectTransform _linesRoot;
        private static RectTransform _tilesRoot;
        private static Text _title;
        private static Text _skillPoints;
        private static Text _message;
        private static Text _level;
        private static Text _xpText;
        private static Image _xpFill;
        private static Text _statPoints;
        private static readonly Text[] TabLabels = new Text[3];
        private static readonly Image[] TabImages = new Image[3];
        private static readonly Text[] AttributeNames = new Text[4];
        private static readonly Text[] AttributeValues = new Text[4];
        private static readonly Text[] AttributeEffects = new Text[4];
        private static readonly GameObject[] AttributeButtons = new GameObject[4];
        private static readonly SlotTileView[] Slots = new SlotTileView[DemoClasses.Slots];
        private static Image _dragGhost;
        private static GameObject _inventoryButton;

        // Details pane.
        private static Image _detailsGlow;
        private static Image _detailsMedallion;
        private static Image _detailsIcon;
        private static Text _detailsName;
        private static Text _detailsRank;
        private static Text _detailsKind;
        private static Text _detailsText;
        private static Button _learn;
        private static Text _learnLabel;

        private static int _tab;
        private static int _lastCounter = -1;
        private static string _classId = "";
        private static string[] _skillIds = new string[0];
        private static string _selected = "";
        private static string _hovered = "";
        private static readonly List<SkillView> Skills = new List<SkillView>();
        private static readonly Dictionary<string, SkillView> ById = new Dictionary<string, SkillView>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Image> SelectionRings = new Dictionary<string, Image>(StringComparer.Ordinal);
        private static readonly List<GameObject> TreeObjects = new List<GameObject>();
        private static float _messageUntil;

        public static bool IsOpen => _root && _root.gameObject.activeSelf;

        // ------------------------------------------------------------ open and close

        public static void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public static void Open()
        {
            if (!ClassesSource.Available || !Player.m_localPlayer || !GUIManager.CustomGUIFront)
            {
                return;
            }
            string character = ClassesSource.Character();
            if (string.IsNullOrEmpty(FlatJson.Get(character, "class")))
            {
                // Nothing to show before the player has chosen a class at the Orlog stone.
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Choose a class at the Orlog stone first");
                return;
            }
            EnsureCreated();
            _root.gameObject.SetActive(true);
            _root.SetAsLastSibling();
            _lastCounter = -1;
            _hovered = "";
            ShowMessage("");
            ClassWindows.SetOpen(ClassWindows.Skills, true);
        }

        public static void Close()
        {
            EndDrag();
            if (_root)
            {
                _root.gameObject.SetActive(false);
            }
            ClassWindows.SetOpen(ClassWindows.Skills, false);
        }

        // ------------------------------------------------------------ building

        private static void EnsureCreated()
        {
            if (_root)
            {
                return;
            }
            float width = SkillsPanelLayout.Width, height = SkillsPanelLayout.Height;
            _root = UiKit.Panel("CraftingOverhaul_SkillsPanel", GUIManager.CustomGUIFront.transform, new Vector2(width, height));
            UiKit.Place(_root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));

            _title = UiKit.Text("Title", _root, 32, new Color(0.94f, 0.78f, 0.43f), TextAnchor.MiddleCenter, true, GUIManager.Instance.NorseBold);
            Place(_title.rectTransform, SkillsPanelLayout.Title);
            BuildClose();
            _skillPoints = UiKit.Text("SkillPoints", _root, 18, UiKit.Gold, TextAnchor.MiddleRight);
            Place(_skillPoints.rectTransform, SkillsPanelLayout.SkillPoints);

            BuildCharacter();
            BuildDetails();
            BuildTabs();
            BuildTree();
            BuildSlots();

            _message = UiKit.Text("Message", _root, 15, new Color(1f, 0.6f, 0.5f), TextAnchor.MiddleCenter);
            Place(_message.rectTransform, SkillsPanelLayout.Message);

            _dragGhost = UiKit.Image("DragGhost", _root, null);
            _dragGhost.rectTransform.sizeDelta = new Vector2(SkillsPanelLayout.SlotSize, SkillsPanelLayout.SlotSize);
            _dragGhost.preserveAspect = true;
            _dragGhost.gameObject.SetActive(false);

            _root.gameObject.SetActive(false);
            Jotunn.Logger.LogInfo("Skills panel added.");
        }

        private static void BuildClose()
        {
            var close = UiKit.Image("Close", _root, SheetArt.Close, Image.Type.Simple, true);
            if (!close.sprite)
            {
                close.color = new Color(0.2f, 0.18f, 0.16f);
            }
            Place(close.rectTransform, SkillsPanelLayout.Close);
            var button = close.gameObject.AddComponent<Button>();
            button.targetGraphic = close;
            button.onClick.AddListener(Close);
        }

        private static void BuildCharacter()
        {
            var box = UiKit.Inset("Character", _root).rectTransform;
            Place(box, SkillsPanelLayout.Character);

            _level = UiKit.Text("Level", box, 26, UiKit.Gold, TextAnchor.MiddleLeft);
            Place(_level.rectTransform, SkillsPanelLayout.Level);
            var xpFrame = UiKit.Image("Xp", box, SheetArt.Bar, Image.Type.Sliced);
            Place(xpFrame.rectTransform, SkillsPanelLayout.XpBar);
            _xpFill = UiKit.Image("Fill", xpFrame.rectTransform, SheetArt.BarFill);
            _xpFill.color = UiKit.Xp;
            _xpFill.type = Image.Type.Filled;
            _xpFill.fillMethod = Image.FillMethod.Horizontal;
            UiKit.Stretch(_xpFill.rectTransform, 2f);
            _xpText = UiKit.Text("XpText", box, 14, UiKit.LightDim, TextAnchor.MiddleLeft);
            Place(_xpText.rectTransform, SkillsPanelLayout.XpText);
            _statPoints = UiKit.Text("StatPoints", box, 16, UiKit.LightDim, TextAnchor.MiddleLeft);
            Place(_statPoints.rectTransform, SkillsPanelLayout.StatPoints);

            for (int i = 0; i < Attributes.Length; i++)
            {
                AttributeNames[i] = UiKit.Text("Attribute" + i, box, 18, UiKit.Light, TextAnchor.MiddleLeft);
                Place(AttributeNames[i].rectTransform, SkillsPanelLayout.AttributeName(i));
                AttributeValues[i] = UiKit.Text("Value" + i, box, 18, UiKit.Light, TextAnchor.MiddleRight);
                Place(AttributeValues[i].rectTransform, SkillsPanelLayout.AttributeValue(i));
                AttributeEffects[i] = UiKit.Text("Effect" + i, box, 14, UiKit.LightDim, TextAnchor.MiddleLeft);
                Place(AttributeEffects[i].rectTransform, SkillsPanelLayout.AttributeEffect(i));
                string attribute = Attributes[i];
                var size = new Vector2(SkillsPanelLayout.PlusSize, SkillsPanelLayout.PlusSize);
                var plus = UiKit.Button("+", box, size, () => SpendStat(attribute));
                Place((RectTransform)plus.transform, SkillsPanelLayout.AttributePlus(i));
                AttributeButtons[i] = plus.gameObject;
            }
        }

        private static void BuildDetails()
        {
            var box = UiKit.Inset("Details", _root).rectTransform;
            Place(box, SkillsPanelLayout.Details);

            var icon = SkillsPanelLayout.DetailsIcon;
            _detailsGlow = UiKit.Image("Glow", box, SheetArt.GlowRing);
            float glow = icon.Width * SheetArt.GlowRingScale;
            Place(_detailsGlow.rectTransform, new SheetRect(icon.X + icon.Width / 2f - glow / 2f, icon.Y + icon.Height / 2f - glow / 2f, glow, glow));
            _detailsMedallion = UiKit.Image("Medallion", box, SheetArt.Medallion);
            Place(_detailsMedallion.rectTransform, icon);
            _detailsIcon = UiKit.Image("Icon", _detailsMedallion.rectTransform, null);
            _detailsIcon.preserveAspect = true;
            UiKit.Stretch(_detailsIcon.rectTransform, icon.Width * 0.16f);

            _detailsName = UiKit.Text("Name", box, 22, UiKit.Gold, TextAnchor.MiddleLeft);
            FitText(_detailsName, 14, 22);
            Place(_detailsName.rectTransform, SkillsPanelLayout.DetailsName);
            _detailsRank = UiKit.Text("Rank", box, 16, UiKit.Light, TextAnchor.MiddleLeft);
            Place(_detailsRank.rectTransform, SkillsPanelLayout.DetailsRank);
            _detailsKind = UiKit.Text("Kind", box, 14, UiKit.LightDim, TextAnchor.MiddleLeft);
            Place(_detailsKind.rectTransform, SkillsPanelLayout.DetailsKind);
            _detailsText = UiKit.Text("Text", box, 16, UiKit.Light, TextAnchor.UpperLeft);
            _detailsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailsText.verticalOverflow = VerticalWrapMode.Truncate;
            _detailsText.lineSpacing = 1.05f;
            Place(_detailsText.rectTransform, SkillsPanelLayout.DetailsText);

            var learn = SkillsPanelLayout.Learn;
            _learn = UiKit.Button("Learn", box, new Vector2(learn.Width, learn.Height), () =>
            {
                string id = DetailsSkill();
                if (id.Length > 0)
                {
                    SpendSkill(id);
                }
            });
            Place((RectTransform)_learn.transform, learn);
            _learnLabel = _learn.GetComponentInChildren<Text>();
        }

        private static void BuildTabs()
        {
            for (int i = 0; i < TabLabels.Length; i++)
            {
                int tab = i;
                var rect = SkillsPanelLayout.Tab(i);
                // The game's own button, as the inventory's and Corruption's bag tabs use.
                var button = UiKit.Button("", _root, new Vector2(rect.Width, rect.Height), () =>
                {
                    _tab = tab;
                    _selected = "";
                    _lastCounter = -1;
                });
                Place((RectTransform)button.transform, rect);
                TabImages[i] = button.image;
                TabLabels[i] = button.GetComponentInChildren<Text>();
                if (TabLabels[i])
                {
                    FitText(TabLabels[i], 11, 16);
                }
            }
        }

        private static void BuildTree()
        {
            // The tablet: a dark well in a bronze, riveted rim, like the slots.
            var tablet = UiKit.Image("Tablet", _root, SheetArt.Tile, Image.Type.Sliced);
            if (!tablet.sprite)
            {
                tablet.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
            }
            Place(tablet.rectTransform, SkillsPanelLayout.Tablet);

            var tree = UiKit.Rect("Tree", _root);
            Place(tree, SkillsPanelLayout.Tree);
            for (int row = 1; row <= SkillTreeLayout.Rows; row++)
            {
                float centre = SkillTreeLayout.RowCentre(row);
                var label = UiKit.Text("Tier" + row, tree, 14, UiKit.LightDim, TextAnchor.MiddleCenter);
                UiKit.SetText(label, "Lv " + SkillTreeLayout.TierLevels[row - 1]);
                Place(label.rectTransform, new SheetRect(0f, centre - 10f, SkillTreeLayout.LabelWidth - 8f, 20f));
                if (row > 1)
                {
                    var divider = UiKit.Image("Divider" + row, tree, null);
                    divider.color = Divider;
                    float y = centre - SkillTreeLayout.RowPitch / 2f;
                    Place(divider.rectTransform, new SheetRect(SkillTreeLayout.LabelWidth, y, SkillTreeLayout.GridWidth - SkillTreeLayout.LabelWidth - 8f, 1f));
                }
            }
            // Paths under the medallions.
            _linesRoot = UiKit.Rect("Paths", tree);
            UiKit.Stretch(_linesRoot);
            _tilesRoot = UiKit.Rect("Skills", tree);
            UiKit.Stretch(_tilesRoot);
        }

        private static void BuildSlots()
        {
            var labelRect = SkillsPanelLayout.SlotsLabel;
            var label = UiKit.Text("SlotsLabel", _root, 20, UiKit.Gold, TextAnchor.MiddleLeft);
            UiKit.SetText(label, "Skill slots");
            Place(label.rectTransform, new SheetRect(labelRect.X, labelRect.Y + labelRect.Height / 2f - 22f, labelRect.Width, 24f));
            var hint = UiKit.Text("SlotsHint", _root, 14, UiKit.LightDim, TextAnchor.MiddleLeft);
            UiKit.SetText(hint, "drag skills here");
            Place(hint.rectTransform, new SheetRect(labelRect.X, labelRect.Y + labelRect.Height / 2f + 2f, labelRect.Width, 20f));

            for (int i = 0; i < Slots.Length; i++)
            {
                var frame = UiKit.Image("Slot" + i, _root, SheetArt.Tile, Image.Type.Sliced, true);
                if (!frame.sprite)
                {
                    frame.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
                }
                Place(frame.rectTransform, SkillsPanelLayout.Slot(i));
                var view = frame.gameObject.AddComponent<SlotTileView>();
                view.Index = i;
                view.Icon = UiKit.Image("Icon", frame.rectTransform, null);
                view.Icon.preserveAspect = true;
                view.Icon.enabled = false;
                UiKit.Stretch(view.Icon.rectTransform, 9f);
                view.Key = UiKit.Text("Key", frame.rectTransform, 14, Color.white, TextAnchor.UpperLeft);
                UiKit.Place(view.Key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(7f, -4f), new Vector2(30f, 16f));
                Slots[i] = view;
            }
        }

        // ------------------------------------------------------------ refresh

        private static void Refresh()
        {
            string character = ClassesSource.Character();
            string classId = FlatJson.Get(character, "class") ?? "";
            if (classId.Length == 0)
            {
                Close();
                return;
            }
            if (classId != _classId)
            {
                _classId = classId;
                _skillIds = FlatJson.Strings(ClassesSource.SkillIds(classId));
                _tab = 0;
                _selected = "";
            }
            string classInfo = ClassesSource.ClassInfo(classId);
            UiKit.SetText(_title, (FlatJson.Get(classInfo, "name") ?? classId).ToUpperInvariant());
            for (int i = 0; i < TabLabels.Length; i++)
            {
                UiKit.SetText(TabLabels[i], FlatJson.Get(classInfo, "tab" + i) ?? "");
                if (TabImages[i])
                {
                    TabImages[i].color = i == _tab ? Color.white : new Color(0.62f, 0.6f, 0.58f, 1f);
                }
                if (TabLabels[i])
                {
                    TabLabels[i].color = i == _tab ? UiKit.Gold : UiKit.LightDim;
                }
            }

            int skillPoints = FlatJson.GetInt(character, "skillPoints", 0);
            int statPoints = FlatJson.GetInt(character, "statPoints", 0);
            UiKit.SetText(_skillPoints, skillPoints > 0 ? "<color=#8FD97A>Skill points: " + skillPoints + "</color>" : "Skill points: 0");
            long xp = FlatJson.GetLong(character, "xp", 0);
            long start = FlatJson.GetLong(character, "xpLevelStart", 0);
            long next = FlatJson.GetLong(character, "xpNextLevel", 0);
            bool max = next <= start;
            UiKit.SetText(_level, "Level " + FlatJson.GetInt(character, "level", 1));
            _xpFill.fillAmount = max ? 1f : Mathf.Clamp01((float)(xp - start) / (next - start));
            UiKit.SetText(_xpText, max ? "Highest level" : (xp - start) + " / " + (next - start) + " XP");
            UiKit.SetText(_statPoints, statPoints > 0 ? "<color=#8FD97A>Stat points: " + statPoints + "</color>" : "Stat points: 0");
            for (int i = 0; i < Attributes.Length; i++)
            {
                string info = ClassesSource.AttributeInfo(Attributes[i]);
                UiKit.SetText(AttributeNames[i], FlatJson.Get(info, "name") ?? Attributes[i]);
                UiKit.SetText(AttributeValues[i], FlatJson.GetInt(info, "value", 0).ToString());
                UiKit.SetText(AttributeEffects[i], FlatJson.Get(info, "effect") ?? "");
                AttributeButtons[i].SetActive(statPoints > 0);
            }

            Skills.Clear();
            ById.Clear();
            foreach (string id in _skillIds)
            {
                var skill = SkillView.Parse(ClassesSource.SkillInfo(id));
                if (skill != null)
                {
                    Skills.Add(skill);
                    ById[skill.Id] = skill;
                }
            }
            var shown = SkillTreeLayout.Visible(Skills, _tab);
            if (!ById.TryGetValue(_selected, out var current) || current.Tab != _tab)
            {
                // Start on the first skill a point can go into, else the first of the tab.
                _selected = "";
                foreach (var skill in shown)
                {
                    if (_selected.Length == 0 || (skill.CanSpend && !ById[_selected].CanSpend))
                    {
                        _selected = skill.Id;
                    }
                }
            }
            RebuildTree(shown);

            string bar = ClassesSource.SkillBar();
            for (int i = 0; i < Slots.Length; i++)
            {
                var view = Slots[i];
                string id = FlatJson.Get(bar, "slot" + i) ?? "";
                view.SkillId = id;
                var icon = id.Length > 0 ? ClassesSource.SkillIcon(id) : null;
                view.Icon.sprite = icon;
                view.Icon.enabled = icon;
                UiKit.SetText(view.Key, FlatJson.Get(bar, "key" + i) ?? "");
            }
            ShowDetails();
        }

        /// <summary>Rebuilds the shown tab's medallions and paths (a few dozen objects, only when something changed).</summary>
        private static void RebuildTree(List<SkillView> shown)
        {
            foreach (var go in TreeObjects)
            {
                UnityEngine.Object.Destroy(go);
            }
            TreeObjects.Clear();
            SelectionRings.Clear();

            // Paths: a dark groove; a learned prerequisite's path carries an eitr-blue light, like a rune (mock-up D).
            var lines = SkillTreeLayout.Lines(shown);
            foreach (var line in lines)
            {
                AddPath(line, null, Groove, 6f);
            }
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (var line in lines)
                {
                    bool learned = ById.TryGetValue(line.From, out var from) && from.Points > 0;
                    if (learned != (pass == 1))
                    {
                        continue;
                    }
                    if (learned)
                    {
                        AddPath(line, SheetArt.GlowLine, new Color(UiKit.Rune.r, UiKit.Rune.g, UiKit.Rune.b, 0.55f), 12f);
                    }
                    AddPath(line, null, learned ? PathLit : PathDim, 2f);
                }
            }
            foreach (var skill in shown)
            {
                TreeObjects.Add(BuildSkill(skill));
            }
            UpdateSelection();
        }

        private static void AddPath(TreeLine line, Sprite sprite, Color colour, float thickness)
        {
            var image = UiKit.Image("Path", _linesRoot, sprite);
            image.color = colour;
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(line.Length, thickness);
            rt.anchoredPosition = new Vector2((line.X1 + line.X2) / 2f, -(line.Y1 + line.Y2) / 2f);
            rt.localRotation = Quaternion.Euler(0f, 0f, line.AngleDegrees);
            TreeObjects.Add(image.gameObject);
        }

        /// <summary>
        ///     A skill as a round bronze medallion (mock-up D): an eitr-blue glow once it has points (gold when maxed), green
        ///     when a point can be spent; greyed when locked, darker when it comes later; its points in a badge.
        /// </summary>
        private static GameObject BuildSkill(SkillView skill)
        {
            SkillTreeLayout.TilePosition(skill.Row, skill.Col, out float x, out float y);
            float size = SkillTreeLayout.Tile;
            var root = UiKit.Rect("Skill_" + skill.Id, _tilesRoot);
            Place(root, new SheetRect(x, y, size, size));
            bool locked = skill.Later || (skill.Points == 0 && !skill.CanSpend);
            float glowSize = size * SheetArt.GlowRingScale;

            var ring = UiKit.Image("Selected", root, SheetArt.GlowRing);
            ring.color = SelectedRing;
            UiKit.Place(ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(glowSize * 1.12f, glowSize * 1.12f));
            ring.enabled = false;
            SelectionRings[skill.Id] = ring;

            if (!skill.Later && (skill.Points > 0 || skill.CanSpend))
            {
                var glow = UiKit.Image("Glow", root, SheetArt.GlowRing);
                Color c = skill.Maxed ? UiKit.Gold : skill.Points > 0 ? UiKit.Rune : UiKit.CanSpend;
                glow.color = new Color(c.r, c.g, c.b, 0.75f);
                UiKit.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(glowSize, glowSize));
            }

            var medallion = UiKit.Image("Medallion", root, SheetArt.Medallion, Image.Type.Simple, true);
            medallion.color = !medallion.sprite ? new Color(0.08f, 0.07f, 0.06f, 0.9f)
                : skill.Later ? new Color(0.45f, 0.43f, 0.41f) : locked ? new Color(0.72f, 0.7f, 0.68f) : Color.white;
            UiKit.Stretch(medallion.rectTransform);

            var icon = UiKit.Image("Icon", medallion.rectTransform, ClassesSource.SkillIcon(skill.Id));
            icon.preserveAspect = true;
            icon.enabled = icon.sprite;
            icon.color = skill.Later ? new Color(0.37f, 0.37f, 0.37f, 1f) : locked ? new Color(0.65f, 0.65f, 0.65f, 1f) : Color.white;
            UiKit.Stretch(icon.rectTransform, size * 0.16f);

            if (!skill.Later)
            {
                // Points badge at the bottom right: a dark disc in a bronze ring.
                var rim = UiKit.Image("BadgeRim", root, SheetArt.Disc);
                rim.color = new Color(0.66f, 0.47f, 0.24f);
                UiKit.Place(rim.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-6f, 6f), new Vector2(24f, 24f));
                var badge = UiKit.Image("Badge", rim.rectTransform, SheetArt.Disc);
                badge.color = new Color(0.06f, 0.05f, 0.04f, 0.96f);
                UiKit.Stretch(badge.rectTransform, 2f);
                var points = UiKit.Text("Points", badge.rectTransform, 13, skill.CanSpend ? UiKit.CanSpend : UiKit.Light, TextAnchor.MiddleCenter, false);
                UiKit.Stretch(points.rectTransform);
                UiKit.SetText(points, skill.Points.ToString());
            }

            var view = medallion.gameObject.AddComponent<SkillTileView>();
            view.SkillId = skill.Id;
            return root.gameObject;
        }

        private static void UpdateSelection()
        {
            foreach (var pair in SelectionRings)
            {
                if (pair.Value)
                {
                    pair.Value.enabled = pair.Value.sprite && pair.Key == _selected;
                }
            }
        }

        // ------------------------------------------------------------ details pane

        /// <summary>The hovered skill, else the selected one.</summary>
        private static string DetailsSkill() => _hovered.Length > 0 ? _hovered : _selected;

        private static void ShowDetails()
        {
            if (!_detailsName)
            {
                return;
            }
            if (!ById.TryGetValue(DetailsSkill(), out var skill))
            {
                UiKit.SetText(_detailsName, "");
                UiKit.SetText(_detailsRank, "");
                UiKit.SetText(_detailsKind, "");
                UiKit.SetText(_detailsText, "Point at a skill to see what it does.");
                _detailsIcon.enabled = false;
                _detailsGlow.enabled = false;
                _learn.interactable = false;
                return;
            }
            bool locked = skill.Later || (skill.Points == 0 && !skill.CanSpend);
            _detailsIcon.sprite = ClassesSource.SkillIcon(skill.Id);
            _detailsIcon.enabled = _detailsIcon.sprite;
            _detailsIcon.color = skill.Later ? new Color(0.37f, 0.37f, 0.37f) : locked ? new Color(0.65f, 0.65f, 0.65f) : Color.white;
            _detailsMedallion.color = skill.Later ? new Color(0.45f, 0.43f, 0.41f) : locked ? new Color(0.72f, 0.7f, 0.68f) : Color.white;
            Color glow = skill.Maxed ? UiKit.Gold : skill.Points > 0 ? UiKit.Rune : UiKit.CanSpend;
            _detailsGlow.enabled = _detailsGlow.sprite && !skill.Later && (skill.Points > 0 || skill.CanSpend);
            _detailsGlow.color = new Color(glow.r, glow.g, glow.b, 0.75f);

            UiKit.SetText(_detailsName, skill.Name);
            string rank = skill.Later ? "Coming later" : "Rank " + skill.Points + " / " + skill.MaxPoints +
                                                         (skill.Level != skill.Points ? "  (level " + skill.Level + ")" : "");
            UiKit.SetText(_detailsRank, rank);
            var kind = new StringBuilder(KindLabel(skill.Kind)).Append("  ·  needs level ").Append(skill.ReqLevel);
            if (skill.EitrCost > 0f)
            {
                kind.Append("  ·  ").Append(skill.EitrCost.ToString("0.#", CultureInfo.InvariantCulture)).Append(" eitr");
            }
            UiKit.SetText(_detailsKind, kind.ToString());

            var text = new StringBuilder();
            if (!string.IsNullOrEmpty(skill.Description))
            {
                text.Append(skill.Description).Append("\n\n");
            }
            if (!skill.Later && !skill.Maxed && !string.IsNullOrEmpty(skill.NextDescription))
            {
                text.Append("<color=#8FD97A>Next rank: ").Append(skill.NextDescription).Append("</color>\n\n");
            }
            if (!skill.Later && !string.IsNullOrEmpty(skill.SpendBlocked))
            {
                text.Append("<color=#FF9A80>").Append(skill.SpendBlocked).Append("</color>\n\n");
            }
            if (skill.Later)
            {
                text.Append("<color=#C7B8A1>Not in the game yet.</color>");
            }
            else if (skill.Passive)
            {
                text.Append("<color=#C7B8A1>Passive: always on once learned.</color>");
            }
            else
            {
                text.Append("<color=#C7B8A1>Drag it onto a skill slot to use it.</color>");
            }
            UiKit.SetText(_detailsText, text.ToString().TrimEnd('\n'));

            _learn.interactable = skill.CanSpend;
            UiKit.SetText(_learnLabel, skill.CanSpend ? "Learn  (+1 rank)" : skill.Maxed ? "Mastered" : "Learn");
        }

        private static string KindLabel(string kind)
        {
            switch (kind)
            {
                case "passive": return "Passive";
                case "cast": return "Cast on its key";
                default: return "Attack on the skill button";
            }
        }

        // ------------------------------------------------------------ actions

        private static void SpendStat(string attribute)
        {
            ShowMessage(ClassesSource.SpendStatPoint(attribute));
            _lastCounter = -1;
        }

        private static void SpendSkill(string skillId)
        {
            ShowMessage(ClassesSource.SpendSkillPoint(skillId));
            _lastCounter = -1;
        }

        private static void SetSlot(int slot, string skillId)
        {
            ShowMessage(ClassesSource.SetSkillSlot(slot, skillId));
            _lastCounter = -1;
        }

        private static void ShowMessage(string text)
        {
            UiKit.SetText(_message, text ?? "");
            _messageUntil = Time.unscaledTime + 4f;
        }

        private static void Select(string skillId)
        {
            _selected = skillId;
            UpdateSelection();
            ShowDetails();
        }

        private static void Hover(string skillId)
        {
            _hovered = skillId ?? "";
            ShowDetails();
        }

        // ------------------------------------------------------------ dragging

        private static string _dragSkill = "";
        private static int _dragFromSlot = -1;

        private static void BeginDrag(string skillId, int fromSlot, PointerEventData data)
        {
            var icon = ClassesSource.SkillIcon(skillId);
            _dragSkill = skillId;
            _dragFromSlot = fromSlot;
            _dragGhost.sprite = icon;
            _dragGhost.color = icon ? new Color(1f, 1f, 1f, 0.85f) : new Color(1f, 0.8f, 0.4f, 0.6f);
            _dragGhost.gameObject.SetActive(true);
            _dragGhost.transform.SetAsLastSibling();
            MoveGhost(data);
        }

        private static void MoveGhost(PointerEventData data)
        {
            if (!_dragGhost || !_dragGhost.gameObject.activeSelf)
            {
                return;
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, data.position, data.pressEventCamera, out Vector2 local))
            {
                _dragGhost.rectTransform.anchorMin = _dragGhost.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                _dragGhost.rectTransform.anchoredPosition = local;
            }
        }

        private static void Drop(PointerEventData data)
        {
            if (_dragSkill.Length == 0)
            {
                return;
            }
            int target = PanelSlotAt(data.position, data.pressEventCamera);
            if (target < 0)
            {
                target = SkillCross.SlotAt(data.position);
            }
            if (target >= 0)
            {
                SetSlot(target, _dragSkill);
            }
            else if (_dragFromSlot >= 0)
            {
                // Dragged out of a slot onto nothing: clear it.
                SetSlot(_dragFromSlot, "");
            }
            EndDrag();
        }

        private static void EndDrag()
        {
            _dragSkill = "";
            _dragFromSlot = -1;
            if (_dragGhost)
            {
                _dragGhost.gameObject.SetActive(false);
            }
        }

        private static int PanelSlotAt(Vector2 screenPoint, Camera camera)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i] && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)Slots[i].transform, screenPoint, camera))
                {
                    return i;
                }
            }
            return -1;
        }

        private sealed class SkillTileView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
            IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public string SkillId;

            public void OnPointerClick(PointerEventData data)
            {
                if (data.button != PointerEventData.InputButton.Left || data.dragging)
                {
                    return;
                }
                Select(SkillId);
                // Double-click learns, as a shortcut for the Learn button.
                if (data.clickCount >= 2)
                {
                    SpendSkill(SkillId);
                }
            }

            public void OnPointerEnter(PointerEventData data) => Hover(SkillId);

            public void OnPointerExit(PointerEventData data) => Hover("");

            public void OnBeginDrag(PointerEventData data)
            {
                if (!ById.TryGetValue(SkillId, out var skill))
                {
                    return;
                }
                if (skill.Passive)
                {
                    ShowMessage("Passive skills don't take a slot");
                    return;
                }
                BeginDrag(SkillId, -1, data);
            }

            public void OnDrag(PointerEventData data) => MoveGhost(data);

            public void OnEndDrag(PointerEventData data) => Drop(data);
        }

        private sealed class SlotTileView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
            IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public int Index;
            public string SkillId = "";
            public Image Icon;
            public Text Key;

            public void OnPointerClick(PointerEventData data)
            {
                // Right-click empties the slot.
                if (data.button == PointerEventData.InputButton.Right && SkillId.Length > 0)
                {
                    SetSlot(Index, "");
                }
            }

            // The slot's skill shows in the details pane, since the slots have no names.
            public void OnPointerEnter(PointerEventData data) => Hover(SkillId);

            public void OnPointerExit(PointerEventData data) => Hover("");

            public void OnBeginDrag(PointerEventData data)
            {
                if (SkillId.Length > 0)
                {
                    BeginDrag(SkillId, Index, data);
                }
            }

            public void OnDrag(PointerEventData data) => MoveGhost(data);

            public void OnEndDrag(PointerEventData data) => Drop(data);
        }

        // ------------------------------------------------------------ inventory button

        private static void UpdateInventoryButton()
        {
            var gui = InventoryGui.instance;
            bool show = gui && InventoryGui.IsVisible() && gui.m_player && ClassesSource.Available
                        && !string.IsNullOrEmpty(FlatJson.Get(ClassesSource.Character(), "class"));
            if (!show)
            {
                if (_inventoryButton)
                {
                    _inventoryButton.SetActive(false);
                }
                return;
            }
            if (!_inventoryButton)
            {
                // Top right of the player's inventory, above it (Corruption's bag tabs sit above its top left).
                _inventoryButton = GUIManager.Instance.CreateButton("Skills", gui.m_player, new Vector2(1f, 1f), new Vector2(1f, 1f),
                    Vector2.zero, 112f, 30f);
                _inventoryButton.name = "CraftingOverhaul_SkillsButton";
                var rt = (RectTransform)_inventoryButton.transform;
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-14f, 2f);
                _inventoryButton.GetComponent<Button>().onClick.AddListener(Toggle);
            }
            _inventoryButton.SetActive(true);
        }

        // ------------------------------------------------------------ per frame

        private static bool CanUseKeys()
        {
            return Player.m_localPlayer && !global::Console.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible()
                   && !(Chat.instance && Chat.instance.HasFocus()) && !Minimap.InTextInput() && !StoreGui.IsVisible();
        }

        private static void Update()
        {
            if (!ClassesSource.Available)
            {
                if (IsOpen)
                {
                    Close();
                }
                return;
            }
            UpdateInventoryButton();
            // While the panel is open the game's input is blocked, which Jötunn reports as text input; the key still closes it.
            if ((IsOpen || CanUseKeys()) && ModConfig.SkillsPanelKey.Value.IsDown())
            {
                Toggle();
            }
            if (!IsOpen)
            {
                return;
            }
            int counter = ClassesSource.ChangeCounter;
            if (counter != _lastCounter)
            {
                _lastCounter = counter;
                Refresh();
            }
            if (_message && _message.text.Length > 0 && Time.unscaledTime > _messageUntil)
            {
                UiKit.SetText(_message, "");
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.Update))]
        private static class Update_Patch
        {
            private static void Postfix()
            {
                try
                {
                    Update();
                }
                catch (Exception ex)
                {
                    Safe.Report("update the skills panel", ex);
                }
            }
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                Close();
                if (_root)
                {
                    UnityEngine.Object.Destroy(_root.gameObject);
                }
                _root = null;
                _inventoryButton = null;
                _classId = "";
                _selected = "";
                _hovered = "";
                _skillIds = new string[0];
                TreeObjects.Clear();
                SelectionRings.Clear();
            }
        }

        /// <summary>Shrinks a label to its box (down to <paramref name="min" />) instead of letting it run into its neighbours.</summary>
        private static void FitText(Text text, int min, int max)
        {
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = min;
            text.resizeTextMaxSize = max;
        }

        /// <summary>Places a rect at a layout rectangle (top-left, y down) inside its parent.</summary>
        private static void Place(RectTransform rt, SheetRect r)
        {
            UiKit.Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(r.X, -r.Y), new Vector2(r.Width, r.Height));
        }
    }
}
