# Classes UI brief (for the UI overhaul session)

**From:** the classes session (`claude/class-skills`), 2026-10-30. **For:** the UI overhaul session (`claude/ui-overhaul`, CraftingOverhaul).
**Status:** agreed with the user; nothing built yet on either side.

Everything the classes system needs on screen, and the contract between the two plugins, so both sessions build at the same time without waiting for each other. Design and reasons: [`class-design.md`](class-design.md). Decisions: [`DECISIONS.md`](DECISIONS.md) (2026-10-30 rows).

## Who builds what

| Classes plugin (working name **Orlog**, `com.krogongames.orlog`, everyone must have it) | UI session (CraftingOverhaul, per player) |
|---|---|
| Levels, XP, attributes, the skill trees and their rules, the skill effects, eitr pool and regen, the Orlog stone, key handling for the skill slots, the skill button and block | Everything drawn on screen below. Reads the API in §3 by reflection (the same pattern as the character sheet reading EquipmentAndQuickSlots' `GetSlotIdsJson`) and uses `FlatJson` for the answers |
| Tooltip and description **text** with the numbers filled in (the classes plugin owns the numbers) | Where and how that text is shown |
| Level-up message (vanilla centre message) | Optional extra flair |

Without the Orlog plugin, CraftingOverhaul shows the new health/eitr/stamina bars (§2.1) and nothing class-related. CraftingOverhaul must keep working for players who don't have Orlog (`VersionCheckOnly`).

## 1. Build order (proposed)

1. **HUD bars** (§2.1). They don't depend on classes; build them first.
2. **Skill cross and XP bar** (§2.2, §2.3) on the demo data (§4).
3. **Skills panel** (§2.4) and **attribute points** (§2.5).
4. **Class choice window** (§2.6).

## 2. Screens

### 2.1 Health, eitr and stamina bars (user, 2026-10-30)

Reference: `docs/images/hud_reference_bars.jpg` (Elden Ring).
- Three horizontal bars together at the **bottom centre**: **health on top, eitr in the middle, stamina at the bottom**, in Valheim's colours (health red, eitr blue, stamina yellow). Replaces vanilla's separate health, stamina and eitr displays.
- Values come from vanilla: `Player.GetHealth`/`GetMaxHealth`, `GetStamina`/`GetMaxStamina`, `GetEitr`/`GetMaxEitr`. The classes plugin adds its eitr pool through the game's own max-eitr calculation, so these already include it.
- To settle in the UI session: bar length grows with the maximum, up to a cap (as in the reference); the eitr bar shows only when max eitr > 0 (always, for a character with a class); a new place for the food icons that sit beside vanilla's health display; check what vanilla already shows near the bottom centre (sailing controls, a mount's stamina) and where EquipmentAndQuickSlots draws its quick slots (players will keep eitr potions there).

### 2.2 Skill cross (4 slots)

Reference: `docs/images/hud_reference_skills.jpg` (four boxes in a cross). At the **bottom left**, where vanilla's health display was; styled like the rest of our UI.
- 4 slots, each showing: the skill's icon, its hotkey label, a **highlight on the slot that's on the skill button** (right click by default), a cooldown sweep, and greyed out with a reason when it can't be used (wrong weapon, not enough eitr, cooldown).
- Players fill and change slots by **dragging a skill from the skills panel** onto a slot; drag out of a slot to clear it.
- Hidden when the player has no class.
- The 1–0 hotbar stays vanilla. The classes plugin handles the hotkeys and the skill button; the cross only shows state.

### 2.3 XP bar

- Level number, XP into the current level and XP needed for the next. Placement is the UI session's call (suggestion: a thin bar right under the three bars, as WoW's XP bar runs along the bottom).
- A visible hint when there are **unspent points** (D2 shows a "level up" button).
- Hidden when the player has no class.

### 2.4 Skills panel

- Opened with a rebindable key (D2 uses T for the skill tree; pick a free default from Valheim's and the group's mods' bindings) and from the inventory.
- **Three tabs** (the class's skill tabs, in order). Each tab is a grid of **6 rows × 3 columns**: rows are the level tiers 1, 6, 12, 18, 24, 30; each skill has its `row` and `col` (§3). Draw the prerequisite lines between skills that are linked (`prereqs`).
- Each skill: icon, hard points (e.g. "3"), and its state: can spend, locked (level or prerequisite), maxed. Skills with status `later` show greyed as "coming later"; `dropped` skills aren't shown (their slot stays empty).
- Unspent skill points shown on the panel. Click (or a + button) on a skill to spend a point; the answer says why when refused.
- Hover tooltip: the description text for the current level and the next (text from the API).
- **Drag a skill from here onto the skill cross.** Passives can't be dragged (they don't take a slot).

### 2.5 Attribute points

- The four D2 attributes: Strength, Dexterity, Vitality, Energy, each with its value, what it gives (text from the API, e.g. "+12% bow damage") and a + button while there are unspent stat points.
- Suggestion: on the character sheet's stats parchment, which already shows the character's stats.

### 2.6 Class choice window (the Orlog stone)

- Opens when a player uses the **Orlog stone** at the spawn area (the classes plugin places the stone and handles the interaction; it raises a request the UI picks up, §3).
- Lists the classes (only the **Amazon** at first; the Barbarian and maybe the Paladin later): name, icon, short description, and a confirm button.
- Changing class later (at the stone, for a cost) comes later; for now the window only appears for a character without a class.

## 3. API contract (version 1)

A static class `Orlog.Api` in the Orlog assembly, found through `Chainloader.PluginInfos["com.krogongames.orlog"]` like the sheet finds EAQS. All methods are `public static`, use only `string`, `int`, `float`, `bool` and `UnityEngine.Sprite`, and work on the **local player**. JSON answers are flat objects or string arrays (what `FlatJson` reads). An empty string means "no data" (no player yet, no class).

**Changes without events:** `GetChangeCounter()` goes up by one whenever anything shown changes (XP, level, points, skills, slots, class). Re-read when it differs from the last value seen. Slot cooldowns and usability change every frame, so the cross polls `GetSlotStateJson` each frame while visible.

| Method | Returns |
|---|---|
| `int GetApiVersion()` | `1`. Check it first; turn the class UI off if it's missing or newer than you know |
| `int GetChangeCounter()` | See above |
| `int GetClassStoneRequest()` | Goes up by one each time the local player uses the Orlog stone: open the class choice window |
| `string GetCharacterJson()` | `{"class":"Amazon","level":12,"maxLevel":99,"xp":4410,"xpLevelStart":3720,"xpNextLevel":4925,"statPoints":5,"skillPoints":1,"strength":20,"dexterity":40,"vitality":25,"energy":15}`. `class` is `""` without a class |
| `string GetAttributeInfoJson(string attribute)` | `attribute` is `strength`, `dexterity`, `vitality` or `energy`: `{"name":"Dexterity","value":40,"effect":"+12% bow damage"}` |
| `string GetClassIdsJson()` | `["Amazon"]` |
| `string GetClassInfoJson(string classId)` | `{"id":"Amazon","name":"Amazon","description":"…","tab0":"Bow and Crossbow","tab1":"Passive and Magic","tab2":"Javelin and Spear"}` |
| `Sprite GetClassIcon(string classId)` | The class icon, or null |
| `string GetSkillIdsJson(string classId)` | Every skill id of the class, in table order: `["MagicArrow","FireArrow",…]` |
| `string GetSkillInfoJson(string skillId)` | `{"id":"MultipleShot","name":"Multiple Shot","tab":0,"row":2,"col":2,"reqLevel":6,"prereqs":"MagicArrow","kind":"attack","status":"v1","maxPoints":20,"points":3,"level":5,"canSpend":true,"spendBlocked":"","description":"Fires 4 arrows in a fan","nextDescription":"Fires 5 arrows in a fan","eitrCost":5}`. `prereqs`: skill ids joined by `+`, `""` for none. `kind`: `attack` (goes on the skill button), `cast` (casts on its hotkey) or `passive` (no slot). `status`: `v1`, `reworked`, `later`, `dropped`. `points`: hard points; `level`: effective level with +skills and synergies. `spendBlocked`: why a point can't be spent, `""` if it can |
| `Sprite GetSkillIcon(string skillId)` | The skill icon, or null |
| `string GetSkillBarJson()` | `{"slot0":"MultipleShot","slot1":"InnerSight","slot2":"","slot3":"FireArrow","key0":"1","key1":"2","key2":"3","key3":"4","skillButton":"Mouse 1","selected":0}`. `key*`: display labels of the slot hotkeys; `selected`: the slot on the skill button, `-1` for none |
| `string GetSlotStateJson(int slot)` | `{"usable":false,"blocked":"weapon","cooldown":1.2,"cooldownTotal":3,"eitrCost":5}`. `blocked`: `""`, `weapon`, `eitr` or `cooldown` |

Actions answer `""` on success, or a short reason the UI can show ("Needs level 12", "No skill points", "Passive skills don't take a slot"):

| Method | Does |
|---|---|
| `string SpendStatPoint(string attribute)` | One point into an attribute |
| `string SpendSkillPoint(string skillId)` | One point into a skill |
| `string SetSkillSlot(int slot, string skillId)` | Puts a skill in slot 0–3; `""` clears the slot. A skill already in another slot moves |
| `string ChooseClass(string classId)` | Only while the class choice window is open from the stone |

Anything the UI needs that isn't here: add it to this table in `master` (both sessions), and bump the version only for changes that break existing callers.

## 4. Demo data (build before the classes plugin exists)

Add a development setting in CraftingOverhaul (e.g. `ClassesUiDemo`, off by default) that serves the calls in §3 from built-in sample data when Orlog isn't installed, so the screens can be built and play-tested now:
- Character: the `GetCharacterJson` example above (an Amazon at level 12 with points to spend).
- Skills: the Amazon's 30 skills from [`data/d2-reference/amazon.csv`](../data/d2-reference/amazon.csv) (tab, row, col, name, required level, prerequisites) with the statuses in [`class-design.md`](class-design.md#5-amazon-first-class) §5 (`v1`, `reworked`, `later`); a few with points; vanilla item icons as placeholders (e.g. the fire arrow's icon for Fire Arrow).
- Actions change the sample data locally (spending a point lowers the unspent count, dragging fills a slot), so the panel and cross can be tried for real.
- Hard-code the sample data in the demo code (don't ship the CSV); the demo setting goes on the "before release" list in `CURRENT_STATE.md`.

## 5. Open on the classes side (affects the UI later)

- How D2 values scale to Valheim (tooltip numbers): the text comes from the API, so no UI change.
- Changing class at the stone for a cost: a later version of the class choice window.
- Skill and class icons: placeholders until art is asked for.
