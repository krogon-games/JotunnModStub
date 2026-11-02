# Handoff: `claude/class-skills`

**Written:** 2026-11-02, from the real branch state. **Updated the same day** after three playtests: the skill tree passed, and the Orlog stone works (its look will be redesigned later, your call). C1 and C2 go to `master` in a PR. `git status` was clean before this file. The branch was 3 commits ahead of `master` (`dd93cb8`) and 16 behind. `master` (`68a63e0`: loot steps 3–4, the Riftforge window brief, the UI session's click fix and housekeeping, PRs #18–#19) is now merged in with no lost work. Conflicts in `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/DECISIONS.md` and `docs/PLAYER_SETUP.md` were resolved by keeping both sides; in `PLAYER_SETUP.md` the two sessions' Orlog lines were combined, and the new skill settings were added to master's settings table. After the merge all three plugins build against stand-in game assemblies and **262 tests pass**. Earlier work from this branch is already in `master` (PRs #8, #9, #16).

Detail lives elsewhere; this file points to it:
- `FEATURES.md`: Levels and XP, Attributes and class eitr, Orlog stone and class choice, Skill tree and skill points.
- `docs/class-design.md`: the design.
- `docs/ROADMAP.md` §4.1: tasks C1–C7.
- `docs/DECISIONS.md`: rows dated 2026-10-30 to 2026-11-01.
- `docs/PLAYTEST.md`: the Orlog sections.
- `docs/classes-ui-brief.md`: the contract with the UI session.

## Purpose

The classes session's branch: **Orlog**, the Diablo 2-style classes plugin. It covers levels and XP, attributes and the class eitr pool, the Orlog stone, the skill trees and, one by one, what each skill does. The Amazon comes first; other classes are extra content after this stretch.

## Stage

**C1 and C2 done and tested**, PR #20 into `master`. The Orlog stone works at about twice a trophy stone; its custom design comes later. **Asking you questions** before C3 (the Amazon's passives).

## Done (built and tested in-game by you, single player)

All in `master`:
- **Levels and XP** (PR #9):
  - Diablo 2's experience table, up to level 99.
  - XP from kills. Creature levels come from your levelling guide (bosses) and from the biome a creature dies in (everything else). Starred creatures give ×2 or ×3.
  - Diablo 2's level-difference cut for lower creatures. Classic WoW's rule for higher ones (your own level's XP, up to +20%), after your pace feedback.
  - Players within 70 m share a kill; this part is untested.
  - XP is saved with the character. The `orlog_info`, `orlog_xp` and `orlog_level` commands work.
- **Attributes and class eitr** (PR #16):
  - Strength, Dexterity, Vitality and Energy, with 5 stat points per level.
  - Health and stamina per level and per Vitality point (D2's numbers scaled by Valheim's starting values).
  - +1% damage per Strength point (melee and thrown) and per Dexterity point (bows).
  - The class eitr pool: 15 to start, +1.5 per level, +1.5 per Energy point. Vanilla regen refills it without eitr food.
  - Respec, and the classes UI showing real data.

- **Skill tree and skill points** (C2): built 2026-11-01, **all 9 playtest steps passed 2026-11-02**. Not in `master` yet; on this branch. Covers:
  - the Amazon's real tree in the skills panel;
  - D2's spending rules, with refusals that give their reason;
  - tooltips with the eitr cost;
  - respec, saving and `orlog_skill`.

## Half-done

- **Roadmap C1, the Orlog stone:** failed its first playtest 2026-11-02 and was reworked the same day. Not re-tested yet.
  - **Look (your choices):** a copy of the game's Vegvisir, with every vanilla behaviour stripped. About twice a trophy stone's height (`StoneSize` 1.7 × the Vegvisir), with glowing blue runes. No mist (removed; for the redesign).
  - **Placement:** 3 m behind the Eikthyr trophy stone, turned like it. A stone from the first version moves there.
  - **Raven:** on the ground beside the stone.
  - **Settings:** `StoneSize`, `StoneGap` and `StoneTurn`.
  - **Use:** opens the class choice window (real data). Without CraftingOverhaul, using the stone twice picks the Amazon.
- **Roadmap C2, the skill tree and skill points:** built 2026-11-01, not tested.
  - **The tree:** the Amazon's 30 skills (19 can take points, 11 later) with D2's spending rules: tier levels, built prerequisites, 20 points per skill, one more point per level past the requirement.
  - **Eitr cost:** D2's mana at the skill's level.
  - **Saving and commands:** points saved with the character; respec returns them; `orlog_skill` spends from the console.
  - **What the skills do is not built yet** (C3 onwards).
- **Data for C3 onwards:** `data/d2-reference/amazon_levels.csv` holds every Amazon skill's values for levels 1–20, from the Arreat Summit and D2R, with the differences noted in its README.

## Broken or unknown

- **Untested in-game:**
  - **C1:** the trophy stone prefab, the stone's floor height and spot in the ring, the raven, and the server confirmation.
  - **C1:** the first round's crash and the second round's freeze weren't seen again; their causes aren't known. The look will be redesigned.
- **Untested anywhere: multiplayer.** XP sharing with a friend, the stone placed through a dedicated server, levels seen by other players. These are in `PLAYTEST.md` → Session 2.
- **Known limits:**
  - A boss counts as one creature of its level for XP.
  - Kall Fimbulbringer's three phases each count as a kill.
  - A kill finished by fall damage or another creature gives no XP.
  - The raven shows once even for a character that already has a class.
- **Assumptions:**
  - The stamina scale uses Valheim's starting stamina from the game code's default (75). If the player prefab sets another value, the scale is off.
  - Valheim 1.0.21 runs Unity 6000.0.75 (from your log), so new animation clips need `JotunnModUnity/` on Unity 6.

## Decisions

Everything you decided in this session; the reasons are as you gave them.

**Setup and scope (2026-10-30):**
- **A separate part of the mod** for Diablo 2 classes. Build only as much of a class as works without breaking the game. Levels, with points to spend on stats and skills each level.
- **Look at EpicMMOSystem first.** Then: **we build our own levels**, not EpicMMOSystem.
- **New animations are allowed where necessary**, which lifts the old "no new animations" rule. Vanilla animations and emotes still come first.
- **The Amazon first** (your group's vote), then the Barbarian, maybe the Paladin. Since 2026-11-01, other classes are extra content after everything else.

**Levels and points:**
- **Follow Diablo 2's rules**, each changeable in config.
- **No XP loss on death**; Valheim's own skill loss stays (you remembered it as 10%).
- **Vanilla skills keep levelling by use.**
- **Levelling is slow, like classic WoW.** Your levelling guide: Eikthyr 5, Elder 11, Bonemass 20, Moder 35, Yagluth 50, Queen 62, Fader 76, Kall 90.
- **Pace fix, option B (2026-10-30):** a creature above your level gives your own level's XP plus WoW's bonus, up to +20%. "We can always adjust the multiplier later."

**Skill tree:**
- **D2's three tabs, not one D4 tree.** Skills open at their level tier instead of through broken prerequisite chains. Upgrade choices on the big skills come later. (You agreed with my points 1 and 2.)

**Casting:**
- **D2's layout, the same for every weapon:** left click is the normal attack, right click the skill attack.
- **Block and parry move to another key**, rebindable. The skill button is rebindable too, because many players have mice with side buttons.
- **A 4-slot skill bar**, filled by dragging from the skills panel. The number keys 1–0 stay for weapons and items. Four skills at a time is enough.

**Eitr:**
- **Skills that cost mana in D2 cost eitr.** Eitr needs reworking because it is punishing now.
- **Class eitr pool:** 15 to start, +1.5 per level, +1.5 per Energy point, scaled to Valheim; eitr food adds on top.
- **Eitr regenerates without eitr food.**
- **Eitr potions** go in the quick slots; loot affixes add eitr.

**The Amazon:**
- **Javelin skills throw a spectral copy.**
- **Penetrate ignores the creature's resistance to the hit.**
- **Decoy and Valkyrie are tinted clones of the player character.**

**The Orlog stone:**
- **A class is chosen at the spawn area**, where the boss trophies go, at a custom stone that blends in with the area's stone structures.
- **Changing class** at the stone for a cost comes later; it's not important now.

**Names:**
- **Keep D2's class and skill names.** The stone is the **Orlog stone**. The plugin name doesn't matter (working name Orlog).

**UI:**
- **The UI session builds the XP bar, skills panel and skill slots.**
- **HUD bars together at the bottom centre, Elden Ring style**, in Valheim's colours. You chose health, eitr, stamina, top to bottom; your later playtest with the UI session changed it to health, stamina, eitr.
- **The four skill slots as a cross at the bottom left.**

**Orlog stone rework (2026-11-02, after your playtest):**
- **Placement:** a few metres behind the Eikthyr stone; we settled on a **3 m gap**.
- **Look:** much larger (**2× a trophy stone**), a **Vegvisir** base, and a "smokey blue outline", which you clarified as **both** blue mist and glowing blue runes.
- **Raven:** on the ground next to the stone, reachable.

**Process:**
- **Phone log:** keep the playtest log in step with `PLAYTEST.md` (you asked whether I used it).
- **2026-11-01: proceed with C1 then C2,** spawning agents where useful.

## Playtest feedback

- **Levels and XP (2026-10-30):** every step passed.
  - "Levelling feels too fast": 3 greydwarves and 1–2 necks reached level 2. Cause: most Meadows enemies are level 4, and D2 paid their full XP. Fixed with option B and re-tested: "pace feels better".
- **Attributes and class eitr (2026-10-31, play-test log):** all 11 steps passed.
  - "Double click sounds when spending points". It was the UI's buttons; the UI session fixed it in `master` ("Buttons play one click instead of two").
  - Your `LogOutput.log` showed Valheim 1.0.21 on Unity 6000.0.75, recorded as a correction in `DECISIONS.md`.
- **Skill tree (2026-11-02):** all 9 steps passed.
- **Orlog stone (2026-11-02), failed. What you saw and what changed:**
  - **Bonemass carving, touching the Bonemass stone, facing out of the circle:** it's now a Vegvisir behind the Eikthyr stone, turned like it.
  - **Hover read "Sacrificial Stone / [E] Read", with vanilla lore on E:** every vanilla hover and use behaviour is now stripped from the copy.
  - **Pressing E crashed the game once:** that came from the vanilla lore-stone behaviour, now removed. The cause is unconfirmed without a log.
  - **Raven unreachable on top:** it's now on the ground beside the stone.
- **Orlog stone re-test (2026-11-02):** the spot, raven, hover and class choice passed. Three findings and what changed:
  - **About 10× too big:** heights are now measured on each stone's main mesh. Fixed.
  - **No blue mist:** it now copies a vanilla smoke effect. Fixed.
  - **Froze on the second E:** each step is now logged, and I asked you for `LogOutput.log` and `Player.log` before restarting the game.
- **Third round (2026-11-02):** works: the window, choosing the Amazon, relogging, no errors, no freeze. Still 15.5 m (the trophy stone measured 7.4 m), and no mist (the copy was an indoor dust effect). You: "still not correct but it at least works. I think we will redesign this in the future". So `StoneSize` is now a plain multiple of the Vegvisir (1.7), and the mist is removed.

## Open questions

Before C3, the Amazon's passives:
1. **Passive chances:** use D2's unchanged?
   - Critical Strike: 16% at level 1 → 68% at level 20.
   - Dodge: 18% → 56%.
   - Avoid: 24% → 65%.
   - Evade: 18% → 56%.
2. **Penetrate's chance:** in D2 it raises attack rating, so no number fits. Options: Critical Strike's curve, Pierce's (23 → 85%), or your own. Does it also break full immunity?
3. **Critical Strike and backstabs:** should a crit double a backstab, or should backstabs not crit?
4. **Where D2 changed numbers in later patches:** use the current D2R values (recommended; the tree data already uses them) or the older Arreat Summit ones?

From the roadmap (§4.1):
5. **Boss XP:** a boss counts as one creature of its level today; D2's act bosses give 9–65× a normal monster. What multiplier?
6. **Bonus skill points** for each boss's first kill: how many? D2 gives 12 in total from quests.
7. **Eitr regen without eitr food:** keep vanilla's 5 per second, or set a rate?
8. **Eitr potions for the early biomes:** which materials, and do they belong to Orlog?

## Next steps

1. **PR #20 into `master`** for C1 + C2: everything passed; waiting for you to merge.
2. **Later:** redesign the stone's look (your call, 2026-11-02).
3. **C3, the Amazon's passives** (Critical Strike, Dodge, Avoid, Evade, Penetrate), once questions 1–4 are answered. Before patching, agree the `Character.Damage` / `RPC_Damage` order with Corruption's prefixes in `mod-compatibility.md`.
4. **C4, boss bonus points and the respec item,** once question 6 is answered. The respec item is crafted from boss essences when Corruption is installed.
5. **C5, the skill button, slots and bow skills.** Prototype the input first (drawing a bow on a non-attack button, block moved to its own key).

## Merge notes

- **Ready to merge:** C1 works and C2 passed; PR into `master`. 262 tests pass.
- **No other session's code is touched.** Orlog's own files are `Orlog/`, `tools/make_orlog_tables.py`, `data/d2-reference/` and the `CraftingOverhaul.Tests/Orlog*Tests.cs` files.
- **Shared docs** (add only, keep both sides in conflicts): `FEATURES.md`, `docs/CURRENT_STATE.md`, `docs/DECISIONS.md`, `docs/PLAYTEST.md`, `docs/ROADMAP.md`, `docs/PLAYER_SETUP.md`, `docs/ARCHITECTURE.md`, `docs/mod-compatibility.md`, `docs/class-design.md`, `docs/classes-ui-brief.md`, and `CraftingOverhaul.Tests/CraftingOverhaul.Tests.csproj`, where each session adds its linked files.
- **Contract with the UI session** (`docs/classes-ui-brief.md` §3): `GetClassStoneRequest`, `ChooseClass`, `GetSkillIdsJson`, `GetSkillInfoJson` and `SpendSkillPoint` are now real. The UI's demo code and `co_classes_stone` can go once Orlog serves everything (C5 fills the skill bar calls).
- **Game methods patched by more than one session** (`docs/mod-compatibility.md`):
  - `Game.Start` (postfix): with Corruption.
  - `Player.GetTotalFoodValue` (postfix): with Corruption's loot. Both add, so they stack.
  - `SEMan.ModifyAttack` (postfix).
  - `Character.OnDeath` (prefix): Corruption's patches are on `Character.Damage` and `RPC_Damage`, and on `CharacterDrop.OnDeath`.
