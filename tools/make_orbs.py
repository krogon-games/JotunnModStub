"""
Generates the currency orbs' models and textures, after the user's concept render
(docs/images/orbs_reference.jpg), and a preview sheet.

    python tools/make_orbs.py            (needs numpy and Pillow)

Writes to Corruption/Assets/Orbs/ (not embedded in the DLL until the orbs are built as items, design phase 7):
    orb.obj                                  the shared hand-hewn sphere
    orb_band.obj, orb_rim.obj, orb_sockets.obj   add-ons: Augmentation's band, Regal's toothed rim, Jeweller's sockets
    orb_<id>_albedo.png/.rgba                colour texture per orb (PNG to view, raw for the game)
    orb_<id>_emission.png/.rgba              glow mask per orb
and docs/images/orbs_preview.png.

Patterns are painted in 3D (each texel knows its point on the sphere), so runes, rings and cracks don't stretch with
the spherical UV map. The rune faces the camera (-z) and is repeated on the back (+z).
Fixed seeds: rerunning gives the same files. Each style is one function below; tweak and rerun.
"""
import math
import os
import random
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import make_shard  # noqa: E402
from make_shard import ROOT, render_parts, write_obj, write_raw  # noqa: E402

OUT = os.path.join(ROOT, "Corruption", "Assets", "Orbs")
SIZE = 256
R = 0.09                      # radius in metres: about fist-sized
HEIGHT = 2 * R
PREVIEW_YAW = 0.12


# ---------------------------------------------------------------- shared shapes

def build_orb():
    """A low-poly sphere with slightly uneven facets, like a hand-hewn stone ball."""
    rng = random.Random(5)
    segments, rings = 10, 7
    lat = [math.pi * (r + 1) / (rings + 1) for r in range(rings)]
    grid = []
    for r, phi in enumerate(lat):
        offset = math.pi / segments * (r % 2)
        ring = []
        for i in range(segments):
            a = offset + 2 * math.pi * i / segments + rng.uniform(-0.05, 0.05)
            k = R * rng.uniform(0.96, 1.03)
            ring.append((k * math.sin(phi) * math.cos(a), R + k * math.cos(phi), k * math.sin(phi) * math.sin(a)))
        grid.append(ring)
    top, bottom = (0.0, 2 * R * 0.99, 0.0), (0.0, R * 0.01, 0.0)
    tris = []
    for i in range(segments):
        j = (i + 1) % segments
        tris.append((grid[0][i], grid[0][j], top))
    for r in range(rings - 1):
        a_ring, b_ring = grid[r], grid[r + 1]
        for i in range(segments):
            j = (i + 1) % segments
            tris += [(a_ring[i], b_ring[j], a_ring[j]), (a_ring[i], b_ring[i], b_ring[j])]
    for i in range(segments):
        j = (i + 1) % segments
        tris.append((grid[-1][j], grid[-1][i], bottom))
    return [fix_winding(t) for t in tris]


def fix_winding(tri):
    """Outward-facing triangles (normal away from the orb's centre)."""
    a, b, c = (np.array(p) for p in tri)
    n = np.cross(b - a, c - a)
    centre = (a + b + c) / 3 - np.array([0.0, R, 0.0])
    return tri if np.dot(n, centre) > 0 else (tri[0], tri[2], tri[1])


def orb_uv(tri):
    pts = np.array(tri)
    centre = pts.mean(axis=0)
    angle = (math.atan2(centre[2], centre[0]) / (2 * math.pi)) % 1.0
    uvs = []
    for p in pts:
        a = angle if abs(p[0]) < 1e-9 and abs(p[2]) < 1e-9 else (math.atan2(p[2], p[0]) / (2 * math.pi)) % 1.0
        if abs(a - angle) > 0.5:
            a += 1.0 if a < angle else -1.0
        uvs.append((a, p[1] / HEIGHT))
    return uvs


def flat_uv(tri):
    return [(0.5, 0.5)] * 3


def tube_loop(points, radius, sides=4):
    """A closed faceted tube through points (rings and bands)."""
    tris, frames = [], []
    n = len(points)
    for k in range(n):
        p = np.array(points[k])
        d = np.array(points[(k + 1) % n]) - np.array(points[k - 1])
        d /= np.linalg.norm(d)
        up = np.array([0.0, 1.0, 0.0]) if abs(d[1]) < 0.9 else np.array([1.0, 0.0, 0.0])
        u = np.cross(d, up); u /= np.linalg.norm(u)
        v = np.cross(d, u)
        frames.append([tuple(p + radius * (math.cos(2 * math.pi * s / sides) * u + math.sin(2 * math.pi * s / sides) * v))
                       for s in range(sides)])
    for k in range(n):
        f0, f1 = frames[k], frames[(k + 1) % n]
        for s in range(sides):
            t = (s + 1) % sides
            tris += [(f0[s], f0[t], f1[t]), (f0[s], f1[t], f1[s])]
    return tris


def build_band():
    """Augmentation: a thin copper band, tilted, standing just off the stone."""
    tilt = math.radians(-22)
    pts = []
    for k in range(32):
        a = 2 * math.pi * k / 32
        x, z = R * 1.05 * math.cos(a), R * 1.05 * math.sin(a)
        pts.append((x, R + x * math.tan(tilt), z))
    return tube_loop(pts, 0.0035)


def build_rim():
    """Regal: a flat toothed copper rim round the orb, facing the camera, like a crown seen from above."""
    teeth = 24
    inner, mid, outer = R * 0.97, R * 1.13, R * 1.25
    tris = []
    for k in range(teeth):
        a0 = 2 * math.pi * k / teeth
        a1 = 2 * math.pi * (k + 1) / teeth
        am = (a0 + a1) / 2
        p = lambda r, a: (r * math.cos(a), R + r * math.sin(a), 0.0)   # noqa: E731
        tris += [(p(inner, a0), p(mid, a0), p(mid, a1)), (p(inner, a0), p(mid, a1), p(inner, a1)),
                 (p(mid, a0), p(outer, am), p(mid, a1))]
    # a second copy a little behind, so the rim has some thickness from the side
    back = [tuple((x, y, z + 0.006) for (x, y, z) in t) for t in tris]
    both = tris + back
    return both + [(t[0], t[2], t[1]) for t in both]   # visible from both sides


SOCKET_DIRS = [np.array(d) / np.linalg.norm(d) for d in
               [(sx, sy, sz) for sx in (-1, 1) for sy in (-1, 1) for sz in (-1, 1)]]


def build_sockets():
    """Jeweller's: copper rings at the eight cube-corner directions, four round the rune on each side."""
    tris = []
    for d in SOCKET_DIRS:
        u = np.cross(d, [0.0, 1.0, 0.0]); u /= np.linalg.norm(u)
        v = np.cross(d, u)
        centre = np.array([0.0, R, 0.0]) + d * R * 1.01
        pts = [tuple(centre + R * 0.17 * (math.cos(2 * math.pi * k / 12) * u + math.sin(2 * math.pi * k / 12) * v))
               for k in range(12)]
        tris += tube_loop(pts, 0.005)
    return tris


# ---------------------------------------------------------------- 3D texture space

v_idx, u_idx = np.mgrid[0:SIZE, 0:SIZE]
V = 1 - (v_idx + 0.5) / SIZE                   # image row 0 = top of the orb
U = (u_idx + 0.5) / SIZE
Y = V * 2 - 1                                  # -1 bottom .. 1 top
RING = np.sqrt(np.clip(1 - Y ** 2, 0, 1))
X = RING * np.cos(2 * math.pi * U)
Z = RING * np.sin(2 * math.pi * U)
P = np.stack([X, Y, Z], axis=-1)               # unit direction of each texel


def mix(a, b, t):
    t = np.clip(t, 0, 1)[..., None]
    return np.array(a, dtype=float) * (1 - t) + np.array(b, dtype=float) * t


def noise(rng, cells=8, octaves=3):
    """Smooth 3D value noise sampled on the sphere (no seam)."""
    total = np.zeros((SIZE, SIZE))
    for o in range(octaves):
        n = cells * 2 ** o
        grid = rng.uniform(0, 1, (n + 1, n + 1, n + 1))
        c = (P + 1) / 2 * n
        i = np.clip(np.floor(c).astype(int), 0, n - 1)
        f = c - i
        f = f * f * (3 - 2 * f)
        val = 0
        for dx in (0, 1):
            for dy in (0, 1):
                for dz in (0, 1):
                    w = (f[..., 0] if dx else 1 - f[..., 0]) * (f[..., 1] if dy else 1 - f[..., 1]) * \
                        (f[..., 2] if dz else 1 - f[..., 2])
                    val = val + w * grid[i[..., 0] + dx, i[..., 1] + dy, i[..., 2] + dz]
        total += val / 2 ** o
    return (total - total.min()) / (total.max() - total.min())


def cells(rng, count):
    """3D Voronoi on the sphere: (nearest cell index, border strength 0..1)."""
    pts = rng.normal(size=(count, 3))
    pts /= np.linalg.norm(pts, axis=1, keepdims=True)
    d = np.stack([np.linalg.norm(P - p, axis=-1) for p in pts])
    order = np.argsort(d, axis=0)
    ds = np.take_along_axis(d, order[:2], axis=0)
    return order[0], ds[1] - ds[0]


def facing(direction):
    """How directly each texel faces a direction (cosine), and its 2D position on the plane facing it."""
    d = np.array(direction, dtype=float); d /= np.linalg.norm(d)
    up = np.array([0.0, 1.0, 0.0]) if abs(d[1]) < 0.9 else np.array([0.0, 0.0, 1.0])
    right = np.cross(d, up); right /= np.linalg.norm(right)
    up = np.cross(right, d)
    return P @ d, P @ right, P @ up


def plane_mask(draw_fn, direction, reach=0.25, canvas=512):
    """Paints a 2D drawing (coordinates -1..1, y up) onto the side of the orb facing a direction."""
    img = Image.new("L", (canvas, canvas), 0)
    draw_fn(ImageDraw.Draw(img), lambda x, y: ((x + 1) / 2 * canvas, (1 - (y + 1) / 2) * canvas), canvas)
    arr = np.asarray(img, dtype=float) / 255.0
    cos, px, py = facing(direction)
    col = np.clip(((px + 1) / 2 * canvas).astype(int), 0, canvas - 1)
    row = np.clip(((1 - (py + 1) / 2) * canvas).astype(int), 0, canvas - 1)
    return arr[row, col] * (cos > reach)


def blur(mask, radius):
    img = Image.fromarray((np.clip(mask, 0, 1) * 255).astype(np.uint8))
    return np.asarray(img.filter(ImageFilter.GaussianBlur(radius)), dtype=float) / 255.0


# Runes in a unit square (x right, y down, 0..1), as stroke lists.
RUNES = {
    "eihwaz": [[(0.5, 0.14), (0.5, 0.86)], [(0.5, 0.14), (0.68, 0.32)], [(0.5, 0.86), (0.32, 0.68)]],
    "uruz": [[(0.36, 0.86), (0.36, 0.14), (0.66, 0.36), (0.66, 0.86)]],
    "gebo": [[(0.3, 0.18), (0.7, 0.82)], [(0.7, 0.18), (0.3, 0.82)]],
    "othala": [[(0.5, 0.14), (0.7, 0.4), (0.3, 0.86)], [(0.5, 0.14), (0.3, 0.4), (0.7, 0.86)]],
    "tiwaz": [[(0.5, 0.14), (0.5, 0.86)], [(0.3, 0.36), (0.5, 0.14), (0.7, 0.36)]],
    "dagaz": [[(0.3, 0.2), (0.3, 0.8), (0.7, 0.2), (0.7, 0.8), (0.3, 0.2)]],
    "naudiz": [[(0.38, 0.14), (0.38, 0.86)], [(0.62, 0.14), (0.62, 0.86)], [(0.38, 0.36), (0.62, 0.62)]],
    "perthro": [[(0.36, 0.14), (0.36, 0.86)], [(0.36, 0.14), (0.62, 0.32), (0.62, 0.18)],
                [(0.36, 0.86), (0.62, 0.68), (0.62, 0.82)]],
    "jera": [[(0.46, 0.14), (0.28, 0.36), (0.46, 0.56)], [(0.54, 0.44), (0.72, 0.64), (0.54, 0.86)]],
}


def rune_masks(name, size=0.62, width=0.075):
    """(stroke, halo) masks of a rune on the front and back."""
    def draw(width_scale):
        def fn(d, to_px, canvas):
            for stroke in RUNES[name]:
                pts = [to_px((x - 0.5) * 2 * size, (0.5 - y) * 2 * size) for x, y in stroke]
                d.line(pts, fill=255, width=int(width * width_scale * canvas / 2), joint="curve")
                for p in pts:
                    r = width * width_scale * canvas / 4
                    d.ellipse((p[0] - r, p[1] - r, p[0] + r, p[1] + r), fill=255)
        return fn
    stroke = plane_mask(draw(1.0), (0, 0, -1)) + plane_mask(draw(1.0), (0, 0, 1))
    halo = plane_mask(draw(2.6), (0, 0, -1)) + plane_mask(draw(2.6), (0, 0, 1))
    return blur(stroke, 0.8), blur(halo, 5)


def carve(albedo, stroke, halo, colour):
    """A rune cut into the surface: dark edge, glowing inside."""
    edge = np.clip(blur(stroke, 2.5) - stroke, 0, 1)
    albedo = albedo * (1 - 0.6 * edge[..., None])
    albedo = mix(albedo, colour, np.clip(halo * 0.25, 0, 1))
    return mix(albedo, colour, stroke)


def hewn(rng, base, dark, light, strength=1.0):
    """Stone with chipped facets: base colour, darker chip lines, lighter worn edges."""
    idx, border = cells(rng, 26)
    shade = rng.uniform(-0.12, 0.12, 26)[idx]
    n = noise(rng, 6)
    albedo = mix(base, light, np.clip(n * 0.6 + shade, 0, 1))
    chips = np.clip(1 - border / 0.05, 0, 1) * strength
    return mix(albedo, dark, chips * 0.55)


# ---------------------------------------------------------------- styles (one per orb)

def transmutation(rng):
    albedo = hewn(rng, [140, 138, 130], [70, 68, 64], [190, 188, 180])
    stroke, halo = rune_masks("eihwaz")
    colour = [175, 215, 255]
    return carve(albedo, stroke, halo, colour), np.clip(stroke + halo * 0.35, 0, 1), (0.45, 0.7, 1.0)


def augmentation(rng):
    albedo = hewn(rng, [78, 86, 98], [36, 40, 48], [120, 130, 142])
    stroke, halo = rune_masks("uruz")
    colour = [110, 170, 255]
    return carve(albedo, stroke, halo, colour), np.clip(stroke + halo * 0.35, 0, 1), (0.3, 0.55, 1.0)


def alchemy(rng):
    albedo = hewn(rng, [42, 40, 40], [20, 19, 19], [72, 68, 66], 0.4)
    _, border = cells(np.random.default_rng(31), 16)
    seams = np.clip(1 - border / 0.035, 0, 1)
    stroke, halo = rune_masks("gebo", width=0.1)
    gold = [255, 196, 40]
    albedo = mix(albedo, gold, seams)
    albedo = carve(albedo, stroke, halo, [255, 220, 90])
    return albedo, np.clip(stroke + seams * 0.9 + halo * 0.3, 0, 1), (1.0, 0.72, 0.12)


def regal(rng):
    albedo = hewn(rng, [62, 66, 74], [30, 32, 36], [100, 104, 112])
    def rivets(d, to_px, canvas):
        for k in range(10):
            a = 2 * math.pi * k / 10 + 0.3
            x, y = to_px(0.86 * math.cos(a), 0.86 * math.sin(a))
            r = canvas * 0.018
            d.ellipse((x - r, y - r, x + r, y + r), fill=255)
    dots = plane_mask(rivets, (0, 0, -1), reach=0.0) + plane_mask(rivets, (0, 0, 1), reach=0.0)
    albedo = mix(albedo, [176, 110, 66], blur(dots, 0.6))
    stroke, halo = rune_masks("othala")
    albedo = carve(albedo, stroke, halo, [255, 205, 70])
    return albedo, np.clip(stroke + halo * 0.3, 0, 1), (1.0, 0.78, 0.2)


def knot_ring(d, to_px, canvas, radius=0.72, amp=0.045, waves=9, width=0.016):
    """A braided band: two strands weaving round a circle, between two border lines."""
    w = int(width * canvas)
    for r in (radius - amp * 1.9, radius + amp * 1.9):
        d.ellipse([*to_px(-r, r), *to_px(r, -r)], outline=255, width=w)
    for sign in (1, -1):
        pts = []
        for k in range(361):
            a = 2 * math.pi * k / 360
            rr = radius + sign * amp * math.sin(waves * a)
            pts.append(to_px(rr * math.cos(a), rr * math.sin(a)))
        d.line(pts, fill=255, width=w)


def exalted(rng):
    albedo = hewn(rng, [196, 198, 204], [120, 122, 130], [235, 236, 240], 0.5)
    band = plane_mask(knot_ring, (0, 0, -1), reach=0.0) + plane_mask(knot_ring, (0, 0, 1), reach=0.0)
    albedo = albedo * (1 - 0.45 * blur(band, 0.7)[..., None])
    stroke, halo = rune_masks("tiwaz")
    albedo = carve(albedo, stroke, halo, [255, 214, 60])
    return albedo, np.clip(stroke * 1.1 + halo * 0.45, 0, 1), (1.0, 0.82, 0.25)


def knot_triangle(d, to_px, canvas, width=0.02):
    """A triangular knot: three interlaced loops inside a triangle (Divine Orb's engraving)."""
    w = int(width * canvas)
    tri = [to_px(0.62 * math.cos(a), 0.62 * math.sin(a) - 0.05) for a in (math.pi / 2, math.pi * 7 / 6, math.pi * 11 / 6)]
    d.line(tri + [tri[0]], fill=255, width=w, joint="curve")
    for a in (math.pi / 2, math.pi * 7 / 6, math.pi * 11 / 6):
        cx, cy = 0.2 * math.cos(a), 0.2 * math.sin(a) - 0.05
        d.ellipse([*to_px(cx - 0.24, cy + 0.24), *to_px(cx + 0.24, cy - 0.24)], outline=255, width=w)


def divine(rng):
    albedo = hewn(rng, [212, 186, 128], [150, 124, 72], [240, 222, 175], 0.5)
    knots = sum(plane_mask(knot_triangle, d, reach=0.35) for d in [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0)])
    albedo = albedo * (1 - 0.55 * blur(knots, 0.7)[..., None])
    stroke, halo = rune_masks("dagaz")
    albedo = carve(albedo, stroke, halo, [255, 245, 215])
    return albedo, np.clip(stroke * 1.1 + halo * 0.5, 0, 1), (1.0, 0.9, 0.65)


def annulment(rng):
    albedo = hewn(rng, [34, 34, 37], [14, 14, 16], [70, 70, 76], 0.6)
    _, border = cells(np.random.default_rng(41), 9)
    crack = np.clip(1 - border / 0.02, 0, 1)
    albedo = albedo * (1 - 0.7 * crack[..., None]) + np.array([60, 60, 66]) * blur(crack, 1.5)[..., None] * 0.3
    hole = np.clip((Y - 0.6) / 0.06, 0, 1)                 # the broken-open top
    lip = np.clip(1 - abs(Y - 0.6) / 0.04, 0, 1)
    albedo = mix(albedo, [5, 5, 6], hole)
    albedo = mix(albedo, [95, 95, 102], lip * 0.6)
    stroke, halo = rune_masks("naudiz")
    albedo = carve(albedo, stroke, halo, [150, 140, 175])
    return albedo, np.clip(stroke * 0.55 + halo * 0.15, 0, 1), (0.6, 0.55, 0.78)


def chance(rng):
    idx, border = cells(np.random.default_rng(51), 22)
    bone = mix([205, 192, 160], [235, 225, 200], noise(rng, 6))
    grain = (np.sin((P @ np.array([0.3, 1.0, 0.2])) * 70 + noise(rng, 4) * 12) + 1) / 2
    wood = mix([92, 60, 34], [140, 98, 58], grain)
    albedo = np.where((idx % 2 == 0)[..., None], bone, wood)
    albedo = albedo * (1 - 0.6 * np.clip(1 - border / 0.04, 0, 1)[..., None])
    stroke, halo = rune_masks("perthro")
    hue = (P[..., 1] * 1.5 + P[..., 0] * 1.5) % 1.0
    rainbow = np.stack([np.clip(np.abs(hue * 6 - 3) - 1, 0, 1),
                        np.clip(2 - np.abs(hue * 6 - 2), 0, 1),
                        np.clip(2 - np.abs(hue * 6 - 4), 0, 1)], axis=-1) * 170 + 70
    albedo = carve(albedo, stroke, halo * 0, [0, 0, 0])
    albedo = albedo * (1 - stroke[..., None]) + rainbow * stroke[..., None]
    return albedo, np.clip(stroke * 0.5 + halo * 0.15, 0, 1), (0.85, 0.85, 0.85)


def jeweller(rng):
    albedo = hewn(rng, [46, 72, 78], [20, 34, 38], [80, 112, 118])
    holes = 0
    for d in SOCKET_DIRS:
        holes = holes + np.clip((P @ d - 0.984) / 0.005, 0, 1)
    albedo = mix(albedo, [8, 8, 8], holes)
    stroke, halo = rune_masks("jera")
    albedo = carve(albedo, stroke, halo, [130, 245, 220])
    return albedo, np.clip(stroke + halo * 0.35, 0, 1), (0.3, 0.9, 0.8)


COPPER = (178, 108, 62)
ORBS = [
    # id, painter, add-on (obj name or None), add-on colour. Order as in the render, left to right, top to bottom.
    ("transmutation", transmutation, None, None),
    ("augmentation", augmentation, "orb_band", COPPER),
    ("alchemy", alchemy, None, None),
    ("regal", regal, "orb_rim", COPPER),
    ("exalted", exalted, None, None),
    ("divine", divine, None, None),
    ("annulment", annulment, None, None),
    ("chance", chance, None, None),
    ("jeweller", jeweller, "orb_sockets", COPPER),
]


def main():
    os.makedirs(OUT, exist_ok=True)
    orb = build_orb()
    addons = {"orb_band": build_band(), "orb_rim": build_rim(), "orb_sockets": build_sockets()}
    write_obj_with(orb, orb_uv, os.path.join(OUT, "orb.obj"))
    for name, tris in addons.items():
        write_obj_with(tris, flat_uv, os.path.join(OUT, name + ".obj"))

    tiles = []
    for i, (oid, painter, addon, addon_colour) in enumerate(ORBS):
        albedo, glow, glow_colour = painter(np.random.default_rng(200 + i))
        albedo_img = Image.fromarray(np.clip(albedo, 0, 255).astype(np.uint8), "RGB")
        glow_img = Image.fromarray((np.clip(glow, 0, 1) * 255).astype(np.uint8), "L")
        albedo_img.save(os.path.join(OUT, f"orb_{oid}_albedo.png"))
        glow_img.save(os.path.join(OUT, f"orb_{oid}_emission.png"))
        write_raw(albedo_img, os.path.join(OUT, f"orb_{oid}_albedo.rgba"))
        write_raw(glow_img, os.path.join(OUT, f"orb_{oid}_emission.rgba"))
        parts = [(orb, albedo_img, glow_img, glow_colour, orb_uv)]
        if addon:
            parts.append((addons[addon], Image.new("RGB", (4, 4), addon_colour), Image.new("L", (4, 4), 0), (0, 0, 0), flat_uv))
        tiles.append(render_parts(parts, PREVIEW_YAW, size=300, height=HEIGHT, zoom=4.2, shine=0.04))

    sheet = Image.new("RGB", (900, 900), (34, 36, 40))
    for i, tile in enumerate(tiles):
        sheet.paste(tile, ((i % 3) * 300, (i // 3) * 300))
    sheet.save(os.path.join(ROOT, "docs", "images", "orbs_preview.png"))
    print("orb triangles:", len(orb), "|", ", ".join(f"{k}: {len(v)}" for k, v in addons.items()))


def write_obj_with(tris, uv_fn, path):
    """write_obj from make_shard, with this model's UV mapping."""
    original = make_shard.facet_uv
    make_shard.facet_uv = uv_fn
    try:
        write_obj(tris, path)
    finally:
        make_shard.facet_uv = original


if __name__ == "__main__":
    main()
