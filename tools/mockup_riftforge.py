"""
Mock-ups of the Riftforge window (U3 in docs/ROADMAP.md), in the agreed classes-UI look, from the real art: the
CraftingOverhaul sheet art (panel, inset, bronze tiles, glows), the orb renders (docs/images/orbs_preview.png) and the shard
render. Two placements: its own window (orb and shard states) and a tab in the game's crafting panel. Mock-ups only: once
the user picks, the window gets a tested layout class and a preview drawn from it, as the skills panel has.

    python tools/mockup_riftforge.py      (writes into docs/images/)

Item names, lines and counts are samples; the operations follow docs/corruption-design.md (currency orbs, corrupted
gear, Affix Splinters and shattering). The item icon is a placeholder skill emblem.
"""
import os
import sys

from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import preview_skills_panel as P  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "docs", "images")
S = P.S
GOLD, LIGHT, DIM, RUNE, GREEN, RED = P.GOLD, P.LIGHT, P.LIGHT_DIM, P.RUNE, P.CAN_SPEND, P.RED
MAGIC = (136, 136, 255)
RARE = (255, 255, 119)
CORRUPT = (220, 60, 60)

ORB_IDS = ["transmutation", "augmentation", "alchemy", "regal", "exalted", "divine", "annulment", "chance", "jeweller"]
ORB_NAMES = {"transmutation": "Orb of Transmutation", "augmentation": "Orb of Augmentation", "alchemy": "Orb of Alchemy",
             "regal": "Regal Orb", "exalted": "Exalted Orb", "divine": "Divine Orb", "annulment": "Orb of Annulment",
             "chance": "Orb of Chance", "jeweller": "Jeweller's Orb"}


def orb_icons():
    sheet = Image.open(os.path.join(ROOT, "docs/images/orbs_preview.png")).convert("RGBA")
    icons = {}
    for i, oid in enumerate(ORB_IDS):
        x, y = (i % 3) * 300, (i // 3) * 300
        cell = sheet.crop((x + 10, y + 10, x + 290, y + 290))
        # Background to transparent (the render's dark slate).
        px = cell.load()
        for yy in range(cell.size[1]):
            for xx in range(cell.size[0]):
                r, g, b, a = px[xx, yy]
                if abs(r - 34) < 10 and abs(g - 36) < 10 and abs(b - 40) < 10:
                    px[xx, yy] = (0, 0, 0, 0)
        icons[oid] = cell
    shard = Image.open(os.path.join(ROOT, "docs/images/shard_preview.png")).convert("RGBA").crop((70, 40, 290, 330))
    px = shard.load()
    for yy in range(shard.size[1]):
        for xx in range(shard.size[0]):
            r, g, b, a = px[xx, yy]
            if abs(r - 34) < 10 and abs(g - 36) < 10 and abs(b - 40) < 10:
                px[xx, yy] = (0, 0, 0, 0)
    icons["shard"] = shard
    return icons


def item_icon():
    return P.art("Impale", P.ICONS)  # stand-in for a bronze spear's icon


def slot(canvas, x, y, size, icon=None, glow=None, label=None, count=None, dim=False):
    if glow:
        g = P.art("glow_box")
        big = P.sliced(g, size + 28, size + 28, 24)
        canvas.alpha_composite(P.tinted(big, glow, 0.9), (int((x - 14) * S), int((y - 14) * S)))
    tile = P.sliced(P.art("tile"), size, size, 18)
    if dim:
        tile = P.tinted(tile, (150, 150, 150))
    P.paste(canvas, tile, x, y)
    if icon is not None:
        inset = size * 0.12
        ic = P.sized(icon, size - 2 * inset, size - 2 * inset)
        if dim:
            ic = P.tinted(ic, (110, 110, 110), 0.85)
        P.paste(canvas, ic, x + inset, y + inset)
    d = ImageDraw.Draw(canvas)
    if count is not None:
        P.badge(canvas, x + size - 7, y + size - 7, count, LIGHT if not dim else DIM)
    if label:
        P.text(d, (x + size / 2, y + size + 4), label, 13, DIM if dim else LIGHT, "ma")


def chance_bar(canvas, x, y, w, h, parts):
    d = ImageDraw.Draw(canvas)
    cx = x
    for frac, colour, label in parts:
        pw = w * frac
        d.rectangle([cx * S, y * S, (cx + pw - 2) * S, (y + h) * S], fill=colour + (255,))
        P.text(d, (cx + pw / 2 - 1, y + h / 2), label, 13, (255, 255, 255), "mm")
        cx += pw


def diamond(canvas, cx, cy, r, colour):
    d = ImageDraw.Draw(canvas)
    pts = [(cx, cy - r), (cx + r * 0.7, cy), (cx, cy + r), (cx - r * 0.7, cy)]
    d.polygon([(px * S, py * S) for px, py in pts], fill=colour + (255,), outline=(20, 14, 10, 255))
    d.line([(cx * S, (cy - r) * S), (cx * S, (cy + r) * S)], fill=(255, 255, 255, 120), width=S)


def shatter_icon(size=96):
    img = Image.new("RGBA", (size * S, size * S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = size * S / 2
    r = size * S * 0.32
    d.polygon([(c, c - r), (c + r * 0.75, c), (c, c + r), (c - r * 0.75, c)], fill=(200, 205, 215, 255), outline=(30, 30, 30, 255))
    pts = [(c - r * 0.1, c - r), (c + r * 0.12, c - r * 0.35), (c - r * 0.15, c + r * 0.05), (c + r * 0.1, c + r * 0.5), (c - r * 0.05, c + r)]
    d.line(pts, fill=(25, 20, 18, 255), width=int(2.5 * S))
    for dx, dy in ((-0.95, -0.6), (0.9, -0.5), (1.0, 0.55), (-0.9, 0.6)):
        d.line([(c + dx * r * 0.85, c + dy * r * 0.85), (c + dx * r * 1.15, c + dy * r * 1.15)], fill=(220, 225, 235, 255), width=int(2 * S))
    return img.resize((size, size), Image.LANCZOS)


def splinter_icon(colour, size=96):
    img = Image.new("RGBA", (size * S, size * S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c, r = size * S / 2, size * S * 0.3
    d.polygon([(c, c - r), (c + r * 0.55, c), (c, c + r), (c - r * 0.55, c)], fill=colour + (255,), outline=(20, 14, 10, 255))
    d.line([(c, c - r), (c, c + r)], fill=(255, 255, 255, 150), width=int(1.5 * S))
    return img.resize((size, size), Image.LANCZOS)


def card_item(canvas, x, y, w, name, colour, base, lines, size=96):
    slot(canvas, x, y, size, item_icon(), glow=RUNE)
    d = ImageDraw.Draw(canvas)
    tx = x + size + 14
    fs = 20
    while fs > 13 and d.textlength(name, font=P.font(fs)) > (w - size - 14) * S:
        fs -= 1
    P.text(d, (tx, y + 8), name, fs, colour, "la")
    P.text(d, (tx, y + 36), base, 14, DIM, "la")
    P.text(d, (tx, y + 56), "Slash 32  ·  Pierce 40", 14, LIGHT, "la")
    yy = y + size + 14
    for line, c in lines:
        P.text(d, (x, yy), line, 15, c, "la")
        yy += 21


def arrow(canvas, x, y):
    d = ImageDraw.Draw(canvas)
    d.polygon([((x) * S, (y - 12) * S), ((x + 20) * S, y * S), (x * S, (y + 12) * S)], fill=GOLD + (230,))
    d.line([((x - 22) * S, y * S), ((x + 2) * S, y * S)], fill=GOLD + (230,), width=4 * S)


TILES = [("transmutation", 14), ("augmentation", 9), ("alchemy", 3), ("regal", 2), ("exalted", 1), ("divine", 0), ("annulment", 4),
         ("chance", 6), ("jeweller", 2), ("shard", 23), ("splinter_speed", 1), ("splinter_health", 1), ("splinter_fire", 1), ("shatter", None)]


def currency(canvas, r, icons, selected, unusable, per_row=7, size=54):
    x, y, w, h = r
    P.paste(canvas, P.sliced(P.art("inset"), w, h, 10), x, y)
    d = ImageDraw.Draw(canvas)
    P.text(d, (x + 16, y + 12), "Use on the item", 18, GOLD, "la")
    P.text(d, (x + w - 16, y + 15), "orbs, shards and splinters from your Currency tab", 13, DIM, "ra")
    gap_x = (w - 32 - per_row * size) / (per_row - 1)
    for i, (tid, count) in enumerate(TILES):
        cx = x + 16 + (i % per_row) * (size + gap_x)
        cy = y + 44 + (i // per_row) * (size + 22)
        icon = icons.get(tid)
        dim = tid in unusable or count == 0
        glow = (255, 240, 200) if tid == selected else None
        slot(canvas, cx, cy, size, icon, glow=glow, count=count, dim=dim)


def result_orb(canvas, r):
    x, y, w, h = r
    d = ImageDraw.Draw(canvas)
    P.text(d, (x, y), "Result", 18, GOLD, "la")
    P.text(d, (x, y + 28), "Rare Bronze Spear", 18, RARE, "la")
    P.text(d, (x, y + 54), "+24% damage (+8)  [5–40]", 15, LIGHT, "la")
    P.text(d, (x, y + 75), "+12% attack speed  [5–15]", 15, LIGHT, "la")
    P.text(d, (x, y + 96), "+ one new random affix", 15, GREEN, "la")
    P.text(d, (x, y + 124), "Gets a new Rare name", 14, DIM, "la")


def result_shard(canvas, r):
    x, y, w, h = r
    d = ImageDraw.Draw(canvas)
    P.text(d, (x, y), "Result: one of three", 18, GOLD, "la")
    chance_bar(canvas, x, y + 30, w, 26, [(0.25, (150, 30, 30), "25%"), (0.25, (90, 84, 78), "25%"), (0.5, (120, 40, 140), "50%")])
    d = ImageDraw.Draw(canvas)
    P.text(d, (x, y + 68), "■ Destroyed", 15, (235, 95, 85), "la")
    P.text(d, (x, y + 90), "■ Unchanged, but locked", 15, (195, 186, 176), "la")
    P.text(d, (x, y + 112), "■ +1 corrupted affix, past the limit", 15, (205, 130, 225), "la")
    P.text(d, (x, y + 136), "Locked afterwards either way", 14, DIM, "la")


ITEM_MAGIC = ("Cruel Bronze Spear of Haste", MAGIC, "Magic Bronze Spear",
              [("+24% damage (+8)  [5–40]", LIGHT), ("+12% attack speed  [5–15]", LIGHT)])
ITEM_RARE = ("Grim Fang", RARE, "Rare Bronze Spear",
             [("+24% damage (+8)  [5–40]", LIGHT), ("+12% attack speed  [5–15]", LIGHT), ("+15 max health  [5–30]", LIGHT),
              ("12% less fire damage  [10–50]", LIGHT)])


def all_icons():
    icons = orb_icons()
    icons["splinter_speed"] = splinter_icon(LIGHT)
    icons["splinter_health"] = splinter_icon(LIGHT)
    icons["splinter_fire"] = splinter_icon(RARE)
    icons["shatter"] = shatter_icon()
    return icons


def window(kind):
    W, H = 1000, 600
    icons = all_icons()
    canvas = Image.new("RGBA", (int((W + 80) * S), int((H + 80) * S)), (56, 74, 52, 255))
    o = 40
    P.paste(canvas, P.sliced(P.art("panel"), W, H, 20), o, o)
    d = ImageDraw.Draw(canvas)
    P.text(d, (o + W / 2, o + 32), "RIFTFORGE", 30, (240, 200, 110), "mm", title=True)
    P.paste(canvas, P.sized(P.art("close"), 36, 36), o + W - 60, o + 14)

    # The bench: item -> reagent -> result.
    bx, by, bw, bh = o + 24, o + 64, W - 48, 250
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), bx, by)
    item = ITEM_MAGIC if kind == "orb" else ITEM_RARE
    card_item(canvas, bx + 20, by + 20, 330, *item)
    arrow(canvas, bx + 372, by + 68)
    reagent = "regal" if kind == "orb" else "shard"
    slot(canvas, bx + 404, by + 20, 96, icons[reagent], glow=(255, 240, 200))
    d = ImageDraw.Draw(canvas)
    P.text(d, (bx + 452, by + 126), "Regal Orb" if kind == "orb" else "Corruption shard", 16, GOLD, "ma")
    P.text(d, (bx + 452, by + 148), "2 left" if kind == "orb" else "23 left", 14, DIM, "ma")
    arrow(canvas, bx + 534, by + 68)
    rr = (bx + 572, by + 22, bw - 572 - 20, bh - 40)
    (result_orb if kind == "orb" else result_shard)(canvas, rr)

    # Currency grid and the action.
    unusable = {"transmutation", "alchemy", "chance", "splinter_speed", "splinter_health", "splinter_fire"} if kind == "orb" else \
        {"transmutation", "augmentation", "alchemy", "regal", "chance", "splinter_speed", "splinter_health", "splinter_fire"}
    currency(canvas, (o + 24, o + 328, 600, H - 328 - 24), icons, reagent, unusable)
    ax, ay, aw, ah = o + 640, o + 328, W - 640 - 24, H - 328 - 24
    P.paste(canvas, P.sliced(P.art("inset"), aw, ah, 10), ax, ay)
    d = ImageDraw.Draw(canvas)
    if kind == "orb":
        desc = "Turns a Magic item into a Rare and adds one random affix. Orbs never destroy an item."
        P.text(d, (ax + 16, ay + 12), "Regal Orb", 18, GOLD, "la")
        for i, ln in enumerate(P.wrap(d, desc, 15, aw - 32)):
            P.text(d, (ax + 16, ay + 40 + i * 20), ln, 15, LIGHT, "la")
        P.button(canvas, (ax + 16, ay + ah - 58, aw - 32, 42), "Apply Regal Orb")
    else:
        desc = "Corruption is the last step: no orb can change the item afterwards."
        P.text(d, (ax + 16, ay + 12), "Corrupt", 18, CORRUPT, "la")
        for i, ln in enumerate(P.wrap(d, desc, 15, aw - 32)):
            P.text(d, (ax + 16, ay + 40 + i * 20), ln, 15, LIGHT, "la")
        P.button(canvas, (ax + 16, ay + ah - 58, aw - 32, 42), "Hold to corrupt  (25% destroys it)")
    canvas = canvas.resize((canvas.size[0] // S, canvas.size[1] // S), Image.LANCZOS)
    path = os.path.join(OUT, "riftforge_window_%s.png" % kind)
    canvas.convert("RGB").save(path)
    print(path)




def compact(kind="orb"):
    """Placement B: a third tab in the game's crafting panel at the Riftforge, beside the inventory."""
    W, H = 600, 640
    icons = all_icons()
    canvas = Image.new("RGBA", (int((W + 80) * S), int((H + 80) * S)), (56, 74, 52, 255))
    o = 40
    P.paste(canvas, P.sliced(P.art("panel"), W, H, 20), o, o)
    d = ImageDraw.Draw(canvas)
    P.text(d, (o + W / 2, o + 30), "RIFTFORGE", 28, (240, 200, 110), "mm", title=True)
    for i, name in enumerate(["Craft", "Upgrade", "Corrupt and craft"]):
        P.button(canvas, (o + 24 + i * 186, o + 58, 178, 32), name, enabled=i == 2)
    bx, by, bw, bh = o + 24, o + 100, W - 48, 232
    P.paste(canvas, P.sliced(P.art("inset"), bw, bh, 10), bx, by)
    item = ITEM_MAGIC
    slot(canvas, bx + 16, by + 16, 72, item_icon(), glow=RUNE)
    arrow(canvas, bx + 116, by + 52)
    slot(canvas, bx + 144, by + 16, 72, icons["regal"], glow=(255, 240, 200))
    d = ImageDraw.Draw(canvas)
    P.text(d, (bx + 240, by + 18), item[0], 16, item[1], "la")
    P.text(d, (bx + 240, by + 40), "+ Regal Orb (2 left)", 15, GOLD, "la")
    P.text(d, (bx + 16, by + 108), "Result", 16, GOLD, "la")
    P.text(d, (bx + 16, by + 132), "Rare Bronze Spear", 16, RARE, "la")
    P.text(d, (bx + 16, by + 156), "+24% damage (+8)  [5–40]", 15, LIGHT, "la")
    P.text(d, (bx + 16, by + 177), "+12% attack speed  [5–15]", 15, LIGHT, "la")
    P.text(d, (bx + 16, by + 198), "+ one new random affix", 15, GREEN, "la")
    currency(canvas, (o + 24, o + 346, W - 48, 196), icons, "regal",
             {"transmutation", "alchemy", "chance", "splinter_speed", "splinter_health", "splinter_fire"})
    P.button(canvas, (o + 24, o + H - 80, W - 48, 44), "Apply Regal Orb")
    canvas = canvas.resize((canvas.size[0] // S, canvas.size[1] // S), Image.LANCZOS)
    path = os.path.join(OUT, "riftforge_tab.png")
    canvas.convert("RGB").save(path)
    print(path)


if __name__ == "__main__":
    window("orb")
    window("shard")
    compact()
