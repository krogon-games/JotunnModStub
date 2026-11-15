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
    ///     The skills panel (brief §2.4, §2.5). Skill cards since 2026-11-15 (the skill tree overhaul; Krogon's mock-up
    ///     <c>docs/images/skills_panel_cards_mockup.webp</c>, drawn in the rest of the UI's style). Left: the character box
    ///     (level, XP, stat points and the four attributes, each with its icon, what it gives and a + button) and the skill details pane (the hovered or
    ///     selected skill: name, rank, kind, cost, what it does now and next). Right: the class's three tabs, the tab's
    ///     nine skills as cards on a bronze-rimmed tablet (medallion, name, rank, a rank bar; no tiers or paths, since any
    ///     skill can be learned from level 1), and a strip with the four skill slots and the Learn button. Click a card to
    ///     select it, double-click or Learn to spend a point, drag it onto a slot here or on the HUD's skill cross. Every
    ///     position comes from <see cref="SkillsPanelLayout" /> and <see cref="SkillCardsLayout" />, which
    ///     <c>tools/preview_skills_panel.py</c> also draws from.
    ///     Opened with <see cref="ModConfig.SkillsPanelKey" /> (the Skills button on the inventory was removed 2026-11-09, user: it clashed with the inventory tabs; its place is to be decided); everything shown and
    ///     every action goes through <see cref="ClassesSource" />.
    /// </summary>
    internal static class SkillsPanel
    {
        private static readonly string[] Attributes = { "strength", "dexterity", "vitality", "energy" };

        /// <summary>Icon inset on a medallion, as a share of its size: the icons carry their own margin (make_skill_icons.py).</summary>
        private const float IconInset = 0.06f;

        // Card colours (mirrored in tools/preview_skills_panel.py).
        private static readonly Color Divider = new Color(0.66f, 0.47f, 0.24f, 0.4f);
        /// <summary>The selected card's gold glow, as the mock-up's.</summary>
        private static readonly Color SelectedGlow = new Color(1f, 0.78f, 0.36f, 1f);
        /// <summary>How bright a card's wood is: full once learned, dimmer before, dark for skills that come later.</summary>
        private const float CardLearned = 1f;
        private const float CardNotLearned = 0.8f;
        private const float CardLater = 0.64f;

        private static RectTransform _root;
        private static RectTransform _cardsRoot;
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
        private static readonly SlotTileView[] Slots = new SlotTileView[ClassesSource.Slots];
        private static Image _dragGhost;

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
        private static readonly Dictionary<string, Image> SelectionGlows = new Dictionary<string, Image>(StringComparer.Ordinal);
        private static readonly List<GameObject> CardObjects = new List<GameObject>();
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
            // As large as the screen allows (user, 2026-11-13), or the size set in the config.
            var canvas = GUIManager.CustomGUIFront.GetComponent<RectTransform>();
            float scale = SkillsPanelLayout.Scale(canvas ? canvas.rect.width : 0f, canvas ? canvas.rect.height : 0f, ModConfig.SkillsPanelSize.Value);
            _root.localScale = new Vector3(scale, scale, 1f);
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
            BuildCards();
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
                var attributeIcon = UiKit.Image("Icon" + i, box, SheetArt.AttributeIcon(Attributes[i]));
                attributeIcon.preserveAspect = true;
                attributeIcon.enabled = attributeIcon.sprite;
                Place(attributeIcon.rectTransform, SkillsPanelLayout.AttributeIcon(i));
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
            UiKit.Stretch(_detailsIcon.rectTransform, icon.Width * IconInset);

            _detailsName = UiKit.Text("Name", box, 22, UiKit.Gold, TextAnchor.MiddleLeft);
            FitText(_detailsName, 14, 22);
            Place(_detailsName.rectTransform, SkillsPanelLayout.DetailsName);
            _detailsRank = UiKit.Text("Rank", box, 16, UiKit.Light, TextAnchor.MiddleLeft);
            Place(_detailsRank.rectTransform, SkillsPanelLayout.DetailsRank);
            _detailsKind = UiKit.Text("Kind", box, 14, UiKit.LightDim, TextAnchor.MiddleLeft);
            FitText(_detailsKind, 10, 14);
            Place(_detailsKind.rectTransform, SkillsPanelLayout.DetailsKind);
            _detailsText = UiKit.Text("Text", box, 15, UiKit.Light, TextAnchor.UpperLeft);
            _detailsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailsText.verticalOverflow = VerticalWrapMode.Truncate;
            _detailsText.lineSpacing = 1.05f;
            Place(_detailsText.rectTransform, SkillsPanelLayout.DetailsText);

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

        private static void BuildCards()
        {
            // The tablet: a dark well in a bronze, riveted rim, like the slots; the cards lie on it.
            var tablet = UiKit.Image("Tablet", _root, SheetArt.Tile, Image.Type.Sliced);
            if (!tablet.sprite)
            {
                tablet.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
            }
            Place(tablet.rectTransform, SkillsPanelLayout.Tablet);
            _cardsRoot = UiKit.Rect("Cards", _root);
            Place(_cardsRoot, SkillsPanelLayout.Cards);
        }

        private static void BuildSlots()
        {
            // The strip under the tablet: the slots on the left, Learn on the right (the mock-up's bottom bar).
            var strip = UiKit.Inset("SlotsStrip", _root).rectTransform;
            Place(strip, SkillsPanelLayout.SlotsRow);
            var divider = UiKit.Image("SlotsDivider", _root, null);
            divider.color = Divider;
            Place(divider.rectTransform, SkillsPanelLayout.SlotsDivider);

            var learn = SkillsPanelLayout.Learn;
            _learn = UiKit.Button("Learn", _root, new Vector2(learn.Width, learn.Height), () =>
            {
                string id = DetailsSkill();
                if (id.Length > 0)
                {
                    SpendSkill(id);
                }
            });
            Place((RectTransform)_learn.transform, learn);
            _learnLabel = _learn.GetComponentInChildren<Text>();
            if (_learnLabel)
            {
                FitText(_learnLabel, 12, 18);
            }

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
                UiKit.Stretch(view.Icon.rectTransform, 4f);
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
            var shown = SkillCardsLayout.Visible(Skills, _tab);
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
            RebuildCards(shown);

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

        /// <summary>Rebuilds the shown tab's cards (nine, a handful of objects each, only when something changed).</summary>
        private static void RebuildCards(List<SkillView> shown)
        {
            foreach (var go in CardObjects)
            {
                UnityEngine.Object.Destroy(go);
            }
            CardObjects.Clear();
            SelectionGlows.Clear();
            foreach (var skill in shown)
            {
                CardObjects.Add(BuildCard(skill));
            }
            UpdateSelection();
        }

        /// <summary>The glow colour of a skill's medallion: gold when maxed, eitr-blue once learned, green when a point can go in.</summary>
        private static Color GlowColour(CardState state)
        {
            return state == CardState.Maxed ? UiKit.Gold : state == CardState.Learned ? UiKit.Rune : UiKit.CanSpend;
        }

        /// <summary>
        ///     A skill as a card (Krogon's mock-up, 2026-11-15): a small panel of the game's own wood with the skill's round
        ///     bronze medallion, its name, its rank and, once it has points, a rank bar. The medallion glows eitr-blue once
        ///     learned (gold when maxed), green when a point can go in; the card is dimmer before it's learned and dark
        ///     for a skill that comes later. The selected card has a gold glow around it.
        /// </summary>
        private static GameObject BuildCard(SkillView skill)
        {
            var state = SkillCardsLayout.StateOf(skill);
            bool later = state == CardState.Later;
            bool learned = state == CardState.Learned || state == CardState.Maxed;
            var root = UiKit.Rect("Skill_" + skill.Id, _cardsRoot);
            Place(root, SkillCardsLayout.Card(skill.Row, skill.Col));

            var selected = UiKit.Image("Selected", root, SheetArt.GlowBox, Image.Type.Sliced);
            selected.color = SelectedGlow;
            UiKit.Stretch(selected.rectTransform, -SheetArt.GlowBoxInset);
            selected.enabled = false;
            SelectionGlows[skill.Id] = selected;

            var card = UiKit.Image("Card", root, null, Image.Type.Simple, true);
            SheetArt.StyleAsGamePanel(card);
            float wood = later ? CardLater : learned ? CardLearned : CardNotLearned;
            card.color = new Color(card.color.r * wood, card.color.g * wood, card.color.b * wood, card.color.a);
            UiKit.Stretch(card.rectTransform);

            var iconRect = SkillCardsLayout.Icon;
            if (state != CardState.Later && state != CardState.NotLearned)
            {
                var glow = UiKit.Image("Glow", root, SheetArt.GlowRing);
                Color c = GlowColour(state);
                glow.color = new Color(c.r, c.g, c.b, 0.75f);
                float glowSize = iconRect.Width * SheetArt.GlowRingScale;
                Place(glow.rectTransform, new SheetRect(iconRect.X + iconRect.Width / 2f - glowSize / 2f, iconRect.Y + iconRect.Height / 2f - glowSize / 2f, glowSize, glowSize));
            }
            var medallion = UiKit.Image("Medallion", root, SheetArt.Medallion);
            medallion.color = !medallion.sprite ? new Color(0.08f, 0.07f, 0.06f, 0.9f)
                : later ? new Color(0.58f, 0.56f, 0.54f) : learned ? Color.white : new Color(0.82f, 0.8f, 0.78f);
            Place(medallion.rectTransform, iconRect);
            var icon = UiKit.Image("Icon", medallion.rectTransform, ClassesSource.SkillIcon(skill.Id));
            icon.preserveAspect = true;
            icon.enabled = icon.sprite;
            icon.color = later ? new Color(0.52f, 0.52f, 0.52f, 1f) : learned ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
            UiKit.Stretch(icon.rectTransform, iconRect.Width * IconInset);

            var name = UiKit.Text("Name", root, 20, later ? UiKit.LightDim : learned ? UiKit.Gold : UiKit.Light, TextAnchor.MiddleCenter);
            FitText(name, 13, 20);
            UiKit.SetText(name, skill.Name);
            Place(name.rectTransform, SkillCardsLayout.Name);
            var rank = UiKit.Text("Rank", root, 15, state == CardState.CanLearn ? UiKit.CanSpend : learned ? UiKit.Light : UiKit.LightDim, TextAnchor.MiddleCenter);
            UiKit.SetText(rank, SkillCardsLayout.RankLabel(skill));
            Place(rank.rectTransform, SkillCardsLayout.Rank);

            if (learned)
            {
                var bar = UiKit.Image("RankBar", root, SheetArt.Bar, Image.Type.Sliced);
                Place(bar.rectTransform, SkillCardsLayout.RankBar);
                var fill = UiKit.Image("Fill", bar.rectTransform, SheetArt.BarFill);
                fill.color = state == CardState.Maxed ? UiKit.Gold : UiKit.Xp;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = SkillCardsLayout.RankFill(skill);
                UiKit.Stretch(fill.rectTransform, 2f);
            }

            var view = card.gameObject.AddComponent<SkillTileView>();
            view.SkillId = skill.Id;
            return root.gameObject;
        }

        private static void UpdateSelection()
        {
            foreach (var pair in SelectionGlows)
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
                UiKit.SetText(_learnLabel, "Learn");
                return;
            }
            var state = SkillCardsLayout.StateOf(skill);
            bool learned = state == CardState.Learned || state == CardState.Maxed;
            _detailsIcon.sprite = ClassesSource.SkillIcon(skill.Id);
            _detailsIcon.enabled = _detailsIcon.sprite;
            _detailsIcon.color = skill.Later ? new Color(0.52f, 0.52f, 0.52f) : learned ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            _detailsMedallion.color = skill.Later ? new Color(0.58f, 0.56f, 0.54f) : learned ? Color.white : new Color(0.82f, 0.8f, 0.78f);
            Color glow = GlowColour(state);
            _detailsGlow.enabled = _detailsGlow.sprite && state != CardState.Later && state != CardState.NotLearned;
            _detailsGlow.color = new Color(glow.r, glow.g, glow.b, 0.75f);

            UiKit.SetText(_detailsName, skill.Name);
            string rank = skill.Later ? "Coming later" : "Rank " + skill.Points + " / " + skill.MaxPoints +
                                                         (skill.Level != skill.Points ? "  (level " + skill.Level + ")" : "");
            UiKit.SetText(_detailsRank, rank);
            var kind = new StringBuilder(KindLabel(skill.Kind));
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

            // The button says why a point can't go in (play-test 2026-10-31: the reason was cut off at the bottom of the text).
            _learn.gameObject.SetActive(true);
            _learn.interactable = skill.CanSpend;
            string label = skill.CanSpend ? "Learn  (+1 rank)" : skill.Maxed ? "Mastered" : skill.Later ? "Coming later"
                : !string.IsNullOrEmpty(skill.SpendBlocked) ? "<color=#FF9A80>" + skill.SpendBlocked + "</color>" : "Learn";
            UiKit.SetText(_learnLabel, label);
            if (_learnLabel)
            {
                _learnLabel.supportRichText = true;
            }
        }

        private static string KindLabel(string kind)
        {
            switch (kind)
            {
                case "passive": return "Passive";
                case "cast": return "Cast";
                default: return "Attack";
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
                _classId = "";
                _selected = "";
                _hovered = "";
                _skillIds = new string[0];
                CardObjects.Clear();
                SelectionGlows.Clear();
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
