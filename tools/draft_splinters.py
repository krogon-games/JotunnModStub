"""
DRAFT designs for the Affix Splinters (issue #29), for Krogon to pick from. Nothing here ships:
it only writes docs/images/splinters_draft.png.

    python tools/draft_splinters.py            (needs numpy and Pillow)

Idea, after Path of Exile's catalysts: one shape per kind of affix, colours inside a kind per stat.
Fixed seeds: rerunning gives the same picture.
"""
import math
import os
import random
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from make_shard import ROOT, render_parts  # noqa: E402

H = 0.2  # metres, about the shard's size


def orient(tris, centre=None):
    """Triangles facing away from the shape's middle."""
    pts = np.array([p for t in tris for p in t])
    c = pts.mean(axis=0) if centre is None else np.array(centre)
    out = []
    for t in tris:
        a, b, d = (np.array(p) for p in t)
        n = np.cross(b - a, d - a)
        out.append(t if np.dot(n, (a + b + d) / 3 - c) > 0 else (t[0], t[2], t[1]))
    return out


def loft(rings, bottom=None, top=None):
    """Joins rings of points (each the same length) into a skin, closed with a point or a flat cap at each end."""
    tris = []
    n = len(rings[0])
    for r in range(len(rings) - 1):
        for i in range(n):
            j = (i + 1) % n
            tris.append((rings[r][i], rings[r][j], rings[r + 1][j]))
            tris.append((rings[r][i], rings[r + 1][j], rings[r + 1][i]))
    for ring, tip in ((rings[0], bottom), (rings[-1], top)):
        tip = tuple(np.mean(ring, axis=0)) if tip is None else tip
        for i in range(n):
            tris.append((ring[i], ring[(i + 1) % n], tip))
    return orient(tris)


def ring(y, rx, rz, n, turn=0.0, rng=None, wobble=0.0, cx=0.0, cz=0.0):
    pts = []
    for i in range(n):
        a = turn + 2 * math.pi * i / n
        k = 1.0 + (rng.uniform(-wobble, wobble) if rng else 0.0)
        pts.append((cx + rx * k * math.cos(a), y, cz + rz * k * math.sin(a)))
    return pts


def move(tris, dx=0.0, dy=0.0, dz=0.0, lean=0.0, turn=0.0):
    """Leans a part sideways (around z) and turns it (around y), then moves it."""
    cl, sl, ct, st = math.cos(lean), math.sin(lean), math.cos(turn), math.sin(turn)
    out = []
    for t in tris:
        pts = []
        for x, y, z in t:
            x, y = x * cl - y * sl, x * sl + y * cl
            x, z = x * ct - z * st, x * st + z * ct
            pts.append((x + dx, y + dy, z + dz))
        out.append(tuple(pts))
    return out


# ---------------------------------------------------------------- the nine shapes

def blade():
    """Attack: a long, thin, flat sliver with a sharp point, like a broken sword tip."""
    rng = random.Random(1)
    rings = [ring(H * f, 0.05 * w, 0.012 * w, 6, rng=rng, wobble=0.08)
             for f, w in ((0.06, 0.55), (0.3, 1.0), (0.62, 0.8), (0.86, 0.42))]
    return move(loft(rings, bottom=(0.005, 0, 0), top=(0.012, H * 1.08, 0)), lean=-0.22, dx=0.02, dy=0.0)


def cluster():
    """Elemental: three jagged crystals grown together."""
    rng = random.Random(2)
    tris = []
    for height, lean, turn, dx in ((1.0, 0.0, 0.3, 0.0), (0.7, 0.5, 1.2, 0.015), (0.55, -0.55, 2.4, -0.015)):
        h = H * height
        rings = [ring(h * f, 0.032 * w, 0.03 * w, 5, turn=turn, rng=rng, wobble=0.1) for f, w in ((0.0, 0.8), (0.55, 1.0), (0.8, 0.7))]
        tris += move(loft(rings, top=(0.004, h, 0.0)), lean=lean, dx=dx)
    return tris


def plate():
    """Defence: a thick six-sided plate, a piece of shield or scale."""
    rng = random.Random(3)
    def hexagon(z, k):
        return [(0.075 * k * math.cos(a) * rng.uniform(0.96, 1.04), H * 0.5 + 0.085 * k * math.sin(a), z)
                for a in (math.pi / 6 + math.pi / 3 * i for i in range(6))]
    rings = [hexagon(-0.022, 0.78), hexagon(-0.012, 1.0), hexagon(0.012, 1.0), hexagon(0.022, 0.78)]
    return move(loft(rings), turn=0.5)


def drop():
    """Vitality: a smooth heavy drop."""
    rings = [ring(H * f, 0.06 * w, 0.06 * w, 10, turn=0.3 * i)
             for i, (f, w) in enumerate(((0.05, 0.55), (0.2, 0.95), (0.38, 1.0), (0.58, 0.72), (0.78, 0.36)))]
    return loft(rings, bottom=(0, 0, 0), top=(0.004, H, 0))


def diamond():
    """Resistance: a clean eight-sided ward stone, two pyramids base to base."""
    rings = [ring(H * 0.5, 0.062, 0.062, 4, turn=math.pi / 4)]
    return loft(rings, bottom=(0, 0, 0), top=(0, H, 0))


def twist():
    """Utility: a square bar wrung into a spiral."""
    rings = [ring(H * f, 0.036 * w, 0.036 * w, 4, turn=1.5 * f) for f, w in
             ((0.0, 0.8), (0.2, 1.0), (0.4, 1.0), (0.6, 1.0), (0.8, 1.0), (1.0, 0.8))]
    return loft(rings)


def nugget():
    """Mining: a rough lump of ore."""
    rng = random.Random(7)
    rings = [ring(0.15 * f, 0.075 * w, 0.06 * w, 7, turn=0.4 * i, rng=rng, wobble=0.22)
             for i, (f, w) in enumerate(((0.08, 0.6), (0.35, 1.0), (0.65, 0.95), (0.9, 0.55)))]
    return move(loft(rings), dy=0.025)


def sliver():
    """Woodcutting: a split of wood, three long splinters."""
    rng = random.Random(8)
    tris = []
    for height, lean, dx, dz in ((1.0, 0.06, 0.0, 0.0), (0.72, -0.2, -0.022, 0.012), (0.6, 0.3, 0.022, -0.01)):
        h = H * height
        rings = [ring(h * f, 0.016 * w, 0.011 * w, 4, turn=0.4, rng=rng, wobble=0.2) for f, w in ((0.0, 1.0), (0.5, 1.15), (0.85, 0.6))]
        tris += move(loft(rings, top=(rng.uniform(-0.008, 0.008), h, 0.0)), lean=lean, dx=dx, dz=dz)
    return tris


def arc():
    """Gathering (any tool): a broken arm ring."""
    tris = []
    steps, sides = 9, 5
    path = []
    for s in range(steps + 1):
        a = math.radians(-35 + 250 * s / steps)
        path.append((0.065 * math.cos(a), H * 0.5 + 0.075 * math.sin(a), a))
    rings = []
    for x, y, a in path:
        r = []
        for k in range(sides):
            b = 2 * math.pi * k / sides
            rad = 0.017 * math.cos(b)
            r.append((x + rad * math.cos(a), y + rad * math.sin(a), 0.017 * math.sin(b)))
        rings.append(r)
    n = sides
    for r in range(len(rings) - 1):
        mid = tuple(np.mean(rings[r] + rings[r + 1], axis=0))
        part = []
        for i in range(n):
            j = (i + 1) % n
            part.append((rings[r][i], rings[r][j], rings[r + 1][j]))
            part.append((rings[r][i], rings[r + 1][j], rings[r + 1][i]))
        tris += orient(part, mid)
    for end, nxt in ((rings[0], rings[1]), (rings[-1], rings[-2])):
        tip = tuple(np.mean(end, axis=0))
        inner = np.mean(nxt, axis=0)
        tris += orient([(end[i], end[(i + 1) % n], tip) for i in range(n)], inner)
    return move(tris, turn=0.35)


# ---------------------------------------------------------------- the sheet

KINDS = [
    ("Attack", "damage, backstab, knockback,\nweapon skill, attack speed", blade, (196, 62, 48)),
    ("Elemental", "fire, frost, lightning,\npoison, spirit damage", cluster, (236, 120, 40)),
    ("Defence", "armour, block, parry", plate, (150, 160, 176)),
    ("Vitality", "health, stamina, eitr,\nregen, life steal", drop, (190, 40, 60)),
    ("Resistance", "fire, frost, lightning,\npoison, all", diamond, (90, 190, 200)),
    ("Utility", "movement, carry weight,\ndurability", twist, (204, 170, 70)),
    ("Mining", "pickaxe affixes", nugget, (176, 116, 70)),
    ("Woodcutting", "axe affixes", sliver, (150, 104, 56)),
    ("Gathering", "affixes for any tool", arc, (96, 170, 96)),
]

VARIANTS = [
    ("Fire", cluster, (236, 110, 36)), ("Frost", cluster, (120, 200, 240)), ("Lightning", cluster, (240, 226, 110)),
    ("Poison", cluster, (120, 200, 70)), ("Spirit", cluster, (226, 226, 240)),
    ("Health", drop, (200, 44, 56)), ("Stamina", drop, (232, 196, 60)), ("Eitr", drop, (90, 130, 236)),
]

BG = (34, 36, 40)


def tile(shape, colour, size):
    rng = np.random.default_rng(5)
    noise = rng.uniform(0.86, 1.0, (32, 32, 1))
    albedo = Image.fromarray(np.clip(np.array(colour) * 0.8 * noise, 0, 255).astype(np.uint8), "RGB")
    glow = Image.new("L", (32, 32), 90)
    light = tuple(min(1.0, c / 255 * 1.2) for c in colour)
    uv = lambda tri: [((p[0] * 40) % 1.0, (p[1] * 4) % 1.0) for p in tri]
    return render_parts([(shape(), albedo, glow, light, uv)], -0.5, size=size, background=BG, height=H, zoom=3.4, shine=0.5)


def font(size):
    try:
        return ImageFont.truetype("DejaVuSans.ttf", size)
    except OSError:
        return ImageFont.load_default()


def main():
    cell, small, pad = 300, 200, 70
    width = cell * 3
    height = 60 + (cell + pad) * 3 + 70 + small + 50
    sheet = Image.new("RGB", (width, height), BG)
    d = ImageDraw.Draw(sheet)
    d.text((20, 16), "Affix Splinters: DRAFT. One shape per kind of affix", font=font(24), fill=(235, 225, 200))
    for i, (name, covers, shape, colour) in enumerate(KINDS):
        x, y = (i % 3) * cell, 60 + (i // 3) * (cell + pad)
        sheet.paste(tile(shape, colour, cell), (x, y))
        d.text((x + cell / 2, y + cell - 6), name, font=font(20), fill=(240, 232, 210), anchor="ma")
        d.multiline_text((x + cell / 2, y + cell + 20), covers, font=font(13), fill=(165, 165, 165), anchor="ma", align="center")
    y = 60 + (cell + pad) * 3
    d.text((20, y + 14), "Colour per stat inside a kind (examples)", font=font(24), fill=(235, 225, 200))
    w = width // len(VARIANTS)
    for i, (name, shape, colour) in enumerate(VARIANTS):
        t = tile(shape, colour, small).resize((w, w))
        sheet.paste(t, (i * w, y + 60))
        d.text((i * w + w / 2, y + 60 + w + 4), name, font=font(14), fill=(220, 220, 220), anchor="ma")
    out = os.path.join(ROOT, "docs", "images", "splinters_draft.png")
    sheet.save(out)
    print(out)


if __name__ == "__main__":
    main()
