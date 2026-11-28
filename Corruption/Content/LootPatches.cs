using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Rules;
using HarmonyLib;
using UnityEngine;

namespace Corruption.Content
{
    /// <summary>
    ///     Magic and Rare drops, maps and currency orbs (<see cref="OrbDrops" />, 2026-11-19), from kills, and each nearby
    ///     player's boss shards (<see cref="ShardDrops.DropBossShards" />). Runs on the machine that simulates the creature, where vanilla drops its loot.
    ///     Personal loot (design; <see cref="PersonalLoot" />): the i-th roll belongs to the i-th nearby player, so only they see and take
    ///     that drop for the first seconds, and every drop gets its loot beam. Every player within the game's own "nearby players" range (the range it counts
    ///     players in to scale creature difficulty, <c>Game.m_difficultyScaleRange</c>) rolls separately.
    ///     Both of the game's death paths count: drops at once, and drops handed to the ragdoll (which turns the
    ///     creature's own drops off first); see <see cref="DeathLoot" />.
    /// </summary>
    internal static class LootDrops
    {
        private static readonly List<Player> Nearby = new List<Player>();
        private static readonly DeathLoot Deaths = new DeathLoot();

        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
        private static class DeathScope
        {
            private static void Prefix(Character __instance)
            {
                try
                {
                    var drop = __instance ? __instance.GetComponent<CharacterDrop>() : null;
                    Deaths.BeginDeath(drop, drop && drop.m_dropsEnabled);
                }
                catch (System.Exception ex)
                {
                    Deaths.BeginDeath(null, false);
                    Safe.Report("track a creature's death for loot", ex);
                }
            }

            private static void Finalizer() => Deaths.EndDeath();
        }

        /// <summary>The ragdoll takes the creature's drops (the game then turns <c>m_dropsEnabled</c> off): still a kill that rolls.</summary>
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Setup))]
        private static class RagdollScope
        {
            private static void Postfix(Ragdoll __instance)
            {
                if (__instance)
                {
                    Deaths.RagdollSetup(__instance.m_dropItems);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.OnDeath))]
        private static class Rolls
        {
            private static void Postfix(CharacterDrop __instance)
            {
                Safe.Run("drop Magic and Rare loot", () =>
                {
                    var character = __instance.m_character;
                    if (!Deaths.TakeRolls(__instance, __instance.m_dropsEnabled) ||
                        !character || character.IsPlayer() || character.IsTamed() ||
                        // Only the creature's owner rolls (the game only calls this there; checked again in case another mod does).
                        (character.m_nview && character.m_nview.IsValid() && !character.m_nview.IsOwner()))
                    {
                        return;
                    }

                    Vector3 position = character.GetCenterPoint();
                    float range = Game.instance ? Game.instance.m_difficultyScaleRange : 100f;
                    Nearby.Clear();
                    Player.GetPlayersInRange(position, range, Nearby);

                    // Every boss kill: each nearby player's own shards. Needs no loot tables (they worked without them before).
                    if (character.IsBoss())
                    {
                        Safe.Run("drop each player's boss shards", () => ShardDrops.DropBossShards(position, Nearby, __instance.m_cheated));
                    }

                    if (!Loot.Loaded || !CorruptionConfig.LootEnabled.Value)
                    {
                        return;
                    }

                    var sources = new List<string>();
                    int stars = character.GetLevel() - 1;
                    if (stars > 0)
                    {
                        sources.Add("star_" + Mathf.Min(stars, 3));
                    }
                    // A rip creature's drop rates are its tier's (read from its own stamp); an unstamped maddened one
                    // (a corrupted boss's summon, a rip opened before tiers existed) counts as tier 1.
                    if (Maddened.IsMaddened(character))
                    {
                        var zdo = character.m_nview && character.m_nview.IsValid() ? character.m_nview.GetZDO() : null;
                        var row = MapTiers.Loaded && MapTiers.IsStamped(zdo) ? MapTiers.RowOf(zdo) : null;
                        sources.AddRange(row != null ? RipTierRules.DropSources(row) : new List<string> { "rip_tier_1" });
                    }
                    bool corruptedBoss = character.IsBoss() && CorruptedBosses.IsCorrupted(character);
                    if (corruptedBoss)
                    {
                        sources.Add("corrupted_boss");
                    }

                    string biome = Loot.BiomeAt(position);
                    int players = DeathLoot.RollsFor(Nearby.Count);
                    for (int i = 0; i < players; i++)
                    {
                        // Roll i belongs to the i-th nearby player: only they see and can take it for the first seconds
                        // (personal loot). With nobody near there is no owner and the drop is free, as before.
                        long owner = i < Nearby.Count && Nearby[i] ? Nearby[i].GetPlayerID() : 0L;
                        // Orbs: their own roll per player, beside the gear roll (Krogon, 2026-11-19: all content; a boss is
                        // one kill). A corrupted boss gives each player one guaranteed orb instead (2026-11-23, decision 26).
                        if (corruptedBoss)
                        {
                            var orb = OrbDrops.FindOne();
                            if (orb)
                            {
                                OrbDrops.DropPersonal(orb, position, owner);
                            }
                        }
                        else
                        {
                            OrbDrops.Drop(OrbDrops.Roll(sources), position, owner);
                        }
                        // A map of the biome it died in (2026-11-23, decisions 19 and 31), once the conditions are met.
                        var map = MapDrops.Roll(biome, sources);
                        if (map)
                        {
                            OrbDrops.DropPersonal(map, position, owner);
                        }
                        foreach (var rarity in Loot.Drops.Roll(sources, () => Random.value * 0.99999f))
                        {
                            // Uniques come with a later step.
                            if (rarity != "magic" && rarity != "rare")
                            {
                                continue;
                            }
                            var item = Loot.Create(rarity, biome);
                            if (item != null)
                            {
                                PersonalLoot.Tag(Loot.Drop(item, position), owner, PersonalLoot.BeamRowFor(item));
                            }
                        }
                    }
                });
            }
        }
    }

    /// <summary>Affix lines at the end of the item tooltip; vanilla's own text and other mods' lines stay.</summary>
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip),
        typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(bool))]
    internal static class LootTooltip
    {
        private const string InactiveColor = ForgeRules.InactiveColor;

        /// <summary>
        ///     While true, tooltips leave out the affix block: the Riftforge's Forge tab shows the affixes in their own
        ///     panel (<see cref="Block" />) and the rest of the tooltip beside it.
        /// </summary>
        [System.ThreadStatic] public static bool OmitAffixes;

        private static void Postfix(ItemDrop.ItemData item, int qualityLevel, float worldLevel, ref string __result)
        {
            if (MapItems.Find(item) != null)
            {
                // A map (rips round, decision 15): tier line, what you'll face and earn, marks, then the game's own
                // text (weight) without the map's description. The Forge no longer asks for a map's tooltip.
                string withMap = __result;
                Safe.Run("rewrite a map's tooltip", () =>
                {
                    string text = MapForge.Tooltip(item, withMap);
                    if (!string.IsNullOrEmpty(text))
                    {
                        withMap = text;
                    }
                });
                __result = withMap;
                return;
            }
            if (OmitAffixes)
            {
                return;
            }
            if (SplinterItem.IsSplinter(item))
            {
                string withLine = __result;
                Safe.Run("add a splinter's affix to its tooltip", () => withLine = withLine + "\n\n" + SplinterItem.Tooltip(item));
                __result = withLine;
                return;
            }
            var loot = Loot.Of(item);
            bool corrupted = RiftforgeCrafting.IsCorrupted(item), mirrored = RiftforgeCrafting.IsMirrored(item);
            if (loot == null)
            {
                if (corrupted || mirrored)
                {
                    __result += "\n" + ForgeRules.CentredMarks(Locks(corrupted, mirrored));
                }
                return;
            }
            string result = __result;
            Safe.Run("add affix lines to a tooltip", () =>
            {
                result = result + "\n\n" + Section(item, qualityLevel, worldLevel, null);
            });
            __result = result;
        }

        /// <summary>
        ///     The affix block as the tooltip shows it: the coloured rarity header ("Rare Bronze Sword") in
        ///     <paramref name="header" />, then one coloured line per affix, in the item's line order (index i is
        ///     <c>Loot.Of(item).Lines[i]</c>). Empty for an item without affixes.
        /// </summary>
        public static List<string> Block(ItemDrop.ItemData item, int qualityLevel, float worldLevel, out string header)
        {
            var lines = new List<string>();
            header = "";
            var loot = Loot.Of(item);
            if (loot == null)
            {
                return lines;
            }
            string color = Loot.ColorFor(loot.Rarity);
            header = "<color=" + color + ">" + (loot.Rarity == "rare" ? "Rare " + Localization.instance.Localize(item.m_shared.m_name)
                         : loot.Rarity == "normal" ? "Normal" : "Magic") + "</color>";
            foreach (var line in loot.Lines)
            {
                bool active = Loot.Affixes.TryGetValue(line.Ids[0], out var affix) && Loot.ActiveEffects.Contains(affix.Effect);
                string bonus = active ? Bonus(item, loot, line, affix.Effect, qualityLevel, worldLevel) : null;
                // A shard's corrupted affix is its own line in the corruption red, so the extra one is clear.
                string lineColor = !active ? InactiveColor : line.Corrupted ? RiftforgeCrafting.CorruptedColor : color;
                string text = "<color=" + lineColor + ">" + LootItem.LineText(line, Loot.Affixes, bonus) +
                              (line.Corrupted ? " (corrupted)" : "") + (active ? "" : " (not active yet)") + "</color>";
                // No cap line here (Krogon, 2026-11-10: tooltips are crowded); caps go on a stats sheet (#60).
                lines.Add(text);
            }
            return lines;
        }

        /// <summary>
        ///     The affix section as every tooltip and the Forge tab's bench show it (Krogon, 2026-11-10: "copy those same
        ///     rules"): the rarity header, a little space, each affix behind a bullet with a hanging indent (a resonant
        ///     affix green with a diamond), then the Corrupted/Mirrored marks centred. Lines listed in
        ///     <paramref name="highlight" /> (indexes into the item's lines) are marked with a soft band and bold.
        ///     Empty for an item without affixes.
        /// </summary>
        public static string Section(ItemDrop.ItemData item, int qualityLevel, float worldLevel, ICollection<int> highlight)
        {
            var lines = Block(item, qualityLevel, worldLevel, out string header);
            var loot = Loot.Of(item);
            if (loot == null)
            {
                return "";
            }
            var text = new StringBuilder(header).Append("\n<size=45%> </size>");
            for (int i = 0; i < lines.Count; i++)
            {
                bool resonant = i < loot.Lines.Count && loot.Lines[i].Resonant;
                string line = ForgeRules.BulletLine(lines[i], resonant);
                text.Append('\n').Append(highlight != null && highlight.Contains(i) ? "<mark=#E8C66A40><b>" + line + "</b></mark>" : line);
            }
            text.Append(ForgeRules.CentredMarks(LocksOf(item)));
            return text.ToString();
        }

        /// <summary>The Riftforge marks of an item, as tooltip lines (empty when it has none).</summary>
        public static string LocksOf(ItemDrop.ItemData item)
        {
            return Locks(RiftforgeCrafting.IsCorrupted(item), RiftforgeCrafting.IsMirrored(item));
        }

        /// <summary>The Riftforge locks: corrupted (no orb but the Mirror) and mirrored (never changed again).</summary>
        private static string Locks(bool corrupted, bool mirrored)
        {
            string text = "";
            if (corrupted)
            {
                text += $"\n<color={RiftforgeCrafting.CorruptedColor}>Corrupted</color>";
            }
            if (mirrored)
            {
                text += $"\n<color={RiftforgeCrafting.MirroredColor}>Mirrored: can't be changed</color>";
            }
            return text;
        }

        /// <summary>
        ///     What one affix line adds to this item at the tooltip's upgrade level (user, 2026-10-30: show the actual
        ///     bonus next to the percentage), worked out from the item's vanilla numbers. Null if it can't be shown.
        /// </summary>
        private static string Bonus(ItemDrop.ItemData item, LootItem loot, LootItem.Line line, string effect, int quality, float worldLevel)
        {
            float share = line.Value / 100f;
            var shared = item.m_shared;
            Loot.Bypass = true;
            try
            {
                switch (effect)
                {
                    case "weapon_damage":
                    {
                        var d = item.GetDamage(quality, worldLevel);
                        return LootItem.BonusText((d.m_blunt + d.m_slash + d.m_pierce + d.m_fire + d.m_frost + d.m_lightning + d.m_poison + d.m_spirit) * share);
                    }
                    case "fire_damage_percent":
                    case "frost_damage_percent":
                    case "lightning_damage_percent":
                    case "poison_damage_percent":
                    case "spirit_damage_percent":
                    {
                        // Same order as the damage patch: a share of the physical damage after enhanced damage.
                        var d = item.GetDamage(quality, worldLevel);
                        float enhanced = 1f + loot.Total("weapon_damage", Loot.Affixes) / 100f;
                        return LootItem.BonusText((d.m_blunt + d.m_slash + d.m_pierce) * enhanced * share);
                    }
                    case "armour_percent":
                        return LootItem.BonusText(item.GetArmor(quality, worldLevel) * share);
                    case "max_durability":
                        return LootItem.BonusText(item.GetMaxDurability(quality) * share);
                    case "block_percent":
                        return LootItem.BonusText(item.GetBaseBlockPower(quality) * share);
                    case "knockback_percent":
                        return LootItem.BonusText(shared.m_attackForce * share);
                    case "backstab_percent":
                        return LootItem.BonusText(shared.m_backstabBonus * share, "x");
                    case "parry_percent":
                        return LootItem.BonusText(shared.m_timedBlockBonus * share, "x");
                    case "attack_stamina_reduction":
                        return shared.m_attack != null ? LootItem.BonusText(-shared.m_attack.m_attackStamina * share) : null;
                    default:
                        return null;
                }
            }
            finally
            {
                Loot.Bypass = false;
            }
        }
    }

    /// <summary>Inventory tooltips: the title shows the rolled name in its rarity colour (a map's name in its tier's colour).</summary>
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
    internal static class LootTooltipTitle
    {
        private static void Postfix(InventoryGrid __instance, ItemDrop.ItemData item, UITooltip tooltip)
        {
            Safe.Run("colour a loot item's tooltip title", () =>
            {
                string name = Loot.ColoredName(item) ?? MapForge.ColoredName(item);
                if (name != null && tooltip)
                {
                    tooltip.Set(name, item.GetTooltip(), __instance.m_tooltipAnchor);
                }
            });
        }
    }

    /// <summary>Items on the ground: the hover text shows the rolled name in its rarity colour; a drop locked to someone else shows nothing.</summary>
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText))]
    internal static class LootHoverText
    {
        private static void Postfix(ItemDrop __instance, ref string __result)
        {
            string result = __result;
            // A drop locked to someone else shows nothing at all (and no owner or time text for anyone).
            bool hidden = false;
            Safe.Run("hide a personal drop's hover text", () => hidden = PersonalLoot.Enabled && PersonalLoot.IsHiddenFromMe(__instance));
            if (hidden)
            {
                __result = "";
                return;
            }
            Safe.Run("colour a loot item's hover text", () =>
            {
                var item = __instance.m_itemData;
                string name = Loot.ColoredName(item) ?? MapForge.ColoredName(item);
                // The game localizes the hover text before returning it, so it starts with the translated base name.
                string baseName = Localization.instance.Localize(item.m_shared.m_name);
                if (name != null && result.StartsWith(baseName))
                {
                    result = name + result.Substring(baseName.Length);
                }
            });
            __result = result;
        }
    }

    /// <summary>
    ///     Item-stat affixes, through the game's own stat getters, so tooltips, combat and repairs all see them:
    ///     enhanced damage and elemental damage (<c>GetDamage</c>), armour, durability and block power.
    /// </summary>
    [HarmonyPatch]
    internal static class LootStatEffects
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDamage), typeof(int), typeof(float))]
        private static void Damage(ItemDrop.ItemData __instance, ref HitData.DamageTypes __result)
        {
            var loot = Loot.Bypass ? null : Loot.Of(__instance);
            if (loot == null)
            {
                return;
            }
            var damage = __result;
            Safe.Run("apply damage affixes", () =>
            {
                // Enhanced damage raises every combat damage type; chop and pickaxe (tool) damage stay as they are.
                float enhanced = 1f + loot.Total("weapon_damage", Loot.Affixes) / 100f;
                damage.m_blunt *= enhanced;
                damage.m_slash *= enhanced;
                damage.m_pierce *= enhanced;
                damage.m_fire *= enhanced;
                damage.m_frost *= enhanced;
                damage.m_lightning *= enhanced;
                damage.m_poison *= enhanced;
                damage.m_spirit *= enhanced;

                // Elemental affixes add a share of the (enhanced) physical damage (AFFIX_RULES: % of physical).
                float physical = damage.m_blunt + damage.m_slash + damage.m_pierce;
                damage.m_fire += physical * loot.Total("fire_damage_percent", Loot.Affixes) / 100f;
                damage.m_frost += physical * loot.Total("frost_damage_percent", Loot.Affixes) / 100f;
                damage.m_lightning += physical * loot.Total("lightning_damage_percent", Loot.Affixes) / 100f;
                damage.m_poison += physical * loot.Total("poison_damage_percent", Loot.Affixes) / 100f;
                damage.m_spirit += physical * loot.Total("spirit_damage_percent", Loot.Affixes) / 100f;
            });
            __result = damage;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetArmor), typeof(int), typeof(float))]
        private static void Armor(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "armour_percent") / 100f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetMaxDurability), typeof(int))]
        private static void Durability(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "max_durability") / 100f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetBaseBlockPower), typeof(int))]
        private static void BlockPower(ItemDrop.ItemData __instance, ref float __result)
        {
            __result *= 1f + Loot.Total(__instance, "block_percent") / 100f;
        }
    }

    /// <summary>Attack stamina affixes: less stamina per attack (or per swing, on tools) with the item.</summary>
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    internal static class LootAttackStamina
    {
        private static void Postfix(Attack __instance, ref float __result)
        {
            if (__result > 0f && __instance.m_weapon != null)
            {
                __result *= Mathf.Max(0f, 1f - Loot.Total(__instance.m_weapon, "attack_stamina_reduction") / 100f);
            }
        }
    }

    /// <summary>
    ///     Knockback, backstab and life steal affixes. A hit's push force and backstab bonus come from the weapon's shared data, so
    ///     they are raised on the attacker's game, where the hit is made, before it is sent to the target's owner. The
    ///     affixes are those of the item the hit comes from (<see cref="HitSources" />): an arrow uses the bow it was
    ///     shot from, even if the player has switched weapons since.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    internal static class LootHitEffects
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (hit == null || !Player.m_localPlayer || hit.GetAttacker() != Player.m_localPlayer || HitSources.IsDamageOverTime(hit))
            {
                return;
            }
            Safe.Run("apply knockback and backstab affixes", () =>
            {
                var weapon = HitSources.For(hit, Player.m_localPlayer);
                hit.m_pushForce *= 1f + Loot.Total(weapon, "knockback_percent") / 100f;
                hit.m_backstabBonus *= 1f + Loot.Total(weapon, "backstab_percent") / 100f;

                // Life steal: a share of the hit's combat damage comes back as health, capped (affix_caps.csv). Worked
                // out on the attacker's game, before the target's armour and resistances, which only its owner knows.
                float steal = AffixTotals.Capped(Loot.Total(weapon, "life_steal", false), Loot.Total(weapon, "life_steal", true), "life_steal", Loot.Caps) / 100f;
                if (steal > 0f && hit.GetAttacker() != null)
                {
                    var d = hit.m_damage;
                    float physical = d.m_blunt + d.m_slash + d.m_pierce;
                    float elemental = d.m_fire + d.m_frost + d.m_lightning + d.m_poison;
                    // A rip creature with Ironskin or Warded takes only 5% of that kind on its owner's game; life steal counts
                    // what the hit really deals to it (Krogon, 2026-11-27, decision 11: "Lifesteal should steal the amount it
                    // damages for."). The stamp is on the creature's ZDO, so this game can read it.
                    var rip = RipCreatures.Of(__instance);
                    if (rip != null)
                    {
                        physical *= rip.PhysicalTaken;
                        elemental *= rip.ElementalTaken;
                    }
                    float damage = physical + elemental + d.m_spirit;
                    Player.m_localPlayer.Heal(damage * steal);
                }
            });
        }
    }

    /// <summary>Parry affix: a bigger parry bonus with the blocking item (only called when a block is a parry).</summary>
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyTimedBlockBonus))]
    internal static class LootParry
    {
        private static void Postfix(SEMan __instance, ref float timedBlockBonus)
        {
            if (__instance.m_character is Humanoid humanoid && humanoid.IsPlayer())
            {
                timedBlockBonus *= 1f + Loot.Total(humanoid.GetCurrentBlocker(), "parry_percent") / 100f;
            }
        }
    }
}
