"""
Cuts the corrupted offerings' inventory icons out of the user's renders (docs/images/offerings/<Item>.jpg, 2026-10-30,
made with the prompts in docs/art-prompts.md -> Corrupted offerings: the vanilla icon recoloured, with a red crack and
a smoky red glow). The renders came back on different flat backgrounds (black, white, mid grey), so each is listed
with its own.

    python tools/make_offering_icons.py            (needs numpy and Pillow)

Writes, into Corruption/Assets/:
    offering_icon_<Item>.png     for viewing
    offering_icon_<Item>.rgba    the same pixels raw, which the mod loads (8-byte header, rows bottom to top)
and docs/images/offering_icons_preview.png: the icons at full size and at 64 px on the game's slot colour.

How the background goes: the area connected to the image's edges that is mostly background colour is made see-through
by "colour to alpha" (each pixel split into the background plus a coloured layer as opaque as it must be), so the red
glow and smoke stay as see-through red instead of a pink or black fringe. Everything inside that area stays opaque.
"""
import os

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCES = os.path.join(ROOT, "docs", "images", "offerings")
ASSETS = os.path.join(ROOT, "Corruption", "Assets")
PREVIEW = os.path.join(ROOT, "docs", "images", "offering_icons_preview.png")

# Item prefab -> background colour of its render.
RENDERS = {
    "TrophyDeer": (255, 255, 255),
    "AncientSeed": (0, 0, 0),
    "WitheredBone": (122, 122, 121),   # a grey square inside a black frame; the frame is cropped off first
    "DragonEgg": (255, 255, 255),
    "GoblinTotem": (0, 0, 0),
    "DvergrKey": (0, 0, 0),
    "Bell": (0, 0, 0),
    "HatefulBlood": (255, 255, 255),
}
SIZE = 128
# A pixel lets the background through (for finding what's connected to the edges) while it is less than this opaque.
PASSABLE = 0.5
# On black, dark parts of the item itself (stone, wood, outlines) look like background, so only near-black counts as
# outside, and the glow band just inside it fades from see-through to opaque over this share of the image width.
PASSABLE_ON_BLACK = 0.06
GLOW_BAND = 0.025
# Colour differences this small are JPEG noise, not item.
NOISE = 4.0


def colour_to_alpha(rgb, background):
    """Per pixel: how opaque a colour layer over the background must be to give this pixel, and that layer's colour."""
    bg = np.array(background, dtype=np.float64)
    c = rgb.astype(np.float64)
    room = np.where(c >= bg, 255.0 - bg, bg)            # how far each channel can move from the background
    room = np.maximum(room, 1.0)
    alpha = (np.maximum(np.abs(c - bg) - NOISE, 0.0) / room).max(axis=2)
    safe = np.maximum(alpha, 1e-4)[..., None]
    colour = np.clip(bg + (c - bg) / safe, 0, 255)
    return alpha, colour


def connected_to_edges(passable):
    reached = np.zeros_like(passable)
    reached[0, :] = reached[-1, :] = reached[:, 0] = reached[:, -1] = True
    reached &= passable
    while True:
        grown = reached.copy()
        grown[1:, :] |= reached[:-1, :]
        grown[:-1, :] |= reached[1:, :]
        grown[:, 1:] |= reached[:, :-1]
        grown[:, :-1] |= reached[:, 1:]
        grown &= passable
        if (grown == reached).all():
            return reached
        reached = grown


def crop_to_background(image, background):
    """Drops a frame of another colour around the render (the bone came inside a black frame)."""
    rgb = np.asarray(image).astype(np.int32)
    near = np.abs(rgb - np.array(background)).max(axis=2) < 12
    rows, cols = np.where(near)
    return image.crop((cols.min() + 2, rows.min() + 2, cols.max() - 1, rows.max() - 1))


def cut(path, background):
    image = Image.open(path).convert("RGB")
    image = crop_to_background(image, background)
    rgb = np.asarray(image)
    alpha, colour = colour_to_alpha(rgb, background)
    if max(background) < 16:
        outside = connected_to_edges(alpha < PASSABLE_ON_BLACK)
        # Depth into the item from the outside, in pixels, up to the glow band's width.
        band = max(1, int(GLOW_BAND * rgb.shape[1]))
        depth = np.zeros(alpha.shape)
        inside = ~outside
        for _ in range(band):
            depth += inside
            shrunk = inside.copy()
            shrunk[1:, :] &= inside[:-1, :]
            shrunk[:-1, :] &= inside[1:, :]
            shrunk[:, 1:] &= inside[:, :-1]
            shrunk[:, :-1] &= inside[:, 1:]
            inside = shrunk
        out_alpha = np.where(outside, alpha, np.maximum(alpha, depth / band))
        out_rgb = np.clip(rgb / np.maximum(out_alpha, 1e-4)[..., None], 0, 255)   # unmixed from black
    else:
        outside = connected_to_edges(alpha < PASSABLE)
        out_alpha = np.where(outside, alpha, 1.0)
        out_rgb = np.where(outside[..., None], colour, rgb)
    rgba = np.dstack([out_rgb, out_alpha * 255]).astype(np.uint8)
    icon = Image.fromarray(rgba, "RGBA")
    icon = icon.crop(icon.getchannel("A").point(lambda a: 255 if a > 24 else 0).getbbox())
    side = int(max(icon.size) * 1.04)
    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    square.paste(icon, ((side - icon.width) // 2, (side - icon.height) // 2), icon)
    return square.resize((SIZE, SIZE), Image.LANCZOS)


def write_raw(image, path):
    """Raw RGBA32 pixels for the mod: 8-byte header (width, height as little-endian int32), then rows bottom to top."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)[::-1]
    h, w = rgba.shape[:2]
    with open(path, "wb") as f:
        f.write(np.array([w, h], dtype="<i4").tobytes())
        f.write(rgba.tobytes())


def main():
    icons = []
    for item, background in RENDERS.items():
        icon = cut(os.path.join(SOURCES, item + ".jpg"), background)
        icon.save(os.path.join(ASSETS, f"offering_icon_{item}.png"))
        write_raw(icon, os.path.join(ASSETS, f"offering_icon_{item}.rgba"))
        icons.append(icon)

    slot = (40, 38, 34, 255)
    preview = Image.new("RGBA", (len(icons) * (SIZE + 8) + 8, SIZE + 8 + 64 + 16), (24, 22, 20, 255))
    for i, icon in enumerate(icons):
        x = 8 + i * (SIZE + 8)
        tile = Image.new("RGBA", (SIZE, SIZE), slot)
        tile.alpha_composite(icon)
        preview.paste(tile, (x, 8))
        small = Image.new("RGBA", (64, 64), slot)
        small.alpha_composite(icon.resize((64, 64), Image.LANCZOS))
        preview.paste(small, (x + (SIZE - 64) // 2, SIZE + 16))
    preview.convert("RGB").save(PREVIEW)


if __name__ == "__main__":
    main()
