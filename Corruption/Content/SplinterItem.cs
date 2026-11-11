using System.Collections.Generic;
using System.Linq;
using Corruption.Rules;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The Affix Splinter (corruption-design.md → Affix Splinters and shattering; issue #29): one item whose affix
    ///     line and exact value live in its custom data (<see cref="SplinterRules.LineKey" />), so it is saved, traded
    ///     and stored like any item. Stack size 1: two splinters never merge. It goes to the Splinters tab by its name.
    ///     Its look (Krogon approved, 2026-11-10): a shape per kind of affix and a colour per stat, from
    ///     <c>splinter_looks.csv</c> and the models of <c>tools/make_splinters.py</c>. Each affix family is its own item
    ///     (<see cref="SplinterLooks.PrefabFor" />), because an item's model and icon belong to its prefab. The plain
    ///     <c>CorruptionAffixSplinter</c> stays registered for a line whose family has no look (a tinted gem).
    /// </summary>
    internal static class SplinterItem
    {
        // The orbs' base item: its material is the one the custom gem models are known to work with.
        private const string BasePrefab = "GemstoneRed";

        // The shard's weight, as the orbs use (no number of its own in the design; the Splinters tab is weightless).
        private const float Weight = 0.2f;

        private static readonly Color PlaceholderColor = new Color(0.55f, 0.85f, 0.75f);

        public const string NameToken = "$item_corruptionaffixsplinter";

        private static SplinterLooks _looks;
        private static int _models, _icons, _registered;

        public static void Register()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "item_corruptionaffixsplinter", "Affix Splinter" },
                {
                    "item_corruptionaffixsplinter_description",
                    "One affix from a shattered item, with its exact value. Put it on a Normal or Magic item at a Riftforge."
                }
            });
            // The plain splinter first: it must exist even if every look fails.
            RegisterOne(ShatterRules.SplinterPrefab, null);

            var errors = new List<string>();
            var looks = SplinterLooks.Parse(DataFiles.Read("splinter_looks.csv"), errors, Loot.Affixes.Values);
            if (errors.Count > 0)
            {
                // A family without a look still works: its splinters use the plain item.
                Jotunn.Logger.LogWarning("splinter_looks.csv has problems (splinters without a look use the plain one):\n" + string.Join("\n", errors));
            }
            _looks = looks;
            foreach (var look in looks.Looks)
            {
                Safe.Run($"register the {look.Prefab}", () => RegisterOne(look.Prefab, look));
            }
            // One line that says whether the looks made it into the game (play-test 2026-11-10: every splinter looked alike).
            Jotunn.Logger.LogInfo($"Affix Splinters: {looks.Looks.Count} looks in the table, {_registered} items registered, " +
                                  $"{_models} with their own model, {_icons} icons rendered.");
        }

        private static void RegisterOne(string prefabName, SplinterLooks.Look look)
        {
            var item = new CustomItem(prefabName, BasePrefab, new ItemConfig
            {
                Name = NameToken,
                Description = NameToken + "_description",
                StackSize = 1,
                Weight = Weight
            });
            var colour = look != null ? new Color(look.R, look.G, look.B) : PlaceholderColor;
            bool custom = false;
            if (look != null)
            {
                try
                {
                    custom = GemModel.Apply(item.ItemPrefab, new GemModel.Look
                    {
                        Name = prefabName,
                        Mesh = $"splinter_{look.Kind}.obj",
                        Albedo = prefabName + "_albedo.rgba",
                        Emission = $"splinter_{look.Kind}_emission.rgba"
                    }, colour);
                }
                catch (System.Exception ex)
                {
                    Jotunn.Logger.LogError($"Custom model for {prefabName} failed; using a tinted gem instead. {ex}");
                }
            }
            if (!custom)
            {
                Corrupted.Tint(item.ItemPrefab, colour, 0.6f);
            }
            else
            {
                _models++;
            }
            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_maxStackSize = 1;
            // Splinters go through portals and can be traded, like orbs (corruption-design.md → Affix Splinters).
            shared.m_teleportable = true;
            Safe.Run("render the Affix Splinter icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                    _icons++;
                }
                else
                {
                    Jotunn.Logger.LogWarning($"No icon could be rendered for {prefabName}; it keeps the base gem's icon.");
                }
            });
            // After the icon render, so the light doesn't colour the icon.
            Safe.Run($"add the {prefabName} glow", () => GemModel.AddGlow(item.ItemPrefab, colour));
            ItemManager.Instance.AddItem(item);
            _registered++;
        }

        /// <summary>The item a splinter holding this line is: its family's look, or the plain splinter.</summary>
        private static string PrefabFor(LootItem.Line line)
        {
            if (_looks != null && line != null && Loot.Affixes.TryGetValue(line.Ids[0], out var affix))
            {
                var look = _looks.For(affix);
                if (look != null && ObjectDB.instance && ObjectDB.instance.GetItemPrefab(look.Prefab))
                {
                    return look.Prefab;
                }
            }
            return ShatterRules.SplinterPrefab;
        }

        public static bool IsSplinter(ItemDrop.ItemData item)
        {
            return item != null && item.m_dropPrefab && item.m_dropPrefab.name.StartsWith(ShatterRules.SplinterPrefab, System.StringComparison.Ordinal);
        }

        /// <summary>The line a splinter holds, or null (not a splinter, or its saved line can't be read).</summary>
        public static LootItem.Line LineOf(ItemDrop.ItemData item)
        {
            return IsSplinter(item) ? SplinterRules.Read(item.m_customData) : null;
        }

        /// <summary>A new splinter item holding the line, not yet in any inventory. Null when the item isn't registered.</summary>
        public static ItemDrop.ItemData Create(LootItem.Line line)
        {
            var prefab = ObjectDB.instance && line != null ? ObjectDB.instance.GetItemPrefab(PrefabFor(line)) : null;
            var template = prefab ? prefab.GetComponent<ItemDrop>() : null;
            if (!template || line == null)
            {
                return null;
            }
            var item = template.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = 1;
            item.m_worldLevel = (byte)Game.m_worldLevel;
            SplinterRules.Write(line, item.m_customData);
            if (prefab.name == ShatterRules.SplinterPrefab)
            {
                Jotunn.Logger.LogWarning($"Affix Splinter for {string.Join("+", line.Ids)} uses the plain look: its family has no registered look.");
            }
            return item;
        }

        /// <summary>"Swift +12% attack speed [3–15]": the held line as results and tooltips show it.</summary>
        public static string LineText(LootItem.Line line)
        {
            return line == null ? "unreadable affix" : LootItem.LineText(line, Loot.Affixes);
        }

        /// <summary>The tooltip's extra lines: the held affix, where it goes and what can take it.</summary>
        public static string Tooltip(ItemDrop.ItemData item)
        {
            var line = LineOf(item);
            if (line == null)
            {
                return "<color=" + ForgeRules.InactiveColor + ">This splinter's affix can't be read.</color>";
            }
            var held = line.Ids.Where(Loot.Affixes.ContainsKey).Select(id => Loot.Affixes[id]).ToList();
            string color = line.Resonant ? ForgeRules.ResonantColor : Loot.MagicColor;
            string text = ForgeRules.BulletLine("<color=" + color + ">" + LineText(line) + "</color>", line.Resonant);
            if (held.Count == line.Ids.Length)
            {
                string place = line.Resonant ? "Prefix and suffix (resonant): needs an item with no affixes" : held[0].IsPrefix ? "Prefix" : "Suffix";
                var classes = held.Select(a => (IEnumerable<string>)a.Classes).Aggregate((a, b) => a.Intersect(b)).OrderBy(c => c);
                text += "\n<color=#B0A898>" + place + "\nFits: " + string.Join(", ", classes.Select(c => c.Replace('_', ' '))) + "</color>";
            }
            return text;
        }
    }
}
