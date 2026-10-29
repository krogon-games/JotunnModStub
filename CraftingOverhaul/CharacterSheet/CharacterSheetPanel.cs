using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CraftingOverhaul.CharacterSheet
{
    /// <summary>
    ///     A World of Warcraft style character sheet in the inventory screen: the equipment slots down two columns round
    ///     a character frame, a stats box underneath and the quick slots in a row at the bottom.
    ///
    ///     The slots themselves belong to EquipmentAndQuickSlots (EAQS): they are cells of hidden inventory rows that EAQS
    ///     moves into its own panel every frame (its postfix on <c>InventoryGrid.UpdateGui</c>). This runs after it and
    ///     moves the same cells to where <see cref="SheetLayout" /> puts them, so dragging, equipping, tooltips,
    ///     keep-on-death and the slots other mods add (our talismans) keep working exactly as before. EAQS's own panel
    ///     backgrounds are hidden. Which slots exist comes from EAQS's public API (<c>GetSlotIdsJson</c>,
    ///     <c>GetSlotInfoJson</c>), read once a second.
    ///
    ///     Without EAQS (or with Auga, which draws its own UI) nothing changes.
    /// </summary>
    internal static class CharacterSheetPanel
    {
        public const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";
        public const string AugaGuid = "randyknapp.mods.auga";

        private const float SlotRefreshInterval = 1f;
        private const float StatsRefreshInterval = 0.25f;
        private const int TitleFontSize = 24;
        private const int StatsFontSize = 16;
        private const string SlotRootName = "EaqsSlotRoot";
        private static readonly string[] EaqsBackgroundNames = { "EaqsEquipmentBkg", "EaqsQuickSlotBkg", "EaqsCustomSlotBkg" };

        private static bool _lookedUp;
        private static Func<string> _getSlotIdsJson;
        private static Func<string, string> _getSlotInfoJson;

        private static RectTransform _panel;
        private static Text _title;
        private static Text _stats;
        private static RectTransform _picture;
        private static SheetDragHandle _drag;

        // The active slots and where their cells are in the player grid (parallel arrays; the plugin can't use tuples).
        private static SheetSlot[] _slots = new SheetSlot[0];
        private static int[] _gridX = new int[0];
        private static int[] _gridY = new int[0];
        private static SheetPlan _plan;
        private static float _nextSlotRefresh;
        private static float _nextStatsRefresh;
        private static bool _chromeHidden;
        private static readonly StringBuilder Builder = new StringBuilder();

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
                cell.anchoredPosition = origin + new Vector2(placement.X, -placement.Y);
            }

            if (Time.unscaledTime >= _nextStatsRefresh)
            {
                _nextStatsRefresh = Time.unscaledTime + StatsRefreshInterval;
                UpdateStats(Player.m_localPlayer);
            }
        }

        /// <summary>Asks EAQS which slots exist and are active, and replans the sheet when that changed.</summary>
        private static void RefreshSlots()
        {
            string[] ids = FlatJson.Strings(_getSlotIdsJson());
            var slots = new List<SheetSlot>();
            var gridX = new List<int>();
            var gridY = new List<int>();
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
            }

            if (SameSlots(slots, gridX, gridY))
            {
                return;
            }
            _slots = slots.ToArray();
            _gridX = gridX.ToArray();
            _gridY = gridY.ToArray();
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
            if (_panel)
            {
                ApplyPlan();
            }
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
            var panelGo = gm.CreateWoodpanel(slotRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, 100f, 100f, false);
            panelGo.name = "CraftingOverhaul_CharacterSheet";
            _panel = (RectTransform)panelGo.transform;
            _panel.pivot = new Vector2(0f, 1f);
            _panel.SetAsFirstSibling();
            _drag = panelGo.AddComponent<SheetDragHandle>();

            var titleGo = gm.CreateText("Character", _panel, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero,
                gm.AveriaSerifBold, TitleFontSize, gm.ValheimOrange, true, Color.black, 100f, SheetLayout.TitleHeight, false);
            titleGo.name = "Title";
            _title = titleGo.GetComponent<Text>();
            _title.alignment = TextAnchor.MiddleCenter;
            _title.raycastTarget = false;
            TopLeft((RectTransform)titleGo.transform);

            var pictureGo = new GameObject("Picture", typeof(RectTransform), typeof(Image));
            _picture = (RectTransform)pictureGo.transform;
            _picture.SetParent(_panel, false);
            TopLeft(_picture);
            var inset = pictureGo.GetComponent<Image>();
            inset.color = new Color(0f, 0f, 0f, 0.35f);
            inset.raycastTarget = false;

            var statsGo = gm.CreateText("", _panel, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero,
                gm.AveriaSerifBold, StatsFontSize, gm.ValheimBeige, true, Color.black, 100f, SheetLayout.StatsHeight, false);
            statsGo.name = "Stats";
            _stats = statsGo.GetComponent<Text>();
            _stats.alignment = TextAnchor.UpperLeft;
            _stats.supportRichText = true;
            _stats.raycastTarget = false;
            TopLeft((RectTransform)statsGo.transform);

            ApplyPlan();
            Jotunn.Logger.LogInfo($"Character sheet created with {_slots.Length} slots ({_plan.Width}x{_plan.Height}) at {Position}");
        }

        private static void TopLeft(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
        }

        private static void Place(RectTransform rt, SheetRect r)
        {
            rt.anchoredPosition = new Vector2(r.X, -r.Y);
            rt.sizeDelta = new Vector2(r.Width, r.Height);
        }

        /// <summary>Sizes the panel and its parts to the current plan.</summary>
        private static void ApplyPlan()
        {
            _panel.sizeDelta = new Vector2(_plan.Width, _plan.Height);
            Place((RectTransform)_title.transform, _plan.Title);
            Place(_picture, _plan.Picture);
            Place((RectTransform)_stats.transform, _plan.Stats);
            _stats.gameObject.SetActive(ModConfig.SheetStats.Value);
        }

        private static void UpdateStats(Player player)
        {
            if (!_stats || !player)
            {
                return;
            }
            if (!ModConfig.SheetStats.Value)
            {
                _stats.gameObject.SetActive(false);
                return;
            }
            _stats.gameObject.SetActive(true);

            var sb = Builder;
            sb.Length = 0;
            var profile = Game.instance ? Game.instance.GetPlayerProfile() : null;
            sb.Append("<color=#FFA13C>").Append(profile != null ? profile.GetName() : player.GetPlayerName()).Append("</color>\n");

            int weight = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
            int maxWeight = Mathf.CeilToInt(player.GetMaxCarryWeight());
            sb.Append("Armor: ").Append(Mathf.RoundToInt(player.GetBodyArmor()));
            sb.Append("    Weight: ");
            if (weight > maxWeight)
            {
                sb.Append("<color=red>").Append(weight).Append("</color>");
            }
            else
            {
                sb.Append(weight);
            }
            sb.Append('/').Append(maxWeight).Append('\n');

            sb.Append("Health: ").Append(Mathf.RoundToInt(player.GetMaxHealth()));
            sb.Append("    Stamina: ").Append(Mathf.RoundToInt(player.GetMaxStamina()));
            float eitr = player.GetMaxEitr();
            if (eitr > 0f)
            {
                sb.Append("    Eitr: ").Append(Mathf.RoundToInt(eitr));
            }
            sb.Append('\n');

            int speed = Mathf.RoundToInt(player.GetEquipmentMovementModifier() * 100f);
            sb.Append("Speed from gear: ");
            if (speed < 0)
            {
                sb.Append("<color=red>").Append(speed).Append("%</color>");
            }
            else if (speed > 0)
            {
                sb.Append("<color=green>+").Append(speed).Append("%</color>");
            }
            else
            {
                sb.Append("0%");
            }
            sb.Append('\n');

            sb.Append("Effects: ");
            bool any = false;
            var seman = player.GetSEMan();
            if (seman != null)
            {
                foreach (var effect in seman.GetStatusEffects())
                {
                    if (effect == null || effect.m_hidden)
                    {
                        continue;
                    }
                    if (any)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(Localization.instance.Localize(effect.m_name));
                    any = true;
                }
            }
            if (!any)
            {
                sb.Append("none");
            }

            string text = sb.ToString();
            if (_stats.text != text)
            {
                _stats.text = text;
            }
        }

        private static void Clear()
        {
            _panel = null;
            _title = null;
            _stats = null;
            _picture = null;
            _drag = null;
            _plan = null;
            _slots = new SheetSlot[0];
            _gridX = new int[0];
            _gridY = new int[0];
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
