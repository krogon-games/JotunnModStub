using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>
    ///     A World of Warcraft style character sheet in the inventory screen, after the user's concept image
    ///     (2026-10-29): an iron-framed wood panel titled "Character", the equipment slots in framed, labelled tiles
    ///     down two columns round a live picture of the character in an arch, a parchment stats box with the name on a
    ///     ribbon, and the quick slots in a row at the bottom.
    ///
    ///     The slots themselves belong to EquipmentAndQuickSlots (EAQS): they are cells of hidden inventory rows that EAQS
    ///     moves into its own panel every frame (its postfix on <c>InventoryGrid.UpdateGui</c>). This runs after it and
    ///     moves the same cells onto our tiles, so dragging, equipping, tooltips, keep-on-death and the slots other mods
    ///     add (our talismans) keep working exactly as before. EAQS's own panel backgrounds and slot labels are hidden.
    ///     Which slots exist comes from EAQS's public API (<c>GetSlotIdsJson</c>, <c>GetSlotInfoJson</c>), read once a
    ///     second.
    ///
    ///     Without EAQS (or with Auga, which draws its own UI) nothing changes.
    /// </summary>
    internal static class CharacterSheetPanel
    {
        public const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";
        public const string AugaGuid = "randyknapp.mods.auga";

        private const float SlotRefreshInterval = 1f;
        private const float StatsRefreshInterval = 0.25f;
        private const int TitleFontSize = 30;
        private const int LabelFontSize = 15;
        private const int NameFontSize = 19;
        private const int StatsFontSize = 15;
        private const float StatsTop = 30f;
        private const float StatsRowHeight = 20f;
        private const int StatsRows = 6;
        private const float StatsPadding = 22f;
        private const float EffectIconSize = 18f;
        private const int MaxEffectName = 22;
        private const float SilhouetteSize = 54f;
        private const string SlotRootName = "EaqsSlotRoot";
        private static readonly string[] EaqsBackgroundNames = { "EaqsEquipmentBkg", "EaqsQuickSlotBkg", "EaqsCustomSlotBkg" };

        // Colours: labels and title in the game's parchment-gold, ink on the parchment, values in the game's own
        // meanings (health red, stamina yellow, eitr blue).
        private static readonly Color Gold = new Color(0.91f, 0.80f, 0.59f);
        private static readonly Color Ink = new Color(0.23f, 0.16f, 0.10f);
        private const string Good = "#2F6B1F";
        private const string Bad = "#9A1F1F";
        private const string Amber = "#7A5200";
        private const string HealthColour = "#A0261E";
        private const string StaminaColour = "#7A5E00";
        private const string EitrColour = "#2A4F9A";
        private static readonly Color SilhouetteTint = new Color(0.78f, 0.70f, 0.56f, 0.4f);

        private static bool _lookedUp;
        private static Func<string> _getSlotIdsJson;
        private static Func<string, string> _getSlotInfoJson;

        private static RectTransform _panel;
        private static SheetDragHandle _drag;
        private static RectTransform _title;
        private static Text _titleText;
        private static RectTransform _close;
        private static RectTransform _picture;
        private static RectTransform _parchment;
        private static RectTransform _ribbon;
        private static Text _name;
        private static Text[] _statRows;
        private static Text[] _effectRows;
        private static Image[] _effectIcons;
        private static readonly List<TileView> Tiles = new List<TileView>();

        // The active slots, where their cells are in the player grid and their mod's names (parallel arrays; the
        // plugin can't use tuples).
        private static SheetSlot[] _slots = new SheetSlot[0];
        private static int[] _gridX = new int[0];
        private static int[] _gridY = new int[0];
        private static string[] _names = new string[0];
        private static SheetPlan _plan;
        private static bool _planChanged;
        private static float _nextSlotRefresh;
        private static float _nextStatsRefresh;
        private static bool _chromeHidden;
        private static readonly StringBuilder Builder = new StringBuilder();
        private static readonly List<StatusEffect> Effects = new List<StatusEffect>();

        private class TileView
        {
            public RectTransform Root;
            public Image Silhouette;
            public bool SilhouetteResolved;
        }

        /// <summary>Where the sheet is while being dragged, otherwise the saved position.</summary>
        private static Vector2 Position => _drag && _drag.DragPosition.HasValue ? _drag.DragPosition.Value : ModConfig.SheetPosition.Value;

        private static bool Available
        {
            get
            {
                if (!_lookedUp)
                {
                    _lookedUp = true;
                    LookUpApi();
                }
                return _getSlotIdsJson != null;
            }
        }

        private static void LookUpApi()
        {
            if (!Chainloader.PluginInfos.TryGetValue(EaqsGuid, out var info) || !info.Instance)
            {
                Jotunn.Logger.LogInfo("EquipmentAndQuickSlots isn't installed: the character sheet is off.");
                return;
            }
            if (Chainloader.PluginInfos.ContainsKey(AugaGuid))
            {
                Jotunn.Logger.LogInfo("Auga is installed: the character sheet is off (Auga lays out the inventory itself).");
                return;
            }
            var api = info.Instance.GetType().Assembly.GetType("EquipmentAndQuickSlots.API");
            var ids = api?.GetMethod("GetSlotIdsJson", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
            var slotInfo = api?.GetMethod("GetSlotInfoJson", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (ids == null || slotInfo == null)
            {
                Jotunn.Logger.LogWarning("This EquipmentAndQuickSlots version has no slot API (GetSlotIdsJson/GetSlotInfoJson): the character sheet is off.");
                return;
            }
            _getSlotIdsJson = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), ids);
            _getSlotInfoJson = (Func<string, string>)Delegate.CreateDelegate(typeof(Func<string, string>), slotInfo);
        }

        private static bool Enabled => ModConfig.SheetEnabled.Value && Available && !GUIManager.IsHeadless();

        /// <summary>Runs after EAQS has placed its cells, every frame the inventory is open.</summary>
        private static void Layout(InventoryGrid grid)
        {
            var gui = InventoryGui.instance;
            if (!gui || !gui.m_player || grid != gui.m_playerGrid)
            {
                return;
            }
            if (!Enabled)
            {
                if (_panel)
                {
                    _panel.gameObject.SetActive(false);
                }
                if (_chromeHidden)
                {
                    // Switched off in the config while playing: give EAQS its own panel back.
                    SetEaqsChrome(gui.m_player, true);
                }
                return;
            }

            // EAQS creates this root the first time it lays its slots out; its cells are positioned inside it.
            var slotRoot = gui.m_player.Find(SlotRootName) as RectTransform;
            if (!slotRoot)
            {
                return;
            }

            if (Time.unscaledTime >= _nextSlotRefresh)
            {
                _nextSlotRefresh = Time.unscaledTime + SlotRefreshInterval;
                RefreshSlots();
                SetEaqsChrome(gui.m_player, false);
            }
            if (_plan == null)
            {
                return;
            }

            EnsurePanel(slotRoot);
            if (_planChanged)
            {
                _planChanged = false;
                ApplyPlan();
            }
            _panel.gameObject.SetActive(true);
            Vector2 origin = Position;
            _panel.anchoredPosition = origin;

            int width = grid.m_inventory != null ? grid.m_inventory.GetWidth() : 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                var element = width > 0 ? grid.GetElement(_gridX[i], _gridY[i], width) : null;
                if (element == null || !element.gameObject.activeSelf)
                {
                    continue;
                }
                var cell = element.transform as RectTransform;
                // Only cells EAQS has put in its root share the sheet's coordinate space; leave anything else alone.
                if (cell == null || cell.parent != slotRoot)
                {
                    continue;
                }
                SlotPlacement placement = _plan.Slots[i];
                Vector2 size = cell.rect.size;
                float x = placement.CellX + (SheetLayout.Cell - size.x) / 2f;
                float y = placement.CellY + (SheetLayout.Cell - size.y) / 2f;
                cell.anchoredPosition = origin + new Vector2(x + cell.pivot.x * size.x, -y - (1f - cell.pivot.y) * size.y);
                DressCell(element, i);
            }

            if (Time.unscaledTime >= _nextStatsRefresh)
            {
                _nextStatsRefresh = Time.unscaledTime + StatsRefreshInterval;
                UpdateStats(Player.m_localPlayer);
            }
        }

        /// <summary>
        ///     Labelled slots: our label replaces EAQS's (which it re-enables every frame, before us), the blue "equipped"
        ///     backing goes (everything in an equipment slot is equipped), and an empty slot shows what goes in it.
        /// </summary>
        private static void DressCell(InventoryElement element, int index)
        {
            if (!_plan.Slots[index].Labelled)
            {
                return;
            }
            var binding = element.transform.Find("binding");
            var label = binding ? binding.GetComponent<TMP_Text>() : null;
            if (label)
            {
                label.enabled = false;
            }
            if (_slots[index].Kind == SlotKind.Equipment && element.m_equiped)
            {
                element.m_equiped.enabled = false;
            }
            if (index < Tiles.Count && Tiles[index] != null)
            {
                var tile = Tiles[index];
                if (!tile.SilhouetteResolved)
                {
                    tile.SilhouetteResolved = ResolveSilhouette(tile, _slots[index].Id);
                }
                if (tile.Silhouette)
                {
                    tile.Silhouette.enabled = tile.Silhouette.sprite && !element.m_used;
                }
            }
        }

        /// <summary>The faint icon of a vanilla item that goes in this slot (see <see cref="SheetLayout.SilhouetteFor" />).</summary>
        private static bool ResolveSilhouette(TileView tile, string slotId)
        {
            string prefabName = SheetLayout.SilhouetteFor(slotId);
            if (prefabName == null || !tile.Silhouette)
            {
                return true;
            }
            if (!ObjectDB.instance)
            {
                return false;
            }
            var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
            var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
            if (drop != null && drop.m_itemData != null)
            {
                tile.Silhouette.sprite = drop.m_itemData.GetIcon();
            }
            return true;
        }

        /// <summary>Asks EAQS which slots exist and are active, and replans the sheet when that changed.</summary>
        private static void RefreshSlots()
        {
            string[] ids = FlatJson.Strings(_getSlotIdsJson());
            var slots = new List<SheetSlot>();
            var gridX = new List<int>();
            var gridY = new List<int>();
            var names = new List<string>();
            foreach (string id in ids)
            {
                string info = _getSlotInfoJson(id);
                if (info == null || !FlatJson.GetBool(info, "active", false))
                {
                    continue;
                }
                SlotKind kind;
                if (FlatJson.GetBool(info, "isQuickSlot", false))
                {
                    if (!ModConfig.SheetQuickSlots.Value)
                    {
                        continue;
                    }
                    kind = SlotKind.Quick;
                }
                else if (FlatJson.GetBool(info, "isEquipmentSlot", false))
                {
                    kind = SlotKind.Equipment;
                }
                else if (FlatJson.GetBool(info, "isCustomSlot", false))
                {
                    kind = SlotKind.Custom;
                }
                else
                {
                    continue;
                }
                int x = FlatJson.GetInt(info, "gridX", -1);
                int y = FlatJson.GetInt(info, "gridY", -1);
                if (x < 0 || y < 0)
                {
                    continue;
                }
                slots.Add(new SheetSlot(id, kind, FlatJson.GetInt(info, "index", 0)));
                gridX.Add(x);
                gridY.Add(y);
                // EAQS's nameToken is already localised.
                names.Add(FlatJson.Get(info, "nameToken") ?? id);
            }

            if (SameSlots(slots, gridX, gridY))
            {
                return;
            }
            _slots = slots.ToArray();
            _gridX = gridX.ToArray();
            _gridY = gridY.ToArray();
            _names = names.ToArray();
            SheetPlan plan = SheetLayout.Plan(_slots);
            // Keep the placements in the same order as the slots, so Layout can index both with one counter.
            var ordered = new SlotPlacement[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
            {
                foreach (var p in plan.Slots)
                {
                    if (p.Id == _slots[i].Id)
                    {
                        ordered[i] = p;
                        break;
                    }
                }
            }
            plan.Slots = ordered;
            _plan = plan;
            _planChanged = true;
        }

        private static bool SameSlots(List<SheetSlot> slots, List<int> gridX, List<int> gridY)
        {
            if (_plan == null || slots.Count != _slots.Length)
            {
                return false;
            }
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Id != _slots[i].Id || slots[i].Index != _slots[i].Index || gridX[i] != _gridX[i] || gridY[i] != _gridY[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Switches EAQS's panel backgrounds, its paperdoll and its drag handles off (or back on). Their objects stay
        ///     (EAQS keeps them active and sized every frame); only their images and scripts are disabled, which EAQS
        ///     never re-enables. Repeated once a second, so a background EAQS creates later is caught too.
        /// </summary>
        private static void SetEaqsChrome(Transform playerPanel, bool enabled)
        {
            _chromeHidden = !enabled;
            foreach (string name in EaqsBackgroundNames)
            {
                var background = playerPanel.Find(name);
                if (!background)
                {
                    continue;
                }
                foreach (var image in background.GetComponentsInChildren<Image>(true))
                {
                    image.enabled = enabled;
                }
                foreach (var script in background.GetComponents<MonoBehaviour>())
                {
                    if (script && script.GetType().Name == "PanelDragHandle")
                    {
                        script.enabled = enabled;
                    }
                }
            }
        }

        // ------------------------------------------------------------ building the panel

        private static void EnsurePanel(RectTransform slotRoot)
        {
            if (_panel)
            {
                if (_panel.parent != slotRoot)
                {
                    _panel.SetParent(slotRoot, false);
                    _panel.SetAsFirstSibling();
                }
                return;
            }

            var gm = GUIManager.Instance;
            _panel = NewRect("CraftingOverhaul_CharacterSheet", slotRoot);
            _panel.SetAsFirstSibling();
            var wood = _panel.gameObject.AddComponent<Image>();
            wood.sprite = SheetArt.Wood;
            wood.type = Image.Type.Tiled;
            wood.color = wood.sprite ? Color.white : new Color(0.22f, 0.14f, 0.09f);
            wood.raycastTarget = true;
            _drag = _panel.gameObject.AddComponent<SheetDragHandle>();

            var frame = NewImage("Frame", _panel, SheetArt.Frame, Image.Type.Sliced);
            Stretch(frame.rectTransform);

            var title = NewImage("Title", _panel, SheetArt.Title, Image.Type.Simple);
            _title = title.rectTransform;
            _titleText = NewText("Text", _title, gm.NorseBold, TitleFontSize, Gold, true, TextAnchor.MiddleCenter);
            _titleText.text = "CHARACTER";
            Stretch(_titleText.rectTransform);

            var close = NewImage("Close", _panel, SheetArt.Close, Image.Type.Simple);
            close.raycastTarget = true;
            if (!close.sprite)
            {
                close.color = new Color(0.2f, 0.18f, 0.16f);
            }
            _close = close.rectTransform;
            var button = close.gameObject.AddComponent<Button>();
            button.targetGraphic = close;
            button.onClick.AddListener(() =>
            {
                if (InventoryGui.instance)
                {
                    InventoryGui.instance.Hide();
                }
            });

            BuildPicture();
            BuildStats(gm);

            Jotunn.Logger.LogInfo($"Character sheet created with {_slots.Length} slots at {Position}");
        }

        /// <summary>The arch: its backdrop masks the live picture to the arch shape; the iron rim and pedestal go on top.</summary>
        private static void BuildPicture()
        {
            _picture = NewRect("Picture", _panel);
            var back = NewImage("Backdrop", _picture, SheetArt.ArchBack, Image.Type.Sliced);
            Stretch(back.rectTransform);
            if (!back.sprite)
            {
                back.color = new Color(0.10f, 0.14f, 0.19f);
            }
            var mask = back.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var liveGo = new GameObject("Live", typeof(RectTransform), typeof(RawImage));
            var live = liveGo.GetComponent<RawImage>();
            live.rectTransform.SetParent(back.rectTransform, false);
            Stretch(live.rectTransform);
            live.raycastTarget = true;
            CharacterPortrait.Attach(live);

            var rim = NewImage("Rim", _picture, SheetArt.ArchRim, Image.Type.Sliced);
            Stretch(rim.rectTransform);
        }

        private static void BuildStats(GUIManager gm)
        {
            var parchment = NewImage("Stats", _panel, SheetArt.Parchment, Image.Type.Simple);
            if (!parchment.sprite)
            {
                parchment.color = new Color(0.84f, 0.75f, 0.59f);
            }
            _parchment = parchment.rectTransform;

            _statRows = new Text[StatsRows];
            _effectRows = new Text[StatsRows];
            _effectIcons = new Image[StatsRows];
            float half = (SheetLayout.Width - 2 * SheetLayout.Margin) / 2f;
            for (int i = 0; i < StatsRows; i++)
            {
                float y = StatsTop + i * StatsRowHeight;
                _statRows[i] = NewText("Stat" + i, _parchment, gm.AveriaSerifBold, StatsFontSize, Ink, false, TextAnchor.MiddleLeft);
                Place(_statRows[i].rectTransform, new SheetRect(StatsPadding, y, half - StatsPadding, StatsRowHeight));

                _effectIcons[i] = NewImage("EffectIcon" + i, _parchment, null, Image.Type.Simple);
                _effectIcons[i].preserveAspect = true;
                Place(_effectIcons[i].rectTransform, new SheetRect(half + 6f, y + (StatsRowHeight - EffectIconSize) / 2f, EffectIconSize, EffectIconSize));
                _effectIcons[i].enabled = false;

                _effectRows[i] = NewText("Effect" + i, _parchment, gm.AveriaSerifBold, StatsFontSize, Ink, false, TextAnchor.MiddleLeft);
                Place(_effectRows[i].rectTransform, new SheetRect(half + 10f + EffectIconSize, y, half - 16f - EffectIconSize - StatsPadding, StatsRowHeight));
            }

            // The ribbon goes after the parchment so it lies across its top edge.
            var ribbon = NewImage("Ribbon", _panel, SheetArt.Ribbon, Image.Type.Simple);
            if (!ribbon.sprite)
            {
                ribbon.color = new Color(0.25f, 0.16f, 0.1f);
            }
            _ribbon = ribbon.rectTransform;
            _name = NewText("Name", _ribbon, gm.AveriaSerifBold, NameFontSize, Gold, true, TextAnchor.MiddleCenter);
            Stretch(_name.rectTransform);
        }

        /// <summary>Sizes the panel and its parts to the current plan and rebuilds the slot tiles.</summary>
        private static void ApplyPlan()
        {
            _panel.sizeDelta = new Vector2(_plan.Width, _plan.Height);
            Place(_title, _plan.Title);
            Place(_close, _plan.Close);
            Place(_picture, _plan.Picture);
            Place(_parchment, _plan.Stats);
            Place(_ribbon, _plan.Ribbon);
            _parchment.gameObject.SetActive(ModConfig.SheetStats.Value);
            _ribbon.gameObject.SetActive(ModConfig.SheetStats.Value);

            foreach (var tile in Tiles)
            {
                if (tile != null && tile.Root)
                {
                    UnityEngine.Object.Destroy(tile.Root.gameObject);
                }
            }
            Tiles.Clear();
            var gm = GUIManager.Instance;
            for (int i = 0; i < _plan.Slots.Length; i++)
            {
                SlotPlacement p = _plan.Slots[i];
                var frame = NewImage("Tile_" + p.Id, _panel, SheetArt.Tile, Image.Type.Sliced);
                if (!frame.sprite)
                {
                    frame.color = new Color(0.09f, 0.08f, 0.07f, 0.9f);
                }
                Place(frame.rectTransform, p.Tile);
                // Tiles sit under the picture and stats in the panel's order, but never overlap them.
                frame.rectTransform.SetSiblingIndex(1);
                var view = new TileView { Root = frame.rectTransform };
                if (p.Labelled)
                {
                    view.Silhouette = NewImage("Silhouette", frame.rectTransform, null, Image.Type.Simple);
                    view.Silhouette.preserveAspect = true;
                    view.Silhouette.color = SilhouetteTint;
                    view.Silhouette.enabled = false;
                    float inset = (SheetLayout.Cell - SilhouetteSize) / 2f;
                    Place(view.Silhouette.rectTransform, new SheetRect(p.CellX - p.Tile.X + inset, p.CellY - p.Tile.Y + inset, SilhouetteSize, SilhouetteSize));

                    var label = NewText("Label", frame.rectTransform, gm.NorseBold, LabelFontSize, Gold, true, TextAnchor.MiddleCenter);
                    label.text = SheetLayout.LabelFor(_slots[i].Id, _names[i]);
                    // Long names (other mods' slots) shrink to fit the tile.
                    label.horizontalOverflow = HorizontalWrapMode.Wrap;
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = 9;
                    label.resizeTextMaxSize = LabelFontSize;
                    float labelTop = p.CellY - p.Tile.Y + SheetLayout.Cell;
                    Place(label.rectTransform, new SheetRect(0f, labelTop, p.Tile.Width, p.Tile.Height - labelTop - 3f));
                }
                Tiles.Add(view);
            }
        }

        // ------------------------------------------------------------ stats

        private static void UpdateStats(Player player)
        {
            if (!_parchment || !player)
            {
                return;
            }
            bool show = ModConfig.SheetStats.Value;
            if (_parchment.gameObject.activeSelf != show)
            {
                _parchment.gameObject.SetActive(show);
                _ribbon.gameObject.SetActive(show);
            }
            if (!show)
            {
                return;
            }

            var profile = Game.instance ? Game.instance.GetPlayerProfile() : null;
            SetText(_name, profile != null ? profile.GetName() : player.GetPlayerName());

            int row = 0;
            SetText(_statRows[row++], Stat("Armor", Mathf.RoundToInt(player.GetBodyArmor()).ToString(), Good));
            int weight = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
            int maxWeight = Mathf.CeilToInt(player.GetMaxCarryWeight());
            SetText(_statRows[row++], Stat("Weight", weight + "/" + maxWeight, weight > maxWeight ? Bad : Amber));
            SetText(_statRows[row++], Stat("Health", Mathf.RoundToInt(player.GetMaxHealth()).ToString(), HealthColour));
            SetText(_statRows[row++], Stat("Stamina", Mathf.RoundToInt(player.GetMaxStamina()).ToString(), StaminaColour));
            float eitr = player.GetMaxEitr();
            if (eitr > 0f)
            {
                SetText(_statRows[row++], Stat("Eitr", Mathf.RoundToInt(eitr).ToString(), EitrColour));
            }
            int speed = Mathf.RoundToInt(player.GetEquipmentMovementModifier() * 100f);
            SetText(_statRows[row++], Stat("Speed from gear", (speed > 0 ? "+" : "") + speed + "%", speed < 0 ? Bad : speed > 0 ? Good : null));
            while (row < StatsRows)
            {
                SetText(_statRows[row++], "");
            }

            Effects.Clear();
            var seman = player.GetSEMan();
            if (seman != null)
            {
                foreach (var effect in seman.GetStatusEffects())
                {
                    if (effect != null && !effect.m_hidden && !string.IsNullOrEmpty(effect.m_name))
                    {
                        Effects.Add(effect);
                    }
                }
            }
            for (int i = 0; i < StatsRows; i++)
            {
                bool more = i == StatsRows - 1 && Effects.Count > StatsRows;
                if (i < Effects.Count && !more)
                {
                    var effect = Effects[i];
                    SetText(_effectRows[i], Shorten(Localization.instance.Localize(effect.m_name)));
                    _effectIcons[i].sprite = effect.m_icon;
                    _effectIcons[i].enabled = effect.m_icon;
                }
                else
                {
                    SetText(_effectRows[i], more ? "+" + (Effects.Count - StatsRows + 1) + " more" : i == 0 ? "No effects" : "");
                    _effectIcons[i].enabled = false;
                }
            }
        }

        /// <summary>Keeps an effect's name on its row (some mods' effects have long names).</summary>
        private static string Shorten(string name)
        {
            return name.Length <= MaxEffectName ? name : name.Substring(0, MaxEffectName - 1).TrimEnd() + "\u2026";
        }

        private static string Stat(string label, string value, string colour)
        {
            var sb = Builder;
            sb.Length = 0;
            sb.Append(label).Append(": ");
            if (colour != null)
            {
                sb.Append("<color=").Append(colour).Append('>').Append(value).Append("</color>");
            }
            else
            {
                sb.Append(value);
            }
            return sb.ToString();
        }

        private static void SetText(Text text, string value)
        {
            if (text && text.text != value)
            {
                text.text = value;
            }
        }

        // ------------------------------------------------------------ UI helpers

        private static RectTransform NewRect(string name, Transform parent)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            TopLeft(rt);
            return rt;
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite, Image.Type type)
        {
            var image = NewRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite ? type : Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        private static Text NewText(string name, Transform parent, Font font, int size, Color colour, bool outline, TextAnchor alignment)
        {
            var gm = GUIManager.Instance;
            var go = gm.CreateText("", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero,
                font ? font : gm.AveriaSerifBold, size, colour, outline, Color.black, 100f, 20f, false);
            go.name = name;
            var text = go.GetComponent<Text>();
            text.alignment = alignment;
            text.supportRichText = true;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            TopLeft(text.rectTransform);
            return text;
        }

        private static void TopLeft(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rt, SheetRect r)
        {
            rt.anchoredPosition = new Vector2(r.X, -r.Y);
            rt.sizeDelta = new Vector2(r.Width, r.Height);
        }

        private static void Clear()
        {
            _panel = null;
            _drag = null;
            _title = null;
            _titleText = null;
            _close = null;
            _picture = null;
            _parchment = null;
            _ribbon = null;
            _name = null;
            _statRows = null;
            _effectRows = null;
            _effectIcons = null;
            Tiles.Clear();
            _plan = null;
            _slots = new SheetSlot[0];
            _gridX = new int[0];
            _gridY = new int[0];
            _names = new string[0];
            _nextSlotRefresh = 0f;
            _nextStatsRefresh = 0f;
            _chromeHidden = false;
        }

        /// <summary>Moves the sheet by dragging its wood; the position is saved to the config when the drag ends.</summary>
        private class SheetDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            private bool _dragging;
            private float _scale = 1f;

            public Vector2? DragPosition { get; private set; }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                {
                    return;
                }
                _dragging = true;
                DragPosition = ModConfig.SheetPosition.Value;
                var canvas = GetComponentInParent<Canvas>();
                _scale = canvas && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (_dragging && DragPosition.HasValue)
                {
                    DragPosition = DragPosition.Value + eventData.delta / _scale;
                }
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (!_dragging)
                {
                    return;
                }
                _dragging = false;
                if (DragPosition.HasValue)
                {
                    ModConfig.SheetPosition.Value = DragPosition.Value;
                }
                DragPosition = null;
            }

            private void OnDisable()
            {
                if (_dragging)
                {
                    OnEndDrag(null);
                }
            }
        }

        /// <summary>After EAQS has positioned its slot cells (its own postfix on the same method), move them onto the sheet.</summary>
        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
        [HarmonyAfter(EaqsGuid)]
        [HarmonyPriority(Priority.Low)]
        private static class UpdateGui_Patch
        {
            private static void Postfix(InventoryGrid __instance)
            {
                // No closure: this runs every frame the inventory is open.
                try
                {
                    Layout(__instance);
                }
                catch (Exception ex)
                {
                    Safe.Report("lay out the character sheet", ex);
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDestroy))]
        private static class OnDestroy_Patch
        {
            private static void Postfix()
            {
                Clear();
            }
        }
    }
}
