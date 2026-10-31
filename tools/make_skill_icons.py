"""
Generates the placeholder skill icons of the Orlog skill trees (the Amazon's three tabs) and the Amazon class icon:
one bold emblem per skill, drawn to sit on the round bronze skill medallion
(CraftingOverhaul/Assets/Sheet/medallion.png) until real art is commissioned.

    python tools/make_skill_icons.py            (needs numpy and Pillow)

Writes, into CraftingOverhaul/Assets/Skills/:
    <Id>.png      for viewing and editing (96 x 96)
    <Id>.rgba     the same pixels raw, which the mod loads (8-byte header, rows bottom to top; see write_raw)
and docs/images/skill_icons_preview.png, a contact sheet of every icon on the medallion at 64 and 48 px (not shipped).

Style rules (keep new icons consistent with them):
  * One bold central motif per skill, strong silhouette, readable at 48 and 64 px; no text or letters.
  * Transparent background; everything stays inside a centred circle of 74 % of the image width (the medallion's dark
    face), so nothing is clipped by the bronze rim.
  * Light from the top left: fills run from a dark edge to a bright core, with a highlight on the top-left edges, a
    shade on the bottom-right edges, and a 1-2 px dark outline round every shape for contrast on the dark face.
    Blades and arrowheads are split along the midrib into a lit and a shaded facet.
  * Colour says the element: magic violet-blue, fire orange-red, cold icy cyan, lightning yellow-white with a blue
    glow, poison sickly green, physical steel silver with bronze, passive gold, summon soft white-gold.
    A faint aura of the element colour sits behind each motif.
  * Drawn at 4x (384 px) in a 100 x 100 unit design space (centre 50, 50; motifs reach about 36 units out and are
    shrunk by SCALE so they end inside the circle), then downscaled with LANCZOS for clean edges.
Everything uses fixed seeds, so reruns give identical files.
"""
import math
import os
import re

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "CraftingOverhaul", "Assets", "Skills")
MEDALLION = os.path.join(ROOT, "CraftingOverhaul", "Assets", "Sheet", "medallion.png")
PREVIEW = os.path.join(ROOT, "docs", "images", "skill_icons_preview.png")

SIZE = 96                 # icon texture size
S = SIZE * 4              # supersampled drawing size
K = S / 100.0             # pixels per design unit
CLIP_RADIUS = 37.0        # content circle: 74 % of the width (in image units, 100 = full width)
SCALE = 0.92              # design space to image: motifs are drawn up to ~37 units out and shrunk to sit inside
OUTLINE = 1.45            # outline width in design units (about 1.4 px at 96 px)
OUTLINE_COLOUR = (0.07, 0.045, 0.03)
LIGHT_DIR = np.array([-1.0, -1.0]) / math.sqrt(2)   # towards the light (top left)


# ---------------------------------------------------------------- colours

def hexc(h):
    h = h.lstrip("#")
    return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)], float) / 255.0


def pal(*hexes):
    return [hexc(h) for h in hexes]


# Ramps from the darkest edge to the brightest core.
STEEL = pal("#2c3139", "#6f7682", "#C8CDD6", "#F6F8FC")
BRONZE = pal("#3f2810", "#7c5626", "#C89A55", "#F2D9A4")
GOLD = pal("#4a3410", "#9c7a2a", "#E8C46A", "#FFF4C8")
MAGIC = pal("#2e2470", "#6f5cd8", "#B8A8FF", "#FFFFFF")
FIRE_OUT = pal("#6a1404", "#C8340A", "#FF7A1A", "#FFB648")
FIRE_CORE = pal("#FF8A24", "#FFD060", "#FFF2B0", "#FFFFFF")
ICE = pal("#174f72", "#4fb6e0", "#8FE3FF", "#E8FBFF")
SPARK = pal("#C8A830", "#FFF07A", "#FFFBD0", "#FFFFFF")
POISON = pal("#1f420c", "#4e9a22", "#8BE04A", "#D8FF8A")
SPIRIT = pal("#6e5a34", "#bfa874", "#F4E6C0", "#FFFFFF")
FEATHER = pal("#4a3a2a", "#9a8a74", "#E2D8C6", "#FFFFFF")
FIRE_FLETCH = pal("#4a1608", "#9a2c10", "#E06A2A", "#FFC890")
ICE_FLETCH = pal("#2c5a76", "#7ec0e0", "#d8f6ff", "#FFFFFF")
POISON_HEAD = pal("#1c3a10", "#4f8a30", "#a8d888", "#E8FFD8")

GLOW_MAGIC = hexc("#9C88FF")
GLOW_FIRE = hexc("#FF6A10")
GLOW_ICE = hexc("#7FD8FF")
GLOW_SPARK = hexc("#9FD0FF")
GLOW_POISON = hexc("#7CD03A")
GLOW_GOLD = hexc("#E8B850")
GLOW_SPIRIT = hexc("#FFF0C8")
GLOW_STEEL = hexc("#B8C4D8")
BOLT_EDGE = hexc("#2a4f9a")
POISON_EDGE = hexc("#0e2006")
FIRE_EDGE = hexc("#3a0802")


# ---------------------------------------------------------------- geometry

def V(x, y):
    return np.array([x, y], float)


def unit(v):
    v = np.asarray(v, float)
    return v / (np.linalg.norm(v) + 1e-12)


def perp(u):
    return np.array([-u[1], u[0]])


def rotate(v, deg):
    a = math.radians(deg)
    c, s = math.cos(a), math.sin(a)
    return np.array([c * v[0] - s * v[1], s * v[0] + c * v[1]])


def polar(centre, deg, r):
    """Point at angle deg (0 = right, 90 = down, image y runs down) and distance r from centre."""
    a = math.radians(deg)
    return V(centre[0] + r * math.cos(a), centre[1] + r * math.sin(a))


def lerp(a, b, t):
    return np.asarray(a, float) + (np.asarray(b, float) - np.asarray(a, float)) * t


def bezier(p0, p1, p2, p3, n=40):
    t = np.linspace(0, 1, n)[:, None]
    p0, p1, p2, p3 = (np.asarray(p, float) for p in (p0, p1, p2, p3))
    return (1 - t) ** 3 * p0 + 3 * (1 - t) ** 2 * t * p1 + 3 * (1 - t) * t ** 2 * p2 + t ** 3 * p3


# ---------------------------------------------------------------- masks (float arrays in [0, 1], S x S)

_Y, _X = np.mgrid[0:S, 0:S].astype(float) + 0.5
GX = (_X / K - 50) / SCALE + 50          # design-space coordinates of each pixel, for half-plane cuts
GY = (_Y / K - 50) / SCALE + 50
_FY = np.fft.fftfreq(S)[:, None]
_FX = np.fft.fftfreq(S)[None, :]
_DISCS = {}


def _blank():
    return Image.new("L", (S, S), 0)


def _arr(img):
    return np.asarray(img, float) / 255.0


def _pts(points):
    """Design units to supersampled pixels (shrunk by SCALE about the centre)."""
    return [(((float(p[0]) - 50) * SCALE + 50) * K, ((float(p[1]) - 50) * SCALE + 50) * K) for p in points]


def m_poly(points):
    img = _blank()
    ImageDraw.Draw(img).polygon(_pts(points), fill=255)
    return _arr(img)


def m_circle(c, r):
    img = _blank()
    (x, y), = _pts([c])
    r = r * SCALE * K
    ImageDraw.Draw(img).ellipse([x - r, y - r, x + r, y + r], fill=255)
    return _arr(img)


def m_ellipse(c, rx, ry, deg=0.0, n=72):
    pts = [V(c[0], c[1]) + rotate(V(rx * math.cos(a), ry * math.sin(a)), deg)
           for a in np.linspace(0, 2 * math.pi, n, endpoint=False)]
    return m_poly(pts)


def m_ring(c, r_out, r_in):
    return sub(m_circle(c, r_out), m_circle(c, r_in))


def m_line(points, width):
    """A polyline of even width with round joints and caps."""
    img = _blank()
    d = ImageDraw.Draw(img)
    pts = _pts(points)
    d.line(pts, fill=255, width=max(1, int(round(width * SCALE * K))), joint="curve")
    r = width * SCALE * K / 2
    for x, y in (pts[0], pts[-1]):
        d.ellipse([x - r, y - r, x + r, y + r], fill=255)
    return _arr(img)


def m_stroke(points, widths, round_start=False):
    """A tapered stroke: a polygon following the points with the given full width at each point."""
    pts = np.asarray(points, float)
    widths = np.broadcast_to(np.asarray(widths, float), (len(pts),))
    left, right = [], []
    for i, p in enumerate(pts):
        a = pts[max(i - 1, 0)]
        b = pts[min(i + 1, len(pts) - 1)]
        n = perp(unit(b - a))
        left.append(p + n * widths[i] / 2)
        right.append(p - n * widths[i] / 2)
    m = m_poly(left + right[::-1])
    if round_start and widths[0] > 0:
        m = union(m, m_circle(pts[0], widths[0] / 2))
    return m


def m_burst(c, r_out, r_in, n, deg=0.0, jitter=0.0, seed=0):
    """A star of n points; jitter varies each point's length."""
    rng = np.random.default_rng(seed)
    pts = []
    for i in range(2 * n):
        r = r_out if i % 2 == 0 else r_in
        if i % 2 == 0 and jitter:
            r *= 1 - jitter * rng.random()
        pts.append(polar(c, deg + i * 180.0 / n, r))
    return m_poly(pts)


def union(*ms):
    out = ms[0]
    for m in ms[1:]:
        out = np.maximum(out, m)
    return out


def sub(a, b):
    return a * (1 - b)


def blur(a, r):
    """Gaussian blur with sigma r design units (FFT; the icons keep a wide empty margin, so wrapping is harmless)."""
    sigma = r * K
    if sigma <= 0:
        return a
    g = np.exp(-2 * (math.pi * sigma) ** 2 * (_FX ** 2 + _FY ** 2))
    return np.real(np.fft.ifft2(np.fft.fft2(a) * g))


def dilate(a, r):
    """Grow a mask by r design units (disc kernel)."""
    rp = max(1, int(round(r * K)))
    if rp not in _DISCS:
        disc = np.zeros((S, S))
        yy, xx = np.mgrid[-rp:rp + 1, -rp:rp + 1]
        k = (xx ** 2 + yy ** 2 <= rp * rp + rp).astype(float)
        disc[np.ix_(np.arange(-rp, rp + 1) % S, np.arange(-rp, rp + 1) % S)] = k
        _DISCS[rp] = np.fft.fft2(disc)
    return np.clip(np.real(np.fft.ifft2(np.fft.fft2(a) * _DISCS[rp])) / 2.0, 0, 1)


# ---------------------------------------------------------------- painting

class Canvas:
    """Premultiplied RGBA float canvas at the supersampled size."""

    def __init__(self):
        self.rgb = np.zeros((S, S, 3))
        self.a = np.zeros((S, S))

    def over(self, colour, alpha):
        alpha = np.clip(alpha, 0, 1)
        colour = np.asarray(colour, float)
        if colour.ndim == 1:
            colour = colour[None, None, :]
        self.rgb = colour * alpha[..., None] + self.rgb * (1 - alpha[..., None])
        self.a = alpha + self.a * (1 - alpha)

    def image(self):
        """Downscaled to SIZE with LANCZOS (premultiplied, so edges don't fringe), clipped to the content circle."""
        r = np.hypot(_X / K - 50, _Y / K - 50)
        clip = np.clip((CLIP_RADIUS - r) / 1.2, 0, 1)
        chans = [self.rgb[..., i] * clip for i in range(3)] + [self.a * clip]
        small = [np.asarray(Image.fromarray(c.astype(np.float32), "F").resize((SIZE, SIZE), Image.LANCZOS))
                 for c in chans]
        a = np.clip(small[3], 0, 1)
        rgb = np.dstack(small[:3]) / np.maximum(a, 1e-6)[..., None]
        rgba = np.dstack([np.clip(rgb, 0, 1) * (a[..., None] > 0), a])
        return Image.fromarray(np.round(rgba * 255).astype(np.uint8), "RGBA")


def ramp(stops, t):
    t = np.clip(t, 0, 1) * (len(stops) - 1)
    i = np.minimum(t.astype(int), len(stops) - 2)
    f = (t - i)[..., None]
    arr = np.stack(stops)
    return arr[i] * (1 - f) + arr[i + 1] * f


def glow(cv, m, colour, radius=3.0, strength=1.0):
    cv.over(colour, np.clip(blur(m, radius) * strength, 0, 0.92))


def outline(cv, m, width=OUTLINE, colour=OUTLINE_COLOUR, alpha=0.9):
    ring = np.clip(dilate(m, width) - m, 0, 1)
    cv.over(colour, ring * alpha)


def fill(cv, m, stops, alpha=1.0, depth=1.6, bias=0.0, gloss=0.55, shade=0.45):
    """Shade a shape: dark edges to a bright core, lit from the top left, with an embossed edge highlight."""
    if m.max() < 0.01:
        return
    inner = np.clip((blur(m, depth) - 0.5) * 2.2, 0, 1)
    ys, xs = np.nonzero(m > 0.5)
    if len(xs) == 0:
        return
    cx, cy = xs.mean(), ys.mean()
    span = max(np.ptp(xs), np.ptp(ys), 8 * K)
    lit = np.clip(0.5 + ((cx - _X) + (cy - _Y)) / span * 0.8, 0, 1)
    t = np.clip(0.1 + 0.42 * lit + 0.5 * inner + bias, 0, 1)
    col = ramp(stops, t)
    e = blur(m, max(depth * 0.45, 0.35))
    k = max(1, int(round(0.55 * K)))
    shifted = np.roll(e, (-k, -k), (0, 1))
    hi = np.clip((shifted - e) * 3.5, 0, 1) * m
    lo = np.clip((e - shifted) * 3.5, 0, 1) * m
    col = col + (1 - col) * (gloss * hi)[..., None]
    col = col * (1 - shade * lo)[..., None]
    cv.over(col, m * alpha)


def flat(cv, m, colour, alpha=1.0):
    cv.over(colour, m * alpha)


def solid(cv, m, stops, alpha=1.0, depth=1.6, bias=0.0, line=OUTLINE, line_colour=OUTLINE_COLOUR, **kw):
    """Outline then fill: the usual way to put down one shape."""
    if line:
        outline(cv, m, line, line_colour, alpha=0.9 * min(1.0, alpha * 1.4))
    fill(cv, m, stops, alpha=alpha, depth=depth, bias=bias, **kw)


def aura(cv, colour, strength=0.2, radius=33.0, c=(50, 50)):
    """A faint disc of the element colour behind the motif."""
    r = np.hypot(_X / K - c[0], _Y / K - c[1])
    a = strength * np.clip(1 - r / radius, 0, 1) ** 1.6
    cv.over(colour, a)


def faceted(cv, left, right, axis_u, stops, alpha=1.0, depth=1.0, contrast=0.22, line=OUTLINE):
    """Two halves of a blade split along its midrib: the half facing the light is brighter."""
    n = perp(unit(axis_u))
    lit_left = float(np.dot(n, LIGHT_DIR)) > 0
    if line:
        outline(cv, union(left, right), line, alpha=0.9 * alpha)
    fill(cv, left, stops, alpha, depth, bias=contrast if lit_left else -contrast)
    fill(cv, right, stops, alpha, depth, bias=-contrast if lit_left else contrast)


# ---------------------------------------------------------------- motifs

def arrow(cv, tail, tip, shaft=3.4, head_len=15.0, head_w=13.0, fletch_len=13.0, fletch_w=6.0,
          head=STEEL, shaft_pal=BRONZE, fletch=FEATHER, alpha=1.0, glow_colour=None, glow_r=2.6, glow_s=1.0,
          fletching=True):
    """An arrow from tail to tip: barbed two-facet head, shaft, two swept vanes. Returns its mask."""
    T, H = V(*tail), V(*tip)
    u = unit(H - T)
    n = perp(u)
    hb = H - u * head_len           # barb line
    hn = H - u * head_len * 0.7     # where the head meets the shaft
    m_shaft = m_poly([T + n * shaft / 2, hn + u * 1 + n * shaft / 2, hn + u * 1 - n * shaft / 2, T - n * shaft / 2])
    head_l = m_poly([H, hb + n * head_w / 2, hn + n * shaft * 0.45, hn])
    head_r = m_poly([H, hn, hn - n * shaft * 0.45, hb - n * head_w / 2])
    vanes = np.zeros((S, S))
    if fletching:
        a = T + u * fletch_len * 0.05
        b = T + u * fletch_len
        for s in (1, -1):
            vanes = union(vanes, m_poly([b + n * s * shaft * 0.3, b - u * fletch_len * 0.45 + n * s * fletch_w,
                                         T - u * fletch_len * 0.12 + n * s * fletch_w, a + n * s * shaft * 0.3]))
    whole = union(m_shaft, head_l, head_r, vanes)
    if glow_colour is not None:
        glow(cv, whole, glow_colour, glow_r, glow_s)
    if fletching:
        solid(cv, vanes, fletch, alpha, depth=0.9)
    solid(cv, m_shaft, shaft_pal, alpha, depth=0.7, bias=0.1)
    faceted(cv, head_l, head_r, u, head, alpha)
    return whole


def spear(cv, butt, tip, head_len=20.0, head_w=10.0, shaft=3.0, head=STEEL, shaft_pal=BRONZE, alpha=1.0,
          glow_colour=None, glow_r=2.6, glow_s=1.0, socket=True):
    """A spear or javelin from butt to tip: leaf-shaped two-facet blade, bronze socket, shaft. Returns its mask."""
    B0, H = V(*butt), V(*tip)
    u = unit(H - B0)
    n = perp(u)
    base = H - u * head_len
    ts = np.linspace(0, 1, 40)
    peak = 0.3                                  # widest point, as a fraction of the blade from its base
    neck = shaft * 0.5
    rise = neck + (head_w / 2 - neck) * np.sin(np.pi / 2 * np.clip(ts / peak, 0, 1)) ** 0.7
    fall = head_w / 2 * np.clip((1 - ts) / (1 - peak), 0, 1) ** 0.85
    hw = np.where(ts < peak, rise, fall)
    edge_l = [base + u * head_len * t + n * w for t, w in zip(ts, hw)]
    edge_r = [base + u * head_len * t - n * w for t, w in zip(ts, hw)]
    blade_l = m_poly(edge_l + [base])
    blade_r = m_poly(edge_r + [base])
    sock_len = min(5.0, head_len * 0.3) if socket else 0.0
    sock0 = base - u * sock_len
    m_shaft = m_poly([B0 + n * shaft / 2, sock0 + n * shaft / 2, sock0 - n * shaft / 2, B0 - n * shaft / 2])
    m_sock = m_poly([sock0 + n * shaft * 0.62, base + u * 0.8 + n * shaft * 0.5, base + u * 0.8 - n * shaft * 0.5,
                     sock0 - n * shaft * 0.62]) if socket else np.zeros((S, S))
    whole = union(m_shaft, m_sock, blade_l, blade_r)
    if glow_colour is not None:
        glow(cv, whole, glow_colour, glow_r, glow_s)
    solid(cv, m_shaft, shaft_pal, alpha, depth=0.8, bias=0.05)
    if socket:
        solid(cv, m_sock, BRONZE, alpha, depth=0.8, bias=0.15)
    faceted(cv, blade_l, blade_r, u, head, alpha, depth=1.4)
    return whole


def flame_mask(base, direction, length, width, sway=0.22, phase=0.0, n=28):
    """One tongue of flame: a rounded base tapering to a wavering point."""
    u = unit(direction)
    nn = perp(u)
    B = V(*base)
    ts = np.linspace(0, 1, n)
    pts = [B + u * length * t + nn * width * sway * math.sin(math.pi * t * 1.7 + phase) * t for t in ts]
    widths = width * (1 - ts) ** 0.9
    return union(m_stroke(pts, widths), m_circle(B + u * width * 0.12, width * 0.5))


def flames(cv, tongues, core=0.6, glow_s=1.0, alpha=1.0):
    """A cluster of flame tongues (base, direction, length, width, phase): red-orange body, yellow-white core."""
    outer = np.zeros((S, S))
    inner = np.zeros((S, S))
    for base, direction, length, width, phase in tongues:
        outer = union(outer, flame_mask(base, direction, length, width, phase=phase))
        if core:
            u = unit(direction)
            inner = union(inner, flame_mask(V(*base) + u * width * 0.12, direction, length * core, width * 0.55,
                                            phase=phase))
    glow(cv, outer, GLOW_FIRE, 3.2, 1.1 * glow_s)
    outline(cv, outer, 1.0, FIRE_EDGE, 0.75 * alpha)
    fill(cv, outer, FIRE_OUT, alpha, depth=1.6, gloss=0.3, shade=0.2)
    if core:
        fill(cv, inner * outer, FIRE_CORE, alpha, depth=1.2, gloss=0.2, shade=0.1)
    return outer


def bolt_points(a, b, segments, amp, seed):
    """A jagged lightning path from a to b."""
    rng = np.random.default_rng(seed)
    A, Bp = V(*a), V(*b)
    n = perp(unit(Bp - A))
    pts = [A]
    sign = 1 if rng.random() < 0.5 else -1
    for i in range(1, segments):
        t = i / segments
        pts.append(lerp(A, Bp, t) + n * sign * amp * (0.45 + 0.55 * rng.random()))
        sign = -sign
    pts.append(Bp)
    return pts


def bolt(cv, pts, w0=2.6, w1=0.6, glow_s=1.0, alpha=1.0, glow_r=2.6):
    """A lightning stroke: blue halo, thin dark-blue edge, yellow-white body, white core."""
    widths = np.linspace(w0, w1, len(pts))
    m = m_stroke(pts, widths, round_start=True)
    glow(cv, m, GLOW_SPARK, glow_r, 1.3 * glow_s)
    outline(cv, m, 0.8, BOLT_EDGE, 0.8 * alpha)
    fill(cv, m, SPARK, alpha, depth=0.6, bias=0.2, gloss=0.2, shade=0.1)
    core = m_stroke(pts, widths * 0.4)
    flat(cv, core, (1, 1, 1), 0.85 * alpha)
    return m


def snowflake_mask(c, r, arm=2.4, deg=90.0, branches=True):
    """Six arms with two pairs of side branches each (or a plain six-point star) and a hexagonal heart."""
    C = V(*c)
    m = np.zeros((S, S))
    for i in range(6):
        a = deg + i * 60
        tip = polar(C, a, r)
        m = union(m, m_line([C, tip], arm))
        for f, L in ((0.45, 0.36), (0.72, 0.26)) if branches else ():
            root = polar(C, a, r * f)
            for s in (-1, 1):
                m = union(m, m_line([root, polar(root, a + s * 50, r * L)], arm * 0.8))
    hexagon = [polar(C, deg + 30 + i * 60, r * 0.22) for i in range(6)]
    return union(m, m_poly(hexagon))


def gem(cv, c, verts, stops, alpha=1.0, line=OUTLINE, line_colour=OUTLINE_COLOUR, edges=True):
    """A faceted crystal: triangles from a centre point, each shaded by which way it faces."""
    C = V(*c)
    verts = [V(*v) for v in verts]
    whole = m_poly(verts)
    if line:
        outline(cv, whole, line, line_colour, 0.9)
    for i in range(len(verts)):
        a, b = verts[i], verts[(i + 1) % len(verts)]
        tri = m_poly([C, a, b])
        facing = float(np.dot(unit((a + b) / 2 - C), LIGHT_DIR))
        col = ramp(stops, np.array(0.5 + 0.38 * facing))
        cv.over(col, tri * alpha)
    if edges:
        for v in verts:
            cv.over(stops[-1], m_line([C, v], 0.35) * 0.45 * alpha)
    return whole


def cloud_puffs(c, r, puffs, seed):
    """A billowing cloud as (centre, radius) discs: a core disc ringed by puffs of varying size."""
    rng = np.random.default_rng(seed)
    C = V(*c)
    discs = [(C, r * 0.62)]
    for i in range(puffs):
        a = i * 360.0 / puffs + rng.uniform(-12, 12)
        pr = r * rng.uniform(0.32, 0.45)
        discs.append((polar(C, a, r - pr), pr))
    return discs


def poison_cloud(cv, c, r, puffs, seed, alpha=1.0, glow_s=1.0):
    """A toxic cloud: soft body shading over the whole cloud, then a lit crown on each puff so it billows."""
    discs = cloud_puffs(c, r, puffs, seed)
    whole = union(*[m_circle(p, pr) for p, pr in discs])
    glow(cv, whole, GLOW_POISON, 2.8, 0.9 * glow_s)
    outline(cv, whole, 1.1, POISON_EDGE, 0.85 * alpha)
    fill(cv, whole, pal("#1a3a0a", "#3e7c1a", "#78c83a", "#B8F070"), alpha, depth=r * 0.35, gloss=0.5)
    for p, pr in sorted(discs[1:], key=lambda d: d[0][1]):
        crown = m_circle(p - V(pr, pr) * 0.22, pr * 0.62) * whole
        cv.over(hexc("#C8FF80"), blur(crown, pr * 0.22) * 0.38 * alpha)
        rim = sub(m_circle(p, pr), m_circle(p - V(pr, pr) * 0.12, pr)) * whole
        cv.over(hexc("#14300a"), blur(rim, 0.35) * 0.45 * alpha)
    return whole


def sparkle(cv, c, r, colour=(1, 1, 1), glow_colour=None, deg=0.0, alpha=1.0):
    m = m_burst(c, r, r * 0.2, 4, deg=deg)
    if glow_colour is not None:
        glow(cv, m, glow_colour, r * 0.35, 1.2 * alpha)
    flat(cv, m, colour, alpha)
    return m


def speed_lines(cv, pts_list, width, stops, alpha=0.8, glow_colour=None):
    """Tapered motion streaks; each entry is a list of points from the thin tail to the thick head."""
    m = np.zeros((S, S))
    for pts in pts_list:
        widths = np.linspace(0.05, width, len(pts))
        m = union(m, m_stroke(pts, widths))
    if glow_colour is not None:
        glow(cv, m, glow_colour, 1.6, 0.8)
    fill(cv, m, stops, alpha, depth=0.4, bias=0.25, gloss=0.2, shade=0.1)
    return m


def arc_points(c, r, a0, a1, n=40, ry=None):
    ry = r if ry is None else ry
    return [V(c[0] + r * math.cos(math.radians(a)), c[1] + ry * math.sin(math.radians(a)))
            for a in np.linspace(a0, a1, n)]


def crescent(c, r, a0, a1, width, taper=True):
    """A swept crescent along a circular arc, thick in the middle."""
    pts = arc_points(c, r, a0, a1)
    t = np.linspace(0, 1, len(pts))
    widths = width * (np.sin(np.pi * t) ** 0.7 if taper else np.ones_like(t))
    return m_stroke(pts, widths)


# ---------------------------------------------------------------- Bow and Crossbow

def icon_magic_arrow(cv):
    aura(cv, GLOW_MAGIC, 0.22)
    arrow(cv, (26, 74), (74, 26), shaft=3.6, head_len=17, head_w=15, fletch_len=14, fletch_w=7,
          head=MAGIC, shaft_pal=MAGIC, fletch=MAGIC, glow_colour=GLOW_MAGIC, glow_r=3.6, glow_s=1.6)
    flat(cv, blur(m_line([V(30, 70), V(63, 37)], 1.1), 0.3), (1, 1, 1), 0.85)
    for c, r, d in (((30, 36), 6, 0), ((68, 66), 5, 0), ((48, 24), 3.4, 45), ((77, 48), 3.2, 45)):
        sparkle(cv, c, r, (1, 1, 1), GLOW_MAGIC, d)


def icon_fire_arrow(cv):
    aura(cv, GLOW_FIRE, 0.22)
    T, H = V(25, 75), V(75, 25)
    back = unit(V(-1.0, 0.15))
    tongues = []
    for i, f in enumerate(np.linspace(0.78, 0.3, 5)):
        tongues.append((lerp(T, H, f), rotate(back, (-1) ** i * 16), 20 + 14 * (f - 0.3), 12 + 5 * f, i * 1.3))
    flames(cv, tongues)
    arrow(cv, T, H, fletch=FIRE_FLETCH)
    flames(cv, [(lerp(T, H, 0.66), rotate(back, -6), 14, 7.5, 0.4), (lerp(T, H, 0.46), rotate(back, 12), 13, 7, 2.0)],
           alpha=0.92, glow_s=0.4)


def icon_cold_arrow(cv):
    aura(cv, GLOW_ICE, 0.2)
    T, H = V(42, 58), V(75, 25)
    n = perp(unit(H - T))
    # frost streams back from the arrow, fanning out past its tail like a comet's tail
    end = T + (T - H) * 0.42
    trail = np.zeros((S, S))
    for side in (-1, 0, 1):
        pts = [lerp(lerp(H, T, 0.35), end, f) + n * side * 10 * f ** 1.3 for f in np.linspace(0, 1, 24)]
        trail = union(trail, m_stroke(pts[::-1], np.linspace(8 if side == 0 else 6, 0.6, 24)))
    glow(cv, trail, GLOW_ICE, 2.6, 1.2)
    fill(cv, blur(trail, 0.5), ICE, 0.85, depth=1.5, bias=0.05, gloss=0.0, shade=0.0)
    arrow(cv, T, H, head=ICE, shaft_pal=pal("#2c4a5e", "#5f8aa4", "#a8d4ea", "#E8FBFF"), fletch=ICE_FLETCH,
          glow_colour=GLOW_ICE, glow_s=0.9)
    for f, side, r in ((0.45, 1, 4.6), (0.75, -1, 5.0), (1.0, 0.4, 4.2)):
        p = lerp(T, end, f) + n * side * 11
        flake = snowflake_mask(p, r, arm=1.4, branches=False)
        glow(cv, flake, GLOW_ICE, 1.2, 1.0)
        solid(cv, flake, ICE, depth=0.3, bias=0.35, line=0.8, line_colour=hexc("#0a2a40"))
    sparkle(cv, (63, 22), 4.5, (1, 1, 1), GLOW_ICE)
    sparkle(cv, (79, 42), 3.2, (1, 1, 1), GLOW_ICE, 45)


def icon_multiple_shot(cv):
    aura(cv, GLOW_STEEL, 0.14)
    origin = V(50, 84)
    for deg, start, length in ((-125, 10, 54), (-55, 10, 54), (-90, 4, 62)):
        d = polar((0, 0), deg, 1)
        arrow(cv, origin + d * start, origin + d * length, shaft=3.2, head_len=14, head_w=12.5, fletch_len=11,
              fletch_w=5.2)


def icon_exploding_arrow(cv):
    aura(cv, GLOW_FIRE, 0.22)
    arrow(cv, (26, 74), (53, 47), fletch=FIRE_FLETCH)
    c = V(59, 41)
    outer = m_burst(c, 23, 11, 11, deg=-8, jitter=0.25, seed=5)
    mid = m_burst(c, 16, 8.5, 9, deg=12, jitter=0.25, seed=6)
    inner = m_burst(c, 9.5, 5, 7, deg=30, jitter=0.2, seed=7)
    glow(cv, outer, GLOW_FIRE, 3.5, 1.3)
    outline(cv, outer, 1.1, FIRE_EDGE, 0.8)
    fill(cv, outer, FIRE_OUT, depth=1.8, gloss=0.2, shade=0.15)
    fill(cv, mid, pal("#E0500E", "#FF9A2A", "#FFD060", "#FFF0B0"), depth=1.6, gloss=0.2, shade=0.1)
    fill(cv, inner, FIRE_CORE, depth=1.4, bias=0.25, gloss=0.1, shade=0.0)
    rng = np.random.default_rng(8)
    for i in range(9):
        a = -200 + i * 30 + rng.uniform(-8, 8)
        p1 = polar(c, a, 28)
        if np.hypot(*(p1 - V(50, 50))) > 34:
            p1 = polar(c, a, 24.5)
        m = m_stroke([polar(c, a, 21), p1], [1.8, 0.2])
        flat(cv, blur(m, 0.3), hexc("#FFD060"), 0.95)


def icon_ice_arrow(cv):
    aura(cv, GLOW_ICE, 0.2)
    arrow(cv, (25, 75), (68, 32), fletch=ICE_FLETCH)
    c = V(59, 41)
    u = unit(V(1, -1))
    n = perp(u)

    def prism(centre, axis, length, width):
        a = unit(axis)
        b = perp(a)
        return [centre + a * length, centre + a * length * 0.55 + b * width, centre - a * length * 0.5 + b * width,
                centre - a * length * 0.8, centre - a * length * 0.5 - b * width,
                centre + a * length * 0.55 - b * width]

    verts = prism(c, u, 23, 11)
    glow(cv, m_poly(verts), GLOW_ICE, 3.5, 1.3)
    navy = hexc("#0a2a40")
    for axis, off, ln, w in ((rotate(u, -62), -4, 13, 4.5), (rotate(u, 58), -6, 12, 4.2), (rotate(u, 150), 4, 9, 3.6)):
        base = c + u * off
        gem(cv, base + unit(axis) * ln * 0.5, prism(base + unit(axis) * ln * 0.5, axis, ln, w), ICE, alpha=0.9,
            line=1.0, line_colour=navy)
    gem(cv, c, verts, ICE, alpha=0.7, line=1.2, line_colour=navy)
    tip = V(68, 32)
    head = union(m_poly([tip, tip - u * 15 + n * 6.5, tip - u * 10.5]),
                 m_poly([tip, tip - u * 10.5, tip - u * 15 - n * 6.5]))
    fill(cv, head, STEEL, 0.5, depth=0.8)
    sparkle(cv, c + u * 15 + n * 3, 4.5, (1, 1, 1), GLOW_ICE, 20)


def icon_guided_arrow(cv):
    aura(cv, GLOW_MAGIC, 0.2)
    tc = V(63, 32)
    rings = union(m_ring(tc, 13, 9.8), m_ring(tc, 6.8, 4.0), m_circle(tc, 1.8))
    glow(cv, rings, GLOW_MAGIC, 2.5, 1.3)
    solid(cv, rings, MAGIC, depth=0.5, bias=-0.1)
    path = bezier((20, 44), (20, 88), (60, 92), (62, 50), 60)
    for t in np.linspace(0.02, 0.56, 8):
        p = path[int(t * (len(path) - 1))]
        m = m_circle(p, 1.0 + 1.8 * t)
        glow(cv, m, GLOW_MAGIC, 1.2, 0.9)
        fill(cv, m, MAGIC, 0.6 + 0.4 * t, depth=0.4, bias=0.3)
    arrow(cv, path[int(0.62 * 59)], (62.5, 46), shaft=3.2, head_len=14, head_w=12.5, fletch_len=10, fletch_w=5,
          head=MAGIC, shaft_pal=MAGIC, fletch=MAGIC, glow_colour=GLOW_MAGIC, glow_s=1.3)


def icon_strafe(cv):
    aura(cv, GLOW_STEEL, 0.14)
    d = polar((0, 0), 70, 1)
    for x, y in ((39, 37), (61, 33), (29, 59), (51, 56), (72, 53)):
        c = V(x, y)
        arrow(cv, c - d * 17, c + d * 17, shaft=2.8, head_len=11.5, head_w=11, fletch_len=8, fletch_w=4.4)


def icon_immolation_arrow(cv):
    aura(cv, GLOW_FIRE, 0.22)
    ground = m_ellipse((50, 73), 28, 7.5)
    glow(cv, ground, GLOW_FIRE, 3, 1.0)
    solid(cv, ground, pal("#2a0802", "#7a1a06", "#D0400E", "#FFA040"), depth=2.5)
    # tongues: (base, direction, length, width, phase)
    back = [((32, 72), (-0.15, -1), 15, 10, 0.0), ((50, 72), (0.05, -1), 30, 13, 1.1),
            ((68, 72), (0.15, -1), 16, 10, 2.2),
            ((41, 72), (-0.1, -1), 22, 11, 3.0), ((59, 72), (0.1, -1), 23, 11, 4.0)]
    flames(cv, back)
    arrow(cv, (58, 18), (48, 60), fletch=FIRE_FLETCH)
    flames(cv, [((40, 75), (-0.05, -1), 11, 9, 0.5), ((54, 75), (0.05, -1), 13, 9.5, 1.7),
                ((66, 75), (0.1, -1), 9, 8, 2.6)],
           glow_s=0.4)


def icon_freezing_arrow(cv):
    aura(cv, GLOW_ICE, 0.22)
    flake = snowflake_mask((50, 50), 31, arm=3.4)
    glow(cv, flake, GLOW_ICE, 3.0, 1.3)
    solid(cv, flake, ICE, depth=0.9, bias=0.15)
    arrow(cv, (26, 74), (74, 26), fletch=ICE_FLETCH)
    sparkle(cv, (50, 50), 5.5, (1, 1, 1), GLOW_ICE)


# ---------------------------------------------------------------- Passive and Magic

def icon_inner_sight(cv):
    aura(cv, GLOW_MAGIC, 0.22)
    c = V(50, 50)
    rays = np.zeros((S, S))
    for i in range(12):
        a = i * 30 + 15
        rays = union(rays, m_stroke([polar(c, a, 18), polar(c, a, 33 if i % 3 == 1 else 28)], [3.4, 0.2]))
    glow(cv, rays, GLOW_MAGIC, 2.0, 1.0)
    fill(cv, rays, MAGIC, 0.9, depth=0.4, bias=0.2)
    eye = m_circle(V(50, 34), 26) * m_circle(V(50, 66), 26)
    glow(cv, eye, GLOW_MAGIC, 3, 1.2)
    solid(cv, eye, pal("#120c30", "#2a2068", "#4a3aa8", "#7a68e0"), depth=3, line=1.6)
    lid = sub(eye, m_circle(V(50, 36), 23.6) * m_circle(V(50, 64), 23.6))
    fill(cv, lid, MAGIC, depth=0.5, bias=0.25)
    iris = m_circle(c, 10.5)
    glow(cv, iris, GLOW_MAGIC, 2, 1.3)
    fill(cv, iris * eye, pal("#4a38c0", "#8a78ff", "#d8d0ff", "#FFFFFF"), depth=3.5, bias=0.1, gloss=0.3)
    flat(cv, blur(m_ellipse(c, 2.6, 7.5), 0.2), hexc("#140a38"), 0.95)
    flat(cv, m_circle(V(46, 46), 2.2), (1, 1, 1), 0.95)


def icon_critical_strike(cv):
    aura(cv, GLOW_STEEL, 0.14)
    c = V(50, 50)
    solid(cv, m_ring(c, 30, 23.5), STEEL, depth=1.2, bias=-0.15)
    solid(cv, m_ring(c, 20, 14.5), BRONZE, depth=1.0)
    solid(cv, m_circle(c, 11), pal("#4a0c08", "#8a1a10", "#D8402A", "#FF9A80"), depth=2.2)
    rng = np.random.default_rng(12)
    cracks = np.zeros((S, S))
    for i in range(6):
        a = i * 60 + rng.uniform(-14, 14)
        pts = [polar(c, a, 8)]
        for r in (15, 21, 27, 31):
            pts.append(polar(c, a + rng.uniform(-9, 9), r))
        cracks = union(cracks, m_stroke(pts, np.linspace(2.2, 0.4, len(pts))))
    flat(cv, cracks, hexc("#120a06"), 0.95)
    star = m_burst(c, 17, 4.0, 8, deg=-90, jitter=0.35, seed=4)
    glow(cv, star, hexc("#FFE6A0"), 2.4, 1.4)
    outline(cv, star, 1.0, alpha=0.7)
    fill(cv, star, pal("#E8C46A", "#FFF0B0", "#FFFFFF", "#FFFFFF"), depth=0.8, bias=0.2, gloss=0.1)


def heater_shield(c, w, h):
    """Outline of a heater shield centred on c: flat top, straight upper sides, curving to a point."""
    C = V(*c)
    top, shoulder, point = -h * 0.42, -h * 0.02, h * 0.58
    right = [V(w / 2, top)] + list(bezier((w / 2, shoulder), (w / 2, h * 0.32), (w * 0.2, h * 0.48), (0, point), 20))
    left = [V(-p[0], p[1]) for p in right[::-1]]
    arch = [V(x, top - h * 0.04 * (1 - (2 * x / w) ** 2)) for x in np.linspace(-w / 2, w / 2, 12)]
    return [C + p for p in arch + right[1:] + left[1:-1]]


def icon_dodge(cv):
    aura(cv, GLOW_GOLD, 0.2)
    for x, a in ((30, 0.16), (42, 0.32)):
        echo = sub(m_poly(heater_shield((x, 50), 32, 46)), m_poly(heater_shield((x, 50.5), 23, 35)))
        fill(cv, echo, GOLD, a, depth=0.6)
    shield = m_poly(heater_shield((58, 50), 34, 48))
    inner = m_poly(heater_shield((58, 50.6), 24, 36))
    glow(cv, shield, GLOW_GOLD, 2.5, 1.0)
    solid(cv, shield, GOLD, depth=1.0)
    fill(cv, inner, pal("#140e06", "#2e220e", "#4e3a16", "#6e5420"), depth=3, gloss=0.2)
    chevron = m_stroke([V(48, 44), V(58, 54), V(68, 44)], 4.2)
    solid(cv, chevron, GOLD, depth=0.6, line=1.0)
    swoosh = union(crescent((70, 50), 44, 152, 208, 4.4), crescent((70, 50), 50, 160, 200, 3.2))
    glow(cv, swoosh, GLOW_GOLD, 1.6, 1.0)
    fill(cv, swoosh, pal("#9c7a2a", "#E8C46A", "#FFF4C8", "#FFFFFF"), depth=0.4, bias=0.2)


def icon_slow_missiles(cv):
    aura(cv, GLOW_MAGIC, 0.22)
    c = V(50, 50)
    prof = np.sin(np.linspace(0, np.pi, 50)) * 3.4
    swirl = union(m_stroke(arc_points(c, 31, 200, 340, 50, ry=11), prof),
                  m_stroke(arc_points(c, 31, 20, 160, 50, ry=11), prof))
    glow(cv, swirl, GLOW_MAGIC, 2, 1.0)
    fill(cv, swirl * (GY < 50), MAGIC, 0.75, depth=0.4, bias=0.15)
    glass = m_poly([V(38, 25), V(62, 25), V(62, 29), V(52.5, 47), V(52.5, 53), V(62, 71), V(62, 75), V(38, 75),
                    V(38, 71), V(47.5, 53), V(47.5, 47), V(38, 29)])
    glow(cv, glass, GLOW_MAGIC, 3, 1.1)
    solid(cv, glass, pal("#140f38", "#241c5c", "#3a2e80", "#5a4aa8"), alpha=0.92, depth=2, gloss=0.35)
    # the trapped arrow, frozen mid-flight inside the glass
    arrow(cv, (50, 29), (50, 72), shaft=3.0, head_len=15, head_w=13.5, fletch_len=11, fletch_w=5.2,
          head=MAGIC, shaft_pal=MAGIC, fletch=MAGIC, glow_colour=GLOW_MAGIC, glow_s=1.6, glow_r=2.2)
    plates = union(m_poly([V(32, 19), V(68, 19), V(68, 25), V(32, 25)]), m_poly([V(32, 75), V(68, 75), V(68, 81),
                                                                                    V(32, 81)]))
    posts = union(m_line([V(35.5, 24), V(35.5, 76)], 2.6), m_line([V(64.5, 24), V(64.5, 76)], 2.6))
    solid(cv, posts, GOLD, depth=0.6)
    solid(cv, plates, GOLD, depth=1.0)
    fill(cv, swirl * (GY >= 50), MAGIC, 0.95, depth=0.4, bias=0.2)
    sparkle(cv, (72, 60), 4, (1, 1, 1), GLOW_MAGIC)
    sparkle(cv, (28, 40), 3, (1, 1, 1), GLOW_MAGIC, 45)


def icon_avoid(cv):
    aura(cv, GLOW_GOLD, 0.2)
    ward = crescent((4, 52), 37, -48, 48, 8.5)
    glow(cv, ward, GLOW_GOLD, 3, 1.3)
    solid(cv, ward, GOLD, depth=1.4)
    flat(cv, blur(crescent((4, 52), 37, -40, 40, 2.4), 0.4), hexc("#FFF4C8"), 0.8)
    hit = V(42, 52)
    trail = np.zeros((S, S))
    for f in np.linspace(0.12, 0.84, 5):
        trail = union(trail, m_line([lerp(hit, V(80, 70), f), lerp(hit, V(80, 70), f + 0.08)], 2.0))
    fill(cv, trail, STEEL, 0.6, depth=0.3, bias=0.2)
    arrow(cv, (45, 47), (71, 24), head_len=13, head_w=12, fletch_len=9, fletch_w=5)
    star = m_burst(hit, 10, 2.6, 6, deg=10, jitter=0.3, seed=3)
    glow(cv, star, GLOW_GOLD, 2, 1.4)
    fill(cv, star, pal("#E8C46A", "#FFF0B0", "#FFFFFF", "#FFFFFF"), depth=0.6, bias=0.2, gloss=0.1)


def icon_penetrate(cv):
    aura(cv, GLOW_STEEL, 0.14)
    T, H = V(21, 64.5), V(79, 37.5)
    u = unit(H - T)
    c = V(47, 52.5)
    arrow(cv, T, c + u * 2, head_len=0.1, head_w=0.1)
    plate = m_ellipse(c, 10, 28, deg=12)
    rim = sub(plate, m_ellipse(c, 7, 24.5, deg=12))
    glow(cv, plate, GLOW_STEEL, 2, 0.6)
    solid(cv, plate, BRONZE, depth=2.5)
    fill(cv, rim, STEEL, depth=0.6)
    for r in (-17, 17):
        solid(cv, m_circle(c + rotate(V(0, r), 12), 1.7), STEEL, depth=0.5, line=0.6)
    rng = np.random.default_rng(14)
    cracks = np.zeros((S, S))
    for i in range(6):
        a = math.radians(i * 60 + rng.uniform(-15, 15))
        cracks = union(cracks, m_stroke([c, c + rotate(V(math.cos(a) * 6.5, math.sin(a) * 14), 12)], [1.8, 0.2]))
    flat(cv, cracks * plate, hexc("#120a06"), 0.9)
    flat(cv, m_ellipse(c, 2.4, 3.8, deg=12), hexc("#0a0604"), 0.95)
    arrow(cv, c, H, fletching=False)


def bust_mask(c, scale=1.0):
    """Head and shoulders of a figure."""
    C = V(*c)
    head = m_circle(C + V(0, -10) * scale, 7.5 * scale)
    body = m_ellipse(C + V(0, 13) * scale, 15.5 * scale, 13.5 * scale) * (GY < C[1] + 17 * scale)
    return union(head, body)


def icon_decoy(cv):
    aura(cv, GLOW_SPIRIT, 0.2)
    ghost = bust_mask((61, 44), 1.05)
    fade = np.clip((63 - GY) / 12, 0.1, 1)          # the double thins out towards its base
    glow(cv, ghost * fade, GLOW_SPIRIT, 3, 1.1)
    fill(cv, ghost * fade, SPIRIT, 0.42, depth=3, bias=0.1)
    flat(cv, np.clip(dilate(ghost, 0.9) - ghost, 0, 1) * fade, hexc("#FFF4D6"), 0.9)
    solid(cv, bust_mask((41, 53), 1.05), pal("#3a3020", "#8a7650", "#D8C08A", "#FFF0C8"), depth=3, line=1.8)
    for c, r, d in (((76, 30), 3.6, 0), ((78, 58), 2.6, 45), ((68, 22), 2.2, 45)):
        sparkle(cv, c, r, (1, 1, 1), GLOW_SPIRIT, d)


def feather_masks(root, angles, lengths, widths, bend):
    """Feathers fanning from root: each a tapered stroke bowed sideways by bend (sign picks the side)."""
    out = []
    for deg, ln, w in zip(angles, lengths, widths):
        d = polar((0, 0), deg, 1)
        pts = [V(*root) + d * ln * t + perp(d) * bend * math.sin(math.pi * t) for t in np.linspace(0, 1, 24)]
        out.append(m_stroke(pts, w * np.sin(np.pi * np.linspace(0.12, 1, 24)) ** 0.55))
    return out


def icon_evade(cv):
    aura(cv, GLOW_GOLD, 0.2)
    speed_lines(cv, [[V(17, 58), V(36, 58)], [V(20, 67), V(37, 67)]], 3.0,
                pal("#9c7a2a", "#E8C46A", "#FFF4C8", "#FFFFFF"), alpha=0.85, glow_colour=GLOW_GOLD)
    boot = m_poly([V(46, 24), V(60, 24), V(59, 50), V(62, 56), V(71, 61), V(75, 66), V(75, 72),
                   V(42, 72), V(41, 60), V(45, 50)])
    cuff = m_poly([V(43, 20), V(63, 20), V(63, 28), V(43, 28)])
    sole = m_poly([V(40, 72), V(77, 72), V(76, 77), V(41, 77)])
    glow(cv, boot, GLOW_GOLD, 2.5, 0.8)
    solid(cv, boot, BRONZE, depth=2.5)
    solid(cv, sole, pal("#2a1a0a", "#4e3418", "#8a6430", "#C89A55"), depth=0.8)
    solid(cv, cuff, GOLD, depth=1.2)
    # the wing: primaries rooted along the back of the ankle, sweeping back and up, longest on top
    feathers = []
    for root, deg, ln in (((46, 48), -180, 17), ((46, 44), -168, 22), ((46, 40), -155, 22), ((46, 36), -140, 20)):
        feathers += feather_masks(root, (deg,), (ln,), (6.4,), 2.6)
    covert = m_ellipse((48, 41), 6, 8.5, deg=-20)
    glow(cv, union(covert, *feathers), GLOW_SPIRIT, 2.5, 1.2)
    for f in feathers:
        solid(cv, f, SPIRIT, depth=1.0, line=1.0)
    solid(cv, covert, GOLD, depth=1.4, line=1.0)


def icon_valkyrie(cv):
    aura(cv, GLOW_SPIRIT, 0.22)
    left = feather_masks((40, 50), (-112, -130, -150, -170), (19, 24, 24, 20), (7, 7.5, 7.5, 7), 2.5)
    right = feather_masks((60, 50), (-68, -50, -30, -10), (19, 24, 24, 20), (7, 7.5, 7.5, 7), -2.5)
    glow(cv, union(*left, *right), GLOW_SPIRIT, 3, 1.3)
    for f in left + right:
        solid(cv, f, SPIRIT, depth=1.0, line=1.0)
    c = V(50, 56)
    dome = m_circle(c, 13) * (GY <= c[1] + 0.5)
    cheeks = m_poly([V(37, 56), V(63, 56), V(62, 70), V(56, 74), V(54, 64), V(46, 64), V(44, 74), V(38, 70)])
    solid(cv, union(dome, cheeks), STEEL, depth=2.6)
    brow = m_poly([V(36, 53.5), V(64, 53.5), V(64, 58), V(36, 58)]) * m_circle(c, 14.5)
    crest = m_poly([V(48.6, 43), V(51.4, 43), V(51.4, 56), V(48.6, 56)])
    nose = m_poly([V(48.4, 57), V(51.6, 57), V(51, 69), V(50, 71), V(49, 69)])
    solid(cv, union(brow, crest), GOLD, depth=0.7, line=1.0)
    solid(cv, nose, GOLD, depth=0.6, line=1.0)
    eyes = union(m_ellipse(V(44.6, 61), 3.0, 2.1, deg=10), m_ellipse(V(55.4, 61), 3.0, 2.1, deg=-10))
    flat(cv, eyes, hexc("#0a0806"), 0.9)


def icon_pierce(cv):
    aura(cv, GLOW_STEEL, 0.14)
    centres = [V(33, 50), V(50, 50), V(67, 50)]
    bands = [sub(m_ellipse(c, 6.2, 16), m_ellipse(c, 3.0, 12)) for c in centres]
    for c, b in zip(centres, bands):
        solid(cv, b * (GX <= c[0] + 0.2), BRONZE, depth=0.8, bias=-0.1)
    arrow(cv, (17, 50), (83, 50), shaft=3.2, head_len=14, head_w=12.5, fletch_len=11, fletch_w=5.5,
          glow_colour=GLOW_STEEL, glow_s=0.5)
    for c, b in zip(centres, bands):
        solid(cv, b * (GX > c[0] + 0.2), BRONZE, depth=0.8, bias=0.1)


# ---------------------------------------------------------------- Javelin and Spear

def icon_jab(cv):
    aura(cv, GLOW_STEEL, 0.14)
    u = unit(V(1, -1))
    n = perp(u)
    tip = V(74, 26)
    for side in (1, -1):
        t = tip - u * 9 + n * side * 12
        spear(cv, t - u * 22, t, head_len=19, head_w=11, shaft=3.0, alpha=0.45, socket=False)
    streaks = [[V(30, 70) + n * s * 10 - u * 2, V(30, 70) + n * s * 10 + u * ln] for s, ln in ((-1, 22), (1, 22))]
    speed_lines(cv, streaks, 2.6, STEEL, alpha=0.8, glow_colour=GLOW_STEEL)
    spear(cv, V(26, 74), tip, head_len=23, head_w=13.5, shaft=3.6, glow_colour=GLOW_STEEL, glow_s=0.6)


def icon_power_strike(cv):
    aura(cv, GLOW_SPARK, 0.22)
    spear(cv, (50, 85), (50, 17), head_len=36, head_w=19, shaft=4.4, glow_colour=GLOW_SPARK, glow_s=1.2)
    # two long bolts climb the blade's edges, two short ones spit sideways
    for a, b, seed in (((37, 62), (44, 18), 3), ((63, 62), (56, 18), 4),
                       ((33, 40), (24, 28), 6), ((67, 40), (76, 28), 7)):
        bolt(cv, bolt_points(a, b, 6 if b[1] < 20 else 3, 3.2, seed), 2.6, 0.7)
    sparkle(cv, (50, 17), 5.5, (1, 1, 1), GLOW_SPARK)


def icon_poison_javelin(cv):
    aura(cv, GLOW_POISON, 0.2)
    T, H = V(25, 75), V(74, 26)
    n = perp(unit(H - T))
    for f, r, side, seed in ((0.1, 8.5, -1, 1), (0.3, 11, 1, 2), (0.52, 8.0, -1, 3)):
        poison_cloud(cv, lerp(T, H, f) + n * side * 2.5, r, 6, seed)
    spear(cv, T, H, head_len=19, head_w=10.5, shaft=3.0, head=POISON_HEAD, glow_colour=GLOW_POISON, glow_s=0.8)
    drops = union(m_circle(V(70, 43), 2.0), m_poly([V(68.2, 42.4), V(71.8, 42.4), V(70, 37.5)]),
                  m_circle(V(62, 48), 1.6), m_poly([V(60.6, 47.6), V(63.4, 47.6), V(62, 44)]))
    solid(cv, drops, POISON, depth=0.5, line=0.8, line_colour=POISON_EDGE)


def icon_impale(cv):
    aura(cv, GLOW_STEEL, 0.14)
    c = V(50, 74)
    rays = np.zeros((S, S))
    for a in (-170, -150, -125, -55, -30, -10):
        rays = union(rays, m_stroke([polar(c, a, 5), polar(c, a, 22 if a in (-125, -55) else 17)], [3.2, 0.2]))
    glow(cv, rays, GLOW_GOLD, 1.8, 0.9)
    fill(cv, rays, GOLD, depth=0.4, bias=0.2)
    ground = m_ellipse(c + V(0, 3), 25, 5)
    solid(cv, ground, pal("#1a1008", "#3a2810", "#6a4c24", "#8a6838"), depth=1.6)
    cracks = np.zeros((S, S))
    for a, ln in ((-8, 22), (190, 22), (25, 12), (155, 12)):
        cracks = union(cracks, m_stroke([c + V(0, 2), polar(c + V(0, 2), a, ln)], [1.8, 0.2]))
    flat(cv, cracks * ground, hexc("#0a0604"), 0.9)
    speed_lines(cv, [[V(x, 17), V(x, 40)] for x in (33, 67)], 3.2, STEEL, alpha=0.8, glow_colour=GLOW_STEEL)
    spear(cv, (50, 14), (50, 77), head_len=33, head_w=22, shaft=5.6)
    flat(cv, blur(m_burst(V(50, 76), 6, 2, 6, deg=-90), 0.4), hexc("#FFF4C8"), 0.85)


def icon_lightning_bolt(cv):
    aura(cv, GLOW_SPARK, 0.22)
    body = m_poly([V(23, 77), V(36, 50), V(30, 50), V(50, 30), V(44, 30), V(62, 18),
                   V(54, 34), V(60, 34), V(42, 54), V(48, 54)])
    u = unit(V(1, -1))
    n = perp(u)
    tip, base = V(75, 25), V(56, 44)
    head = m_poly([tip, base + u * 6 + n * 7, base + n * 2, base - n * 2, base + u * 6 - n * 7])
    whole = union(body, head)
    glow(cv, whole, GLOW_SPARK, 3.5, 1.5)
    outline(cv, whole, 1.1, BOLT_EDGE, 0.9)
    fill(cv, whole, SPARK, depth=1.4, bias=0.12, gloss=0.3, shade=0.2)
    core = union(m_line([V(25, 74), V(37, 53), V(31, 52.5), V(48, 34), V(53, 31)], 1.0),
                 m_line([V(59, 41), V(71, 29)], 1.0))
    flat(cv, blur(core, 0.3), (1, 1, 1), 0.9)
    sparkle(cv, tip, 5, (1, 1, 1), GLOW_SPARK)


def icon_charged_strike(cv):
    aura(cv, GLOW_SPARK, 0.22)
    tip = V(50, 46)
    for i, (a, ln) in enumerate(((-165, 25), (-128, 30), (-90, 30), (-52, 30), (-15, 25))):
        bolt(cv, bolt_points(tip, polar(tip, a, ln), 4, 2.8, 20 + i), 3.0, 0.7)
    spear(cv, (50, 86), tip, head_len=25, head_w=15, shaft=4.4, glow_colour=GLOW_SPARK, glow_s=0.7)
    orb = m_circle(tip, 5)
    glow(cv, orb, GLOW_SPARK, 3, 1.8)
    fill(cv, orb, SPARK, depth=1.5, bias=0.3)
    sparkle(cv, tip, 8, (1, 1, 1), None)


def icon_plague_javelin(cv):
    aura(cv, GLOW_POISON, 0.24)
    c = V(48, 57)
    splash = m_burst(c, 31, 20, 13, deg=-90, jitter=0.3, seed=11)
    glow(cv, splash, GLOW_POISON, 3.5, 1.1)
    outline(cv, splash, 1.0, POISON_EDGE, 0.8)
    fill(cv, splash, pal("#12280a", "#2a5a12", "#4e9222", "#8BE04A"), 0.85, depth=1.5)
    poison_cloud(cv, c, 23, 9, 21)
    rng = np.random.default_rng(22)
    for _ in range(6):
        p = polar(c, rng.uniform(-170, -10), rng.uniform(6, 17))
        r = rng.uniform(1.5, 2.8)
        flat(cv, m_ring(p, r, r * 0.6), hexc("#F0FFC0"), 0.9)
    spear(cv, (73, 25), (50, 50), head_len=19, head_w=11, shaft=3.4, head=POISON_HEAD, glow_colour=GLOW_POISON,
          glow_s=0.6)
    poison_cloud(cv, c + V(-1, 9), 11, 6, 23, glow_s=0.3)


def icon_fend(cv):
    aura(cv, GLOW_STEEL, 0.14)
    c = V(50, 66)
    sweep = crescent(c, 22, -170, -10, 5)
    glow(cv, sweep, GLOW_STEEL, 2, 0.8)
    fill(cv, sweep, STEEL, 0.5, depth=0.5, bias=0.2)
    for a in (-162, -18, -126, -54, -90):
        spear(cv, polar(c, a, 4), polar(c, a, 35 if a == -90 else 32), head_len=16, head_w=10, shaft=2.8, socket=False)
    solid(cv, m_circle(c, 5.5), BRONZE, depth=1.5)
    solid(cv, m_circle(c, 2.2), GOLD, depth=0.6, line=0.8)


def icon_lightning_strike(cv):
    aura(cv, GLOW_SPARK, 0.22)
    nodes = [V(22, 62), V(40, 30), V(60, 64), V(78, 34)]
    for i in range(3):
        bolt(cv, bolt_points(nodes[i], nodes[i + 1], 6, 3.4, 40 + i), 3.4, 2.0)
    for i, p in enumerate(nodes):
        orb = m_circle(p, 5.5 if i else 4.5)
        glow(cv, orb, GLOW_SPARK, 3, 1.6)
        outline(cv, orb, 0.8, BOLT_EDGE, 0.8)
        fill(cv, orb, SPARK, depth=1.8, bias=0.2)
        sparkle(cv, p, 8 if i else 6.5, (1, 1, 1))


def icon_lightning_fury(cv):
    aura(cv, GLOW_SPARK, 0.22)
    burst = V(43, 57)
    rng = np.random.default_rng(50)
    for i, a in enumerate(np.linspace(-120, 10, 7)):
        bolt(cv, bolt_points(burst, polar(burst, a, 29 + rng.uniform(-3, 3)), 4, 2.6, 60 + i), 3.0, 0.6)
    spear(cv, (25, 75), burst + V(1.5, -1.5), head_len=14, head_w=9.5, shaft=3.4, glow_colour=GLOW_SPARK, glow_s=0.8)
    orb = m_circle(burst, 6.5)
    glow(cv, orb, GLOW_SPARK, 3.5, 2.0)
    fill(cv, orb, SPARK, depth=2, bias=0.35)
    sparkle(cv, burst, 11, (1, 1, 1), None, 45)


# ---------------------------------------------------------------- class

def bow_points(centre, deg, L=34, A=7.5, R=4.0, n=40):
    """Centreline of a recurve bow: limbs bowed back by A, tips curling forward by R; rotated by deg."""
    pts = []
    for s in np.linspace(-1, 1, n):
        x = -A * (1 - s * s) + R * np.clip((abs(s) - 0.7) / 0.3, 0, 1) ** 2
        pts.append(V(*centre) + rotate(V(x, s * L / 2), deg))
    return pts


def icon_amazon(cv):
    aura(cv, GLOW_GOLD, 0.24)
    c = V(52, 52)
    deg = -45
    limbs = bow_points(c, deg, L=57, A=11, R=5)
    tips = [limbs[0], limbs[-1]]
    string = m_line(tips, 1.0)
    s_ = np.linspace(-1, 1, len(limbs))
    limb = m_stroke(limbs, 3.2 + 3.0 * (1 - np.abs(s_)) ** 0.8)
    glow(cv, union(limb, string), GLOW_GOLD, 2.5, 0.9)
    outline(cv, string, 0.5, alpha=0.6)
    fill(cv, string, pal("#8a8070", "#d8d0c0", "#FFFFFF", "#FFFFFF"), depth=0.2, bias=0.3)
    spear(cv, (26, 74), (75, 25), head_len=21, head_w=12.5, shaft=3.6, glow_colour=GLOW_GOLD, glow_s=0.8)
    solid(cv, limb, BRONZE, depth=1.0, bias=0.05)
    grip = m_stroke([limbs[17], limbs[20], limbs[22]], 6.2)
    solid(cv, grip, GOLD, depth=0.8, line=1.0)
    for p in tips:
        solid(cv, m_circle(p, 2.6), STEEL, depth=0.6, line=1.0)
    gc = limbs[20]
    gem(cv, gc, [polar(gc, a, 4.4) for a in (-90, 0, 90, 180)], pal("#5a0a1a", "#a01830", "#E04060", "#FFB0C0"),
        line=1.0)


# ---------------------------------------------------------------- registry

TABS = [
    ("Bow and Crossbow", [
        ("MagicArrow", icon_magic_arrow), ("FireArrow", icon_fire_arrow), ("ColdArrow", icon_cold_arrow),
        ("MultipleShot", icon_multiple_shot), ("ExplodingArrow", icon_exploding_arrow), ("IceArrow", icon_ice_arrow),
        ("GuidedArrow", icon_guided_arrow), ("Strafe", icon_strafe), ("ImmolationArrow", icon_immolation_arrow),
        ("FreezingArrow", icon_freezing_arrow)]),
    ("Passive and Magic", [
        ("InnerSight", icon_inner_sight), ("CriticalStrike", icon_critical_strike), ("Dodge", icon_dodge),
        ("SlowMissiles", icon_slow_missiles), ("Avoid", icon_avoid), ("Penetrate", icon_penetrate),
        ("Decoy", icon_decoy), ("Evade", icon_evade), ("Valkyrie", icon_valkyrie), ("Pierce", icon_pierce)]),
    ("Javelin and Spear", [
        ("Jab", icon_jab), ("PowerStrike", icon_power_strike), ("PoisonJavelin", icon_poison_javelin),
        ("Impale", icon_impale), ("LightningBolt", icon_lightning_bolt), ("ChargedStrike", icon_charged_strike),
        ("PlagueJavelin", icon_plague_javelin), ("Fend", icon_fend), ("LightningStrike", icon_lightning_strike),
        ("LightningFury", icon_lightning_fury)]),
]
CLASS_ICON = ("Amazon", icon_amazon)


# ---------------------------------------------------------------- output

def write_raw(image, path):
    """Raw RGBA32 pixels for the mod: 8-byte header (width, height as little-endian int32), then rows bottom to top,
    as Unity stores textures (same format as tools/make_sheet_ui.py)."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8)[::-1]
    h, w = rgba.shape[:2]
    with open(path, "wb") as f:
        f.write(np.array([w, h], dtype="<i4").tobytes())
        f.write(rgba.tobytes())


def render(draw):
    cv = Canvas()
    draw(cv)
    return cv.image()


def font(size):
    for name in ("DejaVuSans-Bold.ttf", "DejaVuSerif-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def label(name):
    return re.sub(r"(?<=[a-z])(?=[A-Z])", " ", name)


def preview(icons):
    """Contact sheet: per tab a row at 64 px on the medallion with names, and a row at 48 px; then the class icon."""
    medallion = Image.open(MEDALLION).convert("RGBA")
    cols, pitch, margin = 10, 100, 20
    w = margin * 2 + cols * pitch
    row_h = 30 + 64 + 22 + 56 + 14
    h = margin + len(TABS) * row_h + 30 + 64 + 22 + margin
    sheet = Image.new("RGBA", (w, h), (0x3A, 0x2A, 0x1E, 255))
    d = ImageDraw.Draw(sheet)
    f_tab, f_name = font(15), font(10)

    def put(name, size, cx, top):
        sheet.alpha_composite(medallion.resize((size, size), Image.LANCZOS), (cx - size // 2, top))
        sheet.alpha_composite(icons[name].resize((size, size), Image.LANCZOS), (cx - size // 2, top))

    y = margin
    for tab, skills in TABS + [("Class", [CLASS_ICON])]:
        d.text((margin, y), tab, font=f_tab, fill=(232, 196, 106))
        y += 26
        for i, (name, _) in enumerate(skills):
            cx = margin + pitch * i + pitch // 2
            put(name, 64, cx, y)
            d.text((cx, y + 68), label(name), font=f_name, fill=(236, 226, 206), anchor="mt")
            if tab != "Class":
                put(name, 48, cx, y + 86)
        y += row_h - 26 if tab != "Class" else 0
    sheet.save(PREVIEW)


def main():
    os.makedirs(OUT, exist_ok=True)
    icons = {}
    for _, skills in TABS + [("Class", [CLASS_ICON])]:
        for name, draw in skills:
            img = render(draw)
            img.save(os.path.join(OUT, name + ".png"))
            write_raw(img, os.path.join(OUT, name + ".rgba"))
            icons[name] = img
            print("wrote", name)
    preview(icons)
    print("wrote", os.path.relpath(PREVIEW, ROOT))


if __name__ == "__main__":
    main()
