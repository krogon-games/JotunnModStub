# Art prompts

Prompts for concept renders made with an image model (Nano Banana / Gemini image). The renders are **drafts to agree a look**; the in-game models are then built by scripts in `tools/` (see `ARCHITECTURE.md` → Custom assets), so keep designs to shapes a simple faceted model, a texture and a glow can carry.

## Boss essences (design phase 3)

Each boss drops its essence, the key ingredient for maps into the corrupted version of its own biome (`corruption-design.md` → Boss essences). Kall Fimbulbringer's essence was added later without a render. The seven should read as **one family** (same size, same rough-cut gem shape as the corruption shard, same camera and lighting) but each clearly belongs to its boss.

**How to use:** attach the corruption shard picture (`docs/images/shard_preview.png` or an in-game screenshot) as a reference image, paste the sheet prompt, then use the single prompts to refine any essence you don't like. Ask for a change in plain words ("make Bonemass greener and wetter, fewer bones").

### Sheet prompt (all seven in one image)

```
Concept art sheet of seven magical gemstones for a Viking survival game, in the stylised, slightly low-poly painterly look of Valheim. Each gem is the "essence" of a slain boss. Use the attached red gemstone as the shared base: every essence has the same size, the same rough-cut, egg-shaped faceted silhouette with a small flat top facet, and glows faintly from inside. They are clearly one family of items, but each takes its colours, inner light and a small surface detail from its boss.

Lay them out in a single row on a plain dark slate-grey background, evenly spaced, each centred in its own square, same three-quarter camera angle, same soft key light from the upper left, a subtle glow halo, no text, no labels, no hands, no other objects. Left to right:

1. Essence of Eikthyr - storm stag of the Meadows: pale icy blue-white crystal with crackling lightning veins inside, two tiny antler-tip spikes growing from the top facet.
2. Essence of the Elder - ancient walking tree of the Black Forest: deep amber-green crystal like hardened sap, bark-brown streaks and moss at the base, a thin root wrapped once around it.
3. Essence of Bonemass - rotting giant of the Swamp: murky toxic-green crystal, sickly glow, cloudy swirls inside, a small fragment of yellowed bone embedded in one facet.
4. Essence of Moder - frost dragon of the Mountains: clear glacier-blue ice crystal, frost crust on the lower facets, a faint dragon-scale pattern on the surface, cold white inner light.
5. Essence of Yagluth - fallen fire king of the Plains: charred black crystal with smouldering orange ember cracks, a thin ring of tarnished gold like a broken crown fragment around the middle.
6. Essence of the Queen - insect queen of the Mistlands: violet-black chitin-like crystal with an iridescent purple and teal sheen, glowing blue-violet eitr veins.
7. Essence of Fader - fire dragon of the Ashlands: black obsidian crystal split by bright molten-orange lava seams, flecks of glowing ash on the surface.

Game item render, readable at small icon size, strong silhouettes, high detail, 16:9.
```

### Single prompts (refine one essence)

Start each with:

```
A single magical gemstone item for a Viking survival game, in Valheim's stylised, slightly low-poly painterly look. Same size and rough-cut, egg-shaped faceted shape with a small flat top facet as the attached red gemstone, glowing faintly from inside. Centred on a plain dark slate-grey background, three-quarter view, soft key light from the upper left, subtle glow halo, no text, no other objects.
```

then add one line:

| Essence | Add |
|---|---|
| Eikthyr | `It is the Essence of Eikthyr, the storm stag: pale icy blue-white crystal with crackling lightning veins inside and two tiny antler-tip spikes on the top facet.` |
| The Elder | `It is the Essence of the Elder, the ancient tree: deep amber-green crystal like hardened sap, bark-brown streaks, moss at the base, a thin root wrapped once around it.` |
| Bonemass | `It is the Essence of Bonemass, the rotting swamp giant: murky toxic-green crystal with a sickly glow and cloudy swirls inside, a small fragment of yellowed bone embedded in one facet.` |
| Moder | `It is the Essence of Moder, the frost dragon: clear glacier-blue ice crystal, frost crust on the lower facets, faint dragon-scale pattern, cold white inner light.` |
| Yagluth | `It is the Essence of Yagluth, the fallen fire king: charred black crystal with smouldering orange ember cracks and a thin ring of tarnished gold like a broken crown fragment.` |
| The Queen | `It is the Essence of the Queen, the insect queen: violet-black chitin-like crystal with an iridescent purple and teal sheen and glowing blue-violet veins.` |
| Fader | `It is the Essence of Fader, the fire dragon of the Ashlands: black obsidian crystal split by bright molten-orange lava seams, glowing ash flecks on the surface.` |

### Notes for turning the renders into models

- **Easy for the generator:** colour, inner glow colour, veins and cracks, frost or moss on lower facets, surface patterns (all texture and glow).
- **Possible but simple only:** small attachments (antler tips, a root, a crown ring, a bone fragment) as a few extra faceted pieces.
- **Not possible:** fine sculpted detail, transparency with real refraction, animated effects other than a glow light.
- Boss look notes are from game knowledge of the vanilla bosses, not checked against the game files.

## Currency orbs (design phase 7)

*Render received 2026-10-28 (`docs/images/orbs_reference.jpg`); models made from it by `tools/make_orbs.py`. It differs from the prompt in places and the models follow the render: Regal's rim goes all round, Annulment is broken open at the top rather than its rune being scratched out, and Jeweller's has sockets all round.*

The nine orbs from `corruption-design.md` → Currency orbs. They should match the essences (same camera, lighting, background, size class and stylised Valheim look) but read as a **different family**: round, hand-made objects of stone and metal, not gems. Valheim, not Path of Exile: no polished glass marbles or sci-fi glow; think worn, carved, rune-marked objects a Viking smith or seer would make.

**Visual language (so the use reads at a glance):**
- **Material shows rarity**, following Valheim's metal progression: stone and wood (common) → bronze and iron (uncommon) → silver and gold (rare).
- **Rune glow colour shows the result:** blue = magic, gold-yellow = rare, white-gold = rerolls, grey-violet = removes, mixed colours = chance.
- **Every orb carries one carved Elder Futhark rune** that glows. Runes picked for meaning: Transmutation ᛇ Eihwaz (change), Augmentation ᚢ Uruz (strength), Alchemy ᚷ Gebo (gift), Regal ᛟ Othala (inheritance), Exalted ᛏ Tiwaz (victory), Divine ᛞ Dagaz (dawn, renewal), Annulment ᚾ Naudiz (need, loss), Chance ᛈ Perthro (fate, the lot cup), Jeweller's ᛃ Jera (harvest).

**How to use:** attach the essence render (`docs/images/essences_reference.jpg`) as a reference for style, camera and lighting, paste the sheet prompt, then refine single orbs with the single prompts. Ask for changes in plain words ("make Exalted brighter, less gold trim").

### Sheet prompt (all nine in one image)

```
Concept art sheet of nine magical crafting orbs for a Viking survival game, in the stylised, slightly low-poly painterly look of Valheim. Match the attached sheet of gemstones for style, camera, lighting and background, but these are a different family: each orb is a roughly round, hand-made sphere about the size of a fist, slightly hand-hewn rather than perfectly smooth, with large flat facets like a low-poly ball. Each has one Norse rune carved deep into its front face, and the rune glows from inside. Materials follow Valheim's progression: worn stone and wood for common orbs, bronze and iron for uncommon, silver and gold for rare. No glass, no polished marbles, no sci-fi glow: worn, carved, rune-marked objects a Viking smith or seer would make.

Lay them out in a 3 by 3 grid on a plain dark slate-grey background, evenly spaced, each centred in its own square, same three-quarter camera angle, same soft key light from the upper left, subtle glow halo, no text, no labels, no hands, no other objects. Left to right, top to bottom:

1. Orb of Transmutation: worn grey river stone, rune Eihwaz carved in the front, glowing soft blue.
2. Orb of Augmentation: grey stone like the first, bound by one thin band of bronze around the middle, rune Uruz glowing blue.
3. Orb of Alchemy: dark stone split by thin seams of molten gold-yellow light, rune Gebo glowing gold-yellow.
4. Regal Orb: dark iron sphere with rivets and a small crown-like rim of bronze points on top, rune Othala glowing gold-yellow.
5. Exalted Orb: bright Mountain silver with fine engraved knotwork, rune Tiwaz glowing radiant gold, a little brighter than the others.
6. Divine Orb: pale gold and white bone, engraved with a Valknut, rune Dagaz glowing warm white-gold light.
7. Orb of Annulment: hollow-looking blackened iron, cracked, the rune Naudiz scratched through as if cancelled, faint cold grey-violet glow.
8. Orb of Chance: carved bone and dark wood in alternating facets, like a gambling lot, rune Perthro in the front and small runes on other facets glowing different colours (blue, gold, green, red).
9. Jeweller's Orb: polished dark stone with three small round empty gem sockets set in bronze around it, rune Jera glowing soft teal.

Game item render, readable at small icon size, strong silhouettes, high detail, square 1:1 image.
```

### Single prompts (refine one orb)

Start each with:

```
A single magical crafting orb for a Viking survival game, in Valheim's stylised, slightly low-poly painterly look, matching the attached item renders for camera, lighting and background. A roughly round, hand-hewn sphere about the size of a fist, with large flat facets, one Norse rune carved deep in its front face glowing from inside. Worn, carved and hand-made, not glass or polished. Centred on a plain dark slate-grey background, three-quarter view, soft key light from the upper left, subtle glow halo, no text, no other objects.
```

then add one line:

| Orb | Add |
|---|---|
| Transmutation | `It is the Orb of Transmutation: worn grey river stone, the rune Eihwaz glowing soft blue.` |
| Augmentation | `It is the Orb of Augmentation: grey stone bound by one thin bronze band around the middle, the rune Uruz glowing blue.` |
| Alchemy | `It is the Orb of Alchemy: dark stone split by thin seams of molten gold-yellow light, the rune Gebo glowing gold-yellow.` |
| Regal | `It is the Regal Orb: dark riveted iron with a small crown-like rim of bronze points on top, the rune Othala glowing gold-yellow.` |
| Exalted | `It is the Exalted Orb: bright silver with fine engraved knotwork, the rune Tiwaz glowing radiant gold, brighter than ordinary orbs.` |
| Divine | `It is the Divine Orb: pale gold and white bone engraved with a Valknut, the rune Dagaz glowing warm white-gold.` |
| Annulment | `It is the Orb of Annulment: cracked, blackened iron that looks hollow, the rune Naudiz scratched through as if cancelled, a faint cold grey-violet glow.` |
| Chance | `It is the Orb of Chance: alternating facets of carved bone and dark wood like a gambling lot, the rune Perthro in front and small runes on other facets glowing blue, gold, green and red.` |
| Jeweller's | `It is the Jeweller's Orb: polished dark stone with three small empty round gem sockets set in bronze, the rune Jera glowing soft teal.` |

### Notes for turning the renders into models

- One shared faceted sphere model, like the shared essence gem; each orb is a texture (material, carved rune, cracks, knotwork) plus a glow mask and colour.
- Simple extra parts are possible, as with the Elder's root and Yagluth's ring: the bronze band, the Regal crown rim, the Jeweller's sockets.
- Fine engraving (knotwork, the Valknut) becomes painted texture lines, so it will be softer than in the render.
- Rune meanings are common Elder Futhark readings, chosen for flavour.

## Corrupted maps (design phase 4)

*Asked for 2026-10-29: the maps' placeholder icon (a tinted bundle of leather scraps) is hard to read. The render is cut into the eight inventory icons directly; the map's look on the ground stays the leather bundle for now.*

*Second render received 2026-10-29 (`docs/images/maps_reference.jpg`) and used for the icons (`tools/make_map_icons.py`, preview `docs/images/map_icons_preview.png`); at 64 px all eight read apart.*

*First render (2026-10-29) rejected by the user: "the maps look too similar, a lot of the fine details will get lost". Shrunk to inventory size (64 px), all eight were the same beige scroll with a red mark; only the hide colour, the red tear and the Ashlands lava still showed, and the ash drawings disappeared. The prompt below is the second version, built for small icons:*
- *each map has its own strong colour over the whole hide (colour is what reads at 64 px);*
- *one huge, bold silhouette instead of a scene;*
- *a different chunky material for the roller ends;*
- *the shared red tear is smaller and in a corner, so it stops making them all look alike.*

The eight maps from `Corruption/Data/maps.csv`. Their in-game text says they are "drawn in ash on a slain guardian's hide": each is a hide stained by its land, with one bold mark in ash. A small red tear (the rips are red in game) marks them all as corrupted, and the bead on the cord takes the colour of the essence the map is made from.

**How to use:** attach the essence render (`docs/images/essences_reference.jpg`) and the orb render (`docs/images/orbs_reference.jpg`) as references for style, camera, lighting and background, paste the sheet prompt, then refine single maps with the single prompts. Keep the background plain and even: the icons are cut out of it.

### Sheet prompt (all eight in one image)

```
Concept art sheet of eight magical maps for a Viking survival game, in the stylised, slightly low-poly painterly look of Valheim. Match the attached item renders for style, lighting and background. These will be used as tiny inventory icons (64 by 64 pixels), so every map must be recognisable from its colour and one big shape alone: no small details, no scenes, no thin lines.

Each map is a thick piece of hide on a short wooden roller, shown almost flat and facing the camera, filling most of its square. The whole hide is strongly stained in the map's own colour. On it is ONE huge, bold silhouette painted in thick black ash strokes, filling most of the hide. In one top corner, a small jagged tear glows deep red. The two ends of the roller are big chunky knobs made of the map's own material. A large round glowing bead hangs from a short cord. All eight have the same size, shape and layout; the colour, the silhouette, the roller ends and the bead are different on each, so no two maps look alike.

Lay them out in a 4 by 2 grid on a plain, even dark slate-grey background, evenly spaced, each centred in its own square, same camera, same soft key light from the upper left, no text, no letters, no labels, no hands, no other objects. Left to right, top row then bottom row:

1. Meadows: hide stained fresh light green. Silhouette: a stag's head with big antlers. Roller ends: antler tips. Bead: pale icy blue-white.
2. Black Forest: hide stained very dark blue-green, almost black. Silhouette: one giant pointed fir tree. Roller ends: gnarled roots. Bead: amber.
3. Swamp: hide stained murky brown-olive, wet and dripping. Silhouette: a big skull. Roller ends: yellowed bone. Bead: toxic green.
4. Mountains: hide white as snow with slate-grey shadows and grey wolf fur along the top edge. Silhouette: one jagged mountain peak. Roller ends: grey stone. Bead: glacier blue.
5. Plains: hide stained golden straw yellow. Silhouette: the head of a huge shaggy horned beast. Roller ends: gold rings. Bead: ember orange.
6. Mistlands: hide stained deep purple with pale mist swirls. Silhouette: a giant insect with long mandibles. Roller ends: shiny black chitin. Bead: bright violet.
7. Ashlands: hide charred black with glowing orange lava cracks. Silhouette: a volcano, glowing orange. Roller ends: black obsidian. Bead: molten orange.
8. Deep North: hide frozen inside a slab of glowing cyan-teal ice with icicles hanging below. Silhouette: a big iceberg. Roller ends: clear ice. Bead: icy teal.

Game item icons, bold, high contrast, strong silhouettes, 2:1 image.
```

### Single prompts (refine one map)

Start each with:

```
A single magical map item for a Viking survival game, in Valheim's stylised, slightly low-poly painterly look, matching the attached item renders for lighting and background. It will be a tiny 64 by 64 pixel inventory icon, so it must read from its colour and one big shape alone: no small details, no thin lines. A thick piece of hide on a short wooden roller, almost flat and facing the camera, filling most of the image. The whole hide is strongly stained in one colour, with ONE huge bold silhouette in thick black ash strokes. A small jagged tear glows deep red in one top corner. The roller ends are big chunky knobs, and a large round glowing bead hangs from a short cord. Centred on a plain, even dark slate-grey background, soft key light from the upper left, no text, no letters, no other objects.
```

then add one line:

| Map | Add |
|---|---|
| Meadows | `The hide is stained fresh light green; the silhouette is a stag's head with big antlers; the roller ends are antler tips; the bead is pale icy blue-white.` |
| Black Forest | `The hide is stained very dark blue-green, almost black; the silhouette is one giant pointed fir tree; the roller ends are gnarled roots; the bead is amber.` |
| Swamp | `The hide is stained murky brown-olive, wet and dripping; the silhouette is a big skull; the roller ends are yellowed bone; the bead is toxic green.` |
| Mountains | `The hide is white as snow with slate-grey shadows and grey wolf fur along the top edge; the silhouette is one jagged mountain peak; the roller ends are grey stone; the bead is glacier blue.` |
| Plains | `The hide is stained golden straw yellow; the silhouette is the head of a huge shaggy horned beast; the roller ends are gold rings; the bead is ember orange.` |
| Mistlands | `The hide is stained deep purple with pale mist swirls; the silhouette is a giant insect with long mandibles; the roller ends are shiny black chitin; the bead is bright violet.` |
| Ashlands | `The hide is charred black with glowing orange lava cracks; the silhouette is a glowing orange volcano; the roller ends are black obsidian; the bead is molten orange.` |
| Deep North | `The hide is frozen inside a slab of glowing cyan-teal ice with icicles hanging below; the silhouette is a big iceberg; the roller ends are clear ice; the bead is icy teal.` |

### Notes

- Bead colours follow each essence's glow colour in `Corruption/Data/essences.csv` (Eikthyr #8CD9FF … Kall #40E6D9), named in words because image models follow names better than hex codes.
- Hide colours were picked so no two maps share a hue at icon size; silhouettes are the biome's best-known sight or boss (game knowledge, not checked against the game files). Change any you like.
- Check a render by shrinking it to 64 px before accepting it.

## Corrupted offerings (design phase 6)

*Asked for 2026-10-29: each corrupted offering is the boss's normal offering made to look corrupted (user). Starting with the deer trophy (Eikthyr's altar takes 2 `TrophyDeer`, `altars.csv`; the corrupted one is crafted from both plus 1 shard). Three takes on "corrupted" to pick from before the other offerings follow.*

The mod's corruption look so far: the deep red of the corruption shard and the rips, a red glow from inside, cracks and tears that open onto the void (Ginnungagap), small touches of purple. Prompts follow the map icon lesson: the item has to read at 64 px, so one strong silhouette and bold colour, no fine detail.

*First three takes (void-cracked, crystal antlers, bound offering) rejected by the user 2026-10-29: "way too over the top. The new icon needs to look a lot closer to the original. Mostly recolouring and very small tweaks." The prompts below are edits of the vanilla icon, from least to most changed.*

**How to use:** attach only the vanilla Deer trophy icon (a screenshot of it in the inventory, cropped to the one slot) and run each prompt on it separately. These are edits: the model should change as little as possible.

### A. Recolour only

```
Edit the attached game icon. Keep it exactly the same: same deer trophy, same shape, pose, angle, outline, size, painting style and background. Change only the colours: shift the fur from light brown to a dark, deep red-brown, darken the antlers to a dark brown-grey with a faint red tint at the tips, and add a very soft, thin dark-red glow around the outline. Do not add anything, remove anything or redraw any part.
```

### B. Recolour and red eyes

```
Edit the attached game icon. Keep it exactly the same: same deer trophy, same shape, pose, angle, outline, size, painting style and background. Change only the colours: shift the fur from light brown to a dark, deep red-brown and darken the antlers to a dark brown-grey. The one small addition: the eyes glow a small, bright red. Do not add anything else, remove anything or redraw any part.
```

### C. Recolour and one thin crack

```
Edit the attached game icon. Keep it exactly the same: same deer trophy, same shape, pose, angle, outline, size, painting style and background. Change only the colours: shift the fur from light brown to a dark, deep red-brown and darken the antlers to a dark brown-grey. The one small addition: a single thin crack on the forehead that glows deep red. Do not add anything else, remove anything or redraw any part.
```

### Notes

- The same edit, with the same wording, then works for every other offering's vanilla icon, so they all match.
- Without any art, the mod could also recolour the vanilla icon itself in code (as it already tints corrupted bosses), which would match option A.
- Once a take is chosen, the same treatment applies to the other offerings in `altars.csv`: Ancient seed (the Elder), Withered bone (Bonemass), Fuling totem (Yagluth), Bell (Fader), Memorial coal and Hateful blood (the Deep North). Moder's altar has no offering in the dump (her dragon eggs go on item stands); check it before making hers.
