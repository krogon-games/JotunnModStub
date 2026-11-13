# Play-tests: `claude/biome-load-freeze`

## Round: no freeze after spawning (2026-11-13)

**What changed:** after you spawned, the mod worked out which biome every crafting material belongs to (for the "By biome" recipe grouping), and the game stood still for 18–22 seconds while it did. Now it saves the answer to a file and reuses it on the next load, and when it has to work it out again it does it in the background, a little each frame, so you can keep playing. Until it's ready, the recipe list shows the game's normal order without biome headers, and switches by itself when it's done.

**Build:** CraftingOverhaul from `claude/biome-load-freeze`. The log line is `CraftingOverhaul 0.3.0 loaded (build: claude/biome-load-freeze <commit>)`.
**Setup:** single player, any world, your usual mods. Recipe grouping set to **By biome** (the button right of the recipe search box). The first time, there is no saved file yet; to repeat the first-time test later, delete `BepInEx/cache/CraftingOverhaul.biomes.txt` with the game closed.

### Checklist
1. [ ] **No freeze:** load the world. As soon as you appear, walk and turn the camera → no long freeze (before, about 20 s). Note any short stutters in the first minute.
2. [ ] **List while it works:** straight after spawning, open a workbench → the recipes are in the game's normal order with no biome headers. Keep it open → after a while it switches to biome headers by itself. Roughly how long did that take?
3. [ ] **Biomes still right:** the headers look as before (e.g. bronze items under Black Forest, iron under Swamp, silver under Mountain). In the console (F5), `co_biome Bronze` → "Bronze: BlackForest" with the explanation underneath.
4. [ ] **Saved file:** quit to the main menu, load the world again, open the workbench → biome headers straight away.
5. [ ] **Refresh:** in the console, `co_biome refresh` → a short pause, then "Biomes recalculated."; the headers are still right.
6. [ ] **Log:** send me these lines from `BepInEx/LogOutput.log`: the ones starting `Working out item biomes`, `Resolved biomes for` and `Loaded biomes for`. No red errors from CraftingOverhaul.

### Your feedback
*(not yet)*
