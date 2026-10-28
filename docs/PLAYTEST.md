# Play-test checklist

Everything built but not yet tested in-game, in one place, in the order that makes a session quick. Tick items off in the phone log or tell Claude the result; Claude then updates `FEATURES.md` and `CURRENT_STATE.md`. Remove a section here once it's tested.

**Phone version:** [Corruption Play-test Log](https://claude.ai/artifact/PyN39Y8QGCDPVJA8mrxjht) (private to the user). Pass / Fail / Skip and notes are saved in the page's database (`results` collection, one document per step id) for Claude to read with `ArtifactData`. Keep its steps in line with this file when either changes.

**Results so far:** the first session (2026-10-28) passed boss essences, corrupted bosses, talismans, craft from chests with AdventureBackpacks, category headers, `co_biome` and config reload; maps and rips worked apart from two bugs, fixed and confirmed in a re-test. Details: `FEATURES.md`.

**Before you start:** rebuild both mods (`CraftingOverhaul.sln`), start the game with EquipmentAndQuickSlots, AdventureBackpacks and PlantEasily installed (the group's setup), load a test world, open the console (F5) and type `devcommands`. `god` and `fly` help.

## Session 1c: solo (about 20 minutes)

### 1. Rip waves (step B)
1. `spawn CorruptionMap_Meadows`, use it and go through. A raid message shows and red boars and necks come at you in waves; some may have stars.
2. Wear a Boar or Neck charm: rip creatures still attack you.
3. Watch whether maddened creatures also fight each other.
4. Destroy the heart: the exit rip opens and a chest appears with Meadows chest loot plus 1–3 corruption shards; the waves stop.
5. Try one later biome, e.g. `spawn CorruptionMap_Swamp` (draugr and skeletons) or `CorruptionMap_Plains` (fulings).
6. No red errors in the console.

### 2. Bulk planting from chests
1. Bulk-plant a PlantEasily grid with the seeds only in a nearby chest: it plants and takes the seeds from the chest.

## Session 2: with a friend

Needs a second player (host a world or use the dedicated server, everyone on the same build).

1. **Craft and build from chests:** the 11 steps in [`multiplayer-test.md`](multiplayer-test.md).
2. **Talismans:** your friend sees creatures ignore you when you wear a charm.
3. **Rips:** you open a rip, your friend follows you through, you both come home through the exit rip. If a rip ever lands near anyone's base, tell Claude (the server should prevent that).
4. **Essences:** kill a boss together: it drops 1 essence.
