"""
DRAFT designs for the Affix Splinters (issue #29), for Krogon to pick from. Nothing here ships:
it only writes docs/images/splinters_draft2.png.

    python tools/draft_splinters.py            (needs numpy and Pillow)

Idea, after Path of Exile's catalysts: one shape per kind of affix, colours inside a kind per stat.
Fixed seeds: rerunning gives the same picture.
"""
import csv
import math
import os
import random
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from make_shard import ROOT, render_parts, write_raw  # noqa: E402

OUT = os.path.join(ROOT, "Corruption", "Assets", "Splinters")
LOOKS = os.path.join(ROOT, "Corruption", "Data", "splinter_looks.csv")

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


# ---------------------------------------------------------------- the nine shapes (draft 2: rough, organic lumps)
# Krogon, 2026-11-10: "shape by kind, colour by stat" approved; rough organic lumps, not clean cut crystals.
# Every shape is a ball of clay pushed around: a coarse sphere whose points are moved by a few rules and some noise.

def icosphere(level=2):
    t = (1 + 5 ** 0.5) / 2
    v = [(-1, t, 0), (1, t, 0), (-1, -t, 0), (1, -t, 0), (0, -1, t), (0, 1, t), (0, -1, -t), (0, 1, -t),
         (t, 0, -1), (t, 0, 1), (-t, 0, -1), (-t, 0, 1)]
    v = [np.array(p) / np.linalg.norm(p) for p in v]
    f = [(0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11), (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6), (7, 1, 8),
         (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9), (4, 9, 5), (2, 4, 11), (6, 2, 10), (8, 6, 7), (9, 8, 1)]
    tris = [(v[a], v[b], v[c]) for a, b, c in f]
    for _ in range(level):
        out = []
        for a, b, c in tris:
            ab, bc, ca = [(x + y) / np.linalg.norm(x + y) for x, y in ((a, b), (b, c), (c, a))]
            out += [(a, ab, ca), (b, bc, ab), (c, ca, bc), (ab, bc, ca)]
        tris = out
    return tris


def lumps(seed, count, sharp):
    """Noise on a sphere: `count` random bumps and dents; higher `sharp` gives smaller, harder knobs."""
    rng = random.Random(seed)
    dirs = []
    for _ in range(count):
        d = np.array([rng.gauss(0, 1) for _ in range(3)])
        dirs.append((d / np.linalg.norm(d), rng.uniform(-1.0, 1.0)))
    def at(p):
        return sum(amp * max(0.0, float(np.dot(p, d))) ** sharp for d, amp in dirs)
    return at


def clay(push, level=2):
    """Moves every point of the sphere with `push` (unit point -> position), sets it on the ground, largest side H."""
    cache = {}
    def moved(p):
        key = tuple(np.round(p, 6))
        if key not in cache:
            cache[key] = np.array(push(np.array(p)), dtype=float)
        return cache[key]
    tris = [tuple(moved(p) for p in t) for t in icosphere(level)]
    pts = np.array([p for t in tris for p in t])
    low, high = pts[:, 1].min(), pts[:, 1].max()
    k = H / max(high - low, pts[:, 0].max() - pts[:, 0].min(), pts[:, 2].max() - pts[:, 2].min())
    mid = (pts[:, 0].mean(), pts[:, 2].mean())
    tris = [tuple(((p[0] - mid[0]) * k, (p[1] - low) * k, (p[2] - mid[1]) * k) for p in t) for t in tris]
    return orient(tris, (0, H / 2, 0))


def blade():
    """Attack: a jagged fang of metal slag, tall and leaning, with a broken point."""
    n = lumps(11, 26, 5)
    def push(p):
        r = 1 + 0.7 * n(p)
        taper = 1.0 - 0.6 * (p[1] + 1) / 2
        return (p[0] * r * 0.75 * taper + 0.3 * p[1], p[1] * 1.5 * (1 + 0.25 * n(p)), p[2] * r * 0.5 * taper)
    return clay(push)


def cluster():
    """Elemental: a seething lump with horns bursting out of it."""
    n = lumps(12, 18, 3)
    horns = [np.array(d) / np.linalg.norm(d) for d in ((0.1, 1, 0.1), (0.8, 0.6, -0.2), (-0.7, 0.7, 0.3), (0.2, 0.5, -0.9), (-0.3, 0.3, 0.9))]
    def push(p):
        r = 1 + 0.4 * n(p)
        for i, h in enumerate(horns):
            r += (1.3 if i == 0 else 0.9) * max(0.0, float(np.dot(p, h))) ** 9
        return p * r
    return clay(push)


def plate():
    """Defence: a thick, chipped slab, like a lump broken off a shield boss."""
    n = lumps(13, 22, 4)
    def push(p):
        flat = np.sign(p[2]) * min(abs(p[2]), 0.45) * 0.55      # squashed front to back, with flat faces
        r = 1 + 0.22 * n(p)
        return (p[0] * r, p[1] * r * 0.92, flat * (1 + 0.5 * n(p)))
    return clay(push)


def drop():
    """Vitality: a soft, swollen, two-lobed clot."""
    n = lumps(14, 14, 3)
    def push(p):
        lobe = 0.35 * math.exp(-((p[0] - 0.5) ** 2 + (p[1] - 0.4) ** 2) * 3) + 0.3 * math.exp(-((p[0] + 0.5) ** 2 + (p[1] + 0.2) ** 2) * 3)
        r = 1 + lobe + 0.3 * n(p)
        return (p[0] * r, p[1] * r * 0.85, p[2] * r * 0.8)
    return clay(push)


def diamond():
    """Resistance: a geode: a rough round stone, split to show a flat face."""
    n = lumps(15, 30, 6)
    cut = np.array([0.45, 0.35, -0.82])
    cut /= np.linalg.norm(cut)
    def push(p):
        q = p * (1 + 0.5 * n(p))
        over = float(np.dot(q, cut)) - 0.3
        return q - cut * over if over > 0 else q
    return clay(push)


def twist():
    """Utility: a knot: a rope of clay wrung and folded on itself."""
    n = lumps(16, 10, 3)
    def push(p):
        a = 2.4 * p[1]
        x, z = p[0] * math.cos(a) - p[2] * math.sin(a), p[0] * math.sin(a) + p[2] * math.cos(a)
        waist = 0.75 + 0.25 * math.cos(p[1] * 5.2)
        r = waist * (1 + 0.35 * n(p))
        return (x * r * 0.9 + 0.3 * math.sin(p[1] * 3), p[1] * 1.3, z * r * 0.6)
    return clay(push)


def nugget():
    """Mining: a squat lump of raw ore, blocky and pitted."""
    n = lumps(17, 34, 7)
    def push(p):
        boxy = p / (np.abs(p).max() ** 0.55)                   # pulled towards a cube
        r = 1 + 0.45 * n(p)
        return (boxy[0] * r * 1.15, boxy[1] * r * 0.72, boxy[2] * r * 0.95)
    return clay(push)


def sliver():
    """Woodcutting: a gnarled knot of wood with a broken branch stub."""
    n = lumps(18, 20, 4)
    stub = np.array([0.75, 0.6, 0.2])
    stub /= np.linalg.norm(stub)
    def push(p):
        grain = 0.07 * math.sin(math.atan2(p[2], p[0]) * 9)      # ridges running up it, like bark
        r = 1 + grain + 0.3 * n(p) + 1.0 * max(0.0, float(np.dot(p, stub))) ** 8
        return (p[0] * r * 0.62, p[1] * r * 1.2, p[2] * r * 0.55)
    return clay(push)


def arc():
    """Gathering (any tool): a bent, kidney-shaped lump, like a river-worn stone."""
    n = lumps(19, 12, 3)
    def push(p):
        r = 1 + 0.4 * n(p)
        x, y, z = p[0] * r * 1.3, p[1] * r * 0.7, p[2] * r * 0.65
        return (x, y + 0.3 * x * x, z)
    return clay(push)


# ---------------------------------------------------------------- files

SHAPES = {
    "attack": blade, "elemental": cluster, "defence": plate, "vitality": drop, "resistance": diamond,
    "utility": twist, "mining": nugget, "woodcutting": sliver, "gathering": arc,
}
TEXTURE = 32
BG = (34, 36, 40)


def box_uv(tri):
    """Projects each facet on the side it faces most; the tiled noise texture hides the seams."""
    a, b, c = (np.array(p) for p in tri)
    n = np.abs(np.cross(b - a, c - a))
    axes = [(1, 2), (0, 2), (0, 1)][int(np.argmax(n))]
    return [(p[axes[0]] * 12.0, p[axes[1]] * 12.0) for p in tri]


def write_obj(tris, path, name):
    lines = [f"# Affix Splinter ({name}), generated by tools/make_splinters.py", "o " + name]
    faces = []
    vi = 1
    for t, tri in enumerate(tris, start=1):
        a, b, c = (np.array(p) for p in tri)
        n = np.cross(b - a, c - a)
        n = n / (np.linalg.norm(n) or 1.0)
        for p in tri:
            lines.append("v %.5f %.5f %.5f" % tuple(p))
        for u, v in box_uv(tri):
            lines.append("vt %.5f %.5f" % (u, v))
        lines.append("vn %.5f %.5f %.5f" % tuple(n))
        faces.append("f %d/%d/%d %d/%d/%d %d/%d/%d" % (vi, vi, t, vi + 1, vi + 1, t, vi + 2, vi + 2, t))
        vi += 3
    with open(path, "w", newline="\n") as f:
        f.write("\n".join(lines + faces) + "\n")


def tiling_noise(rng, size, cells):
    """Smooth noise that tiles (wraps at the edges), 0..1."""
    grid = rng.uniform(0, 1, (cells, cells))
    y, x = np.mgrid[0:size, 0:size] / size * cells
    x0, y0 = x.astype(int), y.astype(int)
    fx, fy = x - x0, y - y0
    fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    g = lambda i, j: grid[j % cells, i % cells]
    top = g(x0, y0) * (1 - fx) + g(x0 + 1, y0) * fx
    bottom = g(x0, y0 + 1) * (1 - fx) + g(x0 + 1, y0 + 1) * fx
    return top * (1 - fy) + bottom * fy


def albedo(colour, seed):
    rng = np.random.default_rng(seed)
    n = 0.6 * tiling_noise(rng, TEXTURE, 4) + 0.4 * tiling_noise(rng, TEXTURE, 8)
    shade = 0.62 + 0.38 * n                       # darker pits, lighter knobs
    rgb = np.clip(np.array(colour)[None, None, :] * shade[..., None], 0, 255).astype(np.uint8)
    return Image.fromarray(rgb, "RGB")


def emission(seed):
    """A faint glow, brighter in a few soft veins, so splinters read as magical next to plain stones."""
    rng = np.random.default_rng(seed)
    n = tiling_noise(rng, TEXTURE, 4)
    veins = np.clip(1 - np.abs(n - 0.5) * 9, 0, 1)
    glow = 0.22 + 0.5 * veins
    return Image.fromarray((np.clip(glow, 0, 1) * 255).astype(np.uint8), "L")


def read_looks():
    with open(LOOKS, encoding="utf-8-sig", newline="") as f:
        rows = list(csv.DictReader(f))
    looks = []
    for r in rows:
        pool, family = r["pool"].strip(), r["family"].strip()
        prefab = "CorruptionAffixSplinter_" + ("tool_" if pool == "gathering" else "") + family
        hexcolour = r["colour"].strip().lstrip("#")
        looks.append((prefab, r["kind"].strip(), tuple(int(hexcolour[i:i + 2], 16) for i in (0, 2, 4)), family))
    return looks


def font(size):
    try:
        return ImageFont.truetype("DejaVuSans.ttf", size)
    except OSError:
        return ImageFont.load_default()


def render(tris, albedo_img, glow_img, colour, size):
    light = tuple(min(1.0, c / 255 * 1.2) for c in colour)
    return render_parts([(tris, albedo_img, glow_img, light, box_uv)], -0.5, size=size, background=BG, height=H, zoom=3.4, shine=0.5)


def main():
    os.makedirs(OUT, exist_ok=True)
    meshes, glows = {}, {}
    for i, (kind, shape) in enumerate(SHAPES.items()):
        meshes[kind] = shape()
        write_obj(meshes[kind], os.path.join(OUT, f"splinter_{kind}.obj"), "splinter_" + kind)
        glows[kind] = emission(300 + i)
        glows[kind].save(os.path.join(OUT, f"splinter_{kind}_emission.png"))
        write_raw(glows[kind], os.path.join(OUT, f"splinter_{kind}_emission.rgba"))

    looks = read_looks()
    tiles = []
    for i, (prefab, kind, colour, family) in enumerate(looks):
        img = albedo(colour, 500 + i)
        img.save(os.path.join(OUT, prefab + "_albedo.png"))
        write_raw(img, os.path.join(OUT, prefab + "_albedo.rgba"))
        tiles.append((render(meshes[kind], img, glows[kind], colour, 150), family, kind))

    columns = 8
    rows = (len(tiles) + columns - 1) // columns
    cell = 150
    sheet = Image.new("RGB", (cell * columns, 50 + rows * (cell + 34)), BG)
    d = ImageDraw.Draw(sheet)
    d.text((14, 12), f"Affix Splinters: {len(tiles)} looks, {len(SHAPES)} shapes (tools/make_splinters.py)", font=font(22), fill=(235, 225, 200))
    for i, (tile, family, kind) in enumerate(tiles):
        x, y = (i % columns) * cell, 50 + (i // columns) * (cell + 34)
        sheet.paste(tile, (x, y))
        d.text((x + cell / 2, y + cell + 2), family.replace("_", " "), font=font(12), fill=(225, 225, 225), anchor="ma")
        d.text((x + cell / 2, y + cell + 17), kind, font=font(11), fill=(150, 150, 150), anchor="ma")
    sheet.save(os.path.join(ROOT, "docs", "images", "splinters_preview.png"))
    print(f"{len(SHAPES)} shapes, {len(looks)} looks; triangles:", {k: len(v) for k, v in meshes.items()})


if __name__ == "__main__":
    main()
