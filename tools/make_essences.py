"""
Generates the boss essences' models and textures, after the user's concept render
(docs/images/essences_reference.jpg), and a preview sheet.

    python tools/make_essences.py            (needs numpy and Pillow)

Writes to Corruption/Assets/:
    essence.obj                               the shared gem shape (taller cut than the shard, bigger facets)
    essence_root.obj, essence_ring.obj        add-ons: the Elder's root, Yagluth's crown ring
    essence_<boss>_albedo.png/.rgba           colour texture per essence (PNG to view, raw for the game)
    essence_<boss>_emission.png/.rgba         glow mask per essence
and docs/images/essences_preview.png.

Fixed seeds: rerunning gives the same files. Each style is one function below; tweak and rerun.
"""
import math
import os
import random
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from make_shard import ASSETS, ROOT, render_parts, write_obj, write_raw  # noqa: E402

SIZE = 256
PREVIEW_YAW = -1.0
HEIGHT = 0.24
WIDTH = 0.15
DEPTH = 0.13


# ---------------------------------------------------------------- shared gem shape

def build_gem():
    """The render's cut: a tall faceted egg with a hexagonal flat top and a rounded point below."""
    rng = random.Random(21)
    segments = 7
    heights = [0.12, 0.38, 0.66, 0.86]
    widths = [0.62, 1.0, 0.92, 0.66]
    rings = []
    for r, (h, w) in enumerate(zip(heights, widths)):
        offset = math.pi / segments * (r % 2)
        ring = []
        for i in range(segments):
            a = offset + 2 * math.pi * i / segments + rng.uniform(-0.06, 0.06)
            k = w * rng.uniform(0.95, 1.04)
            ring.append((WIDTH / 2 * k * math.cos(a), HEIGHT * (h + rng.uniform(-0.01, 0.01)), DEPTH / 2 * k * math.sin(a)))
        rings.append(ring)
    bottom = (0.0, 0.0, 0.0)
    top_y = HEIGHT
    top = [(x * 0.62, top_y, z * 0.62) for (x, _, z) in rings[-1]]
    top_centre = (0.0, top_y, 0.0)
    tris = []
    for i in range(segments):
        j = (i + 1) % segments
        tris.append((rings[0][j], rings[0][i], bottom))
    for r in range(len(rings) - 1):
        low, high = rings[r], rings[r + 1]
        for i in range(segments):
            j = (i + 1) % segments
            tris += [(low[i], low[j], high[j]), (low[i], high[j], high[i])]
    for i in range(segments):
        j = (i + 1) % segments
        tris += [(rings[-1][i], rings[-1][j], top[j]), (rings[-1][i], top[j], top[i])]
        tris.append((top[i], top[j], top_centre))
    return tris


def gem_uv(tri):
    pts = np.array(tri)
    centre = pts.mean(axis=0)
    angle = (math.atan2(centre[2], centre[0]) / (2 * math.pi)) % 1.0
    uvs = []
    for p in pts:
        a = (math.atan2(p[2], p[0]) / (2 * math.pi)) % 1.0
        if abs(a - angle) > 0.5:
            a += 1.0 if a < angle else -1.0
        uvs.append((a, p[1] / HEIGHT))
    return uvs


def flat_uv(tri):
    return [(0.5, 0.5)] * 3


def tube(path, radius, sides=5):
    """A faceted tube along a list of points (for the root)."""
    tris = []
    frames = []
    for k, p in enumerate(path):
        p = np.array(p)
        d = np.array(path[min(k + 1, len(path) - 1)]) - np.array(path[max(k - 1, 0)])
        d /= np.linalg.norm(d)
        up = np.array([0.0, 1.0, 0.0]) if abs(d[1]) < 0.9 else np.array([1.0, 0.0, 0.0])
        u = np.cross(d, up); u /= np.linalg.norm(u)
        v = np.cross(d, u)
        r = radius * (1.0 - 0.5 * k / (len(path) - 1))   # thinner towards the end
        frames.append([tuple(p + r * (math.cos(2 * math.pi * s / sides) * u + math.sin(2 * math.pi * s / sides) * v))
                       for s in range(sides)])
    for k in range(len(frames) - 1):
        for s in range(sides):
            t = (s + 1) % sides
            a, b, c, d = frames[k][s], frames[k][t], frames[k + 1][t], frames[k + 1][s]
            tris += [(a, b, c), (a, c, d)]
    return tris


def build_root():
    """A root climbing once around the gem, from the base towards the shoulder, poking out at the end."""
    path = []
    for k in range(15):
        t = k / 14
        a = -0.6 + t * 2 * math.pi * 1.05
        h = 0.08 + 0.62 * t
        w = (0.75 + 0.3 * math.sin(math.pi * min(1, h / 0.8))) * 1.04
        path.append((WIDTH / 2 * w * math.cos(a), HEIGHT * h, DEPTH / 2 * w * math.sin(a)))
    end = np.array(path[-1])
    path.append(tuple(end + np.array([0.02, 0.015, -0.01])))
    return tube(path, 0.006)


def build_ring():
    """A thin gold ring tilted around the middle, standing slightly off the gem (Yagluth's broken crown)."""
    tilt = math.radians(18)
    path = []
    for k in range(26):
        a = 2 * math.pi * k / 25
        x, z = WIDTH * 0.62 * math.cos(a), DEPTH * 0.66 * math.sin(a)
        y = HEIGHT * 0.45 + x * math.tan(tilt)
        path.append((x, y, z))
    return tube(path, 0.0035, sides=4)


# ---------------------------------------------------------------- painting helpers

def noise(rng, octaves):
    total = np.zeros((SIZE, SIZE))
    for octave in octaves:
        n = rng.uniform(0, 1, (octave, octave))
        total += np.array(Image.fromarray((n * 255).astype(np.uint8)).resize((SIZE, SIZE), Image.BICUBIC)) / 255.0 / octave
    return (total - total.min()) / (total.max() - total.min())


def mix(a, b, t):
    t = np.clip(t, 0, 1)[..., None]
    return np.array(a) * (1 - t) + np.array(b) * t


def lines_mask(segments, width, blur):
    img = Image.new("L", (SIZE, SIZE), 0)
    draw = ImageDraw.Draw(img)
    for seg in segments:
        draw.line(seg, fill=255, width=width, joint="curve")
    return np.array(img.filter(ImageFilter.GaussianBlur(blur))) / 255.0


def lightning(rng, count, length):
    segs = []
    for _ in range(count):
        x, y = rng.uniform(0, SIZE), rng.uniform(0, SIZE)
        pts = [(x, y)]
        for _ in range(length):
            x += rng.uniform(-14, 14)
            y += rng.uniform(-18, 18)
            pts.append((x, y))
            if rng.random() < 0.3:
                bx, by = x, y
                branch = [(bx, by)]
                for _ in range(3):
                    bx += rng.uniform(-12, 12); by += rng.uniform(-12, 12)
                    branch.append((bx, by))
                segs.append(branch)
        segs.append(pts)
    return segs


def cracks(rng, cells, width):
    """Voronoi cell borders: rock split into plates (Yagluth, Fader)."""
    pts = rng.uniform(0, SIZE, (cells, 2))
    yy, xx = np.mgrid[0:SIZE, 0:SIZE]
    d = np.stack([np.hypot(xx - px, yy - py) for px, py in pts])
    d.sort(axis=0)
    edge = np.clip(1 - (d[1] - d[0]) / width, 0, 1)
    return edge


# ---------------------------------------------------------------- styles (one per essence)

y = np.linspace(0, 1, SIZE)[:, None] * np.ones((1, SIZE))   # 0 at top row of the image = top of the gem
up = 1 - y                                                    # 1 at the top of the gem


def eikthyr(rng):
    base = mix([150, 175, 195], [235, 245, 252], noise(rng, (3, 6)) * 0.7 + up * 0.3)
    bolts = lines_mask(lightning(random.Random(1), 9, 8), 2, 0.8)
    halo = lines_mask(lightning(random.Random(1), 9, 8), 6, 4)
    # an antler shape frozen inside, seen through the crystal
    antler = Image.new("L", (SIZE, SIZE), 0)
    d = ImageDraw.Draw(antler)
    for seg in ([(64, 190), (70, 120), (58, 80)], [(70, 140), (92, 110)], [(68, 110), (46, 96)], [(62, 92), (80, 70)]):
        d.line(seg, fill=255, width=9, joint="curve")
    antler = np.array(antler.filter(ImageFilter.GaussianBlur(2))) / 255.0
    albedo = mix(base, [92, 72, 60], antler * 0.75)
    albedo = mix(albedo, [200, 245, 255], np.maximum(bolts, halo * 0.35))
    glow = np.clip(bolts + halo * 0.4 + 0.12, 0, 1)
    return albedo, glow, (0.55, 0.85, 1.0)


def elder(rng):
    sap = mix([70, 70, 12], [175, 150, 40], noise(rng, (3, 6, 12)) * 0.8 + up * 0.2)
    bark = np.zeros((SIZE, SIZE))
    xs = np.arange(SIZE)[None, :]
    for _ in range(9):
        cx, w = rng.uniform(0, SIZE), rng.uniform(5, 14)
        wob = 6 * np.sin(np.arange(SIZE)[:, None] / rng.uniform(12, 30) + rng.uniform(0, 6))
        bark = np.maximum(bark, np.exp(-((xs - cx - wob) ** 2) / (2 * w ** 2)))
    albedo = mix(sap, [58, 36, 18], np.clip(bark * 1.3, 0, 1))
    moss = np.clip((y - 0.72) * 4, 0, 1) * (noise(rng, (8, 16)) > 0.45)
    albedo = mix(albedo, [70, 110, 38], moss * 0.9)
    glow = np.clip((1 - bark) * 0.35 * (0.3 + 0.7 * up), 0, 1)
    return albedo, glow, (0.75, 0.65, 0.15)


def bonemass(rng):
    cloud = noise(rng, (3, 5, 9, 18))
    albedo = mix([40, 70, 28], [130, 170, 70], cloud)
    bone = Image.new("L", (SIZE, SIZE), 0)
    d = ImageDraw.Draw(bone)
    d.line([(104, 176), (156, 84)], fill=255, width=14)
    for cx, cy in ((98, 182), (112, 174), (150, 88), (164, 80)):
        d.ellipse((cx - 11, cy - 11, cx + 11, cy + 11), fill=255)
    bone_img = bone.filter(ImageFilter.GaussianBlur(1.2))
    outline = np.array(bone.filter(ImageFilter.MaxFilter(7)).filter(ImageFilter.GaussianBlur(1.5))) / 255.0
    bone = np.array(bone_img) / 255.0
    albedo = mix(albedo, [24, 40, 18], outline * 0.8)
    albedo = mix(albedo, [228, 220, 170], bone)
    glow = np.clip(cloud * 0.55 * (0.4 + 0.6 * up), 0, 1)
    return albedo, glow, (0.35, 0.9, 0.2)


def moder(rng):
    base = mix([120, 170, 215], [215, 238, 252], noise(rng, (3, 6)) * 0.6 + up * 0.3)
    scales = Image.new("L", (SIZE, SIZE), 0)
    d = ImageDraw.Draw(scales)
    for row in range(8, 24):
        for col in range(0, 26):
            cx = col * 11 + (row % 2) * 5.5
            cy = row * 7.5
            d.arc((cx - 7, cy - 7, cx + 7, cy + 7), 20, 160, fill=255, width=3)
    scales = np.array(scales.filter(ImageFilter.GaussianBlur(0.7))) / 255.0
    band = np.exp(-((np.arange(SIZE)[None, :] - 128) ** 2) / (2 * 55 ** 2))
    albedo = mix(base, [70, 120, 175], scales * band * 0.75)
    frost = np.clip((y - 0.7) * 3.5, 0, 1) * (0.6 + 0.4 * noise(rng, (16, 32)))
    albedo = mix(albedo, [250, 252, 255], frost)
    glow = np.clip(0.18 + 0.25 * up, 0, 1)
    return albedo, glow, (0.75, 0.9, 1.0)


def yagluth(rng):
    rock = mix([22, 20, 20], [70, 64, 62], noise(rng, (6, 12, 24)))
    edge = cracks(np.random.default_rng(5), 22, 5)
    hot = np.clip(edge * 1.4, 0, 1) * (noise(rng, (4, 8)) > 0.35)
    albedo = mix(rock, [255, 120, 30], hot)
    albedo = mix(albedo, [10, 9, 9], np.clip(edge - hot, 0, 1) * 0.8)
    glow = np.clip(hot * 1.2, 0, 1)
    return albedo, glow, (1.0, 0.45, 0.1)


def queen(rng):
    shift = noise(rng, (2, 4, 8))
    albedo = mix([70, 30, 120], [40, 150, 150], np.clip((shift - 0.5) * 2.2, 0, 1))
    albedo = mix(albedo, [25, 12, 45], np.clip(y * 0.6, 0, 1))
    veins = lines_mask(lightning(random.Random(9), 6, 9), 2, 0.8)
    halo = lines_mask(lightning(random.Random(9), 6, 9), 6, 4)
    albedo = mix(albedo, [150, 130, 255], np.maximum(veins, halo * 0.4))
    glow = np.clip(veins + halo * 0.4 + 0.1, 0, 1)
    return albedo, glow, (0.55, 0.45, 1.0)


def fader(rng):
    rock = mix([12, 11, 12], [45, 42, 44], noise(rng, (6, 12, 24)))
    edge = cracks(np.random.default_rng(11), 12, 9)
    albedo = mix(rock, [255, 140, 20], np.clip(edge * 1.5, 0, 1))
    ash = (np.random.default_rng(3).uniform(0, 1, (SIZE, SIZE)) > 0.996).astype(float)
    ash = np.array(Image.fromarray((ash * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(0.8))) / 255.0 * 3
    albedo = mix(albedo, [255, 160, 60], np.clip(ash, 0, 1))
    glow = np.clip(edge * 1.5 + ash, 0, 1)
    return albedo, glow, (1.0, 0.4, 0.05)


ESSENCES = [
    # id, painter, add-on (obj name or None), add-on colour
    ("eikthyr", eikthyr, None, None),
    ("elder", elder, "essence_root", (88, 60, 36)),
    ("bonemass", bonemass, None, None),
    ("moder", moder, None, None),
    ("yagluth", yagluth, "essence_ring", (196, 150, 60)),
    ("queen", queen, None, None),
    ("fader", fader, None, None),
]


def main():
    gem = build_gem()
    addons = {"essence_root": build_root(), "essence_ring": build_ring()}
    write_obj_with(gem, gem_uv, os.path.join(ASSETS, "essence.obj"))
    for name, tris in addons.items():
        write_obj_with(tris, flat_uv, os.path.join(ASSETS, name + ".obj"))

    tiles = []
    for i, (eid, painter, addon, addon_colour) in enumerate(ESSENCES):
        albedo, glow, glow_colour = painter(np.random.default_rng(100 + i))
        albedo_img = Image.fromarray(np.clip(albedo, 0, 255).astype(np.uint8), "RGB")
        glow_img = Image.fromarray((np.clip(glow, 0, 1) * 255).astype(np.uint8), "L")
        albedo_img.save(os.path.join(ASSETS, f"essence_{eid}_albedo.png"))
        glow_img.save(os.path.join(ASSETS, f"essence_{eid}_emission.png"))
        write_raw(albedo_img, os.path.join(ASSETS, f"essence_{eid}_albedo.rgba"))
        write_raw(glow_img, os.path.join(ASSETS, f"essence_{eid}_emission.rgba"))
        parts = [(gem, albedo_img, glow_img, glow_colour, gem_uv)]
        if addon:
            solid = Image.new("RGB", (4, 4), addon_colour)
            parts.append((addons[addon], solid, Image.new("L", (4, 4), 0), (0, 0, 0), flat_uv))
        tiles.append(render_parts(parts, PREVIEW_YAW, size=300, height=HEIGHT, zoom=2.6, shine=0.25))

    sheet = Image.new("RGB", (300 * len(tiles), 300), (34, 36, 40))
    for i, tile in enumerate(tiles):
        sheet.paste(tile, (i * 300, 0))
    sheet.save(os.path.join(ROOT, "docs", "images", "essences_preview.png"))
    print("gem triangles:", len(gem), "| root:", len(addons["essence_root"]), "| ring:", len(addons["essence_ring"]))


def write_obj_with(tris, uv_fn, path):
    """write_obj from make_shard, with this model's UV mapping."""
    import make_shard
    original = make_shard.facet_uv
    make_shard.facet_uv = uv_fn
    try:
        write_obj(tris, path)
    finally:
        make_shard.facet_uv = original


if __name__ == "__main__":
    main()
