# Art prompts

Prompts for concept renders made with an image model (Nano Banana / Gemini image). The renders are **drafts to agree a look**; the in-game models are then built by scripts in `tools/` (see `ARCHITECTURE.md` → Custom assets), so keep designs to shapes a simple faceted model, a texture and a glow can carry.

## Boss essences (design phase 3)

Each boss drops its essence, the key ingredient for maps into the corrupted version of its own biome (`corruption-design.md` → Boss essences). The seven should read as **one family** (same size, same rough-cut gem shape as the corruption shard, same camera and lighting) but each clearly belongs to its boss.

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
