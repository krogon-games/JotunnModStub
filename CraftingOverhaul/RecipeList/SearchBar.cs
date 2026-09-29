using System;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.RecipeList
{
    /// <summary>
    ///     Adds a search box and a grouping toggle above the crafting recipe list. The list is shrunk
    ///     from the top to make room, so the bar lines up with the list at any resolution.
    /// </summary>
    internal class SearchBar : MonoBehaviour
    {
        private const float BarHeight = 32f;
        private const float Gap = 4f;
        private const float ButtonWidth = 110f;

        private static SearchBar _instance;

        private InputField _input;
        private Text _groupingLabel;
        private bool _blockingInput;

        private static void EnsureCreated(InventoryGui gui)
        {
            if (_instance || !ModConfig.ShowSearchBar.Value || GUIManager.IsHeadless())
            {
                return;
            }

            var scrollRect = gui.m_recipeListRoot.GetComponentInParent<ScrollRect>();
            if (!scrollRect)
            {
                Jotunn.Logger.LogWarning("Could not find the recipe list scroll view; search bar disabled.");
                return;
            }

            var listRect = (RectTransform)scrollRect.transform;
            var parent = (RectTransform)listRect.parent;

            // Top band of the list's current area, in the parent's local space.
            Rect r = listRect.rect;
            Vector2 topLeft = (Vector2)listRect.localPosition + new Vector2(r.xMin, r.yMax);
            float width = r.width;

            // Shrink the list (and its scrollbar, if it sits outside the list) to make room.
            listRect.offsetMax -= new Vector2(0f, BarHeight + Gap);
            var scrollbar = gui.m_recipeListScroll ? (RectTransform)gui.m_recipeListScroll.transform : null;
            if (scrollbar && !scrollbar.IsChildOf(listRect) && scrollbar.parent == parent)
            {
                scrollbar.offsetMax -= new Vector2(0f, BarHeight + Gap);
            }
            gui.m_recipeListBaseSize = Mathf.Max(0f, gui.m_recipeListBaseSize - (BarHeight + Gap));

            var inputGo = GUIManager.Instance.CreateInputField(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, InputField.ContentType.Standard, "Search recipes...", 16, width - ButtonWidth - Gap, BarHeight);
            inputGo.name = "CraftingOverhaul_Search";
            Place(parent, (RectTransform)inputGo.transform, topLeft, width - ButtonWidth - Gap);

            var buttonGo = GUIManager.Instance.CreateButton("", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, ButtonWidth, BarHeight);
            buttonGo.name = "CraftingOverhaul_Grouping";
            Place(parent, (RectTransform)buttonGo.transform, topLeft + new Vector2(width - ButtonWidth, 0f), ButtonWidth);

            _instance = inputGo.AddComponent<SearchBar>();
            _instance._input = inputGo.GetComponent<InputField>();
            _instance._input.text = RecipeListOrganizer.SearchText;
            _instance._input.onValueChanged.AddListener(OnSearchChanged);

            _instance._groupingLabel = buttonGo.GetComponentInChildren<Text>();
            buttonGo.GetComponent<Button>().onClick.AddListener(CycleGrouping);
            UpdateGroupingLabel();
        }

        private static void Place(RectTransform parent, RectTransform rt, Vector2 topLeftInParent, float width)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(width, BarHeight);
            // Anchors at the parent's centre: anchoredPosition is relative to parent.rect.center.
            rt.anchoredPosition = topLeftInParent - parent.rect.center;
        }

        private static void OnSearchChanged(string text)
        {
            RecipeListOrganizer.SearchText = text;
            RecipeListOrganizer.Refresh();
        }

        private static void CycleGrouping()
        {
            var values = (GroupingMode[])Enum.GetValues(typeof(GroupingMode));
            int next = (Array.IndexOf(values, ModConfig.Grouping.Value) + 1) % values.Length;
            // Triggers SettingChanged, which refreshes the list.
            ModConfig.Grouping.Value = values[next];
            UpdateGroupingLabel();
        }

        private static void UpdateGroupingLabel()
        {
            if (!_instance || !_instance._groupingLabel)
            {
                return;
            }
            switch (ModConfig.Grouping.Value)
            {
                case GroupingMode.ItemType: _instance._groupingLabel.text = "By type"; break;
                case GroupingMode.Biome: _instance._groupingLabel.text = "By biome"; break;
                default: _instance._groupingLabel.text = "No groups"; break;
            }
        }

        private static void ClearSearch()
        {
            RecipeListOrganizer.SearchText = "";
            if (_instance && _instance._input)
            {
                _instance._input.SetTextWithoutNotify("");
                _instance._input.DeactivateInputField();
            }
        }

        private void Update()
        {
            // While typing, stop keys from moving the player, closing the inventory or triggering hotkeys.
            bool focused = _input && _input.isFocused;
            if (focused != _blockingInput)
            {
                _blockingInput = focused;
                GUIManager.BlockInput(focused);
            }
        }

        private void OnDisable()
        {
            if (_blockingInput)
            {
                _blockingInput = false;
                GUIManager.BlockInput(false);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
        private static class Show_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                EnsureCreated(__instance);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
        private static class Hide_Patch
        {
            private static void Postfix()
            {
                if (ModConfig.ClearSearchOnClose.Value)
                {
                    ClearSearch();
                }
            }
        }
    }
}
