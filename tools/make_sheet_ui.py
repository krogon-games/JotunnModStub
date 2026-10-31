"""
Generates the UI art of the character sheet and the class screens (CraftingOverhaul). Look: the user's mix of the
classes UI mock-ups (2026-10-30, docs/images/classes_ui_mockup.png): bronze riveted slot tiles, round bronze skill
medallions, soft glows (tinted eitr-blue in game), bronze bar frames with knot caps, a bronze arched portrait frame on a
stone pedestal and a close button. Panels use the game's own background in game; 'panel' and 'inset' here are only
look-alikes for when it can't be found. Also a mock-up of the whole sheet.

    python tools/make_sheet_ui.py            (needs numpy and Pillow)

Writes, into CraftingOverhaul/Assets/Sheet/:
    <name>.png     for viewing and editing
    <name>.rgba    the same pixels raw, which the mod loads (8-byte header, rows bottom to top; see write_raw)
and docs/images/sheet_preview.png, a mock-up of the sheet with the default slots (not shipped).

Sizes match CraftingOverhaul/CharacterSheet/SheetLayout.cs (panel width, tile, portrait and stats box);
change both together. Pieces whose size varies in game are 9-sliced there: tiles, bars and glows stretch along their
edges, the portrait frame only along its straight sides. SheetArt.cs holds the 9-slice borders and the glow sizes.
Everything comes from fixed seeds, so reruns give the same files.
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
TITLE_HEIGHT = 56
TILE = 64            # 9-sliced; tiles are 84 x 96 (labelled) or 70 x 70 (quick slots) in game
TILE_BORDER = 18
ARCH_WIDTH = 270
ARCH_TOP = ARCH_WIDTH // 2 + 12      # 9-slice top border: the whole arch
ARCH_BOTTOM = 72                     # 9-slice bottom border: the pedestal
FEET_LINE = 46                       # the character's feet, above the arch's bottom (CharacterPortrait.FeetAboveBottom)
ARCH_HEIGHT = ARCH_TOP + ARCH_BOTTOM + 4
PARCHMENT_HEIGHT = 160
CLOSE = 36
BAR_W = 64           # 9-sliced; the bars are 14 px tall in game (the XP bar 8)
BAR_H = 14
BAR_BORDER = 6
BAR_CAP_W, BAR_CAP_H = 10, 20
MEDALLION = 96        # skills are drawn 64 px across in game
GLOW_RING = 128
GLOW_BOX_BORDER = 24
GLOW_BOX_INSET = 14     # the glow hugs a slot when drawn this much bigger than it on every side

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

def rounded_sdf(x, y, w, h, inset, radius):
    """Signed distance to a rounded rectangle inset from the image edge (negative inside)."""
    hx, hy = w / 2 - inset - radius, h / 2 - inset - radius
    qx = np.abs(x - w / 2) - hx
    qy = np.abs(y - h / 2) - hy
    outside = np.hypot(np.maximum(qx, 0), np.maximum(qy, 0))
    inside = np.minimum(np.maximum(qx, qy), 0)
    return outside + inside - radius


def make_tile():
    """9-sliced slot (user's choice 2026-10-30, from mock-up D): a dark well in a rounded bronze rim with corner rivets.
    Used by the character sheet's slots, the skill slots and the HUD's skill cross."""
    s = TILE
    x, y = grid(s, s)
    d = rounded_sdf(x, y, s, s, 0.5, 9)
    n = noise(s, s, 31, 10)
    well = lerp(np.array([16, 12, 10], float), np.array([34, 27, 21], float), np.clip(0.5 + 0.25 * n, 0, 1))
    well = well * (0.6 + 0.4 * smoothstep(-6, -14, d))[..., None]      # recessed under the rim
    rgb = well.copy()
    alpha = np.full((s, s), 0.93)
    rim = d > -5
    lit = np.where((x + y) < s, 1.0, -1.0)
    rgb[rim] = bevel_band(-d, 0, 5, BRONZE, BRONZE_LIGHT, BRONZE_DARK, lit)[rim]
    alpha[rim] = 1
    inner = (d <= -5) & (d > -6.5)
    rgb[inner] = BRONZE_DARK * 0.6
    alpha *= smoothstep(0.8, -0.6, d)
    for cx, cy in ((9.5, 9.5), (s - 9.5, 9.5), (9.5, s - 9.5), (s - 9.5, s - 9.5)):
        rivet(rgb, alpha, cx, cy, 2.4, BRONZE, BRONZE_LIGHT, BRONZE_DARK)
    return to_image(rgb, alpha)


def make_medallion():
    """A skill: a round bronze medallion with rivets round the rim and a dark face for the icon (mock-up D).
    Tinted grey in game for locked skills and darker for skills that come later. Also the food slots' ring."""
    s = MEDALLION
    x, y = grid(s, s)
    c = s / 2
    r = np.hypot(x - c, y - c)
    outer = c - 2
    rim_w = 8
    face = lerp(np.array([40, 33, 28], float), np.array([12, 10, 9], float), np.clip(r / (outer - rim_w), 0, 1))
    rgb = face.copy()
    alpha = smoothstep(outer + 0.8, outer - 0.6, r)
    band = r > outer - rim_w
    # Lit from the top left: brighter where the rim faces up-left.
    facing = -((x - c) + (y - c)) / (np.maximum(r, 1e-6) * math.sqrt(2))
    t = np.clip((r - (outer - rim_w)) / rim_w, 0, 1)
    profile = np.cos((t - 0.5) * math.pi)
    shade = np.clip(0.45 + 0.35 * facing * (2 * t - 1) * -1 + 0.25 * profile + 0.15 * facing, 0, 1)
    rgb[band] = lerp(BRONZE_DARK, BRONZE_LIGHT, shade)[band]
    line = (r > outer - rim_w - 1.5) & (r <= outer - rim_w)
    rgb[line] = BRONZE_DARK * 0.5
    for i in range(8):
        a = i * math.pi / 4 + math.pi / 8
        rivet(rgb, alpha, c + (outer - rim_w / 2) * math.cos(a), c + (outer - rim_w / 2) * math.sin(a), 1.9,
              BRONZE, BRONZE_LIGHT, BRONZE_DARK)
    return to_image(rgb, alpha)


def make_glow_ring():
    """A soft white ring, tinted eitr-blue (learned) or green (a point can be spent) behind a medallion. Its ring sits
    at the medallion's rim when drawn at GLOW_RING / MEDALLION times the medallion's size."""
    s = GLOW_RING
    x, y = grid(s, s)
    r = np.hypot(x - s / 2, y - s / 2)
    ring = (MEDALLION / 2 - 2) * 1.0
    a = np.exp(-((r - ring) / 7.0) ** 2) * 0.95
    return to_image(np.full((s, s, 3), 255.0), a)


def make_glow_line():
    """A soft white strip for the glowing prerequisite lines (stretched along the line, tinted eitr-blue)."""
    w, h = 4, 32
    x, y = grid(w, h)
    a = np.exp(-((y - h / 2) / 6.0) ** 2)
    return to_image(np.full((h, w, 3), 255.0), a)


def make_glow_box():
    """9-sliced soft glow round a slot, tinted eitr-blue on the slot that is on the skill button."""
    s = 64
    x, y = grid(s, s)
    d = rounded_sdf(x, y, s, s, GLOW_BOX_INSET, 9)
    a = np.exp(-(np.maximum(d, -2) / 5.0) ** 2) * smoothstep(-8, -2, d)
    return to_image(np.full((s, s, 3), 255.0), a)


def make_disc():
    """A plain white disc, for the round cooldown sweep over the power medallion's face (tinted dark in game)."""
    s = 64
    x, y = grid(s, s)
    r = np.hypot(x - s / 2, y - s / 2)
    return to_image(np.full((s, s, 3), 255.0), smoothstep(s / 2, s / 2 - 1.2, r))


def make_panel():
    """Stand-in for the game's own panel background (the inventory's 'Bkg', used in game when it can be found):
    translucent warm brown with a soft darker edge."""
    s = 64
    x, y = grid(s, s)
    d = rounded_sdf(x, y, s, s, 0.5, 10)
    n = noise(s, s, 51, 12)
    rgb = lerp(np.array([52, 38, 26], float), np.array([62, 46, 32], float), np.clip(0.5 + 0.3 * n, 0, 1))
    rgb = rgb * (0.7 + 0.3 * smoothstep(0, -8, d))[..., None]
    alpha = 0.9 * smoothstep(0.8, -0.8, d)
    return to_image(rgb, alpha)


def make_inset():
    """A darker translucent box on a panel, for the attributes and the stats (in place of the parchment)."""
    s = 32
    x, y = grid(s, s)
    d = rounded_sdf(x, y, s, s, 0.5, 7)
    rgb = np.full((s, s, 3), 1.0) * np.array([22, 15, 10], float)
    alpha = 0.72 * smoothstep(0.8, -0.8, d)
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
    """Drawn over the portrait: the bronze arch with a dark inlay, an inner shadow, rivets and a stone pedestal."""
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
    col = lerp(BRONZE_DARK, BRONZE_LIGHT, np.clip(0.5 + 0.3 * np.cos((t - 0.5) * math.pi) * lit, 0.1, 0.9))
    rgb[band] = col[band]
    alpha[band] = 1
    inlay = (d >= -6) & (d < -3)
    rgb[inlay] = BRONZE_DARK * 0.55
    alpha[inlay] = 1
    alpha *= smoothstep(10, 8.5, d)
    # Rivets round the arch top and along the bottom (fixed parts of the 9-slice).
    r = w / 2 - 8
    for i in range(7):
        a = math.pi * (0.08 + 0.84 * i / 6)
        rivet(rgb, alpha, w / 2 - (r + 3) * math.cos(a), 8 + r - (r + 3) * math.sin(a), 3.4, BRONZE, BRONZE_LIGHT, BRONZE_DARK)
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


def make_close():
    """The close button: a bronze-rimmed boss with an X."""
    s = CLOSE
    x, y = grid(s, s)
    d_edge = np.minimum.reduce([x, y, s - x, s - y])
    lit = np.where(np.minimum(x, y) <= np.minimum(s - x, s - y), 1.0, -1.0)
    rgb = lerp(np.array([30, 27, 25], float), np.array([52, 47, 43], float), smoothstep(0, s, y))
    rim = d_edge < 4
    rgb[rim] = bevel_band(d_edge, 0, 4, BRONZE, BRONZE_LIGHT, BRONZE_DARK, lit)[rim]
    alpha = smoothstep(0, 1.0, d_edge)
    for sx in (1, -1):
        dist = np.abs((x - s / 2) - sx * (y - s / 2)) / math.sqrt(2)
        inside = (np.abs(x - s / 2) < s * 0.24) & (np.abs(y - s / 2) < s * 0.24)
        cover = smoothstep(2.6, 1.4, dist) * inside
        rgb = rgb * (1 - cover[..., None]) + np.array([240, 225, 190], float) * cover[..., None]
    return to_image(rgb, alpha)


def make_bar():
    """9-sliced frame for the health, eitr, stamina and XP bars: a dark inset in a thin rounded bronze rim (mock-up D).
    The knot caps at the ends are separate (bar_cap)."""
    w, h = BAR_W, BAR_H
    x, y = grid(w, h)
    d = rounded_sdf(x, y, w, h, 0.5, 3.5)
    rgb = lerp(np.array([12, 10, 9], float), np.array([28, 22, 18], float), smoothstep(0, h, y))
    alpha = np.full((h, w), 0.92)
    rim = d > -2.2
    lit = np.where(y < h / 2, 0.8, 0.35)
    rgb[rim] = lerp(BRONZE_DARK, BRONZE_LIGHT, lit)[rim]
    alpha[rim] = 1
    alpha *= smoothstep(0.8, -0.6, d)
    return to_image(rgb, alpha)


def make_bar_cap():
    """A bronze knot point at the end of a bar (pointing right; mirrored for the left end)."""
    w, h = BAR_CAP_W, BAR_CAP_H
    x, y = grid(w, h)
    # Inside a triangle from (0, 1) to (w - 1, h / 2) to (0, h - 1), as a distance for a soft edge.
    slope = (h / 2 - 1) / (w - 1)
    d = np.abs(y - h / 2) - (h / 2 - 1 - slope * x)
    lit = np.where(y < h / 2, 0.85, 0.35)
    rgb = lerp(BRONZE_DARK, BRONZE_LIGHT, lit)
    edge = d > -1.6
    rgb[edge] = BRONZE_DARK * 0.7
    alpha = smoothstep(0.8, -0.6, d) * (x < w - 0.2)
    return to_image(rgb, alpha)


def make_bar_fill():
    """The bars' fill: white with a soft top light and a darker bottom, tinted per bar in game."""
    w, h = 8, BAR_H - 6
    x, y = grid(w, h)
    t = y / h
    v = 0.78 + 0.22 * np.cos(t * math.pi * 0.9) - 0.25 * smoothstep(0.55, 1.0, t)
    v = v + 0.18 * smoothstep(0.32, 0.0, t)
    rgb = np.dstack([v, v, v]) * 255
    return to_image(rgb)


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
    """A mock-up with the default slots, laid out as SheetLayout.cs does, for checking the look outside the game.
    In game the panel and the stats box use the game's own panel art; here the stand-ins (panel, inset)."""
    tile_w, tile_h, pitch, cell = 84, 96, 102, 64
    margin, gap, top = 18, 14, 72
    rows = 5
    col_h = rows * pitch - (pitch - tile_h)
    stats_y = top + col_h + gap + 14
    quick_y = stats_y + PARCHMENT_HEIGHT + gap
    height = quick_y + 70 + margin
    sheet = nine_slice(pieces["panel"], PANEL_WIDTH, height, (20, 20, 20, 20))
    draw = ImageDraw.Draw(sheet)
    draw.text((PANEL_WIDTH / 2, 8 + TITLE_HEIGHT / 2), "CHARACTER", font=font(26), fill=(240, 200, 110),
              anchor="mm", stroke_width=2, stroke_fill=(0, 0, 0))
    left = ["HEAD", "CHEST", "LEGS", "CAPE", "UTILITY"]
    right = ["TRINKET", "TALISMAN", "TALISMAN"]
    tile = pieces["tile"]
    for column, labels in ((margin, left), (PANEL_WIDTH - margin - tile_w, right)):
        for i, label in enumerate(labels):
            ty = top + i * pitch
            sheet.alpha_composite(nine_slice(tile, tile_w, tile_h, (TILE_BORDER,) * 4), (column, ty))
            cx = column + (tile_w - cell) // 2
            draw.rectangle((cx, ty + 6, cx + cell, ty + 6 + cell), outline=(70, 56, 40))
            draw.text((column + tile_w / 2, ty + tile_h - 13), label, font=font(12), fill=(240, 220, 170),
                      anchor="mm", stroke_width=1, stroke_fill=(0, 0, 0))
    px = margin + tile_w + gap
    back = nine_slice(pieces["arch_back"], ARCH_WIDTH, col_h, (ARCH_WIDTH // 2 - 1, ARCH_BOTTOM, ARCH_WIDTH // 2 - 1, ARCH_TOP))
    rim = nine_slice(pieces["arch_rim"], ARCH_WIDTH, col_h, (ARCH_WIDTH // 2 - 1, ARCH_BOTTOM, ARCH_WIDTH // 2 - 1, ARCH_TOP))
    sheet.alpha_composite(back, (px, top))
    fig = Image.new("RGBA", (ARCH_WIDTH, col_h))
    fd = ImageDraw.Draw(fig)
    cxf, feet = ARCH_WIDTH / 2, col_h - FEET_LINE
    fd.ellipse((cxf - 22, feet - 330, cxf + 22, feet - 282), fill=(90, 80, 70, 255))
    fd.polygon([(cxf - 55, feet - 280), (cxf + 55, feet - 280), (cxf + 40, feet - 140), (cxf - 40, feet - 140)], fill=(80, 70, 60, 255))
    fd.rectangle((cxf - 38, feet - 140, cxf - 8, feet), fill=(70, 60, 52, 255))
    fd.rectangle((cxf + 8, feet - 140, cxf + 38, feet), fill=(70, 60, 52, 255))
    sheet.alpha_composite(fig, (px, top))
    sheet.alpha_composite(rim, (px, top))
    sheet.alpha_composite(nine_slice(pieces["inset"], PANEL_WIDTH - 2 * margin, PARCHMENT_HEIGHT, (10, 10, 10, 10)), (margin, stats_y))
    draw.text((PANEL_WIDTH / 2, stats_y + 14), "Krogon", font=font(18), fill=(240, 200, 110), anchor="mm",
              stroke_width=1, stroke_fill=(0, 0, 0))
    text_col = (236, 226, 206)
    lines = [("Armor:", "20", (143, 217, 122)), ("Weight:", "43/310", (242, 193, 78)), ("Health:", "25", (240, 100, 90)),
             ("Stamina:", "50", (242, 204, 74)), ("Speed from gear:", "-15%", (255, 122, 107))]
    for i, (label, value, colour) in enumerate(lines):
        yy = stats_y + 34 + i * 22
        draw.text((margin + 22, yy), label, font=font(14), fill=text_col)
        draw.text((margin + 22 + draw.textlength(label, font=font(14)) + 6, yy), value, font=font(14), fill=colour)
    for i, label in enumerate(("Cold Immunity", "Rugged Backpack")):
        draw.text((PANEL_WIDTH / 2 + 30, stats_y + 34 + i * 22), label, font=font(14), fill=text_col)
    for i, key in enumerate("ZVB"):
        qx = (PANEL_WIDTH - (3 * 70 + 2 * 8)) // 2 + i * 78
        sheet.alpha_composite(nine_slice(tile, 70, 70, (TILE_BORDER,) * 4), (qx, quick_y))
        draw.text((qx + 11, quick_y + 8), key, font=font(13), fill=(230, 230, 230))
    sheet.alpha_composite(pieces["close"], (PANEL_WIDTH - 10 - CLOSE - 10, 8 + (TITLE_HEIGHT - CLOSE) // 2))

    bg = Image.new("RGBA", (PANEL_WIDTH + 80, height + 80), (70, 96, 70, 255))
    bg.alpha_composite(sheet, (40, 40))
    bg.convert("RGB").save(PREVIEW)


def main():
    os.makedirs(OUT, exist_ok=True)
    pieces = {
        "panel": make_panel(),
        "inset": make_inset(),
        "tile": make_tile(),
        "medallion": make_medallion(),
        "glow_ring": make_glow_ring(),
        "glow_line": make_glow_line(),
        "glow_box": make_glow_box(),
        "disc": make_disc(),
        "arch_back": make_arch_back(),
        "arch_rim": make_arch_rim(),
        "close": make_close(),
        "bar": make_bar(),
        "bar_cap": make_bar_cap(),
        "bar_fill": make_bar_fill(),
    }
    for name, image in pieces.items():
        save(image, name)
    for old in ("wood", "frame", "title", "parchment", "ribbon"):
        for ext in (".png", ".rgba"):
            path = os.path.join(OUT, old + ext)
            if os.path.exists(path):
                os.remove(path)
    preview(pieces)
    print("Wrote", ", ".join(sorted(pieces)), "and", os.path.relpath(PREVIEW, ROOT))


if __name__ == "__main__":
    main()
