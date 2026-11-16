"""
Draws a preview of the Wind Lance look (Orlog) from the real shape numbers, so it can be judged outside the game. The
numbers (the three strand presets, the release ring, the arrow scale and the core) come from WindSwirlRules.cs through
a JSON file a test writes, so there is no second copy of them here. Only the drawing choices (colours, glow, camera,
background) are this script's, and they only approximate WindLanceLook.cs: in game the strands are LineRenderers drawn
with the game's soft trail material and the lights light up the scene.

    ORLOG_SWIRL_JSON=/tmp/swirl.json dotnet test CraftingOverhaul.Tests --filter WritesTheSwirlForThePreviewWhenAsked
    python tools/preview_wind_lance.py /tmp/swirl.json [out.png]

Panels: (1) side view of the lance flying left to right with a normal arrow beside it for size; (2) the view from behind
the player as the lance flies away, its wake trailing back and the release ring close to the camera; (3) the swirl around
the nocked arrow while the shot is charged; (4) the release ring at four ages. Krogon's reference (Elden Ring) is
docs/images/wind_lance_reference.webp.
"""
import json
import math
import os
import sys

import numpy as np
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
S = 2  # drawn at twice the size, then halved, for smooth edges
W, H = 1600, 900

HEAD = (255, 255, 255)        # mirrors WindLanceLook.cs
TAIL = (204, 242, 255)
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
_fonts = {}


def font(size, bold=False):
    key = (size, bold)
    if key not in _fonts:
        _fonts[key] = ImageFont.truetype(FONT_BOLD if bold else FONT, int(size * S))
    return _fonts[key]


def label(draw, x, y, s, size=15, colour=(235, 240, 235), bold=False):
    draw.text((x * S, y * S), s, font=font(size, bold), fill=colour, stroke_width=S, stroke_fill=(0, 0, 0, 255))


def background(w, h):
    """Dark forest green at the top to grey-green at the bottom, with a little noise-free vignette."""
    y = np.linspace(0, 1, h * S)[:, None]
    x = np.linspace(-1, 1, w * S)[None, :]
    top = np.array([18, 34, 28], float)
    bottom = np.array([44, 54, 50], float)
    img = top[None, None, :] * (1 - y[..., None]) + bottom[None, None, :] * y[..., None]
    img = img * (1 - 0.25 * x[..., None] ** 2)
    return Image.fromarray(img.clip(0, 255).astype(np.uint8), "RGB").convert("RGBA")


def lerp_col(u):
    return tuple(int(HEAD[i] + (TAIL[i] - HEAD[i]) * u) for i in range(3))


def densify(a, f=8):
    """More points along a polyline (rows are points), so drawn blobs overlap into a smooth strand."""
    a = np.asarray(a, float)
    t = np.linspace(0, len(a) - 1, (len(a) - 1) * f + 1)
    idx = np.arange(len(a))
    return np.stack([np.interp(t, idx, a[:, k]) for k in range(a.shape[1])], axis=1)


def stroke(layer, xy, width_px, alpha, depth=None, head_alpha=0.9, tint_head=True):
    """Draws one strand as overlapping round blobs. xy: (N,2) px; width_px, alpha, depth (0..1, 1 = towards the viewer): (N,)."""
    d = ImageDraw.Draw(layer)
    n = len(xy)
    for i in reversed(range(n)):          # tail first, so the bright head ends on top
        u = i / max(1, n - 1)
        r = width_px[i] * S / 2
        if r < 0.3 or alpha[i] <= 0.004:
            continue
        shade = 1.0 if depth is None else 0.55 + 0.45 * depth[i]
        c = lerp_col(u if tint_head else 0)
        c = tuple(int(v * shade) for v in c)
        d.ellipse([xy[i][0] * S - r, xy[i][1] * S - r, xy[i][0] * S + r, xy[i][1] * S + r],
                  fill=c + (int(255 * min(1, alpha[i] * head_alpha)),))


def glow(canvas, layer, radii=(6, 18), strength=(0.9, 0.7)):
    """Composite a layer and add blurred copies on top, so the wind looks lit."""
    canvas.alpha_composite(layer)
    base = np.asarray(canvas.convert("RGB")).astype(float)
    for r, k in zip(radii, strength):
        b = layer.filter(ImageFilter.GaussianBlur(r * S / 2))
        a = np.asarray(b).astype(float)
        base += a[..., :3] * (a[..., 3:4] / 255.0) * k
    out = Image.fromarray(base.clip(0, 255).astype(np.uint8), "RGB").convert("RGBA")
    canvas.paste(out)


def panel_frame(canvas, x, y, w, h, title):
    d = ImageDraw.Draw(canvas)
    d.rectangle([x * S, y * S, (x + w) * S, (y + h) * S], outline=(120, 140, 125, 255), width=S)
    label(d, x + 8, y + 5, title, 15, bold=True)


def clip_panel(canvas, x, y, w, h):
    """A fresh canvas for one panel (so lines and glow stay inside its frame); pasted back by paste_panel."""
    return canvas.crop((x * S, y * S, (x + w) * S, (y + h) * S))


def paste_panel(canvas, sub, x, y):
    canvas.paste(sub, (x * S, y * S))


def arrow(d, tip, length_m, pm, shaft=(150, 115, 75), alpha=255):
    """A side-on arrow: shaft, steel head, fletching. tip: px (x, y); the arrow points right (towards +Z)."""
    x, y = tip
    L = length_m * pm
    sc = length_m / 0.8                 # a normal arrow is about 0.8 m
    sw = max(2, 0.012 * sc * pm)
    d.line([(x - L) * S, y * S, x * S, y * S], fill=shaft + (alpha,), width=int(sw * S))
    hl = 0.09 * sc * pm
    hw = 0.035 * sc * pm
    d.polygon([(x * S, y * S), ((x - hl) * S, (y - hw) * S), ((x - hl) * S, (y + hw) * S)], fill=(190, 195, 200, alpha))
    for k in range(3):                  # fletching
        fx = x - L + k * 0.025 * sc * pm
        d.polygon([(fx * S, y * S), ((fx - 0.05 * sc * pm) * S, (y - 0.05 * sc * pm) * S),
                   ((fx + 0.02 * sc * pm) * S, (y - 0.05 * sc * pm) * S), ((fx + 0.07 * sc * pm) * S, y * S)],
                  fill=(210, 200, 185, alpha))


def ribbon_arrays(rib):
    a = np.array(rib, float)
    return a[:, 0], a[:, 1], a[:, 2], a[:, 3], a[:, 4]   # x, y, z, width, alpha


# ---------------------------------------------------------------------------------------------------- panel 1
def side_view(canvas, data, x0, y0, w, h):
    panel_frame(canvas, x0, y0, w, h, "1. Side view: the lance in flight (arrow drawn 3x; a normal arrow below for size)")
    pm = 190.0                                        # px per metre
    cy = y0 + h * 0.44
    head_x = x0 + w - 130                             # where z = lance head (0.6 m) lands; +z to the right
    z0 = data["lance"][0]["ribbons"][0][0][2]
    sx = lambda z: head_x - (z0 - z) * -pm * -1       # noqa: E731  (placeholder replaced below)
    sx = lambda z: head_x + (z - z0) * pm             # noqa: E731
    sub = clip_panel(canvas, x0, y0, w, h)
    ox, oy = x0, y0
    layer = Image.new("RGBA", sub.size, (0, 0, 0, 0))
    ld = ImageDraw.Draw(layer)
    # the arrow, 3x: its tip a little ahead of the core's start
    scale = data["arrowScale"]
    d = ImageDraw.Draw(sub)
    arrow(d, (sx(0.55) - ox, cy - oy), 0.8 * scale, pm)
    # strands (the swirl at time 0)
    for r in data["lance"][0]["ribbons"]:
        x, y, z, wd, al = ribbon_arrays(r)
        pts = np.stack([sx(z) - ox, cy - oy - y * pm], axis=1)
        depth = (x / 0.32 + 1) / 2
        stroke(layer, densify(pts), densify(wd[:, None])[:, 0] * pm, densify(al[:, None])[:, 0], densify(depth[:, None])[:, 0])
    # the core
    cl, cw = data["coreLength"], data["coreWidth"]
    n = 40
    for i in reversed(range(n)):
        u = i / (n - 1)
        z = 0.4 - cl * u
        r = cw * (1 - u) * pm / 2 * S
        ld.ellipse([(sx(z) - ox) * S - r, (cy - oy) * S - r, (sx(z) - ox) * S + r, (cy - oy) * S + r],
                   fill=(255, 255, 255, int(255 * 0.85 * (1 - u))))
    glow(sub, layer, radii=(5, 16), strength=(0.8, 0.8))
    d = ImageDraw.Draw(sub)
    arrow(d, (sx(0.55) - ox, cy - oy), 0.8 * scale, pm, alpha=200)   # the arrow again, so its head shows through the wind
    # comparison: a normal arrow and a 1 m bar
    by = h - 55
    arrow(d, (190, by), 0.8, pm)
    label(d, 40, by - 32, "a normal arrow (0.8 m)", 13)
    d.line([290 * S, by * S, (290 + pm) * S, by * S], fill=(235, 240, 235, 255), width=2 * S)
    for xx in (290, 290 + pm):
        d.line([xx * S, (by - 6) * S, xx * S, (by + 6) * S], fill=(235, 240, 235, 255), width=2 * S)
    label(d, 290 + pm / 2 - 14, by + 8, "1 m", 13)
    label(d, 20, 38, "flight direction  >>>", 13, (190, 205, 190))
    paste_panel(canvas, sub, x0, y0)


# ---------------------------------------------------------------------------------------------------- panel 2
def behind_view(canvas, data, x0, y0, w, h):
    panel_frame(canvas, x0, y0, w, h, "2. From behind the player: lance flying away, wake trailing back, release ring at the bow")
    sub = clip_panel(canvas, x0, y0, w, h)
    d = ImageDraw.Draw(sub)
    cx, cy = w / 2, h * 0.52
    d.rectangle([0, cy * S, w * S, h * S], fill=(30, 38, 33, 255))     # the ground, a shade darker
    f = 820.0                                                         # focal length in px
    cam = -1.0                                                         # camera z (the player stands near 0)
    lance_z = 12.0                                                     # how far the lance has flown

    def proj(x, y, z):
        dist = max(0.3, z - cam)
        return cx + f * x / dist, cy - f * y / dist, f / dist

    layer = Image.new("RGBA", sub.size, (0, 0, 0, 0))
    ld = ImageDraw.Draw(layer)
    # the wake: helices along the path from z = 0 to the lance
    for r in data["wake"]["ribbons"]:
        x, y, z, wd, al = ribbon_arrays(r)
        keep = (z <= lance_z) & (z >= 0.5)
        x, y, z, wd = x[keep], y[keep], z[keep], wd[keep]
        pts = []
        wpx = []
        for xi, yi, zi, wi in zip(x, y, z, wd):
            px, py, sc = proj(xi, yi, zi)
            pts.append((px, py))
            wpx.append(max(1.0, wi * sc))
        pts = np.array(pts)
        depth = (x / 0.22 + 1) / 2
        stroke(layer, densify(pts), densify(np.array(wpx)[:, None])[:, 0], np.full((len(pts) - 1) * 8 + 1, 0.6),
               densify(depth[:, None])[:, 0], tint_head=False, head_alpha=0.75)
    # the lance at its far end: swirl seen end-on, around a glowing core point
    for r in data["lance"][0]["ribbons"]:
        x, y, z, wd, al = ribbon_arrays(r)
        pts = np.array([proj(xi, yi, lance_z + zi)[:2] for xi, yi, zi in zip(x, y, z)])
        sc = np.array([proj(xi, yi, lance_z + zi)[2] for xi, yi, zi in zip(x, y, z)])
        stroke(layer, densify(pts), np.maximum(1.0, densify((wd * sc)[:, None])[:, 0]), densify(al[:, None])[:, 0])
    px, py, sc = proj(0, 0, lance_z + 0.4)
    rr = 0.16 * sc * S
    ld.ellipse([px * S - rr, py * S - rr, px * S + rr, py * S + rr], fill=(255, 255, 255, 290))
    glow(sub, layer, radii=(4, 12), strength=(0.9, 0.8))
    # the release ring near the camera (the burst at age 0.1), a spiral ring in the plane facing the camera
    layer = Image.new("RGBA", sub.size, (0, 0, 0, 0))
    b = data["burst"][1]
    n = data["ringPoints"]
    for line in range(2):
        pts, wpx = [], []
        for i in range(n):
            fr = i / (n - 1)
            ang = 2 * math.pi * fr + (math.pi if line else 0) + 0.8
            rad = b["radius"] * (0.85 + 0.15 * fr)
            pxx, pyy, sc = proj(rad * math.cos(ang), rad * math.sin(ang), 1.6)
            pts.append((pxx, pyy))
            wpx.append((0.08 + (0.02 - 0.08) * fr) * sc)
        stroke(layer, densify(np.array(pts)), densify(np.array(wpx)[:, None])[:, 0],
               np.full((n - 1) * 8 + 1, 0.8 * b["alpha"]), tint_head=False)
    glow(sub, layer, radii=(5, 14), strength=(0.8, 0.6))
    d = ImageDraw.Draw(sub)
    label(d, 14, h - 28, "ring of wind at the bow", 13)
    label(d, cx - 20, cy + 14, "lance", 13)
    paste_panel(canvas, sub, x0, y0)


# ---------------------------------------------------------------------------------------------------- panel 3
def charge_view(canvas, data, x0, y0, w, h):
    panel_frame(canvas, x0, y0, w, h, "3. Charged shot ready: wind round the nocked arrow")
    sub = clip_panel(canvas, x0, y0, w, h)
    d = ImageDraw.Draw(sub)
    pm = 330.0
    cy = h * 0.55
    head_x = w - 70
    sx = lambda z: head_x + (z - 0.45) * pm   # noqa: E731
    # a bow, seen from the side, with the hand at the grip
    gx = sx(0.0)
    d.arc([(gx - 40) * S, (cy - 150) * S, (gx + 120) * S, (cy + 150) * S], 100, 260, fill=(110, 80, 50, 255), width=5 * S)
    arrow(d, (sx(0.45) - 0, cy), 0.8, pm)
    layer = Image.new("RGBA", sub.size, (0, 0, 0, 0))
    for r in data["charge"][0]["ribbons"]:
        x, y, z, wd, al = ribbon_arrays(r)
        pts = np.stack([sx(z), cy - y * pm], axis=1)
        depth = (x / 0.16 + 1) / 2
        stroke(layer, densify(pts), densify(wd[:, None])[:, 0] * pm, densify(al[:, None])[:, 0], densify(depth[:, None])[:, 0])
    glow(sub, layer, radii=(4, 12), strength=(0.8, 0.7))
    d = ImageDraw.Draw(sub)
    label(d, 14, h - 28, "bow hand + a pale light", 13)
    paste_panel(canvas, sub, x0, y0)


# ---------------------------------------------------------------------------------------------------- panel 4
def burst_frames(canvas, data, x0, y0, w, h):
    panel_frame(canvas, x0, y0, w, h, "4. The release ring over its life (0.35 s): grows, turns, fades")
    sub = clip_panel(canvas, x0, y0, w, h)
    n = data["ringPoints"]
    fw, fh = (w - 30) / 2, (h - 60) / 2
    pm = 78.0
    d = ImageDraw.Draw(sub)
    for k, b in enumerate(data["burst"]):
        fx = 10 + (k % 2) * (fw + 10)
        fy = 36 + (k // 2) * (fh + 6)
        layer = Image.new("RGBA", sub.size, (0, 0, 0, 0))
        cxx, cyy = fx + fw / 2, fy + fh / 2
        turn = 2 * math.pi * b["age"]                    # 360 degrees per second
        for line in range(2):
            pts, wpx = [], []
            for i in range(n):
                fr = i / (n - 1)
                ang = 2 * math.pi * fr + (math.pi if line else 0) + turn
                rad = b["radius"] * (0.85 + 0.15 * fr)
                pts.append((cxx + rad * pm * math.cos(ang), cyy + rad * pm * math.sin(ang)))
                wpx.append((0.08 - 0.06 * fr) * pm)
            stroke(layer, densify(np.array(pts)), densify(np.array(wpx)[:, None])[:, 0],
                   np.full((n - 1) * 8 + 1, 0.8 * b["alpha"]), tint_head=False)
        d.rectangle([fx * S, fy * S, (fx + fw) * S, (fy + fh) * S], outline=(80, 95, 85, 255), width=1)
        glow(sub, layer, radii=(3, 9), strength=(0.8, 0.6))
        d = ImageDraw.Draw(sub)
        label(d, fx + 6, fy + 4, "%.2f s   radius %.2f m" % (b["age"], b["radius"]), 12)
    paste_panel(canvas, sub, x0, y0)


def main():
    args = [a for a in sys.argv[1:]]
    if not args:
        print(__doc__)
        sys.exit(1)
    data = json.load(open(args[0]))
    out = args[1] if len(args) > 1 else os.path.join(ROOT, "docs", "images", "wind_lance_preview.png")
    canvas = background(W, H)
    side_view(canvas, data, 20, 20, 1000, 430)
    charge_view(canvas, data, 1040, 20, 540, 430)
    behind_view(canvas, data, 20, 470, 900, 410)
    burst_frames(canvas, data, 940, 470, 640, 410)
    final = canvas.convert("RGB").resize((W, H), Image.LANCZOS)
    final.save(out)
    print("wrote", out, final.size)


if __name__ == "__main__":
    main()
