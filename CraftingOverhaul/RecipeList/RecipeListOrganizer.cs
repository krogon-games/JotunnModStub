using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CraftingOverhaul.RecipeList
{
    /// <summary>
    ///     Filters the vanilla recipe list by the search text and splits it into collapsible categories.
    ///     Runs after the game builds the list and before it restores the selected recipe, so the game's own
    ///     selection, gamepad navigation and crafting logic keep working on the filtered list.
    /// </summary>
    internal static class RecipeListOrganizer
    {
        private static readonly Color HeaderColor = new Color(1f, 0.78f, 0.36f, 1f);

        private static readonly List<GameObject> Headers = new List<GameObject>();
        private static readonly HashSet<string> Collapsed = new HashSet<string>();
        private static readonly HashSet<string> Seen = new HashSet<string>();

        public static string SearchText { get; set; } = "";

        /// <summary>Rebuilds the recipe list if the crafting panel is open.</summary>
        public static void Refresh()
        {
            var gui = InventoryGui.instance;
            if (gui && Player.m_localPlayer && InventoryGui.IsVisible() && gui.m_crafting.gameObject.activeInHierarchy)
            {
                gui.UpdateCraftingPanel();
            }
        }

        private static void Reorganize(InventoryGui gui)
        {
            ClearHeaders();

            var recipes = gui.m_availableRecipes;
            string query = (SearchText ?? "").Trim();
            bool searching = query.Length > 0;
            var mode = ModConfig.Grouping.Value;

            var visible = new List<InventoryGui.RecipeDataPair>(recipes.Count);
            foreach (var entry in recipes)
            {
                if (!searching || Matches(entry.Recipe, query, mode))
                {
                    visible.Add(entry);
                }
                else
                {
                    Object.Destroy(entry.InterfaceElement);
                }
            }

            recipes.Clear();
            int row = 0;

            if (mode == GroupingMode.None)
            {
                foreach (var entry in visible)
                {
                    PlaceRow(gui, entry.InterfaceElement, row++);
                    recipes.Add(entry);
                }
            }
            else
            {
                // GroupBy keeps the game's sort order inside each group.
                var groups = visible
                    .GroupBy(e => RecipeCategories.Get(e.Recipe, mode))
                    .OrderBy(g => g.Key.Order);

                foreach (var group in groups)
                {
                    var category = group.Key;
                    if (Seen.Add(category.Key) && ModConfig.StartCollapsed.Value)
                    {
                        Collapsed.Add(category.Key);
                    }

                    // While searching, show every match regardless of collapsed state.
                    bool collapsed = !searching && Collapsed.Contains(category.Key);
                    int craftable = group.Count(e => e.CanCraft);
                    CreateHeader(gui, category, collapsed, craftable, group.Count(), row++);

                    foreach (var entry in group)
                    {
                        if (collapsed)
                        {
                            Object.Destroy(entry.InterfaceElement);
                            continue;
                        }
                        PlaceRow(gui, entry.InterfaceElement, row++);
                        recipes.Add(entry);
                    }
                }
            }

            float height = Mathf.Max(gui.m_recipeListBaseSize, row * gui.m_recipeListSpace);
            gui.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private static bool Matches(Recipe recipe, string query, GroupingMode mode)
        {
            var shared = recipe.m_item.m_itemData.m_shared;
            if (Contains(Localization.instance.Localize(shared.m_name), query))
            {
                return true;
            }
            if (mode != GroupingMode.None && Contains(RecipeCategories.Get(recipe, mode).Label, query))
            {
                return true;
            }
            if (ModConfig.SearchIngredients.Value)
            {
                foreach (var req in recipe.m_resources)
                {
                    if (req.m_resItem && Contains(Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name), query))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool Contains(string text, string query)
        {
            return text != null && text.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void PlaceRow(InventoryGui gui, GameObject element, int row)
        {
            ((RectTransform)element.transform).anchoredPosition = new Vector2(0f, -row * gui.m_recipeListSpace);
        }

        private static void CreateHeader(InventoryGui gui, RecipeCategory category, bool collapsed, int craftable, int total, int row)
        {
            // Reuse the game's recipe row so headers match the list's look and spacing.
            var header = Object.Instantiate(gui.m_recipeElementPrefab, gui.m_recipeListRoot);
            header.name = "CraftingOverhaul_CategoryHeader";
            header.SetActive(true);
            PlaceRow(gui, header, row);

            SetChildActive(header, "icon", false);
            SetChildActive(header, "Durability", false);
            SetChildActive(header, "QualityLevel", false);
            SetChildActive(header, "selected", false);

            var name = header.transform.Find("name")?.GetComponent<TMP_Text>();
            if (name)
            {
                name.text = $"{(collapsed ? "[+]" : "[-]")} {category.Label}  <size=80%>({craftable}/{total})</size>";
                name.color = HeaderColor;
                name.fontStyle = FontStyles.Bold;
            }

            var button = header.GetComponent<Button>();
            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Toggle(category.Key));
            }

            Headers.Add(header);
        }

        private static void Toggle(string key)
        {
            if (!Collapsed.Remove(key))
            {
                Collapsed.Add(key);
            }
            Refresh();
        }

        private static void SetChildActive(GameObject go, string child, bool active)
        {
            var t = go.transform.Find(child);
            if (t)
            {
                t.gameObject.SetActive(active);
            }
        }

        private static void ClearHeaders()
        {
            foreach (var header in Headers)
            {
                if (header)
                {
                    Object.Destroy(header);
                }
            }
            Headers.Clear();
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipeList))]
        private static class UpdateRecipeList_Patch
        {
            private static void Postfix(InventoryGui __instance)
            {
                Reorganize(__instance);
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
        private static class InventoryGuiAwake_Patch
        {
            private static void Postfix()
            {
                // Recipes and UI objects are recreated with each world load.
                Headers.Clear();
                RecipeCategories.ClearCache();
            }
        }
    }
}
