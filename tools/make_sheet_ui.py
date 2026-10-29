"""
Generates the character sheet's UI art (CraftingOverhaul), after the user's concept image (2026-10-29): dark wood
with an iron frame, a rope-knot title band, recessed iron-rimmed slot tiles, an arched portrait frame on a stone
pedestal, a parchment stats box with a name ribbon, and a close button. Also a mock-up of the whole sheet.

    python tools/make_sheet_ui.py            (needs numpy and Pillow)

Writes, into CraftingOverhaul/Assets/Sheet/:
    <name>.png     for viewing and editing
    <name>.rgba    the same pixels raw, which the mod loads (8-byte header, rows bottom to top; see write_raw)
and docs/images/sheet_preview.png, a mock-up of the sheet with the default slots (not shipped).

Sizes match CraftingOverhaul/CharacterSheet/SheetLayout.cs (panel width, tile, portrait, parchment and ribbon);
change both together. Pieces whose size varies in game are 9-sliced there: the frame and tiles stretch along their
edges, the portrait frame only along its straight sides. Everything comes from fixed seeds, so reruns give the
same files.
"""
import math
import os

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "CraftingOverhaul", "Assets", "Sheet")
PREVIEW = os.path.join(ROOT, "docs", "images", "sheet_preview.png")

# Mirrors SheetLayout.cs.
PANEL_WIDTH = 502
TITLE_WIDTH = PANEL_WIDTH - 20
TITLE_HEIGHT = 56
TILE = 64            # 9-sliced; tiles are 84 x 96 (labelled) or 70 x 70 (quick slots) in game
TILE_BORDER = 18
FRAME = 160          # 9-sliced panel frame
FRAME_BORDER = 56
ARCH_WIDTH = 270
ARCH_TOP = ARCH_WIDTH // 2 + 12      # 9-slice top border: the whole arch
ARCH_BOTTOM = 72                     # 9-slice bottom border: the pedestal
FEET_LINE = 46                       # the character's feet, above the arch's bottom (CharacterPortrait.FeetAboveBottom)
ARCH_HEIGHT = ARCH_TOP + ARCH_BOTTOM + 4
PARCHMENT_WIDTH = PANEL_WIDTH - 36
PARCHMENT_HEIGHT = 160
RIBBON_WIDTH = 300
RIBBON_HEIGHT = 40
CLOSE = 36

IRON = np.array([62, 60, 58], float)
IRON_LIGHT = np.array([122, 118, 112], float)
IRON_DARK = np.array([22, 21, 20], float)
BRONZE = np.array([168, 120, 62], float)
BRONZE_LIGHT = np.array([226, 182, 110], float)
BRONZE_DARK = np.array([78, 52, 26], float)


# ---------------------------------------------------------------- helpers

def tileable_noise(w, h, seed, scale_x, scale_y=None):
    """Smooth noise that wraps at the edges (white noise low-passed in frequency space), roughly in [-1, 1]."""
    scale_y = scale_x if scale_y is None else scale_y
    rng = np.random.default_rng(seed)
    white = rng.standard_normal((h, w))
    fy = np.fft.fftfreq(h)[:, None] * h / scale_y
    fx = np.fft.fftfreq(w)[None, :] * w / scale_x
    spectrum = np.fft.fft2(white) * np.exp(-(fx ** 2 + fy ** 2))
    n = np.real(np.fft.ifft2(spectrum))
    return n / (np.abs(n).max() + 1e-9)


def noise(w, h, seed, scale):
    return tileable_noise(w, h, seed, scale)


def smoothstep(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def lerp(a, b, t):
    t = np.asarray(t)[..., None] if np.ndim(t) else t
    return a + (b - a) * t


def to_image(rgb, alpha=None):
    rgb = np.clip(rgb, 0, 255).astype(np.uint8)
    if alpha is None:
        alpha = np.full(rgb.shape[:2], 255, np.uint8)
    else:
        alpha = np.clip(alpha * 255, 0, 255).astype(np.uint8)
    return Image.fromarray(np.dstack([rgb, alpha]), "RGBA")


def grid(w, h):
    y, x = np.mgrid[0:h, 0:w].astype(float)
    return x + 0.5, y + 0.5


def rivet(rgb, alpha, cx, cy, r, base=IRON, light=IRON_LIGHT, dark=IRON_DARK):
    """A domed rivet: lit from the top left, with a dark ring round it."""
    h, w = alpha.shape
    x, y = grid(w, h)
    d = np.hypot(x - cx, y - cy)
    ring = smoothstep(r + 1.6, r + 0.4, d)
    dome = smoothstep(r, r - 1.0, d)
    nx, ny = (x - cx) / r, (y - cy) / r
    shade = np.clip(0.55 - 0.45 * (nx + ny) / 1.4, 0, 1)
    col = lerp(dark, light, shade)
    spec = smoothstep(r * 0.5, 0, np.hypot(x - cx + r * 0.35, y - cy + r * 0.35))
    col = col + spec[..., None] * 80
    m = ring[..., None]
    rgb[:] = rgb * (1 - m) + lerp(dark, dark, 0) * m
    m = dome[..., None]
    rgb[:] = rgb * (1 - m) + col * m
    alpha[:] = np.maximum(alpha, ring)


def bevel_band(dist, inner, outer, base, light, dark, lit):
    """Colour of a metal band between distances inner..outer from an edge; lit is +1 on lit edges, -1 on shaded."""
    t = np.clip((dist - inner) / max(outer - inner, 1e-6), 0, 1)
    profile = np.cos((t - 0.5) * math.pi)            # rounded cross-section
    edge = np.where(t < 0.5, 1.0, -1.0) * (1 - profile)
    shade = np.clip(0.5 + 0.35 * edge * lit, 0, 1)
    return lerp(dark, light, shade * 0.7 + profile * 0.3)


def write_raw(image, path):
    """Raw RGBA32 pixels for the mod: 8-byte header (width, height as little-endian int32), then rows bottom to top,
    as Unity stores textures (same format as Corruption's textures)."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)[::-1]
    h, w = rgba.shape[:2]
    with open(path, "wb") as f:
        f.write(np.array([w, h], dtype="<i4").tobytes())
        f.write(rgba.tobytes())


def save(image, name):
    image.save(os.path.join(OUT, name + ".png"))
    write_raw(image, os.path.join(OUT, name + ".rgba"))


# ---------------------------------------------------------------- pieces

def make_wood(size=256, planks=4, seed=11, dark=1.0):
    """Seamless dark planks running top to bottom, like the concept's panel."""
    x, y = grid(size, size)
    pw = size / planks
    plank = np.floor(x / pw)
    local = (x % pw) / pw
    rng = np.random.default_rng(seed)
    tone = rng.uniform(0.85, 1.12, planks)[plank.astype(int)]
    offset = rng.uniform(0, 100, planks)[plank.astype(int)]
    warp = tileable_noise(size, size, seed + 1, 6, 2) * 6 + tileable_noise(size, size, seed + 2, 20, 6) * 1.5
    grain = np.sin((local * pw * 0.9 + warp + offset) * 0.9)
    streak = tileable_noise(size, size, seed + 3, size / 1.5, 3)
    fine = tileable_noise(size, size, seed + 4, size, 64)
    v = 0.55 + 0.12 * grain + 0.18 * streak + 0.08 * fine
    v = v * tone
    # Knots: one per plank, away from the plank edges.
    for p in range(planks):
        kx = (p + rng.uniform(0.35, 0.65)) * pw
        ky = rng.uniform(0, size)
        for dy in (-size, 0, size):
            d = np.hypot((x - kx) / 5.0, (y - ky - dy) / 9.0)
            v = v - 0.25 * smoothstep(1.6, 0.0, d) + 0.06 * np.sin(d * 3.0) * smoothstep(4, 1.5, d)
    base = np.array([66, 43, 27], float)
    hi = np.array([104, 72, 46], float)
    rgb = lerp(base * 0.55, hi, np.clip(v, 0, 1.2) / 1.2) * dark
    # Grooves between planks: dark seam with a lit edge on its right.
    seam = np.minimum(local, 1 - local) * pw
    rgb = rgb * (1 - 0.65 * smoothstep(2.2, 0.0, seam))[..., None]
    lit = smoothstep(3.5, 2.0, local * pw) * smoothstep(1.0, 2.0, local * pw)
    rgb = rgb + lit[..., None] * 14
    return to_image(rgb)


def make_frame():
    """9-sliced iron frame: rounded iron band, bronze inlay, inner shadow and riveted corner brackets."""
    s, b = FRAME, FRAME_BORDER
    x, y = grid(s, s)
    d_edge = np.minimum.reduce([x, y, s - x, s - y])
    lit = np.where(np.minimum(x, y) <= np.minimum(s - x, s - y), 1.0, -1.0)
    rgb = np.zeros((s, s, 3))
    alpha = np.zeros((s, s))

    band = d_edge < 9
    col = bevel_band(d_edge, 0, 9, IRON, IRON_LIGHT, IRON_DARK, lit)
    rgb[band] = col[band]
    alpha[band] = 1
    inlay = (d_edge >= 10.5) & (d_edge < 13)
    rgb[inlay] = lerp(BRONZE_DARK, BRONZE_LIGHT, np.clip(0.5 + 0.4 * lit, 0, 1))[inlay]
    alpha[inlay] = 1
    gap = (d_edge >= 9) & (d_edge < 10.5)
    rgb[gap] = IRON_DARK
    alpha[gap] = 1
    shadow = (d_edge >= 13)
    sh = smoothstep(b - 6, 13, d_edge) * 0.6
    rgb[shadow] = 0
    alpha[shadow] = sh[shadow]

    # Corner brackets: an L of iron along both edges, with three rivets.
    for cx, cy in ((0, 0), (s, 0), (0, s), (s, s)):
        ux = np.abs(x - cx)
        uy = np.abs(y - cy)
        arm = ((ux < 44) & (uy < 22)) | ((ux < 22) & (uy < 44))
        bevel = np.minimum.reduce([np.where(ux < 22, 22 - ux, 44 - ux), np.where(uy < 22, 22 - uy, 44 - uy), ux, uy])
        col = lerp(IRON_DARK, IRON_LIGHT, np.clip(0.35 + 0.08 * bevel, 0, 0.75))
        n = noise(s, s, 5, 12)
        col = col * (0.9 + 0.1 * n)[..., None]
        rgb[arm] = col[arm]
        alpha[arm] = 1
        sx = 1 if cx == 0 else -1
        sy = 1 if cy == 0 else -1
        for rx, ry in ((11, 11), (34, 11), (11, 34)):
            rivet(rgb, alpha, cx + sx * rx, cy + sy * ry, 4.2)
    return to_image(rgb, alpha)


def rope(rgb, alpha, x0, x1, cy, amp, period, width, strands=3):
    """A twisted rope of `strands` strands along y = cy, as seen from the front (a helix: the strand nearest the
    viewer is drawn on top and lit)."""
    h, w = alpha.shape
    x, y = grid(w, h)
    inside = (x >= x0) & (x <= x1)
    fade = smoothstep(x0, x0 + 6, x) * smoothstep(x1, x1 - 6, x)
    best_z = np.full((h, w), -9.0)
    for k in range(strands):
        phase = 2 * math.pi * k / strands
        a = 2 * math.pi * x / period + phase
        sy = cy + amp * np.sin(a)
        z = np.cos(a)
        slope = amp * 2 * math.pi / period * np.cos(a)
        dist = np.abs(y - sy) / np.sqrt(1 + slope ** 2)
        cover = smoothstep(width / 2 + 0.8, width / 2 - 0.4, dist) * fade * inside
        top = (z > best_z) & (cover > 0.02)
        across = np.clip(dist / (width / 2), 0, 1)
        shade = np.clip(0.55 + 0.35 * z - 0.45 * across ** 2, 0, 1)
        col = lerp(BRONZE_DARK, BRONZE_LIGHT, shade)
        m = (cover * top)[..., None]
        rgb[:] = rgb * (1 - m) + col * m
        alpha[:] = np.maximum(alpha, cover * top)
        best_z = np.where(top, z, best_z)


def make_title():
    """Dark wood band for the title with iron edges and rope knots either side of the text."""
    w, h = TITLE_WIDTH, TITLE_HEIGHT
    wood = np.asarray(make_wood(256, 4, seed=23, dark=0.62).convert("RGB"), float)
    reps = (h // 256 + 1, w // 256 + 1, 1)
    rgb = np.tile(wood, reps)[:h, :w].copy()
    alpha = np.ones((h, w))
    x, y = grid(w, h)
    for edge_y, lit in ((0, 1.0), (h, -1.0)):
        d = np.abs(y - edge_y)
        band = d < 5
        col = bevel_band(d, 0, 5, IRON, IRON_LIGHT, IRON_DARK, np.full((h, w), lit))
        rgb[band] = col[band]
    # Two ropes either side, leaving the middle third for the title text.
    text_half = 120
    for x0, x1 in ((22, w / 2 - text_half), (w / 2 + text_half, w - 22)):
        rope(rgb, alpha, x0, x1, h / 2, 7, 22, 7)
        for cx in (x0 - 8, x1 + 8):
            rivet(rgb, alpha, cx, h / 2, 4.5, BRONZE, BRONZE_LIGHT, BRONZE_DARK)
    return to_image(rgb, alpha)


def make_tile():
    """9-sliced slot tile: a recessed dark well with an iron rim, a bronze inner line and corner rivets."""
    s = TILE
    x, y = grid(s, s)
    d_edge = np.minimum.reduce([x, y, s - x, s - y])
    lit = np.where(np.minimum(x, y) <= np.minimum(s - x, s - y), 1.0, -1.0)
    n = noise(s, s, 31, 10)
    well = lerp(np.array([14, 12, 11], float), np.array([34, 29, 25], float), np.clip(0.5 + 0.25 * n, 0, 1))
    # Recessed: darker under the top-left rim.
    inner = np.minimum(x, y) - 6
    well = well * (0.55 + 0.45 * smoothstep(0, 10, inner))[..., None]
    rgb = well.copy()
    alpha = np.ones((s, s))
    rim = d_edge < 6
    col = bevel_band(d_edge, 0, 6, IRON, IRON_LIGHT, IRON_DARK, lit)
    rgb[rim] = col[rim]
    line = (d_edge >= 6) & (d_edge < 7.5)
    rgb[line] = lerp(BRONZE_DARK, BRONZE, 0.6)
    alpha *= smoothstep(0, 1.2, d_edge)
    for cx, cy in ((7.5, 7.5), (s - 7.5, 7.5), (7.5, s - 7.5), (s - 7.5, s - 7.5)):
        rivet(rgb, alpha, cx, cy, 2.6)
    return to_image(rgb, alpha)


def arch_distance(x, y, w, h, margin):
    """Signed distance to the arch outline (negative inside): straight sides, a round top, a flat bottom."""
    r = w / 2 - margin
    cy = margin + r
    top = np.where(y < cy, np.hypot(x - w / 2, y - cy) - r, np.abs(x - w / 2) - r)
    return np.maximum(top, y - (h - margin))


def make_arch_back():
    """The portrait's backdrop and mask: a night-blue glow inside the arch, transparent outside."""
    w, h = ARCH_WIDTH, ARCH_HEIGHT
    x, y = grid(w, h)
    d = arch_distance(x, y, w, h, 8)
    glow = np.exp(-(((x - w / 2) / (w * 0.42)) ** 2 + ((y - h * 0.42) / (h * 0.6)) ** 2))
    rgb = lerp(np.array([8, 11, 16], float), np.array([44, 62, 84], float), glow)
    alpha = smoothstep(0.8, -0.8, d)
    return to_image(rgb, alpha)


def make_arch_rim():
    """Drawn over the portrait: the iron arch with a bronze inlay, an inner shadow, rivets and a stone pedestal."""
    w, h = ARCH_WIDTH, ARCH_HEIGHT
    x, y = grid(w, h)
    d = arch_distance(x, y, w, h, 8)
    rgb = np.zeros((h, w, 3))
    alpha = np.zeros((h, w))
    # Inner shadow (vignette) inside the arch.
    vig = smoothstep(-34, 0, d) * 0.75
    rgb[:] = 0
    alpha = np.maximum(alpha, vig * (d < 0))
    # Iron band straddling the outline, with a bronze line on its inside.
    band = (d >= -3) & (d < 9)
    t = (d + 3) / 12
    lit = np.where(y < h / 2, 1.0, -1.0) * np.where(x < w / 2, 1.0, 0.6)
    col = lerp(IRON_DARK, IRON_LIGHT, np.clip(0.5 + 0.3 * np.cos((t - 0.5) * math.pi) * lit, 0.1, 0.9))
    rgb[band] = col[band]
    alpha[band] = 1
    inlay = (d >= -6) & (d < -3)
    rgb[inlay] = lerp(BRONZE_DARK, BRONZE_LIGHT, 0.55)
    alpha[inlay] = 1
    alpha *= smoothstep(10, 8.5, d)
    # Rivets round the arch top and along the bottom (fixed parts of the 9-slice).
    r = w / 2 - 8
    for i in range(7):
        a = math.pi * (0.08 + 0.84 * i / 6)
        rivet(rgb, alpha, w / 2 - (r + 3) * math.cos(a), 8 + r - (r + 3) * math.sin(a), 3.4)
    # Stone pedestal: a flat round stone the character stands on, in front of the feet.
    # The character's feet stand on the stone's centre line, so only the stone's front half (and its side) is drawn.
    # FEET_LINE must match CharacterPortrait.FeetAboveBottom.
    px, py, prx, pry, thick = w / 2, h - FEET_LINE, w * 0.42, 20, 20
    n = noise(w, h, 41, 7)
    u = np.clip((x - px) / prx, -1, 1)
    within = np.abs(x - px) < prx
    lower = py + pry * np.sqrt(1 - u ** 2)            # the stone top's front edge
    e = ((x - px) / prx) ** 2 + ((y - py) / pry) ** 2
    top = (e < 1) & (y > py - 2)
    side = within & (y >= lower) & (y < lower + thick)
    stone_top = lerp(np.array([96, 94, 90], float), np.array([168, 164, 156], float),
                     np.clip(0.45 + 0.35 * (y - py) / pry + 0.2 * n - 0.15 * np.abs(u), 0, 1))
    stone_side = lerp(np.array([34, 33, 34], float), np.array([86, 83, 80], float),
                      np.clip(0.65 - 0.5 * (y - lower) / thick + 0.2 * n - 0.3 * np.abs(u), 0, 1))
    rgb[side] = stone_side[side]
    alpha[side] = 1
    rgb[top] = stone_top[top]
    alpha[top] = np.maximum(alpha[top], smoothstep(py - 2, py + 3, y)[top])
    lip = within & (np.abs(y - lower) < 1.2)
    rgb[lip] = rgb[lip] * 0.55 + 40
    # A soft shadow under the stone.
    shadow = within & (y >= lower + thick) & (y < lower + thick + 6)
    alpha[shadow] = np.maximum(alpha[shadow], 0.45 * (1 - (y[shadow] - lower[shadow] - thick) / 6))
    return to_image(rgb, alpha)


def make_parchment():
    """The stats box: aged parchment with burnt, uneven edges."""
    w, h = PARCHMENT_WIDTH, PARCHMENT_HEIGHT
    x, y = grid(w, h)
    d_edge = np.minimum.reduce([x, y, w - x, h - y])
    rough = tileable_noise(w, h, 51, 9) * 4 + tileable_noise(w, h, 52, 40) * 1.5
    d = d_edge + rough
    n = tileable_noise(w, h, 53, 18)
    blot = tileable_noise(w, h, 54, 5)
    paper = lerp(np.array([196, 166, 118], float), np.array([232, 211, 168], float), np.clip(0.55 + 0.3 * n, 0, 1))
    paper = paper * (1 - 0.12 * smoothstep(0.3, 0.9, blot))[..., None]
    burn = smoothstep(16, 3, d)
    rgb = lerp(paper, np.array([70, 40, 18], float), burn * 0.85)
    alpha = smoothstep(1.5, 3.5, d)
    return to_image(rgb, alpha)


def make_ribbon():
    """The name banner: a dark leather plaque with notched tails and a bronze edge."""
    w, h = RIBBON_WIDTH, RIBBON_HEIGHT
    x, y = grid(w, h)
    tail = 26
    # Body inset by the tails; tails are notched (a V cut into their outer ends).
    body = (x > tail) & (x < w - tail) & (y > 4) & (y < h - 4)
    ex = np.minimum(x, w - x)
    notch = np.abs(y - h / 2) * 0.9
    tails = (ex <= tail + 1) & (y > 9) & (y < h - 1) & (ex > notch * 0.8)
    shape = body | tails
    n = noise(w, h, 61, 6)
    leather = lerp(np.array([44, 28, 18], float), np.array([82, 54, 34], float), np.clip(0.5 + 0.3 * n, 0, 1))
    leather[tails & ~body] *= 0.7
    rgb = leather.copy()
    alpha = shape.astype(float)
    edge = body & ((y < 7) | (y > h - 7))
    rgb[edge] = lerp(BRONZE_DARK, BRONZE_LIGHT, np.where(y < h / 2, 0.75, 0.35))[edge]
    return to_image(rgb, alpha)


def make_close():
    """The close button: an iron boss with an X."""
    s = CLOSE
    x, y = grid(s, s)
    d_edge = np.minimum.reduce([x, y, s - x, s - y])
    lit = np.where(np.minimum(x, y) <= np.minimum(s - x, s - y), 1.0, -1.0)
    rgb = lerp(np.array([30, 27, 25], float), np.array([52, 47, 43], float), smoothstep(0, s, y))
    rim = d_edge < 4
    rgb[rim] = bevel_band(d_edge, 0, 4, IRON, IRON_LIGHT, IRON_DARK, lit)[rim]
    alpha = smoothstep(0, 1.0, d_edge)
    for sx in (1, -1):
        dist = np.abs((x - s / 2) - sx * (y - s / 2)) / math.sqrt(2)
        inside = (np.abs(x - s / 2) < s * 0.24) & (np.abs(y - s / 2) < s * 0.24)
        cover = smoothstep(2.6, 1.4, dist) * inside
        rgb = rgb * (1 - cover[..., None]) + lerp(BRONZE, BRONZE_LIGHT, 0.6) * cover[..., None]
    return to_image(rgb, alpha)


# ---------------------------------------------------------------- preview

def nine_slice(img, w, h, border):
    """Stretch a 9-sliced image to w x h (border = left, bottom, right, top, in pixels), as Unity's Sliced image."""
    l, b, r, t = border
    sw, sh = img.size
    xs = [0, l, sw - r, sw]
    ys = [0, t, sh - b, sh]
    dx = [0, l, w - r, w]
    dy = [0, t, h - b, h]
    out = Image.new("RGBA", (w, h))
    for i in range(3):
        for j in range(3):
            src = img.crop((xs[i], ys[j], xs[i + 1], ys[j + 1]))
            size = (dx[i + 1] - dx[i], dy[j + 1] - dy[j])
            if size[0] > 0 and size[1] > 0 and src.size[0] > 0 and src.size[1] > 0:
                out.alpha_composite(src.resize(size, Image.BILINEAR), (dx[i], dy[j]))
    return out


def font(size):
    for name in ("DejaVuSerif-Bold.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def preview(pieces):
    """A mock-up with the default slots, laid out as SheetLayout.cs does, for checking the look outside the game."""
    tile_w, tile_h, pitch, cell = 84, 96, 102, 64
    margin, gap, top = 18, 14, 72
    rows = 5
    col_h = rows * pitch - (pitch - tile_h)
    stats_y = top + col_h + gap + 14   # the ribbon rises 14 px above the parchment
    quick_y = stats_y + PARCHMENT_HEIGHT + gap
    height = quick_y + 70 + margin
    sheet = Image.new("RGBA", (PANEL_WIDTH, height))
    wood = pieces["wood"]
    for ty in range(0, height, wood.size[1]):
        for tx in range(0, PANEL_WIDTH, wood.size[0]):
            sheet.alpha_composite(wood, (tx, ty))
    sheet.alpha_composite(nine_slice(pieces["frame"], PANEL_WIDTH, height, (FRAME_BORDER,) * 4))
    sheet.alpha_composite(pieces["title"], (10, 8))
    draw = ImageDraw.Draw(sheet)
    draw.text((PANEL_WIDTH / 2, 8 + TITLE_HEIGHT / 2), "CHARACTER", font=font(26), fill=(232, 205, 150),
              anchor="mm", stroke_width=2, stroke_fill=(0, 0, 0))
    left = ["HEAD", "CHEST", "LEGS", "CAPE", "UTILITY"]
    right = ["TRINKET", "TALISMAN", "TALISMAN"]
    tile = pieces["tile"]
    for column, labels in ((margin, left), (PANEL_WIDTH - margin - tile_w, right)):
        for i, label in enumerate(labels):
            ty = top + i * pitch
            sheet.alpha_composite(nine_slice(tile, tile_w, tile_h, (TILE_BORDER,) * 4), (column, ty))
            cx = column + (tile_w - cell) // 2
            draw.rectangle((cx, ty + 6, cx + cell, ty + 6 + cell), outline=(70, 62, 54))
            draw.text((column + tile_w / 2, ty + tile_h - 13), label, font=font(12), fill=(220, 200, 160),
                      anchor="mm", stroke_width=1, stroke_fill=(0, 0, 0))
    px = margin + tile_w + gap
    back = nine_slice(pieces["arch_back"], ARCH_WIDTH, col_h, (ARCH_WIDTH // 2 - 1, ARCH_BOTTOM, ARCH_WIDTH // 2 - 1, ARCH_TOP))
    rim = nine_slice(pieces["arch_rim"], ARCH_WIDTH, col_h, (ARCH_WIDTH // 2 - 1, ARCH_BOTTOM, ARCH_WIDTH // 2 - 1, ARCH_TOP))
    sheet.alpha_composite(back, (px, top))
    # A stand-in figure where the live render goes.
    fig = Image.new("RGBA", (ARCH_WIDTH, col_h))
    fd = ImageDraw.Draw(fig)
    cxf, feet = ARCH_WIDTH / 2, col_h - FEET_LINE
    fd.ellipse((cxf - 22, feet - 330, cxf + 22, feet - 282), fill=(90, 80, 70, 255))
    fd.polygon([(cxf - 55, feet - 280), (cxf + 55, feet - 280), (cxf + 40, feet - 140), (cxf - 40, feet - 140)], fill=(80, 70, 60, 255))
    fd.rectangle((cxf - 38, feet - 140, cxf - 8, feet), fill=(70, 60, 52, 255))
    fd.rectangle((cxf + 8, feet - 140, cxf + 38, feet), fill=(70, 60, 52, 255))
    sheet.alpha_composite(fig, (px, top))
    sheet.alpha_composite(rim, (px, top))
    sheet.alpha_composite(pieces["parchment"], (margin, stats_y))
    rx = (PANEL_WIDTH - RIBBON_WIDTH) // 2
    sheet.alpha_composite(pieces["ribbon"], (rx, stats_y - 14))
    draw.text((PANEL_WIDTH / 2, stats_y - 14 + RIBBON_HEIGHT / 2), "Krogon", font=font(18), fill=(232, 205, 150), anchor="mm")
    ink = (59, 42, 26)
    lines = [("Armor:", "20", (47, 107, 31)), ("Weight:", "43/310", (138, 90, 0)), ("Health:", "25", (47, 107, 31)),
             ("Stamina:", "50", (47, 107, 31)), ("Speed from gear:", "-15%", (154, 31, 31))]
    for i, (label, value, colour) in enumerate(lines):
        yy = stats_y + 34 + i * 22
        draw.text((margin + 22, yy), label, font=font(14), fill=ink)
        draw.text((margin + 22 + draw.textlength(label, font=font(14)) + 6, yy), value, font=font(14), fill=colour)
    for i, (label, colour) in enumerate((("Cold Immunity", (31, 79, 138)), ("Rugged Backpack", (47, 107, 31)))):
        draw.text((PANEL_WIDTH / 2 + 30, stats_y + 34 + i * 22), label, font=font(14), fill=colour)
    for i, key in enumerate("ZVB"):
        qx = (PANEL_WIDTH - (3 * 70 + 2 * 8)) // 2 + i * 78
        sheet.alpha_composite(nine_slice(tile, 70, 70, (TILE_BORDER,) * 4), (qx, quick_y))
        draw.text((qx + 9, quick_y + 6), key, font=font(13), fill=(230, 230, 230))
    sheet.alpha_composite(pieces["close"], (PANEL_WIDTH - 10 - CLOSE - 10, 8 + (TITLE_HEIGHT - CLOSE) // 2))

    # Show it on a darkened game-ish background.
    bg = Image.new("RGBA", (PANEL_WIDTH + 80, height + 80), (40, 60, 50, 255))
    bg.alpha_composite(sheet, (40, 40))
    bg.convert("RGB").save(PREVIEW)


def main():
    os.makedirs(OUT, exist_ok=True)
    pieces = {
        "wood": make_wood(),
        "frame": make_frame(),
        "title": make_title(),
        "tile": make_tile(),
        "arch_back": make_arch_back(),
        "arch_rim": make_arch_rim(),
        "parchment": make_parchment(),
        "ribbon": make_ribbon(),
        "close": make_close(),
    }
    for name, image in pieces.items():
        save(image, name)
    preview(pieces)
    print("Wrote", ", ".join(sorted(pieces)), "and", os.path.relpath(PREVIEW, ROOT))


if __name__ == "__main__":
    main()
