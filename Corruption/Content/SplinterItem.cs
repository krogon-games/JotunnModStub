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
    ///     The look is a PLACEHOLDER (a tinted vanilla Crystal): Krogon asked for its own models, a shape per kind of
    ///     affix and a colour per stat, and picks from drafts first (2026-11-10; docs/images/splinters_draft2.png).
    /// </summary>
    internal static class SplinterItem
    {
        private const string BasePrefab = "Crystal";

        // The shard's weight, as the orbs use (no number of its own in the design; the Splinters tab is weightless).
        private const float Weight = 0.2f;

        private static readonly Color PlaceholderColor = new Color(0.55f, 0.85f, 0.75f);

        public const string NameToken = "$item_corruptionaffixsplinter";

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
            var item = new CustomItem(ShatterRules.SplinterPrefab, BasePrefab, new ItemConfig
            {
                Name = NameToken,
                Description = NameToken + "_description",
                StackSize = 1,
                Weight = Weight
            });
            Corrupted.Tint(item.ItemPrefab, PlaceholderColor, 0.6f);
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
                }
            });
            ItemManager.Instance.AddItem(item);
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
            var prefab = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(ShatterRules.SplinterPrefab) : null;
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
