using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Corruption.Rules;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Corrupted offerings (design phase 6): each boss's vanilla offering made corrupted at the Riftforge
    ///     (<c>Corruption/Data/offerings.csv</c>). How each is used follows the boss's own summoning:
    ///     <list type="bullet">
    ///         <item>altar: one corrupted item, crafted from the full offering + 1 shard, summons the boss corrupted;</item>
    ///         <item>stand: every item stand at the altar must hold a corrupted item (each 1 item + 1 shard; user, 2026-10-30);</item>
    ///         <item>door: the corrupted Sealbreaker opens the Queen's door, and bosses in that location are corrupted.</item>
    ///     </list>
    ///     A corrupted boss's later phases (Kall: FrozenKing, then _p2, then _p3) stay corrupted. Register after the
    ///     Riftforge and the shard.
    /// </summary>
    internal static class CorruptedOfferings
    {
        /// <summary>On a door opened with a corrupted key: bosses in its location are corrupted.</summary>
        public const string KeyCorruptedDoor = "corruption_corrupted_door";

        private static readonly Color Red = new Color(0.7f, 0.09f, 0.05f);
        private static readonly Regex Phase = new Regex(@"^(.+)_p\d+$");

        /// <summary>Offerings by their corrupted item's name token ($item_corrupted_...), as inventories know items.</summary>
        private static readonly Dictionary<string, OfferingTable.Offering> ByName = new Dictionary<string, OfferingTable.Offering>();
        /// <summary>Corrupted item name tokens by the vanilla item's prefab name.</summary>
        private static readonly Dictionary<string, string> NameByVanillaItem = new Dictionary<string, string>();
        /// <summary>Corrupted item prefab hashes, as item stands store what they hold.</summary>
        private static readonly HashSet<int> StandHashes = new HashSet<int>();
        /// <summary>Altars on this game about to summon a corrupted boss.</summary>
        private static readonly HashSet<OfferingBowl> Pending = new HashSet<OfferingBowl>();
        /// <summary>Base names of corrupted bosses that just died on this game; their next phase is corrupted too.</summary>
        private static readonly HashSet<string> PhaseBases = new HashSet<string>();

        public static void Register()
        {
            var errors = new List<string>();
            var offerings = OfferingTable.Parse(DataFiles.Read("offerings.csv"), errors);
            foreach (var error in errors)
            {
                Jotunn.Logger.LogWarning(error);
            }
            var localization = LocalizationManager.Instance.GetLocalization();
            foreach (var offering in offerings)
            {
                Safe.Run($"register {offering.CorruptedItem}", () => RegisterOne(offering, localization));
            }
        }

        private static void RegisterOne(OfferingTable.Offering offering, CustomLocalization localization)
        {
            if (!PrefabManager.Instance.GetPrefab(offering.Item))
            {
                Jotunn.Logger.LogWarning($"offerings.csv: {offering.Item} isn't a game item; no corrupted version.");
                return;
            }
            string token = "item_corrupted_" + offering.Item.ToLowerInvariant();
            localization.AddTranslation("English", new Dictionary<string, string>
            {
                { token, offering.Name },
                { token + "_description", offering.Description }
            });

            var config = new ItemConfig
            {
                Name = "$" + token,
                Description = "$" + token + "_description",
                CraftingStation = Texts.RiftforgePrefab,
                Amount = 1
            };
            config.AddRequirement(offering.Item, offering.Amount);
            config.AddRequirement(Texts.ShardPrefab, 1);

            var item = new CustomItem(offering.CorruptedItem, offering.Item, config);
            Corrupted.Tint(item.ItemPrefab, Red, 0.6f);
            var shared = item.ItemDrop.m_itemData.m_shared;
            Safe.Run($"set the {offering.CorruptedItem} icon", () =>
            {
                // Cut from the user's render by tools/make_offering_icons.py; rendered from the model if missing.
                var icon = GemModel.LoadIcon($"offering_icon_{offering.Item}.rgba") ?? Corrupted.RenderIcon(item.ItemPrefab);
                if (icon)
                {
                    shared.m_icons = new[] { icon };
                }
            });
            ItemManager.Instance.AddItem(item);

            ByName["$" + token] = offering;
            NameByVanillaItem[offering.Item] = "$" + token;
            if (offering.Use == OfferingTable.UseStand)
            {
                StandHashes.Add(offering.CorruptedItem.GetStableHashCode());
            }
        }

        private static OfferingTable.Offering Find(ItemDrop.ItemData item)
        {
            return item != null && ByName.TryGetValue(item.m_shared.m_name, out var offering) ? offering : null;
        }

        private static OfferingTable.Offering FindByVanillaItem(ItemDrop vanilla, string use, out string corruptedName)
        {
            corruptedName = null;
            if (!vanilla || !NameByVanillaItem.TryGetValue(vanilla.name, out corruptedName))
            {
                return null;
            }
            var offering = ByName[corruptedName];
            return offering.Use == use ? offering : null;
        }

        /// <summary>The boss just summoned at an altar: the nearest uncorrupted one of its kind to the spawn point.</summary>
        private static void CorruptSummoned(OfferingBowl altar)
        {
            string prefab = altar.m_bossPrefab ? altar.m_bossPrefab.name : "";
            Character summoned = null;
            float nearest = float.MaxValue;
            foreach (var character in Character.GetAllCharacters())
            {
                if (!character || !character.IsBoss() || CorruptedBosses.IsCorrupted(character) ||
                    Utils.GetPrefabName(character.gameObject) != prefab)
                {
                    continue;
                }
                float distance = Vector3.Distance(character.transform.position, altar.m_bossSpawnPoint);
                if (distance < nearest)
                {
                    nearest = distance;
                    summoned = character;
                }
            }
            if (summoned)
            {
                CorruptedBosses.Corrupt(summoned);
                Jotunn.Logger.LogInfo($"Corrupted offering: {prefab} summoned corrupted.");
            }
            else
            {
                Jotunn.Logger.LogWarning($"Corrupted offering: the summoned {prefab} wasn't found; it isn't corrupted.");
            }
        }

        /// <summary>Opens a door with a corrupted key, as vanilla does with the normal key, and marks the door.</summary>
        private static bool OpenWithCorruptedKey(Door door, Humanoid user, ItemDrop.ItemData key)
        {
            if (!door.CanInteract() || !door.m_nview || !door.m_nview.IsValid())
            {
                return false;
            }
            if (door.m_checkGuardStone && !PrivateArea.CheckAccess(door.transform.position))
            {
                return true;
            }
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", key.m_shared.m_name));
            if (door.m_consumeKey)
            {
                user.GetInventory().RemoveOneItem(key);
            }
            door.m_nview.ClaimOwnership();
            door.m_nview.GetZDO().Set(KeyCorruptedDoor, true);
            door.Open((user.transform.position - door.transform.position).normalized);

            var location = Location.GetLocation(door.transform.position);
            int corrupted = 0;
            foreach (var character in Character.GetAllCharacters())
            {
                if (character && character.IsBoss() && !CorruptedBosses.IsCorrupted(character) &&
                    location && Location.GetLocation(character.transform.position) == location)
                {
                    CorruptedBosses.Corrupt(character);
                    corrupted++;
                }
            }
            Jotunn.Logger.LogInfo($"Corrupted key: door opened in {(location ? location.name : "no location")}, {corrupted} boss(es) there corrupted now.");
            return true;
        }

        /// <summary>Whether a door in the same location as this point was opened with a corrupted key.</summary>
        private static bool BehindCorruptedDoor(Vector3 point)
        {
            var location = Location.GetLocation(point);
            if (!location)
            {
                return false;
            }
            foreach (var door in UnityEngine.Object.FindObjectsOfType<Door>())
            {
                if (door.m_keyItem && door.m_nview && door.m_nview.IsValid() && door.m_nview.GetZDO().GetBool(KeyCorruptedDoor) &&
                    Location.GetLocation(door.transform.position) == location)
                {
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------ altars

        /// <summary>Offering a corrupted item at an altar: the one item is the whole offering.</summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.UseItem))]
        private static class OfferAtAltar
        {
            private static bool Prefix(OfferingBowl __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result, bool __runOriginal)
            {
                if (!__runOriginal)
                {
                    return false;
                }
                try
                {
                    var offering = Find(item);
                    if (offering == null)
                    {
                        Pending.Remove(__instance);
                        return true;
                    }
                    if (__instance.m_useItemStands)
                    {
                        return true;   // stand altars are filled stand by stand
                    }
                    __result = true;
                    if (__instance.IsBossSpawnQueued())
                    {
                        return false;
                    }
                    // Only an altar that summons a boss with this item (not, e.g., the Deep North gate altar).
                    bool summonsBoss = __instance.m_bossPrefab && __instance.m_bossPrefab.GetComponent<Character>();
                    if (offering.Use != OfferingTable.UseAltar || !summonsBoss || !__instance.m_bossItem ||
                        __instance.m_bossItem.name != offering.Item)
                    {
                        user.Message(MessageHud.MessageType.Center, __instance.m_wrongOfferText);
                        return false;
                    }
                    if (!__instance.m_nview || !__instance.m_nview.IsValid())
                    {
                        return false;
                    }
                    // Own the altar, so the summon (and finding the boss after it) runs on this game.
                    __instance.m_nview.ClaimOwnership();
                    Pending.Add(__instance);
                    __instance.m_usedSpawnItem = item;
                    __instance.m_interactUser = user;
                    __instance.InitiateSpawnBoss(__instance.GetSpawnPosition(), true);
                    if (!string.IsNullOrEmpty(__instance.m_setGlobalKey))
                    {
                        ZoneSystem.instance.SetGlobalKey(__instance.m_setGlobalKey);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("offer a corrupted item", ex);
                    return true;
                }
            }
        }

        /// <summary>After a corrupted summon, take the one corrupted item (vanilla would take the full offering's count).</summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.RPC_RemoveBossSpawnInventoryItems))]
        private static class TakeTheCorruptedItem
        {
            private static bool Prefix(OfferingBowl __instance, bool __runOriginal)
            {
                var item = __instance.m_usedSpawnItem;
                if (!__runOriginal || Find(item) == null || !__instance.m_interactUser)
                {
                    return __runOriginal;
                }
                Safe.Run("take the corrupted offering", () =>
                {
                    var user = __instance.m_interactUser;
                    user.GetInventory().RemoveItem(item.m_shared.m_name, 1);
                    user.ShowRemovedMessage(item, 1);
                    user.Message(MessageHud.MessageType.Center, __instance.m_usedAltarText);
                    if (__instance.m_itemSpawnPoint)
                    {
                        __instance.m_fuelAddedEffects.Create(__instance.m_itemSpawnPoint.position, __instance.transform.rotation);
                    }
                });
                return false;
            }
        }

        /// <summary>
        ///     Stand altars (Moder, Yagluth, Fader): every stand corrupted summons the boss corrupted; a mix of corrupted
        ///     and normal items is refused, so no corrupted item is used up on a normal summon.
        /// </summary>
        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.Interact))]
        private static class OfferAtStands
        {
            private static bool Prefix(OfferingBowl __instance, Humanoid user, bool hold, ref bool __result, bool __runOriginal)
            {
                if (!__runOriginal || hold || !__instance.m_useItemStands || __instance.IsBossSpawnQueued())
                {
                    return __runOriginal;
                }
                try
                {
                    var stands = __instance.FindItemStands();
                    if (stands.Count == 0 || stands.Any(s => !s.HaveAttachment()))
                    {
                        return true;   // vanilla says the offering is incomplete
                    }
                    int corrupted = stands.Count(s => StandHashes.Contains(s.GetAttachedItem()));
                    if (corrupted == 0)
                    {
                        Pending.Remove(__instance);
                        return true;
                    }
                    if (corrupted < stands.Count)
                    {
                        user.Message(MessageHud.MessageType.Center, "$corruption_offering_mixed");
                        __result = false;
                        return false;
                    }
                    if (__instance.m_nview && __instance.m_nview.IsValid())
                    {
                        __instance.m_nview.ClaimOwnership();
                        Pending.Add(__instance);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Safe.Report("check a stand altar for corrupted offerings", ex);
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(OfferingBowl), nameof(OfferingBowl.DelayedSpawnBoss))]
        private static class CorruptTheSummon
        {
            private static void Postfix(OfferingBowl __instance)
            {
                if (Pending.Remove(__instance))
                {
                    Safe.Run("corrupt the summoned boss", () => CorruptSummoned(__instance));
                }
            }
        }

        // ------------------------------------------------------------------ item stands

        /// <summary>A stand that takes an offering (egg cup, totem holder, bell holder) takes its corrupted version too.</summary>
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.IsSupported))]
        private static class StandsTakeCorrupted
        {
            private static void Postfix(ItemStand __instance, ItemDrop.ItemData item, ref bool __result)
            {
                if (__result)
                {
                    return;
                }
                var offering = Find(item);
                if (offering != null && offering.Use == OfferingTable.UseStand)
                {
                    __result = __instance.m_supportedItems.Any(s => s && s.name == offering.Item);
                }
            }
        }

        /// <summary>Pressing Use on an empty stand places a corrupted offering if you carry one (vanilla: the normal one).</summary>
        [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
        private static class PlaceCorruptedFirst
        {
            private static bool Prefix(ItemStand __instance, Humanoid user, bool hold, ref bool __result, bool __runOriginal)
            {
                if (!__runOriginal || hold || !__instance.m_autoAttach || __instance.m_supportedItems.Count != 1 ||
                    __instance.HaveAttachment() || !user)
                {
                    return __runOriginal;
                }
                try
                {
                    if (FindByVanillaItem(__instance.m_supportedItems[0], OfferingTable.UseStand, out string name) == null ||
                        !PrivateArea.CheckAccess(__instance.transform.position, 0f, false))
                    {
                        return true;
                    }
                    var corrupted = user.GetInventory().GetItem(name);
                    if (corrupted == null)
                    {
                        return true;
                    }
                    __instance.UseItem(user, corrupted);
                    __result = true;
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("place a corrupted offering", ex);
                    return true;
                }
            }
        }

        // ------------------------------------------------------------------ the Queen's door

        /// <summary>Using a door that takes a key: a corrupted key is used if you carry one.</summary>
        [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
        private static class OpenDoorWithCorruptedKey
        {
            private static bool Prefix(Door __instance, Humanoid character, bool hold, ref bool __result, bool __runOriginal)
            {
                if (!__runOriginal || hold || !__instance.m_keyItem || !character)
                {
                    return __runOriginal;
                }
                try
                {
                    if (FindByVanillaItem(__instance.m_keyItem, OfferingTable.UseDoor, out string name) == null)
                    {
                        return true;
                    }
                    var key = character.GetInventory().GetItem(name);
                    if (key == null)
                    {
                        return true;
                    }
                    __result = OpenWithCorruptedKey(__instance, character, key);
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("open a door with a corrupted key", ex);
                    return true;
                }
            }
        }

        /// <summary>The same when the corrupted key is used from the hotbar on the door.</summary>
        [HarmonyPatch(typeof(Door), nameof(Door.UseItem))]
        private static class UseCorruptedKeyOnDoor
        {
            private static bool Prefix(Door __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result, bool __runOriginal)
            {
                if (!__runOriginal || !__instance.m_keyItem || !user)
                {
                    return __runOriginal;
                }
                try
                {
                    var offering = Find(item);
                    if (offering == null || offering.Use != OfferingTable.UseDoor || offering.Item != __instance.m_keyItem.name)
                    {
                        return true;
                    }
                    __result = OpenWithCorruptedKey(__instance, user, item);
                    return false;
                }
                catch (Exception ex)
                {
                    Safe.Report("use a corrupted key", ex);
                    return true;
                }
            }
        }

        // ------------------------------------------------------------------ bosses appearing later

        /// <summary>
        ///     On the boss's owner, as it dies: a corrupted boss's next phase will be corrupted; an uncorrupted one's
        ///     won't (clearing any leftover from an earlier corrupted fight).
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        private static class RememberCorruptedPhase
        {
            private static void Prefix(Character __instance)
            {
                Safe.Run("remember a corrupted boss's phase", () =>
                {
                    if (!__instance.IsBoss() || !__instance.m_nview || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner())
                    {
                        return;
                    }
                    string name = Utils.GetPrefabName(__instance.gameObject);
                    var match = Phase.Match(name);
                    string baseName = match.Success ? match.Groups[1].Value : name;
                    if (CorruptedBosses.IsCorrupted(__instance))
                    {
                        PhaseBases.Add(baseName);
                    }
                    else
                    {
                        PhaseBases.Remove(baseName);
                    }
                });
            }
        }

        /// <summary>A boss appearing as a corrupted boss's next phase, or behind a door opened with a corrupted key.</summary>
        [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
        private static class CorruptLateBosses
        {
            private static void Postfix(Character __instance)
            {
                Safe.Run("corrupt a boss's next phase", () =>
                {
                    if (!__instance.IsBoss() || !__instance.m_nview || !__instance.m_nview.IsValid() || CorruptedBosses.IsCorrupted(__instance))
                    {
                        return;
                    }
                    string name = Utils.GetPrefabName(__instance.gameObject);
                    var match = Phase.Match(name);
                    if (match.Success && PhaseBases.Remove(match.Groups[1].Value))
                    {
                        CorruptedBosses.Corrupt(__instance);
                        Jotunn.Logger.LogInfo($"Corrupted boss: next phase {name} is corrupted too.");
                        return;
                    }
                    if (BehindCorruptedDoor(__instance.transform.position))
                    {
                        CorruptedBosses.Corrupt(__instance);
                        Jotunn.Logger.LogInfo($"Corrupted key: {name} appeared behind a corrupted door and is corrupted.");
                    }
                });
            }
        }
    }
}
