using System.Collections.Generic;
using Corruption.Rules;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     The currency orbs as items (corruption-design.md → Currency orbs), used at the Riftforge
    ///     (<see cref="RiftforgeCrafting" />). Each uses the shared orb model with its own texture and glow, made by
    ///     <c>tools/make_orbs.py</c> after Krogon's concept render; Augmentation, Regal and the Jeweller's Orb add a copper
    ///     part. The Mirror of Kalandra has no art yet: it's a tinted vanilla gem until it does.
    ///     <para>
    ///         No drops yet (ROADMAP L4 needs the drop sources and rates); for testing, <c>co_craft give</c> hands them out.
    ///         Stack size and weight are the corruption shard's (the existing currency item) until L4's <c>orbs.csv</c>.
    ///     </para>
    /// </summary>
    internal static class OrbItems
    {
        /// <summary>Like the shard (<see cref="ShardItem" />): stack 50, weight 0.2.</summary>
        private const int StackSize = 50;

        private const float Weight = 0.2f;

        private const string BasePrefab = "GemstoneRed";

        /// <summary>The Mirror's stand-in until it has its own art: a pale, silvery vanilla gem.</summary>
        private const string MirrorBasePrefab = "GemstoneBlue";

        private static readonly Color MirrorColor = new Color(0.85f, 0.9f, 1f);

        /// <summary>The copper of the add-on parts (make_orbs.py COPPER).</summary>
        private static readonly Color Copper = new Color(178f / 255f, 108f / 255f, 62f / 255f);

        /// <summary>Glow colours, from each orb's painter in make_orbs.py.</summary>
        private static readonly Dictionary<string, Color> Glow = new Dictionary<string, Color>
        {
            { "transmutation", new Color(0.45f, 0.7f, 1f) },
            { "augmentation", new Color(0.3f, 0.55f, 1f) },
            { "alchemy", new Color(1f, 0.72f, 0.12f) },
            { "regal", new Color(1f, 0.78f, 0.2f) },
            { "exalted", new Color(1f, 0.82f, 0.25f) },
            { "divine", new Color(1f, 0.9f, 0.65f) },
            { "annulment", new Color(0.6f, 0.55f, 0.78f) },
            { "chance", new Color(0.85f, 0.85f, 0.85f) },
            { "jeweller", new Color(0.3f, 0.9f, 0.8f) },
            { "mirror", MirrorColor }
        };

        /// <summary>Add-on parts (make_orbs.py ORBS).</summary>
        private static readonly Dictionary<string, string> Addons = new Dictionary<string, string>
        {
            { "augmentation", "orb_band.obj" }, { "regal", "orb_rim.obj" }, { "jeweller", "orb_sockets.obj" }
        };

        public static void Register()
        {
            var localization = LocalizationManager.Instance.GetLocalization();
            foreach (var orb in OrbRules.Orbs)
            {
                Safe.Run($"register the {OrbRules.Name(orb)}", () => RegisterOne(orb, localization));
            }
        }

        private static void RegisterOne(string orb, CustomLocalization localization)
        {
            string prefab = OrbRules.Prefab(orb);
            string token = "item_" + prefab.ToLowerInvariant();
            localization.AddTranslation("English", new Dictionary<string, string>
            {
                { token, OrbRules.Name(orb) },
                { token + "_description", OrbRules.Description(orb) }
            });

            bool mirror = orb == "mirror";
            var item = new CustomItem(prefab, mirror ? MirrorBasePrefab : BasePrefab, new ItemConfig
            {
                Name = "$" + token,
                Description = "$" + token + "_description",
                StackSize = StackSize,
                Weight = Weight
            });

            var glow = Glow[orb];
            // A broken look must never stop the item from registering (unregistered items vanish from inventories).
            bool custom = false;
            if (!mirror)
            {
                var look = new GemModel.Look
                {
                    Name = prefab,
                    Mesh = "orb.obj",
                    Albedo = $"orb_{orb}_albedo.rgba",
                    Emission = $"orb_{orb}_emission.rgba",
                    AddonMesh = Addons.TryGetValue(orb, out var addon) ? addon : null,
                    AddonColor = Copper
                };
                try
                {
                    custom = GemModel.Apply(item.ItemPrefab, look, glow);
                }
                catch (System.Exception ex)
                {
                    Jotunn.Logger.LogError($"Custom model for {prefab} failed; using a tinted gem instead. {ex}");
                }
            }
            if (!custom)
            {
                Corrupted.Tint(item.ItemPrefab, glow, 0.6f);
            }

            var shared = item.ItemDrop.m_itemData.m_shared;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            // Orbs go through portals and are meant to be traded (corruption-design.md → Currency orbs → Rules).
            shared.m_teleportable = true;
            Safe.Run($"render the {prefab} icon", () =>
            {
                var icon = Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                }
            });
            // After the icon render, so the light doesn't colour the icon.
            Safe.Run($"add the {prefab} glow", () => GemModel.AddGlow(item.ItemPrefab, glow));

            ItemManager.Instance.AddItem(item);
        }
    }
}
