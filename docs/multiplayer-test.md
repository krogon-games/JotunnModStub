# Multiplayer test: craft and build from nearby chests

Two players, about 30 minutes. Mostly checks chest sharing, the part most likely to go wrong online.

## Setup

1. **Both players need** BepInExPack Valheim and Jötunn (r2modman or Thunderstore Mod Manager is easiest), plus the **same build** of `CraftingOverhaul.dll`.
   - The host's Debug build copies the dll to `<Valheim>\BepInEx\plugins\CraftingOverhaul\CraftingOverhaul.dll`. Send that file to your friend; they put it in the same folder in their install.
   - If the versions differ, the game refuses the connection. That's expected.
2. **Host** a world from the main menu ("Start server"), or use a dedicated server with the mod installed. Your friend joins.
3. Both open the console (F5) once and check `BepInEx/LogOutput.log` has `CraftingOverhaul 0.1.0 loaded`.
4. Build a test spot: a workbench, 3 chests within 20 m of it, and a bit of wood, stone and leather scraps in the chests (not in either player's inventory).

## Tests

Write down the result of each step (✅/❌ and what you saw).

| # | Who | Do this | Expected |
|---|---|---|---|
| 1 | Friend | Craft something at the workbench using only materials in the chests. | Crafts. The materials disappear from the chests. |
| 2 | Host | Open each chest. | Contents match what the friend took: nothing missing or duplicated. |
| 3 | Host | Do test 1 yourself. Friend then opens the chests. | Same as tests 1–2, the other way round. |
| 4 | Both | Friend builds a wood piece with the hammer using wood only in a chest. | Builds. Wood leaves the chest, and the host sees the same. |
| 5 | Host + friend | Host **opens and keeps open** the only chest with wood. Friend tries to craft something needing that wood. | The friend's recipe shows it can't be crafted (that chest is skipped while someone has it open). Host closes it → friend can craft. |
| 6 | Host | Build a **personal chest** (the private one, `piece_chest_private`), put wood in it, and empty the other chests. Friend tries to craft. | Friend can't use the private chest's wood. Host can. |
| 7 | Host | Place a **ward** near the bench, activated, without adding the friend. Friend tries to craft from chests inside it. | Friend can't use those chests. Add the friend to the ward → they can. |
| 8 | Both | Both craft from the **same chest at the same moment** (count down, click together), a few times. | Ideally no lost or duplicated items. A known race: if items go missing or double, note how often. |
| 9 | Friend | Walk 30+ m away from the chests and try crafting at a second workbench there. | Far chests aren't used (range 20 m). |
| 10 | Host | Close the game. In `BepInEx/config/com.krogongames.craftingoverhaul.cfg` set `[2 - Nearby Containers] Enabled = false`, restart and host again. Friend joins and tries test 1. | Friend can't use chests: the host's setting overrides theirs. Set it back to `true` afterwards. |
| 11 | Optional | Friend removes `CraftingOverhaul.dll` and joins. Host crafts from chests while the friend watches the chest contents. | Friend can join and play normally (vanilla UI). The chest contents they see update correctly. |

## Send back

- The table with ✅/❌ and notes.
- **Both** players' `BepInEx/LogOutput.log` from the session (they're overwritten on the next game start, so copy them right after).
- Whether you played hosted-from-a-PC or on a dedicated server, and roughly where each player is (same house, same country, far apart).
