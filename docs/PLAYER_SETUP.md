# Setting up the mods

For everyone joining our Valheim server. Takes about 10 minutes. You only do this once; after that, just update when Krogon sends a new version.

## What you'll install

| Mod | What it does | Where from |
|---|---|---|
| **Corruption** | Our endgame mod: corruption shards, the Riftforge, creature talismans | Krogon sends you `Corruption.dll` |
| **Orlog** | Our classes mod: Diablo 2-style character levels and XP from kills, attributes, the class eitr pool; choose your class at the Orlog stone in the spawn area (what the skills do is still to come) | Krogon sends you `Orlog.dll` |
| **CraftingOverhaul** | Our crafting mod: recipe search, collapsible categories, crafting and building from nearby chests | Krogon sends you `CraftingOverhaul.dll` |
| [EquipmentAndQuickSlots](https://thunderstore.io/c/valheim/p/RandyKnapp/EquipmentAndQuickSlots/) | Equipment slots and quick slots. **Talismans go in its slots**, so you need it | Thunderstore |
| [AdventureBackpacks](https://thunderstore.io/c/valheim/p/Vapok/AdventureBackpacks/) | Craftable backpacks | Thunderstore |
| [PlantEasily](https://thunderstore.io/c/valheim/p/Advize/PlantEasily/) | Plant and harvest crops in bulk | Thunderstore (optional) |

### Dependencies

These mods need three more packages. r2modman installs them automatically when you download the mods (accept when it asks); if you install by hand, you need them too:

| Package | Minimum version | Needed by |
|---|---|---|
| [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) (denikson) | 5.4.2351 | Everything: it's the mod loader |
| [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/) (ValheimModding) | 2.30.2 | Corruption, Orlog, CraftingOverhaul, AdventureBackpacks, EquipmentAndQuickSlots |
| [YamlDotNet](https://thunderstore.io/c/valheim/p/ValheimModding/YamlDotNet/) (ValheimModding) | 16.3.1 | AdventureBackpacks |

PlantEasily needs only BepInExPack.

**Everyone must have the same version of Corruption, Orlog and AdventureBackpacks as the server**, or the game won't let you join. Our three mods aren't on Thunderstore, so always use the files Krogon sends.

## 1. Install the mod manager

1. Download **r2modman** from [thunderstore.io/package/ebkr/r2modman](https://thunderstore.io/package/ebkr/r2modman/) and install it.
2. Open it, pick **Valheim**, and create a new profile (call it anything, e.g. "Krogon server").

**Shortcut:** if Krogon sends you a **profile code**, use **Import / update profile → From code** instead. It installs the Thunderstore mods for you. Then skip to step 3.

## 2. Install the Thunderstore mods

In r2modman, go to **Online**, search for each mod and click **Download** (accept the dependencies it offers):

- EquipmentAndQuickSlots (by RandyKnapp)
- AdventureBackpacks (by Vapok)
- PlantEasily (by Advize), optional

### Or install by hand (without r2modman)

1. **BepInExPack Valheim:** download it, open the zip, and copy everything inside its `BepInExPack_Valheim` folder into your Valheim game folder (Steam: right-click Valheim → Manage → Browse local files), next to `valheim.exe`.
2. Start Valheim once and close it. This creates the `BepInEx\plugins` folder.
3. **Jötunn, YamlDotNet, EquipmentAndQuickSlots, AdventureBackpacks, PlantEasily:** download each, open the zip, and copy its `.dll` files (with any folder they're in) into `BepInEx\plugins`, one folder per mod.
4. Continue with step 3 below, using `BepInEx\plugins` in your Valheim folder. Then start Valheim from Steam as normal.

## 3. Add our three mods

1. In r2modman: **Settings → Browse profile folder**.
2. Open `BepInEx`, then `plugins`.
3. Create a folder called `Corruption` and put `Corruption.dll` in it.
4. Create a folder called `Orlog` and put `Orlog.dll` in it.
5. Create a folder called `CraftingOverhaul` and put `CraftingOverhaul.dll` in it. Its classes screens (health/eitr/stamina bars, skills panel, class choice) need it.

## 4. Play

1. In r2modman, click **Start modded** (not the normal Steam button: that starts the game without mods).
2. Join the server as usual.

**Check it worked:** in the profile folder, open `BepInEx/LogOutput.log` and search for `Corruption 0.1.0 loaded`, `Orlog 0.1.0 loaded` and `CraftingOverhaul 0.1.0 loaded`.

## Updating

When Krogon sends new DLLs, replace the old ones in the same folders (step 3), with the game closed. Update the Thunderstore mods in r2modman's **Installed** tab when Krogon says so; don't update them on your own, as the server must match.

## Problems

| Problem | Fix |
|---|---|
| "Incompatible version" or kicked when joining | Your Corruption, Orlog or AdventureBackpacks version differs from the server's. Get the latest DLLs from Krogon / update the mod to the version the server runs |
| Game starts without mods | Start it from r2modman with **Start modded** |
| Game crashes or mods don't load; the log mentions a missing dependency | A package from the Dependencies table is missing or too old. Install or update it |
| No talisman slots | EquipmentAndQuickSlots isn't installed in your profile |
| Talismans do nothing | Same: they need EquipmentAndQuickSlots |
| Something else odd | Send Krogon your `BepInEx/LogOutput.log` from the profile folder |

**Console (PS5/Xbox) players can't join** while the server runs these mods: consoles can't install mods.

## Good to know

- **Cape or backpack:** a backpack takes the cape slot, so you wear one or the other.
- **Talismans** (in game they're called "<creature> charm"): craft a "<creature> charm" at the workbench from 3 of that creature's trophies, 1 chain and 10 leather scraps. Put it in one of the **Talisman** slots in the equipment panel (2 slots, plus the normal trinket slot). That creature family leaves you alone unless you hit them first.
- **Crafting from chests:** materials in chests within 20 m of the crafting station count for recipes and building, as well as your backpack.

---

## For the server admin (Krogon)

The dedicated server needs, in its own BepInEx install:

| Mod | Needed on the server? |
|---|---|
| BepInExPack_Valheim, Jötunn | Yes (same minimum versions as the Dependencies table) |
| Corruption | **Yes** (adds items; everyone must match) |
| Orlog | **Yes** (everyone must match; its level and attribute settings come from the server, XP is shared between players' games and the Orlog stone is placed through the server) |
| AdventureBackpacks (+ YamlDotNet) | **Yes** (adds items) |
| CraftingOverhaul | Recommended: its chest settings then come from the server |
| EquipmentAndQuickSlots | Optional: only syncs its own settings |
| PlantEasily | No |

- **Settings:** the gameplay settings of our mods are server-controlled. Edit them on the server in `BepInEx/config/com.krogongames.corruption.cfg`, `com.krogongames.orlog.cfg` and `com.krogongames.craftingoverhaul.cfg`; players' own values for those are ignored. Look settings (shard and Riftforge colours) stay per player, and so do CraftingOverhaul's screen settings (`3 - Character Sheet`, `4 - HUD`, `5 - Classes UI`) and Orlog's `2 - Display` → `ShowXpGains` ("+N XP" for each kill).
- **Orlog's server settings** (`com.krogongames.orlog.cfg`; defaults from Diablo 2, the eitr ones from Krogon):

| Section | Setting | Default | What it does |
|---|---|---|---|
| `1 - Levels` | `MaxLevel` | 99 | Highest character level; XP stops there |
| `1 - Levels` | `StatPointsPerLevel` | 5 | Attribute points gained each level |
| `1 - Levels` | `SkillPointsPerLevel` | 1 | Skill points gained each level |
| `1 - Levels` | `XpMultiplier` | 1 | Multiplies all XP from kills; 2 = levels twice as fast |
| `1 - Levels` | `ShareRange` | 70 | Players within this many metres of a kill share its XP |
| `1 - Levels` | `PartyBonusPerPlayer` | 0.35 | Extra XP per additional player sharing a kill, before it's split by level |
| `1 - Levels` | `MaxSkillPoints` | 20 | Most points one skill can take |
| `1 - Levels` | `SkillPace` | true | Diablo 2's pace: a skill takes one more point per level past its required level |
| `1 - Levels` | `HighLevelPenalty` | false | Diablo 2's XP cut from level 70; off, as it would put level 90 by Kall Fimbulbringer out of reach |
| `3 - Attributes` | `HealthPerD2Life` | 0.5 | Health per Diablo 2 life point (life per level and per Vitality point) |
| `3 - Attributes` | `StaminaPerD2Stamina` | 0.89 | Stamina per Diablo 2 stamina point |
| `3 - Attributes` | `EitrStart` | 15 | Eitr every class starts with, before food |
| `3 - Attributes` | `EitrPerLevel` | 1.5 | Eitr gained per level |
| `3 - Attributes` | `EitrPerEnergy` | 1.5 | Eitr per Energy point |
| `3 - Attributes` | `DamagePerStrength` | 1 | % more physical damage per Strength point, melee and thrown |
| `3 - Attributes` | `DamagePerDexterity` | 1 | % more physical damage per Dexterity point, bows and crossbows |
| `4 - Orlog Stone` | `StoneSize` | 1.7 | The Orlog stone's size in times the Vegvisir it copies; 1.7 ≈ twice a trophy stone (restart to apply) |
| `4 - Orlog Stone` | `StoneGap` | 3 | Metres between the Eikthyr trophy stone and the Orlog stone behind it |
| `4 - Orlog Stone` | `StoneTurn` | 0 | Degrees to turn the stone if it faces the wrong way |

- **Bigger backpacks (user, 2026-10-29: one more row on every level).** Set on the server in `BepInEx/config/vapok.mods.adventurebackpacks.cfg` (AdventureBackpacks syncs them to everyone). Each backpack has a section `[Backpack: <name>]` with `Backpack Size - Level 1` to `Level 4`, written as `{"x":3.0,"y":2.0}` (width, height). New values (AdventureBackpacks 2.4.6 defaults plus one row; widths unchanged):

| Backpack | Level 1 | Level 2 | Level 3 | Level 4 |
|---|---|---|---|---|
| Satchel (Meadows) | 3×2 | 4×2 | 5×2 | 6×2 |
| Rugged Backpack (Black Forest) | 3×3 | 4×3 | 5×3 | 6×3 |
| Bloodbag Wetpack (Swamp) | 2×4 | 3×4 | 4×4 | 5×4 |
| Arctic Sherpa Pack (Mountains) | 3×4 | 4×4 | 5×4 | 6×4 |
| Lox Hide Knappsack (Plains) | 3×5 | 4×5 | 5×5 | 6×5 |
| Explorers Wisppack (Mistlands) | 8×3 | 5×5 | 6×5 | 7×5 |
| Cinderward Pack (Ashlands) | 6×5 | 7×5 | 8×5 | 9×5 |
| Frostbound Haversack (Deep North) | 6×5 | 7×5 | 8×5 | 9×5 |

- **Sharing a profile:** in r2modman, **Settings → Export profile as code** and send the code with the three DLLs. The code covers the Thunderstore mods and their config, not our DLLs.
- **Building the DLLs:** a Debug build copies them to your `BepInEx/plugins` (see `README.MD`); those are the files to send.
