"""
Cuts the corrupted maps' inventory icons out of the user's concept render (docs/images/maps_reference.jpg, 2026-10-29,
made with the prompt in docs/art-prompts.md -> Corrupted maps): a 4 by 2 grid of maps on a flat slate-grey background.

    python tools/make_map_icons.py            (needs numpy and Pillow)

Writes, into Corruption/Assets/:
    map_icon_<Biome>.png     for viewing
    map_icon_<Biome>.rgba    the same pixels raw, which the mod loads (8-byte header, rows bottom to top; see write_raw)
and docs/images/map_icons_preview.png: the icons at full size and at 64 px (inventory size) on the game's slot colour.

The background is removed by growing the background-coloured area in from each cell's edges, so dark parts inside
a map (the Black Forest hide) stay. Biome order matches the render and Corruption/Data/maps.csv.
"""
import os

import numpy as np
from PIL import Image, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SOURCE = os.path.join(ROOT, "docs", "images", "maps_reference.jpg")
ASSETS = os.path.join(ROOT, "Corruption", "Assets")
PREVIEW = os.path.join(ROOT, "docs", "images", "map_icons_preview.png")

BIOMES = ["Meadows", "BlackForest", "Swamp", "Mountain", "Plains", "Mistlands", "AshLands", "DeepNorth"]
COLUMNS, ROWS = 4, 2
SIZE = 128            # icon texture size; the game shows it at about 64 px
BACKGROUND_TOLERANCE = 18   # colour distance still counted as background (JPEG noise on a flat colour)


def cut(cell):
    rgb = np.asarray(cell.convert("RGB")).astype(np.int32)
    h, w = rgb.shape[:2]
    border = np.concatenate([rgb[0], rgb[-1], rgb[:, 0], rgb[:, -1]])
    background = np.median(border, axis=0)
    distance = np.sqrt(((rgb - background) ** 2).sum(axis=2))

    # Background is the background-coloured area connected to the cell's edges (grown step by step from them).
    background_like = distance < BACKGROUND_TOLERANCE
    reached = np.zeros_like(background_like)
    reached[0, :] = reached[-1, :] = reached[:, 0] = reached[:, -1] = True
    reached &= background_like
    while True:
        grown = reached.copy()
        grown[1:, :] |= reached[:-1, :]
        grown[:-1, :] |= reached[1:, :]
        grown[:, 1:] |= reached[:, :-1]
        grown[:, :-1] |= reached[:, 1:]
        grown &= background_like
        if (grown == reached).all():
            break
        reached = grown
    alpha = Image.fromarray(np.where(reached, 0, 255).astype(np.uint8), "L")
    # Soften the cut edge by a pixel, so it doesn't look jagged.
    alpha = alpha.filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(0.8))

    image = cell.convert("RGBA")
    image.putalpha(alpha)
    box = alpha.point(lambda a: 255 if a > 8 else 0).getbbox()
    image = image.crop(box)

    # Centre on a square canvas with a small margin, then scale to the icon size.
    side = int(max(image.size) * 1.04)
    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    square.paste(image, ((side - image.width) // 2, (side - image.height) // 2), image)
    return square.resize((SIZE, SIZE), Image.LANCZOS)


def write_raw(image, path):
    """Raw RGBA32 pixels for the mod: 8-byte header (width, height as little-endian int32), then rows bottom to top,
    as Unity stores textures (same format as the other Corruption textures)."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)[::-1]
    h, w = rgba.shape[:2]
    with open(path, "wb") as f:
        f.write(np.array([w, h], dtype="<i4").tobytes())
        f.write(rgba.tobytes())


def main():
    source = Image.open(SOURCE)
    cw, ch = source.width / COLUMNS, source.height / ROWS
    icons = []
    for i, biome in enumerate(BIOMES):
        c, r = i % COLUMNS, i // COLUMNS
        cell = source.crop((round(c * cw), round(r * ch), round((c + 1) * cw), round((r + 1) * ch)))
        icon = cut(cell)
        icon.save(os.path.join(ASSETS, f"map_icon_{biome}.png"))
        write_raw(icon, os.path.join(ASSETS, f"map_icon_{biome}.rgba"))
        icons.append(icon)

    # Preview: full size on top, inventory size (64 px) below, on a dark slot colour.
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
