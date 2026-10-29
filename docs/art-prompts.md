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

The eight maps from `Corruption/Data/maps.csv`. Their in-game text says they are "drawn in ash on a slain guardian's hide", so: one family of small rolled hide maps, same hide, same tie, each with its biome drawn in ash and charcoal and a glowing red rip mark (the rips are red in game). A small bead tied on the cord takes the colour of the essence the map is made from, so the map matches its essence at a glance.

**How to use:** attach the essence render (`docs/images/essences_reference.jpg`) and the orb render (`docs/images/orbs_reference.jpg`) as references for style, camera, lighting and background, paste the sheet prompt, then refine single maps with the single prompts. Keep the background plain and even: the icons are cut out of it.

### Sheet prompt (all eight in one image)

```
Concept art sheet of eight magical maps for a Viking survival game, in the stylised, slightly low-poly painterly look of Valheim. Match the attached item renders for style, camera, lighting and background. Each map is a small piece of rough tanned hide, half unrolled so the drawing shows, rolled up at one end and tied with a thin leather cord. The map is drawn in black ash and charcoal: a simple, bold sketch of one land, readable at small icon size. In the middle of every drawing is the same mark: a jagged tear that glows deep red, like a wound in the hide, with faint red light bleeding into the hide around it. A single small glowing bead hangs from each cord in its own colour. They are clearly one family: same hide, same size, same cord, same red tear; only the drawing and the bead change.

Lay them out in a 4 by 2 grid on a plain, even dark slate-grey background, evenly spaced, each centred in its own square with empty space around it, same three-quarter top-down camera angle, same soft key light from the upper left, a subtle glow halo, no text, no letters, no labels, no hands, no other objects. Left to right, top row then bottom row:

1. Meadows: gentle rolling hills, a few round oak trees, a small stag-antler symbol. Bead: pale icy blue-white.
2. Black Forest: a dense wall of pointed fir trees and a ruined stone tower. Bead: amber-green.
3. Swamp: twisted dead trees standing in water and a sunken crypt doorway. Bead: toxic green.
4. Mountains: sharp snowy peaks with a dragon's wing shape above them. Bead: cold glacier blue.
5. Plains: flat open grassland, a tar pit and a ring of sharpened stakes. Bead: ember orange.
6. Mistlands: jagged rock spires in swirls of mist and a giant ribcage. Bead: violet.
7. Ashlands: a volcano with rivers of lava; this hide is singed and charred at the edges. Bead: molten orange.
8. Deep North: glaciers, icebergs and cracked sea ice; this hide has frost on its edges. Bead: icy teal.

Game item render, readable at small icon size, strong silhouettes, high detail, 2:1 image.
```

### Single prompts (refine one map)

Start each with:

```
A single magical map item for a Viking survival game, in Valheim's stylised, slightly low-poly painterly look, matching the attached item renders for camera, lighting and background. A small piece of rough tanned hide, half unrolled, rolled up at one end and tied with a thin leather cord, drawn in black ash and charcoal with a bold, simple sketch of one land. In the middle of the drawing a jagged tear glows deep red like a wound in the hide. One small glowing bead hangs from the cord. Centred on a plain, even dark slate-grey background, three-quarter top-down view, soft key light from the upper left, subtle glow halo, no text, no letters, no other objects.
```

then add one line:

| Map | Add |
|---|---|
| Meadows | `The drawing shows the Meadows: gentle rolling hills, a few round oak trees, a small stag-antler symbol. The bead is pale icy blue-white.` |
| Black Forest | `The drawing shows the Black Forest: a dense wall of pointed fir trees and a ruined stone tower. The bead is amber-green.` |
| Swamp | `The drawing shows the Swamp: twisted dead trees standing in water and a sunken crypt doorway. The bead is toxic green.` |
| Mountains | `The drawing shows the Mountains: sharp snowy peaks with a dragon's wing shape above them. The bead is cold glacier blue.` |
| Plains | `The drawing shows the Plains: flat open grassland, a tar pit and a ring of sharpened stakes. The bead is ember orange.` |
| Mistlands | `The drawing shows the Mistlands: jagged rock spires in swirls of mist and a giant ribcage. The bead is violet.` |
| Ashlands | `The drawing shows the Ashlands: a volcano with rivers of lava. The hide is singed and charred at the edges. The bead is molten orange.` |
| Deep North | `The drawing shows the Deep North: glaciers, icebergs and cracked sea ice. The hide has frost on its edges. The bead is icy teal.` |

### Notes

- Bead colours follow each essence's glow colour in `Corruption/Data/essences.csv` (Eikthyr #8CD9FF … Kall #40E6D9), named in words because image models follow names better than hex codes.
- Landmarks are well-known sights of each biome (game knowledge, not checked against the game files); change any you like.
