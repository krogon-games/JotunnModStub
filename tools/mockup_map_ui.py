"""
Preview of the map item's new look: the inventory hover tooltip and the Riftforge's Forge tab, side by side
(Krogon's approved mock-up: docs/images/map_ui_approved_mockup.png). Unlike the older mock-ups, the text is NOT typed
in here: it is the exact rich text the C# code produces, exported to JSON by a test, and drawn in the game's own font
(Averia Serif Libre) with a small TextMeshPro-style renderer (colour, bold, size, indent, align, mark tags; unknown
tags are ignored). The Forge's sheet and result text are auto-sized down to 70% like the game's text boxes, and each
Forge panel gets a note under it: the size factor used and "fits" or "CLIPS".

    MAP_UI_PREVIEW_OUT=/tmp/map_ui_preview.json dotnet test CraftingOverhaul.Tests --filter MapUiPreviewExport
    python tools/mockup_map_ui.py [--json /tmp/map_ui_preview.json] [--font-dir DIR] [--forge-h 540]

Writes docs/images/map_ui_tier5.png (one map, like the approved mock-up) and docs/images/map_ui_tiers.png (tiers 1, 3
and 6 with their result messages and marks). --font-dir holds averia-serif-libre-latin-400-normal.woff and
-700-normal.woff (from the npm package @fontsource/averia-serif-libre); without them a stand-in serif is used and the
pictures say so.

The layout sizes (panel 640 wide, bench 236, sheet 362, tooltip 360 wide with 18 padding, text 16 with line height
1.3) are the estimates of the earlier approved pictures, NOT measured in the game: the real boxes can differ a little.
Reuses the helpers of tools/mockup_forge_maps.py, mockup_riftforge.py and preview_skills_panel.py.
"""
import argparse
import json
import os
import re
import sys

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import preview_skills_panel as P  # noqa: E402
import mockup_riftforge as M  # noqa: E402
import mockup_forge_maps as F  # noqa: E402

S = P.S
OUT = M.OUT
SCRATCH = "/tmp/claude-0/-home-user-JotunnModStub/c3c0a0e2-f37a-58a7-955d-6152c7328aba/scratchpad"
DEFAULT_JSON = SCRATCH + "/map_ui_preview.json"
DEFAULT_FONTS = SCRATCH + "/fonts/x/package/files"
LIBERATION = "/usr/share/fonts/truetype/liberation/LiberationSerif-%s.ttf"

BG = (24, 20, 17)
GOLD = (248, 188, 66)
DIM = (150, 140, 126)
RED = (255, 90, 80)
BASE_TEXT = (237, 227, 207)
LINE_H = 1.3
NAMED = {"orange": "#FFA500", "white": "#FFFFFF", "red": "#FF0000", "yellow": "#FFFF00", "green": "#008000",
         "blue": "#0000FF", "black": "#000000", "grey": "#808080", "gray": "#808080", "cyan": "#00FFFF",
         "magenta": "#FF00FF", "purple": "#800080", "lightblue": "#ADD8E6", "darkblue": "#00008B", "brown": "#A52A2A"}

# ---------------------------------------------------------------- fonts
_paths = {"regular": None, "bold": None}
STAND_IN = False
_cache = {}


def setup_fonts(font_dir):
    global STAND_IN
    reg = os.path.join(font_dir, "averia-serif-libre-latin-400-normal.woff")
    bold = os.path.join(font_dir, "averia-serif-libre-latin-700-normal.woff")
    if os.path.exists(reg) and os.path.exists(bold):
        _paths["regular"], _paths["bold"] = reg, bold
    else:
        print("WARNING: Averia Serif Libre not found in %s; using Liberation Serif (stand-in font)" % font_dir)
        STAND_IN = True
        _paths["regular"], _paths["bold"] = LIBERATION % "Regular", LIBERATION % "Bold"
    P.font = lambda size, title=False: fnt(size, bold=title)  # the helpers' text() and wrap() use this


def fnt(size, bold=False):
    """A font for `size` canvas units, drawn at scale S."""
    key = (round(size * S, 2), bold)
    if key not in _cache:
        _cache[key] = ImageFont.truetype(_paths["bold" if bold else "regular"], key[0])
    return _cache[key]


# ---------------------------------------------------------------- colours
def colour(s, default=BASE_TEXT):
    """(r, g, b, a) from '#RGB', '#RRGGBB', '#RRGGBBAA' or a name."""
    s = (s or "").strip().strip('"').lower()
    s = NAMED.get(s, s)
    if not s.startswith("#"):
        return default + (255,)
    h = s[1:]
    if len(h) in (3, 4):
        h = "".join(c * 2 for c in h)
    try:
        v = [int(h[i:i + 2], 16) for i in range(0, len(h), 2)]
    except ValueError:
        return default + (255,)
    if len(v) == 3:
        v.append(255)
    return tuple(v[:4]) if len(v) >= 4 else default + (255,)


# ---------------------------------------------------------------- rich text
TAG = re.compile(r"<(/?)([a-zA-Z]+)(?:=([^>]*))?>")


def tokenize(s):
    out, pos = [], 0
    for m in TAG.finditer(s):
        if m.start() > pos:
            out.append(("text", s[pos:m.start()]))
        out.append(("tag", m.group(1) == "/", m.group(2).lower(), m.group(3)))
        pos = m.end()
    if pos < len(s):
        out.append(("text", s[pos:]))
    return out


class Seg:
    def __init__(self, text, style, x, w):
        self.text, self.style, self.x, self.w = text, style, x, w


def layout(s, base, width, default_colour=BASE_TEXT, default_align="left"):
    """Lays rich text out like TextMeshPro. Returns (lines, height); a line is {segs, y, h, align, minx, maxx}.
    Line height = 1.3 x the largest size on the line."""
    colours, sizes, bolds, aligns, marks = [colour(None, default_colour)], [base], [False], [default_align], [None]
    indents = [0.0]
    lines, cur = [], []
    st = {"x": 0.0, "wrapped": False}

    def style():
        return (colours[-1], bolds[-1], sizes[-1], marks[-1])

    def finish():
        if cur:
            h = max(sg.style[2] for sg in cur) * LINE_H
            real = [sg for sg in cur if sg.text.strip()]
            minx = min(sg.x for sg in real) if real else 0
            maxx = max(sg.x + sg.w for sg in real) if real else 0
            al = aligns[-1] if not real else cur_align[0]
        else:
            h, minx, maxx, al = sizes[-1] * LINE_H, 0, 0, aligns[-1]
        lines.append({"segs": list(cur), "h": h, "align": al, "minx": minx, "maxx": maxx})
        cur.clear()
        st["x"], st["wrapped"] = indents[-1], False

    cur_align = [default_align]
    st["x"] = 0.0
    for tok in tokenize(s.replace("\r", "")):
        if tok[0] == "text":
            for para_i, para in enumerate(tok[1].split("\n")):
                if para_i > 0:
                    finish()
                for piece in re.findall(r"\S+|\s+", para):
                    f = fnt(sizes[-1], bolds[-1])
                    w = f.getlength(piece) / S
                    space = piece.isspace()
                    if space and st["wrapped"] and not cur:
                        continue
                    if (not space and st["x"] + w > width and any(sg.text.strip() for sg in cur)):
                        finish()
                        st["wrapped"] = True
                    if not cur:
                        cur_align[0] = aligns[-1]
                    cur.append(Seg(piece, style(), st["x"], w))
                    st["x"] += w
            continue
        _, closing, name, arg = tok
        if name == "color":
            if not closing:
                colours.append(colour(arg, default_colour))
            elif len(colours) > 1:
                colours.pop()
        elif name == "b":
            bolds.append(True) if not closing else (bolds.pop() if len(bolds) > 1 else None)
        elif name == "size":
            if closing:
                sizes.pop() if len(sizes) > 1 else None
            else:
                a = (arg or "100%").strip()
                try:
                    sizes.append(base * float(a[:-1]) / 100 if a.endswith("%") else float(a))
                except ValueError:
                    sizes.append(sizes[-1])
        elif name == "indent":
            if closing:
                if len(indents) > 1:
                    indents.pop()
            else:
                a = (arg or "0").strip()
                try:
                    px = float(a[:-2]) * sizes[-1] if a.endswith("em") else float(a.rstrip("%px"))
                except ValueError:
                    px = 0.0
                indents.append(px)
                st["x"] = max(st["x"], px)
        elif name == "align":
            if closing:
                aligns.pop() if len(aligns) > 1 else None
            else:
                aligns.append((arg or "left").strip().strip('"').lower())
        elif name == "mark":
            marks.append(colour(arg)) if not closing else (marks.pop() if len(marks) > 1 else None)
        # unknown tags are ignored
    if cur or not lines:
        finish()
    # y positions
    y = 0.0
    for ln in lines:
        ln["y"] = y
        y += ln["h"]
    return lines, y


def draw_lines(canvas, lines, x, y, width):
    d = ImageDraw.Draw(canvas)
    for ln in lines:
        shift = 0.0
        if ln["align"] == "center":
            shift = (width - (ln["maxx"] - ln["minx"])) / 2 - ln["minx"]
        elif ln["align"] == "right":
            shift = width - ln["maxx"]
        for sg in ln["segs"]:
            col, bold, size, mark = sg.style
            f = fnt(size, bold)
            sx, sy = (x + sg.x + shift) * S, (y + ln["y"]) * S
            if mark is not None and mark[3] > 0:
                box = Image.new("RGBA", (max(1, int(sg.w * S)), max(1, int(ln["h"] * S))), mark)
                canvas.alpha_composite(box, (int(round(sx)), int(round(sy))))
                d = ImageDraw.Draw(canvas)
            if not sg.text.strip():
                continue
            asc, desc = f.getmetrics()
            base_y = sy + (ln["h"] * S - (asc + desc)) / 2 + asc
            d.text((sx, base_y), sg.text, font=f, fill=col, anchor="ls")


def fit(s, base, width, avail, default_colour=BASE_TEXT, minimum=0.75, default_align="left"):
    """TextMeshPro-style auto-size: 100% down to 75% in 1% steps. Returns (lines, height, factor, fits)."""
    for pct in range(100, int(minimum * 100) - 1, -1):
        lines, h = layout(s, base * pct / 100, width, default_colour, default_align)
        if h <= avail + 0.01:
            return lines, h, pct / 100, True
    lines, h = layout(s, base * minimum, width, default_colour, default_align)
    return lines, h, minimum, False


# ---------------------------------------------------------------- the two views
def tier_corner(canvas, x, y, size, tier, col):
    d = ImageDraw.Draw(canvas)
    d.text(((x + size - 7) * S, (y + 5) * S), str(tier), font=fnt(19 if size < 70 else 24, True), fill=col,
           anchor="ra", stroke_width=2 * S, stroke_fill=(0, 0, 0))


def draw_forge(canvas, x, y, case, forge_h, stats):
    f = case["forge"]
    bx, top, bw, bh = F.shell(canvas, x, y, h=forge_h)
    cx = bx + bw / 2
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), bx, top)
    d = ImageDraw.Draw(canvas)
    P.text(d, (cx, top + 20), "Forge", 20, F.ORANGE, "mm")
    M.slot(canvas, cx - 44, top + 38, 88, F.map_icon(case["biome"]), glow=(255, 240, 200))
    tier_corner(canvas, cx - 44, top + 38, 88, case["tier"], colour(case["tierColor"])[:3])
    # caption (base 17, centred) and the bench text (base 16, from top + 168)
    cap, caph = layout(f.get("caption", ""), 17, bw - 28, BASE_TEXT, "center")
    draw_lines(canvas, cap, bx + 14, top + 146 - 11, bw - 28)
    btext, bth = layout(f.get("benchText", ""), 16, bw - 28)
    draw_lines(canvas, btext, bx + 14, top + 168, bw - 28)
    stats.append(("bench text", 16, len(btext), bth, bh - 60 - 168))
    P.button(canvas, (x + 36, top + bh - 52, bw - 40, 38), "Remove")
    # sheet
    dx, dw = x + 262, 640 - 278
    P.paste(canvas, P.sliced(P.art("inset"), dw, bh, 10), dx, top)
    d = ImageDraw.Draw(canvas)
    d.text(((dx + 16) * S, (top + 30) * S), f["title"], font=fnt(24, True), fill=colour(f["titleColor"])[:3], anchor="lm",
           stroke_width=S, stroke_fill=(0, 0, 0))
    band = 0.0
    if f.get("result"):
        _, band = layout(f["result"], 16, dw - 32)
        band = min(band, 84.0)
    avail = bh - (12 + band + (8 if band else 0)) - 58
    sheet, sh, factor, ok = fit(f["sheet"], 16, dw - 32, avail, minimum=0.70)
    draw_lines(canvas, sheet, dx + 16, top + 58, dw - 32)
    stats.append(("sheet", 16 * factor, len(sheet), sh, avail))
    all_ok = ok
    note = "sheet text at 100%" if factor == 1 else "auto-sized to %d%%" % round(factor * 100)
    if f.get("result"):
        rcol = (200, 235, 160) if f.get("resultGood") else (255, 140, 122)
        res, rh, rf, rok = fit(f["result"], 16, dw - 32, band, rcol)
        draw_lines(canvas, res, dx + 16, top + bh - 12 - band, dw - 32)
        stats.append(("result", 16 * rf, len(res), rh, band))
        all_ok = all_ok and rok
        if rf < 1:
            note += ", result at %d%%" % round(rf * 100)
    return note, all_ok, forge_h


def draw_tooltip(canvas, x, y, case, stats):
    t = case["tooltip"]
    w, pad = 360, 18
    title, th = layout("<b>%s</b>" % t["title"], 26, w - 2 * pad, colour(t["titleColor"])[:3])
    body, bh = layout(t["text"], 16, w - 2 * pad)
    h = pad + th + 4 + bh + pad
    d = ImageDraw.Draw(canvas)
    d.rectangle([x * S, y * S, (x + w) * S, (y + h) * S], fill=(14, 11, 9, 242), outline=(96, 84, 62, 255), width=2 * S)
    draw_lines(canvas, title, x + pad, y + pad, w - 2 * pad)
    draw_lines(canvas, body, x + pad, y + pad + th + 4, w - 2 * pad)
    stats.append(("tooltip", 16, len(body), h, h))
    return h


def draw_case(case, forge_h, headings, label=None):
    """One Forge + tooltip pair on its own canvas. Returns (image at scale S, stats)."""
    pad, gap = 20, 40
    w = pad + 640 + gap + 360 + pad
    head = (44 if headings else 0) + (38 if label else 0)
    stats = []
    probe = Image.new("RGBA", (w * S, (forge_h + 700) * S), BG + (255,))
    y0 = head + 6
    note, ok, _ = draw_forge(probe, pad, y0, case, forge_h, stats)
    th = draw_tooltip(probe, pad + 640 + gap, y0, case, stats)
    height = int(max(y0 + forge_h + 34, y0 + th + 20))
    canvas = probe.crop((0, 0, w * S, height * S))
    d = ImageDraw.Draw(canvas)
    if label:
        d.text((pad * S, 4 * S), label + (" (stand-in font)" if STAND_IN else ""), font=fnt(24, True), fill=GOLD, anchor="la")
    if headings:
        hy = head - 44 + (38 if label else 0)
        d.text(((pad + 320) * S, (hy + 4) * S), "Riftforge", font=fnt(28, True), fill=GOLD, anchor="ma")
        d.text(((pad + 640 + gap + 180) * S, (hy + 4) * S), "Inventory tooltip", font=fnt(28, True), fill=GOLD, anchor="ma")
    d.text((pad * S, (y0 + forge_h + 8) * S), note, font=fnt(14), fill=DIM, anchor="la")
    nw = d.textlength(note + "   ", font=fnt(14)) / S
    d.text(((pad + nw) * S, (y0 + forge_h + 8) * S), "fits" if ok else "CLIPS", font=fnt(14, True),
           fill=DIM if ok else RED, anchor="la")
    return canvas, stats, ok


def report(case, stats):
    for name, base, n, used, avail in stats:
        print("  %-10s %-11s base %5.2f  lines %2d  height %6.1f / %6.1f%s" % (
            case["id"], name, base, n, used, avail, "" if used <= avail + 0.01 else "  OVER"))


def stack(images, name):
    w = max(i.size[0] for i in images)
    h = sum(i.size[1] for i in images)
    canvas = Image.new("RGBA", (w, h), BG + (255,))
    y = 0
    for img in images:
        canvas.alpha_composite(img, (0, y))
        y += img.size[1]
    out = canvas
    if out.size[0] > 2400:
        out = out.resize((2400, int(out.size[1] * 2400 / out.size[0])), Image.LANCZOS)
    path = os.path.join(OUT, name)
    out.convert("RGB").save(path)
    print("wrote", path, out.size)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", default=DEFAULT_JSON)
    ap.add_argument("--font-dir", default=DEFAULT_FONTS)
    ap.add_argument("--forge-h", type=int, default=540)
    a = ap.parse_args()
    setup_fonts(a.font_dir)
    data = json.load(open(a.json, encoding="utf-8"))
    cases = {c["id"]: c for c in data["cases"]}
    print("rendering", a.json)

    case = cases["tier5"]
    img, stats, _ = draw_case(case, a.forge_h, True)
    report(case, stats)
    stack([img], "map_ui_tier5.png")

    rows = []
    top = Image.new("RGBA", (img.size[0], 50 * S), BG + (255,))
    d = ImageDraw.Draw(top)
    d.text(((20 + 320) * S, 8 * S), "Riftforge", font=fnt(28, True), fill=GOLD, anchor="ma")
    d.text(((20 + 640 + 40 + 180) * S, 8 * S), "Inventory tooltip", font=fnt(28, True), fill=GOLD, anchor="ma")
    rows.append(top)
    for cid in ("tier1", "tier3", "tier6"):
        c = cases[cid]
        img, stats, _ = draw_case(c, a.forge_h, False, c["label"])
        report(c, stats)
        rows.append(img)
    stack(rows, "map_ui_tiers.png")


if __name__ == "__main__":
    main()
