"""
Draws a preview of the skills panel (CraftingOverhaul) from the panel's own layout and art, so its look can be judged
outside the game. The positions come from SkillsPanelLayout.cs and SkillCardsLayout.cs through a JSON file a test
writes; the art is CraftingOverhaul/Assets/Sheet (frames, medallion, glows) and CraftingOverhaul/Assets/Skills (icons).
The colours and sizes below mirror SkillsPanel.cs and UiKit.cs: change both together. The points shown are a sample
character (level 10, as in Krogon's mock-up docs/images/skills_panel_cards_mockup.webp).

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


def medallion(canvas, x, y, size, icon, state):
    """A skill's medallion (mirrors SkillsPanel.BuildCard): state is later, notLearned, canLearn, learned or maxed."""
    med, ring = art("medallion"), art("glow_ring")
    glow = {"learned": RUNE, "canLearn": CAN_SPEND, "maxed": GOLD}.get(state)
    if glow:
        g = size * 128 / 96
        paste(canvas, tinted(sized(ring, g, g), glow, 0.75), x + size / 2 - g / 2, y + size / 2 - g / 2)
    tint = {"later": (148, 143, 138), "notLearned": (209, 204, 199), "canLearn": (209, 204, 199)}.get(state, (255, 255, 255))
    paste(canvas, tinted(sized(med, size, size), tint), x, y)
    if icon is not None:
        inset = size * 0.06
        icon_tint = {"later": (133, 133, 133), "notLearned": (204, 204, 204), "canLearn": (204, 204, 204)}.get(state, (255, 255, 255))
        paste(canvas, tinted(sized(icon, size - 2 * inset, size - 2 * inset), icon_tint), x + inset, y + inset)


# A sample character for the preview: points per skill; everything else is unlearned.
SAMPLE_POINTS = {"MultipleShot": 3, "FreezingArrow": 1, "InnerSight": 1, "CriticalStrike": 5, "Dodge": 2, "Evade": 20}
SAMPLE_SKILL_POINTS = 10
SAMPLE_SELECTED = ["MultipleShot", "CriticalStrike", "Jab"]
SAMPLE_TEXT = {
    "MultipleShot": ("Attack  ·  6 eitr", "Fires 4 arrows in a fan for one arrow, each at 75% damage",
                     "Next rank: Level 4: Fires 5 arrows in a fan for one arrow, each at 75% damage",
                     "Drag it onto a skill slot to use it."),
    "CriticalStrike": ("Passive", "42% chance to deal double physical damage",
                       "Next rank: Level 6: 45% chance to deal double physical damage", "Passive: always on once learned."),
    "Jab": ("Attack  ·  2 eitr", "Not learned yet. A fast series of thrusts", "Next rank: Level 1: A fast series of thrusts",
            "Drag it onto a skill slot to use it."),
}
MAX_POINTS = 20


def state_of(skill):
    """Mirrors SkillCardsLayout.StateOf for the sample character."""
    points = SAMPLE_POINTS.get(skill["id"], 0)
    if skill["status"] == "later":
        return "later"
    if points >= MAX_POINTS:
        return "maxed"
    if points > 0:
        return "learned"
    return "canLearn" if SAMPLE_SKILL_POINTS > 0 else "notLearned"


def card(canvas, layout, x, y, skill, selected):
    """One skill card (mirrors SkillsPanel.BuildCard)."""
    w, h = skill["w"], skill["h"]
    state = state_of(skill)
    points = SAMPLE_POINTS.get(skill["id"], 0)
    learned = state in ("learned", "maxed")
    if selected:
        inset = 14
        paste(canvas, tinted(sliced(art("glow_box"), w + 2 * inset, h + 2 * inset, 24), (255, 199, 92)), x - inset, y - inset)
    wood = 0.64 if state == "later" else 1.0 if learned else 0.8
    paste(canvas, tinted(sliced(art("panel"), w, h, 20), (int(255 * wood),) * 3), x, y)
    ix, iy, iw, ih = layout["card"]["icon"]
    medallion(canvas, x + ix, y + iy, iw, art(skill["id"], ICONS), state)
    d = ImageDraw.Draw(canvas)
    nx, ny, nw, nh = layout["card"]["name"]
    text(d, (x + nx + nw / 2, y + ny + nh / 2), skill["name"], 20, LIGHT_DIM if state == "later" else GOLD if learned else LIGHT, "mm")
    rx, ry, rw, rh = layout["card"]["rank"]
    label = "Coming later" if state == "later" else "Rank %d / %d" % (points, MAX_POINTS) if points else "Not learned"
    text(d, (x + rx + rw / 2, y + ry + rh / 2), label, 15, CAN_SPEND if state == "canLearn" else LIGHT if learned else LIGHT_DIM, "mm")
    if learned:
        bx, by, bw, bh = layout["card"]["rankBar"]
        paste(canvas, sliced(art("bar"), bw, bh, 4), x + bx, y + by)
        fill = GOLD if state == "maxed" else XP
        ImageDraw.Draw(canvas).rectangle([(x + bx + 2) * S, (y + by + 2) * S, (x + bx + 2 + (bw - 4) * points / MAX_POINTS) * S,
                                          (y + by + bh - 2) * S], fill=fill + (255,))


def render(layout, tab, out):
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
    text(d, (x + w, y + h / 2), "Skill points: %d" % SAMPLE_SKILL_POINTS, 18, CAN_SPEND, "rm")

    tabs = ["Bow and Crossbow", "Passive and Magic", "Javelin and Spear"]
    for i, name in enumerate(tabs):
        button(canvas, r("tab%d" % i), name, enabled=i == tab)

    # Character box, details pane and the strip under the tablet.
    for box in ("character", "details", "slotsRow"):
        x, y, w, h = r(box)
        paste(canvas, sliced(art("inset"), w, h, 10), x, y)
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("level")
    text(d, (x, y + h / 2), "Level 10", 26, GOLD, "lm")
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

    # Tablet and the tab's nine cards.
    x, y, w, h = r("tablet")
    paste(canvas, sliced(art("tile"), w, h, 18), x, y)
    cx, cy, cw, ch = r("cards")
    data = layout["tabs"][tab]
    selected = SAMPLE_SELECTED[tab]
    for sk in data["skills"]:
        card(canvas, layout, cx + sk["x"], cy + sk["y"], sk, sk["id"] == selected)
    d = ImageDraw.Draw(canvas)

    # Details pane: the selected skill.
    sk = next(s for s in data["skills"] if s["id"] == selected)
    points = SAMPLE_POINTS.get(sk["id"], 0)
    kind, now, nxt, use = SAMPLE_TEXT[sk["id"]]
    x, y, w, h = r("detailsIcon")
    medallion(canvas, x, y, w, art(sk["id"], ICONS), state_of(sk))
    d = ImageDraw.Draw(canvas)
    x, y, w, h = r("detailsName")
    text(d, (x, y + h / 2), sk["name"], 22, GOLD, "lm")
    x, y, w, h = r("detailsRank")
    text(d, (x, y + h / 2), "Rank %d / %d" % (points, MAX_POINTS), 16, LIGHT, "lm")
    x, y, w, h = r("detailsKind")
    text(d, (x, y + h / 2), kind, 14, LIGHT_DIM, "lm")
    x, y, w, h = r("detailsText")
    yy = y
    for chunk, colour in ((now, LIGHT), ("", None), (nxt, CAN_SPEND), ("", None), (use, LIGHT_DIM)):
        if not chunk:
            yy += 8
            continue
        for ln in wrap(d, chunk, 15, w):
            text(d, (x, yy), ln, 15, colour, "la")
            yy += 20

    # The strip: skill slots, a divider, Learn.
    x, y, w, h = r("slotsLabel")
    text(d, (x, y + h / 2 - 10), "Skill slots", 20, GOLD, "lm")
    text(d, (x, y + h / 2 + 12), "drag skills here", 14, LIGHT_DIM, "lm")
    slots = ["MultipleShot", "InnerSight", "FreezingArrow", ""]
    for i, sid in enumerate(slots):
        x, y, w, h = r("slot%d" % i)
        paste(canvas, sliced(art("tile"), w, h, 18), x, y)
        icon = art(sid, ICONS) if sid else None
        if icon is not None:
            paste(canvas, sized(icon, w - 8, h - 8), x + 4, y + 4)
        d = ImageDraw.Draw(canvas)
        text(d, (x + 7, y + 4), "F%d" % (i + 1), 14, (255, 255, 255), "la")
    x, y, w, h = r("slotsDivider")
    ImageDraw.Draw(canvas, "RGBA").rectangle([x * S, y * S, (x + max(w, 1)) * S, (y + h) * S], fill=(168, 120, 62, 102))
    button(canvas, r("learn"), "Learn  (+1 rank)")
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
