"""
Draws a preview of the skills panel (CraftingOverhaul) from the panel's own layout and art, so its look can be judged
outside the game. The positions come from SkillsPanelLayout.cs and SkillTreeLayout.cs through a JSON file a test writes;
the art is CraftingOverhaul/Assets/Sheet (frames, medallion, glows) and CraftingOverhaul/Assets/Skills (icons). The
colours and sizes below mirror SkillsPanel.cs and UiKit.cs: change both together.

    CO_PANEL_LAYOUT=/tmp/layout.json dotnet test CraftingOverhaul.Tests --filter WritesTheLayoutForThePreviewWhenAsked
    python tools/preview_skills_panel.py /tmp/layout.json [tab] [out.png]

In game the panel and its boxes use the game's own panel art; the preview uses the look-alikes (panel, inset).
Text uses a stand-in serif font (the game's Averia isn't available outside it).
"""
import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "tools"))
from make_sheet_ui import nine_slice  # noqa: E402

SHEET = os.path.join(ROOT, "CraftingOverhaul", "Assets", "Sheet")
ICONS = os.path.join(ROOT, "CraftingOverhaul", "Assets", "Skills")
S = 2  # drawn at twice the size, then halved, for smooth edges

# Mirrors UiKit.cs.
GOLD = (232, 204, 150)
LIGHT = (237, 227, 207)
LIGHT_DIM = (199, 184, 161)
RUNE = (110, 199, 255)
CAN_SPEND = (140, 240, 110)
RED = (255, 150, 128)
XP = (158, 102, 217)

FONT_SERIF = "/usr/share/fonts/truetype/liberation/LiberationSerif-Bold.ttf"
FONT_TITLE = "/usr/share/fonts/truetype/dejavu/DejaVuSerif-Bold.ttf"
_fonts = {}


def font(size, title=False):
    key = (size, title)
    if key not in _fonts:
        _fonts[key] = ImageFont.truetype(FONT_TITLE if title else FONT_SERIF, int(size * S))
    return _fonts[key]


def art(name, folder=SHEET):
    path = os.path.join(folder, name + ".png")
    return Image.open(path).convert("RGBA") if os.path.exists(path) else None


def sized(img, w, h):
    return img.resize((max(1, int(w * S)), max(1, int(h * S))), Image.LANCZOS)


def sliced(img, w, h, border):
    big = img.resize((img.size[0] * S, img.size[1] * S), Image.LANCZOS)
    return nine_slice(big, int(w * S), int(h * S), (border * S,) * 4)


def tinted(img, colour, alpha=1.0):
    a = np.asarray(img).astype(float)
    c = np.array(colour[:3], float) / 255.0
    a[..., :3] = a[..., :3] * c
    a[..., 3] = a[..., 3] * alpha
    return Image.fromarray(a.clip(0, 255).astype(np.uint8), "RGBA")


def paste(canvas, img, x, y):
    canvas.alpha_composite(img, (int(round(x * S)), int(round(y * S))))


def text(draw, xy, s, size, colour, anchor="la", title=False, outline=True):
    x, y = xy
    kw = {"stroke_width": max(1, S), "stroke_fill": (0, 0, 0)} if outline else {}
    draw.text((x * S, y * S), s, font=font(size, title), fill=colour, anchor=anchor, **kw)


def wrap(draw, s, size, width):
    words, lines, cur = s.split(), [], ""
    for w in words:
        trial = (cur + " " + w).strip()
        if draw.textlength(trial, font=font(size)) > width * S and cur:
            lines.append(cur)
            cur = w
        else:
            cur = trial
    if cur:
        lines.append(cur)
    return lines


def line(canvas, x1, y1, x2, y2, colour, width):
    ImageDraw.Draw(canvas).line([(x1 * S, y1 * S), (x2 * S, y2 * S)], fill=colour, width=max(1, int(width * S)))


def glow_line(canvas, x1, y1, x2, y2, colour, width, alpha):
    layer = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    ImageDraw.Draw(layer).line([(x1 * S, y1 * S), (x2 * S, y2 * S)], fill=colour + (int(255 * alpha),), width=int(width * S))
    canvas.alpha_composite(layer.filter(ImageFilter.GaussianBlur(width * S / 3)))


def button(canvas, r, label, enabled=True):
    x, y, w, h = r
    d = ImageDraw.Draw(canvas)
    top, bottom = ((120, 84, 44), (78, 52, 26)) if enabled else ((70, 62, 54), (52, 46, 40))
    for i in range(int(h * S)):
        t = i / (h * S)
        c = tuple(int(top[k] * (1 - t) + bottom[k] * t) for k in range(3))
        d.line([(x * S, y * S + i), ((x + w) * S, y * S + i)], fill=c + (255,))
    d.rounded_rectangle([x * S, y * S, (x + w) * S, (y + h) * S], radius=5 * S, outline=(30, 20, 12), width=S)
    text(d, (x + w / 2, y + h / 2), label, 16, GOLD if enabled else LIGHT_DIM, "mm")


def medallion(canvas, x, y, size, icon, state, selected=False):
    med, ring = art("medallion"), art("glow_ring")
    glow = {"learned": RUNE, "spend": CAN_SPEND, "maxed": GOLD}.get(state)
    if selected:
        g = size * 128 / 96 * 1.12
        paste(canvas, tinted(sized(ring, g, g), (255, 240, 200), 1.0), x + size / 2 - g / 2, y + size / 2 - g / 2)
    elif glow:
        g = size * 128 / 96
        paste(canvas, tinted(sized(ring, g, g), glow, 0.75), x + size / 2 - g / 2, y + size / 2 - g / 2)
    tint = {"later": (115, 110, 105), "locked": (184, 178, 173)}.get(state, (255, 255, 255))
    paste(canvas, tinted(sized(med, size, size), tint), x, y)
    if icon is not None:
        inset = size * 0.06
        icon_tint = {"later": (95, 95, 95), "locked": (165, 165, 165)}.get(state, (255, 255, 255))
        paste(canvas, tinted(sized(icon, size - 2 * inset, size - 2 * inset), icon_tint), x + inset, y + inset)


def badge(canvas, cx, cy, value, colour):
    d = ImageDraw.Draw(canvas)
    r = 11
    d.ellipse([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], fill=(16, 12, 10, 240), outline=(168, 120, 62, 255), width=int(1.5 * S))
    text(d, (cx, cy), str(value), 13, colour, "mm", outline=False)


def render(layout, tab, out, hovered="ExplodingArrow"):
    R = {k: tuple(v) for k, v in layout["rects"].items()}
    W, H = R["panel"][2], R["panel"][3]
    canvas = Image.new("RGBA", (int((W + 80) * S), int((H + 80) * S)), (56, 74, 52, 255))
    off = 40

    def r(name):
        x, y, w, h = R[name]
        return (x + off, y + off, w, h)

    x, y, w, h = r("panel")
    paste(canvas, sliced(art("panel"), w, h, 20), x, y)
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("title")
    text(d, (x + w / 2, y + h / 2), "AMAZON", 30, (240, 200, 110), "mm", title=True)
    x, y, w, h = r("close")
    paste(canvas, sized(art("close"), w, h), x, y)
    x, y, w, h = r("skillPoints")
    text(d, (x + w, y + h / 2), "Skill points: 1", 18, GOLD, "rm")

    tabs = ["Bow and Crossbow", "Passive and Magic", "Javelin and Spear"]
    for i, name in enumerate(tabs):
        button(canvas, r("tab%d" % i), name, enabled=i == tab)

    # Character box.
    for box in ("character", "details"):
        x, y, w, h = r(box)
        paste(canvas, sliced(art("inset"), w, h, 10), x, y)
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("level")
    text(d, (x, y + h / 2), "Level 12", 26, GOLD, "lm")
    x, y, w, h = r("xpBar")
    paste(canvas, sliced(art("bar"), w, h, 4), x, y)
    ImageDraw.Draw(canvas).rectangle([(x + 2) * S, (y + 2) * S, (x + 2 + (w - 4) * 0.57) * S, (y + h - 2) * S], fill=XP + (255,))
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("xpText")
    text(d, (x, y + h / 2), "690 / 1205 XP", 14, LIGHT_DIM, "lm")
    x, y, w, h = r("statPoints")
    text(d, (x, y + h / 2), "Stat points: 5", 16, CAN_SPEND, "lm")
    attrs = [("Strength", 20, "+20% melee damage"), ("Dexterity", 40, "+40% bow damage"), ("Vitality", 25, "+75 health"),
             ("Energy", 15, "+22 eitr")]
    for i, (name, value, effect) in enumerate(attrs):
        x, y, w, h = r("attributeName%d" % i)
        text(d, (x, y + h / 2), name, 18, LIGHT, "lm")
        x, y, w, h = r("attributeValue%d" % i)
        text(d, (x + w, y + h / 2), str(value), 18, LIGHT, "rm")
        x, y, w, h = r("attributeEffect%d" % i)
        text(d, (x, y + h / 2), effect, 14, LIGHT_DIM, "lm")
        button(canvas, r("attributePlus%d" % i), "+")
        d = ImageDraw.Draw(canvas)

    # Tablet and tree.
    x, y, w, h = r("tablet")
    paste(canvas, sliced(art("tile"), w, h, 18), x, y)
    tx, ty, tw, th = r("tree")
    d = ImageDraw.Draw(canvas)
    label_w = 56
    for i, cy in enumerate(layout["rowCentres"]):
        text(d, (tx + label_w / 2 - 4, ty + cy), "Lv %d" % layout["tiers"][i], 14, LIGHT_DIM, "mm")
        if i > 0:
            yy = ty + cy - (layout["rowCentres"][1] - layout["rowCentres"][0]) / 2
            line(canvas, tx + label_w, yy, tx + tw - 8, yy, (168, 120, 62, 60), 1)
    data = layout["tabs"][tab]
    by_id = {s["id"]: s for s in data["skills"]}
    # Paths: a dark groove; learned ones carry an eitr-blue light (mirrors SkillsPanel.BuildTree).
    for lit in (False, True):
        for l in data["lines"]:
            learned = by_id[l["from"]]["points"] > 0
            if learned != lit:
                continue
            x1, y1, x2, y2 = tx + l["x1"], ty + l["y1"], tx + l["x2"], ty + l["y2"]
            line(canvas, x1, y1, x2, y2, (0, 0, 0, 140), 6)
            if learned:
                glow_line(canvas, x1, y1, x2, y2, RUNE, 10, 0.55)
                line(canvas, x1, y1, x2, y2, (215, 240, 255, 255), 2)
            else:
                line(canvas, x1, y1, x2, y2, (120, 92, 62, 230), 2)
    tile = layout["tile"]
    for s in data["skills"]:
        state = "later" if s["status"] == "later" else "learned" if s["points"] > 0 else "spend" if s["canSpend"] else "locked"
        icon = art(s["id"], ICONS)
        medallion(canvas, tx + s["x"], ty + s["y"], tile, icon, state, selected=s["id"] == hovered)
        if state != "later":
            badge(canvas, tx + s["x"] + tile - 6, ty + s["y"] + tile - 6, s["points"], CAN_SPEND if s["canSpend"] else LIGHT)
    d = ImageDraw.Draw(canvas)

    # Details pane: the hovered skill.
    sk = next((s for t in layout["tabs"] for s in t["skills"] if s["id"] == hovered), data["skills"][0])
    x, y, w, h = r("detailsIcon")
    medallion(canvas, x, y, w, art(sk["id"], ICONS), "spend" if sk["canSpend"] else "learned" if sk["points"] else "locked")
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("detailsName")
    text(d, (x, y + h / 2), sk["name"], 22, GOLD, "lm")
    x, y, w, h = r("detailsRank")
    text(d, (x, y + h / 2), "Rank %d / 20" % sk["points"], 16, LIGHT, "lm")
    x, y, w, h = r("detailsKind")
    text(d, (x, y + h / 2), "Attack  ·  level 12  ·  5 eitr", 14, LIGHT_DIM, "lm")
    x, y, w, h = r("detailsText")
    yy = y
    for chunk, colour in (("Arrow explodes for area fire damage (8–12).", LIGHT), ("", None),
                          ("Next rank: 11–16 fire damage, larger blast.", CAN_SPEND), ("", None),
                          ("Drag it onto a skill slot to use it.", LIGHT_DIM)):
        if not chunk:
            yy += 8
            continue
        for ln in wrap(d, chunk, 16, w):
            text(d, (x, yy), ln, 16, colour, "la")
            yy += 21
    button(canvas, r("learn"), "Learn  (+1 rank)")
    d = ImageDraw.Draw(canvas)

    # Skill slots.
    x, y, w, h = r("slotsLabel")
    text(d, (x, y + h / 2 - 10), "Skill slots", 20, GOLD, "lm")
    text(d, (x, y + h / 2 + 12), "drag skills here", 14, LIGHT_DIM, "lm")
    slots = ["MultipleShot", "InnerSight", "", "FireArrow"]
    for i, sid in enumerate(slots):
        x, y, w, h = r("slot%d" % i)
        paste(canvas, sliced(art("tile"), w, h, 18), x, y)
        icon = art(sid, ICONS) if sid else None
        if icon is not None:
            paste(canvas, sized(icon, w - 8, h - 8), x + 4, y + 4)
        d = ImageDraw.Draw(canvas)
        text(d, (x + 7, y + 4), str(i + 1), 14, (255, 255, 255), "la")
    canvas = canvas.resize((canvas.size[0] // S, canvas.size[1] // S), Image.LANCZOS)
    canvas.convert("RGB").save(out)
    print("Wrote", out)


def main():
    layout = json.load(open(sys.argv[1]))
    tab = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    out = sys.argv[3] if len(sys.argv) > 3 else os.path.join(ROOT, "docs", "images", "skills_panel_preview.png")
    render(layout, tab, out)


if __name__ == "__main__":
    main()
