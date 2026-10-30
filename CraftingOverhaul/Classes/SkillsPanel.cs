using System;
using System.Collections.Generic;
using CraftingOverhaul.CharacterSheet;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CraftingOverhaul.Classes
{
    /// <summary>
    ///     The skills panel (brief §2.4, §2.5): the class's three skill trees as tabs, each a 6×3 grid by level tier with
    ///     the prerequisite lines; the four skill slots, filled by dragging skills onto them (or onto the HUD's skill
    ///     cross); and the attribute points down the left. Opened with <see cref="ModConfig.SkillsPanelKey" /> or the
    ///     Skills button on the inventory. Everything shown and every action goes through <see cref="ClassesSource" />.
    /// </summary>
    internal static class SkillsPanel
    {
        private const float Margin = 22f;
        private const float HeaderHeight = 56f;
        private const float TabHeight = 34f;
        private const float AttributesWidth = 230f;
        private const float SlotsHeight = 96f;
        private const float SlotTile = 58f;
        private const float MessageHeight = 24f;

        private static readonly string[] Attributes = { "strength", "dexterity", "vitality", "energy" };

        private static RectTransform _root;
        private static RectTransform _grid;
        private static RectTransform _linesRoot;
        private static RectTransform _tilesRoot;
        private static Text _title;
        private static Text _skillPoints;
        private static Text _message;
        private static Text _level;
        private static Text _statPoints;
        private static readonly Text[] TabLabels = new Text[3];
        private static readonly Image[] TabBacks = new Image[3];
        private static readonly Text[] AttributeTexts = new Text[4];
        private static readonly Text[] AttributeEffects = new Text[4];
        private static readonly GameObject[] AttributeButtons = new GameObject[4];
        private static readonly SlotTileView[] Slots = new SlotTileView[DemoClasses.Slots];
        private static RectTransform _tooltip;
        private static Text _tooltipText;
        private static Image _dragGhost;
        private static GameObject _inventoryButton;

        private static int _tab;
        private static int _lastCounter = -1;
        private static string _classId = "";
        private static string[] _skillIds = new string[0];
        private static readonly List<SkillView> Skills = new List<SkillView>();
        private static readonly Dictionary<string, SkillView> ById = new Dictionary<string, SkillView>(StringComparer.Ordinal);
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
            ShowMessage("");
            ClassWindows.SetOpen(ClassWindows.Skills, true);
        }

        public static void Close()
        {
            HideTooltip();
            EndDrag();
            if (_root)
            {
                _root.gameObject.SetActive(false);
            }
            ClassWindows.SetOpen(ClassWindows.Skills, false);
        }

        // ------------------------------------------------------------ building

        private static float PanelWidth => Margin + AttributesWidth + Margin + SkillTreeLayout.GridWidth + Margin;
        private static float TreeTop => HeaderHeight + TabHeight + 12f;
        private static float PanelHeight => TreeTop + SkillTreeLayout.GridHeight + 16f + SlotsHeight + MessageHeight + Margin;

        private static void EnsureCreated()
        {
            if (_root)
            {
                return;
            }
            _root = UiKit.Panel("CraftingOverhaul_SkillsPanel", GUIManager.CustomGUIFront.transform, new Vector2(PanelWidth, PanelHeight));
            UiKit.Place(_root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelWidth, PanelHeight));

            _title = UiKit.Text("Title", _root, 26, UiKit.Gold, TextAnchor.MiddleCenter);
            PlaceTopLeft(_title.rectTransform, 0f, 10f, PanelWidth, 40f);

            var close = UiKit.Button("X", _root, new Vector2(36f, 36f), Close);
            PlaceTopLeft((RectTransform)close.transform, PanelWidth - Margin - 36f, 12f, 36f, 36f);

            BuildAttributes();
            BuildTabs();

            float treeLeft = Margin + AttributesWidth + Margin;
            _skillPoints = UiKit.Text("SkillPoints", _root, 16, UiKit.Gold, TextAnchor.MiddleRight);
            PlaceTopLeft(_skillPoints.rectTransform, treeLeft, HeaderHeight - 14f, SkillTreeLayout.GridWidth, 20f);

            _grid = UiKit.Rect("Tree", _root);
            PlaceTopLeft(_grid, treeLeft, TreeTop, SkillTreeLayout.GridWidth, SkillTreeLayout.GridHeight);
            // Lines first so the tiles cover their ends.
            _linesRoot = UiKit.Rect("Lines", _grid);
            UiKit.Stretch(_linesRoot);
            _tilesRoot = UiKit.Rect("Tiles", _grid);
            UiKit.Stretch(_tilesRoot);
            for (int row = 1; row <= SkillTreeLayout.Rows; row++)
            {
                SkillTreeLayout.TilePosition(row, 1, out _, out float y);
                var label = UiKit.Text("Tier" + row, _grid, 14, UiKit.Dim, TextAnchor.MiddleLeft);
                UiKit.SetText(label, "Lv " + SkillTreeLayout.TierLevels[row - 1]);
                PlaceTopLeft(label.rectTransform, 0f, y, SkillTreeLayout.LabelWidth, SkillTreeLayout.Tile);
            }

            BuildSlots(treeLeft);

            _message = UiKit.Text("Message", _root, 15, new Color(1f, 0.6f, 0.5f), TextAnchor.MiddleCenter);
            PlaceTopLeft(_message.rectTransform, 0f, PanelHeight - Margin - MessageHeight, PanelWidth, MessageHeight);

            _tooltip = UiKit.Panel("Tooltip", _root, new Vector2(300f, 160f));
            _tooltip.pivot = new Vector2(0f, 1f);
            _tooltip.GetComponent<Image>().raycastTarget = false;
            _tooltipText = UiKit.Text("Text", _tooltip, 15, Color.white, TextAnchor.UpperLeft);
            _tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiKit.Stretch(_tooltipText.rectTransform, 14f);
            _tooltip.gameObject.SetActive(false);

            _dragGhost = UiKit.Image("DragGhost", _root, null);
            _dragGhost.rectTransform.sizeDelta = new Vector2(SlotTile, SlotTile);
            _dragGhost.preserveAspect = true;
            _dragGhost.gameObject.SetActive(false);

            _root.gameObject.SetActive(false);
            Jotunn.Logger.LogInfo("Skills panel added.");
        }

        private static void BuildAttributes()
        {
            float top = HeaderHeight + 4f;
            var parchment = UiKit.Image("Attributes", _root, SheetArt.Parchment);
            if (!parchment.sprite)
            {
                parchment.color = new Color(0.84f, 0.75f, 0.59f);
            }
            PlaceTopLeft(parchment.rectTransform, Margin, top, AttributesWidth, 330f);
            var box = parchment.rectTransform;

            _level = UiKit.Text("Level", box, 17, UiKit.Ink, TextAnchor.UpperLeft, false);
            PlaceTopLeft(_level.rectTransform, 16f, 14f, AttributesWidth - 32f, 44f);
            _statPoints = UiKit.Text("StatPoints", box, 15, UiKit.Ink, TextAnchor.UpperLeft, false);
            PlaceTopLeft(_statPoints.rectTransform, 16f, 62f, AttributesWidth - 32f, 20f);

            for (int i = 0; i < Attributes.Length; i++)
            {
                float y = 96f + i * 56f;
                AttributeTexts[i] = UiKit.Text("Attribute" + i, box, 16, UiKit.Ink, TextAnchor.MiddleLeft, false);
                PlaceTopLeft(AttributeTexts[i].rectTransform, 16f, y, AttributesWidth - 70f, 22f);
                AttributeEffects[i] = UiKit.Text("Effect" + i, box, 13, new Color(0.35f, 0.25f, 0.15f), TextAnchor.MiddleLeft, false);
                PlaceTopLeft(AttributeEffects[i].rectTransform, 16f, y + 22f, AttributesWidth - 70f, 18f);
                string attribute = Attributes[i];
                var plus = UiKit.Button("+", box, new Vector2(34f, 34f), () => SpendStat(attribute));
                PlaceTopLeft((RectTransform)plus.transform, AttributesWidth - 16f - 34f, y + 4f, 34f, 34f);
                AttributeButtons[i] = plus.gameObject;
            }
        }

        private static void BuildTabs()
        {
            float left = Margin + AttributesWidth + Margin;
            float width = (SkillTreeLayout.GridWidth - 2f * 6f) / 3f;
            for (int i = 0; i < TabLabels.Length; i++)
            {
                int tab = i;
                var back = UiKit.Image("Tab" + i, _root, SheetArt.Tile, Image.Type.Sliced, true);
                if (!back.sprite)
                {
                    back.color = new Color(0.1f, 0.08f, 0.06f, 0.9f);
                }
                PlaceTopLeft(back.rectTransform, left + i * (width + 6f), HeaderHeight + 8f, width, TabHeight);
                back.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                {
                    _tab = tab;
                    _lastCounter = -1;
                });
                TabBacks[i] = back;
                TabLabels[i] = UiKit.Text("Label", back.rectTransform, 14, UiKit.Gold, TextAnchor.MiddleCenter);
                UiKit.Stretch(TabLabels[i].rectTransform, 4f);
            }
        }

        private static void BuildSlots(float treeLeft)
        {
            float top = TreeTop + SkillTreeLayout.GridHeight + 16f;
            var label = UiKit.Text("SlotsLabel", _root, 15, UiKit.Gold, TextAnchor.MiddleLeft);
            UiKit.SetText(label, "Skill slots: drag skills here");
            PlaceTopLeft(label.rectTransform, Margin, top + 18f, AttributesWidth, 24f);
            float rowWidth = Slots.Length * SlotTile + (Slots.Length - 1) * 14f;
            float left = treeLeft + (SkillTreeLayout.GridWidth - rowWidth) / 2f;
            for (int i = 0; i < Slots.Length; i++)
            {
                var frame = UiKit.Image("Slot" + i, _root, SheetArt.Tile, Image.Type.Sliced, true);
                if (!frame.sprite)
                {
                    frame.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
                }
                PlaceTopLeft(frame.rectTransform, left + i * (SlotTile + 14f), top, SlotTile, SlotTile);
                var view = frame.gameObject.AddComponent<SlotTileView>();
                view.Index = i;
                view.Icon = UiKit.Image("Icon", frame.rectTransform, null);
                view.Icon.preserveAspect = true;
                UiKit.Stretch(view.Icon.rectTransform, 9f);
                view.Key = UiKit.Text("Key", frame.rectTransform, 13, Color.white, TextAnchor.UpperLeft);
                UiKit.Place(view.Key.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(7f, -4f), new Vector2(30f, 16f));
                view.Kind = UiKit.Text("Kind", frame.rectTransform, 12, UiKit.Gold, TextAnchor.UpperCenter);
                UiKit.Place(view.Kind.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(SlotTile + 30f, 16f));
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
            }
            string classInfo = ClassesSource.ClassInfo(classId);
            UiKit.SetText(_title, FlatJson.Get(classInfo, "name") ?? classId);
            for (int i = 0; i < TabLabels.Length; i++)
            {
                UiKit.SetText(TabLabels[i], FlatJson.Get(classInfo, "tab" + i) ?? "");
                TabBacks[i].color = i == _tab ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
                TabLabels[i].color = i == _tab ? UiKit.Gold : UiKit.Dim;
            }

            int skillPoints = FlatJson.GetInt(character, "skillPoints", 0);
            int statPoints = FlatJson.GetInt(character, "statPoints", 0);
            UiKit.SetText(_skillPoints, "Skill points: " + skillPoints);
            long xp = FlatJson.GetLong(character, "xp", 0);
            long next = FlatJson.GetLong(character, "xpNextLevel", 0);
            UiKit.SetText(_level, "Level " + FlatJson.GetInt(character, "level", 1) + "\nXP " + xp + (next > 0 ? " / " + next : ""));
            UiKit.SetText(_statPoints, "Stat points: " + statPoints);
            for (int i = 0; i < Attributes.Length; i++)
            {
                string info = ClassesSource.AttributeInfo(Attributes[i]);
                UiKit.SetText(AttributeTexts[i], (FlatJson.Get(info, "name") ?? Attributes[i]) + ": " + FlatJson.GetInt(info, "value", 0));
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
            BuildTree();

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
                UiKit.SetText(view.Kind, id.Length > 0 && ById.TryGetValue(id, out var skill) ? skill.Name : "");
            }
        }

        /// <summary>Rebuilds the shown tab's tiles and lines (a few dozen objects, only when something changed).</summary>
        private static void BuildTree()
        {
            foreach (var go in TreeObjects)
            {
                UnityEngine.Object.Destroy(go);
            }
            TreeObjects.Clear();

            var shown = SkillTreeLayout.Visible(Skills, _tab);
            foreach (var line in SkillTreeLayout.Lines(shown))
            {
                var image = UiKit.Image("Line", _linesRoot, null);
                bool learned = ById.TryGetValue(line.From, out var from) && from.Points > 0;
                image.color = learned ? UiKit.Gold : new Color(0.5f, 0.45f, 0.4f, 0.6f);
                var rt = image.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(line.Length, 3f);
                rt.anchoredPosition = new Vector2((line.X1 + line.X2) / 2f, -(line.Y1 + line.Y2) / 2f);
                rt.localRotation = Quaternion.Euler(0f, 0f, line.AngleDegrees);
                TreeObjects.Add(image.gameObject);
            }
            foreach (var skill in shown)
            {
                TreeObjects.Add(BuildTile(skill));
            }
        }

        private static GameObject BuildTile(SkillView skill)
        {
            SkillTreeLayout.TilePosition(skill.Row, skill.Col, out float x, out float y);
            float size = SkillTreeLayout.Tile;
            var frame = UiKit.Image("Skill_" + skill.Id, _tilesRoot, SheetArt.Tile, Image.Type.Sliced, true);
            if (!frame.sprite)
            {
                frame.color = new Color(0.08f, 0.07f, 0.06f, 0.9f);
            }
            PlaceTopLeft(frame.rectTransform, x, y, size, size);
            bool locked = skill.Later || (skill.Points == 0 && !skill.CanSpend);
            if (skill.Maxed)
            {
                frame.color = UiKit.Gold;
            }

            var icon = UiKit.Image("Icon", frame.rectTransform, ClassesSource.SkillIcon(skill.Id));
            icon.preserveAspect = true;
            icon.enabled = icon.sprite;
            icon.color = locked ? new Color(0.35f, 0.35f, 0.35f, 0.85f) : Color.white;
            UiKit.Stretch(icon.rectTransform, 8f);

            var points = UiKit.Text("Points", frame.rectTransform, 16, skill.CanSpend ? new Color(0.6f, 1f, 0.5f) : Color.white, TextAnchor.LowerRight);
            UiKit.Place(points.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-6f, 3f), new Vector2(40f, 20f));
            UiKit.SetText(points, skill.Later ? "" : skill.Points.ToString());

            var name = UiKit.Text("Name", frame.rectTransform, 12, skill.Later ? UiKit.Dim : UiKit.Gold, TextAnchor.UpperCenter);
            UiKit.Place(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -1f), new Vector2(size + SkillTreeLayout.ColGap - 4f, 16f));
            UiKit.SetText(name, skill.Later ? skill.Name + " (later)" : skill.Name);

            if (skill.CanSpend)
            {
                var plus = UiKit.Text("Plus", frame.rectTransform, 18, new Color(0.6f, 1f, 0.5f), TextAnchor.UpperRight);
                UiKit.Place(plus.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-5f, -1f), new Vector2(20f, 20f));
                UiKit.SetText(plus, "+");
            }

            var view = frame.gameObject.AddComponent<SkillTileView>();
            view.SkillId = skill.Id;
            return frame.gameObject;
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
            HideTooltip();
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

        // ------------------------------------------------------------ tooltip

        private static void ShowTooltip(string skillId, RectTransform tile)
        {
            if (!ById.TryGetValue(skillId, out var skill) || _dragGhost.gameObject.activeSelf)
            {
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=18><color=#E8CC96>").Append(skill.Name).Append("</color></size>\n");
            if (skill.Later)
            {
                sb.Append("<color=#999999>Coming later</color>\n");
            }
            else
            {
                sb.Append("Points: ").Append(skill.Points).Append(" / ").Append(skill.MaxPoints);
                if (skill.Level != skill.Points)
                {
                    sb.Append("   Level: ").Append(skill.Level);
                }
                sb.Append('\n');
            }
            sb.Append("<color=#BBBBBB>").Append(KindLabel(skill.Kind)).Append(", needs level ").Append(skill.ReqLevel);
            if (skill.EitrCost > 0f)
            {
                sb.Append(", ").Append(skill.EitrCost.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)).Append(" eitr");
            }
            sb.Append("</color>\n");
            if (!string.IsNullOrEmpty(skill.Description))
            {
                sb.Append('\n').Append(skill.Points > 0 ? "Now: " : "").Append(skill.Description).Append('\n');
            }
            if (!string.IsNullOrEmpty(skill.NextDescription) && !skill.Maxed && !skill.Later)
            {
                sb.Append("\n<color=#B0E89A>Next level: ").Append(skill.NextDescription).Append("</color>\n");
            }
            if (!skill.Later && !string.IsNullOrEmpty(skill.SpendBlocked))
            {
                sb.Append("\n<color=#FF9A80>").Append(skill.SpendBlocked).Append("</color>");
            }
            else if (skill.CanSpend)
            {
                sb.Append("\n<color=#B0E89A>Click to spend a point</color>");
            }
            if (!skill.Passive && !skill.Later)
            {
                sb.Append("\n<color=#BBBBBB>Drag onto a skill slot to use it</color>");
            }
            UiKit.SetText(_tooltipText, sb.ToString().TrimEnd('\n'));

            float height = _tooltipText.preferredHeight + 28f;
            _tooltip.sizeDelta = new Vector2(300f, height);
            // Beside the tile, in panel space (top-left anchored), kept inside the panel.
            Vector3 corner = _root.InverseTransformPoint(tile.TransformPoint(new Vector3(tile.rect.xMax, tile.rect.yMax, 0f)));
            float x = corner.x + PanelWidth / 2f + 8f;
            float y = corner.y - PanelHeight / 2f;
            x = Mathf.Min(x, PanelWidth - 300f - 8f);
            y = Mathf.Max(y, -PanelHeight + height + 8f);
            _tooltip.anchorMin = _tooltip.anchorMax = new Vector2(0f, 1f);
            _tooltip.anchoredPosition = new Vector2(x, y);
            _tooltip.gameObject.SetActive(true);
            _tooltip.SetAsLastSibling();
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

        private static void HideTooltip()
        {
            if (_tooltip)
            {
                _tooltip.gameObject.SetActive(false);
            }
        }

        // ------------------------------------------------------------ dragging

        private static string _dragSkill = "";
        private static int _dragFromSlot = -1;

        private static void BeginDrag(string skillId, int fromSlot, PointerEventData data)
        {
            HideTooltip();
            var icon = ClassesSource.SkillIcon(skillId);
            _dragSkill = skillId;
            _dragFromSlot = fromSlot;
            _dragGhost.sprite = icon;
            _dragGhost.color = icon ? new Color(1f, 1f, 1f, 0.8f) : new Color(1f, 0.8f, 0.4f, 0.6f);
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
                if (data.button == PointerEventData.InputButton.Left && !data.dragging)
                {
                    SpendSkill(SkillId);
                }
            }

            public void OnPointerEnter(PointerEventData data) => ShowTooltip(SkillId, (RectTransform)transform);

            public void OnPointerExit(PointerEventData data) => HideTooltip();

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

        private sealed class SlotTileView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public int Index;
            public string SkillId = "";
            public Image Icon;
            public Text Key;
            public Text Kind;

            public void OnPointerClick(PointerEventData data)
            {
                // Right-click empties the slot.
                if (data.button == PointerEventData.InputButton.Right && SkillId.Length > 0)
                {
                    SetSlot(Index, "");
                }
            }

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
                _skillIds = new string[0];
                TreeObjects.Clear();
            }
        }

        private static void PlaceTopLeft(RectTransform rt, float x, float y, float width, float height)
        {
            UiKit.Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -y), new Vector2(width, height));
        }
    }
}
